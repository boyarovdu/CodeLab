module RedBlackTree_FSharp.RedBlackTree

open Algorithms.RedBlackTree_FSharp.Result

type Color =
    | Red
    | Black

type Tree<'T> =
    | Nil
    | Node of Color * Tree<'T> * 'T * Tree<'T>

let (|Left|Right|None|) (x, y) =
    if x < y then Left
    elif x > y then Right
    else None

let insert tree x =

    let rec ins tree =
        match tree with
        | Nil -> ToDo(Node(Red, Nil, x, Nil))
        | Node(color, left, y, right) ->
            match (x, y) with
            | Left -> left |> ins |> map (fun left' -> Node(color, left', y, right)) |> bind balance
            | Right -> right |> ins |> map (fun right' -> Node(color, left, y, right')) |> bind balance
            | None -> Done tree

    and balance tree =
        match tree with
        | Node(Black, Node(Red, a, x, Node(Red, b, y, c)), z, d)
        | Node(Black, Node(Red, Node(Red, a, x, b), y, c), z, d)
        | Node(Black, a, x, Node(Red, Node(Red, b, y, c), z, d))
        | Node(Black, a, x, Node(Red, b, y, Node(Red, c, z, d))) ->
            Node(Red, Node(Black, a, x, b), y, Node(Black, c, z, d)) |> ToDo
        | Node(Black, _, _, _) -> Done tree
        | _ -> ToDo tree

    and blacken tree =
        match tree with
        | Node(Red, left, x, right) -> Node(Black, left, x, right)
        | _ -> tree

    tree |> ins |> fromResult |> blacken

let rec delete tree x =
    let rec del tree =
        match tree with
        | Nil -> Done Nil
        | Node(color, left, y, right) ->
            match x, y with
            | Left -> left |> del |> map (fun left' -> Node(color, left', y, right)) |> bind eqLeft
            | Right -> right |> del |> map (fun right' -> Node(color, left, y, right')) |> bind eqRight
            | None -> delRoot tree

    and delRoot t =
        match t with
        | Node(Black, left, _, Nil) -> blacken left
        | Node(Red, left, _, Nil) -> Done left
        | Node(color, left, _, right) ->
            let m = ref None

            m
            |> delMin right
            |> map (fun right' -> Node(color, left, m.Value.Value, right'))
            |> bind eqRight

    and delMin t m =
        match t with
        | Node(Black, Nil, y, right) ->
            m.Value <- Some y
            blacken right
        | Node(Red, Nil, y, right) ->
            m.Value <- Some y
            Done right
        | Node(color, left, y, right) ->
            m
            |> delMin left
            |> map (fun left' -> Node(color, left', y, right))
            |> bind eqLeft

    and eqLeft t =
        match t with
        | Node(_, sibling, y, Node(Red, left, z, right)) ->
            Node(Red, sibling, y, left)
            |> eqLeft
            |> map (fun left' -> Node(Black, left', z, right))
        | Node(color, sibling, y, Node(Black, left, z, right)) ->
            Node(color, sibling, y, Node(Red, left, z, right)) |> balance

    and eqRight t =
        match t with
        | Node(_, Node(Red, left, x, right), y, sibling) ->
            Node(Red, right, y, sibling)
            |> eqRight
            |> map (fun right' -> Node(Black, left, x, right'))
        | Node(color, Node(Black, left, x, right), y, sibling) ->
            Node(color, Node(Red, left, x, right), y, sibling) |> balance

    and balance t =
        match t with
        | Node(k, Node(Red, a, x, Node(Red, b, y, c)), z, d)
        | Node(k, Node(Red, Node(Red, a, x, b), y, c), z, d)
        | Node(k, a, x, Node(Red, Node(Red, b, y, c), z, d))
        | Node(k, a, x, Node(Red, b, y, Node(Red, c, z, d))) ->
            Node(k, Node(Black, a, x, b), y, Node(Black, c, z, d)) |> Done
        | _ -> blacken t

    and blacken tree : Result<Tree<'T>> =
        match tree with
        | Node(Red, left, x, right) -> Node(Black, left, x, right) |> Done
        | _ -> ToDo tree

    tree |> del |> fromResult
