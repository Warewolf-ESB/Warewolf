using System.Windows;
using System.Windows.Controls;

namespace Warewolf.Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Bootstrapper bootstrapper = new Bootstrapper();
            bootstrapper.Run();
            //EventManager.RegisterClassHandler(typeof(Control), UIElement.GotFocusEvent, new RoutedEventHandler(GotFocus));
        }

        private void GotFocus(object sender, RoutedEventArgs e)
        {
            var control = e.Source as Control;
            if (control != null)
            {
                var name = control.Name;
            }
        }
    }

    public class SetHelpEvent
    {
    }
}
