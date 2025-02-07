using CliWrap;

namespace Distributed.Replication.Kafka.Tests.Utils;

public static class Cmd
{
    public static async Task ExecAsync(string workingDirectory, string command, string arguments)
    {
        var p = AppDomain.CurrentDomain.BaseDirectory;
        
        var cmd = Cli.Wrap(command)
            .WithArguments(arguments)
            .WithWorkingDirectory(workingDirectory)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(TestContext.Progress.WriteLine))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(TestContext.Progress.WriteLine));
            
        var result = await cmd.ExecuteAsync();

        if (result.ExitCode != 0)
        {
            throw new Exception($"Command `{command}` failed with exit code {result.ExitCode}.");
        }
    }
}