using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Data;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// Base class for a ToolWindow host that will be hosted in another element container.
    /// </summary>
    // AS 3/30/09 TFS16355 - WinForms Interop
    // Changed from Decorator to FrameworkElement so we can control whether the 
    // child is a logical child. The decorator always tries to make the value of the 
    // Child property into a logical child which will raise an exception if its already 
    // a logical child of something else.
    //
    //internal abstract class ToolWindowContainer : Decorator, IToolWindowHost
    internal abstract class ToolWindowContainer : FrameworkElement, IToolWindowHost
    {
        #region Member Variables

        private CaptureAction _captureAction;
        private ResizeFlags _resizeFlags;
        private Point _lastMousePoint;

        // AS 3/30/09 TFS16355 - WinForms Interop
        // Need to track the child explicitly since we're not derived from Decorator anymore.
        //
        private UIElement _child;

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		private bool _isInitialPositionPending;

        #endregion //Member Variables

        #region Constructor
        internal ToolWindowContainer()
        {
        }

        static ToolWindowContainer()
        {
            FrameworkElement.HorizontalAlignmentProperty.OverrideMetadata(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
            FrameworkElement.VerticalAlignmentProperty.OverrideMetadata(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentStretchBox));

            // just like a floating window, this element should be its own focus scope
            FocusManager.IsFocusScopeProperty.OverrideMetadata(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
            EventManager.RegisterClassHandler(typeof(ToolWindowContainer), Keyboard.PreviewLostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnPreviousLostKeyboardFocus));

            // IsActive should be true even if focus is logically within us but not a direct visual descendant
            EventManager.RegisterClassHandler(typeof(ToolWindowContainer), Keyboard.GotKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnGotKeyboardFocusWithin), true);
            EventManager.RegisterClassHandler(typeof(ToolWindowContainer), Keyboard.LostKeyboardFocusEvent, new KeyboardFocusChangedEventHandler(OnLostKeyboardFocusWithin), true);

            // AS 4/25/08
            // We should activate the window if you use the mouse down on the element.
            //
            EventManager.RegisterClassHandler(typeof(ToolWindowContainer), Mouse.MouseDownEvent, new MouseButtonEventHandler(OnWindowMouseDown), true);

            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
            KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
        }
        #endregion //Constructor

        #region Base class overrides

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region ArrangeOverride
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (null != _child)
                _child.Arrange(new Rect(finalSize));

            return finalSize;
        } 
        #endregion //ArrangeOverride

        #region Child
        // AS 3/30/09 TFS16355 - WinForms Interop
        //public override UIElement Child
        public UIElement Child
        {
            get
            {
                // AS 3/30/09 TFS16355 - WinForms Interop
                //return base.Child;
                return _child;
            }
            set
            {
                Debug.Assert(value == null || value is ToolWindow);

                UIElement oldChild = this.Child;

                if (oldChild != null)
                {
                    oldChild.ClearValue(ToolWindow.HostProperty);
                }

                // AS 3/30/09 TFS16355 - WinForms Interop
                //base.Child = value;
                _child = value;

                // AS 3/30/09 TFS16355 - WinForms Interop
                if (null != oldChild)
                {
                    this.RemoveVisualChild(oldChild);

                    if (LogicalTreeHelper.GetParent(oldChild) == this)
                        this.RemoveLogicalChild(oldChild);
                }

                if (null != value)
                {
                    this.AddVisualChild(value);

                    if (LogicalTreeHelper.GetParent(value) == null)
                        this.AddLogicalChild(value);
                }

                if (this.Child != null)
                {
					// AS 11/2/10 TFS49402/TFS49912/TFS51985
					ToolWindow tw = value as ToolWindow;
					_isInitialPositionPending = tw != null && tw.WindowStartupLocation != ToolWindowStartupLocation.Manual;


                    this.Child.SetValue(ToolWindow.HostProperty, this);
					// AS 11/2/10 TFS49402/TFS49912/TFS51985
					// This is not specific to this bug but we shouldn't be bound to our own state. I think this meant to be that of the child.
					//
					//this.SetBinding(VisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.TwoWay, this));
                    this.SetBinding(VisibilityProperty, Utilities.CreateBindingObject(VisibilityProperty, BindingMode.TwoWay, value));

                    this.SetBinding(ToolWindowContainer.LeftProperty, Utilities.CreateBindingObject(ToolWindow.LeftProperty, BindingMode.TwoWay, value));
                    this.SetBinding(ToolWindowContainer.TopProperty, Utilities.CreateBindingObject(ToolWindow.TopProperty, BindingMode.TwoWay, value));
                    this.SetBinding(FrameworkElement.WidthProperty, Utilities.CreateBindingObject(FrameworkElement.WidthProperty, BindingMode.TwoWay, value));
                    this.SetBinding(FrameworkElement.HeightProperty, Utilities.CreateBindingObject(FrameworkElement.HeightProperty, BindingMode.TwoWay, value));

                    this.SetBinding(FrameworkElement.MaxWidthProperty, Utilities.CreateBindingObject(FrameworkElement.MaxWidthProperty, BindingMode.OneWay, value));
                    this.SetBinding(FrameworkElement.MaxHeightProperty, Utilities.CreateBindingObject(FrameworkElement.MaxHeightProperty, BindingMode.OneWay, value));
                    this.SetBinding(FrameworkElement.MinWidthProperty, Utilities.CreateBindingObject(FrameworkElement.MinWidthProperty, BindingMode.OneWay, value));
                    this.SetBinding(FrameworkElement.MinHeightProperty, Utilities.CreateBindingObject(FrameworkElement.MinHeightProperty, BindingMode.OneWay, value));

                    this.SetBinding(ToolWindowContainer.TopmostProperty, Utilities.CreateBindingObject(ToolWindow.TopmostProperty, BindingMode.OneWay, value));
                }
                else
                {
                    BindingOperations.ClearAllBindings(this);
                }

                this.OnChildChanged(oldChild, this.Child);
            }
        }
        #endregion //Child

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region GetVisualChild
        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && null != _child)
                return _child;

            return base.GetVisualChild(index);
        } 
        #endregion //GetVisualChild

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region LogicalChildren
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                if (null != _child && LogicalTreeHelper.GetParent(_child) == this)
                    return new SingleItemEnumerator(_child);

                return EmptyEnumerator.Instance;
            }
        } 
        #endregion //LogicalChildren

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region MeasureOverride
        protected override Size MeasureOverride(Size availableSize)
        {
            Size desired = new Size();

            if (null != _child)
            {
                _child.Measure(availableSize);
                desired = _child.DesiredSize;
            }

            return desired;
        } 
        #endregion //MeasureOverride

        #region OnMouseMove
        /// <summary>
        /// Invoked when the mouse has been moved.
        /// </summary>
        /// <param name="e">Provides data for the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.IsMouseCaptured && this._captureAction != CaptureAction.None)
                this.ProcessCaptureAction(e);

            base.OnMouseMove(e);
        }
        #endregion //OnMouseMove

        #region OnMouseLeftButtonUp
        /// <summary>
        /// Invoked when the left mouse button is released.
        /// </summary>
        /// <param name="e">Provides data for the event.</param>
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.IsMouseCaptured)
            {
                this._captureAction = CaptureAction.None;

                

                e.Handled = true;

                this.ReleaseMouseCapture();
            }

            base.OnMouseLeftButtonUp(e);
        }
        #endregion //OnMouseLeftButtonUp

        #region OnLostMouseCapture
        /// <summary>
        /// Invoked when the element loses mouse capture.
        /// </summary>
        /// <param name="e">Provides data for the event.</param>
        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            this.ClearValue(CursorProperty);

            if (this._captureAction != CaptureAction.None)
            {
                
            }

            base.OnLostMouseCapture(e);
        }
        #endregion //OnLostMouseCapture

        // AS 3/30/09 TFS16355 - WinForms Interop
        #region VisualChildrenCount
        protected override int VisualChildrenCount
        {
            get
            {
                return _child == null ? 0 : 1;
            }
        }
        #endregion //VisualChildrenCount

        #endregion //Base class overrides

        #region Properties

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region IsInitialPositionPending
		internal bool IsInitialPositionPending
		{
			get { return _isInitialPositionPending; }
		} 
		#endregion // IsInitialPositionPending

		// AS 6/8/09 TFS18150
		#region IsResizing
		protected bool IsResizing
		{ 
			get { return _captureAction == CaptureAction.Resize; }
		}
		#endregion //IsResizing

		#region Left

		/// <summary>
        /// Identifies the <see cref="Left"/> dependency property
        /// </summary>
        public static readonly DependencyProperty LeftProperty = ToolWindow.LeftProperty.AddOwner(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public double Left
        {
            get
            {
                return (double)this.GetValue(ToolWindowContainer.LeftProperty);
            }
            set
            {
                this.SetValue(ToolWindowContainer.LeftProperty, value);
            }
        }

        #endregion //Left

        #region Top

        /// <summary>
        /// Identifies the <see cref="Top"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TopProperty = ToolWindow.TopProperty.AddOwner(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public double Top
        {
            get
            {
                return (double)this.GetValue(ToolWindowContainer.TopProperty);
            }
            set
            {
                this.SetValue(ToolWindowContainer.TopProperty, value);
            }
        }

        #endregion //Top

        #region Topmost

        /// <summary>
        /// Identifies the <see cref="Topmost"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TopmostProperty = ToolWindow.TopmostProperty.AddOwner(typeof(ToolWindowContainer), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

        public bool Topmost
        {
            get
            {
                return (bool)this.GetValue(ToolWindowContainer.TopmostProperty);
            }
            set
            {
                this.SetValue(ToolWindowContainer.TopmostProperty, value);
            }
        }

        #endregion //Topmost

        #region UsesRelativePosition
        internal bool UsesRelativePosition
        {
            get
            {
                ToolWindow window = this.Child as ToolWindow;

                return null != window &&
                    // AS 8/15/08
                    //(window.VerticalAlignmentMode == ToolWindowAlignmentMode.UseAlignment ||
                    // window.HorizontalAlignmentMode == ToolWindowAlignmentMode.UseAlignment);
                    window.UsesRelativePosition;
            }
        }
        #endregion //UsesRelativePosition

        #endregion //Properties

        #region Methods

        #region GetRelativeScreenPoint
        protected abstract Point GetRelativeScreenPoint(MouseEventArgs e);

        #endregion //GetRelativeScreenPoint

        #region OnChildChanged
        protected virtual void OnChildChanged(UIElement oldChild, UIElement newChild)
        {
        }
        #endregion //OnChildChanged

        #region OnEnteringMode
        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        protected virtual void OnEnteringMove(MouseEventArgs e)
        {
        } 
        #endregion //OnEnteringMode

        #region OnEnteringDrag
        protected virtual void OnEnteringDrag(MouseEventArgs e, ToolWindowResizeElementLocation location)
        {
        } 
        #endregion //OnEnteringDrag

        #region OnGotKeyboardFocusWithin
        private static void OnGotKeyboardFocusWithin(object sender, KeyboardFocusChangedEventArgs e)
        {
            // something within the toolwindow has gotten focus. make sure the toolwindow considers itself as active
            ToolWindow newWindow = e.NewFocus is DependencyObject ? ToolWindow.GetToolWindow((DependencyObject)e.NewFocus) : null;

            if (null != newWindow && false == newWindow.IsActive)
            {
                newWindow.SetValue(ToolWindow.IsActivePropertyKey, KnownBoxes.TrueBox);
            }
        }
        #endregion // OnGotKeyboardFocusWithin

        #region OnLostKeyboardFocusWithin
        private static void OnLostKeyboardFocusWithin(object sender, KeyboardFocusChangedEventArgs e)
        {
            // something within the toolwindow is losing focus. if focus is not still within the toolwindow
            // then clear the isactive flag
            ToolWindow oldWindow = e.OldFocus is DependencyObject ? ToolWindow.GetToolWindow((DependencyObject)e.OldFocus) : null;
            ToolWindow newWindow = e.NewFocus is DependencyObject ? ToolWindow.GetToolWindow((DependencyObject)e.NewFocus) : null;

            if (oldWindow != newWindow && oldWindow != null)
            {
                oldWindow.ClearValue(ToolWindow.IsActivePropertyKey);
            }
        }
        #endregion // OnLostKeyboardFocusWithin

        #region OnPreviousLostKeyboardFocus
        private static void OnPreviousLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ToolWindowContainer container = sender as ToolWindowContainer;

            // when elements (like a menu item) call Keyboard.Focus(null) to try and put
            // focus back to the default element, they are causing focus to go to the main
            // browser window when in an xbap. really what should happen is that the last
            // focused element in that window should get focus. well in the case of an xbap,
            // this is the containing window and therefore we should take the focus and 
            // put it into our focused element.
            if (e.NewFocus is Window && VisualTreeHelper.GetParent((Window)e.NewFocus) == null)
            {
                DependencyObject oldFocus = e.OldFocus as DependencyObject;

                // AS 4/23/08
                // This logic is causing focus to shift within a panetoolwindow that contains
                // multiple contentpanes. Basically if you have 2 or more content panes in a floating
                // split pane and focus is in the second, we are shifting focus to the first
                // when this event is handled because we're assuming to focus our first item.
                //
                if (null != oldFocus && container.IsAncestorOf(oldFocus))
                {
                    UIElement element = FocusManager.GetFocusScope(oldFocus) as UIElement;

                    if (element != container && element.IsVisible)
                        return;
                }

                e.Handled = true;

                IInputElement focusedElement = FocusManager.GetFocusedElement(container);

                if (null != focusedElement)
                    focusedElement.Focus();
                else
                    container.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }
        #endregion //OnPreviousLostKeyboardFocus

        #region OnWindowMouseDown
        private static void OnWindowMouseDown(object sender, MouseButtonEventArgs e)
        {
            ToolWindowContainer adorner = (ToolWindowContainer)sender;
            ((IToolWindowHost)adorner).Activate();
        }
        #endregion //OnWindowMouseDown

		// AS 11/2/10 TFS49402/TFS49912/TFS51985
		#region PerformInitialPositioning
		internal void PerformInitialPositioning( ref Rect thisRect )
		{
			ToolWindow tw = this.Child as ToolWindow;

			if ( null != tw && this.IsInitialPositionPending )
			{
				FrameworkElement owner = tw.Owner;

				if ( null != owner )
				{
					this._isInitialPositionPending = false;

					Rect? screenRect = null;

					switch ( tw.WindowStartupLocation )
					{
						case ToolWindowStartupLocation.CenterOwnerWindow:
							{
								UIElement ownerWindow = owner as ToolWindow
									?? ToolWindow.GetToolWindow(owner)
									?? Utilities.GetCommonAncestor(owner, null) as UIElement
									?? Window.GetWindow(this);

								// the render size of a window includes the non-client area so use the first child
								if ( ownerWindow is Window && VisualTreeHelper.GetChildrenCount(ownerWindow) > 0 )
									ownerWindow = VisualTreeHelper.GetChild(ownerWindow, 0) as UIElement;

								if ( ownerWindow == null )
									screenRect = Utilities.GetWorkArea(this);
								else
									screenRect = new Rect(Utilities.PointToScreenSafe(ownerWindow, new Point()), ownerWindow.RenderSize);
								break;
							}
						case ToolWindowStartupLocation.CenterScreen:
							{
								screenRect = Utilities.GetWorkArea(this);
								break;
							}
					}

					if ( null != screenRect )
					{
						Rect screenRectValue = screenRect.Value;
						thisRect.Y = screenRectValue.Top + (screenRectValue.Height - thisRect.Height) / 2;
						thisRect.X = screenRectValue.Left + (screenRectValue.Width - thisRect.Width) / 2;

						this.Left = thisRect.X;
						this.Top = thisRect.Y;
					}
				}
			}
		}
		#endregion // PerformInitialPositioning

        #region ProcessCaptureAction
        private void ProcessCaptureAction(MouseEventArgs e)
        {
            

            Point newPoint = this.GetRelativeScreenPoint(e);

            if (newPoint != this._lastMousePoint)
            {
                // get the offset
                double offsetX = newPoint.X - this._lastMousePoint.X;
                double offsetY = newPoint.Y - this._lastMousePoint.Y;

                this._lastMousePoint = newPoint;

                double x = this.Left;
                double y = this.Top;

                // AS 5/22/08 BR33228
                // If the left and/or top was never set then we can't alter it so assume 0 for the value.
                //
                if (double.IsNaN(x))
                    x = 0;
                if (double.IsNaN(y))
                    y = 0;

                double currentWidth = this.Width;
                double currentHeight = this.Height;

                // use the current values by default
                double newWidth = currentWidth;
                double newHeight = currentHeight;

                if (this._captureAction == CaptureAction.Drag)
                {
                    x += offsetX;
                    y += offsetY;
                }
                else if (this._captureAction == CaptureAction.Resize)
                {
                    if (double.IsNaN(currentWidth))
                        currentWidth = this.ActualWidth;
                    if (double.IsNaN(currentHeight))
                        currentHeight = this.ActualHeight;

                    double maxWidth = this.MaxWidth;
                    double maxHeight = this.MaxHeight;
                    double minWidth = this.MinWidth;
                    double minHeight = this.MinHeight;

                    #region Left/Right
                    if ((this._resizeFlags & ResizeFlags.Left) == ResizeFlags.Left)
                    {
                        newWidth = currentWidth - offsetX;  // increase width
                        x += offsetX;		// move left

                        if (double.IsNaN(maxWidth) == false && newWidth > maxWidth)
                        {
                            double adjustment = newWidth - maxWidth;
                            x += adjustment;
                            this._lastMousePoint.X += adjustment;
                            newWidth = maxWidth;
                        }

                        if (newWidth < this.MinWidth)
                        {
                            double adjustment = this.MinWidth - newWidth;
                            x -= adjustment;
                            this._lastMousePoint.X -= adjustment;
                            newWidth = this.MinWidth;
                        }
                    }
                    else if ((this._resizeFlags & ResizeFlags.Right) == ResizeFlags.Right)
                    {
                        newWidth = currentWidth + offsetX;

                        if (double.IsNaN(maxWidth) == false && newWidth > maxWidth)
                        {
                            double adjustment = newWidth - maxWidth;
                            this._lastMousePoint.X -= adjustment;
                            newWidth = maxWidth;
                        }

                        if (newWidth < minWidth)
                        {
                            double adjustment = minWidth - newWidth;
                            this._lastMousePoint.X += adjustment;
                            newWidth = minWidth;
                        }
                    }
                    #endregion //Left/Right

                    #region Top/Bottom
                    if ((this._resizeFlags & ResizeFlags.Top) == ResizeFlags.Top)
                    {
                        newHeight = currentHeight - offsetY;  // increase Height
                        y += offsetY;		// move Top

                        if (double.IsNaN(maxHeight) == false && newHeight > maxHeight)
                        {
                            double adjustment = newHeight - maxHeight;
                            y += adjustment;
                            this._lastMousePoint.Y += adjustment;
                            newHeight = maxHeight;
                        }

                        if (newHeight < minHeight)
                        {
                            double adjustment = minHeight - newHeight;
                            y -= adjustment;
                            this._lastMousePoint.Y -= adjustment;
                            newHeight = minHeight;
                        }
                    }
                    else if ((this._resizeFlags & ResizeFlags.Bottom) == ResizeFlags.Bottom)
                    {
                        newHeight = currentHeight + offsetY;

                        if (double.IsNaN(maxHeight) == false && newHeight > maxHeight)
                        {
                            double adjustment = newHeight - maxHeight;
                            this._lastMousePoint.Y -= adjustment;
                            newHeight = maxHeight;
                        }

                        if (newHeight < minHeight)
                        {
                            double adjustment = minHeight - newHeight;
                            this._lastMousePoint.Y += adjustment;
                            newHeight = minHeight;
                        }
                    }
                    #endregion //Top/Bottom
                }

                this.ProcessMoveSize(x, y, newWidth, newHeight);
            }
        }
        #endregion //ProcessCaptureAction

        #region ProcessMoveSize
        protected virtual void ProcessMoveSize(double left, double top, double width, double height)
        {
            ToolWindow tw = this.Child as ToolWindow;
            bool updateHorz = tw.UsesRelativePosition == false || tw.HorizontalAlignmentMode == ToolWindowAlignmentMode.Manual;
            bool updateVert = tw.UsesRelativePosition == false || tw.VerticalAlignmentMode == ToolWindowAlignmentMode.Manual;

            if (updateHorz)
                this.Left = left;

            if (updateVert)
                this.Top = top;

            bool invalidateParent = false;

            if (updateHorz && width != this.Width)
            {
                this.Width = width;
                invalidateParent = true;
            }

            if (updateVert && height != this.Height)
            {
                this.Height = height;
                invalidateParent = true;
            }

            
            if (invalidateParent)
                ((UIElement)this.Parent).InvalidateArrange();
        }

        #endregion //ProcessMoveSize

        #endregion //Methods

        #region IToolWindowHost Members

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void IToolWindowHost.DragMove()
        void IToolWindowHost.DragMove(MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                throw new InvalidOperationException(SR.GetString("LE_ToolWindowLeftMouseButtonNotDown"));

            // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
            this.OnEnteringMove(e);

            // store the offset into this element in screen coordinates
            // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
            //this._lastMousePoint = Utilities.PointToScreenSafe(this, Mouse.GetPosition(this));
            this._lastMousePoint = this.GetRelativeScreenPoint(e);

            this.CaptureMouse();

            if (this.IsMouseCaptured)
                this._captureAction = CaptureAction.Drag;
        }

        // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
        //void IToolWindowHost.DragResize(ToolWindowResizeElementLocation location, Cursor cursor)
        void IToolWindowHost.DragResize(ToolWindowResizeElementLocation location, Cursor cursor, MouseEventArgs e)
        {
            if (Mouse.LeftButton != MouseButtonState.Pressed)
                throw new InvalidOperationException(SR.GetString("LE_ToolWindowLeftMouseButtonNotDown"));

            // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
            this.OnEnteringDrag(e, location);

            // store the offset into this element in screen coordinates
            // AS 7/23/08 NA 2008 Vol 2 - ShowDialog
            //this._lastMousePoint = Utilities.PointToScreenSafe(this, Mouse.GetPosition(this));
            this._lastMousePoint = this.GetRelativeScreenPoint(e);

            this.CaptureMouse();

            if (this.IsMouseCaptured)
            {
                this.Cursor = cursor;
                ResizeFlags flags;

                switch (location)
                {
                    default:
                    case ToolWindowResizeElementLocation.Left:
                        flags = ResizeFlags.Left;
                        break;
                    case ToolWindowResizeElementLocation.Top:
                        flags = ResizeFlags.Top;
                        break;
                    case ToolWindowResizeElementLocation.Right:
                        flags = ResizeFlags.Right;
                        break;
                    case ToolWindowResizeElementLocation.Bottom:
                        flags = ResizeFlags.Bottom;
                        break;
                    case ToolWindowResizeElementLocation.TopLeft:
                        flags = ResizeFlags.TopLeft;
                        break;
                    case ToolWindowResizeElementLocation.TopRight:
                        flags = ResizeFlags.TopRight;
                        break;
                    case ToolWindowResizeElementLocation.BottomLeft:
                        flags = ResizeFlags.BottomLeft;
                        break;
                    case ToolWindowResizeElementLocation.BottomRight:
                        flags = ResizeFlags.BottomRight;
                        break;
                }

                this._resizeFlags = flags;
                this._captureAction = CaptureAction.Resize;
            }
        }

        void IToolWindowHost.Activate()
        {
            if (this.IsKeyboardFocusWithin == false)
            {
                // give preference to the focused element if there is one
                IInputElement focused = FocusManager.GetFocusedElement(this);

                if (null != focused)
                {
                    if (focused.Focus())
                        ((IToolWindowHost)this).BringToFront();
                }
                else
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }

        #region Close

        public abstract void Close();

        #endregion //Close

        public abstract void RelativePositionStateChanged();

        // AS 5/14/08 BR32842
        public abstract void BringToFront();

        // AS 10/13/08 TFS6107/BR34010
        bool IToolWindowHost.HandlesDelayedMinMaxRequests
        {
            get { return false; }
        }

        // AS 3/30/09 TFS16355 - WinForms Interop
        bool IToolWindowHost.IsWindow
        {
            get { return false; }
        }

		// AS 9/11/09 TFS21330
		public abstract bool AllowsTransparency { get; }

		// AS 9/11/09 TFS21330
		public abstract bool SupportsAllowTransparency(bool allowsTransparency);

		// AS 8/4/11 TFS83465/TFS83469
		public virtual void EnsureOnScreen(bool fullyInView)
		{
			Debug.Assert(!fullyInView, "Not implemented");
		}

		// AS 1/24/11 NA 2011 Vol 1 - Min/Max/Taskbar
		public virtual Rect GetRestoreBounds() 
		{
			return Rect.Empty;
		}

		public virtual void SetRestoreBounds(Rect restoreBounds) 
		{
		}

		public void SetWindowState(WindowState newState)
		{
			Debug.Assert(_child is ToolWindow);
			ToolWindow tw = _child as ToolWindow;

			if (tw != null)
				tw.WindowState = newState;
		}

		// AS 6/8/11 TFS76337
		Rect IToolWindowHost.GetWindowBounds()
		{
			return Rect.Empty;
		}

		// AS 11/17/11 TFS91061
		bool IToolWindowHost.IsUsingOsNonClientArea
		{
			get { return false; }
		}
        #endregion //IToolWindowHost Members

        #region CaptureAction
        private enum CaptureAction
        {
            None = 0,
            Drag,
            Resize,
        }
        #endregion //CaptureAction

        #region ResizeFlags
        [Flags()]
        internal enum ResizeFlags
        {
            Top = 0x1,
            Left = 0x2,
            Right = 0x4,
            Bottom = 0x8,
            TopLeft = Top | Left,
            TopRight = Top | Right,
            BottomLeft = Bottom | Left,
            BottomRight = Bottom | Right,
        }
        #endregion //ResizeFlags
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