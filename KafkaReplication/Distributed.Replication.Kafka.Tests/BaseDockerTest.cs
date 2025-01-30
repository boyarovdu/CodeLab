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
    public void StartDockerCompose()
    {
        TestContext.Progress.WriteLine($"Starting Docker Compose for {GetType().Name}...");
        CommandLine.Run("docker-compose up -d", DockerComposeFolderPath);
            
        DockerClient = new DockerClientConfiguration().CreateClient();
    }

    [OneTimeTearDown]
    public void StopDockerCompose()
    {
        TestContext.Progress.WriteLine($"Stopping Docker Compose for {GetType().Name}...");
        CommandLine.Run("docker-compose down", DockerComposeFolderPath);
            
        DockerClient.Dispose();
    }
}