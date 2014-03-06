using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Collections;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Layouts;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Base class for a control used to display appointments relative to the associated time slots.
	/// </summary>
	[TemplatePart(Name = PartTimeslotScrollBar, Type = typeof(ScrollBar))]
	[TemplatePart(Name = PartPrimaryTimeZone, Type = typeof(TimeslotHeaderArea))]
	[TemplatePart(Name = PartSecondaryTimeZone, Type = typeof(TimeslotHeaderArea))]
	public abstract class ScheduleTimeControlBase : ScheduleControlBase
	{
		#region Member Variables

		// part constants
		private const string PartTimeslotScrollBar = "TimeslotScrollBar";
		private const string PartPrimaryTimeZone = "PrimaryTimeZone";
		private const string PartSecondaryTimeZone = "SecondaryTimeZone";

		// template elements
		private ScrollBar _timeslotScrollBar;
		private TimeslotHeaderArea _primaryTimeZone;
		private TimeslotHeaderArea _secondaryTimeZone;

		// internal members
		private int _timeslotInitializerVersion;
		private InternalFlags _flags;
		private ScrollBarInfoMediator _timeslotScrollbarMediator;
		private MergedScrollInfo _timeslotScrollInfo;
		private WorkingHoursSource? _workDaySource;

		internal static readonly DateTime TemplateItemDate = new DateTime(2000, 12, 31, 12, 00, 00);

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ScheduleTimeControlBase"/>
		/// </summary>
		protected ScheduleTimeControlBase()
		{
			_timeslotScrollInfo = new MergedScrollInfo();
			_timeslotScrollInfo.DirtyAction = this.OnMergedScrollInfoChanged;
			_timeslotScrollInfo.ScrollBarVisibility = this.TimeslotScrollBarVisibility;
			_timeslotScrollbarMediator = new ScrollBarInfoMediator(_timeslotScrollInfo);

			this.VerifyTimeslotWorkHourSource();
			this.VerifyWeekWorkDaySource();
		}
		#endregion //Constructor

		#region Base class overrides

		#region BringIntoView
		internal override void BringIntoView( DateTime date )
		{
			if ( null == _timeslotScrollBar )
				return;

			Debug.Assert(date.Kind != DateTimeKind.Utc, "Expecting a local time");

			// we need to make sure we're not waiting for any pending changes
			this.VerifyState();

			DateTime logicalDate = this.ConvertToLogicalDate(date);

			// bring the date into view if needed
			if ( !this.VisibleDates.Contains(logicalDate.Date) )
			{
				
				this.UpdateVisibleDatesForSelectedRangeOverride(this.SelectedTimeRangeNormalized, new DateRange(this.ConvertFromLogicalDate(logicalDate.Date)));

				// since we changed the visible dates, we need to update the state
				// so the groups are updated
				this.VerifyState();
			}

			if ( this.GroupWrappers.Count == 0 )
				return;

			this.BringTimeslotIntoView(date);

			// since we sync the scrollbar with the panels asynchronously we need to force it now
			this.VerifyState();
		}
		#endregion // BringIntoView

		#region CreateHeaderTemplateItem
		internal override TimeslotBase CreateHeaderTemplateItem()
		{
			return this.CreateTimeslotHeader(TemplateItemDate, TemplateItemDate, null);
		}
		#endregion // CreateHeaderTemplateItem

		#region CreateTimeslotCalendarGroup
		internal override CalendarGroupTimeslotAreaAdapter CreateTimeslotCalendarGroup(CalendarGroupBase calendarGroup)
		{
			IList<TimeslotAreaAdapter> groups = this.TimeslotInfo.CreateTimeslotGroups(
				this.CreateTimeslot,
				new TimeslotInitializer(this, calendarGroup).Initialize,
				null,
				calendarGroup
				);

			return new CalendarGroupTimeslotAreaAdapter(this, calendarGroup, groups);
		}
		#endregion //CreateTimeslotCalendarGroup

		#region CreateTimeslotInfo
		internal override ScheduleTimeslotInfo CreateTimeslotInfo()
		{
			return new ScheduleTimeslotInfo(this, this.UseSingleTimeslotGroup, this.TimeslotInterval);
		}
		#endregion // CreateTimeslotInfo

		#region DisplayActivityDialog
		internal override bool DisplayActivityDialog(TimeRangePresenterBase element)
		{
			ResourceCalendar calendar = (element.DataContext as ResourceCalendar) ?? this.ActiveCalendar;
			DateRange? range = ScheduleUtilities.GetTimeslotRange(element, ScheduleUtilities.GetLogicalDayOffset(this));

			if (null != calendar && range != null)
			{
				DateRange actualRange = range.Value;
				return this.DisplayActivityDialog(calendar, actualRange.Start, actualRange.End.Subtract(actualRange.Start), false);
			}

			return false;
		}
		#endregion // DisplayActivityDialog

		#region GetDefaultSelectedRange
		/// <summary>
		/// Returns a local time range that represents the selection range for the specified logical date.
		/// </summary>
		/// <param name="logicalDate">The logical date whose range is to be returned.</param>
		/// <returns></returns>
		internal override DateRange? GetDefaultSelectedRange( DateTime logicalDate )
		{
			if ( this.CalendarGroupsResolved.Count > 0 )
			{
				return this.GetHomeEndTimeslot(logicalDate, true, true, this.ActiveCalendar);
			}

			return null;
		}
		#endregion // GetDefaultSelectedRange

		#region GetKeyCommand
		internal override ScheduleControlCommand? GetKeyCommand(System.Windows.Input.KeyEventArgs e)
		{
			Key key = PresentationUtilities.GetKey(e);
			switch (key)
			{
				case Key.Left:
				case Key.Right:
				{
					
					
					
					if (PresentationUtilities.IsModifierPressed(ModifierKeys.Control))
						return key == Key.Left ? ScheduleControlCommand.VisibleDatesShiftPrevious : ScheduleControlCommand.VisibleDatesShiftNext;
					break;
				}
			}

			return base.GetKeyCommand(e);
		}
		#endregion // GetKeyCommand

		#region GetTimeslotScrollInfo
		internal override ScrollInfo GetTimeslotScrollInfo(TimeslotPanelBase panel)
		{
			DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);

			if (templatedParent is TimeslotArea || templatedParent is TimeslotHeaderArea)
				return _timeslotScrollInfo;

			return base.GetTimeslotScrollInfo(panel);
		}
		#endregion // GetTimeslotScrollInfo

		#region InitializeTemplatePanel
		internal override void InitializeTemplatePanel(Canvas templatePanel)
		{
			base.InitializeTemplatePanel(templatePanel);

			// create a timeslot header so we can calculate the default width and/or height. in the 
			// case of dayview we'll use this to figure out the timeslot height
			ISupportRecycling headerItem = this.CreateHeaderTemplateItem();
			FrameworkElement headerElem = headerItem.CreateInstanceOfRecyclingElement();
			templatePanel.Children.Add(headerElem);
			headerItem.OnElementAttached(headerElem);

			headerElem.SetValue(TimeslotPresenterBase.IsFirstInMajorProperty, KnownBoxes.TrueBox);
			headerElem.SetValue(TimeslotPresenterBase.IsLastInMajorProperty, KnownBoxes.TrueBox);
			headerElem.SetValue(TimeslotPresenterBase.IsFirstInDayProperty, KnownBoxes.TrueBox);
			headerElem.SetValue(TimeslotPresenterBase.IsLastInDayProperty, KnownBoxes.TrueBox);

			headerElem.Tag = ScheduleControlBase.MeasureOnlyItemId;

			this.TrackTemplateItem(headerElem, ScheduleTimeControlTemplateValue.TimeslotHeaderWidth, ScheduleTimeControlTemplateValue.TimeslotHeaderHeight);
		} 
		#endregion // InitializeTemplatePanel

		#region IsVerifyStateNeeded
		internal override bool IsVerifyStateNeeded
		{
			get 
			{
				if (base.IsVerifyStateNeeded || this.GetFlag(InternalFlags.AllVerifyStateFlags))
					return true;

				// if the working hours were invalidated then whether its dirty or not
				// would depend on the new resolved working hours so we need to verify 
				// the state
				this.TimeslotInfo.VerifyGroupRanges();

				return this.GetFlag(InternalFlags.AllVerifyStateFlags); 
			}
		}
		#endregion // IsVerifyStateNeeded

		#region NavigateHomeEnd
		internal override bool NavigateHomeEnd(bool home)
		{
			DateRange? selectionAnchor = this.GetSelectionAnchor();

			if (null == selectionAnchor)
				return false;

			if (!PresentationUtilities.HasNoOtherModifiers(ModifierKeys.Control | ModifierKeys.Shift))
				return false;

			bool isCtrl = PresentationUtilities.IsModifierPressed(ModifierKeys.Control, ModifierKeys.Shift);
			bool extendSelection = PresentationUtilities.IsModifierPressed(ModifierKeys.Shift, ModifierKeys.Control);

			DateTime anchorStart = selectionAnchor.Value.Start;
			DateTime logicalStart = this.ConvertToLogicalDate(anchorStart).Date;
			DateRange? target = this.GetHomeEndTimeslot(logicalStart, home, !isCtrl, this.ActiveCalendar);

			if ( null == target )
				return false;

			return this.SetSelectedTimeRange(target.Value, extendSelection);
		}

		#endregion // NavigateHomeEnd

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

			_timeslotScrollBar = this.GetTemplateChild(PartTimeslotScrollBar) as ScrollBar;

			_primaryTimeZone = this.GetTemplateChild(PartPrimaryTimeZone) as TimeslotHeaderArea;
			_secondaryTimeZone = this.GetTemplateChild(PartSecondaryTimeZone) as TimeslotHeaderArea;

			if (null != _primaryTimeZone)
				_primaryTimeZone.SetValue(ScheduleControlBase.ControlProperty, this);

			if (null != _secondaryTimeZone)
			{
				_secondaryTimeZone.SetValue(ScheduleControlBase.ControlProperty, this);

				// we only show the current time in the primary timezone
				_secondaryTimeZone.CurrentTimeIndicatorVisibility = System.Windows.Visibility.Collapsed;
			}

			// AS 3/7/12 TFS102945
			this.InitializeTouchScrollAreaHelper(GetHeaderAreaTouchHelper(true), _primaryTimeZone);
			this.InitializeTouchScrollAreaHelper(GetHeaderAreaTouchHelper(false), _secondaryTimeZone);

			// if the headers are sync'd then update now
			if (!this.GetFlag(InternalFlags.ReinitializeTimeslotHeaders))
				this.InitializeTimeslotHeaders();

			_timeslotScrollbarMediator.ScrollBar = _timeslotScrollBar;

			this.VerifyCurrentTimeIndicatorVisibility();

			// AS 10/28/10 TFS55791/TFS50143
			// The state may have been verified before the template was applied and therefore 
			// before we had access to the scrollbar element.
			//
			if ( this.GetFlag(InternalFlags.InitialScrollInvoked) == false )
				this.QueueAsyncVerification();
		} 
		#endregion //OnApplyTemplate

		#region OnCurrentDateChanged
		internal override void OnCurrentDateChanged()
		{
			base.OnCurrentDateChanged();

			this.VerifyCurrentTimeIndicatorVisibility();
		}
		#endregion // OnCurrentDateChanged

		#region OnPreVerifyGroups
		/// <summary>
		/// Invoked when the control is about to verify the Groups
		/// </summary>
		internal override void OnPreVerifyGroups()
		{
			base.OnPreVerifyGroups();

			// if there still is no selection then let the control decide what to select
			// but it must be for something already in view (i.e. in the visible dates)
			if ( this.SelectedTimeRange == null &&
				this.SelectedActivities.Count == 0 &&
				this.VisibleDates.Count > 0 &&
				this.ActiveCalendar != null &&
				this.CalendarGroupsResolved.Count > 0 )
			{
				this.InitializeDefaultSelection();
			}


			if (this.GetFlag(InternalFlags.ReinitializeTimeslotHeaders))
				this.InitializeTimeslotHeaders();
		}
		#endregion // OnPreVerifyGroups

		#region OnPostVerifyGroups
		internal override void OnPostVerifyGroups()
		{
			// by this point we have either just created new groups (that wouldn't need to be verified) 
			// or we were called to reinitialize the groups in which case they have been fixed up
			this.SetFlag(InternalFlags.TimeslotDatesChanged, false);
			this.SetFlag(InternalFlags.ReinitializeTimeslots, false);

			base.OnPostVerifyGroups();
		} 
		#endregion // OnPostVerifyGroups

		#region OnPostVerifyState
		internal override void OnPostVerifyState()
		{
			base.OnPostVerifyState();

			// AS 10/28/10 TFS55791/TFS50143
			// Scroll the first working hour timeslot into view initially.
			//
			this.PerformInitialScrollIntoView();
		}
		#endregion // OnPostVerifyState

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

		#region OnSubObjectChanged
		/// <summary>
		/// Invoked when the Settings or a property on the settings of the ScheduleDataManager has changed.
		/// </summary>
		/// <param name="sender">The object whose property changed.</param>
		/// <param name="property">The property that changed.</param>
		/// <param name="extraInfo">Either Null or an instance of DependencyPropertyChangedEventArgs, NotifyCollectionChangedEventArgs or PropertyChangedEventArgs.</param>
		internal override void OnSubObjectChanged(object sender, string property, object extraInfo)
		{
			if (sender is CalendarGroup)
			{
				if (property == "SelectedCalendar")
				{
					
					this.AsyncReinitializeAllTimeslots();
				}
			}

			base.OnSubObjectChanged(sender, property, extraInfo);
		}
		#endregion //OnSubObjectChanged

		#region OnTimeslotGroupRangesInvalidated
		internal override void OnTimeslotGroupRangesInvalidated()
		{
			this.SetFlag(InternalFlags.TimeslotDatesChanged | InternalFlags.ReinitializeTimeslotHeaders, true);
			this.QueueAsyncVerification();

			base.OnTimeslotGroupRangesInvalidated();
		}
		#endregion //OnTimeslotGroupRangesInvalidated

		#region OnTimeslotPanelAttached
		internal override void OnTimeslotPanelAttached(TimeslotPanelBase panel)
		{
			DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);

			if (templatedParent is TimeslotArea || templatedParent is TimeslotHeaderArea)
			{
				panel.PreferredTimeslotExtent = this.TimeslotAreaTimeslotExtent;
				_timeslotScrollInfo.Add(panel.TimeslotScrollInfo);
			}

			base.OnTimeslotPanelAttached(panel);
		}
		#endregion // OnTimeslotPanelAttached

		#region OnTimeslotPanelDetached
		internal override void OnTimeslotPanelDetached(TimeslotPanelBase panel)
		{
			DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);

			if (templatedParent is TimeslotArea || templatedParent is TimeslotHeaderArea)
			{
				_timeslotScrollInfo.Remove(panel.TimeslotScrollInfo);
			}

			base.OnTimeslotPanelDetached(panel);
		}
		#endregion // OnTimeslotPanelDetached

		#region OnCalendarHeaderAreaVisibilityResolvedChanged
		internal override void OnCalendarHeaderAreaVisibilityResolvedChanged(Visibility oldValue, Visibility newValue)
		{
			base.OnCalendarHeaderAreaVisibilityResolvedChanged(oldValue, newValue);

			if (oldValue == Visibility.Collapsed || newValue == Visibility.Collapsed)
			{
				if (_primaryTimeZone != null)
					_primaryTimeZone.OnCalendarHeaderAreaVisibilityChanged();

				if (_secondaryTimeZone != null)
					_secondaryTimeZone.OnCalendarHeaderAreaVisibilityChanged();
			}
		} 
		#endregion // OnCalendarHeaderAreaVisibilityResolvedChanged

		#region OnVisibleDatesChanged
		internal override void OnVisibleDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
			// we need our own flag when the visible dates change because we need to reinitialize
			this.SetFlag(InternalFlags.TimeslotDatesChanged | InternalFlags.ReinitializeTimeslotHeaders, true);
			this.TimeslotInfo.InvalidateGroupRanges();

			base.OnVisibleDatesChanged(added, removed);
		}
		#endregion //OnVisibleDatesChanged

		#region OnWorkingHourRelatedPropertyChange
		internal override void OnWorkingHourRelatedPropertyChange()
		{
			base.OnWorkingHourRelatedPropertyChange();

			_timeslotInitializerVersion++;
			this.AsyncReinitializeAllTimeslots();
		}
		#endregion // OnWorkingHourRelatedPropertyChange

		#region InitializeGroup
		internal override void InitializeGroup(CalendarGroupWrapper group, bool isNewlyRealized)
		{
			if (!isNewlyRealized)
			{
				if (this.GetFlag(InternalFlags.TimeslotDatesChanged))
					this.TimeslotInfo.ReinitializeTimeRanges(group.GroupTimeslotArea, group.CalendarGroup);

				if (this.GetFlag(InternalFlags.ReinitializeTimeslots))
					this.ReinitializeTimeslots(group);
			}

			base.InitializeGroup(group, isNewlyRealized);
		} 
		#endregion // InitializeGroup

		#region RefreshDisplay

		/// <summary>
		/// Releases the entire element tree so it can be recreated
		/// </summary>
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();

			this.VerifyCurrentTimeIndicatorVisibility();

			if (null != _primaryTimeZone)
				_primaryTimeZone.RefreshDisplay();

			if (null != _secondaryTimeZone)
				_secondaryTimeZone.RefreshDisplay();

			this.SetFlag(InternalFlags.AllVerifyStateFlags, true);
		}

		#endregion //RefreshDisplay	

		#region RestoreSelection
		internal override void RestoreSelection( DateRange pendingSelection )
		{
			// AS 11/7/11 TFS85890
			// The start may be out of range so use the earliest timeslot range.
			//
			var inViewRange = this.TimeslotInfo.GetInViewRange(pendingSelection);

			if (null != inViewRange)
				pendingSelection.Start = inViewRange.Value.Start;

			this.TimeslotInfo.VerifyGroupRanges();

			int startIndex, endIndex;
			var startGroup = this.TimeslotInfo.GetTimeslotRangeGroup(pendingSelection.Start, out startIndex);
			var endGroup = this.TimeslotInfo.GetTimeslotRangeGroup(pendingSelection.IsEmpty ? pendingSelection.Start : ScheduleUtilities.GetNonInclusiveEnd(pendingSelection.End), out endIndex);

			Debug.Assert(startGroup != null, "Unable to find group for selection?");

			if ( startGroup == null )
				return;

			DateRange selection = ScheduleUtilities.CalculateDateRange(startGroup.Ranges, startIndex);

			selection.End = endGroup != null && endGroup == startGroup
				? ScheduleUtilities.CalculateDateRange(endGroup.Ranges, endIndex).End
				: ScheduleUtilities.CalculateDateRange(startGroup.Ranges, ScheduleUtilities.GetTimeslotCount(startGroup.Ranges) - 1).End;

			this.SelectedTimeRange = selection;
		}
		#endregion // RestoreSelection

		#region SetSelectedTimeRange
		internal override bool SetSelectedTimeRange( DateRange timeslotRange, bool extendSelection, bool preventBringIntoView = false )
		{
			bool result = base.SetSelectedTimeRange(timeslotRange, extendSelection, preventBringIntoView);

			if ( result 
				&& !preventBringIntoView
				&& false == this.CalculateIsAllDaySelection(this.SelectedTimeRange) 
				&& _timeslotScrollBar != null 
				&& false == this.EditHelper.IsInTimeslotSelectionDrag
				)
			{
				this.BringTimeslotIntoView(timeslotRange.Start);
			}

			return result;
		} 
		#endregion // SetSelectedTimeRange

		#region ShouldReinitializeGroups
		/// <summary>
		/// Used to determine if the InitializeGroup should be re-executed when the <see cref="ScheduleControlBase.IsVerifyGroupsPending"/> is false
		/// </summary>
		internal override bool ShouldReinitializeGroups
		{
			// if the groups aren't dirty we may still have some clean up to do like 
			// updating the working hour state of the timeslots or updating the 
			// timeslotgroups within each calendar group if the visible dates changed
			get { return base.ShouldReinitializeGroups || this.GetFlag(InternalFlags.TimeslotDatesChanged | InternalFlags.ReinitializeTimeslots); }
		}
		#endregion // ShouldReinitializeGroups

		#region UpdateVisibleDatesForSelectedRangeOverride
		internal override void UpdateVisibleDatesForSelectedRangeOverride(DateRange? oldRange, DateRange newRange)
		{
			// selected ranges are expressed in local time but the visible dates is expressed in logical
			// dates which may be different if the logical day offset is specified.

			// calculate what would be considered the active date based on the specified range
			DateTime activeDate = newRange.Start <= newRange.End ? newRange.Start : ScheduleUtilities.GetNonInclusiveEnd(newRange.Start);
			activeDate = this.ConvertToLogicalDate(activeDate).Date;

			// update for week/workweek mode based on the anchor timeslot
			if (this.WeekHelper != null)
			{
				this.WeekHelper.Reset(activeDate);
				return;
			}

			DateRange newLogicalRange = this.ConvertToLogicalDate(newRange);
			newLogicalRange.Normalize();

			DateTime start = newLogicalRange.Start.Date;
			DateTime end = newLogicalRange.IsEmpty ? start : ScheduleUtilities.GetNonInclusiveEnd(newLogicalRange.End).Date;

			DateCollection visDates = this.VisibleDates;
			int visCount = visDates.Count;

			// if the visible dates haven't been initialized then use the dates of the range
			if (visCount == 0)
			{
				visDates.AddRange(start, end, true);
			}
			else if (visCount == 1)
			{
				// just replace it with whatever is the date containing the selection anchor
				visDates[0] = activeDate;
			}
			else
			{
				if (visDates.ContainsDate(activeDate))
					return;

				visDates.Sort();

				DateTime firstVisible = visDates[0];
				DateTime lastVisible = visDates[visCount - 1];

				int firstVisDayOffset = (int)firstVisible.Subtract(activeDate).TotalDays;
				int lastVisDayOffset = (int)activeDate.Subtract(lastVisible).TotalDays;

				// if we're outside the range then we may need to 
				// remove additional items so we have items adjacent 
				// to the new active date
				List<DateTime> newVisibleDates = new List<DateTime>();

				// if we're within the already active dates then we'll just
				// remove the last/first depending on direction being navigated
				if (activeDate > firstVisible && activeDate < lastVisible)
				{
					bool removeFirst;
					newVisibleDates.AddRange(visDates);

					// if there is no old date then just remove the one we are further from
					if (oldRange == null)
					{
						removeFirst = firstVisDayOffset > lastVisDayOffset;
					}
					else
					{
						DateTime oldActive = oldRange.Value.Start.Date;

						// if scrolling forward then remove the first date - otherwise 
						// remove the last
						removeFirst = oldActive < activeDate;
					}

					if (removeFirst)
						newVisibleDates.RemoveAt(0);
					else
						newVisibleDates.RemoveAt(visCount - 1);

					int newIndex = newVisibleDates.BinarySearch(activeDate);
					newVisibleDates.Insert(~newIndex, activeDate);
				}
				else
				{
					bool isBefore = activeDate < firstVisible;

					// we're near the first/last...
					if (isBefore)
					{
						firstVisDayOffset = Math.Min(visCount, firstVisDayOffset);

						for (int i = 0; i < firstVisDayOffset; i++)
							newVisibleDates.Add(activeDate.AddDays(i));

						// if we're close to the end then keep some of the beginning
						if (firstVisDayOffset < visCount)
							newVisibleDates.AddRange(visDates.Take(visCount - firstVisDayOffset));
					}
					else
					{
						lastVisDayOffset = Math.Min(visCount, lastVisDayOffset);

						// if we're close to the end then keep some of the end
						if (lastVisDayOffset < visCount)
							newVisibleDates.AddRange(visDates.Skip(lastVisDayOffset));

						for (int i = lastVisDayOffset - 1; i >= 0; i--)
							newVisibleDates.Add(activeDate.AddDays(-i));
					}
				}

				// make sure the dates are sorted
				visDates.Reinitialize(newVisibleDates);
			}
		}
		#endregion // UpdateVisibleDatesForSelectedRangeOverride

		#region VerifyMergedScrollInfos
		internal override void VerifyMergedScrollInfos()
		{
			_timeslotScrollInfo.VerifyState();

			base.VerifyMergedScrollInfos();
		}
		#endregion // VerifyMergedScrollInfos

		#region VerifyTimeslotPanelExtent
		internal override void VerifyTimeslotPanelExtent(TimeslotPanelBase panel)
		{
			base.VerifyTimeslotPanelExtent(panel);

			DependencyObject templatedParent = PresentationUtilities.GetTemplatedParent(panel);
			
			if (templatedParent is TimeslotArea || templatedParent is TimeslotHeaderArea)
				panel.PreferredTimeslotExtent = this.TimeslotAreaTimeslotExtent;
		} 
		#endregion // VerifyTimeslotPanelExtent

		#region VerifyVisibleDatesOverride
		internal override void VerifyVisibleDatesOverride()
		{
			// AS 10/26/10 TFS57198
			//DateRange selected = this.GetSelectedTimeRangeForNavigation() ?? new DateRange(this.GetActivatableDate(DateTime.Today));
			DateRange? selected = this.GetSelectedTimeRangeForNavigation();
			DateTime logicalDayStart;

			if ( selected == null )
			{
				logicalDayStart = this.GetActivatableDate(DateTime.Today);
			}
			else
			{
				// AS 10/26/10 TFS57198
				// Selection is an actual date but not necessarily a logical one. Also it contains 
				// a time portion. We should be using a logical date and excluding the time.
				//
				// AS 10/28/10 TFS57198
				// We need the date portion of the logical day range.
				//
				logicalDayStart = this.ConvertToLogicalDate(selected.Value.Start).Date;
			}

			var weekHelper = this.WeekHelper;
			int visDateCount = this.VisibleDates.Count;

			if ( null != weekHelper )
			{
				DateTime resetDate = visDateCount == 0
					? logicalDayStart
					: this.VisibleDates[0];
				weekHelper.Reset(resetDate);
			}
			else
			{
				if ( visDateCount == 0 )
				{
					this.VisibleDates.Add(logicalDayStart);
				}
			}
		}
		#endregion // VerifyVisibleDatesOverride

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region CurrentTimeIndicatorVisibility

		/// <summary>
		/// Identifies the <see cref="CurrentTimeIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentTimeIndicatorVisibilityProperty = DependencyPropertyUtilities.Register("CurrentTimeIndicatorVisibility",
			typeof(Visibility), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnCurrentTimeIndicatorVisibilityChanged))
			);

		private static void OnCurrentTimeIndicatorVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase instance = d as ScheduleTimeControlBase;
			instance.VerifyCurrentTimeIndicatorVisibility();
		}

		/// <summary>
		/// Returns or sets a value indicating whether an indicator identifying the current time is displayed within the time slot area.
		/// </summary>
		/// <seealso cref="CurrentTimeIndicatorVisibilityProperty"/>
		public Visibility CurrentTimeIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ScheduleTimeControlBase.CurrentTimeIndicatorVisibilityProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.CurrentTimeIndicatorVisibilityProperty, value);
			}
		}

		#endregion //CurrentTimeIndicatorVisibility

		#region PrimaryTimeZoneLabel

		/// <summary>
		/// Identifies the <see cref="PrimaryTimeZoneLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PrimaryTimeZoneLabelProperty = DependencyPropertyUtilities.Register("PrimaryTimeZoneLabel",
			typeof(string), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the label that is displayed by the time slot area for the current time zone.
		/// </summary>
		/// <seealso cref="PrimaryTimeZoneLabelProperty"/>
		public string PrimaryTimeZoneLabel
		{
			get
			{
				return (string)this.GetValue(ScheduleTimeControlBase.PrimaryTimeZoneLabelProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.PrimaryTimeZoneLabelProperty, value);
			}
		}

		#endregion //PrimaryTimeZoneLabel

		#region SecondaryTimeZoneLabel

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneLabelProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneLabel",
			typeof(string), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the label that is displayed by the time slot area for the alternate time zone.
		/// </summary>
		/// <seealso cref="SecondaryTimeZoneLabelProperty"/>
		/// <seealso cref="SecondaryTimeZoneId"/>
		/// <seealso cref="SecondaryTimeZoneVisibility"/>
		public string SecondaryTimeZoneLabel
		{
			get
			{
				return (string)this.GetValue(ScheduleTimeControlBase.SecondaryTimeZoneLabelProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.SecondaryTimeZoneLabelProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneLabel

		#region SecondaryTimeZoneId

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneIdProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneId",
			typeof(string), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSecondaryTimeZoneIdChanged))
			);

		private static void OnSecondaryTimeZoneIdChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase ctrl = d as ScheduleTimeControlBase;
			ctrl.SetFlag(InternalFlags.ReinitializeTimeslotHeaders, true);
			ctrl.QueueAsyncVerification();
		}

		/// <summary>
		/// Returns or sets the id for the additional time zone.
		/// </summary>
		/// <seealso cref="SecondaryTimeZoneIdProperty"/>
		/// <seealso cref="SecondaryTimeZoneLabel"/>
		/// <seealso cref="SecondaryTimeZoneVisibility"/>
		/// <seealso cref="ScheduleDataConnectorBase.TimeZoneInfoProvider"/>
		/// <seealso cref="ScheduleDataConnectorBase.TimeZoneInfoProviderResolved"/>
		public string SecondaryTimeZoneId
		{
			get
			{
				return (string)this.GetValue(ScheduleTimeControlBase.SecondaryTimeZoneIdProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.SecondaryTimeZoneIdProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneId

		#region SecondaryTimeZoneVisibility

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneVisibilityProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneVisibility",
			typeof(Visibility), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityCollapsedBox)
			);

		/// <summary>
		/// Returns or sets a value indicating whether the time slot area for the additional time zone should be displayed.
		/// </summary>
		/// <seealso cref="SecondaryTimeZoneVisibilityProperty"/>
		/// <seealso cref="SecondaryTimeZoneLabel"/>
		/// <seealso cref="SecondaryTimeZoneId"/>
		public Visibility SecondaryTimeZoneVisibility
		{
			get
			{
				return (Visibility)this.GetValue(ScheduleTimeControlBase.SecondaryTimeZoneVisibilityProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.SecondaryTimeZoneVisibilityProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneVisibility

		#region ShowWorkingHoursOnly

		/// <summary>
		/// Identifies the <see cref="ShowWorkingHoursOnly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWorkingHoursOnlyProperty = DependencyPropertyUtilities.Register("ShowWorkingHoursOnly",
			typeof(Boolean), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnShowWorkingHoursOnlyChanged))
			);

		private static void OnShowWorkingHoursOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase instance = (ScheduleTimeControlBase)d;
			instance.VerifyTimeslotWorkHourSource();
		}

		/// <summary>
		/// Returns or sets a boolean indicating whether the control should filter out non-working hour timeslots.
		/// </summary>
		/// <seealso cref="ShowWorkingHoursOnlyProperty"/>
		public Boolean ShowWorkingHoursOnly
		{
			get
			{
				return (Boolean)this.GetValue(ScheduleTimeControlBase.ShowWorkingHoursOnlyProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.ShowWorkingHoursOnlyProperty, value);
			}
		}

		#endregion //ShowWorkingHoursOnly

		#region TimeslotInterval

		/// <summary>
		/// Identifies the <see cref="TimeslotInterval"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeslotIntervalProperty = DependencyPropertyUtilities.Register("TimeslotInterval",
			typeof(TimeSpan), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(TimeSpan.FromMinutes(15d), new PropertyChangedCallback(OnTimeslotIntervalChanged))
			);

		private static void OnTimeslotIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase ctrl = d as ScheduleTimeControlBase;

			ctrl.OnTimeslotIntervalChanged((TimeSpan)e.OldValue, (TimeSpan)e.NewValue);
		}

		/// <summary>
		/// Invoked when the <see cref="TimeslotInterval"/> has changed.
		/// </summary>
		/// <param name="oldInterval">The old interval</param>
		/// <param name="newInterval">The new interval</param>
		protected virtual void OnTimeslotIntervalChanged(TimeSpan oldInterval, TimeSpan newInterval)
		{
			this.TimeslotInfo.TimeslotInterval = newInterval;
		}

		/// <summary>
		/// Returns or sets a value indicating the duration of each time slot.
		/// </summary>
		/// <seealso cref="TimeslotIntervalProperty"/>
		public TimeSpan TimeslotInterval
		{
			get
			{
				return (TimeSpan)this.GetValue(ScheduleTimeControlBase.TimeslotIntervalProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.TimeslotIntervalProperty, value);
			}
		}

		#endregion //TimeslotInterval

		#region TimeslotScrollBarVisibility

		/// <summary>
		/// Identifies the <see cref="TimeslotScrollBarVisibility"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty TimeslotScrollBarVisibilityProperty = DependencyPropertyUtilities.Register("TimeslotScrollBarVisibility",
			typeof(ScrollBarVisibility), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.ScrollBarVisibilityVisibleBox, new PropertyChangedCallback(OnTimeslotScrollBarVisibilityChanged))
			);

		private static void OnTimeslotScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase instance = (ScheduleTimeControlBase)d;
			instance.OnTimeslotScrollBarVisibilityChanged((ScrollBarVisibility)e.OldValue, (ScrollBarVisibility)e.NewValue);
		}

		internal virtual void OnTimeslotScrollBarVisibilityChanged(ScrollBarVisibility oldValue, ScrollBarVisibility newValue)
		{
			_timeslotScrollInfo.ScrollBarVisibility = newValue;
		}

		/// <summary>
		/// Returns or sets a value that indicates whether the timeslot scrollbar is displayed.
		/// </summary>
		/// <seealso cref="TimeslotScrollBarVisibilityProperty"/>
		internal ScrollBarVisibility TimeslotScrollBarVisibility
		{
			get
			{
				return (ScrollBarVisibility)this.GetValue(ScheduleTimeControlBase.TimeslotScrollBarVisibilityProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.TimeslotScrollBarVisibilityProperty, value);
			}
		}

		#endregion //TimeslotScrollBarVisibility

		#region WeekDisplayMode

		/// <summary>
		/// Identifies the <see cref="WeekDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekDisplayModeProperty = DependencyPropertyUtilities.Register("WeekDisplayMode",
			typeof(WeekDisplayMode), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(WeekDisplayMode.None, new PropertyChangedCallback(OnWeekDisplayModeChanged))
			);

		private static void OnWeekDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase instance = (ScheduleTimeControlBase)d;
			instance.VerifyWeekWorkDaySource();
		}

		/// <summary>
		/// Returns or sets a value used to determine which dates are displayed within the control.
		/// </summary>
		/// <seealso cref="WeekDisplayModeProperty"/>
		public WeekDisplayMode WeekDisplayMode
		{
			get
			{
				return (WeekDisplayMode)this.GetValue(ScheduleTimeControlBase.WeekDisplayModeProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.WeekDisplayModeProperty, value);
			}
		}

		#endregion //WeekDisplayMode

		#region WorkingHoursSource

		/// <summary>
		/// Identifies the <see cref="WorkingHoursSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WorkingHoursSourceProperty = DependencyPropertyUtilities.Register("WorkingHoursSource",
			typeof(WorkingHoursSource), typeof(ScheduleTimeControlBase),
			DependencyPropertyUtilities.CreateMetadata(WorkingHoursSource.CurrentUser, new PropertyChangedCallback(OnWorkingHoursSourceChanged))
			);

		private static void OnWorkingHoursSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleTimeControlBase instance = (ScheduleTimeControlBase)d;
			instance.OnWorkingHoursSourceChanged((WorkingHoursSource)e.OldValue, (WorkingHoursSource)e.NewValue);
		}

		internal virtual void OnWorkingHoursSourceChanged(WorkingHoursSource oldValue, WorkingHoursSource newValue)
		{
			this.VerifyTimeslotWorkHourSource();
			this.VerifyWeekWorkDaySource();
		}

		/// <summary>
		/// Returns or sets a value indicating what object is considered when calculating the working hours.
		/// </summary>
		/// <remarks>
		/// <p class="note">This property does not affect what is used to calculate whether a timeslot is considered a 
		/// working hour or not. That calculation is based on the <see cref="ResourceCalendar.OwningResource"/> for the <see cref="CalendarGroupBase.SelectedCalendar"/>  of the containing 
		/// <see cref="CalendarGroupBase"/>.</p>
		/// </remarks>
		/// <seealso cref="WorkingHoursSourceProperty"/>
		/// <seealso cref="WeekDisplayMode"/>
		/// <seealso cref="ShowWorkingHoursOnly"/>
		public WorkingHoursSource WorkingHoursSource
		{
			get
			{
				return (WorkingHoursSource)this.GetValue(ScheduleTimeControlBase.WorkingHoursSourceProperty);
			}
			set
			{
				this.SetValue(ScheduleTimeControlBase.WorkingHoursSourceProperty, value);
			}
		}

		#endregion //WorkingHoursSource

		#endregion //Public Properties

		#region Internal Properties

		#region CurrentTimeIndicatorVisibilityResolved
		internal Visibility CurrentTimeIndicatorVisibilityResolved
		{
			get
			{
				if (this.CurrentLogicalDate == null || ! this.TimeZoneInfoProviderResolved.IsValid)
					return Visibility.Collapsed;

				return this.CurrentTimeIndicatorVisibility;
			}
		}
		#endregion // CurrentTimeIndicatorVisibilityResolved

		#region TimeslotMergedScrollInfo
		internal MergedScrollInfo TimeslotMergedScrollInfo
		{
			get { return _timeslotScrollInfo; }
		} 
		#endregion // TimeslotMergedScrollInfo

		#region TimeslotScrollbarMediator
		internal ScrollBarInfoMediator TimeslotScrollbarMediator
		{
			get { return _timeslotScrollbarMediator; }
		}
		#endregion // TimeslotScrollbarMediator

		#region UseSingleTimeslotGroup
		internal virtual bool UseSingleTimeslotGroup
		{
			get { return false; }
		}
		#endregion //UseSingleTimeslotGroup

		// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
		// Added separation of WorkingHourSource and WorkDaySource for time controls. This is only 
		// used by the XamOutlookCalendarView. For the standalone controls we default to using the 
		// WorkingHoursSource for both as we had previously.
		//
		#region WorkDaySource
		internal WorkingHoursSource? WorkDaySource
		{
			get { return _workDaySource; }
			set
			{
				if ( value != _workDaySource )
				{
					_workDaySource = value;
					this.VerifyWeekWorkDaySource();
				}
			}
		}

		internal WorkingHoursSource WorkDaySourceResolved
		{
			get { return _workDaySource ?? this.WorkingHoursSource; }
		} 
		#endregion // WorkDaySource

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region AsyncReinitializeAllTimeslots
		internal void AsyncReinitializeAllTimeslots()
		{
			this.SetFlag(InternalFlags.ReinitializeTimeslots, true);

			// bump a version number that indicates that something's working hours has changed so we recache them
			_timeslotInitializerVersion++;

			this.QueueAsyncVerification();
		}
		#endregion // AsyncReinitializeAllTimeslots

		#region BringTimeslotIntoView
		/// <summary>
		/// Attempts to bring the specified timeslot into view and returns true if it is in view.
		/// </summary>
		/// <param name="datetime">The timeslot date/time to bring into view</param>
		/// <param name="timeslotOffsetPercent">The percentage (0-1) that the specified time should be offset within the view or null to just ensure it is in view.</param>
		/// <param name="useNearestTime">True to find the timeslot closest to the specified time</param>
		/// <returns></returns>
		// AS 6/17/11 TFS78894
		//internal bool BringTimeslotIntoView( DateTime datetime, bool forceToTop = false )
		internal bool BringTimeslotIntoView( DateTime datetime, double? timeslotOffsetPercent = null, bool useNearestTime = false )
		{
			if ( null == _timeslotScrollBar )
				return false;

			int timeslotIndex;
			var rangeGroup = this.TimeslotInfo.GetTimeslotRangeGroup(datetime, out timeslotIndex);

			// AS 6/17/11 TFS78894
			// Added an option so we would use the closest time even if the specified date/time is not in the range.
			//
			if (null == rangeGroup && useNearestTime)
			{
				DateRange? nearestSlot = this.TimeslotInfo.GetNextOrClosestTimeslot(datetime);

				if (null != nearestSlot)
				{
					datetime = nearestSlot.Value.Start;
					rangeGroup = this.TimeslotInfo.GetTimeslotRangeGroup(datetime, out timeslotIndex);
				}
			}

			if ( null == rangeGroup )
				return false;

			var scrollInfo = this.TimeslotMergedScrollInfo;
			var scrollBar = _timeslotScrollBar;

			// make sure we've processed any pending changes/synchronization
			scrollInfo.VerifyState();

			// AS 10/28/10 TFS55791/TFS50143
			// Once something causes a scroll we should ensure we don't process our initial scroll
			//
			this.SetFlag(InternalFlags.InitialScrollInvoked, true);

			double offset = scrollBar.Value;

			// get the indexes fully in view
			double usableExtent = Math.Max(0, _timeslotScrollInfo.Viewport - 1);
			int first = (int)Math.Ceiling(offset);
			int last = (int)(offset + usableExtent);

			// AS 6/17/11 TFS78894
			//if ( timeslotIndex < first || forceToTop )
			if ( timeslotIndex < first )
				offset = timeslotIndex;
			else if ( timeslotIndex > last )
				offset = timeslotIndex - (int)Math.Ceiling(usableExtent);
			// AS 6/17/11 TFS78894
			//else
			else if (timeslotOffsetPercent == null)
				return true;

			// AS 6/17/11 TFS78894
			// Instead of just having an option to make the timeslot the first we'll allow a percentage 
			// to be passed along and try to put the time closest to the specified percentage.
			//
			if (timeslotOffsetPercent != null && usableExtent > 0)
			{
				var timeslotRange = this.TimeslotInfo.GetTimeslotRange(datetime);

				if (timeslotRange != null)
				{
					double desiredTimeslotIndex = timeslotIndex - (Math.Floor(_timeslotScrollInfo.Viewport) * Math.Max(Math.Min(timeslotOffsetPercent.Value, 1), 0)) + 1;
					double intraSlotPercent = 1 - datetime.Subtract(timeslotRange.Value.Start).TotalSeconds / timeslotRange.Value.End.Subtract(timeslotRange.Value.Start).TotalSeconds;

					double proposed = Math.Floor(desiredTimeslotIndex) + intraSlotPercent;

					if (Math.Abs(proposed - desiredTimeslotIndex) >= 0.5)
					{
						if (proposed > desiredTimeslotIndex)
							desiredTimeslotIndex--;
						else
							desiredTimeslotIndex++;
					}

					offset = Math.Min(Math.Max(Math.Floor(desiredTimeslotIndex), timeslotIndex - (int)Math.Ceiling(usableExtent)), timeslotIndex);
				}
			}

			offset = Math.Max(scrollBar.Minimum, Math.Min(scrollBar.Maximum, offset));
			scrollBar.Value = offset;
			return true;
		}
		#endregion // BringTimeslotIntoView

		#region CreateTimeslot
		internal virtual Timeslot CreateTimeslot(DateTime start, DateTime end)
		{
			return new Timeslot(start, end);
		}
		#endregion // CreateTimeslot

		#region CreateTimeslotHeader

		internal abstract TimeslotHeaderAdapter CreateTimeslotHeader(DateTime start, DateTime end, TimeZoneToken token);

		#endregion // CreateTimeslotHeader

		#region GetNextPreviousLogicalDate
		/// <summary>
		/// Returns a logical date adjacent to the specified date considering the current visible dates.
		/// </summary>
		/// <param name="logicalDate">The logical date to start with</param>
		/// <param name="next">True to return the next date; otherwise false to return the previous date</param>
		/// <param name="considerVisibleDates">True to evaluate the visible dates when navigating in case there are discontiguous days selected</param>
		/// <param name="limitToVisibleDates">True to not look for a date outside the visible dates</param>
		/// <returns></returns>
		internal DateTime? GetNextPreviousLogicalDate(DateTime logicalDate, bool next, bool considerVisibleDates, bool limitToVisibleDates = false)
		{
			if (considerVisibleDates)
			{
				var visDates = this.VisibleDates;
				int visDateIndex = this.VisibleDates.BinarySearch(logicalDate);

				if (visDateIndex >= 0)
				{
					if (!next && visDateIndex > 0)
						return visDates[visDateIndex - 1];
					else if (next && visDateIndex < visDates.Count - 1)
						return visDates[visDateIndex + 1];
				}
			}

			if ( limitToVisibleDates )
				return null;

			if (this.WeekDisplayMode == Schedules.WeekDisplayMode.WorkWeek)
			{
				DateTime? firstLast = this.WeekHelper.GetFirstLastDayInWeek(!next, logicalDate);

				if (firstLast != null && firstLast != logicalDate)
					return firstLast;

				var otherWeek = logicalDate.AddDays(next ? 7 : -7);
				var adjacentWeek = this.WeekHelper.CalculateWeeks(otherWeek);

				if (adjacentWeek == null || adjacentWeek.Count == 0)
					return null;

				if (next)
					return adjacentWeek[0];

				return adjacentWeek[adjacentWeek.Count - 1];
			}

			return logicalDate.AddDays(next ? 1 : -1);
		}
		#endregion // GetNextPreviousLogicalDate

		// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
		#region GetWeekModeFromDates
		/// <summary>
		/// Helper method to evaluate the specified dates and indicate the week mode that the control would have to be in to display those dates.
		/// </summary>
		/// <param name="sortedDates">The dates to evaluate</param>
		/// <returns></returns>
		internal WeekDisplayMode GetWeekModeFromDates( IList<DateTime> sortedDates )
		{
			if ( sortedDates != null && sortedDates.Count >= 1 && sortedDates.Count <= 7 )
			{
				IList<DateTime> newVisibleDates;
				var currentDisplayMode = this.WeekDisplayMode;
				var weekHelper = this.WeekHelper ?? new ScheduleControlBase.WeekModeHelper(this, false);

				// prefer the current mode
				if ( currentDisplayMode != WeekDisplayMode.None )
				{
					newVisibleDates = weekHelper.CalculateWeeks(sortedDates, weekHelper.WorkDaySource);

					if ( ScheduleUtilities.AreEqual(newVisibleDates, sortedDates, null) )
						return currentDisplayMode;
				}

				// then try week mode
				if ( currentDisplayMode != WeekDisplayMode.Week )
				{
					newVisibleDates = weekHelper.CalculateWeeks(sortedDates, null);

					if ( ScheduleUtilities.AreEqual(newVisibleDates, sortedDates, null) )
						return WeekDisplayMode.Week;
				}

				// and lastly work week assuming we haven't tried it yet
				if ( currentDisplayMode != WeekDisplayMode.WorkWeek )
				{
					newVisibleDates = weekHelper.CalculateWeeks(sortedDates, this.WorkDaySourceResolved);

					if ( ScheduleUtilities.AreEqual(newVisibleDates, sortedDates, null) )
						return WeekDisplayMode.WorkWeek;
				}
			}

			return WeekDisplayMode.None;
		} 
		#endregion // GetWeekModeFromDates

		// AS 3/7/12 TFS102945
		#region GetHeaderAreaTouchHelper
		internal virtual ScrollInfoTouchHelper GetHeaderAreaTouchHelper(bool isPrimary)
		{
			return null;
		} 
		#endregion //GetHeaderAreaTouchHelper

		#region PageTimeslots
		internal bool PageTimeslots(bool near)
		{
			DateRange? selection = this.GetSelectedTimeRangeForNavigation();

			if (null == selection)
				return false;

			if (!PresentationUtilities.HasNoOtherModifiers())
				return false;

			bool extendSelection = PresentationUtilities.IsModifierPressed(ModifierKeys.Shift);

			DateRange newSelectionSource = this.GetNavigationTimeslot(near ? SpatialDirection.Up : SpatialDirection.Down, extendSelection, selection.Value);

			int timeslotIndex;
			var rangeGroup = this.TimeslotInfo.GetTimeslotRangeGroup(newSelectionSource.Start, out timeslotIndex);

			if (rangeGroup == null)
				return false;

			
			int timeslotCount = ScheduleUtilities.GetTimeslotCount(rangeGroup.Ranges);
			int pageSize = Math.Max(1, (int)this.TimeslotMergedScrollInfo.Viewport - 1) * (near ? -1 : 1);
			int index = Math.Max(0, Math.Min(timeslotCount - 1, timeslotIndex + pageSize));

			DateRange range = ScheduleUtilities.CalculateDateRange(rangeGroup.Ranges, index);

			return this.SetSelectedTimeRange(range, extendSelection);
		}

		#endregion // PageTimeslots

		// AS 6/17/11 TFS78894
		#region PerformInitialScrollIntoView
		/// <summary>
		/// This method is not meant to be called directly except by the ScheduleTimeControlBase
		/// </summary>
		internal virtual bool PerformInitialScrollIntoViewImpl()
		{
			if (this.ActiveCalendar == null)
				return false;

			DateTime initialDate = this.VisibleDates[0];
			var dm = this.DataManagerResolved;

			if (dm != null)
			{
				int index = this.VisibleDates.BinarySearch(dm.CurrentDate);

				if (index < 0)
					index = ~index;

				initialDate = this.VisibleDates[Math.Min(index, this.VisibleDates.Count - 1)];
			}

			DateRange? range = this.GetHomeEndTimeslot(initialDate, true, true, this.ActiveCalendar);

			return null != range && this.BringTimeslotIntoView(range.Value.Start, 0d);
		}
		#endregion //PerformInitialScrollIntoView
		
		#endregion //Internal Methods

		#region Private Methods

		#region CreatePrimaryTimeslotHeader
		private TimeslotHeaderAdapter CreatePrimaryTimeslotHeader( DateTime start, DateTime end )
		{
			return this.CreateTimeslotHeader(start, end, null);
		}
		#endregion // CreatePrimaryTimeslotHeader

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

		#region GetHomeEndRange
		private DateRange? GetHomeEndRange( bool home, ref DateRange constrainRange, IList<TimeslotRange> ranges, WorkingHourHelper workHourHelper, IList<TimeslotRangeGroup> rangeGroups )
		{
			foreach (var range in ranges)
			{
				DateRange tempRange = new DateRange(range.StartDate, range.EndDate);

				if ( tempRange.Start > constrainRange.End )
					break;

				if ( !tempRange.Intersect(constrainRange) || tempRange.IsEmpty )
					continue;

				if (null != workHourHelper)
				{
					DateRange? workingRange = workHourHelper.GetIntersectingWorkingHour(tempRange, !home);

					if (workingRange == null)
						continue;

					tempRange.Intersect(workingRange.Value);
				}

				// now get a timeslot for the start or end
				DateTime tempDate = home || tempRange.IsEmpty ? tempRange.Start : ScheduleUtilities.GetNonInclusiveEnd(tempRange.End);

				if ( null == rangeGroups )
					return this.TimeslotInfo.GetTimeslotRange(tempDate);
				else
					return TimeslotInfo.GetTimeslotRange(tempDate, rangeGroups);
			}

			return null;
		} 
		#endregion // GetHomeEndRange

		#region GetHomeEndTimeslot
		internal DateRange? GetHomeEndTimeslot( DateTime logicalDate, bool home, bool useWorkingHours, ResourceCalendar calendar )
		{
			var group = this.TimeslotInfo.GroupFromLogicalDate(logicalDate);

			IList<TimeslotRangeGroup> rangeGroups = null;

			if ( null == group )
			{
				rangeGroups = this.TimeslotInfo.CalculateRangeGroups(new DateTime[] { logicalDate });

				if ( rangeGroups == null || rangeGroups.Count != 1 )
				{
					Debug.Assert(null != group, "No group for selection anchor?"); 
					return null;
				}

				group = rangeGroups[0];
			}

			DateTime date = this.ConvertFromLogicalDate(logicalDate);
			TimeSpan dayOffset, dayDuration;
			this.GetLogicalDayInfo(out dayOffset, out dayDuration);
			DateRange? target = null;
			DateRange logicalDayRange = ScheduleUtilities.GetLogicalDayRange(date, dayOffset, dayDuration);

			// by default limit to the working time
			List<TimeslotRange> ranges = new List<TimeslotRange>(group.Ranges);

			if ( !home )
				ranges.Reverse();

			// if control is not down and we have an active group from which we can get the calendar
			if ( useWorkingHours )
			{
				Resource resource = calendar != null ? calendar.OwningResource : null;
				var workHourHelper = new WorkingHourHelper(this, resource);

				target = GetHomeEndRange(home, ref logicalDayRange, ranges, workHourHelper, rangeGroups);

				if ( target == null )
				{
					// try again using the default working hours. must not be a work day
					workHourHelper.UseDefaultWorkingHours = true;

					target = GetHomeEndRange(home, ref logicalDayRange, ranges, workHourHelper, rangeGroups);
				}
			}

			if ( target == null )
			{
				target = GetHomeEndRange(home, ref logicalDayRange, ranges, null, rangeGroups);
			}

			return target;
		}
		#endregion // GetHomeEndTimeslot

		#region InitializeDefaultSelection
		private void InitializeDefaultSelection()
		{
			// AS 10/28/10 TFS55791/TFS50143
			// The default selection should be based upon the current date/time as outlook does.
			//
			//DateTime date = this.GetActivatableDate(DateTime.Today);
			//
			//int index = this.VisibleDates.BinarySearch(date);
			//
			//if ( index < 0 )
			//{
			//    index = Math.Min(~index, this.VisibleDates.Count - 1);
			//}
			//
			//date = this.VisibleDates[index];
			//
			//DateRange? range = this.GetDefaultSelectedRange(date);
			DateRange? range = null;
			if ( this.CalendarGroupsResolved.Count > 0 )
			{
				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				range = this.TimeslotInfo.GetNextOrClosestTimeslot(DateTime.Now);
			}

			if ( null != range )
			{
				this.SetInitialSelectedTimeRange(range);
			}
		}
		#endregion // InitializeDefaultSelection

		#region InitializeTimeslotHeaders
		/// <summary>
		/// Helper method to update the current and additional timezone elements based on the timeslot ranges.
		/// </summary>
		private void InitializeTimeslotHeaders()
		{
			if (!this.TimeslotInfo.AreGroupRangesAllocated)
				return;

			this.SetFlag(InternalFlags.ReinitializeTimeslotHeaders, false);

			if (null != _primaryTimeZone)
			{
				IList<TimeslotAreaAdapter> groups = this.TimeslotInfo.CreateTimeslotGroups(
					this.CreatePrimaryTimeslotHeader,
					null,
					null,
					null);
				_primaryTimeZone.Timeslots = groups.Count > 0 ? groups[0].Timeslots : null;
			}

			if (null != _secondaryTimeZone)
			{
				string id = this.SecondaryTimeZoneId;
				var timeZoneProvider = this.TimeZoneInfoProviderResolved;
				var alternateProvider = new AlternateTimeslotHeaderProvider(this, timeZoneProvider, id);
				IList<TimeslotAreaAdapter> groups = this.TimeslotInfo.CreateTimeslotGroups(
					alternateProvider.CreateTimeslotHeader,
					null,
					alternateProvider.AdjustDate,
					null
				);
				_secondaryTimeZone.Timeslots = groups.Count > 0 ? groups[0].Timeslots : null;
			}
		}
		#endregion //InitializeTimeslotHeaders

		// AS 10/28/10 TFS55791/TFS50143
		// Moved this out of the initialization of the selection since the selected time 
		// range we default to and what we bring into view by default are different things.
		// Outlook brings the first working hour timeslot to the top of the visible area
		// but defaults the selection relative to the current time of the current date.
		//
		#region PerformInitialScrollIntoView
		private void PerformInitialScrollIntoView()
		{
			// AS 10/5/10 TFS55791
			// This was not isolated to this scenario but came up when testing. Basically if the 
			// activities were selected but not timeslots then when we initialized the default 
			// timeslot selection we were scrolling to bring that into view. We shouldn't be 
			// scrolling in this scenario and right now I can't see a case where we should 
			// scroll to bring it into view but if the need arises then we can clear the flag.
			//
			if ( this.GetFlag(InternalFlags.InitialScrollInvoked) == false )
			{
				if ( this.VisibleDates.Count > 0 && _timeslotScrollBar != null && this.ActiveCalendar != null )
				{
					// AS 6/17/11 TFS78894
					// Use today's date as the starting point. I also moved the impl into a helper 
					// method since scheduleview and day view handle it differently.
					//
					//DateRange? range = this.GetHomeEndTimeslot(this.VisibleDates[0], true, true, this.ActiveCalendar);
					//
					//if (null != range && this.BringTimeslotIntoView(range.Value.Start, true))
					//    this.SetFlag(InternalFlags.InitialScrollInvoked, true);
					if (this.PerformInitialScrollIntoViewImpl())
						this.SetFlag(InternalFlags.InitialScrollInvoked, true);
				}
			}
		}
		#endregion // PerformInitialScrollIntoView

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region VerifyCurrentTimeIndicatorVisibility
		private void VerifyCurrentTimeIndicatorVisibility()
		{
			if (_primaryTimeZone != null)
				_primaryTimeZone.CurrentTimeIndicatorVisibility = this.CurrentTimeIndicatorVisibilityResolved;
		}
		#endregion // VerifyCurrentTimeIndicatorVisibility

		#region VerifyTimeslotWorkHourSource
		private void VerifyTimeslotWorkHourSource()
		{
			this.TimeslotInfo.WorkHourSource = this.ShowWorkingHoursOnly ? this.WorkingHoursSource : (WorkingHoursSource?)null;
		}
		#endregion // VerifyTimeslotWorkHourSource

		#region VerifyWeekWorkDaySource
		private void VerifyWeekWorkDaySource()
		{
			var displayMode = this.WeekDisplayMode;

			if (displayMode == WeekDisplayMode.None)
				this.WeekHelper = null;
			else
			{
				var weekHelper = this.WeekHelper;

				if (weekHelper == null)
					weekHelper = this.WeekHelper = new WeekModeHelper(this, false);

				weekHelper.WorkDaySource = displayMode == WeekDisplayMode.WorkWeek ? this.WorkDaySourceResolved : (WorkingHoursSource?)null;
			}
		}
		#endregion // VerifyWeekWorkDaySource

		#endregion //Private Methods

		#endregion //Methods

		#region TimeslotInitializer class
		private class TimeslotInitializer
		{
			#region Member Variables

			private CalendarGroupBase _group;
			private ScheduleTimeControlBase _control;
			private int _verifiedInitializerVersion;
			private WorkingHourHelper _workingHourHelper;

			#endregion // Member Variables

			#region Constructor
			internal TimeslotInitializer(ScheduleTimeControlBase control, CalendarGroupBase group)
			{
				_group = group;
				_control = control;
				_workingHourHelper = new WorkingHourHelper(_control, GetSelectedResource(group));
			}
			#endregion // Constructor

			#region Methods

			#region Public Methods

			#region Initialize
			public void Initialize(TimeslotBase timeslot)
			{
				Timeslot ts = timeslot as Timeslot;

				DateRange? selectedRange = _control.SelectedTimeRange;
				ts.IsSelected = _group.SelectedCalendar == _control.ActiveCalendar &&
					selectedRange != null &&
					// do not set as selected in dayview if the selection represents day selection
					_control.IsAllDaySelection == false &&
					selectedRange.Value.IntersectsWithExclusive(new DateRange(ts.Start, ts.End));

				ts.IsWorkingHour = this.IsWorkingHour(new DateRange(ts.Start, ts.End));
			}
			#endregion // Initialize

			#endregion // Public Methods

			#region Private Methods

			#region GetSelectedResource
			private static Resource GetSelectedResource( CalendarGroupBase group )
			{
				var calendar = group != null ? group.SelectedCalendar : null;
				return calendar != null ? calendar.OwningResource : null;
			}
			#endregion // GetSelectedResource

			#region IsWorkingHour

			private bool IsWorkingHour(DateRange timeslotRange)
			{
				// if the working hour information at the datamanager level has changed then release the cache
				bool hasChanged = _verifiedInitializerVersion != _control._timeslotInitializerVersion;

				if ( hasChanged )
				{
					_verifiedInitializerVersion = _control._timeslotInitializerVersion;
					_workingHourHelper.Reset(GetSelectedResource(_group));
				}

				return _workingHourHelper.IsWorkingHour(timeslotRange);
			}
			#endregion // IsWorkingHour

			#endregion // Private Methods

			#endregion // Methods
		} 
		#endregion // TimeslotInitializer class

		#region WorkingHourHelper class
		internal class WorkingHourHelper
		{
			#region Member Variables

			private Resource _resource;
			private ScheduleTimeControlBase _control;
			private AggregateTimeRange _workHourRange;
			private HashSet<DateTime> _dates;
			private TimeZoneToken _token;
			private TimeZoneInfoProvider _tzProvider;
			private bool _useDefaultWorkingHours;

			#endregion // Member Variables

			#region Constructor

			internal WorkingHourHelper( ScheduleTimeControlBase control, Resource resource )
			{
				_control = control;
				_resource = resource;
				_dates = new HashSet<DateTime>();
				_workHourRange = new AggregateTimeRange();
			}

			#endregion // Constructor

			#region Properties

			#region Resource
			internal Resource Resource
			{
				get { return _resource; }
			}

			#endregion // Resource

			#region UseDefaultWorkingHours
			internal bool UseDefaultWorkingHours
			{
				get { return _useDefaultWorkingHours; }
				set
				{
					if ( value != _useDefaultWorkingHours )
					{
						_useDefaultWorkingHours = value;
						this.Reset(_resource);
					}
				}
			}
			#endregion // UseDefaultWorkingHours

			#endregion // Properties

			#region Methods

			#region Internal Methods

			#region GetIntersectingWorkingHour
			internal DateRange? GetIntersectingWorkingHour( DateRange timeslotRange, bool fromEnd )
			{
				this.VerifyWorkingHours(timeslotRange);

				timeslotRange.Normalize();

				IList<DateRange> ranges = _workHourRange.CombinedRanges;
				int start = fromEnd ? ranges.Count - 1 : 0;
				int end = fromEnd ? -1 : ranges.Count;
				int adjustment = fromEnd ? -1 : 1;

				for ( int i = start; i != end; i += adjustment )
				{
					DateRange range = ranges[i];

					if ( range.IntersectsWithExclusive(timeslotRange) )
						return range;
				}

				return null;
			}
			#endregion // GetIntersectingWorkingHour

			#region IsWorkingHour

			internal bool IsWorkingHour( DateRange timeslotRange )
			{
				this.VerifyWorkingHours(timeslotRange);

				timeslotRange.Normalize();

				// use IntersectsWithExclusive instead of IntersectsWith
				//
				//// the end of a timeslot is exclusive
				//testRange.End = testRange.End.AddMilliseconds(-1);

				IList<DateRange> ranges = _workHourRange.CombinedRanges;

				int index = ScheduleUtilities.BinarySearch(ranges, timeslotRange.Start);

				if ( index < 0 )
					index = ~index;

				for ( int i = 0, count = ranges.Count; i < count; i++ )
				{
					DateRange range = ranges[i];

					// use IntersectsWithExclusive instead of IntersectsWith
					//
					//// the working hour range includes the end but the end is exclusive
					//range.End = range.End.AddMilliseconds(-1);

					if ( range.IntersectsWithExclusive(timeslotRange) )
						return true;

					if ( range.End > timeslotRange.End )
						break;
				}

				return false;
			}
			#endregion // IsWorkingHour

			#region Reset
			internal void Reset(Resource resource)
			{
				_dates.Clear();
				_workHourRange.Reset();
				_resource = resource;
				_tzProvider = _control.TimeZoneInfoProviderResolved;

				string tzId = _resource != null ? _resource.PrimaryTimeZoneId : null;
				_token = _tzProvider.IsValidTimeZoneId(tzId) ? _tzProvider.GetTimeZoneToken(tzId) : null;

				// if the token is specified as local just clear it to avoid the overhead of 
				// processing the dates which won't be needed because the timeslot times are
				// in local time
				if ( _token == _tzProvider.LocalToken )
					_token = null;
			}
			#endregion // Reset

			#endregion // Intenral Methods

			#region Private Methods

			#region GetWorkingHours
			private IList<TimeRange> GetWorkingHours( DateTime date )
			{
				XamScheduleDataManager dm = _control.DataManagerResolved;

				if ( null != dm && null != _resource )
				{
					var workingHours = dm.GetWorkingHours(_resource, date);

					if ( null == workingHours && _useDefaultWorkingHours )
						workingHours = dm.GetResolvedDefaultWorkingHours();

					return workingHours;
				}
				else
				{
					// this is really just for the design time experience. we'll use the default 
					// working hours for the default working days
					if ( ScheduleUtilities.IsSet(ScheduleSettings.DEFAULT_WORKDAYS, date.DayOfWeek) )
						return WorkingHoursCollection.DefaultWorkingHours;
				}

				return null;
			}
			#endregion // GetWorkingHours

			#region VerifyWorkingHours(DateTime)
			private void VerifyWorkingHours( DateTime date )
			{
				// if the selected resource is in a different timezone then 
				// we need to convert the local timeslot times into times 
				// local to the resource
				if ( _token != null )
					date = _token.ConvertFromLocal(date);

				date = date.Date;

				// if we haven't processed this date then get the working hours now
				if ( _dates.Add(date) )
				{
					var ranges = GetWorkingHours(date);

					// if we're dealing with local time for both then just add the ranges
					_workHourRange.Add(date, ranges, _token);
				}
			}
			#endregion // VerifyWorkingHours(DateTime)

			#region VerifyWorkingHours(DateRange)
			private void VerifyWorkingHours( DateRange range )
			{
				this.VerifyWorkingHours(range.Start);

				if ( !range.IsEmpty )
					this.VerifyWorkingHours(ScheduleUtilities.GetNonInclusiveEnd(range.End));
			}
			#endregion // VerifyWorkingHours(DateRange)

			#endregion // Private Methods

			#endregion // Methods
		}
		#endregion // WorkingHourHelper class

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : short
		{
			// verify state flags
			TimeslotDatesChanged = 0x1,
			ReinitializeTimeslots = 0x2,
			ReinitializeTimeslotHeaders = 0x4,

			// general flags
			InitialScrollInvoked = 0x8,

			AllVerifyStateFlags = TimeslotDatesChanged | ReinitializeTimeslots | ReinitializeTimeslotHeaders,
		}
		#endregion // InternalFlags enum

		#region AlternateTimeslotHeaderProvider class
		private class AlternateTimeslotHeaderProvider
		{
			#region Member Variables

			private TimeZoneToken _token;
			private TimeZoneInfoProvider _provider;
			private ScheduleTimeControlBase _control;

			#endregion // Member Variables

			#region Constructor
			internal AlternateTimeslotHeaderProvider( ScheduleTimeControlBase control, TimeZoneInfoProvider provider, string id )
			{
				_control = control;

				// for an invalid timezone just use the local timezone
				_token = string.IsNullOrEmpty(id) || !provider.IsValidTimeZoneId(id) 
					? provider.LocalToken ?? provider.UtcToken
					: provider.GetTimeZoneToken(id);

				_provider = provider;
			}
			#endregion // Constructor

			#region Methods

			#region AdjustDate
			public DateTime AdjustDate(DateTime date)
			{
				try
				{
					// JJD 5/26/11 - TFS76849
					// If we don't have a local token then don't try to convert since
					// it will throw an exception
					if (_token.Provider.LocalToken == null)
						return date;

					return _token.ConvertFromLocal(date);
				}
				catch (ArgumentException)
				{
					// JJD 5/26/11 - TFS76849
					// Wrap the call to GetAdjustedInvalidTime in a try/catch
					// just in case there is a problem converting the date
					try
					{
						return GetAdjustedInvalidTime(date);
					}
					catch (ArgumentException)
					{
						return date;	
					}
				}
			} 
			#endregion // AdjustDate

			#region CreatePrimaryTimeslotHeader
			public TimeslotHeaderAdapter CreateTimeslotHeader( DateTime start, DateTime end )
			{
				return _control.CreateTimeslotHeader(start, end, _token);
			}
			#endregion // CreatePrimaryTimeslotHeader

			#region GetAdjustedInvalidTime
			private DateTime GetAdjustedInvalidTime(DateTime date)
			{
				TimeZoneInfoProvider.DaylightTimeForYear daylightTime = _provider.GetDaylightTime(_provider.LocalToken, date);

				
				TimeSpan time = date.TimeOfDay;
				date = _token.ConvertFromLocal(date.Date);
				date = date.Add(time);

				return date;
			}
			#endregion // GetAdjustedInvalidTime

			#endregion // Methods
		}
		#endregion // AlternateTimeslotHeaderProvider class

		#region ScheduleTimeControlTemplateValue enum
		internal enum ScheduleTimeControlTemplateValue
		{
			TimeslotHeaderHeight,
			TimeslotHeaderWidth
		}
		#endregion // ScheduleTimeControlTemplateValue enum
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