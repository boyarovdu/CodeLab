open System
open System.Threading
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection

open Distributed.Consensus.Raft.UnitTests
open FSharpx.Control
open Microsoft.FSharp.Control
open FSharpx.Control.Observable

[<EntryPoint>]
let main argv =
    let testCluster = TestCluster.startCluster 3

    // Wait for the leader to be elected
    let appendEntryMessage =
        testCluster
        |> TestCluster.waitAppendEntryMessageAsync (fun li -> true)
        |> Async.RunSynchronously

    let leaderNodes = TestCluster.getLeaderNodes testCluster
    let followerNodes = TestCluster.getFollowerNodes testCluster

    0
