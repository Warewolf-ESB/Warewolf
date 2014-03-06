using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.Windows.Input;
using Infragistics.Controls.Editors;
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules.Primitives
{
	
	/// <summary>
	/// Base class for an element that represents a specific range of dates and/or times.
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstItem, GroupName = VisualStateUtilities.GroupFirstLastItem)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsFirstAndLastItem, GroupName = VisualStateUtilities.GroupFirstLastItem)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsLastItem, GroupName = VisualStateUtilities.GroupFirstLastItem)]
	[TemplateVisualState(Name = VisualStateUtilities.StateIsNotFirstOrLastItem, GroupName = VisualStateUtilities.GroupFirstLastItem)]
	public abstract class TimeRangePresenterBase : ResourceCalendarElementBase
		, ITimeRange
		, IRecyclableElement
		, IReceivePropertyChange<bool>
		, ITimeRangePresenter
	{
		#region Member Variables

		private Panel _ownerPanel;
		private MouseHelper _mouseHelper;
		private Orientation _orientation;
		private Point? _mouseDownPoint;

		#endregion //Member Variables

		#region Constructor
		static TimeRangePresenterBase()
		{
		}

		/// <summary>
		/// Initializes a new <see cref="TimeRangePresenterBase"/>
		/// </summary>
		protected TimeRangePresenterBase()
		{
			_orientation = this.Orientation;
		}
		#endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="TimeRangePresenterBase"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="TimeRangePresenterBaseAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new TimeRangePresenterBaseAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnLostMouseCapture

		/// <summary>
		/// Called when mouse capture has been lost
		/// </summary>
		/// <param name="e">The mouse event args</param>
		protected override void OnLostMouseCapture(MouseEventArgs e)
		{
			base.OnLostMouseCapture(e);

			this._mouseDownPoint = null;
		}

		#endregion //OnLostMouseCapture	

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the timeslot.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			// AS 3/12/12 TFS104584
			// Ignore the enter if it is the result of a touch. I'm doing this because 
			// at least in wpf there is a significant delay between the end of the touch 
			// and when they fix up the correct element that the mouse is over and so the 
			// click to add indicator may show up for an element that the mouse was over 
			// but no longer is.
			//
			if (e.StylusDevice == null




				)
				this.OnHoverStartEnd(true);
		}
		#endregion // OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the timeslot.
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			this.OnHoverStartEnd(false);
		}
		#endregion // OnMouseLeave

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			// AS 3/6/12 TFS102945
			// Moved to a helper method since we may need to defer the processing.
			//
			//if (this.MouseHelper.OnMouseLeftButtonDown(e))
			//{
			//    if (this.MouseHelper.ClickCount == 2)
			//    {
			//        if (this.DisplayAppointmentDialog() != null)
			//        {
			//            this.EndMouseCapture();
			//            e.Handled = true;
			//        }
			//    }
			//}
			//
			//if (!e.Handled)
			//{
			//    ScheduleControlBase control = ScheduleUtilities.GetControl(this);
			//
			//    Debug.Assert(control != null, "Control not found");
			//
			//    if (control != null)
			//    {
			//        // push keyboard focus on the control so navigation works properly. 
			//        //
			//        control.Focus();
			//
			//        CalendarGroupBase group = ScheduleUtilities.GetCalendarGroupFromElement(this);
			//
			//        if (group != null)
			//            control.ActiveCalendar = group.SelectedCalendar;
			//
			//        DateRange thisRange = ScheduleUtilities.GetTimeslotRange(this, ScheduleUtilities.GetLogicalDayOffset(control));
			//
			//        // set the selected time range
			//        // AS 1/13/12 TFS77443
			//        //control.SetSelectedTimeRange(thisRange, PresentationUtilities.IsModifierPressed(ModifierKeys.Shift), true);
			//        if (!control.SetSelectedTimeRange(thisRange, PresentationUtilities.IsModifierPressed(ModifierKeys.Shift), true))
			//            return;
			//    }
			//
			//    _mouseDownPoint = e.GetPosition(this);
			//
			//    // if capture is not taken we should clear the flag. note we shouldn't rely
			//    // on the return result because while we may have taken capture, something 
			//    // may have taken it away while the events were raised during the call after 
			//    // it was given capture
			//    if (!this.CaptureMouse())
			//        _mouseDownPoint = null;
			//
			//    if (null != _mouseDownPoint)
			//        this.OnHoverStartEnd(false);
			//
			//    e.Handled = true;
			//}
			this.ProcessMouseLeftButtonDown(e);

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Invoked when the left mouse button is released.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
		{
			if (_mouseDownPoint != null)
			{
				this.OnHoverStartEnd(true);
				this.EndMouseCapture();
			}

			base.OnMouseLeftButtonUp(e);
		}

		#endregion //OnMouseLeftButtonUp

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse has noved
		/// </summary>
		/// <param name="e">The mouse event args</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (this._mouseDownPoint.HasValue)
			{
				Point pt = e.GetPosition(this);

				if (Math.Abs(pt.X - _mouseDownPoint.Value.X) > MouseHelper.DoubleClickSizeX ||
					Math.Abs(pt.Y - _mouseDownPoint.Value.Y) > MouseHelper.DoubleClickSizeY)
				{
					Point mouseDownPoint = this._mouseDownPoint.Value;

					this.ReleaseMouseCapture();
					
					ScheduleControlBase control = ScheduleUtilities.GetControl( this );

					if (control != null)
					    control.EditHelper.BeginTimeSelectionDrag(this, mouseDownPoint);
				}

				e.Handled = true;

			}
		}

		#endregion //OnMouseMove	

		// AS 1/20/11 TFS62619
		#region OnMouseRightButtonDown
		/// <summary>
		/// Invoked when the right mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse event.</param>
		protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseRightButtonDown(e);

			this.ProcessRightMouseDown(e);
		} 
		#endregion //OnMouseRightButtonDown

		#region ToString
		/// <summary>
		/// Returns the string representation of the element.
		/// </summary>
		/// <returns>A string containing the range of time that the element represents</returns>
		public override string ToString()
		{
			if (null != this.Timeslot)
				return this.Timeslot.ToString();

			return base.ToString();
		}
		#endregion //ToString

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region End

		/// <summary>
		/// Identifies the <see cref="End"/> dependency property
		/// </summary>
		public static readonly DependencyProperty EndProperty = DependencyProperty.Register("End",
			typeof(DateTime), typeof(TimeRangePresenterBase),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Now.AddMinutes(15))
			);

		/// <summary>
		/// Returns the exclusive end date for the timeslot.
		/// </summary>
		/// <seealso cref="EndProperty"/>
		public DateTime End
		{
			get
			{
				return (DateTime)this.GetValue(TimeRangePresenterBase.EndProperty);
			}
			internal set
			{
				this.SetValue(TimeRangePresenterBase.EndProperty, value);
			}
		}

		#endregion //End

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register("Orientation",
			typeof(Orientation), typeof(TimeRangePresenterBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationVerticalBox, new PropertyChangedCallback(OnOrientationChanged))
			);

		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeRangePresenterBase element = d as TimeRangePresenterBase;

			element._orientation = (Orientation)e.NewValue;

			element.InvalidateMeasure();
			element.UpdateVisualState();

		}

		/// <summary>
		/// Returns the current orientation in which the element is arranged.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		public Orientation Orientation
		{
			get
			{
				return _orientation;
			}
			internal set
			{
				this.SetValue(TimeRangePresenterBase.OrientationProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //Orientation

		#region Start

		/// <summary>
		/// Identifies the <see cref="Start"/> dependency property
		/// </summary>
		public static readonly DependencyProperty StartProperty = DependencyProperty.Register("Start",
			typeof(DateTime), typeof(TimeRangePresenterBase),
			DependencyPropertyUtilities.CreateMetadata(DateTime.Now)
			);

		/// <summary>
		/// Returns the earliest time that the element represents.
		/// </summary>
		/// <seealso cref="StartProperty"/>
		public DateTime Start
		{
			get
			{
				return (DateTime)this.GetValue(TimeRangePresenterBase.StartProperty);
			}
			internal set
			{
				this.SetValue(TimeRangePresenterBase.StartProperty, value);
			}
		}

		#endregion //Start

		#endregion //Public Properties

		#region Internal Properties

		#region Control

		internal ScheduleControlBase Control { get { return ScheduleUtilities.GetControl(this); } }

		#endregion //Control	
    
		#region Kind

		internal virtual TimeRangeKind Kind { get { return TimeRangeKind.Time; } }

		#endregion //Kind	
    
		#region LocalRange
		internal virtual DateRange LocalRange
		{
			get
			{
				return new DateRange(this.Start, this.End);
			}
		} 
		#endregion // LocalRange

		#region MouseHelper
		internal MouseHelper MouseHelper
		{
			get
			{
				if (_mouseHelper == null)
					_mouseHelper = new MouseHelper(this);

				return _mouseHelper;
			}
		}
		#endregion // MouseHelper

		#region Timeslot

		private TimeslotBase _timeslot;

		/// <summary>
		/// Returns or sets the associated <see cref="Timeslot"/>
		/// </summary>
		internal TimeslotBase Timeslot
		{
			get
			{
				return _timeslot;
			}
			set
			{
				if (value != _timeslot)
				{
					var tsOld = _timeslot;

					_timeslot = value;
					this.UpdateVisualState();
					this.OnTimeSlotChanged(value, tsOld);
				}
			}
		}

		#endregion //Timeslot

		#endregion // Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			bool isFirstItem = TimeslotPanel.GetIsFirstItem(this);
			bool isLastItem = TimeslotPanel.GetIsLastItem(this);

			if (isFirstItem)
			{
				if (isLastItem)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateIsFirstAndLastItem, VisualStateUtilities.StateIsLastItem, VisualStateUtilities.StateIsFirstItem);
				else
					this.GoToState(VisualStateUtilities.StateIsFirstItem, useTransitions);
			}
			else
			{
				if (isLastItem)
					this.GoToState(VisualStateUtilities.StateIsLastItem, useTransitions);
				else
					this.GoToState(VisualStateUtilities.StateIsNotFirstOrLastItem, useTransitions);
			}

			base.ChangeVisualState(useTransitions);
		}
		#endregion //ChangeVisualState

		#region OnTimeSlotChanged

		internal virtual void OnTimeSlotChanged(TimeslotBase newValue, TimeslotBase oldValue)
		{
		}

		#endregion //OnTimeSlotChanged

		// AS 1/20/11 TFS62619
		#region ProcessRightMouseDown
		internal void ProcessRightMouseDown(MouseButtonEventArgs e)
		{
			if (e.Handled)
				return;

			ScheduleControlBase control = ScheduleUtilities.GetControl(this);

			Debug.Assert(control != null, "Control not found");

			if (control == null)
				return;
			// push keyboard focus on the control so navigation works properly. 
			if (!control.Focus())
				return;

			CalendarGroupBase group = ScheduleUtilities.GetCalendarGroupFromElement(this);

			if (group == null)
				return;

			var calendarToActivate = group.SelectedCalendar;

			control.ActiveCalendar = calendarToActivate;

			if (control.ActiveCalendar != calendarToActivate)
				return;

			DateRange thisRange = ScheduleUtilities.GetTimeslotRange(this, ScheduleUtilities.GetLogicalDayOffset(control));

			if (control.SelectedTimeRange == null || !control.SelectedTimeRange.Value.IntersectsWithExclusive(thisRange))
			{
				// set the selected time range
				control.SetSelectedTimeRange(thisRange, false, true);
			}
		}
		#endregion //ProcessRightMouseDown

		#endregion // Internal Methods

		#region Private Methods

		#region DisplayAppointmentDialog

		private bool? DisplayAppointmentDialog()
		{
			Panel parent = this.Parent as Panel;
			if (parent != null)
			{
				var ctrl = ScheduleControlBase.GetControl(parent);
				Debug.Assert(null != ctrl);

				if (null != ctrl)
					return ctrl.DisplayActivityDialog(this);
			}

			return null;
		}

		#endregion //DisplayAppointmentDialog

		#region EndMouseCapture
		private void EndMouseCapture()
		{
			if (_mouseDownPoint != null)
			{
				this.ReleaseMouseCapture();
			}
		} 
		#endregion // EndMouseCapture

		#region OnHoverStartEnd
		private void OnHoverStartEnd(bool start)
		{
			var panel = VisualTreeHelper.GetParent(this) as TimeslotPanel;

			if (null != panel)
			{
				if (start)
					panel.OnMouseEnterTimeslot(this);
				else
					panel.OnMouseLeaveTimeslot(this);
			}
		}
		#endregion // OnHoverStartEnd

		#region OnVisualStatePropertyChanged
		internal static void OnVisualStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			TimeRangePresenterBase item = (TimeRangePresenterBase)d;
			item.UpdateVisualState();
		}
		#endregion // OnVisualStatePropertyChanged

		// AS 3/6/12 TFS102945
		// Took the implementation from OnMouseLeftButtonDown and modified it a little so it could defer 
		// the processing while a touch is in progress and then be called later if the touch didn't 
		// result in a scroll/pan/flick.
		//
		#region ProcessMouseLeftButtonDown
		private void ProcessMouseLeftButtonDown(MouseButtonEventArgs e, ScrollInfoTouchAction? touchAction = null, bool isTouchDoubleClick = false)
		{
			ScheduleControlBase control = ScheduleUtilities.GetControl(this);

			// we're letting the mousehelper process during the actual mouse down so don't want 
			// to check it again (since during the callback it may be quite a while since the 
			// initial down).
			// AS 3/23/12 TFS106004
			// Added touchAction check since we don't want to increment the clickcount if this is the deferred 
			// processing of the initial touch down.
			//
			if (isTouchDoubleClick || (touchAction == null && this.MouseHelper.OnMouseLeftButtonDown(e) && this.MouseHelper.ClickCount == 2))
			{
				// AS 3/7/12 TFS102945
				// During a touch if we detect a double click wait until the touch operation is done
				//
				if (touchAction == null && control != null && control.ShouldQueueTouchActions)
				{
					control.EnqueueTouchAction((a) => ProcessMouseLeftButtonDown(e, a, true));
					e.Handled = true;
					return;
				}

				// AS 3/23/12 TFS105953
				// WPF has a bug whereby while promoting the touch messages to mouse events
				// it is using the same list instance that the touch device manages and so 
				// if you show a modal dialog or otherwise do something that would manipulate 
				// that list of staging items then it will blow up when the dialog is closed 
				// and they continue processing the list of staging items.
				//
				if (e.StylusDevice != null)
				{

					this.EndMouseCapture();
					this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action( () => { this.DisplayAppointmentDialog(); }));
					e.Handled = true;
					return;

				}

				if (this.DisplayAppointmentDialog() != null)
				{
					this.EndMouseCapture();
					e.Handled = true;
				}

				// AS 3/7/12 TFS102945
				// Regardless of whether we do anything if this is the callback for a double click 
				// do not process the single click touch.
				//
				if (isTouchDoubleClick)
					return;
			}

			// we need to consider whether we have a touch action since we would have 
			// marked the args as handled when the mouse down actually occurred
			if (!e.Handled || touchAction != null)
			{
				Debug.Assert(control != null, "Control not found");

				if (control != null)
				{
					// AS 3/7/12 TFS102945
					if (touchAction == null && control.ShouldQueueTouchActions)
					{
						control.EnqueueTouchAction((a) => ProcessMouseLeftButtonDown(e, a, false));
						e.Handled = true;
						return;
					}

					// push keyboard focus on the control so navigation works properly. 
					//
					control.Focus();

					CalendarGroupBase group = ScheduleUtilities.GetCalendarGroupFromElement(this);

					if (group != null)
						control.ActiveCalendar = group.SelectedCalendar;

					DateRange thisRange = ScheduleUtilities.GetTimeslotRange(this, ScheduleUtilities.GetLogicalDayOffset(control));

					// set the selected time range
					// AS 1/13/12 TFS77443
					//control.SetSelectedTimeRange(thisRange, PresentationUtilities.IsModifierPressed(ModifierKeys.Shift), true);
					if (!control.SetSelectedTimeRange(thisRange, PresentationUtilities.IsModifierPressed(ModifierKeys.Shift), true))
						return;
				}

				// if this was from a touch and the touch was released just do what we would have 
				// on the down - which was to select the timeslot
				if (touchAction != null && touchAction != ScrollInfoTouchAction.Drag)
					return;

				_mouseDownPoint = e.GetPosition(this);

				// if capture is not taken we should clear the flag. note we shouldn't rely
				// on the return result because while we may have taken capture, something 
				// may have taken it away while the events were raised during the call after 
				// it was given capture
				if (!this.CaptureMouse())
					_mouseDownPoint = null;

				if (null != _mouseDownPoint)
					this.OnHoverStartEnd(false);

				e.Handled = true;
			}
		}
		#endregion //ProcessMouseLeftButtonDown

		#endregion // Private Methods

		#endregion //Methods

		#region ITimeRangePresenter members

		TimeRangeKind ITimeRangePresenter.Kind { get { return this.Kind; } }

		#endregion //ITimeRangePresenter members	
    
		#region ITimeRange members
		DateTime ITimeRange.Start
		{
			get
			{
				var ts = this.Timeslot;

				return null != ts ? ts.Start : DateTime.MinValue;
			}
		}

		DateTime ITimeRange.End
		{
			get
			{
				var ts = this.Timeslot;

				return null != ts ? ts.End : DateTime.MaxValue;
			}
		}
		#endregion //ITimeRange members

		#region IRecyclableElement Members

		bool IRecyclableElement.DelayRecycling
		{
			get;
			set;
		}

		Panel IRecyclableElement.OwnerPanel
		{
			get
			{
				return _ownerPanel;
			}
			set
			{
				_ownerPanel = value;

				
				if (value is TimeslotPanelBase)
					this.Orientation = ((TimeslotPanelBase)value).TimeslotOrientation;
			}
		}

		#endregion

		#region IReceivePropertyChange<bool> Members

		void IReceivePropertyChange<bool>.OnPropertyChanged(DependencyProperty property, bool oldValue, bool newValue)
		{
			if (property == TimeslotPanel.IsFirstItemProperty ||
				property == TimeslotPanel.IsLastItemProperty)
				this.UpdateVisualState();
		}

		#endregion
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