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
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class DurationListItem
	{
		#region Member Variables

		private string									_displayString;

		#endregion //Member Variables

		#region Constructor
		internal DurationListItem(TimeSpan? timeSpan)
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
				    this._displayString = DurationStringFromTimeSpan(this.TimeSpan);

				return this._displayString;
			}
		}
		#endregion //DisplayString

		#region TimeSpan
		internal TimeSpan? TimeSpan
		{
			get;
			set;
		}
		#endregion //TimeSpan

		#endregion //Properties

		#region Methods

		#region DivRem (static)
		private static int DivRem(int a, int b, out int rem)
		{
			int c	= a / b;
			rem		= a - (b * c);

			return c;
		}
		#endregion DivRem (static)

		#region DurationStringFromTimeSpan
		internal static string DurationStringFromTimeSpan(TimeSpan? timeSpan)
		{
			string timeSpanString = ScheduleUtilities.GetString("DLG_Recurrence_Duration_None"); ;
			if (timeSpan != null)
			{
				if (timeSpan.Value.TotalMinutes == 0)
					return string.Format(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Minutes"), 0);

				int minutesPerHour	= 60;
				int minutesPerDay	= minutesPerHour * 24;
				int minutesPerWeek	= minutesPerDay * 7;
				int totalMinutes	= Convert.ToInt32(timeSpan.Value.TotalMinutes);
				int rem;

				double weeks = 0d, days = 0d, hours = 0d, minutes = 0d;
				while (true)
				{
					// WEEKS
					// Can we show this timespan in weeks?  (i.e., is the time span >= 1 week and is it exactly N weeks
					// or N.5 weeks)
					if (timeSpan.Value.TotalDays > 6)
					{
						int weeksInt = DurationListItem.DivRem(totalMinutes, minutesPerWeek, out rem);
						if (rem == 0)
						{
							weeks = weeksInt;
							break;
						}
						else
						if (rem == (minutesPerWeek * .5))
						{
							weeks = (double)weeksInt + .5;
							break;
						}
					}

					// DAYS
					// Can we show this time span in days? (i.e., is the time span >= 1 day and is it exactly N days
					// or N.1, N.2, N.25, N.3, N.4, N.5, N.6, N.7, N.75, N.8, N.9 days)
					if (timeSpan.Value.TotalDays >= .5)
					{
						int daysInt = DurationListItem.DivRem(totalMinutes, minutesPerDay, out rem);
						if (rem == 0)
						{
							days = daysInt;
							break;
						}
						else
						{
							double[] pcts = { .1d, .2d, .25d, .3d, .4d, .5d, .6d, .7d, .75d, .8d, .9d };
							for (int i = 0; i < pcts.Length; i++)
							{
								if (rem == (minutesPerDay * pcts[i]))
								{
									days = (double)daysInt + pcts[i];
									break;
								}
							}

							if (days != 0)
								break;
						}
					}

					// HOURS
					// Can we show this time span in hours? (i.e., is the time span >= 1 hour and is it exactly N hours
					// or N.1, N.2, N.25, N.3, N.4, N.5, N.6, N.7, N.75, N.8, N.9 hours)
					if (timeSpan.Value.TotalHours >= 1)
					{
						int hoursInt = DurationListItem.DivRem(totalMinutes, minutesPerHour, out rem);
						if (rem == 0)
						{
							hours = hoursInt;
							break;
						}
						else
						{
							double [] pcts = {.1d, .2d, .25d, .3d, .4d, .5d, .6d, .7d, .75d, .8d, .9d};
							for (int i = 0; i < pcts.Length; i++)
							{
								if (rem == (minutesPerHour * pcts[i]))
								{
									hours = (double)hoursInt + pcts[i];
									break;
								}
							}

							if (hours != 0)
								break;
						}
					}

					// MINUTES
					// Default to expressing the time span in minutes.
					minutes = totalMinutes;
					break;
				}
				

				if (weeks != 0)
				{
					if (weeks == 1)
						return ScheduleUtilities.GetString("DLG_Recurrence_Duration_OneWeek");
					else
						return string.Format(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Weeks"), weeks);
				}

				if (days != 0)
				{
					if (days == 1)
						return ScheduleUtilities.GetString("DLG_Recurrence_Duration_OneDay");
					else
						return string.Format(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Days"), days);
				}

				if (hours != 0)
				{
					if (hours == 1)
						return ScheduleUtilities.GetString("DLG_Recurrence_Duration_OneHour");
					else
						return string.Format(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Hours"), hours);
				}

				if (minutes != 0)
				{
					if (minutes == 1)
						return ScheduleUtilities.GetString("DLG_Recurrence_Duration_OneMinute");
					else
						return string.Format(ScheduleUtilities.GetString("DLG_Recurrence_Duration_Minutes"), minutes);
				}


#pragma warning restore 436
			}

			return timeSpanString;
		}
		#endregion //DurationStringFromTimeSpan

		#endregion //Methods
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