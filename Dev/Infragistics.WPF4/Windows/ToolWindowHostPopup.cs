using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Interop;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Threading;
using Infragistics.Windows.Helpers;
using System.Runtime.InteropServices;
using System.Windows.Media;
using System.Security;
using System.Security.Permissions;
using System.Windows.Navigation;
using System.Runtime.CompilerServices;
using Infragistics.Collections;

namespace Infragistics.Windows.Controls
{
    internal class ToolWindowHostPopup : ToolWindowContainer
		, IToolWindowHost
    {
        #region Member Variables

        private Popup _popup;
        private ToolWindow.ShowDialogCallback _callback;
        // AS 3/30/09 TFS16355 - WinForms Interop
        // The root visual doesn't have to be a page.
        //
        //private Page _containingPage;
        private FrameworkElement _rootVisual;
        private Rect _screenRect;
        private WeakReference _previousFocusedElement;
        private FrameworkElement _elementToDisable;

        // AS 3/30/09 TFS16355 - WinForms Interop
        private PopupController _controller;

        private static readonly object InitialRelativePositionId = new object();
        private static readonly object RelativePositionStateChangedId = new object();
        private static readonly object LayoutUpdatedId = new object();
        private static readonly object SynchronousRelativePositionId = new object();

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		[ThreadStatic]
		private static WeakList<ToolWindowHostPopup> _currentPopups;
		private GroupTempValueReplacement _modalDisabledStates;
		private bool _isModal;

        #endregion //Member Variables

        #region Constructor
        static ToolWindowHostPopup()
        {
            // we don't want this to be a focus scope because this is 
            // a modal window anyway
            FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(ToolWindowHostPopup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
        }

        private ToolWindowHostPopup()
        {
            this.Loaded += new RoutedEventHandler(OnLoaded);
        } 
        #endregion //Constructor

        #region Base class overrides

		// AS 9/11/09 TFS21330
		#region AllowsTransparency
		public override bool AllowsTransparency
		{
			get
			{
				Debug.Assert(_popup != null);

				return _popup != null && _popup.AllowsTransparency;
			}
		} 
		#endregion //AllowsTransparency

        #region BringToFront
        public override void BringToFront()
        {
            // there is no support in the framework for changing
            // the zorder of the popup. since we are only using
            // this modally then we should be ok

            // AS 3/30/09 TFS16355 - WinForms Interop
            // When we have full trust then we can use unmanaged code to bring
            // the popup to the front so we can request that it be brought
            // into view at least in the modeless case.
            //
            ToolWindow tw = this.Child as ToolWindow;

            if (null != tw && !tw.IsModal)
                this.BringIntoView();
        }
        #endregion //BringToFront

        #region Close
        public override void Close()
        {
            ToolWindow window = this.Child as ToolWindow;
            Debug.Assert(null != window);
            Debug.Assert(this._popup.IsOpen);

            if (this._popup.IsOpen)
            {
                if (null != window && window.OnClosingInternal(new CancelEventArgs()))
                    return;

                this._popup.IsOpen = false;
            }
        }
        #endregion //Close

		// AS 8/4/11 TFS83465/TFS83469
		#region EnsureOnScreen
		public override void EnsureOnScreen(bool fullyInView)
		{
			this.VerifyIsInView(true, true, fullyInView);
		} 
		#endregion //EnsureOnScreen

        #region GetRelativeScreenPoint
        protected override Point GetRelativeScreenPoint(MouseEventArgs e)
        {
            Point pt = e.GetPosition(this);
            pt.X += this.Left;
            pt.Y += this.Top;

            // AS 3/30/09 TFS16355 - WinForms Interop
            // Only constrain if we could not create our controller.
            //
			// AS 6/8/09 TFS18150
			// Since the Popup class constraints its size explicitly during the 
			// Measure, we need to also constrain during a resize.
			//
			//if (this.ConstrainToWorkArea)
            if (this.ConstrainToWorkArea || this.IsResizing)
            {
                // constrain the mouse to our restricted screen. this is needed
                // since the popup will not allow any portion of the element
                // to be "offscreen".
                //
                if (pt.X < this._screenRect.X)
                    pt.X = this._screenRect.X;
                else if (pt.X > this._screenRect.Right)
                    pt.X = this._screenRect.Right;

                if (pt.Y < this._screenRect.Y)
                    pt.Y = this._screenRect.Y;
                else if (pt.Y > this._screenRect.Bottom)
                    pt.Y = this._screenRect.Bottom;
            }

            return pt;
        }
        #endregion //GetRelativeScreenPoint

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region OnChildChanged
        protected override void OnChildChanged(UIElement oldChild, UIElement newChild)
        {
            ToolWindow oldWindow = oldChild as ToolWindow;
            ToolWindow newWindow = newChild as ToolWindow;

            if (null != oldWindow)
            {
                FrameworkElement oldOwner = oldWindow.Owner;
                Debug.Assert(null != oldOwner);

                if (null != oldOwner)
                {
                    IList<ToolWindow> toolWindows = GetToolWindows(oldOwner);

                    if (null != toolWindows)
                        toolWindows.Remove(oldWindow);
                }
            }

            if (null != newWindow)
            {
                FrameworkElement newOwner = newWindow.Owner;
                Debug.Assert(null != newOwner);

                if (null != newOwner)
                {
                    IList<ToolWindow> toolWindows = GetToolWindows(newOwner);

                    if (null == toolWindows)
                    {
                        toolWindows = new List<ToolWindow>();
                        newOwner.SetValue(ToolWindowsPropertyKey, toolWindows);
                    }

                    toolWindows.Add(newWindow);
                }
            }

            base.OnChildChanged(oldChild, newChild);
        } 
        #endregion //OnChildChanged

        #region OnEnteringDrag
        protected override void OnEnteringDrag(MouseEventArgs e, ToolWindowResizeElementLocation location)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            // Only constrain if we could not create our controller.
            //
			// AS 6/8/09 TFS18150
			// Since the Popup constrains its own size to a percentage of the screen
			// we always need to constrain even when we're using our controller.
			//
            //if (this.ConstrainToWorkArea)
			if (true)
            {
                Rect workRect = this.GetWorkArea(e);
                Point offset = e.GetPosition(this);

				// AS 6/8/09 TFS18150
				PopupChildWrapper wrapper = _popup.Child as PopupChildWrapper;

				if (null != wrapper)
				{
					Size maxSize = wrapper.CalculateMaxPopupSize(workRect);

					if (!maxSize.IsEmpty)
					{
						Rect currentRect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
						Rect maxRect = new Rect(this.Left, this.Top, Math.Min(workRect.Width, maxSize.Width), Math.Min(workRect.Height, maxSize.Height));

						switch (location)
						{
							case ToolWindowResizeElementLocation.Top:
							case ToolWindowResizeElementLocation.TopLeft:
							case ToolWindowResizeElementLocation.TopRight:
								maxRect.Y = currentRect.Bottom - maxRect.Height;
								break;
						}

						switch (location)
						{
							case ToolWindowResizeElementLocation.Left:
							case ToolWindowResizeElementLocation.TopLeft:
							case ToolWindowResizeElementLocation.BottomLeft:
								maxRect.X = currentRect.Right - maxRect.Width;
								break;
						}

						workRect.Intersect(maxRect);
					}
				}

                // we need to constrain the resize operation so as to not
                // include the difference between the position of the 
                // mouse within the element and the edge of the element
                // that will be resized (e.g. the distance between the 
                // right edge and the relative mouse position).
                //
                switch (location)
                {
                    case ToolWindowResizeElementLocation.Bottom:
                    case ToolWindowResizeElementLocation.BottomLeft:
                    case ToolWindowResizeElementLocation.BottomRight:
                        workRect.Height -= this.ActualHeight - offset.Y;
                        break;
                    case ToolWindowResizeElementLocation.Top:
                    case ToolWindowResizeElementLocation.TopLeft:
                    case ToolWindowResizeElementLocation.TopRight:
                        workRect.Y += offset.Y;
                        workRect.Height -= offset.Y;
                        break;
                }

                switch (location)
                {
                    case ToolWindowResizeElementLocation.BottomLeft:
                    case ToolWindowResizeElementLocation.Left:
                    case ToolWindowResizeElementLocation.TopLeft:
                        workRect.X += offset.X;
                        workRect.Width -= offset.X;
                        break;
                    case ToolWindowResizeElementLocation.BottomRight:
                    case ToolWindowResizeElementLocation.Right:
                    case ToolWindowResizeElementLocation.TopRight:
                        workRect.Width -= this.ActualWidth - offset.X;
                        break;
                }

                this._screenRect = workRect;
            }
            
            base.OnEnteringDrag(e, location);
        }
        #endregion //OnEnteringDrag

        #region OnEnteringMove
        protected override void OnEnteringMove(MouseEventArgs e)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            // Only constrain if we could not create our controller.
            //
            if (this.ConstrainToWorkArea)
            {
                Rect workRect = this.GetWorkArea(e);
                Point offset = e.GetPosition(this);

				// AS 1/5/12 TFS51985
				// Consider the portion of the popup that is currently "offscreen".
				//
				Rect thisRect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
				workRect = Rect.Union(thisRect, workRect);

				// Added Top/Left into the X/Y and Width/Height since the virtual screen is 
				// larger accounting for the portion of the window that is "offscreen" already.
				//
				double extraTop = Math.Min(this.Top, 0);
				double extraLeft = Math.Min(this.Left, 0);

                // we don't want to try and move any portion of the window out
                // of view since the popup doesn't support that so we'll constrain
                // the screen rect based on where the mouse is within this 
                // element.
                //
                workRect.Y = offset.Y + extraTop;
				workRect.X = offset.X + extraLeft;
				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				// This isn't specific but I found this while testing. Make sure not to 
				// try to set the Width/Height to a value < 0.
				//
				//workRect.Width -= this.ActualWidth;
                //workRect.Height -= this.ActualHeight;
				workRect.Width = Math.Max(workRect.Width - this.ActualWidth, 0);
                workRect.Height = Math.Max(workRect.Height - this.ActualHeight, 0);
                this._screenRect = workRect;
            }

            base.OnEnteringMove(e);
        }
        #endregion //OnEnteringMove

        #region RelativePositionStateChanged
        public override void RelativePositionStateChanged()
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            this.RelativePositionStateChanged(RelativePositionStateChangedId);
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        // When we are hosted within a popup and have our controller
        // then we can update the position before waiting for update layout.
        // But when this is called from that callback we do not want to 
        // do another synchronous verify of the position so I added an overload
        // to allow us to know to skip that verification.
        //
        private object RelativePositionStateChanged(object param)
        {
            ToolWindow tw = this.Child as ToolWindow;
            bool hookLayoutUpdated = false;

            // AS 3/30/09 TFS16355 - WinForms Interop [Start]
            // We don't want to verify the position if the popup is closed 
            // unless this is the initial verification.
            //
            //if (null != tw)
            bool verifyPosition = null != tw && _popup != null && _popup.IsOpen;

			// AS 11/2/10 TFS49402/TFS49912/TFS51985
			// Not specific but just in case we shouldn't get into the verify if tw is null.
			//
			//if (!verifyPosition && param == InitialRelativePositionId)
            if (!verifyPosition && null != tw && param == InitialRelativePositionId)
                verifyPosition = true;

            if (verifyPosition)
            {
                try
                {
                    // owner rect
                    // AS 3/30/09 TFS16355 - WinForms Interop [Start]
                    //Point ownerPt = Utilities.PointToScreenSafe(tw.Owner, new Point());
					FrameworkElement owner = tw.Owner;
					Window twWindow = owner as Window;

                    Point ownerPt;

                    if (null != twWindow)
                    {
                        ownerPt = new Point(twWindow.Left, twWindow.Top);
                    }
                    else
                    {
                        ownerPt = Utilities.PointToScreenSafe(owner, new Point());

						
#region Infragistics Source Cleanup (Region)
























#endregion // Infragistics Source Cleanup (Region)

					}
                    // AS 3/30/09 TFS16355 - WinForms Interop [End]

                    Rect ownerRect = new Rect(ownerPt, owner.RenderSize);

					// AS 7/14/09 TFS18424
					// Similar to the change in ToolWindowHostWindow, we need to augment the horizontal
					// original to account for the fact that the rect is flipped.
					//
					if (owner.FlowDirection == FlowDirection.RightToLeft)
						ownerRect.X -= owner.ActualWidth;

					// popup rect
                    Rect thisRect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);

					// AS 11/2/10 TFS49402/TFS49912/TFS51985
					if (this.IsLoaded)
						this.PerformInitialPositioning(ref thisRect);

                    // let the tool window adjust the positioning
                    tw.AdjustRelativeRect(ownerRect, ref thisRect);

                    if (tw.VerticalAlignmentMode != ToolWindowAlignmentMode.Manual)
                    {
                        // calculate width and offset
                        hookLayoutUpdated = true;

                        this.Top = thisRect.Y;

                        if (thisRect.Height != this.ActualHeight)
                            this.Height = thisRect.Height;
                    }

                    if (tw.HorizontalAlignmentMode != ToolWindowAlignmentMode.Manual)
                    {
                        hookLayoutUpdated = true;

                        this.Left = thisRect.X;

                        if (thisRect.Width != this.ActualWidth)
                            this.Width = thisRect.Width;
                    }
                }
                catch (InvalidOperationException)
                {
                    // AS 3/30/09 TFS16355 - WinForms Interop
                    // Calling PointToScreen could fail if the owner
                    // is disconnected from the presentationsource.
                    //
                    hookLayoutUpdated = false;
                }
            }

            // AS 3/30/09 TFS16355 - WinForms Interop
            // Unhook first in case we're already hooked in.
            //
            //if (hookLayoutUpdated && this.IsLoaded)
            this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

            if (hookLayoutUpdated && this.IsLoaded)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                if (param != SynchronousRelativePositionId && this._controller != null)
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new DispatcherOperationCallback(this.RelativePositionStateChanged), SynchronousRelativePositionId);

                this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
            }
            else
                this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

            return null;
        }

        #endregion //RelativePositionStateChanged

		// AS 9/11/09 TFS21330
		#region SupportsAllowTransparency
		public override bool SupportsAllowTransparency(bool allowsTransparency)
		{
			return allowsTransparency == PopupHelper.PopupAllowsTransparency;
		} 
		#endregion //SupportsAllowTransparency

        #endregion //Base class overrides

        #region Properties

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region ConstrainToWorkArea
        private bool ConstrainToWorkArea
        {
			// AS 11/2/10 TFS49402/TFS49912/TFS51985
			// When shown modally and the popup will be constrained to the client area of the 
			// browser, we want to keep the window in the work area.
			//
			//get { return this._controller == null; }
            get { return this._controller == null || (PopupHelper.IsPopupInChildWindow && _isModal); }
        } 
        #endregion //ConstrainToWorkArea

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region CurrentPopups
		private static WeakList<ToolWindowHostPopup> CurrentPopups
		{
			get
			{
				if ( _currentPopups == null )
					_currentPopups = new WeakList<ToolWindowHostPopup>();

				return _currentPopups;
			}
		}
		#endregion // CurrentPopups

		// AS 3/30/09 TFS16355 - WinForms Interop
        #region IsOpen

        private static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen",
            typeof(bool), typeof(ToolWindowHostPopup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsOpenChanged)));

        private static void OnIsOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (false.Equals(e.NewValue))
            {
                ToolWindowHostPopup owner = (ToolWindowHostPopup)d;

                ToolWindow tw = owner.Child as ToolWindow;

                // when shown modally we were waiting for the popup's
                // closed event to clean up but that happens after a delay
                // and when shown modelessly we really don't want that delay
                // so in that case we will clean up when IsOpen goes to false
                if (null != tw && tw.IsModal == false)
                {
                    owner.Child = null;

                    if (null != tw)
                        tw.OnClosedInternal();
                }
            }
        }

        private bool IsOpen
        {
            get
            {
                return (bool)this.GetValue(ToolWindowHostPopup.IsOpenProperty);
            }
            set
            {
                this.SetValue(ToolWindowHostPopup.IsOpenProperty, value);
            }
        }

        #endregion //IsOpen

        #region ModalToolWindowProperty

        private static readonly DependencyPropertyKey ModalToolWindowsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ModalToolWindows", typeof(Stack<ToolWindowHostPopup>), typeof(ToolWindowHostPopup),
            new FrameworkPropertyMetadata());

        private static readonly DependencyProperty ModalToolWindowsProperty = ModalToolWindowsPropertyKey.DependencyProperty;

        private static Stack<ToolWindowHostPopup> GetModalToolWindows(DependencyObject d)
        {
            return (Stack<ToolWindowHostPopup>)d.GetValue(ModalToolWindowsProperty);
        }
 
        #endregion //ModalToolWindowProperty

        // AS 3/30/09 TFS16355 - WinForms Interop
        // Added a collection we can maintain on the owner to know all the open models popups.
        //
        #region ToolWindowProperty

        private static readonly DependencyPropertyKey ToolWindowsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "ToolWindows", typeof(IList<ToolWindow>), typeof(ToolWindowHostPopup),
            new FrameworkPropertyMetadata());

        private static readonly DependencyProperty ToolWindowsProperty = ToolWindowsPropertyKey.DependencyProperty;

        private static IList<ToolWindow> GetToolWindows(DependencyObject d)
        {
            return (IList<ToolWindow>)d.GetValue(ToolWindowsProperty);
        }

        #endregion //ToolWindowProperty

        #endregion //Properties

        #region Methods

        #region AddModalWindows
        
#region Infragistics Source Cleanup (Region)





























#endregion // Infragistics Source Cleanup (Region)

        #endregion //AddModalWindows

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region AddToolWindows
        internal static void AddToolWindows(FrameworkElement owner, List<ToolWindow> windows)
        {
            IList<ToolWindow> popupWindows = GetToolWindows(owner);

            if (null != popupWindows)
            {
                for (int i = popupWindows.Count - 1; i >= 0; i--)
                {
                    windows.Add(popupWindows[i]);
                }
            }
        }
        #endregion //AddToolWindows

        #region GetCurrentDialog
        // AS 3/30/09 TFS16355 - WinForms Interop
        //private static ToolWindowHostPopup GetCurrentDialog(Page page)
        private static ToolWindowHostPopup GetCurrentDialog(FrameworkElement page)
        {
            if (null != page)
            {
                Stack<ToolWindowHostPopup> hosts = GetModalToolWindows(page);

                if (null != hosts && hosts.Count > 0)
                    return hosts.Peek();
            }

            return null;
        } 
        #endregion //GetCurrentDialog

		// AS 6/8/09 TFS18150
		#region GetHost
		internal static ToolWindowHostPopup GetHost(Popup popup)
		{
			if (null == popup)
				return null;

			PopupChildWrapper wrapper = popup.Child as PopupChildWrapper;

			if (null != wrapper)
				return wrapper.Host;

			return popup.Child as ToolWindowHostPopup;
		}
		#endregion //GetHost

		#region GetPage
        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #endregion //GetPage

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetRootVisual
        private static FrameworkElement GetRootVisual(FrameworkElement owner)
        {
            DependencyObject parent = owner;

            while (parent != null)
            {
                if (parent is Page)
                    break;

                DependencyObject tempParent = LogicalTreeHelper.GetParent(parent);

                if (tempParent == null && (parent is Visual || parent is System.Windows.Media.Media3D.Visual3D))
                    tempParent = VisualTreeHelper.GetParent(parent);

				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				// Found an issue when attempting to show a modal popup while a modal popup was shown.
				// Basically this routine would not get up to the page.
				//
				if ( tempParent == null )
				{
					if ( parent is Popup )
					{
						ToolWindowHostPopup host = ToolWindowHostPopup.GetHost(parent as Popup);

						if ( null != host && host.Child is ToolWindow )
							tempParent = ((ToolWindow)host.Child).Owner;
					}
				}

                if (tempParent == null)
                    break;

                parent = tempParent;
            }

			// AS 1/5/12 TFS51985
			// Use the child element of the window since the window's size includes the 
			// non-client area that isn't actually occupied by any WPF elements.
			//
			Window w = parent as Window;

			if (w != null)
				parent = VisualTreeHelper.GetChild(parent, 0);

            return parent as FrameworkElement;
        } 
        #endregion //GetRootVisual 

        #region GetWorkArea
        private Rect GetWorkArea(MouseEventArgs e)
        {
			// AS 6/8/09 TFS18150
			if (!this.ConstrainToWorkArea)
			{
				return NativeWindowMethods.VirtualScreenArea;
			}

            // AS 3/30/09 TFS16355 - WinForms Interop
            //return new Rect(new Point(), this._containingPage.RenderSize);
            return new Rect(new Point(), this._rootVisual.RenderSize);
        } 
        #endregion //GetWorkArea

        #region OnLayoutUpdated
        void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.RelativePositionStateChanged(LayoutUpdatedId);
        } 
        #endregion //OnLayoutUpdated

        #region On(Loaded|Unloaded)
        void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= new RoutedEventHandler(OnUnloaded);
            this.Loaded += new RoutedEventHandler(OnLoaded);

            // make sure to unhook this event
            this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(OnLoaded);
            this.Unloaded += new RoutedEventHandler(OnUnloaded);

            this.RelativePositionStateChanged();
        }

        #endregion //On(Loaded|Unloaded)

        #region OnPopupClosed
        private void OnPopupClosed(object sender, EventArgs e)
        {
            bool restoreFocus = this.IsKeyboardFocusWithin;

            ToolWindow window = this.Child as ToolWindow;
            this.Child = null;

            if (null != window)
            {
				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				//if (null != this._elementToDisable)
                //    this._elementToDisable.IsEnabled = true;
				if ( null != _modalDisabledStates )
					_modalDisabledStates.Dispose();

				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				// Remove the item from the current list of popups.
				//
				CurrentPopups.Remove(this);

                PopModalDialog(this);

                if (restoreFocus)
                {
                    IInputElement oldFocus = Utilities.GetWeakReferenceTargetSafe(this._previousFocusedElement) as IInputElement;

                    if (null != oldFocus)
                        oldFocus.Focus();
                }

                bool dialogResult = window.DialogResult ?? false;
                window.OnClosedInternal();

                if (null != this._callback)
                    this._callback(window, dialogResult);
            }
        }
        #endregion //OnPopupClosed

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region OnPopupClosedModeless
		private void OnPopupClosedModeless( object sender, EventArgs e )
		{
			// AS 11/2/10 TFS49402/TFS49912/TFS51985
			// Remove the item from the current list of popups.
			//
			CurrentPopups.Remove(this);
		} 
		#endregion // OnPopupClosedModeless

        #region PopModalDialog
        private static void PopModalDialog(ToolWindowHostPopup popup)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            //Stack<ToolWindowHostPopup> hosts = GetModalToolWindows(popup._containingPage);
            Stack<ToolWindowHostPopup> hosts = GetModalToolWindows(popup._rootVisual);

            Debug.Assert(null != hosts && hosts.Contains(popup));
            if (null != hosts && hosts.Contains(popup))
            {
                while (hosts.Pop() != popup)
                {
                }
            }
        } 
        #endregion //PopModalDialog

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region PrepareModalDisabledState
		private void PrepareModalDisabledState()
		{
			_modalDisabledStates = new GroupTempValueReplacement();

			List<ToolWindowHostPopup> popups = new List<ToolWindowHostPopup>();

			// the most recent popups will be at the end so walk until we 
			// hit another modal popup
			for ( int i = CurrentPopups.Count - 1; i >= 0; i-- )
			{
				ToolWindowHostPopup currentPopup = _currentPopups[i];

				Debug.Assert(currentPopup != null, "A popup was collected. Was it possibly modal?");

				if ( null == currentPopup )
					continue;

				// we'll handle the element to disable below
				if (currentPopup != _elementToDisable)
					_modalDisabledStates.Add(new TempValueReplacement(currentPopup, UIElement.IsEnabledProperty, KnownBoxes.FalseBox));

				if ( currentPopup._isModal )
					break;
			}

			if ( null != _elementToDisable )
			{
				_modalDisabledStates.Add(new TempValueReplacement(_elementToDisable, UIElement.IsEnabledProperty, KnownBoxes.FalseBox));
			}
		}
		#endregion // PrepareModalDisabledState 

        #region PushModalDialog
        private static void PushModalDialog(ToolWindowHostPopup popup)
        {
            // AS 3/30/09 TFS16355 - WinForms Interop
            //if (null != popup._containingPage)
            if (null != popup._rootVisual)
            {
                Stack<ToolWindowHostPopup> hosts = GetModalToolWindows(popup._rootVisual);

                if (null == hosts)
                {
                    hosts = new Stack<ToolWindowHostPopup>();
                    popup._rootVisual.SetValue(ModalToolWindowsPropertyKey, hosts);
                }

                hosts.Push(popup);
            }
        } 
        #endregion //PushModalDialog

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region Show
		// AS 4/28/11 TFS73532
		// Added return value and removed activate param
		//
        //internal static void Show(ToolWindow toolWindow, bool activate)
        internal static ToolWindow.IDeferShow Show(ToolWindow toolWindow)
        {
            return ShowImpl(toolWindow, false, null);
        } 
        #endregion //Show

        #region ShowDialog
        internal static void ShowDialog(ToolWindow toolWindow, ToolWindow.ShowDialogCallback callback)
        {
            
#region Infragistics Source Cleanup (Region)




















































#endregion // Infragistics Source Cleanup (Region)

			// AS 4/28/11 TFS73532
			//ShowImpl(toolWindow, true, true, callback);
			var deferShow = ShowImpl(toolWindow, true, callback);

			Debug.Assert(null != deferShow, "Expected show helper");

			if (null != deferShow)
				deferShow.Show(true);
        }
        #endregion //ShowDialog

        // AS 3/30/09 TFS16355 - WinForms Interop
        // Refactored from the ShowDialog implementation.
        //
        #region ShowImpl
		// AS 4/28/11 TFS73532
		// Added return value and removed the activate param.
		//
		//private static void ShowImpl(ToolWindow toolWindow, bool activate, bool isModal, ToolWindow.ShowDialogCallback callback)
		private static ToolWindow.IDeferShow ShowImpl(ToolWindow toolWindow, bool isModal, ToolWindow.ShowDialogCallback callback)
        {
            Popup popup = new Popup();

            ToolWindowHostPopup popupHost = new ToolWindowHostPopup();

            if (isModal)
                popupHost._callback = callback;

			// AS 8/24/09 TFS19861
			//popup.AllowsTransparency = true;
            popup.AllowsTransparency = toolWindow.AllowsTransparencyResolved;
            popupHost._popup = popup;
			// AS 4/28/11 TFS73532
			// Moved down so this is more of a top-down built approach.
			//
			//popupHost.Child = toolWindow;

            if (isModal)
                popupHost._previousFocusedElement = new WeakReference(Keyboard.FocusedElement);

            // AS 3/30/09 TFS16355 - WinForms Interop
            //Page rootPage = GetPage(toolWindow.Owner);
            FrameworkElement rootPage = GetRootVisual(toolWindow.Owner);

            Debug.Assert(null != rootPage);
            // AS 3/30/09 TFS16355 - WinForms Interop
            // The root visual doesn't have to be a page.
            //
            //popupHost._containingPage = rootPage;
            popupHost._rootVisual = rootPage;

            if (isModal)
                popupHost._elementToDisable = GetCurrentDialog(rootPage) ?? (FrameworkElement)rootPage;

			// AS 6/8/09 TFS18150
			//popup.Child = popupHost;
			// AS 7/14/09 TFS18424
			//popup.Child = new PopupChildWrapper(popupHost);
			PopupChildWrapper child = new PopupChildWrapper(popupHost);
			popup.Child = child;
            popup.StaysOpen = true;

			// AS 4/28/11 TFS73532
			// Moved from below so the toolwindow isn't added to the logical tree until
			// the popup contains the PopupChildWrapper and that contains the ToolWindowHostPopup.
			//
			popupHost.Child = toolWindow;

			// AS 7/14/09 TFS18424
			child.SetBinding(FrameworkElement.FlowDirectionProperty, Utilities.CreateBindingObject(FrameworkElement.FlowDirectionProperty, System.Windows.Data.BindingMode.OneWay, toolWindow.Owner));

			// AS 6/8/09 TFS18150
			popup.PopupAnimation = PopupAnimation.None;

            // AS 3/30/09 TFS16355 - WinForms Interop
            // When not modal we want to make sure that the uiparentcore of the 
            // popup is not above the logical parent of the toolwindow.
            //
            
            if (isModal)
                popup.PlacementTarget = rootPage ?? toolWindow.Owner;
            else
                popup.PlacementTarget = toolWindow.Owner;

            popup.Placement = PlacementMode.Absolute;

            popup.SetBinding(Popup.HorizontalOffsetProperty, Utilities.CreateBindingObject(ToolWindow.LeftProperty, BindingMode.TwoWay, popupHost));
            popup.SetBinding(Popup.VerticalOffsetProperty, Utilities.CreateBindingObject(ToolWindow.TopProperty, BindingMode.TwoWay, popupHost));

            if (isModal)
                popup.Closed += new EventHandler(popupHost.OnPopupClosed);
			else
			{
				// AS 11/2/10 TFS49402/TFS49912/TFS51985
				popup.Closed += popupHost.OnPopupClosedModeless;
			}

            // AS 3/30/09 TFS16355 - WinForms Interop
            // When shown modelessly we should close the toolwindow immediately.
            // 
            popupHost.SetBinding(IsOpenProperty, Utilities.CreateBindingObject(Popup.IsOpenProperty, BindingMode.OneWay, popup));

            if (double.IsNaN(popup.HorizontalOffset))
                popup.HorizontalOffset = 0d;
            if (double.IsNaN(popup.VerticalOffset))
                popup.VerticalOffset = 0d;

            // AS 3/30/09 TFS16355 - WinForms Interop
            popupHost._controller = PopupController.Create(popup, toolWindow.Owner);

            // push it onto out list
            if (isModal)
                PushModalDialog(popupHost);

			// AS 11/2/10 TFS49402/TFS49912/TFS51985
			// Use the tempvaluereplacements so we can restore the states later.
			//
            //if (null != popupHost._elementToDisable)
			//	popupHost._elementToDisable.IsEnabled = false;
			popupHost._isModal = isModal;

			if ( isModal )
				popupHost.PrepareModalDisabledState();

			CurrentPopups.Add(popupHost);

			// make sure the relative positioning is up to date
            popupHost.RelativePositionStateChanged(InitialRelativePositionId);

			// AS 4/28/11 TFS73532
			// Moved impl into a helper class.
			//
			//popup.IsOpen = true;
			//
			//if (activate)
			//{
			//    toolWindow.Activate();
			//
			//    // the wpf framework (in the KeyboardDevice.ReevaluateFocusCallback) 
			//    // thinks that the popup's presentation source is not active and therefore
			//    // they try to take focus away but they do so without asking
			//    // the current focused element so our previewlostkeyboardfocus handler
			//    // is not getting invoked. to work around this, i'm putting in a delayed
			//    // callback (the ReevaluateFocusCallback is done with an Input priority) 
			//    // at which point we will take focus back into the tool window.
			//    //
			//    toolWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Threading.ContextCallback(popupHost.VerifyActivation), null);
			//}
			return new DeferShowHelper(popupHost);
        } 
        #endregion //ShowImpl

        #region VerifyActivation
        private void VerifyActivation(object param)
        {
            ToolWindow tw = this.Child as ToolWindow;

            if (null != tw && tw.IsVisible)
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //tw.Activate();
                if (true.Equals(param))
                    tw.BringToFront();
                else
                    tw.Activate();
            }
        }

        #endregion //VerifyActivation

		// AS 8/4/11 TFS83465/TFS83469
		#region VerifyIsInView
		private void VerifyIsInView(bool force, bool async, bool ensureFullyInView)
		{
			ToolWindow tw = this.Child as ToolWindow;

			if (tw == null || (tw.KeepOnScreen == false && !force))
				return;

			if (tw.WindowState == WindowState.Minimized)
				return;

			if (async)
				this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new System.Threading.SendOrPostCallback(this.VerifyIsInViewImpl), ensureFullyInView);
			else
				this.VerifyIsInViewImpl(ensureFullyInView);
		}

		private void VerifyIsInViewImpl(object param)
		{
			ToolWindow tw = this.Child as ToolWindow;

			if (tw == null)
				return;

			Rect logicalScreenRect = new Rect(this.Left, this.Top, this.ActualWidth, this.ActualHeight);
			Point? location = Utilities.CalculateOnScreenLocation(logicalScreenRect, this, true.Equals(param));

			if (location != null)
			{
				if (tw != null && tw.HorizontalAlignmentMode == ToolWindowAlignmentMode.Manual)
					this.Left = location.Value.X;

				if (tw != null && tw.VerticalAlignmentMode == ToolWindowAlignmentMode.Manual)
					this.Top = location.Value.Y;
			}
		}
		#endregion //VerifyIsInView

		#endregion //Methods

		// AS 6/8/09 TFS18150
		#region PopupChildWrapper class
		private class PopupChildWrapper : FrameworkElement
		{
			#region Member Variables

			private ToolWindowHostPopup _host;
			private bool _isMeasuringMaxHeight;

			#endregion //Member Variables

			#region Constructor
			internal PopupChildWrapper(ToolWindowHostPopup host)
			{
				_host = host;
				// AS 1/5/12 TFS51985
				// While not technically necessary for this bug I noticed this while debugging. Really the 
				// element should be a logical child before it is a visual child.
				//
				this.AddLogicalChild(host);
				this.AddVisualChild(host);
			}
			#endregion //Constructor

			#region Base class overrides
			protected override Size MeasureOverride(Size availableSize)
			{
				if (_isMeasuringMaxHeight)
					return new Size(int.MaxValue, int.MaxValue);

				_host.Measure(availableSize);
				return _host.DesiredSize;
			}

			protected override Size ArrangeOverride(Size finalSize)
			{
				Debug.Assert(!_isMeasuringMaxHeight);

				_host.Arrange(new Rect(finalSize));
				return finalSize;
			}

			protected override Visual GetVisualChild(int index)
			{
				if (index != 0)
					throw new ArgumentOutOfRangeException();

				return _host;
			}

			protected override int VisualChildrenCount
			{
				get
				{

					return 1;
				}
			}

			protected override System.Collections.IEnumerator LogicalChildren
			{
				get
				{
					return new SingleItemEnumerator(_host);
				}
			}
			#endregion //Base class overrides

			#region Properties
			public ToolWindowHostPopup Host
			{
				get { return _host; }
			}
			#endregion //Properties

			#region Methods
			internal Size CalculateMaxPopupSize(Rect workRect)
			{
				bool wasMeasuring = _isMeasuringMaxHeight;
				_isMeasuringMaxHeight = true;

				try
				{
					// AS 11/2/10 TFS49402/TFS49912/TFS51985
					// Since the GetCommonAncestor will now walk past the popup hosting a 
					// toolwindow we can't use that. Anyway, all we want is to walk up the visual 
					// tree only.
					//
					//// AS 6/8/09 TFS18150
					//UIElement rootVisual = Utilities.GetCommonAncestor(this, null) as UIElement;
					UIElement rootVisual = this;

					while ( rootVisual != null )
					{
						UIElement temp = Utilities.GetParent(rootVisual, false) as UIElement;

						if ( temp == null )
							break;

						rootVisual = temp;
					}

					Debug.Assert(null != rootVisual && rootVisual.GetType().Name == "PopupRoot");

					if (null == rootVisual)
						return Size.Empty;

					Utilities.InvalidateMeasure(this, rootVisual);
					this.InvalidateMeasure();
					rootVisual.InvalidateMeasure();

					rootVisual.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

					Size maxSize = rootVisual.DesiredSize;

					this.InvalidateMeasure();
					rootVisual.InvalidateMeasure();
					Utilities.InvalidateMeasure(this, rootVisual);

					return maxSize;
				}
				finally
				{
					_isMeasuringMaxHeight = wasMeasuring;
				}
			}
			#endregion //Methods
		}
		#endregion //PopupChildWrapper class

		// AS 4/28/11 TFS73532
		#region DeferShowHelper class
		private class DeferShowHelper : ToolWindow.IDeferShow
		{
			private ToolWindowHostPopup _window;

			#region Constructor
			internal DeferShowHelper(ToolWindowHostPopup window)
			{
				_window = window;
			} 
			#endregion //Constructor

			public void Show(bool activate)
			{
				Popup popup = _window._popup;

				Debug.Assert(null != popup, "No popup?");

                ToolWindow toolWindow = _window.Child as ToolWindow;
                Debug.Assert(null != toolWindow, "No longer references the toolwindow?");

                // AS 12/6/11 TFS97074
                // Since it may be shown well after the initial show call resync the AllowsTransparency state.
                //
                if (null != toolWindow)
                    popup.AllowsTransparency = toolWindow.AllowsTransparencyResolved;

				popup.IsOpen = true;

				toolWindow = _window.Child as ToolWindow;
				Debug.Assert(null != toolWindow, "No longer references the toolwindow?");

				if (activate && null != toolWindow)
				{
					toolWindow.Activate();

					// the wpf framework (in the KeyboardDevice.ReevaluateFocusCallback) 
					// thinks that the popup's presentation source is not active and therefore
					// they try to take focus away but they do so without asking
					// the current focused element so our previewlostkeyboardfocus handler
					// is not getting invoked. to work around this, i'm putting in a delayed
					// callback (the ReevaluateFocusCallback is done with an Input priority) 
					// at which point we will take focus back into the tool window.
					//
					toolWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background, new System.Threading.ContextCallback(_window.VerifyActivation), null);
				}

				// AS 8/4/11 TFS83465/TFS83469
				this._window.VerifyIsInView(false, true, false);
			}

			public void Cancel()
			{
				Debug.Assert(_window._popup.IsOpen == false, "The popup was already opened!");
				ToolWindow tw = _window.Child as ToolWindow;

				if (null != tw && tw.IsModal == false)
				{
					_window.Child = null;

					CurrentPopups.Remove(_window);

					if (null != tw)
						tw.OnClosedInternal();
				}
			}
		}
		#endregion //DeferShowHelper class
	}

    // AS 3/30/09 TFS16355 - WinForms Interop
    internal class PopupController : DependencyObject
    {
        #region Member Variables

        private FrameworkElement _owner;
        private PresentationSource _ownerSource;
        private PresentationSource _childSource;
        private Popup _popup;
        private bool? _topMost;

        private static bool _controllerCreateFailed;

        #endregion //Member Variables

        #region Constructor
        private PopupController(Popup popup, FrameworkElement owner)
        {
            _owner = owner;

            _popup = popup;
            _popup.Opened += new EventHandler(OnPopupOpened);
			// AS 9/21/09 TFS18595
            //_popup.RequestBringIntoView += new RequestBringIntoViewEventHandler(OnPopupRequestBringIntoView);

            BindingOperations.SetBinding(this, HorizontalOffsetProperty, Utilities.CreateBindingObject(Popup.HorizontalOffsetProperty, BindingMode.OneWay, popup));
            BindingOperations.SetBinding(this, VerticalOffsetProperty, Utilities.CreateBindingObject(Popup.VerticalOffsetProperty, BindingMode.OneWay, popup));
        }
        #endregion //Constructor

        #region Properties

        #region Public Properties

        #region TopMost
        public bool? TopMost
        {
            get { return _topMost; }
            set { _topMost = value; }
        }
        #endregion //TopMost

        #endregion //Public Properties

        #region Private Properties

        #region HorizontalOffset

        private static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset",
            typeof(double), typeof(PopupController), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnOffsetChanged)));

        #endregion //HorizontalOffset

        #region IsTopMost
        private bool IsTopMost
        {
            get
            {
                bool? isTopMost = this.TopMost;

                if (isTopMost == null)
                {
                    isTopMost = false;

                    if (null != _ownerSource)
                    {
                        Window w = _ownerSource.RootVisual as Window;

                        if (null != w && !Utilities.IsBrowserWindow(w))
                        {
                            isTopMost = w.Topmost;
                        }
                        else
                        {
                            HwndSource hs = _ownerSource as HwndSource;

                            if (null != hs && !hs.IsDisposed)
                            {
                                isTopMost = NativeWindowMethods.IsTopMostApi(hs.Handle);
                            }
                        }
                    }
                }

                return isTopMost.Value;
            }
        }
        #endregion //IsTopMost

        #region VerticalOffset

        private static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register("VerticalOffset",
            typeof(double), typeof(PopupController), new FrameworkPropertyMetadata(0d, new PropertyChangedCallback(OnOffsetChanged)));

        #endregion //VerticalOffset

        #endregion //Private Properties

        #endregion //Properties

        #region Methods

        #region Public Methods

        #region Create
        /// <summary>
        /// Creates a new PopupController for the specified popup.
        /// </summary>
        /// <param name="popup">The Popup which the returned controller will be manipulating.</param>
        /// <param name="owner">The element for which the popup will be associated.</param>
        /// <returns>A new popup controller or null if one could not be created.</returns>
        public static PopupController Create(Popup popup, FrameworkElement owner)
        {
            if (!_controllerCreateFailed)
            {
                try
                {
                    // we'll need ui permissions to use the PresentationSource methods
                    UIPermission perm = new UIPermission(UIPermissionWindow.AllWindows);
                    perm.Demand();

                    Debug.Assert(popup.Placement == PlacementMode.Absolute);

                    return new PopupController(popup, owner);
                }
                catch (SecurityException)
                {
                    _controllerCreateFailed = true;
                }
            }

            return null;
        }
        #endregion //Create

        #endregion //Public Methods

        #region Internal Methods

        #region BringToFront
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void BringToFront(Popup popup)
        {
            if (popup.IsOpen)
            {
                UIElement child = popup.Child;

                if (null != child)
                {
                    HwndSource hs = HwndSource.FromVisual(child) as HwndSource;

                    if (hs.IsDisposed == false && hs.Handle != IntPtr.Zero)
                    {
                        NativeWindowMethods.SetWindowPosApi(hs.Handle,
                            NativeWindowMethods.HWND_TOP,
                            0, 0, 0, 0,
                            NativeWindowMethods.SWP_NOMOVE |
                            NativeWindowMethods.SWP_NOACTIVATE |
                            NativeWindowMethods.SWP_NOSIZE);
                    }
                }
            }
        }
        #endregion //BringToFront 

        #endregion //Internal Methods

        #region Private Methods

        #region AddChildHook
        private void AddChildHook(PresentationSource newSource)
        {
            this.RemoveChildHook(newSource);

            HwndSource hs = newSource as HwndSource;

            if (hs != null)
            {
                hs.AddHook(new HwndSourceHook(this.ChildHookProc));
            }
        }
        #endregion //AddChildHook

        #region AddOwnerHook
        private void AddOwnerHook(PresentationSource newSource)
        {
            this.RemoveOwnerHook(newSource);

            HwndSource hs = newSource as HwndSource;

            if (hs != null)
            {
                hs.AddHook(new HwndSourceHook(this.OwnerHookProc));
            }
        }
        #endregion //AddOwnerHook

        #region BringToFront
        private object BringToFront(object state)
        {
            HwndSource hs = state as HwndSource;

            if (null != hs
                && !hs.IsDisposed
                && hs.Handle != IntPtr.Zero)
            {
                Window w = Window.GetWindow(this._popup);

				// AS 1/3/12 TFS98257
				// We don't want to activate the window if focus is within the popup we are 
				// about to bring to the front (e.g. the popup contains an HwndHost that 
				// contains the keyboard focus).
				//
				//if (null != w && !Utilities.IsBrowserWindow(w))
				//    w.Activate();
				if (null != w && !Utilities.IsBrowserWindow(w))
				{
					IntPtr focus = NativeWindowMethods.GetFocus();
					IntPtr windowHwnd = new WindowInteropHelper(w).Handle;
					bool isActive = false;

					while (focus != IntPtr.Zero)
					{
						if (focus == windowHwnd || focus == hs.Handle)
						{
							isActive = true;
							break;
						}

						focus = NativeWindowMethods.GetParent(focus);
					}

					if (!isActive)
						w.Activate();
				}

                IntPtr hwndAfter = NativeWindowMethods.HWND_TOP;

                
#region Infragistics Source Cleanup (Region)























































#endregion // Infragistics Source Cleanup (Region)


                NativeWindowMethods.SetWindowPosApi(hs.Handle,
                    hwndAfter,
                    0, 0, 0, 0,
                    NativeWindowMethods.SWP_NOMOVE |
                    NativeWindowMethods.SWP_NOACTIVATE |
                    NativeWindowMethods.SWP_NOSIZE);
            }

            return null;
        }
        #endregion //BringToFront

        #region ChildHookProc
        private IntPtr ChildHookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {





            switch (msg)
            {
                case NativeWindowMethods.WM_WINDOWPOSCHANGING:
                    {
                        NativeWindowMethods.WINDOWPOS windowPosStruct = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.WINDOWPOS));

                        HwndSource hs = _childSource as HwndSource;

                        CompositionTarget ct = null != hs ? hs.CompositionTarget : null;
                        Matrix toDevice = ct != null
                            ? hs.CompositionTarget.TransformToDevice
                            : Matrix.Identity;
                        Point pt = new Point(_popup.HorizontalOffset, _popup.VerticalOffset);

						// AS 8/4/11 TFS83465/TFS83469
						// The horizontal/vertical offsets are logical units so convert to device units
						//
						pt = toDevice.Transform(pt);

                        NativeWindowMethods.POINT ptScreen = new NativeWindowMethods.POINT((int)pt.X, (int)pt.Y);

						windowPosStruct.x = ptScreen.X;
						windowPosStruct.y = ptScreen.Y;

                        Marshal.StructureToPtr(windowPosStruct, lParam, true);
                        break;
                    }


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


				case NativeWindowMethods.WM_MOUSEACTIVATE:
                    // when the mouse is pressed down on the popup we want to 
                    // bring it to the front
                    this.BringToFront(_childSource);
                    break;
            }

            return IntPtr.Zero;
        }
        #endregion //ChildHookProc

        #region InitializeChildSource
        private void InitializeChildSource(PresentationSource source)
        {
            
            //Debug.Assert(null != _popupSource);
            HwndSource hs = source as HwndSource;

            if (null != hs)
            {
                bool isTopMost = this.IsTopMost;
                IntPtr hwnd = hs.Handle;

                if (hwnd != IntPtr.Zero)
                {
                    // remove the minimize/maximize bits
                    IntPtr style = NativeWindowMethods.GetWindowLong(hwnd, NativeWindowMethods.GWL_EXSTYLE);
                    IntPtr newStyle = style;

                    if (isTopMost)
                        newStyle = NativeWindowMethods.AddBits(newStyle, NativeWindowMethods.WS_EX_TOPMOST);
                    else
                        newStyle = NativeWindowMethods.RemoveBits(newStyle, NativeWindowMethods.WS_EX_TOPMOST);

                    if (style != newStyle)
                    {
                        IntPtr hwndAfter = isTopMost
                            ? NativeWindowMethods.HWND_TOPMOST
                            : NativeWindowMethods.HWND_NOTOPMOST;

                        NativeWindowMethods.SetWindowPosApi(hwnd, hwndAfter, 0, 0, 0, 0,
                            NativeWindowMethods.SWP_NOACTIVATE |
                            NativeWindowMethods.SWP_NOMOVE |
                            NativeWindowMethods.SWP_NOSIZE);
                    }
                }

                this.AddChildHook(source);
            }
        }
        #endregion //InitializeChildSource

        #region OnChildSourceChanged
        private void OnChildSourceChanged(object sender, SourceChangedEventArgs e)
        {
            this.RemoveChildHook(e.OldSource);

            _childSource = e.NewSource;
            this.InitializeChildSource(e.NewSource);
        }
        #endregion //OnChildSourceChanged

        #region OnOffsetChanged
        private static void OnOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PopupController)d).RefreshPopupLocation();
        }
        #endregion //OnOffsetChanged

        #region OnOwnerSourceChanged
        private void OnOwnerSourceChanged(object sender, SourceChangedEventArgs e)
        {
            this.RemoveOwnerHook(e.OldSource);

            _ownerSource = e.NewSource;
            this.AddOwnerHook(_ownerSource);
        }
        #endregion //OnOwnerSourceChanged

        #region OnPopupClosed
        private void OnPopupClosed(object sender, EventArgs e)
        {
            _popup.Closed -= new EventHandler(OnPopupClosed);
            _popup.Opened += new EventHandler(OnPopupOpened);

            this.RemoveChildHook(_childSource);
            this.RemoveOwnerHook(_ownerSource);

            _ownerSource = null;
            _childSource = null;
            PresentationSource.RemoveSourceChangedHandler(_popup, new SourceChangedEventHandler(OnOwnerSourceChanged));
            PresentationSource.RemoveSourceChangedHandler(_popup.Child, new SourceChangedEventHandler(OnChildSourceChanged));
        }
        #endregion //OnPopupClosed

        #region OnPopupOpened
        private void OnPopupOpened(object sender, EventArgs e)
        {
            _popup.Opened -= new EventHandler(OnPopupOpened);
            _popup.Closed += new EventHandler(OnPopupClosed);

            _ownerSource = PresentationSource.FromVisual(_owner ?? _popup);
            _childSource = PresentationSource.FromVisual(_popup.Child) as HwndSource;

            this.InitializeChildSource(_childSource);
            this.AddOwnerHook(_ownerSource);

            PresentationSource.AddSourceChangedHandler(_popup, new SourceChangedEventHandler(OnOwnerSourceChanged));
            PresentationSource.AddSourceChangedHandler(_popup.Child, new SourceChangedEventHandler(OnChildSourceChanged));

			// AS 7/14/09 TFS18424
			// In RightToLeft, the popup is positioned in the wrong spot but forcing it to get into our
			// WndProc hook to update its position addresses this issue.
			//
			this.RefreshPopupLocation();

			this.BringToFront(_childSource);
        }
        #endregion //OnPopupOpened

        #region OnPopupRequestBringIntoView
		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		#endregion //OnPopupRequestBringIntoView

		#region OwnerHookProc
		private IntPtr OwnerHookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case NativeWindowMethods.WM_WINDOWPOSCHANGED:
                    {
						// AS 6/8/09 TFS18150
						//IToolWindowHost host = _popup.Child as IToolWindowHost;
						IToolWindowHost host = ToolWindowHostPopup.GetHost(_popup);

                        if (null != host)
                        {
                            NativeWindowMethods.WINDOWPOS windowPosStruct = (NativeWindowMethods.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(NativeWindowMethods.WINDOWPOS));

                            const int NoMoveSize = NativeWindowMethods.SWP_NOMOVE | NativeWindowMethods.SWP_NOSIZE;

                            if ((windowPosStruct.flags & NoMoveSize) != NoMoveSize)
                                host.RelativePositionStateChanged();
                        }

                        break;
                    }

					// AS 8/4/11 TFS83465/TFS83469
				case NativeWindowMethods.WM_DISPLAYCHANGE:
					{
						IToolWindowHost host = ToolWindowHostPopup.GetHost(_popup);

						if (null != host)
							host.EnsureOnScreen(true);
						break;
					}
            }

            return IntPtr.Zero;
        }
        #endregion //OwnerHookProc

        #region RefreshPopupLocation
        private void RefreshPopupLocation()
        {
            HwndSource hs = _childSource as HwndSource;

            if (null != hs
                && hs.IsDisposed == false
                && hs.CompositionTarget != null
                && hs.Handle != IntPtr.Zero
                )
            {
                Matrix toDevice = hs.CompositionTarget.TransformToDevice;

                Point pt = new Point(_popup.HorizontalOffset, _popup.VerticalOffset);
                Point screenPt = toDevice.Transform(pt);

                int flags = NativeWindowMethods.SWP_NOACTIVATE |
                    NativeWindowMethods.SWP_NOSIZE |
                    NativeWindowMethods.SWP_NOOWNERZORDER |
                    NativeWindowMethods.SWP_NOZORDER;

                NativeWindowMethods.SetWindowPosApi(hs.Handle, IntPtr.Zero,
                    (int)screenPt.X, (int)screenPt.Y,
                    0, 0,
                    flags);
            }
        }
        #endregion //RefreshPopupLocation

        #region RemoveChildHook
        private void RemoveChildHook(PresentationSource oldSource)
        {
            HwndSource hs = oldSource as HwndSource;

            if (hs != null)
            {
                hs.RemoveHook(new HwndSourceHook(this.ChildHookProc));
            }
        }
        #endregion //RemoveChildHook

        #region RemoveOwnerHook
        private void RemoveOwnerHook(PresentationSource oldSource)
        {
            HwndSource hs = oldSource as HwndSource;

            if (hs != null)
            {
                hs.RemoveHook(new HwndSourceHook(this.OwnerHookProc));
            }
        }
        #endregion //RemoveOwnerHook

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