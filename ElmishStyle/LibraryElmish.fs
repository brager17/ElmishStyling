
module ElmishStyle.LibraryModule
open System
open System
open System
open System.Drawing
open Seed
module oneSomeWorkflow =
    type OneSomeWorkflow() =
        member this.Bind(x, f) =
            match x with
            | Some s -> s
            | None -> f x

        member this.Return(x) = x
    let onesome = OneSomeWorkflow();

type SnapshotInfo =
    {
       text: string
       yPosition: int
       color: ConsoleColor
       formatText: string
    }

let mutable (storage: SnapshotInfo list) = [];

type ChangePosition =
    | Up
    | Down

type TapKey =
    | Button of ConsoleKey

type CountStatesBack = CountStatesBack of int

type RevertModel = CountStatesBack * SnapshotInfo

type ReverseMsg =
    | KeyButton of ConsoleKey


type ReadMsg =
    | KeyButton of ConsoleKey
    | EventStore


type Msg =
    | ReadMsg of ReadMsg
    | ReverseMsg of ReverseMsg

type Model =
    {
      SnapshotInfo: SnapshotInfo
      CountStatesBack: CountStatesBack
    }

let getlines (str: string) startY endY =
        let lines = str.Split [| '\n' |]
        let difference = abs (startY - endY);
        if difference > lines.Length
            then failwith "error"
            else
                let (startY', endY') =
                    if startY < 0
                     then (0, difference)
                     else if endY > lines.Length - 1 then (lines.Length - 1 - difference, lines.Length - 1)
                     else (startY, endY)
                lines.[startY'..endY'] |> List.ofArray
                |> String.concat ("\n")


let init() =
    let (Stix text) = seed.[Author.Block]
    let emptyModel = { text = text; yPosition = 0; color = ConsoleColor.Black; formatText = getlines text 0 3 }
    { SnapshotInfo = emptyModel; CountStatesBack = CountStatesBack 0 }, Cmd.ofMsg (ReadMsg EventStore)

type ButtonAction =
    | ConsoleColor of ConsoleColor
    | Author of Author
    | ChangePosition of ChangePosition

let readUpdate (msg: ReadMsg) (model: Model) =
    match msg with
    | KeyButton button ->

            let (|>>) o1 o2 =
               match o1 with
               | Some s -> Some s
               | None -> o2

            let q =
                    Option.bind (ConsoleColor >> Some)
                        (match button with
                        | ConsoleKey.D1 -> Some ConsoleColor.Black
                        | ConsoleKey.D2 -> Some ConsoleColor.Red
                        | ConsoleKey.D3 -> Some ConsoleColor.Blue
                        | ConsoleKey.D4 -> Some ConsoleColor.Green
                        | ConsoleKey.D5 -> Some ConsoleColor.Cyan
                        | _ -> None)

            let q1 =
                    Option.bind (Author >> Some)
                     (match button with
                     | ConsoleKey.P -> Some Author.Push
                     | ConsoleKey.B -> Some Author.Block
                     | ConsoleKey.E -> Some Author.Esenin
                     | ConsoleKey.L -> Some Author.Ler
                     | _ -> None)

            let q3 =
                    Option.bind (ChangePosition >> Some)
                     (match button with
                     | ConsoleKey.DownArrow -> Some ChangePosition.Down
                     | ConsoleKey.UpArrow -> Some ChangePosition.Up
                     | _ -> None)

            match (q |>> q1 |>> q3) with
            | Some s ->
                match s with
                | ConsoleColor color ->
                    { model with SnapshotInfo = { model.SnapshotInfo with color = color } }, Cmd.ofMsg EventStore
                | Author author ->
                     let (Stix text) = seed.[author];
                     let formatText = getlines text 0 2
                     let snapshotInfo = { text = text; yPosition = 0; color = model.SnapshotInfo.color; formatText = formatText }
                     { model with SnapshotInfo = snapshotInfo }, Cmd.ofMsg EventStore
                | ChangePosition change ->
                     match change with
                     | Up ->
                         if model.SnapshotInfo.yPosition = 0
                          then model, []
                          else
                          let formatText = getlines model.SnapshotInfo.text (model.SnapshotInfo.yPosition - 1) (model.SnapshotInfo.yPosition + 1)
                          let snapshotInfo = { model.SnapshotInfo with yPosition = model.SnapshotInfo.yPosition - 1; formatText = formatText }
                          { model with SnapshotInfo = snapshotInfo }, Cmd.ofMsg EventStore
                     | Down ->
                          if model.SnapshotInfo.yPosition = (model.SnapshotInfo.text.Split [| '\n' |]).Length - 1
                           then model, []
                           else
                              let formatText = getlines model.SnapshotInfo.text (model.SnapshotInfo.yPosition + 1) (model.SnapshotInfo.yPosition + 3)
                              { model with SnapshotInfo = { model.SnapshotInfo with yPosition = model.SnapshotInfo.yPosition + 1; formatText = formatText } }, Cmd.ofMsg EventStore


            | None -> failwith "error"
    | EventStore ->
        storage <- storage @ [ model.SnapshotInfo ]
        model, []


let view (model: Model) dispatch =
    let changeVersion = ReverseMsg.KeyButton >> ReverseMsg >> dispatch

    let changeContent = ReadMsg.KeyButton >> ReadMsg >> dispatch

    let matchDispatch key =
        match key with
        | ConsoleKey.LeftArrow | ConsoleKey.RightArrow -> changeVersion key
        | ConsoleKey.P | ConsoleKey.B | ConsoleKey.L | ConsoleKey.E
        | ConsoleKey.D1 | ConsoleKey.D2 | ConsoleKey.D3 | ConsoleKey.D4 | ConsoleKey.D5
        | ConsoleKey.UpArrow | ConsoleKey.DownArrow -> changeContent key
        | _ -> ()


    let r = model.SnapshotInfo

    Console.Clear();
    Console.ForegroundColor <- r.color
    Console.WriteLine(r.formatText)
    let key = Console.ReadKey().Key
    matchDispatch key




let reverseUpdate (msg: ReverseMsg) (model: Model) =
    let (CountStatesBack count) = model.CountStatesBack;
    match msg with
    | ReverseMsg.KeyButton k ->
        match k with
        | ConsoleKey.LeftArrow ->
            let snapshot = storage.[storage.Length - count - 1 - 1]
            let count = CountStatesBack(count + 1)
            { SnapshotInfo = snapshot; CountStatesBack = count }, []
        | ConsoleKey.RightArrow ->
            let snapshot = storage.[storage.Length - count + 1 - 1]
            let count = CountStatesBack(count - 1 - 1)
            { SnapshotInfo = snapshot; CountStatesBack = count }, []





let update (msg: Msg) (model: Model) =
    match msg with
    | ReadMsg readmsg ->
        let (model, cmd) = readUpdate readmsg model
        model, Cmd.map ReadMsg cmd
    | ReverseMsg reverseMsg ->
        let (model, cmd) = reverseUpdate reverseMsg model
        model, Cmd.map ReverseMsg cmd
    | _ -> failwith "error"



















