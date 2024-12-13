// For more information see https://aka.ms/fsharp-console-apps

open BenchmarkDotNet.Running
open Distributed.Consensus.Raft.Benchmark

printfn "Hello from F#"
BenchmarkRunner.Run<NodeBenchmark>() |> ignore