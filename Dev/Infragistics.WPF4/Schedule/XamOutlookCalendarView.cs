using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Diagnostics;
using Infragistics.Controls.Schedules.Primitives;
using System.Windows.Input;
using System.Collections.Specialized;
using Infragistics.Collections;
using Infragistics.AutomationPeers;
using System.Windows.Automation.Peers;
using Infragistics.Controls.Editors;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// A custom control that uses the various calendar views to provide an Microsoft Outlook like interface
	/// </summary>
	/// <remarks>
	/// <p class="body">The XamOutlookCalendarView is a composite control that contains a <see cref="XamDayView"/>, 
	/// <see cref="XamMonthView"/> and <see cref="XamScheduleView"/> and switches between them based on the 
	/// <see cref="CurrentViewMode"/>. The CurrentViewMode can be set but it will also be changed by the control 
	/// based on interaction with its UI similar to how Microsoft Outlook works. For example clicking on the 
	/// week header when in Month view will switch to a Day view in week mode for that week.</p>
	/// <p class="body">The <see cref="DateNavigator"/> property is used to provide a reference to an object that 
	/// provides date information indicating the dates/view that should be displayed. Similarly as one interacts 
	/// with the XamOutlookCalendarView (via the UI or programatically), it will notify the DateNavigator of the 
	/// dates being displayed so that it may update its selection. This is similar to the relationship between the 
	/// Calendar View in Microsoft Outlook and the DateNavigator control that displays a compact view of one or 
	/// more months. The <see cref="XamDateNavigator"/> implements the <see cref="IOutlookDateNavigator"/> and 
	/// therefore may be used as the DateNavigator for a XamOutlookCalendarView.</p>
	/// <p class="body">The XamOutlookCalendarView exposes many of the same properties defined on the various 
	/// <see cref="ScheduleControlBase"/> derived controls. These controls are defined within its template with 
	/// specific names as defined by the TemplatePart attributes defined on the control similar to how other 
	/// controls define template requirements. The control will automatically bind the properties of those 
	/// controls to the properties that are exposed on the XamOutlookCalendarView.</p>
	/// <p class="body">The control also exposes properties to have it automatically switch between the 
	/// Schedule views and Day views based on <see cref="ResourceCalendar"/> instances being displayed 
	/// within the control. The <see cref="IsDayViewToScheduleViewSwitchEnabled"/> and 
	/// <see cref="IsScheduleViewToDayViewSwitchEnabled"/> properties are used to enable the functionality. 
	/// The <see cref="DayViewToScheduleViewSwitchThreshold"/> and <see cref="ScheduleViewToDayViewSwitchThreshold"/> 
	/// properties are used to control the threshold which must be met in order to automatically switch to the 
	/// corresponding view.</p>
	/// </remarks>
	/// <seealso cref="XamDayView"/>
	/// <seealso cref="XamScheduleView"/>
	/// <seealso cref="XamMonthView"/>

	[InfragisticsFeature(FeatureName = "XamOutlookCalendarView", Version = "11.1")]
	
	

	[TemplatePart(Name = PartDayView, Type = typeof(XamDayView))]
	[TemplatePart(Name = PartScheduleView, Type = typeof(XamScheduleView))]
	[TemplatePart(Name = PartMonthView, Type = typeof(XamMonthView))]
	public class XamOutlookCalendarView : Control
		, ICommandTarget
		, ISelectedActivityCollectionOwner
		, IScheduleControl



	{
		#region Member Variables

		private const string PartDayView = "DayView";
		private const string PartScheduleView = "ScheduleView";
		private const string PartMonthView = "MonthView";

		private InternalFlags _flags = 0;
		private OutlookCalendarViewMode? _oldViewMode = null;
		private OutlookCalendarViewMode _currentViewMode;

		private XamDayView _dayView;
		private XamScheduleView _scheduleView;
		private XamMonthView _monthView;
		private ScheduleControlBase _currentViewControl;
		private ReadOnlyNotifyCollection<CalendarGroupBase> _calendarGroupsResolved;
		private ObservableCollectionExtended<CalendarGroup> _calendarGroupsOverride;
		private SelectedActivityCollection _selectedActivities;

		private static readonly Binding _DefaultBrushProviderBinding;

		// AS 6/8/11 TFS76111
		private CalendarGroupCollection _sharedCalendarGroupsOverride;

		#endregion // Member Variables

		#region Constructor
		static XamOutlookCalendarView()
		{

			XamOutlookCalendarView.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamOutlookCalendarView), new FrameworkPropertyMetadata(typeof(XamOutlookCalendarView)));


			_DefaultBrushProviderBinding = PresentationUtilities.CreateBinding(
				new BindingPart
				{

					PathParameter = XamOutlookCalendarView.DataManagerProperty



				},
				new BindingPart
				{

					PathParameter = XamScheduleDataManager.ColorSchemeResolvedProperty



				},
				new BindingPart
				{

					PathParameter = CalendarColorScheme.DefaultBrushProviderProperty



				}
			);
			_DefaultBrushProviderBinding.RelativeSource = new RelativeSource(RelativeSourceMode.Self);
		}

		/// <summary>
		/// Initializes a new <see cref="XamOutlookCalendarView"/>
		/// </summary>
		public XamOutlookCalendarView()
		{
			#region Licensing/Trial

            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamOutlookCalendarView), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			#endregion // Licensing/Trial




			_sharedCalendarGroupsOverride = new CalendarGroupCollection(); // AS 6/8/11 TFS76111
			_selectedActivities = new SelectedActivityCollection(this);
			_calendarGroupsResolved = new ReadOnlyNotifyCollection<CalendarGroupBase>(new CalendarGroupBase[0]);
			_currentViewMode = this.CurrentViewMode;
			this.SetBinding(DefaultBrushProviderInternalProperty, _DefaultBrushProviderBinding);
			var be = this.GetBindingExpression(DefaultBrushProviderInternalProperty);
			this.Loaded += new RoutedEventHandler(OnLoaded);

			// AS 6/12/12 TFS111820
			CommandSourceManager.RegisterCommandTarget(this);
		}

		#endregion //Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			// get the controls
			this.InitializeControlPart(ref _dayView, PartDayView);
			this.InitializeControlPart(ref _scheduleView, PartScheduleView);
			this.InitializeControlPart(ref _monthView, PartMonthView);

			// set the working day source on each control
			this.InitializeWorkingDaySource();

			// update the view accordingly
			this.VerifyCurrentView();
		} 
		#endregion // OnApplyTemplate

        #region OnCreateAutomationPeer
        /// <summary>
        /// Returns an automation peer that exposes the <see cref="XamOutlookCalendarView"/> to UI Automation.
        /// </summary>
        /// <returns>A <see cref="XamOutlookCalendarViewAutomationPeer"/></returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamOutlookCalendarViewAutomationPeer(this);
        }
        #endregion // OnCreateAutomationPeer

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown( KeyEventArgs e )
		{
			base.OnKeyDown(e);

			if ( !e.Handled )
			{
				ProcessKeyDown(e);
			}
		}

		#endregion //OnKeyDown

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region ScheduleControlBase

		#region ActiveCalendar

		/// <summary>
		/// Identifies the <see cref="ActiveCalendar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActiveCalendarProperty = DependencyPropertyUtilities.Register("ActiveCalendar",
			typeof(ResourceCalendar), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnActiveCalendarChanged))
			);

		private static void OnActiveCalendarChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;

			// raise the event
			instance.OnActiveCalendarChanged(e.OldValue as ResourceCalendar, e.NewValue as ResourceCalendar);
		}

		/// <summary>
		/// Returns or sets the active <see cref="ResourceCalendar"/>
		/// </summary>
		/// <seealso cref="ActiveCalendarProperty"/>
		/// <seealso cref="ScheduleControlBase.ActiveCalendar"/>
		public ResourceCalendar ActiveCalendar
		{
			get
			{
				return (ResourceCalendar)this.GetValue(XamOutlookCalendarView.ActiveCalendarProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ActiveCalendarProperty, value);
			}
		}

		#endregion //ActiveCalendar

		#region AllowCalendarGroupResizing

		/// <summary>
		/// Identifies the <see cref="AllowCalendarGroupResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCalendarGroupResizingProperty = DependencyPropertyUtilities.Register("AllowCalendarGroupResizing",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a boolean indicating if the <see cref="PreferredCalendarGroupHorizontalExtent"/> and <see cref="PreferredCalendarGroupVerticalExtent"/> can be adjusted by the end user via the UI.
		/// </summary>
		/// <seealso cref="AllowCalendarGroupResizingProperty"/>
		/// <seealso cref="ScheduleControlBase.AllowCalendarGroupResizing"/>
		public bool AllowCalendarGroupResizing
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.AllowCalendarGroupResizingProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.AllowCalendarGroupResizingProperty, value);
			}
		}

		#endregion //AllowCalendarGroupResizing

		#region CalendarDisplayMode

		/// <summary>
		/// Identifies the <see cref="CalendarDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarDisplayModeProperty = DependencyPropertyUtilities.Register("CalendarDisplayMode",
			typeof(CalendarDisplayMode?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the preferred calendar display mode.
		/// </summary>
		/// <seealso cref="CalendarDisplayModeProperty"/>
		/// <seealso cref="ScheduleControlBase.CalendarDisplayMode"/>
		public CalendarDisplayMode? CalendarDisplayMode
		{
			get
			{
				return (CalendarDisplayMode?)this.GetValue(XamOutlookCalendarView.CalendarDisplayModeProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.CalendarDisplayModeProperty, value);
			}
		}
		#endregion //CalendarDisplayMode

		#region CalendarGroupsOverride
		/// <summary>
		/// Returns a modifiable collection of <see cref="CalendarGroup"/> instances that represent the groups in view in this control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The CalendarGroupsOverride is primarily meant to be used when you have multiple xamSchedule controls 
		/// associated with the same <see cref="XamScheduleDataManager"/> but you want this control instance to show a different 
		/// set of <see cref="ResourceCalendar"/> instances than those shown in a different xamSchedule control whose 
		/// <see cref="DataManager"/> is set to the same XamScheduleDataManager. To acheive an Outlook type behavior where all 
		/// the controls are displaying the same calendars and as the calendars are modified all the controls are affected you 
		/// would use the <see cref="XamScheduleDataManager.CalendarGroups"/> collection instead.</p>
		/// <p class="bote">If there are any CalendarGroup instances in this collection then those are the calendars 
		/// that will be displayed by the control. If the Count is 0, then the xamScheduleDataManager's CalendarGroups will be used. 
		/// If that too has a Count of 0, then the <see cref="Resource.PrimaryCalendar"/> of the <see cref="XamScheduleDataManager.CurrentUser"/> 
		/// will be displayed.</p>
		/// </remarks>
		/// <see cref="CalendarGroupsResolved"/>
		/// <see cref="ScheduleControlBase.CalendarGroupsOverride"/>
		/// <see cref="XamScheduleDataManager.CalendarGroups"/>
		/// <see cref="XamScheduleDataManager.CurrentUser"/>
		/// <see cref="XamScheduleDataManager.CurrentUserId"/>
		public ObservableCollectionExtended<CalendarGroup> CalendarGroupsOverride
		{
			get
			{
				if ( null == _calendarGroupsOverride )
				{
					_calendarGroupsOverride = new ObservableCollectionExtended<CalendarGroup>();
					_calendarGroupsOverride.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCalendarGroupsChanged);
				}

				return _calendarGroupsOverride;
			}
		}

		private void OnCalendarGroupsChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			// AS 6/8/11 TFS76111
			//this.InitializeCalendarGroupsOverride(_monthView);
			//this.InitializeCalendarGroupsOverride(_dayView);
			//this.InitializeCalendarGroupsOverride(_scheduleView);
			ScheduleUtilities.Reinitialize(_sharedCalendarGroupsOverride, this.CalendarGroupsOverride);
		}
		#endregion // CalendarGroupsOverride

		#region CalendarGroupsResolved
		/// <summary>
		/// Returns the CalendarGroupCollection that the control is using to determine the calendars that are displayed.
		/// </summary>
		/// <remarks>
		/// <p>By default a xamSchedule control will use the <see cref="XamScheduleDataManager.CalendarGroups"/> of the 
		/// associated <see cref="DataManager"/> and therefore all the controls associated with that manager will remain 
		/// in sync. However, one could add <see cref="CalendarGroup"/> instances to the <see cref="CalendarGroupsOverride"/> 
		/// to have this control instance manage its calendars independantly.</p>
		/// <p>The CalendarGroupsResolved returns the collection of <see cref="CalendarGroupBase"/> instances that the 
		/// control is using to determine the display. The collection is based upon the <see cref="CalendarDisplayMode"/> 
		/// and the <see cref="CalendarGroupCollection"/> that the control is using.</p>
		/// </remarks>
		/// <seealso cref="ScheduleControlBase.CalendarGroupsResolved"/>
		public ReadOnlyNotifyCollection<CalendarGroupBase> CalendarGroupsResolved
		{
			get
			{
				return _calendarGroupsResolved;
			}
		}
		#endregion // CalendarGroupsResolved

		#region DataManager

		/// <summary>
		/// Identifies the <see cref="DataManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataManagerProperty = DependencyPropertyUtilities.Register("DataManager",
			typeof(XamScheduleDataManager), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDataManagerChanged))
			);

		private static void OnDataManagerChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView view = d as XamOutlookCalendarView;
			XamScheduleDataManager newValue = e.NewValue as XamScheduleDataManager;
			XamScheduleDataManager oldValue = e.OldValue as XamScheduleDataManager;

			if (null != oldValue)
				oldValue.Controls.Remove(view);

			// AS 6/8/11 TFS76111
			view._sharedCalendarGroupsOverride.Owner = newValue;

			if (null != newValue)
				newValue.Controls.Add(view);
		}

		/// <summary>
		/// Returns or sets the object that provides the activity and resource information that will be displayed by the control.
		/// </summary>
		/// <seealso cref="DataManagerProperty"/>
		/// <seealso cref="ScheduleControlBase.DataManager"/>
		public XamScheduleDataManager DataManager
		{
			get
			{
				return (XamScheduleDataManager)this.GetValue(XamOutlookCalendarView.DataManagerProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.DataManagerProperty, value);
			}
		}
		#endregion //DataManager

		#region DefaultBrushProvider

		private static readonly DependencyPropertyKey DefaultBrushProviderPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DefaultBrushProvider",
			typeof(CalendarBrushProvider), typeof(XamOutlookCalendarView),
			null,
			null
			);

		/// <summary>
		/// Identifies the read-only <see cref="DefaultBrushProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultBrushProviderProperty = DefaultBrushProviderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the default brush provider for elements that don't have a specific <see cref="ResourceCalendar"/> context (read-only)
		/// </summary>
		/// <seealso cref="DefaultBrushProviderProperty"/>
		/// <seealso cref="ScheduleControlBase.DefaultBrushProvider"/>
		public CalendarBrushProvider DefaultBrushProvider
		{
			get
			{
				return (CalendarBrushProvider)this.GetValue(XamOutlookCalendarView.DefaultBrushProviderProperty);
			}
			internal set
			{
				this.SetValue(XamOutlookCalendarView.DefaultBrushProviderPropertyKey, value);
			}
		}

		#endregion //DefaultBrushProvider

		// AS 3/14/12 Touch Support
		#region IsTouchSupportEnabled

		/// <summary>
		/// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
			   typeof(bool), typeof(XamOutlookCalendarView),
			   DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			   );

		/// <summary>
		/// Returns or sets whtner touch support is enabled for this control
		/// </summary>
		/// <seealso cref="IsTouchSupportEnabledProperty"/>
		public bool IsTouchSupportEnabled
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.IsTouchSupportEnabledProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.IsTouchSupportEnabledProperty, value);
			}
		}

		#endregion //IsTouchSupportEnabled

		#region MinCalendarGroupHorizontalExtent

		/// <summary>
		/// Identifies the <see cref="MinCalendarGroupHorizontalExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinCalendarGroupHorizontalExtentProperty = DependencyPropertyUtilities.Register("MinCalendarGroupHorizontalExtent",
			typeof(double?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the minimum width for horizontally arranged CalendarGroup instances within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">This indicates the minimum width for the CalendarGroup instances within the controls that arrange their groups 
		/// horizontally (e.g. XamMonthView and XamDayView). If the available space is less than the minimum extent, the available space 
		/// will be used so that a group is not wider than the available area. This is required since CalendarGroups are scrolled 
		/// by group and not pixel based.</p>
		/// </remarks>
		/// <seealso cref="MinCalendarGroupHorizontalExtentProperty"/>
		/// <seealso cref="MinCalendarGroupVerticalExtent"/>
		/// <seealso cref="ScheduleControlBase.MinCalendarGroupExtent"/>
		public double? MinCalendarGroupHorizontalExtent
		{
			get
			{
				return (double?)this.GetValue(XamOutlookCalendarView.MinCalendarGroupHorizontalExtentProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.MinCalendarGroupHorizontalExtentProperty, value);
			}
		}

		#endregion //MinCalendarGroupHorizontalExtent

		#region MinCalendarGroupVerticalExtent

		/// <summary>
		/// Identifies the <see cref="MinCalendarGroupVerticalExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinCalendarGroupVerticalExtentProperty = DependencyPropertyUtilities.Register("MinCalendarGroupVerticalExtent",
			typeof(double?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the minimum height for vertically arranged CalendarGroup instances within the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">This indicates the minimum height for the CalendarGroup instances within the controls that arrange their groups 
		/// vertically (e.g. XamScheduleView). If the available space is less than the minimum extent, the available space 
		/// will be used so that a group is not wider than the available area. This is required since CalendarGroups are scrolled 
		/// by group and not pixel based.</p>
		/// </remarks>
		/// <seealso cref="MinCalendarGroupVerticalExtentProperty"/>
		/// <seealso cref="MinCalendarGroupHorizontalExtent"/>
		/// <seealso cref="ScheduleControlBase.MinCalendarGroupExtent"/>
		public double? MinCalendarGroupVerticalExtent
		{
			get
			{
				return (double?)this.GetValue(XamOutlookCalendarView.MinCalendarGroupVerticalExtentProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.MinCalendarGroupVerticalExtentProperty, value);
			}
		}

		#endregion //MinCalendarGroupVerticalExtent

		#region PreferredCalendarGroupVerticalExtent

		/// <summary>
		/// Identifies the <see cref="PreferredCalendarGroupVerticalExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredCalendarGroupVerticalExtentProperty = DependencyPropertyUtilities.Register("PreferredCalendarGroupVerticalExtent",
			typeof(double), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN)
			);

		/// <summary>
		/// Returns or sets the preferred height for vertically arranged CalendarGroups in the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The PreferredCalendarGroupHorizontalExtent indicates the height for the CalendarGroup instances within the controls that 
		/// arrange their groups vertically (e.g. XamScheduleView). If the available space is less than the preferred extent, the available space 
		/// will be used so that a group is not wider than the available area. This is required since CalendarGroups are scrolled by group and not 
		/// pixel based. If the sum of the space required to show all the groups is less than the available space, the groups will 
		/// be extended to fill the available space. Also, if <see cref="AllowCalendarGroupResizing"/> is enabled, this property will 
		/// be updated as the user drags the resizer bar at the right edge of the group.</p>
		/// </remarks>
		/// <seealso cref="PreferredCalendarGroupVerticalExtentProperty"/>
		/// <seealso cref="PreferredCalendarGroupHorizontalExtent"/>
		/// <seealso cref="ScheduleControlBase.PreferredCalendarGroupExtent"/>
		public double PreferredCalendarGroupVerticalExtent
		{
			get
			{
				return (double)this.GetValue(XamOutlookCalendarView.PreferredCalendarGroupVerticalExtentProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.PreferredCalendarGroupVerticalExtentProperty, value);
			}
		}

		#endregion //PreferredCalendarGroupVerticalExtent

		#region PreferredCalendarGroupHorizontalExtent

		/// <summary>
		/// Identifies the <see cref="PreferredCalendarGroupHorizontalExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredCalendarGroupHorizontalExtentProperty = DependencyPropertyUtilities.Register("PreferredCalendarGroupHorizontalExtent",
			typeof(double), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN)
			);

		/// <summary>
		/// Returns or sets the preferred width for horizontally arranged CalendarGroups in the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The PreferredCalendarGroupHorizontalExtent indicates the width for the CalendarGroup instances within the controls that 
		/// arrange their groups horizontally (e.g. XamMonthView and XamDayView). If the available space is less than the preferred extent, the 
		/// available space will be used so that a group is not wider than the available area. This is required since CalendarGroups are scrolled 
		/// by group and not pixel based. If the sum of the space required to show all the groups is less than the available space, the groups will 
		/// be extended to fill the available space. Also, if <see cref="AllowCalendarGroupResizing"/> is enabled, this property will 
		/// be updated as the user drags the resizer bar at the right edge of the group.</p>
		/// </remarks>
		/// <seealso cref="PreferredCalendarGroupHorizontalExtentProperty"/>
		/// <seealso cref="PreferredCalendarGroupVerticalExtent"/>
		/// <seealso cref="ScheduleControlBase.PreferredCalendarGroupExtent"/>
		public double PreferredCalendarGroupHorizontalExtent
		{
			get
			{
				return (double)this.GetValue(XamOutlookCalendarView.PreferredCalendarGroupHorizontalExtentProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.PreferredCalendarGroupHorizontalExtentProperty, value);
			}
		}

		#endregion //PreferredCalendarGroupHorizontalExtent

		#region SelectedActivities

		/// <summary>
		/// Returns a collection of activities that are selected.
		/// </summary>
		/// <seealso cref="ScheduleControlBase.SelectedActivities"/>
		public SelectedActivityCollection SelectedActivities { get { return _selectedActivities; } }

		#endregion //SelectedActivities	

		#region SelectedTimeRange
		/// <summary>
		/// Identifies the <see cref="SelectedTimeRange"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedTimeRangeProperty = DependencyPropertyUtilities.Register("SelectedTimeRange",
			typeof(DateRange?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSelectedTimeRangeChanged))
			);

		private static void OnSelectedTimeRangeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			var instance = (XamOutlookCalendarView)d;

			DateRange? oldValue = (DateRange?)e.OldValue;
			DateRange? newValue = (DateRange?)e.NewValue;

			// raise the event
			instance.OnSelectedTimeRangeChanged(oldValue, newValue);
		} 

		/// <summary>
		/// Returns or sets the selected time range of the control
		/// </summary>
		/// <seealso cref="SelectedTimeRangeProperty"/>
		/// <seealso cref="ScheduleControlBase.SelectedTimeRange"/>
		public DateRange? SelectedTimeRange
		{
			get
			{
				return (DateRange?)this.GetValue(XamOutlookCalendarView.SelectedTimeRangeProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.SelectedTimeRangeProperty, value);
			}
		}
		#endregion // SelectedTimeRange

		#region ShowCalendarCloseButton

		/// <summary>
		/// Identifies the <see cref="ShowCalendarCloseButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowCalendarCloseButtonProperty = DependencyPropertyUtilities.Register("ShowCalendarCloseButton",
			typeof(bool?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets whether a close button will be displayed in the <see cref="CalendarHeader"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">This property determines whether the close button is displayed within the <see cref="CalendarHeader"/>. By default 
		/// this property is set to null and the property value used is resolved based on the <see cref="ScheduleSettings.AllowCalendarClosing"/>. 
		/// If that property is set to false then the close button will be hidden. Otherwise it is up to the control as to whether it will display 
		/// the button. <see cref="XamScheduleView"/>, for example, will not display the button by default.</p>
		/// <b class="note">Note: The enabled state is based upon the <see cref="ScheduleSettings.AllowCalendarClosing"/>. So if ShowCalendarCloseButton is 
		/// set to true and AllowCalendarClosing is set to false, the button will be displayed but will be disabled.</b>
		/// </remarks>
		/// <seealso cref="ShowCalendarCloseButtonProperty"/>
		/// <seealso cref="CalendarHeader"/>
		/// <seealso cref="CalendarHeader.CloseButtonVisibility"/>
		/// <seealso cref="ScheduleSettings.AllowCalendarClosing"/>
		/// <seealso cref="ScheduleControlBase.ShowCalendarCloseButton"/>
		public bool? ShowCalendarCloseButton
		{
			get
			{
				return (bool?)this.GetValue(XamOutlookCalendarView.ShowCalendarCloseButtonProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ShowCalendarCloseButtonProperty, value);
			}
		}

		#endregion //ShowCalendarCloseButton

		#region ShowCalendarOverlayButton

		/// <summary>
		/// Identifies the <see cref="ShowCalendarOverlayButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowCalendarOverlayButtonProperty = DependencyPropertyUtilities.Register("ShowCalendarOverlayButton",
			typeof(bool?), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets whether an overlay button will be displayed in the <see cref="CalendarHeader"/>
		/// </summary>
		/// <seealso cref="ShowCalendarOverlayButtonProperty"/>
		/// <seealso cref="CalendarHeader"/>
		/// <seealso cref="CalendarHeader.OverlayButtonVisibility"/>
		/// <seealso cref="ScheduleControlBase.ShowCalendarOverlayButton"/>
		public bool? ShowCalendarOverlayButton
		{
			get
			{
				return (bool?)this.GetValue(XamOutlookCalendarView.ShowCalendarOverlayButtonProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ShowCalendarOverlayButtonProperty, value);
			}
		}

		#endregion //ShowCalendarOverlayButton

		#endregion // ScheduleControlBase

		#region ScheduleTimeControlBase

		#region CurrentTimeIndicatorVisibility

		/// <summary>
		/// Identifies the <see cref="CurrentTimeIndicatorVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentTimeIndicatorVisibilityProperty = DependencyPropertyUtilities.Register("CurrentTimeIndicatorVisibility",
			typeof(Visibility), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox)
			);

		/// <summary>
		/// Returns or sets a value indicating whether an indicator identifying the current time is displayed within the time slot area.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="CurrentTimeIndicatorVisibilityProperty"/>
		/// <seealso cref="ScheduleTimeControlBase.CurrentTimeIndicatorVisibility"/>
		public Visibility CurrentTimeIndicatorVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamOutlookCalendarView.CurrentTimeIndicatorVisibilityProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.CurrentTimeIndicatorVisibilityProperty, value);
			}
		}

		#endregion //CurrentTimeIndicatorVisibility

		#region PrimaryTimeZoneLabel

		/// <summary>
		/// Identifies the <see cref="PrimaryTimeZoneLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PrimaryTimeZoneLabelProperty = DependencyPropertyUtilities.Register("PrimaryTimeZoneLabel",
			typeof(string), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the label that is displayed by the time slot area for the current time zone.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="PrimaryTimeZoneLabelProperty"/>
		/// <seealso cref="ScheduleTimeControlBase.PrimaryTimeZoneLabel"/>
		public string PrimaryTimeZoneLabel
		{
			get
			{
				return (string)this.GetValue(XamOutlookCalendarView.PrimaryTimeZoneLabelProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.PrimaryTimeZoneLabelProperty, value);
			}
		}

		#endregion //PrimaryTimeZoneLabel

		#region SecondaryTimeZoneLabel

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneLabel"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneLabelProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneLabel",
			typeof(string), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the label that is displayed by the time slot area for the alternate time zone.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="SecondaryTimeZoneLabelProperty"/>
		/// <seealso cref="SecondaryTimeZoneId"/>
		/// <seealso cref="SecondaryTimeZoneVisibility"/>
		/// <seealso cref="ScheduleTimeControlBase.SecondaryTimeZoneLabel"/>
		public string SecondaryTimeZoneLabel
		{
			get
			{
				return (string)this.GetValue(XamOutlookCalendarView.SecondaryTimeZoneLabelProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.SecondaryTimeZoneLabelProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneLabel

		#region SecondaryTimeZoneId

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneId"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneIdProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneId",
			typeof(string), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Returns or sets the id for the additional time zone.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="SecondaryTimeZoneIdProperty"/>
		/// <seealso cref="SecondaryTimeZoneLabel"/>
		/// <seealso cref="SecondaryTimeZoneVisibility"/>
		/// <seealso cref="ScheduleDataConnectorBase.TimeZoneInfoProvider"/>
		/// <seealso cref="ScheduleDataConnectorBase.TimeZoneInfoProviderResolved"/>
		/// <seealso cref="ScheduleTimeControlBase.SecondaryTimeZoneId"/>
		public string SecondaryTimeZoneId
		{
			get
			{
				return (string)this.GetValue(XamOutlookCalendarView.SecondaryTimeZoneIdProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.SecondaryTimeZoneIdProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneId

		#region SecondaryTimeZoneVisibility

		/// <summary>
		/// Identifies the <see cref="SecondaryTimeZoneVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SecondaryTimeZoneVisibilityProperty = DependencyPropertyUtilities.Register("SecondaryTimeZoneVisibility",
			typeof(Visibility), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityCollapsedBox)
			);

		/// <summary>
		/// Returns or sets a value indicating whether the time slot area for the additional time zone should be displayed.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="SecondaryTimeZoneVisibilityProperty"/>
		/// <seealso cref="SecondaryTimeZoneLabel"/>
		/// <seealso cref="SecondaryTimeZoneId"/>
		/// <seealso cref="ScheduleTimeControlBase.SecondaryTimeZoneVisibility"/>
		public Visibility SecondaryTimeZoneVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamOutlookCalendarView.SecondaryTimeZoneVisibilityProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.SecondaryTimeZoneVisibilityProperty, value);
			}
		}

		#endregion //SecondaryTimeZoneVisibility

		#region ShowWorkingHoursOnly

		/// <summary>
		/// Identifies the <see cref="ShowWorkingHoursOnly"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWorkingHoursOnlyProperty = DependencyPropertyUtilities.Register("ShowWorkingHoursOnly",
			typeof(Boolean), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a boolean indicating whether the control should filter out non-working hour timeslots.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="ShowWorkingHoursOnlyProperty"/>
		/// <seealso cref="ScheduleTimeControlBase.ShowWorkingHoursOnly"/>
		public Boolean ShowWorkingHoursOnly
		{
			get
			{
				return (Boolean)this.GetValue(XamOutlookCalendarView.ShowWorkingHoursOnlyProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ShowWorkingHoursOnlyProperty, value);
			}
		}

		#endregion //ShowWorkingHoursOnly

		#region TimeslotInterval

		/// <summary>
		/// Identifies the <see cref="TimeslotInterval"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeslotIntervalProperty = DependencyPropertyUtilities.Register("TimeslotInterval",
			typeof(TimeSpan), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(TimeSpan.FromMinutes(15d))
			);

		/// <summary>
		/// Returns or sets a value indicating the duration of each time slot.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="TimeslotIntervalProperty"/>
		/// <seealso cref="ScheduleTimeControlBase.TimeslotInterval"/>
		public TimeSpan TimeslotInterval
		{
			get
			{
				return (TimeSpan)this.GetValue(XamOutlookCalendarView.TimeslotIntervalProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.TimeslotIntervalProperty, value);
			}
		}

		#endregion //TimeslotInterval

		#region WorkingHoursSource

		/// <summary>
		/// Identifies the <see cref="WorkingHoursSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WorkingHoursSourceProperty = DependencyPropertyUtilities.Register("WorkingHoursSource",
			typeof(WorkingHoursSource), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(WorkingHoursSource.CurrentUser)
			);

		/// <summary>
		/// Returns or sets a value indicating what object is considered when calculating the working hours.
		/// </summary>
		/// <remarks>
		/// <p class="note">This property does not affect what is used to calculate whether a timeslot is considered a 
		/// working hour or not. That calculation is based on the <see cref="ResourceCalendar.OwningResource"/> for the <see cref="CalendarGroupBase.SelectedCalendar"/>  of the containing 
		/// <see cref="CalendarGroupBase"/>.</p>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="ScheduleTimeControlBase"/> derived controls (e.g. XamScheduleView and XamDayView) contained within the template.</p>
		/// </remarks>
		/// <seealso cref="WorkingHoursSourceProperty"/>
		/// <seealso cref="WeekDisplayMode"/>
		/// <seealso cref="ShowWorkingHoursOnly"/>
		/// <seealso cref="ScheduleTimeControlBase.WorkingHoursSource"/>
		public WorkingHoursSource WorkingHoursSource
		{
			get
			{
				return (WorkingHoursSource)this.GetValue(XamOutlookCalendarView.WorkingHoursSourceProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.WorkingHoursSourceProperty, value);
			}
		}

		#endregion //WorkingHoursSource

		#endregion // ScheduleTimeControlBase

		#region XamDayView

		#region AllowMultiDayActivityAreaResizing

		/// <summary>
		/// Identifies the <see cref="AllowMultiDayActivityAreaResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowMultiDayActivityAreaResizingProperty = DependencyPropertyUtilities.Register("AllowMultiDayActivityAreaResizing",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a boolean indicating if the end user is allowed to resize the <see cref="MultiDayActivityArea"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="XamDayView"/> contained within the template.</p>
		/// </remarks>
		/// <seealso cref="AllowMultiDayActivityAreaResizingProperty"/>
		/// <seealso cref="XamDayView.AllowMultiDayActivityAreaResizing"/>
		public bool AllowMultiDayActivityAreaResizing
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.AllowMultiDayActivityAreaResizingProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.AllowMultiDayActivityAreaResizingProperty, value);
			}
		}

		#endregion //AllowMultiDayActivityAreaResizing

		#region MultiDayActivityAreaHeight

		/// <summary>
		/// Identifies the <see cref="MultiDayActivityAreaHeight"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MultiDayActivityAreaHeightProperty = DependencyPropertyUtilities.Register("MultiDayActivityAreaHeight",
			typeof(double), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN)
			);

		/// <summary>
		/// Returns or sets the height of the area containing the activities that are 24 hours or longer.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="XamDayView"/> contained within the template.</p>
		/// </remarks>
		/// <seealso cref="MultiDayActivityAreaHeightProperty"/>
		/// <seealso cref="XamDayView.MultiDayActivityAreaHeight"/>
		public double MultiDayActivityAreaHeight
		{
			get
			{
				return (double)this.GetValue(XamOutlookCalendarView.MultiDayActivityAreaHeightProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.MultiDayActivityAreaHeightProperty, value);
			}
		}

		#endregion //MultiDayActivityAreaHeight

		// AS 5/8/12 TFS108279
		#region TimeslotGutterAreaWidth

		/// <summary>
		/// Identifies the <see cref="TimeslotGutterAreaWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeslotGutterAreaWidthProperty = DependencyPropertyUtilities.Register("TimeslotGutterAreaWidth",
			typeof(double), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(10d)
			);

		/// <summary>
		/// Returns or sets the amount of horizontal space that is reserved within the vertically arranged XamDayView timeslots.
		/// </summary>
		/// <seealso cref="TimeslotGutterAreaWidthProperty"/>
		/// <seealso cref="XamDayView.TimeslotGutterAreaWidth"/>
		public double TimeslotGutterAreaWidth
		{
			get
			{
				return (double)this.GetValue(XamOutlookCalendarView.TimeslotGutterAreaWidthProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.TimeslotGutterAreaWidthProperty, value);
			}
		}

		#endregion //TimeslotGutterAreaWidth

		#endregion //XamDayView

		#region XamMonthView

		#region ShowWeekNumbers

		/// <summary>
		/// Identifies the <see cref="ShowWeekNumbers"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowWeekNumbersProperty = DependencyPropertyUtilities.Register("ShowWeekNumbers",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a value indicating whether week numbers should be displayed in the week headers instead of the date range.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="XamMonthView"/> contained within the template.</p>
		/// </remarks>
		/// <seealso cref="ShowWeekNumbersProperty"/>
		/// <seealso cref="XamMonthView.ShowWeekNumbers"/>
		public bool ShowWeekNumbers
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.ShowWeekNumbersProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ShowWeekNumbersProperty, value);
			}
		}

		#endregion //ShowWeekNumbers

		#endregion // XamMonthView

		#region XamScheduleView

		#region AllowCalendarHeaderAreaResizing

		/// <summary>
		/// Identifies the <see cref="AllowCalendarHeaderAreaResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCalendarHeaderAreaResizingProperty = DependencyPropertyUtilities.Register("AllowCalendarHeaderAreaResizing",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox)
			);

		/// <summary>
		/// Returns or sets a boolean indicating if the end user is allowed to resize the XamScheduleView calendar header area.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="XamScheduleView"/> contained within the template. The controls 
		/// that arrange their groups horizontally (e.g. XamMonthView and XamDayView) are not affected by this property.</p>
		/// </remarks>
		/// <seealso cref="AllowCalendarHeaderAreaResizingProperty"/>
		/// <seealso cref="XamScheduleView.AllowCalendarHeaderAreaResizing"/>
		public bool AllowCalendarHeaderAreaResizing
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.AllowCalendarHeaderAreaResizingProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.AllowCalendarHeaderAreaResizingProperty, value);
			}
		}

		#endregion //AllowCalendarHeaderAreaResizing

		#region CalendarHeaderAreaWidth

		/// <summary>
		/// Identifies the <see cref="CalendarHeaderAreaWidth"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarHeaderAreaWidthProperty = DependencyPropertyUtilities.Register("CalendarHeaderAreaWidth",
			typeof(double), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(double.NaN)
			);

		/// <summary>
		/// Returns or sets the width of the area containing the <see cref="CalendarHeader"/> elements
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This property only affects the <see cref="XamScheduleView"/> contained within the template. The controls 
		/// that arrange their groups horizontally (e.g. XamMonthView and XamDayView) are not affected by this property.</p>
		/// </remarks>
		/// <seealso cref="CalendarHeaderAreaWidthProperty"/>
		/// <seealso cref="XamScheduleView.CalendarHeaderAreaWidth"/>
		public double CalendarHeaderAreaWidth
		{
			get
			{
				return (double)this.GetValue(XamOutlookCalendarView.CalendarHeaderAreaWidthProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.CalendarHeaderAreaWidthProperty, value);
			}
		}

		#endregion //CalendarHeaderAreaWidth

		#endregion //XamScheduleView

		#region CurrentViewMode

		/// <summary>
		/// Identifies the <see cref="CurrentViewMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentViewModeProperty = DependencyPropertyUtilities.Register("CurrentViewMode",
			typeof(OutlookCalendarViewMode), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(OutlookCalendarViewMode.DayViewDay, new PropertyChangedCallback(OnCurrentViewModeChanged))
			);

		private static void OnCurrentViewModeChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;
			instance._currentViewMode = (OutlookCalendarViewMode)e.NewValue;

			if (!instance.IsInitializing)
				instance.VerifyCurrentView();

            var oldMode = (OutlookCalendarViewMode)e.OldValue;
            var newMode = (OutlookCalendarViewMode)e.NewValue;

			instance.OnCurrentViewModeChanged(oldMode, newMode);

            var peer = FrameworkElementAutomationPeer.FromElement(instance) as XamOutlookCalendarViewAutomationPeer;

            if (null != peer)
                peer.RaiseCurrentViewPropertyChangedEvent(oldMode, newMode);
		}

		/// <summary>
		/// Returns or sets the current view which indicates which control and the settings of the control that are displayed.
		/// </summary>
		/// <seealso cref="CurrentViewModeProperty"/>
		public OutlookCalendarViewMode CurrentViewMode
		{
			get
			{
				return _currentViewMode;
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.CurrentViewModeProperty, value);
			}
		}

		#endregion //CurrentViewMode

		#region CurrentViewDateRange

		private static readonly DependencyPropertyKey CurrentViewDateRangePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CurrentViewDateRange",
			typeof(DateRange?), typeof(XamOutlookCalendarView), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="CurrentViewDateRange"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentViewDateRangeProperty = CurrentViewDateRangePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the date range that encompasses the <see cref="ScheduleControlBase.VisibleDates"/> of the <see cref="CurrentViewControl"/>.
		/// </summary>
		/// <seealso cref="CurrentViewDateRangeProperty"/>
		public DateRange? CurrentViewDateRange
		{
			get
			{
				return (DateRange?)this.GetValue(XamOutlookCalendarView.CurrentViewDateRangeProperty);
			}
			private set
			{
				this.SetValue(XamOutlookCalendarView.CurrentViewDateRangePropertyKey, value);
			}
		}

		#endregion //CurrentViewDateRange

		#region CurrentViewDateRangeText

		private static readonly DependencyPropertyKey CurrentViewDateRangeTextPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CurrentViewDateRangeText",
			typeof(string), typeof(XamOutlookCalendarView), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="CurrentViewDateRangeText"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CurrentViewDateRangeTextProperty = CurrentViewDateRangeTextPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a string representation of the <see cref="CurrentViewDateRange"/>
		/// </summary>
		/// <seealso cref="CurrentViewDateRangeTextProperty"/>
		public string CurrentViewDateRangeText
		{
			get
			{
				return (string)this.GetValue(XamOutlookCalendarView.CurrentViewDateRangeTextProperty);
			}
			private set
			{
				this.SetValue(XamOutlookCalendarView.CurrentViewDateRangeTextPropertyKey, value);
			}
		}

		#endregion //CurrentViewDateRangeText

		#region DateNavigator

		/// <summary>
		/// Identifies the <see cref="DateNavigator"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateNavigatorProperty = DependencyPropertyUtilities.Register("DateNavigator",
			typeof(IOutlookDateNavigator), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDateNavigatorChanged))
			);

		private static void OnDateNavigatorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;

			instance.OnDateNavigatorChanged(e.OldValue as IOutlookDateNavigator, e.NewValue as IOutlookDateNavigator);
		}

		private void OnDateNavigatorChanged( IOutlookDateNavigator oldValue, IOutlookDateNavigator newValue )
		{
			if ( oldValue != null )
				oldValue.SelectedDatesChanged -= OnDateNavigatorSelectionChanged;

			if ( newValue != null )
				newValue.SelectedDatesChanged += OnDateNavigatorSelectionChanged;
		}

		private void OnDateNavigatorSelectionChanged( object sender, SelectedDatesChangedEventArgs e )
		{
			this.InitializeFromDateNavigator(e);
		}
		/// <summary>
		/// Returns or sets the object that provides and receives notifications regarding selected dates.
		/// </summary>
		/// <remarks>
		/// <p class="body">The DateNavigator is similar to that of the Date Navigator within Outlook. As the user interacts 
		/// with that calendar selecting days, the Calendar View of outlook changes accordingly to show those dates and change 
		/// its view as needed. Similarly as the end user changes or navigates through the Calendar View, the selected dates 
		/// in the DateNavigator are updated accordingly.</p>
		/// <p class="body">While one may provide their own implementation for the <see cref="IOutlookDateNavigator"/> interface, 
		/// the most common scenario will be to use a <see cref="XamDateNavigator"/> which implements this interface. One would 
		/// put an instance of that control in their UI and then bind this property to that element (e.g. using a Binding 
		/// with the ElementName set).</p>
		/// <p class="body">The xamOutlookCalendarView will listen to the <see cref="IOutlookDateNavigator.SelectedDatesChanged"/> of the 
		/// DateNavigator and will update its UI based upon the changes. For example, when the dates that make up a specific 
		/// week are selected, the control will change its <see cref="CurrentViewMode"/> to one of the WeekMode views (e.g. 
		/// DayViewWeekMode or ScheduleViewWeekMode) depending upon the current value of this property. Similarly as the 
		/// state of the control changes (e.g. the CurrentViewMode is set - directly or implicitly via the keyboard interactions 
		/// or commands) or as the user navigates the view to other dates, the control will notify the DateNavigator of the changes 
		/// so that it may update its information so the two controls may be in sync.</p>
		/// <p class="note"><b>Note:</b> The XamOutlookCalendarView does not attempt to contain or reparent the object that is set 
		/// as the value of the DateNavigator property. If this interface is implemented by a control such as XamDateNavigator then 
		/// one would put an instance of that control somewhere within the UI of their application and then set this property to 
		/// that control.</p>
		/// </remarks>
		/// <seealso cref="XamDateNavigator"/>
		/// <seealso cref="DateNavigatorProperty"/>
		public IOutlookDateNavigator DateNavigator
		{
			get
			{
				return (IOutlookDateNavigator)this.GetValue(XamOutlookCalendarView.DateNavigatorProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.DateNavigatorProperty, value);
			}
		}

		#endregion //DateNavigator

		#region DayViewToScheduleViewSwitchThreshold

		/// <summary>
		/// Identifies the <see cref="DayViewToScheduleViewSwitchThreshold"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DayViewToScheduleViewSwitchThresholdProperty = DependencyPropertyUtilities.Register("DayViewToScheduleViewSwitchThreshold",
			typeof(int), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(5)
			);

		/// <summary>
		/// Returns or sets when the <see cref="CurrentViewMode"/> is changed from DayView to ScheduleView based on a change in <see cref="ScheduleControlBase.VisibleCalendarCount"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">When enabled (<see cref="IsDayViewToScheduleViewSwitchEnabled"/>) the <see cref="CurrentViewMode"/> will be changed to one of the 
		/// ScheduleView related views when the view is one of the Day views and the <see cref="ScheduleControlBase.VisibleCalendarCount"/> of the 
		/// <see cref="XamDayView"/> goes from below this property's value to the property's value or higher.</p>
		/// </remarks>
		/// <seealso cref="DayViewToScheduleViewSwitchThresholdProperty"/>
		/// <seealso cref="IsDayViewToScheduleViewSwitchEnabled"/>
		/// <seealso cref="ScheduleViewToDayViewSwitchThreshold"/>
		/// <seealso cref="IsScheduleViewToDayViewSwitchEnabled"/>
		public int DayViewToScheduleViewSwitchThreshold
		{
			get
			{
				return (int)this.GetValue(XamOutlookCalendarView.DayViewToScheduleViewSwitchThresholdProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.DayViewToScheduleViewSwitchThresholdProperty, value);
			}
		}

		#endregion //DayViewToScheduleViewSwitchThreshold

		#region HeaderVisibility

		/// <summary>
		/// Identifies the <see cref="HeaderVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderVisibilityProperty = DependencyPropertyUtilities.Register("HeaderVisibility",
			typeof(Visibility), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.VisibilityVisibleBox)
			);

		/// <summary>
		/// Returns or sets a value that determines the visibility of the header area containing the date range and navigation buttons.
		/// </summary>
		/// <seealso cref="HeaderVisibilityProperty"/>
		public Visibility HeaderVisibility
		{
			get
			{
				return (Visibility)this.GetValue(XamOutlookCalendarView.HeaderVisibilityProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.HeaderVisibilityProperty, value);
			}
		}

		#endregion //HeaderVisibility

		#region IsDayViewToScheduleViewSwitchEnabled

		/// <summary>
		/// Identifies the <see cref="IsDayViewToScheduleViewSwitchEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsDayViewToScheduleViewSwitchEnabledProperty = DependencyPropertyUtilities.Register("IsDayViewToScheduleViewSwitchEnabled",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Returns or sets whether the ability to switch from Day view to Schedule view based on the number of visible calendars compared to the <see cref="DayViewToScheduleViewSwitchThreshold"/> is enabled.
		/// </summary>
		/// <seealso cref="IsDayViewToScheduleViewSwitchEnabledProperty"/>
		/// <seealso cref="DayViewToScheduleViewSwitchThreshold"/>
		/// <seealso cref="ScheduleViewToDayViewSwitchThreshold"/>
		/// <seealso cref="IsScheduleViewToDayViewSwitchEnabled"/>
		public bool IsDayViewToScheduleViewSwitchEnabled
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.IsDayViewToScheduleViewSwitchEnabledProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.IsDayViewToScheduleViewSwitchEnabledProperty, value);
			}
		}

		#endregion //IsDayViewToScheduleViewSwitchEnabled

		#region IsScheduleViewToDayViewSwitchEnabled

		/// <summary>
		/// Identifies the <see cref="IsScheduleViewToDayViewSwitchEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsScheduleViewToDayViewSwitchEnabledProperty = DependencyPropertyUtilities.Register("IsScheduleViewToDayViewSwitchEnabled",
			typeof(bool), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox)
			);

		/// <summary>
		/// Returns or sets whether the ability to switch from Schedule view to Day view based on the number of visible calendars compared to the <see cref="ScheduleViewToDayViewSwitchThreshold"/> is enabled.
		/// </summary>
		/// <seealso cref="IsScheduleViewToDayViewSwitchEnabledProperty"/>
		/// <seealso cref="ScheduleViewToDayViewSwitchThreshold"/>
		/// <seealso cref="IsDayViewToScheduleViewSwitchEnabled"/>
		/// <seealso cref="DayViewToScheduleViewSwitchThreshold"/>
		public bool IsScheduleViewToDayViewSwitchEnabled
		{
			get
			{
				return (bool)this.GetValue(XamOutlookCalendarView.IsScheduleViewToDayViewSwitchEnabledProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.IsScheduleViewToDayViewSwitchEnabledProperty, value);
			}
		}

		#endregion //IsScheduleViewToDayViewSwitchEnabled

		#region ScheduleViewToDayViewSwitchThreshold

		/// <summary>
		/// Identifies the <see cref="ScheduleViewToDayViewSwitchThreshold"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScheduleViewToDayViewSwitchThresholdProperty = DependencyPropertyUtilities.Register("ScheduleViewToDayViewSwitchThreshold",
			typeof(int), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(1)
			);

		/// <summary>
		/// Returns or sets when the <see cref="CurrentViewMode"/> is changed from ScheduleView to DayView based on a change in <see cref="ScheduleControlBase.VisibleCalendarCount"/>
		/// </summary>
		/// <remarks>
		/// <p class="body">When enabled (<see cref="IsScheduleViewToDayViewSwitchEnabled"/>) the <see cref="CurrentViewMode"/> will be changed to one of the 
		/// DayView related views when the view is one of the Schedule views and the <see cref="ScheduleControlBase.VisibleCalendarCount"/> of the 
		/// <see cref="XamScheduleView"/> goes from above this property's value to the property's value or lower.</p>
		/// </remarks>
		/// <seealso cref="ScheduleViewToDayViewSwitchThresholdProperty"/>
		/// <seealso cref="IsScheduleViewToDayViewSwitchEnabled"/>
		/// <seealso cref="IsDayViewToScheduleViewSwitchEnabled"/>
		/// <seealso cref="DayViewToScheduleViewSwitchThreshold"/>
		public int ScheduleViewToDayViewSwitchThreshold
		{
			get
			{
				return (int)this.GetValue(XamOutlookCalendarView.ScheduleViewToDayViewSwitchThresholdProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.ScheduleViewToDayViewSwitchThresholdProperty, value);
			}
		}

		#endregion //ScheduleViewToDayViewSwitchThreshold

		#region WorkingDaysSource

		/// <summary>
		/// Identifies the <see cref="WorkingDaysSource"/> dependency property
		/// </summary>
		public static readonly DependencyProperty WorkingDaysSourceProperty = DependencyPropertyUtilities.Register("WorkingDaysSource",
			typeof(WorkingHoursSource), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(WorkingHoursSource.CurrentUser, new PropertyChangedCallback(OnWorkingDaysSourceChanged))
			);

		private static void OnWorkingDaysSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;
			instance.InitializeWorkingDaySource();
		}

		/// <summary>
		/// Returns or sets a value that determines which days are considered working days within the controls contained within the XamOutlookCalendarView.
		/// </summary>
		/// <seealso cref="WorkingDaysSourceProperty"/>
		public WorkingHoursSource WorkingDaysSource
		{
			get
			{
				return (WorkingHoursSource)this.GetValue(XamOutlookCalendarView.WorkingDaysSourceProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.WorkingDaysSourceProperty, value);
			}
		}

		#endregion //WorkingDaysSource

		#endregion // Public Properties

		#region Internal Properties

		#region CurrentViewControl
		internal ScheduleControlBase CurrentViewControl
		{
			get { return _currentViewControl; }
			private set
			{
				if ( value != _currentViewControl )
				{
					var oldValue = _currentViewControl;
					_currentViewControl = value;

					this.OnCurrentViewControlChanged(oldValue, value);
				}
			}
		}
		#endregion // CurrentViewControl

		#region IsInitializing
		internal bool IsInitializing
		{
			get { return this.GetFlag(InternalFlags.IsInitializing); }
		}
		#endregion //IsInitializing

		#region IsVerifyingCurrentViewControl
		internal bool IsVerifyingCurrentViewControl
		{
			get { return this.GetFlag(InternalFlags.VerifyingCurrentViewControl); }
		} 
		#endregion // IsVerifyingCurrentViewControl

		#region VisibleCalendarCount

		private static readonly DependencyProperty VisibleCalendarCountProperty = DependencyPropertyUtilities.Register("VisibleCalendarCount",
			typeof(int), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnVisibleCalendarCountChanged))
			);

		private static void OnVisibleCalendarCountChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;
			instance.OnVisibleCalendarCountChanged((int)e.OldValue, (int)e.NewValue);
		}

		private void OnVisibleCalendarCountChanged(int oldValue, int newValue)
		{
			if ( this.GetFlag(InternalFlags.IsChangingView) )
				return;

			if ( this.GetFlag(InternalFlags.VerifyingCurrentViewControl) )
				return;

			bool switchView = false;
			var ctrl = this.CurrentViewControl;

			if ( ctrl is XamDayView && newValue > oldValue && this.IsDayViewToScheduleViewSwitchEnabled )
			{
				int threshold = this.DayViewToScheduleViewSwitchThreshold;
				switchView = oldValue < threshold && newValue >= threshold;
			}
			else if ( ctrl is XamScheduleView && newValue < oldValue && this.IsScheduleViewToDayViewSwitchEnabled )
			{
				int threshold = this.ScheduleViewToDayViewSwitchThreshold;
				switchView = oldValue > threshold && newValue <= threshold;
			}

			if ( switchView )
			{
				this.SetCurrentView(GetSwitchedView(this.CurrentViewMode));
			}
		}

		private int VisibleCalendarCount
		{
			get
			{
				return (int)this.GetValue(XamOutlookCalendarView.VisibleCalendarCountProperty);
			}
			set
			{
				this.SetValue(XamOutlookCalendarView.VisibleCalendarCountProperty, value);
			}
		}

		#endregion //VisibleCalendarCount

		#endregion // Internal Properties

		#region Private Properties

		#region DefaultBrushProviderInternal

		private static readonly DependencyProperty DefaultBrushProviderInternalProperty = DependencyPropertyUtilities.Register("DefaultBrushProviderInternal",
			typeof(CalendarBrushProvider), typeof(XamOutlookCalendarView),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDefaultBrushProviderInternalChanged))
			);

		private static void OnDefaultBrushProviderInternalChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamOutlookCalendarView instance = (XamOutlookCalendarView)d;
			instance.DefaultBrushProvider = e.NewValue as CalendarBrushProvider;
		}

		#endregion //DefaultBrushProviderInternal

		#region DayView
		private XamDayView DayView
		{
			get { return _dayView; }
		}
		#endregion // DayView

		#region InitializeWorkingDaySource
		private void InitializeWorkingDaySource()
		{
			WorkingHoursSource src = this.WorkingDaysSource;

			if ( _monthView != null )
				_monthView.WorkingDaysSource = src;

			if ( _dayView != null )
				_dayView.WorkDaySource = src;

			if ( _scheduleView != null )
				_scheduleView.WorkDaySource = src;
		}
		#endregion // InitializeWorkingDaySource

		#region MonthView
		private XamMonthView MonthView
		{
			get { return _monthView; }
		}
		#endregion // MonthView

		#region ScheduleView
		private XamScheduleView ScheduleView
		{
			get { return _scheduleView; }
		}
		#endregion // ScheduleView

		#endregion // Private Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region BeginInit
		/// <summary>
		/// Indicates that the initialization process for the element is starting.
		/// </summary>

		public override void BeginInit()
		{
			base.BeginInit();





			this.SetFlag(InternalFlags.IsInitializing, true);
		}
		#endregion //BeginInit

		#region EndInit
		/// <summary>
		/// Indicates that the initialization process for the element is complete.
		/// </summary>

		public override void EndInit()
		{
			base.EndInit();




			this.SetFlag(InternalFlags.IsInitializing, false);

			this.VerifyState();
		}
		#endregion //EndInit

		#endregion //Public Methods

		#region Internal Methods

		#region ChangeDayCount
		internal void ChangeDayCount( int dayCount, DateTime? logicalDate = null )
		{
			var view = this.CurrentViewMode;

			// determine the new view
			switch ( view )
			{
				case OutlookCalendarViewMode.DayViewDay:
				case OutlookCalendarViewMode.ScheduleViewDay:
					break;
				case OutlookCalendarViewMode.MonthView:
				case OutlookCalendarViewMode.DayViewWeek:
				case OutlookCalendarViewMode.DayViewWorkWeek:
					view = OutlookCalendarViewMode.DayViewDay;
					break;
				case OutlookCalendarViewMode.ScheduleViewWeek:
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
					view = OutlookCalendarViewMode.ScheduleViewDay;
					break;
			}

			this.ChangeDayCount(dayCount, view, logicalDate);
		}

		internal void ChangeDayCount( int dayCount, OutlookCalendarViewMode newView, DateTime? logicalDate = null )
		{
			Debug.Assert(IsSingleDayView(newView), "This only supports DayViewDay or ScheduleViewDay");

			var viewControl = this.CurrentViewControl;

			if ( viewControl != null && logicalDate == null )
			{
				var range = viewControl.GetSelectionAnchor() ?? new DateRange(viewControl.GetActivatableDate(DateTime.Now));
				logicalDate = viewControl.ConvertToLogicalDate(range).Start;
			}

			var newViewControl = this.GetControl(newView, true);
			IList<DateTime> newDates = null;

			if ( newViewControl != null )
			{
				if ( logicalDate == null )
					logicalDate = newViewControl.ConvertToLogicalDate(viewControl.GetActivatableDate(DateTime.Now)).Date;
				else
					logicalDate = logicalDate.Value.Date;

				if ( dayCount == 1 )
				{
					newDates = new DateTime[] { logicalDate.Value };
				}
				else
				{
					var calendarHelper = newViewControl.DateInfoProviderResolved.CalendarHelper;

					DateTime lastDate = calendarHelper.AddDays(logicalDate.Value, dayCount - 1);
					DateTime startDate = calendarHelper.AddDays(lastDate, -dayCount + 1);
					newDates = newViewControl.GetVisibleDatesForRange(new DateRange(startDate, lastDate));
				}
			}

			// change the view but avoid messing with its visible dates since we will do that after this
			this.SetCurrentView(newView, newDates);
		}
		#endregion // ChangeDayCount

		#region GetParameter
		internal virtual object GetParameter( CommandSource source )
		{
			if ( source.Command is ScheduleControlCommandBase )
				return this.CurrentViewControl;

			if ( source.Command is XamOutlookCalendarViewCommandBase )
				return this;

			return null;
		}
		#endregion // GetParameter

		#region IsSingleDayView
		internal static bool IsSingleDayView( OutlookCalendarViewMode view )
		{
			return view == OutlookCalendarViewMode.DayViewDay || view == OutlookCalendarViewMode.ScheduleViewDay;
		}
		#endregion // IsSingleDayView

		#region Navigate
		internal bool Navigate( bool forward )
		{
			var ctrl = this.CurrentViewControl;

			if ( ctrl == null )
				return false;

			if ( ctrl is XamMonthView )
			{
				var monthDay = GetSingleMonthDate(ctrl, true  );

				if ( null != monthDay )
				{
					DateTime monthStart = monthDay.Value;
					var calendarHelper = ctrl.DateInfoProviderResolved.CalendarHelper;
					monthStart = calendarHelper.Calendar.AddMonths(monthStart, forward ? 1 : -1);

					ctrl.WeekHelper.Reset(monthStart);
					return true;
				}
			}

			return ctrl.ExecuteCommand(forward ? ScheduleControlCommand.VisibleDatesShiftPageNext : ScheduleControlCommand.VisibleDatesShiftPagePrevious);
		}
		#endregion // Navigate

		#region SetCurrentView
		/// <summary>
		/// Changes the <see cref="CurrentViewMode"/> to the specified value while optionally avoiding resetting the visible dates of the target control
		/// </summary>
		/// <param name="view">The new view</param>
		/// <param name="newVisibleDates">A list of the new visible dates if the mode is changed otherwise null to keep the default behavior that results from the view switch</param>
		internal bool SetCurrentView( OutlookCalendarViewMode view, IList<DateTime> newVisibleDates = null )
		{
			if ( view != this.CurrentViewMode )
			{
				CurrentViewModeChangingEventArgs beforeArgs = new CurrentViewModeChangingEventArgs(this.CurrentViewMode, view);
				this.OnCurrentViewModeChanging(beforeArgs);

				if ( beforeArgs.Cancel )
					return false;
			}

			bool wasInitializing = this.GetFlag(InternalFlags.IsChangingView);
			bool wasAvoidingDateChanged = this.GetFlag(InternalFlags.DoNotChangeVisibleDates);

			this.SetFlag(InternalFlags.IsChangingView, true);
			this.SetFlag(InternalFlags.DoNotChangeVisibleDates, newVisibleDates != null);

			try
			{
				this.CurrentViewMode = view;

				// if new dates were provided then use them now if the mode was accepted
				if ( null != newVisibleDates && view == this.CurrentViewMode )
				{
					var ctrl = this.GetControl(view, true);

					if ( null != ctrl )
					{
						// the selected time range may have change so process any pending changes first
						ctrl.VerifyState();

						ctrl.VisibleDates.Reinitialize(newVisibleDates);
					}
				}

				this.VerifySelectedActivities();

				return true;
			}
			finally
			{
				this.SetFlag(InternalFlags.IsChangingView, wasInitializing);
				this.SetFlag(InternalFlags.DoNotChangeVisibleDates, wasAvoidingDateChanged);
			}
		}
		#endregion // SetCurrentView

		#region SupportsCommand
		internal virtual bool SupportsCommand( ICommand command )
		{
			return command is ScheduleControlCommandBase || command is XamOutlookCalendarViewCommandBase;
		}
		#endregion // SupportsCommand

		#region SynchronizeControlDates
		internal static void SynchronizeControlDates( ScheduleControlBase source, ScheduleControlBase destination )
		{
			if ( source == null || destination == null )
				return;

			destination.VisibleDates.Reinitialize(source.VisibleDates);
		}
		#endregion // SynchronizeControlDates

		#endregion // Internal Methods

		#region Private Methods

		#region CopySelectedActivities
		private void CopySelectedActivities( bool toViewControl )
		{
			InternalFlags thisFlag = toViewControl ? InternalFlags.CopyingSelectedActivitiesToViewControl : InternalFlags.CopyingSelectedActivitiesFromViewControl;
			InternalFlags otherFlag = toViewControl ? InternalFlags.CopyingSelectedActivitiesFromViewControl : InternalFlags.CopyingSelectedActivitiesToViewControl;

			// if we are synchronizing in the other direction then don't update now
			if ( this.GetFlag(otherFlag) )
				return;

			var ctrl = _currentViewControl;

			if ( ctrl == null )
				return;

			if ( ScheduleUtilities.AreEqual(ctrl.SelectedActivities, this.SelectedActivities) )
				return;

			bool wasChanging = this.GetFlag(thisFlag);
			this.SetFlag(thisFlag, true);

			try
			{
				if ( toViewControl )
					ctrl.SelectedActivities.ReInitialize(this.SelectedActivities);
				else
					this.SelectedActivities.ReInitialize(ctrl.SelectedActivities);
			}
			finally
			{
				this.SetFlag(thisFlag, wasChanging);
			}
		} 
		#endregion // CopySelectedActivities

		#region GetControl
		private ScheduleControlBase GetControl( OutlookCalendarViewMode view, bool resolve = true )
		{
			switch ( view )
			{
				case OutlookCalendarViewMode.DayViewDay:
				case OutlookCalendarViewMode.DayViewWeek:
				case OutlookCalendarViewMode.DayViewWorkWeek:
					return this.DayView ?? (ScheduleControlBase)this.ScheduleView ?? this.MonthView;
				case OutlookCalendarViewMode.ScheduleViewDay:
				case OutlookCalendarViewMode.ScheduleViewWeek:
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
					return this.ScheduleView ?? (ScheduleControlBase)this.DayView ?? this.MonthView;
				case OutlookCalendarViewMode.MonthView:
					return this.MonthView ?? (ScheduleControlBase)this.DayView ?? this.ScheduleView;
				default:
					Debug.Assert(false, "Unrecognized view type:" + view.ToString());
					return null;
			}
		}
		#endregion // GetControl

		#region GetFlag
		/// <summary>
		/// Returns true if any of the specified bits are true.
		/// </summary>
		/// <param name="flag">Flag(s) to evaluate</param>
		/// <returns></returns>
		private bool GetFlag( InternalFlags flag )
		{
			return (_flags & flag) != 0;
		}
		#endregion // GetFlag

		#region GetKeyCommand
		private XamOutlookCalendarViewCommandBase GetKeyCommand( KeyEventArgs e )
		{
			Key key = PresentationUtilities.GetKey(e);

			switch ( key )
			{
				case Key.NumPad0:
				case Key.D0:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Alt) )
							return new XamOutlookCalendarViewDayCountCommand(10);
						break;
					}
				case Key.D4:
				case Key.NumPad4:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Control | ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.MonthView);
						break;
					}

				case Key.OemPlus:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.MonthView);

						break;
					}

				case Key.D2:
				case Key.NumPad2:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Control | ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.DayViewWorkWeek);

						break;
					}
				case Key.D3:
				case Key.NumPad3:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Control | ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.DayViewWeek);
						break;
					}
				case Key.Subtract:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeViewCommand(OutlookCalendarViewMode.DayViewWeek);
						break;
					}
				case Key.D5:
				case Key.NumPad5:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Control | ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeToScheduleViewCommand();

						break;
					}
				case Key.D1:
				case Key.NumPad1:
					{
						if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Control | ModifierKeys.Alt) )
							return new XamOutlookCalendarViewChangeToDayViewCommand();

						break;
					}
			}

			// Alt + 1-9
			if ( PresentationUtilities.IsModifierPressed(ModifierKeys.Alt) )
			{
				if ( key >= Key.NumPad1 && key <= Key.NumPad9 )
					return new XamOutlookCalendarViewDayCountCommand(key - Key.NumPad1 + 1);

				if ( key >= Key.D1 && key <= Key.D9 )
					return new XamOutlookCalendarViewDayCountCommand(key - Key.D1 + 1);
			}

			return null;
		}
		#endregion // GetKeyCommand

		#region GetSingleMonthDate
		// AS 5/19/11 TFS75524
		// Added the onlyIfFullMonth. Basically there are 2 scenarios where we use this routine. One is for 
		// determining the range displayed in the header. For this we only want to show the month if the 
		// selection represents a full month or all the selected dates are within a single month. For 
		// that we want to contiue to return a date even if the selected dates don't comprise an entire 
		// month as long as the dates are within a single month. The other case relates to navigation. 
		// Specifically when navigating forward/back we want to navigate by a month if the selection 
		// represents all the weeks containing dates for a given month.
		//
		private static DateTime? GetSingleMonthDate(ScheduleControlBase ctrl, bool onlyIfFullMonth )
		{
			DateTime? dateInSingleMonth = null;

			if ( ctrl.WeekHelper != null )
			{
				var weeks = ctrl.WeekHelper.Weeks;

				if ( weeks != null && weeks.Count > 1 )
				{
					DateTime startDate = weeks[0].End.AddDays(-1);
					DateTime endDate = weeks[weeks.Count - 1].Start;

					// get the last date of the first week and the first day of the last week. if they are from the same month
					// and it has the # of weeks between indicating that it covers the month then just show that month
					// otherwise show the date range
					var calHelper = ctrl.DateInfoProviderResolved.CalendarHelper;
					if ( calHelper.IsSameMonth(startDate, endDate) )
					{
						int dayCount = (int)endDate.Subtract(startDate).TotalDays;
						dayCount += ((startDate.DayOfWeek - endDate.DayOfWeek) + 7) % 7;

						int weekCount = dayCount / 7 + 1;

						if ( weekCount == weeks.Count )
						{
							DateTime earliestDate = calHelper.AddDays(startDate, -6);
							DateTime latestDate = calHelper.AddDays(endDate, 6);

							if (calHelper.IsSameMonth(earliestDate, latestDate) && !onlyIfFullMonth  )
								dateInSingleMonth = startDate;
							else
							{
								DateTime firstDayOfMonth = calHelper.AddDays(startDate, 1 + -calHelper.GetDayOfMonth(startDate));
								DateTime lastDayOfMonth = calHelper.AddDays(firstDayOfMonth, calHelper.GetDaysInMonth(startDate) - 1);

								if (firstDayOfMonth >= earliestDate && lastDayOfMonth <= latestDate)
									dateInSingleMonth = startDate;
							}
						}
					}
				}
			}

			return dateInSingleMonth;
		}
		#endregion // GetSingleMonthDate

		#region GetSwitchedView
		private static OutlookCalendarViewMode GetSwitchedView( OutlookCalendarViewMode view )
		{
			switch ( view )
			{
				case OutlookCalendarViewMode.DayViewDay:
					return OutlookCalendarViewMode.ScheduleViewDay;
				case OutlookCalendarViewMode.DayViewWeek:
					return OutlookCalendarViewMode.ScheduleViewWeek;
				case OutlookCalendarViewMode.DayViewWorkWeek:
					return OutlookCalendarViewMode.ScheduleViewWorkWeek;

				case OutlookCalendarViewMode.ScheduleViewDay:
					return OutlookCalendarViewMode.DayViewDay;
				case OutlookCalendarViewMode.ScheduleViewWeek:
					return OutlookCalendarViewMode.DayViewWeek;
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
					return OutlookCalendarViewMode.DayViewWorkWeek;
				default:
					Debug.Assert(false, "This is not a view that can be switched!");
					return view;
			}
		}
		#endregion // GetSwitchedView

		#region InitializeCalendarGroupsOverride
		// AS 6/8/11 TFS76111
		//private void InitializeCalendarGroupsOverride( ScheduleControlBase ctrl )
		//{
		//    if (null != ctrl)
		//    {
		//        // AS 1/20/11 TFS62537
		//        //ctrl.CalendarGroupsOverride.ReInitialize(this.CalendarGroupsOverride);
		//        ScheduleUtilities.Reinitialize(ctrl.CalendarGroupsOverride, this.CalendarGroupsOverride);
		//    }
		//} 
		#endregion // InitializeCalendarGroupsOverride

		#region InitializeControlPart
		private void InitializeControlPart<T>( ref T member, string partName ) where T : ScheduleControlBase
		{
			T newValue = this.GetTemplateChild(partName) as T;

			if ( newValue == null )
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_MissingTemplatePart", partName, typeof(T).Name, typeof(XamOutlookCalendarView).Name)); //A template part with a name of '{0}' and type '{1}' is required in the '{2}' template.

			if ( member != newValue )
			{
				T oldValue = member;
				member = newValue;
				this.InitializeScheduleControl(oldValue, newValue);
			}
		}
		#endregion // InitializeControlPart

		#region InitializeCurrentViewDateInfo
		private void InitializeCurrentViewDateInfo()
		{
			var ctrl = this.CurrentViewControl;
			DateCollection visDates = ctrl != null ? ctrl.VisibleDates : null;

			if ( visDates == null || visDates.Count == 0 )
			{
				this.CurrentViewDateRange = null;
				this.CurrentViewDateRangeText = null;
			}
			else
			{
				visDates.Sort();
				this.CurrentViewDateRange = new DateRange(visDates[0], visDates[visDates.Count - 1]);

				var dateProvider = ctrl.DateInfoProviderResolved;
				string text = null;

				if ( visDates.Count == 1 )
				{
					text = dateProvider.FormatDateRange(visDates[0], visDates[visDates.Count - 1], DateRangeFormatType.CalendarDateRange);
				}
				else if ( ctrl is XamMonthView )
				{
					var weekHelper = ctrl.WeekHelper;

					DateTime start = visDates[0];
					DateTime end = visDates[visDates.Count - 1];

					if ( dateProvider.CalendarHelper.IsSameMonth(start, end) )
					{
						end = start;
					}
					else
					{
						DateTime? dateInSingleMonth = GetSingleMonthDate(ctrl, false );

						if (dateInSingleMonth != null)
							start = end = dateInSingleMonth.Value;
					}

					text = dateProvider.FormatDateRange(start, end, DateRangeFormatType.CalendarMonthRange);
				}
				else
				{
					text = dateProvider.FormatDateRange(visDates[0], visDates[visDates.Count - 1], DateRangeFormatType.CalendarDateRange);
				}

				this.CurrentViewDateRangeText = text;
			}

			this.InitializeDateNavigator();
		}
		#endregion // InitializeCurrentViewDateInfo

		#region InitializeDateNavigator
		private void InitializeDateNavigator( bool verifyState = false, bool force = false )
		{
			var ctrl = this.CurrentViewControl;

			if ( ctrl == null )
				return;

			if ( !force && this.GetFlag(InternalFlags.InitializingFromDateNavigator) )
				return;

			if ( verifyState )
				ctrl.VerifyState();

			// update the selected dates of the date navigator
			IOutlookDateNavigator provider = this.DateNavigator;

			if ( null != provider )
			{
				bool wasInitializing = this.GetFlag(InternalFlags.InitializingDateNavigator);
				this.SetFlag(InternalFlags.InitializingDateNavigator, true);

				try
				{
					provider.SetSelectedDates(ctrl.VisibleDates);
				}
				finally
				{
					this.SetFlag(InternalFlags.InitializingDateNavigator, wasInitializing);
				}
			}
		} 
		#endregion // InitializeDateNavigator

		#region InitializeFromDateNavigator
		private void InitializeFromDateNavigator(SelectedDatesChangedEventArgs e)
		{
			if ( this.GetFlag(InternalFlags.InitializingDateNavigator) )
				return;

			bool wasInitializing = this.GetFlag(InternalFlags.InitializingFromDateNavigator);
			this.SetFlag(InternalFlags.InitializingFromDateNavigator, true);

			try
			{
				this.InitializeFromDateNavigatorImpl(e);

				// reset the selection. this is definitely needed when selecting a single
				// date within the currently selected week
				this.InitializeDateNavigator(true, true);
			}
			finally
			{
				this.SetFlag(InternalFlags.InitializingFromDateNavigator, wasInitializing);
			}
		}

		private void InitializeFromDateNavigatorImpl(SelectedDatesChangedEventArgs e)
		{
			#region Set up

			var selectionProvider = this.DateNavigator;
			var selectedDates = selectionProvider.GetSelectedDates();
			var view = this.CurrentViewMode;
			var viewCtrl = this.CurrentViewControl;

			if ( viewCtrl == null )
				return;

			// not sure when this would come up 
			if ( selectedDates == null || selectedDates.Count == 0 )
				return;

			// for some cases adding days to an existing view results in different behavior
			// e.g. in day view if you have 6 days selected and you ctrl click to add the 7th
			// then you are not in week view - you are in dayview for 7 days
			bool isPartialSelection = e.AddedDates.Count != selectedDates.Count;

			const int DaysInWeek = 7;
			const int DaysIn2Weeks = 14;

			// strip out the time portion and then sort them
			var dates = new List<DateTime>(selectedDates.Select(( DateTime date ) => { return date.Date; }));
			dates.Sort();

			for ( int i = dates.Count - 1; i > 0; i-- )
			{
				if ( dates[i] == dates[i - 1] )
					dates.RemoveAt(i);
			} 

			#endregion // Set up

			#region Select single day while in a "week" mode

			bool isInWeekMode = false;

			switch ( view )
			{
				case OutlookCalendarViewMode.DayViewWeek:
				case OutlookCalendarViewMode.DayViewWorkWeek:
				case OutlookCalendarViewMode.ScheduleViewWeek:
				case OutlookCalendarViewMode.ScheduleViewWorkWeek:
					{
						isInWeekMode = true;

						// if we're in week mode and 1 day is selected then show that week/work-week
						if ( dates.Count == 1 )
						{
							viewCtrl.WeekHelper.Reset(viewCtrl.GetActivatableDate(dates[0].Date));
							return;
						}
						break;
					}
				case OutlookCalendarViewMode.MonthView:
				case OutlookCalendarViewMode.ScheduleViewDay:
				case OutlookCalendarViewMode.DayViewDay:
					break;
				default:
					Debug.Assert(false, "Unrecognized view:" + view);
					break;
			} 
			#endregion // Select single day while in a "week" mode

			#region Adding/Removing Days in DayView
			// AS 4/21/11 TFS73200
			// Outlook has some seemingly strange behavior with regards to selecting 14 days. If you 
			// try to use ctrl-shift to select a range that would create 2 weeks of selection, they 
			// only select up to the 13 days. You can then use ctrl-click to add the 14th day in which 
			// case they will stay in day view.
			//
			//if ( isPartialSelection && viewCtrl is XamDayView && dates.Count <= DaysInWeek * 2 )
			if ( isPartialSelection && viewCtrl is XamDayView )
			{
				// AS 4/27/11 TFS73770
				// If the # of days is going to 7 we may want to go into week mode so only stay in day mode 
				// here if the control key is pressed (i.e. they are adding to the selection).
				//
				//if ( dates.Count < DaysInWeek * 2 || (dates.Count == DaysInWeek * 2 && e.AddedDates.Count == 1) )
				bool canStayInDayView = false;

				if (dates.Count == DaysIn2Weeks && e.AddedDates.Count == 1)
					canStayInDayView = true;
				else if (dates.Count != DaysInWeek && dates.Count < DaysIn2Weeks)
					canStayInDayView = true;
				else if (dates.Count == DaysInWeek && PresentationUtilities.IsModifierPressed(ModifierKeys.Control, ModifierKeys.Shift))
					canStayInDayView = true;

				if ( canStayInDayView )
				{
					this.SetCurrentView(OutlookCalendarViewMode.DayViewDay, dates);
					return;
				}
			} 
			#endregion // Adding/Removing Days in DayView

			#region Going to week/work-week mode
			ScheduleTimeControlBase timeCtrl = viewCtrl as ScheduleTimeControlBase;

			// we also want to do this if we're in month view except we'll default to 
			// using day view (which is what outlook prefers).
			if ( timeCtrl == null )
				timeCtrl = this.DayView ?? (ScheduleTimeControlBase)this.ScheduleView;

			if ( timeCtrl != null )
			{
				// if we are currently in a week or work week mode and the user choose the days 
				// that represent either a week mode or work week mode, then update the dates and 
				// ensure we are in that mode. similarly, if we are a "day" mode or month mode 
				// then only go into full week mode. outlook is inconsistent about the former - 
				// they will do it when in a schedule view mode but not in 
				// if we are in schedule or day view and the user chooses days that match either 
				// a full week's dates or the work week's dates for a given week then go into week
				// mode keeping the current view control. outlook does this when in schedule view 
				// although they are inconsistent about it and don't do it when selecting what 
				// would be a work week in day view (although they do handle it in schedule view).
				//
				var displayMode = timeCtrl.GetWeekModeFromDates(dates);

				if ( displayMode != WeekDisplayMode.None )
				{
					if ( displayMode == WeekDisplayMode.Week || isInWeekMode || viewCtrl is XamScheduleView )
					{
						if ( timeCtrl is XamScheduleView )
							view = displayMode == WeekDisplayMode.WorkWeek ? OutlookCalendarViewMode.ScheduleViewWorkWeek : OutlookCalendarViewMode.ScheduleViewWeek;
						else
							view = displayMode == WeekDisplayMode.WorkWeek ? OutlookCalendarViewMode.DayViewWorkWeek : OutlookCalendarViewMode.DayViewWeek;

						this.SetCurrentView(view, dates);
						return;
					}
				}
			} 
			#endregion // Going to week/work-week mode

			#region Going to month view
			// if the user selects multiple weeks
			if ( dates.Count > DaysInWeek && dates.Count % DaysInWeek == 0 )
			{
				var monthView = this.MonthView;

				if ( null != monthView )
				{
					var newVisibleDates = monthView.WeekHelper.CalculateWeeks(dates);

					if ( ScheduleUtilities.AreEqual(newVisibleDates, dates) )
					{
						// if the user is in schedule view and they select 2 weeks then they 
						// stay in schedule view (day mode)
						if ( viewCtrl is XamScheduleView && dates.Count == DaysInWeek * 2 )
						{
							this.SetCurrentView(OutlookCalendarViewMode.ScheduleViewDay, newVisibleDates);
							return;
						}

						// if they are in day view or have chosen more than 2 weeks then they go into month view
						this.SetCurrentView(OutlookCalendarViewMode.MonthView, newVisibleDates);
						return;
					}
				}
			} 
			#endregion // Going to month view

			#region <= 2 weeks - just use day view
			if ( dates.Count <= DaysIn2Weeks )
			{
				// lastly fall back to single day view but leave it using schedule view
				// if it was using that control before
				if ( viewCtrl is XamScheduleView )
					view = OutlookCalendarViewMode.ScheduleViewDay;
				else
					view = OutlookCalendarViewMode.DayViewDay;

				this.SetCurrentView(view, dates);
				return;
			} 
			#endregion // <= 2 weeks - just use day view

			this.SetCurrentView(OutlookCalendarViewMode.MonthView, dates);
			//Debug.Assert(false, "Nothing?");
		}

		#endregion // InitializeFromDateNavigator

		#region InitializeScheduleControlBinding
		private void InitializeScheduleControlBinding(ScheduleControlBase target, DependencyProperty targetProperty, DependencyProperty sourceProperty, BindingMode mode, bool clearBinding)
		{
			if (clearBinding)
				target.ClearValue(targetProperty);
			else
				BindingOperations.SetBinding(target, targetProperty, new Binding { Path = new PropertyPath(DependencyPropertyUtilities.GetName(sourceProperty)), Source = this, Mode = mode });
		} 
		#endregion //InitializeScheduleControlBinding

		#region InitializeScheduleControlBindings
		private void InitializeScheduleControlBindings(ScheduleControlBase target, bool clear)
		{
			#region ScheduleControlBase

			InitializeScheduleControlBinding(target, ScheduleControlBase.AllowCalendarGroupResizingProperty, XamOutlookCalendarView.AllowCalendarGroupResizingProperty, BindingMode.OneWay, clear);
			InitializeScheduleControlBinding(target, ScheduleControlBase.CalendarDisplayModeProperty, XamOutlookCalendarView.CalendarDisplayModeProperty, BindingMode.OneWay, clear);
			InitializeScheduleControlBinding(target, ScheduleControlBase.ShowCalendarCloseButtonProperty, XamOutlookCalendarView.ShowCalendarCloseButtonProperty, BindingMode.OneWay, clear);
			InitializeScheduleControlBinding(target, ScheduleControlBase.ShowCalendarOverlayButtonProperty, XamOutlookCalendarView.ShowCalendarOverlayButtonProperty, BindingMode.OneWay, clear);
			InitializeScheduleControlBinding(target, ScheduleControlBase.IsTouchSupportEnabledProperty, XamOutlookCalendarView.IsTouchSupportEnabledProperty, BindingMode.OneWay, clear); // AS 3/14/12 Touch Support

			#endregion //ScheduleControlBase

			#region ScheduleTimeControlBase
			var time = target as ScheduleTimeControlBase;

			if (null != time)
			{
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.CurrentTimeIndicatorVisibilityProperty, XamOutlookCalendarView.CurrentTimeIndicatorVisibilityProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.PrimaryTimeZoneLabelProperty, XamOutlookCalendarView.PrimaryTimeZoneLabelProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.SecondaryTimeZoneIdProperty, XamOutlookCalendarView.SecondaryTimeZoneIdProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.SecondaryTimeZoneLabelProperty, XamOutlookCalendarView.SecondaryTimeZoneLabelProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.SecondaryTimeZoneVisibilityProperty, XamOutlookCalendarView.SecondaryTimeZoneVisibilityProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.ShowWorkingHoursOnlyProperty, XamOutlookCalendarView.ShowWorkingHoursOnlyProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.TimeslotIntervalProperty, XamOutlookCalendarView.TimeslotIntervalProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleTimeControlBase.WorkingHoursSourceProperty, XamOutlookCalendarView.WorkingHoursSourceProperty, BindingMode.OneWay, clear);
			}
			#endregion //ScheduleTimeControlBase

			#region MonthView
			var mv = target as XamMonthView;

			if (null != mv)
			{
				InitializeScheduleControlBinding(target, XamMonthView.ShowWeekNumbersProperty, XamOutlookCalendarView.ShowWeekNumbersProperty, BindingMode.OneWay, clear);
			}
			#endregion //MonthView

			#region ScheduleView
			var sv = target as XamScheduleView;

			if (null != sv)
			{
				InitializeScheduleControlBinding(target, XamScheduleView.AllowCalendarHeaderAreaResizingProperty, XamOutlookCalendarView.AllowCalendarHeaderAreaResizingProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, XamScheduleView.CalendarHeaderAreaWidthProperty, XamOutlookCalendarView.CalendarHeaderAreaWidthProperty, BindingMode.TwoWay, clear);
			}
			#endregion //ScheduleView

			#region DayView
			var dv = target as XamDayView;

			if (null != dv)
			{
				InitializeScheduleControlBinding(target, XamDayView.AllowMultiDayActivityAreaResizingProperty, XamOutlookCalendarView.AllowMultiDayActivityAreaResizingProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, XamDayView.MultiDayActivityAreaHeightProperty, XamOutlookCalendarView.MultiDayActivityAreaHeightProperty, BindingMode.TwoWay, clear);
				InitializeScheduleControlBinding(target, XamDayView.TimeslotGutterAreaWidthProperty, XamOutlookCalendarView.TimeslotGutterAreaWidthProperty, BindingMode.OneWay, clear); // AS 5/8/12 TFS108279
			}
			#endregion //DayView

			#region Orientation Based
			if (null != dv || null != mv)
			{
				InitializeScheduleControlBinding(target, ScheduleControlBase.MinCalendarGroupExtentProperty, XamOutlookCalendarView.MinCalendarGroupHorizontalExtentProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleControlBase.PreferredCalendarGroupExtentProperty, XamOutlookCalendarView.PreferredCalendarGroupHorizontalExtentProperty, BindingMode.TwoWay, clear);
			}
			else if (null != sv)
			{
				InitializeScheduleControlBinding(target, ScheduleControlBase.MinCalendarGroupExtentProperty, XamOutlookCalendarView.MinCalendarGroupVerticalExtentProperty, BindingMode.OneWay, clear);
				InitializeScheduleControlBinding(target, ScheduleControlBase.PreferredCalendarGroupExtentProperty, XamOutlookCalendarView.PreferredCalendarGroupVerticalExtentProperty, BindingMode.TwoWay, clear);
			}
			#endregion //Orientation Based
		} 
		#endregion //InitializeScheduleControlBindings

		#region InitializeScheduleControl
		private void InitializeScheduleControl( ScheduleControlBase oldControl, ScheduleControlBase newControl )
		{
			if ( newControl != null )
			{
				newControl.SetBinding(ScheduleControlBase.DataManagerProperty, new Binding("DataManager") { Source = this });

				XamMonthView month = newControl as XamMonthView;

				if ( null != month )
				{
					month.DayHeaderClick += new EventHandler<ScheduleHeaderClickEventArgs>(OnDayHeaderClick);
					month.WeekHeaderClick += new EventHandler<ScheduleHeaderClickEventArgs>(OnWeekHeaderClick);
				}

				XamDayView day = newControl as XamDayView;

				if ( null != day )
				{
					day.DayHeaderClick += new EventHandler<ScheduleHeaderClickEventArgs>(OnDayHeaderClick);
				}

				// AS 6/8/11 TFS76111
				//this.InitializeCalendarGroupsOverride(newControl);
				newControl.CalendarGroupsOverride = _sharedCalendarGroupsOverride;

				this.InitializeScheduleControlBindings(newControl, false);
			}

			if ( oldControl != null )
			{
				XamMonthView month = oldControl as XamMonthView;

				if ( null != month )
				{
					month.DayHeaderClick -= new EventHandler<ScheduleHeaderClickEventArgs>(OnDayHeaderClick);
					month.WeekHeaderClick -= new EventHandler<ScheduleHeaderClickEventArgs>(OnWeekHeaderClick);
				}

				XamDayView day = oldControl as XamDayView;

				if ( null != day )
				{
					day.DayHeaderClick -= new EventHandler<ScheduleHeaderClickEventArgs>(OnDayHeaderClick);
				}

				oldControl.ClearValue(ScheduleControlBase.DataManagerProperty);

				// AS 6/8/11 TFS76111
				//oldControl.CalendarGroupsOverride.Clear();
				oldControl.CalendarGroupsOverride = null;

				this.InitializeScheduleControlBindings(oldControl, true);
			}
		}
		#endregion // InitializeScheduleControl

		#region OnDayHeaderClick
		private void OnDayHeaderClick( object sender, ScheduleHeaderClickEventArgs e )
		{
			this.OnDayHeaderClick(e);

			// change to single day view and select that date
			var view = this.CurrentViewMode;

			if (!IsSingleDayView(view))
				view  = OutlookCalendarViewMode.DayViewDay;

			this.SetCurrentView(OutlookCalendarViewMode.DayViewDay, new DateTime[] { e.Date });
		}
		#endregion // OnDayHeaderClick

		#region OnCurrentViewControlChanged
		private void OnCurrentViewControlChanged( ScheduleControlBase oldValue, ScheduleControlBase newValue )
		{
			if ( null != oldValue )
			{
				oldValue.VisibleDates.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnViewControlVisibleDatesChanged);
				oldValue.SelectedActivitiesChanged -= new EventHandler<SelectedActivitiesChangedEventArgs>(OnViewControlSelectedActivitiesChanged);
				oldValue.ClearValue(ScheduleControlBase.ActiveCalendarProperty);
				oldValue.ClearValue(ScheduleControlBase.SelectedTimeRangeProperty);
			}

			if ( null != newValue )
			{
				newValue.VisibleDates.CollectionChanged += new NotifyCollectionChangedEventHandler(OnViewControlVisibleDatesChanged);
				newValue.SelectedActivitiesChanged += new EventHandler<SelectedActivitiesChangedEventArgs>(OnViewControlSelectedActivitiesChanged);
			}

			foreach ( ScheduleControlBase ctrl in new ScheduleControlBase[] { this.DayView, this.ScheduleView, this.MonthView } )
			{
				if ( ctrl == null )
					continue;

				ctrl.SetValue(FrameworkElement.VisibilityProperty, ctrl == newValue ? KnownBoxes.VisibilityVisibleBox : KnownBoxes.VisibilityCollapsedBox);
			}

			if ( newValue != null )
			{
				// we need to watch when the visible calendar count changes so we can switch if needed
				BindingOperations.SetBinding(this, VisibleCalendarCountProperty, new Binding("VisibleCalendarCount") { Source = newValue });
				_calendarGroupsResolved.SetSourceCollection(newValue.CalendarGroupsResolved);

				// push changes to the active calendar between the current view control
				// and this control's active calendar property
				newValue.SetBinding(ScheduleControlBase.ActiveCalendarProperty, new Binding("ActiveCalendar") { Source = this, Mode = BindingMode.TwoWay });

				// push changes to the selected time range between the current view control and this control's property
				newValue.SetBinding(ScheduleControlBase.SelectedTimeRangeProperty, new Binding("SelectedTimeRange") { Source = this, Mode = BindingMode.TwoWay });

				// copy over the selected activities
				this.CopySelectedActivities(true);

				// initialize the selection based on the new control
				this.UpdateSelectedTimeForControlChange(oldValue, newValue);
			}
			else
			{
				this.ClearValue(VisibleCalendarCountProperty);
				_calendarGroupsResolved.SetSourceCollection(new CalendarGroupBase[0]);
			}

			if ( oldValue != null && PresentationUtilities.HasFocus(oldValue) )
			{
				if ( newValue != null )
					newValue.Focus();
			}
		}
		#endregion // OnCurrentViewControlChanged

		#region OnLoaded
		private void OnLoaded( object sender, RoutedEventArgs e )
		{
			this.VerifyState();
		}
		#endregion // OnLoaded

		#region OnViewControlSelectedActivitiesChanged
		private void OnViewControlSelectedActivitiesChanged( object sender, SelectedActivitiesChangedEventArgs e )
		{
			this.CopySelectedActivities(false);
		}
		#endregion // OnViewControlSelectedActivitiesChanged

		#region OnViewControlVisibleDatesChanged
		private void OnViewControlVisibleDatesChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
		{
			if ( !this.GetFlag(InternalFlags.VerifyingCurrentViewControl) )
			{
				this.InitializeCurrentViewDateInfo();

				// then make sure the selection doesn't include things outside the visible dates
				this.VerifySelectedActivities();
			}
		} 
		#endregion // OnViewControlVisibleDatesChanged

		#region OnWeekHeaderClick
		private void OnWeekHeaderClick( object sender, ScheduleHeaderClickEventArgs e )
		{
			this.OnWeekHeaderClick(e);

			this.SetCurrentView(OutlookCalendarViewMode.DayViewWeek, new DateTime[] { e.Date });
		}
		#endregion // OnWeekHeaderClick

		#region ProcessKeyDown
		private void ProcessKeyDown( KeyEventArgs e )
		{
			XamOutlookCalendarViewCommandBase cmd = this.GetKeyCommand(e);

			if ( null != cmd )
			{
				if ( cmd.CanExecute(this) )
				{
					cmd.Execute(this);
					e.Handled = true;
				}
			}
		}
		#endregion // ProcessKeyDown

		#region SetFlag
		private void SetFlag( InternalFlags flag, bool set )
		{
			if ( set )
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region UpdateSelectedTimeForControlChange
		private void UpdateSelectedTimeForControlChange( ScheduleControlBase oldValue, ScheduleControlBase newValue )
		{
			if ( this.SelectedTimeRange == null )
				return;

			if ( newValue == null )
				return;

			DateTime start, end;
			DateRange selectedTimeRange = this.SelectedTimeRange.Value;

			if ( newValue is XamMonthView && !(oldValue is XamMonthView) )
			{
				// translate the selection to the days that would contain it
				start = ScheduleUtilities.GetLogicalDayRange(selectedTimeRange.Start, newValue.TimeslotInfo.DayOffset, newValue.TimeslotInfo.DayDuration).Start;

				end = selectedTimeRange.IsEmpty ? selectedTimeRange.End : ScheduleUtilities.GetNonInclusiveEnd(selectedTimeRange.End);
				end = ScheduleUtilities.GetLogicalDayRange(end, newValue.TimeslotInfo.DayOffset, newValue.TimeslotInfo.DayDuration).End;
			}
			else if ( oldValue is XamMonthView && newValue is ScheduleTimeControlBase )
			{
				if ( newValue is XamDayView && ((XamDayView)newValue).MultiDayActivityAreaVisibility != System.Windows.Visibility.Collapsed )
				{
					// when going to a dayview with all day event support then we can just
					// keep the selection the same
					start = selectedTimeRange.Start;
					end = selectedTimeRange.End;
				}
				else
				{
					// otherwise just select the first working hour timeslot
					start = oldValue.ConvertToLogicalDate(selectedTimeRange.Start);
					var defaultRange = newValue.GetDefaultSelectedRange(start);

					if ( defaultRange != null )
					{
						start = defaultRange.Value.Start;
						end = defaultRange.Value.End;
					}
					else
					{
						start = selectedTimeRange.Start;
						end = selectedTimeRange.End;
					}
				}
			}
			else
			{
				start = selectedTimeRange.Start;
				end = selectedTimeRange.End;
			}

			this.SelectedTimeRange = new DateRange(start, end);
		}
		#endregion // UpdateSelectedTimeForControlChange

		#region VerifyCurrentView
		private void VerifyCurrentView()
		{
			bool wasVerifying = this.GetFlag(InternalFlags.VerifyingCurrentViewControl);
			this.SetFlag(InternalFlags.VerifyingCurrentViewControl, true);

			try
			{
				this.VerifyCurrentViewImpl();
			}
			finally
			{
				this.SetFlag(InternalFlags.VerifyingCurrentViewControl, wasVerifying);
			}

		}

		private void VerifyCurrentViewImpl()
		{
			var view = this.CurrentViewMode;
			var newCtrl = this.GetControl(view, true);
			var oldCtrl = _oldViewMode == null ? null : this.GetControl(_oldViewMode.Value, true);
			_oldViewMode = view;

			if ( null == newCtrl )
				return;

			ScheduleTimeControlBase timeCtrl = newCtrl as ScheduleTimeControlBase;

			if ( null != timeCtrl )
			{
				switch ( view )
				{
					case OutlookCalendarViewMode.DayViewWorkWeek:
					case OutlookCalendarViewMode.ScheduleViewWorkWeek:
						timeCtrl.WeekDisplayMode = WeekDisplayMode.WorkWeek;
						break;
					case OutlookCalendarViewMode.DayViewWeek:
					case OutlookCalendarViewMode.ScheduleViewWeek:
						timeCtrl.WeekDisplayMode = WeekDisplayMode.Week;
						break;
					default:
						timeCtrl.WeekDisplayMode = WeekDisplayMode.None;
						break;
				}
			}

			if ( oldCtrl != null && !this.GetFlag(InternalFlags.DoNotChangeVisibleDates) )
			{
				if ( oldCtrl != newCtrl )
				{
					// if we are going from dayview to scheduleview or vice versa, then copy the visible dates
					if ( oldCtrl is ScheduleTimeControlBase && newCtrl is ScheduleTimeControlBase )
					{
						SynchronizeControlDates(oldCtrl, newCtrl);
					}
					else if ( newCtrl is XamMonthView )
					{
						// when switching to month view then use the 
						var range = oldCtrl.GetSelectionAnchor() ?? new DateRange(oldCtrl.GetActivatableDate(DateTime.Now));

						// reset the visible dates to show the entire month
						DateTime newDate = oldCtrl.ConvertToLogicalDate(range).Start.Date;
						newCtrl.WeekHelper.Reset(newDate);

						// if we have selected activities then we shouldn't select the timerange since 
						// that will clear the selected activties
						if ( this.SelectedActivities.Count == 0 )
						{
							// reset the selection to select that day
							DateTime localDate = newCtrl.ConvertFromLogicalDate(newDate);
							newCtrl.SelectedTimeRange = ScheduleUtilities.GetLogicalDayRange(localDate, newCtrl.TimeslotInfo.DayOffset, newCtrl.TimeslotInfo.DayDuration);
						}
					}
					else
					{
						Debug.Assert(oldCtrl is XamMonthView && newCtrl is ScheduleTimeControlBase);

						// in outlook if you click the schedule view button with 2 weeks selected then 
						// it keeps those 2 weeks selected.
						if ( oldCtrl is XamMonthView && view == OutlookCalendarViewMode.ScheduleViewDay && oldCtrl.WeekHelper.WeekCount <= 2 )
						{
							SynchronizeControlDates(oldCtrl, newCtrl);
						}
						else
						{
							var range = oldCtrl.GetSelectedTimeRangeForNavigation();
							Debug.Assert(null != range, "There is no source range so what should we use to initialize the visible dates?");

							if ( null != range )
							{
								DateTime logicalDate = newCtrl.ConvertToLogicalDate(range.Value.Start).Date;
								newCtrl.VisibleDates.Reinitialize(new DateTime[] { logicalDate });
							}
						}
					}
				}
			}

			this.CurrentViewControl = newCtrl;

			// the visible date info may be dirty and pending a change so we don't want to autoswitch
			// in this case so make sure the control state is initialized
			if ( !this.GetFlag(InternalFlags.IsChangingView) )
			{
				newCtrl.VerifyState();

				// then make sure the selection doesn't include things outside the visible dates
				this.VerifySelectedActivities();
			}

			this.InitializeCurrentViewDateInfo();
		}
		#endregion // VerifyCurrentView

		#region VerifySelectedActivities
		private void VerifySelectedActivities()
		{
			if ( this.SelectedActivities.Count == 0 )
				return;

			var ctrl = this.GetControl(this.CurrentViewMode, true);

			if ( ctrl == null )
				return;

			if ( ctrl.VisibleDates.Count == 0 )
			{
				this.SelectedActivities.Clear();
				return;
			}

			List<DateRange> localRanges = ctrl.GetLocalVisibleDateRanges();

			var localToken = ctrl.TimeZoneInfoProviderResolved.LocalToken;

			foreach ( ActivityBase activity in this.SelectedActivities )
			{
				DateRange activityRange = ScheduleUtilities.ConvertFromUtc(localToken, activity);

				int start = ScheduleUtilities.BinarySearch(localRanges, activityRange.Start);

				if ( start >= 0 )
					continue;

				int end = ScheduleUtilities.BinarySearch(localRanges, activityRange.End);

				// if there is an activity that is not in view then clear all the selected activities. 
				// this follows what outlook does which makes sense. they don't want to keep part of the 
				// selection because you may not realize that some of your selection was lost
				if ( end == start )
				{
					this.SelectedActivities.Clear();
					break;
				}
			}
		}
		#endregion // VerifySelectedActivities

		#region VerifyState
		private void VerifyState()
		{
			this.VerifyCurrentView();
		}
		#endregion // VerifyState

		#endregion // Private Methods

		#endregion // Methods

		#region Events

		#region ActiveCalendarChanged

		/// <summary>
		/// Used to invoke the <see cref="ActiveCalendarChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The old property value</param>
		/// <param name="newValue">The new property value</param>
		/// <seealso cref="ActiveCalendarChanged"/>
		/// <seealso cref="ActiveCalendar"/>
		protected virtual void OnActiveCalendarChanged( ResourceCalendar oldValue, ResourceCalendar newValue )
		{
			var handler = this.ActiveCalendarChanged;

			if ( null != handler )
				handler(this, new RoutedPropertyChangedEventArgs<ResourceCalendar>(oldValue, newValue));
		}

		/// <summary>
		/// Occurs when the value of the ActiveCalendar property changes
		/// </summary>
		/// <seealso cref="OnActiveCalendarChanged(ResourceCalendar, ResourceCalendar)"/>
		/// <seealso cref="ActiveCalendar"/>
		public event RoutedPropertyChangedEventHandler<ResourceCalendar> ActiveCalendarChanged;

		#endregion //ActiveCalendarChanged

		#region CurrentViewModeChanged

		/// <summary>
		/// Used to invoke the <see cref="CurrentViewModeChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The old property value</param>
		/// <param name="newValue">The new property value</param>
		/// <seealso cref="CurrentViewModeChanged"/>
		/// <seealso cref="CurrentViewMode"/>
		protected virtual void OnCurrentViewModeChanged( OutlookCalendarViewMode oldValue, OutlookCalendarViewMode newValue )
		{
			var handler = this.CurrentViewModeChanged;

			if ( null != handler )
				handler(this, new RoutedPropertyChangedEventArgs<OutlookCalendarViewMode>(oldValue, newValue));
		}

		/// <summary>
		/// Occurs when the value of the CurrentViewMode property changes
		/// </summary>
		/// <seealso cref="OnCurrentViewModeChanged(OutlookCalendarViewMode, OutlookCalendarViewMode)"/>
		/// <seealso cref="CurrentViewMode"/>
		public event RoutedPropertyChangedEventHandler<OutlookCalendarViewMode> CurrentViewModeChanged;

		#endregion //CurrentViewModeChanged

		#region CurrentViewModeChanging
		/// <summary>
		/// Used to invoke the <see cref="CurrentViewModeChanging"/> event when the control is about to change the <see cref="CurrentViewMode"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This event is not raised when the <see cref="CurrentViewMode"/> property is changed. It is raised when the 
		/// control is about to change the CurrentViewMode.</p>
		/// </remarks>
		/// <param name="e">Provides the information for the event</param>
		/// <seealso cref="CurrentViewModeChanging"/>
		/// <seealso cref="CurrentViewModeChanged"/>
		/// <seealso cref="CurrentViewMode"/>
		protected virtual void OnCurrentViewModeChanging( CurrentViewModeChangingEventArgs e )
		{
			var handler = this.CurrentViewModeChanging;

			if ( null != handler )
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the control is about to change the value of the <see cref="CurrentViewMode"/>
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> This event is not raised when the <see cref="CurrentViewMode"/> property is changed directly. It is raised when the 
		/// control is about to change the CurrentViewMode.</p>
		/// </remarks>
		/// <seealso cref="OnCurrentViewModeChanging(CurrentViewModeChangingEventArgs)"/>
		/// <seealso cref="CurrentViewModeChanged"/>
		/// <seealso cref="CurrentViewMode"/>
		public event EventHandler<CurrentViewModeChangingEventArgs> CurrentViewModeChanging; 
		#endregion // CurrentViewModeChanging

		#region DayHeaderClick

		/// <summary>
		/// Used to invoke the <see cref="DayHeaderClick"/> event.
		/// </summary>
		/// <param name="e">The event arguments for the event</param>
		/// <seealso cref="DayHeaderClick"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		protected virtual void OnDayHeaderClick( ScheduleHeaderClickEventArgs e )
		{
			var handler = this.DayHeaderClick;

			if ( null != handler )
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the user clicks on the header of a day in <see cref="XamMonthView"/> or <see cref="XamDayView"/> within the control.
		/// </summary>
		/// <seealso cref="XamDayView.DayHeaderClick"/>
		/// <seealso cref="XamMonthView.DayHeaderClick"/>
		/// <seealso cref="WeekHeaderClick"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		public event EventHandler<ScheduleHeaderClickEventArgs> DayHeaderClick;

		#endregion //DayHeaderClick

		#region SelectedActivitiesChanged

		/// <summary>
		/// Used to invoke the <see cref="SelectedActivitiesChanged"/> event.
		/// </summary>
		/// <seealso cref="SelectedActivitiesChanged"/>
		/// <seealso cref="SelectedActivities"/>
		/// <seealso cref="SelectedActivitiesChangedEventArgs"/>
		protected internal virtual void OnSelectedActivitiesChanged()
		{
			var handler = this.SelectedActivitiesChanged;

			if ( null != handler )
				handler(this, new SelectedActivitiesChangedEventArgs());
		}

		/// <summary>
		/// Occurs when the contents of the SelectedActivities collection has changed.
		/// </summary>
		/// <seealso cref="OnSelectedActivitiesChanged()"/>
		/// <seealso cref="SelectedActivities"/>
		/// <seealso cref="SelectedActivitiesChangedEventArgs"/>
		public event EventHandler<SelectedActivitiesChangedEventArgs> SelectedActivitiesChanged;

		#endregion //SelectedActivitiesChanged

		#region SelectedTimeRangeChanged

		/// <summary>
		/// Used to invoke the <see cref="SelectedTimeRangeChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The old property value</param>
		/// <param name="newValue">The new property value</param>
		/// <seealso cref="SelectedTimeRangeChanged"/>
		/// <seealso cref="SelectedTimeRange"/>
		protected virtual void OnSelectedTimeRangeChanged( DateRange? oldValue, DateRange? newValue )
		{
			var handler = this.SelectedTimeRangeChanged;

			if ( null != handler )
				handler(this, new NullableRoutedPropertyChangedEventArgs<DateRange>(oldValue, newValue));
		}

		/// <summary>
		/// Occurs when the value of the SelectedTimeRange property changes
		/// </summary>
		/// <seealso cref="OnSelectedTimeRangeChanged(DateRange?, DateRange?)"/>
		/// <seealso cref="SelectedTimeRange"/>
		public event NullableRoutedPropertyChangedEventHandler<DateRange> SelectedTimeRangeChanged;

		#endregion //SelectedTimeRangeChanged

		#region WeekHeaderClick

		/// <summary>
		/// Used to invoke the <see cref="WeekHeaderClick"/> event.
		/// </summary>
		/// <param name="e">The event arguments for the event</param>
		/// <seealso cref="WeekHeaderClick"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		protected virtual void OnWeekHeaderClick( ScheduleHeaderClickEventArgs e )
		{
			var handler = this.WeekHeaderClick;

			if ( null != handler )
				handler(this, e);
		}

		/// <summary>
		/// Occurs when the user clicks on the week header in the <see cref="XamMonthView"/> within the control.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The <see cref="ScheduleHeaderClickEventArgs.Date"/> represents the logical day that is the start of the week whose header was clicked.</p>
		/// </remarks>
		/// <seealso cref="XamMonthView.WeekHeaderClick"/>
		/// <seealso cref="DayHeaderClick"/>
		/// <seealso cref="ScheduleHeaderClickEventArgs"/>
		public event EventHandler<ScheduleHeaderClickEventArgs> WeekHeaderClick;

		#endregion //WeekHeaderClick

		#endregion //Events

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : long
		{
			IsInitializing = 1L << 0,
			IsChangingView = 1L << 1,
			DoNotChangeVisibleDates = 1L << 2,
			VerifyingCurrentViewControl = 1L << 3,
			InitializingFromDateNavigator = 1L << 4,
			InitializingDateNavigator = 1L << 5,
			CopyingSelectedActivitiesFromViewControl = 1L << 6,
			CopyingSelectedActivitiesToViewControl = 1L << 7,
		}
		#endregion // InternalFlags enum

		#region ICommandTarget Members

		object ICommandTarget.GetParameter( CommandSource source )
		{
			return this.GetParameter(source);
		}

		bool ICommandTarget.SupportsCommand( ICommand command )
		{
			return this.SupportsCommand(command);
		}

		#endregion //ICommandTarget Members

		#region ISelectedActivityCollectionOwner Members

		void ISelectedActivityCollectionOwner.OnSelectedActivitiesChanged()
		{
			// synchronize the view control's activities
			this.CopySelectedActivities(true);

			// then raise the event on this control
			this.OnSelectedActivitiesChanged();
		}

		#endregion //ISelectedActivityCollectionOwner Members

		#region IScheduleControl Members
		void IScheduleControl.OnColorSchemeResolvedChanged()
		{
		}

		void IScheduleControl.OnSettingsChanged(object sender, string property, object extraInfo)
		{
		}

		void IScheduleControl.RefreshDisplay()
		{
			this.InitializeCurrentViewDateInfo();
		}

		void IScheduleControl.VerifyInitialState(List<DataErrorInfo> errorList)
		{
		}
		#endregion //IScheduleControl Members
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