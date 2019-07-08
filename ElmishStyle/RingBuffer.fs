module RingBuffer
    type RingBufferState<'item> =
        | Write of wx: 'item array * wix: int
        | ReadAndWrite of rwx: 'item array * wix: int * rix: int

    type RingBuffer<'a>(n: int) =
        let arrMod wixrix (arr: 'a array) =
            seq {
                yield! arr |> Seq.skip wixrix
                yield! arr |> Seq.take wixrix
                for _ in 0..arr.Length do
                    yield Unchecked.defaultof<'a>
            } |> Array.ofSeq

        let mutable state = Write(Array.zeroCreate (max n 10), 0)

        member this.Pop() =
            match state with
            | ReadAndWrite(arr, wix, rix) ->
                let rix' = (rix + 1)%arr.Length;
                match wix = rix' with
                | true ->
                    state <- Write(arr, wix)
                | false ->
                    state <- ReadAndWrite(arr, wix,  rix')
                Some arr.[rix]
            | _ -> None

        member this.Push(item) =
            match state with
            | Write(arr, wix) ->
                arr.[wix] <- item
                let wix' = (wix+1)% arr.Length 
                state <- ReadAndWrite (arr,wix',wix)
            | ReadAndWrite(arr, wix, rix) ->
                let wix' = (wix+1) % arr.Length
                match wix' = rix with
                | true ->
                    state <- ReadAndWrite(arrMod wix arr, wix', 0)
                    arr.[wix] <- item;
                | false ->
                    arr.[wix] <- item;


