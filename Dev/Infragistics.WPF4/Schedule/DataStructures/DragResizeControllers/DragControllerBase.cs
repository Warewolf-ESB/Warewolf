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





namespace Infragistics.Controls.Schedules
{
	internal abstract class DragControllerBase
	{
		#region Private Members

		protected ScheduleControlBase _control;
		protected FrameworkElement _rootElement;
		protected FrameworkElement _sourceElement;
		protected UIElement _timeRangeArea;
		protected double _timeRangeExtent;
		protected DateTime? _areaStart;
		protected DateTime? _areaEnd;
		protected bool _canScroll;
		protected bool _isVertical;
		protected bool _areEventsWired;
		protected DispatcherTimer _scrollTimer;
		protected double _originalScrollOffset;
		protected ScheduleControlBase _controlUnderPoint;
		protected ResourceCalendar _calendarUnderPoint;
		protected Point _pointInTimeRangeArea;
		protected Point _mousePosition;
		protected long _lastScrollTicks;
		protected const double ScrollMargin = 15;
		protected const int ScrollInterval = 60;
		protected ScrollInfo _timeslotScrollInfo;
		protected TimeZoneInfoProvider _tzProvider;
		protected DateRange _minMaxRange;







		#endregion //Private Members

		#region Constructor

		internal DragControllerBase(ScheduleControlBase control, FrameworkElement rootElement, FrameworkElement sourceElement)
		{








			this._control = control;
			this._sourceElement = sourceElement;
			this._rootElement = rootElement;
			this._tzProvider = control.TimeZoneInfoProviderResolved;
			this._minMaxRange = control.GetMinMaxRange(true); // AS 1/13/12 TFS77443

			this.InitializeTimeslotScrollInfo(sourceElement, true);


		}

		#endregion //Constructor

		#region Properties
		






		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region Abort

		internal void Abort()
		{
			// Just release the mouse capture. This will cause the OnLostMouseCapture
			// handler to be called which will cancel the edit
			this._rootElement.ReleaseMouseCapture();
		}

		#endregion //Abort

		#endregion //Internal Methods

		#region Protected Methods

		#region InitializeTimeslotScrollInfo

		protected void InitializeTimeslotScrollInfo(FrameworkElement sourceElement, bool calledFronCtor)
		{

			this._timeRangeArea = PresentationUtilities.GetVisualAncestor<UIElement>(sourceElement,
				delegate(UIElement element)
				{
					return element is ITimeRangeArea;
				});

			ScheduleControlBase control = null;

			if (_timeRangeArea != null)
				control = PresentationUtilities.GetVisualAncestor<ScheduleControlBase>(sourceElement, null);

			if (control == null)
				control = this._control;
			else
				this._controlUnderPoint = control;

			// we only want to scroll in the direction of the TimeslotOrientation.
			// this means if we are in dayview's all day activity area we should allow timeslot scrolling
			TimeslotPanelBase panel = this._timeRangeArea != null ? ((ITimeRangeArea)(this._timeRangeArea)).TimeRangePanel : null;

			this._calendarUnderPoint = panel != null ? panel.DataContext as ResourceCalendar : null;

			this._timeslotScrollInfo = panel != null ? control.GetTimeslotScrollInfo(panel) : null;
			this._canScroll = this._timeslotScrollInfo != null;

			this._isVertical = panel != null ? panel.TimeslotOrientation == Orientation.Vertical : false;

			if (this._canScroll)
			{
				if (this._isVertical)
					this._timeRangeExtent = this._timeRangeArea.RenderSize.Height;
				else
					this._timeRangeExtent = this._timeRangeArea.RenderSize.Width;

				if (calledFronCtor)
					this._originalScrollOffset = _timeslotScrollInfo.Offset;

				TimeslotCollection timeslots = panel.Timeslots;
				DateRange? startRange = timeslots.GetDateRange(0);

				if (startRange != null)
					this._areaStart = startRange.Value.Start;

				DateRange? endRange = timeslots.GetDateRange(timeslots.Count - 1);

				if (endRange != null)
					this._areaEnd = endRange.Value.End;
			}
		}

		#endregion //InitializeTimeslotScrollInfo

		#region KillTimer

		protected void KillTimer()
		{
			if (this._scrollTimer != null)
			{
				this._scrollTimer.Stop();
				this._scrollTimer.Tick -= new EventHandler(OnScrollTimerTick);
				this._scrollTimer = null;
			}
		}

		#endregion //KillTimer

		#region OnMouseMove

		protected abstract void OnMouseMove(object sender, MouseEventArgs e);

		#endregion //OnMouseMove

		#region CheckIfScrollIsRequired

		protected abstract void CheckIfScrollIsRequired();

		#endregion //CheckIfScrollIsRequired

		#region ProcessCancel

		protected abstract void ProcessCancel();

		#endregion //ProcessCancel

		#region ProcessCommit

		protected abstract void ProcessCommit();

		#endregion //ProcessCommit

		#region ShouldCopy

		protected static bool ShouldCopy
		{
			get { return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control; }
		}

		#endregion //ShouldCopy

		#region StartTimer

		protected void StartTimer()
		{
			if (this._scrollTimer == null)
			{
				this._scrollTimer = new DispatcherTimer();
				this._scrollTimer.Interval = TimeSpan.FromMilliseconds(ScrollInterval);
				this._scrollTimer.Start();
				this._scrollTimer.Tick += new EventHandler(OnScrollTimerTick);
			}
		}

		#endregion //StartTimer

		#region WireMouseEvents

		protected void WireMouseEvents()
		{
			this._areEventsWired = true;

			this._rootElement.LostMouseCapture += new MouseEventHandler(OnLostMouseCapture);
			this._rootElement.MouseMove += new MouseEventHandler(OnMouseMove);
			this._rootElement.MouseLeftButtonUp += new MouseButtonEventHandler(OnMouseLeftButtonUp);
		}

		#endregion //WireMouseEvents

		#endregion //Protected Methods

		#region Private Methods

		#region OnLostMouseCapture

		private void OnLostMouseCapture(object sender, MouseEventArgs e)
		{
			this.UnwireMouseEvents();

			this.ProcessCancel();

			e.Handled = true;

		}

		#endregion //OnLostMouseCapture

		#region OnMouseLeftButtonUp

		private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.UnwireMouseEvents();

			this._rootElement.ReleaseMouseCapture();

			this.ProcessCommit();

			e.Handled = true;

		}

		#endregion //OnMouseLeftButtonUp

		#region OnScrollTimerTick

		private void OnScrollTimerTick(object sender, EventArgs e)
		{
			this.CheckIfScrollIsRequired();
		}

		#endregion //OnScrollTimerTick

		#region UnwireMouseEvents

		private void UnwireMouseEvents()
		{
			this._areEventsWired = false;

			this._rootElement.LostMouseCapture -= new MouseEventHandler(OnLostMouseCapture);
			this._rootElement.MouseMove -= new MouseEventHandler(OnMouseMove);
			this._rootElement.MouseLeftButtonUp -= new MouseButtonEventHandler(OnMouseLeftButtonUp);

			this._rootElement.ClearValue(FrameworkElement.CursorProperty);

			this.KillTimer();
		}

		#endregion //UnwireMouseEvents

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