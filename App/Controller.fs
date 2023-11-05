module App.Controller

open System
open Microsoft.AspNetCore.Builder


type CallLiftDto = { floor: int }

let make (app: WebApplication) (svc: Lift.Service) =

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
