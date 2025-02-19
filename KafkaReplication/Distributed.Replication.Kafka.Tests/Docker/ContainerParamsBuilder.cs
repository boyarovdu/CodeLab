using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests.Docker;

public class ContainerParamsBuilder
{
    private readonly CreateContainerParameters _containerParameters = new()
    {
        HostConfig = new HostConfig(),
        Cmd = new List<string>()
    };

    public ContainerParamsBuilder WithImage(string imageName)
    {
        _containerParameters.Image = imageName;
        return this;
    }

    public ContainerParamsBuilder WithName(string containerName)
    {
        _containerParameters.Name = containerName;
        return this;
    }

    public ContainerParamsBuilder WithPortBinding(string containerPort, string hostPort)
    {
        _containerParameters.HostConfig.PortBindings ??= new Dictionary<string, IList<PortBinding>>();
        _containerParameters.ExposedPorts ??= new Dictionary<string, EmptyStruct>();

        if (!_containerParameters.ExposedPorts.ContainsKey(containerPort))
        {
            _containerParameters.ExposedPorts[containerPort] = new EmptyStruct();
        }
        
        if (!_containerParameters.HostConfig.PortBindings.TryGetValue(containerPort, out IList<PortBinding>? value))
        {
            value = new List<PortBinding>();
            _containerParameters.HostConfig.PortBindings[containerPort] = value;
        }
    
        value.Add(new PortBinding
        {
            HostPort = hostPort
        });
    
        return this;
    }

    public ContainerParamsBuilder WithCommand(params string[] command)
    {
        foreach (var c in command)
            _containerParameters.Cmd.Add(c);
        return this;
    }
    
    // public ContainerParamsBuilder WithNetwork(string networkName)
    // {
    //     _containerParameters.HostConfig.NetworkMode = networkName;
    //     return this;
    // }
    
    public ContainerParamsBuilder WithNetworks(params string[] networkNames)
    {
        foreach (var networkName in networkNames)
        {
            _containerParameters.NetworkingConfig.EndpointsConfig[networkName] = new EndpointSettings();
        }
        return this;
    }

    
    public CreateContainerParameters Build()
    {
        return _containerParameters;
    }
}