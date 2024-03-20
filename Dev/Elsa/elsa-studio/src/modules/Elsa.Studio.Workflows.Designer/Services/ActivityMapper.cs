using System.Text.Json.Nodes;
using Elsa.Api.Client.Extensions;
using Elsa.Api.Client.Resources.ActivityDescriptors.Enums;
using Elsa.Studio.Workflows.Designer.Contracts;
using Elsa.Studio.Workflows.Designer.Models;
using Elsa.Studio.Workflows.Domain.Contexts;
using Elsa.Studio.Workflows.Domain.Contracts;
using Elsa.Studio.Workflows.UI.Contracts;
using Elsa.Studio.Workflows.UI.Models;

namespace Elsa.Studio.Workflows.Designer.Services;

internal class ActivityMapper : IActivityMapper
{
    private readonly IActivityRegistry _activityRegistry;
    private readonly IActivityPortService _activityPortService;
    private readonly IActivityDisplaySettingsRegistry _activityDisplaySettingsRegistry;

    public ActivityMapper(IActivityRegistry activityRegistry, IActivityPortService activityPortService, IActivityDisplaySettingsRegistry activityDisplaySettingsRegistry)
    {
        _activityRegistry = activityRegistry;
        _activityPortService = activityPortService;
        _activityDisplaySettingsRegistry = activityDisplaySettingsRegistry;
    }

    public X6ActivityNode MapActivity(JsonObject activity, ActivityStats? activityStats = default)
    {
        var activityId = activity.GetId();
        var designerMetadata = activity.GetDesignerMetadata();
        var position = designerMetadata.Position;
        var size = designerMetadata.Size;
        var x = position?.X ?? 0;
        var y = position?.Y ?? 0;
        var width = size?.Width ?? 0;
        var height = size?.Height ?? 0;

        if (width == 0) width = 200;
        if (height == 0) height = 50;

        // Create node.
        var node = new X6ActivityNode
        {
            Id = activityId,
            Data = activity,
            Size = new X6Size(width, height),
            Position = new X6Position(x, y),
            Shape = "elsa-activity",
            Ports = GetPorts(activity)
        };

        node.ActivityStats = activityStats;

        return node;
    }

    public X6Ports GetPorts(JsonObject activity)
    {
        // Create input ports.
        var inPorts = GetInPorts(activity);

        // Create output ports.
        var outPorts = GetOutPorts(activity);

        // Concatenate input and output ports.
        return new X6Ports
        {
            Items = inPorts.Concat(outPorts).ToList()
        };
    }

    public IEnumerable<X6Port> GetOutPorts(JsonObject activity)
    {
        var activityType = activity.GetTypeName();
        var activityVersion = activity.GetVersion();
        var activityDescriptor = _activityRegistry.Find(activityType, activityVersion)!;
        var sourcePorts = _activityPortService.GetPorts(new PortProviderContext(activityDescriptor, activity)).Where(x => x.Type == PortType.Flow);
        var displaySettings = _activityDisplaySettingsRegistry.GetSettings(activity.GetTypeName());

        var ports = sourcePorts.Select(sourcePort => new X6Port
        {
            Id = sourcePort.Name,
            Group = "out",
            Attrs = new X6Attrs
            {
                ["text"] = new X6Attrs
                {
                    ["text"] = sourcePort.DisplayName ?? string.Empty
                },
                ["circle"] = new X6Attrs
                {
                    ["fill"] = displaySettings.Color,
                }
            }
        }).ToList();

        if (ports.All(port => port.Group != "out"))
        {
            // Create default output port, except for terminal nodes.
            var isTerminalNode = activityDescriptor.IsTerminal;

            if (!isTerminalNode)
            {
                ports.Add(new X6Port
                {
                    Id = "Done",
                    Group = "out",
                    Attrs = new X6Attrs
                    {
                        ["text"] = new X6Attrs
                        {
                            ["text"] = "Done"
                        },
                        ["circle"] = new X6Attrs
                        {
                            ["fill"] = displaySettings.Color,
                        }
                    }
                });
            }
        }

        return ports;
    }

    public IEnumerable<X6Port> GetInPorts(JsonObject activity)
    {
        var displaySettings = _activityDisplaySettingsRegistry.GetSettings(activity.GetTypeName());

        // Create default input port.
        yield return new X6Port
        {
            Id = "In",
            Group = "in",
            Attrs = new X6Attrs
            {
                ["circle"] = new X6Attrs
                {
                    ["stroke"] = displaySettings.Color,
                }
            }
        };
    }
}