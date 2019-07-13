module Tests
open ElmishStyle.LibraryElmish
open ElmishProgram
open FsCheck.Xunit
open FsCheck
open ElmishStyle.Seed

    let createProgram (msgPool: Msg list) =
                    let mutable counter = 0
                    {
                        init = init
                        update = update;
                        setState =
                            (fun _ msg dispatch ->
                                match msg with
                                | WaitUserAction ->
                                    if counter <= msgPool.Length - 1 then
                                            counter <- counter + 1
                                            dispatch msgPool.[counter - 1]
                                    else ()
                                | _ -> ())
                    }

[<Property(Verbose=true)>]
let ``Цвет равен последнему переданному цвету`` changeColorMsg =
    let state = (createProgram (changeColorMsg |> List.map ChangeColor) |> run)
    match (changeColorMsg |> List.tryLast) with
    | Some s -> state.viewTextInfo.color = s
    | None -> true


type Positive =
    static member Int() =
        Arb.Default.Int32()
        |> Arb.mapFilter abs (fun t -> t >= 0)


[<Property(Verbose=true)>]
let ``Автор равен последнему переданному автору`` authors =
    let state = (createProgram (authors |> List.map ChangeAuthor) |> run)
    match (authors |> List.tryLast) with
    | Some s ->
        let (Poem text) = seed.[s]
        state.viewTextInfo.text = text
    | None -> true



type ChangeContentCommands =
    static member ChangeContentCommands() =
        Arb.Default.Derive()
        |> Arb.filter (fun x -> match x with
                        | ChangePosition _ | ChangeAuthor _ | ChangeColor _ | ChangeVersion _
                         -> true | _ -> false)


type ChangeColorAuthorPosition =
    static member ChangeContentCommands() =
         Arb.Default.Derive()
        |> Arb.filter (fun x -> match x with | ChangeAuthor _ | ChangeColor _ -> true | _ -> false)

[<Property(Verbose=true,Arbitrary=[|typeof<ChangeContentCommands>|])>]
let ``Вызов случайных команд при их откате возвращает систему в первоначальное состояние`` (changeContentCommands: Msg) =
          let countChanges = [ changeContentCommands ].Length
          let reverseChanges = List.init countChanges (fun _ -> (ChangeVersion.Back |> ChangeVersion))
          let (stateInit, _) = init()
          let state = (createProgram ([ changeContentCommands ] @ reverseChanges) |> run)
          stateInit.viewTextInfo = state.viewTextInfo

[<Property(Verbose=true,Arbitrary=[|typeof<ChangeColorAuthorPosition>|])>]
let ``Вызов случайных цепочек команд смены цвета и автора корректен`` msgs =
      let tryLastSomeList list = list |> List.filter (Option.isSome)
                                      |> List.map (Option.get)
                                      |> List.tryLast
      let lastAuthor = msgs
                       |> List.map (fun x -> match x with
                                             | ChangeAuthor a -> Some a
                                             | _ -> None)
                       |> tryLastSomeList

      let lastColor = msgs
                       |> List.map (fun x -> match x with
                                            | ChangeColor a -> Some a
                                            | _ -> None)
                       |> tryLastSomeList

      let state = (createProgram msgs |> run)

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






