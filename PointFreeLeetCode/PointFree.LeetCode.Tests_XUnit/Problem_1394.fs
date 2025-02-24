module PointFree.LeetCode.Tests_XUnit.Problem_1394

open FsCheck.FSharp
open FsCheck.Xunit

open PointFree.LeetCode
open PointFree.LeetCode_CSharp

type NotEmptyPositiveIntsArray =
    
    static member Int() =
        ArbMap.defaults
        |> ArbMap.arbitrary<int>
        |> Arb.mapFilter id (fun t -> t >= 1 && t <= 500)
    
    static member Array() =
        NotEmptyPositiveIntsArray.Int().Generator
        |> Gen.arrayOf
        |> Gen.filter (fun arr -> arr.Length >= 1 && arr.Length <= 500)
        |> Arb.fromGen

[<Property(Arbitrary = [| typeof<NotEmptyPositiveIntsArray> |])>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_1394()
    
    s.FindLucky(array) = Problem_1394.findLucky array 
    
