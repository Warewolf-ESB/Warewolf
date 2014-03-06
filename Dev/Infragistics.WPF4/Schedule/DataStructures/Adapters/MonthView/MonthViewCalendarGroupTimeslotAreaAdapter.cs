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

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class MonthViewCalendarGroupTimeslotAreaAdapter : CalendarGroupTimeslotAreaAdapter
	{
		#region Member Variables

		private ObservableCollection<MonthViewDayOfWeekHeaderAdapter> _daysOfWeek;
		private DayOfWeek[] _daysOfWeekSource;
		private XamMonthView _control;

		#endregion // Member Variables

		#region Constructor
		internal MonthViewCalendarGroupTimeslotAreaAdapter(XamMonthView control, CalendarGroupBase calendarGroup, IList<TimeslotAreaAdapter> timeSlotGroups) : base(control, calendarGroup, timeSlotGroups)
		{
			_control = control;
			_daysOfWeek = new ObservableCollection<MonthViewDayOfWeekHeaderAdapter>();
			this.DaysOfWeekSource = control.WeekHelper.DaysOfWeekList;
		} 
		#endregion //Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override CalendarGroupTimeslotArea CreateInstanceOfRecyclingElement()
		{
			return new MonthViewCalendarGroupTimeslotArea();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(CalendarGroupTimeslotArea element)
		{
			base.OnElementAttached(element);

			var monthGroupArea = element as MonthViewCalendarGroupTimeslotArea;
			monthGroupArea.DayOfWeekHeaders = _daysOfWeek;
			monthGroupArea.SetValue(MonthViewCalendarGroupTimeslotArea.WeekHeaderWidthPropertyKey, _control.WeekHeaderWidthObject);
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		protected override void OnElementReleased(CalendarGroupTimeslotArea element)
		{
			base.OnElementReleased(element);

			var monthGroupArea = element as MonthViewCalendarGroupTimeslotArea;
			monthGroupArea.DayOfWeekHeaders = null;
		}
		#endregion // OnElementReleased

		#endregion // Base class overrides

		#region Properties
		
		#region DaysOfWeekSource
		internal DayOfWeek[] DaysOfWeekSource
		{
			get { return _daysOfWeekSource; }
			set
			{
				if (value != _daysOfWeekSource)
				{
					_daysOfWeekSource = value;
					this.InitializeDaysOfWeek();
				}
			}
		}
		#endregion // DaysOfWeekSource

		#endregion // Properties
		
		#region Methods

		#region InitializeDaysOfWeek
		private void InitializeDaysOfWeek()
		{
			int count = _daysOfWeekSource == null ? 0 : _daysOfWeekSource.Length;

			// remove any we won't reuse
			for (int i = count, currentCount = _daysOfWeek.Count; i < currentCount; i++)
				_daysOfWeek.RemoveAt(count);

			for (int i = 0; i < count; i++)
			{
				var dow = _daysOfWeekSource[i];

				if (i == _daysOfWeek.Count)
					_daysOfWeek.Add(new MonthViewDayOfWeekHeaderAdapter(dow));
				else
					_daysOfWeek[i].DayOfWeek = dow;
			}
		}
		#endregion // InitializeDaysOfWeek

		#region InitializeWeekHeaderWidth
		internal void InitializeWeekHeaderWidth(object width)
		{
			var element = this.AttachedElement as MonthViewCalendarGroupTimeslotArea;

			if (null != element)
			{
				element.SetValue(MonthViewCalendarGroupTimeslotArea.WeekHeaderWidthPropertyKey, width);

				if (element.ItemsPanel != null)
				{
					foreach (var child in element.ItemsPanel.Children)
					{
						var timeslotArea = child as MonthViewTimeslotArea;

						if (null != timeslotArea)
							timeslotArea.SetValue(MonthViewTimeslotArea.WeekHeaderWidthPropertyKey, width);
					}
				}
			}
		} 
		#endregion // InitializeWeekNumberWidth

		#endregion // Methods
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