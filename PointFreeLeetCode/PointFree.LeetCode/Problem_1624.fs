namespace PointFree.LeetCode

// 1624. Largest Substring Between Two Equal Characters
// Problem Link (Practice): https://leetcode.com/problems/largest-substring-between-two-equal-characters/description/
module Problem_1624 =

    let starling' = Combinators.S' // Phoenix bird
    let flip = Combinators.C // Cardinal Bird

    let maxLengthBetweenEqualCharacters: (string -> int) =
        Seq.toList
        >> List.subsequences
        >> List.tail
        >> List.filter (starling' (=) List.head List.last)
        >> List.map (List.length >> flip (-) 2)
        >> List.max
