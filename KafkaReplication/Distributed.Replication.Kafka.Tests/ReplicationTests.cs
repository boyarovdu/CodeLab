using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath => "../../../../";
    private readonly string _bootstrapServers = "kafka-1:9092,kafka-2:9092,kafka-3:9092";

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await StartProducer("producer-1", "5117", $"{_bootstrapServers}");
        await StartConsumer("consumer-1", "5118", $"{_bootstrapServers}");
    }

    [Test]
    public async Task TestA()
    {
        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        var msg = string.Join(',', containers.Select(c => c.Names.First()));

        Console.WriteLine("Produced event to the specified topic");

        await TestContext.Progress.WriteLineAsync($"containers: {msg}");
    }
}