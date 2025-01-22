namespace PointFree.LeetCode

// 1351. Count Negative Numbers in a Sorted Matrix
// Problem Link (Practice): https://leetcode.com/problems/count-negative-numbers-in-a-sorted-matrix
module Problem_1351 =
    let flip = Combinators.C
        
    let countNegatives2: int array array -> int =
        Array.concat
        >> Array.filter (fun e -> e < 0 ) // ((<) 0)
        >> Array.length
        
