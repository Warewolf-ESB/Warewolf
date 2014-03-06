using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Abstract base class for all filters which filters cells in a worksheet.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="CustomFilter"/>
	/// <seealso cref="FixedValuesFilter"/>
	/// <seealso cref="TopOrBottomFilter"/>
	/// <seealso cref="DynamicValuesFilter"/>
	/// <seealso cref="AverageFilter"/>
	/// <seealso cref="RelativeDateRangeFilter"/>
	/// <seealso cref="DatePeriodFilter"/>
	/// <seealso cref="YearToDateFilter"/>
	/// <seealso cref="FontColorFilter"/>
	/// <seealso cref="FillFilter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 abstract class Filter
	{
		#region Member Variables

		private WorksheetTableColumn _owner;

		#endregion // Member Variables

		#region Constructor

		internal Filter(WorksheetTableColumn owner)
		{
			_owner = owner;
		}

		#endregion // Constructor

		#region Methods

		#region GetCellText

		internal static string GetCellText(WorksheetRow row, short columnIndex, out double? numericValue, out ValueFormatter.SectionType formattedAs)
		{
			if (row == null)
			{
				numericValue = null;
				formattedAs = ValueFormatter.SectionType.Text;
				return string.Empty;
			}

			GetCellTextParameters parameters = new GetCellTextParameters(columnIndex);
			parameters.TextFormatMode = TextFormatMode.IgnoreCellWidth;
			parameters.PreventTextFormattingTypes = PreventTextFormattingTypes.None;
			parameters.UseCalculatedValues = true;
			return row.GetCellTextInternal(parameters, out numericValue, out formattedAs);
		}

		#endregion // GetCellText

		#region HasSameData

		internal abstract bool HasSameData(Filter filter);

		#endregion // HasSameData

		#region MeetsCriteria

		internal abstract bool MeetsCriteria(Worksheet worksheet, WorksheetRow row, short columnIndex);

		#endregion // MeetsCriteria

		#region OnBeforeFilterColumn

		internal virtual bool OnBeforeFilterColumn(Worksheet worksheet, int firstRowIndex, int lastRowIndex, short columnIndex) 
		{
			return true;
		}

		#endregion // OnBeforeFilterColumn

		#region OnModified

		internal void OnModified()
		{
			if (_owner != null)
				_owner.OnFilterModified();
		}

		#endregion // OnModified

		#region SetOwner

		internal void SetOwner(WorksheetTableColumn owner)
		{
			Debug.Assert(_owner == null, "This column should not be owned already.");
			_owner = owner;
		}

		#endregion // SetOwner

		#region ShouldSaveIn2003Formats

		internal virtual bool ShouldSaveIn2003Formats(out bool needsAUTOFILTER12Record, out IList<string> allowedTextValues)
		{
			needsAUTOFILTER12Record = true;
			allowedTextValues = null;
			return true;
		}

		#endregion // ShouldSaveIn2003Formats

		#endregion // Methods

		#region Properties

		// MD 4/9/12 - TFS101506
		#region Culture

		internal CultureInfo Culture
		{
			get
			{
				if (_owner == null || _owner.Table == null)
					return CultureInfo.CurrentCulture;

				return _owner.Table.Culture;
			}
		}

		#endregion // Culture

		// MD 5/7/12 - TFS106831
		#region Owner

		internal WorksheetTableColumn Owner
		{
			get { return _owner; }
		}

		#endregion // Owner

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