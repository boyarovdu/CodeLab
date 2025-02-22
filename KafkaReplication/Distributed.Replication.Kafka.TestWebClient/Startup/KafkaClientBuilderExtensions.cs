using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;

namespace Distributed.Replication.Kafka.TestWebClient.Startup;

public static class KafkaClientBuilderExtensions
{
    private static IDictionary<string, string> ParseSettings(string[] settings) =>
        settings.Select(s =>
        {
            var split = s.Split('=');
            return (split[0], split[1]);
        }).ToDictionary();

    public static void AddKafkaConsumer(this IServiceCollection services, IDictionary<string, string> settings) =>
        services.AddSingleton(provider =>
        {
            var logger = provider.GetService<ILogger<IConsumer<string, string>>>();

            var consumer = new ConsumerBuilder<string, string>(settings)
                .SetErrorHandler((_, error) => logger.LogError($"Kafka consumer error: {JsonSerializer.Serialize(error)}"))
                .SetLogHandler((_, log) => logger.LogInformation($"Kafka consumer log: {JsonSerializer.Serialize(log)}"))
                .Build();

            return consumer;
        });

    public static void AddKafkaProducer(this IServiceCollection services, IDictionary<string, string> settings) =>
        services.AddSingleton(provider =>
        {
            var logger = provider.GetService<ILogger<IProducer<string, string>>>();

            var producer = new ProducerBuilder<string, string>(settings)
                .SetErrorHandler((_, error) => logger.LogError($"Kafka producer error: {JsonSerializer.Serialize(error)}"))
                .SetLogHandler((_, log) => logger.LogInformation($"Kafka producer log: {JsonSerializer.Serialize(log)}"))
                .Build();

            return producer;
        });
    
    public static void AddKafkaProducer(this IServiceCollection services, string[] settings) => 
        services.AddKafkaProducer(ParseSettings(settings));
    
    public static void AddKafkaConsumer(this IServiceCollection services, string[] settings) => 
        services.AddKafkaConsumer(ParseSettings(settings));
}