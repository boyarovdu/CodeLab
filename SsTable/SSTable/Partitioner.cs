namespace SSTable;

public static class Partitioner
{
    // internal static Tuple<int, int>[] CalculatePartitions(int length, int elementsPerPartition)
    // {
    //     var partitionsNumber = length / elementsPerPartition;
    //     
    //     if (length < partitionsNumber)
    //         return [new Tuple<int, int>(0, length)];
    //
    //     var result = new Tuple<int, int>[partitionsNumber];
    //     var remainder = length % partitionsNumber;
    //
    //     var offset = 0;
    //     for (var i = 0; i < partitionsNumber; i++)
    //     {
    //         var currentLength = elementsPerPartition + (i < remainder ? 1 : 0); // Handle remainder
    //         result[i] = new Tuple<int, int>(offset, currentLength);
    //         offset += currentLength;
    //     }
    //
    //     return result;
    // }
}