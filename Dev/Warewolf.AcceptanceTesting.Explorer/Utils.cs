using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.AcceptanceTesting.Explorer
{
    public class Utils
    {
        public const string ViewNameKey = "view";

        public static void ShowTheViewForTesting(IView view)
        {
            var window = new Window {Content = view};
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var manageDatabaseSourceControl = Application.Current.MainWindow.Content as IView;
                Assert.IsNotNull(manageDatabaseSourceControl);
                Assert.IsNotNull(manageDatabaseSourceControl.DataContext);
                Application.Current.Shutdown();
            }));

            Application.Current.Run(Application.Current.MainWindow);
        }

        public static void CheckControlEnabled(string controlName, string enabledString, ICheckControlEnabledView view)
        {
            var isEnabled = String.Equals(enabledString, "Enabled", StringComparison.InvariantCultureIgnoreCase);
            var controlEnabled = view.GetControlEnabled(controlName);
            Assert.AreEqual(isEnabled, controlEnabled);
        }
    }
}