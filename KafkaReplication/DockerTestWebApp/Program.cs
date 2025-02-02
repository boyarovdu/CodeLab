using Confluent.Kafka;
using DockerTestWebApp.Middleware;
using DockerTestWebApp.Startup;

namespace DockerTestWebApp;

public class Program
{
    public static string Type { get; set; }
    
    public static async Task Main(string[] args)
    {
        await Cli.Start(args, (bootstrapServers, type) =>
        {
            Type = type;
            
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();
            builder.Services.AddSwaggerGen();

            if (type == "producer")
            {
                builder.Services.AddKafkaProducer(bootstrapServers);    
            }
            else
            {
                builder.Services.AddKafkaConsumer(bootstrapServers);
            }

            var app = builder.Build();
            
            app.UseSwagger();
            app.UseSwaggerUI();
            
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }
            
            app.UseAuthorization();

            app.MapControllers();

            app.UseClientTypeMiddleware();

            app.Run();
        });
    }
}