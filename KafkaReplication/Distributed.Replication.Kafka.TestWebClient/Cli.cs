using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Text.RegularExpressions;

namespace Distributed.Replication.Kafka.TestWebClient;

internal static class Cli
{
    public static async Task Start(string[] args, Action<string, string[]> rootHandler)
    {
        var typeOption = new Option<string>(
            "--client-type",
            description: "Specifies the type of client to run. This value determines the role the application will play when interacting with Kafka. Allowed value: 'consumer' or 'producer'.")
        {
            IsRequired = true
        };
        
        typeOption.AddValidator(option =>
        {
            var value = option.GetValueOrDefault<string>();
            if (value != "consumer" && value != "producer")
            {
                option.ErrorMessage = "The type must be either 'consumer' or 'producer'.";
            }
        });
        
        var kafkaConfigOption = new Option<string[]>(
            "--kafka-config",
            description: "Provides Kafka configuration settings as multiple key-value pairs. Each configuration must follow the format `key=value`, and it should not contain whitespace or additional `=` characters")
        {
            IsRequired = true,
            AllowMultipleArgumentsPerToken = true
        };
        
        kafkaConfigOption.AddValidator(option =>
        {
            var settings = option.GetValueOrDefault<string[]>()!;

            foreach (var setting in settings)
            {
                if (!Regex.IsMatch(setting, @"^[^\s=]+=[^\s=]+$"))
                {
                    option.ErrorMessage = "Kafka config settings must follow the pattern 'key=value'.";
                }
            }
        });
        
        var rootCommand = new RootCommand
        {
            typeOption,
            kafkaConfigOption
        };
        
        rootCommand.Handler = CommandHandler.Create(rootHandler);
        
        await rootCommand.InvokeAsync(args);
    }
}