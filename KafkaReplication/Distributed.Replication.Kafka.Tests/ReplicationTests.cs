using System.Net.Http.Headers;
using System.Text.Json;

using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Distributed.Replication.Kafka.Tests.Utils;
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
        await StartConsumer("consumer", _consumerPort, 
            $"bootstrap.servers={ComposeConstants.KafkaHosts}",
            $"group.id={Guid.NewGuid()}", 
            "auto.offset.reset=earliest");
        
        await StartProducer("producer", _producerPort, $"bootstrap.servers={ComposeConstants.KafkaHosts}");
        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task TestA()
    {
        var topic = "test-topic";
        using HttpClient httpClient = new();

        _ = await SubscribeAsync(httpClient, _consumerPort, topic);
        _ = await ProduceAsync(httpClient, _producerPort, topic, "test-message");
        
        var consumeResp = await ConsumeAsync(httpClient, _consumerPort);
        if (!consumeResp.IsSuccessStatusCode)
        {
            throw new Exception("Consume failed");
        }
    }

    private async Task<HttpResponseMessage> ConsumeAsync(HttpClient client, string port)
    {
        return await client.GetAsync($"http://localhost:{port}/consume");
    }

    private async Task<HttpResponseMessage> ProduceAsync(HttpClient client, string port, string topic, string message)
    {
        return await client.PostAsync(
            $"http://localhost:{port}/produce?topic={topic}",
            new StringContent(
                JsonSerializer.Serialize(new ProduceRequest { Message = message }),
                mediaType: new MediaTypeHeaderValue("application/json")
            ));
    }

    private async Task<HttpResponseMessage> SubscribeAsync(HttpClient client, string port, string topic)
    {
        return await client.PostAsync(
            $"http://localhost:{port}/consume?topic={topic}",
            null);
    }
}