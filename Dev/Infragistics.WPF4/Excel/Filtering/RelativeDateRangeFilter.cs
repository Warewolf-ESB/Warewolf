using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a filter which can filter date cells based on dates relative to the when the filter was applied.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// The RelativeDateRangeFilter allows you to filter in dates which are in the previous, current, or next time period 
	/// relative to the date when the filter was applied. The time periods available are day, week, month, quarter, year.
	/// So when using the previous filter type with a day duration, a 'yesterday' filter is created. Or when using a current 
	/// filter type with a year duration, a 'this year' filter is created. However, these filters compare the data against 
	/// the date when the filter was created. So a 'this year' filter created in 1999 will filter in all cells containing 
	/// dates in 1999, even if the workbook is opened in 2012.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetTableColumn.Filter"/>
	/// <seealso cref="WorksheetTableColumn.ApplyRelativeDateRangeFilter"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class RelativeDateRangeFilter : DateRangeFilter
	{




		#region Member Variables

		private RelativeDateRangeDuration _duration;
		private RelativeDateRangeOffset _offset;

		#endregion // Member Variables

		#region Constructor

		internal RelativeDateRangeFilter(WorksheetTableColumn owner, RelativeDateRangeOffset offset, RelativeDateRangeDuration duration)
			: base(owner)
		{
			this.Initialize(offset, duration);
		}

		internal RelativeDateRangeFilter(WorksheetTableColumn owner, RelativeDateRangeOffset offset, RelativeDateRangeDuration duration, DateTime start, DateTime end)
			: base(owner, start, end)
		{
			this.Initialize(offset, duration);
		}

		private void Initialize(RelativeDateRangeOffset offset, RelativeDateRangeDuration duration)
		{
			Utilities.VerifyEnumValue(offset);
			Utilities.VerifyEnumValue(duration);

			_offset = offset;
			_duration = duration;
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region HasSameData

		internal override bool HasSameData(Filter filter)
		{
			if (Object.ReferenceEquals(this, filter))
				return true;

			RelativeDateRangeFilter other = filter as RelativeDateRangeFilter;
			if (other == null)
				return false;

			return
				_duration == other._duration &&
				_offset == other._offset &&
				base.HasSameData(other);
		}

		#endregion // HasSameData

		#region ReinitializeRangeHelper

		internal override void ReinitializeRangeHelper(out DateTime start, out DateTime end)
		{
			DateTime reapplyDateTime = DateTime.Now;






			start = reapplyDateTime;
			end = reapplyDateTime;

			int offsetValue;
			switch (this.Offset)
			{
				case RelativeDateRangeOffset.Previous:
					offsetValue = -1;
					break;

				case RelativeDateRangeOffset.Current:
					offsetValue = 0;
					break;

				case RelativeDateRangeOffset.Next:
					offsetValue = 1;
					break;

				default:
					Utilities.DebugFail("Unknown RelativeDateValueOffset: " + this.Offset);
					return;
			}

			switch (this.Duration)
			{
				case RelativeDateRangeDuration.Day:
					start = reapplyDateTime.Date.AddDays(offsetValue);
					end = start.AddDays(1);
					break;

				case RelativeDateRangeDuration.Week:
					// Surprisingly, Microsoft Excel doesn't look at the system settings to determine the first day of the week.
					// They always use Sunday (maybe so files shared across machines do not filter differently), so we can use the 
					// DayOfWeek enumeration values to offset us back to the Sunday of the current week before shifting by the 
					// actual offset amount of weeks.
					start = reapplyDateTime.Date.AddDays(-(int)reapplyDateTime.DayOfWeek).AddDays(offsetValue * 7);

					end = start.AddDays(7);
					break;

				case RelativeDateRangeDuration.Month:
					start = new DateTime(reapplyDateTime.Year, reapplyDateTime.Month, 1).AddMonths(offsetValue);
					end = start.AddMonths(1);
					break;

				case RelativeDateRangeDuration.Quarter:
					int year = reapplyDateTime.Year;
					int quarter = Utilities.GetQuarter(reapplyDateTime) + offsetValue;

					if (quarter < 1)
					{
						year--;
						quarter += 4;
					}
					else if (4 < quarter)
					{
						year++;
						quarter -= 4;
					}

					start = new DateTime(year, Utilities.GetFirstMonthOfQuarter(quarter), 1);
					end = start.AddMonths(3);
					break;

				case RelativeDateRangeDuration.Year:
					start = new DateTime(reapplyDateTime.Year, 1, 1).AddYears(offsetValue);
					end = start.AddYears(1);
					break;

				default:
					Utilities.DebugFail("Unknown RelativeDateValueDuration: " + this.Duration);
					return;
			}
		}

		#endregion // ReinitializeRangeHelper

		#region Type2007

		internal override ST_DynamicFilterType Type2007
		{
			get
			{
				ST_DynamicFilterType[] values;
				switch (this.Duration)
				{
					case RelativeDateRangeDuration.Day:
						values = new ST_DynamicFilterType[] { ST_DynamicFilterType.yesterday, ST_DynamicFilterType.today, ST_DynamicFilterType.tomorrow };
						break;

					case RelativeDateRangeDuration.Week:
						values = new ST_DynamicFilterType[] { ST_DynamicFilterType.lastWeek, ST_DynamicFilterType.thisWeek, ST_DynamicFilterType.nextWeek };
						break;

					case RelativeDateRangeDuration.Month:
						values = new ST_DynamicFilterType[] { ST_DynamicFilterType.lastMonth, ST_DynamicFilterType.thisMonth, ST_DynamicFilterType.nextMonth };
						break;

					case RelativeDateRangeDuration.Quarter:
						values = new ST_DynamicFilterType[] { ST_DynamicFilterType.lastQuarter, ST_DynamicFilterType.thisQuarter, ST_DynamicFilterType.nextQuarter };
						break;

					case RelativeDateRangeDuration.Year:
						values = new ST_DynamicFilterType[] { ST_DynamicFilterType.lastYear, ST_DynamicFilterType.thisYear, ST_DynamicFilterType.nextYear };
						break;

					default:
						Utilities.DebugFail("Unknown RelativeDateValueDuration: " + this.Duration);
						return ST_DynamicFilterType._null;
				}

				return values[(int)this.Offset];
			}
		}

		#endregion // Type2007

		#endregion // Base Class Overrides

		#region Properties

		#region Duration

		/// <summary>
		/// Gets or sets the duration of the full range of accepted dates.
		/// </summary>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="RelativeDateRangeDuration"/> enumeration.
		/// </exception>
		/// <seealso cref="Offset"/>
		public RelativeDateRangeDuration Duration
		{
			get { return _duration; }
			set
			{
				if (this.Duration == value)
					return;

				Utilities.VerifyEnumValue(value);
				_duration = value;
				this.OnModified();
			}
		}

		#endregion // Duration

		#region Offset

		/// <summary>
		/// Gets or sets the offset of relative filter (previous, current, or next).
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Type combined with <see cref="Duration"/> determines the relative date range to filter.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// The value assigned is not defined in the <see cref="RelativeDateRangeOffset"/> enumeration.
		/// </exception>
		/// <seealso cref="Duration"/>
		public RelativeDateRangeOffset Offset
		{
			get { return _offset; }
			set
			{
				if (this.Offset == value)
					return;

				Utilities.VerifyEnumValue(value);
				_offset = value;
				this.OnModified();
			}
		}

		#endregion // Offset

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