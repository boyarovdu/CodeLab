using Distributed.Replication.Kafka.Tests.Extensions;
using Distributed.Replication.Kafka.Tests.KafkaWebClient;

namespace Distributed.Replication.Kafka.Tests;

[TestFixture]
public class SynchronousReplicationTests : KafkaWebClientTest
{
    protected override string DockerComposeFolderPath =>
        "../../../../"; // way up from the test project to compose file folder

    private const string ProducerPort = "5117";
    private const string ConsumerPort = "5118";

    [SetUp]
    public async Task StartKafkaClients()
    {
        TestContext.Progress.WriteLine("Waiting when all brokers are healthy.");
        await TestUtil.WaitUntilAsync(
            timeoutMs: 5 * 60_000,
            condition: () =>
                KafkaAdminClient.GetMetadata(TimeSpan.FromSeconds(10)).Brokers.Count ==
                TestEnvironment.KafkaCluster.BrokersCount,
            delay: KafkaMetdataRefreshIntervalMs);
        
        await StartProducer("producer", ProducerPort,
            [TestEnvironment.Network.Internal],
            [
                $"bootstrap.servers={TestEnvironment.KafkaCluster.InternalListeners}",
                $"acks=all"
            ]);

        await StartConsumer("consumer", ConsumerPort,
            [TestEnvironment.Network.Public],
            [
                $"bootstrap.servers={TestEnvironment.KafkaCluster.PublicListeners}",
                $"group.id={Guid.NewGuid()}",
                "auto.offset.reset=earliest"
            ]);

        TestContext.Progress.WriteLine("Waiting when producer is healthy.");
        await ServiceHealthy(ProducerPort);
    }

    [Test]
    public async Task Acks_all_doesnt_guarantee_delivery_when_min_in_sync_replicas_not_adjusted()
    {
        var topicName = "test-topic";
        TestContext.Progress.WriteLine($"Creating topic {topicName}");
        var topicMetadata = (await KafkaAdminClient.CreateTopicAsync(topicName, replicationFactor: 2));

        var leaderId = topicMetadata.Partitions[0].Leader;
        var followerId = topicMetadata.Partitions[0].InSyncReplicas.Except([leaderId]).First();

        _ = await SubscribeAsync(ConsumerPort, topicName);

        // Replica is disconnected so it cannot acknowledge partition leader about receiving the message
        await DisconnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerNameByBrokerId(followerId)]);

        // Producer won't fail to produce, regardless of replication factor 2 for the topic and strong durability guarantees(acks=all) of the producer 
        // This is because min.insync.replicas was not adjusted and its default value is 1
        TestContext.Progress.WriteLine("Producing message.");
        var produceResponse = await ProduceAsync(ProducerPort, topicName, "test-message");
        Assert.That(produceResponse.IsSuccessStatusCode);

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

        // The consumer, depending on what broker it is connected to, may not receive message while replica cannot connect to the leader
        TestContext.Progress.WriteLine("Waiting when consumer receives message.");
        var consumeResp = await ConsumeAsync(ConsumerPort);
        TestContext.Progress.WriteLine(consumeResp.IsSuccessStatusCode
            ? "Consumer received message."
            : "Consumer did not receive message.");

        // Connecting replica back
        await ConnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerNameByBrokerId(followerId)]);

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
            consumeResp = await ConsumeAsync(ConsumerPort);
            Assert.That(consumeResp.IsSuccessStatusCode);
        }
    }

    [Test]
    public async Task Acks_all_guarantees_delivery_when_min_in_sync_replicas_adjusted()
    {
        var topicName = "test-topic";
        TestContext.Progress.WriteLine($"Creating topic {topicName}");

        // Combination of min.insync.replicas = 2 and replicationFactor = 2 provides strong consistency among the replicas
        // Together with producers configured with acks=all, it will provide the highest message delivery guarantees at the cost of performance
        var topicMetadata = await KafkaAdminClient.CreateTopicAsync(topicName, replicationFactor: 2,
            config: new() { ["min.insync.replicas"] = "2" });

        var leaderId = topicMetadata.Partitions[0].Leader;
        var followerId = topicMetadata.Partitions[0].InSyncReplicas.Except([leaderId]).First();

        _ = await SubscribeAsync(ConsumerPort, topicName);

        // Replica is disconnected so it cannot acknowledge partition leader about receiving the message
        await DisconnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerNameByBrokerId(followerId)]);

        // Producer fails with error, because message could not be synced with required quorum of 2 replicas set in min.insync.replicas
        // In such a way code that produces the message will be notified about the failure and will proceed with failover logic
        TestContext.Progress.WriteLine("Producing message.");
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
            await ProduceAsync(ProducerPort, topicName, "test-message"));

        // Connecting replica back
        await ConnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerNameByBrokerId(followerId)]);
        
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

        // Producer won't fail this time because message can be replicated to required amount of replicas
        await ProduceAsync(ProducerPort, topicName, "test-message");
        
        // Consumer receive the message successfully
        TestContext.Progress.WriteLine("Waiting when consumer receives message.");
        var consumeResp = await ConsumeAsync(ConsumerPort);
        Assert.That(consumeResp.IsSuccessStatusCode);
    }
}