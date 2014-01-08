using System.Windows;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for DsfAdminWindow.xaml
    /// </summary>
    public partial class DsfAdminWindow
    {
        public DsfAdminWindow()
        {
            InitializeComponent();

        }

        private void DocumentContentLoaded(object sender, RoutedEventArgs e)
        {
            dynamic dataContext = DataContext;
            dataContext.Console = TxtConsole;
            dataContext.CommandText = TxtUserCommand;
            TxtUserCommand.Focus();
        }


    }
}
