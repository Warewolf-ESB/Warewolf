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


using Infragistics.Windows.Licensing;


using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Editors.Primitives;
using Infragistics.Controls.Primitives;

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
    /// quickly navigate the available dates and then zoom back into to change the selection. The <see cref="CalendarBase.CurrentMode"/> 
	/// is used to control the current mode. The <see cref="MinCalendarMode"/> may be used to control the lowest level 
    /// of dates that the end user may navigate into.
    /// </p>
    /// <p class="body">The default template for CalendarBase uses a <see cref="CalendarItemGroupPanel"/> 
    /// that will generate <see cref="CalendarItemGroup"/> instances based on the row/column count specified 
	/// via the <see cref="CalendarDimensions"/>. In addition, when the <see cref="CalendarBase.AutoAdjustDimensions"/> 
    /// property is set to true, which is the default value, the panel will automatically generate additional groups 
	/// to fill the available space up to its <see cref="CalendarItemGroupPanel.MaxGroups"/>. The <see cref="CalendarBase.ReferenceDate"/> 
    /// is used by the groups to determine which dates should be shown.
    /// </p>
    /// <p class="body">The control supports multiple selection modes which can be controlled via its <see cref="SelectionType"/>. 
	/// When using a multiple selection mode such as <b>Extended</b> or <b>Range</b>, the <see cref="CalendarBase.SelectedDates"/> property 
	/// may be used to access/change the selection up to the <see cref="MaxSelectedDates"/>. The control also exposes a <see cref="CalendarBase.SelectedDate"/> property which is 
    /// primarily used when in a single select mode. When in a multiselect mode, this property will return the first selected date.
    /// </p>
    /// <p class="body">The control exposes a number of properties that may be used to restrict the selectable dates. The 
    /// <see cref="MinDate"/> and <see cref="MaxDate"/> are used to control the range within which the user may navigate. You can 
	/// then disable dates within that range using either the <see cref="DisabledDaysOfWeek"/> and <see cref="DisabledDates"/>.
    /// </p>
    /// </remarks>

    
    

    [StyleTypedProperty(Property = "CalendarItemStyle", StyleTargetType = typeof(CalendarItem))]
    [StyleTypedProperty(Property = "CalendarDayStyle", StyleTargetType = typeof(CalendarDay))]
    public class XamCalendar : CalendarBase
	{
		#region Member Variables


		private UltraLicense                                    _license;


        #endregion //Member Variables

		#region Constructor

		static XamCalendar()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamCalendar), new FrameworkPropertyMetadata(typeof(XamCalendar)));


		}

		/// <summary>
		/// Initializes a new <see cref="XamCalendar"/>
		/// </summary>
		public XamCalendar()
		{


#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

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
				this._license = LicenseManager.Validate(typeof(XamCalendar), this) as UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }

		}

		#endregion //Constructor

		#region Base class overrides

		internal override SelectionType CurrentSelectionType
		{
			get
			{
				switch (this.SelectionMode)
				{
					default:
					case CalendarDateSelectionMode.Extended:
						return SelectionType.Extended;

					case CalendarDateSelectionMode.None:
						return SelectionType.None;

					case CalendarDateSelectionMode.SingleDate:
						return SelectionType.Single;

					case CalendarDateSelectionMode.Range:
						return SelectionType.Range;
				}
			}
		}

		#endregion //Base class overrides	

		#region Properties

		#region Public Properties

		#region DisabledDates
		/// <summary>
		/// Returns a modifiable collection of <see cref="DateRange"/> instances that indicates the items that should be considered disabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">The DisabledDates is a collection of <see cref="DateRange"/> instances that represent ranges of dates that should 
		/// not be selectable by the end user. Dates may also be disabled using the <see cref="XamCalendar.DisabledDaysOfWeek"/> property.</p>
		/// </remarks>
		/// <seealso cref="XamCalendar.DisabledDaysOfWeek"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]

		[Bindable(true)]
		public CalendarDateRangeCollection DisabledDates
		{
			get { return base.DisabledDatesInternal; }
		}
		#endregion //DisabledDates

		#region DisabledDaysOfWeek

		/// <summary>
		/// Identifies the <see cref="DisabledDaysOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisabledDaysOfWeekProperty = DependencyPropertyUtilities.Register("DisabledDaysOfWeek",
			typeof(DayOfWeekFlags), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(DayOfWeekFlags.None, new PropertyChangedCallback(OnDisabledDaysOfWeekChanged))
			);

		private static void OnDisabledDaysOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = (XamCalendar)d;

			cal.DisabledDaysOfWeekInternal = (DayOfWeekFlags)e.NewValue;
		}

		/// <summary>
		/// Returns or sets a flagged enumeration indicating which days of the week are always disabled.
		/// </summary>
		/// <remarks>
		/// <p class="body">The DisabledDaysOfWeek is a flagged enumeration that can be used to prevent selection of 
		/// one or more days of the week. The <see cref="DisabledDates"/> may be used to disable specific dates (or 
		/// ranges of dates).</p>
		/// <p class="body">When the <see cref="DisabledDaysOfWeekVisibility"/> property is set to true 
		/// the disabled days of the week will be hidden from the display.</p>
		/// </remarks>
		/// <seealso cref="DisabledDaysOfWeekProperty"/>
		/// <seealso cref="DisabledDates"/>
		/// <seealso cref="DisabledDaysOfWeekVisibility"/>
		//[Description("Returns or sets a flagged enumeration indicating which days of the week are always disabled.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public DayOfWeekFlags DisabledDaysOfWeek
		{
			get
			{
				return (DayOfWeekFlags)this.GetValue(XamCalendar.DisabledDaysOfWeekProperty);
			}
			set
			{
				this.SetValue(XamCalendar.DisabledDaysOfWeekProperty, value);
			}
		}

		#endregion //DisabledDaysOfWeek

		#region DisabledDaysOfWeekVisibility

		/// <summary>
		/// Identifies the <see cref="DisabledDaysOfWeekVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DisabledDaysOfWeekVisibilityProperty = DependencyPropertyUtilities.Register("DisabledDaysOfWeekVisibility",
			typeof(Visibility), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnDisabledDaysOfWeekVisibilityChanged))
			);
		private static void OnDisabledDaysOfWeekVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{

			XamCalendar cal = (XamCalendar)d;
			
			bool isVisible = CalendarUtilities.ValidateNonHiddenVisibility(e);

			cal.DisabledDaysOfWeekVisibilityInternal = (Visibility)e.NewValue;

		}

		/// <summary>
		/// Returns or sets a boolean indicating whether days of the week disabled using the <see cref="DisabledDaysOfWeek"/> should be displayed in the calendar.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default all days of the week will be displayed within the control including those 
		/// that are disabled using the <see cref="DisabledDaysOfWeek"/>. This property can be set to true to 
		/// hide the disabled days of the week.</p>
		/// </remarks>
		/// <seealso cref="DisabledDaysOfWeekVisibilityProperty"/>
		/// <seealso cref="DisabledDaysOfWeek"/>
		//[Description("Returns or sets a boolean indicating whether days of the week disabled using the 'DisabledDaysOfWeek' should be displayed in the calendar.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public Visibility DisabledDaysOfWeekVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamCalendar.DisabledDaysOfWeekVisibilityProperty);
			}
			set
			{
				this.SetValue(XamCalendar.DisabledDaysOfWeekVisibilityProperty, value);
			}
		}

		#endregion //DisabledDaysOfWeekVisibility

		#region MaxDate

		/// <summary>
		/// Identifies the <see cref="MaxDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxDateProperty = DependencyPropertyUtilities.Register("MaxDate",
			typeof(DateTime), typeof(XamCalendar),
			// JJD 1/11/12 - TFS96836 - Strip off time portion from MaxValue
			//DependencyPropertyUtilities.CreateMetadata(DateTime.MaxValue, new PropertyChangedCallback(OnMinMaxDateChanged))
			DependencyPropertyUtilities.CreateMetadata(DateTime.MaxValue.Date, new PropertyChangedCallback(OnMinMaxDateChanged))
			);

		//private static object CoerceMaxDate(DependencyObject d, object newValue)
		//{
		//    XamCalendar cal = d as XamCalendar;
		//    DateTime newDate = (DateTime)newValue;

		//    if (newDate < cal.MinDate)
		//        newDate = cal.MinDate;

		//    // ensure its valid for the current calendar
		//    newDate = cal.CalendarManager.ConstrainBetweenMinMaxDate(newDate);

		//    return newDate;
		//}

		private static void OnMinMaxDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = d as XamCalendar;

			if (null != cal)
				cal.AllowableDateRange = new DateRange(cal.MinDate, cal.MaxDate);

			//{
			//    using (new CalendarItemGroup.GroupInitializationHelper(new List<CalendarItemGroup>( cal.GetGroups()))
			//    {
			//        cal.CoerceValue(XamCalendar.ReferenceDateProperty);
			//        cal.CoerceValue(XamCalendar.MeasureDateProperty);
			//        cal.NotifyGroupChange(CalendarChange.MaxDateChanged);

			//        // update selection
			//        if (cal.IsInitialized)
			//            cal.SelectedDates.EnsureWithinMinMax();
			//    }
			//}
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
		/// <seealso cref="XamCalendar.DisabledDaysOfWeek"/>
		//[Description("Returns or sets the preferred maximum date that can be selected or activated for the control.")]
		//[Category("Calendar Properties")] // Behavior

		[DependsOn("MinDate")]



		[Bindable(true)]
		public DateTime MaxDate
		{
			get
			{
				return (DateTime)this.GetValue(XamCalendar.MaxDateProperty);
			}
			set
			{
				this.SetValue(XamCalendar.MaxDateProperty, value);
			}
		}

		#endregion //MaxDate

		#region MaxSelectedDates

		/// <summary>
		/// Identifies the <see cref="MaxSelectedDates"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MaxSelectedDatesProperty = DependencyPropertyUtilities.Register("MaxSelectedDates",
			typeof(int), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(CalendarUtilities.ZeroInt, new PropertyChangedCallback(OnMaxSelectedDatesChanged))
			);

		private static void OnMaxSelectedDatesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarBase cal = (CalendarBase)d;
			int max = (int)e.NewValue;

			CoreUtilities.ValidateIsNotNegative(max, DependencyPropertyUtilities.GetName(e.Property));

			cal.MaxSelectedDatesInternal = max;
		}

		/// <summary>
		/// Returns or sets the maximum number of days that can be selected at any time.
		/// </summary>
		/// <p class="note">The MaxSelectedDates is only used when the <see cref="SelectionType"/> is set 
		/// to a value that allows multiple selection such as <b>Range</b> or <b>Extended</b>.</p>
		/// <seealso cref="MaxSelectedDatesProperty"/>
		/// <seealso cref="CalendarBase.SelectedDates"/>
		/// <seealso cref="SelectionType"/>
		//[Description("Returns or sets the maximum number of days that can be selected at any time.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public int MaxSelectedDates
		{
			get
			{
				return (int)this.GetValue(XamCalendar.MaxSelectedDatesProperty);
			}
			set
			{
				this.SetValue(XamCalendar.MaxSelectedDatesProperty, value);
			}
		}

		#endregion //MaxSelectedDates

		#region MinCalendarMode

		/// <summary>
		/// Identifies the <see cref="MinCalendarMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinCalendarModeProperty = DependencyPropertyUtilities.Register("MinCalendarMode",
			typeof(CalendarZoomMode), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(CalendarZoomMode.Days, new PropertyChangedCallback(OnMinCalendarModeChanged))
			);

		private static void OnMinCalendarModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = (XamCalendar)d;

			cal.MinCalendarModeResolved = (CalendarZoomMode)e.NewValue;
		}

		/// <summary>
		/// Returns or sets an enumeration that indicates the minimum calendar item type that can be selected within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The <see cref="CalendarBase"/> provides the ability to zoom out to see larger ranges of dates and then zoom back 
		/// in to change the selection similar to the functionality found in the Microsoft Vista Common Controls Calendar. While the 
		/// CurrentMode controls the current mode that the contained <see cref="CalendarItemGroup"/> instances use to display its items, the 
		/// MinCalendarMode determines the minimum mode into which the control may zoom. For example, when set the <b>Months</b>, the control will not 
		/// be able to zoom in any further to allow selection/viewing of individual dates/days.</p>
		/// <p class="body">When the <see cref="CalendarBase.CurrentMode"/> is set to the same value as the MinCalendarMode, interacting within the items (e.g. 
		/// via the keyboard and mouse) will affect the current <see cref="CalendarBase.SelectedDate"/> and <see cref="CalendarBase.SelectedDates"/>. When the CurrentMode 
		/// is higher than the MinCalendarMode (i.e. the user has zoomed out), the selection will not be changed via the ui. Instead, the keyboard and 
		/// mouse will be used to navigate the calendar without affecting the current selection. When the user then zooms in to the MinCalendarMode, they 
		/// may then modify the selection.</p>
		/// <p class="body">There are two commands (<see cref="CalendarCommandType.ZoomOutCalendarMode"/> 
		/// and <see cref="CalendarCommandType.ZoomInCalendarMode"/>) that may be used to change the CurrentMode.</p>
		/// <p class="note"><b>Note:</b> The value for this property will restrict the available values for the <see cref="CalendarBase.CurrentMode"/>.</p>
		/// </remarks>
		/// <seealso cref="MinCalendarModeProperty"/>
		/// <seealso cref="CalendarBase.CurrentMode"/>
		//[Description("Returns or sets an enumeration that indicates the minimum calendar item type that can be selected within the control.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public CalendarZoomMode MinCalendarMode
		{
			get
			{
				return (CalendarZoomMode)this.GetValue(XamCalendar.MinCalendarModeProperty);
			}
			set
			{
				this.SetValue(XamCalendar.MinCalendarModeProperty, value);
			}
		}

		#endregion //MinCalendarMode

		#region MinDate

		/// <summary>
		/// Identifies the <see cref="MinDate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinDateProperty = DependencyPropertyUtilities.Register("MinDate",
			typeof(DateTime), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(DateTime.MinValue, new PropertyChangedCallback(OnMinMaxDateChanged))
			);

		//private static void OnMinDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		//{
		//    XamCalendar cal = d as XamCalendar;

		//    if (null != cal)
		//    {
		//        using (new CalendarItemGroup.GroupInitializationHelper(cal._groups))
		//        {
		//            cal.CoerceValue(XamCalendar.MaxDateProperty);
		//            cal.CoerceValue(XamCalendar.ReferenceDateProperty);
		//            cal.CoerceValue(XamCalendar.MeasureDateProperty);
		//            cal.NotifyGroupChange(CalendarChange.MinDateChanged);

		//            // update selection
		//            if (cal.IsInitialized)
		//                cal.SelectedDates.EnsureWithinMinMax();
		//        }

		//    }
		//}

		//private static object CoerceMinDate(DependencyObject d, object newValue)
		//{
		//    XamCalendar cal = d as XamCalendar;
		//    DateTime newDate = (DateTime)newValue;

		//    // ensure its valid for the current calendar
		//    newDate = cal._calendarManager.ConstrainBetweenMinMaxDate(newDate);

		//    return newDate;
		//}

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
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]



		public DateTime MinDate
		{
			get
			{
				return (DateTime)this.GetValue(XamCalendar.MinDateProperty);
			}
			set
			{
				this.SetValue(XamCalendar.MinDateProperty, value);
			}
		}

		#endregion //MinDate

		#region FirstDayOfWeek

		/// <summary>
		/// Identifies the <see cref="FirstDayOfWeek"/> dependency property
		/// </summary>
		public static readonly DependencyProperty FirstDayOfWeekProperty = DependencyPropertyUtilities.Register("FirstDayOfWeek",
			typeof(DayOfWeek?), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFirstDayOfWeekChanged))
			);

		private static void OnFirstDayOfWeekChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = d as XamCalendar;

			cal.FirstDayOfWeekInternal = (DayOfWeek?)e.NewValue;
		}

		/// <summary>
		/// Gets or sets the <see cref="DayOfWeek"/> that represents the first day of the week or null to use the value from the current <see cref="System.Globalization.CultureInfo"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">By default the FirstDayOfWeek is set to null. When set to null, the <see cref="DateTimeFormatInfo.FirstDayOfWeek"/> 
		/// from the control's <see cref="FrameworkElement.Language"/> will be used.</p>
		/// </remarks>
		//[Description("Gets or sets the DayOfWeek that represents the first day of the week or null to use the value from the current CultureInfo.")]
		//[Category("Calendar Properties")] // Behavior





		[Bindable(true)]
		public DayOfWeek? FirstDayOfWeek
		{
			get
			{
				return (DayOfWeek?)this.GetValue(XamCalendar.FirstDayOfWeekProperty);
			}
			set
			{
				this.SetValue(XamCalendar.FirstDayOfWeekProperty, value);
			}
		}

		#endregion //FirstDayOfWeek

		#region SelectionMode

		/// <summary>
		/// Identifies the <see cref="SelectionType"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectionModeProperty = DependencyPropertyUtilities.Register("SelectionMode",
			typeof(CalendarDateSelectionMode), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(CalendarDateSelectionMode.Extended, new PropertyChangedCallback(OnSelectionModeChanged))
			);

		private static void OnSelectionModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = (XamCalendar)d;
			// AS 9/4/08
			// The value could go to default.
			//
			//SelectionType type = (SelectionType)e.NewValue;
			SelectionType type = cal.CurrentSelectionTypeResolved;

			if (SelectionStrategyBase.IsSingleSelectStrategy(type))
			{

				DateCollection selectedDates = cal.SelectedDatesInternal;

				if (cal.SelectedDate.HasValue)
					cal._pivotRange = new DateRange(cal.SelectedDate.Value);
				else
					cal._pivotRange = null;

				if (selectedDates.Count > 1)
					selectedDates.Reinitialize(new DateTime[] { cal.SelectedDates[0] });
			}
			else if (false == SelectionStrategyBase.IsMultiSelectStrategy(type))
			{
				// must be none so clear selection
				cal._pivotRange = null;
				cal.SelectedDatesInternal.Clear();
			}
		}

		/// <summary>
		/// Determines how many items can be selected.
		/// </summary>
		/// <remarks>
		/// <p class="body">The default selection type is Extended which allows multiple discontinuous 
		/// ranges of dates to be selected. As dates are selected, the <see cref="CalendarBase.SelectedDatesChanged"/> 
		/// event is raised and the <see cref="CalendarBase.SelectedDate"/> and <see cref="CalendarBase.SelectedDates"/> properties 
		/// are updated. When using a multiple selection type such as Extended or Range, you can use the 
		/// <see cref="XamCalendar.MaxSelectedDates"/> to control the maximum number of <see cref="CalendarItem"/> 
		/// instances that the user can select.</p>
		/// </remarks>
		/// <seealso cref="CalendarBase.SelectedDates"/>
		/// <seealso cref="XamCalendar.MaxSelectedDates"/>
		/// <seealso cref="CalendarBase.SelectedDatesChanged"/>
		//[Description("Determines how items can be selected")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public CalendarDateSelectionMode SelectionMode
		{
			get
			{
				return (CalendarDateSelectionMode)this.GetValue(XamCalendar.SelectionModeProperty);
			}
			set
			{
				this.SetValue(XamCalendar.SelectionModeProperty, value);
			}
		}

		#endregion //SelectionType

		#region TodayButtonVisibility

		/// <summary>
		/// Identifies the <see cref="TodayButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayButtonVisibilityProperty = DependencyPropertyUtilities.Register("TodayButtonVisibility",
			typeof(Visibility), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox, new PropertyChangedCallback(OnTodayButtonVisibilityChanged))
			);

		private static void OnTodayButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar instance = (XamCalendar)d;

		}

		/// <summary>
		/// Returns or sets a boolean that indicates whether the today button should be displayed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Today button is used to allow the user to select and bring into view the 
		/// <see cref="CalendarItem"/> that represents the current date. This button uses the <see cref="CalendarCommandType.Today"/> 
		/// command to perform the operation.</p>
		/// </remarks>
		//[Description("Returns or sets a value that indicates whether the today button should be displayed.")]
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public Visibility TodayButtonVisibility
		{
			get { return (Visibility)this.GetValue(TodayButtonVisibilityProperty); }
			set { this.SetValue(TodayButtonVisibilityProperty, value); }
		}

		#endregion //TodayButtonVisibility

		#region WeekRule

		/// <summary>
		/// Identifies the <see cref="WeekRule"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WeekRuleProperty = DependencyPropertyUtilities.Register("WeekRule",
			typeof(CalendarWeekRule?), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnWeekRuleChanged))
			);


		private static void OnWeekRuleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = d as XamCalendar;

			cal.WeekRuleInternal = (CalendarWeekRule?)e.NewValue;
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
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]





		public CalendarWeekRule? WeekRule
		{
			get { return (CalendarWeekRule?)this.GetValue(WeekRuleProperty); }
			set { this.SetValue(WeekRuleProperty, value); }
		}

		#endregion //WeekRule

		#region Workdays

		/// <summary>
		/// Identifies the <see cref="Workdays"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WorkdaysProperty = DependencyPropertyUtilities.Register("Workdays",
			typeof(DayOfWeekFlags), typeof(XamCalendar),
			DependencyPropertyUtilities.CreateMetadata(DefaultWorkdays, new PropertyChangedCallback(OnWorkdaysChanged))
			);

		private static void OnWorkdaysChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamCalendar cal = (XamCalendar)d;
			cal.WorkDaysInternal = (DayOfWeekFlags)e.NewValue;
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
		//[Category("Calendar Properties")] // Behavior
		[Bindable(true)]
		public DayOfWeekFlags Workdays
		{
			get
			{
				return (DayOfWeekFlags)this.GetValue(XamCalendar.WorkdaysProperty);
			}
			set
			{
				this.SetValue(XamCalendar.WorkdaysProperty, value);
			}
		}

		#endregion //Workdays


		#endregion //Public Properties	
    
		#endregion //Properties	

        #region Methods

        #region Static

        #region RegisterResources

        /// <summary>
        /// Adds an additonal Resx file in which the control will pull its resources from.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that contains the resources to be used.</param>
        /// <param name="assembly">The assembly in which the resx file is embedded.</param>
        /// <remarks>Don't include the extension of the file, but prefix it with the default Namespace of the assembly.</remarks>
        public static void RegisterResources(string name, System.Reflection.Assembly assembly)
        {
#pragma warning disable 436
            SR.AddResource(name, assembly);
#pragma warning restore 436
        }

        #endregion // RegisterResources

        #region UnregisterResources

        /// <summary>
        /// Removes a previously registered resx file.
        /// </summary>
        /// <param name="name">The name of the embedded resx file that was used for registration.</param>
        /// <remarks>
        /// Note: this won't have any effect on controls that are already in view and are already displaying strings.
        /// It will only affect any new controls created.
        /// </remarks>
        public static void UnregisterResources(string name)
        {
#pragma warning disable 436
            SR.RemoveResource(name);
#pragma warning restore 436
        }

        #endregion // UnregisterResources

        #endregion // Static

        #endregion // Methods

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