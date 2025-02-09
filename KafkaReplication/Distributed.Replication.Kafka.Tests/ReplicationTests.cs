using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Distributed.Replication.Kafka.Tests.Utils;

using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath => "../../../../";

    private readonly string _kafkaHosts = "kafka-1:9092,kafka-2:9092,kafka-3:9092";

    private readonly string _producerPort = "5117";
    private readonly string _consumerPort = "5118";

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await StartProducer("producer-1", _producerPort, $"bootstrap.servers={_kafkaHosts}");
        await StartConsumer("consumer-1", _consumerPort, $"bootstrap.servers={_kafkaHosts}");
    }

    [Test]
    public async Task TestA()
    {
        var httpClient = new HttpClient();

        await TestUtil.WaitUntilAsync(timeoutMs: 300_000, async () =>
            (await httpClient.GetAsync($"http://localhost:{_producerPort}/health")).IsSuccessStatusCode);

        
        
        Console.WriteLine("Producer is healthy");
    }
}