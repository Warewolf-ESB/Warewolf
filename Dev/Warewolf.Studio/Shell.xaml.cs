using System.Windows;
using Infragistics.Themes;
using Warewolf.Studio.Themes.Luna;

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
            ThemeManager.ApplicationTheme = new LunaTheme();

        }


    }
}
