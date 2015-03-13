using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.ServerProxyLayer;

namespace Dev2.Common.Interfaces.DB
{
    public interface IManageDbServiceViewModel
    {
        ICollection<IDbSource> Sources { get; set; }
        IDbSource SelectedSource { get; set; }
        IDbAction SelectedAction { get; set; }
        ICollection<IDbAction> AvalaibleActions { get; set; }
        string DataSourceHeader { get; }
        string DataSourceActionHeader { get; }
        ICommand EditSourceCommand { get; }
        bool CanEditSource { get; }
        string NewButtonLabel { get; }
        string MappingsHeader { get; }
        string TestHeader { get; }
        string InputsLabel { get; }
        string OutputsLabel { get; }
        // ReSharper disable ReturnTypeCanBeEnumerable.Global
        ICollection<IDbInput> Inputs { get; }
        // ReSharper restore ReturnTypeCanBeEnumerable.Global
        DataTable TestResults { get; set; }
        ICommand CreateNewSourceCommand { get; set; }
        ICommand TestProcedureCommand { get; set; }
        IList<IDbOutputMapping> OutputMapping { get; set; }
        ICommand SaveCommand { get; set; }
        bool CanSelectProcedure { get; set; }
        bool CanEditMappings { get; set; }
        bool CanTest { get; set; }
        string Path { get; set; }
        string Name { get; set; }
       Guid Id{get;set;}
       bool TestSuccessful { get; set; }
       bool TestResultsAvailable { get; set; }
       string ErrorText { get; set; }
       string RecordsetName { get; set; }
    }

    public interface IDbOutput
    {
    }
}
