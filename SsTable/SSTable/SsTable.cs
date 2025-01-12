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
            var (blocksLength, blocks) = await Serialize(records, fileStream);
            var indexBytes = await Serialize(blocks.ToArray(), fileStream);

            fileStream.Write(footerConverter.GetBytes(new Footer
            {
                BlockIndexOffset = blocksLength,
                BlockIndexLength = indexBytes.Length,
                BlockCompressionType = serializer.CompressionType,
                BlockSerializationType = serializer.SerializationType
            }));
        }

        await checksum.AppendAsync(path);
    }

    private async Task<BlockIndex> ReadIndexAsync(string path)
    {
        await using var fileStream = File.OpenRead(path);
        
        var footerBytes = new byte[footerConverter.SizeOf()];
        await fileStream.ReadExactlyAsync(footerBytes, 
            (int)(fileStream.Length - checksum.GetSize() - footerConverter.SizeOf()), 
            footerConverter.SizeOf());
        var footer = footerConverter.ToFooter(footerBytes);

        var indexBytes = new byte[footer.BlockIndexLength];
        await fileStream.ReadExactlyAsync(indexBytes, (int)footer.BlockIndexOffset, (int)footer.BlockIndexLength);
        return serializer.Deserialize<Block[]>(indexBytes);
    }

    private async Task<ValueTuple<long, List<Block>>> Serialize(DataRecord[] records, Stream stream)
    {
        return await records.ChunkIterAsync(blockSize, new ValueTuple<long, List<Block>>(0, []),
            async (_, state, chunk) =>
            {
                var bytes = serializer.Serialize(chunk);
                await stream.WriteAsync(bytes);
                state.Item1 += bytes.Length;
                state.Item2.Add(
                    new Block { KeyFrom = chunk.First().Item1, Length = bytes.Length, Offset = state.Item1 });
            });
    }
    
    private async Task<byte[]> Serialize(Block[] blocks, FileStream fileStream)
    {
        var indexBytes = serializer.Serialize(blocks);
        await fileStream.WriteAsync(indexBytes);
        return indexBytes;
    }
}