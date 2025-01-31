using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace DockerTestWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class ProduceController(ILogger<ConsumeController> logger, IProducer<string, string> producer) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpPost]
    public IEnumerable<ExampleModel> Post()
    {
        return Enumerable.Range(1, 5)
            .Select(index => new ExampleModel())
            .ToArray();
    }
}