using System.Windows;
using Infragistics.Windows.Themes;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for Shell.xaml
    /// </summary>
    public partial class Shell : Window
    {
        public Shell()
        {
            InitializeComponent();
            ThemeManager.CurrentTheme = "Metro";
        }

        

    }
}
