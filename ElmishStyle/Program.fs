
module Program
open System
open ElmishStyle.LibraryElmish
open ElmishProgram
open ElmishStyle
open ElmishStyle



[<EntryPoint>]
let main argv =
    let (program: Program<Model, Msg, unit>) =
                {
                    init = init
                    update = update;
                    setState =
                     (fun model msg dispatch ->
                        let view =  match msg with
                                     | WaitUserAction -> SnowAndUserActionView
                                     
                                     | ChangePosition _ | ChangeAuthor _ | ChangeColor _
                                     | ChangeVersion _ | ConsoleEvent _ | RememberModel _ -> OnlyShowView
                                     
                                     | Exit -> ExitView
                        view model dispatch)
                }
    run program |> ignore
    0 // return an integer exit code
