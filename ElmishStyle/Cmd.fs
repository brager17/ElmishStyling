module Cmd

    type Dispatch<'msg> = 'msg -> unit

    type Sub<'msg> = Dispatch<'msg> -> unit

    type Cmd<'msg> = Sub<'msg> list

    let exec<'msg> (dispatch: Dispatch<'msg>) (cmd: Cmd<'msg>) =
        cmd
        |> List.map (fun sub -> sub dispatch)

    let ofMsg<'msg> (msg: 'msg): Cmd<'msg> =
        [ fun (dispatch: Dispatch<'msg>) -> dispatch msg ]

    let map<'msg, 'msg1> (mapFn: 'msg -> 'msg1) (cmd: Cmd<'msg>): Cmd<'msg1> =
        cmd
        |> List.map (fun sub -> (fun newDispatch -> mapFn >> newDispatch) >> sub)
