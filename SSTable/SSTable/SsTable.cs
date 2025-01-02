namespace SSTable;

using System;
using System.Security.Cryptography;
using DataRecord = Tuple<string, byte[]>;

public class BytesSegment
{
    public long Offset { get; init; }
    public long Length { get; init; }
}

public class BlockMetadata
{
    public required string KeyFrom { get; set; }
    public required BytesSegment PayloadLocation { get; set; }
}

public class SsTable(ISerializer serializer, IFooterConverter footerConverter, int blockSize)
{
    public async Task<BlockMetadata[]> Serialize(DataRecord[] records, Stream stream)
    {
        var metadata = new List<BlockMetadata>();

        var currentBlockSize = 0;
        var currentBlockOffset = 0;
        var currentBlockKeyFrom = null as string;
        var currentBlockBuffer = new List<DataRecord>();

        for (var i = 0; i < records.Length; i++)
        {
            var record = records[i];

            currentBlockKeyFrom ??= record.Item1;
            currentBlockSize += record.Item2.Length;
            currentBlockBuffer.Add(record);

            if (currentBlockSize >= blockSize || i == records.Length - 1)
            {
                var blockLength = await serializer.Serialize(currentBlockBuffer.ToArray(), stream);

                metadata.Add(new BlockMetadata
                {
                    KeyFrom = currentBlockKeyFrom,
                    PayloadLocation = new BytesSegment
                    {
                        Offset = currentBlockOffset,
                        Length = blockLength
                    }
                });

                currentBlockKeyFrom = null;
                currentBlockOffset += blockLength;
                currentBlockSize = 0;
                currentBlockBuffer.Clear();
            }
        }

        return metadata.ToArray();
    }

    public async Task CreateSsTable(DataRecord[] records)
    {
        await using var file = File.Create("test.sst");
        var blocksIndex = await Serialize(records, file);

        var blockIndexOffset = file.Position;
        var blockIndexLength = await serializer.Serialize(blocksIndex, file);

        var footerBytes =
            footerConverter.GetBytes(new Footer
            {
                BlockIndexOffset = blockIndexOffset,
                BlockIndexLength = blockIndexLength,
                BlockCompressionType = serializer.CompressionType,
                BlockSerializationType = serializer.SerializationType
            });

        await file.WriteAsync(footerBytes);
    }

    public static byte[] GetChecksum(Stream stream)
    {
        using var md5 = MD5.Create();
        return md5.ComputeHash(stream);
    }
}