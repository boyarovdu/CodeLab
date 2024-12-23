module RedBlackTree_FSharp.RedBlackTree

open Algorithms.RedBlackTree_FSharp.Job

type Color =
    | Red
    | Black

type Tree<'T> =
    | NilNode
    | Node of Color * Tree<'T> * 'T * Tree<'T>

let (|Left|Right|None|) (x, y) =
    if x < y then Left
    elif x > y then Right
    else None

let insert tree x =

    let rec ins t =
        match t with
        | NilNode -> ToDo(Node(Red, NilNode, x, NilNode))
        | Node(color, left, y, right) ->
            match (x, y) with
            | Left -> left |> ins |> map (fun left' -> Node(color, left', y, right)) |> bind balance
            | Right -> right |> ins |> map (fun right' -> Node(color, left, y, right')) |> bind balance
            | None -> Done t

    and balance t =
        match t with
        | Node(Black, Node(Red, Node(Red, a, x, b), y, c), z, d) // Case 1
        | Node(Black, Node(Red, a, x, Node(Red, b, y, c)), z, d) // Case 2
        | Node(Black, a, x, Node(Red, Node(Red, b, y, c), z, d)) // Case 3
        | Node(Black, a, x, Node(Red, b, y, Node(Red, c, z, d))) -> // Case 4
            ToDo(Node(Red, Node(Black, a, x, b), y, Node(Black, c, z, d)))
        | _ -> Done t
    
    and blacken tree =
        match tree with
        | Node(Red, left, x, right) -> Node(Black, left, x, right)
        | _ -> tree

    tree |> ins |> fromResult |> blacken

let rec delete tree x =
    let rec del t =
        match t with
        | NilNode -> Done NilNode
        | Node(color, left, y, right) ->
            match x, y with
            | Left -> left |> del |> map (fun left' -> Node(color, left', y, right)) |> bind delLeft
            | Right ->
                right
                |> del
                |> map (fun right' -> Node(color, left, y, right'))
                |> bind delRight
            | None -> delRoot t

    and delRoot t =
        match t with
        | Node(Black, left, _, NilNode) -> blacken left
        | Node(Red, left, _, NilNode) -> Done left
        | Node(color, left, _, right) ->
            let m = ref None

            m
            |> delMin right
            |> map (fun right' -> Node(color, left, (m.Value).Value, right'))
            |> bind delRight

    and delMin t m =
        match t with
        | Node(Black, NilNode, y, right) ->
            m.Value <- Some y
            blacken right
        | Node(Red, NilNode, y, right) ->
            m.Value <- Some y
            Done right
        | Node(color, left, y, right) ->
            m
            |> delMin left
            |> map(fun left' -> Node(color, left', y, right)) 
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
            | _ -> blacken t

        fromResult node
        
    and blacken tree : Job<Tree<'T>> =
        match tree with
        | Node(Red, left, x, right) -> Node(Black, left, x, right) |> Done
        | _ -> tree |> ToDo

    tree |> del |> fromResult