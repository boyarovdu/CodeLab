namespace Distributed.Consensus.Raft.Tests

open NUnit.Framework
open Distributed.Consensus.Raft
open System

module testUtil =
    // Utility to wait until a condition is satisfied or a timeout occurs
    let waitUntil<'T> (timeoutMs: int) (condition: unit -> bool) =
        let sw = System.Diagnostics.Stopwatch.StartNew()

        let rec waitUntil (condition: unit -> bool) =
            async {
                do! Async.Sleep 50

                match sw.ElapsedMilliseconds > int64 timeoutMs, condition () with
                | true, _ -> return false
                | _, true -> return true
                | _ -> return! waitUntil condition
            }

        waitUntil (condition)

[<TestFixture>]
type RaftNodeIntegrationTests() =



    [<Test>]
    member _.ClusterElectsLeader() =
        async {
            // Set up a cluster of 3 nodes
            let cluster = TestCluster.startCluster 3

            // Wait for a leader to be elected
            let! leaderElected =
                testUtil.waitUntil 5000 (fun () ->
                    cluster.nodes
                    |> Array.exists (fun n ->
                        match n.GetState().role with
                        | Leader -> true
                        | _ -> false))

            let leaders =
                cluster.nodes
                |> Array.filter (fun n ->
                    match n.GetState().role with
                    | Leader -> true
                    | _ -> false)

            Assert.IsTrue(leaderElected, "A leader should be elected in the cluster within the timeout.")
            Assert.AreEqual(1, leaders.Length, "Only one leader should be elected in the cluster.")
            Assert.IsTrue(leaders.[0].GetState().electionTerm < 3, "Leader elected in less than 3 terms")
        }

    [<Test>]
    member _.FollowersRespondToLeaderHeartbeats() =
        async {
            // Set up a cluster of 3 nodes
            let cluster = TestCluster.startCluster 3

            // Wait for a leader to emerge
            let! _ =
                testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.exists (fun n ->
                        match n.GetState().role with
                        | Leader -> true
                        | _ -> false))

            let actualLeader =
                cluster.nodes
                |> Array.find (fun n ->
                    match n.GetState().role with
                    | Leader -> true
                    | _ -> false)

            let! heartbeatReceived =
                testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.filter (fun n -> n.Id <> actualLeader.Id) // Exclude the leader
                    |> Array.forall (fun n ->
                        match n.GetState().role with
                        | Follower { leader = Some leaderId } -> actualLeader.Id = leaderId
                        | _ -> false))

            Assert.IsTrue(heartbeatReceived, "Followers should receive the leader's heartbeats.")
        }

    [<Test>]
    member _.ClusterHandlesLeaderFailure() =
        async {
            // Set up a cluster of 3 nodes
            let cluster = TestCluster.startCluster 3

            // Wait for a leader to emerge
            let! _ =
                testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.exists (fun n ->
                        match n.GetState().role with
                        | Leader -> true
                        | _ -> false))

            let initialLeader =
                cluster.nodes
                |> Array.find (fun n ->
                    match n.GetState().role with
                    | Leader -> true
                    | _ -> false)

            // Simulate leader failure by blocking the leader node
            cluster.transport.SetBlockedNodes [| initialLeader.Id |]

            // Wait for a new leader to emerge
            let! newLeaderElected =
                testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.filter (fun n -> n.Id <> initialLeader.Id) // Exclude failed leader
                    |> Array.exists (fun n ->
                        match n.GetState().role with
                        | Leader -> true
                        | _ -> false))

            let newLeader =
                cluster.nodes
                |> Array.find (fun n ->
                    match n.GetState().role with
                    | Leader -> n.Id <> initialLeader.Id
                    | _ -> false)

            let! heartbeatFromNewLeaderReceived =
                testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.filter (fun n -> [| initialLeader.Id; newLeader.Id |] |> Array.contains n.Id |> not) 
                    |> Array.forall (fun n ->
                        match n.GetState().role with
                        | Follower { leader = Some leaderId } -> newLeader.Id = leaderId
                        | _ -> false))

            Assert.IsTrue(heartbeatFromNewLeaderReceived, "Followers should receive the leader's heartbeats.")
            Assert.IsTrue(newLeaderElected, "The cluster should elect a new leader after the original leader fails.")
        }

    [<Test>]
    member _.NetworkPartitionPreventsLeadership() = async {
            // Set up a cluster of 5 nodes
            let cluster = TestCluster.startCluster 5

            // Partition the network (block quorum)
            let partitionedNodes = cluster.nodes.[0..2] |> Array.map (_.Id)
            cluster.transport.SetBlockedNodes partitionedNodes

            // Wait to confirm no new leader is elected
            let! leaderElected =
                (testUtil.waitUntil 5000 (fun _ ->
                    cluster.nodes
                    |> Array.filter (fun n -> not (Array.contains n.Id partitionedNodes))
                    |> Array.exists (fun n ->
                        match n.GetState().role with
                        | Leader -> true
                        | _ -> false)))

            Assert.IsFalse(leaderElected, "No leader should be elected when quorum is unavailable.")
        }

    [<Test>]
    member _.ClusterRecoversAfterPartition() = async {
        // Set up a cluster of 5 nodes
        let cluster = TestCluster.startCluster 5

        // Wait for a leader to emerge
        let! leaderelected =
            testUtil.waitUntil 5000 (fun _ ->
                cluster.nodes
                |> Array.exists (fun n ->
                    match n.GetState().role with
                    | Leader -> true
                    | _ -> false))

        let initialLeader =
            cluster.nodes
            |> Array.find (fun n ->
                match n.GetState().role with
                | Leader -> true
                | _ -> false)
        

        // Partition the network (block quorum)
        let partitionedNodes = cluster.nodes.[0..2] |> Array.map (fun n -> n.Id)
        cluster.transport.SetBlockedNodes partitionedNodes

        // Wait and confirm no leader is elected
        let! leaderDuringPartition =
            testUtil.waitUntil 5000 (fun _ ->
                cluster.nodes
                |> Array.exists (fun n ->
                    match n.GetState().role with
                    | Leader -> n.Id <> initialLeader.Id
                    | _ -> false))

        Assert.IsFalse(leaderDuringPartition, "No leader should be elected during partition.")

        // Heal the network
        cluster.transport.SetBlockedNodes [||]

        // Wait for a new leader to emerge
        let! newLeaderElected =
            testUtil.waitUntil 5000 (fun _ ->
                cluster.nodes
                |> Array.exists (fun n ->
                    match n.GetState().role with
                    | Leader -> true
                    | _ -> false))

        Assert.IsTrue(newLeaderElected, "A new leader should be elected after partition is healed.")
    }