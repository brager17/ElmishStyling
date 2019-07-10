
module Program
open System
open ElmishStyle.LibraryElmishCopy
open ElmishProgram
open ElmishStyle
open ElmishStyle



[<EntryPoint>]
let main argv =

    let messageToView = (fun msg ->
                                        match msg with
                                        | Event | Empty -> SnowAndUserActionView
                                        | ChangePosition _ | ChangeAuthor _ | ChangeColor _
                                        | ChangeVersion _ | ConsoleEvent _ -> OnlyShowView)
    let (program: Program<Model, Msg, unit>) =
                {
                    init = init
                    update = update;
                    view = view;
                    setState = (fun model dispatch -> view model dispatch)
                    eventView = (fun msg ->
                                        match msg with
                                        | Event | Empty -> SnowAndUserActionView
                                        | ChangePosition _ | ChangeAuthor _ | ChangeColor _
                                        | ChangeVersion _ | ConsoleEvent _ -> OnlyShowView

                                )
                }
    run program |> ignore
    0 // return an integer exit code
