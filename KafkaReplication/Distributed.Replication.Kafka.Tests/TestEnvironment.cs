namespace Distributed.Replication.Kafka.Tests;

// TODO: find the way to propagate settings avoiding duplication in Compose file and in the proj
internal static class TestEnvironment
{
    internal static class KafkaCluster
    {
        public const string InternalListeners = "kafka-1.internal:9092,kafka-2.internal:9092,kafka-3.internal:9092";
        public const string PublicListeners = "kafka-1.public:29092,kafka-2.public:29093,kafka-3.public:29094";
        public const string ExternalListeners = "127.0.0.1:19092,127.0.0.1:19093,127.0.0.1:19094";
        
        public const string Kafka1 = "kafka-1";
        public const string Kafka2 = "kafka-2";
        public const string Kafka3 = "kafka-3";

        public static string GetContainerByBrokerId(int brokerId)
        { 
            return $"kafka-{brokerId}";
        }
    }
    
    internal static class Network
    {
        public const string Internal = "internal";
        public const string Public = "public";
    }

    internal static class KafkaWebClient
    {
        public const string ImageName = "test-web-client";
        public const string AssemblyName = "Distributed.Replication.Kafka.TestWebClient.dll";
        public const string InternalPort = "8080";
    }
}