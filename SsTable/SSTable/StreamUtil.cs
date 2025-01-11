namespace SSTable;

public static class StreamUtil
{
    public static async Task ProcessBlocksAsync(this Stream stream, long offset, long length, Action<byte[], int> action, int blockSize = 8192)
    {
        stream.Seek(offset, SeekOrigin.Begin);
        
        var buffer = new byte[blockSize];
        long totalRead = 0;

        while (totalRead < length)
        {
            var bytesToRead = Math.Min(buffer.Length, length - totalRead);
            var read = await stream.ReadAsync(buffer, 0, (int)bytesToRead);

            if (read <= 0) continue;
            action(buffer, read);
            totalRead += read;
        }
    }
}