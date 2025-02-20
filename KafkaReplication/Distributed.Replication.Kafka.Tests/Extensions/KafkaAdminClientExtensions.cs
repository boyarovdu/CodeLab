using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Distributed.Replication.Kafka.Tests.Extensions;

public static class KafkaAdminClientExtensions
{
    public static async Task<PartitionMetadata[]> CreateTopicAsync(this IAdminClient adminClient, string topicName,
        short replicationFactor = 1,
        int partitions = 1)
    {
        await adminClient.CreateTopicsAsync([
            new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor
            }
        ]);

        return adminClient.GetPartitions(topicName);
    }
    
    public static PartitionMetadata[] GetPartitions(this IAdminClient adminClient, string topicName)
    {
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);

        if (topicMetadata == null) throw new Exception($"Topic {topicName} not found");
        return topicMetadata.Partitions.ToArray();
    }
}