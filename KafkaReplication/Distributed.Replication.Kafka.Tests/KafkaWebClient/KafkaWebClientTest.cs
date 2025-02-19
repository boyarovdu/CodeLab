using Confluent.Kafka;
using Docker.DotNet.Models;

using Distributed.Replication.Kafka.Tests.Docker;
using Distributed.Replication.Kafka.Tests.Utils;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public class KafkaWebClientTest : BaseDockerTest
{
    protected IAdminClient KafkaAdminClient { get; private set; }

    private void InitKafkaAdminClient() =>
        KafkaAdminClient = new AdminClientBuilder(new AdminClientConfig
        {
            BootstrapServers = ComposeConstants.KafkaCluster.ExternalListeners
        }).Build();
    
    [OneTimeSetUp]
    public async Task SetUp()
    {
        await ForceRemoveKafkaClients();
        InitKafkaAdminClient();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        // TODO: remove unused volumes
        
        await ForceRemoveKafkaClients();
        KafkaAdminClient.Dispose();
    }

    private async Task ForceRemoveKafkaClients()
    {
        await TestContext.Progress.WriteLineAsync($"Removing containers hosting Kafka clients for {GetType().Name}...");

        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        foreach (var container in containers.Where(c => c.Image == ComposeConstants.KafkaWebClient.ImageName))
        {
            await DockerClient.Containers.RemoveContainerAsync(container.ID,
                new ContainerRemoveParameters { Force = true });
        }
    }

    protected async Task<bool> StartConsumer(string containerName, string port, params string[] config) =>
        await StartKafkaClient(KafkaClientType.Consumer, containerName, port, config);

    protected async Task<bool> StartProducer(string containerName, string port, string config) =>
        await StartKafkaClient(KafkaClientType.Producer, containerName, port, config);

    protected static async Task ServiceHealthy(string producerPort,
        int timeoutMs = (5 * 60 * 1000),
        string host = "localhost",
        string endpointRelativePath = "health")
    {
        using var httpClient = new HttpClient();

        var healthUri = new Uri($"http://{host}:{producerPort}/{endpointRelativePath}");

        await TestUtil.WaitUntilAsync(timeoutMs, async () =>
            (await httpClient.GetAsync(healthUri)).IsSuccessStatusCode);
    }

    private async Task<bool> StartKafkaClient(KafkaClientType type, string containerName, string port,
        params string[] config) =>
        await CreateStartContainer(new ContainerParamsBuilder()
            .WithKafkaTestWebClient(
                clientType: type,
                kafkaConfig: config)
            .WithPortBinding(ComposeConstants.KafkaWebClient.InternalPort, port)
            .WithName(containerName)
            .WithNetworks(ComposeConstants.Network.EuropeWest1, 
                ComposeConstants.Network.EuropeWest2, 
                ComposeConstants.Network.EuropeWest3)
            .Build());
}