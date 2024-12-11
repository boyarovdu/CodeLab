open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection

open Distributed.Consensus.Raft.UnitTests
open Distributed.Consensus.Raft.UnitTests.LeaderElectionTests
open Microsoft.FSharp.Control
open FSharpx.Control.Observable
open NUnit.Framework

let checkIfFollowersConnectedToLeader (leader: LeaderInfo) (followers: FollowerInfo list) =
    followers
    |> List.forall (fun fi ->
        fi.electionTerm = leader.electionTerm
        && match fi.leader with
           | Some li -> li.nodeId = leader.nodeId
           | None -> false)

let waitForTheLeader predicate (cluster: TestClusterType) =
    cluster.diagnosticLogStream
    |> Observable.choose (fun logEntry ->
        match logEntry.finalState with
        | Leader li -> if predicate li then Some li else None
        | _ -> None)

    |> Async.AwaitObservable
    |> Async.RunSynchronously

[<EntryPoint>]
let main argv =
    let nl = Environment.NewLine

    let consoleMailbox =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (counter: int) =
                async {
                    let! message = inbox.Receive()
                    printfn $"----- Operation %d{counter} -----%s{nl}%s{message}%s{nl}"
                    return! loop (counter + 1)
                }

            loop 1)

    let testCluster = TestCluster.startCluster 3

    let diagnosticCollector =
        testCluster.diagnosticLogStream
        |> Observable.filter (fun logEntry ->
            match logEntry.mailboxMessage with
            | ProcessHeartbeatTimeout -> false
            | _ -> true)
        |> Observable.subscribe (fun message ->
            let msg = $"%s{message.ToString()}"
            consoleMailbox.Post(msg))

    try
        // Wait for the leader to be elected
        let initialLeader = testCluster |> waitForTheLeader (fun _ -> true)

        printfn "CHECKPOINT 1"

        // Block current leader
        testCluster.transport.SetBlockedNodes [| initialLeader.nodeId |]

        printfn "CHECKPOINT 2"

        // Wait for the second leader to be elected
        let reelectedLeader = testCluster |> waitForTheLeader (fun li -> li.nodeId <> initialLeader.nodeId)
    
        printfn "CHECKPOINT 3"
        
        Assert.IsTrue(reelectedLeader.electionTerm > initialLeader.electionTerm)
        
        let followerNodes = TestCluster.getFollowerNodes testCluster
        Assert.AreEqual(testCluster.nodes.Length - 2, followerNodes.Length)
        
        printfn "CHECKPOINT 4"
        
        Assert.IsTrue(followerNodes |> checkIfFollowersConnectedToLeader reelectedLeader)
        
        // Old leader becomes follower after restoring communication
        testCluster.transport.SetBlockedNodes [||]
        
        printfn "CHECKPOINT 5"
        
        // Wait for current leader to acknowledge all nodes
        testCluster
        |> getNumberOfAppendEntryMessages (fun li -> li.nodeId <> initialLeader.nodeId) 2
        |> ignore
        
        printfn "CHECKPOINT 6"
    //
    // // All followers connected to new leader
    // let followerNodes2 = TestCluster.getFollowerNodes testCluster
    //
    // printfn "CHECKPOINT 7"
    //
    // Assert.AreEqual(testCluster.nodes.Length - 1, followerNodes2.Length)
    // // if algorithm ever needed more than 6 terms to elect new leader, I want to be informed to investigate such situation
    // Assert.Less(reelectedLeader.electionTerm, 6)
    //
    // Assert.True(followerNodes2 |> checkIfFollowersConnectedToLeader reelectedLeader)
    //
    // printfn "CHECKPOINT 8"

    finally
        diagnosticCollector.Dispose()

    0
