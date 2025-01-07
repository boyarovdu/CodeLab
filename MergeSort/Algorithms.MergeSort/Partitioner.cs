namespace Algorithms.MergeSort;

public static class Partitioner
{
    internal static Partition[] CalculatePartitions(int length, int partitionsNumber)
    {
        if (length < partitionsNumber)
            return [new Partition { Offset = 0, Length = length }];

        var result = new Partition[partitionsNumber];
        var elementsPerPartition = length / partitionsNumber;
        var remainder = length % partitionsNumber;

        var offset = 0;
        for (var i = 0; i < partitionsNumber; i++)
        {
            var currentLength = elementsPerPartition + (i < remainder ? 1 : 0); // Handle remainder
            result[i] = new Partition { Offset = offset, Length = currentLength };
            offset += currentLength;
        }

        return result.ToArray();
    }
}