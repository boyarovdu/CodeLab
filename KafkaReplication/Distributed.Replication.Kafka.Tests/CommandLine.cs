using System;
using System.Diagnostics;

namespace Distributed.Replication.Kafka.Tests;

public static class CommandLine
{
    public static void Run(string command, string workingDirectory)
    {
        var isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

        var shell = isWindows ? "cmd.exe" : "sh";
        var shellArguments = isWindows ? $"/c \"{command}\"" : $"-c \"{command}\"";

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = shellArguments,
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