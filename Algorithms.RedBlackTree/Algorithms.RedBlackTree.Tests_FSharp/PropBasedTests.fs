namespace RedBlackTree_FSharp.Tests

open FsCheck.NUnit
open RedBlackTree_FSharp.RedBlackTree

type PropBasedTests() =

    let isRootBlack tree =
        match tree with
        | Nil -> true
        | Node(color, _, _, _) -> color = Black

    let rec noRedRedViolation tree =
        match tree with
        | Nil -> true
        | Node(Red, Node(Red, _, _, _), _, _) -> false
        | Node(Red, _, _, Node(Red, _, _, _)) -> false
        | Node(_, left, _, right) -> noRedRedViolation left && noRedRedViolation right

    let rec blackHeightValid tree : (bool * int) =
        match tree with
        | Nil -> true, 1
        | Node(color, left, _, right) ->
            let (lValid, lHeight) = blackHeightValid left
            let (rValid, rHeight) = blackHeightValid right

            lValid && rValid && lHeight = rHeight, (if color = Black then lHeight + 1 else lHeight)

    let rec isBST (tree: Tree<'T>) =

        match tree with
        | Nil -> true
        | Node(_, left, y, right) ->
            match left, right with
            | Nil, Nil -> true
            | Node(_, _, x, _), Node(_, _, z, _) -> x < y && y < z
            | Nil, Node(_, _, z, _) -> y < z
            | Node(_, _, x, _), Nil -> x < y
            && isBST left
            && isBST right


    [<Property>]
    member _.``Root is always black after insert``(values: int list) =
        // Insert values and validate root property
        let tree = List.fold insert Nil values
        isRootBlack tree

    [<Property>]
    member _.``Red nodes cannot have red children after insert``(values: int list) =
        List.fold insert Nil values |> noRedRedViolation

    [<Property>]
    member _.``Black height is consistent across all paths after insert``(values: int list) =
        List.fold insert Nil values |> blackHeightValid |> fst

    [<Property>]
    member _.``Tree satisfies the BST property after insert``(values: int list) =
        List.fold insert Nil values |> isBST

    [<Property>]
    member _.``Root is always black after delete``(values: int list) =
        if values.Length > 0
        then List.randomChoice values |> delete(List.fold insert Nil values) |> isRootBlack
        else true

    [<Property>]
    member _.``Red nodes cannot have red children after delete``(values: int list) =
        if values.Length > 0
        then List.randomChoice values |> delete (List.fold insert Nil values) |> noRedRedViolation
        else true

    [<Property>]
    member _.``Black height is consistent across all paths after delete``(values: int list) =
        if values.Length > 0
        then List.randomChoice values |> delete (List.fold insert Nil values) |> blackHeightValid |> fst
        else true


    [<Property>]
    member _.``Tree satisfies the BST property after delete``(values: int list) =
        if values.Length > 0
        then List.randomChoice values |> delete (List.fold insert Nil values) |> isBST
        else true

    [<Property>]
    member _.``Root is always black after random value delete`` (values: int list) (valToDelete: int) =
        valToDelete |> delete(List.fold insert Nil values) |> isRootBlack


    [<Property>]
    member _.``Red nodes cannot have red children after random value delete`` (values: int list) (valToDelete: int) =
        valToDelete |> delete (List.fold insert Nil values) |> noRedRedViolation

    [<Property>]
    member _.``Black height is consistent across all paths after random val delete``
        (values: int list)
        (valToDelete: int)
        =
        valToDelete |> delete (List.fold insert Nil values) |> blackHeightValid |> fst

    [<Property>]
    member _.``Tree satisfies the BST property after random value delete``(values: int list)(valToDelete: int) =
        valToDelete |> delete (List.fold insert Nil values) |> isBST