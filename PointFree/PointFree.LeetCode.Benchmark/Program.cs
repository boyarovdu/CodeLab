using BenchmarkDotNet.Running;

namespace PointFree.LeetCode.Benchmark;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<Problem_485_Benchmark>();
    }
}