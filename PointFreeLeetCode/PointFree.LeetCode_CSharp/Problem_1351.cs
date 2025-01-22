namespace PointFree.LeetCode_CSharp;

public class Problem_1351
{
    public int CountNegatives(int[][] grid)
    {
        var res = 0;
        var i = grid[0].Length - 1;

        foreach (var row in grid)
        {
            while (i >= 0 && row[i] < 0) i--;
            res += grid[0].Length - (i + 1);
        }

        return res;
    }
}