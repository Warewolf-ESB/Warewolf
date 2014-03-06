using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Media;
using Infragistics.Windows.Commands;
using System.Collections;
using System.Windows.Threading;
using Infragistics.Windows.Themes;
using Infragistics.Shared;
using Infragistics.Windows.DockManager.Events;
using System.Windows.Markup;
using Infragistics.Windows.DockManager.Dragging;
using Infragistics.Collections;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// A class used by <see cref="XamDockManager"/> to host floating panes.
	/// </summary>
	//[ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PaneToolWindow : ToolWindow
		, ICommandHost
		, IPaneContainer
	{
		#region Member Variables

		private bool _synchronizingLocation;
		private bool _synchronizingSize;
		private ContentPane _activePane;
		private ContentPane _singlePane;
		private FrameworkElement _captionElement;
		private bool _isClosing;
		private bool _isWindowLoaded;

		// AS 9/11/09 TFS21330
		private DispatcherOperation _pendingVerifyAllowsTransparency;
		private bool _isReshowing = false;
		private int _suspendReshowCount;

		// AS 4/28/11 TFS73532
		private int _syncShowCount;
		private IDisposable _pendingSyncShow;
		private bool _isRaiseWindowLoadedPending;
		private bool _isRaisingToolWindowLoaded; // AS 8/22/11 TFS84648

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		private FrameworkContentElement _dragHelper;

		// AS 9/20/11 TFS88634
		private bool _forceRemainOpen;

		#endregion //Member Variables

		#region Constructor
		static PaneToolWindow()
		{
			// AS 5/9/08
			// register the groupings that should be applied when the theme property is changed
			ThemeManager.RegisterGroupings(typeof(PaneToolWindow), new string[] { PrimitivesGeneric.Location.Grouping, DockManagerGeneric.Location.Grouping });

			ToolWindow.LeftProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(OnTopLeftChanged));
			ToolWindow.TopProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(OnTopLeftChanged));
			ToolWindow.WidthProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(OnWidthHeightChanged));
			ToolWindow.HeightProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(OnWidthHeightChanged));

			// we need to change the visible to collapsed if all children are collapsed
			EventManager.RegisterClassHandler(typeof(PaneToolWindow), DockManagerUtilities.VisibilityChangedEvent, new RoutedEventHandler(OnChildVisibilityChanged));

			// AS 6/23/11 TFS73499
			// We already bind the Visibility of the PaneToolWindow to the Visibility of the root SplitPane
			// but we want to be able to override that based on the new FloatingWindowVisibility.
			//
			UIElement.VisibilityProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisibilityChanged), new CoerceValueCallback(CoerceVisibility)));

			XamDockManager.DockManagerPropertyKey.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDockManagerChanged)));

			// Call the Initialize method of our base class Commands<T> to register bindings for the commands represented
			// by our CommandWrappers.
			Commands<PaneToolWindow>.Initialize(DockManagerCommands.GetCommandWrappers());

			// AS 6/30/10 TFS35252
			// Moved to the instance constructor since the static handler will be invoked before
			// commands were routed to the instance handlers.
			//
			//// AS 5/4/10 TFS31317
			//// If the CanExecute/Execute bubbles up to a floating window then re-route that to the dockmanager so it can continue bubbling 
			//// from their.
			////
			//EventManager.RegisterClassHandler(typeof(PaneToolWindow), CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(OnCanExecuteCommand));
			//EventManager.RegisterClassHandler(typeof(PaneToolWindow), CommandManager.ExecutedEvent, new ExecutedRoutedEventHandler(OnExecuteCommand));

			// AS 6/24/11 FloatingWindowCaptionSource
			ToolWindow.UseOSNonClientAreaProperty.OverrideMetadata(typeof(PaneToolWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceUseOSNonClientArea)));
		}

		/// <summary>
		/// Initializes a new <see cref="PaneToolWindow"/>
		/// </summary>
		public PaneToolWindow()
		{
			// AS 6/4/08 BR33512
			NameScope.SetNameScope(this, new OwnerNameScope(this));

			// AS 4/25/08
			// The elements were not in the visual tree when we wanted to update
			// the floating rect and location so do so when the window is loaded.
			// Also, the location of the elements within the window could change without
			// the tool window being measured/arranged so also hook the layout updated.
			//
			this.Loaded += delegate(object sender, RoutedEventArgs e)
			{
				this.SynchronizeContentPaneFloatingRect();

				this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
			};

			this.Unloaded += delegate(object sender, RoutedEventArgs e)
			{
				this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
			};

			// AS 6/30/10 TFS35252
			// Moved here from the static constructor.
			//
			// AS 5/4/10 TFS31317
			// If the CanExecute/Execute bubbles up to a floating window then re-route that to the dockmanager so it can continue bubbling 
			// from their.
			//
			this.AddHandler(CommandManager.CanExecuteEvent, new CanExecuteRoutedEventHandler(OnCanExecuteCommand));
			this.AddHandler(CommandManager.ExecutedEvent, new ExecutedRoutedEventHandler(OnExecuteCommand));
		}

		#endregion //Constructor

		#region Base class overrides

		// AS 9/20/11 TFS88634
		#region CloseOverride
		internal override void CloseOverride()
		{
			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm != null)
				dm.DragManager.OnToolWindowClosing(this);

			if (this.ForceRemainOpen)
				return;

			base.CloseOverride();
		} 
		#endregion //CloseOverride

		#region CreateDefaultContextMenu
		/// <summary>
		/// Returns the default context menu for the window.
		/// </summary>
		/// <param name="originalSource">The original source for the context menu being shown</param>
		/// <returns>The context menu of the active pane.</returns>
		protected override ContextMenu CreateDefaultContextMenu(object originalSource)
		{
			DependencyObject dep = originalSource as DependencyObject;
			SplitPane split = this.Content as SplitPane;

			while (dep is ContentElement)
				dep = LogicalTreeHelper.GetParent(dep);

			// make sure we only show it for the non-client area
			if (dep is Visual && split != null && split.IsAncestorOf(dep) == false)
			{
				this.VerifyActivePane();

				ContentPane pane = this._activePane ?? DockManagerUtilities.GetFirstLastPane(split, true);

				if (null != pane)
				{
					ContextMenu menu = new ContextMenu();
					menu.SetValue(DefaultStyleKeyProperty, XamDockManager.ContextMenuStyleKey);
					pane.InitializeMenu(menu.Items);
					return menu;
				}
			}

			return base.CreateDefaultContextMenu(originalSource);
		} 
		#endregion //CreateDefaultContextMenu

		// AS 8/4/11 TFS83465/TFS83469
		#region KeepOnScreen
		internal override bool KeepOnScreen
		{
			get
			{
				XamDockManager dm = XamDockManager.GetDockManager(this);

				// don't manipulate the position during the drag
				if (dm != null && dm.DragManager.DragState != DragState.None && (this.Host == null || this.Host.IsWindow))
					return false;

				return base.KeepOnScreen;
			}
		} 
		#endregion //KeepOnScreen

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region LogicalChildren
		/// <summary>
		/// Returns an enumerator of the logical children
		/// </summary>
		protected override System.Collections.IEnumerator LogicalChildren
		{
			get
			{
				IEnumerator baseEnumerator = base.LogicalChildren;

				if (_dragHelper != null)
				{
					return new MultiSourceEnumerator(baseEnumerator, new SingleItemEnumerator(_dragHelper));
				}

				return baseEnumerator;
			}
		} 
		#endregion //LogicalChildren

		#region OnActivated
		/// <summary>
		/// Invoked when the window is activated.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);

			// if the window gets activated and it doesn't have a focused element then 
			// put focus into the first control within the window
			DependencyObject focusScope = FocusManager.GetFocusScope(this);
			IInputElement focusedElement = null != focusScope ? FocusManager.GetFocusedElement(focusScope) : null;

			if (focusScope != null && (focusedElement == focusScope || focusedElement == this))
			{
				this.VerifyActivePane();

				if (null != this._activePane)
					this._activePane.ActivateInternal(true);
			}

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
				dockManager.ActivePaneManager.OnToolWindowActivationChanged(this);
		} 
		#endregion //OnActivated

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template of the element has been initialized.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this._captionElement = this.GetTemplateChild("PART_Caption") as FrameworkElement;
		} 
		#endregion //OnApplyTemplate

		#region OnClosing
		/// <summary>
		/// Used to raise the <see cref="ToolWindow.Closing"/> event
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		internal protected override void OnClosing(CancelEventArgs e)
		{
			Debug.Assert(!_isRaisingToolWindowLoaded, "Do we know that we are closing while we are raising the loaded?");

			base.OnClosing(e);

			// AS 9/11/09 TFS21330
			// We do not want to close the panes if we are closing the window to reshot it without transparency.
			//
			if (_isReshowing)
				return;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (this.IsReshowing)
				return;

			// AS 3/2/12 TFS103671
			// If the window is being closed after we have "unloaded" it then we shouldn't be 
			// setting the Visibility of the contained ContentPanes to false. Also we shouldn't 
			// allow it to be cancelled.
			//
			if (!this.IsWindowLoaded)
			{
				e.Cancel = false;
				return;
			}

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			// AS 4/30/08
			// if the owner is unloaded then we should let the window close but we
			// should not change the state of the content panes. this would happen
			// when you navigate to another page or select a different tab. in that 
			// case we want the panes to remain in the same state when the owner is
			// reloaded and we reshow the floating windows. for all other cases we 
			// will continue to close the child content panes when the user clicks
			// on the close button, etc.
			//
			if (e.Cancel == false && null != dockManager && dockManager.IsLoaded)
			{
				List<ContentPane> closablePanes = new List<ContentPane>();

				SplitPane rootPane = this.Content as SplitPane;
				PaneFilterFlags filter = PaneFilterFlags.AllVisible;
				ContentPane pane = DockManagerUtilities.GetFirstLastPane(rootPane, true, filter);

				while (null != pane)
				{
					if (pane.AllowClose)
						closablePanes.Add(pane);

					pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, rootPane, filter);
				}

				bool wasClosing = this._isClosing;
				this._isClosing = true;

				try
				{
					dockManager.ClosePanes(closablePanes);
				}
				finally
				{
					this._isClosing = wasClosing;
				}

				e.Cancel = null != DockManagerUtilities.GetFirstLastPane(rootPane, true);

				if (e.Cancel)
					this.RefreshToolWindowState();
				else if (this.IsKeyboardFocusWithin)
				{
					// AS 9/27/11 TFS89398
					// If we have focus and are about to close then shift focus into 
					// the associated dockmanager.
					//
					if (dockManager.DragManager.DragState == DragState.ProcessingDrop)
						dockManager.ForceFocus(dockManager.DragHelper);
				}
			}
		} 
		#endregion //OnClosing

		#region OnContentChanged
		/// <summary>
		/// Invoked when the <see cref="ContentControl.Content"/> property has changed.
		/// </summary>
		/// <param name="oldContent">The previous value for the Content property</param>
		/// <param name="newContent">The new value for the Content property</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			if (newContent != null && newContent is SplitPane == false)
				throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidPaneToolWindowContent"));

			base.OnContentChanged(oldContent, newContent);

			SplitPane pane = newContent as SplitPane;

			if (null != pane)
			{
				this.SetBinding(VisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.OneWay, newContent));
				this.SetBinding(FloatingLocationProperty, Utilities.CreateBindingObject(XamDockManager.FloatingLocationProperty, BindingMode.TwoWay, newContent));
				this.SetBinding(FloatingSizeProperty, Utilities.CreateBindingObject(XamDockManager.FloatingSizeProperty, BindingMode.TwoWay, newContent));
			}
			else
			{
				BindingOperations.ClearBinding(this, VisibilityProperty);
				BindingOperations.ClearBinding(this, FloatingLocationProperty);
				BindingOperations.ClearBinding(this, FloatingSizeProperty);
			}

			this.RefreshToolWindowState();

			this.SetValue(PanePropertyKey, pane);
		}
		#endregion //OnContentChanged 

		#region OnDeactivated
		/// <summary>
		/// Invoked when the window is activated.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnDeactivated(EventArgs e)
		{
			base.OnDeactivated(e);

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
				dockManager.ActivePaneManager.OnToolWindowActivationChanged(this);
		} 
		#endregion //OnDeactivated

		#region OnDoubleClickCaption
		/// <summary>
		/// Invoked when the mouse has been double clicked on the caption.
		/// </summary>
		/// <param name="pt">The relative point in the window that was clicked</param>
		/// <param name="mouse">The device used to initiate the action</param>
		/// <returns>True if the action was handled.</returns>
		internal protected override bool OnDoubleClickCaption(Point pt, MouseDevice mouse)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (dockManager != null)
			{
				// AS 6/9/11 TFS76337
				var doubleClickAction = dockManager.FloatingWindowDoubleClickAction;

				if (doubleClickAction == null)
				{
					// if its window state can be toggled then we will do that. otherwise 
					// we'll let it fall back to toggling the docked states
					if (base.OnDoubleClickCaption(pt, mouse))
						return true;
				}
				else if (doubleClickAction == FloatingWindowDoubleClickAction.ToggleWindowState)
				{
					return base.OnDoubleClickCaption(pt, mouse);
				}
			}

			if (XamDockManager.GetPaneLocation(this) == PaneLocation.Floating)
			{
				SplitPane split = this.Content as SplitPane;

				if (null != split)
				{
					List<ContentPane> panes = DockManagerUtilities.GetAllPanes(split, PaneFilterFlags.AllVisible);

					bool executed = false;

					// AS 6/9/11 TFS76337
					//XamDockManager dockManager = XamDockManager.GetDockManager(this);

					if (null != dockManager)
						executed = dockManager.ToggleDockedState(panes, true );

					if (executed)
						return true;
				}
			}

			// AS 5/19/11 TFS75299
			// The base implementation returns false which means that the OS window can act on 
			// the double click operation. Well prior to 11.1, that would never do anything since 
			// it only has meaning if a window can be maximized which is new to 11.1. We did not 
			// intend to have double click of the title bar toggle the docked state - at least 
			// not by default since we still want the previous behavior whereby the window would 
			// toggle with its previous docked state. In the future we can consider adding a 
			// property to control what happens when double clicking the floating window 
			// caption.
			//
			//return base.OnDoubleClickCaption(pt, mouse);
			return true;
		} 
		#endregion //OnDoubleClickCaption

		#region OnDragCaption
		/// <summary>
		/// Overriden. Invoked when the window is about to be moved using the mouse on the caption area.
		/// </summary>
		/// <param name="pt">The point relative to the upper left corner of the window.</param>
		/// <param name="mouse">The mouse device used to initiate the action</param>
		/// <returns>True if the dockmanager has initiated a drag; otherwise false to allow the base implementation to perform the move operation.</returns>
		internal protected override bool OnDragCaption(Point pt, MouseDevice mouse)
		{
		    // AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		    if (this.WindowState == WindowState.Minimized)
		        return base.OnDragCaption(pt, mouse);

		    XamDockManager dockManager = XamDockManager.GetDockManager(this);
		
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If the toolwindow is hosted in a WPF window then we want to let the os drag the mouse
			// so we can support dragging into a maximized state and also when we are not using owned 
			// windows we don't want the dockmanager's window to come in front of the floating 
			// windows.
			//
			if (dockManager != null && 
				this.Host != null && 
				this.Host.IsWindow &&
				(dockManager.FloatingWindowDragMode == FloatingWindowDragMode.UseSystemWindowDrag 
				// AS 2/22/12 TFS101038
				// There seems to be an issue in the OS or WPF whereby when the mouse down was done 
				// on the non-client area that we only get mousemove messages when over the non-client 
				// area of a window. When the mouse is over the client area of a WPF element in the 
				// app, we don't get the mouse move. It does work if we let the OS do the window drag 
				// so we will let it do the drag. To minimize when we ignore the FloatingWindowDragMode 
				// we will only do this if it seems like the drag is happening as a result of a touch 
				// operation.
				//
				|| this.IsNonClientTouchDrag)
				)
			{
				bool isDragStarted = dockManager.DragManager.OnWindowBeforeDrag(this, pt, mouse);

				// if a drag was started then return false to let the os perform the drag 
				// of the window. if the drag was not started then return true so we prevent 
				// the os from capture (the caller will think this window is doing its own capture).
				return !isDragStarted;
			}

			Debug.Assert(dockManager.DragManager.DragState == Infragistics.Windows.DockManager.Dragging.DragState.None);

		    // AS 3/13/09 FloatingWindowDragMode
		    // Since we may want to show a preview, let the drag manager
		    // deal with floating only windows too.
		    // 
		    //if (null != dockManager && XamDockManager.GetPaneLocation(this) == PaneLocation.Floating)
		    bool useDragManager = null != dockManager;
		
		    if (useDragManager)
		    {
		        switch (XamDockManager.GetPaneLocation(this))
		        {
		            case PaneLocation.FloatingOnly:
		                // only use it if deferred to maintain the behavior in VS 
		                // whereby floating only panes are just dragged by the system
		                if (dockManager.DragFullWindows)
		                    useDragManager = false;
		
		                break;
		            case PaneLocation.Floating:
		                // always use the drag manager
		                break;
		            default:
		                Debug.Fail("Unexpected location for tool window:" + XamDockManager.GetPaneLocation(this).ToString());
		                useDragManager = false;
		                break;
		        }
		    }
		
		    if (useDragManager)
		    {
		        return dockManager.DragManager.StartWindowDrag(this, pt, mouse);
		    }
		    else
		    {
		        return base.OnDragCaption(pt, mouse);
		    }
		} 
		#endregion //OnDragCaption

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnEnterMove
		internal override bool OnEnterMove()
		{
			base.OnEnterMove();

			// AS 1/4/12 TFS85418
			// If we started the drag then the drag manager is expecting to be called.
			//
			//// when minimized don't do any dragging logic
			//if ( this.WindowState == WindowState.Minimized )
			//    return false;

			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm == null)
				return false;

			if (!dm.DragManager.IsDragging)
				return false;

			dm.DragManager.OnWindowDragStart(this);
			return true;
		} 
		#endregion //OnEnterMove

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnExitMove
		// AS 5/8/12 TFS107054
		// Added a boolean return value to know if the window did anything.
		//
		internal override bool OnExitMove()
		{
			bool result = base.OnExitMove();

			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm != null && dm.DragManager != null)
				result |= dm.DragManager.OnWindowDragEnd(this);

			return result;
		}
		#endregion //OnExitMove

		#region OnInitialized
		/// <summary>
		/// Invoked when the element has been initialized.
		/// </summary>
		/// <param name="e">Provides data for the event.</param>
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
		} 
		#endregion //OnInitialized

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (e.Handled == true)
				return;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
				dockManager.ProcessKeyDown(e);
		}
		#endregion //OnKeyDown

		#region OnKeyUp

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Handled == true)
				return;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager)
				dockManager.ProcessKeyUp(e);
		}
		#endregion //OnKeyUp

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnPreviewGotKeyboardFocus
		/// <summary>
		/// Invoked when the element is about to receive keyboard focus.
		/// </summary>
		/// <param name="e">Provides data about the event</param>
		protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
		{
			if (_dragHelper != null && e.NewFocus == _dragHelper)
			{
				XamDockManager dm = XamDockManager.GetDockManager(this);

				// ignore this while dragging as the dragmanager is likely focusing it
				if (dm == null || dm.DragManager.IsDragging)
					return;

				if (null == _activePane)
					this.VerifyActivePane();

				if (null != _activePane && _activePane.CanActivate)
				{
					_activePane.Activate();
					e.Handled = true;
				}
				else
				{
					Debug.Assert(!this.IsVisible, "Focus is shifting to the drag helper of the window");
				}
			}
		} 
		#endregion //OnPreviewGotKeyboardFocus

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnMoveCancelled
		internal override void OnMoveCancelled()
		{
			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm != null)
				dm.DragManager.OnWindowMoveCancelled(this);
		} 
		#endregion //OnMoveCancelled

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnWindowMoving
		internal override void OnWindowMoving(Point pt, MouseDevice mouse, Rect logicalScreenRect)
		{
			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (null != dm)
				dm.DragManager.OnWindowDragging(this, pt, mouse, logicalScreenRect);
		}
		#endregion //OnWindowMoving

		// AS 9/11/09 TFS21330
		#region SupportsAllowsTransparency
		/// <summary>
		/// Returns a boolean indicating whether the class supports being shown in a host where AllowsTransparency is true.
		/// </summary>
		protected override bool SupportsAllowsTransparency
		{
			get
			{
				return !DockManagerUtilities.GetPreventAllowsTransparency(this);
			}
		}
		#endregion //SupportsAllowsTransparency

		// AS 7/25/08 NA 2008 Vol 2 - ShowDialog
        #region SupportsShowDialog
        /// <summary>
        /// The PaneToolWindow cannot be shown modally.
        /// </summary>
        protected sealed override bool SupportsShowDialog
        {
            get
            {
                return false;
            }
        } 
        #endregion //SupportsShowDialog

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region Pane

		private static readonly DependencyPropertyKey PanePropertyKey =
			DependencyProperty.RegisterReadOnly("Pane",
			typeof(SplitPane), typeof(PaneToolWindow), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="Pane"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PaneProperty =
			PanePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the root split pane contained within the tool window.
		/// </summary>
		/// <seealso cref="PaneProperty"/>
		//[Description("Returns the root split pane contained within the tool window.")]
		//[Category("DockManager Properties")] // Behavior
		[Bindable(true)]
		[ReadOnly(true)]
		public SplitPane Pane
		{
			get
			{
				return (SplitPane)this.GetValue(PaneToolWindow.PaneProperty);
			}
		}

		#endregion //Pane

		// AS 6/24/11 FloatingWindowCaptionSource
		#region FloatingWindowCaptionSource

		/// <summary>
		/// Identifies the <see cref="FloatingWindowCaptionSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FloatingWindowCaptionSourceProperty = XamDockManager.FloatingWindowCaptionSourceProperty.AddOwner(
			typeof(PaneToolWindow), 
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFloatingWindowCaptionSourceChanged)));

		private static void OnFloatingWindowCaptionSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tw = d as PaneToolWindow;

			tw.CoerceValue(UseOSNonClientAreaProperty);

			tw.VerifyCaptionVisibility();

			var pane = tw.SinglePane;

			if (null != pane)
				pane.VerifyHeaderVisibility();
		}

		/// <summary>
		/// Returns or sets a value indicating if the PaneToolWindow should display the header of the ContentPane when only a single ContentPane is visible instead of displaying the title of the floating window.
		/// </summary>
		/// <seealso cref="FloatingWindowCaptionSourceProperty"/>
		[Bindable(true)]
		public FloatingWindowCaptionSource FloatingWindowCaptionSource
		{
			get
			{
				return (FloatingWindowCaptionSource)this.GetValue(PaneToolWindow.FloatingWindowCaptionSourceProperty);
			}
			set
			{
				this.SetValue(PaneToolWindow.FloatingWindowCaptionSourceProperty, value);
			}
		}

		#endregion //FloatingWindowCaptionSource

		#endregion //Public Properties

		#region Internal Properties

		#region CaptionElement
		internal new FrameworkElement CaptionElement
		{
			get { return this._captionElement; }
		} 
		#endregion //CaptionElement

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// Previously when a pane was dragged, we would focus the dockmanager (or
		// more accurately a FCE it referenced). This was done to mimic the VS 
		// 2008 and prior behavior. Well this causes a problem when the windows 
		// are not owned as the window being dragged could go behind the main 
		// window containing the dockmanager.
		// 
		#region DragHelper
		internal FrameworkContentElement DragHelper
		{
			get
			{
				if (_dragHelper == null)
				{
					_dragHelper = new FrameworkContentElement();

					// Set the name to make it easier to identify when debugging, bug reports, etc.
					_dragHelper.Name = "PaneToolWindowDragHelper";

					// We may need to put focus into this element temporarily.
					_dragHelper.SetValue(FrameworkContentElement.FocusableProperty, KnownBoxes.TrueBox);

					this.AddLogicalChild(_dragHelper);
				}

				return _dragHelper;
			}
		}
		#endregion //DragHelper

		// AS 9/20/11 TFS88634
		#region ForceRemainOpen
		internal bool ForceRemainOpen
		{
			get { return _forceRemainOpen; }
			set
			{
				_forceRemainOpen = value;

				this.CoerceValue(UIElement.VisibilityProperty);

				// if we are no longer forcing the window to remain open we may need to 
				// explicitly close the window
				if (!value)
				{
					if (this.Pane == null || this.Pane.Visibility == System.Windows.Visibility.Collapsed)
					{
						this.Close();
					}
				}
			}
		} 
		#endregion //ForceRemainOpen

		// AS 5/22/08 BR33278
		#region IsClosing
		internal bool IsClosing
		{
			get { return this._isClosing; }
		} 
		#endregion //IsClosing

		#region IsWindowLoaded
		/// <summary>
		/// Property used to control when the <see cref="XamDockManager.ToolWindowLoaded"/> and 
		/// <see cref="XamDockManager.ToolWindowUnloaded"/> events are fired.
		/// </summary>
		internal bool IsWindowLoaded
		{
			get { return this._isWindowLoaded; }
			set
			{
				if (value != this._isWindowLoaded)
				{
					this._isWindowLoaded = value;

					XamDockManager dockManager = XamDockManager.GetDockManager(this);

					Debug.Assert(null != dockManager);

					if (null != dockManager)
					{
						PaneToolWindowEventArgs args = new PaneToolWindowEventArgs(this);

						if (value)
						{
							// AS 4/28/11 TFS73532
							// Wait until the window is to be shown.
							//
							//dockManager.RaiseToolWindowLoaded(args);
							Debug.Assert(_isRaiseWindowLoadedPending == false, "We're already pending a loaded event?");

							SplitPane split = this.Content as SplitPane;

							// show the window if possible
							// AS 5/5/10 TFS30072
							// The host should be shown even if the visibility is hidden - its just that the 
							// root split pane and its contents would not be shown.
							//
							//if (null != split && split.Visibility == Visibility.Visible)
							_isRaiseWindowLoadedPending = true;

							// AS 6/23/11 TFS73499
							// Our visibility is bound to the split but we also could have our visibility changed to collapsed.
							//
							//if (null != split && split.Visibility != Visibility.Collapsed)
							// AS 7/5/11 TFS80666
							//if (this.Visibility != Visibility.Collapsed)
							if (this.Visibility != Visibility.Collapsed && null != split && split.Visibility != Visibility.Collapsed)
							{
								// AS 4/28/11 TFS73532
								// Use a helper since we may need to defer the actual showing
								//
								//// AS 10/15/08 TFS6271
								////this.Show(dockManager, false);
								//DockManagerUtilities.ShowToolWindow(this, dockManager, false);
								this.ShowHelper();
							}
							// AS 4/28/11 TFS73532
							// Just set the flag so we can raise the event.
							//
							else
							{
								Debug.Assert(split != null);

								if (_syncShowCount <= 0)
									this.RaisePendingToolWindowLoaded();
							}
						}
						else
						{
							// AS 4/28/11 TFS73532
							if (_isRaiseWindowLoadedPending)
							{
								_isRaiseWindowLoadedPending = false;
								return;
							}

							dockManager.RaiseToolWindowUnloaded(args);

							// AS 3/2/12 TFS103671
							// When we unload the window and it has an owner then we should 
							// close the window. We will call Show again when it is loaded next.
							//
							if (!this.IsClosing && !this.IsWindowLoaded && this.Owner != null)
								this.Close();
						}
					}
				}
			}
		}
		#endregion //IsWindowLoaded

		#region LastActivePane
		/// <summary>
		/// Returns/sets the pane that is considered active within the PaneToolWindow.
		/// </summary>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal ContentPane LastActivePane
		{
			get { return this._activePane; }
			set 
			{
				Debug.Assert(value == null || ToolWindow.GetToolWindow(value) == this || ToolWindow.GetToolWindow(value) == null);

				this._activePane = value;
			}
		}
		#endregion //LastActivePane

		#region SinglePane
		internal ContentPane SinglePane
		{
			get { return this._singlePane; }
			private set
			{
				if (value != this._singlePane)
				{
					ContentPane oldPane = this._singlePane;

					this._singlePane = value;

					// if the pane was our single pane content and is not that of another window
					// then clear the setting on the old one
					if (null != oldPane && GetSinglePaneToolWindow(oldPane) == this)
						oldPane.ClearValue(SinglePaneToolWindowPropertyKey);

					if (null != this._singlePane)
						this._singlePane.SetValue(SinglePaneToolWindowPropertyKey, this);

					// AS 6/24/11 FloatingWindowCaptionSource
					this.VerifyCaptionVisibility();
				}
			}
		} 
		#endregion //SinglePane

		#region SinglePaneToolWindow

		private static readonly DependencyPropertyKey SinglePaneToolWindowPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("SinglePaneToolWindow",
			typeof(PaneToolWindow), typeof(PaneToolWindow), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSinglePaneToolWindowChanged)));

		private static void OnSinglePaneToolWindowChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ContentPane pane = d as ContentPane;

			Debug.Assert(pane != null, "This is only supposed to be used on ContentPanes!");

			if (null == pane)
				return;

			PaneToolWindow newWindow = e.NewValue as PaneToolWindow;
			PaneToolWindow oldWindow = e.OldValue as PaneToolWindow;

			if (oldWindow != null)
			{
				Debug.Assert(d.Equals(oldWindow.SinglePane) == false);

				// unbind the title
				BindingOperations.ClearBinding(oldWindow, TitleProperty);
			}

			if (newWindow != null)
			{
				Debug.Assert(pane == newWindow.SinglePane);

                // AS 2/11/09 TFS12849
                // Moved to a helper method since the pane may need to update this 
                // should the header change.
                //
                //newWindow.SetBinding(TitleProperty, Utilities.CreateBindingObject(ContentPane.HeaderProperty, BindingMode.OneWay, pane));
                newWindow.RefreshSinglePaneTitle();
			}

			pane.VerifyHeaderVisibility();
		}

		/// <summary>
		/// Identifies the SinglePaneToolWindow" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetSinglePaneToolWindow"/>
		internal static readonly DependencyProperty SinglePaneToolWindowProperty =
			SinglePaneToolWindowPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the PaneToolWindow for which a ContentPane represents the sole content. In this case the 
		/// title of the pane is hidden and the window's title is bound to that of the pane.
		/// </summary>
		/// <seealso cref="SinglePaneToolWindowProperty"/>
		internal static PaneToolWindow GetSinglePaneToolWindow(DependencyObject d)
		{
			return (PaneToolWindow)d.GetValue(PaneToolWindow.SinglePaneToolWindowProperty);
		}

		#endregion //SinglePaneToolWindow

		#endregion //Internal Properties

		#region Private Properties

		#region FloatingLocation

		/// <summary>
		/// Identifies the <see cref="FloatingLocation"/> dependency property
		/// </summary>
		private static readonly DependencyProperty FloatingLocationProperty = XamDockManager.FloatingLocationProperty.AddOwner(typeof(PaneToolWindow),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFloatingLocationChanged)));

		private static void OnFloatingLocationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PaneToolWindow)d).SynchronizeLocation(false);
		}

		/// <summary>
		/// Returns/sets the current location of the window.
		/// </summary>
		/// <seealso cref="FloatingLocationProperty"/>
		private Point? FloatingLocation
		{
			get
			{
				return (Point?)this.GetValue(PaneToolWindow.FloatingLocationProperty);
			}
			set
			{
				this.SetValue(PaneToolWindow.FloatingLocationProperty, value);
			}
		}

		#endregion //FloatingLocation

		#region FloatingSize

		/// <summary>
		/// Identifies the <see cref="FloatingSize"/> dependency property
		/// </summary>
		private static readonly DependencyProperty FloatingSizeProperty = XamDockManager.FloatingSizeProperty.AddOwner(typeof(PaneToolWindow),
			new FrameworkPropertyMetadata(new PropertyChangedCallback(OnFloatingSizeChanged)));

		private static void OnFloatingSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PaneToolWindow)d).SynchronizeSize(false);
		}

		/// <summary>
		/// Returns/sets the current size of the window.
		/// </summary>
		/// <seealso cref="FloatingSizeProperty"/>
		private Size FloatingSize
		{
			get
			{
				return (Size)this.GetValue(PaneToolWindow.FloatingSizeProperty);
			}
			set
			{
				this.SetValue(PaneToolWindow.FloatingSizeProperty, value);
			}
		}

		#endregion //FloatingSize

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddNonClientSize
		internal Size AddNonClientSize(Size arrangeBounds)
		{
			return this.CalculateNonClientSize(arrangeBounds);
		}
		#endregion //AddNonClientSize 

		// AS 12/1/11 TFS96843
		// If the FocusManager.FocusedElement of the toolwindow's FocusScope is its DragHelper then when it is 
		// given the KeyboardFocus then it will activate its currently active pane. That can happen while 
		// attempting to focus another element within the window when keyboard focus is within an hwnd and wpf 
		// tries to acquire keyboard focus within its hwnd. If that were to happen then the PaneToolWindow would 
		// shift focus to the currently active pane. That is what was happening in this case. In the Activate 
		// of a pane within the PaneToolWindow, it was calling Focus on itself. Within that call WPF was trying 
		// to acquire focus and as part of that they set focus to the DragHelper since that was the FocusedElement 
		// of the FocusScope.
		//
		#region ClearDragHelperFocusedElement
		/// <summary>
		/// Helper method to ensure that the FocusedElement of the focusscope of the toolwindow is not its drag helper.
		/// </summary>
		internal void ClearDragHelperFocusedElement()
		{
			if (_dragHelper == null)
				return;

			if (_dragHelper.IsKeyboardFocused)
				return;

			DependencyObject focusScope = FocusManager.GetFocusScope(_dragHelper);

			if (focusScope == null || FocusManager.GetFocusedElement(focusScope) != _dragHelper)
				return;

			// AS 2/16/12 TFS101691
			// We should reset the focusedelement to the PaneToolWindow which is what it used to 
			// be before the DragHelper became the focused element. Setting it to null is causing 
			// some issues with the routed commands. I also changed this to use the dockmanager's 
			// helper method which should avoid actual keyboard focus from being shifted.
			//
			//FocusManager.SetFocusedElement(focusScope, null);
			if (Keyboard.FocusedElement == _dragHelper)
				FocusManager.SetFocusedElement(focusScope, this);
			else
				DockManagerUtilities.SetFocusedElement(focusScope, this);
		} 
		#endregion //ClearDragHelperFocusedElement

        // AS 2/11/09 TFS12849
        // The FrameworkElement defines a method named GetPlainText. The TextBlock overrides this to return 
        // its text but it does not override the ToString to return the GetPlainText as the Control class 
        // does. So the binding of the Header object property to the string Title property gets handled by 
        // the wpf framework's default converter which essentially ends up doing a tostring on the textblock.
        //
        #region RefreshSinglePaneTitle
        internal void RefreshSinglePaneTitle()
        {
            ContentPane singlePane = this.SinglePane;

            if (null != singlePane)
            {
                if (singlePane.Header is TextBlock)
                    this.SetBinding(TitleProperty, Utilities.CreateBindingObject(TextBlock.TextProperty, BindingMode.OneWay, singlePane.Header));
                else
                    this.SetBinding(TitleProperty, Utilities.CreateBindingObject(ContentPane.HeaderProperty, BindingMode.OneWay, singlePane));
            }
            else
                this.ClearValue(TitleProperty);
        } 
        #endregion //RefreshSinglePaneTitle

		#region RefreshToolWindowState
		internal void RefreshToolWindowState()
		{
			// AS 5/19/08 Reuse Group/Split
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null != dockManager && dockManager.IsLoadingLayout)
				return;

			// AS 6/24/11 FloatingWindowCaptionSource
			if (_suspendReshowCount != 0)
				return;

			SplitPane rootPane = this.Content as SplitPane;

			// there's two things that we need to know here.
			// 1) can we allow being closed - this is based on whether we have at least
			//	one contentpane whose allowclose is true
			// 2) do we have a single pane caption? if so, then we want to hide its caption
			//  and show the header in the title of the window. this happens if we have only
			//  1 contentpane but also if we have only 1 tabwindowgroup since only 1 pane
			//  in the group shows its caption
			//

			// AS 6/23/11 TFS73499
			PaneFilterFlags flags = PaneFilterFlags.AllVisible | PaneFilterFlags.IgnoreFloatingWindowVisibility;

			// get the first content pane
			ContentPane firstPane = DockManagerUtilities.GetFirstLastPane(rootPane, true, flags );

			// if we got one then get the last content pane
			ContentPane lastPane = firstPane != null ? DockManagerUtilities.GetFirstLastPane(rootPane, false, flags ) : null;

			ContentPane singlePane = null;

			// if we have 2 different panes then see if they're from the same group
			if (firstPane != null && lastPane != null && firstPane != lastPane)
			{
				IPaneContainer firstContainer = DockManagerUtilities.GetParentPane(firstPane);
				IPaneContainer lastContainer = DockManagerUtilities.GetParentPane(lastPane);

				if (firstContainer == lastContainer && firstContainer is TabGroupPane)
				{
					// AS 5/14/08 
					// Assert is invalid. This could happen when a pane is being moved to another location.
					//
					//Debug.Assert(((TabGroupPane)firstContainer).SelectedItem != null);

					singlePane = ((TabGroupPane)firstContainer).SelectedItem as ContentPane;
				}
			}

			// if we didn't have a single tabgroup and we have only 1 pane then use that
			if (null == singlePane && firstPane == lastPane)
				singlePane = firstPane;

			ContentPane pane = firstPane;
			bool allowClose = false;

			while (pane != null)
			{
				// as long as we have 1 closable pane...
				if (pane.AllowClose)
				{
					allowClose = true;
					break;
				}

				// get the next pane
				pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, rootPane, flags );
			}

			this.SinglePane = singlePane;
			this.AllowClose = allowClose;

			this.VerifyActivePane();

			// AS 9/11/09 TFS21330
			// When a pane is made visible, etc. then we may need to reshow the window 
			// if the current AllowsTransparency state can't be used.
			//
			this.VerifyPreventAllowsTransparency();
		}
		#endregion //RefreshToolWindowState

		// AS 4/28/11 TFS73532
		#region ResumeSyncShow
		/// <summary>
		/// Resumes the synchronous showing of the window. If a show had been initiated while suspended and the window should be displayed it will be when the count goes to 0.
		/// </summary>
		internal void ResumeSyncShow()
		{
			Debug.Assert(_syncShowCount > 0, "Resuming more than suspending?");

			_syncShowCount--;

			if (_syncShowCount == 0)
			{
				if (_pendingSyncShow != null)
				{
					Debug.Assert(_suspendReshowCount == 0, "We have a suspend on the reshow?");

					IDisposable old = _pendingSyncShow;
					_pendingSyncShow = null;

					//Debug.Assert(null != this.Owner, "No owner associated with the window?");
					SplitPane rootSplit = this.Content as SplitPane;

					this.RaisePendingToolWindowLoaded();

					if (this.IsWindowLoaded && rootSplit != null && rootSplit.Visibility != System.Windows.Visibility.Collapsed)
					{
						// do the preparation we would have done with a normal show
						XamDockManager dm;
						DependencyObject focusedElement;
						DockManagerUtilities.BeforeShowToolWindow(this, this.Owner, out focusedElement, out dm);

						// then dispose the object returned from the ShowAsync which will show the window
						old.Dispose();
					}
					else
					{
						this.Close();
					}
				}
				else
				{
					this.RaisePendingToolWindowLoaded();
				}
			}
		} 
		#endregion //ResumeSyncShow

		// AS 4/28/11 TFS73532
		#region SuspendSyncShow
		/// <summary>
		/// Suspends the synchronous showing of the window allowing the window to be created if needed and only when the resume count is 0 will the window be shown.
		/// </summary>
		internal void SuspendSyncShow()
		{
			_syncShowCount++;
		}
		#endregion //SuspendSyncShow

		// AS 9/11/09 TFS21330
		#region VerifyPreventAllowsTransparency
		internal void VerifyPreventAllowsTransparency()
		{
			if (_isReshowing)
				return;

			if (_pendingVerifyAllowsTransparency == null)
			{
				// since we could be in the middle of processing changes, we need to asynchronously verify 
				// the PreventAllowsTransparency state.
				//
				_pendingVerifyAllowsTransparency = this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DockManagerUtilities.MethodInvoker(VerifyPreventAllowsTransparencyImpl));
			}
		}

		private void VerifyPreventAllowsTransparencyImpl()
		{
			if (_isReshowing)
				return;

			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm == null)
				return;

			// make sure we clear the dispatcher operation
			_pendingVerifyAllowsTransparency = null;

			if (this.IsVisible == false)
				return;

			_isReshowing = true;

			try
			{
				bool shouldPreventTransparency = DockManagerUtilities.CalculatePreventAllowsTransparency(this);

				// if we have been shown supporting transparency and we now need to 
				// prevent transparency then we need to hide the window and reshow it
				if (this.IsVisible &&
					shouldPreventTransparency &&
					!DockManagerUtilities.GetPreventAllowsTransparency(this))
				{
					this.SetValue(DockManagerUtilities.PreventAllowsTransparencyPropertyKey, KnownBoxes.TrueBox);

					this.OnSupportsAllowsTransparencyChanged();
				}
			}
			finally
			{
				_isReshowing = false;
			}
		}
		#endregion //VerifyPreventAllowsTransparency

		#endregion //Internal Methods

		#region Private Methods

		// AS 6/24/11 FloatingWindowCaptionSource
		#region CoerceUseOSNonClientArea
		private static object CoerceUseOSNonClientArea(DependencyObject d, object newValue)
		{
			var tw = d as PaneToolWindow;

			if (tw.FloatingWindowCaptionSource == FloatingWindowCaptionSource.UseContentPaneCaption)
				return KnownBoxes.FalseBox;

			return newValue;
		}
		#endregion //CoerceUseOSNonClientArea

		// AS 6/23/11 TFS73499
		#region CoerceVisibility
		private static object CoerceVisibility(DependencyObject d, object newValue)
		{
			var dm = XamDockManager.GetDockManager(d);

			PaneToolWindow tw = d as PaneToolWindow;

			// AS 9/20/11 TFS88634
			// If we are supposed to remain open and we would otherwise have been closed 
			// we'll act as though we are in a hidden state.
			//
			if (tw._forceRemainOpen &&
				tw.Host != null &&
				tw.Host.IsWindow &&
				(tw.Pane == null || tw.Pane.Visibility == Visibility.Collapsed))
			{
				return KnownBoxes.VisibilityHiddenBox;
			}

			if (null != dm)
			{
				newValue = dm.GetValue(XamDockManager.ComputedFloatingWindowVisibilityProperty);
			}

			return newValue;
		} 
		#endregion //CoerceVisibility

		// AS 5/4/10 TFS31317
		#region OnCanExecuteCommand
		private static void OnCanExecuteCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			PaneToolWindow tw = sender as PaneToolWindow;
			XamDockManager dm = XamDockManager.GetDockManager(tw);
			if (dm != null)
			{
				RoutedCommand rc = e.Command as RoutedCommand;
				if (null != rc)
				{
					// if the CanExecute reaches the ToolWindow then nothing has    
					// handled it. we'll delegate the canexecute call to the dockmanager   
					// and let it bubble up from there   
					e.CanExecute = rc.CanExecute(e.Parameter, dm);
					e.ContinueRouting = false;
					e.Handled = true;
				}
			}
		} 
		#endregion //OnCanExecuteCommand

		#region OnChildVisibilityChanged
		private static void OnChildVisibilityChanged(object sender, RoutedEventArgs e)
		{
			PaneToolWindow window = (PaneToolWindow)sender;

			// AS 8/22/11 TFS84648
			// The developer may be changing the element tree while the toolwindow loaded is being invoked 
			// and we don't want to show/close the window at that point since we are already in the process 
			// of showing it.
			//
			if (window._isRaisingToolWindowLoaded)
				return;

			SplitPane split = window.Content as SplitPane;

			// AS 5/5/10 TFS30072
			// The host should be shown even if the visibility is hidden - its just that the 
			// root split pane and its contents would not be shown.
			//
			//if (split.Visibility == Visibility.Visible)
			if (split != null && split.Visibility != Visibility.Collapsed)
			{
				
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				if (!window.ShowIfRootIsVisible())
					return;
			}
			else if (window._isClosing == false)
			{
				// AS 6/24/11 FloatingWindowCaptionSource
				// Added if block - do not close the window while we are processing a drop.
				//
				// AS 9/20/11 TFS88634
				// Added _syncShowCount check. We shouldn't close while a pending sync is in progress.
				//
				if (window._suspendReshowCount == 0 && window._syncShowCount == 0)
					window.Close();
			}

			e.Handled = true;
		}
		#endregion // OnChildVisibilityChanged

		#region OnDockManagerChanged
		private static void OnDockManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// find out when the animation type changes
			if (e.NewValue != null)
				BindingOperations.SetBinding(d, ToolWindow.ThemeProperty, Utilities.CreateBindingObject(XamDockManager.ThemeProperty, BindingMode.OneWay, e.NewValue));
			else
				BindingOperations.ClearBinding(d, ToolWindow.ThemeProperty);
		}
		#endregion //OnDockManagerChanged

		// AS 5/4/10 TFS31317
		#region OnExecuteCommand
		private static void OnExecuteCommand(object sender, ExecutedRoutedEventArgs e)
		{
			PaneToolWindow tw = sender as PaneToolWindow;
			XamDockManager dm = XamDockManager.GetDockManager(tw);
			if (dm != null)
			{
				RoutedCommand rc = e.Command as RoutedCommand;
				if (null != rc)
				{
					// if the Execute reaches the ToolWindow then nothing has    
					// handled it. we'll delegate the canexecute call to the dockmanager  
					// and let it bubble up from there   
					rc.Execute(e.Parameter, dm);
					e.Handled = true;
				}
			}
		}
		#endregion //OnExecuteCommand

		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			if (this.IsVisible)
				this.SynchronizeContentPaneFloatingRect();
		}
		#endregion //OnLayoutUpdated

		#region OnTopLeftChanged
		private static void OnTopLeftChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PaneToolWindow)d).SynchronizeLocation(true);
		}
		#endregion //OnTopLeftChanged

		// AS 6/23/11 TFS73499
		#region OnVisibilityChanged
		private static void OnVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tw = d as PaneToolWindow;

			if (Visibility.Collapsed.Equals(e.OldValue) && e.OldValue != e.NewValue)
				tw.ShowIfRootIsVisible();
		}
		#endregion //OnVisibilityChanged

		#region OnWidthHeightChanged
		private static void OnWidthHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((PaneToolWindow)d).SynchronizeSize(true);
		}
		#endregion //OnWidthHeightChanged

		// AS 4/28/11 TFS73532
		#region RaisePendingToolWindowLoaded
		private void RaisePendingToolWindowLoaded()
		{
			if (!_isRaiseWindowLoadedPending)
				return;

			_isRaiseWindowLoadedPending = false;

			XamDockManager dm = XamDockManager.GetDockManager(this);

			if (dm != null)
			{
				// AS 8/22/11 TFS84648
				bool wasRaising = _isRaisingToolWindowLoaded;
				_isRaisingToolWindowLoaded = true;

				try
				{
					dm.RaiseToolWindowLoaded(new PaneToolWindowEventArgs(this));
				}
				finally
				{
					_isRaisingToolWindowLoaded = wasRaising;
				}
			}
		}
		#endregion //RaisePendingToolWindowLoaded

		// AS 9/11/09 TFS21330
		#region ResumeReshow
		private void ResumeReshow()
		{
			_suspendReshowCount--;

			if (_suspendReshowCount == 0)
			{
				// AS 6/24/11 FloatingWindowCaptionSource
				//this.ShowIfRootIsVisible();
				OnChildVisibilityChanged(this, new RoutedEventArgs(DockManagerUtilities.VisibilityChangedEvent));
				this.RefreshToolWindowState();
			}
		}
		#endregion //ResumeReshow

		// AS 4/28/11 TFS73532
		#region ShowHelper
		private void ShowHelper()
		{
			if (_pendingSyncShow != null)
				return;

			XamDockManager dm = XamDockManager.GetDockManager(this);
			Debug.Assert(null != dm, "No reference to a dockmanager?");

			if (_syncShowCount <= 0)
			{
				this.RaisePendingToolWindowLoaded();

				DockManagerUtilities.ShowToolWindow(this, dm, false);
			}
			else
			{
				_pendingSyncShow = this.ShowAsync(dm, false, dm.ShowToolWindowsInPopup);
			}
		}
		#endregion //ShowHelper

		// AS 9/11/09 TFS21330
		// Refactored from the OnChildVisibilityChanged method.
		//
		#region ShowIfRootIsVisible
		private bool ShowIfRootIsVisible()
		{
			if (this.IsVisible)
				return false;

			// AS 9/11/09 TFS21330
			// If we're waiting for all the panes to be added then don't reshow it yet.
			//
			if (_suspendReshowCount > 0)
				return false;

			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			// AS 5/10/10 TFS30072
			if (this.Owner == dockManager)
				return false;

			// AS 4/30/08
			// We do not want to show the floating window unless the toolwindowloaded has
			// been fired - which should happen when the is loaded. If the visibility 
			// changes such that the window would be shown but the dm is not loaded/shown
			// then we really don't want to show the floating window yet. The dm will
			// trigger that when it is loaded then.
			//
			if (null != dockManager && dockManager.IsLoaded && this.IsWindowLoaded)
			{
				SplitPane rootSplit = this.Pane;

				// AS 7/7/11 TFS80885
				if (null != rootSplit && rootSplit.Visibility == Visibility.Collapsed)
					return false;

				// AS 4/28/11 TFS73532
				//// AS 10/15/08 TFS6271
				////window.Show(window.Owner ?? dockManager, false);
				//DockManagerUtilities.ShowToolWindow(this, this.Owner ?? dockManager, false);
				this.ShowHelper();
			}

			return true;
		}
		#endregion //ShowIfRootIsVisible

		// AS 9/11/09 TFS21330
		#region SuspendReshow
		private void SuspendReshow()
		{
			_suspendReshowCount++;
		}
		#endregion //SuspendReshow

		#region SynchronizeContentPaneFloatingRect
		private void SynchronizeContentPaneFloatingRect()
		{
			// AS 2/17/12 TFS100638
			// We really shouldn't be storing the size information when the toolwindow is calcuating it 
			// minimum size.
			//
			if (this.IsCalculatingSizeWithoutContent)
				return;

			SplitPane root = this.Content as SplitPane;

			if (null != root)
			{
				// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
				if (this.WindowState != WindowState.Normal)
					return;

				Size floatingSize = new Size(this.Width, this.Height);

				bool usingOSNonClientArea = this.IsUsingOSNonClientArea;

				if (usingOSNonClientArea)
					floatingSize = this.AddNonClientSize(floatingSize);

				Rect floatingRect = new Rect(this.Left, this.Top, floatingSize.Width, floatingSize.Height);

				ContentPane pane = DockManagerUtilities.GetFirstLastPane(root, true);
				XamDockManager dockManager = XamDockManager.GetDockManager(this);

				// we need to track the size of the floating window on each pane
				while (pane != null)
				{
					// AS 4/25/08
					// We also need to track where on the screen the window is positioned
					// so if we create a floating only window, we can position it at the same
					// spot.
					//
					FrameworkElement relativeElement = DockManagerUtilities.GetParentPane(pane) as TabGroupPane ?? (FrameworkElement)pane;

					if (null != relativeElement && relativeElement.IsDescendantOf(this))
					{
						Point pt = relativeElement.TransformToAncestor(this).Transform(new Point());
						pt.Offset(this.Left, this.Top);
						pane.LastFloatingLocation = pt;
					}

					// AS 8/22/11 TFS84720
					// We're initializing the LastFloatingSize in the arrange but when not 
					// letting the OS provide the non-client area that could be before we 
					// have calculated the non-client size so we wouldn't have been able to 
					// add in the non-client size earlier. As a backup we'll reinitialize it 
					// here.
					//
					if (null != relativeElement && !usingOSNonClientArea)
					{
						FrameworkElement elementForSize;

						if (relativeElement is SplitPane)
							elementForSize = pane;
						else
							elementForSize = relativeElement;

						var arrangeSize = System.Windows.Controls.Primitives.LayoutInformation.GetLayoutSlot(elementForSize).Size;

						if (!arrangeSize.IsEmpty)
						{
							arrangeSize = this.AddNonClientSize(arrangeSize);

							pane.LastFloatingSize = arrangeSize;
						}
					}

					pane.LastFloatingWindowRect = floatingRect;
					pane = DockManagerUtilities.GetNextPreviousPane(pane, false, true, root);
				}
			}
		}
		#endregion //SynchronizeContentPaneFloatingRect

		#region SynchronizeLocation
		private void SynchronizeLocation(bool updateFromTopLeft)
		{
			if (this._synchronizingLocation)
				return;

			this._synchronizingLocation = true;

			try
			{
				if (updateFromTopLeft)
				{
					this.FloatingLocation = new Point(this.Left, this.Top);
				}
				else
				{
					Point? pt = this.FloatingLocation;

					if (pt.HasValue)
					{
						this.Left = pt.Value.X;
						this.Top = pt.Value.Y;
					}
				}

				this.SynchronizeContentPaneFloatingRect();
			}
			finally
			{
				this._synchronizingLocation = false;
			}

			// AS 8/4/11 TFS83465/TFS83469
			// In case someone sets the location to something that moves it 
			// offscreen then bring it at least partially back into view.
			//
			if (!updateFromTopLeft && this.FloatingLocation != null && this.KeepOnScreen)
				this.EnsureOnScreen(false);
		}
		#endregion //SynchronizeLocation

		#region SynchronizeSize
		private void SynchronizeSize(bool updateFromActual)
		{
			if (this._synchronizingSize)
				return;

			// AS 2/17/12 TFS100638
			// Do not update our information while the window state is non-normal or in the 
			// process of being changed.
			//
			if (this.WindowState != WindowState.Normal)
				return;

			this._synchronizingSize = true;

			try
			{
				if (updateFromActual)
				{
					this.FloatingSize = new Size(this.Width, this.Height);
				}
				else
				{
					Size size = this.FloatingSize;

					this.Width = double.IsInfinity(size.Width) ? double.NaN : size.Width;
					this.Height = double.IsInfinity(size.Height) ? double.NaN : size.Height;
				}

				this.SynchronizeContentPaneFloatingRect();
			}
			finally
			{
				this._synchronizingSize = false;
			}

			// AS 8/4/11 TFS83465/TFS83469
			// In case someone sets the size such that the window moves 
			// offscreen then bring it at least partially back into view.
			//
			if (!updateFromActual && this.KeepOnScreen)
				this.EnsureOnScreen(false);
		}
		#endregion //SynchronizeSize

		#region VerifyActivePane
		private void VerifyActivePane()
		{
			// if we don't have an active pane or its elsewhere then we need a new active pane
			bool changeFocus = this._activePane == null || ToolWindow.GetToolWindow(this._activePane) != this;

			// if we contain the active pane, make sure it has focus if we have focus
			if (false == changeFocus && Keyboard.FocusedElement != null)
			{
				Debug.Assert(Keyboard.FocusedElement is DependencyObject);
				DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;

				// if we contain the focused element...
				if (null != focusedElement && Utilities.IsDescendantOf(this, focusedElement))
				{
					this.LastActivePane = focusedElement as ContentPane ?? Utilities.GetAncestorFromType(focusedElement, typeof(ContentPane), true) as ContentPane;

					// AS 5/14/08 
					// Assert is invalid. This could happen when a pane is being moved to another location.
					//
					//Debug.Assert(this._activePane == null || XamDockManager.GetDockManager(this._activePane) == XamDockManager.GetDockManager(this));

					// AS 5/14/08 
					// Added null check for activepane member variable
					//
					while (null != this._activePane && XamDockManager.GetDockManager(this._activePane) != XamDockManager.GetDockManager(this))
						this.LastActivePane = Utilities.GetAncestorFromType(this._activePane, typeof(ContentPane), true) as ContentPane;

					// AS 5/14/08 
					// Assert is invalid. This could happen when a pane is being moved to another location.
					//
					//Debug.Assert(this._activePane != null);
				}
			}
			else if (changeFocus && this.Content is SplitPane)
			{
				// AS 6/24/11 TFS79218
				// We should try to use the last pane we have that has been activated first.
				//
				//this.LastActivePane = DockManagerUtilities.GetFirstLastPane((SplitPane)this.Content, true);
				ContentPane paneToActivate = null;
				XamDockManager dm = XamDockManager.GetDockManager(this);

				if (null != dm)
				{
					foreach (ContentPane pane in dm.GetPanes(PaneNavigationOrder.ActivationOrder))
					{
						if (ToolWindow.GetToolWindow(pane) == this)
						{
							// AS 6/23/11 TFS73499
							// Skip hidden panes.
							//
							if (!DockManagerUtilities.MeetsCriteria(pane, PaneFilterFlags.AllVisible | PaneFilterFlags.IgnoreFloatingWindowVisibility))
								continue;

							paneToActivate = pane;
							break;
						}
					}
				}

				if (paneToActivate == null)
				{
					// AS 6/23/11 TFS73499
					PaneFilterFlags flags = PaneFilterFlags.AllVisible | PaneFilterFlags.IgnoreFloatingWindowVisibility;

					paneToActivate = DockManagerUtilities.GetFirstLastPane((SplitPane)this.Content, true, flags );
				}

				// if that pane happens to be in a tab group then prefer the selected pane
				if (null != paneToActivate)
				{
					TabGroupPane tgp = LogicalTreeHelper.GetParent(paneToActivate) as TabGroupPane;

					if (null != tgp && tgp.SelectedItem is ContentPane && ((ContentPane)tgp.SelectedItem).CanActivate)
						paneToActivate = tgp.SelectedItem as ContentPane;
				}

				this.LastActivePane = paneToActivate;
			}
		}
		#endregion //VerifyActivePane

		// AS 6/24/11 FloatingWindowCaptionSource
		#region VerifyCaptionVisibility
		private void VerifyCaptionVisibility()
		{
			var caption = this.CaptionElement;

			if (null != caption && caption.IsMeasureValid)
			{
				var parent = VisualTreeHelper.GetParent(caption) as UIElement;

				if (parent != null && parent.IsMeasureValid == false)
				{
					// if the caption has been measured but the parent is dirty then 
					// changing the state now may cause wpf to try and invalidate the 
					// measure of the parent but that won't do anything if the parent 
					// in hte middle of measuring its children
					this.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new MethodInvoker(this.VerifyCaptionVisibilityImpl));
					return;
				}
			}

			this.VerifyCaptionVisibilityImpl();
		}

		private void VerifyCaptionVisibilityImpl()
		{
			if (this.FloatingWindowCaptionSource == FloatingWindowCaptionSource.UseContentPaneCaption && this.SinglePane != null)
			{
				this.CaptionVisibility = Visibility.Collapsed;
			}
			else
			{
				this.CaptionVisibility = Visibility.Visible;
			}
		}
		#endregion //VerifyCaptionVisibility

		#endregion //Private Methods

		#endregion //Methods

		#region ICommandHost Members

		bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			return ((ICommandHost)dockManager).CanExecute(commandInfo);
		}

		// SSP 3/18/10 TFS29783 - Optimizations
		// 
		//long ICommandHost.CurrentState
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			XamDockManager dockManager = XamDockManager.GetDockManager( this );

			if ( null == dockManager )
				return 0;

			// SSP 3/18/10 TFS29783 - Optimizations
			// 
			//return ( (ICommandHost)dockManager ).CurrentState;
			return ( (ICommandHost)dockManager ).GetCurrentState( statesToQuery );
		}

		bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
		{
			XamDockManager dockManager = XamDockManager.GetDockManager(this);

			if (null == dockManager)
				return false;

			return ((ICommandHost)dockManager).Execute(commandInfo);
		}

		#endregion //ICommandHost Members

		#region IPaneContainer Members

		IList IPaneContainer.Panes
		{
			get 
			{
				if (this.Content == null)
					return new SplitPane[0];

				return new SplitPane[] { this.Content as SplitPane }; 
			}
		}

		bool IPaneContainer.RemovePane(object pane)
		{
			Debug.Assert(pane == this.Content);

			if (pane == this.Content)
			{
				XamDockManager dockManager = XamDockManager.GetDockManager(this);

				if (null != dockManager)
				{
					dockManager.Panes.Remove((SplitPane)this.Content);
					return true;
				}
			}

			return false;
		}

		bool IPaneContainer.CanBeRemoved
		{
			// AS 5/17/08 BR32346
			// Let the split pane decide. We don't need to check the toolwindow since we create that.
			//
			//get { return this.Content == null || ((SplitPane)this.Content).Panes.Count == 0; }
			get { return this.Content == null || ((IPaneContainer)this.Content).CanBeRemoved; }
		}
		#endregion

		// AS 6/4/08 BR33512
		#region OwnerNameScope class
        // AS 2/18/09 TFS7941
        // Moved the impl into a base class in windows.
        //
		//private class OwnerNameScope : INameScope
		private class OwnerNameScope : NameScopeProxy
		{
			#region Member Variables

			private PaneToolWindow _toolWindow;

			#endregion // Member Variables

			#region Constructor
			internal OwnerNameScope(PaneToolWindow toolWindow)
			{
				this._toolWindow = toolWindow;
			}
			#endregion // Constructor

            #region Refactored
            
#region Infragistics Source Cleanup (Region)

















































#endregion // Infragistics Source Cleanup (Region)

            #endregion //Refactored

            // AS 2/18/09 TFS7941
            #region StartingElement
            protected override DependencyObject StartingElement
            {
                get { return XamDockManager.GetDockManager(this._toolWindow); }
            } 
            #endregion //StartingElement
        } 
        #endregion // OwnerNameScope class

		// AS 9/11/09 TFS21330
		// I noticed while debugging this that when creating a grouping around a pane in 
		// a floating window the floating window may be closed temporarily but it is then 
		// shown while in the middle of adding the panes. Instead we should wait until 
		// the operation (e.g. adding panes) is complete. Aside from being best for 
		// someone listening to the events it also allows the show to correctly calculate
		// the preventsallowstransparency state based on all the panes that are being added 
		// to the toolwindow as opposed to just basing it on the first pane being added.
		//
		#region SuspendReshowHelper class
		internal class SuspendReshowHelper : IDisposable
		{
			#region Member Variables

			private PaneToolWindow _window;

			#endregion //Member Variables

			#region Constructor
			internal SuspendReshowHelper(FrameworkElement toolWindowChild)
			{
				_window = ToolWindow.GetToolWindow(toolWindowChild) as PaneToolWindow;

				if (null != _window)
					_window.SuspendReshow();
			}
			#endregion //Constructor

			#region IDisposable Members

			public void Dispose()
			{
				PaneToolWindow tw = _window;
				_window = null;

				if (tw != null)
					tw.ResumeReshow();
			}

			#endregion //IDisposable Members
		} 
		#endregion //SuspendReshowHelper class
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