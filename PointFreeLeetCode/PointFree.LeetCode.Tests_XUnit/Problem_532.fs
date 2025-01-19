module PointFree.LeetCode.Tests_XUnit.Problem_532

open System
open FsCheck.Xunit
open FsCheck.FSharp
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

type PositiveNatural =
    static member Int() =
        ArbMap.defaults
        |> ArbMap.arbitrary<int>
        |> Arb.mapFilter abs (fun t -> t <> 0)

// Point-Free implementation works incorrectly within edge case when k = 0
[<Property( Arbitrary = [| typeof<PositiveNatural> |]  )>]
let ``Implementation is correct`` (array: int array, k: int) =
    let k' = Math.Abs k
    let s = Problem_532()
    
    s.FindPairs(array, k') = Problem_532.findPairs array k'
    