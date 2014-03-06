using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class QueryActivitiesHelper
	{
		#region Member Variables

		private ExchangeScheduleDataConnector _connector;
		private List<DataErrorInfo> _errors;
		private ExchangeActivityQueryResult _queryResult;
		private Dictionary<ResourceCalendar, List<DateRange>> _remainingRangesByCalendar;

		#endregion  // Member Variables

		#region Constructor

		private QueryActivitiesHelper(ExchangeScheduleDataConnector connector, ExchangeActivityQueryResult queryResult)
		{
			_connector = connector;
			_errors = new List<DataErrorInfo>();
			_queryResult = queryResult;
			_remainingRangesByCalendar = new Dictionary<ResourceCalendar, List<DateRange>>();
		}

		#endregion  // Constructor

		#region Methods

		#region Execute

		public static void Execute(ExchangeScheduleDataConnector connector, ExchangeActivityQueryResult queryResult)
		{
			ActivityQuery query = queryResult.Query;

			// MD 4/5/12 - TFS101338
			//new QueryActivitiesHelper(connector, queryResult).GetActivities(query.ActivityTypesToQuery, query.Calendars, query.DateRanges);
			new QueryActivitiesHelper(connector, queryResult).GetActivities(
				query.ActivityTypesToQuery,
				connector.GetCalendars(query), 
				query.DateRanges);
		}

		#endregion  // Execute

		#region GetActivities

		private void GetActivities(ActivityTypes activityTypesToQuery, IEnumerable<ResourceCalendar> calendars, IEnumerable<DateRange> dateRanges)
		{
			foreach (ResourceCalendar resourceCalendar in calendars)
			{
				_remainingRangesByCalendar[resourceCalendar] = new List<DateRange>(dateRanges);

				foreach (DateRange range in dateRanges)
				{
					CalendarDateRangeQuery calendarDateRangeQuery = new CalendarDateRangeQuery(this, resourceCalendar, range);

					ExchangeFolder folder = resourceCalendar.DataItem as ExchangeFolder;
					if (folder == null)
					{
						ExchangeConnectorUtilities.DebugFail("Could not get the ExchangeFolder associated with the ResourceCalendar.");
						return;
					}

					if ((activityTypesToQuery & ActivityTypes.Appointment) != 0)
						calendarDateRangeQuery.GetAppointmentsInRange(folder);

					ExchangeService service = folder.Service;

					// If this is the default calendar, add in the journals and tasks as well.
					if (folder == service.CalendarFolder)
					{
						if ((activityTypesToQuery & ActivityTypes.Journal) != 0)
							calendarDateRangeQuery.GetAppointmentsInRange(service.JournalFolder);

						if ((activityTypesToQuery & ActivityTypes.Task) != 0)
							calendarDateRangeQuery.GetAppointmentsInRange(service.TasksFolder);
					}
				}
			}

			// MD 4/5/12
			// Found while fixing TFS101338
			// If there are no calendars pending, none were passed in, so mark the result complete.
			if (_remainingRangesByCalendar.Count == 0)
				_queryResult.InitializeResult(ActivityQueryRequestedDataFlags.None, null, null, null, null, null, null, true);
		}

		#endregion  // GetActivities

		#region ManageQueryResult

		private void ManageQueryResult(ResourceCalendar resourceCalendar, DateRange? range)
		{
			if (range.HasValue)
			{
				List<DateRange> remainingRanges;
				if (_remainingRangesByCalendar.TryGetValue(resourceCalendar, out remainingRanges) == false)
					return;

				remainingRanges.Remove(range.Value);

				if (remainingRanges.Count == 0)
					_remainingRangesByCalendar.Remove(resourceCalendar);

				if (_remainingRangesByCalendar.Count != 0)
					return;
			}

			DataErrorInfo dataError = null;

			if (_errors.Count == 1)
				dataError = _errors[0];
			else if (_errors.Count > 1)
				dataError = DataErrorInfo.CreateFromList(_errors);

			_queryResult.InitializeResult(
				ActivityQueryRequestedDataFlags.None, null, null, null, null, null,
				dataError, true);
		}

		#endregion  // ManageQueryResult

		#endregion  // Methods

		#region CalendarDateRangeQuery class

		public class CalendarDateRangeQuery
		{
			#region Member Variables

			private int _pendingCallCount;
			private ErrorCallback _onError;
			private Action<ExchangeFolder, IList<ItemType>> _onGetAppointmentsInRangeCompleted;
			private Action _onRemoteCallProcessed;
			private DateRange _range;
			private ResourceCalendar _resourceCalendar;
			private QueryActivitiesHelper _queryActivitiesHelper;

			#endregion  // Member Variables

			#region Constructor

			public CalendarDateRangeQuery(
				QueryActivitiesHelper queryActivitiesHelper,
				ResourceCalendar resourceCalendar,
				DateRange range)
			{
				_range = range;
				_resourceCalendar = resourceCalendar;
				_queryActivitiesHelper = queryActivitiesHelper;

				_onError = this.OnError;
				_onGetAppointmentsInRangeCompleted = this.OnGetAppointmentsInRangeCompleted;
				_onRemoteCallProcessed = this.OnRemoteCallProcessed;
			}

			#endregion  // Constructor

			#region Methods

			#region GetAppointmentsInRange

			public void GetAppointmentsInRange(ExchangeFolder folder)
			{
				if (folder == null)
					return;

				_pendingCallCount++;
				folder.GetAppointmentsInRange(_range, _onGetAppointmentsInRangeCompleted, _onError);
			}

			#endregion  // GetAppointmentsInRange

			#region OnError

			private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
			{
				_queryActivitiesHelper._errors.Add(_queryActivitiesHelper._connector.GetDataErrorInfo(reason, error));
				this.OnRemoteCallProcessed();
				return false;
			}

			#endregion  // OnError

			#region OnGetAppointmentsInRangeCompleted

			private void OnGetAppointmentsInRangeCompleted(ExchangeFolder folder, IList<ItemType> items)
			{
				List<ActivityBase> recurringActivityRoots;
				_queryActivitiesHelper._connector.OnItemsReceivedFromServer(
					folder, items,
					ref _pendingCallCount, out recurringActivityRoots,
					_onRemoteCallProcessed, _onError);

				this.OnRemoteCallProcessed();
			}

			#endregion  // OnGetAppointmentsInRangeCompleted

			#region OnRemoteCallProcessed

			private void OnRemoteCallProcessed()
			{
				_pendingCallCount--;
				Debug.Assert(_pendingCallCount >= 0, "The pending call count should never be less than 0");

				if (_pendingCallCount == 0)
					_queryActivitiesHelper.ManageQueryResult(_resourceCalendar, _range);
			}

			#endregion  // OnRemoteCallProcessed

			#endregion  // Methods
		}

		#endregion  // CalendarDateRangeQuery class
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