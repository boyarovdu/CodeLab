namespace PointFree.LeetCode.Benchmark;

using BenchmarkDotNet.Attributes;

public class Problem_485_Benchmark
{
    private int[] _testData;

    [Params(1_000, 10_000, 100_000, 1_000_000)]
    public int ArraySize;

    [GlobalSetup]
    public void Setup()
    {
        var random = new Random();
        _testData = new int[ArraySize];
        for (var i = 0; i < ArraySize; i++)
        {
            _testData[i] = random.Next(0, 2);
        }
    }
    
    [Benchmark]
    public void FindMaxConsecutiveOnes()
    {
        var s = new PointFree.LeetCode_CSharp.Problem_485();
        s.FindMaxConsecutiveOnes(_testData);
    }
    
    [Benchmark]
    public void FindMaxConsecutiveOnes_Dp()
    {
        var s = new PointFree.LeetCode_CSharp.Problem_485();
        s.FindMaxConsecutiveOnes_dp(_testData);
    }
    
    [Benchmark]
    public void FindMaxConsecutiveOnes_Tacit1()
    {
        Problem_485.maxConsecutiveOnes1(_testData);
    }
    
    [Benchmark]
    public void FindMaxConsecutiveOnes_Tacit2()
    {
        Problem_485.maxConsecutiveOnes2(_testData);
    }
}