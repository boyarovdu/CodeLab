module PointFree.LeetCode.Tests_XUnit.Problem_485

open FsCheck.FSharp
open FsCheck.Xunit
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

type Binary =
    static member Int() =
        [| 0; 1 |] |> Gen.elements |> Arb.fromGen

[<Property(Arbitrary = [| typeof<Binary> |])>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_485()
    let r = s.FindMaxConsecutiveOnes array

    [| s.FindMaxConsecutiveOnes_dp array
       Problem_485.maxConsecutiveOnes1 array
       Problem_485.maxConsecutiveOnes2 array |]
    |> Array.forall ((=) r)
