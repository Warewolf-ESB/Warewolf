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
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom control used to choose an <see cref="ActivityType"/> based on a set of available <see cref="ActivityTypes"/>
	/// </summary>
	public class ActivityTypeChooserDialog : ScheduleDialogBase<ActivityType?>
	{
		#region Member Variables

		private ActivityTypes _availableTypes;
		private XamScheduleDataManager _dataManager;
		
		#endregion //Member Variables

		#region Constructor

		static ActivityTypeChooserDialog()
		{

			ActivityTypeChooserDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityTypeChooserDialog), new FrameworkPropertyMetadata(typeof(ActivityTypeChooserDialog)));

		}

		// AS 6/14/12 TFS113929
		/// <summary>
		/// Constructor used at design time to style the control.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ActivityTypeChooserDialog()
			: base()
		{




			_availableTypes = ActivityTypes.All;
			this.ChoiceIsAppointment = true;
		}

		/// <summary>
		/// Initializes a new <see cref="ActivityTypeChooserDialog"/>
		/// </summary>
		/// <param name="dataManager">The associated XamScheduleDataManager</param>
		/// <param name="chooserReason">An enum value that identifies what triggered the display of the dialog.</param>
		/// <param name="availableTypes">The types from which to choose</param>
		/// <param name="chooserResult">An object that will be initialized with the result</param>
		public ActivityTypeChooserDialog(XamScheduleDataManager dataManager, ActivityTypes availableTypes, ActivityTypeChooserReason chooserReason, ActivityTypeChooserDialog.ChooserResult chooserResult)
			: base(chooserResult)
		{




			CoreUtilities.ValidateNotNull(dataManager, "dataManager");

			_dataManager = dataManager;
			_availableTypes = availableTypes;
			this.ChooserReason = chooserReason;

			DependencyProperty propertyToSelect = null;

			if ((_availableTypes & ActivityTypes.Appointment) == 0)
				this.ChoiceIsAppointmentVisibility = Visibility.Collapsed;
			else if (propertyToSelect == null)
				propertyToSelect = ChoiceIsAppointmentProperty;

			if ((_availableTypes & ActivityTypes.Journal) == 0)
				this.ChoiceIsJournalVisibility = Visibility.Collapsed;
			else if (propertyToSelect == null)
				propertyToSelect = ChoiceIsJournalProperty;

			if ((_availableTypes & ActivityTypes.Task) == 0)
				this.ChoiceIsTaskVisibility = Visibility.Collapsed;
			else if (propertyToSelect == null)
				propertyToSelect = ChoiceIsTaskProperty;

			if (null != propertyToSelect)
				this.SetValue(propertyToSelect, KnownBoxes.TrueBox);
		} 
		#endregion //Constructor

		#region Base class overrides

		#region CanSaveAndClose
		internal override bool CanSaveAndClose
		{
			get { return true; }
		}
		#endregion //CanSaveAndClose

		#region InitializeLocalizedStrings
		internal override void InitializeLocalizedStrings(Dictionary<string, string> localizedStrings)
		{
			base.InitializeLocalizedStrings(localizedStrings);

			localizedStrings["DLG_ActivityTypeChooser_Message"] = ScheduleUtilities.GetString("DLG_ActivityTypeChooser_NewActivityMessage");
			localizedStrings["DLG_ActivityType_Appointment"] = ScheduleUtilities.GetString("DLG_ActivityType_Appointment");
			localizedStrings["DLG_ActivityType_Journal"] = ScheduleUtilities.GetString("DLG_ActivityType_Journal");
			localizedStrings["DLG_ActivityType_Task"] = ScheduleUtilities.GetString("DLG_ActivityType_Task");
			localizedStrings["DLG_ScheduleDialog_Btn_Ok"] = ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok");
			localizedStrings["DLG_ScheduleDialog_Btn_Cancel"] = ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Cancel");
		}
		#endregion //InitializeLocalizedStrings

		#region Save
		internal override void Save()
		{
			ActivityType? type;

			if (this.ChoiceIsAppointment)
				type = ActivityType.Appointment;
			else if (this.ChoiceIsJournal)
				type = ActivityType.Journal;
			else if (this.ChoiceIsTask)
				type = ActivityType.Task;
			else
			{
				Debug.Assert(false, "Unexpected value");
				type = null;
			}

			this.Result.Choice = type;
		}
		#endregion //Save

		#endregion //Base class overrides

		#region Properties

		#region ChoiceIsAppointment

		/// <summary>
		/// Identifies the <see cref="ChoiceIsAppointment"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsAppointmentProperty = DependencyPropertyUtilities.Register("ChoiceIsAppointment",
			typeof(bool), typeof(ActivityTypeChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnChoiceIsAppointmentChanged))
			);

		private static void OnChoiceIsAppointmentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityTypeChooserDialog instance = (ActivityTypeChooserDialog)d;

		}

		/// <summary>
		/// Returns or sets a value indicating whether the chosen activity is an Appointment
		/// </summary>
		/// <seealso cref="ChoiceIsAppointmentProperty"/>
		public bool ChoiceIsAppointment
		{
			get
			{
				return (bool)this.GetValue(ActivityTypeChooserDialog.ChoiceIsAppointmentProperty);
			}
			set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsAppointmentProperty, value);
			}
		}

		#endregion //ChoiceIsAppointment

		#region ChoiceIsAppointmentVisibility

		private static readonly DependencyPropertyKey ChoiceIsAppointmentVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ChoiceIsAppointmentVisibility",
			typeof(Visibility), typeof(ActivityTypeChooserDialog), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ChoiceIsAppointmentVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsAppointmentVisibilityProperty = ChoiceIsAppointmentVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the Appointment choice should be visible to the end user.
		/// </summary>
		/// <seealso cref="ChoiceIsAppointmentVisibilityProperty"/>
		public Visibility ChoiceIsAppointmentVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityTypeChooserDialog.ChoiceIsAppointmentVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsAppointmentVisibilityPropertyKey, value);
			}
		}

		#endregion //ChoiceIsAppointmentVisibility

		#region ChoiceIsJournal

		/// <summary>
		/// Identifies the <see cref="ChoiceIsJournal"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsJournalProperty = DependencyPropertyUtilities.Register("ChoiceIsJournal",
			typeof(bool), typeof(ActivityTypeChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnChoiceIsJournalChanged))
			);

		private static void OnChoiceIsJournalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityTypeChooserDialog instance = (ActivityTypeChooserDialog)d;

		}

		/// <summary>
		/// Returns or sets a value indicating whether the chosen activity is an Journal
		/// </summary>
		/// <seealso cref="ChoiceIsJournalProperty"/>
		public bool ChoiceIsJournal
		{
			get
			{
				return (bool)this.GetValue(ActivityTypeChooserDialog.ChoiceIsJournalProperty);
			}
			set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsJournalProperty, value);
			}
		}

		#endregion //ChoiceIsJournal

		#region ChoiceIsJournalVisibility

		private static readonly DependencyPropertyKey ChoiceIsJournalVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ChoiceIsJournalVisibility",
			typeof(Visibility), typeof(ActivityTypeChooserDialog), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ChoiceIsJournalVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsJournalVisibilityProperty = ChoiceIsJournalVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the Journal choice should be visible to the end user.
		/// </summary>
		/// <seealso cref="ChoiceIsJournalVisibilityProperty"/>
		public Visibility ChoiceIsJournalVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityTypeChooserDialog.ChoiceIsJournalVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsJournalVisibilityPropertyKey, value);
			}
		}

		#endregion //ChoiceIsJournalVisibility

		#region ChoiceIsTask

		/// <summary>
		/// Identifies the <see cref="ChoiceIsTask"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsTaskProperty = DependencyPropertyUtilities.Register("ChoiceIsTask",
			typeof(bool), typeof(ActivityTypeChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnChoiceIsTaskChanged))
			);

		private static void OnChoiceIsTaskChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityTypeChooserDialog instance = (ActivityTypeChooserDialog)d;

		}

		/// <summary>
		/// Returns or sets a value indicating whether the chosen activity is an Task
		/// </summary>
		/// <seealso cref="ChoiceIsTaskProperty"/>
		public bool ChoiceIsTask
		{
			get
			{
				return (bool)this.GetValue(ActivityTypeChooserDialog.ChoiceIsTaskProperty);
			}
			set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsTaskProperty, value);
			}
		}

		#endregion //ChoiceIsTask

		#region ChoiceIsTaskVisibility

		private static readonly DependencyPropertyKey ChoiceIsTaskVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ChoiceIsTaskVisibility",
			typeof(Visibility), typeof(ActivityTypeChooserDialog), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="ChoiceIsTaskVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsTaskVisibilityProperty = ChoiceIsTaskVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating whether the Task choice should be visible to the end user.
		/// </summary>
		/// <seealso cref="ChoiceIsTaskVisibilityProperty"/>
		public Visibility ChoiceIsTaskVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityTypeChooserDialog.ChoiceIsTaskVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ActivityTypeChooserDialog.ChoiceIsTaskVisibilityPropertyKey, value);
			}
		}

		#endregion //ChoiceIsTaskVisibility

		#region ChooserReason

		private static readonly DependencyPropertyKey ChooserReasonPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ChooserReason",
			typeof(ActivityTypeChooserReason), typeof(ActivityTypeChooserDialog), ActivityTypeChooserReason.AddActivityViaDoubleClick, null);

		/// <summary>
		/// Identifies the read-only <see cref="ChooserReason"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChooserReasonProperty = ChooserReasonPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns an enumeration indicating the reason for which the dialog is being displayed.
		/// </summary>
		/// <seealso cref="ChooserReasonProperty"/>
		public ActivityTypeChooserReason ChooserReason
		{
			get
			{
				return (ActivityTypeChooserReason)this.GetValue(ActivityTypeChooserDialog.ChooserReasonProperty);
			}
			internal set
			{
				this.SetValue(ActivityTypeChooserDialog.ChooserReasonPropertyKey, value);
			}
		}

		#endregion //ChooserReason

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

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