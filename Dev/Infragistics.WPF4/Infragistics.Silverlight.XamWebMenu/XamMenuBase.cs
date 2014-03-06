using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Controls.Menus.Primitives;

using System.Windows.Controls.Primitives;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// Represents a control that defines choices for users to select.
    /// </summary>
    /// <remarks>
    /// The MenuBase is the base class for controls that define choices for users to select. 
    /// The <see cref="XamMenu"/> and <see cref="XamContextMenu"/> inherit from MenuBase.
    /// </remarks>
	public abstract class XamMenuBase : XamHeaderedItemsControl
    {
        #region Member variables
        internal Control _previouslyFocusedControl;
        private delegate void EventDelegate(ItemClickedEventArgs e);
        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamMenuBase"/> class.
        /// </summary>
        protected XamMenuBase()
        {

            




            this.AddHandler(Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(this.MouseDownOutside), true);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

            this.Unloaded += new RoutedEventHandler(XamWebMenuBase_Unloaded);
        }      

        #endregion //Constructor

        #region Base class Overrides

        #region  GetContainerForItemOverride
        /// <summary>
        /// Overrides the framework invocation when a new container is needed.
        /// </summary>
        /// <returns>A new <see cref="XamMenuItem"/>.</returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            XamMenuItem item = null;
            if (this.DefaultItemsContainer != null)
            {
                item = this.DefaultItemsContainer.LoadContent() as XamMenuItem;
            }

            if (item == null)
                return new XamMenuItem();
            else
                return item;
        }
        #endregion

        #region IsItemItsOwnContainerOverride
        /// <summary>
        /// Overrides the framework invocation to test if an item is already a container.
        /// </summary>
        /// <param name="item">The object to test if it is a container.</param>
        /// <returns>Returns true if the item is already a <see cref="XamMenuItem"/>.</returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is XamMenuItem) || (item is XamMenuSeparator);
        }
        #endregion

        #region PrepareContainerForItemOverride
        /// <summary>
        /// Prepares the specified container to display the specified item.
        /// </summary>
        /// <param name="element">
        /// Container element used to display the specified item.
        /// </param>
        /// <param name="item">Specified item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            if (element is XamMenuSeparator)
            {
                return;
            }
            
            XamMenuItem node = element as XamMenuItem;
            if (node != null)
            {
                // Associate the Parent ItemsControl
                node.ParentItemsControl = this;
            }

            base.PrepareContainerForItemOverride(element, item);
        }
        #endregion

        #region ClearContainerForItemOverride
        /// <summary>
        /// Undoes the effects of PrepareContainerForItemOverride.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The contained item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            // Remove the association with the Parent ItemsControl
            XamMenuItem node = element as XamMenuItem;
            if (node != null)
            {
                node.ParentItemsControl = null;
            }

            base.ClearContainerForItemOverride(element, item);
        }
        #endregion


        /// <summary>
        /// Raised before the MouseDown event is fired.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            // Check to see if we're over a menu item. 
            // If we are, do nothing. 
            // Otherwise, we shoud close the currently opened item. 
            DependencyObject elem  = e.OriginalSource as DependencyObject;
            while (elem != null)
            {
                if (elem is XamMenuItem || elem == this)
                    return;
                elem = GetParent(elem);
            }

            this.CloseCurrentOpen();
             
        }



        #endregion //Base class Overrides

        #region Properties

        #region Internal properties

        #region Internal XamMenuItem CurrentOpen

        /// <summary>
        /// Identifies the <see cref="CurrentOpen"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty CurrentOpenProperty = DependencyProperty.Register("CurrentOpen", typeof(XamMenuItem), typeof(XamMenuBase), new PropertyMetadata(new PropertyChangedCallback(CurrentOpenChanged)));

        internal XamMenuItem CurrentOpen
        {
            get { return (XamMenuItem)this.GetValue(CurrentOpenProperty); }
            set { this.SetValue(CurrentOpenProperty, value); }
        }

        private static void CurrentOpenChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuBase menu = obj as XamMenuBase;
            XamMenuItem oldItem = e.OldValue as XamMenuItem;
            XamMenuItem newItem = e.NewValue as XamMenuItem;

            if (oldItem != null && oldItem.IsSubmenuOpen)
            {
                oldItem.ManipulateSubmenu(false, false);
                

                oldItem.IsHighlighted = false;
                if (newItem != null)
                {
                    newItem.ManipulateSubmenu(true, false);
                }
            }

            if (oldItem == null && newItem != null)
            {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



                Mouse.Capture(menu, CaptureMode.SubTree);

            }
            else if (oldItem != null && newItem == null)
            {


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)



                Mouse.Capture(null);

            }

            menu.OnCurrentOpenItemChanged(oldItem, newItem);
        }

        /// <summary>
        /// Called when the current open menu item has changed.
        /// </summary>
        /// <param name="oldItem">the old item</param>
        /// <param name="newItem">the new item</param>
        protected internal virtual void OnCurrentOpenItemChanged(XamMenuItem oldItem, XamMenuItem newItem)
        {
        }

        #endregion // CurrentOpen

        #region internal XamMenuItem CurrentSelected

        /// <summary>
        /// Identifies the <see cref="CurrentSelected"/> dependency property. 
        /// </summary>
        internal static readonly DependencyProperty CurrentSelectedProperty =
            DependencyProperty.Register("CurrentSelected", typeof(XamMenuItem), typeof(XamMenuBase), new PropertyMetadata(new PropertyChangedCallback(CurrentSelectedChanged)));

        internal XamMenuItem CurrentSelected
        {
            get { return (XamMenuItem)this.GetValue(CurrentSelectedProperty); }
            set { this.SetValue(CurrentSelectedProperty, value); }
        }

        private static void CurrentSelectedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamMenuItem oldItem = e.OldValue as XamMenuItem;

            if (oldItem != null)
                oldItem.IsSelected = false;
        }

        #endregion // CurrentSelected

        #endregion

        #endregion //Properties

        #region Methods

        #region Internal methods
        internal static DependencyObject GetRoot(DependencyObject element)
        {
            DependencyObject parent;
            while (element != null)
            {
                parent = GetParent(element);
                if (parent == null)
                    break;

                element = parent;
            }

            return element;
        }

        internal void RaiseItemClickedEvent(XamMenuItem item)
        {
            ItemClickedEventArgs args = new ItemClickedEventArgs();
            args.Item = item;
            this.OnItemClicked(args);
        }

        internal static ItemContainerGenerator GetGenerator(ItemsControl control)
        {
            XamMenuItem item = control as XamMenuItem;
            if (item != null)
            {
                return item.ItemContainerGenerator;
            }
            else
            {
                XamMenuBase menu = control as XamMenuBase;
                if (menu != null)
                {
                    return menu.ItemContainerGenerator;
                }
            }
            return null;
        }

        internal virtual void CloseCurrentOpen()
        {
            if (this.CurrentOpen != null)
            {
                this.CurrentOpen.ManipulateSubmenu(false, true); // CloseSubmenu(true);
                //Reverting the Previous fix, caused a regression issue.
                //This isn't necessarily the wrong fix, but we need to solve the regression issue.
                //this.CurrentOpen.IsSubmenuOpen = false;  
                this.CurrentOpen = null;
                if (this.CurrentSelected != null)
                {
                    // TFS - 17551 set the item back to normal
                    this.CurrentSelected.IsSelected = false;
                    this.CurrentSelected.ChangeVisualState(false);
                    this.CurrentSelected = null;
                }
            }
        }

        internal void ClearPreviouslyFocusedControl()
        {
            this._previouslyFocusedControl = null;
        }

        internal void ReturnFocus()
        {
            //return focus back to the previously focused control
            if (this._previouslyFocusedControl != null &&
                this._previouslyFocusedControl.IsEnabled &&
                this._previouslyFocusedControl.Visibility == Visibility.Visible)
            {
                this._previouslyFocusedControl.Focus();
                this.ClearPreviouslyFocusedControl();
            }
        }

        internal void RootVisual_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            DependencyObject dpObject = e.OriginalSource as DependencyObject;
            if (dpObject != null)
            {
                if (this.IsAncestorOf(dpObject))
                    return;
                
                UIElement visualParent = PlatformProxy.GetRootVisual(dpObject);
           
                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                if (visualParent != null)
                {
                    Popup p = PlatformProxy.GetParent(visualParent) as Popup;
                    if (p != null && this.IsAncestorOf(p))
                        return;
                }                
            }

            this.CloseCurrentOpen();
            this.ClearPreviouslyFocusedControl();
        }

        #endregion

        #region Protected methods
        #region OnItemClicked
        /// <summary>
        /// Raises a <see cref="ItemClicked"/> event when the user click with left mouse button on any menu item.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected virtual void OnItemClicked(ItemClickedEventArgs e)
        {
            if (this.ItemClicked != null)
            {
                this.ItemClicked(this, e);
            }
        }
        #endregion

        #region OnBeforeUnload
        /// <summary>
        /// Called before unload the menu from the browser.
        /// </summary>
        protected virtual void OnBeforeUnload()
        {
        }
        #endregion //OnBeforeUnload	  
        #endregion

        #region Private



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        internal static bool IsObjectFromMenu(DependencyObject root, object originalSource)
        {
            DependencyObject focused = originalSource as DependencyObject;
            while (focused != null)
            {
                if (object.ReferenceEquals(focused, root))
                {
                    return true;
                }

                // This helps deal with popups that may not be in the same 
                // visual tree
                DependencyObject parent = GetParent(focused);
                focused = parent;
            }

            return false;
        }

        internal static DependencyObject GetParent(DependencyObject element)
        {
            DependencyObject parent = null;

            // tfs 44387. the GetParent method of VisualTreeHelper only accepts 
            // classes of type Visual or Visual3d.
            if (element is Visual || element is System.Windows.Media.Media3D.Visual3D)

            {
                parent = VisualTreeHelper.GetParent(element);
            }

            if (parent == null)
            {
                // Try the logical parent.
                FrameworkElement frameworkElement = element as FrameworkElement;
                if (frameworkElement != null)
                {
                    parent = frameworkElement.Parent;
                }

                // tfs 44387. 
                FrameworkContentElement contentElement = element as FrameworkContentElement;
                if (contentElement != null)
                {
                    parent = contentElement.Parent;
                }

            }

            return parent;
        }


        private void MouseDownOutside(object sender, MouseButtonEventArgs e)
        {
            

            this.CloseCurrentOpen();
            this._previouslyFocusedControl = null;
        }


        #endregion

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion //Methods

        #region Events

        /// <summary>
        /// Occurs when a <see cref="XamMenuItem"/> is clicked.
        /// </summary>
        public event EventHandler<ItemClickedEventArgs> ItemClicked;
        #endregion

        #region EventHandlers

        void XamWebMenuBase_Unloaded(object sender, RoutedEventArgs e)
        {
            this.OnBeforeUnload();
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