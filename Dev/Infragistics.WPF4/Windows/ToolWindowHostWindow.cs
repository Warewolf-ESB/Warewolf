//#define DEBUG_SIZING
//#define DEBUG_RELATIVEPOS
//#define DEBUG_WNDPROC
//#define DEBUG_RESTOREBOUNDS







using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Infragistics.Windows.Controls
{
	// AS 3/19/08 NA 2008 Vol 1 - XamDockManager
	/// <summary>
	/// Custom window used to host a <see cref="ToolWindow"/> control.
	/// </summary>
	internal class ToolWindowHostWindow : Window, IToolWindowHost
	{
		
		
		
		

		#region Member Variables

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private const int CachedIsDwmCompositionEnabledFlag = 0x1;
		//private const int IsInSendMessageFlag = 0x2;
		//private const int UpdatingRelativePositionFlag = 0x4;

		private ToolWindow _content;

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private BitVector32 _flags;
		private InternalFlags _flags;

		private IntPtr _hook;
		private NativeWindowMethods.HookProc _hookCallback;
		private int _wndProcCount;
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private bool _synchronizingLocation;

		// AS 1/20/11 Optimization
		// Since the UseSystemNonClientArea is accessed a lot and accessing a DP has some overhead, I've 
		// switched this to caching the state of the property and using that from the getter.
		//
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		//private bool _useSystemNonClientArea;





		#endregion //Member Variables

		#region Constructor
		static ToolWindowHostWindow()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(typeof(ToolWindowHostWindow)));
			Window.WindowStyleProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(WindowStyle.None));
			Window.ShowInTaskbarProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

			FrameworkElement.MaxWidthProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMaxWidth)));
			FrameworkElement.MaxHeightProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceMaxHeight)));
			FrameworkElement.WidthProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWidthChanged)));
			FrameworkElement.HeightProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnHeightChanged)));

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			ToolWindow.AllowMinimizeProperty.AddOwner(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAllowMinMaxChanged)));
			ToolWindow.AllowMaximizeProperty.AddOwner(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnAllowMinMaxChanged)));

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// There seems to be a bug in the CLR3 Window impl whereby it will update the RestoreBounds while 
			// the window is in the process of being restored but the window state is not yet updated when the 
			// Top/Left is being coerced. It sets the RestoreBounds based on the baseValue handed to the coerce.
			// In CLR4, the manipulation of the RestoreBounds was removed from the top/left coersion and instead 
			// it is updated from the property changed.
			//
			if (System.Environment.Version.Major <= 3)
			{
				ToolWindowHostWindow.TopProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(new TopLeftCoerceProxy(ToolWindowHostWindow.TopProperty, ToolWindowHostWindow.TopProperty.GetMetadata(typeof(ToolWindowHostWindow)).CoerceValueCallback).CoerceValue)));
				ToolWindowHostWindow.LeftProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(null, new CoerceValueCallback(new TopLeftCoerceProxy(ToolWindowHostWindow.LeftProperty, ToolWindowHostWindow.LeftProperty.GetMetadata(typeof(ToolWindowHostWindow)).CoerceValueCallback).CoerceValue)));
			}

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// The Window class does not always change the SizeToContent property when minimizing/maximizing
			// so as a safety we will coerce it to manual when in these states.
			//
			ToolWindowHostWindow.SizeToContentProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnSizeToContentChanged), new CoerceValueCallback(CoerceSizeToContent)));

			// AS 5/19/11 TFS75307
			ToolWindowHostWindow.WindowStateProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnWindowStateChanged)));

			// default template is just an adornerdecorator containing a contentpresenter
			// AS 4/11/08
			// When the tool window is just a simple indicator, having an adorner decorator adds unnecessary overhead.
			// It will have at least 2 extra elements - the adornerdecorator and the adornerlayer but also that adorner
			// layer will be hooked into the layout updated event. Let's put the onus on the tool window template to 
			// include the adorner layer if it wants one.
			//
			//FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(AdornerDecorator));
			//FrameworkElementFactory fefCP = new FrameworkElementFactory(typeof(ContentPresenter));
			//fefRoot.AppendChild(fefCP);
			FrameworkElementFactory fefRoot = new FrameworkElementFactory(typeof(ContentPresenter));
			ControlTemplate template = new ControlTemplate(typeof(ToolWindowHostWindow));
			template.VisualTree = fefRoot;
			template.Seal();
			Window.TemplateProperty.OverrideMetadata(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(template));
		}

		/// <summary>
		/// Initializes a new <see cref="ToolWindow"/>
		/// </summary>
		internal ToolWindowHostWindow()
		{
			this.SetFlag(InternalFlags.UseSystemNonClientArea, (bool)this.GetValue(UseSystemNonClientAreaProperty)); // AS 1/20/11 Optimization
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// This isn't being used.
			//
			//this.CachedIsDwmCompositionEnabled = NativeWindowMethods.IsDWMCompositionEnabled;
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		// We don't want to call the base implementation of this method because
		// it will size the window base on the WindowState, which is not what we want
		// since we're controlling the size of the form through the NCCALCSIZE message.
		//
		/// <summary>
		/// Overridden.  Positions child elements and determines the size of this element based on the subclassed window.
		/// </summary>
		/// <param name="arrangeBounds">The area within the parent that the element should use to arrange itself and its children.</param>
		/// <returns>The actual size used.</returns>
		protected override Size ArrangeOverride(Size arrangeBounds)
		{
			if (this.UseSystemNonClientArea)
			{
				// AS 2/2/10 TFS25694
				// We can assume that the size the window is using is the non-client size and 
				// update the client size from that if we have an explicit client size.
				//
				this.UpdateClientSizeFromNonClientSize(
					!double.IsNaN(this.ClientWidth),
					!double.IsNaN(this.ClientHeight),
					arrangeBounds.Width, arrangeBounds.Height);

				return base.ArrangeOverride(arrangeBounds);
			}

			if (this.UseSystemNonClientArea == false && this.VisualChildrenCount > 0)
			{
				// AS 7/14/09 TFS18424 [Start]
				// Similar to the fix for TFS6238/BR34682 for the xamRibbonWindow, we need to fixup the 
				// righttoleft transform if we are arranging the first child directly.
				//
				//((FrameworkElement)this.GetVisualChild(0)).Arrange(new Rect(arrangeBounds));
				FrameworkElement fe = this.GetVisualChild(0) as FrameworkElement;

				Debug.Assert(null != fe);

				fe.Arrange(new Rect(arrangeBounds));

				// AS 10/18/08 TFS6238/BR34682
				// We need to apply a layout transform on the root element. The window would have done this
				// but we don't call the base ArrangeOverride since it will use the assumed non-client area
				// to reduce the arrange bounds and measure the root child.
				//
				if (base.FlowDirection == FlowDirection.RightToLeft)
					fe.LayoutTransform = new System.Windows.Media.MatrixTransform(-1.0, 0.0, 0.0, 1.0, arrangeBounds.Width, 0.0);
				else
					fe.LayoutTransform = null;
				// AS 7/14/09 TFS18424 [End]
			}

			return arrangeBounds;
		}
		#endregion //ArrangeOverride

		#region MeasureOverrides
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			if (this.UseSystemNonClientArea)
			{
				// AS 2/2/10 TFS25694
				// We can assume that the size the window is using is the non-client size and 
				// update the client size from that if we have an explicit client size.
				//
				this.UpdateClientSizeFromNonClientSize(
					!double.IsInfinity(availableSize.Width) && !double.IsNaN(this.ClientWidth),
					!double.IsInfinity(availableSize.Height) && !double.IsNaN(this.ClientHeight),
					availableSize.Width, availableSize.Height);

				return base.MeasureOverride(availableSize);
			}

			if (this.VisualChildrenCount > 0)
			{
				UIElement element = this.GetVisualChild(0) as UIElement;

				if (null != element)
				{
					element.Measure(availableSize);
					return element.DesiredSize;
				}
			}

			return availableSize;
		}
		#endregion //MeasureOverride

		#region OnActivated
		/// <summary>
		/// Invoked when the window has been activated
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnActivated(EventArgs e)
		{
			if (this._content != null)
				this._content.SetValue(ToolWindow.IsActivePropertyKey, KnownBoxes.TrueBox);

			base.OnActivated(e);
		}
		#endregion //OnActivated

		#region OnClosing
		/// <summary>
		/// Used to raise the Closing event.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			base.OnClosing(e);

            if (null != this._content)
            {
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// In case the window is closing while in a minimized/maximized state, make sure 
				// the content has the latest restore info.
				//
				this.CacheRestoreBounds();

                // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
                //this._content.OnClosing(e);
                this._content.OnClosingInternal(e);
            }

			// AS 10/27/10 TFS36504
			// There is a bug in the OS or framework (seems like OS) where by the owner 
			// window will not be activated when the owned window is closed if a modal 
			// window was displayed while the owned window was displayed. I was originally 
			// going to clear the Owner property (or Owner of the WindowInteropHelper) 
			// but if ShowInTaskBar is false then WPF will set the Owner to an internally 
			// created hidden window and the same problem will manifest itself. Rather 
			// then setting that property to true and possibly seeing a flicker in the 
			// taskbar, I decided to clear the owner handle from the window itself.
			//
			if (!e.Cancel)
			{
				WindowInteropHelper wih = new WindowInteropHelper(this);

				if (wih.Handle != IntPtr.Zero)
				{
					NativeWindowMethods.SetWindowLong(wih.Handle, NativeWindowMethods.GWL_HWNDPARENT, IntPtr.Zero);
				}
			}
		}
		#endregion //OnClosing

		#region OnClosed
		/// <summary>
		/// Used to raise the Closed event.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnClosed(EventArgs e)
		{
			base.OnClosed(e);

			// AS 4/25/08
			// Make sure we have unhooked the layout updated event.
			//
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			if (null != this._content)
			{
				ToolWindow oldContent = this._content;
				this.Content = null;
				oldContent.OnClosedInternal();
			}
		}
		#endregion //OnClosing

		#region OnContentChanged
		/// <summary>
		/// Invoked when the <see cref="ContentControl.Content"/> property has been changed.
		/// </summary>
		/// <param name="oldContent">The previous value of the Content property</param>
		/// <param name="newContent">The new value of the Content property</param>
		protected override void OnContentChanged(object oldContent, object newContent)
		{
			base.OnContentChanged(oldContent, newContent);

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			bool wasInContentChanged = this.GetFlag(InternalFlags.IsInOnContentChanged);

			try
			{
				this.SetFlag(InternalFlags.IsInOnContentChanged, true);
				this.OnContentChangedImpl(oldContent, newContent);
			}
			finally
			{
				this.SetFlag(InternalFlags.IsInOnContentChanged, wasInContentChanged);
			}

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Now that all the properties have been hooked up (except WindowState which 
			// is done in the SourceInitialized and why we're looking at the content's 
			// property), we can copy over the location/size which we would have bailed 
			// out of updating while the IsInOnContentChanged was true.
			//
			if (_content != null && _content.WindowState == WindowState.Normal)
			{
				this.SynchronizeLocation(false);
				this.UpdateNonClientSizeFromClientSize(true, true);
			}
		}

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		private void OnContentChangedImpl(object oldContent, object newContent)
		{
			Debug.Assert(oldContent is ToolWindow || newContent is ToolWindow);

			if (this._content != null)
			{
				this._content.ClearValue(ToolWindow.HostProperty);
				this._content.ClearValue(ToolWindow.IsActivePropertyKey);
			}

			ToolWindow newToolWindow = newContent as ToolWindow;
			this._content = newToolWindow;

			if (newToolWindow != null)
			{
				newToolWindow.SetValue(ToolWindow.HostProperty, this);
				newToolWindow.SetValue(ToolWindow.IsActivePropertyKey, this.IsActive ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);

				// bind the left/top to that of the content
				// AS 4/17/08
				// We need to bind to new internal properties. When we use setwindowpos to reposition the window
				// the top/left are updated (via a coersion in the window class) but the top/left of the 
				// toolwindow properties to which those properties are bound are not updated.
				//
				//this.SetBinding(Window.LeftProperty, Utilities.CreateBindingObject(ToolWindow.LeftProperty, BindingMode.TwoWay, newToolWindow));
				//this.SetBinding(Window.TopProperty, Utilities.CreateBindingObject(ToolWindow.TopProperty, BindingMode.TwoWay, newToolWindow));
				this.SetBinding(ToolWindowHostWindow.InternalLeftProperty, Utilities.CreateBindingObject(ToolWindow.LeftProperty, BindingMode.TwoWay, newToolWindow));
				this.SetBinding(ToolWindowHostWindow.InternalTopProperty, Utilities.CreateBindingObject(ToolWindow.TopProperty, BindingMode.TwoWay, newToolWindow));

				// bind the min/max width - these will be coerce to include the non-client area if necessary
				this.SetBinding(FrameworkElement.MaxWidthProperty, Utilities.CreateBindingObject(FrameworkElement.MaxWidthProperty, BindingMode.OneWay, newToolWindow));
				this.SetBinding(FrameworkElement.MaxHeightProperty, Utilities.CreateBindingObject(FrameworkElement.MaxHeightProperty, BindingMode.OneWay, newToolWindow));
				this.SetBinding(FrameworkElement.MinWidthProperty, Utilities.CreateBindingObject(FrameworkElement.MinWidthProperty, BindingMode.OneWay, newToolWindow));
				this.SetBinding(FrameworkElement.MinHeightProperty, Utilities.CreateBindingObject(FrameworkElement.MinHeightProperty, BindingMode.OneWay, newToolWindow));

				// honor the flow direction set on the toolwindow
				this.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(FrameworkElement.FlowDirectionProperty, BindingMode.OneWay, newToolWindow));

                // AS 10/13/08 TFS6107/BR34010
                // Since the sizing can be affected by the resize mode, we should be doing this last.
                //
				//// bind the width/height of the toolwindow to that of the window
				//this.SetBinding(ToolWindowHostWindow.ClientWidthProperty, Utilities.CreateBindingObject(FrameworkElement.WidthProperty, BindingMode.TwoWay, newToolWindow));
				//this.SetBinding(ToolWindowHostWindow.ClientHeightProperty, Utilities.CreateBindingObject(FrameworkElement.HeightProperty, BindingMode.TwoWay, newToolWindow));

				// we need to know when to show/hide the non-client area
				this.SetBinding(ToolWindowHostWindow.UseSystemNonClientAreaProperty, Utilities.CreateBindingObject(ToolWindow.IsUsingOSNonClientAreaProperty, BindingMode.OneWay, newToolWindow));

				// we want to hide/show the window when the contained content is hidden/shown
				this.SetBinding(FrameworkElement.VisibilityProperty, Utilities.CreateBindingObject(FrameworkElement.VisibilityProperty, BindingMode.TwoWay, newToolWindow));

				// honor the topmost property of the toolwindow
				this.SetBinding(Window.TopmostProperty, Utilities.CreateBindingObject(ToolWindow.TopmostProperty, BindingMode.OneWay, newToolWindow));

				// AS 6/6/08 BR33640
				this.SetBinding(Window.ResizeModeProperty, Utilities.CreateBindingObject(ToolWindow.ResizeModeProperty, BindingMode.OneWay, newToolWindow));

				// AS 7/1/08 BR33583
				this.SetBinding(ToolWindowHostWindow.AllowCloseProperty, Utilities.CreateBindingObject(ToolWindow.AllowCloseProperty, BindingMode.OneWay, newToolWindow));

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				this.SetBinding(Window.ShowInTaskbarProperty, Utilities.CreateBindingObject(ToolWindow.ShowInTaskbarProperty, BindingMode.OneWay, newToolWindow));
				this.SetBinding(ToolWindow.AllowMaximizeProperty, Utilities.CreateBindingObject(ToolWindow.AllowMaximizeProperty, BindingMode.OneWay, newToolWindow));
				this.SetBinding(ToolWindow.AllowMinimizeProperty, Utilities.CreateBindingObject(ToolWindow.AllowMinimizeProperty, BindingMode.OneWay, newToolWindow));

				// we delay binding the WindowState until the OnSourceInitialized so that if the window is to 
				// be maximized it can be maximized on the correct screen. however if the source initialized 
				// has already happened then bind now
				if (new WindowInteropHelper(this).Handle != IntPtr.Zero)
				{
					// AS 5/19/11 TFS75307
					//this.SetBinding(Window.WindowStateProperty, Utilities.CreateBindingObject(ToolWindow.WindowStateProperty, BindingMode.TwoWay, newToolWindow));
					this.SetBinding(WindowStateInternalProperty, Utilities.CreateBindingObject(ToolWindow.WindowStateProperty, BindingMode.TwoWay, newToolWindow));
				}

				// because of the issues with how the command events are transferred while routing,
				// we need to make sure that our focused element is the tool window so it will
				// get a change to evaluate any canexecute/execute events.
				FocusManager.SetFocusedElement(this, newToolWindow);
			}
			else
			{
				BindingOperations.ClearBinding(this, Window.LeftProperty);
				BindingOperations.ClearBinding(this, Window.TopProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.MaxWidthProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.MinWidthProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.MaxHeightProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.MinHeightProperty);
				BindingOperations.ClearBinding(this, ToolWindowHostWindow.ClientWidthProperty);
				BindingOperations.ClearBinding(this, ToolWindowHostWindow.ClientHeightProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.FlowDirectionProperty);
				BindingOperations.ClearBinding(this, ToolWindowHostWindow.UseSystemNonClientAreaProperty);
				BindingOperations.ClearBinding(this, FrameworkElement.VisibilityProperty);
				BindingOperations.ClearBinding(this, Window.TopmostProperty);
				BindingOperations.ClearBinding(this, Window.ResizeModeProperty); // AS 6/6/08 BR33640

				// AS 7/1/08 BR33583
				BindingOperations.ClearBinding(this, ToolWindowHostWindow.AllowCloseProperty);

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				BindingOperations.ClearBinding(this, Window.ShowInTaskbarProperty);
				BindingOperations.ClearBinding(this, ToolWindow.AllowMaximizeProperty);
				BindingOperations.ClearBinding(this, ToolWindow.AllowMinimizeProperty);
				// AS 5/19/11 TFS75307
				//BindingOperations.ClearBinding(this, Window.WindowStateProperty);
				BindingOperations.ClearBinding(this, WindowStateInternalProperty);

				FocusManager.SetFocusedElement(this, null);
			}

			// we may or may not be bound to the title property depending on 
			// whether we are showing the non-client area or not
			this.VerifyTitleBinding();

			// change to toolwindow/borderless as needed based on whether we will show the non-client area
			this.VerifyWindowStyle();

            // AS 10/13/08 TFS6107/BR34010
            // Since the sizing can be affected by the resize mode, we should be doing this last.
            //
            if (null != newToolWindow)
            {
                // bind the width/height of the toolwindow to that of the window
                this.SetBinding(ToolWindowHostWindow.ClientWidthProperty, Utilities.CreateBindingObject(FrameworkElement.WidthProperty, BindingMode.TwoWay, newToolWindow));
                this.SetBinding(ToolWindowHostWindow.ClientHeightProperty, Utilities.CreateBindingObject(FrameworkElement.HeightProperty, BindingMode.TwoWay, newToolWindow));
            }

			// if the toolwindow is positioned relatively then we need to bind to the owner's rect
			this.VerifyRelativePositionBinding();
		}
		#endregion //OnContentChanged

		// AS 5/19/11 TFS75307
		#region WindowStateInternal

		private static readonly DependencyProperty WindowStateInternalProperty = DependencyProperty.Register("WindowStateInternal",
			typeof(WindowState), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(WindowState.Normal, new PropertyChangedCallback(OnWindowStateInternalChanged)));

		private static void OnWindowStateInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow window = d as ToolWindowHostWindow;

			if (window._content == null)
				return;

			WindowState newState = (WindowState)e.NewValue;

			// Instead of binding the WindowState property of the Window to that of the ToolWindow, we'll use a helper object in between 
			// so we can change the window state using the apis. Ultimately the issue is that we are manipulating the height/width based 
			// on the values with which we are being measured while the windowstate is being changed but we don't know that it is happening 
			// and the size provided is that of the maximized window even though it's no longer maximized.
			if (newState == window.WindowState)
			{
				window._content.WindowState = newState;
			}
			else
			{
				((IToolWindowHost)window).SetWindowState(newState);
			}
		}

		#endregion //WindowStateInternal

		#region OnDeactivated
		/// <summary>
		/// Invoked when the window has been deactivated
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnDeactivated(EventArgs e)
		{
			if (this._content != null)
				this._content.ClearValue(ToolWindow.IsActivePropertyKey);

			base.OnDeactivated(e);
		}
		#endregion //OnDeactivated

		#region OnKeyDown
		/// <summary>
		/// Invoked when a key is pressed down while the window has focus.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			// we could try to handle this by processing the WM_CLOSE message but that's
			// what the window class itself uses and we want to allow someone to explicitly
			// call the Close method of the window (or of the toolwindow)
			if (e.Key == Key.System
				&& e.SystemKey == Key.F4
				&& ModifierKeys.Alt == (e.KeyboardDevice.Modifiers & ModifierKeys.Alt)
				&& false == this.AllowClose)
			{
				e.Handled = true;
			}

			base.OnKeyDown(e);
		} 
		#endregion //OnKeyDown

		#region OnLocationChanged
		/// <summary>
		/// Invoked when the location of the window has changed.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnLocationChanged(EventArgs e)
		{
			base.OnLocationChanged(e);

			this.SynchronizeLocation(true);
			// AS 5/8/09 TFS17053
			// I found this while testing these changes. If you resize from the left/top then 
			// the location will change. We don't want to update the relative position until the 
			// size has been changed as well.
			//
			//this.UpdateRelativePosition();
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Send, 
				new Infragistics.Windows.Controls.ToolWindow.MethodInvoker(this.UpdateRelativePosition));
		} 
		#endregion //OnLocationChanged

		#region OnStateChanged
		/// <summary>
		/// Invoked when the <see cref="Window.WindowState"/> has changed.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnStateChanged(EventArgs e)
		{
			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// When we switch to min/max then cache the restore bounds 
			// on the toolwindow.
			//
			this.CacheRestoreBounds();

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// In case the Window class or HwndSource didn't change the SizeToContent
			// from one of the autosize values to manual when going to minimized or 
			// maximized, coerce it.
			//
			this.CoerceValue(SizeToContentProperty);

			base.OnStateChanged(e);
		} 
		#endregion //OnStateChanged

		#region OnSourceInitialized
		/// <summary>
		/// Invoked when the window is initialized.
		/// </summary>
		/// <param name="e">Provides data for the event</param>
		protected override void OnSourceInitialized(EventArgs e)
		{
			WindowInteropHelper interop = new WindowInteropHelper(this);
			IntPtr hwnd = interop.Handle;
			HwndSource source = HwndSource.FromHwnd(hwnd);
			source.AddHook(new HwndSourceHook(this.WndProc));






			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (null != _content)
			{
				// We have to wait until the source initialized to sync the window state since 
				// we want the window to be maximized based on where it was positioned. If we bound 
				// in the OnContentChanged then the window is maximized on the window in which the mouse
				// existed.
				// AS 5/19/11 TFS75307
				//this.SetBinding(Window.WindowStateProperty, Utilities.CreateBindingObject(ToolWindow.WindowStateProperty, BindingMode.TwoWay, _content));
				this.SetBinding(WindowStateInternalProperty, Utilities.CreateBindingObject(ToolWindow.WindowStateProperty, BindingMode.TwoWay, _content));

				// now that the window handle is created, we can copy over the cached restore 
				// bounds from the toolwindow.
				if (null != _content && this.WindowState != WindowState.Normal)
				{
					// AS 1/13/12 TFS99362
					// The HWND may not be minimized yet so setting the windowplacement may not have 
					// any effect yet so we'll asynchronously initialize the restore bounds.
					//
					//this.SetRestoreBounds(hwnd, _content.GetRestoreBounds(false));
					Rect restoreBounds = _content.GetRestoreBounds(false);

					if (NativeWindowMethods.GetWindowState(this) == WindowState.Normal 
						&& this.WindowState == WindowState.Minimized 
						&& !restoreBounds.IsEmpty)
					{
						this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, 
							new System.Threading.SendOrPostCallback(this.AsyncInitializeRestoreBounds), restoreBounds);
					}
					else
					{
						this.SetRestoreBounds(hwnd, restoreBounds);
					}
				}

				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// We don't want to be bound to the toolwindow's setting once the window has been shown.
				// This is necessary because the WPF window cannot go from WindowStyle None and 
				// AllowTransparency true to a WindowStyle of ToolWindow.
				//
				this.UseSystemNonClientArea = this.UseSystemNonClientArea;
			}

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved to a helper method since we will need to change the bits as properties are changed.
			// 
			//// remove the minimize/maximize bits
			//IntPtr style = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE);
			//IntPtr newStyle = NativeWindowMethods.RemoveBits(style, NativeWindowMethods.WS_MINIMIZEBOX | NativeWindowMethods.WS_MAXIMIZEBOX);
			//
			//if (style != newStyle)
			//    NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, newStyle);
			this.VerifyWindowStyleBits(hwnd);

			UpdateAllowClose(this, this.AllowClose);

			// initialize sizes
			// AS 4/25/08
			// When toggling between floating and non-floating I noticed that the size of the window
			// was changing. Basically we should not have been adding in the non-client size.
			//
			//this.UpdateClientSizeFromNonClientSize(true, true);

			// update relative positioning
			this.VerifyRelativePositionBinding();

			// AS 4/29/08
			// The value of the top/left have changed on the window but the binding is 
			// not updating the value on the content so we need to explicitly do that.
			//
			this.SynchronizeLocation(true);

			base.OnSourceInitialized(e);

			// AS 8/4/11 TFS83465/TFS83469
			// When the window is first shown make sure its at least partially in view.
			//
			this.VerifyIsInView(false, true, false);
		}

		#endregion //OnSourceInitialized

		#region OnRenderSizeChanged
		/// <summary>
		/// Invoked when the render size of the element has changed.
		/// </summary>
		/// <param name="sizeInfo">Provides information about the size change</param>
		protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
		{
			base.OnRenderSizeChanged(sizeInfo);

			this.UpdateRelativePosition();
		} 
		#endregion //OnRenderSizeChanged

		#endregion //Base class overrides

		#region Properties

		#region AllowClose
		public static readonly DependencyProperty AllowCloseProperty = ToolWindow.AllowCloseProperty.AddOwner(typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(OnAllowCloseChanged));

		public bool AllowClose
		{
			get { return (bool)this.GetValue(AllowCloseProperty); }
		}

		private static void OnAllowCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow window = (ToolWindowHostWindow)d;
			UpdateAllowClose(window, true.Equals(e.NewValue));
		} 
		#endregion //AllowClose

		#region ClientHeight

		/// <summary>
		/// Identifies the <see cref="ClientHeight"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ClientHeightProperty = DependencyProperty.Register("ClientHeight",
			typeof(double), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnClientHeightChanged)));

		private static void OnClientHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OutputSizeInfo(string.Format("Old: {0}, New: {1}", e.OldValue, e.NewValue), "Client Height Changed");

			((ToolWindowHostWindow)d).UpdateNonClientSizeFromClientSize(false, true);
			d.InvalidateProperty(HeightProperty);
		}

		/// <summary>
		/// Returns the current height of the client area of the form.
		/// </summary>
		/// <seealso cref="ClientHeightProperty"/>
		internal double ClientHeight
		{
			get
			{
				return (double)this.GetValue(ToolWindowHostWindow.ClientHeightProperty);
			}
			set
			{
				this.SetValue(ToolWindowHostWindow.ClientHeightProperty, value);
			}
		}

		#endregion //ClientHeight

		#region ClientWidth

		/// <summary>
		/// Identifies the <see cref="ClientWidth"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty ClientWidthProperty = DependencyProperty.Register("ClientWidth",
			typeof(double), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnClientWidthChanged)));

		private static void OnClientWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OutputSizeInfo(string.Format("Old: {0}, New: {1}", e.OldValue, e.NewValue), "Client Height Changed");

			((ToolWindowHostWindow)d).UpdateNonClientSizeFromClientSize(true, false);
			d.InvalidateProperty(WidthProperty);
		}

		/// <summary>
		/// Returns the current width of the client area of the form.
		/// </summary>
		internal double ClientWidth
		{
			get
			{
				return (double)this.GetValue(ToolWindowHostWindow.ClientWidthProperty);
			}
			set
			{
				this.SetValue(ToolWindowHostWindow.ClientWidthProperty, value);
			}
		}

		#endregion //ClientWidth

		#region CachedIsDwmCompositionEnabled
		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// This isn't being used.
		//
		//private bool CachedIsDwmCompositionEnabled
		//{
		//    get { return this._flags[CachedIsDwmCompositionEnabledFlag]; }
		//    set { this._flags[CachedIsDwmCompositionEnabledFlag] = value; }
		//} 
		#endregion //CachedIsDwmCompositionEnabled

		#region IsGlassActiveInternal

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// This isn't being used.
		//
		//internal bool IsGlassActiveInternal
		//{
		//    get
		//    {
		//        return this.CachedIsDwmCompositionEnabled;
		//    }
		//}

		#endregion //IsGlassActiveInternal

		#region IsInSendMessage
		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// This isn't used any more.
		//
		//private bool IsInSendMessage
		//{
		//    get { return this._flags[IsInSendMessageFlag]; }
		//    set { this._flags[IsInSendMessageFlag] = value; }
		//} 
		#endregion //IsInSendMessage

		#region IsInWndProc
		private bool IsInWndProc
		{
			get { return this._wndProcCount > 0; }
		}
		#endregion //IsInWndProc

		#region InternalLeft

		/// <summary>
		/// Identifies the <see cref="InternalLeft"/> dependency property
		/// </summary>
		private static readonly DependencyProperty InternalLeftProperty = DependencyProperty.Register("InternalLeft",
			typeof(double), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnInternalLeftTopChanged)));

		/// <summary>
		/// Description
		/// </summary>
		/// <seealso cref="InternalLeftProperty"/>
		private double InternalLeft
		{
			get
			{
				return (double)this.GetValue(ToolWindowHostWindow.InternalLeftProperty);
			}
			set
			{
				this.SetValue(ToolWindowHostWindow.InternalLeftProperty, value);
			}
		}

		#endregion //InternalLeft

		#region InternalTop

		/// <summary>
		/// Identifies the <see cref="InternalTop"/> dependency property
		/// </summary>
		private static readonly DependencyProperty InternalTopProperty = DependencyProperty.Register("InternalTop",
			typeof(double), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnInternalLeftTopChanged)));

		/// <summary>
		/// Description
		/// </summary>
		/// <seealso cref="InternalTopProperty"/>
		private double InternalTop
		{
			get
			{
				return (double)this.GetValue(ToolWindowHostWindow.InternalTopProperty);
			}
			set
			{
				this.SetValue(ToolWindowHostWindow.InternalTopProperty, value);
			}
		}

		#endregion //InternalTop

		#region OwnerRect

		/// <summary>
		/// Identifies the <see cref="OwnerRect"/> dependency property
		/// </summary>
		private static readonly DependencyProperty OwnerRectProperty = DependencyProperty.Register("OwnerRect",
			typeof(Rect), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(new Rect(), new PropertyChangedCallback(OnOwnerRectChanged)));

		private static void OnOwnerRectChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow window = (ToolWindowHostWindow)d;

			window.UpdateRelativePosition();
		}

		/// <summary>
		/// Returns the rect of the owner window.
		/// </summary>
		/// <seealso cref="OwnerRectProperty"/>
		private Rect OwnerRect
		{
			get
			{
				return (Rect)this.GetValue(ToolWindowHostWindow.OwnerRectProperty);
			}
		}

		#endregion //OwnerRect

		#region UpdatingRelativePosition
		private bool UpdatingRelativePosition
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			get { return this.GetFlag(InternalFlags.UpdatingRelativePosition); }
			set { this.SetFlag(InternalFlags.UpdatingRelativePosition, value); }
		} 
		#endregion //UpdatingRelativePosition

		#region UseSystemNonClientArea

		/// <summary>
		/// Identifies the <see cref="UseSystemNonClientArea"/> dependency property
		/// </summary>
		private static readonly DependencyProperty UseSystemNonClientAreaProperty = DependencyProperty.Register("UseSystemNonClientArea",
			typeof(bool), typeof(ToolWindowHostWindow), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnUseSystemNonClientAreaChanged)));

		private static void OnUseSystemNonClientAreaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow window = (ToolWindowHostWindow)d;

			// AS 1/20/11 Optimization
			window.SetFlag(InternalFlags.UseSystemNonClientArea, (bool)e.NewValue);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Found this while debugging. We don't need to perform these steps when the 
			// window is closing and disconnected from the content.
			//
			if (window._content == null)
				return;

			WindowInteropHelper interop = new WindowInteropHelper(window);
			IntPtr hwnd = interop.Handle;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Two changes. First we don't need to do any of this if we don't have a window 
			// handle since that will mean the window is closed or its sourceinitialized 
			// has not been invoked (e.g. we're in the contentchanged processing). Second, 
			// we need to invoke this after the style bits have been tweaked.
			//
			//if (hwnd != IntPtr.Zero)
			//{
			//    NativeWindowMethods.SetWindowPosApi(
			//        hwnd,
			//        IntPtr.Zero,
			//        0, 0, 0, 0,
			//        NativeWindowMethods.SWP_NOACTIVATE |
			//        NativeWindowMethods.SWP_NOMOVE |
			//        NativeWindowMethods.SWP_NOSIZE |
			//        NativeWindowMethods.SWP_NOZORDER |
			//        NativeWindowMethods.SWP_NOOWNERZORDER |
			//        NativeWindowMethods.SWP_FRAMECHANGED);
			//
			//    OutputSizeInfo("Sent FrameChanged Notification", "OnUseSystemNonClientAreaChanged");
			//}
			if (hwnd == IntPtr.Zero)
			{
				// AS 11/10/11 TFS95869
				// Even if the window handle isn't created we may need to adjust the window style.
				// Without this we were leaving the WindowStyle set to Single and just stripping out 
				// bits when the source was initialized.
				//
				window.VerifyWindowStyle();
				return;
			}

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			window.CoerceValue(SizeToContentProperty);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// This isn't needed since we always bind the title now.
			//
			//window.VerifyTitleBinding();
			window.VerifyWindowStyle();

			OutputSizeInfo("Updated Title Binding", "OnUseSystemNonClientAreaChanged");

            // AS 4/17/19 TFS16677
            // We should not be forcing the recalculation of the min/max while
            // the toolwindow is no longer associated with this window.
            //
			//if (null != window._content)
			if (null != window._content && window._content.Host == window)
				window._content.CalculateMinMaxExtents(false);

			window.UpdateNonClientSizeFromClientSize(true, true);

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Moved down from above.
			//
			if (hwnd != IntPtr.Zero)
			{
				NativeWindowMethods.SetWindowPosApi(
					hwnd,
					IntPtr.Zero,
					0, 0, 0, 0,
					NativeWindowMethods.SWP_NOACTIVATE |
					NativeWindowMethods.SWP_NOMOVE |
					NativeWindowMethods.SWP_NOSIZE |
					NativeWindowMethods.SWP_NOZORDER |
					NativeWindowMethods.SWP_NOOWNERZORDER |
					NativeWindowMethods.SWP_FRAMECHANGED);

				OutputSizeInfo("Sent FrameChanged Notification", "OnUseSystemNonClientAreaChanged");
			}
		}

		private bool UseSystemNonClientArea
		{
			get
			{
				// AS 1/20/11 Optimization
				//return (bool)this.GetValue(ToolWindowHostWindow.UseSystemNonClientAreaProperty);
				return this.GetFlag(InternalFlags.UseSystemNonClientArea);
			}
			set
			{
				this.SetValue(ToolWindowHostWindow.UseSystemNonClientAreaProperty, value);
			}
		}

		#endregion //UseSystemNonClientArea

		#endregion //Properties

		#region Methods

		#region Internal methods

		#region AddNonClientSize
		internal Size AddNonClientSize(Size clientSize)
		{
			double width = clientSize.Width;
			double height = clientSize.Height;

			UpdateForClientSize(true, ref width, ref height);

			return new Size(width, height);
		}
		#endregion //AddNonClientSize

		#region AreClose
		internal static bool AreClose(double value1, double value2)
		{
			if (value1 == value2)
				return true;

			if (double.IsNaN(value1) && double.IsNaN(value2))
				return true;

			return Math.Abs(value1 - value2) < .0000000001;
		}
		#endregion //AreClose

		#region BringToFront
		internal void BringToFront()
		{
			IntPtr hwnd = new WindowInteropHelper(this).Handle;

			if (IntPtr.Zero != hwnd)
			{
				const int flags = NativeWindowMethods.SWP_NOMOVE | NativeWindowMethods.SWP_NOSIZE | NativeWindowMethods.SWP_NOACTIVATE;
				NativeWindowMethods.SetWindowPosApi(hwnd, IntPtr.Zero, 0, 0, 0, 0, flags);
			}
		}
		#endregion //BringToFront

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region FromRootVisual
        internal static ToolWindow FromRootVisual(System.Windows.Media.Visual rootVisual)
        {
            ToolWindow tw = null;

            if (null != rootVisual)
            {
                ToolWindowHostWindow twhw = rootVisual as ToolWindowHostWindow;

                if (twhw != null)
                    tw = twhw.Content as ToolWindow;
                else
                {
                    Popup p = LogicalTreeHelper.GetParent(rootVisual) as Popup;

					// AS 6/8/09 TFS18150
					//ToolWindowHostPopup twhp = null != p ? p.Child as ToolWindowHostPopup : null;
					ToolWindowHostPopup twhp = ToolWindowHostPopup.GetHost(p);

                    if (null != twhp)
                        tw = twhp.Child as ToolWindow;
                }
            }

            return tw;
        }
        #endregion //FromRootVisual

        #region GetToolWindows
		internal static void GetToolWindows(Window window, FrameworkElement owner, List<ToolWindow> list, out int ownerWindowIndex )
		{
			// AS 5/8/09 TFS17053
			//IntPtr parent = new WindowInteropHelper(window).Handle;
			IntPtr parent = IntPtr.Zero;
			
			if (window != null)
			{
				parent = new WindowInteropHelper(window).Handle;
			}
			else
			{
				IntPtr ownerHwnd = ToolWindow.GetOwnerHwnd(owner);

				if (ownerHwnd != IntPtr.Zero)
					parent = NativeWindowMethods.GetAncestorApi(ownerHwnd, NativeWindowMethods.GA_ROOT);
			}

			int windowIndex = list.Count; // AS 1/10/12 TFS90890

			if (IntPtr.Zero != parent)
			{
				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// Since some windows may be unowned, we need to enumerate all the windows.
				//
				//// we'll enumerate the owned windows in zorder and add any that contain a toolwindow
				//// owned by the specified owner
				//
				//// get the first child
				//IntPtr child = NativeWindowMethods.GetWindow(parent, NativeWindowMethods.GW_ENABLEDPOPUP);
				//
				//while (IntPtr.Zero != child && parent != child)
				//{
				//    HwndSource source = HwndSource.FromHwnd(child);
				//    /* AS 3/30/09 TFS16355 - WinForms Interop
				//     * Refactored to take the ToolWindowHostPopup into account.
				//     * 
				//    ToolWindowHostWindow host = source != null ? source.RootVisual as ToolWindowHostWindow : null;
				//
				//    if (null != host)
				//    {
				//        ToolWindow toolWindow = host.Content as ToolWindow;
				//
				//        if (null != toolWindow && toolWindow.Owner == owner)
				//            list.Add(toolWindow);
				//    }
				//    */
				//    if (null != source)
				//    {
				//        ToolWindow tw = FromRootVisual(source.RootVisual);
				//
				//        if (null != tw && tw.Owner == owner)
				//            list.Add(tw);
				//    }
				//
				//    child = NativeWindowMethods.GetWindow(child, NativeWindowMethods.GW_HWNDNEXT);
				//}
				//
				//// now get the non-top level child windows
				//
				//// get the first child
				//child = NativeWindowMethods.GetWindow(parent, NativeWindowMethods.GW_CHILD);
				//
				//while (IntPtr.Zero != child && child != parent)
				//{
				//    HwndSource source = HwndSource.FromHwnd(child);
				//
				//    if (null != source)
				//    {
				//        ToolWindow tw = FromRootVisual(source.RootVisual);
				//
				//        if (null != tw && tw.Owner == owner)
				//        {
				//            Debug.Assert(list.Contains(tw) == false);
				//            list.Add(tw);
				//        }
				//    }
				//
				//    child = NativeWindowMethods.GetWindow(child, NativeWindowMethods.GW_HWNDNEXT);
				//}

				// instead of using HwndSource.FromHwnd, which does a linear walk build a table 
				// of the sources and use that from the callbacks
				Dictionary<IntPtr, HwndSource> sources = new Dictionary<IntPtr, HwndSource>();

				foreach(PresentationSource source in PresentationSource.CurrentSources)
				{
					HwndSource hs = source as HwndSource;

					if (hs == null || hs.IsDisposed)
						continue;

					IntPtr hwnd = hs.Handle;

					if (hwnd == IntPtr.Zero)
						continue;

					sources[hwnd] = hs;
				}

				NativeWindowMethods.EnumWindowsProc childCallback = delegate(IntPtr hwnd, IntPtr lParam)
				{
					GetToolWindows(owner, list, sources, hwnd);
					return true;
				};

				NativeWindowMethods.EnumWindowsProc rootCallback = delegate(IntPtr hwnd, IntPtr lParam)
				{
					NativeWindowMethods.EnumChildWindows(hwnd, childCallback, IntPtr.Zero);
					GetToolWindows(owner, list, sources, hwnd);

					// AS 1/10/12 TFS90890
					if (hwnd == parent)
						windowIndex = list.Count;

					return true;
				};

				NativeWindowMethods.EnumThreadWindows(NativeWindowMethods.GetCurrentThreadIdApi(), rootCallback, IntPtr.Zero);
			}

			ownerWindowIndex = windowIndex; // AS 1/10/12 TFS90890
		}

		private static void GetToolWindows(FrameworkElement owner, List<ToolWindow> list, Dictionary<IntPtr, HwndSource> sources, IntPtr hwnd)
		{
			HwndSource source;

			if (sources.TryGetValue(hwnd, out source) && !source.IsDisposed)
			{
				ToolWindow tw = FromRootVisual(source.RootVisual);

				if (null != tw && tw.Owner == owner)
				{
					Debug.Assert(list.Contains(tw) == false);
					list.Add(tw);
				}
			}
		}
		#endregion //GetToolWindows

		#region Show
		internal void Show(bool activate)
		{
            // AS 12/6/11 TFS97074
            // Since it may be shown well after the initial show call resync the AllowsTransparency state.
            //
            this.VerifyWindowStyle();

			// if we don't want to activate the window then create a hook
			// before
			if (false == activate)
				this.InstallActivationHook();

			try
			{
				this.Show();
			}
			catch
			{
				// if an exception happens during the show then assume the show failed
				// and remove the hook
				this.ReleaseActivationHook();
				throw;
			}
		}
		#endregion //Show

		#endregion //Internal methods

		#region Private Methods

		// AS 1/13/12 TFS99362
		#region AsyncInitializeRestoreBounds
		private void AsyncInitializeRestoreBounds(object restoreBounds)
		{
			var rect = (Rect)restoreBounds;
			var hwnd = new WindowInteropHelper(this).Handle;

			if (hwnd != null && NativeWindowMethods.GetWindowState(hwnd) != System.Windows.WindowState.Normal)
			{
				this.SizeToContent = System.Windows.SizeToContent.Manual;
				this.SetRestoreBounds(hwnd, rect);
			}
		}
		#endregion //AsyncInitializeRestoreBounds

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region CacheRestoreBounds
		private void CacheRestoreBounds()
		{
			if (_content != null)
			{
				if (this.WindowState != WindowState.Normal)
				{
					Rect restoreBounds = this.RestoreBounds;

					if (!restoreBounds.IsEmpty)
					{
						_content.SetRestoreBounds(restoreBounds, false);

						WindowInteropHelper wih = new WindowInteropHelper(this);
						if (wih.Handle != IntPtr.Zero)
						{
							NativeWindowMethods.WINDOWPLACEMENT placement = NativeWindowMethods.GetWindowPlacement(wih.Handle);
							_content.RestoreToMaximized = (placement.flags & NativeWindowMethods.WPF_RESTORETOMAXIMIZED) == NativeWindowMethods.WPF_RESTORETOMAXIMIZED;
						}
					}
				}
				else
				{
					if (NativeWindowMethods.GetWindowState(this) == WindowState.Normal)
						_content.RestoreToMaximized = false;
				}
			}
		}
		#endregion //CacheRestoreBounds

		#region CloseHelper
		private void CloseHelper()
		{
			if (this.IsInWndProc)
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Infragistics.Windows.Controls.ToolWindow.MethodInvoker(this.CloseHelper));
			else
				this.Close();
		} 
		#endregion //CloseHelper

		#region CoerceMaxWidth
		private static object CoerceMaxWidth(DependencyObject d, object newValue)
		{
			ToolWindowHostWindow window = (ToolWindowHostWindow)d;

			double width = (double)newValue;

			if (double.IsPositiveInfinity(width) || window.UseSystemNonClientArea == false)
				return newValue;

			double tempHeight = double.NaN;

			window.UpdateForClientSize(true, ref width, ref tempHeight);

			return width;
		}
		#endregion //CoerceMaxWidth

		#region CoerceMaxHeight
		private static object CoerceMaxHeight(DependencyObject d, object newValue)
		{
			ToolWindowHostWindow window = (ToolWindowHostWindow)d;

			double height = (double)newValue;

			if (double.IsPositiveInfinity(height) || window.UseSystemNonClientArea == false)
				return newValue;

			double tempWidth = double.NaN;

			window.UpdateForClientSize(true, ref tempWidth, ref height);

			return height;
		}
		#endregion //CoerceMaxHeight

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region CoerceSizeToContent
		private static object CoerceSizeToContent(DependencyObject d, object baseValue)
		{
			ToolWindowHostWindow w = d as ToolWindowHostWindow;

			// when the toolwindow is providing the nonclient area and it is minimized 
			// we need to force the window into manual sizing mode or else the elements 
			// in the template of the toolwindow will not properly conform to the actual 
			// size of the window. also when the toolwindow is not providing the non 
			// client area and you drag it into a maximized state, we need to switch 
			// out of sizing to content.
			//
			if (NativeWindowMethods.GetWindowState(w) != WindowState.Normal && 
				!SizeToContent.Manual.Equals(baseValue))
				return SizeToContent.Manual;

			return baseValue;
		} 
		#endregion //CoerceSizeToContent

		#region ConvertFromLogicalPixels
		private NativeWindowMethods.POINT ConvertFromLogicalPixels(double x, double y)
		{
			WindowInteropHelper helper = new WindowInteropHelper(this);
			IntPtr handle = helper.Handle;
			HwndSource source = IntPtr.Zero != handle ? HwndSource.FromHwnd(handle) : null;
			Point pt;

			if (null != source)
			{
				pt = source.CompositionTarget.TransformToDevice.Transform(new Point(x, y));
			}
			else
			{
				pt = new Point(Utilities.ConvertFromLogicalPixels(x), Utilities.ConvertFromLogicalPixels(y));
			}

			return new NativeWindowMethods.POINT((int)pt.X, (int)pt.Y);
		} 
		#endregion //ConvertFromLogicalPixels

		#region ConvertToLogicalPixels
		private Point ConvertToLogicalPixels(int x, int y)
		{
			WindowInteropHelper helper = new WindowInteropHelper(this);
			IntPtr handle = helper.Handle;
			HwndSource source = IntPtr.Zero != handle ? HwndSource.FromHwnd(handle) : null;
			Point pt;

			if (null != source)
			{
				pt = source.CompositionTarget.TransformFromDevice.Transform(new Point(x, y));
			}
			else
			{
				pt = new Point(Utilities.ConvertToLogicalPixels(x), Utilities.ConvertToLogicalPixels(y));
			}

			return pt;
		} 
		#endregion //ConvertToLogicalPixels

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag(InternalFlags flag)
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		#region HookProc
		private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
		{
			switch (nCode)
			{
				case NativeWindowMethods.HCBT_ACTIVATE:
					// wParam is the window handle being activated
					// lParam is CBTACTIVATESTRUCT
					Window window = Window.GetWindow(this);
					WindowInteropHelper interop = null != window ? new WindowInteropHelper(window) : null;

					if (interop != null && interop.Handle == wParam)
					{
						this.ReleaseActivationHook();
						return new IntPtr(1);
					}
					break;
			}

			return NativeWindowMethods.CallNextHookEx(this._hook, nCode, wParam, lParam);
		}
		#endregion //HookProc

		#region InstallActivationHook
		private void InstallActivationHook()
		{
			Debug.Assert(this._hook == IntPtr.Zero);

			if (this._hook != IntPtr.Zero)
				this.ReleaseActivationHook();

			if (null == this._hookCallback)
				this._hookCallback = new NativeWindowMethods.HookProc(this.HookProc);

			this._hook = NativeWindowMethods.SetWindowsHookEx(NativeWindowMethods.WH_CBT,
				this._hookCallback,
				IntPtr.Zero,
				NativeWindowMethods.GetCurrentThreadIdApi());
		}
		#endregion //InstallActivationHook

		#region OnInternalLeftTopChanged
		private static void OnInternalLeftTopChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((ToolWindowHostWindow)d).SynchronizeLocation(false);
		} 
		#endregion //OnInternalLeftTopChanged

		#region OnHeightChanged
		private static void OnHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OutputSizeInfo(string.Format("Old: {0}, New: {1}", e.OldValue, e.NewValue), "Window Height Changed");

			((ToolWindowHostWindow)d).UpdateClientSizeFromNonClientSize(false, true);
		}
		#endregion //OnHeightChanged

		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Infragistics.Windows.Controls.ToolWindow.MethodInvoker(this.UpdateRelativePosition));
		} 
		#endregion //OnLayoutUpdated

		// AS 9/2/11 TFS85394
		// The height cached by the ToolWindow may be the "snapped" size when using the aero 
		// snap functionality that lets you dock a window to an edge or resize the top/bottom 
		// so that it fills the vertical area. So we need to re-sync its size.
		//
		#region OnAfterWindowStateChanged
		private void OnAfterWindowStateChanged(WindowState newState)
		{
			// assuming everything is in sync
			if (!this.GetFlag(InternalFlags.IsChangingWindowState) &&
				newState == this.WindowState &&
				newState == System.Windows.WindowState.Normal &&
				_content != null &&
				_content.WindowState == newState)
			{
				this.UpdateClientSizeFromNonClientSize(!double.IsNaN(_content.Width), !double.IsNaN(_content.Height));
			}
		}
		#endregion //OnAfterWindowStateChanged

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnAllowMinMaxChanged
		private static void OnAllowMinMaxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow w = d as ToolWindowHostWindow;

			Size nonClientBefore = w.AddNonClientSize(new Size(100, 100));

			// update the window style bits
			w.VerifyWindowStyle();

			// update the size of the window based on the size of the new non-client area
			w.UpdateNonClientSizeFromClientSize(true, true);

			// if we're sized to content the window won't be resized when we 
			// change the window style. it seems like the only way to get the 
			// hwndsource to process the change is to toggle the sizetocontent 
			// property. to avoid the overhead we'll only do this if the size 
			// of the non-client area is actually changed.
			if (w.SizeToContent != SizeToContent.Manual && w.UseSystemNonClientArea)
			{
				Size nonClientAfter = w.AddNonClientSize(new Size(100, 100));

				if (nonClientAfter != nonClientBefore)
				{
					bool wasForcingSizeToContent = w.GetFlag(InternalFlags.IsForcingSizeToContent);

					w.SetFlag(InternalFlags.IsForcingSizeToContent, true);

					try
					{
						SizeToContent s2c = w.SizeToContent;
						w.SizeToContent = SizeToContent.Manual;
						w.SizeToContent = s2c;
					}
					finally
					{
						w.SetFlag(InternalFlags.IsForcingSizeToContent, wasForcingSizeToContent);
					}
				}
			}
		} 
		#endregion //OnAllowMinMaxChanged

		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region OnSizeToContentChanged
		private static void OnSizeToContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

		} 
		#endregion //OnSizeToContentChanged

		#region OnWidthChanged
		private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			OutputSizeInfo(string.Format("Old: {0}, New: {1}", e.OldValue, e.NewValue), "Window Width Changed");

			((ToolWindowHostWindow)d).UpdateClientSizeFromNonClientSize(true, false);
		}
		#endregion //OnWidthChanged

		// AS 5/19/11 TFS75307
		#region OnWindowStateChanged
		private static void OnWindowStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolWindowHostWindow window = d as ToolWindowHostWindow;

			// AS 9/1/11 TFS85394
			// Use the same flag we set when we are setting the WindowState so we can detect when 
			// the Window/OS is changing the WindowState of the hosting window since we may need 
			// to ignore changes while that is in progress.
			//
			bool wasChangingState = window.GetFlag(InternalFlags.IsChangingWindowState);
			window.SetFlag(InternalFlags.IsChangingWindowState, true);

			try
			{
				window.SetValue(WindowStateInternalProperty, e.NewValue);
			}
			finally
			{
				window.SetFlag(InternalFlags.IsChangingWindowState, wasChangingState);
			}

			// AS 9/2/11 TFS85394
			window.OnAfterWindowStateChanged((WindowState)e.NewValue);

			// AS 8/4/11 TFS83465/TFS83469
			if (WindowState.Normal.Equals(e.NewValue))
				window.VerifyIsInView(false, true, false);
		}
		#endregion //OnWindowStateChanged

		#region OutputRelativePosInfo
		[Conditional("DEBUG_RELATIVEPOS")]
		private static void OutputRelativePosInfo(object value, string category)
		{
			Debug.WriteLine(value, category);
		} 
		#endregion //OutputRelativePosInfo

		#region OutputSizeInfo
		[Conditional("DEBUG_SIZING")]
		private static void OutputSizeInfo(object value, string category)
		{
			Debug.WriteLine(value, category);
		}
		#endregion // OutputSizeInfo

		#region ReleaseActivationHook
		private void ReleaseActivationHook()
		{
			if (IntPtr.Zero != this._hook)
			{
				IntPtr oldHook = this._hook;
				this._hook = IntPtr.Zero;
				NativeWindowMethods.UnhookWindowsHookEx(oldHook);
			}
		}
		#endregion //ReleaseActivationHook

		#region SendMessage
		// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// Not using - also its too heavy handed. Other messages could come in while
		// we're sending the message that we would have wanted to process.
		//
		//private void SendMessage(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam)
		//{
		//    bool wasInMessageLoop = this.IsInSendMessage;
		//    this.IsInSendMessage = true;
		//
		//    try
		//    {
		//        NativeWindowMethods.SendMessageApi(hwnd, message, wParam, lParam);
		//    }
		//    finally
		//    {
		//        this.IsInSendMessage = wasInMessageLoop;
		//    }
		//}
		#endregion //SendMessage

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetRestoreBounds
		private void SetRestoreBounds(IntPtr hwnd, Rect restoreBounds)
		{
			if (hwnd != IntPtr.Zero && !restoreBounds.IsEmpty)
			{
				NativeWindowMethods.WINDOWPLACEMENT placement = NativeWindowMethods.GetWindowPlacement(hwnd);
				NativeWindowMethods.POINT topLeft = this.ConvertFromLogicalPixels(restoreBounds.Left, restoreBounds.Top);
				NativeWindowMethods.POINT bottomRight = this.ConvertFromLogicalPixels(restoreBounds.Right, restoreBounds.Bottom);
				placement.rcNormalPosition = new NativeWindowMethods.RECT(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
				NativeWindowMethods.SetWindowPlacement(hwnd, ref placement);
			}
		}
		#endregion //SetRestoreBounds

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region SetRestoreToMaximize
		private void SetRestoreToMaximize(IntPtr hwnd)
		{
			if (hwnd != IntPtr.Zero)
			{
				NativeWindowMethods.WINDOWPLACEMENT placement = NativeWindowMethods.GetWindowPlacement(hwnd);

				// when the actual state and stored state are the same then copy over the restore to maximize
				if (null != _content && placement.showCmd == NativeWindowMethods.SW_SHOWMINIMIZED && this.WindowState == WindowState.Minimized)
				{
					int original = placement.flags;

					if (_content.RestoreToMaximized)
						placement.flags |= NativeWindowMethods.WPF_RESTORETOMAXIMIZED;
					else
						placement.flags &= ~NativeWindowMethods.WPF_RESTORETOMAXIMIZED;

					if (placement.flags != original)
						NativeWindowMethods.SetWindowPlacement(hwnd, ref placement);
				}
			}
		} 
		#endregion //SetRestoreToMaximize

		#region ShowContentContextMenu
		private bool ShowContentContextMenu(IntPtr hwnd, Point? location)
		{
			bool result = false;

			// AS 4/1/08
			// If the keyboard/mouse is used to show the menu, let it show but 
			// still update the state of the menu items.
			//
			//if (null != this._content)
			if (null != this._content && null != location)
			{
				//if (location == null)
				//	location = new Point();
				//else
				//	location = this.TransformToDescendant(this._content).Transform(location.Value);
				// AS 1/6/10 TFS25834
				//location = this.TransformToDescendant(this._content).Transform(location.Value);
				//
				//result = this._content.ShowSystemContextMenu(location.Value);
				GeneralTransform gt = this.TransformToDescendant(this._content);

				if (null != gt)
				{
					location = gt.Transform(location.Value);
					result = this._content.ShowSystemContextMenu(location.Value);
				}
			}

			if (false == result)
			{
				IntPtr sysMenu = NativeWindowMethods.GetSystemMenuApi(hwnd, false);

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MINIMIZE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				//NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_RESTORE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				//NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MAXIMIZE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				if (null == _content || !_content.AllowMaximize)
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MAXIMIZE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				else
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MAXIMIZE, NativeWindowMethods.MF_BYCOMMAND);

				if (null == _content || !_content.AllowMinimize)
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MINIMIZE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				else
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_MINIMIZE, NativeWindowMethods.MF_BYCOMMAND);

				if (null == _content || NativeWindowMethods.GetWindowState(hwnd) == WindowState.Normal)
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_RESTORE, NativeWindowMethods.MF_BYCOMMAND | NativeWindowMethods.MF_GRAYED);
				else
					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_RESTORE, NativeWindowMethods.MF_BYCOMMAND);
			}

			return result;
		}
		#endregion //ShowContentContextMenu

		#region SynchronizeLocation
		private void SynchronizeLocation(bool updateFromTopLeft)
		{
			if (this.GetFlag(InternalFlags.IsSynchronizingLocation))
				return;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (this.GetFlag(InternalFlags.IsInOnContentChanged))
				return;

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Initially I had this only exiting when not updating from the top/left (i.e.
			// from the client location but we also don't want to update the client location
			// when the window is maximized/minimized so things like the dockmanager don't 
			// store that information.
			//
			if (NativeWindowMethods.GetWindowState(this) != WindowState.Normal)
				return;

			this.SetFlag(InternalFlags.IsSynchronizingLocation, true);

			try
			{
                // AS 11/7/08 TFS7872
                // The Left/Top was not kept in sync in the initial wpf release so use the
                // actual hwnd info if available.
                //
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                double top = this.Top;
                double left = this.Left;

                if (IntPtr.Zero != hwnd)
                {
                    NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
                    Point pt = this.ConvertToLogicalPixels(wi.rcWindow.left, wi.rcWindow.top);
                    top = pt.Y;
                    left = pt.X;
                }

                if (updateFromTopLeft)
				{
                    this.InternalLeft = left;
					this.InternalTop = top;
				}
				else
				{
					// explicitly check the value since they could be equal but the
					// left/top could be set as a result of a coerce so don't force
					// the value to be changed
					if (false == this.InternalTop.Equals(top))
						this.Top = this.InternalTop;

					if (false == this.InternalLeft.Equals(left))
						this.Left = this.InternalLeft;
				}
			}
			finally
			{
				this.SetFlag(InternalFlags.IsSynchronizingLocation, false);
			}
		}
		#endregion //SynchronizeLocation

		#region UpdateAllowClose
		private static void UpdateAllowClose(Window window, bool allowClose)
		{
			WindowInteropHelper interop = new WindowInteropHelper(window);
			IntPtr hwnd = interop.Handle;

			if (IntPtr.Zero != hwnd)
			{
				IntPtr sysMenu = NativeWindowMethods.GetSystemMenuApi(hwnd, false);

				if (IntPtr.Zero != sysMenu)
				{
					int flags = NativeWindowMethods.MF_BYCOMMAND;

					if (allowClose)
						flags &= ~NativeWindowMethods.MF_GRAYED;
					else
						flags |= NativeWindowMethods.MF_GRAYED;

					NativeWindowMethods.EnableMenuItem(sysMenu, NativeWindowMethods.SC_CLOSE, flags);
				}
			}
		}
		#endregion //UpdateAllowClose

		#region UpdateClientSizeFromNonClientSize
		private void UpdateClientSizeFromNonClientSize(bool updateWidth, bool updateHeight)
		{
			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If we're in the process of hooking up then don't update the toolwindow. In CLR4
			// since the Width/Height reflect the actual values whether it is size to content or
			// not, we could end up updating the toolwindow before the size to content is 
			// initialized.
			//
			if (this.GetFlag(InternalFlags.IsInOnContentChanged))
				return;

			double width = this.Width;
			double height = this.Height;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If the window is sized to content in the specified orientation then don't store
			// that extent on the toolwindow. That is how it worked in CLR3 but since CLR4 changed 
			// its Window behavior for the Width/Height the window will have a value for those 
			// dimensions even if its size to content.
			//
			if (updateWidth && (this.SizeToContent & SizeToContent.Width) == SizeToContent.Width)
				width = double.NaN;

			if (updateHeight && (this.SizeToContent & SizeToContent.Height) == SizeToContent.Height)
				height = double.NaN;

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Only store the size if the window is in the normal state.
			//
			if (this.WindowState != WindowState.Normal || NativeWindowMethods.GetWindowState(this) != this.WindowState)
				return;

			Debug.Assert((!updateWidth && !updateHeight) || (this.WindowState == WindowState.Normal && NativeWindowMethods.GetWindowState(this) == WindowState.Normal));

			this.UpdateClientSizeFromNonClientSize(updateWidth, updateHeight, width, height);
		}

		// AS 2/2/10 TFS25694
		// Added overload so we can pass in the width/height to use.
		//
		private void UpdateClientSizeFromNonClientSize(bool updateWidth, bool updateHeight, double width, double height)
		{
			if (this._content != null && this._content.IsCalculatingSizeWithoutContent)
				return;

			// AS 7/8/11 TFS75307/TFS81084
			if (this.GetFlag(InternalFlags.IsChangingWindowState))
				return;

			OutputSizeInfo(string.Format("{0} x {1}", this.Width, this.Height), "ClientSize From NonClientSize Window");

			UpdateForClientSize(false, ref width, ref height);

			if (updateHeight && false == AreClose(height, this.ClientHeight))
			{
				OutputSizeInfo(height, "New Client Height");

				this.ClientHeight = double.IsNaN(height) ? height : Math.Max(height, 0d);
			}

			if (updateWidth && false == AreClose(width, this.ClientWidth))
			{
				OutputSizeInfo(width, "New Client Width");

				this.ClientWidth = double.IsNaN(width) ? width : Math.Max(width, 0d);
			}
		}
		#endregion //UpdateClientSizeFromNonClientSize

		#region UpdateForClientSize
		private bool UpdateForClientSize(bool addNonClient, ref double width, ref double height)
		{
			// get the width/height and remove the 
			WindowInteropHelper interop = new WindowInteropHelper(this);
			IntPtr hwnd = interop.Handle;

			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			double ncWidth = 0;
			double ncHeight = 0;

			if (hwnd == IntPtr.Zero)
			{
				if (false == this.UseSystemNonClientArea)
					return false;

				NativeWindowMethods.RECT rect = new NativeWindowMethods.RECT();
				const int ClientWidth = 200;
				const int ClientHeight = 200;
				rect.right = ClientWidth;
				rect.bottom = ClientHeight;
				int style = NativeWindowMethods.WS_CAPTION | NativeWindowMethods.WS_SYSMENU;
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//int styleEx = NativeWindowMethods.WS_EX_TOOLWINDOW | NativeWindowMethods.WS_EX_WINDOWEDGE;

                // AS 10/13/08 TFS6107/BR34010
                // I didn't include all the bits we needed. We need additional standard bits
                // plus we need certain bits based on the resize mode.
                //
                style |= NativeWindowMethods.WS_DLGFRAME | NativeWindowMethods.WS_BORDER;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar [Start]
				if (_content != null && _content.AllowMinimize)
					style |= NativeWindowMethods.WS_MINIMIZEBOX;

				if (_content != null && _content.AllowMaximize)
					style |= NativeWindowMethods.WS_MAXIMIZEBOX;

				int styleEx = NativeWindowMethods.WS_EX_WINDOWEDGE;

				if (this.WindowStyle == WindowStyle.SingleBorderWindow)
					styleEx |= NativeWindowMethods.WS_EX_DLGMODALFRAME;
				else
					styleEx |= NativeWindowMethods.WS_EX_TOOLWINDOW;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar [End]

                switch (this.ResizeMode)
                {
                    case ResizeMode.NoResize:
                        break;
                    case ResizeMode.CanMinimize:
						// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//Debug.Fail("Update for minimize");
						break;
                    case ResizeMode.CanResize:
                    case ResizeMode.CanResizeWithGrip:
                        style |= NativeWindowMethods.WS_THICKFRAME;
                        break;
                }

				if (false == NativeWindowMethods.AdjustWindowRectExApi(ref rect, style, false, styleEx))
					return false;

				// AS 7/9/08 BR34010
				// This isn't directly related but we should have been converting these to logical
				// pixels as we did when we had a window handle.
				//
				//ncWidth = (rect.right - rect.left) - ClientWidth;
				//ncHeight = (rect.bottom - rect.top) - ClientHeight;
				Point pt = this.ConvertToLogicalPixels((rect.right - rect.left) - ClientWidth, (rect.bottom - rect.top) - ClientHeight);
				ncWidth = pt.X;
				ncHeight = pt.Y;
			}
			else
			{
				// AS 9/11/09 TFS21330
				// I found a case while debugging this issue where we were getting here and the OS window
				// still had non-client area in its info. However if we know we're not using it we should
				// not need to check with the os.
				//
				if (false == this.UseSystemNonClientArea)
					return false;

				NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
				Point pt = this.ConvertToLogicalPixels(wi.rcWindow.Width - wi.rcClient.Width, wi.rcWindow.Height - wi.rcClient.Height);
				ncWidth = pt.X;
				ncHeight = pt.Y;
			}

			if (addNonClient == false)
			{
				ncWidth *= -1;
				ncHeight *= -1;
			}

			if (double.IsNaN(width) == false)
				width += ncWidth;

			if (double.IsNaN(height) == false)
				height += ncHeight;

			if (addNonClient)
			{
				if (width > this.MaxWidth)
					width = this.MaxWidth;

				if (height > this.MaxHeight)
					height = this.MaxHeight;
			}

			return true;
		}
		#endregion //UpdateForClientSize

		#region UpdateNonClientSizeFromClientSize
		private void UpdateNonClientSizeFromClientSize(bool updateWidth, bool updateHeight)
		{
			if (this._content != null && this._content.IsCalculatingSizeWithoutContent)
				return;

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If we set the Width/Height while the window is maximized or minimized then it will mess
			// up the restore bounds.
			//
			if (this.WindowState != WindowState.Normal)
				return;

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// I noticed this while debugging. We don't need to do any calculations while shutting down.
			//
			if (_content == null)
				return;

			// AS 9/1/11 TFS85394
			// This change is likely to be because the ToolWindow is coercing its Height/Width so we should ignore it.
			//
			if (this.GetFlag(InternalFlags.IsChangingWindowState))
				return;

			OutputSizeInfo(string.Format("{0} x {1}", this.ClientWidth, this.ClientHeight), "NonClientSize From ClientSize Client");

			double width = this.ClientWidth;
			double height = this.ClientHeight;

			UpdateForClientSize(true, ref width, ref height);

			// AS 7/9/08 BR34010
			// The Client(Width|Height) may not be an integral value so we need to 
			// coerce it to one. Otherwise the window will be sized based on its Width/Height
			// sometimes (which would be a non-integral value) but then sometimes sized
			// using WM_SIZE by the HwndSource which would be an integral value which results
			// in the size appearing "jumpy".
			//
			NativeWindowMethods.POINT intPt = this.ConvertFromLogicalPixels(width, height);
			Point pt = this.ConvertToLogicalPixels(intPt.X, intPt.Y);

			if (false == double.IsNaN(width))
				width = pt.X;

			if (false == double.IsNaN(height))
				height = pt.Y;

			if (this.WindowState != WindowState.Normal || NativeWindowMethods.GetWindowState(this) != WindowState.Normal)
			{
				updateHeight = updateWidth = false;
			}

			if ((this.SizeToContent & SizeToContent.Height) == SizeToContent.Height)
				updateHeight = false;

			if ((this.SizeToContent & SizeToContent.Width) == SizeToContent.Width)
				updateWidth = false;

			if (updateHeight && false == AreClose(height, this.Height))
			{
				OutputSizeInfo(height, "New Window Height");

				this.Height = height;
			}
			// AS 7/9/08 BR34010
			// This shouldn't be an else since both updateHeight and updateWidth could be true.
			//
			//else if (updateWidth && false == AreClose(width, this.Width))
			if (updateWidth && false == AreClose(width, this.Width))
			{
				OutputSizeInfo(width, "New Window Width");

				this.Width = width;
			}
		}
		#endregion //UpdateNonClientSizeFromClientSize

		#region UpdateRelativePosition
		private void UpdateRelativePosition()
		{
			ToolWindow toolWindow = this.Content as ToolWindow;

			if (null != toolWindow)
			{
				Rect rect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
				Rect ownerRect = this.OwnerRect;

				OutputRelativePosInfo(toolWindow.ToString(), "UpdateRelativePosition [Start]");
				OutputRelativePosInfo(rect, "Window Rect");
				OutputRelativePosInfo(ownerRect, "Owner Rect");
				OutputRelativePosInfo(new Point(this.InternalLeft, this.InternalTop), "Internal Left/Top");

				if (ownerRect.IsEmpty)
					return;

				FrameworkElement relativeElement = toolWindow.Owner;

				if (relativeElement == null)
					return;

				// i'm not sure why this is the case but when the window is reshown, it has no
				// width or height so don't try to position it yet
				if (rect.Width == 0d && rect.Height == 0d)
					return;

				if (this.UpdatingRelativePosition)
					return;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// The relative position is not maintained while the window is minimized or maximized.
				//
				if (NativeWindowMethods.GetWindowState(this) != WindowState.Normal)
					return;

				this.UpdatingRelativePosition = true;

				try
				{
					// use the position of the element within the owning window as the relative rect
					// we're really using the ownerrectproperty on this class as a trigger to know when
					// the window containing the relative element has changed

					Point relativePoint;

					// AS 4/25/08
					// Added catch since this method can result in an exception in which case
					// we just won't update the relative position.
					//
					try
					{
						relativePoint = relativeElement.PointToScreen(new Point());

                        // AS 7/9/08 BR34723
                        // PointToScreen returns a value in device pixels and logical so 
                        // convert back to logical before passing it off to the toolwindow.
                        //
                        relativePoint = this.ConvertToLogicalPixels((int)relativePoint.X, (int)relativePoint.Y);

						// AS 7/14/09 TFS18424
						if (relativeElement.FlowDirection == FlowDirection.RightToLeft)
							relativePoint.X -= relativeElement.ActualWidth;
					}
					catch (InvalidOperationException)
					{
						//Debug.Fail("Visual not in tree? " + ex.Message);
						return;
					}

					ownerRect = new Rect(relativePoint.X, relativePoint.Y, relativeElement.ActualWidth, relativeElement.ActualHeight);

					OutputRelativePosInfo(rect, "Rect Before Adjustment");

					toolWindow.AdjustRelativeRect(ownerRect, ref rect);

					OutputRelativePosInfo(rect, "Rect After Adjustment");

					WindowInteropHelper interop = new WindowInteropHelper(this);

					if (interop.Handle == IntPtr.Zero)
					{
						this.Left = rect.Left;
						this.Top = rect.Top;
						this.Width = rect.Width;
						this.Height = rect.Height;
					}
					else
					{
						// AS 5/8/09 TFS17053
						// I found this while testing this bug. Essentially once you manually resize the 
						// window, the Width|Height|Left|Top are set to explicit values in which case 
						// our stretching/positioning don't take effect.
						//
						if (toolWindow.UsesRelativePosition)
						{
							if (toolWindow.VerticalAlignmentMode == ToolWindowAlignmentMode.UseAlignment)
							{
								if (toolWindow.VerticalAlignment == VerticalAlignment.Stretch)
									this.ClearValue(HeightProperty);

								this.ClearValue(TopProperty);
								this.CoerceValue(TopProperty);
							}

							if (toolWindow.HorizontalAlignmentMode == ToolWindowAlignmentMode.UseAlignment)
							{
								if (toolWindow.HorizontalAlignment == HorizontalAlignment.Stretch)
									this.ClearValue(WidthProperty);

								this.ClearValue(LeftProperty);
								this.CoerceValue(LeftProperty);
							}
						}

						// we need to round off the top/left
						NativeWindowMethods.POINT leftTop = this.ConvertFromLogicalPixels(rect.Left, rect.Top);
						NativeWindowMethods.POINT widthHeight = this.ConvertFromLogicalPixels(rect.Width, rect.Height);

						NativeWindowMethods.POINT actualLeftTop = this.ConvertFromLogicalPixels(this.Left, this.Top);
						NativeWindowMethods.POINT actualWidthHeight = this.ConvertFromLogicalPixels(this.ActualWidth, this.ActualHeight);

						if (false == actualLeftTop.Equals(leftTop) ||
							false == actualWidthHeight.Equals(widthHeight))
						{
							OutputRelativePosInfo(string.Format("SetWindowPos: {0},{1},{2},{3}", leftTop.X, leftTop.Y, widthHeight.X, widthHeight.Y), "UpdateRelativePosition:" + toolWindow.ToString());

							NativeWindowMethods.SetWindowPosApi(interop.Handle, IntPtr.Zero,
								leftTop.X, leftTop.Y, widthHeight.X, widthHeight.Y,
								NativeWindowMethods.SWP_NOACTIVATE |
								NativeWindowMethods.SWP_NOOWNERZORDER |
								NativeWindowMethods.SWP_NOZORDER);
						}
					}

					OutputRelativePosInfo(new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight), "Window Rect After Pos");
				}
				finally
				{
					this.UpdatingRelativePosition = false;
				}
			}
		} 
		#endregion //UpdateRelativePosition

		// AS 8/4/11 TFS83465/TFS83469
		#region VerifyIsInView
		private void VerifyIsInView(bool force, bool async, bool ensureFullyInView)
		{
			if (_content == null || (_content.KeepOnScreen == false && !force))
				return;

			if (this.WindowState == WindowState.Minimized)
				return;

			if (async)
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.SendOrPostCallback(this.VerifyIsInViewImpl), ensureFullyInView);
			else
				this.VerifyIsInViewImpl(ensureFullyInView);
		}

		private void VerifyIsInViewImpl(object param)
		{
			if (_content == null)
				return;

			Rect logicalScreenRect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);

			if (_content.WindowState == WindowState.Normal)
			{
				Point? location = Utilities.CalculateOnScreenLocation(logicalScreenRect, this, true.Equals(param));

				if (location != null)
				{
					if (_content != null && _content.HorizontalAlignmentMode == ToolWindowAlignmentMode.Manual)
						this.Left = location.Value.X;

					if (_content != null && _content.VerticalAlignmentMode == ToolWindowAlignmentMode.Manual)
						this.Top = location.Value.Y;
				}
			}
			else if (_content.WindowState == WindowState.Maximized)
			{
				// AS 3/6/12 TFS103963
				// The Left/Top represent the restored position. In theory if we coerced then the Top/Left are 
				// updated but then when we restore the location is incorrect. The correct thing would be to use 
				// the actual position of the hwnd.
				//
				//Point midPt = new Point(this.Left + this.ActualWidth / 2, this.Top + this.ActualHeight / 2);
				var wih = new WindowInteropHelper(this);
				Point midPt;

				if (wih.Handle == IntPtr.Zero)
				{
					// the GetWorkArea is in device coordinates
					midPt = new Point(Utilities.ConvertFromLogicalPixels(this.Left + this.ActualWidth / 2, this), Utilities.ConvertFromLogicalPixels(this.Top + this.ActualHeight / 2, this));
				}
				else
				{
					var windowInfo = NativeWindowMethods.GetWindowInfo(wih.Handle);
					midPt = new Point(windowInfo.rcWindow.left + windowInfo.rcWindow.Width / 2, windowInfo.rcWindow.top + windowInfo.rcWindow.Height / 2);
				}

				// AS 3/23/12 TFS103963
				// Utilities.GetWorkArea will call PointToScreen on the point passed in since its expecting 
				// relative point. However since we already have a screenpoint this can result in the wrong 
				// screen's bounds being obtained. Just use the method that calls which expects a screen 
				// point.
				//
				//if (!Utilities.GetWorkArea(this, midPt, null).Contains(midPt))
				if (!NativeWindowMethods.GetWorkArea(midPt).Contains(midPt))
				{
					((IToolWindowHost)this).SetWindowState(WindowState.Normal);
					((IToolWindowHost)this).SetWindowState(WindowState.Maximized);
				}
			}
		}
		#endregion //VerifyIsInView

		#region VerifyRelativePositionBinding
		private void VerifyRelativePositionBinding()
		{
			ToolWindow toolWindow = this.Content as ToolWindow;

			// we also need to hook/unhook layout updated since we need to know when the element within
			// the window has changed positions
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			if (null == toolWindow ||
				// AS 5/8/09 TFS17053
				// We may not have the Owner property set but we should have an OwningWindow
				// and we still need to hook the layout updated so get into the other block.
				//
				//this.Owner == null ||
				(toolWindow.VerticalAlignmentMode == ToolWindowAlignmentMode.Manual &&
				toolWindow.HorizontalAlignmentMode == ToolWindowAlignmentMode.Manual))
			{
				BindingOperations.ClearBinding(this, ToolWindowHostWindow.OwnerRectProperty);
			}
			else
			{
				// if we are supposed to position the window relatively then we need to track the position
				// of the owner so that when it changes we can change
				Window owner = this.Owner;

				// AS 5/8/09 TFS17053
				// Only set up the binding when we have an owner. Otherwise we'll fix it up
				// when we get a WindowPosChanging.
				//
				if (null == owner)
				{
					BindingOperations.ClearBinding(this, ToolWindowHostWindow.OwnerRectProperty);
				}
				else
				{
					MultiBinding binding = new MultiBinding();
					binding.Bindings.Add(Utilities.CreateBindingObject(Window.LeftProperty, BindingMode.OneWay, owner));
					binding.Bindings.Add(Utilities.CreateBindingObject(Window.TopProperty, BindingMode.OneWay, owner));
					binding.Bindings.Add(Utilities.CreateBindingObject(Window.ActualWidthProperty, BindingMode.OneWay, owner));
					binding.Bindings.Add(Utilities.CreateBindingObject(Window.ActualHeightProperty, BindingMode.OneWay, owner));
					binding.Converter = WindowPositionToRectConverter.Instance;
					this.SetBinding(OwnerRectProperty, binding);
				}

				this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
			}

			#region Initialize SizeToContent

			bool sizeToWidth = false;
			bool sizeToHeight = false;

			if (null != toolWindow)
			{
				if (toolWindow.HorizontalAlignmentMode == ToolWindowAlignmentMode.UseAlignment && toolWindow.HorizontalAlignment == HorizontalAlignment.Stretch)
					sizeToWidth = false;
				else
					sizeToWidth = double.IsNaN(toolWindow.Width);

				if (toolWindow.VerticalAlignmentMode == ToolWindowAlignmentMode.UseAlignment && toolWindow.VerticalAlignment == VerticalAlignment.Stretch)
					sizeToHeight = false;
				else
					sizeToHeight = double.IsNaN(toolWindow.Height);

				// AS 3/31/11 TFS66111
				// If the window is maximized then its going to have coerced its Width/Height to 
				// NaN but if it has restore bounds then it isn't sized to content.
				//
				if (toolWindow.WindowState == System.Windows.WindowState.Maximized)
				{
					if (toolWindow.GetRestoreBounds(false).IsEmpty == false)
					{
						sizeToWidth = sizeToHeight = false;
					}
				}
			}

			if (sizeToWidth && sizeToHeight)
				this.SizeToContent = SizeToContent.WidthAndHeight;
			else if (sizeToWidth)
				this.SizeToContent = SizeToContent.Width;
			else if (sizeToHeight)
				this.SizeToContent = SizeToContent.Height;
			else
				this.SizeToContent = SizeToContent.Manual;

			#endregion //Initialize SizeToContent
		} 
		#endregion //VerifyRelativePositionBinding

		#region VerifyTitleBinding
		private void VerifyTitleBinding()
		{
			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Since the window may be in the taskbar we always want to bind to the title.
			//
			//if (this.UseSystemNonClientArea && this._content != null)
			if (this._content != null)
				BindingOperations.SetBinding(this, Window.TitleProperty, Utilities.CreateBindingObject(ToolWindow.TitleProperty, BindingMode.OneWay, this._content));
			else
				BindingOperations.ClearBinding(this, Window.TitleProperty);
		}
		#endregion //VerifyTitleBinding

		#region VerifyWindowStyle
		private void VerifyWindowStyle()
		{
			bool allowsTransparency;

			if (null != this._content && this.UseSystemNonClientArea)
			{
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// Moved down and wrapped in try/catch
				//
				//this.AllowsTransparency = false;
				allowsTransparency = false;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// The maximize/minimize buttons are not shown when using ToolWindow style
				// so if either is allowed we need to use SingleBorderWindow. The non-client 
				// area will be slightly different but that is the only way to show those
				// buttons.
				//
				//this.WindowStyle = WindowStyle.ToolWindow;
				if (_content.AllowMinimize || _content.AllowMaximize)
					this.WindowStyle = WindowStyle.SingleBorderWindow;
				else
					this.WindowStyle = WindowStyle.ToolWindow;
			}
			else
			{
				this.WindowStyle = WindowStyle.None;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// Moved down and wrapped in try/catch
				//
				//// AS 8/24/09 TFS19861
				////this.AllowsTransparency = true;
				//this.AllowsTransparency = null != _content && _content.AllowsTransparencyResolved;
				allowsTransparency = null != _content && _content.AllowsTransparencyResolved;
			}

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// Since this method could be invoked while the window is showing and the 
			// AllowsTransparency cannot be changed after its shown, we'll wrap this 
			// in a try/catch.
			//
			try
			{
				if (_content != null && new WindowInteropHelper(this).Handle == IntPtr.Zero)
					this.AllowsTransparency = allowsTransparency;
			}
			catch (InvalidOperationException)
			{
			}

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			if (null != _content)
			{
				this.VerifyWindowStyleBits();
			}
		} 
		#endregion //VerifyWindowStyle

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region VerifyWindowStyleBits
		private void VerifyWindowStyleBits()
		{
			if (_content == null)
				return;

			WindowInteropHelper wih = new WindowInteropHelper(this);
			this.VerifyWindowStyleBits(wih.Handle);
		}

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// Previously we were able to do this in the OnSourceInitialized since the bits used/removed were constant 
		// but since they are now based on changable state I moved it into this helper routine.
		//
		private void VerifyWindowStyleBits(IntPtr hwnd)
		{
			if (hwnd != IntPtr.Zero)
			{
				IntPtr style = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE);
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//IntPtr newStyle = NativeWindowMethods.RemoveBits(style, NativeWindowMethods.WS_MINIMIZEBOX | NativeWindowMethods.WS_MAXIMIZEBOX);
				ToolWindow toolWindow = this.Content as ToolWindow;
				IntPtr newStyle = style;
				if (toolWindow == null || !toolWindow.AllowMaximize)
					newStyle = NativeWindowMethods.RemoveBits(newStyle, NativeWindowMethods.WS_MAXIMIZEBOX);
				else
					newStyle = NativeWindowMethods.AddBits(newStyle, NativeWindowMethods.WS_MAXIMIZEBOX);

				if (toolWindow == null || !toolWindow.AllowMinimize)
					newStyle = NativeWindowMethods.RemoveBits(newStyle, NativeWindowMethods.WS_MINIMIZEBOX);
				else
					newStyle = NativeWindowMethods.AddBits(newStyle, NativeWindowMethods.WS_MINIMIZEBOX);

				if (style != newStyle)
					NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_STYLE, newStyle);

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// Don't show the icon in the titlebar.
				//
				style = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_EXSTYLE);
				newStyle = style;

				if (this.WindowStyle == WindowStyle.SingleBorderWindow)
					newStyle = NativeWindowMethods.AddBits(newStyle, NativeWindowMethods.WS_EX_DLGMODALFRAME);
				else
					newStyle = NativeWindowMethods.RemoveBits(newStyle, NativeWindowMethods.WS_EX_DLGMODALFRAME);

				// AS 3/31/11 NA 2011.1
				// To keep the floating windows out of the task manager when the non-client area is off, 
				// we'll set the WS_EX_TOOLWINDOW bit.
				//
				// AS 11/11/11 TFS95869
				// While debugging this issue I found that the Window Key + Arrow Keys were not moving the window. 
				// This seems to be prevented by the WS_EX_TOOLWINDOW style bit. I was going to remove it altogether 
				// since it was only forced in to try and workaround an obscure issue where the taskmanager will 
				// allow minimize/maximize regardless of whether the window itself allows it. However I decided to 
				// only remove the style bit from CLR3 because the Window class has bugs that are interfering with 
				// the proper handling of the os' attempt to place the window. The bugs have been addressed in CLR4.
				//
				if (this.WindowStyle == System.Windows.WindowStyle.None && System.Environment.Version.Major <= 3)
					newStyle = NativeWindowMethods.AddBits(newStyle, NativeWindowMethods.WS_EX_TOOLWINDOW);
				else if (this.WindowStyle != WindowStyle.ToolWindow)
					newStyle = NativeWindowMethods.RemoveBits(newStyle, NativeWindowMethods.WS_EX_TOOLWINDOW);

				if (style != newStyle)
					NativeWindowMethods.SetWindowLong(hwnd, NativeWindowMethods.GWL_EXSTYLE, newStyle);
			}
		}
		#endregion //VerifyWindowStyleBits

		#region WmContextMenu
		private IntPtr WmContextMenu(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			NativeWindowMethods.POINT screenPt = NativeWindowMethods.PointFromLParam(lParam);
			Point? pt = null;

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// when minimized let the os system menu show
			if (this.WindowState == WindowState.Minimized)
			{
				// if the window is providing the chrome it will show the menu
				if (this.UseSystemNonClientArea)
					return IntPtr.Zero;

				// if we're providing the chrome then we need to show it...

				// if this was from the keyboard...
				if (screenPt.X == -1 && screenPt.Y == -1)
					NativeWindowMethods.GetCursorPos(ref screenPt);

				// initialize the system menu
				this.ShowContentContextMenu(hwnd, null);

				// get the menu and show it
				IntPtr sysMenu = NativeWindowMethods.GetSystemMenuApi(hwnd, false);

				if (sysMenu != IntPtr.Zero)
				{
					handled = true;

					uint flags = NativeWindowMethods.TPM_RIGHTBUTTON |
						NativeWindowMethods.TPM_LEFTALIGN |
						NativeWindowMethods.TPM_RETURNCMD;

					int command = NativeWindowMethods.TrackPopupMenuEx(sysMenu, flags, screenPt.X, screenPt.Y, hwnd, IntPtr.Zero);

					if (command != 0)
						NativeWindowMethods.PostMessage(hwnd, NativeWindowMethods.WM_SYSCOMMAND, new IntPtr(command), IntPtr.Zero);

					return IntPtr.Zero;
				}
			}

			if (screenPt.X != -1 || screenPt.Y != -1)
			{
				// AS 9/15/09 TFS22166
				IntPtr htResult = NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_NCHITTEST, IntPtr.Zero, lParam);
				NativeWindowMethods.HitTestResults actualResult = (NativeWindowMethods.HitTestResults)htResult.ToInt64();

				switch (actualResult)
				{
					// If the request is from within the window then we would already 
					// have gotten and processed a ContextMenuOpening so we don't want 
					// to try and show the context menu for the window.
					case NativeWindowMethods.HitTestResults.HTCLIENT:

					// we shouldn't try to show a context menu for the borders
					case NativeWindowMethods.HitTestResults.HTBORDER:
					case NativeWindowMethods.HitTestResults.HTBOTTOM:
					case NativeWindowMethods.HitTestResults.HTBOTTOMLEFT:
					case NativeWindowMethods.HitTestResults.HTBOTTOMRIGHT:
					case NativeWindowMethods.HitTestResults.HTLEFT:
					case NativeWindowMethods.HitTestResults.HTRIGHT:
					case NativeWindowMethods.HitTestResults.HTTOP:
					case NativeWindowMethods.HitTestResults.HTTOPLEFT:
					case NativeWindowMethods.HitTestResults.HTTOPRIGHT:

					// resize grip
					case NativeWindowMethods.HitTestResults.HTGROWBOX:

					// scroll bars
					case NativeWindowMethods.HitTestResults.HTHSCROLL:
					case NativeWindowMethods.HitTestResults.HTVSCROLL:
						return IntPtr.Zero;
				}

				pt = this.PointFromScreen(new Point(screenPt.X, screenPt.Y));
			}

			// show the toolwindow's context menu
			if (this.ShowContentContextMenu(hwnd, pt))
			{
				handled = true;
				return IntPtr.Zero;
			}

			return IntPtr.Zero;
		} 
		#endregion //WmContextMenu

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WmExitSizeMove
		private IntPtr WmExitSizeMove(IntPtr hwnd, ref bool handled )
		{
			// track that we are in a move/size operation
			this.SetFlag(InternalFlags.IsInMoveSize, false);

			// clear the moving flag in case a move starts and ends while we are starting it
			this.SetFlag(InternalFlags.IsMovingWindow, false);

			bool isTrackingMove = this.GetFlag(InternalFlags.TrackWmMoving);
			this.SetFlag(InternalFlags.TrackWmMoving, false);

			if (null != _content && isTrackingMove)
			{
				// AS 5/8/12 TFS107054
				// Mark the event handled if something was done.
				//
				if (_content.OnExitMove())
					handled = true;
			}

			return IntPtr.Zero;
		}
		#endregion //WmExitSizeMove

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WmEnterSizeMove
		private IntPtr WmEnterSizeMove(IntPtr hwnd)
		{
			// track that we are in a move/size operation
			this.SetFlag(InternalFlags.IsInMoveSize, true);

			if (null != _content && this.GetFlag(InternalFlags.IsMovingWindow))
			{
				bool trackMove = _content.OnEnterMove();
				this.SetFlag(InternalFlags.TrackWmMoving, trackMove);
			}

			return IntPtr.Zero;
		}
		#endregion //WmEnterSizeMove

		// AS 6/24/11 FloatingWindowCaptionSource
		// This isn't really specific to this issue. Basically when maximizing a window where the window style is 
		// None (like when our UseOsNonClientArea is false), the maximized window overlays the taskbar. It doesn't 
		// seem like we can conditionally handle this message when we are about to be maximized. Instead it seems 
		// like we need to always handle this when UseSystemNonClientArea is false and base the position and 
		// maximized size on the related monitor.
		//
		#region WmGetMinMaxInfo
		private IntPtr WmGetMinMaxInfo(IntPtr hwnd, IntPtr lParam)
		{
			if (this.UseSystemNonClientArea)
				return IntPtr.Zero;

			IntPtr hMonitor = IntPtr.Zero;

			if (this.GetFlag(InternalFlags.IsInMoveSize))
			{
				// in windows 7 the user can drag the window into a maximized state. well if the system's 
				// show window contents while dragging is off then we can't use the monitor that the window 
				// is within. instead we have to use the window that the mouse is within since that is 
				// where the outline will be
				int pos = NativeWindowMethods.GetMessagePos();
				int x = NativeWindowMethods.SignedLoWord(pos);
				int y = NativeWindowMethods.SignedHiWord(pos);

				hMonitor = NativeWindowMethods.MonitorFromPoint(new NativeWindowMethods.POINT(x, y), NativeWindowMethods.MONITOR_DEFAULTTONEAREST);
			}
			else
			{
				hMonitor = NativeWindowMethods.MonitorFromWindow(hwnd, NativeWindowMethods.MONITOR_DEFAULTTONEAREST);
			}

			if (IntPtr.Zero != hMonitor)
			{
				// AS 1/13/12 TFS99296
				// The ptMaxSize is based upon the primary monitor size so we need to get the primary monitor as well.
				//
				IntPtr hMonitorPrimary = NativeWindowMethods.MonitorFromPoint(new NativeWindowMethods.POINT(0, 0), NativeWindowMethods.MONITOR_DEFAULTTOPRIMARY);

				var monitor = new NativeWindowMethods.MONITORINFO();
				monitor.cbSize = (uint)Marshal.SizeOf(monitor.GetType());

				if (NativeWindowMethods.GetMonitorInfo(hMonitor, out monitor) && IntPtr.Zero != hMonitorPrimary)
				{
					var monitorPrimary = new NativeWindowMethods.MONITORINFO();
					monitorPrimary.cbSize = (uint)Marshal.SizeOf(monitor.GetType());

					// AS 1/13/12 TFS99296
					// If the monitor we're going to use is not the primary then get the primary otherwise 
					// we can just use the monitor information we already have.
					//
					if (hMonitor == hMonitorPrimary)
						monitorPrimary = monitor;
					else if (!NativeWindowMethods.GetMonitorInfo(hMonitorPrimary, out monitorPrimary))
						return IntPtr.Zero;

					var minMaxInfo = (NativeWindowMethods.MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.MINMAXINFO));

					// AS 11/10/11 TFS95834
					// The point is not relative to the monitor so we don't need to offset the maximized position.
					//
					//minMaxInfo.ptMaxPosition.X = monitor.rcWork.left - monitor.rcMonitor.left;
					//minMaxInfo.ptMaxPosition.Y = monitor.rcWork.top - monitor.rcMonitor.top;
					minMaxInfo.ptMaxSize.X = Math.Abs(monitor.rcWork.right - monitor.rcWork.left);
					minMaxInfo.ptMaxSize.Y = Math.Abs(monitor.rcWork.bottom - monitor.rcWork.top);

					// AS 1/13/12 TFS99296
					// The ptMaxSize is based upon the primary monitor. If the scale is the same 
					// or there's only one monitor we can do what we did before and just use the 
					// work area. However if the sizes are different then we have to do something 
					// else. I tried all manner of scaling factors but got very strange results. 
					// The only thing that seemed to work consistently is to use the actual size 
					// of the primary monitor. In this case the window would be scaled to fill the 
					// target monitor. Unfortunately that would cover any taskbars, etc. that 
					// reducing the working area. To get around that we'll try to manipulate 
					// the size in the WM_WINDOWPOSCHANGING as well.
					//
					if (monitor.rcMonitor.Width != monitorPrimary.rcMonitor.Width || 
					    monitor.rcMonitor.Height != monitorPrimary.rcMonitor.Height)
					{
						minMaxInfo.ptMaxSize.X = monitorPrimary.rcMonitor.Width;
						minMaxInfo.ptMaxSize.Y = monitorPrimary.rcMonitor.Height - 1;
					}

					// AS 11/11/11 TFS95834
					// These values may be offset by the cxWindowBorders because the WS_THICKFRAME
					// bit is set. Since we don't have any non-client area we'll set these to 0.
					//
					minMaxInfo.ptMaxPosition.X = 0;
					minMaxInfo.ptMaxPosition.Y = 0;

					Marshal.StructureToPtr(minMaxInfo, lParam, false);
				}
			}

			return IntPtr.Zero;
		} 
		#endregion //WmGetMinMaxInfo

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region WmMoving
		private IntPtr WmMoving(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// do not spend the overhead processing the event if the listener isn't interested
			if (!this.GetFlag(InternalFlags.TrackWmMoving))
				return IntPtr.Zero;

			if (null != _content)
			{
				int pos = NativeWindowMethods.GetMessagePos();
				int x = NativeWindowMethods.SignedLoWord(pos);
				int y = NativeWindowMethods.SignedHiWord(pos);

				NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
				int left = wi.rcClient.left;
				int top = wi.rcClient.top;

				Point logicalPt = this.ConvertToLogicalPixels(x - left, y - top);

				NativeWindowMethods.RECT rect = (NativeWindowMethods.RECT)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.RECT));

				Rect logicalScreenRect = new Rect(
					Utilities.ConvertToLogicalPixels(rect.left),
					Utilities.ConvertToLogicalPixels(rect.top),
					Utilities.ConvertToLogicalPixels(rect.Width),
					Utilities.ConvertToLogicalPixels(rect.Height));

				Rect originalRect = logicalScreenRect;

				_content.OnWindowMoving(logicalPt, Mouse.PrimaryDevice, logicalScreenRect);
			}

			return IntPtr.Zero;
		}
		#endregion //WmMoving

		#region WmNcCalcSize
		private IntPtr WmNcCalcSize(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (this.UseSystemNonClientArea == false && wParam.ToInt64() == 1)
			{
				NativeWindowMethods.NCCALCSIZE_PARAMS ncParams = (NativeWindowMethods.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.NCCALCSIZE_PARAMS));

				// We need to make sure that we leave enough room on the bottom portion of the window so that an
				// auto-hidden taskbar can be reshown when this window is maximized.
				if (NativeWindowMethods.GetWindowState(hwnd) == WindowState.Maximized)
				{
					// Make sure that the window's height is not greater than the height of the screen.
					int screenHeight = NativeWindowMethods.GetScreenHeight(hwnd);
					int heightDifference = ncParams.rectProposed.Height + ncParams.rectProposed.top - screenHeight;
					if (heightDifference > 0)
					{
						ncParams.rectProposed.bottom -= heightDifference + 1;
						Marshal.StructureToPtr(ncParams, lParam, false);
					}
				}

				handled = true;
			}

			return IntPtr.Zero;
		} 
		#endregion //WmNcCalcSize

		#region WmNcHitTest
		private IntPtr WmNcHitTest(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			Point pt = new Point(NativeWindowMethods.GetXFromLParam(lParam), NativeWindowMethods.GetYFromLParam(lParam));
			pt = this.PointFromScreen(pt);

			DependencyObject hitTestElement = this.InputHitTest(pt) as DependencyObject;

			FrameworkElement element = hitTestElement as FrameworkElement;

			if (hitTestElement != null && element == null)
				element = Utilities.GetAncestorFromType(hitTestElement, typeof(FrameworkElement), true) as FrameworkElement;

			if (null != element && element.TemplatedParent == this._content)
			{
				NativeWindowMethods.HitTestResults htResult = ToolWindow.GetHitTestCode(element);
				handled = true;
				return new IntPtr((int)htResult);
			}

			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// When we are providing the non-client area, we should consider 
			// hittesting something above to mean client area and not over 
			// the window for anything else.
			//
			if (!this.UseSystemNonClientArea)
			{
				NativeWindowMethods.HitTestResults htResult = hitTestElement != null
					? NativeWindowMethods.HitTestResults.HTCLIENT
					: NativeWindowMethods.HitTestResults.HTTRANSPARENT;

				handled = true;
				return new IntPtr((int)htResult);
			}

			return IntPtr.Zero;
		} 
		#endregion //WmNcHitTest

		#region WmNclButtonDblClk
		private IntPtr WmNclButtonDblClk(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			if (null != this._content && NativeWindowMethods.IntPtrToInt32(wParam) == (int)NativeWindowMethods.HitTestResults.HTCAPTION)
			{
				int x = NativeWindowMethods.GetXFromLParam(lParam);
				int y = NativeWindowMethods.GetYFromLParam(lParam);

				// AS 11/7/08 TFS7872
				// There must have been a bug in the original 3.0 Window class
				// implementation such that the Top/Left were wrong. Instead
				// we'll use the native window info.
				//
				//NativeWindowMethods.POINT formPt = this.ConvertFromLogicalPixels(this.Left, this.Top);
				//x -= formPt.X;
				//y -= formPt.Y;
				NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
				x -= wi.rcWindow.left;
				y -= wi.rcWindow.top;

				// now we've got the offset into the window convert to logical coords
				Point relativePt = this.ConvertToLogicalPixels(x, y);

				if (true == this._content.OnDoubleClickCaption(relativePt, Mouse.PrimaryDevice))
				{
					handled = true;
					return IntPtr.Zero;
				}
			}

			return IntPtr.Zero;
		} 
		#endregion //WmNclButtonDblClk

		#region WmShowWindow
		private IntPtr WmShowWindow(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// AS 10/13/08 TFS6107/BR34010
			// If we let the toolwindow asynchronously calculate its min/max then it will
			// happen when the window is visible in which case you will get a flicker. To
			// try and avoid this the toolwindow can keep track of whether a calculate is needed
			// and then we will force it before the show.
			// 
			if (IntPtr.Zero == lParam && IntPtr.Zero != wParam)
			{
				ToolWindow tw = this.Content as ToolWindow;

				if (tw.IsDelayedMinMaxPending)
					tw.CalculateMinMaxExtents(false);
			}
			else
			{
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// When the owner is minimized and subsequently restored, any minimized/maximized 
				// owned windows are restored when they are shown.
				//
				if (IntPtr.Zero != wParam && lParam.ToInt32() == NativeWindowMethods.SW_PARENTOPENING)
				{
					NativeWindowMethods.WINDOWPLACEMENT placement = NativeWindowMethods.GetWindowPlacement(hwnd);

					// AS 1/9/12 TFS98950
					// Removing the minimized portion. While this works as one would expect when just manually 
					// minimizing the floater and then minimizing and subsequently restoring the owner (and also 
					// allows the case where one just restores the owner to keep the floater restore) it causes 
					// a problem when one uses WinKey + D to show the desktop since the OS is telling each top 
					// level window to minimize. When that happens we would get into the following block while 
					// the parent is being restored unknowing that the OS would have restored as well. Since 
					// this was really meant to be consistent with the maximize (which was implemented this 
					// way - i.e. to remaximize the owned floater when the owner is restored from the minimized 
					// state - to mimic VS 2010).
					//
					//if (placement.showCmd == NativeWindowMethods.SW_SHOWMINIMIZED)
					//{
					//	NativeWindowMethods.ShowWindow(hwnd, NativeWindowMethods.SW_SHOWMINNOACTIVE);
					//}
					//else if (placement.showCmd == NativeWindowMethods.SW_SHOWMAXIMIZED)
					if (placement.showCmd == NativeWindowMethods.SW_SHOWMAXIMIZED)
					{
						NativeWindowMethods.ShowWindow(hwnd, NativeWindowMethods.SW_SHOWNA);
					}
				}
			}

			return IntPtr.Zero;
		} 
		#endregion //WmShowWindow

		#region WmSize
		private IntPtr WmSize(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{




			if (this.UseSystemNonClientArea == false)
			{
				// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// It seems like the HndSource does not update the measure size for the element 
				// when it is minimized. When using the os nonclient area this isn't a problem 
				// since our element's content is not shown but when we're not using the os 
				// non client area then this causes our window element to be measured with the 
				// last size (e.g. the maximized size).
				//
				if (NativeWindowMethods.GetWindowState(hwnd) == WindowState.Minimized)
				{
					int sizeWord = NativeWindowMethods.IntPtrToInt32(lParam);
					int width = NativeWindowMethods.SignedLoWord(sizeWord);
					int height = NativeWindowMethods.SignedHiWord(sizeWord);

					Point pt = this.ConvertToLogicalPixels(width, height);
					Size size = new Size(pt.X, pt.Y);
					this.Measure(size);
					this.Arrange(new Rect(size));
				}

				// I was going to limit this to when the window was normal but the Window class
				// doesn't seem to make any distinction so we'll always do this
				//if (wParam.ToInt64() == NativeWindowMethods.SIZE_RESTORED)
				// AS 5/8/09 TFS17053
				// I found this while testing the changes to show the window when we don't have 
				// an owning wpf window. I had a toolwindow that didn't use any non-client area
				// and was vertically stretched and aligned right to the owner. When I resized 
				// the window I found a problem where the resulting size was smaller. This happened 
				// because the Window class does not adjust the window rect if the window allows 
				// transparency. So our attempt to compensate for that issue actually made the 
				// window smaller because the window didn't add the non-client size back in.
				//
				// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// Don't process this while minimized or the window may end up forcing it 
				// to return to the normal state.
				//
				//if (!this.AllowsTransparency)
				// AS 11/10/11 TFS95827
				// The WindowState DP was not yet updated so check the actual state of the window.
				//
				//if (!this.AllowsTransparency && this.WindowState != WindowState.Minimized && !this.GetFlag(InternalFlags.IsSendingWmSize))
				if (!this.AllowsTransparency && NativeWindowMethods.GetWindowState(hwnd) != WindowState.Minimized && !this.GetFlag(InternalFlags.IsSendingWmSize))
				{
					// the Window class takes the size that is passed into the WM_SIZE and calls
					// AdjustWindowRect. That method adds in the non-client size. when we
					// are not using the os nonclient area, this causes the actual width/height to
					// differ from the width/height because while the style bits indicate there
					// should be non-client area, there really isn't since we are handling the 
					// wm_nccalcsize
					int sizeWord = NativeWindowMethods.IntPtrToInt32(lParam);
					int width = NativeWindowMethods.SignedLoWord(sizeWord);
					int height = NativeWindowMethods.SignedHiWord(sizeWord);
					NativeWindowMethods.SIZE nonClientSize = NativeWindowMethods.GetNonClientSize(hwnd);

					// AS 11/10/11 TFS95827
					// Just in case we'll make sure we don't create a value with negative numbers.
					//
					//width -= nonClientSize.width;
					//height -= nonClientSize.height;
					width = Math.Max(width - nonClientSize.width, 0);
					height = Math.Max(height - nonClientSize.height, 0);

					IntPtr newLParam = NativeWindowMethods.EncodeSize(width, height);

					// We need to let the base implementation handle the message first, otherwise
					// we will be able to hit-test the buttons correctly, but they won't actually
					// do anything when clicking them.
					// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// Keep a more granular antirecursion flag so we can process any other messages 
					// that happen as a result of the size processing.
					//
					//this.SendMessage(hwnd, NativeWindowMethods.WM_SIZE, wParam, newLParam);
					try
					{
						this.SetFlag(InternalFlags.IsSendingWmSize, true);
						NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_SIZE, wParam, newLParam);
					}
					finally
					{
						this.SetFlag(InternalFlags.IsSendingWmSize, false);
					}
					handled = true;
					return IntPtr.Zero;
				}
			}

			return IntPtr.Zero;
		} 
		#endregion //WmSize

		#region WmStyleChanging
		private IntPtr WmStyleChanging(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			int styleFlags = wParam.ToInt32();

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			//if (NativeWindowMethods.AreBitsPresent(wParam, NativeWindowMethods.GWL_STYLE))
			if (styleFlags == NativeWindowMethods.GWL_STYLE)
			{
				NativeWindowMethods.STYLESTRUCT style = (NativeWindowMethods.STYLESTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.STYLESTRUCT));
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//style.styleNew &= ~(NativeWindowMethods.WS_MAXIMIZEBOX | NativeWindowMethods.WS_MINIMIZEBOX);
				ToolWindow tw = _content;

				if (tw == null || !tw.AllowMaximize)
					style.styleNew &= ~NativeWindowMethods.WS_MAXIMIZEBOX;
				else
					style.styleNew |= NativeWindowMethods.WS_MAXIMIZEBOX;

				if (tw == null || !tw.AllowMinimize)
					style.styleNew &= ~NativeWindowMethods.WS_MINIMIZEBOX;
				else
					style.styleNew |= NativeWindowMethods.WS_MINIMIZEBOX;

				Marshal.StructureToPtr(style, lParam, false);





			}
			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			else if (styleFlags == NativeWindowMethods.GWL_EXSTYLE)
			{
				NativeWindowMethods.STYLESTRUCT style = (NativeWindowMethods.STYLESTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.STYLESTRUCT));

				if (this.WindowStyle == WindowStyle.SingleBorderWindow)
					style.styleNew |= NativeWindowMethods.WS_EX_DLGMODALFRAME;
				else
					style.styleNew &= ~NativeWindowMethods.WS_EX_DLGMODALFRAME;

				Marshal.StructureToPtr(style, lParam, false);





			}

			return IntPtr.Zero;
		} 
		#endregion //WmStyleChanging

		#region WmSysCommand
		private IntPtr WmSysCommand(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			// the low 4 bits are used by the system so ignore them
			int command = wParam.ToInt32() & 0xFFF0;
			ToolWindow content = _content;

			switch (command)
			{
				// we need to be able to override a form drag operation for a dockmanager
				// floating window so we'll call a virtual method on the window when about
				// to start a drag operation. like vs though we will not start such an operation
				// if you just use the system menu to perform a move.
				case NativeWindowMethods.SC_MOVE:
					// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
					// When someone starts the drag directly (e.g. when using SC_DRAGMOVE, then 
					// the lParam is 0 in which case we can just use the current window pos
					//
					//if (lParam != IntPtr.Zero && content != null)
					if (content != null)
					{
						// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
						int x;
						int y;

						if (lParam != IntPtr.Zero)
						{
							x = NativeWindowMethods.GetXFromLParam(lParam);
							y = NativeWindowMethods.GetYFromLParam(lParam);
						}
						else
						{
							NativeWindowMethods.POINT pt = new NativeWindowMethods.POINT();
							if (0 == NativeWindowMethods.GetCursorPos(ref pt))
								return IntPtr.Zero;

							x = pt.X;
							y = pt.Y;
						}

						// AS 6/8/11 TFS76337
						// The point should be relative to the content.
						//
						//// AS 11/7/08 TFS7872
						//// There must have been a bug in the original 3.0 Window class
						//// implementation such that the Top/Left were wrong. Instead
						//// we'll use the native window info.
						////
						////NativeWindowMethods.POINT formPt = this.ConvertFromLogicalPixels(this.Left, this.Top);
						////x -= formPt.X;
						////y -= formPt.Y;
						//NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
						//x -= wi.rcWindow.left;
						//y -= wi.rcWindow.top;
						
						//// now we've got the offset into the window convert to logical coords
						//Point relativePt = this.ConvertToLogicalPixels(x, y);
						Point relativePt = content.PointFromScreen(new Point(x, y));

						// AS 2/22/12 TFS101038
						//if (true == content.OnDragCaption(relativePt, Mouse.PrimaryDevice))
						var windowHandledDrag = false;
						var wasTouchDrag = content.IsNonClientTouchDrag;

						try
						{
							content.IsNonClientTouchDrag = (NativeWindowMethods.GetBits(NativeWindowMethods.GetMessageExtraInfo()) & NativeWindowMethods.MOUSEEVENTF_FROMTOUCH) == NativeWindowMethods.MOUSEEVENTF_FROMTOUCH;
							windowHandledDrag = content.OnDragCaption(relativePt, Mouse.PrimaryDevice);
						}
						finally
						{
							content.IsNonClientTouchDrag = wasTouchDrag;
						}

						if (windowHandledDrag)
						{
							handled = true;
							return IntPtr.Zero;
						}
						else
						{
							// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
							// If we are to let the window perform a drag then explicitly start it now so 
							// we can notify the toolwindow that the move was cancelled if we couldn't start 
							// it.
							//
							this.SetFlag(InternalFlags.IsMovingWindow, true);

							// let the window start the operation
							IntPtr result = NativeWindowMethods.DefWindowProc(hwnd, NativeWindowMethods.WM_SYSCOMMAND, wParam, lParam);

							// the toolwindow may be expecting to be in a drag so if our flag is not set (i.e. 
							// the enter and exit were not called synchronously) and the move/size isn't in 
							// progress then let the window know so it can process accordingly
							if (this.GetFlag(InternalFlags.IsMovingWindow) && !this.GetFlag(InternalFlags.IsInMoveSize))
							{
								content.OnMoveCancelled();
							}

							handled = true;
							return result;
						}
					}
					break;

				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// These are now conditionally allowed based on the associated allow states.
				//
				//case NativeWindowMethods.SC_MINIMIZE:
				//case NativeWindowMethods.SC_MAXIMIZE:
				//    // do not allow minimize/maximize
				//    handled = true;
				//    return IntPtr.Zero;
				case NativeWindowMethods.SC_MAXIMIZE:
					{
						if (null == content || !content.AllowMaximize)
						{
							handled = true;
							return IntPtr.Zero;
						}
						break;
					}
				case NativeWindowMethods.SC_MINIMIZE:
					{
						if (null == content || !content.AllowMinimize)
						{
							// do not allow minimize/maximize
							handled = true;
							return IntPtr.Zero;
						}
						break;
					}
				case NativeWindowMethods.SC_RESTORE: // AS 6/15/12 TFS103939
					{
						var currentState = NativeWindowMethods.GetWindowState(hwnd);

						if (currentState == System.Windows.WindowState.Maximized &&
							(null == content || !content.AllowMaximize))
						{
							handled = true;
							return IntPtr.Zero;
						}
						break;
					}
				case NativeWindowMethods.SC_MOUSEMENU:
				case NativeWindowMethods.SC_KEYMENU:
					// show the toolwindow's context menu
					if (this.ShowContentContextMenu(hwnd, null))
					{
						handled = true;
						return IntPtr.Zero;
					}
					break;
			}

			return IntPtr.Zero;
		} 
		#endregion //WmSysCommand

		#region WmWindowPosChanging
		private IntPtr WmWindowPosChanging(IntPtr hwnd, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			NativeWindowMethods.WINDOWPOS windowPosStruct = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.WINDOWPOS));





			var windowState = NativeWindowMethods.GetWindowState(hwnd);

			// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// If during a move the size of the window changes (such as when you drag to the 
			// right/left edge of a window in windows 7) excluding when going to maximized, 
			// then we need to change the SizeToContent to Manual or else the window will remain 
			// sizing to content.
			//
			if (this.GetFlag(InternalFlags.IsInMoveSize))
			{
				// if the window is being resized then explicitly set the SizeToContent
				if ((windowPosStruct.flags & NativeWindowMethods.SWP_NOSIZE) == 0)
				{
					if (windowState == WindowState.Normal)
					{
						NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);

						if (wi.rcWindow.Width != windowPosStruct.cx ||
							wi.rcWindow.Height != windowPosStruct.cy)
						{
							this.SizeToContent = SizeToContent.Manual;
						}
					}
				}
			}

			// AS 11/11/11 TFS95869
			// In some scenarios the maximized window is getting offset based on the 
			// cx & cy border widths. Since we don't have any non client area it should 
			// fill the available area.
			//
			if ((windowPosStruct.flags & NativeWindowMethods.SWP_STATECHANGED) == NativeWindowMethods.SWP_STATECHANGED ||
				(windowPosStruct.flags & NativeWindowMethods.SWP_NOMOVE) != NativeWindowMethods.SWP_NOMOVE)
			{
				if (windowState == WindowState.Maximized && this.UseSystemNonClientArea == false)
				{
					Point midPt = new Point(windowPosStruct.x + windowPosStruct.cx / 2, windowPosStruct.y + windowPosStruct.cy / 2);

					// AS 1/13/12 TFS99296
					// In the WM_GETMINMAXINFO handling we had to return the full screen size of the primary 
					// monitor when the primary and target monitor were different sizes. Anything else and the 
					// OS did some strange (seeming inconsistent) scaling. Since that would obscure any taskbars 
					// in the target monitor, we'll adjust the maximize size here to be that of the working 
					// area of the target monitor.
					//
					IntPtr hMonitor = NativeWindowMethods.MonitorFromPoint(new NativeWindowMethods.POINT((int)midPt.X, (int)midPt.Y), NativeWindowMethods.MONITOR_DEFAULTTONEAREST);
					IntPtr hMonitorPrimary = NativeWindowMethods.MonitorFromPoint(new NativeWindowMethods.POINT(0, 0), NativeWindowMethods.MONITOR_DEFAULTTOPRIMARY);

					if (hMonitor != hMonitorPrimary)
					{
						var monitorActual = new NativeWindowMethods.MONITORINFO();
						monitorActual.cbSize = (uint)Marshal.SizeOf(monitorActual.GetType());

						if (NativeWindowMethods.GetMonitorInfo(hMonitor, out monitorActual))
						{
							var monitorPrimary = new NativeWindowMethods.MONITORINFO();
							monitorPrimary.cbSize = (uint)Marshal.SizeOf(monitorPrimary.GetType());

							if (NativeWindowMethods.GetMonitorInfo(hMonitorPrimary, out monitorPrimary))
							{
								if (monitorActual.rcMonitor.Width != monitorPrimary.rcMonitor.Width ||
									monitorActual.rcMonitor.Height != monitorPrimary.rcMonitor.Height)
								{
									windowPosStruct.cx = monitorActual.rcWork.Width;
									windowPosStruct.cy = monitorActual.rcWork.Height;
								}
							}
						}
					}

					Rect workRect = NativeWindowMethods.GetWorkArea(midPt);
					windowPosStruct.x = (int)workRect.X;
					windowPosStruct.y = (int)workRect.Y;
					Marshal.StructureToPtr(windowPosStruct, lParam, false);
				}
			}

			// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// The HwndSource disables SizeToContent when various size operations are to occur (e.g. when WM_SIZING message
			// comes in, when WM_SYSCOMMAND for SC_MAXIMIZE or SC_SIZING) but not when the window state is changing 
			// during a drag operation as occurs in Windows 7 when dragging to the window boundaries. When that happens the 
			// HwndSource still processes the WM_WINDOWPOSCHANGING and munges the width/height based on the SizeToContent 
			// setting. Then when the Window class gets the WM_SIZE message it has the SizeToContent based extent which 
			// it then uses to explicitly set the Width/Height properties. So the window ends up being maximized but since 
			// its Width/Height are set to explicit values then that is the size of the window.
			//
			if ((windowPosStruct.flags & NativeWindowMethods.SWP_STATECHANGED) == NativeWindowMethods.SWP_STATECHANGED)
			{
				if (windowState == WindowState.Maximized &&
					// This also seems to happen when just setting the WindowState of the Window.
					//this.WindowState == WindowState.Normal &&
					this.SizeToContent != SizeToContent.Manual)
				{
					this.CoerceValue(SizeToContentProperty);
				}

				this.SetRestoreToMaximize(hwnd);
			}

			// AS 5/8/09 TFS17053
			// When we don't have an owner then the OwnerRect will not be updated and we 
			// have to find an alternate means of knowing when to update the relative 
			// position. The problem is that the element that is the owner can be within 
			// another window that is moved. It seems though that our window will get a
			// WM_WINDOWPOSCHANGING when the owner is activated/moved so if we get this 
			// message when we don't have an owner and we're not about to be resized/moved 
			// then we will update our relative position.
			//
			if (this.Owner == null)
			{
				// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
				//NativeWindowMethods.WINDOWPOS windowPosStruct = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.WINDOWPOS));

				// if we're getting this but our window is not being sized or moved
				// then we'll use this verify our position relative to the owner
				if ((windowPosStruct.flags & NativeWindowMethods.SWP_NOMOVE) != 0 &&
					(windowPosStruct.flags & NativeWindowMethods.SWP_NOSIZE) != 0)
				{
					this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
						new ToolWindow.MethodInvoker(this.UpdateRelativePosition));
				}
			}

			return IntPtr.Zero;
		} 
		#endregion //WmWindowPosChanging

		#region WndProc
		private IntPtr WndProc(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
//            if (this.IsInSendMessage)
//            {
//#if DEBUG_WNDPROC
//                Debug.WriteLine(msg.ToString(), string.Format("[{0}] {1}", DateTime.Now.ToString("hh:mm:ss:ffffff"), "** In SendMessage **"));
//#endif
//                return IntPtr.Zero;
//            }


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			try
			{





				this._wndProcCount++;
				return this.WndProcImpl(hwnd, message, wParam, lParam, ref handled);
			}
			finally
			{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)






				this._wndProcCount--;
			}
		}

		private IntPtr WndProcImpl(IntPtr hwnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch (message)
			{
				#region WM_WINDOWPOSCHANGING
				case NativeWindowMethods.WM_WINDOWPOSCHANGING:
				{
					// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
					//// AS 5/8/09 TFS17053
					//// When we don't have an owner then the OwnerRect will not be updated and we 
					//// have to find an alternate means of knowing when to update the relative 
					//// position. The problem is that the element that is the owner can be within 
					//// another window that is moved. It seems though that our window will get a
					//// WM_WINDOWPOSCHANGING when the owner is activated/moved so if we get this 
					//// message when we don't have an owner and we're not about to be resized/moved 
					//// then we will update our relative position.
					////
					//if (this.Owner == null)
					//{
					//    NativeWindowMethods.WINDOWPOS windowPosStruct = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.WINDOWPOS));
					//
					//    // if we're getting this but our window is not being sized or moved
					//    // then we'll use this verify our position relative to the owner
					//    if ((windowPosStruct.flags & NativeWindowMethods.SWP_NOMOVE) != 0 &&
					//        (windowPosStruct.flags & NativeWindowMethods.SWP_NOSIZE) != 0)
					//    {
					//        this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
					//            new ToolWindow.MethodInvoker(this.UpdateRelativePosition));
					//    }
					//}
					//break; 
					return this.WmWindowPosChanging(hwnd, wParam, lParam, ref handled);
				}
				#endregion //WM_WINDOWPOSCHANGING

				#region WM_WINDOWPOSCHANGED
				case NativeWindowMethods.WM_WINDOWPOSCHANGED:
				{



					break;
				}
				#endregion //WM_WINDOWPOSCHANGED

				#region WM_DWMCOMPOSITIONCHANGED

				// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
				// This isn't being used.
				//
				//// Handle the case where the user enables or disables glass while
				//// the application is running.
				//case NativeWindowMethods.WM_DWMCOMPOSITIONCHANGED:
				//    // Reset our flag to the new value
				//    this.CachedIsDwmCompositionEnabled = NativeWindowMethods.IsDWMCompositionEnabled;
				//    break;

				#endregion //WM_DWMCOMPOSITIONCHANGED

				#region WM_CONTEXTMENU
				case NativeWindowMethods.WM_CONTEXTMENU:
					{
						// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//NativeWindowMethods.POINT screenPt = NativeWindowMethods.PointFromLParam(lParam);
						//Point? pt = null;
						//
						//if (screenPt.X != -1 || screenPt.Y != -1)
						//{
						//    // AS 9/15/09 TFS22166
						//    IntPtr htResult = NativeWindowMethods.SendMessageApi(hwnd, NativeWindowMethods.WM_NCHITTEST, IntPtr.Zero, lParam);
						//    NativeWindowMethods.HitTestResults actualResult = (NativeWindowMethods.HitTestResults)htResult.ToInt64();
						//
						//    switch (actualResult)
						//    {
						//        // If the request is from within the window then we would already 
						//        // have gotten and processed a ContextMenuOpening so we don't want 
						//        // to try and show the context menu for the window.
						//        case NativeWindowMethods.HitTestResults.HTCLIENT:
						//
						//        // we shouldn't try to show a context menu for the borders
						//        case NativeWindowMethods.HitTestResults.HTBORDER:
						//        case NativeWindowMethods.HitTestResults.HTBOTTOM:
						//        case NativeWindowMethods.HitTestResults.HTBOTTOMLEFT:
						//        case NativeWindowMethods.HitTestResults.HTBOTTOMRIGHT:
						//        case NativeWindowMethods.HitTestResults.HTLEFT:
						//        case NativeWindowMethods.HitTestResults.HTRIGHT:
						//        case NativeWindowMethods.HitTestResults.HTTOP:
						//        case NativeWindowMethods.HitTestResults.HTTOPLEFT:
						//        case NativeWindowMethods.HitTestResults.HTTOPRIGHT:
						//
						//            // resize grip
						//        case NativeWindowMethods.HitTestResults.HTGROWBOX:
						//
						//            // scroll bars
						//        case NativeWindowMethods.HitTestResults.HTHSCROLL:
						//        case NativeWindowMethods.HitTestResults.HTVSCROLL:
						//            return IntPtr.Zero;
						//    }
						//
						//    pt = this.PointFromScreen(new Point(screenPt.X, screenPt.Y));
						//}
						//
						//// show the toolwindow's context menu
						//if (this.ShowContentContextMenu(hwnd, pt))
						//{
						//    handled = true;
						//    return IntPtr.Zero;
						//}
						//break;
						return this.WmContextMenu(hwnd, wParam, lParam, ref handled);
					}
				#endregion //WM_CONTEXTMENU

				// AS 8/4/11 TFS83465/TFS83469
				// When a monitor is added/removed/changed then make sure the window is completely in view.
				//
				#region WM_DISPLAYCHANGE
				case NativeWindowMethods.WM_DISPLAYCHANGE:
					this.VerifyIsInView(false, true, true);
					break;
				#endregion //WM_DISPLAYCHANGE

				#region WM_MOVING
				
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				case NativeWindowMethods.WM_MOVING:
					return this.WmMoving(hwnd, wParam, lParam, ref handled);
				#endregion //WM_MOVING

				#region WM_NCACTIVATE
				case NativeWindowMethods.WM_NCACTIVATE:
					{
						if (this.UseSystemNonClientArea == false)
						{
							// Prevent the OS from rendering the inactive caption above our window.
							handled = true;
							// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
							// Pass -1 along to the defwndproc so it doesn't invalidate the non-client area.
							//
							//return new IntPtr(1);
							return NativeWindowMethods.DefWindowProc(hwnd, message, wParam, new IntPtr(-1));
						}
						break;
					}

				#endregion //WM_NCACTIVATE

				#region WM_NCCALCSIZE

				case NativeWindowMethods.WM_NCCALCSIZE:
					{
						// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//if (this.UseSystemNonClientArea == false && wParam.ToInt64() == 1)
						//{
						//    NativeWindowMethods.NCCALCSIZE_PARAMS ncParams = (NativeWindowMethods.NCCALCSIZE_PARAMS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.NCCALCSIZE_PARAMS));
						//
						//    // We need to make sure that we leave enough room on the bottom portion of the window so that an
						//    // auto-hidden taskbar can be reshown when this window is maximized.
						//    if (NativeWindowMethods.GetWindowState(hwnd) == WindowState.Maximized)
						//    {
						//        // Make sure that the window's height is not greater than the height of the screen.
						//        int screenHeight = NativeWindowMethods.GetScreenHeight(hwnd);
						//        int heightDifference = ncParams.rectProposed.Height + ncParams.rectProposed.top - screenHeight;
						//        if (heightDifference > 0)
						//        {
						//            ncParams.rectProposed.bottom -= heightDifference + 1;
						//            Marshal.StructureToPtr(ncParams, lParam, false);
						//        }
						//    }
						//
						//    handled = true;
						//}
						//break;
						return this.WmNcCalcSize(hwnd, wParam, lParam, ref handled);
					}
				#endregion //WM_NCCALCSIZE

				#region WM_NCHITTEST
				case NativeWindowMethods.WM_NCHITTEST:
					{
						// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//Point pt = new Point(NativeWindowMethods.GetXFromLParam(lParam), NativeWindowMethods.GetYFromLParam(lParam));
						//pt = this.PointFromScreen(pt);
						//
						//DependencyObject hitTestElement = this.InputHitTest(pt) as DependencyObject;
						//
						//FrameworkElement element = hitTestElement as FrameworkElement;
						//
						//if (hitTestElement != null && element == null)
						//    element = Utilities.GetAncestorFromType(hitTestElement, typeof(FrameworkElement), true) as FrameworkElement;
						//
						//if (null != element && element.TemplatedParent == this._content)
						//{
						//    NativeWindowMethods.HitTestResults htResult = ToolWindow.GetHitTestCode(element);
						//    handled = true;
						//    return new IntPtr((int)htResult);
						//}
						//break;
						return this.WmNcHitTest(hwnd, wParam, lParam, ref handled);
					}
				#endregion //WM_NCHITTEST

				#region WM_NCLBUTTONDBLCLK
				case NativeWindowMethods.WM_NCLBUTTONDBLCLK:
					{
						// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//if (null != this._content && NativeWindowMethods.IntPtrToInt32(wParam) == (int)NativeWindowMethods.HitTestResults.HTCAPTION)
						//{
						//    int x = NativeWindowMethods.GetXFromLParam(lParam);
						//    int y = NativeWindowMethods.GetYFromLParam(lParam);
						//
						//    // AS 11/7/08 TFS7872
						//    // There must have been a bug in the original 3.0 Window class
						//    // implementation such that the Top/Left were wrong. Instead
						//    // we'll use the native window info.
						//    //
						//    //NativeWindowMethods.POINT formPt = this.ConvertFromLogicalPixels(this.Left, this.Top);
						//    //x -= formPt.X;
						//    //y -= formPt.Y;
						//    NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
						//    x -= wi.rcWindow.left;
						//    y -= wi.rcWindow.top;
						//
						//    // now we've got the offset into the window convert to logical coords
						//    Point relativePt = this.ConvertToLogicalPixels(x, y);
						//
						//    if (true == this._content.OnDoubleClickCaption(relativePt, Mouse.PrimaryDevice))
						//    {
						//        handled = true;
						//        return IntPtr.Zero;
						//    }
						//}
						//break;
						return this.WmNclButtonDblClk(hwnd, wParam, lParam, ref handled);
					} 
				#endregion //WM_NCLBUTTONDBLCLK

                #region WM_SHOWWINDOW
                case NativeWindowMethods.WM_SHOWWINDOW:
					// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
					//// AS 10/13/08 TFS6107/BR34010
					//// If we let the toolwindow asynchronously calculate its min/max then it will
					//// happen when the window is visible in which case you will get a flicker. To
					//// try and avoid this the toolwindow can keep track of whether a calculate is needed
					//// and then we will force it before the show.
					//// 
					//if (IntPtr.Zero == lParam && IntPtr.Zero != wParam)
					//{
					//    ToolWindow tw = this.Content as ToolWindow;

					//    if (tw.IsDelayedMinMaxPending)
					//        tw.CalculateMinMaxExtents(false);
					//}
					//break; 
					return this.WmShowWindow(hwnd, wParam, lParam, ref handled);
                #endregion //WM_SHOWWINDOW

				#region WM_NCPAINT

				case NativeWindowMethods.WM_NCPAINT:
					{
						// We don't want to draw any non-client area when glass is disabled, but if
						// glass is turned on, eating this message will cause no glass to be rendered.
						//
						// NOTE: We cannot use the IsGlassEnabled property that caches the result
						// of IsDWMCompositionEnabled because when disabling glass (i.e. Vista Basic) and then
						// re-enabling glass, we'll get a series of NCPAINT messages *before* getting the
						// WM_DWMCOMPOSITIONCHANGED message, which results in the glass not being rendered
						// on the form.
						if (this.UseSystemNonClientArea == false && !NativeWindowMethods.IsDWMCompositionEnabled)
						{
							handled = true;
						}
						break;
					}

				#endregion //WM_NCPAINT

				#region WM_NCUAHDRAWCAPTION and WM_NCUAHDRAWFRAME

				case NativeWindowMethods.WM_NCUAHDRAWCAPTION:
				case NativeWindowMethods.WM_NCUAHDRAWFRAME:
					{
						if (this.UseSystemNonClientArea == false)
						{
							handled = true;
						}
						break;
					}

				#endregion //WM_NCUAHDRAWCAPTION and WM_NCUAHDRAWFRAME

				#region WM_SIZE
				case NativeWindowMethods.WM_SIZE:
					{
						// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//if (this.UseSystemNonClientArea == false)
						//{
						//    // I was going to limit this to when the window was normal but the Window class
						//    // doesn't seem to make any distinction so we'll always do this
						//    //if (wParam.ToInt64() == NativeWindowMethods.SIZE_RESTORED)
						//    // AS 5/8/09 TFS17053
						//    // I found this while testing the changes to show the window when we don't have 
						//    // an owning wpf window. I had a toolwindow that didn't use any non-client area
						//    // and was vertically stretched and aligned right to the owner. When I resized 
						//    // the window I found a problem where the resulting size was smaller. This happened 
						//    // because the Window class does not adjust the window rect if the window allows 
						//    // transparency. So our attempt to compensate for that issue actually made the 
						//    // window smaller because the window didn't add the non-client size back in.
						//    //
						//    if (!this.AllowsTransparency)
						//    {
						//        // the Window class takes the size that is passed into the WM_SIZE and calls
						//        // AdjustWindowRect. That method adds in the non-client size. when we
						//        // are not using the os nonclient area, this causes the actual width/height to
						//        // differ from the width/height because while the style bits indicate there
						//        // should be non-client area, there really isn't since we are handling the 
						//        // wm_nccalcsize
						//        int sizeWord = NativeWindowMethods.IntPtrToInt32(lParam);
						//        int width = NativeWindowMethods.SignedLoWord(sizeWord);
						//        int height = NativeWindowMethods.SignedHiWord(sizeWord);
						//        NativeWindowMethods.SIZE nonClientSize = NativeWindowMethods.GetNonClientSize(hwnd);
						//
						//        width -= nonClientSize.width;
						//        height -= nonClientSize.height;
						//
						//        IntPtr newLParam = NativeWindowMethods.EncodeSize(width, height);
						//
						//        // We need to let the base implementation handle the message first, otherwise
						//        // we will be able to hit-test the buttons correctly, but they won't actually
						//        // do anything when clicking them.
						//        this.SendMessage(hwnd, message, wParam, newLParam);
						//        handled = true;
						//        return IntPtr.Zero;
						//    }
						//}
						//break;
						return this.WmSize(hwnd, wParam, lParam, ref handled);
					} 
				#endregion //WM_SIZE

				#region WM_STYLECHANGING
				case NativeWindowMethods.WM_STYLECHANGING:
					{
						// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//int styleFlags = wParam.ToInt32();
						//
						//if (NativeWindowMethods.AreBitsPresent(wParam, NativeWindowMethods.GWL_STYLE))
						//{
						//    NativeWindowMethods.STYLESTRUCT style = (NativeWindowMethods.STYLESTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.STYLESTRUCT));
						//    style.styleNew &= ~(NativeWindowMethods.WS_MAXIMIZEBOX | NativeWindowMethods.WS_MINIMIZEBOX);
						//
						//    Marshal.StructureToPtr(style, lParam, false);
						//}
						//break;
						return this.WmStyleChanging(hwnd, wParam, lParam, ref handled);
					}
				#endregion // WM_STYLECHANGING

				#region WM_SYSCOMMAND
				case NativeWindowMethods.WM_SYSCOMMAND:
					{
						// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
						//// the low 4 bits are used by the system so ignore them
						//int command = wParam.ToInt32() & 0xFFF0;
						//
						//switch (command)
						//{
						//        // we need to be able to override a form drag operation for a dockmanager
						//        // floating window so we'll call a virtual method on the window when about
						//        // to start a drag operation. like vs though we will not start such an operation
						//        // if you just use the system menu to perform a move.
						//    case NativeWindowMethods.SC_MOVE:
						//        if (lParam != IntPtr.Zero && this._content != null)
						//        {
						//            int x = NativeWindowMethods.GetXFromLParam(lParam);
						//            int y = NativeWindowMethods.GetYFromLParam(lParam);
						//
						//            // AS 11/7/08 TFS7872
						//            // There must have been a bug in the original 3.0 Window class
						//            // implementation such that the Top/Left were wrong. Instead
						//            // we'll use the native window info.
						//            //
						//            //NativeWindowMethods.POINT formPt = this.ConvertFromLogicalPixels(this.Left, this.Top);
						//            //x -= formPt.X;
						//            //y -= formPt.Y;
						//            NativeWindowMethods.WINDOWINFO wi = NativeWindowMethods.GetWindowInfo(hwnd);
						//            x -= wi.rcWindow.left;
						//            y -= wi.rcWindow.top;
						//
						//            // now we've got the offset into the window convert to logical coords
						//            Point relativePt = this.ConvertToLogicalPixels(x, y);
						//
						//            if (true == this._content.OnDragCaption(relativePt, Mouse.PrimaryDevice))
						//            {
						//                handled = true;
						//                return IntPtr.Zero;
						//            }
						//        }
						//        break;
						//
						//    case NativeWindowMethods.SC_MINIMIZE:
						//    case NativeWindowMethods.SC_MAXIMIZE:
						//        // do not allow minimize/maximize
						//        handled = true;
						//        return IntPtr.Zero;
						//    case NativeWindowMethods.SC_MOUSEMENU:
						//    case NativeWindowMethods.SC_KEYMENU:
						//        // show the toolwindow's context menu
						//        if (this.ShowContentContextMenu(hwnd, null))
						//        {
						//            handled = true;
						//            return IntPtr.Zero;
						//        }
						//        break;
						//}
						//break;
						return this.WmSysCommand(hwnd, wParam, lParam, ref handled);
					}
				#endregion //WM_SYSCOMMAND

				// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
				#region WM_(ENTER|EXIT)SIZEMOVE
				case NativeWindowMethods.WM_ENTERSIZEMOVE:
					return this.WmEnterSizeMove(hwnd);

				case NativeWindowMethods.WM_EXITSIZEMOVE:
					return this.WmExitSizeMove(hwnd, ref handled  );

				#endregion //WM_(ENTER|EXIT)SIZEMOVE

				// AS 6/24/11 FloatingWindowCaptionSource
				#region WM_GETMINMAXINFO
				case NativeWindowMethods.WM_GETMINMAXINFO:
					return this.WmGetMinMaxInfo(hwnd, lParam);
				#endregion //WM_GETMINMAXINFO
			}

			return IntPtr.Zero;
		}
		#endregion //WndProc

		#endregion //Private Methods

		#endregion //Methods

		#region IToolWindowHost Members

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void IToolWindowHost.DragMove()
		void IToolWindowHost.DragMove(MouseEventArgs e)
		{
			// AS 1/27/11 NA 2011 Vol 1 - Min/Max/Taskbar
			// The Window class won't do anything if the window state is not normal.
			//
			//this.DragMove();
			if (this.WindowState != WindowState.Normal)
			{
				this.VerifyAccess();

				if (Mouse.LeftButton != MouseButtonState.Pressed)
					throw new InvalidOperationException();

				WindowInteropHelper wih = new WindowInteropHelper(this);

				if (wih.Handle != IntPtr.Zero)
				{
					NativeWindowMethods.SendMessageApi(wih.Handle, NativeWindowMethods.WM_SYSCOMMAND, new IntPtr(NativeWindowMethods.SC_DRAGMOVE), IntPtr.Zero); // put the window in move mode
					NativeWindowMethods.SendMessageApi(wih.Handle, NativeWindowMethods.WM_LBUTTONUP, IntPtr.Zero, IntPtr.Zero);
				}
			}
			else
			{
				this.DragMove();
			}
		}

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void IToolWindowHost.DragResize(ToolWindowResizeElementLocation location, Cursor cursor)
		void IToolWindowHost.DragResize(ToolWindowResizeElementLocation location, Cursor cursor, MouseEventArgs e)
		{
			Debug.Fail("This should have been handled in the WM_NCHITTEST handling!");
		}

		void IToolWindowHost.Activate()
		{
			this.Activate();
		}

		void IToolWindowHost.Close()
		{
			this.CloseHelper();
		}

		void IToolWindowHost.RelativePositionStateChanged()
		{
			this.VerifyRelativePositionBinding();
			this.UpdateRelativePosition();
		}

		// AS 5/14/08 BR32842
		void IToolWindowHost.BringToFront()
		{
			this.BringToFront();
		}

        // AS 10/13/08 TFS6107/BR34010
        bool IToolWindowHost.HandlesDelayedMinMaxRequests
        {
            get { return true; }
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        bool IToolWindowHost.IsWindow
        {
            get { return true; }
        }

		// AS 9/11/09 TFS21330
		// Used to indicate if the host currently allows transparency.
		bool IToolWindowHost.AllowsTransparency 
		{
			get { return this.AllowsTransparency; }
		}

		// used to indicate if the host can use the specified transparency
		bool IToolWindowHost.SupportsAllowTransparency(bool allowsTransparency)
		{
			Debug.Assert(_content != null);

			if (this._content == null)
				return false;

			if (_content.UseOSNonClientArea)
				return allowsTransparency == false;

			return true;
		}

		// AS 8/3/11 TFS83465/TFS83469
		void IToolWindowHost.EnsureOnScreen(bool fullyInView)
		{
			this.VerifyIsInView(true, true, fullyInView);
		}

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		Rect IToolWindowHost.GetRestoreBounds()
		{
			Rect restoreBounds = this.RestoreBounds;

			// if the window isn't created yet then get the cached restore bounds from the toolwindow
			if (restoreBounds.IsEmpty && _content != null)
			{
				restoreBounds = _content.GetRestoreBounds(false);
			}

			return restoreBounds;
		}

		void IToolWindowHost.SetRestoreBounds(Rect restoreBounds)
		{
			Debug.Assert(!restoreBounds.IsEmpty, "Why are we getting an empty restore bounds?");

			if (!restoreBounds.IsEmpty)
			{
				WindowInteropHelper wih = new WindowInteropHelper(this);
				IntPtr hwnd = wih.Handle;

				if (hwnd != IntPtr.Zero)
				{
					this.SetRestoreBounds(hwnd, restoreBounds);
				}
				else if (_content != null)
				{
					// let the content cache the value until we get the handle created
					_content.SetRestoreBounds(restoreBounds, false);
				}
			}
		}

		void IToolWindowHost.SetWindowState(WindowState state)
		{
			// AS 7/8/11 TFS75307/TFS81084
			// Keep a flag while changing the window state since we don't want to 
			// update the size of the toolwindow with the maximized size while 
			// the window is being restored.
			// 
			bool wasChanging = this.GetFlag(InternalFlags.IsChangingWindowState);
			this.SetFlag(InternalFlags.IsChangingWindowState, true);

			try
			{
				this.WindowState = state;
			}
			finally
			{
				this.SetFlag(InternalFlags.IsChangingWindowState, wasChanging);
			}

			// AS 9/2/11 TFS85394
			this.OnAfterWindowStateChanged(state);
		}

		// AS 6/8/11 TFS76337
		Rect IToolWindowHost.GetWindowBounds()
		{
			WindowInteropHelper wih = new WindowInteropHelper(this);

			if (wih.Handle == IntPtr.Zero)
				return Rect.Empty;

			var wi = NativeWindowMethods.GetWindowInfo(wih.Handle);

			var topLeft = this.ConvertToLogicalPixels(wi.rcWindow.left, wi.rcWindow.top);
			var bottomRight = this.ConvertToLogicalPixels(wi.rcWindow.right, wi.rcWindow.bottom);
			return Utilities.RectFromPoints(topLeft, bottomRight);
		}

		// AS 11/17/11 TFS91061
		bool IToolWindowHost.IsUsingOsNonClientArea
		{
			get { return this.UseSystemNonClientArea; }
		}
		#endregion //IToolWindowHost Members

		#region WindowPositionToRectConverter
		private class WindowPositionToRectConverter : IMultiValueConverter
		{
			#region Member Variables

			private static readonly IMultiValueConverter instance;

			#endregion //Member Variables

			#region Constructor
			private WindowPositionToRectConverter()
			{
			}

			static WindowPositionToRectConverter()
			{
				instance = new WindowPositionToRectConverter();
			} 
			#endregion //Constructor

			#region Properties

			public static IMultiValueConverter Instance { get { return instance; } }

			#endregion //Properties

			#region IMultiValueConverter Members

			object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
			{
				Debug.Assert(values != null && values.Length == 4);
				Debug.Assert(values[0] is double);
				Debug.Assert(values[1] is double);
				Debug.Assert(values[2] is double);
				Debug.Assert(values[3] is double);

				double left = (double)values[0];
				double top = (double)values[1];
				double width = (double)values[2];
				double height = (double)values[3];

				if (double.IsNaN(left) ||
					double.IsNaN(top) ||
					double.IsNaN(width) ||
					double.IsNaN(height))
					return Rect.Empty;

				return new Rect(left, top, width, height);
			}

			object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
			{
				return new object[] { Binding.DoNothing };
			}

			#endregion
		} 
		#endregion //WindowPositionToRectConverter

		// AS 4/28/11 TFS73532
		#region DeferShowHelper class
		internal class DeferShowHelper : ToolWindow.IDeferShow
		{
			private ToolWindowHostWindow _window;

			internal DeferShowHelper(ToolWindowHostWindow window)
			{
				_window = window;
			}

			public void Show(bool activate)
			{
				// AS 6/23/11 TFS73499
				// If the window is shown while its visibility is collapsed/hidden then it 
				// won't show properly when the state is actually toggled to visible so 
				// force it to visible while we are showing it and then let it go back to 
				// what it was.
				//
				using (new TempValueReplacement(_window, UIElement.VisibilityProperty, KnownBoxes.VisibilityVisibleBox))
					_window.Show(activate);
			}

			public void Cancel()
			{
				Debug.Assert(_window._content != null);
				Debug.Assert(!_window.IsInWndProc, "Cancelling after shown?");

				_window.Close();
			}
		}
		#endregion //DeferShowHelper class

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		#region CoerceProxy class
		private class CoerceProxy
		{
			private CoerceValueCallback _baseCallback;

			internal CoerceProxy(CoerceValueCallback baseCallback)
			{
				_baseCallback = baseCallback;
			}

			public virtual object CoerceValue(DependencyObject d, object baseValue)
			{
				if (null != _baseCallback)
					return _baseCallback(d, baseValue);

				return baseValue;
			}
		}
		#endregion //CoerceProxy class

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// See the static ctor for details.
		//
		#region TopLeftCoerceProxy class
		private class TopLeftCoerceProxy : CoerceProxy
		{
			private DependencyProperty _property;

			internal TopLeftCoerceProxy(DependencyProperty property, CoerceValueCallback baseCallback)
				: base(baseCallback)
			{
				_property = property;
			}

			public override object CoerceValue(DependencyObject d, object baseValue)
			{
				if (!Utilities.Antirecursion.Enter(d, _property, true))
					return baseValue;

				try
				{
					ToolWindowHostWindow w = d as ToolWindowHostWindow;
					if (w != null && w.WindowState != WindowState.Normal)
					{
						// if the window is in the process of transitioning do not 
						// call the base coersion or else it will update the 
						// restore bounds which could position the window at a 
						// different location or simply mess with the window animation
						if (NativeWindowMethods.GetWindowState(w) == WindowState.Normal)
							return baseValue;

						WindowInteropHelper wih = new WindowInteropHelper(w);
						IntPtr hwnd = wih.Handle;

						if (hwnd != IntPtr.Zero)
						{
							NativeWindowMethods.WINDOWPLACEMENT placement = NativeWindowMethods.GetWindowPlacement(hwnd);
							object value = base.CoerceValue(d, baseValue);

							if (!placement.Equals(NativeWindowMethods.GetWindowPlacement(hwnd)))
								NativeWindowMethods.SetWindowPlacement(hwnd, ref placement);
							return value;
						}
					}

					return base.CoerceValue(d, baseValue);
				}
				finally
				{
					Utilities.Antirecursion.Exit(d, _property);
				}
			}
		}
		#endregion //TopLeftCoerceProxy class

		// AS 1/31/11 NA 2011 Vol 1 - Min/Max/Taskbar
		// Switched from bit vectors to enum since its more readable and doesn't need 
		// a property for each flag.
		//
		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : int
		{
			UpdatingRelativePosition = 1 << 0,
			IsSendingWmSize = 1 << 1,
			IsSynchronizingLocation = 1 << 2,
			UseSystemNonClientArea = 1 << 3,
			IsInOnContentChanged = 1 << 4,
			IsForcingSizeToContent = 1 << 5,
			TrackWmMoving = 1 << 6,
			IsMovingWindow = 1 << 7,
			IsInMoveSize = 1 << 8,
			IsChangingWindowState = 1 << 9, // AS 7/8/11 TFS75307/TFS81084
		}
		#endregion // InternalFlags enum

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