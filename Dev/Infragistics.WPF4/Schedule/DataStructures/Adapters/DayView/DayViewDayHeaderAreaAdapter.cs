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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Infragistics.Collections;
using System.Diagnostics;

namespace Infragistics.Controls.Schedules.Primitives
{
	internal class DayViewDayHeaderAreaAdapter : CalendarGroupAdapterBase<DayViewDayHeaderArea>
	{
		#region Member Variables

		private ObservableCollectionExtended<DayViewDayHeaderAdapter> _headers;
		private TimeslotCollection _allDayTimeslots;
		private AdapterActivitiesProvider _activityProvider;
		private Visibility _allDayVisibility;
		private Action<TimeslotBase> _allDayInitializer;

		#endregion // Member Variables

		#region Constructor
		internal DayViewDayHeaderAreaAdapter(XamDayView control, CalendarGroupBase calendarGroup) : base(control, calendarGroup)
		{
			_headers = new ObservableCollectionExtended<DayViewDayHeaderAdapter>();

			_allDayInitializer = new XamDayView.AllDayItemInitializer(control, calendarGroup).Initialize;

			if (null != calendarGroup)
			{
				_activityProvider = new AdapterActivitiesProvider(control, this, calendarGroup);
				_activityProvider.ActivityTypes = control.SupportedActivityTypes;
				_activityProvider.ActivityFilter = control.FilterOutNonMultiDayActivity;
			}
		}
		#endregion // Constructor

		#region Base class overrides

		#region CreateInstanceOfRecyclingElement
		protected override DayViewDayHeaderArea CreateInstanceOfRecyclingElement()
		{
			return new DayViewDayHeaderArea();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region OnElementAttached
		protected override void OnElementAttached(DayViewDayHeaderArea element)
		{
			base.OnElementAttached(element);

			element.Items = _headers;
			element.Timeslots = _allDayTimeslots;
			element.ActivityProvider = _activityProvider;
			element.MultiDayActivityAreaVisibility = _allDayVisibility;

			if (null != _activityProvider)
				_activityProvider.OnAttachedElementChanged();
		}
		#endregion // OnElementAttached

		#region OnElementReleased
		protected override void OnElementReleased(DayViewDayHeaderArea element)
		{
			base.OnElementReleased(element);

			element.Items = null;
			element.Timeslots = null;
			element.ActivityProvider = null;

			if (null != _activityProvider)
				_activityProvider.OnAttachedElementChanged();
		}
		#endregion // OnElementReleased

		#region OnTodayChanged
		internal override void OnTodayChanged(DateTime? today)
		{
			for (int i = 0, count = _headers.Count; i < count; i++)
			{
				DayViewDayHeaderAdapter header = _headers[i];
				header.IsToday = header.Date == today;
			}

			if (null != _allDayTimeslots)
				_allDayTimeslots.Reinitialize();

			base.OnTodayChanged(today);
		} 
		#endregion // OnTodayChanged

		#endregion // Base class overrides

		#region Properties

		#region ActivityProvider
		internal AdapterActivitiesProvider ActivityProvider
		{
			get { return _activityProvider; }
		}
		#endregion // ActivityProvider

		#region AllDayAreaVisibility
		public Visibility AllDayAreaVisibility
		{
			get { return _allDayVisibility; }
			set
			{
				if (value != _allDayVisibility)
				{
					_allDayVisibility = value;

					bool useArea = value != Visibility.Collapsed;

					// if we have an activity provider then clear its results and 
					// make sure it won't query then if anyone asks
					if (_activityProvider != null)
						_activityProvider.IsEnabled = useArea;

					if (!useArea)
						_allDayTimeslots = null;

					DayViewDayHeaderArea area = this.AttachedElement as DayViewDayHeaderArea;

					if (area != null)
					{
						area.MultiDayActivityAreaVisibility = value;
						area.Timeslots = _allDayTimeslots;
						area.ActivityProvider = _activityProvider;
					}
				}
			}
		}
		#endregion // AllDayAreaVisibility

		#region AllDayTimeslots
		internal TimeslotCollection AllDayTimeslots
		{
			get { return _allDayTimeslots; }
		} 
		#endregion // AllDayTimeslots

		#region Headers
		internal ObservableCollection<DayViewDayHeaderAdapter> Headers
		{
			get { return _headers; }
		}

		#endregion // Headers

		#endregion // Properties

		#region Methods

		#region VerifyState
		internal void VerifyState(XamDayView control)
		{
			DateInfoProvider dateProvider = ScheduleUtilities.GetDateInfoProvider(control);
			
			Dictionary<DateTime, DayViewDayHeaderAdapter> oldItems = new Dictionary<DateTime, DayViewDayHeaderAdapter>();

			foreach (DayViewDayHeaderAdapter item in _headers)
				oldItems[item.Date] = item;

			_headers.BeginUpdate();
			_headers.Clear();

			DateCollection dates = control.VisibleDates;
			List<DateRange> ranges = new List<DateRange>();
			DateRange range = DateRange.Infinite;
			TimeSpan dayDuration, dayOffset;

			control.GetLogicalDayInfo(out dayOffset, out dayDuration);

			for (int i = 0, count = dates.Count; i < count; i++)
			{
				DateTime date = dates[i];
				DateTime? start = dateProvider.Add(date, dayOffset);
				DateTime? end = start != null ? dateProvider.Add(start.Value, dayDuration) : null;

				if (end == null)
					break;

				DateTime logicalDate = start.Value;

				
				if (i == 0)
				{
					range = new DateRange(logicalDate, end.Value);
				}
				else if (range.End == logicalDate)
				{
					range.End = end.Value;
				}
				else
				{
					ranges.Add(range);
					range = new DateRange(logicalDate, end.Value);
				}

				DayViewDayHeaderAdapter item;

				if (!oldItems.TryGetValue(date, out item))
					item = new DayViewDayHeaderAdapter(date);

				_headers.Add(item);
			}

			if (dates.Count > 0)
				ranges.Add(range);

			if (_allDayVisibility != Visibility.Collapsed)
			{
				_allDayTimeslots = new TimeslotCollection(control.CreateAllDayItem, null, ScheduleUtilities.CreateRangeArray(ranges, dayDuration), _allDayInitializer);
				_activityProvider.ActivityTypes = control.SupportedActivityTypes;

				var token = control.TimeZoneInfoProviderResolved.LocalToken;

				for (int i = 0, count = ranges.Count; i < count; i++)
				{
					ranges[i] = ScheduleUtilities.ConvertToUtc(token, ranges[i]);
				}

				_activityProvider.Ranges = ranges.ToArray();
			}

			if (this.AttachedElement != null)
				((DayViewDayHeaderArea)this.AttachedElement).Timeslots = _allDayTimeslots;

			_headers.EndUpdate();
		}
		#endregion // VerifyState

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