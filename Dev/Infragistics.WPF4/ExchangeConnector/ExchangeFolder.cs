using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.EWS;
using System.Reflection;

namespace Infragistics.Controls.Schedules
{
	internal abstract class ExchangeFolder
	{
		#region Member Variables

		private ResourceCalendar _associatedCalendar;
		private ExchangeService _service;

		#endregion  // Member Variables

		#region Constructor

		public ExchangeFolder(ExchangeService service)
		{
			_service = service;
		} 

		#endregion  // Constructor

		#region Abstract Members

		protected abstract BasePathToElementType CreatePathToItemEndTime();
		protected abstract BasePathToElementType CreatePathToItemStartTime();
		public abstract BaseFolderType Folder { get; }
		public abstract bool ShouldIncludeItem(string itemClass);

		#endregion  // Abstract Members

		#region Methods

		#region CleanItemForAdd

		public virtual void CleanItemForAdd(ItemType item)
		{
			item.EffectiveRights = null;
			item.ItemId = null;
			item.LastModifiedTimeSpecified = false;
			item.ParentFolderId = null;
		} 

		#endregion //CleanItemForAdd

		#region CreateExtendedPropertyDateFilter

		protected static RestrictionType CreateExtendedPropertyDateFilter(DateRange dateRange, string propertySetId, int startPropertyId, int endPropertyId, bool allowNoStartDate)
		{
			RestrictionType restriction = new RestrictionType();

			IsGreaterThanOrEqualToType endsAfterStartOfRangeCondition = new IsGreaterThanOrEqualToType();
			endsAfterStartOfRangeCondition.FieldURIOrConstant = EWSUtilities.CreateConstant(EWSUtilities.GetExchangeDateTime(dateRange.Start));
			endsAfterStartOfRangeCondition.Item = EWSUtilities.CreateExtendedProperty(
				propertySetId,
				endPropertyId,
				MapiPropertyTypeType.SystemTime);

			IsLessThanType startsBeforeEndOfRangeCondition = new IsLessThanType();
			startsBeforeEndOfRangeCondition.FieldURIOrConstant = EWSUtilities.CreateConstant(EWSUtilities.GetExchangeDateTime(dateRange.End));
			startsBeforeEndOfRangeCondition.Item = EWSUtilities.CreateExtendedProperty(
				propertySetId,
				startPropertyId,
				MapiPropertyTypeType.SystemTime);

			if (allowNoStartDate)
			{
				ExistsType startExists = new ExistsType();
				startExists.Item = EWSUtilities.CreateExtendedProperty(
					propertySetId,
					startPropertyId,
					MapiPropertyTypeType.SystemTime);
				NotType startDoesntExist = new NotType();
				startDoesntExist.Item = startExists;

				restriction.Item = EWSUtilities.AndCondition(
					endsAfterStartOfRangeCondition, 
					EWSUtilities.OrCondition(startsBeforeEndOfRangeCondition, startDoesntExist));
			}
			else
			{
				restriction.Item = EWSUtilities.AndCondition(endsAfterStartOfRangeCondition, startsBeforeEndOfRangeCondition);
			}

			return restriction;
		} 

		#endregion  // CreateExtendedPropertyDateFilter

		#region CreateItem

		public void CreateItem(ItemType item, Action<ItemType> onCreateItemsCompleted, ErrorCallback onError)
		{
			this.Service.CreateItem(this, item, onCreateItemsCompleted, onError);
		}

		#endregion  // CreateItem 

		#region CreateReminderBracketCondition

		private SearchExpressionType CreateReminderBracketCondition(
			DateTime baseTime,
			TimeSpan currentReminderBracket,
			TimeSpan? previousReminderBracket)
		{
			DateTime appointmentStartUpperBound = baseTime + currentReminderBracket;
			int reminderLowerBoundMinutes = previousReminderBracket.HasValue ? (int)previousReminderBracket.Value.TotalMinutes : 0;
			int reminderUpperBoundMinutes = (int)currentReminderBracket.TotalMinutes;

			IsLessThanOrEqualToType appointmentWithinTimeRange = new IsLessThanOrEqualToType();
			appointmentWithinTimeRange.Item = this.CreatePathToItemStartTime();
			appointmentWithinTimeRange.FieldURIOrConstant = EWSUtilities.CreateConstant(EWSUtilities.GetExchangeDateTime(appointmentStartUpperBound));

			TwoOperandExpressionType reminderLowerBoundCondition = reminderLowerBoundMinutes == 0
				? (TwoOperandExpressionType)new IsGreaterThanOrEqualToType()
				: (TwoOperandExpressionType)new IsGreaterThanType();
			reminderLowerBoundCondition.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.itemReminderMinutesBeforeStart);
			reminderLowerBoundCondition.FieldURIOrConstant = EWSUtilities.CreateConstant(reminderLowerBoundMinutes.ToString());

			IsLessThanOrEqualToType reminderUpperBoundCondition = new IsLessThanOrEqualToType();
			reminderUpperBoundCondition.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.itemReminderMinutesBeforeStart);
			reminderUpperBoundCondition.FieldURIOrConstant = EWSUtilities.CreateConstant(reminderUpperBoundMinutes.ToString());

			return EWSUtilities.AndCondition(appointmentWithinTimeRange, reminderLowerBoundCondition, reminderUpperBoundCondition);
		}

		#endregion  // CreateReminderBracketCondition

		#region CreateRestrictionForGettingReminders

		public virtual RestrictionType CreateRestrictionForGettingReminders(DateTime baseTime)
		{
			IsEqualToType reminderIsSetCondition = new IsEqualToType();
			reminderIsSetCondition.Item = EWSUtilities.CreateProperty(UnindexedFieldURIType.itemReminderIsSet);
			reminderIsSetCondition.FieldURIOrConstant = EWSUtilities.CreateConstant("true");

			TimeSpan[] reminderBrackets = new TimeSpan[]
			{
				TimeSpan.FromHours( 1 ),
				TimeSpan.FromDays( 1 ),
				TimeSpan.FromDays( 7 ),
				TimeSpan.FromDays( 28 ),
				TimeSpan.FromDays( 365 )
			};

			TimeSpan? previousReminderBracket = null;

			List<SearchExpressionType> allowedReminderConditions = new List<SearchExpressionType>();

			// Include all appointments ending in the past.
			IsLessThanType appointmentEndsInPast = new IsLessThanType();
			appointmentEndsInPast.Item = this.CreatePathToItemEndTime();
			appointmentEndsInPast.FieldURIOrConstant = EWSUtilities.CreateConstant(EWSUtilities.GetExchangeDateTime(baseTime));
			allowedReminderConditions.Add(appointmentEndsInPast);

			for (int i = 0; i < reminderBrackets.Length; i++)
			{
				TimeSpan currentReminderBracket = reminderBrackets[i];
				allowedReminderConditions.Add(
					this.CreateReminderBracketCondition(baseTime, currentReminderBracket, previousReminderBracket)
					);
				previousReminderBracket = currentReminderBracket;
			}

			RestrictionType restriction = new RestrictionType();
			restriction.Item = EWSUtilities.AndCondition(
				reminderIsSetCondition,
				EWSUtilities.OrCondition(allowedReminderConditions.ToArray()));
			return restriction;
		}

		#endregion  // CreateRestrictionForGettingReminders

		#region CreateRestrictionForDateRange

		public virtual RestrictionType CreateRestrictionForDateRange(DateRange dateRange)
		{
			return null;
		} 

		#endregion  // CreateRestrictionForDateRange

		#region CreateViewForDateRange

		public virtual BasePagingType CreateViewForDateRange(DateRange dateRange)
		{
			return new IndexedPageViewType();
		} 

		#endregion  // CreateViewForDateRange

		#region GetAppointmentsInRange

		public void GetAppointmentsInRange(
			DateRange dateRange,
			Action<ExchangeFolder, IList<ItemType>> onGetAppointmentsInRangeCompleted,
			ErrorCallback onError)
		{
			this.Service.GetAppointmentsInRange(this, dateRange, onGetAppointmentsInRangeCompleted, onError);
		}

		#endregion  // GetAppointmentsInRange

		#region GetItemPropertyName

		public virtual string GetItemPropertyName(UnindexedFieldURIType propertyType)
		{
			string propertyName = Enum.GetName(typeof(UnindexedFieldURIType), propertyType);

			if (propertyName.StartsWith("item"))
				return propertyName.Substring(4);

			return null;
		}

		#endregion  // GetItemPropertyName

		#region GetItems

		public void GetItems(
			IList<BaseItemIdType> itemIds,
			Action<ExchangeFolder, IList<ItemType>> onGetItemsCompleted,
			ErrorCallback onError)
		{
			this.Service.GetItems(this, itemIds, onGetItemsCompleted, onError);
		}

		#endregion //GetItems

		#region GetItemUpdates

		public List<ItemChangeDescriptionType> GetItemUpdates(ItemType item, ItemType originalItem)
		{
			TimeZoneInfoProvider provider = this.Service.Connector.TimeZoneInfoProviderResolved;

			ExchangeVersion version = this.Service.Connector.RequestedServerVersionResolved;

			TimeZoneToken newStartToken;
			TimeZoneToken newEndToken;
			EWSUtilities.GetTimeZones(item, provider, out newStartToken, out newEndToken);

			TimeZoneToken originalStartToken;
			TimeZoneToken originalEndToken;
			EWSUtilities.GetTimeZones(originalItem, provider, out originalStartToken, out originalEndToken);

			List<ItemChangeDescriptionType> updates = new List<ItemChangeDescriptionType>();

			bool setEnd = false;
			bool setEndTimeZone = false;
			bool setIsAllDayEvent = false;
			bool setMeetingTimeZone = false;
			bool setRecurrence = false;
			bool setStart = false;
			bool setStartTimeZone = false;
			

			Type itemType = item.GetType();
			foreach (UnindexedFieldURIType propertyType in ExchangeConnectorUtilities.EnumGetValues(typeof(UnindexedFieldURIType)))
			{
				string propertyName = this.GetItemPropertyName(propertyType);

				if (propertyName == null)
					continue;

				switch (propertyName)
				{
					// Never update these fields. They are managed by the server.
					case "EffectiveRights":
					case "IsComplete":
					case "LastModifiedTime":
					case "ParentFolderId":
					case "ReminderDueBy":
					case "TimeZone":
						continue;
				}

				PropertyInfo property = itemType.GetProperty(propertyName);

				if (property == null)
				{
					ExchangeConnectorUtilities.DebugFail("Could not find property: " + propertyName);
					continue;
				}

				object newValue;
				object originalValue;
				switch (propertyName)
				{
					case "MeetingTimeZone":
						if (version == ExchangeVersion.Exchange2007_SP1)
						{
							newValue = newStartToken;
							originalValue = originalStartToken;
						}
						else
						{
							newValue = null;
							originalValue = null;
						}
						break;

					case "EndTimeZone":
						if (version != ExchangeVersion.Exchange2007_SP1)
						{
							newValue = newEndToken;
							originalValue = originalEndToken;
						}
						else
						{
							newValue = null;
							originalValue = null;
						}
						break;

					case "StartTimeZone":
						if (version != ExchangeVersion.Exchange2007_SP1)
						{
							newValue = newStartToken;
							originalValue = originalStartToken;
						}
						else
						{
							newValue = null;
							originalValue = null;
						}
						break;

					default:
						newValue = property.GetValue(item, null);
						originalValue = property.GetValue(originalItem, null);
						break;
				}

				ItemChangeDescriptionType update = null;

				if (newValue != null)
				{
					if (EWSUtilities.AreItemValuesEqual(newValue, originalValue, provider) == false)
						update = EWSUtilities.CreateSetItemField(item, property, propertyType);
				}
				else if (originalValue != null)
				{
					update = EWSUtilities.CreateDeleteItemField(propertyType);
				}

				if (update != null)
				{
					switch (propertyName)
					{
 						case "End":
							setEnd = true;
							break;

						case "EndTimeZone":
							setEndTimeZone = true;
							break;

						case "IsAllDayEvent":
							setIsAllDayEvent = true;
							break;

						case "MeetingTimeZone":
							setMeetingTimeZone = true;
							break;

						case "Recurrence":
							setRecurrence = true;
							break;

						case "Start":
							setStart = true;
							break;

						case "StartTimeZone":
							setStartTimeZone = true;
							break;
					}

					updates.Add(update);
				}
			}

			if (setStart || setEnd || setIsAllDayEvent || setRecurrence)
			{
				if (version == ExchangeVersion.Exchange2007_SP1)
				{
					PropertyInfo meetingTimeZoneProperty = itemType.GetProperty("MeetingTimeZone");

					if (meetingTimeZoneProperty != null && setMeetingTimeZone == false)
						updates.Insert(0, EWSUtilities.CreateSetItemField(item, meetingTimeZoneProperty, UnindexedFieldURIType.calendarMeetingTimeZone));
				}
				else
				{
					bool needsStartTimeZone = setStart || setIsAllDayEvent || setRecurrence;
					bool needsEndTimeZone = setEnd;

					PropertyInfo endTimeZoneProperty = itemType.GetProperty("EndTimeZone");
					PropertyInfo startTimeZoneProperty = itemType.GetProperty("StartTimeZone");

					if (startTimeZoneProperty != null && needsStartTimeZone && setStartTimeZone == false)
						updates.Insert(0, EWSUtilities.CreateSetItemField(item, startTimeZoneProperty, UnindexedFieldURIType.calendarStartTimeZone));

					if (endTimeZoneProperty != null && needsEndTimeZone && setEndTimeZone == false)
						updates.Insert(0, EWSUtilities.CreateSetItemField(item, endTimeZoneProperty, UnindexedFieldURIType.calendarEndTimeZone));
				}
			}

			if (item.ExtendedProperty != null)
			{
				foreach (ExtendedPropertyType extendedProperty in item.ExtendedProperty)
				{
					object newValue = extendedProperty.Item;
					object originalValue = EWSUtilities.GetExtendedPropertyValue(originalItem, extendedProperty.ExtendedFieldURI);

					if (newValue != null)
					{
						if (EWSUtilities.AreItemValuesEqual(newValue, originalValue, provider) == false)
							updates.Add(EWSUtilities.CreateSetItemField(item, extendedProperty));
					}
					else if (originalValue != null)
					{
						updates.Add(EWSUtilities.CreateDeleteItemField(extendedProperty.ExtendedFieldURI));
					}
				}
			}

			return updates;
		} 

		#endregion  // GetItemUpdates

		#region GetNeededProperties

		public virtual void GetNeededProperties(List<BasePathToElementType> neededProperties)
		{
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemBody);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemEffectiveRights);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemItemClass);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemLastModifiedTime);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemParentFolderId);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemReminderDueBy);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemReminderIsSet);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemReminderMinutesBeforeStart);
			EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemSubject);

			if (this.Service.Connector.RequestedServerVersionResolved != ExchangeVersion.Exchange2007_SP1)
			{
				EWSUtilities.AppendAdditionalProperty(neededProperties, UnindexedFieldURIType.itemCategories);
			}
		} 

		#endregion  // GetNeededProperties

		#region IsActivityOperationAllowed

		public bool IsActivityOperationAllowed(ActivityOperation activityOperation)
		{
			EffectiveRightsType effectiveRights = this.Folder.EffectiveRights;

			// If we didn't get the rights, assume we are allowed to do the operation. 
			// The server will give an error if we can't.
			if (effectiveRights == null)
				return true;

			switch (activityOperation)
			{
				case ActivityOperation.Edit:
					// This applies to items, not the folder itself, so just return true.
					return true;

				case ActivityOperation.Add:
					return effectiveRights.CreateContents;

				case ActivityOperation.Remove:
					return effectiveRights.Delete;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ActivityOperation: " + activityOperation);
					return true;
			}
		}

		#endregion  // IsActivityOperationAllowed

		#endregion  // Methods

		#region Properties

		#region AssociatedCalendar

		public ResourceCalendar AssociatedCalendar
		{
			get { return _associatedCalendar; }
			set { _associatedCalendar = value; }
		} 

		#endregion  // AssociatedCalendar

		#region FolderId

		public FolderIdType FolderId
		{
			get
			{
				BaseFolderType folder = this.Folder;

				if (folder == null)
					return null;

				return folder.FolderId; 
			}
		} 

		#endregion  // FolderId

		#region IsRemindersFolder

		public virtual bool IsRemindersFolder
		{
			get { return false; }
		} 

		#endregion  // IsRemindersFolder

		#region Name

		public string Name
		{
			get { return this.Folder.DisplayName; }
		} 

		#endregion  // Name

		#region Service

		public ExchangeService Service
		{
			get { return _service; }
		}

		#endregion  // Service

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