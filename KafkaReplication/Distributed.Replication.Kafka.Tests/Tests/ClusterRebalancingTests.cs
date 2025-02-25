using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

public class ClusterRebalancingTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath =>
        "../../../../"; // way up from the test project to compose file folder

    // With StartContainersAutomatically = false containers from compose file will be created but not started 
    protected override bool StartContainersAutomatically => false;

    private const string ProducerPort = "5117";
    private const string ConsumerPort = "5118";

    [SetUp]
    public async Task StartKafkaClients()
    {
        TestContext.Progress.WriteLine("Starting Kafka cluster");
        await DockerClient.Containers.StartContainerAsync(TestEnvironment.KafkaCluster.Zookeeper, new ContainerStartParameters());
        await DockerClient.Containers.StartContainerAsync(TestEnvironment.KafkaCluster.Zoonavigator, new ContainerStartParameters());
        await DockerClient.Containers.StartContainerAsync(TestEnvironment.KafkaCluster.Kafka1, new ContainerStartParameters());
        await DockerClient.Containers.StartContainerAsync(TestEnvironment.KafkaCluster.Kafka2, new ContainerStartParameters());
        
        TestContext.Progress.WriteLine("Waiting when all brokers are healthy.");
        await TestUtil.WaitUntilAsync(
            timeoutMs: 5 * 60_000,
            condition: () => KafkaAdminClient.GetMetadata(TimeSpan.FromSeconds(10)).Brokers.Count == 2,
            delay: KafkaMetdataRefreshIntervalMs);
    }

    [Test]
    public async Task PlaceholderTest()
    {
        await DockerClient.Containers.StartContainerAsync(TestEnvironment.KafkaCluster.Kafka3, new ContainerStartParameters());
    }
}