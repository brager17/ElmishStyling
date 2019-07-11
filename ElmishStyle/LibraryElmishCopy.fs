module ElmishStyle.LibraryElmishCopy
open ElmishStyle.Seed
open System
open ElmishProgram
open Cmd
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
    | FixEvent
    | WaitUserAction

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

    | _ -> WaitUserAction


let splitStr (str: string) =
        str.Split [| '\n' |]

// no error handling
let getlines (str: string) startY endY =
        let lines = splitStr str
        let _ = abs (startY - endY);
        lines.[startY ..endY] |> List.ofArray
                |> String.concat ("\n")

let init() =
    let (Stix text) = seed.[Author.Block]
    let countLines = (splitStr text).Length
    let emptyModel = { viewTextInfo = { text = text; positionY = 0; color = ConsoleColor.Black; formatText = getlines text 0 3; countLines = countLines }; countVersionBack = 0; history = [] }
    emptyModel, Cmd.ofMsg (ChangePosition ChangePosition.Up)

let updateChangeAuthor (model: Model) (author: Author) =
    let (Stix text) = seed.[author]
    let formatText = getlines text 0 3
    let countLines = (splitStr text).Length
    { model with viewTextInfo = { model.viewTextInfo with text = text; formatText = formatText; countLines = countLines }; countVersionBack = 0 }, Cmd.ofMsg FixEvent


let updateChangeColor (model: Model) (color: ConsoleColor) =
    { model with viewTextInfo = { model.viewTextInfo with color = color }; countVersionBack = 0 }, Cmd.ofMsg FixEvent


let updateChangePosition (model: Model) (changePosition: ChangePosition) =
    let text = model.viewTextInfo.text
    let formatText = model.viewTextInfo.formatText
    
    let (yPosition, formatText) = match changePosition, model.viewTextInfo.positionY, model.viewTextInfo.countLines with
                                    | Up, y, length when y < length - 4 -> (y + 1, getlines text (y + 1) (y + 4))
                                    | Down, y, _ when y > 0 -> (y - 1, getlines text (y - 1) (y + 2))
                                    | _, y, _ -> (y,formatText)

    { model with viewTextInfo = { model.viewTextInfo with positionY = yPosition; formatText = formatText }; countVersionBack = 0 }, Cmd.ofMsg FixEvent


let updateChangeVersion (model: Model) (changeVersion: ChangeVersion) =
    let countVersionBack =
                 match changeVersion, model.countVersionBack, model.history.Length with
                 | Back, countVersionBack, length when countVersionBack < length - 1 -> countVersionBack + 1
                 | Forward, countVersionBack, _ when countVersionBack > 0 -> countVersionBack - 1
                 | _, countVersionBack, _ -> countVersionBack

    let reverseModel = model.history.[model.history.Length - 1 - countVersionBack]
    { model with countVersionBack = countVersionBack; viewTextInfo = reverseModel; }, Cmd.ofMsg WaitUserAction

let updateAddEvent model =
    { model with history = model.history @ [ model.viewTextInfo ] }, Cmd.ofMsg WaitUserAction

let update (msg: Msg) (model: Model) =
    match msg with
    | ConsoleEvent key -> model, Cmd.ofMsg (updateConsoleEvent key)
    | ChangeAuthor author -> updateChangeAuthor model author
    | ChangeColor color -> updateChangeColor model color
    | ChangePosition position -> updateChangePosition model position
    | ChangeVersion version -> updateChangeVersion model version
    | FixEvent -> updateAddEvent model
    | WaitUserAction -> model, []

let clearConsoleAndPrintTextWithColor (text:string) (color:ConsoleColor)=
   Console.Clear();
   Console.WriteLine()
   Console.ForegroundColor <- color
   Console.WriteLine(text)

let OnlyShowView (model: Model) (_: Msg -> unit) =
   clearConsoleAndPrintTextWithColor model.viewTextInfo.formatText  model.viewTextInfo.color

let SnowAndUserActionView (model: Model) (dispatch: Msg -> unit) =
   clearConsoleAndPrintTextWithColor model.viewTextInfo.formatText  model.viewTextInfo.color
   let key = Console.ReadKey().Key;
   Msg.ConsoleEvent key |> dispatch

