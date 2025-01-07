namespace Algorithms.MergeSort;

public static class MergeSortGcFriendly
{
    public static void Sort(int[] array)
    {   
        var buffer = new int[array.Length];
        Sort(array.AsSpan(), buffer.AsSpan());
    }
    
    public static void Merge(Span<int> left, Span<int> right, Span<int> buffer)
    {
        var idx1 = 0;
        var idx2 = 0;

        for (var iter = 0; iter < buffer.Length; iter++)
        {
            if (idx1 < left.Length && (idx2 >= right.Length || left[idx1] < right[idx2]))
            {
                buffer[iter] = left[idx1++];
            }
            else
            {
                buffer[iter] = right[idx2++];
            }
        }
    }

    internal static void Sort(Span<int> span, Span<int> buffer)
    {
        if (span.Length > 1)
        {
            var halfLength = span.Length / 2;

            var left = span.Slice(0, halfLength);
            var right = span.Slice(halfLength);

            Sort(left, buffer.Slice(0, halfLength));
            Sort(right, buffer.Slice(halfLength));

            Merge(left, right, buffer);

            buffer.CopyTo(span);
        }
    }
}