namespace PointFree.LeetCode

// 1365. How Many Numbers Are Smaller Than the Current Number
// Problem Link (Practice): https://leetcode.com/problems/how-many-numbers-are-smaller-than-the-current-number/description/
module Problem_1365 =
    let join = Combinators.W // Warbler combinator
        
    let smallerNumbersThanCurrent(x: int array) : int array =   
        x
        |> join Array.allPairs
        |> Array.map(Combinators.uncurry (>))
        |> Array.chunkBySize (Array.length x)
        |> Array.map (Array.filter id >> Array.length)
        
