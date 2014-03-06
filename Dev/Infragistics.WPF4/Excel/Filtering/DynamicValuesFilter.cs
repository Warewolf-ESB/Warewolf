using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Abstract base class for all filter types which filter data based on a dynamic condition, such as the data present in the filtered
	/// data range, the date when the filter was applied, or the date when the filter is evaluated.
	/// </summary>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="AverageFilter"/>
	/// <seealso cref="RelativeDateRangeFilter"/>
	/// <seealso cref="DatePeriodFilter"/>
	/// <seealso cref="YearToDateFilter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 abstract class DynamicValuesFilter : Filter
	{
		#region Constructor

		internal DynamicValuesFilter(WorksheetTableColumn owner)
			: base(owner) { }

		#endregion // Constructor

		#region Methods

		#region CreateDynamicValuesFilter

		internal static DynamicValuesFilter CreateDynamicValuesFilter(WorkbookSerializationManager manager, WorksheetTableColumn column, ST_DynamicFilterType type, double? value, double? maxValue)
		{
			if (type == ST_DynamicFilterType._null)
				return null;

			switch (type)
			{
				case ST_DynamicFilterType.Q1:
				case ST_DynamicFilterType.Q2:
				case ST_DynamicFilterType.Q3:
				case ST_DynamicFilterType.Q4:
					return new DatePeriodFilter(column, DatePeriodFilterType.Quarter, (type - ST_DynamicFilterType.Q1 + 1));

				case ST_DynamicFilterType.M1:
				case ST_DynamicFilterType.M2:
				case ST_DynamicFilterType.M3:
				case ST_DynamicFilterType.M4:
				case ST_DynamicFilterType.M5:
				case ST_DynamicFilterType.M6:
				case ST_DynamicFilterType.M7:
				case ST_DynamicFilterType.M8:
				case ST_DynamicFilterType.M9:
				case ST_DynamicFilterType.M10:
				case ST_DynamicFilterType.M11:
				case ST_DynamicFilterType.M12:
					return new DatePeriodFilter(column, DatePeriodFilterType.Month, (type - ST_DynamicFilterType.M1 + 1));
			}

			if (type == ST_DynamicFilterType.aboveAverage ||
				type == ST_DynamicFilterType.belowAverage)
			{
				AverageFilterType averageType = type == ST_DynamicFilterType.aboveAverage
					? AverageFilterType.AboveAverage
					: AverageFilterType.BelowAverage;

				if (value.HasValue)
				{
					return new AverageFilter(column, averageType, value.Value);
				}
				else
				{
					Utilities.DebugFail("We should have a minValue and maxValue here.");
					return new AverageFilter(column, averageType);
				}
			}

			DateTime? minDate = null;
			DateTime? maxDate = null;
			if (value.HasValue)
			{
				minDate = ExcelCalcValue.ExcelDateToDateTime(manager.Workbook, value.Value);
				Debug.Assert(minDate.HasValue, "The minValue could not be converted.");
			}
			if (maxValue.HasValue)
			{
				maxDate = ExcelCalcValue.ExcelDateToDateTime(manager.Workbook, maxValue.Value);
				Debug.Assert(maxDate.HasValue, "The maxValue could not be converted.");
			}

			if (type == ST_DynamicFilterType.yearToDate)
			{
				if (minDate.HasValue && maxDate.HasValue)
				{
					return new YearToDateFilter(column, minDate.Value, maxDate.Value);
				}
				else
				{
					Utilities.DebugFail("We should have a minValue and maxValue here.");
					return new YearToDateFilter(column);
				}
			}

			RelativeDateRangeOffset offset;
			RelativeDateRangeDuration duration;
			switch (type)
			{
				case ST_DynamicFilterType.tomorrow:
					offset = RelativeDateRangeOffset.Next;
					duration = RelativeDateRangeDuration.Day;
					break;

				case ST_DynamicFilterType.today:
					offset = RelativeDateRangeOffset.Current;
					duration = RelativeDateRangeDuration.Day;
					break;

				case ST_DynamicFilterType.yesterday:
					offset = RelativeDateRangeOffset.Previous;
					duration = RelativeDateRangeDuration.Day;
					break;

				case ST_DynamicFilterType.nextWeek:
					offset = RelativeDateRangeOffset.Next;
					duration = RelativeDateRangeDuration.Week;
					break;

				case ST_DynamicFilterType.thisWeek:
					offset = RelativeDateRangeOffset.Current;
					duration = RelativeDateRangeDuration.Week;
					break;

				case ST_DynamicFilterType.lastWeek:
					offset = RelativeDateRangeOffset.Previous;
					duration = RelativeDateRangeDuration.Week;
					break;

				case ST_DynamicFilterType.nextMonth:
					offset = RelativeDateRangeOffset.Next;
					duration = RelativeDateRangeDuration.Month;
					break;

				case ST_DynamicFilterType.thisMonth:
					offset = RelativeDateRangeOffset.Current;
					duration = RelativeDateRangeDuration.Month;
					break;

				case ST_DynamicFilterType.lastMonth:
					offset = RelativeDateRangeOffset.Previous;
					duration = RelativeDateRangeDuration.Month;
					break;

				case ST_DynamicFilterType.nextQuarter:
					offset = RelativeDateRangeOffset.Next;
					duration = RelativeDateRangeDuration.Quarter;
					break;

				case ST_DynamicFilterType.thisQuarter:
					offset = RelativeDateRangeOffset.Current;
					duration = RelativeDateRangeDuration.Quarter;
					break;

				case ST_DynamicFilterType.lastQuarter:
					offset = RelativeDateRangeOffset.Previous;
					duration = RelativeDateRangeDuration.Quarter;
					break;

				case ST_DynamicFilterType.nextYear:
					offset = RelativeDateRangeOffset.Next;
					duration = RelativeDateRangeDuration.Year;
					break;

				case ST_DynamicFilterType.thisYear:
					offset = RelativeDateRangeOffset.Current;
					duration = RelativeDateRangeDuration.Year;
					break;

				case ST_DynamicFilterType.lastYear:
					offset = RelativeDateRangeOffset.Previous;
					duration = RelativeDateRangeDuration.Year;
					break;

				default:
					Utilities.DebugFail("Unknown ST_DynamicFilterType: " + type);
					return null;
			}

			if (minDate.HasValue && maxDate.HasValue)
				return new RelativeDateRangeFilter(column, offset, duration, minDate.Value, maxDate.Value);

			return new RelativeDateRangeFilter(column, offset, duration);
		}

		#endregion // CreateDynamicValuesFilter

		#endregion // Methods

		#region Properties

		internal int Type2003 { get { return (int)this.Type2007; } }
		internal abstract ST_DynamicFilterType Type2007 { get; }

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