namespace Distributed.Replication.Kafka.Tests;

// TODO: find the way to propagate settings avoiding duplication in Compose file and in the proj
internal static class ComposeConstants
{
    internal static class KafkaCluster
    {
        public const string InternalListeners = "kafka-1:9092,kafka-2:9092,kafka-3:9092";
        public const string ExternalListeners = "127.0.0.1:19092,127.0.0.1:19093,127.0.0.1:19094";
    }
    
    internal static class Network
    {
        public const string EuropeWest1 = "europe-west1";
        public const string EuropeWest2 = "europe-west2";
        public const string EuropeWest3 = "europe-west3";
    }

    internal static class KafkaWebClient
    {
        public const string ImageName = "test-web-client";
        public const string AssemblyName = "Distributed.Replication.Kafka.TestWebClient.dll";
        public const string InternalPort = "8080";
    }
}