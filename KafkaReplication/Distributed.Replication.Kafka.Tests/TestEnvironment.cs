namespace Distributed.Replication.Kafka.Tests;

// TODO: find the way to propagate settings avoiding duplication in Compose file and in the proj
internal static class TestEnvironment
{
    internal static class KafkaCluster
    {
        // Kafka listeners
        public const string InternalListeners = "kafka-1.internal:9092,kafka-2.internal:9092,kafka-3.internal:9092";
        public const string PublicListeners = "kafka-1.public:29092,kafka-2.public:29093,kafka-3.public:29094";
        public const string ExternalListeners = "127.0.0.1:19092,127.0.0.1:19093,127.0.0.1:19094";
        
        // Container names
        private const string KafkaContainerPrefix = "kafka-";
        public const string Kafka1 = $"{KafkaContainerPrefix}1";
        public const string Kafka2 = $"{KafkaContainerPrefix}2";
        public const string Kafka3 = $"{KafkaContainerPrefix}3";
        
        public const string Zookeeper = "zookeeper";
        public const string Zoonavigator = "zoonavigator";
        
        public const int BrokersCount = 3;

        public static string GetContainerNameByBrokerId(int brokerId)
        { 
            return $"{KafkaContainerPrefix}{brokerId}";
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
        public const string Foo = "foo";
    }
}