module PointFree.LeetCode.Tests_XUnit.Problem_905

open FsCheck.FSharp
open FsCheck.Xunit
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

[<Property>]
let ``Implementation is correct`` (array: int array) =
    let s = Problem_905()
   
    s.SortArrayByParity array = Problem_905.sortArrayByParity array
