namespace PointFree.LeetCode

// 485. Max Consecutive Ones
// Problem Link (Practice): https://leetcode.com/problems/max-consecutive-ones/
module Problem_485 =
    
    // Solution 1
    let maxConsecutiveOnes1 =
        PointFree.LeetCode.List.groupConsequent
        >> List.filter (fst >> (=) 1)
        >> List.map (snd >> List.length)
        >> List.fold max 0
    
    // Solution 2
    let maxConsecutiveOnes2 =
        PointFree.LeetCode.List.groupConsequent
        >> List.map (snd >> List.sum)
        >> List.fold max 0
        
    

        
