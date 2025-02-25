using Confluent.Kafka;

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
    }
}