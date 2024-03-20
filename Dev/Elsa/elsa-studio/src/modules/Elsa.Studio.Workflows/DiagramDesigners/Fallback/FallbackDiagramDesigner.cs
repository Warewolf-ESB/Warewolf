using System.Text.Json.Nodes;
using Elsa.Studio.Workflows.UI.Contexts;
using Elsa.Studio.Workflows.UI.Contracts;
using Elsa.Studio.Workflows.UI.Models;
using Microsoft.AspNetCore.Components;

namespace Elsa.Studio.Workflows.DiagramDesigners.Fallback;

/// <summary>
/// A fallback diagram designer that displays a simplified view of the workflow.
/// </summary>
public class FallbackDiagramDesigner : IDiagramDesigner
{
    private JsonObject _activity = default!;

    /// <inheritdoc />
    public Task LoadRootActivityAsync(JsonObject activity, IDictionary<string, ActivityStats>? activityStatsMap)
    {
        _activity = activity;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateActivityAsync(string id, JsonObject activity)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateActivityStatsAsync(string id, ActivityStats stats)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task SelectActivityAsync(string id)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<JsonObject> ReadRootActivityAsync()
    {
        return Task.FromResult(_activity);
    }

    /// <inheritdoc />
    public RenderFragment DisplayDesigner(DisplayContext context)
    {
        _activity = context.Activity;

        return builder =>
        {
            builder.OpenComponent<FallbackDesigner>(0);
            builder.AddAttribute(1, nameof(FallbackDesigner.Activity), _activity);
            builder.CloseComponent();
        };
    }
}