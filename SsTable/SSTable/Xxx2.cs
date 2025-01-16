using System.Security.Cryptography;

namespace SSTable;

using System.IO;
using DataRecord = Tuple<int, byte[]>;
using BlockIndex = Block[];

public class Xxx1(ISerializer serializer, string path)
{
    private string Path { get; } = path;

    private BlockIndex Index { get; }

    public Xxx1(ISerializer serializer, string path, Block[] index)
        : this(serializer, path)
    {
        Index = index;
    }

    public async Task<DataRecord> FindAsync(int id)
    {
        for (var i = 0; i < Index.Length; i++)
        {
            if (id  >= Index[i].KeyFrom  && (i == Index.Length - 1 || id < Index[i + 1].KeyFrom))
            {
                return await FindAsync(id, Index[i]);
            }
        }

        return null;
    }

    private async Task<DataRecord> FindAsync(int id, Block block)
    {
        await using (var fileStream = File.OpenRead(Path))
        {
            var buffer = new byte[block.Length];
            await fileStream.ReadExactlyAsync(buffer, (int)block.Offset, (int)block.Length);

            var records = serializer.Deserialize<DataRecord[]>(buffer);
            foreach (var record in records)
            {
                if (record.Item1 == id) return record;
            }
        }

        return null;
    }
}

public class Xxx2(ISerializer serializer, IFooterConverter footerConverter, IChecksum checksum, int blockSize)
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
                BlockIndexLength = indexBytes.Length
            }));
        }

        await checksum.AppendAsync(path);
    }

    public async Task<Xxx1> ReadIndexAsync(string path)
    {
        await checksum.VerifyAsync(path);

        await using (var fileStream = File.OpenRead(path))
        {
            var footerBytes = new byte[footerConverter.SizeOf()];

            fileStream.Seek((int)(fileStream.Length - checksum.SizeOf() - footerConverter.SizeOf()), SeekOrigin.Begin);

            await fileStream.ReadExactlyAsync(footerBytes, 0, footerConverter.SizeOf());
            var footer = footerConverter.ToFooter(footerBytes);

            var indexBytes = new byte[footer.BlockIndexLength];
            
            fileStream.Seek((int)footer.BlockIndexOffset, SeekOrigin.Begin);
            await fileStream.ReadExactlyAsync(indexBytes, 0, (int)footer.BlockIndexLength);
            var index = serializer.Deserialize<Block[]>(indexBytes);

            return new Xxx1(serializer, path, index);
        }
    }

    private async Task<ValueTuple<long, List<Block>>> Serialize(DataRecord[] records, Stream stream)
    {
        return await records.ChunkIterAsync(blockSize,
            async (_, state, chunk) =>
            {
                var (offset, blocks) = state;

                var bytes = serializer.Serialize(chunk);
                await stream.WriteAsync(bytes);
                blocks.Add(new Block
                {
                    KeyFrom = chunk.First().Item1,
                    Length = bytes.Length, Offset = offset
                });

                return (offset + bytes.Length, blocks);
            }, state: (0L, new List<Block>()));
    }

    private async Task<byte[]> Serialize(Block[] blocks, FileStream fileStream)
    {
        var indexBytes = serializer.Serialize(blocks);
        await fileStream.WriteAsync(indexBytes);
        return indexBytes;
    }
}