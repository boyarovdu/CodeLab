namespace Algorithms.MergeSort;

public struct Partition
{
    public int Offset { get; init; }
    public int Length { get; init; }
}

public static class MergeSortParallel
{
    public static void Sort(int[] array)
    {
        var degreeOfParallelism = Util.ToNearestLowerPowerOfTwo(Environment.ProcessorCount);

        var memory = array.AsMemory();
        var buffer = new int[memory.Length].AsMemory();

        Sort(memory, buffer, degreeOfParallelism);
    }

    private static void Sort(Memory<int> memory, Memory<int> buffer, int degreeOfParallelism)
    {
        var partitions = Partitioner.CalculatePartitions(memory.Length, degreeOfParallelism);

        Parallel.ForEach(partitions,
            new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism },
            partition =>
                MergeSortGcFriendly.Sort(
                    memory.Slice(partition.Offset, partition.Length).Span,
                    buffer.Slice(partition.Offset, partition.Length).Span));

        Merge(memory, buffer, partitions, degreeOfParallelism);
    }

    private static void Merge(Memory<int> memory, Memory<int> buffer, Partition[] partitions, int degreeOfParallelism,
        int mergeFactor = 2)
    {
        var numberOfMerges = partitions.Length / mergeFactor;

        if (numberOfMerges <= 0) return;

        Parallel.For(0, numberOfMerges, new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism },
            operationIndex =>
            {
                var leftIndex = operationIndex * mergeFactor;
                var rightIndex = leftIndex + mergeFactor / 2;

                var leftOffset = partitions[leftIndex].Offset;
                var rightOffset = partitions[rightIndex].Offset;

                var leftLength = rightOffset - leftOffset;
                var rightLength = operationIndex == numberOfMerges - 1
                    ? memory.Length - rightOffset
                    : partitions[rightIndex + mergeFactor / 2].Offset - rightOffset;

                MergeSortGcFriendly.Merge(
                    memory.Slice(leftOffset, leftLength).Span,
                    memory.Slice(rightOffset, rightLength).Span,
                    buffer.Slice(leftOffset, leftLength + rightLength).Span);
            });

        buffer.CopyTo(memory);
        Merge(memory, buffer, partitions, degreeOfParallelism, mergeFactor * 2);
    }
}