using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Documents.Excel.Filtering;
using Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords;
using Infragistics.Documents.Excel.Sorting;

namespace Infragistics.Documents.Excel.Serialization.Excel2007
{
	#region DataType





	internal enum DataType
	{
		String,
		ST_CellRef = String,
		ST_Ref = String,

        // Note: This is actually in the format of "A1:C2 F4:G6" or such,
        // but unless we actually need to parse out these values, we should
        // just leave it as a string to minimize overhead
        ST_Sqref = String,
		ST_Xstring = String,

        //  BF 8/14/08
        //ST_UnsignedIntHex = String,

        ST_RelationshipID = String,
		Short,
		Integer,
		Long,
		Int16 = Short,
		Int32 = Integer,
		Int64 = Long,
		Boolean,
		DateTime,
		Single,
		Float = Single,
		Double,
		UShort,
		UInt16 = UShort,
		UInt,
		UInt32 = UInt,

		// MD 12/6/11 - 12.1 - Table Support
		ST_DxfId = UInt,

		ULong,
		UInt64 = ULong,
		Byte,
		SByte,
		Decimal,
		Object,
		ST_VectorBaseType,
		ST_Visibility,
		ST_SheetState = ST_Visibility,
		ST_CalcMode,
		ST_RefMode,
		ST_FontScheme,
		ST_UnderlineValues,
		ST_VerticalAlignRun,
		ST_BorderStyle,
		ST_SheetViewType,
		ST_Pane,
		ST_GradientType,
		ST_PatternType,
		ST_Objects,
		ST_Links,
		ST_CellType,
		ST_HorizontalAlignment,
		ST_VerticalAlignment,
		ST_Orientation,
		ST_PageOrder,
		ST_CellComments,
		ST_PrintError,
		ST_PaneState,
		ST_Guid,
		ST_Comments,
		ST_CellFormulaType,
		ST_UnsignedIntHex,
		ST_SystemColorVal,
		ST_EditAs,
		ST_DrawingElementId,
		ST_Coordinate,
		ST_PositiveCoordinate,

		// MD 2/1/11 - Data Validation support
		ST_DataValidationType,
		ST_DataValidationErrorStyle,
		ST_DataValidationOperator,

		// MD 7/15/11 - Shape support
		ST_ShapeType,

		// MD 8/23/11 - TFS84306
		ST_HexBinary3,

		// MD 11/8/11 - TFS85193
		ST_Percentage,
		ST_TextAlignType,
		ST_TextAnchoringType,

		// MD 12/9/11 - 12.1 - Table Support
		ST_CalendarType,
		ST_DateTimeGrouping,
		ST_DynamicFilterType,
		ST_FilterOperator,
		ST_IconSetType,
		ST_SchemeColorVal,
		ST_SortBy,
		ST_SortMethod,
		ST_TableStyleType,
		ST_TableType,
		ST_TotalsRowFunction,

		// MD 7/3/12 - TFS115689
		// Added round trip support for line end properties.
		ST_LineEndLength,
		ST_LineEndType,
		ST_LineEndWidth,
	}

	#endregion DataType

	#region XML simple types

	//  For documentation on the XML simple types, see:
	//  http://openiso.org/Ecma/376/Part4/3.18

	internal enum ST_VectorBaseType
	{
		variant = DataType.Object,
		i1 = DataType.SByte,
		i2 = DataType.Int16,
		i4 = DataType.Int32,
		i8 = DataType.Int64,
		ui1 = DataType.Byte,
		ui2 = DataType.UInt16,
		ui4 = DataType.UInt32,
		ui8 = DataType.UInt64,
		r4 = DataType.Single,
		r8 = DataType.Double,
		lpstr = DataType.String,
		lpwstr = DataType.String,
		bstr = DataType.String,
		date = DataType.DateTime,
		filetime = DataType.DateTime,
		_bool = DataType.Boolean,
		cy = DataType.Decimal,
		error = DataType.Int32, 
		clsid = DataType.Object, 
		cf = DataType.Object, 
	}

	#region ST_Visibility





	internal enum ST_Visibility
	{
		/// <summary>
		/// WorksheetVisibility.Visible
		/// </summary>
		visible = WorksheetVisibility.Visible,

		/// <summary>
		/// WorksheetVisibility.Hidden
		/// </summary>
		hidden = WorksheetVisibility.Hidden,

		/// <summary>
		/// WorksheetVisibility.StrongHidden
		/// </summary>
		veryHidden = WorksheetVisibility.StrongHidden,
	}
	#endregion ST_Visibility

	#region ST_CalcMode





	internal enum ST_CalcMode
	{
		/// <summary>
		/// CalculationMode.Automatic
		/// </summary>
		auto = CalculationMode.Automatic,

		/// <summary>
		/// CalculationMode.AutomaticExceptForDataTables
		/// </summary>
		autoNoTable = CalculationMode.AutomaticExceptForDataTables,

		/// <summary>
		/// CalculationMode.Manual
		/// </summary>
		manual = CalculationMode.Manual,
	}
	#endregion ST_CalcMode

	#region ST_RefMode





	internal enum ST_RefMode
	{
		/// <summary>
		/// CellReferenceMode.A1
		/// </summary>
		A1 = CellReferenceMode.A1,

		/// <summary>
		/// CellReferenceMode.R1C1
		/// </summary>
		R1C1 = CellReferenceMode.R1C1,
	}
	#endregion ST_RefMode

    #region ST_FontScheme





    internal enum ST_FontScheme
    {
        /// <summary>
        /// This font is the major font for this theme
        /// </summary>
        major,
        /// <summary>
        /// This font is the minor font for this theme
        /// </summary>
        minor,
        /// <summary>
        /// This font is not a theme font
        /// </summary>
        none,
    }
    #endregion ST_FontScheme

    #region ST_UnderlineValues





    internal enum ST_UnderlineValues
    {
        /// <summary>
        /// FontUnderlineStyle.Double
        /// </summary>
        _double = FontUnderlineStyle.Double,

        /// <summary>
        /// FontUnderlineStyle.DoubleAccounting
        /// </summary>
        doubleAccounting = FontUnderlineStyle.DoubleAccounting,

	    /// <summary>
        /// FontUnderlineStyle.None
        /// </summary>
        none = FontUnderlineStyle.None,

        /// <summary>
        /// FontUnderlineStyle.Single
        /// </summary>
        _single = FontUnderlineStyle.Single,

        /// <summary>
        /// FontUnderlineStyle.SingleAccounting
        /// </summary>
        singleAccounting = FontUnderlineStyle.SingleAccounting,

    }
    #endregion ST_UnderlineValues

    #region ST_VerticalAlignRun





    internal enum ST_VerticalAlignRun
    {
        /// <summary>
        /// FontSuperscriptSubscriptStyle.None
        /// </summary>
        baseline = FontSuperscriptSubscriptStyle.None,

        /// <summary>
        /// FontSuperscriptSubscriptStyle.Subscript
        /// </summary>
        subscript = FontSuperscriptSubscriptStyle.Subscript,

        /// <summary>
        /// FontSuperscriptSubscriptStyle.Superscript
        /// </summary>
        superscript = FontSuperscriptSubscriptStyle.Superscript,

    }
    #endregion ST_VerticalAlignRun

    #region ST_BorderStyle





    internal enum ST_BorderStyle
    {
        /// <summary>
        /// CellBorderLineStyle.DashDot
        /// </summary>
		// MD 5/14/10 - TFS23310
		// The case was wrong here.
        //dashdot = CellBorderLineStyle.DashDot,
		dashDot = CellBorderLineStyle.DashDot,

        /// <summary>
        /// CellBorderLineStyle.DashDotDot
        /// </summary>
		// MD 5/14/10 - TFS23310
		// The case was wrong here.
		//dashdotdot = CellBorderLineStyle.DashDotDot,
		dashDotDot = CellBorderLineStyle.DashDotDot,

        /// <summary>
        /// CellBorderLineStyle.Dashed
        /// </summary>
        dashed = CellBorderLineStyle.Dashed,

        /// <summary>
        /// CellBorderLineStyle.Dotted
        /// </summary>
        dotted = CellBorderLineStyle.Dotted,

        /// <summary>
        /// CellBorderLineStyle.Double
        /// </summary>
        _double = CellBorderLineStyle.Double,

        /// <summary>
        /// CellBorderLineStyle.Hair
        /// </summary>
        hair = CellBorderLineStyle.Hair,

        /// <summary>
        /// CellBorderLineStyle.Medium
        /// </summary>
        medium = CellBorderLineStyle.Medium,

        /// <summary>
        /// CellBorderLineStyle.MediumDashDot
        /// </summary>
        mediumDashDot = CellBorderLineStyle.MediumDashDot,

        /// <summary>
        /// CellBorderLineStyle.MediumDashDotDot
        /// </summary>
        mediumDashDotDot = CellBorderLineStyle.MediumDashDotDot,

        /// <summary>
        /// CellBorderLineStyle.MediumDashed
        /// </summary>
        mediumDashed = CellBorderLineStyle.MediumDashed,

        /// <summary>
        /// CellBorderLineStyle.None
        /// </summary>
        none = CellBorderLineStyle.None,

        /// <summary>
        /// CellBorderLineStyle.SlantedDashDot
        /// </summary>
		// MD 5/14/10 - TFS23310
		// The value was wrong here.
		//slantedDashDot = CellBorderLineStyle.SlantedDashDot,
		slantDashDot = CellBorderLineStyle.SlantedDashDot,

        /// <summary>
        /// CellBorderLineStyle.Thick
        /// </summary>
        thick = CellBorderLineStyle.Thick,

        /// <summary>
        /// CellBorderLineStyle.Thin
        /// </summary>
        thin = CellBorderLineStyle.Thin,

    }
    #endregion ST_BorderStyle

    // MBS 7/21/08
    #region ST_Links

    internal enum ST_Links
    {
        always,
        never,
        userSet,
    }
    #endregion //ST_Links
    //
    #region ST_Objects

    internal enum ST_Objects
    {
        all = ObjectDisplayStyle.ShowAll,
        none = ObjectDisplayStyle.HideAll,
        placeholders = ObjectDisplayStyle.ShowPlaceholders,
    }
    #endregion //ST_Objects
    //
    #region ST_Pane
    internal enum ST_Pane
    {
        bottomRight,
        topRight,
        bottomLeft,
        topLeft,
    }
    #endregion //ST_Pane
    //
    #region ST_SheetViewType

    internal enum ST_SheetViewType
    {
        normal = WorksheetView.Normal,
        pageBreakPreview = WorksheetView.PageBreakPreview,
        pageLayout = WorksheetView.PageLayout,
    }
    #endregion //ST_SheetViewType       
  
    #region ST_GradientType

    internal enum ST_GradientType
    {

        /// <summary>
        /// the transition from color to the next is along a line
        /// </summary>
        linear,

        /// <summary>
        /// the boundary of the transition from one color to the next is a rectangle defined by the 
        /// top, bottom, left and right attributes on the gradientFill
        /// </summary>
        path,
    }

    #endregion ST_GradientType

    #region ST_PatternType

    /// <summary>
    /// Infragistics.Documents.Excel.FillPatternStyle
    /// </summary>
    internal enum ST_PatternType
    {
        none = FillPatternStyle.None,
        solid = FillPatternStyle.Solid,
        mediumGray = FillPatternStyle.Gray50percent,
        darkGray = FillPatternStyle.Gray75percent,
        lightGray = FillPatternStyle.Gray25percent,
        darkHorizontal = FillPatternStyle.HorizontalStripe,
        darkVertical = FillPatternStyle.VerticalStripe,

		// MD 2/6/12 - TFS101033
		//darkDown = FillPatternStyle.DiagonalStripe,
		//darkUp = FillPatternStyle.ReverseDiagonalStripe,
		darkDown = FillPatternStyle.ReverseDiagonalStripe,
		darkUp = FillPatternStyle.DiagonalStripe,

        darkGrid = FillPatternStyle.DiagonalCrosshatch,
        darkTrellis = FillPatternStyle.ThickDiagonalCrosshatch,
        lightHorizontal = FillPatternStyle.ThinHorizontalStripe,
        lightVertical = FillPatternStyle.ThinVerticalStripe,

		// MD 2/6/12 - TFS101033
		//lightDown = FillPatternStyle.ThinDiagonalStripe,
		//lightUp = FillPatternStyle.ThinReverseDiagonalStripe,
		lightDown = FillPatternStyle.ThinReverseDiagonalStripe,
		lightUp = FillPatternStyle.ThinDiagonalStripe,

        lightGrid = FillPatternStyle.ThinHorizontalCrosshatch,
        lightTrellis = FillPatternStyle.ThinDiagonalCrosshatch,
        gray125 = FillPatternStyle.Gray12percent,
        gray0625 = FillPatternStyle.Gray6percent,
    }

    #endregion ST_PatternType

    #region ST_CellType

    internal enum ST_CellType
    {





        b, 






        e, 






        inlineStr, 






        n, 






        s, 






        str,
    }
    #endregion //ST_CellType

    #region ST_HorizontalAlignment

    /// <summary>
    /// Infragistics.Documents.Excel.HorizontalCellAlignment
    /// </summary>
    internal enum ST_HorizontalAlignment
    {
        /// <summary>
        /// HorizontalCellAlignment.Center
        /// </summary>
        center = HorizontalCellAlignment.Center,

        /// <summary>
        /// HorizontalCellAlignment.CenterAcrossSelection
        /// </summary>
        centerContinuous = HorizontalCellAlignment.CenterAcrossSelection,

        /// <summary>
        /// HorizontalCellAlignment.Fill
        /// </summary>
        fill = HorizontalCellAlignment.Fill,

        /// <summary>
        /// HorizontalCellAlignment.General
        /// </summary>
        general = HorizontalCellAlignment.General,

        /// <summary>
        /// HorizontalCellAlignment.Justify
        /// </summary>
        justify = HorizontalCellAlignment.Justify,
        
        /// <summary>
        /// HorizontalCellAlignment.Left
        /// </summary>
        left = HorizontalCellAlignment.Left,

        /// <summary>
        /// HorizontalCellAlignment.Right
        /// </summary>
        right = HorizontalCellAlignment.Right,

		// MD 3/5/09
		// Found while adding unit tests. 
		// This member was missing.





		distributed = HorizontalCellAlignment.Distributed,
    }

    #endregion ST_HorizontalAlignment
    
    #region ST_VerticalAlignment

    /// <summary>
    /// Infragistics.Documents.Excel.VerticalCellAlignment
    /// </summary>
    internal enum ST_VerticalAlignment
    {
        /// <summary>
        /// VerticalCellAlignment.Bottom
        /// </summary>
        bottom = VerticalCellAlignment.Bottom,

        /// <summary>
        /// VerticalCellAlignment.Center
        /// </summary>
        center = VerticalCellAlignment.Center,

        /// <summary>
        /// VerticalCellAlignment.Distributed
        /// </summary>
        distributed = VerticalCellAlignment.Distributed,

        /// <summary>
        /// VerticalCellAlignment.Justify
        /// </summary>
        justify = VerticalCellAlignment.Justify,

        /// <summary>
        /// VerticalCellAlignment.Top
        /// </summary>
        top = VerticalCellAlignment.Top,
    }

    #endregion ST_VerticalAlignment

    #region ST_Orientation

    internal enum ST_Orientation
    {
        _default = Orientation.Default,
        portrait = Orientation.Portrait,
        landscape = Orientation.Landscape,
    }
    #endregion //ST_Orientation

    #region ST_PageOrder

    internal enum ST_PageOrder
    {
        downThenOver = PageOrder.DownThenOver,
        overThenDown = PageOrder.OverThenDown,
    }
    #endregion //ST_PageOrder

    #region ST_CellComments

    internal enum ST_CellComments
    {
        none = PrintNotes.DontPrint,
        asDisplayed = PrintNotes.PrintAsDisplayed,
        atEnd = PrintNotes.PrintAtEndOfSheet,
    }
    #endregion //ST_CellComments

    #region ST_PrintError

    internal enum ST_PrintError
    {
        displayed = PrintErrors.PrintAsDisplayed,
        blank = PrintErrors.DontPrint,
        dash = PrintErrors.PrintAsDashes,
        NA = PrintErrors.PrintAsNA,
    }
    #endregion //ST_PrintError

    #region ST_PaneState

    internal enum ST_PaneState
    {
        frozen,
        frozenSplit,
        split,
    }
    #endregion //ST_PaneState

    #region ST_Comments
    internal enum ST_Comments
    {
        commNone = CommentDisplayStyle.None,
        commIndicator = CommentDisplayStyle.Indicator,
        commIndAndComment = CommentDisplayStyle.IndicatorAndComment,
    }
    #endregion ST_Comments

    #region ST_SystemColorVal






    internal enum ST_SystemColorVal
    {






        _3dDarkShadow,






        _3dLight,






        activeBorder,






        activeCaption,






        appWorkspace,






        background,





        btnFace,






        btnHighlight,






        btnText,






        captionText,






        gradientActiveCaption,






        gradientInactiveCaption,






        grayText,






        highlight,






        highlightText,






        hotLight,






        inactiveBorder,






        inactiveCaptionText,






        infoBk,






        menu,






        menuBar,






        menuHighlight,






        menuText,






        scrollBar,






        window,






        windowFrame,






        windowText,

    }

    #endregion ST_SystemColorVal

    #region ST_CellFormulaType

    internal enum ST_CellFormulaType
    {
        normal = FormulaType.Formula,
        array = FormulaType.ArrayFormula,
        shared = FormulaType.SharedFormula,
        dataTable,
    }
    #endregion //ST_CellFormulaType

    #region ST_EditAs

    internal enum ST_EditAs
    {
        absolute = ShapePositioningMode.DontMoveOrSizeWithCells,
        oneCell = ShapePositioningMode.MoveWithCells,
        twoCell = ShapePositioningMode.MoveAndSizeWithCells,
    }
    #endregion ST_EditAs

	// MD 2/1/11 - Data Validation support
	#region ST_DataValidationType

	internal enum ST_DataValidationType
	{
		custom = DataValidationType.Formula,
		date = DataValidationType.Date,
		_decimal = DataValidationType.Decimal,
		list = DataValidationType.List,
		none = DataValidationType.AnyValue,
		textLength = DataValidationType.TextLength,
		time = DataValidationType.Time,
		whole = DataValidationType.WholeNumber,
	}

	#endregion // ST_DataValidationType

	// MD 2/1/11 - Data Validation support
	#region ST_DataValidationErrorStyle

	internal enum ST_DataValidationErrorStyle
	{
		information = DataValidationErrorStyle.Information,
		stop = DataValidationErrorStyle.Stop,
		warning = DataValidationErrorStyle.Warning,
	} 

	#endregion // ST_DataValidationErrorStyle

	// MD 2/1/11 - Data Validation support
	#region ST_DataValidationOperator

	internal enum ST_DataValidationOperator
	{
		between = TwoConstraintDataValidationOperator.Between,
		equal = OneConstraintDataValidationOperator.EqualTo,
		greaterThan = OneConstraintDataValidationOperator.GreaterThan,
		greaterThanOrEqual = OneConstraintDataValidationOperator.GreaterThanOrEqualTo,
		lessThan = OneConstraintDataValidationOperator.LessThan,
		lessThanOrEqual = OneConstraintDataValidationOperator.LessThanOrEqualTo,
		notBetween = TwoConstraintDataValidationOperator.NotBetween,
		notEqual = OneConstraintDataValidationOperator.NotEqualTo,
	}

	#endregion // ST_DataValidationOperator

	// MD 7/13/11 - Shape support
	#region ST_ShapeType

	internal enum ST_ShapeType
	{
		accentBorderCallout1,
		accentBorderCallout2,
		accentBorderCallout3,
		accentCallout1,
		accentCallout2,
		accentCallout3,
		actionButtonBackPrevious,
		actionButtonBeginning,
		actionButtonBlank,
		actionButtonDocument,
		actionButtonEnd,
		actionButtonForwardNext,
		actionButtonHelp,
		actionButtonHome,
		actionButtonInformation,
		actionButtonMovie,
		actionButtonReturn,
		actionButtonSound,
		arc,
		bentArrow,
		bentConnector2,
		bentConnector3,
		bentConnector4,
		bentConnector5,
		bentUpArrow,
		bevel,
		blockArc,
		borderCallout1,
		borderCallout2,
		borderCallout3,
		bracePair,
		bracketPair,
		callout1,
		callout2,
		callout3,
		can,
		chartPlus,
		chartStar,
		chartX,
		chevron,
		chord,
		circularArrow,
		cloud,
		cloudCallout,
		corner,
		cornerTabs,
		cube,
		curvedConnector2,
		curvedConnector3,
		curvedConnector4,
		curvedConnector5,
		curvedDownArrow,
		curvedLeftArrow,
		curvedRightArrow,
		curvedUpArrow,
		decagon,
		diagStripe,
		diamond,
		dodecagon,
		donut,
		doubleWave,
		downArrow,
		downArrowCallout,
		ellipse,
		ellipseRibbon,
		ellipseRibbon2,
		flowChartAlternateProcess,
		flowChartCollate,
		flowChartConnector,
		flowChartDecision,
		flowChartDelay,
		flowChartDisplay,
		flowChartDocument,
		flowChartExtract,
		flowChartInputOutput,
		flowChartInternalStorage,
		flowChartMagneticDisk,
		flowChartMagneticDrum,
		flowChartMagneticTape,
		flowChartManualInput,
		flowChartManualOperation,
		flowChartMerge,
		flowChartMultidocument,
		flowChartOfflineStorage,
		flowChartOffpageConnector,
		flowChartOnlineStorage,
		flowChartOr,
		flowChartPredefinedProcess,
		flowChartPreparation,
		flowChartProcess,
		flowChartPunchedCard,
		flowChartPunchedTape,
		flowChartSort,
		flowChartSummingJunction,
		flowChartTerminator,
		foldedCorner,
		frame,
		funnel,
		gear6,
		gear9,
		halfFrame,
		heart,
		heptagon,
		hexagon,
		homePlate,
		horizontalScroll,
		irregularSeal1,
		irregularSeal2,
		leftArrow,
		leftArrowCallout,
		leftBrace,
		leftBracket,
		leftCircularArrow,
		leftRightArrow,
		leftRightArrowCallout,
		leftRightCircularArrow,
		leftRightRibbon,
		leftRightUpArrow,
		leftUpArrow,
		lightningBolt,
		line,
		lineInv,
		mathDivide,
		mathEqual,
		mathMinus,
		mathMultiply,
		mathNotEqual,
		mathPlus,
		moon,
		nonIsoscelesTrapezoid,
		noSmoking,
		notchedRightArrow,
		octagon,
		parallelogram,
		pentagon,
		pie,
		pieWedge,
		plaque,
		plaqueTabs,
		plus,
		quadArrow,
		quadArrowCallout,
		rect,
		ribbon,
		ribbon2,
		rightArrow,
		rightArrowCallout,
		rightBrace,
		rightBracket,
		round1Rect,
		round2DiagRect,
		round2SameRect,
		roundRect,
		rtTriangle,
		smileyFace,
		snip1Rect,
		snip2DiagRect,
		snip2SameRect,
		snipRoundRect,
		squareTabs,
		star10,
		star12,
		star16,
		star24,
		star32,
		star4,
		star5,
		star6,
		star7,
		star8,
		straightConnector1,
		stripedRightArrow,
		sun,
		swooshArrow,
		teardrop,
		trapezoid,
		triangle,
		upArrow,
		upArrowCallout,
		upDownArrow,
		upDownArrowCallout,
		uturnArrow,
		verticalScroll,
		wave,
		wedgeEllipseCallout,
		wedgeRectCallout,
		wedgeRoundRectCallout
	}

	#endregion // ST_ShapeType

	// MD 11/8/11 - TFS85193
	#region ST_TextAlignType

	internal enum ST_TextAlignType
	{
		ctr = HorizontalTextAlignment.Center,
		just = HorizontalTextAlignment.Justified,
		justLow = HorizontalTextAlignment.JustifiedLow,
		l = HorizontalTextAlignment.Left,
		r = HorizontalTextAlignment.Right,

		// I know these look backwards, but this seems to be correct when opening files in Excel 2010. 
		// thaiDist actually makes the text distributed, and dist actually makes the text Thai distributed.
		dist = HorizontalTextAlignment.ThaiDistributed,
		thaiDist = HorizontalTextAlignment.Distributed,
	}

	#endregion // ST_TextAlignType

	// MD 11/8/11 - TFS85193
	#region ST_TextAnchoringType

	internal enum ST_TextAnchoringType
	{
		b = VerticalTextAlignment.Bottom,
		ctr = VerticalTextAlignment.Center,

		// These two don't seem to work correctly for shapes, so let's leave them out until we find a case where they work.
		//dist = VerticalTextAlignment.Distributed,
		//just = VerticalTextAlignment.Justified,

		t = VerticalTextAlignment.Top,
	}

	#endregion // ST_TextAnchoringType


	// MD 12/9/11 - 12.1 - Table Support
	#region ST_CalendarType

	internal enum ST_CalendarType
	{
		gregorian = CalendarType.Gregorian,
		gregorianArabic = CalendarType.GregorianArabic,
		gregorianMeFrench = CalendarType.GregorianMeFrench,
		gregorianUs = CalendarType.GregorianUs,
		gregorianXlitEnglish = CalendarType.GregorianXlitEnglish,
		gregorianXlitFrench = CalendarType.GregorianXlitFrench,
		hebrew = CalendarType.Hebrew,
		hijri = CalendarType.Hijri,
		japan = CalendarType.Japan,
		korea = CalendarType.Korea,
		none = CalendarType.None,
		saka = CalendarType.Saka,
		taiwan = CalendarType.Taiwan,
		thai = CalendarType.Thai,
	}

	#endregion // ST_CalendarType

	#region ST_DateTimeGrouping

	internal enum ST_DateTimeGrouping
	{
		year = 0,
		month = 1,
		day = 2,
		hour = 3,
		minute = 4,
		second = 5,
	}

	#endregion // ST_DateTimeGrouping

	#region ST_DynamicFilterType

	internal enum ST_DynamicFilterType
	{
		_null = AUTOFILTER12Record.CFTNotCustom,
		aboveAverage = AUTOFILTER12Record.CFTAboveAverage,
		belowAverage = AUTOFILTER12Record.CFTBelowAverage,
		tomorrow = AUTOFILTER12Record.CFTTomorrow,
		today = AUTOFILTER12Record.CFTToday,
		yesterday = AUTOFILTER12Record.CFTYesterday,
		nextWeek = AUTOFILTER12Record.CFTNextWeek,
		thisWeek = AUTOFILTER12Record.CFTThisWeek,
		lastWeek = AUTOFILTER12Record.CFTLastWeek,
		nextMonth = AUTOFILTER12Record.CFTNextMonth,
		thisMonth = AUTOFILTER12Record.CFTThisMonth,
		lastMonth = AUTOFILTER12Record.CFTLastMonth,
		nextQuarter = AUTOFILTER12Record.CFTNextQuarter,
		thisQuarter = AUTOFILTER12Record.CFTThisQuarter,
		lastQuarter = AUTOFILTER12Record.CFTLastQuarter,
		nextYear = AUTOFILTER12Record.CFTNextYear,
		thisYear = AUTOFILTER12Record.CFTThisYear,
		lastYear = AUTOFILTER12Record.CFTLastYear,
		yearToDate = AUTOFILTER12Record.CFTYearToDate,
		Q1 = AUTOFILTER12Record.CFT1stQuarter,
		Q2 = AUTOFILTER12Record.CFT2ndQuarter,
		Q3 = AUTOFILTER12Record.CFT3rdQuarter,
		Q4 = AUTOFILTER12Record.CFT4thQuarter,
		M1 = AUTOFILTER12Record.CFT1stMonth,
		M2 = AUTOFILTER12Record.CFT2ndMonth,
		M3 = AUTOFILTER12Record.CFT3rdMonth,
		M4 = AUTOFILTER12Record.CFT4thMonth,
		M5 = AUTOFILTER12Record.CFT5thMonth,
		M6 = AUTOFILTER12Record.CFT6thMonth,
		M7 = AUTOFILTER12Record.CFT7thMonth,
		M8 = AUTOFILTER12Record.CFT8thMonth,
		M9 = AUTOFILTER12Record.CFT9thMonth,
		M10 = AUTOFILTER12Record.CFT10thMonth,
		M11 = AUTOFILTER12Record.CFT11thMonth,
		M12 = AUTOFILTER12Record.CFT12thMonth,
	}

	#endregion // ST_DynamicFilterType

	#region ST_FilterOperator

	internal enum ST_FilterOperator
	{
		lessThan = 0x01,
		equal = 0x02,
		lessThanOrEqual = 0x03,
		greaterThan = 0x04,
		notEqual = 0x05,
		greaterThanOrEqual = 0x06,
	}

	#endregion // ST_FilterOperator

	#region ST_IconSetType

	internal enum ST_IconSetType
	{
		_3Arrows = 0x00,
		_3ArrowsGray = 0x01,
		_3Flags = 0x02,
		_3TrafficLights1 = 0x03,
		_3TrafficLights2 = 0x04,
		_3Signs = 0x05,
		_3Symbols = 0x06,
		_3Symbols2 = 0x07,
		_4Arrows = 0x08,
		_4ArrowsGray = 0x09,
		_4RedToBlack = 0x0A,
		_4Rating = 0x0B,
		_4TrafficLights = 0x0C,
		_5Arrows = 0x0D,
		_5ArrowsGray = 0x0E,
		_5Rating = 0x0F,
		_5Quarters = 0x10,
	}

	#endregion // ST_IconSetType

	#region ST_SchemeColorVal

	internal enum ST_SchemeColorVal
	{
		dk1 = WorkbookThemeColorType.Dark1,
		lt1 = WorkbookThemeColorType.Light1,
		dk2 = WorkbookThemeColorType.Dark2,
		lt2 = WorkbookThemeColorType.Light2,
		accent1 = WorkbookThemeColorType.Accent1,
		accent2 = WorkbookThemeColorType.Accent2,
		accent3 = WorkbookThemeColorType.Accent3,
		accent4 = WorkbookThemeColorType.Accent4,
		accent5 = WorkbookThemeColorType.Accent5,
		accent6 = WorkbookThemeColorType.Accent6,
		hlink = WorkbookThemeColorType.Hyperlink,
		folHlink = WorkbookThemeColorType.FollowedHyperlink,

		phClr,

		// MD 5/22/12 - TFS112375
		// These values were not numbered, so they were out of range of the theme colors, but they refer
		// to the light and dark values, so they should share their numbers.
		//bg1,
		//tx1,
		//bg2,
		//tx2,
		bg1 = lt1,
		tx1 = dk1,
		bg2 = lt2,
		tx2 = dk2,
	}
    
	#endregion // ST_SchemeColorVal

	#region ST_SortBy

	internal enum ST_SortBy
	{
		value = 0,
		cellColor = 1,
		fontColor = 2,
		icon = 3,
	}

	#endregion // ST_SortBy

	#region ST_SortMethod

	internal enum ST_SortMethod
	{
		none = SortMethod.Default,
		pinYin = SortMethod.PinYin,
		stroke = SortMethod.Stroke,
	}

	#endregion // ST_SortMethod

	#region ST_TableStyleType

	internal enum ST_TableStyleType
	{
		// These values are taken from here: http://msdn.microsoft.com/en-us/library/dd907452(v=office.12).aspx

		wholeTable = 0x00,
		headerRow = 0x01,
		totalRow = 0x02,
		firstColumn = 0x03,
		lastColumn = 0x04,
		firstRowStripe = 0x05,
		secondRowStripe = 0x06,
		firstColumnStripe = 0x07,
		secondColumnStripe = 0x08,
		firstHeaderCell = 0x09,
		lastHeaderCell = 0x0A,
		firstTotalCell = 0x0B,
		lastTotalCell = 0x0C,

		// Not sure about the values here
		firstSubtotalColumn = 0x0D,
		secondSubtotalColumn = 0x0E,
		thirdSubtotalColumn = 0x0F,
		firstSubtotalRow = 0x10,
		secondSubtotalRow = 0x11,
		thirdSubtotalRow = 0x12,

		blankRow = 0x13,

		// Not sure about the values here
		firstColumnSubheading = 0x14,
		secondColumnSubheading = 0x15,
		thirdColumnSubheading = 0x16,
		firstRowSubheading = 0x17,
		secondRowSubheading = 0x18,
		thirdRowSubheading = 0x19,

		pageFieldLabels = 0x1A,
		pageFieldValues = 0x1B,
	}

	#endregion // ST_TableStyleType

	#region ST_TableType

	internal enum ST_TableType
	{
		queryTable,
		worksheet,
		xml
	}

	#endregion  // ST_TableType

	#region ST_TotalsRowFunction

	internal enum ST_TotalsRowFunction
	{
		none = 0x00,
		average = 0x01,
		count = 0x02,
		countNums = 0x03,
		max = 0x04,
		min = 0x05,
		sum = 0x06,
		stdDev = 0x07,
		var = 0x08,
		custom = 0x09,
	}

	#endregion  // ST_TotalsRowFunction


	// MD 7/3/12 - TFS115689
	// Added round trip support for line end properties.
	#region ST_LineEndLength

	internal enum ST_LineEndLength
	{
		lg,
		med,
		sm,
	}

	#endregion  // ST_LineEndLength

	#region ST_LineEndType

	internal enum ST_LineEndType
	{
		arrow,
		diamond,
		none,
		oval,
		stealth,
		triangle,
	}

	#endregion  // ST_LineEndType

	#region ST_LineEndWidth

	internal enum ST_LineEndWidth
	{
		lg,
		med,
		sm,
	}

	#endregion  // ST_LineEndWidth

	#endregion XML simple types

	#region HandledAttributeIdentifier



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

	internal enum HandledAttributeIdentifier
	{
		/// <summary>No value</summary>
		None,

		/// <summary>WorksheetShape.ShapeId</summary>
		CNvPrElement_Id,

		/// <summary>WorksheetShape.Visible</summary>
		CNvPrElement_Hidden,

		// MD 7/18/11 - Shape support
		// The parent elements of these attributes are now fully handled, so we don't need to use the consumed value logic for these anymore.
		#region Removed

		///// <summary>WorksheetShape.GetBoundsInTwips.Width</summary>
		//ExtElement_Cx,
		//
		///// <summary>WorksheetShape.GetBoundsInTwips.Height</summary>
		//ExtElement_Cy,
		//
		///// <summary>WorksheetShape.GetBoundsInTwips.Left</summary>
		//OffElement_X,
		//
		///// <summary>WorksheetShape.GetBoundsInTwips.Top</summary>
		//OffElement_Y,
		//
		///// <summary>Group-specific; represents the horizontal component of the offset for the first child. Always the same as Off.x.</summary>
		//ChOffElement_X,
		//
		///// <summary>Group-specific; represents the vertical component of the offset for the first child. Always the same as Off.y</summary>
		//ChOffElement_Y,
		//
		///// <summary>Group-specific; represents the width of the child extent area. Always the same as Ext.cx.</summary>
		//ChExtElement_Cx,
		//
		///// <summary>Group-specific; represents the height of the child extent area. Always the same as Ext.cy.</summary>
		//ChExtElement_Cy,

		#endregion  // Removed

		/// <summary>WorksheetImage.Image</summary>
		BlipElement_Embed,

		// MD 10/12/10 - TFS49853
		/// <summary>The relationship id of the chart data part.</summary>
		ChartElement_Id
	}

	#endregion HandledAttributeIdentifier

    //  BF 7/16/08 Open Packaging Conventions
    #region OpenPackagingNonConformanceReason enumeration






    /// <summary>
    /// For internal use only.
    /// </summary>




	public

		enum OpenPackagingNonConformanceReason
    {
        /// <summary>
        /// The IPackage.GetPart method threw an exception.
        /// </summary>
        CouldNotGetPackagePart = -1,

        /// <summary>
        /// The IPackagePart conforms fully to the ECMA TC45 Open Packaging Conventions.
        /// </summary>
        Conformant = 0,

        /// <summary>
        /// The package implementer shall require a part name. [M1.1]
        /// </summary>
        NameMissing,

        /// <summary>
        /// The package implementer shall require a content type and
        /// the format designer shall specify the content type. [M1.2], [M1.13]
        /// </summary>
        ContentTypeMissing,

        /// <summary>
        /// A part name shall not have empty segments. [M1.3]
        /// </summary>
        SegmentEmpty,

        /// <summary>
        /// A part name shall start with a forward slash (“/”) character. [M1.4]
        /// </summary>
        NameDoesNotStartWithForwardSlash,

        /// <summary>
        /// A part name shall not have a forward slash as the last character. [M1.5]
        /// </summary>
        NameEndsWithForwardSlash,

        /// <summary>
        /// A segment shall not hold any characters other than pchar characters. [M1.6]
        /// </summary>
        SegmentHasNonPCharCharacters,

        /// <summary>
        /// A segment shall not contain percent-encoded forward slash (“/”),
        /// or backward slash (“\”) characters. [M1.7]
        /// </summary>
        SegmentHasPercentEncodedSlashCharacters,

        /// <summary>
        /// A segment shall not contain percent-encoded unreserved characters. [M1.8]
        /// </summary>
        SegmentHasPercentEncodedUnreservedCharacters,

        /// <summary>
        /// A segment shall not end with a dot (“.”) character. [M1.9]
        /// </summary>
        SegmentEndsWithDotCharacter,

        /// <summary>
        /// A segment shall include at least one non-dot character. [M1.10]
        /// </summary>
        SegmentMissingNonDotCharacter,

        /// <summary>
        /// A package implementer shall neither create nor recognize
        /// a part with a part name derived from another part name by
        /// appending segments to it. [M1.11]
        /// </summary>
        NameDerivesFromExistingPartName,

        /// <summary>
        /// Part name equivalence is determined by comparing part names as
        /// case-insensitive ASCII strings. Packages shall not contain equivalent
        /// part names and package implementers shall neither create nor recognize
        /// packages with equivalent part names. [M1.12]
        /// </summary>
        DuplicateName,

        /// <summary>
        /// IPackage implementers shall only create and only recognize parts with a content type;
        /// format designers shall specify a content type for each part included in the format.
        /// Content types for package parts shall fit the definition and syntax for media types
        /// as specified in RFC 2616, §3.7. [M1.13]
        /// </summary>
        ContentTypeHasInvalidSyntax,

        /// <summary>
        /// Content types shall not use linear white space either between the type and subtype or
        /// between an attribute and its value. Content types also shall not have leading or
        /// trailing white spaces. IPackage implementers shall create only such content types
        /// and shall require such content types when retrieving a part from a package;
        /// format designers shall specify only such content types for inclusion in the format. [M1.14]
        /// </summary>
        ContentTypeHasInvalidWhitespace,

        /// <summary>
        /// The package implementer shall require a content type that does not include comments
        /// and the format designer shall specify such a content type. [M1.15]
        /// </summary>
        ContentTypeHasComments,

        /// <summary>
        /// IPackage implementers and format designers shall not create content types
        /// with parameters for the package specific parts defined in this Open Packaging
        /// specification and shall treat the presence of parameters in these content types
        /// as an error. [M1.22]
        /// </summary>
        ContentTypeHasParameters,

        /// <summary>
        /// If the package implementer specifies a growth hint, it is set when a
        /// part is created and the package implementer shall not change the growth
        /// hint after the part has been created. [M1.16]
        /// </summary>
        /// <remarks><p class="body">The PackageConformanceManager does not verify this convention.</p></remarks>
        GrowthHintChanged,

        /// <summary>
        /// XML content shall be encoded using either UTF-8 or UTF-16.
        /// If any part includes an encoding declaration, as defined in
        /// §4.3.3 of the XML 1.0 specification, that declaration shall
        /// not name any encoding other than UTF-8 or UTF-16. IPackage
        /// implementers shall enforce this requirement upon creation
        /// and retrieval of the XML content. [M1.17]
        /// </summary>
        XmlEncodingUnsupported,

        /// <summary>
        /// XML content shall be valid against the corresponding XSD schema defined
        /// in this Open Packaging specification. In particular, the XML content shall
        /// not contain elements or attributes drawn from namespaces that are not explicitly
        /// defined in the corresponding XSD unless the XSD allows elements or attributes drawn
        /// from any namespace to be present in particular locations in the XML markup. IPackage
        /// implementers  shall enforce this requirement upon creation and retrieval of the
        /// XML content. [M1.20]
        /// </summary>
        XmlContentInvalidForSchema,

        /// <summary>
        /// XML content shall not contain elements or attributes drawn from “xml” or “xsi”
        /// namespaces unless they are explicitly defined in the XSD schema or by other means
        /// described in this Open Packaging specification. IPackage implementers shall enforce
        /// this requirement upon creation and retrieval of the XML content. [M1.21]
        /// </summary>
        XmlContentDrawsOnUndefinedNamespace,

        /// <summary>
        /// The Relationships part shall not have relationships to any other part. IPackage
        /// implementers shall enforce this requirement upon the attempt to create such a
        /// relationship and shall treat any such relationship as invalid. [M1.25]
        /// </summary>
        RelationshipTargetsOtherRelationship,

        /// <summary>
        /// The package implementer shall require that every Relationship element
        /// has an Id attribute, the value of which is unique within the Relationships
        /// part, and that the Id type is xsd:ID, the value of which conforms to the naming
        /// restrictions for xsd:IDas described in the W3C Recommendation “XML Schema Part 2:
        /// Datatypes.” [M1.26]
        /// </summary>
        RelationshipIdInvalid,

        /// <summary>
        /// The package implementer shall require the Type attribute to be a URI
        /// that defines the role of the relationship and the format designer shall
        /// specify such a Type. [M1.27]
        /// </summary>
        RelationshipTypeInvalid,

        /// <summary>
        /// The package implementer shall require the Target attribute to be a URI
        /// reference pointing to a target resource. The URI reference shall be a URI
        /// or a relative reference. [M1.28]
        /// </summary>
        RelationshipTargetInvalid,

        /// <summary>
        /// When set to Internal, the Target attribute shall be a relative reference and
        /// that reference is interpreted relative to the “parent” part. For package
        /// relationships, the package implementer shallresolve relative references in
        /// the Target attribute against the pack URI that identifies the entire package
        /// resource. [M1.29]
        /// </summary>
        RelationshipTargetNotRelativeReference,

        /// <summary>
        /// The package implementer shall name relationship parts according to the special
        /// relationships part naming convention and require that parts with names that
        /// conform to this naming convention have the content type for a Relationships
        /// part. [M1.30]
        /// </summary>
        RelationshipNameInvalid,
    }
    #endregion OpenPackagingNonConformanceReason enumeration

    //  BF 7/16/08 Open Packaging Conventions
    #region OpenPackagingNonConformanceExceptionReason enumeration
    /// <summary>
    /// Constants which define the reason that a <see cref="Infragistics.Documents.Excel.Serialization.Excel2007.OpenPackagingNonConformanceException">OpenPackagingNonConformanceException</see> was thrown.
    /// </summary>
    /// <seealso cref="Infragistics.Documents.Excel.Serialization.Excel2007.OpenPackagingNonConformanceException">OpenPackagingNonConformanceException class</seealso>



	public

		enum OpenPackagingNonConformanceExceptionReason
    {
        /// <summary>
        /// Undefined; used for variable initialization.
        /// </summary>

        [System.ComponentModel.Browsable(false)]

        None = 0,

        /// <summary>
        /// The IPackagePart in a IPackage is determined to be the core properties relationship
        /// for the package, after one has already been processed.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in Annex H, Part 4 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "The format designer shall specify and the format producer shall create at most
        /// <b>one</b> core properties relationship for a package. A format consumer shall
        /// consider more than one core properties relationship for a package to be an error."
        /// </p>
        /// </remarks>
        CorePropertiesRelationshipAlreadyProcessed,

        /// <summary>
        /// The IPackagePart in a IPackage is determined to be the core properties 
        /// relationship for the package, and that IPackagePart is found to contain
        /// references to the Markup Compatibility namespace.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in Annex H, Part 4 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "The format designer shall not specify and the format producer shall not
        /// create Core Properties that use the Markup Compatibility namespace as defined
        /// in Annex F, “Standard Namespaces and Content Types”. A format consumer shall
        /// consider the use of the Markup Compatibility namespace to be an error."
        /// </p>
        /// </remarks>
        UsesMarkupCompatibilityNamespace,

        /// <summary>
        /// The IPackagePart in a IPackage is determined to be  the core properties
        /// relationship for the package, and that IPackagePart is found to contain
        /// refinements to Dublin Core elements other than the 'dcterms:created' and
        /// 'dcterms:modified' elements.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in Annex H, Part 4 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "Producers shall not create a document element that contains refinements to
        /// the Dublin Core elements, except for the two specified in the schema: dcterms:created
        /// and dcterms:modified. Consumers shall consider a document element that violates this
        /// constraint to be an error."
        /// </p>
        /// </remarks>
        ContainsDublinCoreRefinements,

        /// <summary>
        /// The IPackagePart in a IPackage is determined to be  the core properties
        /// relationship for the package, and that IPackagePart is found to contain
        /// an element which contains the xml:lang attribute.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in Annex H, Part 4 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "Producers shall not create a document element that contains the xml:lang
        /// attribute. Consumers shall consider a document element that violates this
        /// constraint to be an error."
        /// </p>
        /// </remarks>
        ContainsXmlLanguageAttribute,

        /// <summary>
        /// The IPackagePart in a IPackage is determined to be  the core properties
        /// relationship for the package, and that IPackagePart is found to contain
        /// an element which contains the xsi:type attribute, with the exception
        /// of the 'dcterms:created' and 'dcterms:modified' elements, for which the
        /// attribute is required, and is expected to hold the value 'dcterms:W3CDTF'.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in Annex H, Part 4 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "Producers shall not create a document element that contains the xsi:type
        /// attribute, except for a 'dcterms:created' or 'dcterms:modified' element where
        /// the xsi:type attribute shall be present and shall hold the value dcterms:W3CDTF,
        /// where dcterms is the namespace prefix of the Dublin Core namespace. Consumers
        /// shall consider a document element that violates this constraint to be an error."
        /// </p>
        /// </remarks>
        XsiTypeAttributeInvalid,

        /// <summary>
        /// A loaded IPackagePart is found to contain a DTD (Document Type Definition) declaration.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in section 8.1.4, paragraph 2 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "The XML 1.0 specification allows for the usage of Document Type Definitions (DTDs),
        /// which enable Denial of Service attacks, typically through the use of an internal entity
        /// expansion technique. As mitigation for this potential threat, DTD declarations shall not
        /// be used in the XML markup defined in this Open Packaging specification. IPackage implementers
        /// shall enforce this requirement upon creation and retrieval of the XML content and shall treat
        /// the presence of DTD declarations as an error."
        /// </p>
        /// </remarks>
        XmlContainsDocumentTypeDefinition,

        /// <summary>
        /// A loaded IPackagePart has the same name as an existing one.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// As stipulated in section 8.1.1.2 of
        /// 'Office Open XML Part 2 - Open Packaging Conventions':<br></br>
        /// 
        /// "Part name equivalence is determined by comparing part names as case-insensitive
        /// ASCII strings. Packages shall not contain equivalent part names and package
        /// implementers shall neither create nor recognize packages with equivalent part
        /// names."
        /// </p>
        /// </remarks>
        DuplicatePartName,
    }
    #endregion OpenPackagingNonConformanceExceptionReason enumeration
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