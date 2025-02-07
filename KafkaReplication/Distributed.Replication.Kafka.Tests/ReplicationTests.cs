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
        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        var msg = string.Join(',', containers.Select(c => c.Names.First()));

        var httpClient = new HttpClient();

        await TestUtil.WaitUntilAsync(timeoutMs: 10000, async () =>
            (await httpClient.GetAsync($"http://localhost:{_consumerPort}/health")).IsSuccessStatusCode);

        Console.WriteLine("Produced event to the specified topic");

        await TestContext.Progress.WriteLineAsync($"containers: {msg}");
    }
}