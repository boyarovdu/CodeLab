using BenchmarkDotNet.Attributes;
using MessagePack;
using ProtoBuf;
using Serialization.Binary.Console.MsgPack;
using SportsData.Faker;
using SportsData.Models;

namespace Serialization.Binary.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(PercentilesConfig))]
public class DeserializationBenchmark
{
    private readonly byte[] _protobufDotNetSerializedTestData;
    private readonly byte[] _serializedMsgPackTestData;
    private readonly byte[] _serializedMsgPackChunkedTestData;

    public DeserializationBenchmark(int testDataSize = 50_000)
    {
        var testData = SportsDataFaker.GetSportEventFaker().Generate(testDataSize);
        var chunkSize = 250;
        
        using (var memoStream = new MemoryStream())
        {
            Serializer.Serialize(memoStream, testData);
            _protobufDotNetSerializedTestData = memoStream.ToArray();
        }
        
        using (var memoStream = new MemoryStream())
        {
            MessagePackSerializer.Serialize(memoStream, testData);
            _serializedMsgPackTestData = memoStream.ToArray();
        }
        
        using(var memoStream = new MemoryStream())
        {
            MsgPackHelper.Serialize(testData.ToArray(), memoStream, chunkSize);
            _serializedMsgPackChunkedTestData = memoStream.ToArray();
        }
    }

    [Benchmark(Baseline = true)]
    public void MessagePack_Deserialize()
    {
        using var memoStream = new MemoryStream(_serializedMsgPackTestData);
        MessagePackSerializer.Deserialize<SportEvent[]>(memoStream);
    }
    
    [Benchmark]
    public void ProtobufDotNet_Deserialize()
    {
        using var memoStream = new MemoryStream(_protobufDotNetSerializedTestData);
        Serializer.Deserialize<List<SportEvent>>(memoStream);
    }
    
    [Benchmark]
    public void MessagePackChunked_Deserialize()
    {
        using var memoStream = new MemoryStream(_serializedMsgPackChunkedTestData);
        MsgPackHelper.Deserialize<SportEvent>(memoStream);
    }
}