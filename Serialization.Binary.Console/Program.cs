using BenchmarkDotNet.Running;
using Serialization.Binary.Demos;
using Serialization.Binary.Benchmarks;
using SportsData.Faker;
using SportsData.Models;

namespace Serialization.Binary.Console
{
    class Program
    {
        static void SerializersBenchmark()
        {
            BenchmarkRunner.Run<SerializationBenchmark>();
        }
        
        static void SerializersBenchmark_v2()
        {
            BenchmarkRunner.Run<SerializationBenchmark>();
        }
        
        static void DeserializersBenchmark()
        {
            BenchmarkRunner.Run<DeserializationBenchmark>();
        }
        
        static void SerializersDemo()
        {   
            var totalMemoryBeforeTestDataSet = GC.GetTotalMemory(true);
            var testDataSet = SportsDataFaker.GetSportEventFaker().Generate(50_000);
            var totalMemoryAfterTestDataSet = GC.GetTotalMemory(true);
            System.Console.WriteLine($"Test data set of {testDataSet.Count} objects created. Approximate size of test data set in memory as objects is {(totalMemoryAfterTestDataSet - totalMemoryBeforeTestDataSet) / 1000} kB");
            
            DemoStep("Protobuf.net serializer demo.", true, () => ProtobufDemo.Serialize(testDataSet));
            
            DemoStep("MsgPack serializer demo.", true, () => MsgPackDemo.Serialize(testDataSet));
            DemoStep("MsgPack(with Lz4BlockArray compression) serializer demo.", true, () => MsgPackDemo.SerializeLz4BlockArray(testDataSet));
            DemoStep("MsgPack(with Lz4Block compression) serializer demo.", true, () => MsgPackDemo.SerializeLz4Block(testDataSet));
            
            DemoStep("End of demo.");
        }
        
        static void ImprovedMsgPackSerializersDemo()
        {   
            var totalMemoryBeforeTestDataSet = GC.GetTotalMemory(true);
            var testDataSet = SportsDataFaker.GetSportEventFaker().Generate(50_000);
            var totalMemoryAfterTestDataSet = GC.GetTotalMemory(true);
            System.Console.WriteLine($"Test data set of {testDataSet.Count} objects created. Approximate size of test data set in memory as objects is {(totalMemoryAfterTestDataSet - totalMemoryBeforeTestDataSet) / 1000} kB");
            
            DemoStep("MsgPack custom chunked serializer demo.", true, () => MsgPackDemo.SerializeChunked<SportEvent>(testDataSet.ToArray()));
            DemoStep("MsgPack(with Lz4BlockArray compression) custom chunked serializer demo.", true, () => MsgPackDemo.SerializeChunkedLz4BlockArray<SportEvent>(testDataSet.ToArray()));
            DemoStep("MsgPack(with Lz4Block compression) custom chunked serializer demo.", true, () => MsgPackDemo.SerializeChunkedLz4Block<SportEvent>(testDataSet.ToArray()));
            
            DemoStep("End of demo.");
        }

        static void DemoStep(string comment, bool collectGarbage = false, Action? code = null)
        {
            System.Console.WriteLine("------------------------------------");
            System.Console.WriteLine($"{comment}. Press any key to continue...");
            System.Console.ReadKey();
            code?.Invoke();
            if (collectGarbage)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
            }
        }

        static async Task Main(string[] args)
        {
            // Uncomment desired demo or benchmark method below to run it. 
            
            // SerializersDemo();
            // ImprovedMsgPackSerializersDemo();
            
            // SerializersBenchmark();
            // SerializersBenchmark_v2();
            // DeserializersBenchmark();
        }
    }
}