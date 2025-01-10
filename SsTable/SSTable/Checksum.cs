using System.Collections;
using System.Security.Cryptography;

namespace SSTable;

public class Checksum
{
    public static async Task AppendAsync(string path)
    {
        await using var stream = File.OpenRead(path);
        var checksum = await ComputeAsync(stream, stream.Length);
        await File.WriteAllBytesAsync(path, checksum);
    }

    public static async Task<bool> VerifyAsync(string filePath)
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

    private static async Task<byte[]> ReadChecksumAsync(Stream stream)
    {
        var originalLength = stream.Length - MD5.HashSizeInBytes;
        
        stream.Seek(originalLength, SeekOrigin.Begin);

        var checksum = new byte[MD5.HashSizeInBytes];
        await stream.ReadExactlyAsync(checksum, 0, MD5.HashSizeInBytes);
        
        return checksum;
    }
    
    private static async Task<byte[]> ComputeAsync(Stream stream, long originalLength)
    {
        using var md5 = MD5.Create();
        await stream.ProcessBlocksAsync(originalLength, (bytes, read) =>
            md5.TransformBlock(bytes, 0, read, null, 0));

        md5.TransformFinalBlock([], 0, 0);
        return md5.Hash!;
    }
}