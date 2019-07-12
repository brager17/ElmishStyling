module Tests
open System
open ElmishStyle.LibraryElmishCopy
open ElmishProgram
open ElmishStyle.LibraryElmishCopy

open System

open FsCheck.Xunit
open FsCheck
open ElmishStyle.Seed
let createProgram (msgPool: Msg list) =
                let mutable counter = 0
                {
                    init = init
                    update = update;
                    view = (fun x y -> ())
                    setState = (fun _ msg dispatch ->
                                    match msg with

                                    | WaitUserAction ->
                                        if counter <= msgPool.Length - 1 then
                                                counter <- counter + 1
                                                dispatch msgPool.[counter - 1]
                                        else ()
                                    | _ -> ()
                        )
                }


[<Property(Verbose=true,StartSize=1000,MaxTest=1000)>]
let ``Цвет равен последнему переданному цвету`` (changeColorMsg: ConsoleColor list) =
    let state = (createProgram (changeColorMsg |> List.map ChangeColor) |> run)
    match (changeColorMsg |> List.tryLast) with
    | Some s -> state.viewTextInfo.color = s
    | None -> true

[<Property(Verbose=true,StartSize=1000,MaxTest=1000)>]
let ``Автор равен последнему переданному автору`` authors =
    let state = (createProgram (authors |> List.map ChangeAuthor) |> run)
    match (authors |> List.tryLast) with
    | Some s ->
        let (Poem text) = seed.[s]
        state.viewTextInfo.text = text
    | None -> true

[<Property(Verbose=true,StartSize=1000,MaxTest=1000)>]
let ``Одинаковое количество форматирований вниз и вверх оставляют систему в том же состоянии`` countChangePositions =
       if countChangePositions > 0
       then
           let toUpSeq = List.init countChangePositions (fun x -> ChangePosition.Up)
           let dSeq = List.init countChangePositions (fun x -> ChangePosition.Down)
           let UpToSeq = toUpSeq @ dSeq |> List.map (ChangePosition)
           let SeqToUp = dSeq @ toUpSeq |> List.map (ChangePosition)
           let (model, cmd) = init()
           let state = (createProgram UpToSeq |> run)
           model.viewTextInfo.positionY = state.viewTextInfo.positionY
       else true

[<Property(Verbose=true,StartSize=1000,MaxTest=1000)>]
let ``Вызов случайных команд при их откате возвращает систему в первоначальное состояние``
      changeColors changeAuthors changePosition changeVersion =
          let changeColorsMsgs = changeColors |> List.map (ChangeColor)
          let changeAuthorMsgs = changeAuthors |> List.map (ChangeAuthor)
          let changePositionMsgs = changePosition |> List.map (ChangePosition)
          let changeVersionMsgs = changeVersion |> List.map (ChangeVersion)
          let allChanges = changeColorsMsgs @ changeAuthorMsgs @ changePositionMsgs @ changeVersionMsgs

          let countChanges = allChanges.Length

          let reverseChanges = List.init countChanges (fun _ -> (ChangeVersion.Back |> ChangeVersion))

          let (stateInit, _) = init()
          let state = (createProgram (allChanges @ reverseChanges) |> run)
          stateInit.viewTextInfo = state.viewTextInfo

[<Property(Verbose=true,MaxTest=1000)>]
let ``Вызов случайных цепочек команд смены цвета и автора корректен`` changeColors changeAuthors =
          let changeColorsMsgs = changeColors |> List.map (ChangeColor)
          let changeAuthorMsgs = changeAuthors |> List.map (ChangeAuthor)
          
          let allChanges = changeColorsMsgs @ changeAuthorMsgs 

          let lastColor = changeColors |> List.tryLast
          let lastAuthor = changeAuthors |> List.tryLast

          let state = (createProgram allChanges |> run)

          let colorTest =
              match lastColor with
              | Some s -> state.viewTextInfo.color = s
              | None -> true

          let authorTest =
              match lastAuthor with
              | Some s ->
                  let (Poem t) = seed.[s];
                  state.viewTextInfo.text = t
              | None -> true

          authorTest && colorTest













