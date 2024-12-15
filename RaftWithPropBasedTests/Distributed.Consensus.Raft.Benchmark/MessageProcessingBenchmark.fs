namespace Distributed.Consensus.Raft.Benchmark

open System
open BenchmarkDotNet.Attributes
open Distributed.Consensus.Raft

type MessageProcessingBenchmark() =

    [<Params(10, 100, 1000)>]
    member val ClusterSize = 100 with get, set

    [<GlobalSetup>]
    member _.Setup() = ()

    [<Benchmark>]
    member this.ProcessVotes() =
        let clusterSize = this.ClusterSize // Get the parameterized cluster size
        let quorum = clusterSize / 2 + 1

        let node =
            new Node( "1", clusterSize,
                { electionMinTimeoutMs = 150
                  electionMaxTimeoutMs = 300
                  heartBeatTimeoutMs = 50 }
            )

        // Step 1: Node starts as a candidate
        let nodeState = node.ForceBecomeCandidate() |> Async.RunSynchronously

        match nodeState.role with
        | Candidate _ -> ()
        | _ -> failwith $"Node should be in the candidate state, but was %s{nodeState.role.ToString()}"

        // Step 2: Simulate RequestVote messages from concurrent candidates
        let requestVoteResults =
            [| 1 .. (clusterSize - quorum) |]
            |> Array.map (fun i -> node.ProcessMessage(RequestVote(i.ToString(), nodeState.electionTerm)))
            |> Async.Parallel
            |> Async.RunSynchronously

        if requestVoteResults.Length <> clusterSize - quorum then
            failwith $"Expected %d{clusterSize - quorum} results, but got %d{requestVoteResults.Length}"

        // Step 3: Simulate AcceptVote messages to elect a leader
        let acceptVoteResults =
            [| 1..quorum |]
            |> Array.map (fun i -> node.ProcessMessage(AcceptVote(i.ToString())))
            |> Async.Parallel
            |> Async.RunSynchronously

        if acceptVoteResults.Length <> quorum then
            failwith $"Expected %d{quorum} results, but got %d{acceptVoteResults.Length}"

        match node.GetState().role with
        | Leader -> ()
        | _ -> failwith "Node should be in the leader state"

        (node :> IDisposable).Dispose()
