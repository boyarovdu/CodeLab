using Distributed.Replication.Kafka.Tests.Docker;

namespace Distributed.Replication.Kafka.Tests.KafkaWebClient;

public enum KafkaClientType
{
    Producer,
    Consumer,
}

public static class KafkaWebClientBuilder
{
    public static ContainerParamsBuilder WithKafkaTestWebClient(this ContainerParamsBuilder containerParameters,
        KafkaClientType clientType, params string[] kafkaConfig)
    {
        var containerParams = containerParameters
            .WithImage(TestEnvironment.KafkaWebClient.ImageName)
            .WithCommand("dotnet", TestEnvironment.KafkaWebClient.AssemblyName)
            .WithCommand("--client-type", clientType == KafkaClientType.Producer ? "producer" : "consumer");

        foreach (var kafkaSetting in kafkaConfig)
        {
            containerParams.WithCommand("--kafka-config", kafkaSetting);
        }

        return containerParams;
    }
}