using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace Algorithms.MergeSort.Benchmark;

[MemoryDiagnoser]
public class MergeSortBenchmark
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
            _testData[i] = random.Next();
        }
    }
    
    [Benchmark]
    public void AiMergeSort()
    {
        var array = new int[_testData.Length];
        _testData.CopyTo(array, 0);

        MergeSortAi.Sort(array);
    }
    
    [Benchmark]
    public void GcFriendlyMergeSort()
    {
        var array = new int[_testData.Length];
        _testData.CopyTo(array, 0);

        MergeSortGcFriendly.Sort(array);
    }
    
    [Benchmark(Baseline = true)]
    public void NativeSort()
    {
        var array = new int[_testData.Length];
        _testData.CopyTo(array, 0);

        Array.Sort(array);
    }
    
    [Benchmark]
    public void LinqParallelMergeSort()
    {
        var array = new int[_testData.Length];
        _testData.CopyTo(array, 0);

        array.AsParallel()
            .OrderBy(x => x) 
            .ToArray();
    }
    
    [Benchmark]
    public void GcFriendlyParallelMergeSort()
    {
        var array = new int[_testData.Length];
        _testData.CopyTo(array, 0);

        MergeSortParallel.Sort(array);
    }
}

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<MergeSortBenchmark>();
    }
}