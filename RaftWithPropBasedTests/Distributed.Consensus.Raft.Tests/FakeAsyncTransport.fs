namespace Distributed.Consensus.Raft

open System.Threading
open System.Threading.Tasks
open Distributed.Consensus.Raft

type FakeAsyncTransport(nodes: Node array) =

    let rwLock = new ReaderWriterLockSlim()
    let mutable blockedNodeIds = [||]

    // Fakes asynchronous message delivery
    let sendMessage (senderId: NodeId, message: RaftMessage) =
        rwLock.EnterReadLock()

        try
            if not (Array.contains senderId blockedNodeIds) then
                MessageRouter.getRecipients nodes message
                |> Array.iter (fun recipientNode ->
                    if not (Array.contains recipientNode.Id blockedNodeIds) then
                        recipientNode.ProcessMessage message)
        finally
            rwLock.ExitReadLock()

    do
        for node in nodes do                       
            node.MessagesStream.Add(fun messageDetails ->
                let task = new Task(fun () -> sendMessage messageDetails)
                task.Start())

    member x.SetBlockedNodes bn =
        rwLock.EnterWriteLock()

        try
            blockedNodeIds <- bn
        finally
            rwLock.ExitWriteLock()
