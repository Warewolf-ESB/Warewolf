using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.ComponentModel;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using Infragistics.Windows.Ribbon.Internal;

namespace Infragistics.Windows.Ribbon
{

	/// <summary>
	/// Represents the ApplicationMenu button on a ribbon.
	/// </summary>
	/// <seealso cref="ApplicationMenu"/>
	/// <seealso cref="ApplicationMenu.CreateMenuToolPresenter"/>
	/// <seealso cref="XamRibbon"/>
	[TemplatePart(Name = "PART_FooterToolbar", Type = typeof(ContentPresenter))]
	[TemplatePart(Name = "PART_RecentItems", Type = typeof(ContentPresenter))]
	[TemplatePart(Name = "PART_SubMenuArea", Type = typeof(FrameworkElement))]
	// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
	[TemplatePart(Name = "PART_ApplicationMenuItemsPanel", Type = typeof(ApplicationMenuItemsPanel))]
	[TemplatePart(Name = "PART_RecentItemsPanel", Type = typeof(ApplicationMenuRecentItemsPanel))]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class ApplicationMenuPresenter : ToolMenuItem
	{
		#region Member Variables

		// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
		private ApplicationMenuItemsPanel _menuItemsPanel;
		private ApplicationMenuRecentItemsPanel _recentItemsPanel;

		#endregion //Member Variables

		#region Constructors

		static ApplicationMenuPresenter()
		{
			//This OverrideMetadata call tells the system that this element wants to provide a style that is different than its base class.
			//This style is defined in themes\generic.xaml
			DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationMenuPresenter), new FrameworkPropertyMetadata(typeof(ApplicationMenuPresenter)));

			// AS 10/9/07
			// This defaults to None for menu items but we need to be able to arrow into/out of the menu item.
			//
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ApplicationMenuPresenter), new FrameworkPropertyMetadata(KeyboardNavigationMode.Continue));

			// AS 10/9/07
			// Put focus in the menu tool presenter instead of the menu tool.
			//
			//FrameworkElement.FocusableProperty.OverrideMetadata(typeof(ApplicationMenuPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			// AS 6/3/08 BR32772
			// Several overriden methods on MenuItem have been decorated with a uipermission attribute
			// so we cannot override them and still be able to run in an xbap. Register a separate handler
			// instead.
			//
			EventManager.RegisterClassHandler(typeof(ApplicationMenuPresenter), Keyboard.KeyDownEvent, new KeyEventHandler(OnClassKeyDown));

			// AS 9/17/09 TFS20559
			EventManager.RegisterClassHandler(typeof(ApplicationMenuPresenter), AccessKeyManager.AccessKeyPressedEvent, new AccessKeyPressedEventHandler(OnAccessKeyPressed));
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ApplicationMenuPresenter"/>
		/// </summary>
		public ApplicationMenuPresenter()
		{
		}

		#endregion //Constructors

		#region Base Class Overrides

			#region OnApplyTemplate

		/// <summary>
		/// Called when the template is applied
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 11/19/07 ApplicationMenuRecentItem to ToolMenuItem
			if (this._menuItemsPanel != null)
				this._menuItemsPanel.Menu = null;

			this._menuItemsPanel = this.GetTemplateChild("PART_ApplicationMenuItemsPanel") as ApplicationMenuItemsPanel;

			if (this._menuItemsPanel != null)
				this._menuItemsPanel.Menu = this;

			this._recentItemsPanel = this.GetTemplateChild("PART_RecentItemsPanel") as ApplicationMenuRecentItemsPanel;
		}

			#endregion //OnApplyTemplate	

			// AS 11/29/07 BR28761
			#region OnChildKeyDown
		internal override void OnChildKeyDown(ToolMenuItem childMenuItem, KeyEventArgs e)
		{
			// AS 12/18/07 BR29182
			// The right arrow should be allowed to open a submenu if its 
			if (childMenuItem.Role == MenuItemRole.SubmenuHeader && e.Key == Key.Right)
				return;

			// AS 11/29/07 BR28761
			ApplicationMenuPresenter.ProcessDirectionalNavigationKey(this, e);

			base.OnChildKeyDown(childMenuItem, e);
		}
			#endregion //OnChildKeyDown

			// AS 6/29/11 TFS80311
			// The MenuItem's default automation peer is getting/creating peers for each item. As part of that 
			// the Parent of the peer is getting set to that peer but that peer isn't in the automation tree. 
			// Really the peer of the menu item should never be involved here since we're not including it 
			// in any automation tree.
			//
			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns null since the elements are exposed as child elements of the popup within the application menu.
		/// </summary>
		/// <returns>Returns null</returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return null;
		}
			#endregion //OnCreateAutomationPeer

			#region OnKeyDown

		
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

			#endregion //OnKeyDown	

		#endregion //Base Class Overrides

		#region Properties

			#region Public Properties

				#region HasFooterToolbar

		internal static readonly DependencyPropertyKey HasFooterToolbarPropertyKey =
			DependencyProperty.RegisterReadOnly("HasFooterToolbar",
			typeof(bool), typeof(ApplicationMenuPresenter), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="HasFooterToolbar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasFooterToolbarProperty =
			HasFooterToolbarPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the <see cref="ApplicationMenu.FooterToolbar"/> property has been set to an instance of <see cref="ApplicationMenuFooterToolbar"/> (read-only).
		/// </summary>
		/// <seealso cref="HasFooterToolbarProperty"/>
		//[Description("Returns true if the FooterToolbar property has been set to an instance of ApplicationMenuFooterToolbar (read-only).")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		public bool HasFooterToolbar
		{
			get
			{
				return (bool)this.GetValue(ApplicationMenuPresenter.HasFooterToolbarProperty);
			}
		}

				#endregion //HasFooterToolbar

				#region HasRecentItems
		
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

				#endregion //HasRecentItems

			#endregion //Public Properties

		#region Internal Properties

		#region MenuItemsPanel
		internal ApplicationMenuItemsPanel MenuItemsPanel
		{
			get { return this._menuItemsPanel; }
		} 
				#endregion //MenuItemsPanel

				#region RecentItemsPanel
		internal ApplicationMenuRecentItemsPanel RecentItemsPanel
		{
			get { return this._recentItemsPanel; }
		} 
				#endregion //RecentItemsPanel

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ProcessDirectionalNavigationKey

		internal static void ProcessDirectionalNavigationKey(FrameworkElement source, KeyEventArgs e)
		{
			#region Commented Out
			
#region Infragistics Source Cleanup (Region)
























































































































































































































































































































































#endregion // Infragistics Source Cleanup (Region)


			#endregion //Commented Out

			XamRibbon ribbon = XamRibbon.GetRibbon(source);

			Debug.Assert(ribbon != null);

			ApplicationMenu appMenu = null;

			if (ribbon != null)
				appMenu = ribbon.ApplicationMenu;

			// JJD 10/11/07
			// We want to process keys the directional navigation keys so we can go between the
			// main menu items, the recent items and the footer toolbar if the presenter is open
			//
			if (appMenu == null || false == appMenu.IsOpen)
				return;

			// AS 5/5/10 TFS30526
			// Moved to helper method
			//
			//FocusNavigationDirection direction;
			//
			//switch (e.Key)
			//{
			//    case Key.Up:
			//        direction = FocusNavigationDirection.Up;
			//        break;
			//    case Key.Down:
			//        direction = FocusNavigationDirection.Down;
			//        break;
			//    case Key.Left:
			//        direction = FocusNavigationDirection.Left;
			//        break;
			//    case Key.Right:
			//        direction = FocusNavigationDirection.Right;
			//        break;
			//    case Key.Tab:
			//        // AS 12/4/07 BR28887
			//        // can't predict focus but we still need to handle it because the highlight
			//        // might get messed up in the process
			//        direction = (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0 ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous;
			//        break;

			//    default:
			//        return;
			//}
			FocusNavigationDirection? direction = ToolMenuItem.GetDirection(e);

			if (direction == null)
				return;

			IInputElement focusedItem = Keyboard.FocusedElement;
			FrameworkElement focusedElement = focusedItem as FrameworkElement;

			#region ApplicationMenuPresenter has focus - Focus First Item
			if (focusedElement == appMenu.Presenter)
			{
				PopupOwnerProxy.FocusFirstItem(appMenu.Presenter);
				e.Handled = true;
				return;
			} 
			#endregion //ApplicationMenuPresenter has focus - Focus First Item

			// AS 12/4/07 BR28887
			// We need to handle the tab key because when you tab between a menu item and a tool
			// in the footer toolbar, we need to clear the highlight of the menu tool.
			//
			if (e.Key == Key.Tab)
			{
				focusedElement.MoveFocus(new TraversalRequest(direction.Value));
				e.Handled = true;

				// if focus was in a menu item and focus moved into the application menu footer then 
				// deactivate the last menu item
				if (focusedElement is ToolMenuItem && Keyboard.FocusedElement is DependencyObject && GetAppMenuAncestor(Keyboard.FocusedElement as DependencyObject) is ApplicationMenuFooterToolbar)
					((ToolMenuItem)focusedElement).SimulateMouseLeave();

				return;
			}

			DependencyObject focusedItemPredicted = focusedElement.PredictFocus(direction.Value);
			FrameworkElement focusedItemPredictedElement = focusedItemPredicted as FrameworkElement;

			if (focusedItemPredictedElement != null)
			{
				DependencyObject newAncestor = GetAppMenuAncestor(focusedItemPredictedElement);
				DependencyObject oldAncestor = GetAppMenuAncestor(focusedElement);

				ApplicationMenuPresenter appMenuPresenter = appMenu.Presenter as ApplicationMenuPresenter;

				#region BR28761
				
#region Infragistics Source Cleanup (Region)





































#endregion // Infragistics Source Cleanup (Region)

				#endregion //BR28761

				bool isUpDownNavigation = direction == FocusNavigationDirection.Up || direction == FocusNavigationDirection.Down;
				bool navigateAcrossAncestor = oldAncestor != newAncestor;

				// we need to special case the up/down logic since it doesn't just move up/down but
				// acts more like next/previous
				if (isUpDownNavigation)
				{
					// up/down within the app menu footer will likely be to try and move out 
					// so we'll need to process these as next/previous type action
					if (oldAncestor is ApplicationMenuFooterToolbar)
					{
						#region Footer Toolbar
						if (focusedElement == focusedItemPredictedElement || navigateAcrossAncestor)
						{
							if (direction == FocusNavigationDirection.Up)
								focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
							else
								focusedElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

							// we don't need to do the navigation below
							navigateAcrossAncestor = false;
						} 
						#endregion //Footer Toolbar
					}
					else
					{
						#region IntraList Navigation
						if (false == navigateAcrossAncestor)
						{
							// AS 5/5/10 TFS30526
							// If we're navigating within one area and the prediction is to 
							// give focus to an element within a menu item then first try to 
							// focus the menu item itself.
							//
							DependencyObject target = ToolMenuItem.GetElementToFocus(focusedItemPredictedElement);

							if (target is FrameworkElement)
								focusedItemPredictedElement = target as FrameworkElement;


							// if we're seemingly navigating within th same parent then
							// make sure we're not at the cusp and wrapping around. in which 
							// case we want to treat this as a cross ancestor navigation
							// AS 12/6/07 BR28984
							// Compare points relative to the app menu.
							//
							//Rect newRect = LayoutInformation.GetLayoutSlot(focusedItemPredictedElement);
							//Rect oldRect = LayoutInformation.GetLayoutSlot(focusedElement);
							Rect newRect = new Rect(Utilities.PointToScreenSafe(focusedItemPredictedElement, new Point()), focusedItemPredictedElement.RenderSize);
							Rect oldRect = new Rect(Utilities.PointToScreenSafe(focusedElement, new Point()), focusedElement.RenderSize);

							if (direction == FocusNavigationDirection.Up)
							{
								// AS 5/5/10 TFS30526
								// We need to take the fact the element getting focus may contain the previous element with focus.
								//
								//if (newRect.Top > oldRect.Top)
								if (newRect.Bottom > oldRect.Bottom && !focusedItemPredictedElement.IsAncestorOf(focusedElement))
									navigateAcrossAncestor = true;
							}
							else
							{
								// AS 5/5/10 TFS30526
								// We need to take the fact the element getting focus may contain the previous element with focus.
								//
								//if (newRect.Top < oldRect.Top)
								if (newRect.Top < oldRect.Top && !focusedItemPredictedElement.IsAncestorOf(focusedElement))
									navigateAcrossAncestor = true;
							}
						} 
						#endregion //IntraList Navigation

						// ok if we've decided that we need to move from one location/ancestor to another...
						if (navigateAcrossAncestor)
						{
							#region Cross List Navigation
							// we'll just use an array and iterate through it finding either the 
							// first or last focusable item depending on the order
							bool isUp = direction == FocusNavigationDirection.Up;
							DependencyObject[] ancestors = new DependencyObject[] { appMenuPresenter.MenuItemsPanel, appMenuPresenter.RecentItemsPanel, appMenu.FooterToolbar };
							int start = Array.IndexOf<DependencyObject>(ancestors, oldAncestor) + 3;
							int end = isUp ? start - 4 : start + 4; // offset by 4 so we consider the original ancestor as well in case we have to wrap
							int offset = isUp ? -1 : +1;

							for (int i = start + offset; i != end; i += offset)
							{
								DependencyObject ancestor = ancestors[i % 3];

								if (null != ancestor)
								{
									bool success = isUp
										? PopupOwnerProxy.FocusLastItem(ancestor)
										: PopupOwnerProxy.FocusFirstItem(ancestor);

									if (success)
										break;
								}
							} 
							#endregion //Cross List Navigation
						}
						else
						{
							focusedItemPredictedElement.Focus();

							// AS 1/11/12 TFS30990
							// When processing the OnMouseEnter, the MenuItem will ask the parent 
							// menu item if it should ignore mouse events. Well in CLR4, the MenuItem 
							// class will set this on its parent menu item whenever it receives a 
							// key down for a key it doesn't process (i.e. something other than 
							// navigation keys).
							//
							ToolMenuItem predictedMi = focusedItemPredictedElement as ToolMenuItem;

							if (null != predictedMi)
							{
								var ke = new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.OemTilde);
								predictedMi.RaiseMenuItemOnKeyDown(ke);
							}
						}
					}

					DependencyObject newFocusedElement = Keyboard.FocusedElement as DependencyObject;

					// if focus was in a menu item and focus moved into the application menu footer then 
					// deactivate the last menu item
					if ( focusedElement is ToolMenuItem && newFocusedElement != null && GetAppMenuAncestor(newFocusedElement) is ApplicationMenuFooterToolbar )
						((ToolMenuItem)focusedElement).SimulateMouseLeave();
				}
				else
				{
					focusedItemPredictedElement.Focus();
				}

				if (focusedElement != Keyboard.FocusedElement)
				{
					// AS 3/25/10 TFS27670
					// The IsSelected of the previous menu item is not being cleared for the items 
					// within the recent panels as well as when navigating from the app menu to 
					// the recent items. That was causing all the menu items to look highlighted/selected 
					// when using the arrow keys to navigate between them.
					//
					bool clearIsSelected = false;
					DependencyObject newFocused = Keyboard.FocusedElement as DependencyObject;

					if (focusedElement is ToolMenuItem && Selector.GetIsSelected(focusedElement))
					{
						ToolLocation oldLocation = XamRibbon.GetLocation(focusedElement);

						// when navigating away from an item in the recent items then clear
						// its selected
						if (oldLocation == ToolLocation.ApplicationMenuRecentItems)
							clearIsSelected = true;
						else if (oldLocation == ToolLocation.ApplicationMenu)
						{
							if (newFocused != null && XamRibbon.GetLocation(newFocused) != ToolLocation.ApplicationMenu)
								clearIsSelected = true;
						}
					}

					if (clearIsSelected)
						Selector.SetIsSelected(focusedElement, false);


					e.Handled = true;
				}

				return;
			}
		}

				#endregion //ProcessDirectionalNavigationKey

			#endregion //Internal Methods	
        
			#region Private Methods

				// AS 11/20/07 ApplicationMenuRecentItem to ToolMenuItem
				#region GetAppMenuAncestor
		private static DependencyObject GetAppMenuAncestor(DependencyObject descendant)
		{
			while (descendant != null)
			{
				if (descendant is ApplicationMenuFooterToolbar ||
					descendant is ApplicationMenuRecentItemsPanel ||
					descendant is ApplicationMenuItemsPanel)
					return descendant;

				descendant = Utilities.GetParent(descendant, true);
			}

			return null;
		} 
				#endregion //GetAppMenuAncestor

				#region GetClosestLeftRightElement
		
#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)

				#endregion //GetClosestLeftRightElement

				// AS 9/17/09 TFS20559
				#region OnAccessKeyPressed
		private static void OnAccessKeyPressed(object sender, AccessKeyPressedEventArgs e)
		{
			// a menu item will mark itself as the scope of an access key press if the target 
			// is a menu item and the scope is null. well in the case of the application menu, 
			// the target could be a tool in the application menu footer so we also want to mark 
			// the applicationmenupresenter as the scope in that case
			ApplicationMenuPresenter amp = (ApplicationMenuPresenter)sender;
			ApplicationMenu am = amp.RibbonTool as ApplicationMenu;

			if (e.Target != null && null != am)
			{
				if (am.FooterToolbar == null)
					return;

				DependencyObject d = e.Target as DependencyObject;

				while (d is ContentElement)
					d = LogicalTreeHelper.GetParent(d);

				if (d != null && Utilities.IsDescendantOf(am.FooterToolbar, d, false))
				{
					e.Scope = amp;
					e.Handled = true;
				}
			}
		}
				#endregion //OnAccessKeyPressed

				// AS 6/3/08 BR32772
				#region OnClassKeyDown
		private static void OnClassKeyDown(object sender, KeyEventArgs e)
		{
			// JJD 10/19/07
			// Call the helper method to handle directional navigation keys
			ApplicationMenuPresenter.ProcessDirectionalNavigationKey(sender as ApplicationMenuPresenter, e);
		} 
				#endregion //OnClassKeyDown

			#endregion //Private Methods	
    
		#endregion //Methods	
        
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