using Dev2.ViewModels.Search;
using System.Windows.Input;

namespace Dev2.Views.Search
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView
    {
        public SearchView()
        {
            InitializeComponent();
            txtSearchInput.Focus();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var viewModel = DataContext as SearchViewModel;
                if (viewModel != null)
                {
                    viewModel.SearchInput = txtSearchInput.Text;
                    viewModel.SearchInputCommand.Execute(null);
                }
            }
        }
    }
}
