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
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Abstract base class that supports displays of a UI for the core portion of the <see cref="Appointment"/>, <see cref="Task"/> and <see cref="Journal"/> dialogs. 
	/// </summary>
	/// <seealso cref="AppointmentDialogCore"/>
	/// <seealso cref="TaskDialogCore"/>
	/// <seealso cref="JournalDialogCore"/>
	/// <seealso cref="Appointment"/>
	/// <seealso cref="Task"/>
	/// <seealso cref="Journal"/>
	[TemplatePart(Name = PartNavigationControlSite, Type = typeof(ContentControl))]
	[TemplatePart(Name = PartStartDatePicker, Type = typeof(DatePicker))]
	[TemplatePart(Name = PartEndDatePicker, Type = typeof(DatePicker))]





	[TemplatePart(Name = PartStartTimePicker, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartEndTimePicker, Type = typeof(ComboBox))]
	[TemplatePart(Name = PartDurationTimePicker, Type = typeof(ComboBox))]

	[DesignTimeVisible(false)]
	public abstract class ActivityDialogCore : Control, 
											   ICommandTarget,
											   IDialogElementProxyHost,
											   IPropertyChangeListener,
											   ITimePickerManagerHost
	{
		#region Member Variables

		// Template part names
		private const string PartNavigationControlSite	= "NavigationControlSite";
		private const string PartStartDatePicker		= "StartDatePicker";
		private const string PartEndDatePicker			= "EndDatePicker";
		private const string PartStartTimePicker		= "StartTimePicker";
		private const string PartEndTimePicker			= "EndTimePicker";
		private const string PartStartTimeZonePicker	= "StartTimeZonePicker";
		private const string PartEndTimeZonePicker		= "EndTimeZonePicker";
		private const string PartDurationTimePicker		= "DurationTimePicker";

		// Template part elements
		private ContentControl							_navigationControlSite;
		private DatePicker								_startDatePicker;
		private DatePicker								_endDatePicker;

		private FrameworkElement						_container;
		private XamScheduleDataManager					_dataManager;
		private ActivityBase							_activity;
		private ActivityBase							_activityOriginal;
		private bool									_isDirty;
		private bool									_isNew;
		private TimeZoneTokenHelper						_timeZoneTokenHelper;
		private DialogElementProxy						_dialogElementProxy;
		private bool									_acceptClosingWithNoProcessing;
		private bool									_isInEndEdit;
		private bool									_isActivityRecurrenceDialogActive;
		private bool									_initialized;
		private string									_lastStartDatePickerText;
		private DateTime								_lastStartDatePickerDate;
		private string									_lastEndDatePickerText;
		private DateTime								_lastEndDatePickerDate;
		private bool									_ignoreNextEndDatePickerSelectionChange;
		private TimePickerManager						_timePickerManager;
		private string									_lastStartTimeZoneId;
		private string									_lastEndTimeZoneId;
		private DispatcherOperation						_revertTimeZoneOperationPending;
		private ActivityQueryResult						_queryResult;
		private bool									_hasStartTimePicker;
		private bool									_hasEndTimePicker;






		// JM 12-9-10 TFS60567 Added.
		private Stack<DispatcherOperation>				_revertEndDatePickerOperationsPending;

		private bool									_descriptionModified;
		private ActivityCategoryHelper					_activityCategoryHelper;

		#endregion //Member Variables

		#region Constructor
		/// <param name="container">The FrameworkElement that contains this dialog, or null if there is no containing element.</param>
		/// <param name="dataManager">The current <see cref="XamScheduleDataManager"/>.</param>
		/// <param name="activity">The activity to display.</param>
		protected ActivityDialogCore(FrameworkElement container, XamScheduleDataManager dataManager, ActivityBase activity)
		{
			CoreUtilities.ValidateNotNull(activity, "activity");
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");

			this._container				= container;
			this._dataManager			= dataManager;
			this._activityOriginal		= activity;

			this._isNew					= activity.IsAddNew;

			// If this is a new activity then consider the dialog dirty from the beginning.
			this._isDirty				= activity.IsAddNew;

			// Call BeginEdit to put the Activity in 'edit mode'.
			DataErrorInfo errorInfo;
			this._activity				= dataManager.BeginEditWithCopy(activity, true, out errorInfo) as ActivityBase;
			if (errorInfo != null)
			{



				MessageBox.Show(errorInfo.ToString(), this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_EditAppointment"), MessageBoxButton.OK, MessageBoxImage.Exclamation);


				
			}

			// Set our DataContext to the copy of the appointment returned from XamDataManager.BeginEditWithCopy called above.
			if (this._activity != null)
				this.DataContext = this._activity;

			// Initialize the local and appointment time zone tokens.
			this._timeZoneTokenHelper = new TimeZoneTokenHelper(this.Activity, this.DataManager);
			this._timeZoneTokenHelper.VerifyTimeZoneTokens();

			// If this is a new appointment, set the Start/End time zone ids.
			if (this.IsNew)
			{
				this.Activity.StartTimeZoneId	= this.TimeZoneTokenHelper.LocalTimeZoneToken.Id;
				this.Activity.EndTimeZoneId		= this.TimeZoneTokenHelper.LocalTimeZoneToken.Id;
			}

			// If the Start/End time zone ids are null, set it to the Utc time zone id if available.
			TimeZoneToken utcToken = this.DataManager.TimeZoneInfoProviderResolved.UtcToken;
			if (string.IsNullOrEmpty(this.Activity.StartTimeZoneId) && null != utcToken)
			{
				this.Activity.StartTimeZoneId = utcToken.Id;
				this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
			}

			if (string.IsNullOrEmpty(this.Activity.EndTimeZoneId) && null != utcToken)
			{
				this.Activity.EndTimeZoneId = utcToken.Id;
				this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
			}

			// Save the time zone id values;
			this._lastStartTimeZoneId	= this.Activity.StartTimeZoneId;
			this._lastEndTimeZoneId		= this.Activity.EndTimeZoneId;

			// Initialize the visibility of the Time Zone Pickers.  Need to do this here because external dialogs
			// need to initialize Time Zone Picker visibility
			if (this.CanChangeTimeZonePickerVisibility == false)
				this.TimeZonePickerVisibility = Visibility.Visible;

			// Initialize properties related to Recurrence
			this.VerifyRecurrenceStatus();
		}
		#endregion //Constructor

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
			this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.InitializePrivate));
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

		#region Protected Properties

		#region Container
		/// <summary>
		/// Returns the FrameworkElement that contains this dialog, or null if there is no containing element. (read only)
		/// </summary>
		protected FrameworkElement Container
		{
			get { return this._container; }
		}
		#endregion //Container

		#region EndAsLocal
		/// <summary>
		/// Returns the activity's End relative to the End timezone. (read only)
		/// </summary>
		protected DateTime EndAsLocal
		{
			get { return this.Activity.GetEndLocal(this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved); }
		}
		#endregion //EndAsLocal

		#region IsActivityRecurrenceDialogActive
		/// <summary>
		/// Returns true if an instance of <see cref="ActivityRecurrenceDialogCore"/> is currently active. (read only)
		/// </summary>
		protected bool IsActivityRecurrenceDialogActive
		{
			get { return this._isActivityRecurrenceDialogActive; }
		}
		#endregion //IsActivityRecurrenceDialogActive

		#region IsInEndEdit
		/// <summary>
		/// Returns true if the <see cref="ActivityBase"/> is in the middle of an <see cref="XamScheduleDataManager"/> EndEdit call. (read only)
		/// </summary>
		protected bool IsInEndEdit
		{
			get { return this._isInEndEdit; }
		}
		#endregion //IsInEndEdit

		#region IsNew
		/// <summary>
		/// Returns true if the <see cref="ActivityBase"/> is being created by the dialog, false if it is being edited. (read only)
		/// </summary>
		protected bool IsNew
		{
			get { return this._isNew; }
		}
		#endregion //IsNew

		#region StartAsLocal
		/// <summary>
		/// Returns the activity's Start relative to the Start timezone. (read only)
		/// </summary>
		protected DateTime StartAsLocal
		{
			get { return this.Activity.GetStartLocal(this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved); }
		}
		#endregion //StartAsLocal

		#endregion Protected Properties

		#region Internal Protected

		#region ActivityOriginal
		/// <summary>
		/// Returns the original <see cref="ActivityBase"/> associated with the dialog. (read only)
		/// </summary>
		internal protected ActivityBase ActivityOriginal
		{
			get { return this._activityOriginal; }
		}
		#endregion //ActivityOriginal

		#endregion Internal Protected

		#region Internal Protected Virtual

		#region CanChangeTimeZonePickerVisibility
		/// <summary>
		/// Returns true if the dialog's timezone picker (if any) visibility can be changed, otherwise false.
		/// </summary>
		internal protected virtual bool CanChangeTimeZonePickerVisibility
		{
			get
			{
				DataErrorInfo errorInfo;

				TimeZoneToken startToken =
					ScheduleUtilities.GetTimeZoneToken(this.DataManager.DataConnector,
													   this.Activity.StartTimeZoneId,
													   false,
													   out errorInfo,
													   this.Activity);

				TimeZoneToken endToken =
					ScheduleUtilities.GetTimeZoneToken(this.DataManager.DataConnector,
													   this.Activity.EndTimeZoneId,
													   false,
													   out errorInfo,
													   this.Activity);

				if (startToken == null && endToken == null)
					return true;

				if ((startToken != null && startToken != this.TimeZoneTokenHelper.LocalTimeZoneToken) ||
					(endToken != null && endToken != this.TimeZoneTokenHelper.LocalTimeZoneToken))
					return false;

				return true;
			}
		}
		#endregion //CanChangeTimeZonePickerVisibility

		#endregion //Internal Protected Virtual

		#region Public Properties

		#region Activity
		/// <summary>
		/// Returns the <see cref="ActivityBase"/> instance being edited by the dialog. (read only)
		/// </summary>
		public ActivityBase Activity
		{
			get { return this._activity; }
		}
		#endregion //Activity

		#region ActivityCategoryListItems

		private static readonly DependencyPropertyKey ActivityCategoryListItemsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ActivityCategoryListItems",
			typeof(ReadOnlyObservableCollection<ActivityCategoryListItem>), typeof(ActivityDialogCore), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ActivityCategoryListItems"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityCategoryListItemsProperty = ActivityCategoryListItemsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a read only collection of <see cref="ActivityCategoryListItem"/>s that represent the <see cref="ActivityCategory"/>s assigned to the <see cref="ActivityBase"/> being edited. (read only)
		/// </summary>
		/// <seealso cref="ActivityCategoryListItemsProperty"/>
		/// <seealso cref="ActivityCategoryListItemsVisibility"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public ReadOnlyObservableCollection<ActivityCategoryListItem> ActivityCategoryListItems
		{
			get
			{
				return (ReadOnlyObservableCollection<ActivityCategoryListItem>)this.GetValue(ActivityDialogCore.ActivityCategoryListItemsProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.ActivityCategoryListItemsPropertyKey, value);
			}
		}

		#endregion //ActivityCategoryListItems

		#region ActivityCategoryListItemsVisibility
		/// <summary>
		/// Identifies the <see cref="ActivityCategoryListItemsVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityCategoryListItemsVisibilityProperty = DependencyPropertyUtilities.Register("ActivityCategoryListItemsVisibility",
			typeof(Visibility), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Returns Visibility.Visible if <see cref="ActivityCategory"/>s have been assigned the <see cref="ActivityBase"/> and they should be displayed in the <see cref="ActivityDialogCore"/>.
		/// </summary>
		/// <seealso cref="ActivityCategoryListItemsVisibilityProperty"/>
		/// <seealso cref="ActivityCategoryListItems"/>
		/// <seealso cref="ActivityCategoryListItemsVisibility"/>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public Visibility ActivityCategoryListItemsVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityDialogCore.ActivityCategoryListItemsVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.ActivityCategoryListItemsVisibilityProperty, value);
			}
		}
		#endregion //ActivityCategoryListItemsVisibility

		#region ConflictMessage

		private static readonly DependencyPropertyKey ConflictMessagePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ConflictMessage",
			typeof(string), typeof(ActivityDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="ConflictMessage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ConflictMessageProperty = ConflictMessagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a message that describes the conflict.  This property is only valid when the <see cref="HasConflicts"/> property is set to true.
		/// </summary>
		/// <seealso cref="ConflictMessageProperty"/>
		/// <seealso cref="HasConflicts"/>
		/// <seealso cref="Appointment"/>
		public string ConflictMessage
		{
			get
			{
				return (string)this.GetValue(ActivityDialogCore.ConflictMessageProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.ConflictMessagePropertyKey, value);
			}
		}

		#endregion //ConflictMessage

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"/> associated with the dialog.
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

		#region DescriptionFormatWarningMessage

		private static readonly DependencyPropertyKey DescriptionFormatWarningMessagePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DescriptionFormatWarningMessage",
			typeof(string), typeof(ActivityDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="DescriptionFormatWarningMessage"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DescriptionFormatWarningMessageProperty = DescriptionFormatWarningMessagePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a message that displays a warning about the loss of formatting on the <see cref="ActivityBase"/>'s Description field.  This property is only valid when the <see cref="DescriptionFormatWarningMessageVisibility"/> property is set to Visible.
		/// </summary>
		/// <seealso cref="DescriptionFormatWarningMessageProperty"/>
		/// <seealso cref="DescriptionFormatWarningMessageVisibility"/>
		/// <seealso cref="ActivityBase.Description"/>
		public string DescriptionFormatWarningMessage
		{
			get
			{
				return (string)this.GetValue(ActivityDialogCore.DescriptionFormatWarningMessageProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.DescriptionFormatWarningMessagePropertyKey, value);
			}
		}

		#endregion //DescriptionFormatWarningMessage

		#region DescriptionFormatWarningMessageVisibility
		/// <summary>
		/// Identifies the <see cref="DescriptionFormatWarningMessageVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DescriptionFormatWarningMessageVisibilityProperty = DependencyPropertyUtilities.Register("DescriptionFormatWarningMessageVisibility",
			typeof(Visibility), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Returns/sets the Visibility of a message that displays a warning about the loss of formatting on the <see cref="ActivityBase"/>'s Description field.
		/// </summary>
		/// <seealso cref="DescriptionFormatWarningMessageVisibilityProperty"/>
		/// <seealso cref="DescriptionFormatWarningMessage"/>
		/// <seealso cref="ActivityBase.Description"/>
		public Visibility DescriptionFormatWarningMessageVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityDialogCore.DescriptionFormatWarningMessageVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.DescriptionFormatWarningMessageVisibilityProperty, value);
			}
		}
		#endregion //DescriptionFormatWarningMessageVisibility

		#region HasConflicts

		private static readonly DependencyPropertyKey HasConflictsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasConflicts",
				typeof(bool), typeof(ActivityDialogCore), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="HasConflicts"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasConflictsProperty = HasConflictsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the <see cref="Appointment"/> conflicts with or is adjacent to another <see cref="Appointment"/>.  If so the <see cref="ConflictMessage"/>
		/// property contains a message that describes the conflict
		/// </summary>
		/// <seealso cref="HasConflictsProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ConflictMessage"/>
		public bool HasConflicts
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.HasConflictsProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.HasConflictsPropertyKey, value);
			}
		}

		#endregion //HasConflicts

		#region IsActivityModifiable

		/// <summary>
		/// Identifies the <see cref="IsActivityModifiable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActivityModifiableProperty = DependencyPropertyUtilities.Register("IsActivityModifiable",
			typeof(bool), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null));

		/// <summary>
		/// Returns or sets whether the <see cref="ActivityBase"/> displayed in the dialog can be edited.
		/// </summary>
		/// <seealso cref="IsActivityModifiableProperty"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="ScheduleSettings"/>
		public bool IsActivityModifiable
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.IsActivityModifiableProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.IsActivityModifiableProperty, value);
			}
		}

		#endregion //IsActivityModifiable

		#region IsActivityRemoveable

		/// <summary>
		/// Identifies the <see cref="IsActivityRemoveable"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActivityRemoveableProperty = DependencyPropertyUtilities.Register("IsActivityRemoveable",
			typeof(bool), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, null));

		/// <summary>
		/// Returns or sets whether the <see cref="Activity"/> being displayed in the dialog can be removed.
		/// </summary>
		/// <seealso cref="IsActivityRemoveableProperty"/>
		/// <seealso cref="Activity"/>
		/// <seealso cref="ScheduleSettings"/>
		public bool IsActivityRemoveable
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.IsActivityRemoveableProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.IsActivityRemoveableProperty, value);
			}
		}

		#endregion //IsActivityRemoveable

		#region IsDirty
		/// <summary>
		/// Returns true if the <see cref="ActivityBase"/> has been modified, otherwise returns false.  (read only)
		/// </summary>
		public bool IsDirty
		{
			get { return this._isDirty; }
		}
		#endregion //IsDirty

		#region IsOccurrence

		private static readonly DependencyPropertyKey IsOccurrencePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsOccurrence",
				typeof(bool), typeof(ActivityDialogCore), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsOccurrence"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsOccurrenceProperty = IsOccurrencePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the <see cref="ActivityBase"/> represents an <see cref="ActivityBase"/> occurrence in a recurrence series
		/// </summary>
		/// <seealso cref="IsOccurrenceProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="IsRecurrenceRoot"/>
		public bool IsOccurrence
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.IsOccurrenceProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.IsOccurrencePropertyKey, value);
			}
		}

		#endregion //IsOccurrence

		#region IsRecurrenceRoot

		private static readonly DependencyPropertyKey IsRecurrenceRootPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsRecurrenceRoot",
				typeof(bool), typeof(ActivityDialogCore), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsRecurrenceRoot"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsRecurrenceRootProperty = IsRecurrenceRootPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns whether the <see cref="ActivityBase"/> represents the root <see cref="ActivityBase"/> in a recurrence series
		/// </summary>
		/// <seealso cref="IsRecurrenceRootProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="IsOccurrence"/>
		public bool IsRecurrenceRoot
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.IsRecurrenceRootProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.IsRecurrenceRootPropertyKey, value);
			}
		}

		#endregion //IsRecurrenceRoot

		#region NavigationControlSite
		/// <summary>
		/// Returns the <b>ContentControl</b> Part in the <b>ActivityDialogCore</b> template that is used to display a navigation
		/// control (e.g., a Ribbon or a Menu) for the dialog.
		/// </summary>
		public ContentControl NavigationControlSite
		{
			get
			{
				if (this._navigationControlSite == null)
					return this._navigationControlSite = this.GetTemplateChild(PartNavigationControlSite) as ContentControl;

				return this._navigationControlSite;
			}
		}
		#endregion //NavigationControlSite

		#region NavigationControlSiteContent

		/// <summary>
		/// Identifies the <see cref="NavigationControlSiteContent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty NavigationControlSiteContentProperty = DependencyPropertyUtilities.Register("NavigationControlSiteContent",
			typeof(FrameworkElement), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the content of the <see cref="NavigationControlSite"/>.
		/// </summary>
		/// <seealso cref="NavigationControlSiteContentProperty"/>
		public FrameworkElement NavigationControlSiteContent
		{
			get
			{
				return (FrameworkElement)this.GetValue(ActivityDialogCore.NavigationControlSiteContentProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.NavigationControlSiteContentProperty, value);
			}
		}

		#endregion //NavigationControlSiteContent

		#region OccurrenceDescription

		private static readonly DependencyPropertyKey OccurrenceDescriptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("OccurrenceDescription",
			typeof(string), typeof(ActivityDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="OccurrenceDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OccurrenceDescriptionProperty = OccurrenceDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a message that describes the occurrence.  This property is only valid when the <see cref="OccurrenceDescriptionVisibility"/> property is set to Visible.
		/// </summary>
		/// <seealso cref="OccurrenceDescriptionProperty"/>
		/// <seealso cref="OccurrenceDescriptionVisibility"/>
		/// <seealso cref="RecurrenceRootDescriptionProperty"/>
		/// <seealso cref="RecurrenceRootDescriptionVisibility"/>
		/// <seealso cref="DateRecurrence"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="ActivityBase"/>
		public string OccurrenceDescription
		{
			get
			{
				return (string)this.GetValue(ActivityDialogCore.OccurrenceDescriptionProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.OccurrenceDescriptionPropertyKey, value);
			}
		}

		#endregion //OccurrenceDescription

		#region OccurrenceDescriptionVisibility
		/// <summary>
		/// Identifies the <see cref="OccurrenceDescriptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OccurrenceDescriptionVisibilityProperty = DependencyPropertyUtilities.Register("OccurrenceDescriptionVisibility",
			typeof(Visibility), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Returns/sets whether a message containing the <see cref="ActivityBase"/>'s occurrence details is visible.
		/// </summary>
		/// <seealso cref="OccurrenceDescriptionVisibilityProperty"/>
		/// <seealso cref="IsOccurrence"/>
		/// <seealso cref="DateRecurrence"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="ActivityBase"/>
		public Visibility OccurrenceDescriptionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityDialogCore.OccurrenceDescriptionVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.OccurrenceDescriptionVisibilityProperty, value);
			}
		}
		#endregion //OccurrenceDescriptionVisibility

		#region RecurrenceRootDescription

		private static readonly DependencyPropertyKey RecurrenceRootDescriptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("RecurrenceRootDescription",
			typeof(string), typeof(ActivityDialogCore), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="RecurrenceRootDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecurrenceRootDescriptionProperty = RecurrenceRootDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a message that describes the recurrence series.  This property is only valid when the <see cref="RecurrenceRootDescriptionVisibility"/> property is set to true.
		/// </summary>
		/// <seealso cref="RecurrenceRootDescriptionProperty"/>
		/// <seealso cref="RecurrenceRootDescriptionVisibility"/>
		/// <seealso cref="OccurrenceDescriptionProperty"/>
		/// <seealso cref="OccurrenceDescriptionVisibility"/>
		/// <seealso cref="Appointment"/>
		public string RecurrenceRootDescription
		{
			get
			{
				return (string)this.GetValue(ActivityDialogCore.RecurrenceRootDescriptionProperty);
			}
			internal set
			{
				this.SetValue(ActivityDialogCore.RecurrenceRootDescriptionPropertyKey, value);
			}
		}

		#endregion //RecurrenceRootDescription

		#region RecurrenceRootDescriptionVisibility
		/// <summary>
		/// Identifies the <see cref="RecurrenceRootDescriptionVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecurrenceRootDescriptionVisibilityProperty = DependencyPropertyUtilities.Register("RecurrenceRootDescriptionVisibility",
			typeof(Visibility), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Returns/sets whether a message containing the <see cref="Appointment"/>'s recurrence root details is visible.
		/// </summary>
		/// <seealso cref="RecurrenceRootDescriptionVisibilityProperty"/>
		/// <seealso cref="IsRecurrenceRoot"/>
		public Visibility RecurrenceRootDescriptionVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityDialogCore.RecurrenceRootDescriptionVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.RecurrenceRootDescriptionVisibilityProperty, value);
			}
		}
		#endregion //RecurrenceRootDescriptionVisibility

		#region TimeZonePickerEnabled
		/// <summary>
		/// Identifies the <see cref="TimeZonePickerEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeZonePickerEnabledProperty = DependencyPropertyUtilities.Register("TimeZonePickerEnabled",
			typeof(bool), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Resturns/sets whether the Time Zone pickers are enabled.
		/// </summary>
		/// <seealso cref="TimeZonePickerEnabledProperty"/>
		/// <seealso cref="TimeZonePickerVisibility"/>
		public bool TimeZonePickerEnabled
		{
			get
			{
				return (bool)this.GetValue(ActivityDialogCore.TimeZonePickerEnabledProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.TimeZonePickerEnabledProperty, value);
			}
		}
		#endregion //TimeZonePickerEnabled

		#region TimeZonePickerVisibility
		/// <summary>
		/// Identifies the <see cref="TimeZonePickerVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeZonePickerVisibilityProperty = DependencyPropertyUtilities.Register("TimeZonePickerVisibility",
			typeof(Visibility), typeof(ActivityDialogCore),
			DependencyPropertyUtilities.CreateMetadata(Visibility.Collapsed)
			);

		/// <summary>
		/// Resturns/sets whether the Time Zone pickers are visible.
		/// </summary>
		/// <seealso cref="TimeZonePickerVisibilityProperty"/>
		/// <seealso cref="TimeZonePickerEnabled"/>
		public Visibility TimeZonePickerVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ActivityDialogCore.TimeZonePickerVisibilityProperty);
			}
			set
			{
				this.SetValue(ActivityDialogCore.TimeZonePickerVisibilityProperty, value);
			}
		}
		#endregion //TimeZonePickerVisibility

		#endregion Public Properties

		#region Internal Properties

		#region ActivityCategoryHelper
		internal ActivityCategoryHelper ActivityCategoryHelper
		{
			get
			{
				if (this._activityCategoryHelper == null)
					this._activityCategoryHelper = new ActivityCategoryHelper(this.DataManager, new ActivityBase [] { this.Activity }, this );

				return this._activityCategoryHelper;
			}
		}
		#endregion //ActivityCategoryHelper

		#region DialogElementProxy
		internal DialogElementProxy DialogElementProxy
		{
			get
			{
				if (this._dialogElementProxy == null)
					this._dialogElementProxy = new DialogElementProxy(this);

				return this._dialogElementProxy;
			}
		}
		#endregion //DialogElementProxy

		#region TimeZoneTokenHelper
		internal TimeZoneTokenHelper TimeZoneTokenHelper
		{
			get { return this._timeZoneTokenHelper; }
		}
		#endregion //TimeZoneTokenHelper

		#endregion //Internal Properties

		#region Private Properties

		// JM 12-9-10 TFS60567 Added.
		#region RevertEndDatePickerOperationsPending
		private Stack<DispatcherOperation> RevertEndDatePickerOperationsPending
		{
			get
			{
				if (this._revertEndDatePickerOperationsPending == null)
					this._revertEndDatePickerOperationsPending = new Stack<DispatcherOperation>(5);

				return this._revertEndDatePickerOperationsPending;
			}
		}
		#endregion //RevertEndDatePickerOperationsPending

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

			// Force the CommandSourceManager to requery the status of our commands.
			this.UpdateCommandsStatus();
		}
		#endregion //UpdateDirtyStatus

		#endregion //Public Methods

		#region Protected Methods

		#region UpdateCommandsStatus
		/// <summary>
		/// Force the CommandSourceManager to requery the CanExecute status of the dialog's commands.
		/// </summary>
		protected void UpdateCommandsStatus()
		{
			
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityDialogCoreSaveAndCloseCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityDialogCoreDeleteCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityDialogCoreHideTimeZonePickersCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ActivityDialogCoreShowTimeZonePickersCommand));
		}
		#endregion //UpdateCommandsStatus

		#region VerifyRecurrenceStatus
		/// <summary>
		/// Verifies the Recurrence status of the activity and sets recurrence description properties accordingly for display in the View.
		/// </summary>
		protected void VerifyRecurrenceStatus()
		{
			this.IsRecurrenceRoot			= this.Activity.IsRecurrenceRoot;
			this.IsOccurrence				= this.Activity.IsOccurrence;

			string	recurrenceDescription				= string.Empty;
			bool	useRecurrenceToStringAsDescription	= ScheduleUtilities.GetString("DLG_Recurrence_ShouldShowRecurrenceDescription").ToLower().Trim() == "true";

			// JM 04-20-11 TFS61081 - If we are an occurrence, use the root activity's Recurrence to generate the description.
			RecurrenceBase recurrence = this.Activity.Recurrence;
			if (null == recurrence && null != this.Activity.RootActivity)
				recurrence = this.Activity.RootActivity.Recurrence;

			//if (null != this.Activity.Recurrence && useRecurrenceToStringAsDescription)
			//	recurrenceDescription = this.Activity.Recurrence.ToString();
			if (null != recurrence && useRecurrenceToStringAsDescription)
				recurrenceDescription = recurrence.ToString();

			// Verify IsRecurrenceRoot
			if (this.IsRecurrenceRoot)
			{
				this.RecurrenceRootDescriptionVisibility = Visibility.Visible;

				if (false == string.IsNullOrEmpty(recurrenceDescription))
					this.RecurrenceRootDescription = recurrenceDescription;
				else
					this.RecurrenceRootDescription = ScheduleUtilities.GetString("DLG_Appointment_Core_RecurrenceRootDescription");
			}
			else
				this.RecurrenceRootDescriptionVisibility = Visibility.Collapsed;

			// Verify IsOccurrence
			if (this.IsOccurrence && false == this.IsRecurrenceRoot)
			{
				this.OccurrenceDescriptionVisibility = Visibility.Visible;

				if (false == string.IsNullOrEmpty(recurrenceDescription))
					this.OccurrenceDescription = recurrenceDescription;
				else
					this.OccurrenceDescription = ScheduleUtilities.GetString("DLG_Appointment_Core_OccurrenceDescription");
			}
			else
				this.OccurrenceDescriptionVisibility = Visibility.Collapsed;
		}
		#endregion //VerifyRecurrenceStatus

		#endregion //Protected Methods

		#region Protected Virtual Methods

		#region Initialize
		/// <summary>
		/// Called asynchrounously after the control template has been applied.
		/// </summary>
		protected virtual void Initialize()
		{
		}
		#endregion //Initialize

		#region OnActivityPropertyChanged
		/// <summary>
		/// Called when a property on the Activity changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		protected virtual void OnActivityPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (this.IsInEndEdit)
				return;

			if ( e.PropertyName == "IsTimeZoneNeutral" )
			{
				this.VerifyTimePickerEnabledStatus();
				this.VerifyTimeZonePickerEnabledStatus();

				// If IsAllDayActivity was st to true, force the times to be 12:00 AM and ensure that the start/end dates are at least
				// 1 day apart.
				if (this.Activity.IsTimeZoneNeutral)
				{
					// Force the start/end times to 12:00 AM
					this._timePickerManager.StartTimePickerProxy.Text = ScheduleUtilities.GetTimeString(new DateTime(1, 1, 1, 0, 0, 0));
					this._timePickerManager.ParseTimePickerText(this._timePickerManager.StartTimePickerProxy, true, true);

					this._timePickerManager.EndTimePickerProxy.Text = ScheduleUtilities.GetTimeString(new DateTime(1, 1, 1, 0, 0, 0));
					this._timePickerManager.ParseTimePickerText(this._timePickerManager.EndTimePickerProxy, false, true);

					// Make sure the start/end dates are at least 1 day apart.
					if (this.StartAsLocal.Date == this.EndAsLocal.Date)
					{
						DateInfoProvider	dip		= this.DataManager.DateInfoProviderResolved;
						DateTime?			newDate = dip.Add(this.EndAsLocal, TimeSpan.FromDays(1));
						if (newDate.HasValue && null != this._endDatePicker)
							this._endDatePicker.SelectedDate = newDate;
					}

					this.UpdateCommandsStatus();
				}
				else
				{
					// At this point the IsTimeZoneNeutral property is set to false.  In order to preserve the local time, 
					// get the appointment time and convert it back to UTC by setting the time as local.
					this.Activity.SetStartLocal(this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved, DateTime.SpecifyKind(this.Activity.Start, DateTimeKind.Unspecified));
					this.Activity.SetEndLocal(this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved, DateTime.SpecifyKind(this.Activity.End, DateTimeKind.Unspecified));
				}
			}

			// Don't consider ourselves dirty if the Appointment start/end is being set to the original appointment start/end.  We need to do this here
			// since the DatePicker fires the SelectionChanged event asynchronously, and when we initialize the DatePicker's selected date we
 			// can't ignore the resulting SelectionChanged event because we don't know that it was fired from our initialization code.
			if (e.PropertyName == "Start")
			{
				if (this.Activity.Start == this.ActivityOriginal.Start)
					return;
			}

			if (e.PropertyName == "End")
			{
				if (this.Activity.End == this.ActivityOriginal.End)
					return;
			}

			// Don't consider ourselves dirty if the Recurrence property has been changed while the ActivityRecurrencesDialog is active.
			// We will dirty ourselves if needed when the AActivityRecurrencesDialog closes.
			if (e.PropertyName == "Recurrence" && true == this.IsActivityRecurrenceDialogActive)
				return;

			if (e.PropertyName == "StartTimeZoneId" || e.PropertyName == "EndTimeZoneId")
			{
				bool processEndTimeZoneIdChange = false;

				// Process a StartTimeZoneId change
				if (e.PropertyName == "StartTimeZoneId")
				{
					DateTime oldStartAsLocal		= this.StartAsLocal;
					this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
					this.Activity.Start			= this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved.ConvertToUtc(oldStartAsLocal);
					this._timePickerManager.SynchronizeStartTimePicker();

					// If the Start/End TimeZoneIds were the same before this change, sync up the EndTimeZoneId with the new value.
					if (this._lastStartTimeZoneId == this.Activity.EndTimeZoneId)
					{
						this.Activity.EndTimeZoneId	= this.Activity.StartTimeZoneId;
						processEndTimeZoneIdChange		= true;
					}
				}

				// Process an EndTimeZoneId change
				if (e.PropertyName == "EndTimeZoneId" || processEndTimeZoneIdChange)
				{
					DateTime oldEndAsLocal		= this.EndAsLocal;
					this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
					this.Activity.End		= this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved.ConvertToUtc(oldEndAsLocal);
					this._timePickerManager.SynchronizeEndTimePicker();
				}

				// Check for start/end date conflicts that could result from the time zone id changes.  
				if (this.HasDateConflicts(true))
				{
					if (null == this._revertTimeZoneOperationPending)
						this._revertTimeZoneOperationPending = this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.RevertTimeZoneIds));
				}
				else
				{
					this._lastStartTimeZoneId	= this.Activity.StartTimeZoneId;
					this._lastEndTimeZoneId		= this.Activity.EndTimeZoneId;
				}
			}

			// Don't consider ourselves dirty if the OriginalDescriptionFormat property changes.
			if (e.PropertyName == "OriginalDescriptionFormat" ||
				e.PropertyName == "OriginalDescriptionFormatResolved")
			{
				this.VerifyDescriptionFormat();
				return;
			}

			// Verify the Description format if the Description changes
			if (e.PropertyName == "Description")
			{
				this._descriptionModified = true;
				this.VerifyDescriptionFormat();
			}

			// Verify the ActivityCategoriesListItemsVisibility if the Categories property changes.
			if (e.PropertyName == "Categories")
				this.VerifyActivityCategoryListItemsVisibility();

			// JM 03-31-11 TFS62530
			if (e.PropertyName == "ReminderEnabled")
			{
				if (false	== this.Activity.ReminderEnabled	&&
					null	!= this.Activity.Reminder			&&
					true	== this.Activity.Reminder.IsSnoozed)
					this.Activity.Reminder.IsSnoozed = false;
			}

			this.UpdateDirtyStatus(true);
		}
		#endregion //OnActivityPropertyChanged

		#region OnCommittingActivity
		/// <summary>
		/// Called when the <see cref="ActivityBase"/> is about to be committed.
		/// </summary>
		protected virtual void OnCommittingActivity()
		{
			// Force the date pickers to parse any typing that is in progress.
			if (this._startDatePicker		!= null &&
				this._startDatePicker.Text	!= this._lastStartDatePickerText)
				this._endDatePicker.Focus();
			else
			if (this._endDatePicker			!= null &&
				this._endDatePicker.Text	!= this._lastStartDatePickerText)
				this._startDatePicker.Focus();

			// Force the time pickers to parse any typing that is in progress.
			this._timePickerManager.ForceTimePickersToUpdate();
		}
		#endregion //OnCommittingActivity

		#region PerformOnClosingCleanupTasks
		/// <summary>
		/// Called when the dialog is about to close.
		/// </summary>
		protected virtual void PerformOnClosingCleanupTasks()
		{
			// Unregister as a CommandTarget
			CommandSourceManager.UnregisterCommandTarget(this);

			// Remove tha appointment from the DataManager's activity/dialog map.
			this.DataManager.RemoveActivityFromDialogMap(this.ActivityOriginal);

			// Unhook our property changed event listener
			this.Activity.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);
			// SSP 7/17/12 TFS100159
			// 
			this.Activity.RemoveListener( this );

			// JM 9-29-11 TFS85588
			this.RaiseClosing();
		}
		#endregion //PerformOnClosingCleanupTasks

		#region ProcessRecurrenceDialogClosed
		/// <summary>
		/// Called after the <see cref="ActivityRecurrenceDialogCore"/> has closed.
		/// </summary>
		/// <param name="dialogResult"></param>
		protected virtual void ProcessRecurrenceDialogClosed(bool? dialogResult)
		{
			// JM 11-2-10 TFS58888 - Update the duration since that may have been modified in the Recurrence dialog
			this._timePickerManager.UpdateDuration(this.Activity.End.Subtract(this.Activity.Start), false);

			this._timePickerManager.SynchronizeStartTimePicker();
			this._timePickerManager.SynchronizeEndTimePicker();

			this._startDatePicker.SelectedDate		= this.StartAsLocal.Date;

			if (null != this._endDatePicker)
				this._endDatePicker.SelectedDate	= this.EndAsLocal.Date;

			this.UpdateConflictStatus();
		}
		#endregion //ProcessRecurrenceDialogClosed

		#endregion //Protected Virtual Methods

		#region Internal Methods

		#region Close
		internal void Close()
		{
			this.DialogElementProxy.Close();
		}
		#endregion //Close

		#region Delete
		internal void Delete()
		{
			// If this is a recurring task root and the connector only generates the current task occurrence, 
			// this actually represents the current occurrence, so we need to show the chooser dialog for it, but
			// only if it hasn't been completed yet. If it was completed, then the next occurrence was already
			// generated and we can just delete this activity.
			Task task = this.Activity as Task;
			if (task != null &&
				task.IsRecurrenceRoot && 
				task.PercentComplete < 100 &&
				this.DataManager.DataConnector.RecurringTaskGenerationBehavior == RecurringTaskGenerationBehavior.GenerateCurrentTask)
			{
				// Display the RecurrenceChooserDialog to let the user decide whether we should open the occurrence or the Series.
				var recurrenceChooserDialogResult	= new ActivityRecurrenceChooserDialog.ChooserResult(null);
				var closeHelper						= new DialogManager.CloseDialogHelper<ActivityRecurrenceChooserDialog.ChooserResult>(this.OnTaskRecurrenceChooserDialogClosed, recurrenceChooserDialogResult);

				bool result = this.DataManager.DisplayActivityRecurrenceChooserDialog(this,
															ScheduleUtilities.GetString("DLG_RecurrenceChooserDialog_Title_DeleteRecurringTask"),
															this.Activity,
															RecurrenceChooserType.ChooseForCurrentTaskDeletion,
															recurrenceChooserDialogResult,
															null,
															closeHelper.OnClosed);

				// Just return here - we will do the actual delete in the OnRecurrenceChooserDialogClosed
				// callback.
				return;
			}
			else
			{



				MessageBoxResult result = MessageBox.Show(this.GetLocalizedStringForCurrentActivityType("MSG_TEXT_DeleteAppointmentPrompt"), this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_DeleteAppointment"), MessageBoxButton.YesNo, MessageBoxImage.Question);


				switch (result)
				{
					case MessageBoxResult.Cancel:
					case MessageBoxResult.No:
						{
							break;
						}
					case MessageBoxResult.OK:
					case MessageBoxResult.Yes:
						{
							ActivityOperationResult operationResult = this.DataManager.Remove(this.ActivityOriginal);
							bool success = this.HandleOperationResult(operationResult, this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_DeleteAppointment"), false);

							if (success)
								this.Close();

							break;
						}
					default:
						break;
				}
			}
		}
		#endregion //Delete

		#region DisplayRecurrenceDialog
		internal void DisplayRecurrenceDialog()
		{
			this._isActivityRecurrenceDialogActive = true;

			this.DataManager.DisplayActivityRecurrenceDialog(this,
															  this.Activity,
															  ScheduleUtilities.GetString("DLG_Recurrence_Core_ActivityRecurrenceTitle"),
															  null,
															  this.OnRecurrenceDialogClosed,
															  this.IsActivityModifiable);
		}
		#endregion //DisplayRecurrenceDialog

		#region SaveAndClose
		internal void SaveAndClose()
		{
			if (false == this.DisplayDescriptionFormatWarningMessageIfNecessary(this.SaveAndCloseMessageBoxResultCallback))
			{
				this.CommitActivityHelper();
				this.Close();
			}
		}
		#endregion //SaveAndClose

		#region ShouldHideCategoriesButton
		internal bool ShouldHideCategoriesButton
		{
			get
			{
				IEnumerable<ActivityCategory>	categories		= this.DataManager.GetSupportedActivityCategories(this.Activity);
				bool							hideCategories	= 
					false == this.DataManager.AreCustomActivityCategoriesAllowed(this.Activity.OwningResource) &&
					(null == categories || false == categories.GetEnumerator().MoveNext());

				return hideCategories;
			}
		}
		#endregion //ShouldHideCategoriesButton

		#endregion //Internal Methods

		#region Private Methods

		#region CommitActivityHelper
		private bool CommitActivityHelper()
		{
			this.OnCommittingActivity();

			// Commit the activity.
			bool isAddNew					= this.Activity.IsAddNew;
			this._isInEndEdit				= true;
			ActivityOperationResult result	= this.DataManager.EndEdit(this.Activity);
			this._isInEndEdit				= false;

			bool success = this.HandleOperationResult(result, this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_UpdateAppointment"), isAddNew);

			return success;
		}
		#endregion //CommitActivityHelper

		#region DisplayDateConflictMessage
		private void DisplayDateConflictMessage()
		{
#pragma warning disable 436

			MessageBox.Show(SR.GetString("MSG_TEXT_AppointmentDateConflict"), SR.GetString("MSG_TITLE_AppointmentDateConflict"), MessageBoxButton.OK, MessageBoxImage.Exclamation);



#pragma warning restore 436
		}
		#endregion //DisplayDateConflictMessage

		#region DisplayDescriptionFormatWarningMessageIfNecessary
		// Displays a warning message about overlaying the formatting of the Description proeprty if necessarry.
		// Returns true if a dialog was necessary, false if not.
		private bool DisplayDescriptionFormatWarningMessageIfNecessary(MessageBoxResultDelegate callback)
		{
			if (this._descriptionModified && (this.Activity.OriginalDescriptionFormatResolved == DescriptionFormat.HTML ||
											  this.Activity.OriginalDescriptionFormatResolved == DescriptionFormat.Unknown)
										  && this.DataManager.IsClosingAllDialogs == false)	// JM 01-06-12 TFS95261 Also check whether we are forcing all dialogs closed
			{





				MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_DescriptionFormatOverwriteMessage"), ScheduleUtilities.GetString("MSG_TITLE_DescriptionFormatOverwriteMessage"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				this.Dispatcher.BeginInvoke(callback, new object [] {result});


				return true;
			}
			else
				return false;
		}
		#endregion //DisplayDescriptionFormatWarningMessageIfNecessary

		#region EvaluateActivityQueryResults
		private void EvaluateActivityQueryResults(ActivityQueryResult queryResult)
		{
			if (false == queryResult.IsComplete)
				return;

			if (true == queryResult.IsCanceled)
			{
				this.SetValue(HasConflictsPropertyKey, false);
				return;
			}

			if (this._queryResult.Activities.Count < 1)
			{
				this.SetValue(HasConflictsPropertyKey, false);
				return;
			}

			DateRange dateRange = new DateRange();
			dateRange.Start		= this.Activity.Start;
			dateRange.End		= this.Activity.End;

			bool hasConflict	= false;
			bool hasAdjacent	= false;

			foreach (ActivityBase activity in queryResult.Activities)
			{
				
				// he is returning different appointment instances for recurrences that are in fact the same.
				// Will use Id for now, but that is not a good permanent solution since the data source could
				// change the Id.
				//if (activity == this.ActivityOriginal)
				if (activity.Id == this.ActivityOriginal.Id)
					continue;

				if (hasConflict == false)
				{
					if ((dateRange.Contains(activity.Start) && activity.Start	!= dateRange.End) || 
						(dateRange.Contains(activity.End)	&& activity.End		!= dateRange.Start))
					{
						hasConflict = true;
						break;
					}
				}

				if (hasAdjacent == false && (dateRange.Start == activity.End || dateRange.End == activity.Start))
					hasAdjacent = true;
			}

			if (hasConflict == false && hasAdjacent == false)
				this.SetValue(HasConflictsPropertyKey, KnownBoxes.FalseBox);
			else
			{
				this.SetValue(HasConflictsPropertyKey, KnownBoxes.TrueBox);
#pragma warning disable 436
				if (hasConflict)
					this.SetValue(ConflictMessagePropertyKey, SR.GetString("DLG_Appointment_Core_AppointmentConflictMessage"));
				else
					this.SetValue(ConflictMessagePropertyKey, SR.GetString("DLG_Appointment_Core_AdjacentAppointmentConflictMessage"));
#pragma warning restore 436
			}
		}
		#endregion //EvaluateActivityQueryResults

		#region GetLocalizedStringForCurrentActivityType
		// This routine assumes that the provided name identifies the version of the string resource for
		// the Appointment Activity type, and that the name contains the literal 'Appointment'.  This routine
		// will modify the name by replacing the 'Appointment' literal with either 'Task' or 'Journal'
		// depending on the current activity type, and then get and return the localized string resource with that name.  
		private string GetLocalizedStringForCurrentActivityType(string name, params object[] args)
		{
			string nameOfStringForCurrentActivityType = name;

			if (this.Activity is Journal)
				nameOfStringForCurrentActivityType = nameOfStringForCurrentActivityType.Replace("Appointment", "Journal");
			else if (this.Activity is Task)
				nameOfStringForCurrentActivityType = nameOfStringForCurrentActivityType.Replace("Appointment", "Task");

			return ScheduleUtilities.GetString(nameOfStringForCurrentActivityType, args);
		}
		#endregion //GetLocalizedStringForCurrentActivityType

		#region HandleOperationResult
		private bool HandleOperationResult(OperationResult result, string errorMessageBoxTitle, bool isAddNew)
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
					this.DataManager.AddPendingOperation(result);

					// If this is a new appointment and we have a reference to the owning ScheduleControlBase, add it to the control's
					// ExtraActivities collection so it shows up in the control even though it has not yet been added to the back end.
					if (isAddNew && this.DataManager != null)
						this.DataManager.AddExtraActivity(this.ActivityOriginal);
				}
			}
			else if (errorInfo != null)	// Display a message on error.
			{
				this.DataManager.DisplayErrorMessageBox(errorInfo, errorMessageBoxTitle);
			}

			return success;
		}
		#endregion //HandleOperationResult

		#region HasDateConflicts






		private bool HasDateConflicts(bool displayMessageOnConflict)
		{
			bool hasConflict = this.Activity.Start > this.Activity.End;

			if (hasConflict && displayMessageOnConflict)
				this.DisplayDateConflictMessage();

			return hasConflict;
		}
		#endregion //HasDateConflicts

		#region InitializePrivate
		private void InitializePrivate()
		{
			if (true == this._initialized)
			{
				this._timePickerManager.DisconnectEventListeners();
				this._timePickerManager = null;

				if (null != this._startDatePicker)
				{
					this._startDatePicker.SelectedDateChanged	-= new EventHandler<SelectionChangedEventArgs>(StartDatePicker_SelectedDateChanged);
					this._startDatePicker.KeyDown				-= new KeyEventHandler(DatePicker_KeyDown);
					this._startDatePicker.LostFocus				-= new RoutedEventHandler(StartDatePicker_LostFocus);
				}

				if (null != this._endDatePicker)
				{
					this._endDatePicker.SelectedDateChanged		-= new EventHandler<SelectionChangedEventArgs>(EndDatePicker_SelectedDateChanged);
					this._endDatePicker.KeyDown					-= new KeyEventHandler(DatePicker_KeyDown);
					this._endDatePicker.LostFocus				-= new RoutedEventHandler(EndDatePicker_LostFocus);
				}

				this.Activity.PropertyChanged				-= new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);
				// SSP 7/17/12 TFS100159
				// 
				this.Activity.RemoveListener( this );
			}

			// Setup the TimePickerManager which takes care of managing the interaction between the time pickers.
			FrameworkElement feStartTimePicker	= this.GetTemplateChild(PartStartTimePicker) as FrameworkElement;
			FrameworkElement feEndTimePicker	= this.GetTemplateChild(PartEndTimePicker) as FrameworkElement;
			this._hasStartTimePicker			= feStartTimePicker != null;
			this._hasEndTimePicker				= feEndTimePicker != null;

			this._timePickerManager = new TimePickerManager(this,
															this.DataManager,
															this.Activity,
															this.GetTemplateChild(PartStartTimePicker) as FrameworkElement,
															this.GetTemplateChild(PartEndTimePicker) as FrameworkElement,
															this.GetTemplateChild(PartDurationTimePicker) as FrameworkElement);

			// Find the Start/End DatePickers
			this._startDatePicker						= this.GetTemplateChild(PartStartDatePicker) as DatePicker;
			this._endDatePicker							= this.GetTemplateChild(PartEndDatePicker) as DatePicker;

			// Initialize Start/End Date Pickers.
			if (null != this._startDatePicker)
				this._startDatePicker.SelectedDate		= this._lastStartDatePickerDate	= this.StartAsLocal;

			if (null != this._endDatePicker)
				this._endDatePicker.SelectedDate		= this._lastEndDatePickerDate	= this.EndAsLocal;

			// JM 12-9-10 TFS60567
			//this._lastStartDatePickerText = this._startDatePicker.Text;
			//this._lastEndDatePickerText					= this._endDatePicker.Text;
			this._lastStartDatePickerText				= this.StartAsLocal.ToShortDateString();
			this._lastEndDatePickerText					= this.EndAsLocal.ToShortDateString();

			if (null != this._startDatePicker)
			{
				this._startDatePicker.SelectedDateChanged	+= new EventHandler<SelectionChangedEventArgs>(StartDatePicker_SelectedDateChanged);
				this._startDatePicker.KeyDown				+= new KeyEventHandler(DatePicker_KeyDown);
				this._startDatePicker.LostFocus				+= new RoutedEventHandler(StartDatePicker_LostFocus);
			}

			if (null != this._endDatePicker)
			{
				this._endDatePicker.SelectedDateChanged		+= new EventHandler<SelectionChangedEventArgs>(EndDatePicker_SelectedDateChanged);
				this._endDatePicker.KeyDown					+= new KeyEventHandler(DatePicker_KeyDown);
				this._endDatePicker.LostFocus				+= new RoutedEventHandler(EndDatePicker_LostFocus);
			}

			this.VerifyTimePickerEnabledStatus();
			this.VerifyTimeZonePickerEnabledStatus();
			this.VerifyDescriptionFormat();

			// Listen to the Activity PropertyChanged event.
			this.Activity.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(OnActivityPropertyChanged);
			// SSP 7/17/12 TFS100159
			// In addition to listening for change notifications of the activity itself, we also need to
			// listen to any changes that happen inside the activity's metadata object, which is used to
			// bring in custom data into our object model.
			// 
			this.Activity.AddListener( this, true );

			// Force the CommandSourceManager to requery the status of our commands.
			this.UpdateCommandsStatus();

			// Check start/end dates and update conflict/adjacent status.
			this.UpdateConflictStatus();

			// Initialize the list of ActivityCategoryListItems
			this.ActivityCategoryListItems = this.ActivityCategoryHelper.SelectedCategoryListItems;
			this.VerifyActivityCategoryListItemsVisibility();

			this._initialized = true;

			// Call the virtual Initialize method to give base classes an opportunity to initialize.
			this.Initialize();
		}
		#endregion //InitializePrivate

		#region OnTaskRecurrenceChooserDialogClosed
		private void OnTaskRecurrenceChooserDialogClosed(bool? dialogResult, ActivityRecurrenceChooserDialog.ChooserResult recurrenceChooserDialogResult)
		{
			if (true == dialogResult)
			{
				if (recurrenceChooserDialogResult.Choice == RecurrenceChooserChoice.None)
					return;

				var activity			= recurrenceChooserDialogResult.Choice == RecurrenceChooserChoice.Series
					? this.ActivityOriginal
					: ScheduleUtilities.CreateTaskOccurrenceToBeDeleted(this.Activity as Task);

				ActivityOperationResult operationResult = this.DataManager.Remove(activity);
				bool					success			= this.HandleOperationResult(operationResult, ScheduleUtilities.GetString("MSG_TITLE_DeleteTask"), false);

				if (success)
					this.Close();
			}
		}
		#endregion //OnTaskRecurrenceChooserDialogClosed

		#region OnRecurrenceDialogClosed
		private void OnRecurrenceDialogClosed(bool? dialogResult)
		{
			this._isActivityRecurrenceDialogActive = false;

			if (true == dialogResult)
			{
				this.ProcessRecurrenceDialogClosed(dialogResult);

				this.VerifyRecurrenceStatus();
				this.UpdateDirtyStatus(true);
			}
		}
		#endregion //OnRecurrenceDialogClosed

		#region RevertEndDatePicker
		internal delegate void RevertEndDatePickerDelegate(bool displayDateConflictMessage);	// JM 12-2-10 TFS60567
		private void RevertEndDatePicker(bool displayDateConflictMessage)
		{
			// JM 12-2-10 TFS60567 - Refactor this method.

			this.RevertEndDatePickerOperationsPending.Pop();



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			if (displayDateConflictMessage)
				this.DisplayDateConflictMessage();

			// Set the DatePicker's Text property rather than its SelectedDate property since the DatePicker does a
			// BeginInvoke when the SelectedDate is set which messes us up.
			if (null != this._endDatePicker)
				this._endDatePicker.Text = this._lastEndDatePickerText;

			this.Activity.SetEndLocal(this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved, this._lastEndDatePickerDate);
		}
		#endregion //RevertEndDatePicker

		#region RevertTimeZoneIds
		private void RevertTimeZoneIds()
		{
			// Revert the start/end time zone ids to their previous values
			if (this.Activity.StartTimeZoneId != this._lastStartTimeZoneId)
			{
				this.Activity.StartTimeZoneId	= this._lastStartTimeZoneId;

				DateTime oldStartAsLocal			= this.StartAsLocal;
				this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
				this.Activity.Start				= this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved.ConvertToUtc(oldStartAsLocal);
				this._timePickerManager.SynchronizeStartTimePicker();
			}

			if (this.Activity.EndTimeZoneId != this._lastEndTimeZoneId)
			{
				this.Activity.EndTimeZoneId	= this._lastEndTimeZoneId;

				DateTime oldEndAsLocal			= this.EndAsLocal;
				this.TimeZoneTokenHelper.VerifyTimeZoneTokens();
				this.Activity.End			= this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved.ConvertToUtc(oldEndAsLocal);
				this._timePickerManager.SynchronizeEndTimePicker();
			}

			this._revertTimeZoneOperationPending = null;
		}
		#endregion //RevertTimeZoneIds

		#region UpdateConflictStatus
		private void UpdateConflictStatus()
		{
			DateRange dateRange = new DateRange();
			dateRange.Start		= this.Activity.Start.Subtract(TimeSpan.FromHours(1));
			dateRange.End		= this.Activity.End.Add(TimeSpan.FromHours(1));


			// JM 11-1-10 TFS 58839
			this.DataManager.EnsureOwningCalendarInitialized(this.Activity);

			var query			= new ActivityQuery(ActivityTypes.Appointment | ActivityTypes.Task, new DateRange [] { dateRange }, new ResourceCalendar [] { this.Activity.OwningCalendar });
			_queryResult		= this.DataManager.GetActivities(query);
			if (this._queryResult.IsComplete == true)
				this.EvaluateActivityQueryResults(this._queryResult);
			else
				ScheduleUtilities.AddListener(_queryResult, this, true);
		}
		#endregion //UpdateConflictStatus

		#region VerifyActivityCategoryListItemsVisibility
		private void VerifyActivityCategoryListItemsVisibility()
		{
			if (string.IsNullOrEmpty(this.Activity.Categories))
				this.ActivityCategoryListItemsVisibility = Visibility.Collapsed;
			else
				this.ActivityCategoryListItemsVisibility = Visibility.Visible;
		}
		#endregion //VerifyActivityCategoryListItemsVisibility

		#region VerifyDescriptionFormat
		// Verifies the original format of the ActivityBase.Description property and sets the DescriptionFormatWarningMessage properties accordingly for display in the View.
		private void VerifyDescriptionFormat()
		{
			if (false == this._descriptionModified)
			{
				this.DescriptionFormatWarningMessageVisibility = Visibility.Collapsed;
				return;
			}

			string descriptionFormatWarningMessage = string.Empty;

			switch (this.Activity.OriginalDescriptionFormatResolved)
			{
				case DescriptionFormat.Unknown:
					{
						descriptionFormatWarningMessage = ScheduleUtilities.GetString("DLG_Activity_Core_DescriptionFormatOverwritePossible");
						break;
					}
				case DescriptionFormat.HTML:
					{
						descriptionFormatWarningMessage = ScheduleUtilities.GetString("DLG_Activity_Core_DescriptionFormatOverwriteDefinite");
						break;
					}
			}

			if (true == string.IsNullOrEmpty(descriptionFormatWarningMessage))
				this.DescriptionFormatWarningMessageVisibility = Visibility.Collapsed;
			else
			{
				this.DescriptionFormatWarningMessage			= descriptionFormatWarningMessage;
				this.DescriptionFormatWarningMessageVisibility	= Visibility.Visible;
			}
		}
		#endregion //VerifyDescriptionFormat

		#region VerifyTimePickerEnabledStatus
		private void VerifyTimePickerEnabledStatus()
		{
			bool enableTimePickers = (this.Activity.IsTimeZoneNeutral == false && this.IsActivityModifiable == true);

			this._timePickerManager.SetStartTimePickerEnabledStatus(enableTimePickers);
			this._timePickerManager.SetEndTimePickerEnabledStatus(enableTimePickers);
		}
		#endregion //VerifyTimePickerEnabledStatus

		#region VerifyTimeZonePickerEnabledStatus
		private void VerifyTimeZonePickerEnabledStatus()
		{
			this.TimeZonePickerEnabled = (this.Activity.IsTimeZoneNeutral == false && this.IsActivityModifiable == true);
		}
		#endregion //VerifyTimeZonePickerEnabledStatus

		#endregion //Private Methods

		#endregion  //Methods

		#region Event Handlers

		#region CloseWithWarningMessageBoxResultCallback
		private void CloseWithWarningMessageBoxResultCallback(MessageBoxResult result)
		{
			switch (result)
			{
				case MessageBoxResult.Cancel:
					{
						// Force the CommandSourceManager to requery the status of our commands.
						this.UpdateCommandsStatus();

						return;
					}
				case MessageBoxResult.No:
					{
						DataErrorInfo errorInfo;
						this.DataManager.CancelEdit(this.Activity, out errorInfo);

						break;
					}
				case MessageBoxResult.OK:
				case MessageBoxResult.Yes:
					{
						this.CommitActivityHelper();
						break;
					}
				default:
					break;
			}

			this.PerformOnClosingCleanupTasks();

			this._acceptClosingWithNoProcessing = true;
			this.DialogElementProxy.Close();
			this._acceptClosingWithNoProcessing = false;
		}
		#endregion //CloseWithWarningMessageBoxResultCallback

		#region DialogClosingMessageBoxResultCallback
		private void DialogClosingMessageBoxResultCallback(MessageBoxResult result)
		{
			switch (result)
			{
				case MessageBoxResult.Cancel:
					{
						// Force the CommandSourceManager to requery the status of our commands.
						this.UpdateCommandsStatus();

						return;
					}
				case MessageBoxResult.No:
					{
						DataErrorInfo errorInfo;
						this.DataManager.CancelEdit(this.Activity, out errorInfo);

						break;
					}
				case MessageBoxResult.OK:
				case MessageBoxResult.Yes:
					{
						if (false == this.DisplayDescriptionFormatWarningMessageIfNecessary(this.CloseWithWarningMessageBoxResultCallback))
						{
							this.CommitActivityHelper();
							break;
						}
						else
							return;
					}
				default:
					break;
			}

			this.PerformOnClosingCleanupTasks();

			this._acceptClosingWithNoProcessing = true;
			this.DialogElementProxy.Close();
			this._acceptClosingWithNoProcessing = false;
		}
		#endregion //DialogClosingMessageBoxResultCallback

		#region DatePicker_KeyDown
		void DatePicker_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Tab)
				this.UpdateDirtyStatus(true);
		}
		#endregion //DatePicker_KeyDown

		#region EndDatePicker_LostFocus
		void EndDatePicker_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this._endDatePicker.SelectedDate == null)
				this._endDatePicker.SelectedDate = this._lastEndDatePickerDate;
		}
		#endregion //EndDatePicker_LostFocus

		#region EndDatePicker_SelectedDateChanged
		void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			if (false == this._ignoreNextEndDatePickerSelectionChange)
			{
				if (this.RevertEndDatePickerOperationsPending.Count < 1)
				{
					if (this._endDatePicker.SelectedDate.HasValue == false)
					{
						this.RevertEndDatePickerOperationsPending.Push(this.Dispatcher.BeginInvoke(new RevertEndDatePickerDelegate(this.RevertEndDatePicker), false));
					}
					else
					{
						DateTime endTime = TimePickerManager.GetTimePickerCurrentTime(this._timePickerManager.EndTimePickerProxy).HasValue ?
							TimePickerManager.GetTimePickerCurrentTime(this._timePickerManager.EndTimePickerProxy).Value : new DateTime(1, 1, 1, 0, 0, 0);
						this.Activity.SetEndLocal(this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved,
													  ScheduleUtilities.CombineDateAndTime(this._endDatePicker.SelectedDate.Value, endTime));

						// JM 12-2-10 TFS60567 - Delay displaying the message until after the BeginInvoke so we don't interfere with the DatePicker
						//if (this.HasDateConflicts(true))
						if (this.HasDateConflicts(false))
						{

							this._endDatePicker.IsDropDownOpen = false;	// JM 12-2-10 TFS60567

							this.RevertEndDatePickerOperationsPending.Push(this.Dispatcher.BeginInvoke(new RevertEndDatePickerDelegate(this.RevertEndDatePicker), true));
						}
						else
						{
							this._lastEndDatePickerText = this._endDatePicker.Text;
							this._lastEndDatePickerDate = this.EndAsLocal;
							this._timePickerManager.UpdateDuration(this.Activity.End.Subtract(this.Activity.Start), false);
							this._timePickerManager.SynchronizeEndTimePicker();
						}
					}
				}
			}
			else
				this._ignoreNextEndDatePickerSelectionChange = false;

			// JM 12-2-10 TFS60400
			this.UpdateConflictStatus();
		}
		#endregion //EndDatePicker_SelectedDateChanged

		#region SaveAndCloseMessageBoxResultCallback
		private void SaveAndCloseMessageBoxResultCallback(MessageBoxResult result)
		{
			switch (result)
			{
				case MessageBoxResult.Cancel:
					{
						// Force the CommandSourceManager to requery the status of our commands.
						this.UpdateCommandsStatus();

						return;
					}
				case MessageBoxResult.No:
					{
						DataErrorInfo errorInfo;
						this.DataManager.CancelEdit(this.Activity, out errorInfo);

						break;
					}
				case MessageBoxResult.OK:
				case MessageBoxResult.Yes:
					{
						this.CommitActivityHelper();

						break;
					}
				default:
					break;
			}

			this.PerformOnClosingCleanupTasks();

			this._acceptClosingWithNoProcessing = true;
			this.DialogElementProxy.Close();
			this._acceptClosingWithNoProcessing = false;
		}
		#endregion //SaveAndCloseMessageBoxResultCallback

		#region StartDatePicker_LostFocus
		void StartDatePicker_LostFocus(object sender, RoutedEventArgs e)
		{
			if (this._startDatePicker.SelectedDate == null)
				this._startDatePicker.SelectedDate = this._lastStartDatePickerDate;
		}
		#endregion //StartDatePicker_LostFocus

		#region StartDatePicker_SelectedDateChanged
		void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


			// Can't proceed if we have null dates.
			if (this._startDatePicker.SelectedDate.HasValue							== false ||
				TimePickerManager.GetTimePickerCurrentTime(this._timePickerManager.StartTimePickerProxy).HasValue == false)
				return;

			this.Activity.SetStartLocal(this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved,
											ScheduleUtilities.CombineDateAndTime(this._startDatePicker.SelectedDate.Value, TimePickerManager.GetTimePickerCurrentTime(this._timePickerManager.StartTimePickerProxy).Value));
			this._lastStartDatePickerText	= this._startDatePicker.Text;
			this._lastStartDatePickerDate	= this.StartAsLocal;
			this._timePickerManager.SynchronizeStartTimePicker();

			// Update the End date as appropriate based on the new Start date and the duration.
			this.Activity.End				= this.Activity.Start.Add(this._timePickerManager.Duration);
			if (null != this._endDatePicker)
			{
				this._endDatePicker.SelectedDate = this.EndAsLocal;
				this._timePickerManager.SynchronizeEndTimePicker();
			}

			// JM 12-2-10 TFS60400
			this.UpdateConflictStatus();
		}
		#endregion //StartDatePicker_SelectedDateChanged

		#endregion //EventHandlers

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is ActivityDialogCoreCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			// If a parameter has been specified in the CommandSource, make sure it is a reference to this instance 
			// of ActivityDialogCore.
			CommandBase commandBase = command as CommandBase;
			if (null != commandBase					&& 
				null != commandBase.CommandSource	&&
				null != commandBase.CommandSource.Parameter)
				return commandBase.CommandSource.Parameter == this;

			return command is ActivityDialogCoreCommandBase;
		}

		#endregion

		#region IDialogElementProxyHost Members
		private delegate void MessageBoxResultDelegate(MessageBoxResult result);
		bool IDialogElementProxyHost.OnClosing()
		{
			if (this._acceptClosingWithNoProcessing)
				return false;

			if (this.IsDirty && this.DataManager.IsClosingAllDialogs == false)	// JM 01-06-12 TFS95261 Also check whether we are forcing all dialogs closed
			{





				MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_SavePrompt"), this.IsNew ? this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_AddAppointment") : this.GetLocalizedStringForCurrentActivityType("MSG_TITLE_UpdateAppointment"), MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				this.Dispatcher.BeginInvoke(new MessageBoxResultDelegate(this.DialogClosingMessageBoxResultCallback), new object [] {result});


				// Cancel the closing - we'll close when we receive the callback.
				return true;
			}

			DataErrorInfo errorInfo;
			this.DataManager.CancelEdit(this.Activity, out errorInfo);

			this.PerformOnClosingCleanupTasks();
			return false;
		}
		#endregion

		#region Events

		// JM 9-29-11 TFS85588 Added.
		#region Closing
		internal event System.EventHandler Closing;
		private void RaiseClosing()
		{
			if (Closing != null)
				Closing(this, EventArgs.Empty);
		}
		#endregion //Closing

		#endregion //Events

		#region ITypedPropertyChangeListener<object,string> Members
		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			// SSP 7/17/12 TFS100159
			// 
			if ( null != _activity && dataItem == _activity.Metadata )
			{
				this.UpdateDirtyStatus( true );
				return;
			}

			// SSP 7/17/12 TFS100159
			// Now we also listen into the activity so check for data item being query result.
			// 
			//if (property == "IsComplete")
			if ( _queryResult == dataItem && property == "IsComplete" )
				this.EvaluateActivityQueryResults(this._queryResult);
		}
		#endregion //ITypedPropertyChangeListener<object,string> Members

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
			get { return this.TimeZoneTokenHelper.ActivityStartTimeZoneTokenResolved; }
		}

		TimeZoneToken ITimePickerManagerHost.ActivityEndTimeZoneTokenResolved
		{
			get { return this.TimeZoneTokenHelper.ActivityEndTimeZoneTokenResolved; }
		}

		bool ITimePickerManagerHost.IsOkToParseEndTimePickerText
		{
			get { return this._revertEndDatePickerOperationsPending == null || this._revertEndDatePickerOperationsPending.Count < 1; }
		}

		void ITimePickerManagerHost.UpdateDirtyStatus(bool isDirty)
		{
			this.UpdateDirtyStatus(isDirty);
		}

		void ITimePickerManagerHost.UpdateConflictStatus()
		{
			this.UpdateConflictStatus();
		}

		bool ITimePickerManagerHost.HasDateConflicts(bool displayMessageOnConflict)
		{
			return this.HasDateConflicts(displayMessageOnConflict);
		}

		void ITimePickerManagerHost.UpdateStartDatePickerDateFromNewStartTime(DateTime newStartTime)
		{
			if (this._startDatePicker.SelectedDate.Value.Date != newStartTime.Date)
				this._startDatePicker.SelectedDate = newStartTime.Date;
		}

		void ITimePickerManagerHost.UpdateEndDatePickerDateFromNewEndTime(DateTime newEndTime)
		{
			if (null != this._endDatePicker)
			{
				if (this._endDatePicker.SelectedDate.Value.Date != newEndTime.Date)
					this._endDatePicker.SelectedDate = newEndTime.Date;
			}
		}
		#endregion //ITimePickerManagerHost Members
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