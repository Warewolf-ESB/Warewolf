using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.AutomationPeers;
using Infragistics.Controls.Primitives;
using System.Windows.Input;
using System.Diagnostics;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom control used to display activity relative to time slots in a horizontal arrangement.
	/// </summary>
	[TemplatePart(Name = PartGroupHeadersResizer, Type = typeof(ScheduleResizerBar))] // AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
	public class XamScheduleView : ScheduleTimeControlBase
	{
		#region Member Variables

		private const string PartGroupHeadersResizer = "GroupHeadersResizer"; // AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth

		// template elements
		private ScheduleResizerBar _resizerBar; // AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth

		// AS 2/24/12 TFS102945
		private ScrollInfoTouchHelper _scrollTouchHelper;
		private ScrollInfoTouchHelper _primaryTimeZoneScrollTouchHelper;
		private ScrollInfoTouchHelper _secondaryTimeZoneScrollTouchHelper;

		#endregion // Member Variables

		#region Constructor
		static XamScheduleView()
		{

			XamScheduleView.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamScheduleView), new FrameworkPropertyMetadata(typeof(XamScheduleView)));

		}

		/// <summary>
		/// Initializes a new <see cref="XamScheduleView"/>
		/// </summary>
		public XamScheduleView()
		{




			this.TimeslotAreaTimeslotExtent = 57;

			// AS 2/24/12 TFS102945
			var tupleWidth = Tuple.Create(this.TimeslotScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)GetTouchFirstItemWidth);
			var tupleHeight = Tuple.Create(this.GroupScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)GetTouchFirstItemHeight);
			_scrollTouchHelper = new ScrollInfoTouchHelper(tupleWidth, tupleHeight, this.GetScrollModeFromPoint);
			_primaryTimeZoneScrollTouchHelper = new ScrollInfoTouchHelper(tupleWidth, tupleHeight, this.GetScrollModeFromPoint);
			_secondaryTimeZoneScrollTouchHelper = new ScrollInfoTouchHelper(tupleWidth, tupleHeight, this.GetScrollModeFromPoint);
		}
		#endregion //Constructor

		#region Base class overrides

		#region CalculateIsAllDaySelection
		internal override bool CalculateIsAllDaySelection(DateRange? selectedRange)
		{
			return false;
		}
		#endregion // CalculateIsAllDaySelection

		#region CalendarGroupOrientation
		internal override Orientation CalendarGroupOrientation
		{
			get { return Orientation.Vertical; }
		}
		#endregion // CalendarGroupOrientation

		// AS 3/6/12 TFS102945
		#region ClearTouchActionQueue
		internal override void ClearTouchActionQueue()
		{
			_scrollTouchHelper.ClearPendingActions();
		}
		#endregion //ClearTouchActionQueue

		#region CreateCalendarHeader

		internal override CalendarHeader CreateCalendarHeader()
		{
			return new CalendarHeaderVertical();
		}

		#endregion //CreateCalendarHeader	

		#region CreateTimeslotHeader
		internal override TimeslotHeaderAdapter CreateTimeslotHeader( DateTime start, DateTime end, TimeZoneToken token )
		{
			return new ScheduleViewTimeslotHeaderAdapter(start, end, token);
		}
		#endregion // CreateTimeslotHeader

		#region DefaultCalendarDisplayMode
		internal override CalendarDisplayMode DefaultCalendarDisplayMode
		{
			get { return Infragistics.Controls.Schedules.CalendarDisplayMode.Separate; }
		}
		#endregion // DefaultCalendarDisplayMode

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region DefaultMinCalendarGroupExtent
		internal override double DefaultMinCalendarGroupExtent
		{
			get 
			{
				double min = this.SingleLineActivityHeight * 3;
				min += this.ActivityGutterHeight;

				Debug.Assert(!double.IsNaN(min), "Can't calculate min?");

				min += 4; // add a couple for the chrome within a group since this is the extent for the entire group

				return min; 
			}
		}
		#endregion // DefaultMinCalendarGroupExtent

		// AS 3/6/12 TFS102945
		#region EnqueueTouchAction
		internal override void EnqueueTouchAction(Action<ScrollInfoTouchAction> action)
		{
			_scrollTouchHelper.EnqueuePendingAction(action);
		}
		#endregion //EnqueueTouchAction

		#region GetCalendarGroupAutomationScrollInfo

		/// <summary>
		/// Returns the horizontal and vertical scrollinfo instances to be used by the automation peer for the CalendarGroupTimeslotArea.
		/// </summary>
		/// <param name="horizontal">Out parameter set to the horizontal scrollinfo or null</param>
		/// <param name="vertical">Out parameter set to the vertical scrollinfo or null</param>
		internal override void GetCalendarGroupAutomationScrollInfo(out ScrollInfo horizontal, out ScrollInfo vertical)
		{
			vertical = null;
			horizontal = this.TimeslotMergedScrollInfo;
		}

		#endregion // GetCalendarGroupAutomationScrollInfo

		// AS 3/7/12 TFS102945
		#region GetHeaderAreaTouchHelper
		internal override ScrollInfoTouchHelper GetHeaderAreaTouchHelper(bool isPrimary)
		{
			return isPrimary ? _primaryTimeZoneScrollTouchHelper : _secondaryTimeZoneScrollTouchHelper;
		}
		#endregion //GetHeaderAreaTouchHelper

		#region GetResolvedCalendarHeaderAreaVisibility
		internal override Visibility GetResolvedCalendarHeaderAreaVisibility()
		{
			if (this.CalendarDisplayModeResolved == Schedules.CalendarDisplayMode.Merged)
				return Visibility.Collapsed;

			return Visibility.Visible;
		}
		#endregion // GetResolvedCalendarHeaderAreaVisibility

		#region GetTimeslotRangeInDirection
		internal override DateRange? GetTimeslotRangeInDirection( SpatialDirection direction, bool extendSelection, DateRange selectionRange )
		{
			selectionRange.Normalize();
			bool isNear = direction == SpatialDirection.Up || direction == SpatialDirection.Left;
			DateTime date = isNear || selectionRange.IsEmpty ? selectionRange.Start : ScheduleUtilities.GetNonInclusiveEnd(selectionRange.End);
			var dateProvider = this.DateInfoProviderResolved;

			switch (direction)
			{
				case SpatialDirection.Down:
				case SpatialDirection.Up:
				{
					return null;
				}
			}

			int timeslotIndex;
			var group = this.TimeslotInfo.GetTimeslotRangeGroup(date, out timeslotIndex);

			if (null == group)
				return null;

			// do not navigate to another date
			if (isNear && timeslotIndex == 0)
				return null;
				
			int totalCount = ScheduleUtilities.GetTimeslotCount(group.Ranges);

			// same for the last timeslot and going to the right
			if (!isNear && timeslotIndex == totalCount - 1)
				return null;

			// otherwise get the info for the adjacent range
			return ScheduleUtilities.CalculateDateRange(group.Ranges, timeslotIndex + (isNear ? -1 : 1));
		}
		#endregion // GetTimeslotRangeInDirection

		#region IsTimeslotRangeGroupBreak
		/// <summary>
		/// Used by the <see cref="ScheduleControlBase.TimeslotInfo"/> to determine if 2 visible dates are considered to be part of the same group of timeslots.
		/// </summary>
		/// <param name="previousDate">Previous date processed</param>
		/// <param name="nextDate">Next date to be processed</param>
		/// <returns></returns>
		internal override bool IsTimeslotRangeGroupBreak(DateTime previousDate, DateTime nextDate)
		{
			// all dates displayed together
			return false;
		}
		#endregion // IsTimeslotRangeGroupBreak

		#region Navigate
		internal override bool NavigateTimeslot(SpatialDirection direction)
		{
			if (direction == SpatialDirection.Up || direction == SpatialDirection.Down)
				return this.ActivateNextPreviousGroup(direction == SpatialDirection.Down, false);

			return base.NavigateTimeslot(direction);
		} 
		#endregion // Navigate

		#region NavigatePage
		internal override bool NavigatePage(bool up)
		{
			return this.ActivateNextPreviousGroup(!up, true);
		}
		#endregion // NavigatePage

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if ( null != this.GroupHeadersPanel )
				this.GroupHeadersPanel.MouseWheel -= new MouseWheelEventHandler(OnGroupHeadersMouseWheel);

			base.OnApplyTemplate();

			// AS 2/24/12 TFS102945
			this.InitializeTouchScrollAreaHelper(_scrollTouchHelper,this.GroupsPanel);

			if ( null != this.GroupHeadersPanel )
			{
				this.GroupHeadersPanel.PreferredNonScrollingExtent = this.CalendarHeaderAreaWidth;

				// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				this.GroupHeadersPanel.MouseWheel += new MouseWheelEventHandler(OnGroupHeadersMouseWheel);
			}

			// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
			#region GroupHeadersResizer

			var oldResizer = _resizerBar;

			if ( oldResizer != null )
			{
				oldResizer.Host = null;
			}

			_resizerBar = this.GetTemplateChild(PartGroupHeadersResizer) as ScheduleResizerBar;

			if ( null != _resizerBar )
			{
				_resizerBar.Host = new CalendarHeaderResizeHost(this);
			}

			#endregion //GroupHeadersResizer
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamScheduleView"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="XamScheduleViewAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamScheduleViewAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		// AS 3/14/12 Touch Support
		#region OnIsTouchSupportEnabledChanged
		internal override void OnIsTouchSupportEnabledChanged(bool oldValue, bool newValue)
		{
			base.OnIsTouchSupportEnabledChanged(oldValue, newValue);
			_secondaryTimeZoneScrollTouchHelper.IsEnabled = _primaryTimeZoneScrollTouchHelper.IsEnabled = _scrollTouchHelper.IsEnabled = newValue;
		}
		#endregion //OnIsTouchSupportEnabledChanged

		#region OnTimeslotPanelAttached
		internal override void OnTimeslotPanelAttached(TimeslotPanelBase panel)
		{
			if (PresentationUtilities.GetTemplatedParent(panel) is TimeslotArea)
			{
				ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

				if (null != ap)
				{
					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					//ap.ClickToAddType = ClickToAddType.NonIntersectingAppointment;
					ap.ClickToAddType = this.IsClickToAddEnabled ? ClickToAddType.NonIntersectingActivity : ClickToAddType.None;
					ap.ConstrainToInViewRect = true;
					ap.IsActivityScrollInfoEnabled = true;
				}
			}

			base.OnTimeslotPanelAttached(panel);
		}
		#endregion // OnTimeslotPanelAttached

		// AS 6/17/11 TFS78894
		#region PerformInitialScrollIntoView
		/// <summary>
		/// This method is not meant to be called directly except by the ScheduleTimeControlBase
		/// </summary>
		internal override bool PerformInitialScrollIntoViewImpl()
		{
			if (this.ActiveCalendar == null)
				return false;

			// ScheduleView in Outlook seems to perform the initial scroll such that it puts the current time 
			// about 1/3 of the way in the current viewable range.
			if (this.BringTimeslotIntoView(DateTime.Now, 1 / 3d))
				return true;

			return base.PerformInitialScrollIntoViewImpl();
		}
		#endregion //PerformInitialScrollIntoView

		// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region ProcessMouseWheel
		internal override void ProcessMouseWheel( ITimeRangeArea timeRangeArea, System.Windows.Input.MouseWheelEventArgs e )
		{
			if ( null != timeRangeArea )
			{
				if (timeRangeArea.TimeRangePanel is TimeslotPanelBase)
				{
					this.ScrollGroups(e);
				}
			}

			base.ProcessMouseWheel(timeRangeArea, e);
		}
		#endregion // ProcessMouseWheel

		#region SetTemplateItemExtent
		internal override void SetTemplateItemExtent(Enum itemId, double value)
		{
			base.SetTemplateItemExtent(itemId, value);

			if (ScheduleTimeControlTemplateValue.TimeslotHeaderWidth.Equals(itemId))
			{
				if (!double.IsNaN(value))
					this.TimeslotAreaTimeslotExtent = Math.Max(8, value);
			}

			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			if ( ScheduleControlTemplateValue.MoreActivityIndicatorHeight.Equals(itemId) ||
				ScheduleControlTemplateValue.SingleLineApptHeight.Equals(itemId) )
			{
				if ( this.MinCalendarGroupExtent == null )
					this.OnMinCalendarGroupExtentResolvedChanged();
			}
		}
		#endregion // SetTemplateItemExtent

		// AS 4/20/11 TFS73205
		// Outlook doesn't allow non-integral intervals or intevals > 60 minutes but since they seem to 
		// always use the large hour and smaller minute always return true.
		//
		#region ShouldBumpHeaderHourFontSize
		internal override bool ShouldBumpHeaderHourFontSize(TimeslotBase timeslot)
		{
			return true;
		}
		#endregion //ShouldBumpHeaderHourFontSize

		// AS 3/6/12 TFS102945
		#region ShouldQueueTouchActions
		internal override bool ShouldQueueTouchActions
		{
			get { return _scrollTouchHelper.ShouldDeferMouseActions; }
		}
		#endregion //ShouldQueueTouchActions

		#region ShowCalendarCloseButtonDefault

		internal override bool ShowCalendarCloseButtonDefault { get { return false; } }

		#endregion //ShowCalendarCloseButtonDefault	

		#region ShowCalendarOverlayButtonDefault

		internal override bool ShowCalendarOverlayButtonDefault { get { return false; } }

		#endregion //ShowCalendarOverlayButtonDefault

		#region UseSingleTimeslotGroup
		internal override bool UseSingleTimeslotGroup
		{
			get { return true; }
		}
		#endregion //UseSingleTimeslotGroup

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region VerifyClickToAddState
		internal override void VerifyClickToAddState(TimeslotPanelBase panel, bool isEnabled)
		{
			ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

			if (ap != null)
			{
				ap.ClickToAddType = isEnabled ? ClickToAddType.NonIntersectingActivity : ClickToAddType.None;
			}

			base.VerifyClickToAddState(panel, isEnabled);
		}
		#endregion //VerifyClickToAddState

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region AllowCalendarHeaderAreaResizing

		/// <summary>
		/// Identifies the <see cref="AllowCalendarHeaderAreaResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCalendarHeaderAreaResizingProperty = DependencyPropertyUtilities.Register("AllowCalendarHeaderAreaResizing",
			typeof(bool), typeof(XamScheduleView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a boolean indicating if the end user is allowed to resize the calendar header area.
		/// </summary>
		/// <seealso cref="AllowCalendarHeaderAreaResizingProperty"/>
		public bool AllowCalendarHeaderAreaResizing
		{
			get
			{
				return (bool)this.GetValue(XamScheduleView.AllowCalendarHeaderAreaResizingProperty);
			}
			set
			{
				this.SetValue(XamScheduleView.AllowCalendarHeaderAreaResizingProperty, value);
			}
		}

		#endregion //AllowCalendarHeaderAreaResizing

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region CalendarHeaderAreaWidth

		/// <summary>
		/// Identifies the <see cref="CalendarHeaderAreaWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarHeaderAreaWidthProperty = DependencyPropertyUtilities.Register("CalendarHeaderAreaWidth",
			typeof(double), typeof(XamScheduleView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnCalendarHeaderAreaWidthChanged))
			);

		private static void OnCalendarHeaderAreaWidthChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleView instance = d as XamScheduleView;

			if (null != instance.GroupHeadersPanel)
				instance.GroupHeadersPanel.PreferredNonScrollingExtent = (double)e.NewValue;
		}

		/// <summary>
		/// Returns or sets the width of the area containing the <see cref="CalendarHeader"/> elements
		/// </summary>
		/// <seealso cref="CalendarHeaderAreaWidthProperty"/>
		public double CalendarHeaderAreaWidth
		{
			get
			{
				return (double)this.GetValue(XamScheduleView.CalendarHeaderAreaWidthProperty);
			}
			set
			{
				this.SetValue(XamScheduleView.CalendarHeaderAreaWidthProperty, value);
			}
		}

		#endregion //CalendarHeaderAreaWidth

		#endregion //Public Properties

		#endregion //Properties

		#region Methods

		#region Private Methods

		// AS 2/24/12 TFS102945
		#region GetTouchFirstItemHeight
		private double GetTouchFirstItemHeight()
		{
			var groupElement = this.GroupWrappers.Select((g) => ((ISupportRecycling)g.GroupTimeslotArea).AttachedElement).FirstOrDefault((e) => e != null);

			// the height of the first item is the height of a group
			return groupElement == null ? 0 : ScheduleUtilities.Max(groupElement.ActualHeight, 0);
		}
		#endregion //GetTouchFirstItemHeight

		// AS 2/24/12 TFS102945
		#region GetTouchFirstItemWidth
		private double GetTouchFirstItemWidth()
		{
			var hPanel = this.TimeslotPanels.FirstOrDefault((p) => p is TimeslotPanel);

			// the width of the first item is the width of a single timeslot since horizonal scrolling involves scrolling timeslots
			return hPanel == null ? this.TimeslotAreaTimeslotExtent : hPanel.ActualTimeslotExtent;
		}
		#endregion //GetTouchFirstItemWidth

		// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region OnGroupHeadersMouseWheel
		private void OnGroupHeadersMouseWheel( object sender, MouseWheelEventArgs e )
		{
			this.ScrollGroups(e);
		}
		#endregion // OnGroupHeadersMouseWheel

		// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region ScrollGroups
		private void ScrollGroups( MouseWheelEventArgs e )
		{
			var si = this.GroupScrollbarMediator.ScrollInfo;

			if ( null != si && e.Delta != 0 )
			{
				bool scrollDown = e.Delta < 0;
				si.Scroll(scrollDown ? 1 : -1);
				e.Handled = true;
			}
		} 
		#endregion // ScrollGroups

		#endregion // Private Methods 

		#endregion // Methods

		#region ScheduleViewTimeslotHeaderAdapter class
		private class ScheduleViewTimeslotHeaderAdapter : TimeslotHeaderAdapter
		{
			#region Constructor
			internal ScheduleViewTimeslotHeaderAdapter(DateTime start, DateTime end, TimeZoneToken token)
				: base(start, end, token)
			{
			}
			#endregion // Constructor

			#region Base class overrides
			protected override TimeRangePresenterBase CreateInstanceOfRecyclingElement()
			{
				return new ScheduleViewTimeslotHeader();
			}

			protected override Type RecyclingElementType
			{
				get
				{
					return typeof(ScheduleViewTimeslotHeader);
				}
			}
			#endregion // Base class overrides
		}
		#endregion // ScheduleViewTimeslotHeaderAdapter class

		// AS 11/11/10 NA 11.1 - CalendarHeaderAreaWidth
		#region CalendarHeaderResizeHost class
		private class CalendarHeaderResizeHost : IResizerBarHost
		{
			#region Member Variables

			private XamScheduleView _control;

			#endregion // Member Variables

			#region Constructor
			internal CalendarHeaderResizeHost( XamScheduleView control )
			{
				_control = control;
			}
			#endregion // Constructor

			#region IResizerBarHost Members

			Orientation IResizerBarHost.ResizerBarOrientation
			{
				get { return Orientation.Vertical; }
			}

			bool IResizerBarHost.CanResize()
			{
				return _control.GroupHeadersPanel != null && _control.AllowCalendarHeaderAreaResizing;
			}

			void IResizerBarHost.SetExtent( double extent )
			{
				Debug.Assert(double.IsNaN(extent) || CoreUtilities.GreaterThanOrClose(extent, 0), "Should be NaN or positive double");
				_control.CalendarHeaderAreaWidth = extent;
			}

			ResizeInfo IResizerBarHost.GetResizeInfo()
			{
				if ( ((IResizerBarHost)this).CanResize() )
				{
					double actualExtent = _control.GroupHeadersPanel.ActualWidth;
					return new ResizeInfo(
						_control, 
						actualExtent, // use the actual positioned size as the starting point
						_control.CalendarHeaderAreaWidth,		// if escape or otherwise cancelled go to original property value
						Math.Min(actualExtent, 20)				// just going to impose the minimum used by outlook
						);
				}

				return null;
			}

			#endregion //IResizerBarHost Members
		}
		#endregion // CalendarHeaderResizeHost class
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