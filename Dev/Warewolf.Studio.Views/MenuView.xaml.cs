using System.Windows.Controls;
using System.Windows.Input;
using Dev2.Common.Interfaces.Studio;
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

    }
}
