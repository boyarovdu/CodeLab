using Confluent.Kafka;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest : BaseDockerTest
{
    protected IAdminClient KafkaAdminClient { get; private set; }
    protected const int KafkaMetdataRefreshIntervalMs = 5000;

    [SetUp]
    public async Task InitClients()
    {
        TestContext.Progress.WriteLine($"Starting initialization of admin clients");
        await InitKafkaAdminClient();
        InitHttpClient();
        
        TestContext.Progress.WriteLine($"Removing containers hosting Kafka clients");
        await ForceRemoveKafkaClients();
        TestContext.Progress.WriteLine($"Restoring kafka cluster networks");
        await RestoreKafkaClusterNetwork();
    }

    [TearDown]
    public async Task DisposeClients()
    {
        TestContext.Progress.WriteLine($"Disposing admin clients");
        KafkaAdminClient.Dispose();
        DisposeHttpClient();

        TestContext.Progress.WriteLine($"Removing containers hosting Kafka clients");
        await ForceRemoveKafkaClients();
    }

    private async Task InitKafkaAdminClient()
    {
        KafkaAdminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = TestEnvironment.KafkaCluster.ExternalListeners,
            TopicMetadataRefreshIntervalMs = KafkaMetdataRefreshIntervalMs,
            // TopicMetadataRefreshSparse = false,
            // MetadataMaxAgeMs = KafkaMetdataRefreshIntervalMs * 2,
        }).Build();

        TestContext.Progress.WriteLine($"Waiting when all brokers are registered in Kafka cluster.");
        await TestUtil.WaitUntilAsync(
            timeoutMs: 5 * 60_000,
            condition: () =>
                KafkaAdminClient.GetMetadata(TimeSpan.FromSeconds(10)).Brokers.Count ==
                TestEnvironment.KafkaCluster.BrokersCount,
            delay: KafkaMetdataRefreshIntervalMs);
    }

    protected async Task RestoreKafkaClusterNetwork()
    {
        string[] kafkaNetworks = [TestEnvironment.Network.Internal, TestEnvironment.Network.Public];

        var listParams = new ContainersListParameters
        {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>>()
        };

        listParams.Filters.Add("name", new Dictionary<string, bool>
        {
            { TestEnvironment.KafkaCluster.Kafka1, true },
            { TestEnvironment.KafkaCluster.Kafka2, true },
            { TestEnvironment.KafkaCluster.Kafka3, true }
        });

        var containers = await DockerClient.Containers.ListContainersAsync(listParams);
        foreach (var container in containers)
        foreach (var network in kafkaNetworks)
        {
            if (container.NetworkSettings.Networks.ContainsKey(network)) continue;
            await DockerClient.Networks.ConnectNetworkAsync(network,
                new NetworkConnectParameters { Container = container.ID });
            TestContext.Progress.WriteLine($"Added network '{network}' to kafka container '{container.Names[0]}'");
        }
    }
}