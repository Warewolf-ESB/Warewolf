using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;


using Infragistics.Windows.Controls;


namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for choosing an <see cref="Appointment"/> occurrence or series for deletion or opening.
	/// </summary>
	[DesignTimeVisible(false)]
	public class ActivityRecurrenceChooserDialog : ScheduleDialogBase<RecurrenceChooserChoice>
	{
		#region Member Variables

		private ActivityBase					_activity;
		private XamScheduleDataManager			_dataManager;

		#endregion //Member Variables

		#region Constructor

		static ActivityRecurrenceChooserDialog()
		{

			ActivityRecurrenceChooserDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityRecurrenceChooserDialog), new FrameworkPropertyMetadata(typeof(ActivityRecurrenceChooserDialog)));

		}

		// AS 6/14/12 TFS113929
		/// <summary>
		/// Constructor used at design time to style the control.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public ActivityRecurrenceChooserDialog()
			: base()
		{



		}

		/// <summary>
		/// Creates an instance of the RecurrenceChooserDialog which lets the user choose whether to access a recurrence Series or Occurrence
		/// when an <see cref="ActivityBase"/> is opened or deleted.
		/// </summary>
		/// <param name="dataManager">The XamScheduleDataManager for which the dialog is being displayed.</param>
		/// <param name="activity">The recurring <see cref="ActivityBase"/> for which the choice is being made.</param>
		/// <param name="chooserType">The type of choice to be made, i.e. whether the appointment is being opened or deleted.</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance. The dialog will set 
		/// the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		public ActivityRecurrenceChooserDialog(XamScheduleDataManager dataManager, ActivityBase activity, RecurrenceChooserType chooserType, ChooserResult chooserResult) : base(chooserResult)
		{



			CoreUtilities.ValidateNotNull(dataManager, "dataManager");
			CoreUtilities.ValidateNotNull(activity, "activity");

			this._activity		= activity;
			this._dataManager	= dataManager;
			this.ChooserType	= chooserType;
		}
		#endregion //Constructor

		#region Base Class Overrides

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

			string activitySubject = _activity != null ? _activity.Subject : ScheduleUtilities.GetString("DLG_ActivityType_Appointment");

			switch (this.ChooserType)
			{
				case RecurrenceChooserType.ChooseForDeletion:
					{
						localizedStrings.Add("DLG_RecurrenceChooser_Message", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeletingMessage", new object[] { activitySubject }));
						localizedStrings.Add("DLG_RecurrenceChooser_OccurrenceChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeleteOccurrence"));
						localizedStrings.Add("DLG_RecurrenceChooser_SeriesChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeleteSeries"));
						break;
					}

				case RecurrenceChooserType.ChooseForOpening:
					{
						localizedStrings.Add("DLG_RecurrenceChooser_Message", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_OpeningMessage", new object[] { activitySubject }));
						localizedStrings.Add("DLG_RecurrenceChooser_OccurrenceChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_OpenOccurrence"));
						localizedStrings.Add("DLG_RecurrenceChooser_SeriesChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_OpenSeries"));
						break;
					}

				case RecurrenceChooserType.ChooseForCurrentTaskDeletion:
					{
						localizedStrings.Add("DLG_RecurrenceChooser_Message", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeletingCurrentTaskMessage", new object[] { activitySubject }));
						localizedStrings.Add("DLG_RecurrenceChooser_OccurrenceChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeleteCurrentTaskOccurrence"));
						localizedStrings.Add("DLG_RecurrenceChooser_SeriesChoice", ScheduleUtilities.GetString("DLG_RecurrenceChooser_Literal_DeleteCurrentTaskSeries"));
						break;
					}
			}

			localizedStrings.Add("DLG_ScheduleDialog_Btn_Ok", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
			localizedStrings.Add("DLG_ScheduleDialog_Btn_Cancel", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Cancel"));

			// note we're keeping the previously existing key names in case someone copied the 10.3 templates
			localizedStrings.Add("DLG_RecurrenceChooser_OK", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Ok"));
			localizedStrings.Add("DLG_RecurrenceChooser_Cancel", ScheduleUtilities.GetString("DLG_ScheduleDialog_Btn_Cancel"));
		} 
		#endregion //InitializeLocalizedStrings

		#region Save
		internal override void Save()
		{
			if (this.ChoiceIsOccurrence)
				this.Result.Choice = RecurrenceChooserChoice.Occurrence;
			else
				this.Result.Choice = RecurrenceChooserChoice.Series;
		} 
		#endregion //Save

		#endregion //Base Class Overrides

		#region Properties

		#region Private Properties

		#region ChooserType
		private RecurrenceChooserType ChooserType
		{
			get; set;
		}
		#endregion //ChooserType

		#endregion //Private Properties

		#region Public Properties

		#region ChoiceIsOccurrence

		/// <summary>
		/// Identifies the <see cref="ChoiceIsOccurrence"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsOccurrenceProperty = DependencyPropertyUtilities.Register("ChoiceIsOccurrence",
			typeof(bool), typeof(ActivityRecurrenceChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null));

		/// <summary>
		/// Returns true if the choice is the recurrence Occurrence.
		/// </summary>
		/// <seealso cref="ChoiceIsOccurrenceProperty"/>
		public bool ChoiceIsOccurrence
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceChooserDialog.ChoiceIsOccurrenceProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceChooserDialog.ChoiceIsOccurrenceProperty, value);
			}
		}

		#endregion //ChoiceIsOccurrence

		#region ChoiceIsSeries

		/// <summary>
		/// Identifies the <see cref="ChoiceIsSeries"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ChoiceIsSeriesProperty = DependencyPropertyUtilities.Register("ChoiceIsSeries",
			typeof(bool), typeof(ActivityRecurrenceChooserDialog),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, null));

		/// <summary>
		/// Returns true if the choice is the recurrence Series.
		/// </summary>
		/// <seealso cref="ChoiceIsSeriesProperty"/>
		public bool ChoiceIsSeries
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceChooserDialog.ChoiceIsSeriesProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceChooserDialog.ChoiceIsSeriesProperty, value);
			}
		}

		#endregion //ChoiceIsSeries

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

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