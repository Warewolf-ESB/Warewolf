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
using Infragistics.Collections;
using System.Collections.Generic;






using System.Windows.Data;


namespace Infragistics.Controls.Schedules.Primitives
{
	internal class ReminderDatePickerManager
	{
		#region Member Variables

		private string											_lastTimePickerText;
		private int												_lastTimePickerSelectedIndex = -1;
		private ObservableCollectionExtended<TimePickerItem>	_times;
		private bool											_subscribedToElementEvents;






		#endregion //Member Variables

		#region Constructor


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal ReminderDatePickerManager(IReminderDatePickerManagerHost host, XamScheduleDataManager dataManager, ActivityBase activity, DatePicker datePicker, FrameworkElement timePicker)
		{
			// Validate and save paramaters.
			CoreUtilities.ValidateNotNull(host, "host");
			CoreUtilities.ValidateNotNull(activity, "activity");
			CoreUtilities.ValidateNotNull(dataManager, "dataManager");
			CoreUtilities.ValidateNotNull(datePicker, "datePicker");
			CoreUtilities.ValidateNotNull(timePicker, "timePicker");
			this.Host					= host;
			this.DataManager			= dataManager;
			this.Activity				= activity;
			this.DatePicker			= datePicker;

			// Create the ComboBoxProxies for the time picker element
			this.TimePickerProxy	= new ComboBoxProxy(timePicker);

			// Create a DataTemplate that binds to the DisplayString property of the TimePickerItem and set it as the proxy's ItemTemplate.






			DataTemplate			dt		=  new DataTemplate();
			FrameworkElementFactory fef		= new FrameworkElementFactory(typeof(TextBlock));
			Binding					binding = new Binding("DisplayString");
			fef.SetBinding(TextBlock.TextProperty, binding);
			dt.VisualTree = fef;


			this.TimePickerProxy.ItemTemplate	= dt;

			// Initialize the Reminder date/time based on the activity's Start and ReminderInterval
			this.DatePicker.SelectedDate		= this.ReminderDate = this.InitializeReminderDateFromActivity();

			// Initialize the time picker.
			this.InitializeTimes();

			// Set the Enabled status of the date/time pickers based on the ReminderEnabled status
			this.DatePicker.IsEnabled			= this.TimePickerProxy.IsEnabled = this.Activity.ReminderEnabled;

			// Listen to events on the time and date pickers.
			this.TimePickerProxy.LostFocus			+= new ComboBoxProxy.ProxyLostFocusEventHandler(TimePicker_LostFocus);
			this.TimePickerProxy.DropDownClosed		+= new ComboBoxProxy.ProxyDropDownClosedEventHandler(TimePicker_DropDownClosed);
			this.TimePickerProxy.DropDownOpened		+= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(TimePicker_DropDownOpened);
			this.TimePickerProxy.KeyUp				+= new KeyEventHandler(TimePicker_KeyUp);

			this.DatePicker.SelectedDateChanged		+= new EventHandler<SelectionChangedEventArgs>(DatePicker_SelectedDateChanged);
			
			this.Activity.PropertyChanged			+= new System.ComponentModel.PropertyChangedEventHandler(Activity_PropertyChanged);
			this._subscribedToElementEvents			= true;
		}
		#endregion //Constructor

		#region Properties

		#region Activity
		private ActivityBase Activity
		{
			get;
			set;
		}
		#endregion //Activity

		#region DataManager
		private XamScheduleDataManager DataManager
		{
			get;
			set;
		}
		#endregion //DataManager	

		#region DatePicker
		private DatePicker DatePicker
		{
			get;
			set;
		}

		#endregion //DatePicker

		#region Host
		private IReminderDatePickerManagerHost Host
		{
			get;
			set;
		}
		#endregion //Host	

		#region ReminderDate
		private DateTime ReminderDate
		{
			get;
			set;
		}
		#endregion //ReminderDate

		#region TimePickerProxy
		private ComboBoxProxy TimePickerProxy
		{
			get;
			set;
		}

		#endregion //TimePickerProxy

		#endregion //Properties

		#region Methods

		#region Private Methods 

		#region InitializeReminderDateFromActivity
		private DateTime InitializeReminderDateFromActivity()
		{
			if (this.Activity.ReminderInterval.TotalMinutes == 0)
				return this.Host.StartAsLocal;

			// MD 2/28/11
			// Found this while tasting reminders for tasks.
			// Since we are subtracting a time span from the local time, the result will be in local time also, so we don't need to 
			// convert from Utc. Also, we don't have to do anything separate for positive and negative reminders, because the subtraction
			// operator will handle negatives correctly. Also, TimeSpan.FromMinutes(this.Activity.ReminderInterval.TotalMinutes) is essentially
			// a no-op, so we can just use this.Activity.ReminderInterval.
			//if (this.Activity.ReminderInterval.TotalMinutes < 0)
			//{
			//    DateTime reminderDateUtc = this.Host.StartAsLocal.AddMinutes(Math.Abs(this.Activity.ReminderInterval.TotalMinutes));
			//    return ScheduleUtilities.ConvertFromUtc(this.Host.ActivityStartTimeZoneTokenResolved, reminderDateUtc);
			//}
			//else
			//{
			//    DateTime reminderDateUtc = this.Host.StartAsLocal.Subtract(TimeSpan.FromMinutes(this.Activity.ReminderInterval.TotalMinutes));
			//    return ScheduleUtilities.ConvertFromUtc(this.Host.ActivityStartTimeZoneTokenResolved, reminderDateUtc);
			//}
			return this.Host.StartAsLocal - this.Activity.ReminderInterval;
		}
		#endregion //InitializeReminderDateFromActivity

		#region InitializeTimes
		private void InitializeTimes()
		{
			// Allocate a new list or clear the existing list.
			if (this._times == null)
			{
				this._times = new ObservableCollectionExtended<TimePickerItem>(new List<TimePickerItem>(48));
				this.TimePickerProxy.ItemsSource = this._times;
			}
			else
				this._times.Clear();

			// Add times to the list.
			this._times.BeginUpdate();

			DateTime			nextTime		= new DateTime(this.ReminderDate.Year, this.ReminderDate.Month, this.ReminderDate.Day, 0, 0, 0);
			DateInfoProvider	dateProvider	= this.DataManager.DateInfoProviderResolved;
			for (int i = 0; i < 48; i++)
			{
				this._times.Add(new TimePickerItem(nextTime));

				DateTime? adjustedTime = dateProvider.AddMinutes(nextTime, 30);
				if (adjustedTime == null)
					break;

				nextTime= adjustedTime.Value;
			}

			this._times.EndUpdate();

			this.TimePickerProxy.SelectedIndex	= TimePickerManager.GetTimePickerIndexToSelect(this.TimePickerProxy.Items, this.ReminderDate);
			this.TimePickerProxy.Text			= ScheduleUtilities.GetTimeString(this.ReminderDate);
			this._lastTimePickerText			= this.TimePickerProxy.Text;
		}
		#endregion //InitializeTimes

		#region ParseTimePickerText
		private void ParseTimePickerText(bool restoreTextOnError)
		{
			this.ParseTimePickerText(restoreTextOnError, -1);
		}

		private void ParseTimePickerText(bool restoreTextOnError, int indexOfSelectedTimeEntry)
		{
			// Bypass logic if the combo is not initialized
			if (this.TimePickerProxy.Items.Count < 1)
				return;

			DateTime newTime;
			if (indexOfSelectedTimeEntry != -1)
				newTime = ((TimePickerItem)this.TimePickerProxy.Items[indexOfSelectedTimeEntry]).DateTime;
			else
			{
				DateTime? t = TimePickerManager.GetTimePickerCurrentTime(this.TimePickerProxy);
				if (false == t.HasValue)
				{
					if (restoreTextOnError)
					{
						int ix = TimePickerManager.GetTimePickerIndexToSelect(this.TimePickerProxy.Items, this.ReminderDate);
						if (this.TimePickerProxy.SelectedIndex != ix)
							this.TimePickerProxy.SelectedIndex = ix;

						this.TimePickerProxy.Text = ScheduleUtilities.GetTimeString(this.ReminderDate);
					}

					return;
				}

				newTime = ScheduleUtilities.CombineDateAndTime(this.ReminderDate.Date, t.Value);
			}

			// Update the Reminder date.
			this.ReminderDate			= newTime;

			// Update the time picker text. 
			this.TimePickerProxy.Text	= ScheduleUtilities.GetTimeString(this.ReminderDate);

			this.SetActivityReminderIntervalFromReminderDateTime();
			this._lastTimePickerText = this.TimePickerProxy.Text;
		}
		#endregion //ParseTimePickerText

		#region SetActivityReminderIntervalFromReminderDateTime
		private void SetActivityReminderIntervalFromReminderDateTime()
		{
			if (this.ReminderDate == this.Host.StartAsLocal)
				this.Activity.ReminderInterval = TimeSpan.FromMinutes(0);
			else
				this.Activity.ReminderInterval = this.Host.StartAsLocal.Subtract(this.ReminderDate);

			this.Host.UpdateDirtyStatus(true);
		}
		#endregion //SetActivityReminderIntervalFromReminderDateTime

		#endregion //Private Methods

		#region Internal methods

		#region DisconnectEventListeners
		internal void DisconnectEventListeners()
		{
			if (false == this._subscribedToElementEvents)
				return;

			if (this.TimePickerProxy != null)
			{
				this.TimePickerProxy.LostFocus		-= new ComboBoxProxy.ProxyLostFocusEventHandler(TimePicker_LostFocus);
				this.TimePickerProxy.DropDownClosed -= new ComboBoxProxy.ProxyDropDownClosedEventHandler(TimePicker_DropDownClosed);
				this.TimePickerProxy.DropDownOpened -= new ComboBoxProxy.ProxyDropDownOpenedEventHandler(TimePicker_DropDownOpened);
				this.TimePickerProxy.KeyUp			-= new KeyEventHandler(TimePicker_KeyUp);
			}

			if (this.DatePicker != null)
				this.DatePicker.SelectedDateChanged -= new EventHandler<SelectionChangedEventArgs>(DatePicker_SelectedDateChanged);

			this.Activity.PropertyChanged			-= new System.ComponentModel.PropertyChangedEventHandler(Activity_PropertyChanged);

			this._subscribedToElementEvents = false;
		}
		#endregion //DisconnectEventListeners

		#endregion //Internal Methods

		#endregion //Methods

		#region Event Handlers

		#region Activity_PropertyChanged
		void Activity_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ReminderEnabled")
				this.DatePicker.IsEnabled = this.TimePickerProxy.IsEnabled = this.Activity.ReminderEnabled;
		}
		#endregion //Activity_PropertyChanged

		#region DatePicker_SelectedDateChanged
		void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
		{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


			this.ReminderDate = ScheduleUtilities.CombineDateAndTime(this.DatePicker.SelectedDate.Value, 
																	 TimePickerManager.GetTimePickerCurrentTime(this.TimePickerProxy).Value);			

			this.InitializeTimes();
			this.SetActivityReminderIntervalFromReminderDateTime();
		}
		#endregion //DatePicker_SelectedDateChanged

		#region TimePicker_KeyUp
		void TimePicker_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				this.ParseTimePickerText(true);
			else
				this.Host.UpdateDirtyStatus(true);
		}
		#endregion //TimePicker_KeyUp

		#region TimePicker_LostFocus
		void TimePicker_LostFocus(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(this.TimePickerProxy.Text))
				this.TimePickerProxy.Text = this._lastTimePickerText;

			this.ParseTimePickerText(true);
		}
		#endregion //TimePicker_LostFocus

		#region TimePicker_DropDownClosed
		void TimePicker_DropDownClosed(object sender, EventArgs e)
		{
			if (this.TimePickerProxy.SelectedIndex != this._lastTimePickerSelectedIndex)
				this.ParseTimePickerText(true, this.TimePickerProxy.SelectedIndex);
		}
		#endregion //TimePicker_DropDownClosed

		#region TimePicker_DropDownOpened
		void TimePicker_DropDownOpened(object sender, EventArgs e)
		{
			this._lastTimePickerSelectedIndex = this.TimePickerProxy.SelectedIndex;
		}
		#endregion //TimePicker_DropDownOpened

		#endregion //Event Handlers
	}

	#region Interface IReminderDatePickerManagerHost
	internal interface IReminderDatePickerManagerHost
	{
		DateTime		StartAsLocal { get; }
		TimeZoneToken	ActivityStartTimeZoneTokenResolved { get; }

		void UpdateDirtyStatus(bool isDirty);
	}
	#endregion //Interface IReminderDatePickerManagerHost
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