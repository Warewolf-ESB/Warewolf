using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Schedules.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Schedules.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Schedules.ScheduleControlBase);
				Assembly controlAssembly = t.Assembly;

				#region ScheduleTimeControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleTimeControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentTimeIndicatorVisibility",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_CurrentTimeIndicatorVisibility_Property")),
				    new DisplayNameAttribute("CurrentTimeIndicatorVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeslotInterval",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_TimeslotInterval_Property")),
				    new DisplayNameAttribute("TimeslotInterval"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PrimaryTimeZoneLabel",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_PrimaryTimeZoneLabel_Property")),
				    new DisplayNameAttribute("PrimaryTimeZoneLabel"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneLabel",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_SecondaryTimeZoneLabel_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneLabel"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneVisibility",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_SecondaryTimeZoneVisibility_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneId",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_SecondaryTimeZoneId_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneId"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWorkingHoursOnly",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_ShowWorkingHoursOnly_Property")),
				    new DisplayNameAttribute("ShowWorkingHoursOnly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorkingHoursSource",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_WorkingHoursSource_Property")),
				    new DisplayNameAttribute("WorkingHoursSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekDisplayMode",
					new DescriptionAttribute(SR.GetString("ScheduleTimeControlBase_WeekDisplayMode_Property")),
				    new DisplayNameAttribute("WeekDisplayMode"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleTimeControlBase Properties

				#region ScheduleControlBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleControlBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroupsResolved",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_CalendarGroupsResolved_Property")),
				    new DisplayNameAttribute("CalendarGroupsResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VisibleDates",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_VisibleDates_Property")),
				    new DisplayNameAttribute("VisibleDates"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarDisplayMode",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_CalendarDisplayMode_Property")),
				    new DisplayNameAttribute("CalendarDisplayMode"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VisibleCalendarCount",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_VisibleCalendarCount_Property")),
				    new DisplayNameAttribute("VisibleCalendarCount"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultBrushProvider",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_DefaultBrushProvider_Property")),
				    new DisplayNameAttribute("DefaultBrushProvider"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroupsOverride",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_CalendarGroupsOverride_Property")),
				    new DisplayNameAttribute("CalendarGroupsOverride"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollBarStyle",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_ScrollBarStyle_Property")),
				    new DisplayNameAttribute("ScrollBarStyle"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowCalendarCloseButton",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_ShowCalendarCloseButton_Property")),
				    new DisplayNameAttribute("ShowCalendarCloseButton"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowCalendarOverlayButton",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_ShowCalendarOverlayButton_Property")),
				    new DisplayNameAttribute("ShowCalendarOverlayButton"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarHeaderAreaVisibilityResolved",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_CalendarHeaderAreaVisibilityResolved_Property")),
				    new DisplayNameAttribute("CalendarHeaderAreaVisibilityResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedTimeRange",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_SelectedTimeRange_Property")),
				    new DisplayNameAttribute("SelectedTimeRange"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveCalendar",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_ActiveCalendar_Property")),
				    new DisplayNameAttribute("ActiveCalendar"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivities",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_SelectedActivities_Property")),
				    new DisplayNameAttribute("SelectedActivities"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "BlockingError",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_BlockingError_Property")),
				    new DisplayNameAttribute("BlockingError"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCalendarGroupResizing",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_AllowCalendarGroupResizing_Property")),
				    new DisplayNameAttribute("AllowCalendarGroupResizing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinCalendarGroupExtent",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_MinCalendarGroupExtent_Property")),
				    new DisplayNameAttribute("MinCalendarGroupExtent"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreferredCalendarGroupExtent",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_PreferredCalendarGroupExtent_Property")),
				    new DisplayNameAttribute("PreferredCalendarGroupExtent"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("ScheduleControlBase_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled")				);

				#endregion // ScheduleControlBase Properties

				#region Resource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Resource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Calendars",
					new DescriptionAttribute(SR.GetString("Resource_Calendars_Property")),
				    new DisplayNameAttribute("Calendars")				);


				tableBuilder.AddCustomAttributes(t, "DataItem",
					new DescriptionAttribute(SR.GetString("Resource_DataItem_Property")),
				    new DisplayNameAttribute("DataItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "DaySettingsOverrides",
					new DescriptionAttribute(SR.GetString("Resource_DaySettingsOverrides_Property")),
				    new DisplayNameAttribute("DaySettingsOverrides")				);


				tableBuilder.AddCustomAttributes(t, "DaysOfWeek",
					new DescriptionAttribute(SR.GetString("Resource_DaysOfWeek_Property")),
				    new DisplayNameAttribute("DaysOfWeek")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("Resource_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "EmailAddress",
					new DescriptionAttribute(SR.GetString("Resource_EmailAddress_Property")),
				    new DisplayNameAttribute("EmailAddress")				);


				tableBuilder.AddCustomAttributes(t, "Id",
					new DescriptionAttribute(SR.GetString("Resource_Id_Property")),
				    new DisplayNameAttribute("Id")				);


				tableBuilder.AddCustomAttributes(t, "IsVisibleResolved",
					new DescriptionAttribute(SR.GetString("Resource_IsVisibleResolved_Property")),
				    new DisplayNameAttribute("IsVisibleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("Resource_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "PrimaryCalendar",
					new DescriptionAttribute(SR.GetString("Resource_PrimaryCalendar_Property")),
				    new DisplayNameAttribute("PrimaryCalendar")				);


				tableBuilder.AddCustomAttributes(t, "PrimaryCalendarId",
					new DescriptionAttribute(SR.GetString("Resource_PrimaryCalendarId_Property")),
				    new DisplayNameAttribute("PrimaryCalendarId")				);


				tableBuilder.AddCustomAttributes(t, "IsLocked",
					new DescriptionAttribute(SR.GetString("Resource_IsLocked_Property")),
				    new DisplayNameAttribute("IsLocked")				);


				tableBuilder.AddCustomAttributes(t, "IsVisible",
					new DescriptionAttribute(SR.GetString("Resource_IsVisible_Property")),
				    new DisplayNameAttribute("IsVisible")				);


				tableBuilder.AddCustomAttributes(t, "FirstDayOfWeek",
					new DescriptionAttribute(SR.GetString("Resource_FirstDayOfWeek_Property")),
				    new DisplayNameAttribute("FirstDayOfWeek")				);


				tableBuilder.AddCustomAttributes(t, "PrimaryTimeZoneId",
					new DescriptionAttribute(SR.GetString("Resource_PrimaryTimeZoneId_Property")),
				    new DisplayNameAttribute("PrimaryTimeZoneId")				);


				tableBuilder.AddCustomAttributes(t, "Metadata",
					new DescriptionAttribute(SR.GetString("Resource_Metadata_Property")),
				    new DisplayNameAttribute("Metadata")				);


				tableBuilder.AddCustomAttributes(t, "CustomActivityCategories",
					new DescriptionAttribute(SR.GetString("Resource_CustomActivityCategories_Property")),
				    new DisplayNameAttribute("CustomActivityCategories")				);

				#endregion // Resource Properties

				#region DaySettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DaySettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsWorkday",
					new DescriptionAttribute(SR.GetString("DaySettings_IsWorkday_Property")),
				    new DisplayNameAttribute("IsWorkday")				);


				tableBuilder.AddCustomAttributes(t, "WorkingHours",
					new DescriptionAttribute(SR.GetString("DaySettings_WorkingHours_Property")),
				    new DisplayNameAttribute("WorkingHours")				);

				#endregion // DaySettings Properties

				#region ListScheduleDataConnector Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ListScheduleDataConnector");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("ListScheduleDataConnectorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("ListScheduleDataConnectorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItems",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ResourceItems_Property")),
				    new DisplayNameAttribute("ResourceItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ResourceItemsSource_Property")),
				    new DisplayNameAttribute("ResourceItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AppointmentItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_AppointmentItemsSource_Property")),
				    new DisplayNameAttribute("AppointmentItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "JournalItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_JournalItemsSource_Property")),
				    new DisplayNameAttribute("JournalItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurringAppointmentItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_RecurringAppointmentItemsSource_Property")),
				    new DisplayNameAttribute("RecurringAppointmentItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceCalendarItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ResourceCalendarItemsSource_Property")),
				    new DisplayNameAttribute("ResourceCalendarItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TaskItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_TaskItemsSource_Property")),
				    new DisplayNameAttribute("TaskItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AppointmentPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_AppointmentPropertyMappings_Property")),
				    new DisplayNameAttribute("AppointmentPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "JournalPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_JournalPropertyMappings_Property")),
				    new DisplayNameAttribute("JournalPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourcePropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ResourcePropertyMappings_Property")),
				    new DisplayNameAttribute("ResourcePropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceCalendarPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ResourceCalendarPropertyMappings_Property")),
				    new DisplayNameAttribute("ResourceCalendarPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TaskPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_TaskPropertyMappings_Property")),
				    new DisplayNameAttribute("TaskPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurringAppointmentPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_RecurringAppointmentPropertyMappings_Property")),
				    new DisplayNameAttribute("RecurringAppointmentPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryItemsSource",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ActivityCategoryItemsSource_Property")),
				    new DisplayNameAttribute("ActivityCategoryItemsSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryPropertyMappings",
					new DescriptionAttribute(SR.GetString("ListScheduleDataConnector_ActivityCategoryPropertyMappings_Property")),
				    new DisplayNameAttribute("ActivityCategoryPropertyMappings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ListScheduleDataConnector Properties

				#region ScheduleDataConnectorBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleDataConnectorBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItems",
					new DescriptionAttribute(SR.GetString("ScheduleDataConnectorBase_ResourceItems_Property")),
				    new DisplayNameAttribute("ResourceItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceCalculatorFactory",
					new DescriptionAttribute(SR.GetString("ScheduleDataConnectorBase_RecurrenceCalculatorFactory_Property")),
				    new DisplayNameAttribute("RecurrenceCalculatorFactory"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceCalculatorFactoryResolved",
					new DescriptionAttribute(SR.GetString("ScheduleDataConnectorBase_RecurrenceCalculatorFactoryResolved_Property")),
				    new DisplayNameAttribute("RecurrenceCalculatorFactoryResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeZoneInfoProvider",
					new DescriptionAttribute(SR.GetString("ScheduleDataConnectorBase_TimeZoneInfoProvider_Property")),
				    new DisplayNameAttribute("TimeZoneInfoProvider"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeZoneInfoProviderResolved",
					new DescriptionAttribute(SR.GetString("ScheduleDataConnectorBase_TimeZoneInfoProviderResolved_Property")),
				    new DisplayNameAttribute("TimeZoneInfoProviderResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleDataConnectorBase Properties

				#region Appointment Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Appointment");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("Appointment_End_Property")),
				    new DisplayNameAttribute("End")				);


				tableBuilder.AddCustomAttributes(t, "Location",
					new DescriptionAttribute(SR.GetString("Appointment_Location_Property")),
				    new DisplayNameAttribute("Location")				);


				tableBuilder.AddCustomAttributes(t, "Recurrence",
					new DescriptionAttribute(SR.GetString("Appointment_Recurrence_Property")),
				    new DisplayNameAttribute("Recurrence")				);


				tableBuilder.AddCustomAttributes(t, "Reminder",
					new DescriptionAttribute(SR.GetString("Appointment_Reminder_Property")),
				    new DisplayNameAttribute("Reminder")				);


				tableBuilder.AddCustomAttributes(t, "Subject",
					new DescriptionAttribute(SR.GetString("Appointment_Subject_Property")),
				    new DisplayNameAttribute("Subject")				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("Appointment_Start_Property")),
				    new DisplayNameAttribute("Start")				);


				tableBuilder.AddCustomAttributes(t, "ActivityType",
					new DescriptionAttribute(SR.GetString("Appointment_ActivityType_Property")),
				    new DisplayNameAttribute("ActivityType")				);


				tableBuilder.AddCustomAttributes(t, "EndTimeZoneId",
					new DescriptionAttribute(SR.GetString("Appointment_EndTimeZoneId_Property")),
				    new DisplayNameAttribute("EndTimeZoneId")				);

				#endregion // Appointment Properties

				#region ActivityBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataItem",
					new DescriptionAttribute(SR.GetString("ActivityBase_DataItem_Property")),
				    new DisplayNameAttribute("DataItem"),
				    new TypeConverterAttribute(typeof(StringConverter))
				);


				tableBuilder.AddCustomAttributes(t, "Id",
					new DescriptionAttribute(SR.GetString("ActivityBase_Id_Property")),
				    new DisplayNameAttribute("Id")				);


				tableBuilder.AddCustomAttributes(t, "Metadata",
					new DescriptionAttribute(SR.GetString("ActivityBase_Metadata_Property")),
				    new DisplayNameAttribute("Metadata")				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("ActivityBase_End_Property")),
				    new DisplayNameAttribute("End")				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("ActivityBase_Start_Property")),
				    new DisplayNameAttribute("Start")				);


				tableBuilder.AddCustomAttributes(t, "ActivityType",
					new DescriptionAttribute(SR.GetString("ActivityBase_ActivityType_Property")),
				    new DisplayNameAttribute("ActivityType")				);


				tableBuilder.AddCustomAttributes(t, "OwningResource",
					new DescriptionAttribute(SR.GetString("ActivityBase_OwningResource_Property")),
				    new DisplayNameAttribute("OwningResource")				);


				tableBuilder.AddCustomAttributes(t, "OwningResourceId",
					new DescriptionAttribute(SR.GetString("ActivityBase_OwningResourceId_Property")),
				    new DisplayNameAttribute("OwningResourceId")				);


				tableBuilder.AddCustomAttributes(t, "OwningCalendar",
					new DescriptionAttribute(SR.GetString("ActivityBase_OwningCalendar_Property")),
				    new DisplayNameAttribute("OwningCalendar")				);


				tableBuilder.AddCustomAttributes(t, "OwningCalendarId",
					new DescriptionAttribute(SR.GetString("ActivityBase_OwningCalendarId_Property")),
				    new DisplayNameAttribute("OwningCalendarId")				);


				tableBuilder.AddCustomAttributes(t, "Subject",
					new DescriptionAttribute(SR.GetString("ActivityBase_Subject_Property")),
				    new DisplayNameAttribute("Subject")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("ActivityBase_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "IsVisible",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsVisible_Property")),
				    new DisplayNameAttribute("IsVisible")				);


				tableBuilder.AddCustomAttributes(t, "IsVisibleResolved",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsVisibleResolved_Property")),
				    new DisplayNameAttribute("IsVisibleResolved")				);


				tableBuilder.AddCustomAttributes(t, "IsLocked",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsLocked_Property")),
				    new DisplayNameAttribute("IsLocked")				);


				tableBuilder.AddCustomAttributes(t, "EndTimeZoneId",
					new DescriptionAttribute(SR.GetString("ActivityBase_EndTimeZoneId_Property")),
				    new DisplayNameAttribute("EndTimeZoneId")				);


				tableBuilder.AddCustomAttributes(t, "Recurrence",
					new DescriptionAttribute(SR.GetString("ActivityBase_Recurrence_Property")),
				    new DisplayNameAttribute("Recurrence")				);


				tableBuilder.AddCustomAttributes(t, "RootActivity",
					new DescriptionAttribute(SR.GetString("ActivityBase_RootActivity_Property")),
				    new DisplayNameAttribute("RootActivity")				);


				tableBuilder.AddCustomAttributes(t, "RootActivityId",
					new DescriptionAttribute(SR.GetString("ActivityBase_RootActivityId_Property")),
				    new DisplayNameAttribute("RootActivityId")				);


				tableBuilder.AddCustomAttributes(t, "StartTimeZoneId",
					new DescriptionAttribute(SR.GetString("ActivityBase_StartTimeZoneId_Property")),
				    new DisplayNameAttribute("StartTimeZoneId")				);


				tableBuilder.AddCustomAttributes(t, "IsTimeZoneNeutral",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsTimeZoneNeutral_Property")),
				    new DisplayNameAttribute("IsTimeZoneNeutral")				);


				tableBuilder.AddCustomAttributes(t, "MaxOccurrenceDateTime",
					new DescriptionAttribute(SR.GetString("ActivityBase_MaxOccurrenceDateTime_Property")),
				    new DisplayNameAttribute("MaxOccurrenceDateTime")				);


				tableBuilder.AddCustomAttributes(t, "Duration",
					new DescriptionAttribute(SR.GetString("ActivityBase_Duration_Property")),
				    new DisplayNameAttribute("Duration")				);


				tableBuilder.AddCustomAttributes(t, "IsOccurrence",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsOccurrence_Property")),
				    new DisplayNameAttribute("IsOccurrence")				);


				tableBuilder.AddCustomAttributes(t, "IsRecurrenceRoot",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsRecurrenceRoot_Property")),
				    new DisplayNameAttribute("IsRecurrenceRoot")				);


				tableBuilder.AddCustomAttributes(t, "OriginalOccurrenceEnd",
					new DescriptionAttribute(SR.GetString("ActivityBase_OriginalOccurrenceEnd_Property")),
				    new DisplayNameAttribute("OriginalOccurrenceEnd")				);


				tableBuilder.AddCustomAttributes(t, "OriginalOccurrenceStart",
					new DescriptionAttribute(SR.GetString("ActivityBase_OriginalOccurrenceStart_Property")),
				    new DisplayNameAttribute("OriginalOccurrenceStart")				);


				tableBuilder.AddCustomAttributes(t, "ReminderInterval",
					new DescriptionAttribute(SR.GetString("ActivityBase_ReminderInterval_Property")),
				    new DisplayNameAttribute("ReminderInterval")				);


				tableBuilder.AddCustomAttributes(t, "Reminder",
					new DescriptionAttribute(SR.GetString("ActivityBase_Reminder_Property")),
				    new DisplayNameAttribute("Reminder")				);


				tableBuilder.AddCustomAttributes(t, "IsVariance",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsVariance_Property")),
				    new DisplayNameAttribute("IsVariance")				);


				tableBuilder.AddCustomAttributes(t, "IsOccurrenceDeleted",
					new DescriptionAttribute(SR.GetString("ActivityBase_IsOccurrenceDeleted_Property")),
				    new DisplayNameAttribute("IsOccurrenceDeleted")				);


				tableBuilder.AddCustomAttributes(t, "ReminderEnabled",
					new DescriptionAttribute(SR.GetString("ActivityBase_ReminderEnabled_Property")),
				    new DisplayNameAttribute("ReminderEnabled")				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("ActivityBase_Error_Property")),
				    new DisplayNameAttribute("Error")				);


				tableBuilder.AddCustomAttributes(t, "LastModifiedTime",
					new DescriptionAttribute(SR.GetString("ActivityBase_LastModifiedTime_Property")),
				    new DisplayNameAttribute("LastModifiedTime")				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceVersion",
					new DescriptionAttribute(SR.GetString("ActivityBase_RecurrenceVersion_Property")),
				    new DisplayNameAttribute("RecurrenceVersion")				);


				tableBuilder.AddCustomAttributes(t, "VariantProperties",
					new DescriptionAttribute(SR.GetString("ActivityBase_VariantProperties_Property")),
				    new DisplayNameAttribute("VariantProperties")				);


				tableBuilder.AddCustomAttributes(t, "Categories",
					new DescriptionAttribute(SR.GetString("ActivityBase_Categories_Property")),
				    new DisplayNameAttribute("Categories")				);

				#endregion // ActivityBase Properties

				#region Task Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Task");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Recurrence",
					new DescriptionAttribute(SR.GetString("Task_Recurrence_Property")),
				    new DisplayNameAttribute("Recurrence")				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("Task_End_Property")),
				    new DisplayNameAttribute("End")				);


				tableBuilder.AddCustomAttributes(t, "ActivityType",
					new DescriptionAttribute(SR.GetString("Task_ActivityType_Property")),
				    new DisplayNameAttribute("ActivityType")				);


				tableBuilder.AddCustomAttributes(t, "EndTimeZoneId",
					new DescriptionAttribute(SR.GetString("Task_EndTimeZoneId_Property")),
				    new DisplayNameAttribute("EndTimeZoneId")				);


				tableBuilder.AddCustomAttributes(t, "PercentComplete",
					new DescriptionAttribute(SR.GetString("Task_PercentComplete_Property")),
				    new DisplayNameAttribute("PercentComplete")				);


				tableBuilder.AddCustomAttributes(t, "Duration",
					new DescriptionAttribute(SR.GetString("Task_Duration_Property")),
				    new DisplayNameAttribute("Duration")				);

				#endregion // Task Properties

				#region ActivityQuery Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityQuery");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityTypesToQuery",
					new DescriptionAttribute(SR.GetString("ActivityQuery_ActivityTypesToQuery_Property")),
				    new DisplayNameAttribute("ActivityTypesToQuery")				);


				tableBuilder.AddCustomAttributes(t, "Calendars",
					new DescriptionAttribute(SR.GetString("ActivityQuery_Calendars_Property")),
				    new DisplayNameAttribute("Calendars")				);


				tableBuilder.AddCustomAttributes(t, "DateRanges",
					new DescriptionAttribute(SR.GetString("ActivityQuery_DateRanges_Property")),
				    new DisplayNameAttribute("DateRanges")				);


				tableBuilder.AddCustomAttributes(t, "RequestedInformation",
					new DescriptionAttribute(SR.GetString("ActivityQuery_RequestedInformation_Property")),
				    new DisplayNameAttribute("RequestedInformation")				);

				#endregion // ActivityQuery Properties

				#region ActivityQueryResult Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityQueryResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activities",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_Activities_Property")),
				    new DisplayNameAttribute("Activities")				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_Error_Property")),
				    new DisplayNameAttribute("Error")				);


				tableBuilder.AddCustomAttributes(t, "Query",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_Query_Property")),
				    new DisplayNameAttribute("Query")				);


				tableBuilder.AddCustomAttributes(t, "HasNextActivity",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_HasNextActivity_Property")),
				    new DisplayNameAttribute("HasNextActivity")				);


				tableBuilder.AddCustomAttributes(t, "NextActivity",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_NextActivity_Property")),
				    new DisplayNameAttribute("NextActivity")				);


				tableBuilder.AddCustomAttributes(t, "HasPreviousActivity",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_HasPreviousActivity_Property")),
				    new DisplayNameAttribute("HasPreviousActivity")				);


				tableBuilder.AddCustomAttributes(t, "PreviousActivity",
					new DescriptionAttribute(SR.GetString("ActivityQueryResult_PreviousActivity_Property")),
				    new DisplayNameAttribute("PreviousActivity")				);

				#endregion // ActivityQueryResult Properties

				#region DaySettingsOverrideCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DaySettingsOverrideCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DaySettingsOverrideCollection Properties

				#region ScheduleSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DaySettingsOverrides",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_DaySettingsOverrides_Property")),
				    new DisplayNameAttribute("DaySettingsOverrides")				);


				tableBuilder.AddCustomAttributes(t, "DaysOfWeek",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_DaysOfWeek_Property")),
				    new DisplayNameAttribute("DaysOfWeek")				);


				tableBuilder.AddCustomAttributes(t, "LogicalDayDuration",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_LogicalDayDuration_Property")),
				    new DisplayNameAttribute("LogicalDayDuration")				);


				tableBuilder.AddCustomAttributes(t, "LogicalDayOffset",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_LogicalDayOffset_Property")),
				    new DisplayNameAttribute("LogicalDayOffset")				);


				tableBuilder.AddCustomAttributes(t, "MaxDate",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_MaxDate_Property")),
				    new DisplayNameAttribute("MaxDate")				);


				tableBuilder.AddCustomAttributes(t, "MinDate",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_MinDate_Property")),
				    new DisplayNameAttribute("MinDate")				);


				tableBuilder.AddCustomAttributes(t, "WorkDays",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_WorkDays_Property")),
				    new DisplayNameAttribute("WorkDays")				);


				tableBuilder.AddCustomAttributes(t, "WorkingHours",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_WorkingHours_Property")),
				    new DisplayNameAttribute("WorkingHours")				);


				tableBuilder.AddCustomAttributes(t, "AllowCalendarClosing",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_AllowCalendarClosing_Property")),
				    new DisplayNameAttribute("AllowCalendarClosing")				);


				tableBuilder.AddCustomAttributes(t, "FirstDayOfWeek",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_FirstDayOfWeek_Property")),
				    new DisplayNameAttribute("FirstDayOfWeek")				);


				tableBuilder.AddCustomAttributes(t, "AppointmentSettings",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_AppointmentSettings_Property")),
				    new DisplayNameAttribute("AppointmentSettings")				);


				tableBuilder.AddCustomAttributes(t, "JournalSettings",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_JournalSettings_Property")),
				    new DisplayNameAttribute("JournalSettings")				);


				tableBuilder.AddCustomAttributes(t, "TaskSettings",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_TaskSettings_Property")),
				    new DisplayNameAttribute("TaskSettings")				);


				tableBuilder.AddCustomAttributes(t, "AllowCustomizedCategories",
					new DescriptionAttribute(SR.GetString("ScheduleSettings_AllowCustomizedCategories_Property")),
				    new DisplayNameAttribute("AllowCustomizedCategories")				);

				#endregion // ScheduleSettings Properties

				#region ScheduleDayOfWeek Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleDayOfWeek");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DaySettings",
					new DescriptionAttribute(SR.GetString("ScheduleDayOfWeek_DaySettings_Property")),
				    new DisplayNameAttribute("DaySettings")				);

				#endregion // ScheduleDayOfWeek Properties

				#region DateRecurrence Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DateRecurrence");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Frequency",
					new DescriptionAttribute(SR.GetString("DateRecurrence_Frequency_Property")),
				    new DisplayNameAttribute("Frequency")				);


				tableBuilder.AddCustomAttributes(t, "Interval",
					new DescriptionAttribute(SR.GetString("DateRecurrence_Interval_Property")),
				    new DisplayNameAttribute("Interval")				);


				tableBuilder.AddCustomAttributes(t, "Rules",
					new DescriptionAttribute(SR.GetString("DateRecurrence_Rules_Property")),
				    new DisplayNameAttribute("Rules")				);


				tableBuilder.AddCustomAttributes(t, "Until",
					new DescriptionAttribute(SR.GetString("DateRecurrence_Until_Property")),
				    new DisplayNameAttribute("Until")				);


				tableBuilder.AddCustomAttributes(t, "WeekStart",
					new DescriptionAttribute(SR.GetString("DateRecurrence_WeekStart_Property")),
				    new DisplayNameAttribute("WeekStart")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("DateRecurrence_Count_Property")),
				    new DisplayNameAttribute("Count")				);

				#endregion // DateRecurrence Properties

				#region TimeslotPresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotPresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_End_Property")),
				    new DisplayNameAttribute("End"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsFirstInDay",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_IsFirstInDay_Property")),
				    new DisplayNameAttribute("IsFirstInDay"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsFirstInMajor",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_IsFirstInMajor_Property")),
				    new DisplayNameAttribute("IsFirstInMajor"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsLastInMajor",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_IsLastInMajor_Property")),
				    new DisplayNameAttribute("IsLastInMajor"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsLastInDay",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_IsLastInDay_Property")),
				    new DisplayNameAttribute("IsLastInDay"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("TimeslotPresenterBase_Start_Property")),
				    new DisplayNameAttribute("Start"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotPresenterBase Properties

				#region TimeslotPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayBorderBrush",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_DayBorderBrush_Property")),
				    new DisplayNameAttribute("DayBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayBorderThickness",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_DayBorderThickness_Property")),
				    new DisplayNameAttribute("DayBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayBorderVisibility",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_DayBorderVisibility_Property")),
				    new DisplayNameAttribute("DayBorderVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWorkingHour",
					new DescriptionAttribute(SR.GetString("TimeslotPresenter_IsWorkingHour_Property")),
				    new DisplayNameAttribute("IsWorkingHour"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotPresenter Properties

				#region ResourceCalendar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceCalendar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "Id",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_Id_Property")),
				    new DisplayNameAttribute("Id")				);


				tableBuilder.AddCustomAttributes(t, "IsVisibleResolved",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_IsVisibleResolved_Property")),
				    new DisplayNameAttribute("IsVisibleResolved")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "BaseColor",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_BaseColor_Property")),
				    new DisplayNameAttribute("BaseColor")				);


				tableBuilder.AddCustomAttributes(t, "BrushProvider",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_BrushProvider_Property")),
				    new DisplayNameAttribute("BrushProvider")				);


				tableBuilder.AddCustomAttributes(t, "BrushVersion",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_BrushVersion_Property")),
				    new DisplayNameAttribute("BrushVersion")				);


				tableBuilder.AddCustomAttributes(t, "DataItem",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_DataItem_Property")),
				    new DisplayNameAttribute("DataItem")				);


				tableBuilder.AddCustomAttributes(t, "OwningResourceId",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_OwningResourceId_Property")),
				    new DisplayNameAttribute("OwningResourceId")				);


				tableBuilder.AddCustomAttributes(t, "OwningResource",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_OwningResource_Property")),
				    new DisplayNameAttribute("OwningResource")				);


				tableBuilder.AddCustomAttributes(t, "IsVisible",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_IsVisible_Property")),
				    new DisplayNameAttribute("IsVisible")				);


				tableBuilder.AddCustomAttributes(t, "Metadata",
					new DescriptionAttribute(SR.GetString("ResourceCalendar_Metadata_Property")),
				    new DisplayNameAttribute("Metadata")				);

				#endregion // ResourceCalendar Properties

				#region WorkingHoursCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.WorkingHoursCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // WorkingHoursCollection Properties

				#region DaySettingsOverride Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DaySettingsOverride");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("DaySettingsOverride_Date_Property")),
				    new DisplayNameAttribute("Date")				);


				tableBuilder.AddCustomAttributes(t, "DaySettings",
					new DescriptionAttribute(SR.GetString("DaySettingsOverride_DaySettings_Property")),
				    new DisplayNameAttribute("DaySettings")				);


				tableBuilder.AddCustomAttributes(t, "Recurrence",
					new DescriptionAttribute(SR.GetString("DaySettingsOverride_Recurrence_Property")),
				    new DisplayNameAttribute("Recurrence")				);

				#endregion // DaySettingsOverride Properties

				#region CalendarGroup Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CalendarGroup");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Calendars",
					new DescriptionAttribute(SR.GetString("CalendarGroup_Calendars_Property")),
				    new DisplayNameAttribute("Calendars"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "InitialCalendarIds",
					new DescriptionAttribute(SR.GetString("CalendarGroup_InitialCalendarIds_Property")),
				    new DisplayNameAttribute("InitialCalendarIds")				);


				tableBuilder.AddCustomAttributes(t, "VisibleCalendars",
					new DescriptionAttribute(SR.GetString("CalendarGroup_VisibleCalendars_Property")),
				    new DisplayNameAttribute("VisibleCalendars"),
				    BrowsableAttribute.No				);


				tableBuilder.AddCustomAttributes(t, "InitialSelectedCalendarId",
					new DescriptionAttribute(SR.GetString("CalendarGroup_InitialSelectedCalendarId_Property")),
				    new DisplayNameAttribute("InitialSelectedCalendarId")				);


				tableBuilder.AddCustomAttributes(t, "SelectedCalendar",
					new DescriptionAttribute(SR.GetString("CalendarGroup_SelectedCalendar_Property")),
				    new DisplayNameAttribute("SelectedCalendar"),
				    BrowsableAttribute.No				);

				#endregion // CalendarGroup Properties

				#region VisibleDateCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.VisibleDateCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // VisibleDateCollection Properties

				#region CalendarGroupCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CalendarGroupCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarGroupCollection Properties

				#region XamScheduleView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamScheduleView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamScheduleViewAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamScheduleViewAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCalendarHeaderAreaResizing",
					new DescriptionAttribute(SR.GetString("XamScheduleView_AllowCalendarHeaderAreaResizing_Property")),
				    new DisplayNameAttribute("AllowCalendarHeaderAreaResizing"),
					new CategoryAttribute(SR.GetString("XamScheduleView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarHeaderAreaWidth",
					new DescriptionAttribute(SR.GetString("XamScheduleView_CalendarHeaderAreaWidth_Property")),
				    new DisplayNameAttribute("CalendarHeaderAreaWidth"),
					new CategoryAttribute(SR.GetString("XamScheduleView_Properties"))
				);

				#endregion // XamScheduleView Properties

				#region XamScheduleDataManager Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamScheduleDataManager");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamScheduleDataManagerAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamScheduleDataManagerAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroups",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_CalendarGroups_Property")),
				    new DisplayNameAttribute("CalendarGroups"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentUser",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_CurrentUser_Property")),
				    new DisplayNameAttribute("CurrentUser"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentUserId",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_CurrentUserId_Property")),
				    new DisplayNameAttribute("CurrentUserId"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataConnector",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_DataConnector_Property")),
				    new DisplayNameAttribute("DataConnector"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Settings",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_Settings_Property")),
				    new DisplayNameAttribute("Settings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColorScheme",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_ColorScheme_Property")),
				    new DisplayNameAttribute("ColorScheme"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ColorSchemeResolved",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_ColorSchemeResolved_Property")),
				    new DisplayNameAttribute("ColorSchemeResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DialogFactory",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_DialogFactory_Property")),
				    new DisplayNameAttribute("DialogFactory"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItems",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_ResourceItems_Property")),
				    new DisplayNameAttribute("ResourceItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateInfoProvider",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_DateInfoProvider_Property")),
				    new DisplayNameAttribute("DateInfoProvider"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateInfoProviderResolved",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_DateInfoProviderResolved_Property")),
				    new DisplayNameAttribute("DateInfoProviderResolved"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceCalculatorFactory",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_RecurrenceCalculatorFactory_Property")),
				    new DisplayNameAttribute("RecurrenceCalculatorFactory"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasPendingOperations",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_HasPendingOperations_Property")),
				    new DisplayNameAttribute("HasPendingOperations"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PendingOperations",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_PendingOperations_Property")),
				    new DisplayNameAttribute("PendingOperations"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveReminders",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_ActiveReminders_Property")),
				    new DisplayNameAttribute("ActiveReminders"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PromptForLocalTimeZone",
					new DescriptionAttribute(SR.GetString("XamScheduleDataManager_PromptForLocalTimeZone_Property")),
				    new DisplayNameAttribute("PromptForLocalTimeZone"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // XamScheduleDataManager Properties

				#region DataErrorInfo Properties
				t = controlAssembly.GetType("Infragistics.DataErrorInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ErrorList",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_ErrorList_Property")),
				    new DisplayNameAttribute("ErrorList")				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "Severity",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_Severity_Property")),
				    new DisplayNameAttribute("Severity")				);


				tableBuilder.AddCustomAttributes(t, "UserErrorText",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_UserErrorText_Property")),
				    new DisplayNameAttribute("UserErrorText")				);


				tableBuilder.AddCustomAttributes(t, "DiagnosticText",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_DiagnosticText_Property")),
				    new DisplayNameAttribute("DiagnosticText")				);


				tableBuilder.AddCustomAttributes(t, "Exception",
					new DescriptionAttribute(SR.GetString("DataErrorInfo_Exception_Property")),
				    new DisplayNameAttribute("Exception")				);

				#endregion // DataErrorInfo Properties

				#region Journal Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Journal");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("Journal_End_Property")),
				    new DisplayNameAttribute("End")				);


				tableBuilder.AddCustomAttributes(t, "ActivityType",
					new DescriptionAttribute(SR.GetString("Journal_ActivityType_Property")),
				    new DisplayNameAttribute("ActivityType")				);


				tableBuilder.AddCustomAttributes(t, "EndTimeZoneId",
					new DescriptionAttribute(SR.GetString("Journal_EndTimeZoneId_Property")),
				    new DisplayNameAttribute("EndTimeZoneId")				);

				#endregion // Journal Properties

				#region XamDayView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamDayView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDayViewAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDayViewAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiDayActivityAreaVisibility",
					new DescriptionAttribute(SR.GetString("XamDayView_MultiDayActivityAreaVisibility_Property")),
				    new DisplayNameAttribute("MultiDayActivityAreaVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMultiDayActivityAreaResizing",
					new DescriptionAttribute(SR.GetString("XamDayView_AllowMultiDayActivityAreaResizing_Property")),
				    new DisplayNameAttribute("AllowMultiDayActivityAreaResizing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiDayActivityAreaHeight",
					new DescriptionAttribute(SR.GetString("XamDayView_MultiDayActivityAreaHeight_Property")),
				    new DisplayNameAttribute("MultiDayActivityAreaHeight"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeslotGutterAreaWidth",
					new DescriptionAttribute(SR.GetString("XamDayView_TimeslotGutterAreaWidth_Property")),
				    new DisplayNameAttribute("TimeslotGutterAreaWidth"),
					new CategoryAttribute(SR.GetString("XamDayView_Properties"))
				);

				#endregion // XamDayView Properties

				#region TimeslotHeaderArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotHeaderArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeslotHeaderArea Properties

				#region ScheduleDialogFactoryBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleDialogFactoryBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SupportedActivityDialogTypes",
					new DescriptionAttribute(SR.GetString("ScheduleDialogFactoryBase_SupportedActivityDialogTypes_Property")),
				    new DisplayNameAttribute("SupportedActivityDialogTypes")				);

				#endregion // ScheduleDialogFactoryBase Properties

				#region DayViewTimeslotHeaderArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayViewTimeslotHeaderArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedMargin",
					new DescriptionAttribute(SR.GetString("DayViewTimeslotHeaderArea_ComputedMargin_Property")),
				    new DisplayNameAttribute("ComputedMargin")				);

				#endregion // DayViewTimeslotHeaderArea Properties

				#region CalendarGroupPresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarGroupPresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroup",
					new DescriptionAttribute(SR.GetString("CalendarGroupPresenterBase_CalendarGroup_Property")),
				    new DisplayNameAttribute("CalendarGroup"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("CalendarGroupPresenterBase_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderThickness",
					new DescriptionAttribute(SR.GetString("CalendarGroupPresenterBase_ComputedBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedMargin",
					new DescriptionAttribute(SR.GetString("CalendarGroupPresenterBase_ComputedMargin_Property")),
				    new DisplayNameAttribute("ComputedMargin"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // CalendarGroupPresenterBase Properties

				#region ResourceCalendarCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceCalendarCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllocatedItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_AllocatedItems_Property")),
				    new DisplayNameAttribute("AllocatedItems")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("ViewList`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ViewList`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "SourceItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_SourceItems_Property")),
				    new DisplayNameAttribute("SourceItems")				);

				#endregion // ResourceCalendarCollection Properties

				#region ViewList`1 Properties
				t = controlAssembly.GetType("Infragistics.Collections.ViewList`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllocatedItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_AllocatedItems_Property")),
				    new DisplayNameAttribute("AllocatedItems")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("ViewList`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ViewList`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "SourceItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_SourceItems_Property")),
				    new DisplayNameAttribute("SourceItems")				);

				#endregion // ViewList`1 Properties

				#region ScheduleActivityPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleActivityPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleActivityPanel Properties

				#region TimeslotPanelBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotPanelBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterItemSpacing",
					new DescriptionAttribute(SR.GetString("TimeslotPanelBase_InterItemSpacing_Property")),
				    new DisplayNameAttribute("InterItemSpacing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotPanelBase Properties

				#region ResourceCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllocatedItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_AllocatedItems_Property")),
				    new DisplayNameAttribute("AllocatedItems")				);


				tableBuilder.AddCustomAttributes(t, "Count",
					new DescriptionAttribute(SR.GetString("ViewList`1_Count_Property")),
				    new DisplayNameAttribute("Count")				);


				tableBuilder.AddCustomAttributes(t, "IsReadOnly",
					new DescriptionAttribute(SR.GetString("ViewList`1_IsReadOnly_Property")),
				    new DisplayNameAttribute("IsReadOnly")				);


				tableBuilder.AddCustomAttributes(t, "SourceItems",
					new DescriptionAttribute(SR.GetString("ViewList`1_SourceItems_Property")),
				    new DisplayNameAttribute("SourceItems")				);

				#endregion // ResourceCollection Properties

				#region TimeslotPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeslotPanel Properties

				#region ScheduleTabPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleTabPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterTabSpacing",
					new DescriptionAttribute(SR.GetString("ScheduleTabPanel_InterTabSpacing_Property")),
				    new DisplayNameAttribute("InterTabSpacing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleTabPanel Properties

				#region ScheduleItemsPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleItemsPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleItemsPanel Properties

				#region CalendarGroupBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CalendarGroupBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "VisibleCalendars",
					new DescriptionAttribute(SR.GetString("CalendarGroupBase_VisibleCalendars_Property")),
				    new DisplayNameAttribute("VisibleCalendars")				);


				tableBuilder.AddCustomAttributes(t, "SelectedCalendar",
					new DescriptionAttribute(SR.GetString("CalendarGroupBase_SelectedCalendar_Property")),
				    new DisplayNameAttribute("SelectedCalendar")				);

				#endregion // CalendarGroupBase Properties

				#region DayHeaderBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayHeaderBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderThickness",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_ComputedBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateTime",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_DateTime_Property")),
				    new DisplayNameAttribute("DateTime"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsToday",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_IsToday_Property")),
				    new DisplayNameAttribute("IsToday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("DayHeaderBase_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // DayHeaderBase Properties

				#region ActivityPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_Activity_Property")),
				    new DisplayNameAttribute("Activity"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsInEditMode",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_IsInEditMode_Property")),
				    new DisplayNameAttribute("IsInEditMode"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSingleLineDisplay",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_IsSingleLineDisplay_Property")),
				    new DisplayNameAttribute("IsSingleLineDisplay"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderStrokeThickness",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedBorderStrokeThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderStrokeThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedContentMargin",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedContentMargin_Property")),
				    new DisplayNameAttribute("ComputedContentMargin"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedGeometry",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedGeometry_Property")),
				    new DisplayNameAttribute("ComputedGeometry"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedContentHorizontalAlignment",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedContentHorizontalAlignment_Property")),
				    new DisplayNameAttribute("ComputedContentHorizontalAlignment"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EndLocal",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_EndLocal_Property")),
				    new DisplayNameAttribute("EndLocal"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StartLocal",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_StartLocal_Property")),
				    new DisplayNameAttribute("StartLocal"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PrefixFormatType",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_PrefixFormatType_Property")),
				    new DisplayNameAttribute("PrefixFormatType"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SuffixFormatType",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_SuffixFormatType_Property")),
				    new DisplayNameAttribute("SuffixFormatType"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedDateTimeForeground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedDateTimeForeground_Property")),
				    new DisplayNameAttribute("ComputedDateTimeForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EndOutOfRangeIndicatorVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_EndOutOfRangeIndicatorVisibility_Property")),
				    new DisplayNameAttribute("EndOutOfRangeIndicatorVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StartOutOfRangeIndicatorVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_StartOutOfRangeIndicatorVisibility_Property")),
				    new DisplayNameAttribute("StartOutOfRangeIndicatorVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasPendingOperation",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_HasPendingOperation_Property")),
				    new DisplayNameAttribute("HasPendingOperation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IndicatorAreaVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_IndicatorAreaVisibility_Property")),
				    new DisplayNameAttribute("IndicatorAreaVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AdditionalText",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_AdditionalText_Property")),
				    new DisplayNameAttribute("AdditionalText"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AdditionalTextVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_AdditionalTextVisibility_Property")),
				    new DisplayNameAttribute("AdditionalTextVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SeparatorVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_SeparatorVisibility_Property")),
				    new DisplayNameAttribute("SeparatorVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ToolTipInfo",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ToolTipInfo_Property")),
				    new DisplayNameAttribute("ToolTipInfo"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReminderVisibility",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ReminderVisibility_Property")),
				    new DisplayNameAttribute("ReminderVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Categories",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_Categories_Property")),
				    new DisplayNameAttribute("Categories"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedIndicatorForeground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_ComputedIndicatorForeground_Property")),
				    new DisplayNameAttribute("ComputedIndicatorForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OutOfRangeIndicatorForeground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_OutOfRangeIndicatorForeground_Property")),
				    new DisplayNameAttribute("OutOfRangeIndicatorForeground")				);


				tableBuilder.AddCustomAttributes(t, "OutOfRangeIndicatorBackground",
					new DescriptionAttribute(SR.GetString("ActivityPresenter_OutOfRangeIndicatorBackground_Property")),
				    new DisplayNameAttribute("OutOfRangeIndicatorBackground")				);

				#endregion // ActivityPresenter Properties

				#region PropertySerializationInfo Properties
				t = controlAssembly.GetType("Infragistics.PropertySerializationInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Type",
					new DescriptionAttribute(SR.GetString("PropertySerializationInfo_Type_Property")),
				    new DisplayNameAttribute("Type")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("PropertySerializationInfo_Name_Property")),
				    new DisplayNameAttribute("Name")				);

				#endregion // PropertySerializationInfo Properties

				#region ObjectSerializationInfo Properties
				t = controlAssembly.GetType("Infragistics.ObjectSerializationInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SerializedProperties",
					new DescriptionAttribute(SR.GetString("ObjectSerializationInfo_SerializedProperties_Property")),
				    new DisplayNameAttribute("SerializedProperties")				);

				#endregion // ObjectSerializationInfo Properties

				#region DayViewDayHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayViewDayHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DayViewDayHeader Properties

				#region OfficeColorSchemeBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.OfficeColorSchemeBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BaseColors",
					new DescriptionAttribute(SR.GetString("OfficeColorSchemeBase_BaseColors_Property")),
				    new DisplayNameAttribute("BaseColors")				);


				tableBuilder.AddCustomAttributes(t, "OfficeColorScheme",
					new DescriptionAttribute(SR.GetString("OfficeColorSchemeBase_OfficeColorScheme_Property")),
				    new DisplayNameAttribute("OfficeColorScheme")				);


				tableBuilder.AddCustomAttributes(t, "DialogResources",
					new DescriptionAttribute(SR.GetString("OfficeColorSchemeBase_DialogResources_Property")),
				    new DisplayNameAttribute("DialogResources")				);

				#endregion // OfficeColorSchemeBase Properties

				#region CalendarColorScheme Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CalendarColorScheme");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BaseColors",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_BaseColors_Property")),
				    new DisplayNameAttribute("BaseColors")				);


				tableBuilder.AddCustomAttributes(t, "BrushVersion",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_BrushVersion_Property")),
				    new DisplayNameAttribute("BrushVersion")				);


				tableBuilder.AddCustomAttributes(t, "DefaultBrushProvider",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_DefaultBrushProvider_Property")),
				    new DisplayNameAttribute("DefaultBrushProvider")				);


				tableBuilder.AddCustomAttributes(t, "IsHighContrast",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_IsHighContrast_Property")),
				    new DisplayNameAttribute("IsHighContrast")				);


				tableBuilder.AddCustomAttributes(t, "DialogResources",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_DialogResources_Property")),
				    new DisplayNameAttribute("DialogResources")				);


				tableBuilder.AddCustomAttributes(t, "DateNavigatorResourceProvider",
					new DescriptionAttribute(SR.GetString("CalendarColorScheme_DateNavigatorResourceProvider_Property")),
				    new DisplayNameAttribute("DateNavigatorResourceProvider")				);

				#endregion // CalendarColorScheme Properties

				#region Office2007ColorScheme Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Office2007ColorScheme");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateNavigatorResourceProvider",
					new DescriptionAttribute(SR.GetString("Office2007ColorScheme_DateNavigatorResourceProvider_Property")),
				    new DisplayNameAttribute("DateNavigatorResourceProvider")				);

				#endregion // Office2007ColorScheme Properties

				#region Office2010ColorScheme Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Office2010ColorScheme");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateNavigatorResourceProvider",
					new DescriptionAttribute(SR.GetString("Office2010ColorScheme_DateNavigatorResourceProvider_Property")),
				    new DisplayNameAttribute("DateNavigatorResourceProvider")				);

				#endregion // Office2010ColorScheme Properties

				#region CurrentTimeIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CurrentTimeIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("CurrentTimeIndicator_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground")				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("CurrentTimeIndicator_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush")				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("CurrentTimeIndicator_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // CurrentTimeIndicator Properties

				#region TimeslotGroupPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotGroupPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeslotGroupPanel Properties

				#region AppointmentDialogCore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.AppointmentDialogCore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationControlSite",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_NavigationControlSite_Property")),
				    new DisplayNameAttribute("NavigationControlSite"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "NavigationControlSiteContent",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_NavigationControlSiteContent_Property")),
				    new DisplayNameAttribute("NavigationControlSiteContent"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowAllDayActivities",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_AllowAllDayActivities_Property")),
				    new DisplayNameAttribute("AllowAllDayActivities"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Appointment",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_Appointment_Property")),
				    new DisplayNameAttribute("Appointment"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ConflictMessage",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_ConflictMessage_Property")),
				    new DisplayNameAttribute("ConflictMessage"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HasConflicts",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_HasConflicts_Property")),
				    new DisplayNameAttribute("HasConflicts"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeZonePickerVisibility",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_TimeZonePickerVisibility_Property")),
				    new DisplayNameAttribute("TimeZonePickerVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsOccurrence",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_IsOccurrence_Property")),
				    new DisplayNameAttribute("IsOccurrence"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsRecurrenceRoot",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_IsRecurrenceRoot_Property")),
				    new DisplayNameAttribute("IsRecurrenceRoot"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceDescription",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_OccurrenceDescription_Property")),
				    new DisplayNameAttribute("OccurrenceDescription"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_OccurrenceDescriptionVisibility_Property")),
				    new DisplayNameAttribute("OccurrenceDescriptionVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceRootDescription",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_RecurrenceRootDescription_Property")),
				    new DisplayNameAttribute("RecurrenceRootDescription"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceRootDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_RecurrenceRootDescriptionVisibility_Property")),
				    new DisplayNameAttribute("RecurrenceRootDescriptionVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeZonePickerEnabled",
					new DescriptionAttribute(SR.GetString("AppointmentDialogCore_TimeZonePickerEnabled_Property")),
				    new DisplayNameAttribute("TimeZonePickerEnabled"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // AppointmentDialogCore Properties

				#region TaskPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TaskPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TaskPresenter Properties

				#region JournalPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.JournalPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // JournalPresenter Properties

				#region CalendarBrushProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarBrushProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BaseColor",
					new DescriptionAttribute(SR.GetString("CalendarBrushProvider_BaseColor_Property")),
				    new DisplayNameAttribute("BaseColor")				);


				tableBuilder.AddCustomAttributes(t, "BrushVersion",
					new DescriptionAttribute(SR.GetString("CalendarBrushProvider_BrushVersion_Property")),
				    new DisplayNameAttribute("BrushVersion")				);

				#endregion // CalendarBrushProvider Properties

				#region ScheduleStackPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleStackPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterItemSpacing",
					new DescriptionAttribute(SR.GetString("ScheduleStackPanel_InterItemSpacing_Property")),
				    new DisplayNameAttribute("InterItemSpacing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleStackPanel Properties

				#region GridBagPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Layouts.GridBagPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // GridBagPanel Properties

				#region AppointmentPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.AppointmentPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SeparatorVisibility",
					new DescriptionAttribute(SR.GetString("AppointmentPresenter_SeparatorVisibility_Property")),
				    new DisplayNameAttribute("SeparatorVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // AppointmentPresenter Properties

				#region TimeslotHeaderTickmark Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotHeaderTickmark");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Kind",
					new DescriptionAttribute(SR.GetString("TimeslotHeaderTickmark_Kind_Property")),
				    new DisplayNameAttribute("Kind"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("TimeslotHeaderTickmark_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotHeaderTickmark Properties

				#region ActivityEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityEventArgs_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);

				#endregion // ActivityEventArgs Properties

				#region CancellableActivityEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CancellableActivityEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("CancellableActivityEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);

				#endregion // CancellableActivityEventArgs Properties

				#region ActivityAddedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityAddedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityAddedEventArgs Properties

				#region ActivityAddingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityAddingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityAddingEventArgs Properties

				#region ActivityChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityChangedEventArgs Properties

				#region ActivityChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OriginalActivityData",
					new DescriptionAttribute(SR.GetString("ActivityChangingEventArgs_OriginalActivityData_Property")),
				    new DisplayNameAttribute("OriginalActivityData")				);

				#endregion // ActivityChangingEventArgs Properties

				#region ActivityDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowModifications",
					new DescriptionAttribute(SR.GetString("ActivityDialogDisplayingEventArgs_AllowModifications_Property")),
				    new DisplayNameAttribute("AllowModifications")				);

				#endregion // ActivityDialogDisplayingEventArgs Properties

				#region ActivityRemovedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityRemovedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityRemovedEventArgs Properties

				#region ActivityRemovingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityRemovingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityRemovingEventArgs Properties

				#region ScheduleViewTimeslotHeaderArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleViewTimeslotHeaderArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleViewTimeslotHeaderArea Properties

				#region ActivityOperationResult Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityOperationResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityOperationResult_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemOperationResult`1_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ActivityOperationResult Properties

				#region ActivityPresenterAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ActivityPresenterAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityPresenterAutomationPeer Properties

				#region CalendarHeaderArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("CalendarHeaderArea_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // CalendarHeaderArea Properties

				#region CalendarHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Calendar",
					new DescriptionAttribute(SR.GetString("CalendarHeader_Calendar_Property")),
				    new DisplayNameAttribute("Calendar"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("CalendarHeader_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("CalendarHeader_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("CalendarHeader_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CalendarHeader_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("CalendarHeader_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CanClose",
					new DescriptionAttribute(SR.GetString("CalendarHeader_CanClose_Property")),
				    new DisplayNameAttribute("CanClose"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CloseButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarHeader_CloseButtonVisibility_Property")),
				    new DisplayNameAttribute("CloseButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsInOverlayMode",
					new DescriptionAttribute(SR.GetString("CalendarHeader_IsInOverlayMode_Property")),
				    new DisplayNameAttribute("IsInOverlayMode"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OverlayButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarHeader_OverlayButtonVisibility_Property")),
				    new DisplayNameAttribute("OverlayButtonVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("CalendarHeader_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Header",
					new DescriptionAttribute(SR.GetString("CalendarHeader_Header_Property")),
				    new DisplayNameAttribute("Header"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsCurrentUser",
					new DescriptionAttribute(SR.GetString("CalendarHeader_IsCurrentUser_Property")),
				    new DisplayNameAttribute("IsCurrentUser"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // CalendarHeader Properties

				#region ScheduleViewDayHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleViewDayHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ScheduleViewDayHeader_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("ScheduleViewDayHeader_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("ScheduleViewDayHeader_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("ScheduleViewDayHeader_Date_Property")),
				    new DisplayNameAttribute("Date"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleViewDayHeader Properties

				#region TimeslotHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeadingTickmarkKind",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_LeadingTickmarkKind_Property")),
				    new DisplayNameAttribute("LeadingTickmarkKind"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeadingTickmarkVisibility",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_LeadingTickmarkVisibility_Property")),
				    new DisplayNameAttribute("LeadingTickmarkVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TickmarkBrush",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_TickmarkBrush_Property")),
				    new DisplayNameAttribute("TickmarkBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrailingTickmarkKind",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_TrailingTickmarkKind_Property")),
				    new DisplayNameAttribute("TrailingTickmarkKind"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TrailingTickmarkVisibility",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_TrailingTickmarkVisibility_Property")),
				    new DisplayNameAttribute("TrailingTickmarkVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowAMPMDesignator",
					new DescriptionAttribute(SR.GetString("TimeslotHeader_ShowAMPMDesignator_Property")),
				    new DisplayNameAttribute("ShowAMPMDesignator"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotHeader Properties

				#region ScheduleViewTimeslotHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleViewTimeslotHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleViewTimeslotHeader Properties

				#region DayViewTimeslotHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayViewTimeslotHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DayViewTimeslotHeader Properties

				#region CalendarHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderAutomationPeer Properties

				#region CalendarGroupTimeslotArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarGroupTimeslotArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarGroupTimeslotArea Properties

				#region CalendarHeaderAreaAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarHeaderAreaAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderAreaAutomationPeer Properties

				#region TimeslotArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("TimeslotArea_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderThickness",
					new DescriptionAttribute(SR.GetString("TimeslotArea_ComputedBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotArea Properties

				#region DayViewDayHeaderArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayViewDayHeaderArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MultiDayActivityAreaVisibility",
					new DescriptionAttribute(SR.GetString("DayViewDayHeaderArea_MultiDayActivityAreaVisibility_Property")),
				    new DisplayNameAttribute("MultiDayActivityAreaVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // DayViewDayHeaderArea Properties

				#region CalendarGroupTimeslotAreaAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarGroupTimeslotAreaAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarGroupTimeslotAreaAutomationPeer Properties

				#region DayViewDayHeaderAreaAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.DayViewDayHeaderAreaAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DayViewDayHeaderAreaAutomationPeer Properties

				#region DayViewDayHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.DayViewDayHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DayViewDayHeaderAutomationPeer Properties

				#region ScheduleViewDayHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ScheduleViewDayHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleViewDayHeaderAutomationPeer Properties

				#region TimeslotAreaAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.TimeslotAreaAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeslotAreaAutomationPeer Properties

				#region TimeslotHeaderAreaAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.TimeslotHeaderAreaAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeslotHeaderAreaAutomationPeer Properties

				#region XamDayViewAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamDayViewAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamDayViewAutomationPeer Properties

				#region XamScheduleViewAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamScheduleViewAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamScheduleViewAutomationPeer Properties

				#region CalendarHeaderCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderCommandBase Properties

				#region CalendarHeaderCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CalendarHeaderCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CalendarHeaderCommandSource Properties

				#region CalendarHeaderCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderCloseCommand Properties

				#region ActivityResizerBarAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ActivityResizerBarAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityResizerBarAutomationPeer Properties

				#region TimeRangePresenterBaseAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.TimeRangePresenterBaseAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TimeRangePresenterBaseAutomationPeer Properties

				#region ActivityResizerBar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityResizerBar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsLeading",
					new DescriptionAttribute(SR.GetString("ActivityResizerBar_IsLeading_Property")),
				    new DisplayNameAttribute("IsLeading"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResizeGripVisibility",
					new DescriptionAttribute(SR.GetString("ActivityResizerBar_ResizeGripVisibility_Property")),
				    new DisplayNameAttribute("ResizeGripVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ActivityResizerBar Properties

				#region TimeRangePresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeRangePresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("TimeRangePresenterBase_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("TimeRangePresenterBase_End_Property")),
				    new DisplayNameAttribute("End"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("TimeRangePresenterBase_Orientation_Property")),
				    new DisplayNameAttribute("Orientation"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("TimeRangePresenterBase_Start_Property")),
				    new DisplayNameAttribute("Start"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeRangePresenterBase Properties

				#region TimePickerItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimePickerItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayString",
					new DescriptionAttribute(SR.GetString("TimePickerItem_DisplayString_Property")),
				    new DisplayNameAttribute("DisplayString")				);

				#endregion // TimePickerItem Properties

				#region MoreActivityIndicatorAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.MoreActivityIndicatorAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MoreActivityIndicatorAutomationPeer Properties

				#region MoreActivityIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MoreActivityIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedFill",
					new DescriptionAttribute(SR.GetString("MoreActivityIndicator_ComputedFill_Property")),
				    new DisplayNameAttribute("ComputedFill"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedStroke",
					new DescriptionAttribute(SR.GetString("MoreActivityIndicator_ComputedStroke_Property")),
				    new DisplayNameAttribute("ComputedStroke"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MoreActivityIndicator Properties

				#region TimeslotHeaderTimePresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeslotHeaderTimePresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Foreground",
					new DescriptionAttribute(SR.GetString("TimeslotHeaderTimePresenter_Foreground_Property")),
				    new DisplayNameAttribute("Foreground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // TimeslotHeaderTimePresenter Properties

				#region DateInfoProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DateInfoProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AMDesignator",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_AMDesignator_Property")),
				    new DisplayNameAttribute("AMDesignator")				);


				tableBuilder.AddCustomAttributes(t, "AMDesignatorLowercase",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_AMDesignatorLowercase_Property")),
				    new DisplayNameAttribute("AMDesignatorLowercase")				);


				tableBuilder.AddCustomAttributes(t, "DateSeparator",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_DateSeparator_Property")),
				    new DisplayNameAttribute("DateSeparator")				);


				tableBuilder.AddCustomAttributes(t, "DateTimeFormatInfo",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_DateTimeFormatInfo_Property")),
				    new DisplayNameAttribute("DateTimeFormatInfo")				);


				tableBuilder.AddCustomAttributes(t, "DisplayTimeIn24HourFormat",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_DisplayTimeIn24HourFormat_Property")),
				    new DisplayNameAttribute("DisplayTimeIn24HourFormat")				);


				tableBuilder.AddCustomAttributes(t, "HourPattern",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_HourPattern_Property")),
				    new DisplayNameAttribute("HourPattern")				);


				tableBuilder.AddCustomAttributes(t, "MaxSupportedDateTime",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_MaxSupportedDateTime_Property")),
				    new DisplayNameAttribute("MaxSupportedDateTime")				);


				tableBuilder.AddCustomAttributes(t, "MinSupportedDateTime",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_MinSupportedDateTime_Property")),
				    new DisplayNameAttribute("MinSupportedDateTime")				);


				tableBuilder.AddCustomAttributes(t, "PMDesignator",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_PMDesignator_Property")),
				    new DisplayNameAttribute("PMDesignator")				);


				tableBuilder.AddCustomAttributes(t, "PMDesignatorLowercase",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_PMDesignatorLowercase_Property")),
				    new DisplayNameAttribute("PMDesignatorLowercase")				);


				tableBuilder.AddCustomAttributes(t, "TimeSeparator",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_TimeSeparator_Property")),
				    new DisplayNameAttribute("TimeSeparator")				);


				tableBuilder.AddCustomAttributes(t, "Calendar",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_Calendar_Property")),
				    new DisplayNameAttribute("Calendar")				);


				tableBuilder.AddCustomAttributes(t, "CurrentProvider",
					new DescriptionAttribute(SR.GetString("DateInfoProvider_CurrentProvider_Property")),
				    new DisplayNameAttribute("CurrentProvider")				);

				#endregion // DateInfoProvider Properties

				#region ScheduleDateTimePresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDateTimePresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DateTime",
					new DescriptionAttribute(SR.GetString("ScheduleDateTimePresenter_DateTime_Property")),
				    new DisplayNameAttribute("DateTime"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FormatType",
					new DescriptionAttribute(SR.GetString("ScheduleDateTimePresenter_FormatType_Property")),
				    new DisplayNameAttribute("FormatType"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ConvertDateTimeToLocal",
					new DescriptionAttribute(SR.GetString("ScheduleDateTimePresenter_ConvertDateTimeToLocal_Property")),
				    new DisplayNameAttribute("ConvertDateTimeToLocal"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleDateTimePresenter Properties

				#region ScheduleDatePresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDatePresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FormattedText",
					new DescriptionAttribute(SR.GetString("ScheduleDatePresenterBase_FormattedText_Property")),
				    new DisplayNameAttribute("FormattedText"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleDatePresenterBase Properties

				#region ScheduleDateRangePresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDateRangePresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("ScheduleDateRangePresenter_End_Property")),
				    new DisplayNameAttribute("End"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("ScheduleDateRangePresenter_Start_Property")),
				    new DisplayNameAttribute("Start"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ConvertDateTimeToLocal",
					new DescriptionAttribute(SR.GetString("ScheduleDateRangePresenter_ConvertDateTimeToLocal_Property")),
				    new DisplayNameAttribute("ConvertDateTimeToLocal"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FormatType",
					new DescriptionAttribute(SR.GetString("ScheduleDateRangePresenter_FormatType_Property")),
				    new DisplayNameAttribute("FormatType"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ScheduleDateRangePresenter Properties

				#region BestFitPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Primitives.BestFitPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Criteria",
					new DescriptionAttribute(SR.GetString("BestFitPanel_Criteria_Property")),
				    new DisplayNameAttribute("Criteria"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowPartial",
					new DescriptionAttribute(SR.GetString("BestFitPanel_ShowPartial_Property")),
				    new DisplayNameAttribute("ShowPartial"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // BestFitPanel Properties

				#region CalendarHeaderToggleOverlayModeCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderToggleOverlayModeCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderToggleOverlayModeCommand Properties

				#region CalendarHeaderHorizontal Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderHorizontal");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderHorizontal Properties

				#region CalendarHeaderVertical Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarHeaderVertical");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarHeaderVertical Properties

				#region ScheduleResourceString Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleResourceString");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleResourceString Properties

				#region RecurrenceBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.RecurrenceBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceBase Properties

				#region MonthOfYearRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.MonthOfYearRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Month",
					new DescriptionAttribute(SR.GetString("MonthOfYearRecurrenceRule_Month_Property")),
				    new DisplayNameAttribute("Month")				);

				#endregion // MonthOfYearRecurrenceRule Properties

				#region WeekOfYearRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.WeekOfYearRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "WeekNumber",
					new DescriptionAttribute(SR.GetString("WeekOfYearRecurrenceRule_WeekNumber_Property")),
				    new DisplayNameAttribute("WeekNumber")				);

				#endregion // WeekOfYearRecurrenceRule Properties

				#region DayOfYearRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DayOfYearRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfYear",
					new DescriptionAttribute(SR.GetString("DayOfYearRecurrenceRule_DayOfYear_Property")),
				    new DisplayNameAttribute("DayOfYear")				);

				#endregion // DayOfYearRecurrenceRule Properties

				#region DayOfMonthRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DayOfMonthRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfMonth",
					new DescriptionAttribute(SR.GetString("DayOfMonthRecurrenceRule_DayOfMonth_Property")),
				    new DisplayNameAttribute("DayOfMonth")				);

				#endregion // DayOfMonthRecurrenceRule Properties

				#region DayOfWeekRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DayOfWeekRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Day",
					new DescriptionAttribute(SR.GetString("DayOfWeekRecurrenceRule_Day_Property")),
				    new DisplayNameAttribute("Day")				);


				tableBuilder.AddCustomAttributes(t, "RelativePosition",
					new DescriptionAttribute(SR.GetString("DayOfWeekRecurrenceRule_RelativePosition_Property")),
				    new DisplayNameAttribute("RelativePosition")				);

				#endregion // DayOfWeekRecurrenceRule Properties

				#region HourRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.HourRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Hour",
					new DescriptionAttribute(SR.GetString("HourRecurrenceRule_Hour_Property")),
				    new DisplayNameAttribute("Hour")				);

				#endregion // HourRecurrenceRule Properties

				#region MinuteRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.MinuteRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Minute",
					new DescriptionAttribute(SR.GetString("MinuteRecurrenceRule_Minute_Property")),
				    new DisplayNameAttribute("Minute")				);

				#endregion // MinuteRecurrenceRule Properties

				#region SecondRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.SecondRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Second",
					new DescriptionAttribute(SR.GetString("SecondRecurrenceRule_Second_Property")),
				    new DisplayNameAttribute("Second")				);

				#endregion // SecondRecurrenceRule Properties

				#region SubsetRecurrenceRule Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.SubsetRecurrenceRule");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceInstance",
					new DescriptionAttribute(SR.GetString("SubsetRecurrenceRule_OccurrenceInstance_Property")),
				    new DisplayNameAttribute("OccurrenceInstance")				);

				#endregion // SubsetRecurrenceRule Properties

				#region ClickToAddActivityElement Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ClickToAddActivityElement");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Prompt",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_Prompt_Property")),
				    new DisplayNameAttribute("Prompt"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSingleLineDisplay",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_IsSingleLineDisplay_Property")),
				    new DisplayNameAttribute("IsSingleLineDisplay"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityTypes",
					new DescriptionAttribute(SR.GetString("ClickToAddActivityElement_ActivityTypes_Property")),
				    new DisplayNameAttribute("ActivityTypes"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ClickToAddActivityElement Properties

				#region MultiDayActivityArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MultiDayActivityArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("MultiDayActivityArea_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderThickness",
					new DescriptionAttribute(SR.GetString("MultiDayActivityArea_ComputedBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsToday",
					new DescriptionAttribute(SR.GetString("MultiDayActivityArea_IsToday_Property")),
				    new DisplayNameAttribute("IsToday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MultiDayActivityArea Properties

				#region RecurrenceDialogCoreCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RecurrenceDialogCoreCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceDialogCoreCommandBase Properties

				#region RecurrenceDialogCoreCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RecurrenceDialogCoreCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("RecurrenceDialogCoreCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // RecurrenceDialogCoreCommandSource Properties

				#region RecurrenceDialogCoreSaveAndCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RecurrenceDialogCoreSaveAndCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceDialogCoreSaveAndCloseCommand Properties

				#region RecurrenceDialogCoreCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RecurrenceDialogCoreCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceDialogCoreCloseCommand Properties

				#region RecurrenceInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.RecurrenceInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Recurrence",
					new DescriptionAttribute(SR.GetString("RecurrenceInfo_Recurrence_Property")),
				    new DisplayNameAttribute("Recurrence")				);


				tableBuilder.AddCustomAttributes(t, "StartDateTime",
					new DescriptionAttribute(SR.GetString("RecurrenceInfo_StartDateTime_Property")),
				    new DisplayNameAttribute("StartDateTime")				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceDuration",
					new DescriptionAttribute(SR.GetString("RecurrenceInfo_OccurrenceDuration_Property")),
				    new DisplayNameAttribute("OccurrenceDuration")				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("RecurrenceInfo_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "TimeZone",
					new DescriptionAttribute(SR.GetString("RecurrenceInfo_TimeZone_Property")),
				    new DisplayNameAttribute("TimeZone")				);

				#endregion // RecurrenceInfo Properties

				#region RecurrenceCalculatorFactoryBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.RecurrenceCalculatorFactoryBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceCalculatorFactoryBase Properties

				#region TimeZoneToken Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.TimeZoneToken");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Id",
					new DescriptionAttribute(SR.GetString("TimeZoneToken_Id_Property")),
				    new DisplayNameAttribute("Id")				);


				tableBuilder.AddCustomAttributes(t, "DisplayName",
					new DescriptionAttribute(SR.GetString("TimeZoneToken_DisplayName_Property")),
				    new DisplayNameAttribute("DisplayName")				);


				tableBuilder.AddCustomAttributes(t, "Provider",
					new DescriptionAttribute(SR.GetString("TimeZoneToken_Provider_Property")),
				    new DisplayNameAttribute("Provider")				);

				#endregion // TimeZoneToken Properties

				#region TimeZoneInfoProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.TimeZoneInfoProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalTimeZoneId",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_LocalTimeZoneId_Property")),
				    new DisplayNameAttribute("LocalTimeZoneId")				);


				tableBuilder.AddCustomAttributes(t, "LocalTimeZoneIdResolved",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_LocalTimeZoneIdResolved_Property")),
				    new DisplayNameAttribute("LocalTimeZoneIdResolved")				);


				tableBuilder.AddCustomAttributes(t, "LocalToken",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_LocalToken_Property")),
				    new DisplayNameAttribute("LocalToken")				);


				tableBuilder.AddCustomAttributes(t, "TimeZoneTokens",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_TimeZoneTokens_Property")),
				    new DisplayNameAttribute("TimeZoneTokens")				);


				tableBuilder.AddCustomAttributes(t, "UtcToken",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_UtcToken_Property")),
				    new DisplayNameAttribute("UtcToken")				);


				tableBuilder.AddCustomAttributes(t, "Version",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_Version_Property")),
				    new DisplayNameAttribute("Version")				);


				tableBuilder.AddCustomAttributes(t, "DefaultProvider",
					new DescriptionAttribute(SR.GetString("TimeZoneInfoProvider_DefaultProvider_Property")),
				    new DisplayNameAttribute("DefaultProvider")				);

				#endregion // TimeZoneInfoProvider Properties

				#region OSTimeZoneInfoProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.OSTimeZoneInfoProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalTimeZoneIdResolved",
					new DescriptionAttribute(SR.GetString("OSTimeZoneInfoProvider_LocalTimeZoneIdResolved_Property")),
				    new DisplayNameAttribute("LocalTimeZoneIdResolved")				);


				tableBuilder.AddCustomAttributes(t, "UtcToken",
					new DescriptionAttribute(SR.GetString("OSTimeZoneInfoProvider_UtcToken_Property")),
				    new DisplayNameAttribute("UtcToken")				);


				tableBuilder.AddCustomAttributes(t, "ExportIdMap",
					new DescriptionAttribute(SR.GetString("OSTimeZoneInfoProvider_ExportIdMap_Property")),
				    new DisplayNameAttribute("ExportIdMap")				);


				tableBuilder.AddCustomAttributes(t, "UseOsDisplayNames",
					new DescriptionAttribute(SR.GetString("OSTimeZoneInfoProvider_UseOsDisplayNames_Property")),
				    new DisplayNameAttribute("UseOsDisplayNames")				);

				#endregion // OSTimeZoneInfoProvider Properties

				#region CustomTimeZoneInfoProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CustomTimeZoneInfoProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalTimeZoneIdResolved",
					new DescriptionAttribute(SR.GetString("CustomTimeZoneInfoProvider_LocalTimeZoneIdResolved_Property")),
				    new DisplayNameAttribute("LocalTimeZoneIdResolved")				);


				tableBuilder.AddCustomAttributes(t, "UtcToken",
					new DescriptionAttribute(SR.GetString("CustomTimeZoneInfoProvider_UtcToken_Property")),
				    new DisplayNameAttribute("UtcToken")				);


				tableBuilder.AddCustomAttributes(t, "UtcTimeZoneId",
					new DescriptionAttribute(SR.GetString("CustomTimeZoneInfoProvider_UtcTimeZoneId_Property")),
				    new DisplayNameAttribute("UtcTimeZoneId")				);

				#endregion // CustomTimeZoneInfoProvider Properties

				#region DateRecurrenceCalculatorBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DateRecurrenceCalculatorBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "FirstOccurrenceDate",
					new DescriptionAttribute(SR.GetString("DateRecurrenceCalculatorBase_FirstOccurrenceDate_Property")),
				    new DisplayNameAttribute("FirstOccurrenceDate")				);


				tableBuilder.AddCustomAttributes(t, "LastOccurrenceDate",
					new DescriptionAttribute(SR.GetString("DateRecurrenceCalculatorBase_LastOccurrenceDate_Property")),
				    new DisplayNameAttribute("LastOccurrenceDate")				);

				#endregion // DateRecurrenceCalculatorBase Properties

				#region DateRecurrenceRuleBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.DateRecurrenceRuleBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DateRecurrenceRuleBase Properties

				#region RecurrenceDialogCoreRemoveRecurrenceCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RecurrenceDialogCoreRemoveRecurrenceCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // RecurrenceDialogCoreRemoveRecurrenceCommand Properties

				#region RotateDecorator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Primitives.RotateDecorator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Angle",
					new DescriptionAttribute(SR.GetString("RotateDecorator_Angle_Property")),
				    new DisplayNameAttribute("Angle"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // RotateDecorator Properties

				#region DayViewTimeslotArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayViewTimeslotArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsToday",
					new DescriptionAttribute(SR.GetString("DayViewTimeslotArea_IsToday_Property")),
				    new DisplayNameAttribute("IsToday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // DayViewTimeslotArea Properties

				#region MonthViewDayHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewDayHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MonthNameFormatType",
					new DescriptionAttribute(SR.GetString("MonthViewDayHeader_MonthNameFormatType_Property")),
				    new DisplayNameAttribute("MonthNameFormatType"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MonthNameVisibility",
					new DescriptionAttribute(SR.GetString("MonthViewDayHeader_MonthNameVisibility_Property")),
				    new DisplayNameAttribute("MonthNameVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MonthViewDayHeader Properties

				#region MonthViewTimeslotArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewTimeslotArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("MonthViewTimeslotArea_Start_Property")),
				    new DisplayNameAttribute("Start"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("MonthViewTimeslotArea_End_Property")),
				    new DisplayNameAttribute("End"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekNumber",
					new DescriptionAttribute(SR.GetString("MonthViewTimeslotArea_WeekNumber_Property")),
				    new DisplayNameAttribute("WeekNumber"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekHeaderWidth",
					new DescriptionAttribute(SR.GetString("MonthViewTimeslotArea_WeekHeaderWidth_Property")),
				    new DisplayNameAttribute("WeekHeaderWidth"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MonthViewTimeslotArea Properties

				#region MonthViewWeekHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewWeekHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("MonthViewWeekHeader_End_Property")),
				    new DisplayNameAttribute("End"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("MonthViewWeekHeader_Start_Property")),
				    new DisplayNameAttribute("Start"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("MonthViewWeekHeader_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWeekNumbers",
					new DescriptionAttribute(SR.GetString("MonthViewWeekHeader_ShowWeekNumbers_Property")),
				    new DisplayNameAttribute("ShowWeekNumbers"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekNumber",
					new DescriptionAttribute(SR.GetString("MonthViewWeekHeader_WeekNumber_Property")),
				    new DisplayNameAttribute("WeekNumber"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MonthViewWeekHeader Properties

				#region XamMonthView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamMonthView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamMonthViewAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamMonthViewAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWorkingDaysOfWeekOnly",
					new DescriptionAttribute(SR.GetString("XamMonthView_ShowWorkingDaysOfWeekOnly_Property")),
				    new DisplayNameAttribute("ShowWorkingDaysOfWeekOnly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorkingDaysSource",
					new DescriptionAttribute(SR.GetString("XamMonthView_WorkingDaysSource_Property")),
				    new DisplayNameAttribute("WorkingDaysSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWeekNumbers",
					new DescriptionAttribute(SR.GetString("XamMonthView_ShowWeekNumbers_Property")),
				    new DisplayNameAttribute("ShowWeekNumbers"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // XamMonthView Properties

				#region Reminder Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Reminder");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("Reminder_Text_Property")),
				    new DisplayNameAttribute("Text")				);


				tableBuilder.AddCustomAttributes(t, "IsSnoozed",
					new DescriptionAttribute(SR.GetString("Reminder_IsSnoozed_Property")),
				    new DisplayNameAttribute("IsSnoozed")				);


				tableBuilder.AddCustomAttributes(t, "LastDisplayedTime",
					new DescriptionAttribute(SR.GetString("Reminder_LastDisplayedTime_Property")),
				    new DisplayNameAttribute("LastDisplayedTime")				);


				tableBuilder.AddCustomAttributes(t, "LastSnoozeTime",
					new DescriptionAttribute(SR.GetString("Reminder_LastSnoozeTime_Property")),
				    new DisplayNameAttribute("LastSnoozeTime")				);


				tableBuilder.AddCustomAttributes(t, "SnoozeInterval",
					new DescriptionAttribute(SR.GetString("Reminder_SnoozeInterval_Property")),
				    new DisplayNameAttribute("SnoozeInterval")				);


				tableBuilder.AddCustomAttributes(t, "UserData",
					new DescriptionAttribute(SR.GetString("Reminder_UserData_Property")),
				    new DisplayNameAttribute("UserData")				);

				#endregion // Reminder Properties

				#region MonthViewDay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewDay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsAlternate",
					new DescriptionAttribute(SR.GetString("MonthViewDay_IsAlternate_Property")),
				    new DisplayNameAttribute("IsAlternate")				);

				#endregion // MonthViewDay Properties

				#region DayPresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayPresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("DayPresenterBase_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsToday",
					new DescriptionAttribute(SR.GetString("DayPresenterBase_IsToday_Property")),
				    new DisplayNameAttribute("IsToday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("DayPresenterBase_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // DayPresenterBase Properties

				#region SimpleNumericTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.SimpleNumericTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Value",
					new DescriptionAttribute(SR.GetString("SimpleNumericTextBox_Value_Property")),
				    new DisplayNameAttribute("Value"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("SimpleNumericTextBox_MinValue_Property")),
				    new DisplayNameAttribute("MinValue"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("SimpleNumericTextBox_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // SimpleNumericTextBox Properties

				#region ResourceCalendarElementBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ResourceCalendarElementBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ResourceCalendarElementBase_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("ResourceCalendarElementBase_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ResourceCalendarElementBase Properties

				#region SelectedActivityCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.SelectedActivityCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SelectedActivityCollection Properties

				#region SelectedActivitiesChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.SelectedActivitiesChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // SelectedActivitiesChangedEventArgs Properties

				#region ScheduleControlCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleControlCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleControlCommandBase Properties

				#region ScheduleControlCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleControlCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ScheduleControlCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ScheduleControlCommandSource Properties

				#region ScheduleControlBaseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleControlBaseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleControlBaseCommand Properties

				#region AppointmentPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.AppointmentPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AppointmentProperty",
					new DescriptionAttribute(SR.GetString("AppointmentPropertyMapping_AppointmentProperty_Property")),
				    new DisplayNameAttribute("AppointmentProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // AppointmentPropertyMapping Properties

				#region JournalPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.JournalPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "JournalProperty",
					new DescriptionAttribute(SR.GetString("JournalPropertyMapping_JournalProperty_Property")),
				    new DisplayNameAttribute("JournalProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // JournalPropertyMapping Properties

				#region ResourcePropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourcePropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceProperty",
					new DescriptionAttribute(SR.GetString("ResourcePropertyMapping_ResourceProperty_Property")),
				    new DisplayNameAttribute("ResourceProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // ResourcePropertyMapping Properties

				#region ResourceCalendarPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceCalendarPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceCalendarProperty",
					new DescriptionAttribute(SR.GetString("ResourceCalendarPropertyMapping_ResourceCalendarProperty_Property")),
				    new DisplayNameAttribute("ResourceCalendarProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // ResourceCalendarPropertyMapping Properties

				#region TaskPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.TaskPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "TaskProperty",
					new DescriptionAttribute(SR.GetString("TaskPropertyMapping_TaskProperty_Property")),
				    new DisplayNameAttribute("TaskProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // TaskPropertyMapping Properties

				#region AppointmentPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.AppointmentPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // AppointmentPropertyMappingCollection Properties

				#region JournalPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.JournalPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // JournalPropertyMappingCollection Properties

				#region ResourcePropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourcePropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // ResourcePropertyMappingCollection Properties

				#region ResourceCalendarPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceCalendarPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // ResourceCalendarPropertyMappingCollection Properties

				#region TaskPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.TaskPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // TaskPropertyMappingCollection Properties

				#region MonthViewDayHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.MonthViewDayHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MonthViewDayHeaderAutomationPeer Properties

				#region MonthViewDayOfWeekHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.MonthViewDayOfWeekHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MonthViewDayOfWeekHeaderAutomationPeer Properties

				#region MonthViewWeekHeaderAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.MonthViewWeekHeaderAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MonthViewWeekHeaderAutomationPeer Properties

				#region XamMonthViewAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamMonthViewAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamMonthViewAutomationPeer Properties

				#region MonthViewDayOfWeekHeader Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewDayOfWeekHeader");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AbbreviatedDayName",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_AbbreviatedDayName_Property")),
				    new DisplayNameAttribute("AbbreviatedDayName"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeek",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_DayOfWeek_Property")),
				    new DisplayNameAttribute("DayOfWeek"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayName",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_DayName_Property")),
				    new DisplayNameAttribute("DayName"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShortestDayName",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_ShortestDayName_Property")),
				    new DisplayNameAttribute("ShortestDayName"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderThickness",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_ComputedBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedBorderThickness"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("MonthViewDayOfWeekHeader_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MonthViewDayOfWeekHeader Properties

				#region BestFitDateRangePresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.BestFitDateRangePresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ShortFormattedText",
					new DescriptionAttribute(SR.GetString("BestFitDateRangePresenter_ShortFormattedText_Property")),
				    new DisplayNameAttribute("ShortFormattedText"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // BestFitDateRangePresenter Properties

				#region ActivityContentPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityContentPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterAreaSpacing",
					new DescriptionAttribute(SR.GetString("ActivityContentPanel_InterAreaSpacing_Property")),
				    new DisplayNameAttribute("InterAreaSpacing"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentAreaAlignment",
					new DescriptionAttribute(SR.GetString("ActivityContentPanel_ContentAreaAlignment_Property")),
				    new DisplayNameAttribute("ContentAreaAlignment"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContentAreaMinWidth",
					new DescriptionAttribute(SR.GetString("ActivityContentPanel_ContentAreaMinWidth_Property")),
				    new DisplayNameAttribute("ContentAreaMinWidth"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ActivityContentPanel Properties

				#region ScheduleControlBaseAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ScheduleControlBaseAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleControlBaseAutomationPeer Properties

				#region ReminderDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ReminderDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivitySubject",
					new DescriptionAttribute(SR.GetString("ReminderDialog_SelectedActivitySubject_Property")),
				    new DisplayNameAttribute("SelectedActivitySubject"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivityStartTimeDescription",
					new DescriptionAttribute(SR.GetString("ReminderDialog_SelectedActivityStartTimeDescription_Property")),
				    new DisplayNameAttribute("SelectedActivityStartTimeDescription"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReminderItems",
					new DescriptionAttribute(SR.GetString("ReminderDialog_ReminderItems_Property")),
				    new DisplayNameAttribute("ReminderItems"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ReminderDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivityImageVisibility",
					new DescriptionAttribute(SR.GetString("ReminderDialog_SelectedActivityImageVisibility_Property")),
				    new DisplayNameAttribute("SelectedActivityImageVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivitySubjectVisibility",
					new DescriptionAttribute(SR.GetString("ReminderDialog_SelectedActivitySubjectVisibility_Property")),
				    new DisplayNameAttribute("SelectedActivitySubjectVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSnoozePickerEnabled",
					new DescriptionAttribute(SR.GetString("ReminderDialog_IsSnoozePickerEnabled_Property")),
				    new DisplayNameAttribute("IsSnoozePickerEnabled"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivityImageSource",
					new DescriptionAttribute(SR.GetString("ReminderDialog_SelectedActivityImageSource_Property")),
				    new DisplayNameAttribute("SelectedActivityImageSource"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ReminderDialog Properties

				#region ReminderInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ReminderInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsDismissed",
					new DescriptionAttribute(SR.GetString("ReminderInfo_IsDismissed_Property")),
				    new DisplayNameAttribute("IsDismissed")				);


				tableBuilder.AddCustomAttributes(t, "Reminder",
					new DescriptionAttribute(SR.GetString("ReminderInfo_Reminder_Property")),
				    new DisplayNameAttribute("Reminder")				);


				tableBuilder.AddCustomAttributes(t, "Context",
					new DescriptionAttribute(SR.GetString("ReminderInfo_Context_Property")),
				    new DisplayNameAttribute("Context")				);


				tableBuilder.AddCustomAttributes(t, "IsSnoozed",
					new DescriptionAttribute(SR.GetString("ReminderInfo_IsSnoozed_Property")),
				    new DisplayNameAttribute("IsSnoozed")				);

				#endregion // ReminderInfo Properties

				#region ScheduleHeaderClickEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleHeaderClickEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Calendar",
					new DescriptionAttribute(SR.GetString("ScheduleHeaderClickEventArgs_Calendar_Property")),
				    new DisplayNameAttribute("Calendar")				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("ScheduleHeaderClickEventArgs_Date_Property")),
				    new DisplayNameAttribute("Date")				);

				#endregion // ScheduleHeaderClickEventArgs Properties

				#region ReminderDialogListItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogListItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ReminderDialogListItem_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);


				tableBuilder.AddCustomAttributes(t, "DueIn",
					new DescriptionAttribute(SR.GetString("ReminderDialogListItem_DueIn_Property")),
				    new DisplayNameAttribute("DueIn")				);


				tableBuilder.AddCustomAttributes(t, "ReminderInfo",
					new DescriptionAttribute(SR.GetString("ReminderDialogListItem_ReminderInfo_Property")),
				    new DisplayNameAttribute("ReminderInfo")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("ReminderDialogListItem_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "ImageSource",
					new DescriptionAttribute(SR.GetString("ReminderDialogListItem_ImageSource_Property")),
				    new DisplayNameAttribute("ImageSource")				);

				#endregion // ReminderDialogListItem Properties

				#region CalendarGroupItemsPresenterBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.CalendarGroupItemsPresenterBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarGroupItemsPresenterBase Properties

				#region MonthViewCalendarGroupTimeslotArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.MonthViewCalendarGroupTimeslotArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "WeekHeaderWidth",
					new DescriptionAttribute(SR.GetString("MonthViewCalendarGroupTimeslotArea_WeekHeaderWidth_Property")),
				    new DisplayNameAttribute("WeekHeaderWidth"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // MonthViewCalendarGroupTimeslotArea Properties

				#region TaskSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.TaskSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // TaskSettings Properties

				#region ActivitySettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivitySettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowAdd",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowAdd_Property")),
				    new DisplayNameAttribute("AllowAdd")				);


				tableBuilder.AddCustomAttributes(t, "AllowRemove",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowRemove_Property")),
				    new DisplayNameAttribute("AllowRemove")				);


				tableBuilder.AddCustomAttributes(t, "AllowEdit",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowEdit_Property")),
				    new DisplayNameAttribute("AllowEdit")				);


				tableBuilder.AddCustomAttributes(t, "AllowDragging",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowDragging_Property")),
				    new DisplayNameAttribute("AllowDragging")				);


				tableBuilder.AddCustomAttributes(t, "AllowRecurring",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowRecurring_Property")),
				    new DisplayNameAttribute("AllowRecurring")				);


				tableBuilder.AddCustomAttributes(t, "AllowTimeZoneNeutral",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowTimeZoneNeutral_Property")),
				    new DisplayNameAttribute("AllowTimeZoneNeutral")				);


				tableBuilder.AddCustomAttributes(t, "AllowResizing",
					new DescriptionAttribute(SR.GetString("ActivitySettings_AllowResizing_Property")),
				    new DisplayNameAttribute("AllowResizing")				);


				tableBuilder.AddCustomAttributes(t, "IsAddViaClickToAddEnabled",
					new DescriptionAttribute(SR.GetString("ActivitySettings_IsAddViaClickToAddEnabled_Property")),
				    new DisplayNameAttribute("IsAddViaClickToAddEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsAddViaDoubleClickEnabled",
					new DescriptionAttribute(SR.GetString("ActivitySettings_IsAddViaDoubleClickEnabled_Property")),
				    new DisplayNameAttribute("IsAddViaDoubleClickEnabled")				);


				tableBuilder.AddCustomAttributes(t, "IsAddViaTypingEnabled",
					new DescriptionAttribute(SR.GetString("ActivitySettings_IsAddViaTypingEnabled_Property")),
				    new DisplayNameAttribute("IsAddViaTypingEnabled")				);

				#endregion // ActivitySettings Properties

				#region AppointmentSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.AppointmentSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // AppointmentSettings Properties

				#region JournalSettings Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.JournalSettings");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // JournalSettings Properties

				#region WcfServiceException Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.WcfServiceException");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ServiceExceptionStackTrace",
					new DescriptionAttribute(SR.GetString("WcfServiceException_ServiceExceptionStackTrace_Property")),
				    new DisplayNameAttribute("ServiceExceptionStackTrace")				);


				tableBuilder.AddCustomAttributes(t, "ServiceExceptionTypeName",
					new DescriptionAttribute(SR.GetString("WcfServiceException_ServiceExceptionTypeName_Property")),
				    new DisplayNameAttribute("ServiceExceptionTypeName")				);

				#endregion // WcfServiceException Properties

				#region WcfListScheduleDataConnector Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.WcfListScheduleDataConnector");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("WcfListScheduleDataConnectorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("WcfListScheduleDataConnectorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceItems",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_ResourceItems_Property")),
				    new DisplayNameAttribute("ResourceItems"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PollingInterval",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_PollingInterval_Property")),
				    new DisplayNameAttribute("PollingInterval"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecurityToken",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_SecurityToken_Property")),
				    new DisplayNameAttribute("SecurityToken"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PollingMode",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_PollingMode_Property")),
				    new DisplayNameAttribute("PollingMode"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EndpointConfigurationName",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_EndpointConfigurationName_Property")),
				    new DisplayNameAttribute("EndpointConfigurationName"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RemoteAddress",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_RemoteAddress_Property")),
				    new DisplayNameAttribute("RemoteAddress"),
				    new TypeConverterAttribute(typeof(StringConverter))
,
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RemoteBinding",
					new DescriptionAttribute(SR.GetString("WcfListScheduleDataConnector_RemoteBinding_Property")),
				    new DisplayNameAttribute("RemoteBinding"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // WcfListScheduleDataConnector Properties

				#region PendingOperationIndicator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.PendingOperationIndicator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("PendingOperationIndicator_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // PendingOperationIndicator Properties

				#region ErrorEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ErrorEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("ErrorEventArgs_Error_Property")),
				    new DisplayNameAttribute("Error")				);


				tableBuilder.AddCustomAttributes(t, "LogError",
					new DescriptionAttribute(SR.GetString("ErrorEventArgs_LogError_Property")),
				    new DisplayNameAttribute("LogError")				);

				#endregion // ErrorEventArgs Properties

				#region ActivityRecurrenceChooserDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceChooserDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsOccurrence",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialog_ChoiceIsOccurrence_Property")),
				    new DisplayNameAttribute("ChoiceIsOccurrence"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsSeries",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialog_ChoiceIsSeries_Property")),
				    new DisplayNameAttribute("ChoiceIsSeries"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ScheduleDialogBase`1_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // ActivityRecurrenceChooserDialog Properties

				#region ActivityRecurrenceDialogCore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayDescriptions",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayDescriptions_Property")),
				    new DisplayNameAttribute("DayDescriptions"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayNumberMonthly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayNumberMonthly_Property")),
				    new DisplayNameAttribute("DayNumberMonthly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfMonthYearly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayOfMonthYearly_Property")),
				    new DisplayNameAttribute("DayOfMonthYearly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekMonthly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayOfWeekMonthly_Property")),
				    new DisplayNameAttribute("DayOfWeekMonthly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekYearly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayOfWeekYearly_Property")),
				    new DisplayNameAttribute("DayOfWeekYearly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekOrdinalMonthly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayOfWeekOrdinalMonthly_Property")),
				    new DisplayNameAttribute("DayOfWeekOrdinalMonthly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekOrdinalYearly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_DayOfWeekOrdinalYearly_Property")),
				    new DisplayNameAttribute("DayOfWeekOrdinalYearly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IntervalDaily",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IntervalDaily_Property")),
				    new DisplayNameAttribute("IntervalDaily"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IntervalWeekly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IntervalWeekly_Property")),
				    new DisplayNameAttribute("IntervalWeekly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IntervalMonthly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IntervalMonthly_Property")),
				    new DisplayNameAttribute("IntervalMonthly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IntervalMonthlyComplex",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IntervalMonthlyComplex_Property")),
				    new DisplayNameAttribute("IntervalMonthlyComplex"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IntervalYearly",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IntervalYearly_Property")),
				    new DisplayNameAttribute("IntervalYearly"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDailyPattern",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsDailyPattern_Property")),
				    new DisplayNameAttribute("IsDailyPattern"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDailyPatternTypeEvery",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsDailyPatternTypeEvery_Property")),
				    new DisplayNameAttribute("IsDailyPatternTypeEvery"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDailyPatternTypeWeekday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsDailyPatternTypeWeekday_Property")),
				    new DisplayNameAttribute("IsDailyPatternTypeWeekday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMonthlyPattern",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsMonthlyPattern_Property")),
				    new DisplayNameAttribute("IsMonthlyPattern"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMonthlyPatternTypeSimple",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsMonthlyPatternTypeSimple_Property")),
				    new DisplayNameAttribute("IsMonthlyPatternTypeSimple"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsMonthlyPatternTypeComplex",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsMonthlyPatternTypeComplex_Property")),
				    new DisplayNameAttribute("IsMonthlyPatternTypeComplex"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsRangeEndAfter",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsRangeEndAfter_Property")),
				    new DisplayNameAttribute("IsRangeEndAfter"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsRangeEndBy",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsRangeEndBy_Property")),
				    new DisplayNameAttribute("IsRangeEndBy"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsRangeForever",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsRangeForever_Property")),
				    new DisplayNameAttribute("IsRangeForever"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyPattern",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyPattern_Property")),
				    new DisplayNameAttribute("IsWeeklyPattern"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnSunday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnSunday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnSunday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnMonday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnMonday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnMonday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnTuesday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnTuesday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnTuesday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnWednesday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnWednesday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnWednesday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnThursday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnThursday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnThursday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnFriday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnFriday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnFriday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsWeeklyOnSaturday",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsWeeklyOnSaturday_Property")),
				    new DisplayNameAttribute("IsWeeklyOnSaturday"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsYearlyPattern",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsYearlyPattern_Property")),
				    new DisplayNameAttribute("IsYearlyPattern"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsYearlyPatternTypeSimple",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsYearlyPatternTypeSimple_Property")),
				    new DisplayNameAttribute("IsYearlyPatternTypeSimple"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsYearlyPatternTypeComplex",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsYearlyPatternTypeComplex_Property")),
				    new DisplayNameAttribute("IsYearlyPatternTypeComplex"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsRecurrenceRemoveable",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsRecurrenceRemoveable_Property")),
				    new DisplayNameAttribute("IsRecurrenceRemoveable"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MonthDescriptions",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_MonthDescriptions_Property")),
				    new DisplayNameAttribute("MonthDescriptions"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MonthOfYearSimple",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_MonthOfYearSimple_Property")),
				    new DisplayNameAttribute("MonthOfYearSimple"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MonthOfYearComplex",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_MonthOfYearComplex_Property")),
				    new DisplayNameAttribute("MonthOfYearComplex"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "OrdinalDescriptions",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_OrdinalDescriptions_Property")),
				    new DisplayNameAttribute("OrdinalDescriptions"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RangeStartDate",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_RangeStartDate_Property")),
				    new DisplayNameAttribute("RangeStartDate"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RangeEndDate",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_RangeEndDate_Property")),
				    new DisplayNameAttribute("RangeEndDate"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RangeEndAfterOccurrenceNumber",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_RangeEndAfterOccurrenceNumber_Property")),
				    new DisplayNameAttribute("RangeEndAfterOccurrenceNumber"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceDescription",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_RecurrenceDescription_Property")),
				    new DisplayNameAttribute("RecurrenceDescription"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_Activity_Property")),
				    new DisplayNameAttribute("Activity"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_RecurrenceDescriptionVisibility_Property")),
				    new DisplayNameAttribute("RecurrenceDescriptionVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActivityModifiable",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_IsActivityModifiable_Property")),
				    new DisplayNameAttribute("IsActivityModifiable"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EndTimePickerVisibility",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogCore_EndTimePickerVisibility_Property")),
				    new DisplayNameAttribute("EndTimePickerVisibility"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ActivityRecurrenceDialogCore Properties

				#region ReminderSubscriber Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ReminderSubscriber");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderSubscriber Properties

				#region ScheduleToolTip Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleToolTip");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleToolTip Properties

				#region ActivityToolTipInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityToolTipInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityToolTipInfo_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);


				tableBuilder.AddCustomAttributes(t, "ActivityPresenter",
					new DescriptionAttribute(SR.GetString("ActivityToolTipInfo_ActivityPresenter_Property")),
				    new DisplayNameAttribute("ActivityPresenter")				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("ActivityToolTipInfo_Error_Property")),
				    new DisplayNameAttribute("Error")				);

				#endregion // ActivityToolTipInfo Properties

				#region TimeZoneChooserDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TimeZoneChooserDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("TimeZoneChooserDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedTimeZoneToken",
					new DescriptionAttribute(SR.GetString("TimeZoneChooserDialog_SelectedTimeZoneToken_Property")),
				    new DisplayNameAttribute("SelectedTimeZoneToken"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeZoneTokens",
					new DescriptionAttribute(SR.GetString("TimeZoneChooserDialog_TimeZoneTokens_Property")),
				    new DisplayNameAttribute("TimeZoneTokens"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("TimeZoneChooserDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ScheduleDialogBase`1_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // TimeZoneChooserDialog Properties

				#region ReminderDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ReminderDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("ReminderDialogDisplayingEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);

				#endregion // ReminderDialogDisplayingEventArgs Properties

				#region ReminderActivatedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ReminderActivatedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ReminderInfo",
					new DescriptionAttribute(SR.GetString("ReminderActivatedEventArgs_ReminderInfo_Property")),
				    new DisplayNameAttribute("ReminderInfo")				);

				#endregion // ReminderActivatedEventArgs Properties

				#region ActivitiesEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivitiesEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activities",
					new DescriptionAttribute(SR.GetString("ActivitiesEventArgs_Activities_Property")),
				    new DisplayNameAttribute("Activities")				);

				#endregion // ActivitiesEventArgs Properties

				#region CancellableActivitiesEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CancellableActivitiesEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("CancellableActivitiesEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);

				#endregion // CancellableActivitiesEventArgs Properties

				#region ActivitiesDraggingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivitiesDraggingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivitiesDraggingEventArgs Properties

				#region ActivitiesDraggedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivitiesDraggedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsCopy",
					new DescriptionAttribute(SR.GetString("ActivitiesDraggedEventArgs_IsCopy_Property")),
				    new DisplayNameAttribute("IsCopy")				);

				#endregion // ActivitiesDraggedEventArgs Properties

				#region ActivityResizingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityResizingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsAdjustingStart",
					new DescriptionAttribute(SR.GetString("ActivityResizingEventArgs_IsAdjustingStart_Property")),
				    new DisplayNameAttribute("IsAdjustingStart")				);

				#endregion // ActivityResizingEventArgs Properties

				#region ActivityResizedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityResizedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityResizedEventArgs Properties

				#region MinMaxConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Primitives.MinMaxConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MinValue",
					new DescriptionAttribute(SR.GetString("MinMaxConverter_MinValue_Property")),
				    new DisplayNameAttribute("MinValue")				);


				tableBuilder.AddCustomAttributes(t, "MaxValue",
					new DescriptionAttribute(SR.GetString("MinMaxConverter_MaxValue_Property")),
				    new DisplayNameAttribute("MaxValue")				);

				#endregion // MinMaxConverter Properties

				#region ReminderDialogCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderDialogCommandBase Properties

				#region ReminderDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ReminderDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ReminderDialogCommandSource Properties

				#region ReminderDialogDismissAllCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogDismissAllCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderDialogDismissAllCommand Properties

				#region ReminderDialogOpenSelectedCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogOpenSelectedCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderDialogOpenSelectedCommand Properties

				#region ReminderDialogDismissSelectedCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogDismissSelectedCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderDialogDismissSelectedCommand Properties

				#region ReminderDialogSnoozeSelectedCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ReminderDialogSnoozeSelectedCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ReminderDialogSnoozeSelectedCommand Properties

				#region ScheduleDaysOfWeek Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ScheduleDaysOfWeek");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Monday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Monday_Property")),
				    new DisplayNameAttribute("Monday")				);


				tableBuilder.AddCustomAttributes(t, "Tuesday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Tuesday_Property")),
				    new DisplayNameAttribute("Tuesday")				);


				tableBuilder.AddCustomAttributes(t, "Wednesday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Wednesday_Property")),
				    new DisplayNameAttribute("Wednesday")				);


				tableBuilder.AddCustomAttributes(t, "Thursday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Thursday_Property")),
				    new DisplayNameAttribute("Thursday")				);


				tableBuilder.AddCustomAttributes(t, "Friday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Friday_Property")),
				    new DisplayNameAttribute("Friday")				);


				tableBuilder.AddCustomAttributes(t, "Saturday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Saturday_Property")),
				    new DisplayNameAttribute("Saturday")				);


				tableBuilder.AddCustomAttributes(t, "Sunday",
					new DescriptionAttribute(SR.GetString("ScheduleDaysOfWeek_Sunday_Property")),
				    new DisplayNameAttribute("Sunday")				);

				#endregion // ScheduleDaysOfWeek Properties

				#region ActivityRecurrenceChooserDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityRecurrenceChooserDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ChooserType",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialogDisplayingEventArgs_ChooserType_Property")),
				    new DisplayNameAttribute("ChooserType")				);


				tableBuilder.AddCustomAttributes(t, "ChooserAction",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceChooserDialogDisplayingEventArgs_ChooserAction_Property")),
				    new DisplayNameAttribute("ChooserAction")				);

				#endregion // ActivityRecurrenceChooserDialogDisplayingEventArgs Properties

				#region ErrorDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ErrorDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("ErrorDisplayingEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);


				tableBuilder.AddCustomAttributes(t, "DisplayType",
					new DescriptionAttribute(SR.GetString("ErrorDisplayingEventArgs_DisplayType_Property")),
				    new DisplayNameAttribute("DisplayType")				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("ErrorDisplayingEventArgs_Error_Property")),
				    new DisplayNameAttribute("Error")				);

				#endregion // ErrorDisplayingEventArgs Properties

				#region OperationResult Properties
				t = controlAssembly.GetType("Infragistics.OperationResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Error",
					new DescriptionAttribute(SR.GetString("OperationResult_Error_Property")),
				    new DisplayNameAttribute("Error")				);


				tableBuilder.AddCustomAttributes(t, "IsCanceled",
					new DescriptionAttribute(SR.GetString("OperationResult_IsCanceled_Property")),
				    new DisplayNameAttribute("IsCanceled")				);


				tableBuilder.AddCustomAttributes(t, "IsComplete",
					new DescriptionAttribute(SR.GetString("OperationResult_IsComplete_Property")),
				    new DisplayNameAttribute("IsComplete")				);

				#endregion // OperationResult Properties

				#region CancelOperationResult Properties
				t = controlAssembly.GetType("Infragistics.CancelOperationResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Operation",
					new DescriptionAttribute(SR.GetString("CancelOperationResult_Operation_Property")),
				    new DisplayNameAttribute("Operation")				);

				#endregion // CancelOperationResult Properties

				#region PropertyMappingBase`1 Properties
				t = controlAssembly.GetType("Infragistics.PropertyMappingBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // PropertyMappingBase`1 Properties

				#region MetadataPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.MetadataPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataProperty",
					new DescriptionAttribute(SR.GetString("MetadataPropertyMapping_MetadataProperty_Property")),
				    new DisplayNameAttribute("MetadataProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // MetadataPropertyMapping Properties

				#region MetadataPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.MetadataPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // MetadataPropertyMappingCollection Properties

				#region ItemFactory Properties
				t = controlAssembly.GetType("Infragistics.ItemFactory");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemFactory Properties

				#region ItemFactory`1 Properties
				t = controlAssembly.GetType("Infragistics.ItemFactory`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ItemFactory`1 Properties

				#region ImmutableCollection`1 Properties
				t = controlAssembly.GetType("Infragistics.Collections.ImmutableCollection`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ImmutableCollection`1 Properties

				#region PropertyMappingCollection`2 Properties
				t = controlAssembly.GetType("Infragistics.PropertyMappingCollection`2");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // PropertyMappingCollection`2 Properties

				#region MetadataPropertyValueStore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.MetadataPropertyValueStore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // MetadataPropertyValueStore Properties

				#region NullableRoutedPropertyChangedEventArgs`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.NullableRoutedPropertyChangedEventArgs`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableRoutedPropertyChangedEventArgs`1 Properties

				#region NullableRoutedPropertyChangedEventHandler`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.NullableRoutedPropertyChangedEventHandler`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // NullableRoutedPropertyChangedEventHandler`1 Properties

				#region IGColorScheme Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.IGColorScheme");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BaseColors",
					new DescriptionAttribute(SR.GetString("IGColorScheme_BaseColors_Property")),
				    new DisplayNameAttribute("BaseColors")				);


				tableBuilder.AddCustomAttributes(t, "DateNavigatorResourceProvider",
					new DescriptionAttribute(SR.GetString("IGColorScheme_DateNavigatorResourceProvider_Property")),
				    new DisplayNameAttribute("DateNavigatorResourceProvider")				);

				#endregion // IGColorScheme Properties

				#region ScheduleResizerBar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleResizerBar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ScheduleResizerBar_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground")				);

				#endregion // ScheduleResizerBar Properties

				#region XamOutlookCalendarViewCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewCommandBase Properties

				#region XamOutlookCalendarViewCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarViewCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarViewCommandSource_Properties"))
				);

				#endregion // XamOutlookCalendarViewCommandSource Properties

				#region XamOutlookCalendarViewDayCountCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewDayCountCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewDayCountCommand Properties

				#region XamOutlookCalendarViewChangeViewCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewChangeViewCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewChangeViewCommand Properties

				#region XamOutlookCalendarViewChangeToScheduleViewCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewChangeToScheduleViewCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewChangeToScheduleViewCommand Properties

				#region XamOutlookCalendarViewChangeToDayViewCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewChangeToDayViewCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewChangeToDayViewCommand Properties

				#region XamOutlookCalendarViewNavigateCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.XamOutlookCalendarViewNavigateCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewNavigateCommand Properties

				#region XamOutlookCalendarView Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamOutlookCalendarView");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamOutlookCalendarViewAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamOutlookCalendarViewAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarDisplayMode",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CalendarDisplayMode_Property")),
				    new DisplayNameAttribute("CalendarDisplayMode"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroupsOverride",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CalendarGroupsOverride_Property")),
				    new DisplayNameAttribute("CalendarGroupsOverride"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarGroupsResolved",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CalendarGroupsResolved_Property")),
				    new DisplayNameAttribute("CalendarGroupsResolved"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DefaultBrushProvider",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_DefaultBrushProvider_Property")),
				    new DisplayNameAttribute("DefaultBrushProvider"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowCalendarCloseButton",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ShowCalendarCloseButton_Property")),
				    new DisplayNameAttribute("ShowCalendarCloseButton"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowCalendarOverlayButton",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ShowCalendarOverlayButton_Property")),
				    new DisplayNameAttribute("ShowCalendarOverlayButton"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentTimeIndicatorVisibility",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CurrentTimeIndicatorVisibility_Property")),
				    new DisplayNameAttribute("CurrentTimeIndicatorVisibility"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PrimaryTimeZoneLabel",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_PrimaryTimeZoneLabel_Property")),
				    new DisplayNameAttribute("PrimaryTimeZoneLabel"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneLabel",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_SecondaryTimeZoneLabel_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneLabel"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneId",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_SecondaryTimeZoneId_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneId"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SecondaryTimeZoneVisibility",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_SecondaryTimeZoneVisibility_Property")),
				    new DisplayNameAttribute("SecondaryTimeZoneVisibility"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWorkingHoursOnly",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ShowWorkingHoursOnly_Property")),
				    new DisplayNameAttribute("ShowWorkingHoursOnly"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeslotInterval",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_TimeslotInterval_Property")),
				    new DisplayNameAttribute("TimeslotInterval"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorkingHoursSource",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_WorkingHoursSource_Property")),
				    new DisplayNameAttribute("WorkingHoursSource"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowWeekNumbers",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ShowWeekNumbers_Property")),
				    new DisplayNameAttribute("ShowWeekNumbers"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentViewMode",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CurrentViewMode_Property")),
				    new DisplayNameAttribute("CurrentViewMode"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentViewDateRange",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CurrentViewDateRange_Property")),
				    new DisplayNameAttribute("CurrentViewDateRange"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentViewDateRangeText",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CurrentViewDateRangeText_Property")),
				    new DisplayNameAttribute("CurrentViewDateRangeText"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DateNavigator",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_DateNavigator_Property")),
				    new DisplayNameAttribute("DateNavigator"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayViewToScheduleViewSwitchThreshold",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_DayViewToScheduleViewSwitchThreshold_Property")),
				    new DisplayNameAttribute("DayViewToScheduleViewSwitchThreshold"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HeaderVisibility",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_HeaderVisibility_Property")),
				    new DisplayNameAttribute("HeaderVisibility"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsDayViewToScheduleViewSwitchEnabled",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_IsDayViewToScheduleViewSwitchEnabled_Property")),
				    new DisplayNameAttribute("IsDayViewToScheduleViewSwitchEnabled"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsScheduleViewToDayViewSwitchEnabled",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_IsScheduleViewToDayViewSwitchEnabled_Property")),
				    new DisplayNameAttribute("IsScheduleViewToDayViewSwitchEnabled"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScheduleViewToDayViewSwitchThreshold",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ScheduleViewToDayViewSwitchThreshold_Property")),
				    new DisplayNameAttribute("ScheduleViewToDayViewSwitchThreshold"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveCalendar",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_ActiveCalendar_Property")),
				    new DisplayNameAttribute("ActiveCalendar"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedTimeRange",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_SelectedTimeRange_Property")),
				    new DisplayNameAttribute("SelectedTimeRange"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WorkingDaysSource",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_WorkingDaysSource_Property")),
				    new DisplayNameAttribute("WorkingDaysSource"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivities",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_SelectedActivities_Property")),
				    new DisplayNameAttribute("SelectedActivities"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCalendarGroupResizing",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_AllowCalendarGroupResizing_Property")),
				    new DisplayNameAttribute("AllowCalendarGroupResizing"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinCalendarGroupHorizontalExtent",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_MinCalendarGroupHorizontalExtent_Property")),
				    new DisplayNameAttribute("MinCalendarGroupHorizontalExtent"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinCalendarGroupVerticalExtent",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_MinCalendarGroupVerticalExtent_Property")),
				    new DisplayNameAttribute("MinCalendarGroupVerticalExtent"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreferredCalendarGroupVerticalExtent",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_PreferredCalendarGroupVerticalExtent_Property")),
				    new DisplayNameAttribute("PreferredCalendarGroupVerticalExtent"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "PreferredCalendarGroupHorizontalExtent",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_PreferredCalendarGroupHorizontalExtent_Property")),
				    new DisplayNameAttribute("PreferredCalendarGroupHorizontalExtent"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowMultiDayActivityAreaResizing",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_AllowMultiDayActivityAreaResizing_Property")),
				    new DisplayNameAttribute("AllowMultiDayActivityAreaResizing"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MultiDayActivityAreaHeight",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_MultiDayActivityAreaHeight_Property")),
				    new DisplayNameAttribute("MultiDayActivityAreaHeight"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowCalendarHeaderAreaResizing",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_AllowCalendarHeaderAreaResizing_Property")),
				    new DisplayNameAttribute("AllowCalendarHeaderAreaResizing"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarHeaderAreaWidth",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_CalendarHeaderAreaWidth_Property")),
				    new DisplayNameAttribute("CalendarHeaderAreaWidth"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTouchSupportEnabled",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_IsTouchSupportEnabled_Property")),
				    new DisplayNameAttribute("IsTouchSupportEnabled"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TimeslotGutterAreaWidth",
					new DescriptionAttribute(SR.GetString("XamOutlookCalendarView_TimeslotGutterAreaWidth_Property")),
				    new DisplayNameAttribute("TimeslotGutterAreaWidth"),
					new CategoryAttribute(SR.GetString("XamOutlookCalendarView_Properties"))
				);

				#endregion // XamOutlookCalendarView Properties

				#region CurrentViewModeChangingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CurrentViewModeChangingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("CurrentViewModeChangingEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);


				tableBuilder.AddCustomAttributes(t, "NewValue",
					new DescriptionAttribute(SR.GetString("CurrentViewModeChangingEventArgs_NewValue_Property")),
				    new DisplayNameAttribute("NewValue")				);


				tableBuilder.AddCustomAttributes(t, "OldValue",
					new DescriptionAttribute(SR.GetString("CurrentViewModeChangingEventArgs_OldValue_Property")),
				    new DisplayNameAttribute("OldValue")				);

				#endregion // CurrentViewModeChangingEventArgs Properties

				#region ActivityDialogCoreCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreCommandBase Properties

				#region ActivityDialogCoreCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ActivityDialogCoreCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ActivityDialogCoreCommandSource Properties

				#region ActivityDialogCoreSaveAndCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreSaveAndCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreSaveAndCloseCommand Properties

				#region ActivityDialogCoreCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreCloseCommand Properties

				#region ActivityDialogCoreDeleteCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreDeleteCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreDeleteCommand Properties

				#region ActivityDialogCoreDisplayRecurrenceDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreDisplayRecurrenceDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreDisplayRecurrenceDialogCommand Properties

				#region ActivityDialogCoreShowTimeZonePickersCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreShowTimeZonePickersCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreShowTimeZonePickersCommand Properties

				#region ActivityDialogCoreHideTimeZonePickersCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCoreHideTimeZonePickersCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityDialogCoreHideTimeZonePickersCommand Properties

				#region ActivityDialogRibbonLite Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogRibbonLite");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ColorScheme",
					new DescriptionAttribute(SR.GetString("ActivityDialogRibbonLite_ColorScheme_Property")),
				    new DisplayNameAttribute("ColorScheme")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityDialogRibbonLite_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // ActivityDialogRibbonLite Properties

				#region ActivityDialogCore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityDialogCore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_DataManager_Property")),
				    new DisplayNameAttribute("DataManager")				);


				tableBuilder.AddCustomAttributes(t, "IsActivityModifiable",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_IsActivityModifiable_Property")),
				    new DisplayNameAttribute("IsActivityModifiable")				);


				tableBuilder.AddCustomAttributes(t, "IsActivityRemoveable",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_IsActivityRemoveable_Property")),
				    new DisplayNameAttribute("IsActivityRemoveable")				);


				tableBuilder.AddCustomAttributes(t, "IsDirty",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_IsDirty_Property")),
				    new DisplayNameAttribute("IsDirty")				);


				tableBuilder.AddCustomAttributes(t, "IsOccurrence",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_IsOccurrence_Property")),
				    new DisplayNameAttribute("IsOccurrence")				);


				tableBuilder.AddCustomAttributes(t, "IsRecurrenceRoot",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_IsRecurrenceRoot_Property")),
				    new DisplayNameAttribute("IsRecurrenceRoot")				);


				tableBuilder.AddCustomAttributes(t, "NavigationControlSite",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_NavigationControlSite_Property")),
				    new DisplayNameAttribute("NavigationControlSite")				);


				tableBuilder.AddCustomAttributes(t, "NavigationControlSiteContent",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_NavigationControlSiteContent_Property")),
				    new DisplayNameAttribute("NavigationControlSiteContent")				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceDescription",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_OccurrenceDescription_Property")),
				    new DisplayNameAttribute("OccurrenceDescription")				);


				tableBuilder.AddCustomAttributes(t, "OccurrenceDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_OccurrenceDescriptionVisibility_Property")),
				    new DisplayNameAttribute("OccurrenceDescriptionVisibility")				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceRootDescription",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_RecurrenceRootDescription_Property")),
				    new DisplayNameAttribute("RecurrenceRootDescription")				);


				tableBuilder.AddCustomAttributes(t, "RecurrenceRootDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_RecurrenceRootDescriptionVisibility_Property")),
				    new DisplayNameAttribute("RecurrenceRootDescriptionVisibility")				);


				tableBuilder.AddCustomAttributes(t, "TimeZonePickerEnabled",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_TimeZonePickerEnabled_Property")),
				    new DisplayNameAttribute("TimeZonePickerEnabled")				);


				tableBuilder.AddCustomAttributes(t, "TimeZonePickerVisibility",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_TimeZonePickerVisibility_Property")),
				    new DisplayNameAttribute("TimeZonePickerVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ConflictMessage",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_ConflictMessage_Property")),
				    new DisplayNameAttribute("ConflictMessage")				);


				tableBuilder.AddCustomAttributes(t, "HasConflicts",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_HasConflicts_Property")),
				    new DisplayNameAttribute("HasConflicts")				);


				tableBuilder.AddCustomAttributes(t, "DescriptionFormatWarningMessage",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_DescriptionFormatWarningMessage_Property")),
				    new DisplayNameAttribute("DescriptionFormatWarningMessage")				);


				tableBuilder.AddCustomAttributes(t, "DescriptionFormatWarningMessageVisibility",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_DescriptionFormatWarningMessageVisibility_Property")),
				    new DisplayNameAttribute("DescriptionFormatWarningMessageVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryListItems",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_ActivityCategoryListItems_Property")),
				    new DisplayNameAttribute("ActivityCategoryListItems")				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryListItemsVisibility",
					new DescriptionAttribute(SR.GetString("ActivityDialogCore_ActivityCategoryListItemsVisibility_Property")),
				    new DisplayNameAttribute("ActivityCategoryListItemsVisibility")				);

				#endregion // ActivityDialogCore Properties

				#region JournalDialogCore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.JournalDialogCore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("JournalDialogCore_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // JournalDialogCore Properties

				#region TaskDialogCore Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.TaskDialogCore");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("TaskDialogCore_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);


				tableBuilder.AddCustomAttributes(t, "DatesSectionVisibility",
					new DescriptionAttribute(SR.GetString("TaskDialogCore_DatesSectionVisibility_Property")),
				    new DisplayNameAttribute("DatesSectionVisibility")				);


				tableBuilder.AddCustomAttributes(t, "DueInDescription",
					new DescriptionAttribute(SR.GetString("TaskDialogCore_DueInDescription_Property")),
				    new DisplayNameAttribute("DueInDescription")				);


				tableBuilder.AddCustomAttributes(t, "DueInDescriptionVisibility",
					new DescriptionAttribute(SR.GetString("TaskDialogCore_DueInDescriptionVisibility_Property")),
				    new DisplayNameAttribute("DueInDescriptionVisibility")				);

				#endregion // TaskDialogCore Properties

				#region XamDateNavigator Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.XamDateNavigator");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamDateNavigatorAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamDateNavigatorAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("XamDateNavigator_DataManager_Property")),
				    new DisplayNameAttribute("DataManager"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TodayButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamDateNavigator_TodayButtonVisibility_Property")),
				    new DisplayNameAttribute("TodayButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HighlightDayCriteria",
					new DescriptionAttribute(SR.GetString("XamDateNavigator_HighlightDayCriteria_Property")),
				    new DisplayNameAttribute("HighlightDayCriteria"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowActivityToolTips",
					new DescriptionAttribute(SR.GetString("XamDateNavigator_ShowActivityToolTips_Property")),
				    new DisplayNameAttribute("ShowActivityToolTips"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityToolTipTemplate",
					new DescriptionAttribute(SR.GetString("XamDateNavigator_ActivityToolTipTemplate_Property")),
				    new DisplayNameAttribute("ActivityToolTipTemplate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // XamDateNavigator Properties

				#region ActivityCategory Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityCategory");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Color",
					new DescriptionAttribute(SR.GetString("ActivityCategory_Color_Property")),
				    new DisplayNameAttribute("Color")				);


				tableBuilder.AddCustomAttributes(t, "DataItem",
					new DescriptionAttribute(SR.GetString("ActivityCategory_DataItem_Property")),
				    new DisplayNameAttribute("DataItem")				);


				tableBuilder.AddCustomAttributes(t, "Description",
					new DescriptionAttribute(SR.GetString("ActivityCategory_Description_Property")),
				    new DisplayNameAttribute("Description")				);


				tableBuilder.AddCustomAttributes(t, "CategoryName",
					new DescriptionAttribute(SR.GetString("ActivityCategory_CategoryName_Property")),
				    new DisplayNameAttribute("CategoryName")				);

				#endregion // ActivityCategory Properties

				#region ActivityCategoryPropertyMapping Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityCategoryPropertyMapping");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryProperty",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPropertyMapping_ActivityCategoryProperty_Property")),
				    new DisplayNameAttribute("ActivityCategoryProperty")				);


				tableBuilder.AddCustomAttributes(t, "DataObjectProperty",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_DataObjectProperty_Property")),
				    new DisplayNameAttribute("DataObjectProperty")				);


				tableBuilder.AddCustomAttributes(t, "Converter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_Converter_Property")),
				    new DisplayNameAttribute("Converter")				);


				tableBuilder.AddCustomAttributes(t, "ConverterParameter",
					new DescriptionAttribute(SR.GetString("PropertyMappingBase`1_ConverterParameter_Property")),
				    new DisplayNameAttribute("ConverterParameter")				);

				#endregion // ActivityCategoryPropertyMapping Properties

				#region ActivityCategoryPropertyMappingCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityCategoryPropertyMappingCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "MetadataPropertyMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_MetadataPropertyMappings_Property")),
				    new DisplayNameAttribute("MetadataPropertyMappings")				);


				tableBuilder.AddCustomAttributes(t, "UseDefaultMappings",
					new DescriptionAttribute(SR.GetString("PropertyMappingCollection`2_UseDefaultMappings_Property")),
				    new DisplayNameAttribute("UseDefaultMappings")				);

				#endregion // ActivityCategoryPropertyMappingCollection Properties

				#region ActivityCategoryCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityCategoryCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryCollection Properties

				#region ActivityCategoryBarPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryBarPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Categories",
					new DescriptionAttribute(SR.GetString("ActivityCategoryBarPanel_Categories_Property")),
				    new DisplayNameAttribute("Categories")				);


				tableBuilder.AddCustomAttributes(t, "HideFirstCategory",
					new DescriptionAttribute(SR.GetString("ActivityCategoryBarPanel_HideFirstCategory_Property")),
				    new DisplayNameAttribute("HideFirstCategory")				);

				#endregion // ActivityCategoryBarPanel Properties

				#region ActivityCategoryPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Category",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPresenter_Category_Property")),
				    new DisplayNameAttribute("Category")				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPresenter_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground")				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPresenter_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush")				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPresenter_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground")				);


				tableBuilder.AddCustomAttributes(t, "NameVisibility",
					new DescriptionAttribute(SR.GetString("ActivityCategoryPresenter_NameVisibility_Property")),
				    new DisplayNameAttribute("NameVisibility")				);

				#endregion // ActivityCategoryPresenter Properties

				#region CompactWrapPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Primitives.CompactWrapPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "InterItemSpacingX",
					new DescriptionAttribute(SR.GetString("CompactWrapPanel_InterItemSpacingX_Property")),
				    new DisplayNameAttribute("InterItemSpacingX")				);


				tableBuilder.AddCustomAttributes(t, "InterItemSpacingY",
					new DescriptionAttribute(SR.GetString("CompactWrapPanel_InterItemSpacingY_Property")),
				    new DisplayNameAttribute("InterItemSpacingY")				);


				tableBuilder.AddCustomAttributes(t, "MinItemExtent",
					new DescriptionAttribute(SR.GetString("CompactWrapPanel_MinItemExtent_Property")),
				    new DisplayNameAttribute("MinItemExtent")				);


				tableBuilder.AddCustomAttributes(t, "Orientation",
					new DescriptionAttribute(SR.GetString("CompactWrapPanel_Orientation_Property")),
				    new DisplayNameAttribute("Orientation")				);

				#endregion // CompactWrapPanel Properties

				#region ItemOperationResult`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ItemOperationResult`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemOperationResult`1_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ItemOperationResult`1 Properties

				#region ResourceOperationResult Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ResourceOperationResult");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Resource",
					new DescriptionAttribute(SR.GetString("ResourceOperationResult_Resource_Property")),
				    new DisplayNameAttribute("Resource")				);


				tableBuilder.AddCustomAttributes(t, "Item",
					new DescriptionAttribute(SR.GetString("ItemOperationResult`1_Item_Property")),
				    new DisplayNameAttribute("Item")				);

				#endregion // ResourceOperationResult Properties

				#region ActivityCategoryPresenterAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ActivityCategoryPresenterAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryPresenterAutomationPeer Properties

				#region XamOutlookCalendarViewAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.XamOutlookCalendarViewAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // XamOutlookCalendarViewAutomationPeer Properties

				#region ActivityCategoryCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryCommandBase Properties

				#region ActivityCategoryCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ActivityCategoryCommandSource Properties

				#region ActivityCategoryClearAllActivityCategoriesCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryClearAllActivityCategoriesCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryClearAllActivityCategoriesCommand Properties

				#region ActivityCategoryDisplayActivityCategoriesDialogCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDisplayActivityCategoriesDialogCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDisplayActivityCategoriesDialogCommand Properties

				#region ActivityCategoryToggleActivityCategorySelectedStateCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryToggleActivityCategorySelectedStateCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryToggleActivityCategorySelectedStateCommand Properties

				#region ActivityCategoryCommandParameterInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryCommandParameterInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryHelper",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCommandParameterInfo_ActivityCategoryHelper_Property")),
				    new DisplayNameAttribute("ActivityCategoryHelper")				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryListItem",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCommandParameterInfo_ActivityCategoryListItem_Property")),
				    new DisplayNameAttribute("ActivityCategoryListItem")				);

				#endregion // ActivityCategoryCommandParameterInfo Properties

				#region ActivityCategoryHelper Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryHelper");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryHelper Properties

				#region ActivityCategoryListItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryListItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategory",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_ActivityCategory_Property")),
				    new DisplayNameAttribute("ActivityCategory")				);


				tableBuilder.AddCustomAttributes(t, "BorderBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_BorderBrush_Property")),
				    new DisplayNameAttribute("BorderBrush")				);


				tableBuilder.AddCustomAttributes(t, "BackgroundBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_BackgroundBrush_Property")),
				    new DisplayNameAttribute("BackgroundBrush")				);


				tableBuilder.AddCustomAttributes(t, "Command",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_Command_Property")),
				    new DisplayNameAttribute("Command")				);


				tableBuilder.AddCustomAttributes(t, "ForegroundBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_ForegroundBrush_Property")),
				    new DisplayNameAttribute("ForegroundBrush")				);


				tableBuilder.AddCustomAttributes(t, "HasActivityCategory",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_HasActivityCategory_Property")),
				    new DisplayNameAttribute("HasActivityCategory")				);


				tableBuilder.AddCustomAttributes(t, "IconImageSource",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_IconImageSource_Property")),
				    new DisplayNameAttribute("IconImageSource")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected")				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryHelper",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_ActivityCategoryHelper_Property")),
				    new DisplayNameAttribute("ActivityCategoryHelper")				);


				tableBuilder.AddCustomAttributes(t, "CommandParameter",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_CommandParameter_Property")),
				    new DisplayNameAttribute("CommandParameter")				);


				tableBuilder.AddCustomAttributes(t, "IsCustomizable",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_IsCustomizable_Property")),
				    new DisplayNameAttribute("IsCustomizable")				);


				tableBuilder.AddCustomAttributes(t, "IsInEditMode",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_IsInEditMode_Property")),
				    new DisplayNameAttribute("IsInEditMode")				);


				tableBuilder.AddCustomAttributes(t, "IsNotInMasterList",
					new DescriptionAttribute(SR.GetString("ActivityCategoryListItem_IsNotInMasterList_Property")),
				    new DisplayNameAttribute("IsNotInMasterList")				);

				#endregion // ActivityCategoryListItem Properties

				#region ActivityCategoryDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityCategoryListItems",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_ActivityCategoryListItems_Property")),
				    new DisplayNameAttribute("ActivityCategoryListItems")				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);


				tableBuilder.AddCustomAttributes(t, "SelectedActivityCategoryListItem",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_SelectedActivityCategoryListItem_Property")),
				    new DisplayNameAttribute("SelectedActivityCategoryListItem")				);


				tableBuilder.AddCustomAttributes(t, "IsOwningResourceModifiable",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_IsOwningResourceModifiable_Property")),
				    new DisplayNameAttribute("IsOwningResourceModifiable")				);


				tableBuilder.AddCustomAttributes(t, "AreCustomColorsAllowed",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_AreCustomColorsAllowed_Property")),
				    new DisplayNameAttribute("AreCustomColorsAllowed")				);


				tableBuilder.AddCustomAttributes(t, "DefaultCategoryColors",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_DefaultCategoryColors_Property")),
				    new DisplayNameAttribute("DefaultCategoryColors")				);


				tableBuilder.AddCustomAttributes(t, "IsActivityCategorySelected",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_IsActivityCategorySelected_Property")),
				    new DisplayNameAttribute("IsActivityCategorySelected")				);


				tableBuilder.AddCustomAttributes(t, "IsSelectedActivityCategoryListItemCustomizable",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialog_IsSelectedActivityCategoryListItemCustomizable_Property")),
				    new DisplayNameAttribute("IsSelectedActivityCategoryListItemCustomizable")				);

				#endregion // ActivityCategoryDialog Properties

				#region ActivityCategoryClearActivityCategorySelectedStateCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryClearActivityCategorySelectedStateCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryClearActivityCategorySelectedStateCommand Properties

				#region ActivityCategorySetActivityCategorySelectedStateCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategorySetActivityCategorySelectedStateCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategorySetActivityCategorySelectedStateCommand Properties

				#region ActivityCategoryDialogCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogCommandBase Properties

				#region ActivityCategoryDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ActivityCategoryDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ActivityCategoryDialogCommandSource Properties

				#region ActivityCategoryDialogSaveAndCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogSaveAndCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogSaveAndCloseCommand Properties

				#region ActivityCategoryDialogCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogCloseCommand Properties

				#region ActivityCategoryDialogCreateNewCategoryCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogCreateNewCategoryCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogCreateNewCategoryCommand Properties

				#region ActivityCategoryDialogEditSelectedCategoryCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogEditSelectedCategoryCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogEditSelectedCategoryCommand Properties

				#region ActivityCategoryDialogDeleteSelectedCategoryCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryDialogDeleteSelectedCategoryCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryDialogDeleteSelectedCategoryCommand Properties

				#region ActivityCategoryColorPicker Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryColorPicker");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedColor",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_SelectedColor_Property")),
				    new DisplayNameAttribute("SelectedColor")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColorRedValue",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_SelectedColorRedValue_Property")),
				    new DisplayNameAttribute("SelectedColorRedValue")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColorGreenValue",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_SelectedColorGreenValue_Property")),
				    new DisplayNameAttribute("SelectedColorGreenValue")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColorBlueValue",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_SelectedColorBlueValue_Property")),
				    new DisplayNameAttribute("SelectedColorBlueValue")				);


				tableBuilder.AddCustomAttributes(t, "SelectedColorSwatch",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_SelectedColorSwatch_Property")),
				    new DisplayNameAttribute("SelectedColorSwatch")				);


				tableBuilder.AddCustomAttributes(t, "NoneColorButtonBackgroundBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_NoneColorButtonBackgroundBrush_Property")),
				    new DisplayNameAttribute("NoneColorButtonBackgroundBrush")				);


				tableBuilder.AddCustomAttributes(t, "NoneColorButtonBorderBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_NoneColorButtonBorderBrush_Property")),
				    new DisplayNameAttribute("NoneColorButtonBorderBrush")				);


				tableBuilder.AddCustomAttributes(t, "NoneColorButtonForegroundBrush",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_NoneColorButtonForegroundBrush_Property")),
				    new DisplayNameAttribute("NoneColorButtonForegroundBrush")				);


				tableBuilder.AddCustomAttributes(t, "AreCustomColorsAllowed",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_AreCustomColorsAllowed_Property")),
				    new DisplayNameAttribute("AreCustomColorsAllowed")				);


				tableBuilder.AddCustomAttributes(t, "FixedColors",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_FixedColors_Property")),
				    new DisplayNameAttribute("FixedColors")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);


				tableBuilder.AddCustomAttributes(t, "ColorSwatchExtent",
					new DescriptionAttribute(SR.GetString("ActivityCategoryColorPicker_ColorSwatchExtent_Property")),
				    new DisplayNameAttribute("ColorSwatchExtent")				);

				#endregion // ActivityCategoryColorPicker Properties

				#region DropDownToggleButton Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DropDownToggleButton");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // DropDownToggleButton Properties

				#region ActivityCategoryCreationDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryCreationDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Color",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_Color_Property")),
				    new DisplayNameAttribute("Color")				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);


				tableBuilder.AddCustomAttributes(t, "Name",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_Name_Property")),
				    new DisplayNameAttribute("Name")				);


				tableBuilder.AddCustomAttributes(t, "CategoryName",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_CategoryName_Property")),
				    new DisplayNameAttribute("CategoryName")				);


				tableBuilder.AddCustomAttributes(t, "AreCustomColorsAllowed",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_AreCustomColorsAllowed_Property")),
				    new DisplayNameAttribute("AreCustomColorsAllowed")				);


				tableBuilder.AddCustomAttributes(t, "DefaultCategoryColors",
					new DescriptionAttribute(SR.GetString("ActivityCategoryCreationDialog_DefaultCategoryColors_Property")),
				    new DisplayNameAttribute("DefaultCategoryColors")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ScheduleDialogBase`1_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // ActivityCategoryCreationDialog Properties

				#region ActivityRecurrenceDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityRecurrenceDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AllowModifications",
					new DescriptionAttribute(SR.GetString("ActivityRecurrenceDialogDisplayingEventArgs_AllowModifications_Property")),
				    new DisplayNameAttribute("AllowModifications")				);

				#endregion // ActivityRecurrenceDialogDisplayingEventArgs Properties

				#region DualModeTextBox Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DualModeTextBox");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsEditingAllowed",
					new DescriptionAttribute(SR.GetString("DualModeTextBox_IsEditingAllowed_Property")),
				    new DisplayNameAttribute("IsEditingAllowed")				);


				tableBuilder.AddCustomAttributes(t, "IsInEditMode",
					new DescriptionAttribute(SR.GetString("DualModeTextBox_IsInEditMode_Property")),
				    new DisplayNameAttribute("IsInEditMode")				);


				tableBuilder.AddCustomAttributes(t, "Text",
					new DescriptionAttribute(SR.GetString("DualModeTextBox_Text_Property")),
				    new DisplayNameAttribute("Text")				);

				#endregion // DualModeTextBox Properties

				#region ScheduleDialogBase`1 Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDialogBase`1");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ScheduleDialogBase`1_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // ScheduleDialogBase`1 Properties

				#region ActivityTypeChooserDialogDisplayingEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.ActivityTypeChooserDialogDisplayingEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AvailableTypes",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialogDisplayingEventArgs_AvailableTypes_Property")),
				    new DisplayNameAttribute("AvailableTypes")				);


				tableBuilder.AddCustomAttributes(t, "ActivityType",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialogDisplayingEventArgs_ActivityType_Property")),
				    new DisplayNameAttribute("ActivityType")				);


				tableBuilder.AddCustomAttributes(t, "Calendar",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialogDisplayingEventArgs_Calendar_Property")),
				    new DisplayNameAttribute("Calendar")				);


				tableBuilder.AddCustomAttributes(t, "Cancel",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialogDisplayingEventArgs_Cancel_Property")),
				    new DisplayNameAttribute("Cancel")				);


				tableBuilder.AddCustomAttributes(t, "ChooserReason",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialogDisplayingEventArgs_ChooserReason_Property")),
				    new DisplayNameAttribute("ChooserReason")				);

				#endregion // ActivityTypeChooserDialogDisplayingEventArgs Properties

				#region ActivityCategoryListItemPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityCategoryListItemPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ActivityCategoryListItemPresenter Properties

				#region ScheduleDialogCommandBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDialogCommandBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleDialogCommandBase Properties

				#region ScheduleDialogCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDialogCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("ScheduleDialogCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // ScheduleDialogCommandSource Properties

				#region ScheduleDialogSaveAndCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDialogSaveAndCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleDialogSaveAndCloseCommand Properties

				#region ScheduleDialogCloseCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ScheduleDialogCloseCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ScheduleDialogCloseCommand Properties

				#region ActivityTypeChooserDialog Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityTypeChooserDialog");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsAppointment",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsAppointment_Property")),
				    new DisplayNameAttribute("ChoiceIsAppointment")				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsAppointmentVisibility",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsAppointmentVisibility_Property")),
				    new DisplayNameAttribute("ChoiceIsAppointmentVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsJournal",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsJournal_Property")),
				    new DisplayNameAttribute("ChoiceIsJournal")				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsJournalVisibility",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsJournalVisibility_Property")),
				    new DisplayNameAttribute("ChoiceIsJournalVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsTask",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsTask_Property")),
				    new DisplayNameAttribute("ChoiceIsTask")				);


				tableBuilder.AddCustomAttributes(t, "ChoiceIsTaskVisibility",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChoiceIsTaskVisibility_Property")),
				    new DisplayNameAttribute("ChoiceIsTaskVisibility")				);


				tableBuilder.AddCustomAttributes(t, "ChooserReason",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_ChooserReason_Property")),
				    new DisplayNameAttribute("ChooserReason")				);


				tableBuilder.AddCustomAttributes(t, "DataManager",
					new DescriptionAttribute(SR.GetString("ActivityTypeChooserDialog_DataManager_Property")),
				    new DisplayNameAttribute("DataManager")				);


				tableBuilder.AddCustomAttributes(t, "LocalizedStrings",
					new DescriptionAttribute(SR.GetString("ScheduleDialogBase`1_LocalizedStrings_Property")),
				    new DisplayNameAttribute("LocalizedStrings")				);

				#endregion // ActivityTypeChooserDialog Properties

				#region DayToolTipInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayToolTipInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("DayToolTipInfo_Date_Property")),
				    new DisplayNameAttribute("Date")				);


				tableBuilder.AddCustomAttributes(t, "ActivityToolTipInfos",
					new DescriptionAttribute(SR.GetString("DayToolTipInfo_ActivityToolTipInfos_Property")),
				    new DisplayNameAttribute("ActivityToolTipInfos")				);

				#endregion // DayToolTipInfo Properties

				#region DayActivityToolTipInfo Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.DayActivityToolTipInfo");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "Activity",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_Activity_Property")),
				    new DisplayNameAttribute("Activity")				);


				tableBuilder.AddCustomAttributes(t, "Date",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_Date_Property")),
				    new DisplayNameAttribute("Date")				);


				tableBuilder.AddCustomAttributes(t, "ReminderVisibility",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_ReminderVisibility_Property")),
				    new DisplayNameAttribute("ReminderVisibility")				);


				tableBuilder.AddCustomAttributes(t, "Categories",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_Categories_Property")),
				    new DisplayNameAttribute("Categories")				);


				tableBuilder.AddCustomAttributes(t, "CategoryBackground",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_CategoryBackground_Property")),
				    new DisplayNameAttribute("CategoryBackground")				);


				tableBuilder.AddCustomAttributes(t, "CategoryForeground",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_CategoryForeground_Property")),
				    new DisplayNameAttribute("CategoryForeground")				);


				tableBuilder.AddCustomAttributes(t, "AlignEndTime",
					new DescriptionAttribute(SR.GetString("DayActivityToolTipInfo_AlignEndTime_Property")),
				    new DisplayNameAttribute("AlignEndTime")				);

				#endregion // DayActivityToolTipInfo Properties

				#region RelativeDateRangePresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.RelativeDateRangePresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ConvertDateTimeToLocal",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_ConvertDateTimeToLocal_Property")),
				    new DisplayNameAttribute("ConvertDateTimeToLocal")				);


				tableBuilder.AddCustomAttributes(t, "End",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_End_Property")),
				    new DisplayNameAttribute("End")				);


				tableBuilder.AddCustomAttributes(t, "RelativeDate",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_RelativeDate_Property")),
				    new DisplayNameAttribute("RelativeDate")				);


				tableBuilder.AddCustomAttributes(t, "Start",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_Start_Property")),
				    new DisplayNameAttribute("Start")				);


				tableBuilder.AddCustomAttributes(t, "IncludeEnd",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_IncludeEnd_Property")),
				    new DisplayNameAttribute("IncludeEnd")				);


				tableBuilder.AddCustomAttributes(t, "IncludeStart",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_IncludeStart_Property")),
				    new DisplayNameAttribute("IncludeStart")				);


				tableBuilder.AddCustomAttributes(t, "SizeToWidestTime",
					new DescriptionAttribute(SR.GetString("RelativeDateRangePresenter_SizeToWidestTime_Property")),
				    new DisplayNameAttribute("SizeToWidestTime")				);

				#endregion // RelativeDateRangePresenter Properties

				#region ContentControlWithAutomation Properties
				t = controlAssembly.GetType("Infragistics.Controls.Primitives.ContentControlWithAutomation");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AutomationControlType",
					new DescriptionAttribute(SR.GetString("ContentControlWithAutomation_AutomationControlType_Property")),
				    new DisplayNameAttribute("AutomationControlType")				);

				#endregion // ContentControlWithAutomation Properties

				#region ContentControlWithAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.ContentControlWithAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // ContentControlWithAutomationPeer Properties

				#region ActivityToolTipInfoPresenter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.Primitives.ActivityToolTipInfoPresenter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActivityToolTipInfo",
					new DescriptionAttribute(SR.GetString("ActivityToolTipInfoPresenter_ActivityToolTipInfo_Property")),
				    new DisplayNameAttribute("ActivityToolTipInfo"),
					new CategoryAttribute(SR.GetString("XamSchedule_Properties"))
				);

				#endregion // ActivityToolTipInfoPresenter Properties

				#region CustomScheduleColorScheme Properties
				t = controlAssembly.GetType("Infragistics.Controls.Schedules.CustomScheduleColorScheme");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "BaseColors",
					new DescriptionAttribute(SR.GetString("CustomScheduleColorScheme_BaseColors_Property")),
				    new DisplayNameAttribute("BaseColors")				);


				tableBuilder.AddCustomAttributes(t, "CustomBaseColors",
					new DescriptionAttribute(SR.GetString("CustomScheduleColorScheme_CustomBaseColors_Property")),
				    new DisplayNameAttribute("CustomBaseColors")				);


				tableBuilder.AddCustomAttributes(t, "ResourceOverrides",
					new DescriptionAttribute(SR.GetString("CustomScheduleColorScheme_ResourceOverrides_Property")),
				    new DisplayNameAttribute("ResourceOverrides")				);


				tableBuilder.AddCustomAttributes(t, "ResourceOverridesHighContrast",
					new DescriptionAttribute(SR.GetString("CustomScheduleColorScheme_ResourceOverridesHighContrast_Property")),
				    new DisplayNameAttribute("ResourceOverridesHighContrast")				);

				#endregion // CustomScheduleColorScheme Properties
                this.AddCustomAttributes(tableBuilder);
				return tableBuilder.CreateTable();
			}
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