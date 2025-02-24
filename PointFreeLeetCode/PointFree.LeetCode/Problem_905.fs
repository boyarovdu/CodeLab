namespace PointFree.LeetCode

// 905. Sort Array By Parity
// Problem Link (Practice): https://leetcode.com/problems/sort-array-by-parity
module Problem_905 =
    let flip = Combinators.C

    let sortArrayByParity: int array -> int array =
        Array.partition ((flip (%) 2) >> (=) 0)
        >> Combinators.uncurry Array.append
