using Distributed.Replication.Kafka.Tests.Utils;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public static class KafkaWebClientBuilder
{
    public static ContainerParamsBuilder WithKafkaTestWebClient(this ContainerParamsBuilder containerParameters,
        KafkaClientType clientType, string kafkaConfig) =>
        containerParameters
            .WithImage(ComposeConstants.ImageName)
            .WithCommand("dotnet", ComposeConstants.Dll)
            .WithCommand("--client-type", clientType == KafkaClientType.Producer ? "producer" : "consumer")
            .WithCommand("--kafka-config", kafkaConfig);
}