
module Program
open System
open ElmishStyle.LibraryElmishCopy
open ElmishProgram
open ElmishStyle
open ElmishStyle



[<EntryPoint>]
let main argv =
    let (program: Program<Model, Msg, unit>) =
                {
                    init = init
                    update = update;
                    view = view;
                    eventView = (fun msg ->
                                        match msg with
                                        | Event| Empty -> SnowAndUserActionView
                                        | ChangePosition _ | ChangeAuthor _ | ChangeColor _
                                        | ChangeVersion _ | ConsoleEvent _  -> OnlyShowView

                                )
                }
    run program |> ignore
    0 // return an integer exit code
