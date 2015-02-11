using System.Collections.Generic;

namespace Dev2.Common.Interfaces.DataList.DatalistView
{
    public interface IVariableListViewScalarViewModel : IVariableListItemViewModel
    {

        ICollection<IVariableListViewScalarViewModel> ParentCollection { get; }
    }




}