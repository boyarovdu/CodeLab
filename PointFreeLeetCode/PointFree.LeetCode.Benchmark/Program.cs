using BenchmarkDotNet.Running;

namespace PointFree.LeetCode.Benchmark;

class Program
{
    static void Main(string[] args)
    {
        var s = new LeetCode_CSharp.Problem_532();
        
        s.FindPairs([1,3,1,5,4], 0);
        
        // BenchmarkRunner.Run<Problem_485_Benchmark>();
    }
}