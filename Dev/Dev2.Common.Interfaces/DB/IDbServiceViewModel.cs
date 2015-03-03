using System.Collections.Generic;
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
        ICommand CreateNewSourceCommand { get; set; }
        ICommand TestProcedureCommand { get; set; }
    }

    public interface IDbOutput
    {
    }
    public interface IDbInput
    {
        string Name{ get; set; }
        string Value { get; set; }
    }

    public interface IDbAction
    {
        IList<IServiceInput> Inputs { get; set; }
        string Name { get; set; }
        IDictionary<string, List<string>> Test();
    }

    public interface IServiceInput
    {
        string Name { get; set; }
        string Value { get; set; }
        bool Required { get; set; }
        bool EmptyIsNull { get; set; }
    }


}
