using DockerTestWebApp.Middleware;

namespace DockerTestWebApp.Startup;

public static class ClientTypeMiddlewareExtensions
{
    public static IApplicationBuilder UseClientTypeMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ClientTypeMiddleware>();
    }
}