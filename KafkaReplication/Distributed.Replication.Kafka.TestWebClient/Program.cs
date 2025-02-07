using Distributed.Replication.Kafka.TestWebClient.HealthChecks;
using Distributed.Replication.Kafka.TestWebClient.Startup;

namespace Distributed.Replication.Kafka.TestWebClient;

public static class Program
{
    public static string Type { get; private set; }

    public static async Task Main(string[] args)
    {
        await Cli.Start(args, (clientType, kafkaConfig) =>
        {
            Type = clientType;

            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            builder.Services.AddKafkaClient(kafkaConfig);

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapOpenApi();

            app.UseAuthorization();

            app.MapControllers();

            app.UseClientTypeMiddleware();

            app.UseHealthChecks("/health");

            app.Run();
        });
    }

    private static void AddKafkaClient(this IServiceCollection services, string[] kafkaConfig)
    {
        switch (Type)
        {
            case "producer":
                services.AddKafkaProducer(kafkaConfig);
                services.AddHealthChecks().AddCheck<ProducerHealthCheck>("Kafka");
                break;
            case "consumer":
                services.AddKafkaConsumer(kafkaConfig);
                break;
        }
    }
}