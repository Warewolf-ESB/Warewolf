using System.Windows;
using Infragistics.Themes;
using Warewolf.Studio.Themes.Luna;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ThemeManager.ApplicationTheme = new LunaTheme();
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}
