using Confluent.Kafka;

namespace DockerTestWebApp.Startup;

public static class KafkaClientBuilderExtensions
{
    public static void AddKafkaConsumer(this IServiceCollection services, string bootstrapServers)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers
        };
        
        services.AddSingleton(new ConsumerBuilder<string, string>(config).Build());
    }

    public static void AddKafkaProducer(this IServiceCollection services, string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };
        
        services.AddSingleton(new ProducerBuilder<string, string>(config).Build());
    }
}