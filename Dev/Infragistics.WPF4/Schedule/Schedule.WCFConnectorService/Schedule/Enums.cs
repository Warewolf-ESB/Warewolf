using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.Primitives;


#pragma warning disable 1574

namespace Infragistics.Controls.Schedules.Services



{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


	#region ActivityFeature Enum

	/// <summary>
	/// Used to specify which activityfeatures are supported.
	/// </summary>
	/// <seealso cref="ScheduleDataConnectorBase.IsActivityFeatureSupported"/>
	public enum ActivityFeature
	{
		/// <summary>
		/// Recurring activities.
		/// </summary>
		Recurrence,

		/// <summary>
		/// Modifying an occurrence.
		/// </summary>
		Variance,

		/// <summary>
		/// Making an activity time-zone neutral.
		/// </summary>
		TimeZoneNeutrality,

		/// <summary>
		/// Reminders.
		/// </summary>
		Reminder,

		/// <summary>
		/// Whether owning calendar of an activity can be changed.
		/// </summary>
		CanChangeOwningCalendar,

		/// <summary>
		/// Whether owning resource of an activity can be changed.
		/// </summary>
		CanChangeOwningResource,

		/// <summary>
		/// Whether the activity supports end time.
		/// </summary>
		EndTime
	}

	#endregion // ActivityFeature Enum

	#region ActivityOperation Enum

	/// <summary>
	/// Enum that lists operations that can be performed on an activity.
	/// </summary>
	/// <seealso cref="XamScheduleDataManager.IsActivityOperationAllowed( ActivityBase, ActivityOperation )"/>
	/// <seealso cref="XamScheduleDataManager.IsActivityOperationAllowed( ActivityType, ActivityOperation, ResourceCalendar )"/>
	public enum ActivityOperation
	{
		/// <summary>
		/// Whether an activity can be modified.
		/// </summary>
		Edit,

		/// <summary>
		/// Wheher an activity can be added.
		/// </summary>
		Add,

		/// <summary>
		/// Whether an activity can be removed.
		/// </summary>
		Remove
	}

	#endregion // ActivityOperation Enum

	#region ActivityQueryRequestedDataFlags Enum

	/// <summary>
	/// Used to specify <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.RequestedInformation"/> property.
	/// </summary>
	[Flags]
	public enum ActivityQueryRequestedDataFlags
	{
		/// <summary>
		/// No information will be retrieved.
		/// </summary>
		None = 0x0,

		/// <summary>
		/// Activities that intersect with one of the date ranges in the <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.DateRanges"/> are to be retrieved.
		/// </summary>
		ActivitiesWithinDateRanges = 0x1,

		/// <summary>
		/// Whether an activity exists before the earliest of the dates in the <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.DateRanges"/> is to be ascertained.
		/// </summary>
		HasPreviousActivity = 0x2,

		/// <summary>
		/// Whether an activity exists after the latest of the dates in the <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.DateRanges"/> is to be ascertained.
		/// </summary>
		HasNextActivity = 0x4,

		/// <summary>
		/// The latest activity before the earliest of the dates in the <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.DateRanges"/> is to be retrieved.
		/// </summary>
		PreviousActivity = 0x8,

		/// <summary>
		/// The earliest activity after the latest of the dates in the <see cref="ActivityQuery"/>'s <see cref="ActivityQuery.DateRanges"/> is to be retrieved.
		/// </summary>
		NextActivity = 0x10,

		/// <summary>
		/// All of above.
		/// </summary>
		All = 0xff
	}

	#endregion // ActivityQueryRequestedDataFlags Enum

	#region ActivityType Enum

	/// <summary>
	/// Enum that lists the four activity types.
	/// </summary>
	[Flags]
	public enum ActivityType : byte
	{
		/// <summary>
		/// Appointment activity.
		/// </summary>
		Appointment = 0,

		/// <summary>
		/// Journal activity.
		/// </summary>
		Journal = 1,

		/// <summary>
		/// Task activity.
		/// </summary>
		Task = 2
	}

	#endregion // ActivityType Enum

	#region ActivityTypes Enum

	/// <summary>
	/// Flagged enum that lists the four activity types.
	/// </summary>
	[Flags]
	public enum ActivityTypes
	{
		/// <summary>
		/// None.
		/// </summary>
		None		= 0x0,

		/// <summary>
		/// Appointment activity.
		/// </summary>
		Appointment	= 1 << (int)ActivityType.Appointment,

		/// <summary>
		/// Journal activity.
		/// </summary>
		Journal		= 1 << (int)ActivityType.Journal,

		/// <summary>
		/// Task activity.
		/// </summary>
		Task		= 1 << (int)ActivityType.Task,

		/// <summary>
		/// All activity types.
		/// </summary>
		All			= 0xf
	}

	#endregion // ActivityTypes Enum



#region Infragistics Source Cleanup (Region)





































































































































































































































#endregion // Infragistics Source Cleanup (Region)


	// MD 1/5/11 - NA 11.1 - Exchange Data Connector
	#region DescriptionFormat

	internal enum DescriptionFormat
	{
		Default = 0,
		HTML,
		Text,
		Unknown
	} 

	#endregion  // DescriptionFormat



#region Infragistics Source Cleanup (Region)












































#endregion // Infragistics Source Cleanup (Region)


	#region ListChangeType Enum

	internal enum ListChangeType : byte
	{
		AddItem,
		ChangeItem,
		RemoveItem,
		Reset
	}

	#endregion  // ListChangeType Enum

	#region OfficeColorScheme

	/// <summary>
	/// Determines the overall color scheme to use for all controls associated with a specific <see cref="XamScheduleDataManager"/>
	/// </summary>
	public enum OfficeColorScheme
	{
		/// <summary>
		/// This corresponds to the Office blue color scheme. This is the default.
		/// </summary>
		Blue,

		/// <summary>
		/// This corresponds to the Office black color scheme. 
		/// </summary>
		Black,

		/// <summary>
		/// This corresponds to the Office silver color scheme. 
		/// </summary>
		Silver
	}

	#endregion //OfficeColorScheme	



#region Infragistics Source Cleanup (Region)
















































































































































































#endregion // Infragistics Source Cleanup (Region)


	#region ResourceFeature Enum

	// SSP 2/25/2011 - NAS11.1 Activity Categories
	// 
	/// <summary>
	/// Used to specify which resource features are supported.
	/// </summary>
	/// <seealso cref="ScheduleDataConnectorBase.IsResourceFeatureSupported"/>



	public enum ResourceFeature
	{
		/// <summary>
		/// Feature indicates whether the resource supports activity category customization.
		/// </summary>
		CustomActivityCategories
	} 

	#endregion // ResourceFeature Enum



#region Infragistics Source Cleanup (Region)






























































































































































































































































#endregion // Infragistics Source Cleanup (Region)




#region Infragistics Source Cleanup (Region)





































































































































































































































































#endregion // Infragistics Source Cleanup (Region)

}


namespace Infragistics.Controls.Schedules.Primitives.Services



{


#region Infragistics Source Cleanup (Region)

































#endregion // Infragistics Source Cleanup (Region)


	#region CalendarBrushId

	/// <summary>
	/// Identifies a specific brush
	/// </summary>
	/// <seealso cref="CalendarBrushProvider.GetBrush(CalendarBrushId)"/>
	public enum CalendarBrushId : byte
	{
		/// <summary>
		/// The background for the dropdown portion of the <see cref="ActivityCategoryColorPicker"/>
		/// </summary>
		ActivityCategoryColorPickerDropDownBackground, // AS 6/13/12 TFS105402
		/// <summary>
		/// The border for the dropdown portion of the <see cref="ActivityCategoryColorPicker"/>
		/// </summary>
		ActivityCategoryColorPickerDropDownBorder, // AS 6/13/12 TFS105402
		/// <summary>
		/// Background for days in alternate months
		/// </summary>
		AlternateMonthDayBackground,
		/// <summary>
		/// Background for appointments
		/// </summary>
		AppointmentBackground,
		/// <summary>
		/// Border around appointments
		/// </summary>
		AppointmentBorder,
		/// <summary>
		/// Foreground for appointment date times
		/// </summary>
		AppointmentDateTimeForeground,
		/// <summary>
		/// Foreground for appointments
		/// </summary>
		AppointmentForeground,
		/// <summary>
		/// Foreground for appointments that are not owned by the selected calendar.
		/// </summary>
		AppointmentForegroundOverlayed,
		/// <summary>
		/// Background for a blocking error message
		/// </summary>
		BlockingErrorBackground,
		/// <summary>
		/// Foreground for a blocking error message
		/// </summary>
		BlockingErrorForeground,
		/// <summary>
		/// Foreground for a blocking error message header
		/// </summary>
		BlockingErrorHeaderForeground,
		/// <summary>
		/// Border around calendars
		/// </summary>
		CalendarBorder,
		/// <summary>
		/// Background for calendar headers
		/// </summary>
		CalendarHeaderBackground,
		/// <summary>
		/// Foreground for calendar headers
		/// </summary>
		CalendarHeaderForeground,
		/// <summary>
		/// Border around the currect day
		/// </summary>
		CurrentDayBorder,
		/// <summary>
		/// Border around the currect day in the XamMonthCalendar
		/// </summary>
		CurrentDayBorderMonthCalendar,
		/// <summary>
		/// Background for the current day header
		/// </summary>
		CurrentDayHeaderBackground,
		/// <summary>
		/// Foreground for the current day header
		/// </summary>
		CurrentDayHeaderForeground,
		/// <summary>
		/// Backfround for the current time
		/// </summary>
		CurrentTimeIndicatorBackground,
		/// <summary>
		/// Border around the current time
		/// </summary>
		CurrentTimeIndicatorBorder,
		/// <summary>
		/// Background for days 
		/// </summary>
		DayBackground,
		/// <summary>
		/// Border for days 
		/// </summary>
		DayBorder,
		/// <summary>
		/// Background for day headers 
		/// </summary>
		DayHeaderBackground, 
		/// <summary>
		/// Foreground for day headers 
		/// </summary>
		DayHeaderForground, 
		/// <summary>
		/// Background for XamSchedule dialogs 
		/// </summary>
		DialogBackground,
		/// <summary>
		/// Foreground for XamSchedule dialogs 
		/// </summary>
		DialogForeground,
		/// <summary>
		/// Brush used to fill the <see cref="MoreActivityIndicator"/> when the mouse is over the element
		/// </summary>
		HotTrackingMoreActivityIndicatorFill,
		/// <summary>
		/// Background for journals 
		/// </summary>
		JournalBackground,
		/// <summary>
		/// Border around journals 
		/// </summary>
		JournalBorder,
		/// <summary>
		/// Foreground for journal date times
		/// </summary>
		JournalDateTimeForeground,
		/// <summary>
		/// Foreground for journals 
		/// </summary>
		JournalForeground,
		/// <summary>
		/// Foreground for journal entries that are not owned by the selected calendar.
		/// </summary>
		JournalForegroundOverlayed,
		/// <summary>
		/// Brush used to fill the <see cref="MoreActivityIndicator"/>
		/// </summary>
		MoreActivityIndicatorFill,
		/// <summary>
		/// Background for <see cref="XamMonthView"/>
		/// </summary>
		MonthViewBackground,
		/// <summary>
		/// Background for the day of week header in <see cref="XamMonthView"/>
		/// </summary>
		MonthViewDayOfWeekHeaderBackground,
		/// <summary>
		/// Border for the day of week header in <see cref="XamMonthView"/>
		/// </summary>
		MonthViewDayOfWeekHeaderBorder,
		/// <summary>
		/// Foreground for the day of week header in <see cref="XamMonthView"/>
		/// </summary>
		MonthViewDayOfWeekHeaderForeground,
		/// <summary>
		/// Brush used to draw the edge of the <see cref="MoreActivityIndicator"/>
		/// </summary>
		MoreActivityIndicatorStroke,
		/// <summary>
		/// Background for the multi day activity area
		/// </summary>
		MultiDayActivityAreaBackground,

		/// <summary>
		/// Default foreground for the navigation buttons in a <see cref="XamOutlookCalendarView"/>
		/// </summary>
		NavigationButtonForeground, // AS 12/8/10 NA 11.1 - XamOutlookCalendarView
		/// <summary>
		/// Default background for the navigation buttons in a <see cref="XamOutlookCalendarView"/>
		/// </summary>
		NavigationButtonBackground, // AS 12/8/10 NA 11.1 - XamOutlookCalendarView
		/// <summary>
		/// Background for the navigation buttons in a <see cref="XamOutlookCalendarView"/> when the mouse is over the element
		/// </summary>
		NavigationButtonHoverBackground, // AS 12/8/10 NA 11.1 - XamOutlookCalendarView
		/// <summary>
		/// Background for the navigation buttons in a <see cref="XamOutlookCalendarView"/> when pressed.
		/// </summary>
		NavigationButtonPressedBackground, // AS 12/8/10 NA 11.1 - XamOutlookCalendarView

		/// <summary>
		/// Background for timeslots that are not within the working hour range
		/// </summary>
		NonWorkingHourTimeslotBackground,
		/// <summary>
		/// Border brush for timeslots that are not on an hour border and are not within the working hour range
		/// </summary>
		NonWorkingHourTimeslotMinorBorder,
		/// <summary>
		/// The background for the <see cref="ScheduleResizerBar"/> preview shown during a resize of a CalendarGroup.
		/// </summary>
		ResizerBarPreviewBackground, // AS 4/15/11 NA 11.1 - CalendarHeaderAreaWidth
		/// <summary>
		/// Background brush for the ActivityDialogRibbonLite
		/// </summary>
		RibbonLiteBackgroundBrush,
		/// <summary>
		/// Border brush for Group outer borders in the ActivityDialogRibbonLite 
		/// </summary>
		RibbonLiteGroupOuterBorderBrush,
		/// <summary>
		/// Border brush for Group inner borders in the ActivityDialogRibbonLite 
		/// </summary>
		RibbonLiteGroupInnerBorderBrush,
		/// <summary>
		/// Border brush for selected activities
		/// </summary>
		SelectedActivityBorder,
		/// <summary>
		/// Background for selected all day event area 
		/// </summary>
		SelectedMultiDayActivityAreaBackground,
		/// <summary>
		/// Background for selected days in the XamMonthView
		/// </summary>
		SelectedDayBackgroundMonthView,
		/// <summary>
		/// Background for selected timeslots 
		/// </summary>
		SelectedTimeslotBackground,
		/// <summary>
		/// Background for tasks 
		/// </summary>
		TaskBackground,
		/// <summary>
		/// Border around tasks 
		/// </summary>
		TaskBorder,
		/// <summary>
		/// Foreground for task date times
		/// </summary>
		TaskDateTimeForeground,
		/// <summary>
		/// Foreground for tasks 
		/// </summary>
		TaskForeground,
		/// <summary>
		/// Foreground for tasks that are not owned by the selected calendar.
		/// </summary>
		TaskForegroundOverlayed,
		/// <summary>
		/// Border brush for timeslots that are on an hour border 
		/// </summary>
		TimeslotMajorBorder,
		/// <summary>
		/// Border brush for timeslots that border a day 
		/// </summary>
		TimeslotDayBorder,
		// AS 7/21/10 TFS36040
		// Added background colors for header area.
		//
		/// <summary>
		/// Brush for background of the <see cref="DayViewTimeslotHeaderArea"/>
		/// </summary>
		TimeslotHeaderAreaBackgroundDayView,
		/// <summary>
		/// Brush for background of the <see cref="ScheduleViewTimeslotHeaderArea"/>
		/// </summary>
		TimeslotHeaderAreaBackgroundScheduleView,
		/// <summary>
		/// The border separating the <see cref="TimeslotHeaderArea"/> in a <see cref="ScheduleTimeControlBase"/>
		/// </summary>
		TimeslotHeaderAreaSeparator,
		/// <summary>
		/// Brush for timeslot Foreground in the XamDayView
		/// </summary>
		TimeslotHeaderForegroundDayView,
		/// <summary>
		/// Brush for timeslot Foreground in the XamScheduleView
		/// </summary>
		TimeslotHeaderForegroundScheduleView,
		/// <summary>
		/// Brush for timeslot Tickmarks in the XamDayView
		/// </summary>
		TimeslotHeaderTickmarkDayView,
		/// <summary>
		/// Brush for timeslot Tickmarks in the XamScheduleView
		/// </summary>
		TimeslotHeaderTickmarkScheduleView,
		/// <summary>
		/// Background for tooltips
		/// </summary>
		ToolTipBackground,
		/// <summary>
		/// Brush for the border of tooltips
		/// </summary>
		ToolTipBorder,
		/// <summary>
		/// Foreground for error text in tooltips
		/// </summary>
		ToolTipErrorForeground,
		/// <summary>
		/// Foreground for tooltips
		/// </summary>
		ToolTipForeground,
		/// <summary>
		/// Background of week headers 
		/// </summary>
		WeekHeaderBackground,
		/// <summary>
		/// Border around week headers 
		/// </summary>
		WeekHeaderBorder,
		/// <summary>
		/// Foreground for week headers 
		/// </summary>
		WeekHeaderForeground,
		/// <summary>
		/// Background for timeslots that are within the working hour range
		/// </summary>
		WorkingHourTimeslotBackground,
		/// <summary>
		/// Border brush for timeslots that are not on an hour border and are within the working hour range
		/// </summary>
		WorkingHourTimeslotMinorBorder,
	}

	#endregion //CalendarBrushId



#region Infragistics Source Cleanup (Region)




















































































































































































































































#endregion // Infragistics Source Cleanup (Region)

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