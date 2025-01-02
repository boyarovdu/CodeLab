namespace SSTable;

public enum CompressionType
{
    None = 0
}

public enum SerializationType
{
    ProtobufNet = 0
}

public interface ISerializer
{
    public SerializationType SerializationType { get; } 
    public CompressionType CompressionType { get; } 
    public Task<int> Serialize<T>(T @obj, Stream stream);
}