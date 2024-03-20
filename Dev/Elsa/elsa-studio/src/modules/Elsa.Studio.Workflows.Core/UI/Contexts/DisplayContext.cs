using System.Text.Json.Nodes;
using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Studio.Workflows.UI.Args;
using Elsa.Studio.Workflows.UI.Models;

namespace Elsa.Studio.Workflows.UI.Contexts;

/// <summary>
/// Represents a context for displaying an activity.
/// </summary>
/// <param name="WorkflowDefinition">The Workflow containing the Activity.</param>
/// <param name="Activity">The activity to display.</param>
/// <param name="ActivitySelectedCallback">A callback that is invoked when an activity is selected.</param>
/// <param name="ActivityEmbeddedPortSelectedCallback">A callback that is invoked when an embedded port is selected.</param>
/// <param name="GraphUpdatedCallback">A callback that is invoked when the graph is updated.</param>
/// <param name="IsReadOnly">Whether the activity is read-only.</param>
/// <param name="ActivityStats">A map of activity stats.</param>
public record DisplayContext(WorkflowDefinition WorkflowDefinition,
    JsonObject Activity, 
    Func<JsonObject, Task>? ActivitySelectedCallback = default,
    Func<ActivityEmbeddedPortSelectedArgs, Task>? ActivityEmbeddedPortSelectedCallback = default,
    Func<JsonObject, Task>? ActivityDoubleClickCallback = default,
    Func<Task>? GraphUpdatedCallback = default, 
    bool IsReadOnly = false,
    IDictionary<string, ActivityStats>? ActivityStats = default);