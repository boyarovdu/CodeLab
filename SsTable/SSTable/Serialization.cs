namespace SSTable;

public enum CompressionType
{
    None = 0
}

public enum SerializationType
{
    MsgPack = 0
}

public interface ISerializer
{
    public SerializationType SerializationType { get; } 
    public CompressionType CompressionType { get; } 
    public byte[] Serialize<T>(T obj);
    public T Deserialize<T>(byte[] bytes);
}