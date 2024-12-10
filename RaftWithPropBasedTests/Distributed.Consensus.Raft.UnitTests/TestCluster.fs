namespace Distributed.Consensus.Raft.UnitTests

open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection
open Microsoft.FSharp.Control
open FSharpx.Control.Observable

type TestClusterType =
    { nodes: RaftNode array
      transport: FakeAsyncTransport
      messagesStream: IObservable<NodeId * RaftMessage> }

module TestCluster =
    let startCluster (clusterSize) =

        let nodes =
            [| 1..clusterSize|]
            |> Array.map (fun nodeId -> new RaftNode(nodeId.ToString(), clusterSize))

        let transport = FakeAsyncTransport(nodes)

        let combinedMessagesStream =
            nodes.[1..]
            |> Array.fold
                (fun messagesCollector node -> node.MessagesStream |> Observable.merge messagesCollector)
                nodes.[0].MessagesStream

        nodes |> Array.iter _.Start()

        { nodes = nodes
          transport = transport
          messagesStream = combinedMessagesStream }

    let getLeaderNodes cluster =
        cluster.nodes
        |> Array.fold
            (fun collector node ->
                match node.GetState() with
                | NodeState.Leader li -> li :: collector
                | _ -> collector)
            []

    let getFollowerNodes cluster =
        cluster.nodes
        |> Array.fold
            (fun collector node ->
                match node.GetState() with
                | NodeState.Follower li -> li :: collector
                | _ -> collector)
            []

    let waitAppendEntryMessageAsync predicate cluster =
        cluster.messagesStream
        |> Observable.scan
            (fun (collector: LeaderInfo list) (_, message) ->
                match message with
                | LeaderElection(AppendEntry li) -> if predicate li then li :: collector else collector
                | _ -> collector)
            []
        |> Observable.filter (fun x -> x.Length > 0)
        |> Async.AwaitObservable
