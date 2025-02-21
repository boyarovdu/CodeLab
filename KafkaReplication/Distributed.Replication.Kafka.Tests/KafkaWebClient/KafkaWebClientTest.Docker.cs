using Docker.DotNet.Models;
using Distributed.Replication.Kafka.Tests.Docker;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public partial class KafkaWebClientTest
{
    protected async Task<bool> StartConsumer(string containerName, string port, string[] networks, string[] config) =>
        await StartKafkaClient(KafkaClientType.Consumer, containerName, port, networks, config);

    protected async Task<bool> StartProducer(string containerName, string port, string[] networks, string[] clientConfig) =>
        await StartKafkaClient(KafkaClientType.Producer, containerName, port, networks,
            clientConfig);

    private async Task<bool> StartKafkaClient(KafkaClientType type, string containerName, string port,
        string[] networks, string[] clientConfig) =>
        await CreateStartContainer(new ContainerParamsBuilder()
            .WithKafkaTestWebClient(
                clientType: type,
                kafkaConfig: clientConfig)
            .WithPortBinding(TestEnvironment.KafkaWebClient.InternalPort, port)
            .WithName(containerName)
            .WithNetworks(networks)
            .Build());

    private async Task ForceRemoveKafkaClients()
    {
        await TestContext.Progress.WriteLineAsync($"Removing containers hosting Kafka clients for {GetType().Name}...");

        var listParams = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(listParams);
        foreach (var container in containers.Where(c => c.Image == TestEnvironment.KafkaWebClient.ImageName))
        {
            await DockerClient.Containers.RemoveContainerAsync(container.ID,
                new ContainerRemoveParameters { Force = true });
        }
    }

    public async Task DisconnectAsync(string network, string[] containers)
    {
        foreach (var container in containers)
        {
            await DockerClient.Networks.DisconnectNetworkAsync(network, new NetworkDisconnectParameters
            {
                Force = true,
                Container = container
            });    
        }
    }
    
    public async Task ConnectAsync(string network, string[] containers)
    {
        foreach (var container in containers)
        {
            await DockerClient.Networks.ConnectNetworkAsync(network, new NetworkConnectParameters
            {
                Container = container
            });    
        }
    }
}