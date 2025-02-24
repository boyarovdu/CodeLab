namespace PointFree.LeetCode_CSharp;

public class Problem_1624
{
    public int MaxLengthBetweenEqualCharacters(string s)
    {
        var cache = new Dictionary<char, int>();
        var res = -1;

        for (var i = 0; i < s.Length; i++)
        {
            if (cache.TryGetValue(s[i], out var val))
            {
                res = Math.Max(res, i - val - 1);
            }
            else
            {
                cache.Add(s[i], i);
            }
        }

        return res;
    }
}