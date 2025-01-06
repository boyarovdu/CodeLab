using FsCheck.Xunit;
using Xunit.Abstractions;

// For xUnit integration 

namespace Algorithms.MergeSort.Tests
{
    public class MergeSortTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MergeSortTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

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
            
            _testOutputHelper.WriteLine($"Original: {string.Join(", ", original)}"); 
            _testOutputHelper.WriteLine($"Clone:    {string.Join(", ", clone)}"); 
            _testOutputHelper.WriteLine(""); 
            
            return clone.SequenceEqual(original);
        }
    }
}