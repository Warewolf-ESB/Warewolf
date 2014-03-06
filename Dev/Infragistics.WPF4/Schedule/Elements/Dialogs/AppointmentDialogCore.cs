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
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Threading;
using Infragistics.Collections;
using System.ComponentModel;







using System.Windows.Data;


namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for the core portion of the <see cref="Appointment"/> dialog. 
	/// </summary>
	/// <seealso cref="ActivityDialogCore"/>
	/// <seealso cref="TaskDialogCore"/>
	/// <seealso cref="JournalDialogCore"/>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="Journal"/>
	public class AppointmentDialogCore : ActivityDialogCore
	{
		#region Member Variables

		private bool											_initialized;
		private Dictionary<string, string>						_localizedStrings;

		#endregion //Member Variables

		#region Constructors

		static AppointmentDialogCore()
		{

			AppointmentDialogCore.DefaultStyleKeyProperty.OverrideMetadata(typeof(AppointmentDialogCore), new FrameworkPropertyMetadata(typeof(AppointmentDialogCore)));

		}

		/// <summary>
		/// Creates an instance of the AppointmentDialogCore control which represents the core of the <see cref="Appointment"/> dialog.
		/// </summary>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no containing element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/>.</param>
		/// <param name="appointment">The appointment to display.</param>
		public AppointmentDialogCore(FrameworkElement container, XamScheduleDataManager dataManager, Appointment appointment)
			: base(container, dataManager, appointment)
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
			}

			this.AllowAllDayActivities					= this.DataManager.IsTimeZoneNeutralActivityAllowed(this.Appointment);

			this._initialized							= true;
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

		#region AllowAllDayActivities

		/// <summary>
		/// Identifies the <see cref="AllowAllDayActivities"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowAllDayActivitiesProperty = DependencyPropertyUtilities.Register("AllowAllDayActivities",
			typeof(bool), typeof(AppointmentDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null));

		/// <summary>
		/// Returns or sets whether the <see cref="Appointment"/> displayed in the dialog can be edited.
		/// </summary>
		/// <seealso cref="AllowAllDayActivitiesProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ScheduleSettings"/>
		public bool AllowAllDayActivities
		{
			get
			{
				return (bool)this.GetValue(AppointmentDialogCore.AllowAllDayActivitiesProperty);
			}
			set
			{
				this.SetValue(AppointmentDialogCore.AllowAllDayActivitiesProperty, value);
			}
		}

		#endregion //AllowAllDayActivities

		#region Appointment
		/// <summary>
		/// Returns the <see cref="Appointment"/> being edited.  This is a copy of the actual appointment, but will be saved to the
		/// original appointment when the dialog is committed.
		/// </summary>
		public Appointment Appointment
		{
			get	{ return this.Activity as Appointment; }
		}
		#endregion //Appointment
		
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
					this._localizedStrings = new Dictionary<string,string>(10);

					this._localizedStrings.Add("DLG_Appointment_Core_Subject", ScheduleUtilities.GetString("DLG_Appointment_Core_Subject"));
					this._localizedStrings.Add("DLG_Appointment_Core_Location", ScheduleUtilities.GetString("DLG_Appointment_Core_Location"));
					this._localizedStrings.Add("DLG_Appointment_Core_StartTime", ScheduleUtilities.GetString("DLG_Appointment_Core_StartTime"));
					this._localizedStrings.Add("DLG_Appointment_Core_EndTime", ScheduleUtilities.GetString("DLG_Appointment_Core_EndTime"));
					this._localizedStrings.Add("DLG_Appointment_Core_AllDayEvent", ScheduleUtilities.GetString("DLG_Appointment_Core_AllDayEvent"));
					this._localizedStrings.Add("DLG_Appointment_Core_RecurrenceRootLabel", ScheduleUtilities.GetString("DLG_Appointment_Core_RecurrenceRootLabel"));
					this._localizedStrings.Add("DLG_Appointment_Core_RecurrenceRootDescription", ScheduleUtilities.GetString("DLG_Appointment_Core_RecurrenceRootDescription"));
					this._localizedStrings.Add("DLG_Appointment_Core_OccurrenceDescription", ScheduleUtilities.GetString("DLG_Appointment_Core_OccurrenceDescription"));
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#endregion //Public Properties

		#endregion //Properties
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