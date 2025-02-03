using Distributed.Replication.Kafka.TestWebClient.Middleware;

namespace Distributed.Replication.Kafka.TestWebClient.Startup;

public static class ClientTypeMiddlewareExtensions
{
    public static IApplicationBuilder UseClientTypeMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ClientTypeMiddleware>();
    }
}