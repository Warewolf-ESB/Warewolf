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
                ViewModel.SearchValue.SearchInput = txtSearchInput.Text;
                ViewModel.SearchInputCommand.Execute(null);
            }
        }

        private void CbSearchAll_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UpdateIsAllSelected() || !UpdateIsAllSelected())
            {
                ViewModel.SearchValue.SearchOptions.UpdateAllStates(true);
            }
        }

        private void CbSearchAll_Unchecked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (UpdateIsAllSelected())
            {
                ViewModel.SearchValue.SearchOptions.UpdateAllStates(false);
            }
        }

        private void CbSearch_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewModel.SearchValue.SearchOptions.IsAllSelected = UpdateIsAllSelected();
        }

        private bool UpdateIsAllSelected()
        {
            var areAllChecked = ViewModel.SearchValue.SearchOptions.IsWorkflowNameSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsToolTitleSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsScalarNameSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsObjectNameSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsRecSetNameSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsInputVariableSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsOutputVariableSelected;
            areAllChecked &= ViewModel.SearchValue.SearchOptions.IsTestNameSelected;

            return areAllChecked;
        }
    }
}
