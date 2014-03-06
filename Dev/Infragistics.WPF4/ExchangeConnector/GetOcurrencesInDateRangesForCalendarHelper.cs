using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class GetOcurrencesInDateRangesForCalendarHelper
	{
		#region Member Variables

		private ExchangeScheduleDataConnector _connector;
		private ErrorCallback _onError;
		private ErrorCallback _onErrorCallback;
		private Action _onQueryCompleted;
		private Action<ExchangeFolder, IList<ItemType>> _onOccurrencesReceived;
		private Action<ExchangeFolder, IList<ItemType>> _onReceivedOccurrencesFromServer;
		private int _pendingCallCount;
		private List<ActivityBase> _recurringActivityRoots;
		private List<ServerResponse> _serverResponses; 

		#endregion  // Member Variables

		#region Constructor

		private GetOcurrencesInDateRangesForCalendarHelper(
			ExchangeScheduleDataConnector connector,
			List<ActivityBase> recurringActivityRoots,
			Action<ExchangeFolder, IList<ItemType>> onOccurrencesReceived,
			Action onQueryCompleted,
			ErrorCallback onError)
		{
			_connector = connector;
			_onErrorCallback = onError;
			_onQueryCompleted = onQueryCompleted;
			_onOccurrencesReceived = onOccurrencesReceived;
			_recurringActivityRoots = recurringActivityRoots;
			_serverResponses = new List<ServerResponse>();

			_onError = this.OnError;
			_onReceivedOccurrencesFromServer = this.OnReceivedOccurrencesFromServer;
		} 

		#endregion  // Constructor

		#region Methods

		#region AfterServerResponseReceived

		private void AfterServerResponseReceived()
		{
			_pendingCallCount--;
			Debug.Assert(_pendingCallCount >= 0, "The pending call count should never be less than 0");

			if (_pendingCallCount > 0)
				return;

			this.OnAllResponsesReceived();
		}

		#endregion  // AfterServerResponseReceived

		#region Execute

		public static void Execute(ExchangeScheduleDataConnector connector,
			List<ActivityBase> recurringActivityRoots,
			Action<ExchangeFolder, IList<ItemType>> onOccurrencesReceived,
			Action onQueryCompleted,
			ErrorCallback onError)
		{
			GetOcurrencesInDateRangesForCalendarHelper helper = new GetOcurrencesInDateRangesForCalendarHelper(
				connector,
				recurringActivityRoots,
				onOccurrencesReceived,
				onQueryCompleted,
				onError);

			// If any recurring activities were added or modified, they may have new occurrences that should be added into
			// the existing query results. So we basically just have to requery for items in all ranges and folders which
			// could contain occurrences of the changes recurring items.
			IEnumerable<DateRange> dateRanges;
			IEnumerable<ResourceCalendar> calendars;
			connector.GetDateRangesAndCalendarsWhichMayHaveOccurrences(recurringActivityRoots, out dateRanges, out calendars);

			foreach (ResourceCalendar calendar in calendars)
			{
				ExchangeFolder folder = calendar.DataItem as ExchangeFolder;

				if (folder == null)
				{
					ExchangeConnectorUtilities.DebugFail("Could not get the ExchangeFolder associated with the ResourceCalendar.");
					continue;
				}

				foreach (DateRange dateRange in dateRanges)
					helper.GetAppointmentsInRange(folder, dateRange);
			}

			if (helper._pendingCallCount == 0)
				helper.OnAllResponsesReceived();
		} 

		#endregion //Execute

		#region GetAppointmentsInRange

		private void GetAppointmentsInRange(ExchangeFolder folder, DateRange dateRange)
		{
			_pendingCallCount++;
			folder.GetAppointmentsInRange(dateRange, _onReceivedOccurrencesFromServer, _onError);
		}

		#endregion  // GetAppointmentsInRange

		#region OnAllResponsesReceived

		private void OnAllResponsesReceived()
		{
			// Remove the occurrences for all recurring root activities. If the occurrences still exist, 
			// they will be readded next in the _onOccurrencesReceived call below.
			foreach (ActivityBase recurringRoot in _recurringActivityRoots)
			{
				// MD 2/24/11 - TFS66986
				// Not all recurring root activities are considered recurring masters for occurrences.
				// Tasks do not have occurrences, so we should not do the removal below because it would
				// remove the root task from the UI.
				if (ExchangeConnectorUtilities.IsRecurringMaster(recurringRoot) == false)
					continue;

				if (recurringRoot.IsAddNew == false)
					_connector.VerifyItemsInQueryResults(recurringRoot, false, false);
			}

			for (int i = 0; i < _serverResponses.Count; i++)
			{
				ServerResponse serverResponse = _serverResponses[i];
				_onOccurrencesReceived(serverResponse.Folder, serverResponse.Items);
			}

			if (_onQueryCompleted != null)
				_onQueryCompleted();
		}

		#endregion //OnAllResponsesReceived

		#region OnError

		public bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			this.AfterServerResponseReceived();

			if (_onErrorCallback != null)
				return _onErrorCallback(reason, serverResponseCode, error);

			return _connector.DefaultErrorCallback(reason, serverResponseCode, error);
		}

		#endregion  // OnError

		#region OnReceivedOccurrencesFromServer

		public void OnReceivedOccurrencesFromServer(ExchangeFolder folder, IList<ItemType> items)
		{
			if (items.Count != 0)
			{
				ServerResponse serverResponse = new ServerResponse();
				serverResponse.Folder = folder;
				serverResponse.Items = items;
				_serverResponses.Add(serverResponse);
			}

			this.AfterServerResponseReceived();
		}

		#endregion  // OnReceivedOccurrencesFromServer

		#endregion  // Methods


		#region ServerResponse class

		private class ServerResponse
		{
			public ExchangeFolder Folder;
			public IList<ItemType> Items;
		} 

		#endregion  // ServerResponse class
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