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

        Merge(memory, buffer, Environment.ProcessorCount);
    }

    private static void Merge(Memory<int> memory, Memory<int> buffer, int partitionsNumber)
    {
        var partitions = PartitionCalculator.CalculatePartitions(memory.Length, partitionsNumber);
        
        if (partitions.Length > 1)
        {
            Parallel.For(0, partitions.Length / 2,
                new ParallelOptions { MaxDegreeOfParallelism = ProcessorCount },
                iter =>
                {
                    var partitionLeft = partitions[iter * 2];
                    var partitionRight = partitions[iter * 2 + 1];

                    if (partitionLeft.Offset + partitionLeft.Length < partitionRight.Offset)
                        throw new Exception("Invalid partition order");

                    var sliceLeft = memory.Slice(partitionLeft.Offset, partitionLeft.Length);
                    var sliceRight = memory.Slice(partitionRight.Offset, partitionRight.Length);
                    var sliceBuffer = buffer.Slice(partitionLeft.Offset,
                        partitionLeft.Length + partitionRight.Length);

                    MergeSortGcFriendly.Merge(sliceLeft.Span, sliceRight.Span, sliceBuffer.Span);
                });

            buffer.CopyTo(memory);
            Merge(memory, buffer, partitionsNumber / 2);
        }
    }
}