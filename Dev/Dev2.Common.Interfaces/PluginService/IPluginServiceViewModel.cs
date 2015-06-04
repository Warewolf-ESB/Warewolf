using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces.PluginService
{
    public interface IPluginServiceViewModel :IServiceMappings
    {

        ICollection<IPluginSource> Sources { get; set; }
        IPluginSource SelectedSource { get; set; }
        IPluginAction SelectedAction { get; set; }
        ICollection<IPluginAction> AvalaibleActions { get; set; }
        string PluginSourceHeader { get; }
        string PluginSourceActionHeader { get; }
        ICommand EditSourceCommand { get; }
        bool CanEditSource { get; }
        bool CanEditNamespace { get; set; }
        string NewButtonLabel { get; }
        string TestHeader { get; }
        string InputsLabel { get; }
        string OutputsLabel { get; }
        ICommand RefreshCommand { get; set; }
        bool IsRefreshing { get; set; }
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        bool InputsRequired { get; set; }

        // ReSharper restore ReturnTypeCanBeEnumerable.Global
        string TestResults { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        ICommand CreateNewSourceCommand { get; set; }
        ICommand TestPluginCommand { get; set; }
        bool IsTesting { get; set; }

        ICommand SaveCommand { get; set; }
        bool CanSelectMethod { get; set; }
        bool CanEditMappings { get; set; }
        bool CanTest { get; set; }
        string Path { get; set; }
        string Name { get; set; }
        Guid Id { get; set; }
        bool TestSuccessful { get; set; }
        bool TestResultsAvailable { get; set; }
        string ErrorText { get; set; }

        bool ShowResults { get; set; }
        ICollection<INamespaceItem> NameSpaces { get; set; }
        INamespaceItem SelectedNamespace { get; set; }
        ICollection<NameValue> InputValues { get; set; }
    }
}
