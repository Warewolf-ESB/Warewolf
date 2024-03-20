using System.Text.Json.Nodes;
using Elsa.Api.Client.Extensions;
using Elsa.Api.Client.Resources.ActivityDescriptors.Models;
using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Api.Client.Shared.Models;
using Elsa.Studio.DomInterop.Contracts;
using Elsa.Studio.Workflows.Domain.Contracts;
using Elsa.Studio.Workflows.Shared.Components;
using Elsa.Studio.Workflows.UI.Contracts;
using Humanizer;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;

namespace Elsa.Studio.Workflows.Components.WorkflowDefinitionEditor.Components;

public partial class WorkflowDefinitionVersionViewer
{
    private RadzenSplitterPane _activityPropertiesPane = default!;
    private int _activityPropertiesPaneHeight = 300;
    private DiagramDesignerWrapper? _diagramDesigner;

    [Parameter] public WorkflowDefinition? WorkflowDefinition { get; set; }
    [Inject] private IWorkflowDefinitionService WorkflowDefinitionService { get; set; } = default!;
    [Inject] private IActivityRegistry ActivityRegistry { get; set; } = default!;
    [Inject] private IDiagramDesignerService DiagramDesignerService { get; set; } = default!;
    [Inject] private IDomAccessor DomAccessor { get; set; } = default!;
    [Inject] private IFiles Files { get; set; } = default!;

    private JsonObject? SelectedActivity { get; set; }
    private ActivityDescriptor? ActivityDescriptor { get; set; }
    public string? SelectedActivityId { get; set; }
    private ActivityProperties.ActivityPropertiesPanel? ActivityPropertiesTab { get; set; }

    private RadzenSplitterPane ActivityPropertiesPane
    {
        get => _activityPropertiesPane;
        set
        {
            _activityPropertiesPane = value;

            // Prefix the ID with a non-numerical value so it can always be used as a query selector (sometimes, Radzen generates a unique ID starting with a number).
            _activityPropertiesPane.UniqueID = $"pane-{value.UniqueID}";
        }
    }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await ActivityRegistry.EnsureLoadedAsync();
        
        if (WorkflowDefinition?.Root == null)
            return;
        
        SelectActivity(WorkflowDefinition.Root);
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (WorkflowDefinition?.Root == null)
            return;

        if(_diagramDesigner != null)
            await _diagramDesigner.LoadActivityAsync(WorkflowDefinition.Root);
        
        SelectActivity(WorkflowDefinition.Root);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (WorkflowDefinition?.Root == null)
            return;
        
        if(firstRender)
            await _diagramDesigner!.LoadActivityAsync(WorkflowDefinition.Root);
    }

    private void SelectActivity(JsonObject activity)
    {
        SelectedActivity = activity;
        SelectedActivityId = activity.GetId();
        ActivityDescriptor = ActivityRegistry.Find(activity.GetTypeName());
        StateHasChanged();
    }
    
    private Task OnActivitySelected(JsonObject activity)
    {
        SelectActivity(activity);
        return Task.CompletedTask;
    }

    private async Task OnDownloadClicked()
    {
        var download = await WorkflowDefinitionService.ExportDefinitionAsync(WorkflowDefinition!.DefinitionId, VersionOptions.SpecificVersion(WorkflowDefinition.Version));
        var fileName = $"{WorkflowDefinition.Name.Kebaberize()}.json";
        await Files.DownloadFileFromStreamAsync(fileName, download.Content);
    }
    
    private async Task OnResize(RadzenSplitterResizeEventArgs arg)
    {
        var paneQuerySelector = $"#{ActivityPropertiesPane.UniqueID}";
        var visibleHeight = await DomAccessor.GetVisibleHeightAsync(paneQuerySelector);
        _activityPropertiesPaneHeight = (int)visibleHeight + 50;
    }
}