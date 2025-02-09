using Distributed.Replication.Kafka.Tests.KafkaWebClient;
using Distributed.Replication.Kafka.Tests.Utils;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath => "../../../../";
    
    private readonly string _producerPort = "5117";
    private readonly string _consumerPort = "5118";

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        await StartProducer("producer-1", _producerPort, $"bootstrap.servers={ComposeConstants.KafkaHosts}");
        await StartConsumer("consumer-1", _consumerPort, $"bootstrap.servers={ComposeConstants.KafkaHosts}");
    }

    [Test]
    public async Task TestA()
    {
        var httpClient = new HttpClient();
        
        await TestUtil.WaitUntilAsync(timeoutMs: (int)TimeSpan.FromMinutes(5).TotalMilliseconds, async () =>
            (await httpClient.GetAsync($"http://localhost:{_producerPort}/health")).IsSuccessStatusCode);

        Console.WriteLine("Producer is healthy");
    }
}