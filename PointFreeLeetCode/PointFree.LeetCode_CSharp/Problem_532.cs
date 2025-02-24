namespace PointFree.LeetCode_CSharp;

// 532. K-diff Pairs in an Array
// Problem Link (Practice): https://leetcode.com/problems/k-diff-pairs-in-an-array/description/
public class Problem_532
{
    public int FindPairs(int[] nums, int k)
    {
        var cache = new Dictionary<int, int>();
        foreach (var num in nums)
        {
            cache[num] = cache.TryGetValue(num, out var v) ? v + 1 : 1;
        }

        var res = 0;
        foreach (var (i, n) in cache)
        {
            var j = i + k;
            switch (k)
            {
                case > 0 when cache.ContainsKey(j): 
                case 0 when n > 1:
                    res++;
                    break;
            }
        }
        
        return res;
    }
}