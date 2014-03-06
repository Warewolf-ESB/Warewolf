using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.FormulaUtilities;
using Infragistics.Documents.Excel.FormulaUtilities.CalcEngine;





using System.Drawing;
using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents a rectangular region of cells on a worksheet.
	/// </summary>



	public

		 class WorksheetRegion : 
		IEnumerable<WorksheetCell>		// MD 7/14/08 - Excel formula solving
	{
		#region Member Variables

		// MD 7/14/08 - Excel formula solving
		// MD 4/12/11
		// Found while fixing TFS67084
		// We can do better caching if this is the base type.
		//private RegionCalcReference calcReference;
		private ExcelRefBase calcReference;

		// MD 3/13/12 - 12.1 - Table Support
		// Moved these members to the WorksheetRegionAddress struct.
		#region Moved

		//// MD 4/12/11
		//// Found while fixing TFS67084
		//// We only need 2 bytes to store the column indexes.
		////private int firstColumn;
		//private short firstColumn;

		//private int firstRow;

		//// MD 4/12/11
		//// Found while fixing TFS67084
		//// We only need 2 bytes to store the column indexes.
		////private int lastColumn;
		//private short lastColumn;

		//private int lastRow;

		#endregion // Moved
		private WorksheetRegionAddress regionAddress;

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//// MD 8/21/08 - Excel formula solving - Performance
		//private WorksheetCell topLeftCell;
		private WorksheetRow topRow;

		private Worksheet worksheet;

		#endregion Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="WorksheetRegion"/> class.
		/// </summary>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="worksheet"/> is null.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// <paramref name="firstRow"/> is greater than <paramref name="lastRow"/> or 
		/// <paramref name="firstColumn"/> is greater than <paramref name="lastColumn"/>.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Any row or column indices specified are outside the valid row or column ranges.
		/// </exception>
		/// <param name="worksheet">The worksheet on which the region resides.</param>
		/// <param name="firstRow">The index of the first row of the region.</param>
		/// <param name="firstColumn">The index of the first column of the region.</param>
		/// <param name="lastRow">The index of the last row of the region.</param>
		/// <param name="lastColumn">The index of the last row column of the region.</param>
		public WorksheetRegion( Worksheet worksheet, int firstRow, int firstColumn, int lastRow, int lastColumn )
			// MD 8/21/08 - Excel formula solving
			// Moved all code to the new overload.
			: this( worksheet, firstRow, firstColumn, lastRow, lastColumn, true ) { }

		// MD 8/21/08 - Excel formula solving
		// Created new constructor to specify whether to cache the region on the worksheet
		internal WorksheetRegion( Worksheet worksheet, int firstRow, int firstColumn, int lastRow, int lastColumn, bool addCachedRegion )
		{
			if ( worksheet == null )
				throw new ArgumentNullException( "worksheet" );

			WorksheetRegion.VerifyRowOrder( firstRow, lastRow );
			WorksheetRegion.VerifyColumnOrder( firstColumn, lastColumn );

			// MD 6/31/08 - Excel 2007 Format
			//Utilities.VerifyRowIndex( firstRow, "firstRow" );
			//Utilities.VerifyRowIndex( lastRow, "lastRow" );
			//Utilities.VerifyColumnIndex( firstColumn, "firstColumn" );
			//Utilities.VerifyColumnIndex( lastColumn, "lastColumn" );
			// MD 10/22/10 - TFS36696
			// Cache the workbook so we don't have to get it from the worksheet on each call.
			//Utilities.VerifyRowIndex( worksheet.Workbook, firstRow, "firstRow" );
			//Utilities.VerifyRowIndex( worksheet.Workbook, lastRow, "lastRow" );
			//Utilities.VerifyColumnIndex( worksheet.Workbook, firstColumn, "firstColumn" );
			//Utilities.VerifyColumnIndex( worksheet.Workbook, lastColumn, "lastColumn" );
			// MD 2/24/12 - 12.1 - Table Support
			// The workbook may be null.
			//Workbook workbook = worksheet.Workbook;
			//Utilities.VerifyRowIndex(workbook, firstRow, "firstRow");
			//Utilities.VerifyRowIndex(workbook, lastRow, "lastRow");
			//Utilities.VerifyColumnIndex(workbook, firstColumn, "firstColumn");
			//Utilities.VerifyColumnIndex(workbook, lastColumn, "lastColumn");
			Utilities.VerifyRowIndex(worksheet, firstRow, "firstRow");
			Utilities.VerifyRowIndex(worksheet, lastRow, "lastRow");
			Utilities.VerifyColumnIndex(worksheet, firstColumn, "firstColumn");
			Utilities.VerifyColumnIndex(worksheet, lastColumn, "lastColumn");


			this.worksheet = worksheet;

			// MD 3/13/12 - 12.1 - Table Support
			#region Old Code

			//this.firstRow = firstRow;

			//// MD 4/12/11
			//// Found while fixing TFS67084
			//// We only need 2 bytes to store the column indexes.
			////this.firstColumn = firstColumn;
			//this.firstColumn = (short)firstColumn;

			//this.lastRow = lastRow;

			//// MD 4/12/11
			//// Found while fixing TFS67084
			//// We only need 2 bytes to store the column indexes.
			////this.lastColumn = lastColumn;
			//this.lastColumn = (short)lastColumn;

			#endregion // Old Code
			this.regionAddress = new WorksheetRegionAddress(firstRow, lastRow, (short)firstColumn, (short)lastColumn);

			// MD 8/21/08 - Excel formula solving
			if ( addCachedRegion )
			{
				WorksheetRegion foundRegion;
				this.worksheet.AddCachedRegion( this, out foundRegion );
			}
		}

		#endregion Constructor

		#region Base Class Overrides

		// MD 7/14/08 - Excel formula solving
		#region Equals

		/// <summary>
		/// Determines whether the specified value equals this <see cref="WorksheetRegion"/>.
		/// </summary>
		/// <param name="obj">The value to test for equality.</param>
		public override bool Equals( object obj )
		{
			// MD 3/12/12 - 12.1 - Table Support
			if (Object.ReferenceEquals(this, obj))
				return true;

			WorksheetRegion region = obj as WorksheetRegion;

			if ( region == null )
				return false;

			if ( this.worksheet != region.worksheet )
				return false;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.worksheet == null)
				return true;

			// MD 3/13/12 - 12.1 - Table Support
			#region Old Code

			//if ( this.firstRow != region.firstRow )
			//    return false;

			//if ( this.firstColumn != region.firstColumn )
			//    return false;

			//if ( this.lastRow != region.lastRow )
			//    return false;

			//if ( this.lastColumn != region.lastColumn )
			//    return false;

			#endregion // Old Code
			return this.regionAddress == region.regionAddress;
		} 

		#endregion Equals

		// MD 7/14/08 - Excel formula solving
		#region GetHashCode

		/// <summary>
		/// Gtes the hash code for the <see cref="WorksheetRegion"/>.
		/// </summary>
		public override int GetHashCode()
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.worksheet == null)
				return 0;

			int worksheetHashCode = this.worksheet.GetHashCode();

			// MD 3/13/12 - 12.1 - Table Support
			//worksheetHashCode ^= this.firstRow;
			//worksheetHashCode ^= this.firstColumn	<< 1;
			//worksheetHashCode ^= this.lastRow		<< 2;
			//worksheetHashCode ^= this.lastColumn	<< 3;
			worksheetHashCode ^= this.regionAddress.GetHashCode();

			return worksheetHashCode;
		} 

		#endregion GetHashCode

		#region ToString

		/// <summary>
		/// Gets the string representation of the range of cells in the region.
		/// </summary>
		/// <returns>The string representation of the range of cells in the region.</returns>
		public override string ToString()
		{
			CellReferenceMode cellReferenceMode = CellReferenceMode.A1;

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			//Workbook workbook = this.worksheet.Workbook;
			//
			//if ( workbook != null )
			//    cellReferenceMode = workbook.CellReferenceMode;
			if (this.worksheet != null)
				cellReferenceMode = this.worksheet.CellReferenceMode;

			return this.ToString( cellReferenceMode, true );
		}

		#endregion ToString

		#endregion Base Class Overrides

		#region Interfaces

		// MD 7/14/08 - Excel formula solving
		#region IEnumerable<WorksheetCell> Members

		IEnumerator<WorksheetCell> IEnumerable<WorksheetCell>.GetEnumerator()
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.worksheet == null)
				yield break;

			// MD 3/13/12 - 12.1 - Table Support
			//for ( int rowIndex = this.firstRow; rowIndex <= this.lastRow; rowIndex++ )
			for (int rowIndex = this.regionAddress.FirstRowIndex; rowIndex <= this.regionAddress.LastRowIndex; rowIndex++)
			{
				WorksheetRow row = this.worksheet.Rows[ rowIndex ];

				// MD 3/13/12 - 12.1 - Table Support
				//for ( int columnIndex = this.firstColumn; columnIndex <= this.lastColumn; columnIndex++ )
				for (int columnIndex = this.regionAddress.FirstColumnIndex; columnIndex <= this.regionAddress.LastColumnIndex; columnIndex++)
					yield return row.Cells[ columnIndex ];
			}
		}

		#endregion

		// MD 7/14/08 - Excel formula solving
		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return ( (IEnumerable<WorksheetCell>)this ).GetEnumerator();
		}

		#endregion

		#endregion Interfaces

		#region Methods

		#region Public Methods

		// MD 5/21/07 - BR23024
		#region ApplyArrayFormula

		/// <summary>
		/// Applies a array formula to the region of cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="value"/> is parsed based on the <see cref="CellReferenceMode"/> of the <see cref="Workbook"/>
		/// to which the region belongs. If the region's <see cref="Worksheet"/> has been removed from its parent collection,
		/// the A1 CellReferenceMode will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="value">The array formula to parse and apply to the region.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains another array formula or data table which extends outside the region.
		/// </exception>
		/// <seealso cref="ArrayFormula"/>
		public void ApplyArrayFormula( string value )
		{
			// MD 2/29/12 - 12.1 - Table Support
			this.VerifyRegion();

			// MD 7/9/08 - Excel 2007 Format
			// Moved this code into the static ArrayFormula.Parse method.
			#region Moved

			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			#endregion Moved
			ArrayFormula parsedFormula = ArrayFormula.Parse( value, this.worksheet.Workbook );

			parsedFormula.ApplyTo( this );
		}

		#endregion ApplyArrayFormula

		// MD 5/21/07 - BR23024
		#region ApplyFormula

		/// <summary>
		/// Applies a formula to the region of cells.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="value"/> is parsed based on the <see cref="CellReferenceMode"/> of the <see cref="Workbook"/>
		/// to which the region belongs. If the region's <see cref="Worksheet"/> has been removed from its parent collection,
		/// the A1 CellReferenceMode will be used to parse the formula.
		/// </p>
		/// </remarks>
		/// <param name="value">The formula to parse and apply to the region.</param>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="value"/> is null or empty.
		/// </exception>
		/// <exception cref="FormulaParseException">
		/// <paramref name="value"/> is not a valid formula.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains an array formula or data table which extends outside the region.
		/// </exception>
		/// <seealso cref="Formula"/>
		public void ApplyFormula( string value )
		{
			// MD 2/29/12 - 12.1 - Table Support
			this.VerifyRegion();

			// MD 7/9/08 - Excel 2007 Format
			// Moved this code into the static Formula.Parse method.
			#region Moved

			
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			#endregion Moved
			// MD 8/26/08 - Excel formula solving - Performance
			//Formula parsedFormula = Formula.Parse( value, this.worksheet.Workbook );
			Workbook workbook = this.Worksheet.Workbook;
			bool addToParsedR1C1Formulas = false;

			if ( workbook != null && workbook.CellReferenceMode == CellReferenceMode.R1C1 )
			{
				Formula existingFormula;
				if ( workbook.ParsedR1C1Formulas.TryGetValue( value, out existingFormula ) )
				{
					existingFormula.ApplyTo( this );
					return;
				}

				addToParsedR1C1Formulas = true;
			}

			Formula parsedFormula = Formula.Parse( value, workbook );

			if ( addToParsedR1C1Formulas )
				workbook.ParsedR1C1Formulas.Add( value, parsedFormula );

			parsedFormula.ApplyTo( this );
		}

		#endregion ApplyFormula

		// MD 12/14/11 - 12.1 - Table Support
		
		#region FormatAsTable

		/// <summary>
		/// Formats the region as a table and adds an associated <see cref="WorksheetTable"/> to the <see cref="Excel.Worksheet.Tables"/> 
		/// collection.
		/// </summary>
		/// <param name="tableHasHeaders">
		/// A value which indicates whether the top row of the region contains the headers for the table.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// When the table is created, the <see cref="Workbook.DefaultTableStyle"/> will be applied to the <seealso cref="WorksheetTable.Style"/> 
		/// value.
		/// </p>
		/// <p class="body">
		/// When the table is created, the column names will be taken from the cells in the header row if <paramref name="tableHasHeaders"/> 
		/// is True. If it is False, the column names will be generated and the cells for the header row will be inserted into the worksheet.
		/// </p>
		/// <p class="body">
		/// The column names are unique within the owning WorksheetTable. If, when the table is created, there are two or more columns with 
		/// the same name, the second and subsequent duplicate column names will have a number appended to make them unique. If any cells in 
		/// the header row have a non-string value, their value will be changed to a string (the current display text of the cell). If any 
		/// cells in the header row have no value, they will be given a generated column name.
		/// </p>
		/// <p class="body">
		/// If the region partially contains any merged cell regions, they will be removed from the worksheet and the table region will be expanded 
		/// to include all cells from the merged region.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells from another <see cref="WorksheetTable"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which have a multi-cell <see cref="ArrayFormula"/> applied.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which are part of a <see cref="WorksheetDataTable"/>.
		/// </exception>
		/// <returns>The <see cref="WorksheetTable"/> created the represent the formatted table for the region.</returns>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>
		/// <seealso cref="WorksheetTableCollection.Add(string,bool)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTable FormatAsTable(bool tableHasHeaders)
		{
			return this.FormatAsTable(tableHasHeaders, null);
		}

		/// <summary>
		/// Formats the region as a table and adds an associated <see cref="WorksheetTable"/> to the <see cref="Excel.Worksheet.Tables"/> 
		/// collection.
		/// </summary>
		/// <param name="tableHasHeaders">
		/// A value which indicates whether the top row of the region contains the headers for the table.
		/// </param>
		/// <param name="tableStyle">
		/// The <see cref="WorksheetTableStyle"/> to apply to the table or null to use the <see cref="Workbook.DefaultTableStyle"/>.
		/// </param>
		/// <remarks>
		/// <p class="body">
		/// When the table is created, the specified <paramref name="tableStyle"/> will be applied to the <seealso cref="WorksheetTable.Style"/> 
		/// value.
		/// </p>
		/// <p class="body">
		/// When the table is created, the column names will be taken from the cells in the header row if <paramref name="tableHasHeaders"/> 
		/// is True. If it is False, the column names will be generated and the cells for the header row will be inserted into the worksheet.
		/// </p>
		/// <p class="body">
		/// The column names are unique within the owning WorksheetTable. If, when the table is created, there are two or more columns with 
		/// the same name, the second and subsequent duplicate column names will have a number appended to make them unique. If any cells in 
		/// the header row have a non-string value, their value will be changed to a string (the current display text of the cell). If any 
		/// cells in the header row have no value, they will be given a generated column name.
		/// </p>
		/// <p class="body">
		/// If the region partially contains any merged cell regions, they will be removed from the worksheet and the table region will be expanded 
		/// to include all cells from the merged region.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The specified <paramref name="tableStyle"/> does not exist in the <see cref="Workbook.CustomTableStyles"/> or 
		/// <see cref="Workbook.StandardTableStyles"/> collections.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells from another <see cref="WorksheetTable"/>.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which have a multi-cell <see cref="ArrayFormula"/> applied.
		/// </exception>
		/// <exception cref="InvalidOperationException">
		/// The region contains one or more cells which are part of a <see cref="WorksheetDataTable"/>.
		/// </exception>
		/// <returns>The <see cref="WorksheetTable"/> created the represent the formatted table for the region.</returns>
		/// <seealso cref="WorksheetTable"/>
		/// <seealso cref="Excel.Worksheet.Tables"/>
		/// <seealso cref="WorksheetTableColumn.Name"/>
		/// <seealso cref="Workbook.CustomTableStyles"/>
		/// <seealso cref="Workbook.StandardTableStyles"/>
		/// <seealso cref="WorksheetTable.Style"/>
		/// <seealso cref="WorksheetTable.IsHeaderRowVisible"/>
		/// <seealso cref="WorksheetTableCollection.Add(string,bool,WorksheetTableStyle)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]

		public WorksheetTable FormatAsTable(bool tableHasHeaders, WorksheetTableStyle tableStyle)
		{
			this.VerifyRegion();
			return this.Worksheet.Tables.Add(this, tableHasHeaders, tableStyle);
		}

		#endregion // FormatAsTable

		#region GetBoundsInTwips

		/// <summary>
		/// Gets the bounds of the region in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the region are resized, these bounds will no longer reflect the 
		/// position of the region.
		/// </p>
		/// </remarks>
		/// <returns>The bounds of the region on its worksheet.</returns>
		public Rectangle GetBoundsInTwips()
		{
			// MD 3/24/10 - TFS28374
			// Call of to new overload.
			//return WorksheetShape.GetBoundsInTwips(
			//    this.worksheet.Rows[ this.firstRow ].Cells[ this.firstColumn ], 
			//    Utilities.PointFEmpty,
			//    this.worksheet.Rows[ this.lastRow ].Cells[ this.lastColumn ], 
			//    new PointF( 100, 100 ) );
			return this.GetBoundsInTwips(PositioningOptions.None);
		}

		// MD 3/24/10 - TFS28374
		// Added new overload.
		/// <summary>
		/// Gets the bounds of the region in twips (1/20th of a point).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The bounds returned by this method are only valid with the current configuration of the worksheet.
		/// If any rows or columns before the region are resized, these bounds will no longer reflect the 
		/// position of the region.
		/// </p>
		/// </remarks>
		/// <param name="options">The options to use when getting the bounds of the region.</param>
		/// <returns>The bounds of the region on its worksheet.</returns>
		public Rectangle GetBoundsInTwips(PositioningOptions options)
		{
			// MD 2/29/12 - 12.1 - Table Support
			this.VerifyRegion();

			// MD 4/12/11 - TFS67084
			// Moved away from using WorksheetCell objects.
			//return WorksheetShape.GetBoundsInTwips(
			//    this.worksheet.Rows[this.firstRow].Cells[this.firstColumn],
			//    Utilities.PointFEmpty,
			//    this.worksheet.Rows[this.lastRow].Cells[this.lastColumn],
			//    new PointF(100, 100),
			//    options);
			// MD 3/27/12 - 12.1 - Table Support
			//return WorksheetShape.GetBoundsInTwips(
			//    // MD 3/13/12 - 12.1 - Table Support
			//    //this.worksheet.Rows[this.firstRow], this.firstColumn,
			//    this.worksheet.Rows[this.regionAddress.FirstRowIndex], this.regionAddress.FirstColumnIndex,
			//    Utilities.PointFEmpty,
			//    // MD 3/13/12 - 12.1 - Table Support
			//    //this.worksheet.Rows[this.lastRow], this.lastColumn,
			//    this.worksheet.Rows[this.regionAddress.LastRowIndex], this.regionAddress.LastColumnIndex,
			//    new PointF(100, 100),
			//    options);
			return WorksheetShape.GetBoundsInTwips(
				this.worksheet,
				this.regionAddress.FirstRowIndex, this.regionAddress.FirstColumnIndex,
				Utilities.PointFEmpty,
				this.regionAddress.LastRowIndex, this.regionAddress.LastColumnIndex,
				new PointF(100, 100),
				options);
		}

		#endregion GetBoundsInTwips

		#region ToString( CellReferenceMode, bool )

		/// <summary>
		/// Gets the string representation of the range of cells in the region.
		/// </summary>
		/// <param name="cellReferenceMode">The mode used to generate cell references.</param>
		/// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the range address.</param>
		/// <returns>The string representation of the range of cells in the region.</returns>
		public string ToString( CellReferenceMode cellReferenceMode, bool includeWorksheetName )
		{
			// MD 7/16/08 - Excel formula solving
			// Moved all code to new overload
			return this.ToString( cellReferenceMode, includeWorksheetName, false, false );
		}

		#endregion ToString( CellReferenceMode, bool )

		// MD 7/16/08 - Excel formula solving
		#region ToString( CellReferenceMode, bool, bool, bool )

		/// <summary>
		/// Gets the string representation of the range of cells in the region.
		/// </summary>
		/// <param name="cellReferenceMode">The mode used to generate cell references.</param>
		/// <param name="includeWorksheetName">The value indicating whether to include the worksheet name in the range address.</param>
		/// <param name="useRelativeColumn">The value indicating whether to use a relative column address for the cells in the range.</param>
		/// <param name="useRelativeRow">The value indicating whether to use a relative row address for the cells in the range.</param>
		/// <returns>The string representation of the range of cells in the region.</returns>
		public string ToString( CellReferenceMode cellReferenceMode, bool includeWorksheetName, bool useRelativeColumn, bool useRelativeRow )
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
				return ErrorValue.InvalidCellReference.ToString();

			// MD 3/13/12 - 12.1 - Table Support
			#region Old Code

			//// MD 4/12/11 - TFS67084
			//// Moved away from using WorksheetCell objects.
			////IWorksheetCell sourceCellForRelatives = this.TopLeftCell;
			//// MD 2/20/12 - 12.1 - Table Support
			////WorksheetRow sourceRowForRelatives = this.TopRow;
			//int sourceRowIndexForRelatives = this.FirstRow;
			//short sourceColumnIndexForRelatives = this.FirstColumnInternal;

			//return
			//    ( includeWorksheetName ? Utilities.CreateReferenceString( null, this.worksheet.Name ) : string.Empty ) +
			//    // MD 6/31/08 - Excel 2007 Format
			//    //CellAddress.GetCellReferenceString( this.firstRow, this.firstColumn, null, cellReferenceMode ) +
			//    CellAddress.GetCellReferenceString( 
			//        this.firstRow, this.firstColumn,
			//        useRelativeRow, useRelativeColumn,
			//    // MD 4/12/11 - TFS67084
			//    // Moved away from using WorksheetCell objects.
			//        //this.worksheet.CurrentFormat, sourceCellForRelatives, false, cellReferenceMode ) +
			//        // MD 2/20/12 - 12.1 - Table Support
			//        //this.worksheet.CurrentFormat, sourceRowForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode) +
			//        this.worksheet.CurrentFormat, sourceRowIndexForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode) +
			//    FormulaParser.RangeOperator +
			//    // MD 6/31/08 - Excel 2007 Format
			//    //CellAddress.GetCellReferenceString( this.lastRow, this.lastColumn, null, cellReferenceMode );
			//    CellAddress.GetCellReferenceString( 
			//        this.lastRow, this.lastColumn,
			//        useRelativeRow, useRelativeColumn,
			//    // MD 4/12/11 - TFS67084
			//    // Moved away from using WorksheetCell objects.
			//        //this.worksheet.CurrentFormat, sourceCellForRelatives, false, cellReferenceMode );
			//        // MD 2/20/12 - 12.1 - Table Support
			//        //this.worksheet.CurrentFormat, sourceRowForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode);
			//        this.worksheet.CurrentFormat, sourceRowIndexForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode);

			#endregion // Old Code
			return String.Format("{0}{1}",
				(includeWorksheetName ? Utilities.CreateReferenceString(null, this.worksheet.Name) : string.Empty),
				this.regionAddress.ToString(useRelativeRow, useRelativeColumn, this.worksheet.CurrentFormat, cellReferenceMode));
		}

		#endregion ToString( CellReferenceMode, bool, bool, bool )

		#endregion Public Methods

		#region Internal Methods

		#region ApplyFormulaToRegion

		// MD 4/12/11 - TFS67084
		// Moved away from using WorksheetCell objects.
		//internal virtual void ApplyFormulaToRegion( Formula formula, ref IWorksheetCell firstAppliedToCell )
		internal virtual void ApplyFormulaToRegion(Formula formula, ref WorksheetRow firstAppliedToRow, ref short firstAppliedToColumnIndex)
		{
			for ( int rowIndex = this.FirstRow; rowIndex <= this.LastRow; rowIndex++ )
			{
				WorksheetRow row = this.worksheet.Rows[ rowIndex ];

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects.
				//for ( int columnIndex = this.FirstColumn; columnIndex <= this.LastColumn; columnIndex++ )
				//{
				//    WorksheetCell cell = row.Cells[ columnIndex ];
				//
				//    if ( firstAppliedToCell == null )
				//        firstAppliedToCell = cell;
				//
				//    formula.ApplyTo( firstAppliedToCell, cell );
				//}
				for (short columnIndex = this.FirstColumnInternal; columnIndex <= this.LastColumnInternal; columnIndex++)
				{
					if (firstAppliedToRow == null)
					{
						firstAppliedToRow = row;
						firstAppliedToColumnIndex = columnIndex;
					}

					formula.ApplyTo(firstAppliedToRow, firstAppliedToColumnIndex, row, columnIndex);
				}
			}
		} 

		#endregion ApplyFormulaToRegion

		// MD 4/12/11 - TFS67084
		// This is no longer used.
		#region Removed

		//// MD 7/21/08 - Excel formula solving
		//#region Contains( WorksheetCell )

		//internal bool Contains( WorksheetCell cell )
		//{
		//    if ( cell.Worksheet != this.Worksheet )
		//        return false;

		//    // MD 7/26/10 - TFS34398
		//    // Now that the row is stored on the cell, the RowIndex getter is now a bit slower, so cache it.
		//    //if ( cell.RowIndex < this.firstRow || this.lastRow < cell.RowIndex ||
		//    //    cell.ColumnIndex < this.firstColumn || this.lastColumn < cell.ColumnIndex )
		//    int columnIndex = cell.ColumnIndex;
		//    int rowIndex = cell.RowIndex;
		//    if (rowIndex < this.firstRow || this.lastRow < rowIndex ||
		//        columnIndex < this.firstColumn || this.lastColumn < columnIndex)
		//    {
		//        return false;
		//    }

		//    return true;
		//} 

		//#endregion Contains( WorksheetCell ) 

		#endregion  // Removed

		// MD 5/13/11 - Data Validations / Page Breaks
		#region Contains ( WorksheetCell )

		internal bool Contains(WorksheetCell cell)
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			//return this.Contains(cell.Row, cell.ColumnIndexInternal);
			WorksheetRow row = cell.Row;
			if (row == null)
				return false;

			return this.Contains(row, cell.ColumnIndexInternal);
		}

		#endregion  // Contains ( WorksheetCell )

		#region Contains( WorksheetRegion )






		internal bool Contains( WorksheetRegion region )
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
				return false;

			// MD 7/21/08
			// Found while implementing Excel formula solving
			// The specified region could be on a different worksheet and in that case, it should never be contained in this region.
			if ( region.Worksheet != this.Worksheet )
				return false;

			// MD 3/13/12 - 12.1 - Table Support
			//if ( region.firstRow < this.firstRow ||
			//    region.firstColumn < this.firstColumn ||
			//    this.lastRow < region.lastRow ||
			//    this.lastColumn < region.lastColumn )
			//{
			//    return false;
			//}
			//
			//return true;
			return this.regionAddress.Contains(region.regionAddress);
		}

		#endregion Contains( WorksheetRegion )

		// MD 4/12/11 - TFS67084
		#region Contains( WorksheetRow, short )

		internal bool Contains(WorksheetRow row, short columnIndex)
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
				return false;

			if (row.Worksheet != this.Worksheet)
				return false;

			// MD 3/13/12 - 12.1 - Table Support
			//return this.Contains(row.Index, columnIndex);
			return this.regionAddress.Contains(row.Index, columnIndex);
		}

		#endregion  // Contains( int, int )

		// MD 4/12/11 - TFS67084
		// This is no longer used.
		#region Removed

		//        // MD 7/14/08 - Excel formula solving
		//        #region GetCell

		//#if DEBUG
		//        /// <summary>
		//        /// Gets the cell in the region based on indices relative t the top-left cell of the region.
		//        /// </summary> 
		//#endif
		//        internal WorksheetCell GetCell( int columnInRegion, int rowInRegion )
		//        {
		//            return this.worksheet.Rows[ rowInRegion + this.firstRow ].Cells[ columnInRegion + this.firstColumn ];
		//        } 

		//        #endregion GetCell 

		#endregion  // Removed

		// MD 4/12/11 - TFS67084
		// This is no longer used.
		#region Removed

		//// MD 9/2/08 - Excel formula solving
		//#region GetCreatedCells
		//
		//internal IEnumerable<WorksheetCell> GetCreatedCells()
		//{
		//    for ( int rowIndex = this.firstRow; rowIndex <= this.lastRow; rowIndex++ )
		//    {
		//        WorksheetRow row = this.worksheet.Rows.GetIfCreated( rowIndex );
		//
		//        if ( row == null )
		//            continue;
		//
		//        for ( int columnIndex = this.firstColumn; columnIndex <= this.lastColumn; columnIndex++ )
		//        {
		//            WorksheetCell cell = row.Cells.GetIfCreated( columnIndex );
		//
		//            if ( cell == null )
		//                continue;
		//
		//            yield return cell;
		//        }
		//    }
		//}
		//
		//#endregion GetCreatedCells

		#endregion  // Removed

		// MD 8/21/08 - Excel formula solving
		#region IntersectsWith( WorksheetRegion )

		internal bool IntersectsWith( WorksheetRegion region )
		{
			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (this.Worksheet == null)
				return false;

			// MD 5/13/11 - Data Validations / Page Breaks
			// Make sure the region is in the same worksheet.
			if (this.Worksheet != region.Worksheet)
				return false;

			// MD 3/13/12 - 12.1 - Table Support
			//return this.IntersectsWith( region.FirstRow, region.FirstColumn, region.LastRow, region.LastColumn );
			return this.regionAddress.IntersectsWith(region.regionAddress);
		} 

		#endregion IntersectsWith( WorksheetRegion )

		#region IntersectsWith( int, int, int, int )

		internal bool IntersectsWith( int firstRow, int firstColumn, int lastRow, int lastColumn )
		{
			// MD 3/13/12 - 12.1 - Table Support
			//if ( lastRow < this.firstRow ||
			//    lastColumn < this.firstColumn ||
			//    this.lastRow < firstRow ||
			//    this.lastColumn < firstColumn )
			//{
			//    return false;
			//}
			//
			//return true;
			if (this.Worksheet == null)
				return false;

			return this.regionAddress.IntersectsWith(new WorksheetRegionAddress(firstRow, lastRow, (short)firstColumn, (short)lastColumn));
		}

		#endregion IntersectsWith( int, int, int, int )

		// MD 2/29/12 - 12.1 - Table Support
		#region ShiftRegion

		internal virtual ShiftAddressResult ShiftRegion(CellShiftOperation shiftOperation, bool leaveAttachedToBottomOfWorksheet)
		{
			ShiftAddressResult result = shiftOperation.ShiftRegionAddress(ref this.regionAddress, leaveAttachedToBottomOfWorksheet);
			if (result.DidShift)
			{
				// Clear the topRow cache is the region was shifted.
				this.topRow = null;

				if (result.IsDeleted)
					this.OnShiftedOffWorksheet();
			}

			return result;
		}

		#endregion // ShiftRegion

		// MD 2/23/12 - 12.1 - Table Support
		#region Union

		internal static WorksheetRegion Union(WorksheetRegion region1, WorksheetRegion region2)
		{
			if (region1 == null)
				return region2;

			if (region2 == null)
				return region1;

			if (region1.Worksheet == null)
				return region1;

			Debug.Assert(region1.Worksheet == region2.Worksheet, "These regions are from different worksheets.");
			return region1.Worksheet.GetCachedRegion(
				Math.Min(region1.FirstRow, region2.FirstRow),
				Math.Min(region1.FirstColumn, region2.FirstColumn),
				Math.Max(region1.LastRow, region2.LastRow),
				Math.Max(region1.LastColumn, region2.LastColumn));
		}

		#endregion // Union

		#region VerifyColumnOrder

		internal static void VerifyColumnOrder( int firstColumn, int lastColumn )
		{
			if ( lastColumn < firstColumn )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_LastColumnBeforeFirst" ) );
		}

		#endregion VerifyColumnOrder

		// MD 7/2/08 - Excel 2007 Format
		#region VerifyFormatLimits

        internal void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
        {
            int maxColumnIndex = Workbook.GetMaxColumnCount(testFormat) - 1;

			// MD 3/13/12 - 12.1 - Table Support
            //if (maxColumnIndex < this.lastColumn)
			if (maxColumnIndex < this.regionAddress.LastColumnIndex)
            {
				// MD 3/13/12 - 12.1 - Table Support
                //limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), this.lastColumn, maxColumnIndex));
				limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxColumnIndex"), this.regionAddress.LastColumnIndex, maxColumnIndex));
                return;
            }

            int maxRowIndex = Workbook.GetMaxRowCount(testFormat) - 1;

			// MD 3/13/12 - 12.1 - Table Support
			//if (maxRowIndex < this.lastRow)
			//    limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxRowIndex"), this.lastRow, maxRowIndex));
			if (maxRowIndex < this.regionAddress.LastRowIndex)
				limitErrors.AddError(String.Format(SR.GetString("LE_FormatLimitError_MaxRowIndex"), this.regionAddress.LastRowIndex, maxRowIndex));
        }

		#endregion VerifyFormatLimits

		#region VerifyRowOrder

		internal static void VerifyRowOrder( int firstRow, int lastRow )
		{
			if ( lastRow < firstRow )
				throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_LastRowBeforeFirst" ) );
		}

		#endregion VerifyRowOrder

		#endregion Internal Methods

		// MD 2/29/12 - 12.1 - Table Support
		#region Private Methods

		#region OnShiftedOffWorksheet

		private void OnShiftedOffWorksheet()
		{
			this.worksheet = null;
			this.topRow = null;
			this.calcReference = null;

			this.regionAddress.SetInvalid();
		}

		#endregion // OnShiftedOffWorksheet

		// MD 2/29/12 - 12.1 - Table Support
		#region VerifyRegion

		private void VerifyRegion()
		{
			if (this.worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_RegionShiftedOffWorksheet"), "region");
		} 

		#endregion // VerifyRegion

		#endregion // Private Methods

		#endregion Methods

		#region Properties

		// MD 3/13/12 - 12.1 - Table Support
		#region Address

		internal WorksheetRegionAddress Address
		{
			get { return this.regionAddress; }
		}

		#endregion // Address

		// MD 7/14/08 - Excel formula solving
		#region CalcReference

		internal IExcelCalcReference CalcReference
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (this.worksheet == null)
					return null;

				// MD 4/12/11 - TFS67084
				// Moved away from using WorksheetCell objects and used better caching.
				//if ( this.IsSingleCell )
				//    return this.TopLeftCell.CalcReference;
				//
				//if ( this.calcReference == null )
				//{
				//    // The region references don't have to be added to the calc network because there are never unconnected
				//    // references to them.
				//    this.calcReference = new RegionCalcReference( this );
				//}
				//
				//return this.calcReference;
				if (this.calcReference == null)
				{
					if (this.IsSingleCell)
					{
						this.calcReference = this.TopRow.GetCellCalcReference(this.FirstColumnInternal);
					}
					else
					{
						// The region references don't have to be added to the calc network because there are never unconnected
						// references to them.
						this.calcReference = new RegionCalcReference(this);
					}
				}

				return this.calcReference;
			}
		} 

		#endregion CalcReference

		#region FirstColumn

		/// <summary>
		/// Gets the index of the first column in the region.
		/// </summary>
		/// <value>The index of the first column in the region.</value>
		public int FirstColumn
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.firstColumn; }
			get { return this.FirstColumnInternal; }
		}

		#endregion FirstColumn

		// MD 4/12/11 - TFS67084
		#region FirstColumnInternal

		internal short FirstColumnInternal
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.firstColumn; }
			get { return this.regionAddress.FirstColumnIndex; }
		}

		#endregion FirstColumnInternal

		#region FirstRow

		/// <summary>
		/// Gets the index of the first row in the region.
		/// </summary>
		/// <value>The index of the first row in the region.</value>
		public int FirstRow
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.firstRow; }
			get { return this.regionAddress.FirstRowIndex; }
		}

		#endregion FirstRow

		// MD 7/25/08 - Excel formula solving
		#region Height






		internal int Height
		{
			get { return this.LastRow - this.FirstRow + 1; }
		} 

		#endregion Height

        // MBS 7/22/08 - Excel 2007 Format
        #region IsSingleCell

        internal bool IsSingleCell
        {
            get { return this.FirstRow == this.LastRow && this.FirstColumn == this.LastColumn; }
        }
        #endregion //IsSingleCell

        #region LastColumn

        /// <summary>
		/// Gets the index of the last column in the region.
		/// </summary>
		/// <value>The index of the last column in the region.</value>
		public int LastColumn
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.lastColumn; }
			get { return this.LastColumnInternal; }
		}

		#endregion LastColumn

		// MD 4/12/11 - TFS67084
		#region LastColumnInternal

		internal short LastColumnInternal
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.lastColumn; }
			//
			//// MD 5/13/11 - Data Validations / Page Breaks
			//set { this.lastColumn = value; }
			get { return this.regionAddress.LastColumnIndex; }
		}

		#endregion LastColumnInternal

		#region LastRow

		/// <summary>
		/// Gets the index of the last row in the region.
		/// </summary>
		/// <value>The index of the last row in the region.</value>
		public int LastRow
		{
			// MD 3/13/12 - 12.1 - Table Support
			//get { return this.lastRow; }
			get { return this.regionAddress.LastRowIndex; }
		}

		#endregion LastRow

		// MD 3/13/12 - 12.1 - Table Support
		// This is no longer needed.
		#region Removed

		//// MD 5/13/11 - Data Validations / Page Breaks
		//#region LastRowInternal

		//internal int LastRowInternal
		//{
		//    get { return this.lastrow; }
		//    set { this.lastRow = value; }
		//}

		//#endregion LastRowInternal

		#endregion // Removed

		// MD 7/14/08 - Excel formula solving
		#region TopLeftCell

		internal WorksheetCell TopLeftCell
		{
		    get 
		    {
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				if (this.TopRow == null)
					return null;

				// MD 5/13/11 - Data Validations / Page Breaks
				//if ( this.topLeftCell == null )
				//    this.topLeftCell = this.GetCell( 0, 0 );
				//
				//return this.topLeftCell;
				return this.TopRow.Cells[this.FirstColumn];
		    }
		}

		#endregion TopLeftCell

		// MD 4/12/11 - TFS67084
		#region TopRow

		internal WorksheetRow TopRow
		{
			get
			{
				// MD 2/29/12 - 12.1 - Table Support
				// The worksheet can now be null.
				//if (this.topRow == null)
				if (this.topRow == null && this.worksheet != null)
					this.topRow = this.Worksheet.Rows[this.FirstRow];

				return this.topRow;
			}
		}

		#endregion  // TopRow

		// MD 7/25/08 - Excel formula solving
		#region Width






		internal int Width
		{
			get { return this.LastColumn - this.FirstColumn + 1; }
		} 

		#endregion Width

		#region Worksheet

		/// <summary>
		/// Gets the worksheet on which the region resides.
		/// </summary>
		/// <value>
		/// The worksheet on which the region resides or null if the region has been shifted off the worksheet.
		/// </value>
		public Worksheet Worksheet
		{
			get { return this.worksheet; }
		}

		#endregion Worksheet

		#endregion Properties

		// MD 8/21/08 - Excel formula solving
		#region HorizontalSorter class

		internal class HorizontalSorter : 
			IComparer<WorksheetRegion>,
			IComparer<WorksheetRegionAddress>, // MD 3/13/12 - 12.1 - Table Support
			IComparer<WeakReference>
		{
			public static readonly HorizontalSorter Instance = new HorizontalSorter();

			private HorizontalSorter() { }

			// MD 3/13/12 - 12.1 - Table Support
			//public static int CompareHelper( WorksheetRegion x, WorksheetRegion y )
			//{
			//    return x.FirstColumn - y.FirstColumn;
			//}
			//
			//public static int CompareHelperLast( WorksheetRegion x, WorksheetRegion y )
			//{
			//    return x.LastColumn - y.LastColumn;
			//}
			public static int CompareHelper(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				return x.FirstColumnIndex - y.FirstColumnIndex;
			}

			public static int CompareHelperLast(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				return x.LastColumnIndex - y.LastColumnIndex;
			}

			#region IComparer<WeakReference> Members

			int IComparer<WeakReference>.Compare( WeakReference x, WeakReference y )
			{
				WorksheetRegion xRegion = (WorksheetRegion)Utilities.GetWeakReferenceTarget( x );
				WorksheetRegion yRegion = (WorksheetRegion)Utilities.GetWeakReferenceTarget( y );

				if ( xRegion == null && yRegion == null )
					return 0;

				if ( xRegion == null )
					return -1;

				if ( yRegion == null )
					return 1;

				return ( (IComparer<WorksheetRegion>)this ).Compare( xRegion, yRegion );
			}

			#endregion

			#region IComparer<WorksheetRegion> Members

			int IComparer<WorksheetRegion>.Compare( WorksheetRegion x, WorksheetRegion y )
			{
				// MD 3/13/12 - 12.1 - Table Support
				// Moved this logic to the IComparer<WorksheetRegionAddress>.Compare implementation.
				#region Moved

				// MD 3/13/12 - 12.1 - Table Support
				//int result = HorizontalSorter.CompareHelper( x, y );

				//if ( result != 0 )
				//    return result;

				//result = VerticalSorter.CompareHelper( x, y );

				//if ( result != 0 )
				//    return result;

				//result = HorizontalSorter.CompareHelperLast( x, y );

				//if ( result != 0 )
				//    return result;

				//return VerticalSorter.CompareHelperLast( x, y );

				#endregion // Moved
				return ((IComparer<WorksheetRegionAddress>)this).Compare(x.Address, y.Address);
			}

			#endregion

			// MD 3/13/12 - 12.1 - Table Support
			#region IComparer<WorksheetRegionAddress> Members

			int IComparer<WorksheetRegionAddress>.Compare(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				int result = HorizontalSorter.CompareHelper(x, y);

				if (result != 0)
					return result;

				result = VerticalSorter.CompareHelper(x, y);

				if (result != 0)
					return result;

				result = HorizontalSorter.CompareHelperLast(x, y);

				if (result != 0)
					return result;

				return VerticalSorter.CompareHelperLast(x, y);
			}

			#endregion
		}

		#endregion HorizontalSorter class

		// MD 8/21/08 - Excel formula solving
		#region VerticalSorter class

		internal class VerticalSorter : IComparer<WorksheetRegion>,
			IComparer<WorksheetRegionAddress> // MD 3/13/12 - 12.1 - Table Support
		{
			public static readonly VerticalSorter Instance = new VerticalSorter();

			private VerticalSorter() { }

			// MD 3/13/12 - 12.1 - Table Support
			//public static int CompareHelper( WorksheetRegion x, WorksheetRegion y )
			//{
			//    return x.FirstRow - y.FirstRow;
			//}
			//
			//public static int CompareHelperLast( WorksheetRegion x, WorksheetRegion y )
			//{
			//    return x.LastRow - y.LastRow;
			//}
			public static int CompareHelper(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				return x.FirstRowIndex - y.FirstRowIndex;
			}

			public static int CompareHelperLast(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				return x.LastRowIndex - y.LastRowIndex;
			}

			#region IComparer<WorksheetRegion> Members

			int IComparer<WorksheetRegion>.Compare( WorksheetRegion x, WorksheetRegion y )
			{
				// MD 3/13/12 - 12.1 - Table Support
				// Moved this logic to the IComparer<WorksheetRegionAddress>.Compare implementation.
				#region Moved

				//int result = VerticalSorter.CompareHelper( x, y );

				//if ( result != 0 )
				//    return result;

				//return HorizontalSorter.CompareHelper( x, y );

				#endregion // Moved
				return ((IComparer<WorksheetRegionAddress>)this).Compare(x.Address, y.Address);
			}

			#endregion

			// MD 3/13/12 - 12.1 - Table Support
			#region IComparer<WorksheetRegionAddress> Members

			int IComparer<WorksheetRegionAddress>.Compare(WorksheetRegionAddress x, WorksheetRegionAddress y)
			{
				int result = VerticalSorter.CompareHelper(x, y);

				if (result != 0)
					return result;

				return HorizontalSorter.CompareHelper(x, y);
			}

			#endregion
		}

		#endregion VerticalSorter class
	}

	// MD 3/13/12 - 12.1 - Table Support
	#region WorksheetRegionAddress struct

	internal struct WorksheetRegionAddress : IComparable<WorksheetRegionAddress>
	{
		#region Member Variables

		internal readonly static WorksheetRegionAddress InvalidReference;

		private short _firstColumnIndex;
		private int _firstRowIndex;
		private short _lastColumnIndex;
		private int _lastRowIndex;
		private int _hashCode;

		#endregion // Member Variables

		#region Constructor

		static WorksheetRegionAddress()
		{
			WorksheetRegionAddress.InvalidReference = new WorksheetRegionAddress();
			WorksheetRegionAddress.InvalidReference.SetInvalid();
		}

		public WorksheetRegionAddress(
			int firstRowIndex,
			int lastRowIndex,
			short firstColumnIndex,
			short lastColumnIndex)
		{
			_firstColumnIndex = firstColumnIndex;
			_firstRowIndex = firstRowIndex;
			_lastColumnIndex = lastColumnIndex;
			_lastRowIndex = lastRowIndex;
			_hashCode = WorksheetRegionAddress.CalculateHashCode(_firstRowIndex, _lastRowIndex, _firstColumnIndex, _lastColumnIndex);
		}

		public WorksheetRegionAddress(
			WorksheetCellAddress cell1,
			WorksheetCellAddress cell2)
		{
			_firstColumnIndex = Math.Min(cell1.ColumnIndex, cell2.ColumnIndex);
			_firstRowIndex = Math.Min(cell1.RowIndex, cell2.RowIndex);
			_lastColumnIndex = Math.Max(cell1.ColumnIndex, cell2.ColumnIndex);
			_lastRowIndex = Math.Max(cell1.RowIndex, cell2.RowIndex);
			_hashCode = WorksheetRegionAddress.CalculateHashCode(_firstRowIndex, _lastRowIndex, _firstColumnIndex, _lastColumnIndex);
		}

		#endregion // Constructor

		#region Interfaces

		#region IComparable<WorksheetRegionAddress> Members

		int IComparable<WorksheetRegionAddress>.CompareTo(WorksheetRegionAddress other)
		{
			int result = _firstColumnIndex - other._firstColumnIndex;
			if (result != 0)
				return result;

			result = _firstRowIndex - other._firstRowIndex;
			if (result != 0)
				return result;

			result = _lastColumnIndex - other._lastColumnIndex;
			if (result != 0)
				return result;

			result = _lastRowIndex - other._lastRowIndex;
			if (result != 0)
				return result;

			return 0;
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region Equals

		public override bool Equals(object obj)
		{
			if ((obj is WorksheetRegionAddress) == false)
				return false;

			return this == (WorksheetRegionAddress)obj;
		}

		#endregion // Equals

		#region GetHashCode

		public override int GetHashCode()
		{
			return _hashCode;
		}

		#endregion // GetHashCode

		#region ToString

		public override string ToString()
		{
			return this.ToString(false, false, Workbook.LatestFormat, CellReferenceMode.A1);
		}

		#endregion // ToString

		#endregion // Base Class Overrides

		#region Operators

		#region ==

		public static bool operator ==(WorksheetRegionAddress a, WorksheetRegionAddress b)
		{
			return
				a._firstColumnIndex == b._firstColumnIndex &&
				a._firstRowIndex == b._firstRowIndex &&
				a._lastColumnIndex == b._lastColumnIndex &&
				a._lastRowIndex == b._lastRowIndex;
		}

		#endregion // ==

		#region !=

		public static bool operator !=(WorksheetRegionAddress a, WorksheetRegionAddress b)
		{
			return !(a == b);
		}

		#endregion // !=

		#endregion // Operators

		#region Methods

		#region CalculateHashCode

		public void CalculateHashCode()
		{
			_hashCode = WorksheetRegionAddress.CalculateHashCode(_firstRowIndex, _lastRowIndex, _firstColumnIndex, _lastColumnIndex);
		}

		public static int CalculateHashCode(
			int firstRowIndex,
			int lastRowIndex,
			short firstColumnIndex,
			short lastColumnIndex)
		{
			return
				firstRowIndex ^
				lastRowIndex << 5 ^
				firstColumnIndex << 10 ^
				lastColumnIndex << 15;
		}

		#endregion // CalculateHashCode

		#region Contains

		public bool Contains(WorksheetCellAddress address)
		{
			return this.Contains(address.RowIndex, address.ColumnIndex);
		}

		public bool Contains(int rowIndex, short columnIndex)
		{
			if (this.IsValid == false)
				return false;

			if (rowIndex < _firstRowIndex || _lastRowIndex < rowIndex ||
				columnIndex < _firstColumnIndex || _lastColumnIndex < columnIndex)
			{
				return false;
			}

			return true;
		}

		public bool Contains(WorksheetRegionAddress other)
		{
			if (this.IsValid == false)
				return false;

			if (other._firstRowIndex < this._firstRowIndex ||
				other._firstColumnIndex < this._firstColumnIndex ||
				this._lastRowIndex < other._lastRowIndex ||
				this._lastColumnIndex < other._lastColumnIndex)
			{
				return false;
			}

			return true;
		}

		#endregion // Contains

		#region IntersectsWith

		public bool IntersectsWith(WorksheetRegionAddress other)
		{
			if (other._lastRowIndex < this._firstRowIndex ||
				other._lastColumnIndex < this._firstColumnIndex ||
				this._lastRowIndex < other._firstRowIndex ||
				this._lastColumnIndex < other._firstColumnIndex)
			{
				return false;
			}

			return true;
		}

		#endregion // IntersectsWith

		#region SetInvalid

		public void SetInvalid()
		{
			_firstRowIndex = -1;
			_lastRowIndex = -1;
			_firstColumnIndex = -1;
			_lastColumnIndex = -1;
			this.CalculateHashCode();
		}

		#endregion // SetInvalid

		#region ToString

		public string ToString(bool useRelativeRow, bool useRelativeColumn, WorkbookFormat format, CellReferenceMode cellReferenceMode)
		{
			if (this.IsValid == false)
				return FormulaParser.ReferenceErrorValue;

			int sourceRowIndexForRelatives = _firstRowIndex;
			short sourceColumnIndexForRelatives = _firstColumnIndex;

			return String.Format("{0}{1}{2}",
				CellAddress.GetCellReferenceString(_firstRowIndex, _firstColumnIndex, useRelativeRow, useRelativeColumn,
					format, sourceRowIndexForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode),
				FormulaParser.RangeOperator,
				CellAddress.GetCellReferenceString(_lastRowIndex, _lastColumnIndex, useRelativeRow, useRelativeColumn,
					format, sourceRowIndexForRelatives, sourceColumnIndexForRelatives, false, cellReferenceMode));
		}

		#endregion // ToString

		#endregion // Methods

		#region Properties

		#region FirstColumnIndex

		public short FirstColumnIndex
		{
			get { return _firstColumnIndex; }
			set
			{
				if (_firstColumnIndex == value)
					return;

				_firstColumnIndex = value;
				this.CalculateHashCode();
			}
		}

		#endregion // FirstColumnIndex

		#region FirstRowIndex

		public int FirstRowIndex
		{
			get { return _firstRowIndex; }
			set
			{
				if (_firstRowIndex == value)
					return;

				_firstRowIndex = value;
				this.CalculateHashCode();
			}
		}

		#endregion // FirstRowIndex

		#region Height

		public int Height
		{
			get { return _lastRowIndex - _firstRowIndex + 1; }
		}

		#endregion // Height

		#region IsSingleCell

		public bool IsSingleCell
		{
			get
			{
				return
					_firstRowIndex == _lastRowIndex &&
					_firstColumnIndex == _lastColumnIndex;
			}
		}

		#endregion // IsSingleCell

		#region IsValid

		public bool IsValid
		{
			get { return _firstRowIndex >= 0; }
		}

		#endregion // IsValid

		#region LastColumnIndex

		public short LastColumnIndex
		{
			get { return _lastColumnIndex; }
			set
			{
				if (_lastColumnIndex == value)
					return;

				_lastColumnIndex = value;
				this.CalculateHashCode();
			}
		}

		#endregion // LastColumnIndex

		#region LastRowIndex

		public int LastRowIndex
		{
			get { return _lastRowIndex; }
			set
			{
				if (_lastRowIndex == value)
					return;

				_lastRowIndex = value;
				this.CalculateHashCode();
			}
		}

		#endregion // LastRowIndex

		#region Width

		public int Width
		{
			get { return _lastColumnIndex - _firstColumnIndex + 1; }
		}

		#endregion // Width

		#endregion // Properties
	}

	#endregion // WorksheetRegionAddress struct
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