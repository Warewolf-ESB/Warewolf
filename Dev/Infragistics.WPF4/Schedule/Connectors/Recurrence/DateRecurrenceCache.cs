using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules

{
	internal class DateRecurrenceCache
	{
		#region Nested Data Structures

		#region Enumerable Class

		private class Enumerable : IEnumerable<DateTime>
		{
			#region Enumerator Class

			private class Enumerator : IEnumerator<DateTime>
			{
				#region Member Vars

				private Enumerable _owner;
				private State _state;
				private DateRange _dateRange;
				private int _maxOccurrencesToGenerate;

				private int _count;
				private bool _done;
				private DateTime _current;

				#endregion // Member Vars

				#region Constructor

				internal Enumerator( Enumerable owner )
				{
					_owner = owner;
					_dateRange = _owner._dateRange;
					_maxOccurrencesToGenerate = _owner._maxOccurrencesToGenerate;

					_dateRange.End = ScheduleUtilities.Min( _dateRange.End, _owner._cache._until );

					this.Reset( );
				}

				#endregion // Constructor

				#region Current

				public DateTime Current
				{
					get
					{
						return _current;
					}
				}

				#endregion // Current

				#region Dispose

				public void Dispose( )
				{
				}

				#endregion // Dispose

				#region MoveNext

				public bool MoveNext( )
				{
					while ( !_done )
					{
						_state.Run( );
						_done = _state.IsDone( _dateRange.End );

						if ( _state.IsMatch )
						{
							_current = _state._date;
							bool isCurrentValid = _dateRange.Contains( _current );

							if ( isCurrentValid )
								_count++;

							if ( ! _done )
								_done =
									// SSP 4/11/11 TFS66178
									// 
									//!isCurrentValid && _current >= _dateRange.End || 
									ScheduleUtilities.MaxReached( _maxOccurrencesToGenerate, _count );

							if ( isCurrentValid )
								return true;
						}
					}

					return false;
				}

				#endregion // MoveNext

				#region Reset

				public void Reset( )
				{
					_state = new State( _owner._cache );
					_state.InitializeRun( _dateRange.Start );
					_count = 0;
					
					// SSP 3/16/11 TFS66981
					// 
					//_done = false;
					_done = _state.IsDone( _dateRange.End );
				}

				#endregion // Reset

				#region IEnumerator Implementation

				object System.Collections.IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}

				#endregion // IEnumerator Implementation
			}

			#endregion // Enumerator Class

			#region Member Vars

			private DateRecurrenceCache _cache;
			private DateRange _dateRange;
			private int _maxOccurrencesToGenerate;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="cache">Cache.</param>
			/// <param name="dateRange">Occurrences within this date range will be enumerated.</param>
			/// <param name="maxOccurrencesToGenerate">Maximum number of occurrences to enumerate. 0 specifies no limit.</param>
			internal Enumerable( DateRecurrenceCache cache, DateRange dateRange, int maxOccurrencesToGenerate = 0 )
			{
				_cache = cache;
				_dateRange = dateRange;
				_maxOccurrencesToGenerate = maxOccurrencesToGenerate;
			}

			#endregion // Constructor

			#region GetEnumerator

			public IEnumerator<DateTime> GetEnumerator( )
			{
				return new Enumerator( this );
			}

			#endregion // GetEnumerator

			#region IEnumerable.GetEnumerator

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion // IEnumerable.GetEnumerator
		}

		#endregion // Enumerable Class

		#region Range Struct

		internal struct Range
		{
			private int _start, _end;

			public Range( int start, int end )
			{
				_start = start;
				_end = end;
			}

			public int Start
			{
				get
				{
					return _start;
				}
				set
				{
					_start = value;
				}
			}

			public int End
			{
				get
				{
					return _end;
				}
				set
				{
					_end = value;
				}
			}

			public void Intersect( Range range )
			{
				_start = Math.Max( _start, range._start );
				_end = Math.Min( _end, range._end );
			}

			public void Intersect( int value )
			{
				_start = Math.Max( _start, value );
				_end = Math.Min( _end, value );
			}

			// SSP 3/29/11 TFS66178
			// 
			public void MoveForwardStart( int newStart )
			{
				if ( newStart > _start )
					_start = newStart;
			}

			public bool IsEmpty
			{
				get
				{
					return _end < _start;
				}
			}

			public static Range Empty
			{
				get
				{
					return new Range( -1, 0 );
				}
			}

			public void Offset( int offset )
			{
				_start += offset;
				_end += offset;
			}

			public void Expand( int amount )
			{
				_end += amount;
			}

			public bool Contains( int val )
			{
				return val >= _start && val <= _end;
			}

			// SSP 4/11/11 TFS66178
			// 
			internal void MakeEmpty( )
			{
				_end = _start - 1;
			}

			public int Length
			{
				get
				{
					return 1 + _end - _start;
				}
			}
		}

		#endregion // Range Struct

		#region RunningState Structure

		private struct RunningState
		{
			internal Range _days;
			internal int _year;
			internal int _hour;
			internal int _minute;
			internal int _second;
		}

		#endregion // RunningState Structure

		#region State Class

		internal class State
		{
			private DateRecurrenceCache _cache;
			private int _previousMatchCount;
			private DateTime _lastFrequencyDate;
			private Stack<RunningState> _runningStates;
			private int _cSet;
			private int[] _cRuleIndex;
			private bool _isMatch;
			private bool _isDone;
			// SSP 4/4/11 TFS66178
			// 
			private bool _rrHasNextMatch;
			private int[] _rrHasNextMatchStart;
			private bool[] _rrRuleHasNextMatch;
			private bool[] _rrSetHasNextMatch;
			private int[] _rrSetPass;
			private SubsetInfo _subsetInfo;

			internal Range _days;
			internal int _year;
			internal int _hour;
			internal int _minute;
			internal int _second;
			internal DateTime _date;

			internal State( DateRecurrenceCache cache )
			{
				_cache = cache;
				_runningStates = new Stack<RunningState>( );
				_days = Range.Empty;
				_year = -1;
				_hour = -1;
				_minute = -1;
				_second = -1;

				// SSP 4/4/11 TFS66178
				// 
				// SSP 6/28/11 TFS80109
				// Always allocate SubsetInfo which is also used to sort matched dates within an interval.
				// 
				//var subsetRules = _cache._subsetRules;
				//_subsetInfo = null != subsetRules && subsetRules.Length > 0 ? new SubsetInfo( subsetRules ) : null;
				_subsetInfo = _cache._sortedRules.Length > 0 ? new SubsetInfo( _cache._subsetRules ) : null;
			}

			internal State Clone( )
			{
				State clone = (State)this.MemberwiseClone( );

				clone._runningStates = new Stack<RunningState>( _runningStates );
				clone._cRuleIndex = (int[])_cRuleIndex.Clone( );

				return clone;
			}

			internal void InitializeRun( DateTime generateStartingFromDate )
			{
				_cSet = 0;
				_cRuleIndex = new int[_cache._ruleSets.Length];

				int[] ruleSets = _cache._ruleSets;
				for ( int i = 0; i < ruleSets.Length; i++ )
					_cRuleIndex[i] = ruleSets[i];

				_lastFrequencyDate = this.CalcInitialDate( generateStartingFromDate );
				this.IncrementByFrequencyHelper( true );

				// SSP 4/4/11 TFS66178
				// 
				_rrHasNextMatch = false;
				int ruleCount = _cache._sortedRules.Length;
				int setCount = _cache._ruleSets.Length;
				_rrHasNextMatchStart = new int[ruleCount];
				_rrRuleHasNextMatch = new bool[ruleCount];
				_rrSetHasNextMatch = new bool[setCount];
				_rrSetPass = new int[setCount];
			}

			internal void Run( )
			{
				_isMatch = false;

				// SSP 4/4/11 TFS66178
				// 
				if ( null != _subsetInfo && _subsetInfo._isEnumerating && _subsetInfo.MoveNext( ) )
				{
					this.ProcessMatch( );
					return;
				}

				DateRecurrenceRuleBase[] sortedRules = _cache._sortedRules;
				if ( sortedRules.Length > 0 )
				{
					int[] ruleSets = _cache._ruleSets;
					int nextSet = 1 + _cSet;
					bool isLeafSet = nextSet == ruleSets.Length;
					int ruleEnd = !isLeafSet ? ruleSets[nextSet] - 1 : sortedRules.Length - 1;

					int ruleIndex = _cRuleIndex[_cSet];

					// SSP 3/29/11 TFS66178
					// 
					if ( _rrSetPass[_cSet] > 0 )
					{
						while ( ruleIndex <= ruleEnd && !_rrRuleHasNextMatch[ruleIndex] )
							ruleIndex++;
					}

					if ( ruleIndex > ruleEnd )
					{
						_cRuleIndex[_cSet] = ruleSets[_cSet];

						// SSP 3/29/11 TFS66178
						// Added the if block and enclosed the existing code into the else block.
						// 
						if ( _rrSetHasNextMatch[_cSet] )
						{
							_rrSetHasNextMatch[_cSet] = false;
							_rrSetPass[_cSet]++;
						}
						else
						{
							// SSP 3/29/11 TFS66178
							// 
							_rrSetPass[_cSet] = 0;

							_cSet--;

							if ( -1 == _cSet )
							{
								_cSet = 0;

								// SSP 4/4/11 TFS66178
								// Enclosed the existing code in the if block. If we have subset rules then process
								// the matching dates from the last frequency interval, which are accumulated into
								// the subsetInfo.
								// 
								//this.IncrementByFrequencyHelper( false );
								if ( null != _subsetInfo && _subsetInfo.StartEnumerating( ) )
									// Set the new index value to a value that will cause us to come back here (based
									// on condition above) so we can increment the frequency and evaluate the rules
									// for the next interval.
									// 
									_cRuleIndex[_cSet] = int.MaxValue;
								else
									this.IncrementByFrequencyHelper( false );
							}
							else
								this.RestoreRunningState( );
						}
					}
					else
					{
						this.SaveRunningState( );
						DateRecurrenceRuleBase iiRule = sortedRules[ruleIndex];

						// SSP 3/29/11 TFS66178
						// 
						_rrHasNextMatch = false;
						int tmpStart = _rrHasNextMatchStart[ruleIndex];
						if ( tmpStart > 0 )
							_days.MoveForwardStart( tmpStart );

						iiRule.Apply( this );						
						_cRuleIndex[_cSet] = 1 + ruleIndex;

						// SSP 3/29/11 TFS66178
						// 
						if ( _rrHasNextMatch )
							_rrSetHasNextMatch[_cSet] = true;

						_rrRuleHasNextMatch[ruleIndex] = _rrHasNextMatch;
						_rrHasNextMatchStart[ruleIndex] = _rrHasNextMatch ? 1 + _days.Start : 0;
			
						if ( _days.IsEmpty )
						{
							this.RestoreRunningState( );
						}
						else if ( isLeafSet )
						{
							this.ProcessMatch( );
							this.RestoreRunningState( );
						}
						else
							_cSet++;
					}
				}
				else
				{
					this.ProcessMatch( );
					this.IncrementByFrequencyHelper( false );
				}
			}

			//internal void Run( )
			//{
			//    _isMatch = false;

			//    // SSP 4/4/11 TFS66178
			//    // 
			//    if ( null != _subsetInfo && _subsetInfo._isEnumerating )
			//    {
			//        if ( _subsetInfo.MoveNext( ) )
			//        {
			//            this.ProcessMatch( );
			//            return;
			//        }
			//        else
			//        {
			//            _cSet = -1;
			//        }
			//    }

			//    DateRecurrenceRuleBase[] sortedRules = _cache._sortedRules;
			//    if ( sortedRules.Length > 0 )
			//    {
			//        int[] ruleSets = _cache._ruleSets;
			//        int nextSet = 1 + _cSet;
			//        bool isLeafSet = nextSet == ruleSets.Length;
			//        int ruleEnd = !isLeafSet ? ruleSets[nextSet] - 1 : sortedRules.Length - 1;

			//        int ruleIndex = _cSet >= 0 ? _cRuleIndex[_cSet] : -1;
			//        if ( ruleIndex > ruleEnd )
			//        {
			//            _cRuleIndex[_cSet] = ruleSets[_cSet];

			//            // SSP 3/29/11 TFS66178
			//            // Added the if block and enclosed the existing code into the else block.
			//            // 
			//            if ( _rrSetHasNextMatch[_cSet] )
			//            {
			//                _rrSetHasNextMatch[_cSet] = false;
			//            }
			//            else
			//            {
			//                _cSet--;

			//                if ( -1 == _cSet )
			//                {
			//                    _cSet = 0;

			//                    // SSP 4/4/11 TFS66178
			//                    // Enclosed the existing code in the if block. If we have subset rules then process
			//                    // the matching dates from the last frequency interval, which are accumulated into
			//                    // the subsetInfo.
			//                    // 
			//                    //this.IncrementByFrequencyHelper( false );
			//                    if ( null == _subsetInfo || !_subsetInfo.StartEnumerating( ) )
			//                    {
			//                        this.IncrementByFrequencyHelper( false );
			//                        for ( int i = 0; i < _rrHasNextMatchStart.Length; i++ )
			//                            _rrHasNextMatchStart[i] = 0;
			//                    }
			//                }
			//                else
			//                    this.RestoreRunningState( );
			//            }
			//        }
			//        else
			//        {
			//            // SSP 3/29/11 TFS66178
			//            // 
			//            // ------------------------------------------------------------------
			//            //this.SaveRunningState( );
			//            int tmpStart = _rrHasNextMatchStart[ruleIndex];
			//            if ( tmpStart < 0 )
			//            {
			//                _cRuleIndex[_cSet] = 1 + ruleIndex;
			//                return;
			//            }

			//            DateRecurrenceRuleBase iiRule = sortedRules[ruleIndex];
			//            this.SaveRunningState( );

			//            _rrHasNextMatch = false;
			//            if ( tmpStart > 0 )
			//                _days.MoveForwardStart( tmpStart );
			//            // ------------------------------------------------------------------

			//            iiRule.Apply( this );
			//            _cRuleIndex[_cSet] = 1 + ruleIndex;

			//            // SSP 3/29/11 TFS66178
			//            // 
			//            _rrHasNextMatchStart[ruleIndex] = _rrHasNextMatch ? 1 + _days.Start : -1;
			//            if ( _rrHasNextMatch )
			//                _rrSetHasNextMatch[_cSet] = true;

			//            if ( _days.IsEmpty )
			//            {
			//                this.RestoreRunningState( );
			//            }
			//            else if ( isLeafSet )
			//            {
			//                this.ProcessMatch( );
			//                this.RestoreRunningState( );
			//            }
			//            else
			//                _cSet++;
			//        }
			//    }
			//    else
			//    {
			//        this.ProcessMatch( );
			//        this.IncrementByFrequencyHelper( false );
			//    }
			//}

			private void ProcessMatch( )
			{
				// SSP 4/4/11 TFS66178
				// 
				// --------------------------------------------------------------------------------------------
				if ( !_isDone )
				{
					// If we are processing matches accumulated by subsetInfo then use the date from it.
					// 
					if ( null != _subsetInfo && _subsetInfo._isEnumerating )
					{
						_date = _subsetInfo._currentMatchDate;
					}
					else if ( !_days.IsEmpty )
					{
						// Initialize the _date based on the _year and _days.
						// 
						this.InitMatchingDate( );

						// If we have subset rules, then accumulate matching dates within a frequency interval.
						// 
						if ( null != _subsetInfo && _subsetInfo.AccumulateMatch( _date ) )
							return;
					}
					else
						return;

					// Initialize _isDone and _isMatch based on recurrence constraints (until or count recurrence rules).
					// 
					if ( _date >= _cache._startDate && _date <= _cache._until )
					{
						_isMatch = true;
						_previousMatchCount++;
					}

					if ( ScheduleUtilities.MaxReached( _cache._maxMatchCount, _previousMatchCount ) )
						_isDone = true;
				}

				//if ( !_isDone && !_days.IsEmpty )
				//{
				//    this.InitMatchingDate( );

				//    if ( _date >= _cache._startDate && _date <= _cache._until )
				//    {
				//        _isMatch = true;
				//        _previousMatchCount++;
				//    }

				//    if ( ScheduleUtilities.MaxReached( _cache._maxMatchCount, _previousMatchCount ) )
				//        _isDone = true;
				//}
				// --------------------------------------------------------------------------------------------
			}

			internal bool IsMatch
			{
				get
				{
					return _isMatch;
				}
			}

			internal bool IsDone( DateTime maxDate )
			{
				return _isDone || _lastFrequencyDate > maxDate;
			}

			private void SaveRunningState( )
			{
				RunningState ds = new RunningState( );

				ds._days = _days;
				ds._year = _year;
				ds._hour = _hour;
				ds._minute = _minute;
				ds._second = _second;

				_runningStates.Push( ds );
			}

			private void RestoreRunningState( )
			{
				RunningState ds = _runningStates.Pop( );

				_days = ds._days;
				_year = ds._year;
				_hour = ds._hour;
				_minute = ds._minute;
				_second = ds._second;
			}

			internal DateTime CalcInitialDate( DateTime generateStartingFromDate )
			{
				DateTime date = _cache._startDate;

				switch ( _cache._recurrence.Frequency )
				{
					case DateRecurrenceFrequency.Yearly:
						date = new DateTime( date.Year, 1, 1 );
						break;
					case DateRecurrenceFrequency.Monthly:
						date = new DateTime( date.Year, date.Month, 1 );
						break;
					case DateRecurrenceFrequency.Weekly:
						date = _cache._calendar.GetFirstDayOfWeekForDate( date );
						break;
					case DateRecurrenceFrequency.Daily:
						date = date.Date;
						break;
					case DateRecurrenceFrequency.Hourly:
						date = new DateTime( date.Year, date.Month, date.Day, date.Hour, 0, 0 );
						break;
					case DateRecurrenceFrequency.Minutely:
						date = new DateTime( date.Year, date.Month, date.Day, date.Hour, date.Minute, 0 );
						break;
					case DateRecurrenceFrequency.Secondly:
						date = new DateTime( date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second );
						break;
					default:
						Debug.Assert( false );
						break;
				}

				date = this.ReachFrequencyDate( date, generateStartingFromDate );

				return date;
			}

			/// <summary>
			/// Increments the specified frequency date to reach a frequency interval that contains
			/// the target date. The returned date will be smaller than the target date.
			/// </summary>
			/// <param name="date"></param>
			/// <param name="target"></param>
			/// <returns></returns>
			private DateTime ReachFrequencyDate( DateTime date, DateTime target )
			{
				DateRecurrenceFrequency frequency = _cache._recurrence.Frequency;
				int interval = _cache._recurrence.Interval;

				// If the recurrence is restricted by number of times (recurrence Count property) then
				// we can't advance the date from the start date unless we can calculate the number of
				// times it would have occurred between the start date and the date to which we advance
				// the generation process.
				// 
				if ( _cache._maxMatchCount > 0 )
				{
					if ( 0 == _cache._fixedMatchCountPerInterval )
						return date;
				}

				int totalAppliedInterval = 0;
				DateTime newDate = date;
				while ( newDate < target )
				{
					int iiInterval = interval;
					DateTime ii = newDate;
					int iiAppliedInterval = 0;

					while ( true )
					{
						ii = _cache.AddToDate( ii, frequency, iiInterval );
						if ( ii < target )
						{
							newDate = ii;
							iiAppliedInterval += iiInterval;
							iiInterval *= 2;
						}
						else
						{
							break;
						}
					}

					totalAppliedInterval += iiAppliedInterval;

					// If the while loop above was a NOOP then break out. We've reached
					// max grequency date where the associated date range interval contains
					// the target date.
					// 
					if ( 0 == iiAppliedInterval )
						break;
				}

				if ( _cache._fixedMatchCountPerInterval > 0 )
				{
					_previousMatchCount = totalAppliedInterval * _cache._fixedMatchCountPerInterval;

					if ( ScheduleUtilities.MaxReached( _cache._maxMatchCount, _previousMatchCount ) )
						_isDone = true;
				}

				return newDate;
			}

			internal void IncrementByFrequencyHelper( bool initial )
			{
				if ( !initial )
					_lastFrequencyDate = _cache.AddToDate( _lastFrequencyDate, _cache._recurrence.Frequency, _cache._recurrence.Interval );

				if ( _lastFrequencyDate >= _cache._until )
				{
					_isDone = true;
					return;
				}

				DateTime date = _lastFrequencyDate;
				_year = date.Year;
				_hour = -1;
				_minute = -1;
				_second = -1;

				int dayOffset = _cache.GetDayOfYear( date );
				_days = new Range( dayOffset, dayOffset );

				switch ( _cache._recurrence.Frequency )
				{
					case DateRecurrenceFrequency.Yearly:
						_days.Expand( _cache.GetDaysInYear( _year ) - 1 );
						break;
					case DateRecurrenceFrequency.Monthly:
						_days.Expand( _cache.GetDaysInMonth( _year, date.Month ) - 1 );
						break;
					case DateRecurrenceFrequency.Weekly:
						_days.Expand( 6 );
						break;
					case DateRecurrenceFrequency.Daily:
						break;
					case DateRecurrenceFrequency.Hourly:
						_hour = date.Hour;
						break;
					case DateRecurrenceFrequency.Minutely:
						_hour = date.Hour;
						_minute = date.Minute;
						break;
					case DateRecurrenceFrequency.Secondly:
						_hour = date.Hour;
						_minute = date.Minute;
						_second = date.Second;
						break;
				}
			}

			internal void IntersectMonth( int month )
			{
				int daysInMonth = _cache.GetDaysInMonth( _year, month );
				int monthStart = _cache.GetDayOfYear( new DateTime( _year, month, 1 ) );

				Range monthRange = new Range( monthStart, monthStart + daysInMonth - 1 );
				_days.Intersect( monthRange );
			}

			internal void IntersectWeek( int week )
			{
				Range daysInWeek = _cache.GetWeekRange( _year, week );
				_days.Intersect( daysInWeek );
			}

			internal void IntersectDayOfYear( int dayOfYear )
			{
				if ( dayOfYear > 0 )
				{
					_days.Intersect( new Range( dayOfYear, dayOfYear ) );
				}
				else if ( dayOfYear < 0 )
				{
					int totalDays = _cache.GetDaysInYear( _year );
					int t = totalDays + dayOfYear + 1;
					_days.Intersect( new Range( t, t ) );
				}
				else
				{
					Debug.Assert( false );
					_days.Intersect( Range.Empty );
				}
			}

			internal void IntersectDayOfMonth( int dayOfMonth )
			{
				int month = _cache.GetMonth( _year, _days.Start );
				Range range = month > 0 ? _cache.GetMonthRange( _year, month ) : Range.Empty;

				int dayOfYear = range.Start + dayOfMonth - 1;

				// SSP 3/29/11 TFS66178
				// If the frequency is yearly and dayOfMonth rule is the only rule, then match
				// it in every month of the year.
				// 
				int nextMonthStart = 1 + range.End;
				_rrHasNextMatch = nextMonthStart + dayOfMonth - 1 <= _days.End;

				_days.Intersect( dayOfYear );
			}

			internal void IntersectDayOfWeek( DayOfWeek dayOfWeek, int relativePosition )
			{
				DateTime dt = _cache.GetDate( _year, _days.Start );

				// Value of 0 for relativePosition means it's not relative position.
				// 
				if ( 0 == relativePosition )
				{
					int val = this.GetRelativeDayOfWeek( _days, dayOfWeek, 0 );
					
					// SSP 3/29/11 TFS66178
					// If we have a DayOfWeek rule without position, then it's said to occurr multiple
					// times in the month. Set the _rrHasNextMatch to true indicate that.
					// 
					_rrHasNextMatch = 7 + val <= _days.End;

					_days.Intersect( val );
				}
				else
				{
					if ( _cache._hasMonthlyRule )
					{
						int month = _cache.GetMonth( dt );
						if ( month < 0 )
						{
							_days.Intersect( -1 );
							return;
						}

						Range monthRange = _cache.GetMonthRange( _year, month );
						int val = this.GetRelativeDayOfWeek( monthRange, dayOfWeek, relativePosition );
						_days.Intersect( val );
					}
					else if ( _cache._hasYearlyRule )
					{
						Range yearRange = new Range( 1, _cache.GetDaysInYear( _year ) );
						int val = this.GetRelativeDayOfWeek( yearRange, dayOfWeek, relativePosition );
						_days.Intersect( val );
					}
					else
					{
						
						_days.Intersect( -1 );
						_cache._invalidRecurrenceRuleError = ScheduleUtilities.CreateErrorFromId(dayOfWeek, "LE_InvalidDayOfWeekRule");// "Has invalid relative day of week rule. It's only valid in a monthly or yearly rule or frequency."
					}
				}
			}

			internal int GetRelativeDayOfWeek( Range range, DayOfWeek dayOfWeek, int relativePos )
			{
				DateTime dt = _cache.GetDate( _year, range.Start );
				DayOfWeek ii = _cache.GetDayOfWeek( dt );

				int d = (int)dayOfWeek - (int)ii;
				if ( d < 0 )
					d += 7;

				int start = range.Start + d;

				if ( relativePos > 0 )
				{
					start += 7 * ( relativePos - 1 );
				}
				else if ( relativePos < 0 )
				{
					relativePos = -relativePos;

					int end = range.End;

					int weekCount = 1 + ( end - start ) / 7;

					start += 7 * ( weekCount - relativePos );
				}

				//Debug.Assert( range.Contains( start ) );
				return start;
			}

			internal void InitMatchingDate( )
			{
				DateTime fillDate = _cache._startDate;

				DateTime date = new DateTime( _year, 1, 1,
					_hour >= 0 ? _hour : fillDate.Hour,
					_minute >= 0 ? _minute : fillDate.Minute,
					_second >= 0 ? _second : fillDate.Second
				);

				int delta = _days.Start - 1;

				if ( _days.Length > 1 )
				{
					switch ( _cache._leafRuleUnit )
					{
						case DateRecurrenceFrequency.Yearly:
							{
								int month = _cache.GetMonth( fillDate );
								int monthDay = _cache.GetDayOfMonth( fillDate );

								int maxDays = _cache.GetDaysInMonth( _year, month );
								int dayOfYear = _cache.GetDayOfYear( new DateTime( _year, month, Math.Min( monthDay, maxDays ) ) );
								if ( _days.Contains( dayOfYear ) )
									delta = dayOfYear - 1;
								else
									Debug.Assert( false );
							}
							break;
						case DateRecurrenceFrequency.Monthly:
							{
								int month = _cache.GetMonth( date.AddDays( delta ) );
								int monthDay = _cache.GetDayOfMonth( fillDate );

								int maxDays = _cache.GetDaysInMonth( _year, month );
								int dayOfYear = _cache.GetDayOfYear( new DateTime( _year, month, Math.Min( monthDay, maxDays ) ) );
								if ( dayOfYear >= _days.Start )
									delta = dayOfYear - 1;
								else
									Debug.Assert( false );
							}
							break;
						case DateRecurrenceFrequency.Weekly:
							{
								int dayOfWeek = _cache.GetDayOfWeekNumber( fillDate );
								DateTime weekStart = _cache._calendar.GetFirstDayOfWeekForDate( date.AddDays( delta ) );

								DateTime tmp = _cache.AddDays( weekStart, dayOfWeek );
								int dayOfYear = _cache.GetDayOfYear( tmp );
								if ( _days.Contains( dayOfYear ) )
									delta = dayOfYear - 1;
								// Take into account overflowing into the next year.
								// 
								else if ( 1 + _year == tmp.Year && dayOfYear < _days.Start
									&& _cache.GetDateRange( date, _days ).Contains( tmp ) )
								{
									date = tmp;
									delta = 0;
								}
								else
									Debug.Assert( false );
							}
							break;
					}
				}

				_date = _cache.AddDays( date, delta );
			}
		}

		#endregion // State Class

		#region RecurrenceCalendarHelper class
		internal class RecurrenceCalendarHelper : CalendarHelper
		{
			#region Member Vars

			internal DayOfWeek _firstDayOfWeek;
			private const System.Globalization.CalendarWeekRule WeekRule = System.Globalization.CalendarWeekRule.FirstFourDayWeek;

			#endregion // Member Vars

			#region Constructor
			internal RecurrenceCalendarHelper()
				: base(System.Globalization.CultureInfo.InvariantCulture)
			{
			}
			#endregion // Constructor

			#region Methods

			internal int GetDayOfWeekNumber(DateTime date)
			{
				return GetDayOfWeekNumber(date, DayOfWeekFlags.None, _firstDayOfWeek);
			}

			internal DateTime GetFirstDayOfWeekForDate(DateTime date)
			{
				int additionalOffset;
				return GetFirstDayOfWeekForDate(date, _firstDayOfWeek, out additionalOffset);
			}

			internal DateTime GetFirstWeekOfYearDate(int year)
			{
				int additionalOffset;
				return GetFirstWeekOfYearDate(year, null, WeekRule, _firstDayOfWeek, this.Calendar, out additionalOffset);
			}

			internal int GetWeekNumberForDate(DateTime date, out int weekYear)
			{
				return GetWeekNumberForDate(date, WeekRule, _firstDayOfWeek, out weekYear);
			}
			
			#endregion // Methods

		} 
		#endregion // RecurrenceCalendarHelper class

		#endregion // Nested Data Structures

		#region Member Vars

		internal DateRecurrence _recurrence;

		/// <summary>
		/// If 0 then no count limit.
		/// </summary>
		private int _maxMatchCount;
		private DateTime _until;
		private DateTime _startDate;

		private DateRecurrenceRuleBase[] _sortedRules;
		private int[] _ruleSets;
		private int _frequencyRuleBoundary;
		private RecurrenceCalendarHelper _calendar;
		private bool _hasMonthlyRule;
		private bool _hasYearlyRule;
		private DataErrorInfo _invalidRecurrenceRuleError;
		private DateRecurrenceFrequency _leafRuleUnit;

		/// <summary>
		/// If rules are as such that there's same fixed number of matches per interval period then
		/// this will be set to that number. Otherwise it will be 0 to indicate that there's no
		/// fixed match per interval.
		/// </summary>
		private int _fixedMatchCountPerInterval;

		// SSP 4/4/11 TFS66178
		// 
		private SubsetRecurrenceRule[] _subsetRules;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="info">Contains information for recurrence generation.</param>
		internal DateRecurrenceCache( RecurrenceInfo info )
		{
			DateRecurrence recurrence = info.Recurrence as DateRecurrence;
			CoreUtilities.ValidateNotNull( recurrence, "Recurrence must be a DateRecurrence instace." );
			_recurrence = recurrence;
			// Truncate to seconds - remove any milliseconds portion.
			// 
			_startDate = ScheduleUtilities.TruncateToSecond( info.StartDateTime );
			_until = _recurrence.Until ?? DateTime.MaxValue;
			_maxMatchCount = _recurrence.Count;

			_calendar = new RecurrenceCalendarHelper();
			_calendar._firstDayOfWeek = recurrence.WeekStart;

			_sortedRules = _recurrence.HasRules
				? ( from ii in _recurrence.Rules where !( ii is SubsetRecurrenceRule ) select ii ).ToArray( )
				: new DateRecurrenceRuleBase[0];

			SortRules( _sortedRules, this );

			this.CalculateRuleSets( );
			_frequencyRuleBoundary = this.GetFrequencyRuleEvaluationPoint( );

			// SSP 4/4/11 TFS66178
			// 
			this.CalculateSubsetInstances( );
		}

		#endregion // Constructor

		#region SortRules

		internal static void SortRules( DateRecurrenceRuleBase[] rules, DateRecurrenceCache cache = null )
		{
			CoreUtilities.SortMergeGeneric<DateRecurrenceRuleBase>( rules,
				CoreUtilities.CreateComparer<DateRecurrenceRuleBase>(
					( x, y ) =>
					{
						return x.GetSortValue( cache ).CompareTo( y.GetSortValue( cache ) );
					}
				)
			);
		} 

		#endregion // SortRules

		#region GetDateRange

		internal DateRange GetDateRange( DateTime start, Range range )
		{
			DateTime dt1 = this.AddDays( start, range.Start );
			DateTime dt2 = this.AddDays( start, range.End );

			return new DateRange( dt1, dt2 );
		}

		#endregion // GetDateRange

		#region CalculateRuleSets

		private void CalculateRuleSets( )
		{
			DateRecurrenceRuleBase[] sortedRules = _sortedRules;
			List<int> ruleSets = new List<int>( );

			Type lastType = null;
			for ( int i = 0; i < sortedRules.Length; i++ )
			{
				DateRecurrenceRuleBase ii = sortedRules[i];
				Type iiType = ii.GetType( );

				if ( ii is MonthOfYearRecurrenceRule )
					_hasMonthlyRule = true;

				if ( lastType != iiType )
				{
					ruleSets.Add( i );
					lastType = iiType;
				}
			}

			
			
			
			
			
			
			_fixedMatchCountPerInterval = 0;
			if ( 0 == sortedRules.Length )
				_fixedMatchCountPerInterval = 1;

			DateRecurrenceFrequency frequency = _recurrence.Frequency;
			if ( DateRecurrenceFrequency.Monthly == frequency )
				_hasMonthlyRule = true;
			else if ( DateRecurrenceFrequency.Yearly == frequency )
				_hasYearlyRule = true;

			_leafRuleUnit = _recurrence.Frequency;
			if ( _sortedRules.Length > 0 )
			{
				DateRecurrenceFrequency tmp = _sortedRules[_sortedRules.Length - 1].Unit;
				if ( tmp > _leafRuleUnit )
					_leafRuleUnit = tmp;
			}

			_ruleSets = ruleSets.ToArray( );
		}

		#endregion // CalculateRuleSets

		#region CalculateSubsetInstances

		private class SubsetInfo
		{
			private List<DateTime> _matches = new List<DateTime>( );
			private int[] _subsetIndeces;
			internal bool _isEnumerating;
			internal DateTime _currentMatchDate;

			// SSP 6/28/11 TFS80109
			// 
			private bool _matchesOutOfOrder;
			private DateTime _matchesOutOfOrder_lastDate;

			internal SubsetInfo( SubsetRecurrenceRule[] subsetRules )
			{
				// SSP 6/28/11 TFS80109
				// Now we can pass null for the rules. Null or an empty array means there are no subset rules.
				// 
				//int[] subsetIndeces = ( from ii in subsetRules select ii.OccurrenceInstance ).ToArray( )
				int[] subsetIndeces = null != subsetRules && subsetRules.Length > 0 
					? ( from ii in subsetRules select ii.OccurrenceInstance ).ToArray( ) 
					: null;

				_subsetIndeces = subsetIndeces;
			}

			public bool AccumulateMatch( DateTime date )
			{
				Debug.Assert( !_isEnumerating );

				_matches.Add( date );

				// SSP 6/28/11 TFS80109 - Optimization
				// Keep a flag indicating whether the _matches array is out of order.
				// 
				if ( !_matchesOutOfOrder )
				{
					_matchesOutOfOrder = _matchesOutOfOrder_lastDate > date;
					_matchesOutOfOrder_lastDate = date;
				}

				return true;
			}

			private int _iterator;
			private List<DateTime> _resultList;

			public bool StartEnumerating( )
			{
				_isEnumerating = false;

				if ( _matches.Count > 0 )
				{
					// SSP 6/28/11 TFS80109
					// Only sort if the matches are out of order based on the new _matchesOutOfOrder flag.
					// 
					//_matches.Sort( );
					if ( _matchesOutOfOrder )
					{
						_matches.Sort( );
						_matchesOutOfOrder = false;
					}

					var arr = _subsetIndeces;
					var matches = _matches;
					// SSP 6/28/11 TFS80109
					// If there are no subset rules then simply use all the matched dates.
					// Added the if block and enclosed the existing code in the else block.
					// 
					if ( null == arr || 0 == arr.Length )
					{
						_resultList = matches;
					}
					else
					{
						_resultList = new List<DateTime>( );
						for ( int i = 0; i < arr.Length; i++ )
						{
							int index = arr[i];
							if ( index > 0 )
							{
								if ( index <= matches.Count )
									_resultList.Add( matches[index - 1] );
							}
							else if ( index < 0 )
							{
								index = matches.Count + index;
								if ( index >= 0 )
									_resultList.Add( matches[index] );
							}
						}
					}

					_iterator = -1;
					_isEnumerating = _resultList.Count > 0;
				}

				return _isEnumerating;
			}

			public bool MoveNext( )
			{
				if ( ++_iterator < _resultList.Count )
				{
					_currentMatchDate = _resultList[_iterator];
					return true;
				}

				_isEnumerating = false;
				_matches.Clear( );

				// SSP 6/28/11 TFS80109
				// 
				_matchesOutOfOrder = false;
				_matchesOutOfOrder_lastDate = DateTime.MinValue;

				return false;
			}
		}

		// SSP 4/4/11 TFS66178
		// 
		private void CalculateSubsetInstances( )
		{
			var rules = _recurrence.HasRules ? _recurrence.Rules : null;
			_subsetRules = null != rules 
				? ( from ii in rules where ii is SubsetRecurrenceRule select (SubsetRecurrenceRule)ii ).ToArray( )
				: null;
		} 

		#endregion // CalculateSubsetInstances

		#region Generate

		internal IEnumerable<DateTime> Generate( DateRange dateRange )
		{
			return new Enumerable( this, dateRange );
		}

		#endregion // Generate

		#region GetFirstOccurrenceDate

		// SSP 4/11/11 TFS66178
		// 
		internal DateTime GetFirstOccurrenceDate( DateRange dateRange )
		{
			

			const int MAX_DAYS = 367;
			dateRange.End = dateRange.Start.AddDays( MAX_DAYS );

			return this.Generate( dateRange ).OrderBy( ii => ii ).FirstOrDefault( );
		}

		#endregion // GetFirstOccurrenceDate

		#region GetFrequencyRuleEvaluationPoint

		private int GetFrequencyRuleEvaluationPoint( )
		{
			DateRecurrenceRuleBase[] rules = _sortedRules;
			for ( int i = 0; i < rules.Length; i++ )
			{
				if ( rules[i].Unit > _recurrence.Frequency )
					return i;
			}

			return rules.Length;
		}

		#endregion // GetFrequencyRuleEvaluationPoint

		#region CalculateOutOfOrderDateGenerationBoundary

		// SSP 4/15/11 TFS66178
		
		private void CalculateOutOfOrderDateGenerationBoundary( )
		{
			var sets = _ruleSets;
			var rules = _sortedRules;

			for ( int i = 0; i < sets.Length; i++ )
			{
				int setStart = i > 1 ? sets[i - 1] : 0;
				int setEnd = sets[i];

				int ruleCount = 1 + setEnd - setStart;
				int relativeRuleCount = 0;

				for ( int r = setStart; r <= setEnd; r++ )
				{
					var rule = rules[r];

					var dm = rule as DayOfMonthRecurrenceRule;
					if ( null != dm && dm.DayOfMonth < 0 )
						relativeRuleCount++;

					var dw = rule as DayOfWeekRecurrenceRule;
					if ( null != dw && dw.RelativePosition < 0 )
						relativeRuleCount++;

					var dy = rule as DayOfYearRecurrenceRule;
					if ( null != dy && dy.DayOfYear < 0 )
						relativeRuleCount++;

					var wy = rule as WeekOfYearRecurrenceRule;
					if ( null != wy && wy.WeekNumber < 0 )
						relativeRuleCount++;
				}
			}
		}

		#endregion // CalculateOutOfOrderDateGenerationBoundary

		#region Date Methods

		#region AddToDate

		internal DateTime AddToDate( DateTime date, DateRecurrenceFrequency unit, int n )
		{
			try
			{
				switch ( unit )
				{
					default:
					case DateRecurrenceFrequency.Yearly:
						Debug.Assert( DateRecurrenceFrequency.Yearly == unit, "Unknown frequency value." );
						return this.AddYears( date, n );
					case DateRecurrenceFrequency.Monthly:
						return this.AddMonths( date, n );
					case DateRecurrenceFrequency.Weekly:
						return this.AddWeeks( date, n );
					case DateRecurrenceFrequency.Daily:
						return this.AddDays( date, n );
					case DateRecurrenceFrequency.Hourly:
						return this.AddHours( date, n );
					case DateRecurrenceFrequency.Minutely:
						return this.AddMinutes( date, n );
					case DateRecurrenceFrequency.Secondly:
						return this.AddSeconds( date, n );
				}
			}
			catch
			{
				return DateTime.MaxValue;
			}
		}

		#endregion // AddToDate

		#region AddYears

		internal DateTime AddYears( DateTime date, int years )
		{
			return _calendar.AddYears( date, years );
		}

		#endregion // AddYears

		#region AddMonths

		internal DateTime AddMonths( DateTime date, int months )
		{
			return _calendar.AddMonths( date, months );
		}

		#endregion // AddMonths

		#region AddDays

		internal DateTime AddDays( DateTime date, int days )
		{
			return _calendar.AddDays( date, days );
		}

		#endregion // AddDays

		#region AddWeeks

		internal DateTime AddWeeks( DateTime date, int weeks )
		{
			return _calendar.AddWeeks( date, weeks );
		}

		#endregion // AddWeeks

		#region AddHours

		internal DateTime AddHours( DateTime date, int hours )
		{
			return _calendar.AddHours( date, hours );
		}

		#endregion // AddHours

		#region AddMinutes

		internal DateTime AddMinutes( DateTime date, int minutes )
		{
			return _calendar.AddMinutes( date, minutes );
		}

		#endregion // AddMinutes

		#region AddSeconds

		internal DateTime AddSeconds( DateTime date, int seconds )
		{
			return _calendar.AddSeconds( date, seconds );
		}

		#endregion // AddSeconds

		#region GetDayOfYear

		internal int GetDayOfYear( DateTime date )
		{
			return _calendar.GetDayOfYear( date );
		}

		#endregion // GetDayOfYear

		#region GetDayOfMonth

		internal int GetDayOfMonth( DateTime date )
		{
			return _calendar.GetDayOfMonth( date );
		}

		#endregion // GetDayOfMonth

		#region GetMonth

		internal int GetMonth( DateTime date )
		{
			return _calendar.GetMonthNumber( date );
		}

		internal int GetMonth( int year, int dayOfYear )
		{
			DateTime dt = this.GetDate( year, dayOfYear );

			return this.GetYear( dt ) == year ? this.GetMonth( dt ) : -1;
		}

		#endregion // GetMonth

		#region GetMonthRange

		internal Range GetMonthRange( int year, int month )
		{
			int daysInMonth = this.GetDaysInMonth( year, month );
			int monthStart = this.GetDayOfYear( new DateTime( year, month, 1 ) );

			return new Range( monthStart, monthStart + daysInMonth - 1 );
		}

		#endregion // GetMonthRange

		#region GetLastWeekOfYearDate

		internal DateTime GetLastWeekOfYearDate( int year )
		{
			DateTime dt = new DateTime( year, 12, 31 );

			int weekYear;
			int n = _calendar.GetWeekNumberForDate( dt, out weekYear );
			if ( year != weekYear )
			{
				dt = this.AddDays( dt, -7 );
				n = _calendar.GetWeekNumberForDate( dt, out weekYear );
				Debug.Assert( year == weekYear );
			}

			DateTime weekStart = _calendar.GetFirstWeekOfYearDate( year );
			weekStart = this.AddWeeks( weekStart, n - 1 );

			return weekStart;
		}

		#endregion // GetLastWeekOfYearDate

		#region GetWeekRange

		internal Range GetWeekRange( int year, int week )
		{
			DateTime date = _calendar.GetFirstWeekOfYearDate( year );
			DateTime weekStart;

			if ( week > 0 )
			{
				weekStart = this.AddWeeks( date, week - 1 );
			}
			else if ( week < 0 )
			{
				weekStart = this.GetLastWeekOfYearDate( year );
				this.AddWeeks( weekStart, week + 1 );
			}
			else
			{
				Debug.Assert( false, "Invalid week." );
				return Range.Empty;
			}

			int d = this.GetDayOfYear( weekStart );
			return new Range( d, d + 6 );
		}

		#endregion // GetWeekRange

		#region GetDayOfWeekNumber

		internal int GetDayOfWeekNumber( DateTime date )
		{
			return _calendar.GetDayOfWeekNumber( date );
		}

		#endregion // GetDayOfWeekNumber

		#region GetDayOfWeek

		internal DayOfWeek GetDayOfWeek( DateTime date )
		{
			return _calendar.GetDayOfWeek( date );
		}

		#endregion // GetDayOfWeek

		#region GetDate

		internal DateTime GetDate( int year, int dayOfYear )
		{
			DateTime dt = new DateTime( year, 1, 1 );

			return this.AddDays( dt, dayOfYear - 1 );
		}

		#endregion // GetDate

		#region GetDaysInYear

		internal int GetDaysInYear( int year )
		{
			return _calendar.GetDaysInYear( new DateTime( year, 1, 1 ) );
		}

		#endregion // GetDaysInYear

		#region GetDaysInMonth

		internal int GetDaysInMonth( int year, int month )
		{
			return _calendar.GetDaysInMonth( new DateTime( year, month, 1 ) );
		}

		#endregion // GetDaysInMonth

		#region GetYear

		internal int GetYear( DateTime date )
		{
			return _calendar.GetYear( date );
		} 

		#endregion // GetYear

		#endregion // Date Methods
	}

	#region DateRecurrenceParser Class

	/// <summary>
	/// A class used for serializing and deserializing date recurrence into a string that's conformant to iCalendar RRULE format.
	/// </summary>
	internal class DateRecurrenceParser
	{
		#region Constants

		private const string FREQ = "FREQ";
		private const string INTERVAL = "INTERVAL";
		private const string COUNT = "COUNT";
		private const string UNTIL = "UNTIL";
		private const string WKST = "WKST";
		private const char SEMICOLON = ';';
		private const char COMMA = ',';
		private const char EQUAL = '=';
		private const string ICAL_DATE_FORMAT = @"yyyyMMdd";
		private const string ICAL_DATETIME_FORMAT = @"yyyyMMdd\THHmmss";

		#endregion // Constants

		#region Member Vars

		private object[] _table;
		private const int STEP = 4;

		private DataErrorInfo _error;
		private bool _parsing;
		private string _parseString;
		private DateRecurrence _recurrence;
		private StringBuilder _sb = new StringBuilder( );

		private static string[] ABBREVIATED_DAYS = new string[] { "SU", "MO", "TU", "WE", "TH", "FR", "SA" };

		private static object[] _frequencyTable = new object[]
		{
			DateRecurrenceFrequency.Yearly, "YEARLY",
			DateRecurrenceFrequency.Monthly, "MONTHLY",
			DateRecurrenceFrequency.Weekly, "WEEKLY",
			DateRecurrenceFrequency.Daily, "DAILY",
			DateRecurrenceFrequency.Hourly, "HOURLY",
			DateRecurrenceFrequency.Minutely, "MINUTELY",
			DateRecurrenceFrequency.Secondly, "SECONDLY",
		};

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="recurrence"></param>
		private DateRecurrenceParser( DateRecurrence recurrence )
		{
			_recurrence = recurrence;

			_table = new object[]
			{
				typeof( MonthOfYearRecurrenceRule ), "BYMONTH", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((MonthOfYearRecurrenceRule)ii).Month ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new MonthOfYearRecurrenceRule( ii ) ),

				typeof( WeekOfYearRecurrenceRule ), "BYWEEKNO", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((WeekOfYearRecurrenceRule)ii).WeekNumber ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new WeekOfYearRecurrenceRule( ii ) ),

				typeof( DayOfYearRecurrenceRule ), "BYYEARDAY", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((DayOfYearRecurrenceRule)ii).DayOfYear ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new DayOfYearRecurrenceRule( ii ) ),
			
				typeof( DayOfMonthRecurrenceRule ), "BYMONTHDAY", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((DayOfMonthRecurrenceRule)ii).DayOfMonth ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new DayOfMonthRecurrenceRule( ii ) ),
			
				typeof( DayOfWeekRecurrenceRule ), "BYDAY", 
				(Func<DateRecurrenceRuleBase, string>)( ii => 
				{ 
					DayOfWeekRecurrenceRule rr = (DayOfWeekRecurrenceRule)ii; 
					return ( 0 != rr.RelativePosition ? ToString( rr.RelativePosition ) : string.Empty )
						+ GetDayName( rr.Day );
				} ),				
				(Func<string, DateRecurrenceRuleBase>)( ParseDayOfWeekRule ),
			
				typeof( HourRecurrenceRule ), "BYHOUR", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((HourRecurrenceRule)ii).Hour ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new HourRecurrenceRule( ii ) ),
			
				typeof( MinuteRecurrenceRule ), "BYMINUTE", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((MinuteRecurrenceRule)ii).Minute ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new MinuteRecurrenceRule( ii ) ),
			
				typeof( SecondRecurrenceRule ), "BYSECOND", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((SecondRecurrenceRule)ii).Second ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new SecondRecurrenceRule( ii ) ),
			
				typeof( SubsetRecurrenceRule ), "BYSETPOS", 
				(Func<DateRecurrenceRuleBase, int>)( ii => ((SubsetRecurrenceRule)ii).OccurrenceInstance ), 
				(Func<int, DateRecurrenceRuleBase>)( ii => new SubsetRecurrenceRule( ii ) ),
			};
		}

		#endregion // Constructor

		#region Private Methods

		#region Append

		private void Append( string name, string value, bool prependSemicolon )
		{
			if ( prependSemicolon )
				_sb.Append( SEMICOLON );

			_sb.Append( name ).Append( EQUAL ).Append( value );
		}

		#endregion // Append

		#region GetDayName

		private static string GetDayName( DayOfWeek day )
		{
			return ABBREVIATED_DAYS[(int)day];
		}

		#endregion // GetDayName

		#region GetRuleName

		private string GetRuleName( DateRecurrenceRuleBase rule )
		{
			Type ruleType = rule.GetType( );

			object[] table = _table;
			for ( int i = 0; i < table.Length; i += STEP )
			{
				if ( ruleType == (Type)table[i] )
					return (string)table[1 + i];
			}

			Debug.Assert( false );
			return null;
		}

		#endregion // GetRuleName

		#region GetRuleValue

		private string GetRuleValue( DateRecurrenceRuleBase rule )
		{
			Type ruleType = rule.GetType( );

			object[] table = _table;
			for ( int i = 0; i < table.Length; i += STEP )
			{
				if ( ruleType == (Type)table[i] )
				{
					object converter = table[2 + i];
					Func<DateRecurrenceRuleBase, int> toIntConverter = converter as Func<DateRecurrenceRuleBase, int>;
					if ( null != toIntConverter )
					{
						int value = toIntConverter( rule );
						return ToString( value );
					}
					else
					{
						Func<DateRecurrenceRuleBase, string> toStrConveter = (Func<DateRecurrenceRuleBase, string>)converter;
						return toStrConveter( rule );
					}
				}
			}

			this.OnError( "Unknown type of rule", rule.GetType( ).Name );
			return null;
		}

		#endregion // GetRuleValue

		#region OnError

		private void OnError( string message, string context = null )
		{
			string resId;
			if ( _parsing )
				resId = "LE_RecurrenceParsing";
			else
				resId = "LE_RecurrenceSerializing";
			
			_error = ScheduleUtilities.CreateErrorFromId(null, resId, context );
		}

		#endregion // OnError

		#region ParseDayOfWeek

		private DayOfWeek ParseDayOfWeek( string s )
		{
			int dayIndex = Array.IndexOf( ABBREVIATED_DAYS, s );
			if ( dayIndex >= 0 )
				return (DayOfWeek)dayIndex;

			this.OnError( "Invalid day of week.", s );
			return default( DayOfWeek );
		}

		#endregion // ParseDayOfWeek

		#region ParseDayOfWeekRule

		private DateRecurrenceRuleBase ParseDayOfWeekRule( string s )
		{
			if ( s.Length >= 2 )
			{
				string day = s.Substring( s.Length - 2, 2 );

				string numberStr = s.Substring( 0, s.Length - 2 );

				int number = 0;
				bool error = false;
				if ( numberStr.Length > 0 )
				{
					if ( !int.TryParse( numberStr, out number ) )
						error = true;
				}

				if ( !error )
					return new DayOfWeekRecurrenceRule( ParseDayOfWeek( day ), number );
			}

			throw new ArgumentOutOfRangeException(ScheduleUtilities.GetString("LE_BadDayOfWeekRule", s));// "Invalid day of week rule: "
		}

		#endregion // ParseDayOfWeekRule

		#region ParseFrequency

		private DateRecurrenceFrequency ParseFrequency( string s )
		{
			object[] table = _frequencyTable;

			s = s.ToUpperInvariant( );

			for ( int i = 0; i < table.Length; i += 2 )
			{
				if ( s == (string)table[1 + i] )
					return (DateRecurrenceFrequency)table[i];
			}

			this.OnError( "Invalid frequency.", s );
			return default( DateRecurrenceFrequency );
		}

		#endregion // ParseFrequency

		#region ParseHelper

		private void ParseHelper( string s )
		{
			DateRecurrence recurrence = _recurrence;
			string[] rules = s.Split( SEMICOLON );

			for ( int i = 0; i < rules.Length; i++ )
			{
				string iiStr = rules[i];

				string[] pair = iiStr.Split( EQUAL );
				if ( 2 == pair.Length )
				{
					string name = pair[0].ToUpperInvariant( );
					string value = pair[1];

					switch ( name )
					{
						case FREQ:
							recurrence.Frequency = ParseFrequency( value );
							break;
						case INTERVAL:
							recurrence.Interval = ToInt( value );
							break;
						case COUNT:
							recurrence.Count = ToInt( value );
							break;
						case UNTIL:
							recurrence.Until = ParseICalendarDate( value );
							break;
						case WKST:
							recurrence.WeekStart = ParseDayOfWeek( value );
							break;
						default:
							ParseRule( name, value, recurrence.Rules );
							break;
					}
				}
				else
				{
					this.OnError( "Invalid recurrence rule", iiStr );
				}
			}
		}

		#endregion // ParseHelper

		#region ParseICalendarDate

		private DateTime ParseICalendarDate( string s )
		{
			DateTime dt;
			string format = s.Length > 8 ? ICAL_DATETIME_FORMAT : ICAL_DATE_FORMAT;

			// SSP 3/3/2011 TFS66981
			// Since we append 'Z' at the end of the date in accordance with iCal spec, we need to remove that character
			// otherwise the parse call below will fail.
			// 
			int lastCharIndex = s.Length - 1;
			if ( 'Z' == s[lastCharIndex] )
				s = s.Substring( 0, lastCharIndex );

			if ( !DateTime.TryParseExact( s, ICAL_DATETIME_FORMAT, ScheduleUtilities.ParseCulture, System.Globalization.DateTimeStyles.None, out dt ) )
				OnError( "Invalid date value", s );

			return dt;
		}

		#endregion // ParseICalendarDate

		#region ParseRule

		private void ParseRule( string name, string valueStr, IList<DateRecurrenceRuleBase> rules )
		{
			object[] table = _table;
			for ( int i = 0; i < table.Length; i += STEP )
			{
				if ( name == (string)table[1 + i] )
				{
					string[] values = valueStr.Split( COMMA );
					if ( null != values )
					{
						for ( int j = 0; j < values.Length; j++ )
						{
							string iiValue = values[j];
							DateRecurrenceRuleBase rule;

							object converter = table[3 + i];
							Func<int, DateRecurrenceRuleBase> intToRuleConverter = converter as Func<int, DateRecurrenceRuleBase>;
							if ( null != intToRuleConverter )
							{
								rule = intToRuleConverter( ToInt( iiValue ) );
							}
							else
							{
								Func<string, DateRecurrenceRuleBase> strToRuleConverter = (Func<string, DateRecurrenceRuleBase>)converter;
								rule = strToRuleConverter( iiValue );
							}

							if ( null != rule )
								rules.Add( rule );
							else
								this.OnError( string.Format( "Incorrect value of {1} for rule {0}.", name, valueStr ) );
						}

						return;
					}
				}
			}

			this.OnError( "Unknown rule", name );
		}

		#endregion // ParseRule

		#region SerializeToString

		private void SerializeToString( )
		{
			StringBuilder sb = _sb;
			DateRecurrence recurrence = _recurrence;

			Append( FREQ, ToString( recurrence.Frequency ), false );
			Append( INTERVAL, ToString( recurrence.Interval ), true );

			int count = recurrence.Count;
			if ( count > 0 )
				Append( COUNT, ToString( count ), true );

			DateTime? until = recurrence.Until;
			if ( until.HasValue )
				Append( UNTIL, ToICalendarString( until.Value, true, false ), true );

			Append( WKST, GetDayName( recurrence.WeekStart ), true );

			if ( _recurrence.HasRules )
			{
				DateRecurrenceRuleBase[] rules = _recurrence.Rules.ToArray( );
				DateRecurrenceCache.SortRules( rules );

				Type lastType = null;
				for ( int i = 0; i < rules.Length; i++ )
				{
					DateRecurrenceRuleBase rule = rules[i];
					Type ruleType = rule.GetType( );

					if ( ruleType != lastType )
					{
						lastType = ruleType;
						sb.Append( SEMICOLON ).Append( this.GetRuleName( rule ) ).Append( EQUAL );
					}
					else
					{
						sb.Append( COMMA );
					}

					sb.Append( this.GetRuleValue( rule ) );
				}
			}
		}

		#endregion // SerializeToString

		#region SpaceHelper

		/// <summary>
		/// Appends space if the sb doesn't already end with a space.
		/// </summary>
		/// <param name="sb"></param>
		/// <returns></returns>
		private static StringBuilder SpaceHelper( StringBuilder sb )
		{
			int len = sb.Length;
			if ( len > 0 && !char.IsWhiteSpace( sb[len - 1] ) )
				sb.Append( " " );

			return sb;
		}

		#endregion // SpaceHelper

		#region ToInt

		private int ToInt( string s )
		{
			int r;

			if ( !int.TryParse( s, out r ) )
				this.OnError( "Invalid integer value" + s );

			return r;
		}

		#endregion // ToInt

		#region ToString

		private string ToString( DateRecurrenceFrequency frequency )
		{
			object[] table = _frequencyTable;

			for ( int i = 0; i < table.Length; i += 2 )
			{
				if ( frequency == (DateRecurrenceFrequency)table[i] )
					return (string)table[1 + i];
			}

			this.OnError( "Invalid frequency.", frequency.ToString( ) );
			return string.Empty;
		}

		private static string ToString( int i )
		{
			return i.ToString( ScheduleUtilities.ParseCulture );
		}

		#endregion // ToString

		#endregion // Private Methods

		#region Internal Methods

		#region GetConverter

		internal static IValueConverter GetConverter( )
		{
			Func<object, Type, object, CultureInfo, object> converter = ( value, targetType, parameter, culture ) =>
			{
				DataErrorInfo error;

				bool isTargetTypeRecurrence = typeof( RecurrenceBase ).IsAssignableFrom( targetType );

				DateRecurrence recurrence = value as DateRecurrence;
				if ( null != recurrence )
				{
					if ( isTargetTypeRecurrence )
						return recurrence;

					if ( typeof( string ) == targetType )
					{
						string ret = SerializeToString( recurrence, out error );
						return (object)error ?? ret;
					}
				}

				string str = value as string;
				if ( null != str )
				{
					if ( typeof( string ) == targetType )
						return str;

					if ( isTargetTypeRecurrence )
					{
						recurrence = Parse( str, out error );

						return (object)error ?? recurrence;
					}
				}

				return ScheduleUtilities.IsValueEmpty(value) ? null : ScheduleUtilities.CreateErrorFromId(value, "LE_UnknownTargetType", targetType.Name);// "Unknown target type: {0}"
			};

			return new DelegateValueConverter( converter, converter );
		}

		#endregion // GetConverter

		#region Parse

		internal static DateRecurrence Parse( string s, out DataErrorInfo error )
		{
			error = null;

			if ( string.IsNullOrEmpty( s ) )
				return null;

			DateRecurrenceParser parser = new DateRecurrenceParser( new DateRecurrence( ) );
			parser._parsing = true;
			parser._parseString = s;
			parser.ParseHelper( s );

			error = parser._error;

			return null == error ? parser._recurrence : null;
		}

		#endregion // Parse

		#region SerializeToString

		internal static string SerializeToString( DateRecurrence recurrence, out DataErrorInfo error )
		{
			DateRecurrenceParser parser = new DateRecurrenceParser( recurrence );
			parser._sb = new StringBuilder( );

			parser.SerializeToString( );

			error = parser._error;

			return null == error ? parser._sb.ToString( ) : null;
		}

		#endregion // SerializeToString

		#region ToDisplayStringHelper

		internal static string ToDisplayStringHelper( DateRecurrence recurrence, DateTime? startDate = null )
		{
			DateTimeFormatInfo format = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;

			Func<int, string> calcMonthName = ii =>
			{
				return format.MonthNames[ii - 1];
			};

			Func<int, string> calcNthString = ii =>
			{
				string[] arr = new string[] { "0th", "1st", "2nd", "3rd" };

				if ( -1 == ii )
					return "last";

				bool isNegative = ii < 0;
				if ( isNegative )
					ii = -ii;

				string ss = ii < arr.Length ? arr[ii] : ii + "th";

				return ss + ( isNegative ? " to the last" : string.Empty );
			};

			Func<DayOfWeek, string> calcDayName = ii =>
			{
				return format.GetDayName( ii );
			};

			Func<DayOfWeekRecurrenceRule, string> calcDayOfWeekRuleStr = rr =>
			{
				return 0 == rr.RelativePosition
					? calcDayName( rr.Day )
					: calcNthString( rr.RelativePosition ) + " " + calcDayName( rr.Day );
			};

			Func<IEnumerable<string>, string, string> createList = ( x, separator ) =>
			{
				StringBuilder tmp = new StringBuilder( );

				int count = x.Count( );
				int i = 0;

				foreach ( string ii in x )
				{
					if ( i > 0 )
					{
						if ( i == count - 1 )
							tmp.Append( ' ' ).Append( separator ).Append( ' ' );
						else
							tmp.Append( ", " );
					}

					tmp.Append( ii );
					i++;
				}

				return tmp.ToString( );
			};

			string[] frequencyStrings = new string[]
			{
				"year", "month", "week", "day", "hour", "minute", "second"
			};

			StringBuilder sb = new StringBuilder( );
			sb.Append( "Occurs " );
			DateRecurrenceFrequency frequency = recurrence.Frequency;
			int interval = recurrence.Interval;

			if ( interval > 1 )
				sb.Append( "every " ).Append( interval ).Append( " " ).Append( frequencyStrings[(int)frequency] ).Append( "s" );
			else
				sb.Append( frequency.ToString( ) );

			var rules = recurrence.HasRules ? recurrence.Rules : null;
			if ( null != rules )
			{
				IEnumerable<string> months = from ii in rules
											 let rr = ii as MonthOfYearRecurrenceRule
											 where null != rr
											 select calcMonthName( rr.Month );

				IEnumerable<string> monthDays = from ii in rules
												let rr = ii as DayOfMonthRecurrenceRule
												where null != rr
												select calcNthString( rr.DayOfMonth );

				IEnumerable<string> weeks = from ii in rules
											let rr = ii as WeekOfYearRecurrenceRule
											where null != rr
											select calcNthString( rr.WeekNumber );

				IEnumerable<DayOfWeekRecurrenceRule> fixedWeekDayRules = from ii in rules
													let rr = ii as DayOfWeekRecurrenceRule
													where null != rr && 0 == rr.RelativePosition
													select rr;

				const DayOfWeekFlags WEEKEND = DayOfWeekFlags.Saturday | DayOfWeekFlags.Sunday;
				const DayOfWeekFlags WEEKDAYS = DayOfWeekFlags.Monday | DayOfWeekFlags.Tuesday | DayOfWeekFlags.Wednesday | DayOfWeekFlags.Thursday | DayOfWeekFlags.Friday;
				const DayOfWeekFlags ALLDAYS = WEEKDAYS | WEEKEND;

				IEnumerable<string> fixedWeekDays = from rr in fixedWeekDayRules select calcDayName( rr.Day );

				IEnumerable<DayOfWeekRecurrenceRule> relativeWeekDayRules = from ii in rules
													   let rr = ii as DayOfWeekRecurrenceRule
													   where null != rr && 0 != rr.RelativePosition
													   select rr;

				IEnumerable<string> relativeWeekDays = from rr in relativeWeekDayRules select calcDayOfWeekRuleStr( rr );

				IEnumerable<string> daysOfYear = from ii in rules
												 let rr = ii as DayOfYearRecurrenceRule
												 where null != rr
												 select calcNthString( rr.DayOfYear );

				IEnumerable<int> hours = from ii in rules
										 let rr = ii as HourRecurrenceRule
										 where null != rr
										 select rr.Hour;

				IEnumerable<int> minutes = from ii in rules
										   let rr = ii as MinuteRecurrenceRule
										   where null != rr
										   select rr.Minute;

				IEnumerable<int> seconds = from ii in rules
										   let rr = ii as SecondRecurrenceRule
										   where null != rr
										   select rr.Second;

				IEnumerable<int> subsets = from ii in rules
										   let rr = ii as SubsetRecurrenceRule
										   where null != rr
										   select rr.OccurrenceInstance;


				int ccMonths = months.Count( );
				int ccMonthDays = monthDays.Count( );
				int ccWeeks = weeks.Count( );
				int ccFixedWeekDays = fixedWeekDays.Count( );
				int ccRelativeWeekDays = relativeWeekDays.Count( );
				int ccDaysOfYear = daysOfYear.Count( );
				int ccSubsets = subsets.Count( );

				object lastRule = null;
				lastRule = ccMonths > 0 ? months : lastRule;
				lastRule = ccMonthDays > 0 ? monthDays : lastRule;
				lastRule = ccWeeks > 0 ? weeks : lastRule;
				lastRule = ccFixedWeekDays > 0 ? fixedWeekDays : lastRule;
				lastRule = ccRelativeWeekDays > 0 ? relativeWeekDays : lastRule;
				lastRule = ccDaysOfYear > 0 ? daysOfYear : lastRule;

				int total = ccMonths + ccMonthDays + ccWeeks + ccFixedWeekDays + ccRelativeWeekDays + ccDaysOfYear;

				bool subFilterMode = false;
				bool andMode = false;

				if ( total > 0 )
				{
					if ( ccMonths + ccWeeks == total )
						sb.Append( " in " );
					else
						sb.Append( " on " );
				}

				const string AND = "and";
				const string OR = "or";

				if ( ccDaysOfYear > 0 )
				{
					sb.Append( "the " ).Append( createList( daysOfYear, AND ) ).Append( " " );
					sb.Append( ccDaysOfYear > 0 ? "days" : "day" ).Append( " of year" );
					subFilterMode = true;
				}

				if ( ccWeeks > 0 )
				{
					if ( subFilterMode )
						sb.Append( " that falls in" );

					sb.Append( createList( weeks, AND ) ).Append( " of the year" );
					subFilterMode = true;
				}

				if ( ccFixedWeekDays > 0 )
				{
					if ( subFilterMode )
						sb.Append( " that happens to be" );

					var tmpDays = fixedWeekDayRules.Select( ii => ii.Day ).Distinct( );
					bool allWeekdays = 5 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( WEEKDAYS, ii ) );
					bool allWeekendDays = 2 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( WEEKEND, ii ) );
					bool allDays = 7 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( ALLDAYS, ii ) );

					bool useOr = false;
					bool hasSubset = false;
					if ( ccSubsets > 0 && ( lastRule == fixedWeekDays || lastRule == relativeWeekDays ) )
					{
						sb.Append( createList( subsets.Select( calcNthString ), AND ) );
						SpaceHelper( sb );
						useOr = true;
						hasSubset = true;
					}
					else
						sb.Append( "every " );

					if ( allWeekdays )
						sb.Append( "weekday" );
					else if ( allWeekendDays )
						sb.Append( "weekend day" );
					else if ( allDays )
						sb.Append( "day" );
					else
					{
						if ( hasSubset && ccSubsets > 1 )
							sb.Append( "occurrence of " );

						sb.Append( createList( fixedWeekDays, useOr ? OR : AND ) );
					}

					subFilterMode = false;
					andMode = true;
				}

				if ( ccRelativeWeekDays > 0 )
				{
					if ( subFilterMode )
						sb.Append( " that happens to be" );

					if ( andMode )
						sb.Append( " and" );

					var tmpDays = relativeWeekDayRules.Select( ii => ii.Day ).Distinct( ).ToList( );
					bool allWeekdays = 5 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( WEEKDAYS, ii ) );
					bool allWeekendDays = 2 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( WEEKEND, ii ) );
					bool allDays = 7 == tmpDays.Count( ) && tmpDays.All( ii => CalendarHelper.IsSet( ALLDAYS, ii ) );

					var relativeIndeces = from ii in relativeWeekDayRules select ii.RelativePosition;
					int? relativeIndex = null;
					if ( 1 == relativeIndeces.Distinct( ).Count( ) )
						relativeIndex = relativeIndeces.First( );

					bool useOr = false;
					bool hasSubset = false;
					if ( ccSubsets > 0 && lastRule == relativeWeekDays )
					{
						SpaceHelper( sb );
						sb.Append( createList( subsets.Select( calcNthString ), AND ) );
						useOr = true;
						hasSubset = true;
					}

					SpaceHelper( sb );

					bool useShortcut = relativeIndex.HasValue 
						&& ( allWeekdays || allWeekendDays || allDays );

					bool appendRelativeIndex = useShortcut && relativeIndex.HasValue
						&& ( !hasSubset 
							 || subsets.First( ) > 0 && 1 != relativeIndex.Value 
							 || subsets.First( ) < 0 && -1 != relativeIndex.Value );

					if ( hasSubset && ( ccSubsets > 1 || appendRelativeIndex ) )
					{
						sb.Append( "occurrence of " );
						useShortcut = false;
					}

					if ( useShortcut )
					{
						if ( appendRelativeIndex )
						{
							sb.Append( calcNthString( relativeIndex.Value ) );
							SpaceHelper( sb );
						}

						if ( allWeekdays )
							sb.Append( "weekday" );
						else if ( allWeekendDays )
							sb.Append( "weekend day" );
						else if ( allDays )
							sb.Append( "day" );
					}
					else
					{
						sb.Append( createList( relativeWeekDays, useOr ? OR : AND ) );
					}
				}

				if ( ccMonthDays > 0 )
				{
					if ( andMode )
						sb.Append( " and" );

					SpaceHelper( sb ).Append( createList( monthDays, AND ) );
				}

				if ( ccMonths > 0 )
				{
					if ( subFilterMode )
						sb.Append( " that falls in " );
					else if ( ccFixedWeekDays + ccRelativeWeekDays + ccMonthDays > 0 )
						sb.Append( " of " );

					sb.Append( createList( months, AND ) );
				}
				else if ( ccFixedWeekDays + ccRelativeWeekDays + ccMonthDays > 0 )
				{
					if ( frequency == DateRecurrenceFrequency.Weekly )
						SpaceHelper( sb ).Append( "of the week" );
					else if ( frequency == DateRecurrenceFrequency.Monthly )
						SpaceHelper( sb ).Append( "of the month" );
				}

				int ccHours = hours.Count( );
				int ccMinutes = minutes.Count( );
				int ccSeconds = seconds.Count( );
				total = ccHours + ccMinutes + ccSeconds;

				if ( 1 == ccHours && ccMinutes <= 1 && ccSeconds <= 1 && ( startDate.HasValue || 1 == ccMinutes && 1 == ccSeconds ) )
				{
					DateTime t = startDate.HasValue ? startDate.Value : DateTime.Today;

					DateTime dt = new DateTime( t.Year, t.Month, t.Day,
						hours.First( ),
						ccMinutes > 0 ? minutes.First( ) : t.Minute,
						ccSeconds > 0 ? seconds.First( ) : t.Second
					);

					SpaceHelper( sb ).Append( "at " ).Append( dt.ToString( "hh:mm:ss tt" ) );
				}
				else if ( total > 0 )
				{
					if ( ccSeconds > 0 )
						SpaceHelper( sb ).Append( " on " ).Append( createList( seconds.Select( calcNthString ), AND ) ).Append( " " ).Append( ccSeconds > 1 ? "seconds" : "second" );
					else if ( startDate.HasValue )
						SpaceHelper( sb ).Append( frequency >= DateRecurrenceFrequency.Secondly ? "on every second" : "on " + calcNthString( startDate.Value.Second ) + " second" );

					if ( ccMinutes > 0 )
						SpaceHelper( sb ).Append( " on " ).Append( createList( minutes.Select( calcNthString ), AND ) ).Append( " " ).Append( ccMinutes > 1 ? "minutes" : "minute" );
					else if ( startDate.HasValue )
						SpaceHelper( sb ).Append( frequency >= DateRecurrenceFrequency.Minutely ? "of every minute" : "of " + calcNthString( startDate.Value.Minute ) + " minute" );

					if ( ccHours > 0 )
						SpaceHelper( sb ).Append( " on " ).Append( createList( hours.Select( calcNthString ), AND ) ).Append( " " ).Append( ccHours > 1 ? "hours" : "hour" );
					else if ( startDate.HasValue )
						SpaceHelper( sb ).Append( frequency >= DateRecurrenceFrequency.Hourly ? "of every hour" : "of " + calcNthString( startDate.Value.Hour ) + " hour" );
				}
			}

			if ( recurrence.Count > 0 )
			{
				SpaceHelper( sb ).Append( "for " ).Append( recurrence.Count ).Append( " number of times" );
			}

			DateTime? until = recurrence.Until;
			if ( until.HasValue )
			{
				// SSP 4/18/11 TFS72704
				// 
				//SpaceHelper( sb ).Append( "until " ).Append( until.Value.ToString( format ) );
				SpaceHelper( sb ).Append( "until " ).Append( until.Value.ToString( "d", format ) );
			}

			return sb.ToString( );
		}

		#endregion // ToDisplayStringHelper

		#region ToICalendarString

		internal static string ToICalendarString( DateTime dt, bool appendZ, bool skipTimeIf12AM = false )
		{
			string format = skipTimeIf12AM && dt.TimeOfDay.TotalSeconds < 1 ? ICAL_DATE_FORMAT : ICAL_DATETIME_FORMAT;

			string ret = dt.ToString( format, ScheduleUtilities.ParseCulture );

			if ( appendZ )
				ret = ret + "Z";

			return ret;
		}

		#endregion // ToICalendarString

		#endregion // Internal Methods
	} 

	#endregion // DateRecurrenceParser Class
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