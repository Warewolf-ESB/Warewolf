using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Abstract base class for all XamSchedule dialog factories.
	/// </summary>
	public abstract class ScheduleDialogFactoryBase
	{
		#region Properties

		#region SupportedActivityDialogTypes

		/// <summary>
		/// Returns a flagged enumeration indicating the types of <see cref="ActivityBase"/> for which the <see cref="CreateActivityDialog"/> method will return a dialog to be displayed.
		/// </summary>
		public abstract ActivityTypes SupportedActivityDialogTypes { get; }

		#endregion // SupportedActivityDialogTypes

		#endregion // Properties

		#region Methods

		#region CreateActivityDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for creating and editing <see cref="ActivityBase"/> derived instances.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="activity">The <see cref="ActivityBase"/> derived activity that is being edited</param>
		/// <param name="allowModifications">True if the dialog should allow modifications of the <see cref="ActivityBase"/>, false to display the <see cref="ActivityBase"/> without allowing modifications.</param>
		/// <param name="allowRemove">True if the dialog should allow the <see cref="ActivityBase"/> to be removed, false to disallow removal of the <see cref="ActivityBase"/>.</param>
		/// <returns>A FrameworkElement that represents the dialog or the contents of the dialog.</returns>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="ActivityType"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="Task"/>
		public virtual FrameworkElement CreateActivityDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity, bool allowModifications, bool allowRemove)
		{
			return null;
		}

		#endregion //CreateActivityDialog

		#region CreateActivityCategoryDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for managing <see cref="ActivityCategory"/>s for a list of <see cref="ActivityBase"/> instances.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> instance.</param>
		/// <param name="allowModifications">True if the dialog should allow modifications of the <see cref="ActivityCategory"/>s, false to display the <see cref="ActivityCategory"/>s without allowing modifications.</param>
		/// <returns>A FrameworkElement that represents the dialog or the contents of the dialog.</returns>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public virtual FrameworkElement CreateActivityCategoryDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityCategoryHelper activityCategoryHelper, bool allowModifications)
		{
			return null;
		}

		#endregion //CreateActivityCategoryDialog

		#region CreateActivityCategoryCreationDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for creating an <see cref="ActivityCategory"/>.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> instance.</param>
		/// <param name="creationResult">An instance of the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> class that holds the <see cref="ActivityCategory"/> created by the dialog and (optionally) a piece of user data.</param>
		/// <param name="updateOwningResource">True to automatically update the owning <see cref="Resource"/> with the new <see cref="ActivityCategory"/>.</param>
		/// <returns>A FrameworkElement that represents the dialog or the contents of the dialog.</returns>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public virtual FrameworkElement CreateActivityCategoryCreationDialog(FrameworkElement container, ActivityCategoryHelper activityCategoryHelper, ActivityCategoryCreationDialog.ChooserResult creationResult, bool updateOwningResource)
		{
			return null;
		}

		#endregion //CreateActivityCategoryCreationDialog

		#region CreateActivityRecurrenceDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for creating and editing <see cref="ActivityBase"/> recurrences.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="activity">The <see cref="ActivityBase"/> that is being edited</param>
		/// <param name="allowModifications">True if the dialog should allow modifications of the <see cref="Appointment"/> recurrences, false to display the <see cref="Appointment"/> recurrences without allowing modifications.</param>
		/// <returns>A FrameworkElement that represents the Recurrence dialog or the contents of the Recurrence dialog.</returns>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="RecurrenceBase"/>
		/// <seealso cref="Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore"/>
		public virtual FrameworkElement CreateActivityRecurrenceDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity, bool allowModifications)
		{
			return null;
		}

		#endregion //CreateActivityRecurrenceDialog

		#region CreateActivityRecurrenceChooserDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for letting the user choose whether to access a recurrence Series or Occurrence
		/// when an <see cref="ActivityBase"/> is opened or deleted.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="activity">The recurring <see cref="ActivityBase"/> for which the choice is being made.</param>
		/// <param name="chooserType">The type of <see cref="ActivityRecurrenceChooserDialog"/> to display, i.e. a chooser for opening recurrences or deleting recurrences.</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <returns>A FrameworkElement that represents the Recurrence dialog or the contents of the Recurrence dialog.</returns>
		/// <seealso cref="ActivityRecurrenceChooserDialog"/>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="RecurrenceBase"/>
		/// <seealso cref="Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore"/>
		public virtual FrameworkElement CreateActivityRecurrenceChooserDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity, RecurrenceChooserType chooserType, ActivityRecurrenceChooserDialog.ChooserResult chooserResult)
		{
			return null;
		}

		#endregion //CreateActivityRecurrenceChooserDialog

		#region CreateActivityTypeChooserDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for letting the user choose an <see cref="ActivityType"/> from a set of available <see cref="ActivityTypes"/>
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed. If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="availableTypes">The list of types from which the choice is to be made</param>
		/// <param name="calendar">The calendar associated with the action or null if there is no calendar associated with the action</param>
		/// <param name="chooserReason">The reason for which the dialog is being displayed</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <returns>A FrameworkElement that can be used to choose an activity type.</returns>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="Infragistics.Controls.Schedules.Primitives.ActivityTypeChooserDialog"/>
		public virtual FrameworkElement CreateActivityTypeChooserDialog(FrameworkElement container, XamScheduleDataManager dataManager, ActivityTypes availableTypes, ActivityTypeChooserReason chooserReason, ResourceCalendar calendar, ActivityTypeChooserDialog.ChooserResult chooserResult)
		{
			return null;
		}

		#endregion //CreateActivityTypeChooserDialog

		#region CreateReminderDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for letting the user dismiss or snooze active Reminders.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <returns>A FrameworkElement that represents the Recurrence dialog or the contents of the Recurrence dialog.</returns>
		/// <seealso cref="Reminder"/>
		/// <seealso cref="ReminderDialog"/>
		/// <seealso cref="XamScheduleDataManager"/>
		public virtual FrameworkElement CreateReminderDialog(FrameworkElement container, XamScheduleDataManager dataManager)
		{
			return null;
		}

		#endregion //CreateReminderDialog

		#region CreateTimeZoneChooserDialog

		/// <summary>
		/// Returns a FrameworkElement that should used as the dialog for letting the user select a time zone.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <b>XamScheduleDataManager</b> will automatically host the FrameworkElement returned by this method in an appropriate container
		/// depending on the environment within which the dialog is being displayed.  If the environment supports top level windows then the
		/// FrameworkElement will be hosted in a Dialog Window unless this method returns a Window-derived element in which case that window
		/// will be used as the dialog window.
		/// </para>
		/// </remarks>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no cointaining element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/></param>
		/// <param name="tzInfoProvider">The object that provides the list of time zones</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <returns>A FrameworkElement that represents the TimeZone dialog or the contents of the TimeZone dialog.</returns>
		/// <seealso cref="TimeZoneChooserDialog"/>
		/// <seealso cref="XamScheduleDataManager"/>
		/// <seealso cref="TimeZoneInfoProvider"/>
		public virtual FrameworkElement CreateTimeZoneChooserDialog(FrameworkElement container, XamScheduleDataManager dataManager, TimeZoneInfoProvider tzInfoProvider, TimeZoneChooserDialog.ChooserResult chooserResult)
		{
			return null;
		}

		#endregion //CreateTimeZoneChooserDialog

		#endregion //Methods
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