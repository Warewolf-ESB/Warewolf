using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.Specialized;
using Infragistics.Controls.Schedules.Primitives;
using System.Diagnostics;
using Infragistics.Controls.Primitives;
using Infragistics.Collections;
using System.Windows.Input;
using System.Threading;
using System.Windows.Automation.Peers;
using Infragistics.AutomationPeers;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Custom control used to display XamSchedule related information.
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateError, GroupName = VisualStateUtilities.GroupError)]
	[TemplateVisualState(Name = VisualStateUtilities.StateNoError, GroupName = VisualStateUtilities.GroupError)]

	[TemplatePart(Name = PartGroupHeadersPanel, Type = typeof(ScheduleStackPanel))]
	[TemplatePart(Name = PartGroupsPanel, Type = typeof(ScheduleStackPanel))]
	[TemplatePart(Name = PartTimeslotGroupScrollBar, Type = typeof(ScrollBar))]
	[TemplatePart(Name = PartRootPanel, Type = typeof(Grid))]

    
    

	public abstract class ScheduleControlBase : Control
		, IScheduleControl
		, ICommandTarget
		, ISelectedActivityCollectionOwner // AS 12/8/10 NA 11.1 - XamOutlookCalendarView



	{
		#region Member Variables

		private const string PartGroupsPanel = "GroupsPanel";
		private const string PartGroupHeadersPanel = "GroupHeadersPanel";
		private const string PartTimeslotGroupScrollBar = "TimeslotGroupScrollBar";
		internal const string PartRootPanel = "RootPanel";

		internal const string PropNameActualHeight = "ActualHeight";
		internal const string PropNameActualWidth = "ActualWidth";

		internal const string MeasurePanelId = "MeasurePanel";

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		//private const ActivityType InPlaceActivityType = ActivityType.Appointment;

		// template elements
		private ScheduleStackPanel _groupsPanel;
		private ScheduleStackPanel _groupHeadersPanel;
		private ScrollBar _timeslotGroupScrollBar;
		private Grid _rootPanel;

		// fields backing properties
		private VisibleDateCollection _visibleDates;

		private CalendarGroupCollection _calendarGroupsOveride;
		private ReadOnlyNotifyCollection<CalendarGroupBase> _calendarGroupsResolved;
		private CalendarGroupsController _calendarGroupsController;

		// internal members
		private ScheduleTimeslotInfo _timeslotInfo;
		private WeekModeHelper _weekHelper;
		private ObservableCollection<CalendarGroupTimeslotAreaAdapter> _groupTimeslots;
		private ObservableCollection<CalendarHeaderAreaAdapter> _groupHeaders;
		private List<CalendarGroupWrapper> _groupWrappers;

		private PropertyChangeListener<ScheduleControlBase> _listener;
		private DeferredOperation _deferredValidationOperation;
		private InternalFlags _flags = InternalFlags.AllVerifyStateFlags | InternalFlags.AllVerifyUIStateFlags;
		private DateRange? _processedSelectedRange;
		private ScrollBarInfoMediator _groupScrollbarAdapter;
		private XamScheduleDataManager _dataManagerResolved;
		private CalendarBrushProvider _defaultBrushProvider;
		private DateTime? _currentDate;

		private static Binding s_ResourceCalendarBrushVersionBinding = new Binding("BrushVersion");

		private Binding _brushVersionBinding;

		private Canvas _templatePanel;
		private Dictionary<Enum, double> _templateItemExtent;

		/// <summary>
		/// Identifer used as the tag for objects that we create to use for measurement.
		/// </summary>
		internal static readonly object MeasureOnlyItemId = new object();

		private MergedScrollInfo _calendarGroupScrollInfo;
		private WeakSet<TimeslotPanelBase> _timeslotPanels;

		private ScheduleEditHelper _editHelper;
		private DateRange? _cachedSelectedTimeRange;	// cache for the SelectedTimeRange property
		private DateRange? _activitySelectedTimeRange;	// cached copy of the selection range that represents the selected activities. this is lazily calculated
		private DateRange? _pendingSelectedTimeRange;	// value to use for the selected time range if the selected activities are cleared

		private ObservableCollectionExtended<ActivityBase> _extraActivities;
		private ObservableCollectionExtended<ActivityBase> _filteredOutActivities;

		private SelectedActivityCollection _selectedActivities;
		private ActivityDeletionHelper _deletionHelper;
		
		// JJD 12/02/10 - TFS59874
		private ActivityDlgDisplayHelper _dlgHelper;

		private object _verifyToken;

		private CalendarGroupBase _activeGroup;

		private List<TemplateItemSizeChangeHelper> _templateItems;

		private CalendarGroupResizerHost _groupResizer; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling

		private bool _hasPendingInPlaceCreate; // AS 4/14/11 TFS71617

		private Size _lastAvailableSize; // AS 5/4/11 TFS74447

		#endregion //Member Variables

		#region Constructor
		static ScheduleControlBase()
		{

			Style style = new Style();
			style.Seal();
			Control.FocusVisualStyleProperty.OverrideMetadata(typeof(ScheduleControlBase), new FrameworkPropertyMetadata(style));

		}

		/// <summary>
		/// Initializes a new <see cref="ScheduleControlBase"/>
		/// </summary>
		protected ScheduleControlBase()
		{

            Infragistics.Windows.Utilities.ValidateLicense(typeof(ScheduleControlBase), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			_selectedActivities = new SelectedActivityCollection(this);
			_deferredValidationOperation = new DeferredOperation(new Action(this.VerifyState));
			_listener = new PropertyChangeListener<ScheduleControlBase>(this, OnSubObjectChanged);
			_timeslotPanels = new WeakSet<TimeslotPanelBase>();
			_extraActivities = new ExtraActivityCollection();

			// we specifically don't want to listen to item changes or else that could mess up the activity provider
			_filteredOutActivities = new ObservableCollectionExtended<ActivityBase>(false, false);

			_groupWrappers = new List<CalendarGroupWrapper>();
			_groupTimeslots = new ObservableCollection<CalendarGroupTimeslotAreaAdapter>();
			_groupHeaders = new ObservableCollection<CalendarHeaderAreaAdapter>();
			_calendarGroupsResolved = new ReadOnlyNotifyCollection<CalendarGroupBase>(new CalendarGroupBase[0]);
			_calendarGroupsController = new CalendarGroupsController(this);

			_visibleDates = new VisibleDateCollection(this);

			_calendarGroupScrollInfo = new MergedScrollInfo();
			_calendarGroupScrollInfo.DirtyAction = this.OnMergedScrollInfoChanged;
			_groupScrollbarAdapter = new ScrollBarInfoMediator(_calendarGroupScrollInfo);

			this.Loaded += new RoutedEventHandler(OnLoaded);

			_brushVersionBinding = CreateBrushVersionBinding(this);

			_templateItemExtent = new Dictionary<Enum, double>();

			_cachedSelectedTimeRange = (DateRange?)this.GetValue(SelectedTimeRangeProperty);

			_timeslotInfo = this.CreateTimeslotInfo();
			Debug.Assert(null != _timeslotInfo, "TimeslotInfo is required");

			this.VerifyCalendarGroupResizer(); // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling

			// AS 6/12/12 TFS111820
			CommandSourceManager.RegisterCommandTarget(this);
		}
		#endregion //Constructor

		#region Base class overrides

		#region MeasureOverride

		/// <summary>
		/// Invoked to measure the element and its children.
		/// </summary>
		/// <param name="constraint">The size that reflects the available size that this element can give to its children.</param>
		/// <returns>The <see cref="System.Windows.Size"/> that represents the desired size of the element.</returns>
		protected override Size MeasureOverride(Size constraint)
		{
			_lastAvailableSize = constraint; // AS 5/4/11 TFS74447

			return base.MeasureOverride(constraint);
		}

		#endregion //MeasureOverride	

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			#region GroupsPanel
			ScheduleStackPanel oldGroupsPanel = _groupsPanel;

			if (oldGroupsPanel != null)
			{
				oldGroupsPanel.ClearValue(ScheduleControlBase.ControlProperty);
				oldGroupsPanel.ClearValue(ScheduleStackPanel.OrientationProperty);
				oldGroupsPanel.ResizerBarHost = null; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				oldGroupsPanel.Items = null;
				_calendarGroupScrollInfo.Remove(oldGroupsPanel.ScrollInfo);
			}

			_groupsPanel = this.GetTemplateChild(PartGroupsPanel) as ScheduleStackPanel;

			if (null != _groupsPanel)
			{
				_groupsPanel.ResizerBarHost = this.CalendarGroupResizer; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupsPanel.MinItemExtent = this.MinCalendarGroupExtentResolved; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupsPanel.PreferredItemExtent = this.PreferredCalendarGroupExtent; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupsPanel.SetValue(ScheduleControlBase.ControlProperty, this);
				_groupsPanel.Orientation = this.CalendarGroupOrientation;
				_groupsPanel.Items = _groupTimeslots;
				_calendarGroupScrollInfo.Add(_groupsPanel.ScrollInfo);
			}
			#endregion //GroupsPanel

			#region GroupHeadersPanel
			ScheduleStackPanel oldHeaderPanel = _groupHeadersPanel;

			if (oldHeaderPanel != null)
			{
				oldHeaderPanel.ClearValue(ScheduleControlBase.ControlProperty);
				oldHeaderPanel.ClearValue(ScheduleStackPanel.OrientationProperty);
				oldHeaderPanel.ResizerBarHost = null; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				oldHeaderPanel.Items = null;
				_calendarGroupScrollInfo.Remove(oldHeaderPanel.ScrollInfo);
			}

			_groupHeadersPanel = this.GetTemplateChild(PartGroupHeadersPanel) as ScheduleStackPanel;

			if (null != _groupHeadersPanel)
			{
				_groupHeadersPanel.ResizerBarHost = this.CalendarGroupResizer; // AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupHeadersPanel.MinItemExtent = this.MinCalendarGroupExtentResolved; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupHeadersPanel.PreferredItemExtent = this.PreferredCalendarGroupExtent; // AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				_groupHeadersPanel.SetValue(ScheduleControlBase.ControlProperty, this);
				_groupHeadersPanel.Orientation = this.CalendarGroupOrientation;
				_groupHeadersPanel.Items = _groupHeaders;
				_calendarGroupScrollInfo.Add(_groupHeadersPanel.ScrollInfo);
			}

			#endregion //GroupHeadersPanel

			#region RootPanel
			
			XamScheduleDataManager dm = this.DataManager;

			PresentationUtilities.ReparentElement(_templatePanel, dm, false);
			PresentationUtilities.ReparentElement(_rootPanel, _templatePanel, false);

			_rootPanel = this.GetTemplateChild(PartRootPanel) as Grid;


			if (_templatePanel == null)
			{
				_templatePanel = new MeasurePanel();
				// i was going to set the name but Silverlight throws a value not within expected range exception
				//_templatePanel.Name = MeasurePanelName;
				_templatePanel.Tag = MeasurePanelId;

				// pass along the control state
				ScheduleControlBase.SetControl(_templatePanel, this);

				// make sure it doesn't render or hittest
				_templatePanel.Width = 0;
				_templatePanel.Height = 0;
				_templatePanel.IsHitTestVisible = false;
				_templatePanel.RenderTransform = new TranslateTransform { X = -1000, Y = -1000 };
				_templatePanel.Clip = new System.Windows.Media.RectangleGeometry();
			}

			PresentationUtilities.ReparentElement(_rootPanel, _templatePanel, true);

			if (_templateItems == null || _templateItems.Count == 0)
				this.InitializeTemplatePanel(_templatePanel);

			PresentationUtilities.ReparentElement(_templatePanel, dm, true);

			#endregion // RootPanel

			_timeslotGroupScrollBar = this.GetTemplateChild(PartTimeslotGroupScrollBar) as ScrollBar;
			_groupScrollbarAdapter.ScrollBar = _timeslotGroupScrollBar;
			_calendarGroupScrollInfo.Dirty();
			
			this.ChangeVisualState(false);
		}
		#endregion //OnApplyTemplate

		#region OnKeyDown

		/// <summary>
		/// Called when a key is pressed
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			base.OnKeyDown(e);

			if (null != _editHelper && _editHelper.Activity != null)
			{
				_editHelper.OnKeyDown(e);
			}

			if (!e.Handled)
			{
				ProcessKeyDown(e);
			}
		}

		#endregion //OnKeyDown	

		#region OnKeyUp

		/// <summary>
		/// Called when a key is released
		/// </summary>
		/// <param name="e"></param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			base.OnKeyUp(e);

			if (null != _editHelper)
			{
				_editHelper.OnKeyUp(e);
			}
		}

		#endregion //OnKeyUp

		#region OnTextInput

		/// <summary>
		/// Called when text has been entered via the keyboard
		/// </summary>
		/// <param name="e">The event args with the entered text.</param>
		protected override void OnTextInput(TextCompositionEventArgs e)
		{
			base.OnTextInput(e);

			if (this._editHelper != null)
			{
				if (this._editHelper.IsDragging ||
					 this._editHelper.IsResizing)
					return;
			}

			bool hasSpaceOrText = false;

			if (e.Text.Length > 0)
			{
				foreach (char ch in e.Text)
				{
					if (Char.IsWhiteSpace(ch) 
						|| Char.IsLetterOrDigit(ch)
						|| Char.IsPunctuation(ch)
						|| Char.IsSymbol(ch)
						)
					{
						hasSpaceOrText = true;
						break;
					}
				}
			}

			if (!hasSpaceOrText)
				return;

			// AS 3/1/11 NA 2011.1 ActivityTypeChooser
			//ActivityPresenter presenter;
			//
			//if (this.BeginInPlaceCreate(InPlaceActivityType, out presenter))
			//{
			//    string text = e.Text;
			//
			//    // only use the text if it's not a cr-lf
			//    if (!string.IsNullOrEmpty(text) 
			//        && text != Environment.NewLine 
			//        && ( text.Length > 1 || text.IndexOfAny(new char[] { '\r', '\n' }) < 0 )
			//        )
			//    {
			//        presenter.OnTextInputCreation(e);
			//    }
			//
			//    e.Handled = true;
			//}
			Action<ActivityPresenter> action = delegate(ActivityPresenter presenter) 
			{
				string text = e.Text;

				// only use the text if it's not a cr-lf
				if (!string.IsNullOrEmpty(text)
					&& text != Environment.NewLine
					&& (text.Length > 1 || text.IndexOfAny(new char[] { '\r', '\n' }) < 0)
					)
				{
					presenter.OnTextInputCreation(e);
				}
			};
			e.Handled = this.BeginInPlaceCreate(action);
		}

		#endregion //OnTextInput	

		#endregion //Base class overrides

		#region Properties

		#region Public Properties

		#region ActiveCalendar

		private ResourceCalendar _activeCalendar;

		/// <summary>
		/// Identifies the <see cref="ActiveCalendar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ActiveCalendarProperty = DependencyPropertyUtilities.Register("ActiveCalendar",
			typeof(ResourceCalendar), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnActiveCalendarChanged))
			);

		private static void OnActiveCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;

			var calendar = e.NewValue as ResourceCalendar;

			// store the new calendar and hook its change notifications
			ScheduleUtilities.ManageListenerHelper(ref instance._activeCalendar, calendar, instance._listener, false);

			if (null != calendar)
			{
				CalendarGroupBase calendarGroup = null;

				foreach (CalendarGroupBase group in instance.CalendarGroupsResolved)
				{
					if (group.Contains(calendar))
					{
						calendarGroup = group;
						break;
					}
				}

				// make sure its the selected calendar of the group
				if (calendarGroup != null)
					calendarGroup.SelectedCalendar = calendar;

				Debug.Assert(null != calendarGroup || PresentationUtilities.GetTemplatedParent(instance) is XamOutlookCalendarView, "The calendar is not a member of the visible calendars of a group?");
				instance.ActiveGroup = calendarGroup;
			}

			// if the selected activities don't match the active calendar clear the selection
			if (calendar != null &&
				instance.SelectedActivities.Count > 0 &&
				instance._selectedActivities[0].OwningCalendar != calendar)
				instance._selectedActivities.Clear();

			instance.QueueInvalidation(InternalFlags.ActiveCalendarOrGroup);

			// raise the event
			instance.OnActiveCalendarChanged(e.OldValue as ResourceCalendar, calendar);
		}

		/// <summary>
		/// Returns or sets the active <see cref="ResourceCalendar"/>
		/// </summary>
		/// <seealso cref="ActiveCalendarProperty"/>
		public ResourceCalendar ActiveCalendar
		{
			get
			{
				return _activeCalendar;
			}
			set
			{
				this.SetValue(ScheduleControlBase.ActiveCalendarProperty, value);
			}
		}

		#endregion //ActiveCalendar

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region AllowCalendarGroupResizing

		/// <summary>
		/// Identifies the <see cref="AllowCalendarGroupResizing"/> dependency property
		/// </summary>
		public static readonly DependencyProperty AllowCalendarGroupResizingProperty = DependencyPropertyUtilities.Register("AllowCalendarGroupResizing",
			typeof(bool), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnAllowCalendarGroupResizingChanged))
			);

		private static void OnAllowCalendarGroupResizingChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;
			instance.VerifyCalendarGroupResizer();
		}

		internal IResizerBarHost CalendarGroupResizer
		{
			get { return _groupResizer; }
		}

		private void VerifyCalendarGroupResizer()
		{
			bool hasResizer = _groupResizer != null;

			if ( hasResizer != this.AllowCalendarGroupResizing )
			{
				if ( hasResizer )
					_groupResizer = null;
				else
					_groupResizer = new CalendarGroupResizerHost(this);

				this.OnCalendarGroupResizerChanged();
			}
		}

		internal virtual void OnCalendarGroupResizerChanged()
		{
			if ( null != _groupHeadersPanel )
				_groupHeadersPanel.ResizerBarHost = this.CalendarGroupResizer;

			if ( null != _groupsPanel )
				_groupsPanel.ResizerBarHost = this.CalendarGroupResizer;
		}

		/// <summary>
		/// Returns or sets a boolean indicating if the <see cref="PreferredCalendarGroupExtent"/> can be adjusted by the end user via the UI.
		/// </summary>
		/// <seealso cref="AllowCalendarGroupResizingProperty"/>
		public bool AllowCalendarGroupResizing
		{
			get
			{
				return (bool)this.GetValue(ScheduleControlBase.AllowCalendarGroupResizingProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.AllowCalendarGroupResizingProperty, value);
			}
		}

		#endregion //AllowCalendarGroupResizing

		#region BlockingError

		private static readonly DependencyPropertyKey BlockingErrorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"BlockingError",
			typeof(DataErrorInfo),
			typeof(ScheduleControlBase),
			null,
			OnBlockingErrorChanged
		);

		private static void OnBlockingErrorChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = target as ScheduleControlBase;

			instance.UpdateVisualState();
		}

		/// <summary>
		/// Identifies the read-only <see cref="BlockingError"/> dependency property
		/// </summary>
		public static readonly DependencyProperty BlockingErrorProperty = BlockingErrorPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the blocking error if any.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>BlockingError</b> property is set when an error occurs that prevents further functioning of the control.
		/// </para>
		/// </remarks>
		public DataErrorInfo BlockingError
		{
			get
			{
				return (DataErrorInfo)this.GetValue(BlockingErrorProperty);
			}
			private set
			{
				this.SetValue(BlockingErrorPropertyKey, value);
			}
		}

		#endregion // BlockingError

		#region BrushVersion

		/// <summary>
		/// For internal use only
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static readonly DependencyProperty BrushVersionProperty = DependencyPropertyUtilities.RegisterAttached("BrushVersion",
			typeof(int), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(0, new PropertyChangedCallback(OnBrushVersionChanged))
			);

		private static void OnBrushVersionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ICalendarBrushClient brushClient = d as ICalendarBrushClient;

			if (brushClient != null) 
			{
				if (0 != (int)e.NewValue)
					brushClient.OnBrushVersionChanged();

				return;
			}
		}

		/// <summary>
		/// For internal use only
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="BrushVersionProperty"/>
		/// <seealso cref="SetBrushVersion"/>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]

		[AttachedPropertyBrowsableForChildren(IncludeDescendants=false)]

		public static int GetBrushVersion(DependencyObject d)
		{
			return (int)d.GetValue(ScheduleControlBase.BrushVersionProperty);
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static void SetBrushVersion(DependencyObject d, int value)
		{
			d.SetValue(ScheduleControlBase.BrushVersionProperty, value);
		}

		#endregion //BrushVersion
		
		#region CalendarDisplayMode

		private CalendarDisplayMode? _cachedDisplayModeResolved;

		/// <summary>
		/// Identifies the <see cref="CalendarDisplayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarDisplayModeProperty = DependencyPropertyUtilities.Register("CalendarDisplayMode",
			typeof(CalendarDisplayMode?), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCalendarDisplayModeChanged))
			);

		private static void OnCalendarDisplayModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase item = (ScheduleControlBase)d;
			item._cachedDisplayModeResolved = null;
			item.QueueInvalidation(InternalFlags.CalendarGroupsResolvedChanged);
		}

		/// <summary>
		/// Returns or sets the preferred calendar display mode.
		/// </summary>
		/// <seealso cref="CalendarDisplayModeProperty"/>
		public CalendarDisplayMode? CalendarDisplayMode
		{
			get
			{
				return (CalendarDisplayMode?)this.GetValue(ScheduleControlBase.CalendarDisplayModeProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.CalendarDisplayModeProperty, value);
			}
		}

		internal CalendarDisplayMode CalendarDisplayModeResolved
		{
			get
			{
				if (_cachedDisplayModeResolved == null)
				{
					CalendarDisplayMode? mode = this.CalendarDisplayMode;

					if (mode == null)
						mode = this.DefaultCalendarDisplayMode;

					_cachedDisplayModeResolved = mode.Value;
				}

				return _cachedDisplayModeResolved.Value;
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
		/// <see cref="XamScheduleDataManager.CalendarGroups"/>
		/// <see cref="XamScheduleDataManager.CurrentUser"/>
		/// <see cref="XamScheduleDataManager.CurrentUserId"/>
		public CalendarGroupCollection CalendarGroupsOverride
		{
			get
			{
				if (null == _calendarGroupsOveride)
				{
					_calendarGroupsOveride = new CalendarGroupCollection();
					_calendarGroupsOveride.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCalendarGroupsChanged);
					_calendarGroupsOveride.Owner = this.DataManager;
				}

				return _calendarGroupsOveride;
			}
			// AS 6/8/11 TFS76111
			// Having each control maintain it's own CalendarGroupCollection is a problem because while we can try to 
			// synchronize them that would mean that we would try to reinitialize them based on the calendar group 
			// ids. So for example we would end up adding the group that was created by dayview and putting it into 
			// the overrides of monthview but when that happens monthview ends up reinitializing it from the 
			// calendar group ids - or worst because we call reinitialize which does a remove/add this will happen 
			// for all groups and not just the one added/removed. So instead we'll have the outlookcalendarview 
			// keep its own collection and give that to the controls to use. This also simplifies reinitialization 
			// should the outlookcalendarview's template change.
			//
			internal set
			{
				if (value != _calendarGroupsOveride)
				{
					Debug.Assert(PresentationUtilities.GetTemplatedParent(this) is XamOutlookCalendarView, "This is only meant for use in the outlookcalendarview and could cause issues/rooting in other situations");

					if (_calendarGroupsOveride != null)
					{
						_calendarGroupsOveride.CollectionChanged -= new NotifyCollectionChangedEventHandler(OnCalendarGroupsChanged);
					}

					_calendarGroupsOveride = value;

					if (_calendarGroupsOveride != null)
					{
						_calendarGroupsOveride.CollectionChanged += new NotifyCollectionChangedEventHandler(OnCalendarGroupsChanged);
						_calendarGroupsOveride.Owner = this.DataManager;
					}

					this.QueueInvalidation(InternalFlags.CalendarGroupsResolvedChanged);
				}
			}
		}

		private void OnCalendarGroupsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			// if we're not already using this group then verify that we shouldn't be using this
			// since this takes precedence over the datamanager's calendar groups
			if (_calendarGroupsController.CurrentProvider != CalendarGroupsProvider.ControlGroups)
				this.QueueInvalidation(InternalFlags.CalendarGroupsResolvedChanged);
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
		public ReadOnlyNotifyCollection<CalendarGroupBase> CalendarGroupsResolved
		{
			get
			{
				return _calendarGroupsResolved;
			}
		} 
		#endregion // CalendarGroupsResolved

		#region CalendarHeaderAreaVisibilityResolved

		private Visibility _cachedCalendarHeaderAreaVisibilityResolved = Visibility.Visible;

		private static readonly DependencyPropertyKey CalendarHeaderAreaVisibilityResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CalendarHeaderAreaVisibilityResolved",
			typeof(Visibility), typeof(ScheduleControlBase),
			KnownBoxes.VisibilityVisibleBox,
			new PropertyChangedCallback(OnCalendarHeaderAreaVisibilityResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="CalendarHeaderAreaVisibilityResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarHeaderAreaVisibilityResolvedProperty = CalendarHeaderAreaVisibilityResolvedPropertyKey.DependencyProperty;

		private static void OnCalendarHeaderAreaVisibilityResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;

			Visibility newValue = (Visibility)e.NewValue;
			instance._cachedCalendarHeaderAreaVisibilityResolved = newValue;
			instance.OnCalendarHeaderAreaVisibilityResolvedChanged((Visibility)e.OldValue, instance._cachedCalendarHeaderAreaVisibilityResolved);
		}

		internal virtual void OnCalendarHeaderAreaVisibilityResolvedChanged(Visibility oldValue, Visibility newValue)
		{
			foreach (CalendarGroupWrapper wrapper in _groupWrappers)
			{
				wrapper.OnCalendarHeaderAreaVisibilityChanged(newValue != Visibility.Collapsed);
			}
		}

		/// <summary>
		/// Returns a value indicating whether the <see cref="CalendarHeaderArea"/> and group chrome should be displayed.
		/// </summary>
		/// <seealso cref="CalendarHeaderAreaVisibilityResolvedProperty"/>
		public Visibility CalendarHeaderAreaVisibilityResolved
		{
			get
			{
				return _cachedCalendarHeaderAreaVisibilityResolved;
			}
			private set
			{
				this.SetValue(ScheduleControlBase.CalendarHeaderAreaVisibilityResolvedPropertyKey, value);
			}
		}

		#endregion //CalendarHeaderAreaVisibilityResolved

		#region DataManager

		/// <summary>
		/// Identifies the <see cref="DataManager"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DataManagerProperty = DependencyPropertyUtilities.Register("DataManager",
			typeof(XamScheduleDataManager), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDataManagerChanged))
			);

		private static void OnDataManagerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase ctrl = (ScheduleControlBase)d;

			var oldValue = e.OldValue as XamScheduleDataManager;
			var newValue = e.NewValue as XamScheduleDataManager;

			PresentationUtilities.ReparentElement(ctrl._templatePanel, oldValue, false);
			PresentationUtilities.ReparentElement(ctrl._templatePanel, newValue, true);

			// if the datamanager is set to a new instance then push that into the datamanagerresolved 
			// property. if it is cleared then we should pass that along as well since we don't want to 
			// maintain a reference to it. if we need to later we can create one
			ctrl.DataManagerResolved = newValue;

			// call VerifyInitialState on the data manager.
			// Note: this must be done after the ctrl's DataManagerResolved is set above
			if (newValue != null)
			{
				newValue.VerifyInitialState();

				// JM 04-05-11 TFS69614.  Initialize the scrollbar style.
				var scheme = newValue.ColorSchemeResolved as CalendarColorScheme;
				ctrl.ScrollBarStyle = scheme.ScrollBarStyle;
			}
		}

		/// <summary>
		/// Returns or sets the object that provides the activity and resource information that will be displayed by the control.
		/// </summary>
		/// <seealso cref="DataManagerProperty"/>
		public XamScheduleDataManager DataManager
		{
			get
			{
				return (XamScheduleDataManager)this.GetValue(ScheduleControlBase.DataManagerProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.DataManagerProperty, value);
			}
		}
		#endregion //DataManager

		#region DefaultBrushProvider

		private static readonly DependencyPropertyKey DefaultBrushProviderPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("DefaultBrushProvider",
			typeof(CalendarBrushProvider), typeof(ScheduleControlBase),
			null,
			new PropertyChangedCallback(OnDefaultBrushProviderChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="DefaultBrushProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DefaultBrushProviderProperty = DefaultBrushProviderPropertyKey.DependencyProperty;

		private static void OnDefaultBrushProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;

			instance._defaultBrushProvider = e.NewValue as CalendarBrushProvider;
		}

		/// <summary>
		/// Returns the default brush provider for elements that don't have a specific <see cref="ResourceCalendar"/> context (read-only)
		/// </summary>
		/// <seealso cref="DefaultBrushProviderProperty"/>
		public CalendarBrushProvider DefaultBrushProvider
		{
			get
			{
				return this._defaultBrushProvider;
			}
			internal set
			{
				this.SetValue(ScheduleControlBase.DefaultBrushProviderPropertyKey, value);
			}
		}

		#endregion //DefaultBrushProvider

		// AS 3/14/12 Touch Support
		#region IsTouchSupportEnabled

		/// <summary>
		/// Identifies the <see cref="IsTouchSupportEnabled"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsTouchSupportEnabledProperty = DependencyPropertyUtilities.Register("IsTouchSupportEnabled",
			   typeof(bool), typeof(ScheduleControlBase),
			   DependencyPropertyUtilities.CreateMetadata(KnownBoxes.TrueBox, new PropertyChangedCallback(OnIsTouchSupportEnabledChanged))
			   );

		private static void OnIsTouchSupportEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var instance = (ScheduleControlBase)d;

			instance.OnIsTouchSupportEnabledChanged((bool)e.OldValue, (bool)e.NewValue);
		}

		internal virtual void OnIsTouchSupportEnabledChanged(bool oldValue, bool newValue)
		{
		}

		/// <summary>
		/// Returns or sets whether touch support is enabled for this control.
		/// </summary>
		/// <seealso cref="IsTouchSupportEnabledProperty"/>
		public bool IsTouchSupportEnabled
		{
			get
			{
				return (bool)this.GetValue(ScheduleControlBase.IsTouchSupportEnabledProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.IsTouchSupportEnabledProperty, value);
			}
		}

		#endregion //IsTouchSupportEnabled

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region MinCalendarGroupExtent

		private double? _cachedMinCalendarGroupExtentResolved;

		/// <summary>
		/// Identifies the <see cref="MinCalendarGroupExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty MinCalendarGroupExtentProperty = DependencyPropertyUtilities.Register("MinCalendarGroupExtent",
			typeof(double?), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnMinCalendarGroupExtentChanged))
			);

		private static void OnMinCalendarGroupExtentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;
			double oldValue = instance.MinCalendarGroupExtentResolved;
			instance._cachedMinCalendarGroupExtentResolved = (double?)e.NewValue;

			if (oldValue != instance.MinCalendarGroupExtentResolved)
				instance.OnMinCalendarGroupExtentResolvedChanged();
		}

		internal virtual void OnMinCalendarGroupExtentResolvedChanged()
		{
			if (null != _groupHeadersPanel)
				_groupHeadersPanel.MinItemExtent = this.MinCalendarGroupExtentResolved;

			if (null != _groupsPanel)
				_groupsPanel.MinItemExtent = this.MinCalendarGroupExtentResolved;
		}

		internal virtual double DefaultMinCalendarGroupExtent
		{
			// AS 5/25/11 TFS74447
			// Don't go below 1 for the calendar group extent. Essentially what is happening is that we 
			// end up getting into a situation where the horizontal scrollbar is not needed then it is 
			// needed.
			//
			//get { return 0d; }
			get { return 1d; }
		}

		internal double MinCalendarGroupExtentResolved
		{
			get
			{
				// AS 5/25/11 TFS74447
				//return _cachedMinCalendarGroupExtentResolved ?? this.DefaultMinCalendarGroupExtent;
				return Math.Max(_cachedMinCalendarGroupExtentResolved ?? this.DefaultMinCalendarGroupExtent, 1d);
			}
		}

		/// <summary>
		/// Returns or sets the minimum extent (width for horizontally arranged and height for vertically arranged) for a CalendarGroup within the control.
		/// </summary>
		/// <seealso cref="MinCalendarGroupExtentProperty"/>
		public double? MinCalendarGroupExtent
		{
			get
			{
				return (double?)this.GetValue(ScheduleControlBase.MinCalendarGroupExtentProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.MinCalendarGroupExtentProperty, value);
			}
		}

		#endregion //MinCalendarGroupExtent

		// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region PreferredCalendarGroupExtent

		/// <summary>
		/// Identifies the <see cref="PreferredCalendarGroupExtent"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PreferredCalendarGroupExtentProperty = DependencyPropertyUtilities.Register("PreferredCalendarGroupExtent",
			typeof(double), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(double.NaN, new PropertyChangedCallback(OnPreferredCalendarGroupExtentChanged))
			);

		private static void OnPreferredCalendarGroupExtentChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;
			instance.OnPreferredCalendarGroupExtentChanged((double)e.OldValue, (double)e.NewValue);
		}

		internal virtual void OnPreferredCalendarGroupExtentChanged(double oldValue, double newValue)
		{
			if ( null != _groupHeadersPanel )
				_groupHeadersPanel.PreferredItemExtent = newValue;

			if ( null != _groupsPanel )
				_groupsPanel.PreferredItemExtent = newValue;
		}


		/// <summary>
		/// Returns or sets the preferred extent (width for horizontally arranged and height for vertically arranged) CalendarGroups in the control.
		/// </summary>
		/// <remarks>
		/// <p class="body">The PreferredCalendarGroupExtent indicates the extent that each CalendarGroup should be when arranged. If the available 
		/// space is less than the preferred extent, the available space will be used. Also, if <see cref="AllowCalendarGroupResizing"/> is enabled, this property will 
		/// be updated as the user drags the resizer bar at the right edge of the group.</p>
		/// </remarks>
		/// <seealso cref="PreferredCalendarGroupExtentProperty"/>
		public double PreferredCalendarGroupExtent
		{
			get
			{
				return (double)this.GetValue(ScheduleControlBase.PreferredCalendarGroupExtentProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.PreferredCalendarGroupExtentProperty, value);
			}
		}

		#endregion //PreferredCalendarGroupExtent

		#region ScrollBarStyle

		private static readonly DependencyPropertyKey ScrollBarStylePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ScrollBarStyle",
			typeof(Style), typeof(ScheduleControlBase), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ScrollBarStyle"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ScrollBarStyleProperty = ScrollBarStylePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the style to use for scrollbars (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this value is obtained from the <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>.</para>
		/// </remarks>
		/// <seealso cref="ScrollBarStyleProperty"/>
		public Style ScrollBarStyle
		{
			get
			{
				return (Style)this.GetValue(ScheduleControlBase.ScrollBarStyleProperty);
			}
			internal set
			{
				this.SetValue(ScheduleControlBase.ScrollBarStylePropertyKey, value);
			}
		}

		#endregion //ScrollBarStyle

		#region SelectedActivities
    
    		/// <summary>
		/// Returns a collection of activities that are selected.
		/// </summary>
		public SelectedActivityCollection SelectedActivities { get { return _selectedActivities; } }

   		#endregion //SelectedActivities	
    
		#region SelectedTimeRange

		/// <summary>
		/// Identifies the <see cref="SelectedTimeRange"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SelectedTimeRangeProperty = DependencyPropertyUtilities.Register("SelectedTimeRange",
			typeof(DateRange?), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnSelectedTimeRangeChanged))
			);

		private static void OnSelectedTimeRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;

			DateRange? oldValue = (DateRange?)e.OldValue;
			DateRange? newValue = (DateRange?)e.NewValue;

			instance._cachedSelectedTimeRange = newValue;

			if (newValue != null)
				instance._pendingSelectedTimeRange = newValue;

			instance.VerifyIsAllDaySelection();

			if (oldValue.HasValue && newValue.HasValue && oldValue.Value == newValue.Value)
				return;

			// since the selected time range is affected by the visible dates and the customer could be changing the visible dates
			// we'll just set a flag so we know its dirty and process the change asynchronously
			instance.QueueInvalidation(InternalFlags.SelectedTimeRangeChanged);

			// the selected activities and timeslots are mutually exclusive
			if (newValue != null)
				instance.SelectedActivities.Clear();

			// raise the event
			instance.OnSelectedTimeRangeChanged(oldValue, newValue);
		}

		/// <summary>
		/// Returns or sets the selected time range of the control
		/// </summary>
		/// <seealso cref="SelectedTimeRangeProperty"/>
		public DateRange? SelectedTimeRange
		{
			get
			{
				return _cachedSelectedTimeRange;
			}
			set
			{
				this.SetValue(ScheduleControlBase.SelectedTimeRangeProperty, value);
			}
		}

		internal DateRange? SelectedTimeRangeNormalized
		{
			get
			{
				if (_cachedSelectedTimeRange != null)
					return DateRange.Normalize(_cachedSelectedTimeRange.Value);

				return null;
			}
		}
		#endregion //SelectedTimeRange

		#region ShowCalendarCloseButton

		/// <summary>
		/// Identifies the <see cref="ShowCalendarCloseButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowCalendarCloseButtonProperty = DependencyPropertyUtilities.Register("ShowCalendarCloseButton",
			typeof(bool?), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnShowCalendarCloseButtonChanged))
			);

		private static void OnShowCalendarCloseButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;
			instance.QueueInvalidation(InternalFlags.CalendarGroupsChanged);

		}

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
		public bool? ShowCalendarCloseButton
		{
			get
			{
				return (bool?)this.GetValue(ScheduleControlBase.ShowCalendarCloseButtonProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.ShowCalendarCloseButtonProperty, value);
			}
		}

		#endregion //ShowCalendarCloseButton

		#region ShowCalendarOverlayButton

		/// <summary>
		/// Identifies the <see cref="ShowCalendarOverlayButton"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ShowCalendarOverlayButtonProperty = DependencyPropertyUtilities.Register("ShowCalendarOverlayButton",
			typeof(bool?), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnShowCalendarOverlayButtonChanged))
			);

		private static void OnShowCalendarOverlayButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleControlBase instance = (ScheduleControlBase)d;
			instance.QueueInvalidation(InternalFlags.CalendarGroupsChanged);


		}

		/// <summary>
		/// Returns or sets whether an overlay button will be displayed in the <see cref="CalendarHeader"/>
		/// </summary>
		/// <seealso cref="ShowCalendarOverlayButtonProperty"/>
		/// <seealso cref="CalendarHeader"/>
		/// <seealso cref="CalendarHeader.OverlayButtonVisibility"/>
		public bool? ShowCalendarOverlayButton
		{
			get
			{
				return (bool?)this.GetValue(ScheduleControlBase.ShowCalendarOverlayButtonProperty);
			}
			set
			{
				this.SetValue(ScheduleControlBase.ShowCalendarOverlayButtonProperty, value);
			}
		}

		#endregion //ShowCalendarOverlayButton

		#region VisibleCalendarCount

		private static readonly DependencyPropertyKey VisibleCalendarCountPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("VisibleCalendarCount",
			typeof(int), typeof(ScheduleControlBase),
			0, null);

		/// <summary>
		/// Identifies the read-only <see cref="VisibleCalendarCount"/> dependency property
		/// </summary>
		public static readonly DependencyProperty VisibleCalendarCountProperty = VisibleCalendarCountPropertyKey.DependencyProperty;

		/// <summary>mdmd
		/// Returns the total number of viewable calendars in the <see cref="CalendarGroupsResolved"/>
		/// </summary>
		/// <seealso cref="VisibleCalendarCountProperty"/>
		public int VisibleCalendarCount
		{
			get
			{
				return (int)this.GetValue(ScheduleControlBase.VisibleCalendarCountProperty);
			}
			internal set
			{
				this.SetValue(ScheduleControlBase.VisibleCalendarCountPropertyKey, value);
			}
		}

		#endregion //VisibleCalendarCount
		
		#region VisibleDates

		/// <summary>
		/// Returns a collection of the dates that are displayed within the control.
		/// </summary>
		public DateCollection VisibleDates
		{
			get { return _visibleDates; }
		}
		#endregion //VisibleDates

		#endregion //Public Properties

		#region Internal Properties

		#region ActiveGroup
		/// <summary>
		/// Returns the group containing the active calendar.
		/// </summary>
		internal CalendarGroupBase ActiveGroup
		{
			get { return _activeGroup; }
			private set
			{
				if (value != _activeGroup)
				{
					var old = _activeGroup;

					if (_activeGroup is ISupportPropertyChangeNotifications)
						ScheduleUtilities.RemoveListener(_activeGroup as ISupportPropertyChangeNotifications, _listener);

					_activeGroup = value;

					if (_activeGroup is ISupportPropertyChangeNotifications)
						ScheduleUtilities.AddListener(_activeGroup as ISupportPropertyChangeNotifications, _listener, false);

					this.QueueInvalidation(InternalFlags.ActiveCalendarOrGroup);

					this.OnActiveGroupChanged(old, value);
				}
			}
		} 
		#endregion // ActiveGroup

		#region ActivityBeingDragged

		internal ActivityBase ActivityBeingDragged 
		{ 
			get 
			{
				if (null == _editHelper || !_editHelper.IsDragging)
					return null;

				return _editHelper.Activity; 
			} 
		}

		#endregion //ActivityBeingDragged

		#region ActivityBeingResized

		internal ActivityBase ActivityBeingResized 
		{ 
			get 
			{
				if (null == _editHelper || !_editHelper.IsResizing)
					return null;

				return _editHelper.Activity; 
			} 
		}

		#endregion //ActivityBeingResized	
	
		#region ActivityColumnHeight

		private double _activityColumnHeight = double.PositiveInfinity;

		/// <summary>
		/// The height of activity in an activity panel that aligns activity along horizontally arranged timeslots
		/// </summary>
		internal double ActivityColumnHeight
		{
			get { return _activityColumnHeight; }
			set
			{
				if (value != _activityColumnHeight)
				{
					_activityColumnHeight = ScheduleUtilities.Max(0d, value);

					this.QueueInvalidation(InternalFlags.TimeslotPanelExtents);
				}
			}
		}
		#endregion // ActivityColumnHeight

		#region ActivityGutterHeight

		private double _activityGutterHeight = ScheduleActivityPanel.DefaultGutterExtent;

		/// <summary>
		/// The height of activity in an activity panel that aligns activity along horizontally arranged timeslots
		/// </summary>
		internal double ActivityGutterHeight
		{
			get { return _activityGutterHeight; }
			set
			{
				if (value != _activityGutterHeight)
				{
					_activityGutterHeight = ScheduleUtilities.Max(0d, value);

					this.QueueInvalidation(InternalFlags.TimeslotPanelExtents);
				}
			}
		}
		#endregion // ActivityGutterHeight

		
		#region AllowCalendarClosingResolved

		internal bool AllowCalendarClosingResolved
		{
			get
			{
				XamScheduleDataManager dm = this.DataManagerResolved;

				if (dm == null)
					return false;

				ScheduleSettings settings = dm.Settings;

				if (settings == null)
					return true;

				return settings.AllowCalendarClosing;
			}
		}

		#endregion //AllowCalendarClosingResolved

		#region CalendarGroupOrientation
		internal virtual Orientation CalendarGroupOrientation
		{
			get { return Orientation.Horizontal; }
		}
		#endregion // CalendarGroupOrientation

		#region CalendarGroupMergedScrollInfo
		internal MergedScrollInfo CalendarGroupMergedScrollInfo
		{
			get { return _calendarGroupScrollInfo; }
		}
		#endregion // CalendarGroupMergedScrollInfo

		#region CalendarGroupsResolvedSource
		internal CalendarGroupCollection CalendarGroupsResolvedSource
		{
			get { return _calendarGroupsController.CalendarGroupsResolvedSource; }
		}
		#endregion // CalendarGroupsResolvedSource

		// AS 3/6/12 TFS102945
		#region ClearTouchActionQueue
		internal abstract void ClearTouchActionQueue();
		#endregion //ClearTouchActionQueue

		#region Control

		/// <summary>
		/// Identifies the Control attached dependency property
		/// </summary>
		/// <seealso cref="GetControl"/>
		/// <seealso cref="SetControl"/>
		internal static readonly DependencyProperty ControlProperty = DependencyPropertyUtilities.RegisterAttached("Control",
			typeof(ScheduleControlBase), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(null)
			);

		/// <summary>
		/// Gets the value of the attached Control DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="ControlProperty"/>
		/// <seealso cref="SetControl"/>
		internal static ScheduleControlBase GetControl(DependencyObject d)
		{
			return (ScheduleControlBase)d.GetValue(ScheduleControlBase.ControlProperty);
		}

		/// <summary>
		/// Sets the value of the attached Control DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="ControlProperty"/>
		/// <seealso cref="GetControl"/>
		internal static void SetControl(DependencyObject d, ScheduleControlBase value)
		{
			d.SetValue(ScheduleControlBase.ControlProperty, value);
		}

		#endregion //Control

		#region CurrentLogicalDate
		/// <summary>
		/// Returns the calendar date for the today if the date is in view. Otherwise it returns null.
		/// </summary>
		internal DateTime? CurrentLogicalDate
		{
			get { return _currentDate; }
			private set
			{
				if (value != _currentDate)
				{
					_currentDate = value;

					Debug.Assert(this.GetFlag(InternalFlags.IsVerifyingState));

					this.OnCurrentDateChanged();
				}
			}
		}
		#endregion // CurrentLogicalDate

		#region DateInfoProviderResolved

		internal DateInfoProvider DateInfoProviderResolved
		{
			get
			{
				XamScheduleDataManager dm = this.DataManagerResolved;

				if (dm != null)
					return dm.DateInfoProviderResolved;

				return DateInfoProvider.CurrentProvider;
			}
		}

		#endregion //DateInfoProviderResolved	

		#region DefaultCalendarDisplayMode
		internal virtual CalendarDisplayMode DefaultCalendarDisplayMode
		{
			get { return Infragistics.Controls.Schedules.CalendarDisplayMode.Overlay; }
		} 
		#endregion // DefaultCalendarDisplayMode

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

					// make sure to update the calendar groups so it can refetch the calendars from the associated datamanager
					if (null != _calendarGroupsOveride)
						_calendarGroupsOveride.Owner = value;

					// if the datamanager is changing then the calendar groups are as well
					this.QueueInvalidation(InternalFlags.DataManagerChanged | InternalFlags.CalendarGroupsChanged | InternalFlags.CurrentDateChanged | InternalFlags.CalendarGroupsResolvedChanged | InternalFlags.NotifyCurrentUserChange | InternalFlags.ClickToAddEnabledChange );

					if (null != _dataManagerResolved)
					{
						this.DefaultBrushProvider = _dataManagerResolved.ColorSchemeResolved.DefaultBrushProvider;

						Binding binding = CreateBrushVersionBinding(_dataManagerResolved);

						if ( binding != null )
							BindingOperations.SetBinding(this, BrushVersionProperty, binding);

						_dataManagerResolved.BumpBrushVersion();

						if ( DesignerProperties.GetIsInDesignMode(this) )
							this.ClearValue(BlockingErrorPropertyKey);
						else
							this.BlockingError = _dataManagerResolved.BlockingError;
					}
					else
						this.ClearValue(BlockingErrorPropertyKey);
				}
			}
		}

		#endregion //DataManagerResolved

		#region DialogContainerPanel

		internal Panel DialogContainerPanel
		{
			get	{ return _rootPanel; }
		}

		#endregion //DialogContainerPanel

		#region EditHelper
		internal ScheduleEditHelper EditHelper
		{
			get
			{
				if (null == _editHelper)
					_editHelper = new ScheduleEditHelper(this);

				return _editHelper;
			}
		} 
		#endregion // EditHelper

		// AS 3/6/12 TFS102945
		#region EnqueueTouchAction
		internal abstract void EnqueueTouchAction(Action<ScrollInfoTouchAction> action);
		#endregion //EnqueueTouchAction

		#region ExtraActivities
		/// <summary>
		/// Returns a collection of activities that are not part of query results but should be displayed by the control.
		/// </summary>
		internal ObservableCollectionExtended<ActivityBase> ExtraActivities
		{
			get { return _extraActivities; }
		} 
		#endregion // ExtraActivities

		#region FilteredOutActivities
		internal ObservableCollectionExtended<ActivityBase> FilteredOutActivities
		{
			get { return _filteredOutActivities; }
		}
		#endregion // FilteredOutActivities

		#region GetCalendarGroupAutomationScrollInfo

		/// <summary>
		/// Returns the horizontal and vertical scrollinfo instances to be used by the automation peer for the CalendarGroupTimeslotArea.
		/// </summary>
		/// <param name="horizontal">Out parameter set to the horizontal scrollinfo or null</param>
		/// <param name="vertical">Out parameter set to the vertical scrollinfo or null</param>
		internal abstract void GetCalendarGroupAutomationScrollInfo(out ScrollInfo horizontal, out ScrollInfo vertical);

		#endregion // GetCalendarGroupAutomationScrollInfo

		#region GroupHeadersPanel
		internal ScheduleStackPanel GroupHeadersPanel
		{
			get { return _groupHeadersPanel; }
		} 
		#endregion // GroupHeadersPanel

		#region GroupsPanel
		internal ScheduleStackPanel GroupsPanel
		{
			get { return _groupsPanel; }
		} 
		#endregion // GroupsPanel

		// AS 12/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region GroupScrollbarMediator
		internal ScrollBarInfoMediator GroupScrollbarMediator
		{
			get { return _groupScrollbarAdapter; }
		} 
		#endregion // GroupScrollbarMediator

		#region GroupTimeslots
		internal ObservableCollection<CalendarGroupTimeslotAreaAdapter> GroupTimeslots
		{
			get { return _groupTimeslots; }
		}
		#endregion //GroupTimeslots

		#region GroupWrappers
		internal List<CalendarGroupWrapper> GroupWrappers
		{
			get { return _groupWrappers; }
		} 
		#endregion // GroupWrappers

		#region HasDataManagerChanged
		internal bool HasDataManagerChanged
		{
			get { return this.GetFlag(InternalFlags.DataManagerChanged); }
		} 
		#endregion // HasDataManagerChanged

		#region IsAllDaySelection
		internal bool IsAllDaySelection
		{
			get { return this.GetFlag(InternalFlags.IsAllDaySelection); }
		} 
		#endregion // IsAllDaySelection

		#region IsAttachedElement

		/// <summary>
		/// Identifies the IsAttachedElement attached dependency property
		/// </summary>
		/// <seealso cref="GetIsAttachedElement"/>
		/// <seealso cref="SetIsAttachedElement"/>
		internal static readonly DependencyProperty IsAttachedElementProperty = DependencyPropertyUtilities.RegisterAttached("IsAttachedElement",
			typeof(bool), typeof(ScheduleControlBase),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsAttachedElementChanged))
			);

		private static void OnIsAttachedElementChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			TimeslotPanelBase tsp = d as TimeslotPanelBase;

			if ( null != tsp )
			{
				if (object.Equals(e.OldValue, e.NewValue))
					return;

				ScheduleControlBase control = GetControl(d);
				Debug.Assert(control != null, "The IsAttachedElement is changing for an element not associated with a ScheduleControlBase: " + d.ToString());

				if ( null != control )
				{
					if ( true.Equals(e.NewValue) )
						control.OnTimeslotPanelAttached(tsp);
					else
						control.OnTimeslotPanelDetached(tsp);
				}
			}
		}

		internal virtual void OnTimeslotPanelAttached( TimeslotPanelBase panel )
		{
			_timeslotPanels.Add(panel);
		}

		internal virtual void OnTimeslotPanelDetached( TimeslotPanelBase panel )
		{
			_timeslotPanels.Remove(panel);
		}

		/// <summary>
		/// Gets the value of the attached IsAttachedElement DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="IsAttachedElementProperty"/>
		/// <seealso cref="SetIsAttachedElement"/>
		internal static bool GetIsAttachedElement( DependencyObject d )
		{
			return (bool)d.GetValue(ScheduleControlBase.IsAttachedElementProperty);
		}

		/// <summary>
		/// Sets the value of the attached IsAttachedElement DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="IsAttachedElementProperty"/>
		/// <seealso cref="GetIsAttachedElement"/>
		internal static void SetIsAttachedElement( DependencyObject d, bool value )
		{
			d.SetValue(ScheduleControlBase.IsAttachedElementProperty, value);
		}

		#endregion //IsAttachedElement

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region IsClickToAddEnabled
		internal bool IsClickToAddEnabled
		{
			get
			{
				if (this._dataManagerResolved == null)
					return false;

				var settings = _dataManagerResolved.Settings;

				// if click to add is not enabled for any type...
				if (settings != null &&
					settings.AppointmentSettings != null &&
					settings.AppointmentSettings.IsAddViaClickToAddEnabled == false &&
					settings.TaskSettings != null &&
					settings.TaskSettings.IsAddViaClickToAddEnabled != true &&
					settings.JournalSettings != null &&
					settings.JournalSettings.IsAddViaClickToAddEnabled != true)
				{
					return false;
				}

				return true;
			}
		} 
		#endregion //IsClickToAddEnabled

		#region IsInitializing
		internal bool IsInitializing
		{
			get { return this.GetFlag(InternalFlags.IsInitializing); }
		}
		#endregion //IsInitializing

		#region IsVerifyGroupsPending
		/// <summary>
		/// Returns a boolean indicating if the Groups is considered dirty and a verification is pending.
		/// </summary>
		internal bool IsVerifyGroupsPending
		{
			get { return this.GetFlag(InternalFlags.CalendarGroupsChanged); }
		}
		#endregion // IsVerifyGroupsPending

		#region IsVerifyingState
		/// <summary>
		/// Returns a boolean indicating if the VerifyState method is being processed.
		/// </summary>
		internal bool IsVerifyingState
		{
			get { return this.GetFlag(InternalFlags.IsVerifyingState); }
		} 
		#endregion // IsVerifyingState

		#region IsVerifyStateNeeded
		/// <summary>
		/// Returns a boolean indicating if the VerifyStateOverride is needed.
		/// </summary>
		internal virtual bool IsVerifyStateNeeded
		{
			get { return this.GetFlag(InternalFlags.AllVerifyStateFlags) || this.ShouldReinitializeGroups; }
		}
		#endregion // IsVerifyStateNeeded

		#region IsVerifyUIStateNeeded
		/// <summary>
		/// Returns a boolean indicating if some deferred ui state needs to be verified.
		/// </summary>
		internal virtual bool IsVerifyUIStateNeeded
		{
			get { return this.GetFlag(InternalFlags.AllVerifyUIStateFlags); }
		} 
		#endregion // IsVerifyUIStateNeeded

		#region RootPanel

		internal Grid RootPanel { get { return _rootPanel; } }

		#endregion //RootPanel	
    
		// AS 3/6/12 TFS102945
		#region ShouldQueueTouchActions
		internal abstract bool ShouldQueueTouchActions
		{
			get;
		} 
		#endregion //ShouldQueueTouchActions

		#region ShouldReinitializeGroups
		/// <summary>
		/// Used to determine if the InitializeGroup should be re-executed when the <see cref="IsVerifyGroupsPending"/> is false
		/// </summary>
		internal virtual bool ShouldReinitializeGroups
		{
			get { return this.GetFlag(InternalFlags.CurrentDateChanged | InternalFlags.ActiveCalendarOrGroup); }
		}
		#endregion // ShouldReinitializeGroups

		#region ShowCalendarCloseButtonDefault

		internal virtual bool ShowCalendarCloseButtonDefault { get { return true; } }

		#endregion //ShowCalendarCloseButtonDefault	
    
		#region ShowCalendarCloseButtonResolved

		internal bool ShowCalendarCloseButtonResolved
		{
			get
			{
				XamScheduleDataManager dm = this.DataManagerResolved;

				if (dm == null)
					return false;

				
				// check for explicit setting first
				bool? explicitPropSetting = this.ShowCalendarCloseButton;

				if (explicitPropSetting.HasValue)
					return explicitPropSetting.Value;

				ScheduleSettings settings = dm.Settings;

				if (settings != null && settings.AllowCalendarClosing == false)
					return false;

				return ShowCalendarCloseButtonDefault;
			}
		}

		#endregion //ShowCalendarCloseButtonResolved	

		#region ShowCalendarOverlayButtonDefault

		internal virtual bool ShowCalendarOverlayButtonDefault { get { return true; } }

		#endregion //ShowCalendarOverlayButtonDefault	
    
		#region ShowCalendarOverlayButtonResolved

		internal bool ShowCalendarOverlayButtonResolved
		{
			get
			{
				if (this.CalendarDisplayModeResolved != Schedules.CalendarDisplayMode.Overlay)
					return false;

				bool? explicitPropSetting = this.ShowCalendarOverlayButton;

				if (explicitPropSetting.HasValue)
					return explicitPropSetting.Value;

				return ShowCalendarOverlayButtonDefault;
			}
		}

		#endregion //ShowCalendarOverlayButtonResolved	
    
		#region SingleLineActivityHeight
		internal double SingleLineActivityHeight
		{
			get
			{
				double height;
				if (!_templateItemExtent.TryGetValue(ScheduleControlTemplateValue.SingleLineApptHeight, out height))
					height = 20;

				return height;
			}
		}
		#endregion // SingleLineActivityHeight

		#region SupportedActivityTypes
		private ActivityTypes _supportedActivityTypes = ActivityTypes.All;

		internal ActivityTypes SupportedActivityTypes
		{
			get { return _supportedActivityTypes; }
			set
			{
				if (value != _supportedActivityTypes)
				{
					_supportedActivityTypes = value;
					this.QueueInvalidation(InternalFlags.SupportedActivityTypes);
				}
			}
		}
		#endregion // SupportedActivityTypes

		#region SupportsTodayHighlight
		/// <summary>
		/// Returns a boolean indicating if the control is capable of showing a highlight around the current day.
		/// </summary>
		internal virtual bool SupportsTodayHighlight
		{
			get { return false; }
		}
		#endregion // SupportsTodayHighlight

		#region TimeslotAreaActivityFilter
		// note this is primarily here for dayview - there are no implicit updates of 
		// this state for the timeslotareas if you return a different value for this
		internal virtual Predicate<ActivityBase> TimeslotAreaActivityFilter
		{
			get { return null; }
		}
		#endregion // TimeslotAreaActivityFilter 

		#region TimeslotAreaTimeslotExtent

		private double _timeslotAreaTimeslotExtent = 0d;

		internal double TimeslotAreaTimeslotExtent
		{
			get { return _timeslotAreaTimeslotExtent; }
			set
			{
				if (value != _timeslotAreaTimeslotExtent)
				{
					_timeslotAreaTimeslotExtent = ScheduleUtilities.Max(0d, value);

					this.QueueInvalidation(InternalFlags.TimeslotPanelExtents);
				}
			}
		}
		#endregion // TimeslotAreaTimeslotExtent

		#region TimeslotAreaTimeslotOrientation
		/// <summary>
		/// The orientation of timeslots within a <see cref="TimeslotArea"/>
		/// </summary>
		internal virtual Orientation TimeslotAreaTimeslotOrientation
		{
			get { return Orientation.Horizontal; }
		} 
		#endregion // TimeslotAreaTimeslotOrientation

		#region TimeslotInfo
		internal ScheduleTimeslotInfo TimeslotInfo
		{
			get { return _timeslotInfo; }
		}
		#endregion // TimeslotInfo

		#region TimeslotPanels
		internal ICollection<TimeslotPanelBase> TimeslotPanels
		{
			get { return _timeslotPanels; }
		} 
		#endregion // TimeslotPanels

		#region TimeslotGroupOrientation
		/// <summary>
		/// The orientation the <see cref="TimeslotArea"/> instances within a <see cref="CalendarGroupBase"/>
		/// </summary>
		internal Orientation TimeslotGroupOrientation
		{
			get { return this.TimeslotAreaTimeslotOrientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical; }
		} 
		#endregion // TimeslotGroupOrientation

		#region TimeZoneInfoProviderResolved

		internal TimeZoneInfoProvider TimeZoneInfoProviderResolved
		{
			get
			{
				if (this._dataManagerResolved != null)
					return this._dataManagerResolved.TimeZoneInfoProviderResolved;

				return TimeZoneInfoProvider.DefaultProvider;
			}
		}

		#endregion //TimeZoneInfoProviderResolved	

		#region WeekHelper
		internal WeekModeHelper WeekHelper
		{
			get { return _weekHelper; }
			set
			{
				if (value != _weekHelper)
				{
					_weekHelper = value;
					this.QueueInvalidation(InternalFlags.VisibleDatesChanged);
				}
			}
		} 
		#endregion // WeekHelper

		#endregion //Internal Properties

		#endregion //Properties

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

		#region RefreshDisplay

		/// <summary>
		/// Releases the entire element tree so it can be recreated
		/// </summary>
		public virtual void RefreshDisplay()
		{
			DateInfoProvider dateInfo = this.DateInfoProviderResolved;

			// release the old template items
			if ( _templateItems != null )
			{
				var oldItems = _templateItems.ToArray();
				_templateItems = null;

				foreach ( var item in oldItems )
				{
					item.Release();

					if (null != _templatePanel)
						_templatePanel.Children.Remove(item.Source);
				}

				this.QueueInvalidation(InternalFlags.InitializeTemplatePanel);
			}

			dateInfo.ClearCachedValues();

			ScheduleUtilities.RefreshDisplay(_groupsPanel);
			ScheduleUtilities.RefreshDisplay(_groupHeadersPanel);

			_groupWrappers.Clear();
			_groupTimeslots.Clear();
			_groupHeaders.Clear();

			_timeslotInfo.InvalidateWorkingHours();
			_timeslotInfo.InvalidateGroupRanges();

			if (null != _weekHelper)
				_weekHelper.InvalidateWorkingHours();

			this.QueueInvalidation(InternalFlags.AllVerifyStateFlags);
		}

		#endregion //RefreshDisplay	
    
		#region VerifyState
		/// <summary>
		/// Ensures that any pending operations have been processed.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public void VerifyState()
		{
			// cancel the async operation if we have one
			_deferredValidationOperation.CancelPendingOperation();

			Debug.Assert(!this.GetFlag(InternalFlags.IsVerifyingState), "We probably shouldn't be verifying while we are already in the middle of verifying.");
			bool wasVerifying = this.IsVerifyingState;
			this.SetFlag(InternalFlags.IsVerifyingState, true);

			try
			{
				#region InitializeTemplatePanel
				if ( this.GetFlag(InternalFlags.InitializeTemplatePanel) )
				{
					this.SetFlag(InternalFlags.InitializeTemplatePanel, false);

					if ( null != _templatePanel )
					{
						if ( _templateItems == null || _templateItems.Count == 0 )
							this.InitializeTemplatePanel(_templatePanel);
					}
				} 
				#endregion // InitializeTemplatePanel

				// make sure we have a datamanager
				this.VerifyDataManagerResolved();

				XamScheduleDataManager dm = this.DataManagerResolved;

				// Make sure the local and utc tokens are vaild
				if (dm != null)
				{
					dm.VerifyInitialState();
					dm.VerifyTimeZones();
				}
				else
				{
					if (_verifyToken == null && !DesignerProperties.GetIsInDesignMode(this) && VisualTreeHelper.GetChildrenCount(this) > 0)
					{
						DateTime dt = DateTime.UtcNow.AddSeconds(4);

						_verifyToken = TimeManager.Instance.AddTask(dt, this.ProcessVerifyDataManager);
					}
				}

				// make sure the groups are up to date
				_calendarGroupsController.VerifyState();

				CalendarGroupCollection groups = this.CalendarGroupsResolvedSource;

				if (groups != null)
					groups.ProcessPendingChanges();

				#region RestoreSelectedTimeRange
				if ( this.GetFlag(InternalFlags.RestoreSelectedTimeRange) )
				{
					if ( this.SelectedTimeRange == null && this.SelectedActivities.Count == 0 && _pendingSelectedTimeRange != null )
					{
						DateRange pending = _pendingSelectedTimeRange.Value;
						_pendingSelectedTimeRange = null;
						this.RestoreSelection(pending);
					}

					this.SetFlag(InternalFlags.RestoreSelectedTimeRange, false);
				} 
				#endregion // RestoreSelectedTimeRange

				// if there is any dirty state then invoke the verification
				if (this.IsVerifyStateNeeded)
					this.VerifyStateOverride();

				if (this.IsVerifyUIStateNeeded)
					VerifyUIState();
			}
			finally
			{
				this.SetFlag(InternalFlags.IsVerifyingState, wasVerifying);
			}

			// this shouldn't happen but if we still have invalid state then start another deferred operation
			if (this.IsVerifyStateNeeded || this.IsVerifyUIStateNeeded)
			{
				//Debug.Assert(false, "Some flags are dirty  after the processing. This doesn't seem right as anyone expecting the state to be up to date won't be.");





				_deferredValidationOperation.StartAsyncOperation();
			}

			// AS 10/28/10 TFS55791/TFS50143
			this.OnPostVerifyState();
		}
		#endregion // VerifyState
		
		#endregion //Public Methods
    
		#region Internal Methods

		#region ActivateNextPreviousGroup
		internal bool ActivateNextPreviousGroup(bool next, bool page)
		{
			CalendarGroupBase startingGroup = _activeGroup;

			if (_activeGroup == null)
				return false;

			CalendarGroupBase newGroup = null;

			if (!page)
			{
				newGroup = this.GetNextPreviousGroup(_activeGroup, next);
			}
			else
			{
				// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
				// Added support to scroll to the last/first item in view. If the current item is 
				// the first item and paging up or the last item and paging down then get the first/
				// last item on the previous/next page as occurs in outlook.
				//
				//newGroup = next ? this.CalendarGroupsResolved.LastOrDefault(ScheduleUtilities.HasVisibleCalendars) : this.CalendarGroupsResolved.FirstOrDefault(ScheduleUtilities.HasVisibleCalendars);
				var scrollInfo = this.CalendarGroupMergedScrollInfo;

				if ( scrollInfo.Viewport >= scrollInfo.Extent )
				{
					newGroup = next 
						? this.CalendarGroupsResolved.LastOrDefault(ScheduleUtilities.HasVisibleCalendars) 
						: this.CalendarGroupsResolved.FirstOrDefault(ScheduleUtilities.HasVisibleCalendars);
				}
				else
				{
					var visibleCalendars = this.CalendarGroupsResolved.Where(ScheduleUtilities.HasVisibleCalendars).ToList();

					if ( visibleCalendars.Count > 0 )
					{
						int index = visibleCalendars.IndexOf(_activeGroup);

						int firstInViewIndex = (int)scrollInfo.Offset;
						int lastInViewIndex = Math.Max((int)(scrollInfo.Offset + scrollInfo.Viewport - 1), firstInViewIndex);

						// if going up and this is the first group fully in view...
						if ( index == firstInViewIndex && !next )
							index = firstInViewIndex - (int)scrollInfo.Viewport;
						else if ( index == lastInViewIndex && next )
							index = lastInViewIndex + (int)scrollInfo.Viewport;
						else if ( !next )
							index = firstInViewIndex;
						else
							index = lastInViewIndex;

						index = Math.Max(Math.Min(index, visibleCalendars.Count - 1), 0);
						newGroup = visibleCalendars[index];
					}
				}

				if (newGroup == startingGroup)
					return false;
			}

			if (null == newGroup)
				return false;

			return this.SelectCalendar(newGroup.SelectedCalendar, true, newGroup);
		}
		#endregion // ActivateNextPreviousGroup

		#region AdjustVisibleDates
		/// <summary>
		/// Changes the visible dates by a given offset indicated by the parameters
		/// </summary>
		/// <param name="next">True to shift forward; otherwise false to shift back</param>
		/// <param name="shift">True to try and maintain the offset between the visible dates; otherwise false to drive the new visible dates by the offset first visible date</param>
		/// <param name="adjustment">Indicates the type of offset</param>
		/// <returns>Returns true if the visible dates were changed</returns>
		internal virtual bool AdjustVisibleDates(bool next, bool shift, VisibleDateAdjustment adjustment)
		{
			if ( null != this.WeekHelper )
			{
				return this.WeekHelper.AdjustWeeks(next, shift, adjustment == VisibleDateAdjustment.Page);
			}

			var visDates = this.VisibleDates;
			int visDateCount = visDates.Count;

			if (visDateCount == 0)
				return false;

			visDates.Sort();
			DateTime first = visDates[0];
			DateTime last;
			
			switch(adjustment)
			{
				default:
				case VisibleDateAdjustment.SingleItem:
					Debug.Assert(adjustment == VisibleDateAdjustment.SingleItem, "Unexpected adjustment type");
					last = first;
					break;
				case VisibleDateAdjustment.Page:
					last = shift ? visDates[visDateCount - 1] : first.AddDays(visDateCount - 1);
					break;
				case VisibleDateAdjustment.Week:
					last = first.AddDays(6);
					break;
			}

			TimeSpan diff = last.Subtract(first);
			var calendar = this.DateInfoProviderResolved.CalendarHelper;
			int delta = (int)diff.TotalDays;
			DateTime start, end;

			if (next)
			{
				start = calendar.AddDays(last, 1);

				if (!calendar.TryAddOffset(start, delta, CalendarHelper.DateTimeOffsetType.Days, out end))
					start = calendar.AddDays(end, -delta);
			}
			else
			{
				delta++;
				start = calendar.AddDays(first, -delta);
				end = calendar.AddDays(start, delta);
			}

			if (start == first && end == visDates[visDateCount - 1])
				return false;

			List<DateTime> newVisDates = new List<DateTime>();

			if (shift)
			{
				DateTime previousVis = first;

				for (int i = 0; i < visDateCount; i++)
				{
					DateTime oldVisDate = visDates[i];
					start = calendar.AddDays(start, (int)oldVisDate.Subtract(previousVis).TotalDays);
					newVisDates.Add(start);
					previousVis = oldVisDate;
				}
			}
			else
			{
				for (int i = 0; i < visDateCount; i++)
				{
					DateTime tempDate = start.AddDays(i * 1);
					newVisDates.Add(tempDate);
				}
			}

			

			this.VisibleDates.Reinitialize(newVisDates);
			return true;
		}
		#endregion // AdjustVisibleDates

		#region BindToBrushVersion
		internal void BindToBrushVersion(DependencyObject target)
		{
			if (target != null)
			{
				// In SL the creation of the binding in the ctor could have failed due to
				// a bug in the SL framework. Therefore try it again once.
				// If it still fails then don't keep trying
				if (_brushVersionBinding == null && this.GetFlag( InternalFlags.BrushVersionBindingRetryFailed) == false)
				{
					_brushVersionBinding = CreateBrushVersionBinding(this);

					if (_brushVersionBinding == null)
					{
						this.SetFlag(InternalFlags.BrushVersionBindingRetryFailed, true);

						Debug.Assert(false == DesignerProperties.GetIsInDesignMode(this), "Couldn't create a brush version binding");
					}
				}

				if (_brushVersionBinding != null)
					BindingOperations.SetBinding(target, BrushVersionProperty, this._brushVersionBinding);
			}
		}
		#endregion // BindToBrushVersion

		#region BindToResourceCalendarBrushVersion
		internal static void BindToResourceCalendarBrushVersion(DependencyObject target)
		{
			if (target != null)
				BindingOperations.SetBinding(target, BrushVersionProperty, s_ResourceCalendarBrushVersionBinding);
		}
		#endregion // BindToResourceCalendarBrushVersion

		#region BringIntoView( DateTime )
		internal abstract void BringIntoView(DateTime date);
		#endregion // BringIntoView( DateTime )

		#region BringIntoView
		internal ActivityPresenter BringIntoView(ActivityBase activity)
		{
			this.VerifyState();
			this.UpdateLayout();

			foreach (var panel in _timeslotPanels)
			{
				var activityPanel = panel as ScheduleActivityPanel;

				if (activityPanel != null)
				{
					var activityProvider = activityPanel.ActivityProvider;

					if (null != activityProvider && activityProvider.CouldContainActivity(activity))
					{
						activityPanel.BringIntoView(activity);

						activityPanel.UpdateLayout();

						return activityPanel.GetActivityPresenter(activity);
					}
				}
			}

			return null;
		} 
		#endregion // BringIntoView

		#region CalculateIsAllDaySelection
		internal virtual bool CalculateIsAllDaySelection(DateRange? selectedRange)
		{
			if (_cachedSelectedTimeRange == null)
				return false;

			DateRange selection = _cachedSelectedTimeRange.Value;
			selection.Normalize();

			if (selection.End.Subtract(selection.Start).Ticks < TimeSpan.TicksPerSecond)
				return false;

			TimeSpan offset, duration;
			this.GetLogicalDayInfo(out offset, out duration);

			DateRange startDayRange = ScheduleUtilities.GetLogicalDayRange(selection.Start, offset, duration);

			// if its within the range but not the start then its within the day and therefore
			// not an all day selection
			if (startDayRange.ContainsExclusive(selection.Start) && selection.Start != startDayRange.Start)
				return false;

			// this should return a range for the next logical day
			DateRange endDayRange = ScheduleUtilities.GetLogicalDayRange(selection.End, offset, duration);

			// if the logical day for the end (which is exclusive) contains the time but this 
			// is not the start of the next day then this is not an all day
			if (endDayRange.ContainsExclusive(selection.End) && endDayRange.Start != selection.End)
				return false;

			return true;
		}
		#endregion // CalculateIsAllDaySelection

		#region CanExecuteCommand

		internal virtual bool CanExecuteCommand(ScheduleControlCommand command)
		{
			switch (command)
			{
				case ScheduleControlCommand.EditSelectedActivity: // JJD 12/02/10 - TFS59876
				case ScheduleControlCommand.DisplayDialogsForSelectedActivities: // JJD 12/02/10 - TFS59874
				case ScheduleControlCommand.DeleteSelectedActivities:
					return this.SelectedActivities.Count > 0;

				case ScheduleControlCommand.CreateInPlaceActivity: // AS 9/30/10 TFS49593
					return this.SelectedTimeRange != null && this.ActiveCalendar != null;

				case ScheduleControlCommand.Today:
					return true;

				case ScheduleControlCommand.TimeslotBelow:
				case ScheduleControlCommand.TimeslotAbove:
				case ScheduleControlCommand.TimeslotRight:
				case ScheduleControlCommand.TimeslotLeft:
				case ScheduleControlCommand.TimeslotFirstDayOfWeek:
				case ScheduleControlCommand.TimeslotLastDayOfWeek:
				case ScheduleControlCommand.NavigateHome:
				case ScheduleControlCommand.NavigateEnd:
				case ScheduleControlCommand.NavigatePageDown:
				case ScheduleControlCommand.NavigatePageUp:
				case ScheduleControlCommand.ActivityNext:
				case ScheduleControlCommand.ActivityPrevious:
					return this.SelectedTimeRange != null || this.SelectedActivities.Count > 0;

				case ScheduleControlCommand.CalendarGroupNext:
				case ScheduleControlCommand.CalendarGroupPrevious:
				case ScheduleControlCommand.CalendarGroupPageNext:
				case ScheduleControlCommand.CalendarGroupPagePrevious:
				{
					int index = _activeGroup == null ? -1 : this.CalendarGroupsResolved.IndexOf(_activeGroup);
					bool next = command == ScheduleControlCommand.CalendarGroupNext || command == ScheduleControlCommand.CalendarGroupPageNext;
					return null != ScheduleUtilities.FindNextOrDefault(this.CalendarGroupsResolved, index, next, 1, false, ScheduleUtilities.HasVisibleCalendars);
				}

				case ScheduleControlCommand.VisibleDatesShiftPageNext:
				case ScheduleControlCommand.VisibleDatesPageNext:
				case ScheduleControlCommand.VisibleDatesShiftWeekNext:
				case ScheduleControlCommand.VisibleDatesShiftNext:
				{
					var visDates = this.VisibleDates;
					return visDates.Count > 0 && visDates[visDates.Count - 1] < visDates.AllowedRange.End;
				}
				case ScheduleControlCommand.VisibleDatesShiftPagePrevious:
				case ScheduleControlCommand.VisibleDatesPagePrevious:
				case ScheduleControlCommand.VisibleDatesShiftWeekPrevious:
				case ScheduleControlCommand.VisibleDatesShiftPrevious:
				{
					var visDates = this.VisibleDates;
					return visDates.Count > 0 && visDates[0] > visDates.AllowedRange.Start;
				}
			}
			return false;
		}

		#endregion //CanExecuteCommand	
 
		#region ChangeVisualState
		internal virtual void ChangeVisualState(bool useTransitions)
		{
			if (this.BlockingError != null)
				VisualStateManager.GoToState(this, VisualStateUtilities.StateError, useTransitions);
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateNoError, useTransitions);
		}
		#endregion //ChangeVisualState
   
		#region Convert(To|From)LogicalDate
		internal DateTime ConvertToLogicalDate(DateTime date)
		{
			if (this.DataManagerResolved == null)
				return date;

			return date.Subtract(this.DataManagerResolved.LogicalDayOffset);
		}

		internal DateRange ConvertToLogicalDate(DateRange range)
		{
			if (this.DataManagerResolved == null)
				return range;

			TimeSpan offset = this.DataManagerResolved.LogicalDayOffset;

			if (offset.Ticks == 0)
				return range;
			
			DateTime start = range.Start.Subtract(offset);

			if (range.IsEmpty)
				return new DateRange(start);

			return new DateRange(start, range.End.Subtract(offset));
		}

		internal DateTime ConvertFromLogicalDate(DateTime date)
		{
			// AS 12/8/10 Optimization
			//XamScheduleDataManager dm = this.DataManagerResolved;
			//
			//if ( dm == null )
			//    return date;
			//
			//DateTime? adjusteddate = this.DateInfoProviderResolved.Add(date, dm.LogicalDayOffset);
			DateTime? adjusteddate = this.DateInfoProviderResolved.Add(date, this.TimeslotInfo.DayOffset);

			return adjusteddate.HasValue ? adjusteddate.Value : date;
		}

		internal DateRange ConvertFromLogicalDate(DateRange range)
		{
			return new DateRange(this.ConvertFromLogicalDate(range.Start), this.ConvertFromLogicalDate(range.End));
		}
		#endregion // Convert(To|From)LogicalDate

		#region CreateCalendarGroupWrapper
		internal virtual CalendarGroupWrapper CreateCalendarGroupWrapper(CalendarGroupBase calendarGroup)
		{
			return new CalendarGroupWrapper(this, calendarGroup);
		}
		#endregion //CreateCalendarGroupWrapper

		#region CreateHeaderTemplateItem
		internal virtual TimeslotBase CreateHeaderTemplateItem()
		{
			return null;
		}
		#endregion // CreateHeaderTemplateItem

		#region CreateCalendarHeader

		internal virtual CalendarHeader CreateCalendarHeader()
		{
			return new CalendarHeaderHorizontal();
		}

		#endregion //CreateCalendarHeader	

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
		internal virtual TimeslotAreaAdapter CreateTimeslotAreaAdapter(
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

			TimeslotAreaAdapter group = new TimeslotAreaAdapter(this,
				// since the group represents a date, we should track the start and end date
				groupInfo.Start,
				groupInfo.End,
				firstSlotStart,
				lastSlotEnd,
				new TimeslotCollection(creatorFunc, modifyDateFunc, ranges, initializer),
				activityOwner);
			return group;
		}
		#endregion // CreateTimeslotAreaAdapter

		#region CreateTimeslotCalendarGroup

		internal abstract CalendarGroupTimeslotAreaAdapter CreateTimeslotCalendarGroup(CalendarGroupBase calendarGroup);

		#endregion //CreateTimeslotCalendarGroup

		#region CreateTimeslotInfo
		internal abstract ScheduleTimeslotInfo CreateTimeslotInfo(); 
		#endregion // CreateTimeslotInfo

		#region DisplayActivityDialog

		internal abstract bool DisplayActivityDialog(TimeRangePresenterBase element);



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		internal bool DisplayActivityDialog(ResourceCalendar calendar, DateTime startTime, TimeSpan duration, bool isTimeZoneNeutral)
		{
			
			XamScheduleDataManager dm = this.DataManager;

			if (null != dm && calendar != null)
			{
				// AS 1/13/12 TFS77443
				// Do not try to create an activity via the ui for a date that is outside the min/max range.
				//
				DateRange minMaxRange = this.GetMinMaxRange(true);

				if (duration.Ticks == 0)
				{
					if (!minMaxRange.ContainsExclusive(startTime))
						return false;
				}
				else
				{
					DateRange activityRange = new DateRange(startTime, startTime.Add(duration));

					if (!activityRange.Intersect(minMaxRange) || activityRange.IsEmpty)
						return false;

					startTime = activityRange.Start;
					duration = activityRange.End.Subtract(activityRange.Start);
				}

				// if this is a time zone neutral (i.e. floating time) then just adjust the kind - otherwise convert to utc
				if (isTimeZoneNeutral)
					startTime = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
				else
					startTime = ScheduleUtilities.ConvertToUtc(this.TimeZoneInfoProviderResolved.LocalToken, startTime);

				// AS 3/1/11 NA 2011.1 ActivityTypeChooser
				//return dm.DisplayActivityDialog(ActivityType.Appointment, this.DialogContainerPanel, calendar, startTime, duration, isTimeZoneNeutral);
				int allowedCount;
				ActivityTypes types = this.GetNewActivityTypes(calendar, ActivityCreationTrigger.DoubleClick, out allowedCount);

				if (allowedCount == 0)
					return false;
				else if (allowedCount > 1)
				{
					ActivityTypeChooserDialog.ChooserResult result = new ScheduleDialogBase<ActivityType?>.ChooserResult(null);
					Action<bool?> closedCallback = delegate(bool? chooserDialogResult)
					{
						if (chooserDialogResult == true && result.Choice.HasValue)
						{
							dm.DisplayActivityDialog(result.Choice.Value, this.DialogContainerPanel, calendar, startTime, duration, isTimeZoneNeutral);
						}
					};

					return dm.DisplayActivityTypeChooserDialog(
						this.DialogContainerPanel,
						ScheduleUtilities.GetString("DLG_ActivityTypeChooserDialog_Title_NewActivity"), 
						types,
						ActivityTypeChooserReason.AddActivityViaDoubleClick,
						calendar,
						result, 
						null,
						closedCallback);
				}

				return dm.DisplayActivityDialog(ScheduleUtilities.GetActivityType(types).Value, this.DialogContainerPanel, calendar, startTime, duration, isTimeZoneNeutral);
			}

			return false;
		}

		internal bool DisplayActivityDialog(ActivityBase activity)
		{
			Debug.Assert(activity != null, "Null activity in DisplayAppointmentDialog");

			XamScheduleDataManager dm = this.DataManagerResolved;

			if (activity == null || dm == null)
				return false;

			// AS 9/29/10 TFS49543
			// Let the edit helper know that we are about to display the appt dialog. this could 
			// be an addnew inplace activity in which case we should pull the activity out of 
			// in place edit mode
			this.EditHelper.OnDialogDisplaying(activity);

			return dm.DisplayActivityDialog(activity, this.DialogContainerPanel);
		}
		#endregion // DisplayActivityDialog

		#region ExecuteCommand

		internal virtual bool ExecuteCommand(ScheduleControlCommand command)
		{
			if (!this.CanExecuteCommand(command))
				return false;

			switch (command)
			{
				case ScheduleControlCommand.Today:
				{
					return this.GoToToday();
				}

				case ScheduleControlCommand.DisplayDialogsForSelectedActivities:
				{
					// JJD 12/02/10 - TFS59874
					// create an ActivityDlgDisplayHelper to process the activity dialog display
					_dlgHelper = new ActivityDlgDisplayHelper(this);

					//start the display process
					_dlgHelper.ProcessActivities();
					return true;
				}

				case ScheduleControlCommand.EditSelectedActivity:
				{
					// JJD 12/02/10 - TFS59876
					// Begin an in-place edit on the first selected activity
					ActivityPresenter presenter = this.BringIntoView(this._selectedActivities[0]);

					if ( presenter != null )
					{
						this.EditHelper.BeginEdit(presenter);
						return true;
					}

					break;
				}

				case ScheduleControlCommand.DeleteSelectedActivities:
				{
					this.DeleteSelectedActivities();
					return true;
				}

				case ScheduleControlCommand.CreateInPlaceActivity: // AS 9/30/10 TFS49593
				{
					// AS 3/1/11 NA 2011.1 ActivityTypeChooser
					//ActivityPresenter ap;
					//return this.BeginInPlaceCreate(InPlaceActivityType, out ap);
					return this.BeginInPlaceCreate(null);
				}

				case ScheduleControlCommand.ActivityNext:
				case ScheduleControlCommand.ActivityPrevious:
					return this.NavigateActivities(command == ScheduleControlCommand.ActivityNext);

				case ScheduleControlCommand.TimeslotBelow:
					return this.NavigateTimeslot(SpatialDirection.Down);
				case ScheduleControlCommand.TimeslotLeft:
					return this.NavigateTimeslot(SpatialDirection.Left);
				case ScheduleControlCommand.TimeslotRight:
					return this.NavigateTimeslot(SpatialDirection.Right);
				case ScheduleControlCommand.TimeslotAbove:
					return this.NavigateTimeslot(SpatialDirection.Up);

				case ScheduleControlCommand.CalendarGroupNext:
				case ScheduleControlCommand.CalendarGroupPrevious:
					return this.ActivateNextPreviousGroup(command == ScheduleControlCommand.CalendarGroupNext, false);
				case ScheduleControlCommand.CalendarGroupPageNext:
				case ScheduleControlCommand.CalendarGroupPagePrevious:
					return this.ActivateNextPreviousGroup(command == ScheduleControlCommand.CalendarGroupPageNext, true);

				case ScheduleControlCommand.VisibleDatesShiftPageNext:
				case ScheduleControlCommand.VisibleDatesShiftPagePrevious:
					return this.AdjustVisibleDates(command == ScheduleControlCommand.VisibleDatesShiftPageNext, true, VisibleDateAdjustment.Page);

				case ScheduleControlCommand.VisibleDatesPageNext:
				case ScheduleControlCommand.VisibleDatesPagePrevious:
					return this.AdjustVisibleDates(command == ScheduleControlCommand.VisibleDatesPageNext, false, VisibleDateAdjustment.Page);

				case ScheduleControlCommand.VisibleDatesShiftNext:
				case ScheduleControlCommand.VisibleDatesShiftPrevious:
					return this.AdjustVisibleDates(command == ScheduleControlCommand.VisibleDatesShiftNext, true, VisibleDateAdjustment.SingleItem);

				case ScheduleControlCommand.VisibleDatesShiftWeekNext:
				case ScheduleControlCommand.VisibleDatesShiftWeekPrevious:
					return this.AdjustVisibleDates(command == ScheduleControlCommand.VisibleDatesShiftWeekNext, true, VisibleDateAdjustment.Week);

				case ScheduleControlCommand.TimeslotFirstDayOfWeek:
				case ScheduleControlCommand.TimeslotLastDayOfWeek:
					return this.GoFirstLastDayOfWeek(ScheduleControlCommand.TimeslotFirstDayOfWeek == command, false);

				case ScheduleControlCommand.NavigateHome:
				case ScheduleControlCommand.NavigateEnd:
					return this.NavigateHomeEnd(command == ScheduleControlCommand.NavigateHome);

				case ScheduleControlCommand.NavigatePageUp:
				case ScheduleControlCommand.NavigatePageDown:
					return this.NavigatePage(command == ScheduleControlCommand.NavigatePageUp);
			}

			return false;
		}

		#endregion //ExecuteCommand	

		#region GetActivatableDate
		internal DateTime GetActivatableDate(DateTime date)
		{
			DateRange minMaxRange = this.GetMinMaxRange();
			DateTime result = date;

			if (!minMaxRange.Contains(result))
			{
				// update the active date
				if (result < minMaxRange.Start)
					result = minMaxRange.Start;
				else
					result = minMaxRange.End;
			}

			return result;
		}
		#endregion // GetActivatableDate

		#region GetActivatableRange
		// AS 4/1/11 TFS64258
		//internal DateRange GetActivatableRange(DateRange range)
		//{
		//    DateTime start = GetActivatableDate(range.Start);
		//
		//    if (range.IsEmpty)
		//        return new DateRange(start);
		//
		//    return new DateRange(start, GetActivatableDate(range.End));
		//}
		#endregion // GetActivatableRange

		#region GetActivityNavigationList
		internal virtual List<Tuple<ScheduleActivityPanel, DateRange?>> GetActivityNavigationList( CalendarGroupBase group )
		{
			var panels = this.GetActivityPanels(group, true);

			var list = new List<Tuple<ScheduleActivityPanel, DateRange?>>();

			foreach ( var panel in panels )
				list.Add(Tuple.Create(panel, (DateRange?)null));

			return list;
		}
		#endregion // GetActivityNavigationList

		#region GetActivityPanels
		internal List<ScheduleActivityPanel> GetActivityPanels( CalendarGroupBase group, bool sort )
		{
			List<ScheduleActivityPanel> panels = new List<ScheduleActivityPanel>();

			if (group != null)
			{
				foreach (var panel in _timeslotPanels)
				{
					var ap = panel as ScheduleActivityPanel;

					if (ap != null && ap.TimeslotCount > 0)
					{
						var activityProvider = ap.ActivityProvider;

						if (null != activityProvider && activityProvider.ActivityOwner == group)
						{
							panels.Add(ap);
						}
					}
				}

				if (sort)
				{
					Comparison<ScheduleActivityPanel> comparison = delegate( ScheduleActivityPanel ap1, ScheduleActivityPanel ap2 )
					{
						var range1 = ap1.TimeslotRange.Value;
						var range2 = ap2.TimeslotRange.Value;

						// earlier start first
						int result = range1.Start.CompareTo(range2.Start);

						// longer first
						if (result == 0)
							result = range2.End.CompareTo(range1.End);

						// then use the timeslot interval
						if (result == 0)
							result = ap2.TimeslotInterval.CompareTo(ap1.TimeslotInterval);

						return result;
					};
					panels.Sort(comparison);
				}
			}

			return panels;
		}
		#endregion // GetActivityPanels

		#region GetActivityPresenter
		internal ActivityPresenter GetActivityPresenter(ActivityBase activity)
		{
			foreach (TimeslotPanelBase tsPanel in _timeslotPanels)
			{
				ScheduleActivityPanel activityPanel = tsPanel as ScheduleActivityPanel;

				// skip other timeslot panel types
				if (null == activityPanel)
					continue;

				ActivityPresenter ap = activityPanel.GetActivityPresenter(activity);

				if (null != ap)
					return ap;
			}

			return null;
		}
		#endregion // GetActivityPresenter

		#region GetCalendarGroups
		internal IList<CalendarGroupBase> GetCalendarGroups()
		{
			return this.CalendarGroupsResolved;
		}
		#endregion //GetCalendarGroups

		#region GetDefaultSelectedRange

		/// <summary>
		/// Returns a local time range that represents the selection range for the specified logical date.
		/// </summary>
		/// <param name="logicalDate">The logical date whose range is to be returned.</param>
		/// <returns></returns>
		internal abstract DateRange? GetDefaultSelectedRange( DateTime logicalDate );

		#endregion // GetDefaultSelectedRange

		#region GetKeyCommand
		internal virtual ScheduleControlCommand? GetKeyCommand(KeyEventArgs e)
		{
			Key key = PresentationUtilities.GetKey(e);

			switch (key)
			{
				case Key.F2:
					// JJD 12/02/10 - TFS59876
					if ( this.SelectedActivities.Count > 0 )
						return ScheduleControlCommand.EditSelectedActivity;

					break;

				case Key.Enter:
					// AS 9/30/10 TFS49593
					if ( PresentationUtilities.HasNoOtherModifiers(ModifierKeys.None) )
					{
						// JJD 12/02/10 - TFS59874
						if ( this.SelectedActivities.Count > 0 )
							return ScheduleControlCommand.DisplayDialogsForSelectedActivities;

						return ScheduleControlCommand.CreateInPlaceActivity;
					}

					break;

				case Key.Delete:
					return ScheduleControlCommand.DeleteSelectedActivities;

				case Key.Home:
				case Key.End:
					if (PresentationUtilities.IsModifierPressed(ModifierKeys.Alt, ModifierKeys.Control))
					{
						if (key == Key.Home)
							return ScheduleControlCommand.TimeslotFirstDayOfWeek;
						else
							return ScheduleControlCommand.TimeslotLastDayOfWeek;
					}

					if (key == Key.Home)
						return ScheduleControlCommand.NavigateHome;
					else
						return ScheduleControlCommand.NavigateEnd;

				case Key.PageUp:
					return ScheduleControlCommand.NavigatePageUp;
				case Key.PageDown:
					return ScheduleControlCommand.NavigatePageDown;

				case Key.Up:
				case Key.Down:
				case Key.Left:
				case Key.Right:
					{
						if (PresentationUtilities.IsModifierPressed(ModifierKeys.Alt, ModifierKeys.Control))
						{
							if (key == Key.Up)
								return ScheduleControlCommand.VisibleDatesShiftWeekPrevious;
							else if (key == Key.Down)
								return ScheduleControlCommand.VisibleDatesShiftWeekNext;
						}

						if ( PresentationUtilities.HasNoOtherModifiers() )
						{
							switch (key)
							{
								case Key.Up:
									return ScheduleControlCommand.TimeslotAbove;
								case Key.Down:
									return ScheduleControlCommand.TimeslotBelow;
								case Key.Left:
									return ScheduleControlCommand.TimeslotLeft;
								case Key.Right:
									return ScheduleControlCommand.TimeslotRight;
							}
						}
						break;
					}

				case Key.F6:
					{
						if ( PresentationUtilities.HasNoOtherModifiers() )
						{
							return PresentationUtilities.IsModifierPressed(ModifierKeys.Shift)
								? ScheduleControlCommand.CalendarGroupPrevious
								: ScheduleControlCommand.CalendarGroupNext;
						}
						break;
					}

				case Key.Tab:
					{
						if (PresentationUtilities.IsModifierPressed(ModifierKeys.Control, ModifierKeys.Shift))
						{
							return PresentationUtilities.IsModifierPressed(ModifierKeys.Shift, ModifierKeys.Control)
								? ScheduleControlCommand.CalendarGroupPrevious
								: ScheduleControlCommand.CalendarGroupNext;
						}
						else if (PresentationUtilities.HasNoOtherModifiers())
						{
							return PresentationUtilities.IsModifierPressed(ModifierKeys.Shift, ModifierKeys.Control)
								? ScheduleControlCommand.ActivityPrevious
								: ScheduleControlCommand.ActivityNext;
						}
						break;
					}
			}

			return null;
		}
		#endregion // GetKeyCommand

		#region GetLocalVisibleDateRanges
		internal List<DateRange> GetLocalVisibleDateRanges()
		{
			List<DateRange> localRanges = new List<DateRange>();
			var visDates = this.VisibleDates;
			visDates.Sort();
			TimeSpan dayOffset = this.TimeslotInfo.DayOffset;
			TimeSpan dayDuration = this.TimeslotInfo.DayDuration;

			if ( dayDuration.Ticks == 0 )
			{
				DateRange range = new DateRange(visDates[0]);

				for ( int i = 1, count = visDates.Count; i < count; i++ )
				{
					DateTime temp = visDates[i];

					if ( temp.Subtract(range.End).Ticks > TimeSpan.TicksPerDay )
					{
						localRanges.Add(range);
						range = new DateRange(temp);
					}
					else
					{
						range.End = temp;
					}
				}

				localRanges.Add(range);

				// if we have a logical day offset then adjust the ranges
				if ( dayOffset.Ticks != 0 )
				{
					var dateHelper = this.DateInfoProviderResolved;
					TimeSpan endOffset = TimeSpan.FromTicks(dayOffset.Ticks + dayDuration.Ticks);

					for ( int i = 0; i < localRanges.Count; i++ )
					{
						DateRange temp = localRanges[i];
						temp.Start = dateHelper.Add(temp.Start, dayOffset) ?? dateHelper.MaxSupportedDateTime;
						temp.End = dateHelper.Add(temp.End, endOffset) ?? dateHelper.MaxSupportedDateTime;
						localRanges[i] = temp;
					}
				}
			}
			else
			{
				DateRange range = ScheduleUtilities.GetLogicalDayRange(this.ConvertFromLogicalDate(visDates[0]), dayOffset, dayDuration);

				for ( int i = 1, count = visDates.Count; i < count; i++ )
				{
					DateRange tempRange = ScheduleUtilities.GetLogicalDayRange(this.ConvertFromLogicalDate(visDates[i]), dayOffset, dayDuration);

					if ( range.End < tempRange.Start )
					{
						localRanges.Add(range);
						range = tempRange;
					}
					else
					{
						range.End = tempRange.End;
					}
				}

				localRanges.Add(range);
			}

			return localRanges;
		} 
		#endregion // GetLocalVisibleDateRanges

		#region GetLogicalDayInfo
		internal void GetLogicalDayInfo(out TimeSpan offset, out TimeSpan duration)
		{
			offset = TimeSpan.Zero;
			duration = TimeSpan.FromTicks(TimeSpan.TicksPerDay);
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm != null)
			{
				offset = dm.LogicalDayOffset;
				duration = dm.LogicalDayDuration;
			}
		} 
		#endregion // GetLogicalDayInfo

		#region GetMinMaxRange
		internal DateRange GetMinMaxRange()
		{
			return ScheduleUtilities.GetMinMaxRange(this.DataManagerResolved);
		}

		// AS 4/13/11 TFS64258
		internal DateRange GetMinMaxRange(bool convertFromLogicalRange)
		{
			DateRange minMax = this.GetMinMaxRange();

			if (convertFromLogicalRange)
			{
				// AS 6/14/12 TFS112153
				// We should have been using the ConvertFromLogicalDate method and not the GetLogicalDayRange.
				//
				//TimeSpan dayOffset, dayDuration;
				//this.GetLogicalDayInfo(out dayOffset, out dayDuration);
				//minMax.Start = ScheduleUtilities.GetLogicalDayRange(minMax.Start, dayOffset, dayDuration).Start;
				//minMax.End = ScheduleUtilities.GetLogicalDayRange(minMax.End, dayOffset, dayDuration).End;
				minMax = this.ConvertFromLogicalDate(minMax);
				// AS 6/22/12 TFS115359
				//minMax.End = new DateTime(Math.Min(minMax.End.Ticks + this.DataManagerResolved.LogicalDayDuration.Ticks, DateTime.MaxValue.Ticks));
				minMax.End = new DateTime(Math.Min(minMax.End.Ticks + this.TimeslotInfo.DayDuration.Ticks, DateTime.MaxValue.Ticks));
			}

			return minMax;
		}
		#endregion // GetMinMaxRange

		#region GetNavigationTimeslot
		internal DateRange GetNavigationTimeslot( SpatialDirection direction, bool extendSelection, DateRange source )
		{
			DateRange newSelectionSource;

			if (extendSelection)
			{
				// get the timeslot that represents the anchor. the anchor will be the start 
				// of the selection whether that was done by dragging left to right or right 
				// to left
				bool startIsAnchor = source.Start <= source.End;

				// perform a navigation in a given direction but the range we want to start 
				// with will depend on whether the end is before or after the anchor and the 
				// direction we are traversing. i.e. 
				// * if going near and we are continuing in the near direction then use the selection start.
				// * if going near and we have selection beyond the anchor point use the selection end.
				// * if going far and we are continuing in the far direction then use the selection end.
				// * if going far and we have selection beyond the anchor point use the selection start.
				newSelectionSource = this.GetTimeslotFromSelection(source, startIsAnchor == false);
			}
			else
			{
				// when not manipulating the selection then we just want to move relative to the 
				// closest selected timeslot in that direction. e.g. the top when moving up
				newSelectionSource = DateRange.Normalize(source);

				bool fromStart = direction == SpatialDirection.Left || direction == SpatialDirection.Up;

				newSelectionSource = this.GetTimeslotFromSelection(source, fromStart);
			}
			return newSelectionSource;
		} 
		#endregion // GetNavigationTimeslot

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region GetNewActivityTypes
		internal ActivityTypes GetNewActivityTypes(ResourceCalendar calendar, ActivityCreationTrigger trigger, out int count)
		{
			ActivityTypes types = ActivityTypes.None;
			count = 0;

			if (_dataManagerResolved != null && calendar != null)
			{
				if (_dataManagerResolved.IsActivityOperationAllowedHelper(ActivityType.Appointment, trigger, calendar))
				{
					types |= ActivityTypes.Appointment;
					count++;
				}

				if (_dataManagerResolved.IsActivityOperationAllowedHelper(ActivityType.Task, trigger, calendar))
				{
					types |= ActivityTypes.Task;
					count++;
				}

				if (_dataManagerResolved.IsActivityOperationAllowedHelper(ActivityType.Journal, trigger, calendar))
				{
					types |= ActivityTypes.Journal;
					count++;
				}
			}

			return types;
		}
		#endregion //GetNewActivityTypes

		#region GetNowTimeAdjustment
		internal virtual TimeSpan GetNowTimeAdjustment()
		{
			return TimeSpan.Zero;
		} 
		#endregion // GetNowTimeAdjustment

		#region GetParameter
		internal virtual object GetParameter(CommandSource source)
		{
			if (source.Command is ScheduleControlCommandBase)
				return this;

			return null;
		}
		#endregion // GetParameter

		// AS 3/7/12 TFS102945
		#region GetScrollModeFromPoint
		internal virtual TouchScrollMode GetScrollModeFromPoint(FrameworkElement scrollAreaElement, Point point, UIElement elementDirectlyOver)
		{
			bool isInEditMode = _editHelper != null && _editHelper.IsInPlaceEdit;
			bool shouldEnumerateAncestors = isInEditMode;

			if (shouldEnumerateAncestors)
			{
				DependencyObject ancestor = elementDirectlyOver;

				while (ancestor != null && ancestor != scrollAreaElement)
				{
					var fe = ancestor as FrameworkElement;

					// if the touch is within the edit area and the editor is in edit mode
					// then do not scroll since we cannot defer the operation
					if (isInEditMode && fe != null && fe.Name == ActivityPresenter.EditArea)
					{
						var ap = PresentationUtilities.GetTemplatedParent(fe) as ActivityPresenter;

						if (null != ap && ap.IsInEditMode)
							return TouchScrollMode.None;
					}

					ancestor = VisualTreeHelper.GetParent(ancestor) as DependencyObject;
				}
			}

			return TouchScrollMode.Both;
		}
		#endregion //GetScrollModeFromPoint

		#region GetSelectedTimeRangeForNavigation
		internal DateRange? GetSelectedTimeRangeForNavigation()
		{
			var range = this.SelectedTimeRange;

			if (range == null)
			{
				range = this.GetActivitySelectedTimeRange();
			}

			return range;
		}
		#endregion // GetSelectedTimeRangeForNavigation

		#region GetSelectionAnchor

		/// <summary>
		/// Returns the range that represents the timeslot containing the anchor point for the current selected time range.
		/// </summary>
		internal DateRange? GetSelectionAnchor()
		{
			return this.GetSelectionAnchor(this.GetSelectedTimeRangeForNavigation());
		}

		internal DateRange? GetSelectionAnchor(DateRange? selection)
		{
			if (selection == null)
				return null;

			DateRange source = selection.Value;
			bool startIsAnchor = source.Start <= source.End;
			return this.GetTimeslotFromSelection(source, startIsAnchor);
		}
		#endregion // GetSelectionAnchor

		#region GetTimeslotFromSelection
		/// <summary>
		/// Return the timeslot that represents the start or end of the current timeslot selection.
		/// </summary>
		/// <param name="selection">The selection range to evaluate</param>
		/// <param name="fromStart">True to get the starting timeslot for the selection; otherwise false to get the ending timeslot for the selection</param>
		/// <returns>The range that represents the timeslot for the start/end of the range</returns>
		internal virtual DateRange GetTimeslotFromSelection(DateRange selection, bool fromStart)
		{
			selection.Normalize();

			DateTime date = fromStart || selection.IsEmpty ? selection.Start : ScheduleUtilities.GetNonInclusiveEnd(selection.End);

			DateRange? range = this.TimeslotInfo.GetTimeslotRange(date);

			Debug.Assert(null != range, "Unable to get range for specified date?");

			if (range == null)
				return selection;

			return range.Value;
		}
		#endregion // GetTimeslotFromSelection

		#region GetTimeslotRangeInDirection
		/// <summary>
		/// Returns a DateRange that represents the timeslot range for the specified selection range in the specified direction.
		/// </summary>
		/// <param name="selectionRange">The current selection range to start from</param>
		/// <param name="extendSelection">True if the range will be used to extend the selection</param>
		/// <param name="direction">The direction of the navigation</param>
		/// <returns>The range that should be navigated to</returns>
		internal abstract DateRange? GetTimeslotRangeInDirection(SpatialDirection direction, bool extendSelection, DateRange selectionRange);
		#endregion // GetTimeslotRangeInDirection

		#region GetResolvedCalendarHeaderAreaVisibility
		internal virtual Visibility GetResolvedCalendarHeaderAreaVisibility()
		{
			CalendarDisplayMode displayMode = this.CalendarDisplayModeResolved;

			if (displayMode == Schedules.CalendarDisplayMode.Overlay && this.VisibleCalendarCount == 1)
				return Visibility.Collapsed;

			if (displayMode == Schedules.CalendarDisplayMode.Merged)
				return Visibility.Collapsed;

			return Visibility.Visible;
		}
		#endregion // GetResolvedCalendarHeaderAreaVisibility

		#region GetTemplateItemExtent
		internal double GetTemplateItemExtent(System.Enum itemId)
		{
			double value;

			if (!_templateItemExtent.TryGetValue(itemId, out value))
				value = double.NaN;

			return value;
		}
		#endregion // GetTemplateItemExtent

		#region GetTimeslotScrollInfo
		/// <summary>
		/// Returns the scrollinfo for a given panel whose offset may be updated to scroll the timeslots.
		/// </summary>
		/// <param name="panel">Panel for which the scroll info is being requested</param>
		internal virtual ScrollInfo GetTimeslotScrollInfo(TimeslotPanelBase panel)
		{
			return null;
		} 
		#endregion // GetTimeslotScrollInfo

		#region GetVisibleDatesForRange
		internal IList<DateTime> GetVisibleDatesForRange( DateRange dateRange )
		{
			return _visibleDates.GetAllowedDatesInternal(dateRange);
		}
		#endregion // GetVisibleDatesForRange

		#region GoFirstLastDayOfWeek
		internal bool GoFirstLastDayOfWeek(bool first, bool checkModifiers)
		{
			
			DateRange? selection = this.GetSelectedTimeRangeForNavigation();
			DateRange? selectionAnchor = this.GetSelectionAnchor(selection);

			if (null == selectionAnchor)
				return false;

			var calendarHelper = this.DateInfoProviderResolved.CalendarHelper;
			bool extendSelection = checkModifiers && PresentationUtilities.IsModifierPressed(ModifierKeys.Shift, ModifierKeys.Control);

			if (this.WeekHelper != null)
			{
				bool isCtrl = checkModifiers && PresentationUtilities.IsModifierPressed(ModifierKeys.Control, ModifierKeys.Shift);

				DateTime? newSelection = this.WeekHelper.GetFirstLastDayInWeek(first, isCtrl ? (DateTime?)null : selectionAnchor.Value.Start);

				if (newSelection == null)
					return false;

				DateTime day = newSelection.Value;

				return this.SetSelectedTimeRange(new DateRange(day, calendarHelper.AddDays(day, 1)), extendSelection);
			}
			else
			{
				var visDates = this.VisibleDates;

				if (visDates.Count == 0)
					return false;

				visDates.Sort();
				DayOfWeek? firstDayOfWeek = ScheduleUtilities.GetFirstDayOfWeek(this.DataManagerResolved);

				int additionalOffset;
				DateTime logicalDate = this.ConvertToLogicalDate(selectionAnchor.Value.Start);
				DateTime weekDate = calendarHelper.GetFirstDayOfWeekForDate(logicalDate, firstDayOfWeek, out additionalOffset);

				// if navigating to the last day of week...
				if (!first)
					weekDate = weekDate.AddDays(6 - additionalOffset);

				DateTime weekDateLocal = this.ConvertFromLogicalDate(weekDate);
				DateRange weekDateRange = new DateRange(weekDateLocal, calendarHelper.AddDays(weekDate, 1));

				if (!visDates.Contains(weekDate))
				{
					
					this.UpdateVisibleDatesForSelectedRangeOverride(selection, weekDateRange);
				}

				DateRange range = this.GetTimeslotFromSelection(weekDateRange, first);
				return this.SetSelectedTimeRange(range, extendSelection);
			}
		}
		#endregion // GoFirstLastDayOfWeek

		#region GoToToday
		internal virtual bool GoToToday()
		{
			DateTime date = this.GetActivatableDate(DateTime.Today);
			DateRange? newSelection = this.GetDefaultSelectedRange(date);

			if ( newSelection == null )
				return false;

			if ( this.WeekHelper != null )
			{
				this.WeekHelper.Reset(date);
			}

			this.SetSelectedTimeRange(newSelection.Value, false, false);
			return true;
		}
		#endregion // GoToToday

		#region InitializeActivityProviders
		internal virtual void InitializeActivityProviders(CalendarGroupWrapper wrapper)
		{
			foreach (TimeslotAreaAdapter area in wrapper.GroupTimeslotArea.TimeslotAreas)
			{
				area.ActivityProvider.ActivityTypes = _supportedActivityTypes;
			}
		}
		#endregion // InitializeActivityProviders

		// AS 3/13/12 Touch Support
		#region InitializeTouchScrollAreaHelper
		internal void InitializeTouchScrollAreaHelper(ScrollInfoTouchHelper touchHelper, FrameworkElement scrollAreaElement)
		{
			if (touchHelper != null)
			{
				if (touchHelper.ScrollAreaElement != scrollAreaElement)
				{
					// if the area is changing we should try to avoid providing an area 
					// and then disabling so we'll remove the old element first and then 
					// verify the IsEnabled is in sync and then provide the new scroll area
					touchHelper.ScrollAreaElement = null;
					touchHelper.IsEnabled = this.IsTouchSupportEnabled;
					touchHelper.ScrollAreaElement = scrollAreaElement;
				}
				else
				{
					// just make sure the isenabled is in sync
					touchHelper.IsEnabled = this.IsTouchSupportEnabled;
				}
			}
		} 
		#endregion //InitializeTouchScrollAreaHelper

		#region InitializeTemplatePanel
		internal virtual void InitializeTemplatePanel(Canvas templatePanel)
		{
			#region SingleLineApptHeight

			Appointment appt = new Appointment()
			{
				Id = "0",
				Subject = "Subject",
				Description = "Description",
				Location = "Location",
				Start = new DateTime(2010, 1, 1, 9, 0, 0),
				End = new DateTime(2010, 1, 1, 9, 30, 0)
			};

			ISupportRecycling activityAdapter = new ActivityBaseAdapter(appt);
			ActivityPresenter ap = activityAdapter.CreateInstanceOfRecyclingElement() as ActivityPresenter;
			activityAdapter.OnElementAttached(ap);
			ap.IsSingleLineDisplay = true;
			ap.Tag = ScheduleControlBase.MeasureOnlyItemId;

			templatePanel.Children.Add(ap);

			this.TrackTemplateItem(ap, null, ScheduleControlTemplateValue.SingleLineApptHeight);

			#endregion // SingleLineApptHeight

			#region MoreActivityIndicatorHeight

			MoreActivityIndicator downIndicator = new MoreActivityIndicator();
			downIndicator.Direction = MoreActivityIndicatorDirection.Down;
			templatePanel.Children.Add(downIndicator);
			downIndicator.Tag = ScheduleControlBase.MeasureOnlyItemId;

			this.TrackTemplateItem(downIndicator, null, ScheduleControlTemplateValue.MoreActivityIndicatorHeight); 

			#endregion // MoreActivityIndicatorHeight
		}

		#endregion // InitializeTemplatePanel

		#region InvalidateCalendarGroups
		internal void InvalidateCalendarGroups()
		{
			this.QueueInvalidation(InternalFlags.CalendarGroupsChanged);
		}
		#endregion // InvalidateCalendarGroups

		// AS 5/8/12 TFS108279
		#region InvalidateTimeslotPanelExtents
		internal void InvalidateTimeslotPanelExtents()
		{
			this.QueueInvalidation(InternalFlags.TimeslotPanelExtents);
		} 
		#endregion //InvalidateTimeslotPanelExtents

		#region IsInitialActivityNavigationPanel
		internal virtual bool IsInitialActivityNavigationPanel( ScheduleActivityPanel panel, DateRange timeslotRange )
		{
			var tsRange = panel.TimeslotRange;

			if (tsRange == null)
				return false;

			return tsRange.Value.Contains(timeslotRange.Start);
		}
		#endregion // IsInitialActivityNavigationPanel

		#region IsTimeslotRangeGroupBreak
		/// <summary>
		/// Used by the <see cref="TimeslotInfo"/> to determine if 2 visible dates are considered to be part of the same group of timeslots.
		/// </summary>
		/// <param name="previousDate">Previous date processed</param>
		/// <param name="nextDate">Next date to be processed</param>
		/// <returns></returns>
		internal abstract bool IsTimeslotRangeGroupBreak(DateTime previousDate, DateTime nextDate);
		#endregion // IsTimeslotRangeGroupBreak

		#region NavigateActivities
		internal virtual bool NavigateActivities( bool next )
		{
			int selectedActivityCount = this.SelectedActivities.Count;

			
			if (selectedActivityCount == 0 && this.SelectedTimeRange == null)
				return false;

			
			var startingGroup = this.ActiveGroup ?? this.CalendarGroupsResolved.FirstOrDefault(ScheduleUtilities.HasVisibleCalendars);

			if (startingGroup == null)
				return false;

			Debug.Assert(selectedActivityCount == 0 || startingGroup.Contains(this.SelectedActivities[0].OwningCalendar), "We have selected activities but they're not in the active calendar/group");

			// Get the activity panels for the ActiveGroup
			var panels = this.GetActivityNavigationList(startingGroup);

			ActivityBase activityToSelect = null;
			int startingPanelIndex = -1;
			ScheduleActivityPanel containingPanel = null;

			if (selectedActivityCount == 0)
			{
				#region Based on Selection Timeslot

				var selectionRange = this.SelectedTimeRange;
				DateRange timeslotRange = this.GetTimeslotFromSelection(selectionRange.Value, true);

				for (int i = 0, count = panels.Count; i < count; i++)
				{
					var panel = panels[i];

					if (this.IsInitialActivityNavigationPanel(panel.Item1, timeslotRange))
					{
						startingPanelIndex = i;
						break;
					}
				}

				Debug.Assert(startingPanelIndex >= 0, "We weren't able to find a panel that we should start with?");

				if (startingPanelIndex < 0)
					return false;

				int start = startingPanelIndex;
				int end = next ? panels.Count : -1;
				int adjustment = next ? 1 : -1;

				for (int i = start; i != end; i += adjustment)
				{
					var item = panels[i];

					activityToSelect = item.Item1.GetNextActivity(timeslotRange.Start, item.Item2, next);

					if ( activityToSelect != null )
					{
						containingPanel = item.Item1;
						break;
					}
				} 
				#endregion // Based on Selection Timeslot
			}
			else
			{
				
				ActivityBase activity = this.SelectedActivities[0];
				bool foundActivity = false;

				for (int i = 0, count = panels.Count; i < count; i++)
				{
					var item = panels[i];
					activityToSelect = item.Item1.GetNextActivity(activity, next, item.Item2, out foundActivity);

					// once we found a panel with the activity if we look at other panes we don't need
					// to pass along the activity but can start with the panel's first/last activity
					if (foundActivity)
					{
						if ( activityToSelect != null )
							containingPanel = item.Item1;

						startingPanelIndex = i;
						activity = null;
						break;
					}
				}

				// if we didn't find it in the source panel then work forward/back from that panel
				if (null == activityToSelect && foundActivity)
				{
					do
					{
						int adjustment = next ? 1 : -1;
						startingPanelIndex += adjustment;
						int end = next ? panels.Count : -1;

						for ( int i = startingPanelIndex; i != end; i += adjustment )
						{
							var item = panels[i];

							activityToSelect = item.Item1.GetNextActivity(activity, next, item.Item2, out foundActivity);

							if ( null != activityToSelect )
							{
								containingPanel = item.Item1;
								break;
							}
						}

						if ( null != activityToSelect )
							break;

						// move to the next group
						startingGroup = this.GetNextPreviousGroup(startingGroup, next);
						panels = this.GetActivityNavigationList(startingGroup);
						startingPanelIndex = next ? -1 : panels.Count;

					} while ( startingGroup != null );
				}
			}

			if (activityToSelect == null)
				return false;

			// AS 11/1/10 TFS58843
			// End edit mode if we are currently in an inplace edit.
			//
			if ( !this.EditHelper.TryEndInPlaceEdit() )
				return false;

			// make sure the associated calendar is activated
			if ( activityToSelect.OwningCalendar != this.ActiveCalendar )
			{
				// select/activate the calendar
				if ( !this.SelectCalendar(activityToSelect.OwningCalendar, true, startingGroup) )
					return false;
			}

			bool result = this.SelectActivity(activityToSelect, false);

			if ( result )
			{
				if ( null != containingPanel )
				{
					containingPanel.BringIntoView(activityToSelect, true);
				}
			}

			return result;
		}
		#endregion // NavigateActivities

		#region NavigateTimeslot
		internal virtual bool NavigateTimeslot(SpatialDirection direction)
		{
			DateRange? selection = this.GetSelectedTimeRangeForNavigation();

			if (selection == null)
				return false;

			ScheduleUtilities.AdjustForFlowDirection(this.FlowDirection, ref direction);

			bool extendSelection = PresentationUtilities.IsModifierPressed(ModifierKeys.Shift);

			DateRange newSelectionSource = GetNavigationTimeslot(direction, extendSelection, selection.Value);

			DateRange? range = this.GetTimeslotRangeInDirection(direction, extendSelection, newSelectionSource);

			if (range == null)
				return false;

			// AS 4/13/11 TFS64258
			// Ensure that the timeslot we're navigating to is actually within the allowed range.
			//
			DateRange minMaxRange = this.GetMinMaxRange(true);

			if (!minMaxRange.Contains(range.Value))
			{
				// if it's completely outside the valid range then ignore it
				if (!minMaxRange.IntersectsWithExclusive(range.Value))
					return false;

				// we still need to make sure its in the valid range
				DateRange rangeValue = range.Value;
				if (!rangeValue.Intersect(minMaxRange))
					return false;

				if (range.Value.Start > range.Value.End)
					rangeValue = new DateRange(rangeValue.End, rangeValue.Start);
			}

			return this.SetSelectedTimeRange(range.Value, extendSelection);
		}
		#endregion // NavigateTimeslot

		#region NavigateHomeEnd
		internal abstract bool NavigateHomeEnd(bool home);
		#endregion // NavigateHomeEnd

		#region NavigatePage
		internal abstract bool NavigatePage(bool up);
		#endregion // NavigatePage

		#region OnActiveGroupChanged
		internal virtual void OnActiveGroupChanged(CalendarGroupBase oldGroup, CalendarGroupBase newGroup)
		{
			// AS 11/15/10 NA 11.1 - CalendarGroup Sizing/Scrolling
			// Bring the active group into view.
			//
			if ( newGroup != null )
			{
				var scrollInfo = this.CalendarGroupMergedScrollInfo;
				var visibleCalendars = this.CalendarGroupsResolved.Where(ScheduleUtilities.HasVisibleCalendars).ToList();
				int index = visibleCalendars.IndexOf(newGroup);

				if (index < scrollInfo.Offset || index >= (int)(scrollInfo.Offset + scrollInfo.Viewport))
					this.CalendarGroupMergedScrollInfo.Offset = index;
			}
		} 
		#endregion // OnActiveGroupChanged

		#region OnCurrentDateChanged
		internal virtual void OnCurrentDateChanged()
		{
		}
		#endregion // OnCurrentDateChanged

		#region OnDayHeaderClicked
		internal virtual void OnDayHeaderClicked(DayHeaderBase header)
		{
		} 
		#endregion // OnDayHeaderClicked

		#region OnIsAllDaySelectionChanged
		internal virtual void OnIsAllDaySelectionChanged()
		{
		}
		#endregion // OnIsAllDaySelectionChanged

		#region OnMergedScrollInfoChanged
		internal void OnMergedScrollInfoChanged()
		{
			this.QueueInvalidation(InternalFlags.MergedScrollInfosChanged);
		} 
		#endregion // OnMergedScrollInfoChanged

		// AS 11/2/10 TFS58663
		#region OnMinMaxDatesChanged
		internal virtual void OnMinMaxDatesChanged()
		{
			// AS 4/21/11
			// Found this while running the unit tests. Basically we need to make sure the 
			// selected time range is within the min/max range.
			//
			var selection = this.SelectedTimeRange;

			if (null != selection)
			{
				DateRange actualSelection = selection.Value;
				DateRange minMax = this.GetMinMaxRange(true);

				if (!minMax.Contains(actualSelection))
				{
					if (!actualSelection.Intersect(minMax, false ))
					{
						this.SelectedTimeRange = null;
					}
					else
					{
						this.SelectedTimeRange = actualSelection;
					}
				}
			}
		}
		#endregion // OnMinMaxDatesChanged

		#region OnPreVerifyGroups
		/// <summary>
		/// Invoked when the control is about to verify the Groups
		/// </summary>
		internal virtual void OnPreVerifyGroups()
		{
			// verify the state. we may need to force the groups to be reverified
			this.TimeslotInfo.VerifyGroupRanges();

			// update the active calendar/group
			this.VerifyActiveCalendarGroup();
		}
		#endregion // OnPreVerifyGroups

		#region OnPostVerifyGroups
		/// <summary>
		/// Invoked after the control has verified the Groups
		/// </summary>
		internal virtual void OnPostVerifyGroups()
		{
			this.SetFlag(InternalFlags.CurrentDateChanged, false);
			this.SetFlag(InternalFlags.ActiveCalendarOrGroup, false);
			this.SetFlag(InternalFlags.SupportedActivityTypes, false);
		}
		#endregion // OnPostVerifyGroups

		// AS 10/28/10 TFS55791/TFS50143
		#region OnPostVerifyState
		internal virtual void OnPostVerifyState()
		{
		}
		#endregion // OnPostVerifyState
    
		#region OnSubObjectChanged
		/// <summary>
		/// Invoked when the Settings or a property on the settings of the ScheduleDataManager has changed.
		/// </summary>
		/// <param name="sender">The object whose property changed.</param>
		/// <param name="property">The property that changed.</param>
		/// <param name="extraInfo">Either Null or an instance of DependencyPropertyChangedEventArgs, NotifyCollectionChangedEventArgs or PropertyChangedEventArgs.</param>
		internal virtual void OnSubObjectChanged(object sender, string property, object extraInfo)
		{
			// whenever anything that might result in an IsWorkDay or WorkingsHours collection changing. note 
			// this excludes resource level or lower info - that will be dealt with where needed
			// the property chains for this are:
			// DataManager->ScheduleSettings->DaysOfWeek->DaySettings->IsWorkDay
			// DataManager->ScheduleSettings->DaysOfWeek->DaySettings->WorkingHours (as well as changes to its collection)
			// DataManager->ScheduleSettings->DaySettingsOverrides (as well as changes to its collection)
			// DataManager->ScheduleSettings->WorkDays
			// DataManager->ScheduleSettings->WorkingHours (as well as changes to its collection)
			//
			bool isWorkingHourRelated = false;
			bool isFirstDayOfWeekRelated = false;

			InternalFlags flags = 0;

			if (sender is XamScheduleDataManager)
			{
				if (property == "Settings")
				{
					flags = InternalFlags.DataManagerChanged | InternalFlags.CalendarGroupsChanged;

					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					flags |= InternalFlags.ClickToAddEnabledChange;

					isFirstDayOfWeekRelated = true;
					isWorkingHourRelated = true; // handles DataManager->ScheduleSettings:
				}
				else if (property == "BlockingError")
				{
					if (DesignerProperties.GetIsInDesignMode(this))
						this.ClearValue(BlockingErrorPropertyKey);
					else
						this.BlockingError = (sender as XamScheduleDataManager).BlockingError;
				}
				else if (property == "CurrentDate")
				{
					flags = InternalFlags.CurrentDateChanged;
				}
				else if (property == "CurrentUser")
				{
					if (_calendarGroupsController.CurrentProvider == CalendarGroupsProvider.CurrentUser ||
						_calendarGroupsController.CurrentProvider == CalendarGroupsProvider.None)
					{
						flags = InternalFlags.CalendarGroupsResolvedChanged;
					}

					flags |= InternalFlags.NotifyCurrentUserChange;

					// if the current user property changes and we're showing just the current working hours then invalidate the timeslot info
					if (_timeslotInfo.WorkHourSource == WorkingHoursSource.CurrentUser ||
						(_weekHelper != null && _weekHelper.WorkDaySource == WorkingHoursSource.CurrentUser))
						isWorkingHourRelated = true;

					isFirstDayOfWeekRelated = true;
				}
				else if (property == "ColorSchemeResolved")
				{
					// since we would obtain the color scheme for the current calendar
					if (_calendarGroupsController.CurrentProvider == CalendarGroupsProvider.CurrentUser)
						flags = InternalFlags.CalendarGroupsResolvedChanged;
				}
			}
			else if (sender is ScheduleSettings)
			{
				flags = InternalFlags.DataManagerChanged | InternalFlags.CalendarGroupsChanged;

				// handles DataManager->ScheduleSettings->WorkDays:
				// handles DataManager->ScheduleSettings->WorkingHours:

				// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
				//if (property == "WorkingHours" || property == "WorkDays")
				//    isWorkingHourRelated = true;
				//
				//if (property == "FirstDayOfWeek")
				//    isFirstDayOfWeekRelated = true;
				switch(property)
				{
					case "WorkingHours":
					case "WorkDays":
						isWorkingHourRelated = true;
						break;
					case "FirstDayOfWeek":
						isFirstDayOfWeekRelated = true;
						break;
					case "AppointmentSettings":
					case "TaskSettings":
					case "JournalSettings":
						// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
						flags |= InternalFlags.ClickToAddEnabledChange;
						break;
				}
			}
			else if (sender is CalendarGroupCalendarCollection)
			{
				// calendars were added/remove
				flags = InternalFlags.CalendarGroupsChanged;
			}
			else if (sender is CalendarGroupCollection)
			{
				// calendar groups were added/removed
				flags = InternalFlags.CalendarGroupsChanged | InternalFlags.CalendarGroupsResolvedChanged;

				if (property == "VisibleResources")
				{
					// if the visible resources changes and we're showing just the aggregated resource's working hours then invalidate the timeslot info
					if (_timeslotInfo.WorkHourSource == WorkingHoursSource.AllResourcesInGroups ||
						(_weekHelper != null && _weekHelper.WorkDaySource == WorkingHoursSource.AllResourcesInGroups))
						isWorkingHourRelated = true;
				}
			}
			else if (sender is CalendarColorScheme)
			{
				// if the default brush provider changes or the scrollbar style changes
				// then update that now
				XamScheduleDataManager dm = _dataManagerResolved;

				if (null != dm && dm.ColorSchemeResolved == sender)
				{
					var scheme = sender as CalendarColorScheme;

					this.DefaultBrushProvider = scheme.DefaultBrushProvider;
					this.ScrollBarStyle = scheme.ScrollBarStyle;
				}
			}
			else if (sender is Resource)
			{
				if (sender == _activeCalendar && property == "IsVisibleResolved")
					this.QueueInvalidation(InternalFlags.ActiveCalendarOrGroup);

				// handles Resource->DaySettingsOverride:
				if (property == "DaySettingsOverride" || property == "PrimaryTimeZoneId")
					isWorkingHourRelated = true;

				if (property == "FirstDayOfWeek")
					isFirstDayOfWeekRelated = true;
			}
			else if (sender is CalendarGroupBase)
			{
				if (sender == _activeGroup && property == "SelectedCalendar")
					this.QueueInvalidation(InternalFlags.ActiveCalendarOrGroup);
			}
			else if (sender is ScheduleDayOfWeek)
			{
				// handles DataManager->ScheduleSettings->DaysOfWeek->DaySettings:
				if (property == "DaySettings")
					isWorkingHourRelated = true;
			}
			else if (sender is WorkingHoursCollection		// this handles collection changes
				|| sender is DaySettingsOverrideCollection	// this handles collection changes
				|| sender is DaySettingsOverride			// need to dirty if the date, daysettings or recurrence changes so just listen for the object type
				|| sender is DaySettings					// need to dirty if IsWorkDay & WorkingHours properties change so just listen for the object type
				|| sender is ScheduleDaysOfWeek	// this handles collection changes
				)
			{
				isWorkingHourRelated = true;
			}
			// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
			else if (sender is ActivitySettings)
			{
				if (property == "IsAddViaClickToAddEnabled")
					flags |= InternalFlags.ClickToAddEnabledChange;
			}

			if (isWorkingHourRelated)
			{
				this.OnWorkingHourRelatedPropertyChange();
			}

			if (isFirstDayOfWeekRelated && null != _weekHelper)
				_weekHelper.InvalidateFirstDayOfWeek();

			if (flags != 0)
				this.QueueInvalidation(flags);
		}
		#endregion //OnSubObjectChanged

		#region OnTimeslotAreaRecycled
		internal virtual void OnTimeslotAreaRecycled(TimeslotAreaAdapter areaAdapter)
		{

		} 
		#endregion // OnTimeslotAreaRecycled

		#region OnTimeslotGroupRangesCreated
		/// <summary>
		/// Invoked when the objects used to determine the timeslots have been changed.
		/// </summary>
		internal virtual void OnTimeslotGroupRangesCreated()
		{
		}
		#endregion //OnTimeslotGroupRangesCreated

		#region OnTimeslotGroupRangesInvalidated
		internal virtual void OnTimeslotGroupRangesInvalidated()
		{
		}
		#endregion //OnTimeslotGroupRangesInvalidated

		#region OnVisibleDatesChanged
		internal virtual void OnVisibleDatesChanged(IList<DateTime> added, IList<DateTime> removed)
		{
			this.QueueInvalidation(InternalFlags.VisibleDatesChanged | InternalFlags.CurrentDateChanged | InternalFlags.NotifyPanelVisibleDateChange);
		}
		#endregion //OnVisibleDatesChanged

		#region OnWeekCountChanged
		internal virtual void OnWeekCountChanged()
		{
		}
		#endregion // OnWeekCountChanged

		#region OnWorkingHourRelatedPropertyChange
		internal virtual void OnWorkingHourRelatedPropertyChange()
		{
			// if the working hours changed then the timeslots may have changed as well
			if (_timeslotInfo.WorkHourSource.HasValue)
				_timeslotInfo.InvalidateWorkingHours();

			if (_weekHelper != null && _weekHelper.WorkDaySource.HasValue)
				_weekHelper.InvalidateWorkingHours();
		}
		#endregion // OnWorkingHourRelatedPropertyChange

		#region ProcessKeyDown
		/// <summary>
		/// Invoked when the keydown is pressed after any current edit process has had a chance to process the key.
		/// </summary>
		/// <param name="e">Provides information about the key event</param>
		internal virtual void ProcessKeyDown(KeyEventArgs e)
		{
			ScheduleControlCommand? command = GetKeyCommand(e);

			if (null != command)
			{
				if (this.ExecuteCommand(command.Value))
				{
					e.Handled = true;
				}
				else
				{
					// AS 12/16/10 TFS61923
					var key = PresentationUtilities.GetKey(e);

					switch ( key )
					{
						case Key.Up:
						case Key.Down:
						case Key.Right:
						case Key.Left:
							e.Handled = true;
							break;
					}
				}
			}
		}
		#endregion // ProcessKeyDown

		#region ProcessMouseWheel
		internal virtual void ProcessMouseWheel(ITimeRangeArea timeRangeArea, System.Windows.Input.MouseWheelEventArgs e)
		{
			if (null != timeRangeArea)
			{
				TimeslotPanelBase timeslotPanel = timeRangeArea.TimeRangePanel;

				if (null != timeslotPanel && timeslotPanel.TimeslotOrientation == Orientation.Vertical)
				{
					var si = this.GetTimeslotScrollInfo(timeslotPanel);

					if (null != si && e.Delta != 0)
					{
						bool scrollDown = e.Delta < 0;
						si.Scroll(scrollDown ? 1 : -1);
						e.Handled = true;
					}
				}
			}
		}
		#endregion // ProcessMouseWheel

		#region ProcessVerifyDataManager

		private void ProcessVerifyDataManager()
		{
			// set the token to a refernce to this so we prevent re-triggering this verification
			_verifyToken = this;

			if (this.DataManagerResolved != null)
				return;

			this.BlockingError = ScheduleUtilities.CreateBlockingFromId(this, "LE_DataMgrNotSet", new object[] { this.GetType().Name });
		}

		#endregion //ProcessVerifyDataManager

		#region QueueAsyncVerification
		internal void QueueAsyncVerification()
		{
			// do not start an operation when processing?
			if (!this.IsVerifyingState)
				_deferredValidationOperation.StartAsyncOperation();
		}
		#endregion // QueueAsyncVerification

		#region ReinitializeTimeslots
		internal virtual void ReinitializeTimeslots(CalendarGroupWrapper wrapper)
		{
			foreach (TimeslotAreaAdapter tsArea in wrapper.GroupTimeslotArea.TimeslotAreas)
			{
				tsArea.Timeslots.Reinitialize();
			}
		}
		#endregion // ReinitializeTimeslots

		#region RestoreSelection
		internal abstract void RestoreSelection( DateRange pendingSelection );
		#endregion // RestoreSelection

		#region SelectActivity

		internal bool SelectActivity(ActivityBase activity, bool keepCurrentSelection )
		{
			if (activity == null)
			{
				if (keepCurrentSelection || this._selectedActivities.Count == 0)
					return false;

				// AS 11/1/10 TFS58843
				if ( !this.EditHelper.TryEndInPlaceEdit() )
					return false;

				this._selectedActivities.Clear();

				return true;
			}
			
			ResourceCalendar calendar = activity.OwningCalendar;

			Debug.Assert(calendar != null, "Activity is missing an owning calendar");

			if (calendar == null)
				return false;

			Debug.Assert(_activeGroup != null && _activeGroup.Contains(calendar), "We either don't have an active group or the calendar is not in that active group");

			// AS 11/1/10 TFS58843
			// If we're in edit mode then try to end edit mode but don't force it. If it 
			// doesn't end then don't change the selection.
			//
			if ( _editHelper != null && _editHelper.Activity != activity )
			{
				if ( !_editHelper.TryEndInPlaceEdit() )
					return false;
			}

			int count = this._selectedActivities.Count;

			if (count == 0)
			{
				this._selectedActivities.Add(activity);
				return true;
			}

			int index = this._selectedActivities.IndexOf(activity);

			if (index >= 0)
			{
				if (count == 1 || keepCurrentSelection)
					return true;
			}

			if (keepCurrentSelection)
			{
				this._selectedActivities.BeginUpdate();

				try
				{
					List<ActivityBase> temp = new List<ActivityBase>();

					// filter out activities from other resources
					foreach (ActivityBase existing in this._selectedActivities)
					{
						if (existing.OwningCalendar == calendar)
							temp.Add(existing);
					}

					this._selectedActivities.Clear();

					if (temp.Count > 0)
						this._selectedActivities.AddRange(temp);
						
					// if the activity wasn't already selected then do it now
					if (index < 0)
						this._selectedActivities.Add(activity);
				}
				finally
				{
					this._selectedActivities.EndUpdate();
				}
			}
			else
			{
				this._selectedActivities.ReInitialize(new ActivityBase[] { activity });
			}


			return true;
		}

		#endregion //SelectActivity

		#region SelectCalendar

		internal bool SelectCalendar(ResourceCalendar calendar, bool makeActive, FrameworkElement element)
		{
			return this.SelectCalendar(calendar, makeActive, ScheduleUtilities.GetCalendarGroupFromElement(element));
		}

		internal bool SelectCalendar(ResourceCalendar calendar, bool makeActive, CalendarGroupBase group)
		{
			if (group == null)
				return false;

			if (calendar == null)
				return false;

			Debug.Assert(group.Contains(calendar), "The specified group doesn't contain the calendar");

			
			group.SelectedCalendar = calendar;

			if (group.SelectedCalendar != calendar)
				return false;

			if (makeActive)
			{
				this.ActiveCalendar = calendar;

				return this.ActiveCalendar == calendar;
			}

			return true;
		}

		#endregion //SelectCalendar	

		#region SetInitialSelectedTimeRange
		internal void SetInitialSelectedTimeRange( DateRange? range )
		{
			Debug.Assert(_cachedSelectedTimeRange == null, "Only expecting to use this while there is no selection");
			Debug.Assert(this.IsVerifyingState, "This will clear the change flag so only use this while verifying.");

			_processedSelectedRange = range;
			this.SelectedTimeRange = range;
			this.SetFlag(InternalFlags.SelectedTimeRangeChanged, false);
		}
		#endregion // SetInitialSelectedTimeRange

		#region SetSelectedTimeRange
		internal virtual bool SetSelectedTimeRange(DateRange timeslotRange, bool extendSelection, bool preventBringIntoView = false)
		{
			if (extendSelection)
			{
				DateRange? selectionAnchor = this.GetSelectionAnchor();

				if (null != selectionAnchor)
				{
					// get the timeslot that represents the anchor. the anchor will be the start 
					// of the selection whether that was done by dragging left to right or right 
					// to left
					DateRange anchorRange = selectionAnchor.Value;

					// union the anchor and destination if told
					DateRange normalizedAnchor = DateRange.Normalize(anchorRange);
					DateTime start = ScheduleUtilities.Min(normalizedAnchor.Start, timeslotRange.Start);
					DateTime end = ScheduleUtilities.Max(normalizedAnchor.End, timeslotRange.End);

					if (timeslotRange.Start < anchorRange.Start)
						timeslotRange = new DateRange(end, start);
					else
						timeslotRange = new DateRange(start, end);
				}
			}

			// AS 1/13/12 TFS77443
			// Do not allow selection outside the min/max.
			//
			if (!timeslotRange.Intersect(this.GetMinMaxRange(true), false ) || timeslotRange.IsEmpty)
				return false;

			this.SelectedTimeRange = timeslotRange;
			return true;
		} 
		#endregion // SetSelectedTimeRange

		#region SetTemplateItemExtent
		internal virtual void SetTemplateItemExtent(System.Enum itemId, double value)
		{
			_templateItemExtent[itemId] = value;

			if (ScheduleControlTemplateValue.MoreActivityIndicatorHeight.Equals(itemId))
			{
				this.ActivityGutterHeight = value;
			}

			if (ScheduleControlTemplateValue.SingleLineApptHeight.Equals(itemId))
			{
				if (!double.IsNaN(value))
					this.ActivityColumnHeight = Math.Max(value, 8);
			}
		}
		#endregion // SetTemplateItemExtent

		// AS 4/20/11 TFS73205
		#region ShouldBumpHeaderHourFontSize
		internal virtual bool ShouldBumpHeaderHourFontSize(TimeslotBase timeslot)
		{
			TimeSpan span = timeslot.End.Subtract(timeslot.Start);
			return span.TotalMinutes < 31 && span.TotalMinutes >= 1;
		}
		#endregion //ShouldBumpHeaderHourFontSize

		// AS 12/17/10 TFS62030
		#region ShouldIndentActivityEdge
		internal virtual void ShouldIndentActivityEdge( ActivityPresenter presenter, out bool leading, out bool trailing )
		{
			leading = trailing = false;
		} 
		#endregion // ShouldIndentActivityEdge

		#region SupportsCommand
		internal virtual bool SupportsCommand(ICommand command)
		{
			return command is ScheduleControlCommandBase;
		}
		#endregion // SupportsCommand

		#region TrackTemplateItem
		internal void TrackTemplateItem(FrameworkElement fe, System.Enum widthId, System.Enum heightId)
		{
			if ( null == _templateItems )
				_templateItems = new List<TemplateItemSizeChangeHelper>();

			// in the future if we need to be able to unhook them we can just store the 
			// instance and dispose/unhook it as needed
			var item = new TemplateItemSizeChangeHelper(fe, widthId, heightId, this);

			_templateItems.Add(item);
		} 
		#endregion // TrackTemplateItem

		#region UpdateVisibleDatesForSelectedRangeOverride

		internal abstract void UpdateVisibleDatesForSelectedRangeOverride(DateRange? oldRange, DateRange newRange);

		#endregion // UpdateVisibleDatesForSelectedRangeOverride

		#region UpdateVisualState
		internal void UpdateVisualState()
		{
			this.ChangeVisualState(true);
		}
		#endregion // UpdateVisualState

		// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		#region VerifyClickToAddState
		internal virtual void VerifyClickToAddState(TimeslotPanelBase panel, bool isEnabled)
		{
		}
		#endregion //VerifyClickToAddState 

		#region VerifyInitialState

		internal virtual void VerifyInitialState(List<DataErrorInfo> errorList)
		{
			if (this.VisibleCalendarCount == 0)
				errorList.Add(ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NoVisibleCalendars", this.GetType().Name)); // "{0} contains no visible calendars. Either set the {0}.DataManager.CurrentUserID to a ResourceID with a PrimaryCalendar or add CalendarGroups, containing visible ResourceCalendars, to the {0}.DataManager.CalendarGroups collection or the {0}.CalendarGroupsOverride collection."
		}

		#endregion //VerifyInitialState	
    
		#region VerifyIsAllDaySelection
		internal void VerifyIsAllDaySelection()
		{
			bool isAllDaySelection = this.CalculateIsAllDaySelection(_cachedSelectedTimeRange);

			if (isAllDaySelection != this.IsAllDaySelection)
			{
				this.SetFlag(InternalFlags.IsAllDaySelection, isAllDaySelection);
				this.OnIsAllDaySelectionChanged();
			}
		}
		#endregion // VerifyIsAllDaySelection

		#region VerifyMergedScrollInfos
		internal virtual void VerifyMergedScrollInfos()
		{
			_calendarGroupScrollInfo.VerifyState();
		} 
		#endregion // VerifyMergedScrollInfos

		#region VerifyStateOverride
		internal virtual void VerifyStateOverride()
		{
			// make sure the logical day offset/duration are initialized
			_timeslotInfo.VerifyLogicalDaySettings();

			// the order here is important. first we need to make sure all objects 
			// are using the latest datamanager settings

			// make sure the visible dates know the range of dates that are allowed
			if (this.GetFlag(InternalFlags.DataManagerChanged))
			{
				// recalculate the timeslot info
				_timeslotInfo.InvalidateGroupRanges();

				// the first day of week may have changed as a result of the datamanager changing
				if (_weekHelper != null)
					_weekHelper.InvalidateFirstDayOfWeek();

				this.VerifyIsAllDaySelection();

				DateRange minMax = this.GetMinMaxRange();

				if ( minMax != _visibleDates.AllowedRange )
				{
					this._visibleDates.AllowedRange = minMax;
					this.OnMinMaxDatesChanged(); // AS 11/2/10 TFS58663
				}

				this.SetFlag(InternalFlags.DataManagerChanged, false);
			}

			// process any selection changes. this needs to be done before the 
			// visible dates since the visible dates could be affected by 
			// changes to the selection range
			this.VerifySelectedTimeRange();

			// then do any massaging of the visible dates. note since the visible 
			// dates may have changed but the selected date not this will need to 
			// verify that the selected date is part of the visible dates. if not 
			// then it will need to change the selected date possibly reverifying 
			// it again
			this.VerifyVisibleDates();

			// show/hide the now date highlighting...
			this.VerifyCurrentDate();

			// build/verify the list of calendargroupwrappers
			this.VerifyGroups();
		}

		#endregion //VerifyStateOverride

		#region VerifyTimeslotPanelExtent
		internal virtual void VerifyTimeslotPanelExtent(TimeslotPanelBase panel)
		{
			var activityPanel = panel as ScheduleActivityPanel;

			if (null != activityPanel)
			{
				// the activity orientation is always opposite to the timeslot orientation
				if (activityPanel.TimeslotOrientation == Orientation.Horizontal)
				{
					activityPanel.ActivityColumnExtent = _activityColumnHeight;

					// since the click to add prompt needs to be as tall as a single activity
					// we should use the max of the arrow indicator and the single activity height
					activityPanel.MinEmptySpace = activityPanel.ClickToAddType != ClickToAddType.None 
							&& double.IsInfinity(_activityColumnHeight) == false // AS 10/1/10 TFS50001
						? Math.Max(_activityGutterHeight, _activityColumnHeight)
						: _activityGutterHeight;
				}
			}
		} 
		#endregion // VerifyTimeslotPanelExtent

		#region VerifyUIState
		internal virtual void VerifyUIState()
		{
			if (this.GetFlag(InternalFlags.MergedScrollInfosChanged))
			{
				this.VerifyMergedScrollInfos();
				this.SetFlag(InternalFlags.MergedScrollInfosChanged, false);
			}

			bool extentChanged = this.GetFlag(InternalFlags.TimeslotPanelExtents);
			bool activitySelectionChanged = this.GetFlag(InternalFlags.ActivitySelectionChanged);
			bool visDates = this.GetFlag(InternalFlags.NotifyPanelVisibleDateChange);
			bool currentUserChange = this.GetFlag(InternalFlags.NotifyCurrentUserChange);

			// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
			bool clickToAddChange = this.GetFlag(InternalFlags.ClickToAddEnabledChange);
			bool isClickToAddEnabled = this.IsClickToAddEnabled;

			if (extentChanged || activitySelectionChanged || visDates || currentUserChange || clickToAddChange)
			{
				foreach (TimeslotPanelBase tsp in _timeslotPanels)
				{
					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					if (clickToAddChange)
						this.VerifyClickToAddState(tsp, isClickToAddEnabled);

					// AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
					// Since the click to add indicator may affect the minimum gutter size
					// we'll reverify the extents too.
					//
					//if (extentChanged)
					if (extentChanged || clickToAddChange)
						this.VerifyTimeslotPanelExtent(tsp);

					ScheduleActivityPanel ap = tsp as ScheduleActivityPanel;

					if (null != ap)
					{
						if (activitySelectionChanged)
							ap.VerifySelectionState(this);

						if (visDates)
							ap.OnVisibleDatesChanged();

						if (currentUserChange)
							ap.OnCurrentUserChanged();
					}
				}

				// if the activity selection changes then cache the earliest range so we know what 
				// to 
				if ( activitySelectionChanged )
				{
					this.CachePendingSelectedTimeRange();
				}

				this.SetFlag(InternalFlags.TimeslotPanelExtents | InternalFlags.ActivitySelectionChanged | InternalFlags.NotifyPanelVisibleDateChange | InternalFlags.NotifyCurrentUserChange | InternalFlags.ClickToAddEnabledChange, false);
			}

			// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
			if (currentUserChange)
			{
				Resource currentUser = _dataManagerResolved == null ? null : _dataManagerResolved.CurrentUser;
				foreach (var group in this.GroupWrappers)
					group.GroupHeader.OnCurrentUserChanged(currentUser);
			}
		} 
		#endregion // VerifyUIState

		#region VerifyVisibleDatesOverride
		/// <summary>
		/// Invoked if the VisibleDates have changed to allow them to be updated based on other state (e.g. expanding dates to work/full week).
		/// </summary>
		internal abstract void VerifyVisibleDatesOverride();
		#endregion // VerifyVisibleDatesOverride

		#endregion //Internal Methods

		#region Private Methods

		#region BeginInPlaceCreate
		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		// Refactored since we may need to prompt the user for the type of activity to create.
		//
		//// AS 9/30/10 TFS49593
		//// Refactored from TextInput handling.
		////
		//private bool BeginInPlaceCreate( ActivityType activityType, out ActivityPresenter presenter )
		//{
		//    presenter = null;
		//
		//    DateRange range;
		//    ResourceCalendar calendar;
		//    bool isAllDay;
		//
		//    if ( !this.GetCreateNewInfo(activityType, out range, out calendar, out isAllDay) )
		//        return false;
		//
		//    bool result = this.EditHelper.AddNew(range.Start, range.End, isAllDay, calendar, out presenter, activityType);
		//
		//    if ( !result )
		//        presenter = null;
		//
		//    return result;
		//}
		private bool BeginInPlaceCreate( Action<ActivityPresenter> action )
		{
			var calendar = this.ActiveCalendar;
			int allowedCount;
			ActivityTypes types = this.GetNewActivityTypes(calendar, ActivityCreationTrigger.Typing, out allowedCount);

			if (allowedCount == 0)
				return false;
			else if (allowedCount == 1)
			{
				return this.BeginInPlaceCreate(ScheduleUtilities.GetActivityType(types).Value, calendar, action);
			}
			else
			{
				// AS 4/14/11 TFS71617
				// If we've already shown the activity chooser but it's not closed then eat any further requests.
				//
				if (_hasPendingInPlaceCreate)
					return true;

				XamScheduleDataManager dm = this.DataManagerResolved;
				ActivityTypeChooserDialog.ChooserResult result = new ScheduleDialogBase<ActivityType?>.ChooserResult(this);
				Action<bool?> closedCallback = delegate(bool? chooserDialogResult)
				{
					ScheduleControlBase ctrl = result.UserData as ScheduleControlBase;

					// AS 4/14/11 TFS71617
					// Clear the pending flag so we know that we can let any subsequent in place creation that needs the dialog.
					//
					if (null != ctrl)
						ctrl._hasPendingInPlaceCreate = false;

					if (chooserDialogResult == true && result.Choice.HasValue && calendar == this.ActiveCalendar)
					{
						// AS 3/31/11 TFS69812
						// After some review it doesn't "feel" right that we preserve the first bit 
						// of text that triggered the invocation.
						//
						//this.BeginInPlaceCreate(result.Choice.Value, calendar, action);
						this.BeginInPlaceCreate(result.Choice.Value, calendar, null);
					}
				};

				// AS 4/14/11 TFS71617
				// Assume we will show the dialog. We have to set this now because in WPF the call will be 
				// synchronous/modal but in SL it will be async (pseudo-modal).
				//
				_hasPendingInPlaceCreate = true;

				bool displayDialogResult = dm.DisplayActivityTypeChooserDialog(
					this.DialogContainerPanel,
					ScheduleUtilities.GetString("DLG_ActivityTypeChooserDialog_Title_NewActivity"),
					types,
					ActivityTypeChooserReason.AddActivityViaTyping,
					calendar,
					result,
					null,
					closedCallback);

				// AS 4/14/11 TFS71617
				// If the dialog was cancelled or suppressed then clear the flag.
				//
				if (!displayDialogResult)
					_hasPendingInPlaceCreate = false;

				return displayDialogResult;
			}
		}

		private bool BeginInPlaceCreate(ActivityType activityType, ResourceCalendar calendar, Action<ActivityPresenter> action)
		{
			if (calendar == null)
				return false;

			DateRange? selectedTimeRange = this.SelectedTimeRangeNormalized;

			if (selectedTimeRange == null)
				return false;

			DateRange range = selectedTimeRange.Value;
			bool isAllDay = this.CalculateIsAllDaySelection(range);

			var dm = this.DataManagerResolved;

			if (null != dm && !dm.IsTimeZoneNeutralActivityAllowed(activityType, calendar))
				isAllDay = false;

			ActivityPresenter presenter;
			bool result = this.EditHelper.AddNew(range.Start, range.End, isAllDay, calendar, out presenter, activityType);

			if (!result)
				presenter = null;
			else if (null != action)
				action(presenter);

			return result;
		}
		#endregion // BeginInPlaceCreate

		#region CachePendingSelectedTimeRange
		private void CachePendingSelectedTimeRange()
		{
			// if we are in the middle of deleting then don't update this since that would be updated as 
			// we delete which could be staggered if there are occurrences for which we must prompt whether 
			// to delete the occurrence or series
			if ( this.SelectedActivities.Count > 0 && _deletionHelper == null )
			{
				// we need to select some timeslot if all the activites are deleted. there are lots of different
				// behaviors that we could go down. outlook itself is not consistent amongst the controls. really 
				// we just want to ensure that some reasonable timeslot is selected when the selected activities 
				// are cleared. to that end and to avoid situations where they had selected a long activity, we're
				// just going to select the timeslot containing the start of the earliest activity
				DateRange currentRange = new DateRange(DateTime.MaxValue);
				var token = this.TimeZoneInfoProviderResolved.LocalToken;

				// AS 12/9/10 NA 11.1 - XamOutlookCalendarView
				List<DateRange> localRanges = this.GetLocalVisibleDateRanges();
				DateRange? preferredRange = null;

				foreach ( ActivityBase activity in this.SelectedActivities )
				{
					DateRange tempRange = ScheduleUtilities.ConvertFromUtc(token, activity);

					if ( tempRange.Start < currentRange.Start || (tempRange.Start == currentRange.Start && tempRange.End > currentRange.End) )
					{
						currentRange = tempRange;
					}

					// AS 12/9/10 NA 11.1 - XamOutlookCalendarView
					// It would be best if we chose a range that was actually in view.
					//
					// if we already found something closer then skip this
					if ( preferredRange != null )
					{
						if ( tempRange.Start > preferredRange.Value.Start ) // later than the one we found already
							continue;
						else if ( tempRange.Start == preferredRange.Value.Start && tempRange.End <= preferredRange.Value.End ) // within or matches preferred
							continue;
					}

					int start = ScheduleUtilities.BinarySearch(localRanges, tempRange.Start);

					// if the start is not in view then check the end
					if ( start < 0 )
					{
						int end = ScheduleUtilities.BinarySearch(localRanges, tempRange.End);

						// between or outside all ranges
						if ( start == end )
							continue;
					}

					preferredRange = tempRange;
				}

				if ( currentRange.Start < DateTime.MaxValue )
				{
					// AS 12/9/10 NA 11.1 - XamOutlookCalendarView
					//_pendingSelectedTimeRange = currentRange;
					_pendingSelectedTimeRange = preferredRange ?? currentRange;
				}
				else
					_pendingSelectedTimeRange = null;
			}
		}
		#endregion // CachePendingSelectedTimeRange

		#region CreateBrushVersionBinding

		private static Binding CreateBrushVersionBinding(object source)
		{
			BindingPart part = new BindingPart();




			part.PathParameter = BrushVersionProperty;

			// wrap the CreateBinding in a try/catch because SL has some
			// issues in the VS2010 designer where it can throw an exception if it is 
			// in a bad state
			try
			{
				Binding binding = PresentationUtilities.CreateBinding(part);
				binding.Source = source;
				return binding;
			}
			catch 
			{
				return null;
			}

		}

		#endregion //CreateBrushVersionBinding	
	
		#region CreateDefaultDataManager
		internal static XamScheduleDataManager CreateDefaultDataManager()
		{
			var dm = new XamScheduleDataManager();

			var cn = new ListScheduleDataConnector();

			#region Resources

			var resources = new List<Resource>();
			resources.Add(new Resource { Id = "Default", Name = "Default" }); 
			cn.ResourceItemsSource = resources;

			#endregion // Resources

			dm.DataConnector = cn;
			dm.CurrentUser = cn.ResourceItems[0];

			return dm;
		}
		#endregion // CreateDefaultDataManager

		#region DeleteSelectedActivities

		private void DeleteSelectedActivities()
		{
			//ignore requests while a _deletionHelper is still active
			if (_deletionHelper != null)
				return;

			if (this.SelectedActivities.Count > 0)
			{
				XamScheduleDataManager dm = this.DataManagerResolved;

				if (dm != null)
				{
					// cache the selection info before we start so we know what to select when the deletion ends
					this.CachePendingSelectedTimeRange();

					// create a ActivityDeletionHelper to process the deletions
					_deletionHelper = new ActivityDeletionHelper(this);

					//start the deletion process
					_deletionHelper.ProcessDeletions();
				}
			}
		}

		#endregion //DeleteSelectedActivities	

		#region GetActivitySelectedTimeRange
		private DateRange? GetActivitySelectedTimeRange()
		{
			if (_selectedActivities.Count == 0)
				_activitySelectedTimeRange = null;
			else if (_activitySelectedTimeRange == null)
			{
				var token = this.TimeZoneInfoProviderResolved.LocalToken;
				DateRange range = DateRange.Infinite;

				for (int i = 0, count = _selectedActivities.Count; i < count; i++)
				{
					var activity = _selectedActivities[i];
					var start = activity.GetStartLocal(token);
					var end = activity.GetEndLocal(token);

					if (end > start)
						end = ScheduleUtilities.GetNonInclusiveEnd(end);

					if (i > 0)
					{
						if (range.Start > start)
							range.Start = start;

						if (range.End < end)
							range.End = end;
					}
					else
						range = new DateRange(start, end);
				}

				DateRange? startRange = this.TimeslotInfo.GetTimeslotRange(range.Start);
				DateRange? endRange = this.TimeslotInfo.GetTimeslotRange(range.End);

				if (null != startRange)
					range.Start = startRange.Value.Start;

				if (null != endRange)
					range.End = endRange.Value.End;

				_activitySelectedTimeRange = range;
			}

			return _activitySelectedTimeRange;
		}
		#endregion // GetActivitySelectedTimeRange

		#region GetCreateNewInfo
		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		// This code was only called from one place so I moved it into that location so 
		// we can do the individual checks as we don't necessarily have the activity type 
		// yet.
		//
		// AS 9/30/10 TFS49593
		// Refactored from textinput handling. We may also need it in the future to show an appt dialog (e.g. using ctrl-N).
		//
		//private bool GetCreateNewInfo( ActivityType activityType, out DateRange range, out ResourceCalendar calendar, out bool isAllDay )
		//{
		//    range = DateRange.Infinite;
		//    calendar = null;
		//    isAllDay = false;
		//
		//    DateRange? selectedTimeRange = this.SelectedTimeRangeNormalized;
		//
		//    calendar = this.ActiveCalendar;
		//
		//    if ( selectedTimeRange == null || calendar == null )
		//        return false;
		//
		//    range = selectedTimeRange.Value;
		//
		//    isAllDay = this.CalculateIsAllDaySelection(range);
		//
		//    var dm = this.DataManagerResolved;
		//
		//    if ( null != dm && !dm.IsTimeZoneNeutralActivityAllowed(activityType, calendar) )
		//        isAllDay = false;
		//
		//    return true;
		//}
		#endregion // GetCreateNewInfo
    
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

		#region GetNextPreviousGroup
		private CalendarGroupBase GetNextPreviousGroup( CalendarGroupBase group, bool next )
		{
			var groups = this.CalendarGroupsResolved;
			int index = groups.IndexOf(_activeGroup);

			if (index < 0)
				return null;

			return ScheduleUtilities.FindNextOrDefault(groups, index, next, 1, true, ScheduleUtilities.HasVisibleCalendars);
		}
		#endregion // GetNextPreviousGroup

		#region InitializeGroup
		/// <summary>
		/// Invoked during the VerifyGroups when an existing group is reused.
		/// </summary>
		/// <param name="group">The group being reused</param>
		/// <param name="isNewlyRealized">True if the group is new</param>
		internal virtual void InitializeGroup(CalendarGroupWrapper group, bool isNewlyRealized)
		{
			group.OnTodayChanged(_currentDate);

			if (!isNewlyRealized)
			{
				if (this.GetFlag(InternalFlags.SupportedActivityTypes))
					this.InitializeActivityProviders(group);
			}

			if (isNewlyRealized || this.GetFlag(InternalFlags.ActiveCalendarOrGroup))
			{
				group.GroupHeader.ActiveCalendar = group.CalendarGroup == this.ActiveGroup ? _activeCalendar : null;
			}
		}
		#endregion // InitializeGroup

		// JJD 12/02/10 - TFS59874
		#region OnActivityDlgDisplayEnded

		private void OnActivityDlgDisplayEnded(ActivityDlgDisplayHelper helper)
		{
			Debug.Assert(helper == _dlgHelper, "ActivityDlgDisplayHelper out of sync");

			if ( _dlgHelper == helper )
				_dlgHelper = null;

		}

		#endregion //OnActivityDlgDisplayEnded

		#region OnLoaded
		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.VerifyState();
		}
		#endregion //OnLoaded

		#region OnRecurrenceDeletionEnded

		private void OnRecurrenceDeletionEnded(ActivityDeletionHelper helper)
		{
			Debug.Assert(helper == _deletionHelper, "ActivityDeletionHelper out of sync");

			_deletionHelper = null;

			// we wouldn't have been caching the selection information while processing
			// deletions so if there are still selected activities (because the operation
			// was cancelled) then update the cache now so we don't use stale information 
			// if the selected activities are cleared
			if ( this.SelectedActivities.Count > 0 )
				this.CachePendingSelectedTimeRange();
			else if ( this.SelectedTimeRange == null )
			{
				this.QueueInvalidation(InternalFlags.RestoreSelectedTimeRange);
			}
		}

		#endregion //OnRecurrenceDeletionEnded	

		#region OnSubObjectChanged
		private static void OnSubObjectChanged(ScheduleControlBase ctrl, object sender, string propName, object extraInfo)
		{
			ctrl.OnSubObjectChanged(sender, propName, extraInfo);
		}
		#endregion // OnSubObjectChanged

		#region OnWeekFirstDateChanged
		internal virtual void OnWeekFirstDateChanged()
		{
		}
		#endregion // OnWeekFirstDateChanged

		#region QueueInvalidation
		private void QueueInvalidation(InternalFlags flag)
		{
			if ((_flags & flag) != flag)
			{
				this.SetFlag(flag, true);

				this.QueueAsyncVerification();
			}
		}
		#endregion // QueueInvalidation

		#region SetFlag
		private void SetFlag(InternalFlags flag, bool set)
		{
			if (set)
				_flags |= flag;
			else
				_flags &= ~flag;
		}
		#endregion // SetFlag

		#region VerifyActiveCalendarGroup
		private void VerifyActiveCalendarGroup()
		{
			if (!this.GetFlag(InternalFlags.ActiveCalendarOrGroup | InternalFlags.CalendarGroupsResolvedChanged | InternalFlags.CalendarGroupsChanged))
				return;

			var activeCalendar = this.ActiveCalendar;
			var activeGroup = this.ActiveGroup;
			var calendarGroups = this.CalendarGroupsResolved;

			// if the active calendar was cleared
			if (activeCalendar == null || !activeCalendar.IsVisibleResolved)
			{
				int activeGroupIndex = activeGroup == null ? -1 : calendarGroups.IndexOf(activeGroup);

				if (activeGroupIndex < 0 || activeGroup.VisibleCalendars.Count == 0)
				{
					activeGroup = ScheduleUtilities.FindNearestAfterOrDefault(calendarGroups, Math.Max(0, activeGroupIndex), ScheduleUtilities.HasVisibleCalendars);
				}

				// if there is a group we can use then use its selected calendar
				if (activeGroup != null)
					this.ActiveCalendar = activeGroup.SelectedCalendar;
				else // otherwise clear the group
					this.ActiveGroup = null;
			}
			else
			{
				// we have an active calendar but not an active group or the group we have doesn't 
				// contain the calendar or the group is not the one we're using any more

				if (activeGroup == null || !activeGroup.Contains(activeCalendar) || !calendarGroups.Contains(activeGroup))
				{
					CalendarGroupBase newGroup = null;

					foreach (CalendarGroupBase group in calendarGroups)
					{
						if (group.Contains(activeCalendar))
						{
							newGroup = group;
							break;
						}
					}

					if (null != newGroup)
					{
						this.ActiveGroup = newGroup;
					}
					else
					{
						// if the calendar doesn't exist in a group the resources and calendars
						// may have changed so look for one with the specified id's
						ResourceCalendar newActiveCalendar = null;

						foreach (CalendarGroupBase group in calendarGroups)
						{
							foreach (ResourceCalendar calendar in group.VisibleCalendars)
							{
								if (calendar.Id == activeCalendar.Id &&
									calendar.OwningResourceId == activeCalendar.OwningResourceId)
								{
									newActiveCalendar = calendar;
									newGroup = group;
									break;
								}
							}
						}

						// if that fails then find the first group with a visible calendar
						if (newActiveCalendar == null)
						{
							newGroup = ScheduleUtilities.FindNearestAfterOrDefault(calendarGroups, 0, ScheduleUtilities.HasVisibleCalendars);

							if (newGroup != null)
								newActiveCalendar = newGroup.SelectedCalendar;
						}

						this.ActiveCalendar = newActiveCalendar;
					}
				}
				else if (activeGroup.SelectedCalendar != activeCalendar)
				{
					// the calendar is still in the group but the selected calendar was changed. note if the active 
					// calendar had been changed then we would have updated the group to make sure it was the selected
					// calendar
					this.ActiveCalendar = activeGroup.SelectedCalendar;
				}
			}
		}
		#endregion // VerifyActiveCalendarGroup

		#region VerifyCurrentDate
		private void VerifyCurrentDate()
		{
			if (this.GetFlag(InternalFlags.CurrentDateChanged))
			{
				// note we're not clearing the flag until after the groups have been verified
				XamScheduleDataManager dm = this.DataManagerResolved;
				if (dm == null)
					this.CurrentLogicalDate = null;
				else
				{
					DateTime date = dm.CurrentDate;
					this.CurrentLogicalDate = this.VisibleDates.ContainsDate(date) ? date : (DateTime?)null;
				}
			}
		}
		#endregion // VerifyCurrentDate

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

			if ( DesignerProperties.GetIsInDesignMode(this))
				this.DataManagerResolved = CreateDefaultDataManager();
		} 
		#endregion // VerifyDataManagerResolved

		#region VerifyGroups
		private void VerifyGroups()
		{
			// before we even check the flag call a virtual method so derive controls can 
			// potentially invalidate the group information
			this.OnPreVerifyGroups();

			if (this.IsVerifyGroupsPending)
			{
				// if the groups are dirty verify them
				this.VerifyGroupsImpl();
			}
			else
			{
				// otherwise exit unless the class needs to reinitialize the state of the groups
				if (!this.ShouldReinitializeGroups)
					return;

				foreach (CalendarGroupWrapper wrapper in _groupWrappers)
					this.InitializeGroup(wrapper, false);
			}

			// now let any derived controls know we're done so they can complete any clean up
			this.OnPostVerifyGroups();
		}

		private void VerifyGroupsImpl()
		{
			this.SetFlag(InternalFlags.CalendarGroupsChanged, false);

			var table = new Dictionary<CalendarGroupBase, CalendarGroupWrapper>();

			foreach (CalendarGroupWrapper group in _groupWrappers)
			{
				table[group.CalendarGroup] = group;
			}

			_groupWrappers.Clear();
			_groupTimeslots.Clear();
			_groupHeaders.Clear();

			IList<CalendarGroupBase> calendarGroups = this.GetCalendarGroups();
			int totalCalendarCount = 0;
			bool isNew;

			for (int i = 0, count = calendarGroups.Count; i < count; i++)
			{
				CalendarGroupBase calendarGroup = calendarGroups[i];
				CalendarGroupWrapper wrapper;

				int calendarCount = calendarGroup.VisibleCalendars.Count;

				// skip those without visible calendars
				if (calendarCount == 0)
					continue;

				totalCalendarCount += calendarCount;

				if (!table.TryGetValue(calendarGroup, out wrapper))
				{
					// create a new one
					wrapper = this.CreateCalendarGroupWrapper(calendarGroup);
					isNew = true;
				}
				else
				{
					isNew = false;
					table.Remove(calendarGroup);
				}

				this.InitializeGroup(wrapper, isNew);

				_groupWrappers.Add(wrapper);
				_groupTimeslots.Add(wrapper.GroupTimeslotArea);
				_groupHeaders.Add(wrapper.GroupHeader);

				wrapper.VerifyState();
			}

			this.VisibleCalendarCount = totalCalendarCount;
			this.CalendarHeaderAreaVisibilityResolved = this.GetResolvedCalendarHeaderAreaVisibility();
		} 
		#endregion //VerifyGroups

		#region VerifySelectionForVisibleDates
		private void VerifySelectionForVisibleDates()
		{
			var visDates = this.VisibleDates;
			int visCount = visDates.Count;

			if ( visCount > 0 )
			{
				DateRange? selectedRange = this.SelectedTimeRangeNormalized;
				DateRange? newSelection = selectedRange;

				if ( selectedRange != null )
				{
					DateRange logicalSelectionRange = this.ConvertToLogicalDate(selectedRange.Value);

					// make sure active date is in this range
					int startIndex = visDates.BinarySearch(logicalSelectionRange.Start.Date);
					int endIndex = logicalSelectionRange.IsEmpty ? startIndex : visDates.BinarySearch(ScheduleUtilities.GetNonInclusiveEnd(logicalSelectionRange.End).Date);

					bool startInView = startIndex >= 0;
					bool endInView = endIndex >= 0;

					if ( !startInView || !endInView )
					{
						// if not then we need to select a date closest to the current active date
						if ( !startInView )
							startIndex = ~startIndex;

						if ( !endInView )
							endIndex = ~endIndex;

						// if its after the last date then use the last date
						if ( startIndex == visCount )
							newSelection = this.GetDefaultSelectedRange(visDates[startIndex - 1]);
						else if ( endIndex == 0 ) // or before the first use the first date
							newSelection = this.GetDefaultSelectedRange(visDates[0]);
						else if ( !startInView && !endInView )
						{
							// both are out of view...

							// if they're within the same range then just use the date that they would be before
							if ( startIndex == endIndex )
								newSelection = this.GetDefaultSelectedRange(visDates[startIndex]);
							else
								newSelection = this.ConvertFromLogicalDate(new DateRange(visDates[startIndex], visDates[endIndex - 1].AddDays(1)));
						}
						else
						{
							if ( !startInView )
							{
								// just the start is out of view so use the start of the next day
								logicalSelectionRange.Start = visDates[startIndex];
							}
							else
							{
								// just the end is out of view - use the end of the previous day
								if ( startIndex == endIndex )
									logicalSelectionRange.End = visDates[startIndex].AddDays(1);
								else
									logicalSelectionRange.End = visDates[endIndex - 1].AddDays(1);
							}

							newSelection = this.ConvertFromLogicalDate(logicalSelectionRange);
						}
					}
				}

				if ( newSelection != selectedRange )
				{
					this.SelectedTimeRange = newSelection;

					// since this routine is processed after the active date 
					// changes we need to reverify the visible dates
					this.VerifySelectedTimeRange();

					Debug.Assert(!this.GetFlag(InternalFlags.VisibleDatesChanged), "The VisibleDates should not have been modified by the selected time change");
				}
			}

			this.SetFlag(InternalFlags.SelectedTimeRangeChanged, false);
		}
		#endregion // VerifySelectionForVisibleDates

		#region VerifySelectedTimeRange
		private void VerifySelectedTimeRange()
		{
			// get the previously processed active date and the current date
			DateRange? lastRange = _processedSelectedRange;
			DateRange? range = this.SelectedTimeRange;

			// make sure that date is activatable
			if (range != null)
			{
				// AS 4/1/11 TFS64258
				// The MinMax are logical dates. What we really want is the logical start 
				// of the min and the logical end of the max.
				//
				//DateRange newRange = GetActivatableRange(range.Value);
				DateRange currentRange = range.Value;
				DateRange minMax = this.GetMinMaxRange(true);

				DateRange newRange = currentRange;
				newRange.Intersect(minMax);

				if (currentRange.Start > currentRange.End)
					newRange = new DateRange(newRange.End, newRange.Start);

				if (newRange != range)
					range = this.SelectedTimeRange = newRange;
			}

			// make sure the selected date is within the visible dates
			if (lastRange != range)
			{
				_processedSelectedRange = range;

				if (_processedSelectedRange != null)
					this.UpdateVisibleDatesForSelectedRangeOverride(lastRange, _processedSelectedRange.Value);
			}

			Debug.Assert(_processedSelectedRange == this.SelectedTimeRange);
			this.SetFlag(InternalFlags.SelectedTimeRangeChanged, false);
		}
		#endregion // VerifySelectedTimeRange

		#region VerifyVisibleDates
		private void VerifyVisibleDates()
		{
			// process/update the visible dates
			if ( this.GetFlag(InternalFlags.VisibleDatesChanged) )
			{
				// let the derived classes do any massaging/manipulation
				// of the visible dates that they need to
				this.VerifyVisibleDatesOverride();

				// then make sure the dates are sorted
				this.VisibleDates.Sort();

				// clear any flag indicating that the visible dates are dirty
				this.SetFlag(InternalFlags.VisibleDatesChanged, false);

				this.VerifySelectionForVisibleDates();
			}
		}
		#endregion // VerifyVisibleDates

		#endregion //Private Methods

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

		#endregion //Methods

		#region Events

		#region ActiveCalendarChanged

		/// <summary>
		/// Used to invoke the <see cref="ActiveCalendarChanged"/> event.
		/// </summary>
		/// <param name="oldValue">The old property value</param>
		/// <param name="newValue">The new property value</param>
		/// <seealso cref="ActiveCalendarChanged"/>
		/// <seealso cref="ActiveCalendar"/>
		protected virtual void OnActiveCalendarChanged(ResourceCalendar oldValue, ResourceCalendar newValue)
		{
			RoutedPropertyChangedEventArgs<ResourceCalendar> args = new RoutedPropertyChangedEventArgs<ResourceCalendar>(oldValue, newValue);

			RoutedPropertyChangedEventHandler<ResourceCalendar> handler = this.ActiveCalendarChanged;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Occurs when the value of the ActiveCalendar property changes
		/// </summary>
		/// <seealso cref="OnActiveCalendarChanged(ResourceCalendar, ResourceCalendar)"/>
		/// <seealso cref="ActiveCalendar"/>
		public event RoutedPropertyChangedEventHandler<ResourceCalendar> ActiveCalendarChanged;

		#endregion //ActiveCalendarChanged

		#region SelectedActivitiesChanged

		/// <summary>
		/// Used to invoke the <see cref="SelectedActivitiesChanged"/> event.
		/// </summary>
		/// <seealso cref="SelectedActivitiesChanged"/>
		/// <seealso cref="SelectedActivities"/>
		/// <seealso cref="SelectedActivitiesChangedEventArgs"/>
		protected internal virtual void OnSelectedActivitiesChanged()
		{
			_activitySelectedTimeRange = null;

			this.QueueInvalidation(InternalFlags.ActivitySelectionChanged);

			// the selected activities and timeslots are mutually exclusive
			int count = this.SelectedActivities.Count;
			if (count > 0 && this.SelectedTimeRange != null)
				this.SelectedTimeRange = null;

			if (count == 0)
				this.QueueInvalidation(InternalFlags.RestoreSelectedTimeRange);

			// AS 11/1/10 TFS58843
			// In case we get to this point and are still in edit mode then we should 
			// exit edit mode now.
			// 
			if ( _editHelper != null && _editHelper.IsInPlaceEdit )
			{
				if ( this.SelectedActivities.Count != 1 || this.SelectedActivities[0] != this.EditHelper.Activity )
				{
					// this can happen in sl because it has lost focus but we haven't been notified of that yet
					//Debug.Assert(false, "We are in edit mode but the activity is not selected?");
					this.EditHelper.EndEdit(false, true);
				}
			}

			SelectedActivitiesChangedEventArgs args = new SelectedActivitiesChangedEventArgs();

			EventHandler<SelectedActivitiesChangedEventArgs> handler = this.SelectedActivitiesChanged;

			if (null != handler)
				handler(this, args);

			ScheduleControlBaseAutomationPeer peer = FrameworkElementAutomationPeer.FromElement(this) as ScheduleControlBaseAutomationPeer;

			if (peer != null)
				peer.RaiseSelectionEvents();
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
		protected virtual void OnSelectedTimeRangeChanged(DateRange? oldValue, DateRange? newValue)
		{
			NullableRoutedPropertyChangedEventArgs<DateRange> args = new NullableRoutedPropertyChangedEventArgs<DateRange>(oldValue, newValue);

			NullableRoutedPropertyChangedEventHandler<DateRange> handler = this.SelectedTimeRangeChanged;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Occurs when the value of the SelectedTimeRange property changes
		/// </summary>
		/// <seealso cref="OnSelectedTimeRangeChanged(DateRange?, DateRange?)"/>
		/// <seealso cref="SelectedTimeRange"/>
		public event NullableRoutedPropertyChangedEventHandler<DateRange> SelectedTimeRangeChanged;

		#endregion //SelectedTimeRangeChanged

		#endregion //Events

		#region IScheduleControl

		void IScheduleControl.OnColorSchemeResolvedChanged()
		{
			XamScheduleDataManager dm = this.DataManagerResolved;

			if (dm != null)
			{
				CalendarColorScheme scheme = dm.ColorSchemeResolved;

				this.DefaultBrushProvider = scheme.DefaultBrushProvider;
				this.ScrollBarStyle = scheme.ScrollBarStyle;
			}
		}

		void IScheduleControl.OnSettingsChanged( object sender, string property, object extraInfo )
		{
			this.OnSubObjectChanged(sender, property, extraInfo);
		}

		#region VerifyInitialState

		void IScheduleControl.VerifyInitialState(List<DataErrorInfo> errorList)
		{
			this.VerifyInitialState(errorList);
		}

		#endregion //VerifyInitialState

		#endregion //IScheduleControl

		// AS 12/8/10 NA 11.1 - XamOutlookCalendarView
		#region ISelectedActivityCollectionOwner Members

		void ISelectedActivityCollectionOwner.OnSelectedActivitiesChanged()
		{
			this.OnSelectedActivitiesChanged();
		}

		#endregion //ISelectedActivityCollectionOwner Members

		#region InternalFlags enum
		[Flags]
		private enum InternalFlags : long
		{
			// verifystate related flags
			SelectedTimeRangeChanged			= 1L << 0,
			CalendarGroupsChanged				= 1L << 1,
			DataManagerChanged					= 1L << 2,
			VisibleDatesChanged					= 1L << 3,
			CurrentDateChanged					= 1L << 4,
			CalendarGroupsResolvedChanged		= 1L << 5,
			SupportedActivityTypes				= 1L << 6,
			ActiveCalendarOrGroup				= 1L << 7,

			// ui related verify state flags
			MergedScrollInfosChanged			= 1L << 12,
			TimeslotPanelExtents				= 1L << 13,
			ActivitySelectionChanged			= 1L << 14,
			NotifyPanelVisibleDateChange		= 1L << 15,
			NotifyCurrentUserChange				= 1L << 16,
			ClickToAddEnabledChange				= 1L << 17, // AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled

			// general state
			IsInitializing						= 1L << 22,
			IsVerifyingState					= 1L << 23,
			RestoreSelectedTimeRange			= 1L << 24,
			InitializeTemplatePanel				= 1L << 25,
			BrushVersionBindingRetryFailed		= 1L << 26,
			IsAllDaySelection					= 1L << 27,
			
			// these are the flags that are considered dirty initially and during refresh display
			AllVerifyStateFlags = CalendarGroupsChanged 
				| DataManagerChanged 
				| VisibleDatesChanged 
				| CurrentDateChanged 
				| CalendarGroupsResolvedChanged 
				| SelectedTimeRangeChanged 
				| ActiveCalendarOrGroup,

			AllVerifyUIStateFlags = MergedScrollInfosChanged 
				| TimeslotPanelExtents 
				| ActivitySelectionChanged 
				| NotifyPanelVisibleDateChange 
				| NotifyCurrentUserChange
				| ClickToAddEnabledChange, // AS 3/1/11 NA 2011.1 IsAddViaClickToAddEnabled
		} 
		#endregion // InternalFlags enum

		#region CalendarGroupsProvider enum
		private enum CalendarGroupsProvider
		{
			/// <summary>
			/// There is no current user so nothing is shown.
			/// </summary>
			None,

			/// <summary>
			/// The control is using its CalendarGroupsOverride
			/// </summary>
			ControlGroups,

			/// <summary>
			/// The control is using the CalendarGroups of the DataManager
			/// </summary>
			DataManagerGroups,

			/// <summary>
			/// The control is displaying the PrimaryCalendar of the CurrentUser
			/// </summary>
			CurrentUser,

		}
		#endregion // CalendarGroupsProvider enum

		#region ActivityDeletionHelper private class

		private class ActivityDeletionHelper
		{
			#region Private Members

			private ScheduleControlBase _control;
			private List<ActivityBase> _activitiesToDelete;
			private ActivityRecurrenceChooserDialog.ChooserResult _result;

			#endregion //Private Members	
    
			#region Constructor

			internal ActivityDeletionHelper(ScheduleControlBase control)
			{
				_control = control;

				control.SelectedActivities.BeginUpdate();
				_activitiesToDelete = new List<ActivityBase>(control.SelectedActivities);

				// reverse the list so we can process from the end for efficiency
				_activitiesToDelete.Reverse();
			}

			#endregion //Constructor	
    
			#region Internal Methods

			#region ProcessDeletions

			internal void ProcessDeletions()
			{
				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm == null || dm.DataConnector == null)
				{
					this.EndProcessing();
					return;
				}

				ActivityBase activity = null;
				RecurrenceChooserType chooserType = RecurrenceChooserType.ChooseForDeletion;

				while (activity == null)
				{
					int count = this._activitiesToDelete.Count;

					if (count == 0)
					{
						this.EndProcessing();
						return;
					}

					// get (and remove) the last activity in the list (we reversed the list order in the ctor)
					activity = this._activitiesToDelete[count - 1];

					this._activitiesToDelete.RemoveAt(count - 1);
					
					// bypass activities with pending operations
					if (activity.PendingOperation != null ||
						!dm.IsActivityOperationAllowedHelper(activity, ActivityOperation.Remove))
					{
						// clear the activity stack variable to we remain in the while loop
						// to process the next activity
						activity = null;
						continue;
					}

					// if this is an occurrence then break out so we will display the
					// RecurrenceChooserDialog below
					if (activity.RootActivity != null)
						break;

					// If this is a recurring task root and the connector only generates the current task occurrence, 
					// this actually represents the current occurrence, so we need to show the chooser dialog for it, but
					// only if it hasn't been completed yet. If it was completed, then the next occurrence was already
					// generated and we can just delete this activity.
					Task task = activity as Task;
					if (task != null &&
						task.IsRecurrenceRoot &&
						task.PercentComplete < 100 &&
						dm.DataConnector.RecurringTaskGenerationBehavior == RecurringTaskGenerationBehavior.GenerateCurrentTask)
					{
						chooserType = RecurrenceChooserType.ChooseForCurrentTaskDeletion;
						break;
					}

					if (!this.DeleteActivity(activity))
					{
						this.EndProcessing();
						return;
					}

					// clear the activity stack variable to we remain in the while loop
					// to process the next activity
					activity = null;
				}

				// Display the RecurrenceChooserDialog to let the user decide whether we should delete the occurrence or the Series.
				this._result =
					new ActivityRecurrenceChooserDialog.ChooserResult(activity);

				if (false == dm.DisplayActivityRecurrenceChooserDialog(_control,
													ScheduleUtilities.GetString("DLG_RecurrenceChooserDialog_Title_Delete"),
													activity,
													chooserType,
													this._result,
													null,
													this.OnRecurrenceChooserDialogClosed))
				{
					this.EndProcessing();
				}

			}

			#endregion //ProcessDeletions

			#endregion //Internal Methods	
        
			#region Private Methods
    
			#region DeleteActivity

			private bool DeleteActivity(ActivityBase activity)
			{
				if (activity.PendingOperation != null)
					return true; // continue processing the others

				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm != null && dm.IsActivityOperationAllowedHelper(activity, ActivityOperation.Remove))
				{
					ActivityOperationResult result = dm.Remove(activity);

					if (result.IsComplete)
					{
						// if there was an error thn display an error message
						if (result.Error != null)
						{
							dm.DisplayErrorMessageBox(result.Error, ScheduleUtilities.GetString("MSG_TITLE_DeleteAppointment"));

							return false;
						}
					}
					else
						dm.AddPendingOperation(result);
				}

				return true;
			}

			#endregion //DeleteActivity	

			#region EndProcessing

			private void EndProcessing()
			{
				this._activitiesToDelete.Clear();
				this._control.SelectedActivities.EndUpdate();
				this._control.OnRecurrenceDeletionEnded(this);
			}

			#endregion //EndProcessing	
    
			#region OnRecurrenceChooserDialogClosed

			private void OnRecurrenceChooserDialogClosed(bool? dialogResult)
			{
				XamScheduleDataManager dm = _control.DataManagerResolved;

				Debug.Assert(dm != null, "XamScheduleDataManager not found");

				if (dm == null || dm.DataConnector == null || this._result.Choice == RecurrenceChooserChoice.None || dialogResult != true)
				{
					this.EndProcessing();
					return;
				}

				ActivityBase activity = this._result.UserData as ActivityBase;

				ActivityBase activityToDelete = null;

				if (this._result.Choice == RecurrenceChooserChoice.Series)
				{
					activityToDelete = activity.RootActivity;

					if (activityToDelete != null)
					{
						// walk over the _activitiesToDelete collection backwards and
						// remove all pending activities for the same root
						for (int i = _activitiesToDelete.Count - 1; i >= 0; i--)
						{
							ActivityBase root = _activitiesToDelete[i].RootActivity;

							if (root != null && root == activityToDelete)
							{
								_activitiesToDelete.RemoveAt(i);
							}
						}
					}
				}
				else if (this._result.Choice == RecurrenceChooserChoice.Occurrence)
				{
					Task task = activity as Task;

					// If we are deleting a task occurrence and the task generation behavior is GenerateCurrentTask,
					// the task object is always the recurring root. So to delete the occurrence, we need to create 
					// a temporary task occurrence that has a root activity of the task being deleted.
					if (task != null &&
						task.IsRecurrenceRoot &&
						dm.DataConnector.RecurringTaskGenerationBehavior == RecurringTaskGenerationBehavior.GenerateCurrentTask)
					{
						activityToDelete = ScheduleUtilities.CreateTaskOccurrenceToBeDeleted(task);
					}
				}

				if (activityToDelete == null)
					activityToDelete = activity;

				if (!this.DeleteActivity(activityToDelete))
				{
					this.EndProcessing();
					return;
				}

				this.ProcessDeletions();
			}

			#endregion //OnRecurrenceChooserDialogClosed

			#endregion //Private Methods
		}

		#endregion //ActivityDeletionHelper	

		// JJD 12/02/10 - TFS59874 -added
		#region ActivityDlgDisplayHelper private class

		private class ActivityDlgDisplayHelper
		{
			#region Private Members

			private ScheduleControlBase _control;
			private List<ActivityBase> _activities;

			#endregion //Private Members	
    
			#region Constructor

			internal ActivityDlgDisplayHelper(ScheduleControlBase control)
			{
				_control = control;

				_activities = new List<ActivityBase>(control.SelectedActivities);

				// reverse the list so we can process from the end for efficiency
				_activities.Reverse();
			}

			#endregion //Constructor	
    
			#region Internal Methods

			#region ProcessActivities

			internal void ProcessActivities()
			{
				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm == null)
				{
					this.EndProcessing();
					return;
				}

				ActivityBase activity = null;

				while (activity == null)
				{
					int count = this._activities.Count;

					if (count == 0)
					{
						this.EndProcessing();
						return;
					}

					// get (and remove) the last activity in the list (we reversed the list order in the ctor)
					activity = this._activities[count - 1];

					this._activities.RemoveAt(count - 1);
					
					// bypass activities with pending operations
					if (activity.PendingOperation != null)
					{
						// clear the activity stack variable to we remain in the while loop
						// to process the next activity
						activity = null;
						continue;
					}

					this.DisplayActivityDlg(activity);

					break;
				}

				// call this metod asynchronously to process the next activity
				this._control.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.ProcessActivities));
			}

			#endregion //ProcessDeletions

			#endregion //Internal Methods	
        
			#region Private Methods

			#region DisplayActivityDlg

			private void DisplayActivityDlg(ActivityBase activity)
			{
				if (activity.PendingOperation != null)
					return; // continue processing the others

				this._control.DisplayActivityDialog(activity);
			}

			#endregion //DisplayActivityDlg

			#region EndProcessing

			private void EndProcessing()
			{
				this._activities.Clear();
				this._control.OnActivityDlgDisplayEnded(this);
			}

			#endregion //EndProcessing	
 
			#endregion //Private Methods
		}

		#endregion //ActivityDlgDisplayHelper	

		#region CalendarGroupsController class
		private class CalendarGroupsController : IPropertyChangeListener
		{
			#region Member Variables

			// the owning control
			private ScheduleControlBase _control;

			// an enum indicating who provided the groups we are using
			private CalendarGroupsProvider _currentProvider;

			// the CalendarGroupCollection that provided the groups
			private CalendarGroupCollection _currentGroupsSource;

			// the current user we are watching...
			private Resource _currentUser;

			// the collection of calendar groups backing the public read-only collection
			// these are the actual groups that the control will display
			private IList<CalendarGroupBase> _currentActualGroups;

			private CalendarColorScheme _lastColorScheme;
			private object _lastColorSchemeToken;

			#endregion // Member Variables

			#region Constructor
			internal CalendarGroupsController(ScheduleControlBase control)
			{
				CoreUtilities.ValidateNotNull(control);
				_control = control;
			}
			#endregion // Constructor

			#region Properties
			
			#region CalendarGroupsResolvedSource
			internal CalendarGroupCollection CalendarGroupsResolvedSource
			{
				get 
				{
					this.VerifyState();

					return _currentGroupsSource; 
				}
			}
			#endregion // CalendarGroupsResolvedSource

			#region CurrentProvider
			internal CalendarGroupsProvider CurrentProvider
			{
				get { return _currentProvider; }
			} 
			#endregion // CurrentProvider

			#endregion // Properties

			#region Methods

			#region ReleaseCurrentUserBrushProvider
			private void ReleaseCurrentUserBrushProvider()
			{
				if (_lastColorScheme != null)
				{
					Debug.Assert(_currentProvider == CalendarGroupsProvider.CurrentUser);
					var scheme = _lastColorScheme;
					var token = _lastColorSchemeToken;
					_lastColorScheme = null;
					_lastColorSchemeToken = null;

					scheme.BeginProviderAssigments(this);
					scheme.UnassignProvider(_currentActualGroups[0].SelectedCalendar);
					scheme.EndProviderAssigments();

					GC.KeepAlive(token);
				}
			}
			#endregion // ReleaseCurrentUserBrushProvider

			#region VerifyState
			internal void VerifyState()
			{
				if (!_control.GetFlag(InternalFlags.CalendarGroupsResolvedChanged))
					return;

				// first get the datamanager to make sure its initialized
				XamScheduleDataManager dm = _control.DataManagerResolved;

				CalendarGroupsProvider newProvider = CalendarGroupsProvider.None;
				Resource newUserToWatch = null;
				CalendarGroupCollection newGroupsSource = null;
				IList<CalendarGroupBase> newActualGroups;
				ResourceCalendar currentUserCalendar = null; // AS 9/2/10 - TFS37171

				// first try to use the calendar groups of the control
				if (_control.CalendarGroupsOverride.Count > 0)
				{
					newGroupsSource = _control.CalendarGroupsOverride;
					newProvider = CalendarGroupsProvider.ControlGroups;
				}
				else if (dm != null) // otherwise try to use the datamanager's groups
				{
					newGroupsSource = dm.CalendarGroups;

					if (newGroupsSource.Count > 0)
						newProvider = CalendarGroupsProvider.DataManagerGroups;
				}

				if (newProvider != CalendarGroupsProvider.None)
				{
					if (newProvider != CalendarGroupsProvider.CurrentUser)
						this.ReleaseCurrentUserBrushProvider();

					// make sure it has parsed the groups since the flat collection wouldn't be updated
					newGroupsSource.ProcessPendingChanges();

					// if we are going to use one of these then 
					switch (_control.CalendarDisplayModeResolved)
					{
						default:
						case Infragistics.Controls.Schedules.CalendarDisplayMode.Overlay:
							newActualGroups = newGroupsSource.BaseCollectionWrapper;
							break;
						case Infragistics.Controls.Schedules.CalendarDisplayMode.Separate:
							newActualGroups = newGroupsSource.FlatCalendarGroups;
							break;
						case Infragistics.Controls.Schedules.CalendarDisplayMode.Merged:
							newActualGroups = newGroupsSource.MergedCalendarGroups;
							break;
					}
				}
				else
				{
					newUserToWatch = dm != null ? dm.CurrentUser : null;

					if (null != newUserToWatch)
					{
						// as long as we have a current user we want to watch him
						newProvider = CalendarGroupsProvider.CurrentUser;

						ResourceCalendar calendar = newUserToWatch.PrimaryCalendar;

						if (calendar == null)
							newActualGroups = new CalendarGroupBase[0];
						else
						{
							newActualGroups = new CalendarGroupBase[] { new SingleItemCalendarGroup(calendar) };
							currentUserCalendar = calendar; // AS 9/2/10 - TFS37171
						}
					}
					else
					{
						// we have to wait for the datamanager to be set or the current user to change
						newProvider = CalendarGroupsProvider.None;
						newActualGroups = new CalendarGroupBase[0];
					}
				}

				// if the user we will be watching has changed then unhook from the old one
				if (newUserToWatch != _currentUser)
					ScheduleUtilities.ManageListenerHelper(ref _currentUser, newUserToWatch, this, true);

				CalendarColorScheme newColorScheme = dm != null ? dm.ColorSchemeResolved : null;

				bool hasChanged = newProvider != _currentProvider		// different provider of the groups
					|| _currentGroupsSource != newGroupsSource			// the calendargroup collection reference changed (should only really come up in the datamanager reference changes)
					|| !ScheduleUtilities.AreEqual(newActualGroups, _currentActualGroups, null) // this is really to catch CurrentUser,PrimaryCalendar, etc.
					|| (newColorScheme != _lastColorScheme && newProvider == CalendarGroupsProvider.CurrentUser) // color scheme since we assign a provider based on the color scheme
					|| (currentUserCalendar != null && currentUserCalendar.NeedsNewBrushProvider) // AS 9/1/10 - TFS37171 - base color since we assign a provider based on the color scheme
					;

				if (hasChanged)
				{
					// if the source collection providing (or potentially providing the groups) has changed then 
					// remove the old listener and add the new one. note we will use a weak listener if this is not 
					// the control's groups (i.e. its that of the datamanager)
					if (_currentGroupsSource != newGroupsSource)
					{
						ScheduleUtilities.ManageListenerHelper(ref _currentGroupsSource, newGroupsSource, _control._listener, _currentGroupsSource != _control._calendarGroupsOveride);
					}

					#region BrushProvider Assignment for implicit current user group

					this.ReleaseCurrentUserBrushProvider();

					if (newProvider == CalendarGroupsProvider.CurrentUser && newActualGroups.Count > 0)
					{
						_lastColorScheme = newColorScheme;

						if (null != _lastColorScheme)
						{
							ResourceCalendar calendar = newActualGroups[0].SelectedCalendar;

							// we have to hold a strong reference to the token as the calendar group collection would
							_lastColorSchemeToken = calendar.BrushProviderToken;

							_lastColorScheme.BeginProviderAssigments(this);
							_lastColorScheme.AssignProvider(calendar);
							_lastColorScheme.EndProviderAssigments();
						}
					} 
					#endregion // BrushProvider Assignment for implicit current user group

					_currentProvider = newProvider;
					_currentActualGroups = newActualGroups;
					_control.CalendarGroupsResolved.SetSourceCollection(newActualGroups);

					_control.QueueInvalidation(InternalFlags.CalendarGroupsChanged);
				}

				_control.SetFlag(InternalFlags.CalendarGroupsResolvedChanged, false);
			}
			#endregion // VerifyState

			#endregion // Methods

			#region ITypedPropertyChangeListener<object,string> Members

			void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
			{
				if (dataItem == _currentUser)
				{
					if (_lastColorScheme != null || !_control.IsVerifyingState)
					{
						if (string.IsNullOrEmpty(property) || property == "PrimaryCalendar")
							_control.QueueInvalidation(InternalFlags.CalendarGroupsResolvedChanged);
					}
				}
				else
				{
					// ignore changes while unassigning the provider resources
					if (_lastColorScheme != null)
					{
						// AS 9/2/10 TFS37171
						ResourceCalendar calendar = dataItem as ResourceCalendar;

						if (null != calendar && _currentProvider == CalendarGroupsProvider.CurrentUser && _currentActualGroups != null && _currentActualGroups.Count == 1 && calendar == _currentActualGroups[0].SelectedCalendar)
						{
							if (string.IsNullOrEmpty(property) || property == "BaseColor")
							{
								if (calendar.NeedsNewBrushProvider)
									_control.QueueInvalidation(InternalFlags.CalendarGroupsResolvedChanged);
							}
						}
					}
				}
			}

			#endregion //ITypedPropertyChangeListener<object,string> Members
		} 
		#endregion // CalendarGroupsController class

		#region TemplateItemSizeChangeHelper class
		private class TemplateItemSizeChangeHelper
		{
			#region Member Variables

			System.Enum _widthId;
			System.Enum _heightId;
			ScheduleControlBase _control;
			FrameworkElement _source;

			#endregion // Member Variables

			#region Constructor
			internal TemplateItemSizeChangeHelper(FrameworkElement fe, System.Enum widthId, System.Enum heightId, ScheduleControlBase control)
			{
				_control = control;
				_widthId = widthId;
				_heightId = heightId;
				_source = fe;

				fe.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
			}
			#endregion // Constructor

			#region Properties
			internal FrameworkElement Source
			{
				get { return _source; }
			}
			#endregion // Properties

			#region Methods
			private void OnSizeChanged(object sender, SizeChangedEventArgs e)
			{
				if (_widthId != null)
					_control.SetTemplateItemExtent(_widthId, e.NewSize.Width);

				if (_heightId != null)
					_control.SetTemplateItemExtent(_heightId, e.NewSize.Height);
			}

			internal void Release()
			{
				_source.SizeChanged -= new SizeChangedEventHandler(OnSizeChanged);
			}
			#endregion // Methods
		}
		#endregion // TemplateItemSizeChangeHelper class

		#region ScheduleControlTemplateValue enum
		internal enum ScheduleControlTemplateValue
		{
			SingleLineApptHeight,
			MoreActivityIndicatorHeight,
		} 
		#endregion // ScheduleControlTemplateValue enum
    
		#region ScheduleEditHelper class
		internal class ScheduleEditHelper : IPropertyChangeListener
		{
			#region Member Variables

			private ScheduleControlBase _control;
			private ActivityBase _activity;				// activity being edited
			private ActivityPresenter _editPresenter;	// activity presenter whose IsInEditMode is true if doing an inplace edit or null if doing a resize
			private DragControllerBase _dragController;	// object managing a drag or resize operation if a resize edit is in progress

			[ThreadStatic]
			private static WeakDictionary<ActivityBase, ScheduleEditHelper> _editHosts;		// stores the control managing the edit mode for an activity

			#endregion // Member Variables

			#region Constructor
			internal ScheduleEditHelper(ScheduleControlBase control)
			{
				_control = control;
			}
			#endregion // Constructor

			#region Properties

			#region Activity
			/// <summary>
			/// Returns the activity being edited
			/// </summary>
			internal ActivityBase Activity
			{
				get { return _activity; }
			}
			#endregion // Activity

			#region IsDragging
			/// <summary>
			/// Returns a boolean indicating if a drag operation is in progress
			/// </summary>
			internal bool IsDragging
			{
				get { return _dragController is ActivityDragController; }
			}
			#endregion // IsDragging

			// AS 11/1/10 TFS58843
			#region IsInPlaceEdit
			internal bool IsInPlaceEdit
			{
				get { return _activity != null && _editPresenter != null; }
			} 
			#endregion // IsInPlaceEdit

			#region IsResizing
			/// <summary>
			/// Returns a boolean indicating if a resize operation is in progress
			/// </summary>
			internal bool IsResizing
			{
				get { return _dragController is ActivityResizeController; }
			} 
			#endregion // IsResizing

			#region IsInTimeslotSelectionDrag
			internal bool IsInTimeslotSelectionDrag
			{
				get { return _dragController is TimeSelectionDragController; }
			} 
			#endregion // IsInTimeslotSelectionDrag

			#endregion // Properties

			#region Methods

			#region AddNew
			/// <summary>
			/// Initiates an inplace edit for a new activity with the specified information
			/// </summary>
			/// <param name="startLocal">The start of the appointment in the local timezone</param>
			/// <param name="endLocal">The end of the appointment in the local timezone</param>
			/// <param name="isAllDay">True if the appointment is an all day event</param>
			/// <param name="calendar">The calendar for which the appt is being created</param>
			/// <returns>True if the add occurred and it began edit mode; otherwise false</returns>
			internal bool AddNew(DateTime startLocal, DateTime endLocal, bool isAllDay, ResourceCalendar calendar)
			{
				ActivityPresenter ap;
				return this.AddNew(startLocal, endLocal, isAllDay, calendar, out ap);
			}
			/// <summary>
			/// Initiates an inplace edit for a new activity with the specified information
			/// </summary>
			/// <param name="startLocal">The start of the appointment in the local timezone</param>
			/// <param name="endLocal">The end of the appointment in the local timezone</param>
			/// <param name="isAllDay">True if the appointment is an all day event</param>
			/// <param name="calendar">The calendar for which the appt is being created</param>
			/// <param name="activityPresenter">The activity presenter that represents the newly added activity</param>
			/// <param name="activityType">Type of activity to create</param>
			/// <returns>True if the add occurred and it began edit mode; otherwise false</returns>
			internal bool AddNew(DateTime startLocal, DateTime endLocal, bool isAllDay, ResourceCalendar calendar, out ActivityPresenter activityPresenter, ActivityType activityType = ActivityType.Appointment)
			{
				activityPresenter = null;

				// end any current edit mode first
				this.EndEdit(false, false);

				// if we're still in an edit mode then don't start the add new
				if (this.Activity != null)
					return false;

				Debug.Assert(null != calendar, "No calendar?");
				Debug.Assert(null == calendar || null != calendar.OwningResource, "No owning resource for calendar?");

				var dm = _control.DataManagerResolved;

				if (dm == null || calendar == null)
					return false;

				// ensure adding is allowed
				if (!dm.IsActivityOperationAllowedHelper(activityType, ActivityOperation.Add, calendar))
					return false;

				DataErrorInfo error = null;

				var activity = dm.CreateNew(activityType, out error);

				if (error != null || activity == null)
				{
					dm.DisplayErrorMessageBox(error, ScheduleUtilities.GetString("MSG_TITLE_AddActivity"));
					return false;
				}

				// initialize the values based on the provided information
				TimeZoneInfoProvider tzProvider = _control.TimeZoneInfoProviderResolved;
				var localToken = tzProvider.LocalToken;

				activity.OwningCalendar = calendar;
				activity.OwningResource = calendar.OwningResource;
				activity.IsTimeZoneNeutral = isAllDay;
				activity.StartTimeZoneId = localToken.Id;
				activity.EndTimeZoneId = localToken.Id;
				activity.SetStartLocal(localToken, startLocal);
				activity.SetEndLocal(localToken, endLocal);

				// add the activity into the control's list of extra activities so any 
				// activity panels in use will update to display the activity if needed
				_control.ExtraActivities.Add(activity);

				ScheduleActivityPanel providerPanel = null;

				// find and update whatever provider should include this activity
				foreach (var panel in _control._timeslotPanels)
				{
					var activityPanel = panel as ScheduleActivityPanel;

					if (null != activityPanel)
					{
						var activityProvider = activityPanel.ActivityProvider;

						if (null != activityProvider && activityProvider.CouldContainActivity(activity))
						{
							if (providerPanel != null)
							{
								
							}

							providerPanel = activityPanel;
						}
					}
				}

				// if we were able to use it in a panel then update the layout 
				// so we can get to the activity presenter that represents this
				if (providerPanel != null)
				{
					activityPresenter = _control.BringIntoView(activity);
				}

				// if the edit mode was cancelled then exit
				if (!activity.IsInEdit)
				{
					return false;
				}

				// if we can't get a presenter or we have since entered edit mode 
				// then we can't continue with editing the activity so cancel the add
				if (null == activityPresenter || _activity != null)
				{
					Debug.Assert(_editPresenter == null && _activity == null, "Another edit operation started?");

					dm.CancelEdit(activity, out error);
					return false;
				}
				else
				{
					// otherwise start an edit operation
					return this.BeginEdit(activityPresenter, true);
				}
			}
			#endregion // AddNew

			#region BeginDrag

			internal bool BeginDrag(ActivityPresenter presenter, Point startDragLocation, bool copy, bool calledFromPeerMove, double deltaX, double deltaY)
			{
				// we need the root panel to be able to start a resize operation since that is needed by the resize controller
				if (_control._rootPanel == null)
					return false;

				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm == null)
					return false;

				ActivityBase activity = presenter.Activity;

				// AS 9/30/10 TFS49543
				// If the activity was in edit mode and we don't take it out now there will be 
				// lots of potential issues/complications when the drag ends. For example if 
				// the user makes a copy, no one would call end edit on the original. Similarly 
				// if they do a move to a different calendar we could end up creating a copy and 
				// again no one would call endedit on the original.
				//
				//this.TransferOutOfInPlaceEdit(activity);
				if ( !this.EndEdit(false, false) )
					return false;

				// push keyboard focus on the control. this is partially based on what we did 
				// previously whereby we wanted to ensure whatever else had focus lost it but 
				// also we want to catch key messages (e.g. pressing escape should cancel resize)
				//
				_control.Focus();

				// If the control is not a XamSchedule 
				CalendarGroupBase group = ScheduleUtilities.GetCalendarGroupFromElement(presenter);

				Debug.Assert(group != null, "No ancestor CalendarGroupPresenterBase for ActivityPresenter");

				if ( group != null &&
					activity.OwningCalendar != group.SelectedCalendar )
					group.SelectedCalendar = activity.OwningCalendar;

				ActivityDragController controller = new ActivityDragController(_control, presenter, _control._rootPanel, startDragLocation);
				
				// cache the controller 
				_dragController = controller;

				if (calledFromPeerMove)
				{
					// the peer does a single synchronous resize operation
					controller.ProcessMoveFromPeer(deltaX, deltaY);
				}
				else
				{
					this._activity = activity;

					// otherwise call BeginDrag which will capture the mouse
					if (!controller.BeginDrag(copy))
						this.EndDrag(true);

				}

				return true;
			}

			#endregion //BeginDrag

			#region BeginEdit
			internal bool BeginEdit(ActivityPresenter presenter)
			{
				return BeginEdit(presenter, false);
			}

			private bool BeginEdit(ActivityPresenter presenter, bool isNew)
			{
				ScheduleUtilities.ValidateNotNull(presenter);

				if (presenter == _editPresenter)
				{
					Debug.Assert(presenter.IsInEditMode = true, "Cached Edit Presenter is not in edit mode?");
					Debug.Assert(presenter.Activity != null && presenter.Activity.IsInEdit == true, "The activity associated with the presenter is not in edit mode?");
					return presenter.IsInEditMode;
				}

				Debug.Assert(presenter.IsInEditMode == false, "Already in edit mode?");

				ActivityBase activity = presenter.Activity;

				if (activity == null)
					return false;

				var dm = _control.DataManagerResolved;

				if (dm == null)
					return false;

				Debug.Assert(activity.IsAddNew == isNew, "The IsAddNew state doesn't match with whose calling it. We should only try to edit an activity that we created in this object. Otherwise the object could be in edit mode elsewhere.");

				// if the activity being provided is in edit
				if (!isNew)
				{
					if (activity.PendingOperation != null)
						return false;

					if (activity.IsInEdit)
					{
						EndExistingEdit(activity, dm);

						// if its still in edit then don't try to edit it in this control
						if (activity.IsInEdit)
							return false;
					}
				}

				// if we were in edit mode for an activity then end that first
				if (null != _activity)
				{
					// if we can't end edit mode then don't start edit mode
					if (!this.EndEdit(false, false))
						return false;

					Debug.Assert(_activity == null, "We're still in edit mode for something?");
					if (_activity != null)
						return false;
				}

				// if this was an add new then it would have checked if adding is allowed but otherwise
				// check if editing is allowed and begin the edit operation
				if (!isNew)
				{
					if (!dm.IsActivityOperationAllowedHelper(activity, ActivityOperation.Edit))
						return false;

					DataErrorInfo error = null;
					bool successful = dm.BeginEdit(activity, out error);

					if (!successful || error != null)
					{
						dm.DisplayErrorMessageBox(error, ScheduleUtilities.GetString("MSG_TITLE_EditActivity"));
						return false;
					}

					Debug.Assert(_activity == null, "We entered edit mode for something else while beginning an edit mode?");
					if (_activity != null)
					{
						// if we did somehow then cancel the edit mode for this activity
						if (activity.IsInEdit)
							dm.CancelEdit(activity, out error);

						return false;
					}
				}

				Debug.Assert(activity.IsInEdit, "The activity isn't in edit mode?");

				if (!activity.IsInEdit)
					return false;

				Debug.Assert(presenter.Activity == activity, "Presenter is no longer associated with the same activity?");

				CacheEditInfo(presenter, activity);

				presenter.IsInEditMode = true;

				// assuming we're in edit mode clear the selection and select just this activity
				if (_activity == activity)
				{
					_control.SelectActivity(activity, false);
				}

				Debug.Assert(_editPresenter == presenter && _activity == activity && presenter.Activity == activity, "The edit information changed while entering edit mode");

				return presenter.IsInEditMode;
			}
			#endregion // BeginEdit

			#region BeginTimeSelectionDrag

			internal bool BeginTimeSelectionDrag(TimeRangePresenterBase presenter, Point startDragLocation)
			{
				// we need the root panel to be able to start a resize operation since that is needed by the resize controller
				if (_control._rootPanel == null)
					return false;

				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm == null)
					return false;

				TimeSelectionDragController controller = new TimeSelectionDragController(_control, _control._rootPanel, presenter );
				
				// cache the controller 
				_dragController = controller;

				// call BeginDrag which will capture the mouse
				if (!controller.BeginDrag())
					this.EndTimeSelectionDrag();

				return true;
			}

			#endregion //BeginTimeSelectionDrag

			#region BeginResize

			internal bool BeginResize(ActivityBase activity, ActivityResizerBar resizerBar, bool calledFromPeerMove, double deltaX, double deltaY)
			{
				// we need the root panel to be able to start a resize operation since that is needed by the resize controller
				if (_control._rootPanel == null)
					return false;

				XamScheduleDataManager dm = _control.DataManagerResolved;

				if (dm == null)
					return false;

				ActivityResizingEventArgs args = new ActivityResizingEventArgs(activity, resizerBar.IsLeading);
				dm.OnActivityResizing(args);

				if (args.Cancel)
					return false;

				bool wasEditing = activity == _activity;

				// if we're in edit mode for something else then commit that
				if (activity.IsInEdit && !wasEditing)
				{
					// if its edit mode in a different control then try to end that edit
					EndExistingEdit(activity, dm);

					// if the activity is still in its edit mode then do not proceed
					if (activity.IsInEdit)
						return false;

					// similarly if we started edit mode for a different activity...
					Debug.Assert(_activity == null, "We still have activity?");

					if (_activity != null)
						return false;
				}

				// AS 9/29/10 TFS49543
				// Moved logic into a helper method.
				//
				this.TransferOutOfInPlaceEdit(activity);

				// push keyboard focus on the control. this is partially based on what we did 
				// previously whereby we wanted to ensure whatever else had focus lost it but 
				// also we want to catch key messages (e.g. pressing escape should cancel resize)
				//
				_control.Focus();

				// make sure the calendar is active
				if (_control.ActiveCalendar != activity.OwningCalendar)
				{
					CalendarGroupBase group = ScheduleUtilities.GetCalendarGroupFromElement(resizerBar);

					if (group != null)
						_control.SelectCalendar(activity.OwningCalendar, true, group);
				}

				// make sure the activity is selected
				if ( !_control.SelectedActivities.Contains(activity))
					_control.SelectActivity(activity, (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);

				// if the activity is not in edit mode
				if (!activity.IsInEdit)
				{
					if (!dm.IsActivityOperationAllowedHelper(activity, ActivityOperation.Edit))
						return false;

					DataErrorInfo error = null;
					bool successful = dm.BeginEdit(activity, out error);

					if (!successful || error != null)
					{
						dm.DisplayErrorMessageBox(error, ScheduleUtilities.GetString("MSG_TITLE_ResizeActivity"));
						return false;
					}

					Debug.Assert(_activity == null, "We already have an activity reference?");
					Debug.Assert(activity.IsInEdit, "Activity should be in edit mode");

					if (!activity.IsInEdit)
						return false;

					CacheEditInfo(null, activity);
				}

				Debug.Assert(_activity == activity && activity.IsInEdit, "We don't have the same activity or its not in edit mode?");

				ActivityResizeController controller = new ActivityResizeController(_control, resizerBar, _control._rootPanel, activity);

				_dragController = controller;

				if (calledFromPeerMove)
				{
					// the peer does a single synchronous resize operation
					controller.ProcessMoveFromPeer(resizerBar, deltaX, deltaY);
				}
				else
				{
					// otherwise we need to capture the mouse
					if (!controller.BeginMouseCapture(resizerBar))
					{
						// if we couldn't capture then end the edit. if the activity 
						// was in edit mode then we don't want to cancel the change 
						// but just commit the change
						this.EndEdit(!wasEditing, true);
					}
				}

				return true;
			}

			#endregion //BeginResize

			#region CacheEditInfo
			private void CacheEditInfo(ActivityPresenter presenter, ActivityBase activity)
			{
				if (_editHosts == null)
					_editHosts = new WeakDictionary<ActivityBase, ScheduleEditHelper>(true, true);

				Debug.Assert(_editHosts.ContainsKey(activity) == false, "We already have a presenter cached for this activity?");
				_editHosts[activity] = this;

				_editPresenter = presenter;
				_activity = activity;

				ScheduleUtilities.AddListener(activity, this, false);
			}
			#endregion // CacheEditInfo

			#region EndDrag
			internal bool EndDrag(bool cancel)
			{
				ActivityDragController controller = this._dragController as ActivityDragController;
				if (controller == null)
					return true;

				Debug.Assert(_activity == controller.Activity, "The control is associated with a different activity?");
				this._dragController = null;
				this._activity = null;
				Debug.Assert(_editPresenter == null, "We have an edit presenter when ending the drag?");

				controller.EndDrag(cancel);

				return true;
			}
			#endregion // EndDrag

			#region EndEdit
			internal bool EndEdit(bool cancel, bool force)
			{
				if (_activity == null)
					return true;

				return this.EndEditImpl(_activity, _editPresenter, cancel, force);
			}

			internal bool EndEdit(ActivityBase activity, bool cancel, bool force)
			{
				return EndEditImpl(activity, null, cancel, force);
			}

			internal bool EndEdit(ActivityPresenter presenter, bool cancel, bool force)
			{
				return EndEditImpl(presenter.Activity, presenter, cancel, force);
			}

			private bool EndEditImpl(ActivityBase activity, ActivityPresenter presenter, bool cancel, bool force)
			{
				Debug.Assert(activity == _activity, "Not the activity that we have cached?");
				Debug.Assert(presenter == null || presenter.Activity == activity, "Specified presenter is associated with a different activity");

				if (_activity != activity)
					return false;

				if (null == presenter && _editPresenter != null && _editPresenter.Activity == activity)
					presenter = _editPresenter;

				ActivityDragControllerBase controller = this._dragController as ActivityDragControllerBase;

				Debug.Assert(_editPresenter == null || _editPresenter.Activity == activity, "We have an edit presenter but it's associated with a different activity");
				Debug.Assert(presenter == null || (presenter != null && presenter == _editPresenter), "A presenter was provided but isn't the presenter currently in edit mode");
				Debug.Assert(controller == null || controller.Activity == activity, "We have a resize controller doing the edit but it has a different activity");

				var dm = _control.DataManagerResolved;

				Debug.Assert(null != dm, "How can we cancel if we don't have a reference to the datamanager?");

				if (dm == null)
					return false;

				// if we have an associated presenter that is in edit mode then push any pending changes
				if (presenter != null && presenter.IsInEditMode)
					presenter.ForceLostFocusBindingUpdates();

				DataErrorInfo error;
				bool result;

				if (cancel)
				{
					bool wasAddNew = activity.IsAddNew;

					dm.CancelEdit(activity, out error);
					Debug.Assert(_activity != activity, "We still have the activity even though we cancelled edit?");

					// the following isn't ideal but basically when we do an in place create, we add the activity to 
					// the extraactivities and we also select it. when the edit is cancelled, the isaddnew goes to 
					// false so we pull it out of the extraactivities as we should. however since the selected 
					// activities is not based on a query it doesn't know whether the activity is committed or 
					// not so it keeps it in there. to get around this for now whenever we are cancelling the edit 
					// of an extra activity, we will manually remove it from the selected items.
					if (wasAddNew)
						_control.SelectedActivities.Remove(activity);

					result = true;
				}
				else
				{
					result = ScheduleUtilities.EndEdit(dm, activity, force, out error);

					if (controller is ActivityResizeController)
					{
						dm.OnActivityResized(new ActivityResizedEventArgs(activity));
					}

				}

				if (null != error)
				{
					string msgId;

					if (controller is ActivityResizeController)
						msgId = "MSG_TITLE_ResizeActivity";
					else if (controller is ActivityDragController)
						msgId = "MSG_TITLE_DragActivity";
					else
						msgId = "MSG_TITLE_EditActivity";

					dm.DisplayErrorMessageBox(error, ScheduleUtilities.GetString(msgId));
				}

				return result;
			}
			#endregion // EndEdit

			#region EndTimeSelectionDrag
			internal bool EndTimeSelectionDrag()
			{
				TimeSelectionDragController controller = this._dragController as TimeSelectionDragController;
				if (controller == null)
					return true;

				this._dragController = null;

				controller.EndDrag();

				return true;
			}
			#endregion // EndTimeSelectionDrag

			#region EndExistingEdit
			private static void EndExistingEdit(ActivityBase activity, XamScheduleDataManager dm)
			{
				ScheduleEditHelper helper = null;

				// see if its in an in-place/resize edit of another schedule control
				if (_editHosts != null)
					_editHosts.TryGetValue(activity, out helper);

				// if so then have that control try to end its edit mode before we start ours
				if (helper != null)
					helper.EndEdit(activity, false, false);
				else
				{
					Debug.Assert(false, "The activity is already in edit mode somewhere else? What should we do with the result?");

					// i decided not to force it to end since we didn't start it. also doing so we wouldn't 
					// know whether we should add it to the pending operations or not.
					//
					//// try to commit the edit whereever it may be
					//dm.EndEdit(activity);
				}
			} 
			#endregion // EndExistingEdit

			#region OnActivityEndEdit
			// AS 9/29/10 TFS49543 - Added isTransferringEdit parameter
			private void OnActivityEndEdit( bool isTransferringEdit = false )
			{
				ActivityBase activity = _activity;
				ActivityPresenter presenter = _editPresenter;
				ActivityDragControllerBase resizer = _dragController as ActivityDragControllerBase;

				if ( null != _editHosts )
				{
					Debug.Assert(_editHosts.ContainsKey(activity) && _editHosts[activity]._editPresenter == presenter, "The presenter stored is different?");
					_editHosts.Remove(activity);
				}

				_activity = null;
				_editPresenter = null;
				_dragController = null;

				Debug.Assert(null != activity, "Activity reference was cleared?");

				ScheduleUtilities.RemoveListener(activity, this);

				if (presenter != null)
				{
					// as long as we came out of edit mode then send a change notification with "" 
					// in case there are any controls that wait for lostfocus before pushing in 
					// the values so we can replace their values with the restored previous values
					if (!activity.IsInEdit)
						activity.RaisePropertyChangedEvent(string.Empty);

					presenter.IsInEditMode = false;
				}

				if (resizer != null)
				{
					Debug.Assert(!isTransferringEdit, "We probably should not be calling Abort if we want to transfer edit mode since that will call CancelEdit.");
					resizer.Abort();
					this.OnResizeEnded();
				}

				if ( activity.IsAddNew )
				{
					// AS 9/29/10 TFS49543
					// If we are transferring from in place edit to using the dialog then we don't want 
					// to add the activity to all the controls since the activity will still be an add 
					// new that we have not yet called end edit on. Instead we just want to pull it out 
					// of the extra activities.
					//
					if ( !isTransferringEdit )
						_control.Dispatcher.BeginInvoke(new SendOrPostCallback(OnDelayedEndEditHelper), activity);
					else
						_control.ExtraActivities.Remove(activity);
				}
			}
			#endregion // OnActivityEndEdit

			#region OnDelayedEndEditHelper
			private void OnDelayedEndEditHelper( object param )
			{
				ActivityBase activity = param as ActivityBase;

				// when an addnew activity ends edit mode we want to put that activity into 
				// the extra activities of all the controls as long as it is still awaiting 
				// completion
				if (null != activity && activity.IsAddNew && activity.PendingOperation != null)
				{
					var dm = _control.DataManagerResolved;

					if (null != dm && _control.ExtraActivities.Contains(activity))
					{
						dm.AddExtraActivity(activity);
					}
				}
			} 
			#endregion // OnDelayedEndEditHelper

			#region OnDialogDisplaying
			// AS 9/29/10 TFS49543
			// When we are about to show the dialog for the in place add new activity 
			// then we want to end the in place edit but not call EndEdit. Instead we 
			// want to just treat it like an end edit and pull it out of the extra 
			// activities so it will just be displayed in the dialog.
			//
			internal void OnDialogDisplaying( ActivityBase activity )
			{
				if ( activity != _activity || activity == null )
					return;

				if ( activity.IsAddNew && activity.IsInEdit )
				{
					if ( this.TransferOutOfInPlaceEdit(activity) )
					{
						// force focus to the owning control so focus is pulled out 
						// of the activity presenter because we're going to remove the 
						// element from the tree
						_control.Focus();

						// treat this as ending edit mode without actually calling 
						// end edit on the activity since the dialog will be in edit
						// mode on the activity
						this.OnActivityEndEdit(true);
					}
				}
			}
			#endregion // OnDialogDisplaying

			#region OnKeyDown
			internal void OnKeyDown(KeyEventArgs e)
			{
				if (_activity != null)
			
				{
					Key key = PresentationUtilities.GetKey(e);
					switch (key)
					{

						case Key.LeftCtrl:
						case Key.RightCtrl:



							if (this._dragController is ActivityDragController)
							{
								((ActivityDragController)this._dragController).OnControlKeyToggled();

								e.Handled = true;
							}
							break;

						case Key.Escape:
							if (this._dragController != null)
								this._dragController.Abort();
							else
								this.EndEdit(true, true);

							e.Handled = true;
							break;
					}
				}
			}
			#endregion // OnKeyDown

			#region OnKeyUp
			internal void OnKeyUp(KeyEventArgs e)
			{
				if (_activity != null)
			
				{
					Key key = PresentationUtilities.GetKey(e);

					switch (key)
					{

						case Key.LeftCtrl:
						case Key.RightCtrl:



							if (this._dragController is ActivityDragController)
							{
								((ActivityDragController)this._dragController).OnControlKeyToggled();
								e.Handled = true;
							}

							break;
					}
				}
			}
			#endregion // OnKeyUp

			#region OnResizeEnded
			private void OnResizeEnded()
			{
				// since the activity panels suspended their sizing while in a resize 
				// operation we need to let them know to dirty themselves
				foreach (TimeslotPanelBase panel in _control._timeslotPanels)
				{
					var activityPanel = panel as ScheduleActivityPanel;

					if (null != activityPanel && activityPanel.UsedLastDesiredSize)
						activityPanel.InvalidateMeasure();
				}
			}
			#endregion // OnResizeEnded

			#region TransferOutOfInPlaceEdit
			// AS 9/29/10 TFS49543
			// Moved here from the BeginResize and BeginDrag methods since we need the same thing 
			// when we want to transfer out of in place edit but not end the edit mode for other 
			// scenarios as well - like when we want to show the appt dialog for the in place 
			// created actiity that is still in edit mode.
			//
			private bool TransferOutOfInPlaceEdit( ActivityBase activity )
			{
				Debug.Assert(_editPresenter == null || _editPresenter.Activity == activity, "We have an edit presenter for a different activity?");

				// if we're in an inplace edit mode for this activity then tell the presenter 
				// to come out of edit mode but don't commit the edit
				if ( _editPresenter != null && _editPresenter.Activity == activity )
				{
					ActivityPresenter currentEditPresenter = _editPresenter;
					_editPresenter = null;

					// make sure we push the values into the activity
					currentEditPresenter.ForceLostFocusBindingUpdates();

					// take the presenter out of edit mode to hide the textbox/etc
					currentEditPresenter.IsInEditMode = false;

					return true;
				}

				return false;
			}
			#endregion // TransferOutOfInPlaceEdit

			// AS 11/1/10 TFS58843
			#region TryEndInPlaceEdit
			internal bool TryEndInPlaceEdit()
			{
				if ( this.IsInPlaceEdit )
				{
					if ( !this.EndEdit(false, false) )
						return false;
				}

				return true;
			}
			#endregion // TryEndInPlaceEdit

			#endregion // Methods

			#region ITypedPropertyChangeListener<object,string> Members

			void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
			{
				if (dataItem == _activity && (string.IsNullOrEmpty(property) || property == "IsInEdit"))
				{
					// if the activity comes out of edit mode (either from us handling this or externally by the customer 
					// or by another schedule control instance) then we should clean up the elements
					if (!_activity.IsInEdit)
					{
						this.OnActivityEndEdit();
					}
				}
			}

			#endregion //ITypedPropertyChangeListener<object,string> Members
		} 
		#endregion // ScheduleEditHelper class

		#region TimeslotInfo class
		internal class ScheduleTimeslotInfo
		{
			#region Member Variables

			private ScheduleControlBase _control;
			private bool _useSingleGroup;

			private TimeSpan _timeslotInterval;
			private TimeSpan _dayOffset;
			private TimeSpan _dayDuration = TimeSpan.FromDays(1);
			private WorkingHoursSource? _workHourSource;

			// this contains a TimeslotRange[] for each TimeslotGroup used by every TimeslotCalendarGroup
			private List<TimeslotRangeGroup> _calendarGroupRanges;
			private Dictionary<DateTime, AggregateTimeRange> _currentWorkingHours;
			private bool _areWorkingHoursValid;

			#endregion //Member Variables

			#region Constructor
			internal ScheduleTimeslotInfo(ScheduleControlBase control, bool usesSingleTimeslotGroup, TimeSpan timeslotInterval)
			{
				_control = control;
				_useSingleGroup = usesSingleTimeslotGroup;
				this.TimeslotInterval = timeslotInterval;
			}
			#endregion //Constructor

			#region Properties

			#region AreGroupRangesAllocated
			internal bool AreGroupRangesAllocated
			{
				get { return _calendarGroupRanges != null; }
			}
			#endregion //AreGroupRangesAllocated

			#region AreWorkingHoursValid
			internal bool AreWorkingHoursValid
			{
				get { return _areWorkingHoursValid; }
			}
			#endregion // AreWorkingHoursValid

			#region DayDuration
			public TimeSpan DayDuration
			{
				get { return _dayDuration; }
				private set
				{
					if (value != _dayDuration)
					{
						_dayDuration = value;

						// invalidate the ranges
						this.InvalidateGroupRanges();
					}
				}
			}
			#endregion //DayDuration

			#region DayOffset
			public TimeSpan DayOffset
			{
				get { return _dayOffset; }
				private set
				{
					if (value != _dayOffset)
					{
						_dayOffset = value;

						// invalidate the ranges
						this.InvalidateGroupRanges();
					}
				}
			}
			#endregion //DayOffset

			#region WorkHourSource
			internal WorkingHoursSource? WorkHourSource
			{
				get { return _workHourSource; }
				set
				{
					if (value != _workHourSource)
					{
						_workHourSource = value;

						// invalidate the ranges
						this.InvalidateWorkingHours();
					}
				}
			}
			#endregion //WorkHourSource

			#region TimeslotInterval
			public TimeSpan TimeslotInterval
			{
				get { return _timeslotInterval; }
				set
				{
					if (value != _timeslotInterval)
					{
						Debug.Assert(value > TimeSpan.Zero && value.Ticks <= TimeSpan.TicksPerDay);

						if (value <= TimeSpan.FromSeconds(1))
							value = TimeSpan.FromSeconds(1);
						else if (value.Ticks > TimeSpan.TicksPerDay)
							value = TimeSpan.FromTicks(TimeSpan.TicksPerDay);

						_timeslotInterval = value;

						// invalidate the ranges
						this.InvalidateGroupRanges();
					}
				}
			}
			#endregion //TimeslotInterval

			#endregion //Properties

			#region Methods

			#region Private Methods

			#region AddWorkHours
			private static bool AddWorkHours(AggregateTimeRange aggregateWorkHours, AggregateTimeRange groupRangeAggregate, DateRange range)
			{
				bool hasWorkHours = false;

				foreach (DateRange workHourRange in aggregateWorkHours.CombinedRanges)
				{
					// if a block intersects with the block of this date we're concerned with
					if (workHourRange.Intersect(range))
					{
						hasWorkHours = true;

						// then include it in the visible range
						groupRangeAggregate.Add(workHourRange);
					}
				}

				return hasWorkHours;
			}
			#endregion //AddWorkHours

			#region AdjustForTimeslotBoundary
			private static void AdjustForTimeslotBoundary( AggregateTimeRange groupRangeAggregate, DateRange dayRange, TimeSpan timeslotInterval )
			{
				// make a copy since we'll be manipulating it as we go
				DateRange[] combinedRanges = new DateRange[groupRangeAggregate.CombinedRanges.Count];
				groupRangeAggregate.CombinedRanges.CopyTo(combinedRanges, 0);

				for (int j = 0; j < combinedRanges.Length; j++)
				{
					DateRange range = combinedRanges[j];

					if (!range.IntersectsWith(dayRange))
						continue;

					TimeSpan dayOffset = range.Start.Subtract(dayRange.Start);

					// timeslots start on the logical day boundary so if the offset is not
					// an integral offset we need to "fill in the gap"
					long tickMod = dayOffset.Ticks % timeslotInterval.Ticks;

					if (tickMod != 0)
					{
						// add another range to fill the gap from the start of the timeslot to the start of the range
						groupRangeAggregate.Add(new DateRange(range.Start.Subtract(TimeSpan.FromTicks(tickMod)), range.Start));
					}
				}
			}
			#endregion //AdjustForTimeslotBoundary

			#region CreateTimeslotCalendarGroupRanges
			private static List<TimeslotRangeGroup> CreateTimeslotCalendarGroupRanges(
				ScheduleControlBase control,
				IList<DateTime> sortedDates,
				TimeSpan timeslotInterval,
				TimeSpan dayOffset,
				TimeSpan dayDuration,
				Dictionary<DateTime, AggregateTimeRange> workingHours)
			{
				CoreUtilities.ValidateNotNull(control);

				List<TimeslotRangeGroup> groupRanges = new List<TimeslotRangeGroup>();
				IList<DateTime> visDates = sortedDates;
				int visDateCount = visDates.Count;

				if (visDateCount > 0)
				{
					DateInfoProvider dateProvider = ScheduleUtilities.GetDateInfoProvider(control);

					AggregateTimeRange groupRangeAggregate = new AggregateTimeRange();
					IList<TimeRange> defaultWorkHours = workingHours != null ? GetDefaultWorkingHours(control) : null;
					DateTime? start = null;
					DateTime end = dateProvider.MinSupportedDateTime;
					var localToken = control.TimeZoneInfoProviderResolved.LocalToken;

					for (int i = 0; i < visDateCount; i++)
					{
						#region Setup

						DateTime visDate = visDates[i];

						// when we move to the 2nd date and we're not unioning the timeslots we 
						// need a new aggregate as each represents a TimeslotGroup
						if (i > 0 && control.IsTimeslotRangeGroupBreak(visDates[i - 1], visDate))
						{
							// process the last aggregate
							groupRanges.Add(new TimeslotRangeGroup(start.Value, end, ScheduleUtilities.CreateRangeArray(groupRangeAggregate.CombinedRanges, timeslotInterval)));

							groupRangeAggregate = new AggregateTimeRange();
							start = null;
						}

						DateTime logicalStart;
						DateTime logicalEnd;

						DateTime? logstart = dateProvider.Add(visDate, dayOffset);

						if (logstart == null)
							continue;

						logicalStart = logstart.Value;

						DateTime? logend = dateProvider.Add(logicalStart, dayDuration);

						if (logend == null)
							continue;

						logicalEnd = logend.Value;

						if (start == null)
							start = visDate;

						end = visDate;

						// this is the block of time that we will consider
						DateRange dayRange = new DateRange(logicalStart, logicalEnd);

						DateTime tempDate = dayRange.Start.Date;
						DateTime endDate;

						DateTime? endTemp = dateProvider.Add(dayRange.End, -TimeSpan.FromMilliseconds(1));

						if (endTemp == null)
							continue;

						endDate = endTemp.Value.Date;

						bool hasWorkHours = false;

						#endregion //Setup

						#region Process All Dates For Logical Date
						while (tempDate <= endDate)
						{
							// just use the portion that intersects with the logical day 
							// for example if the logical day offset is -4 hours then 
							// the first date will be just 8pm-12am and the second date
							// will be from 12am to 8pm (exclusive of course)

							DateTime toDate;

							DateTime? temp = dateProvider.AddDays(tempDate, 1);

							if (temp == null)
								break;

							toDate = temp.Value;

							DateRange tempRange = new DateRange(tempDate, toDate);
							tempRange.Intersect(dayRange);

							if (workingHours != null)
							{
								AggregateTimeRange aggregateWorkHours;

								if (workingHours.TryGetValue(tempDate, out aggregateWorkHours))
								{
									if (AddWorkHours(aggregateWorkHours, groupRangeAggregate, tempRange))
										hasWorkHours = true;
								}
							}
							else
							{
								// just use the segment as is
								groupRangeAggregate.Add(tempRange);
							}

							tempDate = toDate;
						}

						// if we're only using the working hours but we didn't have any for this logical 
						// date then we need to create some based on the dates in that logical date and 
						// the default working hours
						if (workingHours != null && !hasWorkHours && defaultWorkHours != null)
						{
							// redo the loop
							tempDate = dayRange.Start.Date;

							while (tempDate <= endDate)
							{
								DateTime toDate;

								DateTime? temp = dateProvider.AddDays(tempDate, 1);

								if (temp == null)
									break;

								toDate = temp.Value;

								DateRange tempRange = new DateRange(tempDate, toDate);
								tempRange.Intersect(dayRange);

								// create a default range based on the default work hours or 
								// there will be no timeslots for this date
								AggregateTimeRange aggregateWorkHours = new AggregateTimeRange();
								aggregateWorkHours.Add(tempDate, defaultWorkHours, localToken);
								AddWorkHours(aggregateWorkHours, groupRangeAggregate, tempRange);

								tempDate = toDate;
							}
						}
						#endregion //Process All Dates For Logical Date

						#region Adjust Aggregate So Ranges Start on Timeslot Boundary

						// if we had working hours then the start of those workings hours
						// may not have been on a timeslot boundary so we need to fix up
						// the ranges
						if (null != workingHours)
							AdjustForTimeslotBoundary(groupRangeAggregate, dayRange, timeslotInterval);

						#endregion //Adjust Aggregate So Ranges Start on Timeslot Boundary
					}

					// create array of timeslotrange from aggregate
					if (start.HasValue)
						groupRanges.Add(new TimeslotRangeGroup(start.Value, end, ScheduleUtilities.CreateRangeArray(groupRangeAggregate.CombinedRanges, timeslotInterval)));
				}

				return groupRanges;
			}
			#endregion //CreateTimeslotCalendarGroupRanges

			#region GetDefaultWorkingHours
			private static IList<TimeRange> GetDefaultWorkingHours(ScheduleControlBase control)
			{
				XamScheduleDataManager dm = control.DataManagerResolved;

				if (null != dm)
					return dm.GetResolvedDefaultWorkingHours();

				// to return default hours anyway
				return WorkingHoursCollection.DefaultWorkingHours;
			}
			#endregion //GetDefaultWorkingHours

			#region GetSeparatedWorkingHours
			private Dictionary<DateTime, AggregateTimeRange> GetSeparatedWorkingHours(IList<DateTime> visDates, XamScheduleDataManager dataManager, ICollection<Resource> resources)
			{
				Dictionary<DateTime, AggregateTimeRange> workingHours = new Dictionary<DateTime, AggregateTimeRange>();

				if (dataManager != null)
				{
					var resourceTokens = ScheduleUtilities.GetResourcesByToken(resources, _control.TimeZoneInfoProviderResolved);
					DateInfoProvider dateProvider = ScheduleUtilities.GetDateInfoProvider(this._control);

					foreach (var pair in resourceTokens)
					{
						GetSeparatedWorkingHours(visDates, dataManager, pair.Value, pair.Key, workingHours, dateProvider, _dayOffset, _dayDuration);
					}
				}

				return workingHours;
			}

			internal static void GetSeparatedWorkingHours(IList<DateTime> visDates, XamScheduleDataManager dataManager, ICollection<Resource> resources, TimeZoneToken token, Dictionary<DateTime, AggregateTimeRange> workingHours, DateInfoProvider dateProvider, TimeSpan logicalDayOffset, TimeSpan logicalDayDuration)
			{
				int visDateCount = visDates.Count;

				HashSet<DateTime> datesProcessed = new HashSet<DateTime>();

				for (int i = 0; i < visDateCount; i++)
				{
					DateTime? start = dateProvider.Add(visDates[i], logicalDayOffset);
					DateTime? end = start.HasValue ? dateProvider.Add(start.Value, logicalDayDuration) : null;

					if (end == null)
						break;

					if (token != null)
					{
						start = token.ConvertFromLocal(start.Value);
						end = token.ConvertFromLocal(end.Value);
					}

					DateTime endDate = ScheduleUtilities.GetNonInclusiveEnd(end.Value).Date;
					DateTime tempDate = start.Value.Date;
					bool isWorkDay;

					while (tempDate <= endDate)
					{
						// if we haven't encountered this date yet...
						if (datesProcessed.Add(tempDate))
						{
							AggregateTimeRange aggregate;
							bool existingAggregate = workingHours.TryGetValue(tempDate, out aggregate);

							if (!existingAggregate)
							{
								aggregate = new AggregateTimeRange();
							}

							foreach (Resource resource in resources)
							{
								// note since this is for the purposes of filtering timeslots out we will 
								// ignore the isworkday since we need to show the date for the purposes 
								// of resolving the state we will consider the is workday state. if there 
								// were no working hours because the date is not a working date then use 
								// a set of default working hours
								IList<TimeRange> workHours = dataManager.GetWorkingHours(resource, tempDate, out isWorkDay);
								aggregate.Add(tempDate, workHours, token);
							}

							if (!existingAggregate && aggregate.Count > 0)
								workingHours[tempDate] = aggregate;
						}

						tempDate = tempDate.AddDays(1);
					}
				}

			}
			#endregion // GetSeparatedWorkingHours

			#region GetUniqueDates
			private HashSet<DateTime> GetUniqueDates(IList<DateTime> visDates, TimeZoneToken token, DateInfoProvider dateProvider)
			{
				int visDateCount = visDates.Count;

				HashSet<DateTime> dateSet = new HashSet<DateTime>();

				// get all the dates for which we need working hour info
				for (int i = 0; i < visDateCount; i++)
				{
					DateTime visDate = visDates[i];
					DateTime? start = dateProvider.Add(visDate, _dayOffset);
					DateTime? end = start.HasValue ? dateProvider.Add(start.Value, _dayDuration) : null;

					if (end == null)
						break;

					if (token != null)
					{
						start = token.ConvertFromLocal(start.Value);
						end = token.ConvertFromLocal(end.Value);
					}

					DateTime endDate = ScheduleUtilities.GetNonInclusiveEnd(end.Value).Date;
					DateTime tempDate = start.Value.Date;

					while (tempDate <= endDate)
					{
						dateSet.Add(tempDate);
						tempDate = tempDate.AddDays(1);
					}
				}

				return dateSet;
			}
			#endregion // GetUniqueDates

			#region GetUnionedWorkingHours
			private Dictionary<DateTime, AggregateTimeRange> GetUnionedWorkingHours(IList<DateTime> visDates, XamScheduleDataManager dataManager, ICollection<Resource> resources)
			{
				// we need the separated work hours since those have the context of the associated date
				Dictionary<DateTime, AggregateTimeRange> workingHours = GetSeparatedWorkingHours(visDates, dataManager, resources);

				if (workingHours.Count > 0 && visDates.Count > 0)
				{
					DateInfoProvider dateProvider = ScheduleUtilities.GetDateInfoProvider(this._control);
					AggregateTimeRange workingAggregate = new AggregateTimeRange();
					DateTime today = visDates[0];

					for (int i = 0, count = visDates.Count; i < count; i++)
					{
						DateTime? start = dateProvider.Add(visDates[i], _dayOffset);
						DateTime? end = start.HasValue ? dateProvider.Add(start.Value, _dayDuration) : null;

						if (end == null)
							break;

						DateRange logicalDayRange = new DateRange(start.Value, end.Value);

						AggregateTimeRange separatedRange;

						if (workingHours.TryGetValue(logicalDayRange.Start.Date, out separatedRange))
						{
							foreach (DateRange workingRange in separatedRange.CombinedRanges)
							{
								if (workingRange.Intersect(logicalDayRange))
								{
									workingAggregate.Add(new DateRange(today.Add(workingRange.Start.TimeOfDay), today.Add(workingRange.End.TimeOfDay)));
								}
							}
						}

						if (logicalDayRange.Start.Date != logicalDayRange.End.Date)
						{
							if (workingHours.TryGetValue(logicalDayRange.End.Date, out separatedRange))
							{
								foreach (DateRange workingRange in separatedRange.CombinedRanges)
								{
									if (workingRange.Intersect(logicalDayRange))
									{
										workingAggregate.Add(new DateRange(today.Add(workingRange.Start.TimeOfDay), today.Add(workingRange.End.TimeOfDay)));
									}
								}
							}
						}
					}

					workingHours.Clear();

					foreach (DateTime date in GetUniqueDates(visDates, null, dateProvider))
					{
						workingHours[date] = workingAggregate.Clone(date.Subtract(today));
					}
				}

				return workingHours;
			}
			#endregion // GetUnionedWorkingHours

			#region GetWorkingHours
			private Dictionary<DateTime, AggregateTimeRange> GetWorkingHours( IList<DateTime> visDates )
			{
				int visDateCount = visDates.Count;

				if (visDateCount == 0)
					return null;

				// get the Working Hours for filtering time slots
				ICollection<Resource> resources = ScheduleUtilities.GetWorkingHourResources(_workHourSource, _control);

				if (resources == null)
					return null;

				XamScheduleDataManager dataManager = _control.DataManagerResolved;

				if (dataManager == null)
					return null;

				if (_useSingleGroup)
					return this.GetSeparatedWorkingHours(visDates, dataManager, resources);
				else
					return this.GetUnionedWorkingHours(visDates, dataManager, resources);
			}
			#endregion //GetWorkingHours

			#region AreEqual
			private static bool AreEqual(Dictionary<DateTime, AggregateTimeRange> oldWorkHours, Dictionary<DateTime, AggregateTimeRange> newWorkHours)
			{
				if (oldWorkHours == newWorkHours)
					return true;

				if (oldWorkHours == null || newWorkHours == null || oldWorkHours.Count != newWorkHours.Count)
					return false;

				foreach (KeyValuePair<DateTime, AggregateTimeRange> newPair in newWorkHours)
				{
					AggregateTimeRange oldRange;

					if (!oldWorkHours.TryGetValue(newPair.Key, out oldRange))
						return false;

					IList<DateRange> oldRanges = oldRange.CombinedRanges;
					IList<DateRange> newRanges = newPair.Value.CombinedRanges;

					if (oldRanges.Count != newRanges.Count)
						return false;

					for (int i = 0, count = oldRanges.Count; i < count; i++)
					{
						if (oldRanges[i] != newRanges[i])
							return false;
					}
				}

				return true;
			}
			#endregion // AreEqual

			#endregion //Private Methods

			#region Internal Methods

			#region CalculateRangeGroups
			internal IList<TimeslotRangeGroup> CalculateRangeGroups( IList<DateTime> sortedDates )
			{
				var workingHours = this.GetWorkingHours(sortedDates);
				return CreateTimeslotCalendarGroupRanges(_control, sortedDates, _timeslotInterval, _dayOffset, _dayDuration, workingHours);
			}
			#endregion // CalculateRangeGroups

			#region CalculateTimeslotRangeGroup
			internal TimeslotRangeGroup CalculateTimeslotRangeGroup( IList<DateTime> sortedVisibleDates, DateTime date, out int rangeIndex )
			{
				var rangeGroups = CalculateRangeGroups(sortedVisibleDates);
				return GetTimeslotRangeGroup(rangeGroups, date, out rangeIndex);
			} 
			#endregion // CalculateTimeslotRangeGroup

			#region CreateTimeslotGroups
			/// <summary>
			/// Creates the adapters for a group of timeslots
			/// </summary>
			/// <param name="creatorFunc">The delegate invoked when a TimeslotBase is to be allocated</param>
			/// <param name="initializer">The delegate invoked when an item is created or Reinitialize is invoked. This is meant to be used to update state on a given timeslot</param>
			/// <param name="modifyDateFunc">Optional callback used to adjust the start/end date for a timeslot.</param>
			/// <param name="activityOwner">The calendargroup whose activities will be displayed</param>
			/// <returns>A list of timeslot group adapters that represent the groups of timeslots</returns>
			internal IList<TimeslotAreaAdapter> CreateTimeslotGroups(
				Func<DateTime, DateTime, TimeslotBase> creatorFunc,
				Action<TimeslotBase> initializer,
				Func<DateTime, DateTime> modifyDateFunc,
				CalendarGroupBase activityOwner)
			{
				ObservableCollectionExtended<TimeslotAreaAdapter> groups = new ObservableCollectionExtended<TimeslotAreaAdapter>();

				this.VerifyGroupRanges();
				List<TimeslotRangeGroup> groupInfos = _calendarGroupRanges;

				foreach (TimeslotRangeGroup groupInfo in groupInfos)
				{
					TimeslotAreaAdapter group = _control.CreateTimeslotAreaAdapter(groupInfo, creatorFunc, initializer, modifyDateFunc, activityOwner);

					if (null != group)
						groups.Add(group);
				}

				return groups;
			}
			#endregion //CreateTimeslotGroups

			// AS 11/7/11 TFS85890
			#region GetInViewRange
			/// <summary>
			///Helper method to get the timeslot range that contains (fully or partially) the specified range. 
			/// </summary>
			/// <param name="range">The range to evaluate</param>
			/// <returns>The range of timeslots that intersects with the specified range or null if none intersect.</returns>
			internal DateRange? GetInViewRange(DateRange range)
			{
				this.VerifyGroupRanges();
				var rangeGroups = _calendarGroupRanges;
				int groupIndex, rangeIndex, timeslotIndex;
				if (!GetClosestTimeslot(rangeGroups, range.Start, true, out groupIndex, out rangeIndex, out timeslotIndex))
					return null;

				var group = rangeGroups[groupIndex];
				var groupRange = group.Ranges[rangeIndex];
				var inViewRange = ScheduleUtilities.CalculateDateRange(groupRange, timeslotIndex);

				if (!range.IsEmpty)
				{
					if (!GetClosestTimeslot(rangeGroups, ScheduleUtilities.GetNonInclusiveEnd(range.End), false, out groupIndex, out rangeIndex, out timeslotIndex))
						return null;

					group = rangeGroups[groupIndex];
					groupRange = group.Ranges[rangeIndex];
					inViewRange.End = ScheduleUtilities.CalculateDateRange(groupRange, timeslotIndex).End;
				}

				if (!inViewRange.IntersectsWithExclusive(range))
					return null;

				return inViewRange;
			} 
			#endregion //GetInViewRange

			// AS 10/28/10 TFS55791/TFS50143
			#region GetNextOrClosestTimeslot
			internal DateRange? GetNextOrClosestTimeslot( DateTime newDate )
			{
				this.VerifyGroupRanges();
				return GetNextOrClosestTimeslot(_calendarGroupRanges, newDate);
			}

			// AS 11/7/11 TFS85890
			// Moved the initial portion of this routine into a separate helper routine.
			//
			//private static DateRange? GetNextOrClosestTimeslot( IList<TimeslotRangeGroup> rangeGroups, DateTime date )
			private static bool GetClosestTimeslot( IList<TimeslotRangeGroup> rangeGroups, DateTime date, bool next, out int groupIndex, out int rangeIndex, out int timeslotIndex )
			{
				groupIndex = rangeIndex = timeslotIndex = -1;

				if ( rangeGroups.Count == 0 )
					return false;

				// find the TimeslotRangeGroup that contains the date
				groupIndex = ScheduleUtilities.BinarySearch(rangeGroups, date);

				if ( groupIndex < 0 )
				{
					// if we didn't find it then use the closest next group
					groupIndex = ~groupIndex;

					// if its at the end then use the last timeslot of the last group
					// also use the last of the previous group if we're getting the nearest previous item
					if ( groupIndex == rangeGroups.Count || (!next && groupIndex > 0))
					{
						groupIndex--;
						rangeIndex = rangeGroups[groupIndex].Ranges.Length - 1;
						timeslotIndex = rangeGroups[groupIndex].Ranges[rangeIndex].TimeslotCount - 1;
					}
					else
					{
						// otherwise it was between groups in which case we use the first timeslot of the first range of the next group
						rangeIndex = 0;
						timeslotIndex = 0;
					}
				}
				else
				{
					// now find the range within the group that contains the time
					TimeslotRangeGroup g = rangeGroups[groupIndex];

					Func<TimeslotRange, DateTime, int> callback = ( TimeslotRange r, DateTime d ) =>
					{
						if ( r.StartDate > d )
							return 1;
						else if ( r.EndDate <= d ) // note we're doing <= because the end date is exclusive
							return -1;
						else
							return 0;
					};

					rangeIndex = ScheduleUtilities.BinarySearch(g.Ranges, callback, date);

					if ( rangeIndex < 0 )
					{
						rangeIndex = ~rangeIndex;

						if ( rangeIndex == g.Ranges.Length || (!next && rangeIndex > 0) )
						{
							rangeIndex--;
							timeslotIndex = g.Ranges[rangeIndex].TimeslotCount - 1;
						}
						else
						{
							timeslotIndex = 0;
						}
					}
					else
					{
						timeslotIndex = ScheduleUtilities.BinarySearch(new TimeslotRange[] { g.Ranges[rangeIndex] }, date);
					}
				}

				return true;
			}

			// AS 11/7/11 TFS85890
			private static DateRange? GetNextOrClosestTimeslot(IList<TimeslotRangeGroup> rangeGroups, DateTime date)
			{
				int groupIndex, rangeIndex, timeslotIndex;

				// AS 11/7/11 TFS85890
				// Moved the implementation into a separate method to allow reuse.
				//
				if (!GetClosestTimeslot(rangeGroups, date, true, out groupIndex, out rangeIndex, out timeslotIndex))
					return null;

				var group = rangeGroups[groupIndex];
				var range = group.Ranges[rangeIndex];
				var dateRange = ScheduleUtilities.CalculateDateRange(range, timeslotIndex);

				// our need for this routine is to mimic what outlook does where it selects the 
				// next timeslot after the one containing the current time. so we'll do the same thing
				// and so if the range contains the date then get the next one if we have one
				if ( dateRange.ContainsExclusive(date) )
				{
					if ( groupIndex < rangeGroups.Count - 1 || rangeIndex < group.Ranges.Length - 1 || timeslotIndex < range.TimeslotCount - 1 )
					{
						timeslotIndex++;

						if ( timeslotIndex >= range.TimeslotCount )
						{
							timeslotIndex = 0;
							rangeIndex++;

							if ( rangeIndex >= group.Ranges.Length )
							{
								rangeIndex = 0;
								groupIndex++;
								group = rangeGroups[groupIndex];
							}

							range = group.Ranges[rangeIndex];
						}

						dateRange = ScheduleUtilities.CalculateDateRange(range, timeslotIndex);
					}
				}

				return dateRange;
			}

			#endregion // GetNextOrClosestTimeslot

			#region GetTimeslotRange
			internal DateRange? GetTimeslotRange(DateTime date)
			{
				this.VerifyGroupRanges();
				return GetTimeslotRange(date, _calendarGroupRanges);
			}

			internal DateRange? GetTimeslotRange( DateTime date, IList<TimeslotRangeGroup> rangeGroups )
			{
				int timeslotIndex;
				var group = GetTimeslotRangeGroup(rangeGroups, date, out timeslotIndex);

				if ( null != group )
					return ScheduleUtilities.CalculateDateRange(group.Ranges, timeslotIndex, null);

				return null;
			}
			#endregion // GetTimeslotRange

			#region GetTimeslotRangeGroup
			internal TimeslotRangeGroup GetTimeslotRangeGroup(DateTime date, out int timeslotIndex)
			{
				this.VerifyGroupRanges();

				return GetTimeslotRangeGroup(_calendarGroupRanges, date, out timeslotIndex);
			}

			private static TimeslotRangeGroup GetTimeslotRangeGroup( IList<TimeslotRangeGroup> rangeGroups, DateTime date, out int timeslotIndex )
			{
				int groupIndex = ScheduleUtilities.BinarySearch(rangeGroups, date);

				if (groupIndex >= 0)
				{
					TimeslotRangeGroup group = rangeGroups[groupIndex];
					timeslotIndex = ScheduleUtilities.BinarySearch(group.Ranges, date);

					if ( timeslotIndex < 0 )
					{
						timeslotIndex = ~timeslotIndex;

						// skip dates that are before the calculated ranges
						if ( timeslotIndex == 0 && ScheduleUtilities.CalculateDateRange(group.Ranges[0], 0).Start > date )
							return null;

						int totalCount = ScheduleUtilities.GetTimeslotCount(group.Ranges);

						if ( timeslotIndex == totalCount )
							return null;
					}

					return group;
				}

				timeslotIndex = -1;
				return null;
			}
			#endregion // GetTimeslotRangeGroup

			#region GroupFromLogicalDate
			internal TimeslotRangeGroup GroupFromLogicalDate( DateTime logicalDate )
			{
				this.VerifyGroupRanges();

				foreach ( var group in _calendarGroupRanges )
				{
					if ( logicalDate >= group.Start && logicalDate <= group.End )
					{
						return group;
					}
				}

				return null;
			}
			#endregion // GroupFromLogicalDate

			#region InvalidateGroupRanges
			internal void InvalidateGroupRanges()
			{
				_calendarGroupRanges = null;
				_control.OnTimeslotGroupRangesInvalidated();
			}
			#endregion //InvalidateGroupRanges

			#region InvalidateWorkingHours
			/// <summary>
			/// Indicates that the working hours are potentially invalid. If the working hours affect the timeslots then the time ranges may need to be rebuilt.
			/// </summary>
			internal void InvalidateWorkingHours()
			{
				_areWorkingHoursValid = false;

				_control.QueueAsyncVerification();
			}
			#endregion //InvalidateWorkingHours

			#region ReinitializeTimeRanges
			/// <summary>
			/// Helper method to update the TimeslotGroup instances within a TimeslotCalendarGroup based on the current ranges.
			/// </summary>
			/// <param name="calendarGroup">The group to update</param>
			/// <param name="activityOwner">The CalendarGroup whose activities will be shown or null for no activities</param>
			internal void ReinitializeTimeRanges(CalendarGroupTimeslotAreaAdapter calendarGroup, CalendarGroupBase activityOwner)
			{
				Debug.Assert(null != _calendarGroupRanges && _areWorkingHoursValid, "Shouldn't the state have been validated by now?");

				if (_calendarGroupRanges == null || !_areWorkingHoursValid)
					return;

				Debug.Assert(_calendarGroupRanges.Count > 0, "Are there no visible dates? Should there be?");

				ObservableCollectionExtended<TimeslotAreaAdapter> groupsSource = calendarGroup.TimeslotGroupsSource as ObservableCollectionExtended<TimeslotAreaAdapter>;

				if (_calendarGroupRanges.Count == 0)
				{
					groupsSource.Clear();
					return;
				}

				if (groupsSource.Count == 0)
				{
					Debug.Assert(groupsSource.Count > 0, "There are groups but no ranges?");
					return;
				}

				Dictionary<DateRange, TimeslotAreaAdapter> oldGroups = new Dictionary<DateRange, TimeslotAreaAdapter>();

				groupsSource.BeginUpdate();

				TimeslotCollection oldTimeslots = groupsSource[0].Timeslots;

				foreach (TimeslotAreaAdapter tsGroup in groupsSource)
					oldGroups[new DateRange(tsGroup.Start, tsGroup.End)] = tsGroup;

				groupsSource.Clear();

				foreach (TimeslotRangeGroup groupInfo in _calendarGroupRanges)
				{
					DateRange range = new DateRange(groupInfo.Start, groupInfo.End);
					TimeslotAreaAdapter newGroup;

					// if we had one for that date then make sure the ranges are equal too
					if (oldGroups.TryGetValue(range, out newGroup))
					{
						TimeslotCollection timeslots = newGroup.Timeslots;

						if (!ScheduleUtilities.AreEqual(timeslots.GroupTemplates, groupInfo.Ranges, null))
							newGroup = null;
						else
							_control.OnTimeslotAreaRecycled(newGroup);
					}

					if (newGroup == null)
					{
						Debug.Assert(groupInfo.Ranges.Length > 0);

						newGroup = _control.CreateTimeslotAreaAdapter(groupInfo, oldTimeslots.CreatorFunc, oldTimeslots.Initializer, oldTimeslots.ModifyDateFunc, activityOwner);

						if (newGroup == null)
							continue;
					}

					groupsSource.Add(newGroup);
				}

				groupsSource.EndUpdate();
			}
			#endregion // ReinitializeTimeRanges

			#region VerifyGroupRanges
			internal void VerifyGroupRanges()
			{
				CalendarGroupCollection resolvedGroups = _control.CalendarGroupsResolvedSource;

				// make sure the groups are up to date since we rely on the resources to get the working hours
				if (null != resolvedGroups)
					resolvedGroups.ProcessPendingChanges();

				// if the groups definitely need to be created or there are changes in the working hours...
				if (_calendarGroupRanges == null || !_areWorkingHoursValid)
				{
					// get the current working hours
					Dictionary<DateTime, AggregateTimeRange> workHours = this.GetWorkingHours(_control.VisibleDates);

					if (_calendarGroupRanges != null)
					{
						// if the working hours have changed then rebuild the group ranges
						if (!AreEqual(workHours, _currentWorkingHours))
							this.InvalidateGroupRanges();
					}

					// in either case the working hours may be considered valid because they are either 
					// valid or we are about to create new time ranges based on the updated working hours
					_areWorkingHoursValid = true;

					if (_calendarGroupRanges == null)
					{
						// cache the working hours used to build the ranges
						_currentWorkingHours = workHours;

						_control.VisibleDates.Sort();
						Debug.Assert(_calendarGroupRanges == null);

						_calendarGroupRanges = CreateTimeslotCalendarGroupRanges( _control, _control.VisibleDates, _timeslotInterval, _dayOffset, _dayDuration, _currentWorkingHours );
						_control.OnTimeslotGroupRangesCreated();
					}
				}
			}
			#endregion //VerifyGroupRanges

			#region VerifyLogicalDaySettings
			internal void VerifyLogicalDaySettings()
			{
				TimeSpan offset, duration;
				_control.GetLogicalDayInfo(out offset, out duration);

				this.DayOffset = offset;
				this.DayDuration = duration;
			}
			#endregion //VerifyLogicalDaySettings

			#endregion //Internal Methods

			#endregion //Methods
		}
		#endregion //TimeslotInfo class

		#region WeekModeHelper class
		internal class WeekModeHelper
		{
			#region Member Variables

			internal const DayOfWeekFlags AllDays = DayOfWeekFlags.Monday | DayOfWeekFlags.Tuesday | DayOfWeekFlags.Wednesday | DayOfWeekFlags.Thursday | DayOfWeekFlags.Friday | DayOfWeekFlags.Saturday | DayOfWeekFlags.Sunday;

			private DayOfWeekFlags _daysOfWeek = AllDays;
			private DayOfWeek? _firstDayOfWeek;
			private DayOfWeek[] _daysOfWeekList;
			private ScheduleControlBase _control;
			private WorkingHoursSource? _workDaySource;
			private DateRange[] _weeks;
			private bool _activateMonth;
			private bool _isVerified;

			private const int MaxWeeks = 6;
			
			#endregion // Member Variables

			#region Constructor
			internal WeekModeHelper(ScheduleControlBase control, bool activatesMonthWeeks)
			{
				_control = control;
				_activateMonth = activatesMonthWeeks;
			}
			#endregion // Constructor

			#region Properties

			#region DaysOfWeekList
			internal DayOfWeek[] DaysOfWeekList
			{
				get
				{
					Debug.Assert(_isVerified, "Requesting days of week list while the list is unverified");

					if (null == _daysOfWeekList)
						_daysOfWeekList = GetDaysOfWeekList(_firstDayOfWeek ?? _control.DateInfoProviderResolved.DateTimeFormatInfo.FirstDayOfWeek, _daysOfWeek);

					return _daysOfWeekList;
				}
			} 
			#endregion // DaysOfWeekList

			#region FirstDayOfWeekUsed
			internal DayOfWeek? FirstDayOfWeekUsed
			{
				get { return _firstDayOfWeek; }
			} 
			#endregion // FirstDayOfWeekUsed

			#region FirstWeekStartDate
			internal DateTime? FirstWeekStartDate
			{
				get { return _weeks == null || _weeks.Length == 0 ? null : (DateTime?)_weeks[0].Start; }
			} 
			#endregion // FirstWeekStartDate

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			#region MaxWeekCount
			internal int MaxWeekCount
			{
				get { return _activateMonth ? MaxWeeks : 1; }
			} 
			#endregion // MaxWeekCount

			#region WeekCount
			internal int WeekCount
			{
				get { return _weeks == null ? 0 : _weeks.Length; }
			} 
			#endregion // WeekCount

			#region Weeks
			internal IList<DateRange> Weeks
			{
				get
				{
					return _weeks;
				}
			} 
			#endregion // Weeks

			#region WorkDaySource
			internal WorkingHoursSource? WorkDaySource
			{
				get { return _workDaySource; }
				set
				{
					if (value != _workDaySource)
					{
						_workDaySource = value;
						this.InvalidateWorkingHours();
					}
				}
			}
			#endregion // WorkDaySource

			#endregion // Properties

			#region Methods

			#region Internal Methods

			#region AdjustWeeks
			/// <summary>
			/// Changes the visible weeks by a given offset indicated by the parameters
			/// </summary>
			/// <param name="next">True to shift forward; otherwise false to shift back</param>
			/// <param name="shift">True to try and maintain the offset between the visible dates; otherwise false to drive the new visible dates by the offset first visible date</param>
			/// <param name="page">True to offset by the number of calendar groups; otherwise false to offset by a single item</param>
			/// <returns>Returns true if the visible dates were changed</returns>
			internal bool AdjustWeeks(bool next, bool shift, bool page)
			{
				if (_weeks == null)
					return false;

				List<DateTime> dates = new List<DateTime>();
				DateTime? date = this.FirstWeekStartDate;
				var calendar = _control.DateInfoProviderResolved.CalendarHelper;

				if (!_activateMonth)
				{
					DateTime start = date == null ? DateTime.Today : date.Value;
					start = calendar.AddDays(start, next ? 7 : -7);
					dates.Add(start.Date);
				}
				else
				{
					if (_weeks == null || _weeks.Length == 0)
					{
						Debug.Assert(false, "Trying to shift when we don't have any weeks defined?");
						return false;
					}

					DateTime first = _weeks[0].Start;
					DateTime last = page && !shift
						? first.AddDays(7 * (_weeks.Length - 1))
						: _weeks.Last().Start;

					int extraDelta = 0;
					first = calendar.GetFirstDayOfWeekForDate(first, last.DayOfWeek, out extraDelta);
					int delta = (int)last.Subtract(first).TotalDays + 7 + extraDelta;
					DateTime start, end;

					if (next)
					{
						if (page)
							start = calendar.AddDays(last, 7);
						else
							start = calendar.AddDays(first, 7 - extraDelta);

						if (!calendar.TryAddOffset(start, delta, CalendarHelper.DateTimeOffsetType.Days, out end))
							start = calendar.AddDays(end, -delta);
					}
					else
					{
						if (page)
							start = calendar.AddDays(first, -delta);
						else
							start = calendar.AddDays(first, -(7 + extraDelta));

						end = calendar.AddDays(start, delta);
					}

					if (start == first && end == _weeks.Last().Start)
						return false;

					if (shift)
					{
						DateTime previous = _weeks[0].Start;

						for (int i = 0; i < _weeks.Length; i++)
						{
							DateTime oldVisDate = _weeks[i].Start;
							start = calendar.AddDays(start, (int)oldVisDate.Subtract(previous).TotalDays);
							dates.Add(start);
							previous = oldVisDate;
						}
					}
					else
					{
						// otherwise add the contiguous dates based on the first displayed weeks as outlook does
						for (int i = 0; i < _weeks.Length; i++)
						{
							DateTime tempDate = start.AddDays(7 * i);
							dates.Add(tempDate);
						}
					}
				}

				
				this.InitializeFromSortedDates(dates.ToArray());
				return date != this.FirstWeekStartDate;
			}
			#endregion // AdjustWeeks

			#region BringIntoView
			internal void BringIntoView(DateTime date)
			{
				if (this.ContainsDate(date))
					return;

				if (_weeks == null)
				{
					this.Reset(date);
				}
				else
				{
					var dateProvider = _control.DateInfoProviderResolved;
					var calendarHelper = dateProvider.CalendarHelper;
					int weekCount = _weeks.Length;

					

					if ( date < _weeks[0].Start )
					{

					}
					else if ( date >= _weeks[weekCount - 1].End )
					{
						int offset;
						date = calendarHelper.GetFirstDayOfWeekForDate(date, _firstDayOfWeek, out offset);
						date = calendarHelper.AddDays(date, -7 * (weekCount - 1));
					}

					this.SetFirstWeekDate(date);
				}
			} 
			#endregion // BringIntoView

			#region CalculateWeeks
			internal IList<DateTime> CalculateWeeks(DateTime resetDate)
			{
				DateTime start, end;
				GetResetRange(resetDate, out start, out end);
				DayOfWeek? firstDayOfWeek;
				DayOfWeekFlags daysOfWeek;
				DateRange[] newWeeks;
				return CalculateWeeks(start, end, out firstDayOfWeek, out daysOfWeek, out newWeeks);
			}

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			internal IList<DateTime> CalculateWeeks( IList<DateTime> dates )
			{
				return this.CalculateWeeks(dates, _workDaySource);
			}

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			internal IList<DateTime> CalculateWeeks( IList<DateTime> dates, WorkingHoursSource? workDaySource )
			{
				DayOfWeek? firstDayOfWeek;
				DayOfWeekFlags daysOfWeek;
				DateRange[] newWeeks;
				return this.CalculateWeeks(dates, workDaySource, out firstDayOfWeek, out daysOfWeek, out newWeeks);
			}
			#endregion // CalculateWeeks

			#region ContainsDate
			internal bool ContainsDate(DateTime date)
			{
				if (_isVerified && _control.VisibleDates.ContainsDate(date))
					return true;

				if (_daysOfWeek == AllDays)
					return false;

				if (null != _weeks)
				{
					foreach (DateRange range in _weeks)
					{
						if (range.ContainsExclusive(date))
							return true;
					}
				}

				return false;
			}
			#endregion // ContainsDate

			#region GetFirstLastDayInWeek
			/// <summary>
			/// Returns the first or last day of the week containing the specified range for the weeks in view. If the range is null, the first or last date among all the weeks.
			/// </summary>
			/// <param name="first">True to get the first day of the week and false to get the last</param>
			/// <param name="date">The date to evaluate</param>
			/// <returns></returns>
			internal DateTime? GetFirstLastDayInWeek(bool first, DateTime? date)
			{
				DateRange? weekRange = null;

				if (_weeks != null)
				{
					if (date == null)
					{
						weekRange = first ? _weeks[0] : _weeks.Last();
					}
					else
					{
						foreach (DateRange week in _weeks)
						{
							if (week.ContainsExclusive(date.Value))
							{
								weekRange = week;
								break;
							}
						}
					}

				}

				if (weekRange == null)
					return null;

				DateRange range = weekRange.Value;

				DateTime start = range.Start;
				DateTime end = range.IsEmpty ? start : ScheduleUtilities.GetNonInclusiveEnd(range.End).Date;

				if (_daysOfWeek == AllDays)
				{
					return first ? start : end;
				}
				else
				{
					return GetFirstLastDayInWeekHelper(start, end, first);
				}
			}

			private DateTime GetFirstLastDayInWeekHelper(DateTime weekStart, DateTime weekEnd, bool first)
			{
				Debug.Assert(_daysOfWeek != 0);

				if (_daysOfWeek == 0)
					return weekStart;

				int adjustment = first ? 1 : -1;
				weekStart = weekStart.Date;
				weekEnd = weekEnd.Date;

				if (!first)
				{
					DateTime temp = weekStart;
					weekStart = weekEnd;
					weekEnd = temp;
				}

				do
				{
					if (CalendarHelper.IsSet(_daysOfWeek, weekStart.DayOfWeek))
						return weekStart;

					weekStart = weekStart.AddDays(adjustment);
				}
				while (weekStart != weekEnd);

				return weekEnd;
			}
			#endregion // GetFirstLastDayInWeek

			#region InitializeFromVisibleDates
			internal void InitializeFromVisibleDates()
			{
				var visDates = _control.VisibleDates;
				visDates.Sort();

				this.InitializeFromSortedDates(visDates);
			}
			#endregion // InitializeFromVisibleDates

			#region InvalidateWorkingHours
			internal void InvalidateWorkingHours()
			{
				_isVerified = false;
				_control.QueueInvalidation(InternalFlags.VisibleDatesChanged);
			}
			#endregion // InvalidateWorkingHours

			#region InvalidateFirstDayOfWeek
			internal void InvalidateFirstDayOfWeek()
			{
				if (_isVerified)
				{
					if (_firstDayOfWeek != ScheduleUtilities.GetFirstDayOfWeek(_control.DataManagerResolved))
					{
						// when the first day of week changes we need to reset the visible dates or else
						// the monthview can just keep increasing the # of weeks it is showing
						_control.VisibleDates.Clear();

						this.InvalidateWorkingHours();
					}
				}
			} 
			#endregion // InvalidateFirstDayOfWeek

			#region Reset
			internal void Reset(DateTime date)
			{
				DateTime start, end;
				GetResetRange(date, out start, out end);

				this.ResetRange(start, end);
			}
			#endregion // Reset

			#region SetFirstWeekDate
			/// <summary>
			/// Updates the weeks to include the specified week as the first week.
			/// </summary>
			/// <param name="date">A date that belongs in the first week that should be in view. The number of weeks depends on the number currently available.</param>
			internal void SetFirstWeekDate(DateTime date)
			{
				if (_weeks == null)
					this.Reset(date);
				else
				{
					var dateProvider = _control.DateInfoProviderResolved;
					var calendarHelper = dateProvider.CalendarHelper;

					int offset;
					DateTime start = calendarHelper.GetFirstDayOfWeekForDate(date, _firstDayOfWeek, out offset);
					DateTime end = calendarHelper.AddDays(date, 7 * (_weeks.Length - 1));

					this.ResetRange(start, end);
				}
			}
			#endregion // SetFirstWeekDate

			#endregion // Internal Methods

			#region Private Methods

			#region CalculateWeeks

			#region CalculateWeeks(DateTime, DateTime, out DayOfWeek?, out DayOfWeekFlags, out DateRange[])
			private List<DateTime> CalculateWeeks(DateTime start, DateTime end, out DayOfWeek? firstDayOfWeek, out DayOfWeekFlags daysOfWeek, out DateRange[] newWeeks)
			{
				var dateProvider = _control.DateInfoProviderResolved;
				var calendarHelper = dateProvider.CalendarHelper;
				var calendar = calendarHelper.Calendar;

				firstDayOfWeek = ScheduleUtilities.GetFirstDayOfWeek(_control.DataManagerResolved);
				int startOffset;
				start = calendarHelper.GetFirstDayOfWeekForDate(start, firstDayOfWeek, out startOffset);

				// note the end we are calculating is exclusive
				end = calendar.AddDays(end, 7 - calendarHelper.GetDayOfWeekNumber(end, DayOfWeekFlags.None, firstDayOfWeek));

				// constrain the # of weeks
				DateTime maxDate = start.AddDays((MaxWeeks * 7) - startOffset);

				if ( end > maxDate )
					end = maxDate;

				// get the Working Hours for filtering time slots
				ICollection<Resource> resources = ScheduleUtilities.GetWorkingHourResources(_workDaySource, _control);
				daysOfWeek = GetDaysOfWeek(new DateRange[] { new DateRange(start, end) }, resources);

				List<DateTime> visibleDates = new List<DateTime>();
				DateTime tempDate = start;

				while (tempDate < end)
				{
					DayOfWeekFlags dateDOW = (DayOfWeekFlags)(1 << (int)tempDate.DayOfWeek);

					if ((dateDOW & daysOfWeek) != 0)
						visibleDates.Add(tempDate);

					tempDate = tempDate.AddDays(1);
				}

				if (_activateMonth)
				{
					List<DateRange> weeks = new List<DateRange>();
					tempDate = start;

					int dayCount = (int)end.Subtract(start).TotalDays + startOffset;
					int weekCount = (dayCount - 1) / 7 + 1;
					DateTime weekStart = start;

					for (int i = 0; i < weekCount; i++)
					{
						DateRange weekRange;

						if (i == 0)
							weekRange = new DateRange(weekStart, start.AddDays(7 - startOffset));
						else if (i == weekCount - 1)
							weekRange = new DateRange(weekStart, end);
						else
							weekRange = new DateRange(weekStart, weekStart.AddDays(7));


						weeks.Add(weekRange);

						// prep for the next week
						weekStart = weekRange.End;
					}

					newWeeks = weeks.ToArray();
				}
				else
				{
					// single week only
					newWeeks = new DateRange[] { new DateRange(start, end) };
				}

				return visibleDates;
			}
			#endregion //CalculateWeeks(DateTime, DateTime, out DayOfWeek?, out DayOfWeekFlags, out DateRange[])

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			// Refactored this from the InitializeFromSortedDates.
			// 
			#region CalculateWeeks( IList<DateTime>, WorkingHoursSource?, out DayOfWeek?, out DayOfWeekFlags, out DateRange[] )
			private IList<DateTime> CalculateWeeks( IList<DateTime> dates, WorkingHoursSource? workDaySource, out DayOfWeek? firstDayOfWeek, out DayOfWeekFlags daysOfWeek, out DateRange[] newWeeks )
			{
				var calHelper = _control.DateInfoProviderResolved.CalendarHelper;
				firstDayOfWeek = ScheduleUtilities.GetFirstDayOfWeek(_control.DataManagerResolved);
				newWeeks = ScheduleUtilities.GetWeeks(dates, calHelper, firstDayOfWeek, MaxWeeks);
				daysOfWeek = DayOfWeekFlags.None;

				if ( newWeeks == null )
					return new DateTime[0];

				// get the Working Hours for filtering time slots
				ICollection<Resource> resources = ScheduleUtilities.GetWorkingHourResources(workDaySource, _control);
				daysOfWeek = GetDaysOfWeek(newWeeks, resources, workDaySource);

				List<DateTime> newVisibleDates = new List<DateTime>();

				foreach ( DateRange range in newWeeks )
				{
					DateTime tempDate = range.Start;
					DateTime end = range.End;

					while ( tempDate < end )
					{
						DayOfWeekFlags dateDOW = (DayOfWeekFlags)(1 << (int)tempDate.DayOfWeek);

						if ( (dateDOW & daysOfWeek) != 0 )
							newVisibleDates.Add(tempDate);

						tempDate = tempDate.AddDays(1);
					}
				}

				return newVisibleDates;
			}
			#endregion //CalculateWeeks( IList<DateTime>, WorkingHoursSource?, out DayOfWeek?, out DayOfWeekFlags, out DateRange[] )

			#endregion // CalculateWeeks

			#region GetDaysOfWeek
			private DayOfWeekFlags GetDaysOfWeek( DateRange[] ranges, ICollection<Resource> resources )
			{
				return GetDaysOfWeek(ranges, resources, _workDaySource);
			}

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			// Broke this up into a helper method so we could supply a different workdaysource than what it is currently using.
			//
			private DayOfWeekFlags GetDaysOfWeek( DateRange[] ranges, ICollection<Resource> resources, WorkingHoursSource? workDaySource )
			{
				if (workDaySource == null || resources == null)
					return AllDays;

				DayOfWeekFlags days = DayOfWeekFlags.None;
				var dm = _control.DataManagerResolved;

				if (dm != null)
				{
					var dateProvider = _control.DateInfoProviderResolved;
					var tzProvider = _control.TimeZoneInfoProviderResolved;

					TimeSpan logicalDayOffset, logicalDayDuration;
					_control.GetLogicalDayInfo(out logicalDayOffset, out logicalDayDuration);

					var table = ScheduleUtilities.GetResourcesByToken(resources, tzProvider);
					AggregateTimeRange aggregateWorkHours = new AggregateTimeRange();

					foreach (var pair in table)
					{
						var token = pair.Key;

						foreach (DateRange range in ranges)
						{
							DateTime rangeDate = range.Start;
							DateTime rangeEnd = range.End;

							while (rangeDate <= rangeEnd)
							{
								DayOfWeekFlags dateDOW = (DayOfWeekFlags)(1 << (int)rangeDate.DayOfWeek);

								// skip days that we already know are part of the work week
								if ((days & dateDOW) == 0)
								{
									#region Setup

									// otherwise get the logical range for the date...
									DateTime? start = dateProvider.Add(rangeDate, logicalDayOffset);
									DateTime? end = start.HasValue ? dateProvider.Add(start.Value, logicalDayDuration) : null;

									if (end == null)
										break;

									// convert that to a time local to the resource since working hours are floating times
									if (token != null)
									{
										start = token.ConvertFromLocal(start.Value);
										end = token.ConvertFromLocal(end.Value);
									}

									DateTime endDate = ScheduleUtilities.GetNonInclusiveEnd(end.Value).Date;
									DateTime tempDate = start.Value.Date;
									bool isWorkDay;

									// this is a temporary class so reset for each logical date processed
									aggregateWorkHours.Reset();

									#endregion // Setup

									#region Get WorkingHours
									while (tempDate <= endDate)
									{
										foreach (Resource resource in resources)
										{
											// note since this is for the purposes of filtering timeslots out we will 
											// ignore the isworkday since we need to show the date for the purposes 
											// of resolving the state we will consider the is workday state. if there 
											// were no working hours because the date is not a working date then use 
											// a set of default working hours
											IList<TimeRange> workHours = dm.GetWorkingHours(resource, tempDate, out isWorkDay);
											aggregateWorkHours.Add(tempDate, workHours, token);
										}

										tempDate = tempDate.AddDays(1);
									}
									#endregion // Get WorkingHours

									#region Check for WorkingHours in Logical Day
									// evaluate the logical day to see if there are any intersecting working hours
									DateRange logicalRange = new DateRange(start.Value, ScheduleUtilities.GetNonInclusiveEnd(end.Value));

									foreach (DateRange workHourRange in aggregateWorkHours.CombinedRanges)
									{
										if (workHourRange.IntersectsWith(logicalRange))
										{
											days |= dateDOW;

											if (days == AllDays)
												return days;

											break;
										}
									}
									#endregion // Check for WorkingHours in Logical Day
								}

								DateTime? nextDay = dateProvider.AddDays(rangeDate, 1);

								if (nextDay == null)
									break;

								rangeDate = nextDay.Value;

							} //while (rangeDate <= rangeEnd)
						} //foreach (DateRange range in ranges)
					} //foreach (var pair in table)
				} // if (dm != null)

				if (days == DayOfWeekFlags.None)
					days = ScheduleUtilities.GetDefaultWorkDays(dm);

				return days;
			}
			#endregion // GetDaysOfWeek

			#region GetDaysOfWeekList
			private static DayOfWeek[] GetDaysOfWeekList(DayOfWeek firstDayOfWeek, DayOfWeekFlags daysOfWeek)
			{
				List<DayOfWeek> days = new List<DayOfWeek>();

				for (int i = (int)firstDayOfWeek, count = i + 7; i < count; i++)
				{
					DayOfWeekFlags day = (DayOfWeekFlags)(1 << (i % 7));

					if ((day & daysOfWeek) == day)
						days.Add((DayOfWeek)(i % 7));
				}

				return days.ToArray();
			}
			#endregion // GetDaysOfWeekList

			#region GetResetRange
			private void GetResetRange(DateTime date, out DateTime start, out DateTime end)
			{
				if (_activateMonth)
				{
					var calendar = _control.DateInfoProviderResolved.CalendarHelper.Calendar;

					int year = calendar.GetYear(date);
					int month = calendar.GetMonth(date);
					int era = calendar.GetEra(date);

					start = calendar.ToDateTime(year, month, 1, 0, 0, 0, 0, era);
					end = calendar.ToDateTime(year, month, calendar.GetDaysInMonth(year, month, era), 0, 0, 0, 0, era);
				}
				else
				{
					// AS 10/26/10 TFS57198
					// Stripped out time portion.
					//
					Debug.Assert(date.TimeOfDay == TimeSpan.Zero, "The reset date shouldn't have a time portion associated with it.");
					start = end = date.Date;
				}
			} 
			#endregion // GetResetRange

			#region InitializeFromSortedDates
			private void InitializeFromSortedDates(IList<DateTime> dates)
			{
				DayOfWeek? firstDayOfWeek;
				DayOfWeekFlags daysOfWeek;
				DateRange[] newWeeks;

				// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
				// Moved the implementation into the CalculateWeeks helper method.
				//
				var newVisibleDates = this.CalculateWeeks(dates, _workDaySource, out firstDayOfWeek, out daysOfWeek, out newWeeks);

				this.OnInitialize(_control.VisibleDates, firstDayOfWeek, newWeeks, daysOfWeek, newVisibleDates);
			} 
			#endregion // InitializeFromSortedDates

			#region OnInitialize
			private void OnInitialize(DateCollection visibleDates, DayOfWeek? firstDayOfWeek, DateRange[] newWeeks, DayOfWeekFlags daysOfWeek, IList<DateTime> newVisibleDates)
			{
				bool hasWeekCountChanged = _weeks == null ? newWeeks.Length != 0 : newWeeks.Length != _weeks.Length;
				DateTime? lastFirstDate = _weeks == null || _weeks.Length == 0 ? null : (DateTime?)_weeks[0].Start;
				DateTime? newFirstDate = newWeeks == null || newWeeks.Length == 0 ? null : (DateTime?)newWeeks[0].Start;

				_weeks = newWeeks;

				if (daysOfWeek != _daysOfWeek || firstDayOfWeek != _firstDayOfWeek)
					_daysOfWeekList = null;

				_daysOfWeek = daysOfWeek;
				_firstDayOfWeek = firstDayOfWeek;
				_isVerified = true;
				visibleDates.Reinitialize(newVisibleDates);

				if (hasWeekCountChanged)
					_control.OnWeekCountChanged();

				if (lastFirstDate != newFirstDate)
					_control.OnWeekFirstDateChanged();
			}
			#endregion // OnInitialize 

			#region ResetRange
			private void ResetRange(DateTime start, DateTime end)
			{
				DayOfWeek? firstDayOfWeek;
				DayOfWeekFlags daysOfWeek;
				DateRange[] newWeeks;
				List<DateTime> visibleDates = CalculateWeeks(start, end, out firstDayOfWeek, out daysOfWeek, out newWeeks);

				this.OnInitialize(_control.VisibleDates, firstDayOfWeek, newWeeks, daysOfWeek, visibleDates);
			}
			#endregion // ResetRange

			#endregion // Private Methods

			#endregion // Methods
		} 
		#endregion // WeekModeHelper class

		#region ICommandTarget Members

		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		#endregion //ICommandTarget Members

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

		#region ExtraActivityCollection class
		private class ExtraActivityCollection : ObservableCollectionExtended<ActivityBase>
		{
			#region Member Variables

			private Dictionary<ActivityBase, OperationResult> _newActivities;

			#endregion // Member Variables

			#region Constructor
			internal ExtraActivityCollection()
				: base(false, true)
			{
				_newActivities = new Dictionary<ActivityBase, OperationResult>();
				this.PropChangeListeners.Add(new PropertyChangeListener<ExtraActivityCollection>(this, OnPropChanged), false);
			}
			#endregion // Constructor

			#region Base class overrides

			#region OnItemAdded
			protected override void OnItemAdded( ActivityBase itemAdded )
			{
				base.OnItemAdded(itemAdded);

				Debug.Assert(_newActivities.ContainsKey(itemAdded) == false, "The activity is already in the list?");

				if ( itemAdded.IsAddNew )
				{
					OperationResult result = itemAdded.PendingOperation;

					if (null != result)
						ScheduleUtilities.AddListener(result, this.PropChangeListeners, true);

					_newActivities[itemAdded] = result;
				}
			}
			#endregion // OnItemAdded

			#region OnItemRemoved
			protected override void OnItemRemoved( ActivityBase itemRemoved )
			{
				base.OnItemRemoved(itemRemoved);

				OperationResult result;
				if ( _newActivities.TryGetValue(itemRemoved, out result) )
				{
					_newActivities.Remove(itemRemoved);

					if (result != null)
						ScheduleUtilities.RemoveListener(result, this.PropChangeListeners);
				}
			}
			#endregion // OnItemRemoved

			#region NotifyItemsChanged
			protected override bool NotifyItemsChanged
			{
				get
				{
					return true;
				}
			}
			#endregion // NotifyItemsChanged

			#endregion // Base class overrides

			#region Methods

			#region OnPropChanged
			private static void OnPropChanged( ExtraActivityCollection instance, object sender, string propertyName, object extraInfo )
			{
				instance.OnPropChanged(sender, propertyName, extraInfo);
			}

			private void OnPropChanged( object sender, string propertyName, object extraInfo )
			{
				if ( sender == this )
				{
					var collectionArgs = extraInfo as NotifyCollectionChangedEventArgs;

					Debug.Assert(collectionArgs == null || collectionArgs.Action != NotifyCollectionChangedAction.Reset, "Expecting add/remove only");
				}
				else if ( sender is ActivityBase )
				{
					var activity = sender as ActivityBase;

					if ( string.IsNullOrEmpty(propertyName) || propertyName == "IsAddNew" || propertyName == "PendingOperation" )
					{
						// if the addnew operation ended...
						if ( activity.IsAddNew == false )
						{
							this.Remove(activity);
						}
						else
						{
							OperationResult previousResult;

							if ( _newActivities.TryGetValue(activity, out previousResult) )
							{
								OperationResult currentResult = activity.PendingOperation;

								if ( previousResult != currentResult )
								{
									if ( currentResult == null )
									{
										this.Remove(activity);
									}
									else
									{
										if ( previousResult != null )
											ScheduleUtilities.RemoveListener(previousResult, this.PropChangeListeners);

										_newActivities[activity] = currentResult;
										ScheduleUtilities.AddListener(currentResult, this.PropChangeListeners, true);
									}
								}
							}
						}
					}
				}
				else if ( sender is OperationResult )
				{
					
				}
			}
			#endregion // OnPropChanged

			#endregion // Methods
		}
		#endregion // ExtraActivityCollection class

		// AS 11/16/10 NA 11.1 - CalendarGroup Sizing/Scrolling
		#region CalendarGroupResizerHost class
		private class CalendarGroupResizerHost : IResizerBarHost
			, IScrollInfoProvider
		{
			#region Member Variables

			private ScheduleControlBase _control;

			#endregion // Member Variables

			#region Constructor
			internal CalendarGroupResizerHost( ScheduleControlBase control )
			{
				_control = control;
			}
			#endregion // Constructor

			#region IResizerBarHost Members

			public Orientation ResizerBarOrientation
			{
				get { return _control.CalendarGroupOrientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal; }
			}

			public bool CanResize()
			{
				return _control._groupsPanel != null && _control.AllowCalendarGroupResizing;
			}

			public void SetExtent( double extent )
			{
				_control.PreferredCalendarGroupExtent = extent;
			}

			public ResizeInfo GetResizeInfo()
			{
				if ( ((IResizerBarHost)this).CanResize() )
				{
					double actualExtent = double.MaxValue;
					bool isHorizontalItems = _control.CalendarGroupOrientation == Orientation.Horizontal;

					foreach ( var child in _control._groupsPanel.Children )
					{
						var groupElement = child as CalendarGroupPresenterBase;

						if ( null != groupElement && groupElement.CalendarGroup != null )
						{
							// i'm using the minimum here because there are cases where the groups will 
							// be a pixel off in size (e.g. in SL when layout rounding the initial n - 1 
							// groups will have an extra pixel if there is extra space after sizing 
							// with the rounded down column extent)
							actualExtent = Math.Min(isHorizontalItems ? groupElement.ActualWidth : groupElement.ActualHeight, actualExtent);
						}
					}

					// AS 5/4/11 TFS74447
					// This isn't directly related to this issue but I noticed it while debugging it. Essentially 
					// the panel containing the items being resized may adjust the minimum to be the value that 
					// would fully fill the element. Well in the case where the control is measured with an 
					// infinite extent the panels for the group elements should not make that adjustment.
					//
					bool canIncreaseMinimum = 
						(isHorizontalItems && !double.IsInfinity(_control._lastAvailableSize.Width)) || 
						(!isHorizontalItems && !double.IsInfinity(_control._lastAvailableSize.Height));

					double min = Math.Min(actualExtent, _control.MinCalendarGroupExtentResolved);
					return new ResizeInfo(_control, actualExtent, _control.PreferredCalendarGroupExtent, min, canIncreaseMinimum: canIncreaseMinimum );
				}

				return null;
			}

			#endregion //IResizerBarHost Members

			#region IScrollInfoProvider Members
			ScrollInfo IScrollInfoProvider.ScrollInfo
			{
				get { return _control.CalendarGroupMergedScrollInfo; }
			} 
			#endregion //IScrollInfoProvider Members
		} 
		#endregion // CalendarGroupResizerHost class
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