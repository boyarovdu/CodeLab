using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class Tests : BaseDockerTest
{
    protected override string DockerComposeFolderPath => "/home/david/Documents/Cluster";

    [Test]
    public async Task TestA()
    {
        var @params = new ContainersListParameters { All = true };
        var containers = await _dockerClient.Containers.ListContainersAsync(@params);

        var msg = String.Join(',', containers.Select(c => c.Names.First()));

        TestContext.Progress.WriteLine($"containers: {msg}");
    }
}