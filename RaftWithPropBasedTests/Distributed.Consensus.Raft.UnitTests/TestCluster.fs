namespace Distributed.Consensus.Raft.UnitTests

open System
open Distributed.Consensus.Raft
open Distributed.Consensus.Raft.LeaderElection
open Microsoft.FSharp.Control


type TestClusterType =
    { nodes: RaftNode array
      transport: FakeAsyncTransport
      messagesStream: IObservable<NodeId * RaftMessage>
      diagnosticLogStream: IObservable<DiagnosticLogEntry> }

module TestCluster =
    let startCluster (clusterSize) =

        let nodes =
            [| 1..clusterSize |]
            |> Array.map (fun nodeId -> new RaftNode(nodeId.ToString(), clusterSize))

        let transport = FakeAsyncTransport(nodes)

        let combinedMessagesStream =
            nodes.[1..]
            |> Array.fold
                (fun messagesCollector node -> node.MessagesStream |> Observable.merge messagesCollector)
                nodes.[0].MessagesStream
                
        let combinedDiagnosticLogStream =
            nodes.[1..]
            |> Array.fold
                (fun messagesCollector node -> node.DiagnosticLogStream |> Observable.merge messagesCollector)
                nodes.[0].DiagnosticLogStream

        nodes |> Array.iter _.Start()

        { nodes = nodes
          transport = transport
          messagesStream = combinedMessagesStream
          diagnosticLogStream = combinedDiagnosticLogStream }

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

    let waitNumberOfAppendEntryMessagesAsync predicate numberOfMessages cluster =
        cluster.messagesStream
        |> Observable.scan (fun (collector: LeaderInfo list) (_, message) ->
            match message with
            | LeaderElection(AppendEntry li) ->
                if predicate li then li :: collector
                else collector
            | _ -> collector) []
        |> Observable.filter (fun messages -> messages.Length >= numberOfMessages)
    
    let waitAppendEntryMessageAsync predicate cluster =
        cluster.messagesStream
        |> Observable.choose (fun (_, message) ->
            match message with
            | LeaderElection(AppendEntry li) -> if predicate li then Some li else None
            | _ -> None)
