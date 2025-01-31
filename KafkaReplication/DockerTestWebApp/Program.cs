using Confluent.Kafka;
using DockerTestWebApp.Startup;

namespace DockerTestWebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        await Cli.Start(args, (bootstrapServers, type) =>
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            if (type == "producer")
            {
                builder.Services.AddKafkaProducer(new ProducerConfig{ BootstrapServers = bootstrapServers});    
            }

            var app = builder.Build();
        
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
        
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        });
    }
}