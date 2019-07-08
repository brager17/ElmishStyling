
module ElmishStyle.LibraryModule
open System
open System
open System
open System.Drawing
open Seed

type SnapshotInfo =
    {
       text: string
       yPosition: int
       color: ConsoleColor
       formatText: string
    }

let mutable (storage: SnapshotInfo list) = [];

type TapKey =
    | Button of ConsoleKey

type CountStatesBack = CountStatesBack of int

type RevertModel = (CountStatesBack  * SnapshotInfo)

type ReverseMsg =
    | KeyButton of ConsoleKey

type Model =
    | Read of SnapshotInfo
    | Reverse of RevertModel * SnapshotInfo

type ReadMsg =
    | KeyButton of ConsoleKey
    | EventStore


type Msg =
    | ReadMsg of ReadMsg
    | ReverseMsg of ReverseMsg

let init() =
    { text = ""; yPosition = 0; color = ConsoleColor.Black; formatText = "" }, Cmd.ofMsg ((Author.Block, ConsoleColor.Black) |> ChangeAuthorAndColor |> EnterText)

let view (model: Model) dispatch =


            Console.Clear();
            Console.ResetColor()
            Console.ForegroundColor <- model.color
            Console.WriteLine(model.formatText);
            let key = Console.ReadKey();
            match key.Key with

            | ConsoleKey.UpArrow ->
                ChangePosition.Up |> ChangePosition |> KeyButton |> dispatch

            | ConsoleKey.DownArrow ->
                ChangePosition.Down |> ChangePosition |> KeyButton |> dispatch

            | ConsoleKey.D1 -> ConsoleColor.Black |> ChangeColor |> EnterText |> dispatch
            | ConsoleKey.D2 -> ConsoleColor.Red |> ChangeColor |> EnterText |> dispatch
            | ConsoleKey.D3 -> ConsoleColor.Blue |> ChangeColor |> EnterText |> dispatch
            | ConsoleKey.D4 -> ConsoleColor.Green |> ChangeColor |> EnterText |> dispatch
            | ConsoleKey.D5 -> ConsoleColor.Cyan |> ChangeColor |> EnterText |> dispatch

            | ConsoleKey.P -> Author.Push |> ChangeAuthor |> EnterText |> dispatch
            | ConsoleKey.L -> Author.Ler |> ChangeAuthor |> EnterText |> dispatch
            | ConsoleKey.E -> Author.Esenin |> ChangeAuthor |> EnterText |> dispatch
            | ConsoleKey.B -> Author.Block |> ChangeAuthor |> EnterText |> dispatch

            | _ -> failwith ""

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


let reverseUpdate (msg:ReverseMsg) (model:RevertModel) =
    match msg with
    |KeyButton button ->
        


let readUpdate (msg: ReadMsg) (model: SnapshotInfo) =
    match msg with
     | KeyButton button ->
             let colorChangedButton = match button with
                                         | ConsoleKey.D1 -> Some ConsoleColor.Black
                                         | ConsoleKey.D2 -> Some ConsoleColor.Red
                                         | ConsoleKey.D3 -> Some ConsoleColor.Blue
                                         | ConsoleKey.D4 -> Some ConsoleColor.Green
                                         | ConsoleKey.D5 -> Some ConsoleColor.Cyan
                                         | _ -> None

             match colorChangedButton with
             | Some s -> { model with color = s },[]
             | None ->
                 let authorChangedButton = match button with
                                             | ConsoleKey.P -> Some Author.Push
                                             | ConsoleKey.B -> Some Author.Block
                                             | ConsoleKey.E -> Some Author.Esenin
                                             | ConsoleKey.L -> Some Author.Ler
                                             | _ -> None

                 match authorChangedButton with
                 | Some s ->
                     let (Stix text) = seed.[s];
                     let formatText = getlines text 0 2
                     {text = text; yPosition = 0; color = model.color; formatText = formatText}, Cmd.ofMsg EventStore
                 | None -> model, []





let update (msg: Msg) (model: Model) =

    match msg with
    | EnterText r ->
        match r with
         | ChangeAuthorAndColor(a, c) ->
                 let (Stix text) = seed.[a];
                 let formatText = getlines text 0 2
                 { text = text; yPosition = 0; color = c; formatText = formatText }, Cmd.ofMsg EventStore

         | ChangeColor c ->
             { model with color = c }, Cmd.ofMsg EventStore

         | ChangeAuthor a ->
                 let (Stix text) = seed.[a];
                 let formatText = getlines text 0 2
                 { text = text; yPosition = 0; color = model.color; formatText = formatText }, Cmd.ofMsg EventStore

    | KeyButton tabKey ->
        match tabKey with
        | ChangePosition changePosition ->
            match changePosition with

            | ChangePosition.Down ->
                if model.yPosition = (model.text.Split [| '\n' |]).Length - 1
                  then model, []
                  else
                      let formatText = getlines model.text (model.yPosition + 1) (model.yPosition + 3)
                      { model with yPosition = model.yPosition + 1; formatText = formatText }, Cmd.ofMsg EventStore

            | ChangePosition.Up ->
                if model.yPosition = 0
                then model, []
                else
                      let formatText = getlines model.text (model.yPosition - 1) (model.yPosition + 1)
                      { model with yPosition = model.yPosition - 1; formatText = formatText }, Cmd.ofMsg EventStore

        | Back ->
            storage.[storage.Length - 1], Cmd.ofMsg EventStore
















