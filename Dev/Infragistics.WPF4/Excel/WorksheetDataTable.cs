using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a data table for a range of cells.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Data tables are a way to show the results of one or more formulas by trying many different values in the variables
	/// of the formulas at the same time, and showing the different results in a table.  An excellent example of a data table
	/// application would be for a multiplication table.  A multiplication table shows results for the formula =X*Y.  However, 
	/// it shows many different results for the formula, where each result is determined by using different values for X and Y.
	/// These results are displayed in a table, where each cell in the table shows the formula's result for specific values of 
	/// X and Y, which are labeled in the row and column headers, respectively.  Therefore, each cell in a row will use the 
	/// same X value, and each cell in a column will use the same Y value.  
	/// </p>
	/// <p class="body">
	/// The multiplication table is known as a two-variable data table. Two-variable data tables are characterized by having a 
	/// single formula and values in the row and column headers of the table.  The formula is entered into the top-left cell of 
	/// the data table and usually references at least two cells outside the data table, known as the column-input and row-input 
	/// cells.  When the formula is evaluated for a specific cell in the data table, the reference to the column-input cell in the
	/// formula is replaced with the value in the cell's row header (this may seem backwards, but the values in the row headers 
	/// run down the left column of the data table, which is why they are used for the column-input cell), and the reference to 
	/// the row-input cell is replaced with the value in the cell's column header.
	/// </p>
	/// <p class="body">
	/// Another type of data table is the one-variable data table.  A one-variable data table can be a column-oriented or 
	/// a row-oriented data table.  A column-oriented data table has data in the cells of the left column and formulas in the 
	/// cells of the top row (anything in the top-left cell of the data table is ignored in this type of data table).  Usually, 
	/// the formulas in the top row all reference the same cell outside the data table, known as the column-input cell.  When a 
	/// cell in the table is evaluated, the formula in its column header is used, with the reference to the column-input cell
	/// replaced by the value in cell's row header.
	/// </p>
	/// <p class="body">
	/// A row-oriented one-variable data table is formed like a column-oriented data table, except the values run along the top 
	/// row, the formulas run down the left column, and the cell referenced by all formulas is known as the row-input cell.
	/// </p>
	/// </remarks>



	public

		 class WorksheetDataTable :
		IWorksheetRegionBlockingValue
	{
		#region Member Variables

		private Worksheet worksheet;

		// MD 3/12/12 - 12.1 - Table Support
		//private WorksheetRegion cellsInTable;
		//private WorksheetRegion interiorCells;
		//
		//private WorksheetCell columnInputCell;
		//private WorksheetCell rowInputCell;
		private WorksheetRegionAddress cellsInTableAddress;

		private bool columnInputCellIsValid;
		private int columnInputCellRowIndex;
		private short columnInputCellColumnIndex;

		private bool rowInputCellIsValid;
		private int rowInputCellRowIndex;
		private short rowInputCellColumnIndex;

		// MD 7/26/10 - TFS34398
		// The formulae and data tables will now cache the calculated values instead of the cells.
		private object[,] calculatedValues;

		#endregion Member Variables

		#region Constructor

		internal WorksheetDataTable( Worksheet worksheet, WorksheetRegion cellsInTable, WorksheetCell columnInputCell, WorksheetCell rowInputCell )
		{
			// MD 3/13/12 - 12.1 - Table Support
			// This member must be marked invalid when we set the input cells.
			this.cellsInTableAddress.SetInvalid();

			this.worksheet = worksheet;

			// MD 3/12/12 - 12.1 - Table Support
			//this.columnInputCell = columnInputCell;
			//this.rowInputCell = rowInputCell;
			this.SetColumnInputCell(columnInputCell);
			this.SetRowInputCell(rowInputCell);

			this.CellsInTable = cellsInTable;
		}

		#endregion Constructor

		#region Interfaces

		#region IWorksheetRegionBlockingValue Members

		void IWorksheetRegionBlockingValue.RemoveFromRegion()
		{
			if ( this.worksheet != null )
				this.worksheet.DataTables.Remove( this );

			// After removing, the worksheet should be set to null
			Debug.Assert( this.worksheet == null );
		}

		void IWorksheetRegionBlockingValue.ThrowBlockingException()
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantChangeDataTable" ) );
		}

		void IWorksheetRegionBlockingValue.ThrowExceptionWhenMergedCellsInRegion()
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_DataTableInMergedCell" ) );
		}

		void IWorksheetRegionBlockingValue.ThrowExceptionWhenTableInRegion()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_DataTableAppliedInTable"));
		}

		WorksheetRegion IWorksheetRegionBlockingValue.Region
		{
			get 
			{
				// MD 7/21/08 - Excel formula solving
				// This is now set in the SetCellsInTableInternal method so it can be used in other places.
				#region Moved

				
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)


				#endregion Moved
				return this.InteriorCells;
			}
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Internal Methods

		// MD 2/8/12 - 12.1 - Table Support
		#region GetDisplayFormula

		internal string GetDisplayFormula()
		{
			// MD 4/9/12 - TFS101506
			//string separator = FormulaParser.GetUnionOperatorResolved();
			CultureInfo culture = CultureInfo.CurrentCulture;

			CellReferenceMode cellReferenceMode = CellReferenceMode.A1;
			if (this.Worksheet != null)
			{
				cellReferenceMode = this.Worksheet.CellReferenceMode;

				// MD 4/9/12 - TFS101506
				culture = this.Worksheet.Culture;
			}

			// MD 4/9/12 - TFS101506
			string separator = FormulaParser.GetUnionOperatorResolved(culture);

			string rowInputCellString = null;
			WorksheetCell rowInputCell = this.RowInputCell;
			if (rowInputCell != null)
				rowInputCellString = rowInputCell.ToString(cellReferenceMode, false, true, true);

			string columnInputCellString = null;
			WorksheetCell columnInputCell = this.ColumnInputCell;
			if (columnInputCell != null)
				columnInputCellString = columnInputCell.ToString(cellReferenceMode, false, true, true);

			return String.Format("=TABLE({0}{1}{2})", rowInputCellString, separator, columnInputCellString);
		}

		#endregion // GetDisplayFormula

		// MD 7/21/08 - Excel formula solving
		#region GetInteriorCellFormula







		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		#region Old Code

		//internal Formula GetInteriorCellFormula(WorksheetCell cell)
		//{
		//    if (this.interiorCells == null ||
		//        this.interiorCells.Contains(cell) == false)
		//    {
		//        Utilities.DebugFail("The specified cell is not in the interior cells of the table.");
		//        return null;
		//    }
		//
		//    WorksheetCell formulaOwner;
		//
		//    if (this.RowInputCell != null && this.ColumnInputCell != null)
		//    {
		//        // In a two-variable data table, the top-left cell is the formula owner for all cells in the table.
		//        formulaOwner = this.CellsInTable.TopLeftCell;
		//    }
		//    else if (this.RowInputCell != null)
		//    {
		//        // In a one-variable data table with a row input cell, the left-most cell in the same row as the specified 
		//        // cell owns the formula for the cell.
		//        // MD 7/26/10 - TFS34398
		//        // Now the cell stores the row directly, so just use that.
		//        //formulaOwner = this.Worksheet.Rows[ cell.RowIndex ].Cells[ this.CellsInTable.FirstColumn ];
		//        formulaOwner = cell.Row.Cells[this.CellsInTable.FirstColumn];
		//    }
		//    else
		//    {
		//        Debug.Assert(this.ColumnInputCell != null, "The column input cell shouls not be null.");
		//
		//        // In a one-variable data table with a column input cell, the top-most cell in the same column as the specified 
		//        // cell owns the formula for the cell.
		//        formulaOwner = this.Worksheet.Rows[this.CellsInTable.FirstRow].Cells[cell.ColumnIndex];
		//    }
		//
		//    Formula formula = formulaOwner.Formula;
		//
		//    // If the formula owner for the interior cell has no formula, use a formula that points to the owner cell's value 
		//    // instead.
		//    if (formula == null)
		//    {
		//        return Formula.Parse(
		//            "=" + formulaOwner.ToString(CellReferenceMode.A1, false, true, true),
		//            CellReferenceMode.A1,
		//            cell.Worksheet.CurrentFormat);
		//    }
		//
		//    // The formula for the cell now depends on the value of the formula owner cell: if the value of formula owner
		//    // changes, the formula for the dependant cell will have to be regenerated. Add the dependant cell to the collection
		//    // which the owner will notify when the value changes.
		//    formulaOwner.DataTableDependantCells.Add(cell);
		//
		//    return formula;
		//}  

		#endregion // Old Code
		internal Formula GetInteriorCellFormula(WorksheetRow row, short columnIndex)
		{
			// MD 3/12/12 - 12.1 - Table Support
			//if ( this.interiorCells == null ||
			//    // MD 2/29/12 - 12.1 - Table Support
			//    // The worksheet can now be null.
			//    this.interiorCells.Worksheet == null || 
			//    this.interiorCells.Contains(row, columnIndex) == false)
			//{
			//    Utilities.DebugFail( "The specified cell is not in the interior cells of the table." );
			//    return null;
			//}
			WorksheetRegion interiorCells = this.InteriorCells;
			if (interiorCells == null ||
				interiorCells.Worksheet == null ||
				interiorCells.Contains(row, columnIndex) == false)
			{
				Utilities.DebugFail("The specified cell is not in the interior cells of the table.");
				return null;
			}

			WorksheetRow formulaOwnerRow;
			short formulaOwnerColumnIndex;

			WorksheetCell rowInputCell = this.RowInputCell;

			// MD 3/12/12 - 12.1 - Table Support
			//if ( this.RowInputCell != null && this.ColumnInputCell != null )
			if (rowInputCell != null && this.ColumnInputCell != null)
			{
				// In a two-variable data table, the top-left cell is the formula owner for all cells in the table.
				formulaOwnerRow = this.CellsInTable.TopRow;
				formulaOwnerColumnIndex = this.CellsInTable.FirstColumnInternal;
			}
			// MD 3/12/12 - 12.1 - Table Support
			//else if ( this.RowInputCell != null )
			else if (rowInputCell != null)
			{
				// In a one-variable data table with a row input cell, the left-most cell in the same row as the specified 
				// cell owns the formula for the cell.
				formulaOwnerRow = row;
				formulaOwnerColumnIndex = this.CellsInTable.FirstColumnInternal;
			}
			else
			{
				Debug.Assert( this.ColumnInputCell != null, "The column input cell should not be null." );

				// In a one-variable data table with a column input cell, the top-most cell in the same column as the specified 
				// cell owns the formula for the cell.
				formulaOwnerRow = this.CellsInTable.TopRow;
				formulaOwnerColumnIndex = columnIndex;
			}

			WorksheetCellBlock formulaOwnerCellBlock = formulaOwnerRow.GetCellBlock(formulaOwnerColumnIndex);
			Formula formula = formulaOwnerCellBlock.GetFormula(formulaOwnerRow, formulaOwnerColumnIndex);

			// If the formula owner for the interior cell has no formula, use a formula that points to the owner cell's value 
			// instead.
			if ( formula == null )
			{
				return Formula.Parse(
					"=" + formulaOwnerRow.GetCellAddressString(formulaOwnerColumnIndex, CellReferenceMode.A1, false, true, true),
					CellReferenceMode.A1,
					row.Worksheet.CurrentFormat );
			}

			// The formula for the cell now depends on the value of the formula owner cell: if the value of formula owner
			// changes, the formula for the dependant cell will have to be regenerated. Add the dependant cell to the collection
			// which the owner will notify when the value changes.
			// MD 3/27/12 - 12.1 - Table Support
			//formulaOwnerRow.GetDataTableDependantCells(formulaOwnerColumnIndex).Add(new WorksheetCellAddress(row, columnIndex));
			formulaOwnerRow.GetDataTableDependantCells(formulaOwnerColumnIndex).Add(new WorksheetCellAddress(row.Index, columnIndex));

			return formula;
		} 

		#endregion GetInteriorCellFormula

		// MD 3/12/12 - 12.1 - Table Support
		#region OnCellsShifted






		internal bool OnCellsShifted(CellShiftOperation shiftOperation)
		{
			if (this.worksheet == null)
				return false;

			ShiftAddressResult result;

			bool tableElementsShifted = false;
			if (this.columnInputCellRowIndex >= 0)
			{
				result = shiftOperation.ShiftCellAddress(ref this.columnInputCellRowIndex, this.columnInputCellColumnIndex);
				tableElementsShifted |= result.DidShift;

				if (result.IsDeleted)
				{
					this.columnInputCellRowIndex = -1;
					this.columnInputCellColumnIndex = -1;
				}
			}

			if (this.rowInputCellRowIndex >= 0)
			{
				result = shiftOperation.ShiftCellAddress(ref this.rowInputCellRowIndex, this.rowInputCellColumnIndex);
				tableElementsShifted |= result.DidShift;

				if (result.IsDeleted)
				{
					this.rowInputCellRowIndex = -1;
					this.rowInputCellColumnIndex = -1;
				}
			}

			if (this.cellsInTableAddress.IsValid)
			{
				result = shiftOperation.ShiftRegionAddress(ref this.cellsInTableAddress, false);
				tableElementsShifted |= result.DidShift;
				Debug.Assert(result.IsDeleted == false, "The data tables cannot be deleted by a shift.");

				// Before the cell shift operation, we temporarily removed the data table from the region so value shifting
				// would just be shifting nulls for the data table and we wouldn't have to worry about the top-left cell
				// being moved last so other cells could continue to reference it.
				this.SetCellsInTableInternal(this.worksheet.GetCachedRegion(this.cellsInTableAddress));
			}

			return tableElementsShifted;
		}

		#endregion // OnCellsShifted

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnCellsShifting






		internal void OnCellsShifting(CellShiftOperation shiftOperation)
		{
			// While we are shifting cell values, we don't want to have to worry about the top-left cell being moved last so 
			// other cells could continue to reference it, because it actually isn't possible in some situations, so temporarily
			// remove the data table from the shifting region.
			if (this.cellsInTableAddress.IsValid &&
				shiftOperation.RegionAddressBeforeShift.Contains(this.cellsInTableAddress))
			{
				WorksheetRegionAddress originalCellsInTableAddress = this.cellsInTableAddress;
				this.SetCellsInTableInternal(null);
				this.cellsInTableAddress = originalCellsInTableAddress;
			}
		}

		#endregion // OnCellsShifting

		#region OnRemovedFromCollection






		internal void OnRemovedFromCollection()
		{
			this.SetCellsInTableInternal( null );

			// MD 3/12/12 - 12.1 - Table Support
			//this.columnInputCell = null;
			//this.rowInputCell = null;
			this.SetColumnInputCell(null);
			this.SetRowInputCell(null);

			this.worksheet = null;
		}

		#endregion OnRemovedFromCollections

		#region VerifyInputCells

		internal static void VerifyTableElements( Worksheet worksheet, 
			WorksheetRegion cellsInTable, string cellsInTableParam,
			WorksheetCell columnInputCell, string columnInputCellParam, 
			WorksheetCell rowInputCell, string rowInputCellParam )
		{
			if ( worksheet == null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_DataTableRemoved" ) );

			if ( cellsInTable == null )
			{
				Debug.Assert( cellsInTableParam != null );
				throw new ArgumentNullException( cellsInTableParam );
			}

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cellsInTable.Worksheet == null)
			{
				Debug.Assert(cellsInTableParam != null);
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), cellsInTableParam);
			}

			if ( cellsInTable.Worksheet != worksheet )
			{
				Debug.Assert( cellsInTableParam != null );
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_CellsInTableFromOtherWorksheet" ), cellsInTableParam );
			}

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			//if ( columnInputCell != null && columnInputCell.Worksheet != worksheet )
			//{
			//    Debug.Assert( columnInputCellParam != null );
			//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_ColumnInputCellFromOtherWorksheet" ), columnInputCellParam );
			//}
			//
			//if ( rowInputCell != null && rowInputCell.Worksheet != worksheet )
			//{
			//    Debug.Assert( rowInputCellParam != null );
			//    throw new ArgumentException( SR.GetString( "LE_ArgumentException_RowInputCellFromOtherWorksheet" ), rowInputCellParam );
			//}
			if (columnInputCell != null)
			{
				if (columnInputCell.Worksheet == null)
				{
					Debug.Assert(columnInputCellParam != null);
					throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), columnInputCellParam);
				}

				if (columnInputCell.Worksheet != worksheet)
				{
					Debug.Assert(columnInputCellParam != null);
					throw new ArgumentException(SR.GetString("LE_ArgumentException_ColumnInputCellFromOtherWorksheet"), columnInputCellParam);
				}
			}

			if (rowInputCell != null)
			{
				if (rowInputCell.Worksheet == null)
				{
					Debug.Assert(rowInputCellParam != null);
					throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), rowInputCellParam);
				}

				if (rowInputCell.Worksheet != worksheet)
				{
					Debug.Assert(rowInputCellParam != null);
					throw new ArgumentException(SR.GetString("LE_ArgumentException_RowInputCellFromOtherWorksheet"), rowInputCellParam);
				}
			}

			if ( columnInputCell == null && rowInputCell == null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_InputCellsBothNull" ) );

			if ( columnInputCell == rowInputCell )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_InputCellsSame" ) );

			WorksheetDataTable.VerifyCellOutsideRange( cellsInTable, columnInputCell );
			WorksheetDataTable.VerifyCellOutsideRange( cellsInTable, rowInputCell );
		}

		#endregion VerifyInputCells

		#endregion Internal Methods

		#region Private Methods

		#region SetCellsInTableInternal






		private void SetCellsInTableInternal( WorksheetRegion value )
		{
			// MD 8/12/08 - Excel formula solving
			// The interior cells must be calculated in here for verifying the values in the blocked cells.
			WorksheetRegion newInteriorCells = null;

			if ( value != null )
			{
				#region Verify the minimum region size

				if ( value.FirstRow == value.LastRow || value.FirstColumn == value.LastColumn )
					throw new ArgumentException( SR.GetString( "LE_InvalidOperationException_CellsInTableMinSize" ), "value" );

				#endregion Verify the minimum region size

				#region Make sure no invalid merged regions are in the exterior cells

				// Check the left column to make sure no merged regions extend outside the data table region
				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//int leftColumnIndex = value.FirstColumn;
				short leftColumnIndex = value.FirstColumnInternal;

				for ( int rowIndex = value.FirstRow + 1; rowIndex <= value.LastRow; rowIndex++ )
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//WorksheetCell cell = this.worksheet.Rows[ rowIndex ].Cells[ leftColumnIndex ];
					//WorksheetMergedCellsRegion mergedRegion = cell.AssociatedMergedCellsRegion;
					WorksheetMergedCellsRegion mergedRegion = this.worksheet.Rows[rowIndex].GetCellAssociatedMergedCellsRegionInternal(leftColumnIndex);

					if ( mergedRegion == null )
						continue;

					// If the merged region extends outside the left column of the data table region, throw an error
					if ( mergedRegion.FirstColumn < leftColumnIndex || leftColumnIndex < mergedRegion.LastColumn ||
						mergedRegion.FirstRow < value.FirstRow || value.LastRow < mergedRegion.LastRow )
					{
						throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MergedCellCrossesDataTableLeftColumn" ) );
					}
				}

				// Check the top row to make sure no merged regions extend outside the data table region
				// MD 4/12/11 - TFS67084
				//int topRowIndex = value.FirstRow;
				//WorksheetRow topRow = this.worksheet.Rows[topRowIndex];
				WorksheetRow topRow = value.TopRow;

				// MD 2/29/12 - 12.1 - Table Support
				// The TopRow can now be null.
				if (topRow == null)
				{
					Utilities.DebugFail("This is unexpected");
					return;
				}

				// MD 4/12/11 - TFS67084
				// Use short instead of int so we don't have to cast.
				//for ( int colIndex = value.FirstColumn + 1; colIndex <= value.LastColumn; colIndex++ )
				for (short colIndex = (short)(value.FirstColumnInternal + 1); colIndex <= value.LastColumnInternal; colIndex++)
				{
					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects.
					//WorksheetCell cell = topRow.Cells[ colIndex ];
					//WorksheetMergedCellsRegion mergedRegion = cell.AssociatedMergedCellsRegion;
					WorksheetMergedCellsRegion mergedRegion = topRow.GetCellAssociatedMergedCellsRegionInternal(colIndex);

					if ( mergedRegion == null )
						continue;

					// If the merged region extends outside the top row of the data table region, throw an error
					if ( mergedRegion.FirstRow < topRow.Index || topRow.Index < mergedRegion.LastRow ||
						mergedRegion.FirstColumn < value.FirstColumn || value.LastColumn < mergedRegion.LastColumn )
					{
						throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MergedCellCrossesDataTableTopRow" ) );
					}
				}

				// Check the top-left cell for a merged cell region
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell topLeftCell = this.worksheet.Rows[ value.FirstRow ].Cells[ value.FirstColumn ];
				//WorksheetMergedCellsRegion topLeftMergedRegion = topLeftCell.AssociatedMergedCellsRegion;
				WorksheetMergedCellsRegion topLeftMergedRegion = topRow.GetCellAssociatedMergedCellsRegionInternal(value.FirstColumnInternal);

				if ( topLeftMergedRegion != null )
				{
					if ( topLeftMergedRegion.FirstRow == topLeftMergedRegion.LastRow )
					{
						if ( topLeftMergedRegion.FirstColumn < value.FirstColumn || value.LastColumn < topLeftMergedRegion.LastColumn )
							throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MergedCellCrossesDataTableTopRow" ) );
					}
					else if ( topLeftMergedRegion.FirstColumn == topLeftMergedRegion.LastColumn )
					{
						if ( topLeftMergedRegion.FirstRow < value.FirstRow || value.LastRow < topLeftMergedRegion.LastRow )
							throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MergedCellCrossesDataTableLeftColumn" ) );
					}
					else
					{
						throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_MergedCellCrossesDataTableTopLeftCell" ) );
					}
				}

				#endregion Make sure no invalid merged regions are in the exterior cells

				// MD 8/12/08 - Excel formula solving
				// Create the interior cell range for verifying the cells before applying the table to the region.
				newInteriorCells = this.worksheet.GetCachedRegion(
					value.FirstRow + 1,
					value.FirstColumn + 1,
					value.LastRow,
					value.LastColumn );

				// MD 8/12/08 - Excel formula solving
				// Use the interior cells, not the entire table region.
				// Verify that there are no values in the interior cells which would prevent this data table from being set
				//WorksheetCell.VerifyRegionForBlockingValue( this, value );
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//WorksheetCell.VerifyRegionForBlockingValue( this, newInteriorCells );
				// MD 3/2/12 - 12.1 - Table Support
				//WorksheetCellBlock.VerifyRegionForBlockingValue(this, newInteriorCells);
				WorksheetCellBlock.VerifyRegionForBlockingValue(this, value, newInteriorCells);
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell.ClearBlockingValue( this );
			WorksheetCellBlock.ClearBlockingValue(this);

			// MD 3/12/12 - 12.1 - Table Support
			//this.cellsInTable = value;

			// MD 8/12/08 - Excel formula solving
			// Cache the interior cells.
			//this.interiorCells = newInteriorCells;

			// MD 7/26/10 - TFS34398
			// Maintain the cached calculated values collection.
			if (value == null)
			{
				// MD 3/12/12 - 12.1 - Table Support
				this.cellsInTableAddress.SetInvalid();
				this.calculatedValues = null;
			}
			else
			{
				// MD 3/12/12 - 12.1 - Table Support
				//this.calculatedValues = new object[this.interiorCells.Width, this.interiorCells.Height];
				this.cellsInTableAddress = value.Address;
				this.calculatedValues = new object[newInteriorCells.Width, newInteriorCells.Height];
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell.ApplyBlockingValue( this );
			WorksheetCellBlock.ApplyBlockingValue(this);
		}

		#endregion SetCellsInTableInternal

		#region VerifyCellOutsideRange



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static void VerifyCellOutsideRange( WorksheetRegion range, WorksheetCell cell )
		{
			// If the new or existing cell is null, no verification can take place
			if ( cell == null )
				return;

			// If the cell is within the range, whatever property is being set is not allowed
			// MD 7/26/10 - TFS34398
			// Now that the row is stored on the cell, the RowIndex getter is now a bit slower, so cache it.
			//if ( range.FirstColumn <= cell.ColumnIndex && cell.ColumnIndex <= range.LastColumn &&
			//    range.FirstRow <= cell.RowIndex && cell.RowIndex <= range.LastRow )
			int columnIndex = cell.ColumnIndex;
			int rowIndex = cell.RowIndex;
			if (range.FirstColumn <= columnIndex && columnIndex <= range.LastColumn &&
				range.FirstRow <= rowIndex && rowIndex <= range.LastRow)
			{
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_InputCellsInTable" ) );
			}
		}

		#endregion VerifyCellOutsideRange

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 7/26/10 - TFS34398
		#region CalculatedValues

		internal object[,] CalculatedValues
		{
			get { return this.calculatedValues; }
		} 

		#endregion // CalculatedValues

		#region CellsInTable

		/// <summary>
		/// Gets or sets the region of cells in the data table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Any interior cells (cells not in the left-most column or top row) in this region will have their values removed 
		/// when this is set. If any interior cells have array formulas with a region that crosses outside the data
		/// table, an error will occur. However, if the array formula's region is confined to cells in the 
		/// interior of the data table, the array formula will have <see cref="ArrayFormula.ClearCellRange"/>
		/// called on it, which will remove it from all its cells. Similarly, if an existing data table's interior cells
		/// contain some of the interior cells in this region as well as some external cells, an error will occur.
		/// However, if all interior cells of the existing data table are contained in the interior cells of the new
		/// region specified here, the existing data table will be removed from the worksheet.
		/// </p>
		/// <p class="body">
		/// After the cells in the table have been specified, the interior cells' values cannot be modified.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> The <see cref="RowInputCell"/> and <see cref="ColumnInputCell"/> cannot be with the region.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned after the data table has been removed from the worksheet.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is a region from a worksheet other than the data table's worksheet.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region specified contains the RowInputCell or the ColumnInputCell.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// One or more of the interior cells of the value assigned (all cells except the left-most column and top row) is an 
		/// interior cell of another data table or is a cell in an array formula, and the entire range of that other 
		/// entity extends outside the interior cells of the value assigned.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The assigned value is only one row tall or one column wide. The cells in the table must be at least two rows by two columns.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The assigned value contains merged regions which are not confined to the left-most column or top row. No merged regions
		/// can exist in the interior cells of the data table and no merged regions can exist in the left-most column or top row
		/// and extend outside the data table region.
		/// </exception>
		/// <value>The region of cells in the data table.</value>
		/// <seealso cref="ArrayFormula"/>
		/// <seealso cref="ColumnInputCell"/>
		/// <seealso cref="RowInputCell"/>
		public WorksheetRegion CellsInTable
		{
			// MD 3/12/12 - 12.1 - Table Support
			//get { return this.cellsInTable; }
			get
			{
				if (this.worksheet == null || this.cellsInTableAddress.IsValid == false)
					return null;

				return worksheet.GetCachedRegion(this.cellsInTableAddress);
			}
			set
			{
				
				// MD 3/12/12 - 12.1 - Table Support
				//if ( this.cellsInTable != value )
				WorksheetRegion cellsInTable = this.CellsInTable;
				if (Object.Equals(cellsInTable, value) == false)
				{
					WorksheetDataTable.VerifyTableElements(
						this.worksheet,
						value, "value",
						// MD 3/12/12 - 12.1 - Table Support
						//this.columnInputCell, null,
						//this.rowInputCell, null );
						this.ColumnInputCell, null,
						this.RowInputCell, null);

					this.SetCellsInTableInternal( value );
				}
			}
		}

		#endregion CellsInTable

		// MD 3/13/12 - 12.1 - Table Support
		#region CellsInTableAddress

		internal WorksheetRegionAddress CellsInTableAddress
		{
			get { return this.cellsInTableAddress; }
		}

		#endregion CellsInTableAddress

		#region ColumnInputCell

		/// <summary>
		/// Gets or sets the cell used as the column-input cell in the data table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This represents the cell reference in all formulas of the data table to replace with row header values.
		/// If this and the <see cref="RowInputCell"/> are non-null, the data table is a two-variable data table.
		/// Otherwise, if only one is non-null, this is a one-variable data table.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> The RowInputCell and ColumnInputCell cannot be within the <see cref="CellsInTable"/> region.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The assigned value does not belong to the same worksheet as the data table.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned after the data table has been removed from the worksheet.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is within the CellsInTable region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The assigned value is null and RowInputCell is null. At least one input cell must be non-null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The assigned value is the same as RowInputCell. The input cells cannot be the same cell.
		/// </exception>
		/// <value>The cell used as the column-input cell in the data table.</value>
		/// <seealso cref="CellsInTable"/>
		/// <seealso cref="RowInputCell"/>
		public WorksheetCell ColumnInputCell
		{
			// MD 3/12/12 - 12.1 - Table Support
			//get { return this.columnInputCell; }
			get
			{
				if (this.worksheet == null || this.columnInputCellRowIndex == -1)
				{
					if (this.columnInputCellIsValid)
						return WorksheetCell.InvalidReference;

					return null;
				}

				return this.worksheet.Rows[this.columnInputCellRowIndex].Cells[this.columnInputCellColumnIndex];
			}
			set 
			{
				// MD 3/12/12 - 12.1 - Table Support
				//if ( this.columnInputCell != value )
				if (this.ColumnInputCell != value)
				{
					WorksheetDataTable.VerifyTableElements( 
						this.worksheet,
						// MD 3/12/12 - 12.1 - Table Support
						//this.cellsInTable, null,
						this.CellsInTable, null,
						value, "value",
						// MD 3/12/12 - 12.1 - Table Support
						//this.rowInputCell, null );
						this.RowInputCell, null);

					// MD 3/12/12 - 12.1 - Table Support
					//this.columnInputCell = value;
					this.SetColumnInputCell(value);
				}
			}
		}

		private void SetColumnInputCell(WorksheetCell value)
		{
			if (value == null)
			{
				this.columnInputCellIsValid = false;
				this.columnInputCellRowIndex = -1;
				this.columnInputCellColumnIndex = -1;
			}
			else
			{
				this.columnInputCellIsValid = true;
				this.columnInputCellRowIndex = value.RowIndex;
				this.columnInputCellColumnIndex = value.ColumnIndexInternal;
			}
		}
		
		#endregion ColumnInputCell

		// MD 8/12/08 - Excel formula solving
		#region InteriorCells

		internal WorksheetRegion InteriorCells
		{
			// MD 3/12/12 - 12.1 - Table Support
			//get { return this.interiorCells; }
			get
			{
				if (this.worksheet == null || this.cellsInTableAddress.IsValid == false)
					return null;

				return worksheet.GetCachedRegion(
					this.cellsInTableAddress.FirstRowIndex + 1, this.cellsInTableAddress.FirstColumnIndex + 1,
					this.cellsInTableAddress.LastRowIndex, this.cellsInTableAddress.LastColumnIndex);
			}
		}

		#endregion InteriorCells

		#region RowInputCell

		/// <summary>
		/// Gets or sets the cell used as the row-input cell in the data table.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This represents the cell reference in all formulas of the data table to replace with column header values.
		/// If this and the <see cref="ColumnInputCell"/> are non-null, the data table is a two-variable data table.
		/// Otherwise, if only one is non-null, this is a one-variable data table.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> The RowInputCell and ColumnInputCell cannot be within the <see cref="CellsInTable"/> region.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The assigned value does not belong to the same worksheet as the data table.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value is assigned after the data table has been removed from the worksheet.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is within the CellsInTable region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The assigned value is null and ColumnInputCell is null. At least one input cell must be non-null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The assigned value is the same as ColumnInputCell. The input cells cannot be the same cell.
		/// </exception>
		/// <value>The cell used as the row-input cell in the data table.</value>
		/// <seealso cref="CellsInTable"/>
		/// <seealso cref="ColumnInputCell"/>
		public WorksheetCell RowInputCell
		{
			// MD 3/12/12 - 12.1 - Table Support
			//get { return this.rowInputCell; }
			get
			{
				if (this.worksheet == null || this.rowInputCellRowIndex == -1)
				{
					if (this.rowInputCellIsValid)
						return WorksheetCell.InvalidReference;

					return null;
				}

				return this.worksheet.Rows[this.rowInputCellRowIndex].Cells[this.rowInputCellColumnIndex];
			}
			set
			{
				// MD 3/12/12 - 12.1 - Table Support
				//if ( this.rowInputCell != value )
				if (this.RowInputCell != value)
				{
					WorksheetDataTable.VerifyTableElements( 
						this.worksheet,
						// MD 3/12/12 - 12.1 - Table Support
						//this.cellsInTable, null,
						this.CellsInTable, null,
						// MD 3/12/12 - 12.1 - Table Support
						//this.columnInputCell, null, 
						this.ColumnInputCell, null, 
						value, "value" );

					// MD 3/12/12 - 12.1 - Table Support
					//this.rowInputCell = value;
					this.SetRowInputCell(value);
				}
			}
		}

		private void SetRowInputCell(WorksheetCell value)
		{
			if (value == null)
			{
				this.rowInputCellIsValid = false;
				this.rowInputCellRowIndex = -1;
				this.rowInputCellColumnIndex = -1;
			}
			else
			{
				this.rowInputCellIsValid = true;
				this.rowInputCellRowIndex = value.RowIndex;
				this.rowInputCellColumnIndex = value.ColumnIndexInternal;
			}
		}

		#endregion RowInputCell

		#region Worksheet

		/// <summary>
		/// Gets the worksheet on which this data table resides.
		/// </summary>
		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

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