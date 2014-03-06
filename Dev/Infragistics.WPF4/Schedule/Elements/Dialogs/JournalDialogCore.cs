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
	/// Displays a UI for the core portion of the <see cref="Journal"/> dialog. 
	/// </summary>
	/// <seealso cref="ActivityDialogCore"/>
	/// <seealso cref="AppointmentDialogCore"/>
	/// <seealso cref="TaskDialogCore"/>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="Journal"/>
	[TemplatePart(Name = PartReminderDatePicker, Type = typeof(DatePicker))]



	[TemplatePart(Name = PartReminderTimePicker, Type = typeof(ComboBox))]

	public class JournalDialogCore : ActivityDialogCore,
									 IReminderDatePickerManagerHost
	{
		#region Member Variables

		// Template part names
		private const string PartReminderDatePicker = "ReminderDatePicker";
		private const string PartReminderTimePicker = "ReminderTimePicker";

		private Dictionary<string, string>					_localizedStrings;
		private bool										_initialized;
		private ReminderDatePickerManager					_reminderDatePickerManager;

		#endregion //Member Variables

		#region Constructors
		static JournalDialogCore()
		{

			JournalDialogCore.DefaultStyleKeyProperty.OverrideMetadata(typeof(JournalDialogCore), new FrameworkPropertyMetadata(typeof(JournalDialogCore)));

		}

		/// <summary>
		/// Creates an instance of the JournalDialogCore control which represents the core of the <see cref="Journal"/> dialog.
		/// </summary>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no containing element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/>.</param>
		/// <param name="journal">The journal to display.</param>
		public JournalDialogCore(FrameworkElement container, XamScheduleDataManager dataManager, Journal journal)
			: base(container, dataManager, journal)
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

			base.OnActivityPropertyChanged(sender, e);
		}
		#endregion //OnActivityPropertyChanged

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

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
					this._localizedStrings.Add("DLG_Appointment_Core_StartTime", ScheduleUtilities.GetString("DLG_Appointment_Core_StartTime"));
					this._localizedStrings.Add("DLG_Journal_Core_Duration", ScheduleUtilities.GetString("DLG_Journal_Core_Duration"));
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