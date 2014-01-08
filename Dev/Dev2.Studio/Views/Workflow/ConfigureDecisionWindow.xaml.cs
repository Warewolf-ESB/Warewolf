using System.Windows;

// ReSharper disable once CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Views
{
    /// <summary>
    /// Interaction logic for ConfigureDecisionWindow.xaml
    /// </summary>
    public partial class ConfigureDecisionWindow
    {
        public ConfigureDecisionWindow()
        {
            InitializeComponent();
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            dynamic data = DataContext;
            if(!data.CanSelect)
            {
                MessageBox.Show("Please choose a decision type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }
    }
}
