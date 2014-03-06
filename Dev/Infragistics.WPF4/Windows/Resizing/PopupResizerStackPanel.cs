using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Custom stack panel implementation that supports items being made larger when hosted within a PopupResizeDecorator.
	/// </summary>
	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class PopupResizerStackPanel : Panel
		, IScrollInfo
	{
		#region Member Variables

		private PagerContentPresenter.ScrollData _scrollDataInfo; 
		
		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="PopupResizerStackPanel"/>
		/// </summary>
		public PopupResizerStackPanel()
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride

		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override Size ArrangeOverride(Size finalSize)
		{
			#region Setup

			List<ResizableItemInfo> resizableItemList = new List<ResizableItemInfo>();
			UIElementCollection elements = this.InternalChildren;
			double fixedExtent = 0;
			bool isVertical = this.Orientation == Orientation.Vertical;

			#endregion //Setup

			#region Get Resizable Elements
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement element = elements[i];

				if (element == null || element.Visibility == Visibility.Collapsed)
					continue;

				bool isResizable = element is FrameworkElement && GetIsResizable(element);

				// in the future if we want to support this we will likely need to create a grid bag manager 
				// and have it provide the resize information
				Debug.Assert(!isResizable || resizableItemList.Count == 0, "Currently only supporting 1 resizable item");
				if (isResizable && resizableItemList.Count == 1)
					isResizable = false;

				var resizeConstraints = !isResizable ? null : PopupResizerDecorator.GetResizeConstraints(element as FrameworkElement);

				if (isResizable)
				{
					resizableItemList.Add(new ResizableItemInfo { Index = i, ResizeConstraints = resizeConstraints });
				}
				else
				{
					Size size = element.DesiredSize;
					fixedExtent += isVertical ? size.Height : size.Width;
				}
			}
			#endregion //Get Resizable Elements

			double availableExtent = isVertical ? finalSize.Height : finalSize.Width;
			int nextResizableIndex = elements.Count;

			#region Calculate Resizable Extents
			if (resizableItemList.Count > 0 && availableExtent > fixedExtent)
			{
				resizableItemList[0].Extent = availableExtent - fixedExtent;

				nextResizableIndex = resizableItemList[0].Index;
			}
			#endregion //Calculate Resizable Extents

			#region Arrange Children
			Rect arrangeRect = new Rect(finalSize);
			int resizableItemIndex = 0;
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement element = elements[i];

				if (element == null || element.Visibility == Visibility.Collapsed)
					continue;

				if (i == nextResizableIndex)
				{
					if (isVertical)
						arrangeRect.Height = resizableItemList[resizableItemIndex].Extent;
					else
						arrangeRect.Width = resizableItemList[resizableItemIndex].Extent;

					resizableItemIndex++;
					nextResizableIndex = resizableItemIndex < resizableItemList.Count ? resizableItemList[resizableItemIndex].Index : elements.Count;
				}
				else
				{
					if (isVertical)
						arrangeRect.Height = element.DesiredSize.Height;
					else
						arrangeRect.Width = element.DesiredSize.Width;
				}

				element.Arrange(arrangeRect);

				if (isVertical)
					arrangeRect.Y += arrangeRect.Height;
				else
					arrangeRect.X += arrangeRect.Width;
			}
			#endregion //Arrange Children

			// if the items were bigger than the available area then allow the implicit clipping
			if (isVertical && arrangeRect.Y > finalSize.Height)
				finalSize.Height = arrangeRect.Y;
			else if (!isVertical && arrangeRect.X > finalSize.Width)
				finalSize.Width = arrangeRect.X;

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			#region Setup

			UIElementCollection elements = this.InternalChildren;
			Size fixedElementMeasureSize = availableSize;
			bool isVertical = this.Orientation == Orientation.Vertical;
			double availableScrollExtent = isVertical ? availableSize.Height : availableSize.Width;
			double availableOtherExtent = isVertical ? availableSize.Width : availableSize.Height;

			if (isVertical)
				fixedElementMeasureSize.Height = double.PositiveInfinity;
			else
				fixedElementMeasureSize.Width = double.PositiveInfinity;

			double largestOtherExtent = 0;
			double extentUsed = 0;
			List<ResizableItemInfo> resizableItemList = new List<ResizableItemInfo>();

			#endregion //Setup

			#region Measure NonResizable Elements
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement element = elements[i];

				if (element == null || element.Visibility == Visibility.Collapsed)
					continue;

				FrameworkElement fe = element as FrameworkElement;
				bool isResizable = fe != null && GetIsResizable(fe);
				PopupResizerDecorator.Constraints resizeConstraints = null;

				if (isResizable)
				{
					// in the future if we want to support this we will likely need to create a grid bag manager 
					// and have it provide the resize information
					Debug.Assert(!isResizable || resizableItemList.Count == 0, "Currently only supporting 1 resizable item");
					if (isResizable && resizableItemList.Count == 1)
						isResizable = false;
					else
					{
						resizeConstraints = PopupResizerDecorator.GetResizeConstraints(fe);

						// for the first measure we will measure with the same constraints used for other items 
						// to get the preferred size and then cache that as the minimum size
						if (resizeConstraints == null)
						{
							element.Measure(fixedElementMeasureSize);

							resizeConstraints = PopupResizerDecorator.GetResizeConstraints(fe);

							if (resizeConstraints == null)
							{
								resizeConstraints = new ItemConstraints();
								PopupResizerDecorator.SetResizeConstraints(fe, resizeConstraints);
							}

							resizeConstraints.MinimumWidth = element.DesiredSize.Width;
							resizeConstraints.MinimumHeight = element.DesiredSize.Height;
						}

						largestOtherExtent = Math.Max(isVertical ? resizeConstraints.MinimumWidth : resizeConstraints.MinimumHeight, largestOtherExtent);
					}
				}

				if (resizeConstraints != null)
				{
					resizableItemList.Add(new ResizableItemInfo { Index = i, ResizeConstraints = resizeConstraints });
				}
				else
				{
					element.Measure(fixedElementMeasureSize);
					Size elementDesiredSize = element.DesiredSize;

					largestOtherExtent = Math.Max(isVertical ? elementDesiredSize.Width : elementDesiredSize.Height, largestOtherExtent);
					extentUsed += isVertical ? elementDesiredSize.Height : elementDesiredSize.Width;
				}
			}
			#endregion //Measure NonResizable Elements

			#region Measure Resizable Elements
			int resizableItemCount = resizableItemList.Count;

			if (resizableItemCount > 0)
			{
				bool isInfiniteScrollExtent = double.IsInfinity(availableScrollExtent);
				bool isInfiniteOtherExtent = double.IsInfinity(availableOtherExtent);
				double scrollableRemaining = Math.Max(availableScrollExtent - extentUsed, 0);
				double otherExtent = isInfiniteOtherExtent
					? largestOtherExtent
					: availableOtherExtent;
				Size resizableMeasureSize = isVertical
					? new Size(otherExtent, double.PositiveInfinity)
					: new Size(double.PositiveInfinity, otherExtent);

				for (int i = 0; i < resizableItemCount; i++)
				{
					var item = resizableItemList[i];
					double extent;

					if (isInfiniteScrollExtent)
					{
						extent = isVertical
							? item.ResizeConstraints.MinimumHeight
							: item.ResizeConstraints.MinimumWidth;
					}
					else
					{
						extent = scrollableRemaining / (resizableItemCount - i);
						scrollableRemaining -= extent;

						if (isVertical)
							resizableMeasureSize.Height = extent;
						else
							resizableMeasureSize.Width = extent;
					}

					// add in the amount we need
					extentUsed += extent;

					// make sure the element is measured with the appropriate size
					elements[item.Index].Measure(resizableMeasureSize);
				}
			} 
			#endregion //Measure Resizable Elements

			Size desiredSize;

			if (isVertical)
				desiredSize = new Size(largestOtherExtent, extentUsed);
			else
				desiredSize = new Size(extentUsed, largestOtherExtent);

			return desiredSize;
		}

		#endregion //MeasureOverride

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region IsResizable

		/// <summary>
		/// IsResizable Attached Dependency Property
		/// </summary>
		public static readonly DependencyProperty IsResizableProperty =
			DependencyProperty.RegisterAttached("IsResizable", typeof(bool), typeof(PopupResizerStackPanel),
				new FrameworkPropertyMetadata((bool)false,
					FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		/// <summary>
		/// Gets the IsResizable property.  This dependency property determines if the direct child of the panel is resizable.
		/// </summary>
		[AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
		public static bool GetIsResizable(DependencyObject d)
		{
			return (bool)d.GetValue(IsResizableProperty);
		}

		/// <summary>
		/// Sets the IsResizable property.  This dependency property indicates if the direct child of the panel is resizable
		/// </summary>
		public static void SetIsResizable(DependencyObject d, bool value)
		{
			d.SetValue(IsResizableProperty, value);
		}

		#endregion // IsResizable

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation",
			typeof(Orientation), typeof(PopupResizerStackPanel), new FrameworkPropertyMetadata(KnownBoxes.OrientationVerticalBox, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// Returns or sets the orientation of the items.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		[Description("Returns or sets the orientation of the items.")]
		[Category("Behavior")]
		[Bindable(true)]
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(PopupResizerStackPanel.OrientationProperty);
			}
			set
			{
				this.SetValue(PopupResizerStackPanel.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		#endregion //Public Properties
		
		#region Private Properties

		#region IsScrollClient
		private bool IsScrollClient
		{
			get { return _scrollDataInfo != null && _scrollDataInfo._scrollOwner != null; }
		}
		#endregion //IsScrollClient

		#region ScrollDataInfo
		private PagerContentPresenter.ScrollData ScrollDataInfo
		{
			get
			{
				if (this._scrollDataInfo == null)
					this._scrollDataInfo = new PagerContentPresenter.ScrollData();

				return this._scrollDataInfo;
			}
		}
		#endregion //ScrollDataInfo

		#endregion //Private Properties
		
		#endregion //Properties

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
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y + PagerContentPresenter.LineOffset);
		}

		/// <summary>
		/// Decreases the <see cref="HorizontalOffset"/> by a small amount.
		/// </summary>
		public void LineLeft()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X - PagerContentPresenter.LineOffset);
		}

		/// <summary>
		/// Increases the <see cref="HorizontalOffset"/> by a small amount.
		/// </summary>
		public void LineRight()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X + PagerContentPresenter.LineOffset);
		}

		/// <summary>
		/// Decreases the <see cref="VerticalOffset"/> by a small amount.
		/// </summary>
		public void LineUp()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y - PagerContentPresenter.LineOffset);
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
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y + (SystemParameters.WheelScrollLines * PagerContentPresenter.LineOffset));
		}

		/// <summary>
		/// Decreases the <see cref="HorizontalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelLeft()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X - (SystemParameters.WheelScrollLines * PagerContentPresenter.LineOffset));
		}

		/// <summary>
		/// Increases the <see cref="HorizontalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelRight()
		{
			if (this.IsScrollClient)
				this.SetHorizontalOffset(this.ScrollDataInfo._offset.X + (SystemParameters.WheelScrollLines * PagerContentPresenter.LineOffset));
		}

		/// <summary>
		/// Decreases the <see cref="VerticalOffset"/> when the end user scrolls with the mouse wheel
		/// </summary>
		public void MouseWheelUp()
		{
			if (this.IsScrollClient)
				this.SetVerticalOffset(this.ScrollDataInfo._offset.Y - (SystemParameters.WheelScrollLines * PagerContentPresenter.LineOffset));
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
				PagerContentPresenter.EnsureIsANumber(offset);
				offset = Math.Max(offset, 0);

				if (false == Utilities.AreClose(offset, this.ScrollDataInfo._offset.X))
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
				PagerContentPresenter.EnsureIsANumber(offset);
				offset = Math.Max(offset, 0);

				if (CoreUtilities.AreClose(offset, this.ScrollDataInfo._offset.Y) == false)
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

		#region ResizableItemInfo class
		private class ResizableItemInfo
		{
			public int Index;
			public double Extent;
			public PopupResizerDecorator.Constraints ResizeConstraints;
		}
		#endregion //ResizableItemInfo class

		#region ItemConstraints class
		private class ItemConstraints : PopupResizerDecorator.Constraints
		{
			internal override void OnElementChanged(FrameworkElement oldElement, FrameworkElement newElement)
			{
				if (newElement == null && oldElement != null)
				{
					var panel = VisualTreeHelper.GetParent(oldElement) as PopupResizerStackPanel;

					if (null != panel)
						panel.InvalidateMeasure();
				}
			}
		} 
		#endregion //ItemConstraints class
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