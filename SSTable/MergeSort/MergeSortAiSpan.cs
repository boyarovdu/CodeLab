using System;

class MergeSortWithSpan
{
    public static void MergeSort(Span<int> span)
    {
        if (span.Length <= 1) return;

        int mid = span.Length / 2;

        var left = span.Slice(0, mid);
        var right = span.Slice(mid);

        MergeSort(left);
        MergeSort(right);

        Merge(span, left, right);
    }

    private static void Merge(Span<int> target, Span<int> left, Span<int> right)
    {
        int i = 0, j = 0, k = 0;

        while (i < left.Length && j < right.Length)
        {
            if (left[i] <= right[j])
            {
                target[k++] = left[i++];
            }
            else
            {
                target[k++] = right[j++];
            }
        }

        while (i < left.Length)
        {
            target[k++] = left[i++];
        }

        while (j < right.Length)
        {
            target[k++] = right[j++];
        }
    }
}