using System.Windows;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics.Debug;
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

        protected override void OnExit(ExitEventArgs e)
        {
            DebugDispatcher.Instance.Shutdown();
            base.OnExit(e);
        }
    }
}
