namespace RedBlackTree_FSharp.Tests

open NUnit.Framework
open RedBlackTree_FSharp.RedBlackTree

[<TestFixture>]
module RedBlackTreeTests =


    [<Test>]
    let ``Insert should add single node to empty tree`` () =
        // Arrange
        let emptyTree = NilNode
        let valueToInsert = 5

        // Act
        let resultingTree = insert emptyTree valueToInsert

        // Assert
        match resultingTree with
        | Node(Black, NilNode, 5, NilNode) -> Assert.Pass()
        | _ -> Assert.Fail("Insert did not add the value correctly.")

    [<Test>]
    let ``Insert should rebalance tree after red-red conflict`` () =
        // Arrange
        let initialTree =
            Node(Black, Node(Red, Node(Red, NilNode, 10, NilNode), 15, NilNode), 20, NilNode)
        
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
            Node(Black, Node(Red, NilNode, 10, NilNode), 20, Node(Black, NilNode, 30, NilNode))
        
        let valueToDelete = 10

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        match resultingTree with
        | Node(Black, NilNode, 20, Node(Black, NilNode, 30, NilNode)) -> Assert.Pass()
        | _ -> Assert.Fail("Delete did not remove the leaf node correctly.")

    [<Test>]
    let ``Delete should re-balance the tree after deletion`` () =
        // Arrange
        let tree =
            Node(Black, Node(Red, Node(Black, NilNode, 5, NilNode), 10, Node(Black, NilNode, 15, NilNode)), 20, NilNode)

        let valueToDelete = 10

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        match resultingTree with
        | Node(Black, Node(Black, NilNode, 5, NilNode), 15, NilNode) -> Assert.Pass()
        | _ -> Assert.Fail("Tree did not re-balance after deletion.")

    [<Test>]
    let ``Delete non-existing value should not modify the tree`` () =
        // Arrange
        let tree =
            Node(Black, Node(Red, NilNode, 10, NilNode), 15, Node(Black, NilNode, 20, NilNode))

        let valueToDelete = 25 // Non-existent value

        // Act
        let resultingTree = delete tree valueToDelete

        // Assert
        Assert.AreEqual(tree, resultingTree, "Delete of non-existing value modified the tree.")