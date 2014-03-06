using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Diagnostics;






namespace Infragistics.Controls

{
	#region DayOfWeekFlags
	/// <summary>
	/// A flagged enumeration used to identify days of the week.
	/// </summary>
	[Flags]
	public enum DayOfWeekFlags
	{
		// note: i'm specifically using the DayOfWeek objects to ensure
		// the values are consistent because I want to use those values
		// when seeing if a date's dayofweek is part of the enum

		/// <summary>
		/// None
		/// </summary>
		None = 0x0000,

		/// <summary>
		/// Sunday
		/// </summary>
		Sunday = 1 << (int)DayOfWeek.Sunday,

		/// <summary>
		/// Monday
		/// </summary>
		Monday = 1 << (int)DayOfWeek.Monday,

		/// <summary>
		/// Tuesday
		/// </summary>
		Tuesday = 1 << (int)DayOfWeek.Tuesday,

		/// <summary>
		/// Wednesday
		/// </summary>
		Wednesday = 1 << (int)DayOfWeek.Wednesday,

		/// <summary>
		/// Thursday
		/// </summary>
		Thursday = 1 << (int)DayOfWeek.Thursday,

		/// <summary>
		/// Friday
		/// </summary>
		Friday = 1 << (int)DayOfWeek.Friday,

		/// <summary>
		/// Saturday
		/// </summary>
		Saturday = 1 << (int)DayOfWeek.Saturday,
	}
	#endregion //DayOfWeekFlags

	internal class CalendarHelper
	{
		#region Member Variables

		private Calendar _calendar;
		private DateTimeFormatInfo _dateTimeFormat;

		// AS 10/3/08 TFS8633
		internal const DayOfWeekFlags AllDays = (DayOfWeekFlags)0x7F;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CalendarHelper"/>
		/// </summary>
		/// <param name="culture">The culture whose Calendar and DateTimeFormat are used to initialize the object</param>
		public CalendarHelper(CultureInfo culture)
			: this(culture.Calendar, culture.DateTimeFormat)
		{
		}

		/// <summary>
		/// Initializes a new <see cref="CalendarHelper"/>
		/// </summary>
		/// <param name="calendar">The Calendar used to perform date manipulations/calculations</param>
		/// <param name="formatInfo">The format info used to provide week rule and other information</param>
		public CalendarHelper(Calendar calendar, DateTimeFormatInfo formatInfo)
		{
			this.Initialize(calendar, formatInfo);
		}
		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Calendar

		/// <summary>
		/// Returns the associated Calendar
		/// </summary>
		public Calendar Calendar
		{
			get { return this._calendar; }
		}
		#endregion //Calendar

		#region DateTimeFormat

		/// <summary>
		/// Returns the associated DateTimeFormatInfo
		/// </summary>
		public DateTimeFormatInfo DateTimeFormat
		{
			get { return this._dateTimeFormat; }
		}
		#endregion //DateTimeFormat

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region VerifyCanChange
		/// <summary>
		/// Used to ensure that the instance is allowed to be modified
		/// </summary>
		protected virtual void VerifyCanChange()
		{
		}
		#endregion // VerifyCanChange

		#region Initialize
		/// <summary>
		/// Provides the Calendar and DateTimeFormatInfo instances used by the object for its calculations.
		/// </summary>
		/// <param name="calendar">The calendar used to perform the date manipulations/calculations</param>
		/// <param name="formatInfo">The object used to provide formatting information such as the first DayOfWeek</param>
		protected void Initialize(Calendar calendar, DateTimeFormatInfo formatInfo)
		{
			if (_calendar != null)
				this.VerifyCanChange();

			CoreUtilities.ValidateNotNull(calendar, "calendar");
			CoreUtilities.ValidateNotNull(formatInfo, "formatInfo");

			_calendar = calendar;
			_dateTimeFormat = formatInfo;
		}
		#endregion // Initialize

		#endregion // Protected Methods

		#region Internal Methods

		#region Coerce Min|Max Date

		/// <summary>
		/// Ensures that the date is within the date range supported by the associated <see cref="Calendar"/>
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The specified date if it is within the allowed range; otherwise the min or max support DateTime of the associated Calendar if the specified date is beyond that supported by the calendar.</returns>
		internal DateTime CoerceMinMaxDate(DateTime date)
		{
			date = date.Date;

			if (date < this._calendar.MinSupportedDateTime)
				date = this._calendar.MinSupportedDateTime;
			else if (date > this._calendar.MaxSupportedDateTime)
				date = this._calendar.MaxSupportedDateTime;

			return date;
		}
		#endregion //Coerce Min|Max Date

		#region GetMinMax
		internal static DateTime GetMinMax(Calendar calendar, bool min)
		{
			return min ? calendar.MinSupportedDateTime : calendar.MaxSupportedDateTime;
		}
		#endregion //GetMinMax

		#region GetVisibleDayCount
		/// <summary>
		/// Calculates the number of visible days of the week between the specified day of the week and the 
		/// </summary>
		/// <param name="hiddenDays">The hidden days</param>
		/// <param name="firstDayOfWeek">Day considered the start of the week</param>
		/// <param name="dayOfWeek">Day of the week to evaluate</param>
		/// <returns></returns>
		internal static int GetVisibleDayCount(DayOfWeekFlags hiddenDays, DayOfWeek firstDayOfWeek, DayOfWeek dayOfWeek)
		{
			Debug.Assert(false == IsSet(hiddenDays, dayOfWeek), "The date being evaluated is hidden!");

			int firstDow = (int)firstDayOfWeek;
			int dow = (int)dayOfWeek;
			int count = 0;

			if (firstDow > dow)
				dow += 7;

			while (dow >= firstDow)
			{
				if (false == IsSet(hiddenDays, (DayOfWeek)dow))
					count++;

				dow--;
			}

			return count - 1;
		}
		#endregion //GetVisibleDayCount

		#region IsSameMonth
		internal bool IsSameMonth( DateTime date1, DateTime date2 )
		{
			var calendar = _calendar;

			if ( calendar.GetEra(date1) != calendar.GetEra(date2) )
				return false;

			if ( calendar.GetYear(date1) != calendar.GetYear(date2) )
				return false;

			if ( calendar.GetMonth(date1) != calendar.GetMonth(date2) )
				return false;

			return true;
		}
		#endregion // IsSameMonth

		#region IsSet
		/// <summary>
		/// Indicates if the specified day of the week is set in the specified day of week flags.
		/// </summary>
		/// <param name="days">The flags to evaluate</param>
		/// <param name="day">The day of week to evaluate</param>
		/// <returns>True if the day is included in the specified flags, otherwise false is returned.</returns>
		internal static bool IsSet(DayOfWeekFlags days, DayOfWeek day)
		{
			return 0 != ((1 << (int)day) & (int)days);
		}
		#endregion //IsSet

		#endregion // Internal Methods

		#region Public Methods

		#region AddDays

		/// <summary>
		/// Adds the specified number of days to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="days">The number of days by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddDays(DateTime date, int days)
		{
			return AddDays(date, days, _calendar);
		}

		internal static DateTime AddDays(DateTime date, int days, Calendar calendar)
		{
			try
			{
				return calendar.AddDays(date, days);
			}
			catch (ArgumentException)
			{
				return GetMinMax(calendar, days < 0);
			}
		}
		#endregion //AddDays

		#region AddHours

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Adds the specified number of hours to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="hours">The number of hours by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		internal DateTime AddHours(DateTime date, int hours)
		{
			return AddHours(date, hours, _calendar);
		}

		internal static DateTime AddHours(DateTime date, int hours, Calendar calendar)
		{
			try
			{
				return calendar.AddHours(date, hours);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, hours < 0);
			}
		}

		#endregion // AddHours

		#region AddMinutes

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Adds the specified number of minutes to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="minutes">The number of minutes by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddMinutes(DateTime date, int minutes)
		{
			return AddMinutes(date, minutes, _calendar);
		}

		internal static DateTime AddMinutes(DateTime date, int minutes, Calendar calendar)
		{
			try
			{
				return calendar.AddMinutes(date, minutes);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, minutes < 0);
			}
		}

		#endregion // AddMinutes

		#region AddMonths
		/// <summary>
		/// Adds the specified number of months to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="months">The number of months by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddMonths(DateTime date, int months)
		{
			return AddMonths(date, months, _calendar);
		}

		internal static DateTime AddMonths(DateTime date, int months, Calendar calendar)
		{
			try
			{
				return calendar.AddMonths(date, months);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, months < 0);
			}
		}
		#endregion //AddMonths

		#region AddSeconds

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Adds the specified number of seconds to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="seconds">The number of seconds by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddSeconds(DateTime date, int seconds)
		{
			return AddSeconds(date, seconds, _calendar);
		}

		internal static DateTime AddSeconds(DateTime date, int seconds, Calendar calendar)
		{
			try
			{
				return calendar.AddSeconds(date, seconds);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, seconds < 0);
			}
		}

		#endregion // AddSeconds

		#region AddWeeks

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Adds the specified number of weeks to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="weeks">The number of weeks by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddWeeks(DateTime date, int weeks)
		{
			return AddWeeks(date, weeks, _calendar);
		}

		internal static DateTime AddWeeks(DateTime date, int weeks, Calendar calendar)
		{
			try
			{
				return calendar.AddWeeks(date, weeks);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, weeks < 0);
			}
		}

		#endregion // AddWeeks

		#region AddYears

		/// <summary>
		/// Adds the specified number of years to the specified date
		/// </summary>
		/// <param name="date">The source date to which the offset will be added</param>
		/// <param name="years">The number of years by which to adjust the <paramref name="date"/></param>
		/// <returns>The adjusted date or the max/min supported date of the calendar if the result was outside the allowed range</returns>
		public DateTime AddYears(DateTime date, int years)
		{
			return AddYears(date, years, _calendar);
		}

		internal static DateTime AddYears(DateTime date, int years, Calendar calendar)
		{
			try
			{
				return calendar.AddYears(date, years);
			}
			catch (ArgumentOutOfRangeException)
			{
				return GetMinMax(calendar, years < 0);
			}
		}
		#endregion //AddYears

		#region GetFirstDayOfWeekForDate

		/// <summary>
		/// Returns the date that represents the start of the week that contains the specified date.
		/// </summary>
		/// <param name="date">The date whose week is to be evaluated</param>
		/// <param name="firstDayOfWeek">Optional value that indicates the first day of week to use for the calculation</param>
		/// <param name="additionalOffset">Out param that is initialized with the number of days before the returned date that would be the first day of the week.</param>
		/// <returns>The date that represents the first day of the week.</returns>
		public DateTime GetFirstDayOfWeekForDate(DateTime date, DayOfWeek? firstDayOfWeek, out int additionalOffset)
		{
			int offset = GetDayOfWeekNumber(date, DayOfWeekFlags.None, firstDayOfWeek);

			try
			{
				date = _calendar.AddDays(date, -offset);

				// if we were able to calculate the first week date 
				// then there is no additional offset
				additionalOffset = 0;
			}
			catch
			{
				additionalOffset = offset - date.Subtract(_calendar.MinSupportedDateTime).Days;
				date = _calendar.MinSupportedDateTime;
			}

			return date;
		}

		#endregion //GetFirstDayOfWeekForDate

		#region GetDaysInMonth

		/// <summary>
		/// Returns the number of days in the month for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The number of days in the month for the specified date</returns>
		public int GetDaysInMonth(DateTime date)
		{
			return GetDaysInMonth(date, this._calendar);
		}

		internal static int GetDaysInMonth(DateTime date, Calendar calendar)
		{
			int month = calendar.GetMonth(date);
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);

			return calendar.GetDaysInMonth(year, month, era);
		}
		#endregion //GetDaysInMonth

		#region GetDaysInYear

		// SSP 7/27/10 - XamSchedule
		// 
		/// <summary>
		/// Returns the number of the days in the year for the year containing the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The number of days in the year for the year containing the specified date</returns>
		public int GetDaysInYear(DateTime date)
		{
			return GetDaysInYear(date, _calendar);
		}

		internal static int GetDaysInYear(DateTime date, Calendar calendar)
		{
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);

			return calendar.GetDaysInYear(year, era);
		}

		#endregion // GetDaysInYear

		#region GetDayOfMonth

		/// <summary>
		/// Returns the day of the month for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The day of the month</returns>
		public int GetDayOfMonth(DateTime date)
		{
			return this._calendar.GetDayOfMonth(date);
		}
		#endregion //GetDayOfMonth

		#region GetDayOfWeek

		/// <summary>
		/// Returns the day of the week for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The day of the week</returns>
		public DayOfWeek GetDayOfWeek(DateTime date)
		{
			return this._calendar.GetDayOfWeek(date);
		}
		#endregion //GetDayOfWeek

		#region GetDayOfWeekNumber

		/// <summary>
		/// Returns the number of the day of the week based on the first day of week.
		/// </summary>
		/// <param name="date">The date whose offset is to be calculated</param>
		/// <param name="hiddenDays">The days of the week that should be considered hidden</param>
		/// <param name="firstDayOfWeek">Optional value that indicates the first day of week to use for the calculation</param>
		/// <returns>A number between 0 and 6 that represents how many days into the week, the specified day represents based on the current first day of week.</returns>
		public int GetDayOfWeekNumber(DateTime date, DayOfWeekFlags hiddenDays, DayOfWeek? firstDayOfWeek)
		{
			return GetDayOfWeekNumber(date, hiddenDays, firstDayOfWeek ?? _dateTimeFormat.FirstDayOfWeek, this._calendar);
		}

		internal static int GetDayOfWeekNumber(DateTime date, DayOfWeekFlags hiddenDays, DayOfWeek firstDayOfWeek, Calendar calendar)
		{
			// if we have hidden days then we need to 
			if (hiddenDays != DayOfWeekFlags.None)
			{
				return GetVisibleDayCount(hiddenDays, firstDayOfWeek, calendar.GetDayOfWeek(date));
			}

			DayOfWeek dow = calendar.GetDayOfWeek(date);

			return ((int)dow - (int)firstDayOfWeek + 7) % 7;
		}
		#endregion //GetDayOfWeekNumber

		#region GetDayOfYear

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Returns the day of the year for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The day of the year</returns>
		internal int GetDayOfYear( DateTime date )
		{
			return _calendar.GetDayOfYear( date );
		}

		#endregion // GetDayOfYear

		#region GetFirstWeekOfYearDate
		internal static DateTime GetFirstWeekOfYearDate(int year, int? era, CalendarWeekRule weekRule, DayOfWeek firstDayOfWeek, Calendar calendar, out int additionalOffset)
		{
			Debug.Assert(year > 0, "Invalid year");

			DateTime date = era == null
				? calendar.ToDateTime(year, 1, 1, 0, 0, 0, 0)
				: calendar.ToDateTime(year, 1, 1, 0, 0, 0, 0, (int)era);

			DayOfWeek dayOfWeek = calendar.GetDayOfWeek(date);

			// how many days between this day and the first day of the week
			int daysFromWeekStart = (int)dayOfWeek - (int)firstDayOfWeek;

			#region Old Code
			
#region Infragistics Source Cleanup (Region)





































































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Old Code
			int offset = 0;

			#region Calculate Offset
			switch (weekRule)
			{
				case CalendarWeekRule.FirstFourDayWeek:
					{
						// if there are more than 3 days from the previous year
						// move to the next week
						if (daysFromWeekStart > 3)
						{
							offset = 7 - daysFromWeekStart;
						}
						else if (daysFromWeekStart < -3)
						{
							offset = -(7 + daysFromWeekStart);
						}
						else
						{
							// otherwise back up to the start of the week
							offset = -daysFromWeekStart;
						}
						break;
					}

				case CalendarWeekRule.FirstFullWeek:
					{
						if (daysFromWeekStart > 0)
						{
							offset = 7 - daysFromWeekStart;
						}
						else if (daysFromWeekStart < 0)
						{
							offset = -daysFromWeekStart;
						}

						break;
					}

				default:
				case CalendarWeekRule.FirstDay:
					{
						if (daysFromWeekStart < 0)
							offset = -(7 + daysFromWeekStart);
						else
						{
							// back up to the beginning of the week containing the first of the year
							offset = -daysFromWeekStart;
						}

						break;
					}
			}
			#endregion //Calculate Offset

			try
			{
				date = date.AddDays(offset);

				// if we were able to calculate the first week date 
				// then there is no additional offset
				additionalOffset = 0;
			}
			catch
			{
				additionalOffset = offset - calendar.MinSupportedDateTime.Subtract(date).Days;
				date = calendar.MinSupportedDateTime;
			}

			return date;
		}
		#endregion //GetFirstWeekOfYearDate

		#region GetMonthNumber

		/// <summary>
		/// Returns the month number for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The month number</returns>
		public int GetMonthNumber(DateTime date)
		{
			return this._calendar.GetMonth(date);
		}
		#endregion //GetMonthNumber

		#region GetWeekNumberForDate
		/// <summary>
		/// Returns the week number for a particular date using the specified CalendarWeekRule and FirstDayOfWeek.
		/// </summary>
		/// <param name="date">DateTime</param>
		/// <param name="weekRule">Optional week rule to use for the calculation</param>
		/// <param name="firstDayOfWeek">Optional first day of week to use for the calculation</param>
		/// <param name="yearContainingWeek">Out integer to receive the year for which the week number belongs.</param>
		/// <returns>Integer denoting the week number that the date belongs to.</returns>
		/// <remarks>
		/// <p class="body">The System.Globalization.Calendar's GetWeekOfYear method simply returns the number of weeks into the year that the date falls on. It does not seem to use the weekrule specified in the arguments. e.g. If the first day of week is sunday and the weekrule is Jan 1, then 12/31/2000 (Sunday) should return 1. Instead it returns 54 and 1/1/2001 (Monday) returns 1.</p>
		/// <p class="body">This routine returns the correct week number for the specified date based on the CalendarWeekRule and FirstDayOfWeek.</p>
		/// <p class="note"> Note, this may be a week for a year other than the year of the date. e.g. 12/31/2000, week tt of Jan 1, and week start of sunday will return 1 because this date falls into the first week of the year 2001.</p>
		/// </remarks>
		public int GetWeekNumberForDate(DateTime date, CalendarWeekRule? weekRule, DayOfWeek? firstDayOfWeek, out int yearContainingWeek)
		{
			return GetWeekNumberForDate(date, weekRule ?? _dateTimeFormat.CalendarWeekRule, firstDayOfWeek ?? _dateTimeFormat.FirstDayOfWeek, _calendar, out yearContainingWeek);
		}

		/// <summary>
		/// Returns the week number for a particular date using the specified CalendarWeekRule and FirstDayOfWeek.
		/// </summary>
		/// <param name="date">DateTime</param>
		/// <param name="weekRule">WeekRule to use for the calculation</param>
		/// <param name="firstDayOfWeek">First day of week to use for the calculation</param>
		/// <param name="calendar">Calendar to use for the calculations</param>
		/// <param name="yearContainingWeek">Out integer to receive the year for which the week number belongs.</param>
		/// <returns>Integer denoting the week number that the date belongs to.</returns>
		/// <remarks>
		/// <p class="body">The System.Globalization.Calendar's GetWeekOfYear method simply returns the number of weeks into the year that the date falls on. It does not seem to use the weekrule specified in the arguments. e.g. If the first day of week is sunday and the weekrule is Jan 1, then 12/31/2000 (Sunday) should return 1. Instead it returns 54 and 1/1/2001 (Monday) returns 1.</p>
		/// <p class="body">This routine returns the correct week number for the specified date based on the CalendarWeekRule and FirstDayOfWeek.</p>
		/// <p class="note"> Note, this may be a week for a year other than the year of the date. e.g. 12/31/2000, week tt of Jan 1, and week start of sunday will return 1 because this date falls into the first week of the year 2001.</p>
		/// </remarks>
		internal static int GetWeekNumberForDate(DateTime date, CalendarWeekRule weekRule, DayOfWeek firstDayOfWeek, Calendar calendar, out int yearContainingWeek)
		{
			int year = calendar.GetYear(date);

			if (year < calendar.GetYear(calendar.MaxSupportedDateTime))
				year++;

			int era = calendar.GetEra(date);
			// AS 9/5/08
			// This logic is incorrect for the first year of the calendar. Since
			// we may not be able to offset the needed # of days the date we get
			// back as the first year will not be a day that is the first day
			// of the week.
			//
			//DateTime firstWeekDate = this.GetFirstWeekOfYearDate(year, era);
			int additionalOffset;
			DateTime firstWeekDate = GetFirstWeekOfYearDate(year, era, weekRule, firstDayOfWeek, calendar, out additionalOffset);

			while (firstWeekDate > date)
			{
				if (year == 1)
				{
					yearContainingWeek = 0;
					return 0;
				}

				// AS 9/5/08
				//firstWeekDate = this.GetFirstWeekOfYearDate(--year, era);
				firstWeekDate = GetFirstWeekOfYearDate(--year, era, weekRule, firstDayOfWeek, calendar, out additionalOffset);
			}

			yearContainingWeek = year;

			TimeSpan timeDiff = date.Subtract(firstWeekDate);

			// AS 9/5/08
			//return ((int)timeDiff.Days / 7) + 1;
			return (((int)timeDiff.Days - additionalOffset) / 7) + 1;
		}
		#endregion //GetWeekNumberForDate

		#region GetYear

		// SSP 8/12/10 - XamSchedule
		// 
		/// <summary>
		/// Returns the year for the specified date
		/// </summary>
		/// <param name="date">The date to evaluate</param>
		/// <returns>The year number</returns>
		public int GetYear(DateTime date)
		{
			return _calendar.GetYear(date);
		}

		#endregion // GetYear

		#region TryAddOffset
		/// <summary>
		/// Adds the specified offset to the specified date
		/// </summary>
		/// <param name="date">The date to adjust</param>
		/// <param name="offset">The amount of time by which to offset the <paramref name="date"/></param>
		/// <param name="offsetType">The unit type that the <paramref name="offset"/> represents</param>
		/// <param name="adjustedDate">Out parameter set to the adjusted date or the min/max supported date time if the adjusted date goes outside the range supported by the associated calendar</param>
		/// <returns>True if the date was able to be adjusted by the specified amount; otherwise false if the resulting value went outside the range supported by the calendar</returns>
		public bool TryAddOffset(DateTime date, int offset, DateTimeOffsetType offsetType, out DateTime adjustedDate)
		{
			try
			{
				switch (offsetType)
				{
					case DateTimeOffsetType.Days:
						adjustedDate = _calendar.AddDays(date, offset);
						break;
					case DateTimeOffsetType.Weeks:
						adjustedDate = _calendar.AddWeeks(date, offset);
						break;
					case DateTimeOffsetType.Months:
						adjustedDate = _calendar.AddMonths(date, offset);
						break;
					case DateTimeOffsetType.Years:
						adjustedDate = _calendar.AddYears(date, offset);
						break;
					case DateTimeOffsetType.Hours:
						adjustedDate = _calendar.AddHours(date, offset);
						break;
					case DateTimeOffsetType.Minutes:
						adjustedDate = _calendar.AddMinutes(date, offset);
						break;
					case DateTimeOffsetType.Seconds:
						adjustedDate = _calendar.AddSeconds(date, offset);
						break;
					default:
						throw new ArgumentException();
				}

				return true;
			}
			catch (ArgumentException)
			{
				adjustedDate = GetMinMax(_calendar, offset < 0);
				return false;
			}
		}
		#endregion // TryAddOffset

		#endregion // Public Methods

		#endregion // Methods

		#region DateTimeOffsetType enum
		internal enum DateTimeOffsetType : byte
		{
			Days,
			Weeks,
			Months,
			Years,
			Hours,
			Minutes,
			Seconds,
		}
		#endregion // DateTimeOffsetType enum
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