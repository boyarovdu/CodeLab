namespace PointFree.LeetCode_CSharp;

public class Problem_1550
{
    public bool ThreeConsecutiveOdds(int[] arr)
    {
        for (var i = 0; i < arr.Length - 2; i++)
        {
            var product = arr[i] * arr[i + 1] * arr[i + 2];
            if (product % 2 == 1) return true;
        }

        return false;
    }
}