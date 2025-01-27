using System.Diagnostics;

namespace Distributed.Replication.Kafka.Tests;

public class CommandLine
{
    public static void Run(string command, string workingDirectory)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "sh",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            }
        };

        process.Start();

        while (!process.StandardOutput.EndOfStream)
        {
            var line = process.StandardOutput.ReadLine();
            TestContext.Progress.WriteLine(line);
        }

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(
                $"Command `{command}` failed with exit code {process.ExitCode}: {process.StandardError.ReadToEnd()}");
        }
    }
}