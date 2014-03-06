using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Documents.Excel.FormulaUtilities;





using Infragistics.Shared;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Class which exposes the various print options available for a worksheet which can be saved with both a 
	/// worksheet and a custom view.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This class provides a way to control how a worksheet is printed.
	/// </p>
	/// </remarks>
	/// <seealso cref="Worksheet.PrintOptions"/>
	/// <seealso cref="CustomView.GetPrintOptions"/>



	public

		 class PrintOptions
	{
		#region Constants

        //  BF 8/11/08 Excel2007 Format
        //  Added default property value constants
        internal const double defaultLeftMargin = 0.7;
        internal const double defaultTopMargin = 0.75;
        internal const double defaultRightMargin = 0.7;
        internal const double defaultBottomMargin = 0.75;
        internal const double defaultHeaderMargin = 0.3;
        internal const double defaultFooterMargin = 0.3;

        internal const PaperSize defaultPaperSize = PaperSize.Letter;
        internal const int defaultScalingFactor = 100;
        internal const int defaultStartPageNumber = 1;
        internal const int defaultMaxPagesHorizontally = 1;
        internal const int defaultMaxPagesVertically = 1;
        internal const PageOrder defaultPageOrder = PageOrder.DownThenOver;
        internal const bool defaultPrintInBlackAndWhite = false;
        internal const bool defaultDraftQuality = false;
        internal const PrintNotes defaultPrintNotes = PrintNotes.DontPrint;
        internal const PageNumbering defaultPageNumbering = PageNumbering.Automatic;
        internal const PrintErrors defaultPrintErrors = PrintErrors.PrintAsDisplayed;
        internal const int defaultResolution = 600;
        internal const int defaultVerticalResolution = defaultResolution;
        internal const int defaultNumberOfCopies = 1;

		#endregion Constants

		#region Member Variables

        // MD 8/1/08 - BR35121
		private Worksheet associatedWorksheet;

        //  BF 8/11/08 Excel2007 Format
        //  I changed the member declarations to use the default value constants

		private double leftMargin = defaultLeftMargin;
		private double topMargin = defaultTopMargin;
		private double rightMargin = defaultRightMargin;
		private double bottomMargin = defaultBottomMargin;
		private double headerMargin = defaultHeaderMargin;
		private double footerMargin = defaultFooterMargin;

		private bool centerHorizontally;
		private bool centerVertically;
		private bool draftQuality = defaultDraftQuality;
		private string footer = string.Empty;
		private string header = string.Empty;
		private int maxPagesHorizontally = defaultMaxPagesHorizontally;
		private int maxPagesVertically = defaultMaxPagesVertically;
		private int numberOfCopies = defaultNumberOfCopies;

        // MBS 7/31/08 - Excel 2007 Format
		//private Orientation orientation = Orientation.Portrait;
        private Orientation orientation = Orientation.Default;

		private PageOrder pageOrder = defaultPageOrder;
		private PageNumbering pageNumbering = defaultPageNumbering;
		private PaperSize paperSize = defaultPaperSize;
		private PrintErrors printErrors = defaultPrintErrors;
		private bool printGridlines;
		private bool printInBlackAndWhite = defaultPrintInBlackAndWhite;
		private PrintNotes printNotes = defaultPrintNotes;
		private bool printRowAndColumnHeaders;
		private int resolution = defaultResolution;
		private int scalingFactor = defaultScalingFactor;
		private ScalingType scalingType = ScalingType.UseScalingFactor;
		private int startPageNumber = defaultStartPageNumber;
		private int verticalResolution = defaultVerticalResolution;

		// MD 2/1/11 - Page Break support
		private HorizontalPageBreakCollection horizontalPageBreaks;
		private VerticalPageBreakCollection verticalPageBreaks;

		private RepeatTitleRange columnsToRepeatAtLeft;
		private PrintAreasCollection printAreas;
		private RepeatTitleRange rowsToRepeatAtTop;

		#endregion Member Variables

		#region Constructor

		// MD 8/1/08 - BR35121
		// Added a parameter so the options could have a reference to the worksheet with which they are associated.
		//internal PrintOptions() { }
		internal PrintOptions( Worksheet associatedWorksheet ) 
		{
			this.associatedWorksheet = associatedWorksheet;
		}

		#endregion Constructor

		#region Methods

		#region Public Methods

		// MD 5/25/11 - Data Validations / Page Breaks
		#region ClearPageBreaks

		/// <summary>
		/// Clears all page breaks from the <see cref="PrintOptions"/>.
		/// </summary>
		/// <seealso cref="PageBreakCollection&lt;T&gt;.Clear"/>
		/// <seealso cref="PageBreakCollection&lt;T&gt;.Remove"/>
		/// <seealso cref="PageBreakCollection&lt;T&gt;.RemoveAt"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public void ClearPageBreaks()
		{
			if (this.horizontalPageBreaks != null)
				this.horizontalPageBreaks.Clear();

			if (this.verticalPageBreaks != null)
				this.verticalPageBreaks.Clear();
		}

		#endregion  // ClearPageBreaks

		// MD 5/25/11 - Data Validations / Page Breaks
		#region InsertPageBreak(WorksheetCell)

		/// <summary>
		/// Inserts a horizontal and/or vertical page break before the specified cell.
		/// </summary>
		/// <param name="cell">The cell at which to insert the page break(s).</param>
		/// <remarks>
		/// <p class="body">
		/// If the cell is not contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the cell is at the top-left corner of the <see cref="Worksheet"/>, an exception will be thrown. If the cell is on the left edge of the 
		/// Worksheet, a horizontal page break will be inserted above the cell. If the cell is on the top edge of the Worksheet, a vertical page break 
		/// will be inserted to the left of the cell. If the cell is anywhere else in the Worksheet, a horizontal page break will be inserted above the 
		/// cell and a vertical page break will be inserted to the left of the cell.
		/// </p>
		/// <p class="body">
		/// If the cell is contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the cell is at the top-left corner of the print area, no page breaks will be inserted. If the cell is on the left edge of the print area, 
		/// a horizontal page break will be inserted above the cell. If the cell is on the top edge of the print area, a vertical page break will be 
		/// inserted to the left of the cell. If the cell is anywhere else in the print area, a horizontal page break will be inserted above the cell
		/// and a vertical page break will be inserted to the left of the cell.
		/// </p>
		/// <p class="body">
		/// When page breaks are inserted in a print area, they will only extend to the edges of the print area.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="cell"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="cell"/> is at the top-left corner of the <see cref="Worksheet"/>.
		/// </exception>
		/// <seealso cref="HorizontalPageBreaks"/>
		/// <seealso cref="VerticalPageBreaks"/>
		/// <seealso cref="ClearPageBreaks"/>
		/// <seealso cref="InsertPageBreak(WorksheetColumn)"/>
		/// <seealso cref="InsertPageBreak(WorksheetRow)"/>
		/// <seealso cref="PrintAreas"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public void InsertPageBreak(WorksheetCell cell)
		{
			if (cell == null)
				throw new ArgumentNullException("cell");

			// MD 2/29/12 - 12.1 - Table Support
			// The worksheet can now be null.
			if (cell.Worksheet == null)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_CellShiftedOffWorksheet"), "cell");

			int columnIndex = cell.ColumnIndex;
			int rowIndex = cell.RowIndex;

			if (columnIndex == 0 && rowIndex == 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreakCantBeA1Cell"));

			Worksheet worksheet = cell.Worksheet;

			int firstColumn = 0;
			int firstRow = 0;
			int lastColumn = worksheet.Columns.MaxCount - 1;
			int lastRow = worksheet.Rows.MaxCount - 1;

			WorksheetRegion printArea = null;

			if (this.printAreas != null)
			{
				for (int i = 0; i < this.printAreas.Count; i++)
				{
					WorksheetRegion region = this.printAreas[i];

					if (region.Contains(cell))
					{
						firstColumn = region.FirstColumn;
						firstRow = region.FirstRow;
						lastColumn = region.LastColumn;
						lastRow = region.LastRow;

						printArea = region;

						break;
					}
				}
			}

			if (columnIndex > firstColumn)
				this.VerticalPageBreaks.AddInternal(new VerticalPageBreak(columnIndex, printArea), false);

			if (rowIndex > firstRow)
				this.HorizontalPageBreaks.AddInternal(new HorizontalPageBreak(rowIndex, printArea), false);
		}

		// MD 5/25/11 - Data Validations / Page Breaks
		#endregion  // InsertPageBreak(WorksheetCell)

		// MD 5/25/11 - Data Validations / Page Breaks
		#region InsertPageBreak(WorksheetColumn)

		/// <summary>
		/// Inserts a vertical page break to the left of the specified column.
		/// </summary>
		/// <param name="column">The column at which to insert the page break.</param>
		/// <remarks>
		/// <p class="body">
		/// If the column is not contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the column is at the left edge of the <see cref="Worksheet"/>, an exception will be thrown. If the column is anywhere else in the Worksheet, 
		/// a vertical page break will be inserted to the left of the column.
		/// </p>
		/// <p class="body">
		/// If the column is contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the column is at the left edge of the print area, no page break will be inserted. If the column is anywhere else in the print area, a 
		/// vertical page break will be inserted to the left of the column.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="column"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="column"/> is at the left edge of the <see cref="Worksheet"/>.
		/// </exception>
		/// <seealso cref="VerticalPageBreaks"/>
		/// <seealso cref="ClearPageBreaks"/>
		/// <seealso cref="InsertPageBreak(WorksheetCell)"/>
		/// <seealso cref="InsertPageBreak(WorksheetRow)"/>
		/// <seealso cref="PrintAreas"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public void InsertPageBreak(WorksheetColumn column)
		{
			if (column == null)
				throw new ArgumentNullException("column");

			if (column.Index == 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreakCantBeBeforeLeftColumn"));

			WorksheetRegion printArea = null;
			WorksheetRegion columnRegion = this.associatedWorksheet.GetCachedRegion(0, column.Index, this.associatedWorksheet.Rows.MaxCount - 1, column.Index);

			if (this.printAreas != null)
			{
				for (int i = 0; i < this.printAreas.Count; i++)
				{
					WorksheetRegion region = this.printAreas[i];

					if (region.Contains(columnRegion))
					{
						if (region.FirstColumn == column.Index)
							return;

						printArea = region;
						break;
					}
				}
			}

			this.VerticalPageBreaks.AddInternal(new VerticalPageBreak(column.Index, printArea), false);
		}

		#endregion  // InsertPageBreak(WorksheetColumn)

		// MD 5/25/11 - Data Validations / Page Breaks
		#region InsertPageBreak(WorksheetRow)

		/// <summary>
		/// Inserts a horizontal page break above the specified row.
		/// </summary>
		/// <param name="row">The row at which to insert the page break.</param>
		/// <remarks>
		/// <p class="body">
		/// If the row is not contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the row is at the top edge of the <see cref="Worksheet"/>, an exception will be thrown. If the row is anywhere else in the Worksheet, 
		/// a horizontal page break will be inserted above the row.
		/// </p>
		/// <p class="body">
		/// If the row is contained in one of the print areas in the <see cref="PrintAreas"/> collection, the page breaks added will be as follows:
		/// If the row is at the top edge of the print area, no page break will be inserted. If the row is anywhere else in the print area, a 
		/// horizontal page break will be inserted above the row.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// Occurs when <paramref name="row"/> is null.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Occurs when <paramref name="row"/> is at the top edge of the <see cref="Worksheet"/>.
		/// </exception>
		/// <seealso cref="HorizontalPageBreaks"/>
		/// <seealso cref="ClearPageBreaks"/>
		/// <seealso cref="InsertPageBreak(WorksheetCell)"/>
		/// <seealso cref="InsertPageBreak(WorksheetColumn)"/>
		/// <seealso cref="PrintAreas"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public void InsertPageBreak(WorksheetRow row)
		{
			if (row == null)
				throw new ArgumentNullException("row");

			if (row.Index == 0)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_PB_PageBreakCantBeBeforeTopRow"));

			WorksheetRegion printArea = null;
			WorksheetRegion rowRegion = this.associatedWorksheet.GetCachedRegion(row.Index, 0, row.Index, this.associatedWorksheet.Columns.MaxCount - 1);

			if (this.printAreas != null)
			{
				for (int i = 0; i < this.printAreas.Count; i++)
				{
					WorksheetRegion region = this.printAreas[i];

					if (region.Contains(rowRegion))
					{
						if (region.FirstRow == row.Index)
							return;

						printArea = region;
						break;
					}
				}
			}

			this.HorizontalPageBreaks.AddInternal(new HorizontalPageBreak(row.Index, printArea), false);
		}

		#endregion  // InsertPageBreak(WorksheetRow)

		#region Reset

		/// <summary>
		/// Resets the print options to their default settings.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The defaults used for each setting are the same defaults with which Microsoft Excel creates a blank worksheet.
		/// </p>
		/// </remarks>
		public void Reset()
		{
			this.bottomMargin = 0.75;
			this.centerHorizontally = false;
			this.centerVertically = false;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.columnsToRepeatAtLeft = null;

			this.draftQuality = false;
			this.footer = string.Empty;
			this.footerMargin = 0.3;
			this.header = string.Empty;
			this.headerMargin = 0.3;
			this.leftMargin = 0.7;
			this.maxPagesHorizontally = 1;
			this.maxPagesVertically = 1;
			this.numberOfCopies = 1;
			this.orientation = Orientation.Portrait;
			this.pageOrder = PageOrder.DownThenOver;
			this.pageNumbering = PageNumbering.Automatic;
			this.paperSize = PaperSize.Letter;
			this.printErrors = PrintErrors.PrintAsDisplayed;
			this.printGridlines = false;
			this.printInBlackAndWhite = false;
			this.printNotes = PrintNotes.DontPrint;
			this.printRowAndColumnHeaders = false;
			this.resolution = 600;
			this.rightMargin = 0.7;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.rowsToRepeatAtTop = null;

			this.scalingFactor = 100;
			this.scalingType = ScalingType.UseScalingFactor;
			this.startPageNumber = 1;
			this.topMargin = 0.75;
			this.verticalResolution = 600;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.ClearPageBreaks();

			if (this.printAreas != null)
				this.printAreas.Clear();
		}

		#endregion Reset

		#endregion // Public Methods

		#region Internal Methods

		#region InitializeFrom

		internal void InitializeFrom( PrintOptions printOptions )
		{
			this.bottomMargin = printOptions.bottomMargin;
			this.centerHorizontally = printOptions.centerHorizontally;
			this.centerVertically = printOptions.centerVertically;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.columnsToRepeatAtLeft = printOptions.columnsToRepeatAtLeft;

			this.draftQuality = printOptions.draftQuality;
			this.footer = printOptions.footer;
			this.footerMargin = printOptions.footerMargin;
			this.header = printOptions.header;
			this.headerMargin = printOptions.headerMargin;
			this.leftMargin = printOptions.leftMargin;
			this.maxPagesHorizontally = printOptions.maxPagesHorizontally;
			this.maxPagesVertically = printOptions.maxPagesVertically;
			this.numberOfCopies = printOptions.numberOfCopies;
			this.orientation = printOptions.orientation;
			this.pageOrder = printOptions.pageOrder;
			this.pageNumbering = printOptions.pageNumbering;
			this.paperSize = printOptions.paperSize;
			this.printErrors = printOptions.printErrors;
			this.printGridlines = printOptions.printGridlines;
			this.printInBlackAndWhite = printOptions.printInBlackAndWhite;
			this.printNotes = printOptions.printNotes;
			this.printRowAndColumnHeaders = printOptions.printRowAndColumnHeaders;
			this.resolution = printOptions.resolution;
			this.rightMargin = printOptions.rightMargin;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.rowsToRepeatAtTop = printOptions.rowsToRepeatAtTop;

			this.scalingFactor = printOptions.scalingFactor;
			this.scalingType = printOptions.scalingType;
			this.startPageNumber = printOptions.startPageNumber;
			this.topMargin = printOptions.topMargin;
			this.verticalResolution = printOptions.verticalResolution;

			// MD 5/25/11 - Data Validations / Page Breaks
			this.ClearPageBreaks();

			if (this.printAreas != null)
				this.printAreas.Clear();

			if (printOptions.HasPrintAreas)
			{
				for (int i = 0; i < printOptions.PrintAreas.Count; i++)
					this.PrintAreas.Add(printOptions.PrintAreas[i]);
			}

			if (printOptions.HasHorizontalPageBreaks)
			{
				for (int i = 0; i < printOptions.HorizontalPageBreaks.Count; i++)
					this.HorizontalPageBreaks.Add(printOptions.HorizontalPageBreaks[i]);
			}

			if (printOptions.HasVerticalPageBreaks)
			{
				for (int i = 0; i < printOptions.VerticalPageBreaks.Count; i++)
					this.VerticalPageBreaks.Add(printOptions.VerticalPageBreaks[i]);
			}
		}

		#endregion InitializeFrom

		// MD 6/30/09 - TFS18936
		// Created a helper method so the ScalingFactor could be set without throwing an exception during loading.
		#region SetScalingFactor

		internal void SetScalingFactor( int value, bool throwOnError )
		{
			if ( this.scalingFactor == value )
				return;

			const int MinValue = 10;
			const int MaxValue = 400;

			if ( throwOnError )
			{
				if ( value < MinValue || MaxValue < value )
					throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_ScalingFactor" ) );
			}
			// MD 3/15/11 - TFS64430
			// Observed while testing this bug. When the value in the file is 0, Microsoft Excel displays 100 (the default value) for the scale.
			else if (value == 0)
				value = defaultScalingFactor;
			else if ( value < MinValue )
				value = MinValue;
			else if ( MaxValue < value )
				value = MaxValue;

			this.scalingFactor = value;
		} 

		#endregion SetScalingFactor

        //  BF 8/11/08 Excel2007 Format
        #region ShouldSerialize methods

            #region ShouldSerializePageSetupInfo


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

        internal bool ShouldSerializePageSetupInfo()
        {
            return  this.paperSize != defaultPaperSize ||
                    this.scalingFactor != defaultScalingFactor ||
                    this.startPageNumber != defaultStartPageNumber ||
                    this.maxPagesHorizontally != defaultMaxPagesHorizontally ||
                    this.maxPagesVertically != defaultMaxPagesVertically ||
                    this.pageOrder != defaultPageOrder ||
                    this.orientation != Orientation.Default ||
                    this.printInBlackAndWhite != defaultPrintInBlackAndWhite ||
                    this.draftQuality != defaultDraftQuality ||
                    this.printNotes != defaultPrintNotes ||
                    this.pageNumbering != defaultPageNumbering ||
                    this.printErrors != defaultPrintErrors ||
                    this.resolution != defaultResolution ||
                    this.verticalResolution != defaultVerticalResolution ||
                    this.numberOfCopies != defaultNumberOfCopies;
        }
            #endregion ShouldSerializePageSetupInfo

        #endregion ShouldSerialize methods

		// MD 5/25/11 - Data Validations / Page Breaks
		#region UpdatePrintAreasNamedReference

		internal void UpdatePrintAreasNamedReference()
		{
			Workbook workbook = this.associatedWorksheet.Workbook;
			if (workbook != null && this == this.associatedWorksheet.PrintOptions)
			{
				NamedReference namedRef = workbook.NamedReferences.Find(NamedReferenceBase.PrintAreaValue, this.associatedWorksheet);

				if (this.HasPrintAreas == false)
				{
					if (namedRef != null)
						workbook.NamedReferences.Remove(namedRef);
				}
				else
				{
					bool shouldAdd;
					this.UpdatePrintAreasNamedReference(workbook, NamedReferenceBase.PrintAreaValue, ref namedRef, out shouldAdd);

					if (shouldAdd)
						workbook.NamedReferences.Add(namedRef, false);
				}
			}
		}

		internal void UpdatePrintAreasNamedReference(Workbook workbook, string name, ref NamedReference namedRef, out bool shouldAdd)
		{
			shouldAdd = (namedRef == null);

			string refString = Utilities.CreateReferenceString(null, this.associatedWorksheet.Name);
			StringBuilder formulaBuilder = new StringBuilder("=");

			// MD 7/5/12 - TFS115501
			string unionOperator = FormulaParser.GetUnionOperatorResolved(workbook.CultureResolved);

			for (int i = 0; i < this.PrintAreas.Count; i++)
			{
				WorksheetRegion region = this.PrintAreas[i];
				formulaBuilder.Append(region.ToString(CellReferenceMode.A1, true, false, false));

				if (i != this.PrintAreas.Count - 1)
				{
					// MD 7/5/12 - TFS115501
					// The union operator may need to be different depending on the culture.
					//formulaBuilder.Append(",");
					formulaBuilder.Append(unionOperator);
				}
			}

			this.UpdateSyncedNamedReference(name, formulaBuilder.ToString(), ref namedRef);
		}

		#endregion  // UpdatePrintAreasNamedReference

		// MD 5/25/11 - Data Validations / Page Breaks
		#region UpdatePrintTitlesNamedReference

		internal void UpdatePrintTitlesNamedReference(Workbook workbook, string name, ref NamedReference namedRef, out bool shouldAdd)
		{
			shouldAdd = (namedRef == null);

			string formula = "=";

			if (this.columnsToRepeatAtLeft != null)
			{
				WorksheetRegion region = this.associatedWorksheet.GetCachedRegion(0, this.columnsToRepeatAtLeft.StartIndex, this.associatedWorksheet.Rows.MaxCount - 1, this.columnsToRepeatAtLeft.EndIndex);
				formula += region.ToString(CellReferenceMode.A1, true, false, false);

				if (this.rowsToRepeatAtTop != null)
				{
					// MD 4/9/12 - TFS101506
					//formula += FormulaParser.GetUnionOperatorResolved();
					formula += FormulaParser.GetUnionOperatorResolved(this.AssociatedWorksheet.Culture);
				}
			}

			if (this.rowsToRepeatAtTop != null)
			{
				WorksheetRegion region = this.associatedWorksheet.GetCachedRegion(this.rowsToRepeatAtTop.StartIndex, 0, this.rowsToRepeatAtTop.EndIndex, this.associatedWorksheet.Columns.MaxCount - 1);
				formula += region.ToString(CellReferenceMode.A1, true, false, false);
			}

			this.UpdateSyncedNamedReference(name, formula, ref namedRef);
		}

		#endregion  // UpdatePrintTitlesNamedReference

		// MD 5/25/11 - Data Validations / Page Breaks
		#region UpdateSyncedNamedReference

		private void UpdateSyncedNamedReference(string name, string formula, ref NamedReference namedRef)
		{
			if (namedRef == null)
			{
				NamedReferenceCollection collection = null;
				if (this.associatedWorksheet.Workbook != null)
					collection = this.associatedWorksheet.Workbook.NamedReferences;

				namedRef = new NamedReference(collection, this.associatedWorksheet);
				namedRef.Name = name;
				namedRef.SetFormula(formula, CellReferenceMode.A1);
			}
			else
			{
				namedRef.SetFormula(formula, CellReferenceMode.A1);
			}
		}

		#endregion  // UpdateSyncedNamedReference

		// MD 5/25/11 - Data Validations / Page Breaks
		#region VerifyFormatLimits

		internal void VerifyFormatLimits(FormatLimitErrors limitErrors, WorkbookFormat testFormat)
		{
			if (this.columnsToRepeatAtLeft != null)
			{
				int maxColumns = Workbook.GetMaxColumnCount(testFormat);
				if (this.columnsToRepeatAtLeft.EndIndex >= maxColumns)
					limitErrors.AddError(SR.GetString("LE_ColumnsToRepeatAtLeftAreOutsideAvailableRange"));
			}

			if (this.rowsToRepeatAtTop != null)
			{
				int maxRows = Workbook.GetMaxRowCount(testFormat);
				if (this.rowsToRepeatAtTop.EndIndex >= maxRows)
					limitErrors.AddError(SR.GetString("LE_RowsToRepeatAtTopAreOutsideAvailableRange"));
			}
		}

		#endregion  // VerifyFormatLimits

		#endregion // Internal Methods

		#region Private Methods

		// MD 5/25/11 - Data Validations / Page Breaks
		#region UpdatePrintTitlesNamedReference

		private void UpdatePrintTitlesNamedReference()
		{
			Workbook workbook = this.associatedWorksheet.Workbook;
			if (workbook != null && this == this.associatedWorksheet.PrintOptions)
			{
				NamedReference namedRef = workbook.NamedReferences.Find(NamedReferenceBase.PrintTitlesValue, this.associatedWorksheet);

				if (this.columnsToRepeatAtLeft == null && this.rowsToRepeatAtTop == null)
				{
					if (namedRef != null)
						workbook.NamedReferences.Remove(namedRef);
				}
				else
				{
					bool shouldAdd;
					this.UpdatePrintTitlesNamedReference(workbook, NamedReferenceBase.PrintTitlesValue, ref namedRef, out shouldAdd);

					if (shouldAdd)
						workbook.NamedReferences.Add(namedRef, false);
				}
			}
		}

		#endregion  // UpdatePrintTitlesNamedReference

		#endregion  // Private Methods

        #endregion Methods

        #region Properties

		#region Internal Properties

		// MD 5/25/11 - Data Validations / Page Breaks
		#region AssociatedWorksheet

		internal Worksheet AssociatedWorksheet
		{
			get { return this.associatedWorksheet; }
		}

		#endregion  // AssociatedWorksheet

		#endregion  // Internal Properties

		#region Public Properties

		#region BottomMargin

		/// <summary>
		/// Gets or sets the margin at the bottom of each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The margin at the bottom of each printed page of the worksheet.</value>
		/// <seealso cref="FooterMargin"/>
		/// <seealso cref="HeaderMargin"/>
		/// <seealso cref="LeftMargin"/>
		/// <seealso cref="RightMargin"/>
		/// <seealso cref="TopMargin"/>
		public double BottomMargin
		{
			get { return this.bottomMargin; }
			set
			{
				if ( this.bottomMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.bottomMargin = value;
				}
			}
		}

		#endregion BottomMargin

		#region CenterHorizontally

		/// <summary>
		/// Gets or sets the value indicating whether the printed pages should be centered horizontally.
		/// </summary>
		/// <value>The value indicating whether the printed pages should be centered horizontally.</value>
		/// <seealso cref="CenterVertically"/>
		public bool CenterHorizontally
		{
			get { return this.centerHorizontally; }
			set { this.centerHorizontally = value; }
		}

		#endregion CenterHorizontally

		#region CenterVertically

		/// <summary>
		/// Gets or sets the value indicating whether the printed pages should be centered vertically.
		/// </summary>
		/// <value>The value indicating whether the printed pages should be centered vertically.</value>
		/// <seealso cref="CenterHorizontally"/>
		public bool CenterVertically
		{
			get { return this.centerVertically; }
			set { this.centerVertically = value; }
		}

		#endregion CenterVertically

		// MD 5/25/11 - Data Validations / Page Breaks
		#region ColumnsToRepeatAtLeft

		/// <summary>
		/// Gets or sets the range of columns which should be printed on every page.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Occurs when the value specified is not null and its <seealso cref="RepeatTitleRange.EndIndex"/> value is greater than or equal 
		/// to the number of columns in the <see cref="Worksheet"/>.
		/// </exception>
		/// <seealso cref="RowsToRepeatAtTop"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public RepeatTitleRange ColumnsToRepeatAtLeft
		{
			get { return this.columnsToRepeatAtLeft; }
			set
			{
				if (this.columnsToRepeatAtLeft == value)
					return;

				if (value != null && value.EndIndex >= this.associatedWorksheet.Columns.MaxCount)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_ColumnsToRepeatAtLeftOutsideRange"), "value");

				this.columnsToRepeatAtLeft = value;
				this.UpdatePrintTitlesNamedReference();
			}
		}

		#endregion  // ColumnsToRepeatAtLeft

		#region DraftQuality

		/// <summary>
		/// Gets or sets the value indicating whether the printed pages should be printed using draft quality.
		/// </summary>
		/// <value>The value indicating whether the printed pages should be printed using draft quality.</value>
		public bool DraftQuality
		{
			get { return this.draftQuality; }
			set { this.draftQuality = value; }
		}

		#endregion DraftQuality

		#region Footer

		/// <summary>
		/// Gets or sets the footer for each page of the printed worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The '&amp;' character in the header and footer is a special character. Depending on what is after it, 
		/// the formatting of the text can be controlled or dynamic text can be inserted. Below is a list of 
		/// the available commands:
		/// <list type="table">
		///		<listheader>
		///			<term>Section commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;L</term>
		///			<description>
		///				The following text will appear in the left section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;C</term>
		///			<description>
		///				The following text will appear in the center section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;R</term>
		///			<description>
		///				The following text will appear in the right section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		/// </list>
		/// <list type="table">
		///		<listheader>
		///			<term>Replacement commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;&amp;</term>
		///			<description>Insert the '&amp;' character.</description>
		///		</item>
		///		<item>
		///			<term>&amp;A</term>
		///			<description>Insert the current worksheet name.</description>
		///		</item>
		///		<item>
		///			<term>&amp;D</term>
		///			<description>Insert the current date.</description>
		///		</item>
		///		<item>
		///			<term>&amp;F</term>
		///			<description>Insert the current file name.</description>
		///		</item>
		///		<item>
		///			<term>&amp;G</term>
		///			<description>Insert an image (<b>Note:</b> This command is currently not supported).</description>
		///		</item>
		///		<item>
		///			<term>&amp;N</term>
		///			<description>Insert the  number of pages the worksheet will need to print.</description>
		///		</item>
		///		<item>
		///			<term>&amp;P</term>
		///			<description>Insert the current page number.</description>
		///		</item>
		///		<item>
		///			<term>&amp;T</term>
		///			<description>Insert the current time.</description>
		///		</item>
		///		<item>
		///			<term>&amp;Z</term>
		///			<description>Insert the current file path (without the file name).</description>
		///		</item>
		/// </list>
		/// <list type="table">
		/// 	<listheader>
		///			<term>Formatting commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;B</term>
		///			<description>Toggle bold.</description>
		///		</item>
		///		<item>
		///			<term>&amp;E</term>
		///			<description>Toggle double underlining.</description>
		///		</item>
		///		<item>
		///			<term>&amp;I</term>
		///			<description>Toggle italics.</description>
		///		</item>
		///		<item>
		///			<term>&amp;S</term>
		///			<description>Toggle strikethrough.</description>
		///		</item>
		///		<item>
		///			<term>&amp;U</term>
		///			<description>Toggle underlining.</description>
		///		</item>
		///		<item>
		///			<term>&amp;X</term>
		///			<description>Toggle superscript.</description>
		///		</item>
		///		<item>
		///			<term>&amp;Y</term>
		///			<description>Toggle subscript.</description>
		///		</item>
		///		<item>
		///			<term>&amp;&lt;FontSize&gt;</term>
		///			<description>
		///				Sets a new font size for the following text. The size is expressed as a positive integer. 
		///				If a number is to follow this command in the header, it must be separated by a space.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;"&lt;FontName&gt;"</term>
		///			<description>
		///				Sets a new font for the following text. If the font name is not recognized, the default 
		///				font will be used.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;"&lt;FontName&gt;,&lt;FontStyle&gt;"</term>
		///			<description>
		///				Sets the new font and font style for the following text. The font style is usually "Regular", 
		///				"Bold", "Italic", or "Bold Italic", but	can be other styles depending on the font. The 
		///				available font styles can be seen in the font dialog when a font is selected.
		///			</description>
		///		</item>
		///	</list>
		/// </p>
		/// <p class="body">
		/// The header or footer string could look like this: &amp;L&amp;"Arial,Bold"&amp;D&amp;CPage &amp;P of 
		/// &amp;N on &amp;A&amp;R&amp;14&amp;F.
		/// </p>
		/// </remarks>
		/// <value>The footer for each page of the worksheet.</value>
		/// <seealso cref="Header"/>
		/// <seealso cref="FooterMargin"/>
		public string Footer
		{
			get { return this.footer; }
			set { this.footer = value; }
		}

		#endregion Footer

		#region FooterMargin

		/// <summary>
		/// Gets or sets the footer margin for each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The footer margin for each printed page of the worksheet.</value>
		/// <seealso cref="Footer"/>
		/// <seealso cref="BottomMargin"/>
		/// <seealso cref="HeaderMargin"/>
		/// <seealso cref="LeftMargin"/>
		/// <seealso cref="RightMargin"/>
		/// <seealso cref="TopMargin"/>
		public double FooterMargin
		{
			get { return this.footerMargin; }
			set
			{
				if ( this.footerMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.footerMargin = value;
				}
			}
		}

		#endregion FooterMargin

		#region Header

		/// <summary>
		/// Gets or sets the header for each page of the printed worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The '&amp;' character in the header and footer is a special character. Depending on what is after it, 
		/// the formatting of the text can be controlled or dynamic text can be inserted. Below is a list of 
		/// the available commands:
		/// <list type="table">
		///		<listheader>
		///			<term>Section commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;L</term>
		///			<description>
		///				The following text will appear in the left section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;C</term>
		///			<description>
		///				The following text will appear in the center section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;R</term>
		///			<description>
		///				The following text will appear in the right section. The formatting of new sections is 
		///				always the default formatting, regardless of the formatting of the previous section.
		///			</description>
		///		</item>
		/// </list>
		/// <list type="table">
		///		<listheader>
		///			<term>Replacement commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;&amp;</term>
		///			<description>Insert the '&amp;' character.</description>
		///		</item>
		///		<item>
		///			<term>&amp;A</term>
		///			<description>Insert the current worksheet name.</description>
		///		</item>
		///		<item>
		///			<term>&amp;D</term>
		///			<description>Insert the current date.</description>
		///		</item>
		///		<item>
		///			<term>&amp;F</term>
		///			<description>Insert the current file name.</description>
		///		</item>
		///		<item>
		///			<term>&amp;G</term>
		///			<description>Insert an image (<b>Note:</b> This command is currently not supported).</description>
		///		</item>
		///		<item>
		///			<term>&amp;N</term>
		///			<description>Insert the  number of pages the worksheet will need to print.</description>
		///		</item>
		///		<item>
		///			<term>&amp;P</term>
		///			<description>Insert the current page number.</description>
		///		</item>
		///		<item>
		///			<term>&amp;T</term>
		///			<description>Insert the current time.</description>
		///		</item>
		///		<item>
		///			<term>&amp;Z</term>
		///			<description>Insert the current file path (without the file name).</description>
		///		</item>
		/// </list>
		/// <list type="table">
		/// 	<listheader>
		///			<term>Formatting commands</term>
		///		</listheader>
		///		<item>
		///			<term>&amp;B</term>
		///			<description>Toggle bold.</description>
		///		</item>
		///		<item>
		///			<term>&amp;E</term>
		///			<description>Toggle double underlining.</description>
		///		</item>
		///		<item>
		///			<term>&amp;I</term>
		///			<description>Toggle italics.</description>
		///		</item>
		///		<item>
		///			<term>&amp;S</term>
		///			<description>Toggle strikethrough.</description>
		///		</item>
		///		<item>
		///			<term>&amp;U</term>
		///			<description>Toggle underlining.</description>
		///		</item>
		///		<item>
		///			<term>&amp;X</term>
		///			<description>Toggle superscript.</description>
		///		</item>
		///		<item>
		///			<term>&amp;Y</term>
		///			<description>Toggle subscript.</description>
		///		</item>
		///		<item>
		///			<term>&amp;&lt;FontSize&gt;</term>
		///			<description>
		///				Sets a new font size for the following text. The size is expressed as a positive integer. 
		///				If a number is to follow this command in the header, it must be separated by a space.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;"&lt;FontName&gt;"</term>
		///			<description>
		///				Sets a new font for the following text. If the font name is not recognized, the default 
		///				font will be used.
		///			</description>
		///		</item>
		///		<item>
		///			<term>&amp;"&lt;FontName&gt;,&lt;FontStyle&gt;"</term>
		///			<description>
		///				Sets the new font and font style for the following text. The font style is usually "Regular", 
		///				"Bold", "Italic", or "Bold Italic", but	can be other styles depending on the font. The 
		///				available font styles can be seen in the font dialog when a font is selected.
		///			</description>
		///		</item>
		///	</list>
		/// </p>
		/// <p class="body">
		/// The header or footer string could look like this: &amp;L&amp;"Arial,Bold"&amp;D&amp;CPage &amp;P of 
		/// &amp;N on &amp;A&amp;R&amp;14&amp;F.
		/// </p>
		/// </remarks>
		/// <value>The header for each page of the worksheet.</value>
		/// <seealso cref="Footer"/>
		/// <seealso cref="HeaderMargin"/>
		public string Header
		{
			get { return this.header; }
			set { this.header = value; }
		}

		#endregion Header

		#region HeaderMargin

		/// <summary>
		/// Gets or sets the header margin for each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The header margin for each printed page of the worksheet.</value>
		/// <seealso cref="Header"/>
		/// <seealso cref="BottomMargin"/>
		/// <seealso cref="FooterMargin"/>
		/// <seealso cref="LeftMargin"/>
		/// <seealso cref="RightMargin"/>
		/// <seealso cref="TopMargin"/>
		public double HeaderMargin
		{
			get { return this.headerMargin; }
			set
			{
				if ( this.headerMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.headerMargin = value;
				}
			}
		}

		#endregion HeaderMargin

		// MD 2/1/11 - Page Break support
		#region HorizontalPageBreaks

		/// <summary>
		/// Gets the collection of horizontal page breaks in the <see cref="Worksheet"/>.
		/// </summary>
		/// <seealso cref="HorizontalPageBreak"/>
		/// <seealso cref="ClearPageBreaks"/>
		/// <seealso cref="InsertPageBreak(WorksheetCell)"/>
		/// <seealso cref="InsertPageBreak(WorksheetColumn)"/>
		/// <seealso cref="InsertPageBreak(WorksheetRow)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public HorizontalPageBreakCollection HorizontalPageBreaks
		{
			get
			{
				if (this.horizontalPageBreaks == null)
					this.horizontalPageBreaks = new HorizontalPageBreakCollection(this);

				return this.horizontalPageBreaks;
			}
		}

		internal bool HasHorizontalPageBreaks
		{
			get
			{
				return
					this.horizontalPageBreaks != null &&
					this.horizontalPageBreaks.Count > 0;
			}
		}

		#endregion  // HorizontalPageBreaks

		#region LeftMargin

		/// <summary>
		/// Gets or sets the margin at the left of each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The margin at the left of each printed page of the worksheet.</value>
		/// <seealso cref="BottomMargin"/>
		/// <seealso cref="FooterMargin"/>
		/// <seealso cref="HeaderMargin"/>
		/// <seealso cref="RightMargin"/>
		/// <seealso cref="TopMargin"/>
		public double LeftMargin
		{
			get { return this.leftMargin; }
			set
			{
				if ( this.leftMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.leftMargin = value;
				}
			}
		}

		#endregion LeftMargin

		#region MaxPagesHorizontally

		/// <summary>
		/// Gets or sets the maximum number of pages allowed in the horizontal direction to print the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A value of zero indicates no maximum is used in the horizontal direction. As many pages as needed will be used.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> For MaxPagesHorizontally and <see cref="MaxPagesVertically"/> to affect the way the worksheet is printed,
		/// <see cref="ScalingType"/> must be set to a value of FitToPages. However, if the the ScalingType is different 
		/// and these values aren't used, they will still be saved with the worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the valid range of 0 and 32767.
		/// </exception>
		/// <value>The maximum number of pages allowed in the horizontal direction to print the worksheet.</value>
		/// <seealso cref="MaxPagesVertically"/>
		/// <seealso cref="ScalingType"/>
		public int MaxPagesHorizontally
		{
			get { return this.maxPagesHorizontally; }
			set
			{
				if ( this.maxPagesHorizontally != value )
				{
					if ( value < 0 || 32767 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MaxPagesHorizontally" ) );

					this.maxPagesHorizontally = value;
				}
			}
		}

		#endregion MaxPagesHorizontally

		#region MaxPagesVertically

		/// <summary>
		/// Gets or sets the maximum number of pages allowed in the vertical direction to print the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A value of zero indicates no maximum is used in the vertical direction. As many pages as needed will be used.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> For <see cref="MaxPagesHorizontally"/> and MaxPagesVertically to affect the way the worksheet is printed,
		/// <see cref="ScalingType"/> must be set to a value of FitToPages. However, if the the ScalingType is different 
		/// and these values aren't used, they will still be saved with the worksheet.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the valid range of 0 and 32767.
		/// </exception>
		/// <value>The maximum number of pages allowed in the vertical direction to print the worksheet.</value>
		/// <seealso cref="MaxPagesHorizontally"/>
		/// <seealso cref="ScalingType"/>
		public int MaxPagesVertically
		{
			get { return this.maxPagesVertically; }
			set
			{
				if ( this.maxPagesVertically != value )
				{
					if ( value < 0 || 32767 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_MaxPagesVertically" ) );

					this.maxPagesVertically = value;
				}
			}
		}

		#endregion MaxPagesVertically

		#region NumberOfCopies

		/// <summary>
		/// Gets or sets the number of copies to print.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of 1 and 65535.
		/// </exception>
		/// <value>The number of copies to print.</value>
		public int NumberOfCopies
		{
			get { return this.numberOfCopies; }
			set
			{
				if ( this.numberOfCopies != value )
				{
					if ( value < 1 || 65535 < value )
					{
						// MD 8/1/08 - BR35121
						// If the workbook is loading, we should ignore the fact the value is out of range and instead make it the default value.
						//throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_NumberOfCopies" ) );
						if ( this.associatedWorksheet.IsLoading == false )
							throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_NumberOfCopies" ) );

						value = 1;
					}

					this.numberOfCopies = value;
				}
			}
		}

		#endregion NumberOfCopies

		#region Orientation

		/// <summary>
		/// Gets or sets the orientation for each page of the printed worksheet.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="Orientation"/> enumeration.
		/// </exception>
		/// <value>The orientation for each page of the printed worksheet.</value>
		public Orientation Orientation
		{
			get { return this.orientation; }
			set
			{
				if ( this.orientation != value )
				{
					if ( Enum.IsDefined( typeof( Orientation ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( Orientation ) );

					this.orientation = value;
				}
			}
		}

        // MBS 7/30/08 - Excel 2007 Format
        // We need a resolved property, since Excel has a concept of a 'default' orientation.
        // We only need this resolved property to preserve compatibility with the 97-2003 format,
        // since it's perfectly valid to serialize 'default' for 2007        
        /// <summary>
        /// Returns the resolved orientation for each page of the printed worksheet.
        /// </summary>
        public Orientation OrientationResolved
        {
            get
            {
                if (this.orientation == Orientation.Default)
                    return Orientation.Portrait;

                return this.orientation;
            }
        }
		#endregion Orientation

		#region PageOrder

		/// <summary>
		/// Gets or sets the order in which to print pages for multiple page worksheets.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="PageOrder"/> enumeration.
		/// </exception>
		/// <value>The order in which to print pages for multiple page worksheets.</value>
		public PageOrder PageOrder
		{
			get { return this.pageOrder; }
			set
			{
				if ( this.pageOrder != value )
				{
					if ( Enum.IsDefined( typeof( PageOrder ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( PageOrder ) );

					this.pageOrder = value;
				}
			}
		}

		#endregion PageOrder

		#region PageNumbering

		/// <summary>
		/// Gets or sets the method with which pages are numbered.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is set to a value of UseStartPageNumber, the first page is numbered using the <see cref="StartPageNumber"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="PageNumbering"/> enumeration.
		/// </exception>
		/// <value>The method with which pages are numbered.</value>
		/// <seealso cref="StartPageNumber"/>
		public PageNumbering PageNumbering
		{
			get { return this.pageNumbering; }
			set
			{
				if ( this.pageNumbering != value )
				{
					if ( Enum.IsDefined( typeof( PageNumbering ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( PageNumbering ) );

					this.pageNumbering = value;
				}
			}
		}

		#endregion PageNumbering

		#region PaperSize

		/// <summary>
		/// Gets or sets the paper size for each printed page of the worksheet.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="PaperSize"/> enumeration.
		/// </exception>
		/// <value>The paper size for each printed page of the worksheet.</value>
		public PaperSize PaperSize
		{
			get { return this.paperSize; }
			set
			{
				if ( this.paperSize != value )
				{
					if ( Enum.IsDefined( typeof( PaperSize ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( PaperSize ) );

					this.paperSize = value;
				}
			}
		}

		#endregion PaperSize

		// MD 5/25/11 - Data Validations / Page Breaks
		#region PrintAreas

		/// <summary>
		/// Gets the collection of print areas in the <see cref="Worksheet"/>.
		/// </summary>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public PrintAreasCollection PrintAreas
		{
			get
			{
				if (this.printAreas == null)
					this.printAreas = new PrintAreasCollection(this);

				return this.printAreas;
			}
		}

		internal bool HasPrintAreas
		{
			get { return this.printAreas != null && this.printAreas.Count != 0; }
		}

		#endregion  // PrintAreas

		#region PrintErrors

		/// <summary>
		/// Gets or sets the way error values of cells are printed.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A cell can have an error value if its <see cref="WorksheetCell.Value"/> is set directly to
		/// an <see cref="ErrorValue"/> or if it is set to a <see cref="Formula"/> that evaluates to an
		/// error.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="PrintErrors"/> enumeration.
		/// </exception>
		/// <value>The way error values of cells are printed.</value>
		/// <seealso cref="ErrorValue"/>
		/// <seealso cref="Formula"/>
		public PrintErrors PrintErrors
		{
			get { return this.printErrors; }
			set
			{
				if ( this.printErrors != value )
				{
					if ( Enum.IsDefined( typeof( PrintErrors ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( PrintErrors ) );

					this.printErrors = value;
				}
			}
		}

		#endregion PrintErrors

		#region PrintGridlines

		/// <summary>
		/// Gets or sets the value which indicates whether to print the worksheet gridlines.
		/// </summary>
		/// <value>The value which indicates whether to print the worksheet gridlines.</value>
		/// <seealso cref="DisplayOptions.ShowGridlines"/>
		public bool PrintGridlines
		{
			get { return this.printGridlines; }
			set { this.printGridlines = value; }
		}

		#endregion PrintGridlines

		#region PrintInBlackAndWhite

		/// <summary>
		/// Gets or sets the value indicating whether the worksheet should be printed in black and white.
		/// </summary>
		/// <value>The value indicating whether the worksheet should be printed in black and white.</value>
		public bool PrintInBlackAndWhite
		{
			get { return this.printInBlackAndWhite; }
			set { this.printInBlackAndWhite = value; }
		}

		#endregion PrintInBlackAndWhite

		#region PrintNotes

		/// <summary>
		/// Gets or sets the way cell comments are printed.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is set to a value of PrintAsDisplayed, the comments will only print if they are displayed on the worksheet.
		/// If comments are hidden but indicators are shown, neither the indicators nor the comments will print.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="PrintNotes"/> enumeration.
		/// </exception>
		/// <value>The way cell notes are printed.</value>
		/// <seealso cref="WorksheetCell.Comment"/>
		/// <seealso cref="WorksheetCellComment"/>
		public PrintNotes PrintNotes
		{
			get { return this.printNotes; }
			set
			{
				if ( this.printNotes != value )
				{
					if ( Enum.IsDefined( typeof( PrintNotes ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( PrintNotes ) );

					this.printNotes = value;
				}
			}
		}

		#endregion PrintNotes

		#region PrintRowAndColumnHeaders

		/// <summary>
		/// Gets or sets the value indicating whether to print row and column headers.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The row and column headers show the identifier of the row or column.
		/// </p>
		/// </remarks>
		/// <value>The value indicating whether to print row and column headers.</value>
		/// <seealso cref="DisplayOptions.ShowRowAndColumnHeaders"/>
		public bool PrintRowAndColumnHeaders
		{
			get { return this.printRowAndColumnHeaders; }
			set { this.printRowAndColumnHeaders = value; }
		}

		#endregion PrintRowAndColumnHeaders

		#region Resolution

		/// <summary>
		/// Gets or sets the horizontal print resolution in DPI.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the valid range of 0 and 65535.
		/// </exception>
		/// <value>The horizontal print resolution in DPI.</value>
		/// <seealso cref="VerticalResolution"/>
		public int Resolution
		{
			get { return this.resolution; }
			set
			{
				if ( this.resolution != value )
				{
					if ( value < 0 || 65535 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Resolution" ) );

					this.resolution = value;
				}
			}
		}

		#endregion Resolution

		#region RightMargin

		/// <summary>
		/// Gets or sets the margin at the right of each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The margin at the right of each printed page of the worksheet.</value>
		/// <seealso cref="BottomMargin"/>
		/// <seealso cref="FooterMargin"/>
		/// <seealso cref="HeaderMargin"/>
		/// <seealso cref="LeftMargin"/>
		/// <seealso cref="TopMargin"/>
		public double RightMargin
		{
			get { return this.rightMargin; }
			set
			{
				if ( this.rightMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.rightMargin = value;
				}
			}
		}

		#endregion RightMargin

		// MD 5/25/11 - Data Validations / Page Breaks
		#region RowsToRepeatAtTop

		/// <summary>
		/// Gets or sets the range of rows which should be printed on every page.
		/// </summary>
		/// <exception cref="ArgumentException">
		/// Occurs when the value specified is not null and its <seealso cref="RepeatTitleRange.EndIndex"/> value is greater than or equal 
		/// to the number of rows in the <see cref="Worksheet"/>.
		/// </exception>
		/// <seealso cref="ColumnsToRepeatAtLeft"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public RepeatTitleRange RowsToRepeatAtTop
		{
			get { return this.rowsToRepeatAtTop; }
			set
			{
				if (this.rowsToRepeatAtTop == value)
					return;

				if (value != null && value.EndIndex >= this.associatedWorksheet.Rows.MaxCount)
					throw new ArgumentException(SR.GetString("LE_ArgumentException_RowsToRepeatAtTopOutsideRange"), "value");

				this.rowsToRepeatAtTop = value;
				this.UpdatePrintTitlesNamedReference();
			}
		}

		#endregion  // RowsToRepeatAtTop

		#region ScalingFactor

		/// <summary>
		/// Gets or sets the scaling factor to use when printing the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The scaling factor is similar to magnifications in that is it stored as a percentage of the normal scaling. 
		/// A value of 100 indicates normal scaling whereas a value of 200 indicates the worksheet is scaled to twice its
		/// normal size.
		/// </p>
		/// <p class="note">
		/// <B>Note:</B> For ScalingFactor to affect the way the worksheet is printed, <see cref="ScalingType"/> must be 
		/// set to a value of UseScalingFactor.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of 10 and 400.
		/// </exception>
		/// <value>The scaling factor to use when printing the worksheet.</value>
		/// <seealso cref="ScalingType"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInNormalView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageBreakView"/>
		/// <seealso cref="WorksheetDisplayOptions.MagnificationInPageLayoutView"/>
		/// <seealso cref="CustomViewDisplayOptions.MagnificationInCurrentView"/>
		public int ScalingFactor
		{
			get { return this.scalingFactor; }
			set
			{
				// MD 6/30/09 - TFS18936
				// Refactored and moved this code to a helper method
				//if ( this.scalingFactor != value )
				//{
				//    if ( value < 10 || 400 < value )
				//        throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_ScalingFactor" ) );
				//
				//    this.scalingFactor = value;
				//}
				this.SetScalingFactor( value, true );
			}
		}

		#endregion ScalingFactor

		#region ScalingType

		/// <summary>
		/// Gets or sets the method for scaling the worksheet when it is printed.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is set to a value of UseScalingFactor, <see cref="ScalingFactor"/> is used to uniformly scale
		/// the worksheet on the printed pages.
		/// </p>
		/// <p class="body">
		/// If this is set to a value of FitToPages, <see cref="MaxPagesHorizontally"/> and <see cref="MaxPagesVertically"/>
		/// are used to set the maximum number of pages to fit the printed worksheet into in both directions.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The assigned value is not defined in the <see cref="ScalingType"/> enumeration.
		/// </exception>
		/// <value>The method for scaling the worksheet when it is printed.</value>
		/// <seealso cref="MaxPagesHorizontally"/>
		/// <seealso cref="MaxPagesVertically"/>
		/// <seealso cref="ScalingFactor"/>
		public ScalingType ScalingType
		{
			get { return this.scalingType; }
			set
			{
				if ( this.scalingType != value )
				{
					if ( Enum.IsDefined( typeof( ScalingType ), value ) == false )
						throw new InvalidEnumArgumentException( "value", (int)value, typeof( ScalingType ) );

					this.scalingType = value;
				}
			}
		}

		#endregion ScalingType

		#region StartPageNumber

		/// <summary>
		/// Gets or sets the page number for the first printed page of the worksheet.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// For this value to be used, <see cref="PageNumbering"/> must be set to a value of UseStartPageNumber.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid range of -32765 and 32767.
		/// </exception>
		/// <value>The page number for the first printed page of the worksheet.</value>
		/// <seealso cref="PageNumbering"/>
		public int StartPageNumber
		{
			get { return this.startPageNumber; }
			set
			{
				if ( this.startPageNumber != value )
				{
					if ( value < -32765 || 32767 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_StartPageNumber" ) );

					this.startPageNumber = value;
				}
			}
		}

		#endregion StartPageNumber

		#region TopMargin

		/// <summary>
		/// Gets or sets the margin at the top of each printed page of the worksheet.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is outside the valid margin range of 0 and 100.
		/// </exception>
		/// <value>The margin at the top of each printed page of the worksheet.</value>
		/// <seealso cref="BottomMargin"/>
		/// <seealso cref="FooterMargin"/>
		/// <seealso cref="HeaderMargin"/>
		/// <seealso cref="LeftMargin"/>
		/// <seealso cref="RightMargin"/>
		public double TopMargin
		{
			get { return this.topMargin; }
			set
			{
				if ( this.topMargin != value )
				{
					if ( value < 0 || 100 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Margins" ) );

					this.topMargin = value;
				}
			}
		}

		#endregion TopMargin

		// MD 2/1/11 - Page Break support
		#region VerticalPageBreaks

		/// <summary>
		/// Gets the collection of vertical page breaks in the <see cref="Worksheet"/>.
		/// </summary>
		/// <seealso cref="VerticalPageBreak"/>
		/// <seealso cref="ClearPageBreaks"/>
		/// <seealso cref="InsertPageBreak(WorksheetCell)"/>
		/// <seealso cref="InsertPageBreak(WorksheetColumn)"/>
		/// <seealso cref="InsertPageBreak(WorksheetRow)"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelAdditions)] 

		public VerticalPageBreakCollection VerticalPageBreaks
		{
			get
			{
				if (this.verticalPageBreaks == null)
					this.verticalPageBreaks = new VerticalPageBreakCollection(this);

				return this.verticalPageBreaks;
			}
		}

		internal bool HasVerticalPageBreaks
		{
			get
			{
				return
					this.verticalPageBreaks != null &&
					this.verticalPageBreaks.Count > 0;
			}
		}

		#endregion  // VerticalPageBreaks

		#region VerticalResolution

		/// <summary>
		/// Gets or sets the vertical print resolution in DPI.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The value assigned is outside the valid range of 0 and 65535.
		/// </exception>
		/// <value>The vertical print resolution in DPI.</value>
		/// <seealso cref="Resolution"/>
		public int VerticalResolution
		{
			get { return this.verticalResolution; }
			set
			{
				if ( this.verticalResolution != value )
				{
					if ( value < 0 || 65535 < value )
						throw new ArgumentOutOfRangeException( "value", value, SR.GetString( "LE_ArgumentOutOfRangeException_Resolution" ) );

					this.verticalResolution = value;
				}
			}
		}

		#endregion VerticalResolution

		#endregion // Public Properties

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