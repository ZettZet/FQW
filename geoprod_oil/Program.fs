open System
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Dapper.FSharp
open Microsoft.Data.Sqlite
open Models


let connectionString = "Data Source = MASTER; Mode = Memory; Cache = Shared;"
let connetion = new SqliteConnection (connectionString)
let wellInfoTable = table<WellInfo>



type Response = {
    message: string
}

let webApp =
    choose [ route "/" >=> text "Hello World!" ]
             
   
let configureApp (app : IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0