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
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for the core portion of the <see cref="Task"/> dialog. 
	/// </summary>
	/// <seealso cref="ActivityDialogCore"/>
	/// <seealso cref="AppointmentDialogCore"/>
	/// <seealso cref="JournalDialogCore"/>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="Journal"/>
	[TemplatePart(Name = PartReminderDatePicker, Type = typeof(DatePicker))]



	[TemplatePart(Name = PartReminderTimePicker, Type = typeof(ComboBox))]

	public class TaskDialogCore : ActivityDialogCore,
								  IReminderDatePickerManagerHost
	{
		#region Member Variables

		// Template part names
		private const string PartReminderDatePicker		= "ReminderDatePicker";
		private const string PartReminderTimePicker		= "ReminderTimePicker";

		private Dictionary<string, string>						_localizedStrings;
		private bool											_initialized;
		private ReminderDatePickerManager						_reminderDatePickerManager;

		#endregion //Member Variables

		#region Constructors
		static TaskDialogCore()
		{

			TaskDialogCore.DefaultStyleKeyProperty.OverrideMetadata(typeof(TaskDialogCore), new FrameworkPropertyMetadata(typeof(TaskDialogCore)));

		}

		/// <summary>
		/// Creates an instance of the TaskDialogCore control which represents the core of the <see cref="Task"/> dialog.
		/// </summary>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no containing element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/>.</param>
		/// <param name="task">The task to display.</param>
		public TaskDialogCore(FrameworkElement container, XamScheduleDataManager dataManager, Task task)
			: base(container, dataManager, task)
		{



		}
		#endregion //Constructors

		#region Base Class Overrides

		#region Initialize
		/// <summary>
		/// Called asynchronously after the control template has been applied.
		/// </summary>
		protected override void Initialize()
		{
			if (true == this._initialized)
			{
				this._reminderDatePickerManager.DisconnectEventListeners();
				this._reminderDatePickerManager = null;
			}

			this._reminderDatePickerManager = 
				new ReminderDatePickerManager(this,
											  this.DataManager,
											  this.Activity,
											  this.GetTemplateChild(PartReminderDatePicker) as DatePicker,
											  this.GetTemplateChild(PartReminderTimePicker) as FrameworkElement);

			this.VerifyDatesSectionVisibility();
			this.VerifyDueInDescription();

			this._initialized = true;
		}
		#endregion //Initialize

		#region OnActivityPropertyChanged
		/// <summary>
		/// Called when a property on the Activity changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected override void OnActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.IsInEndEdit)
				return;

			if (e.PropertyName == "End")
				this.VerifyDueInDescription();

			base.OnActivityPropertyChanged(sender, e);
		}
		#endregion //OnActivityPropertyChanged

		#region ProcessRecurrenceDialogClosed
		/// <summary>
		/// Called after the <see cref="ActivityRecurrenceDialogCore"/> has closed.
		/// </summary>
		/// <param name="dialogResult"></param>
		protected override void ProcessRecurrenceDialogClosed(bool? dialogResult)
		{
			base.ProcessRecurrenceDialogClosed(dialogResult);

			this.VerifyDatesSectionVisibility();
		}
		#endregion //ProcessRecurrenceDialogClosed

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region DatesSectionVisibility

		private static readonly DependencyPropertyKey DatesSectionVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DatesSectionVisibility",
				typeof(Visibility), typeof(TaskDialogCore), Visibility.Visible, null);

		/// <summary>
		/// Identifies the read-only <see cref="DatesSectionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DatesSectionVisibilityProperty = DatesSectionVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the Start and Due dates sectiojn of the dialog should be visible.
		/// </summary>
		/// <seealso cref="DatesSectionVisibilityProperty"/>
		/// <seealso cref="Task"/>
		public Visibility DatesSectionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(TaskDialogCore.DatesSectionVisibilityProperty);
			}
			internal set
			{
				this.SetValue(TaskDialogCore.DatesSectionVisibilityPropertyKey, value);
			}
		}

		#endregion //DatesSectionVisibility

		#region DueInDescription

		private static readonly DependencyPropertyKey DueInDescriptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DueInDescription",
			typeof(string), typeof(TaskDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="DueInDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DueInDescriptionProperty = DueInDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a string that describes when the task is due.  This property is only valid when the <see cref="DueInDescriptionVisibility"/> property is set to Visible.
		/// </summary>
		/// <seealso cref="DueInDescriptionProperty"/>
		/// <seealso cref="DueInDescriptionVisibility"/>
		/// <seealso cref="Task"/>
		public string DueInDescription
		{
			get
			{
				return (string)this.GetValue(TaskDialogCore.DueInDescriptionProperty);
			}
			internal set
			{
				this.SetValue(TaskDialogCore.DueInDescriptionPropertyKey, value);
			}
		}

		#endregion //DueInDescription

		#region DueInDescriptionVisibility
		/// <summary>
		/// Identifies the <see cref="DueInDescriptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DueInDescriptionVisibilityProperty = DependencyPropertyUtilities.Register("DueInDescriptionVisibility",
			typeof(Visibility), typeof(TaskDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Returns/sets whether a message that describes when the <see cref="Task"/> is due is visible.
		/// </summary>
		/// <seealso cref="DueInDescriptionVisibilityProperty"/>
		/// <seealso cref="DueInDescription"/>
		/// <seealso cref="Task"/>
		public Visibility DueInDescriptionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(TaskDialogCore.DueInDescriptionVisibilityProperty);
			}
			set
			{
				this.SetValue(TaskDialogCore.DueInDescriptionVisibilityProperty, value);
			}
		}
		#endregion //DueInDescriptionVisibility

		#region LocalizedStrings
		/// <summary>
		/// Returns a dictionary of localized strings for use by the controls in the template.
		/// </summary>
		public Dictionary<string, string> LocalizedStrings
		{
			get
			{
				if (this._localizedStrings == null)
				{
					this._localizedStrings = new Dictionary<string, string>(10);

					this._localizedStrings.Add("DLG_Appointment_Core_Subject", ScheduleUtilities.GetString("DLG_Appointment_Core_Subject"));
					this._localizedStrings.Add("DLG_Task_Core_StartDate", ScheduleUtilities.GetString("DLG_Task_Core_StartDate"));
					this._localizedStrings.Add("DLG_Task_Core_DueDate", ScheduleUtilities.GetString("DLG_Task_Core_DueDate"));
					this._localizedStrings.Add("DLG_Task_Core_PercentComplete", ScheduleUtilities.GetString("DLG_Task_Core_PercentComplete"));
					this._localizedStrings.Add("DLG_Activity_Core_Reminder", ScheduleUtilities.GetString("DLG_Activity_Core_Reminder"));
					this._localizedStrings.Add("DLG_Activity_Core_Owner", ScheduleUtilities.GetString("DLG_Activity_Core_Owner", this.Activity.OwningResource.Name));
					this._localizedStrings.Add("DLG_Appointment_Core_RecurrenceRootLabel", ScheduleUtilities.GetString("DLG_Appointment_Core_RecurrenceRootLabel"));
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region VerifyDatesSectionVisibility
		private void VerifyDatesSectionVisibility()
		{
			this.DatesSectionVisibility = (this.Activity.IsRecurrenceRoot && this.DataManager.DataConnector.RecurringTaskGenerationBehavior != RecurringTaskGenerationBehavior.GenerateCurrentTask)
				? Visibility.Collapsed : Visibility.Visible;
		}
		#endregion //VerifyDatesSectionVisibility

		#region VerifyDueInDescription
		private void VerifyDueInDescription()
		{
			DateTime dueDate			= this.Activity.GetEndLocal(this.TimeZoneTokenHelper.LocalTimeZoneToken);
			DateTime dueDateMidnight	= new DateTime(dueDate.Year, dueDate.Month, dueDate.Day, 0, 0, 0);
			DateTime now				= ScheduleUtilities.ConvertFromUtc(this.TimeZoneTokenHelper.LocalTimeZoneToken, DateTime.UtcNow);
			DateTime nowMidnight		= new DateTime(now.Year, now.Month, now.Day, 0, 0, 0);
			TimeSpan dueIn				= dueDateMidnight.Subtract(nowMidnight);

			string dueInDescription		= string.Empty;

			if (dueIn.TotalDays == 0)
				dueInDescription	= ScheduleUtilities.GetString("DLG_Task_Core_DueToday");
			else if (dueIn.TotalDays == 1)
				dueInDescription = ScheduleUtilities.GetString("DLG_Task_Core_DueTomorrow");
			else if (dueIn.TotalDays > 1 && dueIn.TotalDays < 15)
				dueInDescription = ScheduleUtilities.GetString("DLG_Task_Core_DueInDays", dueIn.TotalDays.ToString());
			else if (dueIn.TotalDays == -1)
				dueInDescription = ScheduleUtilities.GetString("DLG_Task_Core_DueYesterday");
			else if (dueIn.TotalDays < -1 && dueIn.TotalDays > -15)
				dueInDescription = ScheduleUtilities.GetString("DLG_Task_Core_OverdueByDays", Math.Abs(dueIn.TotalDays).ToString());


			this.DueInDescriptionVisibility = string.IsNullOrEmpty(dueInDescription) ? Visibility.Collapsed : Visibility.Visible;
			this.DueInDescription			= dueInDescription;
		}
		#endregion //VerifyDueInDescription

		#endregion //Methods

		#region IReminderDatePickerManagerHost Members

		DateTime IReminderDatePickerManagerHost.StartAsLocal
		{
			get { return this.StartAsLocal; }
		}

		TimeZoneToken IReminderDatePickerManagerHost.ActivityStartTimeZoneTokenResolved
		{
			get { return this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved; }
		}

		void IReminderDatePickerManagerHost.UpdateDirtyStatus(bool isDirty)
		{
			this.UpdateDirtyStatus(isDirty);
		}

		#endregion
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