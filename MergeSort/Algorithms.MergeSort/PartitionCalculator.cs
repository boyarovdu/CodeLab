namespace Algorithms.MergeSort;

public static class PartitionCalculator
{
    internal static Partition[] CalculatePartitions(int length, int partitionsNumber)
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
}