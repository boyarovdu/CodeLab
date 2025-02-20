using Distributed.Replication.Kafka.Tests.Extensions;
using Distributed.Replication.Kafka.Tests.KafkaWebClient;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class ReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath =>
        "../../../../"; // way up from the test project to compose file folder

    private readonly string _producerPort = "5117";
    private readonly string _consumerPort = "5118";

    private readonly string _topic = "test-topic";

    private int _leader = 0;
    private int _follower = 0;

    private string consumerGroup = Guid.NewGuid().ToString();

    [OneTimeSetUp]
    public async Task StartKafkaClients()
    {
        var partition = (await KafkaAdminClient.CreateTopicAsync(_topic, replicationFactor: 2))[0];
        _leader = partition.Leader;
        _follower = partition.Replicas.Except([partition.Leader]).First();

        await StartProducer("producer", _producerPort,
            [TestEnvironment.Network.Internal],
            [$"bootstrap.servers={TestEnvironment.KafkaCluster.InternalListeners}",
            $"acks=all"]);

        await StartConsumer("consumer", _consumerPort,
            [TestEnvironment.Network.Public],
            [
                $"bootstrap.servers={TestEnvironment.KafkaCluster.PublicListeners}",
                $"group.id={consumerGroup}",
                $"client.rack={TestEnvironment.Network.Public}",
                "auto.offset.reset=earliest"
            ]);

        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task AsyncReplication()
    {
        _ = await SubscribeAsync(_consumerPort, _topic);
        _ = await ProduceAsync(_producerPort, _topic, "test-message");

        var consumeResp = await ConsumeAsync(_consumerPort);
        if (!consumeResp.IsSuccessStatusCode)
        {
            throw new Exception("Consume failed");
        }
    }
}