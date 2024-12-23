module Algorithms.RedBlackTree_FSharp.Job

type Job<'T> =
    | ToDo of 'T
    | Done of 'T

// Computation expression builder
type JobBuilder() =
    member _.Return(value: 'T) : Job<'T> = Done value

    member _.ReturnFrom(job: Job<'T>) : Job<'T> = job

    member _.Bind(job: Job<'T>, binder: 'T -> Job<'T>) : Job<'T> =
        match job with
        | ToDo value -> binder value
        | Done value -> Done value

let job = JobBuilder()

let bind (binder) job' = job.Bind(job', binder)

let map (mapper) job' =
    match job' with
    | ToDo v -> v |> mapper |> ToDo
    | Done v -> v |> mapper |> Done

let fromResult (job: Job<'T>) : 'T =
    match job with
    | Done v
    | ToDo v -> v
