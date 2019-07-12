module ElmishProgram
open Cmd
open RingBuffer

type Program<'model, 'msg, 'view> =
        {
          init: unit ->'model * Cmd<'msg>
          update: 'msg -> 'model -> ('model * Cmd<'msg>)
          view: 'model -> Dispatch<'msg> -> 'view
          setState: 'model -> 'msg -> Dispatch<'msg> -> unit
         }

let runWith<'arg, 'model, 'msg, 'view> (program: Program<'model, 'msg, 'view>) =
    let (initModel, initCmd) = program.init()
    let mutable state = initModel
    let mutable reentered = false
    let buffer = RingBuffer 10

    let rec dispatch msg =
        let mutable nextMsg = Some msg;
        if reentered
         then buffer.Push msg
         else
             while Option.isSome nextMsg do
                 reentered <- true
                 let (model, cmd) = program.update nextMsg.Value state
                 program.setState model nextMsg.Value dispatch
                 Cmd.exec dispatch cmd |> ignore
                 state <- model;
                 nextMsg <- buffer.Pop()
                 reentered <- false;

    Cmd.exec dispatch initCmd |> ignore
    state

let run program = runWith program

