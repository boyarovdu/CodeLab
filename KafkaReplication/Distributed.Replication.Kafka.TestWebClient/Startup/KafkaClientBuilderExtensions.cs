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
        services.AddSingleton(new ConsumerBuilder<string, string>(settings).Build());

    public static void AddKafkaConsumer(this IServiceCollection services, string[] settings) => 
        services.AddKafkaConsumer(ParseSettings(settings));

    public static void AddKafkaProducer(this IServiceCollection services, IDictionary<string, string> settings) => 
        services.AddSingleton(new ProducerBuilder<string, string>(settings).Build());

    public static void AddKafkaProducer(this IServiceCollection services, string[] settings) => 
        services.AddKafkaProducer(ParseSettings(settings));
}