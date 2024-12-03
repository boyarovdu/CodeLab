namespace Distributed.Consensus.Raft

open System
open Distributed.Consensus.Raft

type MessageBus(nodes: Node array) =
    
    let nl = Environment.NewLine
    let timeFormat = "hh:mm:ss.fff"
    
    let consoleMailbox = MailboxProcessor.Start(fun inbox ->
        let rec loop (counter:int) = async {
            let! message = inbox.Receive()
            let time = DateTime.Now.ToString timeFormat
            printfn $"----- Operation %s{counter.ToString()} [%s{time}] -----%s{nl}%s{message}"
            return! loop(counter + 1)
        }
        loop (0)
    )
    
    let sendMessage (message: Message) =
        
        nodes
        |> Array.filter (fun node -> Array.contains node.Id message.recipients)
        |> Array.iter (fun (node:Node) ->
            consoleMailbox.Post $"Sending message from node %s{message.senderId} to node %s{node.Id}:%s{nl}%s{nl}\"%s{message.ToString()}\"%s{nl}"
            node.ProcessMessage message)
    
    do
        for node in nodes do
            node.MessagesStream.Add sendMessage
