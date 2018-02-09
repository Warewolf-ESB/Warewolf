using System.Collections.ObjectModel;
using Dev2.Common;
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
        ObservableCollection<IToolModelConflictItem> Children { get; set; }
        IToolModelConflictItem Model { get; set; }
        IDataListViewModel DataListViewModel { get; set; }
        void GetDataList(IContextualResourceModel resourceModel);

        IToolModelConflictItem CreateToolModelConfictItem(IConflictTreeNode node);

        event ConflictModelChanged SomethingConflictModelChanged;
    }
}
