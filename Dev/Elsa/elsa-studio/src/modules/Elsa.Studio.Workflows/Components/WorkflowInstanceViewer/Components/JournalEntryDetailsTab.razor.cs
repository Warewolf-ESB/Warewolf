using System.Text.Json;
using System.Text.Json.Nodes;
using Elsa.Studio.Models;
using Elsa.Studio.Workflows.Pages.WorkflowInstances.View.Models;
using Humanizer;
using Microsoft.AspNetCore.Components;

namespace Elsa.Studio.Workflows.Components.WorkflowInstanceViewer.Components;

/// <summary>
/// Displays the details of a journal entry.
/// </summary>
public partial class JournalEntryDetailsTab
{
    /// <summary>
    /// The journal entry.
    /// </summary>
    [Parameter]
    public JournalEntry JournalEntry { get; set; } = default!;

    /// <summary>
    /// The height of the visible pane.
    /// </summary>
    [Parameter]
    public int VisiblePaneHeight { get; set; }

    [CascadingParameter] private IDictionary<string, JsonObject> ActivityLookup { get; set; } = default!;

    private JsonObject GetActivity()
    {
        var activityId = JournalEntry.Record.ActivityId;
        return ActivityLookup[activityId];
    }

    private IDictionary<string, DataPanelItem> ParsePayload(object? payload)
    {
        if (payload == null)
            return new Dictionary<string, DataPanelItem>();

        if (payload is not JsonElement jsonElement)
            return new Dictionary<string, DataPanelItem>
            {
                ["Payload"] = new(payload.ToString())
            };

        var properties = jsonElement.EnumerateObject().Where(x => !x.Name.StartsWith("_"));
        var result = new Dictionary<string, DataPanelItem>();

        foreach (var property in properties)
        {
            var propertyName = property.Name.Pascalize();
            var propertyValue = property.Value;
            var propertyValueAsString = propertyValue.ToString();
            result[propertyName] = new(propertyValueAsString);
        }

        return result;
    }

    private static void Merge(IDictionary<string, DataPanelItem> target, IDictionary<string, DataPanelItem> input)
    {
        foreach (var (key, value) in input) target[key] = value;
    }
}