using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Infragistics.Controls.Schedules.EWS;
using System.Diagnostics;
using System.Reflection;
using System.Globalization;
using System.Xml;

namespace Infragistics.Controls.Schedules
{
	internal static class EWSUtilities
	{
		#region Constants

		// MD 4/27/11 - TFS72779
		public const string ExchangeTypesNamespace = "http://schemas.microsoft.com/exchange/services/2006/types"; 

		#endregion  // Constants


		#region ActivityFromItem

		public static ActivityBase ActivityFromItem(ItemType item, ResourceCalendar resourceCalendar, TimeZoneInfoProvider provider)
		{
			ActivityBase activity;
			if (item is CalendarItemType)
			{
				activity = new Appointment();
			}
			else if (item is TaskType)
			{
				activity = new Task();
			}
			else
			{
				MessageType messageItem = item as MessageType;
				if (messageItem != null && messageItem.ItemClass == ExchangeJournalFolder.ItemClass)
				{
					activity = new Journal();
				}
				else
				{
					ExchangeConnectorUtilities.DebugFail("Cannot determine the type of activity to create.");
					return null;
				}
			}

			activity.OwningResource = resourceCalendar.OwningResource;
			activity.OwningCalendar = resourceCalendar;

			EWSUtilities.UpdateActivity(activity, item, provider);
			return activity;
		}

		#endregion  // ActivityFromItem

		#region AndCondition

		public static AndType AndCondition(params SearchExpressionType[] conditions)
		{
			AndType andCondition = new AndType();
			andCondition.Items = conditions;
			return andCondition;
		} 

		#endregion  // AndCondition

		#region AppendAdditionalProperty

		public static void AppendAdditionalProperty(List<BasePathToElementType> additionalProperties, UnindexedFieldURIType field)
		{
			additionalProperties.Add(EWSUtilities.CreateProperty(field));
		}

		internal static void AppendAdditionalProperty(
			List<BasePathToElementType> additionalProperties,
			string propertySetId,
			int propertyId,
			MapiPropertyTypeType propertyType)
		{
			PathToExtendedFieldType extendedProperty = EWSUtilities.CreateExtendedProperty(propertySetId, propertyId, propertyType);
			additionalProperties.Add(extendedProperty);
		}

		#endregion  // AppendAdditionalProperty

		#region ApplyUpdate

		internal static void ApplyUpdate(ExchangeFolder folder, ItemType item, ItemChangeDescriptionType update)
		{
			SetItemFieldType setItem = update as SetItemFieldType;
			DeleteItemFieldType deleteItem = update as DeleteItemFieldType;
			if (setItem == null && deleteItem == null)
			{
				ExchangeConnectorUtilities.DebugFail("Unknown change type.");
				return;
			}

			PathToUnindexedFieldType unindexedFieldPath = update.Item as PathToUnindexedFieldType;
			if (unindexedFieldPath != null)
			{
				Type itemType = item.GetType();

				string propertyName = folder.GetItemPropertyName(unindexedFieldPath.FieldURI);
				PropertyInfo property = itemType.GetProperty(propertyName);

				object value = null;
				if (setItem != null)
					value = property.GetValue(setItem.Item1, null);

				property.SetValue(item, value, null);
				return;
			}

			PathToExtendedFieldType extendedFieldPath = update.Item as PathToExtendedFieldType;
			if (extendedFieldPath != null)
			{
				List<ExtendedPropertyType> extendedProps = new List<ExtendedPropertyType>();
				if (item.ExtendedProperty != null)
					extendedProps.AddRange(item.ExtendedProperty);

				object value = null;
				if (setItem != null)
					value = EWSUtilities.GetExtendedPropertyValue(item, extendedFieldPath);

				EWSUtilities.UpdateExtendedProperty(extendedProps, extendedFieldPath.PropertySetId, extendedFieldPath.PropertyId, extendedFieldPath.PropertyType, value);
				item.ExtendedProperty = extendedProps.ToArray();
			}

			ExchangeConnectorUtilities.DebugFail("Unknown path type.");
		} 

		#endregion // ApplyUpdate

		// MD 4/27/11 - TFS72779
		#region AreCategoriesSupported

		public static bool AreCategoriesSupported(ExchangeVersion version)
		{
			return version > ExchangeVersion.Exchange2007_SP1;
		}

		#endregion //AreCategoriesSupported

		#region AreIdsEqual

		public static bool AreIdsEqual(BaseItemIdType id1, BaseItemIdType id2)
		{
			if (id1.GetType() != id2.GetType())
				return false;

			ItemIdType itemId1 = id1 as ItemIdType;
			if (itemId1 != null)
			{
				ItemIdType itemId2 = (ItemIdType)id2;

				if (itemId1.Id != itemId2.Id)
					return false;

				if (itemId1.ChangeKey != itemId2.ChangeKey)
					return false;

				return true;
			}

			OccurrenceItemIdType occurrenceId1 = id1 as OccurrenceItemIdType;
			if (occurrenceId1 != null)
			{
				OccurrenceItemIdType occurrenceId2 = (OccurrenceItemIdType)id2;

				if (occurrenceId1.RecurringMasterId != occurrenceId2.RecurringMasterId)
					return false;

				if (occurrenceId1.InstanceIndex != occurrenceId2.InstanceIndex)
					return false;

				if (occurrenceId1.ChangeKey != occurrenceId2.ChangeKey)
					return false;

				return true;
			}

			RecurringMasterItemIdType recurringMasterId1 = id1 as RecurringMasterItemIdType;
			if (recurringMasterId1 != null)
			{
				RecurringMasterItemIdType recurringMasterId2 = (RecurringMasterItemIdType)id2;

				if (recurringMasterId1.OccurrenceId != recurringMasterId2.OccurrenceId)
					return false;

				if (recurringMasterId1.ChangeKey != recurringMasterId2.ChangeKey)
					return false;

				return true;
			}

			ExchangeConnectorUtilities.DebugFail("Unknown Item Id");
			return false;
		}

		#endregion  // AreIdsEqual

		#region AreItemValuesEqual

		public static bool AreItemValuesEqual(object newValue, object originalValue, TimeZoneInfoProvider provider)
		{
			if (newValue is DateTime)
			{
				if ((originalValue is DateTime) == false)
					return false;

				DateTime newDateTime = (DateTime)newValue;
				DateTime originalDateTime = (DateTime)originalValue;

				if (newDateTime.Kind == DateTimeKind.Local)
					newDateTime = provider.ConvertLocalToUtc(DateTime.SpecifyKind(newDateTime, DateTimeKind.Unspecified));
				else if (newDateTime.Kind == DateTimeKind.Unspecified)
					newDateTime = provider.ConvertTime(provider.LocalToken, provider.UtcToken, newDateTime);

				if (originalDateTime.Kind == DateTimeKind.Local)
					originalDateTime = provider.ConvertLocalToUtc(DateTime.SpecifyKind(originalDateTime, DateTimeKind.Unspecified));
				else if (originalDateTime.Kind == DateTimeKind.Unspecified)
					originalDateTime = provider.ConvertTime(provider.LocalToken, provider.UtcToken, originalDateTime);

				return newDateTime == originalDateTime;
			}

			BodyType newBody = newValue as BodyType;
			if (newBody != null)
			{
				BodyType originalBody = originalValue as BodyType;

				if (originalBody == null)
					return false;

				if (newBody.BodyType1 != originalBody.BodyType1)
					return false;

				if (String.IsNullOrEmpty(newBody.Value) &&
					String.IsNullOrEmpty(originalBody.Value))
					return true;

				return newBody.Value.Equals(originalBody.Value);
			}

			TaskRecurrenceType newTaskRecurrence = newValue as TaskRecurrenceType;
			if (newTaskRecurrence != null)
			{
				TaskRecurrenceType originalTaskRecurrence = originalValue as TaskRecurrenceType;
				if (originalTaskRecurrence == null)
					return false;

				return EWSUtilities.AreRecurrencesEqual(
					newTaskRecurrence.Item, originalTaskRecurrence.Item,
					newTaskRecurrence.Item1, originalTaskRecurrence.Item1);
			}

			RecurrenceType newRecurrence = newValue as RecurrenceType;
			if (newTaskRecurrence != null)
			{
				RecurrenceType originalRecurrence = originalValue as RecurrenceType;
				if (originalRecurrence == null)
					return false;

				return EWSUtilities.AreRecurrencesEqual(
					newRecurrence.Item, originalRecurrence.Item,
					newRecurrence.Item1, originalRecurrence.Item1);
			}

			string[] newStringArray = newValue as string[];
			if (newStringArray != null)
			{
				string[] originalStringArray = originalValue as string[];
				if (originalStringArray == null)
					return false;

				if (newStringArray.Length != originalStringArray.Length)
					return false;

				for (int i = 0; i < newStringArray.Length; i++)
				{
					if (newStringArray[i] != originalStringArray[i])
						return false;
				}

				return true;
			}

			return newValue.Equals(originalValue);
		}

		#endregion  // AreItemValuesEqual

		#region ArePropertyPathsEqual

		public static bool ArePropertyPathsEqual(BasePathToElementType path1, BasePathToElementType path2)
		{
			if (path1.GetType() != path2.GetType())
				return false;

			PathToUnindexedFieldType unindexedFieldPath1 = path1 as PathToUnindexedFieldType;
			if (unindexedFieldPath1 != null)
			{
				PathToUnindexedFieldType unindexedFieldPath2 = (PathToUnindexedFieldType)path2;
				if (unindexedFieldPath1.FieldURI != unindexedFieldPath2.FieldURI)
					return false;

				return true;
			}

			PathToExtendedFieldType extendedFieldPath1 = path1 as PathToExtendedFieldType;
			if (extendedFieldPath1 != null)
			{
				PathToExtendedFieldType extendedFieldPath2 = (PathToExtendedFieldType)path2;
				if (extendedFieldPath1.DistinguishedPropertySetIdSpecified != extendedFieldPath2.DistinguishedPropertySetIdSpecified)
					return false;

				if (extendedFieldPath1.DistinguishedPropertySetId != extendedFieldPath2.DistinguishedPropertySetId)
					return false;

				if (extendedFieldPath1.PropertyIdSpecified != extendedFieldPath2.PropertyIdSpecified)
					return false;

				if (extendedFieldPath1.PropertyId != extendedFieldPath2.PropertyId)
					return false;

				if (extendedFieldPath1.PropertyName != extendedFieldPath2.PropertyName)
					return false;

				if (extendedFieldPath1.PropertySetId != extendedFieldPath2.PropertySetId)
					return false;

				if (extendedFieldPath1.PropertyTag != extendedFieldPath2.PropertyTag)
					return false;

				if (extendedFieldPath1.PropertyType != extendedFieldPath2.PropertyType)
					return false;

				return true;
			}

			PathToIndexedFieldType indexedFieldPath1 = path1 as PathToIndexedFieldType;
			if (indexedFieldPath1 != null)
			{
				PathToIndexedFieldType indexedFieldPath2 = (PathToIndexedFieldType)path2;
				if (indexedFieldPath1.FieldURI != indexedFieldPath2.FieldURI)
					return false;

				if (indexedFieldPath1.FieldIndex != indexedFieldPath2.FieldIndex)
					return false;

				return true;
			}

			PathToExceptionFieldType exceptionFieldPath1 = path1 as PathToExceptionFieldType;
			if (exceptionFieldPath1 != null)
			{
				PathToExceptionFieldType exceptionFieldPath2 = (PathToExceptionFieldType)path2;
				if (exceptionFieldPath1.FieldURI != exceptionFieldPath2.FieldURI)
					return false;

				return true;
			}

			ExchangeConnectorUtilities.DebugFail("Unknown path type");
			return false;
		}

		#endregion // ArePropertyPathsEqual

		#region AreRecurrencePatternsEqual

		private static bool AreRecurrencePatternsEqual(RecurrencePatternBaseType newPattern, RecurrencePatternBaseType oldPattern)
		{
			if (newPattern.GetType() != oldPattern.GetType())
				return false;

			IntervalRecurrencePatternBaseType newIntervalPattern = newPattern as IntervalRecurrencePatternBaseType;
			if (newIntervalPattern != null)
			{
				IntervalRecurrencePatternBaseType oldIntervalPattern = oldPattern as IntervalRecurrencePatternBaseType;
				if (newIntervalPattern.Interval != oldIntervalPattern.Interval)
					return false;
			}

			AbsoluteMonthlyRecurrencePatternType newAbsoluteMonthlyPattern = newPattern as AbsoluteMonthlyRecurrencePatternType;
			if (newAbsoluteMonthlyPattern != null)
			{
				AbsoluteMonthlyRecurrencePatternType oldAbsoluteMonthlyPattern = oldPattern as AbsoluteMonthlyRecurrencePatternType;
				if (newAbsoluteMonthlyPattern.DayOfMonth != oldAbsoluteMonthlyPattern.DayOfMonth)
					return false;

				return true;
			}

			AbsoluteYearlyRecurrencePatternType newAbsoluteYearlyPattern = newPattern as AbsoluteYearlyRecurrencePatternType;
			if (newAbsoluteYearlyPattern != null)
			{
				AbsoluteYearlyRecurrencePatternType oldAbsoluteYearlyPattern = oldPattern as AbsoluteYearlyRecurrencePatternType;
				if (newAbsoluteYearlyPattern.DayOfMonth != oldAbsoluteYearlyPattern.DayOfMonth)
					return false;

				return true;
			}

			DailyRecurrencePatternType newDailyPattern = newPattern as DailyRecurrencePatternType;
			if (newDailyPattern != null)
			{
				return true;
			}

			DailyRegeneratingPatternType newDailyRegeneratingPattern = newPattern as DailyRegeneratingPatternType;
			if (newDailyRegeneratingPattern != null)
			{
				return true;
			}

			MonthlyRegeneratingPatternType newMonthlyRegeneratingPattern = newPattern as MonthlyRegeneratingPatternType;
			if (newMonthlyRegeneratingPattern != null)
			{
				return true;
			}

			RelativeMonthlyRecurrencePatternType newRelativeMonthlyPattern = newPattern as RelativeMonthlyRecurrencePatternType;
			if (newRelativeMonthlyPattern != null)
			{
				RelativeMonthlyRecurrencePatternType oldRelativeMonthlyPattern = oldPattern as RelativeMonthlyRecurrencePatternType;
				if (newRelativeMonthlyPattern.DayOfWeekIndex != oldRelativeMonthlyPattern.DayOfWeekIndex)
					return false;

				if (newRelativeMonthlyPattern.DaysOfWeek != oldRelativeMonthlyPattern.DaysOfWeek)
					return false;

				return true;
			}

			RelativeYearlyRecurrencePatternType newRelativeYearlyPattern = newPattern as RelativeYearlyRecurrencePatternType;
			if (newRelativeYearlyPattern != null)
			{
				RelativeYearlyRecurrencePatternType oldRelativeYearlyPattern = oldPattern as RelativeYearlyRecurrencePatternType;
				if (newRelativeYearlyPattern.DayOfWeekIndex != oldRelativeYearlyPattern.DayOfWeekIndex)
					return false;

				if (newRelativeYearlyPattern.DaysOfWeek != oldRelativeYearlyPattern.DaysOfWeek)
					return false;

				return true;
			}

			WeeklyRecurrencePatternType newWeeklyPattern = newPattern as WeeklyRecurrencePatternType;
			if (newWeeklyPattern != null)
			{
				WeeklyRecurrencePatternType oldWeeklyPattern = oldPattern as WeeklyRecurrencePatternType;
				if (newWeeklyPattern.DaysOfWeek != oldWeeklyPattern.DaysOfWeek)
					return false;

				if (newWeeklyPattern.FirstDayOfWeek != oldWeeklyPattern.FirstDayOfWeek)
					return false;

				return true;
			}

			WeeklyRegeneratingPatternType newWeeklyRegeneratingPattern = newPattern as WeeklyRegeneratingPatternType;
			if (newWeeklyRegeneratingPattern != null)
			{
				return true;
			}

			YearlyRegeneratingPatternType newYearlyPattern = newPattern as YearlyRegeneratingPatternType;
			if (newYearlyPattern != null)
			{
				return true;
			}

			ExchangeConnectorUtilities.DebugFail("Unknown recurrence pattern type");
			return true;
		} 

		#endregion // AreRecurrencePatternsEqual

		#region AreRecurrenceRangesEqual

		private static bool AreRecurrenceRangesEqual(RecurrenceRangeBaseType newRange, RecurrenceRangeBaseType oldRange)
		{
			if (newRange.GetType() != oldRange.GetType())
				return false;

			if (newRange.StartDate != oldRange.StartDate)
				return false;

			NoEndRecurrenceRangeType newNoEndRange = newRange as NoEndRecurrenceRangeType;
			if (newNoEndRange != null)
				return true;

			EndDateRecurrenceRangeType newEndDateRange = newRange as EndDateRecurrenceRangeType;
			if (newEndDateRange != null)
			{
				EndDateRecurrenceRangeType oldEndDateRange = oldRange as EndDateRecurrenceRangeType;
				if (newEndDateRange.EndDate != oldEndDateRange.EndDate)
					return false;

				return true;
			}

			NumberedRecurrenceRangeType newNumberedRange = newRange as NumberedRecurrenceRangeType;
			if (newNumberedRange != null)
			{
				NumberedRecurrenceRangeType oldNumberedRange = oldRange as NumberedRecurrenceRangeType;
				if (newNumberedRange.NumberOfOccurrences != oldNumberedRange.NumberOfOccurrences)
					return false;

				return true;
			}

			ExchangeConnectorUtilities.DebugFail("Unknown recurrence range type");
			return true;
		} 

		#endregion // AreRecurrenceRangesEqual

		#region AreRecurrencesEqual

		private static bool AreRecurrencesEqual(
			RecurrencePatternBaseType newPattern, RecurrencePatternBaseType oldPattern,
			RecurrenceRangeBaseType newRange, RecurrenceRangeBaseType oldRange)
		{
			if (EWSUtilities.AreRecurrenceRangesEqual(newRange, oldRange) == false)
				return false;

			return EWSUtilities.AreRecurrencePatternsEqual(newPattern, oldPattern);
		} 

		#endregion // AreRecurrencesEqual

		#region ConstructDaysOfWeek

		private static string ConstructDaysOfWeek(DateRecurrence dateRecurrence)
		{
			if (dateRecurrence.HasRules == false)
				return null;

			StringBuilder builder = new StringBuilder();
			foreach (DateRecurrenceRuleBase rule in dateRecurrence.Rules)
			{
				DayOfWeekRecurrenceRule dayOfWeekRule = rule as DayOfWeekRecurrenceRule;

				if (dayOfWeekRule == null)
					continue;

				if (builder.Length > 0)
					builder.Append(" ");

				builder.Append(dayOfWeekRule.Day);
			}

			if (builder.Length == 0)
				return null;

			return builder.ToString();
		} 

		#endregion  // ConstructDaysOfWeek

		#region ConvertToItemIdList

		public static IList<BaseItemIdType> ConvertToItemIdList(IList<ItemType> items)
		{
			BaseItemIdType[] itemIds = new BaseItemIdType[items.Count];

			for (int i = 0; i < items.Count; i++)
				itemIds[i] = items[i].ItemId;

			return itemIds;
		}

		#endregion //ConvertToItemIdList

		#region CreateConstant

		public static FieldURIOrConstantType CreateConstant(string constantValue)
		{
			ConstantValueType constant = new ConstantValueType();
			constant.Value = constantValue;

			FieldURIOrConstantType fieldURIOrConstantType = new FieldURIOrConstantType();
			fieldURIOrConstantType.Item = constant;
			return fieldURIOrConstantType;
		} 

		#endregion  // CreateConstant

		#region CreateDeleteItemField

		public static DeleteItemFieldType CreateDeleteItemField(UnindexedFieldURIType propertyType)
		{
			DeleteItemFieldType deleteField = new DeleteItemFieldType();
			deleteField.Item = EWSUtilities.CreateProperty(propertyType);
			return deleteField;
		}

		internal static ItemChangeDescriptionType CreateDeleteItemField(PathToExtendedFieldType pathToExtendedFieldType)
		{
			return EWSUtilities.CreateDeleteItemField(
				pathToExtendedFieldType.PropertySetId, 
				pathToExtendedFieldType.PropertyId, 
				pathToExtendedFieldType.PropertyType);
		}

		public static DeleteItemFieldType CreateDeleteItemField(string propertySetId, int propertyId, MapiPropertyTypeType propertyType)
		{
			DeleteItemFieldType deleteField = new DeleteItemFieldType();
			deleteField.Item = EWSUtilities.CreateExtendedProperty(propertySetId, propertyId, propertyType);
			return deleteField;
		}

		#endregion  // CreateDeleteItemField

		#region CreateDistinguishedFolderId

		public static DistinguishedFolderIdType CreateDistinguishedFolderId(DistinguishedFolderIdNameType folderIdName)
		{
			DistinguishedFolderIdType folderId = new DistinguishedFolderIdType();
			folderId.Id = folderIdName;
			return folderId;
		}

		#endregion  // CreateDistinguishedFolderId

		#region CreateExchangeFolder

		public static ExchangeFolder CreateExchangeFolder(ExchangeService service, BaseFolderType folder)
		{
			if (folder is SearchFolderType)
				return null;

			CalendarFolderType calendarFolder = folder as CalendarFolderType;

			if (calendarFolder != null)
				return new ExchangeCalendarFolder(service, calendarFolder);

			TasksFolderType tasksFolder = folder as TasksFolderType;

			if (tasksFolder != null)
				return new ExchangeTasksFolder(service, tasksFolder);

			FolderType journalFolder = folder as FolderType;

			if (journalFolder != null)
				return new ExchangeJournalFolder(service, journalFolder);

			ExchangeConnectorUtilities.DebugFail("Unknown folder type here: " + folder.GetType());
			return null;
		} 

		#endregion  // CreateExchangeFolder


		#region CreateExchangeServiceBindingForDefaultUser

		// MD 5/31/11 - TFS77450
		// If the server version is already auto detected, we need to know what it is.
		//public static ExchangeServiceBindingInternal CreateExchangeServiceBindingForDefaultUser(ExchangeServerConnectionSettings settings, string timeZoneId)
		public static ExchangeServiceBindingInternal CreateExchangeServiceBindingForDefaultUser(ExchangeServerConnectionSettings settings, string timeZoneId, ExchangeVersion? autoDetectedVersion)
		{
			ExchangeServiceBindingInternal binding = new ExchangeServiceBindingInternal();
			binding.UseDefaultCredentials = true;

			// MD 5/31/11 - TFS77450
			// If the server version is already auto detected, we need to know what it is.
			//EWSUtilities.InitializeServiceBinding(binding, settings, timeZoneId);
			EWSUtilities.InitializeServiceBinding(binding, settings, timeZoneId, autoDetectedVersion);

			return binding;
		}

		#endregion  // CreateExchangeServiceBindingForDefaultUser


		#region CreateExchangeServiceBindingForUser

		// MD 5/31/11 - TFS77450
		// If the server version is already auto detected, we need to know what it is.
		//public static ExchangeServiceBindingInternal CreateExchangeServiceBindingForUser(ExchangeUser user, ExchangeServerConnectionSettings settings, string timeZoneId)
		public static ExchangeServiceBindingInternal CreateExchangeServiceBindingForUser(ExchangeUser user, ExchangeServerConnectionSettings settings, string timeZoneId, ExchangeVersion? autoDetectedVersion)
		{
			ExchangeServiceBindingInternal binding = new ExchangeServiceBindingInternal();
			binding.Credentials = user.CreateNetworkCredentials();

			// MD 5/31/11 - TFS77450
			// If the server version is already auto detected, we need to know what it is.
			//EWSUtilities.InitializeServiceBinding(binding, settings, timeZoneId);
			EWSUtilities.InitializeServiceBinding(binding, settings, timeZoneId, autoDetectedVersion);

			return binding;
		}

		#endregion  // CreateExchangeServiceBindingForUser

		#region CreateExtendedProperty

		public static PathToExtendedFieldType CreateExtendedProperty(string propertySetId, int propertyId, MapiPropertyTypeType propertyType)
		{
			PathToExtendedFieldType mapiField = new PathToExtendedFieldType();
			mapiField.PropertySetId = propertySetId;
			mapiField.PropertyId = propertyId;
			mapiField.PropertyIdSpecified = true;
			mapiField.PropertyType = propertyType;
			return mapiField;
		} 

		#endregion  // CreateExtendedProperty

		#region CreateItemWithSingleProperty

		public static ItemType CreateItemWithSingleProperty(ItemType item, PropertyInfo property, UnindexedFieldURIType propertyType)
		{
			Type itemType = item.GetType();
			ItemType retItem = (ItemType)Activator.CreateInstance(itemType);

			object value = property.GetValue(item, null);
			property.SetValue(retItem, value, null);

			PropertyInfo isSetProperty = itemType.GetProperty(property.Name + "Specified");
			if (isSetProperty != null)
				isSetProperty.SetValue(retItem, true, null);

			return retItem;
		} 

		#endregion  // CreateItemWithSingleProperty

		#region CreateProperty

		public static PathToUnindexedFieldType CreateProperty(UnindexedFieldURIType field)
		{
			PathToUnindexedFieldType fieldUri = new PathToUnindexedFieldType();
			fieldUri.FieldURI = field;
			return fieldUri;
		}

		#endregion  // CreateProperty

		#region CreateSetItemField

		public static SetItemFieldType CreateSetItemField(ItemType item, PropertyInfo property, UnindexedFieldURIType propertyType)
		{
			SetItemFieldType setItemField = new SetItemFieldType();
			setItemField.Item = EWSUtilities.CreateProperty(propertyType);
			setItemField.Item1 = EWSUtilities.CreateItemWithSingleProperty(item, property, propertyType);
			return setItemField;
		}

		public static SetItemFieldType CreateSetItemField(ItemType item, ExtendedPropertyType extendedProperty)
		{
			Type itemType = item.GetType();
			ItemType itemWithSingleProperty = (ItemType)Activator.CreateInstance(itemType);
			itemWithSingleProperty.ExtendedProperty = new ExtendedPropertyType[] { extendedProperty };

			SetItemFieldType setItemField = new SetItemFieldType();
			setItemField.Item = extendedProperty.ExtendedFieldURI;
			setItemField.Item1 = itemWithSingleProperty;
			return setItemField;
		}

		public static SetItemFieldType CreateSetItemField(ItemType item, ItemType itemWithSingleProperty, string propertySetId, int propertyId, MapiPropertyTypeType propertyType)
		{
			SetItemFieldType setItemField = new SetItemFieldType();
			setItemField.Item = EWSUtilities.CreateExtendedProperty(propertySetId, propertyId, propertyType);
			setItemField.Item1 = itemWithSingleProperty;
			return setItemField;
		}

		#endregion  // CreateSetItemField

		// MD 5/26/11 - TFS76314
		#region ExchangeVersionTypeToExchangeVersion

		public static ExchangeVersionType ExchangeVersionToExchangeVersionType(ExchangeVersion value)
		{
			switch (value)
			{
				case ExchangeVersion.Exchange2007_SP1:
					return ExchangeVersionType.Exchange2007_SP1;

				case ExchangeVersion.Exchange2010:
					return ExchangeVersionType.Exchange2010;

				case ExchangeVersion.Exchange2010_SP1:
					return ExchangeVersionType.Exchange2010_SP1;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown value");
					return ExchangeVersionType.Exchange2007_SP1;
			}
		}

		#endregion  // ExchangeVersionTypeToExchangeVersion

		#region GetBinaryRecurrenceData

		public static byte[] GetBinaryRecurrenceData(ItemType item)
		{
			if (item == null || item.ExtendedProperty == null)
				return null;

			string expectedPropertySetId;
			int expectedPropertyId;
			if (item is CalendarItemType)
			{
				expectedPropertySetId = ExchangeCalendarFolder.PropertySetId;
				expectedPropertyId = ExchangeCalendarFolder.RecurPropertyId;
			}
			else if (item is TaskType)
			{
				expectedPropertySetId = ExchangeTasksFolder.PropertySetId;
				expectedPropertyId = ExchangeTasksFolder.RecurrencePropertyId;
			}
			else
			{
				ExchangeConnectorUtilities.DebugFail("Unknown recurring item type.");
				return null;
			}

			foreach (ExtendedPropertyType prop in item.ExtendedProperty)
			{
				if (prop.ExtendedFieldURI.PropertySetId == expectedPropertySetId &&
					prop.ExtendedFieldURI.PropertyId == expectedPropertyId)
				{
					return Convert.FromBase64String((string)prop.Item);
				}
			}

			return null;
		}

		#endregion  // GetBinaryRecurrenceData

		#region GetCalendarItemPropertyName

		public static string GetCalendarItemPropertyName(UnindexedFieldURIType propertyType)
		{
			string propertyName = Enum.GetName(typeof(UnindexedFieldURIType), propertyType);

			if (propertyName.StartsWith("item"))
				return propertyName.Substring(4);

			if (propertyName.StartsWith("calendar"))
			{
				propertyName = propertyName.Substring(8);

				switch (propertyName)
				{
					case "CalendarItemType":
						propertyName += "1";
						break;
				}

				return propertyName;
			}

			return null;
		}

		#endregion  // GetCalendarItemPropertyName

		#region GetDaysNotInGroup

		public static List<DayOfWeek> GetDaysNotInGroup(DayOfWeek[] daysOfWeek)
		{
			List<DayOfWeek> result = new List<DayOfWeek>();
			result.Add(DayOfWeek.Sunday);
			result.Add(DayOfWeek.Monday);
			result.Add(DayOfWeek.Tuesday);
			result.Add(DayOfWeek.Wednesday);
			result.Add(DayOfWeek.Thursday);
			result.Add(DayOfWeek.Friday);
			result.Add(DayOfWeek.Saturday);

			for (int i = 0; i < daysOfWeek.Length; i++)
				result.Remove(daysOfWeek[i]);

			return result;
		}

		#endregion  // GetDaysNotInGroup

		#region GetDaysOfWeek

		private static void GetDaysOfWeek(DateRecurrence dateRecurrence,
			out DayOfWeekType dayOfWeek,
			out DayOfWeekIndexType dayOfWeekIndex,
			out MonthNamesType monthName)
		{
			monthName = MonthNamesType.January;

			// MD 5/27/11 - TFS75962
			// These are no longer needed.
			//bool hasSubset2Rule = false;
			//bool hasSubset5Rule = false;

			int daysCount = 0;
			bool hasSunday = false;
			bool hasMonday = false;
			bool hasTuesday = false;
			bool hasWednesday = false;
			bool hasThursday = false;
			bool hasFriday = false;
			bool hasSaturday = false;

			int? relativeIndex = null;

			foreach (DateRecurrenceRuleBase rule in dateRecurrence.Rules)
			{
				DayOfWeekRecurrenceRule dayOfWeekRule = rule as DayOfWeekRecurrenceRule;
				if (dayOfWeekRule != null)
				{
					daysCount++;
					switch (dayOfWeekRule.Day)
					{
						case DayOfWeek.Friday:
							Debug.Assert(hasFriday == false, "We shouldn't have had two Fridays.");
							hasFriday = true;
							break;
						case DayOfWeek.Monday:
							Debug.Assert(hasMonday == false, "We shouldn't have had two Mondays.");
							hasMonday = true;
							break;
						case DayOfWeek.Saturday:
							Debug.Assert(hasSaturday == false, "We shouldn't have had two Saturdays.");
							hasSaturday = true;
							break;
						case DayOfWeek.Sunday:
							Debug.Assert(hasSunday == false, "We shouldn't have had two Sundays.");
							hasSunday = true;
							break;
						case DayOfWeek.Thursday:
							Debug.Assert(hasThursday == false, "We shouldn't have had two Thursdays.");
							hasThursday = true;
							break;
						case DayOfWeek.Tuesday:
							Debug.Assert(hasTuesday == false, "We shouldn't have had two Tuesdays.");
							hasTuesday = true;
							break;
						case DayOfWeek.Wednesday:
							Debug.Assert(hasWednesday == false, "We shouldn't have had two Wednesdays.");
							hasWednesday = true;
							break;
						default:
							ExchangeConnectorUtilities.DebugFail("Unknown DayOfWeek: " + dayOfWeekRule.Day);
							break;
					}

					Debug.Assert(relativeIndex.HasValue == false || relativeIndex.Value == dayOfWeekRule.RelativePosition, "The relative positions do not match.");
					relativeIndex = dayOfWeekRule.RelativePosition;

					continue;
				}

				MonthOfYearRecurrenceRule monthOfYearRule = rule as MonthOfYearRecurrenceRule;
				if (monthOfYearRule != null)
				{
					monthName = EWSUtilities.GetMonthName(monthOfYearRule.Month);
					continue;
				}

				SubsetRecurrenceRule subsetRule = rule as SubsetRecurrenceRule;
				if (subsetRule != null)
				{
					// MD 5/27/11 - TFS75962
					// The SubsetRecurrenceRule is now used differently. The OccurrenceInstance is the relative index now.
					//switch (subsetRule.OccurrenceInstance)
					//{
					//    case 2:
					//        hasSubset2Rule = true;
					//        break;
					//
					//    case 5:
					//        hasSubset5Rule = true;
					//        break;
					//
					//    default:
					//        ExchangeConnectorUtilities.DebugFail("Not sure what to do about this SubsetRecurrenceRule.");
					//        break;
					//}
					//continue;
					Debug.Assert(relativeIndex == 0, "When there is a subset rule, it was assumed the relative index would be 0.");
					relativeIndex = subsetRule.OccurrenceInstance;
				}

				ExchangeConnectorUtilities.DebugFail("Not sure what to do about this rule type: " + rule.GetType().Name);
			}

			dayOfWeek = DayOfWeekType.Sunday;
			switch (daysCount)
			{
				case 1:
					if (hasMonday)
						dayOfWeek = DayOfWeekType.Monday;
					else if (hasTuesday)
						dayOfWeek = DayOfWeekType.Tuesday;
					else if (hasWednesday)
						dayOfWeek = DayOfWeekType.Wednesday;
					else if (hasThursday)
						dayOfWeek = DayOfWeekType.Thursday;
					else if (hasFriday)
						dayOfWeek = DayOfWeekType.Friday;
					else if (hasSaturday)
						dayOfWeek = DayOfWeekType.Saturday;
					else if (hasSunday)
						dayOfWeek = DayOfWeekType.Sunday;
					else
						ExchangeConnectorUtilities.DebugFail("Unknown day of week.");
					break;

				case 2:
					Debug.Assert(hasSunday && hasSaturday, "We should only have weekend days when there are two days.");
					dayOfWeek = DayOfWeekType.WeekendDay;
					break;

				case 5:
					Debug.Assert(hasMonday && hasTuesday && hasWednesday && hasThursday && hasFriday, "We should only have weekend days when there are two days.");
					dayOfWeek = DayOfWeekType.Weekday;
					break;

				case 7:
					dayOfWeek = DayOfWeekType.Day;
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Incorrect number of days.");
					break;
			}

			dayOfWeekIndex = DayOfWeekIndexType.First;
			if (relativeIndex.HasValue)
			{
				switch (relativeIndex.Value)
				{
					case -1:
						// MD 5/27/11 - TFS75962
						// This check is no longer correct.
						//Debug.Assert((dayOfWeek == DayOfWeekType.Weekday) == hasSubset5Rule, "With a last weekday rule, there should be subset (5) rule.");
						//Debug.Assert((dayOfWeek == DayOfWeekType.WeekendDay) == hasSubset2Rule, "With a last weekend day rule, there should be subset (2) rule.");

						dayOfWeekIndex = DayOfWeekIndexType.Last;
						break;

					case 1:
						// MD 5/27/11 - TFS75962
						// This check is no longer correct.
						//Debug.Assert(hasSubset2Rule == false && hasSubset5Rule == false, "There should be no subset rules here.");

						dayOfWeekIndex = DayOfWeekIndexType.First;
						break;

					case 2:
						// MD 5/27/11 - TFS75962
						// This check is no longer correct.
						//Debug.Assert(hasSubset2Rule == false && hasSubset5Rule == false, "There should be no subset rules here.");

						dayOfWeekIndex = DayOfWeekIndexType.Second;
						break;

					case 3:
						// MD 5/27/11 - TFS75962
						// This check is no longer correct.
						//Debug.Assert(hasSubset2Rule == false && hasSubset5Rule == false, "There should be no subset rules here.");

						dayOfWeekIndex = DayOfWeekIndexType.Third;
						break;

					case 4:
						// MD 5/27/11 - TFS75962
						// This check is no longer correct.
						//Debug.Assert(hasSubset2Rule == false && hasSubset5Rule == false, "There should be no subset rules here.");

						dayOfWeekIndex = DayOfWeekIndexType.Fourth;
						break;

					default:
						ExchangeConnectorUtilities.DebugFail("Unknown relative index: " + relativeIndex.Value);
						break;
				}
			}
		}

		#endregion  // GetDaysOfWeek

		#region GetEndTime

		public static DateTime? GetEndTime(ItemType item)
		{
			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem != null)
			{
				if (calendarItem.EndSpecified)
					return calendarItem.End;

				return null;
			}

			TaskType taskItem = item as TaskType;
			if (taskItem != null)
			{
				if (taskItem.DueDateSpecified)
					return taskItem.DueDate;

				return null;
			}

			MessageType messageItem = item as MessageType;
			if (messageItem != null && messageItem.ItemClass == ExchangeJournalFolder.ItemClass)
			{
				foreach (ExtendedPropertyType extendedProperty in messageItem.ExtendedProperty)
				{
					if (extendedProperty.ExtendedFieldURI.PropertySetId != ExchangeJournalFolder.PropertySetId)
						continue;

					if (extendedProperty.ExtendedFieldURI.PropertyId != ExchangeJournalFolder.EndPropertyId)
						continue;

					string endTime = (string)extendedProperty.Item;
					DateTime endTimeValue;
					if (EWSUtilities.TryParseExchangeDateTime(endTime, out endTimeValue) == false)
					{
						ExchangeConnectorUtilities.DebugFail("Couldn't parse end time: " + endTime);
						break;
					}

					return endTimeValue;
				}

				return null;
			}

			ExchangeConnectorUtilities.DebugFail("Cannot determine how to get the end of the item.");
			return null;
		}

		#endregion  // GetEndTime

		#region GetExchangeDateTime

		public static string GetExchangeDateTime(DateTime dateTime)
		{
			return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
		}

		#endregion  // GetExchangeDateTime

		#region GetExtendedPropertyValue

		internal static object GetExtendedPropertyValue(ItemType item, PathToExtendedFieldType pathToExtendedFieldType)
		{
			if (item.ExtendedProperty == null)
				return null;

			foreach (ExtendedPropertyType extendedProperty in item.ExtendedProperty)
			{
				if (pathToExtendedFieldType.DistinguishedPropertySetIdSpecified != extendedProperty.ExtendedFieldURI.DistinguishedPropertySetIdSpecified)
					continue;

				if (pathToExtendedFieldType.DistinguishedPropertySetId != extendedProperty.ExtendedFieldURI.DistinguishedPropertySetId)
					continue;

				if (pathToExtendedFieldType.PropertyIdSpecified != extendedProperty.ExtendedFieldURI.PropertyIdSpecified)
					continue;

				if (pathToExtendedFieldType.PropertyId != extendedProperty.ExtendedFieldURI.PropertyId)
					continue;

				if (pathToExtendedFieldType.PropertySetId != extendedProperty.ExtendedFieldURI.PropertySetId)
					continue;

				if (pathToExtendedFieldType.PropertyType != extendedProperty.ExtendedFieldURI.PropertyType)
					continue;

				return extendedProperty.Item;
			}

			return null;
		}

		#endregion  // GetExtendedPropertyValue

		#region GetMeetingRequestWasSent

		public static bool GetMeetingRequestWasSent(ItemType item)
		{
			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem != null)
			{
				if (calendarItem.MeetingRequestWasSentSpecified)
					return calendarItem.MeetingRequestWasSent;

				return false;
			}

			return false;
		} 

		#endregion  // GetMeetingRequestWasSent

		#region GetMonthName

		private static MonthNamesType GetMonthName(int month)
		{
			switch (month)
			{
				case 1: return MonthNamesType.January;
				case 2: return MonthNamesType.February;
				case 3: return MonthNamesType.March;
				case 4: return MonthNamesType.April;
				case 5: return MonthNamesType.May;
				case 6: return MonthNamesType.June;
				case 7: return MonthNamesType.July;
				case 8: return MonthNamesType.August;
				case 9: return MonthNamesType.September;
				case 10: return MonthNamesType.October;
				case 11: return MonthNamesType.November;
				case 12: return MonthNamesType.December;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown month index: " + month);
					return MonthNamesType.January;
			}
		}

		#endregion  // GetMonthName

		#region GetStartTime

		public static DateTime? GetStartTime(ItemType item)
		{
			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem != null)
			{
				if (calendarItem.StartSpecified)
					return calendarItem.Start;

				return null;
			}

			TaskType taskItem = item as TaskType;
			if (taskItem != null)
			{
				if (taskItem.StartDateSpecified)
					return taskItem.StartDate;

				return null;
			}

			MessageType messageItem = item as MessageType;
			if (messageItem != null && messageItem.ItemClass == ExchangeJournalFolder.ItemClass)
			{
				foreach (ExtendedPropertyType extendedProperty in messageItem.ExtendedProperty)
				{
					if (extendedProperty.ExtendedFieldURI.PropertySetId != ExchangeJournalFolder.PropertySetId)
						continue;

					if (extendedProperty.ExtendedFieldURI.PropertyId != ExchangeJournalFolder.StartPropertyId)
						continue;

					string startTime = (string)extendedProperty.Item;
					DateTime startTimeValue;
					if (EWSUtilities.TryParseExchangeDateTime(startTime, out startTimeValue) == false)
					{
						ExchangeConnectorUtilities.DebugFail("Couldn't parse start time: " + startTime);
						break;
					}

					return startTimeValue;
				}

				return null;
			}

			ExchangeConnectorUtilities.DebugFail("Cannot determine how to get the start of the item.");
			return null;
		} 

		#endregion  // GetStartTime

		#region GetTimeZone

		public static void GetTimeZones(ItemType item, TimeZoneInfoProvider provider, out TimeZoneToken startToken, out TimeZoneToken endToken)
		{
			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem == null)
			{
				startToken = provider.LocalToken;
				endToken = provider.LocalToken;
				return;
			}

			startToken = null;
			endToken = null;

			if (calendarItem.StartTimeZone != null)
			{
				if (provider.TryGetTimeZoneToken(calendarItem.StartTimeZone.Id, out startToken) == false)
					provider.TryGetTimeZoneTokenByDisplayName(calendarItem.StartTimeZone.Id, out startToken);
			}

			if (calendarItem.EndTimeZone != null)
			{
				if (provider.TryGetTimeZoneToken(calendarItem.EndTimeZone.Id, out endToken) == false)
					provider.TryGetTimeZoneTokenByDisplayName(calendarItem.EndTimeZone.Id, out endToken);
			}

			if (startToken != null && endToken != null)
				return;

			string timeZoneString;
			if (calendarItem.MeetingTimeZone != null)
				timeZoneString = calendarItem.MeetingTimeZone.TimeZoneName;
			else
				timeZoneString = calendarItem.TimeZone;

			TimeZoneToken token = null;
			if (String.IsNullOrEmpty(timeZoneString) == false)
			{
				if (provider.TryGetTimeZoneToken(timeZoneString, out token) == false)
					provider.TryGetTimeZoneTokenByDisplayName(timeZoneString, out token);
			}

			if (token == null)
				token = provider.LocalToken;

			if (startToken == null)
				startToken = token;

			if (endToken == null)
				endToken = token;
		}

		#endregion  // GetTimeZone

		#region GetTimeZoneTime

		public static SerializableTimeZoneTime GetTimeZoneTime(TimeZoneInfoProvider.TransitionTime transitionTime, TimeSpan delta)
		{
			SerializableTimeZoneTime timeZoneTime = new SerializableTimeZoneTime();

			if (transitionTime.IsFixedDate)
			{
				timeZoneTime.Year = DateTime.Today.Year.ToString();
				timeZoneTime.Month = (short)transitionTime.Month;
				timeZoneTime.DayOrder = (short)transitionTime.Day;
				timeZoneTime.Time = EWSUtilities.TimeSpanToExchangeTime(transitionTime.TimeOfDay.TimeOfDay);

			}
			else
			{
				timeZoneTime.Month = (short)transitionTime.Month;
				timeZoneTime.DayOfWeek = transitionTime.DayOfWeek.ToString();
				timeZoneTime.DayOrder = (short)transitionTime.Week;
				timeZoneTime.Time = EWSUtilities.TimeSpanToExchangeTime(transitionTime.TimeOfDay.TimeOfDay);
			}

			return timeZoneTime;
		} 

		#endregion  // GetTimeZoneTime

		#region GetTimeZoneToken

		private static TimeZoneToken GetTimeZoneToken(string primaryTimeZoneName, string secondayTimeZoneName, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			TimeZoneToken activityStartLocalToken;
			if (string.IsNullOrEmpty(primaryTimeZoneName) || timeZoneInfoProvider.TryGetTimeZoneToken(primaryTimeZoneName, out activityStartLocalToken) == false)
			{
				if (string.IsNullOrEmpty(secondayTimeZoneName) || timeZoneInfoProvider.TryGetTimeZoneToken(secondayTimeZoneName, out activityStartLocalToken) == false)
					activityStartLocalToken = timeZoneInfoProvider.LocalToken;
			}

			return activityStartLocalToken;
		}

		#endregion //GetTimeZoneToken

		#region InitializeServiceBinding

		// MD 5/31/11 - TFS77450
		// If the server version is already auto detected, we need to know what it is.
		//public static void InitializeServiceBinding(ExchangeServiceBindingInternal binding, ExchangeServerConnectionSettings settings, string timeZoneId)
		public static void InitializeServiceBinding(ExchangeServiceBindingInternal binding, ExchangeServerConnectionSettings settings, string timeZoneId, ExchangeVersion? autoDetectedVersion)
		{

			binding.AcceptGzipEncoding = settings.AcceptGZipEncoding;
			binding.AllowAutoRedirect = true;
			binding.EnableDecompression = true;
			binding.PreAuthenticate = settings.PreAuthenticate;
			binding.Proxy = settings.WebProxy;
			binding.UserAgent = settings.UserAgent ?? "ExchangeServicesClient/14.00.0650.007";


			binding.CookieContainer = settings.CookieContainer;		
			binding.HttpHeaders = settings.HttpHeaders;		
			binding.RequestServerVersionValue = new RequestServerVersion();

			// MD 4/28/11 - TFS72779
			// This value could be AutoDetect now.
			//binding.RequestServerVersionValue.Version = (ExchangeVersionType)settings.RequestedServerVersion;
			ExchangeVersion version = settings.RequestedServerVersion;
			if (version == ExchangeVersion.AutoDetect)
			{
				// MD 5/31/11 - TFS77450
				// Use the passed in auto detected server version if it is valid.
				//version = ExchangeVersion.Exchange2007_SP1;
				version = autoDetectedVersion ?? ExchangeVersion.Exchange2007_SP1;
			}

			// MD 5/26/11 - TFS76314
			// These enums are no longer mapped to the same values. We need a method to convert between them.
			//binding.RequestServerVersionValue.Version = (ExchangeVersionType)version;
			binding.RequestServerVersionValue.Version = EWSUtilities.ExchangeVersionToExchangeVersionType(version);

			binding.Timeout = settings.Timeout;
			binding.Url = settings.Url.ToString();

			binding.TimeZoneContext = new TimeZoneContextType();
			binding.TimeZoneContext.TimeZoneDefinition = new TimeZoneDefinitionType();
			binding.TimeZoneContext.TimeZoneDefinition.Id = timeZoneId;
		}

		#endregion  // InitializeServiceBinding

		#region ItemFromActivity

		public static ItemType ItemFromActivity(ActivityBase activity, ExchangeScheduleDataConnector connector)
		{
			ItemType item;
			if (activity is Appointment)
			{
				item = new CalendarItemType();
			}
			else if (activity is Journal)
			{
				item = new MessageType();
				item.ItemClass = ExchangeJournalFolder.ItemClass;
			}
			else if (activity is Task)
			{
				item = new TaskType();
			}
			else
			{
				ExchangeConnectorUtilities.DebugFail("Not sure which item type to create.");
				return null;
			}

			EWSUtilities.UpdateItem(item, activity, connector);
			return item;
		}

		#endregion  // ItemFromActivity

		#region ListContainsId

		public static bool ListContainsId(List<BaseItemIdType> list, BaseItemIdType id)
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (EWSUtilities.AreIdsEqual(list[i], id))
					return true;
			}

			return false;
		} 

		#endregion  // ListContainsId

		#region OrCondition

		public static OrType OrCondition(params SearchExpressionType[] conditions)
		{
			OrType orCondition = new OrType();
			orCondition.Items = conditions;
			return orCondition;
		}

		#endregion  // OrCondition

		#region ParseDayOfWeekIndex

		private static int ParseDayOfWeekIndex(DayOfWeekIndexType dayOfWeekIndex)
		{
			switch (dayOfWeekIndex)
			{
				case DayOfWeekIndexType.First:	return 1;
				case DayOfWeekIndexType.Second: return 2;
				case DayOfWeekIndexType.Third:	return 3;
				case DayOfWeekIndexType.Fourth: return 4;
				case DayOfWeekIndexType.Last:	return -1;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown DayOfWeekIndexType: " + dayOfWeekIndex);
					return 1;
			}
		} 

		#endregion  // ParseDayOfWeekIndex

		#region ParseDaysOfWeek

		private static void ParseDaysOfWeek(DateRecurrence dateRecurrence, string daysOfWeekValue, int relativePosition = 0)
		{
			foreach (DayOfWeek dayOfWeek in EWSUtilities.ParseDaysOfWeek(daysOfWeekValue))
				dateRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(dayOfWeek, relativePosition));
		}

		private static void ParseDaysOfWeek(DateRecurrence dateRecurrence, DayOfWeekType exchangeDayOfWeek, int relativePosition = 0)
		{
			foreach (DayOfWeek dayOfWeek in EWSUtilities.ParseDaysOfWeek(exchangeDayOfWeek))
				dateRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(dayOfWeek, relativePosition));
		}

		public static DayOfWeek[] ParseDaysOfWeek(string daysOfWeekValue)
		{
			List<DayOfWeek> daysOfWeekList = new List<DayOfWeek>();

			string[] daysOfWeek = daysOfWeekValue.Split(' ');

			for (int i = 0; i < daysOfWeek.Length; i++)
			{
				string dayOfWeek = daysOfWeek[i].Trim();

				if (string.IsNullOrEmpty(dayOfWeek))
					continue;

				daysOfWeekList.AddRange(EWSUtilities.ParseDaysOfWeekHelper(dayOfWeek));
			}

			return daysOfWeekList.ToArray();
		}

		private static DayOfWeek[] ParseDaysOfWeek(DayOfWeekType exchangeDayOfWeek)
		{
			switch (exchangeDayOfWeek)
			{
				case DayOfWeekType.Sunday:		return new DayOfWeek[] { DayOfWeek.Sunday };
				case DayOfWeekType.Monday:		return new DayOfWeek[] { DayOfWeek.Monday };
				case DayOfWeekType.Tuesday:		return new DayOfWeek[] { DayOfWeek.Tuesday };
				case DayOfWeekType.Wednesday:	return new DayOfWeek[] { DayOfWeek.Wednesday };
				case DayOfWeekType.Thursday:	return new DayOfWeek[] { DayOfWeek.Thursday };
				case DayOfWeekType.Friday:		return new DayOfWeek[] { DayOfWeek.Friday };
				case DayOfWeekType.Saturday:	return new DayOfWeek[] { DayOfWeek.Saturday };

				case DayOfWeekType.Day:			
					return new DayOfWeek[] { 
						DayOfWeek.Sunday, 
						DayOfWeek.Monday, 
						DayOfWeek.Tuesday, 
						DayOfWeek.Wednesday, 
						DayOfWeek.Thursday, 
						DayOfWeek.Friday, 
						DayOfWeek.Saturday };

				case DayOfWeekType.Weekday:
					return new DayOfWeek[] { 
						DayOfWeek.Monday, 
						DayOfWeek.Tuesday, 
						DayOfWeek.Wednesday, 
						DayOfWeek.Thursday, 
						DayOfWeek.Friday };

				case DayOfWeekType.WeekendDay:
					return new DayOfWeek[] { 
						DayOfWeek.Sunday, 
						DayOfWeek.Saturday };

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown DayOfWeekType: " + exchangeDayOfWeek);
					goto case DayOfWeekType.Sunday;
			}
		}

		#endregion  // ParseDaysOfWeek

		#region ParseDaysOfWeekHelper

		private static DayOfWeek[] ParseDaysOfWeekHelper(string dayOfWeekValue)
		{
			if (dayOfWeekValue == "Weekday")
			{
				return new DayOfWeek[] { 
					DayOfWeek.Monday, 
					DayOfWeek.Tuesday, 
					DayOfWeek.Wednesday, 
					DayOfWeek.Thursday, 
					DayOfWeek.Friday };
			}

			if (dayOfWeekValue == "Day")
			{
				return new DayOfWeek[] { 
					DayOfWeek.Sunday, 
					DayOfWeek.Monday, 
					DayOfWeek.Tuesday, 
					DayOfWeek.Wednesday, 
					DayOfWeek.Thursday, 
					DayOfWeek.Friday, 
					DayOfWeek.Saturday };
			}

			if (dayOfWeekValue == "WeekendDay")
			{
				return new DayOfWeek[] { 
					DayOfWeek.Sunday, 
					DayOfWeek.Saturday };
			}

			DayOfWeek dayOfWeek;
			if (Enum.TryParse<DayOfWeek>(dayOfWeekValue, out dayOfWeek) == false)
			{
				ExchangeConnectorUtilities.DebugFail("Cannot parse the day of week: " + dayOfWeekValue);
				return new DayOfWeek[] { DayOfWeek.Sunday };
			}

			return new DayOfWeek[] { dayOfWeek };
		} 

		#endregion  // ParseDaysOfWeekHelper

		#region ParseExchangeRecurrence

		public static DateRecurrence ParseExchangeRecurrence(RecurrencePatternBaseType pattern, RecurrenceRangeBaseType range)
		{
			DateRecurrence dateRecurrence = new DateRecurrence();

			EWSUtilities.ParseRecurrencePattern(pattern, dateRecurrence);
			EWSUtilities.ParseRecurrenceRange(range, dateRecurrence);

			return dateRecurrence;
		}

		#endregion  // ParseExchangeRecurrence

		#region ParseMonthName

		private static int ParseMonthName(MonthNamesType monthName)
		{
			switch (monthName)
			{
				case MonthNamesType.January:	return 1;
				case MonthNamesType.February:	return 2;
				case MonthNamesType.March:		return 3;
				case MonthNamesType.April:		return 4;
				case MonthNamesType.May:		return 5;
				case MonthNamesType.June:		return 6;
				case MonthNamesType.July:		return 7;
				case MonthNamesType.August:		return 8;
				case MonthNamesType.September:	return 9;
				case MonthNamesType.October:	return 10;
				case MonthNamesType.November:	return 11;
				case MonthNamesType.December:	return 12;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown MonthNamesType: " + monthName);
					return 1;
			}
		} 

		#endregion  // ParseMonthName

		#region ParseRecurrencePattern

		private static void ParseRecurrencePattern(RecurrencePatternBaseType pattern, DateRecurrence dateRecurrence)
		{
			IntervalRecurrencePatternBaseType intervalPattern = pattern as IntervalRecurrencePatternBaseType;

			if (intervalPattern != null)
			{
				dateRecurrence.Interval = intervalPattern.Interval;

				DailyRecurrencePatternType dailyPattern = intervalPattern as DailyRecurrencePatternType;
				if (dailyPattern != null)
				{
					dateRecurrence.Frequency = DateRecurrenceFrequency.Daily;
					return;
				}

				WeeklyRecurrencePatternType weeklyPattern = intervalPattern as WeeklyRecurrencePatternType;
				if (weeklyPattern != null)
				{
					dateRecurrence.Frequency = DateRecurrenceFrequency.Weekly;
					EWSUtilities.ParseDaysOfWeek(dateRecurrence, weeklyPattern.DaysOfWeek);
					return;
				}

				AbsoluteMonthlyRecurrencePatternType absoluteMonthlyPattern = intervalPattern as AbsoluteMonthlyRecurrencePatternType;
				if (absoluteMonthlyPattern != null)
				{
					dateRecurrence.Frequency = DateRecurrenceFrequency.Monthly;
					dateRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(absoluteMonthlyPattern.DayOfMonth));
					return;
				}

				RelativeMonthlyRecurrencePatternType relativeMonthlyPattern = intervalPattern as RelativeMonthlyRecurrencePatternType;
				if (relativeMonthlyPattern != null)
				{
					// MD 5/27/11 - TFS75962
					// The SubsetRecurrenceRule are now used differently. When the days or Day, Weekday, or Weekend Day, the relative index on the
					// day of week rules should be 0 and the SubsetRecurrenceRule should have the actual relative index value.
					int relativePosition = EWSUtilities.ParseDayOfWeekIndex(relativeMonthlyPattern.DayOfWeekIndex);
					int relativePositionForDays;
					switch (relativeMonthlyPattern.DaysOfWeek)
					{
						case DayOfWeekType.Day:
						case DayOfWeekType.Weekday:
						case DayOfWeekType.WeekendDay:
							relativePositionForDays = 0;
							dateRecurrence.Rules.Add(new SubsetRecurrenceRule(relativePosition));
							break;

						default:
							relativePositionForDays = relativePosition;
							break;
					}

					dateRecurrence.Frequency = DateRecurrenceFrequency.Monthly;
					EWSUtilities.ParseDaysOfWeek(
						dateRecurrence,
						relativeMonthlyPattern.DaysOfWeek,
						// MD 5/27/11 - TFS75962
						// When there is a SubsetRecurrenceRule, the day of week rules should have 0 as the relative index instead of the actual value.
						//EWSUtilities.ParseDayOfWeekIndex(relativeMonthlyPattern.DayOfWeekIndex));
						relativePositionForDays);

					// MD 5/27/11 - TFS75962
					// The SubsetRecurrenceRule are now used differently. See comment above.
					//if (relativeMonthlyPattern.DayOfWeekIndex == DayOfWeekIndexType.Last)
					//{
					//    switch (relativeMonthlyPattern.DaysOfWeek)
					//    {
					//        case DayOfWeekType.Weekday:
					//            dateRecurrence.Rules.Add(new SubsetRecurrenceRule(5));
					//            break;
					//
					//        case DayOfWeekType.WeekendDay:
					//            dateRecurrence.Rules.Add(new SubsetRecurrenceRule(2));
					//            break;
					//    }
					//}
					return;
				}

				ExchangeConnectorUtilities.DebugFail("Unknown interval recurrence pattern type: " + pattern.GetType().Name);
			}

			AbsoluteYearlyRecurrencePatternType absoluteYearlyPattern = pattern as AbsoluteYearlyRecurrencePatternType;
			if (absoluteYearlyPattern != null)
			{
				dateRecurrence.Frequency = DateRecurrenceFrequency.Yearly;
				int month = EWSUtilities.ParseMonthName(absoluteYearlyPattern.Month);
				dateRecurrence.Rules.Add(new MonthOfYearRecurrenceRule(month));
				dateRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(absoluteYearlyPattern.DayOfMonth));
				return;
			}

			RelativeYearlyRecurrencePatternType relativeYearlyPattern = pattern as RelativeYearlyRecurrencePatternType;
			if (relativeYearlyPattern != null)
			{
				// MD 5/27/11 - TFS75962
				// The SubsetRecurrenceRule are now used differently. When the days or Day, Weekday, or Weekend Day, the relative index on the
				// day of week rules should be 0 and the SubsetRecurrenceRule should have the actual relative index value.
				int relativePosition = EWSUtilities.ParseDayOfWeekIndex(relativeYearlyPattern.DayOfWeekIndex);
				int relativePositionForDays;
				switch (relativeYearlyPattern.DaysOfWeek)
				{
					case "Day":
					case "Weekday":
					case "WeekendDay":
						relativePositionForDays = 0;
						dateRecurrence.Rules.Add(new SubsetRecurrenceRule(relativePosition));
						break;

					default:
						relativePositionForDays = relativePosition;
						break;
				}

				dateRecurrence.Frequency = DateRecurrenceFrequency.Yearly;

				EWSUtilities.ParseDaysOfWeek(
					dateRecurrence,
					relativeYearlyPattern.DaysOfWeek,
					// MD 5/27/11 - TFS75962
					// When there is a SubsetRecurrenceRule, the day of week rules should have 0 as the relative index instead of the actual value.
					//EWSUtilities.ParseDayOfWeekIndex(relativeYearlyPattern.DayOfWeekIndex));
					relativePositionForDays);

				// MD 5/27/11 - TFS75962
				// The SubsetRecurrenceRule are now used differently. See comment above.
				//if (relativeYearlyPattern.DayOfWeekIndex == DayOfWeekIndexType.Last)
				//{
				//    switch (relativeYearlyPattern.DaysOfWeek)
				//    {
				//        case "Weekday":
				//            dateRecurrence.Rules.Add(new SubsetRecurrenceRule(5));
				//            break;
				//
				//        case "WeekendDay":
				//            dateRecurrence.Rules.Add(new SubsetRecurrenceRule(2));
				//            break;
				//    }
				//}

				int month = EWSUtilities.ParseMonthName(relativeYearlyPattern.Month);
				dateRecurrence.Rules.Add(new MonthOfYearRecurrenceRule(month));

				return;
			}

			ExchangeConnectorUtilities.DebugFail("Unknown recurrence pattern type: " + pattern.GetType().Name);
		}

		#endregion  // ParseRecurrencePattern

		#region ParseRecurrenceRange

		private static void ParseRecurrenceRange(RecurrenceRangeBaseType range, DateRecurrence dateRecurrence)
		{
			EndDateRecurrenceRangeType endDateRange = range as EndDateRecurrenceRangeType;
			if (endDateRange != null)
			{
				dateRecurrence.Until = endDateRange.EndDate;
			}

			NumberedRecurrenceRangeType numberedRange = range as NumberedRecurrenceRangeType;
			if (numberedRange != null)
			{
				dateRecurrence.Count = numberedRange.NumberOfOccurrences;
			}
		} 

		#endregion  // ParseRecurrenceRange

		// MD 4/27/11 - TFS72779
		#region ResolveAutoDetectedVersion

		public static void ResolveAutoDetectedVersion(ExchangeService service, XmlReader soapResponseReader)
		{
			if (soapResponseReader.ReadToDescendant("ServerVersionInfo", EWSUtilities.ExchangeTypesNamespace) == false)
				return;

			string version = soapResponseReader.GetAttribute("Version");

			if (String.IsNullOrEmpty(version))
				return;

			ExchangeVersion versionValue;
			if (Enum.TryParse<ExchangeVersion>(version, out versionValue) == false)
				return;

			service.Connector.SetAutoDetectedVersion(versionValue);
		}

		#endregion //ResolveAutoDetectedVersion

		#region TimeSpanToExchangeTime

		public static string TimeSpanToExchangeTime(TimeSpan timeSpan)
		{
			return string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
		}

		#endregion  // TimeSpanToExchangeTime

		#region TryGetExchangeRecurrence

		public static bool TryGetExchangeRecurrence(ActivityBase activity, DateRecurrence dateRecurrence, TimeZoneInfoProvider timeZoneInfoProvider,
			out RecurrencePatternBaseType pattern,
			out RecurrenceRangeBaseType range)
		{
			// MD 2/2/11 - TFS64582
			// We need to store the days of week string when we get it so we don't get it twice.
			string daysOfWeek = null;

			switch (dateRecurrence.Frequency)
			{
				case DateRecurrenceFrequency.Daily:
					// MD 2/2/11 - TFS64582
					// If the weekday option was used, the frequency will be daily, but we should really send a 
					// WeeklyRecurrencePatternType to the Exchange server.
					//pattern = new DailyRecurrencePatternType();
					daysOfWeek = EWSUtilities.ConstructDaysOfWeek(dateRecurrence);

					// If there are days of week, this is a weekly recurrence as far as the server is concerned.
					if (daysOfWeek != null)
						goto case DateRecurrenceFrequency.Weekly;

					pattern = new DailyRecurrencePatternType();
					break;

				case DateRecurrenceFrequency.Weekly:
					WeeklyRecurrencePatternType weeklyPattern = new WeeklyRecurrencePatternType();

					// MD 2/2/11 - TFS64582
					// Only get the days of week value if we don't already have it.
					//weeklyPattern.DaysOfWeek = EWSUtilities.ConstructDaysOfWeek(dateRecurrence);
					if (daysOfWeek == null)
						daysOfWeek = EWSUtilities.ConstructDaysOfWeek(dateRecurrence);

					weeklyPattern.DaysOfWeek = daysOfWeek;

					pattern = weeklyPattern;
					break;

				case DateRecurrenceFrequency.Monthly:
					{
						DayOfMonthRecurrenceRule dayOfMonthRule = null;
						if (dateRecurrence.Rules.Count == 1)
							dayOfMonthRule = dateRecurrence.Rules[0] as DayOfMonthRecurrenceRule;

						if (dayOfMonthRule != null)
						{
							AbsoluteMonthlyRecurrencePatternType absoluteMonthlyPattern = new AbsoluteMonthlyRecurrencePatternType();
							absoluteMonthlyPattern.DayOfMonth = dayOfMonthRule.DayOfMonth;
							pattern = absoluteMonthlyPattern;
						}
						else
						{
							DayOfWeekType dayOfWeek;
							DayOfWeekIndexType dayOfWeekIndex;
							MonthNamesType notUsed;
							EWSUtilities.GetDaysOfWeek(dateRecurrence, out dayOfWeek, out dayOfWeekIndex, out notUsed);

							RelativeMonthlyRecurrencePatternType relativeMonthlyPattern = new RelativeMonthlyRecurrencePatternType();
							relativeMonthlyPattern.DaysOfWeek = dayOfWeek;
							relativeMonthlyPattern.DayOfWeekIndex = dayOfWeekIndex;
							pattern = relativeMonthlyPattern;
						}
					}
					break;

				case DateRecurrenceFrequency.Yearly:
					{
						DayOfMonthRecurrenceRule dayOfMonthRule = null;
						MonthOfYearRecurrenceRule monthofYearRule = null;
						if (dateRecurrence.Rules.Count == 2)
						{
							foreach (DateRecurrenceRuleBase rule in dateRecurrence.Rules)
							{
								if (dayOfMonthRule == null)
									dayOfMonthRule = rule as DayOfMonthRecurrenceRule;

								if (monthofYearRule == null)
									monthofYearRule = rule as MonthOfYearRecurrenceRule;
							}
						}

						if (dayOfMonthRule != null && monthofYearRule != null)
						{
							AbsoluteYearlyRecurrencePatternType absoluteYearlyPattern = new AbsoluteYearlyRecurrencePatternType();
							absoluteYearlyPattern.DayOfMonth = dayOfMonthRule.DayOfMonth;
							absoluteYearlyPattern.Month = EWSUtilities.GetMonthName(monthofYearRule.Month);
							pattern = absoluteYearlyPattern;
						}
						else
						{
							DayOfWeekType dayOfWeek;
							DayOfWeekIndexType dayOfWeekIndex;
							MonthNamesType monthName;
							EWSUtilities.GetDaysOfWeek(dateRecurrence, out dayOfWeek, out dayOfWeekIndex, out monthName);

							RelativeYearlyRecurrencePatternType relativeYearlyPattern = new RelativeYearlyRecurrencePatternType();
							relativeYearlyPattern.DaysOfWeek = dayOfWeek.ToString();
							relativeYearlyPattern.DayOfWeekIndex = dayOfWeekIndex;
							relativeYearlyPattern.Month = monthName;
							pattern = relativeYearlyPattern;
						}
					}
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Don't know how to get recurrence frequency: " + dateRecurrence.Frequency);
					pattern = null;
					range = null;
					return false;
			}

			IntervalRecurrencePatternBaseType intervalPattern = pattern as IntervalRecurrencePatternBaseType;
			if (intervalPattern != null)
				intervalPattern.Interval = dateRecurrence.Interval;

			if (dateRecurrence.Until.HasValue)
			{
				EndDateRecurrenceRangeType endDateRange = new EndDateRecurrenceRangeType();
				endDateRange.EndDate = dateRecurrence.Until.Value;
				range = endDateRange;
			}
			else if (dateRecurrence.Count != 0)
			{
				NumberedRecurrenceRangeType numberedRange = new NumberedRecurrenceRangeType();
				numberedRange.NumberOfOccurrences = dateRecurrence.Count;
				range = numberedRange;
			}
			else
			{
				range = new NoEndRecurrenceRangeType();
			}

			bool setStartDate = false;

			// For recurring tasks, each occurrence is actually represented by root activity, so we can't assume the start of the 
			// root is the start of the recurrence range. Get the start from the existing Exchange data item if possible.
			if (activity is Task)
			{
				TaskType taskItem = activity.DataItem as TaskType;
				if (taskItem != null && taskItem.Recurrence != null && taskItem.Recurrence.Item1 != null)
				{
					range.StartDate = taskItem.Recurrence.Item1.StartDate;
					setStartDate = true;
				}
			}

			if (setStartDate == false)
			{
				range.StartDate = DateTime.SpecifyKind(
					timeZoneInfoProvider.ConvertUtcToLocal(ExchangeConnectorUtilities.GetActualStartTimeUtc(activity, timeZoneInfoProvider)),
					DateTimeKind.Local);
			}

			return true;
		}

		#endregion  // TryGetExchangeRecurrence

		#region TryParseExchangeDateTime

		private static bool TryParseExchangeDateTime(string endTime, out DateTime endTimeValue)
		{
			return DateTime.TryParse(endTime, CultureInfo.CurrentCulture, DateTimeStyles.AdjustToUniversal, out endTimeValue);
		}

		#endregion  // TryParseExchangeDateTime

		#region UpdateActivity

		public static void UpdateActivity(ActivityBase activity, ItemType item, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			try
			{
				// Preprocess all extended properties so we can always look at the PropertySetId.
				if (item.ExtendedProperty != null)
				{
					for (int i = 0; i < item.ExtendedProperty.Length; i++)
					{
						PathToExtendedFieldType pathToProp = item.ExtendedProperty[i].ExtendedFieldURI;
						if (pathToProp.PropertySetId == null &&
							pathToProp.DistinguishedPropertySetIdSpecified)
						{
							switch (pathToProp.DistinguishedPropertySetId)
							{
 								case DistinguishedPropertySetType.Appointment:
									pathToProp.PropertySetId = ExchangeCalendarFolder.PropertySetId;
									break;

								case DistinguishedPropertySetType.Task:
									pathToProp.PropertySetId = ExchangeTasksFolder.PropertySetId;
									break;
							}

							if (pathToProp.PropertySetId != null)
								pathToProp.DistinguishedPropertySetIdSpecified = false;
						}
					}
				}

				// --------- Update common properties -----------
				if (item.Body != null)
					activity.Description = item.Body.Value;
				else
					activity.Description = null;

				activity.Id = item.ItemId.Id;

				if (item.LastModifiedTimeSpecified)
					activity.LastModifiedTime = item.LastModifiedTime;

				// MD 12/5/11 - TFS81049
				// This needs to be done after the activity start and end times are populated, so this has been moved to the finally block.
				//if (item.ReminderIsSetSpecified)
				//    activity.ReminderEnabled = item.ReminderIsSet;

				if (item.ReminderMinutesBeforeStart != null)
				{
					int reminderMinutes;
					if (int.TryParse(item.ReminderMinutesBeforeStart, out reminderMinutes))
						activity.ReminderInterval = TimeSpan.FromMinutes(reminderMinutes);
					else
						ExchangeConnectorUtilities.DebugFail("Could not parse the reminder minutes: " + item.ReminderMinutesBeforeStart);
				}

				activity.Subject = item.Subject;

				if (item.Categories != null)
					activity.Categories = String.Join(",", item.Categories);

				
				
				
				
				// ----------------------------------------------

				CalendarItemType calendarItem = item as CalendarItemType;
				if (calendarItem != null)
				{
					Appointment appointment = activity as Appointment;
					Debug.Assert(appointment != null, "The activity should have been an Appointment");

					if (appointment != null)
						EWSUtilities.UpdateAppointment(appointment, calendarItem, timeZoneInfoProvider);

					return;
				}

				TaskType taskItem = item as TaskType;
				if (taskItem != null)
				{
					Task task = activity as Task;
					Debug.Assert(task != null, "The activity should have been an Task");

					if (task != null)
						EWSUtilities.UpdateTask(task, taskItem, timeZoneInfoProvider);

					return;
				}

				MessageType messageItem = item as MessageType;
				if (messageItem != null && messageItem.ItemClass == ExchangeJournalFolder.ItemClass)
				{
					Journal journal = activity as Journal;
					Debug.Assert(journal != null, "The activity should have been a Journal");

					if (journal != null)
						EWSUtilities.UpdateJournal(journal, messageItem, timeZoneInfoProvider);

					return;
				}

				ExchangeConnectorUtilities.DebugFail("Cannot determine how to update this activity type.");
			}
			finally
			{
				// MD 12/5/11 - TFS81049
				// Moved from above because we need the activity start time to be set when determining whether the reminder is enabled.
				if (item.ReminderIsSetSpecified)
				{
					// If the reminder is set and this is an occurrence, make sure it starts after the time of the last displayed reminder
					// so we don't show a duplicate reminders.
					bool reminderIsSet = item.ReminderIsSet;
					if (reminderIsSet && activity.IsOccurrence)
					{
						Reminder reminder = activity.RootActivity.Reminder;
						if (reminder != null)
							reminderIsSet = activity.Start > reminder.LastDisplayedTime;
					}

					activity.ReminderEnabled = reminderIsSet;
				}

				// This must be set last because we may look at the previous data item's values in some cases.
				activity.DataItem = item;
			}
		}

		#endregion  // UpdateActivity

		#region UpdateAppointment

		private static void UpdateAppointment(Appointment appointment, CalendarItemType calendarItem, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			TimeZoneToken startToken;
			TimeZoneToken endToken;
			EWSUtilities.GetTimeZones(calendarItem, timeZoneInfoProvider, out startToken, out endToken);

			appointment.StartTimeZoneId = startToken.Id;
			appointment.EndTimeZoneId = endToken.Id;

			if (calendarItem.IsAllDayEventSpecified)
			    appointment.IsTimeZoneNeutral = calendarItem.IsAllDayEvent;

			
			bool supportsTimeOfDay = true;

			if (calendarItem.StartSpecified)
			{
				// MD 11/4/11 - TFS75795
				// The SetStartEnd method needs the time zone for the start/end time.
				//ExchangeConnectorUtilities.SetStartEnd(appointment, calendarItem.Start, timeZoneInfoProvider, supportsTimeOfDay, true);
				ExchangeConnectorUtilities.SetStartEnd(appointment, calendarItem.Start, startToken, timeZoneInfoProvider, supportsTimeOfDay, true);
			}

			if (calendarItem.EndSpecified)
			{
				// MD 11/4/11 - TFS75795
				// The SetStartEnd method needs the time zone for the start/end time.
				//ExchangeConnectorUtilities.SetStartEnd(appointment, calendarItem.End, timeZoneInfoProvider, supportsTimeOfDay, false);
				ExchangeConnectorUtilities.SetStartEnd(appointment, calendarItem.End, endToken, timeZoneInfoProvider, supportsTimeOfDay, false);
			}

			appointment.Location = calendarItem.Location;

			if (calendarItem.Recurrence != null)
			{
				DateRecurrence recurrence;
				
				
				
				
				
				
				
				
				{
					recurrence = EWSUtilities.ParseExchangeRecurrence(
						calendarItem.Recurrence.Item,
						calendarItem.Recurrence.Item1);
				}

				appointment.Recurrence = recurrence;
			}
			else
			{
				appointment.Recurrence = null;
			}

			bool isVariance = false;
			if (calendarItem.CalendarItemType1Specified &&
				calendarItem.CalendarItemType1 == CalendarItemTypeType.Exception)
			{
				isVariance = true;
			}

			appointment.IsVariance = isVariance;
		}

		#endregion  // UpdateAppointment

		#region UpdateCalendarItem

		private static void UpdateCalendarItem(
			CalendarItemType calendarItem, 
			List<ExtendedPropertyType> extendedProperties, 
			Appointment appointment, 
			TimeZoneToken activityStartLocalToken,
			TimeZoneToken activityEndLocalToken,
			ExchangeScheduleDataConnector connector)
		{
			ExchangeVersion version = connector.RequestedServerVersionResolved;

			// In Exchange2007_SP1, the dates are expected to be in local time. In later versions, the dates are in local time.
			if (version == ExchangeVersion.Exchange2007_SP1)
			{
				calendarItem.Start = appointment.GetStartUtc(activityStartLocalToken);
				calendarItem.End = appointment.GetEndUtc(activityEndLocalToken);
			}
			else
			{
				calendarItem.Start = appointment.GetStartLocal(activityStartLocalToken);
				calendarItem.End = appointment.GetEndLocal(activityEndLocalToken);
			}

			calendarItem.StartSpecified = true;
			calendarItem.EndSpecified = true;

			calendarItem.TimeZone = null;
			calendarItem.MeetingTimeZone = null;
			calendarItem.StartTimeZone = null;
			calendarItem.EndTimeZone = null;

			if (version == ExchangeVersion.Exchange2007_SP1)
			{
				calendarItem.MeetingTimeZone = new TimeZoneType();
				calendarItem.MeetingTimeZone.TimeZoneName = activityStartLocalToken.Id;
			}
			else
			{
				calendarItem.StartTimeZone = new TimeZoneDefinitionType();
				calendarItem.StartTimeZone.Id = activityStartLocalToken.Id;

				calendarItem.EndTimeZone = new TimeZoneDefinitionType();
				calendarItem.EndTimeZone.Id = activityEndLocalToken.Id;
			}

			calendarItem.IsAllDayEvent = appointment.IsTimeZoneNeutral;
			calendarItem.IsAllDayEventSpecified = true;

			calendarItem.Location = appointment.Location;

			DateRecurrence dateRecurrence = appointment.Recurrence as DateRecurrence;
			Debug.Assert(
				(dateRecurrence == null) == (appointment.Recurrence == null), 
				"The recurrence was not a DateRecurrence instance.");

			if (dateRecurrence != null)
			{
				
				
				
				
				
				
				
				
				
				
				
				
				
				{
					TimeZoneInfoProvider timeZoneInfoProvider = connector.TimeZoneInfoProviderResolved;

					RecurrencePatternBaseType pattern;
					RecurrenceRangeBaseType range;
					if (EWSUtilities.TryGetExchangeRecurrence(appointment, dateRecurrence, timeZoneInfoProvider, out pattern, out range))
					{
						calendarItem.Recurrence = new RecurrenceType();
						calendarItem.Recurrence.Item = pattern;
						calendarItem.Recurrence.Item1 = range;
					}
				}
			}
			else
			{
				calendarItem.Recurrence = null;
			}
		} 

		#endregion  // UpdateCalendarItem

		#region UpdateExtendedProperty

		public static void UpdateExtendedProperty(
			List<ExtendedPropertyType> extendedProperties,
			string propertySetId,
			int propertyId,
			MapiPropertyTypeType propertyType,
			object value)
		{
			if (value != null)
			{
				if (value is DateTime)
					value = EWSUtilities.GetExchangeDateTime((DateTime)value);
				else if (value is byte[])
					value = Convert.ToBase64String((byte[])value);
				else
					value = value.ToString();
			}

			for (int i = 0; i < extendedProperties.Count; i++)
			{
				ExtendedPropertyType existingProperty = extendedProperties[i];

				if (existingProperty.ExtendedFieldURI.PropertySetId != propertySetId)
					continue;

				if (existingProperty.ExtendedFieldURI.PropertyId != propertyId)
					continue;

				if (value == null)
				{
					extendedProperties.RemoveAt(i);
				}
				else
				{
					existingProperty.ExtendedFieldURI.PropertyType = propertyType;
					existingProperty.Item = value;
				}
				return;
			}

			if (value == null)
				return;

			ExtendedPropertyType property = new ExtendedPropertyType();
			property.ExtendedFieldURI = EWSUtilities.CreateExtendedProperty(
				propertySetId,
				propertyId,
				propertyType);
			property.Item = value;
			extendedProperties.Add(property);
		} 

		#endregion  // UpdateExtendedProperty

		#region UpdateItem

		public static void UpdateItem(ItemType item, ActivityBase activity, ExchangeScheduleDataConnector connector)
		{
			// --------- Update common properties -----------
			item.Body = new BodyType();
			item.Body.BodyType1 = BodyTypeType.Text;
			item.Body.Value = activity.Description ?? string.Empty;

			item.ReminderIsSet = activity.ReminderEnabled;
			item.ReminderIsSetSpecified = true;

			item.ReminderMinutesBeforeStart = Math.Round(activity.ReminderInterval.TotalMinutes).ToString();
			item.Subject = activity.Subject;

			if (string.IsNullOrEmpty(activity.Categories) == false)
				item.Categories = activity.Categories.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			
			
			
			
			// ----------------------------------------------

			TimeZoneInfoProvider timeZoneInfoProvider = connector.TimeZoneInfoProviderResolved;
			TimeZoneToken activityStartLocalToken = EWSUtilities.GetTimeZoneToken(activity.StartTimeZoneId, activity.EndTimeZoneId, timeZoneInfoProvider);
			TimeZoneToken activityEndLocalToken = EWSUtilities.GetTimeZoneToken(activity.EndTimeZoneId, activity.StartTimeZoneId, timeZoneInfoProvider);

			List<ExtendedPropertyType> extendedProperties = new List<ExtendedPropertyType>();
			if (item.ExtendedProperty != null)
				extendedProperties.AddRange(item.ExtendedProperty);

			try
			{
				CalendarItemType calendarItem = item as CalendarItemType;
				if (calendarItem != null)
				{
					Appointment appointment = activity as Appointment;
					Debug.Assert(appointment != null, "The activity should have been an Appointment");

					if (appointment != null)
						EWSUtilities.UpdateCalendarItem(calendarItem, extendedProperties, appointment, activityStartLocalToken, activityEndLocalToken, connector);

					return;
				}

				TaskType taskItem = item as TaskType;
				if (taskItem != null)
				{
					Task task = activity as Task;
					Debug.Assert(task != null, "The activity should have been an Task");

					if (task != null)
						EWSUtilities.UpdateTaskItem(taskItem, extendedProperties, task, activityStartLocalToken, activityEndLocalToken, connector);

					return;
				}

				MessageType messageItem = item as MessageType;
				if (messageItem != null && messageItem.ItemClass == ExchangeJournalFolder.ItemClass)
				{
					Journal journal = activity as Journal;
					Debug.Assert(journal != null, "The activity should have been a Journal");

					if (journal != null)
						EWSUtilities.UpdateJournalItem(messageItem, extendedProperties, journal, activityStartLocalToken, activityEndLocalToken);

					return;
				}

				ExchangeConnectorUtilities.DebugFail("Cannot determine how to update this activity type.");
			}
			finally
			{
				if (extendedProperties.Count == 0)
					item.ExtendedProperty = null;
				else
					item.ExtendedProperty = extendedProperties.ToArray();
			}
		}

		#endregion  // UpdateItem

		#region UpdateJournal

		private static void UpdateJournal(Journal journal, MessageType messageItem, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			string localId = timeZoneInfoProvider.LocalTimeZoneIdResolved;
			journal.StartTimeZoneId = localId;
			journal.EndTimeZoneId = localId;

			
			bool supportsTimeOfDay = true;

			foreach (ExtendedPropertyType extendedProperty in messageItem.ExtendedProperty)
			{
				if (extendedProperty.ExtendedFieldURI.PropertySetId != ExchangeJournalFolder.PropertySetId)
					continue;

				switch (extendedProperty.ExtendedFieldURI.PropertyId)
				{
					case ExchangeJournalFolder.EndPropertyId:
						{
							string endTime = (string)extendedProperty.Item;
							DateTime endTimeValue;
							if (EWSUtilities.TryParseExchangeDateTime(endTime, out endTimeValue) == false)
							{
								ExchangeConnectorUtilities.DebugFail("Couldn't parse end time: " + endTime);
								continue;
							}

							// MD 11/4/11 - TFS75795
							// The SetStartEnd method needs the time zone for the start/end time.
							//ExchangeConnectorUtilities.SetStartEnd(journal, endTimeValue, timeZoneInfoProvider, supportsTimeOfDay, false);
							ExchangeConnectorUtilities.SetStartEnd(journal, endTimeValue, timeZoneInfoProvider.LocalToken, timeZoneInfoProvider, supportsTimeOfDay, false);
						}
						break;

					case ExchangeJournalFolder.StartPropertyId:
						{
							string startTime = (string)extendedProperty.Item;
							DateTime startTimeValue;
							if (EWSUtilities.TryParseExchangeDateTime(startTime, out startTimeValue) == false)
							{
								ExchangeConnectorUtilities.DebugFail("Couldn't parse start time: " + startTime);
								continue;
							}

							// MD 11/4/11 - TFS75795
							// The SetStartEnd method needs the time zone for the start/end time.
							//ExchangeConnectorUtilities.SetStartEnd(journal, startTimeValue, timeZoneInfoProvider, supportsTimeOfDay, true);
							ExchangeConnectorUtilities.SetStartEnd(journal, startTimeValue, timeZoneInfoProvider.LocalToken, timeZoneInfoProvider, supportsTimeOfDay, true);
						}
						break;

					default:
						ExchangeConnectorUtilities.DebugFail("Unknown property ID: " + extendedProperty.ExtendedFieldURI.PropertyId);
						break;
				}
			}
		}

		#endregion  // UpdateJournal

		#region UpdateJournalItem

		private static void UpdateJournalItem(
			MessageType messageItem, 
			List<ExtendedPropertyType> extendedProperties, 
			Journal journal, 
			TimeZoneToken activityStartLocalToken,
			TimeZoneToken activityEndLocalToken)
		{
			EWSUtilities.UpdateExtendedProperty(extendedProperties,
				ExchangeJournalFolder.PropertySetId, ExchangeJournalFolder.StartPropertyId, MapiPropertyTypeType.SystemTime,
				journal.GetStartUtc(activityStartLocalToken));

			EWSUtilities.UpdateExtendedProperty(extendedProperties,
				ExchangeJournalFolder.PropertySetId, ExchangeJournalFolder.EndPropertyId, MapiPropertyTypeType.SystemTime,
				journal.GetEndUtc(activityEndLocalToken));
		} 

		#endregion  // UpdateJournalItem

		#region UpdateTask

		private static void UpdateTask(Task task, TaskType taskItem, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			task.IsTimeZoneNeutral = true;

			string localId = timeZoneInfoProvider.LocalTimeZoneIdResolved;
			task.StartTimeZoneId = localId;
			task.EndTimeZoneId = localId;

			
			bool supportsTimeOfDay = false;

			if (taskItem.DueDateSpecified)
			{
				// MD 11/4/11 - TFS75795
				// The SetStartEnd method needs the time zone for the start/end time.
				//ExchangeConnectorUtilities.SetStartEnd(task, taskItem.DueDate, timeZoneInfoProvider, supportsTimeOfDay, false);
				ExchangeConnectorUtilities.SetStartEnd(task, taskItem.DueDate, timeZoneInfoProvider.LocalToken, timeZoneInfoProvider, supportsTimeOfDay, false);
			}

			if (taskItem.StartDateSpecified)
			{
				// MD 11/4/11 - TFS75795
				// The SetStartEnd method needs the time zone for the start/end time.
				//ExchangeConnectorUtilities.SetStartEnd(task, taskItem.StartDate, timeZoneInfoProvider, supportsTimeOfDay, true);
				ExchangeConnectorUtilities.SetStartEnd(task, taskItem.StartDate, timeZoneInfoProvider.LocalToken, timeZoneInfoProvider, supportsTimeOfDay, true);
			}
			else // If the start is not specified, set it to the same value as the end.
			{
				// MD 11/4/11 - TFS75795
				// The SetStartEnd method needs the time zone for the start/end time.
				//ExchangeConnectorUtilities.SetStartEnd(task, taskItem.DueDate, timeZoneInfoProvider, supportsTimeOfDay, true);
				ExchangeConnectorUtilities.SetStartEnd(task, taskItem.DueDate, timeZoneInfoProvider.LocalToken, timeZoneInfoProvider, supportsTimeOfDay, true);
			}

			// The ReminderMinutesBeforeStart field seems to always be 0 for tasks, so we need to instead us the ReminderDueBy field.
			if (taskItem.ReminderDueBySpecified)
				task.ReminderInterval = taskItem.StartDate - taskItem.ReminderDueBy;

			if (taskItem.PercentCompleteSpecified)
				task.PercentComplete = (int)Math.Round(taskItem.PercentComplete);

			if (taskItem.Recurrence != null)
			{
				DateRecurrence recurrence;
				
				
				
				
				
				
				
				
				{
					recurrence = EWSUtilities.ParseExchangeRecurrence(
						taskItem.Recurrence.Item,
						taskItem.Recurrence.Item1);
				}

				task.Recurrence = recurrence;
			}
			else
			{
				task.Recurrence = null;
			}
		}

		#endregion  // UpdateTask

		#region UpdateTaskItem

		private static void UpdateTaskItem(
			TaskType taskItem, 
			List<ExtendedPropertyType> extendedProperties, 
			Task task,
			TimeZoneToken activityStartLocalToken,
			TimeZoneToken activityEndLocalToken,
			ExchangeScheduleDataConnector connector)
		{
			DateTime taskStart = task.GetStartUtc(activityStartLocalToken);
			DateTime taskEnd = task.GetEndUtc(activityEndLocalToken);

			bool shouldUpdateStartDate = true;

			// If we got this item from the server (the due date was already specified) and the start date was not specified,
			// and the start and end are the same, we should not update the start date on the server. Just leave it set to "None"
			// on the server.
			if (taskItem.DueDateSpecified == true && taskItem.StartDateSpecified == false)
			{
				if (taskStart == taskEnd)
					shouldUpdateStartDate = false;
			}

			if (shouldUpdateStartDate)
			{
				taskItem.StartDate = taskStart;
				taskItem.StartDateSpecified = true;
			}

			taskItem.DueDate = taskEnd;
			taskItem.DueDateSpecified = true;

			taskItem.PercentComplete = task.PercentComplete;
			taskItem.PercentCompleteSpecified = true;

			Resource resource = task.OwningResource;
			string newOwner = resource != null ? resource.Name : null;

			EWSUtilities.UpdateExtendedProperty(extendedProperties,
				ExchangeTasksFolder.PropertySetId, ExchangeTasksFolder.OwnerPropertyId, MapiPropertyTypeType.String,
				newOwner);

			DateRecurrence dateRecurrence = task.Recurrence as DateRecurrence;
			Debug.Assert(
				(dateRecurrence == null) == (task.Recurrence == null),
				"The recurrence was not a DateRecurrence instance.");

			if (dateRecurrence != null)
			{
				
				
				
				
				
				
				
				
				
				
				
				{
					TimeZoneInfoProvider timeZoneInfoProvider = connector.TimeZoneInfoProviderResolved;

					RecurrencePatternBaseType pattern;
					RecurrenceRangeBaseType range;
					if (EWSUtilities.TryGetExchangeRecurrence(task, dateRecurrence, timeZoneInfoProvider, out pattern, out range))
					{
						taskItem.Recurrence = new TaskRecurrenceType();
						taskItem.Recurrence.Item = pattern;
						taskItem.Recurrence.Item1 = range;
					}
				}
			}
			else
			{
				taskItem.Recurrence = null;
			}
		} 

		#endregion  // UpdateTaskItem
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