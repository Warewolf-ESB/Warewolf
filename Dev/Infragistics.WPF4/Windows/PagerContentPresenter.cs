using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using Infragistics.Shared;
using System.ComponentModel;
using Infragistics.Windows.Helpers;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Displays the contents of a <see cref="XamPager"/>
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PagerContentPresenter : ContentPresenter, IScrollInfo
	{
		#region Member Variables

		private ScrollData _scrollDataInfo;
		private IScrollInfo _scrollInfo;

		internal const double LineOffset = 16;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PagerContentPresenter"/>
		/// </summary>
		public PagerContentPresenter()
		{
		}
		#endregion //Constructor

		#region Properties

		#region Public

		#region CanContentScroll

		/// <summary>
		/// Identifies the <see cref="CanContentScroll"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanContentScrollProperty = ScrollViewer.CanContentScrollProperty.AddOwner(typeof(PagerContentPresenter), new FrameworkPropertyMetadata(new PropertyChangedCallback(PagerContentPresenter.OnCanContentScrollChanged)));

		/// <summary>
		/// Indicates if content that implements <see cref="IScrollInfo"/> should be allowed to control scrolling.
		/// </summary>
		/// <seealso cref="CanContentScrollProperty"/>
		public bool CanContentScroll
		{
			get { return (bool)this.GetValue(CanContentScrollProperty); }
			set { this.SetValue(CanContentScrollProperty, value); }
		}
		#endregion //CanContentScroll

        // JJD 2/26/08 - added
        #region ClipContentHorizontally

        /// <summary>
        /// Identifies the <see cref="ClipContentHorizontally"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ClipContentHorizontallyProperty = DependencyProperty.Register("ClipContentHorizontally",
            typeof(bool?), typeof(PagerContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Determines if this preenter should clip its content so that it doesn't extend to the left or right of the presenter.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if not set will determine how to clip based on whether it is scrolling horzontally or vertically.</para>
        /// </remarks>
        /// <seealso cref="ClipContentHorizontallyProperty"/>
        public bool? ClipContentHorizontally
        {
            get
            {
                return (bool?)this.GetValue(PagerContentPresenter.ClipContentHorizontallyProperty);
            }
            set
            {
                this.SetValue(PagerContentPresenter.ClipContentHorizontallyProperty, value);
            }
        }

        #endregion //ClipContentHorizontally

        // JJD 2/26/08 - added
        #region ClipContentVertically

        /// <summary>
        /// Identifies the <see cref="ClipContentVertically"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ClipContentVerticallyProperty = DependencyProperty.Register("ClipContentVertically",
            typeof(bool?), typeof(PagerContentPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Determines if this preenter should clip its content so that it doesn't extend to the left or right of the presenter.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> if not set will determine how to clip based on whether it is scrolling horzontally or vertically.</para>
        /// </remarks>
        /// <seealso cref="ClipContentVerticallyProperty"/>
        public bool? ClipContentVertically
        {
            get
            {
                return (bool?)this.GetValue(PagerContentPresenter.ClipContentVerticallyProperty);
            }
            set
            {
                this.SetValue(PagerContentPresenter.ClipContentVerticallyProperty, value);
            }
        }

        #endregion //ClipContentVertically

        #region ReserveSpaceForScrollButtons

        /// <summary>
        /// Identifies the <see cref="ReserveSpaceForScrollButtons"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ReserveSpaceForScrollButtonsProperty = DependencyProperty.Register("ReserveSpaceForScrollButtons",
            typeof(bool), typeof(PagerContentPresenter), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, OnReserveSpaceForScrollButtonsChanged));

        private bool _cachedReserveSpaceForScrollButtons = true;

        private static void OnReserveSpaceForScrollButtonsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            PagerContentPresenter pcp = target as PagerContentPresenter;

            if (pcp != null)
                pcp._cachedReserveSpaceForScrollButtons = (bool)e.NewValue;

        }

        /// <summary>
        /// Detrmines whether the scroll button elements are overlapping the PagerContentPresenter and therefore need to be clipped.
        /// </summary>
        /// <remarks>
        /// <para class="body">If this property is true , which is it's default value, then overlapping scroll buttons will be used to 
        /// restrict the clipping area for the content.</para></remarks>
        /// <seealso cref="ReserveSpaceForScrollButtonsProperty"/>
        public bool ReserveSpaceForScrollButtons
        {
            get
            {
                return this._cachedReserveSpaceForScrollButtons;
            }
            set
            {
                this.SetValue(PagerContentPresenter.ReserveSpaceForScrollButtonsProperty, value);
            }
        }

        #endregion //ReserveSpaceForScrollButtons

		#endregion //Public

		#region Private

		#region IsScrollClient





		private bool IsScrollClient
		{
			get
			{
				return this._scrollInfo == this;
			}
		}
		#endregion //IsScrollClient

		#region ScrollDataInfo
		private ScrollData ScrollDataInfo
		{
			get
			{
				if (this._scrollDataInfo == null)
					this._scrollDataInfo = new ScrollData();

				return this._scrollDataInfo;
			}
		}
		#endregion //ScrollDataInfo

		#endregion //Private

		#endregion //Properties

		#region Methods

		#region Private

		#region AdjustRect
		// AS 11/27/07 BR28732
		//private void AdjustRect(ref Rect renderRect, PagerScrollDirection direction)
		internal void AdjustRect(ref Rect renderRect, PagerScrollDirection direction)
		{
            // JJD 8/29/08
            // If the new ReserveSpaceForScrollButtons property is false then just return
            if (this._cachedReserveSpaceForScrollButtons == false)
                return;

			XamPager pager = this.TemplatedParent as XamPager;

			if (null != pager)
			{
				FrameworkElement btn = pager.GetScrollElement(direction);

				if (null != btn && btn.Visibility == Visibility.Visible)
				{
					GeneralTransform gt = btn.TransformToVisual(this);
					Rect btnRect = gt.TransformBounds(new Rect(0, 0, btn.ActualWidth, btn.ActualHeight));

					Rect intersection = Rect.Intersect(btnRect, renderRect);

					if (intersection.IsEmpty == false)
					{
						switch (direction)
						{
							case PagerScrollDirection.Left:
								if (intersection.Width > 0)
								{
									renderRect.Width -= intersection.Width;
									renderRect.X = intersection.Right;
								}
								break;
							case PagerScrollDirection.Right:
								if (intersection.Width > 0)
									renderRect.Width -= intersection.Width;
								break;
							case PagerScrollDirection.Up:
								if (intersection.Height > 0)
								{
									renderRect.Height -= intersection.Height;
									renderRect.Y = intersection.Bottom;
								}
								break;
							case PagerScrollDirection.Down:
								if (intersection.Height > 0)
									renderRect.Height -= intersection.Height;
								break;
						}
					}
				}
			}

		}
		#endregion //AdjustRect

		#region EnsureIsANumber
		internal static void EnsureIsANumber(double number)
		{
			if (double.IsNaN(number))
                throw new ArgumentOutOfRangeException("offset", number, SR.GetString("LE_ValueCannotBeNan"));
		}
		#endregion //EnsureIsANumber

		#region OnCanContentScrollChanged
		private static void OnCanContentScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			PagerContentPresenter contentPreseter = d as PagerContentPresenter;

			if (null != contentPreseter && contentPreseter._scrollDataInfo != null)
			{
				contentPreseter.VerifyScrollInfo();
				contentPreseter.InvalidateMeasure();
			}
		}
		#endregion //OnCanContentScrollChanged

		#region VerifyScrollInfo
		private void VerifyScrollInfo()
		{
			XamPager scrollViewer = this.TemplatedParent as XamPager;

			if (null != scrollViewer)
			{
				IScrollInfo info = null;

				// if the contained control can control the scrolling...
				if (this.CanContentScroll)
				{
					info = this.Content as IScrollInfo;

					if (null == info)
					{
						ItemsPresenter itemsPresenter = this.Content as ItemsPresenter;

						if (null != itemsPresenter)
						{
							itemsPresenter.ApplyTemplate();
							if (VisualTreeHelper.GetChildrenCount(itemsPresenter) > 0)
								info = VisualTreeHelper.GetChild(itemsPresenter, 0) as IScrollInfo;
						}
					}
				}

				// if the content can't scroll or we don't contain an IScrollInfo
				// then we'll do the scrolling
				if (info == null)
					info = this;

				// if the scroll info is changing and we were
				// previously associated with an iscrollinfo...
				if (info != this._scrollInfo)
				{
					// if it was us, then release the scroll data
					if (this.IsScrollClient)
						this._scrollInfo = null;
					else if (null != this._scrollInfo && this._scrollInfo.ScrollOwner == scrollViewer)
						this._scrollInfo.ScrollOwner = null;
				}

				if (info != null)
				{
					// store the scroll info so we know whether we are scrolling
					// or whether its the contained element - also so we can
					// clean up later
					this._scrollInfo = info;

					// let the object doing the scrolling have a reference to the
					// scroll viewer
					info.ScrollOwner = scrollViewer;

					// and let the scroll view have a reference to the object 
					// doing the scrolling
					scrollViewer.ScrollInfoInternal = info;
				}
			}
			else if (this._scrollInfo != null)
			{
				// otherwise clean up the scrollinfo
				// and its previously associated scroll viewer
				scrollViewer = this._scrollInfo.ScrollOwner as XamPager;

				if (scrollViewer != null && scrollViewer.ScrollInfoInternal == this._scrollInfo)
					scrollViewer.ScrollInfoInternal = null;

				this._scrollInfo.ScrollOwner = null;
				this._scrollInfo = null;
				this._scrollDataInfo = null;
			}
		}
		#endregion //VerifyScrollInfo

		#endregion //Private

		#region Internal

		#region AreClose
		internal static bool AreClose(double value1, double value2)
		{
			if (value1 == value2)
				return true;

			return Math.Abs(value1 - value2) < .0000000001;
		}
		#endregion //AreClose

		#region IsDescendantInView
		internal bool IsDescendantInView(DependencyObject d)
		{
			bool isInView = false;
			UIElement element = d as UIElement;

			if (element == null && d is ContentElement)
				element = Utilities.GetAncestorFromType(d, typeof(UIElement), true) as UIElement;

			if (element != null && element.IsDescendantOf(this))
			{
				// AS 1/6/10 TFS25834
				//Rect visualRect = element.TransformToAncestor(this).TransformBounds(new Rect(element.RenderSize));
				GeneralTransform gt = element.TransformToAncestor(this);
				Rect visualRect = gt.TransformBounds(new Rect(element.RenderSize));

				Rect availableRect = new Rect(this.RenderSize);

				this.AdjustRect(ref availableRect, PagerScrollDirection.Left);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Right);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Up);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Down);

				isInView = Rect.Intersect(visualRect, availableRect).Size.IsEmpty == false;
			}

			return isInView;
		}
		#endregion //IsDescendantInView

		#endregion //Internal

		#endregion //Methods

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			if (this.IsScrollClient)
				this.ScrollDataInfo.VerifyScrollData(finalSize, this.ScrollDataInfo._extent);

			UIElement child = this.VisualChildrenCount > 0 ? this.GetVisualChild(0) as UIElement : null;

			if (null != child)
			{
				Rect rect = new Rect(child.DesiredSize);

				// if we're doing the scroling then offset the child
				if (this.IsScrollClient)
					rect.Location = new Point(-this.ScrollDataInfo._offset.X, -this.ScrollDataInfo._offset.Y);

				// the available size might be more than the element wants...
				if (finalSize.Width > rect.Width)
					rect.Width = finalSize.Width;
				if (finalSize.Height > rect.Height)
					rect.Height = finalSize.Height;

				child.Arrange(rect);
			}

			return finalSize;
		}
		#endregion //ArrangeOverride

		#region GetLayoutClip
		/// <summary>
		/// Returns a geometry for the clipping mask for the element.
		/// </summary>
		/// <param name="layoutSlotSize">The size of the element</param>
		/// <returns>A geometry to clip in the scrolling direction(s)</returns>
		protected override Geometry GetLayoutClip(Size layoutSlotSize)
		{
			Geometry geometry;

			// AS 10/24/07
			// We should clip even if we are hosting the IScrollInfo implementation.
			//
			//if (this.CanHorizontallyScroll || this.CanVerticallyScroll)
			IScrollInfo scrollInfo = this._scrollInfo;

			bool canHorizontallyScroll = scrollInfo != null && scrollInfo.CanHorizontallyScroll;
			bool canVerticallyScroll = scrollInfo != null && scrollInfo.CanVerticallyScroll;

			if (canHorizontallyScroll || canVerticallyScroll)
			{
				const double ExtraClipSpace = 100;

				Rect renderRect = new Rect(this.RenderSize);

                // JJD 2/26/08 
                // use new ClipContentVertically property
				// increase the clipping vertically outside out
				// bounds if we're not scrolling vertically
                //if (canVerticallyScroll == false ||
                //    scrollInfo.ViewportHeight >= scrollInfo.ExtentHeight ||
                //    AreClose(scrollInfo.ViewportHeight, scrollInfo.ExtentHeight) ||
                bool adjustRectVertically;

                bool? clipContentVertically = this.ClipContentVertically;

                if (clipContentVertically.HasValue)
                    adjustRectVertically = clipContentVertically.Value;
                else
                    adjustRectVertically = !(canVerticallyScroll == false ||
					    scrollInfo.ViewportHeight >= scrollInfo.ExtentHeight ||
					    AreClose(scrollInfo.ViewportHeight, scrollInfo.ExtentHeight));

				if ( adjustRectVertically == false)
				{
					renderRect.Y -= ExtraClipSpace;
					renderRect.Height += ExtraClipSpace * 2;
				}
				else
				{
					this.AdjustRect(ref renderRect, PagerScrollDirection.Up);
					this.AdjustRect(ref renderRect, PagerScrollDirection.Down);
				}
                
                // JJD 2/26/08 
                // use new ClipContentHorizontally property
				// increase the clipping horizontally outside out
				// bounds if we're not scrolling vertically
                //if (canHorizontallyScroll == false ||
                //    scrollInfo.ViewportWidth >= scrollInfo.ExtentWidth ||
                //    AreClose(scrollInfo.ViewportWidth, scrollInfo.ExtentWidth))

                bool adjustRectHorizontally;

                bool? clipContentHorizontally = this.ClipContentHorizontally;

                if (clipContentHorizontally.HasValue)
                    adjustRectHorizontally = clipContentHorizontally.Value;
                else
                    adjustRectHorizontally = !(canHorizontallyScroll == false ||
                        scrollInfo.ViewportWidth >= scrollInfo.ExtentWidth ||
                        AreClose(scrollInfo.ViewportWidth, scrollInfo.ExtentWidth));

				if ( adjustRectHorizontally == false)
				{
					renderRect.X -= ExtraClipSpace;
					renderRect.Width += ExtraClipSpace * 2;
				}
				else
				{
					this.AdjustRect(ref renderRect, PagerScrollDirection.Left);
					this.AdjustRect(ref renderRect, PagerScrollDirection.Right);
				}

				geometry = new RectangleGeometry(renderRect);

                // JJD 4/29/10 - Optimization
                // Freeze the clip geometry so the framework doesn't need to listen for changes
                geometry.Freeze();
			}
			else
				geometry = null;

			return geometry;
		}
		#endregion //GetLayoutClip

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			Size desiredSize = Size.Empty;

			if (this.VisualChildrenCount > 0)
			{
				Size measureSize = availableSize;

				if (this.IsScrollClient)
				{
					if (this.ScrollDataInfo._canHorizontallyScroll)
						measureSize.Width = double.PositiveInfinity;
					if (this.ScrollDataInfo._canVerticallyScroll)
						measureSize.Height = double.PositiveInfinity;
				}

				desiredSize = base.MeasureOverride(measureSize);
			}

			// verify the scroll extent
			if (this.IsScrollClient)
				this.ScrollDataInfo.VerifyScrollData(availableSize, desiredSize);

			// if we need more room than we are offered, just indicate that we
			// need the space returned since we will be scrolling the rest
			if (availableSize.Width < desiredSize.Width)
				desiredSize.Width = availableSize.Width;
			if (availableSize.Height < desiredSize.Height)
				desiredSize.Height = availableSize.Height;

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template has been applied to the element.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this.VerifyScrollInfo();
		}
		#endregion //OnApplyTemplate

		#endregion //Base class overrides

		#region IScrollInfo Members

		/// <summary>
		/// Returns or sets a boolean indicating scrolling along the horizontal axis is possible.
		/// </summary>
		public bool CanHorizontallyScroll
		{
			get
			{
				if (this.IsScrollClient)
					return this.ScrollDataInfo._canHorizontallyScroll;

				return false;
			}
			set
			{
				if (this.IsScrollClient && this.ScrollDataInfo._canHorizontallyScroll != value)
				{
					this.ScrollDataInfo._canHorizontallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		/// <summary>
		/// Returns or sets a boolean indicating scrolling along the vertical axis is possible.
		/// </summary>
		public bool CanVerticallyScroll
		{
			get
			{
				if (this.IsScrollClient)
					return this.ScrollDataInfo._canVerticallyScroll;

				return false;
			}
			set
			{
				if (this.IsScrollClient && this.ScrollDataInfo._canVerticallyScroll != value)
				{
					this.ScrollDataInfo._canVerticallyScroll = value;
					this.InvalidateMeasure();
				}
			}
		}

		/// <summary>
		/// Returns the height of the area to be scrolled.
		/// </summary>
		public double ExtentHeight
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Height : 0; }
		}

		/// <summary>
		/// Returns the width of the area to be scrolled.
		/// </summary>
		public double ExtentWidth
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Width : 0; }
		}

		/// <summary>
		/// Returns the current offset of the scrolled content along the horizontal axis.
		/// </summary>
		public double HorizontalOffset
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._offset.X : 0; }
		}

		/// <summary>
		/// Increases the <see cref="VerticalOffset"/> by a small amount.
		/// </summary>
		public void LineDown()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y + LineOffset);
		}

		/// <summary>
		/// Decreases the <see cref="HorizontalOffset"/> by a small amount.
		/// </summary>
		public void LineLeft()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X - LineOffset);
		}

		/// <summary>
		/// Increases the <see cref="HorizontalOffset"/> by a small amount.
		/// </summary>
		public void LineRight()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X + LineOffset);
		}

		/// <summary>
		/// Decreases the <see cref="VerticalOffset"/> by a small amount.
		/// </summary>
		public void LineUp()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y - LineOffset);
		}

		/// <summary>
		/// Scrolls the content such that the specified parent is in view.
		/// </summary>
		/// <param name="visual">The element to bring into view.</param>
		/// <param name="rectangle">The portion of the parent to bring into view.</param>
		/// <returns></returns>
		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			if (rectangle.IsEmpty || visual == null || this.IsAncestorOf(visual) == false)
				return Rect.Empty;

			// AS 1/6/10 TFS25834
			//Rect visualRect = visual.TransformToAncestor(this).TransformBounds(rectangle);
			GeneralTransform gt = visual.TransformToAncestor(this);

			if (gt == null)
				return Rect.Empty;

			Rect visualRect = gt.TransformBounds(rectangle);

			if (this.IsScrollClient)
			{
				Rect availableRect = new Rect(this.RenderSize);

				this.AdjustRect(ref availableRect, PagerScrollDirection.Left);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Right);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Up);
				this.AdjustRect(ref availableRect, PagerScrollDirection.Down);

				Rect intersection = Rect.Intersect(visualRect, availableRect);

				if (intersection.Width != visualRect.Width)
				{
					double offsetX = 0;

					// try to get the right side in view
					if (visualRect.Right > availableRect.Right)
						offsetX = visualRect.Right - availableRect.Right;

					// make sure that the left side is in view
					if (visualRect.Left - offsetX - availableRect.Left < 0)
						offsetX += visualRect.Left - offsetX - availableRect.Left;

					visualRect.X -= offsetX;

					offsetX += this.HorizontalOffset;

					this.SetHorizontalOffset(offsetX);
				}

				if (intersection.Height != visualRect.Height)
				{
					double offsetY = 0;

					// try to get the right side in view
					if (visualRect.Bottom > availableRect.Bottom)
						offsetY = visualRect.Bottom - availableRect.Bottom;

					// make sure that the left side is in view
					if (visualRect.Top - offsetY - availableRect.Top < 0)
						offsetY += visualRect.Top - offsetY - availableRect.Top;

					visualRect.Y -= offsetY;

					offsetY += this.VerticalOffset;

					this.SetVerticalOffset(offsetY);
				}

			}

			return visualRect;
		}

		/// <summary>
		/// Increases the <see cref="VerticalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelDown()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y + (SystemParameters.WheelScrollLines * LineOffset));
		}

		/// <summary>
		/// Decreases the <see cref="HorizontalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelLeft()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X - (SystemParameters.WheelScrollLines * LineOffset));
		}

		/// <summary>
		/// Increases the <see cref="HorizontalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelRight()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X + (SystemParameters.WheelScrollLines * LineOffset));
		}

		/// <summary>
		/// Decreases the <see cref="VerticalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelUp()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y - (SystemParameters.WheelScrollLines * LineOffset));
		}

		/// <summary>
		/// Increases the <see cref="VerticalOffset"/> based on the current <see cref="ViewportHeight"/>
		/// </summary>
		public void PageDown()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y + this.ScrollDataInfo._viewport.Height);
		}

		/// <summary>
		/// Decreases the <see cref="HorizontalOffset"/> based on the current <see cref="ViewportWidth"/>
		/// </summary>
		public void PageLeft()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.X - this.ScrollDataInfo._viewport.Width);
		}

		/// <summary>
		/// Increases the <see cref="HorizontalOffset"/> based on the current <see cref="ViewportWidth"/>
		/// </summary>
		public void PageRight()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.X + this.ScrollDataInfo._viewport.Width);
		}

		/// <summary>
		/// Decreases the <see cref="VerticalOffset"/> based on the current <see cref="ViewportHeight"/>
		/// </summary>
		public void PageUp()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y - this.ScrollDataInfo._viewport.Height);
		}

		/// <summary>
		/// Returns or sets the <see cref="ScrollViewer"/> that is controlling the scrolling behavior.
		/// </summary>
		public ScrollViewer ScrollOwner
		{
			get
			{
				if (this.IsScrollClient)
					return this.ScrollDataInfo._scrollOwner;

				return null;
			}
			set
			{
				if (this.IsScrollClient)
					this.ScrollDataInfo._scrollOwner = value;
			}
		}

		/// <summary>
		/// Changes the <see cref="HorizontalOffset"/>
		/// </summary>
		/// <param name="offset">The new horizontal scroll offset</param>
		public void SetHorizontalOffset(double offset)
		{
			if (this.IsScrollClient)
			{
				EnsureIsANumber(offset);
				offset = Math.Max(offset, 0);

				if (false == AreClose(offset, this.ScrollDataInfo._offset.X))
				{
					this.ScrollDataInfo._offset.X = offset;

					// AS 2/3/10 TFS26287
					if (null != this.ScrollDataInfo._scrollOwner)
						this.ScrollDataInfo._scrollOwner.InvalidateScrollInfo();

					this.InvalidateArrange();
				}
			}
		}

		/// <summary>
		/// Changes the <see cref="VerticalOffset"/>
		/// </summary>
		/// <param name="offset">The new vertical scroll offset</param>
		public void SetVerticalOffset(double offset)
		{
			if (this.IsScrollClient)
			{
				EnsureIsANumber(offset);
				offset = Math.Max(offset, 0);

				if (AreClose(offset, this.ScrollDataInfo._offset.Y) == false)
				{
					this.ScrollDataInfo._offset.Y = offset;

					// AS 2/3/10 TFS26287
					if (null != this.ScrollDataInfo._scrollOwner)
						this.ScrollDataInfo._scrollOwner.InvalidateScrollInfo();

					this.InvalidateArrange();
				}
			}
		}

		/// <summary>
		/// Returns the current offset of the scrolled content along the vertical axis.
		/// </summary>
		public double VerticalOffset
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._offset.Y : 0; }
		}

		/// <summary>
		/// Returns the width of the available area for the scrolled content.
		/// </summary>
		public double ViewportHeight
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Height : 0; }
		}

		/// <summary>
		/// Returns the width of the available area for the scrolled content.
		/// </summary>
		public double ViewportWidth
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Width : 0; }
		}

		#endregion //IScrollInfo members

		#region ScrollData class
		internal class ScrollData
		{
			#region Member Variables

			internal ScrollViewer _scrollOwner = null;
			internal Size _extent = new Size();
			internal Size _viewport = new Size();
			internal Vector _offset = new Vector();
			internal bool _canHorizontallyScroll = false;
			internal bool _canVerticallyScroll = false;

			#endregion //Member Variables

			#region Methods

			#region Reset

			internal void Reset()
			{
				this._offset = new Vector();
				this._extent = new Size();
				this._viewport = new Size();
			}

			#endregion //Reset

			#region VerifyScrollData
			internal void VerifyScrollData(Size viewPort, Size extent)
			{
				// if we have endless space use the space we need
				if (double.IsInfinity(viewPort.Width))
					viewPort.Width = extent.Width;
				if (double.IsInfinity(viewPort.Height))
					viewPort.Height = extent.Height;

				bool isDifferent = false == PagerContentPresenter.AreClose(this._viewport.Width, viewPort.Width) ||
					false == PagerContentPresenter.AreClose(this._viewport.Height, viewPort.Height) ||
					false == PagerContentPresenter.AreClose(this._extent.Width, extent.Width) ||
					false == PagerContentPresenter.AreClose(this._extent.Width, extent.Width);

				this._viewport = viewPort;
				this._extent = extent;

				isDifferent |= this.VerifyOffset();

				// dirty the scroll viewer if something has changed
				if (null != this._scrollOwner && isDifferent)
				{
					this._scrollOwner.InvalidateScrollInfo();
				}
			}
			#endregion //VerifyScrollData

			#region VerifyOffset
			private bool VerifyOffset()
			{
				double offsetX = Math.Max(Math.Min(this._offset.X, this._extent.Width - this._viewport.Width), 0);
				double offsetY = Math.Max(Math.Min(this._offset.Y, this._extent.Height - this._viewport.Height), 0);
				Vector oldOffset = this._offset;
				this._offset = new Vector(offsetX, offsetY);

				// return true if the offset has changed
				return false == PagerContentPresenter.AreClose(this._offset.X, oldOffset.X) ||
					false == PagerContentPresenter.AreClose(this._offset.Y, oldOffset.Y);
			}
			#endregion //VerifyOffset

			#endregion //Methods
		}
		#endregion //ScrollData class
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