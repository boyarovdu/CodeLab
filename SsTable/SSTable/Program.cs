using MessagePack;

namespace SSTable;

using DataRecord = Tuple<int, byte[]>;

class Serializer : ISerializer
{
    public SerializationType SerializationType => SerializationType.MsgPack;
    public CompressionType CompressionType => CompressionType.None;

    public byte[] Serialize<T>(T obj)
    {
        return MessagePackSerializer.Serialize(obj);
    }

    public T Deserialize<T>(byte[] bytes)
    {
        return MessagePackSerializer.Deserialize<T>(bytes.ToArray());
    }
}

static class Program
{
    static async Task Main(string[] args)
    {
        DataRecord[] data = [new(1, [1, 2, 3]), new(2, [4, 5, 6]), new DataRecord(3, [7, 8, 9])];

        var serializer = new Serializer();

        var propConv = new FooterConverterBuilder()
            .ConfigureSerializer(f => f.BlockIndexOffset,
                BitConverter.GetBytes,
                b => BitConverter.ToInt64(b.AsSpan()))
            .ConfigureSerializer(f => f.BlockIndexLength,
                BitConverter.GetBytes,
                b => BitConverter.ToInt64(b.AsSpan()))
            .Build();

        var a = new Xxx2(new Serializer(), propConv, new Checksum(), 1024);
        
        await a.Create(data, "test.sst");
        
        var b = await a.ReadIndexAsync("test.sst");
        
        var r = await b.FindAsync(3);
        
        Console.WriteLine("Hello, World!");
    }
}