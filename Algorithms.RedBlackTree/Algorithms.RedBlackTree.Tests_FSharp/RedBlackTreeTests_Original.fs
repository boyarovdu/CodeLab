namespace RedBlackTree_FSharp.Tests

open NUnit.Framework
open RedBlackTree_FSharp.RedBlackTree_Original

[<TestFixture>]
module RedBlackTreeTests_Original =

    [<Test>]
    let ``Blacken should return black root for red root`` () =
        // Arrange
        let redTree = Node(Red, Empty, 10, Empty)
        
        // Act
        let result = blacken redTree
        
        // Assert
        match result with
        | Node(Black, _, _, _) -> Assert.Pass()
        | _ -> Assert.Fail("Root was not blackened.")

    [<Test>]
    let ``Insert should add single node to empty tree`` () =
        // Arrange
        let emptyTree = Empty
        let valueToInsert = 5

        // Act
        let resultingTree = insert emptyTree valueToInsert

        // Assert
        match resultingTree with
        | Node(Black, Empty, 5, Empty) -> Assert.Pass()
        | _ -> Assert.Fail("Insert did not add the value correctly.")

    [<Test>]
    let ``Insert should rebalance tree after red-red conflict`` () =
        // Arrange
        let initialTree =
            Node(Black, Node(Red, Node(Red, Empty, 10, Empty), 15, Empty), 20, Empty)
        
        let valueToInsert = 5

        // Act
        let resultingTree = insert initialTree valueToInsert

        // Assert
        // Check that the resulting tree resolves conflicts properly
        match resultingTree with
        | Node(Red, Node(Black, _, _, _), _, Node(Black, _, _, _)) -> Assert.Pass()
        | _ -> Assert.Fail("Tree did not rebalance correctly after insertion.")

    [<Test>]
    let ``Delete should remove a leaf node`` () =
        // Arrange
        let tree =
            Node(Black, Node(Red, Empty, 10, Empty), 20, Node(Black, Empty, 30, Empty))
        
        let valueToDelete = 10

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        match resultingTree with
        | Node(Black, Empty, 20, Node(Black, Empty, 30, Empty)) -> Assert.Pass()
        | _ -> Assert.Fail("Delete did not remove the leaf node correctly.")

    [<Test>]
    let ``Delete should rebalance the tree after deletion`` () =
        // Arrange
        let tree =
            Node(Black, Node(Red, Node(Black, Empty, 5, Empty), 10, Node(Black, Empty, 15, Empty)), 20, Empty)

        let valueToDelete = 10

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        match resultingTree with
        | Node(Black, Node(Black, Empty, 5, Empty), 15, Empty) -> Assert.Pass()
        | _ -> Assert.Fail("Tree did not re-balance after deletion.")

    [<Test>]
    let ``Delete non-existing value should not modify the tree`` () =
        // Arrange
        let tree =
            Node(Black, Node(Red, Empty, 10, Empty), 15, Node(Black, Empty, 20, Empty))

        let valueToDelete = 25 // Non-existent value

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        Assert.AreEqual(tree, resultingTree, "Delete of non-existing value modified the tree.")