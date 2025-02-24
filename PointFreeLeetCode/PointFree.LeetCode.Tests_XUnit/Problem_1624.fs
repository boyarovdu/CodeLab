module PointFree.LeetCode.Tests_XUnit.Problem_1624

open System.Text.RegularExpressions
open FsCheck.FSharp
open FsCheck.Xunit

open PointFree.LeetCode
open PointFree.LeetCode_CSharp

type NotEmptyLowerCaseString =
    
    static member String() =
        ArbMap.defaults
        |> ArbMap.arbitrary<string>
        |> Arb.mapFilter (_.ToLower()) (fun s -> Regex.IsMatch(s, "^[a-z]+$") && s.Length >= 1 && s.Length <= 300)
    


[<Property(Arbitrary = [| typeof<NotEmptyLowerCaseString> |])>]
let ``Implementation is correct`` (string) =
    let s = Problem_1624()
    
    let r1 = s.MaxLengthBetweenEqualCharacters(string)
    let r2 = Problem_1624.maxLengthBetweenEqualCharacters string
    
    r1 = r2 
    
