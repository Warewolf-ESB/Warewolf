using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules
{
	internal class InitializeUserHelper
	{
		#region Member Variables

		private ErrorCallback _onError;
		private ErrorCallback _onErrorResolvingAdditionalInfo;
		private Action<ExchangeService> _onGetDefaultFoldersCompleted;
		private Action<WorkingHours> _onGetWorkingHoursCompleted;
		private Action<string, string> _onResolveUserInfoCompleted;

		// MD 10/21/11 - TFS87807
		// We need to keep track of the IsLocked setting on the resource because we will change it so the user can't step on 
		// anything while it is loading.
		private bool _originalResourceIsLocked;

		private Resource _resource;
		private ExchangeService _service;

		#endregion  // Member Variables

		#region Constructor

		private InitializeUserHelper(Resource resource, ExchangeService service)
		{
			_resource = resource;
			_service = service;

			// MD 10/21/11 - TFS87807
			// We need to keep track of the IsLocked setting on the resource because we will change it so the user can't step on 
			// anything while it is loading.
			_originalResourceIsLocked = _resource.IsLocked;
			_resource.IsLocked = true;

			_onError = this.OnError;
			_onErrorResolvingAdditionalInfo = this.OnErrorResolvingAdditionalInfo;
			_onGetDefaultFoldersCompleted = this.OnGetDefaultFoldersCompleted;
			_onGetWorkingHoursCompleted = this.OnGetWorkingHoursCompleted;
			_onResolveUserInfoCompleted = this.OnResolveUserInfoCompleted;
		} 

		#endregion  // Constructor

		#region Methods

		#region CreateResourceCalendar

		private static ResourceCalendar CreateResourceCalendar(ExchangeFolder folder)
		{
			ResourceCalendar resourceCalendar = new ResourceCalendar();

			resourceCalendar.Name = folder.Name;
			resourceCalendar.Id = folder.FolderId.Id;
			resourceCalendar.DataItem = folder;

			folder.AssociatedCalendar = resourceCalendar;

			ExchangeService service = folder.Service;
			if (service.CalendarFolder == folder)
			{
				if (service.JournalFolder != null)
					service.JournalFolder.AssociatedCalendar = resourceCalendar;

				if (service.TasksFolder != null)
					service.TasksFolder.AssociatedCalendar = resourceCalendar;
			}

			return resourceCalendar;
		}

		#endregion  // CreateResourceCalendar

		#region Execute

		public static void Execute(Resource resource, ExchangeService service)
		{
			InitializeUserHelper helper = new InitializeUserHelper(resource, service);
			helper._service.GetDefaultFolders(helper._onGetDefaultFoldersCompleted, helper._onError);

			ExchangeScheduleDataConnector connector = helper._service.Connector;
			if (connector.IsResourceFeatureSupported(resource, ResourceFeature.CustomActivityCategories))
				helper._service.GetCategories(null, connector.DefaultErrorCallback);
		}

		#endregion  // Execute

		#region OnError

		private bool OnError(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			_service.Connector.PendingConnections.Remove(_service);

			// The exchange service may be in the cancelled connections collection multiple times, so remove all of them.
			ExchangeConnectorUtilities.RemoveAll(_service.Connector.CancelledConnections, _service);

			return _service.Connector.DefaultErrorCallback(reason, serverResponseCode, error);
		}

		#endregion  // OnError

		#region OnErrorResolvingAdditionalInfo

		public bool OnErrorResolvingAdditionalInfo(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			this.OnUserInitialized();
			return _service.Connector.DefaultErrorCallback(reason, serverResponseCode, error);
		}

		#endregion  // OnErrorResolvingAdditionalInfo

		#region OnGetDefaultFoldersCompleted

		private void OnGetDefaultFoldersCompleted(ExchangeService service)
		{
			Debug.Assert(service == _service, "Call received for wrong service.");

			_service.Connector.PendingConnections.Remove(_service);
			if (_service.Connector.CancelledConnections.Contains(_service))
			{
				// The exchange service may be in the cancelled connections collection multiple times, so remove all of them.
				ExchangeConnectorUtilities.RemoveAll(_service.Connector.CancelledConnections, _service);
				return;
			}

			ResourceCalendar defaultCalendar = InitializeUserHelper.CreateResourceCalendar(service.CalendarFolder);
			defaultCalendar.InitializeResource(_resource);
			_resource.Calendars.Add(defaultCalendar);
			_resource.PrimaryCalendar = defaultCalendar;
			_resource.PrimaryCalendarId = defaultCalendar.Id;

			if (service.NonDefaultFolders != null)
			{
				foreach (ExchangeFolder childFolder in service.NonDefaultFolders)
				{
					ResourceCalendar childResourceCalendar = InitializeUserHelper.CreateResourceCalendar(childFolder);
					childResourceCalendar.InitializeResource(_resource);
					_resource.Calendars.Add(childResourceCalendar);
				}
			}

			// MD 10/21/11 - TFS87807
			// We should add the resource to the collection as soon as it is populated with calendars. 
			// The other information can be added in later. This will speed up the initial loading time.
			_service.Connector.ResourcesInternal.Add(_resource);
			_service.Connector.ExchangeServices.Add(_service);

			string userName;


			if (_service.AssociatedUser == null)
				userName = Environment.UserName;
			else

				userName = _service.AssociatedUser.UserName;

			service.ResolveUserInfo(userName, _onResolveUserInfoCompleted, _onErrorResolvingAdditionalInfo);
		}

		#endregion  // OnGetDefaultFoldersCompleted 

		#region OnGetWorkingHoursCompleted

		private void OnGetWorkingHoursCompleted(WorkingHours workingHours)
		{
			Debug.Assert(workingHours.WorkingPeriodArray.Length == 1, "There should only be one working period.");
			if (workingHours.WorkingPeriodArray.Length > 0)
			{
				WorkingPeriod workingPeriod = workingHours.WorkingPeriodArray[0];
				TimeRange range = new TimeRange(
					TimeSpan.FromMinutes(workingPeriod.StartTimeInMinutes),
					TimeSpan.FromMinutes(workingPeriod.EndTimeInMinutes));

				_resource.DaysOfWeek = new ScheduleDaysOfWeek();

				DayOfWeek[] workingDays = EWSUtilities.ParseDaysOfWeek(workingPeriod.DayOfWeek);
				for (int i = 0; i < workingDays.Length; i++)
				{
					ScheduleDayOfWeek dayOfWeekSettings = new ScheduleDayOfWeek();
					dayOfWeekSettings.DaySettings = new DaySettings();
					dayOfWeekSettings.DaySettings.IsWorkday = true;
					dayOfWeekSettings.DaySettings.WorkingHours = new WorkingHoursCollection();
					dayOfWeekSettings.DaySettings.WorkingHours.Add(range);
					_resource.DaysOfWeek[workingDays[i]] = dayOfWeekSettings;
				}

				List<DayOfWeek> nonWorkingDays = EWSUtilities.GetDaysNotInGroup(workingDays);
				for (int i = 0; i < nonWorkingDays.Count; i++)
				{
					ScheduleDayOfWeek dayOfWeekSettings = new ScheduleDayOfWeek();
					dayOfWeekSettings.DaySettings = new DaySettings();
					dayOfWeekSettings.DaySettings.IsWorkday = false;
					_resource.DaysOfWeek[nonWorkingDays[i]] = dayOfWeekSettings;
				}
			}

			this.OnUserInitialized();
		} 

		#endregion  // OnGetWorkingHoursCompleted

		#region OnResolveUserInfoCompleted

		private void OnResolveUserInfoCompleted(string displayName, string emailAddress)
		{
			_resource.EmailAddress = emailAddress;
			_resource.Name = displayName;

			if (emailAddress == null || _service.Connector.UseServerWorkingHours == false)
			{
				this.OnUserInitialized();
			}
			else
			{
				_service.GetWorkingHours(emailAddress, _service.Connector.TimeZoneInfoProviderResolved,
					_onGetWorkingHoursCompleted,
					_onErrorResolvingAdditionalInfo);
			}
		} 

		#endregion  // OnResolveUserInfoCompleted

		#region OnUserInitialized

		private void OnUserInitialized()
		{
			// MD 10/21/11 - TFS87807
			// The resource is now added as soon as the the calendars are populated to speed up the loading time.
			// The user info and working hours can be added later. So all we have to do here, after that other info
			// is added, it reset the IsLocked property on the resource.
			//_service.Connector.ResourcesInternal.Add(_resource);
			//_service.Connector.ExchangeServices.Add(_service);
			_resource.IsLocked = _originalResourceIsLocked;
		}

		#endregion  // OnUserInitialized

		#endregion  // Methods
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