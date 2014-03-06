using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Diagnostics;
using Infragistics.Controls;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Editors
{
	#region CalendarZoomMode
	/// <summary>
	/// Enumeration that determines what is displayed and interacted within each <see cref="CalendarItemGroup"/> of a <see cref="CalendarBase"/>
	/// </summary>
	/// <seealso cref="XamCalendar.MinCalendarMode"/>
	/// <seealso cref="CalendarBase.CurrentMode"/>
	public enum CalendarZoomMode : short
	{
		

		/// <summary>
		/// Each <see cref="CalendarItemGroup"/> of the <see cref="CalendarBase"/> is displaying days of the month.
		/// </summary>
		Days,

		/// <summary>
		/// Each <see cref="CalendarItemGroup"/> of the <see cref="CalendarBase"/> is displaying months of the year.
		/// </summary>
		Months,

		/// <summary>
		/// Each <see cref="CalendarItemGroup"/> of the <see cref="CalendarBase"/> is displaying years of a decade.
		/// </summary>
		Years,

		/// <summary>
		/// Each <see cref="CalendarItemGroup"/> of the <see cref="CalendarBase"/> is displaying ranges of 10 years of a century.
		/// </summary>
		Decades,

		/// <summary>
		/// Each <see cref="CalendarItemGroup"/> of the <see cref="CalendarBase"/> is displaying ranges of 10 years of a century.
		/// </summary>
		Centuries,

		
		
		
		
	}
	#endregion //CalendarZoomMode

	#region DayOfWeekHeaderFormat

	/// <summary>
	/// Enumeration used to identify the format of the day of week captions in the <see cref="CalendarBase"/>
	/// </summary>
	public enum DayOfWeekHeaderFormat
	{
		/// <summary>
		/// Only a single character is used to identify the days of the week
		/// </summary>
		SingleCharacter,

		/// <summary>
		/// Two characters are used to identify the days of the week
		/// </summary>
		TwoCharacters,

		/// <summary>
		/// The abbreviated day of week name from the <see cref="System.Globalization.DateTimeFormatInfo"/> is used.
		/// </summary>
		Abbreviated,

		/// <summary>
		/// The full day of week name from <see cref="System.Globalization.DateTimeFormatInfo"/> is used.
		/// </summary>
		Full,
	}
	#endregion //DayOfWeekHeaderFormat






	internal class CalendarManager : CalendarHelper
	{
		#region Member Variables

		private DayOfWeek? _firstDayOfWeek;
		private CalendarWeekRule? _weekRule;
		private DayOfWeekFlags _hiddenDays;
		private int _visibleDayCount = 7;

		private static CalendarManager currentCulture;

		#endregion //Member Variables

		#region Constructor

		internal CalendarManager()
			: base(CultureInfo.CurrentCulture)
		{
		}

		internal CalendarManager(CultureInfo culture)
			: base(culture)
		{
		}
		#endregion //Constructor

		#region Properties

		#region CurrentCulture
		internal static CalendarManager CurrentCulture
		{
			get
			{
				if (null == CalendarManager.currentCulture)
					CalendarManager.currentCulture = new CalendarManager();

				return CalendarManager.currentCulture;
			}
		}
		#endregion //CurrentCulture

		#region HiddenDays
		public DayOfWeekFlags HiddenDays
		{
			get { return this._hiddenDays; }
			set
			{
				this.VerifyCanChange();
				this._hiddenDays = value;
				this._visibleDayCount = 0;

				for (int i = 0; i < 7; i++)
				{
					if (false == IsSet(value, (DayOfWeek)i))
						this._visibleDayCount++;
				}

				// AS 10/3/08 TFS8633
				//if (this._visibleDayCount == 0)
				//    throw new NotSupportedException("Cannot hide all days of the week");
			}
		}
		#endregion //HiddenDays

		#region FirstDayOfWeek

		public DayOfWeek? FirstDayOfWeek
		{
			get { return this._firstDayOfWeek; }
			set
			{
				this.VerifyCanChange();

				this._firstDayOfWeek = value;
			}
		}
		#endregion //FirstDayOfWeek

		#region FirstDayOfWeekResolved

		internal DayOfWeek FirstDayOfWeekResolved
		{
			get
			{
				return this._firstDayOfWeek.HasValue
				  ? this._firstDayOfWeek.Value
				  : this.DateTimeFormat.FirstDayOfWeek;
			}
		}
		#endregion //FirstDayOfWeekResolved

		#region VisibleDayCount
		public int VisibleDayCount
		{
			get { return this._visibleDayCount; }
		}
		#endregion //VisibleDayCount

		#region WeekRule

		public CalendarWeekRule? WeekRule
		{
			get { return this._weekRule; }
			set
			{
				this.VerifyCanChange();

				this._weekRule = value;
			}
		}
		#endregion //WeekRule

		#region WeekRuleResolved

		internal CalendarWeekRule WeekRuleResolved
		{
			get
			{
				return this._weekRule.HasValue
				  ? this._weekRule.Value
				  : this.DateTimeFormat.CalendarWeekRule;
			}
		}
		#endregion //WeekRuleResolved

		#endregion //Properties

		#region Methods

		#region AddGroupOffset
		internal DateTime? TryAddGroupOffset(DateTime date, int offset, CalendarZoomMode mode, bool adjustForStart)
		{
			try
			{
				switch (mode)
				{
					default:
					case CalendarZoomMode.Days:
						date = this.Calendar.AddMonths(date, offset);
						break;
					case CalendarZoomMode.Months:
						date = this.Calendar.AddYears(date, offset);
						break;
					case CalendarZoomMode.Years:
					case CalendarZoomMode.Decades:
					case CalendarZoomMode.Centuries:
						
						int adjustment = (int)Math.Pow(10, 1 + (mode - CalendarZoomMode.Years));
						date = this.Calendar.AddYears(date, offset * adjustment);
						break;
				}
			}
			catch (ArgumentException)
			{
				// AS 2/19/09 TFS11496
				// If we're getting a previous date we may have tried to go before the 
				// allowable start date. If that happens we'll get the start of the next
				// group. If that group is valid and it starts before our date and after
				// the minimum date supported by the calendar then we'll return the minimum
				// supported date for the calendar (adjusted if needed).
				//
				if (offset < 0)
				{
					DateTime? nextGroup = TryAddGroupOffset(date, offset + 1, mode, false);

					if (nextGroup != null &&
						nextGroup > Calendar.MinSupportedDateTime &&
						nextGroup <= date)
					{
						date = Calendar.MinSupportedDateTime;

						if (adjustForStart)
							date = this.GetGroupStartDate(date, mode);

						return date;
					}
				}

				return null;
			}

			if (adjustForStart)
				date = this.GetGroupStartDate(date, mode);

			return date;
		}

		internal DateTime AddGroupOffset(DateTime date, int offset, CalendarZoomMode mode, bool adjustForStart)
		{
			DateTime? dateValue = TryAddGroupOffset(date, offset, mode, adjustForStart);

			if (null == dateValue)
			{
				dateValue = GetMinMax(Calendar, offset < 0).Date;

				if (adjustForStart)
					dateValue = this.GetGroupStartDate(dateValue.Value, mode);
			}

			return dateValue.Value;
		}
		#endregion //AddGroupOffset

		#region AddItemOffset
		internal DateTime? TryAddItemOffset(DateTime date, int offset, CalendarZoomMode mode)
		{
			try
			{
				switch (mode)
				{
					default:
					case CalendarZoomMode.Days:
						date = this.Calendar.AddDays(date, offset);
						break;
					case CalendarZoomMode.Months:
						date = this.Calendar.AddMonths(date, offset);
						break;
					case CalendarZoomMode.Years:
					case CalendarZoomMode.Decades:
					case CalendarZoomMode.Centuries:
						
						int adjustment = (int)Math.Pow(10, mode - CalendarZoomMode.Years);
						date = this.Calendar.AddYears(date, offset * adjustment);
						break;
				}
			}
			catch (ArgumentException)
			{
				return null;
			}

			return date;
		}

		internal DateTime AddItemOffset(DateTime date, int offset, CalendarZoomMode mode)
		{
			DateTime? dateValue = TryAddItemOffset(date, offset, mode);

			if (null == dateValue)
			{
				if (offset < 0)
					dateValue = this.Calendar.MinSupportedDateTime.Date;
				else
					dateValue = this.Calendar.MaxSupportedDateTime.Date;
			}

			return dateValue.Value;
		}
		#endregion //AddItemOffset

		#region FindVisibleDate
		private static DateTime FindVisibleDate(DateTime date, DayOfWeekFlags hiddenDays, Calendar calendar, int offset)
		{
			try
			{
				while (IsSet(hiddenDays, calendar.GetDayOfWeek(date)))
				{
					date = calendar.AddDays(date, offset);
				}
			}
			catch (ArgumentException)
			{
				if (offset > 0)
					date = calendar.MaxSupportedDateTime;
				else
					date = calendar.MinSupportedDateTime;
			}

			return date;
		}
		#endregion //FindVisibleDate

		#region GetDateCount
		private int GetDateCount(CalendarZoomMode mode, bool includeLeadingDates, bool includeTrailingDates, ref DateTime firstDate)
		{
			DateTime date = firstDate;

			Calendar cal = this.Calendar;

			// to simplify matters we're going to deal with unfiltered dates - i.e. we will
			// ignore the hidden days of week until we are iterating the dates
			DateTime groupStart = GetGroupStartDate(date, mode, cal, DayOfWeekFlags.None);
			DateTime groupEnd = GetGroupEndDate(date, mode, cal, DayOfWeekFlags.None);
			int dateCount;

			#region Base DateCount
			switch (mode)
			{
				case CalendarZoomMode.Days:
					dateCount = this.GetDaysInMonth(date);
					break;
				case CalendarZoomMode.Months:
					{
						int year = cal.GetYear(date);
						int era = cal.GetEra(date);
						dateCount = cal.GetMonthsInYear(year, era);
						break;
					}
				default:
				case CalendarZoomMode.Years:
				case CalendarZoomMode.Decades:
				case CalendarZoomMode.Centuries:
					
					{
						
						Debug.Assert(mode <= CalendarZoomMode.Centuries, "Unrecognized mode!");
						// see how many years between first item and last item date
						double adjustment = Math.Pow(10, mode - CalendarZoomMode.Years);
						int lastDecade = (int)Math.Floor(cal.GetYear(groupEnd) / adjustment);
						int firstDecade = (int)Math.Floor(cal.GetYear(groupStart) / adjustment);
						dateCount = 1 + lastDecade - firstDecade;
						break;
					}
			}
			#endregion //Base DateCount

			#region Leading Dates
			if (includeLeadingDates)
			{
				switch (mode)
				{
					case CalendarZoomMode.Days:
						{
							// start by taking the number of days between the first date
							// of the month and the beginning of the week
							int dateOffset = this.GetDayOfWeekNumber(groupStart, false);

							// we may want a leading week of trailing days if there are 5 or less
							// weeks of dates. for this calculation we will ignore the hidden days
							if (dateOffset == 0 && Math.Ceiling(this.GetDaysInMonth(date) / 7.0) <= 5)
								dateOffset += 7;

							// calculate the new first day
							firstDate = this.AddDays(groupStart, -dateOffset).Date;

							TimeSpan diff = groupStart.Subtract(firstDate);
							dateCount += diff.Days;
							break;
						}
					case CalendarZoomMode.Months:
						{
							break;
						}
					case CalendarZoomMode.Years:
					case CalendarZoomMode.Decades:
					case CalendarZoomMode.Centuries:
						
						{
							int adjustment = -(int)Math.Pow(10, mode - CalendarZoomMode.Years);

							firstDate = this.AddYears(groupStart, adjustment).Date;

							if (firstDate < date)
								dateCount++;
							break;
						}
				}
			}
			#endregion //Leading Dates

			#region Trailing Dates
			if (includeTrailingDates)
			{
				switch (mode)
				{
					case CalendarZoomMode.Days:
						{
							int originalDateCount = dateCount;

							// add days for the trailing days to fill in the last week
							dateCount += 6 - this.GetDayOfWeekNumber(groupEnd, false);

							// then fill in up to 6 weeks unless we didn't add any trailing
							// dates or if the first day of the month is not the first day 
							// of the week
							if (originalDateCount == 35 || this.GetDayOfWeekNumber(groupStart, false) != 0)
							{
								int weekCount = (int)Math.Ceiling(dateCount / 7.0);
								dateCount += 7 * (6 - weekCount);
							}
							break;
						}
					case CalendarZoomMode.Months:
						{
							break;
						}
					default:
						{
							dateCount++;
							break;
						}
				}
			}

			#endregion //Trailing Dates

			// AS 10/22/08 TFS9441
			// We calculated the datecount using the number of days based on the 
			// start date for the group so if we're not including leading dates
			// we need to remove some days based on the difference between our 
			// calculated start and the actual start. We can't do this earlier
			// because we could be including trailing dates only which assumes
			// that all days are considered.
			// 
			if (false == includeLeadingDates &&
				mode == CalendarZoomMode.Days &&
				groupStart < firstDate)
			{
				dateCount -= firstDate.Subtract(groupStart).Days;
			}

			return dateCount;
		}
		#endregion //GetDateCount

		#region GetDayOfWeekNumber

		/// <summary>
		/// Returns the number of the day of the week based on the first day of week.
		/// </summary>
		/// <param name="date">The date whose offset is to be calculated</param>
		/// <returns>A number between 0 and 6 that represents how many days into the week, the specified day represents based on the current first day of week.</returns>
		public int GetDayOfWeekNumber(DateTime date)
		{
			return GetDayOfWeekNumber(date, DayOfWeekFlags.None, _firstDayOfWeek);
		}

		/// <summary>
		/// Returns the number of the day of the week based on the first day of week.
		/// </summary>
		/// <param name="date">The date whose offset is to be calculated</param>
		/// <param name="honorDisabledDays">True to account for the hidden days of the week</param>
		/// <returns>A number between 0 and 6 that represents how many days into the week, the specified day represents based on the current first day of week.</returns>
		public int GetDayOfWeekNumber(DateTime date, bool honorDisabledDays)
		{
			DayOfWeekFlags hiddenDays = honorDisabledDays ? this._hiddenDays : DayOfWeekFlags.None;

			// JJD 3/30/11 - TFS66883
			// Pas in the resolved first day of week
			//return this.GetDayOfWeekNumber(date, hiddenDays, null);
			return this.GetDayOfWeekNumber(date, hiddenDays, this.FirstDayOfWeekResolved);
		}
		#endregion // GetDayOfWeekNumber

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

		#region GetItemColumnCount
		/// <summary>
		/// Returns the number of item columns regardless of the hidden days of week.
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		internal static int GetItemColumnCount(CalendarZoomMode mode)
		{
			switch (mode)
			{
				default:
				case CalendarZoomMode.Days:
					return 7;
				case CalendarZoomMode.Months:
					return 4;
				case CalendarZoomMode.Years:
				case CalendarZoomMode.Decades:
				case CalendarZoomMode.Centuries:
					
					return 4;
			}
		}
		#endregion //GetItemColumnCount

		#region GetItemDates
		internal DateTime[] GetItemDates(DateTime date, CalendarZoomMode mode,
			bool includeLeadingDates, bool includeTrailingDates,
			DateTime minDate, DateTime maxDate)
		{
			// first get the first date of the mode
			DateTime firstDate = date;

			Calendar cal = this.Calendar;

			int dateCount = GetDateCount(mode, includeLeadingDates, includeTrailingDates, ref firstDate);

			#region Calculate Dates
			DateTime[] dates = new DateTime[dateCount];
			DateTime currentDate = firstDate;
			DayOfWeekFlags daysToSkip = mode == CalendarZoomMode.Days ? this._hiddenDays : 0;
			int datesAdded = 0;

			for (int i = 0; i < dateCount; i++)
			{
				if (daysToSkip == 0 || false == IsSet(daysToSkip, cal.GetDayOfWeek(currentDate)))
				{
					DateTime currentEndDate = this.GetItemEndDate(currentDate, mode);

					// if the item intersects with the min/max...
					if (false == (currentEndDate < minDate || currentDate > maxDate))
					{
						// store the starting date
						dates[datesAdded] = currentDate;
						datesAdded++;
					}
				}

				// offset the date based on the mode
				currentDate = this.AddItemOffset(currentDate, 1, mode);

				// make sure we have the start date for that mode
				currentDate = this.GetItemStartDate(currentDate, mode);

				// if its the same as the current end date then we're at the 
				// end of what the calendar can support
				if (datesAdded == 0 && currentDate <= this.GetItemEndDate(firstDate, mode))
					break;
				else if (datesAdded > 0 && currentDate <= this.GetItemEndDate(dates[datesAdded - 1], mode))
					break;
			}
			#endregion //Calculate Dates

			if (dates.Length > datesAdded)
				Array.Resize<DateTime>(ref dates, datesAdded);

			return dates;
		}
		#endregion //GetItemDates

		#region GetItemRowColumn
		internal void GetItemRowColumn(DateTime date, CalendarZoomMode mode,
			DateTime groupStartDate, bool showsLeadingDates,
			out int colOffset, out int rowOffset)
		{
			switch (mode)
			{
				case CalendarZoomMode.Days:
					{
						rowOffset = 0;
						// AS 2/19/09 TFS11497
						// We need to call the overloads that ignore the hidden days since we're
						// calculating based on the unfiltered days to remain consistent.
						//
						//colOffset = this.GetDayOfWeekNumber(date);
						//DateTime firstDate = this.GetGroupStartDate(groupStartDate, mode);
						//int firstDateOffset = this.GetDayOfWeekNumber(firstDate);
						// AS 2/20/09
						// The column offset should consider the hidden days.
						//
						//colOffset = this.GetDayOfWeekNumber(date, false);
						colOffset = this.GetDayOfWeekNumber(date, true);
						DateTime firstDate = GetGroupStartDate(groupStartDate, mode, Calendar, 0);
						int firstDateOffset = this.GetDayOfWeekNumber(firstDate, false);

						// if we're not showing leading days and our first item will be
						// in the first column then we want to shift the items down by one row
						if (firstDateOffset == 0
							&& (showsLeadingDates == false || date >= firstDate)
							&& this.GetDaysInMonth(firstDate) < 35)
							rowOffset = 1;

						// if we're starting off with a date past the first day of the month then
						// we need to figure out what row that date would have started on
						if (date > firstDate)
						{
							// find out which day the first date is on
							int dayOfMonth = this.GetDayOfMonth(date) - 1;

							// add any leading day spaces before first date
							dayOfMonth += firstDateOffset;

							// we're comparing non-filtered dates
							rowOffset += dayOfMonth / 7;
						}
						break;
					}
				case CalendarZoomMode.Months:
					{
						int index = (this.Calendar.GetMonth(date) - 1);
						colOffset = index % 4;
						rowOffset = index / 4;
						break;
					}
				default:
				case CalendarZoomMode.Years:
				case CalendarZoomMode.Decades:
				case CalendarZoomMode.Centuries:
					
					{
						Debug.Assert(mode <= CalendarZoomMode.Centuries);

						
						
						
						
						
						//decimal adjustment = (decimal)Math.Pow(10, mode - CalendarZoomMode.Years);
						double adjustment = Math.Pow(10, mode - CalendarZoomMode.Years);
						int index = (((int)Math.Floor(this.Calendar.GetYear(date) / adjustment) + 1) % 10);
						colOffset = index % 4;
						rowOffset = index / 4;
						break;
					}
			}
		}

		#endregion //GetItemRowColumn

		#region GetDayOfWeekCaption

		internal string GetDayOfWeekCaption(DayOfWeekHeaderFormat format, DayOfWeek day)
		{
			int index = (int)day;
			string caption;
			DateTimeFormatInfo formatInfo = this.DateTimeFormat;

			if (format == DayOfWeekHeaderFormat.Abbreviated)
				caption = formatInfo.AbbreviatedDayNames[index];
			else
			{
				// start with the full name
				caption = formatInfo.DayNames[index];

				switch (format)
				{
					case DayOfWeekHeaderFormat.SingleCharacter:
						caption = GetShortDayOfWeekCaption(caption, 1, formatInfo);
						break;
					case DayOfWeekHeaderFormat.TwoCharacters:
						caption = GetShortDayOfWeekCaption(caption, 2, formatInfo);
						break;
				}
			}

			return caption;
		}
		#endregion //GetDayOfWeekCaption

		// AS 3/5/04 UWS780/816
		#region GetDayOfWeekCaption (DayOfWeek,int,int)
		private static string GetDayOfWeekCaption(string dayOfWeek, int index, int length)
		{
			// get the characters - need to do this in case 2 characters together
			// form a single character
			int[] chars = StringInfo.ParseCombiningCharacters(dayOfWeek);

			// get the number of logical characters
			int len = chars.Length;

			// if the index is beyond what we need, return null
			if (index >= chars.Length)
				return null;
			else if (len <= length)
				return dayOfWeek;
			else if (len == index + length)
				return dayOfWeek.Substring(chars[index]);
			else
				return dayOfWeek.Substring(chars[index], chars[index + length] - chars[index]);
		}
		#endregion //GetDayOfWeekCaption (DayOfWeek,int,int)

		#region GetDayOfWeekCaptions

		internal string[] GetDayOfWeekCaptions(DayOfWeekHeaderFormat format)
		{
			string[] daysOfWeek;

			switch (format)
			{
				default:
				case DayOfWeekHeaderFormat.Abbreviated:
					daysOfWeek = this.DateTimeFormat.AbbreviatedDayNames;
					break;
				case DayOfWeekHeaderFormat.Full:
					daysOfWeek = this.DateTimeFormat.DayNames;
					break;
				case DayOfWeekHeaderFormat.SingleCharacter:
					daysOfWeek = GetShortDaysOfWeek(1);
					break;
				case DayOfWeekHeaderFormat.TwoCharacters:
					daysOfWeek = GetShortDaysOfWeek(2);
					break;
			}

			int offset = (int)this.FirstDayOfWeekResolved;

			if (offset > 0)
			{
				string[] tempDays = new string[offset];
				Array.Copy(daysOfWeek, 0, tempDays, 0, offset);
				Array.Copy(daysOfWeek, offset, daysOfWeek, 0, 7 - offset);
				Array.Copy(tempDays, 0, daysOfWeek, 7 - offset, offset);
			}

			return daysOfWeek;
		}
		#endregion //GetDayOfWeekCaptions

		#region GetDaysOfWeek

		internal DayOfWeek[] GetDaysOfWeek()
		{
			int offset = (int)this.FirstDayOfWeekResolved;
			List<DayOfWeek> days = new List<DayOfWeek>(this.VisibleDayCount);

			for (int i = 0; i < 7; i++)
			{
				DayOfWeek day = (DayOfWeek)((i + offset) % 7);
				if (false == IsSet(this.HiddenDays, day))
					days.Add(day);
			}

			return days.ToArray();
		}
		#endregion //GetDaysOfWeek

		#region GetEndDate

		internal DateTime GetGroupEndDate(DateTime date, CalendarZoomMode mode)
		{
			return GetGroupEndDate(date, mode, this.Calendar, this._hiddenDays);
		}

		internal static DateTime GetGroupEndDate(DateTime date, CalendarZoomMode mode, Calendar calendar, DayOfWeekFlags hiddenDays)
		{
			Debug.Assert(calendar.MinSupportedDateTime <= date);

			int month = calendar.GetMonth(date);
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);
			int days = calendar.GetDaysInMonth(year, month, era);
			DateTime endDate;

			try
			{
				switch (mode)
				{
					case CalendarZoomMode.Days:		// last day of month containing the date
						break;

					case CalendarZoomMode.Months:	// last day of the last month of year containing the date
						month = calendar.GetMonthsInYear(year, era);
						days = calendar.GetDaysInMonth(year, month, era);
						break;
					case CalendarZoomMode.Years:	// last year of the decade containing the date
					case CalendarZoomMode.Decades:	// first decade of the century containing the date
					case CalendarZoomMode.Centuries:
						
						int adjustment = (int)Math.Pow(10, 1 + (mode - CalendarZoomMode.Years));
						year += (adjustment - 1) - (year % adjustment);
						month = calendar.GetMonthsInYear(year, era);
						days = calendar.GetDaysInMonth(year, month, era);
						break;
				}

				// get the first day of the month
				endDate = calendar.ToDateTime(year, month, days, 0, 0, 0, 0, era);
			}
			catch (ArgumentException)
			{
				endDate = calendar.MaxSupportedDateTime;
			}

			// AS 10/3/08 TFS8633
			if (hiddenDays == CalendarManager.AllDays)
				return endDate;

			return FindVisibleDate(endDate, hiddenDays, calendar, -1);
		}

		internal DateTime GetItemEndDate(DateTime date, CalendarZoomMode mode)
		{
			return GetItemEndDate(date, mode, this.Calendar);
		}

		internal static DateTime GetItemEndDate(DateTime date, CalendarZoomMode mode, Calendar calendar)
		{
			int month = calendar.GetMonth(date);
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);
			int day = calendar.GetDaysInMonth(year, month, era);

			try
			{
				switch (mode)
				{
					case CalendarZoomMode.Days:		// same day
						day = calendar.GetDayOfMonth(date);
						break;

					case CalendarZoomMode.Months:	// last day of the month
						break;
					case CalendarZoomMode.Years:	// last day of the year
						month = calendar.GetMonthsInYear(year, era);
						day = calendar.GetDaysInMonth(year, month, era);
						break;
					case CalendarZoomMode.Decades:	// last day of the decade
					case CalendarZoomMode.Centuries:
						
						int adjustment = (int)Math.Pow(10, 1 + (mode - CalendarZoomMode.Decades));
						year += (adjustment - 1) - (year % adjustment);
						month = calendar.GetMonthsInYear(year, era);
						day = calendar.GetDaysInMonth(year, month, era);
						break;
				}

				// get the first day of the month
				return calendar.ToDateTime(year, month, day, 0, 0, 0, 0, era);
			}
			catch (ArgumentException)
			{
				return calendar.MaxSupportedDateTime;
			}
		}
		#endregion //GetEndDate

		#region GetShortDaysOfWeek

		private string[] GetShortDaysOfWeek(int charCount)
		{
			string[] fullNames = this.DateTimeFormat.DayNames;
			string[] shortNames = new string[7];

			for (int i = 0; i < 7; i++)
				shortNames[i] = GetShortDayOfWeekCaption(fullNames[i], charCount, this.DateTimeFormat);

			// iterate all the days to get a unique short string for each
			return shortNames;
		}
		#endregion //GetShortDaysOfWeek

		#region GetShortDayOfWeekCaption
		internal static string GetShortDayOfWeekCaption(string dayOfWeek, int preferredChars, DateTimeFormatInfo dateTimeFormat)
		{
			string text = null;
			int index = 0;
			int length = preferredChars;

			do
			{
				text = GetDayOfWeekCaption(dayOfWeek, index, length);

				if (InUniqueDayOfWeekCaption(index, dayOfWeek, text, length, dateTimeFormat))
					break;

				index++;
			}
			while (text != null);

			// if the text isn't long enough to find a unique character,
			// then return the first one or two depending on the style
			if (text == null)
				text = GetDayOfWeekCaption(dayOfWeek, index, length);

			return text;
		}
		#endregion GetDayOfWeekCaption

		#region GetStartDate

		internal DateTime GetGroupStartDate(DateTime date, CalendarZoomMode mode)
		{
			return GetGroupStartDate(date, mode, this.Calendar, this._hiddenDays);
		}

		internal static DateTime GetGroupStartDate(DateTime date, CalendarZoomMode mode, Calendar calendar, DayOfWeekFlags hiddenDays)
		{
			int month = 1;
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);

			switch (mode)
			{
				case CalendarZoomMode.Days:		// first day of month containing the date
					month = calendar.GetMonth(date);
					break;

				case CalendarZoomMode.Months:	// first day of the first month of year containing the date
					break;

				case CalendarZoomMode.Years:	// first day of the first month of the first year of the decade containing the date
				case CalendarZoomMode.Decades:	// first day of the first month of the first year of a century containing the date
				case CalendarZoomMode.Centuries:
					
					int adjustment = (int)Math.Pow(10, 1 + (mode - CalendarZoomMode.Years));
					year -= year % adjustment;

					if (year < 1)
						year = 1;
					break;
			}

			DateTime startDate;

			try
			{
				// get the first day of the month
				startDate = calendar.ToDateTime(year, month, 1, 0, 0, 0, 0, era);
			}
			catch (ArgumentException)
			{
				startDate = calendar.MinSupportedDateTime;
			}

			// AS 10/3/08 TFS8633
			if (hiddenDays == CalendarManager.AllDays)
				return startDate;

			return FindVisibleDate(startDate, hiddenDays, calendar, 1);
		}

		internal DateTime GetItemStartDate(DateTime date, CalendarZoomMode mode)
		{
			return GetItemStartDate(date, mode, this.Calendar);
		}

		internal static DateTime GetItemStartDate(DateTime date, CalendarZoomMode mode, Calendar calendar)
		{
			int month = 1;
			int year = calendar.GetYear(date);
			int era = calendar.GetEra(date);

			switch (mode)
			{
				case CalendarZoomMode.Days:		// the date itself
					return date;

				case CalendarZoomMode.Months:	// the first day of the month
					month = calendar.GetMonth(date);
					break;

				case CalendarZoomMode.Years:	// first day of the year
				case CalendarZoomMode.Decades:	// first year of the decade
				case CalendarZoomMode.Centuries:
					
					int adjustment = (int)Math.Pow(10, mode - CalendarZoomMode.Years);
					year -= year % adjustment;
					break;
			}

			try
			{
				// get the first day of the month
				return calendar.ToDateTime(year, month, 1, 0, 0, 0, 0, era);
			}
			catch (ArgumentException)
			{
				return calendar.MinSupportedDateTime;
			}
		}

		#endregion //GetStartDate

		#region GetWeekNumberForDate
		/// <summary>
		/// Returns the week number for a particular date using the current CalendarWeekRule and FirstDayOfWeek.
		/// </summary>
		/// <param name="date">DateTime</param>
		/// <returns>Integer denoting the week number that the date belongs to.</returns>
		/// <remarks>
		/// <p class="body">The System.Globalization.Calendar's GetWeekOfYear method simply returns the number of weeks into the year that the date falls on. It does not seem to use the weekrule specified in the arguments. e.g. If the first day of week is sunday and the weekrule is Jan 1, then 12/31/2000 (Sunday) should return 1. Instead it returns 54 and 1/1/2001 (Monday) returns 1.</p>
		/// <p class="body">This routine returns the correct week number for the specified date based on the CalendarWeekRule and FirstDayOfWeek.</p>
		/// <p class="note"> Note, this may be a week for a year other than the year of the date. e.g. 12/31/2000, week tt of Jan 1, and week start of sunday will return 1 because this date falls into the first week of the year 2001.</p>
		/// </remarks>
		public int GetWeekNumberForDate(DateTime date)
		{
			int year;
			return this.GetWeekNumberForDate(date, _weekRule, _firstDayOfWeek, out year);
		} 
		#endregion //GetWeekNumberForDate

		#region InitializeCalendarInfo

		internal void InitializeCalendarInfo(Calendar calendar, DateTimeFormatInfo dateTimeFormat)
		{
			this.VerifyCanChange();

			if (calendar == null)
				calendar = CultureInfo.CurrentCulture.Calendar;

			if (dateTimeFormat == null)
				dateTimeFormat = CultureInfo.CurrentCulture.DateTimeFormat;

			this.Initialize(calendar, dateTimeFormat);
		}
		#endregion //InitializeCalendarInfo

		// AS 3/5/04 UWS780/816
		#region InUniqueDayOfWeekCaption
		private static bool InUniqueDayOfWeekCaption(int index, string dayOfWeek, string text, int length, DateTimeFormatInfo dateTimeFormat)
		{
			foreach (string dow in dateTimeFormat.DayNames)
			{
				if (dow == dayOfWeek)
					continue;

				if (text != GetDayOfWeekCaption(dow, index, length))
					return true;
			}

			return false;
		}
		#endregion //InUniqueDayOfWeekCaption

		#region VerifyCanChange
		protected override void VerifyCanChange()
		{
			if (this == CalendarManager.currentCulture)
				throw new InvalidOperationException("The shared current culture CalendarManager cannot be changed.");
		}
		#endregion //VerifyCanChange

		#endregion //Methods
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