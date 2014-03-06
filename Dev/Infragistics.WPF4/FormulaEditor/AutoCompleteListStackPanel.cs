using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;





namespace Infragistics.Controls.Interactions.Primitives
{
	/// <summary>
	/// A panel which displays items in the <see cref="AutoCompleteList"/>.
	/// </summary>
	public class AutoCompleteListStackPanel : Panel, IScrollInfo
	{
		#region Member Variables

		private ScrollInfo _scrollInfo;

		#endregion  // Member Variables

		#region Base Class Overrides

		#region ArrangeOverride

		/// <summary>
		/// Arranges the children of the panel.
		/// </summary>
		/// <param name="arrangeSize">The size the panel should use to arrange its children.</param>
		/// <returns>The arranged size of the panel and its children.</returns>
		protected override Size ArrangeOverride(Size arrangeSize)
		{
			Rect elementRect = new Rect(new Point(), arrangeSize);

			if (this.IsScrollingEnabled)
			{
				elementRect.X = -_scrollInfo._calculatedOffsetPixelsX;
				elementRect.Y = this.GetPhysicalOffset(_scrollInfo._calculatedOffsetItemsY);
			}

			UIElementCollection children = this.Children;
			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				UIElement element = children[i];
				if (element == null)
					continue;

				elementRect.Height = element.DesiredSize.Height;
				elementRect.Width = Math.Max(arrangeSize.Width, element.DesiredSize.Width);

				element.Arrange(elementRect);

				elementRect.Y += elementRect.Height;
			}

			return arrangeSize;
		}

		#endregion  // ArrangeOverride


		#region HasLogicalOrientation

		/// <summary>
		/// Gets the value indicating whether the panel lays out its children in a single dimension.
		/// </summary>
		protected override bool HasLogicalOrientation
		{
			get
			{
				return true;
			}
		}

		#endregion  // HasLogicalOrientation

		#region LogicalOrientation

		/// <summary>
		/// Gets the value indicating in which dimension the panels lays out its children.
		/// </summary>
		protected override Orientation LogicalOrientation
		{
			get
			{
				return Orientation.Vertical;
			}
		}

		#endregion  // LogicalOrientation


		#region MeasureOverride

		/// <summary>
		/// Measures the element and its children.
		/// </summary>
		/// <param name="constraint">The size available to the panel and its children.</param>
		/// <returns>The desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			Size availableElementSize = new Size(constraint.Width, double.PositiveInfinity);
			Size totalSize = new Size();

			UIElementCollection children = this.Children;

			bool isScrollingEnabled = this.IsScrollingEnabled;
			int firstVisibleItemIndex = isScrollingEnabled
				? AutoCompleteListStackPanel.OffsetToItemIndex(_scrollInfo._offsetItemsY, children.Count)
				: 0;
			int lastVisibleItemIndex = -1;

			double viewportHeightRemaining = constraint.Height;

			int count = children.Count;
			for (int i = 0; i < count; i++)
			{
				UIElement element = children[i];
				if (element == null)
					continue;

				element.Measure(availableElementSize);

				Size desiredSize = element.DesiredSize;
				totalSize.Width = Math.Max(totalSize.Width, desiredSize.Width);
				totalSize.Height += desiredSize.Height;

				if (isScrollingEnabled && lastVisibleItemIndex == -1 && firstVisibleItemIndex <= i)
				{
					viewportHeightRemaining -= desiredSize.Height;

					if (CoreUtilities.LessThanOrClose(viewportHeightRemaining, 0.0))
						lastVisibleItemIndex = i;
				}
			}

			if (isScrollingEnabled == false)
				return totalSize;

			if (lastVisibleItemIndex == -1)
				lastVisibleItemIndex = children.Count - 1;

			while (firstVisibleItemIndex > 0)
			{
				UIElement child = children[firstVisibleItemIndex - 1];
				double childHeight = child != null ? child.DesiredSize.Height : 0;
				double heightRemainingIfPreviousElementInView = viewportHeightRemaining - childHeight;

				if (CoreUtilities.LessThan(heightRemainingIfPreviousElementInView, 0.0))
					break;

				firstVisibleItemIndex--;
				viewportHeightRemaining = heightRemainingIfPreviousElementInView;
			}

			int numberOfVisibleItems = lastVisibleItemIndex - firstVisibleItemIndex;
			if (numberOfVisibleItems == 0 || CoreUtilities.GreaterThanOrClose(viewportHeightRemaining, 0.0))
				numberOfVisibleItems++;

			_scrollInfo._physicalViewportHeight = constraint.Height;

			double viewportPhysicalWidth = constraint.Width;
			double totalExtentPhysicalWidth = totalSize.Width;
			double offsetPhysicalX = Math.Max(0, Math.Min(_scrollInfo._offsetPixelsX, totalExtentPhysicalWidth - viewportPhysicalWidth));

			this.UpdateScrollingData(
				numberOfVisibleItems, viewportPhysicalWidth, 
				children.Count, totalExtentPhysicalWidth,
				firstVisibleItemIndex, offsetPhysicalX);

			totalSize.Width = Math.Min(totalSize.Width, constraint.Width);
			totalSize.Height = Math.Min(totalSize.Height, constraint.Height);
			return totalSize;
		}

		#endregion  // MeasureOverride

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region LineDown

		/// <summary>
		/// Scrolls down by one item.
		/// </summary>
		public void LineDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + 1.0);
		}

		#endregion  // LineDown

		#region LineLeft

		/// <summary>
		/// Scrolls left by the small scroll amount.
		/// </summary>
		public void LineLeft()
		{
			this.SetHorizontalOffset(this.HorizontalOffset - 16.0);
		}

		#endregion  // LineLeft

		#region LineRight

		/// <summary>
		/// Scrolls right by the small scroll amount.
		/// </summary>
		public void LineRight()
		{
			this.SetHorizontalOffset(this.HorizontalOffset + 16.0);
		}

		#endregion  // LineRight

		#region LineUp

		/// <summary>
		/// Scrolls up by one item.
		/// </summary>
		public void LineUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - 1.0);
		}

		#endregion  // LineUp

		#region MakeVisible

		/// <summary>
		/// Scrolls the specified item into view.
		/// </summary>
		/// <param name="visual">The item to scroll into view.</param>
		/// <param name="rectangle">The rectangle within the item which should be visible if possible.</param>
		/// <returns>The rectangle within the panel which is now visible.</returns>
		public Rect MakeVisible(Visual visual, Rect rectangle)
		{
			if (rectangle.IsEmpty ||
				visual == null ||
				visual == this ||
				PresentationUtilities.IsAncestorOf(this, visual) == false)
			{
				return Rect.Empty;
			}


			GeneralTransform transform =



				visual.TransformToAncestor(this);


			Rect rectangleInPanel = transform.TransformBounds(rectangle);

			if (this.IsScrollingEnabled == false)
				return rectangleInPanel;

			Rect newRectInViewport = new Rect();

			double newOffsetPixelsX = 0;
			this.MakeVisibleHorizontal(rectangleInPanel, ref newOffsetPixelsX, ref newRectInViewport);
			newOffsetPixelsX = AutoCompleteListStackPanel.ResolveOffset(
				newOffsetPixelsX,
				_scrollInfo._totalExtentPhysicalWidth,
				_scrollInfo._viewportPhysicalWidth);

			double newOffsetItemsY = 0;
			this.MakeVisibleVertical(this.FindChildIndexThatParentsVisual(visual), ref newOffsetItemsY, ref newRectInViewport);
			newOffsetItemsY = AutoCompleteListStackPanel.ResolveOffset(
				newOffsetItemsY,
				_scrollInfo._totalExtentItemHeight,
				_scrollInfo._viewportItemHeight);

			if (CoreUtilities.AreClose(newOffsetItemsY, _scrollInfo._offsetItemsY) == false ||
				CoreUtilities.AreClose(newOffsetPixelsX, _scrollInfo._offsetPixelsX) == false)
			{
				_scrollInfo._offsetItemsY = newOffsetItemsY;
				_scrollInfo._offsetPixelsX = newOffsetPixelsX;

				this.InvalidateMeasure();
				this.OnScrollChange();
			}

			return newRectInViewport;
		}

		#endregion  // MakeVisible

		#region MouseWheelDown

		/// <summary>
		/// Scrolls down by the amount required by the mouse wheel click.
		/// </summary>
		public void MouseWheelDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + SystemParameters.WheelScrollLines);
		}

		#endregion  // MouseWheelDown

		#region MouseWheelLeft

		/// <summary>
		/// Scrolls left by the amount required by the mouse wheel click.
		/// </summary>
		public void MouseWheelLeft()
		{
			this.SetHorizontalOffset(this.HorizontalOffset - 48);
		}

		#endregion  // MouseWheelLeft

		#region MouseWheelRight

		/// <summary>
		/// Scrolls right by the amount required by the mouse wheel click.
		/// </summary>
		public void MouseWheelRight()
		{
			this.SetHorizontalOffset(this.HorizontalOffset + 48);
		}

		#endregion  // MouseWheelRight

		#region MouseWheelUp

		/// <summary>
		/// Scrolls up by the amount required by the mouse wheel click.
		/// </summary>
		public void MouseWheelUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - SystemParameters.WheelScrollLines);
		}

		#endregion  // MouseWheelUp

		#region PageDown

		/// <summary>
		/// Scrolls down by one page.
		/// </summary>
		public void PageDown()
		{
			this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight);
		}

		#endregion  // PageDown

		#region PageLeft

		/// <summary>
		/// Scrolls left by the large scroll amount.
		/// </summary>
		public void PageLeft()
		{
			this.SetHorizontalOffset(this.HorizontalOffset - this.ViewportWidth);
		}

		#endregion  // PageLeft

		#region PageRight

		/// <summary>
		/// Scrolls right by the large scroll amount.
		/// </summary>
		public void PageRight()
		{
			this.SetHorizontalOffset(this.HorizontalOffset + this.ViewportWidth);
		}

		#endregion  // PageRight

		#region PageUp

		/// <summary>
		/// Scrolls up by one page.
		/// </summary>
		public void PageUp()
		{
			this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight);
		}

		#endregion  // PageUp

		#region SetHorizontalOffset

		/// <summary>
		/// Sets the <see cref="HorizontalOffset"/> of the panel.
		/// </summary>
		/// <param name="offset">The horizontal offset in Device Independent Pixels.</param>
		public void SetHorizontalOffset(double offset)
		{
			this.VerifyScrollInfo();

			double resolvedHorizontalOffset = AutoCompleteListStackPanel.ValidateExplicitOffset(offset, "HorizontalOffset");
			if (CoreUtilities.AreClose(resolvedHorizontalOffset, _scrollInfo._offsetPixelsX) == false)
			{
				_scrollInfo._offsetPixelsX = resolvedHorizontalOffset;
				this.InvalidateMeasure();
			}
		}

		#endregion  // SetHorizontalOffset

		#region SetVerticalOffset

		/// <summary>
		/// Sets the <see cref="VerticalOffset"/> of the panel.
		/// </summary>
		/// <param name="offset">The vertical offset in units of items.</param>
		public void SetVerticalOffset(double offset)
		{
			this.VerifyScrollInfo();

			double resolvedVerticalOffset = AutoCompleteListStackPanel.ValidateExplicitOffset(offset, "VerticalOffset");
			if (CoreUtilities.AreClose(resolvedVerticalOffset, _scrollInfo._offsetItemsY) == false)
			{
				_scrollInfo._offsetItemsY = resolvedVerticalOffset;
				this.InvalidateMeasure();
			}
		}

		#endregion  // SetVerticalOffset

		#endregion  // Public Methods

		#region Private Methods

		#region DeterminePhysicalScrollOffset

		private static double DeterminePhysicalScrollOffset(double minView, double maxView, double minChild, double maxChild)
		{
			bool doesChildExtendBeforeViewOnly = CoreUtilities.LessThan(minChild, minView) && CoreUtilities.LessThan(maxChild, maxView);
			bool doesChildExtendAfterViewOnly = CoreUtilities.GreaterThan(maxChild, maxView) && CoreUtilities.GreaterThan(minChild, minView);
			bool isChildWiderThanView = (maxChild - minChild) > (maxView - minView);

			if (doesChildExtendBeforeViewOnly && isChildWiderThanView == false)
				return minChild;

			if (doesChildExtendAfterViewOnly && isChildWiderThanView)
				return minChild;

			if (doesChildExtendBeforeViewOnly == false && doesChildExtendAfterViewOnly == false)
				return minView;

			return maxChild - (maxView - minView);
		}

		#endregion  // DeterminePhysicalScrollOffset

		#region FindChildIndexThatParentsVisual

		private int FindChildIndexThatParentsVisual(Visual child)
		{
			DependencyObject element = child;
			DependencyObject parent = VisualTreeHelper.GetParent(child);

			while (parent != this)
			{
				element = parent;
				parent = VisualTreeHelper.GetParent(element);

				if (parent == null)
					throw new ArgumentException(FormulaEditorUtilities.GetString("LE_ArgumentException_PanelNotInAncestorChain"));
			}

			return this.Children.IndexOf((UIElement)element);
		}

		#endregion  // FindChildIndexThatParentsVisual

		#region GetPhysicalOffset

		private double GetPhysicalOffset(double logicalOffset)
		{
			UIElementCollection children = this.Children;

			double physicalOffset = 0.0;
			for (int i = 0; i < logicalOffset; i++)
			{
				UIElement child = children[i];

				if (child != null)
					physicalOffset -= child.DesiredSize.Height;
			}

			return physicalOffset;
		}

		#endregion  // GetPhysicalOffset

		#region MakeVisibleHorizontal

		private void MakeVisibleHorizontal(Rect rectangleInPanel, ref double newOffsetPixelsX, ref Rect newRectInViewport)
		{
			double xOffsetInViewport = _scrollInfo._calculatedOffsetPixelsX;
			double viewportWidth = this.ViewportWidth;
			double xOffsetInElement = rectangleInPanel.X + xOffsetInViewport;
			double elementWidth = rectangleInPanel.Width;

			newOffsetPixelsX = AutoCompleteListStackPanel.DeterminePhysicalScrollOffset(
				xOffsetInViewport,
				xOffsetInViewport + viewportWidth,
				xOffsetInElement,
				xOffsetInElement + elementWidth);

			double resolvedLeftEdge = Math.Max(xOffsetInElement, newOffsetPixelsX);
			double resolvedRightEdge = Math.Min(xOffsetInElement + elementWidth, newOffsetPixelsX + viewportWidth);

			newRectInViewport.X = resolvedLeftEdge - xOffsetInViewport;
			newRectInViewport.Width = Math.Max(0, resolvedRightEdge - resolvedLeftEdge);
		}

		#endregion  // MakeVisibleHorizontal

		#region MakeVisibleVertical

		private void MakeVisibleVertical(int childIndex, ref double newOffsetItemsY, ref Rect newRectInViewport)
		{
			double physicalYOffsetInViewport = 0.0;

			int firstVisibleItemIndex = (int)_scrollInfo._calculatedOffsetItemsY;
			int numberOfVisibleItems = (int)_scrollInfo._viewportItemHeight;

			int indexOfItemToMakeFirst = firstVisibleItemIndex;

			UIElement child = this.Children[childIndex];
			double itemHeight = child != null ? child.DesiredSize.Height : 0;

			if (childIndex < firstVisibleItemIndex)
			{
				indexOfItemToMakeFirst = childIndex;
			}
			else if (((firstVisibleItemIndex + numberOfVisibleItems) - 1) < childIndex)
			{
				double remainingVisibleArea = _scrollInfo._physicalViewportHeight - itemHeight;

				int previousItemIndex = childIndex;
				while (previousItemIndex > 0 && CoreUtilities.GreaterThanOrClose(remainingVisibleArea, 0))
				{
					previousItemIndex--;

					UIElement previousChild = this.Children[previousItemIndex];
					itemHeight = previousChild != null ? previousChild.DesiredSize.Height : 0;
					physicalYOffsetInViewport += itemHeight;
					remainingVisibleArea -= itemHeight;
				}

				if (previousItemIndex != childIndex && CoreUtilities.LessThan(remainingVisibleArea, 0))
				{
					physicalYOffsetInViewport -= itemHeight;
					previousItemIndex++;
				}

				indexOfItemToMakeFirst = previousItemIndex;
			}

			newOffsetItemsY = indexOfItemToMakeFirst;
			newRectInViewport.Y = physicalYOffsetInViewport;
			newRectInViewport.Height = itemHeight;
		}

		#endregion  // MakeVisibleVertical

		#region OffsetToItemIndex

		private static int OffsetToItemIndex(double offset, int numberOfItems)
		{
			if (double.IsNegativeInfinity(offset))
				return 0;

			int maxItemIndex = numberOfItems - 1;

			if (double.IsPositiveInfinity(offset))
				return maxItemIndex;

			int index = (int)offset;
			return Math.Max(0, Math.Min(index, maxItemIndex));
		}

		#endregion  // OffsetToItemIndex

		#region OnScrollChange

		private void OnScrollChange()
		{
			if (this.ScrollOwner != null)
				this.ScrollOwner.InvalidateScrollInfo();
		}

		#endregion  // OnScrollChange

		#region ResetScrolling

		private void ResetScrolling()
		{
			this.InvalidateMeasure();

			if (this.IsScrollingEnabled)
				_scrollInfo.Reset();
		}

		#endregion  // ResetScrolling

		#region ResolveOffset

		private static double ResolveOffset(double offset, double extent, double viewport)
		{
			return Math.Max(0, Math.Min(offset, extent - viewport));
		}

		#endregion  // ResolveOffset

		#region UpdateScrollingData

		private void UpdateScrollingData(
			double viewportItemHeight, 
			double viewportPhysicalWidth,
			double totalExtentItemHeight,
			double totalExtentPhysicalWidth, 
			double offsetItemsY,
			double offsetPixelsX)
		{
			_scrollInfo._offsetItemsY = offsetItemsY;
			_scrollInfo._offsetPixelsX = offsetPixelsX;

			if (CoreUtilities.AreClose(viewportItemHeight, _scrollInfo._viewportItemHeight) &&
				CoreUtilities.AreClose(viewportPhysicalWidth, _scrollInfo._viewportPhysicalWidth) &&
				CoreUtilities.AreClose(totalExtentItemHeight, _scrollInfo._totalExtentItemHeight) &&
				CoreUtilities.AreClose(totalExtentPhysicalWidth, _scrollInfo._totalExtentPhysicalWidth) &&
				CoreUtilities.AreClose(offsetItemsY, _scrollInfo._calculatedOffsetItemsY) &&
				CoreUtilities.AreClose(offsetPixelsX, _scrollInfo._calculatedOffsetPixelsX))
			{
				return;
			}

			_scrollInfo._viewportItemHeight = viewportItemHeight;
			_scrollInfo._viewportPhysicalWidth = viewportPhysicalWidth;
			_scrollInfo._totalExtentItemHeight = totalExtentItemHeight;
			_scrollInfo._totalExtentPhysicalWidth = totalExtentPhysicalWidth;
			_scrollInfo._calculatedOffsetItemsY = offsetItemsY;
			_scrollInfo._calculatedOffsetPixelsX = offsetPixelsX;

			this.OnScrollChange();
		}

		#endregion  // UpdateScrollingData

		#region ValidateExplicitOffset

		private static double ValidateExplicitOffset(double offset, string parameterName)
		{
			if (Double.IsNaN(offset))
				throw new ArgumentOutOfRangeException(parameterName, FormulaEditorUtilities.GetString("LE_ArgumentOutOfRangeException_OffsetCantBeNaN", parameterName));

			return Math.Max(0.0, offset);
		}

		#endregion  // ValidateExplicitOffset

		#region VerifyScrollInfo

		private void VerifyScrollInfo()
		{
			if (_scrollInfo == null)
				_scrollInfo = new ScrollInfo();
		}

		#endregion  // VerifyScrollInfo

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region CanHorizontallyScroll

		/// <summary>
		/// Gets or sets a value that indicates whether the panel can be scrolled horizontally.
		/// </summary>
		public bool CanHorizontallyScroll
		{
			get
			{
				if (_scrollInfo == null)
					return false;

				return _scrollInfo._canHorizontallyScroll;
			}
			set
			{
				this.VerifyScrollInfo();
				if (_scrollInfo._canHorizontallyScroll == value)
					return;

				_scrollInfo._canHorizontallyScroll = value;
				this.InvalidateMeasure();
			}
		}

		#endregion  // CanHorizontallyScroll

		#region CanVerticallyScroll

		/// <summary>
		/// Gets or sets a value that indicates whether the panel can be scrolled vertically.
		/// </summary>
		public bool CanVerticallyScroll
		{
			get
			{
				if (_scrollInfo == null)
					return false;

				return _scrollInfo._canVerticallyScroll;
			}
			set
			{
				this.VerifyScrollInfo();
				if (_scrollInfo._canVerticallyScroll == value)
					return;

				_scrollInfo._canVerticallyScroll = value;
				this.InvalidateMeasure();
			}
		}

		#endregion  // CanVerticallyScroll

		#region ExtentHeight

		/// <summary>
		/// Gets the vertical size of the list, in units of items.
		/// </summary>
		public double ExtentHeight
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._totalExtentItemHeight;
			}
		}

		#endregion  // ExtentHeight

		#region ExtentWidth

		/// <summary>
		/// Gets the vertical size of the list, in Device Independent Pixels.
		/// </summary>
		public double ExtentWidth
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._totalExtentPhysicalWidth;
			}
		}

		#endregion  // ExtentWidth

		#region HorizontalOffset

		/// <summary>
		/// Gets the current horizontal scroll offset, in Device Independent Pixels.
		/// </summary>
		public double HorizontalOffset
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._calculatedOffsetPixelsX;
			}
		}

		#endregion  // HorizontalOffset

		#region ScrollOwner

		/// <summary>
		/// Gets or sets the ScrollViewer which is displaying this panel.
		/// </summary>
		public ScrollViewer ScrollOwner
		{
			get
			{
				this.VerifyScrollInfo();
				return _scrollInfo._scrollOwner;
			}
			set
			{
				this.VerifyScrollInfo();
				if (_scrollInfo._scrollOwner == value)
					return;

				this.ResetScrolling();
				_scrollInfo._scrollOwner = value;
			}
		}

		#endregion  // ScrollOwner

		#region VerticalOffset

		/// <summary>
		/// Gets the current vertical scroll offset, in units of items.
		/// </summary>
		public double VerticalOffset
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._calculatedOffsetItemsY;
			}
		}

		#endregion  // VerticalOffset

		#region ViewportHeight

		/// <summary>
		/// Gets the height of the currently visible portion of the list, in units of items.
		/// </summary>
		public double ViewportHeight
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._viewportItemHeight;
			}
		}

		#endregion  // ViewportHeight

		#region ViewportWidth

		/// <summary>
		/// Gets the height of the currently visible portion of the list, in Device Independent Pixels.
		/// </summary>
		public double ViewportWidth
		{
			get
			{
				if (_scrollInfo == null)
					return 0;

				return _scrollInfo._viewportPhysicalWidth;
			}
		}

		#endregion  // ViewportWidth

		#endregion  // Public Properties

		#region Private Properties

		#region IsScrollingEnabled

		private bool IsScrollingEnabled
		{
			get
			{
				return _scrollInfo != null && _scrollInfo._scrollOwner != null;
			}
		}

		#endregion  // IsScrollingEnabled

		#endregion  // Private Properties

		#endregion  // Properties


		#region ScrollInfo class

		private class ScrollInfo
		{
			internal double _calculatedOffsetItemsY;
			internal double _calculatedOffsetPixelsX;
			internal bool _canHorizontallyScroll;
			internal bool _canVerticallyScroll;
			internal double _offsetItemsY;
			internal double _offsetPixelsX;
			internal double _physicalViewportHeight;
			internal ScrollViewer _scrollOwner;
			internal double _totalExtentItemHeight;
			internal double _totalExtentPhysicalWidth;
			internal double _viewportItemHeight;
			internal double _viewportPhysicalWidth;

			internal void Reset()
			{
				_offsetItemsY = 0;
				_offsetPixelsX = 0;
				_physicalViewportHeight = 0;
				_totalExtentItemHeight = 0;
				_totalExtentPhysicalWidth = 0;
				_viewportItemHeight = 0;
				_viewportPhysicalWidth = 0;
			}
		}

		#endregion  // ScrollInfo class
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