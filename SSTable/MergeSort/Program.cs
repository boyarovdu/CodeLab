using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentApiExample
{
   
    class Program
    {
        static void Main()
        {
            int[] array = { 38, 27, 43, 3, 9, 82, 10 };
            int[] buffer = new int[array.Length]; // Allocate buffer

            Console.WriteLine("Original Array: " + string.Join(", ", array));

            // Perform merge sort
            MergeSortAlgorithm.MergeSort(array.AsSpan(), buffer.AsSpan());

            Console.WriteLine("Sorted Array: " + string.Join(", ", array));
            
            
            int[] array2 = { 38, 27, 43, 3, 9, 82, 10 };
            Console.WriteLine("Original Array: " + string.Join(", ", array2));

            // Using the array as a span
            MergeSortWithSpan.MergeSort(array2.AsSpan());

            Console.WriteLine("Sorted Array: " + string.Join(", ", array2));
            
            
            int[] array3 = { 38, 27, 43, 3, 9, 82, 10 };
            Console.WriteLine("Original Array: " + string.Join(", ", array3));

            MergeSortExample.MergeSort(array3);

            Console.WriteLine("Sorted Array: " + string.Join(", ", array3));
            
            
            int[] array4 = { 38, 27, 43, 3, 9, 82, 10 };
            Console.WriteLine("Original Array: " + string.Join(", ", array4));
            ParallelMergeSort.MergeSort(array4);
            Console.WriteLine("Sorted Array: " + string.Join(", ", array4));
        }
    }
}