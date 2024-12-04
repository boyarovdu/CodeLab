open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection

[<EntryPoint>]
let main argv =
    let nodes =
        [| new Node("1", [| "2"; "3" |])
           new Node("2", [| "1"; "3" |])
           new Node("3", [| "1"; "2" |]) |]

    let transport = FakeAsyncTransport(nodes)

    let mergedEventsStream =
        nodes.[0].MessagesStream
        |> Observable.merge nodes.[1].MessagesStream
        |> Observable.merge nodes.[2].MessagesStream

    nodes |> Array.iter _.Start()

    Console.ReadKey() |> ignore

    let temp =
        mergedEventsStream
        |> Observable.filter (fun m ->
            match m.messageType with
            | LeaderElection(AppendEntry li) -> true
            | _ -> false)
        |> Observable.subscribe (fun m ->
            let nodeToStop = m.senderId
            transport.SetBlockedNodes [| nodeToStop |]
            printfn $"Node %s{nodeToStop} is blocked")
        
    Console.ReadKey() |> ignore
    temp.Dispose();

    Console.ReadKey() |> ignore
    0
