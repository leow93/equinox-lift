module Program

open System
open Equinox
open Equinox.MessageDb
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Hosting
open Serilog

let tryGetEnv = Environment.GetEnvironmentVariable >> Option.ofObj
let log = LoggerConfiguration().WriteTo.Console().CreateLogger()

let cache = Cache("cache", sizeMb = 50)

let defaultConnString =
  "Host = localhost; Database=message_store; Username=message_store"

let DBURL = "MESSSAGE_DB_URL" |> tryGetEnv |> Option.defaultValue defaultConnString

let connection = MessageDbConnector(DBURL, DBURL).Establish()
let context = MessageDbContext(connection)
let caching = CachingStrategy.SlidingWindow(cache, TimeSpan.FromMinutes(5))

let svc =
  MessageDbCategory(context, Lift.Events.codec, Lift.Fold.fold, Lift.Fold.initial, caching)
  |> Decider.resolve log
  |> Lift.create

let builder = WebApplication.CreateBuilder()

let app = builder.Build()

type CallLiftDto = { floor: int }

app.MapGet("/healthz", Func<_>(fun () -> "OK")) |> ignore

app.MapPost(
  "/{id}/call",
  Func<string, CallLiftDto, _>(fun id body ->
    task {
      let id = Lift.LiftId.parse id
      do! svc.Call(id, { floor = body.floor })
    })
)
|> ignore


app.MapPost(
  "/{id}/visit",
  Func<string, CallLiftDto, _>(fun id body ->
    task {
      let id = Lift.LiftId.parse id
      do! svc.VisitFloor(id, { floor = body.floor })
    })
)
|> ignore

app.MapGet(
  "/{id}",
  Func<string, _>(fun id ->
    task {
      let id = Lift.LiftId.parse id
      return! svc.Get(id)
    })
)
|> ignore

app.Run()
