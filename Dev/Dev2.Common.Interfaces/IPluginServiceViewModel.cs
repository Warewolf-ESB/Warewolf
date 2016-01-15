
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces.DB;

namespace Dev2.Common.Interfaces
{
    public interface IPluginServiceViewModel : IServiceMappings
    {
        ObservableCollection<IPluginSource> Sources { get; set; }
        IPluginSource SelectedSource { get; set; }
        IPluginAction SelectedAction { get; set; }
        ICollection<IPluginAction> AvalaibleActions { get; set; }
        ICommand EditSourceCommand { get; }
        bool CanEditSource { get; }
        bool CanEditNamespace { get; set; }
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
        string TestResultString { get; set; }
    }
}
