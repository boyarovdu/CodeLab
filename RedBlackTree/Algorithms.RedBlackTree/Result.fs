module Algorithms.RedBlackTree_FSharp.Result

type Result<'T> =
    | ToDo of 'T
    | Done of 'T

// Computation expression builder
type ResultBuilder() =
    member _.Return(value: 'T) : Result<'T> = Done value

    member _.ReturnFrom(job: Result<'T>) : Result<'T> = job

    member _.Bind(job: Result<'T>, binder: 'T -> Result<'T>) : Result<'T> =
        match job with
        | ToDo value -> binder value
        | Done value -> Done value

let job = ResultBuilder()

let bind (binder) job' = job.Bind(job', binder)

let map (mapper) job' =
    match job' with
    | ToDo v -> v |> mapper |> ToDo
    | Done v -> v |> mapper |> Done

let fromResult (job: Result<'T>) : 'T =
    match job with
    | Done v
    | ToDo v -> v
