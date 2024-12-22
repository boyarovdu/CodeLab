using System;

namespace Algorithms.RedBlackTree
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new instance of the Red-Black Tree
            var tree = new RedBlackTree<int>();

            Console.WriteLine("Red-Black Tree Demo:");

            // Insert values
            Console.WriteLine("Inserting values: 20, 15, 25, 10, 5, 30");
            tree.Insert(20);
            tree.Insert(15);
            tree.Insert(25);
            tree.Insert(10);
            tree.Insert(5);
            tree.Insert(30);

            // Perform In-Order Traversal
            Console.WriteLine("In-order Traversal (should be sorted):");
            tree.InOrderTraversal(value => Console.Write(value + " "));
            Console.WriteLine();

            // Search for a specific value
            Console.WriteLine("\nSearching for value 15:");
            var searchResult = tree.Search(15);
            if (searchResult.Value != 0) // assuming default value is 0 for int
                Console.WriteLine($"Value 15 found with color: {searchResult.Color}");
            else
                Console.WriteLine("Value 15 not found.");

            Console.WriteLine("\nSearching for value 99:");
            searchResult = tree.Search(99);
            if (searchResult.Value != 0)
                Console.WriteLine($"Value 99 found with color: {searchResult.Color}");
            else
                Console.WriteLine("Value 99 not found.");

            // Delete a node
            Console.WriteLine("\nDeleting value 15.");
            tree.Delete(15);

            Console.WriteLine("In-order Traversal after deletion:");
            tree.InOrderTraversal(value => Console.Write(value + " "));
            Console.WriteLine();

            // Delete another node
            Console.WriteLine("\nDeleting value 5.");
            tree.Delete(5);

            Console.WriteLine("In-order Traversal after deleting another value:");
            tree.InOrderTraversal(value => Console.Write(value + " "));
            Console.WriteLine();

            Console.WriteLine("\nFinished Red-Black Tree Demo. Press any key to exit.");
            Console.ReadKey();
        }
    }
}