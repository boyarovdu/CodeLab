using Confluent.Kafka;
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
        var list = new List<string>(consumer.Subscription) { topic };
        consumer.Subscribe(list);
    }
    
    [HttpDelete]
    public void Delete()
    {
        consumer.Unsubscribe();
    }
    
    [HttpGet]
    public ActionResult<ConsumeResult<string, string>> Get()
    {
        // Terrible approach for real app, but works great for my use case
        var consumed = consumer.Consume(5_000);

        if (consumed is { IsPartitionEOF: false })
        {
            return consumed;
        }
        
        return NotFound();
    }
}