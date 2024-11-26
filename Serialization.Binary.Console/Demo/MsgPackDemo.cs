using MessagePack;
using Serialization.Binary.Console.MsgPack;

namespace Serialization.Binary.Demos;

public static class MsgPackDemo
{
    public static void Serialize<T>(T data)
    {
        SerializeInternal(data, MessagePackSerializerOptions.Standard, "MsgPack.bin");
    }

    public static void SerializeLz4BlockArray<T>(T data)
    {
        SerializeInternal(data,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray),
            "MsgPackLz4BlockArray.bin");
    }

    public static void SerializeLz4Block<T>(T data)
    {
        SerializeInternal(data,
            MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block),
            "MsgPackLz4Block.bin");
    }
    
    public static void SerializeChunked<T>(T[] data)
    {
        SerializeChunkedInternal(data, MessagePackSerializerOptions.Standard);
    }
    
    public static void SerializeChunkedLz4Block<T>(T[] data)
    {
        SerializeChunkedInternal(data, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4Block));
    }

    public static void SerializeChunkedLz4BlockArray<T>(T[] data)
    {
        SerializeChunkedInternal(data, MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray));
    }
    
    public static void SerializeChunkedInternal<T>(T[] data, MessagePackSerializerOptions? serializerOptions = null)
    {
        var fileName = "MsgPackChunked.bin";
        serializerOptions ??= MessagePackSerializerOptions.Standard;
        
        using (var output = File.Create(fileName))
        {
            MsgPackHelper.Serialize(data, output, 300, serializerOptions);
            System.Console.WriteLine($"MsgPack serialized data size is {output.Length / 1_000} kB");
            output.Flush();
        }

        System.Console.WriteLine("Press any key to continue with deserialization...");
        System.Console.ReadKey();
        
        using (var input = File.OpenRead(fileName))
        {
            // Ensures that deserialization won't throw an exception
            MsgPackHelper.Deserialize<T>(input, data.Length, serializerOptions);
        }
    }
    
    private static void SerializeInternal<T>(T data,
        MessagePackSerializerOptions serializerOptions,
        string fileName = "MsgPack.bin")
    {
        using (var output = File.Create(fileName))
        {
            MessagePackSerializer.Serialize(output, data, serializerOptions);
            System.Console.WriteLine($"MsgPack serialized data size is {output.Length / 1_000} kB");
            output.Flush();
        }

        System.Console.WriteLine("Press any key to continue with deserialization...");
        System.Console.ReadKey();
        
        using (var input = File.OpenRead(fileName))
        {
            // Ensures that deserialization won't throw an exception
            MessagePackSerializer.Deserialize<T>(input, serializerOptions);
        }
    }
}