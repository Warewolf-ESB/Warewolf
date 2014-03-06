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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Resources;


using Infragistics.Windows.Controls;


namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Displays a UI for currently expired reminders.
	/// </summary>
	[TemplatePart(Name = PartReminderListBox, Type = typeof(ListBox))]



	[TemplatePart(Name = PartSnoozePicker, Type = typeof(ComboBox))]

	[DesignTimeVisible(false)]
	public class ReminderDialog : Control,
								  IDialogElementProxyHost,
								  ICommandTarget,
								  IPropertyChangeListener	// JM 03-31-11 TFS62936
	{
		#region Member Variables

		// Template part names
		private const string PartReminderListBox			= "ReminderListBox";
		private const string PartSnoozePicker				= "SnoozePicker";

		private DialogElementProxy								_dialogElementProxy;
		private bool											_initialized;
		private Dictionary<string, string>						_localizedStrings;
		private XamScheduleDataManager							_dataManager;
		private ObservableCollection<ReminderDialogListItem>	_reminderItems;
		private ListBox											_reminderListBox;
		private ComboBoxProxy									_snoozePickerProxy;
		private TimeZoneTokenHelper								_timeZoneTokenHelper;

		private BitmapImage										_appointmentImageSource;
		private BitmapImage										_taskImageSource;
		private BitmapImage										_journalImageSource;

		#endregion //Member Variables

		#region Constructors
		static ReminderDialog()
		{

			ReminderDialog.DefaultStyleKeyProperty.OverrideMetadata(typeof(ReminderDialog), new FrameworkPropertyMetadata(typeof(ReminderDialog)));

		}

		/// <summary>
		/// Creates an instance of the Reminder dialog.
		/// </summary>
		/// <param name="dataManager">The XamScheduleDataManager associated with the reminders displayedin the dialog.</param>
		public ReminderDialog(XamScheduleDataManager dataManager)
		{




			CoreUtilities.ValidateNotNull(dataManager, "datamanager");

			this._timeZoneTokenHelper	= new TimeZoneTokenHelper(null, dataManager);

			this._dataManager			= dataManager;

			// JM 04-29-11 TFS74043 Move this to the Initialize method.
			//this._dataManager.ActiveReminders.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnDataManagerActiveRemindersCollectionChanged);

			// SSP 5/12/11 TFS75043
			// Moved this into the Initialize method.
			// 
			//this.InitializeListOfReminderItems();
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
				this.Close(false);
		}
		#endregion //OnKeyUp

		#endregion //Base Class Overrides

		#region Properties

		#region Public Properties

		#region DataManager
		/// <summary>
		/// Returns the <see cref="XamScheduleDataManager"></see> associated with the dialog./>
		/// </summary>
		public XamScheduleDataManager DataManager
		{
			get { return this._dataManager; }
		}
		#endregion //DataManager

		#region IsSnoozePickerEnabled

		private static readonly DependencyPropertyKey IsSnoozePickerEnabledPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSnoozePickerEnabled",
			typeof(bool), typeof(ReminderDialog), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsSnoozePickerEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSnoozePickerEnabledProperty = IsSnoozePickerEnabledPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the subject for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="IsSnoozePickerEnabledProperty"/>
		public bool IsSnoozePickerEnabled
		{
			get
			{
				return (bool)this.GetValue(ReminderDialog.IsSnoozePickerEnabledProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.IsSnoozePickerEnabledPropertyKey, value);
			}
		}

		#endregion //IsSnoozePickerEnabled

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

#pragma warning disable 436
					this._localizedStrings.Add("DLG_Reminder_Literal_Subject", SR.GetString("DLG_Reminder_Literal_Subject"));
					this._localizedStrings.Add("DLG_Reminder_Literal_DueIn", SR.GetString("DLG_Reminder_Literal_DueIn"));
					this._localizedStrings.Add("DLG_Reminder_Literal_DismissAll", SR.GetString("DLG_Reminder_Literal_DismissAll"));
					this._localizedStrings.Add("DLG_Reminder_Literal_OpenItem", SR.GetString("DLG_Reminder_Literal_OpenItem"));
					this._localizedStrings.Add("DLG_Reminder_Literal_Dismiss", SR.GetString("DLG_Reminder_Literal_Dismiss"));
					this._localizedStrings.Add("DLG_Reminder_Literal_SnoozePrompt", SR.GetString("DLG_Reminder_Literal_SnoozePrompt"));
					this._localizedStrings.Add("DLG_Reminder_Literal_Snooze", SR.GetString("DLG_Reminder_Literal_Snooze"));
#pragma warning restore 436
				}

				return this._localizedStrings;
			}
		}
		#endregion //LocalizedStrings

		#region ReminderItems
		/// <summary>
		/// Returns an ObservableCollection
		/// </summary>
		public ObservableCollection<ReminderDialogListItem> ReminderItems
		{
			get
			{
				if (this._reminderItems == null)
					this._reminderItems = new ObservableCollection<ReminderDialogListItem>();

				return this._reminderItems;
			}
		}
		#endregion //ReminderItems

		#region SelectedActivityImageSource

		private static readonly DependencyPropertyKey SelectedActivityImageSourcePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivityImageSource",
			typeof(ImageSource), typeof(ReminderDialog), (ImageSource)null, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivityImageSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivityImageSourceProperty = SelectedActivityImageSourcePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the ImageSource of the image for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="SelectedActivityImageSourceProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ActivityBase"/>
		public ImageSource SelectedActivityImageSource
		{
			get
			{
				return (ImageSource)this.GetValue(ReminderDialog.SelectedActivityImageSourceProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.SelectedActivityImageSourcePropertyKey, value);
			}
		}

		#endregion //SelectedActivityImageSource

		#region SelectedActivityImageVisibility

		private static readonly DependencyPropertyKey SelectedActivityImageVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivityImageVisibility",
			typeof(Visibility), typeof(ReminderDialog), Visibility.Collapsed, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivityImageVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivityImageVisibilityProperty = SelectedActivityImageVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the image for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="SelectedActivityImageVisibilityProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ActivityBase"/>
		public Visibility SelectedActivityImageVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ReminderDialog.SelectedActivityImageVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.SelectedActivityImageVisibilityPropertyKey, value);
			}
		}

		#endregion //SelectedActivityImageVisibility

		#region SelectedActivitySubject

		private static readonly DependencyPropertyKey SelectedActivitySubjectPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivitySubject",
			typeof(string), typeof(ReminderDialog), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivitySubject"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivitySubjectProperty = SelectedActivitySubjectPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the subject for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="SelectedActivitySubjectProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ActivityBase"/>
		public string SelectedActivitySubject
		{
			get
			{
				return (string)this.GetValue(ReminderDialog.SelectedActivitySubjectProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.SelectedActivitySubjectPropertyKey, value);
			}
		}

		#endregion //SelectedActivitySubject

		#region SelectedActivityStartTimeDescription

		private static readonly DependencyPropertyKey SelectedActivityStartTimeDescriptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivityStartTimeDescription",
			typeof(string), typeof(ReminderDialog), string.Empty, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivityStartTimeDescription"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivityStartTimeDescriptionProperty = SelectedActivityStartTimeDescriptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a string that describes the start time for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="SelectedActivityStartTimeDescriptionProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ActivityBase"/>
		public string SelectedActivityStartTimeDescription
		{
			get
			{
				return (string)this.GetValue(ReminderDialog.SelectedActivityStartTimeDescriptionProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.SelectedActivityStartTimeDescriptionPropertyKey, value);
			}
		}

		#endregion //SelectedActivityStartTimeDescription

		#region SelectedActivitySubjectVisibility

		private static readonly DependencyPropertyKey SelectedActivitySubjectVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("SelectedActivitySubjectVisibility",
			typeof(Visibility), typeof(ReminderDialog), Visibility.Collapsed, null);

		/// <summary>
		/// Identifies the read-only <see cref="SelectedActivitySubjectVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedActivitySubjectVisibilityProperty = SelectedActivitySubjectVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the subject for the currently selected <see cref="ActivityBase"/>
		/// </summary>
		/// <seealso cref="SelectedActivitySubjectVisibilityProperty"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="ActivityBase"/>
		public Visibility SelectedActivitySubjectVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ReminderDialog.SelectedActivitySubjectVisibilityProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialog.SelectedActivitySubjectVisibilityPropertyKey, value);
			}
		}

		#endregion //SelectedActivitySubjectVisibility

		#endregion //Public Properties

		#region Private Properties

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

		#region SnoozeInterval
		internal TimeSpan SnoozeInterval
		{
			get;
			private set;
		}
		#endregion //SnoozeInterval

		#endregion //Private Properties

		#region Internal Properties

		#region AppointmentImageSource
		internal BitmapImage AppointmentImageSource
		{
			get
			{
				if (null == this._appointmentImageSource)
				{



					this._appointmentImageSource = this.GetBitmapImage("Images/Calendar_16x16.png");

				}

				return this._appointmentImageSource;
			}
		}
		#endregion //AppointmentImageSource

		#region JournalImageSource
		internal BitmapImage JournalImageSource
		{
			get
			{
				if (null == this._journalImageSource)



					this._journalImageSource = this.GetBitmapImage("Images/Journal_16x16.png");


				return this._journalImageSource;
			}
		}
		#endregion //JournalImageSource

		#region TaskImageSource
		internal BitmapImage TaskImageSource
		{
			get
			{
				if (null == this._taskImageSource)



					this._taskImageSource = this.GetBitmapImage("Images/Task_16x16.png");


				return this._taskImageSource;
			}
		}
		#endregion //TaskImageSource

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region CanExecuteCommand
		internal bool CanExecuteCommand(ReminderDialogCommand command)
		{
			switch (command)
			{
				case ReminderDialogCommand.DismissAll:
				case ReminderDialogCommand.DismissSelected:
				case ReminderDialogCommand.SnoozeSelected:
				case ReminderDialogCommand.OpenSelected:
					return this._reminderListBox.SelectedItems.Count > 0;
				default: 
					return false;
			}

		}
		#endregion //CanExecuteCommand

		#region ExecuteCommand
		internal void ExecuteCommand(ReminderDialogCommand command)
		{
			switch (command)
			{
				case ReminderDialogCommand.DismissAll:
					{
						// JM 03-02-11 TFS63065 - Prompt for confirmation.





						MessageBoxResult result = MessageBox.Show(ScheduleUtilities.GetString("MSG_TEXT_DismissAllReminders"), ScheduleUtilities.GetString("MSG_TITLE_DismissAllReminders"), MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
						if (result == MessageBoxResult.No)
							return;


						this.ProcessItems(Scope.All, Action.Dismiss);
						break;
					}
				case ReminderDialogCommand.OpenSelected:
					this.ProcessItems(Scope.Selected, Action.Display);
					break;
				case ReminderDialogCommand.DismissSelected:
					this.ProcessItems(Scope.Selected, Action.Dismiss);
					break;
				case ReminderDialogCommand.SnoozeSelected:
					this.ProcessItems(Scope.Selected, Action.Snooze);
					break;
			}
		}
		#endregion //ExecuteCommand

		#endregion //InternalMethods

		#region	Private Methods

		#region AddReminderInfoToReminderList
		private void AddReminderInfoToReminderList(ReminderInfo reminderInfo)
		{
			if (reminderInfo.IsSnoozed || reminderInfo.IsDismissed)
				return;

			ActivityBase activity = reminderInfo.Context as ActivityBase;
			if (null != activity)
			{
				ReminderDialogListItem	rdli = new ReminderDialogListItem(this, reminderInfo);
				this.ReminderItems.Add(rdli);
				this.UpdateDueInDescription(rdli);
			}
			else
				Debug.Assert(false, "Encountered a reminder with a context that is not of type ActivityBase!");
		}
		#endregion //AddReminderInfoToReminderList

		#region Close
		private void Close(bool result)
		{


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

			this.SetDialogResult(result);

		}
		#endregion //Close


		#region GetBitmapImage
		private BitmapImage GetBitmapImage(string resourceName)
		{
			BitmapImage bi = new BitmapImage();
			bi.BeginInit();

			Uri					uri		= CoreUtilities.BuildEmbeddedResourceUri(typeof(ReminderDialog).Assembly, resourceName);
			StreamResourceInfo	info	= Application.GetResourceStream(uri);
			if (info != null)
				bi.StreamSource = info.Stream;
			else
				Debug.Assert(false, "Could not get StreamResourceInfo for resource Uri!!");

			bi.EndInit();

			return bi;
		}
		#endregion //GetBitmapImage


		// JM 02-22-11 TFS63063 Added.
		#region GetSelectedItemWithEarliestStartDate
		private ReminderDialogListItem GetSelectedItemWithEarliestStartDate()
		{
			if (null == this._reminderListBox || this._reminderListBox.SelectedItems.Count < 1)
				return null;

			ReminderDialogListItem earliestItem = null;
			for (int i = 0; i < this._reminderListBox.SelectedItems.Count; i++)
			{
				ReminderDialogListItem	currentItem = this._reminderListBox.SelectedItems[i] as ReminderDialogListItem;
				DateTime				startTime	= currentItem.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken);
				if (null == earliestItem)
					earliestItem = currentItem;
				else
				if (currentItem.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken) < earliestItem.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken))
					earliestItem = currentItem;
			}

			return earliestItem;
		}
		#endregion //GetSelectedItemWithEarliestStartDate

		#region Initialize
		private void Initialize()
		{
			if (true == this._initialized)
			{
				this._snoozePickerProxy.LostFocus		-= new ComboBoxProxy.ProxyLostFocusEventHandler(_snoozePicker_LostFocus);
				this._snoozePickerProxy.DropDownClosed	-= new ComboBoxProxy.ProxyDropDownClosedEventHandler(_snoozePicker_DropDownClosed);
				this._snoozePickerProxy.DropDownOpened	-= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(_snoozePicker_DropDownOpened);
				this._snoozePickerProxy.KeyUp			-= new KeyEventHandler(_snoozePicker_KeyUp);

				// JM 04-29-11 TFS74043 
				this._dataManager.ActiveReminders.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnDataManagerActiveRemindersCollectionChanged);
			}


			// Find the parts we need.
			//
			// SnoozePicker combobox
			FrameworkElement fe = this.GetTemplateChild(PartSnoozePicker) as FrameworkElement;
			if (fe != null)
			{
				this._snoozePickerProxy					= new ComboBoxProxy(fe);
				// JM 02-22-11 TFS63063 - initialize the SnoozePicker in _reminderListBox_SelectionChanged  
				//this._snoozePickerProxy.ItemsSource		= SnoozeListItem.GetSnoozeListItems();
				this._snoozePickerProxy.LostFocus		+= new ComboBoxProxy.ProxyLostFocusEventHandler(_snoozePicker_LostFocus);
				this._snoozePickerProxy.DropDownClosed	+= new ComboBoxProxy.ProxyDropDownClosedEventHandler(_snoozePicker_DropDownClosed);
				this._snoozePickerProxy.DropDownOpened	+= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(_snoozePicker_DropDownOpened);
				this._snoozePickerProxy.KeyUp			+= new KeyEventHandler(_snoozePicker_KeyUp);

				// JM 02-22-11 TFS63063 - initialize the SnoozePicker in _reminderListBox_SelectionChanged  
				//this._snoozePickerProxy.Text = SnoozeListItem.SnoozeStringFromTimeSpan(TimeSpan.FromMinutes(5));

				// JM 02-22-11 TFS63063 - initialize the SnoozePicker in _reminderListBox_SelectionChanged  
				// JM 11-5-10 Force the SnoozeInterval to get initialized.
				//this.ParseSnoozePickerText();
			}

			// Initialize the start time description.
#pragma warning disable 436
			this.SelectedActivityStartTimeDescription = SR.GetString("DLG_Reminder_Literal_RemindersSelected", "0");
#pragma warning restore 436

			// ReminderListBox
			this._reminderListBox = this.GetTemplateChild(PartReminderListBox) as ListBox;

			// AS 5/13/11 TFS75487
			// Moved up before the reminder list box is initialized.
			//
			// SSP 5/12/11 TFS75043
			// Moved this here from the constructor.
			// 
			this.InitializeListOfReminderItems();

			if (this._reminderListBox != null)
			{
				this._reminderListBox.SelectionChanged += new SelectionChangedEventHandler(_reminderListBox_SelectionChanged);

				if (this._reminderListBox.Items.Count > 0)
					this._reminderListBox.SelectedIndex = 0;
			}

			// AS 5/13/11 TFS75487
			// Moved up.
			//
			//// SSP 5/12/11 TFS75043
			//// Moved this here from the constructor.
			//// 
			//this.InitializeListOfReminderItems( );

			// JM 04-29-11 TFS74043 Move here form the constructor
			this._dataManager.ActiveReminders.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnDataManagerActiveRemindersCollectionChanged);

			this._initialized = true;
		}
		#endregion //Initialize

		#region InitializeListOfReminderItems
		private void InitializeListOfReminderItems()
		{
			foreach (ReminderInfo reminderInfo in this._dataManager.ActiveReminders)
			{
				this.AddReminderInfoToReminderList(reminderInfo);
			}
		}
		#endregion //InitializeListOfReminderItems

		#region OnDataManagerActiveRemindersCollectionChanged
		void OnDataManagerActiveRemindersCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action)
			{
				case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
					{
						foreach (object o in e.NewItems)
						{
							ReminderInfo reminderInfo = o as ReminderInfo;
							if (null != reminderInfo)
								this.AddReminderInfoToReminderList(reminderInfo);
						}

						break;
					}
				case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
					{
						foreach (object o in e.OldItems)
						{
							ReminderInfo reminderInfo = o as ReminderInfo;
							if (null != reminderInfo)
								this.RemoveReminderInfoFromReminderList(reminderInfo);
						}

						break;
					}
				case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
					{
						// Remove the old items.
						foreach (object o in e.OldItems)
						{
							ReminderInfo reminderInfo = o as ReminderInfo;
							if (null != reminderInfo)
								this.RemoveReminderInfoFromReminderList(reminderInfo);
						}

						// Add the new items.
						foreach (object o in e.NewItems)
						{
							ReminderInfo reminderInfo = o as ReminderInfo;
							if (null != reminderInfo)
								this.AddReminderInfoToReminderList(reminderInfo);
						}

						break;
					}
				case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
					{
						this.ReminderItems.Clear();
						this.InitializeListOfReminderItems();
						break;
					}
			}

			// MD 1/20/11
			// Found while implementing NA 11.1 - Exchange Data Connector
			// If the last reminder is removed outside of the dialog, we should still close the dialog.
			if (this.ReminderItems.Count < 1)
			{
				this.Close(false);
				return;
			}
		}
		#endregion //OnDataManagerActiveRemindersCollectionChanged

		#region ParseSnoozePickerText
		internal void ParseSnoozePickerText()
		{
			// Pull the number (if any) out of the text.
			string			s				= this._snoozePickerProxy.Text.Trim().ToLower().Replace(" ", "");
			StringBuilder	numberAsString	= new StringBuilder(s.Length);
			int				numberStart		= -1;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (char.IsNumber(c))
				{
					if (numberStart == -1)
						numberStart = i;

					numberAsString.Append(c);
				}
				else if (numberStart != -1)
					break;
			}

			// If there was no number revert the text to it's previous value.
			int j;
			if (numberStart == -1 || false == int.TryParse(numberAsString.ToString(), out j))
			{
				this.RevertSnoozeText();
				return;
			}

			// Since we have a number, look at the rest of the text to see if it contains any of the
			// time literals (e.g., minute, day week etc).  if so, create the appropriate timespan and
			// update the snooze picker.
			TimeSpan timeSpan;
			int number = int.Parse(numberAsString.ToString());
#pragma warning disable 436
			if (s.Contains(SR.GetString("DLG_Recurrence_Duration_Literal_MinutesBeforeStart").ToLower().Replace(" ", "")))
				timeSpan = TimeSpan.FromMinutes(-1 * number);
			else if (s.Contains(SR.GetString("DLG_Recurrence_Duration_Literal_Minute").ToLower()))
				timeSpan = TimeSpan.FromMinutes(number);
			else if (s.Contains(SR.GetString("DLG_Recurrence_Duration_Literal_Hour").ToLower()))
				timeSpan = TimeSpan.FromHours(number);
			else if (s.Contains(SR.GetString("DLG_Recurrence_Duration_Literal_Day").ToLower()))
				timeSpan = TimeSpan.FromDays(number);
			else if (s.Contains(SR.GetString("DLG_Recurrence_Duration_Literal_Week").ToLower()))
				timeSpan = TimeSpan.FromDays(number * 7);
			else
				timeSpan = TimeSpan.FromHours(number);
#pragma warning restore 436

			// Update the duration and end times
			this._snoozePickerProxy.Text	= SnoozeListItem.SnoozeStringFromTimeSpan(timeSpan);
			this.SnoozeInterval				= timeSpan;
		}
		#endregion //ParseSnoozePickerText

		#region ProcessItems
		private void ProcessItems(Scope scope, Action action)
		{
			// First create a temporary list of the items to process.
			List<ReminderDialogListItem> tempList = new List<ReminderDialogListItem>(this._reminderListBox.SelectedItems.Count);
			switch (scope)
			{
				case Scope.Selected:
				{
					foreach (object o in this._reminderListBox.SelectedItems)
					{
						ReminderDialogListItem item = o as ReminderDialogListItem;
						if (item != null)
							tempList.Add(item);
					}

					break;
				}
				case Scope.All:
				{
					foreach (object o in this._reminderListBox.Items)
					{
						ReminderDialogListItem item = o as ReminderDialogListItem;
						if (item != null)
							tempList.Add(item);
					}

					break;
				}
			}

			int highestIndexOfRemovedItem = -1;
			foreach (ReminderDialogListItem item in tempList)
			{
				switch (action)
				{
					case Action.Snooze:
						// If the SnoozeInterval is a negative value that means the user selected a snooze interval
						// of 'xx minutes before start'.  In this case compute an interval thet gets us from now to
						// xx minutes before the activity start time.
						if (this.SnoozeInterval.Minutes < 0)
						{
							TimeSpan absoluteValueSnoozeInterval = TimeSpan.FromMinutes(Math.Abs(this.SnoozeInterval.Minutes));
							if (DateTime.UtcNow.Add(absoluteValueSnoozeInterval) < item.Activity.Start)
								item.ReminderInfo.Snooze(DateTime.UtcNow, item.Activity.Start.Subtract(absoluteValueSnoozeInterval).Subtract(DateTime.UtcNow));
							else
								item.ReminderInfo.Snooze(DateTime.UtcNow, TimeSpan.FromMinutes(0));
						}
						else
							item.ReminderInfo.Snooze(DateTime.UtcNow, this.SnoozeInterval);

						highestIndexOfRemovedItem	= Math.Max(highestIndexOfRemovedItem, this._reminderItems.IndexOf(item));

						// JM 03-31-11 TFS62936. Listen for changes to the ReminderInfo's IsSnoozed property so we can add it
						// back into our list when the snooze expires.
						item.ReminderInfo.AddListener(this, true);

						this.ReminderItems.Remove(item);

						break;
					case Action.Dismiss:
						item.ReminderInfo.Dismiss();
						highestIndexOfRemovedItem	= Math.Max(highestIndexOfRemovedItem, this._reminderItems.IndexOf(item));

						this.ReminderItems.Remove(item);

						break;
					case Action.Display:
						if (item.ReminderInfo.Context is ActivityBase)
							this._dataManager.DisplayActivityDialog(item.ReminderInfo.Context as ActivityBase, this._dataManager);

						break;
				}
			}

			// MD 1/20/11
			// Found while implementing NA 11.1 - Exchange Data Connector
			// We are now closing the dialog for dismissed items in the OnDataManagerActiveRemindersCollectionChanged callback,
			// so only close the dialog here if the last item is snoozed.
			//// If we have dismissed/snoozed the last reminder item(s), close the Dialog.
			//if (this.ReminderItems.Count < 1)
			if (action == Action.Snooze && this.ReminderItems.Count < 1)
			{
				this.Close(false);
				return;
			}

			if (highestIndexOfRemovedItem > -1)
				this.ProcessRemove(highestIndexOfRemovedItem);
		}
		#endregion //ProcessItems

		#region Process Remove
		private void ProcessRemove(int indexOfRemoveditem)
		{
			if (this._reminderListBox.SelectedItems.Count < 1 && this._reminderListBox.Items.Count > 0 && indexOfRemoveditem > -1)
			{
				int newSelectedIndex				= Math.Min(indexOfRemoveditem, (this._reminderListBox.Items.Count - 1));
				this._reminderListBox.SelectedIndex	= newSelectedIndex;
			}
		}
		#endregion //Process Remove

		#region RemoveReminderInfoFromReminderList
		private void RemoveReminderInfoFromReminderList(ReminderInfo reminderInfo)
		{
			int oldSelectedItemIndex = this._reminderListBox.SelectedIndex;

			ActivityBase activityToRemove = reminderInfo.Context as ActivityBase;
			if (null == activityToRemove)
			{
				Debug.Assert(false, "Encountered a reminder with a context that is not of type ActivityBase!");  
				return;
			}


			ReminderDialogListItem reminderDialogListItemToRemove = null;
			foreach (ReminderDialogListItem item in this.ReminderItems)
			{
				if (item.Activity == activityToRemove)
				{
					reminderDialogListItemToRemove = item;
					break;
				}
			}

			if (null != reminderDialogListItemToRemove)
				this.ReminderItems.Remove(reminderDialogListItemToRemove);

			this.ProcessRemove(oldSelectedItemIndex);
		}
		#endregion //RemoveReminderInfoFromReminderList

		#region RevertSnoozeText
		private void RevertSnoozeText()
		{
			this._snoozePickerProxy.Text = SnoozeListItem.SnoozeStringFromTimeSpan(this.SnoozeInterval);
		}
		#endregion //RevertSnoozeText

		#region SetDialogResult

		private void SetDialogResult(bool result)
		{
			if ( this.DialogElementProxy != null )
				this.DialogElementProxy.SetDialogResult(result);
		}

		#endregion //SetDialogResult

		#region UpdateCommandsStatus
		private void UpdateCommandsStatus()
		{
			// Force the CommandSourceManager to requery the CanExecute status of certain commands.
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ReminderDialogDismissAllCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ReminderDialogOpenSelectedCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ReminderDialogDismissSelectedCommand));
			CommandSourceManager.NotifyCanExecuteChanged(typeof(ReminderDialogSnoozeSelectedCommand));
		}
		#endregion //UpdateCommandsStatus

		#region UpdateDueInDescription
		private void UpdateDueInDescription(ReminderDialogListItem item)
		{
			this._timeZoneTokenHelper.Activity = item.Activity;

			// JM 11-5-10 TFS58986 - Change to use the Local timezone token instead of the Start timezone token.
			DateTime startTime = item.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken);

#pragma warning disable 436
			// JM 12-3-10 TFS58986 - Convert 'now' using the local timezone token
			//DateTime now	= DateTime.Now;
			//TimeSpan dueIn= startTime.Subtract(DateTime.Now);
			DateTime now	= ScheduleUtilities.ConvertFromUtc(this._timeZoneTokenHelper.LocalTimeZoneToken, DateTime.UtcNow);
			TimeSpan dueIn	= startTime.Subtract(now);

			// JM 11-23-10 TFS59946
			//if (dueIn.Minutes == 0)
			if (dueIn.TotalMinutes == 0)
			{
				item.DueIn = SR.GetString("TimeSpan_Literal_Minutes", 0);
				return;
			}

			// JM 11-24-10 TFS60989	- Revise the calculation
			// JM 11-24-10 TFS59945 - Eliminate trivial differences in the times
			//bool isOverdue = now > startTime;
			bool isOverdue = Math.Abs(dueIn.TotalMinutes) >= 1 && now > startTime;
			if (isOverdue)
				dueIn = dueIn.Negate();

			string timeSpanDescription = string.Empty;
			if (dueIn.TotalMinutes < 60)
			{
				// JM 11-24-10 TFS59945 - Disp[lay the literal 'Now' if the total minutes < 1
				if ((int)dueIn.TotalMinutes < 1)
					timeSpanDescription = SR.GetString("DLG_Appointment_Reminder_Literal_Now");
				else
				if ((int)dueIn.TotalMinutes == 1)
					timeSpanDescription = SR.GetString("TimeSpan_Literal_OneMinute");
				else
					// JM 04-05-11 TFS66600
					//timeSpanDescription = SR.GetString("TimeSpan_Literal_Minutes", (int)dueIn.TotalMinutes);
					timeSpanDescription = SR.GetString("TimeSpan_Literal_Minutes", (int)Math.Round(dueIn.TotalMinutes));
			}
			// JM 11-23-10 TFS59946 - If 24 hours or less, express the interval in hours, otherwise 
			// express it in days + hours
			else if ((int)dueIn.TotalHours <= 24)
			{
				// JM 11-23-10 - Not sure why we are rounding up the hours for non-overdue appointments.
				// I can't see a scenario right now where this is necessary - commenting this out
				//if (isOverdue)
				//    timeSpanDescription = SR.GetString("TimeSpan_Literal_Hours", (int)dueIn.TotalHours);
				//else
				//{
				//    // If the total hours does not fall on an hour boundary, round the hours up by 1.
				//    bool isOnHourBoundary = (dueIn.TotalMinutes * 60) == dueIn.TotalHours;
				//    if (isOnHourBoundary)
				//        timeSpanDescription = SR.GetString("TimeSpan_Literal_Hours", (int)dueIn.TotalHours);
				//    else
				//        timeSpanDescription = SR.GetString("TimeSpan_Literal_Hours", (int)dueIn.TotalHours + 1);
				//}
				if ((int)dueIn.TotalHours == 1)
					timeSpanDescription = SR.GetString("TimeSpan_Literal_OneHour");
				else
					// JM 04-05-11 TFS66600
					//timeSpanDescription = SR.GetString("TimeSpan_Literal_Hours", (int)dueIn.TotalHours);
					timeSpanDescription = SR.GetString("TimeSpan_Literal_Hours", (int)Math.Round(dueIn.TotalHours));
			}
			// JM 12-2-10 TFS59945 - If less than 14 days express the interval in days and hours, othwerwise
			// express it in weeks
			else if ((int)dueIn.TotalDays < 14)
			{
				if ((int)dueIn.TotalDays == 1)
				{
					// JM 12-2-10 TFS59945 - Just show days - not days + hours
					//if ((int)dueIn.Hours < 1)
						timeSpanDescription = SR.GetString("TimeSpan_Literal_OneDay");
					//else
					//	timeSpanDescription = SR.GetString("TimeSpan_Literal_OneDayAndHours", new object[] { (int)1, (int)dueIn.Hours });
				}
				else
				{
					// JM 12-2-10 TFS59945 - Just show days - not days + hours
					//if ((int)dueIn.Hours < 1)
						// JM 04-05-11 TFS66600
						//timeSpanDescription = SR.GetString("TimeSpan_Literal_Days", (int)dueIn.TotalDays);
						timeSpanDescription = SR.GetString("TimeSpan_Literal_Days", (int)Math.Round(dueIn.TotalDays));
					//else
					//	timeSpanDescription = SR.GetString("TimeSpan_Literal_DaysAndHours", new object[] { (int)dueIn.TotalDays, (int)dueIn.Hours });
				}
			}
			else
			{
				// JM 04-05-11 TFS66600
				//int weeks = (int)dueIn.TotalDays / 7;
				int weeks			= (int)Math.Round(dueIn.TotalDays / 7.0d);
				timeSpanDescription = SR.GetString("TimeSpan_Literal_Weeks", weeks);
			}


			if (isOverdue)
				item.DueIn = SR.GetString("DLG_Reminder_Overdue", timeSpanDescription);
			else
				item.DueIn = timeSpanDescription;
#pragma warning restore 436
		}
		#endregion //UpdateDueInDescription

		#region UpdateDueInDescriptions
		private void UpdateDueInDescriptions()
		{
			foreach (ReminderDialogListItem item in this.ReminderItems)
			{
				this.UpdateDueInDescription(item);
			}
		}
		#endregion //UpdateDueInDescriptions

		#endregion //Private Methods

		#endregion //Methods

		#region Event Handlers

		#region _reminderListBox_SelectionChanged
		void _reminderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
#pragma warning disable 436
			int selectedItemCount = this._reminderListBox.SelectedItems.Count;
			if (selectedItemCount > 0)
			{
				if (selectedItemCount == 1)
				{
					ReminderDialogListItem item = this._reminderListBox.SelectedItems[0] as ReminderDialogListItem;
					if (item != null)
					{
						if (item.ReminderInfo.Reminder != null && false == string.IsNullOrEmpty(item.ReminderInfo.Reminder.Text))
							this.SelectedActivitySubject			= item.ReminderInfo.Reminder.Text;
						else
							this.SelectedActivitySubject			= item.Activity.Subject;

						this._timeZoneTokenHelper.Activity			= item.Activity;

						// JM 11-24-10 - Change to use the Local timezone token instead of the Start timezone token.
						//this.SelectedActivityStartTimeDescription	= SR.GetString("DLG_Reminder_Literal_StartTimeDescription", item.Activity.GetStartLocal(this._timeZoneTokenHelper.ActivityStartTimeZoneTokenResolved).ToString("f"));
						this.SelectedActivityStartTimeDescription	= SR.GetString("DLG_Reminder_Literal_StartTimeDescription", item.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken).ToString("f"));
						this.SelectedActivityImageVisibility		= Visibility.Visible;
						this.SelectedActivitySubjectVisibility		= Visibility.Visible;

						switch (item.Activity.ActivityType)
						{
							case ActivityType.Journal:
								this.SelectedActivityImageSource = this.JournalImageSource;
								break;
							case ActivityType.Task:
								this.SelectedActivityImageSource = this.TaskImageSource;
								break;
							default:
								this.SelectedActivityImageSource = this.AppointmentImageSource;
								break;
						}
					}
				}

				// JM 02-22-11 TFS63063 - initialize the SnoozePicker based on the currently selected item.
				ReminderDialogListItem	earliestItem			= this.GetSelectedItemWithEarliestStartDate();
				DateTime				activityStartDateLocal	= earliestItem.Activity.GetStartLocal(this._timeZoneTokenHelper.LocalTimeZoneToken);
				this._snoozePickerProxy.ItemsSource				= SnoozeListItem.GetSnoozeListItems(activityStartDateLocal, this._timeZoneTokenHelper.LocalTimeZoneToken);
				this._snoozePickerProxy.Text					= SnoozeListItem.SnoozeStringFromTimeSpan(TimeSpan.FromMinutes(5));
				this.ParseSnoozePickerText();
			}

			this.IsSnoozePickerEnabled = selectedItemCount > 0;

			this.UpdateCommandsStatus();

			if (selectedItemCount == 0 || selectedItemCount > 1)
			{
				this.SelectedActivityStartTimeDescription	= SR.GetString("DLG_Reminder_Literal_RemindersSelected", selectedItemCount.ToString());
				this.SelectedActivityImageVisibility		= Visibility.Collapsed;
				this.SelectedActivitySubjectVisibility		= Visibility.Collapsed;
			}
#pragma warning restore 436
		}
		#endregion //_reminderListBox_SelectionChanged

		#region _snoozePicker_KeyUp
		void _snoozePicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseSnoozePickerText();
		}
		#endregion //_snoozePicker_KeyUp

		#region _snoozePicker_LostFocus
		void _snoozePicker_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this._snoozePickerProxy.Text))
				this._snoozePickerProxy.Text = SnoozeListItem.SnoozeStringFromTimeSpan(this.SnoozeInterval);

			this.ParseSnoozePickerText();
		}
		#endregion //_snoozePicker_LostFocus

		#region _snoozePicker_DropDownClosed
		void _snoozePicker_DropDownClosed(object sender, EventArgs e)
		{
			if (this._snoozePickerProxy.Text != SnoozeListItem.SnoozeStringFromTimeSpan(this.SnoozeInterval))
				this.ParseSnoozePickerText();
		}
		#endregion //_snoozePicker_DropDownClosed

		#region _snoozePicker_DropDownOpened
		void _snoozePicker_DropDownOpened(object sender, EventArgs e)
		{
		}
		#endregion //_snoozePicker_DropDownOpened

		#endregion //Event Handlers

		#region IDialogElementProxyHost Members

		bool IDialogElementProxyHost.OnClosing()
		{
			// JM 04-29-11 TFS74043
			this._dataManager.ActiveReminders.CollectionChanged -= new System.Collections.Specialized.NotifyCollectionChangedEventHandler(OnDataManagerActiveRemindersCollectionChanged);

			// Don't cancel the closing.
			return false;
		}

		#endregion

		#region Enumerations

		private enum Action
		{
			Snooze,
			Dismiss,
			Display
		}

		private enum Scope
		{
			All,
			Selected
		}

		#endregion //Enumerations

		#region ICommandTarget Members
		object ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is ReminderDialogCommandBase)
				return this;

			return null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return command is ReminderDialogCommandBase;
		}
		#endregion //ICommandTarget Members

		// JM 03-31-11 TFS62936 Added.
		#region ITypedPropertyChangeListener<object,string> Members
		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			switch (property)
			{
				case "IsSnoozed":
					ReminderInfo reminderInfo = dataItem as ReminderInfo;
					if (null != reminderInfo)
					{
						if (false == reminderInfo.IsSnoozed)
						{
							this.AddReminderInfoToReminderList(reminderInfo);
							reminderInfo.RemoveListener(this);
						}
					}

					break;
			}
		}
		#endregion //ITypedPropertyChangeListener<object,string> Members
	}

	#region SnoozeListItem Class 
	internal class SnoozeListItem
	{
		#region Member Variables

		private string							_displayString;

		// JM 02-22-11 TFS63063 No longer needed GetSnoozeListItems now returns a new instance on every call.
		//[ThreadStatic]
		//private static List<SnoozeListItem>	_snoozeListItems;

		#endregion //Member Variables

		#region Constructor
		internal SnoozeListItem(TimeSpan timeSpan)
		{
			this.TimeSpan = timeSpan;
		}
		#endregion //Constructor

		#region Base Class Overrides

		#region ToString
		public override string ToString()
		{
			return this.DisplayString;
		}
		#endregion //ToString

		#endregion //Base Class Overrides

		#region Properties

		#region DisplayString
		internal string DisplayString
		{
			get
			{
				if (string.IsNullOrEmpty(this._displayString))
					this._displayString = SnoozeListItem.SnoozeStringFromTimeSpan(this.TimeSpan);

				return this._displayString;
			}
		}
		#endregion //DisplayString

		#region TimeSpan
		internal TimeSpan TimeSpan
		{
			get;
			set;
		}
		#endregion //TimeSpan

		#endregion //Properties

		#region Methods

		#region GetSnoozeListItems (static)
		// JM 02-22-11 TFS63063 Pass in a reference datetime so we can determine which entries we should add.
		// (specifically, we should not add one or more of the 'xx minutes before start' entries depending on 
		// where we are with respect to the specified reference date.
		//internal static List<SnoozeListItem> GetSnoozeListItems()
		internal static List<SnoozeListItem> GetSnoozeListItems(DateTime activityStartDateLocal, TimeZoneToken activityStartDateLocalTimeZoneToken)
		{
			// JM 02-22-11 TFS63063 Return a new instance on every call.
			//if (_snoozeListItems != null)
			//    return _snoozeListItems;

			//_snoozeListItems = new List<SnoozeListItem>(19);
			List<SnoozeListItem> _snoozeListItems = new List<SnoozeListItem>(19);

			// JM 02-22-11 TFS63063 Only include the negative entries if there is a sufficient ammount of time before the activity starts.
			DateTime now					= ScheduleUtilities.ConvertFromUtc(activityStartDateLocalTimeZoneToken, DateTime.UtcNow);
			TimeSpan timeUntilActivityStart	= activityStartDateLocal.Subtract(now);
			if (timeUntilActivityStart.TotalMinutes > 15)
				_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, -15, 0)));


			if (timeUntilActivityStart.TotalMinutes > 10)
				_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, -10, 0)));

			if (timeUntilActivityStart.TotalMinutes > 5)
				_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, -5, 0)));

			if (timeUntilActivityStart.TotalMinutes > 0)
				_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, 0, 0)));

			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, 5, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, 10, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, 15, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(0, 30, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(1, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(2, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(4, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(8, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(12, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(1, 0, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(2, 0, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(3, 0, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(4, 0, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(7, 0, 0, 0)));
			_snoozeListItems.Add(new SnoozeListItem(new TimeSpan(14, 0, 0, 0)));

			return _snoozeListItems;
		}
		#endregion //GetSnoozeListItems (static)

		#region SnoozeStringFromTimeSpan (static)
		internal static string SnoozeStringFromTimeSpan(TimeSpan timeSpan)
		{
#pragma warning disable 436
			int minutes = timeSpan.Minutes;
			int hours	= timeSpan.Hours;
			int days	= timeSpan.Days;
			int weeks	= timeSpan.Days / 7;
			if ((weeks * 7) != days)
				weeks	= 0;

			if (minutes < 0)
				return string.Format(SR.GetString("TimeSpan_Literal_MinutesBeforeStart"), Math.Abs(minutes));

			if (minutes == 0 && hours == 0 && days == 0)
				return string.Format(SR.GetString("TimeSpan_Literal_HoursBeforeStart"), minutes);

			if (weeks > 0 && hours == 0 && minutes == 0)
			{
				if (weeks == 1)
					return SR.GetString("TimeSpan_Literal_OneWeek");
				else
					return string.Format(SR.GetString("TimeSpan_Literal_Weeks"), weeks);
			}

			if (days > 0 && hours == 0 && minutes == 0)
			{
				if (days == 1)
					return SR.GetString("TimeSpan_Literal_OneDay");
				else
					return string.Format(SR.GetString("TimeSpan_Literal_Days"), days);
			}

			if (hours > 0 && days == 0 && minutes == 0)
			{
				if (hours == 1)
					return SR.GetString("TimeSpan_Literal_OneHour");
				else
					return string.Format(SR.GetString("TimeSpan_Literal_Hours"), hours);
			}

			if (minutes > 0 && days == 0 && hours == 0)
			{
				if (minutes == 1)
					return SR.GetString("TimeSpan_Literal_OneMinute");
				else
					return string.Format(SR.GetString("TimeSpan_Literal_Minutes"), minutes);
			}

			if (hours > 0 )
			{
				hours = (days * 24) + hours;
				if (hours == 1)
					return SR.GetString("TimeSpan_Literal_OneHour");
				else
					return string.Format(SR.GetString("TimeSpan_Literal_Hours"), hours);
			}

			// Should not be here
			Debug.Assert(false, "Error creating Snooze string from TimeSpan");
			return string.Format(SR.GetString("TimeSpan_Literal_Minutes"), 10);
#pragma warning restore 436
		}
		#endregion //SnoozeStringFromTimeSpan (static)

		#endregion //Methods
	}
	#endregion //SnoozeListItem Class

	#region ReminderDialogListItem Class
	/// <summary>
	/// Represents items in the <see cref="ReminderDialog"/> list.
	/// </summary>
	/// <seealso cref="ReminderDialog"/>
	public class ReminderDialogListItem : DependencyObjectNotifier
	{
		#region MemberVariables

		private ReminderDialog					_reminderDialog;

		#endregion //ReminderDialog

		#region Constructor
		internal ReminderDialogListItem(ReminderDialog reminderDialog, ReminderInfo reminderInfo)
		{
			CoreUtilities.ValidateNotNull(reminderDialog, "reminderDialog");
			CoreUtilities.ValidateNotNull(reminderInfo, "reminderInfo");

			this._reminderDialog = reminderDialog;

			if (!(reminderInfo.Context is ActivityBase))
				throw new ArgumentException(ScheduleUtilities.GetString("LE_InvalidReminderContext")); // "ReminderInfo context is not ActivityBase!"

			this.ReminderInfo = reminderInfo;

			// Set the ImageSource property based on the Activity type.
			if (this.Activity == null)
				this.ImageSource = this._reminderDialog.AppointmentImageSource;
			else
			{
				switch (this.Activity.ActivityType)
				{
					case ActivityType.Journal:
						this.ImageSource = this._reminderDialog.JournalImageSource;
						break;
					case ActivityType.Task:
						this.ImageSource = this._reminderDialog.TaskImageSource;
						break;
					default:
						this.ImageSource = this._reminderDialog.AppointmentImageSource;
						break;
				}
			}
		}
		#endregion //Constructor

		#region Properties

		#region Activity
		/// <summary>
		/// The <see cref="ActivityBase"/> associated with the <see cref="Reminder"/>.
		/// </summary>
		public ActivityBase Activity
		{
			get	{ return this.ReminderInfo.Context as ActivityBase; }
		}
		#endregion //Activity

		#region Description
		/// <summary>
		/// Returns the <see cref="Reminder.Text"/> if specified, otherwise returns the <see cref="ActivityBase.Subject"/>.
		/// </summary>
		public string Description
		{
			get 
			{
 				if (this.Activity			!= null &&
					this.Activity.Reminder	!= null &&
					false					== string.IsNullOrEmpty(this.Activity.Reminder.Text))
					return this.Activity.Reminder.Text;

				return this.Activity.Subject; 
			}
		}
		#endregion //Description

		#region DueIn

		private static readonly DependencyProperty DueInProperty = DependencyPropertyUtilities.Register("DueIn",
			typeof(string), typeof(ReminderDialogListItem), 
			DependencyPropertyUtilities.CreateMetadata(string.Empty)
			);

		/// <summary>
		/// Returns a string representation of the amount of time left before the activity associated with the <see cref="Reminder"/> begins.
		/// </summary>
		/// <seealso cref="DueInProperty"/>
		public string DueIn
		{
			get
			{
				return (string)this.GetValue(ReminderDialogListItem.DueInProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialogListItem.DueInProperty, value);
			}
		}

		#endregion //DueIn

		#region ImageSource

		private static readonly DependencyProperty ImageSourceProperty = DependencyPropertyUtilities.Register("ImageSource",
			typeof(BitmapImage), typeof(ReminderDialogListItem), null, null);

		/// <summary>
		/// Returns a BitmapImage that represents the type of activity associated with the reminder.
		/// </summary>
		public BitmapImage ImageSource
		{
			get
			{
				return (BitmapImage)this.GetValue(ReminderDialogListItem.ImageSourceProperty);
			}
			internal set
			{
				this.SetValue(ReminderDialogListItem.ImageSourceProperty, value);
			}
		}

		#endregion //ImageSource

		#region ReminderInfo
		/// <summary>
		/// The <see cref="ReminderInfo"/> associated with the <see cref="Reminder"/>.
		/// </summary>
		public ReminderInfo ReminderInfo
		{
			get;
			set;
		}
		#endregion //ReminderInfo

		#endregion //Properties
	}
	#endregion //ReminderDialogListItem Class
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