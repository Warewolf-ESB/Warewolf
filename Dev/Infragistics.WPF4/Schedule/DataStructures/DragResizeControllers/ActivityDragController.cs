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
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Linq;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules
{
	internal class ActivityDragController : ActivityDragControllerBase
	{
		#region Private Members

		private CalendarGroupBase _group;
		private ActivityDragManager _dragManager;
		private bool _dragStarted;
		private TimeSpan _startDragTimeOffset;
		private TimeRangeKind _origTimeRangeKind = TimeRangeKind.Time;
		private DateTime _originalStartLocal;
		private DateTime _originalEndLocal;
		private TimeSpan _originalDuration;
		private DateTime _originalLogicalDay;
		private XamScheduleDataManager _dataMgr;
		private IScheduleControl _lastControlProcessed;
		private IScheduleControl _lastTargetControl;
		private bool _originalIsDay;
		private TimeSpan _originalStartDayOffset;
		private bool _isMultiDayActivity;
		private TimeSpan _logicalDayDuration;
		private TimeSpan _logicalDayOffset;
		private Point? _lastValidDropPoint;
		private bool _originalIsTimeZoneNeutral; // AS 11/1/10 TFS58871


		private static bool s_cursorLoadFailed;
		private static readonly Cursor s_moveCursor;
		private static readonly Cursor s_copyCursor;


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


		// AS 10/29/10 TFS56705
		private RelativePositionInfo _relativePositions;

		#endregion //Private Members
	
		#region Constructor


		static ActivityDragController()
		{
			s_moveCursor = Infragistics.Windows.Utilities.LoadCursor(typeof(ActivityDragController), "Cursors/DragMove.cur") ?? Cursors.Arrow;
			s_copyCursor = Infragistics.Windows.Utilities.LoadCursor(typeof(ActivityDragController), "Cursors/DragMovePlus.cur") ?? Cursors.Arrow;
		}



		internal ActivityDragController(ScheduleControlBase control, ActivityPresenter activityPresenter, FrameworkElement rootElement, Point startDragLocation)
			: base(control, rootElement, activityPresenter, activityPresenter.Activity)
		{








			this._dataMgr				= this._control.DataManagerResolved;
			this._dragManager			= this._dataMgr.DragManager;
			this._lastTargetControl		= this._control;
			this._logicalDayDuration	= this._dataMgr.LogicalDayDuration;
			this._logicalDayOffset		= this._dataMgr.LogicalDayOffset;

			this._originalStartLocal	= this._activity.GetStartLocal(this._tzProvider.LocalToken);
			this._originalEndLocal		= this._activity.GetEndLocal(this._tzProvider.LocalToken);
			this._originalDuration		= this._activity.Duration;
			_originalIsTimeZoneNeutral	= this._activity.IsTimeZoneNeutral; // AS 11/1/10 TFS58871

			this._group = ScheduleUtilities.GetCalendarGroupFromElement(_sourceElement);

			Debug.Assert(this._group != null, "No ancestor CalendarGroupPresenterBase for ActivityPresenter");

			// AS 10/29/10 TFS56705
			this._relativePositions		= new RelativePositionInfo(_rootElement);

			GeneralTransform transform;
			if ( PresentationUtilities.TryTransformToRoot(_sourceElement, _rootElement, out transform) )
			{
				// JJD 4/6/11 - TFS68203 
				// In SL, adjust the point based on the browser's zoom factor
				//TimeRangePresenterBase tsp = ScheduleUtilities.GetTimeRangePresenterFromPoint(this._rootElement, transform.Transform(startDragLocation), this._group);
				Point ptTransformed = transform.Transform(startDragLocation);





				ITimeRangePresenter trp = ScheduleUtilities.GetTimeRangePresenterFromPoint(this._rootElement, ptTransformed, this._group);

				if ( trp != null )
				{
					_origTimeRangeKind = trp.Kind;

					TimeRangePresenterBase tsp = trp as TimeRangePresenterBase;

					DateRange tspRange;

					if (tsp != null)
						tspRange = ScheduleUtilities.GetTimeslotRange(tsp, _logicalDayOffset);
					else
						tspRange = new DateRange(trp.Start, trp.End);

					_startDragTimeOffset = this._originalStartLocal - tspRange.Start;

					// if we are dragging from a timeslot then when we drag over a day we need to 
					// apply the delta between the activity start and the logical day start that 
					// contains the original timeslot
					DateRange dayRange = ScheduleUtilities.GetLogicalDayRange(tspRange.Start, _logicalDayOffset);
					this._originalStartDayOffset = _originalStartLocal.Subtract(dayRange.Start);
					this._originalLogicalDay = dayRange.Start;
				}
			}

			this._isMultiDayActivity = _originalDuration.Ticks >= _logicalDayDuration.Ticks;
			this._originalIsDay = _origTimeRangeKind == TimeRangeKind.Day || _origTimeRangeKind == TimeRangeKind.DayViewMultiDayArea;
		}

		#endregion //Constructor	

		#region Base class overrides

		#region CheckIfScrollIsRequired

		protected override void CheckIfScrollIsRequired()
		{
			if (!_canScroll)
			{
				this.KillTimer();
				return;
			}

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

				this._lastValidDropPoint = null;

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
			// AS 10/29/10 TFS56705
			// When the first move occurs we will build a list of the deltas that we can use 
			// to adjust the coordinates which are calculated relative to the root element.
			//
			if (!_relativePositions.IsInitialized)
				_relativePositions.Initialize(e, _dataMgr.Controls.Select(( IScheduleControl ctrl ) => { return ctrl as FrameworkElement; }).Where(c => { return c != null; }));

			Point originalMousePosition = e.GetPosition(this._rootElement);


			e.Handled = true;



			this._mousePosition = originalMousePosition;

			ITimeRangePresenter trp = this.ProcessNewMousePosition(this._mousePosition, true);

			// if we didn't get a hit then shift over and up 2 pixels and try again
			// This is to prevent the no drop cursor from showing when we are over the border
			// between 2 time zones
			if (trp == null )
			{
				Point newPoint = originalMousePosition;
				newPoint.X -= 2;
				newPoint.Y -= 2;
				trp = this.ProcessNewMousePosition(newPoint, true);
			}

			if (trp != null)
			{
				TimeRangePresenterBase tsp = trp as TimeRangePresenterBase;

				this._lastValidDropPoint = this._mousePosition;

				if ( tsp != null )
					this.InitializeTimeslotScrollInfo(tsp, false);

				if (this._canScroll && tsp != null)
				{
					this._pointInTimeRangeArea = e.GetPosition(this._timeRangeArea);
					this.CheckIfScrollIsRequired();
				}
			}
			else
			{
				this.KillTimer();

				foreach (IScheduleControl contrl in this._dataMgr.Controls)
				{
					FrameworkElement fe = contrl as FrameworkElement;

					if ( fe == null || !PresentationUtilities.MayBeVisible(fe) )
						continue;

					if (fe is XamOutlookCalendarView)
						continue;


					Point pt = e.GetPosition(fe);




					CalendarHeader calHeader = PresentationUtilities.GetVisualDescendantFromPoint<CalendarHeader>(fe, pt, null);

					if (calHeader != null)
					{
						calHeader.SelectCalendar();

						IScheduleControl control = ScheduleUtilities.GetIScheduleControl(calHeader);

						if (control != null)
						{
							if (this._dragManager.Move(control, ShouldCopy, calHeader.Calendar))
								_lastTargetControl = control;
							
							this.SetCursor();



						}

						return;
					}
				}

				// in order to prevent the no-drop cursor from being displayed if we go over a border
				// between time slots check if we have gone a minimum # of pixels from the last
				// valid drop point
				if (this._lastValidDropPoint == null ||
					Math.Abs(this._lastValidDropPoint.Value.X - originalMousePosition.X) > 4 ||
					Math.Abs(this._lastValidDropPoint.Value.Y - originalMousePosition.Y) > 4)
				{
					this._lastValidDropPoint = null;

					this._dragManager.EnterInvalidDropLocation();

				    this.SetCursor();



					
				}
			}
		}

		#endregion //OnMouseMove	

		#region ProcessCancel

		protected override void ProcessCancel()
		{
			this.CleanupCursor();
			this._control.EditHelper.EndDrag(true);
		}

		#endregion //ProcessCancel

		#region ProcessCommit

		protected override void ProcessCommit()
		{
			this.CleanupCursor();
			this._control.EditHelper.EndDrag(false);
		}

		#endregion //ProcessCommit	

		#endregion //Base class overrides	
    
		#region Methods

		#region Internal Methods

		#region BeginDrag

		internal bool BeginDrag(bool copy)
		{
			return BeginDrag(copy, false);
		}

		private bool BeginDrag(bool copy, bool calledFromPeer)
		{
			Debug.Assert(_dragStarted == false);

			if (this._dragManager == null || _dragStarted)
				return false;

			if (!this._dragManager.BeginDrag(this._activity, copy, this._control))
				return false;

			_dragStarted = true;

			if (calledFromPeer)
				return true;

			this.SetCursor();

			WireMouseEvents();

			bool captured = this._rootElement.CaptureMouse();

			if (!captured || !this._areEventsWired)
			{
				this.CleanupCursor();
				return false;
			}

			return captured;
		}

		#endregion //BeginDrag

		#region EndDrag

		internal bool EndDrag(bool cancel)
		{
			this.CleanupCursor();

			if (this._dragManager == null || !_dragStarted)
				return false;


			if (!s_cursorLoadFailed)
			{
				try
				{
					this._rootElement.ClearValue(FrameworkElement.CursorProperty);
				}
				catch (Exception)
				{
				}
			}

			bool success = this._dragManager.EndDrag(cancel);

			if (this._areEventsWired)
				this._rootElement.ReleaseMouseCapture();

			return success;

		}

		#endregion //EndDrag

		#region OnControlKeyToggled

		internal void OnControlKeyToggled()
		{
			if (_dragStarted)
			{
				ActivityDragManager.DropAction currentAction = _dragManager.CurrentDropAction;

				bool? isCurrentCopy = null;

				switch (_dragManager.CurrentDropAction)
				{
					case ActivityDragManager.DropAction.Copy:
						isCurrentCopy = true;
						break;
					case ActivityDragManager.DropAction.Move:
						isCurrentCopy = false;
						break;
					case ActivityDragManager.DropAction.Original:
						isCurrentCopy = !ShouldCopy;
						break;
				}

				if (isCurrentCopy.HasValue)
				{
					bool shouldCopy = ShouldCopy;

					if (shouldCopy != isCurrentCopy.Value)
					{
						_dragManager.Move(_lastTargetControl, ShouldCopy);
						
						this.SetCursor();
					}
				}
			}
		}

		#endregion //OnControlKeyToggled	
    
		#region ProcessMoveFromPeer

		internal void ProcessMoveFromPeer(double deltaX, double deltaY)
		{
			GeneralTransform transform;
			if ( !PresentationUtilities.TryTransformToRoot(_sourceElement, _rootElement, out transform) )
			{
				this.ProcessCancel();
				return;
			}

			// map the 0,0 pt from the resizerbar to the root panel
			Point pt = transform.Transform(new Point(0, 0));

			pt.X += deltaX;
			pt.Y += deltaY;

			this.BeginDrag(false, true);

			if (this.ProcessNewMousePosition(pt, false) != null)
				this.ProcessCommit();
			else
				this.ProcessCancel();
		}

		#endregion //ProcessMoveFromPeer

		#endregion //Internal Methods	

		#region Private Methods

		#region CleanupCursor

		private void CleanupCursor()
		{


#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


			if (_rootElement != null)
				_rootElement.ClearValue(FrameworkElement.CursorProperty);
		}

		#endregion //CleanupCursor

		#region ProcessNewMousePosition

		private ITimeRangePresenter ProcessNewMousePosition(Point ptInRootPanelCoords, bool calledFromMouseMove)
		{

			Point ptToTest = ptInRootPanelCoords;


#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

			ITimeRangePresenter trp = null;

			if (_lastControlProcessed != null && _lastControlProcessed != this._control)
			{
				trp = this.ProcessMousePositionForControl(_lastControlProcessed, ptToTest, calledFromMouseMove);
				if (trp != null)
					return trp;

			}

			trp = this.ProcessMousePositionForControl(_control, ptToTest, calledFromMouseMove);
			if (trp != null)
			{
				_lastControlProcessed = _control;
				return trp;
			}

			foreach (IScheduleControl contrl in this._dataMgr.Controls)
			{
				FrameworkElement fe = contrl as FrameworkElement;

				if (fe == _control || fe == _lastControlProcessed || !PresentationUtilities.MayBeVisible(fe) )
					continue;

				if (fe is XamOutlookCalendarView)
					continue;

				trp = this.ProcessMousePositionForControl(contrl, ptToTest, calledFromMouseMove);
				if (trp != null)
				{
					_lastControlProcessed = contrl;
					return trp;
				}
			}

			return null;
		}

		#endregion //ProcessNewMousePosition	

		#region ProcessMousePositionForControl

		private ITimeRangePresenter	ProcessMousePositionForControl(IScheduleControl control, Point ptInRootPanelCoords, bool calledFromMouseMove)
		{
			FrameworkElement fe = control as FrameworkElement;

			if (fe == null)
				return null;

			// AS 10/29/10 TFS56705
			// There was an issue with using the TryTransformToRoot when the elements existed 
			// in different visual trees (i.e. in different presentation sources) and while I 
			// addressed that for when we had the rights to use the screen coordinates after 
			// some discussion we decided to calculate the deltas between the root element and 
			// the various schedule controls and use that to adjust the coordinates.
			//

			if ( !_relativePositions.TryTransformToRelativePoint(fe, ref ptInRootPanelCoords) )
				return null;







			ITimeRangePresenter trp = ScheduleUtilities.GetTimeRangePresenterFromPoint(fe, ptInRootPanelCoords, null);

			if (trp is UIElement)
			{
				TimeRangePresenterBase tsp = trp as TimeRangePresenterBase;
				
				ResourceCalendar calendar = tsp != null ? tsp.DataContext as ResourceCalendar : null;

				if (calendar == null)
					calendar = this._activity.OwningCalendar;

				TimeRangeKind kind = trp.Kind;

				if ( tsp == null || tsp.Timeslot != null)
				{
					DateRange tsRange;

					if (tsp != null)
					{
						tsRange = ScheduleUtilities.GetTimeslotRange(tsp, _logicalDayOffset);
					}
					else
					{
						tsRange = new DateRange(trp.Start, trp.End);
					}

					IScheduleControl controlUnderMouse = ScheduleUtilities.GetIScheduleControl(trp as UIElement);

					Debug.Assert(controlUnderMouse != null, "No control found");

					if (controlUnderMouse == null || control != controlUnderMouse)
						return null;

					DateTime newStart = tsRange.Start + _startDragTimeOffset;
					DateTime newEnd = newStart + this._originalDuration;
					bool isTimeZoneNeutral = _originalIsTimeZoneNeutral; // AS 11/1/10 TFS58871

					if (kind != _origTimeRangeKind)
					{
						XamDayView dv = controlUnderMouse as XamDayView;

						bool hasMultiDayArea = (dv != null && dv.MultiDayActivityAreaVisibility == Visibility.Visible);

						switch (kind)
						{
							case TimeRangeKind.Day:
								// when dragging from a timeslot over a day then we need to apply the delta between
								// the start of the activity and the start of the logical day that the drag started 
								// from and not the delta between the timeslot and the start of the logical day
								newStart = tsRange.Start.Add(_originalStartDayOffset);
								newEnd = newStart + this._originalDuration;
								break;

							case TimeRangeKind.DayViewMultiDayArea:
								// see Day above
								if (_isMultiDayActivity)
								{
									newStart = tsRange.Start.Add(_originalStartDayOffset);
									newEnd = newStart + this._originalDuration;
								}
								else
								// if the activity is a multi day activity (i.e. it would show in the 
								// multi day area) then we don't need to do anything but if it is then 
								// we need to adjust the duration/end so that it will show in the area
								// which means the duration needs to be at least the logical day duration
								{
									newStart = tsRange.Start;
									newEnd = newStart.Add(_logicalDayDuration);

									// AS 11/1/10 TFS58871
									// If we're dragging it into the all day event area then make it a 
									// timezone neutral activity as they do in outlook.
									//
									isTimeZoneNeutral = true;
								}
								break;

							case TimeRangeKind.Time:
								// if we started dragging from a day and we drag over 
								// a timeslot, then make that timeslot the start of the
								// activity
								if (_originalIsDay)
									newStart = tsRange.Start;

								// in the case where we drag over the timeslot area of dayview 
								// and it has a multi day area and the activity is a multi day
								// activity then in order for it to show in the timeslot area 
								// it needs to be a non-multiday activity. we'll use the same
								// default that outlook does here - 30 minutes
								if (hasMultiDayArea && _isMultiDayActivity)
								{
									newEnd = newStart + TimeSpan.FromMinutes(30);

									// AS 11/1/10 TFS58871
									// Dragging a timezone neutral activity from monthview or the all day 
									// area into a timeslot should clear the timezone neutral flag.
									//
									isTimeZoneNeutral = false;
								}
								else // otherwise recalculate the end in case we adjusted the start above
								{
									newEnd = newStart + this._originalDuration;

									// AS 11/1/10 TFS58871
									// If the source activity was timezone neutral then dragging it over a
									// timeslot would indicate an intent that the activity is fixed to a 
									// specific local time unless just the day portion is changing in which 
									// case it could be assumed that the user is just changing the day on 
									// which the timezone neutral activity starts.
									//
									if ( _originalIsTimeZoneNeutral )
									{
										isTimeZoneNeutral = newStart.TimeOfDay == _originalStartLocal.TimeOfDay;
									}
								}
								break;

							case TimeRangeKind.TimeHeader:
								if (controlUnderMouse != this._control)
									return null;

								DateRange dayRange = ScheduleUtilities.GetLogicalDayRange(tsRange.Start, _logicalDayOffset);
								newStart = this._originalLogicalDay.Add(tsRange.Start.Subtract(dayRange.Start));
								TimeSpan duration = this._originalDuration;

								if (hasMultiDayArea && _isMultiDayActivity)
								{
									duration = TimeSpan.FromMinutes(30);

									// AS 11/1/10 TFS58871 - Same comment as Time above
									isTimeZoneNeutral = false;
								}
								else
								{
									// AS 11/1/10 TFS58871 - Same comment as Time above
									if ( _originalIsTimeZoneNeutral )
									{
										isTimeZoneNeutral = newStart.TimeOfDay == _originalStartLocal.TimeOfDay;
									}

								}
								
								newEnd = newStart + duration;

								break;

							case TimeRangeKind.TimeHeaderWithDayContext:
								break;
							default:
								Debug.Assert(false, "Unknown timerange kind");
								return null;
						}

					}

					// AS 1/13/12 TFS77443
					if (!_minMaxRange.IntersectsWithExclusive(new DateRange(newStart, newEnd)))
						return null;

					if ( this._dragManager.Move(controlUnderMouse, ShouldCopy, calendar, newStart, newEnd, isTimeZoneNeutral ) )
						_lastTargetControl = controlUnderMouse;

					this.SetCursor();

					return trp;
				}
			}

			return null;
		}

		#endregion //ProcessMousePositionForControl

		#region SetCursor

		private void SetCursor()
		{
			bool showCopyCursor = false;
			bool showMoveCursor = false;
			switch (this._dragManager.CurrentDropAction)
			{
				case ActivityDragManager.DropAction.Copy:
					showCopyCursor = true;
					break;
				case ActivityDragManager.DropAction.Move:
					showMoveCursor = true;
					break;
				case ActivityDragManager.DropAction.Invalid:
					break;
				case ActivityDragManager.DropAction.Original:
					if (ShouldCopy)
						showCopyCursor = true;
					else
						showMoveCursor = true;
					break;
			}


			if (!s_cursorLoadFailed)
			{
				try
				{
					if (showCopyCursor)
						this._rootElement.Cursor = s_copyCursor;
					else
					if (showMoveCursor)
						this._rootElement.Cursor = s_moveCursor;
					else
						this._rootElement.Cursor = Cursors.No;

				}
				catch (Exception)
				{
					s_cursorLoadFailed = true;
				}
			}


#region Infragistics Source Cleanup (Region)










































#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion //SetCursor	

		#region SetCursorPosition



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


		#endregion //SetCursorPosition	
    
		#endregion //Private Methods	
            	
		#endregion //Methods	
	
		// AS 10/29/10 TFS56705
		#region RelativePositionInfo class
		private class RelativePositionInfo
		{
			#region Member Variables

			private FrameworkElement _rootElement;
			private bool _isInitialized;
			private Dictionary<FrameworkElement, GeneralTransform> _cachedRelativeTransforms;


			private Dictionary<FrameworkElement, Point> _relativePositions;


			#endregion // Member Variables

			#region Constructor
			internal RelativePositionInfo( FrameworkElement rootElement )
			{
				_rootElement = rootElement;
			}
			#endregion // Constructor

			#region Properties

			#region IsInitialized
			internal bool IsInitialized
			{
				get { return _isInitialized; }
			}
			#endregion // IsInitialized

			#endregion // Properties

			#region Methods

			#region GetRelativeTransform
			internal GeneralTransform GetRelativeTransform( FrameworkElement relativeElement )
			{
				if ( _cachedRelativeTransforms == null )
					_cachedRelativeTransforms = new Dictionary<FrameworkElement, GeneralTransform>();

				GeneralTransform transform;

				if ( !_cachedRelativeTransforms.TryGetValue(relativeElement, out transform) )
				{

					if ( !PresentationUtilities.TryTransformToRoot(_rootElement, relativeElement, out transform) )
						transform = null;





					_cachedRelativeTransforms[relativeElement] = transform;
				}

				return transform;
			}
			#endregion // GetRelativeTransform

			#region Initialize
			internal void Initialize( MouseEventArgs e, IEnumerable<FrameworkElement> relativeElements )
			{
				_isInitialized = true;


				if ( _relativePositions == null )
				{
					_relativePositions = new Dictionary<FrameworkElement, Point>();

					Point rootPos = e.GetPosition(_rootElement);

					foreach ( FrameworkElement ctrl in relativeElements )
					{
						if ( !PresentationUtilities.MayBeVisible(ctrl) )
							continue;

						try
						{
							Point ctrlPt = e.GetPosition(ctrl);

							ctrlPt.X -= rootPos.X;
							ctrlPt.Y -= rootPos.Y;

							_relativePositions[ctrl] = ctrlPt;
						}
						catch
						{
						}
					}
				}

			}
			#endregion // Initialize

			#region TryTransformToRelativePoint

			internal bool TryTransformToRelativePoint( FrameworkElement relativeElement, ref Point rootElementPoint )





			{
				Point upperLeft;


				Point tempPt;

				if ( _relativePositions != null && _relativePositions.TryGetValue(relativeElement, out tempPt) )
				{
					rootElementPoint.X += tempPt.X;
					rootElementPoint.Y += tempPt.Y;
					upperLeft = new Point();
				}
				else
				{
					var transform = this.GetRelativeTransform(relativeElement);

					if (transform == null)
						return false;

					rootElementPoint = transform.Transform(rootElementPoint);
					upperLeft = new Point();
				}

				Rect relativeRect = new Rect(upperLeft.X, upperLeft.Y, relativeElement.ActualWidth, relativeElement.ActualHeight);


#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


				if ( !relativeRect.Contains(rootElementPoint) )
					return false;

				return true;
			}
			#endregion // TryTransformToRelativePoint

			#endregion // Methods
		} 
		#endregion // RelativePositionInfo class
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