using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Services.Security;

namespace Dev2.Settings.Security
{
    /// <summary>
    /// Interaction logic for SecurityView.xaml
    /// </summary>
    public partial class SecurityView
    {
        public SecurityView()
        {
            InitializeComponent();
        }

        void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var column = e.Column;

            // prevent the built-in sort from sorting
            e.Handled = true;

            var direction = (column.SortDirection != ListSortDirection.Ascending) ? ListSortDirection.Ascending : ListSortDirection.Descending;

            //set the sort order on the column
            column.SortDirection = direction;

            //use a ListCollectionView to do the sort.
            var lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

            //apply the sort
            lcv.CustomSort = new WindowsGroupPermissionComparer(direction, column.SortMemberPath);
        }

        private void DataGrid_LoadingRow(System.Object sender, DataGridRowEventArgs e)
        {
            e.Row.Tag = e.Row.GetIndex();
        }
    }
}
