module Program

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting

let builder = WebApplication.CreateBuilder()

let app = builder.Build()

app.MapGet("/healthz", Func<_>(fun () -> "OK")) |> ignore

app.Run()
