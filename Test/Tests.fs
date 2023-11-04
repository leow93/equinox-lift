module Tests

open System
open Xunit
open Lift

let given events interpret =
  Fold.fold Fold.initial events |> interpret

open Lift.Events

[<Fact>]
let ``Calling the lift`` () =
  let data = { floor = 3 }
  let events = given [] (Decisions.requestLift data)
  Assert.StrictEqual(events, [ LiftRequested data ])


[<Fact>]
let ``Calling the lift is idempotent`` () =
  let data = { floor = 3 }
  let events = given [ LiftRequested data ] (Decisions.requestLift data)
  Assert.StrictEqual(events, [])


[<Fact>]
let ``Visiting the requested floor`` () =
  let data = { floor = 3 }
  let events = given [ LiftRequested data ] (Decisions.visitFloor data)
  Assert.StrictEqual(events, [ FloorVisited data ])

[<Fact>]
let ``Visiting the requested floor is idempotent`` () =
  let data = { floor = 3 }

  let events =
    given [ LiftRequested data; FloorVisited data ] (Decisions.visitFloor data)

  Assert.StrictEqual(events, [])


[<Fact>]
let ``Attempting to visit an unrequested floor does nothing`` () =
  let data = { floor = 3 }
  let events = given [ LiftRequested data ] (Decisions.visitFloor { floor = 4 })
  Assert.StrictEqual(events, [])
