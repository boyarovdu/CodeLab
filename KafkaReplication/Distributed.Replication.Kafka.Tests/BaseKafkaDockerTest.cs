using NUnit.Framework;
using System.Diagnostics;
using Distributed.Replication.Kafka.Tests.Builders;
using Distributed.Replication.Kafka.Tests.Helpers;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

public class BaseKafkaDockerTest
{
    protected virtual string DockerComposeFolderPath => "./";
    protected DockerClient DockerClient { get; private set; }

    [OneTimeSetUp]
    public async Task StartDockerCompose()
    {
        await TestContext.Progress.WriteLineAsync($"Starting Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "up -d");
            
        DockerClient = new DockerClientConfiguration().CreateClient();
    }

    [OneTimeTearDown]
    public async Task StopDockerCompose()
    {
        await TestContext.Progress.WriteLineAsync($"Stopping Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "down");
            
        DockerClient.Dispose();
    }

    protected async Task<bool> StartConsumer(string containerName, int port, string config) =>
        await StartKafkaClient(KafkaClientType.Consumer, containerName, port, config);

    protected async Task<bool> StartProducer(string containerName, int port, string config) =>
        await StartKafkaClient(KafkaClientType.Producer, containerName, port, config);

    private async Task<bool> StartKafkaClient(KafkaClientType type, string containerName, int port, string config) =>
        await CreateStartContainer(new ContainerParamsBuilder()
            .WithKafkaTestWebClient(
                clientType: type,
                kafkaConfig: config)
            .WithPortBinding("8080", port.ToString())
            .WithName(containerName)
            .Build());

    private async Task<bool> CreateStartContainer(CreateContainerParameters containerParams)
    {
        var container = await DockerClient.Containers.CreateContainerAsync(containerParams);
        return await DockerClient.Containers.StartContainerAsync(container.ID, new());
    }
}