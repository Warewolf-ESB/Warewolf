using Elsa.Api.Client.Resources.ActivityDescriptors.Models;
using Elsa.Studio.Contracts;
using Elsa.Studio.Extensions;
using Elsa.Studio.Workflows.Domain.Contracts;
using Elsa.Studio.Workflows.Domain.Extensions;
using Elsa.Studio.Workflows.Domain.Notifications;
using Elsa.Studio.Workflows.Models;
using Elsa.Studio.Workflows.UI.Contracts;
using Microsoft.AspNetCore.Components;

namespace Elsa.Studio.Workflows.Components.WorkflowDefinitionEditor.Components;

/// <summary>
/// A component that allows the user to pick an activity.
/// </summary>
public partial class ActivityPicker : IDisposable, INotificationHandler<ActivityRegistryRefreshed>
{
    private string _searchText = "";

    private IEnumerable<IGrouping<string, ActivityDescriptor>> GroupedActivityDescriptors
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_searchText))
                return ActivityDescriptors.GroupBy(x => x.Category); // Return all items grouped by category

            var items = ActivityDescriptors.Where(item =>
                item.Name.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase) ||
                item.TypeName.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase) ||
                (item.DisplayName != null && item.DisplayName.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase)) ||
                item.Category.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase) ||
                (item.Description != null && item.Description.Contains(_searchText, StringComparison.InvariantCultureIgnoreCase))
            );
            return items
                .GroupBy(x => x.Category);
        }
    }

    /// <summary>
    /// The drag and drop manager.
    /// </summary>
    [CascadingParameter] public DragDropManager DragDropManager { get; set; } = default!;

    [Inject] private IActivityRegistry ActivityRegistry { get; set; } = default!;
    [Inject] private IActivityDisplaySettingsRegistry ActivityDisplaySettingsRegistry { get; set; } = default!;
    [Inject] private IMediator Mediator { get; set; } = default!;

    private IEnumerable<ActivityDescriptor> ActivityDescriptors { get; set; } = new List<ActivityDescriptor>();

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Mediator.Subscribe<ActivityRegistryRefreshed>(this);
        Refresh();
    }

    private void Refresh()
    {
        ActivityDescriptors = ActivityRegistry.ListBrowsable();
        StateHasChanged();
    }

    private void OnDragStart(ActivityDescriptor activityDescriptor)
    {
        DragDropManager.Payload = activityDescriptor;
    }
    
    Task INotificationHandler<ActivityRegistryRefreshed>.HandleAsync(ActivityRegistryRefreshed notification, CancellationToken cancellationToken)
    {
        Refresh();
        return Task.CompletedTask;
    }
    
    void IDisposable.Dispose()
    {
        Mediator.Unsubscribe(this);
    }
}