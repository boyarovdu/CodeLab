using Distributed.Replication.Kafka.Tests.Utils;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

public class BaseDockerTest
{
    protected virtual string DockerComposeFolderPath => "./";
    protected DockerClient DockerClient { get; private set; }
    
    [SetUp]
    public async Task ComposeUp()
    {
        TestContext.Progress.WriteLine($"Starting Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "up -d --build");
            
        DockerClient = new DockerClientConfiguration().CreateClient();
    }

    [TearDown]
    public async Task ComposeDown()
    {   
        TestContext.Progress.WriteLine($"Stopping Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "down -v");
            
        DockerClient.Dispose();
    }

    protected async Task<bool> CreateStartContainer(CreateContainerParameters containerParams)
    {
        var container = await DockerClient.Containers.CreateContainerAsync(containerParams);
        return await DockerClient.Containers.StartContainerAsync(container.ID, new());
    }
}