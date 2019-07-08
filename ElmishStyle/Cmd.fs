module Cmd 
    
    type Dispatch<'msg> = 'msg -> unit

    type Sub<'msg> = Dispatch<'msg> -> unit

    type Cmd<'msg> = Sub<'msg> list
        
    let exec<'msg> (dispatch: Dispatch<'msg>) (cmd: Cmd<'msg>) =
        cmd
        |> List.map (fun sub -> sub dispatch)

    let ofMsg<'msg> (msg: 'msg): Cmd<'msg> =
        [ fun (dispatch: Dispatch<'msg>) -> dispatch msg ]
