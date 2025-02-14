using Distributed.Replication.Kafka.Tests.Utils;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public static class KafkaWebClientBuilder
{
    public static ContainerParamsBuilder WithKafkaTestWebClient(this ContainerParamsBuilder containerParameters,
        KafkaClientType clientType, params string[] kafkaConfig)
    {
        var containerParams = containerParameters
            .WithImage(ComposeConstants.KafkaWebClient.ImageName)
            .WithCommand("dotnet", ComposeConstants.KafkaWebClient.AssemblyName)
            .WithCommand("--client-type", clientType == KafkaClientType.Producer ? "producer" : "consumer");

        foreach (var kafkaSetting in kafkaConfig)
        {
            containerParams.WithCommand("--kafka-config", kafkaSetting);
        }

        return containerParams;
    }
}