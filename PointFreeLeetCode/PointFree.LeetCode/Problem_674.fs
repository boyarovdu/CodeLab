namespace PointFree.LeetCode

open System

// 674. Longest Continuous Increasing Subsequence
// Problem Link (Practice): https://leetcode.com/problems/longest-continuous-increasing-subsequence
module Problem_674 =

    /// Tracks length of specified "trend" in a sequence of values based on a comparison function
    let trend<'T when 'T: comparison> compare (prev: 'T, memo: int list) next =
        if compare next prev then
            (next, memo.Head + 1 :: memo.Tail)
        else
            (next, 1 :: memo)

    let findLengthOfLCIS: int array -> int =
        Array.fold (trend (>)) (Int32.MinValue, [ 0 ])
        >> snd
        >> List.max
       