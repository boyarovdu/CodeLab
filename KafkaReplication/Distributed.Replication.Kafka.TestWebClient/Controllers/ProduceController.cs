using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;

namespace Distributed.Replication.Kafka.TestWebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ProduceController(ILogger<ConsumeController> logger, IProducer<string, string> producer) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpPost]
    public async Task<TopicPartitionOffset> Post(string topic, string message)
    {
        var res  = await producer.ProduceAsync(topic, new Message<string, string> {Value = message });
        return res.TopicPartitionOffset;
    }
}