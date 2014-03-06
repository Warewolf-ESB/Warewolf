using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;




using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents an array formula for a group of cells.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Array formulas are similar to regular formula in that they have the same grammar. However, array formulas must be set 
	/// on a single region of cells only. When the array formula is applied to a region of cells, each cell's 
	/// <see cref="WorksheetCell.Formula">Formula</see> property will be the array formula. The 
	/// <see cref="WorksheetCell.Formula">Value</see> of each cell cannot be changed unless <see cref="ClearCellRange"/> is 
	/// called on the array formula or another value is applied to a region of cells which completely contains the array 
	/// formula's region.
	/// </p>
	/// <p class="body">
	/// Because the array formula stores the region of the cells to which it is applied in the <see cref="CellRange"/> property, 
	/// the array formula can only be applied to one region of cells.
	/// </p>
	/// <p class="body">
	/// Array formulas are created through Microsoft Excel by selecting a region of cells, entering a formula for
	/// that range, and pressing Ctrl+Shift+Enter. This causes the formula of each cell in the region to appear as follows:
	/// {=Formula}.
	/// </p>
	/// <p class="body">
	/// See the Microsoft Excel documentation for more information on array formulas.
	/// </p>
	/// </remarks>
	[DebuggerDisplay( @"ArrayFormula: \{{ToString(),nq}\}" )]



	public

		class ArrayFormula : Formula,
		IWorksheetRegionBlockingValue
	{
		#region Member Variables

		// MD 7/19/12 - TFS116808 (Table resizing)
		private WorksheetRegion cachedRegionWhileShifting;

		// MD 7/26/10 - TFS34398
		// The formulas will now cache the calculated values instead of the cells.
		private object[,] calculatedValues;

		private WorksheetRegion cellRange;
		private Formula interiorCellFormula;

		#endregion Member Variables

		#region Constructor

		internal ArrayFormula( CellReferenceMode creationMode )
			// MD 8/20/07 - BR25818
			// The formula constructor takes another parameter now which indicates the formula type
			//: base( creationMode ) { }
			: base( creationMode, FormulaType.ArrayFormula ) { }

		internal ArrayFormula( ArrayFormula arrayFormula )
			: base( (Formula)arrayFormula ) { }

		#endregion Constructor

		#region Base Class Overrides

		#region ApplyTo

		// MD 2/24/12
		// Found while implementing 12.1 - Table Support
		// We need to do something custom for ArrayFormulas when it is applied to a single cell.
		internal override void ApplyTo(WorksheetRow row, short columnIndex)
		{
			this.ApplyTo(new WorksheetRegion(row.Worksheet, row.Index, columnIndex, row.Index, columnIndex));
		}

		/// <summary>
		/// Applies the formula to all specified regions of cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This method, or one of the other ApplyTo overrides must be used to set the value of a cell to a formula.
		/// </p>
		/// <p class="body">
		/// After this method returns, the <see cref="WorksheetCell.Formula"/> of all cells in all specified regions will
		/// return the array formula.
		/// </p>
		/// </remarks>
		/// <param name="regions">The regions of cells to apply the formula to.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="regions"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The length <paramref name="regions"/> is anything other than one.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The array formula is already applied to a cell region.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// One or more regions specified contain array formulas or data tables which extend outside the region.
		/// </exception>
		/// <seealso cref="Formula.ApplyTo(WorksheetCell)"/>
		/// <seealso cref="Formula.ApplyTo(WorksheetRegion)"/>
		/// <seealso cref="WorksheetRegion.ApplyArrayFormula(string)"/>
		public override void ApplyTo( WorksheetRegion[] regions )
		{
			if ( regions == null )
				throw new ArgumentNullException( "regions" );

			if ( regions.Length != 1 )
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_ArrayFormulaMustHaveSingleRegion" ), "regions" );

			if ( this.cellRange != null )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_ArrayFormulaAlreadyApplied" ) );

			WorksheetRegion region = regions[ 0 ];

			#region Moved

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////WorksheetCell.VerifyRegionForBlockingValue( this, region );
			//WorksheetCellBlock.VerifyRegionForBlockingValue(this, region);

			//// MD 2/24/12 - 12.1 - Table Support
			//region.Worksheet.VerifyFormula(this);

			//this.cellRange = region;

			//// MD 7/26/10 - TFS34398
			//// The formulas will now cache the calculated values instead of the cells.
			//if (region == null)
			//    this.calculatedValues = null;
			//else
			//    this.calculatedValues = new object[region.Width, region.Height];

			//// MD 8/17/08 - Excel formula solving
			//// Moved from below - the OwningCell has to be set before the array formula is applied to the region so individual cells creating their calc 
			//// formulas can offset relative references correctly.
			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////this.OwningCell = region.Worksheet.Rows[ region.FirstRow ].Cells[ region.FirstColumn ];
			//this.SetOwningCell(region.TopRow, region.FirstColumnInternal);

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////WorksheetCell.ApplyBlockingValue( this );
			//WorksheetCellBlock.ApplyBlockingValue(this);

			//// MD 8/17/08 - Excel formula solving
			//// Moved above - see comment above
			////this.OwningCell = region.Worksheet.Rows[ region.FirstRow ].Cells[ region.FirstColumn ];

			#endregion // Moved
			this.ApplyToRegionHelper(region);
		}

		#endregion ApplyTo

		// MD 3/2/12 - 12.1 - Table Support
		#region ApplyToCell

		internal override void ApplyToCell(WorksheetRow row, short columnIndex, WorksheetCellBlock cellBlock)
		{
			this.ApplyToRegionHelper(row.Worksheet.GetCachedRegion(row.Index, columnIndex, row.Index, columnIndex));
		}

		#endregion // ApplyToCell

		#region Clone

		internal override Formula Clone()
		{
			return new ArrayFormula( this );
		}

		#endregion Clone

		// MD 7/26/10 - TFS34398
		#region GetCalculatedValue

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal override object GetCalculatedValue(WorksheetCell cell)
		//{
		//    Debug.Assert(this.cellRange.Contains(cell), "The cell should be in the array formula's range");
		//
		//    int relativeRow = cell.RowIndex - this.cellRange.FirstRow;
		//    int relativeColumn = cell.ColumnIndex - this.cellRange.FirstColumn;
		//    return this.calculatedValues[relativeColumn, relativeRow];
		//} 
		internal override object GetCalculatedValue(WorksheetRow row, short columnIndex)
		{
			Debug.Assert(this.cellRange.Contains(row, columnIndex), "The cell should be in the array formula's range");

			int relativeRow = row.Index - this.cellRange.FirstRow;
			int relativeColumn = columnIndex - this.cellRange.FirstColumn;
			return this.calculatedValues[relativeColumn, relativeRow];
		} 

		#endregion // GetCalculatedValue

		// MD 8/12/08 - Excel formula solving
		#region IsOwnedByAllCellsAppliedTo






		internal override bool IsOwnedByAllCellsAppliedTo
		{
			get { return false; }
		}

		#endregion IsOwnedByAllCellsAppliedTo

		// MD 7/26/10 - TFS34398
		#region SetCalculatedValue

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal override void SetCalculatedValue(WorksheetCell cell, object value)
		//{
		//    Debug.Assert(this.cellRange.Contains(cell), "The cell should be in the array formula's range");
		//
		//    int relativeRow = cell.RowIndex - this.cellRange.FirstRow;
		//    int relativeColumn = cell.ColumnIndex - this.cellRange.FirstColumn;
		//    this.calculatedValues[relativeColumn, relativeRow] = value;
		//} 
		internal override void SetCalculatedValue(WorksheetRow row, short columnIndex, object value)
		{
			Debug.Assert(this.cellRange.Contains(row, columnIndex), "The cell should be in the array formula's range");

			int relativeRow = row.Index - this.cellRange.FirstRow;
			int relativeColumn = columnIndex - this.cellRange.FirstColumn;
			this.calculatedValues[relativeColumn, relativeRow] = value;
		} 

		#endregion // SetCalculatedValue

		#region VerifyNewOwner

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal override void VerifyNewOwner( IWorksheetCell owner ) { }
		internal override void VerifyNewOwner(WorksheetRow ownerRow, short ownerColumnIndex) { }

		#endregion VerifyNewOwner

		#endregion Base Class Overrides

		#region Interfaces

		#region IWorksheetRegionBlockingValue Members

		void IWorksheetRegionBlockingValue.RemoveFromRegion()
		{
			this.ClearCellRange();
		}

		void IWorksheetRegionBlockingValue.ThrowBlockingException()
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_CantChangeArrayFormula" ) );
		}

		void IWorksheetRegionBlockingValue.ThrowExceptionWhenMergedCellsInRegion()
		{
			throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_ArrayFormulaInMergedCell" ) );
		}

		void IWorksheetRegionBlockingValue.ThrowExceptionWhenTableInRegion()
		{
			throw new InvalidOperationException(SR.GetString("LE_InvalidOperationException_ArrayFormulaAppliedInTable"));
		}

		WorksheetRegion IWorksheetRegionBlockingValue.Region
		{
			get { return this.cellRange; }
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		#region ClearCellRange

		/// <summary>
		/// Removes this array formula as the formula for the cells to which it was applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// After this method returns, the <see cref="CellRange"/> will be null.
		/// </p>
		/// </remarks>
		public void ClearCellRange()
		{
			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//WorksheetCell.ClearBlockingValue( this );
			WorksheetCellBlock.ClearBlockingValue(this);

			this.cellRange = null;
			this.interiorCellFormula = null;

			// MD 3/2/12 - 12.1 - Table Support
			this.calculatedValues = null;
		}

		#endregion ClearCellRange

		#endregion Public Methods

		#region Static Methods

		// MD 7/9/08 - Excel 2007 Format
		#region Parse( string, Workbook )



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal new static ArrayFormula Parse( string value, Workbook workbook )
		{
			CellReferenceMode cellReferenceMode;
			WorkbookFormat currentFormat;

			// MD 4/6/12 - TFS101506
			//Workbook.GetWorkbookOptions( workbook, out cellReferenceMode, out currentFormat );
			//
			//return ArrayFormula.Parse( value, cellReferenceMode, currentFormat );
			CultureInfo culture;
			Workbook.GetWorkbookOptions(workbook, out cellReferenceMode, out currentFormat, out culture);

			return ArrayFormula.Parse(value, cellReferenceMode, currentFormat, culture);
		}

		#endregion Parse( string, Workbook )

		#region Parse( string, CellReferenceMode )

		/// <summary>
		/// Parses the specified formula value and returns the array formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <returns>An <see cref="ArrayFormula"/> instance which represents the array formula value specified.</returns>
		public new static ArrayFormula Parse( string value, CellReferenceMode cellReferenceMode )
		{
			return ArrayFormula.Parse( value, cellReferenceMode, CultureInfo.CurrentCulture );
		}

		#endregion Parse( string, CellReferenceMode )

		#region Parse( string, CellReferenceMode, CultureInfo )

		/// <summary>
		/// Parses the specified formula value and returns the array formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
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
		/// <returns>An <see cref="ArrayFormula"/> instance which represents the array formula value specified.</returns>
		public new static ArrayFormula Parse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture )
		{
			// MD 7/9/08 - Excel 2007 Format
			// Moved all code to the new overload
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return ArrayFormula.Parse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture );
			return ArrayFormula.Parse(value, cellReferenceMode, Workbook.LatestFormat, culture);
		}

		#endregion Parse( string, CellReferenceMode, CultureInfo )

		// MD 7/9/08 - Excel 2007 Format
		#region Parse( string, CellReferenceMode, WorkbookFormat )

		/// <summary>
		/// Parses the specified formula value and returns the array formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the array formula. This will be used to determine certain limits which are format dependant.</param>
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
		/// <returns>An <see cref="ArrayFormula"/> instance which represents the array formula value specified.</returns>
		public new static ArrayFormula Parse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat )
		{
			return ArrayFormula.Parse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture );
		}

		#endregion Parse( string, CellReferenceMode, WorkbookFormat )

		#region Parse( string, CellReferenceMode, WorkbookFormat, CultureInfo )

		/// <summary>
		/// Parses the specified formula value and returns the array formula which was created from it.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the array formula. This will be used to determine certain limits which are format dependant.</param>
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
		/// <returns>An <see cref="ArrayFormula"/> instance which represents the array formula value specified.</returns>
		public new static ArrayFormula Parse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture )
		{
			ArrayFormula formula;
			FormulaParseException exc;

			// MD 7/9/08 - Excel 2007 Format
			//if ( ArrayFormula.TryParse( value, cellReferenceMode, out formula, out exc ) )
			if ( ArrayFormula.TryParse( value, cellReferenceMode, fileFormat, culture, out formula, out exc ) )
			{
				Debug.Assert( formula != null );
				return formula;
			}

			Debug.Assert( exc != null );
			throw exc;
		}

		#endregion Parse( string, CellReferenceMode, WorkbookFormat, CultureInfo )

		#region TryParse ( string, CellReferenceMode, out ArrayFormula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, out ArrayFormula formula )
		{
			return ArrayFormula.TryParse( value, cellReferenceMode, CultureInfo.CurrentCulture, out formula );
		}

		#endregion TryParse ( string, CellReferenceMode, out ArrayFormula )

		#region TryParse ( string, CellReferenceMode, CultureInfo, out ArrayFormula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>,
		/// if the conversion succeeded, or null if the conversion failed. This parameter is passed uninitialized.
		/// </param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="cellReferenceMode"/> is not defined in the <see cref="CellReferenceMode"/> enumeration.
		/// </exception>
		/// <returns>True if <paramref name="value"/> was converted successfully; False otherwise.</returns>
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture, out ArrayFormula formula )
		{
			// MD 7/9/08 - Excel 2007 Format
			// Call off to the new overload
			//FormulaParseException exc;
			//return ArrayFormula.TryParse( value, cellReferenceMode, out formula, out exc );
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return ArrayFormula.TryParse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture, out formula );
			return ArrayFormula.TryParse(value, cellReferenceMode, Workbook.LatestFormat, culture, out formula);
		}

		#endregion TryParse ( string, CellReferenceMode, CultureInfo, out ArrayFormula )

		#region TryParse ( string, CellReferenceMode, out ArrayFormula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, out ArrayFormula formula, out FormulaParseException exception )
		{
			return ArrayFormula.TryParse( value, cellReferenceMode, CultureInfo.CurrentCulture, out formula, out exception );
		}

		#endregion TryParse ( string, CellReferenceMode, out ArrayFormula, out FormulaParseException )

		#region TryParse ( string, CellReferenceMode, CultureInfo, out ArrayFormula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, CultureInfo culture, out ArrayFormula formula, out FormulaParseException exception )
		{
			// MD 7/9/08 - Excel 2007 Format
			// Moved all code to the new overload
			// MD 2/24/12
			// Found while implementing 12.1 - Table Support
			// We should use the least restrictive format version when there is no workbook, not the most.
			//return ArrayFormula.TryParse( value, cellReferenceMode, WorkbookFormat.Excel97To2003, culture, out formula, out exception );
			return ArrayFormula.TryParse(value, cellReferenceMode, Workbook.LatestFormat, culture, out formula, out exception);
		}

		#endregion TryParse ( string, CellReferenceMode, CultureInfo, out ArrayFormula, out FormulaParseException )

		// MD 7/9/08 - Excel 2007 Format
		#region TryParse ( string, CellReferenceMode, WorkbookFormat, out ArrayFormula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>,
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, out ArrayFormula formula )
		{
			return ArrayFormula.TryParse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture, out formula );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, out ArrayFormula )

		#region TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out ArrayFormula )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>,
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture, out ArrayFormula formula )
		{
			FormulaParseException exc;
			return ArrayFormula.TryParse( value, cellReferenceMode, fileFormat, culture, out formula, out exc );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out ArrayFormula )

		// MD 7/9/08 - Excel 2007 Format
		#region TryParse ( string, CellReferenceMode, WorkbookFormat, out ArrayFormula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, out ArrayFormula formula, out FormulaParseException exception )
		{
			return ArrayFormula.TryParse( value, cellReferenceMode, fileFormat, CultureInfo.CurrentCulture, out formula, out exception );
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, out ArrayFormula, out FormulaParseException )

		#region TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out ArrayFormula, out FormulaParseException )

		/// <summary>
		/// Parses the specified formula value. The return value indicates whether the operation succeeded.
		/// </summary>
		/// <param name="value">The string which defines the array formula to parse.</param>
		/// <param name="cellReferenceMode">The mode used to interpret cell references in the array formula.</param>
		/// <param name="fileFormat">The file format to use when parsing the formula. This will be used to determine certain limits which are format dependant.</param>
		/// <param name="culture">The culture used to parse the formula.</param>
		/// <param name="formula">
		/// When this method returns, contains the array formula which was parsed from <paramref name="value"/>
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
		public static bool TryParse( string value, CellReferenceMode cellReferenceMode, WorkbookFormat fileFormat, CultureInfo culture, out ArrayFormula formula, out FormulaParseException exception )
		{
			Formula baseFormula;

			// MD 7/9/08 - Excel 2007 Format
			//bool returnValue = Formula.TryParse( value, cellReferenceMode, FormulaType.ArrayFormula, out baseFormula, out exception );
			// MD 2/23/12 - TFS101504
			// Pass along null for the new indexedReferencesDuringLoad parameter.
			//bool returnValue = Formula.TryParse( value, cellReferenceMode, FormulaType.ArrayFormula, fileFormat, culture, out baseFormula, out exception );
			bool returnValue = Formula.TryParse(value, cellReferenceMode, FormulaType.ArrayFormula, fileFormat, culture, null, out baseFormula, out exception);

			ArrayFormula array = baseFormula as ArrayFormula;

			Debug.Assert( baseFormula == null || array != null );
			formula = array;

			return returnValue;
		}

		#endregion TryParse ( string, CellReferenceMode, WorkbookFormat, CultureInfo, out ArrayFormula, out FormulaParseException )

		#endregion Static Methods

		#region Internal Methods

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnAfterShift






		internal void OnAfterShift()
		{
			Debug.Assert(this.cachedRegionWhileShifting != null, "This is unexpected.");

			// Before the cell shift operation, we temporarily removed the array formula from the region so value shifting
			// would just be shifting nulls for the array formula and we wouldn't have to worry about the top-left cell
			// being moved last so other cells could continue to reference it.
			if (this.cachedRegionWhileShifting != null)
			{
				this.ApplyTo(this.cachedRegionWhileShifting);
				this.cachedRegionWhileShifting = null;
			}
		}

		#endregion // OnAfterShift

		// MD 7/19/12 - TFS116808 (Table resizing)
		#region OnBeforeShift






		internal void OnBeforeShift(CellShiftOperation shiftOperation)
		{
			Debug.Assert(
				this.cellRange != null && shiftOperation.RegionAddressBeforeShift.Contains(this.cellRange.Address),
				"This is unexpected.");

			// While we are shifting cell values, we don't want to have to worry about the top-left cell being moved last so 
			// other cells could continue to reference it, because it actually isn't possible in some situations, so temporarily
			// remove the array formula from the shifting region.
			if (this.cellRange != null && this.cellRange.Worksheet != null)
			{
				this.cachedRegionWhileShifting = this.cellRange;
				this.ClearCellRange();
			}
		}

		#endregion // OnBeforeShift

		#endregion // Internal Methods

		#region Private Methods

		#region ApplyToRegionHelper

		private void ApplyToRegionHelper(WorksheetRegion region)
		{
			if (region.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionsShiftedOffWorksheet"), "regions");

			WorksheetCellBlock.VerifyRegionForBlockingValue(this, region, region);

			region.Worksheet.VerifyFormula(this, region.TopRow, region.FirstColumnInternal);

			this.cellRange = region;
			this.calculatedValues = new object[region.Width, region.Height];

			this.SetOwningCell(region.TopRow, region.FirstColumnInternal);
			WorksheetCellBlock.ApplyBlockingValue(this);
		}

		#endregion // ApplyToRegionHelper

		#endregion // Private Methods

		#endregion Methods

		#region Properties

		#region CellRange

		/// <summary>
		/// Gets the cells to which the array formula is applied.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is null, the formula has not yet been applied.
		/// </p>
		/// </remarks>
		/// <value>The cells to which the array formula is applied.</value>
		public WorksheetRegion CellRange
		{
			get { return this.cellRange; }
		}

		#endregion CellRange

		#region InteriorCellFormula






		internal Formula InteriorCellFormula
		{
			get
			{
				if ( this.cellRange != null && this.interiorCellFormula == null )
					this.interiorCellFormula = new Formula( this );

				return this.interiorCellFormula;
			}
		}

		#endregion InteriorCellFormula

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