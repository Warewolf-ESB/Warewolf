using System;
using System.ComponentModel;
using Microsoft.Windows.Design.Metadata;
using Microsoft.Windows.Design.PropertyEditing;
using System.Reflection;

[assembly: ProvideMetadata(typeof(InfragisticsWPF4.Controls.Editors.XamCalendar.Design.MetadataStore))]

namespace InfragisticsWPF4.Controls.Editors.XamCalendar.Design
{
	internal partial class MetadataStore : IProvideAttributeTable
	{
		public AttributeTable AttributeTable
		{
			get
			{
			    bool isVS = System.Diagnostics.Process.GetCurrentProcess().MainModule.ModuleName.Equals("devenv.exe"); 
				AttributeTableBuilder tableBuilder = new AttributeTableBuilder();
				Type t = typeof(Infragistics.Controls.Editors.XamCalendar);
				Assembly controlAssembly = t.Assembly;

				#region CalendarDay Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarDay");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "IsWorkday",
					new DescriptionAttribute(SR.GetString("CalendarDay_IsWorkday_Property")),
				    new DisplayNameAttribute("IsWorkday"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarDay Properties

				#region CalendarItem Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItem");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("CalendarItem_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBorderBrush",
					new DescriptionAttribute(SR.GetString("CalendarItem_ComputedBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedBorderBrush"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("CalendarItem_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedInnerBorderBrush",
					new DescriptionAttribute(SR.GetString("CalendarItem_ComputedInnerBorderBrush_Property")),
				    new DisplayNameAttribute("ComputedInnerBorderBrush"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContainsSelectedDates",
					new DescriptionAttribute(SR.GetString("CalendarItem_ContainsSelectedDates_Property")),
				    new DisplayNameAttribute("ContainsSelectedDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ContainsToday",
					new DescriptionAttribute(SR.GetString("CalendarItem_ContainsToday_Property")),
				    new DisplayNameAttribute("ContainsToday"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "EndDate",
					new DescriptionAttribute(SR.GetString("CalendarItem_EndDate_Property")),
				    new DisplayNameAttribute("EndDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsActive",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsActive_Property")),
				    new DisplayNameAttribute("IsActive"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsHighlighted",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsHighlighted_Property")),
				    new DisplayNameAttribute("IsHighlighted"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsLeadingOrTrailingItem",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsLeadingOrTrailingItem_Property")),
				    new DisplayNameAttribute("IsLeadingOrTrailingItem"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionActive",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsSelectionActive_Property")),
				    new DisplayNameAttribute("IsSelectionActive"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsToday",
					new DescriptionAttribute(SR.GetString("CalendarItem_IsToday_Property")),
				    new DisplayNameAttribute("IsToday"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "StartDate",
					new DescriptionAttribute(SR.GetString("CalendarItem_StartDate_Property")),
				    new DisplayNameAttribute("StartDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarItem Properties

				#region CalendarItemGroup Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItemGroup");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentMode",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_CurrentMode_Property")),
				    new DisplayNameAttribute("CurrentMode"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FirstDateOfGroup",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_FirstDateOfGroup_Property")),
				    new DisplayNameAttribute("FirstDateOfGroup"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsLeadingGroup",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_IsLeadingGroup_Property")),
				    new DisplayNameAttribute("IsLeadingGroup"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsTrailingGroup",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_IsTrailingGroup_Property")),
				    new DisplayNameAttribute("IsTrailingGroup"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LastDateOfGroup",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_LastDateOfGroup_Property")),
				    new DisplayNameAttribute("LastDateOfGroup"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceGroupOffset",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_ReferenceGroupOffset_Property")),
				    new DisplayNameAttribute("ReferenceGroupOffset"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowLeadingDates",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_ShowLeadingDates_Property")),
				    new DisplayNameAttribute("ShowLeadingDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollNextButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_ScrollNextButtonVisibility_Property")),
				    new DisplayNameAttribute("ScrollNextButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollPreviousButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_ScrollPreviousButtonVisibility_Property")),
				    new DisplayNameAttribute("ScrollPreviousButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ShowTrailingDates",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_ShowTrailingDates_Property")),
				    new DisplayNameAttribute("ShowTrailingDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Title",
					new DescriptionAttribute(SR.GetString("CalendarItemGroup_Title_Property")),
				    new DisplayNameAttribute("Title"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarItemGroup Properties

				#region CalendarDayOfWeek Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarDayOfWeek");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeek",
					new DescriptionAttribute(SR.GetString("CalendarDayOfWeek_DayOfWeek_Property")),
				    new DisplayNameAttribute("DayOfWeek"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisplayText",
					new DescriptionAttribute(SR.GetString("CalendarDayOfWeek_DisplayText_Property")),
				    new DisplayNameAttribute("DisplayText"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarDayOfWeek Properties

				#region XamCalendar Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.XamCalendar");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? true : !string.IsNullOrEmpty(SR.GetString("XamCalendarAssetLibrary")))
					,new Microsoft.Windows.Design.ToolboxCategoryAttribute(SR.GetString("XamCalendarAssetLibrary"))
				);


				tableBuilder.AddCustomAttributes(t, "DisabledDates",
					new DescriptionAttribute(SR.GetString("XamCalendar_DisabledDates_Property")),
				    new DisplayNameAttribute("DisabledDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisabledDaysOfWeek",
					new DescriptionAttribute(SR.GetString("XamCalendar_DisabledDaysOfWeek_Property")),
				    new DisplayNameAttribute("DisabledDaysOfWeek"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DisabledDaysOfWeekVisibility",
					new DescriptionAttribute(SR.GetString("XamCalendar_DisabledDaysOfWeekVisibility_Property")),
				    new DisplayNameAttribute("DisabledDaysOfWeekVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDate",
					new DescriptionAttribute(SR.GetString("XamCalendar_MaxDate_Property")),
				    new DisplayNameAttribute("MaxDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxSelectedDates",
					new DescriptionAttribute(SR.GetString("XamCalendar_MaxSelectedDates_Property")),
				    new DisplayNameAttribute("MaxSelectedDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinCalendarMode",
					new DescriptionAttribute(SR.GetString("XamCalendar_MinCalendarMode_Property")),
				    new DisplayNameAttribute("MinCalendarMode"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinDate",
					new DescriptionAttribute(SR.GetString("XamCalendar_MinDate_Property")),
				    new DisplayNameAttribute("MinDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FirstDayOfWeek",
					new DescriptionAttribute(SR.GetString("XamCalendar_FirstDayOfWeek_Property")),
				    new DisplayNameAttribute("FirstDayOfWeek"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectionMode",
					new DescriptionAttribute(SR.GetString("XamCalendar_SelectionMode_Property")),
				    new DisplayNameAttribute("SelectionMode"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TodayButtonVisibility",
					new DescriptionAttribute(SR.GetString("XamCalendar_TodayButtonVisibility_Property")),
				    new DisplayNameAttribute("TodayButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekRule",
					new DescriptionAttribute(SR.GetString("XamCalendar_WeekRule_Property")),
				    new DisplayNameAttribute("WeekRule"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Workdays",
					new DescriptionAttribute(SR.GetString("XamCalendar_Workdays_Property")),
				    new DisplayNameAttribute("Workdays"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // XamCalendar Properties

				#region CalendarBase Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.CalendarBase");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ActiveDate",
					new DescriptionAttribute(SR.GetString("CalendarBase_ActiveDate_Property")),
				    new DisplayNameAttribute("ActiveDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AllowLeadingAndTrailingGroupActivation",
					new DescriptionAttribute(SR.GetString("CalendarBase_AllowLeadingAndTrailingGroupActivation_Property")),
				    new DisplayNameAttribute("AllowLeadingAndTrailingGroupActivation"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AutoAdjustDimensions",
					new DescriptionAttribute(SR.GetString("CalendarBase_AutoAdjustDimensions_Property")),
				    new DisplayNameAttribute("AutoAdjustDimensions"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarDayStyle",
					new DescriptionAttribute(SR.GetString("CalendarBase_CalendarDayStyle_Property")),
				    new DisplayNameAttribute("CalendarDayStyle"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CalendarItemStyle",
					new DescriptionAttribute(SR.GetString("CalendarBase_CalendarItemStyle_Property")),
				    new DisplayNameAttribute("CalendarItemStyle"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "CurrentMode",
					new DescriptionAttribute(SR.GetString("CalendarBase_CurrentMode_Property")),
				    new DisplayNameAttribute("CurrentMode"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekHeaderFormat",
					new DescriptionAttribute(SR.GetString("CalendarBase_DayOfWeekHeaderFormat_Property")),
				    new DisplayNameAttribute("DayOfWeekHeaderFormat"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekHeaderVisibility",
					new DescriptionAttribute(SR.GetString("CalendarBase_DayOfWeekHeaderVisibility_Property")),
				    new DisplayNameAttribute("DayOfWeekHeaderVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Dimensions",
					new DescriptionAttribute(SR.GetString("CalendarBase_Dimensions_Property")),
				    new DisplayNameAttribute("Dimensions"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelectionActive",
					new DescriptionAttribute(SR.GetString("CalendarBase_IsSelectionActive_Property")),
				    new DisplayNameAttribute("IsSelectionActive"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeadingAndTrailingDatesVisibility",
					new DescriptionAttribute(SR.GetString("CalendarBase_LeadingAndTrailingDatesVisibility_Property")),
				    new DisplayNameAttribute("LeadingAndTrailingDatesVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxDateResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_MaxDateResolved_Property")),
				    new DisplayNameAttribute("MaxDateResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinCalendarModeResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_MinCalendarModeResolved_Property")),
				    new DisplayNameAttribute("MinCalendarModeResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MinDateResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_MinDateResolved_Property")),
				    new DisplayNameAttribute("MinDateResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ReferenceDate",
					new DescriptionAttribute(SR.GetString("CalendarBase_ReferenceDate_Property")),
				    new DisplayNameAttribute("ReferenceDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceProvider",
					new DescriptionAttribute(SR.GetString("CalendarBase_ResourceProvider_Property")),
				    new DisplayNameAttribute("ResourceProvider"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceProviderResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_ResourceProviderResolved_Property")),
				    new DisplayNameAttribute("ResourceProviderResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarBase_ScrollButtonVisibility_Property")),
				    new DisplayNameAttribute("ScrollButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollNextRepeatButtonStyle",
					new DescriptionAttribute(SR.GetString("CalendarBase_ScrollNextRepeatButtonStyle_Property")),
				    new DisplayNameAttribute("ScrollNextRepeatButtonStyle"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollNextRepeatButtonStyleResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_ScrollNextRepeatButtonStyleResolved_Property")),
				    new DisplayNameAttribute("ScrollNextRepeatButtonStyleResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollPreviousRepeatButtonStyle",
					new DescriptionAttribute(SR.GetString("CalendarBase_ScrollPreviousRepeatButtonStyle_Property")),
				    new DisplayNameAttribute("ScrollPreviousRepeatButtonStyle"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollPreviousRepeatButtonStyleResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_ScrollPreviousRepeatButtonStyleResolved_Property")),
				    new DisplayNameAttribute("ScrollPreviousRepeatButtonStyleResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedDate",
					new DescriptionAttribute(SR.GetString("CalendarBase_SelectedDate_Property")),
				    new DisplayNameAttribute("SelectedDate"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "SelectedDates",
					new DescriptionAttribute(SR.GetString("CalendarBase_SelectedDates_Property")),
				    new DisplayNameAttribute("SelectedDates"),
				    BrowsableAttribute.No,
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TodayButtonCaption",
					new DescriptionAttribute(SR.GetString("CalendarBase_TodayButtonCaption_Property")),
				    new DisplayNameAttribute("TodayButtonCaption"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TodayButtonStyle",
					new DescriptionAttribute(SR.GetString("CalendarBase_TodayButtonStyle_Property")),
				    new DisplayNameAttribute("TodayButtonStyle"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "TodayButtonStyleResolved",
					new DescriptionAttribute(SR.GetString("CalendarBase_TodayButtonStyleResolved_Property")),
				    new DisplayNameAttribute("TodayButtonStyleResolved"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekNumberVisibility",
					new DescriptionAttribute(SR.GetString("CalendarBase_WeekNumberVisibility_Property")),
				    new DisplayNameAttribute("WeekNumberVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Today",
					new DescriptionAttribute(SR.GetString("CalendarBase_Today_Property")),
				    new DisplayNameAttribute("Today"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarBase Properties

				#region CalendarDateRangeCollection Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarDateRangeCollection");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarDateRangeCollection Properties

				#region CalendarWeekNumberPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarWeekNumberPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarWeekNumberPanel Properties

				#region CalendarItemGroupAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarItemGroupAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarItemGroupAutomationPeer Properties

				#region CalendarDayOfWeekPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarDayOfWeekPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarDayOfWeekPanel Properties

				#region CalendarCommand Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarCommand");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarCommand Properties

				#region CalendarCommandSource Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarCommandSource");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "CommandType",
					new DescriptionAttribute(SR.GetString("CalendarCommandSource_CommandType_Property")),
				    new DisplayNameAttribute("CommandType")				);

				#endregion // CalendarCommandSource Properties

				#region CalendarWeekNumber Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarWeekNumber");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("CalendarWeekNumber_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "IsSelected",
					new DescriptionAttribute(SR.GetString("CalendarWeekNumber_IsSelected_Property")),
				    new DisplayNameAttribute("IsSelected"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarWeekNumber Properties

				#region CalendarItemArea Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItemArea");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedItemsBorderThickness",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_ComputedItemsBorderThickness_Property")),
				    new DisplayNameAttribute("ComputedItemsBorderThickness"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "DayOfWeekHeaderVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_DayOfWeekHeaderVisibility_Property")),
				    new DisplayNameAttribute("DayOfWeekHeaderVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Items",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_Items_Property")),
				    new DisplayNameAttribute("Items"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FirstItemColumnOffset",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_FirstItemColumnOffset_Property")),
				    new DisplayNameAttribute("FirstItemColumnOffset"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "FirstItemRowOffset",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_FirstItemRowOffset_Property")),
				    new DisplayNameAttribute("FirstItemRowOffset"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemColumns",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_ItemColumns_Property")),
				    new DisplayNameAttribute("ItemColumns"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ItemRows",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_ItemRows_Property")),
				    new DisplayNameAttribute("ItemRows"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "WeekNumberVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemArea_WeekNumberVisibility_Property")),
				    new DisplayNameAttribute("WeekNumberVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarItemArea Properties

				#region CalendarResourceProvider Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.CalendarResourceProvider");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ResourceSet",
					new DescriptionAttribute(SR.GetString("CalendarResourceProvider_ResourceSet_Property")),
				    new DisplayNameAttribute("ResourceSet"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarResourceProvider Properties

				#region SelectedDatesChangedEventArgs Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.SelectedDatesChangedEventArgs");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "RemovedDates",
					new DescriptionAttribute(SR.GetString("SelectedDatesChangedEventArgs_RemovedDates_Property")),
				    new DisplayNameAttribute("RemovedDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "AddedDates",
					new DescriptionAttribute(SR.GetString("SelectedDatesChangedEventArgs_AddedDates_Property")),
				    new DisplayNameAttribute("AddedDates"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // SelectedDatesChangedEventArgs Properties

				#region CalendarItemGroupPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItemGroupPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "AutoAdjustDimensions",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_AutoAdjustDimensions_Property")),
				    new DisplayNameAttribute("AutoAdjustDimensions"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "Dimensions",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_Dimensions_Property")),
				    new DisplayNameAttribute("Dimensions"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "HorizontalContentAlignment",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_HorizontalContentAlignment_Property")),
				    new DisplayNameAttribute("HorizontalContentAlignment"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GroupHeight",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_GroupHeight_Property")),
				    new DisplayNameAttribute("GroupHeight"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "GroupWidth",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_GroupWidth_Property")),
				    new DisplayNameAttribute("GroupWidth"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "LeadingAndTrailingDatesVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_LeadingAndTrailingDatesVisibility_Property")),
				    new DisplayNameAttribute("LeadingAndTrailingDatesVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "MaxGroups",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_MaxGroups_Property")),
				    new DisplayNameAttribute("MaxGroups"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "ScrollButtonVisibility",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_ScrollButtonVisibility_Property")),
				    new DisplayNameAttribute("ScrollButtonVisibility"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);


				tableBuilder.AddCustomAttributes(t, "VerticalContentAlignment",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupPanel_VerticalContentAlignment_Property")),
				    new DisplayNameAttribute("VerticalContentAlignment"),
					new CategoryAttribute(SR.GetString("Calendar_Properties"))
				);

				#endregion // CalendarItemGroupPanel Properties

				#region CalendarItemAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarItemAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarItemAutomationPeer Properties

				#region HorizontalToTextAlignmentConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.HorizontalToTextAlignmentConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // HorizontalToTextAlignmentConverter Properties

				#region CalendarResourceString Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarResourceString");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarResourceString Properties

				#region CalendarDimensionsConverter Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarDimensionsConverter");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarDimensionsConverter Properties

				#region CalendarItemGroupTitle Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItemGroupTitle");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);


				tableBuilder.AddCustomAttributes(t, "ComputedBackground",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupTitle_ComputedBackground_Property")),
				    new DisplayNameAttribute("ComputedBackground")				);


				tableBuilder.AddCustomAttributes(t, "ComputedForeground",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupTitle_ComputedForeground_Property")),
				    new DisplayNameAttribute("ComputedForeground")				);


				tableBuilder.AddCustomAttributes(t, "Group",
					new DescriptionAttribute(SR.GetString("CalendarItemGroupTitle_Group_Property")),
				    new DisplayNameAttribute("Group")				);

				#endregion // CalendarItemGroupTitle Properties

				#region CalendarItemAreaPanel Properties
				t = controlAssembly.GetType("Infragistics.Controls.Editors.Primitives.CalendarItemAreaPanel");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarItemAreaPanel Properties

				#region CalendarBaseAutomationPeer Properties
				t = controlAssembly.GetType("Infragistics.AutomationPeers.CalendarBaseAutomationPeer");
				tableBuilder.AddCustomAttributes(t,  new Microsoft.Windows.Design.ToolboxBrowsableAttribute( isVS ? false : !string.IsNullOrEmpty(""))
				);

				#endregion // CalendarBaseAutomationPeer Properties
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