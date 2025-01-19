namespace PointFree.LeetCode

open System

// 674. Longest Continuous Increasing Subsequence
// Problem Link (Practice): https://leetcode.com/problems/longest-continuous-increasing-subsequence
module Problem_674 =

    let trend<'T> compare (prev: 'T, memo: int list) next  =
        if compare next prev then
            (next, memo.Head + 1 :: memo.Tail)
        else
            (next, 1 :: memo)


    let findLengthOfLCIS (nums: int array) : int =
        nums
        |> Array.fold (trend (>)) (Int32.MinValue, [ 0 ])
        |> snd
        |> List.max
