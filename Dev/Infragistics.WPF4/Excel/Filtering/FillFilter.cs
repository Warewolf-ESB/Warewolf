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
	// MD 2/27/12 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which will filter cells based on their background fills.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// This filter specifies a single <see cref="CellFill"/>. Cells of with this fill will be visible in the data range. 
	/// All other cells will be hidden.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FillFilter : Filter,
		IColorFilter
	{
		#region Member Variables

		private CellFill _fill;

		#endregion // Member Variables

		#region Constructor

		internal FillFilter(WorksheetTableColumn owner, CellFill fill)
			: base(owner)
		{
			if (fill == null)
				throw new ArgumentNullException("fill");

			_fill = fill;
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
			//format.Fill = this.Fill;
			format.Fill = this.Fill.ToResolvedColorFill(manager.Workbook);

			return format;
		}

		bool IColorFilter.IsCellColorFilter
		{
			get { return true; }
		}

		#endregion

		#endregion // Interfaces

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			FillFilter other = filter as FillFilter;
			if (other == null)
				return false;

			// MD 5/7/12 - TFS106831
			// Only resolved colors can be used in the filters. Tints and theme values will cause everything to get filtered out.
			// So compare resolved colors when determining equality.
			//return Object.Equals(_fill, other._fill);
			Workbook workbook = null;
			if (this.Owner != null)
				workbook = this.Owner.Workbook;

			if (workbook == null)
				return Object.Equals(_fill, other._fill);
			else
				return Object.Equals(_fill.ToResolvedColorFill(workbook), other._fill.ToResolvedColorFill(workbook));
		}

		#endregion // HasSameData

		#region MeetsCriteria

		internal override bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex)
		{
			WorksheetCellFormatData cellFormat = worksheet.GetCellFormatElementReadOnly(row, columnIndex);

			// MD 5/7/12 - TFS106831
			//return Utilities.CompareFillsForSortOrFilter(cellFormat, this.Fill);
			return Utilities.CompareFillsForSortOrFilter(worksheet.Workbook, cellFormat, this.Fill);
		}

		#endregion // MeetsCriteria

		#endregion // Base Class Overrides

		#region Methods

		#region CreateFillFilter

		internal static FillFilter CreateFillFilter(WorksheetTableColumn column, WorksheetCellFormatData format)
		{
			if (format.Fill == null)
			{
				Utilities.DebugFail("This is unexpected.");
				return null;
			}

			CellFill fill = format.Fill;
			CellFillPattern patternFill = fill as CellFillPattern;

			// The fill filter does reverse the colors for solid fills, so if passed in format incorrectly thinks they are not reversed, 
			// swap them.
			if (format.DoesReverseColorsForSolidFill == false &&
				patternFill != null &&
				patternFill.PatternStyle == FillPatternStyle.Solid)
			{
				fill = CellFill.CreatePatternFill(patternFill.PatternColorInfo, patternFill.BackgroundColorInfo, FillPatternStyle.Solid);
			}

			return new FillFilter(column, fill);
		}

		#endregion // CreateFillFilter

		#endregion // Methods

		#region Properties

		#region Fill

		/// <summary>
		/// Gets or sets the <see cref="CellFill"/> by which the cells should be filtered.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Cells of with this fill will be visible in the the data range. All other cells will be hidden.
		/// </p>
		/// </remarks>
		/// <exception cref="ArgumentNullException">
		/// The value assigned is null.
		/// </exception>
		/// <value>The CellFill by which the cells should be filtered.</value>
		/// <seealso cref="WorksheetCell.CellFormat"/>
		/// <seealso cref="IWorksheetCellFormat.Fill"/>
		public CellFill Fill
		{
			get { return _fill; }
			set
			{
				if (this.Fill == null)
					return;

				if (value == null)
					throw new ArgumentNullException("value");

				_fill = value;
				this.OnModified();
			}
		}

		#endregion // Fill

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