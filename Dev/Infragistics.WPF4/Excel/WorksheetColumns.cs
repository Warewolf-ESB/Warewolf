using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of columns in a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Columns in this collection are lazily created (they are only created and added to the collection when they are accessed).
	/// Therefore, if this collection is enumerated, it only enumerates the columns which were already accessed.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetColumn"/>
	/// <seealso cref="Worksheet.Columns"/>



	public

		 class WorksheetColumnCollection : RowColumnCollectionBase<WorksheetColumn>
	{
		#region Member Variables

		// MD 3/15/12 - TFS104581
		// We don't need to store this anymore. The base class will store the balance tree.
		//// MD 7/23/10 - TFS35969
		//private ColumnsLoadOnDemandTree tree; 

		#endregion // Member Variables

		#region Constructor

		internal WorksheetColumnCollection( Worksheet parentWorksheet )
			// MD 6/31/08 - Excel 2007 Format
			//: base( parentWorksheet, Workbook.MaxExcelColumnCount ) { }
			: base( parentWorksheet ) { }

		#endregion Constructor

		#region Base Class Overrides

		// MD 3/15/12 - TFS104581
		// We don't need to create a derived tree anymore. We can use the base class.
		#region Removed

		//// MD 7/23/10 - TFS35969
		//#region CreateLoadOnDemandTree

		//internal override LoadOnDemandTree<WorksheetColumn> CreateLoadOnDemandTree()
		//{
		//    this.tree = new ColumnsLoadOnDemandTree(this);
		//    return this.tree;
		//}

		//#endregion // CreateLoadOnDemandTree

		#endregion // Removed

		#region CreateValue






		internal override WorksheetColumn CreateValue( int index )
		{
			// MD 9/28/11 - TFS88683
			// We need to sync overlapping border properties here.
			//return new WorksheetColumn( this.Worksheet, index );
			//WorksheetColumn column = new WorksheetColumn(this.Worksheet, index);
			WorksheetColumn column = new WorksheetColumn(this.Worksheet, (short)index);

			// MD 3/15/12 - TFS104581
			// We don't need to do this synchronization anymore. The formats are stored on the blocks.
			#region Removed

			//if (0 < index)
			//{
			//    WorksheetColumn columnToLeft = this.GetIfCreated(index - 1);
			//    if (columnToLeft != null && columnToLeft.HasCellFormat)
			//    {
			//        WorksheetCellFormatProxy format = column.CellFormatInternal;
			//        WorksheetCellFormatProxy formatToLeft = columnToLeft.CellFormatInternal;
			//        format.SetValue(CellFormatValue.LeftBorderColorInfo, formatToLeft.GetValue(CellFormatValue.RightBorderColorInfo), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//        format.SetValue(CellFormatValue.LeftBorderStyle, formatToLeft.GetValue(CellFormatValue.RightBorderStyle), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//    }
			//}

			//if (index < (Workbook.GetMaxColumnCount(this.Worksheet.CurrentFormat) - 1))
			//{
			//    WorksheetColumn columnToRight = this.GetIfCreated(index + 1);
			//    if (columnToRight != null && columnToRight.HasCellFormat)
			//    {
			//        WorksheetCellFormatProxy format = column.CellFormatInternal;
			//        WorksheetCellFormatProxy formatToRight = columnToRight.CellFormatInternal;
			//        format.SetValue(CellFormatValue.RightBorderColorInfo, formatToRight.GetValue(CellFormatValue.LeftBorderColorInfo), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//        format.SetValue(CellFormatValue.RightBorderStyle, formatToRight.GetValue(CellFormatValue.LeftBorderStyle), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//    }
			//}

			#endregion // Removed

			return column;
		}

		#endregion CreateValue

		// MD 3/15/12 - TFS104581
		#region GetEnumeratorHelper

		internal override IEnumerator<WorksheetColumn> GetEnumeratorHelper()
		{
			return this.GetItemsInRange(0, this.MaxCount - 1, true).GetEnumerator();
		}

		#endregion // GetEnumeratorHelper

		// MD 3/15/12 - TFS104581
		#region GetItemsInRange

		internal override IEnumerable<WorksheetColumn> GetItemsInRange(int startIndex, int endIndex, bool enumerateForwards)
		{
			return this.Worksheet.GetColumnsInRange((short)startIndex, (short)endIndex, enumerateForwards);
		}

		#endregion // GetItemsInRange

		// MD 6/31/08 - Excel 2007 Format
		#region MaxCount

		/// <summary>
		/// Gets the maximum number of items allowed in this collection.
		/// </summary>  
		// MD 2/1/11 - Page Break support
		//protected override int MaxCount
		//{
		//    // TODO: This may be null
		//    get { return this.Worksheet.Workbook.MaxColumnCount; }
		//}
		protected internal override int MaxCount
		{
			get
			{
				// MD 2/24/12 - 12.1 - Table Support
				//Workbook workbook = this.Worksheet.Workbook;
				//
				//if (workbook == null)
				//    return Workbook.GetMaxColumnCount(WorkbookFormat.Excel2007);
				//
				//return workbook.MaxColumnCount;
				return Workbook.GetMaxColumnCount(this.Worksheet.CurrentFormat);
			}
		}

		#endregion MaxCount

		// MD 11/24/10 - TFS34598
		#region OnCurrentFormatChanged

		internal override void OnCurrentFormatChanged()
		{
			base.OnCurrentFormatChanged();

			foreach (WorksheetColumn column in this)
				column.OnCurrentFormatChanged();
		} 

		#endregion // OnCurrentFormatChanged

		#region ThrowIndexOutOfRangeException






		internal override void ThrowIndexOutOfRangeException( int index )
		{
			// MD 6/31/08 - Excel 2007 Format
			//throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxColumns", Workbook.MaxExcelColumnCount ) );
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxColumns", this.Worksheet.Workbook.MaxColumnCount ) );
		}

		#endregion ThrowIndexOutOfRangeException

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			// MD 3/15/12 - TFS104581
			// It is faster to iterate the column blocks now.
			//foreach ( WorksheetColumn column in this )
			//    column.VerifyFormatLimits( limitErrors, testFormat );
			foreach (WorksheetColumnBlock columnBlock in this.Worksheet.ColumnBlocks.Values)
				columnBlock.VerifyFormatLimits(limitErrors, testFormat);
		}

		#endregion VerifyFormatLimits

		#endregion Base Class Overrides

		#region Methods

		// MD 7/23/10 - TFS35969
		#region GetDistanceToLeftOfColumns

		internal void GetDistanceToLeftOfColumns(int leftColumnIndex, int rightColumnIndex, bool ignoreHidden, out int distanceToLeftColumn, out int distanceToRightColumn)
		{
			// MD 3/15/12 - TFS104581
			// We don't want to use a derived tree which does item based measurements because the widths are stored on the 
			// column blocks now.
			//this.tree.GetDistanceToLeftOfColumns(leftColumnIndex, rightColumnIndex, ignoreHidden, out distanceToLeftColumn, out distanceToRightColumn);
			distanceToLeftColumn = 0;
			distanceToRightColumn = 0;
			int defaultColumnWidthInTwips = this.Worksheet.GetColumnWidthInTwips(-1);
			foreach (WorksheetColumnBlock columnBlock in this.Worksheet.ColumnBlocks.Values)
			{
				int columnInBlockWidth = 0;
				if (ignoreHidden || columnBlock.Hidden == false)
				{
					if (columnBlock.Width < 0)
						columnInBlockWidth = defaultColumnWidthInTwips;
					else
						columnInBlockWidth = (int)columnBlock.GetWidth(this.Worksheet, WorksheetColumnWidthUnit.Twip);
				}

				int columnsInBlockBeforeLeftColumn = Math.Max(0, Math.Min(columnBlock.LastColumnIndex, leftColumnIndex - 1) - columnBlock.FirstColumnIndex + 1);
				int columnsInBlockBeforeRightColumn = Math.Max(0, Math.Min(columnBlock.LastColumnIndex, rightColumnIndex - 1) - columnBlock.FirstColumnIndex + 1);

				if (columnsInBlockBeforeLeftColumn == 0 && columnsInBlockBeforeRightColumn == 0)
					break;

				distanceToLeftColumn += columnsInBlockBeforeLeftColumn * columnInBlockWidth;
				distanceToRightColumn += columnsInBlockBeforeRightColumn * columnInBlockWidth;
			}
		}

		#endregion // GetDistanceToLeftOfColumns

		#region InitializeFrom






		internal void InitializeFrom( HiddenColumnCollection hiddenColumns )
		{
			foreach ( WorksheetColumn column in this )
				column.Hidden = hiddenColumns.Contains( column );
		}

		#endregion InitializeFrom

		// MD 3/15/12 - TFS104581
		// We no longer cache the widths on the balance tree nodes, so this is not needed.
		#region Removed

		//// MD 7/23/10 - TFS35969
		//#region ResetColumnWidthCache

		//internal void ResetColumnWidthCache(bool onlyVisibilityChanged)
		//{
		//    this.tree.ResetExtentCache(onlyVisibilityChanged);
		//}

		//internal void ResetColumnWidthCache(int columnIndex, bool onlyVisibilityChanged)
		//{
		//    this.tree.ResetExtentCache(columnIndex, onlyVisibilityChanged);
		//} 

		//#endregion // ResetColumnWidthCache

		#endregion // Removed

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Indexer[ int ]

		/// <summary>
		/// Gets the column at the specified index.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all columns. It will only iterate the columns which have already 
		/// been used.  To create and iterate all columns in the worksheet use a For loop, iterating from 0 to one less than 
		/// the maximum column count, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <param name="index">The zero-based index of the column to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="index"/> is greater than or equal to <see cref="Workbook.MaxExcelColumnCount"/>
		/// or <see cref="Workbook.MaxExcel2007ColumnCount"/>, depending on the <see cref="Workbook.CurrentFormat"/>.
		/// </exception>
		/// <value>The column at the specified index.</value>
		public WorksheetColumn this[ int index ]
		{
			get { return this.InternalIndexer( index ); }
		}

		#endregion Indexer[ int ]

		#endregion Public Properties

		#endregion Properties


		// MD 3/15/12 - TFS104581
		// We don't want to use a derived tree which does item based measurements because the widths are stored on the 
		// column blocks now.
		#region Removed

		//// MD 7/23/10 - TFS35969
		//#region ColumnBinaryTreeNode class

		//private class ColumnBinaryTreeNode : RowColumnBinaryTreeNode<WorksheetColumn>
		//{
		//    // In addition to caching the node extent, the column node will also cache each column's extent because
		//    // most of the time, we will be operating in the first node anyway and so we would lose any performance 
		//    // gain by caching at the node level.
		//    private int?[] cachedExtents;

		//    public ColumnBinaryTreeNode(int index, IBinaryTreeNodeOwner<WorksheetColumn> parent, LoadOnDemandTree<WorksheetColumn> tree)
		//        : base(index, parent, tree)
		//    {
		//        this.cachedExtents = new int?[ColumnsLoadOnDemandTree.BTreeLoadFactor];
		//    }

		//    protected override int GetItemExtent(WorksheetColumn item, int relativeIndex)
		//    {
		//        // Check the per-column cache first.
		//        int? cachedExtent = this.cachedExtents[relativeIndex];
		//        if (cachedExtent.HasValue)
		//            return cachedExtent.Value;

		//        int extent = -1;
		//        if (0 <= item.Width)
		//        {
		//            // MD 2/10/12 - TFS97827
		//            // Consolidated a lot of the unit conversion code so we don't duplicate code.
		//            // Also, realized that there was a rounding issue here. We were converting directly from the column width to twips, but we need to first convert to
		//            // pixels, let them get rounded if necessary, then use their twip equivalent. Rounding the twips directly could make the extent be off by up to 7 twips.
		//            //extent = Utilities.MidpointRoundingAwayFromZero(item.Width * ((ColumnsLoadOnDemandTree)this.tree).columnUnitsToTwipsFactor);
		//            extent = (int)Worksheet.ConvertPixelsToTwips(Utilities.MidpointRoundingAwayFromZero(item.Width * ((ColumnsLoadOnDemandTree)this.tree).columnUnitsToPixelsFactor));
		//        }

		//        this.cachedExtents[relativeIndex] = extent;
		//        return extent;
		//    }

		//    public override void ResetExtentCache(bool onlyVisibilityChanged)
		//    {
		//        base.ResetExtentCache(onlyVisibilityChanged);
		//        this.cachedExtents = new int?[ColumnsLoadOnDemandTree.BTreeLoadFactor];
		//    }
		//} 

		//#endregion // ColumnBinaryTreeNode class

		//// MD 7/23/10 - TFS35969
		//#region ColumnsLoadOnDemandTree class

		//private class ColumnsLoadOnDemandTree : RowColumnLoadOnDemandTree<WorksheetColumn>
		//{
		//    // MD 2/10/12 - TFS97827
		//    // We now store a conversion factor to pixels instead of twips because there could be a rounding error when converting to twips directly.
		//    //internal double columnUnitsToTwipsFactor;
		//    internal double columnUnitsToPixelsFactor;

		//    public ColumnsLoadOnDemandTree(WorksheetColumnCollection owner)
		//        : base(owner) { }

		//    internal override LoadOnDemandTree<WorksheetColumn>.BinaryTreeNode CreateNode(int nodeStartIndex, IBinaryTreeNodeOwner<WorksheetColumn> owner)
		//    {
		//        return new ColumnBinaryTreeNode(nodeStartIndex, owner, this);
		//    }

		//    public void GetDistanceToLeftOfColumns(int topRowIndex, int bottomRowIndex, bool ignoreHidden, out int distanceToTopRow, out int distanceToBottomRow)
		//    {
		//        // Cache the conversion factor before doing the calculations.
		//        // MD 2/10/12 - TFS97827
		//        // We now store a conversion factor to pixels instead of twips because there could be a rounding error when converting to twips directly.
		//        //this.columnUnitsToTwipsFactor = Worksheet.TwipsPerPixelAt96DPI / this.worksheet.Workbook.CharacterWidth256thsPerPixel;
		//        this.columnUnitsToPixelsFactor = this.worksheet.ZeroCharacterWidth / 256d;

		//        // MD 2/10/12 - TFS97827
		//        // Consolidated a lot of the unit conversion code so we don't duplicate code.
		//        // Also, realized that there was a rounding issue here. We were converting directly from the column width to twips, but we need to first convert to
		//        // pixels, let them get rounded if necessary, then use their twip equivalent. Rounding the twips directly could make the extent be off by up to 7 twips.
		//        //int defaultColumnWidth = Utilities.MidpointRoundingAwayFromZero(this.worksheet.DefaultColumnWidth * this.columnUnitsToTwipsFactor);
		//        int defaultColumnWidth = (int)Worksheet.ConvertPixelsToTwips(Utilities.MidpointRoundingAwayFromZero(this.worksheet.DefaultColumnWidth * this.columnUnitsToPixelsFactor));

		//        int unallocatedColumnWidth = defaultColumnWidth;

		//        this.GetDistanceToBeginningOfItems(
		//            topRowIndex,
		//            bottomRowIndex,
		//            ignoreHidden,
		//            out distanceToTopRow,
		//            out distanceToBottomRow,
		//            defaultColumnWidth,
		//            unallocatedColumnWidth);
		//    }
		//} 

		//#endregion // ColumnsLoadOnDemandTree class

		#endregion // Removed
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