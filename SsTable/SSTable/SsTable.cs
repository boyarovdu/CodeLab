using System.Security.Cryptography;

namespace SSTable;

using System.IO;
using DataRecord = Tuple<string, byte[]>;
using BlockIndex = Block[];

public class SsTable(ISerializer serializer, IFooterConverter footerConverter, IChecksum checksum, int blockSize)
{
    public async Task Create(DataRecord[] records, string path)
    {
        await using (var fileStream = File.OpenWrite(path))
        {
            var blocks = await Serialize(records, fileStream);
            
            var footerBytes = footerConverter.GetBytes(new Footer
            {
                BlockIndexOffset = fileStream.Length,
                BlockIndexLength = await serializer.SerializeAsync(blocks, fileStream),
                BlockCompressionType = serializer.CompressionType,
                BlockSerializationType = serializer.SerializationType
            });

            fileStream.Write(footerBytes);
        }

        await checksum.AppendAsync(path);
    }

    public async Task<BlockIndex> ReadIndex(string path)
    {
        await using (var fileStream = File.OpenWrite(path))
        {
            var footerOffset = fileStream.Length - checksum.GetSize() - footerConverter.GetSize();
            var footer = await DeserializeFooter(fileStream, footerOffset, footerConverter.GetSize());
            
            return await DeserializeIndex(fileStream, footer.BlockIndexOffset, (int)footer.BlockIndexLength);
        }
    }
    
    private async Task<Footer> DeserializeFooter(Stream stream, long offset, int length)
    {
        if (stream.Length < offset + length)
            throw new Exception("SSTable footer is corrupted.");
        
        var result = new List<byte>(length);
        await stream.ProcessBlocksAsync(offset, length, (bytes, _) => result.AddRange(bytes));

        return footerConverter.ToFooter(result.ToArray());
    }
    
    private async Task<BlockIndex> DeserializeIndex(Stream stream, long offset, int length)
    {
        if (stream.Length < offset + length)
            throw new Exception("SSTable index is corrupted.");
        
        var result = new List<byte>(length);
        await stream.ProcessBlocksAsync(offset, length, (bytes, _) => result.AddRange(bytes));
        
        return serializer.Deserialize<BlockIndex>(result.ToArray());
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
                    Length = await serializer.SerializeAsync(partitions[i], stream),
                    Offset = offset
                };

            blocks[i] = block;
            offset += block.Length;
        }

        return blocks;
    }
}