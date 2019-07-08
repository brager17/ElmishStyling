
module ElmishStyle.LibraryModule
open System
open System
open System.Drawing
open Seed

type Model =
    {
       text: string
       yPosition: int
       color: ConsoleColor
       formatText: string
    }

type EnterText =
    | ChangeAuthor of Author
    | ChangeColor of ConsoleColor
    | ChangeAuthorAndColor of Author * ConsoleColor

type ChangePosition =
    | Up = 1
    | Down = 2

type TapKey =
    | ChangePosition of ChangePosition
    | Back
    | Forward

type Msg =
    | Initial
    | EnterText of EnterText
    | KeyButton of TapKey
    | EventStore

let init() =
    { text = ""; yPosition = 0; color = ConsoleColor.Black; formatText = "" }, Cmd.ofMsg Msg.Initial

let view (model: Model) dispatch =
    if model.formatText = ""
        then
            Console.WriteLine ("Выберите автора : Push - 1 ; Ler - 2 ; Esenin - 3 ; Block - 4 ; Exit - ESC и цвет Black - 1 Read - 2 Blue - 3 Green - 4 Cyan - 5 в формате : 12")
            let str = Console.ReadLine();
            let a = Convert.ToInt32(str.[0].ToString())
            let c = Convert.ToInt32(str.[1].ToString())


            let author =
                     match a with
                     | 1 -> Author.Push
                     | 2 -> Author.Ler
                     | 3 -> Author.Esenin
                     | 4 -> Author.Block
                     | _ -> failwith "error"

            let color =
                     match c with
                     | 0 -> ConsoleColor.Black
                     | 1 -> ConsoleColor.Red
                     | 2 -> ConsoleColor.Blue
                     | 3 -> ConsoleColor.Green
                     | 4 -> ConsoleColor.Cyan
                     | _ -> failwith "error"

            (author, color) |> ChangeAuthorAndColor |> EnterText |> dispatch
        else
            Console.Clear();
            Console.WriteLine(model.formatText);
            let key = Console.ReadKey();
            match key.Key with
            | ConsoleKey.UpArrow ->
                ChangePosition.Up |> ChangePosition |> KeyButton |> dispatch
            | ConsoleKey.DownArrow ->
                ChangePosition.Down |> ChangePosition |> KeyButton |> dispatch
            | _ -> failwith ""





let update (msg: Msg) (model: Model) =

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

    match msg with
    | Initial -> { text = ""; yPosition = 0; color = ConsoleColor.Black; formatText = "" }, []

    | EnterText r ->
        match r with
         | ChangeAuthorAndColor(a, c) ->
                 let (Stix text) = seed.[a];
                 let formatText = getlines text 0 2
                 { text = text; yPosition = 0; color = c; formatText = formatText }, Cmd.ofMsg EventStore

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










