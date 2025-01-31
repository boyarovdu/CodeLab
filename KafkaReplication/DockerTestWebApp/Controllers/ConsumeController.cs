using Microsoft.AspNetCore.Mvc;

namespace DockerTestWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumeController(ILogger<ConsumeController> logger) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<ExampleModel> Get()
    {
        return Enumerable.Range(1, 5)
            .Select(index => new ExampleModel())
            .ToArray();
    }
}