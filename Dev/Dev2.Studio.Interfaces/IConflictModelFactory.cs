using System.Collections.ObjectModel;
using Dev2.Common.Interfaces;

namespace Dev2.Studio.Interfaces
{
    public interface IConflictModelFactory
    {
        string WorkflowName { get; set; }
        bool IsVariablesChecked { get; set; }
        bool IsWorkflowNameChecked { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
        IMergeToolModel GetModel(string switchName = "");
        IMergeToolModel Model { get; set; }
        void GetDataList();
        event ConflictModelChanged SomethingConflictModelChanged;
    }
}