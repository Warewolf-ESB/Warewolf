using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows;
using Infragistics.Windows.Helpers;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;

using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers.Editors;
using Infragistics.Windows.Internal;
using System.Windows.Input;
using Infragistics.Controls;

namespace Infragistics.Windows.Editors
{
	/// <summary>
	/// Represents a group of calendar items within a <see cref="XamMonthCalendar"/> - e.g. a specific month of the year.
	/// </summary>
    /// <remarks>
    /// <p class="body">A CalendarItemGroup is used to represent a group of <see cref="CalendarItem"/> instances. Based 
    /// on the <see cref="CurrentCalendarMode"/>, the group will contain items that represent either days, months, years, 
    /// decades or centuries.</p>
    /// </remarks>
    //[System.ComponentModel.ToolboxItem(false)]

    // JJD 4/15/10 - NA2010 Vol 2 - Added support for VisualStateManager
    [TemplateVisualState(Name = VisualStateUtilities.StateNormal,              GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMouseOver,           GroupName = VisualStateUtilities.GroupCommon)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDisabled,            GroupName = VisualStateUtilities.GroupCommon)]

    [TemplateVisualState(Name = VisualStateUtilities.StateDay,                 GroupName = VisualStateUtilities.GroupCalendar)]
    [TemplateVisualState(Name = VisualStateUtilities.StateMonth,               GroupName = VisualStateUtilities.GroupCalendar)]
    [TemplateVisualState(Name = VisualStateUtilities.StateYear,                GroupName = VisualStateUtilities.GroupCalendar)]
    [TemplateVisualState(Name = VisualStateUtilities.StateDecade,              GroupName = VisualStateUtilities.GroupCalendar)]
    [TemplateVisualState(Name = VisualStateUtilities.StateCentury,             GroupName = VisualStateUtilities.GroupCalendar)]

    [TemplateVisualState(Name = VisualStateUtilities.StateFocused,             GroupName = VisualStateUtilities.GroupFocus)]
    [TemplateVisualState(Name = VisualStateUtilities.StateUnfocused,           GroupName = VisualStateUtilities.GroupFocus)]

	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class CalendarItemGroup : Control
	{
		#region Member Variables

		private XamMonthCalendar		_control;
        private int                     _suspendInitializeCount;
        private int                     _suspendAnimationCount;
        private int                     _preventAnimationCount;
        private bool                    _isSizingGroup;
        private bool                    _initializeNeeded = true;
        private ICalendarItemArea       _itemArea;
        private CalendarAnimation?      _pendingAnimation;


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        private bool _hasVisualStateGroups;


		#endregion //Member Variables

		#region Constructor

		static CalendarItemGroup()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(CalendarItemGroup), new FrameworkPropertyMetadata(typeof(CalendarItemGroup)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox));
            XamMonthCalendar.MonthCalendarPropertyKey.OverrideMetadata(typeof(CalendarItemGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnMonthCalendarChanged)));


            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            UIElement.IsEnabledProperty.OverrideMetadata(typeof(CalendarItemGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnVisualStatePropertyChanged)));

        }

		/// <summary>
		/// Initializes a new <see cref="CalendarItemGroup"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This constructor is only used for styling purposes. At runtime, the controls are automatically generated.</p>
		/// </remarks>
		public CalendarItemGroup()
			: this(CalendarManager.GetGroupStartDate(DateTime.Today, CalendarMode.Days, CultureInfo.CurrentCulture.Calendar, DayOfWeekFlags.None), false)
		{
		}

		internal CalendarItemGroup(DateTime? firstDate, bool isSizingGroup) 
		{
            this._isSizingGroup = isSizingGroup;
            this.SetValue(FirstDateOfGroupPropertyKey, firstDate);
        }

		#endregion //Constructor

		#region Properties

		#region Internal

        #region AllowAnimation
        internal bool AllowAnimation
        {
            get
            {
                return false == this.IsGroupForSizing &&
                    SystemParameters.ClientAreaAnimation;
            }
        }
        #endregion //AllowAnimation

        #region CalendarManager

		internal CalendarManager CalendarManager
		{
			get 
			{ 
				return this._control != null
					? this._control.CalendarManager
					: CalendarManager.CurrentCulture; 
			}
		}
		#endregion //CalendarManager

		#region CalendarModeInternal

		private static readonly DependencyProperty CalendarModeInternalProperty = DependencyProperty.Register("CalendarModeInternal",
			typeof(CalendarMode), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(CalendarMode.Days, new PropertyChangedCallback(OnCalendarModeInternalChanged)));

		private static void OnCalendarModeInternalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroup c = (CalendarItemGroup)d;

            CalendarAnimation animation;
            
            if (IsZoomFocus(c))
                animation = (CalendarMode)e.NewValue > (CalendarMode)e.OldValue ? CalendarAnimation.ZoomOut : CalendarAnimation.ZoomIn;
            else
                animation = CalendarAnimation.Fade;

            c.PrepareForAnimation(animation);
			c.SetValue(CurrentCalendarModePropertyKey, e.NewValue);
            c.OnCalendarModeChanged();
            c.PerformPendingAnimation(animation);
        }

		private CalendarMode CalendarModeInternal
		{
			get
			{
				return (CalendarMode)this.GetValue(CalendarItemGroup.CalendarModeInternalProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroup.CalendarModeInternalProperty, value);
			}
		}

		#endregion //CalendarModeInternal

		#region FirstDate

		internal DateTime FirstDate
		{
			get
			{
                IList<CalendarItem> items = this.Items;

				if (items.Count > 0)
					return items[0].StartDate;

                // AS 10/3/08 TFS8633
                //Debug.Fail("No days in the month yet!");
                Debug.Assert(XamMonthCalendar.GetMonthCalendar(this) != null && 
                    XamMonthCalendar.GetMonthCalendar(this).DisabledDaysOfWeek == CalendarManager.AllDays, "No days in the month yet!");

				return this.CalendarManager.Calendar.MinSupportedDateTime.Date;
			}
		} 
		#endregion //FirstDate

        #region IsGroupForSizing
        internal bool IsGroupForSizing
        {
            get { return this._isSizingGroup; }
        } 
        #endregion //IsGroupForSizing

		// AS 10/14/09 FR11859
		#region IsLeadingOrTrailingGroup
		internal bool IsLeadingOrTrailingGroup
		{
			get { return this.IsLeadingGroup || this.IsTrailingGroup; }
		} 
		#endregion //IsLeadingOrTrailingGroup

        #region ItemArea
        internal ICalendarItemArea ItemArea
        {
            get { return this._itemArea; }
        } 
        #endregion //ItemArea

        #region Items

        /// <summary>
        /// Returns a collection of <see cref="CalendarItem"/> instances that are displayed within the group.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Bindable(true)]
        [ReadOnly(true)]
        internal IList<CalendarItem> Items
        {
            get
            {
                return this._itemArea != null
                    ? this._itemArea.Items
                    : new CalendarItem[0];
            }
        }

        #endregion //Items

		#region LastDate

		internal DateTime LastDate
		{
			get
			{
                IList<CalendarItem> items = this.Items;

                if (items.Count > 0)
                    return items[items.Count - 1].EndDate;

                // AS 10/3/08 TFS8633
                //Debug.Fail("No days in the month yet!");
                Debug.Assert(XamMonthCalendar.GetMonthCalendar(this) != null &&
                    XamMonthCalendar.GetMonthCalendar(this).DisabledDaysOfWeek == CalendarManager.AllDays, "No days in the month yet!");

                return this.CalendarManager.Calendar.MaxSupportedDateTime.Date;
			}
		}
		#endregion //LastDate

        #region ReferenceDate

        private static readonly DependencyProperty ReferenceDateProperty = XamMonthCalendar.ReferenceDateProperty.AddOwner(typeof(CalendarItemGroup), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnReferenceDateChanged)));

        private DateTime? ReferenceDate
        {
            get { return (DateTime?)this.GetValue(ReferenceDateProperty); }
        }

        /// <summary>
        /// Handles changes to the ReferenceDate property.
        /// </summary>
        private static void OnReferenceDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroup group = (CalendarItemGroup)d;

            CalendarAnimation? animation = e.NewValue != null && e.OldValue != null ? CalendarAnimation.Scroll : (CalendarAnimation?)null;
            group.PrepareForAnimation(animation);
            group.InitializeFirstDateOfGroup();
            group.PerformPendingAnimation(animation);
        }

        #endregion //ReferenceDate

		#endregion //Internal

		#region Public

        #region CurrentCalendarMode

        internal static readonly DependencyPropertyKey CurrentCalendarModePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("CurrentCalendarMode", typeof(CalendarMode), typeof(CalendarItemGroup),
                new FrameworkPropertyMetadata(CalendarMode.Days,
                    FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// CurrentCalendarMode Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentCalendarModeProperty = CurrentCalendarModePropertyKey.DependencyProperty;

        /// <summary>
        /// Returns the current <see cref="CalendarMode"/> for the specified element within the <see cref="CalendarItemGroup"/>
        /// </summary>
        [AttachedPropertyBrowsableForType(typeof(CalendarItem))]
        [AttachedPropertyBrowsableForType(typeof(CalendarItemArea))]
        [AttachedPropertyBrowsableForType(typeof(CalendarItemGroup))]
        public static CalendarMode GetCurrentCalendarMode(DependencyObject d)
        {
            return (CalendarMode)d.GetValue(CurrentCalendarModeProperty);
        }

        // AS 10/1/08 TFS8497
        internal static void SetCurrentCalendarMode(DependencyObject d, CalendarMode mode)
        {
            d.SetValue(CurrentCalendarModePropertyKey, mode);
        }

        /// <summary>
        /// Returns the current calendar mode.
        /// </summary>
        /// <remarks>
        /// <p class="body">The CurrentCalendarMode is based on the <see cref="XamMonthCalendar.CurrentCalendarMode"/> 
        /// but is a readonly inherited property that descendants may use to determine their current mode. Depending on the 
        /// mode, the group will create <see cref="CalendarItem"/> instances that represents days, months, years, decades or 
        /// centuries.</p>
        /// </remarks>
        /// <seealso cref="CurrentCalendarModeProperty"/>
        /// <seealso cref="XamMonthCalendar.CurrentCalendarMode"/>
        /// <seealso cref="GetCurrentCalendarMode(DependencyObject)"/>
        //[Description("Returns the current calendar mode.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        [ReadOnly(true)]
        public CalendarMode CurrentCalendarMode
        {
            get { return GetCurrentCalendarMode(this); }
        }
        #endregion //CurrentCalendarMode

		#region FirstDateOfGroup

		internal static readonly DependencyPropertyKey FirstDateOfGroupPropertyKey = DependencyProperty.RegisterReadOnly(
			"FirstDateOfGroup", typeof(DateTime?), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnFirstDateOfGroupChanged)));

		/// <summary>
		/// Identifies the <see cref="FirstDateOfGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstDateOfGroupProperty = FirstDateOfGroupPropertyKey.DependencyProperty;

		private static void OnFirstDateOfGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroup group = (CalendarItemGroup)d;

            group.SuspendInitialize();

            try
            {
                group.InitializeLastDateOfGroup();
                group.InitializeItems();
            }
            finally
            {
                group.ResumeInitialize();
            }
		}

		/// <summary>
		/// Returns a <see cref="Nullable&lt;DateTime&gt;"/> that represents the first item of the group. Note, this will not be the first item displayed within the group if the group displays leading dates.
		/// </summary>
        /// <remarks>
        /// <p class="body">The FirstDateOfGroup and <see cref="LastDateOfGroup"/> identify the range of dates that make up the group. These dates 
        /// do not include any dates that would represent leading or trailing items. The values are calculated based on the 
        /// <see cref="XamMonthCalendar.ReferenceDate"/>, <see cref="CalendarItemGroup.ReferenceGroupOffset"/> and the <see cref="CurrentCalendarMode"/>.</p>
        /// <p class="note">The property is nullable because it is possible that the group will not be able to 
        /// display any dates. For example, if the <see cref="XamMonthCalendar.MinDate"/> and <see cref="XamMonthCalendar.MaxDate"/> 
        /// were such that you could only see a year, if the <see cref="XamMonthCalendar.CalendarDimensions"/> was 2,2 so that the control displayed 
        /// 4 groups, not all the groups would be able to display items when you start zooming out.</p>
        /// </remarks>
        /// <seealso cref="FirstDateOfGroupProperty"/>
        /// <seealso cref="LastDateOfGroup"/>
		public DateTime? FirstDateOfGroup
		{
			get { return (DateTime?)this.GetValue(FirstDateOfGroupProperty); }
		}
		#endregion //FirstDateOfGroup

		// AS 10/14/09 FR11859
		#region IsLeadingGroup

		/// <summary>
		/// Identifies the <see cref="IsLeadingGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsLeadingGroupProperty = DependencyProperty.Register("IsLeadingGroup",
			typeof(bool), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsLeadingOrTrailingGroupChanged)));

		private static void OnIsLeadingOrTrailingGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarItemGroup group = (CalendarItemGroup)d;

            if (null != group._control)
            {
                group._control.CoerceValue(XamMonthCalendar.ReferenceDateProperty);

                // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
                group.UpdateVisualStates();


            }
		}

		/// <summary>
		/// Returns or sets a boolean indicating if the group is considering a leading group.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default when a date is brought into view the XamMonthCalendar will 
		/// ensure that the <see cref="XamMonthCalendar.ReferenceDate"/>, and therefore the dates 
		/// displayed by the groups in the control, is such that the first group contains the 
		/// <see cref="XamMonthCalendar.MinDate"/> or a date after that and the last group contains 
		/// the <see cref="XamMonthCalendar.MaxDate"/> or a date before that - i.e. it attempts to 
		/// avoid having groups without items. However there are cases where months should be given 
		/// less priority and allowed to be empty such as when showing a small preview of the previous 
		/// and/or next month. When IsLeadingGroup or IsTrailingGroup is set to true, the XamMonthCalendar 
		/// will ignore these groups when adjusting/calculating the reference date and therefore these 
		/// groups may be empty.</p>
		/// <p class="note"><b>Note:</b> This property is always false for groups created by the 
		/// <see cref="CalendarItemGroupPanel"/>. It is intended to be used when creating a custom 
		/// XamMonthCalendar template.</p>
		/// </remarks>
		/// <seealso cref="IsLeadingGroupProperty"/>
		/// <seealso cref="IsTrailingGroup"/>
		//[Description("Returns or sets a boolean indicating if the group is considering a leading group.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsLeadingGroup
		{
			get
			{
				return (bool)this.GetValue(CalendarItemGroup.IsLeadingGroupProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroup.IsLeadingGroupProperty, value);
			}
		}

		#endregion //IsLeadingGroup

		// AS 10/14/09 FR11859
		#region IsTrailingGroup

		/// <summary>
		/// Identifies the <see cref="IsTrailingGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTrailingGroupProperty = DependencyProperty.Register("IsTrailingGroup",
			typeof(bool), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsLeadingOrTrailingGroupChanged)));

		/// <summary>
		/// Returns or sets a boolean indicating if the group is considering a leading group.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default when a date is brought into view the XamMonthCalendar will 
		/// ensure that the <see cref="XamMonthCalendar.ReferenceDate"/>, and therefore the dates 
		/// displayed by the groups in the control, is such that the first group contains the 
		/// <see cref="XamMonthCalendar.MinDate"/> or a date after that and the last group contains 
		/// the <see cref="XamMonthCalendar.MaxDate"/> or a date before that - i.e. it attempts to 
		/// avoid having groups without items. However there are cases where months should be given 
		/// less priority and allowed to be empty such as when showing a small preview of the previous 
		/// and/or next month. When IsLeadingGroup or IsTrailingGroup is set to true, the XamMonthCalendar 
		/// will ignore these groups when adjusting/calculating the reference date and therefore these 
		/// groups may be empty.</p>
		/// <p class="note"><b>Note:</b> This property is always false for groups created by the 
		/// <see cref="CalendarItemGroupPanel"/>. It is intended to be used when creating a custom 
		/// XamMonthCalendar template.</p>
		/// </remarks>
		/// <seealso cref="IsTrailingGroupProperty"/>
		/// <seealso cref="IsLeadingGroup"/>
		//[Description("Returns or sets a boolean indicating if the group is considering a leading group.")]
		//[Category("Behavior")]
		[Bindable(true)]
		public bool IsTrailingGroup
		{
			get
			{
				return (bool)this.GetValue(CalendarItemGroup.IsTrailingGroupProperty);
			}
			set
			{
				this.SetValue(CalendarItemGroup.IsTrailingGroupProperty, value);
			}
		}

		#endregion //IsTrailingGroup

		#region LastDateOfGroup

		private static readonly DependencyPropertyKey LastDateOfGroupPropertyKey = DependencyProperty.RegisterReadOnly(
			"LastDateOfGroup", typeof(DateTime?), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Identifies the <see cref="LastDateOfGroup"/> dependency property
		/// </summary>
		public static readonly DependencyProperty LastDateOfGroupProperty = LastDateOfGroupPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a <see cref="Nullable&lt;DateTime&gt;"/> that represents the last date of the group. Note, this will not be the last date displayed within the group if the group displays trailing days.
		/// </summary>
        /// <remarks>
        /// <p class="body">The <see cref="FirstDateOfGroup"/> and LastDateOfGroup identify the range of dates that make up the group. These dates 
        /// do not include any dates that would represent leading or trailing items. The values are calculated based on the 
        /// <see cref="XamMonthCalendar.ReferenceDate"/>, <see cref="CalendarItemGroup.ReferenceGroupOffset"/> and the <see cref="CurrentCalendarMode"/>.</p>
        /// <p class="note">The property is nullable because it is possible that the group will not be able to 
        /// display any dates. For example, if the <see cref="XamMonthCalendar.MinDate"/> and <see cref="XamMonthCalendar.MaxDate"/> 
        /// were such that you could only see a year, if the <see cref="XamMonthCalendar.CalendarDimensions"/> was 2,2 so that the control displayed 
        /// 4 groups, not all the groups would be able to display items when you start zooming out.</p>
        /// </remarks>
        /// <seealso cref="LastDateOfGroupProperty"/>
        /// <seealso cref="FirstDateOfGroup"/>
        public DateTime? LastDateOfGroup
		{
			get { return (DateTime?)this.GetValue(LastDateOfGroupProperty); }
		}
		#endregion //LastDateOfGroup

        #region ReferenceGroupOffset

        /// <summary>
        /// Identifies the <see cref="ReferenceGroupOffset"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ReferenceGroupOffsetProperty = DependencyProperty.Register("ReferenceGroupOffset",
            typeof(int), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(0, new PropertyChangedCallback(OnReferenceGroupOffsetChanged)));


        private static void OnReferenceGroupOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroup group = (CalendarItemGroup)d;

            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(group);

            if (null != cal && false == group.IsGroupForSizing)
                cal.OnGroupOffsetChanged(group, (int)e.OldValue, (int)e.NewValue);

            group.InitializeFirstDateOfGroup();
        }

        /// <summary>
        /// Returns or sets an integer that is used to calculate the dates displayed within the group based on the <see cref="XamMonthCalendar.ReferenceDate"/> of the containing <see cref="XamMonthCalendar"/>.
        /// </summary>
        /// <remarks>
        /// <p class="body">The ReferenceGroupOffset is used to calculate what range of dates should be displayed by the 
        /// group. It is used along with the <see cref="XamMonthCalendar.ReferenceDate"/> and <see cref="CurrentCalendarMode"/> 
        /// when calculating the <see cref="FirstDateOfGroup"/> and <see cref="LastDateOfGroup"/>.</p>
        /// <p class="body">By default, the template of the <see cref="XamMonthCalendar"/> contains a 
        /// <see cref="CalendarItemGroupPanel"/> which creates <see cref="CalendarItemGroup"/> instances 
        /// based on the <see cref="XamMonthCalendar.CalendarDimensions"/>. It initializes this property 
        /// such that the first displayed group has a ReferenceGroupOffset of 0 and therefore contains the 
        /// ReferenceDate. All subsequent groups are offset positively by 1 so that they show the next 
        /// group worth of dates. If you retemplate the XamMonthCalendar to directly contain CalendarItemGroups 
        /// then you must set this property. For example, you might have 3 groups whose ReferenceGroupOffsets 
        /// are -1, 0 and 1. When the ReferenceDate is August 1 2008 and the CurrentCalendarMode is days, the 
        /// group with -1 would display the dates for July 2008 and the group with 1 would display the dates 
        /// for September 2008.</p>
        /// </remarks>
        /// <seealso cref="ReferenceGroupOffsetProperty"/>
        /// <seealso cref="XamMonthCalendar.ReferenceDate"/>
        /// <seealso cref="CalendarItemGroupPanel"/>
        //[Description("Returns or sets an integer that is used to calculate the dates displayed within the group based on the <see ReferenceDate of the containing XamMonthCalendar.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public int ReferenceGroupOffset
        {
            get
            {
                return (int)this.GetValue(CalendarItemGroup.ReferenceGroupOffsetProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroup.ReferenceGroupOffsetProperty, value);
            }
        }

        #endregion //ReferenceGroupOffset

        #region ShowLeadingDates

        /// <summary>
        /// Identifies the <see cref="ShowLeadingDates"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowLeadingDatesProperty = DependencyProperty.Register("ShowLeadingDates",
            typeof(bool), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShowLeadingDatesChanged)));

        private static void OnShowLeadingDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CalendarItemGroup)d).InitializeItems();
        }

        /// <summary>
        /// Returns or sets a boolean indicating whether the group should display dates from the previous range of dates.
        /// </summary>
        /// <remarks>
        /// <p class="body">Leading dates are dates prior to the <see cref="FirstDateOfGroup"/> that are displayed within the 
        /// group. For example, when viewing a gregorian calendar with the default first day of week as Sunday, a group displaying 
        /// August 2008 would display items for July 28-31.</p>
        /// </remarks>
        /// <seealso cref="ShowLeadingDatesProperty"/>
        /// <seealso cref="ShowTrailingDates"/>
        /// <seealso cref="XamMonthCalendar.ShowLeadingAndTrailingDates"/>
        //[Description("Returns or sets a boolean indicating whether the group should display dates from the previous range of dates.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public bool ShowLeadingDates
        {
            get
            {
                return (bool)this.GetValue(CalendarItemGroup.ShowLeadingDatesProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroup.ShowLeadingDatesProperty, value);
            }
        }

        #endregion //ShowLeadingDates

        #region ScrollNextButtonVisibility

        /// <summary>
        /// Identifies the <see cref="ScrollNextButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScrollNextButtonVisibilityProperty = DependencyProperty.Register("ScrollNextButtonVisibility",
            typeof(Visibility), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Returns or sets the visibility of the button that scrolls its contents to display the next set of dates.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default the template of a <see cref="XamMonthCalendar"/> contains a 
        /// <see cref="CalendarItemGroupPanel"/>. This element dynamically generates CalendarItemGroup 
        /// instances and initializes their ScrollNextButtonVisibility and <see cref="ScrollPreviousButtonVisibility"/> 
        /// based on their location within the panel and the state of the <see cref="XamMonthCalendar.ScrollButtonVisibility"/>.</p>
        /// </remarks>
        /// <seealso cref="ScrollNextButtonVisibilityProperty"/>
        /// <seealso cref="ScrollPreviousButtonVisibility"/>
        /// <seealso cref="XamMonthCalendar.ScrollButtonVisibility"/>
        //[Description("Returns or sets the visibility of the button that scrolls its contents to display the next set of dates.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility ScrollNextButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CalendarItemGroup.ScrollNextButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroup.ScrollNextButtonVisibilityProperty, value);
            }
        }

        #endregion //ScrollNextButtonVisibility

        #region ScrollPreviousButtonVisibility

        /// <summary>
        /// Identifies the <see cref="ScrollPreviousButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ScrollPreviousButtonVisibilityProperty = DependencyProperty.Register("ScrollPreviousButtonVisibility",
            typeof(Visibility), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.VisibilityVisibleBox));

        /// <summary>
        /// Returns or sets the visibility of the button that scrolls its contents to display the previous set of dates.
        /// </summary>
        /// <remarks>
        /// <p class="body">By default the template of a <see cref="XamMonthCalendar"/> contains a 
        /// <see cref="CalendarItemGroupPanel"/>. This element dynamically generates CalendarItemGroup 
        /// instances and initializes their ScrollPreviousButtonVisibility and <see cref="ScrollNextButtonVisibility"/> 
        /// based on their location within the panel and the state of the <see cref="XamMonthCalendar.ScrollButtonVisibility"/>.</p>
        /// </remarks>
        /// <seealso cref="ScrollPreviousButtonVisibilityProperty"/>
        /// <seealso cref="ScrollNextButtonVisibility"/>
        /// <seealso cref="XamMonthCalendar.ScrollButtonVisibility"/>
        //[Description("Returns or sets the visibility of the button that scrolls its contents to display the previous set of dates.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public Visibility ScrollPreviousButtonVisibility
        {
            get
            {
                return (Visibility)this.GetValue(CalendarItemGroup.ScrollPreviousButtonVisibilityProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroup.ScrollPreviousButtonVisibilityProperty, value);
            }
        }

        #endregion //ScrollPreviousButtonVisibility

        #region ShowTrailingDates

        /// <summary>
        /// Identifies the <see cref="ShowTrailingDates"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ShowTrailingDatesProperty = DependencyProperty.Register("ShowTrailingDates",
            typeof(bool), typeof(CalendarItemGroup), new FrameworkPropertyMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShowTrailingDatesChanged)));

        private static void OnShowTrailingDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((CalendarItemGroup)d).InitializeItems();
        }

        /// <summary>
        /// Returns or sets a boolean indicating whether the group should display dates from the next range of dates.
        /// </summary>
        /// <p class="body">Trailing dates are dates after to the <see cref="LastDateOfGroup"/> that are displayed within the 
        /// group. For example, when viewing a gregorian calendar with the default first day of week as Sunday, a group displaying 
        /// August 2008 would display items for September 1-6.</p>
        /// <seealso cref="ShowTrailingDatesProperty"/>
        /// <seealso cref="ShowLeadingDates"/>
        /// <seealso cref="XamMonthCalendar.ShowLeadingAndTrailingDates"/>
        //[Description("Returns or sets a boolean indicating whether the group should display dates from the next range of dates.")]
        //[Category("MonthCalendar Properties")] // Behavior
        [Bindable(true)]
        public bool ShowTrailingDates
        {
            get
            {
                return (bool)this.GetValue(CalendarItemGroup.ShowTrailingDatesProperty);
            }
            set
            {
                this.SetValue(CalendarItemGroup.ShowTrailingDatesProperty, value);
            }
        }

        #endregion //ShowTrailingDates

		#endregion //Public

        #region Private

        #region IsInitializeSuspended
        private bool IsInitializeSuspended
        {
            get { return this._suspendInitializeCount > 0; }
        }
        #endregion //IsInitializeSuspended

        #endregion //Private

        #endregion //Properties

        #region Methods

        #region Internal

        #region CompareDate



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal int CompareDate(DateTime date)
		{
			// make sure to ignore time
			date = date.Date;

			if (date < this.FirstDateOfGroup)
				return -1;
			else if (date > this.LastDateOfGroup)
				return 1;
			else
				return 0;
		}
		#endregion //CompareDate

		#region GetItem

		internal CalendarItem GetItem(DateTime date)
		{
            return null != this._itemArea
                ? this._itemArea.GetItem(date)
                : null;
		}
		#endregion //GetItem

        #region IsZoomFocus
        private static bool IsZoomFocus(CalendarItemGroup c)
        {
            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(c);

            return null != cal && cal.IsZoomFocusGroup(c);
        }
        #endregion //IsZoomFocus

        #region OnCalendarChange
        internal void OnCalendarChange(MonthCalendarChange change)
        {
            // AS 10/3/08 TFS8607
            // For this change we want to allow animations.
            //
            bool suppressInitialize = change != MonthCalendarChange.CurrentModeChanged;

            try
            {
                if (suppressInitialize)
                    this.SuspendInitialize();

                switch (change)
                {
                    case MonthCalendarChange.WeekRuleChanged:
                        if (null != this._itemArea)
                            this._itemArea.InitializeWeekNumbers();
                        break;
                    case MonthCalendarChange.FirstDayOfWeekChanged:
                        // AS 1/12/11 TFS59499/TFS62119
                        // There was a bug in the ObservableCollectionExtended where we never reset 
                        // the _hasChanged flag back to false. As a result when its EndUpdate was 
                        // called we would always be sending out a reset notification which was 
                        // causing this class to set the _initializeNeeded flag. When that bug was 
                        // fixed this bug was uncovered - basically whenever the first day of week 
                        // changes we need to reinitialize more than the first item offset. We need 
                        // to reinitialize the items since they would move and we might even have 
                        // less/more weeks.
                        //
                        //if (this.ShowLeadingDates || this.ShowTrailingDates)
                        //    this.InitializeItems();
                        //else
                        //{
                        //    if (null != this._itemArea)
                        //        this._itemArea.InitializeFirstItemOffset();
                        //}
                        this.InitializeItems();
                        break;
                    case MonthCalendarChange.DaysOfWeekChanged:
                        this.InitializeItems();
                        if (null != this._itemArea)
                            this._itemArea.InitializeRowColCount();
                        break;
                    case MonthCalendarChange.MinDateChanged:
                    case MonthCalendarChange.MaxDateChanged:
                        this.InitializeFirstDateOfGroup();
                        this.InitializeLastDateOfGroup();
                        this.InitializeItems();
                        break;
                    case MonthCalendarChange.DisabledDatesChanged:
                        if (null != this._itemArea)
                            this._itemArea.ReinitializeItems(CalendarItemChange.Enabled);
                        break;
                    case MonthCalendarChange.LanguageChanged:
                        this.InitializeFirstDateOfGroup();
                        this.InitializeLastDateOfGroup();
                        this.InitializeItems();
                        break;
                    case MonthCalendarChange.ItemStyleChange:
                        if (null != this._itemArea)
                            this._itemArea.ReinitializeItems(CalendarItemChange.Style);
                        break;
                    case MonthCalendarChange.SelectionChanged:
                        if (null != this._itemArea)
                            this._itemArea.ReinitializeItems(CalendarItemChange.Selection);
                        break;
                    case MonthCalendarChange.WorkdaysChanged:
                        this.InitializeItems();
                        break;
                    case MonthCalendarChange.CurrentModeChanged:
                        // AS 10/3/08 TFS8607
                        // See the XamMonthCalendar.OnCurrentCalendarModeChanged for details
                        // on why this is needed. Note, we're doing this outside the suspend/
                        // resume initialize because we want animations to occur.
                        //
                        if (this.IsGroupForSizing == false)
                        {
                            XamMonthCalendar cal = XamMonthCalendar.GetMonthCalendar(this);
                            Debug.Assert(null != cal);

                            if (null != cal)
                                this.CalendarModeInternal = cal.CurrentCalendarMode;
                        }
                        break;
					case MonthCalendarChange.TodayChanged:
						// AS 3/23/10 TFS26461
						if (null != this._itemArea)
							this._itemArea.ReinitializeItems(CalendarItemChange.Today);
						break;
                    default:
                        Debug.Fail("Unrecognized change:" + change.ToString());
                        break;
                }
            }
            finally
            {
                if (suppressInitialize)
                    this.ResumeInitialize();
            }
        }

        #endregion //OnCalendarChange

        #region RegisterItemArea
        internal void RegisterItemArea(ICalendarItemArea itemArea)
        {
            Debug.Assert(null == this._itemArea || itemArea == this._itemArea);
            this._itemArea = itemArea;

            // AS 10/1/08 TFS8497
            // Instead of making the area call its own initializeitems, we'll have the 
            // group do it - esp since the group's initialization may be suspended.
            //
            if (null != this._itemArea)
                this.InitializeItems();
        }
        #endregion //RegisterItemArea

        #region ResumeAnimations
        private void ResumeAnimations(bool preventedAnimations)
        {
            if (preventedAnimations)
            {
                Debug.Assert(this._preventAnimationCount > 0);
                this._preventAnimationCount--;
            }

            this._suspendAnimationCount--;

            if (this._suspendAnimationCount == 0)
                this.PerformPendingAnimation(this._pendingAnimation);
        } 
        #endregion //ResumeAnimations

        #region ResumeInitialize
        internal void ResumeInitialize()
        {
            this.ResumeInitializeImpl();
            this.ResumeAnimations(true);
        }

        private void ResumeInitializeImpl()
        {
            Debug.Assert(this._suspendInitializeCount > 0);

            this._suspendInitializeCount--;

            if (this._suspendInitializeCount == 0 && this._initializeNeeded)
                this.InitializeItems();
        }
        #endregion //ResumeInitialize

        #region SuspendAnimations
        private void SuspendAnimations(bool preventAnimations)
        {
            this._suspendAnimationCount++;

            if (preventAnimations)
                this._preventAnimationCount++;
        } 
        #endregion //SuspendAnimations

        #region SuspendInitialize
        internal void SuspendInitialize()
        {
            this.SuspendInitializeImpl();
            this.SuspendAnimations(true);
        }

        private void SuspendInitializeImpl()
        {
            this._suspendInitializeCount++;
        }
        #endregion //SuspendInitialize

        #region UnregisterItemArea
        internal void UnregisterItemArea(ICalendarItemArea itemArea)
        {
            if (itemArea == this._itemArea)
                this._itemArea = null;
        }
        #endregion //UnregisterItemArea

        #region VisualState... Methods


        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the VisualStates of the editor
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected virtual void SetVisualState(bool useTransitions)
        {

            // Set Common states
            if (this.IsEnabled == false)
                VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);
            else if (this.IsMouseOver)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);

            bool isFocused = this.IsKeyboardFocusWithin;

            // Set Focus states
            if (isFocused)
                VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateFocused, VisualStateUtilities.StateUnfocused);
            else
                VisualStateManager.GoToState(this, VisualStateUtilities.StateUnfocused, useTransitions);

            // Set Calendar states
            GoToCalendarState(this, useTransitions);
        }

        internal static void GoToCalendarState(Control control, bool useTransitions)
        {
            string calendarState;

            switch (CalendarItemGroup.GetCurrentCalendarMode(control))
            {
                case CalendarMode.Centuries:
                    calendarState = VisualStateUtilities.StateCentury;
                    break;
                case CalendarMode.Decades:
                    calendarState = VisualStateUtilities.StateDecade;
                    break;
                case CalendarMode.Years:
                    calendarState = VisualStateUtilities.StateYear;
                    break;
                case CalendarMode.Months:
                    calendarState = VisualStateUtilities.StateMonth;
                    break;
                default:
                case CalendarMode.Days:
                    calendarState = VisualStateUtilities.StateDay;
                    break;
            }

            VisualStateManager.GoToState(control, calendarState, useTransitions);
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        internal static void OnVisualStatePropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroup group = target as CalendarItemGroup;

            if (group != null)
                group.UpdateVisualStates();
        }

        // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        protected void UpdateVisualStates()
        {
            this.UpdateVisualStates(true);
        }

        /// <summary>
        /// Called to set the visual states of the control
        /// </summary>
        /// <param name="useTransitions">Determines whether transitions should be used.</param>
        protected void UpdateVisualStates(bool useTransitions)
        {
            if (false == this._hasVisualStateGroups)
                return;

            if (!this.IsLoaded)
                useTransitions = false;

            this.SetVisualState(useTransitions);
        }



        #endregion //VisualState... Methods	

        #endregion //Internal

		#region Private

        #region CoerceMinMaxDate
        private DateTime? CoerceMinMaxDate(DateTime? date)
        {
            if (null != date && null != this._control)
                date = this._control.CoerceMinMaxDate(date.Value);

            return date;
        } 
        #endregion //CoerceMinMaxDate

        #region InitializeControl
        private void InitializeControl(XamMonthCalendar control)
        {
            if (control == this._control)
                return;

            this._control = control;

            if (null != control)
            {
                this.SuspendInitialize();

                try
                {
                    CalendarMode cm = this.CalendarModeInternal;

                    if (this.IsGroupForSizing)
                        this.SetBinding(CalendarModeInternalProperty, Utilities.CreateBindingObject(XamMonthCalendar.MinCalendarModeProperty, System.Windows.Data.BindingMode.OneWay, control));
                    else
                        this.SetBinding(CalendarModeInternalProperty, Utilities.CreateBindingObject(XamMonthCalendar.CurrentCalendarModeProperty, System.Windows.Data.BindingMode.OneWay, control));

                    if (this.IsGroupForSizing)
                        this.SetBinding(ReferenceDateProperty, Utilities.CreateBindingObject(XamMonthCalendar.MeasureDateProperty, System.Windows.Data.BindingMode.OneWay, control));
                    else
                        this.SetBinding(ReferenceDateProperty, Utilities.CreateBindingObject(XamMonthCalendar.ReferenceDateProperty, System.Windows.Data.BindingMode.OneWay, control));

                    // if the mode isn't changing, we still need to fix up the first/last dates and
                    // item offsets
                    if (cm == this.CalendarModeInternal)
                        this.OnCalendarModeChanged();
                }
                finally
                {
                    this.ResumeInitialize();
                }
            }
            else
            {
                
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

            }
        }
        #endregion //InitializeControl

        #region InitializeItems






		private void InitializeItems()
		{
            if (false == this.IsInitialized)
                return;
                
            if (this.IsInitializeSuspended)
            {
                this._initializeNeeded = true;
                return;
            }

            _initializeNeeded = false;

            if (null != this._itemArea)
                this._itemArea.InitializeItems();
        }
		#endregion //InitializeItem

        #region InitializeFirstDateOfGroup
        private void InitializeFirstDateOfGroup()
        {
            DateTime referenceDate = this.ReferenceDate ?? this.CalendarManager.CoerceMinMaxDate(DateTime.Today);
            CalendarMode mode = this.CalendarModeInternal;
            referenceDate = this.CalendarManager.GetGroupStartDate(referenceDate, mode);
            DateTime? startDate = this.CalendarManager.TryAddGroupOffset(referenceDate, this.ReferenceGroupOffset, mode, true);

            if (null != startDate && null != this._control)
            {
                if (false == this.IsGroupForSizing)
                {
                    // do not show any dates if the group's dates would be outside the valid range
                    if (startDate > this._control.MaxDate
                        || this.CalendarManager.GetGroupEndDate(startDate.Value, mode) < this._control.MinDate)
                    {
                        Debug.Assert(this.IsGroupForSizing == false);
                        startDate = null;
                    }
                }

                // make sure its within the min/max range
                startDate = this.CoerceMinMaxDate(startDate);
            }

            this.SetValue(FirstDateOfGroupPropertyKey, startDate);
        }
        #endregion //InitializeFirstDateOfGroup

        #region InitializeLastDateOfGroup
        private void InitializeLastDateOfGroup()
        {
            DateTime? firstDate = this.FirstDateOfGroup;
            DateTime? lastDate = null != firstDate ? (DateTime?)this.CalendarManager.GetGroupEndDate(firstDate.Value, this.CalendarModeInternal) : null;

            lastDate = CoerceMinMaxDate(lastDate);

            DateTime? oldDate = this.LastDateOfGroup;

            this.SetValue(LastDateOfGroupPropertyKey, lastDate);

            if (oldDate != lastDate)
                this.InitializeItems();
        } 
        #endregion //InitializeLastDateOfGroup

        #region OnCalendarModeChanged
        private void OnCalendarModeChanged()
        {
            this.SuspendInitialize();

            try
            {
                this.InitializeFirstDateOfGroup();
                this.InitializeLastDateOfGroup();
                if (null != this._itemArea)
                    this._itemArea.InitializeRowColCount();
                this.InitializeItems();
            }
            finally
            {
                this.ResumeInitialize();
            }
        }
        #endregion //OnCalendarModeChanged

        #region OnMonthCalendarChanged
        private static void OnMonthCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CalendarItemGroup group = (CalendarItemGroup)d;

            group.InitializeControl(e.NewValue as XamMonthCalendar);

            if (null != e.OldValue)
                ((XamMonthCalendar)e.OldValue).UnregisterGroup(group);

            if (null != e.NewValue)
                ((XamMonthCalendar)e.NewValue).RegisterGroup(group);
        } 
        #endregion //OnMonthCalendarChanged

        #region PerformPendingAnimation
        private void PerformPendingAnimation(CalendarAnimation? action)
        {
            if (null != action
                && null != this._itemArea
                && action == this._pendingAnimation
                && this._suspendAnimationCount == 0)
            {
                this._pendingAnimation = null;
                this._itemArea.PerformAnimationAction(action.Value);
            }
        }
        #endregion //PerformPendingAnimation

        #region PrepareForAnimation
        private void PrepareForAnimation(CalendarAnimation? action)
        {
            if (null != this._itemArea && this._preventAnimationCount == 0 && this.AllowAnimation)
            {
                // only take action if we don't have one pending
                if (this._pendingAnimation == null && action != null)
                {
                    this._pendingAnimation = action;
                    this._itemArea.PrepareForAnimationAction(action.Value);
                }
            }
        }
        #endregion //PrepareForAnimation

        #endregion //Private

		#endregion //Methods

		#region Base class overrides

        #region OnApplyTemplate

        /// <summary>
        /// Called when the template is applied.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// OnApplyTemplate is a .NET framework method exposed by the FrameworkElement. This class overrides
        /// it to get the focus site from the control template whenever template gets applied to the control.
        /// </p>
        /// </remarks>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this._hasVisualStateGroups = VisualStateUtilities.GetHasVisualStateGroups(this);

            this.UpdateVisualStates(false);

        }

        #endregion //OnApplyTemplate

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an <see cref="AutomationPeer"/> that represents the group
        /// </summary>
        /// <returns>A <see cref="CalendarItemGroupAutomationPeer"/> instance</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new CalendarItemGroupAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region OnInitialized

        /// <summary>
		/// Overriden. Raises the <see cref="FrameworkElement.Initialized"/> event. This method is invoked when the <see cref="FrameworkElement.IsInitialized"/> is set to true.
		/// </summary>
		/// <param name="e">Event arguments</param>
		protected override void OnInitialized(EventArgs e)
		{
            this.InitializeItems();

            base.OnInitialized(e);
		}
		#endregion //OnInitialized

        #region OnIsKeyboardFocusedChanged

        /// <summary>
        /// Called when the IsKeyboardFocusWithin property changes
        /// </summary>
        /// <param name="e"></param>
        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnIsKeyboardFocusWithinChanged(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }

        #endregion //OnIsKeyboardFocusedChanged	
    
        #region OnMouseEnter
        /// <summary>
        /// Invoked when the mouse is moved within the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
        #endregion //OnMouseEnter

        #region OnMouseLeave
        /// <summary>
        /// Invoked when the mouse is moved outside the bounds of the element.
        /// </summary>
        /// <param name="e">Provides information about the mouse position.</param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            // JJD 4/08/10 - NA2010 Vol 2 - Added support for VisualStateManager
            this.UpdateVisualStates();

        }
        #endregion //OnMouseLeave

        #region ToString
        /// <summary>
        /// Overriden. Returns the date range that the item represents
        /// </summary>
        /// <returns>A string containing the <see cref="FirstDateOfGroup"/> and <see cref="LastDateOfGroup"/>.</returns>
        public override string ToString()
        {
            if (this.FirstDateOfGroup != null && this.LastDateOfGroup != null)
                return string.Format(this.CalendarManager.DateTimeFormat, "CalendarItemGroup {0:d}-{1:d}", this.FirstDateOfGroup, this.LastDateOfGroup);

            return base.ToString();
        } 
        #endregion //ToString

		#endregion //Base class overrides

        #region GroupInitializationHelper
        internal class GroupInitializationHelper : IDisposable
        {
            private IList<CalendarItemGroup> _groups;
            private bool _preventedAnimations;

            internal GroupInitializationHelper(IList<CalendarItemGroup> groups)
                : this(groups, true)
            {
            }

            internal GroupInitializationHelper(IList<CalendarItemGroup> groups, bool preventAnimations)
            {
                this._groups = groups;
                this._preventedAnimations = preventAnimations;

                foreach (CalendarItemGroup group in groups)
                {
                    group.SuspendAnimations(preventAnimations);
                    group.SuspendInitializeImpl();
                }
            }

            #region IDisposable Members

            public void Dispose()
            {
                foreach (CalendarItemGroup group in this._groups)
                {
                    group.ResumeInitializeImpl();
                }

                // resume initialize on all items first before 
                // resuming animations to ensure that the items are
                // available for the new group
                foreach (CalendarItemGroup group in this._groups)
                {
                    group.ResumeAnimations(this._preventedAnimations);
                }
            }

            #endregion
        }
        #endregion //GroupInitializationHelper
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