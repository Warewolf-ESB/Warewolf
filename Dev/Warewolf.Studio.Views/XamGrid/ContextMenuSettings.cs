using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Infragistics.Controls.Grids;
using Infragistics.Controls.Menus;

namespace Warewolf.Studio.Views.XamGridEx
{
    public class ContextMenuSettings : Infragistics.Controls.Grids.SettingsBase
    {
        ContextMenuManager _contextMenuMgr;

        public XamContextMenu ContextMenu { get; set; }

        public bool AllowContextMenu
        {
            get
            {
                return (bool)GetValue(AllowContextMenuProperty);
            }
            set
            {
                SetValue(AllowContextMenuProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for EnableContextMenu.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AllowContextMenuProperty =
            DependencyProperty.Register("AllowContextMenu", typeof(bool), typeof(ContextMenuSettings), new PropertyMetadata(false, AllowContextMenuChanged));

        static void AllowContextMenuChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var settings = (ContextMenuSettings)obj;
            settings.OnAllowContextMenuChanged((bool)e.NewValue);
        }

        internal virtual void OnAllowContextMenuChanged(bool value)
        {
            ResetContextMenuSettings();
        }

        /// <summary>
        ///     Hack to get around the fact that the SettingsBase.Grid property setter is internal
        /// </summary>
        public XamGrid Grid
        {
            get
            {
                return base.Grid;
            }
            set
            {
                base.Grid = value;
            }
        }

        /// <summary>
        ///     Update the state of the context menu when the Grid property is set
        /// </summary>
        protected override void OnGridSet()
        {
            ResetContextMenuSettings();
        }

        /// <summary>
        ///     We use this method to return the object to a known good state
        ///     when properties of the ContextMenuSettings object change.
        /// </summary>
        void ResetContextMenuSettings()
        {
            if(Grid == null)
            {
                return;
            }

            if(AllowContextMenu)
            {
                AttachContextMenu();
            }
            else
            {
                DetachContextMenu();
            }
        }

        /// <summary>
        ///     Sets up and attached the context menu to the grid
        /// </summary>
        void AttachContextMenu()
        {
            //create a ContextMenuManager if we need to
            if(_contextMenuMgr == null)
            {
                _contextMenuMgr = new ContextMenuManager();
                _contextMenuMgr.ContextMenu = ContextMenu;
                _contextMenuMgr.ContextMenu.Opening += ContextMenuOpening;
                _contextMenuMgr.ModifierKeys = ModifierKeys.None;
                _contextMenuMgr.OpenMode = OpenMode.RightClick;

                ContextMenuService.SetManager(Grid, _contextMenuMgr);
            }
        }

        /// <summary>
        ///     Removes the attached context menu from the grid
        /// </summary>
        void DetachContextMenu()
        {
            if(_contextMenuMgr != null)
            {
                if(_contextMenuMgr.ContextMenu != null)
                {
                    _contextMenuMgr.ContextMenu.Opening -= ContextMenuOpening;
                }

                ContextMenuService.SetManager(Grid, null);
            }

            _contextMenuMgr = null;
        }

        /// <summary>
        ///     Handles the XamContextMenu Opening event, verifies the element clicked is a CellControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ContextMenuOpening(object sender, OpeningEventArgs e)
        {
            List<CellControl> elements = e.GetClickedElements<CellControl>();

            if(elements.Count == 0)
            {
                e.Cancel = true;
            }

            //Have to manually transfer menu args to my event args
            var args = new ContextMenuOpeningEventArgs { Cancel = e.Cancel, MouseClickLocation = e.MouseClickLocation };
            var firstOrDefault = elements.FirstOrDefault();
            if(firstOrDefault != null)
            {
                args.Cell = firstOrDefault.Cell;
            }
            args.Menu = _contextMenuMgr.ContextMenu;

            _contextMenuMgr.ContextMenu.DataContext = args.Cell.Row.Data;

            //Cast here because 'Grid' will return the base XamGrid type
            ((XamGridEx)Grid).OnContextMenuOpening(this, args);

            if(args.Cancel)
            {
                e.Cancel = true;
            }
        }
    }
}