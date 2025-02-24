module Example

open FsCheck
open FsCheck.FSharp
open FsCheck.Xunit


type Tree =
    | Leaf of int
    | Branch of Tree * Tree

let tree =
    let rec tree' s =
        match s with
        | 0 -> ArbMap.defaults |> ArbMap.generate<int> |> Gen.map Leaf
        | n when n > 0 ->
            let subtree = tree' (n / 2)

            Gen.oneof
                [ ArbMap.defaults |> ArbMap.generate<int> |> Gen.map Leaf
                  Gen.map2 (fun x y -> Branch(x, y)) subtree subtree ]
        | _ -> invalidArg "s" "Only positive arguments are allowed"

    Gen.sized tree'

type MyGenerators =
    static member Tree() =
        { new Arbitrary<Tree>() with
            override _.Generator = tree
            override _.Shrinker _ = Seq.empty }



type Positive =
    static member Double() =
        ArbMap.defaults
        |> ArbMap.arbitrary<float>
        |> Arb.mapFilter abs (fun t -> t > 0.0)

type Negative =
    static member Double() =
        ArbMap.defaults
        |> ArbMap.arbitrary<float>
        |> Arb.mapFilter (abs >> ((-) 0.0)) (fun t -> t < 0.0)

type Zero =
    static member Double() = 0.0 |> Gen.constant |> Arb.fromGen


[<assembly: Properties(Arbitrary = [| typeof<Zero> |])>]
do ()

module ModuleWithoutProperties =

    [<Property>]
    let ``should use Arb instances from assembly`` (underTest: float) = underTest = 0.0

    [<Property(Arbitrary = [| typeof<Positive> |])>]
    let ``should use Arb instance on method`` (underTest: float) = underTest > 0.0

[<Properties(Arbitrary = [| typeof<Negative> |])>]
module ModuleWithProperties =

    [<Property>]
    let ``should use Arb instances from enclosing module`` (underTest: float) = underTest < 0.0

    [<Property(Arbitrary = [| typeof<Positive> |])>]
    let ``should use Arb instance on method`` (underTest: float) = underTest > 0.0
