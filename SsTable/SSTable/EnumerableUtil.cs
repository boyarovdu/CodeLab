namespace SSTable;

public static class EnumerableUtil
{
    public static async Task<TState> ChunkIterAsync<TData, TState>(this IEnumerable<TData> data, int size,
        Func<int, TState, TData[], Task<TState>> func, TState state)
    {
        var chunks = data.Chunk(size).ToArray();
        for (var i = 0; i < chunks.Length; i++)
        {
            state = await func(i, state, chunks[i]);
        }

        return state;
    }
}