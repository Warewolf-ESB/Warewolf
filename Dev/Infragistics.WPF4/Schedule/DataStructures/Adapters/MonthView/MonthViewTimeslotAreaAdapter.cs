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
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class MonthViewTimeslotAreaAdapter : TimeslotAreaAdapter
	{
		#region Member Variables

		private List<MonthViewDayHeaderAdapter> _headers;
		private int _weekNumber;
		private XamMonthView _control;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="TimeslotAreaAdapter"/>
		/// </summary>
		/// <param name="control">The associated control</param>
		/// <param name="start">The logical starting date for the item</param>
		/// <param name="end">The logical ending date for the item</param>
		/// <param name="firstSlotStart">The time for the first timeslot</param>
		/// <param name="lastSlotEnd">The time for the last timeslot</param>
		/// <param name="timeslots">A collection of <see cref="Timeslot"/> instances</param>
		/// <param name="activityOwner">The calendar group for which activity will be generated</param>
		/// <param name="weekNumber">Week number that the object represents</param>
		internal MonthViewTimeslotAreaAdapter(XamMonthView control,
			DateTime start, DateTime end,
			DateTime firstSlotStart, DateTime lastSlotEnd,
			TimeslotCollection timeslots, CalendarGroupBase activityOwner, int weekNumber)
			: base(control, start, end, firstSlotStart, lastSlotEnd, timeslots, activityOwner)
		{
			_headers = new List<MonthViewDayHeaderAdapter>();
			_weekNumber = weekNumber;
			_control = control;

			TimeSpan dayDuration, dayOffset;
			control.GetLogicalDayInfo(out dayOffset, out dayDuration);

			for (int i = 0, count = timeslots.Count; i < count; i++)
			{
				DateTime logicalStart = timeslots.GetDateRange(i).Value.Start;
				logicalStart = logicalStart.Subtract(dayOffset);
				Debug.Assert(logicalStart.TimeOfDay.Ticks == 0, "Still have an offset?");

				MonthDayType dayType = control.GetDayType(logicalStart);

				_headers.Add(new MonthViewDayHeaderAdapter(logicalStart, dayType));
			}
		}
		#endregion // Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override TimeslotArea CreateInstanceOfRecyclingElement()
		{
			return new MonthViewTimeslotArea();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(TimeslotArea element)
		{
			var mvElement = element as MonthViewTimeslotArea;

			mvElement.Start = this.Start;
			mvElement.End = this.End;
			mvElement.WeekNumber = _weekNumber;
			mvElement.SetValue(MonthViewTimeslotArea.WeekHeaderWidthPropertyKey, _control.WeekHeaderWidthObject);

			base.OnElementAttached(element);
		} 
		#endregion // OnElementAttached

		#region OnTodayChanged
		internal override void OnTodayChanged(DateTime? today)
		{
			foreach (var item in _headers)
			{
				item.IsToday = today == item.Date;
			}
		}
		#endregion // OnTodayChanged

		#region RecyclingElementType
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(MonthViewTimeslotArea);
			}
		}
		#endregion // RecyclingElementType

		#endregion // Base class overrides

		#region Properties

		#region DayHeaders
		internal List<MonthViewDayHeaderAdapter> DayHeaders
		{
			get { return _headers; }
		}
		#endregion // DayHeaders

		#endregion // Properties
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