using ProtoBuf;

namespace Serialization.Binary.Demos;

public class ProtobufDemo
{
    public static void Serialize<T>(T data)
    {
        using (var output = File.Create("Protobuf.bin"))
        {
            Serializer.Serialize(output, data);
            System.Console.WriteLine($"Protobuf serialized data size is {output.Length / 1_000} kB");
            output.Flush();
        }

        System.Console.WriteLine("Press any key to continue with deserialization...");
        System.Console.ReadKey();
        
        using (var input = File.OpenRead("Protobuf.bin"))
        {
            // Ensures that deserialization won't throw an exception
            Serializer.Deserialize<T>(input);
        }
    }
}