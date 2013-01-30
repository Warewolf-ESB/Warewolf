using System.Windows;

namespace Unlimited.Applications.BusinessDesignStudio.Views {
    /// <summary>
    /// Interaction logic for ConfigureDecisionWindow.xaml
    /// </summary>
    public partial class ConfigureDecisionWindow : Window {
        public ConfigureDecisionWindow() {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            dynamic data = this.DataContext;
            if (!data.CanSelect) {
                MessageBox.Show("Please choose a decision type", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Cancel = true;
            }
        }
    }
}
