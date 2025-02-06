namespace Distributed.Replication.Kafka.Tests.Builders;

public enum KafkaClientType
{
    Producer,
    Consumer,
}

public static class KafkaTestWebClientBuilder
{
    public static ContainerParamsBuilder WithKafkaTestWebClient(this ContainerParamsBuilder containerParameters,
        KafkaClientType clientType, string kafkaConfig) =>
        containerParameters
            .WithImage("test-web-client")
            .WithCommand("dotnet", "Distributed.Replication.Kafka.TestWebClient.dll")
            .WithCommand("--client-type", clientType == KafkaClientType.Producer ? "producer" : "consumer")
            .WithCommand("--kafka-config", kafkaConfig);
}