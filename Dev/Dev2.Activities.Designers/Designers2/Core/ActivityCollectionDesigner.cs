
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation;
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
        protected override void OnUnloaded()
        {
            base.OnUnloaded();
            _dataContext.ModelItemCollection.CollectionChanged -= ModelItemCollectionCollectionChanged;

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

            if(theGrid != null)
            {
                if(e.Action != NotifyCollectionChangedAction.Add)
                {
                    theGrid.Items.Refresh();
                }
                else
                {
                    theGrid.ScrollIntoView(e.NewItems[0]);
                }
            }


        }

        void InitializeContextMenu()
        {
            _insertRowMenuItem = new MenuItem { Header = "Insert Row" };
            _insertRowMenuItem.SetValue(AutomationProperties.AutomationIdProperty, "UI_InsertRowMenuItem_AutoID");
            _insertRowMenuItem.Click += InsertDataGridRow;

            _deleteRowMenuItem = new MenuItem { Header = "Delete Row" };
            _deleteRowMenuItem.SetValue(AutomationProperties.AutomationIdProperty, "UI_DeleteRowMenuItem_AutoID");
            _deleteRowMenuItem.Click += DeleteDataGridRow;

            ContextMenu.Items.Add(_insertRowMenuItem);
            ContextMenu.Items.Add(_deleteRowMenuItem);
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
        protected override void OnDispose()
        {
            base.OnDispose();
           CEventHelper.RemoveAllEventHandlers(TheGrid);
           CEventHelper.RemoveAllEventHandlers(this);
           CEventHelper.RemoveAllEventHandlers(ModelItem);
            _dataContext.ModelItemCollection.CollectionChanged -= ModelItemCollectionCollectionChanged;
        }
      
    }
}
