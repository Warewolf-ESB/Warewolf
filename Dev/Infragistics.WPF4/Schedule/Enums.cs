using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules.Primitives;






namespace Infragistics.Controls.Schedules

{

	// AS 1/5/11 NA 11.1 Activity Categories
	#region ActivityCategoryBrushId
	internal enum ActivityCategoryBrushId
	{
		Background,
		Foreground,
		ForegroundOverlay,
		Border
	} 
	#endregion // ActivityCategoryBrushId


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


	#region ActivityTypeChooserReason
	/// <summary>
	/// Enumeration indicating the reason for which the <see cref="ActivityTypeChooserDialog"/> is being displayed.
	/// </summary>
	/// <seealso cref="XamScheduleDataManager.DisplayActivityTypeChooserDialog"/>
	/// <seealso cref="ActivitySettings.IsAddViaDoubleClickEnabled"/>
	/// <seealso cref="ActivitySettings.IsAddViaClickToAddEnabled"/>
	/// <seealso cref="ActivitySettings.IsAddViaTypingEnabled"/>
	public enum ActivityTypeChooserReason
	{
		/// <summary>
		/// An activity is to be added as a result of double clicking on a timeslot or timeslot header area.
		/// </summary>
		AddActivityViaDoubleClick,

		/// <summary>
		/// An activity is to be added as a result of clicking on the <see cref="ClickToAddActivityElement"/>.
		/// </summary>
		AddActivityViaClickToAdd,

		/// <summary>
		/// An activity is to be added as a result of typing while one or more timeslots are selected.
		/// </summary>
		AddActivityViaTyping,
	} 
	#endregion //ActivityTypeChooserReason

	#region AllowActivityDragging Enum

	/// <summary>
	/// Used to specify <see cref="ActivitySettings.AllowDragging"/> property.
	/// </summary>
	/// <seealso cref="ActivitySettings.AllowDragging"/>
	public enum AllowActivityDragging
	{
		/// <summary>
		/// Activity dragging is not allowed.
		/// </summary>
		No,

		/// <summary>
		/// Activity can be dragged within the same calendar.
		/// </summary>
		WithinCalendar,

		/// <summary>
		/// Activity can be dragged within the same resource's calendars.
		/// </summary>
		WithinResource,

		/// <summary>
		/// Activity can be dragged to a different resource' calendar.
		/// </summary>
		AcrossResources
	}

	#endregion // AllowActivityDragging Enum

	#region AllowActivityResizing
	/// <summary>
	/// Indicates whether an activity may be resized in the ui and if so what types of resizing are allowed.
	/// </summary>
	/// <seealso cref="ActivitySettings.AllowResizing"/>
	public enum AllowActivityResizing
	{
		/// <summary>
		/// The activity cannot be resized in the ui.
		/// </summary>
		No,

		/// <summary>
		/// Only the <see cref="ActivityBase.Start"/> may be adjusted.
		/// </summary>
		Start,

		/// <summary>
		/// Only the <see cref="ActivityBase.End"/> may be adjusted thereby only changing the duration.
		/// </summary>
		End,

		/// <summary>
		/// The end user may resize the leading or trailing edge and therefore change either the <see cref="ActivityBase.Start"/> or <see cref="ActivityBase.End"/>.
		/// </summary>
		StartAndEnd,
	} 
	#endregion // AllowActivityResizing

	#region ActivityCategoryCommand
	/// <summary>
	/// An enumeration of available commands that apply to <see cref="ActivityCategory"/>s.
	/// </summary>
	public enum ActivityCategoryCommand
	{
		/// <summary>
		/// Clear all Activity Categories assigned to an Activity.
		/// </summary>
		ClearAllActivityCategories,

		/// <summary>
		/// Displays a dialog that shows all valid ActivityCategories for an Activity and which allows editing of those categories.
		/// </summary>
		DisplayActivityCategoriesDialog,

		/// <summary>
		/// Toggles the selected state of an ActivityCategory for an Activity.
		/// </summary>
		ToggleActivityCategorySelectedState,

		/// <summary>
		/// Clears the selected state of an ActivityCategory for an Activity.
		/// </summary>
		ClearActivityCategorySelectedState,

		/// <summary>
		/// Sets the selected state of an ActivityCategory for an Activity.
		/// </summary>
		SetActivityCategorySelectedState,
	}
	#endregion //ActivityCategoryCommand

	#region ActivityCategoryDialogCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="ActivityCategoryDialog"/> object.
	/// </summary>
	public enum ActivityCategoryDialogCommand
	{
		/// <summary>
		/// Saves the <see cref="ActivityCategory"/> selection changes to the associated activities and closes the <see cref="ActivityCategoryDialog"/> object that is hosting it. 
		/// </summary>
		SaveAndClose,

		/// <summary>
		/// Closes the <see cref="ActivityCategoryDialog"/> object without saving ActivityCategory selection changes.
		/// </summary>
		Close,

		/// <summary>
		/// Creates a new <see cref="ActivityCategory"/>.
		/// </summary>
		CreateNewCategory,

		/// <summary>
		/// Edits the selected <see cref="ActivityCategory"/>.
		/// </summary>
		EditSelectedCategory,

		/// <summary>
		/// Deletes the selected <see cref="ActivityCategory"/>.
		/// </summary>
		DeleteSelectedCategory,
	}
	#endregion //ActivityCategoryDialogCommand

	#region ActivityDialogCoreCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public enum ActivityDialogCoreCommand
	{
		/// <summary>
		/// Saves the <see cref="ActivityBase"/> and closes the <see cref="ActivityDialogCore"/> object that is hosting it. 
		/// </summary>
		SaveAndClose,

		/// <summary>
		/// Closes the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		Close,

		/// <summary>
		/// Deletes the <see cref="ActivityBase"/> currently being edited in the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		Delete,

		/// <summary>
		/// Displays the <see cref="ActivityBase"/> Recurrence dialog.
		/// </summary>
		DisplayRecurrenceDialog,

		/// <summary>
		/// Shows the time zone pickers.
		/// </summary>
		ShowTimeZonePickers,

		/// <summary>
		/// Hides the time zone pickers.
		/// </summary>
		HideTimeZonePickers,
	}
	#endregion //ActivityDialogCoreCommand

	#region CalendarDisplayMode
	/// <summary>
	/// Determines how calendars are displayed within a <see cref="ScheduleControlBase"/>
	/// </summary>
	/// <seealso cref="ScheduleControlBase.CalendarDisplayMode"/>
	public enum CalendarDisplayMode
	{
		/// <summary>
		/// The calendars are displayed in groups based on the <see cref="XamScheduleDataManager.CalendarGroups"/>
		/// </summary>
		Overlay,

		/// <summary>
		/// Each visible <see cref="ResourceCalendar"/> in the <see cref="CalendarGroupBase.VisibleCalendars"/> of the <see cref="XamScheduleDataManager.CalendarGroups"/> is displayed as a separate group.
		/// </summary>
		Separate,

		/// <summary>
		/// All of the visible <see cref="ResourceCalendar"/> instances in the <see cref="CalendarGroupBase.VisibleCalendars"/> of the <see cref="XamScheduleDataManager.CalendarGroups"/> are displayed in a single group.
		/// </summary>
		Merged,
	} 
	#endregion // CalendarDisplayMode

	#region CalendarHeaderCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="CalendarHeader"/> object.
	/// </summary>
	public enum CalendarHeaderCommand
	{
		/// <summary>
		/// Closes the <see cref="CalendarHeader"/> object.
		/// </summary>
		Close,
		/// <summary>
		/// Toggles the <see cref="CalendarHeader"/> object between 'OverLay' and 'Side-By-Side' mode.
		/// </summary>
		ToggleOverlayMode,

	}
	#endregion //CalendarHeaderCommand


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


	#region EditableActivityProperty

	/// <summary>
	/// Represents properties can be edited or read-only depending on the state of an activity.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

	public enum EditableActivityProperty
	{
		/// <summary>
		/// The <see cref="ActivityBase.Recurrence"/> property.
		/// </summary>
		Recurrence,
	} 

	#endregion  // EditableActivityProperty

	#region HighlightDayCriteria

	/// <summary>
	/// Determines which days, if any, are highlighted.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public enum HighlightDayCriteria
	{
		/// <summary>
		/// Only highlight days that have activity.
		/// </summary>
		DaysWithActivity,
		/// <summary>
		/// Highlight all workdays whether or not they have activity.
		/// </summary>
		Workdays,
		/// <summary>
		/// Highlight all days.
		/// </summary>
		All,
		/// <summary>
		/// Don't highlight any days.
		/// </summary>
		None,
	}

	#endregion  // HighlightDayCriteria


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


	#region PromptForLocalTimeZone

	/// <summary>
	/// Determines if a dialog will be displayed asking the user to select a local time zone.
	/// </summary>
	/// <seealso cref="XamScheduleDataManager.PromptForLocalTimeZone"/>
	public enum PromptForLocalTimeZone
	{
		/// <summary>
		/// The <see cref="TimeZoneChooserDialog"/> will be displayed when the <see cref="XamScheduleDataManager"/> is initialized if the local time zone has not been specified or deduced.
		/// </summary>
		OnlyIfRequired,

		/// <summary>
		/// The <see cref="TimeZoneChooserDialog"/> will be displayed every time the <see cref="XamScheduleDataManager"/> is initialized.
		/// </summary>
		Always,

		/// <summary>
		/// A dialog is never displayed
		/// </summary>
		Never
	}

	#endregion //PromptForLocalTimeZone	

	#region RecurrenceChooserAction
	/// <summary>
	/// An enumeration used to determine whether the dialog should be displayed and if so which action should be taken.
	/// </summary>
	public enum RecurrenceChooserAction
	{
		/// <summary>
		/// Display a dialog to the end user to allow them to choose between changing the occurrence or the series.
		/// </summary>
		Prompt,

		/// <summary>
		/// Do not prompt but continue with the operation using the recurrence root.
		/// </summary>
		Series,

		/// <summary>
		/// Do not prompt but continue with the operation using the occurrence.
		/// </summary>
		Occurrence,

		/// <summary>
		/// Do not prompt and do not continue with the operation.
		/// </summary>
		Cancel,
	}
	#endregion // RecurrenceChooserAction

	#region RecurrenceChooserChoice
	/// <summary>
	/// An enumeration indicating the choice that was made in the <see cref="ActivityRecurrenceChooserDialog"/>.
	/// </summary>
	public enum RecurrenceChooserChoice
	{
		/// <summary>
		/// No choice was made
		/// </summary>
		None,

		/// <summary>
		/// Series was chosen.
		/// </summary>
		Series,

		/// <summary>
		/// Occurrence was chosen.
		/// </summary>
		Occurrence
	}
	#endregion //RecurrenceChooserChoice

	#region RecurrenceChooserType
	/// <summary>
	/// An enumeration of the different types of <see cref="ActivityRecurrenceChooserDialog"/>s.
	/// </summary>
	public enum RecurrenceChooserType
	{
		/// <summary>
		/// The <see cref="ActivityRecurrenceChooserDialog"/> will display choices for deleting a Recurrence Series or Occurrence.
		/// </summary>
		ChooseForDeletion,

		/// <summary>
		/// The <see cref="ActivityRecurrenceChooserDialog"/> will display choices for opening a Recurrence Series or Occurrence.
		/// </summary>
		ChooseForOpening,

		/// <summary>
		/// The <see cref="ActivityRecurrenceChooserDialog"/> will display choices for deleting a task when the connector only generates the current occurrence.
		/// </summary>

		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

		ChooseForCurrentTaskDeletion,
	}
	#endregion //RecurrenceChooserType

	#region RecurrenceDialogCoreCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="ActivityRecurrenceDialogCore"/> object.
	/// </summary>
	public enum RecurrenceDialogCoreCommand
	{
		/// <summary>
		/// Saves the changes (if any) in the Recurrence information and closes the <see cref="ActivityRecurrenceDialogCore"/>. 
		/// </summary>
		SaveAndClose,

		/// <summary>
		/// Closes the <see cref="ActivityRecurrenceDialogCore"/> object without saving changes (if any) in the Recurrence information.
		/// </summary>
		Close,

		/// <summary>
		/// Removes the recurrence definition from the <see cref="ActivityBase"/> being edited and closes the <see cref="ActivityRecurrenceDialogCore"/>.
		/// </summary>
		RemoveRecurrence,
	}
	#endregion //RecurrenceDialogCoreCommand

	#region RecurringTaskGenerationBehavior

	/// <summary>
	/// Represents the different ways in which recurring tasks can have their occurrences generated.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

	public enum RecurringTaskGenerationBehavior
	{
		/// <summary>
		/// Generate all occurrences based on the recurrence rules.
		/// </summary>
		GenerateAllTasks,

		/// <summary>
		/// Generate only the current task which isn't completed. Each task in the UI will actually be the recurring root 
		/// and task occurrences will not exist. Therefore, each completed tasks in the past will be a separate recurring
		/// root with no relation to the current task.
		/// </summary>
		GenerateCurrentTask,
	} 

	#endregion  // RecurringTaskGenerationBehavior

	#region ReminderDialogCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="ReminderDialog"/> object.
	/// </summary>
	public enum ReminderDialogCommand
	{
		/// <summary>
		/// Dismisses all the reminders in the <see cref="ReminderDialog"/>. 
		/// </summary>
		DismissAll,

		/// <summary>
		/// Opens the <see cref="Appointment"/> dialog for all currently selected reminder(s) that represent a reminder for an <see cref="Appointment"/> activity type. 
		/// </summary>
		OpenSelected,

		/// <summary>
		/// Dismisses the currently selected reminder(s) in the <see cref="ReminderDialog"/>. 
		/// </summary>
		DismissSelected,

		/// <summary>
		/// Snoozes the currently selected reminder(s) in the <see cref="ReminderDialog"/>. 
		/// </summary>
		SnoozeSelected,
	}
	#endregion //ReminderDialogCommand



	#region ResourceFeature Enum

	// SSP 2/25/2011 - NAS11.1 Activity Categories
	// 
	/// <summary>
	/// Used to specify which resource features are supported.
	/// </summary>
	/// <seealso cref="ScheduleDataConnectorBase.IsResourceFeatureSupported"/>

	[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

	public enum ResourceFeature
	{
		/// <summary>
		/// Feature indicates whether the resource supports activity category customization.
		/// </summary>
		CustomActivityCategories
	} 

	#endregion // ResourceFeature Enum



	#region ResourceRights Enum

	
	/// <summary>
	/// Represents rights that a resource has over one or more activities.
	/// </summary>
	[Flags]
	internal enum ResourceRights
	{
		/// <summary>
		/// This resource can view free-busy time, essentially that an activity exists in the schedule 
		/// but not the details of the activity.
		/// </summary>
		CanViewAsFreeBusy = 0x1,

		/// <summary>
		/// This resource can view full details of the activity.
		/// </summary>
		CanViewFullDetails = 0x2,

		/// <summary>
		/// This resource can add himself or herself as a participat in the activity.
		/// </summary>
		CanAddSelfAsParticipant = 0x4,

		/// <summary>
		/// This resource can add other resources as participants in the activity.
		/// </summary>
		CanAddOthersAsParticipant = 0x8,

		/// <summary>
		/// This resource can change the date and time of the activity.
		/// </summary>
		CanChangeTime = 0x10,

		/// <summary>
		/// This resource can remove the activity.
		/// </summary>
		CanRemove = 0x20,

		/// <summary>
		/// This resource has full rights to the activity.
		/// </summary>
		FullRights = 0x3f
	}

	#endregion // ResourceRights Enum

	#region ScheduleControlCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="ScheduleControlBase"/> object.
	/// </summary>
	public enum ScheduleControlCommand
	{
		/// <summary>
		/// Deletes all activites in the <see cref="ScheduleControlBase.SelectedActivities"/> collection.
		/// </summary>
		DeleteSelectedActivities,

		/// <summary>
		/// Activates the next activity relative to the current selection based on the dates currently in view.
		/// </summary>
		ActivityNext,

		/// <summary>
		/// Activates the previous activity relative to the current selection based on the dates currently in view.
		/// </summary>
		ActivityPrevious,

		/// <summary>
		/// Navigates to the time slot above the current <see cref="ScheduleControlBase.SelectedTimeRange"/>.
		/// </summary>
		TimeslotAbove,

		/// <summary>
		/// Navigates to the time slot to the left of the current <see cref="ScheduleControlBase.SelectedTimeRange"/>.
		/// </summary>
		TimeslotLeft,

		/// <summary>
		/// Navigates to the time slot to the right of the current <see cref="ScheduleControlBase.SelectedTimeRange"/>.
		/// </summary>
		TimeslotRight,

		/// <summary>
		/// Navigates to the time slot below the current <see cref="ScheduleControlBase.SelectedTimeRange"/>.
		/// </summary>
		TimeslotBelow,

		/// <summary>
		/// Changes the <see cref="ScheduleControlBase.ActiveCalendar"/> to the <see cref="CalendarGroupBase.SelectedCalendar"/> of the next <see cref="CalendarGroupBase"/>
		/// </summary>
		CalendarGroupNext,

		/// <summary>
		/// Changes the <see cref="ScheduleControlBase.ActiveCalendar"/> to the <see cref="CalendarGroupBase.SelectedCalendar"/> of the previous <see cref="CalendarGroupBase"/>
		/// </summary>
		CalendarGroupPrevious,

		/// <summary>
		/// Changes the <see cref="ScheduleControlBase.ActiveCalendar"/> to the <see cref="CalendarGroupBase.SelectedCalendar"/> of the end of the current page of groups in view.
		/// </summary>
		CalendarGroupPageNext,

		/// <summary>
		/// Changes the <see cref="ScheduleControlBase.ActiveCalendar"/> to the <see cref="CalendarGroupBase.SelectedCalendar"/> of the beginning of the current page of groups in view.
		/// </summary>
		CalendarGroupPagePrevious,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that they start after the last date currently in the collection attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftPageNext,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that they end just before the first date currently in the collection attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftPagePrevious,

		/// <summary>
		/// Updates the <see cref="ScheduleControlBase.VisibleDates"/> such that they have the same number of timeslot groups but they start based on an offset of the number of timeslot groups from the first date in the selection.
		/// </summary>
		VisibleDatesPageNext,

		/// <summary>
		/// Updates the <see cref="ScheduleControlBase.VisibleDates"/> such that they have the same number of timeslot groups but they end just before the first date currently in the collection.
		/// </summary>
		VisibleDatesPagePrevious,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that they start after the first timeslot group currently in the collection attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftNext,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that they end just before the first timeslot group currently in the collection attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftPrevious,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that the start 7 days before the first date in the VisibleDates attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftWeekNext,

		/// <summary>
		/// Adjusts the <see cref="ScheduleControlBase.VisibleDates"/> to be offset such that the start 7 days after the first date in the VisibleDates attempting to maintain the offset, if any, between the VisibleDates.
		/// </summary>
		VisibleDatesShiftWeekPrevious,

		
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Navigates to the first day of the week based on the current <see cref="ScheduleControlBase.SelectedTimeRange"/> adjusting the <see cref="ScheduleControlBase.VisibleDates"/> if necessary.
		/// </summary>
		TimeslotFirstDayOfWeek,

		/// <summary>
		/// Navigates to the last day of the week based on the current <see cref="ScheduleControlBase.SelectedTimeRange"/> adjusting the <see cref="ScheduleControlBase.VisibleDates"/> if necessary.
		/// </summary>
		TimeslotLastDayOfWeek,

		/// <summary>
		/// For controls like XamScheduleView and XamDayView, Home will navigate to the first working hour timeslot and to the first timeslot in the day when the control key is pressed. For XamMonthView, Home will navigate to the first day of week or the first day in view when the control key is pressed.
		/// </summary>
		NavigateHome,

		/// <summary>
		/// For controls like XamScheduleView and XamDayView, End will navigate to the last working hour timeslot and to the last timeslot in the day when the control key is pressed. For XamMonthView, End will navigate to the last day of week or the last day in view when the control key is pressed.
		/// </summary>
		NavigateEnd,

		/// <summary>
		/// For XamDayView, this will scroll up a page in timeslots. For XamScheduleView this navigates up in the groups. For XamMonthView this scrolls up based on the number of weeks in view.
		/// </summary>
		NavigatePageUp,

		/// <summary>
		/// For XamDayView, this will scroll down a page in timeslots. For XamScheduleView this navigates down in the groups. For XamMonthView this scrolls down based on the number of weeks in view.
		/// </summary>
		NavigatePageDown,

		// AS 9/30/10 TFS49593
		/// <summary>
		/// Creates a new activity in place based on the current <see cref="ScheduleControlBase.SelectedTimeRange"/> for the <see cref="ScheduleControlBase.ActiveCalendar"/> assuming there are timeslots selected.
		/// </summary>
		CreateInPlaceActivity,

		/// <summary>
		/// Navigates to the current date
		/// </summary>
		Today,

		/// <summary>
		/// Puts the selected activity into edit mode.
		/// </summary>
		EditSelectedActivity, // JJD 12/02/10 - TFS59876

		/// <summary>
		/// Displays the activity dialog for the currently <see cref="ScheduleControlBase.SelectedActivities"/>
		/// </summary>
		DisplayDialogsForSelectedActivities, // JJD 12/02/10 - TFS59874
	}
	#endregion //ScheduleControlCommand

	#region ScheduleDialogCommand
	/// <summary>
	/// An enumeration of available commands for schedule dialogs
	/// </summary>
	public enum ScheduleDialogCommand
	{
		/// <summary>
		/// Saves the changes and closes the dialog object that is hosting it. 
		/// </summary>
		SaveAndClose,

		/// <summary>
		/// Closes the dialog object without saving changes.
		/// </summary>
		Close,
	}
	#endregion //ScheduleDialogCommand


	#region ScheduleErrorDisplayType
	/// <summary>
	/// An enumeration used to indicate the type of ui containing the error information that will be displayed to the end user.
	/// </summary>
	public enum ScheduleErrorDisplayType
	{
		/// <summary>
		/// By default an error icon is displayed within the associated <see cref="ActivityPresenter"/> and in its associated ToolTip.
		/// </summary>
		ActivityErrorIcon,

		/// <summary>
		/// A message box will be displayed to the end user.
		/// </summary>
		MessageBox,

		/// <summary>
		/// Sets the <see cref="ScheduleControlBase.BlockingError"/> of the controls associated with the <see cref="XamScheduleDataManager"/>. The default ControlTemplate for the view controls will display an overlay including the error message.
		/// </summary>
		BlockingError,
	} 
	#endregion // ScheduleErrorDisplayType





	#region OutlookCalendarViewMode
	/// <summary>
	/// Enumeration used to describe the current view of the <see cref="XamOutlookCalendarView"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public enum OutlookCalendarViewMode
	{
		/// <summary>
		/// The <see cref="XamOutlookCalendarView.DayView"/> is active and showing one or more days.
		/// </summary>
		DayViewDay,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.DayView"/> is active and showing a full week.
		/// </summary>
		DayViewWeek,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.DayView"/> is active and showing the work week.
		/// </summary>
		DayViewWorkWeek,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.MonthView"/> is active
		/// </summary>
		MonthView,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.ScheduleView"/> is active and showing one or more days.
		/// </summary>
		ScheduleViewDay,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.ScheduleView"/> is active and showing a full week.
		/// </summary>
		ScheduleViewWeek,

		/// <summary>
		/// The <see cref="XamOutlookCalendarView.ScheduleView"/> is active and showing the work week.
		/// </summary>
		ScheduleViewWorkWeek,

	}
	#endregion // OutlookCalendarViewMode

	#region ScheduleTabLayoutStyle
	/// <summary>
	/// Enumeration used to define the type of layout used when arranging the tab items in a ScheduleTabPanel
	/// </summary>
	internal enum ScheduleTabLayoutStyle
	{
		/// <summary>
		/// The items are sized based on their content and arranged in a single row.
		/// </summary>
		SingleRowAutoSize,

		/// <summary>
		/// The items are sized based on their content size and then reduced towards their minimum size if there is not enough room to fit the items. The items are arranged within a single row.
		/// </summary>
		SingleRowJustified,

		/// <summary>
		/// The items are sized based on their content size and then increased in size if there is more room to display the items than required. The items are arranged within a single row.
		/// </summary>
		SingleRowSizeToFit,
	} 
	#endregion // ScheduleTabLayoutStyle

	#region VisibleDateAdjustment
	internal enum VisibleDateAdjustment
	{
		/// <summary>
		/// Adjust by a single day. For a control displaying weeks this is the same as Week.
		/// </summary>
		SingleItem,

		/// <summary>
		/// Adjust by 7 days.
		/// </summary>
		Week,

		/// <summary>
		/// Adjust based on the number of timeslot groups.
		/// </summary>
		Page,
	} 
	#endregion // VisibleDateAdjustment

	#region WcfSchedulePollingMode

	/// <summary>
	/// Represents the various polling modes available on the <see cref="WcfListScheduleDataConnector"/> to poll 
	/// for changes from the server. Polling only works when the item sources on the service send out list change 
	/// notifications.
	/// </summary>
	/// <see cref="WcfListScheduleDataConnector.PollingMode"/>
	public enum WcfSchedulePollingMode
	{
		/// <summary>
		/// Polling will indicate which item sources have changed, at which point the client will requery the 
		/// items from that item source.
		/// </summary>
		RequeryOnAnyChange,

		/// <summary>
		/// Polling will return detailed changes to individual items in the item sources.
		/// </summary>
		Detailed,

		/// <summary>
		/// No polling should be done.
		/// </summary>
		None,
	}

	#endregion  // WcfSchedulePollingMode

	#region WeekDisplayMode
	/// <summary>
	/// Enumeration used to determine which dates are displayed within the <see cref="ScheduleTimeControlBase"/>
	/// </summary>
	/// <seealso cref="ScheduleTimeControlBase.WeekDisplayMode"/>
	/// <seealso cref="ScheduleTimeControlBase.WorkingHoursSource"/>
	public enum WeekDisplayMode
	{
		/// <summary>
		/// The dates displayed in the <see cref="ScheduleTimeControlBase"/> is based upon the dates in the <see cref="ScheduleControlBase.VisibleDates"/> and may be discontiguous.
		/// </summary>
		None,

		/// <summary>
		/// The dates displayed in the <see cref="ScheduleTimeControlBase"/> are the working days in the week containing with the <see cref="ScheduleControlBase.SelectedTimeRange"/> Start. The working hour information is based upon the <see cref="ScheduleTimeControlBase.WorkingHoursSource"/>
		/// </summary>
		WorkWeek,

		/// <summary>
		/// The dates displayed in the <see cref="ScheduleTimeControlBase"/> are the 7 days in the week containing with the <see cref="ScheduleControlBase.SelectedTimeRange"/> Start.
		/// </summary>
		Week,
	}
	#endregion // WeekDisplayMode

	#region WorkingHoursSource
	/// <summary>
	/// Used to determine what objects are considered when calculating the working hours and work day information.
	/// </summary>
	public enum WorkingHoursSource
	{
		/// <summary>
		/// The working hours of the <see cref="XamScheduleDataManager.CurrentUser"/> are used to determine which timeslots are displayed.
		/// </summary>
		CurrentUser,

		/// <summary>
		/// The working hours of all the resources within the <see cref="ScheduleControlBase.CalendarGroupsResolved"/> are unioned together.
		/// </summary>
		AllResourcesInGroups,
	} 
	#endregion // WorkingHoursSource

	#region XamOutlookCalendarViewCommand
	/// <summary>
	/// Enumeration used to identify a given <see cref="XamOutlookCalendarView"/> command
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]

	public enum XamOutlookCalendarViewCommand
	{
		/// <summary>
		/// Switches to viewing a single day.
		/// </summary>
		Show1Day,

		/// <summary>
		/// Switches to viewing 2 days.
		/// </summary>
		Show2Days,

		/// <summary>
		/// Switches to viewing 3 days.
		/// </summary>
		Show3Days,

		/// <summary>
		/// Switches to viewing 4 days.
		/// </summary>
		Show4Days,

		/// <summary>
		/// Switches to viewing 5 days.
		/// </summary>
		Show5Days,

		/// <summary>
		/// Switches to viewing 6 days.
		/// </summary>
		Show6Days,

		/// <summary>
		/// Switches to viewing 7 days.
		/// </summary>
		Show7Days,

		/// <summary>
		/// Switches to viewing 8 days.
		/// </summary>
		Show8Days,

		/// <summary>
		/// Switches to viewing 9 days.
		/// </summary>
		Show9Days,

		/// <summary>
		/// Switches to viewing 10 days.
		/// </summary>
		Show10Days,

		/// <summary>
		/// Switches <see cref="XamOutlookCalendarView.CurrentViewMode"/> to <b>MonthView</b>
		/// </summary>
		SwitchToMonthView,

		/// <summary>
		/// Switches <see cref="XamOutlookCalendarView.CurrentViewMode"/> to <b>DayViewWeek</b>
		/// </summary>
		SwitchToFullWeekView,

		/// <summary>
		/// Switches <see cref="XamOutlookCalendarView.CurrentViewMode"/> to <b>DayViewWorkWeek</b>
		/// </summary>
		SwitchToWorkWeekView,

		/// <summary>
		/// Switches <see cref="XamOutlookCalendarView.CurrentViewMode"/> to one of the XamScheduleView related views. The new value will be based on the current view.
		/// </summary>
		SwitchToScheduleView,

		/// <summary>
		/// Switches <see cref="XamOutlookCalendarView.CurrentViewMode"/> to one of the XamDayView related views. The new value will be based on the current view.
		/// </summary>
		SwitchToDayView,

		/// <summary>
		/// Shifts the visible dates of the <see cref="XamOutlookCalendarView.CurrentViewControl"/> backwards.
		/// </summary>
		NavigateBack,

		/// <summary>
		/// Shifts the visible dates of the <see cref="XamOutlookCalendarView.CurrentViewControl"/> forwards.
		/// </summary>
		NavigateForward,

		/// <summary>
		/// Changes the view such that it shows 7 days starting with the current date.
		/// </summary>
		Next7Days,
	} 
	#endregion // XamOutlookCalendarViewCommand


}




namespace Infragistics.Controls.Schedules.Primitives

{

	#region ActivityContentArea

	/// <summary>
	/// Specifies specific area of an <see cref="ActivityPresenter"/>
	/// </summary>
	public enum ActivityContentArea
	{
		/// <summary>
		/// Contains optional start date or time text.
		/// </summary>
		Prefix,
		/// <summary>
		/// Contains optional end date or time text.
		/// </summary>
		Suffix,
		/// <summary>
		/// Contains the Subject and Location information.
		/// </summary>
		Content,
		/// <summary>
		/// Contains the state indicators, e.g. recurrence, reminder, pending operation etc..
		/// </summary>
		Indicators,
	}

	#endregion //ActivityContentArea

	// AS 3/1/11 NA 2011.1 ActivityTypeChooser
	#region ActivityCreationTrigger
	internal enum ActivityCreationTrigger
	{
		DoubleClick,
		Typing,
		ClickToAdd,
	} 
	#endregion //ActivityCreationTrigger


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


	#region ClickToAddType
	internal enum ClickToAddType
	{
		/// <summary>
		/// No click to add functionality
		/// </summary>
		None,

		/// <summary>
		/// A standard appointment
		/// </summary>
		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		//Appointment,
		Activity,

		/// <summary>
		/// A standard appointment as long as it does not intersect with another standard appointment
		/// </summary>
		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		//NonIntersectingAppointment,
		NonIntersectingActivity,

		/// <summary>
		/// A timezone neutral appointment
		/// </summary>
		Event,
	} 
	#endregion // ClickToAddType

	#region DateTimeFormatType

	/// <summary>
	/// Specifies how to format a specific date
	/// </summary>
	public enum DateTimeFormatType
	{
		/// <summary>
		/// Return an empty string
		/// </summary>
		None,
		/// <summary>
		/// The full day of the week string e.g. 'Tuesday', 'Wednesday' etc.
		/// </summary>
		DayOfWeek,
		/// <summary>
		/// The day of the month string e.g. '1'thru '31'.
		/// </summary>
		DayOfMonthNumber,
		/// <summary>
		/// Uses the full date time pattern to format the date and time, e.g. 'Thursday, July 22, 2010 4:31:00 PM'
		/// </summary>
		FullDateTime,
		/// <summary>
		/// The hour of the day string e.g. '1' thru '12' or '0' thru '23'.
		/// </summary>
		Hour,
		/// <summary>
		/// Uses the long date pattern to format the date, e.g. 'Sunday, July 25, 2010'
		/// </summary>
		LongDate,
		/// <summary>
		/// Uses the long time pattern to format the date, e.g. '4:31:00 PM'
		/// </summary>
		LongTime,
		/// <summary>
		/// The minute of the hour string e.g. '0' thru '59',
		/// </summary>
		Minute,
		/// <summary>
		/// The month and the day e.g. August 25.
		/// </summary>
		MonthDay,
		/// <summary>
		/// The month of the year string e.g. '1' thru '12'.
		/// </summary>
		MonthOfYearNumber,

		/// <summary>
		/// The full name of the month e.g. 'December'.
		/// </summary>
		MonthName,

		/// <summary>
		/// Uses the short date pattern to format the date, e.g. '7/22/2010'
		/// </summary>
		ShortDate,
		/// <summary>
		/// Uses the short time pattern to format the date, e.g. '4:31 PM'
		/// </summary>
		ShortTime,
		/// <summary>
		/// An abbreviated day of the week string e.g. 'Tue', 'Wed' etc.
		/// </summary>
		ShortDayOfWeek,
		/// <summary>
		/// The shorted day of the week string e.g. 'Tu', 'We' etc. Note: in Silverlight this is the same as 'ShortDayOfWeek'
		/// </summary>
		ShortestDayOfWeek,
		/// <summary>
		/// The month and the day e.g. Aug 25.
		/// </summary>
		ShortMonthDay,
		/// <summary>
		/// The month and the day e.g. 8/25.
		/// </summary>
		ShortestMonthDay,
		/// <summary>
		/// The year and the month e.g. 'July, 2010'.
		/// </summary>
		YearMonth,
		/// <summary>
		/// The year with 2 digits e.g. 2010 as '10'.
		/// </summary>
		Year2Digit,
		/// <summary>
		/// The year with 4 digits e.g. '2010'.
		/// </summary>
		Year4Digit,

	}

	#endregion //DateTimeFormatType

	#region DateRangeFormatType

	/// <summary>
	/// Specifies how to format a date range
	/// </summary>
	public enum DateRangeFormatType
	{
		/// <summary>
		/// Return an empty string
		/// </summary>
		None,

		/// <summary>
		/// Format the date range to be used in an activity tooltip
		/// </summary>
		ActivityToolTip,

		/// <summary>
		/// Format the end date to be used in the out of view indicator
		/// </summary>
		EndDateOutOfView,

		/// <summary>
		/// Format the end time only.
		/// </summary>
		EndTimeOnly, 

		/// <summary>
		/// The month day header which displays the month name and day number.
		/// </summary>
		MonthDayHeader,

		/// <summary>
		/// The month day header pattern which displays the month day and year.
		/// </summary>
		MonthDayHeaderFull,

		/// <summary>
		/// Format the date range to be used in the header of the week view when the WeekHeader formatted value cannot be shown.
		/// </summary>
		ShortWeekHeader,

		/// <summary>
		/// Format the start and end time.
		/// </summary>
		StartAndEndTime,

		/// <summary>
		/// Format the start date to be used in the out of view indicator
		/// </summary>
		StartDateOutOfView,

		/// <summary>
		/// Format the start time only.
		/// </summary>
		StartTimeOnly,

		/// <summary>
		/// Format the date range to be used in the header of the week view.
		/// </summary>
		WeekHeader,

		/// <summary>
		/// Format the date range used in the <see cref="XamOutlookCalendarView"/> header when XamScheduleView or XamDayView is the active view control.
		/// </summary>
		CalendarDateRange,

		/// <summary>
		/// Format the date range when weeks from more than 1 month are selected.
		/// </summary>
		CalendarMonthRange,
	}

	#endregion //DateRangeFormatType

	#region MonthDayType
	internal enum MonthDayType
	{
		DayNumber,
		MonthDay,
		MonthDayYear,
	} 
	#endregion // MonthDayType

	#region MoreActivityIndicatorDirection
	internal enum MoreActivityIndicatorDirection
	{
		Up,
		Down,
	} 
	#endregion // MoreActivityIndicatorDirection

	#region SpatialDirection
	internal enum SpatialDirection
	{
		Up,
		Down,
		Left,
		Right,
	} 
	#endregion // SpatialDirection

	#region TimeslotTickmarkKind
	/// <summary>
	/// Determines the kind of tickmark is represented by a specific <see cref="TimeslotHeaderTickmark"/>
	/// </summary>
	public enum TimeslotTickmarkKind
	{
		/// <summary>
		/// The tick mark represents a time that is between major spans.
		/// </summary>
		Minor,

		/// <summary>
		/// The tick mark represents a time that is at the beginning/end of a major span.
		/// </summary>
		Major,

		/// <summary>
		/// The tick mark represents a time that is at the beginning/end of a day.
		/// </summary>
		Day,
	}
	#endregion // TimeslotTickmarkKind

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