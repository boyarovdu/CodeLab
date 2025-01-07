using FsCheck.Xunit;
using Xunit.Abstractions;

namespace Algorithms.MergeSort.Tests;

public class MergeSortTests(ITestOutputHelper testOutputHelper)
{
    [Property]
    public bool MergeSortAi_SortsElementsCorrectly(int[] original)
    {
        var clone = new int[original.Length];
        original.CopyTo(clone, 0);
            
        MergeSortAi.Sort(clone);
        Array.Sort(original);
            
        return clone.SequenceEqual(original);
    }
        
    [Property]
    public bool MergeSortGcFriendly_SortsElementsCorrectly(int[] original)
    {
        var clone = new int[original.Length];
        original.CopyTo(clone, 0);
            
        MergeSortGcFriendly.Sort(clone);
        Array.Sort(original);
            
        return clone.SequenceEqual(original);
    }
        
    [Property]
    public bool MergeSortParallel_SortsElementsCorrectly(int[] original)
    {
        var clone = new int[original.Length];
        original.CopyTo(clone, 0);

        MergeSortParallel.Sort(clone);
        Array.Sort(original);
            
        return clone.SequenceEqual(original);
    }
}