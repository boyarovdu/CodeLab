using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Distributed.Replication.Kafka.TestWebClient.HealthChecks;

public class ConsumerHealthCheck(IConsumer<string, string> consumer) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            // ...            
            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception e)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(e.Message));
        }
    }
}