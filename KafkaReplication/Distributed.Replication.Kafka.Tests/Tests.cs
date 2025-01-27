using Confluent.Kafka;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class Tests : BaseDockerTest
{
    protected override string DockerComposeFolderPath => "./Docker/KafkaCluster";

    [Test]
    public async Task TestA()
    {
        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        var msg = string.Join(',', containers.Select(c => c.Names.First()));

        var config = new ProducerConfig
        {
            BootstrapServers = "localhost:19092,localhost:19093,localhost:19094",
            Acks = Acks.All
        };

        using (var producer = new ProducerBuilder<string, string>(config).Build())
        {
            var deliveryReport = await producer.ProduceAsync("test-topic",
                new Message<string, string> { Key = "user", Value = "item" });
            
            Console.WriteLine("Produced event to the specified topic");
        }

        TestContext.Progress.WriteLine($"containers: {msg}");
    }
}