namespace PointFree.LeetCode

open PointFree.LeetCode.Combinators

// 532. K-diff Pairs in an Array
// Problem Link (Practice): https://leetcode.com/problems/k-diff-pairs-in-an-array/description/
module Problem_532 =
    let join = W // Warbler combinator
    
    let findPairs (nums: int array) (n: int) : int =
        nums
        |> Array.distinct
        |> join Array.allPairs
        |> Array.map (uncurry (-))
        |> Array.filter ((=) n)
        |> Array.length
