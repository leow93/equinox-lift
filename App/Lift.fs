module Lift

module Events =
  type LiftRequested = { floor: int }
  type LiftMoved = { fromFloor: int; toFloor: int }

  type Event = LiftRequested of LiftRequested

module Fold =
  open Events

  type State = { floor: int; queue: int list }
  let initial = { floor = 0; queue = [] }

  let evolve state event =
    match event with
    | LiftRequested cmd ->
      { state with
          queue = cmd.floor :: state.queue }


  let fold: State -> Event seq -> State = Seq.fold evolve

module Decisions =
  let requestLift cmd (state: Fold.State) = []

