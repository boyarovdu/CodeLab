namespace PointFree.LeetCode

open System

// 1394. Find Lucky Integer in an Array
// Problem Link (Practice): https://leetcode.com/problems/find-lucky-integer-in-an-array/description/
module Problem_1394 =
    let uncurry = Combinators.uncurry
    let flip = Combinators.C
        
    let max (a: int) (b: int) = Math.Max(a, b)
        
    let findLucky  =
        Array.countBy id
        >> Array.filter (uncurry (=))
        >> Array.fold (flip (snd >> max)) -1
        
