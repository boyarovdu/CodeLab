namespace PointFree.LeetCode

// Max Consecutive Ones
// Problem Link (Practice): https://leetcode.com/problems/max-consecutive-ones/
module Problem_485 =

    let consequent element list =
        match list with
        | (head' :: tail) when element = fst head' -> (element, (element :: snd head')) :: tail
        | _ -> (element, [ element ]) :: list

    // Solution 1
    let maxConsecutiveOnes1 (nums: int array) : int =
        List.Empty
        |> Array.foldBack consequent nums
        |> List.filter (fst >> (=) 1)
        |> List.map (snd >> List.length)
        |> List.fold max 0

    // Solution 2
    let maxConsecutiveOnes2 (nums: int array) : int =
        List.Empty
        |> Array.foldBack consequent nums
        |> List.map (snd >> List.sum)
        |> List.fold max 0
