using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Primitives;
using System.Windows.Media;

namespace Infragistics.Controls.Editors.Primitives
{
    /// <summary>
    /// A custom element used to display <see cref="CalendarItem"/> instances within a <see cref="CalendarItemGroup"/>
    /// </summary>
    //[System.ComponentModel.ToolboxItem(false)]
	[TemplatePart(Name = PartDayOfWeekPanel, Type = typeof(CalendarDayOfWeekPanel))]
	[TemplatePart(Name = PartWeekNumberPanel, Type = typeof(CalendarWeekNumberPanel))]
	[TemplatePart(Name = PartItemPanel, Type = typeof(UniformGrid))]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!


	public class CalendarItemArea : Control,
        ICalendarItemArea, ICalendarElement



    {
        #region Member Variables

		private const string PartDayOfWeekPanel = "DayOfWeekPanel";
		private const string PartWeekNumberPanel = "WeekNumberPanel";
		private const string PartItemPanel = "ItemPanel";

		private CalendarDayOfWeekPanel _dayOfWeekPanel;
		private CalendarWeekNumberPanel _weekNumberPanel;
		private UniformGrid _itemPanel;

        private CalendarItemGroup _group;
        private ObservableCollectionExtended<CalendarItem> _itemsInternal;
        private ReadOnlyObservableCollection<CalendarItem> _items;
        private ObservableCollectionExtended<int> _weekNumbersInternal;
        private ReadOnlyObservableCollection<int> _weekNumbers;
        private DateRange? _groupRange;
		private bool _isInitialized;
		
		private DateTime? _firstDate;// JJD 5/2/11 - TFS74024 - added
        private DateTime? _lastDate;// JJD 5/2/11 - TFS74024 - added

        #endregion //Member Variables

        #region Constructor
		static CalendarItemArea()
		{

            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(typeof(CalendarItemArea)));
            //UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
           //CalendarItemGroup.CurrentModePropertyKey.OverrideMetadata(typeof(CalendarItemArea), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnCurrentModeChanged)));
										

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

            this.InitializeCalendarModeSettings(CalendarItemGroup.GetCurrentMode(this));

			this.Loaded += new RoutedEventHandler(OnLoaded);
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




#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)


		#region OnApplyTemplate

		/// <summary>
		/// Invoked when the control's template has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			if (_isInitialized == false)
			{
				_isInitialized = true;
				this.InitializeItems();
			}

			CalendarBase cal = CalendarUtilities.GetCalendar(this);

			#region PartDayOfWeekPanel

			CalendarDayOfWeekPanel dayOfWeek = this.GetTemplateChild(PartDayOfWeekPanel) as CalendarDayOfWeekPanel;

			if (dayOfWeek != _dayOfWeekPanel)
			{
				if (_dayOfWeekPanel != null)
					_dayOfWeekPanel.ClearValue(CalendarBase.CalendarPropertyKey);

				_dayOfWeekPanel = dayOfWeek;

				if (_dayOfWeekPanel != null && cal != null)
					CalendarBase.SetCalendar(_dayOfWeekPanel, cal);
			}

			#endregion //PartDayOfWeekPanel

			#region PartWeekNumberPanel

			CalendarWeekNumberPanel weekNumbers = this.GetTemplateChild(PartWeekNumberPanel) as CalendarWeekNumberPanel;

			if (weekNumbers != _weekNumberPanel)
			{
				if (_weekNumberPanel != null)
				{
					_weekNumberPanel.InitializeItemsArea(null);
					_weekNumberPanel.ClearValue(CalendarBase.CalendarPropertyKey);
				}

				_weekNumberPanel = weekNumbers;

				if (_weekNumberPanel != null && cal != null)
				{
					_weekNumberPanel.InitializeItemsArea(this);
					CalendarBase.SetCalendar(_weekNumberPanel, cal);
				}
			}

			#endregion //PartWeekNumberPanel

			#region PartItemPanel

			UniformGrid items = this.GetTemplateChild(PartItemPanel) as UniformGrid;

			if (items != _itemPanel)
			{
				if (_itemPanel != null)
				{
					_itemPanel.Children.Clear();
				}

				_itemPanel = items;

				if (_itemPanel != null && cal != null)
				{
					foreach (CalendarItem item in this.Items)
						_itemPanel.Children.Add(item);
				}
			}

			#endregion //PartItemPanel
		}

		#endregion //OnApplyTemplate	

		#region OnInitialized


		/// <summary>
		/// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)



		{

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

		#region Group

		internal CalendarItemGroup Group { get { return this._group; } }

		#endregion //Group	
    
        #region GroupRange
        internal DateRange? GroupRange
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

        #region CurrentMode
        internal CalendarZoomMode CurrentMode
        {
            get { return CalendarItemGroup.GetCurrentMode(this); }
        } 
        #endregion //CurrentMode

        #endregion //Internal

        #region Public

		#region ComputedItemsBorderThickness

		private static readonly DependencyPropertyKey ComputedItemsBorderThicknessPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedItemsBorderThickness",
			typeof(Thickness), typeof(CalendarItemArea), new Thickness(0), null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedItemsBorderThickness"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedItemsBorderThicknessProperty = ComputedItemsBorderThicknessPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the border thickness around the items (read-only).
		/// </summary>
		/// <seealso cref="ComputedItemsBorderThicknessProperty"/>
		public Thickness ComputedItemsBorderThickness
		{
			get
			{
				return (Thickness)this.GetValue(CalendarItemArea.ComputedItemsBorderThicknessProperty);
			}
			private set
			{
				this.SetValue(CalendarItemArea.ComputedItemsBorderThicknessPropertyKey, value);
			}
		}

		#endregion //ComputedItemsBorderThickness

		#region DayOfWeekHeaderVisibility

		private static readonly DependencyPropertyKey DayOfWeekHeaderVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DayOfWeekHeaderVisibility",
			typeof(Visibility), typeof(CalendarItemArea), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="DayOfWeekHeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayOfWeekHeaderVisibilityProperty = DayOfWeekHeaderVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a value indicating the visibility of the day of week headers (read-only).
		/// </summary>
		/// <seealso cref="DayOfWeekHeaderVisibilityProperty"/>
		/// <seealso cref="CalendarBase.DayOfWeekHeaderVisibility"/>
		//[Description("Returns a value indicating the visibility of the day of week headers.")]
		//[Category("MonthCalendar Properties")] // Appearance
		[Bindable(true)]
		[ReadOnly(true)]
		public Visibility DayOfWeekHeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarItemArea.DayOfWeekHeaderVisibilityProperty);
			}
			private set
			{
				this.SetValue(CalendarItemArea.DayOfWeekHeaderVisibilityPropertyKey, value);
			}
		}

        #endregion //DayOfWeekHeaderVisibility

        #region Items

        /// <summary>
        /// Returns a collection of <see cref="CalendarItem"/> instances that are displayed within the group.
        /// </summary>
        /// <remarks>
        /// <p class="body">The CalendarItem instances are generated based on the containing <see cref="CalendarItemGroup"/> using its 
        /// <see cref="CalendarItemGroup.FirstDateOfGroup"/>, <see cref="CalendarItemGroup.LastDateOfGroup"/>, <see cref="CalendarItemGroup.CurrentMode"/>,
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

		private static readonly DependencyPropertyKey FirstItemColumnOffsetPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("FirstItemColumnOffset",
			typeof(int), typeof(CalendarItemArea), CalendarUtilities.ZeroInt, null);

		/// <summary>
		/// Identifies the read-only <see cref="FirstItemColumnOffset"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstItemColumnOffsetProperty = FirstItemColumnOffsetPropertyKey.DependencyProperty;

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
			internal set
			{
				this.SetValue(CalendarItemArea.FirstItemColumnOffsetPropertyKey, value);
			}
		}

        #endregion //FirstItemColumnOffset
		
		#region FirstItemRowOffset

		private static readonly DependencyPropertyKey FirstItemRowOffsetPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("FirstItemRowOffset",
			typeof(int), typeof(CalendarItemArea), CalendarUtilities.ZeroInt, null);

		/// <summary>
		/// Identifies the read-only <see cref="FirstItemRowOffset"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstItemRowOffsetProperty = FirstItemRowOffsetPropertyKey.DependencyProperty;

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
			internal set
			{
				this.SetValue(CalendarItemArea.FirstItemRowOffsetPropertyKey, value);
			}
		}

		#endregion //FirstItemRowOffset

		#region ItemColumns

		private static readonly DependencyPropertyKey ItemColumnsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ItemColumns",
			typeof(int), typeof(CalendarItemArea), CalendarUtilities.ZeroInt, null);

		/// <summary>
		/// Identifies the read-only <see cref="ItemColumns"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemColumnsProperty = ItemColumnsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the number of columns that should be displayed in the group.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ItemColumns is calculated based on the 
		/// <see cref="CalendarItemGroup.CurrentMode"/> as well as other factors. When the mode 
		/// is <b>Days</b>, the ItemColumns is based upon the number of visible days of the week which 
		/// is affected by the <see cref="XamCalendar.DisabledDaysOfWeekVisibility"/>.</p>
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
			internal set
			{
				this.SetValue(CalendarItemArea.ItemColumnsPropertyKey, value);
			}
		}

        #endregion //ItemColumns

		#region ItemRows

		private static readonly DependencyPropertyKey ItemRowsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ItemRows",
			typeof(int), typeof(CalendarItemArea), CalendarUtilities.ZeroInt, null);

		/// <summary>
		/// Identifies the read-only <see cref="ItemRows"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemRowsProperty = ItemRowsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the number of item rows that should be displayed in the group.
		/// </summary>
		/// <remarks>
		/// <p class="body">The ItemRows is calculated based on the <see cref="CalendarItemGroup.CurrentMode"/>.</p>
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
			internal set
			{
				this.SetValue(CalendarItemArea.ItemRowsPropertyKey, value);
			}
		}

        #endregion //ItemRows

		#region WeekNumberVisibility

		private static readonly DependencyPropertyKey WeekNumberVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("WeekNumberVisibility",
			typeof(Visibility), typeof(CalendarItemArea), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="WeekNumberVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekNumberVisibilityProperty = WeekNumberVisibilityPropertyKey.DependencyProperty;

       /// <summary>
        /// Returns a value indicating whether week numbers should be displayed (read-only).
        /// </summary>
        /// <seealso cref="WeekNumberVisibilityProperty"/>
        /// <seealso cref="CalendarBase.WeekNumberVisibility"/>
        //[Description("Returns a value indicating whether week numbers should be displayed.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
		[ReadOnly(true)]
		public Visibility WeekNumberVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarItemArea.WeekNumberVisibilityProperty);
			}
			private set
			{
				this.SetValue(CalendarItemArea.WeekNumberVisibilityPropertyKey, value);
			}
		}

		#endregion //WeekNumberVisibility

        #endregion //Public

		#region Internal

		// JJD 5/2/11 - TFS74024 - added
		#region First/LastDate

		internal DateTime? FirstDate { get { return _firstDate; } }
		internal DateTime? LastDate { get { return _lastDate; } }

		#endregion //First/LastDate	
    
		#region WeekNumbers



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		internal ReadOnlyObservableCollection<int> WeekNumbers
		{
			get
			{
				return this._weekNumbers;
			}
		}

		#endregion //WeekNumbers

		#endregion //Internal	
    
        #region Private

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Private

        #region InitializeCalendarModeSettings
        private void InitializeCalendarModeSettings(CalendarZoomMode mode)
        {
            switch (mode)
            {
                case CalendarZoomMode.Days:
                    this.SetValue(ItemRowsPropertyKey, 6);
                    this.SetValue(ItemColumnsPropertyKey, this.CalendarManager.VisibleDayCount);
                    break;
                case CalendarZoomMode.Months:
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
                case CalendarZoomMode.Years:
                case CalendarZoomMode.Decades:
                case CalendarZoomMode.Centuries:
                    
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

                this.CalendarManager.GetItemRowColumn(this.Items[0].StartDate, this.CurrentMode,
                    this._group.FirstDateOfGroup.Value, this._group.ShowLeadingDates, out colOffset, out rowOffset);
            }

            this.SetValue(FirstItemColumnOffsetPropertyKey, colOffset);
            this.SetValue(FirstItemRowOffsetPropertyKey, rowOffset);
        }
        #endregion //InitializeFirstItemOffset

        #region InitializeItems
        private void InitializeItems()
        {
			this.InitializeItems(false);
		}
		private void InitializeItems(bool force)
		{
			if (false == _isInitialized && force == false)
				return;

			
			
			

            if (null == this._group)
                return;

            #region Setup

            CalendarBase cal = CalendarUtilities.GetCalendar(this);

			// JJD 07/11/12 - TFS113838
			// If calandar is null then bail
			if (cal == null)
				return;

			SelectedDateCollection selectedDates = cal.SelectedDatesInternal as SelectedDateCollection;

            CalendarManager calendarManager = this.CalendarManager;
            CalendarZoomMode mode = this.CurrentMode;
            DateTime? firstDate = this._group.FirstDateOfGroup;
            DateTime? lastDate = this._group.LastDateOfGroup;
            DateTime[] dates;
			bool isSelectionActive = cal != null && cal.IsSelectionActive;

			// JJD 5/2/11 - TFS74024
			// Clear first and last date cache
			_firstDate = null;
			_lastDate = null;

            if (firstDate != null && lastDate != null)
            {
				DateTime minDate = cal != null ? cal.MinDateResolved : DateTime.MinValue;
				// JJD 1/11/12 - TFS96836 - Strip off time portion from max value
				//DateTime maxDate = cal != null ? cal.MaxDateResolved : DateTime.MaxValue;
                DateTime maxDate = cal != null ? cal.MaxDateResolved : DateTime.MaxValue.Date;

                dates = calendarManager.GetItemDates(firstDate.Value, mode, this._group.ShowLeadingDates, this._group.ShowTrailingDates, minDate, maxDate);
                this._groupRange = new DateRange(firstDate.Value, lastDate.Value);

				// JJD 5/2/11 - TFS74024
				// Cache first and last dates
				if (dates.Length > 0)
				{
					_firstDate = dates[0];
					_lastDate = dates[dates.Length - 1];
				}
			}
            else
            {
                dates = new DateTime[0];
                this._groupRange = null;
            }

            bool createDays = mode == CalendarZoomMode.Days;
            CalendarItem[] items = new CalendarItem[dates.Length];
            DateTime today = DateTime.Today;

            DateTime? activeDate;
            DayOfWeekFlags workingDays;
            bool isMinMode;
            Style itemStyle;
            if (null != cal)
            {
                activeDate = cal.ActiveDate;
                workingDays = cal.WorkDaysInternal;
                isMinMode = cal.IsMinCalendarMode;
				
				itemStyle = createDays ? cal.CalendarDayStyle : cal.CalendarItemStyle;

				if (itemStyle == null)
				{
					CalendarResourceProvider rp = cal.ResourceProviderResolved;
					if (rp != null)
						itemStyle = rp[createDays ? CalendarResourceId.CalendarDayStyle : CalendarResourceId.CalendarItemStyle] as Style;
				}
			}
            else
            {
                activeDate = null;
                workingDays = CalendarBase.DefaultWorkdays;
                isMinMode = true;
                itemStyle = null;
             }

			// JJD 3/29/11 - TFS69928 - Optimization
			// try to recycle the items. We now want to keep all items in the panel
			// so we don't have to re-create them later. Instead we will just collapse
			// the ones we aren't using now. This means we have to keep track of
			// both sets
			List<CalendarItem> oldItemsCanRecycle = new List<CalendarItem>();
			List<CalendarItem> oldItemsCanNotRecycle = new List<CalendarItem>();
			List<CalendarItem> recycledItems = new List<CalendarItem>();

			UIElementCollection panelChildren = this._itemPanel != null ? this._itemPanel.Children : null;

			if (panelChildren != null)
			{
				// JJD 3/29/11 - TFS69928 - Optimization
				// depending on the current mode sort items between those that can be recycled
				// and those that can't
				foreach (UIElement element in panelChildren)
				{
					CalendarItem item = element as CalendarItem;

					if (item != null)
					{
						bool canRecycle = item is CalendarDay;
						if (!createDays)
							canRecycle = !canRecycle;

						if (canRecycle)
							oldItemsCanRecycle.Add(item);
						else
						{
							oldItemsCanNotRecycle.Add(item);

							// JJD 3/29/11 - TFS69928 - Optimization
							// collapse any items we aren't using
							item.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
						}
					}
				}
			}

			CalendarItem[] reuseableItems = new CalendarItem[oldItemsCanRecycle.Count];
			oldItemsCanRecycle.CopyTo(reuseableItems, 0);
            
			int nextOldItem = 0;
            bool hasCalDays = reuseableItems.Length > 0 && reuseableItems[0] is CalendarDay;
            bool hasCalItems = false == hasCalDays && reuseableItems.Length > 0;
 
            // AS 10/23/08 TFS9443
            // Do not recycle in the sizing group or else the relative sizing logic
            // used below to calculate the largest item could get the wrong item.
            //
            if (this._group != null && this._group.IsGroupForSizing)
                hasCalDays = hasCalItems = false;

			CalendarZoomMode currentMode = CalendarItemGroup.GetCurrentMode(this);

            #endregion //Setup

            #region Create Items
            for (int i = 0; i < dates.Length; i++)
            {
                DateTime startDate = dates[i];
                DateTime endDate;

                bool isEnabled = true;
                CalendarItem item;
                bool isToday = false;
				bool canBeHighlighted = false;
				bool isRecycled = false;

                if (createDays)
                {
                    endDate = startDate;
                    isToday = startDate == today;

                    if (hasCalDays)
                    {
                        // recycle an existing item
                        item = items[i] = reuseableItems[nextOldItem];

                        // bump the index and make sure we still have days
                        nextOldItem++;
                        hasCalDays = nextOldItem < reuseableItems.Length;
					
						item.InternalSetPropValue(CalendarItem.PropFlags.IsInitializing, true);

                        item.Recycle(startDate, endDate);

						isRecycled = true;
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

					CalendarUtilities.SetBoolProperty(item, CalendarDay.IsWorkdayPropertyKey,
						CalendarManager.IsSet(workingDays, calendarManager.Calendar.GetDayOfWeek(startDate)), ((CalendarDay)item).IsWorkday, true);

					if (this._group == null || this._group.IsGroupForSizing == false)
						canBeHighlighted = true;
                }
                else
                {
                    endDate = calendarManager.GetItemEndDate(startDate, mode);

                    if (hasCalItems)
                    {
                        item = items[i] = reuseableItems[nextOldItem];

                        // bump the index and make sure we still have days
                        nextOldItem++;
                        hasCalItems = nextOldItem < reuseableItems.Length;

						item.InternalSetPropValue(CalendarItem.PropFlags.IsInitializing, true);
						
						item.Recycle(startDate, endDate);

						isRecycled = true;
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

				CalendarUtilities.SetBoolProperty(item, CalendarItem.IsTodayPropertyKey, isToday && isMinMode, item.IsToday, false);
				CalendarUtilities.SetBoolProperty(item, CalendarItem.ContainsTodayPropertyKey, isToday, item.ContainsToday, false);

				if ( currentMode != CalendarItemGroup.GetCurrentMode( item ))
					item.SetValue(CalendarItemGroup.CurrentModePropertyKey, currentMode);

                bool isActive = startDate <= activeDate && activeDate <= endDate;
                
				// JJD 3/0/11 - TFS68023
				// Moved stack variables out of if block below since we always need to set IsSelected and ContainsSelection
				// properties since we no longer clear them in the Recycle method called above
				bool isSelected = false;
				bool containsSelection = false;

                if (null != cal)
                {
                    // AS 10/3/08 TFS8607
                    // Added mode parameter since navigation mode should not consider
                    // disabled days of week or dates.
                    //
                    isEnabled = cal.CanActivate(startDate, endDate, mode);

                    if (isEnabled)
                    {
						// JJD 3/0/11 - TFS68023
						// Moved stack variables out of if block above since we always need to set IsSelected and ContainsSelection
						// properties since we no longer clear them in the Recycle method called above
						//bool isSelected = false;
						//bool containsSelection = ((SelectedDateCollection)(cal.SelectedDatesInternal)).ContainsSelection(startDate, endDate);
                        containsSelection = selectedDates.ContainsSelection(startDate, endDate);

                        // when in the minimum mode something is selected if its date is in the 
                        if (isMinMode)
                            isSelected = containsSelection;
                        else
                            isSelected = isActive;

                        // an item is also selected if its active and we're in navigation mode
						// JJD 3/0/11 - TFS68023
						// Moved SetBoolProperty calls below outside of if block
						//CalendarUtilities.SetBoolProperty(item, CalendarItem.IsSelectedPropertyKey, isSelected, item.IsSelected, false);
						//CalendarUtilities.SetBoolProperty(item, CalendarItem.ContainsSelectedDatesPropertyKey, containsSelection, item.ContainsSelectedDates, false);
                    }
                }

				// JJD 3/0/11 - TFS68023
				CalendarUtilities.SetBoolProperty(item, CalendarItem.IsSelectedPropertyKey, isSelected, item.IsSelected, false);
				CalendarUtilities.SetBoolProperty(item, CalendarItem.ContainsSelectedDatesPropertyKey, containsSelection, item.ContainsSelectedDates, false);

				// JJD 11/9/11 - TFS85551
				// Moved logic to CalendarItem.SetIsEnabled method
				//if (false == isEnabled)
				//    item.SetValue(IsEnabledProperty, KnownBoxes.FalseBox);
				//else
				//    if (isRecycled && item.IsEnabled == false)
				//        item.ClearValue(IsEnabledProperty);
				item.SetIsEnabled(isEnabled, isRecycled);
				
				CalendarUtilities.SetBoolProperty(item, CalendarItem.IsLeadingOrTrailingItemPropertyKey, startDate > lastDate || endDate < firstDate, item.IsLeadingOrTrailingItem, false);
				CalendarUtilities.SetBoolProperty(item, CalendarItem.IsActivePropertyKey, isActive, item.IsActive, false);
				CalendarUtilities.SetBoolProperty(item, CalendarItem.IsSelectionActivePropertyKey, isSelectionActive, item.IsSelectionActive, false);

				if (canBeHighlighted)
					((CalendarDay)item).InitializeIsHighlighted();

				// JJD 3/29/11 - TFS69928 - Optimization
				// Keep track of recycled items and make sure they are visible
				if (isRecycled)
				{
					recycledItems.Add(item);
					if (item.Visibility == Visibility.Collapsed)
						item.SetValue(VisibilityProperty, KnownBoxes.VisibilityVisibleBox);

					item.InternalSetPropValue(CalendarItem.PropFlags.IsInitializing, false);

				}
				else
					CalendarBase.SetCalendar(item, cal);

                
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

            }
            #endregion //Create Items

            #region Update Collection

			// JJD 3/29/11 - TFS69928 - Optimization
			// reverse the list of items we recycled so we can more efficiently remove them from the oldItemsCanRecycle list
			recycledItems.Reverse();
			foreach (CalendarItem item in recycledItems)
				oldItemsCanRecycle.Remove(item);

			// JJD 3/29/11 - TFS69928 - Optimization
			// any ones remaining we need to collapse
			foreach (CalendarItem item in oldItemsCanRecycle)
				item.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

			// JJD 4/5/11 - TFS71155 
			// any ones that we couldn't recycle we need to collapse
			foreach (CalendarItem item in oldItemsCanNotRecycle)
				item.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

			// JJD 3/29/11 - TFS69928 - Optimization
			if (items.Length != _itemsInternal.Count ||
				items.Length == 0 ||
				 items[0] != _itemsInternal[0])
			{
				_itemsInternal.BeginUpdate();

				this._itemsInternal.Clear();

				// add in all the ones - new or recycled
				if ( items.Length > 0 )
					this._itemsInternal.AddRange(items);

				_itemsInternal.EndUpdate();
			}

			//// remove any that were not recycled
			//for (int i = nextOldItem; i < reuseableItems.Length; i++)
			//{
			//    CalendarItem item = reuseableItems[i];
			//    this.RemoveLogicalChild(item);
			//}

            // add the new ones as logical children
            for (int i = nextOldItem; i < items.Length; i++)
            {
                CalendarItem item = items[i];
                //this.AddLogicalChild(item);

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

				// JJD 3/29/11 - TFS69928 - Optimization
				// MOved to helper method
				SetItemStyleHelper(itemStyle, item);
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

				//for (int i = 0; i < items.Length; i++)
				//{
				//    CalendarItem tempItem = items[i];

				//    if (tempItem != largestItem)
				//        this.RemoveLogicalChild(tempItem);
				//}
            }
            #endregion //Update Collection

			#region Update associated items panel children

			if (panelChildren != null)
			{
				bool arePanelChildrenTheSame = false;

				if (panelChildren.Count == this._items.Count)
				{
					arePanelChildrenTheSame = true;

					for (int i = 0, count = panelChildren.Count; i < count; i++)
					{
						if (panelChildren[i] != this._items[i])
						{
							arePanelChildrenTheSame = false;
							break;
						}
					}
				}

				if (!arePanelChildrenTheSame)
				{
					if (panelChildren.Count == 0)
					{
						foreach (CalendarItem item in this._items)
							panelChildren.Add(item);
					}
					else
					{
						HashSet<CalendarItem> recycledChildrenHash = new HashSet<CalendarItem>(recycledItems);

						
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


						// JJD 4/13/11 - TFS71155
						// we need to insert the items into the panel's children in their 
						// date order so allocate a stack variable for the insert at index
						// and the previous item
						int insertAtIndex = 0;
						CalendarItem previousItem = null;

						// add any new items
						for (int i = 0; i < _items.Count; i++)
						{
							CalendarItem item = _items[i] as CalendarItem;

							// if the item isn't in the recycledChildren hash set created above
							// then insert it here
							if ( item != null && !recycledChildrenHash.Contains(item))
							{
								// JJD 4/13/11 - TFS71155
								// Verify that the insert at index is correct by  checking
								// the previous items index in the index
								
								if (previousItem != null)
								{
									if (panelChildren[insertAtIndex - 1] != previousItem)
										insertAtIndex = Math.Max(insertAtIndex, panelChildren.IndexOf(previousItem) + 1);
								}
								
								panelChildren.Insert(insertAtIndex, item);
							}

							// JJD 4/13/11 - TFS71155
							// keep track of the previous item
							previousItem = item;
							insertAtIndex++;
						}
					}

					Debug.Assert(panelChildren.Count == _items.Count + oldItemsCanNotRecycle.Count + oldItemsCanRecycle.Count, "The counts should match");
				}
			}

			#endregion //Update associated items panel children	
    
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

			this.SetHeaderVisibility();
        }

        #endregion //InitializeItems

        #region InitializeWeekNumbers

        private void InitializeWeekNumbers()
        {
            CalendarManager calendarManager = this.CalendarManager;
            DateTime? firstDateOfGroup = this._group != null ? this._group.FirstDateOfGroup : null;
            bool createWeekNumbers = this.CurrentMode == CalendarZoomMode.Days && firstDateOfGroup != null && this.Items.Count > 0;

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
                calendarManager.GetItemRowColumn(date, CalendarZoomMode.Days, firstDateOfGroup.Value, this._group.ShowLeadingDates, out colOffset, out rowOffset);

                // move to the next date based on that offset so the next row we're evaluating
                // the first date of that row. otherwise we may get too many or too little
                // week numbers.
                int offset = 7 - colOffset;

				bool beginUpdateCalled = false;

				try
				{
					for (int i = 0; i < 6; i++)
					{
						int weekNumber = calendarManager.GetWeekNumberForDate(date);

						if (i > 0 && this._weekNumbersInternal[i - 1] == weekNumber)
							break;

						if (beginUpdateCalled == false)
						{
							beginUpdateCalled = true;
							this._weekNumbersInternal.BeginUpdate();
						}
						this._weekNumbersInternal.Add(weekNumber);

						date = calendarManager.AddItemOffset(date, offset, CalendarZoomMode.Days);

						// for all remaining weeks offset by a full week's dates
						offset = 7;

						// in case we don't have a full range because of the max date
						if (date > this._group.LastDate)
							break;
					}
				}
				finally
				{
					if (beginUpdateCalled)
						this._weekNumbersInternal.EndUpdate();
				}
            }
        }
        #endregion //InitializeWeekNumbers

        #region OnCurrentModeChanged
		internal void OnCurrentModeChanged(CalendarZoomMode newMode, CalendarZoomMode oldMode)
		{
            this.InitializeCalendarModeSettings(newMode);

            this.SetHeaderVisibility();
            // AS 10/1/08 TFS8497
            // The area needs to initialize its own items when its calendar mode has changed.
            this.InitializeItems();
		}
        #endregion //OnCurrentModeChanged
		
		#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.InitializeCalendarModeSettings(this.CurrentMode);
			this.InitializeItems();

			this.Loaded -= new RoutedEventHandler(OnLoaded);
		}
		#endregion //OnLoaded

        #region ReinitializeEnabledState
        private void ReinitializeEnabledState()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
                CalendarZoomMode mode = this.CurrentMode;

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

                    item.SetValue(ContentControl.IsEnabledProperty, value);







				}
            }
        }
        #endregion //ReinitializeEnabledState

		#region ReinitializeIsSelectionActive
		private void ReinitializeIsSelectionActive()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
                bool isSelectionActive = cal.IsSelectionActive;
                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

					if (isSelectionActive)
						item.SetValue(CalendarItem.IsSelectionActivePropertyKey, KnownBoxes.TrueBox);
					else
						item.ClearValue(CalendarItem.IsSelectionActivePropertyKey);
                }
            }
        }
		#endregion //ReinitializeIsSelectionActive

		#region ReinitializeSelectedState
		private void ReinitializeSelectedState()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
                bool isNavMode = cal.IsNavigationMode;
                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];
                    // contains selection is always the same
                    bool containsSelection =((SelectedDateCollection)(cal.SelectedDatesInternal)).ContainsSelection(item.StartDate, item.EndDate);
                    bool selected = isNavMode ? item.IsActive : containsSelection;

                    item.SetValue(CalendarItem.IsSelectedPropertyKey, selected ? KnownBoxes.TrueBox : KnownBoxes.FalseBox);
                    item.SetValue(CalendarItem.ContainsSelectedDatesPropertyKey, containsSelection ? KnownBoxes.TrueBox : KnownBoxes.FalseBox);
                }
            }
        }
        #endregion //ReinitializeSelectedState

		#region ReinitializeResources
		private void ReinitializeResources()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
				this.SetHeaderVisibility();

				// JJD 11/9/11 - TFS85695
				// Loop over all of the items panel child elements. This will
				// pick up any that aren't being used currently
				//for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
				//{
				//    CalendarItem item = this._itemsInternal[i];

				//    item.SetProviderBrushes();
				//}
				UIElementCollection panelChildren = this._itemPanel != null ? this._itemPanel.Children : null;

				if (panelChildren != null)
				{
					for (int i = 0, count = panelChildren.Count; i < count; i++)
					{
						CalendarItem item = panelChildren[i] as CalendarItem;

						if ( item != null )
							item.SetProviderBrushes();
					}
				}

            }
        }
		#endregion //ReinitializeResources

		#region ReinitializeStyle
		private void ReinitializeStyle()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
                bool isDays = this.CurrentMode == CalendarZoomMode.Days;
				CalendarResourceProvider rp = cal.ResourceProviderResolved;
				Style itemStyle = isDays ? cal.CalendarDayStyle : cal.CalendarItemStyle;

				if (itemStyle == null &&rp != null)
					itemStyle = rp[isDays ? CalendarResourceId.CalendarDayStyle : CalendarResourceId.CalendarItemStyle] as Style;

                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

					if (itemStyle != null)
						item.SetValue(FrameworkElement.StyleProperty, itemStyle);
					else
						item.ClearValue(FrameworkElement.StyleProperty);
                }
            }
        }
        #endregion //ReinitializeStyle

		// AS 3/23/10 TFS26461
		#region ReinitializeTodayState
		private void ReinitializeTodayState()
        {
            CalendarBase cal = CalendarUtilities.GetCalendar(this);

            if (null != cal)
            {
				DateTime today = DateTime.Today;
				bool isMinMode = cal.IsMinCalendarMode;

                for (int i = 0, count = this._itemsInternal.Count; i < count; i++)
                {
                    CalendarItem item = this._itemsInternal[i];

					bool isToday = item.StartDate <= today && today <= item.EndDate;

					if (isMinMode && isToday)
						item.SetValue(CalendarDay.IsTodayPropertyKey, KnownBoxes.TrueBox);
					else
						item.ClearValue(CalendarDay.IsTodayPropertyKey);

					if (isToday)
						item.SetValue(CalendarDay.ContainsTodayPropertyKey, KnownBoxes.TrueBox);
					else
						item.ClearValue(CalendarDay.ContainsTodayPropertyKey);
                }
            }
		}
		#endregion //ReinitializeTodayState

		#region SetHeaderVisibility

		private void SetHeaderVisibility()
		{
			double left, top;

			Visibility dayOfWeekHeaderVisibility;
			Visibility weekNumberVisibility;
			Visibility explicitWeekNumberVisibility;

			CalendarZoomMode mode = this.CurrentMode;

			if (mode != CalendarZoomMode.Days || this.Items.Count == 0)
			{
				dayOfWeekHeaderVisibility = Visibility.Collapsed;
				weekNumberVisibility = Visibility.Collapsed;
				explicitWeekNumberVisibility = Visibility.Collapsed;
			}
			else
			{

				CalendarBase cal = CalendarBase.GetCalendar(this);

				if (cal == null)
					return;

				dayOfWeekHeaderVisibility = cal.DayOfWeekHeaderVisibility;
				explicitWeekNumberVisibility = cal.WeekNumberVisibility;

				if ( cal.SupportsWeekSelectionMode )
					weekNumberVisibility = Visibility.Visible;
				else
					weekNumberVisibility = explicitWeekNumberVisibility;
			}

			this.DayOfWeekHeaderVisibility = dayOfWeekHeaderVisibility;
			this.WeekNumberVisibility = weekNumberVisibility;

			left = explicitWeekNumberVisibility == Visibility.Visible ? 1 : 0;
			top = dayOfWeekHeaderVisibility == Visibility.Visible ? 1 : 0;

			this.ComputedItemsBorderThickness = new Thickness(left, top, 0, 0);
		}

		#endregion //SetHeaderVisibility	

		// JJD 3/29/11 - TFS69928 - Optimization
		#region SetItemStyleHelper

		private static void SetItemStyleHelper(Style itemStyle, CalendarItem item)
		{
			// set or clear the style
			// AS 10/1/08 TFS8497
			// Setting the Style property to UnsetValue when it hasn't been set and isn't
			// initialized yet is causing it to not pick up a local style.
			//
			//item.SetValue(StyleProperty, tempStyle ?? itemStyle ?? DependencyProperty.UnsetValue);
			object style = itemStyle ?? DependencyProperty.UnsetValue;
			if (item.ReadLocalValue(StyleProperty) != style)
			{
				if (style != DependencyProperty.UnsetValue)
					item.SetValue(StyleProperty, style);
				else
					item.ClearValue(StyleProperty);
			}
		}

		#endregion //SetItemStyleHelper	
        
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

		#region GetWeek

		internal CalendarWeekNumber GetWeekNumber(int weekNumber)
		{
			return null != this._weekNumberPanel
				? this._weekNumberPanel.GetWeekNumber(weekNumber)
				: null;
		}
		#endregion //GetWeek

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
                if (group == PresentationUtilities.GetTemplatedParent(this))
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

		// JJD 3/29/11 - TFS69928 - Optimization
		#region LoadItemsLazily

		// JJD 3/29/11 - TFS69928 - Optimization
		// this routine is called off a timer and it pre-hydrates a month's worth 
		// of either CalendarItems or CalendarDays
		// so that we can avoid the overhead when the user scrolls or zooms
		internal bool LoadItemsLazily(bool loadDays)
		{
			if ( _itemPanel == null )
				return true;

			UIElementCollection panelChildren = _itemPanel.Children;

			// loop over the existing children.
			// If we find an instance of the CalendarDay or CalendarItem
			// then return false
			for (int i = 0, count = panelChildren.Count; i < count; i++)
			{
				CalendarItem item = panelChildren[i] as CalendarItem;

				if (item != null)
				{
					if (item is CalendarDay)
					{
						if (loadDays)
						{
							// set the visibility of all the days to collapsed
							if (item.Visibility == Visibility.Visible)
							{
								for (i = 0; i < count; i++)
								{
									CalendarDay day = panelChildren[i] as CalendarDay;

									if (day != null)
										day.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);
								}

								return true;
							}

							return false;
						}
					}
					else
					{
						if (loadDays == false)
							return false;
					}
				}
			}
			
			CalendarBase cal = CalendarUtilities.GetCalendar(this);

			if ( cal == null )
				return false;
			
			CalendarManager calendarManager = this.CalendarManager;
			CalendarZoomMode mode = this.CurrentMode;
			DateTime? firstDate = this._group.FirstDateOfGroup;
			DateTime? lastDate = this._group.LastDateOfGroup;
			DateTime[] dates;
			bool isSelectionActive = cal != null && cal.IsSelectionActive;

			// JJD 5/2/11 - TFS74024
			// clear first/last date cache
			_firstDate = null;
			_lastDate = null;
			if (firstDate != null && lastDate != null)
			{
				DateTime minDate = cal != null ? cal.MinDateResolved : DateTime.MinValue;
				// JJD 1/11/12 - TFS96836 - Strip off time portion from max value
				//DateTime maxDate = cal != null ? cal.MaxDateResolved : DateTime.MaxValue;
				DateTime maxDate = cal != null ? cal.MaxDateResolved : DateTime.MaxValue.Date;

				dates = calendarManager.GetItemDates(firstDate.Value, mode, this._group.ShowLeadingDates, this._group.ShowTrailingDates, minDate, maxDate);
				this._groupRange = new DateRange(firstDate.Value, lastDate.Value);

				// JJD 5/2/11 - TFS74024
				// Cache first and last dates
				if (dates.Length > 0)
				{
					_firstDate = dates[0];
					_lastDate = dates[dates.Length - 1];
				}
			}
			else
			{
				dates = new DateTime[0];
				this._groupRange = null;
			}

			// if there are no dates then return false since we didn't do any work
			if (dates.Length == 0)
				return false;

			Style itemStyle = null;
			if (null != cal)
			{
				itemStyle = loadDays ? cal.CalendarDayStyle : cal.CalendarItemStyle;

				if (itemStyle == null)
				{
					CalendarResourceProvider rp = cal.ResourceProviderResolved;
					if (rp != null)
						itemStyle = rp[loadDays ? CalendarResourceId.CalendarDayStyle : CalendarResourceId.CalendarItemStyle] as Style;
				}
			}

			if (loadDays)
			{

				for (int i = 0, count = dates.Length; i < count; i++)
				{
					DateTime startDate = dates[i];

					CalendarDay calday = new CalendarDay(startDate, this._group);

					panelChildren.Add(calday);

					// set the item style
					SetItemStyleHelper(itemStyle, calday);

					// force hydration of the element tree
					calday.ApplyTemplate();

					// set visibility to collapsed
					calday.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

					// JJD 4/13/11 - TFS71155
					// Set the calendar property on the item
					CalendarBase.SetCalendar(calday, cal);
				}
			}
			else
			{
				DateTime startDate = calendarManager.GetGroupStartDate(dates[0], CalendarZoomMode.Months);
				DateTime endDate = calendarManager.GetGroupEndDate(dates[0], CalendarZoomMode.Months);
				endDate = calendarManager.AddMonths(endDate, -11);
				for (int i = 0; i < 12; i++)
				{
					CalendarItem item = new CalendarItem(startDate, endDate, this._group);

					panelChildren.Add(item);

					startDate = calendarManager.AddMonths(startDate, 1);
					endDate = calendarManager.AddItemOffset(startDate, 1, CalendarZoomMode.Months);
					endDate = calendarManager.AddDays(endDate, -1);

					// set the item style
					SetItemStyleHelper(itemStyle, item);

					// force hydration of the element tree
					item.ApplyTemplate();

					// set visibility to collapsed
					item.SetValue(VisibilityProperty, KnownBoxes.VisibilityCollapsedBox);

					// JJD 4/13/11 - TFS71155
					// Set the calendar property on the item
					CalendarBase.SetCalendar(item, cal);
				}
			}

			return true;
		}

		#endregion //LoadItemsLazily

		#region VerifyItemsInitialized

		internal void VerifyItemsInitialized()
		{
			if (this._group != null)
			{
				if (this._items.Count == 0)
					this.InitializeItems(true);
			}
		}

		#endregion //VerifyItemsInitialized	
    
        #endregion //Internal

        #endregion //Methods

        #region ICalendarItemArea Members

        void ICalendarItemArea.InitializeItems()
        {
            this.InitializeItems();
        }

		void ICalendarItemArea.InitializeDaysOfWeek()
		{
			if (_dayOfWeekPanel != null)
				_dayOfWeekPanel.InitializeDaysOfWeek();
		}

        void ICalendarItemArea.InitializeWeekNumbers()
        {
            this.InitializeWeekNumbers();
			this.SetHeaderVisibility();
        }

        void ICalendarItemArea.InitializeFirstItemOffset()
        {
            this.InitializeFirstItemOffset();
        }

        void ICalendarItemArea.InitializeRowColCount()
        {
            this.InitializeCalendarModeSettings(this.CurrentMode);
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
                case CalendarItemChange.IsSelectionActive:
                    this.ReinitializeIsSelectionActive();
                    break;
				// AS 3/23/10 TFS26461
				case CalendarItemChange.Today:
					this.ReinitializeTodayState();
					break;
				case CalendarItemChange.Resources:
					this.InitializeItems();
					this.ReinitializeResources();
					break;
                default:
                    Debug.Assert(false, "Unexpected change type!");
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

        CalendarWeekNumber ICalendarItemArea.GetWeekNumber(int weekNumber)
        {
            return this.GetWeekNumber(weekNumber);
        }
        #endregion //ICalendarItemArea Members

		#region ICalendarElement Members

		void ICalendarElement.OnCalendarChanged(CalendarBase newValue, CalendarBase oldValue)
		{
			// walk over the child items to make sure they have the new value
			int count = this._itemsInternal.Count;

			for (int i = 0; i < count; i++)
			{
				CalendarItem item = this._itemsInternal[i];

				if (item != null)
				{
					CalendarBase.SetCalendar(item, newValue);
					item.InvalidateMeasure();
				}
			}

			if ( _dayOfWeekPanel != null )
				CalendarBase.SetCalendar(_dayOfWeekPanel, newValue);

			if ( _weekNumberPanel != null )
				CalendarBase.SetCalendar(_weekNumberPanel, newValue);
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