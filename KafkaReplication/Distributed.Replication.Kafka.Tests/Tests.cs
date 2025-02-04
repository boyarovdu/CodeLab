using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class Tests : BaseDockerTest
{
    protected override string DockerComposeFolderPath => "../../../../";

    [OneTimeSetUp]
#pragma warning disable CS0108, CS0114
    public async Task StartDockerCompose()
#pragma warning restore CS0108, CS0114
    {
        // This command builds 
        // docker run --name consumer-1 -p 5117:8080  distributed.replication.kafka.testwebclient dotnet Distributed.Replication.Kafka.TestWebClient.dll --kafka-config bootstrap.servers=kafka-1:9092,kafka-2:9092,kafka-3:9092 --client-type producer
        // await Cmd.ExecAsync(DockerComposeFolderPath, "docker", "build -t distributed.replication.kafka.testwebclient -f Distributed.Replication.Kafka.TestWebClient/Dockerfile .");
        
        await Cmd.ExecAsync(DockerComposeFolderPath, "docker", "run -d --name consumer-1 -p 5117:8080  test-web-client dotnet Distributed.Replication.Kafka.TestWebClient.dll --kafka-config bootstrap.servers=kafka-1:9092,kafka-2:9092,kafka-3:9092 --client-type producer");
    }
    
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