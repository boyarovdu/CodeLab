module Distributed.Consensus.Raft.Tests.TestUtil

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
