namespace PointFree.LeetCode_CSharp;

// 674. Longest Continuous Increasing Subsequence
// Problem Link (Practice): https://leetcode.com/problems/longest-continuous-increasing-subsequence
public class Problem_674
{
    public int FindLengthOfLCIS(int[] nums) 
    {
        var totalMax = 1;
        var currentMax = 1;
        for (var i = 1; i < nums.Length; i++)
        {
            if (nums[i] > nums[i - 1])
            {
                currentMax++;
                totalMax = Math.Max(totalMax, currentMax);
            }
            else
            {
                currentMax = 1;
            }
        }    
        
        return totalMax;
    }
}