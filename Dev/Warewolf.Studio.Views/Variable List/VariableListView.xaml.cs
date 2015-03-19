using System.Collections.Generic;
using System.Windows;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DataList.DatalistView;

namespace Warewolf.Studio.Views.Variable_List
{
    /// <summary>
    /// Interaction logic for VariableListView.xaml
    /// </summary>
    public partial class VariableListView : IVariableListView
    {
        public VariableListView()
        {
            InitializeComponent();
        }


        #region Implementation of IVariableListView

        public List<IVariableListViewScalarViewModel> GetAllScalarVariables()
        {
            return null;
        }

        public List<IVariablelistViewRecordSetViewModel> GetAllRecordSetVariables()
        {
            return null;
        }

        public void DeleteUnusedVariables()
        {
        }

        public void Search(string searchTerm)
        {
        }

        public void ClearFilter()
        {
        }

        public void Sort()
        {
        }

        public Visibility GetFilterBoxVisibility()
        {
            return Visibility.Visible;
        }

        #endregion
    }
}
