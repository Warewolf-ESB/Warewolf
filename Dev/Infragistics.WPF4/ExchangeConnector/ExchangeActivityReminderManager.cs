using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class ExchangeActivityReminderManager : ActivityReminderManager
	{
		#region Constructor

		internal ExchangeActivityReminderManager(ExchangeScheduleDataConnector connector)
			: base(connector) { } 

		#endregion  // Constructor

		#region PerformRemindersQuery

		protected override ActivityQueryResult PerformRemindersQuery()
		{
			ExchangeScheduleDataConnector connector = this.Connector;
			IEnumerable<ResourceCalendar> calendars = this.ReminderSubscribers.Keys;

			DateTime now = DateTime.UtcNow;
			DateRange range = new DateRange(now, now.Add(QUERY_INTERVAL));
			ActivityQuery query = new ActivityQuery( ActivityTypes.Appointment | ActivityTypes.Task, range, calendars );

			ExchangeActivityQueryResult queryResult = new ExchangeActivityQueryResult(query, true);
			connector.OnActivityQueryResultCreated(queryResult);

			queryResult.InitializeResult(
				ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges, queryResult.ActivitiesInternal, 
				null, null, null, null, null, false);

			QueryRemindersHelper.QueryReminders(this, queryResult);

			return queryResult;
		} 

		#endregion  // PerformRemindersQuery

		#region Connector

		internal ExchangeScheduleDataConnector Connector
		{
			get
			{
				return (ExchangeScheduleDataConnector)_connector;
			}
		}

		#endregion // Connector


		#region QueryRemindersHelper class

		private class QueryRemindersHelper
		{
			#region Member Variables

			private DataErrorInfo _errorInfo;
			private ErrorCallback _onError;
			private Action<ExchangeFolder, IList<ItemType>> _onOccurrenceReceivedFromServer;
			private Action<ExchangeFolder, IList<ItemType>> _onRemindersReceivedFromServer;
			private Action _onRemoteCallProcessed;
			private int _pendingCallCount;
			private ExchangeActivityQueryResult _queryResult;
			private ExchangeActivityReminderManager _reminderManager; 

			#endregion  // Member Variables

			#region Constructor

			private QueryRemindersHelper(ExchangeActivityReminderManager reminderManager, ExchangeActivityQueryResult queryResult)
			{
				_reminderManager = reminderManager;
				_queryResult = queryResult;

				_onError = this.OnError;
				_onOccurrenceReceivedFromServer = this.OnOccurrenceReceivedFromServer;
				_onRemindersReceivedFromServer = this.OnRemindersReceivedFromServer;
				_onRemoteCallProcessed = this.OnRemoteCallProcessed;
			} 

			#endregion  // Constructor

			#region Methods

			#region OnError

			private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				DataErrorInfo errorInfo = _reminderManager.Connector.GetDataErrorInfo(reason, error);

				if (_errorInfo == null)
					_errorInfo = errorInfo;
				else if (_errorInfo.ErrorList != null)
					_errorInfo.ErrorList.Add(errorInfo);
				else
					_errorInfo = DataErrorInfo.CreateFromList(new List<DataErrorInfo>() { _errorInfo, errorInfo });

				this.OnRemoteCallProcessed();
				return false;
			}

			#endregion  // OnError

			#region OnOccurrenceReceivedFromServer

			private void OnOccurrenceReceivedFromServer(ExchangeFolder folder, IList<ItemType> items)
			{
				List<ActivityBase> recurringActivityRoots;
				_reminderManager.Connector.OnItemsReceivedFromServer(
					folder, items,
					ref _pendingCallCount, out recurringActivityRoots,
					_onRemoteCallProcessed, _onError);

				this.OnRemoteCallProcessed();
			}

			#endregion  // OnOccurrenceReceivedFromServer

			#region OnRecurringMasterReceivedFromServer

			private void OnRecurringMasterReceivedFromServer(ActivityBase activity)
			{
				if (activity.ReminderEnabled == false)
					return;

				ItemType reminderItem = activity.DataItem as ItemType;
				if (reminderItem == null)
					return;

				if (reminderItem.ReminderDueBySpecified == false || reminderItem.ReminderDueBy < DateTime.UtcNow)
					return;

				ResourceCalendar resourceCalendar = activity.OwningCalendar;
				if (resourceCalendar == null)
					return;

				ExchangeFolder folder = resourceCalendar.DataItem as ExchangeFolder;
				if (folder == null)
					return;

				DateRange range = new DateRange(
					reminderItem.ReminderDueBy.AddMinutes(-1),
					reminderItem.ReminderDueBy.AddMinutes(1));

				_pendingCallCount++;
				folder.GetAppointmentsInRange(range, _onOccurrenceReceivedFromServer, _onError);
			}

			#endregion  // OnRecurringMasterReceivedFromServer

			#region OnRemindersReceivedFromServer

			private void OnRemindersReceivedFromServer(ExchangeFolder folder,  IList<ItemType> reminderItems)
			{
				List<ActivityBase> recurringActivityRoots;
				_reminderManager.Connector.OnItemsReceivedFromServer(
					folder, reminderItems, 
					ref _pendingCallCount, out recurringActivityRoots,
					_onRemoteCallProcessed, _onError);

				for (int i = 0; i < recurringActivityRoots.Count; i++)
					this.OnRecurringMasterReceivedFromServer(recurringActivityRoots[i]);

				this.OnRemoteCallProcessed();
			}

			#endregion  // OnRemindersReceivedFromServer

			#region OnRemoteCallProcessed

			private void OnRemoteCallProcessed()
			{
				_pendingCallCount--;
				Debug.Assert(_pendingCallCount >= 0, "The pending call count should never be less than 0");

				if (_pendingCallCount <= 0)
					_queryResult.InitializeResult(ActivityQueryRequestedDataFlags.None, null, null, null, null, null, _errorInfo, true);
			}

			#endregion  // OnRemoteCallProcessed

			#region QueryReminders

			public static void QueryReminders(ExchangeActivityReminderManager reminderManager, ExchangeActivityQueryResult queryResult)
			{
				QueryRemindersHelper helper = new QueryRemindersHelper(reminderManager, queryResult);

				foreach (ExchangeService service in reminderManager.Connector.ExchangeServices)
				{
					helper._pendingCallCount++;
					service.FindReminders(helper._onRemindersReceivedFromServer, helper._onError);
				}
			}

			#endregion  // QueryReminders

			#endregion  // Methods
		} 

		#endregion  // QueryRemindersHelper class
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