using Distributed.Replication.Kafka.Tests.Helpers;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public static class KafkaWebClientBuilder
{
    public static ContainerParamsBuilder WithKafkaTestWebClient(this ContainerParamsBuilder containerParameters,
        KafkaClientType clientType, string kafkaConfig) =>
        containerParameters
            .WithImage(Constants.ImageName)
            .WithCommand("dotnet", Constants.Dll)
            .WithCommand("--client-type", clientType == KafkaClientType.Producer ? "producer" : "consumer")
            .WithCommand("--kafka-config", kafkaConfig);
}