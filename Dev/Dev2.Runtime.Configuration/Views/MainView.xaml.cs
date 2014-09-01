using System.Windows;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.ViewModels;

namespace Dev2.Runtime.Configuration.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            MainViewModel mainViewModel = DataContext as MainViewModel;

            if(mainViewModel != null)
            {
                mainViewModel.SelectedSettingsObjects = e.NewValue as SettingsObject;
            }
        }
    }
}
