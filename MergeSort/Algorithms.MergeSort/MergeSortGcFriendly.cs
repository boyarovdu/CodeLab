namespace Algorithms.MergeSort;

public static class MergeSortGcFriendly
{
    public static void Sort(int[] array)
    {   
        var buffer = new int[array.Length];
        Sort(array.AsSpan(), buffer.AsSpan());
    }
    
    internal static void Merge(Span<int> slice1, Span<int> slice2, Span<int> buffer)
    {
        var idx1 = 0;
        var idx2 = 0;

        for (var iter = 0; iter < buffer.Length; iter++)
        {
            if (idx1 < slice1.Length && (idx2 >= slice2.Length || slice1[idx1] < slice2[idx2]))
            {
                buffer[iter] = slice1[idx1++];
            }
            else
            {
                buffer[iter] = slice2[idx2++];
            }
        }
    }

    internal static void Sort(Span<int> span, Span<int> buffer)
    {
        if (span.Length > 1)
        {
            var halfLength = span.Length / 2;

            var left = span[..halfLength];
            var right = span[halfLength..];

            Sort(left, buffer[..halfLength]);
            Sort(right, buffer[halfLength..]);

            Merge(left, right, buffer);

            buffer.CopyTo(span);
        }
    }
}