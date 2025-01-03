using System.Collections.Concurrent;

namespace FluentApiExample;

public struct Partition
{
    public int Offset { get; set; }
    public int Length { get; set; }
}

public class ParallelMergeSort
{
    public static void MergeSort(int[] array)
    {
        var memory = array.AsMemory();
        var buffer = new int[memory.Length].AsMemory();
        var partitions = CalculatePartitions(memory.Length, Environment.ProcessorCount);

        Parallel.ForEach(partitions, partition =>
            MergeSort(
                memory.Slice(partition.Offset, partition.Length).Span,
                buffer.Slice(partition.Offset, partition.Length).Span)
        );

        MergePartitions(memory, buffer, Environment.ProcessorCount);
    }

    private static void MergePartitions(Memory<int> memory, Memory<int> buffer, int partitionsNumber)
    {
        var partitions = CalculatePartitions(memory.Length, partitionsNumber);
        if (partitions.Length > 1)
        {
            Parallel.For(0, partitions.Length / 2, iter =>
            {
                var partitionLeft = partitions[iter * 2];
                var partitionRight = partitions[iter * 2 + 1];

                var left = memory.Slice(partitionLeft.Offset, partitionLeft.Length);
                var right = memory.Slice(partitionRight.Offset, partitionRight.Length);
                var bufferPartition = buffer.Slice(partitionLeft.Offset,
                    partitionLeft.Length + partitionRight.Length);

                MergeSortAlgorithm.Merge(left.Span, right.Span, bufferPartition.Span);
            });

            buffer.CopyTo(memory);

            MergePartitions(memory, buffer, partitionsNumber / 2);
        }
    }

    private static Partition[] CalculatePartitions(int length, int partitionsNumber)
    {
        var elementsPerPartition = length / partitionsNumber;
        if (elementsPerPartition > 1)
        {
            var partitions = new Partition[partitionsNumber];
            for (var i = 0; i < partitionsNumber; i++)
            {
                var partitionStart = i * elementsPerPartition;
                partitions[i] = i == partitionsNumber - 1
                    ? new Partition { Offset = partitionStart, Length = length - partitionStart }
                    : new Partition { Offset = partitionStart, Length = elementsPerPartition };
            }

            return partitions;
        }

        return [new Partition { Offset = 0, Length = length }];
    }

    private static void MergeSort(Span<int> data, Span<int> buffer)
    {
        MergeSortAlgorithm.MergeSort(data, buffer);
    }
}