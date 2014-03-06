using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using System.Windows.Interop;

namespace Infragistics.Windows.Controls
{





	internal class MousePanningAdorner : Adorner
	{
		#region Member Variables

		private ScrollViewer _scrollViewer = null;
		private double _offsetX;
		private double _offsetY;

		private const double Radius = 12.5d;

		private DispatcherTimer _timerHorz = null;
		private DispatcherTimer _timerVert = null;
		private DispatcherTimer _mouseUpTimer = null;

		private bool _canScrollVert = true;
		private bool _canScrollHorz = true;

		private ScrollCursor _currentScrollDir = ScrollCursor.None;

		private bool _hasScrolled = false;

		private Point _lastMouseMovePt = new Point(-10000, -10000);

		private ImageSource _panImage;

		private IInputElement _focusedElement;

		#endregion //Member Variables

		#region Constructor
		internal MousePanningAdorner(ScrollViewer scrollViewer, double offsetX, double offsetY)
			: base(scrollViewer)
		{
			if (null == scrollViewer)
				throw new ArgumentNullException("scrollViewer");

			this._scrollViewer = scrollViewer;
			this._offsetX = offsetX;
			this._offsetY = offsetY;

			this._canScrollHorz = scrollViewer.ViewportWidth < scrollViewer.ExtentWidth;
			this._canScrollVert = scrollViewer.ViewportHeight < scrollViewer.ExtentHeight;

			object resource;

			if (this._canScrollHorz && this._canScrollVert)
				resource = MousePanningDecorator.ScrollAllImageKey;
			else if (this._canScrollHorz)
				resource = MousePanningDecorator.ScrollEWImageKey;
			else
				resource = MousePanningDecorator.ScrollNSImageKey;

			object resourceValue = scrollViewer.TryFindResource(resource);

			Debug.Assert(resourceValue is ImageSource, "We're expecting to be able to access an image for the panning indicator!");

			this._panImage = resourceValue as ImageSource;
		}
		#endregion //Constructor

		#region Base class overrides

		#region GetDesiredTransform
		/// <summary>
		/// Returns a <see cref="GeneralTransform"/> for the adorner.
		/// </summary>
		/// <param name="transform">The transform that is currently applied to the element.</param>
		/// <returns>The transform to apply to the adorner</returns>
		public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
		{
			// center around the original offset
			GeneralTransformGroup group = new GeneralTransformGroup();
			group.Children.Add(transform);
			group.Children.Add(new TranslateTransform(this._offsetX - Radius, this._offsetY - Radius));
			return group;
		}
		#endregion //GetDesiredTransform

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			return new Size(Radius * 2, Radius * 2);
		}
		#endregion //MeasureOverride

		#region OnGotMouseCapture
		/// <summary>
		/// Invoked when the element receives mouse capture. This is used to initialize the panning operation.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnGotMouseCapture(System.Windows.Input.MouseEventArgs e)
		{
			base.OnGotMouseCapture(e);

			// start a timer so we can track the mouse position relative to us
			this.InitializeCursor(0, 0);
			this.ForceCursor = true;

			this.InitializeFocusedElement(Keyboard.FocusedElement);

			e.Handled = true;
		}
		#endregion //OnGotMouseCapture

		#region OnLostMouseCapture
		/// <summary>
		/// Invoked when the element has lost mouse capture. The panning operation will cease when capture is lost.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			DisposeTimer(ref this._timerHorz);
			DisposeTimer(ref this._timerVert);
			DisposeTimer(ref this._mouseUpTimer);

			this.InitializeFocusedElement(null);

			AdornerLayer adornerLayer = this.Parent as AdornerLayer;
			Debug.Assert(adornerLayer != null, "");

			if (null != adornerLayer)
				adornerLayer.Remove(this);

			e.Handled = true;
		}
		#endregion //OnLostMouseCapture

		#region OnMouseMove
		/// <summary>
		/// Invoked when the mouse moves over the element.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			// reset the timer
			Point pos = e.GetPosition(this.AdornedElement);
			Debug.WriteLine("MouseMove: " + pos.ToString());

			this.ProcessMouseMove(pos);

			base.OnMouseMove(e);
		}
		#endregion //OnMouseMove

		#region OnMouseDown
		/// <summary>
		/// Invoked when the mouse is pressed down on the element. The panning operation is cancelled when the next mouse down occurs.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnMouseDown(MouseButtonEventArgs e)
		{
			base.OnMouseDown(e);

			e.Handled = true;
			this.ReleaseMouseCapture();
		}
		#endregion //OnMouseDown

		#region OnMouseUp
		/// <summary>
		/// Invoked when a mouse button has been released. If a scroll operation has initiated, the panning operation will end. Otherwise, it will continue until the next mouse button is pressed.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnMouseUp(MouseButtonEventArgs e)
		{
			if (this._hasScrolled)
			{
				// if a scroll has occurred then end the panning when the 
				// mouse is released.
				this.ReleaseMouseCapture();
			}
			else
			{
				// otherwise they can still perform the panning when the 
				// middle button is released but we need to start a timer
				// because we will not get mouse messages when the mouse
				// is not over control even though we have capture

                // JJD 11/06/07 - XBAP support
                // Only create the timer if we are not in an XBAP application.
                // Otherwise a security exception is thrown in the OnMouseUpTimerTick
                // method trying to access an api in semi-trust.
                if (!BrowserInteropHelper.IsBrowserHosted)
                    this._mouseUpTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Input, new EventHandler(OnMouseUpTimerTick), this.Dispatcher);
			}

			base.OnMouseUp(e);

			e.Handled = true;
		}
		#endregion //OnMouseUp

		#region OnRender
		/// <summary>
		/// Renders the panning image for the adorner.
		/// </summary>
		/// <param name="drawingContext">The drawing instructions for the element.</param>
		protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
		{
			if (null != this._panImage)
				drawingContext.DrawImage(this._panImage, new Rect(0, 0, Radius * 2, Radius * 2));
		}
		#endregion //OnRender

		#endregion //Base class overrides

		#region Methods

		#region CalculateTimerInterval
		private double CalculateTimerInterval(double offset)
		{
			if (Math.Abs(offset) <= Radius)
				return 0;

			const double MaxOffset = 300;
			offset = Math.Min(MaxOffset - 2, Math.Abs(offset));
			return Math.Pow((MaxOffset - offset) / MaxOffset, 3d) * 500d;
		}
		#endregion //CalculateTimerInterval

		#region CanScroll
		internal static bool CanScroll(ScrollViewer scrollViewer)
		{
			return scrollViewer != null &&
				(scrollViewer.ViewportWidth < scrollViewer.ExtentWidth ||
				 scrollViewer.ViewportHeight < scrollViewer.ExtentHeight);
		}
		#endregion //CanScroll

		#region DisposeTimer
		private static void DisposeTimer(ref DispatcherTimer timer)
		{
			if (null != timer)
			{
				timer.Stop();
				timer = null;
			}
		}
		#endregion //DisposeTimer

		#region InitializeCursor
		private void InitializeCursor(double offsetX, double offsetY)
		{
			// initialize the cursor
			ScrollCursor cursorType = 0;

			if (this._canScrollVert)
			{
				if (offsetY < -Radius)
					cursorType |= ScrollCursor.ScrollUp;

				if (offsetY > Radius)
					cursorType |= ScrollCursor.ScrollDown;
			}

			if (this._canScrollVert)
			{
				if (offsetX < -Radius)
					cursorType |= ScrollCursor.ScrollLeft;

				if (offsetX > Radius)
					cursorType |= ScrollCursor.ScrollRight;
			}

			if (cursorType == 0)
			{
				if (this._canScrollVert)
					cursorType |= ScrollCursor.ScrollUpDown;

				if (this._canScrollHorz)
					cursorType |= ScrollCursor.ScrollLeftRight;
			}

			this._currentScrollDir = cursorType;

			Cursor cursor = null;

			switch (cursorType)
			{
				case ScrollCursor.ScrollDown:
					cursor = Cursors.ScrollS;
					break;
				case ScrollCursor.ScrollUp:
					cursor = Cursors.ScrollN;
					break;
				case ScrollCursor.ScrollUpDown:
					cursor = Cursors.ScrollNS;
					break;
				case ScrollCursor.ScrollLeft:
					cursor = Cursors.ScrollW;
					break;
				case ScrollCursor.ScrollRight:
					cursor = Cursors.ScrollE;
					break;
				case ScrollCursor.ScrollLeftRight:
					cursor = Cursors.ScrollWE;
					break;
				case ScrollCursor.ScrollUpLeft:
					cursor = Cursors.ScrollNW;
					break;
				case ScrollCursor.ScrollUpRight:
					cursor = Cursors.ScrollNE;
					break;
				case ScrollCursor.ScrollDownLeft:
					cursor = Cursors.ScrollSW;
					break;
				case ScrollCursor.ScrollDownRight:
					cursor = Cursors.ScrollSE;
					break;
				case ScrollCursor.ScrollAll:
					cursor = Cursors.ScrollAll;
					break;
			}

			this.Cursor = cursor;
		}
		#endregion //InitializeCursor

		#region InitializeFocusedElement
		private void InitializeFocusedElement(IInputElement focusedElement)
		{
			if (this._focusedElement != focusedElement)
			{
				if (null != this._focusedElement)
				{
					this._focusedElement.PreviewKeyDown -= new KeyEventHandler(this.OnFocusedElementPreviewKeyDown);
					this._focusedElement.LostKeyboardFocus -= new KeyboardFocusChangedEventHandler(this.OnFocusedElementLostFocus);
				}

				this._focusedElement = focusedElement;

				if (null != focusedElement)
				{
					focusedElement.PreviewKeyDown += new KeyEventHandler(this.OnFocusedElementPreviewKeyDown);
					focusedElement.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(this.OnFocusedElementLostFocus);
				}
			}
		}
		#endregion //InitializeFocusedElement

		#region InitializeTimer
		private void InitializeTimer(Orientation orientation, double offset, ref DispatcherTimer timer)
		{
			double interval = this.CalculateTimerInterval(offset);

			Debug.WriteLine(string.Format("Offset = {0}, Interval = {1}, Orientation = {2}", offset, interval, orientation));

			if (interval <= 0)
			{
				if (timer != null)
					timer.Stop();

				return;
			}

			TimeSpan intervalTimeSpan = TimeSpan.FromMilliseconds(interval);

			if (null != timer)
			{
				if (intervalTimeSpan != timer.Interval)
					timer.Interval = intervalTimeSpan;

				if (timer.IsEnabled == false)
					timer.Start();
			}
			else
				timer = new DispatcherTimer(intervalTimeSpan, DispatcherPriority.Input, new EventHandler(this.OnTimerTick), this.Dispatcher);

			return;
		}
		#endregion //InitializeTimer

		#region ProcessMouseMove
		private void ProcessMouseMove(Point mousePosition)
		{
			this._lastMouseMovePt = mousePosition;

			double offsetX = this._canScrollHorz ? mousePosition.X - this._offsetX : 0;
			double offsetY = this._canScrollVert ? mousePosition.Y - this._offsetY : 0;

			this.InitializeTimer(Orientation.Horizontal, offsetX, ref this._timerHorz);
			this.InitializeTimer(Orientation.Vertical, offsetY, ref this._timerVert);

			// if a scroll operation occurs before the mouse is released then we
			// want to end capture when the mouse button is released
			if (this._hasScrolled == false)
				this._hasScrolled = this._timerHorz != null || this._timerVert != null;

			this.InitializeCursor(offsetX, offsetY);
		}
		#endregion //ProcessMouseMove

		#region OnFocusedElementLostFocus
		private void OnFocusedElementLostFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			IInputElement newElement = this.IsMouseCaptured ? Keyboard.FocusedElement : null;
			this.InitializeFocusedElement(newElement);
		}
		#endregion //OnFocusedElementLostFocus

		#region OnFocusedElementPreviewKeyDown
		private void OnFocusedElementPreviewKeyDown(object sender, KeyEventArgs e)
		{
			// note: we're intentionally not marking this as handled
			this.ReleaseMouseCapture();
		}
		#endregion //OnFocusedElementPreviewKeyDown

		#region OnMouseUpTimerTick
		private void OnMouseUpTimerTick(object sender, EventArgs e)
		{
			// GetPosition is returning a cached position value
			// and there does not appear to be a way to force it to
			// get the correct position from the os so we have to
			// ask the os for the position and then transform it
			// relative to the adorned element as our mouse move 
			// processing does
			//
			//Point pt = Mouse.GetPosition(this.AdornedElement);
			Point pt = NativeWindowMethods.GetCursorPosApi();

			pt = this.AdornedElement.PointFromScreen(pt);

			if (pt != this._lastMouseMovePt)
				this.ProcessMouseMove(pt);
		}
		#endregion //OnMouseUpTimerTick

		#region OnTimerTick
		private void OnTimerTick(object sender, EventArgs e)
		{
			Orientation orientation = sender == this._timerHorz ? Orientation.Horizontal : Orientation.Vertical;

			// for now at least we will not be doing pixel level scrolling so we'll just use
			// the line up/down/left/right methods. if this is changed to pixel level scrolling
			// we'll probably need to watch out for using too small a value in the ScrollToVerticalOffset
			// that would not acheive a scroll (since not all controls support pixel level scrolling).
			//
			if (orientation == Orientation.Vertical)
			{
				if ((this._currentScrollDir & ScrollCursor.ScrollUpDown) == ScrollCursor.ScrollUp)
				{
					this._scrollViewer.LineUp();
				}
				else if ((this._currentScrollDir & ScrollCursor.ScrollUpDown) == ScrollCursor.ScrollDown)
				{
					this._scrollViewer.LineDown();
				}
			}
			else
			{
				if ((this._currentScrollDir & ScrollCursor.ScrollLeftRight) == ScrollCursor.ScrollLeft)
				{
					this._scrollViewer.LineLeft();
				}
				else if ((this._currentScrollDir & ScrollCursor.ScrollLeftRight) == ScrollCursor.ScrollRight)
				{
					this._scrollViewer.LineRight();
				}
			}
		}
		#endregion //OnTimerTick

		#endregion //Methods

		#region ScrollCursor enum
		private enum ScrollCursor
		{
			None = 0x0,
			ScrollUp = 0x1,
			ScrollDown = 0x2,
			ScrollLeft = 0x4,
			ScrollRight = 0x8,
			ScrollUpDown = ScrollUp | ScrollDown,
			ScrollLeftRight = ScrollLeft | ScrollRight,
			ScrollUpLeft = ScrollUp | ScrollLeft,
			ScrollUpRight = ScrollUp | ScrollRight,
			ScrollDownRight = ScrollDown | ScrollRight,
			ScrollDownLeft = ScrollDown | ScrollLeft,
			ScrollAll = ScrollUpDown | ScrollLeftRight,
		}
		#endregion //ScrollCursor enum
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