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

    public async Task<BlockIndex> ReadIndex(string path)
    {
        await using (var fileStream = File.OpenWrite(path))
        {
            //...
        }
    }

    // private async Task<Footer> DeserializeFooter(Stream stream, long offset, int length)
    // {
    //     if (stream.Length < offset + length)
    //         throw new Exception("SSTable footer is corrupted.");
    //
    //     var result = new List<byte>(length);
    //     await stream.ProcessBlocksAsync(offset, length, (bytes, _) => result.AddRange(bytes));
    //
    //     return footerConverter.ToFooter(result.ToArray());
    // }
    //
    // private async Task<BlockIndex> DeserializeIndex(Stream stream, long offset, int length)
    // {
    //     if (stream.Length < offset + length)
    //         throw new Exception("SSTable index is corrupted.");
    //
    //     var result = new List<byte>(length);
    //     await stream.ProcessBlocksAsync(offset, length, (bytes, _) => result.AddRange(bytes));
    //
    //     return serializer.Deserialize<BlockIndex>(result.ToArray());
    // }

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