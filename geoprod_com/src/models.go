package main

type GeoInput struct {
	Regions []struct {
		Id    string `json:"id"`
		Wells []struct {
			Id      string             `json:"id"`
			Name    string             `json:"name"`
			Weights map[string]float32 `json:"weights"`
		} `json:"wells"`
	} `json:"regions"`
}

type GeoQueryString struct {
	From_ string `form:"from_" binding:"required"`
	To_   string `form:"to_" binding:"required"`
	Mode  string `from:"mode" binding:"required"`
}

type GeoResponse struct {
	Region   string  `json:"region"`
	Id       string  `json:"id"`
	Name     string  `json:"name"`
	Result   float32 `json:"result"`
	ResultUw float32 `json:"result_uw"`
}
