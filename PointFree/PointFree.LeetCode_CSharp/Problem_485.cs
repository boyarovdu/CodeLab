namespace PointFree.LeetCode_CSharp;

public class Problem_485()
{
    public int FindMaxConsecutiveOnes(int[] nums)
    {
        var maxCount = 0;
        var currentCount = 0;

        foreach (var num in nums)
        {
            if (num == 1)
            {
                currentCount++;
                if (currentCount > maxCount)
                {
                    maxCount = currentCount;
                }
            }
            else
            {
                currentCount = 0;
            }
        }

        return maxCount;
    }
    
    [Obsolete("Use with caution due to risk of StackOverFlow exception on bigger arrays")]
    public int FindMaxConsecutiveOnes_dp(int[] nums)
    {
        (int, int) FindMaxConsecutiveOnes(int pos)
        {
            if (pos <= 0) return (nums[0], nums[0]);

            var (total, current) = FindMaxConsecutiveOnes(pos - 1);
            current = nums[pos] == 1 ? current + 1 : 0;

            return (Math.Max(total, current), current);
        }

        return nums.Length > 0 ? FindMaxConsecutiveOnes(nums.Length - 1).Item1 : 0;
    }
}