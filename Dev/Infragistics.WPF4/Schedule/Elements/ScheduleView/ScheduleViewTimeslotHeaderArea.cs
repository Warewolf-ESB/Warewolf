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
using Infragistics.Collections;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Custom <see cref="TimeslotHeaderArea"/> used within a <see cref="XamScheduleView"/>
	/// </summary>
	[TemplatePart(Name = PartTimeslotGroupPanel, Type = typeof(TimeslotGroupPanel))]
	public class ScheduleViewTimeslotHeaderArea : TimeslotHeaderArea
		, TimeslotGroupPanel.ITimeslotGroupProvider
	{
		#region Member Variables

		private const string PartTimeslotGroupPanel = "TimeslotGroupPanel";
		private TimeslotGroupPanel _timeslotGroupPanel;
		private int _cachedPositionVersion = -1;
		private TimeSpan _logicalOffset;

		#endregion //Member Variables

		#region Constructor
		static ScheduleViewTimeslotHeaderArea()
		{

			ScheduleViewTimeslotHeaderArea.DefaultStyleKeyProperty.OverrideMetadata(typeof(ScheduleViewTimeslotHeaderArea), new FrameworkPropertyMetadata(typeof(ScheduleViewTimeslotHeaderArea)));

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleViewTimeslotHeaderArea"/>
		/// </summary>
		public ScheduleViewTimeslotHeaderArea()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_timeslotGroupPanel != null)
			{
				_timeslotGroupPanel.SetIsAttachedElement(false);
				_timeslotGroupPanel.ClearValue(ScheduleControlBase.ControlProperty);
				_timeslotGroupPanel.ClearValue(TimeslotPanel.TimeslotOrientationProperty);
				_timeslotGroupPanel.Timeslots = null;
			}

			_timeslotGroupPanel = this.GetTemplateChild(PartTimeslotGroupPanel) as TimeslotGroupPanel;

			if (null != _timeslotGroupPanel)
			{
				var ctrl = ScheduleControlBase.GetControl(this);
				_timeslotGroupPanel.SetValue(ScheduleControlBase.ControlProperty, ctrl);
				_timeslotGroupPanel.TimeslotOrientation = ctrl != null ? ctrl.TimeslotAreaTimeslotOrientation : Orientation.Horizontal;
				_timeslotGroupPanel.Timeslots = this.Timeslots;
				_timeslotGroupPanel.GroupProvider = this;
				_timeslotGroupPanel.SetIsAttachedElement(null != this.Timeslots);
			}
		}
		#endregion //OnApplyTemplate

		#region OnTimeslotsChanged
		internal override void OnTimeslotsChanged(TimeslotCollection oldValue, TimeslotCollection newValue)
		{
			base.OnTimeslotsChanged(oldValue, newValue);

			if ( _timeslotGroupPanel != null )
			{
				_timeslotGroupPanel.Timeslots = newValue;
				_timeslotGroupPanel.SetIsAttachedElement(newValue != null);
			}

			// consider the group information invalid since the date times have changed
			_cachedPositionVersion--;
		} 
		#endregion // OnTimeslotsChanged

		#region RefreshDisplay
		internal override void RefreshDisplay()
		{
			base.RefreshDisplay();

			ScheduleUtilities.RefreshDisplay(_timeslotGroupPanel);
		}
		#endregion // RefreshDisplay

		#endregion //Base class overrides

		#region Methods

		#region GetDates
		private IEnumerable<DateTime> GetDates(DateCollection dates, Func<DateTime, DateTime> modifyDateFunc, DateInfoProvider dateProvider)
		{
			DateTime? previousDate = null;

			for (int i = 0, count = dates.Count; i < count; i++)
			{
				DateTime date = dates[i];

				date = modifyDateFunc(date);

				if (date.TimeOfDay == TimeSpan.Zero)
					yield return date;
				else
				{
					date = date.Date;

					if (date != previousDate)
						yield return date;

					DateTime? newDate = dateProvider.AddDays(date, 1);

					if (newDate.HasValue)
						date = newDate.Value;
					else
						break;
 
					previousDate = date;

					yield return date;
				}
			}
		}
		#endregion // GetDates

		#endregion // Methods

		#region ITimeslotGroupProvider Members

		void TimeslotGroupPanel.ITimeslotGroupProvider.InitializeGroups(TimeslotGroupPanel.GroupMeasureInfo measureInfo)
		{
			if (_cachedPositionVersion != measureInfo.PositionInfo.Version || _logicalOffset != measureInfo.Panel.LogicalDayOffset)
			{
				this.InitializeGroupsImpl(measureInfo);
			}
		}

		private void InitializeGroupsImpl(TimeslotGroupPanel.GroupMeasureInfo measureInfo)
		{
			var control = ScheduleControlBase.GetControl(this);

			if (control != null)
			{
				DateInfoProvider dateProvider = ScheduleUtilities.GetDateInfoProvider(control);
				TimeslotGroupPanel panel = measureInfo.Panel;

				_cachedPositionVersion = measureInfo.PositionInfo.Version;
				_logicalOffset = panel.LogicalDayOffset;
				TimeSpan dayOffset = _logicalOffset;

				var oldItems = new Dictionary<DateTime, TimeslotGroupPanel.Group>();

				foreach (TimeslotGroupPanel.Group group in measureInfo.Groups)
				{
					ScheduleViewDayHeaderAdapter adapter = group.Item as ScheduleViewDayHeaderAdapter;
					Debug.Assert(null != adapter, string.Format("Unexpected item type: {0}", group.Item));
					oldItems[adapter.Date] = group;
				}

				measureInfo.Groups.Clear();

				// this could be for an alternate timezone
				Func<DateTime, DateTime> modifyDateFunc = this.Timeslots != null ? this.Timeslots.ModifyDateFunc : null;

				DateCollection visibleDates = control.VisibleDates;

				// make sure the dates are sorted
				visibleDates.Sort();

				IEnumerable<DateTime> dates = modifyDateFunc == null
					? visibleDates
					: GetDates(visibleDates, modifyDateFunc, dateProvider);

				TimeSpan dayOffsetEnd = dayOffset.Add(TimeSpan.FromTicks(TimeSpan.TicksPerDay));
				bool verifyTimeslotDates = dayOffset.Ticks != 0 || TimeSpan.TicksPerDay % measureInfo.PositionInfo.TimeslotIntervalTicks != 0;

				foreach (DateTime date in dates)
				{
					TimeslotGroupPanel.Group group;

					if (!oldItems.TryGetValue(date, out group))
					{
						ScheduleViewDayHeaderAdapter adapter = new ScheduleViewDayHeaderAdapter(date);
						group = new TimeslotGroupPanel.Group(adapter);
					}

					DateTime? start = dateProvider.Add(date, dayOffset);

					// if we couldn't get that date then presumably we're on the border
					// and should start with the min supported time
					if (start == null)
					{
						if (dayOffset.Ticks <= 0)
							start = dateProvider.MinSupportedDateTime;
						else
							break;
					}

					// since the start may have been adjusted to the MinSupportedDateTime
					// we will just add 1 day to the date plus the day offset. if that is 
					// beyond the allowed time then we'll use the end as our cap
					DateTime? end = dateProvider.Add(date, dayOffsetEnd) ?? dateProvider.MaxSupportedDateTime;

					if (end <= start)
						break;

					if (verifyTimeslotDates)
					{
						// since we can have non-integral logical day start offset's we have to validate the dates
						// so we go to the end of the timeslot if we're in the middle of one
						int startIndex, endIndex;
						panel.GetTimeslotIndex(start.Value, end.Value, out startIndex, out endIndex);

						if (startIndex < 0)
							startIndex = ~startIndex;

						DateRange startRange = panel.GetTimeslotTime(startIndex);

						// if the calculated start isn't the same as the start of the timeslot then use
						// its non-inclusive end (which should be the inclusive start of the next)
						if (startRange.Start < start)
							start = startRange.End;

						if (endIndex < 0)
						{
							// if we don't have a timeslot for the end we need to invert
							// it and use the previous slot since this would return the 
							// binary search result which would be the index at which 
							// the item would be inserted.
							endIndex = ~endIndex;
							endIndex--;
						}

						DateRange endRange = panel.GetTimeslotTime(endIndex);

						if (endRange.Start < end)
							end = endRange.End;
						else
							end = endRange.Start;
					}

					group.Start = start.Value;
					group.End = end.Value;

					measureInfo.Groups.Add(group);
				}
			}
		}
		#endregion //ITimeslotGroupProvider Members
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