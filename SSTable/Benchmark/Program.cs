﻿using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using FluentApiExample;

namespace MergeSortBenchmark
{
    [MemoryDiagnoser]
    public class MergeSortBenchmark
    {
        private int[] _data;

        [Params(1_000_000)] // Different sizes of input arrays to benchmark
        public int ArraySize;

        [GlobalSetup]
        public void Setup()
        {
            var random = new Random();
            _data = new int[ArraySize];
            for (int i = 0; i < ArraySize; i++)
            {
                _data[i] = random.Next();
            }
        }

        [Benchmark]
        public void MyMergeSort()
        {
            // Copy the data to ensure each run starts clean
            var dataCopy = new int[_data.Length];
            var bufferCopy = new int[_data.Length];
            _data.CopyTo(dataCopy, 0);

            // Run the sorting algorithm
            MergeSortAlgorithm.MergeSort(dataCopy, bufferCopy);
        }
        
        [Benchmark]
        public void MyParallelMergeSort()
        {
            // Copy the data to ensure each run starts clean
            var dataCopy = new int[_data.Length];
            _data.CopyTo(dataCopy, 0);

            // Run the sorting algorithm
            ParallelMergeSort.MergeSort(dataCopy);
        }        
        
        
        [Benchmark]
        public void AiMergeSort()
        {
            // Copy the data to ensure each run starts clean
            var dataCopy = new int[_data.Length];
            _data.CopyTo(dataCopy, 0);

            // Run the sorting algorithm
            MergeSortExample.MergeSort(dataCopy);
        }
        
        [Benchmark]
        public void NativeSort()
        {
            // Copy the data to ensure each run starts clean
            var dataCopy = new int[_data.Length];
            _data.CopyTo(dataCopy, 0);

            // Run the sorting algorithm
            Array.Sort(dataCopy);
        }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            // Run the benchmark
            var summary = BenchmarkRunner.Run<MergeSortBenchmark>();
        }
    }
}