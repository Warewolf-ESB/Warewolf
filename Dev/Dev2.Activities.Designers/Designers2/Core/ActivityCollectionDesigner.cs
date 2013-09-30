using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using Caliburn.Micro;
using EventTrigger = System.Windows.Interactivity.EventTrigger;

namespace Dev2.Activities.Designers2.Core
{
    public abstract class ActivityCollectionDesigner<TViewModel> :
        ActivityDesigner<TViewModel>
        where TViewModel : ActivityCollectionDesignerViewModel
    {
        MenuItem _insertRowMenuItem;
        MenuItem _deleteRowMenuItem;

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

            _insertRowMenuItem = CreateMenuItem("Insert Row");
            var insertEvtTrigger = CreateEventTrigger("InsertItem");
            Interaction.GetTriggers(_insertRowMenuItem).Add(insertEvtTrigger);

            _deleteRowMenuItem = CreateMenuItem("Delete Row");
            var deleteEvtTrigger = CreateEventTrigger("DeleteItem");
            Interaction.GetTriggers(_deleteRowMenuItem).Add(deleteEvtTrigger);

            ctxMenu.Items.Add(_insertRowMenuItem);
            ctxMenu.Items.Add(_deleteRowMenuItem);

            ContextMenu = ctxMenu;
        }

        static EventTrigger CreateEventTrigger(string methodName)
        {
            var m = new ActionMessage
            {
                MethodName = methodName
            };
            var evtTrigger = new EventTrigger
            {
                EventName = "Click"
            };
            evtTrigger.Actions.Add(m);
            return evtTrigger;
        }

        static MenuItem CreateMenuItem(string header)
        {
            var menuItem = new MenuItem
            {
                Header = header
            };

            return menuItem;
        }
    }
}