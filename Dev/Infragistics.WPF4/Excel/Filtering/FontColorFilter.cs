using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Serialization;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which will filter cells based on their font colors.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This filter specifies a single color. Cells with this color font will be visible in the data range. All other cells 
	/// will be hidden.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFontColorFilter(Color)"/>
	/// <seealso cref="WorksheetTableColumn.ApplyFontColorFilter(WorkbookColorInfo)"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FontColorFilter : Filter,
		IColorFilter
	{
		#region Member Variables

		private WorkbookColorInfo _fontColorInfo;

		#endregion // Member Variables

		#region Constructor

		internal FontColorFilter(WorksheetTableColumn owner, WorkbookColorInfo fontColorInfo)
			: base(owner)
		{
			if (fontColorInfo == null)
				throw new ArgumentNullException("fontColorInfo");

			_fontColorInfo = fontColorInfo;
		}

		#endregion // Constructor

		#region Interfaces

		#region IColorFilter Members

		WorksheetCellFormatData IColorFilter.GetDxf(WorkbookSerializationManager manager)
		{
			WorksheetCellFormatData format = manager.Workbook.CreateNewWorksheetCellFormatInternal(WorksheetCellFormatType.DifferentialFormat);
			format.IsForFillOrSortCondition = true;

			// MD 5/7/12 - TFS106831
			// Only resolved colors can be used in the filters. Tints and theme values will cause everything to get filtered out.
			//format.Fill = CellFill.CreateSolidFill(this.FontColorInfo);
			Color resolvedColor = this.FontColorInfo.GetResolvedColor(manager.Workbook);
			format.Fill = CellFill.CreatePatternFill(
				resolvedColor,
				Utilities.ColorWhite, 
				FillPatternStyle.Solid);

			return format;
		}

		bool IColorFilter.IsCellColorFilter
		{
			get { return false; }
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			FontColorFilter other = filter as FontColorFilter;
			if (other == null)
				return false;

			// MD 5/7/12 - TFS106831
			// Only resolved colors can be used in the filters. Tints and theme values will cause everything to get filtered out.
			// So compare resolved colors when determining equality.
			//return _fontColorInfo == other._fontColorInfo;
			Workbook workbook = null;
			if (this.Owner != null)
				workbook = this.Owner.Workbook;

			if (workbook == null)
				return _fontColorInfo == other._fontColorInfo;
			
			return
				Utilities.ColorToArgb(_fontColorInfo.GetResolvedColor(workbook)) ==
				Utilities.ColorToArgb(other._fontColorInfo.GetResolvedColor(workbook));
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			WorksheetCellFormatData cellFormat = worksheet.GetCellFormatElementReadOnly(row, columnIndex);
			WorkbookColorInfo cellFontColor = cellFormat.FontColorInfoResolved;

			Workbook workbook = worksheet.Workbook;

			// MD 5/7/12 - TFS106831
			//if (workbook == null && (cellFontColor.ThemeColorType.HasValue || this.FontColorInfo.ThemeColorType.HasValue))
			//    return false;
			//
			//return cellFontColor.GetResolvedColor(workbook) == this.FontColorInfo.GetResolvedColor(workbook);
			if (workbook == null)
				return cellFontColor == this.FontColorInfo;
			else
				return cellFontColor.GetResolvedColor(workbook) == this.FontColorInfo.GetResolvedColor(workbook);
		}

		#endregion // MeetsCriteria

		#endregion // Base Class Overrides

		#region Methods

		#region CreateFontColorFilter

		internal static FontColorFilter CreateFontColorFilter(WorksheetTableColumn column, WorksheetCellFormatData format)
		{
			CellFillPattern patternFill = format.Fill as CellFillPattern;
			if (patternFill == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return null;
			}

			Debug.Assert(patternFill.PatternStyle == FillPatternStyle.Solid, "This is unexpected.");

			WorkbookColorInfo colorInfo = patternFill.GetFileFormatForegroundColorInfo(format);
			return new FontColorFilter(column, colorInfo);
		}

		#endregion // CreateFontColorFilter

		#endregion // Methods

		#region Properties

		#region FontColorInfo

		/// <summary>
		/// Gets or sets the <see cref="WorkbookColorInfo"/> which describes the font color by which the cells should be filtered.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Cells of this font color will be visible in the the data range. All other cells will be hidden.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <value>The WorkbookColorInfo which describes the color by which the cells should be filtered.</value>
		/// <seealso cref="Type"/>
		/// <seealso cref="WorksheetCell.CellFormat"/>
		/// <seealso cref="IWorksheetCellFormat.Font"/>
		/// <seealso cref="IWorkbookFont.ColorInfo"/>
		public WorkbookColorInfo FontColorInfo
		{
			get { return _fontColorInfo; }
			set
			{
				if (this.FontColorInfo == null)
					return;

				if (value == null)
					throw new ArgumentNullException("value");

				_fontColorInfo = value;
				this.OnModified();
			}
		}

		#endregion // FontColorInfo

		#endregion // Properties
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