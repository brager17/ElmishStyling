﻿
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
                    view = (fun x y -> ())
                    setState = (fun model msg dispatch ->
                                    let view =  match msg with
                                                    
                                                    | WaitUserAction -> SnowAndUserActionView
                                                    | ChangePosition _ | ChangeAuthor _ | ChangeColor _
                                                    | ChangeVersion _ | ConsoleEvent _ | RememberModel _ -> OnlyShowView
                                    
                                    view model dispatch
                        )
                }
    run program |> ignore
    0 // return an integer exit code
