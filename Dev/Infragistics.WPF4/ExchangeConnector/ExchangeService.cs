using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.IO;
using System.Xml;

namespace Infragistics.Controls.Schedules
{
	internal partial class ExchangeService
	{
		#region Constants

		private const string UserConfigurationCategoryListName = "CategoryList"; 

		#endregion  // Constants

		#region Member Variables

		private ExchangeUser _associatedUser;
		private ExchangeCalendarFolder _calendarFolder;
		private CategoryList _categoryList;
		private ExchangeScheduleDataConnector _connector;
		private FolderType _deletedItemsFolder;
		private bool _isGettingDefaultFolders;
		private ExchangeJournalFolder _journalFolder;
		private FolderType _msgFolderRootFolder;
		private List<ExchangeFolder> _nonDefaultFolders;
		private List<Action> _onFindRemindersFolderCompletedActions;
		private List<Action<ExchangeService>> _onGetDefaultFoldersCompletedActions;
		private PullNotificationInfo _pullNotificationInfo;
		private ExchangeRemindersFolder _remindersFolder;
		private Resource _resource;
		private ExchangeServiceBindingInternal _serviceBinding;
		private ExchangeTasksFolder _tasksFolder;

		#endregion  // Member Variables

		#region Constructors

		public ExchangeService(ExchangeScheduleDataConnector connector, ExchangeServiceBindingInternal serviceBinding, Resource resource, ExchangeUser associatedUser)
		{
			_associatedUser = associatedUser;
			_connector = connector;
			_resource = resource;
			_serviceBinding = serviceBinding;

			// MD 4/27/11 - TFS72779
			_serviceBinding.Service = this;

			_serviceBinding.CreateItemCompleted += new CreateItemCompletedEventHandler(this.OnServiceBindingCreateItemCompleted);
			_serviceBinding.DeleteItemCompleted += new DeleteItemCompletedEventHandler(this.OnServiceBindingDeleteItemCompleted);
			_serviceBinding.FindFolderCompleted += new FindFolderCompletedEventHandler(this.OnServiceBindingFindFolderCompleted);
			_serviceBinding.FindItemCompleted += new FindItemCompletedEventHandler(this.OnServiceBindingFindItemCompleted);
			_serviceBinding.GetEventsCompleted += new GetEventsCompletedEventHandler(this.OnServiceBindingGetEventsCompleted);
			_serviceBinding.GetFolderCompleted += new GetFolderCompletedEventHandler(this.OnServiceBindingGetFolderCompleted);
			_serviceBinding.GetItemCompleted += new GetItemCompletedEventHandler(this.OnServiceBindingGetItemCompleted);
			_serviceBinding.GetUserAvailabilityCompleted += new GetUserAvailabilityCompletedEventHandler(this.OnServiceBindingGetUserAvailabilityCompleted);
			_serviceBinding.GetUserConfigurationCompleted += new GetUserConfigurationCompletedEventHandler(this.OnServiceBindingGetUserConfigurationCompleted);
			_serviceBinding.MoveItemCompleted += new MoveItemCompletedEventHandler(this.OnServiceBindingMoveItemCompleted);
			_serviceBinding.ResolveNamesCompleted += new ResolveNamesCompletedEventHandler(this.OnServiceBindingResolveNamesCompleted);
			_serviceBinding.SubscribeCompleted += new SubscribeCompletedEventHandler(this.OnServiceBindingSubscribeCompleted);
			_serviceBinding.UnsubscribeCompleted += new UnsubscribeCompletedEventHandler(this.OnServiceBindingUnsubscribeCompleted);
			_serviceBinding.UpdateItemCompleted += new UpdateItemCompletedEventHandler(this.OnServiceBindingUpdateItemCompleted);
			_serviceBinding.UpdateUserConfigurationCompleted += new UpdateUserConfigurationCompletedEventHandler(this.OnServiceBindingUpdateUserConfigurationCompleted);
		}

		#endregion  // Constructors

		#region Methods

		#region Public Methods

		#region CreateItem

		public void CreateItem(
			ExchangeFolder folder, 
			ItemType item,
			Action<ItemType> onCreateItemsCompleted, 
			ErrorCallback onError)
		{
			folder = this.ResolveFolder(folder, item);

			item = item.Clone();
			folder.CleanItemForAdd(item);

			CreateItemType createItem = new CreateItemType();
			createItem.MessageDisposition = MessageDispositionType.SaveOnly;
			createItem.MessageDispositionSpecified = true;
			createItem.SavedItemFolderId = new TargetFolderIdType();
			createItem.SavedItemFolderId.Item = folder.FolderId;

			// MD 2/2/11 - TFS64545
			// To create an appointment as opposed to a meeting, the SendMeetingInvitations value needs to be SendToNone.
			//createItem.SendMeetingInvitations = CalendarItemCreateOrDeleteOperationType.SendToAllAndSaveCopy;
			createItem.SendMeetingInvitations = CalendarItemCreateOrDeleteOperationType.SendToNone;

			createItem.SendMeetingInvitationsSpecified = true;

			createItem.Items = new NonEmptyArrayOfAllItemsType();
			createItem.Items.Items = new ItemType[] { item };

			CreateItemUserState state = new CreateItemUserState(this, item, onCreateItemsCompleted, onError);
			_serviceBinding.CreateItemAsync(createItem, state);
		}

		#endregion  // CreateItem

		#region DeleteItem

		public void DeleteItem(ItemType item, bool deleteAllTaskOccurrences, Action onDeleteItemsCompleted, ErrorCallback onError)
		{
			ItemIdType itemId = item.ItemId;

			if (itemId == null)
			{
				if (onError != null)
					onError(RemoteCallErrorReason.Error, ResponseCodeType.NoError, new InvalidOperationException(ExchangeConnectorUtilities.GetString("LE_ErrorDeletingActivity")));

				return;
			}

			DeleteItemType deleteItem = new DeleteItemType();
			deleteItem.DeleteType = DisposalType.MoveToDeletedItems;

			if (item is TaskType)
			{
				if (deleteAllTaskOccurrences == false)
					deleteItem.DeleteType = DisposalType.HardDelete;

				deleteItem.AffectedTaskOccurrences = deleteAllTaskOccurrences
					? AffectedTaskOccurrencesType.AllOccurrences
					: AffectedTaskOccurrencesType.SpecifiedOccurrenceOnly;
				deleteItem.AffectedTaskOccurrencesSpecified = true;
			}
			else
			{
				deleteItem.SendMeetingCancellations = CalendarItemCreateOrDeleteOperationType.SendToAllAndSaveCopy;
				deleteItem.SendMeetingCancellationsSpecified = true;
			}

			deleteItem.ItemIds = new BaseItemIdType[] { itemId };

			DeleteItemUserState state = new DeleteItemUserState(this, onDeleteItemsCompleted, onError);
			_serviceBinding.DeleteItemAsync(deleteItem, state);
		}

		#endregion  // DeleteItem

		#region DetermineBodyFormat

		public void DetermineBodyFormat(
			ItemType item, 
			ExchangeFolder folder,
			Action<DescriptionFormat> onBodyFormatDetermined,
			ErrorCallback onError)
		{
			GetItemType getItem = new GetItemType();

			getItem.ItemShape = new ItemResponseShapeType();
			getItem.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;
			List<BasePathToElementType> additionalProperties = new List<BasePathToElementType>();
			EWSUtilities.AppendAdditionalProperty(additionalProperties, UnindexedFieldURIType.itemBody);
			getItem.ItemShape.AdditionalProperties = additionalProperties.ToArray();

			getItem.ItemIds = new BaseItemIdType[] { item.ItemId };

			DetermineBodyFormatUserState state = new DetermineBodyFormatUserState(this, folder, onBodyFormatDetermined, onError);
			_serviceBinding.GetItemAsync(getItem, state);
		}

		#endregion  // DetermineBodyFormat

		#region FilderItemIdsByExpectedType

		public void FilterItemIdsByExpectedType(
			ExchangeFolder folder,
			List<BaseItemIdType> itemIds,
			Action<ExchangeFolder, IList<ItemType>> onFilterItemIdsByExpectedTypeCompleted,
			ErrorCallback onError)
		{
			GetItemType getItem = new GetItemType();

			getItem.ItemShape = new ItemResponseShapeType();
			getItem.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;

			List<BasePathToElementType> neededProperties = new List<BasePathToElementType>();
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemItemClass);
			getItem.ItemShape.AdditionalProperties = neededProperties.ToArray();

			getItem.ItemIds = itemIds.ToArray();

			FilterItemIdsByExpectedTypeUserState state = new FilterItemIdsByExpectedTypeUserState(this, folder, onFilterItemIdsByExpectedTypeCompleted, onError);
			_serviceBinding.GetItemAsync(getItem, state);
		} 

		#endregion  // FilderItemIdsByExpectedType

		#region FindFolderWithId

		public ExchangeFolder FindFolderWithId(FolderIdType folderId)
		{
			if (ExchangeService.DoesCalendarHaveId(this.CalendarFolder, folderId))
				return this.CalendarFolder;

			if (ExchangeService.DoesCalendarHaveId(this.JournalFolder, folderId))
				return this.JournalFolder;

			if (ExchangeService.DoesCalendarHaveId(this.TasksFolder, folderId))
				return this.TasksFolder;

			if (ExchangeService.DoesCalendarHaveId(this.RemindersFolder, folderId))
				return this.RemindersFolder;

			if (_nonDefaultFolders != null)
			{
				for (int i = 0; i < _nonDefaultFolders.Count; i++)
				{
					ExchangeFolder folder = _nonDefaultFolders[i];
					if (ExchangeService.DoesCalendarHaveId(folder, folderId))
						return folder;
				}
			}

			return null;
		}

		#endregion  // FindFolderWithId

		#region FindRecurringMasterCalendarItem

		public void FindRecurringMasterCalendarItems(
			ExchangeFolder folder, 
			List<ItemType> occurrences,
			Action<ExchangeFolder, IList<CalendarItemType>> onFindRecurringMasterCalendarItemsCompleted,
			ErrorCallback onError)
		{
			List<BaseItemIdType> itemIds = new List<BaseItemIdType>(occurrences.Count);

			for (int i = 0; i < occurrences.Count; i++)
			{
				RecurringMasterItemIdType recurringMasterItemId = new RecurringMasterItemIdType();
				recurringMasterItemId.OccurrenceId = occurrences[i].ItemId.Id;
				itemIds.Add(recurringMasterItemId);
			}

			this.GetItems(folder, itemIds,
				(folder2, items) =>
				{
					List<CalendarItemType> calendarItems = new List<CalendarItemType>(items.Count);
					for (int i = 0; i < items.Count; i++)
					{
						CalendarItemType recurringMasterItem = items[i] as CalendarItemType;

						if (recurringMasterItem == null)
						{
							ExchangeConnectorUtilities.DebugFail("Unknown item type.");
							continue;
						}

						calendarItems.Add(recurringMasterItem);
					}

					if (onFindRecurringMasterCalendarItemsCompleted != null)
						onFindRecurringMasterCalendarItemsCompleted(folder2, calendarItems);
				},
				onError);
		}

		#endregion  // FindRecurringMasterCalendarItem

		#region FindReminders

		public void FindReminders(Action<ExchangeFolder, IList<ItemType>> onFindRemindersCompleted, ErrorCallback onError)
		{
			if (this.RemindersFolder == null)
			{
				if (_onFindRemindersFolderCompletedActions == null)
					_onFindRemindersFolderCompletedActions = new List<Action>();

				_onFindRemindersFolderCompletedActions.Add(() => this.FindReminders(onFindRemindersCompleted, onError));
				return;
			}

			FindItemType findItemTypeForRemindersFolder = ExchangeService.CreateFindItemTypeForReminders(this.RemindersFolder);

			FindItemType findItemTypeForTasksFolder = null;
			if (this.TasksFolder != null)
				findItemTypeForTasksFolder = ExchangeService.CreateFindItemTypeForReminders(this.TasksFolder);

			List<ItemType> allReminders = new List<ItemType>();
			FindRemindersUserState stateForRemindersFolder = new FindRemindersUserState(this, allReminders, onFindRemindersCompleted, onError);
			FindRemindersUserState stateForTasksFolder = null;
			if (findItemTypeForTasksFolder != null)
			{
				stateForTasksFolder = new FindRemindersUserState(this, allReminders, onFindRemindersCompleted, onError);
				stateForTasksFolder.OtherPendingCalls = new FindRemindersUserState[] { stateForRemindersFolder };
				stateForRemindersFolder.OtherPendingCalls = new FindRemindersUserState[] { stateForTasksFolder };
			}

			_serviceBinding.FindItemAsync(findItemTypeForRemindersFolder, stateForRemindersFolder);

			if (stateForTasksFolder != null)
				_serviceBinding.FindItemAsync(findItemTypeForTasksFolder, stateForTasksFolder);
		}

		#endregion  // FindReminders

		#region FindResourceCalendarWithId

		public ResourceCalendar FindResourceCalendarWithId(FolderIdType folderId)
		{
			ExchangeFolder folder = this.FindFolderWithId(folderId);

			if (folder != null)
				return folder.AssociatedCalendar;

			return null;
		}

		#endregion  // FindResourceCalendarWithId

		#region GetAppointmentsInRange

		public void GetAppointmentsInRange(
			ExchangeFolder folder, 
			DateRange dateRange,
			Action<ExchangeFolder, IList<ItemType>> onGetAppointmentsInRangeCompleted,
			ErrorCallback onError)
		{
			FindItemType findItemType = new FindItemType();
			findItemType.Traversal = ItemQueryTraversalType.Shallow;

			findItemType.ItemShape = new ItemResponseShapeType();
			findItemType.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;

			findItemType.Item = folder.CreateViewForDateRange(dateRange);
			findItemType.Restriction = folder.CreateRestrictionForDateRange(dateRange);

			findItemType.ParentFolderIds = new BaseFolderIdType[] { folder.FolderId };

			GetAppointmentsInRangeUserState state = new GetAppointmentsInRangeUserState(this, folder, onGetAppointmentsInRangeCompleted, onError);
			_serviceBinding.FindItemAsync(findItemType, state);
		}

		#endregion  // GetAppointmentsInRange

		#region GetCategories

		public void GetCategories(Action onGetCategoriesCompleted, ErrorCallback onError)
		{
			GetUserConfigurationType getUserConfigurationType = new GetUserConfigurationType();

			getUserConfigurationType.UserConfigurationName = new UserConfigurationNameType();
			getUserConfigurationType.UserConfigurationName.Item = EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.calendar);
			getUserConfigurationType.UserConfigurationName.Name = ExchangeService.UserConfigurationCategoryListName;

			getUserConfigurationType.UserConfigurationProperties = UserConfigurationPropertyType.XmlData;

			GetCategoriesUserState userState = new GetCategoriesUserState(this, onGetCategoriesCompleted, onError);
			_serviceBinding.GetUserConfigurationAsync(getUserConfigurationType, userState);
		} 

		#endregion //GetCategories

		#region GetDefaultFolders

		public void GetDefaultFolders(Action<ExchangeService> onGetCalendarCompletedAction, ErrorCallback onError)
		{
			if (_isGettingDefaultFolders)
			{
				if (onGetCalendarCompletedAction != null)
					_onGetDefaultFoldersCompletedActions.Add(onGetCalendarCompletedAction);

				return;
			}

			_isGettingDefaultFolders = true;

			if (onGetCalendarCompletedAction != null)
			{
				if (_onGetDefaultFoldersCompletedActions == null)
					_onGetDefaultFoldersCompletedActions = new List<Action<ExchangeService>>();

				_onGetDefaultFoldersCompletedActions.Add(onGetCalendarCompletedAction);
			}

			GetFolderType getFolderType = new GetFolderType();
			getFolderType.FolderIds = new BaseFolderIdType[] { 
				EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.calendar), 
				EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.deleteditems), 
				EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.journal), 
				EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.msgfolderroot), 
				EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.tasks) };

			getFolderType.FolderShape = new FolderResponseShapeType();
			getFolderType.FolderShape.BaseShape = DefaultShapeNamesType.AllProperties;

			GetDefaultFoldersUserState state = new GetDefaultFoldersUserState(this, onError);
			_serviceBinding.GetFolderAsync(getFolderType, state);
		}

		#endregion  // GetDefaultFolders

		#region GetItems

		public void GetItems(
			ExchangeFolder folder,
			IList<BaseItemIdType> itemIds,
			Action<ExchangeFolder, IList<ItemType>> onGetItemsCompleted,
			ErrorCallback onError)
		{
			// MD 2/2/11
			// Found while fixing TFS64548
			// We might get in here when there are no item ids, which cause the server to return an error.
			// If this is the case, we shouldn't even contact the server because we technically have all the
			// items we need.
			if (itemIds.Count == 0)
			{
				if (onGetItemsCompleted != null)
					onGetItemsCompleted(folder, new ItemType[0]);

				return;
			}

			GetItemType getItem = new GetItemType();

			getItem.ItemShape = new ItemResponseShapeType();
			getItem.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;

			List<BasePathToElementType> neededProperties = new List<BasePathToElementType>();
			folder.GetNeededProperties(neededProperties);
			getItem.ItemShape.AdditionalProperties = neededProperties.ToArray();

			
			{
				getItem.ItemShape.BodyType = BodyTypeResponseType.Text;
				getItem.ItemShape.BodyTypeSpecified = true;
			}

			getItem.ItemIds = itemIds.ToArray();

			GetItemUserState state = new GetItemUserState(this, folder, onGetItemsCompleted, onError);
			_serviceBinding.GetItemAsync(getItem, state);
		}

		#endregion  // GetItems

		#region GetNonDefaultFolders

		public void GetNonDefaultFolders(
			Action<IList<ExchangeFolder>> onGetNonDefaultFoldersCompleted,
			ErrorCallback onError)
		{
			FindFolderType findFolder = new FindFolderType();
			findFolder.Traversal = FolderQueryTraversalType.Deep;

			findFolder.FolderShape = new FolderResponseShapeType();
			findFolder.FolderShape.BaseShape = DefaultShapeNamesType.AllProperties;

			findFolder.Restriction = new RestrictionType();

			IsEqualToType calendarTypeFilter = new IsEqualToType();
			calendarTypeFilter.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.folderFolderClass);
			calendarTypeFilter.FieldURIOrConstant = EWSUtilities.CreateConstant(ExchangeCalendarFolder.FolderClass);

			IsEqualToType journalTypeFilter = new IsEqualToType();
			journalTypeFilter.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.folderFolderClass);
			journalTypeFilter.FieldURIOrConstant = EWSUtilities.CreateConstant(ExchangeJournalFolder.FolderClass);

			IsEqualToType taskTypeFilter = new IsEqualToType();
			taskTypeFilter.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.folderFolderClass);
			taskTypeFilter.FieldURIOrConstant = EWSUtilities.CreateConstant(ExchangeTasksFolder.FolderClass);

			findFolder.Restriction.Item = EWSUtilities.OrCondition(calendarTypeFilter, journalTypeFilter, taskTypeFilter);

			findFolder.ParentFolderIds = new BaseFolderIdType[] { EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.root) };

			GetNonDefaultFoldersUserState state = new GetNonDefaultFoldersUserState(this, onGetNonDefaultFoldersCompleted, onError);
			_serviceBinding.FindFolderAsync(findFolder, state);
		}

		#endregion  // GetNonDefaultFolders

		#region GetWorkingHours

		public void GetWorkingHours(string emailAddress, TimeZoneInfoProvider provider, Action<WorkingHours> onGetUserAvailabilityCompleted, ErrorCallback onError)
		{
			GetUserAvailabilityRequestType request = new GetUserAvailabilityRequestType();

			SerializableTimeZone timeZone = new SerializableTimeZone();
			request.TimeZone = timeZone;

			TimeZoneToken token = provider.LocalToken;
			timeZone.Bias = -(int)provider.GetBaseUtcOffset(token).TotalMinutes;
			TimeZoneInfoProvider.TimeAdjustmentRule[] adjustmentRules = provider.GetAdjustmentRules(token);

			timeZone.DaylightTime = new SerializableTimeZoneTime();
			timeZone.StandardTime = new SerializableTimeZoneTime();

			if (adjustmentRules.Length == 0)
			{
				string time = EWSUtilities.TimeSpanToExchangeTime(TimeSpan.FromHours(2));

				timeZone.DaylightTime.Bias = 0;
				timeZone.DaylightTime.DayOrder = 1;
				timeZone.DaylightTime.DayOfWeek = "Sunday";
				timeZone.DaylightTime.Month = 10;
				timeZone.DaylightTime.Time = time;

				timeZone.StandardTime.Bias = 0;
				timeZone.StandardTime.DayOrder = 1;
				timeZone.StandardTime.DayOfWeek = "Sunday";
				timeZone.StandardTime.Month = 3;
				timeZone.StandardTime.Time = time;
			}
			else
			{
				TimeZoneInfoProvider.TimeAdjustmentRule rule = adjustmentRules[adjustmentRules.Length - 1];

				timeZone.DaylightTime = EWSUtilities.GetTimeZoneTime(rule.TransitionStart, -rule.Delta);
				timeZone.StandardTime = EWSUtilities.GetTimeZoneTime(rule.TransitionEnd, TimeSpan.Zero);	
			}

			MailboxData mailboxData = new MailboxData();
			mailboxData.AttendeeType = MeetingAttendeeType.Required;
			mailboxData.ExcludeConflicts = false;
			mailboxData.ExcludeConflictsSpecified = true;
			mailboxData.Email = new EmailAddress();
			mailboxData.Email.Address = emailAddress;
			request.MailboxDataArray = new MailboxData[] { mailboxData };

			request.FreeBusyViewOptions = new FreeBusyViewOptionsType();
			request.FreeBusyViewOptions.MergedFreeBusyIntervalInMinutes = 30;
			request.FreeBusyViewOptions.MergedFreeBusyIntervalInMinutesSpecified = true;
			request.FreeBusyViewOptions.RequestedView = FreeBusyViewType.Detailed;
			request.FreeBusyViewOptions.RequestedViewSpecified = true;
			request.FreeBusyViewOptions.TimeWindow = new Duration();
			DateTime today = DateTime.Today;
			request.FreeBusyViewOptions.TimeWindow.StartTime = today;
			request.FreeBusyViewOptions.TimeWindow.EndTime = today.AddDays(1);

			GetUserAvailabilityUserState state = new GetUserAvailabilityUserState(this, onGetUserAvailabilityCompleted, onError);
			_serviceBinding.GetUserAvailabilityAsync(request, state);
		}

		#endregion  // GetWorkingHours

		#region MoveItem

		public void MoveItem(
			ExchangeFolder toFolder,
			ItemType item,
			Action<ExchangeFolder, ItemType> onMoveItemCompleted,
			ErrorCallback onError)
		{
			if (item.ItemId == null)
			{
				ExchangeConnectorUtilities.DebugFail("Cannot move the item because it doesn't have an ID yet.");
				return;
			}

			MoveItemType moveItem = new MoveItemType();
			moveItem.ToFolderId = new TargetFolderIdType();
			moveItem.ToFolderId.Item = toFolder.FolderId;
			moveItem.ItemIds = new BaseItemIdType[] { item.ItemId };

			MoveItemUserState state = new MoveItemUserState(this, toFolder, onMoveItemCompleted, onError);
			_serviceBinding.MoveItemAsync(moveItem, state);
		} 

		#endregion  // MoveItem

		#region PollForChanges

		public void PollForChanges(
			Action<ServerChangeType, ExchangeFolder, ItemIdType> onItemChangeDetected,
			Action onServerResponseCompleted,
			ErrorCallback onError)
		{
			if (_pullNotificationInfo == null)
				return;

			GetEventsType getEvents = new GetEventsType();
			getEvents.SubscriptionId = _pullNotificationInfo.SubscriptionId;
			getEvents.Watermark = _pullNotificationInfo.Watermark;

			PollForChangesUserState state = new PollForChangesUserState(this, onItemChangeDetected, onServerResponseCompleted, onError);
			_serviceBinding.GetEventsAsync(getEvents, state);
		}

		#endregion  // PollForChanges

		#region RefreshFolderId

		public void RefreshFolderId(FolderIdType folderId)
		{
			ExchangeFolder folder = this.FindFolderWithId(folderId);
			if (folder == null)
			{
				ExchangeConnectorUtilities.DebugFail("Could not find the folder.");
				return;
			}

			GetFolderType getFolderType = new GetFolderType();

			FolderIdType folderIdType = new FolderIdType();
			folderIdType.Id = folder.FolderId.Id;
			getFolderType.FolderIds = new BaseFolderIdType[] { folderIdType };

			getFolderType.FolderShape = new FolderResponseShapeType();
			getFolderType.FolderShape.BaseShape = DefaultShapeNamesType.IdOnly;

			RefreshFolderIdUserState state = new RefreshFolderIdUserState(this, folder);
			_serviceBinding.GetFolderAsync(getFolderType, state);
		}

		#endregion //RefreshFolderId

		// MD 11/4/11 - TFS81088
		#region RefreshItemId

		public void RefreshItemId(ExchangeFolder folder, 
			ItemType item, 
			Action<ExchangeFolder, IList<ItemType>> onGetItemsCompleted, 
			ErrorCallback onError)
		{
			GetItemType getItemType = new GetItemType();

			ItemIdType iIdType = new ItemIdType();
			iIdType.Id = item.ItemId.Id;
			getItemType.ItemIds = new BaseItemIdType[] { iIdType };

			getItemType.ItemShape = new ItemResponseShapeType();
			getItemType.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;

			RefreshItemIdUserState state = new RefreshItemIdUserState(this, folder, item, onGetItemsCompleted, onError);
			_serviceBinding.GetItemAsync(getItemType, state);
		}

		#endregion //RefreshFolderId

		#region ResolveUserInfo

		public void ResolveUserInfo(string userName, Action<string, string> onResolveUserInfoCompleted, ErrorCallback onError)
		{
			ResolveNamesType resolveNames = new ResolveNamesType();
			resolveNames.ReturnFullContactData = true;
			resolveNames.SearchScope = ResolveNamesSearchScopeType.ActiveDirectory;
			resolveNames.UnresolvedEntry = userName;

			ResolveUserInfoUserState state = new ResolveUserInfoUserState(this, onResolveUserInfoCompleted, onError);
			_serviceBinding.ResolveNamesAsync(resolveNames, state);
		}

		#endregion  // ResolveUserInfo

		// MD 4/27/11 - TFS72779
		#region SetAutoDetectedVersion

		public void SetAutoDetectedVersion(ExchangeVersion versionValue)
		{
			// MD 5/26/11 - TFS76314
			// These enums are no longer mapped to the same values. We need a method to convert between them.
			//_serviceBinding.RequestServerVersionValue.Version = (ExchangeVersionType)versionValue;
			_serviceBinding.RequestServerVersionValue.Version = EWSUtilities.ExchangeVersionToExchangeVersionType(versionValue);

			ExchangeScheduleDataConnector connector = this.Connector;
			if (EWSUtilities.AreCategoriesSupported(versionValue))
				this.GetCategories(null, connector.DefaultErrorCallback);
		} 

		#endregion //SetAutoDetectedVersion

		#region UnsubscribeFromPullNotifications

		public void UnsubscribeFromPullNotifications(ErrorCallback onError)
		{
			if (_pullNotificationInfo == null)
				return;

			UnsubscribeType unsubscribe = new UnsubscribeType();
			unsubscribe.SubscriptionId = _pullNotificationInfo.SubscriptionId;

			UnsubscribeUserState state = new UnsubscribeUserState(this, onError);
			_serviceBinding.UnsubscribeAsync(unsubscribe, state);
		}

		#endregion  // UnsubscribeFromPullNotifications

		#region UpdateCategories

		public void UpdateCategories(Action onUpdateCategoriesUserState, ErrorCallback onError)
		{
			if (_categoryList == null)
			{
				_categoryList = new CategoryList();
				_categoryList.Categories = new List<Category>();
			}

			if (_resource.CustomActivityCategories == null ||
				_resource.CustomActivityCategories.Count == 0)
			{
				_categoryList.Categories.Clear();
				_categoryList.DefaultCategory = "";
			}
			else
			{
				Category defaultCategory = null;
				for (int i = 0; i < _categoryList.Categories.Count; i++)
				{
					Category category = _categoryList.Categories[i];
					if (category.Name == _categoryList.DefaultCategory)
					{
						defaultCategory = category;
						break;
					}
				}

				_categoryList.Categories.Clear();

				for (int i = 0; i < _resource.CustomActivityCategories.Count; i++)
				{
					ActivityCategory activityCategory = _resource.CustomActivityCategories[i];

					Category category = activityCategory.DataItem as Category;
					if (category == null)
					{
						category = new Category();
						category.Id = Guid.NewGuid();
						activityCategory.DataItem = category;
					}

					if (activityCategory.Color.HasValue)
						category.ColorIndex = Array.IndexOf(ExchangeScheduleDataConnector.ExchangeCategoryColors, activityCategory.Color.Value);
					else
						category.ColorIndex = -1;

					category.Name = activityCategory.CategoryName;

					_categoryList.Categories.Add(category);
				}

				if (defaultCategory == null && _categoryList.Categories.Count > 0)
					defaultCategory = _categoryList.Categories[0];

				if (defaultCategory != null)
					_categoryList.DefaultCategory = defaultCategory.Name;
				else
					_categoryList.DefaultCategory = string.Empty;
			}

			_categoryList.LastSavedTime = DateTime.Now;

			UpdateUserConfigurationType updateUserConfigurationType = new UpdateUserConfigurationType();
			updateUserConfigurationType.UserConfiguration = new UserConfigurationType();

			using (MemoryStream stream = new MemoryStream())
			using (XmlWriter writer = XmlWriter.Create(stream))
			{
				_categoryList.Save(writer);
				updateUserConfigurationType.UserConfiguration.XmlData = stream.ToArray();
			}

			updateUserConfigurationType.UserConfiguration.UserConfigurationName = new UserConfigurationNameType();
			updateUserConfigurationType.UserConfiguration.UserConfigurationName.Item = EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.calendar);
			updateUserConfigurationType.UserConfiguration.UserConfigurationName.Name = ExchangeService.UserConfigurationCategoryListName;

			UpdateCategoriesUserState userState = new UpdateCategoriesUserState(this, onUpdateCategoriesUserState, onError);
			_serviceBinding.UpdateUserConfigurationAsync(updateUserConfigurationType, userState);
		} 

		#endregion //UpdateCategories

		#region UpdateItem

		public void UpdateItem(
			ExchangeFolder owningFolder,
			ItemType item,
			ItemType originalItem,
			Action<ItemType, bool> onUpdateCalendarItemCompleted,
			ErrorCallback onError)
		{
			if (item.ItemId == null)
			{
				ExchangeConnectorUtilities.DebugFail("Cannot update the item because it doesn't have an ID yet.");
				return;
			}

			owningFolder = this.ResolveFolder(owningFolder, item);

			UpdateItemType updateItem = new UpdateItemType();
			updateItem.ConflictResolution = ConflictResolutionType.AutoResolve;
			updateItem.MessageDisposition = MessageDispositionType.SaveOnly;
			updateItem.MessageDispositionSpecified = true;

			// If we try to send out notifications when there are no people in the To field, we get back ErrorInvalidRecipients
			// even though the update passes. It seems the best way to determine whether to send the updates out is if the meeting 
			// request was sent.
			if (EWSUtilities.GetMeetingRequestWasSent(item))
				updateItem.SendMeetingInvitationsOrCancellations = CalendarItemUpdateOperationType.SendToAllAndSaveCopy;
			else
				updateItem.SendMeetingInvitationsOrCancellations = CalendarItemUpdateOperationType.SendToNone;

			updateItem.SendMeetingInvitationsOrCancellationsSpecified = true;

			ItemChangeType itemChange = new ItemChangeType();
			itemChange.Item = item.ItemId;

			List<ItemChangeDescriptionType> updates = owningFolder.GetItemUpdates(item, originalItem);

			if (updates.Count == 0)
			{
				if (onUpdateCalendarItemCompleted != null)
					onUpdateCalendarItemCompleted(item, false);

				return;
			}

			itemChange.Updates = updates.ToArray();

			updateItem.ItemChanges = new ItemChangeType[] { itemChange };

			UpdateItemUserState state = new UpdateItemUserState(this, item, onUpdateCalendarItemCompleted, onError);
			_serviceBinding.UpdateItemAsync(updateItem, state);
		}

		#endregion  // UpdateItem

		#endregion  // Public Methods

		#region Private Methods

		#region CreateFindItemTypeForReminders

		private static FindItemType CreateFindItemTypeForReminders(ExchangeFolder folder)
		{
			FindItemType findItemType = new FindItemType();
			findItemType.Traversal = ItemQueryTraversalType.Shallow;

			findItemType.ItemShape = new ItemResponseShapeType();
			findItemType.ItemShape.BaseShape = DefaultShapeNamesType.IdOnly;

			List<BasePathToElementType> additionalProperties = new List<BasePathToElementType>();
			EWSUtilities.AppendAdditionalProperty(additionalProperties, UnindexedFieldURIType.itemParentFolderId);
			findItemType.ItemShape.AdditionalProperties = additionalProperties.ToArray();

			findItemType.Restriction = folder.CreateRestrictionForGettingReminders(DateTime.UtcNow);
			findItemType.ParentFolderIds = new BaseFolderIdType[] { folder.FolderId };
			return findItemType;
		} 

		#endregion  // CreateFindItemTypeForReminders

		#region DoesCalendarHaveId

		private static bool DoesCalendarHaveId(ExchangeFolder folder, FolderIdType folderId)
		{
			if (folder != null &&
				folder.FolderId != null &
				folder.FolderId.Id == folderId.Id)
			{
				return true;
			}

			return false;
		} 

		#endregion  // DoesCalendarHaveId

		#region FindNextAncestors

		private void FindNextAncestors(
			List<FolderAncestorInfo> folderAncestorInfos,
			Action<List<FolderAncestorInfo>> onFindNextAncestorsCompleted,
			ErrorCallback onError)
		{
			GetFolderType getFolderType = new GetFolderType();

			getFolderType.FolderIds = new BaseFolderIdType[folderAncestorInfos.Count];
			for (int i = 0; i < folderAncestorInfos.Count; i++)
				getFolderType.FolderIds[i] = folderAncestorInfos[i].CurrentAncestorId;

			getFolderType.FolderShape = new FolderResponseShapeType();
			getFolderType.FolderShape.BaseShape = DefaultShapeNamesType.IdOnly;
			getFolderType.FolderShape.AdditionalProperties = new BasePathToElementType[] { 
				EWSUtilities.CreateProperty(UnindexedFieldURIType.folderParentFolderId) };

			FindNextAncestorsUserState state = new FindNextAncestorsUserState(this, folderAncestorInfos, onFindNextAncestorsCompleted, onError);
			_serviceBinding.GetFolderAsync(getFolderType, state);
		} 

		#endregion  // FindNextAncestors

		#region FindRemindersFolder

		private void FindRemindersFolder(ErrorCallback onError)
		{
			if (this.RemindersFolder != null)
				return;

			FindFolderType findFolder = new FindFolderType();
			findFolder.Traversal = FolderQueryTraversalType.Shallow;

			findFolder.FolderShape = new FolderResponseShapeType();
			findFolder.FolderShape.BaseShape = DefaultShapeNamesType.AllProperties;

			findFolder.Restriction = new RestrictionType();
			IsEqualToType searchFilter = new IsEqualToType();
			searchFilter.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.folderFolderClass);
			searchFilter.FieldURIOrConstant = EWSUtilities.CreateConstant(ExchangeRemindersFolder.FolderClass);
			findFolder.Restriction.Item = searchFilter;

			findFolder.ParentFolderIds = new BaseFolderIdType[] { EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.root) };

			FindRemindersFolderUserState state = new FindRemindersFolderUserState(this, onError);
			_serviceBinding.FindFolderAsync(findFolder, state);
		}

		#endregion  // FindRemindersFolder

		#region GetFolderFromResponse

		private static T GetFolderFromResponse<T>(FolderInfoResponseMessageType folderInfoResponseMessage) where T : BaseFolderType
		{
			if (folderInfoResponseMessage.Folders.Length == 0)
			{
				ExchangeConnectorUtilities.DebugFail("Could not find the default folder of the user.");
				return null;
			}

			T defaultFolder = folderInfoResponseMessage.Folders[0] as T;
			Debug.Assert(defaultFolder != null, "Unknown type of default folder: " + folderInfoResponseMessage.Folders[0].GetType());
			return defaultFolder;
		} 

		#endregion  // GetFolderFromResponse

		#region OnServiceBindingCreateItemCompleted

		private void OnServiceBindingCreateItemCompleted(object sender, CreateItemCompletedEventArgs e)
		{
			CreateItemUserState state = (CreateItemUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingCreateItemCompleted

		#region OnServiceBindingDeleteItemCompleted

		private void OnServiceBindingDeleteItemCompleted(object sender, DeleteItemCompletedEventArgs e)
		{
			DeleteItemUserState state = (DeleteItemUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingDeleteItemCompleted

		#region OnServiceBindingFindFolderCompleted

		private void OnServiceBindingFindFolderCompleted(object sender, FindFolderCompletedEventArgs e)
		{
			FindFolderUserState state = (FindFolderUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingFindFolderCompleted

		#region OnServiceBindingFindItemCompleted

		private void OnServiceBindingFindItemCompleted(object sender, FindItemCompletedEventArgs e)
		{
			FindItemUserState state = (FindItemUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingFindItemCompleted

		#region OnServiceBindingGetEventsCompleted

		private void OnServiceBindingGetEventsCompleted(object sender, GetEventsCompletedEventArgs e)
		{
			GetEventsUserState state = (GetEventsUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingGetEventsCompleted

		#region OnServiceBindingGetFolderCompleted

		private void OnServiceBindingGetFolderCompleted(object sender, GetFolderCompletedEventArgs e)
		{
			GetFolderUserState state = (GetFolderUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingGetFolderCompleted

		#region OnServiceBindingGetItemCompleted

		private void OnServiceBindingGetItemCompleted(object sender, GetItemCompletedEventArgs e)
		{
			GetItemUserState state = (GetItemUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingGetItemCompleted

		#region OnServiceBindingGetUserAvailabilityCompleted

		private void OnServiceBindingGetUserAvailabilityCompleted(object sender, GetUserAvailabilityCompletedEventArgs e)
		{
			GetUserAvailabilityUserState state = (GetUserAvailabilityUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingGetUserAvailabilityCompleted

		#region OnServiceBindingGetUserConfigurationCompleted

		private void OnServiceBindingGetUserConfigurationCompleted(object sender, GetUserConfigurationCompletedEventArgs e)
		{
			GetUserConfigurationUserState state = (GetUserConfigurationUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion //OnServiceBindingGetUserConfigurationCompleted

		#region OnServiceBindingMoveItemCompleted

		private void OnServiceBindingMoveItemCompleted(object sender, MoveItemCompletedEventArgs e)
		{
			MoveItemUserState state = (MoveItemUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingMoveItemCompleted

		#region OnServiceBindingResolveNamesCompleted

		private void OnServiceBindingResolveNamesCompleted(object sender, ResolveNamesCompletedEventArgs e)
		{
			ResolveNamesUserState state = (ResolveNamesUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingResolveNamesCompleted

		#region OnServiceBindingSubscribeCompleted

		private void OnServiceBindingSubscribeCompleted(object sender, SubscribeCompletedEventArgs e)
		{
			SubscribeUserState state = (SubscribeUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingSubscribeCompleted

		#region OnServiceBindingUnsubscribeCompleted

		private void OnServiceBindingUnsubscribeCompleted(object sender, UnsubscribeCompletedEventArgs e)
		{
			UnsubscribeUserState state = (UnsubscribeUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingUnsubscribeCompleted

		#region OnServiceBindingUpdateItemCompleted

		private void OnServiceBindingUpdateItemCompleted(object sender, UpdateItemCompletedEventArgs e)
		{
			UpdateItemsUserState state = (UpdateItemsUserState)e.UserState;
			state.ProcessResponse(e);
		}

		#endregion  // OnServiceBindingUpdateItemCompleted

		#region OnServiceBindingUpdateUserConfigurationCompleted

		private void OnServiceBindingUpdateUserConfigurationCompleted(object sender, UpdateUserConfigurationCompletedEventArgs e)
		{
			UpdateUserConfigurationUserState state = (UpdateUserConfigurationUserState)e.UserState;
			state.ProcessResponse(e);
		} 

		#endregion //OnServiceBindingUpdateUserConfigurationCompleted

		#region ResolveFolder

		private ExchangeFolder ResolveFolder(ExchangeFolder owningFolder, ItemType item)
		{
			if (owningFolder == this.CalendarFolder)
			{
				Type itemType = item.GetType();

				if (itemType == typeof(TaskType))
					return this.TasksFolder;

				if (itemType == typeof(MessageType))
					return this.JournalFolder;
			}

			return owningFolder;
		} 

		#endregion  // ResolveFolder

		#region SubscribeToPullNotifications

		private void SubscribeToPullNotifications(ErrorCallback onError)
		{
			SubscribeType subscribe = new SubscribeType();
			PullSubscriptionRequestType pullSubscriptionRequest = new PullSubscriptionRequestType();
			pullSubscriptionRequest.EventTypes = new NotificationEventTypeType[]
			{
				NotificationEventTypeType.CopiedEvent,
				NotificationEventTypeType.CreatedEvent,
				NotificationEventTypeType.DeletedEvent,
				NotificationEventTypeType.ModifiedEvent,
				NotificationEventTypeType.MovedEvent,
			};

			List<BaseFolderIdType> folderIds = new List<BaseFolderIdType>();
			folderIds.Add(EWSUtilities.CreateDistinguishedFolderId(DistinguishedFolderIdNameType.calendar));

			// Note: We do not need to subscribe to notifications for the reminders folder, because when items are 
			// added or removed from the reminders folder, we will also get the change notification for the item
			// in the main folder.

			if (this.JournalFolder != null)
				folderIds.Add(this.JournalFolder.FolderId);

			if (this.TasksFolder != null)
				folderIds.Add(this.TasksFolder.FolderId);

			if (_nonDefaultFolders != null)
			{
				foreach (ExchangeFolder folder in _nonDefaultFolders)
					folderIds.Add(folder.FolderId);
			}

			pullSubscriptionRequest.FolderIds = folderIds.ToArray();

			TimeSpan pollingInterval = _connector.PollingInterval;

			if (pollingInterval.Equals(TimeSpan.Zero))
				pullSubscriptionRequest.Timeout = 10;
			else
				pullSubscriptionRequest.Timeout = Math.Min(1, (int)Math.Ceiling(_connector.PollingInterval.TotalMinutes * 2));

			subscribe.Item = pullSubscriptionRequest;

			SubscribeUserState state = new SubscribeUserState(this, onError);
			_serviceBinding.SubscribeAsync(subscribe, state);
		}

		#endregion  // SubscribeToPullNotifications

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region AssociatedUser

		public ExchangeUser AssociatedUser
		{
			get { return _associatedUser; }
		} 

		#endregion // AssociatedUser

		#region CalendarFolder

		public ExchangeCalendarFolder CalendarFolder
		{
			get { return _calendarFolder; }
		}

		#endregion  // CalendarFolder 

		#region Connector

		public ExchangeScheduleDataConnector Connector
		{
			get { return _connector; }
		} 

		#endregion  // Connector

		#region JournalFolder

		public ExchangeJournalFolder JournalFolder
		{
			get { return _journalFolder; }
		}

		#endregion  // JournalFolder

		#region NonDefaultFolders

		public List<ExchangeFolder> NonDefaultFolders
		{
			get { return _nonDefaultFolders; }
		}

		#endregion  // NonDefaultFolders

		#region RemindersFolder

		public ExchangeRemindersFolder RemindersFolder
		{
			get { return _remindersFolder; }
		}

		#endregion  // RemindersFolder

		#region Resource

		public Resource Resource
		{
			get { return _resource; }
		} 

		#endregion //Resource

		#region ServiceBinding

		public ExchangeServiceBindingInternal ServiceBinding
		{
			get { return _serviceBinding; }
		} 

		#endregion // ServiceBinding

		#region TasksFolder

		public ExchangeTasksFolder TasksFolder
		{
			get { return _tasksFolder; }
		} 

		#endregion  // TasksFolder

		#endregion  // Properties


		#region FolderAncestorInfo class

		private class FolderAncestorInfo
		{
			private FolderIdType _currentAncestorId;
			private BaseFolderType _folder;
			private FolderIdType _nextAncestorId;

			public FolderAncestorInfo(BaseFolderType folder, FolderIdType currentAncestorId)
			{
				_folder = folder;
				_currentAncestorId = currentAncestorId;
			}

			public FolderIdType CurrentAncestorId
			{
				get { return _currentAncestorId; }
			}

			public BaseFolderType Folder
			{
				get { return _folder; }
			}

			public FolderIdType NextAncestorId
			{
				get { return _nextAncestorId; }
				set { _nextAncestorId = value; }
			}
		}

		#endregion  // FolderAncestorInfo class

		#region PullNotificationInfo class

		private class PullNotificationInfo
		{
			private string subscriptionId;
			private string watermark;

			public PullNotificationInfo(string subscriptionId, string watermark)
			{
				this.subscriptionId = subscriptionId;
				this.watermark = watermark;
			}

			public string SubscriptionId
			{
				get { return this.subscriptionId; }
			}

			public string Watermark
			{
				get { return this.watermark; }
				set { this.watermark = value; }
			}
		}

		#endregion  // PullNotificationInfo class
	}

	internal delegate bool ErrorCallback(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error);
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