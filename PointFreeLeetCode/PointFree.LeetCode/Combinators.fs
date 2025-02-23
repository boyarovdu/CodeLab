module PointFree.LeetCode.Combinators

let curry f a b = f (a,b)
let uncurry f (a, b) = f a b
let psi f g x y = f (g x) (g y) // Psi combinator

(* ----------------------------------------------------------------------------
 ---- The following source code was copied from -------------------------------
 ---- https://gist.github.com/Happypig375/8adac33d210fc8dd3d36d847d1b855a3 ----
 ------------------------------------------------------------------------------ *)

// Code                    // Usual function name  // "Bird"
let I x = x                // id (FSharp.Core)     // Identity bird, or the Idiot bird
let K x y = x              // konst (FSharpPlus)   // the Kestrel
let M x y = x (x y)        // twice                // the Mockingbird
let T x y = y x            // |> (FSharp.Core)     // the Thrush
let Q x y z = y (x z)      // >> (FSharp.Core)     // the Queer bird
let S x y z = x z (y z)    // apply (applicative)  // the Starling
let rec Y f x = f (Y f) x  // fix (FSharpx.Extras) // the Sage bird

// More combinators not mentioned in F# for Fun and Profit
// https://www.angelfire.com/tx4/cus/combinator/birds.html
// https://hackage.haskell.org/package/data-aviary-0.4.0/docs/Data-Aviary-Birds.html
let A x y = x y            // <| (FSharp.Core)
let ``I*`` x y = x y       // <| (FSharp.Core)     // Identity Bird Once Removed
let B x y z = x (y z)      // << (FSharp.Core)     // Bluebird
let C x y z = x z y        // flip (FSharpPlus)    // Cardinal
let W x y = x y y          // join (monad)         // Warbler

// All birds: https://hackage.haskell.org/package/data-aviary-0.4.0/docs/src/Data-Aviary-Birds.html