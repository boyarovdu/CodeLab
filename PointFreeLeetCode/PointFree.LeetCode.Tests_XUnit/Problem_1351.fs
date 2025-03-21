﻿module PointFree.LeetCode.Tests_XUnit.Problem_1351

open FsCheck
open FsCheck.FSharp
open FsCheck.Xunit
open PointFree.LeetCode_CSharp
open PointFree.LeetCode

let matrixGen =
    gen {
        let! rowCount = Gen.sized (fun size -> Gen.choose (1, max 1 size))
        let! rowLength = Gen.choose (1, 100)
        let firstEl (arr: 'T array) = arr.[0]

        let row =
            ArbMap.defaults
            |> ArbMap.arbitrary<int>
            |> Arb.filter(fun el -> el >= -100 && el <= 100)
            |> Arb.toGen
            |> Gen.arrayOfLength rowLength
            |> Gen.map Array.sortDescending

        return! Gen.arrayOfLength rowCount row |> Gen.map (Array.sortBy firstEl)
    }

type sortedMatrix =
    static member Array() =
        { new Arbitrary<int array array>() with
            override _.Generator = matrixGen
            override _.Shrinker _ = Seq.empty }


[<Property(Arbitrary = [| typeof<sortedMatrix> |])>]
let ``Implementation is correct`` (array: int array array) =
    let s = Problem_1351()
    s.CountNegatives array = Problem_1351.countNegatives array
