namespace Distributed.Consensus.Raft.UnitTests

open NUnit.Framework
open Distributed.Consensus.Raft.UnitTests

module LeaderElectionTests =
    
    [<Test>]
    let ``Only one node elected as leader, all other nodes are followers`` () =
        let testCluster = TestCluster.startCluster 3

        // Wait for the leader to be elected
        let appendEntryMessage =
            testCluster
            |> TestCluster.waitAppendEntryMessageAsync (fun li -> true)
            |> Async.RunSynchronously

        let leaderNodes = TestCluster.getLeaderNodes testCluster
        let followerNodes = TestCluster.getFollowerNodes testCluster

        Assert.AreEqual(1, leaderNodes.Length)
        Assert.AreEqual(testCluster.nodes.Length - 1, followerNodes.Length)
        Assert.AreEqual(appendEntryMessage.Head.nodeId, leaderNodes.Head.nodeId)


    [<Test>]
    let ``Cluster re-elects new leader in case old one becomes unavailable`` () =
        let testCluster = TestCluster.startCluster 3

        // Wait for the leader to be elected
        let firstLeaderMessage =
            testCluster
            |> TestCluster.waitAppendEntryMessageAsync (fun _ -> true)
            |> Async.RunSynchronously

        // Block current leader
        testCluster.transport.SetBlockedNodes [| firstLeaderMessage.Head.nodeId |]

        // Wait for the second leader to be elected
        let secondLeaderMessage =
            testCluster
            |> TestCluster.waitAppendEntryMessageAsync (fun li ->
                li.nodeId <> firstLeaderMessage.Head.nodeId
                // second leader must be with greater election term than old one
                && li.electionTerm > firstLeaderMessage.Head.electionTerm)
            |> Async.RunSynchronously
        
        // All followers connected to new leader
        let followerNodesAfterReelection = TestCluster.getFollowerNodes testCluster
        Assert.AreEqual(testCluster.nodes.Length - 2, followerNodesAfterReelection.Length)
        Assert.True(
            followerNodesAfterReelection
            |> List.forall (fun fi ->
                fi.electionTerm = secondLeaderMessage.Head.electionTerm
                && match fi.leader with
                   | Some li -> li.nodeId = secondLeaderMessage.Head.nodeId
                   | None -> false)
        )

        // Old leader becomes follower after restoring communication  
        testCluster.transport.SetBlockedNodes [||]
        Async.Sleep(200) |> Async.RunSynchronously
        
        // All followers connected to new leader
        let followerNodesAfterRestoringOldLeaderConnection = TestCluster.getFollowerNodes testCluster
        Assert.AreEqual(testCluster.nodes.Length - 1, followerNodesAfterRestoringOldLeaderConnection.Length)
        Assert.True(
            followerNodesAfterRestoringOldLeaderConnection
            |> List.forall (fun fi ->
                fi.electionTerm = secondLeaderMessage.Head.electionTerm
                && match fi.leader with
                   | Some li -> li.nodeId = secondLeaderMessage.Head.nodeId
                   | None -> false)
        )

        
        