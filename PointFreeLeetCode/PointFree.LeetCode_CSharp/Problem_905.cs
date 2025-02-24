namespace PointFree.LeetCode_CSharp;

public class Problem_905
{
    public int[] SortArrayByParity(int[] nums)
    {
        var i = 0;
        var j = nums.Length - 1;
        for (; i <= j; i++)
        {
            if (nums[i] % 2 == 0) continue;
            
            for (; j > i; j--)
            {
                if (nums[j] % 2 == 0)
                {
                    (nums[j], nums[i]) = (nums[i], nums[j]);
                    break;
                }
            }
        }
        
        return nums;
    }
}