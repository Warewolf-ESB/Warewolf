using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Api.Client.Shared.UIHints.DropDown;
using Elsa.Studio.Models;
using Microsoft.AspNetCore.Components;
using Elsa.Api.Client.Resources.Scripting.Models;

namespace Elsa.Studio.UIHints.Components;

/// <summary>
/// Provides a component for picking a variable, input or output.
/// </summary>
public partial class AssignVariablePicker
{
    private ICollection<SelectListItem> _items = Array.Empty<SelectListItem>();

    [Parameter] public DisplayInputEditorContext EditorContext { get; set; } = default!;

    private ICollection<Variable> Variables => EditorContext.WorkflowDefinition.Variables;
    private ICollection<InputDefinition> InputVariables => EditorContext.WorkflowDefinition.Inputs;
    private ICollection<OutputDefinition> OutputVariables => EditorContext.WorkflowDefinition.Outputs;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var allItems = new List<SelectListItem>();  

        allItems.AddRange(Variables.Select(x => new SelectListItem(x.Name, x.Name)));
        allItems.AddRange(InputVariables.Select(x => new SelectListItem(x.DisplayName, x.Name)));
        allItems.AddRange(OutputVariables.Select(x => new SelectListItem(x.DisplayName, x.Name)));

        _items = allItems.OrderBy(x => x.Text).ToList(); 
    }

    private SelectListItem? GetSelectedValue()
    {
        var outputName = EditorContext.GetExpressionValueOrDefault();
        return _items.FirstOrDefault(x => x.Value == outputName);
    }

    private async Task OnValueChanged(SelectListItem? value)
    {
        var outputName = value?.Value ?? "";
        await EditorContext.UpdateExpressionAsync(Expression.CreateLiteral(outputName));
    }
}