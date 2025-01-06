namespace Algorithms.MergeSort;

public struct Partition
{
    public int Offset { get; init; }
    public int Length { get; init; }
}

public static class MergeSortParallel
{
    private static int ProcessorCount => Environment.ProcessorCount;

    public static void Sort(int[] array)
    {
        var memory = array.AsMemory();
        var buffer = new int[memory.Length].AsMemory();

        Sort(memory, buffer);
    }

    private static void Sort(Memory<int> memory, Memory<int> buffer)
    {
        var partitions = PartitionCalculator.CalculatePartitions(memory.Length, ProcessorCount);

        Parallel.ForEach(partitions,
            new ParallelOptions { MaxDegreeOfParallelism = ProcessorCount },
            partition =>
                MergeSortGcFriendly.Sort(
                    memory.Slice(partition.Offset, partition.Length).Span,
                    buffer.Slice(partition.Offset, partition.Length).Span));

        Merge(memory, buffer, partitions);
    }

    private static void Merge(Memory<int> memory, Memory<int> buffer, Partition[] partitions, int mergeFactor = 2)
    {
        var numberOfMerges = partitions.Length / mergeFactor;

        if (numberOfMerges <= 0) return;

        Parallel.For(0, numberOfMerges, new ParallelOptions { MaxDegreeOfParallelism = ProcessorCount },
            operationIndex =>
            {
                var leftPartitionIndex = operationIndex * mergeFactor;
                var rightPartitionIndex = leftPartitionIndex + mergeFactor / 2;

                var leftOffset = partitions[leftPartitionIndex].Offset;
                var rightOffset = partitions[rightPartitionIndex].Offset;

                var leftLength = rightOffset - leftOffset;
                var rightLength = operationIndex == numberOfMerges - 1
                    ? memory.Length - rightOffset
                    : partitions[rightPartitionIndex + mergeFactor / 2].Offset - rightOffset;

                var sliceLeft = memory.Slice(leftOffset, leftLength);
                var sliceRight = memory.Slice(rightOffset, rightLength);
                var sliceBuffer = buffer.Slice(leftOffset, leftLength + rightLength);

                MergeSortGcFriendly.Merge(sliceLeft.Span, sliceRight.Span, sliceBuffer.Span);
            });

        buffer.CopyTo(memory);
        Merge(memory, buffer, partitions, mergeFactor * 2);
    }
}