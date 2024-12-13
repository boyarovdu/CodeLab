namespace Distributed.Consensus.Raft

open System
open Distributed.Consensus.Raft
open Microsoft.FSharp.Control

type TestClusterType =
    { nodes: Node array
      transport: FakeAsyncTransport
      messagesStream: IObservable<NodeId * RaftMessage>
      diagnosticLogStream: IObservable<DiagnosticLogEntry> }

module TestCluster =
    let nl = Environment.NewLine

    let consoleMailbox =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (counter: int) =
                async {
                    let! message = inbox.Receive()
                    printfn $"--------------- Log Record %d{counter} ---------------%s{nl}%s{message}%s{nl}"
                    return! loop (counter + 1)
                }

            loop 1)
    
    let startCluster (clusterSize) =

        let nodes =
            [| 1..clusterSize |]
            |> Array.map (fun nodeId -> new Node(nodeId.ToString(), clusterSize))

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

        let _ =
            combinedDiagnosticLogStream
            |> Observable.filter (fun logEntry ->
                logEntry.initialState <> logEntry.finalState)     
            |> Observable.subscribe (fun message ->
                let msg = $"%s{message.ToString()}"
                consoleMailbox.Post(msg))
        
        nodes |> Array.iter _.Start()

        { nodes = nodes
          transport = transport
          messagesStream = combinedMessagesStream
          diagnosticLogStream = combinedDiagnosticLogStream }
