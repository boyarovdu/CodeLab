namespace PointFree.LeetCode_CSharp;

public class Problem_1351
{
    public int CountNegatives(int[][] grid)
    {
        var count = 0;
        var n = grid[0].Length;

        foreach (var row in grid)
        {
            int left = 0, right = n - 1;
            
            while (left <= right)
            {
                var mid = (right + left) / 2;
                if (row[mid] < 0)
                {
                    right = mid - 1;
                }
                else
                {
                    left = mid + 1;
                }
            }
            count += (n - left);
        }

        return count;
    }
}