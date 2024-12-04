namespace Distributed.Consensus.Raft

open System
open Distributed.Consensus.Raft

type FakeAsyncTransport(nodes: Node array) =

    let nl = Environment.NewLine
    let timeFormat = "hh:mm:ss.fff"

    let blockedNodesLock = obj ()
    let mutable blockedNodeIds = [||]

    // As nodes communicate asynchronously, I do not want them to fight for the console pointer, so I use a mailbox
    // processor to write messages to the console in the order they were sent
    let consoleMailbox =
        MailboxProcessor.Start(fun inbox ->
            let rec loop (counter: int) =
                async {
                    let! message = inbox.Receive()
                    let time = DateTime.Now.ToString timeFormat
                    printfn $"----- Operation %d{counter} [%s{time}] -----%s{nl}%s{message}"
                    return! loop (counter + 1)
                }

            loop 1)

    // Fakes asynchronous message delivery
    let sendMessageAsync (message: Message) =
        async {
            nodes
            |> Array.filter (fun node -> Array.contains node.Id message.recipients)
            |> Array.iter (fun recipientNode ->
                if
                    not (
                        Array.contains message.senderId blockedNodeIds
                        || Array.contains recipientNode.Id blockedNodeIds
                    )
                then
                    consoleMailbox.Post
                        $"Sending message from node %s{message.senderId} to node %s{recipientNode.Id}:%s{nl}%s{nl}\"%s{message.ToString()}\"%s{nl}"

                    recipientNode.PostMessage message)

            lock blockedNodesLock (fun () -> ())

        }

    do
        for node in nodes do
            node.MessagesStream.Add(fun message -> sendMessageAsync message |> Async.Start)

    member x.SetBlockedNodes bn =
        lock blockedNodesLock (fun () -> blockedNodeIds <- bn)
