module Example

open FsCheck.FSharp
open FsCheck.Xunit
  
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
    static member Double() =
        0.0
        |> Gen.constant
        |> Arb.fromGen


[<assembly: Properties( Arbitrary = [| typeof<Zero> |] )>] do()
module ModuleWithoutProperties =

    [<Property>]
    let ``should use Arb instances from assembly``(underTest:float) =
        underTest = 0.0

    [<Property( Arbitrary=[| typeof<Positive> |] )>]
    let ``should use Arb instance on method``(underTest:float) =
        underTest > 0.0

[<Properties( Arbitrary=[| typeof<Negative> |] )>]
module ModuleWithProperties =

    [<Property>]
    let ``should use Arb instances from enclosing module``(underTest:float) =
        underTest < 0.0

    [<Property( Arbitrary=[| typeof<Positive> |] )>]
    let ``should use Arb instance on method``(underTest:float) =
        underTest > 0.0
