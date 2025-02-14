using Distributed.Replication.Kafka.Tests.Utils;
using Docker.DotNet;

namespace Distributed.Replication.Kafka.Tests;

public class BaseDockerTest
{
    protected virtual string DockerComposeFolderPath => "./";
    protected DockerClient DockerClient { get; private set; }

    [OneTimeSetUp]
    public async Task ComposeUp()
    {
        await TestContext.Progress.WriteLineAsync($"Starting Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "up -d --build");
            
        DockerClient = new DockerClientConfiguration().CreateClient();
    }

    [OneTimeTearDown]
    public async Task ComposeDown()
    {   
        await TestContext.Progress.WriteLineAsync($"Stopping Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "down");
            
        DockerClient.Dispose();
    }
}