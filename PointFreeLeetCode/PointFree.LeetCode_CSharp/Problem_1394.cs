namespace PointFree.LeetCode_CSharp;

public class Problem_1394
{
    public int FindLucky(int[] arr)
    {
        var counter = new Dictionary<int, int>();

        foreach (var el in arr)
        {
            if (counter.TryGetValue(el, out var count))
            {
                counter[el] = ++count;
                continue;
            }

            counter.Add(el, 1);
        }

        var res = -1;
        foreach (var (val, count) in counter)
        {
            if (val == count) res = Math.Max(res, val);
        }

        return res;
    }
}