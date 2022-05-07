module Models

open System.Collections.Generic
open Microsoft.Data.Sqlite

type Well = {
    id: string option
    name: string option
    weights: Dictionary<string, float>
}

type WellInfo = {
    well: string
    wellNum: string
    onDate: string
    oil: float
}