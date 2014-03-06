using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Infragistics.Collections;
using System.Windows.Controls.Primitives;
using Infragistics.Controls.Primitives;
using System.Diagnostics;
using Infragistics.AutomationPeers;
using System.Windows.Input;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom control used to display activity relative to time slots in a vertical arrangement.
	/// </summary>
	[TemplatePart(Name = PartDayHeadersPanel, Type = typeof(ScheduleStackPanel))]
	[TemplatePart(Name = PartDayHeadersScrollBar, Type = typeof(ScrollBar))]
	[TemplatePart(Name = PartMultiDayActivityAreaResizer, Type = typeof(ScheduleResizerBar))] // AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
	public class XamDayView : ScheduleTimeControlBase
	{
		#region Member Variables

		private const string PartDayHeadersPanel = "DayHeadersPanel";
		private const string PartDayHeadersScrollBar = "DayHeadersScrollBar";
		private const string PartMultiDayActivityAreaResizer = "MultiDayActivityAreaResizer"; // AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight

		// template elements
		private ScheduleStackPanel _dayHeadersPanel;
		private ScrollBar _dayHeadersScrollBar;
		private ScheduleResizerBar _multiDayAreaResizer; // AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight

		// internal members
		private ScrollBarInfoMediator _dayHeadersScrollbarMediator;
		private ScrollBarVisibilityCoordinator _scrollBarVisibilityCoordinator;
		private ObservableCollectionExtended<DayViewDayHeaderAreaAdapter> _headers;
		private InternalFlags _flags = InternalFlags.AllVerifyStateFlags;
		private MergedScrollInfo _dayHeaderActivityScrollInfo;

		private Predicate<ActivityBase> _timeslotActivityFilter;
		private Visibility _cachedAllDayAreaVisibility;

		// AS 2/24/12 TFS102945
		private ScrollInfoTouchHelper _scrollTouchHelperTimeslots;
		private ScrollInfoTouchHelper _scrollTouchHelperAllDay;
		private ScrollInfoTouchHelper _primaryTimeZoneScrollTouchHelper;
		private ScrollInfoTouchHelper _secondaryTimeZoneScrollTouchHelper;

		#endregion //Member Variables

		#region Constructor
		static XamDayView()
		{

			XamDayView.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDayView), new FrameworkPropertyMetadata(typeof(XamDayView)));

		}

		/// <summary>
		/// Initializes a new <see cref="XamDayView"/>
		/// </summary>
		public XamDayView()
		{




			_headers = new ObservableCollectionExtended<DayViewDayHeaderAreaAdapter>();

			this.TimeslotAreaTimeslotExtent = 23;

			_dayHeaderActivityScrollInfo = new MergedScrollInfo();
			_dayHeaderActivityScrollInfo.DirtyAction = this.OnMergedScrollInfoChanged;
			_dayHeaderActivityScrollInfo.ScrollBarVisibility = this.TimeslotScrollBarVisibility;
			_dayHeadersScrollbarMediator = new ScrollBarInfoMediator(_dayHeaderActivityScrollInfo);

			_scrollBarVisibilityCoordinator = new ScrollBarVisibilityCoordinator();
			_scrollBarVisibilityCoordinator.Add(this.TimeslotScrollbarMediator);
			this.AddRemoveDayHeaderScrollBarMediator();

			// AS 2/24/12 TFS102945
			var tupleWidth = Tuple.Create(this.GroupScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)GetTouchFirstGroupWidth);
			var tupleHeightTimeslots = Tuple.Create(this.TimeslotScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)GetTouchFirstTimeslotHeight);

			_scrollTouchHelperTimeslots = new ScrollInfoTouchHelper(tupleWidth, tupleHeightTimeslots, this.GetScrollModeFromPoint);
			_primaryTimeZoneScrollTouchHelper = new ScrollInfoTouchHelper(null, tupleHeightTimeslots, this.GetScrollModeFromPoint);
			_secondaryTimeZoneScrollTouchHelper = new ScrollInfoTouchHelper(null, tupleHeightTimeslots, this.GetScrollModeFromPoint);

			_scrollTouchHelperAllDay = new ScrollInfoTouchHelper(tupleWidth,
				Tuple.Create(this._dayHeadersScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)GetTouchFirstAllDayItemHeight)
				, this.GetScrollModeFromPoint
				);
		}
		#endregion //Constructor

		#region Base class overrides

		#region CalculateIsAllDaySelection
		internal override bool CalculateIsAllDaySelection(DateRange? selectedRange)
		{
			if (_cachedAllDayAreaVisibility == System.Windows.Visibility.Collapsed)
				return false;

			return base.CalculateIsAllDaySelection(selectedRange);
		} 
		#endregion // CalculateIsAllDaySelection

		// AS 3/6/12 TFS102945
		#region ClearTouchActionQueue
		internal override void ClearTouchActionQueue()
		{
			_scrollTouchHelperAllDay.ClearPendingActions();
			_scrollTouchHelperTimeslots.ClearPendingActions();
		}
		#endregion //ClearTouchActionQueue

		#region CreateCalendarGroupWrapper
		internal override CalendarGroupWrapper CreateCalendarGroupWrapper(CalendarGroupBase calendarGroup)
		{
			return new DayViewCalendarGroupWrapper(this, calendarGroup);
		} 
		#endregion // CreateCalendarGroupWrapper

		#region CreateTimeslotAreaAdapter
		/// <summary>
		/// Helper method for creating a timeslotgroup adapter
		/// </summary>
		/// <param name="groupInfo">Provides the date information for the group</param>
		/// <param name="creatorFunc">The callback used to create an instance of the timeslotbase instance.</param>
		/// <param name="initializer">The method used to initialize the state of the timeslotbase instance.</param>
		/// <param name="modifyDateFunc">Optional callback used to adjust the start/end date for a timeslot.</param>
		/// <param name="activityOwner">An optional group that provides the activity information or null if no activity will be shown.</param>
		/// <returns></returns>
		internal override TimeslotAreaAdapter CreateTimeslotAreaAdapter(
			TimeslotRangeGroup groupInfo,
			Func<DateTime, DateTime, TimeslotBase> creatorFunc,
			Action<TimeslotBase> initializer,
			Func<DateTime, DateTime> modifyDateFunc,
			CalendarGroupBase activityOwner )
		{
			TimeslotRange[] ranges = groupInfo.Ranges;

			if ( ranges.Length == 0 )
				return null;

			DateTime firstSlotStart = ranges[0].StartDate;
			DateTime lastSlotEnd = ranges[ranges.Length - 1].EndDate;

			TimeslotAreaAdapter group = new DayViewTimeslotAreaAdapter(this,
				// since the group represents a date, we should track the start and end date
				groupInfo.Start,
				groupInfo.End,
				firstSlotStart,
				lastSlotEnd,
				new TimeslotCollection(creatorFunc, modifyDateFunc, ranges, initializer),
				activityOwner);
			return group;
		}
		#endregion // CreateTimeslotAreaAdapter

		#region CreateTimeslotHeader
		internal override TimeslotHeaderAdapter CreateTimeslotHeader( DateTime start, DateTime end, TimeZoneToken token )
		{
			return new DayViewTimeslotHeaderAdapter(start, end, token);
		}
		#endregion // CreateTimeslotHeader

		#region DisplayActivityDialog
		internal override bool DisplayActivityDialog(TimeRangePresenterBase element)
		{
			MultiDayActivityAreaAdapter timeslot = element.Timeslot as MultiDayActivityAreaAdapter;

			if (null != timeslot)
			{
				ResourceCalendar calendar = element.DataContext as ResourceCalendar;
				Debug.Assert(null != calendar);

				var dm = this.DataManagerResolved;
				bool isAllDayActivity = null == dm || dm.IsTimeZoneNeutralActivityAllowed( ActivityType.Appointment, calendar );

				return this.DisplayActivityDialog(calendar, timeslot.Start, TimeSpan.FromDays(1), isAllDayActivity);
			}

			return base.DisplayActivityDialog(element);
		}
		#endregion // DisplayActivityDialog

		// AS 3/6/12 TFS102945
		#region EnqueueTouchAction
		internal override void EnqueueTouchAction(Action<ScrollInfoTouchAction> action)
		{
			_scrollTouchHelperAllDay.EnqueuePendingAction(action);
			_scrollTouchHelperTimeslots.EnqueuePendingAction(action);
		}
		#endregion //EnqueueTouchAction

		#region GetActivityNavigationList
		internal override List<Tuple<ScheduleActivityPanel, DateRange?>> GetActivityNavigationList( CalendarGroupBase group )
		{
			if ( this._cachedAllDayAreaVisibility != System.Windows.Visibility.Collapsed )
			{
				var panels = this.GetActivityPanels(group, true);

				ScheduleActivityPanel allDayPanel;
				if (ScheduleUtilities.Remove(panels, ( ScheduleActivityPanel p ) => { return PresentationUtilities.GetTemplatedParent(p) is DayViewDayHeaderArea; }, out allDayPanel))
				{
					Debug.Assert(panels.Count == this.VisibleDates.Count, "We don't have the same amount of panels as we do visible dates?");
					var list = new List<Tuple<ScheduleActivityPanel, DateRange?>>();

					for (int i = 0, count = panels.Count; i < count; i++)
					{
						var panel = panels[i];
						list.Add(Tuple.Create(allDayPanel, panel.TimeslotRange));
						list.Add(Tuple.Create(panel, (DateRange?)null));
					}

					return list;
				}
			}

			return base.GetActivityNavigationList( group );
		}
		#endregion // GetActivityNavigationList

		#region GetCalendarGroupAutomationScrollInfo

		/// <summary>
		/// Returns the horizontal and vertical scrollinfo instances to be used by the automation peer for the CalendarGroupTimeslotArea.
		/// </summary>
		/// <param name="horizontal">Out parameter set to the horizontal scrollinfo or null</param>
		/// <param name="vertical">Out parameter set to the vertical scrollinfo or null</param>
		internal override void GetCalendarGroupAutomationScrollInfo(out ScrollInfo horizontal, out ScrollInfo vertical)
		{
			horizontal = null;
			vertical = this.TimeslotMergedScrollInfo;
		}

		#endregion // GetCalendarGroupAutomationScrollInfo

		// AS 3/7/12 TFS102945
		#region GetHeaderAreaTouchHelper
		internal override ScrollInfoTouchHelper GetHeaderAreaTouchHelper(bool isPrimary)
		{
			return isPrimary ? _primaryTimeZoneScrollTouchHelper : _secondaryTimeZoneScrollTouchHelper;
		}
		#endregion //GetHeaderAreaTouchHelper

		#region GetNowTimeAdjustment
		internal override TimeSpan GetNowTimeAdjustment()
		{
			DateTime? currentDate = this.CurrentLogicalDate;

			if (currentDate == null ||
				this.VisibleDates.Count == 0 || 
				this.VisibleDates[0] == currentDate)
				return TimeSpan.Zero;

			return this.VisibleDates[0].Subtract(currentDate.Value);
		}
		#endregion // GetNowTimeAdjustment

		#region GetTimeslotFromSelection
		/// <summary>
		/// Return the timeslot that represents the start or end of the current timeslot selection.
		/// </summary>
		/// <param name="selection">The selection range to evaluate</param>
		/// <param name="fromStart">True to get the starting timeslot for the selection; otherwise false to get the ending timeslot for the selection</param>
		/// <returns>The range that represents the timeslot for the start/end of the range</returns>
		internal override DateRange GetTimeslotFromSelection(DateRange selection, bool fromStart)
		{
			selection.Normalize();

			if (!this.CalculateIsAllDaySelection(selection))
				return base.GetTimeslotFromSelection(selection, fromStart);

			DateTime date = fromStart || selection.IsEmpty ? selection.Start : ScheduleUtilities.GetNonInclusiveEnd(selection.End);
			var dayOffset = ScheduleUtilities.GetLogicalDayOffset(this);
			return ScheduleUtilities.GetLogicalDayRange(date, dayOffset);
		}
		#endregion // GetTimeslotFromSelection

		#region GetTimeslotRangeInDirection
		internal override DateRange? GetTimeslotRangeInDirection( SpatialDirection direction, bool extendSelection, DateRange selectionRange )
		{
			selectionRange.Normalize();

			bool isAllDayRange = this.CalculateIsAllDaySelection(selectionRange);
			var dayOffset = ScheduleUtilities.GetLogicalDayOffset(this);
			bool isNear = direction == SpatialDirection.Up || direction == SpatialDirection.Left;
			DateTime date = isNear || selectionRange.IsEmpty ? selectionRange.Start : ScheduleUtilities.GetNonInclusiveEnd(selectionRange.End);
			DateTime logicalDate = this.ConvertToLogicalDate(date).Date;

			if (isAllDayRange)
			{
				#region Multi Day Area
				switch (direction)
				{
					case SpatialDirection.Left:
					case SpatialDirection.Right:
						DateTime? adjacentDate = GetNextPreviousLogicalDate(logicalDate, !isNear, true, extendSelection);

						if (null == adjacentDate)
							return null;

						adjacentDate = this.ConvertFromLogicalDate(adjacentDate.Value);
						return ScheduleUtilities.GetLogicalDayRange(adjacentDate.Value, dayOffset);

					case SpatialDirection.Down:
						// return the 1st slot
						return this.TimeslotInfo.GetTimeslotRange(selectionRange.Start);
					case SpatialDirection.Up:
						// can't go anywhere from here
						return null;
				} 
				#endregion // Multi Day Area
			}

			var dateProvider = this.DateInfoProviderResolved;
			int timeslotIndex;
			var group = this.TimeslotInfo.GetTimeslotRangeGroup(date, out timeslotIndex);

			Debug.Assert(null != group, "Cannot find range group for date?");

			if (group == null)
				return null;

			switch (direction)
			{
				default:
				case SpatialDirection.Left:
				case SpatialDirection.Right:
				{
					DateTime? adjacentDate = GetNextPreviousLogicalDate(logicalDate, !isNear, true, extendSelection);

					if (null == adjacentDate)
						return null;

					int dayDelta = adjacentDate.Value.Subtract(logicalDate).Days;
					adjacentDate = this.ConvertFromLogicalDate(adjacentDate.Value);

					DateRange currentRange = ScheduleUtilities.CalculateDateRange(group.Ranges, timeslotIndex);

					currentRange.Start = currentRange.Start.AddDays(dayDelta);
					currentRange.End = currentRange.End.AddDays(dayDelta);
					return currentRange;
				}
				case SpatialDirection.Up:
				{
					if (timeslotIndex == 0)
					{
						if (this._cachedAllDayAreaVisibility == System.Windows.Visibility.Collapsed)
							return null;

						// when going up from the top timeslot we navigate to the all day area
						return ScheduleUtilities.GetLogicalDayRange(date, dayOffset);
					}

					return ScheduleUtilities.CalculateDateRange(group.Ranges, timeslotIndex - 1);
				}
				case SpatialDirection.Down:
				{
					int totalCount = ScheduleUtilities.GetTimeslotCount(group.Ranges);

					if (timeslotIndex == totalCount - 1)
						return null;

					return ScheduleUtilities.CalculateDateRange(group.Ranges, timeslotIndex + 1);
				}
			}
		}
		#endregion // GetTimeslotRangeInDirection

		#region InitializeGroup
		internal override void InitializeGroup(CalendarGroupWrapper group, bool isNewlyRealized)
		{
			// update the filter state of the activity providers
			if (isNewlyRealized || this.GetFlag(InternalFlags.ActivityFilterChanged))
			{
				this.InitializeActivityFilter(group as DayViewCalendarGroupWrapper);
			}

			// if this is a new group or an old group and the current dates have changed...
			if (isNewlyRealized || this.GetFlag(InternalFlags.InitializeDayHeaderAreas))
			{
				this.InitializeHeaders(group as DayViewCalendarGroupWrapper);
			}

			// make sure to do our initialization first because the base may update the today state
			base.InitializeGroup(group, isNewlyRealized);
		}
		#endregion // InitializeGroup

		#region InitializeActivityProviders
		internal override void InitializeActivityProviders(CalendarGroupWrapper wrapper)
		{
			base.InitializeActivityProviders(wrapper);

			// if we already initialized the headers
			if (!this.GetFlag(InternalFlags.InitializeDayHeaderAreas))
			{
				var dayviewWrapper = wrapper as DayViewCalendarGroupWrapper;

				if (null != dayviewWrapper)
					dayviewWrapper.VerifyState();
			}
		}
		#endregion // InitializeActivityProviders

		#region IsInitialActivityNavigationPanel
		internal override bool IsInitialActivityNavigationPanel( ScheduleActivityPanel panel, DateRange timeslotRange )
		{
			bool isInitial = base.IsInitialActivityNavigationPanel(panel, timeslotRange);

			if (isInitial)
			{
				bool isAllDayRange = this.CalculateIsAllDaySelection(timeslotRange);
				bool isAllDayPanel = PresentationUtilities.GetTemplatedParent(panel) is DayViewDayHeaderArea;

				if (isAllDayRange != isAllDayPanel)
					isInitial = false;
			}

			return isInitial;
		}
		#endregion // IsInitialActivityNavigationPanel

		#region IsTimeslotRangeGroupBreak
		/// <summary>
		/// Used by the <see cref="ScheduleControlBase.TimeslotInfo"/> to determine if 2 visible dates are considered to be part of the same group of timeslots.
		/// </summary>
		/// <param name="previousDate">Previous date processed</param>
		/// <param name="nextDate">Next date to be processed</param>
		/// <returns></returns>
		internal override bool IsTimeslotRangeGroupBreak(DateTime previousDate, DateTime nextDate)
		{
			// all dates in separate timeslot areas
			return true;
		}
		#endregion // IsTimeslotRangeGroupBreak
		
		#region IsVerifyStateNeeded
		/// <summary>
		/// Returns a boolean indicating if the VerifyStateOverride is needed.
		/// </summary>
		internal override bool IsVerifyStateNeeded
		{
			get { return this.GetFlag(InternalFlags.ActivityFilterChanged) || base.IsVerifyStateNeeded; }
		}
		#endregion // IsVerifyStateNeeded

		#region NavigatePage
		internal override bool NavigatePage(bool up)
		{
			return this.PageTimeslots(up);
		}
		#endregion // NavigatePage

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
			#region MultiDayActivityAreaResizer

			var oldResizer = _multiDayAreaResizer;

			if ( oldResizer != null )
			{
				oldResizer.Host = null;
			}

			_multiDayAreaResizer = this.GetTemplateChild(PartMultiDayActivityAreaResizer) as ScheduleResizerBar;

			if ( null != _multiDayAreaResizer )
			{
				_multiDayAreaResizer.Host = new MultiDayAreaResizeHost(this);
			}

			#endregion //MultiDayActivityAreaResizer

			#region DayHeadersPanel

			ScheduleStackPanel oldPanel = _dayHeadersPanel;

			if (oldPanel != null)
			{
				oldPanel.ClearValue(ScheduleControlBase.ControlProperty);
				oldPanel.ClearValue(ScheduleStackPanel.OrientationProperty);
				oldPanel.ResizerBarHost = null; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				oldPanel.Items = null;
				this.CalendarGroupMergedScrollInfo.Remove(oldPanel.ScrollInfo);
			}

			_dayHeadersPanel = this.GetTemplateChild(PartDayHeadersPanel) as ScheduleStackPanel;

			if (null != _dayHeadersPanel)
			{
				_dayHeadersPanel.ResizerBarHost = this.CalendarGroupResizer; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_dayHeadersPanel.MinItemExtent = this.MinCalendarGroupExtentResolved; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_dayHeadersPanel.PreferredItemExtent = this.PreferredCalendarGroupExtent; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_dayHeadersPanel.SetValue(ScheduleControlBase.ControlProperty, this);
				_dayHeadersPanel.Orientation = this.CalendarGroupOrientation;
				_dayHeadersPanel.Items = _headers;
				this.CalendarGroupMergedScrollInfo.Add(_dayHeadersPanel.ScrollInfo);
			}

			this.CalendarGroupMergedScrollInfo.Dirty();

			#endregion //DayHeadersPanel

			_dayHeadersScrollBar = this.GetTemplateChild(PartDayHeadersScrollBar) as ScrollBar;

			_dayHeadersScrollbarMediator.ScrollBar = _dayHeadersScrollBar;

			if (!_scrollBarVisibilityCoordinator.Contains(_dayHeadersScrollbarMediator))
				this.HideMultiDayScrollBar();

			_scrollBarVisibilityCoordinator.StartAsyncVerification();

			// AS 2/24/12 TFS102945
			this.InitializeTouchScrollAreaHelper(_scrollTouchHelperAllDay,_dayHeadersPanel);
			this.InitializeTouchScrollAreaHelper(_scrollTouchHelperTimeslots,this.GroupsPanel);
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamDayView"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="XamDayViewAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamDayViewAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnDayHeaderClicked
		internal override void OnDayHeaderClicked(DayHeaderBase header)
		{
			var dayHeader = header as DayViewDayHeader;

			if (null != dayHeader)
			{
				this.OnDayHeaderClick(header.ResourceCalendar, header.DateTime);
			}
			else
				base.OnDayHeaderClicked(header);
		}
		#endregion // OnDayHeaderClicked

		#region OnIsAllDaySelectionChanged
		internal override void OnIsAllDaySelectionChanged()
		{
			base.OnIsAllDaySelectionChanged();

			this.AsyncReinitializeAllTimeslots();
		} 
		#endregion // OnIsAllDaySelectionChanged

		// AS 3/14/12 Touch Support
		#region OnIsTouchSupportEnabledChanged
		internal override void OnIsTouchSupportEnabledChanged(bool oldValue, bool newValue)
		{
			base.OnIsTouchSupportEnabledChanged(oldValue, newValue);
			_secondaryTimeZoneScrollTouchHelper.IsEnabled = _primaryTimeZoneScrollTouchHelper.IsEnabled = _scrollTouchHelperTimeslots.IsEnabled = _scrollTouchHelperAllDay.IsEnabled = newValue;
		}
		#endregion //OnIsTouchSupportEnabledChanged

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region OnCalendarGroupResizerChanged
		internal override void OnCalendarGroupResizerChanged()
		{
			base.OnCalendarGroupResizerChanged();

			if ( null != _dayHeadersPanel )
				_dayHeadersPanel.ResizerBarHost = this.CalendarGroupResizer;
		} 
		#endregion // OnCalendarGroupResizerChanged

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region OnMinCalendarGroupExtentResolvedChanged
		internal override void OnMinCalendarGroupExtentResolvedChanged()
		{
			base.OnMinCalendarGroupExtentResolvedChanged();

			if ( null != _dayHeadersPanel )
				_dayHeadersPanel.MinItemExtent = this.MinCalendarGroupExtentResolved;
		} 
		#endregion // OnMinCalendarGroupExtentResolvedChanged

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region OnPreferredCalendarGroupExtentChanged
		internal override void OnPreferredCalendarGroupExtentChanged( double oldValue, double newValue )
		{
			base.OnPreferredCalendarGroupExtentChanged(oldValue, newValue);

			if ( null != _dayHeadersPanel )
				_dayHeadersPanel.PreferredItemExtent = newValue;
		} 
		#endregion // OnPreferredCalendarGroupExtentChanged

		#region OnPreVerifyGroups
		internal override void OnPreVerifyGroups()
		{
			base.OnPreVerifyGroups();

			if (this.IsVerifyGroupsPending)
				this.SetFlag(InternalFlags.InitializeDayHeaderAreas, true);

			if (this.GetFlag(InternalFlags.ActivityFilterChanged))
			{
				if (this.MultiDayActivityAreaVisibility != Visibility.Collapsed)
					_timeslotActivityFilter = this.FilterOutMultiDayActivity;
				else
					_timeslotActivityFilter = null;
			}
		} 
		#endregion // OnPreVerifyGroups

		#region OnPostVerifyGroups
		internal override void OnPostVerifyGroups()
		{
			if (this.GetFlag(InternalFlags.InitializeDayHeaderAreas))
			{
				this.SetFlag(InternalFlags.InitializeDayHeaderAreas, false);

				var headers = from n in GroupWrappers 
							  select ((DayViewCalendarGroupWrapper)n).DayHeader;
				_headers.ReInitialize(headers.ToArray());
			}

			this.SetFlag(InternalFlags.ActivityFilterChanged, false);

			base.OnPostVerifyGroups();
		} 
		#endregion // OnPostVerifyGroups

		#region OnTimeslotPanelAttached
		internal override void OnTimeslotPanelAttached(TimeslotPanelBase panel)
		{
			ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

			if (ap != null)
			{
				DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);

				if (templatedParent is DayViewDayHeaderArea)
				{
					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					//ap.ClickToAddType = ClickToAddType.Event;
					ap.IsActivityScrollInfoEnabled = true;

					_dayHeaderActivityScrollInfo.Add(ap.ActivityScrollInfo);
					ap.ActivityScrollOffsetProvider = _dayHeaderActivityScrollInfo;
					ap.PreferredNonScrollingExtent = this.MultiDayActivityAreaHeight; // AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
				}
				else if (templatedParent is TimeslotArea)
				{
					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					//ap.ClickToAddType = ClickToAddType.NonIntersectingAppointment;
					ap.UseTimeslotIntervalAsMin = true;
					ap.MinEmptySpace = Math.Max(this.TimeslotGutterAreaWidth, 0d); // AS 5/8/12 TFS108279
				}

				// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
				this.VerifyClickToAddState(panel, this.IsClickToAddEnabled);
			}

			// AS 4/20/11 TFS73203
			// This isn't specific to this issue but I noticed that when the day view has a very 
			// small width that the all day items in the all day event area don't line up with the 
			// day headers. That's because we never set the PreferredTimeslotExtent on those and 
			// so it remains the default 20 which gets used as the min whereas the schedule stack 
			// panel arranging the day headers just uses 1 pixel.
			//
			if (PresentationUtilities.GetTemplatedParent(panel) is DayViewDayHeaderArea)
			{
				panel.PreferredTimeslotExtent = 1d;
			}

			base.OnTimeslotPanelAttached(panel);
		}
		#endregion // OnTimeslotPanelAttached

		#region OnTimeslotPanelDetached
		internal override void OnTimeslotPanelDetached(TimeslotPanelBase panel)
		{
			if (PresentationUtilities.GetTemplatedParent(panel) is DayViewDayHeaderArea)
			{
				ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

				if (null != ap)
				{
					_dayHeaderActivityScrollInfo.Remove(ap.ActivityScrollInfo);
					ap.ActivityScrollOffsetProvider = null;
				}
			}

			base.OnTimeslotPanelDetached(panel);
		}
		#endregion // OnTimeslotPanelDetached

		#region OnTimeslotScrollBarVisibilityChanged
		internal override void OnTimeslotScrollBarVisibilityChanged(ScrollBarVisibility oldValue, ScrollBarVisibility newValue)
		{
			base.OnTimeslotScrollBarVisibilityChanged(oldValue, newValue);
			_dayHeaderActivityScrollInfo.ScrollBarVisibility = newValue;
		} 
		#endregion // OnTimeslotScrollBarVisibilityChanged

		#region ProcessMouseWheel
		internal override void ProcessMouseWheel(ITimeRangeArea timeRangeArea, System.Windows.Input.MouseWheelEventArgs e)
		{
			if (timeRangeArea is DayViewDayHeaderArea)
			{
				var activityPanel = timeRangeArea.ActivityPanel as ScheduleActivityPanel;

				if (null != activityPanel && activityPanel.TimeslotOrientation == Orientation.Horizontal)
				{
					var si = activityPanel.ActivityScrollOffsetProvider;

					if (null != si && e.Delta != 0)
					{
						bool scrollDown = e.Delta < 0;
						si.Scroll(scrollDown ? 1 : -1);
						e.Handled = true;
						return;
					}
				}
			}

			base.ProcessMouseWheel(timeRangeArea, e);
		}
		#endregion // ProcessMouseWheel

		#region RefreshDisplay

		/// <summary>
		/// Releases the entire element tree so it can be recreated
		/// </summary>
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();

			ScheduleUtilities.RefreshDisplay(_dayHeadersPanel);
			_headers.Clear();

			this.SetFlag(InternalFlags.AllVerifyStateFlags, true);
		}

		#endregion //RefreshDisplay	

		#region ReinitializeTimeslots
		internal override void ReinitializeTimeslots(CalendarGroupWrapper calendarGroup)
		{
			base.ReinitializeTimeslots(calendarGroup);

			var dayGroupWrapper = calendarGroup as DayViewCalendarGroupWrapper;

			if (null != dayGroupWrapper)
			{
				var timeslots = dayGroupWrapper.DayHeader.AllDayTimeslots;

				if (null != timeslots)
					timeslots.Reinitialize();
			}
		}
		#endregion // ReinitializeTimeslots

		#region RestoreSelection
		internal override void RestoreSelection( DateRange pendingSelection )
		{
			DateRange selection = pendingSelection;

			TimeSpan dayOffset, dayDuration;
			this.GetLogicalDayInfo(out dayOffset, out dayDuration);

			if ( selection.End.Subtract(selection.Start) >= dayDuration )
			{
				// AS 11/7/11 TFS85890
				// Prefer the first date in view.
				//
				var inViewRange = this.TimeslotInfo.GetInViewRange(pendingSelection);
				DateTime start = inViewRange != null ? inViewRange.Value.Start : selection.Start;

				// if the activity spanned multiple logical days then select the 
				// first logical day for that activity
				DateRange dayRange = ScheduleUtilities.GetLogicalDayRange(start, dayOffset, dayDuration);
				this.SelectedTimeRange = dayRange;

				// AS 11/7/11 TFS85890
				// Added return - we should have been bailing out here.
				//
				return;
			}

			base.RestoreSelection(pendingSelection);
		}
		#endregion // RestoreSelection

		#region SetTemplateItemExtent
		internal override void SetTemplateItemExtent(Enum itemId, double value)
		{
			base.SetTemplateItemExtent(itemId, value);

			if (ScheduleTimeControlTemplateValue.TimeslotHeaderHeight.Equals(itemId) || ScheduleControlTemplateValue.SingleLineApptHeight.Equals(itemId))
			{
				double extent = ScheduleUtilities.Max(this.GetTemplateItemExtent(ScheduleTimeControlTemplateValue.TimeslotHeaderHeight), this.GetTemplateItemExtent(ScheduleControlTemplateValue.SingleLineApptHeight));
				this.TimeslotAreaTimeslotExtent = Math.Max(5, extent);
			}
		} 
		#endregion // SetTemplateItemExtent

		// AS 12/17/10 TFS62030
		#region ShouldIndentActivityEdge
		internal override void ShouldIndentActivityEdge( ActivityPresenter presenter, out bool leading, out bool trailing )
		{
			var panel = VisualTreeHelper.GetParent(presenter) as ScheduleActivityPanel;

			if ( panel == null || panel.IsVertical )
				leading = trailing = false;
			else
			{
				leading = presenter.IsNearInPanel;
				trailing = presenter.IsFarInPanel;
			}
		}
		#endregion // ShouldIndentActivityEdge

		// AS 3/6/12 TFS102945
		#region ShouldQueueTouchActions
		internal override bool ShouldQueueTouchActions
		{
			get { return _scrollTouchHelperTimeslots.ShouldDeferMouseActions || _scrollTouchHelperAllDay.ShouldDeferMouseActions; }
		}
		#endregion //ShouldQueueTouchActions

		#region ShouldReinitializeGroups
		internal override bool ShouldReinitializeGroups
		{
			get
			{
				return this.GetFlag(InternalFlags.InitializeDayHeaderAreas | InternalFlags.ActivityFilterChanged) || base.ShouldReinitializeGroups;
			}
		} 
		#endregion // ShouldReinitializeGroups

		#region SupportsTodayHighlight
		/// <summary>
		/// Returns a boolean indicating if the control is capable of showing a highlight around the current day.
		/// </summary>
		internal override bool SupportsTodayHighlight
		{
			get { return true; }
		}
		#endregion // SupportsTodayHighlight

		#region TimeslotAreaActivityFilter
		internal override Predicate<ActivityBase> TimeslotAreaActivityFilter
		{
			get { return _timeslotActivityFilter; }
		}
		#endregion // TimeslotAreaActivityFilter 

		#region TimeslotAreaTimeslotOrientation
		/// <summary>
		/// The orientation of timeslots within a <see cref="TimeslotArea"/>
		/// </summary>
		internal override Orientation TimeslotAreaTimeslotOrientation
		{
			get { return Orientation.Vertical; }
		}
		#endregion // TimeslotAreaTimeslotOrientation

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region VerifyClickToAddState
		internal override void VerifyClickToAddState(TimeslotPanelBase panel, bool isEnabled)
		{
			ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

			if (ap != null)
			{
				DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);

				if (templatedParent is DayViewDayHeaderArea)
				{
					ap.ClickToAddType = isEnabled ? ClickToAddType.Event : ClickToAddType.None;
				}
				else if (templatedParent is TimeslotArea)
				{
					ap.ClickToAddType = isEnabled ? ClickToAddType.NonIntersectingActivity : ClickToAddType.None;
				}
			}

			base.VerifyClickToAddState(panel, isEnabled);
		}
		#endregion //VerifyClickToAddState

		#region VerifyMergedScrollInfos
		internal override void VerifyMergedScrollInfos()
		{
			_dayHeaderActivityScrollInfo.VerifyState();

			base.VerifyMergedScrollInfos();
		}
		#endregion // VerifyMergedScrollInfos

		#region VerifyTimeslotPanelExtent
		internal override void VerifyTimeslotPanelExtent(TimeslotPanelBase panel)
		{
			base.VerifyTimeslotPanelExtent(panel);

			var activityPanel = panel as ScheduleActivityPanel;

			if (null != activityPanel)
			{
				// the activity orientation is always opposite to the timeslot orientation
				if (activityPanel.TimeslotOrientation == Orientation.Vertical)
				{
					activityPanel.MinEmptySpace = Math.Max(this.TimeslotGutterAreaWidth, 0); // AS 5/8/12 TFS108279
				}
			}
		}
		#endregion // VerifyTimeslotPanelExtent

		#region VerifyVisibleDatesOverride
		internal override void VerifyVisibleDatesOverride()
		{
			base.VerifyVisibleDatesOverride();

			this.SetFlag(InternalFlags.InitializeDayHeaderAreas, true);
		}
		#endregion // VerifyVisibleDatesOverride

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
		#region AllowMultiDayActivityAreaResizing

		/// <summary>
		/// Identifies the <see cref="AllowMultiDayActivityAreaResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMultiDayActivityAreaResizingProperty = DependencyPropertyUtilities.Register("AllowMultiDayActivityAreaResizing",
			typeof(bool), typeof(XamDayView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAllowMultiDayActivityAreaResizingChanged))
			);

		private static void OnAllowMultiDayActivityAreaResizingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamDayView instance = (XamDayView)d;

		}

		/// <summary>
		/// Returns or sets a boolean indicating if the end user is allowed to resize the <see cref="MultiDayActivityArea"/>
		/// </summary>
		/// <seealso cref="AllowMultiDayActivityAreaResizingProperty"/>
		public bool AllowMultiDayActivityAreaResizing
		{
			get
			{
				return (bool)this.GetValue(XamDayView.AllowMultiDayActivityAreaResizingProperty);
			}
			set
			{
				this.SetValue(XamDayView.AllowMultiDayActivityAreaResizingProperty, value);
			}
		}

		#endregion //AllowMultiDayActivityAreaResizing

		// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
		#region MultiDayActivityAreaHeight

		/// <summary>
		/// Identifies the <see cref="MultiDayActivityAreaHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MultiDayActivityAreaHeightProperty = DependencyPropertyUtilities.Register("MultiDayActivityAreaHeight",
			typeof(double), typeof(XamDayView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnMultiDayActivityAreaHeightChanged))
			);

		private static void OnMultiDayActivityAreaHeightChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamDayView instance = d as XamDayView;
			double newValue = (double)e.NewValue;

			foreach ( var panel in instance.TimeslotPanels )
			{
				ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

				if ( ap != null && PresentationUtilities.GetTemplatedParent(ap) is DayViewDayHeaderArea )
				{
					ap.PreferredNonScrollingExtent = newValue;
				}
			}
		}

		/// <summary>
		/// Returns or sets the height of the area containing the activities that are 24 hours or longer.
		/// </summary>
		/// <seealso cref="MultiDayActivityAreaHeightProperty"/>
		public double MultiDayActivityAreaHeight
		{
			get
			{
				return (double)this.GetValue(XamDayView.MultiDayActivityAreaHeightProperty);
			}
			set
			{
				this.SetValue(XamDayView.MultiDayActivityAreaHeightProperty, value);
			}
		}

		#endregion //MultiDayActivityAreaHeight

		#region MultiDayActivityAreaVisibility

		/// <summary>
		/// Identifies the <see cref="MultiDayActivityAreaVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MultiDayActivityAreaVisibilityProperty = DependencyPropertyUtilities.Register("MultiDayActivityAreaVisibility",
			typeof(Visibility), typeof(XamDayView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnMultiDayActivityAreaVisibilityChanged))
			);

		private static void OnMultiDayActivityAreaVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDayView ctrl = d as XamDayView;
			ctrl._cachedAllDayAreaVisibility = (Visibility)e.NewValue;
			ctrl.SetFlag(InternalFlags.ActivityFilterChanged, true);

			// if we are showing the area then we need to fix up the headers
			if (ctrl._cachedAllDayAreaVisibility != Visibility.Collapsed)
				ctrl.SetFlag(InternalFlags.InitializeDayHeaderAreas, true);

			ctrl.AddRemoveDayHeaderScrollBarMediator();
			ctrl.QueueAsyncVerification();
		}

		/// <summary>
		/// Returns or sets a value indicating whether the multi day activity area should be displayed beneath each day header.
		/// </summary>
		/// <seealso cref="MultiDayActivityAreaVisibilityProperty"/>
		public Visibility MultiDayActivityAreaVisibility
		{
			get
			{
				return _cachedAllDayAreaVisibility;
			}
			set
			{
				this.SetValue(XamDayView.MultiDayActivityAreaVisibilityProperty, value);
			}
		}

		#endregion //MultiDayActivityAreaVisibility

		// AS 5/8/12 TFS108279
		#region TimeslotGutterAreaWidth

		/// <summary>
		/// Identifies the <see cref="TimeslotGutterAreaWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeslotGutterAreaWidthProperty = DependencyPropertyUtilities.Register("TimeslotGutterAreaWidth",
			typeof(double), typeof(XamDayView),
			DependencyPropertyUtilities.CreateMetadata(10d, new PropertyChangedCallback(OnTimeslotGutterAreaWidthChanged))
			);

		private static void OnTimeslotGutterAreaWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDayView instance = (XamDayView)d;
			instance.InvalidateTimeslotPanelExtents();
		}

		/// <summary>
		/// Returns or sets the amount of horizontal space that is reserved within the vertically arranged timeslots.
		/// </summary>
		/// <seealso cref="TimeslotGutterAreaWidthProperty"/>
		public double TimeslotGutterAreaWidth
		{
			get
			{
				return (double)this.GetValue(XamDayView.TimeslotGutterAreaWidthProperty);
			}
			set
			{
				this.SetValue(XamDayView.TimeslotGutterAreaWidthProperty, value);
			}
		}

		#endregion //TimeslotGutterAreaWidth

		#endregion //Public Properties

		#region Internal Properties

		#region DayHeaderActivityScrollInfo
		/// <summary>
		/// Returns the scrollinfo used to control the scrolling of the activity in the all day activity area.
		/// </summary>
		internal ScrollInfo DayHeaderActivityScrollInfo
		{
			get { return _dayHeaderActivityScrollInfo; }
		} 
		#endregion // DayHeaderActivityScrollInfo

		#endregion // Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region CreateAllDayItem
		internal TimeslotBase CreateAllDayItem(DateTime start, DateTime end)
		{
			var dm = this.DataManagerResolved;
			TimeSpan dayOffset = dm != null ? dm.LogicalDayOffset : TimeSpan.Zero;
			return new MultiDayActivityAreaAdapter(start.Subtract(dayOffset), start, end);
		}
		#endregion // CreateAllDayItem

		#region FilterOutMultiDayActivity
		internal bool FilterOutMultiDayActivity(ActivityBase activity)
		{
			if (activity.Duration.Ticks >= this.TimeslotInfo.DayDuration.Ticks)
				return false;

			return true;
		}
		#endregion // FilterOutMultiDayActivity

		#region FilterOutNonMultiDayActivity
		internal bool FilterOutNonMultiDayActivity(ActivityBase activity)
		{
			return !FilterOutMultiDayActivity(activity);
		}
		#endregion // FilterOutNonMultiDayActivity

		#endregion // Internal Methods

		#region Private Methods

		#region AddRemoveDayHeaderScrollBarMediator
		private void AddRemoveDayHeaderScrollBarMediator()
		{
			var mediator = _dayHeadersScrollbarMediator;

			if (this.MultiDayActivityAreaVisibility == System.Windows.Visibility.Collapsed)
			{
				_scrollBarVisibilityCoordinator.Remove(mediator);
				mediator.ScrollBarVisibilityAction = this.HideMultiDayScrollBar;
				this.HideMultiDayScrollBar();
			}
			else if (!_scrollBarVisibilityCoordinator.Contains(mediator))
			{
				mediator.ScrollBarVisibilityAction = null;
				_scrollBarVisibilityCoordinator.Add(mediator);
			}
		}
		#endregion // AddRemoveDayHeaderScrollBarMediator

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag(InternalFlags flag)
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		// AS 2/24/12 TFS102945
		#region GetTouchFirstGroupWidth
		private double GetTouchFirstGroupWidth()
		{
			var groupElement = this.GroupWrappers.Select((g) => ((ISupportRecycling)g.GroupTimeslotArea).AttachedElement).FirstOrDefault((e) => e != null);

			// the first item width is the width of each calendar group since horizontal scrolling scrolls groups
			return groupElement == null ? 0 : ScheduleUtilities.Max(groupElement.ActualWidth, 0);
		}
		#endregion //GetTouchFirstGroupWidth

		// AS 2/24/12 TFS102945
		#region GetTouchFirstAllDayItemHeight
		private double GetTouchFirstAllDayItemHeight()
		{
			return this.SingleLineActivityHeight;
		}
		#endregion //GetTouchFirstAllDayItemHeight

		// AS 2/24/12 TFS102945
		#region GetTouchFirstTimeslotHeight
		private double GetTouchFirstTimeslotHeight()
		{
			var hPanel = this.TimeslotPanels.FirstOrDefault((p) => p is TimeslotPanel && p.TimeslotOrientation == Orientation.Vertical);

			// the width of the first item is the width of a single timeslot since horizonal scrolling involves scrolling timeslots
			return hPanel == null ? this.TimeslotAreaTimeslotExtent : hPanel.ActualTimeslotExtent;
		}
		#endregion //GetTouchFirstTimeslotHeight

		#region HideMultiDayScrollBar
		private void HideMultiDayScrollBar()
		{
			if (_dayHeadersScrollbarMediator.ScrollBar != null)
				_dayHeadersScrollbarMediator.ScrollBar.Visibility = Visibility.Collapsed;
		}
		#endregion // HideMultiDayScrollBar

		#region InitializeActivityFilter
		private void InitializeActivityFilter(DayViewCalendarGroupWrapper wrapper)
		{
			wrapper.DayHeader.AllDayAreaVisibility = _cachedAllDayAreaVisibility;

			foreach (TimeslotAreaAdapter area in wrapper.GroupTimeslotArea.TimeslotAreas)
			{
				area.ActivityProvider.ActivityFilter = _timeslotActivityFilter;
			}
		}
		#endregion // InitializeActivityFilter 

		#region InitializeHeaders
		private void InitializeHeaders(DayViewCalendarGroupWrapper wrapper)
		{
			Debug.Assert(null != wrapper, "Expected a DayViewCalendarGroupWrapper");

			wrapper.DayHeader.VerifyState(this);
		}
		#endregion // InitializeHeaders

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#endregion // Private Methods

		#endregion // Methods

		#region Events

		#region DayHeaderClick

		/// <summary>
		/// Used to invoke the <see cref="DayHeaderClick"/> event.
		/// </summary>
		/// <param name="calendar">ResourceCalendar whose week header was clicked</param>
		/// <param name="date">The start of the logical day whose header was clicked</param>
		/// <seealso cref="DayHeaderClick"/>
		/// <seealso cref="DayViewDayHeader"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		internal virtual void OnDayHeaderClick(ResourceCalendar calendar, DateTime date)
		{
			ScheduleHeaderClickEventArgs args = new ScheduleHeaderClickEventArgs(calendar, date);

			var handler = this.DayHeaderClick;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Occurs when the user clicks on the <see cref="MonthViewDayHeader"/>
		/// </summary>
		/// <seealso cref="MonthViewDayHeader"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		public event EventHandler<ScheduleHeaderClickEventArgs> DayHeaderClick;

		#endregion //DayHeaderClick

		#endregion //Events

		#region DayViewCalendarGroupWrapper class
		private class DayViewCalendarGroupWrapper : CalendarGroupWrapper
		{
			#region Member Variables

			private DayViewDayHeaderAreaAdapter _dayHeader;

			#endregion // Member Variables

			#region Constructor
			internal DayViewCalendarGroupWrapper(XamDayView control, CalendarGroupBase calendarGroup)
				: base(control, calendarGroup)
			{
				_dayHeader = new DayViewDayHeaderAreaAdapter(control, calendarGroup);
			}
			#endregion // Constructor

			#region Base class overrides

			#region OnCalendarHeaderAreaVisibilityChanged
			internal override void OnCalendarHeaderAreaVisibilityChanged(bool hasCalendarArea)
			{
				base.OnCalendarHeaderAreaVisibilityChanged(hasCalendarArea);

				_dayHeader.OnCalendarHeaderAreaVisibilityChanged(hasCalendarArea);
			} 
			#endregion // OnCalendarHeaderAreaVisibilityChanged

			#region OnSelectedCalendarChanged
			internal override void OnSelectedCalendarChanged(ResourceCalendar selectedCalendar)
			{
				base.OnSelectedCalendarChanged(selectedCalendar);

				_dayHeader.OnSelectedCalendarChanged(selectedCalendar);
			}
			#endregion // OnSelectedCalendarChanged

			#region OnTodayChanged
			internal override void OnTodayChanged(DateTime? today)
			{
				base.OnTodayChanged(today);

				_dayHeader.OnTodayChanged(today);
			}
			#endregion // OnTodayChanged

			#endregion // Base class overrides

			#region Properties

			#region DayHeader
			internal DayViewDayHeaderAreaAdapter DayHeader
			{
				get { return _dayHeader; }
			}
			#endregion // DayHeader 

			#endregion // Properties
		} 
		#endregion // DayViewCalendarGroupWrapper class

		#region DayViewTimeslotHeaderAdapter class
		private class DayViewTimeslotHeaderAdapter : TimeslotHeaderAdapter
		{
			#region Constructor
			internal DayViewTimeslotHeaderAdapter(DateTime start, DateTime end, TimeZoneToken token)
				: base(start, end, token)
			{
			} 
			#endregion // Constructor

			#region Base class overrides
			protected override TimeRangePresenterBase CreateInstanceOfRecyclingElement()
			{
				return new DayViewTimeslotHeader();
			}

			protected override Type RecyclingElementType
			{
				get
				{
					return typeof(DayViewTimeslotHeader);
				}
			} 
			#endregion // Base class overrides
		} 
		#endregion // DayViewTimeslotHeaderAdapter class

		#region AllDayItemInitializer class
		internal class AllDayItemInitializer
		{
			private ScheduleControlBase _control;
			private CalendarGroupBase _group;

			internal AllDayItemInitializer(ScheduleControlBase control, CalendarGroupBase group)
			{
				_control = control;
				_group = group;
			}

			public void Initialize(TimeslotBase timeslot)
			{
				var allDayItem = timeslot as MultiDayActivityAreaAdapter;

				if (null != allDayItem)
				{
					allDayItem.IsToday = allDayItem.LogicalDate == _control.CurrentLogicalDate;

					DateRange? selectedRange = _control.SelectedTimeRange;
					allDayItem.IsSelected = _group.SelectedCalendar == _control.ActiveCalendar &&
						selectedRange != null &&
						_control.IsAllDaySelection &&
						selectedRange.Value.IntersectsWithExclusive(new DateRange(allDayItem.Start, allDayItem.End));
				}
			}
		}
		#endregion // AllDayItemInitializer class

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : byte
		{
			// verifystate related flags
			InitializeDayHeaderAreas = 0x1,
			ActivityFilterChanged = 0x2,

			// general state

			// these are the flags that are considered dirty initially
			AllVerifyStateFlags = InitializeDayHeaderAreas | ActivityFilterChanged,
		}
		#endregion // InternalFlags enum

		// AS 11/11/10 NA 11.1 - MultiDayActivityAreaHeight
		#region MultiDayAreaResizeHost class
		private class MultiDayAreaResizeHost : IResizerBarHost
		{
			#region Member Variables

			private XamDayView _control; 

			#endregion // Member Variables

			#region Constructor
			internal MultiDayAreaResizeHost( XamDayView control )
			{
				_control = control;
			} 
			#endregion // Constructor

			#region IResizerBarHost Members

			Orientation IResizerBarHost.ResizerBarOrientation
			{
				get { return Orientation.Horizontal; }
			}

			bool IResizerBarHost.CanResize()
			{
				return _control.AllowMultiDayActivityAreaResizing && _control.MultiDayActivityAreaVisibility != Visibility.Collapsed;
			}

			void IResizerBarHost.SetExtent( double extent )
			{
				_control.MultiDayActivityAreaHeight = extent;
			}

			ResizeInfo IResizerBarHost.GetResizeInfo()
			{
				if ( ((IResizerBarHost)this).CanResize() )
				{
					double actualExtent = 0;
					double minEmptySpace = 0;

					foreach ( TimeslotPanelBase panel in _control.TimeslotPanels )
					{
						ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

						if ( null != ap && PresentationUtilities.GetTemplatedParent(ap) is DayViewDayHeaderArea )
						{
							actualExtent = Math.Max(actualExtent, ap.ActualHeight);
							minEmptySpace = Math.Max(minEmptySpace, ap.MinEmptySpace);
						}
					}

					double min = Math.Min(actualExtent, _control.SingleLineActivityHeight + minEmptySpace);
					return new ResizeInfo(_control, actualExtent, _control.MultiDayActivityAreaHeight, min);
				}

				return null;
			}

			#endregion
		} 
		#endregion // MultiDayAreaResizeHost class
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