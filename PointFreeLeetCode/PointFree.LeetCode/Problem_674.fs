namespace PointFree.LeetCode

open System

// 674. Longest Continuous Increasing Subsequence
// Problem Link (Practice): https://leetcode.com/problems/longest-continuous-increasing-subsequence
module Problem_674 =

    // Tracks length of specified "trend" in a sequence of values based on a comparison function
    let findLengthOfLCIS: int list -> int =
        List.groupByAdjacent (>)
        >> List.map List.length
        >> List.max
       