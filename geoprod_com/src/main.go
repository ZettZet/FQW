package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"github.com/akrennmair/slice"
	"github.com/gin-gonic/gin"
	"golang.org/x/exp/constraints"
	"golang.org/x/exp/maps"
	"io"
	"io/ioutil"
	"log"
	"net/http"
)

func groupBy[T any, K constraints.Ordered, V any](in []T, keyChooser func(T) K, valueChooser func(T) V) map[K][]V {
	var result map[K][]V = make(map[K][]V)
	for _, value := range in {
		key := keyChooser(value)
		newValue := valueChooser(value)
		_, ok := result[key]
		if ok {
			result[key] = append(result[key], newValue)
		} else {
			result[key] = []V{newValue}
		}
	}
	return result
}

func main() {
	r := gin.Default()
	r.POST("/", compute)
	err := r.Run(":8000")
	if err != nil {
		return
	} // listen and serve on 0.0.0.0:8080 (for windows "localhost:8080")
}

func compute(c *gin.Context) {
	var gi GeoInput
	var gqs GeoQueryString
	c.BindQuery(&gqs)
	c.BindJSON(&gi)

	zak, errZ := makeGeoRequest(gi, gqs, "zak")
	liq, errL := makeGeoRequest(gi, gqs, "liq")
	if errZ != nil {
		log.Print(errZ)
		return
	}
	if errL != nil {
		log.Print(errL)
		return
	}

	/*
		[
		   	{
		   	"region",
		       "id",
		       "name",
		       "result",
		       "result_uw",
		   	}
		]
		group by "region":["result"] . to  dict "key" sum{["result"]}

	*/

	zakSumated := sumByRegion(zak)
	liqSumated := sumByRegion(liq)

	var result = make(map[string]float32)
	for _, v := range maps.Keys(zakSumated) {
		result[v] = zakSumated[v] / liqSumated[v]
	}
	c.JSON(http.StatusOK, result)
	/*
		zak = [region : sum]
		liq = [region : sum]
		region : zak / liq
	*/

}

func sumByRegion(well []GeoResponse) map[string]float32 {
	var wellOrdered = groupBy[GeoResponse, string, float32](well,
		func(item GeoResponse) string {
			return item.Region
		}, func(item GeoResponse) float32 {
			return item.Result
		},
	)
	var wellSum = make(map[string]float32)
	for k, v := range wellOrdered {
		wellSum[k] = slice.Reduce(v, func(t2 float32, t1 float32) float32 {
			return t2 + t1
		})
	}
	return wellSum
}

func makeGeoRequest(gi GeoInput, gqs GeoQueryString, url string) ([]GeoResponse, error) {
	body, _ := json.Marshal(gi)
	req, err := http.NewRequest("POST", fmt.Sprintf("http://%s:8000/", url), bytes.NewBuffer(body))
	if err != nil {
		log.Print(err)
		return nil, err
	}
	q := req.URL.Query()
	q.Add("from_", gqs.From_)
	q.Add("to_", gqs.To_)
	q.Add("mode", "0")
	req.URL.RawQuery = q.Encode()

	client := &http.Client{}
	resp, _ := client.Do(req)

	defer func(Body io.ReadCloser) {
		err := Body.Close()
		if err != nil {
			log.Print(err)
			return
		}
	}(resp.Body)
	result, err := ioutil.ReadAll(resp.Body)
	var gr []GeoResponse
	err = json.Unmarshal(result, &gr)
	if err != nil {
		return nil, err
	}
	return gr, nil
}
