namespace PointFree.LeetCode

// 1365. How Many Numbers Are Smaller Than the Current Number
// Problem Link (Practice): https://leetcode.com/problems/how-many-numbers-are-smaller-than-the-current-number/description/
module Problem_1365 =
    let smallerNumbersThanCurrent (x: int list) : int list =
        let outerProduct f xs ys =
            [ for a in xs do
                for b in ys do
                    yield f a b ]
            |> List.chunkBySize (List.length ys)
        x
        |> outerProduct (>) x
        |> List.map (List.filter id >> List.length)
        
        
