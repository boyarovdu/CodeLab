using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class Tests : BaseKafkaDockerTest
{
    protected override string DockerComposeFolderPath => "../../../../";
    private readonly string _bootstrapServers = "kafka-1:9092,kafka-2:9092,kafka-3:9092";

    private async Task ForceRemoveKafkaClients()
    {
        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        foreach (var container in containers.Where(c => c.Image == "test-web-client"))
        {
            await DockerClient.Containers.RemoveContainerAsync(container.ID,
                new ContainerRemoveParameters { Force = true });
        }
    }

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await ForceRemoveKafkaClients();

        await StartProducer("producer-1", 5117, $"{_bootstrapServers}");
        await StartConsumer("consumer-1", 5118, $"{_bootstrapServers}");
    }

    [OneTimeTearDown]
    public async Task StopKafkaClients()
    {
        await ForceRemoveKafkaClients();
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