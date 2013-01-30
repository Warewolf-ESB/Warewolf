using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.ViewModels.Base;
using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Dev2.Studio.InterfaceImplementors
{
    /// <summary>
    /// This is the default implementation which uses the standard Window.Show and Window.ShowDialog()
    /// Do not used this in tests!
    /// </summary>
    [Export(typeof(IDev2WindowManager))]
    public class Dev2WindowManager : IDev2WindowManager
    {
        public void Show(BaseViewModel viewModel)
        {
            var window = LocateView(viewModel.GetType());
            if (window == null) return;

            window.Owner = Application.Current.MainWindow;
            window.DataContext = viewModel;
            window.Show();
        }

        public void Show(Window window, BaseViewModel viewModel)
        {
            window.Owner = Application.Current.MainWindow;
            window.DataContext = viewModel;
            window.Show();
        }

        public void ShowDialog(Window window, BaseViewModel viewModel)
        {
            window.Owner = Application.Current.MainWindow;
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        public void ShowDialog(BaseViewModel viewModel)
        {
            var window = LocateView(viewModel.GetType());
            if (window == null) return;

            window.Owner = Application.Current.MainWindow;
            window.DataContext = viewModel;
            window.ShowDialog();
        }

        private static Window LocateView(Type typeOf)
        {
            var locateName = typeOf.Name.ToLower().Replace("model","");

            var type = ((typeOf.Assembly.GetTypes().FirstOrDefault(t => t.Name.ToLower() == locateName) ??
                        Assembly.GetExecutingAssembly().GetTypes().FirstOrDefault(t => t.Name.ToLower() == locateName)) ??
                        Assembly.GetEntryAssembly().GetTypes().FirstOrDefault(t => t.Name.ToLower() == locateName)) ??
                        Assembly.GetCallingAssembly().GetTypes().FirstOrDefault(t => t.Name.ToLower() == locateName);

            Window item = null;

            if (type != null && !type.IsAbstract && !type.IsInterface)
            {
                item = (Window)Activator.CreateInstance(type, true);
            }

            return item;
        }
    }
}
