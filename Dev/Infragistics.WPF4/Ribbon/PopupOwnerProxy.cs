using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using System.Windows.Threading;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Helper class for "root" popups - e.g. RibbonGroup in Qat, Collapsed Ribbon Group, Qat Overflow, MenuTools on the 
	/// Ribbon or QAT and the minimized tab control. The purpose of the class is to centralize the following behaviors:
	/// * Shift focus into the children when a navigation key is pressed
	/// * Close up when the escape or alt key is pressed
	/// * Handle when to shift focus out of the ribbon if the ribbon was in Normal mode
	/// </summary>
	internal class PopupOwnerProxy : DependencyObject
	{
		#region Member Variables

		private IRibbonPopupOwner	_popupOwner;
		private FrameworkElement	_popupTarget;
		private bool				_restoreDocumentFocusOnClose;
		private Popup				_hookedPopup;

		// AS 12/19/07 BR29199
		private PopupOpeningReason	_openingReason;

		#endregion //Member Variables

		#region Constructor
		internal PopupOwnerProxy(IRibbonPopupOwner popupOwner)
		{
			if (null == popupOwner)
				throw new ArgumentNullException("popupOwner");

			this._popupOwner = popupOwner;
		} 
		#endregion //Constructor

		#region Properties

		#region Private Properties

		private Popup HookedPopup
		{
			get { return this._hookedPopup; }
			set
			{
				if (this._hookedPopup != value)
				{
					if (this._hookedPopup != null)
						this._hookedPopup.KeyDown -= new KeyEventHandler(this.OnPopupKeyDown);

					this._hookedPopup = value;

					if (this._hookedPopup != null)
					{
						this._hookedPopup.KeyDown += new KeyEventHandler(this.OnPopupKeyDown);
						BindingOperations.SetBinding(this, IsKeyboardFocusWithinPopupProperty, Utilities.CreateBindingObject(UIElement.IsKeyboardFocusWithinProperty, BindingMode.OneWay, this._hookedPopup));
					}
					else
						BindingOperations.ClearBinding(this, IsKeyboardFocusWithinPopupProperty);
				}
			}
		}

		#region IsKeyboardFocusWithin

		private static readonly DependencyProperty IsKeyboardFocusWithinProperty = DependencyProperty.Register("IsKeyboardFocusWithin",
			typeof(bool), typeof(PopupOwnerProxy), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsFocusWithinTargetChanged)));

		private static readonly DependencyProperty IsKeyboardFocusWithinPopupProperty = DependencyProperty.Register("IsKeyboardFocusWithinPopup",
			typeof(bool), typeof(PopupOwnerProxy), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsFocusWithinTargetChanged)));

		private static void OnIsFocusWithinTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PopupOwnerProxy proxy = (PopupOwnerProxy)d;

			// AS 10/3/07 BR27027
			// If focus leaves the dropdown then close it up.
			//
			if (false.Equals(e.NewValue) && proxy.IsOpen)
			{
				bool isFocusInTarget = proxy._popupTarget != null && proxy._popupTarget.IsKeyboardFocusWithin;
				bool isFocusInPopup = proxy._hookedPopup != null && proxy._hookedPopup.IsKeyboardFocusWithin;

				if (false == isFocusInTarget && false == isFocusInPopup)
				{
					// AS 10/9/07
					// See if the focus is within the popup.
					//
					// AS 10/12/07
					// We want to look at the popup of the focused element even if we have a hooked popup.
					// Without this, right clicking on the menu portion of a menu within the qat overflow
					// was causing the overflow to close up because it had a hooked popup.
					//
					//Popup popup = Keyboard.FocusedElement is DependencyObject && proxy._hookedPopup == null
					Popup popup = Keyboard.FocusedElement is DependencyObject
						? Utilities.GetAncestorFromType(Keyboard.FocusedElement as DependencyObject, typeof(Popup), true) as Popup
						: null;

					// if focus is moving to the popup we need to hook the keydown of the popup
					// AS 10/10/07
					// This isn't robust enough. We could be losing focus to a context menu shown for a menu item
					// within this popup. We need to walk up the popups until we find the root one.
					//
					//if (null != popup && popup.TemplatedParent == proxy._popupOwner.PopupTemplatedParent)

					// if we found a popup, let's find out if its in our chain
					Popup previousPopup = popup;
					UIElement popupOwnerParent = proxy._popupOwner.PopupTemplatedParent;

					while (popup != null)
					{
						if (popup.TemplatedParent == popupOwnerParent)
							break;

						previousPopup = popup;
						DependencyObject startingPt = VisualTreeHelper.GetParent(popup) ?? LogicalTreeHelper.GetParent(popup) ?? popup.PlacementTarget;

						if (startingPt == null)
							break;

						popup = Utilities.GetAncestorFromType(startingPt, typeof(Popup), true) as Popup;
					}

					if (popup == null)
						popup = previousPopup;

					// AS 10/12/07
					// This was incorrect. We should not have been checking to see if the 
					// popup owner was an ancestor because then if a context menu is shown 
					// for the element outside this popup but inside the "button portion" 
					// of the templated parent then it will return true and the popup will 
					// remain open. E.g. Right click on the button portion of an open menu 
					// and the menu was staying open.
					//
					//if (popup != null && (popup.TemplatedParent == popupOwnerParent || popupOwnerParent.IsAncestorOf(popup.PlacementTarget)))
					if (popup != null && popup.TemplatedParent == popupOwnerParent)
					{
						// AS 10/12/07
						// Do not try to hook the popup unless we don't have one already.
						//
						//if (proxy._popupOwner.HookKeyDown)
						if (proxy._hookedPopup == null && proxy._popupOwner.HookKeyDown)
						{
							proxy.HookedPopup = popup;
						}
					}
					else
					{
						// if focus is not within the popup of the parent then close up
						proxy.IsOpen = false;
					}
				}
			}
		}

		#endregion //IsKeyboardFocusWithin

		#region IsOpen
		private bool IsOpen
		{
			get { return this._popupOwner.IsOpen; }
			set { this._popupOwner.IsOpen = value; }
		} 
		#endregion //IsOpen

		#region Ribbon
		private XamRibbon Ribbon
		{
			get { return this._popupOwner.Ribbon; }
		}
		#endregion //Ribbon

		#endregion //Private Properties 

		#region Public Properties

		// AS 12/19/07 BR29199
		#region OpeningReason
		/// <summary>
		/// Returns the reason that the proxy is opening the associated popup.
		/// </summary>
		public PopupOpeningReason OpeningReason
		{
			get { return this._openingReason; }
		}
		#endregion //OpeningReason

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		#region FocusFirstItem
		private void FocusFirstItem()
		{
			this._popupOwner.FocusFirstItem();
		} 
		#endregion //FocusFirstItem

		#region OnPopupKeyDown
		private void OnPopupKeyDown(object sender, KeyEventArgs e)
		{
			this.ProcessKeyDown(e);
		} 
		#endregion //OnPopupKeyDown

		#region OnPopupTargetKeyDown
		private void OnPopupTargetKeyDown(object sender, KeyEventArgs e)
		{
			this.ProcessKeyDown(e);
		}
		#endregion //OnPopupTargetKeyDown

		// AS 12/19/07 BR29199
		#region OpenPopup
		private void OpenPopup(PopupOpeningReason reason)
		{
			try
			{
				// AS 12/19/07 BR29199
				this._openingReason = reason;

				this.IsOpen = true;
			}
			finally
			{
				// AS 12/19/07 BR29199
				this._openingReason = PopupOpeningReason.None;
			}
		} 
		#endregion //OpenPopup

		#endregion //Private Methods

		#region Public Methods

		#region FocusFirstItem
		/// <summary>
		/// Helper method for setting focus to the first element within an items control.
		/// </summary>
		/// <param name="itemsControl">The items control whose item should be given focus.</param>
		public static bool FocusFirstItem(ItemsControl itemsControl)
		{
			if (null == itemsControl)
				throw new ArgumentNullException("itemsControl");

			ItemCollection items = itemsControl.Items;
			int count = items.Count;
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);
			ItemContainerGenerator generator = itemsControl.ItemContainerGenerator;

			for (int i = 0; i < count; i++)
			{
				DependencyObject item = generator.ContainerFromIndex(i);

				UIElement element = item as UIElement;

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Helper method for setting focus to the first element within an items control.
		/// </summary>
		/// <param name="children">The collection of children whose first focusable element will be given focus.</param>
		public static bool FocusFirstItem(UIElementCollection children)
		{
			if (null == children)
				throw new ArgumentNullException("children");

			int count = children.Count;
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);

			for (int i = 0; i < count; i++)
			{
				UIElement element = children[i];

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Helper method for setting focus to the first element within an element collection.
		/// </summary>
		/// <param name="ancestor">The visual whose children will be iterated.</param>
		public static bool FocusFirstItem(DependencyObject ancestor)
		{
			if (null == ancestor)
				throw new ArgumentNullException("ancestor");

			int count = VisualTreeHelper.GetChildrenCount(ancestor);
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);

			for (int i = 0; i < count; i++)
			{
				UIElement element = VisualTreeHelper.GetChild(ancestor, i) as UIElement;

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}

		#endregion //FocusFirstItem

		#region FocusLastItem
		/// <summary>
		/// Helper method for setting focus to the last element within an items control.
		/// </summary>
		/// <param name="itemsControl">The items control whose item should be given focus.</param>
		public static bool FocusLastItem(ItemsControl itemsControl)
		{
			if (null == itemsControl)
				throw new ArgumentNullException("itemsControl");

			ItemCollection items = itemsControl.Items;
			int count = items.Count;
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.First);
			ItemContainerGenerator generator = itemsControl.ItemContainerGenerator;

			for (int i = count - 1; i >= 0; i--)
			{
				DependencyObject item = generator.ContainerFromIndex(i);

				UIElement element = item as UIElement;

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Helper method for setting focus to the last element within an element collection.
		/// </summary>
		/// <param name="children">The collection of children whose last focusable element will be given focus.</param>
		public static bool FocusLastItem(UIElementCollection children)
		{
			if (null == children)
				throw new ArgumentNullException("children");

			int count = children.Count;
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Last);

			for (int i = count - 1; i >= 0; i--)
			{
				UIElement element = children[i];

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Helper method for setting focus to the last element within an element collection.
		/// </summary>
		/// <param name="ancestor">The visual whose children will be iterated.</param>
		public static bool FocusLastItem(DependencyObject ancestor)
		{
			if (null == ancestor)
				throw new ArgumentNullException("ancestor");

			int count = VisualTreeHelper.GetChildrenCount(ancestor);
			TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Last);

			for (int i = count - 1; i >= 0; i--)
			{
				UIElement element = VisualTreeHelper.GetChild(ancestor, i) as UIElement;

				if (element != null && element.IsVisible)
				{
					if (element.Focusable == false)
					{
						if (element.MoveFocus(request))
							return true;
					}
					else if (element.IsEnabled)
					{
						return element.Focus();
					}
				}
			}

			return false;
		}
		#endregion //FocusLastItem

		#region Initialize
		/// <summary>
		/// Used to associate the proxy with the element that will receive focus when the popup is opened.
		/// </summary>
		/// <param name="popupTarget">The uielement that will receive focus when the popup is opened.</param>
		public void Initialize(FrameworkElement popupTarget)
		{
			if (this._popupTarget != null)
				this._popupTarget.KeyDown -= new System.Windows.Input.KeyEventHandler(OnPopupTargetKeyDown);

			this._popupTarget = popupTarget;

			if (this._popupTarget != null && this._popupOwner.HookKeyDown)
				this._popupTarget.KeyDown += new System.Windows.Input.KeyEventHandler(OnPopupTargetKeyDown);

			if (null != this._popupTarget)
				BindingOperations.SetBinding(this, IsKeyboardFocusWithinProperty, Utilities.CreateBindingObject(UIElement.IsKeyboardFocusWithinProperty, BindingMode.OneWay, popupTarget));
			else
				BindingOperations.ClearBinding(this, IsKeyboardFocusWithinProperty);
		}
		#endregion //Initialize

		#region OnOpen
		/// <summary>
		/// Used to notify the proxy when the popup is being opened.
		/// </summary>
		public void OnOpen()
		{
			if (this._popupTarget != null)
			{
				XamRibbon ribbon = this.Ribbon;

				// AS 10/3/07 BR27027
				// Make sure we have focus when we are dropped down.
				//
				if (this._popupTarget.IsKeyboardFocusWithin == false)
					this._popupTarget.Focus();

                // AS 2/19/09 TFS6747
				//if (ribbon.Mode == RibbonMode.KeyTipsActive)
				if (null != ribbon && ribbon.Mode == RibbonMode.KeyTipsActive)
				{
					FrameworkElement elementToFocus = this._popupOwner.ParentElementToFocus ?? this._popupTarget;

					if (null != elementToFocus)
						elementToFocus.Focus();
				}

				Popup containingPopup = Utilities.GetAncestorFromType(this._popupTarget, typeof(Popup), true) as Popup;

				// AS 10/12/07
				// Do not shift focus if we are in a popup. This came up because a menu tool in the appmenu
				// footer toolbar was closing the app menu by shifting focus out of the ribbon.
				//
				if (containingPopup != null && XamRibbon.GetRibbon(containingPopup) == XamRibbon.GetRibbon(this._popupTarget))
					this._restoreDocumentFocusOnClose = false;
				else
					this._restoreDocumentFocusOnClose = ribbon != null && ribbon.Mode == RibbonMode.Normal;
			}
		}
		#endregion //OnOpen

		#region OnClose
		/// <summary>
		/// Used to notify the proxy when the popup is being closed.
		/// </summary>
		public void OnClose()
		{
			if (this._popupTarget != null)
			{
				XamRibbon ribbon = this.Ribbon;

				if (null != ribbon)
					Debug.WriteLine(ribbon.Mode, "Ribbon Mode On CloseUp:");

				// AS 10/11/07
				// There are cases where we were not in Normal mode when we entered but we still
				// want to shift focus out when we close.
				//
				//if (ribbon != null && this._restoreDocumentFocusOnClose)
				//	ribbon.TransferFocusOutOfRibbon();
				if (ribbon != null)
				{
					// AS 9/23/09 TFS22369
					ClosePopupHelper.SuppressWindowMouseDown(ribbon);

					// AS 10/11/07
					// If the ribbon is in normal mode and focus is within the popup owner 
					// then restore focus to the document.
					//
					if (ribbon.Mode == RibbonMode.Normal && this._popupOwner.PopupTemplatedParent != null && this._popupOwner.PopupTemplatedParent.IsKeyboardFocusWithin)
					{
						this._restoreDocumentFocusOnClose = true;

						// AS 10/12/07
						// Do not shift focus if we are in a popup. This came up because a menu tool in the appmenu
						// footer toolbar was closing the app menu by shifting focus out of the ribbon.
						//
						Popup containingPopup = Utilities.GetAncestorFromType(this._popupTarget, typeof(Popup), true) as Popup;

						if (containingPopup != null && XamRibbon.GetRibbon(containingPopup) == XamRibbon.GetRibbon(this._popupTarget))
							this._restoreDocumentFocusOnClose = false;
					}

					if (this._restoreDocumentFocusOnClose)
						ribbon.TransferFocusOutOfRibbon();
				}
			}

			this.HookedPopup = null;
		}
		#endregion //OnClose

		#region ProcessKeyDown
		/// <summary>
		/// Used to let the proxy process keyboard notifications while the popup is opened.
		/// </summary>
		/// <param name="e"></param>
		public void ProcessKeyDown(KeyEventArgs e)
		{
			if (e.Handled)
				return;

			if (this._popupTarget == null)
				return;

			XamRibbon ribbon = this.Ribbon;

			if ( ribbon != null)
			{
				switch (e.Key)
				{
					case Key.Up:
					case Key.Down:
					case Key.Right:
					case Key.Left:
					case Key.Tab:
						#region NavigationKey
						// AS 10/5/07 keyboard navigation
						// if we are not in activation mode then go into activation mode while
						if (this.IsOpen)
						{
							
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

							if (ribbon.Mode == RibbonMode.Normal)
							{
								ribbon.EnterNavigationMode();
							}

							FrameworkElement parentFocusableElement = this._popupOwner.ParentElementToFocus;

							// AS 11/9/07
							// When navigating a menu, focus may get shifted back to the parent menu item.
							// If you target is focused, just try to focus the first item.
							//
							if (parentFocusableElement != null && parentFocusableElement.IsKeyboardFocused)
							{
								this.FocusFirstItem();
								e.Handled = true;
							}
						}
						break; 
						#endregion //NavigationKey

					case Key.System:
						{
							#region SystemKey

							switch (e.SystemKey)
							{
								case Key.LeftAlt:
								case Key.RightAlt:
									#region Alt

									if (this.IsOpen)
									{
										this.IsOpen = false;

										// AS 10/8/07
										// Do not mark this as handled because we want to close all popups
										// between this element and the ribbon - e.g. the tabcontrol if 
										// minimized and expanded.
										//
										//e.Handled = true;
									}
									break;

									#endregion //Alt
							}
							break;

							#endregion //SystemKey
						}

					case Key.Escape:
						#region Escape
						if (this.IsOpen)
						{
							// AS 10/3/07
							// Since we're getting in here when not in navigation mode, we need to check
							// before we set the active item.
							//
							// AS 10/5/07
							// Do not make this the active item if we initiated the mode since we will 
							// exit the mode on leave.
							//
							//if (ribbon.Mode == RibbonMode.ActiveItemNavigation)
							if (ribbon.Mode == RibbonMode.ActiveItemNavigation && this._restoreDocumentFocusOnClose == false)
							{
								FrameworkElement elementToFocus = this._popupOwner.ParentElementToFocus ?? this._popupTarget;

								if (null != elementToFocus)
									elementToFocus.Focus();
							}

							this.IsOpen = false;

							e.Handled = true;
						}

						break;

						#endregion //Escape

					case Key.Enter:
					case Key.Space:
					case Key.F4:
						#region Enter/Space/F4

						{
							if (ribbon.Mode == RibbonMode.ActiveItemNavigation)
							{
								FrameworkElement activeItem = ribbon.ActiveItem;

								if (this._popupTarget == activeItem)
								{
									if (this._popupOwner.CanOpen)
									{
										if (this.IsOpen == false)
										{
											// AS 12/19/07 BR29199
											//this.IsOpen = true;
											this.OpenPopup(PopupOpeningReason.Keyboard);

											this.FocusFirstItem();
											e.Handled = true;

											return;
										}
									}

								}
							}
							break;
						}

						#endregion //Enter/Space/F4
				}
			}
		}
		#endregion //ProcessKeyDown

		#endregion //Public Methods

		#endregion //Methods

		// AS 9/23/09 TFS22369
		#region ClosePopupHelper class
		private class ClosePopupHelper
		{
			#region Member Variables

			private DispatcherOperation _cancelSuppressOperation;
			private FrameworkElement _rootVisual;
			private XamRibbon _ribbon;

			#endregion //Member Variables

			#region Constructor
			private ClosePopupHelper(FrameworkElement rootVisual, XamRibbon ribbon)
			{
				_rootVisual = rootVisual;
				_ribbon = ribbon;
			}
			#endregion //Constructor

			#region Internal methods
			internal static void SuppressWindowMouseDown(XamRibbon ribbon)
			{
				if (null == ribbon)
					return;

				FrameworkElement rootVisual = Window.GetWindow(ribbon) ?? ribbon.TopLevelVisual;

				if (rootVisual == null)
					return;

				ClosePopupHelper closeHelper = new ClosePopupHelper(rootVisual, ribbon);
				closeHelper.Initialize();
			}
			#endregion //Internal methods

			#region Private methods
			private void CancelSuppress()
			{
				_rootVisual.RemoveHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));

				if (_cancelSuppressOperation.Status == DispatcherOperationStatus.Pending)
					_cancelSuppressOperation.Abort();

				_cancelSuppressOperation = null;
			}

			private void Initialize()
			{
				_rootVisual.AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler(OnPreviewMouseDown));
				_cancelSuppressOperation = _rootVisual.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Infragistics.Windows.Ribbon.XamRibbon.MethodInvoker(this.CancelSuppress));
			}

			private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
			{
				if (sender == _rootVisual)
				{
					DependencyObject d = e.OriginalSource as DependencyObject;

					// do not mark it handled if the end user is clicking on another 
					// element within the ribbon
					if (null != d && XamRibbon.GetRibbon(d) == _ribbon)
						return;

					this.CancelSuppress();
					e.Handled = true;
				}
			}
			#endregion //Private methods
		}
		#endregion //ClosePopupHelper class
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