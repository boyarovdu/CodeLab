module PointFree.LeetCode.List

// Function similar to Haskel's groupBy from Data.List
let groupByAdjacent predicate (list: 'T list) : 'T list list =
    let folder predicate list element =
        match list with
        | [] -> [ [ element ] ]
        | group :: tail when predicate element (List.head group) -> (element :: group) :: tail
        | _ -> [ element ] :: list
    
    list |> List.fold (folder predicate) []

// Function similar to Haskel's group from List
let groupConsequent list =
    let folder element list =
        match list with
        | (head' :: tail) when element = fst head' -> (element, (element :: snd head')) :: tail
        | _ -> (element, [ element ]) :: list
        
    List.foldBack folder list []
    
let rec subsequences (list: 'a list) : 'a list list =
    match list with
    | [] -> [[]] 
    | head :: tail ->
        let rest = subsequences tail
        rest @ List.map (fun subSeq -> head :: subSeq) rest
