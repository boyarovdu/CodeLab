namespace SSTable;

public static class EnumerableUtil
{
    public static async Task<TState> ChunkIterAsync<TData, TState>(this IEnumerable<TData> data, int size, TState state,
        Func<int, TState, TData[], Task> func)
    {
        var chunks = data.Chunk(size).ToArray();
        for (var i = 0; i < chunks.Length; i++)
        {
            await func(i, state, chunks[i]);
        }

        return state;
    }
}