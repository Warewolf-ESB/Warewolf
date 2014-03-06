using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX
{
    #region XLSXElementType enumeration






	internal enum XLSXElementType
	{
		workbook,
		bookViews,
		workbookView,
		fileVersion,
		sheet,
		c,
		calcChain,
		sst,
		styleSheet,
		externalLink,
		calcPr,
		fonts,
		font,
		b,
		color,
		i,
		name,
		outline,
		strike,
		sz,
		u,
		vertAlign,
		borders,
		border,
		left,
		right,
		top,
		bottom,
		diagonal,
		horizontal,
		vertical,
		family,
		scheme,
		fills,
		fill,
		gradientFill,
		patternFill,
		bgColor,
		fgColor,
		stop,
		cellStyles,
		cellStyleXfs,
		cellXfs,
		dxfs,
		dxf,
		xf,
		protection,
		alignment,
		cellStyle,
		numFmt,
		numFmts,
		colors,
		indexedColors,
		mruColors,
		rgbColor,
		tableStyles,
		tableStyle,
		worksheet,
		sheets,
		sheetViews,
		sheetView,
		pageMargins,
		printOptions,
		selection,
		dimension,
		sheetFormatPr,
		workbookPr,
		sheetData,
		row,
		v,
		si,
		t,
		r,
		rPr,
		rFont,
		pageSetup,
		col,
		cols,
		pane,
		headerFooter,
		oddHeader,
		oddFooter,
		customSheetViews,
		customSheetView,
		customWorkbookViews,
		customWorkbookView,
		sheetPr,
		outlinePr,
		pageSetUpPr,
		tabColor,
		mergeCell,
		mergeCells,
		picture,
		sheetCalcPr,
		f,
		brk,
		colBreaks,
		definedName,
		definedNames,
		externalReferences,
		externalReference,
		externalBook,
		sheetNames,
		sheetName,
		sheetDataSet,
		cell,
		drawing,
		comments,
		authors,
		author,
		commentList,
		comment,
		text,
		legacyDrawing,
		dataValidations,

		// MD 3/22/11 - TFS66776
		sheetProtection,

		#region NA 2011.1 - Infragistics.Word

		document,
		body,
		p,
		pPr,
		jc,
		ind,
		pageBreakBefore,
		cr,
		dstrike,
		rFonts,
		smallCaps,
		caps,
		emboss,
		imprint,
		shadow,
		kern,
		spacing,
		position,
		vanish,
		w,
		hyperlink,
		rStyle,
		inline,
		noProof,
		extent,
		docPr,
		graphic,
		graphicData,
		pic,
		nvPicPr,
		cNvPicPr,
		cNvPr,
		blipFill,
		blip,
		extLst,
		ext,
		spPr,
		stretch,
		fillRect,
		xfrm,
		prstGeom,
		off,
		anchor,
		simplePos,
		positionH,
		positionV,
		posOffset,
		wrapSquare,
		wrapTopAndBottom,
		wrapNone,
		align,
		tbl,
		tblPr,
		tblStyle,
		tblW,
		tblInd,
		tblGrid,
		gridCol,
		tr,
		trPr,
		tc,
		trHeight,
		tcPr,
		tcW,
		gridSpan,
		vMerge,
		shd,
		rPrDefault,
		lang,

		#endregion NA 2011.1 - Infragistics.Word

		// MD 2/1/11 - Data Validation support
		dataValidation,
		formula1,
		formula2,

		// MD 2/1/11 - Page Break support
		rowBreaks,

		// MD 11/22/11
		// Found while writing Interop tests
		workbookProtection,

		// MD 12/6/11 - 12.1 - Table Support
		autoFilter,
		calculatedColumnFormula,
		colorFilter,
		customFilter,
		customFilters,
		dateGroupItem,
		dynamicFilter,
		filterColumn,
		filter,
		filters,
		iconFilter,
		schemeClr,
		sortCondition,
		sortState,
		table,
		tableColumn,
		tableColumns,
		tablePart,
		tableParts,
		tableStyleElement,
		tableStyleInfo,
		top10,
		totalsRowFormula,
	}
    #endregion XLSXElementType enumeration

	// MD 10/1/08
	// Found while fixing TFS8453
	// This is no logner needed
//    #region XLSXContentType enumeration
//#if DEBUG
//    /// <summary>
//    /// Constants which identify the content types for the various
//    /// SpreadsheetML elements which represent the constituent parts
//    /// of an Excel workbook.
//    /// </summary>
//#endif
//    internal enum XLSXContentType
//    {
//        CalculationChain,
//        Comments,
//        ExternalWorkbook,
//        SharedStringTable,
//        Workbook,
//        WorkbookStyles,
//        Worksheet,
//    }
//    #endregion XLSXContentType enumeration

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