module PointFree.LeetCode.Tests_XUnit.Problem_1550

open FsCheck.FSharp
open FsCheck.Xunit

open PointFree.LeetCode
open PointFree.LeetCode_CSharp

type NotEmptyPositiveIntsArray =
    
    static member Int() =
        ArbMap.defaults
        |> ArbMap.arbitrary<int>
        |> Arb.mapFilter id (fun t -> t >= 1 && t <= 1000)
    
    static member Array() =
        NotEmptyPositiveIntsArray.Int().Generator
        |> Gen.arrayOf
        |> Gen.filter (fun arr -> arr.Length >= 3 && arr.Length <= 1000)
        |> Arb.fromGen

[<Property(Arbitrary = [| typeof<NotEmptyPositiveIntsArray> |])>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_1550()
    
    let r1 = s.ThreeConsecutiveOdds(array)
    let r2 = Problem_1550.threeConsecutiveOdds (array |> Array.toList)
    
    r1 = r2 
    
