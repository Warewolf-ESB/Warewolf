using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows.Media;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services.Schedules
{
	#region ActivityBaseSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents an activity.
	/// </summary>
	[DataContract]
	[KnownType(typeof(AppointmentSerializableItem))]
	[KnownType(typeof(JournalSerializableItem))]
	[KnownType(typeof(TaskSerializableItem))]
	public abstract class ActivityBaseSerializableItem : SerializableItemBase
	{
		// MD 2/9/11 - TFS65718
		/// <summary>
		/// The Categories property value of the associated resource.
		/// </summary>
		[DataMember]
		public string Categories;

		/// <summary>
		/// The Description property value of the associated activity.
		/// </summary>
		[DataMember]
		public string Description;

		/// <summary>
		/// The End property value of the associated activity.
		/// </summary>
		[DataMember]
		public DateTime End;

		/// <summary>
		/// The EndTimeZoneId property value of the associated activity.
		/// </summary>
		[DataMember]
		public string EndTimeZoneId;

		/// <summary>
		/// The IsLocked property value of the associated activity.
		/// </summary>
		[DataMember]
		public bool? IsLocked;

		/// <summary>
		/// The IsOccurrenceDeleted property value of the associated activity.
		/// </summary>
		[DataMember]
		public bool IsOccurrenceDeleted;

		/// <summary>
		/// The IsTimeZoneNeutral property value of the associated activity.
		/// </summary>
		[DataMember]
		public bool IsTimeZoneNeutral;

		/// <summary>
		/// The IsVisible property value of the associated activity.
		/// </summary>
		[DataMember]
		public bool? IsVisible;

		/// <summary>
		/// The LastModifiedTime property value of the associated activity.
		/// </summary>
		[DataMember]
		public DateTime LastModifiedTime;

		/// <summary>
		/// The MaxOccurrenceDateTime property value of the associated activity.
		/// </summary>
		[DataMember]
		public DateTime? MaxOccurrenceDateTime;

		/// <summary>
		/// The OriginalOccurrenceEnd property value of the associated activity.
		/// </summary>
		[DataMember]
		// MD 10/5/10 - TS50092
		// We need to support nulls as the default value since the .NET min value for DateTimes is less than the SQl min value for DateTimes.
		//public DateTime OriginalOccurrenceEnd;
		public DateTime? OriginalOccurrenceEnd;

		/// <summary>
		/// The OriginalOccurrenceStart property value of the associated activity.
		/// </summary>
		[DataMember]
		// MD 10/5/10 - TS50092
		// We need to support nulls as the default value since the .NET min value for DateTimes is less than the SQl min value for DateTimes.
		//public DateTime OriginalOccurrenceStart;
		public DateTime? OriginalOccurrenceStart;

		/// <summary>
		/// The OwningCalendarId property value of the associated activity.
		/// </summary>
		[DataMember]
		public string OwningCalendarId;

		/// <summary>
		/// The OwningResourceId property value of the associated activity.
		/// </summary>
		[DataMember]
		public string OwningResourceId;

		/// <summary>
		/// The Recurrence property value of the associated activity.
		/// </summary>
		[DataMember]
		public string Recurrence;

		/// <summary>
		/// The RecurrenceVersion property value of the associated activity.
		/// </summary>
		[DataMember]
		public int RecurrenceVersion;

		/// <summary>
		/// The Reminder property value of the associated activity.
		/// </summary>
		[DataMember]
		public string Reminder;

		/// <summary>
		/// The ReminderEnabled property value of the associated activity.
		/// </summary>
		[DataMember]
		public bool ReminderEnabled;

		/// <summary>
		/// The ReminderInterval property value of the associated activity.
		/// </summary>
		[DataMember]
		public TimeSpan ReminderInterval;

		/// <summary>
		/// The RootActivityId property value of the associated activity.
		/// </summary>
		[DataMember]
		public string RootActivityId;

		/// <summary>
		/// The Start property value of the associated activity.
		/// </summary>
		[DataMember]
		public DateTime Start;

		/// <summary>
		/// The StartTimeZoneId property value of the associated activity.
		/// </summary>
		[DataMember]
		public string StartTimeZoneId;

		/// <summary>
		/// The Subject property value of the associated activity.
		/// </summary>
		[DataMember]
		public string Subject;

		/// <summary>
		/// The VariantProperties property value of the associated activity.
		/// </summary>
		[DataMember]
		public long VariantProperties;

		internal abstract ActivityType ActivityType { get; }

		internal ActivityBaseSerializableItem Clone()
		{
			return (ActivityBaseSerializableItem)this.MemberwiseClone();
		}

		internal override bool HasSameData(SerializableItemBase other)
		{
			ActivityBaseSerializableItem otherActivity = other as ActivityBaseSerializableItem;

			if (otherActivity == null)
				return false;

			// MD 2/9/11 - TFS65718
			if (this.Categories != otherActivity.Categories)
				return false;

			if (this.Description != otherActivity.Description)
				return false;

			if (this.End != otherActivity.End)
				return false;

			if (this.EndTimeZoneId != otherActivity.EndTimeZoneId)
				return false;

			if (this.IsLocked != otherActivity.IsLocked)
				return false;

			if (this.IsOccurrenceDeleted != otherActivity.IsOccurrenceDeleted)
				return false;

			if (this.IsTimeZoneNeutral != otherActivity.IsTimeZoneNeutral)
				return false;

			if (this.IsVisible != otherActivity.IsVisible)
				return false;

			if (this.LastModifiedTime != otherActivity.LastModifiedTime)
				return false;

			if (this.MaxOccurrenceDateTime != otherActivity.MaxOccurrenceDateTime)
				return false;

			if (this.OriginalOccurrenceEnd != otherActivity.OriginalOccurrenceEnd)
				return false;

			if (this.OriginalOccurrenceStart != otherActivity.OriginalOccurrenceStart)
				return false;

			if (this.OwningCalendarId != otherActivity.OwningCalendarId)
				return false;

			if (this.OwningResourceId != otherActivity.OwningResourceId)
				return false;

			if (this.Recurrence != otherActivity.Recurrence)
				return false;

			if (this.RecurrenceVersion != otherActivity.RecurrenceVersion)
				return false;

			if (this.Reminder != otherActivity.Reminder)
				return false;

			if (this.ReminderEnabled != otherActivity.ReminderEnabled)
				return false;

			if (this.ReminderInterval != otherActivity.ReminderInterval)
				return false;

			if (this.RootActivityId != otherActivity.RootActivityId)
				return false;

			if (this.Start != otherActivity.Start)
				return false;

			if (this.StartTimeZoneId != otherActivity.StartTimeZoneId)
				return false;

			if (this.Subject != otherActivity.Subject)
				return false;

			if (this.VariantProperties != otherActivity.VariantProperties)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // ActivityBaseSerializableItem class

	// MD 2/9/11 - TFS65718
	#region ActivityCategorySerializableItem class

	/// <summary>
	/// A WCF serializable class which represents an activity category.
	/// </summary>
	[DataContract]
	public class ActivityCategorySerializableItem : SerializableItemBase
	{
		/// <summary>
		/// The CategoryName property value of the associated activity category.
		/// </summary>
		[DataMember]
		public string CategoryName;

		/// <summary>
		/// The Color property value of the associated activity category.
		/// </summary>
		[DataMember]
		public Color? Color;

		/// <summary>
		/// The Description property value of the associated activity category.
		/// </summary>
		[DataMember]
		public string Description;

		internal override bool HasSameData(SerializableItemBase other)
		{
			ActivityCategorySerializableItem otherActivityCategory = other as ActivityCategorySerializableItem;

			if (otherActivityCategory == null)
				return false;

			if (this.CategoryName != otherActivityCategory.CategoryName)
				return false;

			if (this.Color != otherActivityCategory.Color)
				return false;

			if (this.Description != otherActivityCategory.Description)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // ActivityCategorySerializableItem class

	#region ActivityListType enum

	/// <summary>
	/// Represents a type of activity list or items source.
	/// </summary>
	public enum ActivityListType
	{
		/// <summary>
		/// The appointment list.
		/// </summary>
		Appointment,

		/// <summary>
		/// The journal list.
		/// </summary>
		Journal,

		/// <summary>
		/// The recurring appointment list.
		/// </summary>
		RecurringAppointment,

		/// <summary>
		/// The task list.
		/// </summary>
		Task
	}

	#endregion // ActivityListType enum

	#region AppointmentSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents an appointment.
	/// </summary>
	[DataContract]
	public class AppointmentSerializableItem : ActivityBaseSerializableItem
	{
		/// <summary>
		/// The Location property value of the associated appointment.
		/// </summary>
		[DataMember]
		public string Location;

		internal override ActivityType ActivityType
		{
			get { return ActivityType.Appointment; }
		}

		internal override bool HasSameData(SerializableItemBase other)
		{
			AppointmentSerializableItem otherAppointment = other as AppointmentSerializableItem;

			if (otherAppointment == null)
				return false;

			if (this.Location != otherAppointment.Location)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // AppointmentSerializableItem class

	#region CallContext class

	/// <summary>
	/// The base class for client context information for all remote calls.
	/// </summary>
	[DataContract]
	[KnownType(typeof(GetInitialInfoContext))]
	[KnownType(typeof(PerformActivityOperationContext))]
	[KnownType(typeof(PollForItemSourceChangesContext))]
	[KnownType(typeof(PollForItemSourceChangesDetailedContext))]
	[KnownType(typeof(QueryActivitiesContext))]
	public class CallContext
	{
		/// <summary>
		/// Contains the security information from the client or null if the service does not require 
		/// security information to process messages.
		/// </summary>
		[DataMember]
		public string SecurityToken;
	}

	#endregion  // CallContext class

	#region CallResult class

	/// <summary>
	/// The base class for remote call result data for all remote calls.
	/// </summary>
	[DataContract]
	[KnownType(typeof(GetInitialInfoResult))]
	[KnownType(typeof(PerformActivityOperationResult))]
	[KnownType(typeof(PollForItemSourceChangesResult))]
	[KnownType(typeof(PollForItemSourceChangesDetailedResult))]
	[KnownType(typeof(QueryActivitiesResult))]
	public class CallResult
	{
		/// <summary>
		/// Information about any exception that may have been thrown during the remote call.
		/// </summary>
		[DataMember]
		public WcfRemoteErrorInfo ErrorInfo;
	}

	#endregion  // CallResult class

	#region DetailedItemSourceChangeInfo Class

	/// <summary>
	/// Represents detailed information about what has changed in an item source.
	/// </summary>
	[DataContract]
	public class DetailedItemSourceChangeInfo
	{
		/// <summary>
		/// The current version information of the list manager.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo VersionInfo;

		/// <summary>
		/// The list of changes or null if no changes have occurred.
		/// </summary>
		[DataMember]
		public List<ItemSourceChange> Changes;
	}

	#endregion  // DetailedItemSourceChangeInfo Class

	#region GetInitialInfoContext class

	/// <summary>
	/// The client context information and all paramters needed by the <see cref="IWcfListConnectorService.GetInitialInfo"/> 
	/// operation contract.
	/// </summary>
	[DataContract]
	public class GetInitialInfoContext : CallContext
	{
		/// <summary>
		/// The types of information needed by the caller.
		/// </summary>
		[DataMember]
		public InitialInfoTypes RequestedInfo;
	}

	#endregion  // GetInitialInfoContext class

	#region GetInitialInfoResult class

	/// <summary>
	/// The result information for the <see cref="IWcfListConnectorService.GetInitialInfo"/> operation contract.
	/// </summary>
	[DataContract]
	public class GetInitialInfoResult : CallResult
	{
		// MD 2/9/11 - TFS65718
		/// <summary>
		/// An enumerable collection of all activity categories in the database.
		/// </summary>
		[DataMember]
		public IEnumerable<ActivityCategorySerializableItem> ActivityCategoryItemsSource;

		// MD 2/9/11 - TFS65718
		/// <summary>
		/// The current version information of the activity category items.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo ActivityCategoryItemsVersionInfo;

		/// <summary>
		/// Information about the item sources.
		/// </summary>
		[DataMember]
		public Dictionary<ItemSourceType, ItemSourceInfo> ItemSourceInfos;

		/// <summary>
		/// An enumerable collection of all resource calendars in the database.
		/// </summary>
		[DataMember]
		public IEnumerable<ResourceCalendarSerializableItem> ResourceCalendarItemsSource;

		/// <summary>
		/// The current version information of the resource calendar items.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo ResourceCalendarItemsVersionInfo;

		/// <summary>
		/// An enumerable collection of all resources in the database.
		/// </summary>
		[DataMember]
		public IEnumerable<ResourceSerializableItem> ResourceItemsSource;

		/// <summary>
		/// The current version information of the resource items.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo ResourceItemsVersionInfo;
	}

	#endregion  // GetInitialInfoResult class

	#region InitialInfoTypes enum

	/// <summary>
	/// Bit flags which specify types of information needed by a client.
	/// </summary>
	[Flags]
	public enum InitialInfoTypes
	{
		/// <summary>
		/// The ResourceItemSource is needed by the client.
		/// </summary>
		ResourceItemSource = 1,

		/// <summary>
		/// The ResourceCalendarItemSource is needed by the client.
		/// </summary>
		ResourceCalendarItemSource = 2,

		/// <summary>
		/// Information about all the items sources is needed by the client.
		/// </summary>
		ItemSourceInfos = 4,

		// MD 2/9/11 - TFS65718
		/// <summary>
		/// The ActivityCategoryItemSource is needed by the client.
		/// </summary>
		ActivityCategoryItemSource = 8,

		/// <summary>
		/// All information is needed by the client.
		/// </summary>
		All = -1
	}

	#endregion // InitialInfoTypes enum

	#region ItemSourceChange class

	/// <summary>
	/// Represents a change which has occurred on an item source.
	/// </summary>
	[DataContract]
	public class ItemSourceChange
	{
		/// <summary>
		/// The type of change that has occurred.
		/// </summary>
		[DataMember]
		public ItemSourceChangeType ChangeType;

		/// <summary>
		/// The changed item or null if the <see cref="ChangeType"/> is <see cref="ItemSourceChangeType"/>.Reset.
		/// </summary>
		[DataMember]
		public SerializableItemBase ChangedItem;
	}

	#endregion  // ItemSourceChange class

	#region ItemSourceChangeType enum

	/// <summary>
	/// Represents a type of change to an item source.
	/// </summary>
	public enum ItemSourceChangeType
	{
		/// <summary>
		/// An item has been added to the item source.
		/// </summary>
		AddItem = ListChangeType.AddItem,

		/// <summary>
		/// An item has been changed on the item source.
		/// </summary>
		ChangeItem = ListChangeType.ChangeItem,

		/// <summary>
		/// An item has been removed from the item source.
		/// </summary>
		RemoveItem = ListChangeType.RemoveItem,

		/// <summary>
		/// The item source has been reset or too many changes to the item source have occurred 
		/// and the client should just requery as if a reset has occurred.
		/// </summary>
		Reset = ListChangeType.Reset,
	}

	#endregion  // ItemSourceChangeType enum

	#region ItemSourceType enum

	/// <summary>
	/// Indentifies a type of item source.
	/// </summary>
	public enum ItemSourceType
	{
		// MD 2/9/11 - TFS65718
		/// <summary>
		/// The activity category item source.
		/// </summary>
		ActivityCategory,

		/// <summary>
		/// The appointment item source.
		/// </summary>
		Appointment,

		/// <summary>
		/// The journal item source.
		/// </summary>
		Journal,

		/// <summary>
		/// The recurring appointment item source.
		/// </summary>
		RecurringAppointment,

		/// <summary>
		/// The resource calendar item source.
		/// </summary>
		ResourceCalendar,

		/// <summary>
		/// The resource item source.
		/// </summary>
		Resource,

		/// <summary>
		/// The task item source.
		/// </summary>
		Task
	}

	#endregion // ItemSourceType enum

	#region ItemSourceInfo class

	/// <summary>
	/// A WCF serializable class which provides information about an items source from the server.
	/// </summary>
	[DataContract]
	public class ItemSourceInfo
	{
		/// <summary>
		/// The current version information of the list manager.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo VersionInfo;

		/// <summary>
		/// A list of properties that have mappings on the server side.
		/// </summary>
		[DataMember]
		public List<string> MappedProperties;
	}

	#endregion  // ItemSourceInfo class

	#region JournalSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents a journal entry.
	/// </summary>
	[DataContract]
	public class JournalSerializableItem : ActivityBaseSerializableItem
	{
		internal override ActivityType ActivityType
		{
			get { return ActivityType.Journal; }
		}
	}

	#endregion  // JournalSerializableItem class

	#region ListManagerVersionInfo class

	/// <summary>
	/// Represents the version of a list manager and it's data.
	/// </summary>
	[DataContract]
	public class ListManagerVersionInfo
	{
		internal ListManagerVersionInfo(ListManagerChangeInformation changeInformation)
		{
			this.ChangeHistoryId = changeInformation.Id;
			this.DataListVersion = changeInformation.DataListVersion;
			this.HasList = changeInformation.HasList;
			this.ListCapabilities = (byte)changeInformation.ListCapabilities;
			this.PropertyMappingsVersion = changeInformation.PropertyMappingsVersion;
		}

		/// <summary>
		/// The Id of the class which manages the data list version number.
		/// </summary>
		[DataMember]
		public Guid ChangeHistoryId;

		/// <summary>
		/// The current version of the item source.
		/// </summary>
		[DataMember]
		public int DataListVersion;

		/// <summary>
		/// Gets or sets the value indicating whether an item source list is specified.
		/// </summary>
		[DataMember]
		public bool HasList;

		/// <summary>
		/// Represents the operations which can be perfromed on the item source. 
		/// </summary>
		[DataMember]
		public byte ListCapabilities;

		/// <summary>
		/// The current version of the property mappings.
		/// </summary>
		[DataMember]
		public int PropertyMappingsVersion;
	}

	#endregion  // ListManagerVersionInfo class

	#region PerformActivityOperationContext class

	/// <summary>
	/// The client context information and all paramters needed by the <see cref="IWcfListConnectorService.PerformActivityOperation"/> 
	/// operation contract.
	/// </summary>
	[DataContract]
	public class PerformActivityOperationContext : CallContext
	{
		/// <summary>
		/// The activity which should be added, updated, or removed.
		/// </summary>
		[DataMember]
		public ActivityBaseSerializableItem Activity;

		/// <summary>
		/// The client's version information of the list manager associated with the activity.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo ClientVersionInfo;

		/// <summary>
		/// The operation to be performed.
		/// </summary>
		[DataMember]
		public ActivityOperation Operation;
	}

	#endregion  // PerformActivityOperationContext class

	#region PerformActivityOperationResult class

	/// <summary>
	/// The result information for the <see cref="IWcfListConnectorService.PerformActivityOperation"/> operation contract.
	/// </summary>
	[DataContract]
	public class PerformActivityOperationResult : CallResult
	{
		/// <summary>
		/// The server's version information of the list associated with the activity after the add operation is complete.
		/// </summary>
		[DataMember]
		public ListManagerVersionInfo NewVersionInfo;

		/// <summary>
		/// During an add or update operation, contains the updated activity information if the service has 
		/// modified the data on the client's activity.
		/// </summary>
		[DataMember]
		public ActivityBaseSerializableItem UpdatedActivityIfChanged;

		/// <summary>
		/// Indicates whether the client's version of the list was out of date when the call was received.
		/// </summary>
		[DataMember]
		public bool VersionWasOutOfDate;
	}

	#endregion  // PerformActivityOperationResult class

	#region PollForItemSourceChangesContext class

	/// <summary>
	/// The client context information and all paramters needed by the <see cref="IWcfListConnectorService.PollForItemSourceChanges"/> 
	/// operation contract.
	/// </summary>
	[DataContract]
	public class PollForItemSourceChangesContext : CallContext
	{

	}

	#endregion  // PollForItemSourceChangesContext class

	#region PollForItemSourceChangesResult class

	/// <summary>
	/// The result information for the <see cref="IWcfListConnectorService.PollForItemSourceChanges"/> operation contract.
	/// </summary>
	[DataContract]
	public class PollForItemSourceChangesResult : CallResult
	{
		/// <summary>
		/// A dictionary of list manager version information keyed by the type of item source.
		/// </summary>
		[DataMember]
		public Dictionary<ItemSourceType, ListManagerVersionInfo> VersionInfos;
	}

	#endregion  // PollForItemSourceChangesResult class

	#region PollForItemSourceChangesDetailedContext class

	/// <summary>
	/// The client context information and all paramters needed by the <see cref="IWcfListConnectorService.PollForItemSourceChangesDetailed"/> 
	/// operation contract.
	/// </summary>
	[DataContract]
	public class PollForItemSourceChangesDetailedContext : CallContext
	{
		/// <summary>
		/// A collection of the versions of the item sources the client's data reflects. 
		/// All changes returned from this method will have occurred after the item source had this version.
		/// </summary>
		[DataMember]
		public Dictionary<ItemSourceType, ListManagerVersionInfo> ClientItemSourceVersions;
	}

	#endregion  // PollForItemSourceChangesDetailedContext class

	#region PollForItemSourceChangesDetailedResult class

	/// <summary>
	/// The result information for the <see cref="IWcfListConnectorService.PollForItemSourceChangesDetailed"/> operation contract.
	/// </summary>
	[DataContract]
	public class PollForItemSourceChangesDetailedResult : CallResult
	{
		/// <summary>
		/// A dictionary of detailed changes to an item source since a specifiec version of it.
		/// </summary>
		[DataMember]
		public Dictionary<ItemSourceType, DetailedItemSourceChangeInfo> DetailedChanges;
	}

	#endregion  // PollForItemSourceChangesDetailedResult class

	#region QueryActivitiesContext class

	/// <summary>
	/// The client context information and all paramters needed by the <see cref="IWcfListConnectorService.QueryActivities"/> 
	/// operation contract.
	/// </summary>
	[DataContract]
	public class QueryActivitiesContext : CallContext
	{
		/// <summary>
		/// The type of activity list which should being queried.
		/// </summary>
		[DataMember]
		public ActivityListType ListType;

		/// <summary>
		/// The XML encoded linq query.
		/// </summary>
		[DataMember]
		public string LinqStatementEncoded;
	}

	#endregion  // QueryActivitiesContext class

	#region QueryActivitiesResult class

	/// <summary>
	/// The result information for the <see cref="IWcfListConnectorService.QueryActivities"/> operation contract.
	/// </summary>
	[DataContract]
	public class QueryActivitiesResult : CallResult
	{
		/// <summary>
		/// An enumerable collection of the activities meeting the conditions on the query.
		/// </summary>
		[DataMember]
		public IEnumerable<ActivityBaseSerializableItem> Activities;
	}

	#endregion  // QueryActivitiesResult class

	#region ResourceCalendarSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents a resource calendar.
	/// </summary>
	[DataContract]
	public class ResourceCalendarSerializableItem : SerializableItemBase
	{
		/// <summary>
		/// The BaseColor property value of the associated resource calendar.
		/// </summary>
		[DataMember]
		public Color? BaseColor;

		/// <summary>
		/// The IsVisible property value of the associated resource calendar.
		/// </summary>
		[DataMember]
		public bool? IsVisible;

		/// <summary>
		/// The Description property value of the associated resource calendar.
		/// </summary>
		[DataMember]
		public string Description;

		/// <summary>
		/// The Name property value of the associated resource calendar.
		/// </summary>
		[DataMember]
		public string Name;

		/// <summary>
		/// The OwningResourceId property value of the associated resource calendar.
		/// </summary>
		[DataMember]
		public string OwningResourceId;

		internal override bool HasSameData(SerializableItemBase other)
		{
			ResourceCalendarSerializableItem otherResourceCalendar = other as ResourceCalendarSerializableItem;

			if (otherResourceCalendar == null)
				return false;

			if (this.BaseColor != otherResourceCalendar.BaseColor)
				return false;

			if (this.IsVisible != otherResourceCalendar.IsVisible)
				return false;

			if (this.Description != otherResourceCalendar.Description)
				return false;

			if (this.Name != otherResourceCalendar.Name)
				return false;

			if (this.OwningResourceId != otherResourceCalendar.OwningResourceId)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // ResourceCalendarSerializableItem class

	#region ResourceSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents a resource.
	/// </summary>
	[DataContract]
	public class ResourceSerializableItem : SerializableItemBase
	{
		// MD 2/9/11 - TFS65718
		/// <summary>
		/// The CustomActivityCategories property value of the associated resource.
		/// </summary>
		[DataMember]
		public string CustomActivityCategories;

		/// <summary>
		/// The DaySettingsOverrides property value of the associated resource.
		/// </summary>
		[DataMember]
		public string DaySettingsOverrides;

		/// <summary>
		/// The DaysOfWeek property value of the associated resource.
		/// </summary>
		[DataMember]
		public string DaysOfWeek;

		/// <summary>
		/// The Description property value of the associated resource.
		/// </summary>
		[DataMember]
		public string Description;

		/// <summary>
		/// The EmailAddress property value of the associated resource.
		/// </summary>
		[DataMember]
		public string EmailAddress;

		/// <summary>
		/// The FirstDayOfWeek property value of the associated resource.
		/// </summary>
		[DataMember]
		public DayOfWeek? FirstDayOfWeek;

		/// <summary>
		/// The IsLocked property value of the associated resource.
		/// </summary>
		[DataMember]
		public bool IsLocked;

		/// <summary>
		/// The IsVisible property value of the associated resource.
		/// </summary>
		[DataMember]
		public bool? IsVisible;

		/// <summary>
		/// The Name property value of the associated resource.
		/// </summary>
		[DataMember]
		public string Name;

		/// <summary>
		/// The PrimaryCalendarId property value of the associated resource.
		/// </summary>
		[DataMember]
		public string PrimaryCalendarId;

		/// <summary>
		/// The PrimaryTimeZoneId property value of the associated resource.
		/// </summary>
		[DataMember]
		public string PrimaryTimeZoneId;

		internal override bool HasSameData(SerializableItemBase other)
		{
			ResourceSerializableItem otherResource = other as ResourceSerializableItem;

			if (otherResource == null)
				return false;

			// MD 2/9/11 - TFS65718
			if (this.CustomActivityCategories != otherResource.CustomActivityCategories)
				return false;

			if (this.DaySettingsOverrides != otherResource.DaySettingsOverrides)
				return false;

			if (this.DaysOfWeek != otherResource.DaysOfWeek)
				return false;

			if (this.Description != otherResource.Description)
				return false;

			if (this.EmailAddress != otherResource.EmailAddress)
				return false;

			if (this.FirstDayOfWeek != otherResource.FirstDayOfWeek)
				return false;

			if (this.IsLocked != otherResource.IsLocked)
				return false;

			if (this.IsVisible != otherResource.IsVisible)
				return false;

			if (this.Name != otherResource.Name)
				return false;

			if (this.PrimaryCalendarId != otherResource.PrimaryCalendarId)
				return false;

			if (this.PrimaryTimeZoneId != otherResource.PrimaryTimeZoneId)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // ResourceSerializableItem class

	#region SerializableItemBase class

	/// <summary>
	/// A base class for any WCF serializable item.
	/// </summary>
	[DataContract]
	[KnownType(typeof(AppointmentSerializableItem))]
	[KnownType(typeof(JournalSerializableItem))]
	[KnownType(typeof(ResourceCalendarSerializableItem))]
	[KnownType(typeof(ResourceSerializableItem))]
	[KnownType(typeof(TaskSerializableItem))]
	public abstract class SerializableItemBase
	{
		/// <summary>
		/// The Id property value of the associated item.
		/// </summary>
		[DataMember]
		public string Id;

		/// <summary>
		/// The UnmappedProperties property value of the associated resource.
		/// </summary>
		[DataMember]
		public string UnmappedProperties;

		internal virtual bool HasSameData(SerializableItemBase other)
		{
			if (other == null)
				return false;

			if (this.Id != other.Id)
				return false;

			if (this.UnmappedProperties != other.UnmappedProperties)
				return false;

			return true;
		}
	}

	#endregion  // SerializableItemBase class

	#region TaskSerializableItem class

	/// <summary>
	/// A WCF serializable class which represents a task.
	/// </summary>
	[DataContract]
	public class TaskSerializableItem : ActivityBaseSerializableItem
	{
		/// <summary>
		/// The PercentComplete property value of the associated task.
		/// </summary>
		[DataMember]
		public int PercentageComplete;

		internal override ActivityType ActivityType
		{
			get { return ActivityType.Task; }
		}

		internal override bool HasSameData(SerializableItemBase other)
		{
			TaskSerializableItem otherTask = other as TaskSerializableItem;

			if (otherTask == null)
				return false;

			if (this.PercentageComplete != otherTask.PercentageComplete)
				return false;

			return base.HasSameData(other);
		}
	}

	#endregion  // TaskSerializableItem class

	#region WcfRemoteErrorInfo class

	/// <summary>
	/// Represents an error thrown when processing a message.
	/// </summary>
	[DataContract]
	public class WcfRemoteErrorInfo
	{
		/// <summary>
		/// The full type name of the exception.
		/// </summary>
		[DataMember]
		public string ExceptionType;

		/// <summary>
		/// The message describing the error.
		/// </summary>
		[DataMember]
		public string Message;

		/// <summary>
		/// The stack tace of the error.
		/// </summary>
		[DataMember]
		public string StackTrace;
	}

	#endregion // WcfRemoteErrorInfo class
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