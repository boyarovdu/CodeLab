open System
open Distributed.Consensus.Raft

open Microsoft.FSharp.Control

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
        ()
    finally
        diagnosticCollector.Dispose()

    0
