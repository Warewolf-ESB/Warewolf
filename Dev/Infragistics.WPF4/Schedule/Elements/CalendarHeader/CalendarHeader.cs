using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.AutomationPeers;
using System.Windows.Automation.Peers;
using Infragistics.Collections;
using System.Diagnostics;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules.Primitives
{
	/// <summary>
	/// Base class that represents a <see cref="ResourceCalendar"/> from the <see cref="CalendarGroupBase.VisibleCalendars"/> of a <see cref="CalendarGroupPresenterBase.CalendarGroup"/> of a <see cref="CalendarHeaderArea"/>
	/// </summary>
	[TemplateVisualState(Name = VisualStateUtilities.StateNormal, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateMouseOver, GroupName = VisualStateUtilities.GroupCommon)]
	[TemplateVisualState(Name = VisualStateUtilities.StateDisabled, GroupName = VisualStateUtilities.GroupCommon)]

	[TemplateVisualState(Name = VisualStateUtilities.StateSelected, GroupName = VisualStateUtilities.GroupSelection)]
	[TemplateVisualState(Name = VisualStateUtilities.StateUnselected, GroupName = VisualStateUtilities.GroupSelection)]
	
	[TemplateVisualState(Name = VisualStateUtilities.StateHorizontal, GroupName = VisualStateUtilities.GroupOrientation)]
	[TemplateVisualState(Name = VisualStateUtilities.StateVertical, GroupName = VisualStateUtilities.GroupOrientation)]
	public abstract class CalendarHeader : ResourceCalendarElementBase
		, IRecyclableElement
		, ICommandTarget
		, IPropertyChangeListener // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
	{
		#region Private Members
		
		private bool _isMouseOver;

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		private ResourceCalendar _calendar;
		private Resource _resource;
		private bool _isCurrentUser;

		#endregion //Private Members	
    
		#region Constructor
		static CalendarHeader()
		{

			UIElement.IsEnabledProperty.OverrideMetadata(typeof(CalendarHeader), new FrameworkPropertyMetadata(new PropertyChangedCallback(OnIsEnabledPropertyChanged)));
			UIElement.FocusableProperty.OverrideMetadata(typeof(CalendarHeader), new FrameworkPropertyMetadata(KnownBoxes.FalseBox)); // AS 12/16/10 TFS61923

		}

		/// <summary>
		/// Initializes a new <see cref="CalendarHeader"/>
		/// </summary>
		public CalendarHeader()
		{



		}
		#endregion //Constructor

		#region Base class overrides

		#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="CalendarHeader"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="CalendarHeaderAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new CalendarHeaderAutomationPeer(this);
		}
		#endregion // OnCreateAutomationPeer

		#region OnMouseEnter
		/// <summary>
		/// Invoked when the mouse enters the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseEnter(MouseEventArgs e)
		{
			base.OnMouseEnter(e);

			_isMouseOver = true;
			this.UpdateVisualState();
		}
		#endregion // OnMouseEnter

		#region OnMouseLeave
		/// <summary>
		/// Invoked when the mouse leaves the bounds of the element
		/// </summary>
		/// <param name="e">Provides information about the event</param>
		protected override void OnMouseLeave(MouseEventArgs e)
		{
			base.OnMouseLeave(e);

			_isMouseOver = false;
			this.UpdateVisualState();
		}
		#endregion // OnMouseLeave

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Invoked when the left mouse button is pressed down on the element.
		/// </summary>
		/// <param name="e">Provides information about the mouse operation</param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			// JJD 09/14/10 - TFS36353
			// Set focus on the control which we exit an in-plcae editing operation
			ScheduleControlBase control = ScheduleUtilities.GetControl(this);
			if (control != null)
				control.Focus();

			if (this.SelectCalendar())
				e.Handled = true;

			base.OnMouseLeftButtonDown(e);
		}
		#endregion // OnMouseLeftButtonDown

		#endregion // Base class overrides

		#region Properties

		#region Public Properties

		#region Calendar

		/// <summary>
		/// Identifies the <see cref="Calendar"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CalendarProperty = DependencyProperty.Register("Calendar",
			typeof(ResourceCalendar), typeof(CalendarHeader),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCalendarChanged))
			);

		private static void OnCalendarChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader item = (CalendarHeader)d;
			item.VerifyHeaderSource(); // AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		}

		/// <summary>
		/// Returns or sets the calendar that the element represents
		/// </summary>
		/// <seealso cref="CalendarProperty"/>
		public ResourceCalendar Calendar
		{
			get
			{
				return (ResourceCalendar)this.GetValue(CalendarHeader.CalendarProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.CalendarProperty, value);
			}
		}

		#endregion //Calendar

		#region CanClose

		private static readonly DependencyPropertyKey CanClosePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CanClose",
			typeof(bool), typeof(CalendarHeader),
			KnownBoxes.TrueBox,
			null
			);

		/// <summary>
		/// Identifies the read-only <see cref="CanClose"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CanCloseProperty = CanClosePropertyKey.DependencyProperty;

		private static void OnCanCloseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader instance = (CalendarHeader)d;
			
			//instance.InvalidateMeasure();

		}

		/// <summary>
		/// Returns true if the user can close theis calendar (read-only)
		/// </summary>
		/// <seealso cref="CanCloseProperty"/>
		public bool CanClose
		{
			get
			{
				return (bool)this.GetValue(CalendarHeader.CanCloseProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.CanClosePropertyKey, value);
			}
		}

		#endregion //CanClose

		#region CloseButtonVisibility

		private static readonly DependencyPropertyKey CloseButtonVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CloseButtonVisibility",
			typeof(Visibility), typeof(CalendarHeader), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="CloseButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty CloseButtonVisibilityProperty = CloseButtonVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the close button (read-only)
		/// </summary>
		/// <seealso cref="CloseButtonVisibilityProperty"/>
		/// <seealso cref="ScheduleSettings.AllowCalendarClosing"/>
		public Visibility CloseButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarHeader.CloseButtonVisibilityProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.CloseButtonVisibilityPropertyKey, value);
			}
		}

		#endregion //CloseButtonVisibility

		#region ComputedForeground

		private static readonly DependencyPropertyKey ComputedForegroundPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ComputedForeground",
			typeof(Brush), typeof(CalendarHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="ComputedForeground"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ComputedForegroundProperty = ComputedForegroundPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the brush to use for the Foreground based on the element's state and the associated <see cref="XamScheduleDataManager"/>'s <see cref="XamScheduleDataManager.ColorScheme"/>
		/// </summary>
		/// <seealso cref="ComputedForegroundProperty"/>
		public Brush ComputedForeground
		{
			get
			{
				return (Brush)this.GetValue(CalendarHeader.ComputedForegroundProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.ComputedForegroundPropertyKey, value);
			}
		}

		#endregion //ComputedForeground

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region Header

		private static readonly DependencyPropertyKey HeaderPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("Header",
			typeof(string), typeof(CalendarHeader), null, null);

		/// <summary>
		/// Identifies the read-only <see cref="Header"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HeaderProperty = HeaderPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns or sets the header to be displayed by the element
		/// </summary>
		/// <seealso cref="HeaderProperty"/>
		public string Header
		{
			get
			{
				return (string)this.GetValue(CalendarHeader.HeaderProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.HeaderPropertyKey, value);
			}
		}

		#endregion //Header

		#region IsActive

		/// <summary>
		/// Identifies the <see cref="IsActive"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register("IsActive",
			typeof(bool), typeof(CalendarHeader),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsActiveChanged))
			);

		private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader item = (CalendarHeader)d;
			item.UpdateVisualState();
		}

		/// <summary>
		/// Returns a boolean indicating if the item is currently the active calendar within the containing control.
		/// </summary>
		/// <seealso cref="IsActiveProperty"/>
		public bool IsActive
		{
			get
			{
				return (bool)this.GetValue(CalendarHeader.IsActiveProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.IsActiveProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsActive

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region IsCurrentUser

		/// <summary>
		/// Identifies the <see cref="IsCurrentUser"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsCurrentUserProperty = DependencyProperty.Register("IsCurrentUser",
			typeof(bool), typeof(CalendarHeader),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsCurrentUserChanged))
			);

		private static void OnIsCurrentUserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader item = (CalendarHeader)d;
			item._isCurrentUser = (bool)e.NewValue;
			item.InitializeHeader();
		}

		/// <summary>
		/// Returns a boolean indicating if the item is a calendar associated with the <see cref="XamScheduleDataManager.CurrentUser"/> of the associated <see cref="ScheduleControlBase.DataManager"/>.
		/// </summary>
		/// <seealso cref="IsCurrentUserProperty"/>
		public bool IsCurrentUser
		{
			get
			{
				return _isCurrentUser;
			}
			internal set
			{
				this.SetValue(CalendarHeader.IsCurrentUserProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsCurrentUser

		#region IsInOverlayMode

		private static readonly DependencyPropertyKey IsInOverlayModePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("IsInOverlayMode",
			typeof(bool), typeof(CalendarHeader), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="IsInOverlayMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsInOverlayModeProperty = IsInOverlayModePropertyKey.DependencyProperty;

		/// <summary>
		/// Returns true if this calendare header is in overlay mode.
		/// </summary>
		/// <seealso cref="IsInOverlayModeProperty"/>
		public bool IsInOverlayMode
		{
			get
			{
				return (bool)this.GetValue(CalendarHeader.IsInOverlayModeProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.IsInOverlayModePropertyKey, value);
			}
		}

		#endregion //IsInOverlayMode

		#region IsSelected

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
			typeof(bool), typeof(CalendarHeader),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSelectedChanged))
			);

		private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader item = (CalendarHeader)d;

			// bring the today item to the front so its border shows
			if (true.Equals(e.NewValue))
				Canvas.SetZIndex(item, 1);
			else
				item.ClearValue(Canvas.ZIndexProperty);

			item.UpdateVisualState();

			CalendarHeaderAutomationPeer peer = FrameworkElementAutomationPeer.FromElement(item) as CalendarHeaderAutomationPeer;

			if (null != peer)
				peer.RaiseAutomationIsSelectedChanged((bool)e.NewValue);
		}

		/// <summary>
		/// Returns a boolean indicating if the item is currently selected.
		/// </summary>
		/// <seealso cref="IsSelectedProperty"/>
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(CalendarHeader.IsSelectedProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.IsSelectedProperty, KnownBoxes.FromValue(value));
			}
		}

		#endregion //IsSelected

		#region Orientation

		/// <summary>
		/// Identifies the <see cref="Orientation"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OrientationProperty = DependencyPropertyUtilities.Register("Orientation",
			typeof(Orientation), typeof(CalendarHeader),
			DependencyPropertyUtilities.CreateMetadata(KnownBoxes.OrientationVerticalBox, new PropertyChangedCallback(OnOrientationChanged))
			);

		private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader element = d as CalendarHeader;
			element.InvalidateMeasure();
			element.UpdateVisualState();
		}

		/// <summary>
		/// Returns the current orientation in which the element is arranged.
		/// </summary>
		/// <seealso cref="OrientationProperty"/>
		public Orientation Orientation
		{
			get
			{
				return (Orientation)this.GetValue(CalendarHeader.OrientationProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.OrientationProperty, value);
			}
		}

		#endregion //Orientation

		#region OverlayButtonVisibility

		private static readonly DependencyPropertyKey OverlayButtonVisibilityPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("OverlayButtonVisibility",
			typeof(Visibility), typeof(CalendarHeader), KnownBoxes.VisibilityVisibleBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="OverlayButtonVisibility"/> dependency property
		/// </summary>
		public static readonly DependencyProperty OverlayButtonVisibilityProperty = OverlayButtonVisibilityPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns the visibility of the overlay button which toggles whether the calendar is displayed in 'Overlay' or 'Side-By-Side' mode (read-only)
		/// </summary>
		/// <seealso cref="OverlayButtonVisibilityProperty"/>
		public Visibility OverlayButtonVisibility
		{
			get
			{
				return (Visibility)this.GetValue(CalendarHeader.OverlayButtonVisibilityProperty);
			}
			internal set
			{
				this.SetValue(CalendarHeader.OverlayButtonVisibilityPropertyKey, value);
			}
		}

		#endregion //OverlayButtonVisibility

		#endregion //Public Properties	

		#region Internal Properties

		#endregion //Internal Properties	
		
		#endregion // Properties

		#region Methods

		#region Internal Methods

		#region ChangeVisualState
		internal override void ChangeVisualState(bool useTransitions)
		{
			if (this.IsEnabled)
			{
				if (_isMouseOver)
					VisualStateUtilities.GoToState(this, useTransitions, VisualStateUtilities.StateMouseOver, VisualStateUtilities.StateNormal);
				else
					VisualStateManager.GoToState(this, VisualStateUtilities.StateNormal, useTransitions);
			}
			else
				VisualStateManager.GoToState(this, VisualStateUtilities.StateDisabled, useTransitions);

			if (this.IsSelected)
				this.GoToState(VisualStateUtilities.StateSelected, useTransitions);
			else
				this.GoToState(VisualStateUtilities.StateUnselected, useTransitions);

			if (this.Orientation == Orientation.Horizontal)
				this.GoToState(VisualStateUtilities.StateHorizontal, useTransitions);
			else
				this.GoToState(VisualStateUtilities.StateVertical, useTransitions);

			base.ChangeVisualState(useTransitions);
		}
		#endregion //ChangeVisualState

		#region Close
	
			internal void Close()
		{
			ResourceCalendar calendar = this.DataContext as ResourceCalendar;

			if ( calendar != null )
				calendar.IsVisible = false;
		}

		#endregion //Close	
	
		#region SelectCalendar
		internal bool SelectCalendar()
		{
			var header = PresentationUtilities.GetVisualAncestor<CalendarHeaderArea>(this, null);

			if (null != header)
			{
				header.SelectCalendar(this);
				return true;
			}

			return false;
		} 
		#endregion // SelectCalendar

		#region SetProviderBrushes

		internal override void SetProviderBrushes()
		{
			var ctrl = ScheduleUtilities.GetControl(this);
			CalendarBrushProvider brushProvider = ScheduleUtilities.GetCalendarGroupBrushProvider(this, ctrl);

			if (brushProvider == null)
				return;

			this.SetValue(CalendarBrushProvider.BrushProviderProperty, brushProvider);

			CalendarBrushId brushId;

			if (ctrl != null && ctrl.CalendarGroupOrientation == System.Windows.Controls.Orientation.Vertical)
				brushId = CalendarBrushId.NonWorkingHourTimeslotBackground;
			else
				brushId = CalendarBrushId.CalendarHeaderBackground;

			Brush br = brushProvider.GetBrush(brushId);

			if (br != null)
				this.ComputedBackground = br;

			br = brushProvider.GetBrush(CalendarBrushId.CalendarBorder);
			if (br != null)
				this.ComputedBorderBrush = br;

			br = brushProvider.GetBrush(CalendarBrushId.CalendarHeaderForeground);
			if (br != null)
				this.ComputedForeground = br;

		}

		#endregion //SetProviderBrushes

		#region ToggleOverlayMode

		internal void ToggleOverlayMode()
		{
			this.VerifyState();

			if (this.OverlayButtonVisibility == System.Windows.Visibility.Collapsed)
				return;

			ResourceCalendar calendar = this.Calendar;

			if (calendar == null)
				return;

			ScheduleControlBase control = ScheduleUtilities.GetControl(this);

			if (control == null || control.CalendarDisplayModeResolved != CalendarDisplayMode.Overlay)
				return;

			CalendarGroupCollection groups = control.CalendarGroupsResolvedSource;

			Debug.Assert(groups != null, "CalendarGroupsResolved should not be null");
			Debug.Assert(groups.Count > 0, "There should be at least one group");

			if (groups == null || groups.Count == 0)
				return;
			
			CalendarHeaderArea hdrArea = PresentationUtilities.GetVisualAncestor<CalendarHeaderArea>(this, null);

			Debug.Assert(hdrArea!= null, "Should be inside a CalendarHeaderArea");

			if (hdrArea == null)
				return;

			CalendarGroup oldGroup = hdrArea.CalendarGroup as CalendarGroup;

			Debug.Assert(oldGroup != null, "hdrArea.CalendarGroup should be a CalendarGroup");

			if (oldGroup == null)
				return;

			Debug.Assert(groups.Contains(oldGroup), "old group not found");

			if (!groups.Contains(oldGroup))
				return;

			CalendarGroup firstGroup = null;

			foreach (CalendarGroup group in groups)
			{
				if (group.VisibleCalendars.Count > 0)
				{
					firstGroup = group;
					break;
				}
			}

			if (firstGroup == null)
				return;

			// if the calenadr is in the first group then we need to pull it out and
			// create a new group for it
			if (firstGroup == oldGroup)
			{
				Debug.Assert(firstGroup.VisibleCalendars.Count > 1, "There should be more than oe calendar in the 1st group at this point");

				// can't remove the last calendar from the 1st group
				if (firstGroup.VisibleCalendars.Count < 2)
					return;

				firstGroup.Calendars.Remove(calendar);

				// pass in true to the internal ctor so we knoe this group is auto-generated
				CalendarGroup newGroup = new CalendarGroup(true);

				newGroup.Calendars.Add(calendar);

				// insert the new group as the 2nd group in the collection
				groups.Insert(1, newGroup);

				// make it the selected calendar in the new group and make sure its activated
				control.SelectCalendar(calendar, true, newGroup);
			}
			else
			{
				// remove the calendar for the old group and insert it as the last calenadr in the 1st hgroup
				oldGroup.Calendars.Remove(calendar);

				// if the group was auto-generated and it no longer has any calendars the remove it
 				// from the groups collection
				if (oldGroup.IsAutoGenerated && oldGroup.Calendars.Count == 0)
					groups.Remove(oldGroup);

				firstGroup.Calendars.Add(calendar);

				// make it the selected calendar in the group
				control.SelectCalendar(calendar, true, firstGroup);
			}


		}

		#endregion //ToggleOverlayMode	
    
		#region VerifyState
		internal void VerifyState()
		{
			bool canClose = false;
			bool showCloseBtn = false;
			bool canToggleOverlayMode = false;
			bool isInOverlayMode = false;

			ScheduleControlBase control = ScheduleUtilities.GetControl(this);

			if (control != null)
			{
				if (control.VisibleCalendarCount > 1)
				{
					Debug.Assert(null != control.DataManagerResolved);

					
					// Get the allow close and show close btn settings from the control
					canClose = control.AllowCalendarClosingResolved;
					showCloseBtn = control.ShowCalendarCloseButtonResolved;

					if (control.ShowCalendarOverlayButtonResolved)
					{
						ReadOnlyNotifyCollection<CalendarGroupBase> groups = control.CalendarGroupsResolved;

						// AS 10/21/10 TFS57202
						//CalendarGroupBase firstGroup = groups[0];
						CalendarGroupBase firstGroup = groups.FirstOrDefault(ScheduleUtilities.HasVisibleCalendars);

						if (ScheduleUtilities.HasVisibleCalendars(firstGroup))
						{
							ResourceCalendar calendar = this.Calendar;
							if (firstGroup.VisibleCalendars.Count > 1)
								canToggleOverlayMode = true;
							else
								canToggleOverlayMode = calendar != firstGroup.VisibleCalendars[0];

							if (canToggleOverlayMode)
								isInOverlayMode = firstGroup.VisibleCalendars.Contains(calendar);
						}
					}
				}
			}

			if (canClose)
				this.ClearValue(CanClosePropertyKey);
			else
				this.SetValue(CanClosePropertyKey, KnownBoxes.FalseBox);

			if (showCloseBtn)
				this.ClearValue(CloseButtonVisibilityPropertyKey);
			else
				this.SetValue(CloseButtonVisibilityPropertyKey, KnownBoxes.VisibilityCollapsedBox);

			if (canToggleOverlayMode)
				this.ClearValue(OverlayButtonVisibilityPropertyKey);
			else
				this.SetValue(OverlayButtonVisibilityPropertyKey, KnownBoxes.VisibilityCollapsedBox);

			if (isInOverlayMode)
				this.SetValue(IsInOverlayModePropertyKey, KnownBoxes.TrueBox);
			else
				this.ClearValue(IsInOverlayModePropertyKey);

		}
		#endregion // VerifyState

		#endregion //Internal Methods	

		#region Private Methods

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region InitializeHeader
		private void InitializeHeader()
		{
			string resourceName = _resource != null ? _resource.Name : null;
			string calendarName = _calendar != null ? _calendar.Name : null;
			bool isDefault = _resource != null && _resource.PrimaryCalendar == _calendar;
			bool isCurrentUser = this.IsCurrentUser;

			string resourceString;

			if (isCurrentUser)
				resourceString = isDefault ? "CalendarHeaderCurrentUserDefaultCalendar" : "CalendarHeaderCurrentUserNonDefaultCalendar";
			else
				resourceString = isDefault ? "CalendarHeaderOtherUserDefaultCalendar" : "CalendarHeaderOtherUserNonDefaultCalendar";

			this.Header = ScheduleUtilities.GetString(resourceString, resourceName, calendarName);
		} 
		#endregion //InitializeHeader

		#region OnIsEnabledChanged


		private static void OnIsEnabledPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			CalendarHeader instance = target as CalendarHeader;

			instance.UpdateVisualState();
		}







		#endregion //OnIsEnabledChanged

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region VerifyHeaderSource
		private bool VerifyHeaderSource()
		{
			ResourceCalendar calendar = this.Calendar;
			Resource resource = calendar != null ? calendar.OwningResource : null;
			bool hasChanged = false;

			if (_calendar != calendar)
			{
				hasChanged = true;

				if (_calendar != null)
					ScheduleUtilities.RemoveListener(_calendar, this);

				_calendar = calendar;

				if (_calendar != null)
					ScheduleUtilities.AddListener(_calendar, this, true);
			}

			if (_resource != resource)
			{
				hasChanged = true;

				if (_resource != null)
					ScheduleUtilities.RemoveListener(_resource, this);

				_resource = resource;

				if (_resource != null)
					ScheduleUtilities.AddListener(_resource, this, true);
			}

			if (hasChanged)
				this.InitializeHeader();

			return hasChanged;
		}
		#endregion //VerifyHeaderSource

		#endregion //Private Methods	
    
		#endregion //Methods	
	
		#region IRecyclableElement Members

		bool IRecyclableElement.DelayRecycling
		{
			get;
			set;
		}

		private Panel _ownerPanel;

		Panel IRecyclableElement.OwnerPanel
		{
			get
			{
				return _ownerPanel;
			}
			set
			{
				_ownerPanel = value;

				
				ScheduleItemsPanel itemsPanel = value as ScheduleItemsPanel;

				if (itemsPanel != null)
					this.Orientation = itemsPanel.Orientation;
			}
		}

		#endregion

		#region ICommandTarget Members

		object  ICommandTarget.GetParameter(CommandSource source)
		{
			if (source.Command is CalendarHeaderCommandBase)
				return this;

			return null;
		}

		bool  ICommandTarget.SupportsCommand(System.Windows.Input.ICommand command)
		{
			return command is CalendarHeaderCommandBase;
		}

		#endregion

		// AS 2/28/11 NA 2011.1 - Enhanced CalendarHeader
		#region ITypedPropertyChangeListener Members
		void ITypedPropertyChangeListener<object, string>.OnPropertyValueChanged(object dataItem, string property, object extraInfo)
		{
			bool isEmptyChange = string.IsNullOrEmpty(property);

			if (dataItem == _resource)
			{
				if (string.IsNullOrEmpty(property))
				{
					if (!this.VerifyHeaderSource())
						this.InitializeHeader();
				}
				else if (property == "PrimaryCalendar")
					this.VerifyHeaderSource();
				else if (property == "Name")
					this.InitializeHeader();
			}
			else if (dataItem == _calendar)
			{
				if (string.IsNullOrEmpty(property) || property == "Name")
				{
					this.InitializeHeader();
				}
			}

		} 
		#endregion //ITypedPropertyChangeListener Members
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