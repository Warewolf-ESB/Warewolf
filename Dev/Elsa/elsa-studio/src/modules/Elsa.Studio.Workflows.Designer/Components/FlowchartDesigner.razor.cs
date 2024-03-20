using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Elsa.Api.Client.Extensions;
using Elsa.Api.Client.Resources.ActivityDescriptors.Enums;
using Elsa.Api.Client.Resources.ActivityDescriptors.Models;
using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Api.Client.Shared.Models;
using Elsa.Studio.Contracts;
using Elsa.Studio.Extensions;
using Elsa.Studio.Workflows.Designer.Contracts;
using Elsa.Studio.Workflows.Designer.Interop;
using Elsa.Studio.Workflows.Designer.Models;
using Elsa.Studio.Workflows.Designer.Services;
using Elsa.Studio.Workflows.Domain.Contracts;
using Elsa.Studio.Workflows.Extensions;
using Elsa.Studio.Workflows.UI.Args;
using Elsa.Studio.Workflows.UI.Models;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Utilities;
using ThrottleDebounce;

namespace Elsa.Studio.Workflows.Designer.Components;

/// <summary>
/// A Blazor component that renders a flowchart designer.
/// </summary>
public partial class FlowchartDesigner : IDisposable, IAsyncDisposable
{
    private readonly string _containerId = $"container-{Guid.NewGuid():N}";
    private DotNetObjectReference<FlowchartDesigner>? _componentRef;
    private IFlowchartMapper? _flowchartMapper = default!;
    private IActivityMapper? _activityMapper = default!;
    private X6GraphApi _graphApi = default!;
    private readonly PendingActionsQueue _pendingGraphActions;
    private RateLimitedFunc<Task> _rateLimitedLoadFlowchartAction;
    private IDictionary<string, ActivityStats>? _activityStats;

    /// <inheritdoc />
    public FlowchartDesigner()
    {
        _pendingGraphActions = new PendingActionsQueue(() => new(_graphApi != null!));

        _rateLimitedLoadFlowchartAction = Debouncer.Debounce(async () => { await InvokeAsync(async () => await LoadFlowchartAsync(Flowchart, ActivityStats)); }, TimeSpan.FromMilliseconds(100));
    }

    /// <summary>
    /// Gets or sets the workflow definition.
    /// </summary>
    [Parameter]
    public WorkflowDefinition? WorkflowDefinition { get; set; }

    /// <summary>
    /// The flowchart to render.
    /// </summary>
    [Parameter]
    public JsonObject Flowchart { get; set; } = default!;

    /// <summary>
    /// The activity stats to render.
    /// </summary>
    [Parameter]
    public IDictionary<string, ActivityStats>? ActivityStats { get; set; }

    /// <summary>
    /// Whether the flowchart is read-only.
    /// </summary>
    [Parameter]
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// An event raised when an activity is selected.
    /// </summary>
    [Parameter]
    public Func<JsonObject, Task>? ActivitySelected { get; set; }

    /// <summary>
    /// An event raised when an activity embedded port is selected.
    /// </summary>
    [Parameter]
    public Func<ActivityEmbeddedPortSelectedArgs, Task>? ActivityEmbeddedPortSelected { get; set; }

    /// <summary>
    /// An event raised when an activity is double clicked.
    /// </summary>
    [Parameter]
    public Func<JsonObject, Task>? ActivityDoubleClick { get; set; }

    /// <summary>
    /// An event raised when the canvas is selected.
    /// </summary>
    [Parameter]
    public Func<Task>? CanvasSelected { get; set; }

    /// <summary>
    /// An event raised when the graph is updated.
    /// </summary>
    [Parameter]
    public Func<Task>? GraphUpdated { get; set; }

    [Inject] private DesignerJsInterop DesignerJsInterop { get; set; } = default!;
    [Inject] private IThemeService ThemeService { get; set; } = default!;
    [Inject] private IActivityRegistry ActivityRegistry { get; set; } = default!;
    [Inject] private IMapperFactory MapperFactory { get; set; } = default!;
    [Inject] private IIdentityGenerator IdentityGenerator { get; set; } = default!;
    [Inject] private IActivityNameGenerator ActivityNameGenerator { get; set; } = default!;

    /// <summary>
    /// Invoked from JavaScript when an activity is selected.
    /// </summary>
    /// <param name="activity">The selected activity.</param>
    [JSInvokable]
    public async Task HandleActivitySelected(JsonObject activity)
    {
        if (ActivitySelected == null)
            return;

        await InvokeAsync(async () => await ActivitySelected(activity));
    }

    /// <summary>
    /// Invoked from JavaScript when an activity embedded port is selected.
    /// </summary>
    /// <param name="activity">The selected activity.</param>
    /// <param name="portName">The selected port name.</param>
    [JSInvokable]
    public async Task HandleActivityEmbeddedPortSelected(JsonObject activity, string portName)
    {
        if (ActivityEmbeddedPortSelected == null)
            return;

        var args = new ActivityEmbeddedPortSelectedArgs(activity, portName);
        await InvokeAsync(async () => await ActivityEmbeddedPortSelected(args));
    }

    /// <summary>
    /// Invoked from JavaScript when an activity is double clicked.
    /// </summary>
    /// <param name="activity">The clicked activity.</param>
    [JSInvokable]
    public async Task HandleActivityDoubleClick(JsonObject activity)
    {
        if (ActivityDoubleClick == null)
            return;

        await InvokeAsync(async () => await ActivityDoubleClick(activity));
    }

    /// <summary>
    /// Invoked from JavaScript when the canvas is selected.
    /// </summary>
    [JSInvokable]
    public async Task HandleCanvasSelected()
    {
        if (CanvasSelected == null)
            return;

        await InvokeAsync(async () => await CanvasSelected());
    }

    /// <summary>
    /// Invoked from JavaScript when the graph is updated.
    /// </summary>
    [JSInvokable]
    public async Task HandleGraphUpdated()
    {
        if (GraphUpdated == null)
            return;

        await InvokeAsync(async () => await GraphUpdated());
    }

    /// <summary>
    /// Invoked from JavaScript when the graph is updated.
    /// </summary>
    [JSInvokable]
    public async Task HandlePasteCellsRequested(X6ActivityNode[] activityNodes, X6Edge[] edges)
    {
        var allActivities = Flowchart.GetActivities().ToList();
        var activityLookup = new Dictionary<string, X6ActivityNode>();

        foreach (var activityNode in activityNodes)
        {
            var activity = activityNode.Data;
            var activityType = activity.GetTypeName();
            var activityVersion = activity.GetVersion();
            var descriptor = ActivityRegistry.Find(activityType, activityVersion)!;
            var newActivityId = IdentityGenerator.GenerateId();
            var newName = ActivityNameGenerator.GenerateNextName(allActivities, descriptor);

            // Capture the original activity ID so we can update the edges.
            activityLookup[activityNode.Id] = activityNode;

            // Update the activity ID and name.
            activity.SetId(newActivityId);
            activity.SetName(newName);
            activityNode.Id = newActivityId;

            // Add activity to the list of all activities. This is used to generate a new name for the next activity.
            allActivities.Add(activity);

            // Update the activity metadata.
            var designerMetadata = activity.GetDesignerMetadata();
            designerMetadata.Position.X = activityNode.Position.X + 64;
            designerMetadata.Position.Y = activityNode.Position.Y + 64;
            activityNode.Position.X = designerMetadata.Position.X;
            activityNode.Position.Y = designerMetadata.Position.Y;
            activity.SetDesignerMetadata(designerMetadata);

            // If the activity contains embedded ports, generate new IDs for the contained flowchart.
            ProcessEmbeddedPorts(activity, descriptor);
        }

        // Update the edges.
        foreach (var edge in edges)
        {
            var sourceActivityId = edge.Source.Cell;
            var targetActivityId = edge.Target.Cell;
            var sourceActivity = activityLookup[sourceActivityId];
            var targetActivity = activityLookup[targetActivityId];

            edge.Source.Cell = sourceActivity.Data.GetId();
            edge.Target.Cell = targetActivity.Data.GetId();
        }

        // Update the flowchart.
        await ScheduleGraphActionAsync(async () => await _graphApi.PasteCellsAsync(activityNodes, edges));
    }

    /// <summary>
    /// Reads the flowchart from the graph.
    /// </summary>
    /// <returns>The flowchart.</returns>
    public async Task<JsonObject> ReadFlowchartAsync()
    {
        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return await ScheduleGraphActionAsync(async () =>
        {
            var data = await _graphApi.ReadGraphAsync();
            var cells = data.GetProperty("cells").EnumerateArray();
            var nodes = cells.Where(x => x.GetProperty("shape").GetString() == "elsa-activity").Select(x => x.Deserialize<X6ActivityNode>(serializerOptions)!).ToList();
            var edges = cells.Where(x => x.GetProperty("shape").GetString() == "elsa-edge").Select(x => x.Deserialize<X6Edge>(serializerOptions)!).ToList();
            var graph = new X6Graph(nodes, edges);
            var flowchartMapper = await GetFlowchartMapperAsync();
            var exportedFlowchart = flowchartMapper.Map(graph);
            var activities = exportedFlowchart.GetActivities();
            var connections = exportedFlowchart.GetConnections();

            var flowchart = Flowchart;
            flowchart.SetProperty(activities, "activities");
            flowchart.SetProperty(connections.SerializeToNode(), "connections");

            return flowchart;
        });
    }

    /// <summary>
    /// Loads the flowchart into the graph.
    /// </summary>
    /// <param name="activity">The flowchart to load.</param>
    /// <param name="activityStats">The activity stats to load.</param>
    public async Task LoadFlowchartAsync(JsonObject activity, IDictionary<string, ActivityStats>? activityStats)
    {
        Flowchart = activity;
        ActivityStats = activityStats;
        var flowchartMapper = await GetFlowchartMapperAsync();
        var flowchart = activity.GetFlowchart();
        var graph = flowchartMapper.Map(flowchart, activityStats);
        await ScheduleGraphActionAsync(() => _graphApi.LoadGraphAsync(graph));
    }

    /// <summary>
    /// Adds an activity to the graph.
    /// </summary>
    /// <param name="activity">The activity to add.</param>
    public async Task AddActivityAsync(JsonObject activity)
    {
        var mapper = await GetActivityMapperAsync();
        var node = mapper.MapActivity(activity);
        await ScheduleGraphActionAsync(() => _graphApi.AddActivityNodeAsync(node));
    }

    /// <summary>
    /// Selects the specified activity in the graph.
    /// </summary>
    /// <param name="id">The ID of the activity to select.</param>
    public async Task SelectActivityAsync(string id)
    {
        await ScheduleGraphActionAsync(() => _graphApi.SelectActivityAsync(id));
    }

    /// <summary>
    /// Zoom the canvas to fit all activities.
    /// </summary>
    public async Task ZoomToFitAsync() => await ScheduleGraphActionAsync(() => _graphApi.ZoomToFitAsync());

    /// <summary>
    /// Center the canvas content.
    /// </summary>
    public async Task CenterContentAsync() => await ScheduleGraphActionAsync(() => _graphApi.CenterContentAsync());

    /// <summary>
    /// Update the Graph Layout.
    /// </summary>
    public async Task AutoLayoutAsync(JsonObject activity, IDictionary<string, ActivityStats>? activityStats)
    {
        var flowchartMapper = await GetFlowchartMapperAsync();
        var flowchart = activity.GetFlowchart();
        var graph = flowchartMapper.Map(flowchart, activityStats);
        await ScheduleGraphActionAsync(() => _graphApi.AutoLayoutAsync(graph));
    }

    /// <summary>
    /// Update the specified activity on the graph.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="activity">The updated activity.</param>
    public async Task UpdateActivityAsync(string id, JsonObject activity) => await ScheduleGraphActionAsync(() => _graphApi.UpdateActivityAsync(activity));

    /// <summary>
    /// Update the specified activity stats on the graph.
    /// </summary>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="stats">The updated activity stats.</param>
    public async Task UpdateActivityStatsAsync(string activityId, ActivityStats stats) => await ScheduleGraphActionAsync(() => DesignerJsInterop.UpdateActivityStatsAsync($"activity-{activityId}", activityId, stats));

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        ThemeService.IsDarkModeChanged += OnDarkModeChanged;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _componentRef = DotNetObjectReference.Create(this);
            _graphApi = await DesignerJsInterop.CreateGraphAsync(_containerId, _componentRef, IsReadOnly);
            await _pendingGraphActions.ProcessAsync();
        }
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (!Equals(_activityStats, ActivityStats))
        {
            _activityStats = ActivityStats;
            await _rateLimitedLoadFlowchartAction.InvokeAsync();
        }
    }

    private async Task<IFlowchartMapper> GetFlowchartMapperAsync() => _flowchartMapper ??= await MapperFactory.CreateFlowchartMapperAsync();
    private async Task<IActivityMapper> GetActivityMapperAsync() => _activityMapper ??= await MapperFactory.CreateActivityMapperAsync();
    private async Task SetGridColorAsync(string color) => await ScheduleGraphActionAsync(() => _graphApi.SetGridColorAsync(color));
    private async Task ScheduleGraphActionAsync(Func<Task> action) => await _pendingGraphActions.EnqueueAsync(action);
    private async Task<T> ScheduleGraphActionAsync<T>(Func<Task<T>> action) => await _pendingGraphActions.EnqueueAsync(action);

    private void GenerateNewActivityIds(JsonObject container)
    {
        var activities = container.GetActivities().ToList();
        var activityLookup = new Dictionary<string, JsonObject>();
        var newContainerId = IdentityGenerator.GenerateId();

        // Update the container ID.
        container.SetId(newContainerId);

        foreach (var activity in activities)
        {
            var activityType = activity.GetTypeName();
            var activityVersion = activity.GetVersion();
            var descriptor = ActivityRegistry.Find(activityType, activityVersion)!;
            var newActivityId = IdentityGenerator.GenerateId();

            // Capture the original activity ID so we can update the edges.
            activityLookup[activity.GetId()] = activity;

            // Update the activity ID.
            activity.SetId(newActivityId);

            // If the activity contains embedded ports, generate new IDs for the contained flowchart.
            ProcessEmbeddedPorts(activity, descriptor);
        }

        // Update connections.
        var connections = container.GetConnections().ToList();

        foreach (var connection in connections)
        {
            var sourceActivityId = connection.Source.ActivityId;
            var targetActivityId = connection.Target.ActivityId;
            var sourceActivity = activityLookup[sourceActivityId];
            var targetActivity = activityLookup[targetActivityId];

            connection.Source.ActivityId = sourceActivity.GetId();
            connection.Target.ActivityId = targetActivity.GetId();
        }

        container.SetConnections(connections);
    }

    /// <summary>
    /// Processes each embedded port's activity and generates new IDs for the contained flowchart.
    /// </summary>
    private void ProcessEmbeddedPorts(JsonObject activity, ActivityDescriptor descriptor)
    {
        var embeddedPorts = descriptor.Ports.Where(x => x.Type == PortType.Embedded);
        foreach (var embeddedPort in embeddedPorts)
        {
            var camelName = embeddedPort.Name.Camelize();
            if (activity.TryGetPropertyValue(camelName, out var propValue) && propValue is JsonObject childActivity)
                GenerateNewActivityIds(childActivity);
        }
    }

    private async void OnDarkModeChanged()
    {
        var palette = ThemeService.CurrentPalette;
        var gridColor = palette.BackgroundGrey;
        await SetGridColorAsync(gridColor.ToString(MudColorOutputFormats.HexA));
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_graphApi != null!)
            await _graphApi.DisposeGraphAsync();

        ((IDisposable)this).Dispose();
    }

    void IDisposable.Dispose()
    {
        ThemeService.IsDarkModeChanged -= OnDarkModeChanged;
        _componentRef?.Dispose();
    }
}