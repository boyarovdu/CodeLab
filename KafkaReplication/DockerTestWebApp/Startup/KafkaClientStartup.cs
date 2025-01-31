using Confluent.Kafka;

namespace DockerTestWebApp.Startup;

public static class KafkaClientStartup
{
    public static void AddKafkaConsumer(this IServiceCollection services, ConsumerConfig config)
    {
        // services.AddSingleton<Consumer>();
    }

    public static void AddKafkaProducer(this IServiceCollection services, ProducerConfig config)
    {
        services.AddSingleton(new ProducerBuilder<string, string>(config).Build());
    }
}