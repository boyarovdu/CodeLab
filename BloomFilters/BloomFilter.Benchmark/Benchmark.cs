using BenchmarkDotNet.Attributes;

namespace BloomFilter.Benchmark;

public class BloomFilterBenchmark(int testDataSize = 1_000_000)
{
    [Benchmark]
    public void BloomFilterAdd()
    {
        var filter = new BloomFilter(testDataSize, 0.001f);
        
        for (var i = 0; i < testDataSize; i++)
        {
            filter.Add($"item{i}");
        }
    }
}