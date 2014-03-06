using System;
using Infragistics.Documents.Excel.Serialization.Excel2007;
using System.ComponentModel;

namespace Infragistics.Documents.Excel
{
	#region BuiltInName

	internal enum BuiltInName
	{
		ConsolidateArea = 0x00,
		AutoOpen = 0x01,
		AutoClose = 0x02,
		Extract = 0x03,
		Database = 0x04,
		Criteria = 0x05,
		PrintArea = 0x06,
		PrintTitles = 0x07,
		Recorder = 0x08,
		DataForm = 0x09,
		AutoActivate = 0x0A,
		AutoDeactivate = 0x0B,
		SheetTitle = 0x0C,
		FilterDatabase = 0x0D
	}

	#endregion BuiltInName

	#region BuiltInStyleType

	internal enum BuiltInStyleType
	{
		Normal = 0,
		RowLevelX = 1,
		ColLevelX = 2,
		Comma = 3,
		Currency = 4,
		Percent = 5,
		Comma0 = 6,
		Currency0 = 7,
		Hyperlink = 8,
		FollowedHyperlink = 9,

		// MD 12/28/11 - 12.1 - Cell Format Updates
		Note = 10,
		WarningText = 11,
		Emphasis1 = 12,
		Emphasis2 = 13,
		Emphasis3 = 14,
		Title = 15,
		Heading1 = 16,
		Heading2 = 17,
		Heading3 = 18,
		Heading4 = 19,
		Input = 20,
		Output = 21,
		Calculation = 22,
		CheckCell = 23,
		LinkedCell = 24,
		Total = 25,
		Good = 26,
		Bad = 27,
		Neutral = 28,
		Accent1 = 29,
		Accent1pct20 = 30,
		Accent1pct40 = 31,
		Accent1pct60 = 32,
		Accent2 = 33,
		Accent2pct20 = 34,
		Accent2pct40 = 35,
		Accent2pct60 = 36,
		Accent3 = 37,
		Accent3pct20 = 38,
		Accent3pct40 = 39,
		Accent3pct60 = 40,
		Accent4 = 41,
		Accent4pct20 = 42,
		Accent4pct40 = 43,
		Accent4pct60 = 44,
		Accent5 = 45,
		Accent5pct20 = 46,
		Accent5pct40 = 47,
		Accent5pct60 = 48,
		Accent6 = 49,
		Accent6pct20 = 50,
		Accent6pct40 = 51,
		Accent6pct60 = 52,
		ExplanatoryText = 53,
	}

	#endregion BuiltInStyleType

	#region CalculationMode

	/// <summary>
	/// Represents the the ways formulas are recalculated when their referenced values change.
	/// </summary>
	/// <seealso cref="Workbook.CalculationMode"/>



	public

		 enum CalculationMode
	{
		/// <summary>
		/// Formulas must be recalculated manually, by pressing a button in the Microsoft Excel interface.
		/// </summary>
		Manual = 0,

		/// <summary>
		/// Formulas and data tables are automatically recalculated when the values in referenced cells change.
		/// </summary>
		Automatic = 1,

		/// <summary>
		/// Only formulas are automatically recalculated when the values in referenced cells change.
		/// Data tables must be recalculated manually.
		/// </summary>
		AutomaticExceptForDataTables = 2 // The docs say this shuold be 0xFFFF, but Excel 07 saves it as 2 and when 0xFFFF is saved, it uses the default value of Automatic
	}

	#endregion CalculationMode

	#region CellBorderLineStyle

	/// <summary>
	/// Represents the different cell border line styles.
	/// </summary>



	public

		 enum CellBorderLineStyle
	{
		/// <summary>
		/// Use the default border line style.
		/// </summary>
		Default = -1,

		/// <summary>
		/// No border.
		/// </summary>
		None = 0,

		/// <summary>
		/// Thin border.
		/// </summary>
		Thin = 1,

		/// <summary>
		/// Medium border.
		/// </summary>
		Medium = 2,

		/// <summary>
		/// Dashed border.
		/// </summary>
		Dashed = 3,

		/// <summary>
		/// Dotted border.
		/// </summary>
		Dotted = 4,

		/// <summary>
		/// Thick border.
		/// </summary>
		Thick = 5,

		/// <summary>
		/// Double-line border.
		/// </summary>
		Double = 6,

		/// <summary>
		/// Dotted border with small dots.
		/// </summary>
		Hair = 7,

		/// <summary>
		/// Dotted border with big dots.
		/// </summary>
		MediumDashed = 8,

		/// <summary>
		/// Dash-dot border.
		/// </summary>
		DashDot = 9,

		/// <summary>
		/// Medium dash-dot border.
		/// </summary>
		MediumDashDot = 10,

		/// <summary>
		/// Dash-dot-dot border.
		/// </summary>
		DashDotDot = 11,

		/// <summary>
		/// Medium dash-dot-dot border.
		/// </summary>
		MediumDashDotDot = 12,

		/// <summary>
		/// Slanted dash-dot border.
		/// </summary>
		SlantedDashDot = 13
	}

	#endregion CellBorderLineStyle

	// MD 5/12/10 - TFS26732
	#region CellFormatValue

	internal enum CellFormatValue
	{
		Alignment,

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//BottomBorderColor,
		BottomBorderColorInfo,

		BottomBorderStyle,

		// MD 10/26/11 - TFS91546
		// MD 1/16/12 - 12.1 - Cell Format Updates
		//DiagonalBorderColor,
		DiagonalBorderColorInfo,

		DiagonalBorders,
		DiagonalBorderStyle,

		// MD 1/19/12 - 12.1 - Cell Format Updates
		//FillPattern,
		//FillPatternBackgroundColor,
		//FillPatternForegroundColor,
		Fill,

		// MD 10/13/10 - TFS43003
		FontBold,

		// MD 1/17/12 - 12.1 - Cell Format Updates
		//FontColor,
		FontColorInfo,

		FontHeight,
		FontItalic,
		FontName,
		FontStrikeout,
		FontSuperscriptSubscriptStyle,
		FontUnderlineStyle,

		// MD 12/30/11 - 12.1 - Cell Format Updates
		FormatOptions,

		FormatString,
		Indent,

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//LeftBorderColor,
		LeftBorderColorInfo,

		LeftBorderStyle,
		Locked,

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//RightBorderColor,
		RightBorderColorInfo,

		RightBorderStyle,
		Rotation,
		ShrinkToFit,
		Style,

		// MD 1/16/12 - 12.1 - Cell Format Updates
		//TopBorderColor,
		TopBorderColorInfo,

		TopBorderStyle,
		VerticalAlignment,
		WrapText,

		
	} 

	#endregion // CellFormatValue

	#region CellReferenceMode

	/// <summary>
	/// Represents the various ways cells can be referenced in a formula.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Setting this on the <see cref="Workbook.CellReferenceMode"/> will also affect the row and column labels.
	/// </p>
	/// </remarks>



	public

		 enum CellReferenceMode
	{
		/// <summary>
		/// Cells are referenced by first specifying characters representing the column and a one-based number 
		/// specifying the row (R54 or CA56). The dollar sign ($) can preface one or both identifiers to make them 
		/// absolute references ($A$7). Without the dollar sign, references still use absolute row and column addresses,
		/// although shifting a formula to a new cell will perform a similar shift on all relative references.
		/// </summary>
		A1 = 1,

		/// <summary>
		/// Cells are referenced in the following format R&lt;RowIndex&gt;C&lt;ColumnIndex&gt; (R34C5 or R2C345). 
		/// These indices are one-based and represent absolute references. To create a relative reference in R1C1 mode, a relative
		/// index must be placed inside square brackets following the R and/or C ( R[-1]C[5] or R9C[-3] ).  An R by itself 
		/// also represents a relative reference and is equivalent to R[0]. Similarly, C is equivalent to C[0], which means a 
		/// formula of =RC always references the cell which contains the formula.
		/// </summary>
		R1C1 = 0
	}

	#endregion CellReferenceMode

	// MD 7/19/12 - TFS116808 (Table resizing)
	#region CellShiftDeleteReason

	internal enum CellShiftDeleteReason : byte
	{
		NotDeleted,
		ShiftDownCoveredAddress,
		ShiftUpCoveredAddress,
		ShiftedOffWorksheetBottom,
		ShiftedOffWorksheetTop,
	}

	#endregion // CellShiftDeleteReason

	// MD 3/20/12 - 12.1 - Table Support
	#region CellShiftInitializeFormatType

	internal enum CellShiftInitializeFormatType
	{
		UseDefaultFormat,
		FromShiftedCellsAdjacentToInsertRegion,
		FromStationaryCellsAdjacentToInsertRegion,
	}

	#endregion // CellShiftInitializeFormatType

	// MD 2/29/12 - 12.1 - Table Support
	#region CellShiftResult

	internal enum CellShiftResult
	{
		Success,
		ErrorLossOfData,
		ErrorLossOfObject,
		ErrorSplitTable,
		ErrorSplitMergedRegion,
		ErrorSplitBlockingValue,
	}

	#endregion // CellShiftResult

	// MD 7/19/12 - TFS116808 (Table resizing)
	#region CellShiftType

	internal enum CellShiftType : byte
	{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		VerticalRotate,






		VerticalShift,
	}

	#endregion // CellShiftType

	// MD 8/30/07 - BR26111
	#region ColorableItem






	internal enum ColorableItem
	{
		CellBorder,
		CellFill,
		CellFont,
		WorksheetGrid,
		WorksheetTab
	}

	#endregion ColorableItem

	#region DateSystem

	/// <summary>
	/// Represents the various ways a date can be stored internally.
	/// </summary>



	public

		 enum DateSystem
	{
		/// <summary>
		/// Dates are stored as time elapsed since 1900.
		/// </summary>
		From1900 = 0,

		/// <summary>
		/// Dates are stored as time elapsed since 1904 (used mainly on Apple computers).
		/// </summary>
		From1904 = 1
	}

	#endregion DateSystem

	// MD 10/26/11 - TFS91546
	#region DiagonalBorders

	/// <summary>
	/// Represents the diagonal borders which can be displayed in cells.
	/// </summary>



	public

		 enum DiagonalBorders
	{
		// MD 12/22/11 - 12.1 - Table Support
		// Added a default value so differential formats can have the concept of an unset DiagonalBorders property.
		/// <summary>
		/// The default value for the diagonal borders.
		/// </summary>
		Default = 0,

		/// <summary>
		/// No diagonal borders will be displayed in the cell.
		/// </summary>
		// MD 12/22/11 - 12.1 - Table Support
		// None must have a bit set so we can detect when a value is explicitly set or now.
		//None = 0,
		None = 1,

		/// <summary>
		/// A diagonal border going from the top-left to bottom-right corner will be displayed in the cell.
		/// </summary>
		// MD 12/22/11 - 12.1 - Table Support
		// All bits have been shifted so we can make room for the None bit. We should also or it in here because it is the bit which
		// tells us that the value was explicitly set.
		//DiagonalDown = 1,
		DiagonalDown = Utilities.DiagonalDownBit | None,

		/// <summary>
		/// A diagonal border going from the bottom-left to top-right corner will be displayed in the cell.
		/// </summary>
		// MD 12/22/11 - 12.1 - Table Support
		// All bits have been shifted so we can make room for the None bit. We should also or it in here because it is the bit which
		// tells us that the value was explicitly set.
		//DiagonalUp = 2,
		DiagonalUp = Utilities.DiagonalUpBit | None,

		/// <summary>
		/// Both diagonal borders will be displayed in the cell.
		/// </summary>
		All = DiagonalDown | DiagonalUp,
	}

	#endregion  // DiagonalBorders

	#region ExcelDefaultableBoolean

	/// <summary>
	/// Enumeration for a boolean type property that allows for a
	/// default setting. This is used in property override situations.
	/// </summary>



	public

		 enum ExcelDefaultableBoolean
	{
		/// <summary>
		/// Use the current default.
		/// </summary>
		Default = 0,

		/// <summary>
		/// True.
		/// </summary>
		True = 1,

		/// <summary>
		/// False.
		/// </summary>
		False = 2
	}

	#endregion ExcelDefaultableBoolean

	#region FillPatternStyle

	/// <summary>
	/// Enumeration for fill pattern styles. Default value is used in property override situations.
	/// </summary>



	public

		 enum FillPatternStyle
	{
		/// <summary>
		/// Obsolete. Use None instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("The FillPatternStyle.Default value is deprecated. Use FillPatternStyle.None instead.")]
		Default = -1,

		/// <summary>
		/// No fill pattern.
		/// </summary>
		None = 0,

		/// <summary>
		/// Solid fill pattern with fill pattern foreground color. 
		/// </summary>
		Solid = 1,

		/// <summary>
		/// "50% gray" fill pattern.
		/// </summary>
		Gray50percent = 2,

		/// <summary>
		/// "75% gray" fill pattern.
		/// </summary>
		Gray75percent = 3,

		/// <summary>
		/// "25% gray" fill pattern.
		/// </summary>
		Gray25percent = 4,

		/// <summary>
		/// Horizontal stripe fill pattern.
		/// </summary>
		HorizontalStripe = 5,

		/// <summary>
		/// Vertical stripe fill pattern.
		/// </summary>
		VerticalStripe = 6,

		/// <summary>
		/// Reverse diagonal stripe fill pattern.
		/// </summary>
		ReverseDiagonalStripe = 7,

		/// <summary>
		/// Diagonal stripe fill pattern.
		/// </summary>
		DiagonalStripe = 8,

		/// <summary>
		/// Diagonal crosshatch fill pattern.
		/// </summary>
		DiagonalCrosshatch = 9,

		/// <summary>
		/// Thick diagonal crosshatch fill pattern.
		/// </summary>
		ThickDiagonalCrosshatch = 10,

		/// <summary>
		/// Thin horizontal stripe fill pattern.
		/// </summary>
		ThinHorizontalStripe = 11,

		/// <summary>
		/// Thin vertical stripe fill pattern.
		/// </summary>
		ThinVerticalStripe = 12,

		/// <summary>
		/// Thin reverse diagonal stripe fill pattern.
		/// </summary>
		ThinReverseDiagonalStripe = 13,

		/// <summary>
		/// Thin diagonal stripe fill pattern.
		/// </summary>
		ThinDiagonalStripe = 14,

		/// <summary>
		/// Thin horizontal crosshatch fill pattern.
		/// </summary>
		ThinHorizontalCrosshatch = 15,

		/// <summary>
		/// Thin diagonal crosshatch fill pattern.
		/// </summary>
		ThinDiagonalCrosshatch = 16,

		/// <summary>
		/// "12.5% gray" fill pattern.
		/// </summary>
		Gray12percent = 17,

		/// <summary>
		/// "6.25% gray" fill pattern.
		/// </summary>
		Gray6percent = 18
	}

	#endregion FillPatternStyle

	#region FontSuperscriptSubscriptStyle

	/// <summary>
	/// Enumeration for font superscript or subscript style. Default value is used in property override situations.
	/// </summary>



	public

		 enum FontSuperscriptSubscriptStyle
	{
		/// <summary>
		/// Use the current default.
		/// </summary>
		Default = -1,

		/// <summary>
		/// No superscript or subscript style.
		/// </summary>
		None = 0,

		/// <summary>
		/// Superscript style.
		/// </summary>
		Superscript = 1,

		/// <summary>
		/// Subscript style.
		/// </summary>
		Subscript = 2
	}

	#endregion FontSuperscriptSubscriptStyle

	#region FontUnderlineStyle

	/// <summary>
	/// Enumeration for font underline styles. Default value is used in property override situations.
	/// </summary>



	public

		 enum FontUnderlineStyle
	{
		/// <summary>
		/// Use the current default.
		/// </summary>
		Default = -1,

		/// <summary>
		/// No underline style.
		/// </summary>
		None = 0x00,

		/// <summary>
		/// Single underline style.
		/// </summary>
		Single = 0x01,

		/// <summary>
		/// Double underline style.
		/// </summary>
		Double = 0x02,

		/// <summary>
		/// Single accounting underline style.
		/// </summary>
		SingleAccounting = 0x21,

		/// <summary>
		/// Double accounting underline style.
		/// </summary>
		DoubleAccounting = 0x22
	}

	#endregion FontUnderlineStyle

	#region FormulaType

	internal enum FormulaType
	{
		Formula,
		ArrayFormula,
		SharedFormula,
		NamedReferenceFormula,
		ExternalNamedReferenceFormula,

		// MD 12/21/11 - TFS97840
		// Data validation formulas need to be parsed slightly differently, so they need their own formula types.
		ListDataValidationFormula,
		NonListDataValidationFormula,
	}

	#endregion FormulaType

	#region FunctionGroup

	internal enum FunctionGroup
	{
		Financial = 1,
		DateAndTime = 2,
		MathAndTrig = 3,
		Statistical = 4,
		LookupAndReference = 5,
		Database = 6,
		Text = 7,
		Logical = 8,
		Information = 9,
		Commands = 10,
		Customizing = 11,
		MacroControl = 12,
		DDEExternal = 13,
		UserDefined = 14,
	}

	#endregion FunctionGroup

	#region HorizontalCellAlignment

	/// <summary>
	/// Enumeration for horizontal alignment styles. Default value is used in property override situations.
	/// </summary>



	public

		 enum HorizontalCellAlignment
	{
		/// <summary>
		/// Use the current default.
		/// </summary>
		Default = -1,

		/// <summary>
		/// Alignment depends on underlying data type.
		/// </summary>
		General = 0,

		/// <summary>
		/// Left alignment.
		/// </summary>
		Left = 1,

		/// <summary>
		/// Centered alignment.
		/// </summary>
		Center = 2,

		/// <summary>
		/// Right alignment.
		/// </summary>
		Right = 3,

		/// <summary>
		/// Repeat cell value to fill whole cell.
		/// </summary>
		Fill = 4,

		/// <summary>
		/// Justify alignment.
		/// </summary>
		Justify = 5,

		/// <summary>
		/// Centers the contents of the left-most cell in a center across selection group. All other cells in the center across selection group must be empty. The cells are not merged, and the data may appear to be in a cell other than the left-most cell.
		/// </summary>
		CenterAcrossSelection = 6,

		// MD 7/7/08 - BR34589
		/// <summary>
		/// Distributed alignment.
		/// </summary>
		Distributed = 7,
	}

	#endregion HorizontalCellAlignment

	// MD 11/8/11 - TFS85193
	#region HorizontalTextAlignment

	/// <summary>
	/// Represents the various horizontal text alignment types.
	/// </summary>



	public

		enum HorizontalTextAlignment
	{
		/// <summary>
		/// Align text in the center of the line.
		/// </summary>
		Center,

		/// <summary>
		/// Distributes the words across the entire line.
		/// </summary>
		Distributed,

		/// <summary>
		/// Align text so it is justified across the line.
		/// </summary>
		Justified,

		/// <summary>
		/// Aligns the text with an adjusted kashida length for Arabic text.
		/// </summary>
		JustifiedLow,

		/// <summary>
		/// Align the text to the left of the line.
		/// </summary>
		Left,

		/// <summary>
		/// Align the text to the right of the line.
		/// </summary>
		Right,

		/// <summary>
		/// Distributes Thai text by treating each character as a word.
		/// </summary>
		ThaiDistributed,
	}

	#endregion  // HorizontalTextAlignment

	#region LengthType






	internal enum LengthType
	{
		EightBit,
		SixteenBit
	}

	#endregion LengthType

	#region ObjectDisplayStyle

	/// <summary>
	/// Represents the various way objects and shapes are displayed in the workbook.
	/// </summary>



	public

		 enum ObjectDisplayStyle
	{
		/// <summary>
		/// All object are be shown.
		/// </summary>
		ShowAll = 0,

		/// <summary>
		/// Placeholders are shown in place of objects.
		/// </summary>
		ShowPlaceholders = 1,

		/// <summary>
		/// No objects or shapes are shown.
		/// </summary>
		HideAll = 2
	}

	#endregion ObjectDisplayStyle

	#region Orientation

	/// <summary>
	/// Represents the page orientations available when a worksheet is printed.
	/// </summary>



	public

		 enum Orientation
	{
		/// <summary>
		/// The page is printed with the larger dimension horizontal.
		/// </summary>
		Landscape = 0,

		/// <summary>
		/// The page is printed with the larger dimension vertical.
		/// </summary>
		Portrait = 1,

        // MBS 7/30/08 - Excel 2007 Format
        /// <summary>
        /// The page is printed with the default setting.
        /// </summary>
        Default = 2,
	}

	#endregion Orientation

	#region PageOrder

	/// <summary>
	/// Represents the ways to order the pages of multiple page worksheets.
	/// </summary>



	public

		 enum PageOrder
	{
		/// <summary>
		/// The first page to print is the top-left page. The next pages printed are below the first page.  
		/// When there are no more pages below, the page to the right of the top-left page is printed, then the pages 
		/// below it, and so on.
		/// </summary>
		DownThenOver = 0,

		/// <summary>
		/// The first page to print is the top-left page. The next pages printed are right of the first page.  
		/// When there are no more pages to the right, the page below the top-left page is printed, then the pages 
		/// to the right of it, and so on.
		/// </summary>
		OverThenDown = 1
	}

	#endregion PageOrder

	#region PageNumbering

	/// <summary>
	/// Represents the way pages are numbered when printed.
	/// </summary>



	public

		 enum PageNumbering
	{
		/// <summary>
		/// Pages are automatically numbered based on the style of the worksheet.
		/// </summary>
		Automatic = 0,

		/// <summary>
		/// The starting page number specified is used for the first page, additional
		/// pages receive a page number that it one greater than the previous page.
		/// </summary>
		UseStartPageNumber = 1
	}

	#endregion PageNumbering

	#region PaneLocation

	/// <summary>
	/// Represents the locations of the various panes which could exist in a multi-pane
	/// view of a worksheet.
	/// </summary>
	// MD 1/2/08 - BR28895
	// Uncommented enum but changed visibility to internal because we don't want to expose it yet
	//public enum PaneLocation
	internal enum PaneLocation
	{
		/// <summary>
		/// The bottom-right pane of the worksheet. This location is only valid if the worksheet
		/// is split both horizontally and veritcally.
		/// </summary>
		BottomRight = 0,

		/// <summary>
		/// The top-right pane of the worksheet. This location is only valid if the worksheet
		/// is split vertically. If the worksheet is only has left and right panes, this is the 
		/// location of the right pane.
		/// </summary>
		TopRight = 1,

		/// <summary>
		/// The bottom-left pane of the worksheet. This location is only valid is the worksheet
		/// is split horizontally. If the worksheet only has top and bottom panes, this is the 
		/// location of the bottom pane.
		/// </summary>
		BottomLeft = 2,

		/// <summary>
		/// This top-left pane of the worksheet. This location is always valid. If the worksheet
		/// does not have any pane splits, this is the only pane. If the worksheet only has top
		/// and bottom panes, this is the location of the top pane. If the worksheet only has
		/// left and right panes, this is the location of the left pane.
		/// </summary>
		TopLeft = 3
	}

	#endregion PaneLocation

	#region PaperSize

	/// <summary>
	/// Represents the various paper sizes available for printing.
	/// </summary>



	public

		 enum PaperSize
	{
		/// <summary>
		/// Undefined
		/// </summary>
		Undefined = 0,

		/// <summary>
		/// Letter 8 1/2\" x 11\"
		/// </summary>
		Letter = 1,

		/// <summary>
		/// Letter small 8 1/2\" x 11\"
		/// </summary>
		LetterSmall = 2,

		/// <summary>
		/// Tabloid 11\" x 17\"
		/// </summary>
		Tabloid = 3,

		/// <summary>
		/// Ledger 17\" x 11\"
		/// </summary>
		Ledger = 4,

		/// <summary>
		/// Legal 8 1/2\" x 14\"
		/// </summary>
		Legal = 5,

		/// <summary>
		/// Statement 5 1/2\" x 8 1/2\"
		/// </summary>
		Statement = 6,

		/// <summary>
		/// Executive 7 1/4\" x 10 1/2\"
		/// </summary>
		Executive = 7,

		/// <summary>
		/// A3 297mm x 420mm
		/// </summary>
		A3 = 8,

		/// <summary>
		/// A4 210mm x 297mm
		/// </summary>
		A4 = 9,

		/// <summary>
		/// A4 small 210mm x 297mm
		/// </summary>
		A4Small = 10,

		/// <summary>
		/// A5 148mm x 210mm
		/// </summary>
		A5 = 11,

		/// <summary>
		/// B4 (JIS) 257mm x 364mm
		/// </summary>
		B4JIS = 12,

		/// <summary>
		/// B5 (JIS) 182mm x 257mm
		/// </summary>
		B5JIS = 13,

		/// <summary>
		/// Folio 8 1/2\" x 13\"
		/// </summary>
		Folio = 14,

		/// <summary>
		/// Quarto 215mm x 275mm
		/// </summary>
		Quarto = 15,

		/// <summary>
		/// 10x14 10\" x 14\"
		/// </summary>
		Size10x14 = 16,

		/// <summary>
		/// 11x17 11\" x 17\"
		/// </summary>
		Size11x17 = 17,

		/// <summary>
		/// Note 8 1/2\" x 11\"
		/// </summary>
		Note = 18,

		/// <summary>
		/// Envelope #9 3 7/8\" x 8 7/8\"
		/// </summary>
		Envelope9 = 19,

		/// <summary>
		/// Envelope #10 4 1/8\" x 9 1/2\"
		/// </summary>
		Envelope10 = 20,

		/// <summary>
		/// Envelope #11 4 1/2\" x 10 3/8\"
		/// </summary>
		Envelope11 = 21,

		/// <summary>
		/// Envelope #12 4 3/4\" x 11\"
		/// </summary>
		Envelope12 = 22,

		/// <summary>
		/// Envelope #14 5\" x 11 1/2\"
		/// </summary>
		Envelope14 = 23,

		/// <summary>
		/// C 17\" x 22\"
		/// </summary>
		C = 24,

		/// <summary>
		/// D 22\" x 34\"
		/// </summary>
		D = 25,

		/// <summary>
		/// E 34\" x 44\"
		/// </summary>
		E = 26,

		/// <summary>
		/// Envelope DL 110mm x 220mm
		/// </summary>
		EnvelopeDL = 27,

		/// <summary>
		/// Envelope C5 162mm x 229mm
		/// </summary>
		EnvelopeC5 = 28,

		/// <summary>
		/// Envelope C3 324mm x 458mm
		/// </summary>
		EnvelopeC3 = 29,

		/// <summary>
		/// Envelope C4 229mm x 324mm
		/// </summary>
		EnvelopeC4 = 30,

		/// <summary>
		/// Envelope C6 114mm x 162mm
		/// </summary>
		EnvelopeC6 = 31,

		/// <summary>
		/// Envelope C6/C5 114mm x 229mm
		/// </summary>
		EnvelopeC6C5 = 32,

		/// <summary>
		/// B4 (ISO) 250mm x 353mm
		/// </summary>
		B4ISO_1 = 33,

		/// <summary>
		/// B5 (ISO) 176mm x 250mm
		/// </summary>
		B5ISO = 34,

		/// <summary>
		/// B6 (ISO) 125mm x 176mm
		/// </summary>
		B6ISO = 35,

		/// <summary>
		/// Envelope Italy 110mm x 230mm
		/// </summary>
		EnvelopeItaly = 36,

		/// <summary>
		/// Envelope Monarch 3 7/8\" x 7 1/2\"
		/// </summary>
		EnvelopeMonarch = 37,

		/// <summary>
		/// 6 3/4 Envelope 3 5/8\" x 6 1/2\"
		/// </summary>
		Size634Envelope = 38,

		/// <summary>
		/// US Standard Fanfold 14 7/8\" x 11\"
		/// </summary>
		USStandardFanfold = 39,

		/// <summary>
		/// German Std. Fanfold 8 1/2\" x 12\"
		/// </summary>
		GermanStandardFanfold = 40,

		/// <summary>
		/// German Legal Fanfold 8 1/2\" x 13\"
		/// </summary>
		GermanLegalFanfold = 41,

		/// <summary>
		/// B4 (ISO) 250mm x 353mm
		/// </summary>
		B4ISO_2 = 42,

		/// <summary>
		/// Japanese Postcard 100mm x 148mm
		/// </summary>
		JapanesePostcard = 43,

		/// <summary>
		/// 9x11 9\" x 11\"
		/// </summary>
		Size9x11 = 44,

		/// <summary>
		/// 10x11 10\" x 11\"
		/// </summary>
		Size10x11 = 45,

		/// <summary>
		/// 15x11 15\" x 11\"
		/// </summary>
		Size15x11 = 46,

		/// <summary>
		/// Envelope Invite 220mm x 220mm
		/// </summary>
		EnvelopeInvite = 47,

		/// <summary>
		/// Letter Extra 9 1/2\" x 12\"
		/// </summary>
		LetterExtra = 50,

		/// <summary>
		/// Legal Extra 9 1/2\" x 15\"
		/// </summary>
		LegalExtra = 51,

		/// <summary>
		/// Tabloid Extra 11 11/16\" x 18\"
		/// </summary>
		TabloidExtra = 52,

		/// <summary>
		/// A4 Extra 235mm x 322mm
		/// </summary>
		A4Extra = 53,

		/// <summary>
		/// Letter Transverse 8 1/2\" x 11\"
		/// </summary>
		LetterTransverse = 54,

		/// <summary>
		/// A4 Transverse 210mm x 297mm
		/// </summary>
		A4Transverse = 55,

		/// <summary>
		/// Letter Extra Transv. 9 1/2\" x 12\"
		/// </summary>
		LetterExtraTransverse = 56,

		/// <summary>
		/// Super A/A4 227mm x 356mm
		/// </summary>
		SuperAA4 = 57,

		/// <summary>
		/// Super B/A3 305mm x 487mm
		/// </summary>
		SuperBA3 = 58,

		/// <summary>
		/// Letter Plus 8 1/2\" x 12 11/16\"
		/// </summary>
		LetterPlus = 59,

		/// <summary>
		/// A4 Plus 210mm x 330mm
		/// </summary>
		A4Plus = 60,

		/// <summary>
		/// A5 Transverse 148mm x 210mm
		/// </summary>
		A5Transverse = 61,

		/// <summary>
		/// B5 (JIS) Transverse 182mm x 257mm
		/// </summary>
		B5JISTransverse = 62,

		/// <summary>
		/// A3 Extra 322mm x 445mm
		/// </summary>
		A3Extra = 63,

		/// <summary>
		/// A5 Extra 174mm x 235mm
		/// </summary>
		A5Extra = 64,

		/// <summary>
		/// B5 (ISO) Extra 201mm x 276mm
		/// </summary>
		B5ISOExtra = 65,

		/// <summary>
		/// A2 420mm x 594mm
		/// </summary>
		A2 = 66,

		/// <summary>
		/// A3 Transverse 297mm x 420mm
		/// </summary>
		A3Transverse = 67,

		/// <summary>
		/// A3 Extra Transverse 322mm x 445mm
		/// </summary>
		A3ExtraTransverse = 68,

		/// <summary>
		/// Dbl. Japanese Postcard 200mm x 148mm
		/// </summary>
		DblJapanesePostcard = 69,

		/// <summary>
		/// A6 105mm x 148mm
		/// </summary>
		A6 = 70,

		/// <summary>
		/// Letter Rotated 11\" x 8 1/2\"
		/// </summary>
		LetterRotated = 75,

		/// <summary>
		/// A3 Rotated 420mm x 297mm
		/// </summary>
		A3Rotated = 76,

		/// <summary>
		/// A4 Rotated 297mm x 210mm
		/// </summary>
		A4Rotated = 77,

		/// <summary>
		/// A5 Rotated 210mm x 148mm
		/// </summary>
		A5Rotated = 78,

		/// <summary>
		/// B4 (JIS) Rotated 364mm x 257mm
		/// </summary>
		B4JISRotated = 79,

		/// <summary>
		/// B5 (JIS) Rotated 257mm x 182mm
		/// </summary>
		B5JISRotated = 80,

		/// <summary>
		/// Japanese Postcard Rot. 148mm x 100mm
		/// </summary>
		JapanesePostcardRotated = 81,

		/// <summary>
		/// Dbl. Jap. Postcard Rot. 148mm x 200mm
		/// </summary>
		DblJapanesePostcardRotated = 82,

		/// <summary>
		/// A6 Rotated 148mm x 105mm
		/// </summary>
		A6Rotated = 83,

		/// <summary>
		/// B6 (JIS) 128mm x 182mm
		/// </summary>
		B6JIS = 88,

		/// <summary>
		/// B6 (JIS) Rotated 182mm x 128mm
		/// </summary>
		B6JISRotated = 89,

		/// <summary>
		/// 12x11 12\" x 11\"
		/// </summary>
		Size12x11 = 90
	}

	#endregion PaperSize

	// MD 3/24/10 - TFS28374
	#region PositioningOptions

	/// <summary>
	/// Represents the options available for getting or setting the bounds of a shape, cell, or region.
	/// </summary>
	[Flags]



	public

		 enum PositioningOptions
	{
		/// <summary>
		/// No special options should be used. Get and set actual bounds on the worksheet in its current state.
		/// </summary>
		None = 0,

		/// <summary>
		/// Ignore the <see cref="RowColumnBase.Hidden"/> value on all rows and columns. 
		/// Get and set bounds as if all rows and columns were currently visible.
		/// </summary>
		IgnoreHiddenRowsAndColumns = 1,
	}

	#endregion // PositioningOptions

	#region Precision

	/// <summary>
	/// Represents the types of precision which can be used when obtaining the value of a cell.
	/// </summary>



	public

		 enum Precision
	{
		/// <summary>
		/// The display value of the cell is used. If the cell's actual value is 10.005, but it is using currency formatting,
		/// it will display as $10.01.  When this cell is used in calculations, its displayed value of 10.01 will be used.
		/// </summary>
		UseDisplayValues = 0,

		/// <summary>
		/// The actual value of the cell is used. If the cell's actual value is 10.005, but it is using currency formatting,
		/// it will display as $10.01.  When this cell is used in calculations, its stored value of 10.005 will be used, 
		/// even though the display shows a slightly different value.
		/// </summary>
		UseRealCellValues = 1
	}

	#endregion Precision

	// MD 7/14/11 - Shape support
	#region PredefinedShapeType

	/// <summary>
	/// Represents the shape types that are predefined in Microsoft Excel.
	/// </summary>



	[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelShapesSupport)] 
	public

		enum PredefinedShapeType
	{
		/// <summary>
		/// Represents the <see cref="PredefinedShapes.DiamondShape"/> shape.
		/// </summary>
		Diamond = ShapeType.Diamond,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.EllipseShape"/> shape.
		/// </summary>
		Ellipse = ShapeType.Ellipse,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.HeartShape"/> shape.
		/// </summary>
		Heart = ShapeType.Heart,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.IrregularSeal1Shape"/> shape.
		/// </summary>
		IrregularSeal1 = ShapeType.IrregularSeal1,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.IrregularSeal2Shape"/> shape.
		/// </summary>
		IrregularSeal2 = ShapeType.IrregularSeal2,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.LightningBoltShape"/> shape.
		/// </summary>
		LightningBolt = ShapeType.LightningBolt,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.LineShape"/> shape.
		/// </summary>
		Line = ShapeType.Line,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.PentagonShape"/> shape.
		/// </summary>
		Pentagon = ShapeType.Pentagon,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.RectangleShape"/> shape.
		/// </summary>
		Rectangle = ShapeType.Rectangle,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.RightTriangleShape"/> shape.
		/// </summary>
		RightTriangle = ShapeType.RightTriangle,

		/// <summary>
		/// Represents the <see cref="PredefinedShapes.StraightConnector1Shape"/> shape.
		/// </summary>
		StraightConnector1 = ShapeType.StraightConnector1,
	}

	#endregion // PredefinedShapeType

	#region PrintErrors

	/// <summary>
	/// Represents the various ways to print cell errors in a worksheet.
	/// </summary>



	public

		 enum PrintErrors
	{
		/// <summary>
		/// Errors are printed just as they are displayed on the worksheet.
		/// </summary>
		PrintAsDisplayed = 0,

		/// <summary>
		/// Errors are not prints, as though the cells containing them have no value.
		/// </summary>
		DontPrint = 1,

		/// <summary>
		/// Errors are printed as two dashes "--".
		/// </summary>
		PrintAsDashes = 2,

		/// <summary>
		/// Errors are prints as "#N/A".
		/// </summary>
		PrintAsNA = 3
	}

	#endregion PrintErrors

	#region PrintNotes

	/// <summary>
	/// Represents the various ways to print cell notes.
	/// </summary>



	public

		 enum PrintNotes
	{
		/// <summary>
		/// Cell notes are not printed.
		/// </summary>
		DontPrint = 0,

		/// <summary>
		/// Cell notes are printed as they are shown on the worksheet. With this option, cell
		/// notes will only appear in the printed worksheet if they are displayed on the worksheet
		/// in Microsoft Excel. If the notes just show indicators in Excel, the indicators and notes 
		/// will not be printed. 
		/// </summary>
		PrintAsDisplayed = 1,

		/// <summary>
		/// Cell notes are printed on the last page, after the entire worksheet has printed.
		/// </summary>
		PrintAtEndOfSheet = 2
	}

	#endregion PrintNotes

	// MD 3/4/12 - 12.1 - Table Support
	#region ReferenceShiftType

	internal enum ReferenceShiftType
	{






		MaintainReference,







		MaintainRelativeReferenceOffset,
	}

	#endregion // ReferenceShiftType

	#region ScalingType

	/// <summary>
	/// Represents the ways to scale a worksheet when it is printed.
	/// </summary>



	public

		 enum ScalingType
	{
		/// <summary>
		/// The scaling factor is used to scale the worksheet when printing.
		/// </summary>
		UseScalingFactor = 0,

		/// <summary>
		/// The page maximums are used to determine how many pages the worksheet can be printed on.
		/// Less pages can be used if there is not enough printable content in the worksheet.
		/// </summary>
		FitToPages = 1
	}

	#endregion ScalingType

	#region ScrollBars

	/// <summary>
	/// Represents the various scroll bar configurations available for the workbook.
	/// </summary>
	[Flags]



	public

		 enum ScrollBars
	{
		/// <summary>
		/// No scroll bars are shown in Microsoft Excel.
		/// </summary>
		None = 0,

		/// <summary>
		/// Only the horizontal scroll bar is shown in Microsoft Excel.
		/// </summary>
		Horizontal = 1,

		/// <summary>
		/// Only the vertical scroll bar is shown in Microsoft Excel.
		/// </summary>
		Vertical = 2,

		/// <summary>
		/// Both scroll bars are shown in Microsoft Excel.
		/// </summary>
		Both = 3
	}

	#endregion ScrollBars

	#region ShapePositioningMode

	/// <summary>
	/// Represents the ways shapes will be repositioned when rows and columns are resized.
	/// </summary>



	public

		 enum ShapePositioningMode
	{
		/// <summary>
		/// Shapes will move and size with the cells. If columns before (or rows above) the shape are expanded,
		/// the shape will shift left. If columns within a shape are expanded, the shape will be widened.
		/// </summary>
		MoveAndSizeWithCells = 0,

		/// <summary>
		/// Shapes will move but not size with the cells. If columns before (or rows above) the shape are
		/// expanded, the shape will shift left. If columns within a shape are expanded, the shape will not
		/// be widened.
		/// </summary>
		MoveWithCells = 2,

		/// <summary>
		/// Shapes will not move or size with the cells. The shape will remain in its absolute pixel position of
		/// the worksheet, regardless the rows and columns resized before or inside it.
		/// </summary>
		DontMoveOrSizeWithCells = 3,
	}

	#endregion ShapePositioningMode

	// MD 7/20/2007 - BR25039
	// Moved constants from the EscherRecords.Shape class to this enum
	#region ShapeType

	internal enum ShapeType
	{
		NotPrimitive = 0,
		Rectangle = 1,
		RoundRectangle = 2,
		Ellipse = 3,
		Diamond = 4,
		IsocelesTriangle = 5,
		RightTriangle = 6,
		Parallelogram = 7,
		Trapezoid = 8,
		Hexagon = 9,
		Octagon = 10,
		Plus = 11,
		Star = 12,
		Arrow = 13,
		ThickArrow = 14,
		HomePlate = 15,
		Cube = 16,
		Balloon = 17,
		Seal = 18,
		Arc = 19,
		Line = 20,
		Plaque = 21,
		Can = 22,
		Donut = 23,
		TextSimple = 24,
		TextOctagon = 25,
		TextHexagon = 26,
		TextCurve = 27,
		TextWave = 28,
		TextRing = 29,
		TextOnCurve = 30,
		TextOnRing = 31,
		StraightConnector1 = 32,
		BentConnector2 = 33,
		BentConnector3 = 34,
		BentConnector4 = 35,
		BentConnector5 = 36,
		CurvedConnector2 = 37,
		CurvedConnector3 = 38,
		CurvedConnector4 = 39,
		CurvedConnector5 = 40,
		Callout1 = 41,
		Callout2 = 42,
		Callout3 = 43,
		AccentCallout1 = 44,
		AccentCallout2 = 45,
		AccentCallout3 = 46,
		BorderCallout1 = 47,
		BorderCallout2 = 48,
		BorderCallout3 = 49,
		AccentBorderCallout1 = 50,
		AccentBorderCallout2 = 51,
		AccentBorderCallout3 = 52,
		Ribbon = 53,
		Ribbon2 = 54,
		Chevron = 55,
		Pentagon = 56,
		NoSmoking = 57,
		Seal8 = 58,
		Seal16 = 59,
		Seal32 = 60,
		WedgeRectCallout = 61,
		WedgeRRectCallout = 62,
		WedgeEllipseCallout = 63,
		Wave = 64,
		FoldedCorner = 65,
		LeftArrow = 66,
		DownArrow = 67,
		UpArrow = 68,
		LeftRightArrow = 69,
		UpDownArrow = 70,
		IrregularSeal1 = 71,
		IrregularSeal2 = 72,
		LightningBolt = 73,
		Heart = 74,
		PictureFrame = 75,
		QuadArrow = 76,
		LeftArrowCallout = 77,
		RightArrowCallout = 78,
		UpArrowCallout = 79,
		DownArrowCallout = 80,
		LeftRightArrowCallout = 81,
		UpDownArrowCallout = 82,
		QuadArrowCallout = 83,
		Bevel = 84,
		LeftBracket = 85,
		RightBracket = 86,
		LeftBrace = 87,
		RightBrace = 88,
		LeftUpArrow = 89,
		BentUpArrow = 90,
		BentArrow = 91,
		Seal24 = 92,
		StripedRightArrow = 93,
		NotchedRightArrow = 94,
		BlockArc = 95,
		SmileyFace = 96,
		VerticalScroll = 97,
		HorizontalScroll = 98,
		CircularArrow = 99,
		NotchedCircularArrow = 100,
		UturnArrow = 101,
		CurvedRightArrow = 102,
		CurvedLeftArrow = 103,
		CurvedUpArrow = 104,
		CurvedDownArrow = 105,
		CloudCallout = 106,
		EllipseRibbon = 107,
		EllipseRibbon2 = 108,
		FlowChartProcess = 109,
		FlowChartDecision = 110,
		FlowChartInputOutput = 111,
		FlowChartPredefinedProcess = 112,
		FlowChartInternalStorage = 113,
		FlowChartDocument = 114,
		FlowChartMultidocument = 115,
		FlowChartTerminator = 116,
		FlowChartPreparation = 117,
		FlowChartManualInput = 118,
		FlowChartManualOperation = 119,
		FlowChartConnector = 120,
		FlowChartPunchedCard = 121,
		FlowChartPunchedTape = 122,
		FlowChartSummingJunction = 123,
		FlowChartOr = 124,
		FlowChartCollate = 125,
		FlowChartSort = 126,
		FlowChartExtract = 127,
		FlowChartMerge = 128,
		FlowChartOfflineStorage = 129,
		FlowChartOnlineStorage = 130,
		FlowChartMagneticTape = 131,
		FlowChartMagneticDisk = 132,
		FlowChartMagneticDrum = 133,
		FlowChartDisplay = 134,
		FlowChartDelay = 135,
		TextPlainText = 136,
		TextStop = 137,
		TextTriangle = 138,
		TextTriangleInverted = 139,
		TextChevron = 140,
		TextChevronInverted = 141,
		TextRingInside = 142,
		TextRingOutside = 143,
		TextArchUpCurve = 144,
		TextArchDownCurve = 145,
		TextCircleCurve = 146,
		TextButtonCurve = 147,
		TextArchUpPour = 148,
		TextArchDownPour = 149,
		TextCirclePour = 150,
		TextButtonPour = 151,
		TextCurveUp = 152,
		TextCurveDown = 153,
		TextCascadeUp = 154,
		TextCascadeDown = 155,
		TextWave1 = 156,
		TextWave2 = 157,
		TextWave3 = 158,
		TextWave4 = 159,
		TextInflate = 160,
		TextDeflate = 161,
		TextInflateBottom = 162,
		TextDeflateBottom = 163,
		TextInflateTop = 164,
		TextDeflateTop = 165,
		TextDeflateInflate = 166,
		TextDeflateInflateDeflate = 167,
		TextFadeRight = 168,
		TextFadeLeft = 169,
		TextFadeUp = 170,
		TextFadeDown = 171,
		TextSlantUp = 172,
		TextSlantDown = 173,
		TextCanUp = 174,
		TextCanDown = 175,
		FlowChartAlternateProcess = 176,
		FlowChartOffpageConnector = 177,
		Callout90 = 178,
		AccentCallout90 = 179,
		BorderCallout90 = 180,
		AccentBorderCallout90 = 181,
		LeftRightUpArrow = 182,
		Sun = 183,
		Moon = 184,
		BracketPair = 185,
		BracePair = 186,
		Seal4 = 187,
		DoubleWave = 188,
		ActionButtonBlank = 189,
		ActionButtonHome = 190,
		ActionButtonHelp = 191,
		ActionButtonInformation = 192,
		ActionButtonForwardNext = 193,
		ActionButtonBackPrevious = 194,
		ActionButtonEnd = 195,
		ActionButtonBeginning = 196,
		ActionButtonReturn = 197,
		ActionButtonDocument = 198,
		ActionButtonSound = 199,
		ActionButtonMovie = 200,
		HostControl = 201,
		TextBox = 202
	}

	#endregion ShapeType

	// MD 12/19/11 - 12.1 - Table Support
	#region StructuredTableReferenceKeywordType

	internal enum StructuredTableReferenceKeywordType
	{
		All,
		Data,
		Headers,
		Totals,
		ThisRow,
	}

	#endregion // StructuredTableReferenceKeywordType

	// MD 2/6/12 - 12.1 - Cell Format Updates
	#region StyleCategory

	internal enum StyleCategory
	{
		Custom = 0,
		GoodBadNeutral = 1,
		DataModel = 2,
		TitleAndHeading = 3,
		Themed = 4,
		NumberFormat = 5
	}

	#endregion // StyleCategory

	// MD 2/13/12 - 12.1 - Table Support
	#region TextFormatMode




	/// <summary>
	/// Represents the various way to combine a cell's value and format string to get its text.
	/// </summary>
	/// <seealso cref="WorksheetCell.GetText()"/>
	/// <seealso cref="WorksheetCell.GetText(TextFormatMode)"/>
	/// <seealso cref="WorksheetRow.GetCellText(int)"/>
	/// <seealso cref="WorksheetRow.GetCellText(int,TextFormatMode)"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 enum TextFormatMode
	{
		/// <summary>
		/// Format the cell text as it is displayed in the Microsoft Excel UI. This takes into account the cell width when 
		/// formatting the text.
		/// </summary>
		AsDisplayed,

		/// <summary>
		/// Format the cell text as if it had unlimited space in the cell. However, this will not include padding characters 
		/// from the format string.
		/// </summary>
		IgnoreCellWidth
	}

	#endregion // TextFormatMode

	#region WorksheetCellFormatOptions

	// MD 12/30/11 - 12.1 - Cell Format Updates
	// This is now public and renamed so we can expose it from the IWorksheetCellFormat.FormatOptions property.
	//[Flags]
	//internal enum StyleCellFormatOptions
	//{
	//    None = 0,
	//    UseNumberFormatting = 1,
	//    UseAlignmentFormatting = 2,
	//    UseFontFormatting = 4,
	//    UseBorderFormatting = 8,
	//    UsePatternsFormatting = 16,
	//    UseProtectionFormatting = 32,
	//}




	/// <summary>
	/// Flagged enumeration which indicates which groups of formatting properties are used in a format.
	/// </summary>
	/// <seealso cref="IWorksheetCellFormat.FormatOptions"/>
	[Flags]
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

		enum WorksheetCellFormatOptions
	{
		/// <summary>
		/// No format properties are used on the format.
		/// </summary>
		None = 0,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.FormatString"/> property is used on the format.
		/// </summary>
		ApplyNumberFormatting = 1,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.Alignment"/>, <see cref="IWorksheetCellFormat.Indent"/>, <see cref="IWorksheetCellFormat.Rotation"/>, 
		/// <see cref="IWorksheetCellFormat.ShrinkToFit"/>, <see cref="IWorksheetCellFormat.VerticalAlignment"/>, and 
		/// <see cref="IWorksheetCellFormat.WrapText"/> properties are used on the format.
		/// </summary>
		ApplyAlignmentFormatting = 2,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.Font"/> property is used on the format.
		/// </summary>
		ApplyFontFormatting = 4,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.BottomBorderColorInfo"/>, <see cref="IWorksheetCellFormat.BottomBorderStyle"/>, 
		/// <see cref="IWorksheetCellFormat.DiagonalBorderColorInfo"/>, <see cref="IWorksheetCellFormat.DiagonalBorders"/>, 
		/// <see cref="IWorksheetCellFormat.DiagonalBorderStyle"/>, <see cref="IWorksheetCellFormat.LeftBorderColorInfo"/>, 
		/// <see cref="IWorksheetCellFormat.LeftBorderStyle"/>, <see cref="IWorksheetCellFormat.RightBorderColorInfo"/>, 
		/// <see cref="IWorksheetCellFormat.RightBorderStyle"/>, <see cref="IWorksheetCellFormat.TopBorderColorInfo"/>, and 
		/// <see cref="IWorksheetCellFormat.TopBorderStyle"/> properties are used on the format.
		/// </summary>
		ApplyBorderFormatting = 8,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.Fill"/> property is used on the format.
		/// </summary>
		ApplyFillFormatting = 16,

		/// <summary>
		/// The <see cref="IWorksheetCellFormat.Locked"/> property is used on the format.
		/// </summary>
		ApplyProtectionFormatting = 32,

		/// <summary>
		/// All properties are used on the format.
		/// </summary>
		All = ApplyNumberFormatting |
			ApplyAlignmentFormatting |
			ApplyFontFormatting |
			ApplyBorderFormatting |
			ApplyFillFormatting |
			ApplyProtectionFormatting,
	}

	#endregion // WorksheetCellFormatOptions

	#region VerticalCellAlignment

	/// <summary>
	/// Enumeration for vertical alignment styles. Default value is used in property override situations.
	/// </summary>



	public

		 enum VerticalCellAlignment
	{
		/// <summary>
		/// Use the current default.
		/// </summary>
		Default = -1,

		/// <summary>
		/// Top alignment.
		/// </summary>
		Top = 0,

		/// <summary>
		/// Center alignment.
		/// </summary>
		Center = 1,

		/// <summary>
		/// Bottom alignment.
		/// </summary>
		Bottom = 2,

		/// <summary>
		/// Justify alignment.
		/// </summary>
		Justify = 3,

		// MD 7/7/08 - BR34589
		/// <summary>
		/// Distributed alignment.
		/// </summary>
		Distributed = 4,
	}

	#endregion VerticalCellAlignment

	// MD 11/8/11 - TFS85193
	#region VerticalTextAlignment

	/// <summary>
	/// Represents the various vertical text alignment types.
	/// </summary>



	public

		enum VerticalTextAlignment
	{
		/// <summary>
		/// Align the text to the bottom of the available area.
		/// </summary>
		Bottom,

		/// <summary>
		/// Align the center to the bottom of the available area.
		/// </summary>
		Center,

		// These two don't seem to work correctly for shapes, so let's leave them out until we find a case where they work.
		///// <summary>
		///// Align the text so it is distributed vertically across the available area.
		///// </summary>
		//Distributed,
		//
		///// <summary>
		///// Align the text so it is justified vertically in the available area.
		///// </summary>
		//Justified,

		/// <summary>
		/// Align the text to the top of the available area.
		/// </summary>
		Top,
	}

	#endregion  // VerticalTextAlignment

	// MD 6/20/08 - Excel 2007 Format
	#region WorkbookFormat

	/// <summary>
	/// Represents the various file formats in which a workbook can be saved.
	/// </summary>



	public

		 enum WorkbookFormat
	{
		// XLS format docs: http://msdn.microsoft.com/en-us/library/cc313154(v=office.12).aspx
		// XLS format index: http://msdn.microsoft.com/en-us/library/dd945789(office.12).aspx
		/// <summary>
		/// The Excel 97-2003 BIFF8 file format.
		/// </summary>
		Excel97To2003,

		// MD 5/7/10 - 10.2 - Excel Templates
		/// <summary>
		/// The Excel 97-2003 Template BIFF8 file format.
		/// </summary>
		Excel97To2003Template,

		/// <summary>
		/// The Excel 2007 XML file format.
		/// </summary>
		Excel2007,

		// MD 10/1/08 - TFS8471
		/// <summary>
		/// The Excel 2007 Macro-Enabled XML file format.
		/// </summary>
		Excel2007MacroEnabled,

		// MD 5/7/10 - 10.2 - Excel Templates
		/// <summary>
		/// The Excel 2007 Macro-Enabled Template XML file format.
		/// </summary>
		Excel2007MacroEnabledTemplate,

		// MD 5/7/10 - 10.2 - Excel Templates
		/// <summary>
		/// The Excel 2007 Template XML file format.
		/// </summary>
		Excel2007Template,

		///// <summary>
		///// The Excel 2007 binary file format.
		///// </summary>
		//Excel2007Binary,
	} 

	#endregion WorkbookFormat

	// MRS 6/28/05 - BR04756
	#region WorkbookPaletteMode

	/// <summary>
	/// Obsolete. The WorkbookPaletteMode enumeration is no longer used.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
	[Obsolete("The WorkbookPaletteMode has been deprecated.")] // MD 1/16/12 - 12.1 - Cell Format Updates



	public

		 enum WorkbookPaletteMode
	{
		/// <summary>
		/// Use a custom palette based on the actual colors.
		/// </summary>
		CustomPalette,

		/// <summary>
		/// Use the standard Excel palette by matching the nearest color. 
		/// </summary>
		StandardPalette
	}

	#endregion WorkbookPaletteMode

	// MD 1/16/12 - 12.1 - Cell Format Updates
	#region WorkbookThemeColorType




	/// <summary>
	/// Represents the various theme colors in a workbook.
	/// </summary>
	/// <seealso cref="WorkbookColorInfo.ThemeColorType"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]
	public

		enum WorkbookThemeColorType
	{
		/// <summary>
		/// Represents the Text/Background - Light 1 theme color.
		/// </summary>
		Light1 = 0,

		/// <summary>
		/// Represents the Text/Background - Dark 1 theme color.
		/// </summary>
		Dark1 = 1,

		/// <summary>
		/// Represents the Text/Background - Light 2 theme color.
		/// </summary>
		Light2 = 2,

		/// <summary>
		/// Represents the Text/Background - Dark 2 theme color.
		/// </summary>
		Dark2 = 3,

		/// <summary>
		/// Represents the Accent 1 theme color.
		/// </summary>
		Accent1 = 4,

		/// <summary>
		/// Represents the Accent 2 theme color.
		/// </summary>
		Accent2 = 5,

		/// <summary>
		/// Represents the Accent 3 theme color.
		/// </summary>
		Accent3 = 6,

		/// <summary>
		/// Represents the Accent 4 theme color.
		/// </summary>
		Accent4 = 7,

		/// <summary>
		/// Represents the Accent 5 theme color.
		/// </summary>
		Accent5 = 8,

		/// <summary>
		/// Represents the Accent 6 theme color.
		/// </summary>
		Accent6 = 9,

		/// <summary>
		/// Represents the Hyperlink theme color.
		/// </summary>
		Hyperlink = 10,

		/// <summary>
		/// Represents the Followed Hyperlink theme color.
		/// </summary>
		FollowedHyperlink = 11
	}

	#endregion // WorkbookThemeColorType

	// MD 12/13/11 - 12.1 - Table Support
	#region WorksheetTableArea




	/// <summary>
	/// Represents the various areas which can have a format applied at the table level.
	/// </summary>
	/// <seealso cref="WorksheetTable.AreaFormats"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		enum WorksheetTableArea
	{
		/// <summary>
		/// The format is applied to the entire table. Only the outer border properties can be set on this area format.
		/// Setting any other will cause an exception.
		/// </summary>
		WholeTable,

		/// <summary>
		/// The format is applied to the data area of the table.
		/// </summary>
		DataArea,

		/// <summary>
		/// The format is applied to the header row of the table. All but the top border properties can be set on this 
		/// area format. Setting the top border properties will cause an exception.
		/// </summary>
		HeaderRow,

		/// <summary>
		/// The format is applied to the totals row of the table. All but the bottom border properties can be set on this 
		/// area format. Setting the bottom border properties will cause an exception.
		/// </summary>
		TotalsRow,
	}

	#endregion // WorksheetTableArea

	// MD 12/13/11 - 12.1 - Table Support
	#region WorksheetTableColumnArea




	/// <summary>
	/// Represents the various areas which can have a format applied at the table column level.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.AreaFormats"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		enum WorksheetTableColumnArea
	{
		/// <summary>
		/// The format is applied to the data area of the table column.
		/// </summary>
		DataArea,

		/// <summary>
		/// The format is applied to the header cell of the table column.
		/// </summary>
		HeaderCell,

		/// <summary>
		/// The format is applied to the total cell of the table column.
		/// </summary>
		TotalCell,
	}

	#endregion // WorksheetTableColumnArea

	// MD 12/13/11 - 12.1 - Table Support
	#region WorksheetTableStyleArea




	/// <summary>
	/// Represents the various areas which can have a format applied at the table style level.
	/// </summary>
	/// <remarks>
	/// <p class="note">
	/// <b>Note:</b> Only certain properties can be set on the table style are formats. The are as follows:
	/// <list type="bullet">
	/// <item><see cref="IWorksheetCellFormat.BottomBorderColorInfo"/></item>
	/// <item><see cref="IWorksheetCellFormat.BottomBorderStyle"/></item>
	/// <item><see cref="IWorksheetCellFormat.Fill"/></item>
	/// <item><see cref="IWorksheetCellFormat.LeftBorderColorInfo"/></item>
	/// <item><see cref="IWorksheetCellFormat.LeftBorderStyle"/></item>
	/// <item><see cref="IWorksheetCellFormat.RightBorderColorInfo"/></item>
	/// <item><see cref="IWorksheetCellFormat.RightBorderStyle"/></item>
	/// <item><see cref="IWorksheetCellFormat.TopBorderColorInfo"/></item>
	/// <item><see cref="IWorksheetCellFormat.TopBorderStyle"/></item>
	/// <item><see cref="IWorkbookFont.Bold">Font.Bold</see></item>
	/// <item><see cref="IWorkbookFont.ColorInfo">Font.ColorInfo</see></item>
	/// <item><see cref="IWorkbookFont.Italic">Font.Italic</see></item>
	/// <item><see cref="IWorkbookFont.Strikeout">Font.Strikeout</see></item>
	/// <item><see cref="IWorkbookFont.UnderlineStyle">Font.UnderlineStyle</see></item>
	/// </list>
	/// Setting any other format or font properties on the table style areas will cause an exception.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

		enum WorksheetTableStyleArea
	{
		/// <summary>
		/// The format is applied to the entire table.
		/// </summary>
		WholeTable = ST_TableStyleType.wholeTable,

		/// <summary>
		/// The format is applied to the odd numbered column stripes in the table.
		/// </summary>
		ColumnStripe = ST_TableStyleType.firstColumnStripe,

		/// <summary>
		/// The format is applied to the even numbered column stripes in the table.
		/// </summary>
		AlternateColumnStripe = ST_TableStyleType.secondColumnStripe,

		/// <summary>
		/// The format is applied to the odd numbered row stripes in the table.
		/// </summary>
		RowStripe = ST_TableStyleType.firstRowStripe,

		/// <summary>
		/// The format is applied to the even numbered row stripes in the table.
		/// </summary>
		AlternateRowStripe = ST_TableStyleType.secondRowStripe,

		/// <summary>
		/// The format is applied to the last column in the table.
		/// </summary>
		LastColumn = ST_TableStyleType.lastColumn,

		/// <summary>
		/// The format is applied to the first column in the table.
		/// </summary>
		FirstColumn = ST_TableStyleType.firstColumn,

		/// <summary>
		/// The format is applied to the header row in the table.
		/// </summary>
		HeaderRow = ST_TableStyleType.headerRow,

		/// <summary>
		/// The format is applied to the totals row in the table.
		/// </summary>
		TotalRow = ST_TableStyleType.totalRow,

		/// <summary>
		/// The format is applied to the first header cell in the table.
		/// </summary>
		FirstHeaderCell = ST_TableStyleType.firstHeaderCell,

		/// <summary>
		/// The format is applied to the last header cell in the table.
		/// </summary>
		LastHeaderCell = ST_TableStyleType.lastHeaderCell,

		/// <summary>
		/// The format is applied to the first total cell in the table.
		/// </summary>
		FirstTotalCell = ST_TableStyleType.firstTotalCell,

		/// <summary>
		/// The format is applied to the last total cell in the table.
		/// </summary>
		LastTotalCell = ST_TableStyleType.lastTotalCell,
	}

	#endregion // WorksheetTableStyleArea

	// MD 2/10/12 - TFS97827
	#region WorksheetColumnWidthUnit




	/// <summary>
	/// Represents the various units in which a column width can be represented.
	/// </summary>
	/// <seealso cref="Worksheet.GetDefaultColumnWidth(WorksheetColumnWidthUnit)"/>
	/// <seealso cref="Worksheet.SetDefaultColumnWidth(double, WorksheetColumnWidthUnit)"/>
	/// <seealso cref="WorksheetColumn.GetWidth(WorksheetColumnWidthUnit)"/>
	/// <seealso cref="WorksheetColumn.SetWidth(double, WorksheetColumnWidthUnit)"/>
	public 

		enum WorksheetColumnWidthUnit
	{
		/// <summary>
		/// The column width is represented in units of the '0' digit character width, including column padding. The digit is 
		/// measured with the default font for the workbook. The padding is a few pixels on either side of the column plus an 
		/// additional pixel for the gridline.
		/// </summary>
		Character,

		/// <summary>
		/// The column width is represented in 256ths of the '0' digit character width, including column padding, which means this 
		/// value will be 256 times the width expressed in Character units. The digit is measured with the default font for the 
		/// workbook. The padding is a few pixels on either side of the column plus an additional pixel for the gridline. These 
		/// units are the units in which the <see cref="WorksheetColumn.Width"/> and <see cref="Worksheet.DefaultColumnWidth"/> 
		/// properties are expressed.
		/// </summary>
		Character256th,

		/// <summary>
		/// The column width is represented in units of the '0' digit character width, excluding padding. The digit is measured with 
		/// the default font for the workbook. These units are the units in which Microsoft Excel displays column widths to the user
		/// and accepts new column widths from the user in the 'Column Width' dialog.
		/// </summary>
		CharacterPaddingExcluded,

		/// <summary>
		/// The column width is represented in pixels.
		/// </summary>
		Pixel,

		/// <summary>
		/// The column width is represented in points.
		/// </summary>
		Point,

		/// <summary>
		/// The column width is represented in twips (20ths of a point).
		/// </summary>
		Twip,
	}

	#endregion // WorksheetColumnWidthUnit

	#region WorksheetView

	/// <summary>
	/// Represents the various views for a worksheet.
	/// </summary>



	public

		 enum WorksheetView
	{
		/// <summary>
		/// The worksheet is displayed in the normal view.
		/// </summary>
		Normal = 0,

		/// <summary>
		/// The worksheet is displayed as it will appear when printed. This view displays where
		/// printed pages will begin and end as well as any headers or footers for the workbook.
		/// This value is only supported in Excel 2007 and defaults to Normal in earlier version.
		/// </summary>
		PageLayout = 1,

		/// <summary>
		/// This view shows a preview of where pages will break when the worksheet is printed.
		/// </summary>
		PageBreakPreview = 2,
	}

	#endregion WorksheetView

	#region WorksheetVisibility

	/// <summary>
	/// Represents the various visibilities of a worksheet.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The worksheet visibility indicates how the worksheet will be displayed in the tab bar at 
	/// the bottom of the workbook window in Microsoft Excel.
	/// </p>
	/// </remarks>



	public

		 enum WorksheetVisibility
	{
		/// <summary>
		/// The worksheet tab is present in the tab bar.
		/// </summary>
		Visible = 0,

		/// <summary>
		/// The worksheet tab is not present in the tab bar. The worksheet can be made visible
		/// from the Unhide dialog in Microsoft Excel.
		/// </summary>
		Hidden = 1,

		/// <summary>
		/// The worksheet tab is not present in the tab bar. The worksheet can only be made visible
		/// again through a Visual Basic procedure in Microsoft Excel. The worksheet can not be made 
		/// visible through the user interface.
		/// </summary>
		StrongHidden = 2,
	}

	#endregion WorksheetVisibility

    #region CommentDisplayStyle
    /// <summary>
    /// Defines options for displaying comments in the user interface.
    /// </summary>



	public

		 enum CommentDisplayStyle
    {
        /// <summary>
        /// Indicates that comments not be shown in the user interface.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that only the comment indicator be shown in the user interface.
        /// </summary>
        Indicator = 1,

        /// <summary>
        /// Indicates that both the comment indicator and comment text be show in the user interface.
        /// </summary>
        IndicatorAndComment = 2,
    }
    #endregion CommentDisplayStyle
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