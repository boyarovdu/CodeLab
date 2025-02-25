using Distributed.Replication.Kafka.Tests.Utils;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

public class BaseDockerTest
{
    protected virtual string DockerComposeFolderPath => "./";
    protected virtual string[] DockerComposeCommands => [];
    protected virtual bool StartContainersAutomatically => true;

    protected DockerClient DockerClient { get; private set; }

    [SetUp]
    public async Task ComposeUp()
    {
        TestContext.Progress.WriteLine($"Starting Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose",
            $"up -d --build " +
            $"{(StartContainersAutomatically ? string.Empty : "--no-start")} " +
            $"{string.Join(' ', DockerComposeCommands)}");

        DockerClient = new DockerClientConfiguration().CreateClient();
    }

    [TearDown]
    public async Task ComposeDown()
    {
        TestContext.Progress.WriteLine($"Stopping Docker Compose for {GetType().Name}...");
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker-compose", "down -v");

        DockerClient.Dispose();
    }
}