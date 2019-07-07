// Learn more about F# at http://fsharp.org
open System
type Dispatch<'msg> = 'msg -> unit

type Sub<'msg> = Dispatch<'msg> -> unit

type Cmd<'msg> = Sub<'msg> list

module RingBuffer =
    type RingBufferState<'item> =
        | Write of wx: 'item array * wix: int
        | ReadAndWrite of rwx: 'item array * wix: int * rix: int

    type RungBuffer<'a>(n: int) =
        let arrMod wixrix (arr: 'a array) =
            seq {
                yield! arr |> Seq.skip wixrix
                yield! arr |> Seq.take wixrix
                for _ in 0..arr.Length do
                    yield Unchecked.defaultof<'a>
            } |> Array.ofSeq

        let mutable state = Write(Array.zeroCreate (max n 10), 0)

        member this.Pop() =
            match state with
            | ReadAndWrite(arr, wix, rix) ->
                let rix' = (rix + 1)%arr.Length;
                match wix = rix' with
                | true ->
                    state <- Write(arr, wix)
                | false ->
                    state <- ReadAndWrite(arr, wix,  rix')
                Some arr.[rix]
            | _ -> None

        member this.Push(item) =
            match state with
            | Write(arr, wix) ->
                arr.[wix] <- item
                let wix' = (wix+1)% arr.Length 
                state <- ReadAndWrite (arr,wix',wix)
            | ReadAndWrite(arr, wix, rix) ->
                let wix' = (wix+1) % arr.Length
                match wix' = rix with
                | true ->
                    state <- ReadAndWrite(arrMod wix arr, wix', 0)
                    arr.[wix] <- item;
                | false ->
                    arr.[wix] <- item;





module Cmd =
    let exec<'msg> (dispatch: Dispatch<'msg>) (cmd: Cmd<'msg>) =
        cmd
        |> List.map (fun sub -> sub dispatch)

    let ofMsg<'msg> (msg: 'msg): Cmd<'msg> =
        [ fun (dispatch: Dispatch<'msg>) -> dispatch msg ]


module Program =
   type Program<'model, 'msg, 'view> =
        {
          init: 'model * Cmd<'msg>
          update: 'msg -> 'model -> ('model * Cmd<'msg>)
          view: 'model -> Dispatch<'msg> -> 'view
         }

   let runWith<'arg, 'model, 'msg, 'view> (arg: 'arg) (program: Program<'model, 'msg, 'view>) =
        let (initModel, initCmd) = program.init
        let mutable state = initModel
        let mutable reentered = false
        let buffer = RingBuffer.RungBuffer<'msg> 10

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
type Model =
    { x: int }

type Msg =
    | Increment
    | Decrement

let init =
    { x = 0 }, Cmd.ofMsg Increment

let update msg model =
    match msg with
    | Increment when model.x < 3 ->
        { model with x = model.x + 1 }, Cmd.ofMsg Increment

    | Increment ->
        { model with x = model.x + 1 }, Cmd.ofMsg Decrement

    | Decrement when model.x > 0 ->
        { model with x = model.x - 1 }, Cmd.ofMsg Decrement

    | Decrement ->
        { model with x = model.x - 1 }, Cmd.ofMsg Increment

let view model dispath = printf "%A\n" model;

[<EntryPoint>]
let main argv =

    let (program: Program.Program<Model, Msg, unit>) =
                {
                    init = init
                    update = update;
                    view = view
                }
    Program.run program |> ignore
    0 // return an integer exit code
