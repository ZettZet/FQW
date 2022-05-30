open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open System.Text.Json
open System.Text.Json.Serialization
open Giraffe
open Npgsql.FSharp
open Npgsql

open Models
open DbConnection

let wellsSatisfy from_ to_ mode id name =
    task {
        use conn = new NpgsqlConnection(connectionString)

        let result =
            Sql.existingConnection conn
            |> Sql.query """select * from "OilWell" where well=@well and well_num=@well_num and dt between @from_ and @to_"""
            |> Sql.parameters [ "well", Sql.string id
                                "well_num", Sql.string name
                                "from_", Sql.timestamp from_
                                "to_", Sql.timestamp to_ ]
            |> Sql.executeAsync (OilWellValue.read mode)

        return! result
    }

let logic (query: GeoQueryString) (regions: GeoInput) =
    task {
        let mutable wellsWithResult = []
        let wellsSatisfy = wellsSatisfy query.from_ query.to_ query.mode

        for region in regions.regions do
            for well in region.wells do
                if well.id.IsSome && well.name.IsSome then
                    let well_id = well.id.Value.Trim('"')
                    let well_name = well.name.Value.Trim('"')

                    let! satisfied = wellsSatisfy well_id well_name

                    let mutable result = 0.0
                    let mutable result_unweighted = 0.0

                    for ws in satisfied do
                        let weight =
                            if well.weights.Count > 0 then
                                well.weights
                                |> Seq.minBy (fun t -> DateTime.Parse t.Key - DateTime.Today)
                                |> (fun t -> t.Value)
                            else
                                1.0

                        result <- result + weight * ws.oil
                        result_unweighted <- result_unweighted + ws.oil

                    wellsWithResult <-
                        { region = region.id
                          id = well_id
                          name = well_name
                          result = result
                          result_uw = result_unweighted }
                        :: wellsWithResult


        return wellsWithResult
    }

let calculateProduction =
    handleContext (fun ctx ->
        task {
            let geoParams = ctx.BindQueryString<GeoQueryString>()
            let! jsonInput = ctx.BindJsonAsync<GeoInput>()
            let! result = logic geoParams jsonInput
            return! ctx.WriteJsonAsync(result)
        })

let webApp = choose [ route "/" >=> calculateProduction ]


let configureApp (app: IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    // Add System.Text.Json and JsonFSharpConverter as JSON (de)serializer
    let jsonOptions = JsonSerializerOptions()
    jsonOptions.Converters.Add(JsonFSharpConverter())
    services.AddSingleton(jsonOptions) |> ignore

    services.AddSingleton<Json.ISerializer, SystemTextJson.Serializer>()
    |> ignore

    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")

    clearResponse
    >=> ServerErrors.INTERNAL_ERROR ex.Message

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .Configure(configureApp)
                .ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
