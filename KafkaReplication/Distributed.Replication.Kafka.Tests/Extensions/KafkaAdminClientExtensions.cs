using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Distributed.Replication.Kafka.Tests.Extensions;

public static class KafkaAdminClientExtensions
{
    public static async Task<TopicMetadata> CreateTopicAsync(this IAdminClient adminClient, 
        string topicName,
        short replicationFactor = 1,
        int partitions = 1,
        Dictionary<string, string> config = null)
    {
        await adminClient.CreateTopicsAsync([
            new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor,
                Configs = config
            }
        ]);

        return adminClient.GetMetadata(TimeSpan.FromSeconds(10))
            .Topics
            .FirstOrDefault(t => t.Topic == topicName)!;
    }
    
    public static TopicMetadata GetTopicMetadata(this IAdminClient adminClient, string topicName)
    {
        var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));
        var topicMetadata = metadata.Topics.FirstOrDefault(t => t.Topic == topicName);

        if (topicMetadata == null) throw new Exception($"Topic {topicName} not found");
        return topicMetadata;
    }
}