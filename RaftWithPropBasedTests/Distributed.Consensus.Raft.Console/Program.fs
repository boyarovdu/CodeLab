open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection

[<EntryPoint>]
let main argv =
    // Timer resets when node receives a leader ping
    // ....
    
    let node = new Node("1")
    let nl = Environment.NewLine
    
    node.MessagesStream.Subscribe(fun message ->
        match message.messageType with
        | LeaderElection leaderElectionMessage ->
            match leaderElectionMessage with
            | RequestVote candidate ->
                let candidateString = candidate.ToString()
                printfn $"Received vote request from candidate:%s{nl}%s{candidateString}%s{nl}"
            | _ -> ())
    |> ignore

    System.Console.ReadKey()
    |> ignore
    
    0
