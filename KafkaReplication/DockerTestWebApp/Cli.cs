using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace DockerTestWebApp;

internal static class Cli
{
    public static async Task Start(string[] args, Action<string, string> rootHandler)
    {
        var bootstrapServersOption = new Option<string>(
            "--bootstrap-servers",
            description: "Comma-separated list of Kafka bootstrap servers")
        {
            IsRequired = true
        };
        
        var typeOption = new Option<string>(
            "--type",
            description: "The type of Kafka component: 'consumer' or 'producer'")
        {
            IsRequired = true
        };
        
        typeOption.AddValidator(option =>
        {
            var value = option.GetValueOrDefault<string>();
            if (value != "consumer" && value != "producer")
            {
                throw new ArgumentException("The type must be either 'consumer' or 'producer'.");
            }
        });
        
        var rootCommand = new RootCommand
        {
            bootstrapServersOption,
            typeOption
        };
        
        rootCommand.Handler = CommandHandler.Create<string, string>(rootHandler);
        
        await rootCommand.InvokeAsync(args);
    }
}