using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariablelistViewRecordSetViewModel : IVariableListItemViewModel
    {
        ICollection<IVariableListViewColumnViewModel> Columns { get; }
        void AddColumn(IVariableListViewColumnViewModel variableListViewColumn);
        void RemoveColumn(IVariableListViewColumnViewModel variableListViewColumn);
        ICollection<IVariablelistViewRecordSetViewModel> ParentCollection { get; }
    }
}