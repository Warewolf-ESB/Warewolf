using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Controls.Primitives;
using System.Linq;

namespace Infragistics.Controls.Schedules
{
	#region ActivityReminderManager Class

	internal class ActivityReminderManager : IPropertyChangeListener
	{
		#region QueuedTimer Class

		private class QueuedTimer
		{
			internal ActivityBase _activity;
			internal object _timeManagerToken;
			internal DateTime _dueUtc;
		} 

		#endregion // QueuedTimer Class

		#region Member Vars

		protected readonly TimeSpan QUERY_INTERVAL = new TimeSpan( 1, 0, 0 );
		protected readonly TimeSpan REQUERY_INTERVAL = new TimeSpan( 0, 30, 0 );
		private object _requeryTask;

		protected readonly ScheduleDataConnectorBase _connector;
		private ActivityQueryResult _activitiesResult;
		private Dictionary<ResourceCalendar, List<ReminderSubscriber>> _reminderSubscribers;
		private Dictionary<ActivityBase, QueuedTimer> _queuedTimers;
		private DeferredOperation _deferredReevalReminders;
		private WeakDictionary<ActivityBase, ReminderInfo> _reminderInfoCache;
		private DictionaryPropertyStore<ReminderInfo, bool> _preventRedisplay = new DictionaryPropertyStore<ReminderInfo, bool>( );
		private VerifyAsyncItemsQueue<ActivityBase> _asyncProcessActivityQueue;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connector">Connector.</param>
		internal ActivityReminderManager( ScheduleDataConnectorBase connector )
		{
			CoreUtilities.ValidateNotNull( connector );
			_connector = connector;

			_queuedTimers = new Dictionary<ActivityBase, QueuedTimer>( );
			_reminderInfoCache = new WeakDictionary<ActivityBase, ReminderInfo>( true, true );
			_asyncProcessActivityQueue = new VerifyAsyncItemsQueue<ActivityBase>( ii => this.ProcessActivities( ii, false ), true );

			// SSP 11/4/10 TFS59104
			// Since we skip raising of reminders when LocalTimeZone is null, we need to resume it when
			// time zone becomes available.
			// 
			ScheduleUtilities.AddListener( _connector, this, true );
		}

		#endregion // Constructor

		#region Properties

		#region Private Properties

		#region LocalTimeZone

		private TimeZoneToken LocalTimeZone
		{
			get
			{
				return _connector.TimeZoneInfoProviderResolved.LocalToken;
			}
		}

		#endregion // LocalTimeZone

		#region ReminderSubscribers

		/// <summary>
		/// List of reminder subscribers. When a reminder for an activity is to be displayed,
		/// the task for conveying the reminder is delegated to these reminder subscribers.
		/// </summary>
		protected Dictionary<ResourceCalendar, List<ReminderSubscriber>> ReminderSubscribers
		{
			get
			{
				if ( null == _reminderSubscribers )
					_reminderSubscribers = new Dictionary<ResourceCalendar, List<ReminderSubscriber>>( );

				return _reminderSubscribers;
			}
		}

		#endregion // ReminderSubscribers 

		#endregion // Private Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region PerformRemindersQuery

		protected virtual ActivityQueryResult PerformRemindersQuery( )
		{
			return null;
		} 

		#endregion // PerformRemindersQuery

		#endregion // Protected Methods

		#region Internal Methods

		#region StartReminders

		internal void StartReminders( )
		{
			this.ReevalReminders( );
		}

		#endregion // StartReminders

		#region StopReminders

		internal void StopReminders( )
		{
			this.RemoveRequeryTask( );

			Dictionary<ActivityBase, QueuedTimer> timers = _queuedTimers;
			_queuedTimers = new Dictionary<ActivityBase, QueuedTimer>( );

			foreach ( KeyValuePair<ActivityBase, QueuedTimer> ii in timers )
				this.RemoveTask( ii.Value._timeManagerToken );
		}

		#endregion // StopReminders

		#region SubscribeToReminders

		/// <summary>
		/// Subscribes to reminders for the activities of the specified calendar. Note that more than one 
		/// subscriber can be subscribed to a single calendar as well as multiple calendars can be 
		/// subscribed to.
		/// </summary>
		/// <param name="calendar">This calendars activity reminders will be deilvered to the specified subscribed.</param>
		/// <param name="subscriber">When a reminder is due for an activity, the subscriber's <see cref="ReminderSubscriber.DeliverReminder"/> method will be invoked.</param>
		/// <param name="error">If there's an error, this will be set to error information.</param>
		internal void SubscribeToReminders( ResourceCalendar calendar, ReminderSubscriber subscriber, out DataErrorInfo error )
		{
			error = null;
			List<ReminderSubscriber> list = this.GetReminderSubscriberList( calendar );

			Debug.Assert( !list.Contains( subscriber ) );
			if ( !list.Contains( subscriber ) )
			{
				list.Add( subscriber );

				this.ReevalRemindersAsync( );
			}
		}

		#endregion // SubscribeToReminders

		#region UnsubscribeFromReminders

		/// <summary>
		/// Unsubscribes from activity reminders of the specified calendar. If the specified subscriber hadn't been subscribed
		/// previously then this method will take no action.
		/// </summary>
		/// <param name="calendar">The calendar's activity reminders to unsubscribe from.</param>
		/// <param name="subscriber">Subscriber instance.</param>
		internal void UnsubscribeFromReminders( ResourceCalendar calendar, ReminderSubscriber subscriber )
		{
			List<ReminderSubscriber> list = this.GetReminderSubscriberList( calendar );

			Debug.Assert( list.Contains( subscriber ) );
			if ( list.Remove( subscriber ) )
			{
				this.ReevalRemindersAsync( );
			}
		}

		#endregion // UnsubscribeFromReminders

		#endregion // Internal Methods

		#region Private Methods

		#region AddActivityTask

		private void AddActivityTask( ActivityBase activity, DateTime dueUtc )
		{
			QueuedTimer queuedTimer;
			if ( !_queuedTimers.TryGetValue( activity, out queuedTimer ) )
				_queuedTimers[activity] = queuedTimer = new QueuedTimer( ) { _activity = activity };

			object timeManagerToken = this.AddTask( dueUtc, ( ) => this.ProcessActivity( activity, false ) );

			queuedTimer._dueUtc = dueUtc;
			queuedTimer._timeManagerToken = timeManagerToken;

			// SSP 5/12/11 TFS75043
			// 
			this.ClearPreventRedisplayFlagHelper( activity );
		}

		#endregion // AddActivityTask

		#region AddTask

		/// <summary>
		/// Adds a task that will be performed at the specified time, which is in UTC.
		/// </summary>
		/// <param name="dateTime">Time in UTC.</param>
		/// <param name="action">Action to perform.</param>
		/// <returns></returns>
		private object AddTask( DateTime dateTime, Action action )
		{
			return TimeManager.Instance.AddTask( dateTime, action );
		}

		#endregion // AddTask

		#region ClearPreventRedisplayFlagHelper

		private void ClearPreventRedisplayFlagHelper( ActivityBase activity )
		{
			ReminderInfo reminderInfo = this.GetReminderInfo( activity, false, false );
			if ( null != reminderInfo )
				_preventRedisplay.SetValue( reminderInfo, false );
		}

		#endregion // ClearPreventRedisplayFlagHelper

		#region DisplayRemindersHelper

		private DateTime? DisplayRemindersHelper( ActivityBase activity, out bool shouldNotifySubscriberOfRemoval )
		{
			shouldNotifySubscriberOfRemoval = false;

			if ( activity.ReminderEnabled )
			{
				DateTime now = DateTime.UtcNow;
				TimeZoneToken localTimeZone = this.LocalTimeZone;

				// SSP 11/4/10 TFS59104
				// If there's no local time zone then return null. This is a blocking error condition and therefore
				// we do not need to take any action.
				// 
				if ( null == localTimeZone )
					return null;

				TimeSpan reminderInterval = activity.ReminderInterval;
				DateTime due = activity.GetStartUtc( localTimeZone ) - reminderInterval;

				if ( due > now )
				{
					shouldNotifySubscriberOfRemoval = true;
					return due;
				}

				ReminderInfo reminderInfo = this.GetReminderInfo( activity, true, false );
				Reminder reminder = reminderInfo.Reminder ?? activity.Reminder ?? new Reminder( );
				if ( null != reminder )
				{
					bool displayReminder = false;

					// If the reminder was snoozed, then re-display it if the snooze time has expired.
					// 
					if ( reminder.IsSnoozed )
					{
						DateTime snoozeExpireTime = reminder.LastSnoozeTime + reminder.SnoozeInterval;
						bool snoozeExpired = snoozeExpireTime <= now;
						if ( snoozeExpired )
							displayReminder = true;
						// SSP 11/2/10 TFS58970
						// If the reminder is snoozed then we do have to add a task to the time manager
						// to re-invoke this method when the snooze time expires.
						// 
						else
							return snoozeExpireTime;
					}
					// If we have already displayed the reminder the last time then don't display
					// it again. Note that if we had previously displayed the reminder and then
					// the start or the display interval was changed, re-display it based on the 
					// new interval value. For that we are clearing _preventRedisplay flag for 
					// the reminderInfo whenever the activity's ReminderInterval or ReminderEnabled
					// changes.
					// 
					else if ( ! _preventRedisplay.GetValue( reminderInfo ) )
					{
						displayReminder = true;
					}

					if ( displayReminder )
					{
						List<ReminderSubscriber> subscribers = this.GetReminderSubscriberList(activity, false);
						
						if ( null != subscribers && subscribers.Count > 0 )
						{
							reminderInfo = this.GetReminderInfo( activity, true, true );

							// If any change in the property of the activity or the reminder causes us to get
							// in this method recursively, don't re-display the reminder while in the process
							// of displaying it.
							// 
							if ( ScheduleUtilities.Antirecursion.Enter( reminderInfo, this, true ) )
							{
								try
								{
									// Set a flag to prevent the reminder from being re-displayed. Note that 
									// the flag has scope of the duration of the application. If the application
									// is re-run, the reminder will be re-activated. This behavior is intentional.
									// 
									_preventRedisplay.SetValue( reminderInfo, true );

									// Reset IsSnoozed to false in case the reminder was previously snoozed.
									// 
									reminder.IsSnoozed = false;

									foreach ( ReminderSubscriber ss in subscribers )
										ss.DeliverReminder( reminderInfo );
								}
								finally
								{
									ScheduleUtilities.Antirecursion.Exit( reminderInfo, this );
								}
							}
						}
					}
				}
			}
			else
			{
				shouldNotifySubscriberOfRemoval = true;
			}

			return null;
		}

		#endregion // DisplayRemindersHelper

		#region GetReminderInfo

		/// <summary>
		/// Gets the reminder info object associated with the activity. This reminder info is exposed via data manager's active reminders collection.
		/// </summary>
		/// <param name="activity">Activity instance.</param>
		/// <param name="allocateIfNecessary">If true allocates one if one hasn't been allocated.</param>
		/// <param name="reactivateIfNecessary">If the reminder was dismissed, resets its IsDismissed to false.</param>
		/// <returns>Returns the reminder info object.</returns>
		private ReminderInfo GetReminderInfo( ActivityBase activity, bool allocateIfNecessary, bool reactivateIfNecessary )
		{
			ReminderInfo info;
			if ( !_reminderInfoCache.TryGetValue( activity, out info ) && allocateIfNecessary )
			{
				_reminderInfoCache[activity] = info = new ReminderInfo( activity.Reminder, activity );
				info.AddListener( this, false );
			}

			if ( reactivateIfNecessary )
				info.IsDismissed = false;

			return info;
		} 

		#endregion // GetReminderInfo

		#region GetReminderSubscriberList

		private List<ReminderSubscriber> GetReminderSubscriberList(ActivityBase activity, bool allocate = true)
		{
			return this.GetReminderSubscriberList(activity.OwningCalendar, allocate);
		}

		private List<ReminderSubscriber> GetReminderSubscriberList(ResourceCalendar calendar, bool allocate = true)
		{
			List<ReminderSubscriber> subscribers = null;
			Debug.Assert(null != calendar);

			if (null != calendar)
			{
				if (this.ReminderSubscribers.TryGetValue(calendar, out subscribers) == false)
				{
					if (allocate)
						this.ReminderSubscribers[calendar] = subscribers = new List<ReminderSubscriber>();
					else
						subscribers = null;
				}
			}

			return subscribers;
		}

		#endregion  // GetReminderSubscriberList

		#region NotifySubscribersOfRemoval

		private void NotifySubscribersOfRemoval(ActivityBase activity)
		{
			List<ReminderSubscriber> subscribers = this.GetReminderSubscriberList(activity, false);

			if (null != subscribers && subscribers.Count > 0)
			{
				ReminderInfo reminderInfo = this.GetReminderInfo(activity, false, false);

				if (reminderInfo != null)
				{
					// If any change in the property of the activity or the reminder causes us to get
					// in this method recursively, don't remove the reminder again while we are in the
					// process of removing it.
					// 
					if (ScheduleUtilities.Antirecursion.Enter(reminderInfo, this, true))
					{
						try
						{
							foreach (ReminderSubscriber ss in subscribers)
								ss.RemoveReminder(reminderInfo);
						}
						finally
						{
							ScheduleUtilities.Antirecursion.Exit(reminderInfo, this);
						}
					}
				}
			}
		} 

		#endregion  // NotifySubscribersOfRemoval

		#region ProcessActivities

		private void ProcessActivities( IEnumerable<ActivityBase> activities, bool remove )
		{
			foreach ( ActivityBase ii in activities )
				this.ProcessActivity( ii, remove );
		}

		#endregion // ProcessActivities

		#region ProcessActivity

		private bool ProcessActivity( ActivityBase activity, bool remove )
		{
			if ( remove )
			{
				this.RemoveActivityTask( activity, true, true );
			}
			else
			{
				// DisplayRemindersHelper displays the reminder if it's due otherwise returns a date-time value when
				// the reminder is due.
				// 
				bool shouldNotifySubscriberOfRemoval;
				DateTime? dueUtc = DisplayRemindersHelper(activity, out shouldNotifySubscriberOfRemoval);

				if ( dueUtc.HasValue )
				{
					if (shouldNotifySubscriberOfRemoval)
						this.NotifySubscribersOfRemoval(activity);

					this.RequeueActivityTask( activity, dueUtc.Value );
					return true;
				}
				else
				{
					this.RemoveActivityTask(activity, false, shouldNotifySubscriberOfRemoval);
				}
			}

			return false;
		}

		#endregion // ProcessActivity

		#region ReevalRemindersAsync

		private void ReevalRemindersAsync( )
		{
			if ( null == _deferredReevalReminders )
			{
				_deferredReevalReminders = new DeferredOperation( this.ReevalReminders );
				_deferredReevalReminders.StartAsyncOperation( );
			}
		} 

		#endregion // ReevalRemindersAsync

		#region ReevalReminders

		private void ReevalReminders( )
		{
			if ( null != _deferredReevalReminders )
			{
				_deferredReevalReminders.CancelPendingOperation( );
				_deferredReevalReminders = null;
			}

			// Remove any previous requery task.
			// 
			this.RemoveRequeryTask( );

			// Requery for reminders activities after requery interval. We get activities that are due within next QUERY_INTERVAL
			// amount of time. After that we need to requery for activities that would be due then.
			// 
			_requeryTask = this.AddTask( DateTime.UtcNow.Add( REQUERY_INTERVAL ), this.ReevalReminders );

			// Get the activities that are due within next QUERY_INTERVAL.
			// 
			ActivityQueryResult result = this.PerformRemindersQuery( );
			if ( null != result )
			{
				ScheduleUtilities.ManageListenerHelperObj( ref _activitiesResult, result, this, true );

				if ( null != result && result.IsComplete )
					this.ReprocessAllActivities( );
			}
		}

		#endregion // ReevalReminders

		#region RemoveActivityTask

		private void RemoveActivityTask( ActivityBase activity, bool removeActivity, bool notifySubscriberOfRemoval )
		{
			QueuedTimer queuedTimer;
			if ( _queuedTimers.TryGetValue( activity, out queuedTimer ) )
			{
				if ( null != queuedTimer )
				{
					object token = queuedTimer._timeManagerToken;
					if ( null != token )
					{
						queuedTimer._timeManagerToken = null;
						this.RemoveTask( token );
					}
				}

				if ( removeActivity )
					_queuedTimers.Remove( activity );
			}

			if (notifySubscriberOfRemoval)
				this.NotifySubscribersOfRemoval(activity);
		}

		#endregion // RemoveActivityTask

		#region RemoveRequeryTask

		private void RemoveRequeryTask( )
		{
			if ( null != _requeryTask )
			{
				this.RemoveTask( _requeryTask );
				_requeryTask = null;
			}
		}

		#endregion // RemoveRequeryTask

		#region ReprocessAllActivities

		private void ReprocessAllActivities( )
		{
			// SSP 4/16/11 TFS63818
			// Remove any reminders associated with old activities that are no longer in the new result, which would
			// indicate that either they were removed or the data source was reset to a different data source or their
			// ReminderEnabled was set to false. In any of these cases, we should not have the activity be displayed
			// in the active reminders list.
			// 
			if ( null != _reminderInfoCache && _reminderInfoCache.Count > 0 )
			{
				HashSet<ActivityBase> oldReminderActivities = new HashSet<ActivityBase>( _reminderInfoCache.Keys );

				if ( null != _activitiesResult )
				{
					foreach ( ActivityBase ii in _activitiesResult.Activities )
						oldReminderActivities.Remove( ii );
				}

				foreach ( ActivityBase ii in oldReminderActivities )
					this.NotifySubscribersOfRemoval( ii );
			}

			if ( null != _activitiesResult )
				this.ProcessActivities( _activitiesResult.Activities, false );
		}

		#endregion // ReprocessAllActivities

		#region RequeueActivityTask

		private void RequeueActivityTask( ActivityBase activity, DateTime newDueUtc )
		{
			QueuedTimer queuedTimer;
			if ( !_queuedTimers.TryGetValue( activity, out queuedTimer ) || queuedTimer._dueUtc != newDueUtc )
			{
				this.RemoveActivityTask( activity, false, false );
				this.AddActivityTask( activity, newDueUtc );
			}
		}

		#endregion // RequeueActivityTask

		#region RemoveTask

		private void RemoveTask( object addTaskToken )
		{
			TimeManager.Instance.Remove( addTaskToken );
		}

		#endregion // RemoveTask

		#region SyncWithActivityReminder

		// SSP 7/22/11 TFS81068
		// 
		/// <summary>
		/// Synchronizes ReminderInfo's Reminder to activity's Reminder or the other way around based on the 'copyToActivity' parameter.
		/// </summary>
		/// <param name="activity">Activity object.</param>
		/// <param name="copyToActivity">If copyToActivity is true then reminder info's Reminder is copied 
		/// to Activity and if it's false then activity's reminder is copied to the reminder info.</param>
		private void SyncWithActivityReminder( ActivityBase activity, bool copyToActivity )
		{
			// When the activity's Reminder property changes, reset the associated ReminderInfo's Reminder
			// property to the new Reminder object.
			// 
			ReminderInfo info = this.GetReminderInfo( activity, false, false );
			if ( null != info )
			{
				if ( ScheduleUtilities.Antirecursion.Enter( info, "reminder_update", true ) )
				{
					try
					{
						if ( copyToActivity )
							activity.Reminder = info.Reminder;
						else
							info.Reminder = activity.Reminder;
					}
					finally
					{
						ScheduleUtilities.Antirecursion.Exit( info, "reminder_update" );
					}
				}
			}
		}

		#endregion // SyncWithActivityReminder

		#endregion // Private Methods

		#endregion // Methods

		#region IPropertyChangeListener Interface Implementation

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object sender, string propName, object extraInfo )
		{
			bool reprocessActivities = false;
			ActivityQueryResult activitiesResult = _activitiesResult;

			if ( sender == activitiesResult )
			{
				switch ( propName )
				{
					case "Activities":
						reprocessActivities = true;
						break;
				}
			}
			else if ( null != activitiesResult && sender == activitiesResult.Activities )
			{
				NotifyCollectionChangedEventArgs args = extraInfo as NotifyCollectionChangedEventArgs;
				if ( null != args )
				{
					switch ( args.Action )
					{
						case NotifyCollectionChangedAction.Add:
							this.ProcessActivities( ScheduleUtilities.ToTyped<ActivityBase>( args.NewItems ), false );
							break;
						case NotifyCollectionChangedAction.Remove:
							this.ProcessActivities( ScheduleUtilities.ToTyped<ActivityBase>( args.OldItems ), true );
							break;
						case NotifyCollectionChangedAction.Reset:
							reprocessActivities = true;
							break;
					}
				}
			}
			else if ( sender is ActivityBase )
			{
				ActivityBase activity = (ActivityBase)sender;
				bool reprocess = false;

				switch ( propName )
				{
					case "ReminderInterval":
					case "ReminderEnabled":
						// SSP 11/2/10 TFS58963
						// We should re-display the reminder dialog if the Start time changes and the reminder hasn't
						// been dismissed yet. If the reminder has already been dismissed, its ReminderEnabled will be
						// false and thus this will do nothing.
						// 
					case "Start":
						this.ClearPreventRedisplayFlagHelper( activity );

						// SSP 11/2/10 TFS58965
						// Don't display the reminder dialog while the activity is in edit mode. Wait until the EndEdit
						// to evaluate whether the reminder for the activity should be displayed. For example, when 
						// resizing an activity, we are updating the Start time as the mouse is moved. We should wait
						// until the mouse is released before re-evaluating whether the reminder for the activity should
						// be displayed based on the final Start time. Enclosed the existing code in the if block that 
						// checks for IsInEdit flag.
						// 
						//reprocess = true;
						if ( ! activity.IsInEdit )
							reprocess = true;

						break;
					// SSP 11/2/10 TFS58965
					// 
					case "IsInEdit":
						if ( !activity.IsInEdit )
							reprocess = true;
						break;
					case "Reminder":
						{
							// SSP 7/22/11 TFS81068
							// When the activity's Reminder property changes, reset the associated ReminderInfo's Reminder
							// property to the new Reminder object.
							// 
							this.SyncWithActivityReminder( activity, false );

							reprocess = true;
						}
						break;
				}

				if ( reprocess )
				{
					// SSP 11/2/10 TFS58963
					// Process asynchronously because it's possible for us to get multiple change
					// notifications for different properties and even for the same property.
					// 
					//this.ProcessActivity( activity, false );
					_asyncProcessActivityQueue.Enque( activity, true );
				}
			}
			else if ( sender is ReminderInfo )
			{
				ReminderInfo reminderInfo = (ReminderInfo)sender;
				ActivityBase activity = reminderInfo.Context as ActivityBase;

				Debug.Assert( null != activity );
				if ( null != activity )
				{
					if ( "Reminder" == propName || extraInfo is Reminder )
					{
						// SSP 7/22/11 TFS81068
						// Added anti-recursion logic.
						// 
						//activity.Reminder = reminderInfo.Reminder;
						this.SyncWithActivityReminder( activity, true );
					}
				}
			}
			// SSP 11/4/10 TFS59104
			// Since we skip raising of reminders when LocalTimeZone is null, we need to resume it when
			// time zone becomes available.
			// 
			else if ( sender == _connector && "TimeZoneInfoProviderResolved" == propName
				|| null != _connector && sender == _connector.TimeZoneInfoProviderResolved 
					&& ( "LocalTimeZoneId" == propName || "Version" == propName ) 
				)
			{
				reprocessActivities = true;
			}

			if ( reprocessActivities )
				this.ReprocessAllActivities( );
		} 

		#endregion // IPropertyChangeListener Interface Implementation
	} 

	#endregion // ActivityReminderManager Class

	internal class ListConnectorActivityReminderManager : ActivityReminderManager
	{
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connector">Connector.</param>
		internal ListConnectorActivityReminderManager( ListScheduleDataConnector connector )
			: base( connector )
		{
		}

		#endregion // Constructor

		#region Connector

		internal ListScheduleDataConnector Connector
		{
			get
			{
				return (ListScheduleDataConnector)_connector;
			}
		} 

		#endregion // Connector

		#region PerformRemindersQuery

		protected override ActivityQueryResult PerformRemindersQuery( )
		{
			IEnumerable<ResourceCalendar> calendars = this.ReminderSubscribers.Keys;

			// Get an hour's worth of reminder activities.
			// 
			DateTime now = DateTime.UtcNow;
			DateRange range = new DateRange( now, now.Add( QUERY_INTERVAL ) );

			ActivityQuery query = new ActivityQuery( ActivityTypes.Appointment | ActivityTypes.Task, range, calendars );

			ListScheduleDataConnector listConnector = this.Connector;

			List<ListQueryResult> list = new List<ListQueryResult>( );

			listConnector._appointmentListManager.GetReminderQueryResults( query, list );
			listConnector._recurringAppointmentListManager.GetReminderQueryResults( query, list );
			listConnector._taskListManager.GetReminderQueryResults( query, list );
			listConnector._journalListManager.GetReminderQueryResults( query, list );

			return new ListConnectorActivityQueryResult( query, list );
		}

		#endregion // PerformRemindersQuery
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