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
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;
using System.Windows.Threading;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Editors;


namespace Infragistics.Controls.Schedules
{
	internal class TimeSelectionDragController : DragControllerBase
	{
		#region Private Members

		private CalendarGroupBase _restrictToGroup;
		private DateTime _start;
		private DateTime _end;
		private bool _dragStarted;
		private TimeSpan _logicalDayOffset;

		#endregion //Private Members	

		#region Constructor

		internal TimeSelectionDragController(ScheduleControlBase control, FrameworkElement rootElement, TimeRangePresenterBase sourceElement)
			: base(control, rootElement, sourceElement)
		{
			_logicalDayOffset = ScheduleUtilities.GetLogicalDayOffset(_control);
			var range = this.GetTimeslotRange(sourceElement);

			_start = range.Start;
			_end = range.End;

			// for day view headers we'll use the active group. 
			if (sourceElement is TimeslotHeader && sourceElement.Kind == TimeRangeKind.TimeHeader)
			{
				Debug.Assert(ScheduleUtilities.GetCalendarGroupFromElement(sourceElement) == null, "This is a header but we have an associated group?");
				Debug.Assert(control.ActiveGroup != null || control.VisibleCalendarCount == 0, "No active group?");
				_restrictToGroup = control.ActiveGroup;
			}
			else if (control is XamScheduleView)
			{
				// do not restrict in schedule view since all the items are in a line
			}
			else
			{
				_restrictToGroup = ScheduleUtilities.GetCalendarGroupFromElement(sourceElement);
				Debug.Assert(_restrictToGroup != null, "Group could not be found");
			}
		}

		#endregion //Constructor	

		#region Base class overrides

		#region CheckIfScrollIsRequired

		protected override void CheckIfScrollIsRequired()
		{
			Debug.Assert(_canScroll);

			double pointValue;

			if (this._isVertical)
				pointValue = this._pointInTimeRangeArea.Y;
			else
				pointValue = this._pointInTimeRangeArea.X;

			bool scrollUp;
			if (pointValue < ScrollMargin)
				scrollUp = true;
			else
				if (pointValue > this._timeRangeExtent - ScrollMargin)
					scrollUp = false;
				else
				{
					this.KillTimer();
					return;
				}

			long ticks = DateTime.Now.Ticks;

			if (ticks - this._lastScrollTicks > ScrollInterval)
			{
				double oldOffset = _timeslotScrollInfo.Offset;

				if (scrollUp)
					_timeslotScrollInfo.Offset -= 1.0;
				else
					_timeslotScrollInfo.Offset += 1.0;

				this._rootElement.UpdateLayout();
				this.ProcessNewMousePosition(this._mousePosition, true);

				if (oldOffset == _timeslotScrollInfo.Offset)
				{
					this.KillTimer();
					return;
				}
			}

			this._lastScrollTicks = ticks;

			this.StartTimer();
		}

		#endregion //CheckIfScrollIsRequired

		#region OnMouseMove

		protected override void OnMouseMove(object sender, MouseEventArgs e)
		{

			e.Handled = true;
			Point originalMousePosition = e.GetPosition(this._rootElement);



			this._mousePosition = originalMousePosition;

			this.ProcessNewMousePosition(this._mousePosition, true);

			if (this._canScroll)
			{
				this._pointInTimeRangeArea = e.GetPosition(this._timeRangeArea);

				// constrain the point so it is just within the margins
				// in other words if it is outside the scroll bounds bring it in
				// so that it is enough to trigger scrolling but stille be over
				// a valid timeslot
				double originalValue = this._isVertical ? this._pointInTimeRangeArea.Y : this._pointInTimeRangeArea.X;
				double constrainedValue = originalValue;

				constrainedValue = Math.Min(constrainedValue, this._timeRangeExtent + 1 - ScrollMargin);
				constrainedValue = Math.Max(constrainedValue, ScrollMargin - 1);
				if (this._isVertical)
					this._mousePosition.Y += constrainedValue - originalValue;
				else
					this._mousePosition.X += constrainedValue - originalValue;

				if (originalValue != constrainedValue)
				{
					TimeRangePresenterBase tsp = ScheduleUtilities.GetTimeRangePresenterFromPoint(this._rootElement, originalMousePosition, null) as TimeRangePresenterBase;

					if (tsp != null)
					{
						UIElement elem = PresentationUtilities.GetVisualAncestor<UIElement>(tsp,
									   delegate(UIElement element)
									   {
										   return element is ITimeRangeArea;
									   });

						// special case the all day activity area inside the DayViewDayHeaderArea
						if (elem is DayViewDayHeaderArea)
						{
							_timeslotScrollInfo.Offset = this._originalScrollOffset;

							if (this.ProcessMoveOverMultiDayArea())
							{
								this.KillTimer();
								return;
							}
						}
					}
				}

				this.CheckIfScrollIsRequired();
			}
		}

		#endregion //OnMouseMove

		#region ProcessCancel

		protected override void ProcessCancel()
		{
			this._control.EditHelper.EndTimeSelectionDrag();
		}

		#endregion //ProcessCancel

		#region ProcessCommit

		protected override void ProcessCommit()
		{
			this._control.EditHelper.EndTimeSelectionDrag();
		}

		#endregion //ProcessCommit

		#endregion //Base class overrides

		#region Methods

		#region Internal Methods

		#region BeginDrag

		internal bool BeginDrag()
		{
			Debug.Assert(_dragStarted == false);

			if (_dragStarted)
				return false;
			_dragStarted = true;
			_logicalDayOffset = ScheduleUtilities.GetLogicalDayOffset(_control);

			WireMouseEvents();

			bool captured = this._rootElement.CaptureMouse();

			if (!captured || !this._areEventsWired)
				return false;

			return captured;
		}

		#endregion //BeginDrag

		#region EndDrag

		internal bool EndDrag()
		{
			if (!_dragStarted)
				return false;

			if ( this._areEventsWired )
				this._rootElement.ReleaseMouseCapture();

			return true;
		}

		#endregion //EndDrag

		#endregion //Internal Methods

		#region Private Methods

		#region GetTimeslotRange
		private DateRange GetTimeslotRange(TimeRangePresenterBase tsp)
		{
			return ScheduleUtilities.GetTimeslotRange(tsp, _logicalDayOffset);
		}
		#endregion // GetTimeslotRange

		#region ProcessMoveOverMultiDayArea

		private bool ProcessMoveOverMultiDayArea()
		{
			//if (this._isLeading)
			//{
			//    if (this._areaStart.HasValue)
			//    {
			//        this.Activity.SetStartLocal(this._tzProvider.LocalToken, this._areaStart.Value);
			//        return true;
			//    }
			//}
			//else
			//{
			//    if (this._areaEnd.HasValue)
			//    {
			//        this.Activity.SetEndLocal(_tzProvider.LocalToken, this._areaEnd.Value);

			//        return true;
			//    }
			//}

			return true;
		}

		#endregion //ProcessMoveOverMultiDayArea	

		#region ProcessNewMousePosition

		private bool ProcessNewMousePosition(Point ptInRootPanelCoords, bool calledFromMouseMove)
		{
			// try getting a time range presenter that belongs to this group
			TimeRangePresenterBase tsp = ScheduleUtilities.GetTimeRangePresenterFromPoint(this._rootElement, ptInRootPanelCoords, this._restrictToGroup) as TimeRangePresenterBase;

			if (tsp == null && this._restrictToGroup != null)
			{
				// look for any time range presenter under this point
				tsp = ScheduleUtilities.GetTimeRangePresenterFromPoint(this._rootElement, ptInRootPanelCoords, null) as TimeRangePresenterBase;

				if (tsp != null)
				{
					// only allow timeslot headers from this control to be processed
					if (!(tsp is TimeslotHeader) ||
						ScheduleUtilities.GetControl(tsp) != this._control)
						return false;
				}
			}

			if (tsp != null)
			{
				bool wasDragStartedOverMultiDayArea = this._sourceElement is MultiDayActivityArea;
				bool isOverMultiDayArea = tsp is MultiDayActivityArea;

				DateRange range = this.GetTimeslotRange(tsp);
				DateTime rangeStart = range.Start;
				DateTime rangeEnd = range.End;

				DateTime adjustedStart = this._start;
				DateTime adjustedEnd = this._end;

				if (isOverMultiDayArea != wasDragStartedOverMultiDayArea)
				{
					if (isOverMultiDayArea)
					{
						DateRange logicalRange = ScheduleUtilities.GetLogicalDayRange(this._start, this._logicalDayOffset);
						adjustedStart = logicalRange.Start.Date + rangeStart.TimeOfDay;
						adjustedEnd = logicalRange.Start.Date + rangeEnd.TimeOfDay;
					}
					else
					{
						adjustedEnd = this._start + (rangeEnd - rangeStart);
					}
				}

				if (rangeStart > adjustedStart)
					rangeStart = adjustedStart;

				if (rangeEnd < adjustedEnd)
					rangeEnd = adjustedEnd;

				// maintain the anchor point
				DateRange newRange = rangeStart < _start 
					? new DateRange(rangeEnd, rangeStart) 
					: new DateRange(rangeStart, rangeEnd);

				this._control.SelectedTimeRange = newRange;
				return true;
			}

			return false;
		}

		#endregion //ProcessNewMousePosition

		#endregion //Private Methods

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