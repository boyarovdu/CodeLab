using Docker.DotNet.Models;

namespace Distributed.Replication.Kafka.Tests.Builders;

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

    public CreateContainerParameters Build()
    {
        return _containerParameters;
    }
}