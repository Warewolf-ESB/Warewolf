using System;
using System.Windows;
using System.Windows.Threading;
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;

namespace Warewolf.AcceptanceTesting.Explorer
{
    public class Utils
    {
        public const string ViewNameKey = "view";
        public const string ViewModelNameKey = "viewModel";

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

        public static T GetViewModel<T>()
        {
            var viewModel = ScenarioContext.Current.Get<T>(ViewModelNameKey);
            return viewModel;
        }

        public static T GetView<T>()
        {
            var view = ScenarioContext.Current.Get<T>(ViewNameKey);
            return view;
        }

        public static void CheckControlEnabled(string controlName, string enabledString, ICheckControlEnabledView view)
        {
            var isEnabled = String.Equals(enabledString, "Enabled", StringComparison.InvariantCultureIgnoreCase);
            var controlEnabled = view.GetControlEnabled(controlName);
            Assert.AreEqual(isEnabled, controlEnabled);
        }
    }
}