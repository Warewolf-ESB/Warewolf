using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Commands;
using Infragistics.Shared;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Control used to display the list of visible <see cref="ContentPane"/> instances 
	/// in a <see cref="XamDockManager"/> to allow activation of a pane.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PaneNavigator : Control
	{
		#region Member Variables

		private XamDockManager _dockManager;
		private ReadOnlyCollection<ContentPane> _panes;
		private ReadOnlyCollection<ContentPane> _documents;
		private ReadOnlyCollection<ContentPane> _allPanes;
		private Popup _popup;
		private bool _closeWhenModifiersReleased;
		private const ModifierKeys AllExceptShift = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Windows;

		#endregion //Member Variables

		#region Constructor
		static PaneNavigator()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(PaneNavigator), new FrameworkPropertyMetadata(typeof(PaneNavigator)));
			FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(PaneNavigator), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

			EventManager.RegisterClassHandler(typeof(PaneNavigator), Selector.SelectedEvent, new RoutedEventHandler(OnListBoxItemSelected), true);

			CommandManager.RegisterClassCommandBinding(typeof(PaneNavigator), new CommandBinding(ContentPaneCommands.ActivatePane, new ExecutedRoutedEventHandler(OnExecuteCommand), new CanExecuteRoutedEventHandler(OnCanExecuteCommand)));
		}

		/// <summary>
		/// Initializes a new <see cref="PaneNavigator"/>
		/// </summary>
		public PaneNavigator() : this(null)
		{
		}


		private PaneNavigator(XamDockManager dockManager)
		{
			this._dockManager = dockManager;
			IList<ContentPane> panes = new List<ContentPane>();
			IList<ContentPane> documents = new List<ContentPane>();
			IList<ContentPane> allPanes = new List<ContentPane>();

			if (null != dockManager)
			{
				foreach(ContentPane pane in dockManager.ActivePaneManager.GetPanes(PaneFilterFlags.AllVisible, dockManager.NavigationOrder, true))
				{
					if (pane.IsDocument)
						documents.Add(pane);
					else
						panes.Add(pane);

					allPanes.Add(pane);
				}
			}

			this._panes = new ReadOnlyCollection<ContentPane>(panes);
			this._documents = new ReadOnlyCollection<ContentPane>(documents);
			this._allPanes = new ReadOnlyCollection<ContentPane>(allPanes);
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnIsKeyboardFocusWithinChanged
		/// <summary>
		/// Invoked when the control gets or loses the keyboard focus.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnIsKeyboardFocusWithinChanged(e);

			// if the control loses focus then close the navigator
			if (false.Equals(e.NewValue))
				this.Close(true);
		}
		#endregion //OnIsKeyboardFocusWithinChanged

		#region OnKeyDown
		/// <summary>
		/// Invoked when a key is pressed
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			Key key = e.Key == Key.System ? e.SystemKey : e.Key;

			if (key == Key.Escape)
			{
				this.Close(true);
				e.Handled = true;
			}
			else if (key == Key.Enter)
			{
				this.Close(false);
				e.Handled = true;
			}
			else if (key == Key.F7 && (Keyboard.Modifiers & AllExceptShift) == ModifierKeys.Alt)
			{
				IInputElement focusedElement = Keyboard.FocusedElement;

				if (null != focusedElement)
				{
					FocusNavigationDirection direction = (Keyboard.Modifiers & ModifierKeys.Shift) == 0 ? FocusNavigationDirection.Next : FocusNavigationDirection.Previous;
					TraversalRequest request = new TraversalRequest(direction);

					if (DockManagerUtilities.MoveFocus(focusedElement as DependencyObject, request))
						e.Handled = true;
				}
			}
		}
		#endregion //OnKeyDown

		#region OnPreviewKeyUp
		/// <summary>
		/// Invoked when a key is being released.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnPreviewKeyUp(KeyEventArgs e)
		{
			base.OnPreviewKeyUp(e);

			if (this.IsOpen && this._closeWhenModifiersReleased)
			{
				Key key = e.Key == Key.System ? e.SystemKey : e.Key;

				switch (key)
				{
					case Key.LeftAlt:
					case Key.RightAlt:
					case Key.LeftCtrl:
					case Key.RightCtrl:
						if (e.KeyboardDevice.IsKeyUp(Key.LeftAlt) &&
							e.KeyboardDevice.IsKeyUp(Key.LeftCtrl) &&
							e.KeyboardDevice.IsKeyUp(Key.RightAlt) &&
							e.KeyboardDevice.IsKeyUp(Key.RightCtrl))
						{
							this.Close(false);
						}
						break;
				}
			}
		}
		#endregion //OnPreviewKeyUp

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region AllPanes
		/// <summary>
		/// Returns a collection of the activatable <see cref="ContentPane"/> instances in the <see cref="DocumentContentHost"/> of the associated <see cref="XamDockManager"/> when the navigator was created.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ReadOnlyCollection<ContentPane> AllPanes
		{
			get { return this._allPanes; }
		}
		#endregion //AllPanes

		#region Documents
		/// <summary>
		/// Returns a collection of the activatable <see cref="ContentPane"/> instances in the <see cref="DocumentContentHost"/> of the associated <see cref="XamDockManager"/> when the navigator was created.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ReadOnlyCollection<ContentPane> Documents
		{
			get { return this._documents; }
		} 
		#endregion //Documents

		#region Panes
		/// <summary>
		/// Returns a collection of the activatable non-document <see cref="ContentPane"/> instances in the associated <see cref="XamDockManager"/> when the navigator was created.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public ReadOnlyCollection<ContentPane> Panes
		{
			get { return this._panes; }
		} 
		#endregion //Panes

		#region SelectedPane

		/// <summary>
		/// Identifies the <see cref="SelectedPane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedPaneProperty = DependencyProperty.Register("SelectedPane",
			typeof(ContentPane), typeof(PaneNavigator), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSelectedPaneChanged)));

		private static void OnSelectedPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			Debug.Write(string.Format("Old: {0}, New:{1}", e.OldValue, e.NewValue), "PaneNavigator.SelectedPane Changed");

			PaneNavigator navigator = (PaneNavigator)d;

			if (e.NewValue != null && false == navigator.AllPanes.Contains((ContentPane)e.NewValue))
			{
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidateNavigatorSelectedPane"));
			}


			// Set some properties related to the newly selected pane that can be bound to in the PaneNavigator template.
			ContentPane selectedPane	= e.NewValue as ContentPane;
			// AS 5/23/08 BR33261
			// Cannot assume selection will not go to null.
			//
			//bool		isWide			= (selectedPane.ActualWidth / selectedPane.ActualHeight) > 1.2d;
			//navigator.SetValue(PaneNavigator.SelectedPaneAspectRatioIsWidePropertyKey, isWide);
			//navigator.SetValue(PaneNavigator.SelectedPaneIsDocumentPropertyKey, selectedPane.IsDocument);
			bool isWide = null != selectedPane && (selectedPane.ActualWidth / selectedPane.ActualHeight) > 1.2d;
			bool isDocument = null != selectedPane && selectedPane.IsDocument;
			navigator.SetValue(PaneNavigator.SelectedPaneAspectRatioIsWidePropertyKey, isWide);
			navigator.SetValue(PaneNavigator.SelectedPaneIsDocumentPropertyKey, isDocument);
		}

		/// <summary>
		/// Returns/sets the currently selected pane that will be activated when the dialog is closed.
		/// </summary>
		/// <seealso cref="SelectedPaneProperty"/>
		//[Description("Returns/sets the currently selected pane that will be activated when the dialog is closed.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		public ContentPane SelectedPane
		{
			get
			{
				return (ContentPane)this.GetValue(PaneNavigator.SelectedPaneProperty);
			}
			set
			{
				this.SetValue(PaneNavigator.SelectedPaneProperty, value);
			}
		}

		#endregion //SelectedPane

		#region SelectedPaneAspectRatioIsWide

		private static readonly DependencyPropertyKey SelectedPaneAspectRatioIsWidePropertyKey =
			DependencyProperty.RegisterReadOnly("SelectedPaneAspectRatioIsWide",
			typeof(bool), typeof(PaneNavigator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="SelectedPaneAspectRatioIsWide"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedPaneAspectRatioIsWideProperty =
			SelectedPaneAspectRatioIsWidePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the aspect ratio of the selected pane is considered wide.
		/// </summary>
		/// <seealso cref="SelectedPaneAspectRatioIsWideProperty"/>
		//[Description("Returns true if the aspect ratio of the selected pane is considered wide.")]
		//[Category("DockManager Properties")] // Appearance
		[Bindable(true)]
		[ReadOnly(true)]
		public bool SelectedPaneAspectRatioIsWide
		{
			get
			{
				return (bool)this.GetValue(PaneNavigator.SelectedPaneAspectRatioIsWideProperty);
			}
		}

		#endregion //SelectedPaneAspectRatioIsWide

		#region SelectedPaneIsDocument

		private static readonly DependencyPropertyKey SelectedPaneIsDocumentPropertyKey =
			DependencyProperty.RegisterReadOnly("SelectedPaneIsDocument",
			typeof(bool), typeof(PaneNavigator), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the <see cref="SelectedPaneIsDocument"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedPaneIsDocumentProperty =
			SelectedPaneIsDocumentPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if the SelectedPane is a document.
		/// </summary>
		/// <seealso cref="SelectedPaneIsDocumentProperty"/>
		//[Description("Returns true if the SelectedPane is a document.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public bool SelectedPaneIsDocument
		{
			get
			{
				return (bool)this.GetValue(PaneNavigator.SelectedPaneIsDocumentProperty);
			}
		}

		#endregion //SelectedPaneIsDocument

		#endregion //Public Properties

		#region Internal Properties

		#region IsOpen
		internal bool IsOpen
		{
			get { return this._popup != null && this._popup.IsOpen; }
		} 
		#endregion //IsOpen

		#endregion //Internal Properties

		#region Private Properties

		#region Popup
		private Popup Popup
		{
			get { return this._popup; }
			set
			{
				if (value != this._popup)
				{
					if (null != this._popup)
						this._popup.Opened -= new EventHandler(OnPopupOpened);

					this._popup = value;

					if (null != this._popup)
						this._popup.Opened += new EventHandler(OnPopupOpened);
				}
			}
		} 
		#endregion //Popup

		#endregion //Private Properties

		#endregion //Properties	

		#region Methods

		#region Internal Methods

		#region Close
		/// <summary>
		/// Closes the navigator popup
		/// </summary>
		internal void Close(bool cancel)
		{
			if (this.IsOpen)
			{
				Popup oldPopup = this.Popup;
				this.Popup = null;
				oldPopup.IsOpen = false;

				if (false == cancel && null != this.SelectedPane)
				{
					// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
					//this.SelectedPane.Activate();
					this.SelectedPane.ActivateInternal(false, true, true);
				}
				else
				{
					// since we took keyboard focus when we showed the navigator, if the element
					// is still focused then we want to restore focus to the last active pane
					if (this.IsKeyboardFocusWithin)
					{
						ContentPane pane = this._dockManager.ActivePaneManager.GetLastActivePane();

						if (null != pane)
							pane.ActivateInternal(true);
						else
							Keyboard.Focus(null);
					}
				}
			}
		}
		#endregion //Close

		#region ProcessKeyDown
		/// <summary>
		/// Helper method to show the pane navigator based on a keystroke if required
		/// </summary>
		/// <param name="dockManager">The dockmanager for which the navigator would be shown</param>
		/// <param name="e">The key event arguments</param>
		internal static void ProcessKeyDown(XamDockManager dockManager, KeyEventArgs e)
		{
			Key key = e.Key == Key.System ? e.SystemKey : e.Key;
			bool navigatePanes = false;

			switch (key)
			{
				case Key.F7:
					if (ModifierKeys.Alt != (Keyboard.Modifiers & AllExceptShift))
						return;
					navigatePanes = true;
					break;
				case Key.Tab:
					if (ModifierKeys.Control != (Keyboard.Modifiers & AllExceptShift))
						return;
					break;
				default:
					return;
			}

			// make sure the dockmanager has at least 1 activatable pane - we don't want to 
			// show a navigator with no items
			if (dockManager.ActivePaneManager.HasActivatablePanes == false)
				return;

			// show the navigator
			bool isShiftDown = ModifierKeys.Shift == (Keyboard.Modifiers & ModifierKeys.Shift);
			PaneNavigatorStartInfo startInfo = new PaneNavigatorStartInfo();

			startInfo.ActivationOffset = isShiftDown ? -1 : 1;
			startInfo.StartWithDocuments = navigatePanes == false;
			startInfo.CloseWhenModifiersRelease = true;

			ExecuteCommandInfo commandInfo = new ExecuteCommandInfo(DockManagerCommands.ShowPaneNavigator, startInfo, dockManager);

			if (((ICommandHost)dockManager).Execute(commandInfo))
				e.Handled = true;
		} 
		#endregion //ProcessKeyDown

		#region Show
		/// <summary>
		/// Helper method for showing the <see cref="PaneNavigator"/> for the <see cref="XamDockManager"/>
		/// </summary>
		/// <param name="dockManager">The dockmanager for which the navigator is being shown.</param>
		/// <param name="startInfo">Provides information used to initialize the navigator.</param>
		// AS 9/9/09 TFS21110
		//internal static void Show(XamDockManager dockManager, PaneNavigatorStartInfo startInfo)
		internal static bool Show(XamDockManager dockManager, PaneNavigatorStartInfo startInfo)
		{
			DockManagerUtilities.ThrowIfNull(dockManager, "dockManager");
			DockManagerUtilities.ThrowIfNull(startInfo, "startInfo");

			// setup
			PaneNavigator navigator = new PaneNavigator(dockManager);
			Popup popup = new Popup();

			// center the navigator within the dockmanager
			popup.PlacementTarget = dockManager;
			popup.Placement = PlacementMode.Center;
			popup.Child = navigator;
			popup.StaysOpen = false;
			popup.AllowsTransparency = true;

			// initialize the selected item to be a particular pane
			ContentPane initialSelectedPane = startInfo.StartWithDocuments.HasValue
				? dockManager.ActivePaneManager.GetInitialPaneNavigatorItem(startInfo.StartWithDocuments.Value, startInfo.ActivationOffset)
				: dockManager.ActivePane ?? dockManager.ActivePaneManager.GetActiveDocument() ?? dockManager.ActivePaneManager.GetLastActivePane();

			Debug.Assert(null != initialSelectedPane, "We couldn't find a pane to start with. Should we be showing the navigator?");

			// the navigator needs to reference the popup so it can close it if needed
			navigator.Popup = popup;
			navigator._closeWhenModifiersReleased = startInfo.CloseWhenModifiersRelease;
			navigator.SelectedPane = initialSelectedPane;
			popup.IsOpen = true;

			// AS 4/25/08
			// There were a lot of issues/inconsistencies when we were relying upon the selection state of the 
			// list items within the navigator's lists. Therefore, we are switching over to relying on focus. 
			// So we have a binding in the template to binding the selectedpane to the focusedelement's 
			// datacontext (since the navigator is a focus scope). However since that is a style set value, the 
			// local value we set above is taking precedence. We need to set the SelectedPane above because
			// we want the ListBoxes in the template to be able to initialize their selection based on the
			// initial SelectedPane. So after we have opened the popup (and therefore after the template has
			// been applied) we can remove the locally set value which will allow the style value to be used 
			// used - i.e. the binding between the selectedpane and the focusedelement.datacontext. Note, 
			// we still need to listen to the selectionchanged event so that we can initialize the focusedelement
			// (by calling focus on the listboxitem) when we set the selected pane above.
			//
			navigator.ClearValue(SelectedPaneProperty);

			// AS 9/9/09 TFS21110
			// When you call Focus on an element, ultimately the KeyboardDevice.TryChangeFocus method is invoked. 
			// That method will eventually call AcquireFocus on the IKeyboardInputProvider associated with the 
			// HwndSource that contains the element which you are trying to focus. For the default framework 
			// impl (i.e. HwndKeyboardInputProvider) it will only call the SetFocus api on the associated 
			// HwndSource's Hwnd if it doesn't have the WM_EX_SHOWNOACTIVATE window style bit - which popups 
			// do. Well the PaneNavigator is within a popup so when focus is within an HwndHost, focus will 
			// not be within a HwndSource that has the same Dispatcher and therefore the call to AcquireFocus 
			// will return false. Since that returns false, the framework will not try to change focus to 
			// the requested element. Therefore keyboard focus remains where it was - which in this case is 
			// within an HwndHost in the Window - so it seems like the PaneNavigator isn't responsive. 
			// We need to try and take focus out of the HwndHost. The best case is to focus the ContentPane 
			// itself but if we can't do that we'll focus the xamDockManager.
			// 
			dockManager.TransferFocusOutOfHwndHost();

			navigator.Focus();

			// AS 9/9/09 TFS21110
			// If we can't take keyboard focus we probably should just close up since 
			// it may appear "unresponsive" even though you can still click an item with 
			// the mouse.
			//
			if (!navigator.IsKeyboardFocusWithin)
				navigator.Close(true);

			// added a return value so we know whether to mark the keypress as handled
			return navigator.IsKeyboardFocusWithin;
		}
		#endregion //Show

		#endregion //Internal Methods

		#region Private Methods

		#region OnCanExecuteCommand
		private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (e.Command == ContentPaneCommands.ActivatePane)
			{
				DependencyObject d = e.OriginalSource as DependencyObject;
				ContentPane cp = d != null ? d.GetValue(FrameworkElement.DataContextProperty) as ContentPane : null;

				e.Handled = true;
				e.CanExecute = cp != null && cp.CanActivate;
			}
		} 
		#endregion //OnCanExecuteCommand

		#region OnExecuteCommand
		private static void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			PaneNavigator navigator = sender as PaneNavigator;

			if (e.Command == ContentPaneCommands.ActivatePane)
			{
				DependencyObject d = e.OriginalSource as DependencyObject;
				ContentPane cp = d != null ? d.GetValue(FrameworkElement.DataContextProperty) as ContentPane : null;

				if (cp != null && cp.CanActivate)
				{
					navigator.SelectedPane = cp;
					navigator.Close(false);
					e.Handled = true;
				}
			}
		} 
		#endregion //OnExecuteCommand

		#region OnListBoxItemSelected

		private static void OnListBoxItemSelected(object sender, RoutedEventArgs e)
		{
			PaneNavigator paneNavigator = e.Source as PaneNavigator;
			if (paneNavigator != null && paneNavigator.IsLoaded == false)
			{
				ListBoxItem listBoxItem = e.OriginalSource as ListBoxItem;
				if (listBoxItem != null)
					listBoxItem.Focus();
			}
		}

		#endregion //OnListBoxItemSelected	
    
		#region OnPopupOpened
		private void OnPopupOpened(object sender, EventArgs e)
		{
			//this.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));

            // AS 3/30/09 TFS16355 - WinForms Interop
            Infragistics.Windows.Controls.PopupHelper.BringToFront(sender as Popup);
		} 
		#endregion //OnPopupOpened

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