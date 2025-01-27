using Docker.DotNet;

namespace Distributed.Replication.Kafka.Console;
    
using Docker.DotNet;

class Program
{
    static async Task Main(string[] args)
    {
        DockerClient client = new DockerClientConfiguration().CreateClient();

        var networks = await client.Networks.ListNetworksAsync();

        foreach (var network in networks)
        {
            System.Console.WriteLine(network.Name);
        }
        
        // System.Console.WriteLine("Hello, World!");
    }
}