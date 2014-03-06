using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Globalization;
using System.Security;
using System.ComponentModel;
using System.Collections;
using Infragistics.Documents.Excel.CalcEngine;
using Infragistics.Documents.Excel.Serialization.Excel2007;


using Infragistics.Shared;


namespace Infragistics.Documents.Excel.Filtering
{
	// MD 12/13/11 - 12.1 - Table Support



	/// <summary>
	/// Represents a fixed range of dates.
	/// </summary>
	/// <seealso cref="FixedValuesFilter.DateGroups"/>
	[InfragisticsFeature(Version = FeatureInfo.Version_12_1, FeatureName = FeatureInfo.FeatureName_ExcelTables)]
	public

 class FixedDateGroup
	{
		#region Member Variables

		private readonly DateRange _range;
		private readonly Dictionary<CalendarType, DateRange> _rangesByCalendar;
		private readonly FixedDateGroupType _type;
		private readonly DateTime _value;

		#endregion // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="FixedDateGroup"/> instance.
		/// </summary>
		/// <param name="type">The type, or precision, of the group.</param>
		/// <param name="value">The reference date which determines range of accepted dates.</param>
		/// <remarks>
		/// <p class="body">
		/// <paramref name="type"/> indicates the precision of <paramref name="value"/>, which defines the range of accepted dates in the group.
		/// For example, if the type is Hour and value is 12/19/2011 1:29:13 PM, the date range allowed by the <see cref="FixedDateGroup"/>
		/// would be 12/19/2011 1:00:00 PM to 12/19/2011 1:59:59 PM.
		/// </p>
		/// </remarks>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="type"/> is not defined in the <see cref="FixedDateGroupType"/> enumeration.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// <paramref name="value"/> cannot be defined in the Excel.
		/// </exception>
		public FixedDateGroup(FixedDateGroupType type, DateTime value)
		{
			Utilities.VerifyEnumValue(type);

			if (ExcelCalcValue.DateTimeToExcelDate(null, value).HasValue == false)
				throw new ArgumentException(SR.GetString("LE_ArgumentException_InvalidExcelDate"), "value");

			_type = type;
			_value = value;

			_rangesByCalendar = new Dictionary<CalendarType, DateRange>();
			this.GetRange(null, CalendarType.None, out _range);
		}

		#endregion // Constructor

		#region Base Class Overrides

		#region Equals

		/// <summary>
		/// Determines whether the <see cref="FixedDateGroup"/> is equal to the specified object.
		/// </summary>
		/// <param name="obj">The object to test for equality.</param>
		/// <returns>True if the object is equal to this instance; False otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (Object.ReferenceEquals(this, obj))
				return true;

			FixedDateGroup other = obj as FixedDateGroup;
			if (other == null)
				return false;

			return
				_type == other._type &&
				_range.Start == other._range.Start &&
				_range.End == other._range.End;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Gets the hash code for the <see cref="FixedDateGroup"/>.
		/// </summary>
		/// <returns>A number which can be used to hash this instance.</returns>
		public override int GetHashCode()
		{
			return _type.GetHashCode() ^ _range.Start.GetHashCode() ^ (_range.End.GetHashCode() << 1);
		}

		#endregion // GetHashCode

		#endregion // Base Class Overrides

		#region Methods

		#region Public Methods

		#region GetRange

		/// <summary>
		/// Gets the accepted date range based on the specified calendar type.
		/// </summary>
		/// <param name="calendarType">The calendar type in which to get the accepted date range.</param>
		/// <param name="start">Out parameter which will contain the inclusive start date of the accepted date range.</param>
		/// <param name="end">Out parameter which will contain the exclusive end date of the accepted date range.</param>
		/// <exception cref="InvalidEnumArgumentException">
		/// <paramref name="calendarType"/> is not defined in the <see cref="CalendarType"/> enumeration.
		/// </exception>
		/// <seealso cref="Start"/>
		/// <seealso cref="End"/>
		public void GetRange(CalendarType calendarType, out DateTime start, out DateTime end)
		{
			Utilities.VerifyEnumValue(calendarType);

			DateRange range;
			this.GetRange(null, calendarType, out range);

			start = range.Start;
			end = range.End;
		}

		#endregion // GetRange

		#endregion // Public Methods

		#region Internal Methods

		#region CreateFixedDateGroup

		internal static FixedDateGroup CreateFixedDateGroup(ST_DateTimeGrouping dateTimeGrouping, ushort year, ushort month, ushort day, ushort hour, ushort minute, ushort second)
		{
			switch (dateTimeGrouping)
			{
				case ST_DateTimeGrouping.second:
					return new FixedDateGroup(FixedDateGroupType.Second, new DateTime(year, month, day, hour, minute, second));

				case ST_DateTimeGrouping.minute:
					return new FixedDateGroup(FixedDateGroupType.Minute, new DateTime(year, month, day, hour, minute, 0));

				case ST_DateTimeGrouping.hour:
					return new FixedDateGroup(FixedDateGroupType.Hour, new DateTime(year, month, day, hour, 0, 0));

				case ST_DateTimeGrouping.day:
					return new FixedDateGroup(FixedDateGroupType.Day, new DateTime(year, month, day));

				case ST_DateTimeGrouping.month:
					return new FixedDateGroup(FixedDateGroupType.Month, new DateTime(year, month, 1));

				case ST_DateTimeGrouping.year:
					return new FixedDateGroup(FixedDateGroupType.Year, new DateTime(year, 1, 1));

				default:
					Utilities.DebugFail("Unknown ST_DateTimeGrouping: " + dateTimeGrouping);
					return null;
			}
		}

		#endregion // CreateFixedDateGroup

		#region GetRange

		internal void GetRange(Calendar calendar, CalendarType calendarType, out DateRange range)
		{
			lock (this._rangesByCalendar)
			{
				if (this._rangesByCalendar.TryGetValue(calendarType, out range))
					return;

				if (calendar == null)
					calendar = Utilities.CreateCalendar(calendarType);

				int era = calendar.GetEra(_value);
				int yearInEra = calendar.GetYear(_value);

				DateTime start;
				DateTime end;

				switch (this.Type)
				{
					case FixedDateGroupType.Second:
						start = new DateTime(_value.Year, _value.Month, _value.Day, _value.Hour, _value.Minute, _value.Second);
						end = start.AddSeconds(1);
						break;

					case FixedDateGroupType.Minute:
						start = new DateTime(_value.Year, _value.Month, _value.Day, _value.Hour, _value.Minute, 0);
						end = start.AddMinutes(1);
						break;

					case FixedDateGroupType.Hour:
						start = new DateTime(_value.Year, _value.Month, _value.Day, _value.Hour, 0, 0);
						end = start.AddHours(1);
						break;

					case FixedDateGroupType.Day:
						start = new DateTime(_value.Year, _value.Month, _value.Day, 0, 0, 0);
						end = start.AddDays(1);
						break;

					case FixedDateGroupType.Month:
						int monthInEra = calendar.GetMonth(_value);
						start = calendar.ToDateTime(yearInEra, monthInEra, 1, 0, 0, 0, 0, era);
						end = start.AddDays(calendar.GetDaysInMonth(yearInEra, monthInEra, era));
						break;

					case FixedDateGroupType.Year:
						start = calendar.ToDateTime(yearInEra, 1, 1, 0, 0, 0, 0, era);
						end = start.AddDays(calendar.GetDaysInYear(yearInEra, era));
						break;

					default:
						Utilities.DebugFail("Unknown FixedDateGroupType: " + this.Type);
						goto case FixedDateGroupType.Second;
				}

				range = new DateRange(start, end);
				_rangesByCalendar.Add(calendarType, range);
			}
		}

		#endregion // GetRange

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region End

		/// <summary>
		/// Gets the exclusive end date of the accepted date range with a <see cref="CalendarType"/> of None.
		/// </summary>
		/// <seealso cref="Start"/>
		/// <seealso cref="GetRange(CalendarType,out DateTime,out DateTime)"/>
		public DateTime End
		{
			get { return _range.End; }
		}

		#endregion // End

		#region Start

		/// <summary>
		/// Gets the inclusive start date of the accepted date range with a <see cref="CalendarType"/> of None.
		/// </summary>
		/// <seealso cref="End"/>
		/// <seealso cref="GetRange(CalendarType,out DateTime,out DateTime)"/>
		public DateTime Start
		{
			get { return _range.Start; }
		}

		#endregion // Start

		#region Type

		/// <summary>
		/// Gets the type, or precision, of the group.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// The group type indicates the precision of the <see cref="Value"/>, which defines the range of accepted dates in the group.
		/// For example, if the Type is Hour and the Value is 12/19/2011 1:29:13 PM, the date range allowed by the <see cref="FixedDateGroup"/>
		/// would be 12/19/2011 1:00:00 PM to 12/19/2011 1:59:59 PM.
		/// </p>
		/// </remarks>
		/// <seealso cref="Value"/>
		public FixedDateGroupType Type
		{
			get { return _type; }
		}

		#endregion // Type

		#region Value

		/// <summary>
		/// Gets the reference date which determines range of accepted dates.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// <see cref="Type"/> indicates the precision of the Value, which defines the range of accepted dates in the group.
		/// For example, if the Type is Hour and the Value is 12/19/2011 1:29:13 PM, the date range allowed by the <see cref="FixedDateGroup"/>
		/// would be 12/19/2011 1:00:00 PM to 12/19/2011 1:59:59 PM.
		/// </p>
		/// </remarks>
		/// <seealso cref="Type"/>
		public DateTime Value
		{
			get { return _value; }
		}

		#endregion // Value

		#endregion // Public Properties

		#region Internal Properties

		#region DateTimeGrouping

		internal ST_DateTimeGrouping DateTimeGrouping
		{
			get
			{
				switch (this.Type)
				{
					case FixedDateGroupType.Second:
						return ST_DateTimeGrouping.second;

					case FixedDateGroupType.Minute:
						return ST_DateTimeGrouping.minute;

					case FixedDateGroupType.Hour:
						return ST_DateTimeGrouping.hour;

					case FixedDateGroupType.Day:
						return ST_DateTimeGrouping.day;

					case FixedDateGroupType.Month:
						return ST_DateTimeGrouping.month;

					case FixedDateGroupType.Year:
						return ST_DateTimeGrouping.year;

					default:
						Utilities.DebugFail("Unknown FixedDateGroupType: " + this.Type);
						return ST_DateTimeGrouping.year;
				}
			}
		}

		#endregion // DateTimeGrouping

		#endregion // Internal Properties

		#endregion // Properties


		#region DateRange struct

		internal struct DateRange
		{
			public DateTime End;
			public DateTime Start;

			public DateRange(DateTime start, DateTime end)
			{
				this.Start = start;
				this.End = end;
			}
		}

		#endregion // DateRange struct
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