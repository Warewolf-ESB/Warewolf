using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Infragistics.Collections;
using System.Diagnostics;
using System.Collections.Generic;

namespace Infragistics.Controls.Primitives
{
	internal class TimeManager
	{
		#region Member Variables

		[ThreadStatic]
		private static TimeManager _instance;

		private DispatcherTimer _timer;
		private WeakList<Task> _tasks;

		// the time at which the next task is due
		private DateTime? _nextTaskTime;

		// a flag indicating if items have been added which would 
		// indicate that the order of the tasks must be updated
		private bool _isNextTaskTimeSortDirty;

		// the smallest daterange that represents the safe area within which the time 
		// can change without worrying about whether we left an entered range
		private DateRange? _smallestExitRange = DateRange.Infinite;
		private WeakList<TimeRangeItem> _exitRanges;

		// this is a combined set of the date ranges of all the items 
		// for which notifications should be sent when the current time enters
		private DateRange[] _combinedEnterRanges;
		private WeakList<TimeRangeItem> _enterRanges;

		private bool _isInvoking;

		private const long MinTimerInterval = TimeSpan.TicksPerMillisecond * 10;
		private const long MaxTimerInterval = 2147483647 * TimeSpan.TicksPerMillisecond;

		// check the system time at least every 3 seconds
		private const long SystemTimeMaxInterval = TimeSpan.TicksPerSecond * 3;

		#endregion // Member Variables

		#region Constructor
		private TimeManager()
		{
			_nextTaskTime = null;
			_tasks = new WeakList<Task>();
			_timer = new DispatcherTimer();
			_timer.Tick += new EventHandler(OnTimerTick);
			_exitRanges = new WeakList<TimeRangeItem>();
			_enterRanges = new WeakList<TimeRangeItem>();
		}
		#endregion // Constructor

		#region Properties

		#region Instance
		internal static TimeManager Instance
		{
			get
			{
				if (_instance == null)
				{

				Debug.Assert(Dispatcher.CurrentDispatcher != null, "Being called on a thread that doesn't have a dispatcher?");


					_instance = new TimeManager();
				}

				return _instance;
			}
		} 
		#endregion // Instance

		#region IsTimerNeeded
		private bool IsTimerNeeded
		{
			get
			{
				if (_tasks.Count > 0)
					return true;

				if (_exitRanges.Count > 0)
					return true;

				if (_enterRanges.Count > 0)
					return true;

				return false;
			}
		}
		#endregion // IsTimerNeeded

		#endregion // Properties

		#region Methods

		#region Public methods

		#region AddTask
		/// <summary>
		/// Invokes a specified action when the current time hits or exceeds the specified time.
		/// </summary>
		/// <param name="time">The time at which the action should be invoked</param>
		/// <param name="action">The action to invoke</param>
		/// <returns>A token that must be strongly referenced by the caller.</returns>
		public object AddTask(DateTime time, Action action)
		{
			Debug.Assert(time.Kind == DateTimeKind.Utc, "Expecting Utc time");

			Task task = new Task(time, action);

			Debug.Assert(_nextTaskTime == null || _tasks.Count > 0, "We should not have a task time if there are no more tasks");

			// if there are no tasks then use this as the starting task time
			if (_tasks.Count == 0)
			{
				_tasks.Add(task);
				_nextTaskTime = task.Time;
			}
			else if (_nextTaskTime != null && _nextTaskTime > task.Time)
			{
				// if we have already calculated the next task time and this new task 
				// is before that just insert at the front of the list and use its time.
				_tasks.Insert(0, task);
				_nextTaskTime = task.Time;
			}
			else
			{
				Debug.Assert(_nextTaskTime != null, "We have tasks but no next task time?");

				// otherwise just add it to the end and consider the collection order dirty
				_isNextTaskTimeSortDirty = true;
				_tasks.Add(task);
			}

			if (!_isInvoking)
				this.StartTimer(CurrentTime.Now);

			return task;
		}
		#endregion // AddTask

		#region AddTimeRange
		/// <summary>
		/// Adds a notification that is triggered when the current time enters/leaves the specified range
		/// </summary>
		/// <param name="timeRange">The time range to watch</param>
		/// <param name="isEnterAction">True to invoke the action when the current time enters the <paramref name="timeRange"/>; false to invoke the action when the current time leaves the range.</param>
		/// <param name="action">The action to invoke when the time enters/leaves the specified range.</param>
		/// <returns>A token that must be strongly referenced by the caller.</returns>
		public object AddTimeRange(DateRange timeRange, bool isEnterAction, Action action)
		{
			Debug.Assert(timeRange.Start.Kind == DateTimeKind.Utc, "Expecting Utc time");

			TimeRangeItem range = new TimeRangeItem(timeRange, isEnterAction, action);

			if (range.IsEnterAction)
			{
				#region Enter Range Item

				if (_enterRanges.Count == 0)
					_combinedEnterRanges = new DateRange[] { range.Range };
				else
				{
					_combinedEnterRanges = null;
				}

				_enterRanges.Add(range);

				#endregion // Enter Range Item
			}
			else
			{
				#region Leave Range Alert Items

				_exitRanges.Add(range);

				// if we have a smallest range and this intersects with it then we may need to reduce
				// that range
				if (_smallestExitRange != null)
				{
					DateRange intersection = _smallestExitRange.Value;

					if (intersection.Intersect(range.Range))
						_smallestExitRange = intersection;
					else
					{
						// otherwise clear the range so we check for items that 
						// left the entered range which possibly includes this item
						_smallestExitRange = null;
					}
				}
				#endregion // Leave Range Alert Items
			}

			if (!_isInvoking)
				this.StartTimer(CurrentTime.Now);

			return range;
		} 
		#endregion // AddTimeRange

		#region Remove
		/// <summary>
		/// Removes the specified time item.
		/// </summary>
		/// <param name="token">The object that was returned from the add method used to add the time item</param>
		public void Remove(object token)
		{
			Debug.Assert(token is TimeItem);
			this.RemoveItem(token as TimeItem);
		}
		#endregion // Remove

		#region SnoozeTask
		/// <summary>
		/// Increases the time before a task's action is to be invoked
		/// </summary>
		/// <param name="item">The token for the task returned from the AddTask method</param>
		/// <param name="newTime">A time in the future from the current task time that the associated action should be invoked.</param>
		public void SnoozeTask(object item, DateTime newTime)
		{
			Task task = item as Task;
			Debug.Assert(newTime.Kind == DateTimeKind.Utc, "Expecting Utc time");
			Debug.Assert(task != null, "Can only snooze a task");
			Debug.Assert(task == null || newTime > task.Time, "Should only be snoozing to a future time");
			Debug.Assert(task == null || _tasks.Contains(task), "The task is no longer active");

			if (null != task && task.Time < newTime)
			{
				_isNextTaskTimeSortDirty = true;
				task.Time = newTime;
			}
		} 
		#endregion // SnoozeTask

		#endregion // Public methods

		#region Private methods

		#region InvokeItems
		private void InvokeItems(List<TimeItem> itemsToProcess)
		{
			bool wasInvoking = _isInvoking;
			_isInvoking = false;

			try
			{
				foreach (TimeItem item in itemsToProcess)
				{
					item.Invoke();
				}
			}
			finally
			{
				_isInvoking = wasInvoking;
			}
		}
		#endregion // InvokeItems

		#region InvokeRanges
		private void InvokeRanges(DateTime now)
		{
			List<TimeItem> itemsToProcess = null;

			if (_exitRanges.Count > 0)
			{
				if (_smallestExitRange == null || !_smallestExitRange.Value.ContainsExclusive(now))
				{
					#region Enter Ranges

					// assume there will be no more entered tasks
					_smallestExitRange = null;

					if (itemsToProcess == null)
						itemsToProcess = new List<TimeItem>();

					bool hasEnteredItems = false;
					DateRange newExitRange = DateRange.Infinite;

					#region Enumerate Exit Ranges
					for (int i = 0, count = _exitRanges.Count; i < count; i++)
					{
						TimeRangeItem item = _exitRanges[i];

						if (null == item)
							continue;

						DateRange range = item.Range;

						if (!range.ContainsExclusive(now))
						{
							_exitRanges[i] = null;
							itemsToProcess.Add(item);
						}
						else if (!hasEnteredItems)
						{
							hasEnteredItems = true;
							newExitRange = range;
						}
						else
						{
							newExitRange.Intersect(range);
						}
					}

					if (hasEnteredItems)
						_smallestExitRange = newExitRange;

					#endregion // Enumerate Exit Ranges

					if (_smallestExitRange == null)
						_exitRanges.Clear(); 

					#endregion // Enter Ranges
				}
			}

			if (_enterRanges.Count > 0)
			{
				// if we don't have a list of ranges or we do and we entered into range that wants to know...
				if (_combinedEnterRanges == null || CoreUtilities.BinarySearch(_combinedEnterRanges, now) >= 0)
				{
					#region Exit Ranges
					List<DateRange> ranges = new List<DateRange>();

					if (itemsToProcess == null)
						itemsToProcess = new List<TimeItem>();

					#region Enumerate Enter Ranges
					for (int i = 0, count = _enterRanges.Count; i < count; i++)
					{
						TimeRangeItem item = _enterRanges[i];

						if (null == item)
							continue;

						DateRange range = item.Range;

						if (range.ContainsExclusive(now))
						{
							_enterRanges[i] = null;
							itemsToProcess.Add(item);
						}
						else
						{
							ranges.Add(item.Range);
						}
					}
					#endregion // Enumerate Enter Ranges

					if (ranges.Count == 0)
					{
						_combinedEnterRanges = null;
						_enterRanges.Clear();
					}
					else
					{
						_combinedEnterRanges = DateRange.CombineRanges(ranges);
					} 
					#endregion // Exit Ranges
				}
			}

			if (null != itemsToProcess)
				InvokeItems(itemsToProcess);
		} 
		#endregion // InvokeRanges

		#region InvokeTasks
		private bool InvokeTasks(DateTime now)
		{
			if (_nextTaskTime == null)
				return false;

			// if the time is before the next task time there is nothing to do
			if (now < _nextTaskTime)
				return false;

			// if we don't know the next task time or we do and some items have 
			// since been added and therefore we may need to process more than one...
			if (_isNextTaskTimeSortDirty)
			{
				this.RebuildTaskList();

				// if we didn't know the task time but we know it to be later
				// then there is nothing to do now
				if (_nextTaskTime == null || now < _nextTaskTime)
					return true;
			}

			#region Get Tasks To Process and Update NextTaskTime

			// assume there will be no more tasks
			_nextTaskTime = null;

			List<TimeItem> itemsToProcess = new List<TimeItem>();

			int i = 0;
			int count = _tasks.Count;

			for (; i < count; i++)
			{
				Task task = _tasks[i];

				if (null == task)
					continue;

				// if we hit something beyond the current time then bail
				if (task.Time > now)
				{
					// there is a task after so use its 
					// time as the next start time
					_nextTaskTime = task.Time;
					break;
				}

				_tasks[i] = null;
				itemsToProcess.Add(task);
			}

			// if there are no more tasks to process then clear the list
			if (_nextTaskTime == null)
				_tasks.Clear();
			else if (count > 0) // otherwise remove the block that are processing
				_tasks.RemoveRange(0, i);

			#endregion // Get Tasks To Process and Update NextTaskTime

			InvokeItems(itemsToProcess);
			return true;
		}
		#endregion // InvokeTasks

		#region OnTimerTick
		private void OnTimerTick(object sender, EventArgs e)
		{
			DateTime now = CurrentTime.Now;

			if (_tasks.Count > 0)
				this.InvokeTasks(now);

			if (_exitRanges.Count > 0 || _enterRanges.Count > 0)
				this.InvokeRanges(now);

			this.VerifyTimer(now);
		}
		#endregion // OnTimerTick

		#region RebuildTaskList
		private void RebuildTaskList()
		{
			List<Task> tasks = new List<Task>();

			// get all the existing tasks - the enumerator only returns
			// non-null items
			foreach (Task task in _tasks)
				tasks.Add(task);

			// we're going to rebuild the collection so clear it
			_tasks.Clear();

			if (tasks.Count > 0)
			{
				// if tasks were added then the sort order would be dirty and we need to reverify that
				tasks.Sort();

				// use the time of the first item
				_nextTaskTime = tasks[0].Time;
				_tasks.AddRange(tasks);
			}
			else
			{
				// if there are no remaining tasks then clear the task time
				_nextTaskTime = null;
			}

			_isNextTaskTimeSortDirty = false;
		}
		#endregion // RebuildTaskList

		#region RemoveItem
		private void RemoveItem(TimeItem item)
		{
			Task task = item as Task;

			if (task != null)
			{
				_tasks.Remove(task);

				// if there are no more tasks then clear the task time
				if (_tasks.Count == 0)
					_nextTaskTime = null;
			}
			else
			{
				TimeRangeItem range = item as TimeRangeItem;

				if (null != range)
				{
					if (!range.IsEnterAction)
					{
						bool removed = _exitRanges.Remove(range);
						Debug.Assert(removed, "The specified range wasn't in the entered ranges");

						if (_exitRanges.Count == 0)
							_smallestExitRange = null;
					}
					else
					{
						bool removed = _enterRanges.Remove(range);
						Debug.Assert(removed, "The specified range wasn't in the exited ranges");

						if (_enterRanges.Count == 0)
							_combinedEnterRanges = null;
					}
				}
			}

			// if we're not in the middle of invoking actions and the timer is no 
			// longer needed then stop it. note, we're not calling verifytimer 
			// because even if we removed the earliest task, we don't need to 
			// synchronously recalculate the timer start. instead the timer will 
			// trigger with the lowest time we had and we can update the task 
			// time at that point. this way if there are multiple items being 
			// removed we will just verify the state once
			if (!_isInvoking && !this.IsTimerNeeded)
				this.StopTimer();
		}
		#endregion // RemoveItem

		#region StartTimer
		private void StartTimer(DateTime now)
		{
			// if we're in the process of invoking then it will verify the 
			// timer when its done
			if (_isInvoking)
				return;

			long ticks = MaxTimerInterval;

			if (_nextTaskTime != null)
			{
				ticks = _nextTaskTime.Value.Subtract(now).Ticks;
			}
			else
			{
				// we need to identify if we're waiting for the max time
				ticks = long.MaxValue;
			}

			if (_exitRanges.Count > 0)
			{
				// if we haven't calculated a range then use the smallest interval
				if (_smallestExitRange == null)
					ticks = 0;
				else // otherwise wait until the end of the range we are watching
					ticks = Math.Min(ticks, _smallestExitRange.Value.End.Subtract(now).Ticks);
			}

			if (ticks > 0 && _enterRanges.Count > 0)
			{
				if (_combinedEnterRanges == null)
					ticks = 0;
				else
				{
					int index = CoreUtilities.BinarySearch(_combinedEnterRanges, now);

					// if we're already in a range then invoke as soon as possible
					if (index >= 0)
						ticks = 0;
					else
					{
						index = ~index;

						if (index < _combinedEnterRanges.Length)
						{
							// just use the range that it's before
							ticks = Math.Min(ticks, _combinedEnterRanges[index].Start.Subtract(now).Ticks);
						}
						else
						{
							// AS 3/4/11
							// this can happen if the enter range we are watching is before the 
							// current time. In this case we would have used the interval we use 
							// for watching for the system time changing (~3 seconds) and leave 
							// the assert below.
							//
							ticks = Math.Min(ticks, SystemTimeMaxInterval);
						}
					}
				}
			}

			Debug.Assert(ticks != long.MaxValue, "Nothing wanted to be notified so we're going to wait a long time.");

			// we have a minimum threshold for how often we check the time
			// in case the system time is changed
			if (ticks > SystemTimeMaxInterval)
				ticks = SystemTimeMaxInterval;

			// make sure its within the allowed range for a timer
			ticks = Math.Min(MaxTimerInterval, Math.Max(MinTimerInterval, ticks));

			_timer.Interval = TimeSpan.FromTicks(ticks);

			if (!_timer.IsEnabled)
				_timer.Start();
		}
		#endregion // StartTimer

		#region StopTimer
		private void StopTimer()
		{
			_timer.Stop();
		}
		#endregion // StopTimer

		#region VerifyTimer
		private void VerifyTimer(DateTime now)
		{
			if (_isInvoking)
				return;

			bool isTimerNeeded = this.IsTimerNeeded;

			if (this.IsTimerNeeded)
				this.StartTimer(now);
			else
				this.StopTimer();
		}
		#endregion // VerifyTimer

		#endregion // Private methods

		#endregion // Methods

		#region TimeItem class
		private class TimeItem
		{
			#region Member Variables

			private Action _action;

			#endregion // Member Variables

			#region Constructor
			protected TimeItem(Action action)
			{
				CoreUtilities.ValidateNotNull(action, "action");
				_action = action;
			}
			#endregion // Constructor

			#region Invoke
			internal void Invoke()
			{
				_action();
			}
			#endregion // Invoke
		} 
		#endregion // TimeItem class

		#region Task class
		private class Task : TimeItem
			, IComparable<Task>
		{
			#region Member Variables

			internal DateTime Time;

			#endregion // Member Variables

			#region Constructor
			internal Task(DateTime time, Action action)
				: base(action)
			{
				this.Time = time;
			}
			#endregion // Constructor

			#region IComparable<Task> Members

			int IComparable<Task>.CompareTo(Task other)
			{
				if (other == null)
					return 1;

				return this.Time.CompareTo(other.Time);
			}

			#endregion //IComparable<Task> Members
		} 
		#endregion // Task class

		#region TimeRangeItem class
		private class TimeRangeItem : TimeItem
		{
			#region Member Variables

			internal DateRange Range;
			internal readonly bool IsEnterAction;

			#endregion // Member Variables

			#region Constructor
			internal TimeRangeItem(DateRange range, bool isEnterAction, Action action)
				: base(action)
			{
				Range = range;
				IsEnterAction = isEnterAction;
			}
			#endregion // Constructor
		} 
		#endregion // TimeRangeItem class
	}

	#region CurrentTime
	internal static class CurrentTime
	{
		#region Now
		internal static DateTime Now
		{
			get { return DateTime.UtcNow; }
		}
		#endregion // Now
	} 
	#endregion // CurrentTime
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