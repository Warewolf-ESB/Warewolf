using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
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

            var comparer = new CounterByResoureEqualityComparer(direction, column.Key);
            lcv.CustomSort = comparer;
            dataGrid.ItemsSource = lcv;
            e.Cancel = true;
        }
    }
}
