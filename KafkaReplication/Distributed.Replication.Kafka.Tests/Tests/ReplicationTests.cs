using System.Net.Http.Headers;
using System.Text.Json;

using Confluent.Kafka.Admin;

using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Distributed.Replication.Kafka.TestWebClient.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath =>
        "../../../../"; // way up from the test project to compose file folder

    private readonly string _producerPort = "5117";
    private readonly string _consumerPort = "5118";

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await StartConsumer("consumer", _consumerPort,
            $"bootstrap.servers={ComposeConstants.KafkaCluster.InternalListeners}",
            $"group.id={Guid.NewGuid()}",
            "auto.offset.reset=earliest");

        await StartProducer("producer", _producerPort, 
            $"bootstrap.servers={ComposeConstants.KafkaCluster.InternalListeners}");
        
        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task TestExample()
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

    [Test]
    public async Task AsyncReplication()
    {
        var topic = "test-topic";
        await KafkaAdminClient.CreateTopicsAsync([
            new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 2 }
        ]);
        
        using HttpClient httpClient = new();
        
        _ = await SubscribeAsync(httpClient, _consumerPort, topic);
        _ = await ProduceAsync(httpClient, _producerPort, topic, "test-message");
        
        var consumeResp = await ConsumeAsync(httpClient, _consumerPort);
        if (!consumeResp.IsSuccessStatusCode)
        {
            throw new Exception("Consume failed");
        }
        
        var metadata = KafkaAdminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topic);
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