using Dev2.ViewModels.Search;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Controls;
using System.Windows.Input;

namespace Dev2.Views.Search
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : IView
    {
        public SearchView()
        {
            InitializeComponent();
            txtSearchInput.Focus();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is SearchViewModel viewModel)
            {
                viewModel.SearchValue.SearchInput = txtSearchInput.Text;
                viewModel.SearchInputCommand.Execute(null);
            }
        }

        private void CbSearchAll_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && DataContext is SearchViewModel viewModel)
            {
                viewModel.SearchValue.SearchOptions.UpdateAllStates(checkBox.IsChecked.Value);
            }
        }
    }
}
