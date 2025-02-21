using System.Diagnostics;

public static class TestUtil
{
    public static async Task<bool> WaitUntilAsync(int timeoutMs, Func<Task<bool>> asyncCondition, int delay = 100, bool throwException = true)
    {
        return await WaitUntilInternal(timeoutMs, asyncCondition, null, delay, throwException);
    }

    public static Task<bool> WaitUntilAsync(int timeoutMs, Func<bool> condition, int delay = 100, bool throwException = true)
    {
        return WaitUntilInternal(timeoutMs, null, condition, delay, throwException);
    }

    private static async Task<bool> WaitUntilInternal(
        int timeoutMs, 
        Func<Task<bool>>? asyncCondition, 
        Func<bool>? condition, 
        int delay, 
        bool throwException)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            if (stopwatch.ElapsedMilliseconds > timeoutMs)
            {
                if (throwException)
                {
                    throw new TimeoutException("Condition of the test was not met in the specified time");
                }
                return false;
            }

            try
            {
                if (asyncCondition != null && await asyncCondition())
                {
                    return true;
                }
                if (condition != null && condition())
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                // ignored
            }

            await Task.Delay(delay);
        }
    }
}