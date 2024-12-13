namespace Distributed.Consensus.Raft.Benchmark

open System
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
    member _.AcceptVoteBenchmark() =
        let node = new Node("1", 1_000)
        
        // Step 1: Node starts as a candidate
        let nodeState = node.ForceBecomeCandidate() |> Async.RunSynchronously

        match nodeState.role with
        | Candidate _ -> ()
        | _ -> failwith $"Node should be in the candidate state, but was %s{nodeState.role.ToString()}"

        // Step 2: Node becomes a leader
        let _ =
            [| 1..500 |]
            |> Array.map (fun i -> node.ProcessMessage(AcceptVote(i.ToString())))
            |> Async.Parallel
            |> Async.RunSynchronously
        
        match node.GetState().role with
        | Leader -> ()
        | _ -> failwith "Node should be in the leader state"
        
        (node :> IDisposable).Dispose()