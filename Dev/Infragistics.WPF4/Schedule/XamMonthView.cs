using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Controls.Primitives;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Infragistics.Collections;
using Infragistics.AutomationPeers;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Automation;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom control used to display activity relative to the weeks they occupy.
	/// </summary>
	[TemplatePart(Name = PartWeekScrollBar, Type = typeof(ScrollBar))]
	public class XamMonthView : ScheduleControlBase
	{
		#region Member Variables

		private const string PartWeekScrollBar = "WeekScrollBar";

		// template elements
		private ScrollBar _weekScrollBar;

		private InternalFlags _flags = InternalFlags.AllVerifyStateFlags;

		private ToolTip _scrollTip;
		private ScrollBarInfoMediator _weekScrollMediator;

		private DateTime[] _firstDayOfMonths;
		private WeakSet<MonthViewWeekHeader> _headers;

		// AS 11/2/10 TFS58663
		// Store the calculated min/max (particularly the max) since we need to adjust 
		// the max based on the maxdate and can't just rely on the calculated max of 
		// the scrollinfo as we did before. We'll default to the same values we used 
		// previously.
		//
		private double _minScrollPos = -52.1;
		private double _maxScrollPos = 53.1;

		// AS 2/24/12 TFS102945
		private ScrollInfoTouchHelper _scrollTouchHelper;

		#endregion // Member Variables

		#region Constructor
		static XamMonthView()
		{

			XamMonthView.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamMonthView), new FrameworkPropertyMetadata(typeof(XamMonthView)));

		}

		/// <summary>
		/// Initializes a new <see cref="XamMonthView"/>
		/// </summary>
		public XamMonthView()
		{




			var si = new WeekScrollInfo(this);
			si.Minimum = -52.1;
			si.Offset = 0;
			si.ScrollBarVisibility = this.WeekScrollBarVisibility;
			_weekScrollMediator = new ScrollBarInfoMediator(si);
			_headers = new WeakSet<MonthViewWeekHeader>();

			this.WeekHelper = new WeekModeHelper(this, true);
			this.VerifyWeekWorkDaySource();

			// AS 2/24/12 TFS102945
			_scrollTouchHelper = new ScrollInfoTouchHelper(
				Tuple.Create(this.GroupScrollbarMediator.ScrollInfo, ScrollType.Item, (Func<double>)this.GetTouchFirstItemWidth),
				Tuple.Create(this._weekScrollMediator.ScrollInfo, ScrollType.Item, (Func<double>)this.GetTouchFirstItemHeight)
				, this.GetScrollModeFromPoint
				);

			// because we set the week scrollbar's value to an integral value we should do the same 
			// for the touch scroll helper to avoid too many changes. otherwise we get a change for 
			// a non-integral value and then ultimately change that back to an integral value
			_scrollTouchHelper.VerticalOffsetCoerce = (d, s) => 
			{
				return CoreUtilities.GreaterThanOrClose(d, (int)s.Maximum) || CoreUtilities.LessThanOrClose(d, (int)s.Minimum)
					? d
					: (int)d;
			};
		}
		#endregion //Constructor

		#region Base class overrides

		#region BringIntoView
		internal override void BringIntoView(DateTime date)
		{
			Debug.Assert(date.Kind != DateTimeKind.Utc, "Expecting a local time");

			// we need to make sure we're not waiting for any pending changes
			this.VerifyState();

			DateTime logicalDate = this.ConvertToLogicalDate(date);

			// bring the date into view if needed
			if (!this.VisibleDates.Contains(logicalDate.Date))
			{
				
				this.UpdateVisibleDatesForSelectedRangeOverride(this.SelectedTimeRangeNormalized, new DateRange(this.ConvertFromLogicalDate(logicalDate.Date)));

				// since we changed the visible dates, we need to update the state
				// so the groups are updated
				this.VerifyState();
			}
		}
		#endregion // BringIntoView

		#region CalculateIsAllDaySelection
		internal override bool CalculateIsAllDaySelection(DateRange? selectedRange)
		{
			return selectedRange != null;
		} 
		#endregion // CalculateIsAllDaySelection

		// AS 3/6/12 TFS102945
		#region ClearTouchActionQueue
		internal override void ClearTouchActionQueue()
		{
			_scrollTouchHelper.ClearPendingActions();
		}
		#endregion //ClearTouchActionQueue

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
			CalendarGroupBase activityOwner)
		{
			TimeslotRange[] ranges = groupInfo.Ranges;

			if (ranges.Length == 0)
				return null;

			DateTime firstSlotStart = ranges[0].StartDate;
			DateTime lastSlotEnd = ranges[ranges.Length - 1].EndDate;

			var dateProvider = this.DateInfoProviderResolved;
			int year;
			int weekNumber = dateProvider.CalendarHelper.GetWeekNumberForDate(groupInfo.Start, null, this.WeekHelper.FirstDayOfWeekUsed, out year);

			var group = new MonthViewTimeslotAreaAdapter(this,
				// since the group represents a date, we should track the start and end date
				groupInfo.Start,
				groupInfo.End,
				firstSlotStart,
				lastSlotEnd,
				new TimeslotCollection(creatorFunc, modifyDateFunc, ranges, initializer),
				activityOwner,
				weekNumber);


			return group;
		}
		#endregion // CreateTimeslotAreaAdapter

		#region CreateTimeslotCalendarGroup
		internal override CalendarGroupTimeslotAreaAdapter CreateTimeslotCalendarGroup(CalendarGroupBase calendarGroup)
		{
			IList<TimeslotAreaAdapter> groups = this.TimeslotInfo.CreateTimeslotGroups(
				this.CreateTimeslot,
				new TimeslotInitializer(this, calendarGroup).Initialize,
				null,
				calendarGroup
				);

			return new MonthViewCalendarGroupTimeslotAreaAdapter(this, calendarGroup, groups);
		} 
		#endregion // CreateTimeslotCalendarGroup

		#region CreateTimeslotInfo
		internal override ScheduleTimeslotInfo CreateTimeslotInfo()
		{
			return new ScheduleTimeslotInfo(this, false, TimeSpan.FromDays(1));
		}
		#endregion // CreateTimeslotInfo

		#region DisplayActivityDialog
		internal override bool DisplayActivityDialog(TimeRangePresenterBase element)
		{
			var timeslot = element.Timeslot as MonthViewDayAdapter;

			if (null != timeslot)
			{
				ResourceCalendar calendar = element.DataContext as ResourceCalendar;
				Debug.Assert(null != calendar);

				var dm = this.DataManagerResolved;
				bool isAllDayActivity = null == dm || dm.IsTimeZoneNeutralActivityAllowed(ActivityType.Appointment, calendar);

				return this.DisplayActivityDialog(calendar, timeslot.Start, timeslot.End.Subtract(timeslot.Start), isAllDayActivity);
			}

			return false;
		}
		#endregion // DisplayActivityDialog

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
			vertical = _weekScrollMediator.ScrollInfo;
			horizontal = null;
		}

		#endregion // GetCalendarGroupAutomationScrollInfo

		#region GetDefaultSelectedRange
		/// <summary>
		/// Returns a local time range that represents the selection range for the specified logical date.
		/// </summary>
		/// <param name="logicalDate">The logical date whose range is to be returned.</param>
		/// <returns></returns>
		internal override DateRange? GetDefaultSelectedRange( DateTime logicalDate )
		{
			TimeSpan dayOffset, dayDuration;
			this.GetLogicalDayInfo(out dayOffset, out dayDuration);
			DateTime date = logicalDate.Add(dayOffset);
			return ScheduleUtilities.GetLogicalDayRange(date, dayOffset, dayDuration);
		}
		#endregion // GetDefaultSelectedRange

		#region GetTimeslotRangeInDirection
		internal override DateRange? GetTimeslotRangeInDirection( SpatialDirection direction, bool extendSelection, DateRange selectionRange )
		{
			selectionRange.Normalize();

			DateRange logicalRange = this.ConvertToLogicalDate(selectionRange);

			bool isNear = direction == SpatialDirection.Up || direction == SpatialDirection.Left;
			DateTime date = isNear || logicalRange.IsEmpty
				? logicalRange.Start 
				: ScheduleUtilities.GetNonInclusiveEnd(logicalRange.End);
			var dateProvider = this.DateInfoProviderResolved;

			date = date.Date;

			switch (direction)
			{
				case SpatialDirection.Up:
				{
					
					date = dateProvider.AddDays(date, -7) ?? dateProvider.MinSupportedDateTime;
					break;
				}
				case SpatialDirection.Down:
				{
					
					date = dateProvider.AddDays(date, 7) ?? dateProvider.MaxSupportedDateTime;
					break;
				}
				case SpatialDirection.Left:
				{
					
					date = dateProvider.AddDays(date, -1) ?? dateProvider.MinSupportedDateTime;
					break;
				}
				case SpatialDirection.Right:
				{
					
					date = dateProvider.AddDays(date, 1) ?? dateProvider.MaxSupportedDateTime;
					break;
				}
			}

			TimeSpan dayOffset, dayDuration;
			this.GetLogicalDayInfo(out dayOffset, out dayDuration);

			return ScheduleUtilities.GetLogicalDayRange(date, dayOffset, dayDuration);
		}
		#endregion // GetTimeslotRangeInDirection

		#region OnTimeslotAreaRecycled
		internal override void OnTimeslotAreaRecycled(TimeslotAreaAdapter areaAdapter)
		{
			base.OnTimeslotAreaRecycled(areaAdapter);

			var monthAdapter = areaAdapter as MonthViewTimeslotAreaAdapter;

			foreach (var item in monthAdapter.DayHeaders)
			{
				item.DayType = this.GetDayType(item.Date);
			}
		}
		#endregion // OnTimeslotAreaRecycled

		#region InitializeGroup
		internal override void InitializeGroup(CalendarGroupWrapper group, bool isNewlyRealized)
		{
			if (!isNewlyRealized)
			{
				if (this.GetFlag(InternalFlags.GroupDatesChanged))
					this.TimeslotInfo.ReinitializeTimeRanges(group.GroupTimeslotArea, group.CalendarGroup);

				if (this.GetFlag(InternalFlags.ReinitializeTimeslots))
					this.ReinitializeTimeslots(group);

				var timeslotArea = group.GroupTimeslotArea as MonthViewCalendarGroupTimeslotAreaAdapter;
				timeslotArea.DaysOfWeekSource = this.WeekHelper.DaysOfWeekList;

				if (this.GetFlag(InternalFlags.WeekHeaderWidth))
					timeslotArea.InitializeWeekHeaderWidth(_weekHeaderWidthObject);
			}

			base.InitializeGroup(group, isNewlyRealized);
		}
		#endregion // InitializeGroup

		#region InitializeTemplatePanel
		internal override void InitializeTemplatePanel(Canvas templatePanel)
		{
			base.InitializeTemplatePanel(templatePanel);

			MonthViewWeekHeader weekHeader = new MonthViewWeekHeader();
			DateTime date = new DateTime(2010,9,1);
			weekHeader.Start = date;
			weekHeader.End = date.AddDays(6);
			weekHeader.WeekNumber = 52;
			weekHeader.Tag = ScheduleControlBase.MeasureOnlyItemId;
			templatePanel.Children.Add(weekHeader);

			this.TrackTemplateItem(weekHeader, MonthViewTemplateValue.WeekHeaderWidth, null);
		}
		#endregion // InitializeTemplatePanel

		#region IsTimeslotRangeGroupBreak
		/// <summary>
		/// Used by the <see cref="ScheduleControlBase.TimeslotInfo"/> to determine if 2 visible dates are considered to be part of the same group of timeslots.
		/// </summary>
		/// <param name="previousDate">Previous date processed</param>
		/// <param name="nextDate">Next date to be processed</param>
		/// <returns></returns>
		internal override bool IsTimeslotRangeGroupBreak(DateTime previousDate, DateTime nextDate)
		{
			var calendarHelper = this.DateInfoProviderResolved.CalendarHelper;
			int firstYear, firstWeekNumber, secondYear, secondWeekNumber;

			firstWeekNumber = calendarHelper.GetWeekNumberForDate(previousDate, null, this.WeekHelper.FirstDayOfWeekUsed, out firstYear);
			secondWeekNumber = calendarHelper.GetWeekNumberForDate(nextDate, null, this.WeekHelper.FirstDayOfWeekUsed, out secondYear);

			return firstWeekNumber != secondWeekNumber || firstYear != secondYear;
		}
		#endregion // IsTimeslotRangeGroupBreak

		#region IsVerifyStateNeeded
		internal override bool IsVerifyStateNeeded
		{
			get
			{
				if (this.GetFlag(InternalFlags.AllVerifyStateFlags | InternalFlags.HasPendingScroll | InternalFlags.WeekHeaderWidth) || base.IsVerifyStateNeeded)
					return true;

				// if the working hours were invalidated then whether its dirty or not
				// would depend on the new resolved working hours so we need to verify 
				// the state
				this.TimeslotInfo.VerifyGroupRanges();

				return this.GetFlag(InternalFlags.AllVerifyStateFlags);
			}
		}
		#endregion // IsVerifyStateNeeded

		#region IsVerifyUIStateNeeded
		internal override bool IsVerifyUIStateNeeded
		{
			get
			{
				return base.IsVerifyUIStateNeeded || this.GetFlag(InternalFlags.ShowWeekNumbers);
			}
		} 
		#endregion // IsVerifyUIStateNeeded

		#region NavigateHomeEnd
		internal override bool NavigateHomeEnd(bool home)
		{
			if (!PresentationUtilities.HasNoOtherModifiers(ModifierKeys.Control | ModifierKeys.Shift))
				return false;

			return this.GoFirstLastDayOfWeek(home, true);
		}
		#endregion // NavigateHomeEnd

		#region NavigatePage
		internal override bool NavigatePage(bool up)
		{
			return this.WeekHelper.AdjustWeeks(!up, false, true);
		}
		#endregion // NavigatePage

		#region OnActiveCalendarChanged
		/// <summary>
		/// Used to invoke the <see cref="ScheduleControlBase.ActiveCalendarChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The old property value</param>
		/// <param name="newValue">The new property value</param>
		/// <seealso cref="ScheduleControlBase.ActiveCalendarChanged"/>
		/// <seealso cref="ScheduleControlBase.ActiveCalendar"/>
		protected override void OnActiveCalendarChanged(ResourceCalendar oldValue, ResourceCalendar newValue)
		{
			base.OnActiveCalendarChanged(oldValue, newValue);

			this.AsyncReinitializeAllTimeslots();
		}
		#endregion // OnActiveCalendarChanged

		#region OnActiveGroupChanged
		internal override void OnActiveGroupChanged(CalendarGroupBase oldGroup, CalendarGroupBase newGroup)
		{
			base.OnActiveGroupChanged(oldGroup, newGroup);

			this.AsyncReinitializeAllTimeslots();
		}
		#endregion // OnActiveGroupChanged

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			#region WeekScrollBar
			if (null != _weekScrollBar)
			{
				_weekScrollBar.Scroll -= new ScrollEventHandler(OnWeekScrollBarScroll);
				_weekScrollBar.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(OnWeekScrollBarValueChanged);
			}

			_weekScrollBar = this.GetTemplateChild(PartWeekScrollBar) as ScrollBar;

			if (null != _weekScrollBar)
			{
				this.OnWeekCountChanged();
				_weekScrollBar.Scroll += new ScrollEventHandler(OnWeekScrollBarScroll);
				_weekScrollBar.ValueChanged += new RoutedPropertyChangedEventHandler<double>(OnWeekScrollBarValueChanged);
			}

			_weekScrollMediator.ScrollBar = _weekScrollBar;
			#endregion // WeekScrollBar

			// AS 2/24/12 TFS102945
			this.InitializeTouchScrollAreaHelper(_scrollTouchHelper,this.GroupsPanel);
		}
		#endregion //OnApplyTemplate

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="XamMonthView"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="XamMonthViewAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new XamMonthViewAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnCurrentDateChanged
		internal override void OnCurrentDateChanged()
		{
			base.OnCurrentDateChanged();

			this.AsyncReinitializeAllTimeslots();
		}
		#endregion // OnCurrentDateChanged

		#region OnDayHeaderClicked
		internal override void OnDayHeaderClicked(DayHeaderBase header)
		{
			var dayHeader = header as MonthViewDayHeader;

			if (null != dayHeader)
			{
				this.OnDayHeaderClick(header.ResourceCalendar, header.DateTime);
			}
			else
				base.OnDayHeaderClicked(header);
		}
		#endregion // OnDayHeaderClicked

		// AS 3/14/12 Touch Support
		#region OnIsTouchSupportEnabledChanged
		internal override void OnIsTouchSupportEnabledChanged(bool oldValue, bool newValue)
		{
			base.OnIsTouchSupportEnabledChanged(oldValue, newValue);
			_scrollTouchHelper.IsEnabled = newValue;
		}
		#endregion //OnIsTouchSupportEnabledChanged

		// AS 11/2/10 TFS58663
		#region OnMinMaxDatesChanged
		internal override void OnMinMaxDatesChanged()
		{
			base.OnMinMaxDatesChanged();

			this.InitializeWeekScrollBarInfo(true);
		}
		#endregion // OnMinMaxDatesChanged

		#region OnPostVerifyGroups
		internal override void OnPostVerifyGroups()
		{
			base.OnPostVerifyGroups();

			this.SetFlag(InternalFlags.GroupDatesChanged, false);
			this.SetFlag(InternalFlags.ReinitializeTimeslots, false);
			this.SetFlag(InternalFlags.WeekHeaderWidth, false);
		} 
		#endregion // OnPostVerifyGroups

		#region OnSelectedTimeRangeChanged
		/// <summary>
		/// Invoked when the <see cref="ScheduleControlBase.SelectedTimeRange"/> has changed.
		/// </summary>
		/// <param name="oldValue">The previous selected range</param>
		/// <param name="newValue">The new selected range</param>
		protected override void OnSelectedTimeRangeChanged(DateRange? oldValue, DateRange? newValue)
		{
			base.OnSelectedTimeRangeChanged(oldValue, newValue);

			this.AsyncReinitializeAllTimeslots();
		}
		#endregion // OnSelectedTimeRangeChanged

		#region OnTimeslotGroupRangesInvalidated
		internal override void OnTimeslotGroupRangesInvalidated()
		{
			this.SetFlag(InternalFlags.GroupDatesChanged, true);
			this.QueueAsyncVerification();

			base.OnTimeslotGroupRangesInvalidated();
		}
		#endregion //OnTimeslotGroupRangesInvalidated

		#region OnTimeslotPanelAttached
		internal override void OnTimeslotPanelAttached(TimeslotPanelBase panel)
		{
			if (PresentationUtilities.GetTemplatedParent(panel) is TimeslotArea)
			{
				// don't impose a min size for the days. otherwise the days don't line up with the headers
				// when it gets too small
				panel.PreferredTimeslotExtent = 0;

				ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

				if (null != ap)
				{
					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					//ap.ClickToAddType = ClickToAddType.Event;
					ap.ClickToAddType = this.IsClickToAddEnabled ? ClickToAddType.Event : ClickToAddType.None;
					ap.AlignToTimeslot = true;
					ap.SortByTimeslotCountFirst = true;

					ap.IsActivityScrollInfoEnabled = true;
				}
			}

			base.OnTimeslotPanelAttached(panel);
		}
		#endregion // OnTimeslotPanelAttached

		#region OnVisibleDatesChanged
		internal override void OnVisibleDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
			base.OnVisibleDatesChanged(added, removed);

			_firstDayOfMonths = null;
			this.SetFlag(InternalFlags.GroupDatesChanged, true);
			this.SetFlag(InternalFlags.HasPendingScroll, false);
			this.TimeslotInfo.InvalidateGroupRanges();
		}
		#endregion //OnVisibleDatesChanged

		#region OnWeekCountChanged
		internal override void OnWeekCountChanged()
		{
			// AS 11/2/10 TFS58663
			// Moved initialization to a helper routine.
			//
			this.InitializeWeekScrollBarInfo(false);
		}
		#endregion // OnWeekCountChanged

		#region OnWeekFirstDateChanged
		internal override void OnWeekFirstDateChanged()
		{
			double newValue = this.GetWeekScrollBarValue(this.WeekHelper.FirstWeekStartDate);

			if (null != _weekScrollBar)
				_weekScrollBar.Value = newValue;
			else
				_weekScrollMediator.ScrollInfo.Offset = newValue;

			this.SetFlag(InternalFlags.HasPendingScroll, false);
		}
		#endregion // OnWeekFirstDateChanged

		#region ProcessMouseWheel
		internal override void ProcessMouseWheel(ITimeRangeArea timeRangeArea, MouseWheelEventArgs e)
		{
			if (timeRangeArea is MonthViewTimeslotArea)
			{
				var si = _weekScrollBar;

				if (null != si && e.Delta != 0)
				{
					bool scrollDown = e.Delta < 0;
					// AS 11/3/10 TFS58863
					// We should use the scroll info since that will call scrollweeks and now 
					// it will ensure it doesn't invoke it if we're at the min/max.
					//
					//this.ScrollWeeks(scrollDown ? ScrollEventType.SmallIncrement : ScrollEventType.SmallDecrement, si.Value + (scrollDown ? 1 : -1));
					this._weekScrollMediator.ScrollInfo.Scroll(scrollDown ? ScrollAmount.SmallIncrement : ScrollAmount.SmallDecrement);
					e.Handled = true;
					return;
				}
			}

			base.ProcessMouseWheel(timeRangeArea, e);
		}
		#endregion // ProcessMouseWheel

		#region RestoreSelection
		internal override void RestoreSelection( DateRange pendingSelection )
		{
			// AS 11/7/11 TFS85890
			// Use a timeslot within the in view timeslot first if possible.
			//
			//TimeSpan dayOffset, dayDuration;
			//this.GetLogicalDayInfo(out dayOffset, out dayDuration);
			//
			//// if the activity spanned multiple logical days then select the 
			//// first logical day for that activity
			//DateRange dayRange = ScheduleUtilities.GetLogicalDayRange(pendingSelection.Start, dayOffset, dayDuration);
			//this.SelectedTimeRange = dayRange;
			DateRange dayRange;
			var inViewRange = this.TimeslotInfo.GetInViewRange(pendingSelection);

			if (null != inViewRange)
				dayRange = this.TimeslotInfo.GetTimeslotRange(inViewRange.Value.Start).Value;
			else
			{
				TimeSpan dayOffset, dayDuration;
				this.GetLogicalDayInfo(out dayOffset, out dayDuration);

				// if the activity spanned multiple logical days then select the 
				// first logical day for that activity
				dayRange = ScheduleUtilities.GetLogicalDayRange(pendingSelection.Start, dayOffset, dayDuration);
			}

			this.SelectedTimeRange = dayRange;
		}
		#endregion // RestoreSelection

		#region SetTemplateItemExtent
		internal override void SetTemplateItemExtent(Enum itemId, double value)
		{
			base.SetTemplateItemExtent(itemId, value);

			if (MonthViewTemplateValue.WeekHeaderWidth.Equals(itemId))
			{
				this.WeekHeaderWidth = value + 2d;
			}
		}
		#endregion // SetTemplateItemExtent

		// AS 12/17/10 TFS62030
		#region ShouldIndentActivityEdge
		internal override void ShouldIndentActivityEdge( ActivityPresenter presenter, out bool leading, out bool trailing )
		{
			leading = presenter.IsNearInPanel;
			trailing = presenter.IsFarInPanel;
		}
		#endregion // ShouldIndentActivityEdge

		// AS 3/6/12 TFS102945
		#region ShouldQueueTouchActions
		internal override bool ShouldQueueTouchActions
		{
			get { return _scrollTouchHelper.ShouldDeferMouseActions; }
		}
		#endregion //ShouldQueueTouchActions

		#region ShouldReinitializeGroups
		internal override bool ShouldReinitializeGroups
		{
			get
			{
				return base.ShouldReinitializeGroups || this.GetFlag(InternalFlags.GroupDatesChanged | InternalFlags.ReinitializeTimeslots | InternalFlags.WeekHeaderWidth);
			}
		}
		#endregion // ShouldReinitializeGroups

		#region SupportsTodayHighlight
		internal override bool SupportsTodayHighlight
		{
			get
			{
				return true;
			}
		} 
		#endregion // SupportsTodayHighlight

		#region UpdateVisibleDatesForSelectedRangeOverride
		internal override void UpdateVisibleDatesForSelectedRangeOverride(DateRange? oldRange, DateRange newRange)
		{
			DateCollection visDates = this.VisibleDates;
			int visCount = visDates.Count;

			newRange.Normalize();

			if (visCount == 0)
			{
				// select the month containing the active date
				this.WeekHelper.Reset(newRange.Start);
			}
			else
			{
				if (this.VisibleDates.Contains(newRange.Start))
					this.WeekHelper.InitializeFromVisibleDates();
				else
				{
					// bring that week into view possibly releasing some weeks but keeping
					// the count the same
					this.WeekHelper.BringIntoView(newRange.Start);
				}
			}
		}
		#endregion // UpdateVisibleDatesForSelectedRangeOverride

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region VerifyClickToAddState
		internal override void VerifyClickToAddState(TimeslotPanelBase panel, bool isEnabled)
		{
			ScheduleActivityPanel ap = panel as ScheduleActivityPanel;

			if (ap != null)
			{
				ap.ClickToAddType = isEnabled ? ClickToAddType.Event : ClickToAddType.None;
			}

			base.VerifyClickToAddState(panel, isEnabled);
		}
		#endregion //VerifyClickToAddState

		#region VerifyStateOverride
		internal override void VerifyStateOverride()
		{
			if (this.GetFlag(InternalFlags.HasPendingScroll))
			{
				this.SetFlag(InternalFlags.HasPendingScroll, false);

				if (null != _weekScrollBar)
					this.ScrollWeeks(ScrollEventType.ThumbPosition, _weekScrollBar.Value);
			}

			base.VerifyStateOverride();
		} 
		#endregion // VerifyStateOverride

		#region VerifyUIState
		internal override void VerifyUIState()
		{
			base.VerifyUIState();

			if (this.GetFlag(InternalFlags.ShowWeekNumbers))
			{
				this.SetFlag(InternalFlags.ShowWeekNumbers, false);
				bool showWeekNumbers = this.ShowWeekNumbers;

				foreach (MonthViewWeekHeader header in _headers)
				{
					header.ShowWeekNumbers = showWeekNumbers;
				}
			}
		} 
		#endregion // VerifyUIState

		#region VerifyVisibleDatesOverride
		internal override void VerifyVisibleDatesOverride()
		{
			var visDates = this.VisibleDates;

			if (visDates.Count == 0)
			{
				// if there are no visible dates then get the weeks for the month of the new active date
				DateRange? selectedRange = this.GetSelectedTimeRangeForNavigation();

				if ( selectedRange == null )
				{
					selectedRange = this.GetDefaultSelectedRange(this.GetActivatableDate(DateTime.Today));
					this.SetInitialSelectedTimeRange(selectedRange);
					selectedRange = this.SelectedTimeRange;
				}

				if ( null != selectedRange )
					this.WeekHelper.Reset(selectedRange.Value.Start);
			}
			else
			{
				this.WeekHelper.InitializeFromVisibleDates();
			}
		}
		#endregion // VerifyVisibleDatesOverride

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region ShowWeekNumbers

		/// <summary>
		/// Identifies the <see cref="ShowWeekNumbers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWeekNumbersProperty = DependencyPropertyUtilities.Register("ShowWeekNumbers",
			typeof(bool), typeof(XamMonthView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowWeekNumbersChanged))
			);

		private static void OnShowWeekNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthView instance = (XamMonthView)d;
			instance.SetFlag(InternalFlags.ShowWeekNumbers, true);
			instance.QueueAsyncVerification();
		}

		/// <summary>
		/// Returns or sets a value indicating whether week numbers should be displayed in the week headers instead of the date range.
		/// </summary>
		/// <seealso cref="ShowWeekNumbersProperty"/>
		public bool ShowWeekNumbers
		{
			get
			{
				return (bool)this.GetValue(XamMonthView.ShowWeekNumbersProperty);
			}
			set
			{
				this.SetValue(XamMonthView.ShowWeekNumbersProperty, value);
			}
		}

		#endregion //ShowWeekNumbers

		#region ShowWorkingDaysOfWeekOnly

		/// <summary>
		/// Identifies the <see cref="ShowWorkingDaysOfWeekOnly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWorkingDaysOfWeekOnlyProperty = DependencyPropertyUtilities.Register("ShowWorkingDaysOfWeekOnly",
			typeof(bool), typeof(XamMonthView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowWorkingDaysOfWeekOnlyChanged))
			);

		private static void OnShowWorkingDaysOfWeekOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthView instance = (XamMonthView)d;
			instance.VerifyWeekWorkDaySource();
		}

		/// <summary>
		/// Returns or sets a value indicating whether non-working days of the week should be excluded from the display.
		/// </summary>
		/// <remarks>
		/// <p class="body">This property is used to hide an entire day of the week if the currently selected weeks do not contain any 
		/// dates </p>
		/// </remarks>
		/// <seealso cref="ShowWorkingDaysOfWeekOnlyProperty"/>
		public bool ShowWorkingDaysOfWeekOnly
		{
			get
			{
				return (bool)this.GetValue(XamMonthView.ShowWorkingDaysOfWeekOnlyProperty);
			}
			set
			{
				this.SetValue(XamMonthView.ShowWorkingDaysOfWeekOnlyProperty, value);
			}
		}

		#endregion //ShowWorkingDaysOfWeekOnly

		#region WeekScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="WeekScrollBarVisibility"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty WeekScrollBarVisibilityProperty = DependencyPropertyUtilities.Register("WeekScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(XamMonthView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.ScrollBarVisibilityVisibleBox, new PropertyChangedCallback(OnWeekScrollBarVisibilityChanged))
			);

		private static void OnWeekScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthView instance = (XamMonthView)d;
			instance.OnWeekScrollBarVisibilityChanged((ScrollBarVisibility)e.OldValue, (ScrollBarVisibility)e.NewValue);
		}

		internal virtual void OnWeekScrollBarVisibilityChanged(ScrollBarVisibility oldValue, ScrollBarVisibility newValue)
		{
			_weekScrollMediator.ScrollInfo.ScrollBarVisibility = newValue;
		}

		/// <summary>
		/// Returns or sets a value that indicates whether the week scrollbar is displayed.
		/// </summary>
		/// <seealso cref="WeekScrollBarVisibilityProperty"/>
		internal ScrollBarVisibility WeekScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(XamMonthView.WeekScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(XamMonthView.WeekScrollBarVisibilityProperty, value);
			}
		}

		#endregion //WeekScrollBarVisibility

		#region WorkingDaysSource

		/// <summary>
		/// Identifies the <see cref="WorkingDaysSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WorkingDaysSourceProperty = DependencyPropertyUtilities.Register("WorkingDaysSource",
			typeof(WorkingHoursSource), typeof(XamMonthView),
			DependencyPropertyUtilities.CreateMetadata(WorkingHoursSource.CurrentUser, new PropertyChangedCallback(OnWorkingDaysSourceChanged))
			);

		private static void OnWorkingDaysSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthView instance = (XamMonthView)d;
			instance.VerifyWeekWorkDaySource();
		}

		/// <summary>
		/// Returns or sets a value that indicates what is used to resolved the working days when the <see cref="ShowWorkingDaysOfWeekOnly"/> is enabled.
		/// </summary>
		/// <seealso cref="WorkingDaysSourceProperty"/>
		public WorkingHoursSource WorkingDaysSource
		{
			get
			{
				return (WorkingHoursSource)this.GetValue(XamMonthView.WorkingDaysSourceProperty);
			}
			set
			{
				this.SetValue(XamMonthView.WorkingDaysSourceProperty, value);
			}
		}

		#endregion //WorkingDaysSource

		#endregion //Public Properties

		#region Internal Properties

		#region WeekHeaderWidth

		private double _weekHeaderWidth = 0d;
		private object _weekHeaderWidthObject = 0d;

		internal double WeekHeaderWidth
		{
			get { return _weekHeaderWidth; }
			set
			{
				if (value != _weekHeaderWidth)
				{
					_weekHeaderWidth = ScheduleUtilities.Max(0d, value);
					_weekHeaderWidthObject = _weekHeaderWidth;

					this.SetFlag(InternalFlags.WeekHeaderWidth, true);
					this.QueueAsyncVerification();
				}
			}
		}

		internal object WeekHeaderWidthObject
		{
			get { return _weekHeaderWidthObject; }
		}

		#endregion // WeekHeaderWidth

		#endregion // Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AddWeekHeader
		internal void AddWeekHeader(MonthViewWeekHeader header)
		{
			_headers.Add(header);
		} 
		#endregion // AddWeekHeader

		#region GetDayType
		internal MonthDayType GetDayType(DateTime logicalDate)
		{
			if (!this.IsFirstDayForMonth(logicalDate))
				return MonthDayType.DayNumber;

			// we're going to follow outlook here where they only show the year number when
			// this is the first day in january
			if (this.DateInfoProviderResolved.CalendarHelper.Calendar.GetMonth(logicalDate) == 1)
				return MonthDayType.MonthDayYear;

			return MonthDayType.MonthDay;
		} 
		#endregion // GetDayType

		#region IsAlternateDay
		internal bool IsAlternateDay(DateTime anchor, DateTime date)
		{
			
			int modToday = anchor.Month % 2;
			int modMonth = date.Month % 2;

			return modToday != modMonth;
		}
		#endregion // IsAlternateDay

		#region IsFirstDayForMonth
		internal bool IsFirstDayForMonth(DateTime dateTime)
		{
			if (_firstDayOfMonths == null)
			{
				List<DateTime> dates = new List<DateTime>();
				var calendar = this.DateInfoProviderResolved.CalendarHelper.Calendar;
				var visDates = this.VisibleDates;
				visDates.Sort();
				int previousEra, previousYear, previousMonth;
				previousEra = previousYear = previousMonth = -1;

				for (int i = 0, count = visDates.Count; i < count; i++)
				{
					DateTime tempDate = visDates[i];

					int era = calendar.GetEra(tempDate);
					int year = calendar.GetYear(tempDate);
					int month = calendar.GetMonth(tempDate);

					if (era == previousEra && year == previousYear && month == previousMonth)
						continue;

					previousEra = era;
					previousYear = year;
					previousMonth = month;
					dates.Add(tempDate);
				}

				_firstDayOfMonths = dates.ToArray();
			}

			return ScheduleUtilities.BinarySearch(_firstDayOfMonths, dateTime, null, false) >= 0;
		}
		#endregion // IsFirstDayForMonth

		#endregion // Internal Methods		

		#region Private Methods

		#region AsyncReinitializeAllTimeslots
		private void AsyncReinitializeAllTimeslots()
		{
			this.SetFlag(InternalFlags.ReinitializeTimeslots, true);

			this.QueueAsyncVerification();
		}
		#endregion // AsyncReinitializeAllTimeslots

		// AS 11/2/10 TFS58663
		// Created helper routine from GetWeekScrollBarValue impl.
		//
		#region CalculateWeekScrollBarValue
		private double CalculateWeekScrollBarValue( DateTime date, double min, double max )
		{
			var calHelper = this.DateInfoProviderResolved.CalendarHelper;
			int zeroWeekOffset, newWeekOffset;
			var firstDayOfWeek = this.WeekHelper.FirstDayOfWeekUsed;
			double newValue;

			DateRange minMax = this.GetMinMaxRange();
			DateTime zeroWeekDate = ScheduleUtilities.Max(minMax.Start, ScheduleUtilities.Min(minMax.End, DateTime.Today));
			DateTime zeroWeekStart = calHelper.GetFirstDayOfWeekForDate(zeroWeekDate, firstDayOfWeek, out zeroWeekOffset);

			// AS 11/2/10 TFS58663
			// Since we call this with the min/max dates, this is more likely to result
			// in 1st chance exceptions so to try and minimize that, we'll do some 
			// gross diff checks first.
			//
			long tickDiff = date.Ticks - zeroWeekDate.Ticks;

			if ( tickDiff < min * TimeSpan.TicksPerDay * 7 )
				return min;
			else if ( tickDiff > max * TimeSpan.TicksPerDay * 7 )
				return max;

			DateTime newWeekStart = calHelper.GetFirstDayOfWeekForDate(date, firstDayOfWeek, out newWeekOffset);

			if ( newWeekStart == zeroWeekStart )
				newValue = 0;
			else if ( newWeekStart < zeroWeekStart )
				newValue = -(zeroWeekStart.Subtract(newWeekStart).TotalDays + newWeekOffset) / 7;
			else
				newValue = ((newWeekStart.Subtract(zeroWeekStart).TotalDays + zeroWeekOffset) / 7);

			newValue = Math.Max(min, Math.Min(max, newValue));

			return newValue;
		}
		#endregion // CalculateWeekScrollBarValue
	
		#region CreateTimeslot
		private TimeslotBase CreateTimeslot(DateTime start, DateTime end)
		{
			DateTime day = start.Subtract(this.TimeslotInfo.DayOffset);
			return new MonthViewDayAdapter(day, start, end);
		}
		#endregion // CreateTimeslot

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
		#region GetTouchFirstItemHeight
		private double GetTouchFirstItemHeight()
		{
			var timeslotArea = this.GroupWrappers.Select((g) => ((MonthViewCalendarGroupTimeslotAreaAdapter)g.GroupTimeslotArea).TimeslotAreas.FirstOrDefault())
				.Where((a) => ((ISupportRecycling)a).AttachedElement != null)
				.FirstOrDefault();

			// the first item height is the height of a week since the vertical scrolling scrolls weeks
			return timeslotArea == null ? 0 : ScheduleUtilities.Max(((ISupportRecycling)timeslotArea).AttachedElement.ActualHeight, 0);
		}
		#endregion //GetTouchFirstItemHeight

		// AS 2/24/12 TFS102945
		#region GetTouchFirstItemWidth
		private double GetTouchFirstItemWidth()
		{
			var groupElement = this.GroupWrappers.Select((g) => ((ISupportRecycling)g.GroupTimeslotArea).AttachedElement).FirstOrDefault((e) => e != null);

			// the first item width is the width of each calendar group since horizontal scrolling scrolls groups
			return groupElement == null ? 0 : ScheduleUtilities.Max(groupElement.ActualWidth, 0);
		}
		#endregion //GetTouchFirstItemWidth

		#region GetWeekScrollBarValue
		private double GetWeekScrollBarValue(DateTime? date)
		{
			if (null == date)
				return _weekScrollMediator.ScrollInfo.Offset;

			var si = _weekScrollMediator.ScrollInfo;

			// ensure its within the min/max range - note we're truncating this
			// because we have .1 extra on each end to allow continuing to scroll 
			// beyond the min/max
			return (int)CalculateWeekScrollBarValue(date.Value, si.Minimum, si.Maximum);
		}
		#endregion // GetWeekScrollBarValue

		// AS 11/2/10 TFS58663
		// Created helper routine from previous OnWeekCountChanged impl and also 
		// added logic to calculate the min/max based on the min/max dates on the 
		// datamanager.
		//
		#region InitializeWeekScrollBarInfo
		private void InitializeWeekScrollBarInfo(bool initializeMinMax)
		{
			var si = _weekScrollMediator.ScrollInfo;

			if ( initializeMinMax )
			{
				DateRange range = this.GetMinMaxRange();
				_minScrollPos = CalculateWeekScrollBarValue(range.Start, -52.1, 0);
				_maxScrollPos = Math.Max(0, Math.Min(53.1, CalculateWeekScrollBarValue(range.End, 0, 53.1) + 1));
				si.Minimum = _minScrollPos;
			}

			int weekCount = this.WeekHelper.WeekCount;
			si.Initialize(weekCount, _maxScrollPos - _minScrollPos, si.Offset);

			Debug.Assert(CoreUtilities.AreClose(si.Maximum + si.Viewport, _maxScrollPos), "Maximum and calculated max are out of sync?");
		}
		#endregion // InitializeWeekScrollBarInfo

		#region OnWeekScrollBarValueChanged
		private void OnWeekScrollBarValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			// do not reset the weeks in view based on the scrollbar position
			// if the position is just changing because we're synchronizing it
			// with the scroll info (as would happen when the onapplytemplate 
			// syncs the two)
			if ( !_weekScrollMediator.IsInitializingScrollBar )
			{
				this.SetFlag(InternalFlags.HasPendingScroll, true);
				this.QueueAsyncVerification();
			}
		} 
		#endregion // OnWeekScrollBarValueChanged

		#region OnWeekScrollBarScroll
		private void OnWeekScrollBarScroll(object sender, ScrollEventArgs e)
		{
			this.ScrollWeeks(e.ScrollEventType, e.NewValue);
		}
		#endregion // OnWeekScrollBarScroll

		#region ScrollWeeks
		private void ScrollWeeks(ScrollEventType scrollType, double newValue)
		{
			this.SetFlag(InternalFlags.HasPendingScroll, false);

			// AS 11/2/10 TFS58663
			// Constrain the starting point based on the min/max.
			//
			//DateTime date = DateTime.Today;
			DateRange minMax = this.GetMinMaxRange();
			DateTime date = ScheduleUtilities.Max(minMax.Start, ScheduleUtilities.Min(minMax.End, DateTime.Today));

			var dateProvider = this.DateInfoProviderResolved;
			bool resetValue = true;

			// AS 11/2/10 TFS58663
			// Since the scrollbar deals with doubles sometimes the value it gives during a drag is close
			// to an integral value but not quite. Since SL only has bankers rounding, we have to round 
			// away from zero ourselves.
			//
			var si = _weekScrollMediator.ScrollInfo;
			newValue = Math.Max( si.Minimum, Math.Min( si.Maximum, newValue + .5 * Math.Sign( newValue ) ) );

			int newIntValue = (int)newValue;

			switch (scrollType)
			{
				case ScrollEventType.First:
				case ScrollEventType.Last:
				{
					date = dateProvider.CalendarHelper.AddWeeks(date, newIntValue);
					break;
				}

				case ScrollEventType.LargeIncrement:
				case ScrollEventType.LargeDecrement:
				case ScrollEventType.SmallIncrement:
				case ScrollEventType.SmallDecrement:
				{
					bool isBefore = scrollType == ScrollEventType.LargeDecrement || scrollType == ScrollEventType.SmallDecrement;
					bool isPage = scrollType == ScrollEventType.LargeDecrement || scrollType == ScrollEventType.LargeIncrement;

					int weekCount = isPage ? this.WeekHelper.WeekCount : 1;
					var visDates = this.VisibleDates;

					if (visDates.Count > 0)
						date = visDates.First();

					date = dateProvider.CalendarHelper.AddWeeks(date, weekCount * (isBefore ? -1 : 1));

					// note since the user may have scroll beyond the end in one direction for a while, 
					// and then tried to page in the other direction we may need to fix up the value 
					// since the scrollbar would have interpretted the page down as increasing the 
					// value by the viewport but really the effective value was beyond the range value
					newIntValue = (int)this.GetWeekScrollBarValue(date);
					break;
				}

				case ScrollEventType.ThumbPosition:
				{
					date = dateProvider.CalendarHelper.AddWeeks(date, newIntValue);
					break;
				}
				case ScrollEventType.EndScroll:
				{
					if (_scrollTip != null)
					{
						_scrollTip.IsOpen = false;
						_scrollTip = null;
					}

					date = dateProvider.CalendarHelper.AddWeeks(date, newIntValue);
					break;
				}
				case ScrollEventType.ThumbTrack:
				{
					#region Create ScrollTip
					if (_scrollTip == null)
					{
						_scrollTip = new ScheduleToolTip();
						_scrollTip.PlacementTarget = _weekScrollBar;
						_scrollTip.Placement = PlacementMode.Left;
						CalendarBrushProvider.SetBrushProvider(_scrollTip, this.DefaultBrushProvider);

						Thumb scrollThumb = PresentationUtilities.GetScrollThumb(_weekScrollBar);

						if (null != scrollThumb)
						{
							try
							{
								var position = scrollThumb.TransformToVisual(_weekScrollBar).Transform(new Point());
								_scrollTip.VerticalOffset = position.Y;
							}
							catch
							{
							}
						}

						_scrollTip.IsOpen = true;
					}
					#endregion // Create ScrollTip

					DateTime newDate = dateProvider.CalendarHelper.AddWeeks(date, newIntValue);

					// get the week start for the date we show to the end user
					int offset;
					newDate = dateProvider.CalendarHelper.GetFirstDayOfWeekForDate(newDate, this.WeekHelper.FirstDayOfWeekUsed, out offset);

					_scrollTip.Content = dateProvider.FormatDate(newDate, DateTimeFormatType.ShortDate);

					resetValue = false;
					return;
				}
			}

			if (resetValue)
			{
				_weekScrollBar.Value = newIntValue;

				// clear the flag again since we could've manipulated the state
				this.SetFlag(InternalFlags.HasPendingScroll, false);
			}

			this.WeekHelper.SetFirstWeekDate(date);
		}
		#endregion // ScrollWeeks

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region VerifyWeekWorkDaySource
		private void VerifyWeekWorkDaySource()
		{
			this.WeekHelper.WorkDaySource = this.ShowWorkingDaysOfWeekOnly ? this.WorkingDaysSource : (WorkingHoursSource?)null;
		}
		#endregion // VerifyWeekWorkDaySource

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
		/// <seealso cref="MonthViewDayHeader"/>
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

		#region WeekHeaderClick

		/// <summary>
		/// Used to invoke the <see cref="WeekHeaderClick"/> event.
		/// </summary>
		/// <param name="calendar">ResourceCalendar whose week header was clicked</param>
		/// <param name="date">The start of the logical week</param>
		/// <seealso cref="WeekHeaderClick"/>
		/// <seealso cref="MonthViewWeekHeader"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		internal virtual void OnWeekHeaderClick(ResourceCalendar calendar, DateTime date)
		{
			ScheduleHeaderClickEventArgs args = new ScheduleHeaderClickEventArgs(calendar, date);

			var handler = this.WeekHeaderClick;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Occurs when the user clicks on the <see cref="MonthViewWeekHeader"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The <see cref="ScheduleHeaderClickEventArgs.Date"/> represents the logical day that is the start of the week whose header was clicked.</p>
		/// </remarks>
		/// <seealso cref="MonthViewWeekHeader"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		public event EventHandler<ScheduleHeaderClickEventArgs> WeekHeaderClick;

		#endregion //WeekHeaderClick

		#endregion //Events

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : byte
		{
			GroupDatesChanged = 0x1,
			ReinitializeTimeslots = 0x2,
			HasPendingScroll = 0x4,
			ShowWeekNumbers = 0x8,
			WeekHeaderWidth = 0x10,

			AllVerifyStateFlags = GroupDatesChanged | InternalFlags.ReinitializeTimeslots,
		}
		#endregion // InternalFlags enum

		#region TimeslotInitializer class
		private class TimeslotInitializer
		{
			#region Member Variables

			private CalendarGroupBase _group;
			private XamMonthView _control;

			#endregion // Member Variables

			#region Constructor
			internal TimeslotInitializer(XamMonthView control, CalendarGroupBase group)
			{
				_control = control;
				_group = group;
			}
			#endregion // Constructor

			#region Methods
			public void Initialize(TimeslotBase timeslot)
			{
				var day = timeslot as MonthViewDayAdapter;

				if (null != day)
				{
					day.IsToday = day.LogicalDate == _control.CurrentLogicalDate;

					DateRange? selectedRange = _control.SelectedTimeRange;
					day.IsSelected = _group.SelectedCalendar == _control.ActiveCalendar && selectedRange != null && selectedRange.Value.IntersectsWithExclusive(new DateRange(day.Start, day.End));

					// for the alternate day we want to consider the selected activities. otherwise 
					// the alternate coloration goes away when you select an activity
					if (selectedRange == null)
						selectedRange = _control.GetSelectedTimeRangeForNavigation();

					DateTime activeDate = selectedRange != null ? selectedRange.Value.Start : DateTime.Today;
					day.IsAlternate = _control.IsAlternateDay(activeDate, day.LogicalDate);
				}
			}
			#endregion // Methods
		} 
		#endregion // TimeslotInitializer class

		#region WeekScrollInfo class
		private class WeekScrollInfo : ScrollInfo
		{
			#region Member Variables

			private XamMonthView _control;

			#endregion // Member Variables

			#region Constructor
			internal WeekScrollInfo(XamMonthView control)
				: base()
			{
				_control = control;
			}
			#endregion // Constructor

			#region Base class overrides
			internal override void Scroll(ScrollAmount scrollAmount)
			{
				ScrollEventType scrollType;
				double offset = this.Offset; 

				switch (scrollAmount)
				{
					case ScrollAmount.LargeDecrement:
					{
						scrollType = ScrollEventType.LargeDecrement;
						offset -= this.Viewport;
						break;
					}
					case ScrollAmount.LargeIncrement:
					{
						scrollType = ScrollEventType.LargeIncrement;
						offset += this.Viewport;
						break;
					}
					case ScrollAmount.SmallDecrement:
					{
						scrollType = ScrollEventType.SmallDecrement;
						offset -= 1;
						break;
					}
					case ScrollAmount.SmallIncrement:
					{
						scrollType = ScrollEventType.SmallIncrement;
						offset += 1;
						break;
					}
					default:
					{
						return;
					}
				}

				// AS 11/3/10 TFS58863
				// We're going to calculate the new offset because with the min/max dates set, 
				// there is a min/max scroll position and therefore we want to ensure that we 
				// don't call the scrollweekswhen the adjusted value remains the same.
				//
				//// note we're assuming (since that's the way it works) that we don't need 
				//// to calculate the new value
				//_control.ScrollWeeks(scrollType, this.Offset);

				offset = Math.Max(Math.Min(offset, this.Maximum), this.Minimum);

				if ( CoreUtilities.AreClose(offset, this.Offset) )
					return;

				_control.ScrollWeeks(scrollType, offset);
			}

			#endregion // Base class overrides
		} 
		#endregion // WeekScrollInfo class

		#region MonthViewTemplateValue enum
		internal enum MonthViewTemplateValue
		{
			WeekHeaderWidth,
		}
		#endregion // MonthViewTemplateValue enum
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