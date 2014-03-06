using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities.Tokens;







using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a formula for a cell or group of cells.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Formulas provide a way to show dynamic values in a cell. The value could be based any number of factors, such as 
	/// the values of other cells and the time of day.  Those alternate cells can even exist in different workbook files
	/// on the user's machine or on the internet.
	/// </p>
	/// <p class="body">
	/// See Microsoft Excel help for more information on formulas.
	/// </p>
	/// <p class="body">
	/// Use one of the Parse or TryParse overloads to create a new formula.
	/// </p>
	/// </remarks>
	[DebuggerDisplay( "Formula: {ToString(),nq}" )]



	public

		 class Formula : IWorksheetCellOwnedValue
	{
		#region Member Variables

		private CellReferenceMode creationMode;
		private bool isArrayOrSharedFormula;
		private bool isDataTableInterior;
		private bool isSharedFormula;

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//private IWorksheetCell owningCell;
		private WorksheetRow owningCellRow;
		private short owningCellColumnIndex;

		// MD 6/31/08 - Excel 2007 Format
		private WorkbookFormat parsedWorkbookFormat;

		private List<FormulaToken> postfixTokenList;
		private bool recalculateAlways;

		// MD 8/20/07 - BR25818
		// The formula type is now stored with the formula
		private FormulaType type;

		// MD 10/13/10 - TFS49055
		// The formula string may be different depending on the culture, so we need a dictionary of caches.
		//private string cachedA1Value;
		//private string cachedR1C1Value;
		private Dictionary<CultureInfo, string> cachedA1Value;
		private Dictionary<CultureInfo, string> cachedR1C1Value;

		// MD 7/26/10 - TFS34398
		// The formulas will now cache the calculated values instead of the cells.
		private object calculatedValue;

		// MD 2/28/12 - 12.1 - Table Support
		private List<ExcelCalcFormula> compiledFormulas;

		#endregion Member Variables

		#region Constructor

		// MD 8/20/07 - BR25818
		// Added a new parameter which indicates the formula type
		//internal Formula( CellReferenceMode creationMode )
		internal Formula( CellReferenceMode creationMode, FormulaType type )
		{
			this.creationMode = creationMode;
			this.postfixTokenList = new List<FormulaToken>();

			// MD 8/20/07 - BR25818
			// The formula type is now stored with the formula
			this.type = type;
		}

		internal Formula( ArrayFormula arrayFormula )
			// MD 8/20/07 - BR25818
			// The formula constructor takes another parameter now which indicates the formula type
			//: this( arrayFormula.creationMode )
			: this( arrayFormula.creationMode, FormulaType.Formula )
		{
			this.isArrayOrSharedFormula = true;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.postfixTokenList.Add( new ExpToken( this, arrayFormula.CellRange.FirstColumn, arrayFormula.CellRange.FirstRow ) );
			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//this.postfixTokenList.Add(new ExpToken(arrayFormula.CellRange.FirstColumn, arrayFormula.CellRange.FirstRow));
			this.postfixTokenList.Add(new ExpToken(arrayFormula.CellRange.FirstColumnInternal, arrayFormula.CellRange.FirstRow));
		}

		internal Formula( WorksheetDataTable dataTable )
			// MD 8/20/07 - BR25818
			// The formula constructor takes another parameter now which indicates the formula type
			//: this( dataTable.Worksheet.Workbook.CellReferenceMode )
			: this( dataTable.Worksheet.Workbook.CellReferenceMode, FormulaType.Formula )
		{
			this.isDataTableInterior = true;

			// MD 10/22/10 - TFS36696
			// We don't need to store the formula on the token anymore.
			//this.postfixTokenList.Add( new TblToken( this, dataTable.CellsInTable.FirstColumn + 1, dataTable.CellsInTable.FirstRow + 1 ) );
			// MD 4/12/11 - TFS67084
			// Use short instead of int so we don't have to cast.
			//this.postfixTokenList.Add(new TblToken(dataTable.CellsInTable.FirstColumn + 1, dataTable.CellsInTable.FirstRow + 1));
			// MD 3/12/12 - 12.1 - Table Support
			//this.postfixTokenList.Add(new TblToken((short)(dataTable.CellsInTable.FirstColumnInternal + 1), dataTable.CellsInTable.FirstRow + 1));
			WorksheetRegion cellsInTable = dataTable.CellsInTable;
			this.postfixTokenList.Add(new TblToken((short)(cellsInTable.FirstColumnInternal + 1), cellsInTable.FirstRow + 1));
		}

		internal Formula( Formula formula )
			// MD 8/20/07 - BR25818
			// The formula constructor takes another parameter now which indicates the formula type
			//: this( formula.creationMode )
			: this( formula.creationMode, formula.type )
		{
			// MD 8/21/08 - Excel formula solving
			this.parsedWorkbookFormat = formula.parsedWorkbookFormat;

			this.recalculateAlways = formula.recalculateAlways;

			foreach ( FormulaToken token in formula.postfixTokenList )
			{
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//FormulaToken newToken = token.Clone( this );
				FormulaToken newToken = token.GetTokenForClonedFormula();

				Debug.Assert( newToken.GetType() == token.GetType() );

				this.postfixTokenList.Add( newToken );
			}
		}

		#endregion Constructor

		#region Base Class Overrides

		#region ToString

		/// <summary>
		/// Converts the formula to a string representation, similar to the string with which it was created. 
		/// This uses the <see cref="CellReferenceMode"/> with which the formula was created to create cell
		/// reference strings.
		/// </summary>
		/// <returns>The string representing the formula.</returns>
		public override string ToString()
		{
			return this.ToString( this.creationMode );
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Interfaces

		#region IWorksheetCellValue Members

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//void IWorksheetCellOwnedValue.VerifyNewOwner( IWorksheetCell owner )
		//{
		//    this.VerifyNewOwner( owner );
		//}
		void IWorksheetCellOwnedValue.VerifyNewOwner(WorksheetRow ownerRow, short ownerColumnIndex)
		{
			this.VerifyNewOwner(ownerRow, ownerColumnIndex);
		}

		// MD 8/12/08 - Excel formula solving
		bool IWorksheetCellOwnedValue.IsOwnedByAllCellsAppliedTo
		{
			get { return this.IsOwnedByAllCellsAppliedTo; }
		}

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//IWorksheetCell IWorksheetCellOwnedValue.OwningCell
		//{
		//    get { return this.OwningCell; }
		//    set { this.OwningCell = value; }
		//}
		void IWorksheetCellOwnedValue.SetOwningCell(WorksheetRow row, short columnIndex)
		{
			this.SetOwningCell(row, columnIndex);
		}

		internal void SetOwningCell(WorksheetRow row, short columnIndex)
		{
			if (this.owningCellRow == row && this.owningCellColumnIndex == columnIndex)
				return;

			// MD 6/16/12 - CalcEngineRefactor
			if (this.owningCellRow != null)
				this.DisconnectReferences();

			this.owningCellRow = row;
			this.owningCellColumnIndex = columnIndex;

			this.ClearCache();

			// Clear the cached value when the cell changes.
			this.calculatedValue = null;

			// MD 6/16/12 - CalcEngineRefactor
			if (this.owningCellRow != null)
			{
				this.ConnectReferences(
					new FormulaContext(
						this.owningCellRow.Worksheet,
						this.owningCellRow.Index,
						this.owningCellColumnIndex,
						this.owningCellRow.Worksheet.CurrentFormat,
						this)
					);
			}
		}

		#endregion

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region ApplyTo ( WorksheetCell )

		/// <summary>
		/// Applies the formula to the specified cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, or one of the other ApplyTo overrides must be used to set the value of a cell to a formula.
		/// </p>
		/// <p class="body">
		/// After this method returns, the <see cref="WorksheetCell.Formula"/> of the specified cell will return the formula.
		/// </p>
		/// </remarks>
		/// <param name="cell">The cell to apply the formula to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="cell"/> is part of an array formula or data table which is not confined to just the cell.
		/// </exception>
		/// <seealso cref="WorksheetCell.Value"/>
		/// <seealso cref="ApplyTo(WorksheetRegion)"/>
		/// <seealso cref="ApplyTo(WorksheetRegion[])"/>
		/// <seealso cref="WorksheetCell.ApplyFormula(string)"/>
		/// <seealso cref="WorksheetRegion.ApplyFormula(string)"/>
		/// <seealso cref="WorksheetRegion.ApplyArrayFormula(string)"/>
		public void ApplyTo( WorksheetCell cell )
		{
			if ( cell == null )
				throw new ArgumentNullException( "cell" );

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			// MD 4/12/11 - TFS67084
			// Moved all code to the new overload.
			this.ApplyTo(cell.Row, cell.ColumnIndexInternal);
		}

		// MD 4/12/11 - TFS67084
		// Added a new overload so we don't have to use WorksheetCell objects.
		// MD 2/24/12
		// Found while implementing 12.1 - Table Support
		// When an array formula is applied to a single cell, it shouldn't use this code. Marked this virtual so we can do 
		// something different on the ArrayFormula class.
		//internal void ApplyTo(WorksheetRow row, short columnIndex)
		internal virtual void ApplyTo(WorksheetRow row, short columnIndex)
		{
			// MD 2/24/12 - 12.1 - Table Support
			row.Worksheet.VerifyFormula(this, row, columnIndex);

			// MD 8/21/08 - Excel formula solving - Performance
			// The region doesn't have to be added to the cache on the worksheet because the region will be discarded when
			// this method returns.
			//this.ApplyTo( new WorksheetRegion( cell.Worksheet, cell.RowIndex, cell.ColumnIndex, cell.RowIndex, cell.ColumnIndex ) );
			// MD 7/26/10 - TFS34398
			// Now that the cell stores the row instead of the worksheet, we should cache the row and get the index and 
			// worksheet from that.
			//this.ApplyTo( new WorksheetRegion( cell.Worksheet, cell.RowIndex, cell.ColumnIndex, cell.RowIndex, cell.ColumnIndex, false ) );
			// MD 11/1/10 - TFS56976
			// Although it was easier to manage, it is slower to create a region to hold the cell and then to later set the formula on the individual 
			// cells of the region. Instead, we will duplicate a little bit of logic here for performance.
			//WorksheetRow row = cell.Row;
			//this.ApplyTo(new WorksheetRegion(row.Worksheet, row.Index, cell.ColumnIndex, row.Index, cell.ColumnIndex, false));
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetRegionBlockingValue blockingValue = ((IWorksheetCell)cell).RegionBlockingValue;
			IWorksheetRegionBlockingValue blockingValue = row.GetCellValueRaw(columnIndex) as IWorksheetRegionBlockingValue;

			if (blockingValue != null)
			{
				WorksheetRegion region = blockingValue.Region;

				if (region.FirstRow != region.LastRow || region.FirstColumn != region.LastColumn)
					blockingValue.ThrowBlockingException();
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//this.ApplyTo(cell, cell);
			this.ApplyTo(row, columnIndex, row, columnIndex);
		}

		#endregion ApplyTo ( WorksheetCell )

		#region ApplyTo ( WorksheetRegion )

		/// <summary>
		/// Applies the formula to the specified region of cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, or one of the other ApplyTo overrides must be used to set the value of a cell to a formula.
		/// </p>
		/// <p class="body">
		/// After this method returns, the <see cref="WorksheetCell.Formula"/> of all cells in the specified region will
		/// return the formula.
		/// </p>
		/// </remarks>
		/// <param name="region">The region of cells to apply the formula to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="region"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="region"/> contains an array formula or data table which extends outside the region.
		/// </exception>
		/// <seealso cref="ApplyTo(WorksheetCell)"/>
		/// <seealso cref="ApplyTo(WorksheetRegion[])"/>
		/// <seealso cref="WorksheetCell.ApplyFormula(string)"/>
		/// <seealso cref="WorksheetRegion.ApplyFormula(string)"/>
		/// <seealso cref="WorksheetRegion.ApplyArrayFormula(string)"/>
		public void ApplyTo( WorksheetRegion region )
		{
			if ( region == null )
				throw new ArgumentNullException( "region" );

			this.ApplyTo( new WorksheetRegion[] { region } );
		}

		#endregion ApplyTo ( WorksheetRegion )

		#region ApplyTo ( WorksheetRegion[] )

		/// <summary>
		/// Applies the formula to all specified regions of cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, or one of the other ApplyTo overrides must be used to set the value of a cell to a formula.
		/// </p>
		/// <p class="body">
		/// After this method returns, the <see cref="WorksheetCell.Formula"/> of all cells in all specified regions will
		/// return the formula.
		/// </p>
		/// </remarks>
		/// <param name="regions">The regions of cells to apply the formula to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="regions"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="regions"/> has a length of 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Not all regions specified are from the same worksheet.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// One or more regions specified contain array formulas or data tables which extend outside the region.
		/// </exception>
		/// <seealso cref="ApplyTo(WorksheetCell)"/>
		/// <seealso cref="ApplyTo(WorksheetRegion)"/>
		/// <seealso cref="WorksheetCell.ApplyFormula(string)"/>
		/// <seealso cref="WorksheetRegion.ApplyFormula(string)"/>
		public virtual void ApplyTo( WorksheetRegion[] regions )
		{
			if ( regions == null )
				throw new ArgumentNullException( "regions" );

			if ( regions.Length == 0 )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_NoRegionsInArray" ), "regions" );

			Worksheet worksheet = regions[ 0 ].Worksheet;

			for ( int i = 1; i < regions.Length; i++ )
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (regions[i].Worksheet == null)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionsShiftedOffWorksheet"), "regions");

				if ( regions[ i ].Worksheet != worksheet )
					throw new ArgumentException( SR.GetString( "LE_ArgumentException_RegionsFromMixedWorksheets" ), "regions" );
			}

			for ( int i = 0; i < regions.Length; i++ )
			{
				WorksheetRegion region = regions[ i ];

				for ( int rowIndex = region.FirstRow; rowIndex <= region.LastRow; rowIndex++ )
				{
					WorksheetRow row = worksheet.Rows[ rowIndex ];

					// MD 4/12/11 - TFS67084
					// Use short instead of int so we don't have to cast.
					//for ( int columnIndex = region.FirstColumn; columnIndex <= region.LastColumn; columnIndex++ )
					for (short columnIndex = region.FirstColumnInternal; columnIndex <= region.LastColumnInternal; columnIndex++)
					{
						// MD 7/14/08 - Excel formula solving
						// The Value property now returns the calculated value, not the ArrayFormula or data table on the cell.
						//IWorksheetRegionBlockingValue blockingValue = row.Cells[ columnIndex ].Value as IWorksheetRegionBlockingValue;
						// MD 4/12/11 - TFS67084
						// Moved away from using WorksheetCell objects.
						//IWorksheetRegionBlockingValue blockingValue = ( (IWorksheetCell)row.Cells[ columnIndex ] ).RegionBlockingValue;
						IWorksheetRegionBlockingValue blockingValue = row.GetCellValueRaw(columnIndex) as IWorksheetRegionBlockingValue;

						if ( blockingValue != null && region.Contains( blockingValue.Region ) == false )
							blockingValue.ThrowBlockingException();
					}
				}
			}

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//// MD 5/21/07 - BR23050
			//// If this formula has already been applied to a cell, it should have no bearing on this call to Apply
			////IWorksheetCell firstAppliedToCell = this.owningCell;
			//IWorksheetCell firstAppliedToCell = null;
			//
			//for ( int i = 0; i < regions.Length; i++ )
			//    regions[ i ].ApplyFormulaToRegion( this, ref firstAppliedToCell );
			WorksheetRow firstAppliedToRow = null;
			short firstAppliedToColumnIndex = -1;
			for (int i = 0; i < regions.Length; i++)
				regions[i].ApplyFormulaToRegion(this, ref firstAppliedToRow, ref firstAppliedToColumnIndex);
		}

		#endregion ApplyTo ( WorksheetRegion[] )

		#region ToString( CellReferenceMode )

		/// <summary>
		/// Converts the formula to a string representation, similar to the string with which it was created.
		/// </summary>
		/// <param name="cellReferenceMode">The cell reference mode used to create cell reference strings.</param>
		/// <returns>The string representing the formula.</returns>
		public string ToString( CellReferenceMode cellReferenceMode )
		{
			// MD 4/6/12 - TFS101506
			//return this.ToString( cellReferenceMode, CultureInfo.CurrentCulture );
			return this.ToString(cellReferenceMode, this.Culture);
		}

		#endregion ToString( CellReferenceMode )

		#region ToString( CellReferenceMode, CultureInfo )

		/// <summary>
		/// Converts the formula to a string representation, similar to the string with which it was created.
		/// </summary>
		/// <param name="cellReferenceMode">The cell reference mode used to create cell reference strings.</param>
		/// <param name="culture">The culture used to generate the formula string.</param>
		/// <returns>The string representing the formula.</returns>
		public string ToString( CellReferenceMode cellReferenceMode, CultureInfo culture )
		{
			if ( cellReferenceMode == CellReferenceMode.A1 )
				return this.ToStringHelper( ref this.cachedA1Value, cellReferenceMode, culture );

			return this.ToStringHelper( ref this.cachedR1C1Value, cellReferenceMode, culture );
		}

		#endregion ToString( CellReferenceMode, CultureInfo )

		#endregion Public Methods

		#region Internal Methods

		#region ApplyTo( IWorksheetCell, WorksheetCell )



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal void ApplyTo( IWorksheetCell firstCellAppliedTo, IWorksheetCell cell )
		// MD 12/22/11 - 12.1 - Table Support
		// Added a return type so we can get the actual Formula instance applied to the cell.
		//internal void ApplyTo(WorksheetRow firstCellAppliedToRow, short firstCellAppliedToColumnIndex, WorksheetRow row, short columnIndex)
		internal Formula ApplyTo(WorksheetRow firstCellAppliedToRow, short firstCellAppliedToColumnIndex, WorksheetRow row, short columnIndex)
		{
			// MD 9/26/08
			// This can never be null and is not called publicly, so this check isn't needed.
			//if ( cell == null )
			//	throw new ArgumentNullException( "cell" );

			// Remove the blocking value from the cell, we have already checked that all blocking values are covered 
			// by the regions applied to.
			// MD 7/14/08 - Excel formula solving
			// The Value property now returns the calculated value, not the ArrayFormula or data table on the cell.
			//IWorksheetRegionBlockingValue blockingValue = cell.Value as IWorksheetRegionBlockingValue;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//IWorksheetRegionBlockingValue blockingValue = cell.RegionBlockingValue;
			WorksheetCellBlock cellBlock = row.GetCellBlock(columnIndex);
			IWorksheetRegionBlockingValue blockingValue = cellBlock.GetCellValueRaw(row, columnIndex) as IWorksheetRegionBlockingValue;

			if ( blockingValue != null )
				blockingValue.RemoveFromRegion();

			// Clone the formula so translating relative references will not affect the formulas of other cells.
			// MD 3/6/12 - 12.1 - Table Support
			// Try to only clone the formula when we need to: if the formula is already owned or if the tokens
			// will be offset below.
			//Formula formula = this.Clone();
			Formula formula = this;
			if (this.OwningCellRow != null ||
					(
						firstCellAppliedToRow != null &&
						(columnIndex != firstCellAppliedToColumnIndex || row.Index != firstCellAppliedToRow.Index)
					)
				)
			{
				formula = formula.Clone();
			}

			// MD 7/12/12 - TFS109194
			// Moved from below.
			Workbook workbook = row.Worksheet.Workbook;

			// Offset formula's relative references if it has already been applied to a cell
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//if ( this.isSharedFormula == false && firstCellAppliedTo != null )
			if (this.isSharedFormula == false && firstCellAppliedToRow != null)
			{
				// Make sure they came from the same worksheet
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//Debug.Assert( firstCellAppliedTo.Worksheet == cell.Worksheet );
				Debug.Assert(firstCellAppliedToRow.Worksheet == row.Worksheet);

				// Determine the translation that should take place for all relative references.
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//Point offset = new Point(
				//    cell.ColumnIndex - firstCellAppliedTo.ColumnIndex,
				//    cell.RowIndex - firstCellAppliedTo.RowIndex );
				Point offset = new Point(
					columnIndex - firstCellAppliedToColumnIndex,
					row.Index - firstCellAppliedToRow.Index);

				// MD 3/10/12 - 12.1 - Table Support
				// Moved this code to a helper method.
				//// Offset all relative references by the translation offset
				//foreach ( FormulaToken token in formula.postfixTokenList )
				//    token.OffsetReferences( offset );
				//
				//formula.ClearCache();
				// MD 7/12/12 - TFS109194
				// OffsetReferences now needs a reference to the workbook.
				//formula.OffsetReferences(offset);
				formula.OffsetReferences(workbook, offset);
			}

			// MD 7/9/08 - Excel 2007 Format
			// Verify the limits with the current file format.
			FormatLimitErrors limitErrors = new FormatLimitErrors();

            // MBS 7/10/08 - Excel 2007 Format
            // We need to know the current cell reference mode so that we can re-validate the formula correctly.  Also
            // Pass in true to the last parameter to tell the method that it should re-parse the function to make sure it's valid
			// MD 7/26/10 - TFS34398
			// Now that the Row is stored on the cell, the Worksheet getter is now a bit slower, so cache it.
            //CellReferenceMode cellReferenceMode = cell.Worksheet.Workbook != null ? cell.Worksheet.Workbook.CellReferenceMode : CellReferenceMode.A1;
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Worksheet worksheet = cell.Worksheet;
			// MD 7/12/12 - TFS109194
			//Workbook workbook = row.Worksheet.Workbook;		

			// MD 8/4/08 - Excel formula solving
			// We don't always need to re-parse the formula when verifying the limits. We only need to do it when the formulawith which
			//formula.VerifyFormatLimits( limitErrors, cell.Worksheet.CurrentFormat, cellReferenceMode, true);
			// MD 11/1/10 - TFS56976
			// If the formats match, we don't have to do any verification at all. Put this call in an if block and changed the last parameter back to True.
			//formula.VerifyFormatLimits( 
			//    limitErrors,
			//    // MD 7/26/10 - TFS34398
			//    // Use the cached worksheet.
			//    //cell.Worksheet.CurrentFormat, 
			//    worksheet.CurrentFormat, 
			//    cellReferenceMode,
			//    // MD 7/26/10 - TFS34398
			//    // Use the cached worksheet.
			//    //this.CurrentFormat != cell.Worksheet.CurrentFormat );
			//    this.CurrentFormat != worksheet.CurrentFormat);
			// MD 4/12/11
			// Found while fixing TFS67084
			// Get the format from the workbook directly.
			//if (this.CurrentFormat != worksheet.CurrentFormat)
			//    formula.VerifyFormatLimits(limitErrors, worksheet.CurrentFormat, cellReferenceMode, true);
			if (this.CurrentFormat != workbook.CurrentFormat)
			{
				CellReferenceMode cellReferenceMode = workbook != null ? workbook.CellReferenceMode : CellReferenceMode.A1;

				// MD 3/2/12 - 12.1 - Table Support
				//formula.VerifyFormatLimits(limitErrors, workbook.CurrentFormat, cellReferenceMode, true);
				formula.VerifyFormatLimits(workbook, limitErrors, workbook.CurrentFormat, cellReferenceMode, true);
			}

			if ( limitErrors.HasErrors )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_FormulaReferencesInvalidCells" ) );

			// MD 5/21/07 - BR23050
			// Don't do this here, just in case the formula is applied to a new cell, we want to retain the 
			// relative references.  We will do this when we save all formulas, including array formulas,
			// which wasn't being done before.
			//// Iterate through all tokens in the formula, and replace all shared formula tokens with tokens
			//// that are specific to the cell the formula is being applied to.
			//for ( int i = 0; i < formula.postfixTokenList.Count; i++ )
			//{
			//    formula.postfixTokenList[ i ] =
			//        formula.postfixTokenList[ i ].GetNonSharedEquivalent( cell );
			//}

			// Apply the formula to the cell
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//cell.InternalSetValue( formula );
			// MD 1/31/12 - TFS100573
			// The cell block may get replaced by this operation. If it does, replace our reference to it (even though we don't 
			// currently do anything with the cell block after this, it will prevent bugs from being introduced if code is added
			// later which does use it).
			//cellBlock.SeCellValueInternal(row, columnIndex, formula);
			// MD 3/2/12 - 12.1 - Table Support
			// Moved this code to a virtual method.
			//WorksheetCellBlock replacementBlock;
			//cellBlock.SetCellValueRaw(row, columnIndex, formula, out replacementBlock);
			//cellBlock = replacementBlock ?? cellBlock;
			formula.ApplyToCell(row, columnIndex, cellBlock);

			// MD 11/29/10 - TFS57334
			// If the HYPERLINK function is used in the formula, change the cell's appearance to look like a hyperlink.
			for (int i = 0; i < formula.PostfixTokenList.Count; i++)
			{
				FunctionOperator function = formula.PostfixTokenList[i] as FunctionOperator;

				if (function == null)
					continue;

				if (function.Function == Function.HYPERLINK)
				{
					// It seems the Blue color is always used for hyperlinks in Excel, even when the SystemColor.Hyperlink color is not blue.

					// MD 4/12/11 - TFS67084
					// Moved away from using WorksheetCell objects and added an alias at the top of the file to the Colors
					// class so we don't need an #if here.
					//                    cell.CellFormat.Font.Color =
					//#if SILVERLIGHT
					//                        Colors.Blue;
					//#else
					//                        Color.Blue;
					//#endif
					//
					//                    cell.CellFormat.Font.UnderlineStyle = FontUnderlineStyle.Single;
					IWorksheetCellFormat cellFormat = row.GetCellFormatInternal(columnIndex);

					// MD 1/17/12 - 12.1 - Cell Format Updates
					//cellFormat.Font.Color = Color.Blue;
					cellFormat.Font.ColorInfo = new WorkbookColorInfo(Color.Blue);

					cellFormat.Font.UnderlineStyle = FontUnderlineStyle.Single;
					break;
				}
			}

			// MD 12/22/11 - 12.1 - Table Support
			return formula;
		}

		#endregion ApplyTo( IWorksheetCell, WorksheetCell )

		// MD 3/2/12 - 12.1 - Table Support
		#region ApplyToCell

		internal virtual void ApplyToCell(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock) 
		{
			WorksheetCellBlock replacementBlock;
			cellBlock.SetCellValueRaw(row, columnIndex, this, out replacementBlock);
			cellBlock = replacementBlock ?? cellBlock;
		}

		#endregion // ApplyToCell

		#region Clone

		internal virtual Formula Clone()
		{
			return new Formula( this );
		}

		#endregion Clone

		// MD 6/16/12 - CalcEngineRefactor
		#region ConnectReferences

		internal void ConnectReferences(FormulaContext context)
		{
			for (int i = 0; i < this.postfixTokenList.Count; i++)
				this.postfixTokenList[i].ConnectReferences(context);
		}

		#endregion // ConnectReferences

		// MD 3/2/12 - 12.1 - Table Support
		#region ConvertTableReferencesToRanges

		internal void ConvertTableReferencesToRanges(Workbook workbook, WorksheetTable table)
		{
			FormulaContext sourceContext = new FormulaContext(workbook, this);

			bool referencesUpdated = false;
			for (int i = 0; i < this.PostfixTokenList.Count; i++)
			{
				FormulaToken token = this.postfixTokenList[i];

				FormulaToken replacementToken;
				token.ConvertTableReferencesToRanges(sourceContext, table, out replacementToken);

				if (replacementToken != null)
				{
					referencesUpdated = true;
					this.postfixTokenList[i] = replacementToken;
					this.ClearCache();
				}
			}

			if (referencesUpdated)
				this.RecompileFormulas();
		}

		#endregion // ConvertTableReferencesToRanges

		// MD 6/16/12 - CalcEngineRefactor
		#region DisconnectReferences

		internal void DisconnectReferences()
		{
			for (int i = 0; i < this.postfixTokenList.Count; i++)
				this.postfixTokenList[i].DisconnectReferences();
		}

		#endregion // DisconnectReferences

		// MD 7/12/12 - TFS109194
		#region DoesHaveRelativeReferences

		internal bool DoesHaveRelativeReferences()
		{
			for (int i = 0; i < this.postfixTokenList.Count; i++)
			{
				CellReferenceToken cellReferenceToken = this.postfixTokenList[i] as CellReferenceToken;
				if (cellReferenceToken != null && cellReferenceToken.HasRelativeAddresses)
					return true;
			}

			return false;
		}

		#endregion // DoesHaveRelativeReferences

		// MD 7/26/10 - TFS34398
		#region GetCalculatedValue

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal virtual object GetCalculatedValue(WorksheetCell cell)
		internal virtual object GetCalculatedValue(WorksheetRow row, short columnIndex)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Debug.Assert(cell == this.owningCell, "Only the owning cell should have been passed in here.");
			Debug.Assert(
				this.owningCellRow != null && row == this.owningCellRow && columnIndex == this.owningCellColumnIndex, 
				"Only the owning cell should have been passed in here.");

			return this.calculatedValue;
		} 

		#endregion // GetCalculatedValue

		// MD 6/16/12 - CalcEngineRefactor
		#region InitializeSerializationManager

		internal void InitializeSerializationManager(WorkbookSerializationManager manager, Worksheet owningWorksheet)
		{
			FormulaContext context = new FormulaContext(manager.Workbook, owningWorksheet, this.owningCellRow, this.owningCellColumnIndex, this);
			foreach (FormulaToken token in this.postfixTokenList)
				token.InitializeSerializationManager(manager, context);
		}

		#endregion InitializeSerializationManager

		// MD 12/22/11 - 12.1 - Table Support
		#region IsEquivalentTo







		internal bool IsEquivalentTo(Formula formula)
		{
			if (this.OwningCellRow == null || formula.OwningCellRow == null)
			{
				Utilities.DebugFail("We can only check the formulas for equivalency if they are both applied to cells.");
				return false;
			}

			if (this.Type != formula.Type)
				return false;

			if (this.PostfixTokenList.Count != formula.PostfixTokenList.Count)
				return false;

			// MD 6/13/12 - CalcEngineRefactor
			FormulaContext sourceContext = new FormulaContext(this.Workbook, this);
			FormulaContext comparisonContext = new FormulaContext(formula.Workbook, formula);

			for (int i = 0; i < this.PostfixTokenList.Count; i++)
			{
				FormulaToken sourceToken = this.PostfixTokenList[i];
				FormulaToken comparisonToken = formula.PostfixTokenList[i];

				// MD 6/13/12 - CalcEngineRefactor
				//if (sourceToken.IsEquivalentTo(comparisonToken,
				//    this.OwningCellRow, this.OwningCellColumnIndex,
				//    formula.OwningCellRow, formula.OwningCellColumnIndex) == false)
				if (sourceToken.IsEquivalentTo(sourceContext, comparisonToken, comparisonContext) == false)
				{
					return false;
				}
			}

			return true;
		}

		#endregion // IsEquivalentTo

		// MD 3/10/12 - 12.1 - Table Support
		#region OffsetReferences

		// MD 7/12/12 - TFS109194
		// OffsetReferences now needs a reference to the Workbook so it knows the row/column counts for wrapping purposes.
		//internal void OffsetReferences(Point offset)
		internal void OffsetReferences(Workbook workbook, Point offset)
		{
			if (this.isSharedFormula == false)
			{
				// MD 7/12/12 - TFS109194
				FormulaContext context = new FormulaContext(workbook, this);

				// Offset all relative references by the translation offset
				foreach (FormulaToken token in this.postfixTokenList)
				{
					// MD 7/12/12 - TFS109194
					// OffsetReferences now needs a reference to the Workbook so it knows the row/column counts for wrapping purposes.
					//token.OffsetReferences(offset);
					token.OffsetReferences(context, offset);
				}

				this.ClearCache();
			}
		}

		#endregion // OffsetReferences

		// MD 2/28/12 - 12.1 - Table Support
		#region OnCellsShifted

		internal void OnCellsShifted(Worksheet owningWorksheet,
			CellShiftOperation shiftOperation,
			ReferenceShiftType shiftType)
		{
			Workbook workbook = null;
			if (owningWorksheet != null)
				workbook = owningWorksheet.Workbook;

			FormulaContext context = new FormulaContext(workbook, owningWorksheet,
				this.OwningCellRow,
				this.OwningCellColumnIndex,
				this);

			bool referencesUpdated = false;
			for (int i = 0; i < this.PostfixTokenList.Count; i++)
			{
				FormulaToken token = this.postfixTokenList[i];

				FormulaToken replacementToken;
				referencesUpdated |= token.UpdateReferencesOnCellsShiftedVertically(context,
					shiftOperation,
					shiftType,
					out replacementToken);

				if (replacementToken != null)
					this.postfixTokenList[i] = replacementToken;
			}

			if (referencesUpdated)
			{
				this.ClearCache();
				this.RecompileFormulas();
			}
		}

		#endregion // OnCellsShifted

		#region OnNamedReferenceRemoved

		internal void OnNamedReferenceRemoved(NamedReferenceBase namedReference)
		{
			bool tokensChanged = false;
			for (int i = 0; i < this.postfixTokenList.Count; i++)
				tokensChanged |= this.postfixTokenList[i].OnNamedReferenceRemoved(namedReference);

			if (tokensChanged)
			{
				this.RecompileFormulas();
				this.ClearCache();
			}
		}

		#endregion // OnNamedReferenceRemoved

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResized






		internal void OnTableResized()
		{
			// When the table resize began, we removed the compiled formulas from the calc network, but left them temporarily in the
			// compiledFormulas collection. Now that the table is resized, we can re-add them to the calc network. Copy the collection
			// and clear the compiled formulas before re-compiling because the old compiled formulas shouldn't be in the collection.
			if (this.compiledFormulas != null)
			{
				List<ExcelCalcFormula> oldCompiledFormulas = new List<ExcelCalcFormula>(this.compiledFormulas);
				this.compiledFormulas = null;
				this.RecompileFormulasHelper(oldCompiledFormulas);
			}

			this.ClearCache();
		}

		#endregion // OnTableResized

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnTableResizing






		internal bool OnTableResizing(WorksheetTable table, List<WorksheetTableColumn> columnsBeingRemoved)
		{
			// Notify each token that the table will be resized and which columns are being removed due to the operation.
			// If the token needs to be replaced, modify the token list.
			bool formulaReferencesTable = false;
			for (int i = 0; i < this.postfixTokenList.Count; i++)
			{
				FormulaToken token = this.postfixTokenList[i];

				bool tableReferenced;
				FormulaToken replacementToken;
				token.OnTableResizing(table, columnsBeingRemoved, out tableReferenced, out replacementToken);

				formulaReferencesTable |= tableReferenced;

				if (replacementToken != null)
					this.postfixTokenList[i] = replacementToken;
			}

			if (formulaReferencesTable)
			{
				this.ClearCache();

				// If the formula references the table, temporarily remove the compiled formulas from the calc engine, but
				// leave the compiled formulas in the compiledFormulas collection so that we can re-add them in the 
				// OnTableResized method.
				if (this.compiledFormulas != null)
				{
					for (int i = 0; i < this.compiledFormulas.Count; i++)
					{
						ExcelRefBase reference = this.compiledFormulas[i].BaseReference as ExcelRefBase;
						if (reference != null)
							reference.SetAndCompileFormula(null, false, true, false);
					}
				}
			}

			return formulaReferencesTable;
		}

		#endregion // OnTableResizing

		// MD 3/1/12 - 12.1 - Table Support
		#region OnTableColumnsRenamed

		internal void OnTableColumnsRenamed(WorksheetTable table, List<KeyValuePair<WorksheetTableColumn, string>> changedColumnNames)
		{
			for (int i = 0; i < this.postfixTokenList.Count; i++)
			{
				StructuredTableReference token = this.postfixTokenList[i] as StructuredTableReference;
				if (token != null)
					token.OnTableColumnsRenamed(table, changedColumnNames);			
			}

			this.ClearCache();
		}

		#endregion // OnTableColumnsRenamed

		// MD 6/18/12 - TFS102878
		#region OnWorksheetMoved

		internal void OnWorksheetMoved(Worksheet worksheet, int oldIndex)
		{
			bool formulaUpdated = false;
			foreach (FormulaToken token in this.postfixTokenList)
				formulaUpdated |= token.OnWorksheetMoved(worksheet, oldIndex);

			if (formulaUpdated)
			{
				this.ClearCache();
				this.RecompileFormulas();
			}
		}

		#endregion // OnWorksheetMoved

		// MD 8/21/08 - Excel formula solving
		#region OnWorksheetRemoved







		internal void OnWorksheetRemoved(Worksheet worksheet, int oldIndex)
		{
			bool tokensChanged = false;
			for (int i = 0; i < this.postfixTokenList.Count; i++)
			{
				FormulaToken token = this.postfixTokenList[i];
				FormulaToken replacementToken;
				tokensChanged |= token.OnWorksheetRemoved(worksheet, oldIndex, out replacementToken);

				if (replacementToken != null)
					this.postfixTokenList[i] = replacementToken;
			}

			if (tokensChanged)
			{
				this.RecompileFormulas();
				this.ClearCache();
			}
		}

		#endregion OnWorksheetRemoved

		// MD 8/20/07 - BR25818
		// We may need to resolve named references names after we finish parsing the formula
		#region ResolveNamedReferencesAfterLoad



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal void ResolveNamedReferencesAfterLoad( WorkbookSerializationManager manager )
		{
			foreach ( FormulaToken token in this.postfixTokenList )
			{
				NameToken nameToken = token as NameToken;

				if ( nameToken == null )
					continue;

				nameToken.ResolveNamedReferenceAfterLoad( manager );
			}
		}

		#endregion ResolveNamedReferencesAfterLoad

		#region Save

		internal int Save(BiffRecordStream stream, bool writeSize, bool isForExternalNamedReference)
		{
			// MD 9/19/11 - TFS86108
			// Moved all code to the new overload.
			return this.Save(stream, writeSize, isForExternalNamedReference, true);
		}

		// MD 9/19/11 - TFS86108
		// Added a new overload with a parameter called resolveSharedReferences
		internal int Save(BiffRecordStream stream, bool writeSize, bool isForExternalNamedReference, bool resolveSharedReferences)
		{
			int size = 0;

			// If the size will be written, write a size placeholder...we will come back later and write the real size
			if ( writeSize )
				stream.Write( (ushort)0 );

			if ( this.postfixTokenList.Count == 0 )
				return size;

			// Start the start position of the rpn token array
			long startPos = stream.Position;

			// MD 7/6/09 - TFS18865
			// This needs to be done after getting the non shared equivalents of each token. That way all references on token point to other 
			// tokens in the resolvedTokenList.
			//if ( this.IsSpecialFormula == false )
			//{
			//    // Resolve all token references in the postfix list
			//    TokenReferenceResolver resolver = new TokenReferenceResolver( this );
			//    EvaluationResult<FormulaToken> resolveResult = resolver.Evaluate();
			//    Debug.Assert( resolveResult.Completed );
			//}

			// MD 5/21/07 - BR23050
			// Copy the token array so we can resolve the relative references tokens without changing the formulas
			FormulaToken[] resolvedTokenList = this.postfixTokenList.ToArray();

			// MD 5/13/11 - Data Validations / Page Breaks
			// Cache this value so we don't have to get it multiple times.
			int owningCellRowIndex = this.owningCellRow == null ? -1 : this.owningCellRow.Index;

			// MD 9/19/11 - TFS86108
			// Wrapped in an if statement. Only do this if the resolveSharedReferences is True.
			if (resolveSharedReferences)
			{
				FormulaContext context = new FormulaContext(stream.Manager.Workbook, this);

				// Iterate through all tokens in the formula, and replace all shared formula tokens with tokens
				// that are specific to the cell the formula is applied to.
				for (int i = 0; i < this.postfixTokenList.Count; i++)
					resolvedTokenList[i] = resolvedTokenList[i].GetNonSharedEquivalent(context);
			}

			// MD 10/22/10 - TFS36696
			// To save space, we will no longer save the portions in the record stream on the tokens.
			Dictionary<FormulaToken, TokenPositionInfo> tokenPositionsInRecordStream = new Dictionary<FormulaToken, TokenPositionInfo>();

			// MD 7/6/09 - TFS18865
			// Moved from above. This needs to be done after getting the non shared equivalents of each token. That way all references on 
			// token point to other tokens in the resolvedTokenList.
			if ( this.IsSpecialFormula == false )
			{
				// Resolve all token references in the postfix list
				// MD 10/22/10 - TFS36696
				// Certain info is no longer stored on the tokens, so we need to store it in a dictionary. since this evaluator sets some of that info, 
				// it needs to take the dictionary.
				//TokenReferenceResolver resolver = new TokenReferenceResolver( this, new List<FormulaToken>( resolvedTokenList ) );
				TokenReferenceResolver resolver = new TokenReferenceResolver(this, new List<FormulaToken>(resolvedTokenList), tokenPositionsInRecordStream );

				EvaluationResult<FormulaToken> resolveResult = resolver.Evaluate();
				Debug.Assert( resolveResult.Completed );
			}

			// The first pass across all tokens should set their start positions
			long nextTokenStart = startPos;
			foreach ( FormulaToken token in resolvedTokenList )
			{
				// MD 10/22/10 - TFS36696
				// To save space, we will no longer save the portions in the record stream on the tokens.
				//token.PositionInRecordStream = nextTokenStart;
				//nextTokenStart += token.GetSize( stream, isForExternalNamedReference );
				TokenPositionInfo positionInfo;
				if (tokenPositionsInRecordStream.TryGetValue(token, out positionInfo))
					positionInfo.PositionInRecordStream = nextTokenStart;
				else
					tokenPositionsInRecordStream[token] = new TokenPositionInfo(nextTokenStart, token);

				nextTokenStart += token.GetSize(stream, isForExternalNamedReference, tokenPositionsInRecordStream);
			}

			// The second pass across all tokens should actually write the tokens
			foreach ( FormulaToken token in resolvedTokenList )
			{
				// MD 10/22/10 - TFS36696
				// To save space, we will no longer save the portions in the record stream on the tokens.
				//Debug.Assert( stream.Position == token.PositionInRecordStream );
				Debug.Assert(stream.Position == tokenPositionsInRecordStream[token].PositionInRecordStream);

				stream.Write( (byte)token.Token );
				token.Save(stream, isForExternalNamedReference, tokenPositionsInRecordStream);
			}

			// Determine the size of the rpn token array
			long endPos = stream.Position;
			size = (int)( endPos - startPos );

			// If the size should be written, go back and write the rpn token array size
			if ( writeSize )
			{
				stream.Position = startPos - 2;
				stream.Write( (ushort)size );
				stream.Position = endPos;
			}

			// Save the additional data for complex tokens
			foreach ( FormulaToken token in resolvedTokenList )
				token.SaveAdditionalData( stream );

			return size;
		}

		#endregion Save

		// MD 7/26/10 - TFS34398
		#region SetCalculatedValue

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal virtual void SetCalculatedValue(WorksheetCell cell, object value)
		internal virtual void SetCalculatedValue(WorksheetRow row, short columnIndex, object value)
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//Debug.Assert(cell == this.owningCell, "Only the owning cell should have been passed in here.");
			Debug.Assert(
				this.owningCellRow != null && row == this.owningCellRow && columnIndex == this.owningCellColumnIndex, 
				"Only the owning cell should have been passed in here.");

			this.calculatedValue = value;
		} 

		#endregion // SetCalculatedValue

		// MD 6/31/08 - Excel 2007 Format
		#region VerifyFormatLimits

        // MBS 7/10/08 - Excel 2007 Format
        // Added cellReferenceMode and parseFormula parameters
		// MD 3/2/12 - 12.1 - Table Support
		//internal void VerifyFormatLimits( FormatLimitErrors limitErrors, WorkbookFormat testFormat, CellReferenceMode cellReferenceMode, bool parseFormula)
		internal void VerifyFormatLimits(Workbook workbook, FormatLimitErrors limitErrors, WorkbookFormat testFormat, CellReferenceMode cellReferenceMode, bool parseFormula)
		{
			// MD 3/2/12 - 12.1 - Table Support
			
			this.ConvertTableReferencesToRanges(workbook, null);

			foreach ( FormulaToken token in this.postfixTokenList )
				token.VerifyFormatLimits( limitErrors, testFormat );

		    // MBS 7/10/08 - Excel 2007 Format
	        // We can handle the various format limits of the function by re-parsing it, so that
            // we can keep all of our logic centralized and not duplicate code
            if (parseFormula)
            {
                string formulaText = this.ToString(cellReferenceMode);
                FormulaParseException ex;
                Formula formula;
                if (Formula.TryParse(formulaText, cellReferenceMode, testFormat, out formula, out ex) == false)
                {
					// MD 3/2/12 - 12.1 - Table Support
                    //throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_InvalidForWorkbookFormat"), ex);
					limitErrors.AddError(SR.GetString("LE_InvalidOperationException_InvalidForWorkbookFormat"));
                }
            }
		} 

		#endregion VerifyFormatLimits

		#region VerifyNewOwner

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal virtual void VerifyNewOwner( IWorksheetCell owner )
		//{
		//    if ( this.owningCell != null && this.owningCell != owner )
		//        throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_FormulaAlreadyOwned" ) );
		//}
		internal virtual void VerifyNewOwner(WorksheetRow ownerRow, short ownerColumnIndex)
		{
			if (this.owningCellRow != null &&
				(this.owningCellRow != ownerRow || this.owningCellColumnIndex != ownerColumnIndex))
			{
				throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_FormulaAlreadyOwned"));
			}
		}

		#endregion VerifyNewOwner

		#endregion Internal Methods

		#region Static Methods

		#region CreateFormula



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static Formula CreateFormula( CellReferenceMode creationMode, FormulaType type )
		{
			Formula formula;

			if ( type == FormulaType.ArrayFormula )
				formula = new ArrayFormula( creationMode );
			else
			{
				// MD 8/20/07 - BR25818
				// The formula constructor takes another parameter now which indicates the formula type
				//formula = new Formula( creationMode );
				formula = new Formula( creationMode, type );
			}

			return formula;
		}

		#endregion CreateFormula

		#region Equals

		/// <summary>
		/// Determines whether two <see cref="Formula"/> instances are equal using the specified cell reference mode.
		/// </summary>
		/// <remarks>
		/// This essentially performs a case-insensitive string comparison, ignoring the white space in the formula.
		/// </remarks>
		/// <param name="formulaA">The first Formula to compare.</param>
		/// <param name="formulaB">The second Formula to compare.</param>
		/// <param name="cellReferenceMode">The cell reference mode to use when comparing the two formulas.</param>
		/// <returns>True if the formulas are both null or both equivalent; False otherwise.</returns>
		public static bool Equals( Formula formulaA, Formula formulaB, CellReferenceMode cellReferenceMode )
		{
			if ( formulaA == null && formulaB == null )
				return true;

			if ( formulaA == null ^ formulaB == null )
				return false;

			if ( formulaA == formulaB )
				return true;

			if ( formulaA.GetType() != formulaB.GetType() )
				return false;

			// MD 4/6/12 - TFS101506
			//FormulaStringGenerator stringGenerator1 = new FormulaStringGenerator( formulaA, cellReferenceMode, CultureInfo.CurrentCulture, true );
			//FormulaStringGenerator stringGenerator2 = new FormulaStringGenerator( formulaB, cellReferenceMode, CultureInfo.CurrentCulture, true );
			FormulaStringGenerator stringGenerator1 = new FormulaStringGenerator(formulaA, cellReferenceMode, CultureInfo.InvariantCulture, true);
			FormulaStringGenerator stringGenerator2 = new FormulaStringGenerator(formulaB, cellReferenceMode, CultureInfo.InvariantCulture, true);

			EvaluationResult<string> result1 = stringGenerator1.Evaluate();
			EvaluationResult<string> result2 = stringGenerator2.Evaluate();
			Debug.Assert( result1.Completed && result2.Completed );

			// MD 4/6/12 - TFS101506
			//return String.Compare( result1.Result, result2.Result, StringComparison.CurrentCultureIgnoreCase ) == 0;
			return String.Compare(result1.Result, result2.Result, StringComparison.InvariantCultureIgnoreCase) == 0;
		}

		#endregion Equals

		#region Load







		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//internal static Formula Load( WorkbookSerializationManager manager, BiffRecordStream stream, FormulaType type )
		// MD 6/13/12 - CalcEngineRefactor
		//internal static Formula Load( WorkbookSerializationManager manager, BiffRecordStream stream, FormulaType type, ref byte[] data, ref int dataIndex )
		internal static Formula Load(BiffRecordStream stream, FormulaType type, ref byte[] data, ref int dataIndex)
		{
			ushort tokenArraySize = stream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			Formula formula = Formula.CreateFormula(stream.Manager.Workbook.CellReferenceMode, type);
			Formula.LoadFormulaData( stream, tokenArraySize, type, formula, ref data, ref dataIndex );

			return formula;
		}







		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//internal static Formula Load( WorkbookSerializationManager manager, BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type )
		// MD 6/13/12 - CalcEngineRefactor
		//internal static Formula Load( WorkbookSerializationManager manager, BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type, ref byte[] data, ref int dataIndex )
		internal static Formula Load(BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type, ref byte[] data, ref int dataIndex)
		{
			// MD 9/12/08 - TFS6887
			// Some formulas are optional. In that case, they have a token array size of 0 and a null formula should be returned.
			if ( rpnTokenArraySize == 0 )
				return null;

			Formula formula = Formula.CreateFormula(stream.Manager.Workbook.CellReferenceMode, type);
			Formula.LoadFormulaData( stream, rpnTokenArraySize, type, formula, ref data, ref dataIndex );

			return formula;
		}

		#endregion Load

		#region LoadFormulaData







		// MD 9/23/09 - TFS19150
		// Every read operation is relatively slow, so the buffer is now cached and passed into this method so we can get values from it.
		//internal static void LoadFormulaData( WorkbookSerializationManager manager, BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type, Formula formula )
		// MD 6/13/12 - CalcEngineRefactor
		//internal static void LoadFormulaData( WorkbookSerializationManager manager, BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type, Formula formula, ref byte[] data, ref int dataIndex )
		internal static void LoadFormulaData(BiffRecordStream stream, ushort rpnTokenArraySize, FormulaType type, Formula formula, ref byte[] data, ref int dataIndex)
		{
			formula.isSharedFormula = type == FormulaType.SharedFormula;

			// If there is now data to read for the formula, just return
			if ( rpnTokenArraySize == 0 )
				return;

			// Determine where the end of the rpn token array is, which is also where the addition data section begins
			int additionalDataStart = dataIndex + rpnTokenArraySize;







			// MD 5/10/12 - TFS111368
			bool shouldResolveAddInFunctions = false;

			// Keep reading tokens until we get to the additional data section
			while ( dataIndex < additionalDataStart )
			{
				// Store the start position of the next token we will read
				int startPosition = dataIndex;

				// Read the token code for the next token
				byte tokenCode = stream.ReadByteFromBuffer( ref data, ref dataIndex );

				// Create a token based on the token code
				// MD 10/22/10 - TFS36696
				// We don't need to store the formula on the token anymore.
				//FormulaToken currentToken = FormulaToken.CreateToken( formula, stream, tokenCode, ref data, ref dataIndex );
				FormulaToken currentToken = FormulaToken.CreateToken(stream, tokenCode, ref data, ref dataIndex);

				// If we couldn't get the token, there is a problem, stop looping because the next token 
				// code we read will most likely be data from the current token not the next token code
				if ( currentToken == null )
				{
					Utilities.DebugFail( "Couldn't read the current token." );
					break;
				}

				// Make sure we created the right token
				Debug.Assert( currentToken.Token == (Token)tokenCode, "The type of the token doesn't match the token code" );

				// Store the position of the token in the data stream
				// MD 10/22/10 - TFS36696
				// To save space, we will no longer save the portions in the record stream on the tokens.
				//currentToken.PositionInRecordStream = startPosition;




				// Let the token read its data from the stream
				currentToken.Load( stream, type == FormulaType.ExternalNamedReferenceFormula, ref data, ref dataIndex );

				// If the current token is a volatile attr token, this formula should recalculate always
				// (only the first token will be volatile)
				AttrTokenBase attrToken = currentToken as AttrTokenBase;
				if ( attrToken != null && attrToken.Volatile )
					formula.recalculateAlways = true;

				// MD 5/10/12 - TFS111368
				// If an add in function was loaded, we need to resolve the function name after everything loads.
				if (shouldResolveAddInFunctions == false)
				{
					FunctionVOperator funcVToken = currentToken as FunctionVOperator;
					if (funcVToken != null && funcVToken.Function == Function.AddInFunction)
						shouldResolveAddInFunctions = true;
				}

				formula.postfixTokenList.Add( currentToken );

				// Make sure our token size calculations are correct for the token type, we will need that to be correct 
				// when saving
				// MD 10/22/10 - TFS36696
				// To save space, we will no longer save the portions in the record stream on the tokens.
				//Debug.Assert(
				//    dataIndex - startPosition ==
				//    currentToken.GetSize( stream, type == FormulaType.ExternalNamedReferenceFormula ) );
				// Make sure our token size calculations are correct for the token type, we will need that to be correct 
				// when saving
				
				// MD 4/21/11
				// Found while fixing TFS64442
				// Getting the size of some tokens actually writes to the CurrentRecordStream, so we should not be doing this check anymore.
//#if DEBUG
//                Debug.Assert(
//                    dataIndex - startPosition ==
//                    currentToken.GetSize(stream, type == FormulaType.ExternalNamedReferenceFormula, tokenPositionsInRecordStream)); 
//#endif
			}

			// After reading the rpn token array, make sure the stream is at the additional data section
			if ( dataIndex != additionalDataStart )
			{
				Utilities.DebugFail( "We did not read the entire rpn token array." );
				dataIndex = additionalDataStart;
			}

			// Let the tokens read their additional data
			foreach ( FormulaToken currentToken in formula.postfixTokenList )
				currentToken.LoadAdditionalData( stream, ref data, ref dataIndex );

			// If the token list only has one token, it could be a special formula indicating its part of a shared formula, 
			// array formula, or data table interior region
			if ( formula.postfixTokenList.Count == 1 )
			{
				ExpToken expToken = formula.postfixTokenList[ 0 ] as ExpToken;

				if ( expToken != null )
				{
					formula.isArrayOrSharedFormula = true;
					return;
				}

				TblToken tblToken = formula.postfixTokenList[ 0 ] as TblToken;

				if ( tblToken != null )
				{
					formula.isDataTableInterior = true;
					return;
				}
			}

			// MD 5/10/12 - TFS111368
			// We need any add-in FuncV tokens to be resolved.
			if (shouldResolveAddInFunctions)
				new AddInFunctionResolver(formula).Evaluate();
		}

		#endregion LoadFormulaData

		// MD 7/9/08 - Excel 2007 Format
		#region Parse( string, Workbook )



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static Formula Parse( string value, Workbook workbook )
		{
			CellReferenceMode cellReferenceMode;
			WorkbookFormat currentFormat;

			// MD 4/6/12 - TFS101506
			//Workbook.GetWorkbookOptions( workbook, out cellReferenceMode, out currentFormat );
			//
			//return Formula.Parse( value, cellReferenceMode, currentFormat );
			CultureInfo culture;
			Workbook.GetWorkbookOptions(workbook, out cellReferenceMode, out currentFormat, out culture);

			return Formula.Parse(value, cellReferenceMode, currentFormat, culture);
		}

		#endregion Parse( string, Workbook )

		#region Parse( string, CellReferenceMode )

		/// <summary>
		/// Parses the specified formula value and returns the formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <returns>A <see cref="Formula"/> instance which represents the formula specified.</returns>
		public static Formula Parse( string value, CellReferenceMode cellReferenceMode )
		{
			return Formula.Parse( value, cellReferenceMode, CultureInfo.CurrentCulture );
		}

		#endregion Parse( string, CellReferenceMode )

		#region Parse( string, CellReferenceMode, CultureInfo )

		/// <summary>
		/// Parses the specified formula value and returns the formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <returns>A <see cref="Formula"/> instance which represents the formula specified.</returns>
		public static Formula Parse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture )
		{
			// MD 7/9/08 - Excel 2007 Format
			// Call off to new overload
			//return Formula.Parse( value, cellReferenceMode, FormulaType.Formula );
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return Formula.Parse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture );
			return Formula.Parse(value, cellReferenceMode, Workbook.LatestFormat, culture);
		}

		#endregion Parse( string, CellReferenceMode, CultureInfo )

		// MD 7/9/08 - Excel 2007 Format
		#region Parse( string, CellReferenceMode, WorkbookFormat )

		/// <summary>
		/// Parses the specified formula value and returns the formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <returns>A <see cref="Formula"/> instance which represents the formula specified.</returns>
		public static Formula Parse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat )
		{
			return Formula.Parse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture );
		}

		#endregion Parse( string, CellReferenceMode, WorkbookFormat )

		#region Parse( string, CellReferenceMode, WorkbookFormat, CultureInfo )

		/// <summary>
		/// Parses the specified formula value and returns the formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <returns>A <see cref="Formula"/> instance which represents the formula specified.</returns>
		public static Formula Parse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture )
		{
			// MD 2/23/12 - TFS101504
			// Pass along null for the new indexedReferencesDuringLoad parameter.
			//return Formula.Parse( value, cellReferenceMode, FormulaType.Formula, fileFormat, culture );
			return Formula.Parse(value, cellReferenceMode, FormulaType.Formula, fileFormat, culture, null);
		}

		#endregion Parse( string, CellReferenceMode, WorkbookFormat, CultureInfo )

		#region Parse( string, CellReferenceMode, FormulaType, WorkbookFormat, CultureInfo )



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		// MD 7/9/08 - Excel 2007 Format
		//internal static Formula Parse( string value, CellReferenceMode cellReferenceMode, FormulaType type )
		// MD 2/23/12 - TFS101504
		// Added a new indexedReferencesDuringLoad parameter.
		//internal static Formula Parse( string value, CellReferenceMode cellReferenceMode, FormulaType type, WorkbookFormat fileFormat, CultureInfo culture )
		internal static Formula Parse(string value, CellReferenceMode cellReferenceMode, FormulaType type, WorkbookFormat fileFormat, CultureInfo culture, List<WorkbookReferenceBase> indexedReferencesDuringLoad)
		{
			Formula formula;
			FormulaParseException exc;

			// MD 7/9/08 - Excel 2007 Format
			//if ( Formula.TryParse( value, cellReferenceMode, type, out formula, out exc ) )
			// MD 2/23/12 - TFS101504
			// Added a new indexedReferencesDuringLoad parameter.
			//if ( Formula.TryParse( value, cellReferenceMode, type, fileFormat, culture, out formula, out exc ) )
			if (Formula.TryParse(value, cellReferenceMode, type, fileFormat, culture, indexedReferencesDuringLoad, out formula, out exc))
			{
				Debug.Assert( formula != null );
				return formula;
			}

			Debug.Assert( exc != null );
			throw exc;
		}

		#endregion Parse( string, CellReferenceMode, FormulaType, WorkbookFormat, CultureInfo )

		#region TryParse ( string, CellReferenceMode, out Formula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, out Formula formula )
		{
			return Formula.TryParse( value, cellReferenceMode, CultureInfo.CurrentCulture, out formula );
		}

		#endregion TryParse ( string, CellReferenceMode, out Formula )

		#region TryParse ( string, CellReferenceMode, CultureInfo, out Formula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture, out Formula formula )
		{
			// MD 7/9/08 - Excel 2007 Format
			//FormulaParseException exc;
			//return Formula.TryParse( value, cellReferenceMode, out formula, out exc );
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return Formula.TryParse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture, out formula );
			return Formula.TryParse(value, cellReferenceMode, Workbook.LatestFormat, culture, out formula);
		}

		#endregion TryParse ( string, CellReferenceMode, CultureInfo, out Formula )

		#region TryParse ( string, CellReferenceMode, out Formula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>
		/// if the conversion succeeded or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <param name="exception">
		/// When this method returns, contains the error information if the conversion failed or null if the 
		/// conversion succeeded. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, out Formula formula, out FormulaParseException exception )
		{
			return Formula.TryParse( value, cellReferenceMode, CultureInfo.CurrentCulture, out formula, out exception );
		}

		#endregion TryParse ( string, CellReferenceMode, out Formula, out FormulaParseException )

		#region TryParse ( string, CellReferenceMode, CultureInfo, out Formula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>
		/// if the conversion succeeded or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <param name="exception">
		/// When this method returns, contains the error information if the conversion failed or null if the 
		/// conversion succeeded. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture, out Formula formula, out FormulaParseException exception )
		{
			// MD 7/9/08 - Excel 2007 Format
			// Call off to new overload
			//return Formula.TryParse( value, cellReferenceMode, FormulaType.Formula, out formula, out exception );
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return Formula.TryParse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture, out formula, out exception );
			return Formula.TryParse(value, cellReferenceMode, Workbook.LatestFormat, culture, out formula, out exception);
		}

		#endregion TryParse ( string, CellReferenceMode, CultureInfo, out Formula, out FormulaParseException )

		// MD 7/9/08 - Excel 2007 Format
		#region TryParse ( string, CellReferenceMode, WorkbookFormat, out Formula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, out Formula formula )
		{
			return Formula.TryParse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture, out formula );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, out Formula )

		#region TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out Formula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture, out Formula formula )
		{
			FormulaParseException exc;
			return Formula.TryParse( value, cellReferenceMode, fileFormat, culture, out formula, out exc );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out Formula )

		// MD 7/9/08 - Excel 2007 Format
		#region TryParse ( string, CellReferenceMode, WorkbookFormat, out Formula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>
		/// if the conversion succeeded or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <param name="exception">
		/// When this method returns, contains the error information if the conversion failed or null if the 
		/// conversion succeeded. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, out Formula formula, out FormulaParseException exception )
		{
			return Formula.TryParse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture, out formula, out exception );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, out Formula, out FormulaParseException )

		#region TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out Formula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the formula which was parsed from <paramref name="value"/>
		/// if the conversion succeeded or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <param name="exception">
		/// When this method returns, contains the error information if the conversion failed or null if the 
		/// conversion succeeded. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="fileFormat"/> is not defined in the <see cref="WorkbookFormat"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture, out Formula formula, out FormulaParseException exception )
		{
			// MD 2/23/12 - TFS101504
			// Pass along null for the new indexedReferencesDuringLoad parameter.
			//return Formula.TryParse( value, cellReferenceMode, FormulaType.Formula, fileFormat, culture, out formula, out exception );
			return Formula.TryParse(value, cellReferenceMode, FormulaType.Formula, fileFormat, culture, null, out formula, out exception);
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out Formula, out FormulaParseException )

		#region TryParse ( string, CellReferenceMode, FormulaType, WorkbookFormat, out Formula, out FormulaParseException )



#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

		// MD 7/9/08 - Excel 2007 Format
		//internal static bool TryParse( string value, CellReferenceMode cellReferenceMode, FormulaType type, out Formula formula, out FormulaParseException exception )
		internal static bool TryParse( 
			string value, CellReferenceMode cellReferenceMode, FormulaType type, WorkbookFormat fileFormat, CultureInfo culture,
			List<WorkbookReferenceBase> indexedReferencesDuringLoad,	// MD 2/23/12 - TFS101504
			out Formula formula, out FormulaParseException exception )
		{
			if ( String.IsNullOrEmpty( value ) )
				throw new ArgumentNullException( "value", "The formula string cannot be null." );

			// MD 10/22/10 - TFS36696
			// Use the utility function instead of Enum.IsDefined. It is faster.
			//if ( Enum.IsDefined( typeof( CellReferenceMode ), cellReferenceMode ) == false )
			if (Utilities.IsCellReferenceModeDefined(cellReferenceMode) == false)
				throw new InvalidEnumArgumentException( "cellReferenceMode", (int)cellReferenceMode, typeof( CellReferenceMode ) );

			// MD 7/9/08 - Excel 2007 Format
			// MD 10/22/10 - TFS36696
			// Use the utility function instead of Enum.IsDefined. It is faster.
			//if ( Enum.IsDefined( typeof( WorkbookFormat ), fileFormat ) == false )
			if (Utilities.IsWorkbookFormatDefined(fileFormat) == false)
				throw new InvalidEnumArgumentException( "fileFormat", (int)fileFormat, typeof( WorkbookFormat ) );

			// MD 7/9/08 - Excel 2007 Format
			//formula = FormulaParser.ParseFormula( value, cellReferenceMode, type, out exception );
			// MD 2/23/12 - TFS101504
			// Added a new indexedReferencesDuringLoad parameter.
			//formula = FormulaParser.ParseFormula( value, cellReferenceMode, type, fileFormat, culture, out exception );
			formula = FormulaParser.ParseFormula(value, cellReferenceMode, type, fileFormat, culture, indexedReferencesDuringLoad, out exception);

			Debug.Assert( formula == null ^ exception == null );

			return formula != null;
		}

		#endregion TryParse ( string, CellReferenceMode, FormulaType, WorkbookFormat, out Formula, out FormulaParseException )

		#endregion Static Methods

		#region Private Methods

		#region ClearCache

		// MD 9/19/11 - TFS86108
		// Made internal so this could be used in other classes.
		//private void ClearCache()
		internal void ClearCache()
		{
			this.cachedA1Value = null;
			this.cachedR1C1Value = null;
		}

		#endregion ClearCache

		#region GetFormulaTokens






		[System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
		internal FormulaToken[] GetFormulaTokens()
		{
			return this.postfixTokenList.ToArray();
		} 

		#endregion GetFormulaTokens

		// MD 3/2/12 - 12.1 - Table Support
		#region RecompileFormulas

		private void RecompileFormulas()
		{
			if (this.compiledFormulas == null)
				return;

			List<ExcelCalcFormula> oldCompiledFormulas = new List<ExcelCalcFormula>(this.compiledFormulas);

			// MD 7/19/12 - TFS116808 (Table resizing)
			// Moved code to new overload.
			this.RecompileFormulasHelper(oldCompiledFormulas);
		}

		// MD 7/19/12 - TFS116808 (Table resizing)
		private void RecompileFormulasHelper(List<ExcelCalcFormula> oldCompiledFormulas)
		{
			for (int i = 0; i < oldCompiledFormulas.Count; i++)
			{
				ExcelCalcFormula oldCompiledFormula = oldCompiledFormulas[i];
				ExcelRefBase reference = oldCompiledFormula.BaseReference as ExcelRefBase;
				if (reference != null)
					reference.SetAndCompileFormula(this, false, true, true);
			}
		}

		#endregion // RecompileFormulas

		#region ToStringHelper

		// MD 10/13/10 - TFS49055
		// The formula string may be different depending on the culture, so we need a dictionary of caches.
		//private string ToStringHelper( ref string cache, CellReferenceMode cellReferenceMode, CultureInfo culture )
		private string ToStringHelper(ref Dictionary<CultureInfo, string> cache, CellReferenceMode cellReferenceMode, CultureInfo culture)
		{
			// MD 5/13/11 - Data Validations / Page Breaks
			// We can allow a null CultureInfo and assume the current culture.
			if (culture == null)
			{
				// MD 4/9/12 - TFS101506
				//culture = CultureInfo.CurrentCulture;
				culture = this.Culture;
			}

			// MD 10/13/10 - TFS49055
			// The formula string may be different depending on the culture, so we need a dictionary of caches.
			//if ( cache != null )
			//    return cache;
			if (cache == null)
			{
				cache = new Dictionary<CultureInfo, string>();
			}
			else
			{
				string cachedValue;
				if (cache.TryGetValue(culture, out cachedValue))
					return cachedValue;
			}

			// MD 2/4/11 - TFS65015
			// Moved this code to a new ToStringHelper so we can call it from other places.
			//FormulaStringGenerator generator = new FormulaStringGenerator( this, cellReferenceMode, culture );
			//EvaluationResult<string> result = generator.Evaluate();
			//
			//Debug.Assert( result.Completed );
			//
			//// Only cache the result if the formula was successfully converted to a string
			//if ( result.Completed )
			//{
			//    // MD 10/13/10 - TFS49055
			//    // The formula string may be different depending on the culture, so we need a dictionary of caches.
			//    //cache = result.Result;
			//    cache[culture] = result.Result;
			//}
			//
			//return result.Result;
			string value = this.ToStringHelper(cellReferenceMode, culture, false);

			if (value != null)
				cache[culture] = value;

			return value;
		}

		// MD 2/4/11 - TFS65015
		// Added a new helper method with an isForSaving parameter. When saving add-in functions in the 2007 format, 
		// their names need to be preceded by "_xll."
		internal string ToStringHelper(CellReferenceMode cellReferenceMode, CultureInfo culture, bool isForSaving)
		{
			// MD 10/30/11
			// Found while fixing TFS90733
			// Instead asserting all the time while debugging a formula that is loading, we should just return an empty string when 
			// there are no tokens.
			if (this.postfixTokenList == null || this.postfixTokenList.Count == 0)
				return string.Empty;

			FormulaStringGenerator generator = new FormulaStringGenerator(this, cellReferenceMode, culture);
			generator.IsForSaving = isForSaving;
			EvaluationResult<string> result = generator.Evaluate();

			Debug.Assert(result.Completed, "There was an error generating the formula string.");
			return result.Result;
		}

		#endregion ToStringHelper

		#endregion Private Methods

		#endregion Methods

		#region Properties

		// MD 2/28/12 - 12.1 - Table Support
		#region AddCompiledFormula

		internal void AddCompiledFormula(ExcelCalcFormula compiledFormula)
		{
			if (this.compiledFormulas == null)
				this.compiledFormulas = new List<ExcelCalcFormula>();
			
			this.compiledFormulas.Add(compiledFormula);
		}

		#endregion // AddCompiledFormula

		// MD 4/6/12 - TFS101506
		#region Culture

		internal CultureInfo Culture
		{
			get
			{
				if (this.OwningCellRow != null)
					return this.OwningCellRow.Worksheet.Culture;

				return CultureInfo.CurrentCulture;
			}
		}

		#endregion // Culture

		// MD 6/31/08 - Excel 2007 Format
		#region CurrentFormat

		internal WorkbookFormat CurrentFormat
		{
			get
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//if ( this.owningCell == null )
				//    return this.parsedWorkbookFormat;
				//
				//return this.owningCell.Worksheet.CurrentFormat;
				if (this.owningCellRow == null)
					return this.parsedWorkbookFormat;

				return this.owningCellRow.Worksheet.CurrentFormat;
			}
			// MD 7/23/08 - Excel formula solving
			// The formula needs to have a reference to a workbook with the correct format set when it is being parsed.
			set
			{
				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//Debug.Assert( this.owningCell == null, "The WorkbookForTokenReferences should not be set when the formula is owned." );
				Debug.Assert(this.owningCellRow == null, "The WorkbookForTokenReferences should not be set when the formula is owned.");

				this.parsedWorkbookFormat = value;
			}
		}

		#endregion CurrentFormat

		// MD 8/12/08 - Excel formula solving
		#region IsOwnedByAllCellsAppliedTo






		internal virtual bool IsOwnedByAllCellsAppliedTo
		{
			get { return true; }
		}

		#endregion IsOwnedByAllCellsAppliedTo

		#region IsSpecialFormula

		internal bool IsSpecialFormula
		{
			get { return this.isArrayOrSharedFormula || this.isDataTableInterior; }
		}

		#endregion IsSpecialFormula

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		#region Removed
      
		//#region OwningCell

		//internal IWorksheetCell OwningCell
		//{
		//    get { return this.owningCell; }
		//    set
		//    {
		//        this.owningCell = value;

		//        // MD 8/20/08
		//        // Found while fixing Excel formula solving
		//        // There is no need to repeat this code, it is already in ClearCache.
		//        //// MD 10/19/07
		//        //// Found while fixing BR27421
		//        //// If the owning cell has changed, the string representatins of the formula may change, reset the cache here
		//        //this.cachedA1Value = null;
		//        //this.cachedR1C1Value = null;
		//        this.ClearCache();

		//        // MD 7/26/10 - TFS34398
		//        // Clear the cached value when the cell changes.
		//        this.calculatedValue = null;
		//    }
		//}

		//#endregion OwningCell 
    
		#endregion  // Removed

		// MD 4/12/11 - TFS67084
		#region OwningCellRow

		internal WorksheetRow OwningCellRow
		{
			get { return this.owningCellRow; }
		}

		#endregion  // OwningCellRow

		// MD 4/12/11 - TFS67084
		#region OwningCellColumnIndex

		internal short OwningCellColumnIndex
		{
			get { return this.owningCellColumnIndex; }
		}

		#endregion  // OwningCellColumnIndex

		#region PostfixTokenList

		internal List<FormulaToken> PostfixTokenList
		{
			get { return this.postfixTokenList; }
		}

		#endregion PostfixTokenList

		#region RecalculateAlways

		internal bool RecalculateAlways
		{
			get { return this.recalculateAlways; }
			// MD 7/23/08 - Excel formula solving
			// The when the formula is being parsed, it has to have it's recalculateAlways member set.
			set 
			{
				Debug.Assert( value, "The RecalculateAlways setter should only be used to set the property to True." );
				this.recalculateAlways = value; 
			}
		}

		#endregion RecalculateAlways 

		// MD 2/28/12 - 12.1 - Table Support
		#region RemoveCompiledFormula

		internal void RemoveCompiledFormula(ExcelCalcFormula compiledFormula)
		{
			Debug.Assert(this.compiledFormulas != null && this.compiledFormulas.Contains(compiledFormula), "This is unexpected.");
			if (this.compiledFormulas == null)
				return;

			this.compiledFormulas.Remove(compiledFormula);
		}

		#endregion // RemoveCompiledFormula

		// MD 8/20/07 - BR25818
		// The formula type is now stored with the formula
		#region Type






		internal FormulaType Type
		{
			get { return this.type; }
		}

		#endregion Type

		#region Workbook

		internal Workbook Workbook
		{
			get
			{
				// MD 6/13/12 - CalcEngineRefactor
				//if (this.owningCellRow == null)
				//    return null;
				//
				//Worksheet worksheet = this.owningCellRow.Worksheet;
				Worksheet worksheet = this.Worksheet;

				if (worksheet == null)
					return null;

				return worksheet.Workbook;
			}
		}

		#endregion // Workbook

		// MD 6/13/12 - CalcEngineRefactor
		#region Worksheet

		internal Worksheet Worksheet
		{
			get
			{
				if (this.owningCellRow == null)
					return null;

				return this.owningCellRow.Worksheet;
			}
		}

		#endregion // Worksheet

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