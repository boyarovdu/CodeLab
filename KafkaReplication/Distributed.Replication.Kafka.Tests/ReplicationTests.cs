using System.Net.Http.Headers;
using System.Text.Json;

using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Distributed.Replication.Kafka.TestWebClient.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath => "../../../../";

    private readonly string _producerPort = "5117";
    private readonly string _consumerPort = "5118";

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await StartConsumer("consumer", _consumerPort, $"bootstrap.servers={ComposeConstants.KafkaHosts}",
            $"group.id={Guid.NewGuid()}");
        await StartProducer("producer", _producerPort, $"bootstrap.servers={ComposeConstants.KafkaHosts}");
        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task TestA()
    {
        var topic = "test-topic";
        using HttpClient client = new();

        var subscribeResp = await client.PostAsync(
            $"http://localhost:{_consumerPort}/consume?topic={topic}",
            null);
        
        var produceResp = await client.PostAsync(
            $"http://localhost:{_producerPort}/produce?topic={topic}",
            new StringContent(
                JsonSerializer.Serialize(new ProduceRequest { Message = "test-message" }),
                mediaType: new MediaTypeHeaderValue("application/json")
            ));
        
        if (subscribeResp.IsSuccessStatusCode)
        {
            var consumeResp = await client.GetAsync($"http://localhost:{_consumerPort}/consume");
            Console.WriteLine(consumeResp.Content.ReadAsStringAsync().GetAwaiter().GetResult());
        }

        Console.WriteLine("Producer is healthy");
    }
}