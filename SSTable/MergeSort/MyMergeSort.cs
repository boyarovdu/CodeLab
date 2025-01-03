namespace FluentApiExample;

using System;

public static class MergeSortAlgorithm
{
    public static void Merge(Span<int> slice1, Span<int> slice2, Span<int> buffer)
    {
        var i1 = 0;
        var i2 = 0;
        
        for (var i = 0; i < buffer.Length; i++)
        {
            if (i1 < slice1.Length && (i2 >= slice2.Length || slice1[i1] < slice2[i2]))
            {
                buffer[i] = slice1[i1++];
            }
            else
            {
                buffer[i] = slice2[i2++];
            }
        }
    }
    
    public static void MergeSort(Span<int> span, Span<int> buffer)
    {
        if (span.Length <= 1) return;
        
        var halfLength = span.Length / 2;

        var slice1 = span[..halfLength];
        var slice2 = span[halfLength..];

        MergeSort(slice1, buffer[..halfLength]);
        MergeSort(slice2, buffer[halfLength..]);

        Merge(slice1, slice2, buffer);
        
        buffer.CopyTo(span);
    }
}