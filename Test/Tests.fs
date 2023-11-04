module Tests

open System
open Xunit
open Lift

let given events interpret =
  Fold.fold Fold.initial events |> interpret

open Lift.Events

[<Fact>]
let ``Calling the lift`` () =
  let events = given [] (Decisions.requestLift 3)
  Assert.StrictEqual(events, [ LiftRequested 3 ])


[<Fact>]
let ``Calling the lift is idempotent`` () =
  let events = given [ LiftRequested 3 ] (Decisions.requestLift 3)
  Assert.StrictEqual(events, [])


[<Fact>]
let ``Visiting the requested floor`` () =
  let events = given [ LiftRequested 3 ] (Decisions.visitFloor 3)
  Assert.StrictEqual(events, [ FloorVisited 3 ])

[<Fact>]
let ``Visiting the requested floor is idempotent`` () =
  let events = given [ LiftRequested 3; FloorVisited 3 ] (Decisions.visitFloor 3)
  Assert.StrictEqual(events, [])


[<Fact>]
let ``Attempting to visit an unrequested floor does nothing`` () =
  let events = given [ LiftRequested 3 ] (Decisions.visitFloor 4)
  Assert.StrictEqual(events, [])
