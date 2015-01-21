using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewColumnViewModel : IVariableListViewColumn,IVariableListItemViewModel
    {
        ICollection<IVariableListViewColumnViewModel> ParentCollection { get; }

    }
}