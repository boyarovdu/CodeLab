namespace SSTable;

using System;
using System.Security.Cryptography;
using DataRecord = Tuple<string, byte[]>;

public enum CompressionType
{
    None = 0
}

public enum SerializationType
{
    ProtobufNet = 0
}

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

public class Footer
{
    public required BytesSegment BlockIndexLocation { get; init; }
    public required CompressionType BlockCompressionType { get; init; }
    public required SerializationType BlockSerializationType { get; init; }
}

public interface ISerializer
{
    public SerializationType SerializationType { get; } 
    public CompressionType CompressionType { get; } 
    public Task<int> Serialize<T>(T @obj, Stream stream);
}

public interface IFooterConverter
{
    public Version Version { get; }
    public int FooterLength { get; }
    public Footer ToFooter(byte[] bytes);
    public byte[] GetBytes(Footer footer);
}

public class Version(uint major, uint minor, uint patch)
{
    public uint MajorVersion { get; } = major;
    public uint MinorVersion { get; } = minor;
    public uint PatchVersion { get; } = patch;

    private const int VersionComponentLength = sizeof(uint);

    public byte[] GetBytes()
    {
        var result = new List<byte>(VersionComponentLength * 3);

        result.AddRange(BitConverter.GetBytes(MajorVersion));
        result.AddRange(BitConverter.GetBytes(MinorVersion));
        result.AddRange(BitConverter.GetBytes(PatchVersion));

        return result.ToArray();
    }

    public static Version FromBytes(byte[] bytes)
    {
        return FromBytes(bytes.AsSpan());
    }

    public static Version FromBytes(ReadOnlySpan<byte> bytes)
    {
        var vcl = VersionComponentLength;
        var major = BitConverter.ToUInt32(bytes.Slice(vcl * 0, vcl));
        var minor = BitConverter.ToUInt32(bytes.Slice(vcl * 1, vcl));
        var patch = BitConverter.ToUInt32(bytes.Slice(vcl * 2, vcl));

        return new Version(major, minor, patch);
    }

    public override string ToString() => $"{MajorVersion}.{MinorVersion}.{PatchVersion}";


    public override bool Equals(object? @object)
    {
        var other = @object as Version;

        if (other == null)
            return false;

        return MajorVersion == other.MajorVersion &&
               MinorVersion == other.MinorVersion &&
               PatchVersion == other.PatchVersion;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(MajorVersion, MinorVersion, PatchVersion);
    }
}

public class FooterConverter : IFooterConverter
{
    public Version Version { get; } = new Version(0, 0, 1);

    /// <summary>
    /// Length of the serialized footer in bytes
    /// </summary>
    public int FooterLength => 36;

    public Footer ToFooter(byte[] bytes)
    {
        var span = bytes.AsSpan();

        var version = Version.FromBytes(span.Slice(24, sizeof(uint) * 3));
        if (!Equals(version, Version))
            throw new Exception(
                $"Cannot convert given bytes into Footer object. Given footer format of version {version} but expected version {Version}.");

        var position = 0;
        var blockIndexOffest = BitConverter.ToInt64(span.Slice(sizeof(long), sizeof(long)));
        position += sizeof(long);
        var blockIndexLength = BitConverter.ToInt64(span.Slice(position, sizeof(long)));
        position += sizeof(long);
        var blockCompressionType = (CompressionType)BitConverter.ToInt32(span.Slice(position, sizeof(int)));
        position += sizeof(int);
        var blockSerializationType = (SerializationType)BitConverter.ToInt32(span.Slice(position, sizeof(int)));
        position += sizeof(int);

        return new Footer
        {
            BlockIndexLocation = new BytesSegment
            {
                Offset = blockIndexOffest,
                Length = blockIndexLength
            },
            BlockCompressionType = blockCompressionType,
            BlockSerializationType = blockSerializationType
        };
    }

    public byte[] GetBytes(Footer footer)
    {
        var result = new List<byte>(FooterLength);

        result.AddRange(BitConverter.GetBytes(footer.BlockIndexLocation.Offset)); // 8 bytes
        result.AddRange(BitConverter.GetBytes(footer.BlockIndexLocation.Length)); // 8 bytes
        result.AddRange(BitConverter.GetBytes((int)footer.BlockCompressionType)); // 4 bytes
        result.AddRange(BitConverter.GetBytes((int)footer.BlockSerializationType)); // 4 bytes
        result.AddRange(Version.GetBytes()); // 4 bytes * 3 = 12 bytes

        return result.ToArray();
    }
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

    public async Task<BytesSegment> Serialize(BlockMetadata[] blocksIndex, Stream stream)
    {
        var indexOffset = stream.Position;
        var indexLength = await serializer.Serialize(blocksIndex, stream);

        return new BytesSegment
        {
            Offset = indexOffset,
            Length = indexLength
        };
    }

    public async Task CreateSsTable(DataRecord[] records)
    {
        await using var file = File.Create("test.sst");
        var blocksIndex = await Serialize(records, file);

        var footerBytes =
            Serialize(new Footer
            {
                BlockIndexLocation = await Serialize(blocksIndex, file),
                BlockCompressionType = serializer.CompressionType,
                BlockSerializationType = serializer.SerializationType
            });

        var footerOffset = file.Position;
    }

    public byte[] Serialize(Footer footer)
    {
        return footerConverter.GetBytes(footer);
    }

    public static byte[] GetChecksum(Stream stream)
    {
        using var md5 = MD5.Create();
        return md5.ComputeHash(stream);
    }
}