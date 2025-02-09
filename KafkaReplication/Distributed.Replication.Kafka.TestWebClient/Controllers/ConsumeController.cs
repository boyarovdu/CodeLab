using Confluent.Kafka;
using Distributed.Replication.Kafka.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Distributed.Replication.Kafka.TestWebClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ConsumeController(ILogger<ConsumeController> logger, IConsumer<string, string> consumer) : ControllerBase
{
    private readonly ILogger<ConsumeController> _logger = logger;

    [HttpPost]
    public void Post(string topic)
    {
        consumer.Subscribe(topic);
    }
    
    [HttpDelete]
    public void Delete()
    {
        consumer.Unsubscribe();
    }
    
    [HttpGet]
    public ActionResult<ConsumeResult<string, string>> Get()
    {
        // Not great for real app, but works great for my use case
        var consumed = consumer.Consume(5_000);

        if (consumed is { IsPartitionEOF: false })
        {
            var res = new ConsumeResult
            {
                Value = consumed.Message.Value,
                Topic = consumed.Topic
            };
            
            return consumed;
        }
        
        return NotFound();
    }
}