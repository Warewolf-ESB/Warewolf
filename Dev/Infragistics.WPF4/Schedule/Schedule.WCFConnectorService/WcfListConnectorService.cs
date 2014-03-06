using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Windows;
using Infragistics.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services.Schedules
{
	/// <summary>
	/// Base class for WCF services used for providing schedule data to a client over WCF services.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// There are two classes derived from this base class which cover most scenarios. <see cref="WcfListConnectorServiceSingle"/>
	/// is a single instance, single threaded WCF service. This service is ideal when a relatively small number of clients will be
	/// connecting to the service. The data sources can be initialized once when the service is first used, which gives a performance
	/// boost on subsequent remote calls. However, because all calls are processed on the same thread, having too many clients could 
	/// cause a bottleneck, especially if one of the clients requires a large amount of data from the service. When many clients will
	/// be connecting to the service, the <see cref="WcfListConnectorServiceMulti"/> is a better choice. Each remote call is processed 
	/// on a different thread and with a different service instance. Because of this, the data sources must be thread safe, such as a 
	/// SQL database. Also, because a new service instance is needed for each call, some work must be done internally to hook up to 
	/// the data sources for each remote call. This will cause a slight performance hit for each call over the 
	/// WcfListConnectorServiceSingle.
	/// </p>
	/// </remarks>
	/// <seealso cref="WcfListConnectorServiceSingle"/>
	/// <seealso cref="WcfListConnectorServiceMulti"/>
	public 

	abstract

	class WcfListConnectorService : IWcfListConnectorService, IScheduleDataConnector
	{
		#region Member Variables

		[ThreadStatic]
		private static ObjectSerializer _linqStatementSerializer;

		// MD 2/9/11 - TFS65718
		internal ActivityCategoryListManager _activityCategoryListManager;

		internal AppointmentListManager _appointmentListManager;
		internal JournalListManager _journalListManager;
		internal AppointmentListManager _recurringAppointmentListManager;
		internal TaskListManager _taskListManager;
		internal ResourceListManager _resourceListManager;
		internal ResourceCalendarListManager _resourceCalendarListManager; 

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="WcfListConnectorService"/> instance.
		/// </summary>



		protected

			WcfListConnectorService()
		{
			
		}

		#endregion  // Constructor

		#region IScheduleDataConnector Members

		void IScheduleDataConnector.NotifyListeners(object sender, string property, object extraInfo) { }

		void IScheduleDataConnector.OnError(DataErrorInfo error)
		{
			
		}

		RecurrenceCalculatorFactoryBase IScheduleDataConnector.RecurrenceCalculatorFactoryResolved
		{
			get { return null; }
		}

		ResourceCollection IScheduleDataConnector.ResourceItems
		{
			get { return ((ResourceListManager)this.GetListManager(ItemSourceType.Resource)).Resources; }
		}

		TimeZoneInfoProvider IScheduleDataConnector.TimeZoneInfoProviderResolved
		{
			get { return null; }
		}

		bool IScheduleDataConnector.IsActivityOperationSupported( ActivityBase activity, ActivityOperation operation )
		{
			
			return true;
		}

		#endregion

		#region IWCFScheduleConnectorService Members

		GetInitialInfoResult IWcfListConnectorService.GetInitialInfo(GetInitialInfoContext context)
		{
			this.OnRemoteCallReceived(context);

			GetInitialInfoResult result = new GetInitialInfoResult();

			try
			{
				this.RaiseValidateSecurityToken(context);

				ResourceListManager resourceListManager = null;
				ResourceCalendarListManager resourceCalendarListManager = null;

				// MD 2/9/11 - TFS65718
				ActivityCategoryListManager activityCategoryListManager = null;

				if ((context.RequestedInfo & InitialInfoTypes.ResourceItemSource) != 0)
				{
					resourceListManager = (ResourceListManager)this.GetListManager(ItemSourceType.Resource);

					IEnumerable resourceItemsSourceLocal = this.GetList(ItemSourceType.Resource);

					List<ResourceSerializableItem> serializableResources = new List<ResourceSerializableItem>();
					if (resourceItemsSourceLocal != null)
					{
						foreach (object resource in resourceItemsSourceLocal)
						{
							serializableResources.Add(
								this.CreateSerializableType<ResourceSerializableItem, Resource, ResourceProperty>(resource, resourceListManager)
								);
						}
					}
					result.ResourceItemsSource = serializableResources;
					result.ResourceItemsVersionInfo = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.Resource));
				}

				if ((context.RequestedInfo & InitialInfoTypes.ResourceCalendarItemSource) != 0)
				{
					resourceCalendarListManager = (ResourceCalendarListManager)this.GetListManager(ItemSourceType.ResourceCalendar);

					IEnumerable resourceCalendarItemsSourceLocal = this.GetList(ItemSourceType.ResourceCalendar);

					List<ResourceCalendarSerializableItem> serializableResourceCalendars = new List<ResourceCalendarSerializableItem>();
					if (resourceCalendarItemsSourceLocal != null)
					{
						foreach (object resourceCalendar in resourceCalendarItemsSourceLocal)
						{
							serializableResourceCalendars.Add(
								this.CreateSerializableType<ResourceCalendarSerializableItem, ResourceCalendar, ResourceCalendarProperty>(resourceCalendar, resourceCalendarListManager)
								);
						}
					}
					result.ResourceCalendarItemsSource = serializableResourceCalendars;
					result.ResourceCalendarItemsVersionInfo = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.ResourceCalendar));
				}

				// MD 2/9/11 - TFS65718
				if ((context.RequestedInfo & InitialInfoTypes.ActivityCategoryItemSource) != 0)
				{
					activityCategoryListManager = (ActivityCategoryListManager)this.GetListManager(ItemSourceType.ActivityCategory);

					IEnumerable activityCategoryItemsSourceLocal = this.GetList(ItemSourceType.ActivityCategory);

					List<ActivityCategorySerializableItem> serializableActivityCategories = new List<ActivityCategorySerializableItem>();
					if (activityCategoryItemsSourceLocal != null)
					{
						foreach (object activityCategory in activityCategoryItemsSourceLocal)
						{
							serializableActivityCategories.Add(
								this.CreateSerializableType<ActivityCategorySerializableItem, ActivityCategory, ActivityCategoryProperty>(activityCategory, activityCategoryListManager)
								);
						}
					}
					result.ActivityCategoryItemsSource = serializableActivityCategories;
					result.ActivityCategoryItemsVersionInfo = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.ActivityCategory));
				}

				if ((context.RequestedInfo & InitialInfoTypes.ItemSourceInfos) != 0)
				{
					result.ItemSourceInfos = new Dictionary<ItemSourceType, ItemSourceInfo>();

					// MD 2/9/11 - TFS65718
					ItemSourceInfo itemSourceInfo;
					itemSourceInfo = this.CreateItemSourceInfo<ActivityCategory, ActivityCategoryProperty>(ItemSourceType.ActivityCategory);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.ActivityCategory] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<ActivityBase, AppointmentProperty>(ItemSourceType.Appointment);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.Appointment] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<ActivityBase, JournalProperty>(ItemSourceType.Journal);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.Journal] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<ActivityBase, AppointmentProperty>(ItemSourceType.RecurringAppointment);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.RecurringAppointment] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<ResourceCalendar, ResourceCalendarProperty>(ItemSourceType.ResourceCalendar);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.ResourceCalendar] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<Resource, ResourceProperty>(ItemSourceType.Resource);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.Resource] = itemSourceInfo;

					itemSourceInfo = this.CreateItemSourceInfo<ActivityBase, TaskProperty>(ItemSourceType.Task);
					if (itemSourceInfo != null)
						result.ItemSourceInfos[ItemSourceType.Task] = itemSourceInfo;
				}
			}
			catch (Exception exc)
			{
				result.ErrorInfo = WcfListConnectorService.GetErrorInfo(exc);
			}
			finally
			{
				this.OnRemoteCallProcessed(result);
			}

			return result;
		}

		PerformActivityOperationResult IWcfListConnectorService.PerformActivityOperation(PerformActivityOperationContext context)
		{
			this.OnRemoteCallReceived(context);

			PerformActivityOperationResult result = new PerformActivityOperationResult();

			try
			{
				this.RaiseValidateSecurityToken(context);

				AppointmentSerializableItem appointment = context.Activity as AppointmentSerializableItem;
				if (appointment != null)
				{
					AppointmentListManager listManager;
					AppointmentListManager recurringAppointmentListManager = (AppointmentListManager)this.GetListManager(ItemSourceType.RecurringAppointment);
					if ((string.IsNullOrEmpty(appointment.Recurrence) == false || string.IsNullOrEmpty(appointment.RootActivityId) == false) &&
						recurringAppointmentListManager.HasRecurringActivities)
						listManager = recurringAppointmentListManager;
					else
						listManager = (AppointmentListManager)this.GetListManager(ItemSourceType.Appointment);

					AppointmentSerializableItem updatedItemToSendToClient;
					this.PerformActivityOperation(
						appointment,
						context.Operation,
						listManager,
						AppointmentProperty.Id,
						context.ClientVersionInfo,
						out result.NewVersionInfo,
						out result.VersionWasOutOfDate,
						out updatedItemToSendToClient);

					result.UpdatedActivityIfChanged = updatedItemToSendToClient;
					return result;
				}

				JournalSerializableItem journal = context.Activity as JournalSerializableItem;
				if (journal != null)
				{
					JournalSerializableItem updatedItemToSendToClient;
					this.PerformActivityOperation(
						journal,
						context.Operation,
						(JournalListManager)this.GetListManager(ItemSourceType.Journal),
						JournalProperty.Id,
						context.ClientVersionInfo,
						out result.NewVersionInfo,
						out result.VersionWasOutOfDate,
						out updatedItemToSendToClient);

					result.UpdatedActivityIfChanged = updatedItemToSendToClient;
					return result;
				}

				TaskSerializableItem task = context.Activity as TaskSerializableItem;
				if (task != null)
				{
					TaskSerializableItem updatedItemToSendToClient;
					this.PerformActivityOperation(
						task,
						context.Operation,
						(TaskListManager)this.GetListManager(ItemSourceType.Task),
						TaskProperty.Id,
						context.ClientVersionInfo,
						out result.NewVersionInfo,
						out result.VersionWasOutOfDate,
						out updatedItemToSendToClient);

					result.UpdatedActivityIfChanged = updatedItemToSendToClient;
					return result;
				}

				Debug.Fail("Unknown serializable activity type: " + context.Activity.GetType().Name);
			}
			catch (Exception exc)
			{
				result.ErrorInfo = WcfListConnectorService.GetErrorInfo(exc);
			}
			finally
			{
				this.OnRemoteCallProcessed(result);
			}

			return result;
		}

		void IWcfListConnectorService.Ping() { }

		PollForItemSourceChangesResult IWcfListConnectorService.PollForItemSourceChanges(PollForItemSourceChangesContext context)
		{
			this.OnRemoteCallReceived(context);

			PollForItemSourceChangesResult result = new PollForItemSourceChangesResult();

			try
			{
				this.RaiseValidateSecurityToken(context);

				result.VersionInfos = new Dictionary<ItemSourceType, ListManagerVersionInfo>();

				result.VersionInfos[ItemSourceType.Appointment] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.Appointment));
				result.VersionInfos[ItemSourceType.Journal] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.Journal));
				result.VersionInfos[ItemSourceType.RecurringAppointment] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.RecurringAppointment));
				result.VersionInfos[ItemSourceType.ResourceCalendar] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.ResourceCalendar));
				result.VersionInfos[ItemSourceType.Resource] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.Resource));
				result.VersionInfos[ItemSourceType.Task] = new ListManagerVersionInfo(this.GetChangeInformation(ItemSourceType.Task));
			}
			catch (Exception exc)
			{
				result.ErrorInfo = WcfListConnectorService.GetErrorInfo(exc);
			}
			finally
			{
				this.OnRemoteCallProcessed(result);
			}

			return result;
		}

		PollForItemSourceChangesDetailedResult IWcfListConnectorService.PollForItemSourceChangesDetailed(PollForItemSourceChangesDetailedContext context)
		{
			this.OnRemoteCallReceived(context);

			PollForItemSourceChangesDetailedResult result = new PollForItemSourceChangesDetailedResult();

			try
			{
				this.RaiseValidateSecurityToken(context);

				result.DetailedChanges = new Dictionary<ItemSourceType, DetailedItemSourceChangeInfo>();

				foreach (KeyValuePair<ItemSourceType, ListManagerVersionInfo> clientVersionInfo in context.ClientItemSourceVersions)
				{
					DetailedItemSourceChangeInfo detailedChangeInfo;
					switch (clientVersionInfo.Key)
					{
						case ItemSourceType.Appointment:
							detailedChangeInfo = this.GetDetailedChangeInfo<AppointmentSerializableItem, ActivityBase, AppointmentProperty>(
								ItemSourceType.Appointment,
								clientVersionInfo.Value);
							break;

						case ItemSourceType.Journal:
							detailedChangeInfo = this.GetDetailedChangeInfo<JournalSerializableItem, ActivityBase, JournalProperty>(
								ItemSourceType.Journal,
								clientVersionInfo.Value);
							break;

						case ItemSourceType.RecurringAppointment:
							detailedChangeInfo = this.GetDetailedChangeInfo<AppointmentSerializableItem, ActivityBase, AppointmentProperty>(
								ItemSourceType.RecurringAppointment,
								clientVersionInfo.Value);
							break;

						case ItemSourceType.Resource:
							detailedChangeInfo = this.GetDetailedChangeInfo<ResourceSerializableItem, Resource, ResourceProperty>(
								ItemSourceType.Resource,
								clientVersionInfo.Value);
							break;

						case ItemSourceType.ResourceCalendar:
							detailedChangeInfo = this.GetDetailedChangeInfo<ResourceCalendarSerializableItem, ResourceCalendar, ResourceCalendarProperty>(
								ItemSourceType.ResourceCalendar,
								clientVersionInfo.Value);
							break;

						case ItemSourceType.Task:
							detailedChangeInfo = this.GetDetailedChangeInfo<TaskSerializableItem, ActivityBase, TaskProperty>(
								ItemSourceType.Task,
								clientVersionInfo.Value);
							break;

						default:
							Debug.Fail("Unknown ItemSourceType:" + clientVersionInfo.Key);
							continue;
					}

					if (detailedChangeInfo != null)
						result.DetailedChanges[clientVersionInfo.Key] = detailedChangeInfo;
				}
			}
			catch (Exception exc)
			{
				result.ErrorInfo = WcfListConnectorService.GetErrorInfo(exc);
			}
			finally
			{
				this.OnRemoteCallProcessed(result);
			}

			return result;
		}

		QueryActivitiesResult IWcfListConnectorService.QueryActivities(QueryActivitiesContext context)
		{
			this.OnRemoteCallReceived(context);

			QueryActivitiesResult result = new QueryActivitiesResult();

			try
			{
				this.RaiseValidateSecurityToken(context);

				switch (context.ListType)
				{
					case ActivityListType.Appointment:
						result.Activities = this.QueryActivities<AppointmentSerializableItem, Appointment, AppointmentProperty>(context.LinqStatementEncoded, (AppointmentListManager)this.GetListManager(ItemSourceType.Appointment));
						break;

					case ActivityListType.Journal:
						result.Activities = this.QueryActivities<JournalSerializableItem, Journal, JournalProperty>(context.LinqStatementEncoded, (JournalListManager)this.GetListManager(ItemSourceType.Journal));
						break;

					case ActivityListType.RecurringAppointment:
						result.Activities = this.QueryActivities<AppointmentSerializableItem, Appointment, AppointmentProperty>(context.LinqStatementEncoded, (AppointmentListManager)this.GetListManager(ItemSourceType.RecurringAppointment));
						break;

					case ActivityListType.Task:
						result.Activities = this.QueryActivities<TaskSerializableItem, Task, TaskProperty>(context.LinqStatementEncoded, (TaskListManager)this.GetListManager(ItemSourceType.Task));
						break;

					default:
						Debug.Fail("Unknown ActivityListType: " + context.ListType);
						break;
				}
			}
			catch (Exception exc)
			{
				result.ErrorInfo = WcfListConnectorService.GetErrorInfo(exc);
			}
			finally
			{
				this.OnRemoteCallProcessed(result);
			}

			return result;
		}

		#endregion

		#region Events

		#region ActivityAdded

		/// <summary>
		/// Raised after an activity is added.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityAdded</b> event is raised after a new activity is successfully commited.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityAddedEventArgs"/>
		/// <seealso cref="OnActivityAdded"/>
		public event EventHandler<ActivityAddedEventArgs> ActivityAdded;

		/// <summary>
		/// Used to invoke the <see cref="ActivityAdded"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityAdded"/>
		protected virtual void OnActivityAdded(ActivityAddedEventArgs args)
		{
			EventHandler<ActivityAddedEventArgs> handler = this.ActivityAdded;

			if (null != handler)
				handler(this, args);
		}

		#endregion  // ActivityAdded

		#region ActivityChanged

		/// <summary>
		/// Raised after user changes to an activity are committed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityChanged</b> event is raised after user changes to an 
		/// activity are committed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityChangedEventArgs"/>
		/// <seealso cref="OnActivityChanged"/>
		public event EventHandler<ActivityChangedEventArgs> ActivityChanged;

		/// <summary>
		/// Used to invoke the <see cref="ActivityChanged"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityChanged"/>
		protected virtual void OnActivityChanged(ActivityChangedEventArgs args)
		{
			EventHandler<ActivityChangedEventArgs> handler = this.ActivityChanged;

			if (null != handler)
				handler(this, args);
		}

		#endregion  // ActivityChanged

		#region ActivityRemoved

		/// <summary>
		/// Raised after an activity is removed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityRemoved</b> event is raised after an activity is removed by the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityRemovedEventArgs"/>
		/// <seealso cref="OnActivityRemoved"/>
		public event EventHandler<ActivityRemovedEventArgs> ActivityRemoved;

		/// <summary>
		/// Used to invoke the <see cref="ActivityRemoved"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityRemoved"/>
		protected virtual void OnActivityRemoved(ActivityRemovedEventArgs args)
		{
			EventHandler<ActivityRemovedEventArgs> handler = this.ActivityRemoved;

			if (null != handler)
				handler(this, args);
		}

		#endregion  // ActivityChanged

		#region ValidateSecurityToken

		/// <summary>
		/// Raised when a call is made on the service so the service can validate the caller.
		/// An exception should be thrown from the handler is the security information is invalid.
		/// </summary>
		/// <seealso cref="ValidateSecurityTokenEventArgs"/>
		/// <seealso cref="OnValidateSecurityToken"/>
		public event EventHandler<ValidateSecurityTokenEventArgs> ValidateSecurityToken;

		/// <summary>
		/// Used to invoke the <see cref="ValidateSecurityToken"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ValidateSecurityToken"/>
		protected virtual void OnValidateSecurityToken(ValidateSecurityTokenEventArgs args)
		{
			EventHandler<ValidateSecurityTokenEventArgs> handler = this.ValidateSecurityToken;

			if (null != handler)
				handler(this, args);
		}

		#region RaiseValidateSecurityToken

		private void RaiseValidateSecurityToken(CallContext context)
		{
			string securityToken = context == null
				? null
				: context.SecurityToken;

			this.OnValidateSecurityToken(new ValidateSecurityTokenEventArgs(securityToken));
		}

		#endregion  // RaiseValidateSecurityToken

		#endregion  // ValidateSecurityToken

		#endregion  // Events

		#region Methods

		#region Protected Methods

		#region OnRemoteCallProcessed

		/// <summary>
		/// Occurs when a remote call is finished processing and it about to return to the client.
		/// </summary>
		protected virtual void OnRemoteCallProcessed(CallResult result) { }

		#endregion  // OnRemoteCallProcessed

		#region OnRemoteCallReceived

		/// <summary>
		/// Occurs when a remote call is received by a client and is about to be processed.
		/// </summary>
		protected virtual void OnRemoteCallReceived(CallContext context) { }

		#endregion  // OnRemoteCallReceived 

		#endregion  // Protected Methods

		#region Internal Methods

		#region GetChangeInformation

		internal virtual ListManagerChangeInformation GetChangeInformation(ItemSourceType listManagerType)
		{
			return this.GetListManager(listManagerType).ChangeInformation;
		}

		#endregion  // GetChangeInformation

		#region GetList

		internal virtual IEnumerable GetList(ItemSourceType listManagerType)
		{
			return this.GetListManager(listManagerType).List;
		}

		#endregion  // GetList

		#region GetListManager




		internal abstract IListManager GetListManager(ItemSourceType listManagerType);


		#endregion  // GetListManager 

		#endregion  // Internal Methods

		#region Private Methods

		#region AddActivity

		private void AddActivity<TSerializable, TMappingKey>(
			ListManager<ActivityBase, TMappingKey> listManager,
			TSerializable serializableItem,
			TMappingKey idField,
			ListManagerVersionInfo clientVersionInfo,
			out ListManagerVersionInfo newVersionInfo,
			out bool versionWasOutOfDate,
			out TSerializable updatedItemToSendToClient)
			where TSerializable : ActivityBaseSerializableItem
		{
			updatedItemToSendToClient = null;

			ListManagerChangeInformation changeInformation = listManager.ChangeInformation;

			versionWasOutOfDate = (
				clientVersionInfo.DataListVersion != changeInformation.DataListVersion ||
				clientVersionInfo.ChangeHistoryId != changeInformation.Id);

			TSerializable originalItemCopy = (TSerializable)serializableItem.Clone();

			ActivityBase viewObject = this.AddActivityHelper<TSerializable, TMappingKey>(listManager, serializableItem, idField);

			// MD 10/5/10
			// Found while fixing TFS50092
			// The activity could be changed in the ActivityAdded event, which may bump the version number, so get the newVersionInfo
			// after the event is fired so the client has the latest version from the server.
			//newVersionInfo = new ListManagerVersionInfo(listManager.ChangeInformation);

			this.OnActivityAdded(new ActivityAddedEventArgs(viewObject));

			// MD 10/5/10
			// Found while fixing TFS50092
			// Moved from above the OnActivityAdded call.
			newVersionInfo = new ListManagerVersionInfo(listManager.ChangeInformation);

			this.CopyValues(listManager, serializableItem, viewObject.DataItem, true);
			if (originalItemCopy.HasSameData(serializableItem) == false)
				updatedItemToSendToClient = serializableItem;
		}

		#endregion  // AddActivity

		#region AddActivityHelper

		private ActivityBase AddActivityHelper<TSerializable, TMappingKey>(
			ListManager<ActivityBase, TMappingKey> listManager,
			TSerializable serializableItem,
			TMappingKey idField)
			where TSerializable : ActivityBaseSerializableItem
		{
			DataErrorInfo error;
			ActivityBase viewObject = listManager.CreateNew(out error);

			WcfListConnectorService.ValidateErrorInfo(error);

			// MD 11/12/10
			// We need to manually merk these properties becasue they are not set by the list manager. They are normally set by 
			// the XamScheduleDataManager, but we are not using one of those here.
			if (null != viewObject)
			{
				viewObject.IsAddNew = true;
				viewObject.IsInEdit = true;
			}

			// MD 10/5/10 - TFS50092
			// Now that the changes are being submitted to SQL databases in the EndEdit call, we need the data from the client 
			// to be copied to the server data item before we call EndEdit. So associate the new view item with a data item and
			// copyt the values from the client.
			listManager.AssociateDataItemToAddNewViewItem(viewObject, out error);

			WcfListConnectorService.ValidateErrorInfo(error);

			this.CopyValues<TSerializable, ActivityBase, TMappingKey>(
				listManager,
				serializableItem,
				viewObject.DataItem,
				false);

			// ****************** End of TFS50092 Fix ***********************

			ActivityOperationResult result = ListScheduleDataConnectorUtilities<ActivityBase>.EndEdit(
				this,
				viewObject,
				false,
				ListScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditOverrideImplementation,
				new Func<ActivityBase, IEditList<ActivityBase>>( this.GetEditList ) ) as ActivityOperationResult;

			WcfListConnectorService.ValidateErrorInfo(result.Error);

			// MD 10/5/10 - TFS50092
			// Now that the changes are being submitted to SQL databases in the EndEdit call, we need the data from the client 
			// to be copied to the server data item before we call EndEdit.
			//
			//// Give prioirty ot the client's ID since occurrences have an id related to their recurrence root and we'll 
			//// get that from the client. If we didn't get one from the client, use the ID from the server.
			//if (String.IsNullOrEmpty(serializableItem.Id))
			//{
			//    IFieldValueAccessor idAccessor = listManager.GetRawFieldValueAccessor(idField);
			//    serializableItem.Id = (string)idAccessor.GetValue(viewObject.DataItem);
			//    Debug.Assert(String.IsNullOrEmpty(serializableItem.Id) == false, "We should have an ID at this point.");
			//}
			//
			//this.CopyValues<TSerializable, ActivityBase, TMappingKey>(
			//    listManager,
			//    serializableItem,
			//    viewObject.DataItem,
			//    false);

			return viewObject;
		}

		#endregion // AddActivityHelper

		#region CopyValues

		private void CopyValues<TSerializable, TViewItem, TMappingKey>(
			ListManager<TViewItem, TMappingKey> listManager,
			TSerializable serializableItem,
			object dbObject,
			bool dbObjectIsSource) where TViewItem : class
		{
			Type serializableType = serializableItem.GetType();

			foreach (TMappingKey property in Enum.GetValues(typeof(TMappingKey)))
			{
				FieldInfo serializableTypeProperty = serializableType.GetField(property.ToString());

				if (serializableTypeProperty == null)
				{
					Debug.Fail("Cannot find property on serializable item: " + property);
					continue;
				}

				IFieldValueAccessor accessor = listManager.GetFieldValueAccessorConverted(property, serializableTypeProperty.FieldType);

				if (accessor == null)
					continue;

				if (dbObjectIsSource)
				{
					object value = accessor.GetValue(dbObject);
					serializableTypeProperty.SetValue(serializableItem, value);
				}
				else
				{
					object value = serializableTypeProperty.GetValue(serializableItem);
					accessor.SetValue(dbObject, value);
				}
			}
		}

		#endregion  // CopyValues

		#region CreateItemSourceInfo

		private ItemSourceInfo CreateItemSourceInfo<TViewItem, TMappingKey>(ItemSourceType itemSourceType) where TViewItem : class
		{
			if (this.GetList(itemSourceType) == null)
				return null;

			ListManager<TViewItem, TMappingKey> listManager = (ListManager<TViewItem, TMappingKey>)this.GetListManager(itemSourceType);

			// MD 2/9/11 - TFS65718
			if (listManager.Mappings == null)
				return null;

			ItemSourceInfo itemSourceInfo = new ItemSourceInfo();
			itemSourceInfo.VersionInfo = new ListManagerVersionInfo(this.GetChangeInformation(itemSourceType));
			itemSourceInfo.MappedProperties = new List<string>();

			if (listManager.Mappings.UseDefaultMappings)
			{
				foreach (TMappingKey property in Enum.GetValues(typeof(TMappingKey)))
				{
					ISupportValueChangeNotifications<TViewItem> temp;
					IFieldValueAccessor accessor = listManager.GetFieldValueAccessor(property, out temp);

					if (accessor == null)
						continue;

					itemSourceInfo.MappedProperties.Add(property.ToString());
				}
			}
			else
			{
				foreach (PropertyMappingBase<Task> mapping in listManager.Mappings)
					itemSourceInfo.MappedProperties.Add(mapping.ScheduleProperty.ToString());
			}

			return itemSourceInfo;
		}

		#endregion  // CreateItemSourceInfo

		#region CreateSerializableType

		private TSerializable CreateSerializableType<TSerializable, TViewItem, TMappingKey>(object dataItem, ListManager<TViewItem, TMappingKey> listManager)
			where TSerializable : SerializableItemBase, new()
			where TViewItem : class
		{
			if (dataItem == null)
				return null;

			TSerializable serializableResource = new TSerializable();

			this.CopyValues(
				listManager,
				serializableResource,
				dataItem,
				true);

			return serializableResource;
		}

		#endregion  // CreateSerializableType

		#region GetDataItemWithId

		private static object GetDataItemWithId<TMappingKey>(
			TMappingKey idField,
			string id,
			ListManager<ActivityBase, TMappingKey> listManager)
		{
			string mappedField;
			DataErrorInfo error;
			listManager.GetMappedField(idField, out mappedField, out error);

			WcfListConnectorService.ValidateErrorInfo(error);

			LinqQueryManager.LinqCondition condition =
				new LinqQueryManager.LinqCondition(mappedField, LinqQueryManager.LinqOperator.Equal, id);

			IEnumerable appointments = listManager.PerformQueryHelper(condition, out error);

			WcfListConnectorService.ValidateErrorInfo(error);

			foreach (object dbAppointment in appointments)
				return dbAppointment;

			return null;
		}

		#endregion  // GetDataItemWithId

		#region GetDetailedChangeInfo

		private DetailedItemSourceChangeInfo GetDetailedChangeInfo<TSerializable, TViewItem, TMappingKey>(ItemSourceType listManagerType, ListManagerVersionInfo clientVersionInfo)
			where TSerializable : SerializableItemBase, new()
			where TViewItem : class
		{
			DetailedItemSourceChangeInfo changeInfo = new DetailedItemSourceChangeInfo();
			changeInfo.VersionInfo = new ListManagerVersionInfo(this.GetChangeInformation(listManagerType));

			bool addResetChange = false;

			if (clientVersionInfo == null)
			{
				if (changeInfo.VersionInfo.HasList == false)
					return null;
				
				addResetChange = true;
			}
			else if (clientVersionInfo.ChangeHistoryId != changeInfo.VersionInfo.ChangeHistoryId)
			{
				addResetChange = true;
			}
			else if (clientVersionInfo.DataListVersion != changeInfo.VersionInfo.DataListVersion)
			{
				ListManager<TViewItem, TMappingKey> listManager = (ListManager<TViewItem, TMappingKey>)this.GetListManager(listManagerType);
				IEnumerable<ListChangeHistoryItem> enumerable = this.GetChangeInformation(listManagerType).GetHistoryFromVersion(clientVersionInfo.DataListVersion);

				foreach (ListChangeHistoryItem historyItem in enumerable)
				{
					if (changeInfo.Changes == null)
						changeInfo.Changes = new List<ItemSourceChange>();

					ItemSourceChange change = new ItemSourceChange();

					change.ChangedItem = this.CreateSerializableType<TSerializable, TViewItem, TMappingKey>(historyItem.ChangedItem, listManager);
					change.ChangeType = (ItemSourceChangeType)historyItem.ChangeType;

					changeInfo.Changes.Add(change);
				}
			}

			if (addResetChange)
			{
				if (changeInfo.Changes == null)
					changeInfo.Changes = new List<ItemSourceChange>();

				ItemSourceChange change = new ItemSourceChange();
				change.ChangeType = ItemSourceChangeType.Reset;
				changeInfo.Changes.Add(change);
			}

			return changeInfo;
		}

		#endregion  // GetDetailedChangeInfo

		#region GetErrorInfo

		private static WcfRemoteErrorInfo GetErrorInfo(Exception exception)
		{
			WcfRemoteErrorInfo errorInfo = new WcfRemoteErrorInfo();

			errorInfo.ExceptionType = exception.GetType().FullName;
			errorInfo.Message = exception.Message;
			errorInfo.StackTrace = exception.StackTrace;

			return errorInfo;
		}

		#endregion  // GetErrorInfo		

		#region GetListManager

		private IEditList<ActivityBase> GetEditList(ActivityBase activity)
		{
			ActivityType activityType = activity.ActivityType;

			switch (activityType)
			{
				case ActivityType.Appointment:
					if (activity.IsRecurrenceRoot || activity.IsOccurrence)
					{
						AppointmentListManager recurringAppointmentListManager = (AppointmentListManager)this.GetListManager(ItemSourceType.RecurringAppointment);

						if (recurringAppointmentListManager.HasRecurringActivities)
							return recurringAppointmentListManager;
					}

					return (IEditList<ActivityBase>)this.GetListManager(ItemSourceType.Appointment);

				case ActivityType.Journal:
					return (IEditList<ActivityBase>)this.GetListManager(ItemSourceType.Journal);

				case ActivityType.Task:
					return (IEditList<ActivityBase>)this.GetListManager(ItemSourceType.Task);

				default:
					Debug.Assert(false, "Unknown ActivityType:" + activityType);
					CoreUtilities.ValidateEnum(typeof(ActivityType), activityType);
					return null;
			}
		}

		#endregion // GetListManager

		#region GetPropertyValue

		private static object GetPropertyValue<TViewItem, TMappingKey>(ListManager<TViewItem, TMappingKey> listManager, TMappingKey property, object dbObject) where TViewItem : class
		{
			ISupportValueChangeNotifications<TViewItem> temp;
			IFieldValueAccessor accessor = listManager.GetFieldValueAccessor(property, out temp);
			return accessor.GetValue(dbObject);
		}

		#endregion  // GetPropertyValue

		#region PerformActivityOperation

		private void PerformActivityOperation<TSerializable, TViewItem, TMappingKey>(
			TSerializable activity,
			ActivityOperation operation,
			ActivityListManager<TViewItem, TMappingKey> listManager,
			TMappingKey idField,
			ListManagerVersionInfo clientVersionInfo,
			out ListManagerVersionInfo newVersionInfo,
			out bool versionWasOutOfDate,
			out TSerializable updatedItemToSendToClient)
			where TSerializable : ActivityBaseSerializableItem
			where TViewItem : ActivityBase, new()
		{
			updatedItemToSendToClient = null;

			switch (operation)
			{
				case ActivityOperation.Add:
					this.AddActivity(
						listManager,
						activity,
						idField,
						clientVersionInfo,
						out newVersionInfo,
						out versionWasOutOfDate,
						out updatedItemToSendToClient);
					break;

				case ActivityOperation.Edit:
					this.UpdateActivity(
						listManager,
						activity,
						idField,
						clientVersionInfo,
						out newVersionInfo,
						out versionWasOutOfDate,
						out updatedItemToSendToClient);
					break;

				case ActivityOperation.Remove:
					// MD 11/12/10
					// Changed the method signature of this method to take the full activity being removed.
					//this.RemoveActivity(
					//    listManager,
					//    idField,
					//    activity.Id,
					//    clientVersionInfo,
					//    out newVersionInfo,
					//    out versionWasOutOfDate);
					this.RemoveActivity(
						listManager,
						activity,
						idField,
						clientVersionInfo,
						out newVersionInfo,
						out versionWasOutOfDate);
					break;

				default:
					newVersionInfo = null;
					versionWasOutOfDate = false;
					Debug.Fail("Unknown ActivityOperation: " + operation);
					break;
			}
		}

		#endregion // PerformActivityOperation

		#region QueryActivities

		private List<TSerializable> QueryActivities<TSerializable, TViewItem, TMappingKey>(string linqStatementEncoded, ActivityListManager<TViewItem, TMappingKey> listManager)
			where TViewItem : ActivityBase, new()
			where TSerializable : ActivityBaseSerializableItem, new()
		{
			LinqQueryManager.ILinqStatement statement;
			using (XmlReader xmlReader = XmlTextReader.Create(new StringReader(linqStatementEncoded)))
			{
				object value;
				WcfListConnectorService.LinqStatementSerializer.Parse(xmlReader, typeof(LinqQueryManager.ILinqStatement), out value);
				statement = (LinqQueryManager.ILinqStatement)value;
				statement = WcfListConnectorService.RemapLinqQuery(statement, listManager);
			}

			DataErrorInfo error;
			IEnumerable activities = listManager.PerformQueryHelper(statement, out error);

			WcfListConnectorService.ValidateErrorInfo(error);

			List<TSerializable> serializableAactivities = new List<TSerializable>();
			foreach (object activity in activities)
			{
				serializableAactivities.Add(
					this.CreateSerializableType<TSerializable, ActivityBase, TMappingKey>(activity, listManager)
					);
			}

			return serializableAactivities;
		}

		#endregion  // QueryActivities

		#region RemapLinqQuery

		private static LinqQueryManager.ILinqStatement RemapLinqQuery<T, TKey>(LinqQueryManager.ILinqStatement statement, ListManager<T, TKey> listManager) where T : class
		{
			LinqQueryManager.LinqCondition condition = statement as LinqQueryManager.LinqCondition;
			if (condition != null)
			{
				TKey key = (TKey)Enum.Parse(typeof(TKey), condition.FieldName);
				string propertyMapping = listManager.GetMappedFieldIfAny(key);

				return new LinqQueryManager.LinqCondition(propertyMapping, condition.Operator, condition.Operand);
			}

			LinqQueryManager.LinqInstructionOrderBy orderBy = statement as LinqQueryManager.LinqInstructionOrderBy;
			if (orderBy != null)
			{
				TKey key = (TKey)Enum.Parse(typeof(TKey), orderBy.FieldName);
				string propertyMapping = listManager.GetMappedFieldIfAny(key);

				return new LinqQueryManager.LinqInstructionOrderBy(
					propertyMapping,
					WcfListConnectorService.RemapLinqQuery(orderBy.InnerStatement, listManager),
					orderBy.Descending);
			}

			LinqQueryManager.LinqInstructionFirstOrLast firstOrLast = statement as LinqQueryManager.LinqInstructionFirstOrLast;
			if (firstOrLast != null)
			{
				return new LinqQueryManager.LinqInstructionFirstOrLast(
					firstOrLast.First,
					firstOrLast.OrDefault,
					(LinqQueryManager.ILinqCondition)WcfListConnectorService.RemapLinqQuery(firstOrLast.Condition, listManager),
					WcfListConnectorService.RemapLinqQuery(firstOrLast.InnerStatement, listManager));
			}

			LinqQueryManager.LinqConditionGroup group = statement as LinqQueryManager.LinqConditionGroup;
			if (group != null)
			{
				LinqQueryManager.LinqConditionGroup remappedGroup = new LinqQueryManager.LinqConditionGroup(group.LogicalOperator);

				foreach (LinqQueryManager.ILinqStatement subStatement in group)
					remappedGroup.Add((LinqQueryManager.ILinqCondition)WcfListConnectorService.RemapLinqQuery(subStatement, listManager));

				return remappedGroup;
			}

			Debug.Fail("Unknown ILinqStatement type: " + statement.GetType().Name);
			return null;
		}

		#endregion  // RemapLinqQuery

		#region RemoveActivity

		// MD 11/12/10
		// Changed the method signature to take the full activity being removed.
		//private void RemoveActivity<TViewItem, TMappingKey>(
		//    ActivityListManager<TViewItem, TMappingKey> listManager,
		//    TMappingKey idField,
		//    string id,
		//    ListManagerVersionInfo clientVersionInfo,
		//    out ListManagerVersionInfo newVersionInfo,
		//    out bool versionWasOutOfDate)
		//    where TViewItem : ActivityBase, new()
		private void RemoveActivity<TSerializable, TMappingKey>(
			ListManager<ActivityBase, TMappingKey> listManager,
			TSerializable serializableItem,
			TMappingKey idField,
			ListManagerVersionInfo clientVersionInfo,
			out ListManagerVersionInfo newVersionInfo,
			out bool versionWasOutOfDate)
			where TSerializable : ActivityBaseSerializableItem
		{
			newVersionInfo = clientVersionInfo;

			ListManagerChangeInformation changeInformation = listManager.ChangeInformation;

			versionWasOutOfDate = (
				clientVersionInfo.DataListVersion != changeInformation.DataListVersion ||
				clientVersionInfo.ChangeHistoryId != changeInformation.Id);

			// MD 11/12/10
			// Refactored this code because it was incorrect. When an occurrence is deleted, an entry must be added or updated
			// in the database, so we actually have to do the opposite of a removal.
			#region Refactored

			//object dataItem = WcfListConnectorService.GetDataItemWithId(idField, id, listManager);

			//if (dataItem == null)
			//{
			//    Debug.Fail("Could not find data item with Id:" + id);
			//    return;
			//}

			//ActivityBase activity = listManager.ViewItemManager.GetViewItem(dataItem, true);

			//ActivityOperationResult result = ListScheduleDataConnectorUtilities.Remove(
			//    this, activity,
			//    new Action<IEditList<ActivityBase>, ActivityOperationResult, bool>(ListScheduleDataConnectorUtilities.DefaultEndEditOverrideImplementation),
			//    new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditList));

			//WcfListConnectorService.ValidateErrorInfo(result.Error); 

			#endregion  // Refactored
			ActivityBase activity;
			if (string.IsNullOrEmpty(serializableItem.RootActivityId) == false)
			{
				activity = this.UpdateActivityHelper(listManager, serializableItem, idField);
			}
			else
			{
				object dataItem = WcfListConnectorService.GetDataItemWithId(idField, serializableItem.Id, listManager);

				if (dataItem == null)
				{
					Debug.Fail("Could not find data item with Id:" + serializableItem.Id);
					return;
				}

				activity = listManager.ViewItemManager.GetViewItem(dataItem, true);

				ActivityOperationResult result = ListScheduleDataConnectorUtilities<ActivityBase>.Remove(
					this, activity,
					ListScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditOverrideImplementation,
					new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditList));

				WcfListConnectorService.ValidateErrorInfo(result.Error);
			}

			newVersionInfo = new ListManagerVersionInfo(listManager.ChangeInformation);

			this.OnActivityRemoved(new ActivityRemovedEventArgs(activity));
		}

		#endregion  // RemoveActivity

		#region UpdateActivity

		private void UpdateActivity<TSerializable, TMappingKey>(
			ListManager<ActivityBase, TMappingKey> listManager,
			TSerializable serializableItem,
			TMappingKey idField,
			ListManagerVersionInfo clientVersionInfo,
			out ListManagerVersionInfo newVersionInfo,
			out bool versionWasOutOfDate,
			out TSerializable updatedItemToSendToClient)
			where TSerializable : ActivityBaseSerializableItem
		{
			updatedItemToSendToClient = null;
			newVersionInfo = clientVersionInfo;

			ListManagerChangeInformation changeInformation = listManager.ChangeInformation;

			versionWasOutOfDate = (
				clientVersionInfo.DataListVersion != changeInformation.DataListVersion ||
				clientVersionInfo.ChangeHistoryId != changeInformation.Id);

			TSerializable originalItemCopy = (TSerializable)serializableItem.Clone();

			// MD 11/12/10
			// Moved this code to the new UpdateActivityHelper method so it could be used in other places.
			#region Moved

			//object dataItem = WcfListConnectorService.GetDataItemWithId(idField, serializableItem.Id, listManager);

			//ActivityBase activity;
			//if (dataItem == null && string.IsNullOrEmpty(serializableItem.RootActivityId) == false)
			//{
			//    activity = this.AddActivityHelper<TSerializable, TMappingKey>(listManager, serializableItem, idField);
			//}
			//else
			//{
			//    if (dataItem == null)
			//        throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CannotFindDataItemOnServer"));

			//    // MD 10/5/10 - TFS50092
			//    // Added calls to BeginEdit and EndEdit and moved this code below the BeginEdit call. Without this, the data never gets 
			//    // committed to the database when connected to SQL.
			//    //this.CopyValues(
			//    //    listManager,
			//    //    serializableItem,
			//    //    dataItem,
			//    //    false);

			//    activity = listManager.ViewItemManager.GetViewItem(dataItem, true);

			//    Func<ActivityBase, IEditList<ActivityBase>> getEditListCallback = new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditList);

			//    DataErrorInfo error;
			//    ListScheduleDataConnectorUtilities.BeginEdit(this, activity, getEditListCallback, out error);

			//    WcfListConnectorService.ValidateErrorInfo(error);

			//    this.CopyValues(
			//        listManager,
			//        serializableItem,
			//        dataItem,
			//        false);

			//    ActivityOperationResult result = ListScheduleDataConnectorUtilities.EndEdit(
			//        this,
			//        activity,
			//        false,
			//        new Action<IEditList<ActivityBase>, ActivityOperationResult, bool>(ListScheduleDataConnectorUtilities.DefaultEndEditOverrideImplementation),
			//        getEditListCallback);

			//    WcfListConnectorService.ValidateErrorInfo(result.Error);
			//} 

			#endregion  // Moved
			ActivityBase activity = this.UpdateActivityHelper(listManager, serializableItem, idField);

			// MD 10/5/10
			// Found while fixing TFS50092
			// The activity could be changed in the ActivityChanged event, which may bump the version number, so get the newVersionInfo
			// after the event is fired so the client has the latest version from the server.
			//newVersionInfo = new ListManagerVersionInfo(listManager.ChangeInformation);

			this.OnActivityChanged(new ActivityChangedEventArgs(activity));

			// MD 10/5/10
			// Found while fixing TFS50092
			// Moved from above the OnActivityChanged call.
			newVersionInfo = new ListManagerVersionInfo(listManager.ChangeInformation);

			this.CopyValues(listManager, serializableItem, activity.DataItem, true);
			if (originalItemCopy.HasSameData(serializableItem) == false)
				updatedItemToSendToClient = serializableItem;
		}

		#endregion  // UpdateActivity

		// MD 11/12/10
		// Moved this code from UpdateActivity so it could be used in other places.
		#region UpdateActivityHelper

		private ActivityBase UpdateActivityHelper<TSerializable, TMappingKey>(
			ListManager<ActivityBase, TMappingKey> listManager,
			TSerializable serializableItem,
			TMappingKey idField)
			where TSerializable : ActivityBaseSerializableItem
		{
			object dataItem = WcfListConnectorService.GetDataItemWithId(idField, serializableItem.Id, listManager);

			ActivityBase activity;
			if (dataItem == null && string.IsNullOrEmpty(serializableItem.RootActivityId) == false)
			{
				activity = this.AddActivityHelper<TSerializable, TMappingKey>(listManager, serializableItem, idField);
			}
			else
			{
				if (dataItem == null)
					throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CannotFindDataItemOnServer"));

				// MD 10/5/10 - TFS50092
				// Added calls to BeginEdit and EndEdit and moved this code below the BeginEdit call. Without this, the data never gets 
				// committed to the database when connected to SQL.
				//this.CopyValues(
				//    listManager,
				//    serializableItem,
				//    dataItem,
				//    false);

				activity = listManager.ViewItemManager.GetViewItem(dataItem, true);

				Func<ActivityBase, IEditList<ActivityBase>> getEditListCallback = new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditList);

				DataErrorInfo error;
				ListScheduleDataConnectorUtilities<ActivityBase>.BeginEdit( this, activity, getEditListCallback, out error );

				WcfListConnectorService.ValidateErrorInfo(error);

				this.CopyValues(
					listManager,
					serializableItem,
					dataItem,
					false);

				ActivityOperationResult result = ListScheduleDataConnectorUtilities<ActivityBase>.EndEdit(
					this,
					activity,
					false,
					ListScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditOverrideImplementation,
					getEditListCallback ) as ActivityOperationResult;

				WcfListConnectorService.ValidateErrorInfo(result.Error);
			}

			return activity;
		} 

		#endregion  // UpdateActivityHelper

		#region ValidateErrorInfo

		private static void ValidateErrorInfo(DataErrorInfo error)
		{
			if (error == null)
				return;

			if (error.Exception != null)
				throw error.Exception;

			
			throw new InvalidOperationException(error.UserErrorText);
		}

		#endregion  // ValidateErrorInfo

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region LinqStatementSerializer

		private static ObjectSerializer LinqStatementSerializer
		{
			get
			{
				if (_linqStatementSerializer == null)
				{
					_linqStatementSerializer = new ObjectSerializer(new AttributeValueParser());
					LinqQueryManager.RegisterSerializerInfos(_linqStatementSerializer);
				}

				return _linqStatementSerializer;
			}
		} 

		#endregion // LinqStatementSerializer

		#endregion  // Properties
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