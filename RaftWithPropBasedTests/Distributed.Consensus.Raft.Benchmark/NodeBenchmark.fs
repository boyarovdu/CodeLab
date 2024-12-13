namespace Distributed.Consensus.Raft.Benchmark

open BenchmarkDotNet.Attributes
open Distributed.Consensus.Raft

[<MemoryDiagnoser>]
type NodeBenchmark() =

    let mutable node: Node = Unchecked.defaultof<Node>

    [<GlobalSetup>] 
    member _.Setup() =
        node <- new Node("1", 3) 
        node.Start()

    [<Benchmark>]
    member _.FullNodeLifecycle() =
        // Step 1: Node becomes a candidate
        let nodeState = node.ForceBecomeCandidate() |> Async.RunSynchronously
        match nodeState.role with
        | Candidate _ -> ()
        | _ -> failwith "Node should be in the candidate state"
        
        // Step 2: Node becomes a leader
        node.ProcessMessage (AcceptVote "2") |> Async.RunSynchronously |> ignore
        let nodeState = node.ProcessMessage (AcceptVote "3") |> Async.RunSynchronously
        match nodeState.role with
        | Leader -> ()
        | _ -> failwith "Node should be in the leader state"
        
        // Step 3: Node becomes a follower due to heartbeat timeout from leader with higher election term 
        let nodeState = node.ProcessMessage (AppendEntry("3", nodeState.electionTerm + 1)) |> Async.RunSynchronously
        match nodeState.role with
        | Follower fi ->
            if fi.leader = Some "3" then ()
            else failwith "Node should be following leader 3"
        | _ -> failwith "Node should be in the follower state"
        
        // Step 4: Node becomes follower of another candidate with higher term
        let nodeState = node.ProcessMessage (RequestVote ("2", nodeState.electionTerm + 1)) |> Async.RunSynchronously
        match nodeState.role with
        | Follower fi ->
            if fi.votedFor = Some "2" then ()
            else failwith "Node should have voted for candidate 2"
        | _ -> failwith "Node should be in the follower state"
        
        // Step 2: Node follows new leader
        let nodeState = node.ProcessMessage (AppendEntry ("2", nodeState.electionTerm + 1)) |> Async.RunSynchronously
        match nodeState.role with
        | Follower fi -> 
            if fi.leader = Some "2" then ()
            else failwith "Node should be following leader 2"
        | _ -> failwith "Node should be in the follower state"

