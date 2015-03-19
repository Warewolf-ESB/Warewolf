using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces.DataList.DatalistView;
using Microsoft.Practices.Prism.Mvvm;

namespace Dev2.Common.Interfaces
{
    public interface IVariableListView : IView {
        List<IVariableListViewScalarViewModel> GetAllScalarVariables();

        List<IVariablelistViewRecordSetViewModel> GetAllRecordSetVariables();

        void DeleteUnusedVariables();

        void Search(string searchTerm);

        void ClearFilter();

        void Sort();

        Visibility GetFilterBoxVisibility();
    }
}
