namespace PointFree.LeetCode

// 1550. Three Consecutive Odds
// Problem Link (Practice): https://leetcode.com/problems/three-consecutive-odds/description/
module Problem_1550 =
        
    let on = Combinators.psi // Ψ(psi) combinator
    let flip = Combinators.C // Cardinal Bird 
    
    let odd = flip (%) 2 >> (=) 0

    let threeConsecutiveOdds =
        PointFree.LeetCode.List.groupByAdjacent (on (=) odd) 
        >> List.filter (List.head >> odd >> not)
        >> List.map List.length
        >> List.fold max 0
        >> flip (>=) 3
