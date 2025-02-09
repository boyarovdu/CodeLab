namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

// TODO: find the way to propagate settings avoiding duplication in Compose file and in the proj
internal static class ComposeConstants
{
    public const string KafkaHosts = "kafka-1:9092,kafka-2:9092,kafka-3:9092";
    public const string DefaultNetwork = "kafka-network";
    
    internal static class KafkaWebClient
    {
        public const string ImageName = "test-web-client";
        public const string AssemblyName = "Distributed.Replication.Kafka.TestWebClient.dll";
        public const string InternalPort = "8080";
    }
}