using System.Collections;
using System.Security.Cryptography;

namespace SSTable;

public interface IChecksum
{
    public Task AppendAsync(string filePath);
    public Task<bool> VerifyAsync(string filePath);
    public int GetSize();
}

public class Checksum : IChecksum
{
    public async Task AppendAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var checksum = await ComputeAsync(stream, stream.Length);
        await File.WriteAllBytesAsync(filePath, checksum);
    }

    public async Task<bool> VerifyAsync(string filePath)
    {
        byte[] controlChecksum;
        await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            if (fileStream.Length < MD5.HashSizeInBytes) return false;
            controlChecksum = await ComputeAsync(fileStream, fileStream.Length - MD5.HashSizeInBytes)!;
        }

        byte[] checksum;
        await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            checksum = await ReadChecksumAsync(fileStream);
        }
        
        return StructuralComparisons.StructuralEqualityComparer.Equals(controlChecksum, checksum);
    }

    public int GetSize()
    {
        return MD5.HashSizeInBytes;
    }

    private async Task<byte[]> ReadChecksumAsync(Stream stream)
    {
        var originalLength = stream.Length - MD5.HashSizeInBytes;
        
        stream.Seek(originalLength, SeekOrigin.Begin);

        var checksum = new byte[MD5.HashSizeInBytes];
        await stream.ReadExactlyAsync(checksum, 0, MD5.HashSizeInBytes);
        
        return checksum;
    }
    
    private async Task<byte[]> ComputeAsync(Stream stream, long originalLength)
    {
        using var md5 = MD5.Create();
        await stream.ProcessBlocksAsync(0, originalLength, (bytes, read) =>
            md5.TransformBlock(bytes, 0, read, null, 0));

        md5.TransformFinalBlock([], 0, 0);
        return md5.Hash!;
    }
}