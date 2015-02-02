using System.Windows.Controls;
using Warewolf.Studio.Core.View_Interfaces;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for MenuView.xaml
    /// </summary>
    public partial class MenuView : UserControl, IMenuView
    {
        public MenuView()
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
