using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls;

namespace Infragistics.Windows.Editors
{
    /// <summary>
    /// A custom element used to display <see cref="CalendarItem"/> instances within a <see cref="CalendarItemGroup"/>
    /// </summary>
    //[System.ComponentModel.ToolboxItem(false)]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarItemArea : Control,
        ICalendarItemArea
    {
        #region Member Variables

        private CalendarItemGroup _group;
        private ObservableCollectionExtended<CalendarItem> _itemsInternal;
        private ReadOnlyObservableCollection<CalendarItem> _items;
        private ObservableCollectionExtended<int> _weekNumbersInternal;
        private ReadOnlyObservableCollection<int> _weekNumbers;
        private CalendarDateRange? _groupRange;

        #endregion //Member Variables

        #region Constructor
		static CalendarItemArea()
		{
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(typeof(CalendarItemArea)));
            UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
            CalendarItemGroup.CurrentCalendarModePropertyKey.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentCalendarModeChanged)));
        }

        /// <summary>
        /// Initializes a new <see cref="CalendarItemArea"/>
        /// </summary>
        public CalendarItemArea()
        {
            this._itemsInternal = new ObservableCollectionExtended<CalendarItem>();
            this._items = new ReadOnlyObservableCollection<CalendarItem>(this._itemsInternal);

            this._weekNumbersInternal = new ObservableCollectionExtended<int>();
            this._weekNumbers = new ReadOnlyObservableCollection<int>(this._weekNumbersInternal);

            this.InitializeCalendarModeSettings(CalendarItemGroup.GetCurrentCalendarMode(this));

            // AS 10/1/08 TFS8497
            // I need to get rid of the findancestor binding used in the itemtemplate
            // of the itemarea's itemscontrol. Otherwise when we initially measure the 
            // sizing group.
            //
            this.SetValue(ItemAreaPropertyKey, this);
        } 
        #endregion //Constructor

        #region Base class overrides

        #region LogicalChildren
        /// <summary>
        /// Returns an enumerator to iterate the logical children.
        /// </summary>
        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return this._itemsInternal.GetEnumerator();
            }
        }
        #endregion //LogicalChildren 

        #region OnInitialized

        /// <summary>
        /// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnInitialized(EventArgs e)
        {
            this.InitializeCalendarModeSettings(this.CurrentCalendarMode);
            this.InitializeItems();

            base.OnInitialized(e);
        }
        #endregion //OnInitialized

        #region OnVisualParentChanged
        /// <summary>
        /// Invoked when the visual parent of the element has been changed.
        /// </summary>
        /// <param name="oldParent">The previous visual parent</param>
        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            CalendarItemGroup group = this.TemplatedParent as CalendarItemGroup;

            if (null != group)
                this.InitializeGroup(group);

            base.OnVisualParentChanged(oldParent);
        } 
        #endregion //OnVisualParentChanged

        #endregion //Base class overrides

        #region Properties

        #region Internal

        #region GroupRange
        internal CalendarDateRange? GroupRange
        {
            get { return this._groupRange; }
        } 
        #endregion //GroupRange

        #region CalendarManager

        internal CalendarManager CalendarManager
        {
            get
            {
                return this._group != null
                    ? this._group.CalendarManager
                    : CalendarManager.CurrentCulture;
            }
        }
        #endregion //CalendarManager

        #region CurrentCalendarMode
        internal CalendarMode CurrentCalendarMode
        {
            get { return CalendarItemGroup.GetCurrentCalendarMode(this); }
        } 
        #endregion //CurrentCalendarMode

        #endregion //Internal

        #region Public

        #region DayOfWeekHeaderVisibility

        /// <summary>
        /// Identifies the <see cref="DayOfWeekHeaderVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DayOfWeekHeaderVisibilityProperty = XamMonthCalendar.DayOfWeekHeaderVisibilityProperty.AddOwner(
            typeof(CalendarItemArea), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceDayOfWeekHeaderVisibility)));

        private static object CoerceDayOfWeekHeaderVisibility(DependencyObject d, object newValue)
        {
            CalendarItemArea area = (CalendarItemArea)d;

            if (area.CurrentCalendarMode != CalendarMode.Days || area.Items.Count == 0)
                return KnownBoxes.VisibilityCollapsedBox;

            return newValue;
        }

        /// <summary>
        /// Returns a value indicating the visibility of the day of week headers.
        /// </summary>
        /// <seealso cref="DayOfWeekHeaderVisibilityProperty"/>
        /// <seealso cref="XamMonthCalendar.DayOfWeekHeaderVisibility"/>
        //[Description("Returns a value indicating the visibility of the day of week headers.")]
        //[Category("MonthCalendar Properties")] // Appearance
        [Bindable(true)]
        public Visibility DayOfWeekHeaderVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CalendarItemArea.DayOfWeekHeaderVisibilityProperty);
            }
            set
            {
                this.SetValue(CalendarItemArea.DayOfWeekHeaderVisibilityProperty, value);
            }
        }

        #endregion //DayOfWeekHeaderVisibility

        #region Items

        /// <summary>
        /// Returns a collection of <see cref="CalendarItem"/> instances that are displayed within the group.
        /// </summary>
        /// <remarks>
        /// <p class="body">The CalendarItem instances are generated based on the containing <see cref="CalendarItemGroup"/> using its 
        /// <see cref="CalendarItemGroup.FirstDateOfGroup"/>, <see cref="CalendarItemGroup.LastDateOfGroup"/>, <see cref="CalendarItemGroup.CurrentCalendarMode"/>,
        /// <see cref="CalendarItemGroup.ShowLeadingDates"/> and <see cref="CalendarItemGroup.ShowTrailingDates"/>.</p>
        /// </remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        [ReadOnly(true)]
        public ReadOnlyObservableCollection<CalendarItem> Items
        {
            get
            {
                return this._items;
            }
        }

        #endregion //Items

        #region FirstItemColumnOffset

        private static readonly DependencyPropertyKey FirstItemColumnOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly("FirstItemColumnOffset",
            typeof(int), typeof(CalendarItemArea), new FrameworkPropertyMetadata(Utils.ZeroInt));

        /// <summary>
        /// Identifies the <see cref="FirstItemColumnOffset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FirstItemColumnOffsetProperty =
            FirstItemColumnOffsetPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns an integer that represents the number of items between the first date in the <see cref="Items"/> collection and the first column.
        /// </summary>
        /// <seealso cref="FirstItemColumnOffsetProperty"/>
        /// <seealso cref="FirstItemRowOffset"/>
        [Browsable(false)]
        public int FirstItemColumnOffset
        {
            get
            {
                return (int)this.GetValue(CalendarItemArea.FirstItemColumnOffsetProperty);
            }
        }

        #endregion //FirstItemColumnOffset

        #region FirstItemRowOffset

        private static readonly DependencyPropertyKey FirstItemRowOffsetPropertyKey =
            DependencyProperty.RegisterReadOnly("FirstItemRowOffset",
            typeof(int), typeof(CalendarItemArea), new FrameworkPropertyMetadata(Utils.ZeroInt));

        /// <summary>
        /// Identifies the <see cref="FirstItemRowOffset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty FirstItemRowOffsetProperty =
            FirstItemRowOffsetPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns an integer that represents the number of items between the first date in the <see cref="Items"/> collection and the first row.
        /// </summary>
        /// <seealso cref="FirstItemRowOffsetProperty"/>
        /// <seealso cref="FirstItemColumnOffset"/>
        [Browsable(false)]
        public int FirstItemRowOffset
        {
            get
            {
                return (int)this.GetValue(CalendarItemArea.FirstItemRowOffsetProperty);
            }
        }

        #endregion //FirstItemRowOffset

        #region ItemArea

        private static readonly DependencyPropertyKey ItemAreaPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly("ItemArea", typeof(CalendarItemArea), typeof(CalendarItemArea),
                new FrameworkPropertyMetadata((CalendarItemArea)null,
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// ItemArea Read-Only Dependency Property
        /// </summary>
        public static readonly DependencyProperty ItemAreaProperty
            = ItemAreaPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the containing <see cref="CalendarItemArea"/>
        /// </summary>
        /// <remarks>
        /// <p class="body">The <see cref="ItemAreaProperty"/> is a readonly inherited property that is used to 
        /// provide the containing <see cref="CalendarItemArea"/> to the descendant elements such as the <see cref="CalendarItem"/> 
        /// instances in its <see cref="Items"/> collection.</p>
        /// </remarks>
        public static CalendarItemArea GetItemArea(DependencyObject d)
        {
            return (CalendarItemArea)d.GetValue(ItemAreaProperty);
        }

        #endregion //ItemArea

        #region ItemColumns

        private static readonly DependencyPropertyKey ItemColumnsPropertyKey =
            DependencyProperty.RegisterReadOnly("ItemColumns",
            typeof(int), typeof(CalendarItemArea), new FrameworkPropertyMetadata(Utils.ZeroInt));

        /// <summary>
        /// Identifies the <see cref="ItemColumns"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemColumnsProperty =
            ItemColumnsPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the number of columns that should be displayed in the group.
        /// </summary>
        /// <remarks>
        /// <p class="body">The ItemColumns is calculated based on the 
        /// <see cref="CalendarItemGroup.CurrentCalendarMode"/> as well as other factors. When the mode 
        /// is <b>Days</b>, the ItemColumns is based upon the number of visible days of the week which 
        /// is affected by the <see cref="XamMonthCalendar.ShowDisabledDaysOfWeek"/>.</p>
        /// </remarks>
        /// <seealso cref="ItemColumnsProperty"/>
        /// <seealso cref="ItemRows"/>
        //[Description("Returns the number of columns that should be displayed in the group.")]
        //[Category("MonthCalendar Properties")] // Layout
        [Bindable(true)]
        [ReadOnly(true)]
        public int ItemColumns
        {
            get
            {
                return (int)this.GetValue(CalendarItemArea.ItemColumnsProperty);
            }
        }

        #endregion //ItemColumns

        #region ItemRows

        private static readonly DependencyPropertyKey ItemRowsPropertyKey =
            DependencyProperty.RegisterReadOnly("ItemRows",
            typeof(int), typeof(CalendarItemArea), new FrameworkPropertyMetadata(Utils.ZeroInt));

        /// <summary>
        /// Identifies the <see cref="ItemRows"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ItemRowsProperty =
            ItemRowsPropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the number of item rows that should be displayed in the group.
        /// </summary>
        /// <remarks>
        /// <p class="body">The ItemRows is calculated based on the <see cref="CalendarItemGroup.CurrentCalendarMode"/>.</p>
        /// </remarks>
        /// <seealso cref="ItemRowsProperty"/>
        /// <seealso cref="ItemColumns"/>
        //[Description("Returns the number of item rows that should be displayed in the group.")]
        //[Category("MonthCalendar Properties")] // Layout
        [Bindable(true)]
        [ReadOnly(true)]
        public int ItemRows
        {
            get
            {
                return (int)this.GetValue(CalendarItemArea.ItemRowsProperty);
            }
        }

        #endregion //ItemRows

        #region WeekNumbers

        /// <summary>
        /// Returns a collection of integers that represent the week numbers of the days displayed within the month.
        /// </summary>
        /// <remarks>
        /// <p class="body">When the <see cref="XamMonthCalendar.CurrentCalendarMode"/> is <b>Days</b>, this will contain a set 
        /// of integers representing the week numbers for the days within the month. For all other navigation modes, the collection 
        /// will be empty. The <see cref="FirstItemRowOffset"/> is applicable to the week numbers as well since week numbers are 
        /// only generated based on the items within the <see cref="Items"/> collection.</p>
        /// </remarks>
        //[Description("Returns a collection of integers that represent the week numbers of the days displayed within the month.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ReadOnlyObservableCollection<int> WeekNumbers
        {
            get
            {
                return this._weekNumbers;
            }
        }

        #endregion //WeekNumbers

        #region WeekNumberVisibility

        /// <summary>
        /// Identifies the <see cref="WeekNumberVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty WeekNumberVisibilityProperty = XamMonthCalendar.WeekNumberVisibilityProperty.AddOwner(
            typeof(CalendarItemArea), new FrameworkPropertyMetadata(null, new CoerceValueCallback(CoerceWeekNumberVisibility)));

        private static object CoerceWeekNumberVisibility(DependencyObject d, object newValue)
        {
            CalendarItemArea area = (CalendarItemArea)d;

            if (area.CurrentCalendarMode != CalendarMode.Days || area.Items.Count == 0)
                return KnownBoxes.VisibilityCollapsedBox;

            return newValue;
        }

        /// <summary>
        /// Returns a value indicating whether week numbers should be displayed.
        /// </summary>
        /// <seealso cref="WeekNumberVisibilityProperty"/>
        /// <seealso cref="XamMonthCalendar.WeekNumberVisibility"/>
        //[Description("Returns a value indicating whether week numbers should be displayed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility WeekNumberVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CalendarItemArea.WeekNumberVisibilityProperty);
            }
            set
            {
                this.SetValue(CalendarItemArea.WeekNumberVisibilityProperty, value);
            }
        }

        #endregion //WeekNumberVisibility

        #endregion //Public

        #region Private

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Private

        #region InitializeCalendarModeSettings
        private void InitializeCalendarModeSettings(CalendarMode mode)
        {
            switch (mode)
            {
                case CalendarMode.Days:
                    this.SetValue(ItemRowsPropertyKey, 6);
                    this.SetValue(ItemColumnsPropertyKey, this.CalendarManager.VisibleDayCount);
                    break;
                case CalendarMode.Months:
                    {
                        // handle 13 month years
                        int rows = 3;

                        if (null != this._group && this._group.FirstDateOfGroup.HasValue)
                        {
                            DateTime firstDate = this._group.FirstDateOfGroup.Value;
                            System.Globalization.Calendar calendar = this.CalendarManager.Calendar;
                            int year = calendar.GetYear(firstDate);
                            rows = (calendar.GetMonthsInYear(year) + 3) / 4;
                        }

                        this.SetValue(ItemRowsPropertyKey, rows);
                        this.SetValue(ItemColumnsPropertyKey, 4);
                        break;
                    }
                case CalendarMode.Years:
                case CalendarMode.Decades:
                case CalendarMode.Centuries:
                    
                    this.SetValue(ItemRowsPropertyKey, 3);
                    this.SetValue(ItemColumnsPropertyKey, 4);
                    break;
            }
        }
        #endregion //InitializeCalendarModeSettings

        #region InitializeFirstItemOffset

        private void InitializeFirstItemOffset()
        {
            int colOffset = 0;
            int rowOffset = 0;

            if (this._items.Count > 0)
            {
                Debug.Assert(null != this._group);

                this.CalendarManager.GetItemRowColumn(this.Items[0].StartDate, this.CurrentCalendarMode,
                    this._group.FirstDateOfGroup.Value, this._group.ShowLeadingDates, out colOffset, out rowOffset);
            }

            this.SetValue(FirstItemColumnOffsetPropertyKey, colOffset);
            this.SetValue(FirstItemRowOffsetPropertyKey, rowOffset);
        }
        #endregion //InitializeFirstItemOffset

        #region InitializeItems
        private void InitializeItems()
        {
            if (false == this.IsInitialized)
                return;

			// JJD 07/12/12 - TFS113981
			// Remove assert that was interfering with Blend at design time
			//Debug.Assert(null != this._group);

            if (null == this._group)
                return;

            #region Setup

            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);
            CalendarManager calendarManager = this.CalendarManager;
            CalendarMode mode = this.CurrentCalendarMode;
            DateTime? firstDate = this._group.FirstDateOfGroup;
            DateTime? lastDate = this._group.LastDateOfGroup;
            DateTime[] dates;


            if (firstDate != null && lastDate != null)
            {
                DateTime minDate = cal != null ? cal.MinDate : DateTime.MinValue;
                DateTime maxDate = cal != null ? cal.MaxDate : DateTime.MaxValue;

                dates = calendarManager.GetItemDates(firstDate.Value, mode, this._group.ShowLeadingDates, this._group.ShowTrailingDates, minDate, maxDate);
                this._groupRange = new CalendarDateRange(firstDate.Value, lastDate.Value);
            }
            else
            {
                dates = new DateTime[0];
                this._groupRange = null;
            }

            bool createDays = mode == CalendarMode.Days;
            CalendarItem[] items = new CalendarItem[dates.Length];
            DateTime today = DateTime.Today;

            DateTime? activeDate;
            DayOfWeekFlags workingDays;
            bool isMinMode;
            Style itemStyle;
            StyleSelector itemStyleSelector;

            if (null != cal)
            {
                activeDate = cal.ActiveDate;
                workingDays = cal.Workdays;
                isMinMode = cal.IsMinCalendarMode;
                itemStyle = (Style)cal.GetValue(createDays ? XamMonthCalendar.CalendarDayStyleProperty : XamMonthCalendar.CalendarItemStyleProperty);
                itemStyleSelector = (StyleSelector)cal.GetValue(createDays ? XamMonthCalendar.CalendarDayStyleSelectorProperty : XamMonthCalendar.CalendarItemStyleSelectorProperty);
            }
            else
            {
                activeDate = null;
                workingDays = XamMonthCalendar.DefaultWorkdays;
                isMinMode = true;
                itemStyle = null;
                itemStyleSelector = null;
            }

            // try to recycle the items
            CalendarItem[] oldItems = new CalendarItem[this._itemsInternal.Count];
            this._itemsInternal.CopyTo(oldItems, 0);
            int nextOldItem = 0;
            bool hasCalDays = oldItems.Length > 0 && oldItems[0] is CalendarDay;
            bool hasCalItems = false == hasCalDays && oldItems.Length > 0;
            Style tempStyle;

            // AS 10/23/08 TFS9443
            // Do not recycle in the sizing group or else the relative sizing logic
            // used below to calculate the largest item could get the wrong item.
            //
            if (this._group != null && this._group.IsGroupForSizing)
                hasCalDays = hasCalItems = false;

            #endregion //Setup

            #region Create Items
            for (int i = 0; i < dates.Length; i++)
            {
                DateTime startDate = dates[i];
                DateTime endDate;

                bool isEnabled = true;
                CalendarItem item;
                bool isToday = false;

                if (createDays)
                {
                    endDate = startDate;
                    isToday = startDate == today;

                    if (hasCalDays)
                    {
                        // recycle an existing item
                        item = items[i] = oldItems[nextOldItem];

                        // bump the index and make sure we still have days
                        nextOldItem++;
                        hasCalDays = nextOldItem < oldItems.Length;

                        item.Recycle(startDate, endDate);
                    }
                    else
                    {
                        // just create a new one
                        item = items[i] = new CalendarDay(startDate, this._group);

                        // AS 10/1/08 TFS8497
                        // Make all the changes within begininit/endinit so 
                        // its Initialized event is fired after all the changes are made.
                        //
                        ((ISupportInitialize)item).BeginInit();
                    }

                    if (false == CalendarManager.IsSet(workingDays, calendarManager.Calendar.GetDayOfWeek(startDate)))
                        item.SetValue(CalendarDay.IsWorkdayPropertyKey, KnownBoxes.FalseBox);
                }
                else
                {
                    endDate = calendarManager.GetItemEndDate(startDate, mode);

                    if (hasCalItems)
                    {
                        item = items[i] = oldItems[nextOldItem];

                        // bump the index and make sure we still have days
                        nextOldItem++;
                        hasCalItems = nextOldItem < oldItems.Length;

                        item.Recycle(startDate, endDate);
                    }
                    else
                    {
                        item = items[i] = new CalendarItem(startDate, endDate, this._group);

                        // AS 10/1/08 TFS8497
                        // Make all the changes within begininit/endinit so 
                        // its Initialized event is fired after all the changes are made.
                        //
                        ((ISupportInitialize)item).BeginInit();
                    }

                    isToday = startDate <= today && today <= endDate;
                }

                if (isToday)
                {
                    if (isMinMode)
                        item.SetValue(CalendarDay.IsTodayPropertyKey, KnownBoxes.TrueBox);

                    item.SetValue(CalendarDay.ContainsTodayPropertyKey, KnownBoxes.TrueBox);
                }

                bool isActive = startDate <= activeDate && activeDate <= endDate;

                if (null != cal)
                {
                    // AS 10/3/08 TFS8607
                    // Added mode parameter since navigation mode should not consider
                    // disabled days of week or dates.
                    //
                    isEnabled = cal.CanActivate(startDate, endDate, mode);

                    if (isEnabled)
                    {
                        bool isSelected = false;
                        bool containsSelection = cal.SelectedDates.ContainsSelection(startDate, endDate);

                        // when in the minimum mode something is selected if its date is in the 
                        if (isMinMode)
                            isSelected = containsSelection;
                        else
                            isSelected = isActive;

                        // an item is also selected if its active and we're in navigation mode
                        if (isSelected)
                            item.SetValue(CalendarItem.IsSelectedPropertyKey, KnownBoxes.TrueBox);

                        if (containsSelection)
                            item.SetValue(CalendarItem.ContainsSelectedDatesPropertyKey, KnownBoxes.TrueBox);
                    }
                }

                if (false == isEnabled)
                    item.SetValue(IsEnabledProperty, KnownBoxes.FalseBox);

                if (startDate > lastDate || endDate < firstDate)
                    item.SetValue(CalendarItem.IsLeadingOrTrailingItemPropertyKey, KnownBoxes.TrueBox);

                if (isActive)
                    item.SetValue(CalendarItem.IsActivePropertyKey, KnownBoxes.TrueBox);

                
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

            }
            #endregion //Create Items

            #region Update Collection

            this._itemsInternal.Clear();

            // add in all the ones - new or recycled
            this._itemsInternal.AddRange(items);

            // remove any that were not recycled
            for (int i = nextOldItem; i < oldItems.Length; i++)
            {
                CalendarItem item = oldItems[i];
                this.RemoveLogicalChild(item);
            }

            // add the new ones as logical children
            for (int i = nextOldItem; i < items.Length; i++)
            {
                CalendarItem item = items[i];
                this.AddLogicalChild(item);

                // AS 10/1/08 TFS8497
                // Make all the changes within begininit/endinit so 
                // its Initialized event is fired after all the changes are made.
                //
                ((ISupportInitialize)item).EndInit();
            }

            // AS 10/21/08 TFS9395
            // Moved from above. We need to wait until after end init and after
            // its been added to the logical tree so it can get the inherited 
            // property values.
            //
            for (int i = 0; i < items.Length; i++)
            {
                CalendarItem item = items[i];

                if (null != itemStyleSelector)
                    tempStyle = itemStyleSelector.SelectStyle(item, item);
                else
                    tempStyle = null;

                // set or clear the style
                // AS 10/1/08 TFS8497
                // Setting the Style property to UnsetValue when it hasn't been set and isn't
                // initialized yet is causing it to not pick up a local style.
                //
                //item.SetValue(StyleProperty, tempStyle ?? itemStyle ?? DependencyProperty.UnsetValue);
                object style = tempStyle ?? itemStyle ?? DependencyProperty.UnsetValue;
                if (item.ReadLocalValue(StyleProperty) != style)
                    item.SetValue(StyleProperty, style);
            }

            // AS 10/1/08 TFS8497
            // To reduce overhead we only want to keep one item from the collection so if this
            // is for the sizing group then we want to measure each as they exist at this
            // point and keep the largest item only.
            //
            if (this._group != null && this._group.IsGroupForSizing && items.Length > 1)
            {
                double largestArea = -1d;
                Size infinite = new Size(double.PositiveInfinity, double.PositiveInfinity);
                CalendarItem largestItem = null;

                for (int i = 0; i < items.Length; i++)
                {
                    CalendarItem tempItem = items[i];

                    tempItem.Measure(infinite);
                    Size tempSize = tempItem.DesiredSize;
                    double tempArea = tempSize.Width * tempSize.Height;

                    if (tempArea > largestArea)
                    {
                        largestItem = tempItem;
                        largestArea = tempArea;
                    }
                }

                this._itemsInternal.ReInitialize(new CalendarItem[] { largestItem });

                for (int i = 0; i < items.Length; i++)
                {
                    CalendarItem tempItem = items[i];

                    if (tempItem != largestItem)
                        this.RemoveLogicalChild(tempItem);
                }
            }
            #endregion //Update Collection

            // AS 2/9/09 TFS11631
            // We can get into a similar situation if we are not animating if we 
            // reuse the items but don't reuse the active one so we should also 
            // shift focus to the control in this situation as well.
            //
            if (null != cal && (this.IsKeyboardFocusWithin || cal.IsKeyboardFocused))
                cal.FocusActiveItemWithDelay();

            this.InitializeFirstItemOffset();

            // reinitialize the week numbers
            this.InitializeWeekNumbers();

            this.CoerceValue(CalendarItemArea.WeekNumberVisibilityProperty);
            this.CoerceValue(CalendarItemArea.DayOfWeekHeaderVisibilityProperty);
        }

        #endregion //InitializeItems

        #region InitializeWeekNumbers

        private void InitializeWeekNumbers()
        {
            CalendarManager calendarManager = this.CalendarManager;
            DateTime? firstDateOfGroup = this._group != null ? this._group.FirstDateOfGroup : null;
            bool createWeekNumbers = this.CurrentCalendarMode == CalendarMode.Days && firstDateOfGroup != null && this.Items.Count > 0;

            if (false == createWeekNumbers && this._weekNumbersInternal.Count == 0)
                return;

            this._weekNumbersInternal.Clear();

            if (createWeekNumbers)
            {
                DateTime date = this._group.FirstDate;
                DateTime lastDate = this._group.LastDate;

                int colOffset, rowOffset;

                // the first date may not be in the first column so we need to find out
                // where it is
                calendarManager.GetItemRowColumn(date, CalendarMode.Days, firstDateOfGroup.Value, this._group.ShowLeadingDates, out colOffset, out rowOffset);

                // move to the next date based on that offset so the next row we're evaluating
                // the first date of that row. otherwise we may get too many or too little
                // week numbers.
                int offset = 7 - colOffset;

                for (int i = 0; i < 6; i++)
                {
                    int weekNumber = calendarManager.GetWeekNumberForDate(date);

                    if (i > 0 && this._weekNumbersInternal[i - 1] == weekNumber)
                        break;

                    this._weekNumbersInternal.Add(weekNumber);

                    date = calendarManager.AddItemOffset(date, offset, CalendarMode.Days);

                    // for all remaining weeks offset by a full week's dates
                    offset = 7;

                    // in case we don't have a full range because of the max date
                    if (date > this._group.LastDate)
                        break;
                }
            }
        }
        #endregion //InitializeWeekNumbers

        #region OnCurrentCalendarModeChanged
        private static void OnCurrentCalendarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemArea area = (CalendarItemArea)d;

            area.InitializeCalendarModeSettings((CalendarMode)e.NewValue);
            area.CoerceValue(WeekNumberVisibilityProperty);
            area.CoerceValue(DayOfWeekHeaderVisibilityProperty);

            // AS 10/1/08 TFS8497
            // The area needs to initialize its own items when its calendar mode has changed.
            area.InitializeItems();
        } 
        #endregion //OnCurrentCalendarModeChanged

        #region ReinitializeEnabledState
        private void ReinitializeEnabledState()
        {
            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

            if (null != cal)
            {
                CalendarMode mode = this.CurrentCalendarMode;

                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

                    // AS 10/3/08 TFS8607
                    // Added mode parameter since navigation mode should not consider
                    // disabled days of week or dates.
                    //
                    object value = cal.CanActivate(item.StartDate, item.EndDate, mode)
                        ? DependencyProperty.UnsetValue
                        : KnownBoxes.FalseBox;

                    item.SetValue(FrameworkElement.IsEnabledProperty, value);
                }
            }
        }
        #endregion //ReinitializeEnabledState

        #region ReinitializeSelectedState
        private void ReinitializeSelectedState()
        {
            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

            if (null != cal)
            {
                bool isNavMode = cal.IsNavigationMode;
                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

                    // contains selection is always the same
                    bool containsSelection = cal.SelectedDates.ContainsSelection(item.StartDate, item.EndDate);
                    bool selected = isNavMode ? item.IsActive : containsSelection;

                    item.SetValue(CalendarItem.IsSelectedPropertyKey, selected ? KnownBoxes.TrueBox : KnownBoxes.FalseBox);
                    item.SetValue(CalendarItem.ContainsSelectedDatesPropertyKey, containsSelection ? KnownBoxes.TrueBox : KnownBoxes.FalseBox);
                }
            }
        }
        #endregion //ReinitializeSelectedState

		#region ReinitializeStyle
		private void ReinitializeStyle()
        {
            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

            if (null != cal)
            {
                bool isDays = this.CurrentCalendarMode == CalendarMode.Days;
                Style style = isDays ? cal.CalendarDayStyle : cal.CalendarItemStyle;
                StyleSelector selector = isDays ? cal.CalendarDayStyleSelector : cal.CalendarItemStyleSelector;
                Style tempStyle;

                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

                    if (selector != null)
                        tempStyle = selector.SelectStyle(item, item);
                    else
                        tempStyle = null;

                    item.SetValue(FrameworkElement.StyleProperty, tempStyle ?? style ?? DependencyProperty.UnsetValue);
                }
            }
        }
        #endregion //ReinitializeStyle

		// AS 3/23/10 TFS26461
		#region ReinitializeTodayState
		private void ReinitializeTodayState()
        {
            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);

            if (null != cal)
            {
				DateTime today = DateTime.Today;
				bool isMinMode = cal.IsMinCalendarMode;

                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

					bool isToday = item.StartDate <= today && today <= item.EndDate;

					item.SetValue(CalendarDay.IsTodayPropertyKey, isMinMode && isToday ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);
					item.SetValue(CalendarDay.ContainsTodayPropertyKey, isToday ? KnownBoxes.TrueBox : DependencyProperty.UnsetValue);
                }
            }
		}
		#endregion //ReinitializeTodayState

        #endregion //Private

        #region Internal

        #region GetItem
        internal CalendarItem GetItem(DateTime date)
        {
            IList<CalendarItem> items = this.Items;
            int low = 0;
            int high = items.Count - 1;

            while (low <= high)
            {
                int mid = low + ((high - low) / 2);
                int comparison = items[mid].CompareTo(date);

                if (comparison == 0)
                    return items[mid];

                if (comparison > 0)
                    high = mid - 1;
                else
                    low = mid + 1;
            }

            return null;
        } 
        #endregion //GetItem

        #region InitializeGroup
        internal void InitializeGroup(CalendarItemGroup group)
        {
            Debug.Assert(null == this._group || group == this._group);

            if (this._group == group)
                return;

            if (null != this._group)
                this._group.UnregisterItemArea(this);

            this._group = group;

            if (null != this._group)
            {
                if (group == this.TemplatedParent)
                {
                    this._group.RegisterItemArea(this);

                    // AS 10/1/08 TFS8497
                    // Let the group do this when we register the item area.
                    //
                    //((ICalendarItemArea)this).InitializeItems();
                }
            }
        }
        #endregion //InitializeGroup

        #endregion //Internal

        #endregion //Methods

        #region ICalendarItemArea Members

        void ICalendarItemArea.InitializeItems()
        {
            this.InitializeItems();
        }

        void ICalendarItemArea.InitializeWeekNumbers()
        {
            this.InitializeWeekNumbers();
        }

        void ICalendarItemArea.InitializeFirstItemOffset()
        {
            this.InitializeFirstItemOffset();
        }

        void ICalendarItemArea.InitializeRowColCount()
        {
            this.InitializeCalendarModeSettings(this.CurrentCalendarMode);
        }

        IList<CalendarItem> ICalendarItemArea.Items
        {
            get { return this.Items; }
        }

        void ICalendarItemArea.ReinitializeItems(CalendarItemChange change)
        {
            switch (change)
            {
                case CalendarItemChange.Enabled:
                    this.ReinitializeEnabledState();
                    break;
                case CalendarItemChange.Style:
                    this.ReinitializeStyle();
                    break;
                case CalendarItemChange.Selection:
                    this.ReinitializeSelectedState();
                    break;
				// AS 3/23/10 TFS26461
				case CalendarItemChange.Today:
					this.ReinitializeTodayState();
					break;
                default:
                    Debug.Fail("Unexpected change type!");
                    ((ICalendarItemArea)this).InitializeItems();
                    break;
            }
        }

        void ICalendarItemArea.PrepareForAnimationAction(CalendarAnimation action)
        {
        }

        void ICalendarItemArea.PerformAnimationAction(CalendarAnimation action)
        {
            // no animation when embedded directly
        }

        CalendarItem ICalendarItemArea.GetItem(DateTime date)
        {
            return this.GetItem(date);
        }
        #endregion //ICalendarItemArea Members
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