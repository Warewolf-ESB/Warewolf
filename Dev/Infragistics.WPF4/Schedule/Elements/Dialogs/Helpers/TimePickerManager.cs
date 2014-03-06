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
using System.Collections;
using Infragistics.Collections;
using System.Collections.Generic;
using System.Text;





using System.Windows.Data;


namespace Infragistics.Controls.Schedules.Primitives
{
	#region Class TimePickerManager
	internal class TimePickerManager
	{
		#region Member Variables

		private string											_lastStartTimePickerText;
		private int												_lastStartTimePickerSelectedIndex = -1;
		private string											_lastEndTimePickerText;
		private int												_lastEndTimePickerSelectedIndex = -1;
		private ObservableCollectionExtended<TimePickerItem>	_startTimes;
		private ObservableCollectionExtended<TimePickerItem>	_endTimes;
		private List<DurationListItem>							_durationListItems;
		private bool											_subscribedToElementEvents;
		private bool											_hasStartTimePicker;
		private bool											_hasEndTimePicker;
		private bool											_hasDurationPicker;

		#endregion //Member Variables

		#region Constructor


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal TimePickerManager(ITimePickerManagerHost host, XamScheduleDataManager dataManager, ActivityBase activity, FrameworkElement startTimePicker, FrameworkElement endTimePicker, FrameworkElement durationPicker)
		{
			// Validate and save paramaters.
			CoreUtilities.ValidateNotNull(activity, "activity");
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");
			CoreUtilities.ValidateNotNull(host, "host");
			this.DataManager			= dataManager;
			this.Activity				= activity;
			this.Host					= host;

			this._hasStartTimePicker	= null != startTimePicker;
			this._hasEndTimePicker		= null != endTimePicker;
			this._hasDurationPicker		= null != durationPicker;

			// Create the ComboBoxProxies for the time picker elements
			if (this._hasStartTimePicker)
				this.StartTimePickerProxy	= new ComboBoxProxy(startTimePicker);

			if (this._hasEndTimePicker)
				this.EndTimePickerProxy		= new ComboBoxProxy(endTimePicker);

			if (this._hasDurationPicker)
				this.DurationPickerProxy	= new ComboBoxProxy(durationPicker);

			// Listen to events on the pickers.
			if (this._hasStartTimePicker)
			{
				this.StartTimePickerProxy.LostFocus			+= new ComboBoxProxy.ProxyLostFocusEventHandler(StartTimePicker_LostFocus);
				this.StartTimePickerProxy.DropDownClosed	+= new ComboBoxProxy.ProxyDropDownClosedEventHandler(StartTimePicker_DropDownClosed);
				this.StartTimePickerProxy.DropDownOpened	+= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(StartTimePicker_DropDownOpened);
				this.StartTimePickerProxy.KeyUp				+= new KeyEventHandler(StartTimePicker_KeyUp);
			}

			if (this._hasEndTimePicker)
			{
				this.EndTimePickerProxy.LostFocus			+= new ComboBoxProxy.ProxyLostFocusEventHandler(EndTimePicker_LostFocus);
				this.EndTimePickerProxy.DropDownClosed		+= new ComboBoxProxy.ProxyDropDownClosedEventHandler(EndTimePicker_DropDownClosed);
				this.EndTimePickerProxy.DropDownOpened		+= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(EndTimePicker_DropDownOpened);
				this.EndTimePickerProxy.KeyUp				+= new KeyEventHandler(EndTimePicker_KeyUp);
			}

			if (this._hasDurationPicker)
			{
				this.DurationPickerProxy.LostFocus		+= new ComboBoxProxy.ProxyLostFocusEventHandler(DurationPicker_LostFocus);
				this.DurationPickerProxy.DropDownClosed	+= new ComboBoxProxy.ProxyDropDownClosedEventHandler(DurationPicker_DropDownClosed);
				this.DurationPickerProxy.DropDownOpened	+= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(DurationPicker_DropDownOpened);
				this.DurationPickerProxy.KeyUp			+= new KeyEventHandler(DurationPicker_KeyUp);
			}

			this._subscribedToElementEvents				= true;

			// Create a DataTemplate that binds to the DisplayString property of the TimePickerItem and set it as the proxy's ItemTemplate.






			DataTemplate			dt		=  new DataTemplate();
			FrameworkElementFactory fef		= new FrameworkElementFactory(typeof(TextBlock));
			Binding					binding = new Binding("DisplayString");
			fef.SetBinding(TextBlock.TextProperty, binding);
			dt.VisualTree = fef;


			if (this._hasStartTimePicker)
				this.StartTimePickerProxy.ItemTemplate	= dt;

			if (this._hasEndTimePicker)
				this.EndTimePickerProxy.ItemTemplate	= dt;

			// Initialize the appointment duration.
			this.Duration = this.GetDurationFromActivity(this.Activity);
			if (this._hasDurationPicker)
			{
				this.DurationPickerProxy.ItemsSource	= this.DurationListItems;
				this.DurationPickerProxy.Text			= DurationListItem.DurationStringFromTimeSpan(this.Duration);
			}

			// Initialize the start/end time pickers.
			this.InitializeStartTimes();

			if (this._hasStartTimePicker)
			{
				this.StartTimePickerProxy.SelectedIndex = TimePickerManager.GetTimePickerIndexToSelect(this.StartTimePickerProxy.Items, this.Host.StartAsLocal);
				this.StartTimePickerProxy.Text			= ScheduleUtilities.GetTimeString(this.Host.StartAsLocal);
				this._lastStartTimePickerText			= this.StartTimePickerProxy.Text;
			}

			if (this._hasEndTimePicker)
				this.SynchronizeEndTimePicker();

		}
		#endregion //Constructor

		#region Properties

		#region Internal Properties

		#region Duration
		internal TimeSpan Duration
		{
			get;
			private set;
		}
		#endregion //Duration	
    
		#region DurationPickerProxy
		internal ComboBoxProxy DurationPickerProxy
		{
			get;
			set;
		}
		#endregion //DurationPickerProxy	
    
		#region EndTimePickerProxy
		internal ComboBoxProxy EndTimePickerProxy
		{
			get;
			set;
		}
		#endregion //EndTimePickerProxy	
    
		#region HasEndTimePicker
		internal bool HasEndTimePicker
		{
			get { return this._hasEndTimePicker; }
		}
		#endregion //HasEndTimePicker

		#region HasStartTimePicker
		internal bool HasStartTimePicker
		{
			get { return this._hasStartTimePicker; }
		}
		#endregion //HasStartTimePicker

		#region StartTimePickerProxy
		internal ComboBoxProxy StartTimePickerProxy
		{
			get;
			set;
		}

		#endregion //StartTimePickerProxy

		#endregion //Internal properties

		#region Private Properties

		#region Activity
		private ActivityBase Activity
		{
			get;
			set;
		}
		#endregion //Activity

		#region DurationListItems
		private List<DurationListItem> DurationListItems
		{
			get
			{
				if (this._durationListItems == null)
				{
					this._durationListItems = new List<DurationListItem>(25);
					this._durationListItems.Add(new DurationListItem(new TimeSpan(0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(0, 5, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(0, 10, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(0, 15, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(0, 30, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(1, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(2, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(3, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(4, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(5, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(6, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(7, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(8, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(9, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(10, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(11, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(12, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(18, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(1, 0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(2, 0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(3, 0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(4, 0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(7, 0, 0, 0)));
					this._durationListItems.Add(new DurationListItem(new TimeSpan(14, 0, 0, 0)));
				}

				return this._durationListItems;
			}
		}
		#endregion //DurationListItems
    
		#region Host
		private ITimePickerManagerHost Host
		{
			get;
			set;
		}
		#endregion //Host	
    
		#region DataManager
		private XamScheduleDataManager DataManager
		{
			get;
			set;
		}
		#endregion //DataManager	
    
		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region DisconnectEventListeners
		internal void DisconnectEventListeners()
		{
			if (false == this._subscribedToElementEvents)
				return;

			if (this._hasStartTimePicker)
			{
				this.StartTimePickerProxy.LostFocus			-= new ComboBoxProxy.ProxyLostFocusEventHandler(StartTimePicker_LostFocus);
				this.StartTimePickerProxy.DropDownClosed	-= new ComboBoxProxy.ProxyDropDownClosedEventHandler(StartTimePicker_DropDownClosed);
				this.StartTimePickerProxy.DropDownOpened	-= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(StartTimePicker_DropDownOpened);
				this.StartTimePickerProxy.KeyUp				-= new KeyEventHandler(StartTimePicker_KeyUp);
			}

			if (this._hasEndTimePicker)
			{
				this.EndTimePickerProxy.LostFocus			-= new ComboBoxProxy.ProxyLostFocusEventHandler(EndTimePicker_LostFocus);
				this.EndTimePickerProxy.DropDownClosed		-= new ComboBoxProxy.ProxyDropDownClosedEventHandler(EndTimePicker_DropDownClosed);
				this.EndTimePickerProxy.DropDownOpened		-= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(EndTimePicker_DropDownOpened);
				this.EndTimePickerProxy.KeyUp				-= new KeyEventHandler(EndTimePicker_KeyUp);
			}

			if (this._hasDurationPicker)
			{
				this.DurationPickerProxy.LostFocus		-= new ComboBoxProxy.ProxyLostFocusEventHandler(DurationPicker_LostFocus);
				this.DurationPickerProxy.DropDownClosed	-= new ComboBoxProxy.ProxyDropDownClosedEventHandler(DurationPicker_DropDownClosed);
				this.DurationPickerProxy.DropDownOpened	-= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(DurationPicker_DropDownOpened);
				this.DurationPickerProxy.KeyUp			-= new KeyEventHandler(DurationPicker_KeyUp);
			}


			this._subscribedToElementEvents = false;
		}
		#endregion //DisconnectEventListeners

		#region ForceTimePickersToUpdate
		internal void ForceTimePickersToUpdate()
		{
			if (this._hasStartTimePicker && this.StartTimePickerProxy.Text != this._lastStartTimePickerText)
				this.ParseTimePickerText(this.StartTimePickerProxy, true, true);
			else
			if (this._hasEndTimePicker && this.EndTimePickerProxy.Text != this._lastEndTimePickerText)
				this.ParseTimePickerText(this.EndTimePickerProxy, false, true);
			else
			if (this._hasDurationPicker && this.DurationPickerProxy.Text != DurationListItem.DurationStringFromTimeSpan(this.Duration))
				this.ParseDurationPickerText();
		}
		#endregion //ForceTimePickersToUpdate

		#region GetTimePickerCurrentTime (static)
		internal static DateTime? GetTimePickerCurrentTime(ComboBoxProxy timePicker)
		{
			if (null == timePicker)
				return null;

			if (timePicker.Items.Count < 1)
				return null;

			TimePickerItem timePickerItem = timePicker.SelectedItem as TimePickerItem;
			DateTime? time;

			try
			{
				string timeString = timePicker.Text;

				if (string.IsNullOrEmpty(timeString) && timePickerItem != null)
					timeString = timePickerItem.SimpleString;


				time = DateTime.Parse(timeString);
			}
			catch (Exception)
			{
				time = null;
			}

			return time;
		}
		#endregion //GetTimePickerCurrentTime (static)

		#region GetTimePickerIndexToSelect (static)
		internal static int GetTimePickerIndexToSelect(IList timePickerItems, DateTime dateTimeToSelect)
		{
			int index = 0;

			// Special case if time is after the last entry but before the first entry
			if (dateTimeToSelect.TimeOfDay > ((TimePickerItem)timePickerItems[timePickerItems.Count - 1]).DateTime.TimeOfDay  &&
				dateTimeToSelect.TimeOfDay < ((TimePickerItem)timePickerItems[0]).DateTime.TimeOfDay)
				return timePickerItems.Count - 1;

			// Check to see if there is an exact match with an item in the list
			for (int i = 0; i < timePickerItems.Count; i++)
			{
				TimePickerItem item = timePickerItems[i] as TimePickerItem;
				if (dateTimeToSelect.TimeOfDay == item.DateTime.TimeOfDay)
					return i;
			}

			for (int i = 0; i < timePickerItems.Count; i++)
			{
				TimePickerItem item = timePickerItems[i] as TimePickerItem;
				if (dateTimeToSelect.TimeOfDay == item.DateTime.TimeOfDay)
				{
					index = i;
					break;
				}
				else if (dateTimeToSelect.TimeOfDay > item.DateTime.TimeOfDay)
				{
					//index = Math.Max(0, i - 1);
					index = i;
					break;
				}
			}

			return index;
		}
		#endregion //GetTimePickerIndexToSelect (static)

		#region ParseTimePickerText
		internal void ParseTimePickerText(ComboBoxProxy timePicker, bool isStartTimePicker, bool restoreTextOnError)
		{
			if (null != timePicker)
				this.ParseTimePickerText(timePicker, isStartTimePicker, restoreTextOnError, -1);
		}

		internal void ParseTimePickerText(ComboBoxProxy timePicker, bool isStartTimePicker, bool restoreTextOnError, int indexOfSelectedTimeEntry)
		{
			if (null == timePicker)
				return;

			// Bypass if TimePicker is EndTimePicker and the activity does snot support end times.
			if (false == isStartTimePicker && false == this.DataManager.IsEndDateSupportedByActivity(this.Activity))
				return;

			// Bypass logic if the combo is not initialized
			if (timePicker.Items.Count < 1)
				return;

			// JM 04-11-11 TFS70524
			if (true	== isStartTimePicker && this._lastStartTimePickerText	== timePicker.Text)
				return;
			else
			if (false	== isStartTimePicker && this._lastEndTimePickerText		== timePicker.Text)
				return;

			DateTime newTime;
			if (indexOfSelectedTimeEntry != -1)
				newTime = ((TimePickerItem)timePicker.Items[indexOfSelectedTimeEntry]).DateTime;
			else
			{
				DateTime? t = TimePickerManager.GetTimePickerCurrentTime(timePicker);
				if (false == t.HasValue)
				{
					

					if (restoreTextOnError)
					{
						int ix = TimePickerManager.GetTimePickerIndexToSelect(timePicker.Items, (isStartTimePicker ? this.Host.StartAsLocal : this.Host.EndAsLocal));
						if (timePicker.SelectedIndex != ix)
							timePicker.SelectedIndex = ix;

						timePicker.Text = ScheduleUtilities.GetTimeString(isStartTimePicker ? this.Host.StartAsLocal : this.Host.EndAsLocal);
					}

					return;
				}

				// Update the time picker selection.
				int index = TimePickerManager.GetTimePickerIndexToSelect(timePicker.Items, t.Value);
				if (timePicker.SelectedIndex != index)
					timePicker.SelectedIndex = index;

				TimePickerItem item = timePicker.SelectedItem as TimePickerItem;
				newTime = isStartTimePicker ? ScheduleUtilities.CombineDateAndTime(item.DateTime.Date, t.Value) :
											  ScheduleUtilities.CombineDateAndTime(item.DateTime.Date, t.Value);
			}

			if (isStartTimePicker)
			{
				// Update the appointment start time.
				this.Activity.SetStartLocal(this.Host.ActivityStartTimeZoneTokenResolved, newTime);
				timePicker.Text					= ScheduleUtilities.GetTimeString(this.Host.StartAsLocal);
				this._lastStartTimePickerText	= timePicker.Text;

				this.Host.UpdateStartDatePickerDateFromNewStartTime(newTime);

				// Update the appointment end time.
				if (this.DataManager.IsEndDateSupportedByActivity(this.Activity))
					this.Activity.SetEndLocal(this.Host.ActivityEndTimeZoneTokenResolved, newTime.Add(this.Duration));

				// Synchronize the EndTimePicker
				if (this._hasEndTimePicker)
					this.SynchronizeEndTimePicker();
			}
			else
			{
				DateTime holdEnd = this.Activity.End;

				// Update the appointment end time.
				this.Activity.SetEndLocal(this.Host.ActivityEndTimeZoneTokenResolved, newTime);
				if (this.Host.HasDateConflicts(true))
				{
					this.Activity.End	= holdEnd;
					timePicker.Text			= ScheduleUtilities.GetTimeString(this.Host.EndAsLocal);
				}
				else
				{
					timePicker.Text				= ScheduleUtilities.GetTimeString(this.Host.EndAsLocal);
					this._lastEndTimePickerText = timePicker.Text;

					this.Host.UpdateEndDatePickerDateFromNewEndTime(newTime);

					// Update the duration.
					this.Duration = this.Activity.GetEndLocal(this.Host.ActivityEndTimeZoneTokenResolved).Subtract(this.Activity.GetStartLocal(this.Host.ActivityStartTimeZoneTokenResolved));

					if (this._hasDurationPicker)
						this.DurationPickerProxy.Text = DurationListItem.DurationStringFromTimeSpan(this.Duration);
				}
			}

			// Check start/end dates and update conflict/adjacent status.
			this.Host.UpdateConflictStatus();
		}
		#endregion //ParseTimePickerText

		#region SetEndTimePickerEnabledStatus
		internal void SetEndTimePickerEnabledStatus(bool enabled)
		{
			if (this._hasEndTimePicker)
				this.EndTimePickerProxy.IsEnabled = enabled;
		}
		#endregion //SetEndTimePickerEnabledStatus

		#region SetStartTimePickerEnabledStatus
		internal void SetStartTimePickerEnabledStatus(bool enabled)
		{
			if (this._hasStartTimePicker)
				this.StartTimePickerProxy.IsEnabled = enabled;
		}
		#endregion //SetStartTimePickerEnabledStatus

		#region SynchronizeEndTimePicker
		internal void SynchronizeEndTimePicker()
		{
			if (false == this._hasEndTimePicker)
				return;

			if (false == this.DataManager.IsEndDateSupportedByActivity(this.Activity))
				return;

			this.InitializeEndTimes();

			int index = TimePickerManager.GetTimePickerIndexToSelect(this.EndTimePickerProxy.Items, this.Host.EndAsLocal);
			if (this.EndTimePickerProxy.SelectedIndex != index)
				this.EndTimePickerProxy.SelectedIndex	= index;

			this.EndTimePickerProxy.Text				= ScheduleUtilities.GetTimeString(this.Host.EndAsLocal);
			this._lastEndTimePickerText					= this.EndTimePickerProxy.Text;
		}
		#endregion //SynchronizeEndTimePicker

		#region SynchronizeStartTimePicker
		internal void SynchronizeStartTimePicker()
		{
			if (false == this._hasStartTimePicker)
				return;

			this.InitializeStartTimes();

			int index = TimePickerManager.GetTimePickerIndexToSelect(this.StartTimePickerProxy.Items, this.Host.StartAsLocal);
			if (this.StartTimePickerProxy.SelectedIndex != index)
				this.StartTimePickerProxy.SelectedIndex = index;

			this.StartTimePickerProxy.Text	= ScheduleUtilities.GetTimeString(this.Host.StartAsLocal);
			this._lastStartTimePickerText	= this.StartTimePickerProxy.Text;
		}
		#endregion //SynchronizeStartTimePicker

		#region UpdateDuration
		internal void UpdateDuration(TimeSpan newDuration, bool updateEndTimePicker)
		{
			this.Duration = newDuration;
			if (updateEndTimePicker && this.DataManager.IsEndDateSupportedByActivity(this.Activity))
			{
				this.Activity.End = this.Activity.Start.Add(this.Duration); 

				if (this._hasEndTimePicker)
					this.SynchronizeEndTimePicker();
			}
		}
		#endregion //UpdateDuration

		#endregion //Internal Methods

		#region Private Methods

		#region GetDurationFromActivity
		private TimeSpan GetDurationFromActivity(ActivityBase activity)
		{
			if (activity.End == DateTime.MinValue  ||  false == this.DataManager.IsEndDateSupportedByActivity(activity))
				return TimeSpan.FromTicks(0);

			// JM 12-1-10 TFS60389
			//return activity.End.Subtract(activity.Start);
			return this.Host.EndAsLocal.Subtract(this.Host.StartAsLocal);
		}
		#endregion //GetDurationFromActivity

		#region InitializeEndTimes
		private void InitializeEndTimes()
		{
			if (false == this._hasEndTimePicker)
				return;

			// Allocate a new list or clear the existing list.
			if (this._endTimes == null)
			{
				this._endTimes						= new ObservableCollectionExtended<TimePickerItem>(new List<TimePickerItem>(48));
				this.EndTimePickerProxy.ItemsSource = this._endTimes;
			}
			else
				this._endTimes.Clear();

			this._endTimes.BeginUpdate();
			if (this.Host.EndAsLocal.Subtract(this.Host.StartAsLocal).Ticks < TimeSpan.FromHours(24).Ticks)
				this.DataManager.PopulateListWithAppointmentEndTimes(this._endTimes, this.Host.StartAsLocal, this.Host.StartAsLocal, true);
			else
				this.DataManager.PopulateListWithAppointmentEndTimes(this._endTimes, this.Host.StartAsLocal, this.Host.EndAsLocal.Date, false);
			this._endTimes.EndUpdate();
		}
		#endregion //InitializeEndTimes

		#region InitializeStartTimes
		private void InitializeStartTimes()
		{
			if (false == this._hasStartTimePicker)
				return;

			// Allocate a new list or clear the existing list.
			if (this._startTimes == null)
			{
				this._startTimes						= new ObservableCollectionExtended<TimePickerItem>(new List<TimePickerItem>(48));
				this.StartTimePickerProxy.ItemsSource	= this._startTimes;
			}
			else
				this._startTimes.Clear();

			this._startTimes.BeginUpdate();
			DateTime start = this.Host.StartAsLocal;
			this.DataManager.PopulateListWithAppointmentStartTimes(this._startTimes, start.Month, start.Day, start.Year);
			this._startTimes.EndUpdate();
		}
		#endregion //InitializeStartTimes

		#region ParseDurationPickerText
		internal void ParseDurationPickerText()
		{
			if (false == this._hasDurationPicker)
				return;

			// Pull the number (if any) out of the text.
			string			s				= this.DurationPickerProxy.Text.Trim().ToLower();
			StringBuilder	numberAsString	= new StringBuilder(s.Length);
			int				numberStart		= -1;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (char.IsNumber(c) || s[i] == '.')
				{
					if (numberStart == -1)
						numberStart = i;

					numberAsString.Append(c);
				}
				else if (numberStart != -1)
					break;
			}

			// If there was no number revert the text to it's previous value.
			double number;
			if (numberStart == -1 || false == double.TryParse(numberAsString.ToString(), out number))
			{
				this.RevertDurationText();
				return;
			}

			// Since we have a number, look at the rest of the text to see if it contains any of the
			// time literals (e.g., minute, day week etc).  if so, create the appropriate timespan and
			// update the appointment reminder
			TimeSpan timeSpan;
			if (s.Contains(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Literal_Minute").ToLower()))
				timeSpan	= TimeSpan.FromMinutes(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Literal_Hour").ToLower()))
				timeSpan	= TimeSpan.FromHours(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Literal_Day").ToLower()))
				timeSpan	= TimeSpan.FromDays(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Literal_Week").ToLower()))
				timeSpan	= TimeSpan.FromDays(number * 7);
			else
				timeSpan	= TimeSpan.FromHours(number);

			// If the activity does not support EndDate, force the duration to be 0.
			if (false == this.DataManager.IsEndDateSupportedByActivity(this.Activity))
				timeSpan = TimeSpan.FromTicks(0);

			// Update the duration and end times
			this.DurationPickerProxy.Text = DurationListItem.DurationStringFromTimeSpan(timeSpan);
			this.UpdateDuration(timeSpan, true);
			this.Host.UpdateDirtyStatus(true);
		}
		#endregion //ParseDurationPickerText

		#region RevertDurationText
		private void RevertDurationText()
		{
			if (false == this._hasDurationPicker)
				return;

#pragma warning disable 436
			string text = SR.GetString("DLG_Appointment_Reminder_None");
#pragma warning restore 436

			text = DurationListItem.DurationStringFromTimeSpan(this.Duration);

			this.DurationPickerProxy.Text = text;
		}
		#endregion //RevertDurationText

		#endregion //Private Methods

		#endregion //Methods

		#region Event Handlers

		#region DurationPicker_KeyUp
		void DurationPicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseDurationPickerText();
			else
				this.Host.UpdateDirtyStatus(true);
		}
		#endregion //DurationPicker_KeyUp

		#region DurationPicker_LostFocus
		void DurationPicker_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.DurationPickerProxy.Text))
				this.DurationPickerProxy.Text = DurationListItem.DurationStringFromTimeSpan(this.Duration);

			this.ParseDurationPickerText();
		}
		#endregion //DurationPicker_LostFocus

		#region DurationPicker_DropDownClosed
		void DurationPicker_DropDownClosed(object sender, EventArgs e)
		{
			if (this.DurationPickerProxy.Text != DurationListItem.DurationStringFromTimeSpan(this.Duration))
				this.ParseDurationPickerText();
		}
		#endregion //DurationPicker_DropDownClosed

		#region DurationPicker_DropDownOpened
		void DurationPicker_DropDownOpened(object sender, EventArgs e)
		{
		}
		#endregion //DurationPicker_DropDownOpened

		#region EndTimePicker_KeyUp
		void EndTimePicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseTimePickerText(this.EndTimePickerProxy, false, true);
			else
				this.Host.UpdateDirtyStatus(true);
		}
		#endregion //EndTimePicker_KeyUp

		#region EndTimePicker_LostFocus
		void EndTimePicker_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.EndTimePickerProxy.Text))
				this.EndTimePickerProxy.Text = this._lastEndTimePickerText;

			if (this.Host.IsOkToParseEndTimePickerText)
				this.ParseTimePickerText(this.EndTimePickerProxy, false, true);
		}
		#endregion //EndTimePicker_LostFocus

		#region EndTimePicker_DropDownClosed
		void EndTimePicker_DropDownClosed(object sender, EventArgs e)
		{
			if (this.EndTimePickerProxy.SelectedIndex != this._lastEndTimePickerSelectedIndex)
				this.ParseTimePickerText(this.EndTimePickerProxy, false, true, this.EndTimePickerProxy.SelectedIndex);
		}
		#endregion //EndTimePicker_DropDownClosed

		#region EndTimePicker_DropDownOpened
		void EndTimePicker_DropDownOpened(object sender, EventArgs e)
		{
			this._lastEndTimePickerSelectedIndex = this.EndTimePickerProxy.SelectedIndex;
		}
		#endregion //EndTimePicker_DropDownOpened

		#region StartTimePicker_KeyUp
		void StartTimePicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseTimePickerText(this.StartTimePickerProxy, true, true);
			else
				this.Host.UpdateDirtyStatus(true);
		}
		#endregion //StartTimePicker_KeyUp

		#region StartTimePicker_LostFocus
		void StartTimePicker_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.StartTimePickerProxy.Text))
				this.StartTimePickerProxy.Text = this._lastStartTimePickerText;

			this.ParseTimePickerText(this.StartTimePickerProxy, true, true);
		}
		#endregion //StartTimePicker_LostFocus

		#region StartTimePicker_DropDownClosed
		void StartTimePicker_DropDownClosed(object sender, EventArgs e)
		{
			if (this.StartTimePickerProxy.SelectedIndex != this._lastStartTimePickerSelectedIndex)
				this.ParseTimePickerText(this.StartTimePickerProxy, true, true, this.StartTimePickerProxy.SelectedIndex);
		}
		#endregion //StartTimePicker_DropDownClosed

		#region StartTimePicker_DropDownOpened
		void StartTimePicker_DropDownOpened(object sender, EventArgs e)
		{
			this._lastStartTimePickerSelectedIndex = this.StartTimePickerProxy.SelectedIndex;
		}
		#endregion //StartTimePicker_DropDownOpened

		#endregion //Event Handlers
	}
	#endregion //Class TimePickerManager

	#region Interface ITimePickerManagerHost
	internal interface ITimePickerManagerHost
	{
		DateTime StartAsLocal { get; }
		DateTime EndAsLocal { get; }

		TimeZoneToken ActivityStartTimeZoneTokenResolved { get; }
		TimeZoneToken ActivityEndTimeZoneTokenResolved { get; }

		bool IsOkToParseEndTimePickerText { get; }

		void UpdateDirtyStatus(bool isDirty);
		void UpdateConflictStatus();
		bool HasDateConflicts(bool displayMessageOnConflict);

		void UpdateStartDatePickerDateFromNewStartTime(DateTime newStartTime);
		void UpdateEndDatePickerDateFromNewEndTime(DateTime newStartTime);
	}
	#endregion //Interface ITimePickerManagerHost
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