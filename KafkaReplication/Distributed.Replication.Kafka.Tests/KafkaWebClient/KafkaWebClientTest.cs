using Confluent.Kafka;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest : BaseDockerTest
{
    protected IAdminClient KafkaAdminClient { get; private set; }
    protected const int KafkaMetdataRefreshIntervalMs = 5000;

    [OneTimeSetUp]
    public void InitAdminClients()
    {
        TestContext.Progress.WriteLine($"Starting initialization of Kafka and Docker admin clients");
        InitKafkaAdminClient();
        InitHttpClient();
    }

    [OneTimeTearDown]
    public void DisposeClients()
    {
        TestContext.Progress.WriteLine($"Disposing Kafka and Docker admin clients");
        KafkaAdminClient.Dispose();
        DisposeHttpClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        TestContext.Progress.WriteLine($"Removing containers hosting Kafka clients and restoring Kafka cluster network");
        // TODO: remove unused volumes
        await ForceRemoveKafkaClients();
        await RestoreKafkaClusterNetwork();
    }

    [SetUp]
    public async Task SetUp()
    {
        TestContext.Progress.WriteLine($"Removing containers hosting Kafka clients");
        await ForceRemoveKafkaClients();
    }

    private void InitKafkaAdminClient()
    {
        KafkaAdminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = TestEnvironment.KafkaCluster.ExternalListeners,
            TopicMetadataRefreshIntervalMs = KafkaMetdataRefreshIntervalMs,
            TopicMetadataRefreshSparse = false,
            MetadataMaxAgeMs = KafkaMetdataRefreshIntervalMs * 2,
        }).Build();
    }

    private async Task RestoreKafkaClusterNetwork()
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
            await DockerClient.Networks.ConnectNetworkAsync(network, new NetworkConnectParameters { Container = container.ID });
            TestContext.Progress.WriteLine($"Added network '{network}' to kafka container '{container.Names[0]}'");
        }
    }
}