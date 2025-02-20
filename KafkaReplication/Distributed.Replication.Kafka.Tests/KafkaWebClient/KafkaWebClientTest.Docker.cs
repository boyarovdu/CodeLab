using Docker.DotNet.Models;
using Distributed.Replication.Kafka.Tests.Docker;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest
{
    protected async Task<bool> StartConsumer(string containerName, string port, params string[] config) =>
        await StartKafkaClient(KafkaClientType.Consumer, containerName, port, config);

    protected async Task<bool> StartProducer(string containerName, string port, params string[] config) =>
        await StartKafkaClient(KafkaClientType.Producer, containerName, port, config);

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
    
    private async Task ForceRemoveKafkaClients()
    {
        await TestContext.Progress.WriteLineAsync($"Removing containers hosting Kafka clients for {GetType().Name}...");

        var listParams = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(listParams);
        foreach (var container in containers.Where(c => c.Image == ComposeConstants.KafkaWebClient.ImageName))
        {
            await DockerClient.Containers.RemoveContainerAsync(container.ID,
                new ContainerRemoveParameters { Force = true });
        }
    }
}