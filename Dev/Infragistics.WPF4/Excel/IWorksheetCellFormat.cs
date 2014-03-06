using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;





using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Represents the format for the cell.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Depending on where the format is exposed, it will have a different meaning. For example, a cell's format just applies to itself, 
	/// but a row or column's format applies to all cells in that row or column. There are also style formats, which can be the parent of
	/// other formats, meaning they provide defaults for values not set on the format. And finally, there are differential formats, such
	/// as the format for areas in a table, which provide default values for cells which exist in the associated area.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetCell.CellFormat"/>
	/// <seealso cref="RowColumnBase.CellFormat"/>
	/// <seealso cref="WorksheetMergedCellsRegion.CellFormat"/>
	/// <seealso cref="WorkbookStyle.StyleFormat"/>
	/// <seealso cref="WorksheetTable.AreaFormats"/>
	/// <seealso cref="WorksheetTableColumn.AreaFormats"/>
	/// <seealso cref="WorksheetTableStyle.AreaFormats"/>
	/// <seealso cref="WorksheetCell.GetResolvedCellFormat"/>
	/// <seealso cref="RowColumnBase.GetResolvedCellFormat"/>
	/// <seealso cref="WorksheetMergedCellsRegion.GetResolvedCellFormat"/>



	public

		 interface IWorksheetCellFormat
	{
		/// <summary>
		/// Copies all cell format properties to from the specified cell format.
		/// </summary>
		/// <param name="source">The cell format from which to copy the properties.</param>
		void SetFormatting( IWorksheetCellFormat source );

		/// <summary>
		/// Gets or sets the horizontal alignment of the content in a cell.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="HorizontalCellAlignment"/> enumeration.
		/// </exception>
		/// <value>The horizontal alignment of the content in a cell.</value>
		/// <seealso cref="VerticalAlignment"/>
		HorizontalCellAlignment Alignment { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="BottomBorderColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.BottomBorderColor is deprecated. It has been replaced by IWorksheetCellFormat.BottomBorderColorInfo.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		Color BottomBorderColor { get; set; }

		// MD 1/16/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the bottom border color.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border color is set to a non-default value and the <see cref="BottomBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin.
		/// </p>
		/// </remarks>
		/// <value>The bottom border color.</value>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo BottomBorderColorInfo { get; set; }

		/// <summary>
		/// Gets or sets the bottom border style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border style is set to a non-default value and the <see cref="BottomBorderColorInfo"/> is null,
		/// it will be resolved to <see cref="WorkbookColorInfo.Automatic"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The bottom border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>
		CellBorderLineStyle BottomBorderStyle { get; set; }

		// MD 10/26/11 - TFS91546
		/// <summary>
		/// Obsolete. Use <see cref="DiagonalBorderColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.DiagonalBorderColor is deprecated. It has been replaced by IWorksheetCellFormat.DiagonalBorderColorInfo.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		Color DiagonalBorderColor { get; set; }

		// MD 1/16/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the diagonal border color.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border color is set to a non-default value and the <see cref="DiagonalBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin.
		/// </p>
		/// </remarks>
		/// <value>The diagonal border color.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo DiagonalBorderColorInfo { get; set; }

		// MD 10/26/11 - TFS91546
		/// <summary>
		/// Gets or sets the diagonal borders.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the diagonal borders are set to something other than None and the <see cref="DiagonalBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin. Similarly, if the <see cref="DiagonalBorderColor"/> is default (Color.Empty), it will be 
		/// resolved to Color.Black.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The diagonal border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>
		DiagonalBorders DiagonalBorders { get; set; }

		// MD 10/26/11 - TFS91546
		/// <summary>
		/// Gets or sets the diagonal border style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border style is set to a non-default value and the <see cref="DiagonalBorderColorInfo"/> is null, 
		/// it will be resolved to <see cref="WorkbookColorInfo.Automatic"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The diagonal border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>
		CellBorderLineStyle DiagonalBorderStyle { get; set; }

		/// <summary>
		/// Gets or sets the fill of the cell.
		/// </summary>
		/// <value>The fill of the cell.</value>
		/// <seealso cref="CellFill"/>
		/// <seealso cref="CellFillPattern"/>
		/// <seealso cref="CellFillLinearGradient"/>
		/// <seealso cref="CellFillRectangularGradient"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		CellFill Fill { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="Fill"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/19/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.FillPattern is deprecated. It has been replaced by IWorksheetCellFormat.Fill.")] // MD 1/19/12 - 12.1 - Cell Format Updates
		FillPatternStyle FillPattern { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="Fill"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/19/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.FillPatternBackgroundColor is deprecated. It has been replaced by IWorksheetCellFormat.Fill.")] // MD 1/19/12 - 12.1 - Cell Format Updates
		Color FillPatternBackgroundColor { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="Fill"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/19/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.FillPatternForegroundColor is deprecated. It has been replaced by IWorksheetCellFormat.Fill.")] // MD 1/19/12 - 12.1 - Cell Format Updates
		Color FillPatternForegroundColor { get; set; }

		/// <summary>
		/// Gets or sets the default font formatting.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This font formatting is just the default font used in the cell. This can be overridden by strings on a character by character basis by
		/// using the <see cref="FormattedString"/> class.
		/// </p>
		/// </remarks>
		/// <value>The default font formatting.</value>
		IWorkbookFont Font { get; }

		// MD 12/30/11 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the groups of properties provided by the format.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// For style formats, this value indicates which properties should be used in the format when applying the style to 
		/// a cell. All other groups of properties will return a default value.
		/// </p>
		/// <p class="body">
		/// For cell and differential formats, this value indicates which properties should be resolved and returned by the 
		/// format. All other properties should will be resolved and returned by the parent <see cref="Style"/>.
		/// </p>
		/// </remarks>
		/// <seealso cref="Style"/>
		/// <seealso cref="WorkbookStyle.StyleFormat"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorksheetCellFormatOptions FormatOptions { get; set; }

		/// <summary>
		/// Gets or sets the number format string.
		/// </summary>
		/// <remarks>
		/// <p class="body">For more information on excel format strings, consult Microsoft Excel help.</p>
		/// </remarks>
		/// <exception cref="ArgumentException">
		/// The assigned value is an invalid format string and <see cref="Workbook.ValidateFormatStrings"/> is True.
		/// </exception>
		/// <value>The number format string.</value>
		/// <seealso cref="Workbook.ValidateFormatStrings"/>
		string FormatString { get; set; }

		/// <summary>
		/// Gets or sets the indent in units of average character widths.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// A value of -1 indicates that the default value should be used.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">
		/// The assigned value is not -1 and is outside the valid indent level range of 0 and 250.
		/// </exception>
		/// <value>The indent in units of average character widths or -1 to use the default indent.</value>
		int Indent { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="LeftBorderColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.LeftBorderColor is deprecated. It has been replaced by IWorksheetCellFormat.LeftBorderColorInfo.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		Color LeftBorderColor { get; set; }

		// MD 1/16/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the left border color.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border color is set to a non-default value and the <see cref="LeftBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin.
		/// </p>
		/// </remarks>
		/// <value>The left border color.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo LeftBorderColorInfo { get; set; }

		/// <summary>
		/// Gets or sets the left border style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border style is set to a non-default value and the <see cref="LeftBorderColorInfo"/> is null, 
		/// it will be resolved to <see cref="WorkbookColorInfo.Automatic"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The left border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>
		CellBorderLineStyle LeftBorderStyle { get; set; }

		/// <summary>
		/// Gets or sets the valid which indicates whether the cell is locked in protected mode.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Locked valid is used in Excel file only if the associated <see cref="Worksheet"/> or <see cref="Workbook"/> 
		/// is protected. Otherwise the value is ignored.
		/// </p>
		/// </remarks>
		/// <value>The valid which indicates whether the cell is locked in protected mode.</value>
		/// <seealso cref="Workbook.Protected"/>
		ExcelDefaultableBoolean Locked { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="RightBorderColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.RightBorderColor is deprecated. It has been replaced by IWorksheetCellFormat.RightBorderColorInfo.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		Color RightBorderColor { get; set; }

		// MD 1/16/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the right border color.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border color is set to a non-default value and the <see cref="RightBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin.
		/// </p>
		/// </remarks>
		/// <value>The right border color.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo RightBorderColorInfo { get; set; }

		/// <summary>
		/// Gets or sets the right border style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border style is set to a non-default value and the <see cref="RightBorderColorInfo"/> is null, 
		/// it will be resolved to <see cref="WorkbookColorInfo.Automatic"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The right border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="TopBorderColorInfo"/>
		/// <seealso cref="TopBorderStyle"/>
		CellBorderLineStyle RightBorderStyle { get; set; }

		/// <summary>
		/// Gets or sets the rotation of the cell content in degrees.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Cell text rotation, in degrees; 0 � 90 is up 0 � 90 degrees, 91 � 180 is down 1 � 90 degrees, and 255 is vertical.
		/// </p>
		/// </remarks>
		/// <value>The rotation of the cell content in degrees.</value>
		int Rotation { get; set; }

		/// <summary>
		/// Gets or sets the value indicating whether the cell content will shrink to fit the cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If True, the size of the cell font will shrink so all data fits within the cell.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <value>The value indicating whether the cell content will shrink to fit the cell.</value>
		ExcelDefaultableBoolean ShrinkToFit { get; set; }

		// MD 12/30/11 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the parent <see cref="WorkbookStyle"/> of the format.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The parent style of a cell or differential format provides default format values. Depending on which <see cref="FormatOptions"/>
		/// flags are present, only certain groups of format properties will be resolved from parent style. When any of the properties in a
		/// group is set to a non-default value, the FormatOptions will automatically have that flag included so the format provides values
		/// in that group rather than the style providing values in that group.
		/// </p>
		/// <p class="body">
		/// By default, all cell and differential formats will have a parent style of the normal style, which is exposed by the 
		/// <see cref="WorkbookStyleCollection.NormalStyle"/> property. If a value of null is assigned as the Style for a cell or differential 
		/// format, then NormalStyle will actually be set.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidOperationException">
		/// The value assigned is non-null and the format is a style format.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The value assigned is from a different <see cref="Workbook"/>.
		/// </exception>
		/// <value>
		/// A <see cref="WorkbookStyle"/> instance if this is a cell or differential format, or null if this is a style format.
		/// </value>
		/// <seealso cref="FormatOptions"/>
		/// <seealso cref="Workbook.Styles"/>
		/// <seealso cref="WorkbookStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookStyle Style { get; set; }

		/// <summary>
		/// Obsolete. Use <see cref="TopBorderColorInfo"/> instead.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)] // MD 1/16/12 - 12.1 - Cell Format Updates
		[Obsolete("The IWorksheetCellFormat.TopBorderColor is deprecated. It has been replaced by IWorksheetCellFormat.TopBorderColorInfo.")] // MD 1/16/12 - 12.1 - Cell Format Updates
		Color TopBorderColor { get; set; }

		// MD 1/16/12 - 12.1 - Cell Format Updates
		/// <summary>
		/// Gets or sets the top border color.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border color is set to a non-default value and the <see cref="TopBorderStyle"/> is set to Default, 
		/// it will be resolved to Thin.
		/// </p>
		/// </remarks>
		/// <value>The top border color.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderStyle"/>

		[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelCellFormats)]

		WorkbookColorInfo TopBorderColorInfo { get; set; }

		/// <summary>
		/// Gets or sets the top border style.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the border style is set to a non-default value and the <see cref="TopBorderColorInfo"/> is null, 
		/// it will be resolved to <see cref="WorkbookColorInfo.Automatic"/>.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="CellBorderLineStyle"/> enumeration.
		/// </exception>
		/// <value>The top border style.</value>
		/// <seealso cref="BottomBorderColorInfo"/>
		/// <seealso cref="BottomBorderStyle"/>
		/// <seealso cref="DiagonalBorderColorInfo"/>
		/// <seealso cref="DiagonalBorders"/>
		/// <seealso cref="DiagonalBorderStyle"/>
		/// <seealso cref="LeftBorderColorInfo"/>
		/// <seealso cref="LeftBorderStyle"/>
		/// <seealso cref="RightBorderColorInfo"/>
		/// <seealso cref="RightBorderStyle"/>
		/// <seealso cref="TopBorderColorInfo"/>
		CellBorderLineStyle TopBorderStyle { get; set; }

		/// <summary>
		/// Gets or sets the vertical alignment of the content in a cell.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="VerticalCellAlignment"/> enumeration.
		/// </exception>
		/// <value>The vertical alignment of the content in a cell.</value>
		/// <seealso cref="Alignment"/>
		VerticalCellAlignment VerticalAlignment { get; set; }

		/// <summary>
		/// Gets or sets the value which indicates whether text will wrap in a cell.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If True, and the row associated with the cell has a default <see cref="WorksheetRow.Height"/>, the row's
		/// height will automatically be increased to fit wrapped content.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="ExcelDefaultableBoolean"/> enumeration.
		/// </exception>
		/// <value>The value which indicates whether text will wrap in a cell.</value>
		ExcelDefaultableBoolean WrapText { get; set; }
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