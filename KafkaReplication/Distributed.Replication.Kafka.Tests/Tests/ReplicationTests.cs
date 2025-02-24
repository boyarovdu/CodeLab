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

    [SetUp]
    public async Task StartKafkaClients()
    {
        await StartProducer("producer", _producerPort,
            [TestEnvironment.Network.Internal],
            [
                $"bootstrap.servers={TestEnvironment.KafkaCluster.InternalListeners}",
                $"acks=all"
            ]);

        await StartConsumer("consumer", _consumerPort,
            [TestEnvironment.Network.Public],
            [
                $"bootstrap.servers={TestEnvironment.KafkaCluster.PublicListeners}",
                $"group.id={Guid.NewGuid()}",
                "auto.offset.reset=earliest"
            ]);

        TestContext.Progress.WriteLine("Waiting when producer is healthy.");
        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task Replica_removed_from_ISR_when_unavailable()
    {
        var topicName = "test-topic";
        TestContext.Progress.WriteLine($"Creating topic {topicName}");
        var topicMetadata = (await KafkaAdminClient.CreateTopicAsync(topicName, replicationFactor: 2));

        var leaderId = topicMetadata.Partitions[0].Leader;
        var followerId = topicMetadata.Partitions[0].InSyncReplicas.Except([leaderId]).First();

        _ = await SubscribeAsync(_consumerPort, topicName);

        // Replica is disconnected so it cannot acknowledge partition leader about receiving the message
        await DisconnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerByBrokerId(followerId)]);

        // Replica removed from ISR after 10 seconds(default value)
        // replica.lag.time.max.ms - setting that defines after what period of time replica is being removed from ISR
        TestContext.Progress.WriteLine("Waiting when replica removed from ISR.");
        await TestUtil.WaitUntilAsync(
            timeoutMs: 5 * 60_000,
            condition: () =>
            {
                var inSyncReplicas = KafkaAdminClient.GetTopicMetadata(topicName).Partitions[0].InSyncReplicas;
                TestContext.Progress.WriteLine($"Replicas: {string.Join(", ", topicMetadata)}");
                return inSyncReplicas.Contains(followerId) == false;
            },
            delay: KafkaMetdataRefreshIntervalMs);

        // Producer won't fail to produce, regardless of replication factor 2 for the topic and strong durability guarantees(acks=all) of the producer 
        // This is because min.insync.replicas was not adjusted and its default value is 1
        TestContext.Progress.WriteLine("Producing message.");
        var produceResponse = await ProduceAsync(_producerPort, topicName, "test-message");
        Assert.That(produceResponse.IsSuccessStatusCode);

        // The consumer, depending on what broker it is connected to, may not receive message while replica cannot connect to the leader
        TestContext.Progress.WriteLine("Waiting when consumer receives message.");
        var consumeResp = await ConsumeAsync(_consumerPort);
        TestContext.Progress.WriteLine(consumeResp.IsSuccessStatusCode
            ? "Consumer received message."
            : "Consumer did not receive message.");
        
        // Connecting replica back
        await ConnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerByBrokerId(followerId)]);

        if (!consumeResp.IsSuccessStatusCode)
        {
            // Replica must be added back to ISR after restoring the network
            TestContext.Progress.WriteLine("Waiting when replica added back to ISR.");
            await TestUtil.WaitUntilAsync(
                timeoutMs: 5 * 60_000,
                condition: () =>
                {
                    var inSyncReplicas = KafkaAdminClient.GetTopicMetadata(topicName).Partitions[0].InSyncReplicas;
                    TestContext.Progress.WriteLine($"Replicas: {string.Join(", ", topicMetadata)}");
                    return inSyncReplicas.Contains(followerId);
                },
                delay: KafkaMetdataRefreshIntervalMs);
            
            // After replica connected back - consumer MUST receive the message
            await TestContext.Progress.WriteLineAsync("Waiting when consumer receives message.");
            consumeResp = await ConsumeAsync(_consumerPort);
            Assert.That(consumeResp.IsSuccessStatusCode);
        }
    }
}