using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of rows in a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Rows in this collection are lazily created (they are only created and added to the collection when they are accessed).
	/// If this collection is enumerated, it only enumerates the rows which were already accessed.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetRow"/>
	/// <seealso cref="Worksheet.Rows"/>



	public

		 class WorksheetRowCollection : RowColumnCollectionBase<WorksheetRow> 
	{
		#region Member Variables

		// MD 7/23/10 - TFS35969
		private RowsLoadOnDemandTree tree; 

		#endregion // Member Variables

		#region Constructor

		internal WorksheetRowCollection( Worksheet parentWorksheet )
			// MD 6/31/08 - Excel 2007 Format
			//: base( parentWorksheet, Workbook.MaxExcelRowCount ) { }
			: base( parentWorksheet ) { }

		#endregion Constructor

		#region Base Class Overrides

		// MD 7/23/10 - TFS35969
		#region CreateLoadOnDemandTree

		internal override LoadOnDemandTree<WorksheetRow> CreateLoadOnDemandTree()
		{
			this.tree = new RowsLoadOnDemandTree(this);
			return this.tree;
		} 

		#endregion // CreateLoadOnDemandTree

		#region CreateValue






		internal override WorksheetRow CreateValue( int index )
		{
			WorksheetRow row = new WorksheetRow( this.Worksheet, index );

			// Initialize the new row's hidden to the default from the worksheet
			row.Hidden = this.Worksheet.DefaultRowHidden;

			// MD 3/22/12 - TFS104630
			// We no longer sync overlapping borders.
			#region Old Code

			//// MD 9/28/11 - TFS88683
			//// We need to sync overlapping border properties here.
			//if (0 < index)
			//{
			//    WorksheetRow rowAbove = this.GetIfCreated(index - 1);
			//    if (rowAbove != null && rowAbove.HasCellFormat)
			//    {
			//        WorksheetCellFormatProxy format = row.CellFormatInternal;
			//        WorksheetCellFormatProxy formatAbove = rowAbove.CellFormatInternal;
			//        format.SetValue(CellFormatValue.TopBorderColorInfo, formatAbove.GetValue(CellFormatValue.BottomBorderColorInfo), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//        format.SetValue(CellFormatValue.TopBorderStyle, formatAbove.GetValue(CellFormatValue.BottomBorderStyle), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//    }
			//}

			//if (index < (Workbook.GetMaxRowCount(this.Worksheet.CurrentFormat) - 1))
			//{
			//    WorksheetRow rowBelow = this.GetIfCreated(index + 1);
			//    if (rowBelow != null && rowBelow.HasCellFormat)
			//    {
			//        WorksheetCellFormatProxy format = row.CellFormatInternal;
			//        WorksheetCellFormatProxy formatBelow = rowBelow.CellFormatInternal;
			//        format.SetValue(CellFormatValue.BottomBorderColorInfo, formatBelow.GetValue(CellFormatValue.TopBorderColorInfo), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//        format.SetValue(CellFormatValue.BottomBorderStyle, formatBelow.GetValue(CellFormatValue.TopBorderStyle), true, CellFormatValueChangedOptions.PreventAdjacentBorderSyncronization);
			//    }
			//}

			#endregion // Old Code

			return row;
		}

		#endregion CreateValue

		// MD 6/31/08 - Excel 2007 Format
		#region MaxCount

		/// <summary>
		/// Gets the maximum number of items allowed in this collection.
		/// </summary>
		// MD 2/1/11 - Page Break support
		//protected override int MaxCount
		//{
		//    // TODO: This may be null
		//    get { return this.Worksheet.Workbook.MaxRowCount; }
		//}
		protected internal override int MaxCount
		{
			get
			{
				// MD 2/24/12 - 12.1 - Table Support
				//Workbook workbook = this.Worksheet.Workbook;
				//
				//if (workbook == null)
				//    return Workbook.GetMaxRowCount(WorkbookFormat.Excel2007);
				//
				//return workbook.MaxRowCount;
				return Workbook.GetMaxRowCount(this.Worksheet.CurrentFormat);
			}
		} 

		#endregion MaxCount

		// MD 7/2/09 - TFS18634
		#region OnCurrentFormatChanged

		internal override void OnCurrentFormatChanged()
		{
			base.OnCurrentFormatChanged();

			foreach ( WorksheetRow row in this )
				row.OnCurrentFormatChanged();
		} 

		#endregion OnCurrentFormatChanged

		#region ThrowIndexOutOfRangeException






		internal override void ThrowIndexOutOfRangeException( int index )
		{
			// MD 6/31/08 - Excel 2007 Format
			//throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxRows", Workbook.MaxExcelRowCount ) );
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MaxRows", this.Worksheet.Workbook.MaxRowCount ) );
		}

		#endregion ThrowIndexOutOfRangeException

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal override void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			foreach ( WorksheetRow row in this )
				row.VerifyFormatLimits( limitErrors, testFormat );
		}

		#endregion VerifyFormatLimits

		#endregion Base Class Overrides

		#region Methods

		// MD 7/23/10 - TFS35969
		#region GetDistanceToTopOfRow

		internal void GetDistanceToTopOfRows(int topRowIndex, int bottomRowIndex, bool ignoreHidden, out int distanceToTopRow, out int distanceToBottomRow)
		{
			this.tree.GetDistanceToTopOfRows(topRowIndex, bottomRowIndex, ignoreHidden, out distanceToTopRow, out distanceToBottomRow);
		}

		#endregion // GetDistanceToTopOfRow

		// MD 3/15/12 - TFS104581
		// Moved this from the base collection because this is the only collection where it should be used now 
		// and we don't want to inadvertently use it for columns or cells in the future.
		#region GetIfCreated






		internal WorksheetRow GetIfCreated(int index)
		{
			WorksheetRow item;
			if (this.ItemsInternal.TryGetValue(index, out item))
				return item;

			return null;
		}

		#endregion GetIfCreated

		#region InitializeFrom






		internal void InitializeFrom( HiddenRowCollection hiddenRows )
		{
			foreach ( WorksheetRow row in this )
				row.Hidden = hiddenRows.Contains( row );
		}

		#endregion InitializeFrom

		// MD 7/23/10 - TFS35969
		#region ResetHeightCache

		internal void ResetHeightCache(bool onlyVisibilityChanged)
		{
			this.tree.ResetExtentCache(onlyVisibilityChanged);
		}

		internal void ResetHeightCache(int rowIndex, bool onlyVisibilityChanged)
		{
			this.tree.ResetExtentCache(rowIndex, onlyVisibilityChanged);
		}

		#endregion // ResetHeightCache

		#endregion Methods

		#region Properties

		#region Public Properties

		#region Indexer[ int ]

		/// <summary>
		/// Gets the row at the specified index.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all rows. It will only iterate the rows which have already 
		/// been used.  To create and iterate all rows in the worksheet use a For loop, iterating from 0 to one less than 
		/// the maximum row count, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <param name="index">The zero-based index of the row to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="index"/> is greater than or equal to <see cref="Workbook.MaxExcelRowCount"/>
		/// or <see cref="Workbook.MaxExcel2007RowCount"/>, depending on the <see cref="Workbook.CurrentFormat"/>.
		/// </exception>
		/// <value>The row at the specified index.</value>
		public WorksheetRow this[ int index ]
		{
			get { return this.InternalIndexer( index ); }
		}

		#endregion Indexer[ int ]

		#endregion Public Properties

		#endregion Properties


		// MD 7/23/10 - TFS35969
		#region RowBinaryTreeNode class

		private class RowBinaryTreeNode : RowColumnBinaryTreeNode<WorksheetRow>
		{
			public RowBinaryTreeNode(int index, IBinaryTreeNodeOwner<WorksheetRow> parent, LoadOnDemandTree<WorksheetRow> tree)
				: base(index, parent, tree) { }

			protected override int GetItemExtent(WorksheetRow item, int relativeIndex)
			{
				// MD 1/18/11 - TFS62762
				// The Height property doesn't take into account the cells with wrapped text, so use GetResolvedHeight instead
				// and ignore the Hidden property value, because we just want to know what the row height would be when visible.
				//return item.Height;
				return item.GetResolvedHeight(true);
			}
		}

		#endregion // RowBinaryTreeNode class

		// MD 7/23/10 - TFS35969
		#region RowsLoadOnDemandTree class

		private class RowsLoadOnDemandTree : RowColumnLoadOnDemandTree<WorksheetRow>
		{
			public RowsLoadOnDemandTree(WorksheetRowCollection owner)
				: base(owner) { }

			internal override LoadOnDemandTree<WorksheetRow>.BinaryTreeNode CreateNode(int nodeStartIndex, IBinaryTreeNodeOwner<WorksheetRow> owner)
			{
				return new RowBinaryTreeNode(nodeStartIndex, owner, this);
			}

			public void GetDistanceToTopOfRows(int topRowIndex, int bottomRowIndex, bool ignoreHidden, out int distanceToTopRow, out int distanceToBottomRow)
			{
				int defaultRowHeight = this.worksheet.DefaultRowHeight;
				int unallocatedRowHeight = defaultRowHeight;
				if (ignoreHidden == false && this.worksheet.DefaultRowHidden)
					unallocatedRowHeight = 0;

				this.GetDistanceToBeginningOfItems(
					topRowIndex,
					bottomRowIndex,
					ignoreHidden,
					out distanceToTopRow,
					out distanceToBottomRow,
					defaultRowHeight,
					unallocatedRowHeight);
			}
		}

		#endregion // RowsLoadOnDemandTree class
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