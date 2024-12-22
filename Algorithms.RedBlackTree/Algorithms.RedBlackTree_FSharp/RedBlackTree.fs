module RedBlackTree_FSharp.RedBlackTree

open Algorithms.RedBlackTree_FSharp

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
        | NilNode -> Job.ToDo(Node(Red, NilNode, x, NilNode))
        | Node(color, left, y, right) ->
            match (x, y) with
            | Left -> left |> ins |> Job.bind (fun left' -> Node(color, left', y, right) |> balance)
            | Right -> right |> ins |> Job.bind (fun right' -> Node(color, left, y, right') |> balance)
            | None -> Job.Done t

    and balance t =
        match t with
        | Node(Black, Node(Red, Node(Red, a, x, b), y, c), z, d) // Case 1
        | Node(Black, Node(Red, a, x, Node(Red, b, y, c)), z, d) // Case 2
        | Node(Black, a, x, Node(Red, Node(Red, b, y, c), z, d)) // Case 3
        | Node(Black, a, x, Node(Red, b, y, Node(Red, c, z, d))) -> // Case 4
            Job.ToDo(Node(Red, Node(Black, a, x, b), y, Node(Black, c, z, d)))
        | _ -> Job.Done t

    and (|Left|Right|None|) (x, y) =
        if x < y then Left
        elif x > y then Right
        else None

    tree |> ins |> Job.fromResult |> blacken

//
//
// let rec delete tree x =
//     let rec del t =
//         match t with
//         | Empty -> done' Empty
//         | Node(color, left, y, right) ->
//             if x < y then
//                 left
//                 |> del
//                 |> (apply (fun l -> Node(color, l, y, right)))
//                 |> applyIfToDo delLeft
//             elif x > y then
//                 right
//                 |> del
//                 |> (apply (fun r -> Node(color, left, y, r)))
//                 |> applyIfToDo delRight
//             else
//                 delRoot t
//
//     and delRoot t =
//         match t with
//         | Node(Black, left, _, Empty) -> blacken left |> done'
//         | Node(Red, left, _, Empty) -> done' left
//         | Node(color, left, _, right) ->
//             let m = ref None // Mutable reference to hold the min value
//
//             m
//             |> delMin right
//             |> (apply (fun r -> Node(color, left, (m.Value).Value, r)))
//             |> applyIfToDo delRight
//
//     and delMin t m =
//         match t with
//         | Node(Black, Empty, y, right) ->
//             m.Value <- Some y
//             blacken right |> done'
//         | Node(Red, Empty, y, right) ->
//             m.Value <- Some y
//             done' right
//         | Node(color, left, y, right) ->
//             m
//             |> delMin left
//             |> (apply (fun l -> Node(color, l, y, right)))
//             |> applyIfToDo delLeft
//
//     and delLeft t =
//         match t with
//         | Node(Black, Node(Red, left, x, right), y, sibling) ->
//             todo (Node(Red, Node(Black, left, x, right), y, sibling))
//         | Node(color, left, y, sibling) -> balance (Node(color, left, y, sibling)) |> done'
//         | _ -> done' t
//
//     and delRight t =
//         match t with
//         | Node(Black, sibling, y, Node(Red, left, x, right)) ->
//             todo (Node(Red, sibling, y, Node(Black, left, x, right)))
//         | Node(color, sibling, y, right) -> balance (Node(color, sibling, y, right)) |> done'
//         | _ -> done' t
//
//     and balance t =
//         let (xxx, node) =
//
//             match t with
//             | Node(color, Node(Red, Node(Red, a, x, b), y, c), z, d) // Symmetric balancing cases
//             | Node(color, Node(Red, a, x, Node(Red, b, y, c)), z, d)
//             | Node(color, a, x, Node(Red, Node(Red, b, y, c), z, d))
//             | Node(color, a, x, Node(Red, b, y, Node(Red, c, z, d))) ->
//                 done' (Node(color, Node(Black, a, x, b), y, Node(Black, c, z, d)))
//             | _ -> blacken t |> done'
//
//         node
//
//     blacken (fromResult (del tree))
