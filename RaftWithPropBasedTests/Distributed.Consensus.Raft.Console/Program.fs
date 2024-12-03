open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection

[<EntryPoint>]
let main argv =
    let nodes = [| new Node("1", [|"2"; "3"|]); new Node("2", [|"1"; "3"|]); new Node("3", [|"1"; "2"|]) |]
    let _ = MessageBus(nodes)

    nodes |> Array.iter _.Start()

    Console.ReadKey() |> ignore
    0
