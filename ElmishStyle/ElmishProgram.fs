module ElmishProgram
open Cmd
open RingBuffer

type Program<'model, 'msg, 'view> =
        {
          init: unit ->'model * Cmd<'msg>
          update: 'msg -> 'model -> ('model * Cmd<'msg>)
          view: 'model -> Dispatch<'msg> -> 'view
         }

   let runWith<'arg, 'model, 'msg, 'view> (arg: 'arg) (program: Program<'model, 'msg, 'view>) =
        let (initModel, initCmd) = program.init()
        let mutable state = initModel
        let mutable reentered = false
        let buffer = RingBuffer<'msg> 10

        let rec dispatch msg =
            let mutable nextMsg = Some msg;
            if reentered
             then buffer.Push msg
             else
                 while Option.isSome nextMsg do
                     reentered <- true
                     let (model, cmd) = program.update nextMsg.Value state
                     let view = program.view model dispatch
                     Cmd.exec dispatch cmd |> ignore
                     state <- model;
                     nextMsg <- buffer.Pop()
                     reentered <- false;

        Cmd.exec dispatch initCmd

   let run program = runWith () program

