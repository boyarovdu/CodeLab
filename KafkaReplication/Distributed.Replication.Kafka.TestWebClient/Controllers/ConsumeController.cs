using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace Distributed.Replication.Kafka.TestWebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumeController(ILogger<ConsumeController> logger, IConsumer<string, string> consumer) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpGet]
    public IEnumerable<ExampleModel> Get()
    {
        return Enumerable.Range(1, 5)
            .Select(index => new ExampleModel())
            .ToArray();
    }
}