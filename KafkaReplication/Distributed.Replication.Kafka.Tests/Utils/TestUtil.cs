using System.Diagnostics;

namespace Distributed.Replication.Kafka.Tests.Utils;

public static class TestUtil
{
    public static async Task WaitUntilAsync(int timeoutMs, Func<Task<bool>> asyncCondition, int delay = 100)
    {
        var stopwatch = Stopwatch.StartNew();

        while (true)
        {
            await Task.Delay(delay);

            if (stopwatch.ElapsedMilliseconds > timeoutMs)
                throw new TimeoutException("Condition of the test was not met in the specified time");

            try
            {
                if (await asyncCondition()) return;
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}