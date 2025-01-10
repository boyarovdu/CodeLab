namespace SSTable;

using System.IO;
using DataRecord = Tuple<string, byte[]>;

public class SsTable(ISerializer serializer, FooterConverter footerConverter, int blockSize)
{
    public async Task Create(DataRecord[] records, string path)
    {
        var fileStream = File.OpenWrite(path);
        var blocks = await Serialize(records, fileStream);

        var footerBytes = footerConverter.GetBytes(new Footer
        {
            BlockIndexOffset = fileStream.Length,
            BlockIndexLength = await serializer.Serialize(blocks, fileStream),
            BlockCompressionType = serializer.CompressionType,
            BlockSerializationType = serializer.SerializationType
        });

        fileStream.Write(footerBytes);
    }

    private async Task<Block[]> Serialize(DataRecord[] records, Stream stream)
    {
        var partitions = records.Chunk(blockSize).ToArray();
        var blocks = new Block[partitions.Length];

        var offset = stream.Length;

        for (var i = 0; i < partitions.Count(); i++)
        {
            var block =
                new Block
                {
                    KeyFrom = partitions[i].First().Item1,
                    Length = await serializer.Serialize(partitions[i], stream),
                    Offset = offset
                };

            blocks[i] = block;
            offset += block.Length;
        }

        return blocks;
    }
}
