using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Dev2.Activities.Designers2.Core.Adorners;
using Dev2.Data.Settings.Security;

namespace Dev2.Settings.Security
{
    /// <summary>
    /// Interaction logic for SecurityView.xaml
    /// </summary>
    public partial class SecurityView
    {
        readonly Help.HelpAdorner _serverHelpAdorner;
        readonly Help.HelpAdorner _resourceHelpAdorner;

        public SecurityView()
        {
            InitializeComponent();
            _serverHelpAdorner = new Help.HelpAdorner(ServerHelpToggleButton);
            _resourceHelpAdorner = new Help.HelpAdorner(ResourceHelpToggleButton);

            DataContextChanged += OnDataContextChanged;
        }

        void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if(args.OldValue != null)
            {
                BindingOperations.ClearBinding(_serverHelpAdorner, AdornerControl.IsAdornerVisibleProperty);
                BindingOperations.ClearBinding(_resourceHelpAdorner, AdornerControl.IsAdornerVisibleProperty);
            }

            if(args.NewValue != null)
            {
                BindingOperations.SetBinding(_serverHelpAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(SecurityViewModel.IsServerHelpVisibleProperty.Name)
                {
                    Source = args.NewValue,
                    Mode = BindingMode.OneWay
                });

                BindingOperations.SetBinding(_resourceHelpAdorner, AdornerControl.IsAdornerVisibleProperty, new Binding(SecurityViewModel.IsResourceHelpVisibleProperty.Name)
                {
                    Source = args.NewValue,
                    Mode = BindingMode.OneWay
                });
            }
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
    }
}
