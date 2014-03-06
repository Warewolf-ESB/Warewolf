using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls.Primitives;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.Windows.Ribbon.Events;
using Infragistics.Windows.Ribbon.Internal;
using System.Windows.Input;
using System.Collections;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using System.Collections.Specialized;
using System.Windows.Threading;
using Infragistics.Shared;
using Infragistics.Windows.Automation.Peers.Ribbon;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.Ribbon
{

	/// <summary>
	/// Absract base class for all menu tools
	/// </summary>
    /// <remarks>
    /// <para class="body">Tool items, elements that implement the <see cref="IRibbonTool"/> interface, that are added as children of a menu tool will be displayed inside the menu's <see cref="Popup"/> when its <see cref="IsOpen"/> property is True.</para>
    /// </remarks>
    /// <seealso cref="MenuTool"/>
    /// <seealso cref="ApplicationMenu"/>
    /// <seealso cref="Opening"/>
    /// <seealso cref="Opened"/>
    /// <seealso cref="Closed"/>

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateActive,              GroupName = VisualStateUtilities.GroupActive)]
    [TemplateVisualState(Name = VisualStateUtilities.StateInactive,            GroupName = VisualStateUtilities.GroupActive)]

    [TemplateVisualState(Name = VisualStateUtilities.StateMenu,                GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateRibbon,              GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateQAT,                 GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenu,             GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuFooterToolbar,GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuRecentItems,  GroupName = VisualStateUtilities.GroupLocation)]
    [TemplateVisualState(Name = VisualStateUtilities.StateAppMenuSubMenu,      GroupName = VisualStateUtilities.GroupLocation)]

	[TemplatePart(Name = "PART_MenuToolPresenterSite", Type = typeof(ContentPresenter))]
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public abstract class MenuToolBase : ItemsControl, 
		IRibbonTool, 
		IRibbonToolLocation,
		IKeyTipContainer,
		IRibbonPopupOwner
	{
		#region Private Members

		private ContentPresenter					_presenterSite;
		private ToolMenuItem						_presenter;
		private MenuItem							_selectedMenuItem;
		private bool								_bypassRaisingNextOpeningEvent;
		// JJD 10/10/07 - BR26870
		// The Opening event is no longer cancelable
		//private bool _openingEventCancelled;
		private MenuToolMenu						_menu;
		private ArrayList							_reparentedTools;

		// JJD 10/16/07
		// Keep track of the instance of the menutoool that currently owns the reparented tools.
		// This member is only maintained on the root MenuToolBase. Use the GetReparentedToolsOwner
		// and SetReparentedToolsOwner methods instead of using this member
		private ToolMenuItem						_currentReparentedToolsOwner;

		// used temporarily during PrepareContainerForItemInternal and ClearContainerForItemInternal
		private ToolMenuItem						_parentMenuItemBeingProcessed;

		// AS 10/10/07 PopupOwnerProxy
		private PopupOwnerProxy						_popupOwnerProxy;

		// AS 12/19/07 BR29199
		private PopupOpeningReason					_openingReason;

		// JM 10-15-08 [BR35158 - TFS6448]
		private static bool?						_itemsControlHasItemStringFormatProperty;
		private static DependencyProperty			_itemsControlItemStringFormatProperty;


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Private Members

		#region Constructors

        /// <summary>
        /// Initializes a new instance of a<see cref="MenuToolBase"/> derived class.
        /// </summary>
        protected MenuToolBase()
		{
		}

		static MenuToolBase()
		{
			FrameworkElement.FocusVisualStyleProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(new Style()));

			XamRibbon.IsActivePropertyKey.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsActiveChanged)));

			// AS 10/9/07
			// Put focus in the menu tool presenter instead of the menu tool.
			//
			FrameworkElement.FocusableProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			ToolTipService.ShowOnDisabledProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

            // AS 2/6/09 TFS13621
            // Since we are (as of the fix for TFS6448) using the ItemsPanel of the MenuTool 
            // on the ToolMenuItem, we need to initialize the ItemsPanel of the MenuTool to 
            // the same panel that the ToolMenuItem would have used.
            //
            ItemsPanelTemplate template = new ItemsPanelTemplate(new FrameworkElementFactory(typeof(MenuToolPanel)));
            template.Seal();
            ItemsControl.ItemsPanelProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(template));

			// AS 10/16/09 TFS23117
			// Moved here from the MenuTool class.
			//
			// JM 02-03-09 TFS9245
			ToolTipService.IsEnabledProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceToolTipServiceIsEnabled)));


            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            XamRibbon.LocationProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)), XamRibbon.LocationPropertyKey);
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(MenuToolBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		#endregion //Constructors

		#region Base class overrides

			#region ClearContainerForItemOverride

		/// <summary>
		/// Undoes the effects of PrepareContainerForItemOverride.
		/// </summary>
		/// <param name="element">The container element</param>
		/// <param name="item">The item.</param>
		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			Debug.Assert(this._parentMenuItemBeingProcessed != null);

			// AS 5/4/10 TFS30711
			// Moved down so the proxy can get a chance to perform its clean up first.
			//
			//if (this._parentMenuItemBeingProcessed != null)
			//    this._parentMenuItemBeingProcessed.ClearContainerForItemBase(element, item);
			//else
			//    base.ClearContainerForItemOverride(element, item);

			ToolMenuItem menuItem = element as ToolMenuItem;
			FrameworkElement feItem = item as FrameworkElement;

			if (menuItem != null && feItem != null)
			{
				IRibbonTool tool = item as IRibbonTool;

				if (tool != null)
				{
					RibbonToolProxy proxy = tool.ToolProxy;

					if (proxy == null)
						throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));

					proxy.ClearToolMenuItem(menuItem, feItem);
				}
			}

			// AS 5/4/10 TFS30711
			// In WPF3, the ItemsControl's implementation of ClearContainerForItemOverride was 
			// empty. In WPF4, it now cleans up what it was doing in the PrepareContainerForItemOverride 
			// which depending on the type of element (contentcontrol, headereditemscontrol, etc.) would 
			// do different things. So for HeaderItemsControl it now sets the header to DisconnectedItem
			// during the clear so we want to give the proxy a chance to handle this first so I moved 
			// this down from above.
			//
			if (this._parentMenuItemBeingProcessed != null)
				this._parentMenuItemBeingProcessed.ClearContainerForItemBase(element, item);
			else
				base.ClearContainerForItemOverride(element, item);
		}

		internal void ClearContainerForItemInternal(ToolMenuItem parentMenuItem, DependencyObject element, object item)
		{
			this._parentMenuItemBeingProcessed = parentMenuItem;

			// AS 5/4/10 TFS30711
			// We were only calling the base and not our override of that method so the ClearToolMenuItem of the 
			// proxy associated with the tool was never being called.
			//
			//base.ClearContainerForItemOverride(element, item);
			this.ClearContainerForItemOverride(element, item);
			
			FrameworkElement feItem = item as FrameworkElement;

			if (feItem != null)
			{
				// clear the ToolMenuItem attached property
				feItem.ClearValue(ToolMenuItemProperty);

				MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

				// AS 6/9/09 TFS17541 [Start]
				// The ItemsSource of the ApplicationMenuPresenter is the private 
				// ApplicationMenuItems class which is a combination of the Items 
				// and RecentItems. Whenever either collection is modified, a Reset 
				// notification is sent. When the reset is received by the ItemsControl
				// - the ApplicationMenuPresenter in this case - it releases the old 
				// wrappers (i.e. the ToolMenuItems it created for the items) and 
				// eventually generates new ones. Well if those items are MenuTool 
				// instances and the associated ToolMenuItem wrapper that is associated 
				// with that tool is the _currentReparentedToolsOwner for that tool's 
				// Items then we need to put the items back into the logical tree 
				// of the menu tool. Otherwise the toolmenuitem is the root ancestor 
				// of those items and it appears as though the tools are no longer in 
				// the logical or visual tree of the xamribbon and therefore any 
				// tools added to the qat for those items are removed because the tool 
				// is unregistered. To get around this behavior, we'll use the clear 
				// of the container (since we used the prepare to reparent the items 
				// under the wrapper).
				//
				if (null != rootMenu)
				{
					DependencyObject toolParent = feItem.Parent;

					if (toolParent is ToolMenuItem.LogicalContainer)
						toolParent = LogicalTreeHelper.GetParent(toolParent);

					// if the item is logical parented to the parent menu item and 
					// the parent menu item is still the current tool owner then 
					// we need to reparent the tools back into the menu
					if (toolParent == parentMenuItem && 
						null != rootMenu._reparentedTools && 
						parentMenuItem == rootMenu._currentReparentedToolsOwner)
					{
						
#region Infragistics Source Cleanup (Region)






























#endregion // Infragistics Source Cleanup (Region)

						XamRibbon ribbon = XamRibbon.GetRibbon(rootMenu);

						Debug.Assert(null != ribbon);

						if (null != ribbon)
						{
							ribbon.VerifyMenuToolChildrenAsync(rootMenu);
						}
					}
				}

				ToolMenuItem tmi = element as ToolMenuItem;

				// if the item being released is a menu tool and its 
				// ItemsSource is still set then we need to release the 
				// items source so clearcontainer can be called on its 
				// children so they can be reparented back into the menu 
				// tool
				MenuToolBase childMenu = feItem as MenuToolBase;
				if (tmi.ItemsSource != null && childMenu != null)
				{
					tmi.ItemsSource = null;
				}
				// AS 6/9/09 TFS17541 [End]
			}

			this._parentMenuItemBeingProcessed = null;
		}

			#endregion //ClearContainerForItemOverride	
    
			#region GetContainerForItemOverride

		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>The newly created container</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new ToolMenuItem();
		}

		internal DependencyObject GetContainerForItemInternal(ToolMenuItem parentMenuItem)
		{
			this._parentMenuItemBeingProcessed = parentMenuItem;

			DependencyObject container = this.GetContainerForItemOverride();

			this._parentMenuItemBeingProcessed = null;

			return container;
		}

			#endregion //GetContainerForItemOverride	
    
			#region IsItemItsOwnContainerOverride

		/// <summary>
		/// Determines if the item requires a separate container.
		/// </summary>
		/// <param name="item">The item in question.</param>
		/// <returns>True if the item does not require a wrapper</returns>
		protected override bool IsItemItsOwnContainerOverride(object item)
		{
			return item is ToolMenuItem;
		}

		internal bool IsItemItsOwnContainerInternal(object item)
		{
			return this.IsItemItsOwnContainerOverride(item);
		}

			#endregion //IsItemItsOwnContainerOverride	

			#region LogicalChildren

		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override IEnumerator LogicalChildren
		{
			get
			{
				
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

				return new MultiSourceEnumerator(
					new FilteringEnumerator(base.LogicalChildren, this._reparentedTools), 
					new SingleItemEnumerator(this._menu)
					);
			}
		}

			#endregion //LogicalChildren	

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			VerifyTypeOfChildren(this);

			ContentPresenter menuSite = this.GetTemplateChild("PART_MenuToolPresenterSite") as ContentPresenter;

			if (this._presenterSite != menuSite)
			{
				if (this._presenterSite != null)
					this._presenterSite.ClearValue(ContentPresenter.ContentProperty);

				this._presenterSite = menuSite;

                
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

                if (this._menu != null)
                {
                    Menu menu = _menu;
                    this._menu = null;
                    this.RemoveLogicalChild(menu);
                    menu.Items.Clear();
                }

                if (this._presenterSite != null)
				{
					// Create a Host menu to contain a top level menu item
					//MenuToolHost host = new MenuToolHost();
					if (this._menu == null)
					{
						this._menu = new MenuToolMenu(this);

						this.AddLogicalChild(this._menu);
					}
					else
						this._menu.Items.Clear();

					this._menu.Background = Brushes.Transparent;

					this.EnsurePresenterIsInitialized();

					// add the top level menu item to the host menu
					this._menu.Items.Add(this._presenter);

					this._presenterSite.Content = this._menu;
				}
			}

			// AS 10/11/07
			// Only use the proxy for root level menus.
			//
			if (this._popupOwnerProxy == null && this._presenter != null)
				this._popupOwnerProxy = new PopupOwnerProxy(this);

			if (this._popupOwnerProxy != null)
			{
				if (this._presenter != null && this._menu != null)
					this._popupOwnerProxy.Initialize(this);
				else
					this._popupOwnerProxy.Initialize(null);
			}


            // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);


		}

			#endregion //OnApplyTemplate	

			#region OnGotKeyboardFocus

		/// <summary>
		/// Invoked when an unhandled System.Windows.Input.Keyboard.GotKeyboardFocus�attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
		/// </summary>
		/// <param name="e">The System.Windows.Input.KeyboardFocusChangedEventArgs that contains the event data.</param>
		protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			base.OnGotKeyboardFocus(e);
		}

			#endregion //OnGotKeyboardFocus
    
			#region OnIsKeyboardFocusWithinChanged
		/// <summary>
		/// Invoked when the value of the <see cref="UIElement.IsKeyboardFocusWithin"/> property changes.
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			// AS 10/10/07
			// Ok now I know why it was making a difference. The XamRibbon was keying off the GotFocus
			// event and not the GotKeyboardFocus. Since the MenuPresenter was still the FocusedElement
			// of its focusscope - the MenuToolMenu in this case - it would not get a GotFocus and therefore
			// the ribbon would not make it an active item. Now we're listening to GotKeyboardFocus so
			// we don't need to do this.
			//
			// AS 10/9/07
			// I'm not sure exactly why this is happening but if you navigate into a menu tool, which 
			// in turn focuses the MenuToolPresenter, then leave, you cannot arrow back into it. it 
			// seems to be related to the fact that the MenuToolMenu is a focusscope because if we 
			// clear out its focused element, it will accept focus.
			//
			//if (false.Equals(e.NewValue) && this._menu != null)
			//	FocusManager.SetFocusedElement(this._menu, null);

			// AS 10/10/07
			// If, however, focus is moved within us then make sure to clear the focused element
			// of our focus scope if the focused element is within our menu. Without this, the 
			// FocusedElement of the Ribbon (our focusscope) remains the last element with focus
			// so when it gets focus again, it does not get the gotfocus event. This was especially
			// important so that an editor tool would lose focus and come out of edit mode.
			//
			if (this._menu != null && true.Equals(e.NewValue) && 
				this.IsKeyboardFocused == false && this.IsKeyboardFocusWithin)
			{
				DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;

				// AS 11/30/07
				// IsAncestorOf will not account for crossing over popups, etc. or other breaks in the visual tree
				//
				//if (focusedElement != null && this._menu.IsAncestorOf(focusedElement))
				if (focusedElement != null && Utilities.IsDescendantOf(this, focusedElement))
				{
					DependencyObject focusScope = FocusManager.GetFocusScope(this);

					if (focusScope != null)
						FocusManager.SetFocusedElement(focusScope, null);
				}
			}

			base.OnIsKeyboardFocusWithinChanged(e);
		}
			#endregion //OnIsKeyboardFocusWithinChanged

			#region OnItemsChanged

		/// <summary>
		/// Called when one or more items have changed
		/// </summary>
		/// <param name="e">Describes the items that changed.</param>
		protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			if (this.IsInitialized)
			{
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
					case NotifyCollectionChangedAction.Replace:
					case NotifyCollectionChangedAction.Reset:
						this.VerifyReparentedItems();
						break;
				}
				switch (e.Action)
				{
					case NotifyCollectionChangedAction.Reset:
						VerifyTypeOfChildren(this);
						break;
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Replace:
						for (int i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++)
							VerifyTypeOfChild(this.Items[i]);
						break;
				}
			}
		}

			#endregion //OnItemsChanged	

			#region OnKeyDown

		/// <summary>
		/// Called when the element has input focus and a key is pressed.
		/// </summary>
		/// <param name="e">An instance of KeyEventArgs that contains information about the key that was pressed.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


			base.OnKeyDown(e);

			// AS 10/10/07 PopupOwnerProxy
			if (this._popupOwnerProxy != null)
				this._popupOwnerProxy.ProcessKeyDown(e);

			//if (e.Handled)
			//    return;

			//switch (e.Key)
			//{
			//    case Key.Up:
			//    case Key.Down:

			//        if (this._selectedMenuItem == null && this._presenter != null)
			//        {
			//            ItemCollection items = this._presenter.Items;
			//            int count = items.Count;

			//            for (int i = 0; i < count; i++)
			//            {
			//                DependencyObject item = items[i] as DependencyObject;

			//                if (item != null)
			//                {
			//                    MenuItem menuItem = ItemsControl.ContainerFromElement(this._presenter, item) as MenuItem;

			//                    if (menuItem != null &&
			//                         menuItem.IsEnabled)
			//                        menuItem.SetValue(Selector.IsSelectedProperty, KnownBoxes.TrueBox);
			//                }
			//            }
			//        }
			//        return;
			//}

		}

			#endregion //OnKeyDown	
    
			#region OnMouseEnter

		/// <summary>
		/// Raised when the mouse enters the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);


            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

			#endregion //OnMouseEnter	
    
			#region OnMouseLeave

		/// <summary>
		/// Raised when the mouse leaves the element.
		/// </summary>
		/// <param name="e">EventArgs containing the event information.</param>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);


            // JJD 4/23/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

		}

			#endregion //OnMouseLeave	

			#region OnPropertyChanged

		/// <summary>
		/// Called when a property has changed
		/// </summary>
		/// <param name="e">The event args with the property being changed and the new and old values.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (this._presenter != null )
			{
				if ( e.Property == XamRibbon.IsActiveProperty)
				{
					this._presenter.SetValue(XamRibbon.IsActivePropertyKey, e.NewValue);
				}
				else
				if ( e.Property == XamRibbon.LocationProperty)
				{
					this._presenter.SetValue(XamRibbon.LocationPropertyKey, e.NewValue);
				}
			}

		}

			#endregion //OnPropertyChanged	
        
			#region PrepareContainerForItemOverride

		/// <summary>
		/// Prepares the container to 'host' the item.
		/// </summary>
		/// <param name="element">The container that wraps the item.</param>
		/// <param name="item">The data item that is wrapped.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// AS 6/9/08 BR32242
			ToolMenuItem container = element as ToolMenuItem;

			if (null != container)
				container.SetValue(ToolMenuItem.ToolPropertyKey, item as IRibbonTool);

			Debug.Assert(this._parentMenuItemBeingProcessed != null);

			if (this._parentMenuItemBeingProcessed != null)
				this._parentMenuItemBeingProcessed.PrepareContainerForItemBase(element, item);
			else
				base.PrepareContainerForItemOverride(element, item);

			ToolMenuItem		menuItem	= element as ToolMenuItem;
			FrameworkElement	feItem		= item as FrameworkElement;

			this.SetLocation(element, item);

			if (menuItem != null && feItem != null)
			{
                // JJD 11/08/07
                // Bind the visibility of the menuitem to the item's visibility
                if ( feItem != menuItem )
                    menuItem.SetBinding(VisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.OneWay, feItem));

				IRibbonTool tool = item as IRibbonTool;
	
				if (tool != null)
				{
					RibbonToolProxy proxy = tool.ToolProxy;

					if (proxy == null)
						throw new InvalidOperationException(XamRibbon.GetString("LE_IRibbonToolProxyIsNull"));

					// JJD 10/25/07
					// If the tool is not activatable set the menu item's focusable to false
					if (!proxy.IsActivateable(feItem))
						menuItem.Focusable = false;

					proxy.PrepareToolMenuItem(menuItem, feItem);

					// If we are not embedding the tool in the caption area, hookup the MenuItemDescription attached property on the tool
					// to the same attached property on the menuitem.
					if (tool is FrameworkElement  &&
						proxy.GetMenuItemDisplayMode((FrameworkElement)tool) != RibbonToolProxy.ToolMenuItemDisplayMode.EmbedToolInCaptionArea)
						menuItem.SetBinding(MenuToolBase.MenuItemDescriptionProperty, Utilities.CreateBindingObject(MenuToolBase.MenuItemDescriptionProperty, BindingMode.OneWay, tool));
				}
			}

			Panel panel = item as Panel;

			if (panel != null)
				KeyboardNavigation.SetDirectionalNavigation(panel, KeyboardNavigationMode.Contained);
		}

		internal void PrepareContainerForItemInternal(ToolMenuItem parentMenuItem, DependencyObject element, object item)
		{
			this._parentMenuItemBeingProcessed = parentMenuItem;

			FrameworkElement feItem = item as FrameworkElement;

			if ( feItem != null )
			{
				// set the ToolMenuItem attached property
				if ( element is ToolMenuItem )
					feItem.SetValue(ToolMenuItemProperty, element);

				DependencyObject toolParent = feItem.Parent;

				// When we are embedding the tool in the caption area we need to logically reparent the
				// tool into the internal menu we created. This is because the menu is a focus scope
				// and the embedded tool needs to be within it. Otherwise there are issues with commands
				// not being haandled by the tool causing stack overflow exceptions as the framework
				// tries to percolate the command up the logicasl chain.
				if (toolParent != parentMenuItem && parentMenuItem != null)
				{
					//Debug.Assert(toolParent == this || toolParent == null);

                    // JM/JD 10-18-07
					//if (toolParent == this || toolParent == null)
                    if (toolParent is MenuToolBase || toolParent == null)
                    {
                        // JM/JD 10-18-07
                        MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

						XamRibbon ribbon = XamRibbon.GetRibbon(this);

                        // AS 2/19/09 TFS6747
						//Debug.Assert(ribbon != null);
						Debug.Assert(ribbon != null || Utilities.GetAncestorFromType(this, typeof(XamRibbon), true) == null);

						// temporarily suspend tool registration so the tool avoids being 
						// unregistered simply to be re-registered on the next line when
						// it is re-parented below
						if (ribbon != null)
							ribbon.SuspendToolRegistration();

						try
						{
							// AS 10/8/09 TFS23328
							// Store the datacontext locally while we reparent the tool.
							//
							using (new TempValueReplacement(feItem, FrameworkElement.DataContextProperty))
							{
								// remove the tool as the logical child of us
								// JM/JD 10-18-07
								//if (toolParent == this)
								//    this.RemoveLogicalChild(feItem);
								if (toolParent != null)
									((MenuToolBase)toolParent).RemoveLogicalChild(feItem);

								// add the tool as the logical child of the menu
								parentMenuItem.AddLogicalChildInternal(feItem);
							}

							// if not aleady allocated, allocate an arraylist to keep
							// track of the reparented tools
                            // JM/JD 10-18-07
                            //if (this._reparentedTools == null)
                            //    this._reparentedTools = new ArrayList(5);

                            //this._reparentedTools.Add(feItem);
                            if (rootMenu._reparentedTools == null)
                                rootMenu._reparentedTools = new ArrayList(5);

                            rootMenu._reparentedTools.Add(feItem);
                        }
						finally
						{
							// resume the registration of tools
							if (ribbon != null)
								ribbon.ResumeToolRegistration();
						}
					}
				}
			}

			this.PrepareContainerForItemOverride(element, item);

			this._parentMenuItemBeingProcessed = null;
		}

			#endregion //PrepareContainerForItemOverride	
    
		#endregion //Base class overrides

		#region Events

			#region Common Tool Events

				#region Activated

		/// <summary>
		/// Event ID for the <see cref="Activated"/> routed event
		/// </summary>
		/// <seealso cref="Activated"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent ActivatedEvent =
			EventManager.RegisterRoutedEvent("Activated", RoutingStrategy.Bubble, typeof(EventHandler<ItemActivatedEventArgs>), typeof(MenuToolBase));

		/// <summary>
		/// Occurs after a tool has been activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="ActivatedEvent"/>
		/// <seealso cref="ItemActivatedEventArgs"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		//[Description("Occurs after a tool has been activated")]
		//[Category("Ribbon Properties")]
		internal event EventHandler<ItemActivatedEventArgs> Activated
		{
			add
			{
				base.AddHandler(MenuToolBase.ActivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuToolBase.ActivatedEvent, value);
			}
		}

				#endregion //Activated

				#region Cloned

		/// <summary>
		/// Event ID for the <see cref="Cloned"/> routed event
		/// </summary>
		/// <seealso cref="Cloned"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent ClonedEvent =
			EventManager.RegisterRoutedEvent("Cloned", RoutingStrategy.Bubble, typeof(EventHandler<ToolClonedEventArgs>), typeof(MenuToolBase));

		/// <summary>
		/// Occurs after a tool has been cloned.
		/// </summary>
		/// <remarks>
		/// <para class="body">Fired when a tool is cloned to enable its placement in an additional location in the XamRibbon.  For example, when a tool is added to the QuickAccessToolbar, the XamRibbon clones the instance of the tool that appears on the Ribbon and places the cloned instance on the QAT.  This event is fired after the cloning takes place and is a convenient point hook up event listeners for events on the cloned tool.</para>
		/// </remarks>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="ClonedEvent"/>
		/// <seealso cref="ToolClonedEventArgs"/>
		//[Description("Occurs after a tool has been cloned")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolClonedEventArgs> Cloned
		{
			add
			{
				base.AddHandler(MenuToolBase.ClonedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuToolBase.ClonedEvent, value);
			}
		}

				#endregion //Cloned

				#region CloneDiscarded

		/// <summary>
		/// Event ID for the <see cref="CloneDiscarded"/> routed event
		/// </summary>
		/// <seealso cref="CloneDiscarded"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		public static readonly RoutedEvent CloneDiscardedEvent =
			EventManager.RegisterRoutedEvent("CloneDiscarded", RoutingStrategy.Bubble, typeof(EventHandler<ToolCloneDiscardedEventArgs>), typeof(MenuToolBase));

		/// <summary>
		/// Occurs when a clone of a tool is being discarded.
		/// </summary>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="CloneDiscardedEvent"/>
		/// <seealso cref="ToolCloneDiscardedEventArgs"/>
		//[Description("Occurs when a clone of a tool is being discarded.")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolCloneDiscardedEventArgs> CloneDiscarded
		{
			add
			{
				base.AddHandler(MenuToolBase.CloneDiscardedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuToolBase.CloneDiscardedEvent, value);
			}
		}

				#endregion //CloneDiscarded

				#region Deactivated

		/// <summary>
		/// Event ID for the <see cref="Deactivated"/> routed event
		/// </summary>
		/// <seealso cref="Deactivated"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		internal static readonly RoutedEvent DeactivatedEvent =
			EventManager.RegisterRoutedEvent("Deactivated", RoutingStrategy.Bubble, typeof(EventHandler<ItemDeactivatedEventArgs>), typeof(MenuToolBase));

		/// <summary>
		/// Occurs after a tool has been de-activated
		/// </summary>
		/// <seealso cref="XamRibbon.ActiveItem"/>
		/// <seealso cref="XamRibbon.IsActiveProperty"/>
		/// <seealso cref="XamRibbon.GetIsActive(DependencyObject)"/>
		/// <seealso cref="OnRaiseToolEvent"/>
		/// <seealso cref="DeactivatedEvent"/>
		/// <seealso cref="ItemDeactivatedEventArgs"/>
		// AS 10/31/07
		// Changed to internal for now since there is no defined use case and they are not consistently raised.
		//
		//[Description("Occurs after a tool has been de-activated")]
		//[Category("Ribbon Properties")]
		internal event EventHandler<ItemDeactivatedEventArgs> Deactivated
		{
			add
			{
				base.AddHandler(MenuToolBase.DeactivatedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuToolBase.DeactivatedEvent, value);
			}
		}

				#endregion //Deactivated

			#endregion //Common Tool Events

			#region Closed

		/// <summary>
		/// Event ID for the <see cref="Closed"/> routed event
		/// </summary>
		/// <seealso cref="Closed"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnClosed"/>
		public static readonly RoutedEvent ClosedEvent =
			EventManager.RegisterRoutedEvent("Closed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuToolBase));

		/// <summary>
		/// Occurs after the tool has been closed
		/// </summary>
		/// <seealso cref="Closed"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="ClosedEvent"/>
		protected virtual void OnClosed(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseClosed(RoutedEventArgs args)
		{
			args.RoutedEvent = MenuTool.ClosedEvent;
			args.Source = this;
			this.OnClosed(args);
		}

		/// <summary>
		/// Occurs after the tool has been closed
		/// </summary>
		/// <seealso cref="OnClosed"/>
		/// <seealso cref="ClosedEvent"/>
		//[Description("Occurs after this tool has been closed")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Closed
		{
			add
			{
				base.AddHandler(MenuTool.ClosedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.ClosedEvent, value);
			}
		}

			#endregion //Closed

			#region Opened

		/// <summary>
		/// Event ID for the <see cref="Opened"/> routed event
		/// </summary>
		/// <seealso cref="Opened"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnOpened"/>
		public static readonly RoutedEvent OpenedEvent =
			EventManager.RegisterRoutedEvent("Opened", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MenuToolBase));

		/// <summary>
		/// Occurs after the tool has been opened
		/// </summary>
		/// <seealso cref="Opened"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OpenedEvent"/>
		protected virtual void OnOpened(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseOpened(RoutedEventArgs args)
		{
			args.RoutedEvent = MenuTool.OpenedEvent;
			args.Source = this;
			this.OnOpened(args);
		}

		/// <summary>
		/// Occurs after the tool has been opened
		/// </summary>
		/// <seealso cref="OnOpened"/>
		/// <seealso cref="OpenedEvent"/>
		//[Description("Occurs after this tool has been opened")]
		//[Category("Ribbon Events")]
		public event RoutedEventHandler Opened
		{
			add
			{
				base.AddHandler(MenuTool.OpenedEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.OpenedEvent, value);
			}
		}

			#endregion //Opened

			#region Opening

		/// <summary>
		/// Event ID for the <see cref="Opening"/> routed event
		/// </summary>
		/// <seealso cref="Opening"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OnOpening"/>
		public static readonly RoutedEvent OpeningEvent = 
			EventManager.RegisterRoutedEvent("Opening", RoutingStrategy.Bubble, typeof(EventHandler<ToolOpeningEventArgs>), typeof(MenuToolBase));

		/// <summary>
		/// Occurs before the tool has been opened
		/// </summary>
		/// <seealso cref="Opening"/>
		/// <seealso cref="IsOpen"/>
		/// <seealso cref="OpeningEvent"/>
		protected virtual void OnOpening(ToolOpeningEventArgs args)
		{
			this.RaiseEvent(args);
		}

		private void RaiseOpening(ToolOpeningEventArgs args)
		{
			args.RoutedEvent = MenuTool.OpeningEvent;
			args.Source = this;

			if (PreOpening != null)
				PreOpening(this, args);

			// JJD 10/10/07 - BR26870
			// The Opening event is no longer cancelable
			//if (args.Cancel == false)
				this.OnOpening(args);
		}

		// JJD 10/10/07 - BR26870
		// The Opening event is no longer cancelable
		//internal bool RaiseOpeningEvent()
		internal void RaiseOpeningEvent()
		{
			// check the _bypassRaisingNextOpeningEvent flag to prevent
			// the Opening event from being raised twice
			if (this._bypassRaisingNextOpeningEvent == true ||
				this.IsOpen == true)
			{
				// JJD 10/10/07 - BR26870
				// The Opening event is no longer cancelable
				//bool rtn = this._openingEventCancelled == false;
				
				//this._bypassRaisingNextOpeningEvent = false;
				//this._openingEventCancelled = false;

				//return rtn;
				this._bypassRaisingNextOpeningEvent = false;
				
				return;
			}


            // JM BR27453 10-18-07
			//ToolOpeningEventArgs args = new ToolOpeningEventArgs(this);
            FrameworkElement        clonedFromTool  = RibbonToolProxy.GetRootSourceTool(this);
            ToolOpeningEventArgs    args            = new ToolOpeningEventArgs(clonedFromTool == null ? this : clonedFromTool);

			// Raise the opening event
			this.RaiseOpening(args);

			// set a flag so we know to bypass redundant attempts to raise this event
			// Note: the flag will get reset in the property change callback of the IsOpen property
			// or on the second attempt to call this methos before that point
			this._bypassRaisingNextOpeningEvent = true;


			// JJD 10/10/07 - BR26870
			// The Opening event is no longer cancelable
			//this._openingEventCancelled = args.Cancel;

			//// if not cancelled then return true;
			//return  args.Cancel == false;
		}

		/// <summary>
		/// Occurs before the tool has been opened
		/// </summary>
		/// <seealso cref="OnOpening"/>
		/// <seealso cref="OpeningEvent"/>
		//[Description("Occurs before this tool has been opened")]
		//[Category("Ribbon Events")]
		public event EventHandler<ToolOpeningEventArgs> Opening
		{
			add
			{
				base.AddHandler(MenuTool.OpeningEvent, value);
			}
			remove
			{
				base.RemoveHandler(MenuTool.OpeningEvent, value);
			}
		}

			#endregion //Opening

			#region PreOpening (internal CLR event)

		internal event EventHandler<ToolOpeningEventArgs> PreOpening;

			#endregion //PreOpening (internal CLR event)

		#endregion //Events

		#region Properties

			#region Public Properties

				#region IsOpen

		/// <summary>
		/// Identifies the <see cref="IsOpen"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
			typeof(bool), typeof(MenuToolBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsOpenChanged), new CoerceValueCallback(CoerceIsOpened)));

		private static object CoerceIsOpened(DependencyObject target, object value)
		{
			if (value is bool)
			{
				bool newValue = (bool)value;

				// we only need to raise the Opening event here if the IsOpened property is being set to true.
				// The after events will be raised in the OnIsOpenChanged callback
				if (newValue == true)
				{
					MenuToolBase menu = target as MenuToolBase;

					if (menu != null)
					{
						MenuToolPresenter mtp = menu.Presenter as MenuToolPresenter;

						// for MenuTool's prevent the menu from opening if the MenuButtonArea
						// is not enabled
						if (mtp != null && !mtp.IsMenuButtonAreaEnabled)
							return KnownBoxes.FalseBox;

						// JJD 10/10/07 - BR26870
						// The Opening event is no longer cancelable
						//if (menu.RaiseOpeningEvent() == false)
						//    return KnownBoxes.FalseBox;
						menu.RaiseOpeningEvent();
					}
				}
			}

			return value;
		}

		private static void OnIsOpenChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuToolBase menu = target as MenuToolBase;

			if (menu != null && e.NewValue is bool)
			{
				// AS 10/16/09 TFS23117
				// Moved here from the MenuTool class.
				//
				// JM 02-03-09 TFS9245 - Ensure that the MenuTool's Tooltip (if any) does not show when the mouse is over the MenuTool's popup (i.e.
				//						 when it's open)
				menu.CoerceValue(ToolTipService.IsEnabledProperty);

				bool val = (bool)(e.NewValue);

				RoutedEventArgs args = new RoutedEventArgs();

				// reset the Opening event flags
				menu._bypassRaisingNextOpeningEvent = false;
				
				// JJD 10/10/07 - BR26870
				// The Opening event is no longer cancelable
				//menu._openingEventCancelled = false;

				XamRibbon ribbon = XamRibbon.GetRibbon(menu);

				if (val == true)
				{
					// give the menu keyboard focus

					if (menu._presenter != null &&
						!menu._presenter.IsKeyboardFocusWithin)
					{
						menu._presenter.Focus();

						Debug.Assert(menu.IsOpen, "The IsOpen state has changed as a result of giving focus to the menu!");
					}

					// AS 12/19/07 BR29199
					KeyTipManager.NotifyPopupOpened(menu.OpeningReasonResolved, menu);

					// AS 10/10/07 PopupOwnerProxy
					if (menu._popupOwnerProxy != null)
						menu._popupOwnerProxy.OnOpen();

					// JJD 10/22/07
					// Enter navigation mode for ApplicatuonMenu
					if (menu is ApplicationMenu &&
						ribbon.Mode == RibbonMode.Normal)
						ribbon.EnterNavigationMode();

					// raise the Opened event
					menu.RaiseOpened(args);
				}
				else
				{
					// AS 3/25/10 TFS27670
					// I also noticed that when you had navigated to a recent item, closed the menu 
					// and reopened it, that recent item still appeared selected. This is because the 
					// menu is not clearing the selected state. Since we're tracking the menu item 
					// that was selected, we can clear it when we close.
					//
					if (null != menu._selectedMenuItem && Selector.GetIsSelected(menu._selectedMenuItem))
					{
						menu._selectedMenuItem.ClearValue(Selector.IsSelectedProperty);
					}

					menu._selectedMenuItem = null;

					// AS 10/10/07 PopupOwnerProxy
					if (menu._popupOwnerProxy != null)
						menu._popupOwnerProxy.OnClose();

					// raise the Closed event
					menu.RaiseClosed(args);
				}

                // TK Raise property change event
                bool newValue = (bool)e.NewValue;
                bool oldValue = !newValue;
                menu.RaiseAutomationExpandCollapseStateChanged(oldValue, newValue);
			}
		}

		/// <summary>
		/// Gets/sets whether the menu is open.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> Changing its value will cause either the <see cref="Closed"/> event or the <see cref="Opening"/>/<see cref="Opened"/> event pair to be raised.</para>
		/// </remarks>
		/// <seealso cref="IsOpenProperty"/>
		//[Description("Gets/sets whether the menu is open.")]
		//[Category("Ribbon Properties")]
		public bool IsOpen
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.IsOpenProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.IsOpenProperty, value);
			}
		}

				#endregion //IsOpen

				#region MenuItemDescriptionMinWidth

		/// <summary>
		/// Identifies the <see cref="MenuItemDescriptionMinWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MenuItemDescriptionMinWidthProperty = DependencyProperty.Register("MenuItemDescriptionMinWidth",
			typeof(double), typeof(MenuToolBase), new FrameworkPropertyMetadata(200d));

		/// <summary>
		/// Returns/sets the minimum width of the <see cref="MenuItemDescriptionProperty"/>s (if any) associated with the MenuTool's menu items.
		/// </summary>
		/// <seealso cref="MenuItemDescriptionMinWidthProperty"/>
		/// <seealso cref="MenuItemDescriptionProperty"/>
		/// <seealso cref="HasMenuItemDescriptionProperty"/>
		//[Description("Returns/sets the minimum width of the MenuItemDescriptions (if any) associated with the MenuTool's menu items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double MenuItemDescriptionMinWidth
		{
			get
			{
				return (double)this.GetValue(MenuToolBase.MenuItemDescriptionMinWidthProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.MenuItemDescriptionMinWidthProperty, value);
			}
		}

				#endregion //MenuItemDescriptionMinWidth

				#region SubmenuHeaderTemplateKey

		/// <summary>
		/// The key used to identify the ControlTemplate used for menu tools when they appear inside another menu
		/// </summary>
		public static readonly ResourceKey SubmenuHeaderTemplateKey = new StaticPropertyResourceKey(typeof(MenuToolBase), "SubmenuHeaderTemplateKey");

				#endregion //SubmenuHeaderTemplateKey

				#region SubmenuItemTemplateKey

		/// <summary>
		/// The key used to identify the ControlTemplate used for any tools (other than menu tools) when they appear inside a menu
		/// </summary>
		public static readonly ResourceKey SubmenuItemTemplateKey = new StaticPropertyResourceKey(typeof(MenuToolBase), "SubmenuItemTemplateKey");

				#endregion //SubmenuItemTemplateKey

				// AS 10/12/07 UseLargeImages
				#region UseLargeImages

		/// <summary>
		/// Identifies the <see cref="UseLargeImages"/> dependency property
		/// </summary>
		public static readonly DependencyProperty UseLargeImagesProperty = DependencyProperty.Register("UseLargeImages",
			typeof(bool), typeof(MenuToolBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Returns/sets a boolean indicating whether the items within the menu should display large images.
		/// </summary>
		/// <seealso cref="UseLargeImagesProperty"/>
		//[Description("Returns/sets a boolean indicating whether the items within the menu should display large images.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool UseLargeImages
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.UseLargeImagesProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.UseLargeImagesProperty, value);
			}
		}

				#endregion //UseLargeImages

			#endregion //Public Properties

			#region Common Tool Properties

				#region Caption

		/// <summary>
		/// Identifies the Caption dependency property.
		/// </summary>
		/// <seealso cref="Caption"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		public static readonly DependencyProperty CaptionProperty = RibbonToolHelper.CaptionProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns/sets the caption associated with the tool.
		/// </summary>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="HasCaption"/>
		//[Description("Returns/sets the caption associated with the tool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Caption
		{
			get
			{
				return (string)this.GetValue(MenuToolBase.CaptionProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.CaptionProperty, value);
			}
		}

				#endregion //Caption

				#region HasCaption

		/// <summary>
		/// Identifies the HasCaption dependency property.
		/// </summary>
		/// <seealso cref="HasCaption"/>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="Caption"/>
		public static readonly DependencyProperty HasCaptionProperty = RibbonToolHelper.HasCaptionProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns true if the tool has a caption with a length greater than zero, otherwise returns false. (read only)
		/// </summary>
		/// <seealso cref="HasCaptionProperty"/>
		/// <seealso cref="CaptionProperty"/>
		/// <seealso cref="Caption"/>
		//[Description("Returns true if the tool has a caption with a length greater than zero, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool HasCaption
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.HasCaptionProperty);
			}
		}

				#endregion //HasCaption

				#region Id

		/// <summary>
		/// Identifies the Id dependency property.
		/// </summary>
		/// <seealso cref="Id"/>
		public static readonly DependencyProperty IdProperty = RibbonToolHelper.IdProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns/sets the Id associated with the tool.
		/// </summary>
		/// <seealso cref="IdProperty"/>
		//[Description("Returns/sets the Id associated with the tool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Id
		{
			get
			{
				return (string)this.GetValue(MenuToolBase.IdProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.IdProperty, value);
			}
		}

				#endregion //Id

				#region IsActive

		/// <summary>
		/// Identifies the IsActive dependency property.
		/// </summary>
		/// <seealso cref="IsActive"/>
		public static readonly DependencyProperty IsActiveProperty = XamRibbon.IsActiveProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns true if the tool is the current active item, otherwise returns false. (read only)
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		//[Description("Returns true if the tool is the current active item, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.IsActiveProperty);
			}
		}

				#endregion //IsActive

				#region IsQatCommonTool

		/// <summary>
		/// Identifies the IsQatCommonTool dependency property.
		/// </summary>
		/// <seealso cref="IsQatCommonTool"/>
		public static readonly DependencyProperty IsQatCommonToolProperty = RibbonToolHelper.IsQatCommonToolProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns true if the tool should be shown in the list of 'common tools' displayed in the <see cref="QuickAccessToolbar"/>'s Quick Customize Menu. 
		/// </summary>
		/// <seealso cref="IsQatCommonToolProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		//[Description("Returns true if the tool should be shown in the list of 'common tools' displayed in the QuickAccessToolbar's Quick Customize Menu.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsQatCommonTool
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.IsQatCommonToolProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.IsQatCommonToolProperty, value);
			}
		}

				#endregion //IsQatCommonTool

				#region IsOnQat

		/// <summary>
		/// Identifies the IsOnQat dependency property.
		/// </summary>
		/// <seealso cref="IsOnQat"/>
		public static readonly DependencyProperty IsOnQatProperty = RibbonToolHelper.IsOnQatProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns true if the tool (or an instance of the tool with the same Id) exists on the <see cref="QuickAccessToolbar"/>, otherwise returns false. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">To determine whether a specific tool instance is directly on the Qat, check the <see cref="Location"/> property.</p>
		/// </remarks>
		/// <seealso cref="IsOnQatProperty"/>
		/// <seealso cref="QuickAccessToolbar"/>
		/// <seealso cref="QatPlaceholderTool"/>
		/// <seealso cref="Location"/>
		//[Description("Returns true if the tool (or an instance of the tool with the same Id) exists on the QuickAccessToolbar, otherwise returns false. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsOnQat
		{
			get
			{
				return (bool)this.GetValue(MenuToolBase.IsOnQatProperty);
			}
		}

				#endregion //IsOnQat

				#region KeyTip

		/// <summary>
		/// Identifies the KeyTip dependency property.
		/// </summary>
		/// <seealso cref="KeyTip"/>
		public static readonly DependencyProperty KeyTipProperty = RibbonToolHelper.KeyTipProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// A string with a maximum length of 3 characters that is used to navigate to the item when keytips.
		/// </summary>
		/// <remarks>
		/// <p class="body">Key tips are displayed when the ribbon is showing and the Alt key is pressed.</p>
		/// <p class="note"><br>Note: </br>If the key tip for the item conflicts with another item in the same container, this key tip may be changed.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">The value assigned has more than 3 characters.</exception>
		/// <seealso cref="KeyTipProperty"/>
		//[Description("A string with a maximum length of 3 characters that is used to navigate to the item when keytips.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string KeyTip
		{
			get
			{
				return (string)this.GetValue(MenuToolBase.KeyTipProperty);
			}
			set
			{
				this.SetValue(MenuToolBase.KeyTipProperty, value);
			}
		}

				#endregion //KeyTip

				#region Location

		/// <summary>
		/// Identifies the Location dependency property.
		/// </summary>
		/// <seealso cref="Location"/>
		public static readonly DependencyProperty LocationProperty = XamRibbon.LocationProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns an enumeration that indicates the location of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible tool locations include: Ribbon, Menu, QuickAccessToolbar, ApplicationMenu, ApplicationMenuFooterToolbar, ApplicationMenuRecentItems.</p>
		/// </remarks>
		/// <seealso cref="LocationProperty"/>
		/// <seealso cref="ToolLocation"/>
		//[Description("Returns an enumeration that indicates the location of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ToolLocation Location
		{
			get
			{
				return (ToolLocation)this.GetValue(MenuToolBase.LocationProperty);
			}
		}

				#endregion //Location

				#region SizingMode

		/// <summary>
		/// Identifies the SizingMode dependency property.
		/// </summary>
		/// <seealso cref="SizingMode"/>
		public static readonly DependencyProperty SizingModeProperty = RibbonToolHelper.SizingModeProperty.AddOwner(typeof(MenuToolBase));

		/// <summary>
		/// Returns an enumeration that indicates the current size of the tool. (read only)
		/// </summary>
		/// <remarks>
		/// <p class="body">Possible sizes include: ImageOnly, ImageAndTextNormal, ImageAndTextLarge.</p>
		/// </remarks>
		/// <seealso cref="SizingModeProperty"/>
		/// <seealso cref="RibbonToolSizingMode"/>
		//[Description("Returns an enumeration that indicates the current size of the tool. (read only)")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public RibbonToolSizingMode SizingMode
		{
			get
			{
				return (RibbonToolSizingMode)this.GetValue(MenuToolBase.SizingModeProperty);
			}
		}

				#endregion //SizingMode

			#endregion //Common Tool Properties

			#region Attached Properties

				#region HasMenuItemDescription

		internal static readonly DependencyPropertyKey HasMenuItemDescriptionPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("HasMenuItemDescription",
			typeof(bool), typeof(MenuToolBase), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the 'HasMenuItemDescription' attached readonly property which returns true if the attached 
		/// <see cref="MenuItemDescriptionProperty"/> has been set.
		/// </summary>
		/// <seealso cref="GetHasMenuItemDescription"/>
		public static readonly DependencyProperty HasMenuItemDescriptionProperty =
			HasMenuItemDescriptionPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns true if the attached <see cref="MenuItemDescriptionProperty"/> has been set.
		/// </summary>
		/// <seealso cref="HasMenuItemDescriptionProperty"/>
		/// <seealso cref="MenuItemDescriptionProperty"/>
		/// <seealso cref="GetMenuItemDescription"/>
		public static bool GetHasMenuItemDescription(DependencyObject d)
		{
			return (bool)d.GetValue(MenuToolBase.HasMenuItemDescriptionProperty);
		}

		internal static void SetHasMenuItemDescription(DependencyObject d, bool value)
		{
			d.SetValue(MenuToolBase.HasMenuItemDescriptionPropertyKey, value);
		}

				#endregion //HasMenuItemDescription

				// JM 11-05-07 - Change property to type object.
				#region MenuItemDescription

					#region Old definition with property as type string
		///// <summary>
		///// Identifies the 'MenuItemDescription' attached dependency property which returns/sets the optional descriptive string
		///// displayed for a tool when the tool is displayed in a menu.
		///// </summary>
		///// <seealso cref="GetMenuItemDescription"/>
		///// <seealso cref="SetMenuItemDescription"/>
		///// <seealso cref="GetHasMenuItemDescription"/>
		//public static readonly DependencyProperty MenuItemDescriptionProperty = DependencyProperty.RegisterAttached("MenuItemDescription",
		//    typeof(string), typeof(MenuToolBase), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMenuItemDescriptionChanged)));

		//private static void OnMenuItemDescriptionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		//{
		//    string menuItemDescription = e.NewValue as string;

		//    // Maintain the HasMenuItemDescription property.
		//    if (string.IsNullOrEmpty(menuItemDescription) == false)
		//        target.SetValue(MenuToolBase.HasMenuItemDescriptionPropertyKey, KnownBoxes.TrueBox);
		//    else
		//        target.ClearValue(MenuToolBase.HasMenuItemDescriptionPropertyKey);
		//}

		///// <summary>
		///// Returns the optional descriptive string displayed for a tool when the tool is displayed in a menu.
		///// </summary>
		///// <seealso cref="MenuItemDescriptionProperty"/>
		///// <seealso cref="SetMenuItemDescription"/>
		///// <seealso cref="GetHasMenuItemDescription"/>
		//[AttachedPropertyBrowsableForChildren()]
		//public static string GetMenuItemDescription(DependencyObject d)
		//{
		//    return (string)d.GetValue(MenuToolBase.MenuItemDescriptionProperty);
		//}

		///// <summary>
		///// Sets the optional descriptive string displayed for a tool when the tool is displayed in a menu.
		///// </summary>
		///// <seealso cref="MenuItemDescriptionProperty"/>
		///// <seealso cref="GetMenuItemDescription"/>
		///// <seealso cref="GetHasMenuItemDescription"/>
		//public static void SetMenuItemDescription(DependencyObject d, string value)
		//{
		//    d.SetValue(MenuToolBase.MenuItemDescriptionProperty, value);
		//}

					#endregion //Old definition with property as type string

		/// <summary>
		/// Identifies the 'MenuItemDescription' attached dependency property which returns/sets the optional descriptive content
		/// displayed for a tool when the tool is displayed in a menu.
		/// </summary>
		/// <seealso cref="GetMenuItemDescription"/>
		/// <seealso cref="SetMenuItemDescription"/>
		/// <seealso cref="GetHasMenuItemDescription"/>
		public static readonly DependencyProperty MenuItemDescriptionProperty = DependencyProperty.RegisterAttached("MenuItemDescription",
			typeof(object), typeof(MenuToolBase), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnMenuItemDescriptionChanged)));

		private static void OnMenuItemDescriptionChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			// Maintain the HasMenuItemDescription property.
			if (e.NewValue != null)
				target.SetValue(MenuToolBase.HasMenuItemDescriptionPropertyKey, KnownBoxes.TrueBox);
			else
				target.ClearValue(MenuToolBase.HasMenuItemDescriptionPropertyKey);
		}

		/// <summary>
		/// Returns the optional descriptive content displayed for a tool when the tool is displayed in a menu.
		/// </summary>
		/// <seealso cref="MenuItemDescriptionProperty"/>
		/// <seealso cref="SetMenuItemDescription"/>
		/// <seealso cref="GetHasMenuItemDescription"/>
		[AttachedPropertyBrowsableForChildren()]
		[TypeConverter(typeof(StringConverter))]	// JM BR29038 12-13-07
		public static object GetMenuItemDescription(DependencyObject d)
		{
			return d.GetValue(MenuToolBase.MenuItemDescriptionProperty);
		}

		/// <summary>
		/// Sets the optional descriptive content displayed for a tool when the tool is displayed in a menu.
		/// </summary>
		/// <seealso cref="MenuItemDescriptionProperty"/>
		/// <seealso cref="GetMenuItemDescription"/>
		/// <seealso cref="GetHasMenuItemDescription"/>
		[TypeConverter(typeof(StringConverter))]	// JM BR29038 12-13-07
		public static void SetMenuItemDescription(DependencyObject d, object value)
		{
			d.SetValue(MenuToolBase.MenuItemDescriptionProperty, value);
		}

				#endregion //MenuItemDescription

				#region InputGestureText

		/// <summary>
		/// Identifies the InputGestureText attached dependency property which describes an input gesture that will call the command tied to the specified item. 
		/// </summary>
		/// <seealso cref="GetInputGestureText"/>
		/// <seealso cref="SetInputGestureText"/>
		public static readonly DependencyProperty InputGestureTextProperty = DependencyProperty.RegisterAttached("InputGestureText",
			typeof(string), typeof(MenuToolBase), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Gets the value of the 'InputGestureText' attached property which describes an input gesture that will call the command tied to the specified item. 
		/// </summary>
		/// <seealso cref="InputGestureTextProperty"/>
		/// <seealso cref="SetInputGestureText"/>
		[AttachedPropertyBrowsableForChildren()]
		public static string GetInputGestureText(DependencyObject d)
		{
			return (string)d.GetValue(MenuToolBase.InputGestureTextProperty);
		}

		/// <summary>
		/// Sets the value of the 'InputGestureText' attached property which describes an input gesture that will call the command tied to the specified item. 
		/// </summary>
		/// <seealso cref="InputGestureTextProperty"/>
		/// <seealso cref="GetInputGestureText"/>
		public static void SetInputGestureText(DependencyObject d, string value)
		{
			d.SetValue(MenuToolBase.InputGestureTextProperty, value);
		}

				#endregion //InputGestureText

				#region ToolMenuItem

		internal static readonly DependencyProperty ToolMenuItemProperty = DependencyProperty.RegisterAttached("ToolMenuItem",
			typeof(ToolMenuItem), typeof(MenuToolBase), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnToolMenuItemChanged) ));

		private static void OnToolMenuItemChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			MenuToolBase menuTool = target as MenuToolBase;

			if (menuTool != null && !(menuTool is ApplicationMenu))
			{
				ToolMenuItem oldTmi = e.OldValue as ToolMenuItem;

				// clear the old IsOpen binding
				if (oldTmi != null)
					BindingOperations.ClearBinding(oldTmi, ToolMenuItem.IsSubmenuOpenProperty);

				ToolMenuItem tmi = e.NewValue as ToolMenuItem;

				// set up a binding with the IsOpen property with the new value
				if (tmi != null)
				{
					tmi.SetBinding(ToolMenuItem.IsSubmenuOpenProperty, Utilities.CreateBindingObject(MenuToolBase.IsOpenProperty, BindingMode.TwoWay, menuTool));
				
					MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(menuTool) as MenuToolBase;

					if (rootMenu != null &&
						rootMenu._currentReparentedToolsOwner == oldTmi)
						rootMenu.SetReparentedToolsOwner(tmi);
				}
			}
		}

		internal static ToolMenuItem GetToolMenuItem(DependencyObject d)
		{
			return (ToolMenuItem)d.GetValue(MenuToolBase.ToolMenuItemProperty);
		}

		internal static void SetToolMenuItem(DependencyObject d, ToolMenuItem value)
		{
			d.SetValue(MenuToolBase.ToolMenuItemProperty, value);
		}

				#endregion //ToolMenuItem
    
			#endregion //Attached Properties

			#region Private Properties
   
			#endregion //Private Properties

			#region Internal Properties

				#region InternalMenu

		internal MenuToolMenu InternalMenu { get { return this._menu; } }

				#endregion //InternalMenu	

				// JM 10-15-08 [BR35158 - TFS6448]
				#region ItemsControlItemStringFormatProperty

		internal static DependencyProperty ItemsControlItemStringFormatProperty
		{
			get
			{
				if (MenuToolBase._itemsControlHasItemStringFormatProperty.HasValue == false)
				{
					DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromName("ItemStringFormat", typeof(ItemsControl), typeof(ItemsControl));
					MenuToolBase._itemsControlHasItemStringFormatProperty = (dpd != null);
					MenuToolBase._itemsControlItemStringFormatProperty = (dpd == null) ? null : dpd.DependencyProperty;
				}

				return MenuToolBase._itemsControlItemStringFormatProperty;
			}
		}

				#endregion //ItemsControlItemStringFormatProperty

				// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
				#region ItemsToBind





		internal virtual IList ItemsToBind
		{
			get { return this.Items; }
		} 
				#endregion //ItemsToBind

				// AS 12/19/07 BR29199
				#region OpeningReason
		internal PopupOpeningReason OpeningReason
		{
			get { return this._openingReason; }
		}

		internal PopupOpeningReason OpeningReasonResolved
		{
			get
			{
				PopupOpeningReason reason = this._openingReason;

				if (reason == PopupOpeningReason.None)
				{
					if (this._popupOwnerProxy != null)
						reason = this._popupOwnerProxy.OpeningReason;

					if (reason == PopupOpeningReason.None)
					{
						ToolMenuItem menuItem = GetToolMenuItem(this);

						if (null != menuItem)
							reason = menuItem.OpeningReason;
					}
				}

				return reason;
			}
		}
				#endregion //OpeningReason

				#region Presenter

		internal ToolMenuItem Presenter
		{
			get { return this._presenter; }
		}

				#endregion //Presenter	

				#region PresenterSite

		internal ContentPresenter PresenterSite
		{
			get { return this._presenterSite; }
		}

				#endregion //PresenterSite	

				// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
				#region ReparentedTools
		internal ICollection ReparentedTools
		{
			get { return this._reparentedTools; }
		} 
				#endregion //ReparentedTools

				#region SelectedMenuItem

		internal MenuItem SelectedMenuItem { get { return this._selectedMenuItem; } }

				#endregion //SelectedMenuItem	
    
			#endregion Internal Properties

		#endregion //Properties

		#region Methods

			#region Protected methods (common tool methods)

				#region OnRaiseToolEvent

		/// <summary>
		/// Occurs when the tool event is about to be raised
		/// </summary>
		/// <remarks><para class="body">The base class implemenation simply calls the RaiseEvent method.</para></remarks>
		protected virtual void OnRaiseToolEvent(RoutedEventArgs args)
		{
			this.RaiseEvent(args);
		}

				#endregion //OnRaiseToolEvent

			#endregion //Protected methods (common tool methods)

			#region Protected methods

				#region CreateMenuToolPresenter

		/// <summary>
		/// Called to create an instance of a MenuToolPresenter to represent this tool on the ribbon.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the DataContext of this presenter will be set by the caller to this tool.</para></remarks>
		/// <returns>A newly created instance of a <see cref="MenuToolPresenter"/> with its Content initialized appropriately.</returns>
		/// <seealso cref="InitializeMenuToolPresenter"/>
		protected virtual ToolMenuItem CreateMenuToolPresenter()
		{
			return new MenuToolPresenter();
		}

				#endregion //CreateMenuToolPresenter	

				#region InitializeMenuToolPresenter

		/// <summary>
		/// Called to initialize the instance of a ToolMenuItem returned from <see cref="CreateMenuToolPresenter"/>.
		/// </summary>
		/// <param name="menuToolPresenter">The menuToolPresenter returned from CreateMenuToolPresenter.</param>
		/// <seealso cref="CreateMenuToolPresenter"/>
		protected virtual void InitializeMenuToolPresenter(ToolMenuItem menuToolPresenter)
		{
			
		}

				#endregion //InitializeMenuToolPresenter	

                #region VisualState... Methods


        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {
            RibbonToolHelper.SetToolVisualState(this, useTransitions, this.IsActive);

        }

        // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            MenuToolBase tool = target as MenuToolBase;

            if ( tool != null )
                tool.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



                #endregion //VisualState... Methods	
    
			#endregion //Protected methods	

			#region Internal Methods

				#region GetKeyTipProviders
		internal virtual IKeyTipProvider[] GetKeyTipProviders()
		{
			
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

			ToolMenuItem menuItem = this._presenter ?? GetToolMenuItem(this);

			if (null != menuItem)
				return menuItem.GetChildKeyTipProviders();

			return new IKeyTipProvider[0];
		}
				#endregion //GetKeyTipProviders

				#region GetReparentedToolsOwner

		// JJD 10/16/07
		// Keep track of the instance of the menutool that currently owns the reparented tools.
		internal ToolMenuItem GetReparentedToolsOwner()
		{
			MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

			Debug.Assert(rootMenu != null);

			if (rootMenu == null)
				return null;

			if (rootMenu._currentReparentedToolsOwner == null)
				rootMenu._currentReparentedToolsOwner = MenuToolBase.GetToolMenuItem(rootMenu);

			return rootMenu._currentReparentedToolsOwner;
		}
	
				#endregion //GetReparentedToolsOwner	
    
				#region OnMenuItemSelected
		internal void OnMenuItemSelected(ToolMenuItem toolMenuItem, bool isSelected)
		{
			if (isSelected)
			{
				this._selectedMenuItem = toolMenuItem;
			}
			else
			{
				if (toolMenuItem == this._selectedMenuItem)
					this._selectedMenuItem = null;
			}
		} 
				#endregion //OnMenuItemSelected

				// AS 12/19/07 BR29199
				#region OpenMenu
		internal void OpenMenu(PopupOpeningReason reason)
		{
			try
			{
				Debug.Assert(this._openingReason == PopupOpeningReason.None);
				this._openingReason = reason;
				this.IsOpen = true;
			}
			finally
			{
				this._openingReason = PopupOpeningReason.None;
			}
		} 
				#endregion //OpenMenu

				#region SetReparentedToolsOwner

		// JJD 10/16/07
		// Keep track of the instance of the menutool that currently owns the reparented tools.
		internal void SetReparentedToolsOwner(ToolMenuItem newOwner)
		{
			Debug.Assert(!_isReparenting);

			bool wasReparenting = _isReparenting;
			_isReparenting = true;

			try
			{
				SetReparentedToolsOwnerImpl(newOwner);
			}
			finally
			{
				_isReparenting = wasReparenting;
			}
		}

		bool _isReparenting = false;

		private void SetReparentedToolsOwnerImpl(ToolMenuItem newOwner)
		{
			Debug.Assert(newOwner != null);

			if (newOwner == null)
				return;

			ToolMenuItem oldOwner = this.GetReparentedToolsOwner();

            // JJD 11/27/07 
            // Don't get out just yet. For nested menus we still need to call VerifyOnlyOneGalleryTool
            // below so that the IsFirstInMenu and IsLastInMenu properties on the GalleryToolDropDownPresenter
            // get initialized correctly.
            //if (oldOwner == newOwner)
            //    return;

            // JJD 11/07/07
            // added isFirst/IsLast properties on GalleryToolDropDownPresenter to support triggering of separators
            // Therefore we need to call VerifyOnlyOneGalleryTool to make sure the flags are set properly
            if (this is MenuTool)
                ((MenuTool)this).VerifyOnlyOneGalleryTool(false);

            // JJD 11/27/07 
            // Now we can return after the call to VerifyOnlyOneGalleryTool if the owner hasn't changed (see note above)
			if (oldOwner == newOwner)
				return;

			MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

			Debug.Assert(rootMenu != null);

			if (rootMenu == null)
				return;

			// AS 10/17/07
			// When you dropdown menu A, any tool elements being embedded would be site in the 
			// menu items. Then you clone Menu A and drop it down. When the ToolMenuItems were 
			// being created the tool elements were properly reparented into the menu item. 
			// However, if you then redropped down the original menu A, the reparented elements 
			// were gone - they were still in the clone's menu items. We tried a couple of things 
			// to do fixups and none worked completely. Instead, we will not clear the ItemsSource
			// of the previous tools owner so that its menu items will be released. Then we will
			// set the new menu item as the itemssource.
			//

            // JJD 1/8/08 - BR29447
            // Suspend tool registration while we reparent to tools so we don't
            // unregister anything
            XamRibbon ribbon = XamRibbon.GetRibbon(this);
            if (ribbon != null)
                ribbon.SuspendToolRegistration();

            try
            {

                if (rootMenu._currentReparentedToolsOwner != null)
                    rootMenu._currentReparentedToolsOwner.ItemsSource = null;

                rootMenu._currentReparentedToolsOwner = newOwner;

                // AS 10/17/07
                // Make sure that the ItemsSource are the menu's tools.
                //
                if (rootMenu._currentReparentedToolsOwner != null)
                {
                    // AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
                    //rootMenu._currentReparentedToolsOwner.ItemsSource = this.Items;
                    rootMenu._currentReparentedToolsOwner.ItemsSource = this.ItemsToBind;
                }

                rootMenu.TransferReparentedItems(newOwner);
            }
            finally
            {
                // JJD 1/8/08 - BR29447
                // Resume tool registration now that we are done
                if (ribbon != null)
                    ribbon.ResumeToolRegistration();
            }

		}
	
				#endregion //SetReparentedToolsOwner	

				// AS 9/22/09 TFS22390
				#region VerifyReparentedToolsContainer
		internal void VerifyReparentedToolsContainer()
		{
			MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

			if (null == rootMenu || rootMenu._reparentedTools == null)
				return;

			XamRibbon ribbon = XamRibbon.GetRibbon(this);

			if (ribbon == null)
				return;

			ToolMenuItem currentOwner = rootMenu._currentReparentedToolsOwner;

			Debug.Assert(null != currentOwner);

			bool reparentAll = !Utilities.IsDescendantOf(ribbon, currentOwner, true) && !Utilities.IsDescendantOf(ribbon, currentOwner, false);

			for (int i = rootMenu._reparentedTools.Count - 1; i >= 0; i--)
			{
				FrameworkElement feItem = rootMenu._reparentedTools[i] as FrameworkElement;

				if (feItem == null)
					continue;

				DependencyObject toolParent = feItem.Parent;

				if (toolParent is ToolMenuItem.LogicalContainer)
					toolParent = LogicalTreeHelper.GetParent(toolParent);

				if (toolParent != _currentReparentedToolsOwner)
					continue;

				// if the owner itself is removed then we don't need to check each item that is within that menu item
				if (!reparentAll)
				{
					if (Utilities.IsDescendantOf(ribbon, feItem, true) || Utilities.IsDescendantOf(ribbon, feItem, false))
						continue;
				}

				// AS 10/8/09 TFS23328
				// Temporarily store the datacontext while we reparent the tool.
				//
				using (new TempValueReplacement(feItem, FrameworkElement.DataContextProperty))
				{
					currentOwner.RemoveLogicalChildInternal(feItem);
					rootMenu._reparentedTools.RemoveAt(i);
					rootMenu.AddLogicalChild(feItem);
				}
			}
		}
				#endregion //VerifyReparentedToolsContainer

				#region VerifyTypeOfChild

		internal static void VerifyTypeOfChild(object child)
		{
			if (child is MenuItem &&
				 !(child is ToolMenuItem))
				throw new NotSupportedException(XamRibbon.GetString("LE_MenuItemNotAllowed"));
		}

				#endregion //VerifyTypeOfChild

				#region VerifyTypeOfChildren

		internal static void VerifyTypeOfChildren(ItemsControl menu)
		{
			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			//ItemCollection items = menu.Items;
			IList items = menu is MenuToolBase ? ((MenuToolBase)menu).ItemsToBind : menu.Items;

			int count = items.Count;

			// loop over the reparentd tools backwards to see if they are
			// still in the items collection
			for (int i = 0; i < count; i++)
				VerifyTypeOfChild( items[i] );

		}

				#endregion //VerifyTypeOfChildren

			#endregion //Internal Methods	

			#region Private Methods

				// JM 02-03-09 TFS9245
				#region CoerceToolTipServiceIsEnabled

		// AS 10/16/09 TFS23117
		// Moved here from the MenuTool class.
		//
		private static object CoerceToolTipServiceIsEnabled(DependencyObject d, object newValue)
		{
			MenuToolBase menuTool = d as MenuToolBase;
			if (menuTool != null && menuTool.IsOpen)
				return false;

			return newValue;
		}

				#endregion //CoerceToolTipServiceIsEnabled
    
				#region EnsurePresenterIsInitialized

		private void EnsurePresenterIsInitialized()
		{
			if (this._presenter != null)
				return;

			this._presenter = this.CreateMenuToolPresenter();

			this._presenter.SetValue(XamRibbon.LocationPropertyKey, RibbonKnownBoxes.FromValue(this.Location));

			// set the presenter's DataContext to this tool
			// AS 6/9/08 BR32242
			// Since the tools are logical children of the presenter (because they need to
			// be in the focusscope of the menu), we cannot set the datacontext on these
			// top level menu items either or else the tools may (and will) use this as
			// their datacontext even though we "fixed" the datacontext of the visual
			// parent (the containing menu item).
			//
			//this._presenter.DataContext = this;
			this._presenter.SetValue(ToolMenuItem.ToolPropertyKey, this);

			// set the ToolMenuItem attached property
			this.SetValue(ToolMenuItemProperty, this._presenter);

			// bind the sizingmode property
			this._presenter.SetBinding(RibbonGroupPanel.SizingModeVersionProperty, Utilities.CreateBindingObject(RibbonGroupPanel.SizingModeVersionProperty, BindingMode.OneWay, this));

			// bind the header to the caption property
			this._presenter.SetBinding(MenuItem.HeaderProperty, Utilities.CreateBindingObject(RibbonToolHelper.CaptionProperty, BindingMode.OneWay, this));

			// JM 10-15-08 [BR35158 - TFS6448]
			this._presenter.SetBinding(ItemsControl.ItemTemplateProperty, Utilities.CreateBindingObject(ItemsControl.ItemTemplateProperty, BindingMode.OneWay, this));
			this._presenter.SetBinding(ItemsControl.ItemTemplateSelectorProperty, Utilities.CreateBindingObject(ItemsControl.ItemTemplateSelectorProperty, BindingMode.OneWay, this));
			this._presenter.SetBinding(ItemsControl.ItemContainerStyleProperty, Utilities.CreateBindingObject(ItemsControl.ItemContainerStyleProperty, BindingMode.OneWay, this));
			this._presenter.SetBinding(ItemsControl.ItemContainerStyleSelectorProperty, Utilities.CreateBindingObject(ItemsControl.ItemContainerStyleSelectorProperty, BindingMode.OneWay, this));
			this._presenter.SetBinding(ItemsControl.DisplayMemberPathProperty, Utilities.CreateBindingObject(ItemsControl.DisplayMemberPathProperty, BindingMode.OneWay, this));
			this._presenter.SetBinding(ItemsControl.ItemsPanelProperty, Utilities.CreateBindingObject(ItemsControl.ItemsPanelProperty, BindingMode.OneWay, this));

            // AS 10/16/08 TFS6447
			this._presenter.SetBinding(ItemsControl.ContextMenuProperty, Utilities.CreateBindingObject(ItemsControl.ContextMenuProperty, BindingMode.OneWay, this));

			if (MenuToolBase.ItemsControlItemStringFormatProperty != null)
				this._presenter.SetBinding(MenuToolBase.ItemsControlItemStringFormatProperty, Utilities.CreateBindingObject("ItemStringFormat", BindingMode.OneWay, this));

			// set the toplevel menu item's ItemsSource to this tool's items
			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			//this._presenter.ItemsSource = this.Items;
            // AS 1/30/09 TFS13295
            // We should only set the ItemsSource if this is the MenuItem that owns the items. 
            // A new menutoolpresenter could have been created because the tool was cloned 
            // for the qat but the main tool may still own the items.
            //
            //this._presenter.ItemsSource = this.ItemsToBind;
            MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(this) as MenuToolBase;

            // AS 2/5/09 TFS13295
            // In the case of the application menu, we do want to set the ItemsSource
            // since there is no _currentReparentedToolsOwner set yet. We'll also 
            // set the _currentReparentedToolsOwner in case anyone needs to reparent
            // them thereafter.
            //
            //if (null == rootMenu || rootMenu._currentReparentedToolsOwner == this._presenter)
            //    this._presenter.ItemsSource = this.ItemsToBind;
            if (null == rootMenu ||
                rootMenu._currentReparentedToolsOwner == this._presenter ||
                (rootMenu == this && rootMenu._currentReparentedToolsOwner == null))
            {
                if (null != rootMenu)
                    rootMenu._currentReparentedToolsOwner = this._presenter;

                this._presenter.ItemsSource = this.ItemsToBind;
            }

			this._presenter.SetBinding(ToolMenuItem.IsSubmenuOpenProperty, Utilities.CreateBindingObject(IsOpenProperty, BindingMode.TwoWay, this));

			this.InitializeMenuToolPresenter(this._presenter);
		}

				#endregion //EnsurePresenterIsInitialized	
    
				#region OnIsActiveChanged

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			MenuToolBase menuTool = d as MenuToolBase;

			if (null != menuTool)
			{
				MenuToolPresenter mtp = menuTool._presenter as MenuToolPresenter;

				if (mtp != null)
					mtp.OnIsActiveChanged();

                // JJD 4/13/10 - NA2010 Vol 2 - Added support for VisualStateManager
                menuTool.UpdateVisualStates();

            }
		}
 
				#endregion //OnIsActiveChanged

                #region RaiseAutomationExpandCollapseStateChanged

        private void RaiseAutomationExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            MenuToolBaseAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as MenuToolBaseAutomationPeer;

            if (null != peer)
                peer.RaiseExpandCollapseStateChanged(oldValue, newValue);
        }

                #endregion //RaiseAutomationExpandCollapseStateChanged	

				#region SetLocation

		private void SetLocation(DependencyObject element, object item)
		{
			object location;

			if (this is ApplicationMenu)
			{
				// AS 11/20/07 ApplicationMenuRecentItem to ToolMenuItem
				int index = this.Presenter.ItemContainerGenerator.IndexFromContainer(element);

				if (index >= this.Items.Count)
					location = RibbonKnownBoxes.ToolLocationApplicationMenuRecentItemsBox;
				else
					location = RibbonKnownBoxes.ToolLocationApplicationMenuBox;
			}
			else
			if (this.Location == ToolLocation.ApplicationMenu)
				location = RibbonKnownBoxes.ToolLocationApplicationMenuSubMenuBox;
			else
				location = RibbonKnownBoxes.ToolLocationMenuBox;

			element.SetValue(XamRibbon.LocationPropertyKey, location);

			DependencyObject dItem = item as DependencyObject;

			if ( dItem != null )
				dItem.SetValue(XamRibbon.LocationPropertyKey, location);


		}

				#endregion //SetLocation	
    
				#region TransferReparentedItems

		// JJD 10/15/07 
		// Added method to TransferReparentedItems when an instance of the menu is about to be dropped down
		private void TransferReparentedItems(ToolMenuItem newOwner)
		{
			if (this._reparentedTools == null)
				return;

			int count = this._reparentedTools.Count;

			if (count == 0)
				return;

			XamRibbon ribbon = XamRibbon.GetRibbon(this);

			Debug.Assert(ribbon != null);

			// temporarily suspend tool registration so the tool avoids being 
			// unregistered simply to be re-registered on the next line when
			// it is re-parented below
			if (ribbon != null)
				ribbon.SuspendToolRegistration();
			
			try
			{
				// loop over the reparented tools and reparent them to the newparent
				for (int i = 0; i < count; i++)
				{
					FrameworkElement child = this._reparentedTools[i] as FrameworkElement;
					
					// clear the tool's logical parent
					DependencyObject oldParent = child.Parent;

					// AS 1/5/10 TFS25529
					if (oldParent is ToolMenuItem.LogicalContainer)
						oldParent = LogicalTreeHelper.GetParent(oldParent);

					// AS 10/8/09 TFS23328
					// Reorganized the following block.
					//
					//if (oldParent is ToolMenuItem)
					//    ((ToolMenuItem)oldParent).RemoveLogicalChildInternal(child);
					//else
					//    if ( oldParent != null )
					//        continue;
					if (oldParent is ToolMenuItem == false && oldParent != null)
						continue;

					// AS 10/8/09 TFS23328
					// Temporarily store the datacontext while we reparent the tool.
					//
					using (new TempValueReplacement(child, FrameworkElement.DataContextProperty))
					{
						if (oldParent is ToolMenuItem)
							((ToolMenuItem)oldParent).RemoveLogicalChildInternal(child);

						// AS 11/7/07 BR28308
						// There was still a problem with tools that were embedded in the menu item. Even though 
						// we removed it from the logical tree, it was still in the visual tree of the old menu item.
						// Because of this, we never got an UnregisterTool call so when we trying to register the 
						// embedded tool (a label in this case), its logical parent was a toolmenuitem which doesn't
						// implement iribbontoollocation. So we need to make sure it comes out of the visual tree of 
						// the old menu item.
						//
						ToolMenuItem oldMenuItem = Utilities.GetAncestorFromType(child, typeof(ToolMenuItem), true) as ToolMenuItem;

						if (oldMenuItem != null && oldMenuItem.DataContext == child)
							oldMenuItem.DataContext = null;

						// add the tool as the logical child of the menu
						newOwner.AddLogicalChildInternal(child);
					}
				}

			}
			finally
			{
				// resume the registration of tools
				if (ribbon != null)
					ribbon.ResumeToolRegistration();
			}

		}

				#endregion //TransferReparentedItems
    
				#region VerifyReparentedItems

		// AS 3/24/10 TFS29482
		//private void VerifyReparentedItems()
		internal void VerifyReparentedItems()
		{
			if (this._reparentedTools == null)
				return;

			// if we aren't the current owener of the reparented tools then retutn
			if (this != RibbonToolProxy.GetRootSourceTool(this))
				return;

			int count = this._reparentedTools.Count;

			if (count == 0)
				return;

			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			//ItemCollection items = this.Items;
			IList items = this.ItemsToBind;

			// loop over the reparented tools backwards to see if they are
			// still in the items collection
			for (int i = count - 1; i >= 0; i--)
			{
				FrameworkElement child = this._reparentedTools[i] as FrameworkElement;

				// see if the child is in the items collection
				if (child != null && 
					!items.Contains(child))
				{
					// since it wasn't in the items collection remove it from the cached list
					this._reparentedTools.RemoveAt(i);

					ToolMenuItem parent = child.Parent as ToolMenuItem;

					// AS 1/5/10 TFS25529
					if (parent == null && child.Parent is ToolMenuItem.LogicalContainer)
						parent = LogicalTreeHelper.GetParent(child.Parent) as ToolMenuItem;

					// clear the tool's logical parent
					if ( parent != null)
						parent.RemoveLogicalChildInternal(child);
				}
			}

		}

				#endregion //VerifyReparentedItems

			#endregion //Private Methods

		#endregion //Methods

		#region IRibbonTool Members

		RibbonToolProxy IRibbonTool.ToolProxy
		{
			get { return MenuToolBaseProxy.Instance; }
		}

		#endregion
		
		#region IRibbonToolLocation Members

		ToolLocation IRibbonToolLocation.Location
		{
			get { return ToolLocation.Menu; }
		}

		#endregion

		#region MenuToolMenu internal class

		internal class MenuToolMenu : Menu
		{
			#region Private Members

			private MenuToolBase	_menuTool;

			// AS 11/5/07
			private static DependencyProperty _showKeyboardCuesProperty = null;

			#endregion //Private Members

			#region Constructors

			static MenuToolMenu()
			{
                // AS 2/12/09 TFS12346
                // We should not be setting the DefaultStyleKey to null. When the menu is 
                // added to the logical tree of the MenuToolBase, it is picking up the Foreground, etc.
                // of the MenuToolBase. However, during its OnInitialized if the ThemeStyle 
                // for the menu had not been accessed (which it won't be) then the theme 
                // style is updated. If the actual Menu class has been used and its Style 
                // has been loaded, this ends up getting that style. Thats why having a 
                // TabGroupPane within a DocumentContentHost or a ContentPane in a docked/floating 
                // split pane was leading to the issue - because those templates use a Menu.
                // The same thing happens if you just have a menu on the window with the ribbon 
                // albeit unlikely. Anyway, that default Menu style has setters which bind 
                // the foreground and font properties to those of the SystemFonts class. 
                // Since we don't want that but want it to inherit these properties we need 
                // to set the DefaultStyleKey to this class' type and define a default style 
                // that provides a basic template.
                //
				//FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(null));
                FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(typeof(MenuToolMenu)));

                #region Define Default Template

                FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(Border));
                fefRoot.SetValue(Border.BorderThicknessProperty, new TemplateBindingExtension(Menu.BorderThicknessProperty));
                fefRoot.SetValue(Border.BorderBrushProperty, new TemplateBindingExtension(Menu.BorderThicknessProperty));
                fefRoot.SetValue(Border.PaddingProperty, new TemplateBindingExtension(Menu.PaddingProperty));
                fefRoot.SetValue(Border.BackgroundProperty, new TemplateBindingExtension(Menu.BackgroundProperty));
                fefRoot.SetValue(Border.SnapsToDevicePixelsProperty, KnownBoxes.TrueBox);
                FrameworkElementFactory fefIP = new FrameworkElementFactory(typeof(ItemsPresenter));
                fefIP.SetValue(ItemsPresenter.SnapsToDevicePixelsProperty, new TemplateBindingExtension(Menu.SnapsToDevicePixelsProperty));
                fefRoot.AppendChild(fefIP);
                ControlTemplate template = new ControlTemplate(typeof(MenuToolMenu));
                template.VisualTree = fefRoot;
                template.Seal();
                Control.TemplateProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(template)); 

                #endregion //Define Default Template

				Menu.IsMainMenuProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

				FrameworkElementFactory fef = new FrameworkElementFactory(typeof(CardPanel));
				MenuToolMenu.ItemsPanelProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(new ItemsPanelTemplate(fef)));

				EventManager.RegisterClassHandler(typeof(MenuToolMenu), Mouse.LostMouseCaptureEvent, new MouseEventHandler(OnLostMouseCapture));

                // AS 11/10/08 TFS6035
                EventManager.RegisterClassHandler(typeof(MenuToolMenu), Mouse.PreviewMouseDownOutsideCapturedElementEvent, new MouseButtonEventHandler(OnPreviewMouseDownOutsideCaptured));

				// AS 10/9/07
				// Allow focus to shift out of the menutoolmenu since it is just used within a single menu tool.
				//
				KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(MenuToolMenu), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

				// AS 11/5/07
				// We need to set the ShowKeyboardCues property but its currently internal so we need to try and use reflection to get to 
				// it. If we can't get to it, we'll try to pick it up later if the Alt key is pressed, etc.
				//
				try
				{
					System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
					System.Reflection.FieldInfo field = typeof(KeyboardNavigation).GetField("ShowKeyboardCuesProperty", bindingFlags);
					MenuToolMenu._showKeyboardCuesProperty = field.GetValue(null) as DependencyProperty;
				}
				catch
				{
				}
			}

			internal MenuToolMenu(MenuToolBase menuTool)
			{
				this._menuTool = menuTool;
			}

			#endregion //Constructors
    
			#region Base class overrides

				#region OnKeyDown

			protected override void OnKeyDown(KeyEventArgs e)
			{
				// don't let the base Menu class process the up and down arrow keys since we want to do that in the menu tool
				switch (e.Key)
				{
					case Key.Up:
					case Key.Down:
						return;

					case Key.Escape:
						Debug.Assert(this.Items.Count == 1);

						if (this.Items.Count > 0)
						{
							ToolMenuItem menuItem = this.Items[0] as ToolMenuItem;

							Debug.Assert(menuItem != null);

							// if the menu item isn't open then don't call the base OnKeyDown which
							// will mark the key as being handled
							if (menuItem == null || menuItem.IsSubmenuOpen == false)
								return;
						}
						break;
				}

				base.OnKeyDown(e);
			}

				#endregion //OnKeyDown	
 
				#region OnIsKeyboardFocusWithinChanged
			/// <summary>
			/// Invoked when the focus within the menu has been changed.
			/// </summary>
			/// <param name="e">Provides information about the focus change</param>
			protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
			{
				base.OnIsKeyboardFocusWithinChanged(e);

				// AS 11/5/07
				if (true.Equals(e.NewValue) && MenuToolMenu._showKeyboardCuesProperty != null)
					this.SetValue(MenuToolMenu._showKeyboardCuesProperty, KnownBoxes.TrueBox);
			} 
				#endregion //OnIsKeyboardFocusWithinChanged

				#region OnPropertyChanged
			/// <summary>
			/// Invoked when a property on the element has been changed.
			/// </summary>
			/// <param name="e">Provides information about the property being changed.</param>
			protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
			{
				base.OnPropertyChanged(e);

				// AS 11/5/07
				// The ShowKeyboardCues dependency property is internal but we need it to show
				// 
				if (_showKeyboardCuesProperty == null && e.Property.Name == "ShowKeyboardCues")
					_showKeyboardCuesProperty = e.Property;
			} 
				#endregion //OnPropertyChanged

			#endregion //Base class overrides

			#region Properties

			// AS 11/5/07
			internal static DependencyProperty ShowKeyboardCuesProperty
			{
				get { return _showKeyboardCuesProperty; }
			} 

			#endregion //Properties

			#region Methods

				#region Internal Methods

				#endregion //Internal Methods

				#region Private Methods

					#region OnLostMouseCapture

			private static void OnLostMouseCapture(object sender, MouseEventArgs e)
			{
				MenuToolMenu menu = sender as MenuToolMenu;

				Debug.Assert(menu != null);

				if (menu != null)
					menu.Dispatcher.BeginInvoke(DispatcherPriority.Send, new XamRibbon.MethodInvoker(menu.VerifyCapture));
			}

					#endregion //OnLostMouseCapture

                    // AS 11/10/08 TFS6035
                    #region OnPreviewMouseDownOutsideCaptured
            private static void OnPreviewMouseDownOutsideCaptured(object sender, MouseButtonEventArgs e)
            {
                MenuToolMenu mtm = (MenuToolMenu)sender;

                // if the menu has capture and is within the ribbon then we 
                // may want to prevent the menu from shifting focus out of itself (and the ribbon)
                //
                if (Mouse.Captured == mtm 
                    &&  null != mtm._menuTool 
                    && mtm._menuTool.Location != ToolLocation.Unknown)
                {
                    DependencyObject rootVisual = mtm;

                    // we need the root visual in the tree so we can hittest so 
                    // see if the mouse is going to be pressed within that element
                    // if so then shift focus to the root visual
                    while (true)
                    {
                        DependencyObject parent = VisualTreeHelper.GetParent(rootVisual);

                        if (null == parent)
                            break;

                        // if this is within the ribbon (i.e. its not in a popup)
                        // then we don't want to interfere
                        if (parent is XamRibbon)
                            return;

                        rootVisual = parent;
                    }

                    UIElement rootElement = rootVisual as UIElement;
                    if (null != rootElement)
                    {
                        IInputElement element = rootElement.InputHitTest(e.GetPosition(rootElement));

                        // if the mouse is being pressed within the same root visual
                        // and not within the menu...
                        if (element is DependencyObject && 
                            false == Utilities.IsDescendantOf(mtm, (DependencyObject)element))
                        {
                            // if the menu has keyboard focus then just focus the
                            // root visual
                            if (mtm.IsKeyboardFocusWithin)
                                rootElement.Focus();
                        }
                    }
                }
            }
                    #endregion //OnPreviewMouseDownOutsideCaptured

                    #region VerifyCapture

            private void VerifyCapture()
            {
                MenuToolMenu menuWithCapture = Mouse.Captured as MenuToolMenu;

                if (menuWithCapture != this)
                    return;

                bool releaseCapture = true;

                if (this.Items.Count == 1)
                {
                    DependencyObject item = this.Items[0] as DependencyObject;

                    if (item != null)
                    {
                        MenuItem menuitem = this.ContainerFromElement(item) as MenuItem;

                        if (menuitem != null &&
                             menuitem.IsSubmenuOpen == true)
                            releaseCapture = false;
                    }
                }

                if (releaseCapture)
                {
                    DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;

                    // JJD 11/29/07 - BR28763
                    // If the keyboard focus is in an element nested within a menuitem set focus
                    // to the menuitem.
                    // Otherwise the MenuItem base logic throws a null ref exception when trying to close up
                    if (focusedElement != null)
                    {
                        ToolMenuItem tmi = focusedElement as ToolMenuItem;
                        if (tmi == null)
                            tmi = Utilities.GetAncestorFromType(focusedElement, typeof(ToolMenuItem), true, null, typeof(Popup)) as ToolMenuItem;

                        if (tmi != null && !(tmi is ApplicationMenuPresenter))
                            return;
                    }

                    this.ReleaseMouseCapture();
                }


            }

                    #endregion //VerifyCapture	
    
				#endregion //Private Methods	
        
			#endregion //Methods
		}

		#endregion //MenuToolMenu internal class	

		// AS 10/12/07 UseLargeImages
		#region UseLargeImagesToDoubleConverter
		internal class UseLargeImagesToDoubleConverter : IValueConverter
		{
			#region Members

			private static readonly object LargeSize = 32;
			private static readonly object SmallSize = 16;

			internal static readonly UseLargeImagesToDoubleConverter Instance;

			#endregion //Members

			#region Constructor
			private UseLargeImagesToDoubleConverter()
			{
			}

			static UseLargeImagesToDoubleConverter()
			{
				Instance = new UseLargeImagesToDoubleConverter();
			}
			#endregion //Constructor

			#region IValueConverter Members

			object IValueConverter.Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				if (targetType == typeof(double) && value is bool)
				{
					if (true == (bool)value)
						return UseLargeImagesToDoubleConverter.LargeSize;
				}

				return UseLargeImagesToDoubleConverter.SmallSize;
			}

			object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				return Binding.DoNothing;
			}

			#endregion
		} 
		#endregion //UseLargeImagesToDoubleConverter


		#region FilteringEnumerator private class

		internal class FilteringEnumerator : IEnumerator
		{
			private IEnumerator _sourceEnumerator;
			private ArrayList _filteredOutItems;
			private bool _endReached;
			private object _currentItem;

			static object UnsetObjectMarker = new object();

			internal FilteringEnumerator(IEnumerator sourceEnumerator, ICollection filteredOutItems)
			{
				if (sourceEnumerator == null)
					throw new ArgumentNullException("sourceEnumerator");

				if (filteredOutItems == null)
					this._filteredOutItems = new ArrayList(1);
				else
					this._filteredOutItems = new ArrayList(filteredOutItems);
				this._sourceEnumerator = sourceEnumerator;
				this._currentItem = UnsetObjectMarker;
			}

			#region IEnumerator Members

			public object Current
			{
				get
				{
					if (this._currentItem == UnsetObjectMarker)
					{
						if (this._endReached)
							throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidOperationException_4"));
						else
							throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidOperationException_3"));
					}

					return this._currentItem;
				}
			}

			public bool MoveNext()
			{
				if (this._endReached)
					throw new InvalidOperationException(XamRibbon.GetString("LE_InvalidOperationException_4"));

				if (!this._sourceEnumerator.MoveNext())
					return false;

				this._currentItem = this._sourceEnumerator.Current;

				if (this._filteredOutItems.Contains(this._currentItem))
					return this.MoveNext();

				return true;
			}

			public void Reset()
			{
				this._endReached = false;
				this._currentItem = UnsetObjectMarker;

				// reset the source enumerators
				this._sourceEnumerator.Reset();
			}

			#endregion
		}

		#endregion //FilteringEnumerator private class	
        
		#region MenuToolBaseProxy

		/// <summary>
		/// Derived <see cref="RibbonToolProxy"/> for <see cref="MenuToolBase"/> instances
		/// </summary>
		protected class MenuToolBaseProxy : RibbonToolProxy<MenuToolBase>
		{
			// AS 5/16/08 BR32980 - See the ToolProxyTests.NoInstanceVariablesOnProxies proxy for details.
			//[ThreadStatic()]
			internal static readonly MenuToolBaseProxy Instance = new MenuToolBaseProxy();

			#region Constructor
			static MenuToolBaseProxy()
			{
				// AS 6/10/08
				// See RibbonToolProxy<T>.Clone. Changed ignored property handling to registration mechanism.
				//
				RibbonToolProxy.RegisterPropertiesToIgnore(typeof(MenuToolBaseProxy), MenuTool.IsOpenProperty);
			} 
			#endregion //Constructor

			#region Bind

			/// <summary>
			/// Binds properties of the target tool to corresponding properties on the specified 
			/// source tool.  The specific properties that are bound are implementation details of the tool.  Generally, any property that 
			/// represents �tool state� and whose value is changeable and should be shared across instances of a given tool, should be bound 
			/// in tool�s implementation of this interface method.
			/// </summary>
			/// <param name="sourceTool">The tool that this tool is being bound to.</param>
			/// <param name="targetTool">The tool whose properties are being bound to the properties of <paramref name="sourceTool"/></param>
			protected override void Bind(MenuToolBase sourceTool, MenuToolBase targetTool)
			{
				base.Bind(sourceTool, targetTool);

				targetTool.ItemsSource = sourceTool.Items;
			}

			#endregion //Bind

			#region Clone

			
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)




			#endregion //Clone	
    
			#region PerformAction
			/// <summary>
			/// Performs a tool's default action.
			/// </summary>
			/// <param name="tool">The tool whose action should be performed.</param>
			/// <returns>A boolean indicating whether the action was performed.</returns>
			public override bool PerformAction(MenuToolBase tool)
			{
				// AS 10/15/07 BR27327
				// The menuitem class will not set its IsSubmenuOpen to true if its role is not submenu
				// header or top level header.
				//
				if (tool._presenter != null)
				{
					// AS 11/1/07 BR28040
					// The menu item may not be initialized as the owner of the menu's items and therefore
					// its items collection may not be initialized. We need to make sure it is before we
					// check the Role.
					//
					tool.SetReparentedToolsOwner(tool._presenter);

					switch (tool._presenter.Role)
					{
						case MenuItemRole.SubmenuItem:
						case MenuItemRole.TopLevelItem:
							return false;
					}
				}

				// AS 10/3/07 BR27037
				// We need to focus the menu tool before dropping it down. Otherwise it will focus itself
				// after it drops down which takes focus out of the contained Menu instance which causes it 
				// to leave "MenuMode" and close up.
				//
				if (base.PerformAction(tool))
				{
					// AS 12/19/07 BR29199
					//tool.IsOpen = true;
					tool.OpenMenu(PopupOpeningReason.KeyTips);

					// AS 10/10/07
					// When the menu is opened via a keytip or item navigation then we want to 
					// focus/activate the first item.
					if (tool.IsOpen)
					{
						MenuItem menuItem = tool._presenter ?? GetToolMenuItem(tool);

						if (null != menuItem)
							PopupOwnerProxy.FocusFirstItem(menuItem);
					}

					return tool.IsOpen;
				}

				return false;
			}
			#endregion //PerformAction

			#region PrepareToolMenuItem

			/// <summary>
			/// Prepares the container <see cref="ToolMenuItem"/>to 'host' the tool.
			/// </summary>
			/// <param name="toolMenuItem">The container that wraps the tool.</param>
			/// <param name="tool">The tool that is being wrapped.</param>
			protected override void PrepareToolMenuItem(ToolMenuItem toolMenuItem, MenuToolBase tool)
			{
				base.PrepareToolMenuItem(toolMenuItem, tool);

				toolMenuItem.ItemsSource = tool.Items;

				toolMenuItem.SetBinding(ToolMenuItem.IsSubmenuOpenProperty, Utilities.CreateBindingObject(MenuToolBase.IsOpenProperty, BindingMode.TwoWay, tool));

				// JM 10-15-08 [BR35158 - TFS6448]
				toolMenuItem.SetBinding(ItemsControl.ItemTemplateProperty, Utilities.CreateBindingObject(ItemsControl.ItemTemplateProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ItemsControl.ItemTemplateSelectorProperty, Utilities.CreateBindingObject(ItemsControl.ItemTemplateSelectorProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ItemsControl.ItemContainerStyleProperty, Utilities.CreateBindingObject(ItemsControl.ItemContainerStyleProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ItemsControl.ItemContainerStyleSelectorProperty, Utilities.CreateBindingObject(ItemsControl.ItemContainerStyleSelectorProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ItemsControl.DisplayMemberPathProperty, Utilities.CreateBindingObject(ItemsControl.DisplayMemberPathProperty, BindingMode.OneWay, tool));
				toolMenuItem.SetBinding(ItemsControl.ItemsPanelProperty, Utilities.CreateBindingObject(ItemsControl.ItemsPanelProperty, BindingMode.OneWay, tool));
				if (MenuToolBase.ItemsControlItemStringFormatProperty != null)
					toolMenuItem.SetBinding(MenuToolBase.ItemsControlItemStringFormatProperty, Utilities.CreateBindingObject("ItemStringFormat", BindingMode.OneWay, tool));
			}

			#endregion //PrepareToolMenuItem	
    
			#region RaiseToolEvent

			/// <summary>
			/// Called by the <b>Ribbon</b> to raise one of the common tool events. 
			/// </summary>
			/// <remarks>
			/// <para class="body">This method will be called to raise a commmon tool event, e.g. <see cref="MenuToolBase.Cloned"/>, <see cref="MenuToolBase.CloneDiscarded"/>.</para>
			/// <para class="note"><b>Note:</b> the implementation of this method calls a protected virtual method named <see cref="MenuToolBase.OnRaiseToolEvent"/> that simply calls the RaiseEvent method. This allows derived classes the opportunity of adding custom logic.</para>
			/// </remarks>
			/// <param name="sourceTool">The tool for which the event should be raised.</param>
			/// <param name="args">The event arguments</param>
			/// <seealso cref="XamRibbon"/>
			/// <seealso cref="ToolClonedEventArgs"/>
			/// <seealso cref="ToolCloneDiscardedEventArgs"/>
			protected override void RaiseToolEvent(MenuToolBase sourceTool, RoutedEventArgs args)
			{
				ToolCloneDiscardedEventArgs cloneDiscardeArgs = args as ToolCloneDiscardedEventArgs;

				if (cloneDiscardeArgs != null)
				{
					MenuToolBase clonedMenu = cloneDiscardeArgs.ClonedTool as MenuToolBase;

					Debug.Assert(clonedMenu != null);

					// JJD 10/16/07
					// If the tool being discarded is the current owner of the reparented tools
					// we need to transfer ownership to the root menu tool
					if (clonedMenu != null)
					{
						if (clonedMenu.GetReparentedToolsOwner() == MenuToolBase.GetToolMenuItem(clonedMenu))
						{
							MenuToolBase rootMenu = RibbonToolProxy.GetRootSourceTool(clonedMenu) as MenuToolBase;

							Debug.Assert(rootMenu != null);

							if (rootMenu != null && MenuToolBase.GetToolMenuItem(rootMenu) != null)
								clonedMenu.SetReparentedToolsOwner(MenuToolBase.GetToolMenuItem(rootMenu));
						}

						clonedMenu.ClearValue(MenuToolBase.ToolMenuItemProperty);
					}
				}

				sourceTool.OnRaiseToolEvent(args);
			}

			#endregion //RaiseToolEvent
		}
		#endregion //MenuToolBaseProxy

		#region IKeyTipContainer Members

		void IKeyTipContainer.Deactivate()
		{
			this.IsOpen = false;
		}

		IKeyTipProvider[] IKeyTipContainer.GetKeyTipProviders()
		{
			return this.GetKeyTipProviders();
		}

		bool IKeyTipContainer.ReuseParentKeyTips
		{
			get { return false; }
		}

		#endregion //IKeyTipContainer

		#region IRibbonPopupOwner Members

		XamRibbon IRibbonPopupOwner.Ribbon
		{
			get { return XamRibbon.GetRibbon(this); }
		}

		bool IRibbonPopupOwner.CanOpen
		{
			get { return this._menu != null && this._presenter != null; }
		}

		bool IRibbonPopupOwner.IsOpen
		{
			get
			{
				return this.IsOpen;
			}
			set
			{
				this.IsOpen = value;
			}
		}

		bool IRibbonPopupOwner.FocusFirstItem()
		{
			return PopupOwnerProxy.FocusFirstItem(this._presenter);
		}

		UIElement IRibbonPopupOwner.PopupTemplatedParent
		{
			get { return this._presenter; }
		}

		bool IRibbonPopupOwner.HookKeyDown
		{
			get { return false; }
		}

		// AS 10/18/07
		FrameworkElement IRibbonPopupOwner.ParentElementToFocus 
		{
			get { return this._presenter; }
		}

		#endregion
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