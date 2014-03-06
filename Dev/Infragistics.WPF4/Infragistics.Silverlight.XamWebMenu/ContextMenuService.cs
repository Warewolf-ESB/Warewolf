using System;
using System.Windows;
using Infragistics.Controls.Menus.Primitives;


using System.Windows.Controls.Primitives;
using System.Windows.Input;


namespace Infragistics.Controls.Menus
{

    /// <summary>
    ///  Provides the system implementation for attaching a <see cref="XamContextMenu"/> to elements in a WPF application.
    /// </summary>





    public static class ContextMenuService
    {

        #region Members
        private static bool _handlerAdded;
        private static XamContextMenu _activeContextMenu;
        #endregion //Members

        #region Properties

        #region Public Properties

        #region Manager
        /// <summary>
        /// Identifies the Manager attached dependency property.
        /// </summary>
        /// <seealso cref="GetManager"/>
        /// <seealso cref="SetManager"/>
        public static readonly DependencyProperty ManagerProperty = DependencyProperty.RegisterAttached("Manager",
            typeof(ContextMenuManager), typeof(ContextMenuService), new PropertyMetadata(null, OnManagerChanged));

        private static void OnManagerChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            ContextMenuManager oldManager = e.OldValue as ContextMenuManager;
            ContextMenuManager newManager = e.NewValue as ContextMenuManager;

            if (oldManager != null)
            {
                oldManager.ParentElement = null;
            }

            if (newManager != null)
            {
                newManager.ParentElement = target as UIElement;

                newManager.ParentElement.AddHandler(UIElement.KeyDownEvent, new KeyEventHandler(ContextMenuService.XamContextMenu_KeyDown), true);

            }

            CommandSourceManager.NotifyCanExecuteChanged(typeof(OpenCommand));
            CommandSourceManager.NotifyCanExecuteChanged(typeof(CloseCommand));
        }

        /// <summary>
        /// Gets the value of the <see cref="ContextMenuManager"/> attached property.
        /// </summary>
        /// <param name="element">Object to query concerning the <see cref="ContextMenuManager"/>property.</param>
        /// <returns>Value of the <see cref="ContextMenuManager"/>property.</returns>
        /// <seealso cref="ManagerProperty"/>
        /// <seealso cref="SetManager"/>
        /// <seealso cref="ContextMenuManager"/>
        public static ContextMenuManager GetManager(DependencyObject element)
        {
            return (ContextMenuManager)element.GetValue(ManagerProperty);
        }

        /// <summary>
        /// Sets the value of the <see cref="ContextMenuManager"/>  attached property.
        /// </summary>
        /// <param name="element">Object to set the property on.</param>
        /// <param name="value">The value to set.</param>
        /// <seealso cref="ManagerProperty"/>
        /// <seealso cref="GetManager"/>
        /// <seealso cref="ContextMenuManager"/>
        public static void SetManager(DependencyObject element, ContextMenuManager value)
        {
            element.SetValue(ManagerProperty, value);
        }

        #endregion //ContextMenu

        #endregion //Public Properties

        #endregion //Properties

        #region Methods

        public static bool IsMenuOpenForElement(DependencyObject obj)
        {
            if (obj == null)
                return false;

            if (_activeContextMenu == null || !_activeContextMenu.IsOpen || _activeContextMenu.PlacementTargetResolved == null)
                return false;

            DependencyObject searchingObject = _activeContextMenu.PlacementTargetResolved;
            while (searchingObject != null)
            {
                if (obj == searchingObject)
                    return true;

                searchingObject = PlatformProxy.GetParent(searchingObject);
            }

            return false;
        }

        #region Internal Methods

        internal static void OnContextMenuClosed(XamContextMenu contextMenu)
        {
            if (contextMenu == _activeContextMenu)
            {
                _activeContextMenu = null;
            }
        }

        internal static void OnContextMenuOpened(XamContextMenu xamContextMenu)
        {
            CloseActiveContextMenu();
            _activeContextMenu = xamContextMenu;


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

        }

        internal static Rect GetBounds(UIElement element)
        {
            Rect rect;
            FrameworkElement fe = element as FrameworkElement;

            bool mustRecalcRect = true;

            double x = 0, y = 0;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

            rect = fe != null ? new Rect(x, y, fe.ActualWidth, fe.ActualHeight) : new Rect(0, 0, element.RenderSize.Width, element.RenderSize.Height);

			// AS 7/12/12 TFS117000
			// A width and/or height of 0 is perfectly valid for an element and would not prevent 
			// coordinate transformation. Also we don't have to worry about negative values because 
			// the ActualHeight|Width and RenderSize will never be negative values.
			//
            //if (rect.Height > 0 && rect.Width > 0)
            {
                try
                {
                    UIElement rootVisual = PlatformProxy.GetRootVisual(element);
                    if (rootVisual != null)
                    {
                        
                        rect = element.TransformToVisual(rootVisual).TransformBounds(rect);

                        mustRecalcRect = false;

                    }
                }
                catch (ArgumentException)
                {
                    // TransformToVisual throws "Value does not fall within the expected range."
                }
                catch (InvalidOperationException)
                {
                    // TransformToVisual throws "Not common visual ancestor, so there is no valid transformation"
                }

                
                if (mustRecalcRect)
                {
                    FrameworkElement root = PlatformProxy.GetRootVisual(element) as FrameworkElement;
                    if (root == null)
                    {
                        root = PlatformProxy.GetRootParent(element) as FrameworkElement;
                    }

                    if (root != null)
                    {
                        Popup popup = root as Popup;
                        if (popup != null)
                        {
                            

                            root = popup.Child as FrameworkElement;
                        }

                        if ((root != null) && !(root is Popup))
                        {
                            Point location, p = new Point(0, 0);
                            location = element.PointToScreen(p);
                            p = root.PointToScreen(p);

                            location.X -= p.X;
                            location.Y -= p.Y;

                            rect.X = location.X;
                            rect.Y = location.Y;
                        }
                    }
                }

            }

            return rect;
        }







        public static bool CloseActiveContextMenu()
        {
            if (_activeContextMenu != null)
            {
                _activeContextMenu.IsOpen = false;
            }

            return _activeContextMenu == null || !_activeContextMenu.IsOpen;
        }



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal static void PositionContextMenu(XamContextMenu contextMenu, Point p, UIElement parentElement)
        {
            if (contextMenu != _activeContextMenu)
            {
                // we must close active menu first.
                if (!CloseActiveContextMenu())
                {
                    return; // the old menu stays open
                }
            }

            if (parentElement != null)
            {
                ContextMenuManager manager = GetManager(parentElement);
                if (manager != null && manager.ContextMenu == contextMenu && contextMenu != null)
                {
                    contextMenu.MouseClickLocation = p;
                    contextMenu.ParentElement = parentElement;
                    contextMenu.IsOpen = true;
                }
            }
        }

        #endregion //Internal Methods

        #region Private Methods
        #endregion //Private Methods

        #region Event Handlers



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #endregion //Event Handlers

        #endregion //Methods

        #region EventHandlers


        internal static void XamContextMenu_KeyDown(object sender, KeyEventArgs e)
        {
            if (!e.Handled)
            {
                if (e.Key == Key.Apps)
                {
                    DependencyObject dependObject = sender as DependencyObject;
                    if (dependObject != null)
                    {
                        ContextMenuManager manager = ContextMenuService.GetManager(dependObject);
                        if (manager != null)
                        {
                            if (!manager.ContextMenu.IsOpen)
                            {
                                manager.ContextMenu.OpenInternal();
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
        }


        #endregion // EventHandlers
    }
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved