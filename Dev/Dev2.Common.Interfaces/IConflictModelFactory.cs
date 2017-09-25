using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces
{
    public interface IConflictModelFactory
    {
        string WorkflowName { get; set; }
        bool IsVariablesChecked { get; set; }
        bool IsWorkflowNameChecked { get; set; }
        //IMergeToolModel MergeToolModel { get; set; }
        ObservableCollection<IMergeToolModel> Children { get; set; }
        IMergeToolModel GetModel(string switchName = "");
        IMergeToolModel Model { get; set; }
        void GetDataList();
    }
}