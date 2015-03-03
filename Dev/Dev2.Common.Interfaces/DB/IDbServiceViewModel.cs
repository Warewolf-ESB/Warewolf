using System.Collections.Generic;
using System.Data;
using System.Windows.Input;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;

namespace Dev2.Common.Interfaces.DB
{
    public interface IManageDbServiceViewModel
    {
        ICollection<IDbSource> Sources { get; set; }
        IManageDatabaseSourceViewModel SelectedSource { get; set; }
        IDbAction SelectedAction { get; set; }
        ICollection<IDbAction> AvalaibleActions { get; set; }
        IDbOutput Outputs { get; set; }
        string DataSourceHeader { get; }
        string DataSourceActionHeader { get; }
        ICommand EditSourceCommand { get; }
        bool CanEditSource { get; }
        string NewButtonLabel { get; }
        ICollection<string> Actions { get; }
        string MappingsHeader { get; }
        string TestHeader { get; }
        string InputsLabel { get; }
        string InspectLabel { get; }
        string OutputsLabel { get; }
        string MappingNamesHeader { get; }
        ICollection<IDbInput> Inputs { get; }
        DataTable TestResults { get; set; }
        ICommand CreateNewSourceCommand { get; set; }
        ICommand TestProcedureCommand { get; set; }
        IList<IDbOutputMapping> OutputMapping { get; set; }
        ICommand SaveCommand { get; set; }
        bool CanSelectProcedure { get; set; }
        bool CanEditMappings { get; set; }
        bool CanTest { get; set; }
    }

    public interface IDbOutput
    {
    }
}
