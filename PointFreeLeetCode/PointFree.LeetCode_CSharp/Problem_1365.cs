public class Problem_1365
{
    public int[] SmallerNumbersThanCurrent(int[] nums)
    {
        var count = new int[101];
        var result = new int[nums.Length];

        for (var index = 0; index < nums.Length; index++) count[nums[index]]++;
        for (var i = 1; i < 101; i++) count[i] += count[i - 1];
        for (var i = 0; i < nums.Length; i++)
            if (nums[i] == 0) result[i] = 0;
            else result[i] = count[nums[i] - 1];

        return result;
    }
}