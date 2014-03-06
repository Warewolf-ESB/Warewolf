using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;







using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules

{
	#region DateRecurrenceRuleBase Class

	/// <summary>
	/// Base class for date recurrence rule.
	/// </summary>
	public abstract class DateRecurrenceRuleBase : PropertyChangeNotifierExtended
	{
		#region Properties

		#region Internal Properties

		#region Unit

		internal abstract DateRecurrenceFrequency Unit
		{
			get;
		}

		#endregion // Unit

		#endregion // Internal Properties

		#endregion // Properties

		#region Base Overrides

		#region Equals

		/// <summary>
		/// Overridden. Returns true if the specified object equals this object.
		/// </summary>
		/// <param name="obj">Object to compare.</param>
		/// <returns>True if the object equals this object. False otherwise.</returns>
		public override bool Equals( object obj )
		{
			DateRecurrenceRuleBase r = obj as DateRecurrenceRuleBase;
			if ( null != r )
			{
				if ( this.CompareValue == r.CompareValue )
				{
					return r.GetType( ) == this.GetType( );
				}
			}

			return false;
		}

		#endregion // Equals

		#region GetHashCode

		/// <summary>
		/// Overridden. Returns the hash code of this object.
		/// </summary>
		/// <returns>Integer hash code value.</returns>
		public override int GetHashCode( )
		{
			return this.CompareValue.GetHashCode( );
		}

		#endregion // GetHashCode

		#endregion // Base Overrides

		#region Methods

		#region Internal Methods

		#region Apply

		internal abstract void Apply( DateRecurrenceCache.State state );

		#endregion // Apply

		#region Clone

		internal virtual DateRecurrenceRuleBase Clone( )
		{
			return (DateRecurrenceRuleBase)this.MemberwiseClone( );
		} 

		#endregion // Clone

		#region CompareValue

		internal virtual int CompareValue
		{
			get
			{
				return -1;
			}
		} 

		#endregion // CompareValue

		#region GetSortValue

		internal abstract int GetSortValue( DateRecurrenceCache cache );

		#endregion // GetSortValue

		#endregion // Internal Methods

		#endregion // Methods
	} 

	#endregion // DateRecurrenceRuleBase Class

	#region MonthOfYearRecurrenceRule Class

	/// <summary>
	/// Rule that matches a month.
	/// </summary>
	public class MonthOfYearRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _month;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="MonthOfYearRecurrenceRule"/> object.
		/// </summary>
		public MonthOfYearRecurrenceRule( ) : this (1)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="MonthOfYearRecurrenceRule"/> object.
		/// </summary>
		/// <param name="month">Identifies the month. Valid values are 1 to 12.</param>
		public MonthOfYearRecurrenceRule( int month )
		{
			Validate(month);
			_month = month;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _month;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region Month

		/// <summary>
		/// Gets/sets the month number.
		/// </summary>
		public int Month
		{
			get
			{
				return _month;
			}
			set
			{
				if (value != _month)
				{
					Validate(value);

					_month = value;
					this.RaisePropertyChangedEvent("Month");
				}
			}
		}

		#endregion // Month

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Monthly;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 1 << 24 ) + _month;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			state.IntersectMonth( _month );
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int month)
		{
			if (month < 1 || month > 12)
				throw new ArgumentOutOfRangeException("month");
		}

		#endregion //Validate
	} 

	#endregion // MonthOfYearRecurrenceRule Class

	#region WeekOfYearRecurrenceRule Class

	/// <summary>
	/// Rule that matches week number of a year.
	/// </summary>
	public class WeekOfYearRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _weekNumber;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="WeekOfYearRecurrenceRule"/> object.
		/// </summary>
		public WeekOfYearRecurrenceRule() : this(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="WeekOfYearRecurrenceRule"/> object.
		/// </summary>
		/// <param name="weekNumber">Identifies the week in the year. Valid values are 1 to 53 and -53 to -1.
		/// <see cref="WeekNumber"/> for more information.
		/// </param>
		public WeekOfYearRecurrenceRule( int weekNumber )
		{
			Validate(weekNumber);

			_weekNumber = weekNumber;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _weekNumber;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region WeekNumber

		/// <summary>
		/// Gets/sets the number that identifies the week of the year. Valid range of values is 1 to 53 and -53 to -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>WeekNumber</b> identifies the week in the year. Valid values are 1 to 53 and -53 to -1. A negative value
		/// indicates the n'th week to the end of the year where 'n' is the absolute value of the negative value. For example,
		/// -1 specifies the last week of the year. Likewise -10 specifies the 10th to the last week of the year.
		/// </para>
		/// </remarks>
		public int WeekNumber
		{
			get
			{
				return _weekNumber;
			}
			set
			{
				if (_weekNumber != value)
				{
					Validate(value);

					_weekNumber = value;
					this.RaisePropertyChangedEvent("WeekNumber");
				}
			}
		}

		#endregion // WeekNumber

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Weekly;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 2 << 24 ) + _weekNumber;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			state.IntersectWeek( _weekNumber );
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int weekOfYear)
		{
			int abs = Math.Abs(weekOfYear);
 
			if (abs < 1 || abs > 53)
				throw new ArgumentOutOfRangeException("weekOfYear");
		}

		#endregion //Validate
	} 

	#endregion // WeekOfYearRecurrenceRule Class

	#region DayOfYearRecurrenceRule Class

	/// <summary>
	/// Rule that matches day of year.
	/// </summary>
	public class DayOfYearRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _dayOfYear;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfYearRecurrenceRule"/> object.
		/// </summary>
		public DayOfYearRecurrenceRule( ) : this(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfYearRecurrenceRule"/> object.
		/// </summary>
		/// <param name="dayOfYear">Identifies the day of the year. Valid values are from 1 to 366 and -366 to -1.
		/// <see cref="DayOfYear"/> for more information.
		/// </param>
		public DayOfYearRecurrenceRule( int dayOfYear )
		{
			Validate(dayOfYear);
			_dayOfYear = dayOfYear;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _dayOfYear;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 3 << 24 ) + _dayOfYear;
		}

		#endregion // GetSortValue

		#region DayOfYear

		/// <summary>
		/// Gets/sets the day of the year. Valid range of values are 1 to 366 and -366 to -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DayOfYear</b> identifies the day of the year. Valid values are 1 to 366 and -366 to -1. A negative value
		/// indicates the n'th day to the end of the year where 'n' is the absolute value of the negative value. For example,
		/// -1 specifies the last day of the year. Likewise -10 specifies the 10th day to the last day the year.
		/// </para>
		/// </remarks>
		public int DayOfYear
		{
			get
			{
				return _dayOfYear;
			}
			set
			{
				if (_dayOfYear != value)
				{
					Validate(value);

					_dayOfYear = value;
					this.RaisePropertyChangedEvent("DayOfYear");
				}
			}
		}

		#endregion // DayOfYear

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Daily;
			}
		}

		#endregion // Unit

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			state.IntersectDayOfYear( _dayOfYear );
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int dayOfYear)
		{
			int abs = Math.Abs(dayOfYear);

			if (abs < 1 || abs > 366)
				throw new ArgumentOutOfRangeException("dayOfYear");
		}

		#endregion //Validate
	}

	#endregion // DayOfYearRecurrenceRule Class

	#region DayOfMonthRecurrenceRule Class

	/// <summary>
	/// Rule that matches day of month.
	/// </summary>
	public class DayOfMonthRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _dayOfMonth;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfMonthRecurrenceRule"/> object.
		/// </summary>
		public DayOfMonthRecurrenceRule( ) : this(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfMonthRecurrenceRule"/> object.
		/// </summary>
		/// <param name="dayOfMonth">
		/// Identifies the day of the year. Valid values are from 1 to 31 and -31 to -1.
		/// <see cref="DayOfMonth"/> for more information.
		/// </param>
		public DayOfMonthRecurrenceRule( int dayOfMonth )
		{
			Validate(dayOfMonth);
			_dayOfMonth = dayOfMonth;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _dayOfMonth;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region DayOfMonth

		/// <summary>
		/// Gets/sets the day of the month. Valid range of values are 1 to 31 and -31 to -1.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>DayOfMonth</b> identifies the day of the month. Valid values are 1 to 31 and -31 to -1. A negative value
		/// indicates the n'th day to the end of the month where 'n' is the absolute value of the negative value. For example,
		/// -1 specifies the last day of the month. Likewise -10 specifies the 10th day to the last day the month.
		/// </para>
		/// </remarks>
		public int DayOfMonth
		{
			get
			{
				return _dayOfMonth;
			}
			set
			{
				if (_dayOfMonth != value)
				{
					Validate(value);

					_dayOfMonth = value;
					this.OnPropertyChanged("DayOfMonth");
				}
			}
		}

		#endregion // DayOfMonth

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Daily;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 4 << 24 ) + _dayOfMonth;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			state.IntersectDayOfMonth( _dayOfMonth );
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int dayOfMonth)
		{
			int abs = Math.Abs(dayOfMonth);

			if (abs < 1 || abs > 31)
				throw new ArgumentOutOfRangeException("dayOfMonth");
		}

		#endregion //Validate
	} 

	#endregion // DayOfMonthRecurrenceRule Class

	#region DayOfWeekRecurrenceRule Class

	/// <summary>
	/// Rule that matches day of week or a relative day of week within month or year.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>DayOfWeekRecurrenceRule</b> can be used to specify a day of week or a relative
	/// day of week within month or year. If the frequency is monthly or if a 
	/// <see cref="MonthOfYearRecurrenceRule"/> is present then the 
	/// <see cref="DayOfWeekRecurrenceRule.RelativePosition"/> 
	/// indentifies the relative position within a month. Otherwise if the 
	/// frequency of the recurrence is yearly
	/// then it identifies the relative position within the year. In case of
	/// monthly, valid values are -5 to -1 and 1 to 5. In case of yearly,
	/// valid values are -53 to -1 and 1 to 53.
	/// </para>
	/// </remarks>
	public class DayOfWeekRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private DayOfWeek _day;
		private int _relativePosition;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfWeekRecurrenceRule"/> object.
		/// </summary>
		public DayOfWeekRecurrenceRule() : this(DayOfWeek.Monday)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="DayOfWeekRecurrenceRule"/> object.
		/// </summary>
		/// <param name="day">Identifies the week day.</param>
		/// <param name="relativePosition">Optional value that specifies the relative
		/// position of the day within month or year. If the frequency is monthly or if a 
		/// <see cref="MonthOfYearRecurrenceRule"/> is present then the relative position 
		/// indentifies the relative position within a month. If frequency is yearly
		/// then it identifies the relative position within the year. In case of
		/// monthly, valid values are -5 to -1 and 1 to 5. In case of yearly,
		/// valid values are -53 to -1 and 1 to 53.</param>
		public DayOfWeekRecurrenceRule( DayOfWeek day, int relativePosition = 0 )
		{
			Validate(relativePosition);
			_day = day;
			_relativePosition = relativePosition;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return ( _relativePosition << 16 ) ^ (int)_day;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region Day

		/// <summary>
		/// Gets/sets the week day.
		/// </summary>
		public DayOfWeek Day
		{
			get
			{
				return _day;
			}
			set
			{
				if (_day != value)
				{

					_day = value;
					this.RaisePropertyChangedEvent("Day");
				}
			}
		}

		#endregion // Day

		#region RelativePosition

		/// <summary>
		/// Relative position of the occurrence of the week-day within a month or a year. 
		/// Valid values are 1 to 5 and -5 to -1 for monthly frequency and 1 to 53 and -53 to -1 
		/// for yearly frequency.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RelativePosition</b> specifies the relative position of the occurrence of the week-day within a 
		/// month or a year. Valid values are 1 to 5 and -5 to -1 for monthly rule and 1 to 53 and -53 to -1 for yearly rule. 
		/// For example, if <see cref="DayOfWeekRecurrenceRule.Day"/> is Monday and the <i>RelativePosition</i> is -1, 
		/// then it specifies the last Monday in the month.
		/// </para>
		/// </remarks>
		public int RelativePosition
		{
			get
			{
				return _relativePosition;
			}
			set
			{
				if (_relativePosition != value)
				{
					Validate(value);

					_relativePosition = value;
					this.RaisePropertyChangedEvent("RelativePosition");
				}
			}
		}

		#endregion // RelativePosition

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Daily;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			// SSP 6/28/11 TFS80109
			// 
			
			//return ( 5 << 24 ) + (int)_day;
			int weekStart = null != cache ? (int)cache._recurrence.WeekStart : (int)DayOfWeek.Monday;
			int day = (int)_day;

			if ( day < weekStart )
				day += 7;

			return ( 5 << 24 ) + day;
			
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			state.IntersectDayOfWeek( _day, _relativePosition );
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int relativePosition)
		{
			int abs = Math.Abs(relativePosition);

			if (abs > 53)
				throw new ArgumentOutOfRangeException("relativePosition");
		}

		#endregion //Validate
	}

	#endregion // DayOfWeekRecurrenceRule Class

	#region HourRecurrenceRule Class

	/// <summary>
	/// Rule that matches hour of day.
	/// </summary>
	public class HourRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _hour;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="HourRecurrenceRule"/> object.
		/// </summary>
		public HourRecurrenceRule( ) : this(0)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="HourRecurrenceRule"/> object.
		/// </summary>
		/// <param name="hour">Specifes the hour. Valid values are 0 to 23.</param>
		public HourRecurrenceRule( int hour )
		{
			Validate(hour);
			_hour = hour;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _hour;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region Hour

		/// <summary>
		/// Gets/sets the hour value. Valid values are 0 to 23.
		/// </summary>
		public int Hour
		{
			get
			{
				return _hour;
			}
			set
			{
				if (_hour != value)
				{
					Validate(value);

					_hour = value;
					this.RaisePropertyChangedEvent("Hour");
				}
			}
		}

		#endregion // Hour

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Hourly;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 6 << 24 ) + _hour;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			// SSP 4/11/11 TFS66178
			// 
			
			//state._hour = _hour;
			if ( state._hour >= 0 && state._hour != _hour )
				state._days.MakeEmpty( );
			else
				state._hour = _hour;
			
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int hour)
		{
			if (hour < 0 ||hour > 23)
				throw new ArgumentOutOfRangeException("hour");
		}

		#endregion //Validate
	} 

	#endregion // HourRecurrenceRule Class

	#region MinuteRecurrenceRule Class

	/// <summary>
	/// Rule that matches minute of hour.
	/// </summary>
	public class MinuteRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _minute;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="MinuteRecurrenceRule"/> object.
		/// </summary>
		public MinuteRecurrenceRule( ) : this(0)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="MinuteRecurrenceRule"/> object.
		/// </summary>
		/// <param name="minute">Specifes the minute value. Valid values are 0 to 59.</param>
		public MinuteRecurrenceRule( int minute )
		{
			Validate(minute);

			_minute = minute;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _minute;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region Minute

		/// <summary>
		/// Gets/sets the minute value. Valid values are 0 to 59.
		/// </summary>
		public int Minute
		{
			get
			{
				return _minute;
			}
			set
			{
				if (_minute != value)
				{
					Validate(value);

					_minute = value;
					this.RaisePropertyChangedEvent("Minute");
				}
			}
		}

		#endregion // Minute

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Minutely;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 7 << 24 ) + _minute;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			// SSP 4/11/11 TFS66178
			// 
			
			//state._minute = _minute;
			if ( state._minute >= 0 && state._minute != _minute )
				state._days.MakeEmpty( );
			else
				state._minute = _minute;
			
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int minute)
		{
			if (minute < 0 || minute > 59)
				throw new ArgumentOutOfRangeException("minute");
		}

		#endregion //Validate
	} 

	#endregion // MinuteRecurrenceRule Class

	#region SecondRecurrenceRule Class

	/// <summary>
	/// Rule that matches second of a minute.
	/// </summary>
	public class SecondRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _second;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="SecondRecurrenceRule"/> object.
		/// </summary>
		public SecondRecurrenceRule( ) : this(0)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="SecondRecurrenceRule"/> object.
		/// </summary>
		/// <param name="second">Specifes the second value. Valid values are 0 to 59.</param>
		public SecondRecurrenceRule( int second )
		{
			Validate(second);

			_second = second;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _second;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region Second

		/// <summary>
		/// Gets/sets the second value. Valid values are 0 to 59.
		/// </summary>
		public int Second
		{
			get
			{
				return _second;
			}
			set
			{
				if (_second != value)
				{
					Validate(value);

					_second = value;
					this.RaisePropertyChangedEvent("Second");
				}
			}
		}

		#endregion // Second

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				return DateRecurrenceFrequency.Secondly;
			}
		}

		#endregion // Unit

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 8 << 24 ) + _second;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
			// SSP 4/11/11 TFS66178
			// 
			
			//state._second = _second;
			if ( state._second >= 0 && state._second != _second )
				state._days.MakeEmpty( );
			else
				state._second = _second;
			
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int second)
		{
			if (second < 0 || second > 59)
				throw new ArgumentOutOfRangeException("second");
		}

		#endregion //Validate
	}

	#endregion // SecondRecurrenceRule Class

	#region SubsetRecurrenceRule Class

	/// <summary>
	/// Rule that matches a subset of matches within an interval.
	/// </summary>
	public class SubsetRecurrenceRule : DateRecurrenceRuleBase
	{
		#region Private Vars

		private int _occurrenceInstance;

		#endregion // Private Vars

		#region Constructor

		/// <summary>
		/// Initializes a new instance of <see cref="SubsetRecurrenceRule"/> object.
		/// </summary>
		public SubsetRecurrenceRule() : this(1)
		{
		}

		/// <summary>
		/// Initializes a new instance of <see cref="SubsetRecurrenceRule"/> object.
		/// </summary>
		/// <param name="occurrenceInstance">
		/// Specifies the instance of the occurrence within the set of occurrences.
		/// <see cref="OccurrenceInstance"/> for more information. Valid values are 1 to 366 
		/// and -366 to -1 where negative values indicate nth to the last.
		/// </param>
		public SubsetRecurrenceRule( int occurrenceInstance )
		{
			Validate(occurrenceInstance);

			_occurrenceInstance = occurrenceInstance;
		}

		#endregion // Constructor

		#region Base Overrides

		#region CompareValue

		internal override int CompareValue
		{
			get
			{
				return _occurrenceInstance;
			}
		}

		#endregion // CompareValue

		#endregion // Base Overrides

		#region OccurrenceInstance

		/// <summary>
		/// Identifies the instance of the occurrence within the set of occurrences within the period specified by the 
		/// <see cref="DateRecurrence.Frequency"/> and and <see cref="DateRecurrence.Interval"/> 
		/// properties that are generated by all the recurrence rules of the <see cref="DateRecurrence"/> that are 
		/// not SubsetRecurrenceRule's. Valid values are 1 to 366 and -366 to -1 where negative values 
		/// indicate nth to the last.
		/// </summary>
		public int OccurrenceInstance
		{
			get
			{
				return _occurrenceInstance;
			}
			set
			{
				if (_occurrenceInstance != value)
				{
					Validate(value);

					_occurrenceInstance = value;
					this.RaisePropertyChangedEvent("OccurrenceInstance");
				}
			}
		}

		#endregion // OccurrenceInstance

		#region GetSortValue

		internal override int GetSortValue( DateRecurrenceCache cache )
		{
			return ( 9 << 24 ) + _occurrenceInstance;
		}

		#endregion // GetSortValue

		#region Apply

		internal override void Apply( DateRecurrenceCache.State state )
		{
		}

		#endregion // Apply

		#region Validate

		private static void Validate(int instance)
		{
			int abs = Math.Abs(instance);

			if (abs < 1 || abs > 366)
				throw new ArgumentOutOfRangeException("OccurrenceInstance");
		}

		#endregion //Validate

		#region Unit

		internal override DateRecurrenceFrequency Unit
		{
			get
			{
				throw new NotImplementedException( );
			}
		}

		#endregion // Unit
	}

	#endregion // SubsetRecurrenceRule Class

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