using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using System.Collections;
using Infragistics.Windows.Helpers;
using System.Windows.Media;
using System.Windows.Threading;
using Infragistics.Windows.Reporting;
using System.Windows.Input;

namespace Infragistics.Windows.Controls
{
	/// <summary>
	/// Panel used to arrange items like tab headers in single or multiple rows.
	/// </summary>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class TabItemPanel : Panel,
		IScrollInfo
	{
		
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		#region Member Variables

		private List<TabRowInfo> _rows = new List<TabRowInfo>();
		private double? _lastExtent;
		private double _lastPreferredExtent = 0;
		private bool _isMeasuringInArrange = false;
		private double _tabRowHeight = 0;
		private Selector _hookedSelector;
        private int _maximumTabRows;

        // JJD 8/28/08 - added for multi-row support
        private bool _alignTabRowToSelectedTab;

        // JJD 9/2/08
        // Keep track of the number of rows that were arranged
        private int _lastNumberOfRowsArranged;

		#endregion //Member Variables

        #region Constants

        // AS 2/11/08 NA 2008 Vol 1
        private const double LineOffset = 16;

        // JJD 8/27/08 - added for multi-row support
        private const double ElementOffset = 10000;

        #endregion //Constants	
    
		#region Constructor

		static TabItemPanel()
		{
			// AS 5/4/10 TFS30695
			// The base TabControl class will set the KeyboardNaviation.TabOnceActiveElement when the 
			// selection changes but that only works if the direct parent of the element (the tabitem in this 
			// case) has its TabNavigation set to Once. As the intrinsic TabPanel does, we'll default these 
			// property values for this class so tabnavigation only goes to the selected tab and the arrow 
			// keys navigate within the panel.
			//
			KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(TabItemPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Once));
			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(TabItemPanel), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));
		}

		/// <summary>
		/// Initializes a new <see cref="TabItemPanel"/>
		/// </summary>
		public TabItemPanel()
		{
            // JJD 8/1/08 
            // Initialize cached value
            this._maximumTabRows = (int)MaximumTabRowsProperty.DefaultMetadata.DefaultValue;
		}
		#endregion //Constructor

		#region Base class overrides

		#region ArrangeOverride
		/// <summary>
		/// Positions child elements and determines a size for this element.
		/// </summary>
		/// <param name="finalSize">The size available to this element for arranging its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> used by this element to arrange its children.</returns>
		protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
		{
			#region Variables

			bool isVertical = this.IsVertical;
			double rowHeight = 0;
			double interTabSpacing = this.InterTabSpacing;
			double interRowSpacing = this.InterRowSpacing;
			Dock tabPlacement = this.TabStripPlacement;
			int rowsProcessed = 0;
			double maxAvailableExtent = isVertical ? finalSize.Height : finalSize.Width;

			#endregion //Variables

			// if the extent has changed then we need to redistribute the values
			if (maxAvailableExtent != this._lastExtent)
				this.UpdateExtents(maxAvailableExtent);

			#region More Variables

			TabLayoutStyle layoutStyle = this.TabLayoutStyle;
			double tabRowHeight = this._tabRowHeight;
			List<TabRowInfo> rows = this._rows;

			// determine the first row of tabs to position based on the scroll position
			int startingRowIndex = this.FirstTabItemRowIndex;

            Debug.WriteLine("Starting Row index: " + startingRowIndex.ToString() + ", row count" + rows.Count.ToString());

			// since we need to position all the tab items regardless of where 
			// we are scrolled to, store what would be the first visible row
			int originalStartingRowIndex = startingRowIndex;

			#endregion //More Variables

			#region Determine starting point for tab positioning
			// initialize rect based on placement
			Rect tabRect = new Rect();

			// the tab rows are positioned with the first row being closest
			// to the center of the control and subsequent rows moving outwards
			switch (tabPlacement)
			{
				case Dock.Top:
					tabRect.Y = finalSize.Height;
					break;
				case Dock.Left:
					tabRect.X = finalSize.Width;
					break;
			} 
			#endregion //Determine starting point for tab positioning

			#region Adjust starting point when more rows than should fit
			// then if there are more rows than are allowed to fit into view...
			if (rows.Count > this.MaximumTabRows)
			{
				// adjust the starting row position where the first
				// row of tabs would be
				switch (tabPlacement)
				{
					case Dock.Top:
						tabRect.Y += tabRowHeight * startingRowIndex;
						break;
					case Dock.Left:
						tabRect.X += tabRowHeight * startingRowIndex;
						break;
					case Dock.Bottom:
						tabRect.Y -= tabRowHeight * startingRowIndex;
						break;
					case Dock.Right:
						tabRect.X -= tabRowHeight * startingRowIndex;
						break;
				}

				// then reset the starting row index so we position
				// starting with the first row of tabs
				startingRowIndex = 0;
			} 
			#endregion //Adjust starting point when more rows than should fit

			#region Calculate tab origin
			// AS 2/11/08 NA 2008 Vol 1
			Point origin = new Point();

			// we now support scrolling a single row so offset the origin of the row
			if (this.IsMultiRow == false && this.IsScrollClient)
			{
				// adjust the starting row position where the first
				// row of tabs would be
				switch (tabPlacement)
				{
					case Dock.Top:
					case Dock.Bottom:
						origin.X = tabRect.X - this._scrollDataInfo._offset.X;
						break;
					case Dock.Left:
					// AS 6/24/08 BR34248
					// Tabs on the left start on top like they do when on the right.
					//
					//	origin.Y = finalSize.Height + this._scrollDataInfo._offset.Y;
					//	break;
					case Dock.Right:
						origin.Y = tabRect.Y - this._scrollDataInfo._offset.Y;
						break;
				}
			} 
			#endregion //Calculate tab origin

			#region More Variable Setup

			int maxRowIndex = originalStartingRowIndex + (this.MaximumTabRows - 1);
			int rowCount = rows.Count;
			// AS 6/24/08 BR34248
			//bool tabsOnLeft = tabPlacement == Dock.Left; 

			#endregion //More Variable Setup

			// position the tabs in each row
			for (int rowIndex = startingRowIndex, endRow = startingRowIndex + rowCount; rowIndex < endRow; rowIndex++)
			{
				TabRowInfo row = rows[rowIndex % rowCount];
				List<TabItemInfo> tabs = row.Tabs;

				#region Row Change

				double lastRowHeight = rowHeight;
				rowHeight = tabRowHeight;		// the height for this row
				rowsProcessed++;				// the number of rows arranged

				#region Adjust TabRect for New Row
				// adjust the tab rect such that new rows are further
				// from the center of the control
				switch (tabPlacement)
				{
					case Dock.Top:
						tabRect.Y -= rowHeight;
						if (rowsProcessed > 1)
							tabRect.Y -= interRowSpacing;
						break;
					case Dock.Bottom:
						tabRect.Y += lastRowHeight;
						if (rowsProcessed > 1)
							tabRect.Y += interRowSpacing;
						break;
					case Dock.Left:
						tabRect.X -= rowHeight;
						if (rowsProcessed > 1)
							tabRect.X -= interRowSpacing;
						break;
					case Dock.Right:
						tabRect.X += lastRowHeight;
						if (rowsProcessed > 1)
							tabRect.X += interRowSpacing;
						break;
				} 
				#endregion //Adjust TabRect for New Row

				#region Reset the thickness of the tab and start pt in row
				if (isVertical)
				{
					tabRect.Y = origin.Y;
					tabRect.Width = rowHeight;
				}
				else
				{
					tabRect.Height = rowHeight;
					tabRect.X = origin.X;
				} 
				#endregion //Reset the thickness of the tab and start pt in row

				#endregion //Row Change

				#region Process Row Changes
				for (int i = 0, count = tabs.Count; i < count; i++)
				{
					TabItemInfo tab = tabs[i];
					UIElement element = tab.Element;

					Size desiredSize = element.DesiredSize;
					double desiredExtent = tab.Extent;

					if (isVertical)
						tabRect.Height = desiredExtent;
					else
						tabRect.Width = desiredExtent;

					if (tabRect.Width < element.DesiredSize.Width ||
						tabRect.Height < element.DesiredSize.Height)
					{
						this._isMeasuringInArrange = true;
						element.Measure(tabRect.Size);
						this._isMeasuringInArrange = false;
					}

					// AS 6/24/08 BR34248
					//// have to shift up for tabs on the left since they arrange
					//// from bottom to top
					//if (tabsOnLeft)
					//	tabRect.Y -= desiredExtent;

                    // JJD 8/27/08
                    // Instead of messing with Opacity just position the tabs not in view so that they
                    // are way out of view
                    if (rowIndex < originalStartingRowIndex || rowIndex > maxRowIndex)
                    {
                        Vector offset = new Vector();

                        if (isVertical)
                        {
                            if (tabRect.X < 0)
                                offset.X = -ElementOffset;
                            else
                                offset.X = ElementOffset;
                        }
                        else
                        {
                            if (tabRect.Y < 0)
                                offset.Y = -ElementOffset;
                            else
                                offset.Y = ElementOffset;
                        }

                        element.Arrange(Rect.Offset(tabRect, offset));
                    }
                    else
					    element.Arrange(tabRect);

					// multirow support - if the tab is scrolled out of view
					// then we need to hide the tab item
					
					
                    
                    // JJD 8/27/08
                    // Instead of messing with Opacity just position the tabs not in view so that they
                    // are way out of view
                    //if (rowIndex < originalStartingRowIndex || rowIndex > maxRowIndex)
                    //    element.Opacity = 0d;
                    //else
                    //    element.ClearValue(OpacityProperty);

					// shift the rect for the next tab
					if (isVertical == false)
						tabRect.X += desiredExtent + interTabSpacing;
					// AS 6/24/08 BR34248
					//else if (tabsOnLeft == false)
					//	tabRect.Y += desiredExtent + interTabSpacing;
					//else // left
					//	tabRect.Y -= interTabSpacing;
					else
						tabRect.Y += desiredExtent + interTabSpacing;
				}
				#endregion //Process Row Changes
			}

            // JJD 9/02/08
            // If the number of rows changes or if the alignTabRowToSelectedTab is true, set when a property 
            // is changed that effects tab layout, then call BringSelectedTabIntoView asynchronously
            if (this._alignTabRowToSelectedTab == true ||
                this._lastNumberOfRowsArranged != rowCount)
            {
                this._alignTabRowToSelectedTab = false;
                this._lastNumberOfRowsArranged = rowCount;

                EventHandler handler = new EventHandler(OnLayoutUpdated);
                this.LayoutUpdated -= handler;
                this.LayoutUpdated += handler;
            }

			return finalSize;
		}

		#endregion //ArrangeOverride

		#region MeasureOverride
		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="availableSize">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
		{
			#region Setup

			
			// probably use the order of the tabs in the rows? possibly validate
			// the order of the tabs with the original order we had?
			this._rows.Clear();

			// clear out the cached extent so we know to force
			// a calculation of the extents in the arrange
			this._lastExtent = null;

			double tabRowHeight = 0;		// current row's logical height
			double tabRowExtent = 0;		// current row's logical width

			double maxTabRowExtent = 0;		// widest tab row
			double maxTabRowHeight = 0;		// tallest tab row

			int tabsInRow = 0;				// number of tabs in the current row
			int tabRowCount = 0;			// number of tab rows

			double interTabSpacing = this.InterTabSpacing;
			double interRowSpacing = this.InterRowSpacing;

			bool isVertical = this.IsVertical;
			bool isMultiRow = this.IsMultiRow;

			// the amount of space available for each row of tabs
			double availableExtent = isVertical ? availableSize.Height : availableSize.Width;

			UIElementCollection children = this.Children;
			TabRowInfo row = null;			// current row
			bool useMinExtent = isMultiRow == false && this.TabLayoutStyle != TabLayoutStyle.SingleRowAutoSize;

			// AS 2/11/08 NA 2008 Vol 1
			// If we're being sized with infinity we can assume that we're trying to 
			// find out the preferred size so measure with the preferred size.
			//
			if (useMinExtent && double.IsPositiveInfinity(availableExtent) && this.IsScrollClient)
				useMinExtent = false;

			this._lastPreferredExtent = 0;

			Size infiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

			#endregion // Setup

			#region Iterate Tabs
			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement element = children[i];

				// skip collapsed items
				if (element.Visibility == Visibility.Collapsed)
					continue;

				// AS 2/28/07 NA 2008 Vol 1
				// For the first row we can assume that the first visible tab is the 
				// first tab item in a row. for all others, wait until a row break.
				//
				if (tabsInRow == 0)
					element.SetValue(IsFirstTabInRowPropertyKey, KnownBoxes.TrueBox);
				else
					element.ClearValue(IsFirstTabInRowPropertyKey);

				// measure it with the full available size
				element.Measure(infiniteSize);

				// get its desired size so we can determine its position
				Size desiredSize = element.DesiredSize;
				double minExtent = this.GetMinExtent(element, isVertical);
				double preferredExtent = Math.Max(minExtent, isVertical ? desiredSize.Height : desiredSize.Width);
				double extentToUse = useMinExtent ? minExtent : preferredExtent;

				double desiredTabHeight = isVertical ? desiredSize.Width : desiredSize.Height;

				#region Create New Row
				// for multirow tab layouts we may need to start a new row
				if (isMultiRow &&			// using multiple rows
					tabsInRow > 0 &&		// we have at least one row
					extentToUse + tabRowExtent > availableExtent - interTabSpacing) // this tab would go over the line
				{
					// AS 2/28/07 NA 2008 Vol 1
					// We know this is the first tab in a row so set the flag and remeasure.
					//
					element.SetValue(IsFirstTabInRowPropertyKey, KnownBoxes.TrueBox);

					// if the element was dirtied by changing the flag then remeasure
					// and get the new values
					if (false == element.IsMeasureValid)
					{
						// measure it with the full available size
						element.Measure(infiniteSize);

						// get its desired size so we can determine its position
						desiredSize = element.DesiredSize;
						minExtent = this.GetMinExtent(element, isVertical);
						preferredExtent = Math.Max(minExtent, isVertical ? desiredSize.Height : desiredSize.Width);
						extentToUse = useMinExtent ? minExtent : preferredExtent;

						desiredTabHeight = isVertical ? desiredSize.Width : desiredSize.Height;
					}

					// track the tallest row
					if (tabRowHeight > maxTabRowHeight)
						maxTabRowHeight = tabRowHeight;

					// track the widest row
					if (row.PreferredExtent > this._lastPreferredExtent)
						this._lastPreferredExtent = row.PreferredExtent;

					// store the widest row of tabs
					if (tabRowExtent > maxTabRowExtent)
						maxTabRowExtent = tabRowExtent;

					tabsInRow = 0;
				}
				#endregion // Create New Row

				// if this is the first tab in this row then start a new row
				if (tabsInRow == 0)
				{
					row = new TabRowInfo();
					this._rows.Add(row);

					tabRowCount++;
					tabRowHeight = desiredTabHeight;
					tabRowExtent = extentToUse;
				}
				else
				{
					// store the width of the tab items plus the space
					// between the tab items
					tabRowExtent += extentToUse + interTabSpacing;

					// store the tallest tab height for the row
					if (desiredTabHeight > tabRowHeight)
						tabRowHeight = desiredTabHeight;

					// AS 2/11/08 NA 2008 Vol 1
					// Include the intertab spacing.
					//
					row.PreferredExtent += interTabSpacing;
				}

				tabsInRow++;

				row.PreferredExtent += preferredExtent;
				TabItemInfo tabInfo = new TabItemInfo(element, i, row);
				row.Tabs.Add(tabInfo);
				element.SetValue(TabInfoProperty, tabInfo);
			} 
			#endregion // Iterate Tabs

			#region Process Last Row

			// update the metrics for the last row of tabs
			if (tabRowCount > 0)
			{
				if (tabRowHeight > maxTabRowHeight)
					maxTabRowHeight = tabRowHeight;

				if (row.PreferredExtent > this._lastPreferredExtent)
					this._lastPreferredExtent = row.PreferredExtent;

				// store the widest row of tabs
				if (tabRowExtent > maxTabRowExtent)
					maxTabRowExtent = tabRowExtent;
			} 
			#endregion // Process Last Row

			// AS 2/11/08 NA 2008 Vol 1
			// If we are asked to measure with more than we need for the minimum then go up
			// as far as the preferred.
			//
			if (this._lastPreferredExtent > maxTabRowExtent && maxTabRowExtent < availableExtent)
				maxTabRowExtent = Math.Min(this._lastPreferredExtent, availableExtent);

			this._tabRowHeight = maxTabRowHeight;

			#region Initialize ScrollInfo
			Size scrollViewPort, scrollExtent;

			if (isMultiRow)
			{
				int lastRow = Math.Max(0, this._rows.Count - 1);

				if (isVertical)
				{
					scrollViewPort = new Size(Math.Min(lastRow, this.MaximumTabRows - 1), availableSize.Height);
					scrollExtent = new Size(lastRow, maxTabRowExtent);
				}
				else
				{
					scrollViewPort = new Size(availableSize.Width, Math.Min(lastRow, this.MaximumTabRows - 1));
					scrollExtent = new Size(maxTabRowExtent, lastRow);
				}
			}
			else
			{
				// AS 2/11/08 NA 2008 Vol 1
				// We need to support scrolling when using a single row layout as well.
				// Otherwise we will not be able to properly handle single row justified
				// layout style (and possibly other styles). In this case, we're getting
				// measured with Infinity because we're in a scrollviewed. Well with single
				// row justified, we want to size to the smallest available size when there
				// isn't enough room but if there is enough room then go up to the maximum.
				// If we assume Infinity means to indicate the preferred then we end up 
				// measuring with autosize and if the scrollviewer doesn't have enough room,
				// the panel is then scrollable instead of reducing the item size towards the 
				// minimum. If we assume to measure with the minimum then we can never indicate
				// to the caller what our preferred size should be.
				//
				scrollViewPort = new Size(availableSize.Width, availableSize.Height);

				if (isVertical)
					scrollExtent = new Size(maxTabRowHeight, maxTabRowExtent);
				else
					scrollExtent = new Size(maxTabRowExtent, maxTabRowHeight);
			}

            // JJD 9/02/08
            // Hold the old offset
            Vector oldOffset = this.ScrollDataInfo._offset;

			this.ScrollDataInfo.VerifyScrollData(scrollViewPort, scrollExtent);

            // JJD 9/02/08
            // If we are in multirow and the offsets were constrained about then
            // then set the flag so we will asynchronously bring the selected tab into view
            if (isMultiRow && this.IsInitialized &&
                 (false == PagerContentPresenter.AreClose(this.ScrollDataInfo._offset.X, oldOffset.X) ||
                    false == PagerContentPresenter.AreClose(this.ScrollDataInfo._offset.Y, oldOffset.Y)))
                this._alignTabRowToSelectedTab = true;

			#endregion // Initialize ScrollInfo

			#region DesiredSize calculations

			
			double totalHeight = (maxTabRowHeight + interRowSpacing) * Math.Min(this._rows.Count, this.MaximumTabRows);

			// remove 1 inter-row space
			if (totalHeight > 0)
				totalHeight -= interRowSpacing;

			Size desiredElementSize = isVertical
				? new Size(totalHeight, maxTabRowExtent)
				: new Size(maxTabRowExtent, totalHeight);

			if (this.IsScrollClient)
			{
				if (this.ScrollDataInfo._canHorizontallyScroll && availableSize.Width < desiredElementSize.Width)
					desiredElementSize.Width = availableSize.Width;

				if (this.ScrollDataInfo._canVerticallyScroll && availableSize.Height < desiredElementSize.Height)
					desiredElementSize.Height = availableSize.Height;
			}

			return desiredElementSize; 
			
			#endregion // DesiredSize calculations

		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Invoked when the <see cref="UIElement.DesiredSize"/> of an element changes.
		/// </summary>
		/// <param name="child">The child whose size is being changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isMeasuringInArrange)
				return;

			base.OnChildDesiredSizeChanged(child);
		}
		#endregion //OnChildDesiredSizeChanged

		#region OnIsItemsHostChanged
		/// <summary>
		/// Invoked when the <see cref="Panel.IsItemsHost"/> has changed.
		/// </summary>
		/// <param name="oldIsItemsHost">The old property value</param>
		/// <param name="newIsItemsHost">The new property value</param>
		protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
		{
			ItemsControl items = ItemsControl.GetItemsOwner(this);

			
			this.HookSelector(items as Selector);

			base.OnIsItemsHostChanged(oldIsItemsHost, newIsItemsHost);
		}
		#endregion //OnIsItemsHostChanged

		#region OnPropertyChanged
		/// <summary>
		/// Invoked when a property has changed on the object
		/// </summary>
		/// <param name="e">Provides information about the property change</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if (e.Property == FrameworkElement.ActualWidthProperty)
			{
				if (e.OldValue is double &&
					(double)e.OldValue < this._lastPreferredExtent &&
					this.IsVertical == false &&
					this.IsMultiRow == false &&
					this.TabLayoutStyle != TabLayoutStyle.SingleRowAutoSize)
				{
					this.InvalidateMeasure();
				}
			}
			else if (e.Property == FrameworkElement.ActualHeightProperty)
			{
				if (e.OldValue is double &&
					(double)e.OldValue < this._lastPreferredExtent &&
					this.IsVertical == true &&
					this.IsMultiRow == false &&
					this.TabLayoutStyle != TabLayoutStyle.SingleRowAutoSize)
				{
					this.InvalidateMeasure();
				}
			}
		}
		#endregion //OnPropertyChanged

		#region OnVisualChildrenChanged
		/// <summary>
		/// Invoked when a tab item has been removed.
		/// </summary>
		/// <param name="visualAdded">Visual being added</param>
		/// <param name="visualRemoved">Visual being removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			base.OnVisualChildrenChanged(visualAdded, visualRemoved);

			if (visualRemoved != null)
			{
				// AS 11/23/09 TFS24548
				// The row containing the tab item will still reference the tabiteminfo
				// from the item that was removed.
				//
				TabItemInfo ti = (TabItemInfo)visualRemoved.GetValue(TabInfoProperty);

				if (null != ti)
				{
					ti.Row.Tabs.Remove(ti);
				}

				visualRemoved.ClearValue(TabInfoProperty);
			}
		}
		#endregion //OnVisualChildrenChanged

		#endregion //Base class overrides

		#region Properties

		#region FirstTabItemRowIndex
		private int FirstTabItemRowIndex
		{
			get
			{
				return this._rows == null || this._rows.Count > this.MaximumTabRows
					? (int)Math.Round(this.IsVertical ? ((IScrollInfo)this).HorizontalOffset : ((IScrollInfo)this).VerticalOffset, 0d)
					: this.SelectedTabRowIndex;
			}
		}
		#endregion //FirstTabItemRowIndex

		#region HorizontalOffsetInternal
		/// <summary>
		/// Returns the actual horizontal offset
		/// </summary>
		internal double HorizontalOffsetInternal
		{
			get { return this.ScrollDataInfo._offset.X; }
		}
		#endregion // HorizontalOffsetInternal

		#region InterRowSpacing

		/// <summary>
		/// Identifies the <see cref="InterRowSpacing"/> property
		/// </summary>
		/// <seealso cref="InterRowSpacing"/>
        // AS 10/17/08
        // The corresponding props on XTC were made public so make these as well.
        //
        public static readonly DependencyProperty InterRowSpacingProperty = XamTabControl.InterRowSpacingProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(XamTabControl.DefaultInterRowSpacing, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// The amount of spacing between rows of tab items.
		/// </summary>
		/// <seealso cref="InterRowSpacingProperty"/>
		//[Description("The amount of spacing between rows of tab items.")]
		//[Category("Layout")]
		[Bindable(true)]
        // AS 10/17/08
        // The corresponding props on XTC were made public so make these as well.
        //
        public double InterRowSpacing
		{
			get { return (double)this.GetValue(InterRowSpacingProperty); }
			set { this.SetValue(InterRowSpacingProperty, value); }
		}
		#endregion //InterRowSpacing

		#region InterTabSpacing

		/// <summary>
		/// Identifies the <see cref="InterTabSpacing"/> property
		/// </summary>
		/// <seealso cref="InterTabSpacing"/>
		// AS 10/16/07
		// Changed to internal. Perhaps we should just expose IsFirstTabInRow, IsLastTabInRow later and let the item change it margins based on that.
		//
		//public static readonly DependencyProperty InterTabSpacingProperty = XamTabControl.InterTabSpacingProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(XamTabControl.DefaultInterTabSpacing, FrameworkPropertyMetadataOptions.AffectsMeasure));
        // AS 10/17/08
        // The corresponding props on XTC were made public so make these as well.
        //
        public static readonly DependencyProperty InterTabSpacingProperty = XamTabControl.InterTabSpacingProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(XamTabControl.DefaultInterTabSpacing, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// The amount of spacing between the tab items in a row.
		/// </summary>
		/// <seealso cref="InterTabSpacingProperty"/>
		//[Description("The amount of spacing between the tab items in a row.")]
		//[Category("Layout")]
		[Bindable(true)]
		// AS 10/16/07
		// Changed to internal. Perhaps we should just expose IsFirstTabInRow, IsLastTabInRow later and let the item change it margins based on that.
		//
		//public double InterTabSpacing
        // AS 10/17/08
        // The corresponding props on XTC were made public so make these as well.
        //
        public double InterTabSpacing
		{
			get { return (double)this.GetValue(InterTabSpacingProperty); }
			set { this.SetValue(InterTabSpacingProperty, value); }
		}
		#endregion //InterTabSpacing

		#region InvertsHorizontalOffset
		private bool InvertsHorizontalOffset
		{
			get
			{
				return this.IsMultiRow == true && this.TabStripPlacement == Dock.Left;
			}
		}
		#endregion // InvertsHorizontalOffset

		#region InvertsVerticalOffset
		private bool InvertsVerticalOffset
		{
			get
			{
				// for multirow tabs on top or single row tabs on left, we need to invert the value
				// AS 6/24/08 BR34248
				//return (this.IsMultiRow == false && this.TabStripPlacement == Dock.Left)
				//				||
				//		(this.IsMultiRow == true && this.TabStripPlacement == Dock.Top);
				return (this.IsMultiRow == true && this.TabStripPlacement == Dock.Top);
			}
		}
		#endregion // InvertsVerticalOffset

		#region IsFirstTabInRow

		private static readonly DependencyPropertyKey IsFirstTabInRowPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("IsFirstTabInRow",
			typeof(bool), typeof(TabItemPanel), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));

		/// <summary>
		/// Identifies the 'IsFirstTabInRow' attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetIsFirstTabInRow"/>
		public static readonly DependencyProperty IsFirstTabInRowProperty =
			IsFirstTabInRowPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns a boolean indicating if the item is the first item in a row.
		/// </summary>
		/// <seealso cref="IsFirstTabInRowProperty"/>
		public static bool GetIsFirstTabInRow(DependencyObject d)
		{
			return (bool)d.GetValue(TabItemPanel.IsFirstTabInRowProperty);
		}

		#endregion //IsFirstTabInRow

		#region IsMultiRow
		private bool IsMultiRow
		{
			get
			{
				return IsMultiRowStyle(this.TabLayoutStyle);
			}
		}
		#endregion //IsMultiRow

		#region IsScrollClient
		private bool IsScrollClient
		{
			// AS 2/11/08 NA 2008 Vol 1
			// We'll consider the panel to be in control of scrolling if its been provided a scroll owner.
			//
			get { return this._scrollDataInfo != null && this._scrollDataInfo._scrollOwner != null; }
		}
		#endregion //IsScrollClient

		#region IsVertical
		private bool IsVertical
		{
			get
			{
				Dock placement = this.TabStripPlacement;

				return placement == Dock.Left || placement == Dock.Right;
			}
		}
		#endregion //IsVertical

		#region MaximumSizeToFitAdjustment

		/// <summary>
		/// Identifies the <see cref="MaximumSizeToFitAdjustment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaximumSizeToFitAdjustmentProperty = XamTabControl.MaximumSizeToFitAdjustmentProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(XamTabControl.DefaultMaximumSizeToFit, FrameworkPropertyMetadataOptions.AffectsArrange, new PropertyChangedCallback(OnMaximumSizeToFitAdjustment)));

		private static void OnMaximumSizeToFitAdjustment(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TabItemPanel panel = d as TabItemPanel;

			if (null != panel && panel.TabLayoutStyle == TabLayoutStyle.SingleRowSizeToFit)
			{
				// clear out the last extent so the layotu will be redone
				panel._lastExtent = null;
			}
		}

		/// <summary>
        /// The maximum amount of additional size to add to an element when the TabLayoutStyle is 'SingleRowSizeToFit' or 'MultiRowSizeToFit'.
        /// </summary>
		/// <seealso cref="MaximumSizeToFitAdjustmentProperty"/>
        /// <seealso cref="XamTabControl.MaximumSizeToFitAdjustment"/>
		/// <seealso cref="XamTabControl.TabLayoutStyle"/>
		//[Description("The maximum amount of additional size to add to an element when the TabLayoutStyle is SingleRowSizeToFit.")]
		//[Category("Layout")]
		[Bindable(true)]
		public double MaximumSizeToFitAdjustment
		{
			get
			{
				return (double)this.GetValue(TabItemPanel.MaximumSizeToFitAdjustmentProperty);
			}
			set
			{
				this.SetValue(TabItemPanel.MaximumSizeToFitAdjustmentProperty, value);
			}
		}

		#endregion //MaximumSizeToFitAdjustment

        // JJD 9/2/08 - added
        #region MaxHorizontalOffset

        internal double MaxHorizontalOffset
        {
            get
            {
                // if this is a multirow tabs where the tabs are on the left/right (i.e. horizontal scrolling)
                // then the offset is the full extent
                if (this.IsMultiRow && this.IsVertical == true)
                    return Math.Max(this.ScrollDataInfo._extent.Width, 0);
                else
                    return Math.Max(this.ScrollDataInfo._extent.Width - this.ScrollDataInfo._viewport.Width, 0);
            }
        }

        #endregion //MaxHorizontalOffset	
    
        // JJD 9/2/08 - added
        #region MaxVerticalOffset

        internal double MaxVerticalOffset
        {
            get
            {
                // if this is a multirow tabs where the tabs are on top/bottom (i.e. vertical scrolling)
                // then the offset is the full extent
                if (this.IsMultiRow && this.IsVertical == false)
                    return Math.Max(this.ScrollDataInfo._extent.Height, 0);
                else
                    return Math.Max(this.ScrollDataInfo._extent.Height - this.ScrollDataInfo._viewport.Height, 0);
            }
        }

        #endregion //MaxVerticalOffset	
    
		#region TabSeparatorOpacity

		private static readonly DependencyPropertyKey TabSeparatorOpacityPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("TabSeparatorOpacity",
			typeof(double), typeof(TabItemPanel), new FrameworkPropertyMetadata(1d));

		/// <summary>
		/// Identifies the TabSeparatorOpacity attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetTabSeparatorOpacity"/>
		public static readonly DependencyProperty TabSeparatorOpacityProperty =
			TabSeparatorOpacityPropertyKey.DependencyProperty;


		/// <summary>
		/// Returns the opacity (as a value between 0.0 and 1.0) of the separator for a tab item based on the current layout style.
		/// </summary>
		/// <seealso cref="TabSeparatorOpacityProperty"/>
		public static double GetTabSeparatorOpacity(DependencyObject d)
		{
			return (double)d.GetValue(TabItemPanel.TabSeparatorOpacityProperty);
		}

		#endregion //TabSeparatorOpacity

		#region MinimumTabExtent

		/// <summary>
		/// Identifies the <see cref="MinimumTabExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinimumTabExtentProperty = XamTabControl.MinimumTabExtentProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(XamTabControl.DefaultMinimumTabExtent, FrameworkPropertyMetadataOptions.AffectsMeasure));

		/// <summary>
		/// The minimum extent for a tab item. That is, the minimum physical width when the TabStripPlacement is Top or Bottom and the minimum physical height when the TabStripPlacement is Left or Right.
		/// </summary>
		/// <seealso cref="MinimumTabExtentProperty"/>
		//[Description("The minimum extent for a tab item. That is, the minimum physical width when the TabStripPlacement is Top or Bottom and the minimum physical height when the TabStripPlacement is Left or Right.")]
		//[Category("Layout")]
		[Bindable(true)]
		public double MinimumTabExtent
		{
			get
			{
				return (double)this.GetValue(TabItemPanel.MinimumTabExtentProperty);
			}
			set
			{
				this.SetValue(TabItemPanel.MinimumTabExtentProperty, value);
			}
		}

		#endregion //MinimumTabExtent

        #region MaximumTabRows

        /// <summary>
        /// Identifies the <see cref="MaximumTabRows"/> dependency property
        /// </summary>
        /// <seealso cref="MaximumTabRows"/>
        public static readonly DependencyProperty MaximumTabRowsProperty = XamTabControl.MaximumTabRowsProperty.AddOwner(typeof(TabItemPanel),
            new FrameworkPropertyMetadata(XamTabControl.MaximumTabRowsProperty.DefaultMetadata.DefaultValue, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnMaximumTabRowsChanged)));

        private static void OnMaximumTabRowsChanged(object target, DependencyPropertyChangedEventArgs e)
        {
            TabItemPanel tip = target as TabItemPanel;

            if (tip != null)
            {
                tip._maximumTabRows = (int)(e.NewValue);
            }
        }

        /// <summary>
        /// Determines the maximum number of tab rows that will be displayed before a vertical scrollbar will appear.
        /// </summary>
        /// <remarks>
        /// <para class="body">The property can only be set to a positive integer. It defaults to 3.</para>
        /// <para class="note"><b>Note:</b> This property is ignored if the <see cref="TabItemPanel.TabLayoutStyle"/> property is not set to one of the 'Multi...' layouts.</para>
        /// </remarks>
        /// <seealso cref="MaximumTabRowsProperty"/>
        /// <seealso cref="TabItemPanel.TabLayoutStyle"/>
        //[Description("Determines the maximum number of tab rows that will be displayed before a vertical scrollbar will appear")]
        //[Category("Behavior")]
        [Bindable(true)]
        public int MaximumTabRows
        {
            get
            {
                return this._maximumTabRows;
            }
            set
            {
                this.SetValue(TabItemPanel.MaximumTabRowsProperty, value);
            }
        }

        #endregion //MaximumTabRows

		#region SelectedTabRowIndex
		private int SelectedTabRowIndex
		{
			get
			{
				int index = 0;

				Selector selector = ItemsControl.GetItemsOwner(this) as Selector;

				if (null != selector && this._rows.Count > 1)
				{
					object selectedItem = selector.SelectedItem;

					index = this.GetTabItemRowNumber(selectedItem);
				}

                Debug.WriteLine("SelectedTabRowIndex: " + index.ToString());

				return index;
			}
		}
		#endregion //SelectedTabRowIndex

		#region TabInfo

		private static readonly DependencyProperty TabInfoProperty = DependencyProperty.RegisterAttached("TabInfo",
			typeof(TabItemInfo), typeof(TabItemPanel), new FrameworkPropertyMetadata(null));

		#endregion //TabInfo

		#region TabLayoutStyle

		/// <summary>
		/// Identifies the <see cref="TabLayoutStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabLayoutStyleProperty = XamTabControl.TabLayoutStyleProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(TabLayoutStyle.SingleRowAutoSize, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnTabLayoutStyleChanged)));

        // JJD 8/28/08 - added for multi-row support
        private static void OnTabLayoutStyleChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemPanel panel = target as TabItemPanel;

            if (panel != null && panel.IsInitialized)
            {
                // JJD 8/28/08 - added for multi-row support
                // set the _alignTabRowToSelectedTab flag so we scroll to the proper row oon the next arrange
                panel._alignTabRowToSelectedTab = true;
            }
        }

		/// <summary>
		/// Determines how the tab items will be arranged.
		/// </summary>
		/// <seealso cref="TabLayoutStyleProperty"/>
		//[Description("Determines how the tab items will be arranged.")]
		//[Category("Layout")]
		[Bindable(true)]
		public TabLayoutStyle TabLayoutStyle
		{
			get
			{
				return (TabLayoutStyle)this.GetValue(TabItemPanel.TabLayoutStyleProperty);
			}
			set
			{
				this.SetValue(TabItemPanel.TabLayoutStyleProperty, value);
			}
		}

		#endregion //TabLayoutStyle

		#region TabStripPlacement

		/// <summary>
		/// Identifies the <see cref="TabStripPlacement"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TabStripPlacementProperty = TabControl.TabStripPlacementProperty.AddOwner(typeof(TabItemPanel), new FrameworkPropertyMetadata(KnownBoxes.DockTopBox, FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(OnTabStripPlacementChanged)));

        // JJD 8/28/08 - added for multi-row support
        private static void OnTabStripPlacementChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            TabItemPanel panel = target as TabItemPanel;

            if (panel != null && panel.IsInitialized)
            {
                // JJD 8/28/08 - added for multi-row support
                // set the _alignTabRowToSelectedTab flag so we scroll to the proper row oon the next arrange
                panel._alignTabRowToSelectedTab = true;
            }
        }
		/// <summary>
		/// Determines the orientation and placement of the tab items.
		/// </summary>
		/// <seealso cref="TabStripPlacementProperty"/>
		//[Description("Determines the orientation and placement of the tab items.")]
		//[Category("Layout")]
		[Bindable(true)]
		public Dock TabStripPlacement
		{
			get
			{
				return (Dock)this.GetValue(TabItemPanel.TabStripPlacementProperty);
			}
			set
			{
				this.SetValue(TabItemPanel.TabStripPlacementProperty, value);
			}
		}

		#endregion //TabStripPlacement

		#region VerticalOffsetInternal
		/// <summary>
		/// Returns the actual vertical offset
		/// </summary>
		internal double VerticalOffsetInternal
		{
			get { return this.ScrollDataInfo._offset.Y; }
		}
		#endregion // VerticalOffsetInternal

		#endregion //Properties

		#region Methods

		#region AdjustHorizontalOffset
		private void AdjustHorizontalOffset(double adjustment, bool adjustForOrientation)
		{
			this.SetHorizontalOffset(((IScrollInfo)this).HorizontalOffset + adjustment, true);
		} 
		#endregion // AdjustHorizontalOffset

		#region AdjustTabsProportionally

		
		
		
		private double AdjustTabsProportionally(List<TabItemInfo> tabs,
			bool hasMultiplePriorities,
			bool isVertical,
			double adjustment,
			double maxPerTabAdjustment)
		{
			List<TabItemInfo> sortedTabs;
			bool expanding = adjustment > 0;

			// AS 6/6/08
			// We always need to sort the tabs because we need them to be sorted
			// based on their extents in descending order (assuming we're reducing).
			// Otherwise we just end up reducing the first {x} tab items that happen
			// to have the same size.
			//
			//if (hasMultiplePriorities == false)
			//{
			//	sortedTabs = tabs;
			//}
			//else
			{
				sortedTabs = new List<TabItemInfo>(tabs);
				sortedTabs.Sort(TabPriorityComparer.Instance);

				if (expanding)
					sortedTabs.Reverse();
			}

			double totalAdjustmentsMade = AdjustTabsProportionallyImpl(sortedTabs, hasMultiplePriorities, isVertical, adjustment, maxPerTabAdjustment);

			// if there is a maximum per tab adjustment then store the percent of 
			// that adjustment that could still be used for the element - i.e. the 
			// percent of the adjustment that it could still be increased by. the
			// reason for storing it in this way is to make it easier to use this
			// as the opacity for the separator
			if (maxPerTabAdjustment > 0)
			{
				for (int i = 0, tabCount = tabs.Count; i < tabCount; i++)
				{
					TabItemInfo tab = sortedTabs[i];

					// if we have allocated less than the maximum then there is some percent
					// remaining and we need to cache that percent
					if (tab.AdjustmentMade > 0)
					{
						double percentRemaining = tab.AdjustmentMade / maxPerTabAdjustment;
						object opacity = percentRemaining > .3d ? KnownBoxes.DoubleZeroBox : 1d - (percentRemaining / .3);
						tab.Element.SetValue(TabSeparatorOpacityPropertyKey, opacity);
					}
				}
			}

			return totalAdjustmentsMade;
		}

		private double AdjustTabsProportionallyImpl(List<TabItemInfo> sortedTabs,
			bool hasMultiplePriorities,
			bool isVertical,
			double adjustment,
			double maxPerTabAdjustment)
		{
			bool expanding = adjustment > 0;
			int tabCount = sortedTabs.Count;
			double totalAdjustmentsMade = 0;
			int startingIndex = 0;
			int tabsWithPriority;
			int? priority;

			// AS 7/12/12 TFS117129
			int previousStartingIndex = -1;
			int previousTabsWithPriority = 0;

			// AS 1/9/08 BR29523
			//while (adjustment != 0 && startingIndex < tabCount)
			while (false == CoreUtilities.AreClose(adjustment, 0) && startingIndex < tabCount)
			{
				priority = null;
				tabsWithPriority = 0;
				int i = startingIndex;

				// AS 10/25/07
				double currentExtent = 0d;
				double? nextExtent = null;

				for (; i < tabCount; i++)
				{
					TabItemInfo tab = sortedTabs[i];

					if (double.IsNaN(tab.Extent))
						continue;
					else if (priority == null)
					{
						priority = tab.Priority;
						tabsWithPriority++;

						// AS 10/25/07
						// Keep track of the current extent of this tab since we only
						// want to process the tabs with this size so that we can decrease
						// the largest tabs first and then continue with these tabs and the 
						// next set of tabs.
						//
						currentExtent = tab.Extent;
					}
					else if (priority != tab.Priority)
						break;
					// AS 10/25/07
					else if (expanding == false && CoreUtilities.AreClose(tab.Extent, currentExtent) == false)
					{
						// AS 7/12/12 TFS117129
						// To avoid a case where we get stuck processing the same item over and over, we'll 
						// make sure we process at least as many as we processed on the last iteration.
						//
						if ( previousStartingIndex == startingIndex && previousTabsWithPriority >= tabsWithPriority )
						{
							currentExtent = tab.Extent;
							tabsWithPriority++;
						}
						else
						{
							nextExtent = tab.Extent;
						}
						break;
					}
					else
						tabsWithPriority++;
				}

				// AS 7/12/12 TFS117129
				// Keep track of how many items we processed and which we started with.
				//
				previousStartingIndex = startingIndex;
				previousTabsWithPriority = tabsWithPriority;

				if (tabsWithPriority > 0)
				{
					// AS 10/25/07
					// Resize this set of tabs down to the next extent.
					//
					double sign = adjustment < 0 ? -1d : 1d;
					// AS 1/4/08 BR29413
					// Since we're treating the adjustment as absolute and then we also need to do that with 
					// the difference between the extent being processed and the next extent or we could
					// end up trying to adjust more than what we were asked to adjust.
					//
					//double currentAdjustment = expanding == false && nextExtent != null ? sign * Math.Min(Math.Abs(adjustment), (currentExtent - (double)nextExtent) * tabsWithPriority) : adjustment;
					double currentAdjustment = expanding == false && nextExtent != null ? sign * Math.Min(Math.Abs(adjustment), Math.Abs(currentExtent - (double)nextExtent) * tabsWithPriority) : adjustment;
					
					double adjustmentMade = AdjustTabsProportionallyImpl(sortedTabs,
						isVertical,
						startingIndex,
						tabsWithPriority,
						// AS 10/25/07
						//adjustment,
						currentAdjustment,
						expanding,
						maxPerTabAdjustment);

					// then reduce the adjustment and see if there is any adjustment that 
					// can be made to the other priority level.
					adjustment -= adjustmentMade;
					totalAdjustmentsMade += adjustmentMade;

					// AS 10/25/07
					// If we're reducing the tabs then we're just processing all the items with 
					// a particular size and since we're not done with this priority keep starting
					// with the current starting index.
					//
					// AS 1/9/08 BR29523
					//if (adjustmentMade != 0d && expanding == false && nextExtent != null)
					if (expanding == false && nextExtent != null && false == CoreUtilities.AreClose(adjustmentMade, 0))
						continue;
				}

				startingIndex = i;
			}

			return totalAdjustmentsMade;
		}

		private double AdjustTabsProportionallyImpl(List<TabItemInfo> tabs,
			bool isVertical,
			int startIndex,
			int tabsToAdjust,
			double adjustment,
			bool expanding,
			double maxPerTabAdjustment)
		{
			int tabsAdjusted = 0;
			double tabAdjustment = 0;
			double tabExtent, minExtent;
			int tabCount = tabs.Count;
			double totalAdjustmentMade = 0;

			int count = tabs.Count;

			for (int i = startIndex; i < tabCount; i++)
			{
				TabItemInfo tab = tabs[i];

				if (double.IsNaN(tab.Extent))
					continue;

				tabExtent = tab.Extent;
				minExtent = tab.MinExtent;

				tabAdjustment = adjustment / (tabsToAdjust - tabsAdjusted);

				if (expanding && maxPerTabAdjustment > 0 && tabAdjustment > maxPerTabAdjustment)
					tabAdjustment = maxPerTabAdjustment;
				else if (false == expanding && maxPerTabAdjustment < 0 && tabAdjustment < maxPerTabAdjustment)
					tabAdjustment = maxPerTabAdjustment;

				tabExtent += tabAdjustment;

				// enforce the max tab width
				if (tabExtent < minExtent)
				{
					tabAdjustment += minExtent - tabExtent;
					tabExtent += minExtent - tabExtent;
				}

				tab.Extent = tabExtent;
				tab.AdjustmentMade += tabAdjustment;

				totalAdjustmentMade += tabAdjustment;

				adjustment -= tabAdjustment;
				tabsAdjusted++;

				if (tabsAdjusted == tabsToAdjust || adjustment == 0)
					return totalAdjustmentMade;

				// JJD 3/25/03
				// If we are expanding return when adjustement goes negative
				// otherwise return when adjustement goes positiove
				if (expanding == true)
				{
					if (adjustment < 0)
						return totalAdjustmentMade;
				}
				else
				{
					if (adjustment > 0)
						return totalAdjustmentMade;
				}
			}

			return totalAdjustmentMade;

		}

		#endregion //AdjustTabsProportionally

		#region AdjustVerticalOffset
		private void AdjustVerticalOffset(double adjustment, bool adjustForOrientation)
		{
			this.SetVerticalOffset(((IScrollInfo)this).VerticalOffset + adjustment, true);
		}
		#endregion // AdjustVerticalOffset

        // JJD 8/29/08 - added
        #region BringSelectedTabIntoView

        private delegate void MethodInvoker();

        // JJD 8/29/08
        // If the alignTabRowToSelectedTab is true, set when a property is changed that effects tab layout,
        // then call BringSelectedTabIntoView asynchronously
        private void BringSelectedTabIntoView()
        {
            Selector selector = ItemsControl.GetItemsOwner(this) as Selector;

            if (null != selector && selector.Items.Count > 0)
            {
                int index = selector.SelectedIndex;

                if (index < 0)
                    index = 0;

                Visual container = selector.ItemContainerGenerator.ContainerFromIndex(index) as Visual;

                if (container != null)
                    ((IScrollInfo)this).MakeVisible(container, new Rect(new Point(), new Size(this.ActualWidth, this.ActualHeight)));
            }
        }
            
        #endregion //BringSelectedTabIntoView	
    
		#region GetScrollAdjustment
		private double GetScrollAdjustment(double adjustment)
		{
			if (false == this.IsMultiRow)
				adjustment *= LineOffset;

            // JJD 8/28/08
            // flip the sign on the adjustemt if we are inverting the offset
            if (this.InvertsHorizontalOffset || this.InvertsVerticalOffset)
                return -adjustment;
            else
			    return adjustment;
		}
		#endregion //GetScrollAdjustment

		#region GetMinExtent
		private double GetMinExtent(UIElement tabElement, bool isVertical)
		{
			FrameworkElement frameworkElement = tabElement as FrameworkElement;
			double minExtent;

			if (frameworkElement != null)
				minExtent = isVertical ? frameworkElement.MinHeight : frameworkElement.MinWidth;
			else
				minExtent = double.NaN;

			double defaultMin = this.MinimumTabExtent;

			if (double.IsNaN(minExtent))
				minExtent = defaultMin;
			else if (minExtent < defaultMin)
				minExtent = defaultMin;

			return minExtent;
		}
		#endregion //GetMinExtent

		#region GetTabItemRowNumber
		private int GetTabItemRowNumber(object item)
		{
			int index = 0;

			Selector selector = ItemsControl.GetItemsOwner(this) as Selector;

			if (null != selector && this._rows.Count > 1)
			{
				UIElement selectedElement = selector.ItemContainerGenerator.ContainerFromItem(item) as UIElement;

				if (null != selectedElement)
				{
					
					TabItemInfo tabInfo = (TabItemInfo)selectedElement.GetValue(TabInfoProperty);

					if (null != tabInfo)
						return this._rows.IndexOf(tabInfo.Row);
				}
			}

			return index;
		}
		#endregion //GetTabItemRowNumber

		#region HookSelector
		private void HookSelector(Selector selector)
		{
			this.UnhookSelector();

			this._hookedSelector = selector;

			if (null != selector)
				selector.SelectionChanged += new SelectionChangedEventHandler(OnSelectorSelectionChanged);
		} 
		#endregion // HookSelector

		#region IsMultiRowStyle
		internal static bool IsMultiRowStyle(TabLayoutStyle style)
		{
			switch (style)
			{
                case TabLayoutStyle.MultiRowAutoSize:
                case TabLayoutStyle.MultiRowSizeToFit:
                    return true;
				case TabLayoutStyle.SingleRowAutoSize:
				case TabLayoutStyle.SingleRowJustified:
				case TabLayoutStyle.SingleRowSizeToFit:
					return false;
				default:
					Debug.Fail("Unexepected style:" + style.ToString());
					return false;
			}
		}
		#endregion //IsMultiRowStyle

        // JJD 9/2/08 - added
        #region OnLayoutUpdated

        void OnLayoutUpdated(object sender, EventArgs e)
        {
            this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);
            this.BringSelectedTabIntoView();
        }

        #endregion //OnLayoutUpdated	
    
		#region OnSelectorSelectionChanged
		void OnSelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
		}
		#endregion //OnSelectorSelectionChanged

		#region SetHorizontalOffset
		private void SetHorizontalOffset(double offset, bool adjustForOrientation)
		{
			if (this.IsScrollClient)
			{
				double newOffset;

				if (this.ScrollDataInfo._viewport.Width >= this.ScrollDataInfo._extent.Width)
					newOffset = 0;
				else
				{
					// largest offset value
                    // JJD 9/2/08
                    // Use MaxHorizontalOffset property
                    //double max = Math.Max(0, this.ScrollDataInfo._extent.Width - this.ScrollDataInfo._viewport.Width);
                    double max = this.MaxHorizontalOffset;

					// if this is a multirow tabs where the tabs are on top/bottom (i.e. vertical scrolling)
                    // then round the offset value
                    if (this.IsMultiRow && this.IsVertical == true)
                        offset = Math.Round(offset, 0);

					// for multirow tabs on top or single row tabs on left, we need to invert the value
					if (adjustForOrientation && this.InvertsHorizontalOffset)
					{
						offset = max - offset;
					}

					newOffset = Math.Max(Math.Min(offset, max), 0);
				}

				if (newOffset != this.ScrollDataInfo._offset.X)
				{
					this.ScrollDataInfo._offset.X = newOffset;
					this.InvalidateArrange();
				}
			}
		}
		#endregion // SetHorizontalOffset

		#region SetVerticalOffset
		private void SetVerticalOffset(double offset, bool adjustForOrientation)
		{
            Debug.WriteLine("SetVerticalOffset offset: " + offset.ToString() + ", adjust: " + adjustForOrientation.ToString());
			if (this.IsScrollClient)
			{
				double newOffset;

				if (this.ScrollDataInfo._viewport.Height >= this.ScrollDataInfo._extent.Height)
					newOffset = 0;
				else
				{
					// largest offset value
                    // JJD 9/2/08
                    // Use MaxVerticalOffset property
                    //double max = Math.Max(0, this.ScrollDataInfo._extent.Height - this.ScrollDataInfo._viewport.Height);
					double max = this.MaxVerticalOffset;

					// if this is a multirow tabs where the tabs are on top/bottom (i.e. vertical scrolling)
                    // then round the offset value
                    if (this.IsMultiRow && this.IsVertical == false)
                        offset = Math.Round(offset, 0);
 
					if (adjustForOrientation && this.InvertsVerticalOffset)
					{
						offset = max - offset;
					}

					newOffset = Math.Max(Math.Min(offset, max), 0);
				}

				if (newOffset != this.ScrollDataInfo._offset.Y)
				{
					this.ScrollDataInfo._offset.Y = newOffset;
					this.InvalidateArrange();
				}
			}
		}
		#endregion // SetVerticalOffset

		#region UnhookSelector
		private void UnhookSelector()
		{
			if (null != this._hookedSelector)
			{
				this._hookedSelector.SelectionChanged += new SelectionChangedEventHandler(OnSelectorSelectionChanged);
				this._hookedSelector = null;
			}
		} 
		#endregion // UnhookSelector

		#region UpdateExtents
		private void UpdateExtents(double extent)
		{
			this._lastExtent = extent;
			bool isVertical = this.IsVertical;
			List<TabRowInfo> rows = this._rows;
			TabRowInfo row = null;
			List<TabItemInfo> tabs = null;
			TabItemInfo tab = null;
			TabLayoutStyle layoutStyle = this.TabLayoutStyle;
			double interTabSpacing = this.InterTabSpacing;

			// start by initializing the extents to the desired extents
			for (int rowIndex = 0, rowCount = rows.Count; rowIndex < rowCount; rowIndex++)
			{
				row = rows[rowIndex];
				tabs = row.Tabs;
				double rowExtent = 0;
				int firstPriority = tabs.Count > 0 ? XamTabControl.GetTabPriority(tabs[0].Element) : 0;
				bool hasSamePriority = true;

				for (int i = 0, count = tabs.Count; i < count; i++)
				{
					tab = tabs[i];

					// clear the max size percent
					tab.AdjustmentMade = 0d;
					tab.Element.ClearValue(TabSeparatorOpacityPropertyKey);

					if (tab.Element.Visibility == Visibility.Collapsed)
						tab.Extent = double.NaN;
					else
					{
						double tabExtent = isVertical ? tab.Element.DesiredSize.Height : tab.Element.DesiredSize.Width;
						tab.Extent = Math.Max(tabExtent, this.GetMinExtent(tab.Element, isVertical));
					}

					tab.Priority = XamTabControl.GetTabPriority(tab.Element);
					tab.MinExtent = this.GetMinExtent(tab.Element, isVertical);
					rowExtent += tab.Extent + interTabSpacing;

					if (hasSamePriority && firstPriority != tab.Priority)
						hasSamePriority = false;
				}

				row.CurrentExtent = rowExtent - interTabSpacing;
				row.HasMultiplePriorities = hasSamePriority == false;
			}

			// nothing further to do for autosized tabs
			if (layoutStyle == TabLayoutStyle.SingleRowAutoSize)
				return;

            if (layoutStyle == TabLayoutStyle.MultiRowAutoSize)
                return;

			// now for certain layout styles, we need to update the extents
			// based on the available extent
			// start by initializing the extents to the desired extents
			for (int rowIndex = 0, rowCount = rows.Count; rowIndex < rowCount; rowIndex++)
			{
				row = rows[rowIndex];
				tabs = row.Tabs;

				switch (layoutStyle)
				{
					case TabLayoutStyle.SingleRowJustified:
						if (row.CurrentExtent > extent)
							this.AdjustTabsProportionally(tabs, row.HasMultiplePriorities, isVertical, extent - row.CurrentExtent, 0);
						break;
					case TabLayoutStyle.MultiRowSizeToFit:
					case TabLayoutStyle.SingleRowSizeToFit:
						if (row.CurrentExtent < extent)
							this.AdjustTabsProportionally(tabs, row.HasMultiplePriorities, isVertical, extent - row.CurrentExtent, this.MaximumSizeToFitAdjustment);
						else if (row.CurrentExtent > extent)
							this.AdjustTabsProportionally(tabs, row.HasMultiplePriorities, isVertical, extent - row.CurrentExtent, 0);
						break;
				}
			}
		}
		#endregion //UpdateExtents

		#endregion //Methods

		#region TabItemInfo class
		private class TabItemInfo
		{
			internal TabItemInfo(UIElement element, int childIndex, TabRowInfo row)
			{
				Debug.Assert(element != null, "The TabItemInfo MUST have an element");

				this.ChildIndex = childIndex;
				this.Element = element;
				this.Row = row;
			}

			internal int ChildIndex;
			internal double Extent;
			internal double AdjustmentMade;
			internal double MinExtent;
			internal UIElement Element;
			internal int Priority;
			internal TabRowInfo Row;
		}
		#endregion //TabItemInfo class

		#region TabRowInfo class
		private class TabRowInfo
		{
			internal double CurrentExtent;
			internal double PreferredExtent;
			internal bool HasMultiplePriorities;
			internal List<TabItemInfo> Tabs = new List<TabItemInfo>();
		}
		#endregion //TabRowInfo class

		#region TabPriorityComparer
		private class TabPriorityComparer : IComparer<TabItemInfo>
		{
			#region Member Variables

			private static TabPriorityComparer instance;

			#endregion //Member Variables

			#region Constructor
			static TabPriorityComparer()
			{
				instance = new TabPriorityComparer();
			}

			private TabPriorityComparer()
			{
			}
			#endregion //Constructor

			#region Properties
			public static IComparer<TabItemInfo> Instance
			{
				get { return TabPriorityComparer.instance; }
			}
			#endregion //Properties

			#region IComparer<TabInfo> Members

			public int Compare(TabItemInfo x, TabItemInfo y)
			{
				// AS 10/25/07
				// We should sort by the extent and index if the priority is the same
				// returning the one with the larger extent first or the later index.
				//
				//return x.Priority.CompareTo(y.Priority);
				int priorityCompare = x.Priority.CompareTo(y.Priority);

				if (priorityCompare != 0)
					return priorityCompare;

				if (x.Extent != y.Extent)
					return y.Extent.CompareTo(x.Extent);

				return y.ChildIndex.CompareTo(x.ChildIndex);
			}

			#endregion //IComparer<TabInfo> Members
		}
		#endregion //TabPriorityComparer

		#region ScrollDataInfo

		private ScrollData _scrollDataInfo;

		private ScrollData ScrollDataInfo
		{
			get
			{
                if (this._scrollDataInfo == null)
                {
                    // JJD 9/2/08 - added back ref to panel
                    //this._scrollDataInfo = new ScrollData();
                    this._scrollDataInfo = new ScrollData(this);
                }

				return this._scrollDataInfo;
			}
		}
		#endregion //ScrollDataInfo

		#region IScrollInfo Members

		bool IScrollInfo.CanHorizontallyScroll
		{
			get
			{
				return this.IsScrollClient ? this.ScrollDataInfo._canHorizontallyScroll : false;
			}
			set
			{
				if (this.IsScrollClient)
					this.ScrollDataInfo._canHorizontallyScroll = value;
			}
		}

		bool IScrollInfo.CanVerticallyScroll
		{
			get
			{
				return this.IsScrollClient ? this.ScrollDataInfo._canVerticallyScroll : false;
			}
			set
			{
				if (this.IsScrollClient)
					this.ScrollDataInfo._canVerticallyScroll = value;
			}
		}

		double IScrollInfo.ExtentHeight
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Height : 0; }
		}

		double IScrollInfo.ExtentWidth
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._extent.Width : 0; }
		}

		double IScrollInfo.HorizontalOffset
		{
			get
			{
				double offset = 0;

				if (this.IsScrollClient)
				{
					offset = this.ScrollDataInfo._offset.X;

                    if (this.InvertsHorizontalOffset)
                    {
                        // JJD 8/26/08
                        // For multi-row we want to allow scrolling to last row
                        //offset = (this.ScrollDataInfo._extent.Width - this.ScrollDataInfo._viewport.Width) - offset;
                        offset = (this.ScrollDataInfo._extent.Width) - offset;
                    }
				}

				return offset;
			}
		}

		void IScrollInfo.LineDown()
		{
			this.AdjustVerticalOffset(this.GetScrollAdjustment(1), true);
		}

		void IScrollInfo.LineLeft()
		{
			this.AdjustHorizontalOffset(this.GetScrollAdjustment(-1), true);
		}

		void IScrollInfo.LineRight()
		{
			this.AdjustHorizontalOffset(this.GetScrollAdjustment(1), true);
		}

		void IScrollInfo.LineUp()
		{
			this.AdjustVerticalOffset(this.GetScrollAdjustment(-1), true);
		}

		Rect IScrollInfo.MakeVisible(System.Windows.Media.Visual visual, Rect rectangle)
		{
			if (this.IsScrollClient)
			{
				if (this.IsMultiRow)
				{
					TabItem tab = visual as TabItem;

					if (null == tab)
						tab = (TabItem)Utilities.GetAncestorFromType(visual, typeof(TabItem), true, this);

					if (tab != null)
					{
						TabItemInfo tabInfo = (TabItemInfo)tab.GetValue(TabInfoProperty);

						if (tabInfo != null)
						{
							int newIndex = this._rows.IndexOf(tabInfo.Row);

							if (newIndex >= 0)
							{
                                Debug.WriteLine("MakeVisible newIndex: " + newIndex.ToString());

								
								if (IsVertical)
									((IScrollInfo)this).SetHorizontalOffset(newIndex);
								else
									((IScrollInfo)this).SetVerticalOffset(newIndex);
							}
						}
					}
				}
				else
				{
					// AS 2/11/08 NA 2008 Vol 1
					// Since we now support scrolling a single row of tab items then we 
					// need to support bringing that element into view as well.
					//
					if (rectangle.IsEmpty || visual == null || this.IsAncestorOf(visual) == false)
						return Rect.Empty;

					// AS 1/6/10 TFS25834
					//Rect visualRect = visual.TransformToAncestor(this).TransformBounds(rectangle);
					GeneralTransform gt = visual.TransformToAncestor(this);

					if (gt == null)
						return Rect.Empty;

					Rect visualRect = gt.TransformBounds(rectangle);

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

						offsetX += ((IScrollInfo)this).HorizontalOffset;

						((IScrollInfo)this).SetHorizontalOffset(offsetX);
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

						offsetY += ((IScrollInfo)this).VerticalOffset;

						((IScrollInfo)this).SetVerticalOffset(offsetY);
					}

					return visualRect;
				}
			}

			return rectangle;
		}

		void IScrollInfo.MouseWheelDown()
		{
			this.AdjustVerticalOffset(this.GetScrollAdjustment(-SystemParameters.WheelScrollLines), true);
		}

		void IScrollInfo.MouseWheelLeft()
		{
			this.AdjustHorizontalOffset(this.GetScrollAdjustment(-SystemParameters.WheelScrollLines), true);
		}

		void IScrollInfo.MouseWheelRight()
		{
			this.AdjustHorizontalOffset(this.GetScrollAdjustment(SystemParameters.WheelScrollLines), true);
		}

		void IScrollInfo.MouseWheelUp()
		{
			this.AdjustVerticalOffset(this.GetScrollAdjustment(SystemParameters.WheelScrollLines), true);
		}

		void IScrollInfo.PageDown()
		{
			this.AdjustVerticalOffset(this.ScrollDataInfo._viewport.Height, true);
		}

		void IScrollInfo.PageLeft()
		{
			this.AdjustHorizontalOffset(-this.ScrollDataInfo._viewport.Width, true);
		}

		void IScrollInfo.PageRight()
		{
			this.AdjustHorizontalOffset(this.ScrollDataInfo._viewport.Width, true);
		}

		void IScrollInfo.PageUp()
		{
			this.AdjustVerticalOffset(-this.ScrollDataInfo._viewport.Height, true);
		}

		ScrollViewer IScrollInfo.ScrollOwner
		{
			get
			{
				return this.IsScrollClient ? this.ScrollDataInfo._scrollOwner : null;
			}
			set
			{
				// AS 2/11/08 NA 2008 Vol 1
				// Whether we are in control of scrolling has a bearing on our measurement
				// so make sure we invalidate the measure if changed.
				//
				if (value != this.ScrollDataInfo._scrollOwner)
				{
					this.ScrollDataInfo._scrollOwner = value;
					this.InvalidateMeasure();
				}
			}
		}

		void IScrollInfo.SetHorizontalOffset(double offset)
		{
			this.SetHorizontalOffset(offset, true);
		}

		void IScrollInfo.SetVerticalOffset(double offset)
		{
			this.SetVerticalOffset(offset, true);
		}

		double IScrollInfo.VerticalOffset
		{
			get
			{
				double offset = 0;

				if (this.IsScrollClient)
				{
					offset = this.ScrollDataInfo._offset.Y;

                    if (this.InvertsVerticalOffset)
                    {
                        // JJD 8/26/08
                        // For multi-row we want to allow scrolling to last row
                        //offset = (this.ScrollDataInfo._extent.Height - this.ScrollDataInfo._viewport.Height) - offset;
                        offset = (this.ScrollDataInfo._extent.Height) - offset;
                    }
				}

				return offset;
			}
		}

		double IScrollInfo.ViewportHeight
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Height : 0; }
		}

		double IScrollInfo.ViewportWidth
		{
			get { return this.IsScrollClient ? this.ScrollDataInfo._viewport.Width : 0; }
		}

		#endregion //IScrollInfo

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

            // JJD 9/2/08 - added back ref to panel
            private TabItemPanel _panel;

			#endregion //Member Variables

            #region Constructor

            // JJD 9/2/08 - added back ref to panel
            internal ScrollData(TabItemPanel panel)
            {
                this._panel = panel;
            }

            #endregion //Constructor	
    
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
					false == PagerContentPresenter.AreClose(this._extent.Height, extent.Height);

				this._viewport = viewPort;
				this._extent = extent;

                Debug.WriteLine("VerfifyScrollData viewport: " + this._viewport.ToString() + ", extent: " + this._extent.ToString());
                Debug.WriteLine("VerfifyScrollData offset before: " + this._offset.ToString());

				isDifferent |= this.VerifyOffset();
                
                Debug.WriteLine("VerfifyScrollData offset sfter: " + this._offset.ToString());

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
                // JJD 9/2/08
                // Use MaxHorizontalOffset and MaxVerticalOffset properties exposed off the TabItemPanel instead
                //double offsetX = Math.Max(0, Math.Min(this._offset.X, this._extent.Width - this._viewport.Width));
                //double offsetY = Math.Max(0, Math.Min(this._offset.Y, this._extent.Height - this._viewport.Height));
                double offsetX = Math.Max(Math.Min(this._offset.X, this._panel.MaxHorizontalOffset), 0);
                double offsetY = Math.Max(Math.Min(this._offset.Y, this._panel.MaxVerticalOffset), 0);
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