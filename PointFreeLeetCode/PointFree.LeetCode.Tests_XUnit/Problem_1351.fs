module PointFree.LeetCode.Tests_XUnit.Problem_1351

open FsCheck.Xunit
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

[<Property>]
let ``Implementation is correct`` (array: int array array) =
    let s = Problem_1351()
    s.CountNegatives array = Problem_1351.countNegatives2 array
