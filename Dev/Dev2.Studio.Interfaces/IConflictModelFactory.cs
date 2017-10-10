using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;
using Dev2.Studio.Interfaces.DataList;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictModelFactory
    {
        string WorkflowName { get; set; }
        string ServerName { get; set; }
        bool IsVariablesChecked { get; set; }
        bool IsWorkflowNameChecked { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
        IMergeToolModel GetModel(string switchName = "");
        IMergeToolModel Model { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        void GetDataList();
        event ConflictModelChanged SomethingConflictModelChanged;
    }
}