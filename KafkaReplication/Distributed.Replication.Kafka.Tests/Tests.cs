using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class Tests : BaseDockerTest
{
    protected override string DockerComposeFolderPath => "../../../../";

    [Test]
    public async Task TestA()
    {
        var @params = new ContainersListParameters { All = true };
        var containers = await DockerClient.Containers.ListContainersAsync(@params);
        var msg = string.Join(',', containers.Select(c => c.Names.First()));
        
        Console.WriteLine("Produced event to the specified topic");

        await TestContext.Progress.WriteLineAsync($"containers: {msg}");
    }
}