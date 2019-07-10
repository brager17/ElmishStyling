module ElmishStyle.LibraryElmishCopy
open ElmishStyle.Seed
open System
open ElmishProgram
open Cmd
type YPosition = YPosition of int

type ChangeVersion =
    | Back
    | Forward

type ChangePosition =
    | Up
    | Down

type ViewTextInfo =
    {
        text: string;
        formatText: string;
        countLines: int;
        positionY: int;
        color: ConsoleColor
    }

type Msg =
    | ConsoleEvent of ConsoleKey
    | ChangeAuthor of Author
    | ChangeColor of ConsoleColor
    | ChangePosition of ChangePosition
    | ChangeVersion of ChangeVersion
    | Event
    | Empty

type Model =
    {
        viewTextInfo: ViewTextInfo
        countVersionBack: int
        history: ViewTextInfo list
    }

let updateConsoleEvent (key: ConsoleKey) =
    match key with

    | ConsoleKey.D1 -> ChangeColor ConsoleColor.Red
    | ConsoleKey.D2 -> ChangeColor ConsoleColor.Green
    | ConsoleKey.D3 -> ChangeColor ConsoleColor.Blue
    | ConsoleKey.D4 -> ChangeColor ConsoleColor.Black
    | ConsoleKey.D5 -> ChangeColor ConsoleColor.Cyan

    | ConsoleKey.LeftArrow -> ChangeVersion Back
    | ConsoleKey.RightArrow -> ChangeVersion Forward

    | ConsoleKey.P -> ChangeAuthor Author.Push
    | ConsoleKey.E -> ChangeAuthor Author.Esenin
    | ConsoleKey.B -> ChangeAuthor Author.Block
    | ConsoleKey.L -> ChangeAuthor Author.Ler

    | ConsoleKey.UpArrow -> ChangePosition Up
    | ConsoleKey.DownArrow -> ChangePosition Down

    | _ -> Empty


let splitStr (str: string) =
        str.Split [| '\n' |]

let getlines (str: string) startY endY =
        let lines = splitStr str
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
    let countLines = (splitStr text).Length
    let emptyModel = { viewTextInfo = { text = text; positionY = 0; color = ConsoleColor.Black; formatText = getlines text 0 3; countLines = countLines }; countVersionBack = 0; history = [] }
    emptyModel, Cmd.ofMsg (ChangePosition ChangePosition.Up)

let updateChangeAuthor (model: Model) (author: Author) =
    let (Stix text) = seed.[author]
    let formatText = getlines text 0 2
    let countLines = (splitStr text).Length
    { model with viewTextInfo = { model.viewTextInfo with text = text; formatText = formatText; countLines = countLines } ; countVersionBack = 0 }, Cmd.ofMsg Event


let updateChangeColor (model: Model) (color: ConsoleColor) =
    { model with viewTextInfo = { model.viewTextInfo with color = color }; countVersionBack = 0  }, Cmd.ofMsg Event


let updateChangePosition (model: Model) (changePosition: ChangePosition) =
    let (yPosition, formatText) = match changePosition, model.viewTextInfo.positionY, model.viewTextInfo.countLines with
                                    | Up, y, length when y < length - 3 -> (y + 1, getlines model.viewTextInfo.text (y + 1) (y + 4))
                                    | Down, y, _ when y > 0 -> (y - 1, getlines model.viewTextInfo.text (y - 1) (y + 3))
                                    | _, _, _ -> (model.viewTextInfo.positionY, model.viewTextInfo.formatText)

    { model with viewTextInfo = { model.viewTextInfo with positionY = yPosition; formatText = formatText }; countVersionBack = 0 }, Cmd.ofMsg Event


let updateChangeVersion (model: Model) (changeVersion: ChangeVersion) =
    let countVersionBack =
                 match changeVersion, model.countVersionBack, model.history.Length with
                 | Back, countVersionBack, length when countVersionBack < length -> model.countVersionBack + 1
                 | Forward, countVersionBack, _ when countVersionBack > 0 -> model.countVersionBack - 1
                 | _, _, _ -> model.countVersionBack

    let reverseModel = model.history.[model.history.Length - 1 - countVersionBack]
    { model with countVersionBack = countVersionBack; viewTextInfo = reverseModel;},Cmd.ofMsg Empty


let updateAddEvent model =
    { model with history = model.history @ [ model.viewTextInfo ] }, []


let update (msg: Msg) (model: Model) =
    match msg with
    | ConsoleEvent key -> model, Cmd.ofMsg (updateConsoleEvent key)
    | ChangeAuthor author -> updateChangeAuthor model author
    | ChangeColor color -> updateChangeColor model color
    | ChangePosition position -> updateChangePosition model position
    | ChangeVersion version -> updateChangeVersion model version
    | Event -> updateAddEvent model
    | Empty -> model, Cmd.ofMsg Empty

let OnlyShowView (model: Model) (dispatch: Msg -> unit) =
   Console.Clear();
   Console.ForegroundColor <- model.viewTextInfo.color
   Console.WriteLine(model.viewTextInfo.formatText)

let SnowAndUserActionView (model: Model) (dispatch: Msg -> unit) =
   Console.Clear();
   Console.ForegroundColor <- model.viewTextInfo.color
   Console.WriteLine(model.viewTextInfo.formatText)
   let key = Console.ReadKey().Key;
   Msg.ConsoleEvent key |> dispatch

let view msg dispath = ()
