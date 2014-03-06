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
	internal class ActivityResizeController : ActivityDragControllerBase
	{
		#region Private Members

		private bool _isLeading;
		private CalendarGroupBase _group;
		private Point? _initialMouseDown;
		private TimeSpan _dayOffset;
		private TimeSpan _dayDuration;
		private DateRange _originalActivityRangeLocal;
		private DateRange _oppositeEdgeDayRange;
		
		#endregion //Private Members
	
		#region Constructor

		internal ActivityResizeController(ScheduleControlBase control, ActivityResizerBar resizerBar, FrameworkElement rootElement, ActivityBase activity) : base( control,rootElement, resizerBar, activity )
		{
			_isLeading = resizerBar.IsLeading;
			control.GetLogicalDayInfo(out _dayOffset, out _dayDuration);

			DateRange activityRange = ScheduleUtilities.ConvertFromUtc(control.TimeZoneInfoProviderResolved.LocalToken, activity);
			DateTime oppositeDate = !_isLeading || activityRange.IsEmpty ? activityRange.Start : ScheduleUtilities.GetNonInclusiveEnd(activityRange.End);
			_oppositeEdgeDayRange = ScheduleUtilities.GetLogicalDayRange(oppositeDate, _dayOffset, _dayDuration);
			_originalActivityRangeLocal = activityRange;

			// AS 1/13/12 TFS77443
			// If the activity is outside the min/max range we'll consider the time within to be valid.
			//
			_minMaxRange.Start = ScheduleUtilities.Min(_minMaxRange.Start, _originalActivityRangeLocal.Start);
			_minMaxRange.End = ScheduleUtilities.Max(_minMaxRange.End, _originalActivityRangeLocal.End);

			if (!(control is XamScheduleView))
			{
				this._group = ScheduleUtilities.GetCalendarGroupFromElement(_sourceElement);

				Debug.Assert(this._group != null, "No ancestor CalendarGroupPresenterBase for ActivityResizerBar");
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

			// when we capture the mouse we are getting a mouse move. for a zero minute
			// activity like in dayview that ends up causing us to make the activity 
			// the duration of 1 timeslot on the mouse down which isn't ideal. the same
			// issue occurs in schedule view so ignore the initial move
			if ( _initialMouseDown == null )
			{
				_initialMouseDown = _mousePosition;
				return;
			}

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

							if (this.ProcessMoveOverMultiDayArea(ScheduleUtilities.GetTimeslotRange(tsp, _dayOffset)))
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
			this._control.EditHelper.EndEdit(this._activity, true, true);

			if (this._areEventsWired)
				this._rootElement.ReleaseMouseCapture();
		}

		#endregion //ProcessCancel

		#region ProcessCommit

		protected override void ProcessCommit()
		{
			this._control.EditHelper.EndEdit(this._activity, false, true);

			if (this._areEventsWired)
				this._rootElement.ReleaseMouseCapture();
		}

		#endregion //ProcessCommit	

		#endregion //Base class overrides	
    
		#region Methods

		#region Internal Methods

		#region BeginMouseCapture

		internal bool BeginMouseCapture(ActivityResizerBar resizerBar)
		{
			this._rootElement.Cursor = resizerBar.Cursor;

			WireMouseEvents();

			bool captured = this._rootElement.CaptureMouse();

			if (!captured || !this._areEventsWired)
				return false;

			return captured;
		}

		#endregion //BeginMouseCapture

		#region ProcessMoveFromPeer

		internal void ProcessMoveFromPeer(ActivityResizerBar resizerBar, double deltaX, double deltaY)
		{
			GeneralTransform transform;
			if ( !PresentationUtilities.TryTransformToRoot(resizerBar, _rootElement, out transform) )
			{
				this.ProcessCancel();
				return;
			}

			// map the 0,0 pt from the resizerbar to the root panel
			Point pt = transform.Transform(new Point(0, 0));

			// adjust the point based on the delta passed in
			if (resizerBar.VerticalAlignment == VerticalAlignment.Stretch)
				pt.X += deltaX;
			else
				pt.Y += deltaY;

			if (this.ProcessNewMousePosition(pt, false))
				this.ProcessCommit();
			else
				this.ProcessCancel();
		}

		#endregion //ProcessMoveFromPeer

		#endregion //Internal Methods	

		#region Private Methods

		#region ProcessMoveOverMultiDayArea

		private bool ProcessMoveOverMultiDayArea(DateRange localRange)
		{
			var token = _tzProvider.LocalToken;

			DateTime newDate;

			if ( this._isLeading )
			{
				if ( localRange.ContainsExclusive(_originalActivityRangeLocal.End) || localRange.Start < _originalActivityRangeLocal.End )
				{
					newDate = localRange.Start;
				}
				else
				{
					newDate = ScheduleUtilities.Max(_originalActivityRangeLocal.Start, _oppositeEdgeDayRange.Start);
				}

				// AS 1/13/12 TFS77443
				newDate = ScheduleUtilities.Max( newDate, _minMaxRange.Start );

				this.Activity.SetStartLocal(token, newDate);
			}
			else
			{
				if ( _originalActivityRangeLocal.Start < localRange.Start || localRange.ContainsExclusive(_originalActivityRangeLocal.Start) )
				{
					newDate = localRange.End;
				}
				else
				{
					newDate = ScheduleUtilities.Min(_originalActivityRangeLocal.End, _oppositeEdgeDayRange.End);
				}

				// AS 1/13/12 TFS77443
				newDate = ScheduleUtilities.Min( newDate, _minMaxRange.End );

				this.Activity.SetEndLocal(token, newDate);
			}

			return true;
		}

		#endregion //ProcessMoveOverMultiDayArea	
    
		#region ProcessNewMousePosition

		private bool ProcessNewMousePosition(Point ptInRootPanelCoords, bool calledFromMouseMove)
		{
			TimeRangePresenterBase tsp = ScheduleUtilities.GetTimeRangePresenterFromPoint(_rootElement, ptInRootPanelCoords, _group, true) as TimeRangePresenterBase;

			if (tsp != null)
			{
				DateRange tsRange = ScheduleUtilities.GetTimeslotRange(tsp, _dayOffset);

				if ( tsp.Kind == TimeRangeKind.Day )
					return this.ProcessMoveOverMultiDayArea(tsRange);

				DateRange localActivityRange = ScheduleUtilities.ConvertFromUtc(_tzProvider.LocalToken, _activity);
				if (this._isLeading)
				{
					if ( tsRange.Start > localActivityRange.End )
						this._activity.Start = this._activity.End;
					else
					{
						// AS 1/13/12 TFS77443
						//_activity.SetStartLocal(_tzProvider.LocalToken, tsRange.Start);
						_activity.SetStartLocal(_tzProvider.LocalToken, ScheduleUtilities.Max(tsRange.Start, _minMaxRange.Start));
					}
				}
				else
				{
					if ( tsRange.End < localActivityRange.Start )
						this._activity.End = this._activity.Start;
					else
					{
						// AS 1/13/12 TFS77443
						//_activity.SetEndLocal(_tzProvider.LocalToken, tsRange.End);
						_activity.SetEndLocal(_tzProvider.LocalToken, ScheduleUtilities.Min(tsRange.End, _minMaxRange.End));
					}
				}

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