using DockerTestWebApp.Controllers;

namespace DockerTestWebApp.Middleware;

public class ClientTypeMiddleware(RequestDelegate next)
{
    private readonly string _consumerControllerShortName =
        nameof(ConsumeController).Replace("Controller", string.Empty);

    private readonly string _produceControllerShortName = 
        nameof(ConsumeController).Replace("Controller", string.Empty);

    private async Task SetError(HttpResponse response, string message)
    {
        response.StatusCode = 400;
        response.ContentType = "text/plain";
        await response.WriteAsync(message);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;

        var controller = request.RouteValues["controller"];
        var controllerName = controller != null ? controller.ToString() : string.Empty;

        if (Program.Type == "consumer" && controllerName == _produceControllerShortName)
        {
            await SetError(context.Response,
                "Consumer API is not available for this service. Use producer API instead.");
        }
        else if (Program.Type == "producer" && controllerName == _consumerControllerShortName)
        {
            await SetError(context.Response,
                "Producer API is not available for this service. Use consumer API instead.");
        }
        else
        {
            await next(context);
        }
    }
}