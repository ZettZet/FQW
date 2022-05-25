module DbConnection

open System


let private userName = Environment.GetEnvironmentVariable "POSTGRES_USER"
let private userPassword = Environment.GetEnvironmentVariable "POSTGRES_PASSWORD"
let private host = Environment.GetEnvironmentVariable "POSTGRES_HOST"
let private port = Environment.GetEnvironmentVariable "POSTGRES_PORT"
let private dbName = Environment.GetEnvironmentVariable "POSTGRES_DB"


let connectionString =
    $"User ID={userName};Password={userPassword};Host={host};Port={port};Database={dbName};"
// $"postgresql://{userName}:{userPassword}@{host}:{port}/{dbName}"
