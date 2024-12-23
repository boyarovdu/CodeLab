module RedBlackTree_FSharp.RedBlackTree

open Algorithms.RedBlackTree_FSharp
open Algorithms.RedBlackTree_FSharp.Job

type Color =
    | Red
    | Black

type Tree<'T> =
    | NilNode
    | Node of Color * Tree<'T> * 'T * Tree<'T>

let blacken tree =
    match tree with
    | Node(Red, left, x, right) -> Node(Black, left, x, right)
    | _ -> tree

let insert tree x =

    let rec ins t =
        match t with
        | NilNode -> ToDo(Node(Red, NilNode, x, NilNode))
        | Node(color, left, y, right) ->
            match (x, y) with
            | Left ->
                left |> ins |> map (fun left' -> Node(color, left', y, right)) |> bind balance
            | Right ->
                right |> ins |> map (fun right' -> Node(color, left, y, right')) |> bind balance
            | None -> Done t

    and balance t =
        match t with
        | Node(Black, Node(Red, Node(Red, a, x, b), y, c), z, d) // Case 1
        | Node(Black, Node(Red, a, x, Node(Red, b, y, c)), z, d) // Case 2
        | Node(Black, a, x, Node(Red, Node(Red, b, y, c), z, d)) // Case 3
        | Node(Black, a, x, Node(Red, b, y, Node(Red, c, z, d))) -> // Case 4
            ToDo(Node(Red, Node(Black, a, x, b), y, Node(Black, c, z, d)))
        | _ -> Done t

    and (|Left|Right|None|) (x, y) =
        if x < y then Left
        elif x > y then Right
        else None

    tree |> ins |> fromResult |> blacken


let rec delete tree x =
    let rec del t =
        match t with
        | NilNode -> Done NilNode
        | Node(color, left, y, right) ->
            if x < y then
                left |> del |> Job.map (fun l -> Node(color, l, y, right)) |> Job.bind delLeft
            elif x > y then
                right |> del |> Job.map (fun r -> Node(color, left, y, r)) |> Job.bind delRight
            else
                delRoot t
    
    // and delRoot t =
    //     match t with
    //     | Node(Black, left, _, NilNode) -> blacken left |> Done
    //     | Node(Red, left, _, NilNode) -> Done left
    //     | Node(color, left, _, right) ->
    //         // Refactor to remove `mutable ref` usage
    //         delMin right |> map (fun (minValue, newRight) -> Node(color, left, minValue, newRight)) |> bind delRight
    //
    // and delMin t =
    //     match t with
    //     | Node(Black, NilNode, y, right) -> Done (y, blacken right)
    //     | Node(Red, NilNode, y, right) -> Done (y, right)
    //     | Node(color, left, y, right) ->
    //         delMin left |> map (fun (minValue, newLeft) -> (minValue, Node(color, newLeft, y, right))) |> bind delLeft
    //
    // and delLeft t =
    //     match t with
    //     | Node(Black, Node(Red, left, x, right), y, sibling) -> ToDo(Node(Red, Node(Black, left, x, right), y, sibling))
    //     | Node(color, left, y, sibling) -> balance (Node(color, left, y, sibling)) |> Done
    //     | _ -> Done t
    //
    // and delRight t =
    //     match t with
    //     | Node(Black, sibling, y, Node(Red, left, x, right)) -> ToDo(Node(Red, sibling, y, Node(Black, left, x, right)))
    //     | Node(color, sibling, y, right) -> balance (Node(color, sibling, y, right)) |> Done
    //     | _ -> Done t
    
    and delRoot t =
        match t with
        | Node(Black, left, _, NilNode) -> blacken left |> Done
        | Node(Red, left, _, NilNode) -> Done left
        | Node(color, left, _, right) ->
            let m = ref None // Mutable reference to hold the min value
    
            m
            |> delMin right
            |> map (fun r -> Node(color, left, (m.Value).Value, r))
            |> bind delRight
     
    and delMin t m =
        match t with
        | Node(Black, NilNode, y, right) ->
            m.Value <- Some y
            blacken right |> Done
        | Node(Red, NilNode, y, right) ->
            m.Value <- Some y
            Done right
        | Node(color, left, y, right) ->
            m
            |> delMin left
            |> map (fun l -> Node(color, l, y, right))
            |> bind delLeft
    
    and delLeft t =
        match t with
        | Node(Black, Node(Red, left, x, right), y, sibling) -> ToDo(Node(Red, Node(Black, left, x, right), y, sibling))
        | Node(color, left, y, sibling) -> balance (Node(color, left, y, sibling)) |> Done
        | _ -> Done t
    
    and delRight t =
        match t with
        | Node(Black, sibling, y, Node(Red, left, x, right)) -> ToDo(Node(Red, sibling, y, Node(Black, left, x, right)))
        | Node(color, sibling, y, right) -> balance (Node(color, sibling, y, right)) |> Done
        | _ -> Done t

    and balance t =
        let (node) =

            match t with
            | Node(color, Node(Red, Node(Red, a, x, b), y, c), z, d) // Symmetric balancing cases
            | Node(color, Node(Red, a, x, Node(Red, b, y, c)), z, d)
            | Node(color, a, x, Node(Red, Node(Red, b, y, c), z, d))
            | Node(color, a, x, Node(Red, b, y, Node(Red, c, z, d))) ->
                (Node(color, Node(Black, a, x, b), y, Node(Black, c, z, d))) |> Done
            | _ -> blacken t |> Done

        fromResult node

    blacken (fromResult (del tree))
