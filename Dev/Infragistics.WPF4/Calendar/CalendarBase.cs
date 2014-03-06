using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using Infragistics.Controls.Editors;

using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using Infragistics.Controls.Editors.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Editors
{
	
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


	/// <summary>
	/// A custom control used to display one or more months.
	/// </summary>
    /// <remarks>
    /// <p class="body">The CalendarBase provides functionality similar to that of the Microsoft Vista 
    /// Common Controls Calendar class.
    /// </p>
    /// <p class="body">The control provides navigation style functionality whereby you can zoom out to more 
    /// quickly navigate the available dates and then zoom back into to change the selection. The <see cref="CurrentMode"/> 
    /// is used to control the current mode. The <see cref="XamCalendar.MinCalendarMode"/> may be used to control the lowest level 
    /// of dates that the end user may navigate into.
    /// </p>
    /// <p class="body">The default template for CalendarBase uses a <see cref="CalendarItemGroupPanel"/> 
    /// that will generate <see cref="CalendarItemGroup"/> instances based on the row/column count specified 
    /// via the <see cref="CalendarDimensions"/>. In addition, when the <see cref="AutoAdjustDimensions"/> 
    /// property is set to true, which is the default value, the panel will automatically generate additional groups 
    /// to fill the available space up to its <see cref="CalendarItemGroupPanel.MaxGroups"/>. The <see cref="ReferenceDate"/> 
    /// is used by the groups to determine which dates should be shown.
    /// </p>
    /// <p class="body">The control supports multiple selection modes which can be controlled via its <see cref="SelectionType"/>. 
    /// When using a multiple selection mode such as <b>Extended</b> or <b>Range</b>, the <see cref="SelectedDates"/> property 
    /// may be used to access/change the selection up to the <see cref="XamCalendar.MaxSelectedDates"/>. The control also exposes a <see cref="SelectedDate"/> property which is 
    /// primarily used when in a single select mode. When in a multiselect mode, this property will return the first selected date.
    /// </p>
    /// <p class="body">The control exposes a number of properties that may be used to restrict the selectable dates. The 
    /// <see cref="XamCalendar.MinDate"/> and <see cref="XamCalendar.MaxDate"/> are used to control the range within which the user may navigate. You can 
	/// then disable dates within that range using either the <see cref="XamCalendar.DisabledDaysOfWeek"/> and <see cref="XamCalendar.DisabledDates"/>.
    /// </p>
    /// </remarks>
	[TemplatePart(Name = PartRootPanel, Type = typeof(Grid))]
	[StyleTypedProperty(Property = "CalendarItemStyle", StyleTargetType = typeof(CalendarItem))]
	[StyleTypedProperty(Property = "CalendarDayStyle", StyleTargetType = typeof(CalendarDay))]
	[StyleTypedProperty(Property = "ScrollNextRepeatButtonStyle", StyleTargetType = typeof(System.Windows.Controls.Primitives.RepeatButton))]
	[StyleTypedProperty(Property = "ScrollPreviousRepeatButtonStyle", StyleTargetType = typeof(System.Windows.Controls.Primitives.RepeatButton))]
	[StyleTypedProperty(Property = "TodayButtonStyle", StyleTargetType = typeof(Button))]
	public abstract class CalendarBase : Control,
		ISelectionHost,
		ICommandTarget



	{
		#region Member Variables

		private const string PartRootPanel = "RootPanel";

		internal const DayOfWeekFlags DefaultWorkdays = DayOfWeekFlags.Monday | DayOfWeekFlags.Tuesday | DayOfWeekFlags.Wednesday | DayOfWeekFlags.Thursday | DayOfWeekFlags.Friday;
		
		private Panel											_rootPanel;

		private Canvas											_templatePanel;

		private CalendarZoomMode								_minCalendarModeResolved;
		private DayOfWeekFlags									_disabledDaysOfWeekInternal = DayOfWeekFlags.None;
		private Visibility										_disabledDaysOfWeekVisibilityInternal = Visibility.Visible;
		private DayOfWeek?										_firstDayOfWeekResolved;
		private CalendarWeekRule?								_weekRuleResolved;
		private int												_maxSelectedDatesInternal;
		private DayOfWeekFlags									_workDaysInternal = DefaultWorkdays;

		// JJD 9/9/11 - TFS74024 - Added
		private bool											_selectedStateVerifyPending;






		internal static readonly object MeasurePanelId = new object();

		private CalendarManager									_calendarManager;

		private System.Globalization.Calendar					_suppliedCalendar;
		private DateTimeFormatInfo								_suppliedDateTimeFormat;

        private SelectedDateCollection                          _selectedDates;
        
		private SelectionStrategyBase							_selectionStrategy;
		private ObservableCollectionExtended<DateTime>			_selectedDatesSnapshot;
		internal DateRange?										_pivotRange;

        private ObservableCollectionExtended<DayOfWeek>         _daysOfWeekInternal;
        private ReadOnlyObservableCollection<DayOfWeek>         _daysOfWeek;
        internal const int                                       AllDays = 0x7F;
        private List<CalendarItemGroup>                         _groups;
        private CalendarItemGroup                               _groupForSizing;
        private CalendarDateRangeCollection                     _disabledDates;

		private CalendarResourceProvider					_defaultResourceProvider;

        // this flag is used to track whether we need to wait to bring
        // a date (particularly the active date) into view. when null, 
        // we should just bring the date into view. if false then we
        // have a current selection strategy but don't have a date that
        // we need to bring into view. if true then we have a current
        // selection strategy still and we have a date that we need to
        // bring into view when the selection strategy is cleared.
        private bool?                                           _shouldBringActiveDateIntoView;

        // the following info is used when navigating rows/groups (i.e. using
        // up/down/pageup/pagedown keys) so that navigation will "move around"
        // disabled days while keeping the preferred position/offset. these are
        // cleared when the active date, etc. changes but set by the routines
        // that would use the info
        private int                                             _preferredNavColumn = -1;
        private int                                             _preferredGroupOffset;
        private DateTime?                                       _preferredGroupNavStart;

		// JJD 1/25/11
		// While the mouse is down (during a selection drag operation we don't want to update the real
		// selected dates collection so we need to clone it and use a temporary collection in its place.
		// This is to prevent the selected dates from changing until the mouse is released
		private SelectionSnapshot								_mouseDownSnapshot;

        private CalendarItemGroup								_zoomFocusGroup;

        // AS 10/3/08 TFS8607
        private bool                                            _suppressBringActiveDateInView;

        // AS 2/5/09 TFS10681
        private bool?                                           _shouldProcessActiveSelectionChanged = null;
        private WeakReference                                   _activeStrategy;

        // AS 2/9/09 TFS11631
        private bool                                            _preventFocusActiveItem;

		// JJD 1/11/12 - TFS96836 - Strip off time portion from max value
		//DateRange												_allowableDateRange = new DateRange(DateTime.MinValue, DateTime.MaxValue);
		DateRange												_allowableDateRange = new DateRange(DateTime.MinValue, DateTime.MaxValue.Date);
		
		private SelectionController								_selectionController;
		
		private bool											_notifyResourcesChangedPending;
		private bool											_notifySelectionActiveChangePending;

		private bool											_isInitialized;

		// JJD 10/12/11 - TFS89043
		// added member to hold a strong reference on the token returned from
		// TimeManager's AddTimeRange so our callback will be called when Today changes
		private object											_todayToken;








        #endregion //Member Variables

		#region Constructor

		static CalendarBase()
		{

			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLanguageChanged)));
			//Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
			//Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

			Style style = new Style();
			style.Seal();
			Control.FocusVisualStyleProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(style));

            // we don't want the tab index of the children having any comparison to those
            // outside the control
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

            // when focus is within the monthcalendar, it should not be a tabstop
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(CalendarBase), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceIsTabStop)));

		}






		internal CalendarBase()
		{
			this._calendarManager = new CalendarManager();
			
			this._selectionController = new SelectionController((ISelectionHost)this);

			// AS 8/19/08 ActiveStrategy
			// The XamMonthCalendar mimics the common controls calendar behavior of
			// bringing the group for a leading/trailing item into view when a leading/
			// trailing item is activated. The group however should not be brought into
			// view while the mouse is being used to activate the item. In order to know 
			// when this happens (because the mouse may or may not be captured at the 
			// time and may or may not be down on the element) I'm adding a virtual 
			// method for a derived class to know when the selection strategy used 
			// by the selection controller is changed.
			//
			this._selectionController.ActiveStrategyChanged += new EventHandler(OnActiveSelectionStrategyChanged);

            this._groups = new List<CalendarItemGroup>();

			// initialize the calendar manager
			this._calendarManager.FirstDayOfWeek = this.FirstDayOfWeekInternal;
			this._calendarManager.WeekRule = this.WeekRuleInternal;

            this._daysOfWeekInternal = new ObservableCollectionExtended<DayOfWeek>();
            this._daysOfWeekInternal.CollectionChanged += new NotifyCollectionChangedEventHandler(OnDaysOfWeekChanged);
            this._daysOfWeekInternal.ReInitialize(this._calendarManager.GetDaysOfWeek());
            this._daysOfWeek = new ReadOnlyObservableCollection<DayOfWeek>(this._daysOfWeekInternal);

            this._disabledDates = new CalendarDateRangeCollection(this);
            this._disabledDates.CollectionChanged += new NotifyCollectionChangedEventHandler(OnDisabledDatesChanged);

			this._selectedDates = new SelectedDateCollection(this);
			this._selectedDatesSnapshot = new ObservableCollectionExtended<DateTime>();

            this.SetValue(CalendarPropertyKey, this);

			//// AS 3/23/10 TFS26461
			//_currentDateWatcher = new CurrentDate();
			//_currentDateWatcher.ValueChanged += new EventHandler(OnCurrentDateChanged);

			//// AS 8/16/10 TFS36762
			//// The control may have been created after the date changed in 
			//// which case we need to update the today property value anyway.
			////
			//this.SetValue(TodayPropertyKey, _currentDateWatcher.GetValue(CurrentDate.ValueProperty));

			// JJD 10/12/11 - TFS89043
			// Moved logic into helper method
			this.InitializeToday();

			// AS 6/12/12 TFS111820
			CommandSourceManager.RegisterCommandTarget(this);
		}
        
		#endregion //Constructor

		#region Properties

		#region Public

		#region ActiveDate

		/// <summary>
		/// Identifies the <see cref="ActiveDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActiveDateProperty = DependencyPropertyUtilities.Register("ActiveDate",
			typeof(DateTime?), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnActiveDateChanged))
			);

		private void ResolveActiveDate()
		{
			if (!_isInitialized)
				return;

			DateTime? resolvedDate = this.ActiveDate;

            // AS 9/4/08
            // If we have a selected date but no active date then we should use
            // the selected date as the active date. If we have neither then use
            // today's date. In this way a control like the XamDateTimeEditor that
            // is only binding the SelectedDate. Can have the MC have a day with 
            // focus even if there is no selection.
            //
            if (null == resolvedDate)
                resolvedDate = this.SelectedDate ?? DateTime.Today;

			if (null != resolvedDate)
			{
				DateTime dateValue = (DateTime)resolvedDate;

                CalendarZoomMode mode = this.CurrentMode;

                // we always want to use the earliest start date
                dateValue = this._calendarManager.GetItemStartDate(dateValue, mode);

                // make sure its in the min/max range
                dateValue = this.ConstrainBetweenMinMaxDate(dateValue);

                // first get the range of dates for the item that contains
                // the active date. we may be able to just get another date
                // for that same item so the same item remains selected
                DateTime itemEndDate = this._calendarManager.GetItemEndDate(dateValue, mode);

                itemEndDate = this.ConstrainBetweenMinMaxDate(itemEndDate);

                // try to get an activatable date within that item
                resolvedDate = this.GetActivatableDate(dateValue, itemEndDate, true, mode);

                if (resolvedDate == null)
                {
                    // find the closest date within the group
                    resolvedDate = this.GetClosestActivatableDate(dateValue, itemEndDate);
                }
			}

			this.ActiveDate = resolvedDate;
		}

		private static void OnActiveDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// make sure the active date is in view
			CalendarBase cal = (CalendarBase)d;

			if (cal != null)
			{
                cal.ClearPreferredNavInfo();

				if (e.NewValue is DateTime)
				{
					DateTime newDate = (DateTime)e.NewValue;

					// make sure the date is in view
                    // AS 10/3/08 TFS8607
                    // When the calendar mode changes, we need to fix up the active date since while
                    // navigating the active date could be a date that when not in navigation mode
                    // is disabled. However, we don't want that date to be scrolled into view when
                    // zooming back in or you will never be able to see the disabled month that you
                    // zoomed into.
                    //
					//if (cal.IsDateInView(newDate, true) == false)
					if (false == cal._suppressBringActiveDateInView &&
						cal.IsDateInView(newDate, true) == false)
					{
                        // if the selection strategy is active and the date
                        // is in the leading/trailing then do not bring
                        // it into view. instead, wait until later
                        if (cal._shouldBringActiveDateIntoView != null &&
							// AS 10/14/09 FR11859
							// If the item is a leading/trailing group then its a 
							// leading/trailing item as well.
							//
							//cal.IsDateInView(newDate, false) == true)
                            cal.IsDateInView(newDate, false, false) == true)
                        {
                            cal._shouldBringActiveDateIntoView = true;
                        }
                        else
                        {
                            // if we were going to bring the active date
                            // into view and we're activating a non-leading
                            // trailing date then do not bring it into view
                            // when the strategy is changed to null
                            if (cal._shouldBringActiveDateIntoView == true)
                                cal._shouldBringActiveDateIntoView = false;

                            cal.BringDateIntoView(newDate);
                        }


						// if the control has keyboard focus then update
						// the layout immediately since we need to put focus
						// into it
						if (cal.IsKeyboardFocusWithin)
							cal.UpdateLayout();

					}

                    bool isNavigationMode = cal.IsNavigationMode;

					// deactivate the previous active item
					if (e.OldValue is DateTime)
					{
						CalendarItem oldItem = cal.GetItem((DateTime)e.OldValue);

                        if (null != oldItem)
                        {
                            oldItem.SetValue(CalendarItem.IsActivePropertyKey, KnownBoxes.FalseBox);

                            if (isNavigationMode)
                                oldItem.SetValue(CalendarItem.IsSelectedPropertyKey, KnownBoxes.FalseBox);
                        }
					}

					// find the calendar day
					CalendarItem item = cal.GetItem(newDate);

					if (null != item)
					{
						item.SetValue(CalendarItem.IsActivePropertyKey, KnownBoxes.TrueBox);

                        if (isNavigationMode)
                            item.SetValue(CalendarItem.IsSelectedPropertyKey, KnownBoxes.TrueBox);

						
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

					}
				}
				else if (e.OldValue is DateTime)
				{
					CalendarItem oldItem = cal.GetItem((DateTime)e.OldValue);

					if (null != oldItem)
						oldItem.SetValue(CalendarDay.IsActivePropertyKey, KnownBoxes.FalseBox);
				}
			}
		}

		/// <summary>
		/// Returns or sets the date of the active day.
		/// </summary>
        /// <remarks>
        /// <p class="body">The ActiveDate is used to determine which <see cref="CalendarItem"/> within the control has the input focus when the keyboard focus is 
        /// within the <see cref="CalendarBase"/>. Keyboard navigation within the control is then based on that item's dates.</p>
        /// </remarks>
		/// <seealso cref="ActiveDateProperty"/>
		//[Description("Returns or sets the date of the active day.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]



		public DateTime? ActiveDate
		{
			get
			{
				return (DateTime?)this.GetValue(CalendarBase.ActiveDateProperty);
			}
			set
			{
				this.SetValue(CalendarBase.ActiveDateProperty, value);
			}
		}

		#endregion //ActiveDate

		#region AllowLeadingAndTrailingGroupActivation

		/// <summary>
		/// Identifies the <see cref="AllowLeadingAndTrailingGroupActivation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowLeadingAndTrailingGroupActivationProperty = DependencyPropertyUtilities.Register("AllowLeadingAndTrailingGroupActivation",
			typeof(bool), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Returns or sets a boolean that indicates if items within a CalendarItemGroup whose IsLeadingGroup or IsTrailingGroup is true can be activated.
		/// </summary>
		/// <seealso cref="AllowLeadingAndTrailingGroupActivationProperty"/>
		/// <seealso cref="CalendarItemGroup.IsLeadingGroup"/>
		/// <seealso cref="CalendarItemGroup.IsTrailingGroup"/>
		//[Description("Returns or sets a boolean that indicates if items within a CalendarItemGroup whose IsLeadingGroup or IsTrailingGroup is true can be activated.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool AllowLeadingAndTrailingGroupActivation
		{
			get
			{
				return (bool)this.GetValue(CalendarBase.AllowLeadingAndTrailingGroupActivationProperty);
			}
			set
			{
				this.SetValue(CalendarBase.AllowLeadingAndTrailingGroupActivationProperty, value);
			}
		}

		#endregion //AllowLeadingAndTrailingGroupActivation

    	#region AutoAdjustDimensions

		/// <summary>
		/// Identifies the <see cref="AutoAdjustDimensions"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AutoAdjustDimensionsProperty = DependencyPropertyUtilities.Register("AutoAdjustDimensions",
			typeof(bool), typeof(CalendarBase), 
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Returns or sets whether the control should automatically calculate and change the calendar dimensions based on the size used to arrange the CalendarBase.
		/// </summary>
		/// <remarks>
		/// <p class="body">The AutoAdjustDimensions property is used by the <see cref="CalendarItemGroupPanel"/> within the 
		/// template of the <see cref="CalendarBase"/> that indicates whether it should automatically generate additional <see cref="CalendarItemGroup"/> 
		/// instances when it has more space available than can be used by the groups specified by the <see cref="CalendarDimensions"/>.</p>
		/// <p class="note">If you retemplate the <see cref="CalendarBase"/> such that it does not contain a <see cref="CalendarItemGroupPanel"/>, 
		/// this property will not affect the display of the control.</p>
		/// </remarks>
		/// <seealso cref="AutoAdjustDimensionsProperty"/>
		/// <seealso cref="CalendarDimensions"/>
		/// <seealso cref="CalendarItemGroupPanel"/>
		/// <seealso cref="CalendarItemGroupPanel.MaxGroups"/>
		//[Description("Returns or sets whether the control should automatically calculate and change the calendar dimensions based on the size used to arrange the CalendarBase.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public bool AutoAdjustDimensions
		{
			get
			{
				return (bool)this.GetValue(CalendarBase.AutoAdjustDimensionsProperty);
			}
			set
			{
				this.SetValue(CalendarBase.AutoAdjustDimensionsProperty, value);
			}
		}

		#endregion //AutoAdjustDimensions

		#region CalendarDayStyle

		/// <summary>
		/// Identifies the <see cref="CalendarDayStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarDayStyleProperty = DependencyPropertyUtilities.Register("CalendarDayStyle",
			typeof(Style), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnItemStyleChanged))
			);

		private static void OnItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((CalendarBase)d).NotifyGroupChange(CalendarChange.ItemStyleChange);
		}

		/// <summary>
		/// Returns or sets the default style to use for <see cref="CalendarDay"/> instances within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set, the <see cref="CalendarItemArea"/> will set the <see cref="FrameworkElement.Style"/> property 
		/// of the <see cref="CalendarDay"/> instances that it creates.</p>
		/// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using this value, any local styles that target the <see cref="CalendarDay"/> will not be used.</p>
		/// </remarks>
		/// <seealso cref="CalendarDayStyleProperty"/>
		/// <seealso cref="CalendarItemStyle"/>
		//[Description("Returns or sets the default style to use for CalendarDay instances within the control.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public Style CalendarDayStyle
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.CalendarDayStyleProperty);
			}
			set
			{
				this.SetValue(CalendarBase.CalendarDayStyleProperty, value);
			}
		}

		#endregion //CalendarDayStyle

		#region CalendarItemStyle

		/// <summary>
		/// Identifies the <see cref="CalendarItemStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarItemStyleProperty = DependencyPropertyUtilities.Register("CalendarItemStyle",
			typeof(Style), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnItemStyleChanged))
			);

		/// <seealso cref="CalendarItemStyleProperty"/>
		/// <summary>
		/// Returns or sets the default style to use for <see cref="CalendarItem"/> instances within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">When this property is set, the <see cref="CalendarItemArea"/> will set the <see cref="FrameworkElement.Style"/> property 
		/// of the <see cref="CalendarItem"/> instances that it creates.</p>
		/// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using this value, any local styles that target the <see cref="CalendarItem"/> will not be used.</p>
		/// </remarks>
		/// <seealso cref="CalendarItemStyleProperty"/>
		/// <seealso cref="CalendarDayStyle"/>
		//[Description("Returns or sets the default style to use for CalendarItem instances within the control.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public Style CalendarItemStyle
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.CalendarItemStyleProperty);
			}
			set
			{
				this.SetValue(CalendarBase.CalendarItemStyleProperty, value);
			}
		}

		#endregion //CalendarItemStyle

		#region CurrentMode

		/// <summary>
		/// Identifies the <see cref="CurrentMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentModeProperty = DependencyPropertyUtilities.Register("CurrentMode",
			typeof(CalendarZoomMode), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(CalendarZoomMode.Days, new PropertyChangedCallback(OnCurrentModeChanged))
			);

		private void ResolveCurrentMode()
        {
            CalendarZoomMode mode = this.CurrentMode;

			CalendarZoomMode minMode = this.MinCalendarModeResolved;

			if (mode < minMode)
				mode = minMode;
			else
			{
				CalendarZoomMode maxMode = this.MaxCalendarMode;

				if (mode > maxMode)
					mode = maxMode;
			}

			this.CurrentMode = mode;
        }

		private static void OnCurrentModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            CalendarBase cal = (CalendarBase)d;

            cal.ClearPreferredNavInfo();

            // AS 10/3/08 TFS8607
            // The active date changed will try to get an item from the group 
            // to focus. This is a problem because the group's mode hasn't changed
            // so we will get an item from the "old" mode. So we need to push the
            // new mode into the groups.
            //
            cal.NotifyGroupChange(CalendarChange.CurrentModeChanged);

            // AS 10/3/08
            // We need to fix up the active date but we don't want to bring the date
            // into view. We are zooming out/in a specific group.
            //
            bool oldSuppress = cal._suppressBringActiveDateInView;
            cal._suppressBringActiveDateInView = true;

            try
            {
                cal.ResolveActiveDate();
            }
            finally
            {
                cal._suppressBringActiveDateInView = oldSuppress;
            }
        }

		/// <summary>
		/// Returns or sets which types of calendar items will be displayed to the end user.
		/// </summary>
        /// <remarks>
        /// <p class="body">The <see cref="CalendarBase"/> provides the ability to zoom out to see larger ranges of dates and then zoom back 
        /// in to change the selection similar to the functionality found in the Microsoft Vista Common Controls Calendar. The CurrentMode 
        /// controls the current mode that the contained <see cref="CalendarItemGroup"/> instances use to initialize its items. For example, when set to 
        /// <b>Days</b>, which is the default value, the CalendarItemGroup will contain <see cref="CalendarDay"/> instances where each represents 
        /// a discrete date. When set to <b>Months</b>, the CalendarItemGroups will contain <see cref="CalendarItem"/> instances where each represents 
        /// a single month within a specific year.</p>
		/// <p class="body">There are two commands (<see cref="CalendarCommandType.ZoomOutCalendarMode"/> 
		/// and <see cref="CalendarCommandType.ZoomInCalendarMode"/>) that may be used to change the CurrentMode.</p>
		/// <p class="note"><b>Note:</b> The value for this property cannot be set to a value that would be less than the <see cref="XamCalendar.MinCalendarMode"/>.</p>
        /// </remarks>
        /// <seealso cref="CurrentModeProperty"/>
		/// <seealso cref="XamCalendar.MinCalendarMode"/>
		//[Description("Returns or sets which types of calendar items will be displayed to the end user.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]

        [DependsOn("MinCalendarMode")]

        public CalendarZoomMode CurrentMode
		{
			get
			{
				return (CalendarZoomMode)this.GetValue(CalendarBase.CurrentModeProperty);
			}
			set
			{
				this.SetValue(CalendarBase.CurrentModeProperty, value);
			}
		}

		#endregion //CurrentMode

		#region DayOfWeekHeaderFormat

		/// <summary>
		/// Identifies the <see cref="DayOfWeekHeaderFormat"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekHeaderFormatProperty = DependencyPropertyUtilities.Register("DayOfWeekHeaderFormat",
			typeof(DayOfWeekHeaderFormat), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(DayOfWeekHeaderFormat.TwoCharacters, OnDayOfWeekHeaderFormatChanged));

		private static void OnDayOfWeekHeaderFormatChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = target as CalendarBase;

			instance.InvalidateDisplay();
			instance.NotifyGroupChange(CalendarChange.CalendarInfoChanged);
		}

        /// <summary>
        /// Returns or sets the format for the header of the days of the week.
        /// </summary>
        /// <seealso cref="DayOfWeekHeaderFormatProperty"/>
         //[Description("Returns or sets the format for the header of the days of the week.")]
        //[Category("Calendar Properties")] // Behavior
        [Bindable(true)]
		public DayOfWeekHeaderFormat DayOfWeekHeaderFormat
		{
			get
			{
				return (DayOfWeekHeaderFormat)this.GetValue(CalendarBase.DayOfWeekHeaderFormatProperty);
			}
			set
			{
				this.SetValue(CalendarBase.DayOfWeekHeaderFormatProperty, value);
			}
		}

		#endregion //DayOfWeekHeaderFormat

		#region DayOfWeekHeaderVisibility

		/// <summary>
		/// Identifies the <see cref="DayOfWeekHeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekHeaderVisibilityProperty = DependencyPropertyUtilities.Register("DayOfWeekHeaderVisibility",
			typeof(Visibility), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnDayOfWeekHeaderVisibilityChanged))
			);

		private static void OnDayOfWeekHeaderVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance.NotifyGroupChange(CalendarChange.WeekNumberVisibility);
		}

		/// <summary>
		/// Returns or sets a value indicating whether the day of week headers should be displayed.
		/// </summary>
		/// <seealso cref="DayOfWeekHeaderFormat"/>
		/// <seealso cref="DayOfWeekHeaderVisibilityProperty"/>
        //[Description("Returns or sets a value indicating whether the day of week headers should be displayed.")]
        //[Category("Calendar Properties")] // Behavior
        [Bindable(true)]
		public Visibility DayOfWeekHeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarBase.DayOfWeekHeaderVisibilityProperty);
			}
			set
			{
				this.SetValue(CalendarBase.DayOfWeekHeaderVisibilityProperty, value);
			}
		}

		#endregion //DayOfWeekHeaderVisibility

		#region Dimensions

		/// <summary>
		/// Identifies the <see cref="Dimensions"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DimensionsProperty = DependencyPropertyUtilities.Register("Dimensions",
			typeof(CalendarDimensions), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(new CalendarDimensions(1,1), new PropertyChangedCallback(OnDimensionsChanged))
			);

		private static void OnDimensionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

		}

		/// <summary>
		/// Returns or sets a value indicating the preferred rows and columns of groups to be displayed within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The CalendarDimensions is used by the <see cref="CalendarItemGroupPanel"/> within the template of the 
		/// <see cref="CalendarBase"/> to determine the minimum number of rows and columns of <see cref="CalendarItemGroup"/> 
		/// instances that it should create and arrange. If the <see cref="AutoAdjustDimensions"/> is true, which is the 
		/// default value, and the CalendarItemGroupPanel has space to display mode groups it will automatically create additional 
		/// groups up to its <see cref="CalendarItemGroupPanel.MaxGroups"/>.</p>
		/// <p class="note">If you re-template the <see cref="CalendarBase"/> such that it does not contain a <see cref="CalendarItemGroupPanel"/>, 
		/// this property will not affect the display of the control.</p>
		/// </remarks>
		/// <seealso cref="DimensionsProperty"/>
		/// <seealso cref="AutoAdjustDimensions"/>
		/// <seealso cref="CalendarItemGroupPanel"/>
		//[Description("Returns or sets a value indicating the preferred rows and columns of groups to be displayed within the control.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public CalendarDimensions Dimensions
		{
			get
			{
				return (CalendarDimensions)this.GetValue(CalendarBase.DimensionsProperty);
			}
			set
			{
				this.SetValue(CalendarBase.DimensionsProperty, value);
			}
		}

		#endregion //Dimensions

		#region IsSelectionActive

		private static readonly DependencyPropertyKey IsSelectionActivePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsSelectionActive",
			typeof(bool), typeof(CalendarBase), KnownBoxes.FalseBox, OnIsSelectionActiveChange);

		private static void OnIsSelectionActiveChange(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = target as CalendarBase;

			// JJD 3/29/11 - Optimization
			// Delay processing the notification so that if we are also changing the ref data
			// we can bypass the redundant logic
			// reset the _notifySelectionActiveChangePending so we ignore processing the notification
			//instance.NotifyGroupChange(CalendarChange.IsSelectionActiveChanged);
			if (instance._notifySelectionActiveChangePending == false)
			{
				instance._notifySelectionActiveChangePending = true;
				instance.Dispatcher.BeginInvoke(new CalendarUtilities.MethodInvoker(instance.ProcessSelectionActiveChange));
			}
		}

		private void ProcessSelectionActiveChange()
		{
			if (_notifySelectionActiveChangePending)
			{
				_notifySelectionActiveChangePending = false;
				this.NotifyGroupChange(CalendarChange.IsSelectionActiveChanged);
			}
		}

		/// <summary>
		/// Identifies the read-only <see cref="IsSelectionActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectionActiveProperty = IsSelectionActivePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if keyboard focus is within the control (read-only)
		/// </summary>
		/// <remarks>
		/// <p class="body">The IsSelectionActive property is a read only property similar to that of 
		/// the <see cref="System.Windows.Controls.Primitives.Selector.GetIsSelectionActive(DependencyObject)"/> that is used to indicate whether keyboard focus is 
		/// within the control and therefore can be used to control how the selected items are rendered.</p>
		/// </remarks>
		/// <seealso cref="IsSelectionActiveProperty"/>
		public bool IsSelectionActive
		{
			get
			{
				return (bool)this.GetValue(CalendarBase.IsSelectionActiveProperty);
			}
			internal set
			{
				this.SetValue(CalendarBase.IsSelectionActivePropertyKey, value);
			}
		}

        #endregion //IsSelectionActive (readonly)

		#region LeadingAndTrailingDatesVisibility

		/// <summary>
		/// Identifies the <see cref="LeadingAndTrailingDatesVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LeadingAndTrailingDatesVisibilityProperty = DependencyPropertyUtilities.Register("LeadingAndTrailingDatesVisibility",
			typeof(Visibility), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnLeadingAndTrailingDatesVisibilityChanged))
			);

		private static void OnLeadingAndTrailingDatesVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			CalendarUtilities.ValidateNonHiddenVisibility(e);
		}

		/// <summary>
		/// Returns or sets a boolean indicating whether to show days from the month before/after the first/last visible month.
		/// </summary>
		/// <remarks>
		/// <p class="body">Leading and trailing dates are those dates displayed within a group that do not belong that to that particular 
		/// group. For example, when viewing a gregorian calendar that displays August 2008 with a default first day of week of 
		/// Sunday, a calendar has space available to show the 27-31 of July (leading) and the 1-6 of September (trailing). By default 
		/// leading and trailing dates are displayed within the first and last <see cref="CalendarItemGroup"/> within the control.</p>
		/// <p class="note"><b>Note:</b> The default template for the CalendarBase uses a <see cref="CalendarItemGroupPanel"/> that 
		/// ensures that only the first and last CalendarItemGroup instances have their <see cref="CalendarItemGroup.ShowTrailingDates"/> 
		/// and <see cref="CalendarItemGroup.ShowLeadingDates"/> initialized based on this property. If you retemplate the control to 
		/// directly contain CalendarItemGroup instances you will need to control which groups use this property.</p>
		/// </remarks>
		/// <seealso cref="LeadingAndTrailingDatesVisibilityProperty"/>
		/// <seealso cref="CalendarItemGroup.ShowTrailingDates"/>
		/// <seealso cref="CalendarItemGroup.ShowLeadingDates"/>
		//[Description("Returns or sets a boolean indicating whether to show days from the month before/after the first/last visible month.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public Visibility LeadingAndTrailingDatesVisibility
		{
			get { return (Visibility)this.GetValue(CalendarBase.LeadingAndTrailingDatesVisibilityProperty); }
			set { this.SetValue(CalendarBase.LeadingAndTrailingDatesVisibilityProperty, value); }
		}

		#endregion //LeadingAndTrailingDatesVisibility

		#region MaxDateResolved

		private static readonly DependencyPropertyKey MaxDateResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MaxDateResolved",
			// JJD 1/11/12 - TFS96836 - Strip off time portion from MaxValue
			//typeof(DateTime), typeof(CalendarBase), DateTime.MaxValue, null);
			typeof(DateTime), typeof(CalendarBase), DateTime.MaxValue.Date, null);

		/// <summary>
		/// Identifies the read-only <see cref="MaxDateResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDateResolvedProperty = MaxDateResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the maximum allowed date (read-only)
		/// </summary>
		/// <seealso cref="MaxDateResolvedProperty"/>
		public DateTime MaxDateResolved
		{
			get
			{
				return (DateTime)this.GetValue(XamCalendar.MaxDateResolvedProperty);
			}
			private set
			{
				this.SetValue(XamCalendar.MaxDateResolvedPropertyKey, value);
			}
		}

		#endregion //MaxDateResolved

		#region MinCalendarModeResolved

		private static readonly DependencyPropertyKey MinCalendarModeResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MinCalendarModeResolved",
			typeof(CalendarZoomMode), typeof(CalendarBase),
			CalendarZoomMode.Days,
			new PropertyChangedCallback(OnMinCalendarModeResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="MinCalendarModeResolved"/> dependency property
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty MinCalendarModeResolvedProperty = MinCalendarModeResolvedPropertyKey.DependencyProperty;

		private static void OnMinCalendarModeResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance._minCalendarModeResolved = (CalendarZoomMode)e.NewValue;

			instance.ResolveCurrentMode();
		}

		/// <summary>
		/// Returns the minimum <see cref="CalendarZoomMode"/> that is supported
		/// </summary>
		/// <seealso cref="XamCalendar.MinCalendarMode"/>
		/// <seealso cref="MinCalendarModeResolvedProperty"/>
		[EditorBrowsable(EditorBrowsableState.Never)] 
		public CalendarZoomMode MinCalendarModeResolved
		{
			get
			{
				return (CalendarZoomMode)this.GetValue(CalendarBase.MinCalendarModeResolvedProperty);
			}
			internal set
			{
				this.SetValue(CalendarBase.MinCalendarModeResolvedPropertyKey, value);
			}
		}

		#endregion //MinCalendarModeResolved
		
		#region MinDateResolved

		private static readonly DependencyPropertyKey MinDateResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("MinDateResolved",
			typeof(DateTime), typeof(CalendarBase), DateTime.MinValue, null);

		/// <summary>
		/// Identifies the read-only <see cref="MinDateResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDateResolvedProperty = MinDateResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the minimum allowed date (read-only)
		/// </summary>
		/// <seealso cref="MinDateResolvedProperty"/>
		public DateTime MinDateResolved
		{
			get
			{
				return (DateTime)this.GetValue(XamCalendar.MinDateResolvedProperty);
			}
			private set
			{
				this.SetValue(XamCalendar.MinDateResolvedPropertyKey, value);
			}
		}

		#endregion //MinDateResolved

		#region ReferenceDate

		/// <summary>
		/// Identifies the <see cref="ReferenceDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ReferenceDateProperty = DependencyPropertyUtilities.Register("ReferenceDate",
			typeof(DateTime?), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnReferenceDateChanged))
			);

        internal void ResolveReferenceDate()
        {
			if (!_isInitialized)
				return;

			// JJD 1/12/12 - TFS96805 - refactored
			// Moved logic into overload that takes the date setting
			//DateTime? setting = this.ReferenceDate;

			//DateTime dateValue;

			//if (null != setting)
			//    dateValue = this.GetDefaultReferenceDate(setting.Value);
			//else
			//    dateValue = this.GetDefaultReferenceDate();

			//this.ReferenceDate = dateValue;
			this.ResolveReferenceDate(this.ReferenceDate);
        }

		// JJD 1/12/12 - TFS96805
		// Added overload that takes the date setting
		internal void ResolveReferenceDate(DateTime? date)
		{
			if (!_isInitialized)
				return;

            DateTime dateValue;

            if (null != date)
                dateValue = this.GetDefaultReferenceDate(date.Value);
            else
                dateValue = this.GetDefaultReferenceDate();

            this.ReferenceDate = dateValue;
		}

        private static void OnReferenceDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
			CalendarBase cal = d as CalendarBase;
            Debug.Assert(null != e.NewValue);

            // if it happens to get cleared reset to today
            if (e.NewValue == null)
            {
                d.SetValue(ReferenceDateProperty, DateTime.Today);
            }

			// reset the _notifySelectionActiveChangePending so we ignore processing the notification
			cal._notifySelectionActiveChangePending = false;

			cal.NotifyGroupChange(CalendarChange.ReferenceDateChanged);

        }

        /// <summary>
        /// Returns or sets a date that is used to determine the dates that should be displayed within the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">The reference date is used to determine the dates that are displayed within the reference group. The 
        /// reference group is the <see cref="CalendarItemGroup"/> whose <see cref="CalendarItemGroup.ReferenceGroupOffset"/> is 
        /// zero. By default, the CalendarBase used a <see cref="CalendarItemGroupPanel"/> that autogenerates the groups 
        /// and sets the first created group's ReferenceGroupOffset to 0.</p>
        /// </remarks>
        /// <seealso cref="ReferenceDateProperty"/>
        //[Description("Returns or sets a date that is used to determine the dates that should be displayed within the control.")]
        //[Category("Calendar Properties")] // Behavior
        [Bindable(true)]



		public DateTime? ReferenceDate
        {
            get
            {
                return (DateTime?)this.GetValue(CalendarBase.ReferenceDateProperty);
            }
            set
            {
                this.SetValue(CalendarBase.ReferenceDateProperty, value);
            }
        }

        #endregion //ReferenceDate

		#region ResourceProvider

		/// <summary>
		/// Identifies the <see cref="ResourceProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResourceProviderProperty = DependencyPropertyUtilities.Register("ResourceProvider",
			typeof(CalendarResourceProvider), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnResourceProviderChanged))
			);

		private static void OnResourceProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance.ResolveResourceProvider();
		}

		/// <summary>
		/// Returns or sets an object that provides resources for use by elements in the visual tree.
		/// </summary>
		/// <seealso cref="ResourceProviderProperty"/>
		public CalendarResourceProvider ResourceProvider
		{
			get
			{
				return (CalendarResourceProvider)this.GetValue(CalendarBase.ResourceProviderProperty);
			}
			set
			{
				this.SetValue(CalendarBase.ResourceProviderProperty, value);
			}
		}

		#endregion //ResourceProvider

		#region ResourceProviderResolved

		private static readonly DependencyPropertyKey ResourceProviderResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ResourceProviderResolved",
			typeof(CalendarResourceProvider), typeof(CalendarBase),
			null,
			new PropertyChangedCallback(OnResourceProviderResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="ResourceProviderResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ResourceProviderResolvedProperty = ResourceProviderResolvedPropertyKey.DependencyProperty;

		private static void OnResourceProviderResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance.ResolveButtonStyles();

			CalendarResourceProvider oldProvider = e.OldValue as CalendarResourceProvider;
			CalendarResourceProvider newProvider = e.NewValue as CalendarResourceProvider;
			
			if (oldProvider != null)
			{
				oldProvider.PropertyChanged -= new PropertyChangedEventHandler(instance.OnResourceProviderPropertyChanged);

				instance.NotifyResourcesChanged();
			}

			if (newProvider != null)
				newProvider.PropertyChanged += new PropertyChangedEventHandler(instance.OnResourceProviderPropertyChanged);
		}

		/// <summary>
		/// Returns the resource provider to use.
		/// </summary>
		/// <seealso cref="ResourceProviderResolvedProperty"/>
		public CalendarResourceProvider ResourceProviderResolved
		{
			get
			{
				return (CalendarResourceProvider)this.GetValue(CalendarBase.ResourceProviderResolvedProperty);
			}
			internal set
			{
				this.SetValue(CalendarBase.ResourceProviderResolvedPropertyKey, value);
			}
		}

		#endregion //ResourceProviderResolved

		#region ScrollButtonVisibility

		/// <summary>
		/// Identifies the <see cref="ScrollButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollButtonVisibilityProperty = DependencyPropertyUtilities.Register("ScrollButtonVisibility",
			typeof(Visibility), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnScrollButtonVisibilityChanged))
			);

		private static void OnScrollButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

		}

		/// <summary>
		/// Returns or sets the visibility of the scroll buttons with the <see cref="CalendarItemGroup"/> instances.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ScrollButtonVisibility is used to indicate whethe the <see cref="CalendarItemGroup"/> instances 
		/// within the control should display the previous and next scroll buttons. The default template for the <see cref="CalendarBase"/> 
		/// uses a <see cref="CalendarItemGroupPanel"/> that autogenerates the groups displayed within the control. When scroll buttons 
		/// are to be displayed, it ensures that the scroll previous of the upper left most group and the scroll next of the 
		/// upper right most group will display their scroll buttons. If you retemplate the CalendarBase to directly contain 
		/// CalendarItemGroups then you must selectively bind the <see cref="CalendarItemGroup.ScrollPreviousButtonVisibility"/> 
		/// and <see cref="CalendarItemGroup.ScrollNextButtonVisibility"/> to this property.</p>
		/// </remarks>
		/// <seealso cref="ScrollButtonVisibilityProperty"/>
		/// <seealso cref="CalendarItemGroup.ScrollNextButtonVisibility"/>
		/// <seealso cref="CalendarItemGroup.ScrollPreviousButtonVisibility"/>
		//[Description("Returns or sets of the scroll buttons with the CalendarItemGroup instances.")]
        //[Category("Calendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility ScrollButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CalendarBase.ScrollButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(CalendarBase.ScrollButtonVisibilityProperty, value);
            }
        }

        #endregion //ScrollButtonVisibility

		#region ScrollNextRepeatButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollNextRepeatButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollNextRepeatButtonStyleProperty = DependencyPropertyUtilities.Register("ScrollNextRepeatButtonStyle",
			typeof(Style), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnButtonStyleChanged))
			);

		private static void OnButtonStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance.ResolveButtonStyles();
		}

		/// <summary>
		/// Returns or sets the style for the 'ScrollNext' RepeatButton.
		/// </summary>
		/// <seealso cref="ScrollNextRepeatButtonStyleProperty"/>
		/// <seealso cref="ScrollNextRepeatButtonStyleResolved"/>
		/// <seealso cref="ScrollNextRepeatButtonStyleResolvedProperty"/>
		public Style ScrollNextRepeatButtonStyle
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.ScrollNextRepeatButtonStyleProperty);
			}
			set
			{
				this.SetValue(CalendarBase.ScrollNextRepeatButtonStyleProperty, value);
			}
		}

		#endregion //ScrollNextRepeatButtonStyle

		#region ScrollNextRepeatButtonStyleResolved

		private static readonly DependencyPropertyKey ScrollNextRepeatButtonStyleResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ScrollNextRepeatButtonStyleResolved",
			typeof(Style), typeof(CalendarBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ScrollNextRepeatButtonStyleResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollNextRepeatButtonStyleResolvedProperty = ScrollNextRepeatButtonStyleResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the style to use for the 'ScrollNext' RepeatButton (read-only)
		/// </summary>
		/// <seealso cref="ScrollNextRepeatButtonStyle"/>
		/// <seealso cref="ScrollNextRepeatButtonStyleResolvedProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public Style ScrollNextRepeatButtonStyleResolved
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.ScrollNextRepeatButtonStyleResolvedProperty);
			}
			private set
			{
				this.SetValue(CalendarBase.ScrollNextRepeatButtonStyleResolvedPropertyKey, value);
			}
		}

		#endregion //ScrollNextRepeatButtonStyleResolved

		#region ScrollPreviousRepeatButtonStyle

		/// <summary>
		/// Identifies the <see cref="ScrollPreviousRepeatButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollPreviousRepeatButtonStyleProperty = DependencyPropertyUtilities.Register("ScrollPreviousRepeatButtonStyle",
			typeof(Style), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnButtonStyleChanged))
			);

		/// <summary>
		/// Returns or sets the style for the 'ScrollPrevious' RepeatButton.
		/// </summary>
		/// <seealso cref="ScrollPreviousRepeatButtonStyleProperty"/>
		/// <seealso cref="ScrollPreviousRepeatButtonStyleResolved"/>
		/// <seealso cref="ScrollPreviousRepeatButtonStyleResolvedProperty"/>
		public Style ScrollPreviousRepeatButtonStyle
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.ScrollPreviousRepeatButtonStyleProperty);
			}
			set
			{
				this.SetValue(CalendarBase.ScrollPreviousRepeatButtonStyleProperty, value);
			}
		}

		#endregion //ScrollPreviousRepeatButtonStyle

		#region ScrollPreviousRepeatButtonStyleResolved

		private static readonly DependencyPropertyKey ScrollPreviousRepeatButtonStyleResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ScrollPreviousRepeatButtonStyleResolved",
			typeof(Style), typeof(CalendarBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ScrollPreviousRepeatButtonStyleResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollPreviousRepeatButtonStyleResolvedProperty = ScrollPreviousRepeatButtonStyleResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the style to use for the 'ScrollPrevious' RepeatButton (read-only)
		/// </summary>
		/// <seealso cref="ScrollPreviousRepeatButtonStyle"/>
		/// <seealso cref="ScrollPreviousRepeatButtonStyleResolvedProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public Style ScrollPreviousRepeatButtonStyleResolved
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.ScrollPreviousRepeatButtonStyleResolvedProperty);
			}
			private set
			{
				this.SetValue(CalendarBase.ScrollPreviousRepeatButtonStyleResolvedPropertyKey, value);
			}
		}

		#endregion //ScrollPreviousRepeatButtonStyleResolved

		#region SelectedDate

		/// <summary>
		/// Identifies the <see cref="SelectedDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedDateProperty = DependencyPropertyUtilities.Register("SelectedDate",
			typeof(DateTime?), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSelectedDateChanged), DependencyPropertyUtilities.MetadataOptionFlags.BindsTwoWayByDefault | DependencyPropertyUtilities.MetadataOptionFlags.Journal)
			);


		private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase cal = (CalendarBase)d;

			if (null != cal)
			{
				if (e.NewValue == null)
					cal.SelectedDatesInternal.Clear();
				else
				{
					if (cal.CurrentSelectionTypeResolved == SelectionType.None)
						throw new InvalidOperationException(GetString("LE_MaxSelectedDatesExceeded", 1, 0));

					DateTime selectedDate = (DateTime)e.NewValue;

					// AS 10/15/09 TFS23545
					selectedDate = selectedDate.Date;

					DateCollection selectedDates = cal.SelectedDatesInternal;

					// if there is no selection or if this isn't the first item
					// then the selection is changing in which case we'll clear 
					// the collection and select just this item
					if (selectedDates.Count == 0 || selectedDates[0] != selectedDate)
					{
						selectedDates.Reinitialize(new DateTime[] { selectedDate });
					}
				}

				// AS 9/4/08
				// The ActiveDate takes the value of the selected date into account
				// if there is no explicit active date.
				//
				cal.ResolveActiveDate();
			}
		}

		/// <summary>
		/// Returns or sets the date of the <see cref="CalendarItem"/> that should be selected or null if no item is selected.
		/// </summary>
		/// <remarks>
		/// <p class="body">The SelectedDate returns the first date that is selected within the control and is primarily used 
		/// when using a <see cref="SelectionType"/> of <b>Single</b>. If you are using a SelectionType that allows multiple 
		/// dates to be selected, then you can use the <see cref="SelectedDates"/> to get a complete list of the dates that 
		/// are selected. The SelectedDates will include the SelectedDate.</p>
		/// <p class="note"><b>Note:</b> When the <see cref="XamCalendar.MinCalendarMode"/> is set to a value other than days, selecting 
		/// a <see cref="CalendarItem"/> will only add 1 entry for each selected item. It will not add each date in the item's 
		/// range into the SelectedDates.</p>
		/// <p class="note"><b>Note:</b> When in navigation mode (i.e. the <see cref="CalendarBase.CurrentMode"/> is 
		/// not equal to the <see cref="XamCalendar.MinCalendarMode"/>), the selection cannot be changed via the ui. Instead 
		/// using the mouse will change the active item and the <see cref="CalendarItem.IsSelected"/> state will be based on which 
		/// item is active (<see cref="CalendarItem.IsActive"/>).</p>
		/// </remarks>
		/// <seealso cref="SelectedDateProperty"/>
		/// <seealso cref="SelectedDates"/>
		/// <seealso cref="SelectionType"/>
		/// <seealso cref="SelectedDatesChanged"/>
		//[Description("Returns or sets the date of the item that should be selected or null if no item is selected.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]



		public DateTime? SelectedDate
		{
			get
			{
				return (DateTime?)this.GetValue(CalendarBase.SelectedDateProperty);
			}
			set
			{
				this.SetValue(CalendarBase.SelectedDateProperty, value);
			}
		}

        #endregion //SelectedDate

		#region SelectedDates

		/// <summary>
		/// Returns a collection of <see cref="DateTime"/> instances that represents the currently selected dates of the items in the <see cref="XamCalendar.MinCalendarMode"/>.
		/// </summary>
        /// <remarks>
        /// <p class="body">The SelectedDates is a collection of DateTime instances that represent the <see cref="CalendarItem"/> 
        /// instances that should be selected or have been selected by the end user. When the <see cref="SelectionType"/> is set 
        /// to a value that allows multiple selection such as <b>Range</b> or <b>Extended</b>, the control will allow selection of 
        /// one more CalendarItems. A single date for each item will be added to the SelectedDates. The <see cref="SelectedDate"/> 
        /// property can be used to access the first item.</p>
		/// <p class="note"><b>Note:</b> When the <see cref="XamCalendar.MinCalendarMode"/> is set to a value other than days, selecting 
        /// a <see cref="CalendarItem"/> will only add 1 entry for each selected item. It will not add each date in the item's 
        /// range into the SelectedDates.</p>
        /// </remarks>
        /// <seealso cref="SelectedDate"/>
		/// <seealso cref="XamCalendar.MaxSelectedDates"/>
        /// <seealso cref="SelectionType"/>
		/// <seealso cref="XamCalendar.MinCalendarMode"/>
        /// <seealso cref="SelectedDatesChanged"/>

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

        [Bindable(true)]
        public DateCollection SelectedDates
		{
			get { return this._selectedDates; }
		}
		#endregion //SelectedDates

		#region TodayButtonCaption

		private static readonly DependencyPropertyKey TodayButtonCaptionPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TodayButtonCaption",
			typeof(string), typeof(CalendarBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="TodayButtonCaption"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayButtonCaptionProperty = TodayButtonCaptionPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the caption to use for the 'Today' Button (read-only)
		/// </summary>
		/// <seealso cref="TodayButtonCaptionProperty"/>
		public string TodayButtonCaption
		{
			get
			{
				return (string)this.GetValue(CalendarBase.TodayButtonCaptionProperty);
			}
			private set
			{
				this.SetValue(CalendarBase.TodayButtonCaptionPropertyKey, value);
			}
		}

		#endregion //TodayButtonCaption

		#region TodayButtonStyle

		/// <summary>
		/// Identifies the <see cref="TodayButtonStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayButtonStyleProperty = DependencyPropertyUtilities.Register("TodayButtonStyle",
			typeof(Style), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnButtonStyleChanged))
			);

		/// <summary>
		/// Returns or sets the style for the 'Today' Button.
		/// </summary>
		/// <seealso cref="TodayButtonStyleProperty"/>
		/// <seealso cref="TodayButtonStyleResolved"/>
		/// <seealso cref="TodayButtonStyleResolvedProperty"/>
		public Style TodayButtonStyle
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.TodayButtonStyleProperty);
			}
			set
			{
				this.SetValue(CalendarBase.TodayButtonStyleProperty, value);
			}
		}

		#endregion //TodayButtonStyle

		#region TodayButtonStyleResolved

		private static readonly DependencyPropertyKey TodayButtonStyleResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TodayButtonStyleResolved",
			typeof(Style), typeof(CalendarBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="TodayButtonStyleResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayButtonStyleResolvedProperty = TodayButtonStyleResolvedPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the style to use for the 'Today' Button (read-only)
		/// </summary>
		/// <seealso cref="TodayButtonStyle"/>
		/// <seealso cref="TodayButtonStyleResolvedProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public Style TodayButtonStyleResolved
		{
			get
			{
				return (Style)this.GetValue(CalendarBase.TodayButtonStyleResolvedProperty);
			}
			private set
			{
				this.SetValue(CalendarBase.TodayButtonStyleResolvedPropertyKey, value);
			}
		}

		#endregion //TodayButtonStyleResolved

		#region WeekNumberVisibility

		/// <summary>
		/// Identifies the <see cref="WeekNumberVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekNumberVisibilityProperty = DependencyPropertyUtilities.Register("WeekNumberVisibility",
			typeof(Visibility), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnWeekNumberVisibilityChanged))
			);

		private static void OnWeekNumberVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

			instance.NotifyGroupChange(CalendarChange.WeekNumberVisibility);
		}

		/// <summary>
		/// Returns or sets a boolean that indicates whether week numbers should be displayed.
		/// </summary>
         /// <seealso cref="CalendarItemArea.WeekNumberVisibility"/>
        //[Description("Returns or sets a value that indicates whether week numbers should be displayed.")]
        //[Category("Calendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility WeekNumberVisibility
		{
            get { return (Visibility)this.GetValue(WeekNumberVisibilityProperty); }
			set { this.SetValue(WeekNumberVisibilityProperty, value); }
		}

		#endregion //WeekNumberVisibility

    	#region Today

		private static readonly DependencyPropertyKey TodayPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Today",
			typeof(DateTime), typeof(CalendarBase), DateTime.Today, null );

		/// <summary>
		/// Identifies the read-only <see cref="Today"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayProperty = TodayPropertyKey.DependencyProperty;
			
		/// <seealso cref="TodayProperty"/>
		/// <summary>
		/// Returns a DateTime that represents today's date.
		/// </summary>
		/// <seealso cref="TodayProperty"/>
		[Bindable(true)]
		[ReadOnly(true)]
		public DateTime Today
		{
			get
			{
				return (DateTime)this.GetValue(CalendarBase.TodayProperty);
			}
			internal set
			{
				this.SetValue(CalendarBase.TodayPropertyKey, value);
			}
		}

		#endregion //Today

		#endregion //Public

		#region Protected

		#region AllowableDateRange

		/// <summary>
		/// Set by dervied classes to constrain the min/max date range
		/// </summary>
		/// <remarks><para class="body">When this is changed the <see cref="MinDateResolved"/> and <see cref="MaxDateResolved"/> properties are set accoringly.</para></remarks>
		/// <seealso cref="MinDateResolved"/>
		/// <seealso cref="MaxDateResolved"/>
		protected DateRange AllowableDateRange
		{
			get { return this._allowableDateRange; }
			set
			{
				if (value != _allowableDateRange)
				{
					this._allowableDateRange = value;

					DateTime minDate;
					DateTime maxDate;
					if (this._allowableDateRange.Start <= this._allowableDateRange.End)
					{
						minDate = this._allowableDateRange.Start;
						// JJD 1/11/12 - TFS96836 - set max date as well
						maxDate = this._allowableDateRange.End;

					}
					else
					{
						minDate = this._allowableDateRange.End;
						// JJD 1/11/12 - TFS96836 - set max date as well
						maxDate = this._allowableDateRange.Start;
					}

					// JJD 1/11/12 - TFS96836 - Strip off time portions
					//this.SetValue(MinDateResolvedPropertyKey, minDate);
					//this.SetValue(MaxDateResolvedPropertyKey, this._allowableDateRange.End);
					this.SetValue(MinDateResolvedPropertyKey, minDate.Date);
					this.SetValue(MaxDateResolvedPropertyKey, maxDate.Date);

					this.NotifyGroupChange(CalendarChange.AllowableDatesChanged);

					// JJD /8/11 - TFS67545 - Asynchronously make sure the the reference date is within bounds
					if (this._isInitialized )
						this.Dispatcher.BeginInvoke(new CalendarUtilities.MethodInvoker(this.ResolveReferenceDate));
				}
			}
		}

		#endregion //AllowableDateRange	

		#region FirstDayOfWeekInternal

		/// <summary>
		/// Set by derived classes to change the first day of the week
		/// </summary>
		protected internal DayOfWeek? FirstDayOfWeekInternal
		{
			get { return _firstDayOfWeekResolved; }
			set
			{
				if (value != _firstDayOfWeekResolved)
				{
					_firstDayOfWeekResolved = value;
					_calendarManager.FirstDayOfWeek = value;

					if (_isInitialized)
					{
						using (new CalendarItemGroup.GroupInitializationHelper(_groups))
						{
							this.NotifyGroupChange(CalendarChange.FirstDayOfWeekChanged);
							this.InitializeDaysOfWeek();
						}
					}
				}
			}
		}

		#endregion //FirstDayOfWeekInternal

		#region WeekRuleInternal

		/// <summary>
		/// Set by derived classes to change the week rule
		/// </summary>
		protected internal CalendarWeekRule? WeekRuleInternal
		{
			get { return _weekRuleResolved; }
			set
			{
				if (value != _weekRuleResolved)
				{
					_weekRuleResolved = value;
					_calendarManager.WeekRule = value;

					this.NotifyGroupChange(CalendarChange.WeekRuleChanged);
				}
			}
		}

		#endregion //WeekRuleInternal

		#endregion //Protected

		#region Internal

		#region CalendarManager

		internal CalendarManager CalendarManager
		{
			get { return this._calendarManager; }
		}
		#endregion //CalendarManager

		#region CurrentSelectionType/Resolved

		internal virtual SelectionType CurrentSelectionType
		{
			get
			{
				return SelectionType.Extended;
			}
		}
		internal SelectionType CurrentSelectionTypeResolved
		{
			get
			{
				if (this.IsMinCalendarMode)
					return this.CurrentSelectionType;

				return SelectionType.Single;
			}
		}

		#endregion //CurrentSelectionType/Resolved

		#region DaysOfWeek



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal ReadOnlyObservableCollection<DayOfWeek> DaysOfWeek
		{
			get
			{
				return this._daysOfWeek;
			}
		}
		#endregion //DaysOfWeek

		// JJD 07/24/12 - TFS113395 - added
		#region DisableAnimations

		// JJD 07/24/12 - TFS113395
		// Added ability to temporarily disable animations.
		// This is used by the XamDateTimeInput to prevent animating
		// dates while its dropdown calendar is being opened.
		internal bool DisableAnimations { get; set; }

		#endregion //DisableAnimations	
    
		#region DisabledDatesInternal
		
		internal CalendarDateRangeCollection DisabledDatesInternal
		{
			get { return this._disabledDates; }
		}
		#endregion //DisabledDatesInternal

		#region DisabledDaysOfWeekInternal

		internal DayOfWeekFlags DisabledDaysOfWeekInternal
		{
			get
			{
				return _disabledDaysOfWeekInternal;
			}
			set
			{
				if (value != _disabledDaysOfWeekInternal)
				{
					_disabledDaysOfWeekInternal = value;
					using (new CalendarItemGroup.GroupInitializationHelper(_groups))
					{
						// if we're hiding days of the week then we need to rebuild the days of the week
						if (Visibility.Visible != this.DisabledDaysOfWeekVisibilityInternal)
						{
							this.CalendarManager.HiddenDays = this.DisabledDaysOfWeekInternal;
							this.InitializeDaysOfWeek();
						}

						this.NotifyGroupChange(CalendarChange.DisabledDatesChanged);
					}
				}
			}
		}

		#endregion //DisabledDaysOfWeekInternal	

		#region DisabledDaysOfWeekVisibilityInternal

		internal Visibility DisabledDaysOfWeekVisibilityInternal
		{
			get
			{
				return _disabledDaysOfWeekVisibilityInternal;
			}
			set
			{
				if (value != _disabledDaysOfWeekVisibilityInternal)
				{
					_disabledDaysOfWeekVisibilityInternal = value;

					if (

						_disabledDaysOfWeekInternal != DayOfWeekFlags.None)
					{
						DayOfWeekFlags hiddenDays = value != Visibility.Visible ? _disabledDaysOfWeekInternal : DayOfWeekFlags.None;
						_calendarManager.HiddenDays = hiddenDays;

						// if the days of the week haven't changed then we need to 
						// tell the group to reset the enabled state
						if (false == this.InitializeDaysOfWeek())
						{
							this.NotifyGroupChange(CalendarChange.DisabledDatesChanged);
						}
					}
				}
			}
		}

		#endregion //DisabledDaysOfWeekVisibilityInternal

		#region DefaultResourceProvider

		internal CalendarResourceProvider DefaultResourceProvider
		{
			get { return this._defaultResourceProvider; }
			set
			{
				if (value != _defaultResourceProvider)
				{
					_defaultResourceProvider = value;
					this.ResolveResourceProvider();
				}
			}
		}

		#endregion //DefaultResourceProvider	

		#region MaxSelectedDatesInternal
    
           internal int MaxSelectedDatesInternal
        {
            get
            {
				return _maxSelectedDatesInternal;
			}
			set
			{ 
				if ( value != _maxSelectedDatesInternal )
				{
					_maxSelectedDatesInternal = value;

					DateCollection selectedDates = this.SelectedDatesInternal;

					if (_isInitialized && _maxSelectedDatesInternal > 0 && _maxSelectedDatesInternal < selectedDates.Count)
					{
						// reduce selection and raise event
						DateTime[] dates = new DateTime[selectedDates.Count];
						selectedDates.CopyTo(dates, 0);
						Array.Resize(ref dates, _maxSelectedDatesInternal);
						selectedDates.Reinitialize(dates);
					}
				}
			}
		}

   		#endregion //MaxSelectedDatesInternal	
    
		#region MaxSelectedDatesResolved
    
        internal int MaxSelectedDatesResolved
        {
            get
            {
                SelectionType selectionType = this.CurrentSelectionTypeResolved;

                if (SelectionStrategyBase.IsMultiSelectStrategy(selectionType))
                {
                    int max = this.MaxSelectedDatesInternal;

                    if (0 == max)
                        max = int.MaxValue;

                    return max;
                }

                if (SelectionStrategyBase.IsSingleSelectStrategy(selectionType))
                    return 1;

                return 0;
            }
        }

   		#endregion //MaxSelectedDatesResolved	
        
        #region IsNavigationMode
        internal bool IsNavigationMode
        {
            get { return this.IsMinCalendarMode == false; }
        } 
        #endregion //IsNavigationMode

        #region IsMinCalendarMode
        internal bool IsMinCalendarMode
        {
            get { return this.CurrentMode == this.MinCalendarModeResolved; }
        } 
        #endregion //IsMinCalendarMode
		
		#region MaxCalendarMode
        internal CalendarZoomMode MaxCalendarMode
        {
            get { return CalendarZoomMode.Centuries; }
        } 
        #endregion //MaxCalendarMode

		#region MeasureDate

		internal static readonly DependencyProperty MeasureDateProperty = DependencyPropertyUtilities.Register("MeasureDate",
			typeof(DateTime), typeof(CalendarBase),
			DependencyPropertyUtilities.CreateMetadata(new DateTime(2008, 9, 1), new PropertyChangedCallback(OnMeasureDateChanged))
			);

		private static void OnMeasureDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase instance = (CalendarBase)d;

		}

		private void ResolveMeasureDate()
        {
			if (!_isInitialized)
				return;

			DateTime newDate = (DateTime)this.GetValue(MeasureDateProperty);

            // ensure its valid for the current calendar
            newDate = this._calendarManager.CoerceMinMaxDate(newDate);

            this.SetValue(MeasureDateProperty, newDate);
        }

        #endregion //MeasureDate

		#region MouseOverItem



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


		#endregion //MouseOverItem

		#region Calendar

		internal static readonly DependencyPropertyKey CalendarPropertyKey = DependencyPropertyUtilities.RegisterAttachedReadOnly("Calendar",
			typeof(CalendarBase), typeof(CalendarBase), null, OnCalendarChanged );

		private static void OnCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ICalendarElement navElement = d as ICalendarElement;

			if (navElement != null)
				navElement.OnCalendarChanged(e.NewValue as CalendarBase, e.OldValue as CalendarBase);
		} 

		/// <summary>
		/// Identifies the read-only Calendar attached dependency property
		/// </summary>
		/// <seealso cref="GetCalendar"/>
		public static readonly DependencyProperty CalendarProperty = CalendarPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the containing <see cref="CalendarBase"/>
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="CalendarProperty"/>
		public static CalendarBase GetCalendar(DependencyObject d)
		{
			return (CalendarBase)d.GetValue(CalendarBase.CalendarProperty);
		}

		internal static void SetCalendar(DependencyObject d, CalendarBase value)
		{
			d.SetValue(CalendarBase.CalendarPropertyKey, value);
		}

        #endregion //Calendar

		#region RootPanel

		internal Panel RootPanel { get { return _rootPanel; } }

		#endregion //RootPanel	
		
		#region SelectedDatesInternal

		// JJD 1/25/11
		// While the mouse is down (during a selection drag operation we don't want to update the real
		// selected dates collection so we need to clone it and use a temporary collection in its place.
		// This is to prevent the selected dates from changing until the mouse is released
		internal DateCollection SelectedDatesInternal
		{
			get { return this._mouseDownSnapshot != null ? _mouseDownSnapshot.SelectedDates : this._selectedDates; }
		}

		#endregion //SelectedDatesInternal	
        
        #region SelectionHost
        internal ISelectionHost SelectionHost
        {
            get { return this; }
        }
        #endregion //SelectionHost

		#region SupportsWeekSelectionMode

		internal virtual bool SupportsWeekSelectionMode { get { return false; } }

		#endregion //SupportsWeekSelectionMode	
    
		#region WorkDaysInternal

		internal DayOfWeekFlags WorkDaysInternal
		{
			get { return _workDaysInternal; }
			set
			{
				if (_workDaysInternal != value)
				{
					_workDaysInternal = value;
					
					this.NotifyGroupChange(CalendarChange.WorkdaysChanged);
				}
			}
		}

		#endregion //WorkDaysInternal	
    
		#endregion //Internal

		#region Private

		#region CurrentState

		private CalendarStates CurrentState
		{
			get
			{
				CalendarStates state = 0;

				if (this._groups.Count > 0)
				{
					// AS 10/14/09 FR11859
					// We need to continue scrolling groups if the min/max dates are 
					// in leading/trailing groups.
					//
					//CalendarItemGroup first = this.FirstGroup;
					//CalendarItemGroup last = this.LastGroup;
					CalendarItemGroup first = this.GetFirstGroup(true, true);
					CalendarItemGroup last = this.GetLastGroup(true, true);

                    if (null != first && first.FirstDateOfGroup <= this.MinDateResolved)
						state |= CalendarStates.MinDateInView;

					if (null != last && last.LastDateOfGroup >= this.MaxDateResolved)
						state |= CalendarStates.MaxDateInView;

					if (CanActivate(DateTime.Today))
						state |= CalendarStates.TodayIsEnabled;

					if (this.ActiveDate != null)
						state |= CalendarStates.ActiveDate;

					if (this.CurrentMode > this.MinCalendarModeResolved)
						state |= CalendarStates.CanZoomInCalendarMode;

					
					if (this.CurrentMode < this.MaxCalendarMode
						// AS 10/14/09 FR11859
						//&& (false == this.IsDateInView(this.MinDate, true) || false == this.IsDateInView(this.MaxDate, true)))
                        && (false == this.IsDateInView(this.MinDateResolved, true, false) || false == this.IsDateInView(this.MaxDateResolved, true, false)))
						state |= CalendarStates.CanZoomOutCalendarMode;

                    switch (this.CurrentMode)
                    {
                        case CalendarZoomMode.Days:
                            state |= CalendarStates.CalendarModeDays;
                            break;
                        case CalendarZoomMode.Decades:
                            state |= CalendarStates.CalendarModeDecades;
                            break;
                        case CalendarZoomMode.Centuries:
                            state |= CalendarStates.CalendarModeCenturies;
                            break;
                        case CalendarZoomMode.Months:
                            state |= CalendarStates.CalendarModeMonths;
                            break;
                        case CalendarZoomMode.Years:
                            state |= CalendarStates.CalendarModeYears;
                            break;
                    }

                    if (this.IsMinCalendarMode)
                        state |= CalendarStates.MinCalendarMode;
				}

				// AS 1/5/10 TFS23198
				if (this.FlowDirection == FlowDirection.RightToLeft)
					state |= CalendarStates.RightToLeft;

				return state;
			}
		}
		#endregion //CurrentState

        #region FirstGroup

        internal CalendarItemGroup FirstGroup
        {
            get
            {
				// AS 10/14/09 FR11859
				// Moved to a helper method that can be used to ignore 
				// leading/trailing groups. This property will continue 
				// to function as before.
				//
				//for (int i = 0, count = this._groups.Count; i < count; i++)
				//{
				//    CalendarItemGroup group = this._groups[i];
				//
				//    if (group.Items.Count > 0)
				//        return group;
				//}
				//
				//return null;
				return GetFirstGroup(true, false);
            }
        }
        #endregion //FirstGroup

		#region LastGroup

        internal CalendarItemGroup LastGroup
		{
            get 
            {
				// AS 10/14/09 FR11859
				// Moved to a helper method that can be used to ignore 
				// leading/trailing groups. This property will continue 
				// to function as before.
				//
				//for (int i = this._groups.Count - 1; i >= 0; i--)
				//{
				//    CalendarItemGroup group = this._groups[i];
				//
				//    if (group.Items.Count > 0)
				//        return group;
				//}
				//
				//return null;
				return GetLastGroup(true, false);
            }
        }
        #endregion //LastGroup

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Public

        #region BringDateIntoView

        /// <summary>
		/// Scrolls the specified <see cref="DateTime"/> into view ignoring leading and trailing dates.
		/// </summary>
		/// <param name="date">The <see cref="DateTime"/> representing the date that should be brought into view.</param>
		public void BringDateIntoView(DateTime date)
		{
            this.BringDateIntoView(date, null);
		}

		internal void BringDateIntoView(DateTime date, CalendarItemGroup group)
		{
            int index = group == null ? -1 : this._groups.IndexOf(group);
            this.BringDateIntoView(date, index, true);
		}

		internal void BringDateIntoView(DateTime date, int groupIndex, bool ignoreLeadingAndTrailingDates)
		{
//            Debug.Assert(this._shouldBringActiveDateIntoView == null);

            if (null != this._shouldBringActiveDateIntoView)
                this._shouldBringActiveDateIntoView = false;

            // make sure the calendar can support it
            date = this.ConstrainBetweenMinMaxDate(date);

            if (this._groups.Count == 0)
				// JJD 1/12/12 - TFS96805
				// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
                //this.ReferenceDate = date;
                this.ResolveReferenceDate( date );
            else
            {
                if (groupIndex < 0)
                {
                    // if its already in view then exit
					if (this.IsDateInView(date, ignoreLeadingAndTrailingDates))
                        return;

                    if (this._groups.Count == 1)
						// JJD 1/12/12 - TFS96805
						// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
						//this.ReferenceDate = date;
						this.ResolveReferenceDate( date );
                    else
                    {
                        CalendarZoomMode mode = this.CurrentMode;

                        // find a date X number of months before this date to
                        // use to initialize the first month
                        DateTime groupStartDate = this._calendarManager.GetGroupStartDate(date, mode);
						// AS 10/14/09 FR11859
						//CalendarItemGroup referenceGroup = groupStartDate < this.ReferenceDate
						//    ? this._groups[0] : this._groups[this._groups.Count - 1];
						CalendarItemGroup referenceGroup = groupStartDate < this.ReferenceDate
							? this.GetFirstGroup(false) 
							: this.GetLastGroup(false);

                        int referenceGroupOffset = referenceGroup.ReferenceGroupOffset;

                        // now find out what the first date would be for the first group so that this
                        // date will be in the last group
                        groupStartDate = this._calendarManager.AddGroupOffset(groupStartDate, -referenceGroupOffset, mode, true);

						// JJD 1/12/12 - TFS96805
						// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
						//this.ReferenceDate = groupStartDate;
						this.ResolveReferenceDate(groupStartDate);
                    }
                }
                else
                {
                    // make sure the group index is within range
					// AS 10/14/09 FR11859
					//groupIndex = Math.Min(0, Math.Max(this._groups.Count - 1, groupIndex));
                    groupIndex = Math.Min(_groups.IndexOf(this.GetFirstGroup(false)), 
						Math.Max(_groups.IndexOf(this.GetLastGroup(false)), groupIndex));

                    CalendarItemGroup group = this._groups[groupIndex];
                    DateTime? groupStart = null;
                    DateTime? groupEnd = null;

                    if (group.FirstDateOfGroup != null)
                        groupStart = ignoreLeadingAndTrailingDates ? group.FirstDateOfGroup.Value : group.FirstDate;

                    if (group.LastDateOfGroup != null)
                        groupEnd = ignoreLeadingAndTrailingDates ? group.LastDateOfGroup.Value : group.LastDate;

                    // if its not in this month
                    if (date < groupStart || groupEnd < date || groupStart == null || groupEnd == null)
                    {
                        CalendarZoomMode mode = this.CurrentMode;
                        DateTime dateToInitialize = this._calendarManager.GetGroupStartDate(date, mode);
                        dateToInitialize = this._calendarManager.AddGroupOffset(date, -groupIndex, mode, true);

						// JJD 1/12/12 - TFS96805
						// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
						//this.ReferenceDate = dateToInitialize;
						this.ResolveReferenceDate(dateToInitialize);
                    }
                }
            }
		}
		#endregion //BringDateIntoView

		#region ExecuteCommand

		/// <summary>
		/// Executes the specified <see cref="CalendarCommandType"/>.
		/// </summary>
		/// <param name="command">The Command to execute.</param>
		/// <param name="parameter">An optional parameter.</param>
		/// <param name="sourceElement">The source of the command</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="CalendarCommandType"/>
		public bool ExecuteCommand(CalendarCommandType command, object parameter, FrameworkElement sourceElement)
		{
			return this.ExecuteCommandImpl(command, parameter, sourceElement);
		}

		private bool ExecuteCommandImpl(CalendarCommandType commandType, object parameter, FrameworkElement sourceElement)
		{
			// Make sure the minimal control state exists to execute the command.
			if (this.CanExecuteCommand(commandType, sourceElement) == false)
			    return false;

			bool shiftKeyDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
			bool ctlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

			// =========================================================================================
			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;

            #region Scroll(Next|Previous)Group(s)
			switch (commandType)
			{
				case CalendarCommandType.ScrollNextGroup:
				case CalendarCommandType.ScrollPreviousGroup:
					{
						int offset = commandType == CalendarCommandType.ScrollNextGroup ? 1 : -1;
						handled = this.ScrollGroups(offset);
						break;
					}
				case CalendarCommandType.ToggleActiveDateSelection:
					handled = ToggleActiveDateSelection(shiftKeyDown, ctlKeyDown);
					break;
				case CalendarCommandType.ScrollNextGroups:
				case CalendarCommandType.ScrollPreviousGroups:
					{
						int scrollCount = parameter is int
							? (int)parameter
							// AS 10/14/09 FR11859
							//: (this._groups[this._groups.Count - 1].ReferenceGroupOffset - this._groups[0].ReferenceGroupOffset) + 1;
							: (this.GetLastGroup(false).ReferenceGroupOffset
								- this.GetFirstGroup(false).ReferenceGroupOffset) + 1;

						if (commandType == CalendarCommandType.ScrollPreviousGroups)
							scrollCount *= -1;

						handled = this.ScrollGroups(scrollCount);
						break;
					}
			#endregion //Scroll(Next|Previous)Group(s)
				case CalendarCommandType.ScrollToDate:
					{
						#region ScrollToDate
						if (parameter is DateTime)
						{
							DateTime date = (DateTime)parameter;
							CalendarItemGroup group = null;

							// see if there is a group within the path and scroll the date into
							// view in that group
							if (sourceElement is DependencyObject)
							{
								if (sourceElement is CalendarItemGroup)
									group = (CalendarItemGroup)sourceElement;
								else
									group = (CalendarItemGroup)PresentationUtilities.GetVisualAncestor<CalendarItemGroup>(sourceElement, null);
							}
							this.BringDateIntoView(date, group);

							handled = true;
						}
						break;
						#endregion //ScrollToDate
					}
				case CalendarCommandType.ActivateDate:
					{
						#region ActivateDate

						// now get the date into which we will "zoom"
						DateTime? newActiveDate = null;

						if (parameter is DateTime)
							newActiveDate = (DateTime)parameter;
						else
						{
							CalendarItem item = parameter as CalendarItem;

							// if the command parameter isn't an item but the original source was...
							if (item == null && sourceElement is DependencyObject)
							{
								if (sourceElement is CalendarItem)
									item = (CalendarItem)sourceElement;
								else
									item = PresentationUtilities.GetVisualAncestor<CalendarItem>(sourceElement, null);
							}

							if (item != null)
								newActiveDate = item.StartDate;
						}

						
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

						handled = this.ActivateDate(newActiveDate, false);

						break;
						#endregion //ActivateDate
					}
				case CalendarCommandType.ActivateSelectedDate:
					{
						#region ActivateSelectedDate

						this.ActiveDate = this.SelectedDate;
						handled = true;
						break;

						#endregion //ActivateSelectedDate
					}
				case CalendarCommandType.Today:
					{
						handled = this.ActivateDate(DateTime.Today);

						// AS 3/24/10 TFS28062
						// Note I'm always doing this to be consistent with outlook's behavior.
						// 
						this.RaiseSelectionCommitted(new EventArgs());
						break;
					}

				#region Next/Previous/First/Last
				case CalendarCommandType.PreviousItem:
				case CalendarCommandType.NextItem:
					{
						// find the next/previous activatable date

						// JJD 11/15/11 - TFS74059
						//handled = this.ActivateDate(this.GetActivatableDate(this.ActiveDate, commandType == CalendarCommandType.PreviousItem, false));
						DateTime? dateToActivate = this.GetActivatableDate(this.ActiveDate, commandType == CalendarCommandType.PreviousItem, false);

						// JJD 11/15/11 - TFS74059
						// If SupportsWeekSelectionMode returns true (i.e. a XamDateNavigator) then
 						// we want to keep going forward or backward until we find a date to activate that is
						// not currently selected.
						if (dateToActivate.HasValue &&
							this.SupportsWeekSelectionMode )
						{
							DateCollection selectedDates = this.SelectedDates;

							while (dateToActivate.HasValue && selectedDates.ContainsDate(dateToActivate.Value))
								dateToActivate = this.GetActivatableDate(dateToActivate, commandType == CalendarCommandType.PreviousItem, false);
						}

						handled = this.ActivateDate(dateToActivate);
						break;
					}

				case CalendarCommandType.PreviousItemRow:
				case CalendarCommandType.NextItemRow:
					{
						if (null != this.ActiveDate)
						{
							handled = this.NavigateRow(this.ActiveDate.Value, commandType == CalendarCommandType.NextItemRow);
						}
						break;
					}

				case CalendarCommandType.PreviousGroup:
				case CalendarCommandType.NextGroup:
					{
						if (null != this.ActiveDate)
							handled = this.NavigateGroup(this.ActiveDate.Value, commandType == CalendarCommandType.NextGroup);
						break;
					}

				#region (First|Last)ItemOfGroup
				case CalendarCommandType.FirstItemOfGroup:
					{
						if (null != this.ActiveDate)
						{
							// start with the first date in a group and work foward
							DateTime? newActiveDate = this.GetActivatableDate(this._calendarManager.GetGroupStartDate(this.ActiveDate.Value, this.CurrentMode), false, true);

							handled = this.ActivateDate(newActiveDate);
						}
						break;
					}
				case CalendarCommandType.LastItemOfGroup:
					{
						if (null != this.ActiveDate)
						{
							// start with the last date in a group and work backward
							DateTime? newActiveDate = this.GetActivatableDate(this._calendarManager.GetGroupEndDate(this.ActiveDate.Value, this.CurrentMode), true, true);

							handled = this.ActivateDate(newActiveDate);
						}
						break;
					}
				#endregion //(First|Last)ItemOfGroup

				case CalendarCommandType.FirstItemOfFirstGroup:
					{
						// AS 10/14/09 FR11859
						//CalendarItemGroup group = this.FirstGroup;
						CalendarItemGroup group = this.GetFirstGroup(false);

						if (null != group)
						{
							DateTime? newActiveDate = this.GetActivatableDate(group.FirstDateOfGroup, false, true);
							handled = this.ActivateDate(newActiveDate);
						}
						break;
					}
				case CalendarCommandType.LastItemOfLastGroup:
					{
						// AS 10/14/09 FR11859
						//CalendarItemGroup group = this.LastGroup;
						CalendarItemGroup group = this.GetLastGroup(false);

						if (null != group)
						{
							DateTime? newActiveDate = this.GetActivatableDate(group.LastDateOfGroup, true, true);
							handled = this.ActivateDate(newActiveDate);
						}
						break;
					}
				#endregion //Next/Previous/First/Last

				case CalendarCommandType.ZoomInCalendarMode:
				case CalendarCommandType.ZoomOutCalendarMode:
					{
						#region (Dec|Inc)reaseCalendarMode

						this.ChangeCalendarMode(commandType == CalendarCommandType.ZoomOutCalendarMode, parameter, sourceElement);

						handled = true;

						break;
						#endregion //(Dec|Inc)reaseCalendarMode
					}
			}

			// =========================================================================================

			// AS 10/15/09 TFS23867
			// We don't want to raise ExecutedCommand since we didn't process it but if 
			// the command was invoked as a result of a navigational keyboard key then 
			// we want to mark the event handled so wpf doesn't navigate outside of 
			// the control.
			//if (parameter == CalendarCommands.NavigationKeyParameter)
			//    handled = true;

			return handled;
		}

		#endregion //ExecuteCommandImpl

		#endregion //Public

		#region Protected

		#region GetDayOfWeekHeader

		/// <summary>
		/// Returns the day of week caption
		/// </summary>
		/// <param name="dayofWeek">The day of the week</param>
		/// <returns>A string representing this day of week.</returns>
		internal protected virtual string GetDayOfWeekHeader(DayOfWeek dayofWeek)
		{
			DayOfWeekHeaderFormat format = this.DayOfWeekHeaderFormat;

			return this._calendarManager.GetDayOfWeekCaption(format, dayofWeek);
		}

		#endregion //GetDayOfWeekHeader

		#region InvalidateDisplay

		/// <summary>
		/// Will cause the display to refresh. Called when a property that affects the display has changed
		/// </summary>
		internal protected virtual void InvalidateDisplay()
		{
			if (this.CurrentMode == CalendarZoomMode.Days)
				this.ClearPreferredNavInfo();

			this.NotifyGroupChange(CalendarChange.DaysOfWeekChanged);
			this.NotifyGroupChange(CalendarChange.Resources);
		}

		#endregion //InvalidateDisplay

		#region InvalidateIsHighlighted

		/// <summary>
		/// Will cause all days within a month to re-initialize their IsHighlighted state. 
		/// </summary>
		/// <param name="dayinMonth">Any day of the month in question.</param>
		protected void InvalidateIsHighlighted(DateTime dayinMonth)
		{
			if (this.CurrentMode == CalendarZoomMode.Days)
			{
				CalendarItemGroup group = this.GetGroup(dayinMonth);

				if (group != null)
				{
					ICalendarItemArea area = group.ItemArea;

					if (area != null)
					{
						foreach (CalendarItem sibling in area.Items)
						{
							CalendarDay day = sibling as CalendarDay;

							if (day != null)
								day.InitializeIsHighlighted();
						}
					}
				}
			}
		}

		#endregion //InvalidateIsHighlighted	
    
		#region SetCalendarInfo

		/// <summary>
		/// Called by a derived class to supply calendar information
		/// </summary>
		/// <param name="calendar">The calendar to use for date manipulations.</param>
		/// <param name="dateTimeFormat">The DateTimeFormatInfo used for formatting dates.</param>
		protected void SetCalendarInfo(System.Globalization.Calendar calendar, DateTimeFormatInfo dateTimeFormat)
		{
			if (_suppliedCalendar			!= calendar || 
				_suppliedDateTimeFormat		!= dateTimeFormat)
			{
				_suppliedCalendar = calendar;
				_suppliedDateTimeFormat = dateTimeFormat;


				this.OnCalendarInfoChanged(this.Language);



			}
		}

		#endregion //SetCalendarInfo

		#region ShouldHighlightDay

		/// <summary>
		/// Returns true if the day element should be highlighed
		/// </summary>
		/// <param name="dayElement"></param>
		/// <returns>True to highlight the day.</returns>
		/// <seealso cref="CalendarDay"/>
		/// <seealso cref="CalendarItem.IsHighlighted"/>
		protected internal virtual bool ShouldHighlightDay(CalendarDay dayElement)
		{
			// JJD 4/5/11 - TFS71155 - only highlight the day if it isn't leading or trailing
			//return dayElement.IsWorkday;
			// JJD 8/26/11 - TFS85067
			// Only highlight a day if it is enabled
			//return dayElement.IsWorkday && false == dayElement.IsLeadingOrTrailingItem;
			return dayElement.IsWorkday && false == dayElement.IsLeadingOrTrailingItem && dayElement.IsEnabled;
		}

		#endregion //ShouldHighlightDay	
    
		#endregion //Protected

		#region Internal

		#region AddLogicalDayDuration
		internal virtual DateTime AddLogicalDayDuration(DateTime start)
		{
			CalendarManager cm = this.CalendarManager;
			if (cm != null)
				return cm.AddDays(start, 1).AddMilliseconds(-1);

			return (start + TimeSpan.FromTicks(TimeSpan.TicksPerDay)).AddMilliseconds(-1);
		}
		#endregion // AddLogicalDayDuration

		#region ApplyLogicalDayOffset
		internal virtual DateTime ApplyLogicalDayOffset(DateTime date)
		{
			return date;
		}
		#endregion // ApplyLogicalDayOffset

		#region BindCalendarItemGroupPanel

		internal void BindCalendarItemGroupPanel(CalendarItemGroupPanel panel)
		{
			panel.SetBinding(CalendarItemGroupPanel.AutoAdjustDimensionsProperty, this.CreateBindingHelper(CalendarBase.AutoAdjustDimensionsProperty, "AutoAdjustDimensionsProperty"));
			panel.SetBinding(CalendarItemGroupPanel.DimensionsProperty, this.CreateBindingHelper(CalendarBase.DimensionsProperty, "DimensionsProperty"));
			panel.SetBinding(CalendarItemGroupPanel.LeadingAndTrailingDatesVisibilityProperty, this.CreateBindingHelper(CalendarBase.LeadingAndTrailingDatesVisibilityProperty, "LeadingAndTrailingDatesVisibilityProperty"));
			panel.SetBinding(CalendarItemGroupPanel.ScrollButtonVisibilityProperty, this.CreateBindingHelper(CalendarBase.ScrollButtonVisibilityProperty, "ScrollButtonVisibilityProperty"));
		}

		#endregion //BindCalendarItemGroupPanel	
    
		#region CanActivate
		internal bool CanActivate(DateTime date)
        {
            return this.GetActivatableItemDate(date, this.CurrentMode) != null;
        }

        // AS 10/3/08 TFS8607
        // Added mode parameter since navigation mode should not consider
        // disabled days of week or dates.
        //
        internal bool CanActivate(DateTime start, DateTime end, CalendarZoomMode mode)
        {
            return GetActivatableDate(start, end, true, mode) != null;
        }
        #endregion //CanActivate

		#region CanExecuteCommand

		internal bool CanExecuteCommand(CalendarCommandType command, FrameworkElement sourceElement)
		{
			switch (command)
			{
				case CalendarCommandType.ScrollNextGroup:
				case CalendarCommandType.ScrollNextGroups:
					return (this.CurrentState & CalendarStates.MaxDateInView) != CalendarStates.MaxDateInView;
				case CalendarCommandType.ScrollPreviousGroup:
				case CalendarCommandType.ScrollPreviousGroups:
					return (this.CurrentState & CalendarStates.MinDateInView) != CalendarStates.MinDateInView;
				case CalendarCommandType.Today:
					return (this.CurrentState & CalendarStates.TodayIsEnabled) == CalendarStates.TodayIsEnabled;
				case CalendarCommandType.FirstItemOfGroup:
				case CalendarCommandType.FirstItemOfFirstGroup:
				case CalendarCommandType.LastItemOfGroup:
				case CalendarCommandType.LastItemOfLastGroup:
				case CalendarCommandType.NextGroup:
				case CalendarCommandType.NextItem:
				case CalendarCommandType.NextItemRow:
				case CalendarCommandType.PreviousGroup:
				case CalendarCommandType.PreviousItem:
				case CalendarCommandType.PreviousItemRow:
					return (this.CurrentState & CalendarStates.ActiveDate) == CalendarStates.ActiveDate;
				case CalendarCommandType.ZoomInCalendarMode:
					return (this.CurrentState & CalendarStates.CanZoomInCalendarMode) == CalendarStates.CanZoomInCalendarMode;
				case CalendarCommandType.ZoomOutCalendarMode:
					return (this.CurrentState & CalendarStates.CanZoomOutCalendarMode) == CalendarStates.CanZoomOutCalendarMode;
				case CalendarCommandType.ToggleActiveDateSelection:
					return (this.CurrentState & (CalendarStates.ActiveDate | CalendarStates.MinCalendarMode)) == (CalendarStates.ActiveDate | CalendarStates.MinCalendarMode);
			}
			return true;
		}

		#endregion //CanExecuteCommand	

		#region ClearMouseOverItem



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		#endregion //ClearMouseOverItem

		#region ConstrainBetweenMinMaxDate
		internal DateTime ConstrainBetweenMinMaxDate(DateTime date)
        {
            // strip out the time portion
            date = date.Date;

            // make sure this is within the min/max date range
			DateTime max = this.MaxDateResolved;
			if (date > max)
				date = max;
			else
			{
				DateTime min = this.MinDateResolved;
				if (date < min)
					date = min;
			}

            return date;
        }
        #endregion //ConstrainBetweenMinMaxDate

        // AS 2/9/09 TFS11631
        #region FocusActiveItemWithDelay
        internal void FocusActiveItemWithDelay()
        {

            if (!this.IsKeyboardFocusWithin)
                return;

			bool? focusable = XamlHelper.GetFocusable(this);

			if (focusable.HasValue && focusable.Value == false)
				return;

            bool oldValue = _preventFocusActiveItem;
            _preventFocusActiveItem = true;

            try
            {
                this.Focus();

//#if WPF
//                this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Utils.MethodInvoker(FocusActiveItem));
//#else
//                this.Dispatcher.BeginInvoke(new Utils.MethodInvoker(FocusActiveItem));
//#endif
            }
            finally
            {
                _preventFocusActiveItem = oldValue;
            }
        }
        #endregion //FocusActiveItemWithDelay

        #region GetActivatableDate
        internal DateTime? GetActivatableDate(CalendarItem item)
        {
            DateTime? date;

            if (item == null)
                date = null;
            else
                date = this.GetActivatableDate(item.StartDate, item.EndDate, true, this.CurrentMode);

            return date;
        }

        internal DateTime? GetActivatableDate(DateTime start, DateTime end, bool next)
        {
            return GetActivatableDate(start, end, next, this.CurrentMode);
        }

        // AS 10/3/08 TFS8607
        // Added mode parameter since navigation mode should not consider
        // disabled days of week or dates.
        //
        internal DateTime? GetActivatableDate(DateTime start, DateTime end, bool next, CalendarZoomMode mode)
        {
            Debug.Assert(start <= end);

            // if the range is outside the min/max then it cannot be
            if (end < this.MinDateResolved || start > this.MaxDateResolved)
                return null;

            // AS 10/3/08 TFS8607
            // When in navigation mode, we want to ignore the disabled dates and days of week.
            //
			if (mode != this.MinCalendarModeResolved)
			{
				// AS 9/2/09 TFS18434
				// Depending on how we are navigating we should return the end date.
				//
				if (!next)
					return end;

				return start;
			}

            DateTime actualStart = this.ConstrainBetweenMinMaxDate(start);
            DateTime actualEnd = this.ConstrainBetweenMinMaxDate(end);

            // if all the days of the week in the range are disabled it cannot be
            int disabledDays = (int)this.DisabledDaysOfWeekInternal;
            int days = this.GetDaysOfWeek(actualStart, actualEnd);

            // if every day is disabled..
            if ((disabledDays & days) == days)
                return null;

            // if we have these dates in the disabled ranges
            return this._disabledDates.GetAvailableDate(actualStart, actualEnd, next);
        }
        #endregion //GetActivatableDate

        #region GetActivatableItemDate
        // AS 9/11/08
        // Added helper method to get the first activatable date for an item
        // AS 10/3/08 TFS8631
        // Added the CalendarZoomMode as a param.
        //
        private DateTime? GetActivatableItemDate(DateTime date, CalendarZoomMode mode)
        {
            DateTime start = this.CalendarManager.GetItemStartDate(date, mode);
            DateTime end = this.CalendarManager.GetItemEndDate(date, mode);

            return GetActivatableDate(start, end, true, mode);
        } 
        #endregion //GetActivatableItemDate

		#region GetTodayRange

		internal virtual DateRange GetTodayRange()
		{
			DateTime start = DateTime.Today.ToUniversalTime();

			return new DateRange(start, start.AddDays(1));
		}

		#endregion //GetTodayRange	
    
        #region GetGroup

        internal CalendarItemGroup GetGroup(DateTime date)
        {
            CalendarItemGroup containingGroup = null;

            if (this._groups.Count > 0)
			{
                for (int i = 0; i < this._groups.Count; i++)
				{
                    CalendarItemGroup group = this._groups[i];

					if (group.Items.Count == 0)
						continue;
                   
					if (group.FirstDateOfGroup == null || group.FirstDate > date)
                        break;

                    // skip earlier groups
                    if (group.LastDate < date)
                        continue;

                    containingGroup = group;

                    // if this is a non-leading/trailing date for item then we're
                    // done but if this is a trailing date for example then we want to
                    // keep looking in case there is a subsequent group with this item
                    // as a non-trailing item
                    if (group.FirstDateOfGroup <= date && date <= group.LastDateOfGroup)
                        break;
				}
			}

			return containingGroup;
		}
		#endregion //GetGroup

        #region GetGroups
        internal IEnumerable<CalendarItemGroup> GetGroups()
        {
            for (int i = 0; i < this._groups.Count; i++)
            {
                yield return this._groups[i];
            }

            yield break;
        }
        #endregion //GetGroups

        #region GetSelectableDates
        /// <summary>
        /// Returns an array of ranges indicating the dates which are selectable in given a range.
        /// </summary>
        /// <param name="range">The range of dates whose selectable state is to be evaluated.</param>
        /// <returns></returns>
        internal DateTime[] GetSelectableDates(DateRange range)
        {
            // ensure the range is start < end
            range.Normalize();

			// AS 1/8/10
			// Explicitly remove the time - this used to happen within the normalize.
			//
			range.RemoveTime();

            // nothing is selectable if its outside the min/max
            if (range.End > this.MaxDateResolved || range.Start < this.MinDateResolved)
                return new DateTime[0];

            // constrain the range to the current min/max
            if (range.Start < this.MinDateResolved)
                range.Start = this.MinDateResolved;
            else if (range.End > this.MaxDateResolved)
                range.End = this.MaxDateResolved;

            List<DateTime> dates = new List<DateTime>();
            this._disabledDates.AddAvailableDates(range, dates);
            return dates.ToArray();
        }
        #endregion //GetSelectableDates

		#region GetString
		internal static string GetString(string name)
		{
			return GetString(name, null);
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}
		#endregion // GetString

        #region InternalSelectItem



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        internal bool InternalSelectItem(ISelectableItem item,
            bool clearExistingSelection,
            bool select)
        {
            CalendarItem calItem = this.GetItem(item);
			DateRange? dateRange = null;

			if (calItem != null)
			{
				DateTime? date = this.GetActivatableDate(calItem);

				if (date != null)
					dateRange = new DateRange(date.Value);
			}
			else
			{
				dateRange = this.GetRangeFromItem(item);
			}

            return this.InternalSelectItem(dateRange, clearExistingSelection, select);
        }

        internal bool InternalSelectItem(DateRange? dateRange,
			bool clearExistingSelection,
			bool select)
		{
			// we cannot do a selection if we were not given a day
            if (dateRange == null && (clearExistingSelection == false || this.SelectedDatesInternal.Count == 0))
				return false;

			// if the item isn't selectable, ignore the select flag
            if (dateRange != null && select && (this.CanActivate(dateRange.Value.Start) == false || this.CanActivate(dateRange.Value.End) == false))
				select = false;

			SelectedDateCollection selectedDates = this.SelectedDatesInternal as SelectedDateCollection;

			// if the item is already selected/unselected...
			if (dateRange != null && selectedDates.IsSelected(dateRange.Value) == select)
			{
				// and we're not clearing the selection - or we are but 
				// there is nothing selected - then exit
				if (clearExistingSelection == false || this.SelectedDatesInternal.Count == 0)
					return true;
			}

            IList<DateTime> newSelection = this.CalculateNewSelection(dateRange, clearExistingSelection, select);
            this.SelectedDatesInternal.Reinitialize(newSelection);
            return true;
        }
		#endregion //InternalSelectItem

		#region InternalSelectRange



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool InternalSelectRange(ISelectableItem item,
			bool clearExistingSelection,
			bool select)
		{
			CalendarItem calItem = this.GetItem(item);
			DateRange? dateRange = null;

			if (calItem != null)
			{
				DateTime? date = this.GetActivatableDate(calItem);

				if (date != null)
					dateRange = new DateRange(date.Value);
			}
			else
			{
				dateRange = this.GetRangeFromItem(item);
			}


			//we cannot know the extent of the range if we don't have an end pt
			if (dateRange == null)
				return false;

			dateRange.Value.Normalize();
			dateRange.Value.RemoveTime();

			if (this._pivotRange == null)
				this.SelectionHost.SetPivotItem(item, false);

            IList<DateTime> newSelection = this.CalculateNewSelectionRange(dateRange, clearExistingSelection, select);
            this.SelectedDatesInternal.Reinitialize(newSelection);
            return true;
		}
		#endregion //InternalSelectRange

		#region IsDateInView

		// AS 10/14/09 FR11859
		// Changed parameter name from ignoreLeadingTrailing to ignoreLeadingTrailingDates
		// to avoid confusion in thinking that this may evaluating the IsLeading/IsTrailing
		// property of groups. This is only used for evaluating/ignoring the leading/trailing 
		// dates within any group.
		//
		internal bool IsDateInView(DateTime date, bool ignoreLeadingTrailingDates)
		{
			
			// If we aren't in the min zoom mode then ignore the AllowLeadingAndTrailingGroupActivation setting
			
			return IsDateInView(date, ignoreLeadingTrailingDates, this.IsMinCalendarMode && !this.AllowLeadingAndTrailingGroupActivation);
		}

		// AS 10/14/09 FR11859
		// Added an overload so we can ignore leading/trailing groups.
		//
		internal bool IsDateInView(DateTime date, bool ignoreLeadingTrailingDates, bool ignoreLeadingTrailingGroups)
		{
            if (this._groups.Count > 0)
			{
				// AS 10/14/09 FR11859
				//CalendarItemGroup firstGroup = this.FirstGroup;
                //CalendarItemGroup lastGroup = this.LastGroup;
                CalendarItemGroup firstGroup = this.GetFirstGroup(true, ignoreLeadingTrailingGroups);
                CalendarItemGroup lastGroup = this.GetLastGroup(true, ignoreLeadingTrailingGroups);

                if (null != firstGroup && null != lastGroup)
                {
                    Debug.Assert(null != firstGroup.FirstDateOfGroup);
                    Debug.Assert(null != lastGroup.LastDateOfGroup);

                    DateTime startDate = ignoreLeadingTrailingDates ? firstGroup.FirstDateOfGroup.Value : firstGroup.FirstDate;
                    DateTime endDate = ignoreLeadingTrailingDates ? lastGroup.LastDateOfGroup.Value : lastGroup.LastDate;

                    // if its already in view there is nothing to do
                    if (date >= startDate && date <= endDate)
                        return true;
                }
			}

			return false;
		}
		#endregion //IsDateInView

        #region IsZoomFocusGroup
        internal bool IsZoomFocusGroup(CalendarItemGroup group)
        {
            return group == this._zoomFocusGroup;
        }
        #endregion //IsZoomFocusGroup

        #region OnAutoGeneratedGroupsChanged
        internal void OnAutoGeneratedGroupsChanged()
        {
            using (new CalendarItemGroup.GroupInitializationHelper(this._groups))
            {
                this.ResolveReferenceDate();
            }
        }
        #endregion //OnAutoGeneratedGroupsChanged

        #region OnGroupOffsetChanged
        internal void OnGroupOffsetChanged(CalendarItemGroup group, int oldOffset, int newOffset)
        {
            this.SortGroups();
        }
        #endregion //OnGroupOffsetChanged

		// JJD 11/10/11 - TFS95841 - made internal
		#region RaisePendingSelectionChanged

		internal void RaisePendingSelectionChanged()
		{
			if (_mouseDownSnapshot == null)
				return;

			_mouseDownSnapshot.ProcessEndSelection();
			_mouseDownSnapshot = null;
		}

		#endregion //RaisePendingSelectionChanged	
    
        #region RegisterGroup
        internal void RegisterGroup(CalendarItemGroup group)
        {
			CalendarBase.SetCalendar(group, this);

            if (group.IsGroupForSizing)
            {
                Debug.Assert(this._groupForSizing == null || group == this._groupForSizing);
                this._groupForSizing = group;
				PresentationUtilities.ReparentElement(_templatePanel, _groupForSizing, true);

            }
            else
            {
                Debug.Assert(false == this._groups.Contains(group));
                this._groups.Add(group);

                this.SortGroups();
            }
        }
        #endregion //RegisterGroup

		#region SetMouseOverItem



#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)


		#endregion //SetMouseOverItem

		// JJD 11/10/11 - TFS95841 - added 
		#region SnapshotSelection

		internal void SnapshotSelection()
		{
			// If we already have a snapshot then just return
			if (_mouseDownSnapshot != null)
				return;

			// take a snapshot of the current selection so we can delay raising the SelectionChanged
			// event until the mouse is released
			this._mouseDownSnapshot = new SelectionSnapshot(this);

			this._mouseDownSnapshot.Initialize(this._selectedDates);
		}

		#endregion //SnapshotSelection	

		#region UnregisterGroup
		internal void UnregisterGroup(CalendarItemGroup group)
        {
            if (group.IsGroupForSizing)
            {
                Debug.Assert(group == this._groupForSizing);

				if (group == this._groupForSizing)
				{
					this._groupForSizing = null;
					PresentationUtilities.ReparentElement(_templatePanel, _groupForSizing, false);
				}
            }
            else
            {
                Debug.Assert(this._groups.Contains(group));
                this._groups.Remove(group);
            }
        }
        #endregion //UnregisterGroup

        #region OnSelectedStateChanged
        internal void OnSelectedStateChanged(IList<DateTime> datesSelected, IList<DateTime> datesUnselected)
        {
            // when we're not in the minimum mode then the items don't affect
            // selection and their selected state is not based on the selection
            // properties
			// AS 10/15/09 TFS23544
			// If we have overlapping groups then the items could exist in multiple in which 
			// case we will just reinitialize the selected state of all items.
			//
			//if (this.IsMinCalendarMode)
            if (this.IsMinCalendarMode && !this.HasOverlappingGroups())
            {
                this.UpdateSelectedState(datesSelected, true);
                this.UpdateSelectedState(datesUnselected, false);
            }
            else
            {
                // otherwise let the groups know because they may need to update
                // the ContainsSelectedDates properties of the items
                this.NotifyGroupChange(CalendarChange.SelectionChanged);
            }
        } 
        #endregion //OnSelectedStateChanged

		// JJD 9/9/11 - TFS74024 - Added
		#region VerifySelectedDayStates

		internal void VerifySelectedDayStatesAsync()
		{
			if (_selectedStateVerifyPending == false && this.CurrentMode == CalendarZoomMode.Days)
			{
				_selectedStateVerifyPending = true;
				this.Dispatcher.BeginInvoke(new CalendarUtilities.MethodInvoker(VerifySelectedDayStates));
			}
		}

		// JJD 9/9/11 - TFS74024 - Added
		internal void VerifySelectedDayStates()
		{
			_selectedStateVerifyPending = false;

			if (this.CurrentMode == CalendarZoomMode.Days)
			{
				SelectedDateCollection dates = this.SelectedDatesInternal as SelectedDateCollection;

				this.UpdateSelectedState(dates, true);

				dates.VerifyWeekSelection();
			}
		}
		#endregion //VerifySelectedDayStates

		#endregion //Internal

		#region Private

		#region ActivateDate
		private bool ActivateDate(DateTime? dateValue)
        {
            return this.ActivateDate(dateValue, true);
        }

		private bool ActivateDate(DateTime? dateValue, bool useSelectionStrategy)
		{
            bool result = false;

            if (null != dateValue)
            {
                DateTime? activatableDate = this.GetActivatableItemDate(dateValue.Value, this.CurrentMode);

                // if the item with the specified date cannot be activated
                if (null == activatableDate)
                    return false;

                DateTime date = dateValue.Value;
                bool setActiveDate = true;

                if (useSelectionStrategy)
                {
                    // AS 10/22/08 TFS9453
                    // If we don't have a pivot date then use the active date since
                    // we may be about to perform a range selection. If we don't then 
                    // the pivot will be null and the strategy will use the element 
                    // we navigate to as the pivot date.
                    //
					if (this._pivotRange == null)
					{
						DateTime? dt = this.SelectedDate ?? this.ActiveDate;

						if (dt.HasValue)
							_pivotRange = new DateRange(dt.Value);
						else
							_pivotRange = null;
					}

                    this.BringDateIntoView(date);

                    CalendarItem item = this.GetItem(date);

                    Debug.Assert(null != item, "We cannot select the item if its not in view.");

                    if (null != item)
                    {
                        SelectionStrategyBase selectionStrategy = this.SelectionHost.GetSelectionStrategyForItem(item);
                        if (selectionStrategy != null)
                        {
                            bool shiftKeyDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
                            bool ctlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
                            result = selectionStrategy.SelectItemViaKeyboard(item, shiftKeyDown, ctlKeyDown, false);
                            setActiveDate = false;
                        }
                    }
                }

                if (setActiveDate)
                {
                    this.ActiveDate = date;
                    result = this.ActiveDate == activatableDate;
                }
            }

			return result;
		}

		#endregion //ActivateDate

		#region CalculateNewSelection



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal IList<DateTime> CalculateNewSelection(
			DateRange? dateRange,
			bool clearExistingSelection,
			bool select)
		{
            List<DateTime> selected = new List<DateTime>();

            Debug.Assert(dateRange != null || clearExistingSelection);

			SelectedDateCollection selectedDates = this.SelectedDatesInternal as SelectedDateCollection;

			if (false == clearExistingSelection)
			{
				// copy existing selected items
				selected.AddRange(selectedDates);
			}

            if (null != dateRange)
            {
				DateRange range = dateRange.Value;
				range.Normalize();
				range.RemoveTime();

				DateTime dt = range.Start;
				DateTime dtEnd = range.End;

				// JJD 3/8/11 - TFS66513
				// Get the cal manager for adding days
				CalendarManager cm = this.CalendarManager;

				while (dt <= dtEnd)
				{
					if (select)
					{
						// JJD 4/5/11 - TFS66907
						// only add the date if it isn't already in the selected collection
						if (true == clearExistingSelection ||
							selectedDates.IsSelected(dt) == false)
						{
							if (this.CanActivate(dt))
								selected.Add(dt);
						}
					}
					else
					{
						// JJD 4/5/11 - TFS66907
						// only remove the date if the collection wasn't cleared above and
						// it is in the collection 
						if (false == clearExistingSelection &&
							selectedDates.IsSelected(dt) == true)
							selected.Remove(dt);
					}


					// JJD 3/8/11 - TFS66513
					// Use the calendar manager for adding days because it deals gracefully with
					// min and ma dates without blowing up
					//dt = dt.AddDays(1);
					dt = cm.AddDays(dt, 1);
				}
            }

			// Make sure the number of selected items doesn't exceed the max.
			//
			if (select)
				this.EnsureItemsWithinMaxSelectedItemsBounds(selected);

			// return the newly created Selected object
			return selected;
		}

		#endregion //CalculateNewSelection

		#region CalculateNewSelectionRange



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal IList<DateTime> CalculateNewSelectionRange(
			DateRange? dateRange,
			bool clearExistingSelection,
			bool select)
		{
			List<DateTime> selected = new List<DateTime>();

            Debug.Assert(dateRange != null || clearExistingSelection);

			if (null == this._pivotRange)
				return selected;

			DateTime rangeStartDate;
			DateTime rangeEndDate;

            if (null != dateRange)
            {
                bool isInverted = false;

                if (this._pivotRange.Value.Start <= dateRange.Value.Start)
                {
                    rangeStartDate = this._pivotRange.Value.Start;
                }
                else
                {
                    isInverted = true;
                    rangeStartDate = dateRange.Value.Start;
                }

				rangeEndDate = this._pivotRange.Value.End > dateRange.Value.End ? this._pivotRange.Value.End : dateRange.Value.End;

                if (false == clearExistingSelection)
                {
                    foreach (DateTime oldDate in this._selectedDatesSnapshot)
                    {
                        // copy existing selected items that are outside the range
                        if (oldDate < rangeStartDate || oldDate > rangeEndDate)
                        {
                            Debug.Assert(this.CanActivate(oldDate));
                            selected.Add(oldDate);
                        }
                    }
                }

                if (select)
                {
                    int adjustment = 1;

                    if (isInverted)
                    {
                        // restore the inversion since we need to 
                        // add the dates from the pivot date towards the date
                        rangeStartDate = this._pivotRange.Value.End;
                        rangeEndDate = dateRange.Value.Start;
                        adjustment = -1;
                    }

                    
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                    CalendarZoomMode mode = this.CurrentMode;
                    DateTime? currentDate = rangeStartDate;

                    // AS 10/23/08
                    // The range could be inverted so we need to do different comparisons in that case.
                    //
                    //while (currentDate != null && currentDate <= rangeEndDate)
                    while (currentDate != null)
                    {
                        if (isInverted == false && currentDate > rangeEndDate)
                            break;
                        else if (isInverted && currentDate < rangeEndDate)
                            break;

                        DateTime? selectableDate = this.GetActivatableItemDate(currentDate.Value, mode);
                        if (null != selectableDate)
                            selected.Add(selectableDate.Value);

                        // AS 10/23/08
                        // Don't return a default value.
                        //
                        //currentDate = this._calendarManager.AddItemOffset(currentDate.Value, adjustment, mode);
                        currentDate = this._calendarManager.TryAddItemOffset(currentDate.Value, adjustment, mode);
                    }
                }

                // Make sure the number of selected items doesn't exceed the max.
                //
                if (select)
                    this.EnsureItemsWithinMaxSelectedItemsBounds(selected);
            }

			// return the newly created Selected object
			return selected;
		}

		#endregion //CalculateNewSelectionRange

		#region CancelPendingMouseOperations

		private bool CancelPendingMouseOperations()
		{
			bool handled = false;
			if (this._selectionController != null)
				handled = this._selectionController.CancelPendingOperations();

			return handled;
		}

		#endregion //CancelPendingMouseOperations	

        #region ChangeCalendarMode
        private void ChangeCalendarMode(bool zoomOut, object parameter, object originalSource)
        {
            // use parameter to figure out the date to use when reinitializing 
            // the dates and which group contains that month e.g. if clicking on the 
            // Aug 2007 header and that is the second group then the second group 
            // should show Jan-Dec of 2007

            // find out which group raised the request so we can make sure that it
            // continues to show the same date range containing the dates it was 
            // previously displaying
            CalendarItemGroup group = null;
            DateTime? startDate = null;

            if (parameter is DateTime)
                startDate = (DateTime)parameter;

            if (zoomOut)
            {
                group = parameter as CalendarItemGroup;

                // if the command parameter isn't a group but the original source was...
                if (null == group && originalSource is FrameworkElement)
                {
                    if (originalSource is CalendarItemGroup)
                        group = (CalendarItemGroup)originalSource;
                    else
                        group = PresentationUtilities.GetVisualAncestor<CalendarItemGroup>( (FrameworkElement)originalSource, null);
                }

                if (null == startDate && null != group)
                    startDate = group.FirstDateOfGroup;
            }
            else // decreasing mode...
            {
                CalendarItem item = parameter as CalendarItem;

                if (null == item && originalSource is FrameworkElement)
                {
                    if (originalSource is CalendarItem)
                        item = (CalendarItem)originalSource;
                    else
						item = PresentationUtilities.GetVisualAncestor<CalendarItem>((FrameworkElement)originalSource, null);
				}

                if (null != item)
                {
                    group = item.Group;

                    if (null == startDate)
                        startDate = item.StartDate;
                }
            }

            // if we didn't get handed a date and couldn't ascertain one
            // because there was no context of an item then we should just
            // change the mode back and let the coersion of the reference
            // date handle it
            //if (null == startDate)
            //	startDate = this.ActiveDate ?? this.SelectedDate ?? this.ReferenceDate ?? this.ConstrainBetweenMinMaxDate(DateTime.Today);

            // AS 10/3/08 TFS8607
            // If there is no group and we have focus then we will get the one 
            // with the active item if we have it.
            //
            DateTime? newReferenceDate = startDate;

#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

            int groupOffset = group != null ? group.ReferenceGroupOffset : 0;
            int modeOffset = zoomOut ? 1 : -1;
            CalendarZoomMode newMode = (CalendarZoomMode)(modeOffset + (int)this.CurrentMode);

            using (new CalendarItemGroup.GroupInitializationHelper(this._groups, false))
            {
                try
                {
                    // track the group we are focused on so we can zoom its contents
                    this._zoomFocusGroup = group;

                    this.CurrentMode = newMode;
                }
                finally
                {
                    this._zoomFocusGroup = null;
                }

                if (this.CurrentMode == newMode && null != newReferenceDate)
                {
                    // now adjust the start date to be that of the increased group
                    // AS 10/3/08 TFS8607
                    // We don't want to alter the start date here. We just want to get the reference
                    // date. Below we will use the start date to find the date to activate.
                    //
                    //startDate = this._calendarManager.GetGroupStartDate(startDate.Value, newMode);
                    DateTime groupStartDate = this._calendarManager.GetGroupStartDate(newReferenceDate.Value, newMode);

                    // now offset that start date by the group index
                    if (groupOffset != 0)
                        groupStartDate = this._calendarManager.AddGroupOffset(groupStartDate, -groupOffset, newMode, true);

					// JJD 1/12/12 - TFS96805
					// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
					//this.ReferenceDate = groupStartDate;
					this.ResolveReferenceDate(groupStartDate);
                }

                this.ResolveReferenceDate();
            }

            // AS 10/3/08 TFS8607
            // Actually we want to prefer the start date. For example, suppose your zooming out
            // from August 2008 but the active date is Jan 2009. You don't want January to be focused.
            // You want August 2008 to be active.
            //
            //DateTime? newActiveDate = this.ActiveDate ?? startDate;
            DateTime? newActiveDate = startDate ?? this.ActiveDate;

            if (null != newActiveDate)
            {
                CalendarItem activeItem = this.GetItem(newActiveDate.Value);

                if (null != activeItem)
                {
                    Debug.Assert(newMode == CalendarItemGroup.GetCurrentMode(activeItem));

//                    // AS 6/28/10 TFS32190
//                    // If focus is outside the control then we won't try to take focus however to 
//                    // maintain the previous behavior whereby the activedate would have been updated 
//                    // by the element when it got focus, we'll set the ActiveDate to make sure it 
//                    // has stored the same value.
//                    //
//                    //activeItem.Focus();
//#if WPF
//                    if (this.IsKeyboardFocusWithin)
//                        activeItem.Focus();
//                    else
//#endif
						this.ActiveDate = activeItem.StartDate;
                }
            }
        }

        #endregion //ChangeCalendarMode

        #region ClearPreferredNavInfo
        private void ClearPreferredNavInfo()
        {
            this._preferredNavColumn = -1;
            this._preferredGroupNavStart = null;
            this._preferredGroupOffset = 0;
        }
        #endregion //ClearPreferredNavInfo


        #region CoerceIsTabStop
        private static object CoerceIsTabStop(DependencyObject d, object newValue)
        {
            CalendarBase cal = (CalendarBase)d;
			bool? focusable = XamlHelper.GetFocusable(cal);

			if (focusable.HasValue && focusable.Value == false)
				return KnownBoxes.FalseBox;

            // when focus is moved within the calendar then we want to turn
            // off the istabstop so the user can shift tab out of the control
            // into the previous control
            if (cal.IsKeyboardFocused == false && cal.IsKeyboardFocusWithin)
                return KnownBoxes.FalseBox;

            return newValue;
        } 
        #endregion //CoerceIsTabStop


		#region CreateBindingHelper

		private Binding CreateBindingHelper(DependencyProperty dp, string propertyPropertyName)
		{
			Binding binding = PresentationUtilities.CreateBinding(
				new BindingPart
				{

					PathParameter = dp



				}
			);

			binding.Source = this;

			return binding;
		}

		#endregion //CreateBindingHelper	

		#region EnsureItemsWithinMaxSelectedItemsBounds






		private void EnsureItemsWithinMaxSelectedItemsBounds(List<DateTime> newSelection)
		{
			int maxSelectedDates = this.MaxSelectedDatesResolved;

			int overflow = newSelection.Count - maxSelectedDates;

            if (overflow > 0)
            {
                // AS 10/23/08
                //newSelection.RemoveRange((newSelection.Count - 1) - overflow, overflow);
                newSelection.RemoveRange(newSelection.Count - overflow, overflow);
            }
		}

		#endregion //EnsureItemsWithinMaxSelectedItemsBounds

 
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

        #region GetActivatableDate
        private DateTime? GetActivatableDate(DateTime? date, bool previousItem, bool includeInitialDate)
        {
            return GetActivatableDate(date, previousItem, includeInitialDate, true);
        }

        private DateTime? GetActivatableDate(DateTime? date, bool previousItem, bool includeInitialDate, bool coerceMinMax)
        {
            DateTime? newDate = date;

            if (null != newDate)
            {
                // AS 10/24/08 TFS9578
                if (coerceMinMax)
                    newDate = this.ConstrainBetweenMinMaxDate(newDate.Value);

                CalendarZoomMode mode = this.CurrentMode;
                DateTime startDate = this._calendarManager.GetItemStartDate(newDate.Value, mode);
                DateTime endDate = this._calendarManager.GetItemEndDate(startDate, mode);
                int itemOffset = previousItem ? -1 : 1;

                if (includeInitialDate)
                    newDate = this.GetActivatableDate(startDate, endDate, true);
                else
                    newDate = null;

                if (null == newDate)
                {
                    while (true)
                    {
                        // offset the item by offset amount
                        newDate = this._calendarManager.TryAddItemOffset(startDate, itemOffset, mode);

                        // ignore any date outside the available range
                        if (newDate < this.MinDateResolved || newDate > this.MaxDateResolved)
                            newDate = null;

                        if (newDate == null)
                            break;

                        // so we can use this as our start date
                        startDate = newDate.Value;

                        // get the end date so we can see if any date within
                        // the item is available
                        endDate = this._calendarManager.GetItemEndDate(startDate, mode);

                        // see if there is an activatable date in this item
                        newDate = this.GetActivatableDate(newDate.Value, endDate, true);
                        
                        if (null != newDate)
                            break;
                    }
                }
            }

            return newDate;
        }
        #endregion //GetActivatableDate

        #region GetClosestActivatableDate
        private DateTime? GetClosestActivatableDate(DateTime itemStart, DateTime itemEnd)
        {
            DateTime? next = this.GetActivatableDate(itemStart, this.MaxDateResolved, true);
            DateTime? previous = this.GetActivatableDate(this.MinDateResolved, itemStart, false);
            bool useNext = true;

            if (null != next && null != previous)
            {
                TimeSpan diffNext = next.Value.Subtract(itemStart);
                TimeSpan diffPrev = itemEnd.Subtract(previous.Value);
                useNext = diffPrev > diffNext;
            }
            else if (null != previous)
                useNext = false;

            return useNext ? next : previous;
        }
        #endregion //GetClosestActivatableDate

        #region GetDaysOfWeek
        private int GetDaysOfWeek(DateTime start, DateTime end)
        {
            Debug.Assert(start <= end);

            if (start == end)
                return 1 << (int)this._calendarManager.Calendar.GetDayOfWeek(start);
            else if (end.Subtract(start).Days > 6)
                return AllDays;
            else
            {
                int value = 0;
                System.Globalization.Calendar calendar = this._calendarManager.Calendar;

                for (int i = 0, count = end.Subtract(start).Days; i < count; i++)
                {
                    value |= 1 << (int)calendar.GetDayOfWeek(start);
                    start = this._calendarManager.AddDays(start, 1).Date;
                }

                return value;
            }
        }
        #endregion //GetDaysOfWeek

        #region GetDefaultReferenceDate
        private DateTime GetDefaultReferenceDate()
        {
            // AS 9/4/08
            // We don't need to consider the SelectedDate separately since
            // the ActiveDate uses the SelectedDate when there is no explicit
            // ActiveDate.
            //
            DateTime date = this.ActiveDate ?? DateTime.Today;
            return GetDefaultReferenceDate(date);
        }

        private DateTime GetDefaultReferenceDate(DateTime date)
        {
            CalendarZoomMode mode = this.CurrentMode;

            // make sure its within the min/max
            date = this.ConstrainBetweenMinMaxDate(date);

            // then make sure its the start date of a group
            date = this._calendarManager.GetGroupStartDate(date, mode);

            // then check again to make sure its within the valid range
            date = this.ConstrainBetweenMinMaxDate(date);

            if (this._groups.Count > 0)
            {
                // next make sure we won't have groups after this that are unusable (because
                // they would contain a date after the maxdate)
				// AS 10/14/09 FR11859
				// We want to ignore the leading/trailing groups when massaging the reference date. 
				// It is ok for leading/trailing groups to be empty.
				//
				//int lastGroupOffset = this._groups[this._groups.Count - 1].ReferenceGroupOffset;
				int lastGroupOffset = this.GetLastGroup(false, true).ReferenceGroupOffset;
                DateTime? lastGroupDate = this._calendarManager.TryAddGroupOffset(date, lastGroupOffset, mode, true);

                // if we can't build such a date or its beyond our valid date...
                if (null == lastGroupDate || lastGroupDate.Value > this.MaxDateResolved)
                {
                    // calculate the date for the last group
                    lastGroupDate = this._calendarManager.GetGroupStartDate(this.MaxDateResolved, mode);

                    // try to get the reference group offset using the last group as the anchor
                    DateTime? newDate = this._calendarManager.TryAddGroupOffset(lastGroupDate.Value, -lastGroupOffset, mode, true);

                    // if we can't build one the first group must be before the calendar's supported
                    // min date so use the min date
                    if (null == newDate || newDate.Value < this.MinDateResolved)
                        date = this._calendarManager.GetGroupStartDate(this.MinDateResolved, mode);
                    else
                        date = newDate.Value;
                }

				// AS 10/14/09 FR11859
				//int firstGroupOffset = this._groups[0].ReferenceGroupOffset;
				int firstGroupOffset = this.GetFirstGroup(false, true).ReferenceGroupOffset;

                // then make sure we won't have groups before this that are unusable (because
                // they would contain a date before the mindate)
                if (firstGroupOffset < 0)
                {
                    DateTime? firstGroupDate = this._calendarManager.TryAddGroupOffset(date, firstGroupOffset, mode, true);

                    // if we cannot build such a date using the culture's calendar or its
                    // before our mindate...
                    if (null == firstGroupDate || firstGroupDate < this.MinDateResolved)
                    {
                        // start with our min...
                        firstGroupDate = this._calendarManager.GetGroupStartDate(this.MinDateResolved, mode);

                        DateTime? newDate = this._calendarManager.TryAddGroupOffset(firstGroupDate.Value, -firstGroupOffset, mode, true);

                        // if we cannot build one then use the max date as the reference date
                        if (null == newDate)
                            date = this._calendarManager.GetGroupStartDate(this.MaxDateResolved, mode);
                        else
                            date = newDate.Value;
                    }
                }

                date = this.ConstrainBetweenMinMaxDate(date);
            }

            return date;
        } 
        #endregion //GetDefaultReferenceDate

		// AS 10/14/09 FR11859
		#region GetFirstGroup
		private CalendarItemGroup GetFirstGroup(bool ignoreEmptyGroups)
		{
			return GetFirstGroup(ignoreEmptyGroups, !this.AllowLeadingAndTrailingGroupActivation);
		}

		private CalendarItemGroup GetFirstGroup(bool ignoreEmptyGroups, bool ignoreLeadingTrailingGroups)
		{
			for (int i = 0, count = this._groups.Count; i < count; i++)
			{
				CalendarItemGroup group = this._groups[i];

				if (ignoreLeadingTrailingGroups && group.IsLeadingOrTrailingGroup)
					continue;

				if (!ignoreEmptyGroups || group.Items.Count > 0)
					return group;
			}

			if (ignoreLeadingTrailingGroups)
				return GetFirstGroup(ignoreEmptyGroups, false);

			return null;
		}
		#endregion //GetFirstGroup

		#region GetFirstLastItem
        private ISelectableItem GetFirstLastItem(CalendarItem sourceItem, Point pt, CalendarItemGroup group)
        {
            CalendarItem[] items = new CalendarItem[] { group.Items[0], group.Items[group.Items.Count - 1] };

            for (int i = 0; i < items.Length; i++)
            {
                GeneralTransform transform = items[i].TransformToVisual(this);
                Rect rect = new Rect(new Point(), new Size(items[i].ActualWidth, items[i].ActualHeight));

                rect = transform.TransformBounds(rect);

                if (pt.Y >= rect.Y && pt.Y < rect.Bottom)
                {
                    // only deal with the gap before the first
                    // item or after the last
                    if (i == 0 && pt.X < rect.X ||
                        i == 1 && pt.X >= rect.Right)
                    {
                        // we're over the row containing the first
                        // or last item. return an item based on the 
                        // source item
                        //
                        int groupIndex = this._groups.IndexOf(group);

                        // if the mouse is over the area in front of 
                        // the first row but is 
                        if (i == 0 && sourceItem.StartDate < items[i].StartDate)
                        {
                            // get the last item of the previous group
                            if (groupIndex > 0)
                            {
                                CalendarItemGroup prevGroup = this._groups[groupIndex - 1];

                                if (prevGroup.Items.Count > 0)
                                    return prevGroup.Items[prevGroup.Items.Count - 1];
                            }
                        }
                        else if (i == 1 && sourceItem.StartDate > items[i].StartDate)
                        {
                            // get the first item of the next group
                            if (groupIndex < this._groups.Count - 1)
                            {
                                CalendarItemGroup nextGroup = this._groups[groupIndex + 1];

                                if (nextGroup.Items.Count > 0)
                                    return nextGroup.Items[0];
                            }
                        }
                        else
                        {
                            return items[i];
                        }
                    }
                }
            }

            return null;
        }
        #endregion //GetFirstLastItem

        #region GetItem

		private CalendarItem GetItem(ISelectableItem item)
		{
            item = this.SelectionHost.TranslateItem(item);

			CalendarItem calItem = item as CalendarItem;

			if (calItem != null)
			{
                calItem = this.GetItem(calItem.StartDate);
			}

			return calItem;
		}

		internal CalendarItem GetItem(DateTime date)
		{
			CalendarItemGroup month = this.GetGroup(date);

			return null != month
				? month.GetItem(date)
				: null;
		}
		#endregion //GetItem

		#region GetRangeFromItem

		private DateRange? GetRangeFromItem(ISelectableItem item)
		{
			CalendarItem calItem = this.GetItem(item);

			if (calItem != null)
				return new DateRange(calItem.StartDate);
			else
			{
				if (this.SupportsWeekSelectionMode)
				{
					CalendarWeekNumber week = item as CalendarWeekNumber;

					if (week != null)
						return week.Range;
				}
			}

			return null;
		}

		#endregion //GetRangeFromItem	
    
		#region GetWeekNumber

		internal CalendarWeekNumber GetWeekNumber(DateTime date)
		{
			int year;

			int week = this.CalendarManager.GetWeekNumberForDate(date, this.WeekRuleInternal, this.FirstDayOfWeekInternal, out year);

			CalendarItemGroup month = this.GetGroup(date);

			return null != month
				? month.GetWeekNumber(week)
				: null;
		}

		#endregion //GetWeekNumber	
    
		// AS 10/14/09 FR11859
		#region GetLastGroup
		private CalendarItemGroup GetLastGroup(bool ignoreEmptyGroups)
		{
			return GetLastGroup(ignoreEmptyGroups, !this.AllowLeadingAndTrailingGroupActivation);
		}

		private CalendarItemGroup GetLastGroup(bool ignoreEmptyGroups, bool ignoreLeadingTrailingGroups)
		{
			for (int i = this._groups.Count - 1; i >= 0; i--)
			{
				CalendarItemGroup group = this._groups[i];

				if (ignoreLeadingTrailingGroups && group.IsLeadingOrTrailingGroup)
					continue;

				if (!ignoreEmptyGroups || group.Items.Count > 0)
					return group;
			}

			if (ignoreLeadingTrailingGroups)
				return GetLastGroup(ignoreEmptyGroups, false);

			return null;
		}
		#endregion //GetLastGroup

		// AS 10/15/09 TFS23544
		#region HasOverlappingGroups
		private bool HasOverlappingGroups()
		{
			if (null != _groups)
			{
				DateTime? lastEndDate = null;

				for (int i = 0; i < _groups.Count; i++)
				{
					CalendarItemGroup group = _groups[i];

					if (group.LastDateOfGroup == null)
						continue;

					if (lastEndDate != null && group.FirstDate <= lastEndDate)
						return true;

					lastEndDate = group.LastDate;
				}
			}

			return false;
		}
		#endregion //HasOverlappingGroups

		#region InitializeDaysOfWeek

		private bool InitializeDaysOfWeek()
		{
            if (_isInitialized == false)
                return false;

            DayOfWeek[] dow = this._calendarManager.GetDaysOfWeek();
            bool updateDays = dow.Length != this._daysOfWeekInternal.Count;

            if (false == updateDays)
            {
                for (int i = 0; i < dow.Length; i++)
                {
                    if (dow[i] != this._daysOfWeekInternal[i])
                    {
                        updateDays = true;
                        break;
                    }
                }
            }

            if (updateDays)
                this._daysOfWeekInternal.ReInitialize(dow);

            return updateDays;
		}

		#endregion //InitializeDaysOfWeek

		// JJD 10/12/11 - TFS89043 - added
		#region InitializeToday

		private void InitializeToday()
		{
			DateRange range = this.GetTodayRange();

			// JJD 10/12/11 - TFS89043
			// added member to hold a strong reference on the token returned from
			// TimeManager's AddTimeRange so our callback will be called when Today changes
			//TimeManager.Instance.AddTimeRange(range, false, this.OnCurrentDateChanged);
			_todayToken = TimeManager.Instance.AddTimeRange(range, false, this.OnCurrentDateChanged);

			this.SetValue(TodayPropertyKey, DateTime.Today);
		}

		#endregion //InitializeToday	
    
		#region NavigateGroup
		private bool NavigateGroup(DateTime date, bool next)
        {
            CalendarZoomMode mode = this.CurrentMode;
            int offset = next ? 1 : -1;

            if (this._preferredGroupNavStart != null)
            {
                date = this._preferredGroupNavStart.Value;
                offset += this._preferredGroupOffset;
            }

            while (true)
            {
                // offset by the specified number of groups
                DateTime newDate = this.CalendarManager.AddGroupOffset(date, offset, mode, false);

                // see if there is any date in the group that is activatable
                DateTime groupStart = this.CalendarManager.GetGroupStartDate(newDate, mode);
                DateTime groupEnd = this.CalendarManager.GetGroupEndDate(newDate, mode);

                // get the end date of this item
                DateTime itemEndDate = this.CalendarManager.GetItemEndDate(newDate, mode);

                // start with the item date and go outwards
                DateTime? activatableDate = next
                    ? this.GetActivatableDate(newDate, groupEnd, true)
                    : this.GetActivatableDate(groupStart, itemEndDate, false);

                if (null == activatableDate)
                {
                    activatableDate = next == false
                        ? this.GetActivatableDate(newDate, groupEnd, true)
                        : this.GetActivatableDate(groupStart, itemEndDate, false);

                    if (null == activatableDate)
                    {
                        if ((next && groupEnd >= this.MaxDateResolved) || (false == next && groupStart <= this.MinDateResolved))
                            return false;

                        offset += next ? 1 : -1;
                        continue;
                    }
                }

                // we got a date. let's use the first date within that item as 
                // the active date
                activatableDate = this.GetActivatableDate(this.CalendarManager.GetItemStartDate(activatableDate.Value, mode),
                    this.CalendarManager.GetItemEndDate(activatableDate.Value, mode), true);

                Debug.Assert(null != activatableDate);

                bool result = this.ActivateDate(activatableDate);

				if (result)
				{
                    this._preferredGroupOffset = offset;
                    this._preferredGroupNavStart = date;
                }

                return result;
            }
        } 
        #endregion //NavigateGroup

        #region NavigateRow
        private bool NavigateRow(DateTime date, bool next)
        {
            CalendarZoomMode mode = this.CurrentMode;
            int colCount = CalendarManager.GetItemColumnCount(mode);
            int offset = next == false ? -colCount : colCount;

            // it is possible that a date could have a different column based
            // on whether its a leading/trailing date or not. e.g. 1999 is 0
            // when a leading date but 2 when its not a leading/trailing date
            // since active dates shouldn't be in the leading/trailing section
            // we're going to assume its not leading trailing
            int rowOffset, colOffset;
            DateTime groupStart = this.CalendarManager.GetGroupStartDate(date, mode);

            this.CalendarManager.GetItemRowColumn(date, mode, groupStart, false, out colOffset, out rowOffset);

            // if we have a preferred column (because we just scrolled up/down) and this
            // it is not in that column then get the date that would be at that column
            // and calculate the starting date based on that
            if (this._preferredNavColumn >= 0 && colOffset != this._preferredNavColumn)
            {
                date = this.CalendarManager.AddItemOffset(date, this._preferredNavColumn - colOffset, mode);
                colOffset = this._preferredNavColumn;
            }

            while (true)
            {
                // calculate the new date
                DateTime newDate = this.CalendarManager.AddItemOffset(date, offset, mode);

                // calculate the range for that row
                DateTime rowStart = this.CalendarManager.AddItemOffset(newDate, -colOffset, mode);
                DateTime rowEnd = this.CalendarManager.AddItemOffset(newDate, (colCount - colOffset) - 1, mode);

                // we need the end date for that last item in the row
                rowEnd = this.CalendarManager.GetItemEndDate(rowEnd, mode);

                DateTime itemEndDate = this.CalendarManager.GetItemEndDate(newDate, mode);

                // when navigating up (i.e. previous), we want to find a date starting
                // with the item's start date or later up to the end of the row that 
                // we can activate. if navigating down (i.e. next), we want to find
                // a date starting with the row start up to the end of the preferred item.
                // we always want to give preference to a date in the preferred item
                DateTime? activatableDate = next == false
                    ? this.GetActivatableDate(newDate, rowEnd, true)
                    : this.GetActivatableDate(rowStart, itemEndDate, false);

                if (null == activatableDate)
                {
                    // search in the other direction
                    activatableDate = next == false
                        ? this.GetActivatableDate(rowStart, itemEndDate, false)
                        : this.GetActivatableDate(newDate, rowEnd, true);

                    // if there is no date within the row we can activate...
                    if (null == activatableDate)
                    {
                        // if we're at the range we support then there is nothing more to do
                        if ((next && rowEnd >= this.MaxDateResolved) || (false == next && rowStart <= this.MinDateResolved))
                            return false;

                        // otherwise use the calculated date as the starting date for the next loop
                        date = newDate;
                        continue;
                    }
                }

                // we got a date. let's use the first date within that item as 
                // the active date
                activatableDate = this.GetActivatableDate(this.CalendarManager.GetItemStartDate(activatableDate.Value, mode), 
                    this.CalendarManager.GetItemEndDate(activatableDate.Value, mode), true);

                Debug.Assert(null != activatableDate);

                bool result = this.ActivateDate(activatableDate);

                // we want to cache the column offset so we use that as our starting
                // point if we navigate up/down again
				// AS 9/9/09 TFS18434
                //if (result)
                if (result && ShouldCachePreferredNavInfo(mode))
                    this._preferredNavColumn = colOffset;

                return result;
            }
        } 
        #endregion //NavigateRow

        #region NotifyGroupChange
		internal void NotifyGroupChange(CalendarChange change)
        {
            if (null != this._groupForSizing)
                this._groupForSizing.OnCalendarChange(change);

            for (int i = 0, count = this._groups.Count; i < count; i++)
                this._groups[i].OnCalendarChange(change);

			switch(change)
			{
				case CalendarChange.DisabledDatesChanged:
                this.ResolveActiveDate();
					break;
				case CalendarChange.Resources:
					this.ResolveButtonStyles();
					break;
			}
        } 
	    #endregion //NotifyGroupChange

		#region NotifyResourcesChanged

		private void NotifyResourcesChanged()
		{
			if (_notifyResourcesChangedPending == true ||
				_isInitialized == false)
				return;

			this._notifyResourcesChangedPending = true;

			this.Dispatcher.BeginInvoke(new CalendarUtilities.MethodInvoker(ProcessNotifyResourcesChanged));

		}

		#endregion //NotifyResourcesChanged	

		// JJD 3/31/11 - Added helper method
		#region OnCalendarInfoChanged


		private void OnCalendarInfoChanged(XmlLanguage lang)
		{
			CultureInfo culture = null;

			if (_suppliedCalendar == null || _suppliedDateTimeFormat == null)
			{
				try
				{
					culture = lang == null ? CultureInfo.CurrentCulture : lang.GetEquivalentCulture();
				}
				catch (InvalidOperationException)
				{
					culture = CultureInfo.CurrentCulture;
				}
			}






			using (new CalendarItemGroup.GroupInitializationHelper(this._groups))
			{
				this.ClearPreferredNavInfo();

				this._calendarManager.InitializeCalendarInfo(
					this._suppliedCalendar != null ? this._suppliedCalendar : culture.Calendar,
					this._suppliedDateTimeFormat != null ? this._suppliedDateTimeFormat : culture.DateTimeFormat);

				this.ResolveAllDates();
				this.InitializeDaysOfWeek();
				
				this.NotifyGroupChange(CalendarChange.CalendarInfoChanged);
			}
		}

		#endregion //OnCalendarInfoChanged	
    
		// AS 3/23/10 TFS26461
		// Previously the template was using the static DateTime.Today to initialize the today button's 
		// content since there was no property with change notifications exposed by the framework. Also 
		// when the date changed, the IsToday/ContainsToday properties of the items were not updated. 
		// To address this I created a helper class to watch for when the system date changes. We have 
		// to expose the date property so we can get to it in the template.
		//
		#region OnCurrentDateChanged
		private void OnCurrentDateChanged()
		{
			// JJD 10/12/11 - TFS89043 
			// Call InitializeToday which will update the Today property
			// and call TimeManager's AddTimeRange method so this method will
			// get called when the next day occurs.
			this.InitializeToday();

			this.NotifyGroupChange(CalendarChange.TodayChanged);

			// JJD 10/12/11 - TFS89043 
			// this is now done inside InitializeToday method above
			//this.SetValue(TodayPropertyKey, DateTime.Today);

			this.ResolveButtonStyles();
		}
		#endregion //OnCurrentDateChanged

        #region OnDaysOfWeekChanged
        private void OnDaysOfWeekChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CurrentMode == CalendarZoomMode.Days)
                this.ClearPreferredNavInfo();

            this.NotifyGroupChange(CalendarChange.DaysOfWeekChanged);
        }
        #endregion //OnDaysOfWeekChanged

        #region OnDisabledDatesChanged
        private void OnDisabledDatesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyGroupChange(CalendarChange.DisabledDatesChanged);
        } 
        #endregion //OnDisabledDatesChanged


		#region OnLanguageChanged
		private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase cal = (CalendarBase)d;

			XmlLanguage lang = (XmlLanguage)e.NewValue;

			// JJD 3/31/11
			// Moved logic to helper method
			cal.OnCalendarInfoChanged(lang);
		}
		
		#endregion //OnLanguageChanged


		#region OnResourceProviderPropertyChanged

		private void OnResourceProviderPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string propName = e.PropertyName;
			if (propName == "ResourceVersion" ||
				 string.IsNullOrEmpty(propName))
				this.NotifyResourcesChanged();
		}

		#endregion //OnResourceProviderPropertyChanged	
    
		#region ProcessNotifyResourcesChanged

		private void ProcessNotifyResourcesChanged()
		{
			this._notifyResourcesChangedPending = false;

			this.NotifyGroupChange(CalendarChange.Resources);

		}

		#endregion //ProcessNotifyResourcesChanged	
    
        // AS 2/5/09 TFS10681
        // When you click on an item, the selection strategy calls ActivateItem. When that 
        // happens we change the activedate and keep a flag to not bring that date into 
        // view until after the selection strategy is released. When you are zoomed out, 
        // releasing the mouse button on an item will zoom into that item. The problem is 
        // when you click on a leading or trailing item, we were trying to bring that 
        // date into view before we performed/started the zoom operation. This happened 
        // because the selection strategy was getting cleared before the OnMouseUp 
        // of the selection strategy was called. To get around this, we needed to be able 
        // to wait to process the code that was previously in the 
        // OnActiveSelectionStrategyChanged - i.e. to wait before bringing the active
        // date into view. To do that I moved that methods impl into this method and added 
        // a flag that we can use to suppress processing the method. If this method gets 
        // called while it is suppressed then we flip the flag to know that we should 
        // call the method again when clearing the flag. We then set this flag to suppress
        // the processing from our custom selection strategy's OnMouseUp so we can 
        // have the control perform the zoom first. Otherwise we changing the reference 
        // date and therefore the dates in view in the group before we zoom so we zoom 
        // into the wrong dates.
        // 
        #region ProcessActiveSelectionStrategyChanged
        private void ProcessActiveSelectionStrategyChanged()
        {
            if (_shouldProcessActiveSelectionChanged == false)
            {
                _shouldProcessActiveSelectionChanged = true;
                return;
            }

            SelectionStrategyBase strategy = _activeStrategy == null ? null : CoreUtilities.GetWeakReferenceTargetSafe(_activeStrategy) as SelectionStrategyBase;

            bool? oldState = this._shouldBringActiveDateIntoView;

            // we need to track whether to prevent bringing a date into view
            if (null == strategy)
            {
                this._shouldBringActiveDateIntoView = null;
            }
            else if (this._shouldBringActiveDateIntoView == null)
            {
                this._shouldBringActiveDateIntoView = false;
            }

            // if we had delayed bringing a date into view then bring it in now
            // if we don't have an active strategy
            if (oldState == true && this._shouldBringActiveDateIntoView == null)
            {
				if (null != this.ActiveDate)
					this.BringDateIntoView(this.ActiveDate.Value);
            }
        }
        #endregion //ProcessActiveSelectionStrategyChanged

		#region ResolveAllDates
		private void ResolveAllDates()
		{
			using (new CalendarItemGroup.GroupInitializationHelper(this._groups))
			{
				// refresh the resolved min/max dates
				this.ResolveActiveDate();
				this.ResolveReferenceDate();
				this.ResolveMeasureDate();
			}
		}
		#endregion //ResolveAllDates

		#region ResolveButtonStyles

		private void ResolveButtonStyles()
		{
			CalendarResourceProvider rp = this.ResourceProviderResolved;

			if (rp == null)
			{
				this.ResolveResourceProvider();
				return;
			}
			
			Style style = this.ScrollNextRepeatButtonStyle;

			if (style == null)
				style = rp[CalendarResourceId.ScrollNextRepeatButtonStyle] as Style;

			this.ScrollNextRepeatButtonStyleResolved = style;
			
			style = this.ScrollPreviousRepeatButtonStyle;

			if (style == null)
				style = rp[CalendarResourceId.ScrollPreviousRepeatButtonStyle] as Style;

			this.ScrollPreviousRepeatButtonStyleResolved = style;

			style = this.TodayButtonStyle;

			if (style == null)
				style = rp[CalendarResourceId.TodayButtonStyle] as Style;

			this.TodayButtonStyleResolved = style;

			string format = CalendarUtilities.GetString("TodayButtonCaption");

			this.TodayButtonCaption = string.Format(CultureInfo.CurrentCulture.DateTimeFormat, format, DateTime.Today);
		}

		#endregion //ResolveButtonStyles

		#region ResolveResourceProvider

		private void ResolveResourceProvider()
		{
			CalendarResourceProvider rp = this.ResourceProvider;

			if (rp == null)
			{
				rp = this.DefaultResourceProvider;

				if (rp == null)
				{
					rp = new CalendarResourceProvider();
					this._defaultResourceProvider = rp;
				}
			}

			this.ResourceProviderResolved = rp;
		}

		#endregion //ResolveResourceProvider	
    
        #region ScrollGroups

        private bool ScrollGroups(int numberOfGroups)
		{
            Debug.Assert(this._groups.Count > 0);

            if (this._groups.Count == 0)
				return false;

            DateTime date = this.ReferenceDate ?? this.GetDefaultReferenceDate();
            DateTime minDate = this.MinDateResolved;
            DateTime maxDate = this.MaxDateResolved;

			CalendarZoomMode mode = this.CurrentMode;

			// adjust by the specified number of months
			date = this._calendarManager.AddGroupOffset(date, numberOfGroups, mode, true);

			if (date > maxDate)
			{
                // make sure the last month isn't beyond the max date

				// JJD /8/11 - TFS67475
				// Make sure we have a last group before trying to access it
                //DateTime firstMonthForMaxDate = this._calendarManager.AddGroupOffset(this._calendarManager.GetGroupStartDate(maxDate, mode), -this.LastGroup.ReferenceGroupOffset, mode, true);
				DateTime firstMonthForMaxDate = this._calendarManager.GetGroupStartDate(maxDate, mode);
				CalendarItemGroup lastGroup = this.LastGroup;

				if ( LastGroup != null )
					firstMonthForMaxDate = this._calendarManager.AddGroupOffset(firstMonthForMaxDate, - lastGroup.ReferenceGroupOffset, mode, true);

				// if the new day is beyond the first month's date when 
				// 
				if (date > firstMonthForMaxDate)
					date = firstMonthForMaxDate;
			}

			if (date < minDate)
				date = minDate;

            DateTime? oldRefDate = this.ReferenceDate;

			// JJD 1/12/12 - TFS96805
			// Call ResolveReferenceDate instead of setting it directly since the value may need to be coerced
			//this.ReferenceDate = date;
			this.ResolveReferenceDate(date);

            // a scroll happened as long as the reference date changed
            return false == object.Equals(oldRefDate, this.ReferenceDate);
		}
		#endregion //ScrollGroups

		// AS 9/9/09 TFS18434
		// For the modes where the items can be in different columns depending on which group contains 
		// the item, we will not try to maintain the preferred column.
		//
		#region ShouldCachePreferredNavInfo
		private static bool ShouldCachePreferredNavInfo(CalendarZoomMode mode)
		{
			switch (mode)
			{
				case CalendarZoomMode.Centuries:
				case CalendarZoomMode.Decades:
				case CalendarZoomMode.Years:
					return false;
				case CalendarZoomMode.Days:
				case CalendarZoomMode.Months:
					return true;
				default:
					Debug.Assert(false, "Unexpected mode:" + mode.ToString());
					return false;
			}
		} 
		#endregion //ShouldCachePreferredNavInfo

        #region SortGroups
        private void SortGroups()
        {
            // we need to keep the groups sorted based on the dates they will show
            Comparison<CalendarItemGroup> comparison = new Comparison<CalendarItemGroup>(delegate(CalendarItemGroup g1, CalendarItemGroup g2)
            {
                return g1.ReferenceGroupOffset.CompareTo(g2.ReferenceGroupOffset);
            });

            IComparer<CalendarItemGroup> comparer = CoreUtilities.CreateComparer(comparison);

            // we'll use our sort merge to maintain the order in which they were added
            CoreUtilities.SortMergeGeneric<CalendarItemGroup>(this._groups, comparer);
        }
        #endregion //SortGroups

        #region ToggleActiveDateSelection
        private bool ToggleActiveDateSelection(bool shiftKeyDown, bool ctlKeyDown)
        {
            DateTime activeDate = this.ActiveDate.Value;

            CalendarItem item = this.GetItem(activeDate);
            if (null != item)
            {
                SelectionStrategyBase strategy = this.SelectionHost.GetSelectionStrategyForItem(item);

                if (null != strategy && (strategy.IsMultiSelect || strategy.IsSingleSelect))
                {
                    return strategy.SelectItemViaKeyboard(item, shiftKeyDown, ctlKeyDown, true);
                }
            }

            return false;
        }
        #endregion //ToggleActiveDateSelection

        #region UpdateSelectedState

        private void UpdateSelectedState(IList<DateTime> dates, bool select)
        {
            if (dates != null)
            {
                Debug.Assert(this.IsMinCalendarMode);

                object selectValue = KnownBoxes.FromValue(select);

                for (int i = 0, count = dates.Count; i < count; i++)
                {
                    DateTime date = dates[i];
                    CalendarItem selectedDay = this.GetItem(date);

                    if (selectedDay != null)
                    {
                        selectedDay.SetValue(CalendarItem.IsSelectedPropertyKey, selectValue);
                        selectedDay.SetValue(CalendarItem.ContainsSelectedDatesPropertyKey, selectValue);
                    }
                }
            }
        }
        #endregion //UpdateSelectedState

        #region ValidatePositiveInt
		private static bool ValidatePositiveInt(object value)
		{
			return value is int &&
				(int)value > 0;
		} 
		#endregion //ValidatePositiveInt

		#region VerifyWeekSelectionMode

		internal void VerifyWeekSelectionMode()
		{

			if ( CoreUtilities.Antirecursion.InProgress(this, "VerifyWeekSelectionMode"))
				return;

			CoreUtilities.Antirecursion.Enter(this, "VerifyWeekSelectionMode", true);

			try
			{
				int offset;
				bool selectFullWeeks = false;
				DateCollection selectedDates = this.SelectedDatesInternal;
				int count = selectedDates.Count;

				if (this.SupportsWeekSelectionMode && this.IsMinCalendarMode)
				{
					if (count > 8)
						selectFullWeeks = true;
					else if (count > 6 )
						selectFullWeeks = _pivotRange != null && _pivotRange.Value.End > _pivotRange.Value.Start;
				}

				if (selectFullWeeks == false)
					return;

				selectedDates.Sort();

				DayOfWeek? firstDOW = this.FirstDayOfWeekInternal;

				DateTime dtFirst = selectedDates[0];
				DateTime dtFirstinWeek = CalendarManager.GetFirstDayOfWeekForDate(dtFirst, firstDOW, out offset);

				DateTime dtLast = selectedDates[count - 1];
				DateTime dtLastInWeek = CalendarManager.GetFirstDayOfWeekForDate(dtLast, firstDOW, out offset);
				dtLastInWeek = _calendarManager.AddDays(dtLastInWeek, 6 + offset);

				if (dtFirst > dtFirstinWeek)
				{
					// JJD 3/31/11 - TFS69037
					// Use calendar managers AddDays, which handles min and max dates without throwing an exception,
					// and only add the range if the returned value is still applicable 
					//selectedDates.AddRange(new DateRange(dtFirstinWeek, dtFirst.AddDays(-1)), true);
					DateTime adjusteddate = _calendarManager.AddDays(dtFirst, -1);

					
					
					
					if ( adjusteddate >= dtFirstinWeek )
						selectedDates.AddRange(new DateRange(dtFirstinWeek, adjusteddate), true);
				}

				if (dtLast < dtLastInWeek)
				{
					// JJD 3/31/11 - TFS69037
					// Use calendar managers AddDays, which handles min and max dates without throwing an exception,
					// and only add the range if the returned value is still applicable 
					//selectedDates.AddRange(new DateRange(dtLast.AddDays(1), dtLastInWeek), true);
					DateTime adjusteddate = _calendarManager.AddDays(dtLast, 1);

					
					
					
					if ( adjusteddate <= dtLastInWeek )
						selectedDates.AddRange(new DateRange( adjusteddate, dtLastInWeek), true);
				
				}
			}
			finally
			{
				CoreUtilities.Antirecursion.Exit(this, "VerifyWeekSelectionMode");
			}
		}

		#endregion //VerifyWeekSelectionMode	
    
		#endregion //Private

		#endregion //Methods

		#region Events

        #region SelectedDatesChanged

        /// <summary>
        /// Occurs after the <see cref="CalendarBase.SelectedDates"/> has been changed.
        /// </summary>
        /// <seealso cref="SelectedDatesChanged"/>
        /// <seealso cref="SelectedDatesChangedEventArgs"/>
        protected virtual void OnSelectedDatesChanged(SelectedDatesChangedEventArgs args)
        {
			if (this.SelectedDatesChanged != null)
				this.SelectedDatesChanged(this, args);
			
			if (this.SelectedDatesInternalChanged != null)
				this.SelectedDatesInternalChanged(this, args);
       }

		internal void RaiseSelectedDatesChanged(IList<DateTime> unselected, IList<DateTime> selected, bool force)
        {
			if ( this._mouseDownSnapshot == null || force )
				this.OnSelectedDatesChanged(new SelectedDatesChangedEventArgs(unselected, selected));
			else
			if (this.SelectedDatesInternalChanged != null)
				this.SelectedDatesInternalChanged(this, new SelectedDatesChangedEventArgs(unselected, selected));
        }

        /// <summary>
        /// Occurs after the <see cref="CalendarBase.SelectedDates"/> has been changed.
        /// </summary>
        /// <seealso cref="OnSelectedDatesChanged"/>
         /// <seealso cref="SelectedDatesChangedEventArgs"/>
        /// <seealso cref="SelectedDates"/>
        /// <seealso cref="SelectedDate"/>
        /// <seealso cref="SelectionType"/>
        //[Description("Occurs after the SelectedDates has been changed.")]
        //[Category("Calendar Properties")] // Behavior
        public event EventHandler<SelectedDatesChangedEventArgs> SelectedDatesChanged;
        
		internal event EventHandler<SelectedDatesChangedEventArgs> SelectedDatesInternalChanged;

        #endregion //SelectedDatesChanged

		// AS 3/24/10 TFS28062
		// Renamed SelectionMouseUpEvent to SelectionCommitted to be more indicative of the usage.
		//
        #region SelectionCommittedEvent

        /// <summary>
        /// Occurs after the mouse is release after a selection.
        /// </summary>
        private void OnSelectionCommitted(EventArgs args)
        {
			if (this.SelectionCommitted != null)
				this.SelectionCommitted(this, args);
		}

        internal void RaiseSelectionCommitted(EventArgs args)
        {
            this.OnSelectionCommitted(args);
        }

        /// <summary>
        /// Occurs after the mouse is release after a selection.
        /// </summary>
        /// <seealso cref="OnSelectionCommitted"/>
        internal event EventHandler SelectionCommitted;

        #endregion //SelectionCommittedEvent

        #endregion //Events

        #region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the control's template has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();








			this.ResolveCurrentMode();

			// AS 10/15/09 TFS23543
			this.ResolveAllDates();

			#region RootPanel

			PresentationUtilities.ReparentElement(_templatePanel, _groupForSizing, false);
			PresentationUtilities.ReparentElement(_rootPanel, _templatePanel, false);

			// JJD 07/19/12 - TFS108812
			// Walk over the children of the old root panel and null out the 
			// backward ref to the Calendar. This should filter down thru the
			// groupd and calendar items to prevent rooting of old elements
			if (_rootPanel != null)
			{
				foreach (UIElement child in _rootPanel.Children)
				{
					ICalendarElement calElement = child as ICalendarElement;

					if (calElement != null)
						CalendarBase.SetCalendar(child, null);
				}
			}

			_rootPanel = this.GetTemplateChild(PartRootPanel) as Panel;

			if (_templatePanel == null)
			{
				_templatePanel = new MeasurePanel();
				// i was going to set the name but Silverlight throws a value not within expected range exception
				//_templatePanel.Name = MeasurePanelName;
				_templatePanel.Tag = MeasurePanelId;

				// make sure it doesn't render or hittest
				_templatePanel.Width = 0;
				_templatePanel.Height = 0;
				_templatePanel.IsHitTestVisible = false;
				_templatePanel.RenderTransform = new TranslateTransform { X = -1000, Y = -1000 };
				_templatePanel.Clip = new System.Windows.Media.RectangleGeometry();
			}

			PresentationUtilities.ReparentElement(_rootPanel, _templatePanel, true);

			//if (_templateItems == null || _templateItems.Count == 0)
			//    this.InitializeTemplatePanel(_templatePanel);

			// JJD 04/20/12 - TFS110183
			// Instead of reparenting the old _groupForSizing just null it out since a new one
			// will get created and registered by the new CalendarItemGroupPanel in the new template
			//PresentationUtilities.ReparentElement(_templatePanel, _groupForSizing, true);
			_groupForSizing = null;

			// JJD 04/20/12 - TFS110183
			// Remove any groups that are no longer our descendants, which should be all of then
			if (_groups != null)
			{
				for (int i = _groups.Count - 1; i >= 0; i--)
				{
					if (!PresentationUtilities.IsAncestorOf(this, _groups[i]))
						_groups.RemoveAt(i);
				}

				Debug.Assert(_groups.Count == 0, "All the old groups should have been pulled out of the Calendar's visual tree in OnApplyTemplate");
			}

			#endregion // RootPanel
		} 
		#endregion //OnApplyTemplate

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="CalendarBase"/> Automation Peer Class <see cref="CalendarAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CalendarBaseAutomationPeer(this);
        }

        #endregion //OnCreateAutomationPeer


        #region OnGotKeyboardFocus
        /// <summary>
		/// Invoked when the element receives keyboard focus.
		/// </summary>
		/// <param name="e">Provides data about the event</param>
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);

            if (e.NewFocus == this)
            {
                //DependencyObject oldFocus = e.OldFocus as DependencyObject;

                //if (null != oldFocus && this.IsAncestorOf(oldFocus))
                //    return;

                
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

                //this.FocusActiveItem();
            }
        }
        #endregion //OnGotKeyboardFocus
        
		#region OnIsKeyboardFocusedChanged
        /// <summary>
        /// Invoked when the IsKeyboardFocused property is changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnIsKeyboardFocusedChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusedChanged(e);


            this.CoerceValue(KeyboardNavigation.IsTabStopProperty);
        } 
        #endregion //OnIsKeyboardFocusedChanged

        #region OnIsKeyboardFocusWithinChanged
        /// <summary>
        /// Invoked when the IsKeyboardFocusWithin property is changed.
        /// </summary>
        /// <param name="e">Provides information about the change</param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            this.CoerceValue(KeyboardNavigation.IsTabStopProperty);

            // AS 9/24/08 TFS7577
            // We need an isselectionactive type property so the selection can change
            // when focus is lost from the control.
            //
            bool isSelectionActive = (bool)e.NewValue;

            if (false == isSelectionActive)
            {
                DependencyObject focusedElement = Keyboard.FocusedElement as DependencyObject;

                // if focus goes into another focus scope (e.g. menu/toolbar/ribbon) in the same
                // window then do not consider the element to have lost focus
                if (null != focusedElement && 
                    FocusManager.GetFocusScope(focusedElement) != FocusManager.GetFocusScope(this) &&
                    Window.GetWindow(focusedElement) == Window.GetWindow(this))
                {
                    isSelectionActive = true;
                }
            }

            this.SetValue(IsSelectionActivePropertyKey, KnownBoxes.FromValue(isSelectionActive));
        } 
        #endregion //OnIsKeyboardFocusWithinChanged


#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)


		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			CalendarBase cal = CalendarBase.GetCalendar(this);

			if (cal == null)
				return;

			bool altKeyDown = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;

			if (altKeyDown)
				return;

			bool shiftKeyDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
			bool ctlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

			CalendarCommandType? commandType = null;

			switch (e.Key)
			{
				case Key.Add:
					if (ctlKeyDown)
						commandType = CalendarCommandType.ZoomOutCalendarMode;
					break;
				case Key.Subtract:
					// JJD 12/5/11 - TFS96831
					// Zoom i when a '-' is pressed with the 'Ctrl' key down
					if (ctlKeyDown)
						commandType = CalendarCommandType.ZoomInCalendarMode;
					break;
				case Key.Down:
					commandType = CalendarCommandType.NextItemRow;
					break;
				case Key.End:
					if (ctlKeyDown)
						commandType = CalendarCommandType.LastItemOfLastGroup;
					else
						commandType = CalendarCommandType.LastItemOfGroup;
					break;
				case Key.Enter:
					if (ctlKeyDown)
						commandType = CalendarCommandType.ZoomOutCalendarMode;
					else
						commandType = CalendarCommandType.ZoomInCalendarMode;
					break;
				case Key.Home:
					if (ctlKeyDown)
						commandType = CalendarCommandType.FirstItemOfFirstGroup;
					else
						commandType = CalendarCommandType.FirstItemOfGroup;
					break;
				case Key.Left:
					commandType = CalendarCommandType.PreviousItem;
					break;
				case Key.PageUp:
					commandType = CalendarCommandType.PreviousGroup;
					break;
				case Key.PageDown:
					commandType = CalendarCommandType.NextGroup;
					break;
				case Key.Right:
					commandType = CalendarCommandType.NextItem;
					break;
				case Key.Space:
					// JJD 12/5/11 - TFS96831
					if (ctlKeyDown == false && shiftKeyDown == false && (Keyboard.Modifiers & ModifierKeys.Alt) == 0 &&
						this.ActiveDate.HasValue && this.CurrentMode != this.MinCalendarModeResolved )
					{
						commandType = CalendarCommandType.ZoomInCalendarMode;
						break;
					}

					// JJD 12/5/11 - TFS96831
					if (shiftKeyDown == false && (Keyboard.Modifiers & ModifierKeys.Alt) == 0 )
						commandType = CalendarCommandType.ToggleActiveDateSelection;

					break;
				case Key.Up:
					commandType = CalendarCommandType.PreviousItemRow;
					break;
			}

			if (commandType.HasValue)
				e.Handled = cal.ExecuteCommand(commandType.Value, null, this);

			base.OnKeyDown(e);
		}

		#endregion //OnKeyDown	

		#region OnInitialized


		/// <summary>
		/// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)



		{

			base.OnInitialized(e);


			if (_isInitialized)
				return;

			_isInitialized = true;

			this.ResolveResourceProvider();
			this.ResolveAllDates();
			this.InitializeDaysOfWeek();

		}
		#endregion //OnInitialized

		#region OnLostMouseCapture

		/// <summary>
		/// Called when mouse capture is lost
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnLostMouseCapture(System.Windows.Input.MouseEventArgs e)
		{
			// SSP 5/20/09 TFS17816
			// LostMouseCapture is a bubble event. If some other element inside the data presenter
			// had capture (for example the label presenter) and the capture is given to the data 
			// presenter, the OnLostMouseCapture will get called on the data presenter. However 
			// this doesn't mean that the data presenter has lost capture, especially if the 
			// capture is being given to it currently.
			// 
			// ------------------------------------------------------------------------------------
			//this.CancelPendingMouseOperations();

			RaisePendingSelectionChanged();

			if (e.OriginalSource == this

				|| !this.IsMouseCaptured)



			{
				this.CancelPendingMouseOperations();
			}
			// ------------------------------------------------------------------------------------

			base.OnLostMouseCapture(e);
		}

		#endregion //OnLostMouseCapture

		#region OnMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnMouseLeftButtonDown(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);



#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


			if (this._selectionController != null && !e.Handled)
			{
				// JJD 11/10/11 - TFS95841
				// Moved logic into SnapshotSelection method
				//// take a snapshot of the current selection so we can delay raising the SelectionChanged
				//// event until the mouse is released
				//this._mouseDownSnapshot = new SelectionSnapshot(this);
				
				//this._mouseDownSnapshot.Initialize(this._selectedDates);
				this.SnapshotSelection();

				this._selectionController.OnMouseLeftButtonDown(e);

				// if the mouse wasn't over a selectable item then clea the snapshot created above
				if (!e.Handled)
				{
					// JJD 07/16/12 - TFS99856
					// Instead of clearing the snapshot, process it in case a partial range
 					// selection was made
					//this._mouseDownSnapshot = null;
					this.RaisePendingSelectionChanged();
				}
			}
		}

		#endregion //OnMouseLeftButtonDown

		#region OnMouseMove

		/// <summary>
		/// Called when the mouse is moved
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnMouseMove(System.Windows.Input.MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if (this._selectionController != null

				&& !e.Handled)



			{
				this._selectionController.OnMouseMove(e);

			}
		}

		#endregion //OnMouseMove

		#region OnMouseLeftButtonUp

		/// <summary>
		/// Called when the left mouse button is released
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnMouseLeftButtonUp(System.Windows.Input.MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonUp(e);

			this.RaisePendingSelectionChanged();

			if (this._selectionController != null && !e.Handled)
			{
				this._selectionController.OnMouseLeftButtonUp(e);

				if (e.Handled)
					return;
			}
		}

		#endregion //OnMouseLeftButtonUp	

        #region OnMouseWheel
        /// <summary>
        /// Raised when the mouse wheel is rotated while over the control.
        /// </summary>
        /// <param name="e">Provides information about the event</param>
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            if (false == e.Handled)
            {
				CalendarCommandType cmd = e.Delta < 0 
                    ? CalendarCommandType.ScrollNextGroups 
                    : CalendarCommandType.ScrollPreviousGroups;

                e.Handled = this.ExecuteCommandImpl(cmd, Math.Abs(e.Delta / CalendarUtilities.MouseWheelScrollDelta), this);
            }
        } 
        #endregion //OnMouseWheel

		#region OnPreviewMouseLeftButtonDown

		/// <summary>
		/// Called when the left mouse button is pressed
		/// </summary>
		/// <param name="e">The event arguments</param>
		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseLeftButtonDown(e);

			// give the control focus if it doesn't already have it
			if (!e.Handled &&
				!this.IsKeyboardFocused &&
				!this.IsKeyboardFocusWithin &&
				this.Focusable &&
				this.IsEnabled
				// AS 9/2/09 TFS19928
				// IsKeyboardFocusWithin may be false but focus may logically be within 
				// the control and so we shouldn't steal focus from the focused element.
				//
				&& !PresentationUtilities.IsAncestorOf(this, Keyboard.FocusedElement as Visual)
				)
				this.Focus();
		}

		#endregion //OnPreviewMouseLeftButtonDown	

		#endregion //Base class overrides

		#region ISelectionHost Members

		#region ActivateItem

        // JJD 7/14/09 - TFS18784 
        // Added preventScrollItemIntoView param
        bool ISelectionHost.ActivateItem(ISelectableItem item, bool preventScrollItemIntoView)
		{
			if (item is CalendarWeekNumber)
				return true;

			CalendarItem calItem = this.GetItem(item);

			if (null != calItem && calItem.IsEnabled)
			{
                // get first activatable date in group to use as the activation date
                DateTime? activatableDate = this.GetActivatableDate(calItem);

                if (null == activatableDate)
                    return false;

                DateTime date = activatableDate.Value;
				this.ActiveDate = date;

                // this date might have been out of view so get the item again
                calItem = this.GetItem(date);

				return null != calItem && calItem.IsActive;
			}

			return false;
		} 
		#endregion //ActivateItem

		#region ClearSnapshot

		void ISelectionHost.ClearSnapshot()
		{
            Debug.Assert(this.IsMinCalendarMode);

            this._selectedDatesSnapshot.Clear();
		} 
		#endregion //ClearSnapshot

		#region DeselectItem

		bool ISelectionHost.DeselectItem(ISelectableItem item)
		{
            if (this.IsNavigationMode)
            {
                Debug.Assert(false, "Why are we trying to deactivate a navigation item?");
                return false;
            }

			bool rtn = this.InternalSelectItem(item, false, false);

			
			
			

			return rtn;
		} 
		#endregion //DeselectItem

		#region DeselectRange

		bool ISelectionHost.DeselectRange(ISelectableItem item)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return false;

			bool rtn = this.InternalSelectRange(item, false, false);

			this.VerifyWeekSelectionMode();

			return rtn;
		} 
		#endregion //DeselectRange

		#region DoAutoScrollHorizontal
		void ISelectionHost.DoAutoScrollHorizontal(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed)
		{
            if (direction == ScrollDirection.Decrement)
                this.ExecuteCommand(CalendarCommandType.ScrollPreviousGroup, null, null);
            else
                this.ExecuteCommand(CalendarCommandType.ScrollNextGroup, null, null);
        } 
		#endregion //DoAutoScrollHorizontal

		#region DoAutoScrollVertical

		void ISelectionHost.DoAutoScrollVertical(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed)
		{
            if (direction == ScrollDirection.Decrement)
                this.ExecuteCommand(CalendarCommandType.ScrollPreviousGroup, null, null);
            else
                this.ExecuteCommand(CalendarCommandType.ScrollNextGroup, null, null);
        } 
		#endregion //DoAutoScrollVertical

		#region EnterSnakingMode

		void ISelectionHost.EnterSnakingMode(ISelectableItem item)
		{
		} 
		#endregion //EnterSnakingMode

		#region GetAutoScrollInfo

		AutoScrollInfo ISelectionHost.GetAutoScrollInfo(ISelectableItem item)
		{
			CalendarStates currentState = this.CurrentState;
            SelectionStrategyBase strategy = this.SelectionHost.GetSelectionStrategyForItem(item);

            // only scroll during single select
            bool scrollingAllowed = null != strategy && strategy.IsSingleSelect == true && strategy.IsMultiSelect == false;
            bool scrollPrevious = scrollingAllowed && (currentState & CalendarStates.MinDateInView) == 0;
            bool scrollNext = scrollingAllowed && (currentState & CalendarStates.MaxDateInView) == 0;

            AutoScrollInfo scrollInfo = new AutoScrollInfo(this,
				scrollPrevious,
				scrollPrevious,
                scrollNext,
				scrollNext);

            // if the mouse is such that we can scroll horizontally and vertically, 
            // we want to prefer horizontal regardless of the distance from the horizontal
            // edge vs the distance to the vertical edge since that is what the
            // monthcalendar does
            scrollInfo.ScrollOrientation = ScrollOrientation.Horizontal;

            return scrollInfo;
		} 
		#endregion //GetAutoScrollInfo

		#region GetNearestCompatibleItem
		ISelectableItem ISelectionHost.GetNearestCompatibleItem(ISelectableItem item, MouseEventArgs e)
		{
			Debug.Assert(item != null);

			if (item == null)
				return null;

			CalendarItem sourceItem = item as CalendarItem;

			Point pt = e.GetPosition(this);



			
#region Infragistics Source Cleanup (Region)






















































#endregion // Infragistics Source Cleanup (Region)

			CalendarItem calItem = PresentationUtilities.GetVisualDescendantFromPoint<CalendarItem>(this, pt, null);

            #region Over CalendarItem

			// make sure the element may be selected
			if (calItem != null)
			{
                ISelectableItem selectableItem = ((ISelectableElement)calItem).SelectableItem;

                if (false == this.SelectionHost.IsItemSelectableWithCurrentSelection(selectableItem))
                {
                    return null;
                }

                // if the mouse is over a disabled day then find the 
                // closest enabled date between it and the pivot date
                if (calItem.IsEnabled == false && this._pivotRange != null)
                {
                    DateTime? dateToSelect;

                    if (this._pivotRange.Value.End > calItem.StartDate)
                    {
                        dateToSelect = GetActivatableDate(calItem.EndDate, this._pivotRange.Value.End, true);
                    }
                    else
                    {
                        dateToSelect = GetActivatableDate(this._pivotRange.Value.Start, calItem.StartDate, false);
                    }

                    if (null != dateToSelect)
                        calItem = this.GetItem(dateToSelect.Value);
                }

                return calItem; 
            }
            #endregion //Over CalendarItem

			if (this.SupportsWeekSelectionMode)
			{
				CalendarWeekNumber wkNumber = PresentationUtilities.GetVisualDescendantFromPoint<CalendarWeekNumber>(this, pt, null);

				if (wkNumber != null)
					return wkNumber;
			}

			

			
			
			CalendarItemGroup group =PresentationUtilities.GetVisualDescendantFromPoint<CalendarItemGroup>(this, pt, null);

           #region Before First Item/After Last Item
            // we're over a group so find the closest item
            if (null != sourceItem && null != group && group.Items.Count > 0)
            {
                return this.GetFirstLastItem(sourceItem, pt, group);
            }
            #endregion //Before First Item/After Last Item

            return null;
		}
		#endregion //GetNearestCompatibleItem

		#region GetPivotItem

		ISelectableItem ISelectionHost.GetPivotItem(ISelectableItem item)
		{
			Debug.Assert(false, "Why do we need this? What if the pivot date is out of view?");

			if (null != this._pivotRange)
			{
				if (_pivotRange.Value.Start == _pivotRange.Value.End)
				{
					CalendarItem calendarItem = this.GetItem(this._pivotRange.Value.Start);

					Debug.Assert(calendarItem != null, "The day of the pivot date is not in view!");

					return calendarItem;
				}
				else
				{
					CalendarWeekNumber week = this.GetWeekNumber(this._pivotRange.Value.Start);

					if ( week == null )
						week = this.GetWeekNumber(this._pivotRange.Value.End);

					return week;
				}
			}


			return null;
		} 
		#endregion //GetPivotItem

		#region GetSelectionStrategyForItem

		SelectionStrategyBase ISelectionHost.GetSelectionStrategyForItem(ISelectableItem item)
		{
            SelectionStrategyBase strategy = null;

			SelectionType type = this.CurrentSelectionTypeResolved;

            // we need to use a custom selection strategy to control the scroll interval
            if (type == SelectionType.Single && this._selectionStrategy is SelectionStrategySingle == false)
                strategy = this._selectionStrategy = new CustomSingleSelectionStrategy(this);
            else
                strategy = this._selectionStrategy = SelectionStrategyBase.GetSelectionStrategy(type, this.SelectionHost, this._selectionStrategy);
 
			return strategy;
		} 
		#endregion //GetSelectionStrategyForItem

		#region IsItemSelectableWithCurrentSelection

		bool ISelectionHost.IsItemSelectableWithCurrentSelection(ISelectableItem item)
		{
			if (_pivotRange != null && item != null)
			{
				if (_pivotRange.Value.Start == _pivotRange.Value.End)
					return item is CalendarItem;
			}

			//Debug.Assert(item is CalendarItem);
			return item is CalendarItem || (this.SupportsWeekSelectionMode && item is CalendarWeekNumber);
		} 
		#endregion //IsItemSelectableWithCurrentSelection

		#region IsMaxSelectedItemsReached

		bool ISelectionHost.IsMaxSelectedItemsReached(ISelectableItem item)
		{
            if (this.CurrentMode != this.MinCalendarModeResolved)
                return false;

			int maxSelectedDates = this.MaxSelectedDatesResolved;

			return this.SelectedDatesInternal.Count >= maxSelectedDates;
		} 
		#endregion //IsMaxSelectedItemsReached

		#region OnActiveSelectionStrategyChanged
		private void OnActiveSelectionStrategyChanged(object sender, EventArgs e)
		{
			this.OnActiveSelectionStrategyChanged(this._selectionController.ActiveStrategy);
		}

		/// <summary>
		/// Invoked when the active <see cref="SelectionStrategyBase"/> used by the control has been changed.
		/// </summary>
		/// <param name="strategy">The new active strategy or null if no strategy is currently being used.</param>
		private void OnActiveSelectionStrategyChanged(SelectionStrategyBase strategy)
		{
			
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

			_activeStrategy = strategy == null ? null : new WeakReference(strategy);
			this.ProcessActiveSelectionStrategyChanged();
		}

		#endregion //OnActiveSelectionStrategyChanged

		#region OnDrag

		void ISelectionHost.OnDragEnd(bool canceled)
		{
		}

		void ISelectionHost.OnDragMove(MouseEventArgs e)
		{
		}

		bool ISelectionHost.OnDragStart(ISelectableItem item, MouseEventArgs e)
		{
			return false;
		}

		#endregion //OnDrag

		#region OnMouseUp

		void ISelectionHost.OnMouseUp(MouseEventArgs e)
		{
			MouseButtonEventArgs buttonArgs = e as MouseButtonEventArgs;

			// if the left mouse button was released...
			if (buttonArgs != null

				&& buttonArgs.ChangedButton == MouseButton.Left)



			{
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)



				Point pt = e.GetPosition(this);



				CalendarItem item = PresentationUtilities.GetVisualDescendantFromPoint<CalendarItem>(this, pt, null);

				if (null != item)
				{
                    if (this.CurrentMode > this.MinCalendarModeResolved)
                        this.ExecuteCommandImpl(CalendarCommandType.ZoomInCalendarMode, item, item);
                    else
                    {
						// AS 3/24/10 TFS28062
						// Changed name to SelectionCommitted instead of SelectionMouseUp
						//
						//this.RaiseSelectionMouseUp(new RoutedEventArgs());
						this.RaiseSelectionCommitted(new RoutedEventArgs());
                    }
				}
			}
		} 
		#endregion //OnMouseUp

		#region RootElement

		FrameworkElement ISelectionHost.RootElement
		{
			get { return this; }
		} 
		#endregion //RootElement

		#region SelectItem

		bool ISelectionHost.SelectItem(ISelectableItem item, bool clearExistingSelection)
		{
            if (this.IsNavigationMode)
                return this.SelectionHost.ActivateItem(item, false);

			bool rtn = this.InternalSelectItem(item, clearExistingSelection, true);

			
			
			

			return rtn;
		} 
		#endregion //SelectItem
		#region SelectRange

		bool ISelectionHost.SelectRange(ISelectableItem item, bool clearExistingSelection)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return false;

			bool rtn = this.InternalSelectRange(item, clearExistingSelection, true);
	
			this.VerifyWeekSelectionMode();

			return rtn;
	} 
		#endregion //SelectRange

		#region SetPivotItem

		void ISelectionHost.SetPivotItem(ISelectableItem item, bool isRangeSelect)
		{
			DateRange? oldPivotRange = this._pivotRange;

			// clear out the old pivot if the types aren't the same
			if ( item == null )
				oldPivotRange = null;
			else
			if (oldPivotRange != null)
			{
				if (oldPivotRange.Value.Start == oldPivotRange.Value.End)
				{
					if (!(item is CalendarItem))
						oldPivotRange = null;
				}
				else
				{
					if (!(item is CalendarWeekNumber))
						oldPivotRange = null;
				}
			}

			// Set pivotRcd item if shift key is not pressed.
			// Also set if we don't have a pivotRcd item (even if shift is pressed).
			if (isRangeSelect && oldPivotRange != null)
				return;

			this._pivotRange = GetRangeFromItem(item);
		}

		#endregion //SetPivotItem

		#region SnapshotSelection

		void ISelectionHost.SnapshotSelection(ISelectableItem item)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return;

            // reset the snapshot based on the current selection
			this._selectedDatesSnapshot.ReInitialize(this.SelectedDatesInternal);



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		} 
		#endregion //SnapshotSelection

		#region TranslateItem

		ISelectableItem ISelectionHost.TranslateItem(ISelectableItem item)
		{
            if (item is CalendarItem == false && item is CalendarWeekNumber == false)
                return null;

			return item;
		} 
		#endregion //TranslateItem

		#endregion //ISelectionHost

        #region CustomSingleSelectionStrategy class
        private class CustomSingleSelectionStrategy : SelectionStrategySingle
        {
            #region Member Variables

            private const int ScrollInterval = 250;

            #endregion //Member Variables

            #region Constructor
            internal CustomSingleSelectionStrategy(ISelectionHost host)
                : base(host)
            {
				// JJD 12/1/11 - TFS96941
				// Set the new AllowToggle property to false since we don't want that behavior
				this.AllowToggle = false;
			} 
            #endregion //Constructor

            #region Base class overrides

            #region AutoScrollXXXInterval
            protected override int AutoScrollHorizontalIntervalMax
            {
                get
                {
                    return ScrollInterval;
                }
            }

            protected override int AutoScrollHorizontalIntervalMin
            {
                get
                {
                    return ScrollInterval;
                }
            }

            protected override int AutoScrollVerticalIntervalMax
            {
                get
                {
                    return ScrollInterval;
                }
            }

            protected override int AutoScrollVerticalIntervalMin
            {
                get
                {
                    return ScrollInterval;
                }
            }
            #endregion //AutoScrollXXXInterval

            // AS 2/5/09 TFS10681
            // During the processing of the mouse up, we will release capture. When that 
            // happens the active selection strategy is going to null and the 
            // OnActiveSelectionStrategyChagned  is being called. This tries to bring 
            // the active date into view which is causing us to change the reference 
            // date and therefore the date from which we will zoom in. To get around 
            // this we'll suppress the processing of the OnActiveSelectionStrategyChanged 
            // while we're calling the OnMouseLeftButtonUp.
            //
            #region OnMouseLeftButtonUp
            public override void OnMouseLeftButtonUp(ISelectableItem item, MouseEventArgs e)
            {
                CalendarBase cal = this.SelectionHost as CalendarBase;

                if (null != cal)
                {
                    Debug.Assert(cal._shouldProcessActiveSelectionChanged == null);
                    cal._shouldProcessActiveSelectionChanged = false;
                }

                try
                {
                    base.OnMouseLeftButtonUp(item, e);
                }
                finally
                {
                    if (null != cal)
                    {
                        bool shouldProcess = cal._shouldProcessActiveSelectionChanged == true;
                        cal._shouldProcessActiveSelectionChanged = null;

                        if (shouldProcess)
                            cal.ProcessActiveSelectionStrategyChanged();
                    }
                }
            } 
            #endregion //OnMouseLeftButtonUp

            // the default threshold of 20 means that you can be over
            // a day and you will still be scrolling. we'll use a smaller
            // value
            #region PixelThresholdForXXXAutoScroll
            protected override int PixelThresholdForHorizontalAutoScroll
            {
                get
                {
                    return 5;
                }
            }

            protected override int PixelThresholdForVerticalAutoScroll
            {
                get
                {
                    return 5;
                }
            }
            #endregion //PixelThresholdForXXXAutoScroll

            #endregion //Base class overrides
        }
        #endregion //CustomSingleSelectionStrategy class

		#region SelectionSnapshot

		private class SelectionSnapshot
		{
			private CalendarBase _owner;
			// JJD 1/25/11
			// While the mouse is down (during a selection drag operation we don't want to update the real
			// selected dates collection so we need to clone it and use a temporary collection in its place.
			// This is to prevent the selected dates from changing until the mouse is released
			private SelectedDateCollection _selectedDates;

			#region Constructor

			internal SelectionSnapshot(CalendarBase owner)
			{
				this._owner = owner;
			}

			#endregion //Constructor

			internal void Initialize(DateCollection currentSelection)
			{
				this._selectedDates = new SelectedDateCollection(_owner);
				this._selectedDates.Reinitialize(currentSelection);
				this._selectedDates.Sort();
			}

			#region SelectedDates

			internal SelectedDateCollection SelectedDates { get { return _selectedDates; } }

			#endregion //SelectedDates	
    
			#region ProcessEndSelection

			internal void ProcessEndSelection( )
			{

				// JJD 3/30/11 - TFS68023
				// First check to see is the selection has changed since the snapshot was taken. If it hasn't
				// then bail.
				if (_selectedDates.Count == _owner.SelectedDates.Count)
				{
					bool selectionisTheSame = true;
					for (int i = 0, count = _owner.SelectedDates.Count; i < count; i++)
					{
						if (_owner.SelectedDates[i] != _selectedDates[i])
						{
							selectionisTheSame = false;
							break;
						}
					}

					if (selectionisTheSame)
						return;
				}

				DateCollection newSelection = new DateCollection();
				newSelection.Reinitialize(new List<DateTime>(this._selectedDates));
				
				
				
				

				HashSet<DateTime> oldSelection = new HashSet<DateTime>(_owner.SelectedDates);

				List<DateTime> itemsAdded = new List<DateTime>(newSelection.Count);

				// walk over each date in the new selection. If it existed in the old 
				// collection at the start then just remove 
				// it from the hash set since it shouldn't
				// be included in the items added or removed lists. If not then is
				// should be in the added list.
				foreach (DateTime dt in newSelection)
				{
					if (oldSelection.Contains(dt))
						oldSelection.Remove(dt);
					else
						itemsAdded.Add(dt);
				}

				if (itemsAdded.Count == 0 &&
					oldSelection.Count == 0)
					return;

				// update the public DateCollection
				_owner.SelectedDates.Reinitialize(newSelection);

				List<DateTime> oldList =  new List<DateTime>(oldSelection);

				
				oldList.Sort();

				// JJD 11/10/11 - TFS95841
				// Clear the snapshot member before we raise the event since the 
				// selection can be manipulated synchronously by a listener
				_owner._mouseDownSnapshot = null;

				// finally raise the event
				_owner.RaiseSelectedDatesChanged(oldList, itemsAdded, true);
			}

			#endregion //ProcessEndSelection
		}

		#endregion //SelectionSnapshot	
    
		#region MeasurePanel class
		internal class MeasurePanel : Canvas
		{
			#region Base class overrides
			protected override AutomationPeer OnCreateAutomationPeer()
			{
				return new MeasurePanelAutomationPeer(this);
			}
			#endregion // Base class overrides

			#region MeasurePanelAutomationPeer
			private class MeasurePanelAutomationPeer : FrameworkElementAutomationPeer
			{
				internal MeasurePanelAutomationPeer(MeasurePanel owner)
					: base(owner)
				{
				}

				protected override List<AutomationPeer> GetChildrenCore()
				{
					// we don't want the measure items to be included
					return new List<AutomationPeer>();
				}

				protected override bool IsOffscreenCore()
				{
					return true;
				}
			}
			#endregion // MeasurePanelAutomationPeer
		}
		#endregion // MeasurePanel class

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return source is CalendarCommandSource ? this : null;
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return command is CalendarCommand;
		}

		#endregion

		#region ISupportInitialize Members


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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