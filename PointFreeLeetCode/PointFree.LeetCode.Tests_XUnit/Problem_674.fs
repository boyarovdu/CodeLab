module PointFree.LeetCode.Tests_XUnit.Problem_674

open System
open FsCheck.Xunit
open FsCheck.FSharp
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

type NonEmptyArray =
    static member Generate() =
        ArbMap.defaults
        |> ArbMap.arbitrary<int array>
        |> Arb.mapFilter id (fun arr -> arr.Length > 0)

[<Property( Arbitrary=[| typeof<NonEmptyArray> |] )>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_674()
    
    s.FindLengthOfLCIS(array) = Problem_674.findLengthOfLCIS array
    