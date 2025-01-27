using NUnit.Framework;
using System.Diagnostics;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests
{
    public class BaseDockerTest
    {
        protected virtual string DockerComposeFolderPath => "./relative/path/to/compose/file";

        [OneTimeSetUp]
        public void StartDockerCompose()
        {
            TestContext.Progress.WriteLine($"Starting Docker Compose for {GetType().Name}...");
            CommandLine.Run("docker-compose up -d", DockerComposeFolderPath);
        }

        [OneTimeTearDown]
        public void StopDockerCompose()
        {
            TestContext.Progress.WriteLine($"Stopping Docker Compose for {GetType().Name}...");
            CommandLine.Run("docker-compose down", DockerComposeFolderPath);
        }
    }

    [TestFixture]
    public class MyTestClass2 : BaseDockerTest
    {
        protected override string DockerComposeFolderPath => "/home/david/Documents/Cluster";

        [Test]
        public async Task TestA()
        {
            using (DockerClient client = new DockerClientConfiguration().CreateClient())
            {
                var @params = new ContainersListParameters { All = true };
                var containers = await client.Containers.ListContainersAsync(@params);

                var msg = String.Join(',', containers.Select(c => c.Names.First()));

                TestContext.Progress.WriteLine($"containers: {msg}");
            }
        }
    }
}