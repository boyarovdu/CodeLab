using System.Diagnostics;

namespace Distributed.Replication.Kafka.Tests.Utils;

public static class TestUtil
{
    public static async Task<bool> WaitUntilAsync(int timeoutMs, Func<Task<bool>> asyncCondition, int delay = 100, bool throwException = true)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            await Task.Delay(delay);

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
                if (await asyncCondition()) return true;
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}