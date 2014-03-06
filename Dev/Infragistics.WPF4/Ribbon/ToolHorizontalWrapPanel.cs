using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Collections;
using System.Diagnostics;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A Panel derived element used to arrange tools horizontally within a <see cref="RibbonGroup"/>.  
	/// </summary>
	/// <remarks>
	/// <p class="body">The <see cref="MinRows"/> and <see cref="MaxRows"/> are used to determine the range of rows into which the items 
	/// may be arranged. By default, the panel will arrange the items using the MinRows. When a <see cref="GroupVariant"/> with a <see cref="GroupVariant.ResizeAction"/> 
	/// of <b>IncreaseHorizontalWrapRowCount</b> is processed, the <see cref="RowCount"/> is increased towards the MaxRows value.</p>
	/// <p class="body">The <see cref="SortOrderProperty"/> attached property can be used to determine the order of the items when the <see cref="RowCount"/> is 
	/// increased above the <see cref="MinRows"/> value. By default 2 rows will be created.  If the containing <see cref="RibbonGroup"/> is resized smaller and there 
	/// is not enough room on 2 rows for all the tools, the tools will be re-arranged on 3 rows and possibly reordered based on the <see cref="SortOrderProperty"/> 
	/// attached property (potentially) assigned to each tool.</p>
	/// </remarks>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="ToolVerticalWrapPanel"/>
	/// <seealso cref="ButtonGroup"/>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class ToolHorizontalWrapPanel : Panel, IRibbonToolPanel
	{
		#region Member Variables

		private double _smallToolHeight;
		private ItemRowGrouping _currentRows;
		private List<UIElement> _smallTools;
		private List<UIElement> _largeTools;

		
		private const int InterToolItemSpacingHorizontal = 3;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ToolHorizontalWrapPanel"/>
		/// </summary>
		public ToolHorizontalWrapPanel()
		{
		}

		static ToolHorizontalWrapPanel()
		{
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));
			RibbonToolHelper.SizingModePropertyKey.OverrideMetadata(typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));
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
			// position all the large items
			Rect largeRect = new Rect(finalSize);

			// position the large tools first using the full height
			for (int i = 0, count = this._largeTools.Count; i < count; i++)
			{
				UIElement child = this._largeTools[i];

				// make the tool as wide as it wanted
				largeRect.Width = child.DesiredSize.Width;

				child.Arrange(largeRect);

				// move over for the next tool
				largeRect.X += largeRect.Width;
			}

			// then position the small items based on the cached grouping
			int rowCount = this._currentRows.Rows.Length;
			double smallToolHeight = this._smallToolHeight;
			double spaceToDistribute = finalSize.Height - (smallToolHeight * rowCount);
			double offsetPerRow = spaceToDistribute / (rowCount + 1);
			int intraHorizontalToolSpacing = InterToolItemSpacingHorizontal;

			for (int i = 0; i < rowCount; i++)
			{
				// calculate the starting rect for the items in this row
				Rect smallToolRect = new Rect(largeRect.X, offsetPerRow + (i * offsetPerRow) + (i * smallToolHeight), 0, smallToolHeight);

				// get the row
				ItemRow row = this._currentRows.Rows[i];

				// position all the items in the row
				for (int item = row.StartIndex, end = item + row.Count; item < end; item++)
				{
					UIElement child = this._smallTools[item];
					smallToolRect.Width = child.DesiredSize.Width;
					child.Arrange(smallToolRect);
					smallToolRect.X += smallToolRect.Width;

					if (child is Separator == false)
						smallToolRect.X += intraHorizontalToolSpacing;
				}
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
		protected override Size MeasureOverride(Size availableSize)
		{
			#region Setup

			UIElementCollection elements = this.InternalChildren;

			int rowCount = this.RowCount;
			Size largeToolSize = availableSize;
			Size smallToolSize = availableSize;

			// only measure the small tools with a 3rd of the height
			// regardless of whether we're compressed so they get sized the
			// same regardless of compression
			smallToolSize.Height /= 3;

			Size desiredSize = new Size();
			int smallToolCount = 0;
			double smallToolWidth = 0;
			double maxSmallToolHeight = 0;
			List<UIElement> largeItems = new List<UIElement>();
			List<UIElement> smallItems = new List<UIElement>();
			List<double> smallItemWidths = new List<double>();

			int intraHorizontalToolSpacing = InterToolItemSpacingHorizontal;

			#endregion //Setup

			#region Measure the tools
			// first measure every one
			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				RibbonToolSizingMode sizingMode = RibbonToolHelper.GetSizingMode(child);
				bool isLarge = sizingMode == RibbonToolSizingMode.ImageAndTextLarge;

				// if the tool is large, measure it with the full height
				if (isLarge)
				{
					// keep track of the space needed for the large tools...
					child.Measure(largeToolSize);

					Size childSize = child.DesiredSize;

					desiredSize.Width += childSize.Width;

					if (childSize.Height > desiredSize.Height)
						desiredSize.Height = childSize.Height;

					largeItems.Add(child);
				}
				else
				{
					child.Measure(smallToolSize);

					// keep track of how many small tools we have and how
					// much space they would take up in a single row
					if (child is Separator == false)
					{
						smallToolCount++;

						// add spacing between tools
						if (smallToolCount > 1)
							smallToolWidth += intraHorizontalToolSpacing;
					}

					smallItems.Add(child);
					Size childSize = child.DesiredSize;
					smallToolWidth += childSize.Width;
					maxSmallToolHeight = Math.Max(maxSmallToolHeight, childSize.Height);
				}
			}
			#endregion //Measure the tools

			// sort the large and small tools
			this.Sort(largeItems);
			this.Sort(smallItems);

			// now that they're sorted, cached their widths
			for (int i = 0, count = smallItems.Count; i < count; i++)
				smallItemWidths.Add(smallItems[i].DesiredSize.Width);

			// if there are small tools, then we need to figure out how to 
			// organize them and therefore how much space is needed for them
			#region Setup for rows

			int preferredRows = Math.Min(smallToolCount, rowCount);
			ItemRowGrouping[] grouping;
			int minGroupingIndex = 0;

			// if there is only 1 tool there can only be 1 row
			if (preferredRows > 0)
			{
				// now get the preferred size of the group so we can wrap it to 2 or 3 lines
				int itemCount = smallItems.Count;

				double splitWidth = smallToolWidth / preferredRows;
				int numOptions = (int)Math.Pow(2, (preferredRows - 1));
				grouping = new ItemRowGrouping[numOptions];
				double minGroupingWidth = double.MaxValue;

				#region Iterate each split option
				// iterate through each option to calculate the split positions
				for (int i = 0; i < numOptions; i++)
				{
					// create a grouping for the position
					grouping[i] = new ItemRowGrouping(preferredRows);

					// start the first row with the first tool
					int firstTool = 0;

					// keep track of the remaining width
					double remainingWidth = smallToolWidth;

					// keep track of the number of items remaining
					int nonSeparatorsRemaining = smallToolCount;

					#region Iterate the rows
					// iterate through the rows
					for (int row = 0; row < preferredRows - 1; row++)
					{
						// assume a 0 width row
						double rowWidth = 0;

						// keep track of the number of non separators added
						int toolsAdded = 0;

						int originalFirstIndex = firstTool;

						// see if this row should have just more or just less 
						// than the split width 
						bool isMore = (i & (1 << row)) > 0;

						Debug.Write("Row:" + row.ToString() + (isMore ? " More " : " Less "));

						#region Iterate the items to find the start of the next row
						// go from the starting tool to the end
						for (; firstTool < itemCount; firstTool++)
						{
							bool isSeparator = smallItems[firstTool] is Separator;

							// don't add a separator as the first item in a row
							if (isSeparator && toolsAdded == 0)
							{
								remainingWidth -= smallItemWidths[firstTool];
								originalFirstIndex++;
								continue;
							}

							// each row should have at least 1 tool
							if (toolsAdded > 0)
							{
								// if we want to find the first tool
								// on the next row when the row is wider
								// then the split width...
								if (isMore && rowWidth > splitWidth)
									break;

								// if we want to find the first tool on the next row
								// when the row is just a bit less wide then the
								// split width...
								if (isMore == false &&
									firstTool < itemCount - 1 &&
									rowWidth + smallItemWidths[firstTool] > splitWidth)
									break;

								// if there is only enough for 1 tool on each remaining row
								// then don't add any more tools
								if (nonSeparatorsRemaining == (preferredRows - 1) - row)
									break;
							}

							// then add in the row width if we're keeping this tool
							toolsAdded++;
							nonSeparatorsRemaining--;
							rowWidth += smallItemWidths[firstTool];
							remainingWidth -= smallItemWidths[firstTool];

							// if there are multiple tools then also take into
							// account the space between the tools
							if (toolsAdded > 1)
							{
								remainingWidth -= intraHorizontalToolSpacing;
								rowWidth += intraHorizontalToolSpacing;
							}
						}
						#endregion //Iterate the items to find the start of the next row

						int tempCount = firstTool - originalFirstIndex;

						// if the last tool added was a separator, remove that 
						// since a separator must always be before an item
						if (smallItems[firstTool - 1] is Separator)
						{
							tempCount--;
							rowWidth -= smallItemWidths[firstTool - 1];
						}

						grouping[i].Rows[row] = new ItemRow(originalFirstIndex, tempCount, rowWidth);
					}
					#endregion //Iterate the rows

					Debug.Assert(firstTool < itemCount, "The last row won't have any tools!");

					int lastRowCount = itemCount - firstTool;

					// if the last tool added was a separator, remove that 
					// since a separator must always be before an item
					if (firstTool < itemCount && smallItems[firstTool] is Separator)
					{
						lastRowCount--;
						remainingWidth -= smallItemWidths[firstTool];
						firstTool++;
					}

					// the remaining width will still include the space between the last tool
					// on each row and the tool on the next row so remove that now
					remainingWidth -= (preferredRows - 1) * intraHorizontalToolSpacing;

					// lastly set the values for the last row
					grouping[i].Rows[preferredRows - 1] = new ItemRow(firstTool, lastRowCount, remainingWidth);

					double groupingWidth = 0;

					for (int j = 0; j < preferredRows; j++)
						groupingWidth = Math.Max(groupingWidth, grouping[i].Rows[j].Width);

					if (groupingWidth < minGroupingWidth)
					{
						minGroupingWidth = groupingWidth;
						minGroupingIndex = i;
					}

					Debug.WriteLineIf(true, " Width:" + groupingWidth.ToString(), "Grouping#:" + i.ToString());
				}
				#endregion //Iterate each split option

				Debug.Assert(grouping[minGroupingIndex].Rows[preferredRows - 1].Count > 0, "The last row won't have any tools!");

				// cache the row structure to use
				this._currentRows = grouping[minGroupingIndex];

				// add this width to the minimum group width
				desiredSize.Width += minGroupingWidth;
			}
			else
			{
				grouping = null;
				desiredSize.Width += smallToolWidth;
				this._currentRows = new ItemRowGrouping(0);
			}
			#endregion //Setup for rows

			// the height will be the greater of a large tool size and 3 small tools
			desiredSize.Height = Math.Max(desiredSize.Height, maxSmallToolHeight * 3);

			// cache information for the positioning
			this._smallToolHeight = maxSmallToolHeight;
			this._largeTools = largeItems;
			this._smallTools = smallItems;

			return desiredSize;
		}
		#endregion //MeasureOverride

		#endregion //Base class overrides

		#region Properties

		#region RowCount

		internal static readonly DependencyPropertyKey RowCountPropertyKey =
			DependencyProperty.RegisterReadOnly("RowCount",
			typeof(int), typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure,
			new PropertyChangedCallback(OnRowCountChanged), new CoerceValueCallback(OnCoerceRowCount)), new ValidateValueCallback(OnValidateRowCount));

		/// <summary>
		/// Identifies the <see cref="RowCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RowCountProperty =
			RowCountPropertyKey.DependencyProperty;

		private static void OnRowCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}

		private static object OnCoerceRowCount(DependencyObject d, object value)
		{
			ToolHorizontalWrapPanel panel = (ToolHorizontalWrapPanel)d;
			int intValue = (int)value;

			if (intValue < panel.MinRows)
				return panel.MinRows;
			else if (intValue > panel.MaxRows)
				return panel.MaxRows;

			return value;
		}

		private static bool OnValidateRowCount(object value)
		{
			int intValue = value is int ? (int)value : 0;
			return intValue >= 1 && intValue <= 3;
		}

		/// <summary>
		/// Returns the number of rows that the panel is using to arrange its items.
		/// </summary>
		/// <seealso cref="RowCountProperty"/>
		//[Description("Returns the number of rows that the panel is using to arrange its items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		[ReadOnly(true)]
		public int RowCount
		{
			get
			{
				return (int)this.GetValue(ToolHorizontalWrapPanel.RowCountProperty);
			}
		}

		#endregion //RowCount

		#region MaxRows

		internal const int DefaultMaxRows = 3;

		/// <summary>
		/// Identifies the <see cref="MaxRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxRowsProperty = DependencyProperty.Register("MaxRows",
			typeof(int), typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(DefaultMaxRows, FrameworkPropertyMetadataOptions.AffectsMeasure,
			new PropertyChangedCallback(OnMaxRowsChanged), new CoerceValueCallback(OnCoerceMaxRows)), new ValidateValueCallback(OnValidateRowCount));

		private static void OnMaxRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolHorizontalWrapPanel panel = (ToolHorizontalWrapPanel)d;
			panel.CoerceValue(RowCountProperty);
		}

		private static object OnCoerceMaxRows(DependencyObject d, object value)
		{
			ToolHorizontalWrapPanel panel = (ToolHorizontalWrapPanel)d;

			if ((int)value < panel.MinRows)
				return panel.MinRows;

			return value;
		}

		/// <summary>
		/// Returns/sets the maximum number of rows that the panel can use to arrange its items.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>MinRows</b> represents the preferred number of rows that the panel should use 
		/// to arrange its items. This must be a value between 1 and 3.</p>
		/// </remarks>
		/// <seealso cref="MaxRowsProperty"/>
		/// <seealso cref="MinRowsProperty"/>
		/// <seealso cref="MinRows"/>
		/// <exception cref="ArgumentOutOfRangeException">MaxRows must be a value between 1 and 3.</exception>
		//[Description("Returns/sets the maximum number of rows that the panel can use to arrange its items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MaxRows
		{
			get
			{
				return (int)this.GetValue(ToolHorizontalWrapPanel.MaxRowsProperty);
			}
			set
			{
				this.SetValue(ToolHorizontalWrapPanel.MaxRowsProperty, value);
			}
		}

		#endregion //MaxRows

		#region MinRows

		/// <summary>
		/// Identifies the <see cref="MinRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinRowsProperty = DependencyProperty.Register("MinRows",
			typeof(int), typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure,
			new PropertyChangedCallback(OnMinRowsChanged)), new ValidateValueCallback(OnValidateRowCount));

		private static void OnMinRowsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ToolHorizontalWrapPanel panel = (ToolHorizontalWrapPanel)d;

			// make sure the current and max values are valid
			panel.CoerceValue(MaxRowsProperty);
			panel.CoerceValue(RowCountProperty);
		}

		/// <summary>
		/// Returns/sets the preferred number of rows that the panel should use to arrange its items.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <b>MinRows</b> represents the preferred number of rows that the panel should use 
		/// to arrange its items. This must be a value between 1 and 3.</p>
		/// </remarks>
		/// <seealso cref="MinRowsProperty"/>
		/// <seealso cref="MaxRowsProperty"/>
		/// <seealso cref="MaxRows"/>
		/// <exception cref="ArgumentOutOfRangeException">MinRows must be a value between 1 and 3.</exception>
		//[Description("Returns/sets the preferred number of rows that the panel should use to arrange its items.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public int MinRows
		{
			get
			{
				return (int)this.GetValue(ToolHorizontalWrapPanel.MinRowsProperty);
			}
			set
			{
				this.SetValue(ToolHorizontalWrapPanel.MinRowsProperty, value);
			}
		}

		#endregion //MinRows

		#region SortOrder

		/// <summary>
		/// Identifies the SortOrder attached dependency property
		/// </summary>
		/// <remarks></remarks>
		/// <seealso cref="GetSortOrder"/>
		/// <seealso cref="SetSortOrder"/>
		public static readonly DependencyProperty SortOrderProperty = DependencyProperty.RegisterAttached("SortOrder",
			typeof(int), typeof(ToolHorizontalWrapPanel), new FrameworkPropertyMetadata(int.MaxValue));

		/// <summary>
		/// An attached property that is used to determine the order in which the children will be displayed when the panel can arrange the items into multiple rows and the <see cref="RowCount"/> is the same as the <see cref="MaxRows"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">If the sort order is not specified for all elements, the children will remain in their original order. If the 
		/// sort order is specified for some elements but not all, all of the elements without a specified sort order will be displayed 
		/// after the elements for which the value is specified and then sorted based on their original sort order.</p>
		/// </remarks>
		/// <seealso cref="SortOrderProperty"/>
		/// <seealso cref="SetSortOrder"/>
		[AttachedPropertyBrowsableForChildren()]
		public static int GetSortOrder(DependencyObject d)
		{
			return (int)d.GetValue(ToolHorizontalWrapPanel.SortOrderProperty);
		}

		/// <summary>
		/// Sets the value of the 'SortOrder' attached property
		/// </summary>
		/// <seealso cref="SortOrderProperty"/>
		/// <seealso cref="GetSortOrder"/>
		public static void SetSortOrder(DependencyObject d, int value)
		{
			d.SetValue(ToolHorizontalWrapPanel.SortOrderProperty, value);
		}

		#endregion //SortOrder

		#endregion //Properties

		#region Methods

		#region Sort
		private void Sort(List<UIElement> elements)
		{
			// we decided to sort as long as the row count is not the min and it
			// has been compressed at all - i.e. its row count is not the minros
			if (elements.Count > 0 && this.MinRows < this.MaxRows && this.RowCount != this.MinRows)
			{
				long[] indexes = new long[elements.Count];
				int previousSortOrder = (int)ToolHorizontalWrapPanel.SortOrderProperty.DefaultMetadata.DefaultValue;
				bool isSortNeeded = false;

				for (int i = 0, count = elements.Count; i < count; i++)
				{
					int sortOrder = ToolHorizontalWrapPanel.GetSortOrder(elements[i]);

					// build a sort index using the index in the low word and the 
					// sort order in the high word. since we want the items
					// with the sort order explicitly set to be first, we'll
					// take the difference from the value and the sort order
					long order = i + ((long)(sortOrder) << 32);

					indexes[i] = order;

					// keep track of whether the items are out of their sort order
					// - i.e. whether we need to sort them or not
					// AS 1/3/08 BR29363
					// This check is incorrect. Really we just want to sort if this item
					// is not the first item and its sort order is before its previous item.
					//
					//isSortNeeded |= sortOrder > previousSortOrder;
					if (isSortNeeded == false && i > 0 && sortOrder < previousSortOrder)
						isSortNeeded = true;

					previousSortOrder = sortOrder;
				}

				// sort if needed
				if (isSortNeeded)
				{
					// sort the indexes
					Array.Sort<long>(indexes);

					// then move the items to the new array
					UIElement[] elementArray = new UIElement[elements.Count];

					for (int i = 0; i < indexes.Length; i++)
					{
						int currentIndex = (int)(indexes[i] & 0xffffffff);
						elementArray[i] = elements[currentIndex];
					}

					// then update the original list
					elements.Clear();
					elements.AddRange(elementArray);
				}
			}
		}
		#endregion //Sort

		#endregion //Methods

		#region IRibbonToolPanel
		IList<FrameworkElement> IRibbonToolPanel.GetResizableTools(RibbonToolSizingMode destinationSize)
		{
			List<FrameworkElement> tools = null;

			// we do not support compressing large tools to imageandtextnormal. the main reason is that we could have 
			// a conflicting situation. i.e. since we support sorting the tools and we also support performing the resize
			// variant actions "out of order", the user could define their variant structure such that the large tools 
			// are compressed first and then the horizontal rows are compressed. when the latter happens, we would end 
			// up sorting the large tools as well and if the large tools are reordered, the now compressed large tools
			// could end up in different positions resulting in a different size. and we don't want large tools that 
			// were compressed to small ones to go into the horizontal rows (i.e. we don't want to change the order).
			// alternatively we could support changing the large tools to imageandtextnormal and allow them to go into
			// the horizontal rows
			if (destinationSize == RibbonToolSizingMode.ImageOnly && _smallTools != null )
			{
				if (this.IsMeasureValid == false)
					this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

				tools = new List<FrameworkElement>();

				foreach (UIElement child in this._smallTools)
				{
					FrameworkElement frameworkElement = child as FrameworkElement;

					if (frameworkElement != null && 
						RibbonToolHelper.GetSizingMode(frameworkElement) == RibbonToolSizingMode.ImageAndTextNormal &&
						// AS 1/4/08 BR29415
						// Only include the element if it allows being reduced to image only.
						//
						RibbonGroup.GetMinimumSize(frameworkElement) == RibbonToolSizingMode.ImageOnly)
						tools.Add(frameworkElement);
				}
			}

			return tools;
		} 
		#endregion //IRibbonToolPanel

		#region ItemRow
		private struct ItemRow
		{
			public int StartIndex;
			public int Count;
			public double Width;

			public ItemRow(int startIndex, int count, double width)
			{
				this.StartIndex = startIndex;
				this.Count = count;
				this.Width = width;
			}
		}
		#endregion //ItemRow

		#region ItemRowGrouping
		private struct ItemRowGrouping
		{
			public ItemRow[] Rows;

			internal ItemRowGrouping(int rowCount)
			{
				this.Rows = new ItemRow[rowCount];
			}
		}
		#endregion //ItemRowGrouping

		#region ColumnInfo
		private class ColumnInfo
		{
			internal double Width;
			internal bool IsLargeTool;
			internal List<UIElement> Children;

			internal ColumnInfo(double width, bool isLarge, UIElement child)
			{
				this.Width = width;
				this.IsLargeTool = isLarge;
				this.Children = new List<UIElement>();
				this.Children.Add(child);
			}
		}
		#endregion //ColumnInfo
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