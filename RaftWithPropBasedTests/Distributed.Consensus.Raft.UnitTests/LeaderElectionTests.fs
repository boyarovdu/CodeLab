namespace Distributed.Consensus.Raft.UnitTests

open Distributed.Consensus.Raft.LeaderElection
open NUnit.Framework
open Distributed.Consensus.Raft.UnitTests
open FSharpx.Control.Observable

module LeaderElectionTests =

    let checkIfFollowersConnectedToLeader (leader: LeaderInfo) (followers: FollowerInfo list) =
        followers
        |> List.forall (fun fi ->
            fi.electionTerm = leader.electionTerm
            && match fi.leader with
               | Some li -> li.nodeId = leader.nodeId
               | None -> false)

    let getNumberOfAppendEntryMessages predicate amount cluster =
        cluster
        |> TestCluster.waitNumberOfAppendEntryMessagesAsync predicate amount
        |> Async.AwaitObservable
        |> Async.RunSynchronously

    [<Test>]
    let ``Only one node elected as leader, all other nodes are followers`` () =
        let testCluster = TestCluster.startCluster 3

        // Wait for the leader to be elected
        let leaderInfo =
            testCluster |> getNumberOfAppendEntryMessages (fun _ -> true) 2 |> List.head

        let leaderNodes = TestCluster.getLeaderNodes testCluster
        let followerNodes = TestCluster.getFollowerNodes testCluster

        Assert.IsTrue(followerNodes |> checkIfFollowersConnectedToLeader leaderInfo)
        Assert.AreEqual(1, leaderNodes.Length)
        Assert.AreEqual(testCluster.nodes.Length - 1, followerNodes.Length)
        Assert.AreEqual(leaderInfo.nodeId, leaderNodes.Head.nodeId)
        Assert.Less(leaderInfo.electionTerm, 3)


    [<Test>]
    let ``Cluster re-elects new leader in case old one becomes unavailable`` () =
        let testCluster = TestCluster.startCluster 3

        // Wait for the leader to be elected
        let initialLeader =
            testCluster |> getNumberOfAppendEntryMessages (fun _ -> true) 2 |> List.head

        // Block current leader
        testCluster.transport.SetBlockedNodes [| initialLeader.nodeId |]

        // Wait for the second leader to be elected
        let reelectedLeader =
            testCluster
            |> getNumberOfAppendEntryMessages (fun li -> li.nodeId <> initialLeader.nodeId) 2
            |> List.head

        Assert.IsTrue(reelectedLeader.electionTerm > initialLeader.electionTerm)
        
        let followerNodes = TestCluster.getFollowerNodes testCluster
        Assert.AreEqual(testCluster.nodes.Length - 2, followerNodes.Length)

        Assert.IsTrue(followerNodes |> checkIfFollowersConnectedToLeader reelectedLeader)

        // Old leader becomes follower after restoring communication
        testCluster.transport.SetBlockedNodes [||]

        // Wait for current leader to acknowledge all nodes
        testCluster
        |> getNumberOfAppendEntryMessages (fun li -> li.nodeId <> initialLeader.nodeId) 2
        |> ignore

        // All followers connected to new leader
        let followerNodes2 = TestCluster.getFollowerNodes testCluster

        Assert.AreEqual(testCluster.nodes.Length - 1, followerNodes2.Length)
        // if algorithm ever needed more than 6 terms to elect new leader, I want to be informed to investigate such situation
        Assert.Less(reelectedLeader.electionTerm, 6)

        Assert.True(followerNodes2 |> checkIfFollowersConnectedToLeader reelectedLeader)
