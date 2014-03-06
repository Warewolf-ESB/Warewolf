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
using Infragistics.Controls.Schedules;

using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Threading;
using System.Threading;
using Infragistics.Collections;
using Infragistics.Controls;
using Infragistics.Controls.Schedules.Primitives;
using Infragistics.Controls.Editors;
using Infragistics.Controls.Editors.Primitives;

namespace Infragistics.Controls.Schedules
{

	/// <summary>
	/// A custom control used to display one or more months.
	/// </summary>
	/// <seealso cref="XamOutlookCalendarView.DateNavigator"/>
	[TemplatePart(Name = ScheduleControlBase.PartRootPanel, Type = typeof(Grid))]

	
	
	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!

	public class XamDateNavigator : CalendarBase, IScheduleControl, IOutlookDateNavigator
	{
		#region Member Variables


		private Infragistics.Windows.Licensing.UltraLicense _license;

		private DispatcherOperation _activityVerifyPending;
		private XamScheduleDataManager _dataManagerResolved;
		private Dictionary<DateTime, MonthActivity> _monthMap;
		private RoutedEventHandler _toolTipOpenedHandler;
		private RoutedEventHandler _toolTipClosedHandler;

		// JJD 5/2/11 - TFS74024
		// Added explicit event for IOutlookDateNavigator.SelectedDatesChanged so
		// we could control the order of the event firing
		private EventHandler<SelectedDatesChangedEventArgs> _selectionChanged;


		#endregion //Member Variables

		#region Constructor

		static XamDateNavigator()
		{

			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDateNavigator), new FrameworkPropertyMetadata(typeof(XamDateNavigator)));


		}

		/// <summary>
		/// Initializes a new <see cref="XamDateNavigator"/>
		/// </summary>
		public XamDateNavigator()
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
				this._license = LicenseManager.Validate(typeof(XamDateNavigator), this) as Infragistics.Windows.Licensing.UltraLicense;
			}
			catch (System.IO.FileNotFoundException) { }


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)



			_toolTipOpenedHandler = new RoutedEventHandler(this.OnDayToolTipOpened);
			_toolTipClosedHandler = new RoutedEventHandler(this.OnDayToolTipClosed);

		}

		#endregion //Constructor

		#region Base class overrides

		#region AddLogicalDayDuration
		internal override DateTime AddLogicalDayDuration(DateTime start)
		{
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm != null)
			{
				DateInfoProvider dip = dm.DateInfoProviderResolved;

				if (dip != null)
				{
					DateTime? dt = dip.Add(start, dm.LogicalDayDuration);

					if (dt.HasValue)
						return dt.Value;
					else
						return start;
				}
			}

			return base.AddLogicalDayDuration(start);
		}
		#endregion // AddLogicalDayDuration

		#region ApplyLogicalDayOffset
		internal override DateTime ApplyLogicalDayOffset(DateTime date)
		{
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm != null)
			{
				TimeSpan logicalDayOffset = dm.LogicalDayOffset;

				if (logicalDayOffset != TimeSpan.Zero)
				{
					DateInfoProvider dip = dm.DateInfoProviderResolved;

					if (dip != null)
					{
						DateTime? dt = dip.Add(date, logicalDayOffset);

						if (dt.HasValue)
							return dt.Value;
					}
				}
			}

			return date;
		}
		#endregion // ApplyLogicalDayOffset

		// JJD 10/12/11 - TFS89043 - added
		#region GetTodayRange

		internal override DateRange GetTodayRange()
		{
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm != null)
			{
				// JJD 10/12/11 - TFS89043 
				// Call the dm's GetLogicalDayRange
				DateRange? logDayRange = dm.GetLogicalDayRange();

				if (logDayRange.HasValue)
					return logDayRange.Value;
			}

			return base.GetTodayRange();
		}

		#endregion //GetTodayRange

		// JJD 5/2/11 - TFS74024 - added
		#region OnSelectedDatesChanged

		/// <summary>
		/// Called when the selection has changed
		/// </summary>
		protected override void OnSelectedDatesChanged(SelectedDatesChangedEventArgs args)
		{
			// call the base implementaion first which will raise the SelectedDatesChanged event
			// for all listeners
			base.OnSelectedDatesChanged(args);

			// now that all listeners of the control's event have been notified we can
			// raise the IOutlookDateNavigator.SelectedDatesChanged event. The order of this
			// event raising is important because an IOutlookDateNavigator listener can
			// synchronouusly change the selection, e.g. if we are in week selection mode
			// and a single day is selected.
			if (_selectionChanged != null)
				_selectionChanged(this, args);
		}

		#endregion //OnSelectedDatesChanged	
    
		#region ShouldHighlightDay

		/// <summary>
		/// Returns true if the day element should be highlighted
		/// </summary>
		/// <param name="dayElement"></param>
		/// <returns>True to highlight the day.</returns>
		/// <seealso cref="CalendarDay"/>
		/// <seealso cref="CalendarItem.IsHighlighted"/>
		protected internal override bool ShouldHighlightDay(CalendarDay dayElement)
		{
			bool shouldHighlight = false;
			bool hasActivity = false;

			DateTime date = dayElement.StartDate.Date;

			HighlightDayCriteria criteria = this.HighlightDayCriteria;

			switch (criteria)
			{
				case HighlightDayCriteria.All:
					shouldHighlight = true;
					break;
				case HighlightDayCriteria.None:
					shouldHighlight = false;
					break;
				case HighlightDayCriteria.Workdays:
					shouldHighlight = dayElement.IsWorkday;
					break;
			}

			bool showTooltips = this.ShowActivityToolTips;

			if (showTooltips || criteria == HighlightDayCriteria.DaysWithActivity)
			{
				if (dayElement.IsLeadingOrTrailingItem == false)
				{
					hasActivity = this.HasActivity(date);

					if (criteria == HighlightDayCriteria.DaysWithActivity)
						shouldHighlight = hasActivity;
				}
			}

			// get the existing tooltip on the day element
			ScheduleToolTip tt = ToolTipService.GetToolTip(dayElement) as ScheduleToolTip;

			if (hasActivity && showTooltips)
			{
				// create a new tooltip if we don't already have one
				if (tt == null)
				{
					tt = new ScheduleToolTip();

					// wire the Opened and Closed events. 
					// Note: we delay creating the tooltip's content until it is opened
					tt.Opened += _toolTipOpenedHandler;
					tt.Closed += _toolTipClosedHandler;

					// set the Tag to the day element so we can know which day we
					// are dealing with in the Opened and Closed event handlers
					tt.Tag = dayElement;

					ToolTipService.SetToolTip(dayElement, tt);
				}
			}
			else
			{
				// since we are not showing a tooltip for this day clear its tooltip property
				if (tt != null && tt.Tag is CalendarDay)
					dayElement.ClearValue(ToolTipService.ToolTipProperty);
			}

			return shouldHighlight;
		}

		#endregion //ShouldHighlightDay

		#region SupportsWeekSelectionMode

		internal override bool SupportsWeekSelectionMode { get { return true; } }

		#endregion //SupportsWeekSelectionMode

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ActivityToolTipTemplate

		/// <summary>
		/// Identifies the <see cref="ActivityToolTipTemplate"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActivityToolTipTemplateProperty = DependencyPropertyUtilities.Register("ActivityToolTipTemplate",
			typeof(DataTemplate), typeof(XamDateNavigator),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the template used for activity tooltips
		/// </summary>
		/// <seealso cref="ActivityToolTipTemplateProperty"/>
		/// <seealso cref="ShowActivityToolTips"/>
		public DataTemplate ActivityToolTipTemplate
		{
			get
			{
				return (DataTemplate)this.GetValue(XamDateNavigator.ActivityToolTipTemplateProperty);
			}
			set
			{
				this.SetValue(XamDateNavigator.ActivityToolTipTemplateProperty, value);
			}
		}

		#endregion //ActivityToolTipTemplate

		#region DataManager

		/// <summary>
		/// Identifies the <see cref="DataManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataManagerProperty = DependencyPropertyUtilities.Register("DataManager",
			typeof(XamScheduleDataManager), typeof(XamDateNavigator),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDataManagerChanged))
			);

		private static void OnDataManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateNavigator ctrl = (XamDateNavigator)d;

			var oldValue = e.OldValue as XamScheduleDataManager;
			var newValue = e.NewValue as XamScheduleDataManager;


			// if the data manager is set to a new instance then push that into the datamanagerresolved 
			// property. if it is cleared then we should pass that along as well since we don't want to 
			// maintain a reference to it. if we need to later we can create one
			ctrl.DataManagerResolved = newValue;

			// JJD 11/10/11 - TFS76826
			// Make sure we synchonize the settings right away since calling VerifyInitialState on the Data Manager
			// won't always call our IScheduleControl.VerifyInitialState(List<DataErrorInfo> errorList) method
			// right away. When waiting for a data connection to be established sometimes it can take sevveral seonds.
			// In the meantime the XamDataNavigator should be displayed with the correct settings and color scheme
			ctrl.VerifyResourceProvider(false);

			// call VerifyInitialState on the data manager.
			// Note: this must be done after the ctrl's DataManagerResolved is set above
			if (newValue != null)
				newValue.VerifyInitialState();

		}

		/// <summary>
		/// Returns or sets the object that provides the activity and resource information that will be displayed by the control.
		/// </summary>
		/// <seealso cref="DataManagerProperty"/>
		public XamScheduleDataManager DataManager
		{
			get
			{
				return (XamScheduleDataManager)this.GetValue(XamDateNavigator.DataManagerProperty);
			}
			set
			{
				this.SetValue(XamDateNavigator.DataManagerProperty, value);
			}
		}
		#endregion //DataManager

		#region HighlightDayCriteria

		/// <summary>
		/// Identifies the <see cref="HighlightDayCriteria"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HighlightDayCriteriaProperty = DependencyPropertyUtilities.Register("HighlightDayCriteria",
			typeof(HighlightDayCriteria), typeof(XamDateNavigator),
			DependencyPropertyUtilities.CreateMetadata(HighlightDayCriteria.DaysWithActivity, new PropertyChangedCallback(OnHighlightDayCriteriaChanged))
			);

		private static void OnHighlightDayCriteriaChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateNavigator instance = (XamDateNavigator)d;

			instance.ProcessActivityVerification();
			instance.InvalidateDisplay();
		}

		/// <summary>
		/// Determines which days, if any, are highlighted.
		/// </summary>
		/// <seealso cref="HighlightDayCriteriaProperty"/>
		public HighlightDayCriteria HighlightDayCriteria
		{
			get
			{
				return (HighlightDayCriteria)this.GetValue(XamDateNavigator.HighlightDayCriteriaProperty);
			}
			set
			{
				this.SetValue(XamDateNavigator.HighlightDayCriteriaProperty, value);
			}
		}

		#endregion //HighlightDayCriteria

		#region ShowActivityToolTips

		/// <summary>
		/// Identifies the <see cref="ShowActivityToolTips"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowActivityToolTipsProperty = DependencyPropertyUtilities.Register("ShowActivityToolTips",
			typeof(bool), typeof(XamDateNavigator),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnShowActivityToolTipsChanged))
			);

		private static void OnShowActivityToolTipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateNavigator instance = (XamDateNavigator)d;
			instance.ProcessActivityVerification();
			instance.InvalidateDisplay();

		}

		/// <summary>
		/// Returns or sets whether a tooltip will be displayed for each day that has activity
		/// </summary>
		/// <seealso cref="ShowActivityToolTipsProperty"/>
		/// <seealso cref="ActivityToolTipTemplate"/>
		public bool ShowActivityToolTips
		{
			get
			{
				return (bool)this.GetValue(XamDateNavigator.ShowActivityToolTipsProperty);
			}
			set
			{
				this.SetValue(XamDateNavigator.ShowActivityToolTipsProperty, value);
			}
		}

		#endregion //ShowActivityToolTips

		#region TodayButtonVisibility

		/// <summary>
		/// Identifies the <see cref="TodayButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TodayButtonVisibilityProperty = DependencyPropertyUtilities.Register("TodayButtonVisibility",
			typeof(Visibility), typeof(XamDateNavigator),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityCollapsedBox, new PropertyChangedCallback(OnTodayButtonVisibilityChanged))
			);

		private static void OnTodayButtonVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamDateNavigator instance = (XamDateNavigator)d;
		}

		/// <summary>
		/// Returns or sets a boolean that indicates whether the today button should be displayed.
		/// </summary>
		/// <remarks>
		/// <p class="body">The Today button is used to allow the user to select and bring into view the 
		/// <see cref="CalendarItem"/> that represents the current date. This button uses the <see cref="CalendarCommandType.Today"/> 
		/// routed command to perform the operation.</p>
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

		#endregion //Public Properties

		#region Internal Properties

		#region DataManagerResolved

		internal XamScheduleDataManager DataManagerResolved
		{
			get
			{
				this.VerifyDataManagerResolved();

				return _dataManagerResolved;
			}
			private set
			{
				if (_dataManagerResolved != value)
				{
					if (null != _dataManagerResolved)
						_dataManagerResolved.Controls.Remove(this);

					_dataManagerResolved = value;

					if (null != _dataManagerResolved)
						_dataManagerResolved.Controls.Add(this);

					// if the datamanager is changing then the calendar groups are as well
					//this.QueueInvalidation(InternalFlags.DataManagerChanged | InternalFlags.CalendarGroupsChanged | InternalFlags.CurrentDateChanged | InternalFlags.CalendarGroupsResolvedChanged);

					if (null != _dataManagerResolved)
					{
						//this.DefaultBrushProvider = _dataManagerResolved.ColorSchemeResolved.DefaultBrushProvider;

						//Binding binding = CreateBrushVersionBinding(_dataManagerResolved);

						//if (binding != null)
						//    BindingOperations.SetBinding(this, BrushVersionProperty, binding);

						_dataManagerResolved.BumpBrushVersion();

						//if (DesignerProperties.GetIsInDesignMode(this))
						//    this.ClearValue(BlockingErrorPropertyKey);
						//else
						//    this.BlockingError = _dataManagerResolved.BlockingError;
					}
					//else
					//    this.ClearValue(BlockingErrorPropertyKey);
				}
			}
		}

		#endregion //DataManagerResolved

		#endregion //Internal Properties

		#endregion //Properties

		#region Methods

		#region Internal Methods

		#region GetActivityForDay

		internal List<ActivityBase> GetActivityForDay(DateTime date)
		{
			MonthActivity ma = this.GetMonthActivity(date);

			return ma != null ? ma.GetActivityForDay(date) : new List<ActivityBase>();
		}

		#endregion //GetActivityForDay

		#region PostActivityVerification

		internal void PostActivityVerification()
		{
			if (_activityVerifyPending == null)
				_activityVerifyPending = this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.ProcessActivityVerification));

		}

		#endregion //PostActivityVerification

		#endregion //Internal Methods

		#region Private Methods

		#region GetMonthActivity

		private MonthActivity GetMonthActivity(DateTime date)
		{
			Infragistics.Controls.Editors.CalendarManager cm = this.CalendarManager;

			DateTime firstDayOfMonth = cm.GetItemStartDate(date, Infragistics.Controls.Editors.CalendarZoomMode.Months);

			MonthActivity ma = null;

			if (_monthMap != null && _monthMap.TryGetValue(firstDayOfMonth, out ma))
			{
				this.PostActivityVerification();
				return ma;
			}

			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm == null || dm.DataConnector == null)
				return null;

			Resource resource = dm.CurrentUser;

			if (resource == null)
				return null;

			List<ResourceCalendar> visibleCals = this.GetVisibleCalendarsForResource(resource);

			if (visibleCals.Count == 0)
				return null;

			DateTime lastDayOfMonth = cm.AddDays(firstDayOfMonth, cm.GetDaysInMonth(firstDayOfMonth));

			DateRange range = new DateRange(firstDayOfMonth, lastDayOfMonth);

			DateInfoProvider dip = dm.DateInfoProviderResolved;

			TimeZoneInfoProvider tzProvider = dm.TimeZoneInfoProviderResolved;

			TimeSpan offset = dm.LogicalDayOffset;

			DateRange utcRange = ScheduleUtilities.ConvertToUtc(tzProvider.LocalToken, range);

			utcRange.Start = dip.Add(utcRange.Start, offset) ?? utcRange.Start;
			utcRange.End = dip.Add(utcRange.End, offset) ?? utcRange.End;

			ActivityQuery query = new ActivityQuery(ActivityTypes.All, utcRange, visibleCals);

			ActivityQueryResult result = dm.GetActivities(query);

			ma = new MonthActivity(this, firstDayOfMonth, range, result, offset);

			if (_monthMap == null)
				_monthMap = new Dictionary<DateTime, MonthActivity>();

			_monthMap.Add(firstDayOfMonth, ma);

			return ma;
		}

		#endregion //GetMonthActivity

		#region GetVisibleCalendarsForResource

		private List<ResourceCalendar> GetVisibleCalendarsForResource(Resource resource)
		{
			List<ResourceCalendar> cals = new List<ResourceCalendar>();

			foreach (ResourceCalendar cal in resource.Calendars)
			{
				if (cal.BrushProvider != null)
					cals.Add(cal);
			}

			if (cals.Count == 0 &&
				resource.PrimaryCalendar != null)
				cals.Add(resource.PrimaryCalendar);

			return cals;
		}

		#endregion //GetVisibleCalendarsForResource

		#region HasActivity

		private bool HasActivity(DateTime date)
		{
			MonthActivity ma = this.GetMonthActivity(date);

			return ma != null && ma.HasActivity(date);
		}

		#endregion //HasActivity

		#region OnDayToolTipClosed

		private void OnDayToolTipClosed(object sender, RoutedEventArgs e)
		{

			e.Handled = true;

			ScheduleToolTip tt = sender as ScheduleToolTip;

			tt.ClearValue(ScheduleToolTip.ContentProperty);
		}

		#endregion //OnDayToolTipClosed

		#region OnDayToolTipOpened

		private void OnDayToolTipOpened(object sender, RoutedEventArgs e)
		{

			e.Handled = true;

			ScheduleToolTip tt = sender as ScheduleToolTip;

			// Get the Calendar day from the tag
			CalendarDay day = tt.Tag as CalendarDay;

			if (day == null)
				return;

			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm == null)
				return;

			CalendarColorScheme colorScheme = dm.ColorSchemeResolved;

			CalendarBrushProvider provider = null;
			if (colorScheme != null)
				provider = colorScheme.DefaultBrushProvider;

			DateTime date = day.StartDate.Date;

			// get the day's activities
			List<ActivityBase> activities = this.GetActivityForDay(date);

			List<DayActivityToolTipInfo> activityInfos = new List<DayActivityToolTipInfo>();

			if (activities != null)
			{
				foreach (ActivityBase activity in activities)
				{
					if (activity.IsVisibleResolved)
						activityInfos.Add(new DayActivityToolTipInfo(date, activity, dm, provider));
				}
			}

			// create a DayToolTipInfo object to expose the date and list of activities
			DayToolTipInfo ttInfo = new DayToolTipInfo(date, activityInfos, dm);

			// set the brush provider on both the tooltip and the DayToolTipInfo
			CalendarBrushProvider.SetBrushProvider(tt, provider);
			CalendarBrushProvider.SetBrushProvider(ttInfo, provider);

			tt.Content = ttInfo;
			tt.ContentTemplate = this.ActivityToolTipTemplate;

		}

		#endregion //OnDayToolTipOpened

		#region ProcessActivityVerification

		private void ProcessActivityVerification()
		{
			_activityVerifyPending = null;

			if (this.ShowActivityToolTips == false && this.HighlightDayCriteria != HighlightDayCriteria.DaysWithActivity)
			{
				ReleaseAllActivityQueries();

				return;
			}

			XamScheduleDataManager dm = this.DataManagerResolved;

			// if we don't have any entries in the month map but we have enough info to create them
			// then invalidate the display to trigger calls to ShouldHighlghtDay which should create them
			if (_monthMap == null || _monthMap.Count == 0)
			{
				if (dm != null &&
					dm.DataConnector != null &&
					dm.CurrentUser != null)
				{
					List<ResourceCalendar> visibleCals = this.GetVisibleCalendarsForResource(dm.CurrentUser);

					if (visibleCals.Count > 0)
						this.InvalidateDisplay();

					return;
				}
			}

			if (_monthMap != null)
			{
				if (dm == null || dm.DataConnector == null)
				{
					this.ReleaseAllActivityQueries();
					return;
				}

				Resource currentUser = dm.CurrentUser;

				Resource resource = dm.CurrentUser;

				List<ResourceCalendar> visibleCals = resource != null ? this.GetVisibleCalendarsForResource(resource) : null;

				CalendarItemGroup firstGroup = this.FirstGroup;
				CalendarItemGroup lastGroup = this.LastGroup;

				DateTime? start = firstGroup.FirstDateOfGroup;
				DateTime? end = lastGroup.LastDateOfGroup;

				if (resource == null || visibleCals.Count == 0 || start == null || end == null)
				{
					this.ReleaseAllActivityQueries();
					return;
				}

				TimeSpan logicalDayOffset = dm.LogicalDayOffset;

				List<MonthActivity> months = new List<MonthActivity>(_monthMap.Values);

				foreach (MonthActivity ma in months)
				{
					if (ma.Month < start.Value || ma.Month > end.Value)
					{
						this.ReleaseMonthActivity(ma);
						continue;
					}

					if (!ma.IsCompatible(visibleCals, logicalDayOffset))
					{
						this.ReleaseMonthActivity(ma);

						this.InvalidateIsHighlighted(ma.Month);
						continue;
					}

					if (ma.IsDirty)
					{
						if (!ma.ProcessQueryResults())
							this.ReleaseMonthActivity(ma);

						this.InvalidateIsHighlighted(ma.Month);
					}
				}
			}
		}

		#endregion //ProcessActivityVerification

		#region ReleaseAllActivityQueries

		private void ReleaseAllActivityQueries()
		{
			if (_monthMap != null)
			{
				List<MonthActivity> monthActs = new List<MonthActivity>(_monthMap.Values);

				// null out the map so we don't incur the overhead of removing each
				// month activity when we call ReleaseMonthActivity below
				_monthMap = null;

				foreach (MonthActivity ma in monthActs)
				{
					this.ReleaseMonthActivity(ma);
					this.InvalidateIsHighlighted(ma.Month);
				}
			}
		}

		#endregion //ReleaseAllActivityQueries

		#region ReleaseMonthActivity

		private void ReleaseMonthActivity(MonthActivity monthActivity)
		{
			if (_monthMap != null)
				_monthMap.Remove(monthActivity.Month);

			monthActivity.ReleaseQuery();
		}

		#endregion //ReleaseMonthActivity

		#region VerifyDataManagerResolved
		private void VerifyDataManagerResolved()
		{
			if (_dataManagerResolved != null)
				return;

			// if the DataManager property is bound then we shouldn't put
			// a default one in. this comes up particularly when using an 
			// elementname binding as at least in sl, the elementname binding 
			// waits until the loaded event of the target element to get
			// the element so you end up seeing a flicker from the default 
			// calendar to the real one. the primary intent of the default 
			// datamanager is to handle runtime and some limited (i.e. 
			// prototype) scenarios anyway
			object dm = this.ReadLocalValue(DataManagerProperty);

			if (dm != null && dm != DependencyProperty.UnsetValue)
				return;

			Debug.Assert(this.DataManager == null);

			if (DesignerProperties.GetIsInDesignMode(this))
				this.DataManagerResolved = ScheduleControlBase.CreateDefaultDataManager();
		}
		#endregion // VerifyDataManagerResolved

		#region VerifyResourceProvider

		// JJD 11/10/11 - TFS76826 
		// Added optional verifyActivity parameter
		//private void VerifyResourceProvider()
		private void VerifyResourceProvider(bool verifyActivity = true)
		{
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm == null)
				return;

			CalendarColorScheme colorScheme = dm.ColorSchemeResolved;

			if (colorScheme != null)
				this.DefaultResourceProvider = colorScheme.DateNavigatorResourceProvider;

			base.WorkDaysInternal = ScheduleUtilities.GetDefaultWorkDays(dm);

			base.AllowableDateRange = ScheduleUtilities.GetMinMaxRange(dm);

			base.FirstDayOfWeekInternal = ScheduleUtilities.GetFirstDayOfWeek(dm);

			DateInfoProvider diProvider = dm.DateInfoProviderResolved;

			base.SetCalendarInfo(diProvider.Calendar, diProvider.DateTimeFormatInfo);

			// JJD 11/10/11 - TFS76826 
			// Only post activity verification when the verifyActivity parameter is true.
			if (verifyActivity)
				this.PostActivityVerification();

		}

		#endregion //VerifyResourceProvider

		#endregion //Private Methods

		#endregion //Methods

		#region IScheduleControl Members

		void IScheduleControl.OnColorSchemeResolvedChanged()
		{
			this.VerifyResourceProvider();
		}

		void IScheduleControl.OnSettingsChanged(object sender, string property, object extraInfo)
		{
			
			this.VerifyResourceProvider();
		}

		void IScheduleControl.RefreshDisplay()
		{
			this.VerifyResourceProvider();
		}

		void IScheduleControl.VerifyInitialState(List<DataErrorInfo> errorList)
		{
			this.VerifyResourceProvider();
		}

		#endregion

		#region IOutlookDateNavigator Members

		IList<DateTime> IOutlookDateNavigator.GetSelectedDates()
		{
			return this.SelectedDates;
		}

		void IOutlookDateNavigator.SetSelectedDates(IList<DateTime> dates)
		{
			// JJD 5/2/11 - TFS74024
			// See if the dates are the same as the existing selected dates
			//DateCollection dateCollection = this.SelectedDates;
			DateCollection dateCollection = this.SelectedDates;
			bool areDatesTheSame = false;
			if (dates != null && dates.Count == dateCollection.Count)
			{
				// JJD 5/2/11 - TFS74024
				// Since the counts are the same assume the dates are the same
				// until we check each date below
				areDatesTheSame = true;

				// JJD 5/2/11 - TFS74024
				// loop over the passed in dates. If any date isn't in the 
				// existing selected dates then set a flag and stop enumerating
				foreach (DateTime date in dates)
				{
					if (!dateCollection.ContainsDate(date))
					{
						areDatesTheSame = false;
						break;
					}
				}
			}

			// JJD 5/2/11 - TFS74024
			// Only update the selection if there is a change
			if (!areDatesTheSame)
			{
				// JJD 11/10/11 - TFS95841
				// Since the selection is being changed we need to snapshot the existing
				// selection and re-get the date collection using SelectedDatesInternal
				// This will return a temp collection that won't be committed until we
				// call RaisePendingSelectionChanged below
				this.SnapshotSelection();
				dateCollection = this.SelectedDatesInternal;

				// JJD 11/10/11 - TFS95841 
				dateCollection.Clear();

				if (dates != null && dates.Count > 0)
				{
					List<DateTime> dateList = new List<DateTime>(dates);
					dateList.Sort();

					DateRange range = new DateRange(dateList[0].Date);

					for (int i = 1, count = dateList.Count; i < count; i++)
					{
						DateTime date = dateList[i].Date;

						if (date.Subtract(range.End).Days > 1)
						{
							dateCollection.AddRange(new DateRange(range.Start, range.End), true);
							range = new DateRange(date);
						}
						else
						{
							range.End = date;
						}
					}

					dateCollection.AddRange(new DateRange(range.Start, range.End), true);
				}

				// JJD 11/10/11 - TFS95841 
				// Call RaisePendingSelectionChanged which will copy over the selection created above and raise
				// the SelectionChanged event with the appropriate added and removed dates in its event args
				this.RaisePendingSelectionChanged();
			}

			int selcount = this.SelectedDates.Count;

			if (selcount > 0)
			{
				DateTime firstSelectedDate = dateCollection[0];

				// set the active date to the first selected date this will scroll the active date into view
				this.ActiveDate = firstSelectedDate;

				if (selcount > 1)
				{
					DateTime lastSelectedDate = this.SelectedDates[selcount - 1];

					CalendarItemGroup lastGroup = this.LastGroup;
					CalendarItemGroup firstGroup = this.FirstGroup;

					// try to bring the entire selection into view
					if (lastGroup != firstGroup &&
						lastGroup.FirstDateOfGroup.HasValue &&
						firstGroup.FirstDateOfGroup.HasValue &&
						lastGroup.LastDateOfGroup.HasValue &&
						firstGroup.LastDateOfGroup.HasValue &&
						lastSelectedDate > lastGroup.LastDateOfGroup &&
						firstSelectedDate > firstGroup.LastDateOfGroup)
					{
						DateTime enddt = firstSelectedDate + (lastGroup.FirstDateOfGroup.Value - firstGroup.FirstDateOfGroup.Value);

						// use the lesser of the end date calculated above or the last selected date
						this.BringDateIntoView(enddt < lastSelectedDate ? enddt : lastSelectedDate);
					}

				}
			}
		}

		// JJD 5/2/11 - TFS74024
		// Added explicit event for IOutlookDateNavigator.SelectedDatesChanged so
		// we could control the order of the event firing
		event EventHandler<SelectedDatesChangedEventArgs> IOutlookDateNavigator.SelectedDatesChanged
		{
			add
			{
				_selectionChanged = Delegate.Combine(_selectionChanged, value) as EventHandler<SelectedDatesChangedEventArgs>;
			}
			remove
			{
				_selectionChanged = Delegate.Remove(_selectionChanged, value) as EventHandler<SelectedDatesChangedEventArgs>;
			}
		}

		#endregion

		#region MonthActivity private class

		private class MonthActivity
		{
			#region Private Members

			private XamDateNavigator _dateNavigator;
			private ActivityQueryResult _queryResult;
			private DateTime _month;
			private DateRange _range;
			private TimeSpan _logicalDayOffset;
			private bool _isDirty;
			private HashSet<DateTime> _daysWithActivity;
			private PropertyChangeListener<ActivityQueryResult> _listener;

			// JJD 11/11/11 - TFS79534
			// Cache the providers so we can tell when they have changed
			private DateInfoProvider _dateInfoProvider;
			private TimeZoneInfoProvider _tzProvider;
			private TimeZoneToken _localToken;

			#endregion //Private Members

			#region Constructor

			internal MonthActivity(XamDateNavigator dn, DateTime month, DateRange range, ActivityQueryResult queryResult, TimeSpan logicalDayOffset)
			{
				_dateNavigator = dn;
				_month = month;
				_range = range;
				_queryResult = queryResult;
				_logicalDayOffset = logicalDayOffset;
				_isDirty = true;

				_listener = new PropertyChangeListener<ActivityQueryResult>(_queryResult, this.OnQuerySubObjectChanged);

				ScheduleUtilities.AddListener(_queryResult, _listener, false);

				if (_queryResult.IsComplete)
					this.ProcessQueryResults();
			}

			#endregion //Constructor

			#region Properties

			internal bool IsDirty { get { return _isDirty; } }

			internal TimeSpan LogicalDayOffset { get { return _logicalDayOffset; } }

			internal DateTime Month { get { return _month; } }

			internal ActivityQueryResult QueryResult { get { return _queryResult; } }

			#endregion //Properties

			#region Methods

			#region GetActivityForDay

			internal List<ActivityBase> GetActivityForDay(DateTime date)
			{
				List<ActivityBase> activities = new List<ActivityBase>();

				if (!this.HasActivity(date))
					return activities;

				if (_queryResult.IsCanceled)
					return activities;

				// JJD 11/11/11 - TFS79534
				// Use cached providers and local token 
				Debug.Assert(_queryResult.IsComplete == false || _dateInfoProvider != null, "Cached Date Provider missing");
				Debug.Assert(_queryResult.IsComplete == false || _tzProvider != null, "Cached Time Zone Provider missing");
				Debug.Assert(_queryResult.IsComplete == false || _localToken != null, "Cached Local Token missing");
				//if (_queryResult.IsComplete)
				if (_queryResult.IsComplete && _dateInfoProvider != null && _tzProvider != null && _localToken != null)
				{
					XamScheduleDataManager dm = this._dateNavigator.DataManagerResolved;
			
					// JJD 11/11/11 - TFS79534
					// Use cached providers and local token 
					//DateInfoProvider dip = dm.DateInfoProviderResolved;
					//TimeZoneInfoProvider provider = dm.TimeZoneInfoProviderResolved;
					//TimeZoneToken token = _tzProvider.LocalToken;

					TimeSpan offset = new TimeSpan(-_logicalDayOffset.Ticks);

					TimeSpan duration = dm.LogicalDayDuration;


					DateTime startRange = date;
					DateTime? endRangeTemp = _dateInfoProvider.AddDays(date, 1);

					if (endRangeTemp == null)
						return activities;

					DateTime endRange = ScheduleUtilities.GetNonInclusiveEnd(endRangeTemp.Value);

					if (offset.Ticks != 0)
					{
						DateTime? temp = _dateInfoProvider.Add(startRange, offset);

						if (temp.HasValue)
							startRange = temp.Value;

						temp = _dateInfoProvider.Add(endRange, offset);

						if (temp.HasValue)
							endRange = temp.Value;

					}


					foreach (ActivityBase activity in _queryResult.Activities)
					{
						DateTime dt = activity.GetStartLocal(_localToken);
						DateTime dtEnd = activity.GetEndLocal(_localToken);

						if (dtEnd != dt)
							dtEnd = ScheduleUtilities.GetNonInclusiveEnd(dtEnd);

						if (offset.Ticks != 0)
						{
							DateTime? temp = _dateInfoProvider.Add(dt, offset);

							if (temp.HasValue)
								dt = temp.Value;

							temp = _dateInfoProvider.Add(dtEnd, offset);

							if (temp.HasValue)
								dtEnd = temp.Value;

						}

						if (endRange < dt)
							continue;

						if (startRange > dtEnd)
							continue;

						activities.Add(activity);
					}
				}

				return activities;

			}

			#endregion //GetActivityForDay

			#region HasActivity

			internal bool HasActivity(DateTime date)
			{
				return _daysWithActivity != null && _daysWithActivity.Contains(date);
			}

			#endregion //HasActivity

			#region IsCompatible

			internal bool IsCompatible(List<ResourceCalendar> calendars, TimeSpan logicalDayOffset)
			{
				if (logicalDayOffset != _logicalDayOffset)
					return false;

				int count = calendars.Count;

				ImmutableCollection<ResourceCalendar> queryCalendars = _queryResult.Query.Calendars;

				if (count != queryCalendars.Count)
					return false;

				for (int i = 0; i < count; i++)
				{
					if (calendars[i] != queryCalendars[i])
						return false;
				}

				// JJD 11/11/11 - TFS79534
				// If either of the providers or the local token have changed then return false
				if (_dateInfoProvider != null || _tzProvider != null)
				{
					XamScheduleDataManager dm = this._dateNavigator.DataManagerResolved;

					if ( _dateInfoProvider != null && _dateInfoProvider != dm.DateInfoProviderResolved )
						return false;

					if (_tzProvider != null)
					{
						if (_tzProvider != dm.TimeZoneInfoProviderResolved
							|| _tzProvider.LocalToken != _localToken)
							return false;
					}
				}

				return true;
			}

			#endregion //IsCompatible

			#region OnQuerySubObjectChanged

			private void OnQuerySubObjectChanged(ActivityQueryResult queryResult, object sender, string property, object extraInfo)
			{
				Debug.Assert(queryResult == this._queryResult);

				if (this._isDirty)
					return;

				if (!queryResult.IsComplete)
					return;

				if (sender is ReadOnlyNotifyCollection<ActivityBase>)
				{
					this._isDirty = true;
				}
				else
					if (sender == _queryResult)
					{
						switch (property)
						{
							case "IsComplete":
							case "IsCanceled":
								this._isDirty = true;
								break;
						}
					}
					else
					{
						ActivityBase activity = sender as ActivityBase;

						if (activity != null)
						{
							switch (property)
							{
								case "Start":
								case "StartTimeZoneId":
								case "End":
								case "EndTimeZoneId":
									this._isDirty = true;
									break;
							}
						}
					}

				if (this._isDirty)
					this._dateNavigator.PostActivityVerification();
			}

			#endregion //OnQuerySubObjectChanged

			#region ProcessQueryResults

			internal bool ProcessQueryResults()
			{
				if (!this._isDirty)
					return true;

				this._isDirty = false;

				if (_queryResult.IsCanceled)
					return false;

				if (_queryResult.IsComplete)
				{
					if (_daysWithActivity != null)
						_daysWithActivity.Clear();
					else
						_daysWithActivity = new HashSet<DateTime>();

					XamScheduleDataManager dm = this._dateNavigator.DataManagerResolved;

					// JJD 11/11/11 - TFS79534
					// Cache the providers so we can tell when they have changed
					//DateInfoProvider dip = dm.DateInfoProviderResolved;
					//TimeZoneInfoProvider provider = dm.TimeZoneInfoProviderResolved;
					//TimeZoneToken token = provider.LocalToken;
					_dateInfoProvider = dm.DateInfoProviderResolved;
					_tzProvider = dm.TimeZoneInfoProviderResolved;
					_localToken = _tzProvider.LocalToken;

					TimeSpan offset = new TimeSpan(-_logicalDayOffset.Ticks);

					TimeSpan duration = dm.LogicalDayDuration;

					foreach (ActivityBase activity in _queryResult.Activities)
					{
						DateTime dt = activity.GetStartLocal(_localToken);
						DateTime dtEnd = activity.GetEndLocal(_localToken);

						if (dtEnd != dt)
							dtEnd = ScheduleUtilities.GetNonInclusiveEnd(dtEnd);

						if (offset.Ticks != 0)
						{
							DateTime? temp = _dateInfoProvider.Add(dt, offset);

							if (temp.HasValue)
								dt = temp.Value;

							temp = _dateInfoProvider.Add(dtEnd, offset);

							if (temp.HasValue)
								dtEnd = temp.Value;

						}

						// strip off the time 
						dt = dt.Date;
						dtEnd = dtEnd.Date;

						if (_range.Start > dt)
							dt = _range.Start;

						// add all the days that this activity spans
						while (dt <= dtEnd && dt <= _range.End)
						{
							_daysWithActivity.Add(dt);
							dt = dt.AddDays(1);
						}
					}
				}
				return true;
			}

			#endregion //ProcessQueryResults

			#region ReleaseQuery

			internal void ReleaseQuery()
			{
				ScheduleUtilities.RemoveListener(_queryResult, _listener);

				if (_queryResult.IsComplete == false)
				{
					XamScheduleDataManager dm = this._dateNavigator.DataManagerResolved;

					if (dm != null)
						dm.CancelPendingOperation(_queryResult);
				}
			}

			#endregion //ReleaseQuery

			#endregion //Methods
		}

		#endregion //MonthActivity private class
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