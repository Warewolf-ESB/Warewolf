using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A custom panel that positions items from left to right breaking to a new line based on the constraining width.
	/// </summary>
	/// <remarks>
	/// <p class="body">The <b>GalleryWrapPanel</b> is a custom panel that arranges items from left to right wrapping to 
	/// the next line when there is not enough room to fit the remaining items. The panel will position the items in at 
	/// least as many columns as specified by the <see cref="MinColumns"/> regardless of space and will likely use at most 
	/// <see cref="MaxColumns"/> even if there is more than enough space. You may control how many columns a single item will 
	/// span using the <see cref="SetColumnSpan(DependencyObject, Int32)"/> attached property.</p>
	/// <p class="body">The widths of the columns and the heights of the rows are uniform. By default, the height of a row 
	/// is based on the height of the largest item in the panel. The default width of a row is calculated based on the width 
	/// of the largest item taking their <see cref="ColumnSpanProperty"/> into account. You may force an explicit size for the 
	/// column width and row height using the <see cref="ColumnWidth"/> and <see cref="RowHeight"/> properties respectively. 
	/// You may control the amount of spacing between columns using the <see cref="ColumnSpacing"/>.</p>
	/// <p class="note"><b>Note:</b> If the <see cref="MinColumns"/> is less than the largest <see cref="ColumnSpanProperty"/> 
	/// value item for an item in the panel then the maximum ColumnSpanProperty value will be used as the minimum. 
	/// Likewise, if the <see cref="MaxColumns"/> is less than the largest <b>ColumnSpanProperty</b> then the value of that 
	/// largest ColumnSpanProperty will be used as the maximum number of columns.</p>
	/// </remarks>
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class GalleryWrapPanel : Panel
	{
		#region Member Variables

		private int _calcPreferredCols;
		private Size _itemSize;
		private int _calcMinColumns;
		private int _calcMaxColumns;
		private int _calcRowCount;
		private int _columnCountForLastMeasure;
		private int? _columnCountForLastArrange;

        // JJD 11/26/07 - BR25892
        // Cache whether we are in a dropdown
        private bool _isInDropDown;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="GalleryWrapPanel"/>
		/// </summary>
		public GalleryWrapPanel()
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
			// find out how many columns we can fit
			double colSpacing = this.ColumnSpacing;
			int maxAvailableCols = (int)((finalSize.Width + colSpacing) / (this._itemSize.Width + colSpacing));

			// use as many columns as possible up but keep it in range
			int columnCount = Math.Max(this._calcMinColumns, Math.Min(this._calcMaxColumns, maxAvailableCols));
			int rowCount;

			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			// if we're going to arrange with a different number of columns than we measured with
			// than hook the layout updated so we can invalidate our measure and cache the number
			// of columns that the measure should use
			if (columnCount != this._columnCountForLastMeasure)
			{
				this._columnCountForLastArrange = columnCount;
				this.LayoutUpdated += new EventHandler(OnLayoutUpdated);
			}
			else
			{
				this._columnCountForLastArrange = null;
			}

			Size arrangeSize = this.DoLayout(columnCount, this._itemSize, true, finalSize.Height, out rowCount);

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
			// measure each item and get the maximum size
			UIElementCollection children = this.InternalChildren;
			int visibleItemCount = 0;
			int maxItemColSpan = 1;
			int colSpanTotal = 0;
			double colSpacing = this.ColumnSpacing;
			double rowSpacing = this.RowSpacing;

			double columnWidth = this.ColumnWidth;
			double rowHeight = this.RowHeight;

			bool hasFixedWidth = double.IsNaN(columnWidth) == false;
			bool hasFixedHeight = double.IsNaN(rowHeight) == false;

			Size maxItemSize = new Size();

			#region Calculate Item Size / Max ColumnSpan
			
			// always measure with infinity to find out the desired size - however if we
			// have a fixed width and/or height measure with that
			Size measureSize = new Size(hasFixedWidth ? columnWidth : double.PositiveInfinity, 
				hasFixedHeight ? rowHeight : double.PositiveInfinity);

			// JJD 11/26/07 - BR25891
            // Moved stack variable above for loop so we can cache the gallery tool if the item
            // are not in groups but directly in the GalleryTool 
            GalleryTool galleryToolInDropDown = null;

			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement child = children[i];

				if (child.Visibility == Visibility.Collapsed)
					continue;
                
                // JJD 11/26/07 - BR25891
                // If the child is a GalleryItemPresenter then cache the GalleryTool
                if ( this._isInDropDown &&
                    galleryToolInDropDown == null &&
                    child is GalleryItemPresenter)
                    galleryToolInDropDown = ((GalleryItemPresenter)child).GalleryTool;

				visibleItemCount++;

				int colSpan = (int)child.GetValue(GalleryWrapPanel.ColumnSpanProperty);

				// keep track of the largest single column span
				if (colSpan > maxItemColSpan)
					maxItemColSpan = colSpan;

				colSpanTotal += colSpan;

				// if the item width is specifically set then measure with a multiple of that
				if (hasFixedWidth)
				{
					measureSize.Width = ((ColumnWidth + colSpacing) * colSpan) - colSpacing;
				}

				child.Measure(measureSize);

				Size childSize = child.DesiredSize;

				// if the child is wider than 1 column then consider the
				// size it would consider each item to be a fraction of the
				// desired width.
				if (colSpan > 1 && childSize.Width > 0)
				{
					// remove the column spacing since that doesn't count
					// towards the width of a column
					double width = childSize.Width - ((colSpan - 1) * colSpacing);

					width /= colSpan;

					childSize.Width = Math.Max(width, 0);
				}

				if (childSize.Width > maxItemSize.Width)
					maxItemSize.Width = childSize.Width;

				if (childSize.Height > maxItemSize.Height)
					maxItemSize.Height = childSize.Height;
			}

            // JJD 11/26/07 - BR25891
            // Moved galleryToolInDropDown stack variable above for previous for loop.
            //GalleryTool galleryToolInDropDown = null;

            #region Old code

            //GalleryItemGroup parentGroup = ItemsControl.GetItemsOwner(this) as GalleryItemGroup;

            //// See if the ItemsOwner is a GalleryItemGroup which would mean we are in a dropdown
            //if (parentGroup != null)
            //{
            //    // Get the GalleryTool from the group
            //    galleryToolInDropDown = parentGroup.GalleryTool;

            //    // Use the MaxColumnSpan from all groups that is maintained by the GalleryTool
            //    if (galleryToolInDropDown != null)
            //        maxItemColSpan = (int)galleryToolInDropDown.GetValue(GalleryTool.MaxColumnSpanProperty);
            //}

            #endregion //Old code	
			
            GalleryItemGroup parentGroup = null;
    
            // JJD 11/26/07 - BR25891
            // Only look for a GalleryItemGroup if we didn't encounter a GalleryItemPresenter as a direct child
            if ( galleryToolInDropDown == null &&
                 this._isInDropDown)
            {
			    parentGroup = ItemsControl.GetItemsOwner(this) as GalleryItemGroup;

			    // See if the ItemsOwner is a GalleryItemGroup which would mean we are in a dropdown
			    if (parentGroup != null)
				    // Get the GalleryTool from the group
				    galleryToolInDropDown = parentGroup.GalleryTool;
            }

			// Use the MaxColumnSpan from all groups that is maintained by the GalleryTool
			if (galleryToolInDropDown != null)
				maxItemColSpan = (int)galleryToolInDropDown.GetValue(GalleryTool.MaxColumnSpanProperty);

			#endregion //Calculate Item Size / Max ColumnSpan

			// the number of columns must be at least as wide as the largest single item column span
			int minCols = Math.Max(this.MinColumns, maxItemColSpan);
			int maxCols = Math.Max(this.MaxColumns == 0 ? int.MaxValue : this.MaxColumns, maxItemColSpan);
			int prefCols = Math.Max(this.PreferredColumns, maxItemColSpan);

			// cannot span more than the total number of column slots
			maxCols = Math.Min(colSpanTotal, maxCols);
			prefCols = Math.Max(minCols, Math.Min(prefCols, maxCols));

			// if we had a fixed width/height then use that for the item size calculations
			if (hasFixedWidth)
				maxItemSize.Width = columnWidth;

			if (hasFixedHeight)
				maxItemSize.Height = rowHeight;

            // JJD 11/28/07 - BR27268
            // Moved logic to DoLayout method
            #region Moved code

            //double extraWidth = 0;
            //double extraHeight = 0;

            //// JJD 11/26/07 - BR25892
            //// Allow for any extra chrome between the panel and the gallerytool (e.g. scrollbars)
            //// Note: the 1st time thru the measure this won't be accurate but since this info is
            //// only used for resizing then it will self correct on the next pass.
            //if (galleryToolInDropDown != null)
            //{
            //    extraWidth = galleryToolInDropDown.DesiredSize.Width - this.DesiredSize.Width;
            //    extraHeight = galleryToolInDropDown.DesiredSize.Height - this.DesiredSize.Height;
            //}

            #endregion //Moved code	
    
 			// the minimum size is simply the size of one row
			Size minSize = new Size((maxItemSize.Width * minCols), maxItemSize.Height);
			int rowCount = 0;
			// AS 11/14/07 BR28454
			// The arrange will think we have 1 column - which is correct since that is the minimum
			// but since we didn't have any visible items, the columnCountForDesiredSize was left
			// at 0 in which case we hooked the layoutupdated and invalidated the measure. The invalidation
			// caused the problem but we shouldn't have invalidated in this case.
			//
			//int columnCountForDesiredSize = 0;
			int columnCountForDesiredSize = minCols;

			#region Calculate Desired Size
			// the desired size requires an iteration to see how many rows we would need
			Size desiredSize;

			if (visibleItemCount == 0)
				desiredSize = new Size();
			else
			{
				// by default measure for the preferred number of columns
				columnCountForDesiredSize = prefCols;

				// but if we're given a specific width to measure to then we should
				// use the most columns we can fit in that space.
				if (double.IsInfinity(availableSize.Width) == false)
				{
					// find out how many columns we can fit
					int maxAvailableCols = (int)((availableSize.Width + colSpacing) / (maxItemSize.Width + colSpacing));

					// use as many columns as possible up but keep it in range
					columnCountForDesiredSize = Math.Max(minCols, Math.Min(maxCols, maxAvailableCols));
				}
				else if (this._columnCountForLastArrange != null)
					columnCountForDesiredSize = this._columnCountForLastArrange.Value;

				if (maxItemColSpan == 1)
				{
					double width = ((maxItemSize.Width + colSpacing) * columnCountForDesiredSize) - colSpacing;
					rowCount = visibleItemCount / columnCountForDesiredSize;
					if (visibleItemCount % columnCountForDesiredSize > 0)
						rowCount++;
					double height = (rowCount * maxItemSize.Height) + ((rowCount - 1) * rowSpacing);
					desiredSize = new Size(width, height);
				}
				else
				{
					// we actually need to do an arrange and calculate the sizes
					// because some items could cause gaps
					desiredSize = this.DoLayout(columnCountForDesiredSize, maxItemSize, false, double.PositiveInfinity, out rowCount);
				}
			} 
			#endregion //Calculate Desired Size

			#region PopupResizerDecorator

            // JJD 11/28/07 - BR27268
            // Added member to cache the mesured size of an item.
            if (galleryToolInDropDown != null)
                galleryToolInDropDown.MeasuredItemSize = minSize;


            // JJD 11/28/07 - BR27268
            // Moved logic to DoLayout method
            #region Moved code

            //if (galleryToolInDropDown != null)
            //{
            //    PopupResizerDecorator.Constraints constraints = PopupResizerDecorator.GetResizeConstraints(galleryToolInDropDown);

            //    if (constraints == null)
            //    {
            //        constraints = new PopupResizerDecorator.Constraints();

            //        PopupResizerDecorator.SetResizeConstraints(galleryToolInDropDown, constraints);
            //    }

            //    constraints.MinimumWidth = minSize.Width;

            //    // JJD 11/26/07 - BR25891
            //    // only add the etra space if we have groups
            //    //constraints.MinimumHeight = minSize.Height + 15; // allow some extra space for a possible group header
            //    double minheight = minSize.Height;

            //    // JJD 11/26/07 - BR25891
            //    // allow some extra space for a possible group header if we have groups
            //    if (parentGroup != null)
            //        minheight += 15;

            //    constraints.MinimumHeight = minheight; 
            //}

            #endregion //Moved code	
    
			#endregion //PopupResizerDecorator

			#region Gallery Resizing

			// gallery resizing logic
			GalleryToolPreviewPresenter itemsControl = ItemsControl.GetItemsOwner(this) as GalleryToolPreviewPresenter;

			if (itemsControl != null)
			{
				GalleryTool gallery = itemsControl.GalleryTool;

				if (null != gallery)
					gallery.SetValue(GalleryTool.MaxPossiblePreviewColumnsPropertyKey, colSpanTotal);
			} 

			#endregion //Gallery Resizing

			// store the calculated values for use in the arrange
			this._itemSize = maxItemSize;
			this._calcMaxColumns = maxCols;
			this._calcMinColumns = minCols;
			this._calcPreferredCols = prefCols;
			this._calcRowCount = rowCount;
			this._columnCountForLastMeasure = columnCountForDesiredSize;

			return desiredSize;
		}

		#endregion //MeasureOverride

		#region OnVisualChildrenChanged
		/// <summary>
		/// Invoked when a child is added/removed.
		/// </summary>
		/// <param name="visualAdded">The child that was added</param>
		/// <param name="visualRemoved">The child that was removed</param>
		protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
		{
			// AS 11/4/11 TFS89335
			// When a child is added/removed, the panel's measure was being invalidated but since it 
			// was measured with a constrained size, its desired size was still based upon that size.
			// However we were getting arranged with a wider size which got us into an endless 
			// cycle of invalidating measures in the layout updated. We need to dirty the measure 
			// of all the elements up to the popupresizedecorator so that we can be remeasured 
			// with the newer measure constraint.
			//
			if (_isInDropDown && this.IsMeasureValid)
			{
				// in case we have constraints clear them
				this.ClearValue(PopupResizerDecorator.ResizeConstraintsProperty);

				DependencyObject ancestor = this;

				while (ancestor != null)
				{
					UIElement ancestorElement = ancestor as UIElement;

					if (null != ancestorElement)
						ancestorElement.InvalidateMeasure();

					ancestor = System.Windows.Media.VisualTreeHelper.GetParent(ancestor);
				}
			}

			base.OnVisualChildrenChanged(visualAdded, visualRemoved);
		} 
		#endregion //OnVisualChildrenChanged

        // JJD 11/26/07 - BR25892
        #region OnVisualParentChanged

        /// <summary>
        /// Invoked when the parent element of this element reports a change to its underlying visual parent.
        /// </summary>
        /// <param name="oldParent">The previous parent. This may be null if the element did not have a parent element previously.</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);

            // JJD 11/26/07 - BR25892
            // Cache whether we are in a dropdown
            this._isInDropDown = Utilities.GetAncestorFromType(this, typeof(GalleryToolDropDownPresenter), true) != null;
        }

        #endregion //OnVisualParentChanged

        #endregion //Base class overrides

		#region Properties

		#region Attached Properties

		#region ColumnSpan

		/// <summary>
		/// Identifies the ColumnSpan attached dependency property
		/// </summary>
		/// <seealso cref="GetColumnSpan"/>
		/// <seealso cref="SetColumnSpan"/>
		public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.RegisterAttached("ColumnSpan",
			typeof(int), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(1), new ValidateValueCallback(ValidateIsPositiveInt));

		/// <summary>
		/// Determines the number of columns that the specified item within the collection should span.
		/// </summary>
		/// <seealso cref="ColumnSpanProperty"/>
		/// <seealso cref="SetColumnSpan"/>
		[AttachedPropertyBrowsableForChildren()]
		public static int GetColumnSpan(DependencyObject d)
		{
			return (int)d.GetValue(GalleryWrapPanel.ColumnSpanProperty);
		}

		/// <summary>
		/// Sets the value of the 'ColumnSpan' attached property
		/// </summary>
		/// <seealso cref="ColumnSpanProperty"/>
		/// <seealso cref="GetColumnSpan"/>
		public static void SetColumnSpan(DependencyObject d, int value)
		{
			d.SetValue(GalleryWrapPanel.ColumnSpanProperty, value);
		}

		#endregion //ColumnSpan

		#endregion //Attached Properties

		#region Public Properties

		#region ColumnSpacing

		/// <summary>
		/// Identifies the <see cref="ColumnSpacing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColumnSpacingProperty = DependencyProperty.Register("ColumnSpacing",
			typeof(double), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateIsAtLeastZero));

		/// <summary>
		/// Returns/sets the amount of space between columns in device independent pixels.
		/// </summary>
		/// <seealso cref="ColumnSpacingProperty"/>
		//[Description("Returns/sets the amount of space between columns in device independent pixels.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double ColumnSpacing
		{
			get
			{
				return (double)this.GetValue(GalleryWrapPanel.ColumnSpacingProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.ColumnSpacingProperty, value);
			}
		}

		#endregion //ColumnSpacing

		#region ColumnWidth

		/// <summary>
		/// Identifies the <see cref="ColumnWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColumnWidthProperty = DependencyProperty.Register("ColumnWidth",
			typeof(double), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateIsPositiveDouble));

		/// <summary>
		/// Returns/sets the preferred width for the columns.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, this property is set to <see cref="Double.NaN"/> and the column width 
		/// will be calculated based on the width of the largest item in the panel taking their column span 
		/// (<see cref="ColumnSpanProperty"/>) into account.</p>
		/// </remarks>
		/// <seealso cref="ColumnWidthProperty"/>
		/// <seealso cref="RowHeight"/>
		/// <seealso cref="ColumnSpacing"/>
		/// <seealso cref="RowSpacing"/>
		//[Description("Returns/sets the preferred width for the columns.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double ColumnWidth
		{
			get
			{
				return (double)this.GetValue(GalleryWrapPanel.ColumnWidthProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.ColumnWidthProperty, value);
			}
		}

		#endregion //ColumnWidth

		#region MaxColumns

		/// <summary>
		/// Identifies the <see cref="MaxColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxColumnsProperty = DependencyProperty.Register("MaxColumns",
			typeof(int), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure,
			new PropertyChangedCallback(OnMaxColumnsChanged), new CoerceValueCallback(OnCoerceMaxColumns)), new ValidateValueCallback(ValidateIsAtLeastZeroInt));

		private static void OnMaxColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GalleryWrapPanel panel = (GalleryWrapPanel)d;
			panel.CoerceValue(PreferredColumnsProperty);

			// gallery resizing logic - since the measures of the parent chain is not invalidated
			// when the elements are invalidated within a measure context, we need to explicitly
			// invalidate the leaf elements. this is normally handled within the ribbongrouppanle
			// but it does not have a reference to this panel so the panel will need to do this
			// on its behalf.
			GalleryToolPreviewPresenter itemsControl = ItemsControl.GetItemsOwner(panel) as GalleryToolPreviewPresenter;

			if (itemsControl != null)
			{
				RibbonGroupPanel groupPanel = (RibbonGroupPanel)Utilities.GetAncestorFromType(panel, typeof(RibbonGroupPanel), true);

				if (null != groupPanel && groupPanel.IsInMeasure)
					RibbonGroupPanel.InvalidateMeasure(panel, groupPanel);
			}
		}

		private static object OnCoerceMaxColumns(DependencyObject d, object value)
		{
			GalleryWrapPanel panel = (GalleryWrapPanel)d;

			if ((int)value > 0 && (int)value < panel.MinColumns)
				return panel.MinColumns;

			return value;
		}

		/// <summary>
		/// Returns the maximum number of columns in which the items should be arranged.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> If the <b>MaxColumns</b> is less than the largest <see cref="ColumnSpanProperty"/> 
		/// value for one or more of the items then the ColumnSpan value will be used as the maximum number of columns.</p>
		/// </remarks>
		/// <seealso cref="MaxColumnsProperty"/>
		/// <seealso cref="PreferredColumns"/>
		/// <seealso cref="MinColumns"/>
		//[Description("Returns the maximum number of columns in which the items should be arranged.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MaxColumns
		{
			get
			{
				return (int)this.GetValue(GalleryWrapPanel.MaxColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.MaxColumnsProperty, value);
			}
		}

		#endregion //MaxColumns

		#region MinColumns

		/// <summary>
		/// Identifies the <see cref="MinColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinColumnsProperty = DependencyProperty.Register("MinColumns",
			typeof(int), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(1, FrameworkPropertyMetadataOptions.AffectsMeasure, 
			new PropertyChangedCallback(OnMinColumnsChanged)), new ValidateValueCallback(ValidateIsPositiveInt));

		private static void OnMinColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			GalleryWrapPanel panel = (GalleryWrapPanel)d;

			// make sure the current and max values are valid
			panel.CoerceValue(MaxColumnsProperty);
			panel.CoerceValue(PreferredColumnsProperty);
		}

		/// <summary>
		/// Returns/sets the minimum number of columns in which the items should be arranged.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> If the <b>MinColumns</b> is less than the largest <see cref="ColumnSpanProperty"/> 
		/// value for one or more of the items then the ColumnSpan value will be used as the minimum number of columns.</p>
		/// </remarks>
		/// <seealso cref="MinColumnsProperty"/>
		/// <seealso cref="PreferredColumns"/>
		/// <seealso cref="MaxColumns"/>
		//[Description("Returns/sets the minimum number of columns in which the items should be arranged.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MinColumns
		{
			get
			{
				return (int)this.GetValue(GalleryWrapPanel.MinColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.MinColumnsProperty, value);
			}
		}

		#endregion //MinColumns

		#region PreferredColumns

		/// <summary>
		/// Identifies the <see cref="PreferredColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredColumnsProperty = DependencyProperty.Register("PreferredColumns",
			typeof(int), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(int.MaxValue, FrameworkPropertyMetadataOptions.AffectsMeasure,
			new PropertyChangedCallback(OnPreferredColumnsChanged), new CoerceValueCallback(OnCoercePreferredColumns)), new ValidateValueCallback(ValidateIsPositiveInt));

		private static void OnPreferredColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private static object OnCoercePreferredColumns(DependencyObject d, object value)
		{
			GalleryWrapPanel panel = (GalleryWrapPanel)d;
			int intValue = (int)value;

			if (intValue < panel.MinColumns)
				return panel.MinColumns;
			else if (panel.MaxColumns > 0 && intValue > panel.MaxColumns)
				return panel.MaxColumns;

			return value;
		}

		/// <summary>
		/// Returns the preferred number of columns in which to arrange the items.
		/// </summary>
		/// <seealso cref="PreferredColumnsProperty"/>
		/// <seealso cref="MinColumns"/>
		/// <seealso cref="MaxColumns"/>
		//[Description("Returns the preferred number of columns in which to arrange the items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int PreferredColumns
		{
			get
			{
				return (int)this.GetValue(GalleryWrapPanel.PreferredColumnsProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.PreferredColumnsProperty, value);
			}
		}

		#endregion //PreferredColumns

		#region RowSpacing
		
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

		private double RowSpacing
		{
			get { return 0; }
		}
		#endregion //RowSpacing

		#region RowHeight

		/// <summary>
		/// Identifies the <see cref="RowHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register("RowHeight",
			typeof(double), typeof(GalleryWrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure), new ValidateValueCallback(ValidateIsPositiveDouble));

		/// <summary>
		/// Returns/sets the preferred height for the rows and therefore the heights of the items.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default, this property is set to <see cref="Double.NaN"/> and the row height 
		/// will be calculated based on the height of the largest item in the panel.</p>
		/// </remarks>
		/// <seealso cref="RowHeightProperty"/>
		/// <seealso cref="ColumnWidth"/>
		/// <seealso cref="ColumnSpacing"/>
		/// <seealso cref="RowSpacing"/>
		//[Description("Returns/sets the preferred height for the rows and therefore the heights of the items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public double RowHeight
		{
			get
			{
				return (double)this.GetValue(GalleryWrapPanel.RowHeightProperty);
			}
			set
			{
				this.SetValue(GalleryWrapPanel.RowHeightProperty, value);
			}
		}

		#endregion //RowHeight

		#endregion //Public Properties

		#region Internal Properties

		#region ItemHeight
		internal double ItemHeight
		{
			get { return _itemSize.Height; }
		}
		#endregion //ItemHeight

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region CalculateFullyVisibleRowAreaHeight
		internal double CalculateFullyVisibleRowAreaHeight(Size constraint)
		{
			if (this.IsMeasureValid == false)
				this.Measure(constraint);

			Thickness margin = this.Margin;

			constraint.Width -= margin.Left + margin.Right;
			constraint.Height -= margin.Top + margin.Bottom;

			// if a specific height was passed to the measure then indicate
			// how many rows of items would fit in view
			//Debug.Assert(double.IsInfinity(constraint.Height) == false);

			// the number of rows that actually fit will be the lower of the total
			// number of rows or the number of rows that fully fit in view
			double rowSpacing = this.RowSpacing;
			int fullyVisibleRowCount = Math.Min(this._calcRowCount, Math.Max(1, (int)((constraint.Height + rowSpacing) / (this._itemSize.Height + rowSpacing))));

			// get the height required to show just those rows
			double fullyVisibleRowAreaHeight = (fullyVisibleRowCount * (this._itemSize.Height + rowSpacing)) - rowSpacing;

			fullyVisibleRowAreaHeight += margin.Top + margin.Bottom;

			return fullyVisibleRowAreaHeight;
		}
		#endregion //CalculateFullyVisibleRowAreaHeight

		#region DoLayout
		/// <summary>
		/// Calculates and optionally arranges items
		/// </summary>
		/// <param name="columnCount">The number of columns into view the items should be arranged</param>
		/// <param name="itemSize">The resolved size for each cell/item</param>
		/// <param name="positionChildren">True to arrange the children</param>
		/// <param name="availableHeight">The amount of height available for the items. This is only used if positionchildren is true. If the panel is scrolling, this is used to position other items out of view and vertically center the rows that are in view.</param>
		/// <param name="rowCount">An out parameter set to the number of rows that the panel contains.</param>
		/// <returns>The size required to arrange all the items</returns>
		private Size DoLayout(int columnCount, Size itemSize, bool positionChildren, double availableHeight, out int rowCount)
		{
			return this.DoLayout(columnCount, itemSize, positionChildren, availableHeight, 0, int.MaxValue, double.NaN, out rowCount);
		}

		/// <summary>
		/// This method should not be called directly except recursively.
		/// </summary>
		private Size DoLayout(int columnCount, Size itemSize, bool positionChildren, double availableHeight, int firstRowInView, int lastRowInView, double preferredFirstRowOffset, out int rowCount)
		{
			#region Setup

			rowCount = 0;
			int colSpanInRow = 0;
			UIElementCollection children = this.InternalChildren;
			double columnSpacing = this.ColumnSpacing;
			double rowSpacing = this.RowSpacing;
			double firstRowOffset = preferredFirstRowOffset;
			int maxRowsInView = double.IsInfinity(availableHeight) || itemSize.Height == 0 ? int.MaxValue : (int)((availableHeight + rowSpacing) / (itemSize.Height + rowSpacing));
			double firstColOffset = 0;
			Rect rect = new Rect(firstColOffset, 0, itemSize.Width, itemSize.Height);

            // JJD 11/28/07 - BR27268
            // Added stack variable to hold the tool.
            GalleryTool galleryTool = null;

			#endregion //Setup

			#region Calculate/Position Children
			for (int i = 0, count = children.Count; i < count; i++)
			{
				UIElement child = children[i];

				if (child.Visibility == Visibility.Collapsed)
					continue;

                // JJD 11/28/07 - BR27268
                // Initialize stack variable with the tool.
                if (galleryTool == null &&
                     child is GalleryItemPresenter)
                    galleryTool = ((GalleryItemPresenter)child).GalleryTool;

				int colSpan = (int)child.GetValue(GalleryWrapPanel.ColumnSpanProperty);

				if (colSpan + colSpanInRow > columnCount)
				{
					rowCount++;
					colSpanInRow = 0;

					if (positionChildren)
					{
						rect.X = firstColOffset;
						rect.Y += itemSize.Height + rowSpacing;

						if (rowCount == firstRowInView)
							rect.Y = firstRowOffset;
						else if (rowCount == lastRowInView + 1)
							rect.Y = availableHeight + rowSpacing;
					}
				}

				if (positionChildren)
				{
					rect.Width = (itemSize.Width * colSpan) + ((colSpan - 1) * columnSpacing);
					child.Arrange(rect);
					rect.X += rect.Width + columnSpacing;
				}

				colSpanInRow += colSpan;
			}

			// this would only not be the case if there were no items
			if (colSpanInRow > 0)
			{
				rowCount++;
			} 
			#endregion //Calculate/Position Children

            #region PopupResizerDecorator

            // JJD 11/28/07 - BR27268
            // If we have a gallery tool and we are in a dropdown, and this is the arrange pass 
            // then update its sizing constraints
            // Note: we moved much of this logic from the MeasureOverride
            if (positionChildren &&
                this._isInDropDown &&
                galleryTool != null)
            {
                Size minSize = galleryTool.MeasuredItemSize;

                // JJD 11/26/07 - BR25892
                // Allow for any extra chrome between the panel and the gallerytool (e.g. scrollbars)
                minSize.Width += Math.Max(galleryTool.DesiredSize.Width - this.DesiredSize.Width, 0);

                PopupResizerDecorator.Constraints constraints = PopupResizerDecorator.GetResizeConstraints(galleryTool);

                if (constraints == null)
                {
                    constraints = new PopupResizerDecorator.Constraints();

                    PopupResizerDecorator.SetResizeConstraints(galleryTool, constraints);
                }

                // JJD 11/26/07 - BR25891
                // allow some extra height for a possible group header if we have groups
                if (galleryTool.Groups.Count > 0)
                {
                    GalleryItemGroup parentGroup = ItemsControl.GetItemsOwner(this) as GalleryItemGroup;

                    if (parentGroup != null)
                        minSize.Height += Math.Max(parentGroup.DesiredSize.Height - this.DesiredSize.Height, 0);
                }

                constraints.MinimumWidth = minSize.Width;
                constraints.MinimumHeight = minSize.Height;
            }

            #endregion //PopupResizerDecorator

			double width = ((itemSize.Width + columnSpacing) * columnCount) - columnSpacing;
			double height = Math.Max(((itemSize.Height + rowSpacing) * rowCount) - rowSpacing, 0);
			return new Size(width, height);
		}
		#endregion //DoLayout

		#region OnLayoutUpdated
		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			this.LayoutUpdated -= new EventHandler(OnLayoutUpdated);

			this.InvalidateMeasure();
		}

		#endregion //OnLayoutUpdated

		#region ValidateIsPositiveInt
		private static bool ValidateIsPositiveInt(object value)
		{
			return value is int && (int)value > 0;
		}
		#endregion //ValidateIsPositiveInt 

		#region ValidateIsPositiveDouble
		private static bool ValidateIsPositiveDouble(object value)
		{
			if (value is double)
			{
				double dblValue = (double)value;

				return double.IsNaN(dblValue) || dblValue > 0;
			}

			return false;
		}
		#endregion //ValidateIsPositiveDouble

		#region ValidateIsAtLeastZero
		private static bool ValidateIsAtLeastZero(object value)
		{
			if (value is double)
			{
				double dblValue = (double)value;

				return dblValue >= 0;
			}

			return false;
		}
		#endregion //ValidateIsAtLeastZero

		#region ValidateIsAtLeastZeroInt
		private static bool ValidateIsAtLeastZeroInt(object value)
		{
			if (value is int)
			{
				int intValue = (int)value;

				return intValue >= 0;
			}

			return false;
		}
		#endregion //ValidateIsAtLeastZeroInt

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