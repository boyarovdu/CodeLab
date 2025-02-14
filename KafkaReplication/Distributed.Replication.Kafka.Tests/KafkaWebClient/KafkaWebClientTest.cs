using Distributed.Replication.Kafka.Tests.Utils;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public class KafkaWebClientTest : BaseDockerTest
{
    [OneTimeSetUp]
    [OneTimeTearDown]
    public async Task ForceRemoveKafkaClients()
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
    
    private async Task<bool> StartKafkaClient(KafkaClientType type, string containerName, string port, params string[] config) =>
        await CreateStartContainer(new ContainerParamsBuilder()
            .WithKafkaTestWebClient(
                clientType: type,
                kafkaConfig: config)
            .WithPortBinding(ComposeConstants.KafkaWebClient.InternalPort, port)
            .WithName(containerName)
            .WithNetwork(ComposeConstants.DefaultNetwork)
            .Build());

    private async Task<bool> CreateStartContainer(CreateContainerParameters containerParams)
    {
        var container = await DockerClient.Containers.CreateContainerAsync(containerParams);
        return await DockerClient.Containers.StartContainerAsync(container.ID, new());
    }
}