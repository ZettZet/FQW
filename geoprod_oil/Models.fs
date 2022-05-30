module Models

open System


type Well =
    { id: string option // well
      name: string option //well_num
      weights: Map<string, float> }


type Region = { id: string; wells: Well list }


type GeoInput = { regions: Region list }


type OilWell =
    { id: int
      well: string
      well_num: string
      dt: DateTime
      oil: float
      oil_cum: float
      oilm3: float
      oilm3_cum: float }

type Mode =
    | CubeMeter = 0
    | Tonne = 1


type OilWellCalculated =
    { region: string
      id: string
      name: string
      result: float
      result_uw: float }

type OilWellValue =
    { well: string
      well_num: string
      oil: float
      dt: DateTime }

    static member read (mode: Mode) (row: RowReader) =
        { well = row.string "well"
          well_num = row.string "well_num"
          dt = row.dateTime "dt"
          oil =
            row.double (
                if mode = Mode.CubeMeter then
                    "oilm3"
                else
                    "oil"
            ) }

[<CLIMutable>]
type GeoQueryString =
    { from_: DateTime
      to_: DateTime
      mode: Mode }
