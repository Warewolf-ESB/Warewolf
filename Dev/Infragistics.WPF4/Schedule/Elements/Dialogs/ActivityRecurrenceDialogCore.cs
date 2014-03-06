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
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;


using Infragistics.Windows.Controls;


namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for the core portion of the <see cref="ActivityBase"/> Recurrence dialog. 
	/// </summary>





	[TemplatePart(Name = PartStartTimePicker, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartEndTimePicker, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartDurationPicker, Type = typeof(ComboBox))]

	[TemplatePart(Name = PartDailyFrequency, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartMonthlyDayNumber, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartMonthlyFrequency, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartMonthlyFrequencyComplex, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartYearlyDayNumber, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartRangeEndAfterOccurrenceNumber, Type = typeof(FrameworkElement))]
	[TemplatePart(Name = PartMonthlyMoveableElement1, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartMonthlyMoveableElement2, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartMonthlyMoveableElement3, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartMonthlyMoveableElement4, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartMonthlyMoveableElement6, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartYearlyMoveableElement1, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartYearlyMoveableElement2, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartYearlyMoveableElement3, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartYearlyMoveableElement4, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartYearlyMoveableElement5, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartYearlyMoveableElement6, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartYearlyMoveableElement7, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartYearlyMoveableElement8, Type = typeof(TextBlock))]
	[TemplatePart(Name = PartRangeEndAfterDate, Type = typeof(DatePicker))]		// JM 04-05-11 TFS71183
	[DesignTimeVisible(false)]
	public class ActivityRecurrenceDialogCore : Control,
												ICommandTarget,
												IDialogElementProxyHost,
												ITimePickerManagerHost
	{
		#region Member Variables

		// Template part names
		private const string	PartStartTimePicker					= "StartTimePicker";
		private const string	PartEndTimePicker					= "EndTimePicker";
		private const string	PartDurationPicker					= "DurationPicker";
		private const string	PartRootPanel						= "RootPanel";
		private const string	PartDailyFrequency					= "DailyFrequency";
		private const string	PartMonthlyDayNumber				= "MonthlyDayNumber";
		private const string	PartMonthlyFrequency				= "MonthlyFrequency";
		private const string	PartMonthlyFrequencyComplex			= "MonthlyFrequencyComplex";
		private const string	PartYearlyDayNumber					= "YearlyDayNumber";
		private const string	PartRangeEndAfterOccurrenceNumber	= "RangeEndAfterOccurrenceNumber";
		private const string	PartMonthlyMoveableElement1			= "MonthlyMoveableElement1";
		private const string	PartMonthlyMoveableElement2			= "MonthlyMoveableElement2";
		private const string	PartMonthlyMoveableElement3			= "MonthlyMoveableElement3";
		private const string	PartMonthlyMoveableElement4			= "MonthlyMoveableElement4";
		private const string	PartMonthlyMoveableElement6			= "MonthlyMoveableElement6";
		private const string	PartYearlyMoveableElement1			= "YearlyMoveableElement1";
		private const string	PartYearlyMoveableElement2			= "YearlyMoveableElement2";
		private const string	PartYearlyMoveableElement3			= "YearlyMoveableElement3";
		private const string	PartYearlyMoveableElement4			= "YearlyMoveableElement4";
		private const string	PartYearlyMoveableElement5			= "YearlyMoveableElement5";
		private const string	PartYearlyMoveableElement6			= "YearlyMoveableElement6";
		private const string	PartYearlyMoveableElement7			= "YearlyMoveableElement7";
		private const string	PartYearlyMoveableElement8			= "YearlyMoveableElement8";
		private const string	PartRangeEndAfterDate				= "RangeEndDatePicker";		// JM 04-05-11 TFS71183

		// MD 2/17/11 - TFS66136
		private const int DayOfWeekOrdinalValueLast = 4;


		private FrameworkElement							_container;
		//private ActivityBase								_activity;	// JM 04-11-11 TFS70524 Commenting out since using _activityOriginal and _activityClone instead.
		private ActivityBase								_activityOriginal;
		private XamScheduleDataManager						_dataManager;
		private bool										_isDirty;
		private DialogElementProxy							_dialogElementProxy;
		private Dictionary<string, string>					_localizedStrings;
		private bool										_initialized;
		private TimePickerManager							_timePickerManager;
		private TimeZoneTokenHelper							_timeZoneTokenHelper;
		private bool										_shouldCallBeginAndEndEdit;
		private bool										_initializingView;
		private FrameworkElement						_tbDailyFrequency;
		private FrameworkElement						_tbMonthlyDayNumber;
		private FrameworkElement						_tbMonthlyFrequency;
		private FrameworkElement						_tbMonthlyFrequencyComplex;
		private FrameworkElement						_tbYearlyDayNumber;
		private FrameworkElement						_tbRangeEndAfterOccurrenceNumber;
		private bool										_resettingMonthlyPatternTypeComplexViewProperties;
		private bool										_resettingYearlyPatternTypeSimpleViewProperties;
		private bool										_resettingYearlyPatternTypeComplexViewProperties;

		private List<string>								_ordinalDescriptions;
		private List<string>								_dayDescriptions;
		private List<string>								_monthDescriptions;

		// JM 04-11-11 TFS70524.  This is no longer necessary since the dialog no longer 
		// manipulates the recurrence directly on the activity.
		// These variables are only needed in the case where we are being called from the Appointment dialog where
		// a BeginEditWithCopy has already been done on the Activity.  These variables allow us to restore the
		// original state of the Recurrence and the original values of the Start and End fields if the user makes
		// changes in this dialog and then clicks Cancel.  
		//private DateRecurrence								_originalRecurrence;
		//private DateTime									_originalActivityStart;
		//private DateTime									_originalActivityEnd;

		// JM 04-05-11 TFS71183
		private FrameworkElement							_dpRangeEndDatePicker;

		// JM 04-11-11 TFS70524
		private ActivityBase								_activityClone;

		// JM 04-05-11 TFS71182
		private bool										_bypassRangeEndDateNormalization;

		#endregion //Member Variables

		#region Constructors

		static ActivityRecurrenceDialogCore()
		{

			ActivityRecurrenceDialogCore.DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityRecurrenceDialogCore), new FrameworkPropertyMetadata(typeof(ActivityRecurrenceDialogCore)));

		}

		/// <summary>
		/// Creates an instance of the RecurrenceDialogCore control which represents the core of the <see cref="ActivityBase"/> Recurrence dialog.
		/// </summary>
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no containing element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/>.</param>
		/// <param name="activity">The appointment to display.</param>
		public ActivityRecurrenceDialogCore(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity)
		{




			CoreUtilities.ValidateNotNull(activity, "activity");
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");

			this._container				= container;
			this._dataManager			= dataManager;

			// Determine if the activity is already in edit or if we need to perform BeginEdit now.
			this._shouldCallBeginAndEndEdit = activity.IsEditCopy == false;
			if (this._shouldCallBeginAndEndEdit)
			{
				DataErrorInfo errorInfo;
				this._activityOriginal = dataManager.BeginEditWithCopy(activity, true, out errorInfo);
				if (errorInfo != null)	// Display a message on error.
				{
					_dataManager.DisplayErrorMessageBox(errorInfo, ScheduleUtilities.GetString("MSG_TITLE_EditActivity"));

					
				}
			}
			else
			{
				this._activityOriginal = activity;

				// JM 04-11-11 TFS70524.  This is no longer necessary since the dialog no longer 
				// manipulates the recurrence directly on the activity.
				// Save the state of the activity fields which can be changed by the end user in this dialog
				// so that we can restore these values if the end user cancels out of this dialog.  
				//this._originalActivityStart	= activity.Start;
				//this._originalActivityEnd	= activity.End;
				//this._originalRecurrence	= null == activity.Recurrence ? null : activity.Recurrence.Clone() as DateRecurrence;
			}

			// JM 04-11-11 TFS70524.  Work with a clone of the activity.
			this._activityClone			= this._activityOriginal.Clone(true);

			// Set our DataContext to the appointment.
			this.DataContext			= this._activityClone;

			// Set a flag indicating whether the recurrence is removeable.
			this.IsRecurrenceRemoveable = (null != this.Recurrence);

			// Initialize the local and appointment time zone tokens.
			this._timeZoneTokenHelper	= new TimeZoneTokenHelper(this._activityClone, this._dataManager);
			this._timeZoneTokenHelper.VerifyTimeZoneTokens();

			// Set the visibility of the Recurrence description.  Since the description returned from this.Recurrence.ToString()
			// is not currently localized, we will set the visibility to collapsed if the current resources are not english.  We
			// will detect this by loking for the localized string DLG_Recurrence_ShouldShowRecurrenceDescription in strings.resx.
			// The Engligh translation will return the string 'true', other translations will return an empty string.  This logic will
			// be removed once this.Recurrence.ToString() is localized.
#pragma warning disable 436
			if (SR.GetString("DLG_Recurrence_ShouldShowRecurrenceDescription").ToLower().Trim() != "true")
				this.RecurrenceDescriptionVisibility = Visibility.Collapsed;
#pragma warning restore 436
		}

		#endregion //Constructors

		#region Base Class Overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.DialogElementProxy.Initialize();

			// Initialize.
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.Initialize));
		}
		#endregion //OnApplyTemplate

		#region OnKeyUp
		/// <summary>
		/// Called before the System.Windows.UIElement.KeyUp event occurs
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (e.Key == Key.Escape)
				this.Close();
		}
		#endregion //OnKeyUp

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region Activity
		/// <summary>
		/// Returns a clone of the <see cref="Activity"/> whose recurrence is being edited.  
		/// </summary>
		public ActivityBase Activity
		{
			// JM 04-11-11 TFS70524.  Work with a clone of the activity.
			//get { return this._activity; }
			get { return this._activityClone; }
		}
		#endregion //Activity

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

		#region DayDescriptions
		/// <summary>
		/// Returns a list of descriptions for the day choices in the RecurrenceDialog.  These include descriptions for
		/// Sunday thru Saturday as well as Day, Weekday and WeekendDay.
		/// </summary>
		public List<string> DayDescriptions
		{
			get
			{
				if (this._dayDescriptions == null)
				{
#pragma warning disable 436
					this._dayDescriptions = new List<string>(10);

					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_Day"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_WeekDay"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_WeekendDay"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Sunday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Monday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Tuesday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Wednesday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Thursday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Friday"]);
					this._dayDescriptions.Add(this.LocalizedStrings["DLG_Recurrence_WeeklyPattern_Literal_Saturday"]);
#pragma warning restore 436
				}

				return this._dayDescriptions;
			}
		}
		#endregion //DayDescriptions

		#region DayNumberMonthly

		/// <summary>
		/// Identifies the <see cref="DayNumberMonthly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayNumberMonthlyProperty = DependencyPropertyUtilities.Register("DayNumberMonthly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayNumberMonthlyChanged)));

		private static void OnDayNumberMonthlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a Monthly recurrence.
		/// </summary>
		/// <seealso cref="DayNumberMonthlyProperty"/>
		/// <seealso cref="Activity"/>
		public int DayNumberMonthly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayNumberMonthlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayNumberMonthlyProperty, value);
			}
		}

		#endregion //DayNumberMonthly

		#region DayOfMonthYearly

		/// <summary>
		/// Identifies the <see cref="DayOfMonthYearly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfMonthYearlyProperty = DependencyPropertyUtilities.Register("DayOfMonthYearly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayOfMonthYearlyChanged)));

		private static void OnDayOfMonthYearlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingYearlyPatternTypeSimpleViewProperties)
			{
				if (dialog.IsYearlyPatternTypeSimple == false)
					dialog.IsYearlyPatternTypeSimple = true;

				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the day of the month in a yearly recurrence.
		/// </summary>
		/// <seealso cref="DayOfMonthYearlyProperty"/>
		/// <seealso cref="DayDescriptions"/>
		public int DayOfMonthYearly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayOfMonthYearlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayOfMonthYearlyProperty, value);
			}
		}

		#endregion //DayOfMonthYearly

		#region DayOfWeekMonthly

		/// <summary>
		/// Identifies the <see cref="DayOfWeekMonthly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekMonthlyProperty = DependencyPropertyUtilities.Register("DayOfWeekMonthly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayOfWeekMonthlyChanged)));

		private static void OnDayOfWeekMonthlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingMonthlyPatternTypeComplexViewProperties)
			{
				if (dialog.IsMonthlyPatternTypeComplex == false)
					dialog.IsMonthlyPatternTypeComplex	= true;

				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="DayDescriptions"/> collection which represents the day of week in a monthly recurrence.
		/// </summary>
		/// <seealso cref="DayOfWeekMonthlyProperty"/>
		/// <seealso cref="DayDescriptions"/>
		public int DayOfWeekMonthly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayOfWeekMonthlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayOfWeekMonthlyProperty, value);
			}
		}

		#endregion //DayOfWeekMonthly

		#region DayOfWeekYearly

		/// <summary>
		/// Identifies the <see cref="DayOfWeekYearly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekYearlyProperty = DependencyPropertyUtilities.Register("DayOfWeekYearly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayOfWeekYearlyChanged)));

		private static void OnDayOfWeekYearlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingYearlyPatternTypeComplexViewProperties)
			{
				if (dialog.IsYearlyPatternTypeComplex == false)
					dialog.IsYearlyPatternTypeComplex = true;

				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="DayDescriptions"/> collection which represents the day of week in a yearly recurrence.
		/// </summary>
		/// <seealso cref="DayOfWeekYearlyProperty"/>
		/// <seealso cref="DayDescriptions"/>
		public int DayOfWeekYearly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayOfWeekYearlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayOfWeekYearlyProperty, value);
			}
		}

		#endregion //DayOfWeekYearly

		#region DayOfWeekOrdinalMonthly

		/// <summary>
		/// Identifies the <see cref="DayOfWeekOrdinalMonthly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekOrdinalMonthlyProperty = DependencyPropertyUtilities.Register("DayOfWeekOrdinalMonthly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayOfWeekOrdinalMonthlyChanged)));

		private static void OnDayOfWeekOrdinalMonthlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingMonthlyPatternTypeComplexViewProperties)
			{
				if (dialog.IsMonthlyPatternTypeComplex == false)
					dialog.IsMonthlyPatternTypeComplex	= true;

				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="OrdinalDescriptions"/> collection which represents the ordinal position of the day of week in a monthly recurrence.
		/// </summary>
		/// <seealso cref="DayOfWeekOrdinalMonthlyProperty"/>
		/// <seealso cref="OrdinalDescriptions"/>
		public int DayOfWeekOrdinalMonthly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayOfWeekOrdinalMonthlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayOfWeekOrdinalMonthlyProperty, value);
			}
		}

		#endregion //DayOfWeekOrdinalMonthly

		#region DayOfWeekOrdinalYearly

		/// <summary>
		/// Identifies the <see cref="DayOfWeekOrdinalYearly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekOrdinalYearlyProperty = DependencyPropertyUtilities.Register("DayOfWeekOrdinalYearly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnDayOfWeekOrdinalYearlyChanged)));

		private static void OnDayOfWeekOrdinalYearlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingYearlyPatternTypeComplexViewProperties)
			{
				if (dialog.IsYearlyPatternTypeComplex == false)
					dialog.IsYearlyPatternTypeComplex = true;

				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="OrdinalDescriptions"/> collection which represents the ordinal position of the day of week in a yearly recurrence.
		/// </summary>
		/// <seealso cref="DayOfWeekOrdinalYearlyProperty"/>
		/// <seealso cref="OrdinalDescriptions"/>
		public int DayOfWeekOrdinalYearly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.DayOfWeekOrdinalYearlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.DayOfWeekOrdinalYearlyProperty, value);
			}
		}

		#endregion //DayOfWeekOrdinalYearly

		#region EndTimePickerVisibility
		/// <summary>
		/// Identifies the <see cref="EndTimePickerVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndTimePickerVisibilityProperty = DependencyPropertyUtilities.Register("EndTimePickerVisibility",
			typeof(Visibility), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Visible)
			);

		/// <summary>
		/// Returns/sets whether the dialog's End TimePicker should be visible.
		/// </summary>
		/// <seealso cref="EndTimePickerVisibilityProperty"/>
		public Visibility EndTimePickerVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityRecurrenceDialogCore.EndTimePickerVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.EndTimePickerVisibilityProperty, value);
			}
		}
		#endregion //EndTimePickerVisibility

		#region IntervalDaily

		/// <summary>
		/// Identifies the <see cref="IntervalDaily"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IntervalDailyProperty = DependencyPropertyUtilities.Register("IntervalDaily",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnIntervalDailyChanged)));

		private static void OnIntervalDailyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateDailyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a Daily recurrence.
		/// </summary>
		/// <seealso cref="IntervalDailyProperty"/>
		/// <seealso cref="IntervalWeekly"/>
		/// <seealso cref="IntervalMonthly"/>
		/// <seealso cref="IntervalMonthlyComplex"/>
		/// <seealso cref="IntervalYearly"/>
		/// <seealso cref="Activity"/>
		public int IntervalDaily
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.IntervalDailyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IntervalDailyProperty, value);
			}
		}

		#endregion //IntervalDaily

		#region IntervalWeekly

		/// <summary>
		/// Identifies the <see cref="IntervalWeekly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IntervalWeeklyProperty = DependencyPropertyUtilities.Register("IntervalWeekly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnIntervalWeeklyChanged)));

		private static void OnIntervalWeeklyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a Weekly recurrence.
		/// </summary>
		/// <seealso cref="IntervalWeeklyProperty"/>
		/// <seealso cref="IntervalDaily"/>
		/// <seealso cref="IntervalMonthly"/>
		/// <seealso cref="IntervalMonthlyComplex"/>
		/// <seealso cref="IntervalYearly"/>
		/// <seealso cref="Activity"/>
		public int IntervalWeekly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.IntervalWeeklyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IntervalWeeklyProperty, value);
			}
		}

		#endregion //IntervalWeekly

		#region IntervalMonthly

		/// <summary>
		/// Identifies the <see cref="IntervalMonthly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IntervalMonthlyProperty = DependencyPropertyUtilities.Register("IntervalMonthly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnIntervalMonthlyChanged)));

		private static void OnIntervalMonthlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a Monthly recurrence.
		/// </summary>
		/// <seealso cref="IntervalMonthlyProperty"/>
		/// <seealso cref="IntervalDaily"/>
		/// <seealso cref="IntervalWeekly"/>
		/// <seealso cref="IntervalMonthlyComplex"/>
		/// <seealso cref="IntervalYearly"/>
		/// <seealso cref="Activity"/>
		public int IntervalMonthly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.IntervalMonthlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IntervalMonthlyProperty, value);
			}
		}

		#endregion //IntervalMonthly

		#region IntervalMonthlyComplex

		/// <summary>
		/// Identifies the <see cref="IntervalMonthlyComplex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IntervalMonthlyComplexProperty = DependencyPropertyUtilities.Register("IntervalMonthlyComplex",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnIntervalMonthlyComplexChanged)));

		private static void OnIntervalMonthlyComplexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a complex Monthly recurrence.
		/// </summary>
		/// <seealso cref="IntervalMonthlyComplexProperty"/>
		/// <seealso cref="IntervalDaily"/>
		/// <seealso cref="IntervalWeekly"/>
		/// <seealso cref="IntervalMonthly"/>
		/// <seealso cref="IntervalYearly"/>
		/// <seealso cref="Activity"/>
		public int IntervalMonthlyComplex
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.IntervalMonthlyComplexProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IntervalMonthlyComplexProperty, value);
			}
		}

		#endregion //IntervalMonthlyComplex

		#region IntervalYearly

		/// <summary>
		/// Identifies the <see cref="IntervalYearly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IntervalYearlyProperty = DependencyPropertyUtilities.Register("IntervalYearly",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnIntervalYearlyChanged)));

		private static void OnIntervalYearlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the interval for a Yearly recurrence.
		/// </summary>
		/// <seealso cref="IntervalYearlyProperty"/>
		/// <seealso cref="IntervalDaily"/>
		/// <seealso cref="IntervalWeekly"/>
		/// <seealso cref="IntervalMonthly"/>
		/// <seealso cref="IntervalMonthlyComplex"/>
		/// <seealso cref="Activity"/>
		public int IntervalYearly
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.IntervalYearlyProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IntervalYearlyProperty, value);
			}
		}

		#endregion //IntervalYearly

		#region IsActivityModifiable

		/// <summary>
		/// Identifies the <see cref="IsActivityModifiable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActivityModifiableProperty = DependencyPropertyUtilities.Register("IsActivityModifiable",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null));

		/// <summary>
		/// Returns or sets whether the <see cref="ActivityBase"/> displayed in the dialog can be edited.
		/// </summary>
		/// <seealso cref="IsActivityModifiableProperty"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="ScheduleSettings"/>
		public bool IsActivityModifiable
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsActivityModifiableProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsActivityModifiableProperty, value);
			}
		}

		#endregion //IsActivityModifiable

		#region IsDailyPattern

		/// <summary>
		/// Identifies the <see cref="IsDailyPattern"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDailyPatternProperty = DependencyPropertyUtilities.Register("IsDailyPattern",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDailyPatternChanged)));

		private static void OnIsDailyPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				ActivityRecurrenceDialogCore dialog		= d as ActivityRecurrenceDialogCore;

				// Call InitializeParts since the template may not have been fully hydrated in SL when it was originally
				// called in SL
				dialog.InitializeParts();

				// Always set the recurrence to the default recurrence for this pattern if we are
 				// already initialized.
				if (dialog.IsInitializationComplete == true)
				{
					dialog.Recurrence = dialog.GetDefaultDailyRecurrence();
					dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified
					dialog.UpdateDefaultRangeEndDateBasedOnPattern(PatternType.Daily);
					dialog.UpdateDirtyStatus(true);
				}

				dialog.UpdateViewModelFromDailyRecurrence();
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence pattern is daily.
		/// </summary>
		/// <seealso cref="IsDailyPatternProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsMonthlyPattern"/>
		/// <seealso cref="IsYearlyPattern"/>
		/// <seealso cref="Activity"/>
		public bool IsDailyPattern
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsDailyPatternProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsDailyPatternProperty, value);
			}
		}

		#endregion //IsDailyPattern

		#region IsDailyPatternTypeEvery

		/// <summary>
		/// Identifies the <see cref="IsDailyPatternTypeEvery"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDailyPatternTypeEveryProperty = DependencyPropertyUtilities.Register("IsDailyPatternTypeEvery",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDailyPatternTypeEveryChanged)));

		private static void OnIsDailyPatternTypeEveryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore	dialog		= d as ActivityRecurrenceDialogCore;
			DateRecurrence			recurrence	= dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				if ((bool)e.NewValue == true)
					dialog.UpdateDailyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the daily pattern type is 'every 'n' days'.
		/// </summary>
		/// <seealso cref="IsDailyPattern"/>
		/// <seealso cref="IsDailyPatternTypeWeekday"/>
		public bool IsDailyPatternTypeEvery
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsDailyPatternTypeEveryProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsDailyPatternTypeEveryProperty, value);
			}
		}

		#endregion //IsDailyPatternTypeEvery

		#region IsDailyPatternTypeWeekday

		/// <summary>
		/// Identifies the <see cref="IsDailyPatternTypeWeekday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDailyPatternTypeWeekdayProperty = DependencyPropertyUtilities.Register("IsDailyPatternTypeWeekday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsDailyPatternTypeWeekdayChanged)));

		private static void OnIsDailyPatternTypeWeekdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore	dialog		= d as ActivityRecurrenceDialogCore;
			DateRecurrence			recurrence	= dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				if ((bool)e.NewValue == true)
					dialog.UpdateDailyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the daily pattern type is 'every weekday'.
		/// </summary>
		/// <seealso cref="IsDailyPattern"/>
		/// <seealso cref="IsDailyPatternTypeEvery"/>
		public bool IsDailyPatternTypeWeekday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsDailyPatternTypeWeekdayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsDailyPatternTypeWeekdayProperty, value);
			}
		}

		#endregion //IsDailyPatternTypeWeekday

		#region IsMonthlyPattern

		/// <summary>
		/// Identifies the <see cref="IsMonthlyPattern"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMonthlyPatternProperty = DependencyPropertyUtilities.Register("IsMonthlyPattern",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMonthlyPatternChanged)));

		private static void OnIsMonthlyPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				ActivityRecurrenceDialogCore dialog	= d as ActivityRecurrenceDialogCore;

				// Call InitializeParts since the template may not have been fully hydrated in SL when it was originally
				// called in SL
				dialog.InitializeParts();

				// Always set the recurrence to the default recurrence for this pattern if we are
				// already initialized.
				if (dialog.IsInitializationComplete == true)
				{
					dialog.Recurrence = dialog.GetDefaultMonthlyRecurrence();
					dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified
					dialog.UpdateDefaultRangeEndDateBasedOnPattern(PatternType.Monthly);
					dialog.UpdateDirtyStatus(true);
				}

				dialog.UpdateViewModelFromMonthlyRecurrence();
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence pattern is monthly.
		/// </summary>
		/// <seealso cref="IsMonthlyPatternProperty"/>
		/// <seealso cref="IsDailyPattern"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsYearlyPattern"/>
		/// <seealso cref="Activity"/>
		public bool IsMonthlyPattern
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternProperty, value);
			}
		}

		#endregion //IsMonthlyPattern

		#region IsMonthlyPatternTypeSimple

		/// <summary>
		/// Identifies the <see cref="IsMonthlyPatternTypeSimple"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMonthlyPatternTypeSimpleProperty = DependencyPropertyUtilities.Register("IsMonthlyPatternTypeSimple",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMonthlyPatternTypeSimpleChanged)));

		private static void OnIsMonthlyPatternTypeSimpleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the monthly pattern type is 'on the nth day of every 'n' months'.
		/// </summary>
		/// <seealso cref="IsMonthlyPattern"/>
		/// <seealso cref="IsMonthlyPatternTypeComplex"/>
		public bool IsMonthlyPatternTypeSimple
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternTypeSimpleProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternTypeSimpleProperty, value);
			}
		}

		#endregion //IsMonthlyPatternTypeSimple

		#region IsMonthlyPatternTypeComplex

		/// <summary>
		/// Identifies the <see cref="IsMonthlyPatternTypeComplex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsMonthlyPatternTypeComplexProperty = DependencyPropertyUtilities.Register("IsMonthlyPatternTypeComplex",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsMonthlyPatternTypeComplexChanged)));

		private static void OnIsMonthlyPatternTypeComplexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateMonthlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the monthly pattern type is 'on the nth day of every 'n' month'.
		/// </summary>
		/// <seealso cref="IsMonthlyPattern"/>
		/// <seealso cref="IsMonthlyPatternTypeSimple"/>
		public bool IsMonthlyPatternTypeComplex
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternTypeComplexProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsMonthlyPatternTypeComplexProperty, value);
			}
		}

		#endregion //IsMonthlyPatternTypeComplex

		#region IsRangeEndAfter

		/// <summary>
		/// Identifies the <see cref="IsRangeEndAfter"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRangeEndAfterProperty = DependencyPropertyUtilities.Register("IsRangeEndAfter",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsRangeEndAfterChanged)));

		private static void OnIsRangeEndAfterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence ends after a specific number of occurrences.
		/// </summary>
		/// <seealso cref="IsRangeEndAfterProperty"/>
		/// <seealso cref="IsRangeEndBy"/>
		/// <seealso cref="IsRangeForever"/>
		/// <seealso cref="RangeEndAfterOccurrenceNumber"/>
		/// <seealso cref="RangeEndDate"/>
		public bool IsRangeEndAfter
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsRangeEndAfterProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsRangeEndAfterProperty, value);
			}
		}

		#endregion //IsRangeEndAfter

		#region IsRangeEndBy

		/// <summary>
		/// Identifies the <see cref="IsRangeEndBy"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRangeEndByProperty = DependencyPropertyUtilities.Register("IsRangeEndBy",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsRangeEndByChanged)));

		private static void OnIsRangeEndByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence ends at a specific date.
		/// </summary>
		/// <seealso cref="IsRangeEndByProperty"/>
		/// <seealso cref="IsRangeEndAfter"/>
		/// <seealso cref="IsRangeForever"/>
		/// <seealso cref="RangeEndAfterOccurrenceNumber"/>
		/// <seealso cref="RangeEndDate"/>
		public bool IsRangeEndBy
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsRangeEndByProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsRangeEndByProperty, value);
			}
		}

		#endregion //IsRangeEndBy

		#region IsRangeForever

		/// <summary>
		/// Identifies the <see cref="IsRangeForever"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRangeForeverProperty = DependencyPropertyUtilities.Register("IsRangeForever",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsRangeForeverChanged)));

		private static void OnIsRangeForeverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence repeats forever with no end date.
		/// </summary>
		/// <seealso cref="IsRangeForeverProperty"/>
		/// <seealso cref="IsRangeEndAfter"/>
		/// <seealso cref="IsRangeEndBy"/>
		/// <seealso cref="RangeEndAfterOccurrenceNumber"/>
		/// <seealso cref="RangeEndDate"/>
		public bool IsRangeForever
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsRangeForeverProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsRangeForeverProperty, value);
			}
		}

		#endregion //IsRangeForever

		#region IsWeeklyPattern

		/// <summary>
		/// Identifies the <see cref="IsWeeklyPattern"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyPatternProperty = DependencyPropertyUtilities.Register("IsWeeklyPattern",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyPatternChanged)));

		private static void OnIsWeeklyPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				ActivityRecurrenceDialogCore dialog	= d as ActivityRecurrenceDialogCore;

				// Call InitializeParts since the template may not have been fully hydrated in SL when it was originally
				// called in SL
				dialog.InitializeParts();

				// Always set the recurrence to the default recurrence for this pattern if we are
				// already initialized.
				if (dialog.IsInitializationComplete == true)
				{
					dialog.Recurrence = dialog.GetDefaultWeeklyRecurrence();
					dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified
					dialog.UpdateDefaultRangeEndDateBasedOnPattern(PatternType.Weekly);
					dialog.UpdateDirtyStatus(true);
				}

				dialog.UpdateViewModelFromWeeklyRecurrence();
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence pattern is weekly.
		/// </summary>
		/// <seealso cref="IsWeeklyPatternProperty"/>
		/// <seealso cref="IsDailyPattern"/>
		/// <seealso cref="IsMonthlyPattern"/>
		/// <seealso cref="IsYearlyPattern"/>
		/// <seealso cref="Activity"/>
		public bool IsWeeklyPattern
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyPatternProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyPatternProperty, value);
			}
		}

		#endregion //IsWeeklyPattern

		#region IsWeeklyOnSunday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnSunday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnSundayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnSunday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnSundayChanged)));

		private static void OnIsWeeklyOnSundayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Sunday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnSundayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnSunday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnSundayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnSundayProperty, value);
			}
		}

		#endregion //IsWeeklyOnSunday

		#region IsWeeklyOnMonday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnMonday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnMondayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnMonday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnMondayChanged)));

		private static void OnIsWeeklyOnMondayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Monday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnMondayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnMonday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnMondayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnMondayProperty, value);
			}
		}

		#endregion //IsWeeklyOnMonday

		#region IsWeeklyOnTuesday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnTuesday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnTuesdayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnTuesday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnTuesdayChanged)));

		private static void OnIsWeeklyOnTuesdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Tuesday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnTuesdayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnTuesday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnTuesdayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnTuesdayProperty, value);
			}
		}

		#endregion //IsWeeklyOnTuesday

		#region IsWeeklyOnWednesday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnWednesday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnWednesdayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnWednesday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnWednesdayChanged)));

		private static void OnIsWeeklyOnWednesdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Wednesday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnWednesdayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnWednesday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnWednesdayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnWednesdayProperty, value);
			}
		}

		#endregion //IsWeeklyOnWednesday

		#region IsWeeklyOnThursday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnThursday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnThursdayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnThursday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnThursdayChanged)));

		private static void OnIsWeeklyOnThursdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Thursday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnThursdayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnThursday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnThursdayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnThursdayProperty, value);
			}
		}

		#endregion //IsWeeklyOnThursday

		#region IsWeeklyOnFriday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnFriday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnFridayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnFriday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnFridayChanged)));

		private static void OnIsWeeklyOnFridayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Friday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnFridayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnSaturday"/>
		public bool IsWeeklyOnFriday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnFridayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnFridayProperty, value);
			}
		}

		#endregion //IsWeeklyOnFriday

		#region IsWeeklyOnSaturday

		/// <summary>
		/// Identifies the <see cref="IsWeeklyOnSaturday"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsWeeklyOnSaturdayProperty = DependencyPropertyUtilities.Register("IsWeeklyOnSaturday",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsWeeklyOnSaturdayChanged)));

		private static void OnIsWeeklyOnSaturdayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateWeeklyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the weekly recurrence pattern occurs on Saturday.
		/// </summary>
		/// <seealso cref="IsWeeklyOnSaturdayProperty"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsWeeklyOnSunday"/>
		/// <seealso cref="IsWeeklyOnMonday"/>
		/// <seealso cref="IsWeeklyOnTuesday"/>
		/// <seealso cref="IsWeeklyOnWednesday"/>
		/// <seealso cref="IsWeeklyOnThursday"/>
		/// <seealso cref="IsWeeklyOnFriday"/>
		public bool IsWeeklyOnSaturday
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsWeeklyOnSaturdayProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsWeeklyOnSaturdayProperty, value);
			}
		}

		#endregion //IsWeeklyOnSaturday

		#region IsYearlyPattern

		/// <summary>
		/// Identifies the <see cref="IsYearlyPattern"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsYearlyPatternProperty = DependencyPropertyUtilities.Register("IsYearlyPattern",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsYearlyPatternChanged)));

		private static void OnIsYearlyPatternChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if ((bool)e.NewValue == true)
			{
				ActivityRecurrenceDialogCore dialog	= d as ActivityRecurrenceDialogCore;

				// Call InitializeParts since the template may not have been fully hydrated in SL when it was originally
				// called in SL
				dialog.InitializeParts();

				// Always set the recurrence to the default recurrence for this pattern if we are
				// already initialized.
				if (dialog.IsInitializationComplete == true)
				{
					dialog.Recurrence = dialog.GetDefaultYearlyRecurrence();
					dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified
					dialog.UpdateDefaultRangeEndDateBasedOnPattern(PatternType.Yearly);
					dialog.UpdateDirtyStatus(true);
				}

				dialog.UpdateViewModelFromYearlyRecurrence();
			}
		}

		/// <summary>
		/// Returns or sets whether the recurrence pattern is yearly.
		/// </summary>
		/// <seealso cref="IsYearlyPatternProperty"/>
		/// <seealso cref="IsDailyPattern"/>
		/// <seealso cref="IsWeeklyPattern"/>
		/// <seealso cref="IsMonthlyPattern"/>
		/// <seealso cref="Activity"/>
		public bool IsYearlyPattern
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsYearlyPatternProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsYearlyPatternProperty, value);
			}
		}

		#endregion //IsYearlyPattern

		#region IsYearlyPatternTypeSimple

		/// <summary>
		/// Identifies the <see cref="IsYearlyPatternTypeSimple"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsYearlyPatternTypeSimpleProperty = DependencyPropertyUtilities.Register("IsYearlyPatternTypeSimple",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsYearlyPatternTypeSimpleChanged)));

		private static void OnIsYearlyPatternTypeSimpleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the yearly pattern type is on a specific date within the year.
		/// </summary>
		/// <seealso cref="IsYearlyPattern"/>
		/// <seealso cref="IsYearlyPatternTypeComplex"/>
		public bool IsYearlyPatternTypeSimple
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsYearlyPatternTypeSimpleProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsYearlyPatternTypeSimpleProperty, value);
			}
		}

		#endregion //IsYearlyPatternTypeSimple

		#region IsYearlyPatternTypeComplex

		/// <summary>
		/// Identifies the <see cref="IsYearlyPatternTypeComplex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsYearlyPatternTypeComplexProperty = DependencyPropertyUtilities.Register("IsYearlyPatternTypeComplex",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsYearlyPatternTypeComplexChanged)));

		private static void OnIsYearlyPatternTypeComplexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;
			DateRecurrence recurrence = dialog.Recurrence;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete)
			{
				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets whether the yearly pattern type is 'on the nth day within month 'x'' within the year.
		/// </summary>
		/// <seealso cref="IsYearlyPattern"/>
		/// <seealso cref="IsYearlyPatternTypeSimple"/>
		public bool IsYearlyPatternTypeComplex
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsYearlyPatternTypeComplexProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsYearlyPatternTypeComplexProperty, value);
			}
		}

		#endregion //IsYearlyPatternTypeComplex

		#region IsRecurrenceRemoveable

		/// <summary>
		/// Identifies the <see cref="IsRecurrenceRemoveable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRecurrenceRemoveableProperty = DependencyPropertyUtilities.Register("IsRecurrenceRemoveable",
			typeof(bool), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, null));

		/// <summary>
		/// Returns or sets whether the <see cref="Recurrence"/> being displayed in the dialog can be removed.
		/// </summary>
		/// <seealso cref="IsRecurrenceRemoveableProperty"/>
		/// <seealso cref="RecurrenceBase"/>
		/// <seealso cref="DateRecurrence"/>
		public bool IsRecurrenceRemoveable
		{
			get
			{
				return (bool)this.GetValue(ActivityRecurrenceDialogCore.IsRecurrenceRemoveableProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.IsRecurrenceRemoveableProperty, value);
			}
		}

		#endregion //IsRecurrenceRemoveable

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

					DateTimeFormatInfo dtfi = this._dataManager.DateInfoProviderResolved.DateTimeFormatInfo;

#pragma warning disable 436
					this._localizedStrings.Add("DLG_Recurrence_Core_ActivityTime", SR.GetString("DLG_Recurrence_Core_ActivityTime"));
					this._localizedStrings.Add("DLG_Recurrence_Core_Start", SR.GetString("DLG_Recurrence_Core_Start"));
					this._localizedStrings.Add("DLG_Recurrence_Core_End", SR.GetString("DLG_Recurrence_Core_End"));
					this._localizedStrings.Add("DLG_Recurrence_Core_Duration", SR.GetString("DLG_Recurrence_Core_Duration"));
					this._localizedStrings.Add("DLG_Recurrence_Core_RecurrencePattern", SR.GetString("DLG_Recurrence_Core_RecurrencePattern"));
					this._localizedStrings.Add("DLG_Recurrence_Core_RecurrenceRange", SR.GetString("DLG_Recurrence_Core_RecurrenceRange"));
					this._localizedStrings.Add("DLG_ScheduleDialog_Btn_Ok", SR.GetString("DLG_ScheduleDialog_Btn_Ok"));
					this._localizedStrings.Add("DLG_ScheduleDialog_Btn_Cancel", SR.GetString("DLG_ScheduleDialog_Btn_Cancel"));

					// note we're keeping the previously existing key names in case someone copied the 10.3 templates
					this._localizedStrings.Add("DLG_Recurrence_Core_OK", SR.GetString("DLG_ScheduleDialog_Btn_Ok"));
					this._localizedStrings.Add("DLG_Recurrence_Core_Cancel", SR.GetString("DLG_ScheduleDialog_Btn_Cancel"));

					this._localizedStrings.Add("DLG_Recurrence_Core_RemoveRecurrence", SR.GetString("DLG_Recurrence_Core_RemoveRecurrence"));
					this._localizedStrings.Add("DLG_Recurrence_Pattern_Literal_Daily", SR.GetString("DLG_Recurrence_Pattern_Literal_Daily"));
					this._localizedStrings.Add("DLG_Recurrence_Pattern_Literal_Weekly", SR.GetString("DLG_Recurrence_Pattern_Literal_Weekly"));
					this._localizedStrings.Add("DLG_Recurrence_Pattern_Literal_Monthly", SR.GetString("DLG_Recurrence_Pattern_Literal_Monthly"));
					this._localizedStrings.Add("DLG_Recurrence_Pattern_Literal_Yearly", SR.GetString("DLG_Recurrence_Pattern_Literal_Yearly"));
					this._localizedStrings.Add("DLG_Recurrence_DailyPattern_Literal_Every", SR.GetString("DLG_Recurrence_DailyPattern_Literal_Every"));
					this._localizedStrings.Add("DLG_Recurrence_DailyPattern_Literal_Days", SR.GetString("DLG_Recurrence_DailyPattern_Literal_Days"));
					this._localizedStrings.Add("DLG_Recurrence_DailyPattern_Literal_EveryWeekday", SR.GetString("DLG_Recurrence_DailyPattern_Literal_EveryWeekday"));
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_RecurEvery", SR.GetString("DLG_Recurrence_WeeklyPattern_Literal_RecurEvery"));
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_WeeksOn", SR.GetString("DLG_Recurrence_WeeklyPattern_Literal_WeeksOn"));
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Sunday", dtfi.DayNames[0]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Monday", dtfi.DayNames[1]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Tuesday", dtfi.DayNames[2]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Wednesday", dtfi.DayNames[3]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Thursday", dtfi.DayNames[4]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Friday", dtfi.DayNames[5]);
					this._localizedStrings.Add("DLG_Recurrence_WeeklyPattern_Literal_Saturday", dtfi.DayNames[6]);
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_Day", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Day"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_WeekDay", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_WeekDay"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_WeekendDay", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_WeekendDay"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_DayCapitalized", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_DayCapitalized"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField2", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField2"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField4", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField4"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField6", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_ComplexTextField6"));
					this._localizedStrings.Add("DLG_Recurrence_MonthlyPattern_Literal_The", SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_The"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_RecurEvery", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_RecurEvery"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_Years", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_Years"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_On", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_On"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_OnThe", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_OnThe"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_SimpleTextField", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_SimpleTextField"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField2", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField2"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField4", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField4"));
					this._localizedStrings.Add("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField6", SR.GetString("DLG_Recurrence_YearlyPattern_Literal_ComplexTextField6"));
					this._localizedStrings.Add("DLG_Recurrence_Range_Literal_Start", SR.GetString("DLG_Recurrence_Range_Literal_Start"));
					this._localizedStrings.Add("DLG_Recurrence_Range_Literal_NoEndDate", SR.GetString("DLG_Recurrence_Range_Literal_NoEndDate"));
					this._localizedStrings.Add("DLG_Recurrence_Range_Literal_EndAfter", SR.GetString("DLG_Recurrence_Range_Literal_EndAfter"));
					this._localizedStrings.Add("DLG_Recurrence_Range_Literal_Occurrences", SR.GetString("DLG_Recurrence_Range_Literal_Occurrences"));
					this._localizedStrings.Add("DLG_Recurrence_Range_Literal_EndBy", SR.GetString("DLG_Recurrence_Range_Literal_EndBy"));
#pragma warning restore 436
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#region MonthDescriptions
		/// <summary>
		/// Returns a list of Month descriptions.
		/// </summary>
		public List<string> MonthDescriptions
		{
			get
			{
				if (this._monthDescriptions == null)
				{
					string [] monthNames	= this._dataManager.DateInfoProviderResolved.DateTimeFormatInfo.MonthNames;
					this._monthDescriptions = new List<string>(monthNames);
				}

				return this._monthDescriptions;
			}
		}
		#endregion //MonthDescriptions

		#region MonthOfYearSimple

		/// <summary>
		/// Identifies the <see cref="MonthOfYearSimple"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MonthOfYearSimpleProperty = DependencyPropertyUtilities.Register("MonthOfYearSimple",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnMonthOfYearSimpleChanged)));

		private static void OnMonthOfYearSimpleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingYearlyPatternTypeSimpleViewProperties)
			{
				if (dialog.IsYearlyPatternTypeSimple == false)
					dialog.IsYearlyPatternTypeSimple = true;

				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="MonthDescriptions"/> collection which represents the month in a yearly recurrence.
		/// </summary>
		/// <seealso cref="MonthOfYearSimpleProperty"/>
		/// <seealso cref="MonthDescriptions"/>
		public int MonthOfYearSimple
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.MonthOfYearSimpleProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.MonthOfYearSimpleProperty, value);
			}
		}

		#endregion //MonthOfYearSimple

		#region MonthOfYearComplex

		/// <summary>
		/// Identifies the <see cref="MonthOfYearComplex"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MonthOfYearComplexProperty = DependencyPropertyUtilities.Register("MonthOfYearComplex",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)1, new PropertyChangedCallback(OnMonthOfYearComplexChanged)));

		private static void OnMonthOfYearComplexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView && true == dialog.IsInitializationComplete && false == dialog._resettingYearlyPatternTypeComplexViewProperties)
			{
				if (dialog.IsYearlyPatternTypeComplex == false)
					dialog.IsYearlyPatternTypeComplex = true;

				dialog.UpdateYearlyRecurrenceFromViewModel();

				if (dialog.IsInitializationComplete == true)
					dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the index within the <see cref="MonthDescriptions"/> collection which represents the month in a yearly recurrence.
		/// </summary>
		/// <seealso cref="MonthOfYearComplexProperty"/>
		/// <seealso cref="MonthDescriptions"/>
		public int MonthOfYearComplex
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.MonthOfYearComplexProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.MonthOfYearComplexProperty, value);
			}
		}

		#endregion //MonthOfYearComplex

		#region OrdinalDescriptions
		/// <summary>
		/// Returns a list of ordinal position descriptions that includes first, second, third, fourth and last.
		/// </summary>
		public List<string> OrdinalDescriptions
		{
			get
			{
				if (this._ordinalDescriptions == null)
				{
#pragma warning disable 436
					this._ordinalDescriptions = new List<string>(5);

					this._ordinalDescriptions.Add(SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Ordinal_First"));
					this._ordinalDescriptions.Add(SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Ordinal_Second"));
					this._ordinalDescriptions.Add(SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Ordinal_Third"));
					this._ordinalDescriptions.Add(SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Ordinal_Fourth"));
					this._ordinalDescriptions.Add(SR.GetString("DLG_Recurrence_MonthlyPattern_Literal_Ordinal_Last"));
#pragma warning restore 436
				}

				return this._ordinalDescriptions;
			}
		}
		#endregion //OrdinalDescriptions

		#region RangeStartDate

		/// <summary>
		/// Identifies the <see cref="RangeStartDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RangeStartDateProperty = DependencyPropertyUtilities.Register("RangeStartDate",
			typeof(DateTime?), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Now.Date, new PropertyChangedCallback(OnRangeStartDateChanged)));

		private static void OnRangeStartDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView)
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

			if (dialog.IsInitializationComplete == true)
				dialog.UpdateDirtyStatus(true);
		}

		/// <summary>
		/// Returns or sets the start date of the recurrence.
		/// </summary>
		/// <seealso cref="RangeStartDateProperty"/>
		/// <seealso cref="RangeEndDate"/>
		public DateTime? RangeStartDate
		{
			get
			{
				return (DateTime?)this.GetValue(ActivityRecurrenceDialogCore.RangeStartDateProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.RangeStartDateProperty, value);
			}
		}

		#endregion //RangeStartDate

		#region RangeEndDate

		/// <summary>
		/// Identifies the <see cref="RangeEndDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RangeEndDateProperty = DependencyPropertyUtilities.Register("RangeEndDate",
			typeof(DateTime?), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Now.Date.Add(TimeSpan.FromDays(60)).Date, new PropertyChangedCallback(OnRangeEndDateChanged)));

		private static void OnRangeEndDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			// JM 04-05-11 TFS71182
			if (false == dialog._bypassRangeEndDateNormalization && dialog.RangeEndDate.HasValue)
			{
				DateTime dt = dialog.RangeEndDate.Value;

				dialog._bypassRangeEndDateNormalization = true;
				dialog.RangeEndDate						= new DateTime(dt.Year, dt.Month, dt.Day, 23, 59, 59, dt.Kind);
				dialog._bypassRangeEndDateNormalization = false;
			}

			if (false == dialog._initializingView)
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

			if (dialog.IsInitializationComplete == true)
				dialog.UpdateDirtyStatus(true);
		}

		/// <summary>
		/// Returns or sets the end date of the recurrence.
		/// </summary>
		/// <seealso cref="RangeEndDateProperty"/>
		/// <seealso cref="RangeStartDate"/>
		public DateTime? RangeEndDate
		{
			get
			{
				return (DateTime?)this.GetValue(ActivityRecurrenceDialogCore.RangeEndDateProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.RangeEndDateProperty, value);
			}
		}

		#endregion //RangeEndDate

		#region RangeEndAfterOccurrenceNumber

		/// <summary>
		/// Identifies the <see cref="RangeEndAfterOccurrenceNumber"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RangeEndAfterOccurrenceNumberProperty = DependencyPropertyUtilities.Register("RangeEndAfterOccurrenceNumber",
			typeof(int), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata((int)10, new PropertyChangedCallback(OnRangeEndAfterOccurrenceNumberChanged)));

		private static void OnRangeEndAfterOccurrenceNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ActivityRecurrenceDialogCore dialog = d as ActivityRecurrenceDialogCore;

			if (false == dialog._initializingView)
				dialog.UpdateRecurrenceRangeFromViewModel(dialog.Recurrence);	// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified

			if (dialog.IsInitializationComplete == true)
			{
				dialog.UpdateDefaultRangeEndDateBasedOnPattern(dialog.CurrentPatternType);
				dialog.UpdateDirtyStatus(true);
			}
		}

		/// <summary>
		/// Returns or sets the number of occurrences after which the recurrence ends.
		/// </summary>
		/// <seealso cref="RangeEndAfterOccurrenceNumberProperty"/>
		/// <seealso cref="RangeStartDate"/>
		/// <seealso cref="RangeEndDate"/>
		public int RangeEndAfterOccurrenceNumber
		{
			get
			{
				return (int)this.GetValue(ActivityRecurrenceDialogCore.RangeEndAfterOccurrenceNumberProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.RangeEndAfterOccurrenceNumberProperty, value);
			}
		}

		#endregion //RangeEndAfterOccurrenceNumber

		#region RecurrenceDescription

		private static readonly DependencyPropertyKey RecurrenceDescriptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("RecurrenceDescription",
			typeof(string), typeof(ActivityRecurrenceDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="RecurrenceDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecurrenceDescriptionProperty = RecurrenceDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a string that describes Recurrence settings and rules.
		/// </summary>
		/// <seealso cref="RecurrenceDescriptionProperty"/>
		public string RecurrenceDescription
		{
			get
			{
				return (string)this.GetValue(ActivityRecurrenceDialogCore.RecurrenceDescriptionProperty);
			}
			internal set
			{
				this.SetValue(ActivityRecurrenceDialogCore.RecurrenceDescriptionPropertyKey, value);
			}
		}

		#endregion //RecurrenceDescription

		#region RecurrenceDescriptionVisibility
		/// <summary>
		/// Identifies the <see cref="RecurrenceDescriptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecurrenceDescriptionVisibilityProperty = DependencyPropertyUtilities.Register("RecurrenceDescriptionVisibility",
			typeof(Visibility), typeof(ActivityRecurrenceDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Visible)
			);

		/// <summary>
		/// Returns/sets whether a natural language description of the Recurrence's details should be visible.
		/// </summary>
		/// <seealso cref="RecurrenceDescriptionVisibilityProperty"/>
		/// <seealso cref="RecurrenceDescription"/>
		public Visibility RecurrenceDescriptionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityRecurrenceDialogCore.RecurrenceDescriptionVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityRecurrenceDialogCore.RecurrenceDescriptionVisibilityProperty, value);
			}
		}
		#endregion //RecurrenceDescriptionVisibility

		#endregion //Public Properties

		#region Internal Properties

		#region IsDirty
		internal bool IsDirty
		{
			get { return this._isDirty; }
		}
		#endregion //IsDirty

		#endregion //Internal Properties

		#region Private Properties

		#region CurrentPatternType
		private PatternType CurrentPatternType
		{
			get
			{
				if (this.IsDailyPattern)
					return PatternType.Daily;

				if (this.IsMonthlyPattern)
					return PatternType.Monthly;

				if (this.IsYearlyPattern)
					return PatternType.Yearly;

				return PatternType.Weekly;
			}
		}
		#endregion //CurrentPatternType

		#region DialogElementProxy
		private DialogElementProxy DialogElementProxy
		{
			get
			{
				if (this._dialogElementProxy == null)
					this._dialogElementProxy = new DialogElementProxy(this);

				return this._dialogElementProxy;
			}
		}
		#endregion //DialogElementProxy

		#region EndAsLocal
		private DateTime EndAsLocal
		{
			// JM 04-11-11 TFS70524.  Use the clone of the activity.
			//get { return this._activity.GetEndLocal(this._timeZoneTokenHelper.ActivityEndTimeZoneTokenResolved); }
			get { return this._activityClone.GetEndLocal(this._timeZoneTokenHelper.ActivityEndTimeZoneTokenResolved); }
		}
		#endregion //EndAsLocal

		#region IsInitializationComplete
		private bool IsInitializationComplete
		{
			get
			{

				return this._initialized;



			}
		}
		#endregion //IsInitializationComplete

		#region Recurrence
		private DateRecurrence Recurrence
		{
			// JM 04-11-11 TFS70524.  Work with a clone of the activity.
			//get { return this._activity.Recurrence as DateRecurrence; }
			//set { this._activity.Recurrence	= value; }
			get { return this._activityClone.Recurrence as DateRecurrence; }
			set { this._activityClone.Recurrence	= value; }
		}
		#endregion //Recurrence

		#region StartAsLocal
		private DateTime StartAsLocal
		{
			// JM 04-11-11 TFS70524.  Use the clone of the activity.
			//get { return this._activity.GetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved); }
			get { return this._activityClone.GetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved); }
		}
		#endregion //StartAsLocal

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region UpdateDirtyStatus
		/// <summary>
		/// Updates the dirty status of the dialog.
		/// </summary>
		/// <param name="isDirty">True to mark the dialog dirty, otherwise false.</param>
		public void UpdateDirtyStatus(bool isDirty)
		{
			this._isDirty = isDirty && this.IsActivityModifiable;

			if (this.Recurrence != null)
				this.RecurrenceDescription = this.Recurrence.ToString();

			// Force the CommandSourceManager to requery the status of our commands.
			this.UpdateCommandsStatus();
		}
		#endregion //UpdateDirtyStatus

		#endregion //Public Methods

		#region Internal Methods

		#region Close
		internal void Close()
		{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			this.SetDialogResult(false);

		}
		#endregion //Close

		#region RemoveRecurrence
		internal void RemoveRecurrence()
		{


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			this.Recurrence = null;
			this.UpdateDirtyStatus(true);
			this.CommitActivityHelper();
			this.SetDialogResult(true);

		}
		#endregion //RemoveRecurrence

		#region SaveAndClose
		internal void SaveAndClose()
		{
			// Force the time pickers to parse any typing that is in progress.
			this._timePickerManager.ForceTimePickersToUpdate();

			// Perform date range edit and then update the appointment start/end date
			// JM 04-11-11 TFS70524.  Reference the Activity property rather than the member variable.
			if (this.Activity.GetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved) >
				this.RangeEndDate)
			{
#pragma warning disable 436



				MessageBoxResult result = MessageBox.Show(SR.GetString("MSG_TEXT_UpdateRecurrenceInvalidPattern"), SR.GetString("MSG_TITLE_UpdateRecurrence"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

#pragma warning restore 436

				return;
			}
			// If we have a simple yearly pattern, make sure the day number is appropriate for the selected month.
			else if (this.IsYearlyPattern && this.IsYearlyPatternTypeSimple)
			{
				bool isBadDay = false;
				switch (this.MonthOfYearSimple + 1)
				{
					case 2:
						{
							if (this.DayOfMonthYearly > 29)
								isBadDay = true;
							break;
						}
					case 4:
					case 6:
					case 9:
					case 11:
						{
							if (this.DayOfMonthYearly > 30)
								isBadDay = true;
							break;
						}
				}

				if (isBadDay)
				{
#pragma warning disable 436



					MessageBoxResult result = MessageBox.Show(SR.GetString("MSG_TEXT_UpdateRecurrenceInvalidPattern"), SR.GetString("MSG_TITLE_UpdateRecurrence"), MessageBoxButton.OK, MessageBoxImage.Exclamation);

#pragma warning restore 436

					return;
				}
			}
			// JM 04-14-11 TFS61958 - Make sure that the duration of the activity is shorter than how frequently
			// the recurrence recurs.
			else if (this.DataManager.HasOverlappingOccurrences(this.Activity, this.Recurrence))
			{



				MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_UpdateRecurrenceDurationConflict"), ScheduleUtilities.GetString("MSG_TITLE_UpdateRecurrence"), MessageBoxButton.OK, MessageBoxImage.Exclamation);


				return;
			}
			else
			{
				// JM 11-2-10 TFS58915.  Convert the date to UTC before calling GetFirstOccurrenceStartTime
				//DateTime newStartDate			= this.RangeStartDate.Value.Date;
				DateTime newStartDate;
				if (this.Activity.IsTimeZoneNeutral)
					newStartDate = DateTime.SpecifyKind(this.RangeStartDate.Value.Date, DateTimeKind.Utc);
				else	
					newStartDate = this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved.ConvertToUtc(this.RangeStartDate.Value.Date);

				DateTime? firstOccurrenceDate = this.DataManager.GetFirstOccurrenceStartTime(this.Activity, newStartDate, this.Recurrence);
				if (firstOccurrenceDate.HasValue)
					newStartDate = firstOccurrenceDate.Value;

				DateTime newStartTime			= DateTime.Parse(this._timePickerManager.StartTimePickerProxy.Text);
				DateTime newCombinedStart		= ScheduleUtilities.CombineDateAndTime(newStartDate, newStartTime);

				// JM 04-11-11 TFS70524.  Reference the Activity property rather than the member variable.
				this.Activity.SetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved, newCombinedStart);
				if (this.DataManager.IsEndDateSupportedByActivity(this.Activity))
					this.Activity.SetEndLocal(this._timeZoneTokenHelper.ActivityEndTimeZoneTokenResolved, newCombinedStart.Add(this._timePickerManager.Duration));
			}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			this.CommitActivityHelper();
			this.SetDialogResult(true);

		}
		#endregion //SaveAndClose

		#endregion //Internal Methods

		#region Private Methods

		#region AddRemoveGotFocusHelper
		/// <summary>
		/// Unhooks the gotfocus of the old element, updates the field to reference the new value and hooks it if non-null.
		/// </summary>
		/// <param name="field">Field that will be updated with the new value. If the old value is non-null the event will be unhooked first</param>
		/// <param name="newValue">The new value for the field</param>
		/// <param name="handler">The handler to hook/unhook</param>
		private void AddRemoveGotFocusHelper( ref FrameworkElement field, FrameworkElement newValue, RoutedEventHandler handler )
		{
			if ( field != null )
				field.GotFocus -= handler;

			field = newValue;

			if ( field != null )
				field.GotFocus += handler;
		}
		#endregion // AddRemoveGotFocusHelper

		// JM 04-05-11 TFS71183 Added.
		#region AddRemoveKeyDownHelper
		/// <summary>
		/// Unhooks the KeyDownof the old element, updates the field to reference the new value and hooks it if non-null.
		/// </summary>
		/// <param name="field">Field that will be updated with the new value. If the old value is non-null the event will be unhooked first</param>
		/// <param name="newValue">The new value for the field</param>
		/// <param name="handler">The handler to hook/unhook</param>
		private void AddRemoveKeyDownHelper(ref FrameworkElement field, FrameworkElement newValue, KeyEventHandler handler)
		{
			if (field != null)
				field.KeyDown -= handler;

			field = newValue;

			if (field != null)
				field.KeyDown += handler;
		}
		#endregion // AddRemoveKeyDownHelper

		#region CommitActivityHelper
		private bool CommitActivityHelper()
		{
			if (this.IsDirty)
			{
				// JM 04-11-11 TFS70524.  Update the recurrence and start/end dates on the original activity from our local cloned copy.
				this._activityOriginal.Recurrence	= this._activityClone.Recurrence;
				this._activityOriginal.Start		= this._activityClone.Start;
				this._activityOriginal.End			= this._activityClone.End;

				// Commit the appointment.
				if (this._shouldCallBeginAndEndEdit)
				{
					ActivityOperationResult result = this._dataManager.EndEdit(this._activityOriginal);
#pragma warning disable 436
					bool success = this.HandleOperationResult(result, SR.GetString("MSG_TITLE_UpdateActivity"));
#pragma warning restore 436

					return success;
				}
				else
					this.UpdateDirtyStatus(false);
			}

			return true;
		}
		#endregion //CommitActivityHelper

		#region EnsureDayOfWeek
		// Ensures that the specified date falls on the specified DayOfWeek.  Adjusts the date to the nearest future date
		// that falls on the specified DayOfWeek.
		private DateTime EnsureDayOfWeek(DayOfWeek dayOfWeek, DateTime date)
		{
			DateTime tempDate = date;

			while (tempDate.DayOfWeek != dayOfWeek)
			{
				tempDate = tempDate.AddDays(1);
			}

			return tempDate;
		}
		#endregion //EnsureDayOfWeek

		#region GetDayDescriptionIndexFromDayOfWeek
		private int GetDayDescriptionIndexFromDayOfWeek(DayOfWeek dayOfWeek)
		{
			return (int)dayOfWeek + 3;
		}
		#endregion //GetDayDescriptionIndexFromDayOfWeek

		#region GetDaysOfWeekFromDayDescription
		private DayOfWeek[] GetDaysOfWeekFromDayDescription(string dayDescription)
		{
			if (dayDescription == this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_Day"])
				return new DayOfWeek [] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday };

			if (dayDescription == this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_WeekDay"])
				return new DayOfWeek[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };

			if (dayDescription == this.LocalizedStrings["DLG_Recurrence_MonthlyPattern_Literal_WeekendDay"])
				return new DayOfWeek[] { DayOfWeek.Saturday, DayOfWeek.Sunday };

			DateTimeFormatInfo dtfi = this._dataManager.DateInfoProviderResolved.DateTimeFormatInfo;
			for (int i = 0; i < 7; i++)
			{
				if (dayDescription == dtfi.DayNames[i])
					return new DayOfWeek[] { (DayOfWeek)i };
			}

			Debug.Assert(false, "Bad day description");
			return null;
		}
		#endregion //GetDaysOfWeekFromDayDescription

		#region GetDayOfWeekOrdinalInMonthForDate
		// Gets the ordinal position of the specified date's DayOfWeek within the month, zero based (e.g.
		// if the specified date falls on a tuesday this routine returns whether it is the first tuesday,
		// the second tuesday etc.)
		private int GetDayOfWeekOrdinalInMonthForDate(DateTime date)
		{
			DateTime	tempDate			= new DateTime(date.Year, date.Month, 1);
			int			dayOfWeekOrdinal	= 0;
			int			dayIncrement		= 1;

			while (tempDate.Month == date.Month)
			{
				if (tempDate.DayOfWeek == date.DayOfWeek)
				{
					dayIncrement = 7;
					if (tempDate.Date == date.Date)
						break;
					else
						dayOfWeekOrdinal++;
				}

				tempDate = tempDate.AddDays(dayIncrement);
			}

			return dayOfWeekOrdinal;
		}
		#endregion //GetDayOfWeekOrdinalInMonthForDate

		#region GetDefaultDailyRecurrence
		private DateRecurrence GetDefaultDailyRecurrence()
		{
			DateRecurrence defaultDailyRecurrence	= new DateRecurrence();
			defaultDailyRecurrence.Interval			= 1;
			defaultDailyRecurrence.Frequency		= DateRecurrenceFrequency.Daily;
			defaultDailyRecurrence.Rules.Clear();

			return defaultDailyRecurrence;
		}
		#endregion //GetDefaultDailyRecurrence

		#region GetDefaultWeeklyRecurrence
		private DateRecurrence GetDefaultWeeklyRecurrence()
		{
			DateRecurrence defaultWeeklyRecurrence	= new DateRecurrence();
			defaultWeeklyRecurrence.Interval		= 1;
			defaultWeeklyRecurrence.Frequency		= DateRecurrenceFrequency.Weekly;
			defaultWeeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(this.StartAsLocal.DayOfWeek));

			return defaultWeeklyRecurrence;
		}
		#endregion //GetDefaultWeeklyRecurrence

		#region GetDefaultMonthlyRecurrence
		private DateRecurrence GetDefaultMonthlyRecurrence()
		{
			DateRecurrence defaultMonthlyRecurrence	= new DateRecurrence();
			defaultMonthlyRecurrence.Interval		= 1;
			defaultMonthlyRecurrence.Frequency		= DateRecurrenceFrequency.Monthly;
			defaultMonthlyRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(this.StartAsLocal.Day));

			return defaultMonthlyRecurrence;
		}
		#endregion //GetDefaultMonthlyRecurrence

		#region GetDefaultYearlyRecurrence
		private DateRecurrence GetDefaultYearlyRecurrence()
		{
			DateRecurrence defaultYearlyRecurrence	= new DateRecurrence();
			defaultYearlyRecurrence.Interval		= 1;
			defaultYearlyRecurrence.Frequency		= DateRecurrenceFrequency.Yearly;
			defaultYearlyRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(this.StartAsLocal.Day));
			defaultYearlyRecurrence.Rules.Add(new MonthOfYearRecurrenceRule(this.StartAsLocal.Month));

			return defaultYearlyRecurrence;
		}
		#endregion //GetDefaultYearlyRecurrence

		#region GetMonthlyRulesByType
		private void GetMonthlyRulesByType(DateRecurrence recurrence, out List<DayOfWeekRecurrenceRule> weekdayRules, 
																	  out List<DayOfWeekRecurrenceRule> weekendDayRules,
																	  out List<SubsetRecurrenceRule> subsetRules,
																	  out List<DateRecurrenceRuleBase> otherRules)
		{
			weekdayRules	= new List<DayOfWeekRecurrenceRule>();
			weekendDayRules = new List<DayOfWeekRecurrenceRule>();
			subsetRules		= new List<SubsetRecurrenceRule>();
			otherRules		= new List<DateRecurrenceRuleBase>();

			foreach (DateRecurrenceRuleBase rule in recurrence.Rules)
			{
				DayOfWeekRecurrenceRule dowRule		= rule as DayOfWeekRecurrenceRule;
				SubsetRecurrenceRule	subsetRule	= rule as SubsetRecurrenceRule;
				if (dowRule != null)
				{
					if (dowRule.Day == DayOfWeek.Saturday || dowRule.Day == DayOfWeek.Sunday)
						weekendDayRules.Add(dowRule);
					else
						weekdayRules.Add(dowRule);
				}
				else if (subsetRule != null)
					subsetRules.Add(subsetRule);
				else
					otherRules.Add(rule);
			}
		}
		#endregion //GetMonthlyRulesByType

		#region GetYearlyRulesByType
		private void GetYearlyRulesByType(DateRecurrence recurrence, out List<DayOfWeekRecurrenceRule> weekdayRules, 
																	 out List<DayOfWeekRecurrenceRule> weekendDayRules,
																	 out List<DayOfMonthRecurrenceRule> dayOfMonthRules,
																	 out List<MonthOfYearRecurrenceRule> monthOfYearRules,
																	 out List<SubsetRecurrenceRule> subsetRules,
																	 out List<DateRecurrenceRuleBase> otherRules)
		{
			ScheduleUtilities.GetRulesByType(recurrence, 
				out weekdayRules, out weekendDayRules, out dayOfMonthRules, out monthOfYearRules, out subsetRules, out otherRules);
		}
		#endregion //GetYearlyRulesByType

		#region HandleOperationResult
		private bool HandleOperationResult(OperationResult result, string errorMessageBoxTitle)
		{
			Debug.Assert(result.Error == null, "OperationResult error!");

			DataErrorInfo errorInfo = result.Error;
			bool success = (null == errorInfo);
			if (success)
			{
				this.UpdateDirtyStatus(false);
				if (false == result.IsComplete)
				{
					// Handoff the pending operation result to the DataManager.
					this._dataManager.AddPendingOperation(result);
				}
			}
			else if (errorInfo != null)	// Display a message on error.
			{
				_dataManager.DisplayErrorMessageBox(errorInfo, errorMessageBoxTitle);
			}

			return success;
		}
		#endregion //HandleOperationResult

		#region Initialize
		private void Initialize()
		{
			if (true == this.IsInitializationComplete)
			{
				if (this._timePickerManager != null)
				{
					this._timePickerManager.DisconnectEventListeners();
					this._timePickerManager = null;
				}

				// JM 04-11-11 TFS70524.  Reference the Activity property rather than the member variable.
				this.Activity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);
			}

			// Hide the end time picker if the Activity doesn't support end times,
			if (false == this.DataManager.IsEndDateSupportedByActivity(this.Activity))
				this.EndTimePickerVisibility = Visibility.Collapsed;

			// Setup the TimePickerManager which takes care of managing the interaction between the time pickers.
			this._timePickerManager = new TimePickerManager(this,
															this._dataManager,
															this._activityClone,	// JM 04-12-11 TFS70524 - Use a clone of the activity
															this.GetTemplateChild(PartStartTimePicker) as FrameworkElement,
															this.GetTemplateChild(PartEndTimePicker) as FrameworkElement,
															this.GetTemplateChild(PartDurationPicker) as FrameworkElement);

			this._timePickerManager.StartTimePickerProxy.ComboBoxControl.Focus();

			// Force the CommandSourceManager to requery the status of our commands.
			this.UpdateCommandsStatus();

			// Listen to the Activity's PropertyChanged event.
			// JM 04-11-11 TFS70524.  Reference the Activity property rather than the member variable.
			this.Activity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);

			// Find all the Parts in the Template and initialize them.
			this.InitializeParts();

			// Initialize the recurrence pattern.  If there is no Recurrence, default to a weekly recurrence pattern.
			if (null == this.Recurrence)
			{
				this.Recurrence			= this.GetDefaultWeeklyRecurrence();
				this.IsWeeklyPattern	= true;
			}
			else
			{
				switch (this.Recurrence.Frequency)
				{
					case DateRecurrenceFrequency.Daily:
						this.IsDailyPattern = true;
						break;
					case DateRecurrenceFrequency.Weekly:
						this.IsWeeklyPattern = true;
						break;
					case DateRecurrenceFrequency.Monthly:
						this.IsMonthlyPattern = true;
						break;
					case DateRecurrenceFrequency.Yearly:
						this.IsYearlyPattern = true;
						break;
				}
			}

			// Initialize the recurrrence range view fields.
			this.UpdateViewModelFromRecurrenceRange();

			// Initialize the Recurrence Description
			this.RecurrenceDescription	= this.Recurrence.ToString();

			this._initialized			= true;
		}
		#endregion //Initialize

		#region InitializeParts
		private void InitializeParts()
		{
			// Daily Recurrence Pattern Section
			//
			// Frequency textBox
			AddRemoveGotFocusHelper(ref _tbDailyFrequency, this.GetTemplateChild(PartDailyFrequency) as FrameworkElement, _tbDailyFrequency_GotFocus);

			// Monthly Recurrence Pattern Section
			//
			// DayNumber textBox
			AddRemoveGotFocusHelper(ref _tbMonthlyDayNumber, this.GetTemplateChild(PartMonthlyDayNumber) as FrameworkElement, _tbMonthlyDayNumber_GotFocus);

			// Frequency textBox
			AddRemoveGotFocusHelper(ref _tbMonthlyFrequency, this.GetTemplateChild(PartMonthlyFrequency) as FrameworkElement, _tbMonthlyFrequency_GotFocus);

			// Frequency textBox complex
			AddRemoveGotFocusHelper(ref _tbMonthlyFrequencyComplex, this.GetTemplateChild(PartMonthlyFrequencyComplex) as FrameworkElement, _tbMonthlyFrequencyComplex_GotFocus);

			// Moveable monthly pattern elements - To support localization that changes the order of the elements in the
			// complex monthly pattern section, find the moveable elements and arrange them in the order
			// specified in the localizable string 'DLG_Recurrence_MonthlyPattern_Complex_ElementOrder'.
			FrameworkElement [] fe	= new FrameworkElement[6];
			fe[0]					= this.GetTemplateChild(PartMonthlyMoveableElement1) as FrameworkElement;
			fe[1]					= this.GetTemplateChild(PartMonthlyMoveableElement2) as FrameworkElement;
			fe[2]					= this.GetTemplateChild(PartMonthlyMoveableElement3) as FrameworkElement;
			fe[3]					= this.GetTemplateChild(PartMonthlyMoveableElement4) as FrameworkElement;
			fe[4]					= this._tbMonthlyFrequencyComplex;
			fe[5]					= this.GetTemplateChild(PartMonthlyMoveableElement6) as FrameworkElement;
#pragma warning disable 436
			string order			= SR.GetString("DLG_Recurrence_MonthlyPattern_Complex_ElementOrder");
#pragma warning restore 436
			this.LocalizeElementPositions(fe, order);


			// Yearly Recurrence Pattern Section
			//
			// DayNumber textBox
			AddRemoveGotFocusHelper(ref _tbYearlyDayNumber, this.GetTemplateChild(PartYearlyDayNumber) as FrameworkElement, _tbYearlyDayNumber_GotFocus);

			// Recurrence Range Section
			//
			// RangeEndAfterOcurrenceNumber textbox
			AddRemoveGotFocusHelper(ref _tbRangeEndAfterOccurrenceNumber, this.GetTemplateChild(PartRangeEndAfterOccurrenceNumber) as FrameworkElement, _tbRangeEndAfterOccurrenceNumber_GotFocus);

			// JM 04-05-11 TFS71183
			// RangeEndAfterDate DatePicker
			AddRemoveGotFocusHelper(ref _dpRangeEndDatePicker, this.GetTemplateChild(PartRangeEndAfterDate) as FrameworkElement, _dpRangeEndAfterDate_GotFocus);
			AddRemoveKeyDownHelper(ref _dpRangeEndDatePicker, this.GetTemplateChild(PartRangeEndAfterDate) as FrameworkElement, _dpRangeEndAfterDate_KeyDown);

			// Moveable yearly pattern Simple elements - To support localization that changes the order of the elements in the
			// complex yearly pattern section, find the moveable elements and arrange them in the order
			// specified in the localizable string 'DLG_Recurrence_YearlyPattern_Complex_ElementOrder'.
			fe = new FrameworkElement[3];
			fe[0] = this.GetTemplateChild(PartYearlyMoveableElement7) as FrameworkElement;
			fe[1] = this._tbYearlyDayNumber;
			fe[2] = this.GetTemplateChild(PartYearlyMoveableElement8) as FrameworkElement;
#pragma warning disable 436
			order = SR.GetString("DLG_Recurrence_YearlyPattern_Simple_ElementOrder");
#pragma warning restore 436
			this.LocalizeElementPositions(fe, order);

			// Moveable yearly pattern Complex elements - To support localization that changes the order of the elements in the
			// complex yearly pattern section, find the moveable elements and arrange them in the order
			// specified in the localizable string 'DLG_Recurrence_YearlyPattern_Complex_ElementOrder'.
			fe		= new FrameworkElement[6];
			fe[0]	= this.GetTemplateChild(PartYearlyMoveableElement1) as FrameworkElement;
			fe[1]	= this.GetTemplateChild(PartYearlyMoveableElement2) as FrameworkElement;
			fe[2]	= this.GetTemplateChild(PartYearlyMoveableElement3) as FrameworkElement;
			fe[3]	= this.GetTemplateChild(PartYearlyMoveableElement4) as FrameworkElement;
			fe[4]	= this.GetTemplateChild(PartYearlyMoveableElement5) as FrameworkElement;
			fe[5]	= this.GetTemplateChild(PartYearlyMoveableElement6) as FrameworkElement;
#pragma warning disable 436
			order	= SR.GetString("DLG_Recurrence_YearlyPattern_Complex_ElementOrder");
#pragma warning restore 436
			this.LocalizeElementPositions(fe, order);
		}

		#endregion //InitializeParts

		#region LocalizeElementPositions
		private void LocalizeElementPositions(FrameworkElement[] elements, string positions)
		{
			string[] elementPositions = positions.Split(',');

			// JM 04-07-11 TFS71603.  Exit if inconsistent parameters have been specified.
			if (elements.Length != elementPositions.Length)
				return;

			for (int i = 0; i < elements.Length; i++)
			{
				int		elementNumber;
				bool	ok = int.TryParse(elementPositions[i], out elementNumber);

				// JM 04-07-11 TFS71603.  Move up the check for 'ok == true'.
				if (ok)
				{
					FrameworkElement fe = elements[elementNumber - 1];
					//if (ok && null != fe)
					if (null != fe)
						fe.SetValue(Grid.ColumnProperty, i + 1);
				}
			}
		}
		#endregion //LocalizeElementPositions

		#region OnActivityPropertyChanged
		private void OnActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			this.UpdateDirtyStatus(true);
		}
		#endregion //OnActivityPropertyChanged

		// JM 04-05-11 TFS68431 Added.
		#region ResetDailyPatternTypeSimpleViewProperties
		private void ResetDailyPatternTypeSimpleViewProperties()
		{
			this.IntervalDaily = 1;
		}
		#endregion //ResetDailyPatternTypeSimpleViewProperties

		#region ResetMonthlyPatternTypeComplexViewProperties
		private void ResetMonthlyPatternTypeComplexViewProperties()
		{
			this._resettingMonthlyPatternTypeComplexViewProperties = true;

			try
			{
				this.DayOfWeekOrdinalMonthly	= this.GetDayOfWeekOrdinalInMonthForDate(this.StartAsLocal);
				this.DayOfWeekMonthly			= (int)this.StartAsLocal.DayOfWeek + 3;
				this.IntervalMonthlyComplex		= 1;
			}
			finally { this._resettingMonthlyPatternTypeComplexViewProperties = false; }
		}
		#endregion //ResetMonthlyPatternTypeComplexViewProperties

		#region ResetMonthlyPatternTypeSimpleViewProperties
		private void ResetMonthlyPatternTypeSimpleViewProperties()
		{
			this.DayNumberMonthly	= (int)this.StartAsLocal.Day;
			this.IntervalMonthly	= 1;
		}
		#endregion //ResetMonthlyPatternTypeSimpleViewProperties

		#region ResetYearlyPatternTypeComplexViewProperties
		private void ResetYearlyPatternTypeComplexViewProperties()
		{
			this._resettingYearlyPatternTypeComplexViewProperties = true;

			try
			{
				this.DayOfWeekOrdinalYearly		= this.GetDayOfWeekOrdinalInMonthForDate(this.StartAsLocal); ;
				this.DayOfWeekYearly			= (int)this.StartAsLocal.DayOfWeek + 3;
				this.MonthOfYearComplex			= this.StartAsLocal.Month - 1;
			}
			finally { this._resettingYearlyPatternTypeComplexViewProperties = false; }
		}
		#endregion //ResetYearlyPatternTypeComplexViewProperties

		#region ResetYearlyPatternTypeSimpleViewProperties
		private void ResetYearlyPatternTypeSimpleViewProperties()
		{
			this._resettingYearlyPatternTypeSimpleViewProperties = true;

			try
			{
				this.DayOfMonthYearly	= (int)this.StartAsLocal.Day;
				this.MonthOfYearSimple	= this.StartAsLocal.Month - 1;
			}
			finally { this._resettingYearlyPatternTypeSimpleViewProperties = false; }
		}
		#endregion //ResetYearlyPatternTypeSimpleViewProperties

		#region SetDialogResult

		private void SetDialogResult(bool result)
		{
			if ( this.DialogElementProxy != null )
				this.DialogElementProxy.SetDialogResult(result);
		}

		#endregion //SetDialogResult

		#region UpdateDefaultRangeEndDateBasedOnPattern
		private void UpdateDefaultRangeEndDateBasedOnPattern(PatternType patternType)
		{
			DateTime tempDate;
			switch (patternType)
			{
				case PatternType.Daily:
					this.RangeEndDate = this.StartAsLocal.Date.Add(TimeSpan.FromDays(Math.Max(0, this.RangeEndAfterOccurrenceNumber - 1))).Date;
					break;
				case PatternType.Weekly:
					tempDate			= this.StartAsLocal.Date.Add(TimeSpan.FromDays(7 * Math.Max(0, this.RangeEndAfterOccurrenceNumber - 1))).Date;
					this.RangeEndDate	= this.EnsureDayOfWeek(this.StartAsLocal.DayOfWeek, tempDate);
					break;
				case PatternType.Monthly:
					tempDate			= this.StartAsLocal.Date.Add(TimeSpan.FromDays(30 * Math.Max(0, this.RangeEndAfterOccurrenceNumber - 1))).Date;
					this.RangeEndDate	= this.EnsureDayOfWeek(this.StartAsLocal.DayOfWeek, tempDate);
					break;
				case PatternType.Yearly:
					this.RangeEndDate = this.StartAsLocal.Date.Add(TimeSpan.FromDays(365 * Math.Max(0, this.RangeEndAfterOccurrenceNumber - 1))).Date;
					break;
			}
		}
		#endregion //UpdateDefaultRangeEndDateBasedOnPattern

		#region UpdateCommandsStatus
		private void UpdateCommandsStatus()
		{
			// Force the CommandSourceManager to requery the CanExecute status of certain commands.
			CommandSourceManager.NotifyCanExecuteChanged(typeof(RecurrenceDialogCoreSaveAndCloseCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(RecurrenceDialogCoreCloseCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(RecurrenceDialogCoreRemoveRecurrenceCommand));
		}
		#endregion //UpdateCommandsStatus

		#region UpdateViewModelFromDailyRecurrence
		private void UpdateViewModelFromDailyRecurrence()
		{
			this._initializingView = true;

			try
			{
				if (this.Recurrence == null || this.Recurrence.Frequency != DateRecurrenceFrequency.Daily)
				{
					Debug.Assert(false, "Recurrence is invalid!");
					return;
				}

				this.IntervalDaily = this.Recurrence.Interval;
				if (this.Recurrence.Rules.Count > 0 && this.Recurrence.Rules[0] is DayOfWeekRecurrenceRule)
				{
					this.IsDailyPatternTypeWeekday = true;

					// JM 04-05-11 TFS68431
					this.ResetDailyPatternTypeSimpleViewProperties();
				}
				else
					this.IsDailyPatternTypeEvery = true;
			}
			finally { this._initializingView = false; }
		}
		#endregion //UpdateViewModelFromDailyRecurrence

		#region UpdateViewModelFromWeeklyRecurrence
		private void UpdateViewModelFromWeeklyRecurrence()
		{
			this._initializingView = true;

			try
			{
				if (this.Recurrence == null || this.Recurrence.Frequency != DateRecurrenceFrequency.Weekly)
				{
					Debug.Assert(false, "Recurrence is invalid!");
					return;
				}

				this.IntervalWeekly = this.Recurrence.Interval;

				// Reset the IsWeeklyOnxxxx View properties, then set as needed based on the specified DayOfWeekRecurrenceRules.
				this.IsWeeklyOnSunday	= false;
				this.IsWeeklyOnMonday	= false;
				this.IsWeeklyOnTuesday	= false;
				this.IsWeeklyOnWednesday= false;
				this.IsWeeklyOnThursday	= false;
				this.IsWeeklyOnFriday	= false;
				this.IsWeeklyOnSaturday	= false;

				foreach (DateRecurrenceRuleBase rule in this.Recurrence.Rules)
				{
					DayOfWeekRecurrenceRule dowRule = rule as DayOfWeekRecurrenceRule;
					if (dowRule != null)
					{
						if (dowRule.Day == DayOfWeek.Sunday)
							this.IsWeeklyOnSunday = true;
						else if (dowRule.Day == DayOfWeek.Monday)
							this.IsWeeklyOnMonday = true;
						else if (dowRule.Day == DayOfWeek.Tuesday)
							this.IsWeeklyOnTuesday = true;
						else if (dowRule.Day == DayOfWeek.Wednesday)
							this.IsWeeklyOnWednesday = true;
						else if (dowRule.Day == DayOfWeek.Thursday)
							this.IsWeeklyOnThursday = true;
						else if (dowRule.Day == DayOfWeek.Friday)
							this.IsWeeklyOnFriday = true;
						else if (dowRule.Day == DayOfWeek.Saturday)
							this.IsWeeklyOnSaturday = true;
					}
				}
			}
			finally { this._initializingView = false; }
		}
		#endregion //UpdateViewModelFromWeeklyRecurrence

		#region UpdateViewModelFromMonthlyRecurrence
		private void UpdateViewModelFromMonthlyRecurrence()
		{
			this._initializingView = true;

			try
			{
				if (this.Recurrence == null || this.Recurrence.Frequency != DateRecurrenceFrequency.Monthly)
				{
					Debug.Assert(false, "Recurrence is invalid!");
					return;
				}

				DateRecurrence recurrence = this.Recurrence;

				// If we have a single rule and it is a DayOfMonth recurrence rule, then we have a simple pattern.
				// Otherwise we have a complex pattern.
				if (recurrence.Rules.Count == 1 && recurrence.Rules[0] is DayOfMonthRecurrenceRule)
				{
					this.IsMonthlyPatternTypeSimple		= true;
					this.IsMonthlyPatternTypeComplex	= false;
					this.IntervalMonthly				= recurrence.Interval;
					this.DayNumberMonthly				= ((DayOfMonthRecurrenceRule)recurrence.Rules[0]).DayOfMonth;

					// Reset the complex pattern view properties.
					this.ResetMonthlyPatternTypeComplexViewProperties();
				}
				else 
				{
					// Determine the type of complex pattern we have.  If the recurrence contains a set of rules that we do not
					// support in the dialog, force the recurrence to use the defaylt monthly recurrence.
					List<DayOfWeekRecurrenceRule> weekdayRules;
					List<DayOfWeekRecurrenceRule> weekendDayRules;
					List<SubsetRecurrenceRule> subsetRules;
					List<DateRecurrenceRuleBase> otherRules;

					this.GetMonthlyRulesByType(recurrence, out weekdayRules, out weekendDayRules, out subsetRules, out otherRules);
					bool hasWeekendDaysOnly	= weekendDayRules.Count == 2 && weekdayRules.Count == 0;
					bool hasWeekdaysOnly	= weekendDayRules.Count == 0 && weekdayRules.Count == 5;

					// Identify the situations where we should use the default monthly recurrence.
					bool useDefaultMonthlyRecurrence	= false;
					// Should not have any 'other' rules
					if (otherRules.Count > 0)  
						useDefaultMonthlyRecurrence = true;
					else 
					// Must have at least 1 rule
					if (weekendDayRules.Count == 0 && weekdayRules.Count == 0)  
 						useDefaultMonthlyRecurrence = true;
					// If we do not have weekends or weekdays then the number of rules must be either 1 (for a single day)
					// or 7 for 'all days'.
					else if (hasWeekdaysOnly == false && hasWeekendDaysOnly == false)
					{
						if (weekdayRules.Count + weekendDayRules.Count != 1 &
							weekdayRules.Count + weekendDayRules.Count != 7)
							useDefaultMonthlyRecurrence = true;
					}
					else // Make sure we have a subset rule if we have weekend or weekday rules and the 
					// relative position is last (i.e., -1) 
					{
						int relativePosition = weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition;
						if (relativePosition == -1 && (hasWeekendDaysOnly || hasWeekdaysOnly) && subsetRules.Count == 0)
							useDefaultMonthlyRecurrence = true;
					}

					// If we have a good set of rules, evaluate the rules and update the View proeprties.
					if (false == useDefaultMonthlyRecurrence)
					{
						this.IntervalMonthlyComplex			= recurrence.Interval;
						this.IsMonthlyPatternTypeSimple		= false;
						this.IsMonthlyPatternTypeComplex	= true;

						// SSP 4/27/11 TFS66178/TFS73425
						// See the changes made in the UpdateMonthlyRecurrenceFromViewModel method.
						// 
						//int relativePosition				= weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition;
						int relativePosition = 1 == subsetRules.Count ? subsetRules[0].OccurrenceInstance 
							: ( weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition );

						// Set the DayOfWeekOrdinalMonthly based on the relative position of the DayOfWeek rules.
						if (relativePosition == -1)
						{
							// MD 2/17/11 - TFS66136
							// This is incorrect. -1 indicates the "Last" value, not the default.
							//this.DayOfWeekOrdinalMonthly = this.GetDayOfWeekOrdinalInMonthForDate(this.StartAsLocal);
							this.DayOfWeekOrdinalMonthly = ActivityRecurrenceDialogCore.DayOfWeekOrdinalValueLast;
						}
						else
							this.DayOfWeekOrdinalMonthly = relativePosition - 1;

						// Set the DayOfWeekMonthly value
						if (hasWeekdaysOnly)
							this.DayOfWeekMonthly			= 1;
						else
						if (hasWeekendDaysOnly)
							this.DayOfWeekMonthly			= 2;
						else if (weekdayRules.Count + weekendDayRules.Count > 1)
							this.DayOfWeekMonthly = 0;
						else
						{
							// Determine which day of the week has been set.
							if (weekdayRules.Count > 0)
								this.DayOfWeekMonthly = this.GetDayDescriptionIndexFromDayOfWeek(weekdayRules[0].Day);
							else
								this.DayOfWeekMonthly = this.GetDayDescriptionIndexFromDayOfWeek(weekendDayRules[0].Day);
						}
					}

					// If we found rules we cannot handle, ignore the rules in the recurrence and default to
					// a simple rule.
					if (useDefaultMonthlyRecurrence)
					{
						this.Recurrence						= this.GetDefaultMonthlyRecurrence();
						this.IntervalMonthly				= this.Recurrence.Interval;
						this.IsMonthlyPatternTypeSimple		= true;
						this.IsMonthlyPatternTypeComplex	= false;
						this.DayNumberMonthly				= ((DayOfMonthRecurrenceRule)this.Recurrence.Rules[0]).DayOfMonth;
					}

					// Reset the simple pattern view properties.
					this.ResetMonthlyPatternTypeSimpleViewProperties();
				}
			}
			finally	{ this._initializingView = false; }
		}
		#endregion //UpdateViewModelFromMonthlyRecurrence

		#region UpdateViewModelFromRecurrenceRange
		private void UpdateViewModelFromRecurrenceRange()
		{
			this._initializingView = true;

			try
			{
				this.RangeStartDate = this.Activity.GetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved);

				// Forever
				if (this.Recurrence.Until == null && this.Recurrence.Count == 0)
				{
					this.IsRangeForever					= true;
					this.RangeEndAfterOccurrenceNumber	= 10;
					this.UpdateDefaultRangeEndDateBasedOnPattern(this.CurrentPatternType);
				}
				// End after 'n' occurrences
				else if (this.Recurrence.Count > 0 && this.Recurrence.Until == null)
				{
					this.IsRangeEndAfter				= true;
					this.RangeEndAfterOccurrenceNumber	= this.Recurrence.Count;
					this.UpdateDefaultRangeEndDateBasedOnPattern(this.CurrentPatternType);
				}
				// End by specific date
				else if (this.Recurrence.Until != null && this.Recurrence.Count == 0)
				{
					this.IsRangeEndBy					= true;
					this.RangeEndAfterOccurrenceNumber	= 10;
					this.RangeEndDate					= this.Recurrence.Until;
				}
				else // Bad Recurrence settings - reset to defaults
				{
					// Set view properties to default values.
					this.IsRangeForever					= true;
					this.RangeEndAfterOccurrenceNumber	= 10;
					this.UpdateDefaultRangeEndDateBasedOnPattern(this.CurrentPatternType);

					// Reset the Recurrence fields to their defaults
					this.Recurrence.Count = 0;
					this.Recurrence.Until = null;
				}
			}
			finally { this._initializingView = false; }
		}
		#endregion //UpdateViewModelFromRecurrenceRange

		#region UpdateViewModelFromYearlyRecurrence
		private void UpdateViewModelFromYearlyRecurrence()
		{
			this._initializingView = true;

			try
			{
				if (this.Recurrence == null || this.Recurrence.Frequency != DateRecurrenceFrequency.Yearly)
				{
					Debug.Assert(false, "Recurrence is invalid!");
					return;
				}

				DateRecurrence recurrence	= this.Recurrence;

				this.IntervalYearly			= recurrence.Interval;

				// Organize the rules in the recurrence by type so that we can evaluate what type of complex pattern we have.
				List<DayOfWeekRecurrenceRule>	weekdayRules;
				List<DayOfWeekRecurrenceRule>	weekendDayRules;
				List<DayOfMonthRecurrenceRule>	dayOfMonthDayRules;
				List<MonthOfYearRecurrenceRule>	monthOfYearRules;
				List<SubsetRecurrenceRule>		subsetRules;
				List<DateRecurrenceRuleBase>	otherRules;

				this.GetYearlyRulesByType(recurrence, out weekdayRules, out weekendDayRules, out dayOfMonthDayRules, out monthOfYearRules, out subsetRules, out otherRules);
				bool hasWeekendDaysOnly	= weekendDayRules.Count == 2 && weekdayRules.Count == 0;
				bool hasWeekdaysOnly	= weekendDayRules.Count == 0 && weekdayRules.Count == 5;

				// Determine the type of complex pattern we have.  If the recurrence contains a set of rules that we do not
				// support in the dialog, force the recurrence to use the defaylt monthly recurrence.
				//
				// If we have a DayOfMonth and MonthOfyear recurrence rule, then we have a simple pattern.
				// Otherwise we have a complex pattern.
				if (recurrence.Rules.Count == 2 && dayOfMonthDayRules.Count == 1 && monthOfYearRules.Count == 1)
				{
					this.IsYearlyPatternTypeSimple	= true;
					this.IsYearlyPatternTypeComplex = false;
					this.MonthOfYearSimple			= monthOfYearRules[0].Month - 1;
					this.DayOfMonthYearly			= dayOfMonthDayRules[0].DayOfMonth;

					// Reset the complex pattern view properties.
					this.ResetYearlyPatternTypeComplexViewProperties();
				}
				else
				{
					// Identify the situations where we should use the default monthly recurrence.
					bool useDefaultYearlyRecurrence = false;

					// Should not have any 'other' rules
					if (otherRules.Count > 0)
						useDefaultYearlyRecurrence = true;
					else
						// Must have at least 1 rule
						if (weekendDayRules.Count == 0 && weekdayRules.Count == 0 && monthOfYearRules.Count == 0)
							useDefaultYearlyRecurrence = true;
						// If we do not have weekends or weekdays then the number of rules must be either 1 (for a single day)
						// or 7 for 'all days'.
						else if (hasWeekdaysOnly == false && hasWeekendDaysOnly == false)
						{
							if (weekdayRules.Count + weekendDayRules.Count != 1 &
								weekdayRules.Count + weekendDayRules.Count != 7)
								useDefaultYearlyRecurrence = true;
						}
						// Must have 1 month of year rule
						else if (monthOfYearRules.Count != 1)
							useDefaultYearlyRecurrence = true;
						else // Make sure we have a subset rule if we have weekend or weekday rules and the 
						// relative position is last (i.e., -1) 
						{
							int relativePosition = weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition;
							if (relativePosition == -1 && (hasWeekendDaysOnly || hasWeekdaysOnly) && subsetRules.Count == 0)
								useDefaultYearlyRecurrence = true;
						}

					// If we have a good set of rules, evaluate the rulesand update the View proeprties.
					if (false == useDefaultYearlyRecurrence)
					{
						this.IsYearlyPatternTypeSimple = false;
						this.IsYearlyPatternTypeComplex = true;

						// SSP 4/27/11 TFS66178/TFS73425
						// See the changes made in the UpdateMonthlyRecurrenceFromViewModel method.
						// 
						//int relativePosition = weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition;
						int relativePosition = subsetRules.Count > 0 ? subsetRules[0].OccurrenceInstance 
							: ( weekdayRules.Count > 0 ? weekdayRules[0].RelativePosition : weekendDayRules[0].RelativePosition );

						// Set the DayOfWeekOrdinalYearly based on the relative position of the DayOfWeek rules.
						if (relativePosition == -1)
						{
							// MD 2/17/11 - TFS66136
							// This is incorrect. -1 indicates the "Last" value, not the default.
							//this.DayOfWeekOrdinalYearly = this.GetDayOfWeekOrdinalInMonthForDate(this.StartAsLocal);
							this.DayOfWeekOrdinalYearly = ActivityRecurrenceDialogCore.DayOfWeekOrdinalValueLast;
						}
						else
							this.DayOfWeekOrdinalYearly = relativePosition - 1;

						// Set the DayOfWeekYearly value
						if (hasWeekdaysOnly)
							this.DayOfWeekYearly = 1;
						else if (hasWeekendDaysOnly)
							this.DayOfWeekYearly = 2;
						else if (weekdayRules.Count + weekendDayRules.Count > 1)
							this.DayOfWeekYearly = 0;
						else
						{
							// Determine which day of the week has been set.
							if (weekdayRules.Count > 0)
								this.DayOfWeekYearly = this.GetDayDescriptionIndexFromDayOfWeek(weekdayRules[0].Day);
							else
								this.DayOfWeekYearly = this.GetDayDescriptionIndexFromDayOfWeek(weekendDayRules[0].Day);
						}

						this.MonthOfYearComplex = monthOfYearRules[0].Month - 1;
					}

					// If we found rules we cannot handle, ignore the rules in the recurrence and default to
					// a simple rule.
					if (useDefaultYearlyRecurrence)
					{
						this.Recurrence					= this.GetDefaultYearlyRecurrence();
						this.IntervalYearly				= this.Recurrence.Interval;
						this.IsYearlyPatternTypeSimple	= true;
						this.IsYearlyPatternTypeComplex = false;

						this.GetYearlyRulesByType(this.Recurrence, out weekdayRules, out weekendDayRules, out dayOfMonthDayRules, out monthOfYearRules, out subsetRules, out otherRules);
						this.MonthOfYearSimple			= monthOfYearRules[0].Month - 1;
						this.DayOfMonthYearly			= dayOfMonthDayRules[0].DayOfMonth;
					}

					// Reset the simple pattern view properties.
					this.ResetYearlyPatternTypeSimpleViewProperties();
				}
			}
			finally { this._initializingView = false; }
		}
		#endregion //UpdateViewModelFromYearlyRecurrence

		#region UpdateDailyRecurrenceFromViewModel
		private void UpdateDailyRecurrenceFromViewModel()
		{
			if (false == this.IsInitializationComplete)
				return;

			DateRecurrence dailyRecurrence	= new DateRecurrence();
			dailyRecurrence.Interval		= this.IntervalDaily;
			dailyRecurrence.Frequency		= DateRecurrenceFrequency.Daily;

			if (this.IsDailyPatternTypeWeekday)
			{
				dailyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Monday));
				dailyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Tuesday));
				dailyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Wednesday));
				dailyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Thursday));
				dailyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Friday));

				// JM 04-05-11 TFS68431
				this.ResetDailyPatternTypeSimpleViewProperties();
				dailyRecurrence.Interval = this.IntervalDaily;
			}

			// JM 02-07-11 TFS64382 - Update the recurrence range on the newly created recurrence.
			this.UpdateRecurrenceRangeFromViewModel(dailyRecurrence);

			this.Recurrence = dailyRecurrence;
		}
		#endregion //UpdateDailyRecurrenceFromViewModel

		#region UpdateRecurrenceRangeFromViewModel
		// JM 02-07-11 TFS64382 - Pass a parameter that represents the Recurrence to be modified and change all refereces in the method to this parameter.
		//private void UpdateRecurrenceRangeFromViewModel()
		private void UpdateRecurrenceRangeFromViewModel(DateRecurrence recurrence)
		{
			if (false == this.IsInitializationComplete || null == recurrence)
				return;

			if (this.IsRangeEndAfter)
			{
				recurrence.Count	= this.RangeEndAfterOccurrenceNumber;
				recurrence.Until	= null;
			}
			else if (this.IsRangeEndBy)
			{
				recurrence.Count	= 0;
				recurrence.Until	= this.RangeEndDate;
			}
			else
			{
				recurrence.Count	= 0;
				recurrence.Until	= null;
			}
		}
		#endregion //UpdateRecurrenceRangeFromViewModel

		#region UpdateWeeklyRecurrenceFromViewModel
		private void UpdateWeeklyRecurrenceFromViewModel()
		{
			if (false == this.IsInitializationComplete)
				return;

			DateRecurrence weeklyRecurrence = new DateRecurrence();
			weeklyRecurrence.Interval		= this.IntervalWeekly;
			weeklyRecurrence.Frequency		= DateRecurrenceFrequency.Weekly;

			if (this.IsWeeklyOnSunday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Sunday));
			if (this.IsWeeklyOnMonday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Monday));
			if (this.IsWeeklyOnTuesday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Tuesday));
			if (this.IsWeeklyOnWednesday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Wednesday));
			if (this.IsWeeklyOnThursday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Thursday));
			if (this.IsWeeklyOnFriday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Friday));
			if (this.IsWeeklyOnSaturday)
				weeklyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(DayOfWeek.Saturday));

			// JM 02-07-11 TFS64382 - Update the recurrence range on the newly created recurrence.
			this.UpdateRecurrenceRangeFromViewModel(weeklyRecurrence);

			this.Recurrence = weeklyRecurrence;
		}
		#endregion //UpdateWeeklyRecurrenceFromViewModel

		#region UpdateMonthlyRecurrenceFromViewModel
		private void UpdateMonthlyRecurrenceFromViewModel()
		{
			if (false == this.IsInitializationComplete)
				return;

			DateRecurrence monthlyRecurrence	= new DateRecurrence();
			monthlyRecurrence.Frequency			= DateRecurrenceFrequency.Monthly;

			if (this.IsMonthlyPatternTypeSimple == this.IsMonthlyPatternTypeComplex)
				return;

			if (this.IsMonthlyPatternTypeSimple)
			{
				monthlyRecurrence.Interval = this.IntervalMonthly;
				monthlyRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(this.DayNumberMonthly));

				// Reset the complex pattern view properties.
				this.ResetMonthlyPatternTypeComplexViewProperties();
			}
			else  // Complex pattern
			{
				monthlyRecurrence.Interval = this.IntervalMonthlyComplex;

				int relativePosition = 0;

				// MD 2/17/11 - TFS66136
				// Put this value in a constant.
				//if (this.DayOfWeekOrdinalMonthly == 4)
				if (this.DayOfWeekOrdinalMonthly == ActivityRecurrenceDialogCore.DayOfWeekOrdinalValueLast)
					relativePosition = -1;
				else
					relativePosition = this.DayOfWeekOrdinalMonthly + 1;

				// SSP 4/26/11 TFS66178/TFS73425
				// Day, Weekday or Weekend Day require a subset rule. The subset rule selects the first, second, third, fourth or
				// the matching day among the days that are added using DayOfWeekRecurrenceRules.
				// 
				int dayOfWeekMonthly = this.DayOfWeekMonthly;
				bool needsSubsetRule = 0 != relativePosition && ( dayOfWeekMonthly == 0 || dayOfWeekMonthly == 1 || dayOfWeekMonthly == 2 );

				if (this.DayOfWeekMonthly > -1)
				{
					DayOfWeek[] daysOfWeek = this.GetDaysOfWeekFromDayDescription(this.DayDescriptions[this.DayOfWeekMonthly]);
					foreach (DayOfWeek dow in daysOfWeek)
					{
						// SSP 4/26/11 TFS66178/TFS73425
						// If we need to use subset rule to match only one of the day, then we need to specify the relative
						// position via the subset rule which we add further below. 
						// 
						//monthlyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(dow, relativePosition));
						monthlyRecurrence.Rules.Add( new DayOfWeekRecurrenceRule( dow, needsSubsetRule ? 0 : relativePosition ) );
					}
				}

				// If this is a weekend or weekday pattern, and they relative position is -1 (i.e.
				// they want the 'last' occurrence) add an appropriate subset rule so we only get the last occurrence.
				// SSP 4/11/11 TFS66178/TFS73425
				// 
				// ----------------------------------------------------------------------------------------------
				// Day, Weekday or Weekend Day have multiple days added to the rules so we need to choose
				// a single one, either the first, second, third, fourth or the last matching day based on
				// the relativePosition value.
				// 
				if ( needsSubsetRule )
					monthlyRecurrence.Rules.Add( new SubsetRecurrenceRule( relativePosition ) );
				//if (relativePosition == -1)
				//{
				//    if (this.DayOfWeekMonthly == 1)	// Weekday
				//        monthlyRecurrence.Rules.Add(new SubsetRecurrenceRule(2));
				//    else if (this.DayOfWeekMonthly == 2)	// Weekend Day
				//        monthlyRecurrence.Rules.Add(new SubsetRecurrenceRule(5));
				//}
				// ----------------------------------------------------------------------------------------------

				// Reset the simple pattern view properties.
				this.ResetMonthlyPatternTypeSimpleViewProperties();
			}

			// JM 02-07-11 TFS64382 - Update the recurrence range on the newly created recurrence.
			this.UpdateRecurrenceRangeFromViewModel(monthlyRecurrence);

			this.Recurrence = monthlyRecurrence;
		}
		#endregion //UpdateMonthlyRecurrenceFromViewModel

		#region UpdateYearlyRecurrenceFromViewModel
		private void UpdateYearlyRecurrenceFromViewModel()
		{
			if (false == this.IsInitializationComplete)
				return;

			DateRecurrence yearlyRecurrence = new DateRecurrence();
			yearlyRecurrence.Interval		= this.IntervalYearly;
			yearlyRecurrence.Frequency		= DateRecurrenceFrequency.Yearly;

			if (this.IsYearlyPatternTypeSimple == this.IsYearlyPatternTypeComplex)
				return;

			if (this.IsYearlyPatternTypeSimple)
			{
				yearlyRecurrence.Rules.Add(new DayOfMonthRecurrenceRule(this.DayOfMonthYearly));
				yearlyRecurrence.Rules.Add(new MonthOfYearRecurrenceRule(this.MonthOfYearSimple + 1));

				// Reset the complex pattern view properties.
				this.ResetYearlyPatternTypeComplexViewProperties();
			}
			else  // Complex pattern
			{
				int relativePosition = 0;

				// MD 2/17/11 - TFS66136
				// Put this value in a constant.
				//if (this.DayOfWeekOrdinalYearly == 4)
				if (this.DayOfWeekOrdinalYearly == ActivityRecurrenceDialogCore.DayOfWeekOrdinalValueLast)
					relativePosition = -1;
				else
					relativePosition = this.DayOfWeekOrdinalYearly + 1;

				// SSP 4/26/11 TFS66178/TFS73425
				// Day, Weekday or Weekend Day require a subset rule. The subset rule selects the first, second, third, fourth or
				// the matching day among the days that are added using DayOfWeekRecurrenceRules.
				// 
				int dayOfWeekYearly = this.DayOfWeekYearly;
				bool needsSubsetRule = 0 != relativePosition && ( dayOfWeekYearly == 0 || dayOfWeekYearly == 1 || dayOfWeekYearly == 2 );

				if (this.DayOfWeekYearly > -1)
				{
					DayOfWeek[] daysOfWeek = this.GetDaysOfWeekFromDayDescription(this.DayDescriptions[this.DayOfWeekYearly]);
					foreach (DayOfWeek dow in daysOfWeek)
					{
						// SSP 4/26/11 TFS66178/TFS73425
						// If we need to use subset rule to match only one of the day, then we need to specify the relative
						// position via the subset rule which we add further below. 
						// 
						//yearlyRecurrence.Rules.Add(new DayOfWeekRecurrenceRule(dow, relativePosition));
						yearlyRecurrence.Rules.Add( new DayOfWeekRecurrenceRule( dow, needsSubsetRule ? 0 : relativePosition ) );
					}
				}

				// If this is a weekend or weekday pattern, and they relative position is -1 (i.e.
				// they want the 'last' occurrence) add an appropriate subset rule so we only get the last occurrence.
				// SSP 4/11/11 TFS66178/TFS73425
				// 
				// ----------------------------------------------------------------------------------------------
				// Day, Weekday or Weekend Day have multiple days added to the rules so we need to choose
				// a single one, either the first, second, third, fourth or the last matching day based on
				// the relativePosition value.
				// 
				if ( needsSubsetRule )
					yearlyRecurrence.Rules.Add( new SubsetRecurrenceRule( relativePosition ) );
				//if (relativePosition == -1)
				//{
				//    if (this.DayOfWeekYearly == 1)	// Weekday
				//        yearlyRecurrence.Rules.Add(new SubsetRecurrenceRule(2));
				//    else if (this.DayOfWeekYearly == 2)	// Weekend Day
				//        yearlyRecurrence.Rules.Add(new SubsetRecurrenceRule(5));
				//}
				// ----------------------------------------------------------------------------------------------

				yearlyRecurrence.Rules.Add(new MonthOfYearRecurrenceRule(this.MonthOfYearComplex + 1));

				// Reset the simple pattern view properties.
				this.ResetYearlyPatternTypeSimpleViewProperties();
			}

			// JM 02-07-11 TFS64382 - Update the recurrence range on the newly created recurrence.
			this.UpdateRecurrenceRangeFromViewModel(yearlyRecurrence);

			this.Recurrence = yearlyRecurrence;
		}
		#endregion //UpdateYearlyRecurrenceFromViewModel

		#endregion //Private methods

		#endregion //Methods

		#region Event Handlers

		#region _tbDailyFrequency_GotFocus
		void _tbDailyFrequency_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsDailyPatternTypeEvery = true;
		}
		#endregion //_tbDailyFrequency_GotFocus

		#region _tbMonthlyDayNumber_GotFocus
		void _tbMonthlyDayNumber_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsMonthlyPatternTypeSimple = true;
		}
		#endregion //_tbMonthlyFrequency_GotFocus

		#region _tbMonthlyFrequency_GotFocus
		void _tbMonthlyFrequency_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsMonthlyPatternTypeSimple = true;
		}
		#endregion //_tbMonthlyFrequency_GotFocus

		#region _tbMonthlyFrequencyComplex_GotFocus
		void _tbMonthlyFrequencyComplex_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsMonthlyPatternTypeComplex = true;
		}
		#endregion //_tbMonthlyFrequencyComplex_GotFocus

		#region _tbYearlyDayNumber_GotFocus
		void _tbYearlyDayNumber_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsYearlyPatternTypeSimple = true;
		}
		#endregion //_tbYearlyDayNumber_GotFocus

		// JM 04-05-11 TFS71183 Added.
		#region _dpRangeEndAfterDate_GotFocus
		void _dpRangeEndAfterDate_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsRangeEndBy = true;
		}
		#endregion //_dpRangeEndAfterDate_GotFocus

		// JM 04-05-11 TFS71183 Added.
		#region _dpRangeEndAfterDate_KeyDown
		void _dpRangeEndAfterDate_KeyDown(object sender, RoutedEventArgs e)
		{
			this.IsRangeEndBy = true;
		}
		#endregion //_dpRangeEndAfterDate_KeyDown

		#region _tbRangeEndAfterOccurrenceNumber_GotFocus
		void _tbRangeEndAfterOccurrenceNumber_GotFocus(object sender, RoutedEventArgs e)
		{
			this.IsRangeEndAfter = true;
		}
		#endregion //_tbRangeEndAfterOccurrenceNumber_GotFocus

		#endregion //Event Handlers

		#region IDialogElementProxyHost Members
		bool IDialogElementProxyHost.OnClosing()
		{
			if (this._isDirty)
			{
				// If we are here, that means the user either clicked Cancel or clicked the Close Box on the dialog.
				// If the user had clicked OK, we would have already updated the Activity and cleared our dirty status.
				if (this._shouldCallBeginAndEndEdit)
				{
					DataErrorInfo errorInfo;
					this._dataManager.CancelEdit(this._activityOriginal, out errorInfo);
				}
				else
				{
					// JM 04-11-11 TFS70524.  This is no longer necessary since the dialog no longer 
					// manipulates the recurrence on the original activity - it always works with the clone.
					//
					// Since we have not done a BeginEdit (presumably the caller of this dialog has, i.e.
					// the Appointment Dialog) restore the activity field that the end user might have changed.
					//if (this._activity.Start != this._originalActivityStart)
					//    this._activity.Start = this._originalActivityStart;
					//if (this.DataManager.IsEndDateSupportedByActivity(this._activity))
					//{
					//    if (this._activity.End != this._originalActivityEnd)
					//        this._activity.End = this._originalActivityEnd;
					//}

					//this.Recurrence = this._originalRecurrence;
				}

				this.UpdateDirtyStatus(false);
			}

			// Unhook our property changed event listener
			// JM 04-11-11 TFS70524.  Reference the Activity property rather than the member variable.
			this.Activity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);

			// Don't cancel the closing.
			return false;
		}

		#endregion

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is RecurrenceDialogCoreCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return command is RecurrenceDialogCoreCommandBase;
		}

		#endregion

		#region ITimePickerManagerHost Members

		DateTime ITimePickerManagerHost.StartAsLocal
		{
			get { return this.StartAsLocal; }
		}

		DateTime ITimePickerManagerHost.EndAsLocal
		{
			get { return this.EndAsLocal; }
		}

		TimeZoneToken ITimePickerManagerHost.ActivityStartTimeZoneTokenResolved
		{
			get { return this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved; }
		}

		TimeZoneToken ITimePickerManagerHost.ActivityEndTimeZoneTokenResolved
		{
			get { return this._timeZoneTokenHelper.ActivityEndTimeZoneTokenResolved; }
		}

		bool ITimePickerManagerHost.IsOkToParseEndTimePickerText
		{
			get { return true; }
		}

		void ITimePickerManagerHost.UpdateDirtyStatus(bool isDirty)
		{
			this.UpdateDirtyStatus(isDirty);
		}

		void ITimePickerManagerHost.UpdateConflictStatus()
		{
			// None at this time
		}

		bool ITimePickerManagerHost.HasDateConflicts(bool displayMessageOnConflict)
		{
			// None at this time.
			return false;
		}

		void ITimePickerManagerHost.UpdateStartDatePickerDateFromNewStartTime(DateTime newStartTime)
		{
			// Not applicable for this dialog.
		}

		void ITimePickerManagerHost.UpdateEndDatePickerDateFromNewEndTime(DateTime newEndTime)
		{
			// Not applicable for this dialog.
		}
		#endregion //ITimePickerManagerHost Members

		#region PatternType enum
		private enum PatternType
		{
			Daily,
			Weekly,
			Monthly,
			Yearly
		}
		#endregion //PatternType enum
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