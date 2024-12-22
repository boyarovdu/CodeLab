module RedBlackTree_FSharp.RedBlackTree


type Color =
    | Red
    | Black


type Tree<'T> =
    | Empty
    | Node of Color * Tree<'T> * 'T * Tree<'T>


// Monad-like helper functions for operations
let todo value = (true, value)
let done' value = (false, value)
let fromResult result = snd result

let xxx f x =
    let (isTodo, value) = x
    (isTodo, f value)


let (=<<) handler x =
    let (isTodo, value) = x
    if isTodo then handler value else (isTodo, value)


let blacken tree =
    match tree with
    | Node(Red, left, x, right) -> Node(Black, left, x, right)
    | _ -> tree


let insert tree x =
    let rec ins t =
        match t with
        | Empty -> todo (Node(Red, Empty, x, Empty))
        | Node(color, left, y, right) ->
            if x < y then
                (=<<) balance (xxx (fun l -> Node(color, l, y, right)) (ins left))
            elif x > y then
                (=<<) balance (xxx (fun r -> Node(color, left, y, r)) (ins right))
            else
                done' t

    and balance t =
        match t with
        | Node(Black, Node(Red, Node(Red, a, x, b), y, c), z, d) // Case 1
        | Node(Black, Node(Red, a, x, Node(Red, b, y, c)), z, d) // Case 2
        | Node(Black, a, x, Node(Red, Node(Red, b, y, c), z, d)) // Case 3
        | Node(Black, a, x, Node(Red, b, y, Node(Red, c, z, d))) -> // Case 4
            todo (Node(Red, Node(Black, a, x, b), y, Node(Black, c, z, d)))
        | _ -> done' t

    blacken (fromResult (ins tree))


let rec delete tree x =
    let rec del t =
        match t with
        | Empty -> done' Empty
        | Node(color, left, y, right) ->
            if x < y then
                (=<<) delLeft (xxx (fun l -> Node(color, l, y, right)) (del left))
            elif x > y then
                (=<<) delRight (xxx (fun r -> Node(color, left, y, r)) (del right))
            else
                delRoot t

    and delRoot t =
        match t with
        | Node(Black, left, _, Empty) -> blacken left |> done'
        | Node(Red, left, _, Empty) -> done' left
        | Node(color, left, _, right) ->
            let m = ref None // Mutable reference to hold the min value
            (=<<) delRight (xxx (fun r -> Node(color, left, (m.Value).Value, r)) (delMin right m))

    and delMin t m =
        match t with
        | Node(Black, Empty, y, right) ->
            m.Value <- Some y
            blacken right |> done'
        | Node(Red, Empty, y, right) ->
            m.Value <- Some y
            done' right
        | Node(color, left, y, right) -> (=<<) delLeft (xxx (fun l -> Node(color, l, y, right)) (delMin left m))

    and delLeft t =
        match t with
        | Node(Black, Node(Red, left, x, right), y, sibling) ->
            todo (Node(Red, Node(Black, left, x, right), y, sibling))
        | Node(color, left, y, sibling) -> balance (Node(color, left, y, sibling)) |> done'
        | _ -> done' t

    and delRight t =
        match t with
        | Node(Black, sibling, y, Node(Red, left, x, right)) ->
            todo (Node(Red, sibling, y, Node(Black, left, x, right)))
        | Node(color, sibling, y, right) -> balance (Node(color, sibling, y, right)) |> done'
        | _ -> done' t

    and balance t =
        let (xxx, node) =

            match t with
            | Node(color, Node(Red, Node(Red, a, x, b), y, c), z, d) // Symmetric balancing cases
            | Node(color, Node(Red, a, x, Node(Red, b, y, c)), z, d)
            | Node(color, a, x, Node(Red, Node(Red, b, y, c), z, d))
            | Node(color, a, x, Node(Red, b, y, Node(Red, c, z, d))) ->
                done' (Node(color, Node(Black, a, x, b), y, Node(Black, c, z, d)))
            | _ -> blacken t |> done'

        node

    blacken (fromResult (del tree))
