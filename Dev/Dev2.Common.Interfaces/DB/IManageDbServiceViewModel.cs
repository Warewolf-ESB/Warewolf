
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
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.DB
{
    public interface IManageDbServiceViewModel : IServiceMappings
    {
        ObservableCollection<IDbSource> Sources { get; set; }
        IDbSource SelectedSource { get; set; }
        IDbAction SelectedAction { get; set; }
        ICollection<IDbAction> AvalaibleActions { get; set; }
        string DataSourceHeader { get; }
        string DataSourceActionHeader { get; }
        ICommand EditSourceCommand { get; set; }
        bool CanEditSource { get; set; }
        ICommand RefreshCommand { get; set; }
        bool IsRefreshing { get; set; }
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        bool InputsRequired { get; set; }

        // ReSharper restore ReturnTypeCanBeEnumerable.Global
        DataTable TestResults { get; set; }
        bool IsTestResultsEmptyRows { get; set; }
        ICommand CreateNewSourceCommand { get; set; }
        ICommand TestProcedureCommand { get; set; }
        bool IsTesting { get; set; }

        ICommand SaveCommand { get; set; }
        bool CanSelectProcedure { get; set; }
        bool CanEditMappings { get; set; }
        bool CanTest { get; set; }
        string Path { get; set; }
        string Name { get; set; }
        Guid Id { get; set; }
        bool TestSuccessful { get; set; }
        bool TestResultsAvailable { get; set; }
        string ErrorText { get; set; }

        bool ShowRecordSet { get; set; }
    }
}
