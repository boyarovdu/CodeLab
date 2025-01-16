using System.Runtime.Serialization;

namespace SSTable;

[DataContract]
public class Block
{
    [DataMember(Order = 1)]
    public required int KeyFrom { get; init; }
    [DataMember(Order = 2)]
    public required long Offset { get; init; }
    [DataMember(Order = 3)]
    public required long Length { get; init; }
}