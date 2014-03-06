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
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal static class AppointmentDialogUtilities
	{
		#region Member Variables

		[ThreadStatic]
		private static List<ReminderListItem> _reminderListItems;

		#endregion //Member Variables

		#region GetAppointmentDialogCoreFromElement
		internal static AppointmentDialogCore GetAppointmentDialogCoreFromElement(object element)
		{
			AppointmentDialogCore adc = null;

			if (element is FrameworkElement)
				adc = ((FrameworkElement)element).DataContext as AppointmentDialogCore;

			return adc;
		}
		#endregion //GetAppointmentDialogCoreFromElement

		#region GetReminderListItems
		internal static List<ReminderListItem> GetReminderListItems()
		{
			if (_reminderListItems != null)
				return _reminderListItems;

			_reminderListItems = new List<ReminderListItem>(25);

			_reminderListItems.Add(new ReminderListItem(null));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(0, 5, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(0, 10, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(0, 15, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(0, 30, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(1, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(2, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(3, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(4, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(5, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(6, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(7, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(8, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(9, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(10, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(11, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(12, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(18, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(1, 0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(2, 0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(3, 0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(4, 0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(7, 0, 0, 0)));
			_reminderListItems.Add(new ReminderListItem(new TimeSpan(14, 0, 0, 0)));

			return _reminderListItems;
		}
		#endregion //GetReminderListItems

		#region ParseReminderText
		internal static void ParseReminderText(DependencyObject sender, DependencyProperty sendersTextProperty, string textToParse, bool forceUpdate)
		{
			AppointmentDialogCore adc = GetAppointmentDialogCoreFromElement(sender);

			// Pull the number (if any) out of the text.
			string			s				= textToParse.Trim().ToLower();
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

			// If there was no number, check to see if the 'None' text is present.  If so, update the appointment
			// by removing the reminder.  Otherwise, revert the text to what it was.
			double number;
			if (numberStart == -1 || false == double.TryParse(numberAsString.ToString(), out number))
			{
				string noneText = ScheduleUtilities.GetString("DLG_Appointment_Reminder_None");
				if (s.Contains(noneText.ToLower()))
				{
					UpdateTextAndAppointment(sender, sendersTextProperty, adc, noneText, null, forceUpdate);

					return;
				}

				RevertReminderText(sender, sendersTextProperty, adc);
				return;
			}

			// Since we have a number, look at the rest of the text to see if it contains any of the
			// time literals (e.g., minute, day week etc).  if so, create the appropriate timespan and
			// update the appointment reminder
			TimeSpan	timeSpan;
			if (s.Contains(ScheduleUtilities.GetString("DLG_Appointment_Reminder_Literal_Minute").ToLower()))
				timeSpan = TimeSpan.FromMinutes(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Appointment_Reminder_Literal_Hour").ToLower()))
				timeSpan = TimeSpan.FromHours(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Appointment_Reminder_Literal_Day").ToLower()))
				timeSpan = TimeSpan.FromDays(number);
			else if (s.Contains(ScheduleUtilities.GetString("DLG_Appointment_Reminder_Literal_Week").ToLower()))
				timeSpan = TimeSpan.FromDays(number * 7);
			else
				timeSpan = TimeSpan.FromHours(number);

			string normalizedString = DurationListItem.DurationStringFromTimeSpan(timeSpan);
			UpdateTextAndAppointment(sender, sendersTextProperty, adc, normalizedString, timeSpan, forceUpdate);
		}
		#endregion //ParseReminderText

		#region RevertReminderText
		internal static void RevertReminderText(DependencyObject sender, DependencyProperty sendersTextProperty, AppointmentDialogCore adc)
		{
			if (adc != null)
			{
#pragma warning disable 436
				string text = SR.GetString("DLG_Appointment_Reminder_None");
#pragma warning restore 436

				if (true == adc.Appointment.ReminderEnabled)
					text = DurationListItem.DurationStringFromTimeSpan(adc.Appointment.ReminderInterval);

				sender.SetValue(sendersTextProperty, text);
			}
		}
		#endregion //RevertReminderText

		#region UpdateDirtyStatus
		internal static void UpdateDirtyStatus(object sender)
		{
			AppointmentDialogCore adc = GetAppointmentDialogCoreFromElement(sender);
			if (adc != null)
				adc.UpdateDirtyStatus(true);
		}
		#endregion //UpdateDirtyStatus

		#region UpdateTextAndAppointment
		private static void UpdateTextAndAppointment(DependencyObject sender, DependencyProperty sendersTextProperty, AppointmentDialogCore adc, string newText, TimeSpan? newTimeSpan, bool forceUpdate)
		{
			sender.SetValue(sendersTextProperty, newText);

			bool updateDirtyStatus = false;
			if (adc != null)
			{
				if (newTimeSpan == null)
				{
					if (adc.Appointment.ReminderEnabled)
					{
						adc.Appointment.ReminderEnabled	= false;
						updateDirtyStatus				= true;
					}
				}
				else if (false == adc.Appointment.ReminderEnabled)
				{
					adc.Appointment.ReminderEnabled		= true;
					adc.Appointment.ReminderInterval	= newTimeSpan.Value;
					updateDirtyStatus					= true;
				}
				else
				{
					if (adc.Appointment.ReminderInterval != newTimeSpan.Value)
					{
						adc.Appointment.ReminderInterval	= newTimeSpan.Value;
						updateDirtyStatus					= true;
					}
				}
			}

			if (updateDirtyStatus || forceUpdate)
				UpdateDirtyStatus(sender);
		}

		#endregion //UpdateTextAndAppointment
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