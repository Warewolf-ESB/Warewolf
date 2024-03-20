using Elsa.Api.Client.Resources.StorageDrivers.Models;
using Elsa.Api.Client.Resources.VariableTypes.Models;
using Elsa.Api.Client.Resources.WorkflowDefinitions.Models;
using Elsa.Studio.Workflows.Domain.Contracts;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Elsa.Studio.Workflows.Components.WorkflowDefinitionEditor.Components.WorkflowProperties.Tabs.InputOutput.Components.Inputs;

public partial class InputsSection
{
    private ICollection<VariableTypeDescriptor> _variableTypes = new List<VariableTypeDescriptor>();
    private ICollection<StorageDriverDescriptor> _storageDriverDescriptors = new List<StorageDriverDescriptor>();

    [Parameter] public WorkflowDefinition WorkflowDefinition { get; set; } = default!;
    [Parameter] public Func<Task>? OnWorkflowDefinitionUpdated { get; set; }
    [Inject] private IStorageDriverService StorageDriverService { get; set; } = default!;
    [Inject] private IDialogService DialogService { get; set; } = default!;
    [Inject] private IVariableTypeService VariableTypeService { get; set; } = default!;

    private ICollection<InputDefinition> Inputs => WorkflowDefinition.Inputs;

    protected override async Task OnInitializedAsync()
    {
        _storageDriverDescriptors = (await StorageDriverService.GetStorageDriversAsync()).ToList();
        _variableTypes = (await VariableTypeService.GetVariableTypesAsync()).ToList();
    }

    private StorageDriverDescriptor? GetStorageDriverDescriptor(string typeName)
    {
        return _storageDriverDescriptors.FirstOrDefault(x => x.TypeName == typeName);
    }

    private async Task RaiseWorkflowDefinitionUpdatedAsync()
    {
        if (OnWorkflowDefinitionUpdated != null)
            await OnWorkflowDefinitionUpdated();
    }

    private async Task OpenInputEditorDialog(InputDefinition? inputDefinition)
    {
        var isNew = inputDefinition == null;

        var parameters = new DialogParameters<EditInputDialog>
        {
            [nameof(EditInputDialog.WorkflowDefinition)] = WorkflowDefinition
        };

        if (!isNew)
            parameters[nameof(EditInputDialog.Input)] = inputDefinition;

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Medium,
            FullWidth = true,
            CloseButton = true,
            CloseOnEscapeKey = true
        };

        var title = inputDefinition == null ? "Create input" : "Edit input";
        var dialog = await DialogService.ShowAsync<EditInputDialog>(title, parameters, options);
        var result = await dialog.Result;

        if (result.Canceled)
            return;

        if (isNew)
        {
            inputDefinition = (InputDefinition)result.Data;
            WorkflowDefinition.Inputs.Add(inputDefinition);
        }

        await RaiseWorkflowDefinitionUpdatedAsync();
    }

    private async Task OnEditClicked(InputDefinition input)
    {
        await OpenInputEditorDialog(input);
    }

    private async Task OnDeleteClicked(InputDefinition input)
    {
        var result = await DialogService.ShowMessageBox("Delete selected input?", "Are you sure you want to delete the selected input?", yesText: "Delete", cancelText: "Cancel");

        if (result != true)
            return;

        WorkflowDefinition.Inputs.Remove(input);

        await RaiseWorkflowDefinitionUpdatedAsync();
    }

    private async Task OnAddInputClicked()
    {
        await OpenInputEditorDialog(null);
    }
}