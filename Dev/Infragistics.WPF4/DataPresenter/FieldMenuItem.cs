using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// Custom menu item for use with a <see cref="FieldMenuDataItem"/>
	/// </summary>
	[DesignTimeVisible(false)]
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	public class FieldMenuItem : MenuItem
	{
		#region Member Variables

		internal static DependencyProperty _showKeyboardCuesProperty = null;
		private bool _isShiftingFocusPrevious;

		#endregion //Member Variables

		#region Constructor
		static FieldMenuItem()
		{
			FieldMenuItem.DefaultStyleKeyProperty.OverrideMetadata(typeof(FieldMenuItem), new FrameworkPropertyMetadata(typeof(FieldMenuItem)));

			// We need to set the ShowKeyboardCues property but its currently internal so we need to try and use reflection to get to 
			// it. If we can't get to it, we'll try to pick it up later if the Alt key is pressed, etc.
			//
			try
			{
				System.Reflection.BindingFlags bindingFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
				System.Reflection.FieldInfo field = typeof(KeyboardNavigation).GetField("ShowKeyboardCuesProperty", bindingFlags);

				if (null != field)
					_showKeyboardCuesProperty = field.GetValue(null) as DependencyProperty;
			}
			catch
			{
			}
		}

		/// <summary>
		/// Initializes a new <see cref="FieldMenuItem"/>
		/// </summary>
		public FieldMenuItem()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetContainerForItemOverride
		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <returns>A <see cref="FieldMenuItem"/> instance</returns>
		protected override DependencyObject GetContainerForItemOverride()
		{
			return new FieldMenuItem();
		} 
		#endregion //GetContainerForItemOverride

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="FieldMenuItem"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.FieldMenuItemAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new Infragistics.Windows.Automation.Peers.DataPresenter.FieldMenuItemAutomationPeer(this);
		}
		#endregion //OnCreateAutomationPeer

		#region OnKeyDown
		/// <summary>
		/// Invoked when a key is pressed while the element contains the keyboard focus.
		/// </summary>
		/// <param name="e">Provides information about the key event</param>
		protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			var modifiers = Keyboard.Modifiers;
			var key = e.Key;

			if (key == System.Windows.Input.Key.System)
				key = e.SystemKey;

			// normally pressing the Alt key will cause the mainmenu to open/focus
			// but in the case of the excel filter menu we want this menu to 
			// stay open so we need to handle that key
			FieldMenuDataItem dataItem = this.DataContext as FieldMenuDataItem;

			if (dataItem != null && dataItem.IsControlHost)
			{
				// AS 8/19/11 TFS84468
				// Moved this out of the RecordFilterTreeControl.
				//
				if (key == System.Windows.Input.Key.LeftAlt ||
					key == System.Windows.Input.Key.RightAlt)
				{
					e.Handled = true;
				}
				else if (key == Key.Down || key == Key.Up)
				{
					// AS 8/19/11 TFS84468
					// if an up/down reaches the menu item then we'll do what excel does which is 
					// to treat it as a next/previous focus shift
					FrameworkElement fe = e.OriginalSource as FrameworkElement ?? Utilities.GetAncestorFromType(e.OriginalSource as DependencyObject, typeof(FrameworkElement), true) as FrameworkElement;

					if (fe != null && fe.MoveFocus(new TraversalRequest(key == Key.Down ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous)))
					{
						e.Handled = true;

						// note we need to return because menuitem doesn't look at the handled state
						return;
					}
				}
			}

			if (!e.Handled)
			{
				// AS 8/19/11
				// When you just dropdown the filter menu and press tab, focus is shifting from the root 
				// menu item to outside the menu item. It should go to the first focusable item.
				//
				if (this.IsKeyboardFocused && key == Key.Tab && this.Role == MenuItemRole.TopLevelHeader && this.IsSubmenuOpen)
				{
					var generator = this.ItemContainerGenerator;

					for (int i = 0, count = this.Items.Count; i < count; i++)
					{
						MenuItem mi = generator.ContainerFromIndex(i) as MenuItem;

						if (mi != null && mi.IsEnabled && mi.Focusable)
						{
							mi.Focus();
							e.Handled = true;
							return;
						}
					}
				}
			}

			base.OnKeyDown(e);
		}
		#endregion //OnKeyDown

		#region OnPreviewGotKeyboardFocus
		/// <summary>
		/// Invoked when the menu item is about to receive keyboard focus.
		/// </summary>
		/// <param name="e">Provides information about the focus change</param>
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			// AS 8/19/11 TFS84468
			// When hosting a control that we want to give focus to we will either shift focus into 
			// that control or move to the previous item depending on where focus is coming from.
			//
			//
			if (e.NewFocus == this)
			{
				FieldMenuDataItem dataItem = this.DataContext as FieldMenuDataItem;

				if (dataItem != null && dataItem.IsControlHost)
				{
					// if focus is shifting from within the menu item to the menu item
					// then push focus along to the previous menu item
					if (e.OldFocus is DependencyObject && this.IsAncestorOf(e.OldFocus as DependencyObject))
					{
						// don't do anything if we try to focus ourselves in a recursive call
						if (_isShiftingFocusPrevious)
						{
							// prevent the focus shift
							e.Handled = true;
							return;
						}

						// something within is giving us focus. could be because of a shift-tab, the element is collapsed, etc.

						var dependencyObject = e.OldFocus as DependencyObject;
						var feOldFocus = dependencyObject as FrameworkElement ?? Utilities.GetAncestorFromType(dependencyObject, typeof(FrameworkElement), true) as FrameworkElement;

						_isShiftingFocusPrevious = true;

						try
						{
							// if the item was collapsed...
							if (feOldFocus.IsVisible == false)
							{
								// we'll try to shift focus to the previous element within us. this could try to focus 
								// us so we need to know that
								e.Handled = feOldFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

								// if it tried to focus us then we would have marked it handled in which case focus 
								// would not have moved so now we'll try to move to an element after it
								if (!e.Handled)
									e.Handled = feOldFocus.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
							}
							else
							{
								// if the item is visible then its probably a keyboard navigation so just move 
								// to the element before us. note since it is possible there is only 1 enabled 
								// item, we will also use the flag above in case it wraps around to us
								e.Handled = this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));

								// if we couldn't shift focus there then we'll try going back within the menu item
								// back starting from the last element
								if (!e.Handled && this.VisualChildrenCount > 0)
								{
									FrameworkElement fe = this.GetVisualChild(0) as FrameworkElement;

									if (null != fe)
									{
										e.Handled = fe.MoveFocus(new TraversalRequest(FocusNavigationDirection.Last));
									}
								}
							}
						}
						finally
						{
							_isShiftingFocusPrevious = false;
						}

					}
					else if (this.VisualChildrenCount > 0)
					{
						// if the parent is processing keyboard navigation and trying to activate 
						// this menu item then we'll shift focus into the contained control
						FrameworkElement fe = this.GetVisualChild(0) as FrameworkElement;

						if (null != fe)
						{
							e.Handled = fe.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
						}
					}
				}
			}

			base.OnPreviewGotKeyboardFocus(e);
		} 
		#endregion //OnPreviewGotKeyboardFocus

		#region OnPreviewLostKeyboardFocus
		/// <summary>
		/// Invoked when the element is about to lose keyboard focus.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnPreviewLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			FieldMenuDataItem dataItem = this.DataContext as FieldMenuDataItem;

			// AS 8/19/11 TFS84468
			// Moved this out of the RecordFilterTreeControl.
			//
			if (dataItem != null && dataItem.IsControlHost)
			{
				if (e.NewFocus is MenuItem)
				{
					var miGettingFocus = e.NewFocus as MenuItem;
					var miLosingFocus = this;

					// when the mouse is moved out of the menu item, it will try to give focus 
					// to the parent menu item. assuming the parent is still open we want to 
					// prevent that
					if (miLosingFocus != null &&
						miGettingFocus.IsSubmenuOpen &&
						miLosingFocus != miGettingFocus &&
						Utilities.IsDescendantOf(miGettingFocus, miLosingFocus))
					{
						if (ItemsControl.ItemsControlFromItemContainer(miLosingFocus) == miGettingFocus)
							e.Handled = true;
					}
				}
			}

			base.OnPreviewLostKeyboardFocus(e);
		}
		#endregion //OnPreviewLostKeyboardFocus

		#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property has been changed
		/// </summary>
		/// <param name="e">Provides information about the property that was changed</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			// the ShowKeyboardCues is internal but we want to force it to true when in the 
			// filterbutton so we'll watch for that property in case we didn't have the rights 
			// to get it in the static ctor
			if (_showKeyboardCuesProperty == null && e.Property.OwnerType == typeof(KeyboardNavigation) && e.Property.Name == "ShowKeyboardCues")
				_showKeyboardCuesProperty = e.Property;

			base.OnPropertyChanged(e);
		} 
		#endregion //OnPropertyChanged

		#region PrepareContainerForItemOverride
		/// <summary>
		/// Used to initialize the container for child menu items.
		/// </summary>
		/// <param name="element">The container for the item being prepared</param>
		/// <param name="item">The dataitem for which the container is being prepared.</param>
		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			// ShowKeyboardCues is inherited. at least in the case where this is within a FilterButton
			// we always want to show the mnemonics so we need to explicitly set it to true so it 
			// doesn't inherit the ancestors value
			if (_showKeyboardCuesProperty != null)
			{
				FieldMenuItem menuItem = this;

				while (menuItem.TemplatedParent is FilterButton == false)
				{
					FieldMenuItem parentMenuItem = ItemsControl.ItemsControlFromItemContainer(menuItem) as FieldMenuItem;

					if (parentMenuItem == null)
						break;

					menuItem = parentMenuItem;
				}

				if (menuItem.TemplatedParent is FilterButton)
					element.SetValue(_showKeyboardCuesProperty, KnownBoxes.TrueBox);
			}

			base.PrepareContainerForItemOverride(element, item);
		} 
		#endregion //PrepareContainerForItemOverride

		#endregion //Base class overrides
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