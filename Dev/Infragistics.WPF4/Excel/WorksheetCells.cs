using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Diagnostics;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of cells in a row.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Cells in this collection are lazily created (they are only created and added to the collection when they are accessed).
	/// If this collection is enumerated, it only enumerates the cells which were already accessed.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetCell"/>
	/// <seealso cref="WorksheetRow.Cells"/>



	public

		 class WorksheetCellCollection : RowColumnCollectionBase<WorksheetCell>
	{
		#region Member Variables

		// MD 4/19/11 - TFS73111
		private int cellCount = -1;

		private WorksheetRow row;

		#endregion Member Variables

		#region Constructor

		internal WorksheetCellCollection( WorksheetRow row )
			// MD 6/31/08 - Excel 2007 Format
			//: base( row.Worksheet, Workbook.MaxExcelColumnCount )
			: base( row.Worksheet )
		{
			this.row = row;
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 4/19/11 - TFS73111
		#region Count

		internal override int Count
		{
			get
			{
				if (this.cellCount < 1)
				{
					this.cellCount = 0;
					foreach (CellDataContext cellDataContext in this.row.GetCellsWithData())
						this.cellCount++;
				}

				return this.cellCount;
			}
		} 

		#endregion  // Count

		// MD 4/12/11 - TFS67084
		// We don't want to create a LoadOnDemandTree for cells because they are no longer rooted.
		#region CreateLoadOnDemandTree

		internal override LoadOnDemandTree<WorksheetCell> CreateLoadOnDemandTree()
		{
			return null;
		}

		#endregion  // CreateLoadOnDemandTree

		#region CreateValue

		internal override WorksheetCell CreateValue( int index )
		{
			// MD 3/16/09 - TFS14252
			// Since this column index never requires more than 2 bytes, this has been changed to a short so the memory 
			// footprint of a cell can be reduced.
			//return new WorksheetCell( this.Worksheet, this.row.Index, index );
			if (Int16.MaxValue < index)
			{
				Utilities.DebugFail("The cell's column index should never be more than 2 bytes wide.");
				return null;
			}

			// MD 7/26/10 - TFS34398
			// The cells only store the row reference now instead of the row index and worksheet.
			//return new WorksheetCell( this.Worksheet, this.row.Index, (short)index );
			return new WorksheetCell(this.row, (short)index);
		}

		#endregion CreateValue

		// MD 4/12/11 - TFS67084
		#region GetEnumeratorHelper

		internal override IEnumerator<WorksheetCell> GetEnumeratorHelper()
		{
			foreach (CellDataContext cellDataContext in this.row.GetCellsWithData())
				yield return this[cellDataContext.ColumnIndex];
		}

		// MD 12/1/11 - TFS96113
		// MD 2/16/12 - 12.1 - Table Support
		// Changed the name and added another parameter.
		//internal override IEnumerable<WorksheetCell> GetEnumeratorHelper(int startIndex, int endIndex)
		internal override IEnumerable<WorksheetCell> GetItemsInRange(int startIndex, int endIndex, bool enumerateForwards)
		{
			if (enumerateForwards)
			{
				Utilities.DebugFail("The cells collection doesn't yet support enumerating the cells backwards.");
				yield break;
			}

			foreach (CellDataContext cellDataContext in this.row.GetCellsWithData(startIndex, endIndex))
				yield return this[cellDataContext.ColumnIndex];
		}

		#endregion  // GetEnumeratorHelper

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
				// MD 6/18/12
				// Found while fixing TFS102878
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

			
			// MD 1/19/11 - TFS62268
			// Removed becasue ther eis no logic in the WorksheetCell.OnCurrentFormatChanged() method anymore.
			//foreach (WorksheetCell cell in this)
			//    cell.OnCurrentFormatChanged();
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
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//foreach ( WorksheetCell cell in this )
			//    cell.VerifyFormatLimits( limitErrors, testFormat );
		}

		#endregion VerifyFormatLimits

		#endregion Base Class Overrides

		#region Methods

		// MD 4/19/11 - TFS73111
		#region DirtyCellCount

		internal void DirtyCellCount()
		{
			this.cellCount = -1;
		}

		#endregion  // DirtyCellCount

		#endregion  // Methods

		#region Properties

		#region Indexer[ int ]

		/// <summary>
		/// Gets the cell at the specified column index in the owning row.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> Iterating the collection will not create all cells. It will only iterate the cells which have already 
		/// been used.  To create and iterate all cells in the worksheet use a For loop, iterating from 0 to one less than 
		/// the maximum column count, and pass in each index to the collection's indexer.
		/// </p>
		/// </remarks>
		/// <param name="index">The zero-based column index of the cell to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="index"/> is greater than or equal to <see cref="Workbook.MaxExcelColumnCount"/>
		/// or <see cref="Workbook.MaxExcel2007ColumnCount"/>, depending on the <see cref="Workbook.CurrentFormat"/>.
		/// </exception>
		/// <value>The cell at the specified column index in the owning row.</value>
		public WorksheetCell this[ int index ]
		{
			// MD 4/12/11 - TFS67084
			// We always want to recreate the cell objects when they are requested because we no longer root them.
			//get { return this.InternalIndexer( index ); }
			get
			{
				if (index < 0)
					throw new ArgumentOutOfRangeException("index", index, SR.GetString("LE_ArgumentOutOfRangeException_IndexNegative"));

				if (index >= this.MaxCount)
					this.ThrowIndexOutOfRangeException(index);

				return new WorksheetCell(this.row, (short)index);
			}
		}

		#endregion Indexer[ int ]

		#endregion Properties
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