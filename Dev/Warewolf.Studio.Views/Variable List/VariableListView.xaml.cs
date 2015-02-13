using System.Windows.Controls;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for VariableListView.xaml
    /// </summary>
    public partial class VariableListView : UserControl,IVariableListView
    {
        public VariableListView()
        {
            InitializeComponent();
        }

        #region Implementation of IWarewolfView

        public void Blur()
        {
        }

        public void UnBlur()
        {
        }

        #endregion
    }
}
