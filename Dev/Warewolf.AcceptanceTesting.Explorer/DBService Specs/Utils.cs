using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Studio.Views;

namespace Warewolf.AcceptanceTesting.Explorer.DBService_Specs
{
    public class Utils
    {
        public static void ShowTheViewForTesting(IView view)
        {
            var window = new Window {Content = view};
            var app = Application.Current;
            app.MainWindow = window;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
            {
                var manageDatabaseSourceControl = (ManageDatabaseSourceControl) Application.Current.MainWindow.Content;
                Assert.IsNotNull(manageDatabaseSourceControl);
                Assert.IsNotNull(manageDatabaseSourceControl.DataContext);
                Assert.IsInstanceOfType(manageDatabaseSourceControl.DataContext, typeof (IManageDatabaseSourceViewModel));
                Application.Current.Shutdown();
            }));

            Application.Current.Run(Application.Current.MainWindow);
        }
    }
}