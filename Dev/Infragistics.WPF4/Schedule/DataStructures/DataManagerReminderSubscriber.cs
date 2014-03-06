using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules
{
	internal class DataManagerReminderSubscriber : ReminderSubscriber, IPropertyChangeListener
	{
		private XamScheduleDataManager _owner;
		private ScheduleDataConnectorBase _connector;
		private Resource _resource;
		private HashSet<ResourceCalendar> _subscribedCalendars;
		private HashSet<ActivityBase> _pendingCommitActivities;
		private DeferredOperation _commitActivityOperation;
		internal readonly ObservableCollectionExtended<ReminderInfo> _activeReminders;

		internal DataManagerReminderSubscriber( XamScheduleDataManager owner )
		{
			CoreUtilities.ValidateNotNull( owner );
			_owner = owner;
			_activeReminders = new ObservableCollectionExtended<ReminderInfo>( false, true );
			( (ISupportPropertyChangeNotifications)_activeReminders ).AddListener( this, true );
			_subscribedCalendars = new HashSet<ResourceCalendar>( );
			_owner.PropChangeListeners.Add( this, false );
			this.Resubscribe( );
		}

		public override void DeliverReminder( ReminderInfo info )
		{
			if ( ! _activeReminders.Contains( info ) )
				_activeReminders.Add( info );

			_owner.OnReminderAdded( info );
		}

		public override void RemoveReminder(ReminderInfo info)
		{
			_activeReminders.Remove(info);
		}

		private void OnError( DataErrorInfo error )
		{
			ScheduleUtilities.RaiseErrorHelper( _owner, error );
		}

		private void SubscribeHelper( ResourceCalendar calendar, bool unsubscribe )
		{
			if ( unsubscribe )
			{
				if ( _subscribedCalendars.Contains( calendar ) )
				{
					_subscribedCalendars.Remove( calendar );
					_connector.UnsubscribeFromReminders( calendar, this );
				}
			}
			else
			{
				if ( ! _subscribedCalendars.Contains( calendar ) )
				{
					DataErrorInfo error;
					_connector.SubscribeToReminders( calendar, this, out error );

					if ( null != error )
						this.OnError( error );
					else
						_subscribedCalendars.Add( calendar );
				}
			}
		}

		private void Resubscribe( )
		{
			ScheduleDataConnectorBase newConnector = _owner.DataConnector;
			Resource newResource = _owner.CurrentUser;
			HashSet<ResourceCalendar> newCalendars = null != newResource && null != newResource.Calendars
				? new HashSet<ResourceCalendar>( newResource.Calendars ) : null;

			List<ResourceCalendar> unsubscribeList = new List<ResourceCalendar>( );
			if ( null == newCalendars || _connector != newConnector )
				unsubscribeList.AddRange( _subscribedCalendars );
			else
				unsubscribeList.AddRange( _subscribedCalendars.Except( newCalendars ) );

			foreach ( ResourceCalendar ii in unsubscribeList )
				this.SubscribeHelper( ii, true );

			_connector = newConnector;
			_resource = newResource;

			if ( null != newCalendars )
			{
				foreach ( ResourceCalendar ii in newCalendars )
					this.SubscribeHelper( ii, false );
			}

			// SSP 4/16/11 TFS63818
			// When CurrentUser is changed to a different user or if a calendar from the current user is removed
			// then remove the corresponding reminders.
			// 
			// --------------------------------------------------------------------------------------------------
			var remindersToRemove = from ii in _activeReminders
									let activity = ii.Context as ActivityBase
									where null != activity && !_subscribedCalendars.Contains( activity.OwningCalendar )
									select ii;

			remindersToRemove = remindersToRemove.ToList( );

			ScheduleUtilities.RemoveAllItems( _activeReminders, remindersToRemove );
			// --------------------------------------------------------------------------------------------------
		}

		private void CommitActivityAsync( ActivityBase activity )
		{
			if ( null == _pendingCommitActivities )
				_pendingCommitActivities = new HashSet<ActivityBase>( );

			_pendingCommitActivities.Add( activity );

			if ( null == _commitActivityOperation )
			{
				_commitActivityOperation = new DeferredOperation( this.CommitPendingActivities );
				_commitActivityOperation.StartAsyncOperation( );
			}
		}

		private void CommitPendingActivities( )
		{
			if ( null != _commitActivityOperation )
			{
				_commitActivityOperation.CancelPendingOperation( );
				_commitActivityOperation = null;
			}

			HashSet<ActivityBase> activities = _pendingCommitActivities;
			_pendingCommitActivities = null;

			if ( null != activities )
			{
				foreach ( ActivityBase ii in activities )
					_owner.CommitActivityHelper( ii );
			}
		}

		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged( object sender, string property, object extraInfo )
		{
			
			

			bool rehook = false;
			ActivityBase activity = null;
			ActivityBase activityDeleted = null;
			ReminderInfo dismissedChangedReminder = null;

			if ( sender is ReminderInfo )
			{
				ReminderInfo reminderInfo = (ReminderInfo)sender;
				activity = null != reminderInfo ? reminderInfo.Context as ActivityBase : null;
				Debug.Assert( null != activity, "No activity context!" );

				if ( "IsDismissed" == property )
				{
					dismissedChangedReminder = reminderInfo;

					if ( reminderInfo.IsDismissed )
						activity.ReminderEnabled = false;
				}

				if ( null != activity )
				{
					ActivityBase commitActivity = null;

					if ( activity.IsOccurrence && !activity.IsVariance )
					{
						// If UserData or Text changes, then commit the occurrence and make it a variance.
						// 
						if ( "UserData" == property || "Text" == property )
						{
							commitActivity = activity;
						}
						// Otherwise if the reminder is dismissed then update the root activity's reminder's 
						// LastDisplayedTime to the occurrence's start. We'll use that to not re-display the
						// reminder for the occurrence (including past occurrences).
						// 
						else if ( "IsDismissed" == property )
						{
							ActivityBase rootActivity = activity.RootActivity;
							Debug.Assert( null != rootActivity );
							if ( null != rootActivity )
							{
								Reminder reminder = rootActivity.Reminder;
								if ( null == reminder )
									rootActivity.Reminder = reminder = new Reminder( );

								if ( reminder.LastDisplayedTime < activity.Start )
								{
									reminder.LastDisplayedTime = activity.Start;
									commitActivity = rootActivity;
								}
							}
						}
					}
					else
					{
						commitActivity = activity;
					}

					if ( null != commitActivity )
						this.CommitActivityAsync( commitActivity );
				}
			}
			// MD 1/20/11
			// Found while implementing NA 11.1 - Exchange Data Connector
			// The activity should also be removed if the ReminderEnabled proeprty is set to False.
			//else if ( sender is ActivityBase && "IsDeleted" == property )
			//{
			//    activity = (ActivityBase)sender;
			//    activityDeleted = activity;
			//}
			else if (sender is ActivityBase )
			{
				activity = (ActivityBase)sender;

				if ("IsDeleted" == property)
					activityDeleted = activity;
				else if ("ReminderEnabled" == property && activity.ReminderEnabled == false)
					activityDeleted = activity;
			}
			else if ( sender == _owner && ( "DataConnector" == property || "CurrentUser" == property ) )
			{
				rehook = true;
			}
			else if ( sender == _resource && "Calendars" == property || null != _resource && sender == _resource.Calendars )
			{
				rehook = true;
			}

			ReminderInfo reminderToDelete = null;

			// If an activity is deleted, remove the associated reminder from the active reminders collection.
			// 
			if ( null != activityDeleted )
				reminderToDelete = _activeReminders.FirstOrDefault( ii => ii.Context == activityDeleted );
			// When a reminder is dismissed, remove the reminder from the active reminders collection.
			// 
			else if ( null != dismissedChangedReminder && dismissedChangedReminder.IsDismissed )
				reminderToDelete = dismissedChangedReminder;

			if ( null != reminderToDelete )
				_activeReminders.Remove( reminderToDelete );

			if ( rehook )
				this.Resubscribe( );
		}
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