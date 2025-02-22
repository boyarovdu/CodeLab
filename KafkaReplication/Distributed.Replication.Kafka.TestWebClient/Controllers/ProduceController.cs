using Confluent.Kafka;
using Distributed.Replication.Kafka.TestWebClient.Models;
using Microsoft.AspNetCore.Mvc;

namespace Distributed.Replication.Kafka.TestWebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ProduceController(ILogger<ConsumeController> logger, IProducer<string, string> producer) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpPost]
    public async Task<TopicPartitionOffset> Post([FromQuery] string topic, [FromBody] ProduceRequest request)
    {
        var res = await producer.ProduceAsync(topic, 
            new Message<string, string> { Value = request.Message },
            HttpContext.RequestAborted);
        
        return res.TopicPartitionOffset;
    }
}