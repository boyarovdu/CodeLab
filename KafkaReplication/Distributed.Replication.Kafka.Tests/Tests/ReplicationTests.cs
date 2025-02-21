using Confluent.Kafka;
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

        await ServiceHealthy(_producerPort);
    }

    [Test]
    public async Task Replica_removed_from_ISR_when_unavailable()
    {
        var topicName = "test-topic";
        var topicMetadata = (await KafkaAdminClient.CreateTopicAsync(topicName, replicationFactor: 2));
        
        var leaderId = topicMetadata.Partitions[0].Leader;
        var followerId = topicMetadata.Partitions[0].Replicas.Except([leaderId]).First();

        _ = await SubscribeAsync(_consumerPort, topicName);

        // Replica is disconnected so it cannot acknowledge partition leader about receiving the message
        await DisconnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerByBrokerId(followerId)]);

        // Replica removed from ISR after 10 seconds(default value)
        // replica.lag.time.max.ms - setting that defines after what period of time replica is being removed from ISR
        await TestUtil.WaitUntilAsync(
            timeoutMs: 60_000,
            condition: () =>
                KafkaAdminClient.GetTopicMetadata(topicName).Partitions[0].InSyncReplicas.Contains(followerId) == false,
            delay: 1000);

        // Producer won't fail to produce, regardless of replication factor 2 for the topic and strong durability guarantees(acks=all) of the producer 
        // This is because min.insync.replicas was not adjusted and its default value is 1
        var produceResponse = await ProduceAsync(_producerPort, topicName, "test-message");
        Assert.That(produceResponse.IsSuccessStatusCode);

        var consumeResp = await ConsumeAsync(_consumerPort);
        Assert.That(consumeResp.IsSuccessStatusCode);
        
        await ConnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerByBrokerId(followerId)]);
    }

    [Test]
    public async Task Replica_removed_from_ISR_when_unavailable_2()
    {
        var topicName = "test-topic-2";
        var topicMetadata = await KafkaAdminClient.CreateTopicAsync(topicName, replicationFactor: 2, 
            config: new() { ["min.insync.replicas"] = "2" });
        
        var leaderId = topicMetadata.Partitions[0].Leader;
        var followerId = topicMetadata.Partitions[0].Replicas.Except([leaderId]).First();

        _ = await SubscribeAsync(_consumerPort, topicName);

        // Replica is disconnected so it cannot acknowledge partition leader about receiving the message
        await DisconnectAsync(TestEnvironment.Network.Internal,
            [TestEnvironment.KafkaCluster.GetContainerByBrokerId(followerId)]);

        // Replica removed from ISR after 10 seconds(default value)
        // replica.lag.time.max.ms - setting that defines after what period of time replica is being removed from ISR
        await TestUtil.WaitUntilAsync(
            timeoutMs: 60_000,
            condition: () =>
                KafkaAdminClient.GetTopicMetadata(topicName).Partitions[0].InSyncReplicas.Contains(followerId) == false,
            delay: 1000);

        // TODO: add meaningful comment
        Assert.ThrowsAsync<TaskCanceledException>(() => ProduceAsync(_producerPort, topicName, "test-message"));
    }
}