using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Distributed.Replication.Kafka.TestWebClient.HealthChecks;

public class ProducerHealthCheck(IProducer<string, string> producer) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
        CancellationToken cancellationToken = new CancellationToken())
    {
        try
        {
            var result = await producer.ProduceAsync("healthcheck", new Message<string, string> { Value = "ping" },
                cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception e)
        {
            return HealthCheckResult.Unhealthy(e.Message);
        }
    }
}