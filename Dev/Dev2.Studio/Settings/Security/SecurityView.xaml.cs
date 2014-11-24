
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
