using NUnit.Framework;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

public class BaseDockerTest
{
    protected virtual string DockerComposeFolderPath => "./";
    protected DockerClient DockerClient;

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
}