using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesigner<TViewModel> :
        ActivityDesigner<TViewModel>
        where TViewModel : ActivityCollectionDesignerViewModel
    {
        MenuItem _insertRowMenuItem;
        MenuItem _deleteRowMenuItem;

        DataGrid TheGrid
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
            _dataContext.ModelItemCollection.CollectionChanged += ModelItemCollectionCollectionChanged;
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);

            var theGrid = TheGrid;
            if(theGrid == null)
            {
                return;
            }

            _deleteRowMenuItem.IsEnabled = ViewModel.CanRemoveAt(theGrid.SelectedIndex + 1);
            _insertRowMenuItem.IsEnabled = ViewModel.CanInsertAt(theGrid.SelectedIndex + 1);
        }

        void ModelItemCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var theGrid = TheGrid;

            if(theGrid != null && e.Action != NotifyCollectionChangedAction.Add)
            {
                theGrid.Items.Refresh();
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
                ViewModel.RemoveAt(TheGrid.SelectedIndex + 1);
                theGrid.Items.Refresh();
            }
        }

        protected void InsertDataGridRow(object sender, RoutedEventArgs e)
        {
            var theGrid = TheGrid;
            if(theGrid != null)
            {
                ViewModel.InsertAt(TheGrid.SelectedIndex + 1);
                theGrid.Items.Refresh();
            }
        }
    }
}