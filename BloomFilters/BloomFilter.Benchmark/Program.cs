using BenchmarkDotNet.Running;

namespace BloomFilter.Benchmark;

class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<BloomFilterBenchmark>();
    }
}