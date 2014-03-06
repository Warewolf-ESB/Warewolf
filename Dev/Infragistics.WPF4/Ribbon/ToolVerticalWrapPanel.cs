using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Diagnostics;
using System.ComponentModel;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// A Panel derived element used to arrange tools vertically within a <see cref="RibbonGroup"/>. 
	/// </summary>
	/// <remarks>
	/// <p class="body"> This is the default panel for a <see cref="RibbonGroup"/>, but it can also be used explicitly to 
	/// control the vertical distribution of a subset of tools within a <see cref="RibbonGroup"/> using the 
	/// <see cref="ToolVerticalWrapPanel.VerticalToolAlignment"/> property.</p>
	/// </remarks>
	/// <seealso cref="RibbonGroup"/>
	/// <seealso cref="VerticalToolAlignment"/>
	/// <seealso cref="ToolHorizontalWrapPanel"/>
	/// <seealso cref="ButtonGroup"/>
    [DesignTimeVisible(false)]	// JJD 06/04/10 - TFS32695 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
    public class ToolVerticalWrapPanel : Panel, IRibbonToolPanel
	{
		#region Member Variables

		private double _smallToolHeight;
		private List<ColumnInfo> _columns;

		private const int InterColumnSpacing = 0;

		private bool _isInArrange = false;

		#endregion //Member Variables

		#region Constructor

		static ToolVerticalWrapPanel()
		{
			RibbonGroup.MinimumSizeProperty.OverrideMetadata(typeof(ToolVerticalWrapPanel), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));
			RibbonToolHelper.SizingModePropertyKey.OverrideMetadata(typeof(ToolVerticalWrapPanel), new FrameworkPropertyMetadata(RibbonKnownBoxes.RibbonToolSizingModeImageAndTextLargeBox));
		}

		/// <summary>
		/// Initializes a new instance of a <see cref="ToolVerticalWrapPanel"/> class.
		/// </summary>
		public ToolVerticalWrapPanel()
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
			bool wasInArrange = this._isInArrange;
			try
			{
				this._isInArrange = true;
				return this.ArrangeOverrideImpl(finalSize);
			}
			finally
			{
				this._isInArrange = wasInArrange;
			}
		}

		private Size ArrangeOverrideImpl(Size finalSize)
		{
			List<ColumnInfo> columns = this._columns;

			if (null != columns)
			{
				UIElementCollection elements = this.InternalChildren;
				Rect toolRect = new Rect(finalSize);
				// AS 10/15/07 
				// Use 1/3 of the height provided. If the tools in this panel happened to be shorter then they
				// would end up not lining up with tools in the rows of other ribbon groups.
				//
				//double smallToolHeight = this._smallToolHeight;
				double smallToolHeight = finalSize.Height / 3;
				RibbonPanelVerticalToolAlignment vertAlign = this.VerticalToolAlignment;

				// position the items based on the calculated column info
				for (int i = 0, count = columns.Count; i < count; i++)
				{
					ColumnInfo column = columns[i];
					Debug.Assert(elements.Count > column.EndingIndex, "The number of elements changed but the column info was not updated!");

					// assume the full size will be used
					toolRect.Width = column.Width;
					toolRect.Y = 0;

					if (column.IsLargeTool)
					{
						toolRect.Height = finalSize.Height;
						elements[column.StartingIndex].Arrange(toolRect);
					}
					else
					{
						#region Small Tool Positioning

						toolRect.Height = smallToolHeight;
						double intraItemSpacing = 0;

						// this is a set of small tools so figure out how to distribute them
						switch (vertAlign)
						{

							case RibbonPanelVerticalToolAlignment.Top:
								// just stack them starting with a y of 0
								break;
							case RibbonPanelVerticalToolAlignment.Bottom:
								// start them near the bottom
								toolRect.Y = finalSize.Height - (smallToolHeight * column.ItemCount);
								break;
							case RibbonPanelVerticalToolAlignment.Center:
								double spaceToDistribute = finalSize.Height - (smallToolHeight * column.ItemCount);
								intraItemSpacing = spaceToDistribute / (column.ItemCount + 1);
								toolRect.Y = intraItemSpacing;
								break;
						}

						for (int j = column.StartingIndex, end = Math.Min(elements.Count - 1, column.EndingIndex); j <= end; j++)
						{
							UIElement child = elements[j];

							if (child == null || child.Visibility == Visibility.Collapsed)
								continue;

							child.Arrange(toolRect);

							toolRect.Y += intraItemSpacing + smallToolHeight;
						}
						#endregion //Small Tool Positioning
					}

					toolRect.X += column.Width + InterColumnSpacing;
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
			Size largeToolSize = availableSize;
			Size smallToolSize = availableSize;

			// only measure the small tools with a 3rd of the height
			// regardless of whether we're compressed so they get sized the
			// same regardless of compression
			smallToolSize.Height /= 3;

			UIElementCollection elements = this.InternalChildren;
			Size desiredSize = new Size();
			double maxSmallToolHeight = 0;
			int toolsInColumn = 0;
			double toolColumnWidth = 0;
			List<ColumnInfo> columns = new List<ColumnInfo>();
			ColumnInfo smallToolColumn = null;
			bool wasPreviousItemReducedFromLarge = false;

			for (int i = 0, count = elements.Count; i < count; i++)
			{
				UIElement child = elements[i];

				if (child == null || child.Visibility == Visibility.Collapsed)
					continue;

				// measure with current but separate based on original size
				RibbonToolSizingMode currentSizingMode = RibbonToolHelper.GetSizingMode(child);
				RibbonToolSizingMode maxSizingMode = RibbonGroup.GetMaximumSize(child);

				bool isLarge = currentSizingMode == RibbonToolSizingMode.ImageAndTextLarge;
				bool wasLarge = maxSizingMode == RibbonToolSizingMode.ImageAndTextLarge;

				// JM 10-09-07
				if (child is Separator)
					isLarge = true;

				// keep track of whether this is not large but wanted to be since that 
				// would mean that the element was resized and therefore we should not put
				// such an element with a "normal" small tool
				bool isReducedFromLarge = wasLarge && isLarge == false;

				if (smallToolColumn != null)
				{
					// AS 9/18/09 TFS19766
					// We have another condition where we need a column break.
					//
					//// if we were in the middle of a small column and we need to start another
					//// or this small column is filled or we have a large tool (which needs
					//// its own column) then start one now
					//if (isLarge ||
					//    isReducedFromLarge != wasPreviousItemReducedFromLarge ||
					//    toolsInColumn == 3)
					bool isColumnBreak = isLarge ||
						isReducedFromLarge != wasPreviousItemReducedFromLarge ||
						toolsInColumn == 3;

					// AS 9/18/09 TFS19766
					// If there are 2 image tools in a row, some of them may have been 
					// large tools previously in which case we want to maintain the column 
					// associations.
					//
					if (!isColumnBreak && toolsInColumn > 0)
					{
						if (ToolSizingModeAction.GetPreviousSizingMode(elements[smallToolColumn.StartingIndex]) !=
							ToolSizingModeAction.GetPreviousSizingMode(elements[i]))
						{
							isColumnBreak = true;
						}
					}

					if (isColumnBreak)
					{
						// the ending index will either be this item (if we put the 3rd
						// item in the column) or the previous index if we've just
						// positioned a large item
						smallToolColumn.EndingIndex = isLarge ? i - 1 : i;
						smallToolColumn.Width = toolColumnWidth;
						smallToolColumn.ItemCount = toolsInColumn;
						smallToolColumn = null;

						// update the desired width with this tool column's width
						desiredSize.Width += toolColumnWidth;
						toolsInColumn = 0;
						toolColumnWidth = 0;
					}
				}

				wasPreviousItemReducedFromLarge = isReducedFromLarge;

				if (isLarge)
				{
					#region Large Tools

					child.Measure(largeToolSize);

					Size childSize = child.DesiredSize;

					desiredSize.Width += childSize.Width;

					if (desiredSize.Height < childSize.Height)
						desiredSize.Height = childSize.Height;

					// store the info for the column
					columns.Add(new ColumnInfo(i, childSize.Width, true));

					#endregion //Large Tools
				}
				else
				{
					#region Small Tools

					Debug.Assert(child is Separator == false, "Should a separator be treated as a small tool?");

					// get a column's worth of small tools
					child.Measure(smallToolSize);

					Size childSize = child.DesiredSize;

					if (maxSmallToolHeight < childSize.Height)
						maxSmallToolHeight = childSize.Height;

					toolsInColumn++;

					if (toolColumnWidth < childSize.Width)
						toolColumnWidth = childSize.Width;

					// store the info for the column
					if (smallToolColumn == null)
					{
						smallToolColumn = new ColumnInfo(i, 0, false);
						columns.Add(smallToolColumn);
					}

					#endregion //Small Tools
				}
			}

			// if we were in the middle of processing a column update the desired width now
			#region Last column fixup
			if (toolsInColumn > 0 && smallToolColumn != null)
			{
				// update the small column info that was in progress
				smallToolColumn.EndingIndex = elements.Count - 1;
				smallToolColumn.Width = toolColumnWidth;
				smallToolColumn.ItemCount = toolsInColumn;
				smallToolColumn = null;

				// update the calculated desired width for this last column
				desiredSize.Width += toolColumnWidth;
			}
			#endregion //Last column fixup

			// use the larger of the large tool height or 3 small tools
			desiredSize.Height = Math.Max(desiredSize.Height, maxSmallToolHeight * 3);

			desiredSize.Width += Math.Max(0, (columns.Count - 1) * InterColumnSpacing);

			// store some information for the arrangement
			this._smallToolHeight = maxSmallToolHeight;
			this._columns = columns;

			return desiredSize;
		}
		#endregion //MeasureOverride

		#region OnChildDesiredSizeChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="UIElement.DesiredSize"/> for a child element has changed.
		/// </summary>
		/// <param name="child">The child element whose size has changed.</param>
		protected override void OnChildDesiredSizeChanged(UIElement child)
		{
			if (this._isInArrange)
				return;

			base.OnChildDesiredSizeChanged(child);
		}
		#endregion //OnChildDesiredSizeChanged

		#endregion //Base class overrides

		#region Properties

		#region VerticalToolAlignment

		/// <summary>
		/// Identifies the <see cref="VerticalToolAlignment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VerticalToolAlignmentProperty = DependencyProperty.Register("VerticalToolAlignment",
			typeof(RibbonPanelVerticalToolAlignment), typeof(ToolVerticalWrapPanel), new FrameworkPropertyMetadata(RibbonPanelVerticalToolAlignment.Top));

		/// <summary>
		/// Returns/sets a value that determines how items are arranged vertically when there are less than 3 small items within a column.
		/// </summary>
		/// <seealso cref="VerticalToolAlignmentProperty"/>
		//[Description("Returns/sets a value that determines how items are arranged vertically when there are less than 3 small items within a column.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public RibbonPanelVerticalToolAlignment VerticalToolAlignment
		{
			get
			{
				return (RibbonPanelVerticalToolAlignment)this.GetValue(ToolVerticalWrapPanel.VerticalToolAlignmentProperty);
			}
			set
			{
				this.SetValue(ToolVerticalWrapPanel.VerticalToolAlignmentProperty, value);
			}
		}

		#endregion //VerticalToolAlignment

		#endregion //Properties

		#region Methods

		// AS 9/18/09 TFS19766
		// Refactored this from the IRibbonToolPanel.GetResizableTools routine below.
		//
		#region GetResizableLargeTools
		private void GetResizableLargeTools(List<FrameworkElement> tools, RibbonToolSizingMode destination)
		{
			UIElementCollection children = this.InternalChildren;
			Debug.Assert(children.Count > 0);

			// AS 9/18/09 TFS19766
			bool isImageOnly = destination == RibbonToolSizingMode.ImageOnly;

			int consecutiveLargeColumns = 0;

			// if the destination size is ImageAndTextNormal then look for groups of 3 consecutive large columns

			// we'll iterate backwards to resize the farthest tools first
			for (int i = this._columns.Count - 1; i >= 0; i--)
			{
				ColumnInfo column = this._columns[i];

				Debug.Assert(column.StartingIndex < children.Count);

				// AS 9/18/09 TFS19766
				int lastConsecutiveColumns = consecutiveLargeColumns;

				if (column.IsLargeTool &&
					children[column.StartingIndex] is FrameworkElement &&
					children[column.StartingIndex] is SeparatorTool == false && // do not resize a separator
					RibbonGroup.GetMinimumSize(children[column.StartingIndex]) <= destination)
				{
					MenuTool menu = children[column.StartingIndex] as MenuTool;

					// do not include a menu tool that is displaying a gallery
					if (menu == null || menu.IsGalleryPreviewVisible == false)
						consecutiveLargeColumns++;
					else
						consecutiveLargeColumns = 0;
				}
				else
					consecutiveLargeColumns = 0;

				if (consecutiveLargeColumns == 3)
				{
					// this was the third large column so add the three large tools
					for (int j = 0; j < 3; j++)
					{
						tools.Add((FrameworkElement)children[this._columns[i + j].StartingIndex]);
					}

					// AS 10/10/08 TFS6237
					consecutiveLargeColumns = 0;
				}
				// AS 9/18/09 TFS19766
				// If we had some consecutive large columns that can go to image only and we 
				// hit the end of the list or we had < 3 consecutive columns, we'll include 
				// the tools anyway.
				//
				else if (isImageOnly && ((lastConsecutiveColumns > 0 && consecutiveLargeColumns == 0) || i == 0))
				{
					int iStartingIndex = i == 0 ? 0 : i + 1;
					int columnCount = i == 0 ? consecutiveLargeColumns : lastConsecutiveColumns;

					for (int j = 0; j < columnCount; j++)
					{
						tools.Add((FrameworkElement)children[this._columns[iStartingIndex + j].StartingIndex]);
					}
				}

			}
		}
		#endregion //GetResizableLargeTools

		#endregion //Methods

		#region IRibbonToolPanel
		IList<FrameworkElement> IRibbonToolPanel.GetResizableTools(RibbonToolSizingMode destinationSize)
		{
			UIElementCollection children = this.InternalChildren;

			if (this.IsMeasureValid == false)
				this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            // AS 8/21/08 BR33778/BR35771
            // I think part of this issue was fixed with the fix for BR35771
            // but for a ribbon group that is collapsed, the children
            // may be empty even though we had column information at one point.
            //
            if (children.Count == 0)
                return null;

			if (this._columns.Count > 0)
			{
                //Debug.Assert(this._columns[this._columns.Count - 1].EndingIndex < children.Count);

                // AS 8/21/08 BR33778/BR35771
                // This shouldn't happen but just in case let's not let the resize
                // logic crash the application.
                //
                if (children.Count <= this._columns[this._columns.Count - 1].EndingIndex)
                    return null;

				List<FrameworkElement> tools = new List<FrameworkElement>();

				if (destinationSize == RibbonToolSizingMode.ImageAndTextNormal)
				{
					#region Large => Normal

					
#region Infragistics Source Cleanup (Region)






































#endregion // Infragistics Source Cleanup (Region)

					GetResizableLargeTools(tools, RibbonToolSizingMode.ImageAndTextNormal);
					#endregion //Large => Normal
				}
				else if (destinationSize == RibbonToolSizingMode.ImageOnly)
				{
					#region Normal => ImageOnly
					int childCount = children.Count;

					// iterate each column of small tools currently showing image and text normal but
					// that can be image and text small
					for (int i = 0, count = this._columns.Count; i < count; i++)
					{
						ColumnInfo column = this._columns[i];

						if (column.IsLargeTool == false)
						{
							for (int j = column.StartingIndex, end = column.EndingIndex; j <= end; j++)
							{
								Debug.Assert(j < children.Count);

								FrameworkElement element = children[j] as FrameworkElement;

								if (element != null &&
									RibbonToolHelper.GetSizingMode(element) == RibbonToolSizingMode.ImageAndTextNormal &&
									RibbonGroup.GetMinimumSize(element) == RibbonToolSizingMode.ImageOnly)
								{
									tools.Add(element);
								}
							}
						}
					}

					// AS 9/18/09 TFS19766
					GetResizableLargeTools(tools, RibbonToolSizingMode.ImageOnly);

					#endregion //Normal => ImageOnly
				}

				return tools;
			}

			return null;
		} 
		#endregion //IRibbonToolPanel

		#region ColumnInfo
		private class ColumnInfo
		{
			internal double Width;
			internal int StartingIndex;
			internal int EndingIndex;
			internal int ItemCount;
			internal bool IsLargeTool;

			internal ColumnInfo(int itemIndex, double width, bool isLarge)
			{
				this.StartingIndex = this.EndingIndex = itemIndex;
				this.ItemCount = 1;
				this.Width = width;
				this.IsLargeTool = isLarge;
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