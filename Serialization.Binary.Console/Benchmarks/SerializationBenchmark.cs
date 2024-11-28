using BenchmarkDotNet.Attributes;
using MessagePack;
using ProtoBuf;
using Serialization.Binary.Console.MsgPack;
using SportsData.Faker;
using SportsData.Models;

namespace Serialization.Binary.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(PercentilesConfig))]
public class SerializationBenchmark(int testDataSize = 50_000)
{
    private readonly List<SportEvent> _testData = SportsDataFaker.GetSportEventFaker().Generate(testDataSize);
    private readonly int _chunkSize = 250;
    private readonly int _memoBufferInitialSize = 500_000_000;

    [Benchmark(Baseline = true)]
    public void MessagePack()
    {
        var memoStream = new MemoryStream(_memoBufferInitialSize);
        MessagePackSerializer.Serialize(memoStream, _testData);
    }
    
    [Benchmark]
    public void MsgPack_Lz4BlockArray()
    {
        var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        MessagePackSerializer.Serialize(memoStream, _testData, options);
    }
    
    [Benchmark]
    public void MsgPack_Lz4Block()
    {
        var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        MessagePackSerializer.Serialize(memoStream, _testData, options);
    }
    
    [Benchmark]
    public void ProtobufDotNet()
    {
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        Serializer.Serialize(memoStream, _testData);
    }
}