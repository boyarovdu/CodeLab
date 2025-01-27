module PointFree.LeetCode.Tests_XUnit.Problem_1365

open FsCheck
open FsCheck.FSharp
open FsCheck.Xunit
open PointFree.LeetCode

type AtLeastTwoRangedElementsArray =
    
    static member Int() =
        ArbMap.defaults
        |> ArbMap.arbitrary<int>
        |> Arb.mapFilter abs (fun t -> t >= 0 && t <= 100)
    
    static member Array() =
        AtLeastTwoRangedElementsArray.Int().Generator
        |> Gen.arrayOf
        |> Gen.filter (fun arr -> arr.Length >= 2)
        |> Arb.fromGen

[<Property(Arbitrary = [| typeof<AtLeastTwoRangedElementsArray> |])>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_1365()
        
    s.SmallerNumbersThanCurrent(array) = (Problem_1365.smallerNumbersThanCurrent (Array.toList array) |> List.toArray) 
    
