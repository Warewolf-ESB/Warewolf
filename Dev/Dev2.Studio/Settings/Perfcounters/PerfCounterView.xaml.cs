using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Services.Security;
using Infragistics.Controls.Grids;

namespace Dev2.Settings.Perfcounters
{
    /// <summary>
    /// Interaction logic for PerfCounterView.xaml
    /// </summary>
    public partial class PerfCounterView : UserControl
    {
        public PerfCounterView()
        {
            InitializeComponent();
        }
         ListSortDirection? _previousServerDirection;
        ListSortDirection? _previousResourceDirection;



        void OnDataGridSorting(object sender, DataGridSortingEventArgs e)
        {
            var dataGrid = (DataGrid)sender;
            var column = e.Column;

            // prevent the built-in sort from sorting
            e.Handled = true;

            var direction = column.SortDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;

            //set the sort order on the column
            column.SortDirection = direction;

            //use a ListCollectionView to do the sort.
            var lcv = (ListCollectionView)CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);

            //apply the sort
            lcv.CustomSort = new WindowsGroupPermissionComparer(direction, column.SortMemberPath);
        }

        private void DataGrid_LoadingRow(Object sender, DataGridRowEventArgs e)
        {
            e.Row.Tag = e.Row.GetIndex();
        }

        void ServerColumnSorting(object sender, SortingCancellableEventArgs e)
        {
            if (_previousServerDirection == null)
            {
                _previousServerDirection = ListSortDirection.Ascending;
            }

            Sort(sender, e);
        }
        
        void ResourceColumnSorting(object sender, SortingCancellableEventArgs e)
        {
            if (_previousResourceDirection == null)
            {
                _previousResourceDirection = ListSortDirection.Ascending;
            }

            Sort(sender, e);
        }

        void Sort(object sender, SortingCancellableEventArgs e)
        {
            var dataGrid = (XamGrid)sender;
            var column = e.Column;
            var direction = _previousServerDirection != ListSortDirection.Ascending ? ListSortDirection.Ascending : ListSortDirection.Descending;
            _previousServerDirection = direction;
            var collectionView = CollectionViewSource.GetDefaultView(dataGrid.ItemsSource);
            var lcv = (ListCollectionView)collectionView;

            var windowsGroupPermissionComparer = new WindowsGroupPermissionComparer(direction, column.Key);
            lcv.CustomSort = windowsGroupPermissionComparer;
            dataGrid.ItemsSource = lcv;
            e.Cancel = true;
        }
    }
}
