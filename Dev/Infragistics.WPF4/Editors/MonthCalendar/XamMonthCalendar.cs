using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Commands;
using System.Windows.Input;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Windows.Helpers;
using System.Globalization;
using Infragistics.Windows.Controls.Events;
using Infragistics.Windows.Selection;
using System.Collections.Specialized;
using System.Collections;
using System.Windows.Media;
using Infragistics.Windows.Themes;
using System.Windows.Markup;
using System.Windows.Controls;
using Infragistics.Windows.Editors.Events;
using Infragistics.Windows.Licensing;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;
using Infragistics.Shared;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;

namespace Infragistics.Windows.Editors
{
	
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


	/// <summary>
	/// A custom control used to display one or more months.
	/// </summary>
    /// <remarks>
    /// <p class="body">The XamMonthCalendar provides functionality similar to that of the Microsoft Vista 
    /// Common Controls MonthCalendar class.
    /// </p>
    /// <p class="body">The control provides navigation style functionality whereby you can zoom out to more 
    /// quickly navigate the available dates and then zoom back into to change the selection. The <see cref="CurrentCalendarMode"/> 
    /// is used to control the current mode. The <see cref="MinCalendarMode"/> may be used to control the lowest level 
    /// of dates that the end user may navigate into.
    /// </p>
    /// <p class="body">The default template for XamMonthCalendar uses a <see cref="CalendarItemGroupPanel"/> 
    /// that will generate <see cref="CalendarItemGroup"/> instances based on the row/column count specified 
    /// via the <see cref="CalendarDimensions"/>. In addition, when the <see cref="AutoAdjustCalendarDimensions"/> 
    /// property is set to true, which is the default value, the panel will automatically generate additional groups 
    /// to fill the available space up to its <see cref="CalendarItemGroupPanel.MaxGroups"/>. The <see cref="ReferenceDate"/> 
    /// is used by the groups to determine which dates should be shown.
    /// </p>
    /// <p class="body">The control supports multiple selection modes which can be controlled via its <see cref="SelectionType"/>. 
    /// When using a multiple selection mode such as <b>Extended</b> or <b>Range</b>, the <see cref="SelectedDates"/> property 
    /// may be used to access/change the selection up to the <see cref="MaxSelectedDates"/>. The control also exposes a <see cref="SelectedDate"/> property which is 
    /// primarily used when in a single select mode. When in a multiselect mode, this property will return the first selected date.
    /// </p>
    /// <p class="body">The control exposes a number of properties that may be used to restrict the selectable dates. The 
    /// <see cref="MinDate"/> and <see cref="MaxDate"/> are used to control the range within which the user may navigate. You can 
    /// then disable dates within that range using either the <see cref="DisabledDaysOfWeek"/> and <see cref="DisabledDates"/>.
    /// </p>
    /// </remarks>
    
    
    [StyleTypedProperty(Property = "CalendarItemStyle", StyleTargetType = typeof(CalendarItem))]
    [StyleTypedProperty(Property = "CalendarDayStyle", StyleTargetType = typeof(CalendarDay))]
    public class XamMonthCalendar : IGControlBase,
		ICommandHost,
		ISelectionHost
	{
		#region Member Variables

		private UltraLicense                                    _license;
		private CalendarManager									_calendarManager;

        private SelectedDateCollection                          _selectedDates;
		private ISelectionStrategyFilter						_selectionStrategyFilter;
        
		private SelectionStrategyBase							_selectionStrategy;
		private ObservableCollectionExtended<DateTime>			_selectedDatesSnapshot;
		private DateTime?										_pivotDate;

        private ObservableCollectionExtended<DayOfWeek>         _daysOfWeekInternal;
        private ReadOnlyObservableCollection<DayOfWeek>         _daysOfWeek;
        internal const int                                       AllDays = 0x7F;
        private List<CalendarItemGroup>                         _groups;
        private CalendarItemGroup                               _groupForSizing;
        private CalendarDateRangeCollection                     _disabledDates;

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

        private CalendarItemGroup                               _zoomFocusGroup;

        // AS 10/3/08 TFS8607
        private bool                                            _suppressBringActiveDateInView;

        // AS 2/5/09 TFS10681
        private bool?                                           _shouldProcessActiveSelectionChanged = null;
        private WeakReference                                   _activeStrategy;

        // AS 2/9/09 TFS11631
        private bool                                            _preventFocusActiveItem;

		// AS 3/23/10 TFS26461
		private CurrentDate										_currentDateWatcher;

        #endregion //Member Variables

		#region Constructor

		static XamMonthCalendar()
		{
            // AS 5/9/08
            // register the groupings that should be applied when the theme property is changed
            ThemeManager.RegisterGroupings(typeof(XamMonthCalendar), new string[] { PrimitivesGeneric.Location.Grouping, EditorsGeneric.Location.Grouping });

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(typeof(XamMonthCalendar)));
			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnLanguageChanged)));
            Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentCenterBox));
            Control.VerticalContentAlignmentProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.VerticalAlignmentCenterBox));

            // we don't want the tab index of the children having any comparison to those
            // outside the control
            KeyboardNavigation.TabNavigationProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KeyboardNavigationMode.Local));

            // when focus is within the monthcalendar, it should not be a tabstop
            KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceIsTabStop)));
		}

		/// <summary>
		/// Initializes a new <see cref="XamMonthCalendar"/>
		/// </summary>
		public XamMonthCalendar()
		{
            // verify and cache the license
            //
            // Wrapped in a try/catch for a FileNotFoundException.
            // When the assembly is loaded dynamically, VS seems 
            // to be trying to reload a copy of Shared even though 
            // one is in memory. This generates a FileNotFoundException
            // when the dll is not in the gac and not in the AppBase
            // for the AppDomain.
            //
            try
            {
                // We need to pass our type into the method since we do not want to pass in 
                // the derived type.
                this._license = LicenseManager.Validate(typeof(XamMonthCalendar), this) as UltraLicense;
            }
            catch (System.IO.FileNotFoundException) { }

			this._calendarManager = new CalendarManager();

            this._groups = new List<CalendarItemGroup>();

			// initialize the calendar manager
			this._calendarManager.FirstDayOfWeek = this.FirstDayOfWeek;
			this._calendarManager.WeekRule = this.WeekRule;

            this._daysOfWeekInternal = new ObservableCollectionExtended<DayOfWeek>();
            this._daysOfWeekInternal.CollectionChanged += new NotifyCollectionChangedEventHandler(OnDaysOfWeekChanged);
            this._daysOfWeekInternal.ReInitialize(this._calendarManager.GetDaysOfWeek());
            this._daysOfWeek = new ReadOnlyObservableCollection<DayOfWeek>(this._daysOfWeekInternal);

            this._disabledDates = new CalendarDateRangeCollection(this);
            this._disabledDates.CollectionChanged += new NotifyCollectionChangedEventHandler(OnDisabledDatesChanged);

			this._selectedDates = new SelectedDateCollection(this);
			this._selectedDatesSnapshot = new ObservableCollectionExtended<DateTime>();

            this.SetValue(MonthCalendarPropertyKey, this);

			// AS 3/23/10 TFS26461
			_currentDateWatcher = new CurrentDate();
			_currentDateWatcher.ValueChanged += new EventHandler(OnCurrentDateChanged);

			// AS 8/16/10 TFS36762
			// The control may have been created after the date changed in 
			// which case we need to update the today property value anyway.
			//
			this.SetValue(TodayPropertyKey, _currentDateWatcher.GetValue(CurrentDate.ValueProperty));
		}
        #endregion //Constructor

        #region ResourceKeys

        #region ScrollNextRepeatButtonStyleKey

        /// <summary>
        /// The key used to identify the <see cref="FrameworkElement.Style"/> for the <see cref="System.Windows.Controls.Primitives.RepeatButton"/> used to scroll the groups in view forward within the <see cref="XamMonthCalendar"/>.
        /// </summary>
        /// <seealso cref="ScrollPreviousRepeatButtonStyleKey"/>
        public static readonly ResourceKey ScrollNextRepeatButtonStyleKey = new StaticPropertyResourceKey(typeof(XamMonthCalendar), "ScrollNextRepeatButtonStyleKey");

        #endregion //ScrollNextRepeatButtonStyleKey

        #region ScrollPreviousRepeatButtonStyleKey

        /// <summary>
        /// The key used to identify the <see cref="FrameworkElement.Style"/> for the <see cref="System.Windows.Controls.Primitives.RepeatButton"/> used to scroll the groups in view backward within the <see cref="XamMonthCalendar"/>.
        /// </summary>
        /// <seealso cref="ScrollNextRepeatButtonStyleKey"/>
        public static readonly ResourceKey ScrollPreviousRepeatButtonStyleKey = new StaticPropertyResourceKey(typeof(XamMonthCalendar), "ScrollPreviousRepeatButtonStyleKey");

        #endregion //ScrollPreviousRepeatButtonStyleKey

        #region TodayButtonStyleKey

        /// <summary>
        /// The key used to identify the <see cref="FrameworkElement.Style"/> used for the <see cref="System.Windows.Controls.Button"/> that represents the <b>Today</b> at the bottom of the <see cref="XamMonthCalendar"/>.
        /// </summary>
        /// <seealso cref="TodayButtonVisibility"/>
        public static readonly ResourceKey TodayButtonStyleKey = new StaticPropertyResourceKey(typeof(XamMonthCalendar), "TodayButtonStyleKey");

        #endregion //TodayButtonStyleKey

        #endregion //ResourceKeys

		#region Properties

		#region Public

		#region ActiveDate

		/// <summary>
		/// Identifies the <see cref="ActiveDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActiveDateProperty = DependencyProperty.Register("ActiveDate",
			typeof(DateTime?), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnActiveDateChanged), new CoerceValueCallback(CoerceActiveDate)));

		private static object CoerceActiveDate(DependencyObject d, object value)
		{
			XamMonthCalendar cal = (XamMonthCalendar)d;

            // AS 9/4/08
            // If we have a selected date but no active date then we should use
            // the selected date as the active date. If we have neither then use
            // today's date. In this way a control like the XamDateTimeEditor that
            // is only binding the SelectedDate. Can have the MC have a day with 
            // focus even if there is no selection.
            //
            if (null == value)
                value = cal.SelectedDate ?? DateTime.Today;

			if (null != value)
			{
				DateTime dateValue = (DateTime)value;

                CalendarMode mode = cal.CurrentCalendarMode;

                // we always want to use the earliest start date
                dateValue = cal._calendarManager.GetItemStartDate(dateValue, mode);

                // make sure its in the min/max range
                dateValue = cal.CoerceMinMaxDate(dateValue);

                // first get the range of dates for the item that contains
                // the active date. we may be able to just get another date
                // for that same item so the same item remains selected
                DateTime itemEndDate = cal._calendarManager.GetItemEndDate(dateValue, mode);

                itemEndDate = cal.CoerceMinMaxDate(itemEndDate);

                // try to get an activatable date within that item
                value = cal.GetActivatableDate(dateValue, itemEndDate, true, mode);

                if (value == null)
                {
                    // find the closest date within the group
                    value = cal.GetClosestActivatableDate(dateValue, itemEndDate);
                }
			}

			return value;
		}

		private static void OnActiveDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			// make sure the active date is in view
			XamMonthCalendar cal = (XamMonthCalendar)d;

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

						if (cal.IsKeyboardFocusWithin)
							item.Focus();
                        
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
        /// within the <see cref="XamMonthCalendar"/>. Keyboard navigation within the control is then based on that item's dates.</p>
        /// </remarks>
		/// <seealso cref="ActiveDateProperty"/>
		/// <seealso cref="MonthCalendarCommands"/>
		//[Description("Returns or sets the date of the active day.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
        [TypeConverter(typeof(NullableConverter<DateTime>))]
		public DateTime? ActiveDate
		{
			get
			{
				return (DateTime?)this.GetValue(XamMonthCalendar.ActiveDateProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.ActiveDateProperty, value);
			}
		}

		#endregion //ActiveDate

		// AS 10/14/09 FR11859
		#region AllowLeadingAndTrailingGroupActivation

		/// <summary>
		/// Identifies the <see cref="AllowLeadingAndTrailingGroupActivation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowLeadingAndTrailingGroupActivationProperty = DependencyProperty.Register("AllowLeadingAndTrailingGroupActivation",
			typeof(bool), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

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
				return (bool)this.GetValue(XamMonthCalendar.AllowLeadingAndTrailingGroupActivationProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.AllowLeadingAndTrailingGroupActivationProperty, value);
			}
		}

		#endregion //AllowLeadingAndTrailingGroupActivation

        #region AutoAdjustCalendarDimensions

        /// <summary>
        /// Identifies the <see cref="AutoAdjustCalendarDimensions"/> dependency property
        /// </summary>
        public static readonly DependencyProperty AutoAdjustCalendarDimensionsProperty = DependencyProperty.Register("AutoAdjustCalendarDimensions",
            typeof(bool), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

        /// <summary>
        /// Returns or sets whether the control should automatically calculate and change the calendar dimensions based on the size used to arrange the XamMonthCalendar.
        /// </summary>
        /// <remarks>
        /// <p class="body">The AutoAdjustCalendarDimensions property is used by the <see cref="CalendarItemGroupPanel"/> within the 
        /// template of the <see cref="XamMonthCalendar"/> that indicates whether it should automatically generate additional <see cref="CalendarItemGroup"/> 
        /// instances when it has more space available than can be used by the groups specified by the <see cref="CalendarDimensions"/>.</p>
        /// <p class="note">If you retemplate the <see cref="XamMonthCalendar"/> such that it does not contain a <see cref="CalendarItemGroupPanel"/>, 
        /// this property will not affect the display of the control.</p>
        /// </remarks>
        /// <seealso cref="AutoAdjustCalendarDimensionsProperty"/>
        /// <seealso cref="CalendarDimensions"/>
        /// <seealso cref="CalendarItemGroupPanel"/>
        /// <seealso cref="CalendarItemGroupPanel.MaxGroups"/>
        //[Description("Returns or sets whether the control should automatically calculate and change the calendar dimensions based on the size used to arrange the XamMonthCalendar.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public bool AutoAdjustCalendarDimensions
        {
            get
            {
                return (bool)this.GetValue(XamMonthCalendar.AutoAdjustCalendarDimensionsProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.AutoAdjustCalendarDimensionsProperty, value);
            }
        }

        #endregion //AutoAdjustCalendarDimensions

        #region CalendarDimensions

        /// <summary>
        /// Identifies the <see cref="CalendarDimensions"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CalendarDimensionsProperty = DependencyProperty.Register("CalendarDimensions",
            typeof(CalendarDimensions), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(new CalendarDimensions(1,1)));

        /// <summary>
        /// Returns or sets a value indicating the preferred rows and columns of groups to be displayed within the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">The CalendarDimensions is used by the <see cref="CalendarItemGroupPanel"/> within the template of the 
        /// <see cref="XamMonthCalendar"/> to determine the minimum number of rows and columns of <see cref="CalendarItemGroup"/> 
        /// instances that it should create and arrange. If the <see cref="AutoAdjustCalendarDimensions"/> is true, which is the 
        /// default value, and the CalendarItemGroupPanel has space to display mode groups it will automatically create additional 
        /// groups up to its <see cref="CalendarItemGroupPanel.MaxGroups"/>.</p>
        /// <p class="note">If you retemplate the <see cref="XamMonthCalendar"/> such that it does not contain a <see cref="CalendarItemGroupPanel"/>, 
        /// this property will not affect the display of the control.</p>
        /// </remarks>
        /// <seealso cref="CalendarDimensionsProperty"/>
        /// <seealso cref="AutoAdjustCalendarDimensions"/>
        /// <seealso cref="CalendarItemGroupPanel"/>
        //[Description("Returns or sets a value indicating the preferred rows and columns of groups to be displayed within the control.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public CalendarDimensions CalendarDimensions
        {
            get
            {
                return (CalendarDimensions)this.GetValue(XamMonthCalendar.CalendarDimensionsProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.CalendarDimensionsProperty, value);
            }
        }

        #endregion //CalendarDimensions

        #region CalendarDayStyle

        /// <summary>
        /// Identifies the <see cref="CalendarDayStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CalendarDayStyleProperty = DependencyProperty.Register("CalendarDayStyle",
            typeof(Style), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemStyleChanged)));

        private static void OnItemStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((XamMonthCalendar)d).NotifyGroupChange(MonthCalendarChange.ItemStyleChange);
        }

        /// <summary>
        /// Returns or sets the default style to use for <see cref="CalendarDay"/> instances within the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">When this property is set, the <see cref="CalendarItemArea"/> will set the <see cref="FrameworkElement.Style"/> property 
        /// of the <see cref="CalendarDay"/> instances that it creates.</p>
        /// <p class="note"><b>Note:</b> If a <see cref="CalendarDayStyleSelector"/> is provided and that class provides a Style, the value for this 
        /// property will not be used for that item.</p>
        /// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using this value, any local styles (including those 
        /// provided via the <see cref="Theme"/> property, that target the <see cref="CalendarDay"/> will not be used.</p>
        /// </remarks>
        /// <seealso cref="CalendarDayStyleProperty"/>
        /// <seealso cref="CalendarDayStyleSelector"/>
        /// <seealso cref="CalendarItemStyle"/>
        //[Description("Returns or sets the default style to use for CalendarDay instances within the control.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Style CalendarDayStyle
        {
            get
            {
                return (Style)this.GetValue(XamMonthCalendar.CalendarDayStyleProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.CalendarDayStyleProperty, value);
            }
        }

        #endregion //CalendarDayStyle

        #region CalendarDayStyleSelector

        /// <summary>
        /// Identifies the <see cref="CalendarDayStyleSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CalendarDayStyleSelectorProperty = DependencyProperty.Register("CalendarDayStyleSelector",
            typeof(StyleSelector), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemStyleChanged)));

        /// <summary>
        /// Returns or sets an instance of a StyleSelector class that provides a style for a specific <see cref="CalendarDay"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">This class is used by the <see cref="CalendarItemArea"/> to obtain a <see cref="FrameworkElement.Style"/> for a 
        /// specific <see cref="CalendarDay"/> instance. If a Style is returned for a specified day, the value of the <see cref="CalendarDayStyle"/> will not be used for that item.</p>
        /// <p class="note"><b>Note:</b> The CalendarItemArea may reuse items so you should not set the value of any other property on the day from the SelectStyle method.</p>
        /// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using the Style returned by this StyleSelector instance, any local styles (including those 
        /// provided via the <see cref="Theme"/> property, that target the <see cref="CalendarDay"/> will not be used.</p>
        /// </remarks>
        /// <seealso cref="CalendarDayStyleSelectorProperty"/>
        /// <seealso cref="CalendarDayStyle"/>
        /// <seealso cref="CalendarItemStyleSelector"/>
        //[Description("Returns or sets an instance of a StyleSelector class that provides a style for a specific CalendarDay")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public StyleSelector CalendarDayStyleSelector
        {
            get
            {
                return (StyleSelector)this.GetValue(XamMonthCalendar.CalendarDayStyleSelectorProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.CalendarDayStyleSelectorProperty, value);
            }
        }

        #endregion //CalendarDayStyleSelector

        #region CalendarItemStyle

        /// <summary>
        /// Identifies the <see cref="CalendarItemStyle"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CalendarItemStyleProperty = DependencyProperty.Register("CalendarItemStyle",
            typeof(Style), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemStyleChanged)));

        /// <summary>
        /// Returns or sets the default style to use for <see cref="CalendarItem"/> instances within the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">When this property is set, the <see cref="CalendarItemArea"/> will set the <see cref="FrameworkElement.Style"/> property 
        /// of the <see cref="CalendarItem"/> instances that it creates.</p>
        /// <p class="note"><b>Note:</b> If a <see cref="CalendarItemStyleSelector"/> is provided and that class provides a Style, the value for this 
        /// property will not be used for that item.</p>
        /// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using this value, any local styles (including those 
        /// provided via the <see cref="Theme"/> property, that target the <see cref="CalendarItem"/> will not be used.</p>
        /// </remarks>
        /// <seealso cref="CalendarItemStyleProperty"/>
        /// <seealso cref="CalendarItemStyleSelector"/>
        /// <seealso cref="CalendarDayStyle"/>
        //[Description("Returns or sets the default style to use for CalendarItem instances within the control.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Style CalendarItemStyle
        {
            get
            {
                return (Style)this.GetValue(XamMonthCalendar.CalendarItemStyleProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.CalendarItemStyleProperty, value);
            }
        }

        #endregion //CalendarItemStyle

   		#region CalendarItemStyleSelector

        /// <summary>
        /// Identifies the <see cref="CalendarItemStyleSelector"/> dependency property
        /// </summary>
        public static readonly DependencyProperty CalendarItemStyleSelectorProperty = DependencyProperty.Register("CalendarItemStyleSelector",
            typeof(StyleSelector), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnItemStyleChanged)));

        /// <summary>
        /// Returns or sets an instance of a StyleSelector class that provides a style for a specific <see cref="CalendarItem"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">This class is used by the <see cref="CalendarItemArea"/> to obtain a <see cref="FrameworkElement.Style"/> for a 
        /// specific <see cref="CalendarItem"/> instance. If a Style is returned for a specified item, the value of the <see cref="CalendarItemStyle"/> will not be used for that item.</p>
        /// <p class="note"><b>Note:</b> The CalendarItemArea may reuse items so you should not set the value of any other property on the item from the SelectStyle method.</p>
        /// <p class="note"><b>Note:</b> Since the Style property will be explicitly set using the Style returned by this StyleSelector instance, any local styles (including those 
        /// provided via the <see cref="Theme"/> property, that target the <see cref="CalendarItem"/> will not be used.</p>
        /// </remarks>
        /// <seealso cref="CalendarItemStyleSelectorProperty"/>
        /// <seealso cref="CalendarItemStyle"/>
        /// <seealso cref="CalendarDayStyleSelector"/>
        //[Description("Returns or sets an instance of a StyleSelector class that provides a style for a specific CalendarItem")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public StyleSelector CalendarItemStyleSelector
        {
            get
            {
                return (StyleSelector)this.GetValue(XamMonthCalendar.CalendarItemStyleSelectorProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.CalendarItemStyleSelectorProperty, value);
            }
        }

        #endregion //CalendarItemStyleSelector

		#region CurrentCalendarMode

		/// <summary>
		/// Identifies the <see cref="CurrentCalendarMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentCalendarModeProperty = DependencyProperty.Register("CurrentCalendarMode",
			typeof(CalendarMode), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(CalendarMode.Days, new PropertyChangedCallback(OnCurrentCalendarModeChanged), new CoerceValueCallback(CoerceCurrentCalendarMode)));

        private static object CoerceCurrentCalendarMode(DependencyObject d, object value)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;
            CalendarMode mode = (CalendarMode)value;

            if (mode < cal.MinCalendarMode)
                return cal.GetValue(MinCalendarModeProperty);
            else if (mode > cal.MaxCalendarMode)
                return cal.MaxCalendarMode;

            return value;
        }

		private static void OnCurrentCalendarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
            XamMonthCalendar cal = (XamMonthCalendar)d;

            cal.ClearPreferredNavInfo();

            // AS 10/3/08 TFS8607
            // The active date changed will try to get an item from the group 
            // to focus. This is a problem because the group's mode hasn't changed
            // so we will get an item from the "old" mode. So we need to push the
            // new mode into the groups.
            //
            cal.NotifyGroupChange(MonthCalendarChange.CurrentModeChanged);

            // AS 10/3/08
            // We need to fix up the active date but we don't want to bring the date
            // into view. We are zooming out/in a specific group.
            //
            bool oldSuppress = cal._suppressBringActiveDateInView;
            cal._suppressBringActiveDateInView = true;

            try
            {
                cal.CoerceValue(ActiveDateProperty);
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
        /// <p class="body">The <see cref="XamMonthCalendar"/> provides the ability to zoom out to see larger ranges of dates and then zoom back 
        /// in to change the selection similar to the functionality found in the Microsoft Vista Common Controls MonthCalendar. The CurrentCalendarMode 
        /// controls the current mode that the contained <see cref="CalendarItemGroup"/> instances use to initialize its items. For example, when set to 
        /// <b>Days</b>, which is the default value, the CalendarItemGroup will contain <see cref="CalendarDay"/> instances where each represents 
        /// a discrete date. When set to <b>Months</b>, the CalendarItemGroups will contain <see cref="CalendarItem"/> instances where each represents 
        /// a single month within a specific year.</p>
        /// <p class="body">The <see cref="MonthCalendarCommands"/> class defines two commands (<see cref="MonthCalendarCommands.ZoomOutCalendarMode"/> 
        /// and <see cref="MonthCalendarCommands.ZoomInCalendarMode"/>) that may be used to change the CurrentCalendarMode.</p>
        /// <p class="note"><b>Note:</b> The value for this property cannot be set to a value that would be less than the <see cref="MinCalendarMode"/>.</p>
        /// </remarks>
        /// <seealso cref="CurrentCalendarModeProperty"/>
        /// <seealso cref="MinCalendarMode"/>
		//[Description("Returns or sets which types of calendar items will be displayed to the end user.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
        [DependsOn("MinCalendarMode")]
        public CalendarMode CurrentCalendarMode
		{
			get
			{
				return (CalendarMode)this.GetValue(XamMonthCalendar.CurrentCalendarModeProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.CurrentCalendarModeProperty, value);
			}
		}

		#endregion //CurrentCalendarMode

		#region DayOfWeekHeaderFormat

        /// <summary>
        /// Identifies the <see cref="DayOfWeekHeaderFormat"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DayOfWeekHeaderFormatProperty = DependencyProperty.Register(
			"DayOfWeekHeaderFormat", typeof(DayOfWeekHeaderFormat), typeof(XamMonthCalendar), new PropertyMetadata(DayOfWeekHeaderFormat.TwoCharacters));

        /// <summary>
        /// Returns or sets the format for the header of the days of the week.
        /// </summary>
        /// <seealso cref="DayOfWeekHeaderFormatProperty"/>
        /// <seealso cref="DayOfWeekToStringConverter"/>
        //[Description("Returns or sets the format for the header of the days of the week.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public DayOfWeekHeaderFormat DayOfWeekHeaderFormat
		{
			get { return (DayOfWeekHeaderFormat)this.GetValue(DayOfWeekHeaderFormatProperty); }
			set { this.SetValue(DayOfWeekHeaderFormatProperty, value); }
		} 
		#endregion //DayOfWeekHeaderFormat

		#region DayOfWeekHeaderVisibility

        /// <summary>
        /// Identifies the <see cref="DayOfWeekHeaderVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DayOfWeekHeaderVisibilityProperty = DependencyProperty.Register(
            "DayOfWeekHeaderVisibility", typeof(Visibility), typeof(XamMonthCalendar), new PropertyMetadata(KnownBoxes.VisibilityVisibleBox));

		/// <summary>
		/// Returns or sets a value indicating whether the day of week headers should be displayed.
		/// </summary>
		/// <seealso cref="DayOfWeekHeaderFormat"/>
		/// <seealso cref="DaysOfWeek"/>
		/// <seealso cref="DayOfWeekHeaderVisibilityProperty"/>
        //[Description("Returns or sets a value indicating whether the day of week headers should be displayed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility DayOfWeekHeaderVisibility
		{
            get { return (Visibility)this.GetValue(DayOfWeekHeaderVisibilityProperty); }
			set { this.SetValue(DayOfWeekHeaderVisibilityProperty, value); }
		}

		#endregion //DayOfWeekHeaderVisibility

		#region DaysOfWeek

		/// <summary>
		/// Returns a collection of <see cref="DayOfWeek"/> objects in order based on the <see cref="FirstDayOfWeek"/>
		/// </summary>
        /// <remarks>
        /// <p class="body">The DaysOfWeek collection is exposed primarily to be used from within the templates of the 
        /// <see cref="CalendarItemArea"/> instances within the control. The collection is ordered based on the resolved 
        /// first day of the week.</p>
        /// </remarks>
		/// <seealso cref="FirstDayOfWeek"/>
		/// <seealso cref="DayOfWeekHeaderFormat"/>
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        [ReadOnly(true)]
        public ReadOnlyObservableCollection<DayOfWeek> DaysOfWeek
		{
			get
			{
				return this._daysOfWeek;
			}
		}
		#endregion //DaysOfWeek

        #region DisabledDates
        /// <summary>
        /// Returns a modifiable collection of <see cref="CalendarDateRange"/> instances that indicates the items that should be considered disabled.
        /// </summary>
        /// <remarks>
        /// <p class="body">The DisabledDates is a collection of <see cref="CalendarDateRange"/> instances that represent ranges of dates that should 
        /// not be selectable by the end user. Dates may also be disabled using the <see cref="DisabledDaysOfWeek"/> property.</p>
        /// </remarks>
        /// <seealso cref="DisabledDaysOfWeek"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
        public CalendarDateRangeCollection DisabledDates
        {
            get { return this._disabledDates; }
        } 
        #endregion //DisabledDates

        #region DisabledDaysOfWeek

        /// <summary>
        /// Identifies the <see cref="DisabledDaysOfWeek"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DisabledDaysOfWeekProperty = DependencyProperty.Register("DisabledDaysOfWeek",
            typeof(DayOfWeekFlags), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(DayOfWeekFlags.None, new PropertyChangedCallback(OnDisabledDaysOfWeekChanged))
                );

        
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


        private static void OnDisabledDaysOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
            {
                // if we're hiding days of the week then we need to rebuild the days of the week
                if (false == cal.ShowDisabledDaysOfWeek)
                {
                    cal._calendarManager.HiddenDays = cal.DisabledDaysOfWeek;
                    cal.InitializeDaysOfWeek();
                }

                cal.NotifyGroupChange(MonthCalendarChange.DisabledDatesChanged);
            }
        }

        /// <summary>
        /// Returns or sets a flagged enumeration indicating which days of the week are always disabled.
        /// </summary>
        /// <remarks>
        /// <p class="body">The DisabledDaysOfWeek is a flagged enumeration that can be used to prevent selection of 
        /// one or more days of the week. The <see cref="DisabledDates"/> may be used to disable specific dates (or 
        /// ranges of dates).</p>
        /// <p class="body">When the <see cref="ShowDisabledDaysOfWeek"/> property is set to true 
        /// the disabled days of the week will be hidden from the display.</p>
        /// </remarks>
        /// <seealso cref="DisabledDaysOfWeekProperty"/>
        /// <seealso cref="DisabledDates"/>
        /// <seealso cref="ShowDisabledDaysOfWeek"/>
        //[Description("Returns or sets a flagged enumeration indicating which days of the week are always disabled.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public DayOfWeekFlags DisabledDaysOfWeek
        {
            get
            {
                return (DayOfWeekFlags)this.GetValue(XamMonthCalendar.DisabledDaysOfWeekProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.DisabledDaysOfWeekProperty, value);
            }
        }

        #endregion //DisabledDaysOfWeek

		#region FirstDayOfWeek

		/// <summary>
		/// Identifies the <see cref="FirstDayOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstDayOfWeekProperty = DependencyProperty.Register(
			"FirstDayOfWeek", typeof(DayOfWeek?), typeof(XamMonthCalendar), new FrameworkPropertyMetadata((DayOfWeek?)null, new PropertyChangedCallback(OnFirstDayOfWeekChanged)));

		private static void OnFirstDayOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthCalendar cal = d as XamMonthCalendar;

			if (cal != null)
			{
				cal._calendarManager.FirstDayOfWeek = (DayOfWeek?)e.NewValue;

				if (cal.IsInitialized)
				{
                    using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
                    {
                        cal.NotifyGroupChange(MonthCalendarChange.FirstDayOfWeekChanged);
                        cal.InitializeDaysOfWeek();
                    }
				}
			}
		}

		/// <summary>
		/// Gets or sets the <see cref="DayOfWeek"/> that represents the first day of the week or null to use the value from the current <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
        /// <remarks>
        /// <p class="body">By default the FirstDayOfWeek is set to null. When set to null, the <see cref="DateTimeFormatInfo.FirstDayOfWeek"/> 
        /// from the control's <see cref="FrameworkElement.Language"/> will be used.</p>
        /// </remarks>
        /// <see cref="DaysOfWeek"/>
        //[Description("Gets or sets the DayOfWeek that represents the first day of the week or null to use the value from the current CultureInfo.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [TypeConverter(typeof(NullableConverter<DayOfWeek>))]
        public DayOfWeek? FirstDayOfWeek
		{
			get { return (DayOfWeek?)this.GetValue(FirstDayOfWeekProperty); }
			set { this.SetValue(FirstDayOfWeekProperty, value); }
		}

		#endregion //FirstDayOfWeek

        // AS 9/24/08 TFS7577
        #region IsSelectionActive (readonly, attached)

        internal static readonly DependencyPropertyKey IsSelectionActivePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsSelectionActive",
            typeof(bool), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the IsSelectionActive" attached readonly dependency property
        /// </summary>
        /// <seealso cref="GetIsSelectionActive"/>
        public static readonly DependencyProperty IsSelectionActiveProperty =
            IsSelectionActivePropertyKey.DependencyProperty;


        /// <summary>
        /// Gets the value of the 'IsSelectionActive' attached readonly property
        /// </summary>
        /// <remarks>
        /// <p class="body">The IsSelectionActive property is a readonly inherited property similar to that of 
        /// the <see cref="System.Windows.Controls.Primitives.Selector.GetIsSelectionActive(DependencyObject)"/> that is used to indicate whether keyboard focus is 
        /// within the control and therefore can be used to control how the selected items are rendered.</p>
        /// </remarks>
        /// <seealso cref="IsSelectionActiveProperty"/>
        public static bool GetIsSelectionActive(DependencyObject d)
        {
            return (bool)d.GetValue(XamMonthCalendar.IsSelectionActiveProperty);
        }

        #endregion //IsSelectionActive (readonly, attached)

        #region MaxDate

		/// <summary>
		/// Identifies the <see cref="MaxDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDateProperty = DependencyProperty.Register("MaxDate",
			typeof(DateTime), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(DateTime.MaxValue, new PropertyChangedCallback(OnMaxDateChanged), new CoerceValueCallback(CoerceMaxDate)));

		private static object CoerceMaxDate(DependencyObject d, object newValue)
		{
			XamMonthCalendar cal = d as XamMonthCalendar;
            DateTime newDate = (DateTime)newValue;

			if (newDate < cal.MinDate)
                newDate = cal.MinDate;

            // ensure its valid for the current calendar
            newDate = cal._calendarManager.CoerceMinMaxDate(newDate);

			return newDate;
		}

		private static void OnMaxDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthCalendar cal = d as XamMonthCalendar;

			if (null != cal)
			{
                using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
                {
                    cal.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
                    cal.CoerceValue(XamMonthCalendar.MeasureDateProperty);
                    cal.NotifyGroupChange(MonthCalendarChange.MaxDateChanged);

                    // update selection
                    if (cal.IsInitialized)
                        cal.SelectedDates.EnsureWithinMinMax();
                }
            }
		}

		/// <summary>
		/// Returns or sets the preferred maximum date that can be selected or activated for the control.
		/// </summary>
        /// <remarks>
        /// <p class="body">The MaxDate and <see cref="MinDate"/> are used to control the range of dates that 
        /// are available to the end user. Days outside that range will not be displayed.</p>
        /// <p class="body">The <see cref="DisabledDates"/> and <see cref="DisabledDaysOfWeek"/> may be 
        /// used to prevent selection/activation of dates within the MinDate/MaxDate range.</p>
        /// <p class="note"><b>Note:</b> The actual range available could be smaller than that specified by the 
        /// MinDate and MaxDate if it is outside the values allowed by the <see cref="System.Globalization.Calendar.MaxSupportedDateTime"/> 
        /// and <see cref="System.Globalization.Calendar.MinSupportedDateTime"/> of the culture used by the control. The culture used is 
        /// based upon the <see cref="FrameworkElement.Language"/> property.</p>
        /// </remarks>
		/// <seealso cref="MaxDateProperty"/>
		/// <seealso cref="MinDate"/>
        /// <seealso cref="DisabledDates"/>
        /// <seealso cref="DisabledDaysOfWeek"/>
        //[Description("Returns or sets the preferred maximum date that can be selected or activated for the control.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[DependsOn("MinDate")]
        [Bindable(true)]
		public DateTime MaxDate
		{
			get
			{
				return (DateTime)this.GetValue(XamMonthCalendar.MaxDateProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.MaxDateProperty, value);
			}
		}

		#endregion //MaxDate

		#region MaxSelectedDates

		/// <summary>
		/// Identifies the <see cref="MaxSelectedDates"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxSelectedDatesProperty = DependencyProperty.Register("MaxSelectedDates",
			typeof(int), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(Utils.ZeroInt, new PropertyChangedCallback(OnMaxSelectedDatesChanged)), new ValidateValueCallback(ValidateMaxSelected));

        private static void OnMaxSelectedDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;
            int max = (int)e.NewValue;

            // AS 10/23/08
            //if (cal.IsInitialized && max > 0 && max > cal.SelectedDates.Count)
            if (cal.IsInitialized && max > 0 && max < cal.SelectedDates.Count)
            {
                // reduce selection and raise event
                DateTime[] dates = new DateTime[cal.SelectedDates.Count];
                cal.SelectedDates.CopyTo(dates, 0);
                Array.Resize(ref dates, max);
                cal.SelectedDates.Reinitialize(dates);
            }
        }

		private static bool ValidateMaxSelected(object value)
		{
			if (value is int)
				return ((int)value) >= 0;

			return false;
		}

		/// <summary>
		/// Returns or sets the maximum number of days that can be selected at any time.
		/// </summary>
        /// <p class="note">The MaxSelectedDates is only used when the <see cref="SelectionType"/> is set 
        /// to a value that allows multiple selection such as <b>Range</b> or <b>Extended</b>.</p>
		/// <seealso cref="MaxSelectedDatesProperty"/>
		/// <seealso cref="SelectedDates"/>
        /// <seealso cref="SelectionType"/>
        //[Description("Returns or sets the maximum number of days that can be selected at any time.")]
		//[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
		public int MaxSelectedDates
		{
			get
			{
				return (int)this.GetValue(XamMonthCalendar.MaxSelectedDatesProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.MaxSelectedDatesProperty, value);
			}
		}

        internal int MaxSelectedDatesResolved
        {
            get
            {
                SelectionType selectionType = this.SelectionTypeResolved;

                if (SelectionStrategyBase.IsMultiSelectStrategy(selectionType))
                {
                    int max = this.MaxSelectedDates;

                    if (0 == max)
                        max = int.MaxValue;

                    return max;
                }

                if (SelectionStrategyBase.IsSingleSelectStrategy(selectionType))
                    return 1;

                return 0;
            }
        }

		#endregion //MaxSelectedDates

		#region MinDate

		/// <summary>
		/// Identifies the <see cref="MinDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDateProperty = DependencyProperty.Register("MinDate",
            typeof(DateTime), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(DateTime.MinValue, new PropertyChangedCallback(OnMinDateChanged), new CoerceValueCallback(CoerceMinDate)));

		private static void OnMinDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthCalendar cal = d as XamMonthCalendar;

			if (null != cal)
			{
                using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
                {
                    cal.CoerceValue(XamMonthCalendar.MaxDateProperty);
                    cal.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
                    cal.CoerceValue(XamMonthCalendar.MeasureDateProperty);
                    cal.NotifyGroupChange(MonthCalendarChange.MinDateChanged);

                    // update selection
                    if (cal.IsInitialized)
                        cal.SelectedDates.EnsureWithinMinMax();
                }

            }
		}

        private static object CoerceMinDate(DependencyObject d, object newValue)
        {
            XamMonthCalendar cal = d as XamMonthCalendar;
            DateTime newDate = (DateTime)newValue;

            // ensure its valid for the current calendar
            newDate = cal._calendarManager.CoerceMinMaxDate(newDate);

            return newDate;
        }

		/// <summary>
		/// Returns or sets the preferred minimum date that can be selected or activated for the control.
		/// </summary>
        /// <remarks>
        /// <p class="body">The MinDate and <see cref="MaxDate"/> are used to control the range of dates that 
        /// are available to the end user. Days outside that range will not be displayed.</p>
        /// <p class="body">The <see cref="DisabledDates"/> and <see cref="DisabledDaysOfWeek"/> may be 
        /// used to prevent selection/activation of dates within the MinDate/MaxDate range.</p>
        /// <p class="note"><b>Note:</b> The actual range available could be smaller than that specified by the 
        /// MinDate and MaxDate if it is outside the values allowed by the <see cref="System.Globalization.Calendar.MaxSupportedDateTime"/> 
        /// and <see cref="System.Globalization.Calendar.MinSupportedDateTime"/> of the culture used by the control. The culture used is 
        /// based upon the <see cref="FrameworkElement.Language"/> property.</p>
        /// </remarks>
        /// <seealso cref="MinDateProperty"/>
		/// <seealso cref="MaxDate"/>
        /// <seealso cref="DisabledDates"/>
        /// <seealso cref="DisabledDaysOfWeek"/>
        //[Description("Returns or sets the preferred minimum date that can be selected or activated for the control.")]
		//[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
		public DateTime MinDate
		{
			get
			{
				return (DateTime)this.GetValue(XamMonthCalendar.MinDateProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.MinDateProperty, value);
			}
		}

		#endregion //MinDate

		#region MinCalendarMode

		/// <summary>
		/// Identifies the <see cref="MinCalendarMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinCalendarModeProperty = DependencyProperty.Register("MinCalendarMode",
            typeof(CalendarMode), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(CalendarMode.Days, new PropertyChangedCallback(OnMinCalendarModeChanged)));

        private static void OnMinCalendarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            cal.CoerceValue(CurrentCalendarModeProperty);
        }

		/// <summary>
		/// Returns or sets an enumeration that indicates the minimum calendar item type that can be selected within the control.
		/// </summary>
        /// <remarks>
        /// <p class="body">The <see cref="XamMonthCalendar"/> provides the ability to zoom out to see larger ranges of dates and then zoom back 
        /// in to change the selection similar to the functionality found in the Microsoft Vista Common Controls MonthCalendar. While the 
        /// CurrentCalendarMode controls the current mode that the contained <see cref="CalendarItemGroup"/> instances use to display its items, the 
        /// MinCalendarMode determines the minimum mode into which the control may zoom. For example, when set the <b>Months</b>, the control will not 
        /// be able to zoom in any further to allow selection/viewing of individual dates/days.</p>
        /// <p class="body">When the <see cref="CurrentCalendarMode"/> is set to the same value as the MinCalendarMode, interacting within the items (e.g. 
        /// via the keyboard and mouse) will affect the current <see cref="SelectedDate"/> and <see cref="SelectedDates"/>. When the CurrentCalendarMode 
        /// is higher than the MinCalendarMode (i.e. the user has zoomed out), the selection will not be changed via the ui. Instead, the keyboard and 
        /// mouse will be used to navigate the calendar without affecting the current selection. When the user then zooms in to the MinCalendarMode, they 
        /// may then modify the selection.</p>
        /// <p class="body">The <see cref="MonthCalendarCommands"/> class defines two commands (<see cref="MonthCalendarCommands.ZoomOutCalendarMode"/> 
        /// and <see cref="MonthCalendarCommands.ZoomInCalendarMode"/>) that may be used to change the CurrentCalendarMode.</p>
        /// <p class="note"><b>Note:</b> The value for this property will restrict the available values for the <see cref="CurrentCalendarMode"/>.</p>
        /// </remarks>
		/// <seealso cref="MinCalendarModeProperty"/>
		/// <seealso cref="CurrentCalendarMode"/>
		//[Description("Returns or sets an enumeration that indicates the minimum calendar item type that can be selected within the control.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public CalendarMode MinCalendarMode
		{
			get
			{
				return (CalendarMode)this.GetValue(XamMonthCalendar.MinCalendarModeProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.MinCalendarModeProperty, value);
			}
		}

		#endregion //MinCalendarMode

        #region ReferenceDate

        /// <summary>
        /// Identifies the <see cref="ReferenceDate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ReferenceDateProperty = DependencyProperty.Register("ReferenceDate",
            typeof(DateTime?), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnReferenceDateChanged), new CoerceValueCallback(CoerceReferenceDate)));

        private static object CoerceReferenceDate(DependencyObject d, object value)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            DateTime dateValue;

            if (null != value)
                dateValue = cal.GetDefaultReferenceDate((DateTime)value);
            else
                dateValue = cal.GetDefaultReferenceDate();

            return dateValue;
        }

        private static void OnReferenceDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Debug.Assert(null != e.NewValue);

            // if it happens to get cleared reset to today
            if (e.NewValue == null)
            {
                d.SetValue(ReferenceDateProperty, DateTime.Today);
            }
        }

        /// <summary>
        /// Returns or sets a date that is used to determine the dates that should be displayed within the control.
        /// </summary>
        /// <remarks>
        /// <p class="body">The reference date is used to determine the dates that are displayed within the reference group. The 
        /// reference group is the <see cref="CalendarItemGroup"/> whose <see cref="CalendarItemGroup.ReferenceGroupOffset"/> is 
        /// zero. By default, the XamMonthCalendar used a <see cref="CalendarItemGroupPanel"/> that autogenerates the groups 
        /// and sets the first created group's ReferenceGroupOffset to 0.</p>
        /// </remarks>
        /// <seealso cref="ReferenceDateProperty"/>
        //[Description("Returns or sets a date that is used to determine the dates that should be displayed within the control.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [TypeConverter(typeof(NullableConverter<DateTime>))]
        public DateTime? ReferenceDate
        {
            get
            {
                return (DateTime?)this.GetValue(XamMonthCalendar.ReferenceDateProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.ReferenceDateProperty, value);
            }
        }

        #endregion //ReferenceDate

        #region ScrollButtonVisibility

        /// <summary>
        /// Identifies the <see cref="ScrollButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScrollButtonVisibilityProperty = DependencyProperty.Register("ScrollButtonVisibility",
            typeof(Visibility), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Returns or sets the visibility of the scroll buttons with the <see cref="CalendarItemGroup"/> instances.
        /// </summary>
        /// <remarks>
        /// <p class="body">The ScrollButtonVisibility is used to indicate whethe the <see cref="CalendarItemGroup"/> instances 
        /// within the control should display the previous and next scroll buttons. The default template for the <see cref="XamMonthCalendar"/> 
        /// uses a <see cref="CalendarItemGroupPanel"/> that autogenerates the groups displayed within the control. When scroll buttons 
        /// are to be displayed, it ensures that the scroll previous of the upper left most group and the scroll next of the 
        /// upper right most group will display their scroll buttons. If you retemplate the XamMonthCalendar to directly contain 
        /// CalendarItemGroups then you must selectively bind the <see cref="CalendarItemGroup.ScrollPreviousButtonVisibility"/> 
        /// and <see cref="CalendarItemGroup.ScrollNextButtonVisibility"/> to this property.</p>
        /// </remarks>
        /// <seealso cref="ScrollButtonVisibilityProperty"/>
        /// <seealso cref="ScrollNextRepeatButtonStyleKey"/>
        /// <seealso cref="ScrollPreviousRepeatButtonStyleKey"/>
        /// <seealso cref="CalendarItemGroup.ScrollNextButtonVisibility"/>
        /// <seealso cref="CalendarItemGroup.ScrollPreviousButtonVisibility"/>
        //[Description("Returns or sets of the scroll buttons with the CalendarItemGroup instances.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility ScrollButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(XamMonthCalendar.ScrollButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.ScrollButtonVisibilityProperty, value);
            }
        }

        #endregion //ScrollButtonVisibility

        #region SelectedDate

        /// <summary>
        /// Identifies the <see cref="SelectedDate"/> dependency property
        /// </summary>
        public static readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register("SelectedDate",
            typeof(DateTime?), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null,
                FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                new PropertyChangedCallback(OnSelectedDateChanged)));

        private static void OnSelectedDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            if (null != cal)
            {
                if (e.NewValue == null)
                    cal.SelectedDates.Clear();
                else
                {
                    if (cal.SelectionType == SelectionType.None)
                        throw new InvalidOperationException(GetString("LE_MaxSelectedDatesExceeded", 1, 0));

                    DateTime selectedDate = (DateTime)e.NewValue;

					// AS 10/15/09 TFS23545
					selectedDate = selectedDate.Date;

                    // if there is no selection or if this isn't the first item
                    // then the selection is changing in which case we'll clear 
                    // the collection and select just this item
                    if (cal.SelectedDates.Count == 0 || cal.SelectedDates[0] != selectedDate)
                    {
                        cal.SelectedDates.Reinitialize(new DateTime[] { selectedDate });
                    }
                }

				// AS 11/17/11 TFS89449
				// In a single select strategy the selection and active item are kept in 
				// sync. The activedate may have been set to an explicit value (e.g. when 
				// it received keyboard focus) so coercing is not enough. We need to set 
				// the active date to the selected date (assuming we have one).
				//
				if (null != e.NewValue && SelectionStrategyBase.IsSingleSelectStrategy(cal.SelectionTypeResolved))
				{
					cal.SetValue(ActiveDateProperty, e.NewValue);
				}
				else
				{
					// AS 9/4/08
					// The ActiveDate takes the value of the selected date into account
					// if there is no explicit active date.
					//
					cal.CoerceValue(ActiveDateProperty);
				}
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
        /// <p class="note"><b>Note:</b> When the <see cref="MinCalendarMode"/> is set to a value other than days, selecting 
        /// a <see cref="CalendarItem"/> will only add 1 entry for each selected item. It will not add each date in the item's 
        /// range into the SelectedDates.</p>
        /// <p class="note"><b>Note:</b> When in navigation mode (i.e. the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is 
        /// not equal to the <see cref="XamMonthCalendar.MinCalendarMode"/>), the selection cannot be changed via the ui. Instead 
        /// using the mouse will change the active item and the <see cref="CalendarItem.IsSelected"/> state will be based on which 
        /// item is active (<see cref="CalendarItem.IsActive"/>).</p>
        /// </remarks>
        /// <seealso cref="SelectedDateProperty"/>
        /// <seealso cref="SelectedDates"/>
        /// <seealso cref="SelectionType"/>
        /// <seealso cref="SelectedDatesChanged"/>
        //[Description("Returns or sets the date of the item that should be selected or null if no item is selected.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [TypeConverter(typeof(NullableConverter<DateTime>))]
        public DateTime? SelectedDate
        {
            get
            {
                return (DateTime?)this.GetValue(XamMonthCalendar.SelectedDateProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.SelectedDateProperty, value);
            }
        }

        #endregion //SelectedDate

		#region SelectedDates

		/// <summary>
		/// Returns a collection of <see cref="DateTime"/> instances that represents the currently selected dates of the items in the <see cref="MinCalendarMode"/>.
		/// </summary>
        /// <remarks>
        /// <p class="body">The SelectedDates is a collection of DateTime instances that represent the <see cref="CalendarItem"/> 
        /// instances that should be selected or have been selected by the end user. When the <see cref="SelectionType"/> is set 
        /// to a value that allows multiple selection such as <b>Range</b> or <b>Extended</b>, the control will allow selection of 
        /// one more CalendarItems. A single date for each item will be added to the SelectedDates. The <see cref="SelectedDate"/> 
        /// property can be used to access the first item.</p>
        /// <p class="note"><b>Note:</b> When the <see cref="MinCalendarMode"/> is set to a value other than days, selecting 
        /// a <see cref="CalendarItem"/> will only add 1 entry for each selected item. It will not add each date in the item's 
        /// range into the SelectedDates.</p>
        /// </remarks>
        /// <seealso cref="SelectedDate"/>
        /// <seealso cref="MaxSelectedDates"/>
        /// <seealso cref="SelectionType"/>
        /// <seealso cref="MinCalendarMode"/>
        /// <seealso cref="SelectedDatesChanged"/>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Bindable(true)]
		[Browsable(false)] // JJD 11/9/11 - TFS79598 - hide from designer
        public SelectedDateCollection SelectedDates
		{
			get { return this._selectedDates; }
		}
		#endregion //SelectedDates

		#region SelectionType

		/// <summary>
		/// Identifies the <see cref="SelectionType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionTypeProperty = DependencyProperty.Register("SelectionType",
			typeof(SelectionType), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(SelectionType.Default, new PropertyChangedCallback(OnSelectionTypeChanged)), new ValidateValueCallback(IsSelectionTypeValid));

        private static void OnSelectionTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;
            // AS 9/4/08
            // The value could go to default.
            //
            //SelectionType type = (SelectionType)e.NewValue;
            SelectionType type = cal.SelectionTypeResolved;

            if (SelectionStrategyBase.IsSingleSelectStrategy(type))
            {
                cal._pivotDate = cal.SelectedDate;

                if (cal.SelectedDates.Count > 1)
                    cal.SelectedDates.Reinitialize(new DateTime[] { cal.SelectedDates[0] });
            }
            else if (false == SelectionStrategyBase.IsMultiSelectStrategy(type))
            {
                // must be none so clear selection
                cal._pivotDate = null;
                cal.SelectedDates.Clear();
            }
        }

		private static bool IsSelectionTypeValid(object value)
		{
			return Enum.IsDefined(typeof(SelectionType), value);
		}

		/// <summary>
		/// Determines how many items can be selected.
		/// </summary>
        /// <remarks>
        /// <p class="body">The default selection type is Extended which allows multiple discontiguous 
        /// ranges of dates to be selected. As dates are selected, the <see cref="SelectedDatesChanged"/> 
        /// event is raised and the <see cref="SelectedDate"/> and <see cref="SelectedDates"/> properties 
        /// are updated. When using a multiple selection type such as Extended or Range, you can use the 
        /// <see cref="MaxSelectedDates"/> to control the maximum number of <see cref="CalendarItem"/> 
        /// instances that the user can select.</p>
        /// </remarks>
		/// <seealso cref="SelectionTypeResolved"/>
		/// <seealso cref="SelectedDates"/>
        /// <seealso cref="MaxSelectedDates"/>
        /// <seealso cref="SelectedDatesChanged"/>
        //[Description("Determines hows items can be selected")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public SelectionType SelectionType
		{
			get
			{
				return (SelectionType)this.GetValue(XamMonthCalendar.SelectionTypeProperty);
			}
			set
			{
				this.SetValue(XamMonthCalendar.SelectionTypeProperty, value);
			}
		}

		/// <summary>
		/// Returns the resolved value that determines how dates can be selected (read-only).
		/// </summary>
		/// <seealso cref="Infragistics.Windows.Controls.SelectionType"/>
		/// <seealso cref="SelectionType"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Browsable(false)]
		public SelectionType SelectionTypeResolved
		{
			get
			{
				SelectionType selectionType = this.SelectionType;

				if (selectionType == SelectionType.Default)
					selectionType = SelectionType.Extended;

				return selectionType;
			}
		}

		internal SelectionType CurrentSelectionTypeResolved
		{
			get
			{
				if (this.IsMinCalendarMode)
					return this.SelectionTypeResolved;

				return SelectionType.Single;
			}
		}

		#endregion //SelectionType

        #region ShowDisabledDaysOfWeek

        /// <summary>
        /// Identifies the <see cref="ShowDisabledDaysOfWeek"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowDisabledDaysOfWeekProperty = DependencyProperty.Register("ShowDisabledDaysOfWeek",
            typeof(bool), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShowDisabledDaysOfWeek)));

        private static void OnShowDisabledDaysOfWeek(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            if (

                cal.DisabledDaysOfWeek != DayOfWeekFlags.None)
            {
                DayOfWeekFlags hiddenDays = false.Equals(e.NewValue) ? cal.DisabledDaysOfWeek : DayOfWeekFlags.None;
                cal._calendarManager.HiddenDays = hiddenDays;

                // if the days of the week haven't changed then we need to 
                // tell the group to reset the enabled state
                if (false == cal.InitializeDaysOfWeek())
                {
                    cal.NotifyGroupChange(MonthCalendarChange.DisabledDatesChanged);
                }
            }
        }

        /// <summary>
        /// Returns or sets a boolean indicating whether days of the week disabled using the <see cref="DisabledDaysOfWeek"/> should be displayed in the calendar.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default all days of the week will be displayed within the control including those 
        /// that are disabled using the <see cref="DisabledDaysOfWeek"/>. This property can be set to true to 
        /// hide the disabled days of the week.</p>
        /// </remarks>
        /// <seealso cref="ShowDisabledDaysOfWeekProperty"/>
        /// <seealso cref="DisabledDaysOfWeek"/>
        //[Description("Returns or sets a boolean indicating whether days of the week disabled using the 'DisabledDaysOfWeek' should be displayed in the calendar.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public bool ShowDisabledDaysOfWeek
        {
            get
            {
                return (bool)this.GetValue(XamMonthCalendar.ShowDisabledDaysOfWeekProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.ShowDisabledDaysOfWeekProperty, value);
            }
        }

        #endregion //ShowDisabledDaysOfWeek

		#region ShowLeadingAndTrailingDates

		/// <summary>
		/// Identifies the <see cref="ShowLeadingAndTrailingDates"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowLeadingAndTrailingDatesProperty = DependencyProperty.Register("ShowLeadingAndTrailingDates",
			typeof(bool), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));

		/// <summary>
		/// Returns or sets a boolean indicating whether to show days from the month before/after the first/last visible month.
		/// </summary>
        /// <remarks>
        /// <p class="body">Leading and trailing dates are those dates displayed within a group that do not belong that to that particular 
        /// group. For example, when viewing a gregorian calendar that displays August 2008 with a default first day of week of 
        /// Sunday, a calendar has space available to show the 27-31 of July (leading) and the 1-6 of September (trailing). By default 
        /// leading and trailing dates are displayed within the first and last <see cref="CalendarItemGroup"/> within the control.</p>
        /// <p class="note"><b>Note:</b> The default template for the XamMonthCalendar uses a <see cref="CalendarItemGroupPanel"/> that 
        /// ensures that only the first and last CalendarItemGroup instances have their <see cref="CalendarItemGroup.ShowTrailingDates"/> 
        /// and <see cref="CalendarItemGroup.ShowLeadingDates"/> initialized based on this property. If you retemplate the control to 
        /// directly contain CalendarItemGroup instances you will need to control which groups use this property.</p>
        /// </remarks>
		/// <seealso cref="ShowLeadingAndTrailingDatesProperty"/>
        /// <seealso cref="CalendarItemGroup.ShowTrailingDates"/>
        /// <seealso cref="CalendarItemGroup.ShowLeadingDates"/>
        //[Description("Returns or sets a boolean indicating whether to show days from the month before/after the first/last visible month.")]
		//[Category("MonthCalendar Properties")] // Behavior
		[Bindable(true)]
		public bool ShowLeadingAndTrailingDates
		{
			get	{ return (bool)this.GetValue(XamMonthCalendar.ShowLeadingAndTrailingDatesProperty); }
			set	{ this.SetValue(XamMonthCalendar.ShowLeadingAndTrailingDatesProperty, value); }
		}

		#endregion //ShowLeadingAndTrailingDates

        #region TodayButtonVisibility

        /// <summary>
        /// Identifies the <see cref="TodayButtonVisibility"/> property
        /// </summary>
        public static readonly DependencyProperty TodayButtonVisibilityProperty = DependencyProperty.Register(
            "TodayButtonVisibility", typeof(Visibility), typeof(XamMonthCalendar), new PropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Returns or sets a boolean that indicates whether the today button should be displayed.
        /// </summary>
        /// <remarks>
        /// <p class="body">The Today button is used to allow the user to select and bring into view the 
        /// <see cref="CalendarItem"/> that represents the current date. This button uses the <see cref="MonthCalendarCommands.Today"/> 
        /// routed command to perform the operation.</p>
        /// </remarks>
        /// <seealso cref="TodayButtonStyleKey"/>
        //[Description("Returns or sets a value that indicates whether the today button should be displayed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility TodayButtonVisibility
        {
            get { return (Visibility)this.GetValue(TodayButtonVisibilityProperty); }
            set { this.SetValue(TodayButtonVisibilityProperty, value); }
        }

        #endregion //TodayButtonVisibility

		#region WeekNumberVisibility

		/// <summary>
		/// Identifies the <see cref="WeekNumberVisibility"/> property
		/// </summary>
		public static readonly DependencyProperty WeekNumberVisibilityProperty = DependencyProperty.Register(
            "WeekNumberVisibility", typeof(Visibility), typeof(XamMonthCalendar), new PropertyMetadata(KnownBoxes.VisibilityCollapsedBox));

		/// <summary>
		/// Returns or sets a boolean that indicates whether week numbers should be displayed.
		/// </summary>
        /// <seealso cref="CalendarItemArea.WeekNumbers"/>
        /// <seealso cref="CalendarItemArea.WeekNumberVisibility"/>
        //[Description("Returns or sets a value that indicates whether week numbers should be displayed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility WeekNumberVisibility
		{
            get { return (Visibility)this.GetValue(WeekNumberVisibilityProperty); }
			set { this.SetValue(WeekNumberVisibilityProperty, value); }
		}

		#endregion //WeekNumberVisibility

        #region Theme

        /// <summary>
        /// Identifies the 'Theme' dependency property
        /// </summary>
        public static readonly DependencyProperty ThemeProperty = ThemeManager.ThemeProperty.AddOwner(typeof(XamMonthCalendar), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnThemeChanged)));

        /// <summary>
        /// Routed event used to notify when the <see cref="Theme"/> property has been changed.
        /// </summary>
        public static readonly RoutedEvent ThemeChangedEvent = ThemeManager.ThemeChangedEvent.AddOwner(typeof(XamMonthCalendar));

        private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar control = target as XamMonthCalendar;

            control.OnThemeChanged((string)(e.OldValue), (string)(e.NewValue));
        }

        /// <summary>
        /// Gets/sets the default look for the control.
        /// </summary>
        /// <remarks>
        /// <para class="body">If left set to null then the default 'Generic' theme will be used. 
        /// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
        /// </remarks>
        /// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
        /// <seealso cref="ThemeProperty"/>
        //[Description("Gets/sets the general look of the XamMonthCalendar and its elements.")]
        //[Category("MonthCalendar Properties")] // MonthCalendar Properties // Appearance
        [Bindable(true)]
        [TypeConverter(typeof(Infragistics.Windows.Themes.Internal.EditorsThemeTypeConverter))]
        public string Theme
        {
            get
            {
                return (string)this.GetValue(XamMonthCalendar.ThemeProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.ThemeProperty, value);
            }
        }

        /// <summary>
        /// Used to raise the <see cref="ThemeChanged"/> event.
        /// </summary>
        protected virtual void OnThemeChanged(string previousValue, string currentValue)
        {
            RoutedPropertyChangedEventArgs<string> newEvent = new RoutedPropertyChangedEventArgs<string>(previousValue, currentValue);
            newEvent.RoutedEvent = XamMonthCalendar.ThemeChangedEvent;
            newEvent.Source = this;
            this.RaiseEvent(newEvent);
        }

        /// <summary>
        /// Occurs when the <see cref="Theme"/> property has been changed.
        /// </summary>
        //[Description("Occurs when the 'Theme' property has been changed.")]
        //[Category("MonthCalendar Properties")] // DockManager Events // Behavior
        public event RoutedPropertyChangedEventHandler<string> ThemeChanged
        {
            add
            {
                base.AddHandler(XamMonthCalendar.ThemeChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamMonthCalendar.ThemeChangedEvent, value);
            }
        }

        #endregion //Theme

		// AS 3/23/10 TFS26461
		#region Today

		private static readonly DependencyPropertyKey TodayPropertyKey =
			DependencyProperty.RegisterReadOnly("Today",
			typeof(DateTime), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(DateTime.Today));

		/// <summary>
		/// Identifies the <see cref="Today"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayProperty =
			TodayPropertyKey.DependencyProperty;

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
				return (DateTime)this.GetValue(XamMonthCalendar.TodayProperty);
			}
		}

		#endregion //Today

		#region WeekRule

		/// <summary>
		/// Identifies the <see cref="WeekRule"/> property
		/// </summary>
		public static readonly DependencyProperty WeekRuleProperty = DependencyProperty.Register(
			"WeekRule", typeof(CalendarWeekRule?), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnWeekRuleChanged)));

		private static void OnWeekRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthCalendar cal = d as XamMonthCalendar;

			if (cal != null)
			{
				cal._calendarManager.WeekRule = (CalendarWeekRule?)e.NewValue;

                cal.NotifyGroupChange(MonthCalendarChange.WeekRuleChanged);
			}
		}

		/// <summary>
		/// Returns or sets the rule used to determine the first week of the year.
		/// </summary>
        /// <remarks>
        /// <p class="body">By default the <see cref="DateTimeFormatInfo.CalendarWeekRule"/> of the Culture associated 
        /// with the control's <see cref="FrameworkElement.Language"/> is used to determine the week numbers for 
        /// dates displayed within the control. The WeekRule is used when calculating the 
        /// <see cref="CalendarItemArea.WeekNumbers"/>.</p>
        /// </remarks>
        //[Description("Returns or sets the rule used to determine the first week of the year.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [TypeConverter(typeof(NullableConverter<CalendarWeekRule>))]
        public CalendarWeekRule? WeekRule
		{
			get { return (CalendarWeekRule?)this.GetValue(WeekRuleProperty); }
			set { this.SetValue(WeekRuleProperty, value); }
		}

		#endregion //WeekRule

        #region Workdays

        internal const DayOfWeekFlags DefaultWorkdays = DayOfWeekFlags.Monday | DayOfWeekFlags.Tuesday | DayOfWeekFlags.Wednesday | DayOfWeekFlags.Thursday | DayOfWeekFlags.Friday;

        /// <summary>
        /// Identifies the <see cref="Workdays"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WorkdaysProperty = DependencyProperty.Register("Workdays",
            typeof(DayOfWeekFlags), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(DefaultWorkdays, new PropertyChangedCallback(OnWorkdaysChanged)));

        private static void OnWorkdaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;
            cal.NotifyGroupChange(MonthCalendarChange.WorkdaysChanged);
        }

        /// <summary>
        /// Returns or sets a flagged enumeration indicating which days of the week represent working days.
        /// </summary>
        /// <remarks>
        /// <p class="body">The Workdays is a flagged enumeration that may be set to one or more days. The WorkDays is 
        /// used to initialize the <see cref="CalendarDay.IsWorkday"/> to allow custom styling of days. This property 
        /// defaults to all days of the week excluding Saturday and Sunday.</p>
        /// </remarks>
        /// <seealso cref="WorkdaysProperty"/>
        /// <seealso cref="CalendarDay.IsWorkday"/>
        //[Description("Returns or sets a flagged enumeration indicating which days of the week represent working days.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public DayOfWeekFlags Workdays
        {
            get
            {
                return (DayOfWeekFlags)this.GetValue(XamMonthCalendar.WorkdaysProperty);
            }
            set
            {
                this.SetValue(XamMonthCalendar.WorkdaysProperty, value);
            }
        }

        #endregion //Workdays

		#endregion //Public

		#region Internal

		#region CalendarManager

		internal CalendarManager CalendarManager
		{
			get { return this._calendarManager; }
		}
		#endregion //CalendarManager

        #region IsNavigationMode
        internal bool IsNavigationMode
        {
            get { return this.IsMinCalendarMode == false; }
        } 
        #endregion //IsNavigationMode

        #region IsMinCalendarMode
        internal bool IsMinCalendarMode
        {
            get { return this.CurrentCalendarMode == this.MinCalendarMode; }
        } 
        #endregion //IsMinCalendarMode

        #region MaxCalendarMode
        internal CalendarMode MaxCalendarMode
        {
            get { return CalendarMode.Centuries; }
        } 
        #endregion //MaxCalendarMode

        #region MeasureDate

        internal static readonly DependencyProperty MeasureDateProperty = DependencyProperty.Register("MeasureDate",
            typeof(DateTime), typeof(XamMonthCalendar), new FrameworkPropertyMetadata(new DateTime(2008, 9, 1), null, new CoerceValueCallback(CoerceMeasureDate)));

        private static object CoerceMeasureDate(DependencyObject d, object newValue)
        {
            XamMonthCalendar cal = d as XamMonthCalendar;
            DateTime newDate = (DateTime)newValue;

            // ensure its valid for the current calendar
            newDate = cal._calendarManager.CoerceMinMaxDate(newDate);

            return newDate;
        }

        #endregion //MeasureDate

        #region MonthCalendar

        internal static readonly DependencyPropertyKey MonthCalendarPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly("MonthCalendar", typeof(XamMonthCalendar), typeof(XamMonthCalendar),
                new FrameworkPropertyMetadata((XamMonthCalendar)null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// MonthCalendar Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyProperty MonthCalendarProperty
            = MonthCalendarPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the containing <see cref="XamMonthCalendar"/>
        /// </summary>
        public static XamMonthCalendar GetMonthCalendar(DependencyObject d)
        {
            return (XamMonthCalendar)d.GetValue(MonthCalendarProperty);
        }

        #endregion //MonthCalendar

        #region SelectionHost
        internal ISelectionHost SelectionHost
        {
            get { return this; }
        }
        #endregion //SelectionHost

		#region SelectionStrategyFilter

		/// <summary>
		/// Returns or sets a filter for supplying selection stratgeies for items.
		/// </summary>
		/// <seealso cref="ISelectionStrategyFilter"/>
		/// <seealso cref="SelectionStrategyBase"/>
		//[Browsable(false)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		internal ISelectionStrategyFilter SelectionStrategyFilter
		{
			get { return this._selectionStrategyFilter; }
			set { this._selectionStrategyFilter = value; }
		}

		#endregion //SelectionStrategyFilter

		#endregion //Internal

		#region Private

		#region CurrentState

		private MonthCalendarStates CurrentState
		{
			get
			{
				MonthCalendarStates state = 0;

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

                    if (null != first && first.FirstDateOfGroup <= this.MinDate)
						state |= MonthCalendarStates.MinDateInView;

					if (null != last && last.LastDateOfGroup >= this.MaxDate)
						state |= MonthCalendarStates.MaxDateInView;

					if (CanActivate(DateTime.Today))
						state |= MonthCalendarStates.TodayIsEnabled;

					if (this.ActiveDate != null)
						state |= MonthCalendarStates.ActiveDate;

					if (this.CurrentCalendarMode > this.MinCalendarMode)
						state |= MonthCalendarStates.CanZoomInCalendarMode;

					
					if (this.CurrentCalendarMode < this.MaxCalendarMode
						// AS 10/14/09 FR11859
						//&& (false == this.IsDateInView(this.MinDate, true) || false == this.IsDateInView(this.MaxDate, true)))
                        && (false == this.IsDateInView(this.MinDate, true, false) || false == this.IsDateInView(this.MaxDate, true, false)))
						state |= MonthCalendarStates.CanZoomOutCalendarMode;

                    switch (this.CurrentCalendarMode)
                    {
                        case CalendarMode.Days:
                            state |= MonthCalendarStates.CalendarModeDays;
                            break;
                        case CalendarMode.Decades:
                            state |= MonthCalendarStates.CalendarModeDecades;
                            break;
                        case CalendarMode.Centuries:
                            state |= MonthCalendarStates.CalendarModeCenturies;
                            break;
                        case CalendarMode.Months:
                            state |= MonthCalendarStates.CalendarModeMonths;
                            break;
                        case CalendarMode.Years:
                            state |= MonthCalendarStates.CalendarModeYears;
                            break;
                    }

                    if (this.IsMinCalendarMode)
                        state |= MonthCalendarStates.MinCalendarMode;
				}

				// AS 1/5/10 TFS23198
				if (this.FlowDirection == FlowDirection.RightToLeft)
					state |= MonthCalendarStates.RightToLeft;

				return state;
			}
		}
		#endregion //CurrentState

        #region FirstGroup

        private CalendarItemGroup FirstGroup
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

        private CalendarItemGroup LastGroup
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
            Debug.Assert(this._shouldBringActiveDateIntoView == null);

            if (null != this._shouldBringActiveDateIntoView)
                this._shouldBringActiveDateIntoView = false;

            // make sure the calendar can support it
            date = this.CoerceMinMaxDate(date);

            if (this._groups.Count == 0)
                this.ReferenceDate = date;
            else
            {
                if (groupIndex < 0)
                {
                    // if its already in view then exit
					if (this.IsDateInView(date, ignoreLeadingAndTrailingDates))
                        return;

                    if (this._groups.Count == 1)
                        this.ReferenceDate = date;
                    else
                    {
                        CalendarMode mode = this.CurrentCalendarMode;

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

                        this.ReferenceDate = groupStartDate;
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
                        CalendarMode mode = this.CurrentCalendarMode;
                        DateTime dateToInitialize = this._calendarManager.GetGroupStartDate(date, mode);
                        dateToInitialize = this._calendarManager.AddGroupOffset(date, -groupIndex, mode, true);

                        this.ReferenceDate = dateToInitialize;
                    }
                }
            }
		}
		#endregion //BringDateIntoView

		#region ExecuteCommand

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="command">The RoutedCommand to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="MonthCalendarCommands"/>
		public bool ExecuteCommand(RoutedCommand command)
		{
			return this.ExecuteCommandImpl(new ExecuteCommandInfo(command));
		}

		/// <summary>
		/// Executes the specified RoutedCommand.
		/// </summary>
		/// <param name="commandInfo">Contains information about the command to execute.</param>
		/// <returns>True if command was executed, false if canceled.</returns>
		/// <seealso cref="MonthCalendarCommands"/>
		public bool ExecuteCommand(ExecuteCommandInfo commandInfo)
		{
			return this.ExecuteCommandImpl(commandInfo);
		}

		private bool ExecuteCommandImpl(ExecuteCommandInfo commandInfo)
		{
            Utils.ValidateNull("commandInfo", commandInfo);
            
            RoutedCommand command = commandInfo.RoutedCommand;
            object commandParameter = commandInfo.Parameter;
            object originalSource = commandInfo.OriginalSource;
            
			// Make sure we have a command to execute.
            Utils.ValidateNull("command", command);

			// Make sure the minimal control state exists to execute the command.
			if (MonthCalendarCommands.IsMinimumStatePresentForCommand(this as ICommandHost, command) == false)
				return false;

			// Fire the 'before executed' cancelable event.
			ExecutingCommandEventArgs beforeArgs = new ExecutingCommandEventArgs(command);

            if (false == this.RaiseExecutingCommand(beforeArgs))
            {
                // JJD 06/02/10 - TFS33112
                // Return the inverse of ContinueKeyRouting so that the developer can prevent
                // the original key message from bubbling
                //return false;
                return !beforeArgs.ContinueKeyRouting;
            }

			bool shiftKeyDown = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
			bool ctlKeyDown = (Keyboard.Modifiers & ModifierKeys.Control) != 0;

			// =========================================================================================
			// Determine which of our supported commands should be executed and do the associated action.
			bool handled = false;

            #region Scroll(Next|Previous)Group(s)
            if (command == MonthCalendarCommands.ScrollNextGroup ||
                command == MonthCalendarCommands.ScrollPreviousGroup)
            {
                int offset = command == MonthCalendarCommands.ScrollNextGroup ? 1 : -1;
                handled = this.ScrollGroups(offset);
            }
            else if (command == MonthCalendarCommands.ToggleActiveDateSelection)
            {
                handled = ToggleActiveDateSelection(shiftKeyDown, ctlKeyDown);
            }
            else if (command == MonthCalendarCommands.ScrollNextGroups ||
                command == MonthCalendarCommands.ScrollPreviousGroups)
            {
                int scrollCount = commandParameter is int
                    ? (int)commandParameter
					// AS 10/14/09 FR11859
					//: (this._groups[this._groups.Count - 1].ReferenceGroupOffset - this._groups[0].ReferenceGroupOffset) + 1;
                    : (this.GetLastGroup(false).ReferenceGroupOffset 
						- this.GetFirstGroup(false).ReferenceGroupOffset) + 1;

                if (command == MonthCalendarCommands.ScrollPreviousGroups)
                    scrollCount *= -1;

                handled = this.ScrollGroups(scrollCount);
            }
            #endregion //Scroll(Next|Previous)Group(s)
            else if (command == MonthCalendarCommands.ScrollToDate)
            {
                #region ScrollToDate
                if (commandParameter is DateTime)
                {
                    DateTime date = (DateTime)commandParameter;
                    CalendarItemGroup group = null;

                    // see if there is a group within the path and scroll the date into
                    // view in that group
                    if (originalSource is DependencyObject)
                    {
                        if (originalSource is CalendarItemGroup)
                            group = (CalendarItemGroup)originalSource;
                        else
                            group = (CalendarItemGroup)Utilities.GetAncestorFromType((DependencyObject)originalSource, typeof(CalendarItemGroup), true);
                    }

                    this.BringDateIntoView(date, group);
                    handled = true;
                }
                #endregion //ScrollToDate
            }
            else if (command == MonthCalendarCommands.ActivateDate)
            {
                #region ActivateDate

                // now get the date into which we will "zoom"
                DateTime? newActiveDate = null;

                if (commandParameter is DateTime)
                    newActiveDate = (DateTime)commandParameter;
                else
                {
                    CalendarItem item = commandParameter as CalendarItem;

                    // if the command parameter isn't an item but the original source was...
                    if (item == null && originalSource is DependencyObject)
                    {
                        if (originalSource is CalendarItem)
                            item = (CalendarItem)originalSource;
                        else
                            item = (CalendarItem)Utilities.GetAncestorFromType((DependencyObject)originalSource, typeof(CalendarItem), true);
                    }

                    if (item != null)
                        newActiveDate = item.StartDate;
                }

                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

                handled = this.ActivateDate(newActiveDate, false);

                #endregion //ActivateDate
            }
            else if (command == MonthCalendarCommands.ActivateSelectedDate)
            {
                #region ActivateSelectedDate

                this.ActiveDate = this.SelectedDate;
                handled = true;

                #endregion //ActivateSelectedDate
            }
            else if (command == MonthCalendarCommands.Today)
            {
                handled = this.ActivateDate(DateTime.Today);

				// AS 3/24/10 TFS28062
				// Note I'm always doing this to be consistent with outlook's behavior.
				// 
				this.RaiseSelectionCommitted(new RoutedEventArgs());
            }
            #region Next/Previous/First/Last
            else if (command == MonthCalendarCommands.PreviousItem
                || command == MonthCalendarCommands.NextItem)
            {
                // find the next/previous activatable date
                handled = this.ActivateDate(this.GetActivatableDate(this.ActiveDate, command == MonthCalendarCommands.PreviousItem, false));
            }
            else if (command == MonthCalendarCommands.PreviousItemRow
                || command == MonthCalendarCommands.NextItemRow)
            {
                if (null != this.ActiveDate)
                {
                    handled = this.NavigateRow(this.ActiveDate.Value, command == MonthCalendarCommands.NextItemRow);
                }
            }
            else if (command == MonthCalendarCommands.PreviousGroup
                || command == MonthCalendarCommands.NextGroup)
            {
                if (null != this.ActiveDate)
                    handled = this.NavigateGroup(this.ActiveDate.Value, command == MonthCalendarCommands.NextGroup);
            }
            #region (First|Last)ItemOfGroup
            else if (command == MonthCalendarCommands.FirstItemOfGroup)
            {
                if (null != this.ActiveDate)
                {
                    // start with the first date in a group and work foward
                    DateTime? newActiveDate = this.GetActivatableDate(this._calendarManager.GetGroupStartDate(this.ActiveDate.Value, this.CurrentCalendarMode), false, true);

                    handled = this.ActivateDate(newActiveDate);
                }
            }
            else if (command == MonthCalendarCommands.LastItemOfGroup)
            {
                if (null != this.ActiveDate)
                {
                    // start with the last date in a group and work backward
                    DateTime? newActiveDate = this.GetActivatableDate(this._calendarManager.GetGroupEndDate(this.ActiveDate.Value, this.CurrentCalendarMode), true, true);

                    handled = this.ActivateDate(newActiveDate);
                }
            }
            #endregion //(First|Last)ItemOfGroup
            else if (command == MonthCalendarCommands.FirstItemOfFirstGroup)
            {
				// AS 10/14/09 FR11859
				//CalendarItemGroup group = this.FirstGroup;
                CalendarItemGroup group = this.GetFirstGroup(false);

                if (null != group)
                {
                    DateTime? newActiveDate = this.GetActivatableDate(group.FirstDateOfGroup, false, true);
                    handled = this.ActivateDate(newActiveDate);
                }
            }
            else if (command == MonthCalendarCommands.LastItemOfLastGroup)
            {
				// AS 10/14/09 FR11859
				//CalendarItemGroup group = this.LastGroup;
                CalendarItemGroup group = this.GetLastGroup(false);

                if (null != group)
                {
                    DateTime? newActiveDate = this.GetActivatableDate(group.LastDateOfGroup, true, true);
                    handled = this.ActivateDate(newActiveDate);
                }
            }
            #endregion //Next/Previous/First/Last
            else if (command == MonthCalendarCommands.ZoomOutCalendarMode
                || command == MonthCalendarCommands.ZoomInCalendarMode)
            {
                #region (Dec|Inc)reaseCalendarMode

                this.ChangeCalendarMode(command == MonthCalendarCommands.ZoomOutCalendarMode, commandParameter, originalSource);

                handled = true;

                #endregion //(Dec|Inc)reaseCalendarMode
            }

        // =========================================================================================
#pragma warning disable 0164
        PostExecute:
#pragma warning restore 0164
            // If the command was executed, fire the 'after executed' event.
			if (handled == true)
				this.RaiseExecutedCommand(new ExecutedCommandEventArgs(command));

			// AS 10/15/09 TFS23867
			// We don't want to raise ExecutedCommand since we didn't process it but if 
			// the command was invoked as a result of a navigational keyboard key then 
			// we want to mark the event handled so wpf doesn't navigate outside of 
			// the control.
			if (commandParameter == MonthCalendarCommands.NavigationKeyParameter)
				handled = true;

			return handled;
		}

		#endregion //ExecuteCommandImpl

		#endregion //Public

		#region Internal

        #region CanActivate
        internal bool CanActivate(DateTime date)
        {
            return this.GetActivatableItemDate(date, this.CurrentCalendarMode) != null;
        }

        // AS 10/3/08 TFS8607
        // Added mode parameter since navigation mode should not consider
        // disabled days of week or dates.
        //
        internal bool CanActivate(DateTime start, DateTime end, CalendarMode mode)
        {
            return GetActivatableDate(start, end, true, mode) != null;
        }
        #endregion //CanActivate

        #region CoerceMinMaxDate
        internal DateTime CoerceMinMaxDate(DateTime date)
        {
            // strip out the time portion
            date = date.Date;

            // make sure this is within the min/max date range
            if (date > this.MaxDate)
                date = this.MaxDate;
            else if (date < this.MinDate)
                date = this.MinDate;

            return date;
        }
        #endregion //CoerceMinMaxDate

        // AS 2/9/09 TFS11631
        #region FocusActiveItemWithDelay
        internal void FocusActiveItemWithDelay()
        {
            if (!this.IsKeyboardFocusWithin)
                return;

            bool oldValue = _preventFocusActiveItem;
            _preventFocusActiveItem = true;

            try
            {
                this.Focus();

                this.Dispatcher.BeginInvoke(DispatcherPriority.Render, new Utils.MethodInvoker(FocusActiveItem));
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
                date = this.GetActivatableDate(item.StartDate, item.EndDate, true, this.CurrentCalendarMode);

            return date;
        }

        internal DateTime? GetActivatableDate(DateTime start, DateTime end, bool next)
        {
            return GetActivatableDate(start, end, next, this.CurrentCalendarMode);
        }

        // AS 10/3/08 TFS8607
        // Added mode parameter since navigation mode should not consider
        // disabled days of week or dates.
        //
        internal DateTime? GetActivatableDate(DateTime start, DateTime end, bool next, CalendarMode mode)
        {
            Debug.Assert(start <= end);

            // if the range is outside the min/max then it cannot be
            if (end < this.MinDate || start > this.MaxDate)
                return null;

            // AS 10/3/08 TFS8607
            // When in navigation mode, we want to ignore the disabled dates and days of week.
            //
			if (mode != this.MinCalendarMode)
			{
				// AS 9/2/09 TFS18434
				// Depending on how we are navigating we should return the end date.
				//
				if (!next)
					return end;

				return start;
			}

            DateTime actualStart = this.CoerceMinMaxDate(start);
            DateTime actualEnd = this.CoerceMinMaxDate(end);

            // if all the days of the week in the range are disabled it cannot be
            int disabledDays = (int)this.DisabledDaysOfWeek;
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
        // Added the CalendarMode as a param.
        //
        private DateTime? GetActivatableItemDate(DateTime date, CalendarMode mode)
        {
            DateTime start = this.CalendarManager.GetItemStartDate(date, mode);
            DateTime end = this.CalendarManager.GetItemEndDate(date, mode);

            return GetActivatableDate(start, end, true, mode);
        } 
        #endregion //GetActivatableItemDate

        #region GetGroup

        internal CalendarItemGroup GetGroup(DateTime date)
        {
            CalendarItemGroup containingGroup = null;

            if (this._groups.Count > 0)
			{
                for (int i = 0; i < this._groups.Count; i++)
				{
                    CalendarItemGroup group = this._groups[i];

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
        internal DateTime[] GetSelectableDates(CalendarDateRange range)
        {
            // ensure the range is start < end
            range.Normalize();

			// AS 1/8/10
			// Explicitly remove the time - this used to happen within the normalize.
			//
			range.RemoveTime();

            // nothing is selectable if its outside the min/max
            if (range.End > this.MaxDate || range.Start < this.MinDate)
                return new DateTime[0];

            // constrain the range to the current min/max
            if (range.Start < this.MinDate)
                range.Start = this.MinDate;
            else if (range.End > this.MaxDate)
                range.End = this.MaxDate;

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
            DateTime? date = this.GetActivatableDate(calItem);
            return this.InternalSelectItem(date, clearExistingSelection, select);
        }

        internal bool InternalSelectItem(DateTime? date,
			bool clearExistingSelection,
			bool select)
		{
			// we cannot do a selection if we were not given a day
            if (date == null && (clearExistingSelection == false || this._selectedDates.Count == 0))
				return false;

			// if the item isn't selectable, ignore the select flag
            if (date != null && select && this.CanActivate(date.Value) == false)
				select = false;

			// if the item is already selected/unselected...
            if (date != null && this.SelectedDates.IsSelected(date.Value) == select)
			{
				// and we're not clearing the selection - or we are but 
				// there is nothing selected - then exit
				if (clearExistingSelection == false || this._selectedDates.Count == 0)
					return true;
			}

            IList<DateTime> newSelection = this.CalculateNewSelection(date, clearExistingSelection, select);
            this.SelectedDates.Reinitialize(newSelection);
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

			//we cannot know the extent of the range if we don't have an end pt
			if (calItem == null)
				return false;

			if (this._pivotDate == null)
				this.SelectionHost.SetPivotItem(calItem, false);

            DateTime? date = this.GetActivatableDate(calItem);

            IList<DateTime> newSelection = this.CalculateNewSelectionRange(date, clearExistingSelection, select);
            this.SelectedDates.Reinitialize(newSelection);
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
                this.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
            }
        }
        #endregion //OnAutoGeneratedGroupsChanged

        #region OnGroupOffsetChanged
        internal void OnGroupOffsetChanged(CalendarItemGroup group, int oldOffset, int newOffset)
        {
            this.SortGroups();
        }
        #endregion //OnGroupOffsetChanged

        #region RegisterGroup
        internal void RegisterGroup(CalendarItemGroup group)
        {
            if (group.IsGroupForSizing)
            {
                Debug.Assert(this._groupForSizing == null || group == this._groupForSizing);
                this._groupForSizing = group;
            }
            else
            {
                Debug.Assert(false == this._groups.Contains(group));
                this._groups.Add(group);

                this.SortGroups();
            }
        }
        #endregion //RegisterGroup

        #region UnregisterGroup
        internal void UnregisterGroup(CalendarItemGroup group)
        {
            if (group.IsGroupForSizing)
            {
                Debug.Assert(group == this._groupForSizing);

                if (group == this._groupForSizing)
                    this._groupForSizing = null;
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
                this.NotifyGroupChange(MonthCalendarChange.SelectionChanged);
            }
        } 
        #endregion //OnSelectedStateChanged

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
                DateTime? activatableDate = this.GetActivatableItemDate(dateValue.Value, this.CurrentCalendarMode);

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
                    if (this._pivotDate == null)
                        this._pivotDate = this.SelectedDate ?? this.ActiveDate;

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
			DateTime? date,
			bool clearExistingSelection,
			bool select)
		{
            List<DateTime> selected = new List<DateTime>();

            Debug.Assert(date != null || clearExistingSelection);

			if (false == clearExistingSelection)
			{
				// copy existing selected items
				selected.AddRange(this._selectedDates);
			}

            if (null != date)
            {
                if (select)
                {
                    if (this.CanActivate(date.Value))
                        selected.Add(date.Value);
                }
                else
                    selected.Remove(date.Value);
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
			DateTime? date,
			bool clearExistingSelection,
			bool select)
		{
			List<DateTime> selected = new List<DateTime>();

            Debug.Assert(date != null || clearExistingSelection);

			if (null == this._pivotDate)
				return selected;

			DateTime rangeStartDate;
			DateTime rangeEndDate;

            if (null != date)
            {
                bool isInverted = false;

                if (this._pivotDate.Value <= date)
                {
                    rangeStartDate = this._pivotDate.Value;
                    rangeEndDate = date.Value;
                }
                else
                {
                    isInverted = true;
                    rangeStartDate = date.Value;
                    rangeEndDate = this._pivotDate.Value;
                }

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
                        rangeStartDate = this._pivotDate.Value;
                        rangeEndDate = date.Value;
                        adjustment = -1;
                    }

                    
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                    CalendarMode mode = this.CurrentCalendarMode;
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

        #region ChangeCalendarMode
        private void ChangeCalendarMode(bool zoomOut, object commandParameter, object originalSource)
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

            if (commandParameter is DateTime)
                startDate = (DateTime)commandParameter;

            if (zoomOut)
            {
                group = commandParameter as CalendarItemGroup;

                // if the command parameter isn't a group but the original source was...
                if (null == group && originalSource is DependencyObject)
                {
                    if (originalSource is CalendarItemGroup)
                        group = (CalendarItemGroup)originalSource;
                    else
                        group = (CalendarItemGroup)Utilities.GetAncestorFromType((DependencyObject)originalSource, typeof(CalendarItemGroup), true);
                }

                if (null == startDate && null != group)
                    startDate = group.FirstDateOfGroup;
            }
            else // decreasing mode...
            {
                CalendarItem item = commandParameter as CalendarItem;

                if (null == item && originalSource is DependencyObject)
                {
                    if (originalSource is CalendarItem)
                        item = (CalendarItem)originalSource;
                    else
                        item = (CalendarItem)Utilities.GetAncestorFromType((DependencyObject)originalSource, typeof(CalendarItem), true);
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
            //	startDate = this.ActiveDate ?? this.SelectedDate ?? this.ReferenceDate ?? this.CoerceMinMaxDate(DateTime.Today);

            // AS 10/3/08 TFS8607
            // If there is no group and we have focus then we will get the one 
            // with the active item if we have it.
            //
            DateTime? newReferenceDate = startDate;

            if (null == group && FocusWithinManager.CheckFocusWithinHelper(this))
            {
                // if there is an active date then we want to zoom the active group
                if (this.ActiveDate.HasValue)
                    group = this.GetGroup(this.ActiveDate.Value);

                // if we have that group then use its active date
                // as the new active date and reference date
                if (null != group && startDate == null)
                    newReferenceDate = startDate = this.ActiveDate;
            }

            int groupOffset = group != null ? group.ReferenceGroupOffset : 0;
            int modeOffset = zoomOut ? 1 : -1;
            CalendarMode newMode = (CalendarMode)(modeOffset + (int)this.CurrentCalendarMode);

            using (new CalendarItemGroup.GroupInitializationHelper(this._groups, false))
            {
                try
                {
                    // track the group we are focused on so we can zoom its contents
                    this._zoomFocusGroup = group;

                    this.CurrentCalendarMode = newMode;
                }
                finally
                {
                    this._zoomFocusGroup = null;
                }

                if (this.CurrentCalendarMode == newMode && null != newReferenceDate)
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

                    this.ReferenceDate = groupStartDate;
                }

                this.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
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
                    Debug.Assert(newMode == CalendarItemGroup.GetCurrentCalendarMode(activeItem));

					// AS 6/28/10 TFS32190
					// If focus is outside the control then we won't try to take focus however to 
					// maintain the previous behavior whereby the activedate would have been updated 
					// by the element when it got focus, we'll set the ActiveDate to make sure it 
					// has stored the same value.
					//
					//activeItem.Focus();
					if (this.IsKeyboardFocusWithin)
						activeItem.Focus();
					else
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

        #region CoerceAllDates
        private void CoerceAllDates()
        {
            using (new CalendarItemGroup.GroupInitializationHelper(this._groups))
            {
                // refresh the resolved min/max dates
                this.CoerceValue(XamMonthCalendar.MinDateProperty);
                this.CoerceValue(XamMonthCalendar.MaxDateProperty);
                this.CoerceValue(XamMonthCalendar.ActiveDateProperty);
                this.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
                this.CoerceValue(XamMonthCalendar.MeasureDateProperty);
            }
        }
        #endregion //CoerceAllDates

        #region CoerceIsTabStop
        private static object CoerceIsTabStop(DependencyObject d, object newValue)
        {
            XamMonthCalendar cal = (XamMonthCalendar)d;

            // when focus is moved within the calendar then we want to turn
            // off the istabstop so the user can shift tab out of the control
            // into the previous control
            if (cal.IsKeyboardFocused == false && cal.IsKeyboardFocusWithin)
                return KnownBoxes.FalseBox;

            return newValue;
        } 
        #endregion //CoerceIsTabStop

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

        // AS 2/9/09 TFS11631
        #region FocusActiveItem
        private void FocusActiveItem()
        {
            if (_preventFocusActiveItem)
                return;

            if (this.IsKeyboardFocusWithin == false)
                return;

            // if the control has an active date and we get focus
            // we should focus the active date
            if (null != this.ActiveDate)
            {
                CalendarItem item = this.GetItem(this.ActiveDate.Value);

                if (null != item)
                {
                    item.Focus();
                }
            }
        }
        #endregion //FocusActiveItem

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
                    newDate = this.CoerceMinMaxDate(newDate.Value);

                CalendarMode mode = this.CurrentCalendarMode;
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
                        if (newDate < this.MinDate || newDate > this.MaxDate)
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
            DateTime? next = this.GetActivatableDate(itemStart, this.MaxDate, true);
            DateTime? previous = this.GetActivatableDate(this.MinDate, itemStart, false);
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
            CalendarMode mode = this.CurrentCalendarMode;

            // make sure its within the min/max
            date = this.CoerceMinMaxDate(date);

            // then make sure its the start date of a group
            date = this._calendarManager.GetGroupStartDate(date, mode);

            // then check again to make sure its within the valid range
            date = this.CoerceMinMaxDate(date);

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
                if (null == lastGroupDate || lastGroupDate.Value > this.MaxDate)
                {
                    // calculate the date for the last group
                    lastGroupDate = this._calendarManager.GetGroupStartDate(this.MaxDate, mode);

                    // try to get the reference group offset using the last group as the anchor
                    DateTime? newDate = this._calendarManager.TryAddGroupOffset(lastGroupDate.Value, -lastGroupOffset, mode, true);

                    // if we can't build one the first group must be before the calendar's supported
                    // min date so use the min date
                    if (null == newDate || newDate.Value < this.MinDate)
                        date = this._calendarManager.GetGroupStartDate(this.MinDate, mode);
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
                    if (null == firstGroupDate || firstGroupDate < this.MinDate)
                    {
                        // start with our min...
                        firstGroupDate = this._calendarManager.GetGroupStartDate(this.MinDate, mode);

                        DateTime? newDate = this._calendarManager.TryAddGroupOffset(firstGroupDate.Value, -firstGroupOffset, mode, true);

                        // if we cannot build one then use the max date as the reference date
                        if (null == newDate)
                            date = this._calendarManager.GetGroupStartDate(this.MaxDate, mode);
                        else
                            date = newDate.Value;
                    }
                }

                date = this.CoerceMinMaxDate(date);
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
                GeneralTransform transform = items[i].TransformToAncestor(this);
                Rect rect = new Rect(items[i].RenderSize);

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
            if (this.IsInitialized == false)
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

        #region NavigateGroup
        private bool NavigateGroup(DateTime date, bool next)
        {
            CalendarMode mode = this.CurrentCalendarMode;
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
                        if ((next && groupEnd >= this.MaxDate) || (false == next && groupStart <= this.MinDate))
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
            CalendarMode mode = this.CurrentCalendarMode;
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
                        if ((next && rowEnd >= this.MaxDate) || (false == next && rowStart <= this.MinDate))
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
		private void NotifyGroupChange(MonthCalendarChange change)
        {
            if (null != this._groupForSizing)
                this._groupForSizing.OnCalendarChange(change);

            for (int i = 0, count = this._groups.Count; i < count; i++)
                this._groups[i].OnCalendarChange(change);

            if (change == MonthCalendarChange.DisabledDatesChanged)
                this.CoerceValue(ActiveDateProperty);
        } 
	    #endregion //NotifyGroupChange

		// AS 3/23/10 TFS26461
		// Previously the template was using the static DateTime.Today to initialize the today button's 
		// content since there was no property with change notifications exposed by the framework. Also 
		// when the date changed, the IsToday/ContainsToday properties of the items were not updated. 
		// To address this I created a helper class to watch for when the system date changes. We have 
		// to expose the date property so we can get to it in the template.
		//
		#region OnCurrentDateChanged
		private void OnCurrentDateChanged(object sender, EventArgs e)
		{
			this.NotifyGroupChange(MonthCalendarChange.TodayChanged);

			this.SetValue(TodayPropertyKey, _currentDateWatcher.GetValue(CurrentDate.ValueProperty));
		}
		#endregion //OnCurrentDateChanged

        #region OnDaysOfWeekChanged
        private void OnDaysOfWeekChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.CurrentCalendarMode == CalendarMode.Days)
                this.ClearPreferredNavInfo();

            this.NotifyGroupChange(MonthCalendarChange.DaysOfWeekChanged);
        }
        #endregion //OnDaysOfWeekChanged

        #region OnDisabledDatesChanged
        private void OnDisabledDatesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyGroupChange(MonthCalendarChange.DisabledDatesChanged);
        } 
        #endregion //OnDisabledDatesChanged

		#region OnLanguageChanged
		private static void OnLanguageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamMonthCalendar cal = (XamMonthCalendar)d;

			XmlLanguage lang = (XmlLanguage)e.NewValue;
			CultureInfo culture = null;
			try
			{
				culture = lang == null ? CultureInfo.CurrentCulture : lang.GetEquivalentCulture();
			}
			catch (InvalidOperationException)
			{
				culture = CultureInfo.CurrentCulture;
			}

            using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
            {
                cal.ClearPreferredNavInfo();
                cal._calendarManager.InitializeCulture(culture);
                cal.CoerceAllDates();
                cal.InitializeDaysOfWeek();
            }
		} 
		#endregion //OnLanguageChanged

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

            SelectionStrategyBase strategy = _activeStrategy == null ? null : Utilities.GetWeakReferenceTargetSafe(_activeStrategy) as SelectionStrategyBase;

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

        #region ScrollGroups

        private bool ScrollGroups(int numberOfGroups)
		{
            Debug.Assert(this._groups.Count > 0);

            if (this._groups.Count == 0)
				return false;

            DateTime date = this.ReferenceDate ?? this.GetDefaultReferenceDate();
            DateTime minDate = this.MinDate;
            DateTime maxDate = this.MaxDate;

			CalendarMode mode = this.CurrentCalendarMode;

			// adjust by the specified number of months
			date = this._calendarManager.AddGroupOffset(date, numberOfGroups, mode, true);

			if (date > maxDate)
			{
                // make sure the last month isn't beyond the max date
                DateTime firstMonthForMaxDate = this._calendarManager.AddGroupOffset(this._calendarManager.GetGroupStartDate(maxDate, mode), -this.LastGroup.ReferenceGroupOffset, mode, true);

				// if the new day is beyond the first month's date when 
				// 
				if (date > firstMonthForMaxDate)
					date = firstMonthForMaxDate;
			}

			if (date < minDate)
				date = minDate;

            DateTime? oldRefDate = this.ReferenceDate;
            this.ReferenceDate = date;

            // a scroll happened as long as the reference date changed
            return false == object.Equals(oldRefDate, this.ReferenceDate);
		}
		#endregion //ScrollGroups

		// AS 9/9/09 TFS18434
		// For the modes where the items can be in different columns depending on which group contains 
		// the item, we will not try to maintain the preferred column.
		//
		#region ShouldCachePreferredNavInfo
		private static bool ShouldCachePreferredNavInfo(CalendarMode mode)
		{
			switch (mode)
			{
				case CalendarMode.Centuries:
				case CalendarMode.Decades:
				case CalendarMode.Years:
					return false;
				case CalendarMode.Days:
				case CalendarMode.Months:
					return true;
				default:
					Debug.Fail("Unexpected mode:" + mode.ToString());
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

            IComparer<CalendarItemGroup> comparer = Utilities.CreateComparer(comparison);

            // we'll use our sort merge to maintain the order in which they were added
            Utilities.SortMergeGeneric<CalendarItemGroup>(this._groups, comparer);
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

		#endregion //Private

		#endregion //Methods

		#region Events

		#region ExecutingCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutingCommand"/> routed event
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
        /// <seealso cref="MonthCalendarCommands"/>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        public static readonly RoutedEvent ExecutingCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutingCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutingCommandEventArgs>), typeof(XamMonthCalendar));

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="ExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
		protected virtual void OnExecutingCommand(ExecutingCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal bool RaiseExecutingCommand(ExecutingCommandEventArgs args)
		{
			args.RoutedEvent = XamMonthCalendar.ExecutingCommandEvent;
			args.Source = this;
			this.OnExecutingCommand(args);

			return args.Cancel == false;
		}

		/// <summary>
		/// Occurs before a command is performed
		/// </summary>
		/// <seealso cref="OnExecutingCommand"/>
		/// <seealso cref="ExecutingCommandEvent"/>
		/// <seealso cref="ExecutingCommandEventArgs"/>
        /// <seealso cref="MonthCalendarCommands"/>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        //[Description("Occurs before a command is performed")]
		//[Category("MonthCalendar Properties")] // Behavior
		public event EventHandler<ExecutingCommandEventArgs> ExecutingCommand
		{
			add
			{
				base.AddHandler(XamMonthCalendar.ExecutingCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamMonthCalendar.ExecutingCommandEvent, value);
			}
		}

		#endregion //ExecutingCommand

		#region ExecutedCommand

		/// <summary>
		/// Event ID for the <see cref="ExecutedCommand"/> routed event
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
        /// <seealso cref="MonthCalendarCommands"/>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        public static readonly RoutedEvent ExecutedCommandEvent =
			EventManager.RegisterRoutedEvent("ExecutedCommand", RoutingStrategy.Bubble, typeof(EventHandler<ExecutedCommandEventArgs>), typeof(XamMonthCalendar));

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="ExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
		protected virtual void OnExecutedCommand(ExecutedCommandEventArgs args)
		{
			this.RaiseEvent(args);
		}

		internal void RaiseExecutedCommand(ExecutedCommandEventArgs args)
		{
			args.RoutedEvent = XamMonthCalendar.ExecutedCommandEvent;
			args.Source = this;
			this.OnExecutedCommand(args);
		}

		/// <summary>
		/// Occurs after a command is performed
		/// </summary>
		/// <seealso cref="OnExecutedCommand"/>
		/// <seealso cref="ExecutedCommandEvent"/>
		/// <seealso cref="ExecutedCommandEventArgs"/>
        /// <seealso cref="MonthCalendarCommands"/>
        /// <seealso cref="ExecuteCommand(RoutedCommand)"/>
        //[Description("Occurs after a command is performed")]
		//[Category("MonthCalendar Properties")] // Behavior
		public event EventHandler<ExecutedCommandEventArgs> ExecutedCommand
		{
			add
			{
				base.AddHandler(XamMonthCalendar.ExecutedCommandEvent, value);
			}
			remove
			{
				base.RemoveHandler(XamMonthCalendar.ExecutedCommandEvent, value);
			}
		}

		#endregion //ExecutedCommand

        #region SelectedDatesChanged

        /// <summary>
        /// Event ID for the <see cref="SelectedDatesChanged"/> routed event
        /// </summary>
        /// <seealso cref="SelectedDatesChanged"/>
        /// <seealso cref="OnSelectedDatesChanged"/>
        /// <seealso cref="SelectedDatesChangedEventArgs"/>
        public static readonly RoutedEvent SelectedDatesChangedEvent =
            EventManager.RegisterRoutedEvent("SelectedDatesChanged", RoutingStrategy.Bubble, typeof(EventHandler<SelectedDatesChangedEventArgs>), typeof(XamMonthCalendar));

        /// <summary>
        /// Occurs after the <see cref="XamMonthCalendar.SelectedDates"/> has been changed.
        /// </summary>
        /// <seealso cref="SelectedDatesChanged"/>
        /// <seealso cref="SelectedDatesChangedEvent"/>
        /// <seealso cref="SelectedDatesChangedEventArgs"/>
        protected virtual void OnSelectedDatesChanged(SelectedDatesChangedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectedDatesChanged(SelectedDatesChangedEventArgs args)
        {
            args.RoutedEvent = XamMonthCalendar.SelectedDatesChangedEvent;
            args.Source = this;
            this.OnSelectedDatesChanged(args);
        }

        /// <summary>
        /// Occurs after the <see cref="XamMonthCalendar.SelectedDates"/> has been changed.
        /// </summary>
        /// <seealso cref="OnSelectedDatesChanged"/>
        /// <seealso cref="SelectedDatesChangedEvent"/>
        /// <seealso cref="SelectedDatesChangedEventArgs"/>
        /// <seealso cref="SelectedDates"/>
        /// <seealso cref="SelectedDate"/>
        /// <seealso cref="SelectionType"/>
        //[Description("Occurs after the SelectedDates has been changed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        public event EventHandler<SelectedDatesChangedEventArgs> SelectedDatesChanged
        {
            add
            {
                base.AddHandler(XamMonthCalendar.SelectedDatesChangedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamMonthCalendar.SelectedDatesChangedEvent, value);
            }
        }

        #endregion //SelectedDatesChanged

		// AS 3/24/10 TFS28062
		// Renamed SelectionMouseUpEvent to SelectionCommitted to be more indicative of the usage.
		//
        #region SelectionCommittedEvent

        /// <summary>
        /// Event ID for the <see cref="SelectionCommittedEvent"/> routed event
        /// </summary>
        /// <seealso cref="SelectionCommittedEvent"/>
        /// <seealso cref="OnSelectionCommitted"/>
        internal static readonly RoutedEvent SelectionCommittedEvent =
            EventManager.RegisterRoutedEvent("SelectionCommitted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(XamMonthCalendar));

        /// <summary>
        /// Occurs after the mouse is release after a selection.
        /// </summary>
        private void OnSelectionCommitted(RoutedEventArgs args)
        {
            this.RaiseEvent(args);
        }

        internal void RaiseSelectionCommitted(RoutedEventArgs args)
        {
            args.RoutedEvent = XamMonthCalendar.SelectionCommittedEvent;
            args.Source = this;
            this.OnSelectionCommitted(args);
        }

        /// <summary>
        /// Occurs after the mouse is release after a selection.
        /// </summary>
        /// <seealso cref="OnSelectionCommitted"/>
        /// <seealso cref="SelectionCommittedEvent"/>
        internal event RoutedEventHandler SelectionCommitted
        {
            add
            {
                base.AddHandler(XamMonthCalendar.SelectionCommittedEvent, value);
            }
            remove
            {
                base.RemoveHandler(XamMonthCalendar.SelectionCommittedEvent, value);
            }
        }

        #endregion //SelectionCommittedEvent

        #endregion //Events

        #region Base class overrides

        #region Commands

        /// <summary>
		/// Overriden. Returns the commands supported by the <see cref="XamMonthCalendar"/>
		/// </summary>
		internal protected override CommandsBase Commands
		{
			get { return MonthCalendarCommands.Instance; }
		}
		#endregion //Commands

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the control's template has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// AS 10/15/09 TFS23543
			this.CoerceValue(XamMonthCalendar.ReferenceDateProperty);
		} 
		#endregion //OnApplyTemplate

        #region OnActiveSelectionStrategyChanged

        /// <summary>
        /// Invoked when the active <see cref="SelectionStrategyBase"/> used by the control has been changed.
        /// </summary>
        /// <param name="strategy">The new active strategy or null if no strategy is currently being used.</param>
        protected override void OnActiveSelectionStrategyChanged(SelectionStrategyBase strategy)
        {
            
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

            _activeStrategy = strategy == null ? null : new WeakReference(strategy);
            this.ProcessActiveSelectionStrategyChanged();

            base.OnActiveSelectionStrategyChanged(strategy);
        }

        #endregion //OnActiveSelectionStrategyChanged

        #region OnCreateAutomationPeer

        /// <summary>
        /// Returns <see cref="XamMonthCalendar"/> Automation Peer Class <see cref="XamMonthCalendarAutomationPeer"/>
        /// </summary>
        /// <returns>AutomationPeer</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new XamMonthCalendarAutomationPeer(this);
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

                this.FocusActiveItem();
            }
        }
        #endregion //OnGotKeyboardFocus

        #region OnInitialized

        /// <summary>
		/// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)
		{
            this.CoerceAllDates();
			this.InitializeDaysOfWeek();

			base.OnInitialized(e);
		}

		#endregion //OnInitialized

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
                RoutedCommand cmd = e.Delta < 0 
                    ? MonthCalendarCommands.ScrollNextGroups 
                    : MonthCalendarCommands.ScrollPreviousGroups;

                e.Handled = this.ExecuteCommandImpl(new ExecuteCommandInfo(cmd, Math.Abs(e.Delta / Utils.MouseWheelScrollDelta), this));
            }
        } 
        #endregion //OnMouseWheel

		#endregion //Base class overrides

		#region ICommandHost Members

		// SSP 3/18/10 TFS29783 - Optimizations
		// Changed CurrentState property to a method.
		// 
		long ICommandHost.GetCurrentState( long statesToQuery )
		{
			return (long)this.CurrentState & statesToQuery; 
		}

        bool ICommandHost.CanExecute(ExecuteCommandInfo commandInfo)
        {
            RoutedCommand command = commandInfo.RoutedCommand;
            return command != null && command.OwnerType == typeof(MonthCalendarCommands);
        }

        bool ICommandHost.Execute(ExecuteCommandInfo commandInfo)
        {
            return this.ExecuteCommandImpl(commandInfo);
        }

        #endregion // ICommandHost

		#region ISelectionHost Members

		#region ActivateItem

        // JJD 7/14/09 - TFS18784 
        // Added preventScrollItemIntoView param
        bool ISelectionHost.ActivateItem(ISelectableItem item, bool preventScrollItemIntoView)
		{
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
                Debug.Fail("Why are we trying to deactivate a navigation item?");
                return false;
            }

            return this.InternalSelectItem(item, false, false);
		} 
		#endregion //DeselectItem

		#region DeselectRange

		bool ISelectionHost.DeselectRange(ISelectableItem item)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return false;

            return this.InternalSelectRange(item, false, false);
		} 
		#endregion //DeselectRange

		#region DoAutoScrollHorizontal
		void ISelectionHost.DoAutoScrollHorizontal(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed)
		{
            if (direction == ScrollDirection.Decrement)
                this.ExecuteCommand(MonthCalendarCommands.ScrollPreviousGroup);
            else
                this.ExecuteCommand(MonthCalendarCommands.ScrollNextGroup);
        } 
		#endregion //DoAutoScrollHorizontal

		#region DoAutoScrollVertical

		void ISelectionHost.DoAutoScrollVertical(ISelectableItem item, ScrollDirection direction, ScrollSpeed speed)
		{
            if (direction == ScrollDirection.Decrement)
                this.ExecuteCommand(MonthCalendarCommands.ScrollPreviousGroup);
            else
                this.ExecuteCommand(MonthCalendarCommands.ScrollNextGroup);
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
			MonthCalendarStates currentState = this.CurrentState;
            SelectionStrategyBase strategy = this.SelectionHost.GetSelectionStrategyForItem(item);

            // only scroll during single select
            bool scrollingAllowed = null != strategy && strategy.IsSingleSelect == true && strategy.IsMultiSelect == false;
            bool scrollPrevious = scrollingAllowed && (currentState & MonthCalendarStates.MinDateInView) == 0;
            bool scrollNext = scrollingAllowed && (currentState & MonthCalendarStates.MaxDateInView) == 0;

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
            // AS 9/17/08
            // The default HitTest will include invisible items.
            //
            //HitTestResult hitResult = VisualTreeHelper.HitTest(this, pt);
            HitTestResult hitResult = null;
            HitTestResultCallback resultCallback = delegate(HitTestResult result)
            {
                hitResult = result;
                return HitTestResultBehavior.Stop;
            };
            HitTestFilterCallback filterCallback = delegate(DependencyObject hit)
            {
				// AS 10/15/09 TFS23860
				// Added If check - do not check for non-uielement types.
				//
				if (Utils.IsUIElementOrUIElement3D(hit))
				{
					if (false.Equals(hit.GetValue(UIElement.IsHitTestVisibleProperty)) ||
						//false.Equals(hit.GetValue(UIElement.IsEnabledProperty)) ||
						false.Equals(hit.GetValue(UIElement.IsVisibleProperty)))
						return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
				}

                // skip all disabled elements except calendar items
                if (false == hit is CalendarItem && false.Equals(hit.GetValue(UIElement.IsEnabledProperty)))
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;

                return HitTestFilterBehavior.Continue;
            };
            VisualTreeHelper.HitTest(this, filterCallback, resultCallback, new PointHitTestParameters(pt));
            DependencyObject objectHit = null != hitResult ? hitResult.VisualHit : null;

			CalendarItem calItem = objectHit as CalendarItem;

			if (calItem == null && objectHit != null)
				calItem = (CalendarItem)Utilities.GetAncestorFromType(objectHit, typeof(CalendarItem), true);

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
                if (calItem.IsEnabled == false && this._pivotDate != null)
                {
                    DateTime? dateToSelect;

                    if (this._pivotDate.Value > calItem.StartDate)
                    {
                        dateToSelect = GetActivatableDate(calItem.EndDate, this._pivotDate.Value, true);
                    }
                    else
                    {
                        dateToSelect = GetActivatableDate(this._pivotDate.Value, calItem.StartDate, false);
                    }

                    if (null != dateToSelect)
                        calItem = this.GetItem(dateToSelect.Value);
                }

                return calItem; 
            }
            #endregion //Over CalendarItem

			CalendarItemGroup group = objectHit as CalendarItemGroup;

			if (group == null)
				group = (CalendarItemGroup)Utilities.GetAncestorFromType(objectHit, typeof(CalendarItemGroup), true);

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
			Debug.Fail("Why do we need this? What if the pivot date is out of view?");

			if (null != this._pivotDate)
			{
				CalendarItem calendarItem = this.GetItem(this._pivotDate.Value);

				Debug.Assert(calendarItem != null, "The day of the pivot date is not in view!");

				return calendarItem;
			}

			return null;
		} 
		#endregion //GetPivotItem

		#region GetSelectionStrategyForItem

		SelectionStrategyBase ISelectionHost.GetSelectionStrategyForItem(ISelectableItem item)
		{
            SelectionStrategyBase strategy = null;

            // do not use the strategy when in navigation mode
            if (this.IsMinCalendarMode && this._selectionStrategyFilter != null)
                strategy = this._selectionStrategyFilter.GetSelectionStrategyForItem(item);

            if (null == strategy)
            {
                SelectionType type = this.CurrentSelectionTypeResolved;

                // we need to use a custom selection strategy to control the scroll interval
                if (type == SelectionType.Single && this._selectionStrategy is SelectionStrategySingle == false)
                    strategy = this._selectionStrategy = new CustomSingleSelectionStrategy(this);
                else
                    strategy = this._selectionStrategy = SelectionStrategyBase.GetSelectionStrategy(type, this.SelectionHost, this._selectionStrategy);
            }

			return strategy;
		} 
		#endregion //GetSelectionStrategyForItem

		#region IsItemSelectableWithCurrentSelection

		bool ISelectionHost.IsItemSelectableWithCurrentSelection(ISelectableItem item)
		{
			Debug.Assert(item is CalendarItem);
			return item is CalendarItem;
		} 
		#endregion //IsItemSelectableWithCurrentSelection

		#region IsMaxSelectedItemsReached

		bool ISelectionHost.IsMaxSelectedItemsReached(ISelectableItem item)
		{
            if (this.CurrentCalendarMode != this.MinCalendarMode)
                return false;

			int maxSelectedDates = this.MaxSelectedDatesResolved;

			return this._selectedDates.Count >= maxSelectedDates;
		} 
		#endregion //IsMaxSelectedItemsReached

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
			if (buttonArgs != null && 
				buttonArgs.ChangedButton == MouseButton.Left)
			{
				IInputElement hitTestElement = this.InputHitTest(e.GetPosition(this));

				CalendarItem item = hitTestElement as CalendarItem;

				if (item == null && hitTestElement is DependencyObject)
					item = (CalendarItem)Utilities.GetAncestorFromType((DependencyObject)hitTestElement, typeof(CalendarItem), true);

				if (null != item)
				{
                    if (this.CurrentCalendarMode > this.MinCalendarMode)
                        this.ExecuteCommandImpl(new ExecuteCommandInfo(MonthCalendarCommands.ZoomInCalendarMode, item, item));
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

			return this.InternalSelectItem(item, clearExistingSelection, true);
		} 
		#endregion //SelectItem

		#region SelectRange

		bool ISelectionHost.SelectRange(ISelectableItem item, bool clearExistingSelection)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return false;

			return this.InternalSelectRange(item, clearExistingSelection, true);
		} 
		#endregion //SelectRange

		#region SetPivotItem

		void ISelectionHost.SetPivotItem(ISelectableItem item, bool isRangeSelect)
		{
			DateTime? oldPivotDate = this._pivotDate;

			// Set pivotRcd item if shift key is not pressed.
			// Also set if we don't have a pivotRcd item (even if shift is pressed).
			//
			if (isRangeSelect && oldPivotDate != null)
				return;

			CalendarItem calItem = this.GetItem(item);

			if (calItem != null)
				this._pivotDate = calItem.StartDate;
			else
				this._pivotDate = null;
		} 
		#endregion //SetPivotItem

		#region SnapshotSelection

		void ISelectionHost.SnapshotSelection(ISelectableItem item)
		{
            Debug.Assert(this.IsMinCalendarMode);

            if (this.IsNavigationMode)
                return;

            // reset the snapshot based on the current selection
			this._selectedDatesSnapshot.ReInitialize(this._selectedDates);
		} 
		#endregion //SnapshotSelection

		#region TranslateItem

		ISelectableItem ISelectionHost.TranslateItem(ISelectableItem item)
		{
            if (item is CalendarItem == false)
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
                XamMonthCalendar cal = this.SelectionHost as XamMonthCalendar;

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