namespace PointFree.LeetCode

// 1550. Three Consecutive Odds
// Problem Link (Practice): https://leetcode.com/problems/three-consecutive-odds/description/
module Problem_1550 =
        
    let on = Combinators.psi // Ψ(psi) combinator
    let flip = Combinators.C // Cardinal Bird 
    
    let odd = flip (%) 2 >> (=) 0
    
    // Function similar to Data.List (groupBy)
    let groupByAdjacent predicate (list: 'T list) : 'T list list =
        list
        |> List.fold (fun acc x ->
            match acc with
            | [] -> [[x]]
            | group :: tail when predicate x (List.head group) -> (x :: group) :: tail
            | _ -> [x] :: acc
        ) []

    let threeConsecutiveOdds =
        groupByAdjacent (on (=) odd) 
        >> List.filter (List.head >> odd >> not)
        >> List.map List.length
        >> List.fold max 0
        >> flip (>=) 3
