using BenchmarkDotNet.Attributes;
using MessagePack;
using ProtoBuf;
using Serialization.Binary.Console.MsgPack;
using SportsData.Faker;
using SportsData.Models;

namespace Serialization.Binary.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(PercentilesConfig))]
public class SerializationBenchmark_v2(int testDataSize = 20_000)
{
    private readonly SportEvent[] _testData = SportsDataFaker.GetSportEventFaker().Generate(testDataSize).ToArray();
    private readonly int _chunkSize = 250;
    private readonly int _memoBufferInitialSize = 200_000_000;

    [Benchmark(Baseline = true)]
    public void MessagePack()
    {
        var memoStream = new MemoryStream(_memoBufferInitialSize);
        MsgPackHelper.Serialize(_testData.ToArray(), memoStream, _chunkSize, MessagePackSerializerOptions.Standard);
    }
    
    [Benchmark]
    public void MsgPack_WithoutImprovements()
    {
        var options = MessagePackSerializerOptions.Standard;
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        MsgPackHelper.Serialize_Slow(_testData.ToArray(), memoStream, _chunkSize, options);
    }
    
    [Benchmark]
    public void MsgPack_Lz4BlockArray()
    {
        var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        MsgPackHelper.Serialize(_testData.ToArray(), memoStream, _chunkSize, options);
    }
    
    [Benchmark]
    public void MsgPack_Lz4Block()
    {
        var options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block);
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        MsgPackHelper.Serialize(_testData.ToArray(), memoStream, _chunkSize, options);
    }
    
    [Benchmark]
    public void ProtobufDotNet()
    {
        using var memoStream = new MemoryStream(_memoBufferInitialSize);
        Serializer.Serialize(memoStream, _testData);
    }
}