using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using Caliburn.Micro;
using Dev2.Interfaces;
using Dev2.Studio.Core.Activities.Utils;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using EventTrigger = System.Windows.Interactivity.EventTrigger;

namespace Dev2.Activities.Designers
{
    public abstract class ActivityDesignerCollectionBase<TViewModel> :
        ActivityDesignerBase<TViewModel>
        where TViewModel : ActivityCollectionViewModelBase
    {
        private bool _showRightClickOptions;

        protected override void OnModelItemChanged(object newItem)
        {
            base.OnModelItemChanged(newItem);
            InitializeContextMenu();
        }

        public bool ShowRightClickOptions
        {
            get
            {
                return _showRightClickOptions;
            }
            set
            {
                if(_showRightClickOptions == value)
                {
                    return;
                }

                _showRightClickOptions = value;
                OnPropertyChanged();
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            // Massimo.Guerrera BUG_10181 - This is the only way we cound find if the right click happened in the datagrid
            DependencyObject tb = e.OriginalSource as DependencyObject;
            var dataGrid = tb.GetSelfAndAncestors().OfType<DataGrid>().ToList();

            if (dataGrid.Count > 0)
            {
                ShowRightClickOptions = (dataGrid[0].SelectedIndex != (dataGrid[0].Items.Count - 1));
            }
            else
            {
                ShowRightClickOptions = false;
            }           
        }

        private void InitializeContextMenu()
        {
            var ctxMenu = new ContextMenu();

            var insertRowMenuItem = CreateMenuItem("Insert Row");
            var insertEvtTrigger = CreateEventTrigger("InsertItem");
            Interaction.GetTriggers(insertRowMenuItem).Add(insertEvtTrigger);

            var deleteRowMenuItem = CreateMenuItem("Delete Row");
            var deleteEvtTrigger = CreateEventTrigger("DeleteItem");
            Interaction.GetTriggers(deleteRowMenuItem).Add(deleteEvtTrigger);

            ctxMenu.Items.Add(insertRowMenuItem);
            ctxMenu.Items.Add(deleteRowMenuItem);

            ContextMenu = ctxMenu;
        }

        private static EventTrigger CreateEventTrigger(string methodName)
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

        private MenuItem CreateMenuItem(string header)
        {
            var menuItem = new MenuItem
                {
                    Header = header
                };

            var rightClickBinding = new Binding
            {
                Path = new PropertyPath("ShowRightClickOptions"),
                Source = this
            };

            menuItem.SetBinding(IsEnabledProperty, rightClickBinding);
            return menuItem;
        }
    }
}
