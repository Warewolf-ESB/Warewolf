using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Dev2.Activities.Designers2.Core.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesigner<TViewModel> :
        ActivityDesigner<TViewModel>
        where TViewModel : ActivityCollectionDesignerViewModel
    {
        MenuItem _insertRowMenuItem;
        MenuItem _deleteRowMenuItem;

        Dev2DataGrid TheGrid
        {
            get
            {
                var activityDesignerTemplate = (ActivityDesignerTemplate)Content;
                return activityDesignerTemplate.DataGrid;
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
            InitializeContextMenu();
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            // Massimo.Guerrera BUG_10181 - This is the only way we cound find if the right click happened in the datagrid
            var tb = e.OriginalSource as DependencyObject;
            var dataGrid = tb.GetSelfAndAncestors().OfType<DataGrid>().ToList();

            var showMenuItems = dataGrid.Count > 0 && (dataGrid[0].SelectedIndex != (dataGrid[0].Items.Count - 1));
            if(_deleteRowMenuItem != null)
            {
                _deleteRowMenuItem.IsEnabled = showMenuItems;
            }
            if(_insertRowMenuItem != null)
            {
                _insertRowMenuItem.IsEnabled = showMenuItems;
            }
        }

        void InitializeContextMenu()
        {
            var ctxMenu = new ContextMenu();

            _insertRowMenuItem = new MenuItem { Header = "Insert Row" };
            _insertRowMenuItem.Click += InsertDataGridRow;

            _deleteRowMenuItem = new MenuItem { Header = "Delete Row" };
            _deleteRowMenuItem.Click += DeleteDataGridRow;

            ctxMenu.Items.Add(_insertRowMenuItem);
            ctxMenu.Items.Add(_deleteRowMenuItem);

            ContextMenu = ctxMenu;
        }

        protected void DeleteDataGridRow(object sender, RoutedEventArgs e)
        {
            var theGrid = TheGrid;
            if(theGrid != null)
            {
                theGrid.RemoveRow(TheGrid.SelectedIndex);
                ViewModel.UpdateDisplayName();
            }
        }

        protected void InsertDataGridRow(object sender, RoutedEventArgs e)
        {
            var theGrid = TheGrid;
            if(theGrid != null)
            {
                theGrid.InsertRow(TheGrid.SelectedIndex);
                ViewModel.UpdateDisplayName();
            }
        }

        protected void AddDataGridRow(object sender, RoutedEventArgs e)
        {
            var theGrid = TheGrid;
            if(theGrid != null)
            {
                theGrid.AddRow();
                ViewModel.UpdateDisplayName();
            }
        }
    }
}