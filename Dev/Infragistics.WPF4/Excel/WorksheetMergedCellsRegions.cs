using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// A collection of <see cref="WorksheetMergedCellsRegion"/> instances in a <see cref="Worksheet"/>.
	/// </summary>
	/// <seealso cref="WorksheetMergedCellsRegion"/>
	/// <seealso cref="Worksheet.MergedCellsRegions"/>
	[DebuggerDisplay( "Count = {Count}" )]



	public

		 sealed class WorksheetMergedCellsRegionCollection : 
		ICollection<WorksheetMergedCellsRegion>
	{
		#region Member Variables

		private List<WorksheetMergedCellsRegion> mergedRegions;
		private Worksheet parentWorksheet;

		#endregion Member Variables

		#region Constructor

		internal WorksheetMergedCellsRegionCollection( Worksheet parentWorksheet )
		{
			this.parentWorksheet = parentWorksheet;
			this.mergedRegions = new List<WorksheetMergedCellsRegion>();
		}

		#endregion Constructor

		#region Interfaces

		#region ICollection<WorksheetMergedCellsRegion> Members

		void ICollection<WorksheetMergedCellsRegion>.Add( WorksheetMergedCellsRegion item )
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantAddMergedRegion" ) );
		}

		void ICollection<WorksheetMergedCellsRegion>.Clear()
		{
			this.Clear();
		}

		bool ICollection<WorksheetMergedCellsRegion>.Contains( WorksheetMergedCellsRegion item )
		{
			return this.mergedRegions.Contains( item );
		}

		void ICollection<WorksheetMergedCellsRegion>.CopyTo( WorksheetMergedCellsRegion[] array, int arrayIndex )
		{
			this.mergedRegions.CopyTo( array, arrayIndex );
		}

		int ICollection<WorksheetMergedCellsRegion>.Count
		{
			get { return this.Count; }
		}

		bool ICollection<WorksheetMergedCellsRegion>.IsReadOnly
		{
			get { return false; }
		}

		bool ICollection<WorksheetMergedCellsRegion>.Remove( WorksheetMergedCellsRegion item )
		{
			return this.Remove( item );
		}

		#endregion

		#region IEnumerable<WorksheetMergedCellsRegion> Members

		IEnumerator<WorksheetMergedCellsRegion> IEnumerable<WorksheetMergedCellsRegion>.GetEnumerator()
		{
			return this.mergedRegions.GetEnumerator();
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.mergedRegions.GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Creates new merged cell region and adds it to the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The new merged cell region will take its value from the first cell containing a value, starting at the top-left and going across 
		/// then down in the region. The value of all other cells will be lost. Similarly, the new region will initialize its cell format 
		/// from the first cell containing a non-default cell format.
		/// </p>
		/// </remarks>
		/// <param name="firstRow">The index of the first row of the merged cell region.</param>
		/// <param name="firstColumn">The index of the first column of the merged cell region.</param>
		/// <param name="lastRow">The index of the last row of the merged cell region.</param>
		/// <param name="lastColumn">The index of the last row column of the merged cell region.</param>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="firstRow"/> is greater than <paramref name="lastRow"/> or 
		/// <paramref name="firstColumn"/> is greater than <paramref name="lastColumn"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Any row or column indices specified are outside the valid row or column ranges.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The new merged cell region overlaps with an existing merged cell region.
		/// </exception>
		/// <returns>The newly created <see cref="WorksheetMergedCellsRegion"/>.</returns>
		public WorksheetMergedCellsRegion Add( int firstRow, int firstColumn, int lastRow, int lastColumn )
		{
			// MD 4/12/11 - TFS67084
			//this.VerifyMergedRegionArea( firstRow, firstColumn, lastRow, lastColumn );
			this.VerifyMergedRegionArea(firstRow, (short)firstColumn, lastRow, (short)lastColumn);

			WorksheetMergedCellsRegion mergedRegion = new WorksheetMergedCellsRegion( this.parentWorksheet, firstRow, firstColumn, lastRow, lastColumn );

			this.mergedRegions.Add( mergedRegion );

			return mergedRegion;
		}

		#endregion Add

		#region Clear

		/// <summary>
		/// Clears all merged cell regions from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When a merged cell region is removed, the top-left cell in the region will retain the region's value.
		/// All other cells in the region will have a null value. In addition, all cells in the merged region will
		/// have the region's cell format when it is removed.
		/// </p>
		/// </remarks>
		public void Clear()
		{
			for ( int i = this.mergedRegions.Count - 1; i >= 0; i-- )
				this.RemoveAt( i );
		}

		#endregion Clear

		#region IsOverlappingWithMergedRegion

		/// <summary>
		/// Checks if any part of specified region is already a part of a merged cell region.
		/// </summary>
		/// <param name="firstRow">The index of the first row of the merged cell region.</param>
		/// <param name="firstColumn">The index of the first column of the merged cell region.</param>
		/// <param name="lastRow">The index of the last row of the merged cell region.</param>
		/// <param name="lastColumn">The index of the last row column of the merged cell region.</param>
		/// <returns>True if any part of specified region is a part of merged cell region; False otherwise.</returns>
		public bool IsOverlappingWithMergedRegion( int firstRow, int firstColumn, int lastRow, int lastColumn )
		{
			WorksheetRegion.VerifyRowOrder( firstRow, lastRow );
			WorksheetRegion.VerifyColumnOrder( firstColumn, lastColumn );

			// MD 6/31/08 - Excel 2007 Format
			//Utilities.VerifyRowIndex( firstRow, "firstRow" );
			//Utilities.VerifyRowIndex( lastRow, "lastRow" );
			//
			//Utilities.VerifyColumnIndex( firstColumn, "firstColumn" );
			//Utilities.VerifyColumnIndex( lastColumn, "lastColumn" );
			// MD 4/12/11
			// Found while fixing TFS67084
			// Cache the workbook.
			//Utilities.VerifyRowIndex( this.parentWorksheet.Workbook, firstRow, "firstRow" );
			//Utilities.VerifyRowIndex( this.parentWorksheet.Workbook, lastRow, "lastRow" );
			//Utilities.VerifyColumnIndex( this.parentWorksheet.Workbook, firstColumn, "firstColumn" );
			//Utilities.VerifyColumnIndex( this.parentWorksheet.Workbook, lastColumn, "lastColumn" );
			// MD 2/24/12 - 12.1 - Table Support
			// The workbook may be null.
			//Workbook workbook = this.parentWorksheet.Workbook;
			//Utilities.VerifyRowIndex(workbook, firstRow, "firstRow");
			//Utilities.VerifyRowIndex(workbook, lastRow, "lastRow");
			//Utilities.VerifyColumnIndex(workbook, firstColumn, "firstColumn");
			//Utilities.VerifyColumnIndex(workbook, lastColumn, "lastColumn");
			Utilities.VerifyRowIndex(this.parentWorksheet, firstRow, "firstRow");
			Utilities.VerifyRowIndex(this.parentWorksheet, lastRow, "lastRow");
			Utilities.VerifyColumnIndex(this.parentWorksheet, firstColumn, "firstColumn");
			Utilities.VerifyColumnIndex(this.parentWorksheet, lastColumn, "lastColumn");

			for ( int row = firstRow; row <= lastRow; row++ )
			{
				// MD 10/21/10 - TFS34398
				// Cache the row so we don't have to get it multiple times.
				WorksheetRow rowValue = this.parentWorksheet.Rows[row];

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//for ( int column = firstColumn; column <= lastColumn; column++ )
				//{
				//    // MD 10/21/10 - TFS34398
				//    // Use the cached row so we don't have to get it multiple times.
				//    //WorksheetCell wsCell = this.parentWorksheet.Rows[ row ].Cells[ column ];
				//    WorksheetCell wsCell = rowValue.Cells[column];
				//
				//    // MD 10/20/10 - TFS36617
				//    // Use the new HasAssociatedMergedCellsRegion property, which doesn't actually search for the region.
				//    //if ( wsCell.AssociatedMergedCellsRegion != null )
				//    if (wsCell.HasAssociatedMergedCellsRegion)
				//        return true;
				//}
				for (short columnIndex = (short)firstColumn; columnIndex <= lastColumn; columnIndex++)
				{
					if (rowValue.GetCellAssociatedMergedCellsRegionInternal(columnIndex) != null)
						return true;
				}
			}

			return false;
		}

		#endregion IsOverlappingWithMergedRegion

		#region Remove

		/// <summary>
		/// Removes the specified merged cell region from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When a merged cell region is removed, the top-left cell in the region will retain the region's value.
		/// All other cells in the region will have a null value. In addition, all cells in the merged region will
		/// have the region's cell format when it is removed.
		/// </p>
		/// </remarks>
		/// <param name="region">The merged cell region to remove from the collection.</param>
		/// <returns>
		/// True if the merged cell region was successfully removed; False if the merged cell region was not 
		/// in the collection.
		/// </returns>
		public bool Remove( WorksheetMergedCellsRegion region )
		{
			if ( region == null )
				return false;

			int index = this.mergedRegions.IndexOf( region );

			if ( index < 0 )
				return false;

			this.RemoveAt( index );
			return true;
		}

		#endregion Remove

		#region RemoveAt

		/// <summary>
		/// Removes the merged cell region at the specified index from the collection.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// When a merged cell region is removed, the top-left cell in the region will retain the region's value.
		/// All other cells in the region will have a null value. In addition, all cells in the merged region will
		/// have the region's cell format when it is removed.
		/// </p>
		/// </remarks>
		/// <param name="index">The zero-based index of the merged cell region in the collection.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		public void RemoveAt( int index )
		{
			if ( index < 0 || this.Count <= index )
				throw new ArgumentOutOfRangeException( "index", index, SR.GetString( "LE_ArgumentOutOfRangeException_CollectionIndex" ) );

			WorksheetMergedCellsRegion region = this.mergedRegions[ index ];
			this.mergedRegions.RemoveAt( index );
			region.OnRemovedFromCollection();
		}

		#endregion RemoveAt

		#endregion Public Methods

		#region Internal Methods

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

		internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat )
		{
			foreach ( WorksheetMergedCellsRegion region in this )
				region.VerifyFormatLimits( limitErrors, testFormat );
		}

		#endregion VerifyFormatLimits

		#endregion Internal Methods

		#region Private Methods

		#region VerifyMergedRegionArea

		// MD 4/12/11 - TFS67084
		// Use short instead of int so we don't have to cast.
		//private void VerifyMergedRegionArea( int firstRow, int firstColumn, int lastRow, int lastColumn )
		private void VerifyMergedRegionArea(int firstRow, short firstColumn, int lastRow, short lastColumn)
		{
			// Check to make sure the merged region will not overlap a data table incorrectly
			// (cell can be merged in the top row or left column of the data table region, but the
			// merged regioon cannot extend outside the data table region or extend into the interior 
			// of the table)
			if ( this.parentWorksheet.HasDataTables )
			{
				
				foreach ( WorksheetDataTable dataTable in this.parentWorksheet.DataTables )
				{
					// See if the data table region has already been set
					WorksheetRegion dataTableRegion = dataTable.CellsInTable;

					// If the new merged region will be in the current data table, check to make sure it overlaps it correctly
					if ( dataTableRegion.IntersectsWith( firstRow, firstColumn, lastRow, lastColumn ) )
					{
						bool invalidRegion = false;

						// If the merged region is confined to the top row of the data table, make sure it stays inside the table horizontally
						if ( firstRow == dataTableRegion.FirstRow && lastRow == dataTableRegion.FirstRow )
						{
							if ( firstColumn < dataTableRegion.FirstColumn || dataTableRegion.LastColumn < lastColumn )
								invalidRegion = true;
						}
						// If the merged region is confined to the left column of the data table, make sure it stays inside the table vertically
						else if ( firstColumn == dataTableRegion.FirstColumn && lastColumn == dataTableRegion.FirstColumn )
						{
							if ( firstRow < dataTableRegion.FirstRow || dataTableRegion.LastRow < lastRow )
								invalidRegion = true;
						}
						// If the merged region overlaps the data table, but is not confined to the top row of left column, it is invalid
						else
						{
							invalidRegion = true;
						}

						if ( invalidRegion )
							throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantOverlapDataTableInterior" ) );
					}
				}
			}

			
			// Make sure this region will not overlap with any other merged cell regions or array formulas
			for ( int rowIndex = firstRow; rowIndex <= lastRow; rowIndex++ )
			{
				WorksheetRow row = this.parentWorksheet.Rows[ rowIndex ];

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//for ( int columnIndex = firstColumn; columnIndex <= lastColumn; columnIndex++ )
				//{
				//    WorksheetCell cell = row.Cells[ columnIndex ];
				//
				//    // MD 10/20/10 - TFS36617
				//    // Use the new HasAssociatedMergedCellsRegion property, which doesn't actually search for the region.
				//    //if ( cell.AssociatedMergedCellsRegion != null )
				//    if (cell.HasAssociatedMergedCellsRegion)
				//        throw new InvalidOperationException( SR.GetString( "LER_Exception_MergedRegionsOverlap" ) );
				//
				//    // MD 7/14/08 - Excel formula solving
				//    // The Value property no longer returns the Formula instance on the cell.
				//    //if ( cell.Value is ArrayFormula )
				//    if ( cell.Formula is ArrayFormula )
				//        throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantOverlapArrayFormula" ) );
				//}
				for (short columnIndex = firstColumn; columnIndex <= lastColumn; columnIndex++)
				{
					if (row.GetCellAssociatedTable(columnIndex) != null)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_MergedCellsAppliedInTable"));

					if (row.GetCellAssociatedMergedCellsRegionInternal(columnIndex) != null)
						throw new InvalidOperationException(SR.GetString("LER_Exception_MergedRegionsOverlap"));

					if (row.GetCellValueRaw(columnIndex) is ArrayFormula)
						throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_CantOverlapArrayFormula"));
				}
			}
		}

		#endregion VerifyMergedRegionArea

		#endregion Private Methods

		#endregion Methods

		#region Properties

		#region Count

		/// <summary>
		/// Gets the number of merged cell regions in the collection.
		/// </summary>
		/// <value>The number of merged cell regions in the collection.</value>
		public int Count
		{
			get { return this.mergedRegions.Count; }
		}

		#endregion Count

		#region Indexer [ int ]

		/// <summary>
		/// Gets the merged cell region at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the merged cell region to get.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index"/> is less than zero or <paramref name="index"/> is greater than or 
		/// equal to <see cref="Count"/>.
		/// </exception>
		/// <value>The merged cell region at the specified index.</value>
		public WorksheetMergedCellsRegion this[ int index ]
		{
			get { return this.mergedRegions[ index ]; }
		}

		#endregion Indexer [ int ]

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