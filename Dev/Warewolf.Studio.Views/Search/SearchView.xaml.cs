using Dev2.ViewModels.Search;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows.Input;

namespace Dev2.Views.Search
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : IView
    {
        SearchViewModel ViewModel => DataContext as SearchViewModel;

        public SearchView()
        {
            InitializeComponent();
            txtSearchInput.Focus();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.Search.SearchInput = txtSearchInput.Text;
                ViewModel.SearchInputCommand.Execute(null);
            }
        }

        private void CbSearchAll_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UpdateIsAllSelected() || !UpdateIsAllSelected())
            {
                ViewModel.Search.SearchOptions.UpdateAllStates(true);
            }
        }

        private void CbSearchAll_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UpdateIsAllSelected())
            {
                ViewModel.Search.SearchOptions.UpdateAllStates(false);
            }
        }

        private void CbSearch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.Search.SearchOptions.IsAllSelected = UpdateIsAllSelected();
        }

        private bool UpdateIsAllSelected()
        {
            var areAllChecked = ViewModel.Search.SearchOptions.IsWorkflowNameSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsToolTitleSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsScalarNameSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsObjectNameSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsRecSetNameSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsInputVariableSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsOutputVariableSelected;
            areAllChecked &= ViewModel.Search.SearchOptions.IsTestNameSelected;

            return areAllChecked;
        }
    }
}
