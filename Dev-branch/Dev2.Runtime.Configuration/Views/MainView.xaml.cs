using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Runtime.Configuration.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MainViewModel mainViewModel = this.DataContext as MainViewModel;

            if (mainViewModel != null)
            {
                mainViewModel.SelectedSettingsObjects = e.NewValue as SettingsObject;
            }
        }
    }
}
