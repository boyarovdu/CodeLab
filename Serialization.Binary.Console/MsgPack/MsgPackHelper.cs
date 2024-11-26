using System.Buffers;
using MessagePack;

namespace Serialization.Binary.Console.MsgPack;

public static class MsgPackHelper
{
    public static void Serialize_Slow<T>(T[] dataArray,
        Stream output,
        int chunkSize = 1_000,
        MessagePackSerializerOptions? serializerOptions = null)
    {
        if (dataArray.Length < chunkSize) chunkSize = dataArray.Length;
        serializerOptions ??= MessagePackSerializerOptions.Standard;

        for (var i = 0; i < dataArray.Length; i += chunkSize)
        {
            var remaining = dataArray.Length - i;
            var currentChunkSize = remaining < chunkSize ? remaining : chunkSize;

            var chunk = dataArray.Skip(i).Take(currentChunkSize);
            var chunkBytes = MessagePackSerializer.Serialize(chunk, serializerOptions);

            output.Write(BitConverter.GetBytes(chunkBytes.Length));
            output.Write(chunkBytes);
        }

        output.Flush();
    }
    
    public static void Serialize<T>(T[] dataArray,
        Stream output,
        int chunkSize = 1_000,
        MessagePackSerializerOptions? serializerOptions = null)
    {
        if (dataArray.Length < chunkSize) chunkSize = dataArray.Length;
        serializerOptions ??= MessagePackSerializerOptions.Standard;

        var chunkBuffer = new ArrayBufferWriter<byte>(256_000);
        Span<byte> chunkLengthBuffer = stackalloc byte[sizeof(int)];

        var dataArrayAsMemory = dataArray.AsMemory();
        for (var i = 0; i < dataArrayAsMemory.Length; i += chunkSize)
        {
            var remaining = dataArrayAsMemory.Length - i;
            var currentChunkSize = remaining < chunkSize ? remaining : chunkSize;

            var chunk = dataArrayAsMemory.Slice(i, currentChunkSize);
            MessagePackSerializer.Serialize(chunkBuffer, chunk, serializerOptions);

            BitConverter.TryWriteBytes(chunkLengthBuffer, chunkBuffer.WrittenCount);
            output.Write(chunkLengthBuffer);
            output.Write(chunkBuffer.WrittenSpan);

            chunkBuffer.ResetWrittenCount();
        }

        output.Flush();
    }

    public static T[] Deserialize<T>(Stream input,
        int capacity = 0,
        MessagePackSerializerOptions? serializerOptions = null)
    {
        var matchesList = new List<T>(capacity);

        var chunkLengthBuffer = new byte[4];
        var chunkBuffer = new byte[256_000];

        serializerOptions ??= MessagePackSerializerOptions.Standard;

        while (input.Position < input.Length)
        {
            _ = input.Read(chunkLengthBuffer);
            var chunkLength = BitConverter.ToInt32(chunkLengthBuffer);

            // If the buffer is too small, resize it
            if (chunkBuffer.Length < chunkLength)
            {
                var newSize = chunkBuffer.Length;
                while (newSize < chunkLength) newSize *= 2;
                chunkBuffer = new byte[newSize];
            }

            input.Read(chunkBuffer, 0, chunkLength);
            var chunk = MessagePackSerializer.Deserialize<T[]>(chunkBuffer, serializerOptions);
            matchesList.AddRange(chunk);
        }

        return matchesList.ToArray();
    }
}