using NUnit.Framework;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests
{
    public class BaseDockerTest
    {
        /// <summary>
        /// Path to the folder containing the 'docker-compose.yml' for this test class.
        /// Should be overridden in each test class.
        /// </summary>
        protected virtual string DockerComposeFolderPath => "./relative/path/to/compose/file";

        [OneTimeSetUp]
        public void StartDockerCompose()
        {
            TestContext.Progress.WriteLine($"Starting Docker Compose for {GetType().Name}...");
            RunDockerCommand("docker-compose up -d", DockerComposeFolderPath);
        }

        [OneTimeTearDown]
        public void StopDockerCompose()
        {
            TestContext.Progress.WriteLine($"Stopping Docker Compose for {GetType().Name}...");
            RunDockerCommand("docker-compose down", DockerComposeFolderPath);
        }

        private void RunDockerCommand(string command, string workingDirectory)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "sh", // Use 'cmd' for Windows and 'sh' for Linux/Mac
                    Arguments = $"-c \"{command}\"", // Use /c on Windows
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
                TestContext.Progress.WriteLine(line); // Output to test console
            }

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception(
                    $"Command `{command}` failed with exit code {process.ExitCode}: {process.StandardError.ReadToEnd()}");
            }
        }
    }
    
    [TestFixture]
    public class MyTestClass2 : BaseDockerTest
    {
        // Override to provide the relative path for the docker-compose.yml
        protected override string DockerComposeFolderPath => "/home/david/Documents/Cluster";

        [Test]
        public async Task TestA()
        {
            DockerClient client = new DockerClientConfiguration().CreateClient();

            var @params = new ContainersListParameters { All = true };
            var containers = await client.Containers.ListContainersAsync(@params);

            var msg = String.Join(',', containers.Select(c => c.Names.First()));
            
            TestContext.Progress.WriteLine($"containers: {msg}");
            
            Assert.Pass("TestA executed.");
        }

        [Test]
        public void TestB()
        {
            Assert.Pass("TestB executed.");
        }
    }
}