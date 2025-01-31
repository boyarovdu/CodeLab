using Confluent.Kafka;

namespace DockerTestWebApp.KafkaClients;

public class Producer : IDisposable
{
    private readonly IProducer<string, string> _producer;
    
    public Producer(ProducerConfig config)
    {
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public Task<DeliveryResult<string, string>> ProduceAsync(string topic, Message<string, string> message)
    {
        return _producer.ProduceAsync("test", new Message<string, string> { Key = "key", Value = "value" });
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}