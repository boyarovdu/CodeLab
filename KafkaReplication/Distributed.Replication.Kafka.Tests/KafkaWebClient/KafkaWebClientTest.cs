using Confluent.Kafka;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest : BaseDockerTest
{
    protected IAdminClient KafkaAdminClient { get; private set; }

    [SetUp]
    public async Task SetUp()
    {
        await ForceRemoveKafkaClients();
        InitKafkaAdminClient();
        InitHttpClient();
    }

    [TearDown]
    public async Task TearDown()
    {
        // TODO: remove unused volumes

        await ForceRemoveKafkaClients();
        KafkaAdminClient.Dispose();
        DisposeHttpClient();
    }
    
    private void InitKafkaAdminClient() =>
        KafkaAdminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = TestEnvironment.KafkaCluster.ExternalListeners
        }).Build();
}

