module Program
open ElmishStyle.LibraryModule
open ElmishProgram

[<EntryPoint>]
let main argv =
    let (program: Program<Model, Msg, unit>) =
                {
                    init = init
                    update = update;
                    view = view
                }
    run program |> ignore
    0 // return an integer exit code
