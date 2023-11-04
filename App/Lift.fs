module Lift

module Events =
  type Floor = int

  type Event =
    | LiftRequested of Floor
    | FloorVisited of Floor

open Events

module Fold =
  open Events

  type State = { floor: int; queue: int list }
  let initial = { floor = 0; queue = [] }

  let evolve state event =
    match event with
    | LiftRequested floor ->
      { state with
          queue = state.queue @ [ floor ] }
    | FloorVisited floor ->
      { floor = floor
        queue = state.queue |> List.filter ((<>) floor) }


  let fold: State -> Event seq -> State = Seq.fold evolve

module Decisions =
  let requestLift (floor: Floor) (state: Fold.State) =
    if state.queue |> List.contains floor then
      []
    else
      [ LiftRequested floor ]

  let visitFloor (floor: Floor) (state: Fold.State) =
    if state.queue |> List.contains floor then
      [ FloorVisited floor ]
    else
      []
