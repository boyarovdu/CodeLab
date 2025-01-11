namespace SSTable;

public class Block
{
    public required string KeyFrom { get; init; }
    public required long Offset { get; init; }
    public required long Length { get; init; }
}