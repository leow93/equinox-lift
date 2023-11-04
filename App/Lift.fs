module Lift

module Events =
  type FloorDetails = { floor: int }

  type Event =
    | LiftRequested of FloorDetails
    | FloorVisited of FloorDetails

    interface TypeShape.UnionContract.IUnionContract

  let codec = FsCodec.SystemTextJson.Codec.Create<Event>()

open Events

module Fold =
  open Events

  type State = { floor: int; queue: int list }
  let initial = { floor = 0; queue = [] }

  let evolve state event =
    match event with
    | LiftRequested x ->
      { state with
          queue = state.queue @ [ x.floor ] }
    | FloorVisited x ->
      { floor = x.floor
        queue = state.queue |> List.filter ((<>) x.floor) }


  let fold: State -> Event seq -> State = Seq.fold evolve

module Decisions =
  let requestLift (cmd: FloorDetails) (state: Fold.State) =
    if state.queue |> List.contains cmd.floor then
      []
    else
      [ LiftRequested cmd ]

  let visitFloor (cmd: FloorDetails) (state: Fold.State) =
    if state.queue |> List.contains cmd.floor then
      [ FloorVisited cmd ]
    else
      []

open FSharp.UMX
open System

type LiftId = Guid<liftId>
and [<Measure>] liftId

module LiftId =
  let inline ofGuid (g: Guid) : LiftId = %g
  let inline parse (id: string) = Guid.Parse id |> ofGuid
  
  let inline tryParse (id: string) =
    match Guid.TryParse id with
    | true, x -> Some (ofGuid x)
    | _ -> None
  let inline toGuid (id: LiftId) : Guid = %id
  let inline toString (id: LiftId) = (toGuid id).ToString()

[<Literal>]
let Category = "Lift"

let streamId = Equinox.StreamId.gen LiftId.toString

module Queries =
  let summary = id 
   
type Service internal (resolve: LiftId -> Equinox.Decider<Events.Event, Fold.State>) =
  member _.Call(id, cmd) =
    let decider = resolve id
    decider.Transact(Decisions.requestLift cmd)

  member _.VisitFloor(id, cmd) =
    let decider = resolve id
    decider.Transact(Decisions.visitFloor cmd)
  
  member _.Get(id) =
    let decider = resolve id
    decider.Query(Queries.summary)

let create resolve = Service(streamId >> resolve Category)
