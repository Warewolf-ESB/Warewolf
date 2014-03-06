using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Diagnostics;
using System.Linq;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Abstract base class used for formatting and manipulating dates
	/// </summary>
	public class DateInfoProvider
	{
		#region Private Members

		private string _amDesignatorLowercase;
		private string _dateSeparator;
		private string _pmDesignatorLowercase;
		private string _timeSeparator;
		private string _hourPattern;
		private bool? _displayTimeIn24HourFormat;
		private string _shortMonthDayPattern;
		private string _shortestMonthDayPattern;
		private string _fullMonthDayHeaderPattern;

		// AS 3/30/11 NA 2011.1 - Date Parsing
		private string _shortWeekHeaderInterMonth;
		private string _shortWeekHeaderIntraMonth;
		private string _weekHeaderInterMonth;
		private string _weekHeaderIntraMonth;
		private string _calendarRangeInterYear;
		private string _calendarRangeInterMonth;
		private string _calendarRangeIntraMonth;
		private string _calendarRangeSingleDate;
		private string _calendarMonthRangeInterYear;
		private string _calendarMonthRangeIntraYear;
		private string _calendarMonthRangeSingleDate;

		private CultureInfo _sourceCulture;
		private Calendar _calendar;
		private DateTimeFormatInfo _dateTimeFormat;
		private CalendarHelper _calendarHelper;

		[ThreadStatic]
		private static DateInfoProvider s_currentProvider;

		// AS 3/30/11 NA 2011.1 - Date Parsing
		private static readonly string[] _culturesToSkipCombining;

		#endregion //Private Members	

		#region Constructor
		static DateInfoProvider()
		{
			// AS 3/30/11 NA 2011.1 - Date Parsing
			_culturesToSkipCombining = new string[] {
				"zh", // china
				"ja", // japan
				"ko", // korean
			};
		}

		/// <summary>
		/// Initializes a new <see cref="DateInfoProvider"/>
		/// </summary>
		/// <param name="calendar">The calendar used for date calculations</param>
		/// <param name="dateTimeFormat">Provides information about the format settings</param>
		public DateInfoProvider(Calendar calendar, DateTimeFormatInfo dateTimeFormat)
		{
			this.Initialize(calendar, dateTimeFormat);
		} 

		/// <summary>
		/// Creates a new instance of <see cref="DateInfoProvider"/> using the Calendar and DateTimeFormat of the specified culture.
		/// </summary>
		/// <param name="cultureInfo">The culture to use</param>
		public DateInfoProvider(CultureInfo cultureInfo) : this(cultureInfo, false)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="DateInfoProvider"/>
		/// </summary>
		/// <param name="cultureInfo">The culture to use</param>
		/// <param name="restrictToGregorian">If true will force the use of a Gregorian calendar</param>
		public DateInfoProvider(CultureInfo cultureInfo, bool restrictToGregorian)
		{
			CoreUtilities.ValidateNotNull(cultureInfo, "cultureInfo");

			_sourceCulture = cultureInfo;

			Calendar calendar = cultureInfo.Calendar;
			DateTimeFormatInfo dateTimeFormat = cultureInfo.DateTimeFormat;

			if (restrictToGregorian && !(calendar is GregorianCalendar))
			{
				calendar = null;

				foreach (Calendar cal in cultureInfo.OptionalCalendars)
				{
					calendar = cal as GregorianCalendar;

					if (calendar != null)
						break;
				}

				string cultureName;

				if (this._calendar == null)
				{
					_calendar = new GregorianCalendar();
					cultureName = CultureInfo.InvariantCulture.Name;
				}
				else
					cultureName = cultureInfo.Name;


				dateTimeFormat = new CultureInfo(cultureName).DateTimeFormat;
				dateTimeFormat.Calendar = calendar;
			}

			this.Initialize(calendar, dateTimeFormat);

//#if WPF
//            Debug.WriteLine("--------------------");
//            foreach (DateTimeFormatType formatType in Enum.GetValues(typeof(DateTimeFormatType)))
//            {
//                Debug.WriteLine(formatType.ToString() + ": " + this.FormatDate(DateTime.Now, formatType));
//            }
//            Debug.WriteLine("--------------------");
//#endif
			
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


		}

		
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region AMDesignator

		/// <summary>
		/// Returns the designator string for morning hours (read-only)
		/// </summary>
		public virtual string AMDesignator 
		{
			get { return _dateTimeFormat.AMDesignator; }
		}

		#endregion //AMDesignator

		#region AMDesignatorLowercase

		/// <summary>
		/// Returns and caches the <see cref="AMDesignator"/> converted to lowercase (read-only)
		/// </summary>
		public string AMDesignatorLowercase 
		{ 
			get
			{
				if (this._amDesignatorLowercase == null)
					this._amDesignatorLowercase = this.AMDesignator.ToLowerInvariant();

				return this._amDesignatorLowercase;
			}
		}

		#endregion //AMDesignatorLowercase

		#region Calendar

		/// <summary>
		/// Returns the calendar (read-only)
		/// </summary>
		public Calendar Calendar { get { return _calendar; } }

		#endregion //Calendar

		#region CalendarHelper
		internal CalendarHelper CalendarHelper
		{
			get { return _calendarHelper; }
		} 
		#endregion // CalendarHelper

		#region CurrentProvider

		/// <summary>
		/// Returns the provider for the <see cref="CultureInfo.CurrentCulture"/> (read-only)
		/// </summary>
		public static DateInfoProvider CurrentProvider
		{
			get
			{
				CultureInfo currentCulture = CultureInfo.CurrentCulture;

				if (s_currentProvider == null || s_currentProvider._sourceCulture != currentCulture)
					s_currentProvider = new DateInfoProvider(currentCulture);

				return s_currentProvider;
			}
		}

		#endregion //CurrentProvider	

		#region DateSeparator

		/// <summary>
		/// Returns the separator between year, month and day (read-only)
		/// </summary>
		public virtual string DateSeparator 
		{ 
			get
			{
				if (this._dateSeparator == null)
				{
					this._dateSeparator = DateTime.Now.ToString("%/", _dateTimeFormat);
				}

				return this._dateSeparator;
			}

		}

		#endregion //DateSeparator

		#region DateTimeFormatInfo

		/// <summary>
		/// Returns the object that contains the format info for dates and times (read-only)
		/// </summary>
		public DateTimeFormatInfo DateTimeFormatInfo { get { return _dateTimeFormat; } }

		#endregion //DateTimeFormatInfo

		#region DisplayTimeIn24HourFormat

		/// <summary>
		/// Returns true if time should be displayed in 24 hour format or false to dispay in 12 hour format with AM and PM designators
		/// </summary>
		/// <value>True wiill format the hour is 24 hour format, e.g. 3:00 PM would be displayed as 15:00. By default this is determined by looking for a uppercase 'H' in the <see cref="HourPattern"/>.</value>
		/// <seealso cref="HourPattern"/>
		public virtual bool DisplayTimeIn24HourFormat
		{
			get
			{
				if (!this._displayTimeIn24HourFormat.HasValue)
				{
					string hourPattern = this.HourPattern;

					// default to true
					this._displayTimeIn24HourFormat = true;

					foreach (char chr in hourPattern)
					{
						if (chr == 'h' )
						{
							this._displayTimeIn24HourFormat = false;
							break;
						}
						if (chr == 'H')
						{
							this._displayTimeIn24HourFormat = true;
							break;
						}
					}
				}

				return _displayTimeIn24HourFormat.Value;
			}
		}

		#endregion //DisplayTimeIn24HourFormat	

		#region HourPattern

		/// <summary>
		/// Returns the pattern to use to display just hours
		/// </summary>
		/// <value>The pattern used to format the hour portion of a time. By default this is created by parsing the LongTimePattern off <see cref="DateTimeFormatInfo"/>.</value>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this pattern is used to determine whether dates are displayed in 12 or 24 hour format based on whether the pattern has an 'h' or 'H' character in it respectively.</para>
		/// </remarks>
		/// <seealso cref="DisplayTimeIn24HourFormat"/>
		public virtual string HourPattern
		{
			get
			{
				if (this._hourPattern == null)
				{
					string timeFormat = _dateTimeFormat.LongTimePattern;

					List<char> characters = new List<char>(2);
					foreach (char chr in timeFormat)
					{
						if (chr == 'h' || chr == 'H')
						{
							characters.Add(chr);
						}
						else
						{
							if (characters.Count > 0)
								break;
						}
					}

					Debug.Assert(characters.Count > 0, "No 'h' or 'H' in long time pattern");

					if (characters.Count == 0)
						characters.Add('H');

					// if the pattern is a single character we can't use it as is because the dateTime formatting logic
					// special cases a single character pattern. so we need to prefix it with a '%' character in that case.
					if (characters.Count == 1)
						characters.Insert(0, '%');


					this._hourPattern = new string(characters.ToArray());
				}

				return this._hourPattern;
			}
		}

		#endregion //HourPattern	
	
		#region MaxSupportedDateTime

		/// <summary>
		/// Returns the latest date time that is supported (read-only)
		/// </summary>
		public DateTime MaxSupportedDateTime { get { return _calendar.MaxSupportedDateTime; } }

		#endregion //MaxSupportedDateTime	
			
		#region MinSupportedDateTime

		/// <summary>
		/// Returns the earliest date time that is supported  (read-only)
		/// </summary>
		public DateTime MinSupportedDateTime { get { return _calendar.MinSupportedDateTime; } }

		#endregion //MinSupportedDateTime	

		#region PMDesignator

		/// <summary>
		/// Returns the designator string for afternoon hours (read-only)
		/// </summary>
		public virtual string PMDesignator
		{
			get { return _dateTimeFormat.PMDesignator; }
		}

		#endregion //PMDesignator

		#region PMDesignatorLowercase

		/// <summary>
		/// Returns and caches the <see cref="PMDesignator"/> converted to lowercase (read-only)
		/// </summary>
		public string PMDesignatorLowercase 
		{ 
			get
			{
				if (this._pmDesignatorLowercase == null)
					this._pmDesignatorLowercase = this.PMDesignator.ToLowerInvariant();

				return this._pmDesignatorLowercase;
			}
		}

		#endregion //PMDesignatorLowercase

		#region TimeSeparator

		/// <summary>
		/// Returns the separator between hours, minutes and seconds (read-only)
		/// </summary>
		public virtual string TimeSeparator 
		{ 
			get
			{
				if (this._timeSeparator == null)
				{
					this._timeSeparator = DateTime.Now.ToString("%:", _dateTimeFormat);
				}

				return this._timeSeparator;
			}

		}

		#endregion //TimeSeparator
	
		#endregion //Public Properties	
	
		#region Internal Properties

		#region ShortMonthDayPattern
		internal string ShortMonthDayPattern
		{
			get
			{
				if (null == _shortMonthDayPattern)
					_shortMonthDayPattern = _dateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM");

				return _shortMonthDayPattern;
			}
		}
		#endregion // ShortMonthDayPattern

		#endregion // Internal Properties

		#endregion //Properties	
	
		#region Methods

		#region Public Methods

		#region ClearCachedValues

		/// <summary>
		/// Clears any values that have been cached so they can be re-created when needed
		/// </summary>
		public virtual void ClearCachedValues()
		{
			this._dateSeparator = null;
			this._displayTimeIn24HourFormat = null;
			this._hourPattern = null;
			this._timeSeparator = null;
			this._amDesignatorLowercase = null;
			this._pmDesignatorLowercase = null;
			this._shortestMonthDayPattern = null;
			this._shortMonthDayPattern = null;
			this._fullMonthDayHeaderPattern = null;

			// AS 3/30/11 NA 2011.1 - Date Parsing
			_shortWeekHeaderIntraMonth = null;
			_shortWeekHeaderInterMonth = null;
			_weekHeaderInterMonth = null;
			_weekHeaderIntraMonth = null;
			_calendarRangeInterMonth = null;
			_calendarRangeInterYear = null;
			_calendarRangeIntraMonth = null;
			_calendarRangeSingleDate = null;
			_calendarMonthRangeInterYear = null;
			_calendarMonthRangeIntraYear = null;
			_calendarMonthRangeSingleDate = null;
		}

		#endregion //ClearCachedValues

		#region FormatDate

		/// <summary>
		/// Returns a formatted string for the supplied date.
		/// </summary>
		/// <param name="dateTime">The date/time to format.</param>
		/// <param name="formatType">An enumaration that specifies how to format the date time.</param>
		/// <returns>A formatted string.</returns>
		/// <seealso cref="DateTimeFormatType"/>
		public virtual string FormatDate(DateTime dateTime, DateTimeFormatType formatType)
		{
			DateTimeFormatInfo formatInfo = this.DateTimeFormatInfo;

			string format = null;
			switch (formatType)
			{
				case DateTimeFormatType.None:
					return string.Empty;

				case DateTimeFormatType.DayOfMonthNumber:
					format = "%d";
					break;
				case DateTimeFormatType.FullDateTime:
					format = formatInfo.FullDateTimePattern;
					break;
				case DateTimeFormatType.Hour:
					format = this.HourPattern;
					break;
				case DateTimeFormatType.LongDate:
					format = formatInfo.LongDatePattern;
					break;
				case DateTimeFormatType.LongTime:
					format = formatInfo.LongTimePattern;
					break;
				case DateTimeFormatType.Minute:
					format = "mm";
					break;
				case DateTimeFormatType.MonthName: // AS 12/6/10 NA 11.1 - XamOutlookCalendarView
					return formatInfo.MonthNames[this.Calendar.GetMonth(dateTime) - 1];
				case DateTimeFormatType.MonthDay:
					format = formatInfo.MonthDayPattern;
					break;
				case DateTimeFormatType.MonthOfYearNumber:
					format = "%M";
					break;
				case DateTimeFormatType.ShortDate:
					format = formatInfo.ShortDatePattern;
					break;
				case DateTimeFormatType.ShortTime:
					format = formatInfo.ShortTimePattern;
					break;
				case DateTimeFormatType.ShortDayOfWeek:
				case DateTimeFormatType.DayOfWeek:
				case DateTimeFormatType.ShortestDayOfWeek:
					return FormatDayOfWeek(formatType, dateTime.DayOfWeek);
				case DateTimeFormatType.ShortMonthDay:
					format = this.ShortMonthDayPattern;
					break;
				case DateTimeFormatType.ShortestMonthDay:
					
					if (null == _shortestMonthDayPattern)
						_shortestMonthDayPattern = string.Format("M{0}d", this.DateSeparator);

					format = _shortestMonthDayPattern;
					break;

				case DateTimeFormatType.YearMonth:
					format = formatInfo.YearMonthPattern;
					break;

				case DateTimeFormatType.Year2Digit:
					format = "yy";
					break;

				case DateTimeFormatType.Year4Digit:
					format = "yyyy";
					break;

			}

			Debug.Assert(format != null, "Unknown DateTimeFormatType");
			if (format == null)
				format = formatInfo.LongDatePattern;

			return dateTime.ToString(format, formatInfo);
		}

		#endregion //FormatDate	

		#region FormatDateRange

		/// <summary>
		/// Returns a formatted string for a range of date times.
		/// </summary>
		/// <param name="start">The start date/time.</param>
		/// <param name="end">The start date/time.</param>
		/// <param name="formatType">An enumeration that specifies how to format the date range.</param>
		/// <returns>A formatted string.</returns>
		/// <seealso cref="DateTimeFormatType"/>
		public virtual string FormatDateRange(DateTime start, DateTime end, DateRangeFormatType formatType)
		{

			StringBuilder sb;

			switch (formatType)
			{
				default:
				{
					Debug.Assert(false, "Unrecognized format type:" + formatType.ToString());
					return string.Empty;
				}

				case DateRangeFormatType.None:
				{
					return string.Empty;
				}

				#region ActivityToolTip

				case DateRangeFormatType.ActivityToolTip:
				{
					sb = new StringBuilder();
					TimeSpan span = end.Subtract(start);

					if (span.TotalDays > 1)
					{
						sb.Append(this.FormatDate(start, DateTimeFormatType.ShortDayOfWeek));
						sb.Append(" ");
						sb.Append(this.FormatDate(start, DateTimeFormatType.ShortTime));
						sb.Append(" - ");
						sb.Append(this.FormatDate(end, DateTimeFormatType.ShortDayOfWeek));
						sb.Append(" ");

						if (span.TotalDays >= 7)
						{
							sb.Append(this.FormatDate(end, DateTimeFormatType.MonthDay));
							sb.Append(" ");
						}

						sb.Append(this.FormatDate(end, DateTimeFormatType.ShortTime));
					}
					else
					{
						sb.Append(this.FormatDate(start, DateTimeFormatType.ShortTime));
						sb.Append(" - ");
						if (end.Day > start.Day)
						{
							sb.Append(this.FormatDate(end, DateTimeFormatType.ShortDayOfWeek));
							sb.Append(" ");
						}
						sb.Append(this.FormatDate(end, DateTimeFormatType.ShortTime));
					}
					break;
				}

				#endregion //ActivityToolTip

				// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
				#region CalendarDateRange
				case DateRangeFormatType.CalendarDateRange:
				{
					// AS 3/30/11 NA 2011.1 - Date Parsing
					if (_calendarRangeIntraMonth == null)
					{
						DateFormatBuilder.BuildLongDateRange(this.DateTimeFormatInfo.LongDatePattern, this.ShouldCombineDateSections(), out _calendarRangeSingleDate, out _calendarRangeIntraMonth, out _calendarRangeInterMonth, out _calendarRangeInterYear);
					}

					bool sameYear = this.Calendar.GetYear(start) == this.Calendar.GetYear(end);
					sb = new StringBuilder();

					string format;

					if (start == end)
						format = _calendarRangeSingleDate;
					else if (!sameYear)
						format = _calendarRangeInterYear;
					else if (this.CalendarHelper.GetMonthNumber(start) != this.CalendarHelper.GetMonthNumber(end))
						format = _calendarRangeInterMonth;
					else
						format = _calendarRangeIntraMonth;

					sb.AppendFormat(this.DateTimeFormatInfo, format, start, end);
					break;
				} 
				#endregion // CalendarDateRange

				// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
				#region CalendarMonthRange
				case DateRangeFormatType.CalendarMonthRange:
				{
					// AS 3/30/11 NA 2011.1 - Date Parsing
					if (_calendarMonthRangeInterYear == null)
					{
						DateFormatBuilder.BuildMonthYearRange(this.DateTimeFormatInfo.LongDatePattern, this.ShouldCombineDateSections(), out _calendarMonthRangeSingleDate, out _calendarMonthRangeIntraYear, out _calendarMonthRangeInterYear);
					}

					sb = new StringBuilder();

					string format;

					if (this.CalendarHelper.IsSameMonth(start, end))
						format = _calendarMonthRangeSingleDate;
					else if (this.CalendarHelper.Calendar.GetYear(start) == this.CalendarHelper.Calendar.GetYear(end))
						format = _calendarMonthRangeIntraYear;
					else
						format = _calendarMonthRangeInterYear;

					sb.AppendFormat(this.DateTimeFormatInfo, format, start, end);
					break;
				} 
				#endregion // CalendarMonthRange

				#region EndDateOutOfView

				case DateRangeFormatType.EndDateOutOfView:
					return ScheduleUtilities.GetString("EndDateOutOfView", this.FormatDate(end, DateTimeFormatType.ShortMonthDay));

				#endregion //EndDateOutOfView

				#region EndTimeOnly

				case DateRangeFormatType.EndTimeOnly:
				{
					return this.FormatDate(end, DateTimeFormatType.ShortTime);
				}

				#endregion //EndTimeOnly

				#region MonthDayHeader
				case DateRangeFormatType.MonthDayHeader:
				{
					
					return this.FormatDate(start, DateTimeFormatType.ShortMonthDay);
				} 
				#endregion // MonthDayHeader

				#region MonthDayHeaderFull
				case DateRangeFormatType.MonthDayHeaderFull:
				{
					if (_fullMonthDayHeaderPattern == null)
					{
						
						// 'foo' MMMM dd, YYYY then they would keep the 'foo' but not if 
						// it was 'foo' dddd MMMM dd, YYYY
						string pattern = _dateTimeFormat.LongDatePattern.Replace("MMMM", "MMM");
						pattern = pattern.Replace("YYYY", "YY");
						pattern = pattern.Replace("yyyy", "yy");
						pattern = pattern.Replace("ddd", "dd");
						_fullMonthDayHeaderPattern = pattern;
					}

					return start.ToString(_fullMonthDayHeaderPattern, this.DateTimeFormatInfo);
				}
				#endregion // MonthDayHeaderFull

				#region ShortWeekHeader

				case DateRangeFormatType.ShortWeekHeader:
				{
					sb = new StringBuilder();
					// AS 3/30/11 NA 2011.1 - Date Parsing
					//sb.Append(this.FormatDate(start, DateTimeFormatType.ShortestMonthDay));
					//sb.Append("-");
					//
					//if (this.CalendarHelper.GetMonthNumber(start) != this.CalendarHelper.GetMonthNumber(end))
					//    sb.Append(this.FormatDate(end, DateTimeFormatType.ShortestMonthDay));
					//else
					//    sb.Append(this.FormatDate(end, DateTimeFormatType.DayOfMonthNumber));
					if (_shortWeekHeaderInterMonth == null)
					{
						DateFormatBuilder.BuildMonthDayRange(this.DateTimeFormatInfo.ShortDatePattern, this.ShouldCombineDateSections(), out _shortWeekHeaderIntraMonth, out _shortWeekHeaderInterMonth);
					}

					string format = this.CalendarHelper.GetMonthNumber(start) != this.CalendarHelper.GetMonthNumber(end)
						? _shortWeekHeaderInterMonth
						: _shortWeekHeaderIntraMonth;

					sb.AppendFormat(this.DateTimeFormatInfo, format, start, end);
					break;
				}

				#endregion //ShortWeekHeader

				#region StartAndEndTime

				case DateRangeFormatType.StartAndEndTime:
				{
					sb = new StringBuilder();
					sb.Append(this.FormatDate(start, DateTimeFormatType.ShortTime));
					sb.Append("   ");
					sb.Append(this.FormatDate(end, DateTimeFormatType.ShortTime));
					break;
				}
				#endregion //StartAndEndTime

				#region StartDateOutOfView

				case DateRangeFormatType.StartDateOutOfView:
					return ScheduleUtilities.GetString("StartDateOutOfView", this.FormatDate(start, DateTimeFormatType.ShortMonthDay));

				#endregion //StartDateOutOfView	
    
				#region StartTimeOnly

				case DateRangeFormatType.StartTimeOnly:
				{
					return this.FormatDate(start, DateTimeFormatType.ShortTime);
				}

				#endregion //StartTimeOnly

				#region WeekHeader

				case DateRangeFormatType.WeekHeader:
				{
					sb = new StringBuilder();
					// AS 3/30/11 NA 2011.1 - Date Parsing
					//sb.Append(this.FormatDate(start, DateTimeFormatType.ShortMonthDay));
					//sb.Append("-");
					//
					//if (this.CalendarHelper.GetMonthNumber(start) != this.CalendarHelper.GetMonthNumber(end))
					//    sb.Append(this.FormatDate(end, DateTimeFormatType.ShortMonthDay));
					//else
					//    sb.Append(this.FormatDate(end, DateTimeFormatType.DayOfMonthNumber));
					if (_shortWeekHeaderInterMonth == null)
					{
						DateFormatBuilder.BuildMonthDayRange(this.DateTimeFormatInfo.LongDatePattern, this.ShouldCombineDateSections(), out _weekHeaderIntraMonth, out _weekHeaderInterMonth);
					}

					string format = this.CalendarHelper.GetMonthNumber(start) != this.CalendarHelper.GetMonthNumber(end)
						? _weekHeaderInterMonth
						: _weekHeaderIntraMonth;

					sb.AppendFormat(this.DateTimeFormatInfo, format, start, end);
					break;
				}

				#endregion //WeekHeader
			}

			if ( sb == null )
				return string.Empty;

			return sb.ToString();
		}

		/// <summary>
		/// Returns a formatted string for a range of date times relative to a specific date.
		/// </summary>
		/// <param name="start">The start date/time.</param>
		/// <param name="end">The end date/time.</param>
		/// <param name="relativeDate">The relative date context</param>
		/// <param name="includeStart">If true will include the start time in the formatted string.</param>
		/// <param name="includeEnd">If true will include the end time in the formatted string.</param>
		/// <returns>A formatted string.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> If the start and end times are on the same day as the RelativeDate then only the time portion is returned in the formatted string. 
		/// Otherwise the short day of week is also included and if the span between the start and end is 7 days or greater then the date is included as well.</para>
		/// </remarks>
		/// <seealso cref="DateTimeFormatType"/>
		public virtual string FormatDateRange(DateTime start, DateTime end, DateTime relativeDate, bool includeStart, bool includeEnd)
		{

			StringBuilder sb;

			sb = new StringBuilder();
			
			TimeSpan overallSpan = end.Subtract(start);

			if (includeStart)
			{
				FormatRelativeTime(start, relativeDate, overallSpan, sb);

				if (includeEnd)
					sb.Append(" - ");
			}

			if ( includeEnd )
				FormatRelativeTime(end, relativeDate, overallSpan, sb);

			if ( sb == null )
				return string.Empty;

			return sb.ToString();
		}

		#endregion //FormatDateRange	

		#region FormatDayOfWeek
		/// <summary>
		/// Returns a formatted string for the specified day of the week.
		/// </summary>
		/// <param name="formatType">One of the day of week related values that indicates whether to return the full day name (DayOfWeek), abbreviated (ShortDayOfWeek) or shortest (ShortestDayOfWeek) representation for the day of week.</param>
		/// <param name="dayOfWeek">The day of week whose string representation is to be returned.</param>
		/// <returns>The full, abbreviated or shortest version of the specified day of week.</returns>
		public virtual string FormatDayOfWeek(DateTimeFormatType formatType, DayOfWeek dayOfWeek)
		{
			var formatInfo = this.DateTimeFormatInfo;

			switch (formatType)
			{
				case DateTimeFormatType.DayOfWeek:
				return formatInfo.GetDayName(dayOfWeek);
				case DateTimeFormatType.ShortDayOfWeek:
				return formatInfo.GetAbbreviatedDayName(dayOfWeek);
				case DateTimeFormatType.ShortestDayOfWeek:

					return formatInfo.GetShortestDayName(dayOfWeek);




				default:
				throw new ArgumentException("");
			}
		}
		#endregion // FormatDayOfWeek

		#endregion //Public Methods

		#region Internal Methods

		#region Add
		internal DateTime? Add(DateTime date, TimeSpan timespan)
		{
			DateTime newDate;

			if (!this.AddSpanHelper(date, timespan, out newDate))
				return null;

			return newDate;
		} 
		#endregion // Add
		
		#region AddMinutes
		internal DateTime? AddMinutes(DateTime date, int minutes)
		{
			try
			{
				return _calendar.AddMinutes(date, minutes);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
		#endregion // AddMinutes

		#region AddDays
		internal DateTime? AddDays(DateTime date, int days)
		{
			try
			{
				return _calendar.AddDays(date, days);
			}
			catch (ArgumentException)
			{
				return null;
			}
		}
		#endregion // AddDays

		// AS 3/30/11 NA 2011.1 - Date Parsing
		#region ShouldCombineDateSections
		internal bool ShouldCombineDateSections()
		{
			return ShouldCombineDateSections(_sourceCulture ?? CultureInfo.CurrentCulture);
		} 

		internal static bool ShouldCombineDateSections(CultureInfo culture)
		{
			if (culture == null)
				return false;

			// don't combine for specific cultures (e.g. japanese, chinese, korean)
			if (_culturesToSkipCombining.Contains(culture.TwoLetterISOLanguageName))
				return false;

			return true;
		}
		#endregion //ShouldCombineDateSections

		#endregion //Internal Methods	
	
		#region Private Methods

		#region AddSpanHelper
		private bool AddSpanHelper(DateTime date, TimeSpan timespan, out DateTime adjustedDate)
		{
			int totalDays = (int)timespan.TotalDays;

			if (0 == totalDays)
				return _calendarHelper.TryAddOffset(date, (int)timespan.TotalSeconds, CalendarHelper.DateTimeOffsetType.Seconds, out adjustedDate);

			if (!_calendarHelper.TryAddOffset(date, totalDays, CalendarHelper.DateTimeOffsetType.Days, out adjustedDate))
				return false;

			// AS 4/20/11 TFS73218
			// We were passing in ticks as seconds. We really just want the modulus of the seconds for a day.
			//
			//return _calendarHelper.TryAddOffset(adjustedDate, (int)(timespan.Ticks % TimeSpan.TicksPerDay), CalendarHelper.DateTimeOffsetType.Seconds, out adjustedDate);
			const int SecondsPerDay = 24 * 60 * 60;
			return _calendarHelper.TryAddOffset(adjustedDate, (int)(timespan.TotalSeconds) % SecondsPerDay, CalendarHelper.DateTimeOffsetType.Seconds, out adjustedDate);
		} 
		#endregion // AddSpanHelper

		#region FormatRelativeTime

		private void FormatRelativeTime(DateTime dateTime, DateTime relativeDate, TimeSpan overallSpan, StringBuilder sb)
		{
			if (dateTime.Date != relativeDate.Date)
			{
				sb.Append(this.FormatDate(dateTime, DateTimeFormatType.ShortDayOfWeek));
				sb.Append(" ");
				if (Math.Abs(overallSpan.TotalDays) > 6)
				{
					sb.Append(this.FormatDate(dateTime, DateTimeFormatType.MonthDay));
					sb.Append(" ");
				}
			}

			sb.Append(this.FormatDate(dateTime, DateTimeFormatType.ShortTime));
		}

		#endregion //FormatRelativeTime	
    
		#region Initialize
		private void Initialize(Calendar calendar, DateTimeFormatInfo dateTimeFormat)
		{
			CoreUtilities.ValidateNotNull(calendar, "calendar");
			CoreUtilities.ValidateNotNull(dateTimeFormat, "dateTimeFormat");

			_calendar = calendar;
			_dateTimeFormat = dateTimeFormat;
			_calendarHelper = new CalendarHelper(calendar, dateTimeFormat);
		}
		#endregion // Initialize

		#endregion // Private Methods	

		#endregion //Methods	

		// AS 3/30/11 NA 2011.1 - Date Parsing
		#region DateFormatBuilder class
		internal static class DateFormatBuilder
		{
			#region Internal Methods

			#region BuildMonthDayRange
			internal static void BuildMonthDayRange(string pattern, bool shouldCombineDateSections, out string intraMonthFormat, out string interMonthFormat)
			{
				var list = Parse(pattern);

				// remove any day of week sections
				RemoveDayOfWeekSections(list);

				RemoveSections(list, DateSectionType.Year);
				RemoveSections(list, DateSectionType.Era);

				// change any MMMM sections into the abbreviated as outlook does
				for (int i = 0; i < list.Count; i++)
				{
					var item = list[i];

					if (item.Item2 == DateSectionType.Month)
					{
						if (item.Item1.Length % 2 == 0) // if its MMMM => MMM or MM => M
							list[i] = Tuple.Create(item.Item1.Substring(1), DateSectionType.Month);
					}
					else if (item.Item2 == DateSectionType.Day && item.Item1.Length == 2)
						list[i] = Tuple.Create("d", DateSectionType.Day);
				}

				// and in case we left some trailing whitespace we'll remove any
				RemoveWhitespace(list, true, true);

				if (shouldCombineDateSections)
				{
					intraMonthFormat = BuildRangeFormat(list, DateSectionType.Day);
					interMonthFormat = BuildDuplicateRange(list);
				}
				else
				{
					interMonthFormat = intraMonthFormat = BuildDuplicateRange(list);
				}
			}
			#endregion //BuildMonthDayRange

			#region BuildMonthYearRange
			internal static void BuildMonthYearRange(string pattern, bool shouldCombineDateSections, out string singleDateFormat, out string intraYearFormat, out string interYearFormat)
			{
				var list = Parse(pattern);

				// remove any day of week sections
				RemoveDayOfWeekSections(list);

				RemoveSections(list, DateSectionType.Day);

				// replace separators with whitespace if there isn't whitespace after it
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].Item2 == DateSectionType.Literal)
					{
						// if this is the 2nd or later item and we don't already have an adjacent
						// whitespace...
						if (i > 0 && list[i - 1].Item2 != DateSectionType.Whitespace)
						{
							if (i < list.Count - 1 && list[i + 1].Item2 != DateSectionType.Whitespace)
							{
								list.Insert(i, Tuple.Create(" ", DateSectionType.Whitespace));
								i++;
							}
						}

						list.RemoveAt(i);
						i--;
					}
				}

				// and in case we left some trailing whitespace we'll remove any
				RemoveWhitespace(list, true, true);

				if (shouldCombineDateSections)
				{
					intraYearFormat = BuildRangeFormat(list, DateSectionType.Month);
					interYearFormat = BuildDuplicateRange(list);
				}
				else
				{
					interYearFormat = intraYearFormat = BuildDuplicateRange(list);
				}

				singleDateFormat = BuildFormat(list);
			}
			#endregion //BuildMonthYearRange

			#region BuildLongDateRange
			internal static void BuildLongDateRange(string longDatePattern, bool shouldCombineDateSections, out string singleDateFormat, out string intraMonthFormat, out string interMonthFormat, out string interYearFormat)
			{
				var list = Parse(longDatePattern);

				// first remove any day of week sections
				RemoveDayOfWeekSections(list);

				if (shouldCombineDateSections)
				{
					// if there is no month and/or day section then add one. these fixups aren't ideal but 
					// are needed to ensure we have all the required sections and is based upon what 
					// outlook does.
					AddMonthDayYearSections(list);

					// and in case we left some trailing whitespace we'll remove any
					RemoveWhitespace(list, true, true);

					// now build a format for a range of dates in a given month
					intraMonthFormat = BuildRangeFormat(list, DateSectionType.Day);

					// then build a format for a range of dates in a given year
					interMonthFormat = BuildMonthRangeFormat(list);
				}
				else
				{
					// and in case we left some trailing whitespace we'll remove any
					RemoveWhitespace(list, true, true);

					intraMonthFormat = interMonthFormat = BuildDuplicateRange(list);
				}

				interYearFormat = BuildDuplicateRange(list);

				singleDateFormat = BuildFormat(list);
			}
			#endregion //BuildLongDateRange

			#endregion //Internal Methods

			#region Private Methods

			#region AddMonthDayYearSections
			private static void AddMonthDayYearSections(List<Tuple<string, DateSectionType>> list)
			{
				Predicate<Tuple<string, DateSectionType>> dayCallback = (a) => { return a.Item2 == DateSectionType.Day; };
				Predicate<Tuple<string, DateSectionType>> monthCallback = (a) => { return a.Item2 == DateSectionType.Month; };
				Predicate<Tuple<string, DateSectionType>> yearCallback = (a) => { return a.Item2 == DateSectionType.Year; };
				Predicate<Tuple<string, DateSectionType>> dateCallback = (a) => { return IsDateSection(a.Item2); };

				int daySection = ScheduleUtilities.FindIndex(list, dayCallback);
				int monthSection = ScheduleUtilities.FindIndex(list, monthCallback);
				int yearSection = ScheduleUtilities.FindIndex(list, yearCallback);

				bool hasDay = daySection >= 0;
				bool hasMonth = monthSection >= 0;
				bool hasYear = yearSection >= 0;

				int value = 0;

				if (daySection >= 0)
					value |= 0x1;

				if (monthSection >= 0)
					value |= 0x2;

				if (yearSection >= 0)
					value |= 0x4;

				// if we have mont and day sections they must be together
				if ((value & 0x3) == 0x3)
				{
					int earliest = Math.Min(daySection, monthSection);
					int latest = Math.Max(daySection, monthSection);

					if (ScheduleUtilities.FindIndex(list, earliest + 1, dateCallback) < latest)
					{
						int latestStart = FindInsertionIndex(list, latest, true);
						int latestEnd = FindInsertionIndex(list, latest, false);
						int countToMove = latestEnd - latestStart;

						// copy the sections temporarily and remove them from the list
						var sectionsToMove = list.Skip(latestStart).Take(countToMove).ToArray();
						list.RemoveRange(latestStart, countToMove);

						// then move them after the earlier section
						int insertAt = FindInsertionIndex(list, earliest, false);
						list.InsertRange(insertAt, sectionsToMove);

						// in case there wasn't any white space between them add one now
						if (list[insertAt].Item2 != DateSectionType.Whitespace && list[insertAt - 1].Item2 != DateSectionType.Whitespace)
							list.Insert(insertAt, Tuple.Create(" ", DateSectionType.Whitespace));
					}
				}

				switch (value)
				{
					case 0x0: // none
						{
							list.AddRange(Parse("M-d-yy"));
							break;
						}
					case 0x1: // day only
						{
							// if day section only then suffix "-M-yy"
							list.AddRange(Parse("-M-yy"));
							break;
						}
					case 0x2: // month only
						{
							// if month section only then suffix "-d-yy"
							list.AddRange(Parse("-d-yy"));
							break;
						}
					case 0x3: // day/month
						{
							// if month/day then suffix "-yy"
							list.AddRange(Parse("-yy"));
							break;
						}
					case 0x4: // year only
						{
							// if year section only then prefix "M-d-"
							list.InsertRange(0, Parse("M-d-"));
							break;
						}
					case 0x5: // day/year
						{
							// if day/year then add "M-" before year
							int index = FindInsertionIndex(list, yearSection, true);
							list.InsertRange(index, Parse("M-"));
							break;
						}
					case 0x6: // month/year
						{
							// if year/month then add "-d" after month
							int index = FindInsertionIndex(list, monthSection, false);
							list.InsertRange(index, Parse("-d"));
							break;
						}
					case 0x7: // month/day/year
						{
							// if we have all 3 then we can just go on
							break;
						}
				}
			}

			#endregion //AddMonthDayYearSections

			#region BuildRangeFormat
			private static string BuildRangeFormat(List<Tuple<string, DateSectionType>> list, DateSectionType type)
			{
				StringBuilder sb = new StringBuilder();

				foreach (var item in list)
				{
					BuildFormatHelper(sb, item, 0);

					if (item.Item2 == type)
					{
						sb.Append(" - ");
						BuildFormatHelper(sb, item, 1);
					}
				}

				return sb.ToString();
			}
			#endregion //BuildRangeFormat

			#region BuildDuplicateRange
			private static string BuildDuplicateRange(List<Tuple<string, DateSectionType>> list)
			{
				StringBuilder sb = new StringBuilder();

				for (int i = 0; i < 2; i++)
				{
					foreach (var item in list)
						BuildFormatHelper(sb, item, i);

					if (i == 0)
						sb.Append(" - ");
				}

				return sb.ToString();
			}
			#endregion //BuildDuplicateRange

			#region BuildFormat
			private static string BuildFormat(List<Tuple<string, DateSectionType>> list)
			{
				string singleDateFormat;
				StringBuilder sb = new StringBuilder();

				foreach (var item in list)
					BuildFormatHelper(sb, item, 0);

				singleDateFormat = sb.ToString();
				return singleDateFormat;
			}
			#endregion //BuildFormat

			#region BuildMonthRangeFormat
			private static string BuildMonthRangeFormat(List<Tuple<string, DateSectionType>> list)
			{
				Predicate<Tuple<string, DateSectionType>> dayCallback = (a) => { return a.Item2 == DateSectionType.Day; };
				Predicate<Tuple<string, DateSectionType>> monthCallback = (a) => { return a.Item2 == DateSectionType.Month; };

				int daySection = ScheduleUtilities.FindIndex(list, dayCallback);
				int monthSection = ScheduleUtilities.FindIndex(list, monthCallback);

				int earliest = Math.Min(daySection, monthSection);
				int latest = Math.Max(daySection, monthSection);

				// find the parts to cut out with the
				earliest = FindInsertionIndex(list, earliest, true);
				latest = FindInsertionIndex(list, latest, false);

				StringBuilder sb = new StringBuilder();

				// add anything before the duplication point
				for (int i = 0; i < earliest; i++)
					BuildFormatHelper(sb, list[i], 0);

				for (int j = 0; j < 2; j++)
				{
					for (int i = earliest; i < latest; i++)
					{
						BuildFormatHelper(sb, list[i], j);
					}

					if (j == 0)
						sb.Append(" - ");
				}

				for (int i = latest, count = list.Count; i < count; i++)
					BuildFormatHelper(sb, list[i], 0);

				return sb.ToString();
			}
			#endregion //BuildMonthRangeFormat

			#region BuildFormatHelper
			private static void BuildFormatHelper(StringBuilder sb, Tuple<string, DateSectionType> item, int dateReplacementIndex)
			{
				if (IsDateSection(item.Item2))
				{
					string format = item.Item1;

					sb.Append('{');
					sb.Append(dateReplacementIndex);
					sb.Append(':');

					// to ensure this doesn't get treated like the expandable single character formats
					// we need to indicate this is a custom format
					if (format.Length == 1)
						sb.Append('%');

					sb.Append(format);

					sb.Append('}');
				}
				else
				{
					sb.Append(item.Item1);
				}
			}
			#endregion //BuildFormatHelper

			#region FindInsertionIndex
			private static int FindInsertionIndex(List<Tuple<string, DateSectionType>> list, int startIndex, bool before)
			{
				int offset = before ? -1 : 1;
				int end = before ? -1 : list.Count;
				int index = startIndex + offset;

				// look for a whitespace before the next date section
				for (; index != end; index += offset)
				{
					var itemType = list[index].Item2;

					if (itemType == DateSectionType.Whitespace ||
						itemType == DateSectionType.Literal)
					{
						return before ? index + 1 : index;
					}
					else if (IsDateSection(itemType))
					{
						// assume it ends where the next section starts for going forward
						if (!before)
							return index;

						break;
					}
				}

				// if we walked until the end and didn't hit a whitespace or date section (i.e. just literals, etc.)
				// then just return the first/last index to prefix/suffix the list
				if (index == end)
					return before ? 0 : list.Count;

				return before ? startIndex : startIndex + 1;
			}
			#endregion //FindInsertionIndex

			#region IsDateSection
			private static bool IsDateSection(DateSectionType type)
			{
				switch (type)
				{
					case DateSectionType.Day:
					case DateSectionType.Month:
					case DateSectionType.Year:
					case DateSectionType.Era:
						return true;
					default:
						return false;
				}
			}
			#endregion //IsDateSection

			#region Parse
			private static List<Tuple<string, DateSectionType>> Parse(string dateFormat)
			{
				var list = new List<Tuple<string, DateSectionType>>();

				if (dateFormat != null)
				{
					DateSectionType? type = null;
					StringBuilder sb = new StringBuilder();
					const char quote = '\'';
					int last = dateFormat.Length - 1;

					for (int i = 0; i < dateFormat.Length; i++)
					{
						char ch = dateFormat[i];

						// if we're in a quote then see if this will be 
						// part of the quote
						if (type == DateSectionType.Quote)
						{
							if (ch == quote)
							{
								// if there is a double '' then consider it escaped and include just one
								if (i < last && dateFormat[i + 1] == quote)
								{
									i++;
									sb.Append(quote);
								}
								else
								{
									// the quote is over
									if (sb.Length > 0)
									{
										list.Add(Tuple.Create(sb.ToString(), type.Value));
										sb.Clear();
									}
									type = null;
								}
							}
							else
							{
								sb.Append(ch);
							}
						}
						else
						{
							DateSectionType sectionType;

							if (ch == quote)
							{
								// the start of a quote
								sectionType = DateSectionType.Quote;
							}
							else if (Char.IsWhiteSpace(ch))
							{
								// white space
								sectionType = DateSectionType.Whitespace;
							}
							else
							{
								switch (ch)
								{
									case 'M':
									case 'm':
										sectionType = DateSectionType.Month;
										break;
									case 'd':
									case 'D':
										sectionType = DateSectionType.Day;
										break;
									case 'y':
									case 'Y':
										sectionType = DateSectionType.Year;
										break;
									case 'g':
									case 'G':
										sectionType = DateSectionType.Era;
										break;
									default:
										sectionType = DateSectionType.Literal;
										break;
								}
							}

							if (sectionType != type)
							{
								if (type != null && sb.Length > 0)
								{
									list.Add(Tuple.Create(sb.ToString(), type.Value));
									sb.Clear();
								}

								type = sectionType;
							}

							if (sectionType != DateSectionType.Quote)
								sb.Append(ch);
						}
					}

					if (type != null && sb.Length > 0)
						list.Add(Tuple.Create(sb.ToString(), type.Value));
				}

				return list;
			}
			#endregion //Parse

			#region RemoveDayOfWeekSections
			private static void RemoveDayOfWeekSections(List<Tuple<string, DateSectionType>> list)
			{
				// in outlook if the first date section is a day of week then everything up to the next day section is removed
				// otherwise they remove everything from the previous section up to the day of week section
				Predicate<Tuple<string, DateSectionType>> dayOfWeekCallback = (a) =>
				{
					return IsDateSection(a.Item2) && a.Item2 == DateSectionType.Day && a.Item1.Length >= 3;
				};

				Predicate<Tuple<string, DateSectionType>> nonDayOfWeekCallback = (a) =>
				{
					return IsDateSection(a.Item2) && (a.Item2 != DateSectionType.Day || a.Item1.Length < 3);
				};

				int count = list.Count;
				int dayOfWeekIndex = ScheduleUtilities.FindIndex(list, dayOfWeekCallback);

				// if there aren't any such sections then we can exit
				if (dayOfWeekIndex < 0)
					return;

				int previousSection = -1;

				// if there is then we need to see if there is a date section before it
				if (dayOfWeekIndex > 0)
					previousSection = ScheduleUtilities.FindLastIndex(list, dayOfWeekIndex - 1, nonDayOfWeekCallback);

				if (previousSection >= 0)
				{
					int sectionEnd = FindInsertionIndex(list, previousSection, false);

					list.RemoveRange(sectionEnd, (dayOfWeekIndex - sectionEnd) + 1);
				}
				else
				{
					// otherwise find the date section after it and remove everything up to the start of that section
					int nextSection = ScheduleUtilities.FindIndex(list, dayOfWeekIndex + 1, nonDayOfWeekCallback);

					if (nextSection >= 0)
					{
						int sectionStart = FindInsertionIndex(list, nextSection, true);
						list.RemoveRange(0, sectionStart);
					}
					else
					{
						// there are no other sections?
						list.Clear();
					}
				}

				// call the routine again if we removed sections in case there are more
				if (list.Count != count)
					RemoveDayOfWeekSections(list);
			}
			#endregion //RemoveDayOfWeekSections

			#region RemoveSections
			private static void RemoveSections(List<Tuple<string, DateSectionType>> list, DateSectionType type)
			{
				Predicate<Tuple<string, DateSectionType>> typeCallback = (a) => { return a.Item2 == type; };
				Predicate<Tuple<string, DateSectionType>> dateCallback = (a) => { return IsDateSection(a.Item2); };

				int index = ScheduleUtilities.FindIndex(list, typeCallback);

				if (index < 0)
					return;

				int listCount = list.Count;
				int previousSection = index == 0 ? -1 : ScheduleUtilities.FindLastIndex(list, index - 1, dateCallback);
				int nextSection = index == listCount - 1 ? -1 : ScheduleUtilities.FindIndex(list, index + 1, dateCallback);

				// start at the end of the previous section
				int start = previousSection < 0 ? 0 : FindInsertionIndex(list, previousSection, false);

				// end just after the section or at the end of the list
				int end = nextSection < 0 ? list.Count : FindInsertionIndex(list, index, false);

				list.RemoveRange(start, end - start);

				// then try again in case there are more
				RemoveSections(list, type);
			}
			#endregion //RemoveSections

			#region RemoveWhitespace
			private static void RemoveWhitespace(List<Tuple<string, DateSectionType>> list, bool leading, bool trailing)
			{
				Predicate<Tuple<string, DateSectionType>> nonWhiteSpaceCallback = (a) => { return a.Item2 != DateSectionType.Whitespace; };

				if (leading)
				{
					int index = ScheduleUtilities.FindIndex(list, nonWhiteSpaceCallback);

					if (index > 0)
						list.RemoveRange(0, index);
				}

				if (trailing)
				{
					int index = ScheduleUtilities.FindLastIndex(list, nonWhiteSpaceCallback) + 1;

					if (index < list.Count - 1)
					{
						list.RemoveRange(index, list.Count - index);
					}
				}
			}
			#endregion //RemoveWhitespace

			#endregion //Private Methods

			#region DateSectionType enum
			private enum DateSectionType
			{
				Month,
				Day,
				Year,
				Era,
				Quote,
				Literal, // separators
				Whitespace
			}
			#endregion //DateSectionType enum
		}
		#endregion //DateFormatBuilder class
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