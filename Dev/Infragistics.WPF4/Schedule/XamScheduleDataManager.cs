using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.ComponentModel;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;
using System.Threading;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Globalization;
using System.Linq;





using Infragistics.Windows.Controls;


using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// XamScheduleDataManager class manages schedule data.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>XamScheduleDataManager</b> class manages schedule data. Various schedule controls, like the <see cref="XamDayView"/>,
	/// are provided with an instance of <i>XamScheduleDataManager</i> from which they retrieve the scheduling information
	/// displayed by them.
	/// </para>
	/// <para class="body">
	/// The <see cref="ScheduleControlBase.DataManager"/> property is used to associate a schedule control with an
	/// instance of <i>XamScheduleDataManager</i>.
	/// </para>
	/// </remarks>
	/// <seealso cref="ScheduleControlBase.DataManager"/>
	/// <seealso cref="ScheduleControlBase"/>
	/// <seealso cref="ScheduleDataConnectorBase"/>
	[TemplatePart(Name = PartRootPanel, Type = typeof(Panel))]
	public class XamScheduleDataManager : Control
	{
		#region Member Vars

		private const string PartRootPanel = "RootPanel";

		private ScheduleDataConnectorBase _cachedDataConnector;
		private ScheduleSettings _cachedSettings;
		private DateInfoProvider _dateInfoProviderResolved;
		
		private WeakList<IScheduleControl> _controls;

		private PropertyChangeListenerList _propChangeListeners;

		private DataManagerReminderSubscriber _reminderSubscriber;

		private CalendarColorScheme _colorSchemeResolved;

		private DateInfoProvider _cachedDateInfoProvider;

		private CalendarGroupCollection _calendarGroups;

		private object _currentDayTimerToken;
		private Panel _rootPanel;

		private ScheduleDialogFactoryBase _cachedDialogFactory;

		private ObservableCollectionExtended<OperationResult> _pendingOperationsSource;
		private ReadOnlyNotifyCollection<OperationResult> _pendingOperations;

		// JM 01-06-12 TFS95261 - Not sure why we made this static originally but there  doesn't seem to be any
		// benefit to doing so and there is a downside in that we could end up affecting dialogs from other DM's.
		//[ThreadStatic]
		//private static WeakDictionary<object, FrameworkElement> _activeDialogMap;
		private WeakDictionary<object, FrameworkElement> _activeDialogMap;

		[ThreadStatic]
		private static bool _wasTimeZoneChooserDisplayed;

		[ThreadStatic]
		private static bool _isTimeZoneChooserDisplaying;
		private bool _verifyInitialStatePending;
		
		private DispatcherOperation _timeZoneChooserPending;
		DataErrorInfo _pendingTimeZoneError;

		private object _verifyToken;

		private ActivityDragManager _dragManager;

		private WeakReference _reminderDialog;

		// JM 01-06-12 TFS95261
		private WeakList<SubDialogCloseHelper> _activeSubDialogs;
		private bool _isClosingAllDialogs;

		#endregion // Member Vars

		#region Constructor

		static XamScheduleDataManager()
		{

			XamScheduleDataManager.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamScheduleDataManager), new FrameworkPropertyMetadata(typeof(XamScheduleDataManager)));

		}

		/// <summary>
		/// Initializes a new instance of <see cref="XamScheduleDataManager"/>.
		/// </summary>
		public XamScheduleDataManager( ) : base( )
		{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			
			_controls = new WeakList<IScheduleControl>();
			_dragManager = new ActivityDragManager(this);

			_reminderSubscriber = new DataManagerReminderSubscriber( this );
			_activeReminders = new ReadOnlyNotifyCollection<ReminderInfo>( _reminderSubscriber._activeReminders );

			this.ColorSchemeResolved = new Office2010ColorScheme();

			this.UpdateLogicalDayTimer();

			// JM 01-04-12 TFS95261
			this.Unloaded += new RoutedEventHandler(OnUnloaded);
		}
		#endregion // Constructor

		#region Base class overrides

		#region OnApplyTemplate
		/// <summary>
		/// Invoked when the template for the control has been applied.
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			#region RootPanel

			var connector = this.DataConnector;

			PresentationUtilities.ReparentElement(_rootPanel, connector, false);

			_rootPanel = this.GetTemplateChild(PartRootPanel) as Panel;

			PresentationUtilities.ReparentElement(_rootPanel, connector, true);

			#endregion // RootPanel

			this.VerifyTimeZones();
		}
		#endregion //OnApplyTemplate

		#endregion //Base class overrides

		#region Events

		#region ActivitiesDragging

		/// <summary>
		/// Used to invoke the <see cref="ActivitiesDragging"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivitiesDragging"/>
		internal protected virtual void OnActivitiesDragging(ActivitiesDraggingEventArgs args)
		{
			EventHandler<ActivitiesDraggingEventArgs> handler = this.ActivitiesDragging;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised when an activity is about to be dragged
		/// </summary>
		/// <remarks>
		/// <para class="body">The <b>ActivitiesDragging</b> event is raised when an activity is about to be dragged</para>
		/// </remarks>
		/// <seealso cref="ActivitiesDraggingEventArgs"/>
		/// <seealso cref="OnActivitiesDragging"/>
		public event EventHandler<ActivitiesDraggingEventArgs> ActivitiesDragging;

		#endregion // ActivitiesDragging

		#region ActivitiesDragged

		/// <summary>
		/// Used to invoke the <see cref="ActivitiesDragged"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivitiesDragged"/>
		internal protected virtual void OnActivitiesDragged(ActivitiesDraggedEventArgs args)
		{
			EventHandler<ActivitiesDraggedEventArgs> handler = this.ActivitiesDragged;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised after a drag operation involving one or more activities has been completed.
		/// </summary>
		/// <remarks>
		/// <para class="body">The <b>ActivitiesDragged</b> event is raised after a drag operation involving one or more activities has been completed.</para>
		/// </remarks>
		/// <seealso cref="ActivitiesDraggedEventArgs"/>
		/// <seealso cref="OnActivitiesDragged"/>
		public event EventHandler<ActivitiesDraggedEventArgs> ActivitiesDragged;

		#endregion // ActivitiesDragged

		#region ActivityDialogDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ActivityDialogDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityDialogDisplaying"/>
		protected virtual void OnActivityDialogDisplaying( ActivityDialogDisplayingEventArgs args )
		{
			EventHandler<ActivityDialogDisplayingEventArgs> handler = this.ActivityDialogDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised before an activity dialog is displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityDialogDisplaying</b> event is raised before an activity dialog is displayed.
		/// The dialog can be displayed for the purposes of viewing or modifying an existing activity or
		/// adding a new activity.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityDialogDisplayingEventArgs"/>
		/// <seealso cref="OnActivityDialogDisplaying"/>
		/// <seealso cref="ActivityAdding"/>
		/// <seealso cref="ActivityAdded"/>
		/// <seealso cref="ActivityChanging"/>
		/// <seealso cref="ActivityChanged"/>
		public event EventHandler<ActivityDialogDisplayingEventArgs> ActivityDialogDisplaying;

		#endregion // ActivityDialogDisplaying

		#region ActivityAdding

		/// <summary>
		/// Used to invoke the <see cref="ActivityAdding"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityAdding"/>
		protected virtual void OnActivityAdding( ActivityAddingEventArgs args )
		{
			EventHandler<ActivityAddingEventArgs> handler = this.ActivityAdding;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityAdding( ActivityAddingEventArgs args )
		{
			this.OnActivityAdding( args );
		}

		/// <summary>
		/// Raised before an activity is added.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityAdding</b> event is raised before a new activity is added by the user.
		/// The event is raised after the new activity dialog is okayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityAddingEventArgs"/>
		/// <seealso cref="OnActivityAdding"/>
		/// <seealso cref="ActivityDialogDisplaying"/>
		/// <seealso cref="ActivityAdded"/>
		public event EventHandler<ActivityAddingEventArgs> ActivityAdding;

		#endregion // ActivityAdding

		#region ActivityAdded

		/// <summary>
		/// Used to invoke the <see cref="ActivityAdded"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityAdded"/>
		protected virtual void OnActivityAdded( ActivityAddedEventArgs args )
		{
			EventHandler<ActivityAddedEventArgs> handler = this.ActivityAdded;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityAdded( ActivityAddedEventArgs args )
		{
			this.OnActivityAdded( args );
		}

		/// <summary>
		/// Raised after an activity is added.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityAdded</b> event is raised after a new activity is successfully commited.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityAddedEventArgs"/>
		/// <seealso cref="OnActivityAdded"/>
		/// <seealso cref="ActivityDialogDisplaying"/>
		/// <seealso cref="ActivityAdding"/>
		public event EventHandler<ActivityAddedEventArgs> ActivityAdded;

		#endregion // ActivityAdded ActivityAdded

		#region ActivityChanging

		/// <summary>
		/// Used to invoke the <see cref="ActivityChanging"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityChanging"/>
		protected virtual void OnActivityChanging( ActivityChangingEventArgs args )
		{
			EventHandler<ActivityChangingEventArgs> handler = this.ActivityChanging;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityChanging( ActivityChangingEventArgs args )
		{
			this.OnActivityChanging( args );
		}

		/// <summary>
		/// Raised right before user changes to an activity are being committed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityChanging</b> event is raised right before user changes to an 
		/// activity are being committed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityChangingEventArgs"/>
		/// <seealso cref="OnActivityChanging"/>
		/// <seealso cref="ActivityChanged"/>
		public event EventHandler<ActivityChangingEventArgs> ActivityChanging;

		#endregion // ActivityChanging

		#region ActivityChanged

		/// <summary>
		/// Used to invoke the <see cref="ActivityChanged"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityChanged"/>
		protected virtual void OnActivityChanged( ActivityChangedEventArgs args )
		{
			EventHandler<ActivityChangedEventArgs> handler = this.ActivityChanged;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityChanged( ActivityChangedEventArgs args )
		{
			this.OnActivityChanged( args );
		}

		/// <summary>
		/// Raised after user changes to an activity are committed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityChanged</b> event is raised after user changes to an 
		/// activity are committed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityChangedEventArgs"/>
		/// <seealso cref="OnActivityChanged"/>
		/// <seealso cref="ActivityChanging"/>
		public event EventHandler<ActivityChangedEventArgs> ActivityChanged;

		#endregion // ActivityChanged

		// JM 02-18-11 TFS61928 Added.
		#region ActivityRecurrenceDialogDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ActivityRecurrenceDialogDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityRecurrenceDialogDisplaying"/>
		protected virtual void OnActivityRecurrenceDialogDisplaying(ActivityRecurrenceDialogDisplayingEventArgs args)
		{
			EventHandler<ActivityRecurrenceDialogDisplayingEventArgs> handler = this.ActivityRecurrenceDialogDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised before an activity dialog is displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <see cref="XamScheduleDataManager.ActivityRecurrenceDialogDisplaying"/> event is raised before an <see cref="ActivityRecurrenceDialogCore"/> is displayed.
		/// The dialog can be displayed for the purposes of viewing or modifying an existing activity or
		/// adding a new activity.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityRecurrenceDialogDisplayingEventArgs"/>
		/// <seealso cref="OnActivityRecurrenceDialogDisplaying"/>
		/// <seealso cref="ActivityRecurrenceDialogCore"/>
		public event EventHandler<ActivityRecurrenceDialogDisplayingEventArgs> ActivityRecurrenceDialogDisplaying;

		#endregion // ActivityRecurrenceDialogDisplaying

		#region ActivityRecurrenceChooserDialogDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ActivityDialogDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityRecurrenceChooserDialogDisplaying"/>
		protected virtual void OnActivityRecurrenceChooserDialogDisplaying( ActivityRecurrenceChooserDialogDisplayingEventArgs args )
		{
			EventHandler<ActivityRecurrenceChooserDialogDisplayingEventArgs> handler = this.ActivityRecurrenceChooserDialogDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised before the dialog that allows the end user to choose between dealing with the series or occurrence of a recurring activity is displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityRecurrenceChooserDialogDisplaying</b> event is raised before the <see cref="ActivityRecurrenceChooserDialog"/> is displayed. The dialog allows 
		/// the end user to choose whether to affect the occurrence or the series of a recurring activity. The <see cref="ActivityRecurrenceChooserDialogDisplayingEventArgs.ChooserAction"/> 
		/// may be set to cancel the operation or to explicitly use the series or occurrence without prompting the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityRecurrenceChooserDialogDisplayingEventArgs"/>
		/// <seealso cref="OnActivityRecurrenceChooserDialogDisplaying"/>
		public event EventHandler<ActivityRecurrenceChooserDialogDisplayingEventArgs> ActivityRecurrenceChooserDialogDisplaying;

		#endregion // ActivityRecurrenceChooserDialogDisplaying

		#region ActivityRemoving

		/// <summary>
		/// Used to invoke the <see cref="ActivityRemoving"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityRemoving"/>
		protected virtual void OnActivityRemoving( ActivityRemovingEventArgs args )
		{
			EventHandler<ActivityRemovingEventArgs> handler = this.ActivityRemoving;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityRemoving( ActivityRemovingEventArgs args )
		{
			this.OnActivityRemoving( args );
		}

		/// <summary>
		/// Raised before an activity is removed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityRemoving</b> event is raised before an activity is removed by the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityRemovingEventArgs"/>
		/// <seealso cref="OnActivityRemoving"/>
		/// <seealso cref="ActivityRemoved"/>
		public event EventHandler<ActivityRemovingEventArgs> ActivityRemoving;

		#endregion // ActivityRemoving

		#region ActivityRemoved

		/// <summary>
		/// Used to invoke the <see cref="ActivityRemoved"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityRemoved"/>
		protected virtual void OnActivityRemoved( ActivityRemovedEventArgs args )
		{
			EventHandler<ActivityRemovedEventArgs> handler = this.ActivityRemoved;

			if (null != handler)
				handler(this, args);
		}

		internal void RaiseActivityRemoved( ActivityRemovedEventArgs args )
		{
			this.OnActivityRemoved( args );
		}

		/// <summary>
		/// Raised after an activity is removed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityRemoved</b> event is raised after an activity is removed by the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityRemovedEventArgs"/>
		/// <seealso cref="OnActivityRemoved"/>
		/// <seealso cref="ActivityRemoving"/>
		public event EventHandler<ActivityRemovedEventArgs> ActivityRemoved;

		#endregion // ActivityRemoved

		#region ActivityResizing

		/// <summary>
		/// Used to invoke the <see cref="ActivityResizing"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityResizing"/>
		internal protected virtual void OnActivityResizing(ActivityResizingEventArgs args)
		{
			EventHandler<ActivityResizingEventArgs> handler = this.ActivityResizing;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised before an activity is resized.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityResizing</b> event is raised before an activity is resized by the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityResizingEventArgs"/>
		/// <seealso cref="OnActivityResizing"/>
		/// <seealso cref="ActivityResized"/>
		public event EventHandler<ActivityResizingEventArgs> ActivityResizing;

		#endregion // ActivityResizing

		#region ActivityResized

		/// <summary>
		/// Used to invoke the <see cref="ActivityResized"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityResized"/>
		internal protected virtual void OnActivityResized(ActivityResizedEventArgs args)
		{
			EventHandler<ActivityResizedEventArgs> handler = this.ActivityResized;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised after an activity is Resized.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityResized</b> event is raised after an activity is Resized by the user.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityResizedEventArgs"/>
		/// <seealso cref="OnActivityResized"/>
		/// <seealso cref="ActivityResizing"/>
		public event EventHandler<ActivityResizedEventArgs> ActivityResized;

		#endregion // ActivityResized

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region ActivityTypeChooserDialogDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ActivityTypeChooserDialogDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ActivityTypeChooserDialogDisplaying"/>
		protected virtual void OnActivityTypeChooserDialogDisplaying(ActivityTypeChooserDialogDisplayingEventArgs args)
		{
			EventHandler<ActivityTypeChooserDialogDisplayingEventArgs> handler = this.ActivityTypeChooserDialogDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised before the dialog that allows the end user to choose from a list of available activity types is displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityTypeChooserDialogDisplaying</b> event is raised before the <see cref="ActivityTypeChooserDialog"/> is displayed. The dialog allows 
		/// the end user to choose an activity type from a list of available types. The <see cref="ActivityTypeChooserDialogDisplayingEventArgs.ActivityType"/> 
		/// may be set to explicitly choose the type to create or left to null to allow the end user to see the prompt dialog and choose the activity type. The 
		/// <see cref="ActivityTypeChooserDialogDisplayingEventArgs.Cancel"/> may be set to true to cancel the operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityTypeChooserDialogDisplayingEventArgs"/>
		/// <seealso cref="OnActivityTypeChooserDialogDisplaying"/>
		public event EventHandler<ActivityTypeChooserDialogDisplayingEventArgs> ActivityTypeChooserDialogDisplaying;

		#endregion // ActivityTypeChooserDialogDisplaying

		#region Error

		/// <summary>
		/// Used to invoke the <see cref="Error"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="Error"/>
		protected virtual void OnError( ErrorEventArgs args )
		{
			EventHandler<ErrorEventArgs> handler = this.Error;

			if ( null != handler )
				handler( this, args );
		}

		internal void RaiseError( ErrorEventArgs args )
		{
			this.OnError( args );
		}

		/// <summary>
		/// Raised when an error occurs.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Error</b> event is raised when an error occurs.
		/// </para>
		/// </remarks>
		/// <seealso cref="ErrorEventArgs"/>
		/// <seealso cref="OnError"/>
		public event EventHandler<ErrorEventArgs> Error;

		#endregion // Error

		#region ErrorDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ErrorDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ErrorDisplaying"/>
		internal protected virtual void OnErrorDisplaying( ErrorDisplayingEventArgs args )
		{
			EventHandler<ErrorDisplayingEventArgs> handler = this.ErrorDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised when an error is to be displayed to the end user.
		/// </summary>
		/// <remarks>
		/// <para class="body">This event is raised after an error has occurred and that error is about to be displayed to the end user. The type of 
		/// display is indicated by the <see cref="ErrorDisplayingEventArgs.DisplayType"/>. The <see cref="ErrorDisplayingEventArgs.Cancel"/> property 
		/// may be set to true to prevent the error from being displayed.
		/// </para>
		/// </remarks>
		/// <seealso cref="ErrorEventArgs"/>
		/// <seealso cref="OnError"/>
		public event EventHandler<ErrorDisplayingEventArgs> ErrorDisplaying;

		#endregion // ErrorDisplaying

		#region ReminderActivated

		/// <summary>
		/// Used to invoke the <see cref="ReminderActivated"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ReminderActivated"/>
		protected virtual void OnReminderActivated(ReminderActivatedEventArgs args)
		{
			EventHandler<ReminderActivatedEventArgs> handler = this.ReminderActivated;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised when a reminder has been triggered.
		/// </summary>
		/// <remarks>
		/// <para class="body">The <b>ReminderActivated</b> event is raised when a reminder has been triggered.</para>
		/// </remarks>
		/// <seealso cref="ReminderActivatedEventArgs"/>
		/// <seealso cref="OnReminderActivated"/>
		public event EventHandler<ReminderActivatedEventArgs> ReminderActivated;

		#endregion // ReminderActivated

		#region ReminderDialogDisplaying

		/// <summary>
		/// Used to invoke the <see cref="ReminderDialogDisplaying"/> event.
		/// </summary>
		/// <param name="args">Event args for raising the event.</param>
		/// <seealso cref="ReminderDialogDisplaying"/>
		protected virtual void OnReminderDialogDisplaying(ReminderDialogDisplayingEventArgs args)
		{
			EventHandler<ReminderDialogDisplayingEventArgs> handler = this.ReminderDialogDisplaying;

			if (null != handler)
				handler(this, args);
		}

		/// <summary>
		/// Raised when the reminder dialog is about to be displayed.
		/// </summary>
		/// <remarks>
		/// <para class="body">The <b>ReminderDialogDisplaying</b> event is raised when the reminder dialog is about to be displayed.</para>
		/// </remarks>
		/// <seealso cref="ReminderDialogDisplayingEventArgs"/>
		/// <seealso cref="OnReminderDialogDisplaying"/>
		public event EventHandler<ReminderDialogDisplayingEventArgs> ReminderDialogDisplaying;

		#endregion // ReminderDialogDisplaying

		#endregion // Events

		#region Nested Classes

		// JM 01-06-12 TFS95261 Added.
		#region SubDialogCloseHelper
		internal class SubDialogCloseHelper
		{
			internal Action<bool?>			_originalCallback;
			internal XamScheduleDataManager _dataManager;

			internal SubDialogCloseHelper(XamScheduleDataManager dataManager, Action<bool?> originalCallback)
			{
				this._dataManager		= dataManager;
				this._originalCallback	= originalCallback;
			}

			internal FrameworkElement Dialog { get; set; }

			public void OnClosed(bool? dialogResult)
			{
				this._dataManager.ActiveSubDialogs.Remove(this);
				if (this._originalCallback != null)
				{
					this._originalCallback(dialogResult);
				}
			}
		}
		#endregion //SubDialogCloseHelper

		#endregion //Nested Classes

		#region Properties

		#region Public Properties

		#region ActiveReminders

		private ReadOnlyNotifyCollection<ReminderInfo> _activeReminders;

		/// <summary>
		/// Gets a collection of active reminders for the <see cref="CurrentUser"/>.
		/// </summary>
		/// <seealso cref="CurrentUser"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[Browsable(false)]
		public ReadOnlyNotifyCollection<ReminderInfo> ActiveReminders
		{
			get
			{
				return _activeReminders;
			}
		}

		#endregion // ActiveReminders

		#region ColorScheme

		/// <summary>
		/// Identifies the <see cref="ColorScheme"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColorSchemeProperty = DependencyPropertyUtilities.Register(
	  "ColorScheme", typeof(CalendarColorScheme), typeof(XamScheduleDataManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnColorSchemeChanged))
			);

		private static void OnColorSchemeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamScheduleDataManager item = (XamScheduleDataManager)d;

			if (item != null)
			{
				if (e.NewValue != null)
					item.ColorSchemeResolved = e.NewValue as CalendarColorScheme;
				else
					item.ColorSchemeResolved = new Office2010ColorScheme();

				// bump the version number 
				item.BumpBrushVersion();
			}            
		}

		/// <summary>
		/// Gets/sets the ColorScheme to use to create and cache brushes for the various elements in the associated <see cref="ScheduleControlBase"/> derivied classes
		/// </summary>
		/// <seealso cref="ColorSchemeProperty"/>
		public CalendarColorScheme ColorScheme
		{
			get
			{
				return (CalendarColorScheme)this.GetValue(XamScheduleDataManager.ColorSchemeProperty);
			}
			set
			{
				this.SetValue(XamScheduleDataManager.ColorSchemeProperty, value);
			}
		}

		#endregion //ColorScheme

		#region ColorSchemeResolved

		private static readonly DependencyPropertyKey ColorSchemeResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("ColorSchemeResolved",
			typeof(CalendarColorScheme), typeof(XamScheduleDataManager),
			null,
			new PropertyChangedCallback(OnColorSchemeResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="ColorSchemeResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColorSchemeResolvedProperty = ColorSchemeResolvedPropertyKey.DependencyProperty;

		private static void OnColorSchemeResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamScheduleDataManager instance = (XamScheduleDataManager)d;
			instance._colorSchemeResolved = e.NewValue as CalendarColorScheme;

			// we have to get the controls to update their default brush providers before we transfer the calendar providers.
			// that also means we need to make sure its default provider is assigned already
			instance._colorSchemeResolved.VerifyDefaultBrushProvider();
			instance.NotifyColorSchemeResolvedChanged();

			instance._colorSchemeResolved.TransferProviderAssignmentsFrom(e.OldValue as CalendarColorScheme);

			ScheduleUtilities.NotifyListenersHelper(instance, e, instance.PropChangeListeners, true, false);

		}

		/// <summary>
		/// Returns the <see cref="CalendarColorScheme"/> that will be used (read-only)
		/// </summary>
		/// <value>Returns the <see cref="ColorScheme"/> property value if set, otherwise it returns an instance of the <see cref="Office2010ColorScheme"/>.</value>
		/// <seealso cref="ColorSchemeResolvedProperty"/>
		/// <seealso cref="ColorScheme"/>
		public CalendarColorScheme ColorSchemeResolved
		{
			get
			{
				return this._colorSchemeResolved;
			}
			internal set
			{
				this.SetValue(XamScheduleDataManager.ColorSchemeResolvedPropertyKey, value);
			}
		}

		#endregion //ColorSchemeResolved
	
		#region CalendarGroups
		/// <summary>
		/// Returns the default collection of <see cref="CalendarGroup"/> instances that represent the groups in view in the controls whose <see cref="ScheduleControlBase.DataManager"/> is set to this instance.
		/// </summary>
		public CalendarGroupCollection CalendarGroups
		{
			get
			{
				if (null == _calendarGroups)
				{
					_calendarGroups = new CalendarGroupCollection();
					// note i'm not hooking into change notifications. the control using this will 
					// add in a listener if needed
					//_calendarGroups.PropChangeListeners.Add(this.PropChangeListeners, false);
					_calendarGroups.Owner = this;
				}

				return _calendarGroups;
			}
		} 
		#endregion // CalendarGroups

		#region CurrentUser

		private Resource _currentUser;

		/// <summary>
		/// Identifies the <see cref="CurrentUser"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CurrentUserProperty = DependencyPropertyUtilities.Register(
			"CurrentUser",
			typeof( Resource ),
			typeof( XamScheduleDataManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnCurrentUserChanged ) )
		);

		private static void OnCurrentUserChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager dm = (XamScheduleDataManager)d;

			dm._currentUser = (Resource)e.NewValue;
			dm.SyncCurrentUserAndId( false );

			ScheduleUtilities.NotifyListenersHelper( dm, e, dm.PropChangeListeners, true, false );
		}

		/// <summary>
		/// Identifies current user.
		/// </summary>
		/// <seealso cref="CurrentUserProperty"/>
		public Resource CurrentUser
		{
			get
			{
				return _currentUser;
			}
			set
			{
				this.SetValue( CurrentUserProperty, value );
			}
		}

		#endregion // CurrentUser

		#region CurrentUserId

		/// <summary>
		/// Identifies the <see cref="CurrentUserId"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty CurrentUserIdProperty = DependencyPropertyUtilities.Register(
			"CurrentUserId",
			typeof( string ),
			typeof( XamScheduleDataManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnCurrentUserIdChanged ) )
		);

		private static void OnCurrentUserIdChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager item = (XamScheduleDataManager)d;

			item.SyncCurrentUserAndId( true );

			ScheduleUtilities.NotifyListenersHelper( item, e, item.PropChangeListeners, false, false );
		}

		/// <summary>
		/// Specifies the resource id of the current user.
		/// </summary>
		/// <seealso cref="CurrentUserIdProperty"/>
		public string CurrentUserId
		{
			get
			{
				return (string)this.GetValue( CurrentUserIdProperty );
			}
			set
			{
				this.SetValue( CurrentUserIdProperty, value );
			}
		}

		private void SyncCurrentUserAndId( bool? syncUserToId )
		{
			ScheduleUtilities.Antirecursion.Enter( this, "SyncCurrentUserAndId", false );
			try
			{
				if ( syncUserToId ?? null == this.CurrentUser )
				{
					string resourceId = this.CurrentUserId;
					if ( !ScheduleUtilities.IsValueEmpty( resourceId ) )
					{
						ResourceCollection resources = this.ResourceItems;
						Resource resource = null != resources ? resources.GetResourceFromId( resourceId ) : null;
						this.CurrentUser = resource;
					}
				}
				else
				{
					Resource resource = this.CurrentUser;
					if ( null != resource || ! ScheduleUtilities.Antirecursion.InProgress( this, "SyncCurrentUserAndId" ) )
					{
						string resourceId = null != resource ? resource.Id : null;
						this.CurrentUserId = resourceId;
					}
				}
			}
			finally
			{
				ScheduleUtilities.Antirecursion.Exit( this, "SyncCurrentUserAndId" );
			}
		}

		#endregion // CurrentUserId

		#region DataConnector

		/// <summary>
		/// Identifies the <see cref="DataConnector"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty DataConnectorProperty = DependencyPropertyUtilities.Register(
			"DataConnector",
			typeof( ScheduleDataConnectorBase ),
			typeof( XamScheduleDataManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnDataConnectorChanged ) )
		);

		private static void OnDataConnectorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager item = (XamScheduleDataManager)d;

			ScheduleDataConnectorBase oldConnector = (ScheduleDataConnectorBase)e.OldValue;
			ScheduleDataConnectorBase newConnector = (ScheduleDataConnectorBase)e.NewValue;

			item._cachedDataConnector = newConnector;

			PresentationUtilities.ReparentElement(item._rootPanel, oldConnector, false);
			PresentationUtilities.ReparentElement(item._rootPanel, newConnector, true);

			item.OnDataConnectorChanged( oldConnector, newConnector );

			ScheduleUtilities.NotifyListenersHelper( item, e, item.PropChangeListeners, true, true );
		}

		private void OnDataConnectorChanged( ScheduleDataConnectorBase oldConnector, ScheduleDataConnectorBase newConnector )
		{
			this.SetResourceItemsInternal( null != newConnector ? newConnector.ResourceItems : null );

			if (oldConnector != null)
				this.RefreshDisplay();
		}

		/// <summary>
		/// Gets or sets the data connector that provides the scheduling data.
		/// </summary>
		/// <remarks>
		/// <b>DataConnector</b> property is used to specify a <see cref="ScheduleDataConnectorBase"/> derived class instance
		/// that provides the scheduling data to this <i>XamScheduleDataManager</i> instance. Any changes made to the schedule
		/// information via this class or via any of the schedule controls are updated back to the data connector which may
		/// update its underlying data sources.
		/// </remarks>
		/// <seealso cref="DataConnectorProperty"/>
		/// <seealso cref="ScheduleDataConnectorBase"/>
		/// <seealso cref="ListScheduleDataConnector"/>
		public ScheduleDataConnectorBase DataConnector
		{
			get
			{
				return _cachedDataConnector;
			}
			set
			{
				this.SetValue( DataConnectorProperty, value );
			}
		}

		#endregion // DataConnector

		#region DateInfoProvider

		/// <summary>
		/// Identifies the <see cref="DateInfoProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DateInfoProviderProperty = DependencyPropertyUtilities.Register("DateInfoProvider",
			typeof(DateInfoProvider), typeof(XamScheduleDataManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDateInfoProviderChanged))
			);

		private static void OnDateInfoProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamScheduleDataManager instance = (XamScheduleDataManager)d;

			instance._cachedDateInfoProvider = e.NewValue as DateInfoProvider;

			if (instance._dateInfoProviderResolved != null)
			{
				instance._dateInfoProviderResolved = null;
				instance.RefreshDisplay();
			}
		}

		/// <summary>
		/// Returns or sets an object that formats and manipulates dates
		/// </summary>
		/// <seealso cref="DateInfoProviderProperty"/>
		public DateInfoProvider DateInfoProvider
		{
			get
			{
				return this._cachedDateInfoProvider;
			}
			set
			{
				this.SetValue(XamScheduleDataManager.DateInfoProviderProperty, value);
			}
		}

		#endregion //DateInfoProvider

		#region DateInfoProviderResolved

		/// <summary>
		/// Returns the object that formats and manipulates dates
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if <see cref="DateInfoProvider"/> is not set then an instance of a <see cref="DateInfoProvider"/> with the current culture will be used.</para>
		/// </remarks>
		/// <seealso cref="DateInfoProvider"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[Browsable(false)]
		[ReadOnly(true)]
		[Bindable(false)]
		public DateInfoProvider DateInfoProviderResolved
		{
			get
			{
				if (this._dateInfoProviderResolved == null)
				{
					if (this._cachedDateInfoProvider != null)
						this._dateInfoProviderResolved = this._cachedDateInfoProvider;
					else
						this._dateInfoProviderResolved = DateInfoProvider.CurrentProvider;
				}

				return this._dateInfoProviderResolved;
			}
		}

		#endregion //DateInfoProviderResolved	

		#region DialogFactory

		/// <summary>
		/// Identifies the <see cref="DialogFactory"/> dependency property
		/// </summary>
		public static readonly DependencyProperty DialogFactoryProperty = DependencyPropertyUtilities.Register("DialogFactory",
			typeof(ScheduleDialogFactoryBase), typeof(XamScheduleDataManager),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnDialogFactoryChanged))
			);

		private static void OnDialogFactoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamScheduleDataManager instance = (XamScheduleDataManager)d;

			instance._cachedDialogFactory = e.NewValue as ScheduleDialogFactoryBase;
		}

		/// <summary>
		/// Returns/sets the <see cref="ScheduleDialogFactoryBase"/> derived class that supplies dialogs for the XamSchedule controls.   
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If this property is not set, the <b>XamSchedule</b> controls will use default the 'lightweight' dialogs contained in the
		/// <b>XamSchedule</b> assembly.  If you want to use the 'heavyweight' versions of the dialogs which reference external
		/// controls (e.g., the <b>XamRibbon</b> control), then you should add a reference to the <b>Schedule.Dialogs</b> assembly
		/// to your project, and set this property to an instance of the <b>ScheduleDialogFactory class defined in that assembly.</b>
		/// </para>
		/// </remarks>
		/// <seealso cref="DialogFactoryProperty"/>
		public ScheduleDialogFactoryBase DialogFactory
		{
			get
			{
				return this._cachedDialogFactory;
			}
			set
			{
				this.SetValue(XamScheduleDataManager.DialogFactoryProperty, value);
			}
		}

		#endregion //DialogFactory

		#region HasPendingOperations

		private static readonly DependencyPropertyKey HasPendingOperationsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("HasPendingOperations",
			typeof(bool), typeof(XamScheduleDataManager), KnownBoxes.FalseBox, null);

		/// <summary>
		/// Identifies the read-only <see cref="HasPendingOperations"/> dependency property
		/// </summary>
		public static readonly DependencyProperty HasPendingOperationsProperty = HasPendingOperationsPropertyKey.DependencyProperty;

		/// <summary>
		/// Returns a boolean indicating if there are any pending operations waiting to be completed.
		/// </summary>
		/// <seealso cref="HasPendingOperationsProperty"/>
		public bool HasPendingOperations
		{
			get
			{
				return (bool)this.GetValue(XamScheduleDataManager.HasPendingOperationsProperty);
			}
			private set
			{
				this.SetValue(XamScheduleDataManager.HasPendingOperationsPropertyKey, value);
			}
		}

		#endregion //HasPendingOperations

		#region PendingOperations
		/// <summary>
		/// Returns a collection of <see cref="OperationResult"/> instances that have not yet completed.
		/// </summary>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[Browsable(false)]
		public ReadOnlyNotifyCollection<OperationResult> PendingOperations
		{
			get
			{
				if (_pendingOperations == null)
					_pendingOperations = new ReadOnlyNotifyCollection<OperationResult>(this.PendingOperationsSource);

				return _pendingOperations;
			}
		} 
		#endregion // PendingOperations

		#region PromptForLocalTimeZone

		/// <summary>
		/// Identifies the <see cref="PromptForLocalTimeZone"/> dependency property
		/// </summary>
		public static readonly DependencyProperty PromptForLocalTimeZoneProperty = DependencyPropertyUtilities.Register("PromptForLocalTimeZone",
			typeof(PromptForLocalTimeZone), typeof(XamScheduleDataManager),
			DependencyPropertyUtilities.CreateMetadata(PromptForLocalTimeZone.OnlyIfRequired)
			);

		/// <summary>
		/// Returns or sets an enum that determines if a prompt will be displayed asking the user to select a local time zone
		/// </summary>
		/// <seealso cref="PromptForLocalTimeZoneProperty"/>
		public PromptForLocalTimeZone PromptForLocalTimeZone
		{
			get
			{
				return (PromptForLocalTimeZone)this.GetValue(XamScheduleDataManager.PromptForLocalTimeZoneProperty);
			}
			set
			{
				this.SetValue(XamScheduleDataManager.PromptForLocalTimeZoneProperty, value);
			}
		}

		#endregion //PromptForLocalTimeZone

		#region RecurrenceCalculatorFactory

		/// <summary>
		/// Gets the recurrence calculator factory used to get <see cref="DateRecurrenceCalculatorBase"/> instance 
		/// for a <see cref="DateRecurrence"/>, which is used for calculating occurrences of the recurrence.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can provide a custom logic for calculating recurrences by setting the ScheduleDataConnectorBase's 
		/// <see cref="ScheduleDataConnectorBase.RecurrenceCalculatorFactory"/> property.
		/// </para>
		/// </remarks>
		/// <seealso cref="DateRecurrence"/>
		/// <seealso cref="DateRecurrenceCalculatorBase"/>
		/// <seealso cref="ScheduleDataConnectorBase.RecurrenceCalculatorFactory"/>
		public RecurrenceCalculatorFactoryBase RecurrenceCalculatorFactory
		{
			get
			{
				ScheduleDataConnectorBase connector = this.DataConnector;
				return null != connector ? connector.RecurrenceCalculatorFactoryResolved : null;
			}
		}

		#endregion // RecurrenceCalculatorFactory

		#region ResourceItems

		/// <summary>
		/// Identifies the property key for read-only <see cref="ResourceItems"/> dependency property.
		/// </summary>
		private static readonly DependencyPropertyKey ResourceItemsPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"ResourceItems",
			typeof( ResourceCollection ),
			typeof( XamScheduleDataManager ),
			null, 
			new PropertyChangedCallback( OnResourceItemsChanged )
		);

		/// <summary>
		/// Identifies the read-only <see cref="ResourceItems"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResourceItemsProperty = ResourceItemsPropertyKey.DependencyProperty;

		private void SetResourceItemsInternal( ResourceCollection collection )
		{
			this.SetValue( ResourceItemsPropertyKey, collection );
		}

		/// <summary>
		/// Gets a collection of <see cref="Resource"/>s.
		/// </summary>
		/// <remarks>
		/// <b>ResourceItems</b> property returns a collection of <see cref="Resource"/> objects that represent the
		/// resources in the schedule.
		/// </remarks>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[Browsable(false)]
		public ResourceCollection ResourceItems
		{
			get
			{
				return (ResourceCollection)this.GetValue( ResourceItemsProperty );
			}
		}

		private static void OnResourceItemsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager item = (XamScheduleDataManager)d;

			ScheduleUtilities.NotifyListenersHelper(item, e, item.PropChangeListeners, true, false);
		}

		#endregion // ResourceItems

		#region Settings

		/// <summary>
		/// Identifies the <see cref="Settings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty SettingsProperty = DependencyPropertyUtilities.Register(
			"Settings",
			typeof( ScheduleSettings ),
			typeof( XamScheduleDataManager ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnSettingsChanged ) )
		);

		private static void OnSettingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager item = (XamScheduleDataManager)d;
			item._cachedSettings = (ScheduleSettings)e.NewValue;

			ScheduleUtilities.NotifyListenersHelper( item, e, item.PropChangeListeners, true, false );
		}

		/// <summary>
		/// Gets or sets the ScheduleSettings object used for specifying various settings.
		/// </summary>
		/// <seealso cref="ScheduleSettings"/>
		public ScheduleSettings Settings
		{
			get
			{
				return _cachedSettings;
			}
			set
			{
				this.SetValue( SettingsProperty, value );
			}
		}

		#endregion // Settings

		#endregion // Public Properties

		#region Internal Properties

		#region BlockingError

		private DataErrorInfo _cachedBlockingError;

		private static readonly DependencyPropertyKey BlockingErrorPropertyKey = DependencyPropertyUtilities.RegisterReadOnly(
			"BlockingError",
			typeof( DataErrorInfo ),
			typeof( XamScheduleDataManager ),
			null,
			new PropertyChangedCallback( OnBlockingErrorChanged )
		);

		/// <summary>
		/// Identifies the read-only <see cref="BlockingError"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty BlockingErrorProperty = BlockingErrorPropertyKey.DependencyProperty;

		private static void OnBlockingErrorChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			XamScheduleDataManager dm = (XamScheduleDataManager)d;
			dm._cachedBlockingError = (DataErrorInfo)e.NewValue;
			ScheduleUtilities.NotifyListenersHelper( dm, e, dm.PropChangeListeners, false, false );
		}

		/// <summary>
		/// Gets the blocking error if any.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>BlockingError</b> property is set when an error occurs that prevents further functioning of the control.
		/// </para>
		/// </remarks>
		internal DataErrorInfo BlockingError
		{
			get
			{
				return _cachedBlockingError;
			}
			private set
			{
				this.SetValue( BlockingErrorPropertyKey, value );
			}
		}

		#endregion // BlockingError

		#region Controls
		internal WeakList<IScheduleControl> Controls
		{
			get { return _controls; }
		}
		#endregion //Controls

		#region CurrentDate

		private static readonly DependencyPropertyKey CurrentDatePropertyKey = DependencyPropertyUtilities.RegisterReadOnly("CurrentDate",
			typeof(DateTime), typeof(XamScheduleDataManager),
			DateTime.Today,
			new PropertyChangedCallback(OnCurrentDateChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="CurrentDate"/> dependency property
		/// </summary>
		internal static readonly DependencyProperty CurrentDateProperty = CurrentDatePropertyKey.DependencyProperty;

		private static void OnCurrentDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			XamScheduleDataManager instance = (XamScheduleDataManager)d;
			instance.NotifySettingsChanged(instance, DependencyPropertyUtilities.GetName(e.Property), e);
		}

		/// <summary>
		/// Returns the calendar date for the current logical day
		/// </summary>
		/// <seealso cref="CurrentDateProperty"/>
		internal DateTime CurrentDate
		{
			get
			{
				return (DateTime)this.GetValue(XamScheduleDataManager.CurrentDateProperty);
			}
			private set
			{
				this.SetValue(XamScheduleDataManager.CurrentDatePropertyKey, value);
			}
		}

		#endregion //CurrentDate

		#region DragManager
		internal ActivityDragManager DragManager
		{
			get { return _dragManager; }
		}
		#endregion // DragManager

		// JM 01-06-12 TFS95261 Added.
		#region IsClosingAllDialogs
		internal bool IsClosingAllDialogs
		{
			get { return this._isClosingAllDialogs; }
		}
		#endregion //IsClosingAllDialogs

		#region IsLogicalDayDifferent






		internal bool IsLogicalDayDifferent
		{
			get
			{
				return	this.LogicalDayOffset	!= TimeSpan.Zero ||
						this.LogicalDayDuration != TimeSpan.FromTicks(TimeSpan.TicksPerDay);
			}
		}
		#endregion IsLogicalDayDifferent

		#region LogicalDayDuration
		internal TimeSpan LogicalDayDuration
		{
			get
			{
				if (_cachedSettings == null)
					return TimeSpan.FromTicks(TimeSpan.TicksPerDay);

				long durationTicks = Math.Min(TimeSpan.TicksPerDay, Math.Max(TimeSpan.TicksPerSecond, _cachedSettings.LogicalDayDuration.Ticks));
				return TimeSpan.FromTicks(durationTicks);
			}
		}
		#endregion // LogicalDayDuration

		#region LogicalDayOffset
		internal TimeSpan LogicalDayOffset
		{
			get 
			{ 
				return _cachedSettings != null 
					? ScheduleUtilities.ConstrainLogicalDayOffset(_cachedSettings.LogicalDayOffset) 
					: TimeSpan.Zero; 
			}
		}
		#endregion // LogicalDayOffset

		#region TimeZoneInfoProviderResolved

		internal TimeZoneInfoProvider TimeZoneInfoProviderResolved
		{
			get
			{
				if (this._cachedDataConnector != null)
					return this._cachedDataConnector.TimeZoneInfoProviderResolved;

				return TimeZoneInfoProvider.DefaultProvider;
			}
		}

		#endregion //TimeZoneInfoProviderResolved	

		#endregion //Internal Properties

		#region Private Properties

		// JM 01-06-12 TFS95261
		#region ActiveSubDialogs
		private WeakList<SubDialogCloseHelper> ActiveSubDialogs
		{
			get
			{
				if (this._activeSubDialogs == null)
					this._activeSubDialogs = new WeakList<SubDialogCloseHelper>();

				return this._activeSubDialogs;
			}
		}
		#endregion //ActiveSubDialogs

		#region ActiveDialogMap
		// JM 01-06-12 TFS95261 - Change this method and all references within it to be non-static.
		private WeakDictionary<object, FrameworkElement> ActiveDialogMap
		{
			get
			{
				if (this._activeDialogMap == null)
					this._activeDialogMap = new WeakDictionary<object,FrameworkElement>(true, true, 10);

				return this._activeDialogMap;
			}
		}
		#endregion //ActiveDialogMap

		#region PendingOperationsSource
		private ObservableCollectionExtended<OperationResult> PendingOperationsSource
		{
			get
			{
				if (_pendingOperationsSource == null)
				{
					_pendingOperationsSource = new ObservableCollectionExtended<OperationResult>(false, true);
					_pendingOperationsSource.PropChangeListeners.Add(new PropertyChangeListener<XamScheduleDataManager>(this, OnPendingOperationsChanged), false);
				}

				return _pendingOperationsSource;
			}
		}
		#endregion // PendingOperationsSource

		#region PropChangeListener

		/// <summary>
		/// Gets collection of property change listeners.
		/// </summary>
		internal PropertyChangeListenerList PropChangeListeners
		{
			get
			{
				if ( null == _propChangeListeners )
				{
					_propChangeListeners = new PropertyChangeListenerList( );
					_propChangeListeners.Add( new PropertyChangeListener<XamScheduleDataManager>( this, OnSubObjectPropertyChanged ), false );
				}

				return _propChangeListeners;
			}
		}

		#endregion // PropChangeListener

		#endregion // Private Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region AreActivitiesSupported

		/// <summary>
		/// Indicates whether the activities of the specified activity type are supported by this data connector.
		/// </summary>
		/// <param name="activityType">Activity type.</param>
		/// <returns>True if the specified activities are supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if activities of the specified type are supported by the data connector.
		/// If they are not supported, relevant user interface will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		public bool AreActivitiesSupported( ActivityType activityType )
		{
			ScheduleDataConnectorBase connector = this.DataConnector;

			return null != connector && connector.AreActivitiesSupported( activityType );
		}

		#endregion // AreActivitiesSupported

		#region AreCustomActivityCategoriesAllowed

		/// <summary>
		/// Indicates whether the user is allowed to create, remove or modify the activity categories in
		/// the associated Resource object's <see cref="Resource.CustomActivityCategories"/> collection.
		/// </summary>
		/// <param name="resource">Resource object.</param>
		/// <returns>True if custom activity categories for the resource are allowed.</returns>

		[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

		public virtual bool AreCustomActivityCategoriesAllowed( Resource resource )
		{
			return this.IsResourceFeatureSupported( resource, ResourceFeature.CustomActivityCategories )
				&& ( null == _cachedSettings || ( _cachedSettings.AllowCustomizedCategories ?? true ) );
		}

		#endregion // AreCustomActivityCategoriesAllowed

		#region BeginEdit

		/// <summary>
		/// Begins modifications to an activity.
		/// </summary>
		/// <param name="activity">ActivityBase derived object that is to be modified.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A value indicating whether the operation succeeded.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> BeginEdit cannot be called more than once without an intervening call to CancelEdit or EndEdit. Successive BeginEdit
		/// calls will result an error and return false.
		/// </para>
		/// </remarks>
		public virtual bool BeginEdit( ActivityBase activity, out DataErrorInfo errorInfo )
		{
			// SSP 11/2/10 TFS58839
			// Make sure the activity's owning calendar is initialized if this activity is from the data source
			// (data source is a list of our activity objects) and the list connector hasn't had a chance to
			// initialize it. This can happen if one uses an activity directly from the data source to perform
			// edit operation.
			// 
			this.EnsureOwningCalendarInitialized( activity );

			bool retVal = this.ValidateHasConnector( out errorInfo )
				&& this.DataConnector.BeginEdit( activity, out errorInfo );

			// Set IsInEdit property to true to indicate that the activity is in edit mode.
			// 
			if ( retVal )
				activity.IsInEdit = true;

			// Manage activity's Error property. If there's an error, set it otherwise clear it.
			// 
			DataManagerActivityOperationCompletedHandler.ManageActivityErrorHelper( this, activity, errorInfo, true );

			return retVal;
		}

		#endregion // BeginEdit

		#region BeginEdit

		/// <summary>
		/// Begins modifications to a Resource object.
		/// </summary>
		/// <param name="resource">Resource object that is to be modified.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A value indicating whether the operation succeeded.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> BeginEdit cannot be called more than once without an intervening call to CancelEdit or EndEdit. Successive BeginEdit
		/// calls will result an error and return false.
		/// </para>
		/// </remarks>
		public virtual bool BeginEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			bool retVal = this.ValidateHasConnector( out errorInfo )
				&& this.DataConnector.BeginEdit( resource, out errorInfo );

			return retVal;
		}

		#endregion // BeginEdit

		#region BeginEditWithCopy

		/// <summary>
		/// Used to make a copy of an activity for the purposes of editing it without affecting the original activity until all the changes need to be committed.
		/// </summary>
		/// <param name="activity">ActivityBase derived object for which a copy will be created.</param>
		/// <param name="synchronizeChangesFromOriginalActivity">If true any changes made to the original activity will be
		/// applied to the copy that's returned.</param>
		/// <param name="error">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>Cloned copy of the specified activity.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>BeginEditWithCopy</b> method is meant to be used to create a copy of an activity so the copy can be
		/// modified without affecting the original activity. It returns a copy of the original activity.
		/// When the changes made to the copy need to be committed to the original activity, call 
		/// <see cref="EndEdit(ActivityBase, bool)"/> method passing to it the clone activity that's returned by this method. Also to 
		/// cancel and discard the clone activity, you need to call CancelEdit passing it the clone activity.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that successive calls to this method without intervening cancel edit or end edit operations
		/// on the returned clone will result in the same clone being returned.
		/// </para>
		/// </remarks>
		public ActivityBase BeginEditWithCopy( ActivityBase activity, bool synchronizeChangesFromOriginalActivity, out DataErrorInfo error )
		{
			// SSP 11/2/10 TFS58839
			// Make sure the activity's owning calendar is initialized if this activity is from the data source
			// (data source is a list of our activity objects) and the list connector hasn't had a chance to
			// initialize it. This can happen if one uses an activity directly from the data source to perform
			// edit operation.
			// 
			this.EnsureOwningCalendarInitialized( activity );

			error = null;
			ActivityBase clone = activity.GetEditCopy( true, synchronizeChangesFromOriginalActivity );

			// MD 1/5/11 - NA 11.1 - Exchange Data Connector
			this.DataConnector.InitializeEditCopy(clone);

			return clone;
		}

		#endregion // BeginEditWithCopy

		#region CancelEdit

		/// <summary>
		/// Cancels a new activity that was created by the <see cref="CreateNew"/> call however one that 
		/// hasn't been commited yet.
		/// </summary>
		/// <param name="activity">ActivityBase derived object that was created using <see cref="CreateNew"/> method however
		/// one that hasn't been committed yet.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>CancelEdit</b> method is called to cancel a new activity that was created using the 
		/// <see cref="CreateNew"/> method however the activity must not have been commited yet. This is 
		/// typically done when the user cancels the dialog for creating a new activity, like the new appointment dialog.
		/// </para>
		/// </remarks>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		public virtual bool CancelEdit( ActivityBase activity, out DataErrorInfo errorInfo )
		{
			// If the edit copy is passed in (used by the edit appointment dialog) then clear edit copy
			// on the original activity.
			// 
			bool retVal;
			if ( activity.IsEditCopy )
			{
				errorInfo = (DataErrorInfo)this.EndEditOnCopyHelper( activity, true, true );
				retVal = null == errorInfo;
			}
			else
			{
				retVal = this.ValidateHasConnector( out errorInfo )
					&& this.DataConnector.CancelEdit( activity, out errorInfo );

				if ( !retVal && null == errorInfo )
					errorInfo = ScheduleUtilities.CreateDiagnosticFromId(activity, "LE_CancelEditFailed"); // "Unspecified error. Cancel edit operation failed."

				// Reset the IsInEdit and IsAddNew to false since the operation is canceled.
				// 
				activity.IsInEdit = false;
				activity.IsAddNew = false;
			}

			// Manage activity's Error property. If there's an error, set it otherwise clear it.
			// 
			DataManagerActivityOperationCompletedHandler.ManageActivityErrorHelper( this, activity, errorInfo, true );

			return retVal;
		}

		#endregion // CancelEdit

		#region CancelEdit

		/// <summary>
		/// Cancels modifications to a Resource object.
		/// </summary>
		/// <param name="resource">Resource object.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>CancelEdit</b> method is called to cancel modifications that were made to a Resource object
		/// after <see cref="BeginEdit(Resource, out DataErrorInfo)"/> method was called on it.
		/// </para>
		/// </remarks>
		/// <seealso cref="EndEdit(Resource, bool)"/>
		/// <seealso cref="CancelEdit(Resource, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		public virtual bool CancelEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			bool retVal = this.ValidateHasConnector( out errorInfo )
					&& this.DataConnector.CancelEdit( resource, out errorInfo );

			return retVal;
		}

		#endregion // CancelEdit

		#region CancelPendingOperation

		/// <summary>
		/// Cancels a pending operation.
		/// </summary>
		/// <param name="operation">Pending operation that is to be canceled.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>CancelPendingOperation</b> method is called to cancel a pending operation. It's only valid for
		/// operations that are still pending, that is their <see cref="OperationResult.IsComplete"/> is false.
		/// </para>
		/// <para class="body">
		/// An example of how this method is used is as follows. <see cref="GetActivities(ActivityQuery)"/>
		/// method returns <see cref="ActivityQueryResult"/> object. The activities can be retrieved 
		/// asynchronously. Before the activities are retrieved, there may be a need for canceling the operation.
		/// For example, the user scrolls the schedule control to a different range of dates in which case
		/// it's not necessary to retrieve activities for the perviously visible date range. In such a case,
		/// the previous query operation will be canceled if it's still pending.
		/// </para>
		/// </remarks>
		public CancelOperationResult CancelPendingOperation( OperationResult operation )
		{
			Debug.Assert( null != operation && !operation.IsComplete );

			DataErrorInfo error;
			if ( !this.ValidateHasConnector( out error ) )
				return new CancelOperationResult( operation, error, true );

			CancelOperationResult result = this.DataConnector.CancelPendingOperation( operation );
			
			// If an activity operation is being canceled, set the activity's PendingOperation to 
			// the result of the cancel operation if they are still pending.
			// 
			DataManagerActivityOperationCompletedHandler.OnCancelOperationBeingPerformed( this, result );

			return result;
		}

		#endregion // CancelPendingOperation
		
		#region CreateNew

		/// <summary>
		/// Creates a new ActivityBase derived instance based on the activityType parameter.
		/// </summary>
		/// <param name="activityType"></param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A new ActivityBase derived object created according to the activityType parameter.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>CreateActivity</b> creates a new <see cref="ActivityBase"/> derived object, like Appointment, Journal or Task.
		/// Which activity type to create is specified by the <paramref name="activityType"/> parameter. Note that the created activity 
		/// doesn't get commited to the data source until <see cref="EndEdit(ActivityBase, bool)"/> method is called. Also if you wish to
		/// not commit the created activity then it is necessary to call <see cref="CancelEdit(ActivityBase, out DataErrorInfo)"/> 
		/// so the activity object can be properly discarded by the the schedule data connector.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> <b>CreateNew</b> method is called to create a new Appointment, Journal or Task object. 
		/// This is typically done when the user initiates creation of a new activity in one of the calendar view controls. If 
		/// the user commits the appointment then <i>EndEdit</i> method is called to commit the activity. 
		/// If the user cancels the activity creation then <i>CancelEdit</i> method is called to discard the activity object.
		/// </para>
		/// </remarks>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="Remove"/>
		public virtual ActivityBase CreateNew( ActivityType activityType, out DataErrorInfo errorInfo )
		{
			ActivityBase activity = this.ValidateHasConnector( out errorInfo )
				? this.DataConnector.CreateNew( activityType, out errorInfo )
				: null;

			// Set the IsAddNew flag on the activity so we know to raise ActivityAdding when EndEdit is called
			// to commit the activity.
			// 
			if ( null != activity )
			{
				activity.IsAddNew = true;
				activity.IsInEdit = true;
			}

			return activity;
		}

		#endregion // CreateNew

		#region EndEdit

		/// <summary>
		/// Commits a new or modified activity.
		/// </summary>
		/// <param name="activity">A new or modified ActivityBase derived instance. 
		/// If <see cref="BeginEditWithCopy"/> is used to start edit operation, you can pass the copy that 
		/// was returned from that method.</param>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode.
		/// </param>
		/// <returns><see cref="ActivityOperationResult"/> instance which may be initialized with the result
		/// asynchronously.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>EndEdit</b> method is used to commit a modified activity or a new activity that 
		/// was created using the <see cref="CreateNew"/> method.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the operation of committing an activity can be performed either synchronously
		/// or asynchronously. If the operation is performed synchronously then the information regarding
		/// the result of the operation will be contained in the returned <i>ActivityOperationResult</i>
		/// instance. If the operation is performed asynchronously, the method will return an 
		/// <i>ActivityOperationResult</i> instance whose results will be initialized later when they
		/// are available via the ActivityOperationResult's <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>
		/// method. The caller, which may be a schedule control, will indicate via the UI that the operation
		/// is pending and when the results are initialized, it will show the user with appropriate
		/// status of the operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		public virtual ActivityOperationResult EndEdit( ActivityBase activity, bool force = false )
		{
			CoreUtilities.ValidateNotNull( activity );

			DataErrorInfo errorInfo;
			ActivityOperationResult result;
			if ( !this.ValidateHasConnector( out errorInfo ) )
			{
				result = new ActivityOperationResult( activity, errorInfo, true );
			}
			// If the edit copy is passed in (used by the edit appointment dialog) then apply it's value to
			// the originaly activity and perform end edit operation on the original activity via the connector.
			// 
			else if ( activity.IsEditCopy )
			{
				object retVal = this.EndEditOnCopyHelper( activity, false, force );
				if ( retVal is ActivityOperationResult )
					result = (ActivityOperationResult)retVal;
				else
					result = new ActivityOperationResult( activity, (DataErrorInfo)retVal, true );
			}
			else
			{
				bool activityAddingRaised = false;
				bool activityChangingRaised = false;
				bool canceled = false;

				// If the activity is an add-new activity then raise ActivityAdding event.
				// 
				if ( activity.IsAddNew )
				{
					ActivityAddingEventArgs args = new ActivityAddingEventArgs( activity );
					this.RaiseActivityAdding( args );
					activityAddingRaised = true;
					canceled = args.Cancel;
				}
				else
				{
					// Raise ActivityChanging event after calling BeginEdit to be consistent with what we do
					// for in-place editing of the activity subject.
					// 
					ActivityChangingEventArgs args = new ActivityChangingEventArgs( activity, activity.BeginEditData );
					this.RaiseActivityChanging( args );
					activityChangingRaised = true;
					canceled = args.Cancel;
				}

				// If the ActivityAdding or ActivityChanging is canceled then cancel the activity and return 
				// an empty result to indicate success. We should not return an error here because the operation
				// is canceled by the developer and therefore there's really no error that took place.
				// 
				
				// of invalid data and that the add dialog should be kept open. We might want to add
				// validating event.
				// 
				if ( canceled )
				{
					bool retVal = this.CancelEdit( activity, out errorInfo );

					// Note that the cancel edit operation cannot be canceled. That means the CancelEdit
					// call above must end the edit mode.
					// 
					result = new ActivityOperationResult( activity, errorInfo, true );
				}
				else
				{
					// If the activity was modified then update the last modified time. If BeginEditData is
					// null then that means the connector is keeping track of the data itself and therefore
					// it should manage the LastModifiedTime.
					// 
					if ( activity.IsAddNew || null != activity.BeginEditData && activity.IsDataDifferentFromBeginEditData( ) )
						activity.UpdateLastModifiedTime( );

					result = this.DataConnector.EndEdit( activity, force );

					// Reset the IsInEdit flag to false. Note that we don't wait for the asynchronous result
					// to complete because IsInEdit flag is tied to activity maintaining edit state (original
					// values), and indirectly tied to the user interface being in edit mode. Since the 
					// edit state is being discarded and the user interface should exit edit mode, reset
					// the flag to false at this point.
					// 
					activity.IsInEdit = false;

					// If we raised ActivityAdding or ActivityChanging above, then raise the corresponding ActivityAdded
					// or ActivityChanged respectively.
					// 
					if ( activityAddingRaised || activityChangingRaised )
						DataManagerActivityOperationCompletedHandler.RaiseEventHelper( this, result, activityAddingRaised ? ActivityOperation.Add : ActivityOperation.Edit );
				}
			}

			// Manage activity's Error property. If there's an error, set it otherwise clear it.
			// 
			DataManagerActivityOperationCompletedHandler.ManageActivityErrorHelper( this, result, true );

			return result;
		}

		#endregion // EndEdit

		#region EndEdit

		/// <summary>
		/// Commits a modified Resource object.
		/// </summary>
		/// <param name="resource">Resource object.</param>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode.
		/// </param>
		/// <returns><see cref="ResourceOperationResult"/> instance which may be initialized with the result
		/// asynchronously.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>EndEdit</b> method is used to commit modifications made to a Resource object after 
		/// <see cref="BeginEdit(Resource, out DataErrorInfo)"/> is called on it.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that the operation of committing a Resource object can be performed either synchronously
		/// or asynchronously. If the operation is performed synchronously then the information regarding
		/// the result of the operation will be contained in the returned <i>ActivityOperationResult</i>
		/// instance. If the operation is performed asynchronously, the method will return an 
		/// <i>ActivityOperationResult</i> instance whose results will be initialized later when they
		/// are available via the ActivityOperationResult's <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>
		/// method.
		/// </para>
		/// </remarks>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="CancelEdit(Resource, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		public virtual ResourceOperationResult EndEdit( Resource resource, bool force = false )
		{
			CoreUtilities.ValidateNotNull( resource );

			DataErrorInfo errorInfo;
			ResourceOperationResult result;
			if ( !this.ValidateHasConnector( out errorInfo ) )
				result = new ResourceOperationResult( resource, errorInfo, true );
			else
				result = this.DataConnector.EndEdit( resource, force );

			return result;
		}

		#endregion // EndEdit

		#region IsActivityEditingAllowed

		/// <summary>
		/// Indicates whether the specified property can be edited on the specified activity.
		/// </summary>
		/// <param name="activity">Activity instance on which the property might be edited.</param>
		/// <param name="property">The property to check whether editing is allowed.</param>
		/// <returns>True if the property can be edited. False otherwise.</returns>

		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

		public bool IsActivityEditingAllowed(ActivityBase activity, EditableActivityProperty property)
		{
			ScheduleDataConnectorBase connector = this.DataConnector;
			return null == connector || connector.IsActivityEditingAllowed(activity, property);
		} 

		#endregion  // IsActivityEditingAllowed

		#region IsActivityFeatureSupported

		/// <summary>
		/// Indicates whether the specified feature is supported for the activities of the specified type.
		/// </summary>
		/// <param name="activity">Activity object to check if the specified feature is supported.</param>
		/// <param name="activityFeature">Feature to check for support.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// Whether a feature is supported is dependent upon the capabilities of the underlying schedule
		/// data connector. For example, in the case of the list connector, a feature would be supported
		/// if the data source contains the necessary property or properties required to support the specified feature.
		/// </para>
		/// </remarks>
		public bool IsActivityFeatureSupported( ActivityBase activity, ActivityFeature activityFeature )
		{
			CoreUtilities.ValidateNotNull(activity, "activity");
			return this.IsActivityFeatureSupported(activity.ActivityType, activityFeature, activity.OwningCalendar);
		}

		/// <summary>
		/// Indicates whether the specified feature is supported for the activities of the specified type.
		/// </summary>
		/// <param name="activityType">Activity type for which to check if the specified feature is supported.</param>
		/// <param name="activityFeature">Feature to check for support.</param>
		/// <param name="calendar">Resource calendar context.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// Whether a feature is supported is dependent upon the capabilities of the underlying schedule
		/// data connector. For example, in the case of the list connector, a feature would be supported
		/// if the data source contains the necessary property or properties required to support the specified feature.
		/// </para>
		/// </remarks>
		public bool IsActivityFeatureSupported( ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar )
		{
			ScheduleDataConnectorBase connector = this.DataConnector;
			return null != connector && connector.IsActivityFeatureSupported(activityType, activityFeature, calendar);
		}

		#endregion // IsActivityFeatureSupported

		#region IsActivityOperationAllowed

		/// <summary>
		/// Indicates whether the specified activity operation for the specified activity type is supported.
		/// </summary>
		/// <param name="activity">Activity instance.</param>
		/// <param name="activityOperation">Activity operation.</param>
		/// <returns>True if the operation is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified operation is supprted for the activities of the specified type.
		/// If the operation is not supported, relevant user interface will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		public bool IsActivityOperationAllowed( ActivityBase activity, ActivityOperation activityOperation )
		{
			
			if ( !this.CheckPermissionsForActivityOperation( activity.ActivityType, activityOperation, activity, null ) )
				return false;

			ScheduleDataConnectorBase connector = this.DataConnector;

			return this.AreActivitiesSupported( activity.ActivityType )
				&& null != connector && connector.IsActivityOperationSupported( activity, activityOperation );
		}

		/// <summary>
		/// Indicates whether the specified activity operation for the specified activity type is supported.
		/// </summary>
		/// <param name="activityType">Activity type.</param>
		/// <param name="activityOperation">Activity operation.</param>
		/// <param name="calendar">ResourceCalendar for which to check if the operation can be performed.</param>
		/// <returns>True if the operation is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified operation is supprted for the activities of the 
		/// specified type for the specified ResourceCalendar. If the operation is not supported, relevant user interface will 
		/// be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		public bool IsActivityOperationAllowed( ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar )
		{
			
			if ( !this.CheckPermissionsForActivityOperation( activityType, activityOperation, null, calendar ) )
				return false;

			ScheduleDataConnectorBase connector = this.DataConnector;
			
			return this.AreActivitiesSupported( activityType )
				&& null != connector && connector.IsActivityOperationSupported( activityType, activityOperation, calendar );
		}

		#endregion // IsActivityOperationAllowed

		#region IsResourceFeatureSupported

		/// <summary>
		/// Indicates whether the specified feature is supported for the specified resource.
		/// </summary>
		/// <param name="resource">Resource object.</param>
		/// <param name="resourceFeature">Resource feature.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// Whether a feature is supported is dependent upon the capabilities of the underlying schedule
		/// data connector. For example, in the case of the list connector, a feature would be supported
		/// if the data source contains the necessary property or properties required to support the specified feature.
		/// </para>
		/// </remarks>

		[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

		public virtual bool IsResourceFeatureSupported( Resource resource, ResourceFeature resourceFeature )
		{
			ScheduleDataConnectorBase connector = this.DataConnector;
			return null != connector && connector.IsResourceFeatureSupported( resource, resourceFeature );
		} 

		#endregion // IsResourceFeatureSupported

		#region Remove

		/// <summary>
		/// Deletes an activity.
		/// </summary>
		/// <param name="activity">ActivityBase derived instance to delete.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		public virtual ActivityOperationResult Remove( ActivityBase activity )
		{
			CoreUtilities.ValidateNotNull( activity );

			DataErrorInfo errorInfo;
			ActivityOperationResult result;
			if ( !this.ValidateHasConnector( out errorInfo ) )
			{
				result = new ActivityOperationResult( activity, errorInfo, true );
			}
			else
			{
				ActivityRemovingEventArgs args = new ActivityRemovingEventArgs( activity );
				this.RaiseActivityRemoving( args );

				if ( args.Cancel )
				{
					result = new ActivityOperationResult( activity, true );
				}
				else
				{
					result = this.DataConnector.Remove( activity );

					// If the activity was an add-new activity and we raised ActivityAdding above then also
					// raise the corresponding ActivityAdded event.
					// 
					DataManagerActivityOperationCompletedHandler.RaiseEventHelper( this, result, ActivityOperation.Remove );
				}
			}

			// Manage activity's Error property. If there's an error, set it otherwise clear it.
			// 
			DataManagerActivityOperationCompletedHandler.ManageActivityErrorHelper( this, result, true );

			return result;
		}

		#endregion // Remove

		#region DisplayActivityDialog
		/// <summary>
		/// Displays a dialog that allows the user to add an <see cref="ActivityBase"/> derived activity (e.g., <see cref="Appointment"/>, <see cref="Journal"/> and <see cref="Task"/>).
		/// </summary>
		/// <param name="activityType">The type of see<see cref="ActivityBase"/> derived activity to display the dialog for.</param>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="calendar">The <see cref="ResourceCalendar"/> to which the <see cref="Appointment "/> should be added.</param>
		/// <param name="startTime">The preferred start time of the <see cref="Appointment "/> .</param>
		/// <param name="duration">The preferred duration of the <see cref="Appointment "/> </param>
		/// <param name="isTimeZoneNeutral">True if the activity's times are time-zone neutral (i.e., 'floating').  </param>
		/// <returns>True if a dialog was displayed, otherwise false.</returns>
		/// <seealso cref="ActivityType"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="Task"/>
		/// <seealso cref="ResourceCalendar"/>
		public bool DisplayActivityDialog(ActivityType activityType, FrameworkElement container, ResourceCalendar calendar, DateTime startTime, TimeSpan duration, bool isTimeZoneNeutral)
		{
			CoreUtilities.ValidateEnum(typeof(ActivityType), activityType);
			CoreUtilities.ValidateNotNull(container, "container");
			CoreUtilities.ValidateNotNull(calendar, "calendar");

			if (false == this.IsActivityOperationAllowed(activityType, ActivityOperation.Add, calendar))
				return false;

			DataErrorInfo	errorInfo;
			ActivityBase	activity = this.CreateNew(activityType, out errorInfo);

			Debug.Assert(activity != null, (errorInfo != null && errorInfo.Exception != null) ? errorInfo.Exception.Message : "Error creating activity!");
			if (activity == null)
				return false;

			activity.OwningCalendar		= calendar;
			activity.Start				= startTime;
			activity.End				= startTime + duration;
			activity.IsTimeZoneNeutral	= isTimeZoneNeutral;

			var token					= this.TimeZoneInfoProviderResolved.LocalTimeZoneIdResolved;
			activity.StartTimeZoneId	= token;
			activity.EndTimeZoneId		= token;

			// AS 6/8/11 TFS78425
			// The DisplayActivityDialog method being called will ultimately raise this event.
			//
			//ActivityDialogDisplayingEventArgs args	= new ActivityDialogDisplayingEventArgs(activity, true);
			//args.Cancel								= false;
			//this.OnActivityDialogDisplaying(args);
			//if (args.Cancel)
			//    return false;
			//else
			//    return this.DisplayActivityDialog(activity, container);
			return this.DisplayActivityDialog(activity, container);
		}

		/// <summary>
		/// Displays a dialog that allows the user to edit an <see cref="ActivityBase"/> derived activity (e.g., <see cref="Appointment"/>, <see cref="Journal"/> and <see cref="Task"/>).
		/// </summary>
		/// <param name="activity">The <see cref="ActivityBase"/> derived instance to display the dialog for.</param>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <returns>True if a dialog was displayed, otherwise false.</returns>
		/// <seealso cref="ActivityType"/>
		/// <seealso cref="ActivityBase"/>
		/// <seealso cref="Appointment"/>
		/// <seealso cref="Journal"/>
		/// <seealso cref="Task"/>
		public bool DisplayActivityDialog(ActivityBase activity, FrameworkElement container)
		{
			CoreUtilities.ValidateNotNull(activity, "activity");

			ActivityTypes activityType		= ScheduleUtilities.GetActivityTypes(activity.ActivityType);
			ActivityTypes supportedTypes	= ActivityTypes.Appointment | ActivityTypes.Journal | ActivityTypes.Task;

			var dialogFactory = this.DialogFactory;

			if (null != dialogFactory)
				supportedTypes |= dialogFactory.SupportedActivityDialogTypes;

			if ((activityType & supportedTypes) == 0)
				return false;

			// If the activity instance is an occurrence of a recurring activity, ask the user if they want to edit the occurrence or the series.
			if (activity.IsOccurrence)
			{
				// Display the RecurrenceChooserDialog to let the user decide whether we should open the occurrence or the Series.
				var userData = Tuple.Create(container, activity);
				var recurrenceChooserDialogResult = new ActivityRecurrenceChooserDialog.ChooserResult(userData);

				var closeHelper = new DialogManager.CloseDialogHelper<ActivityRecurrenceChooserDialog.ChooserResult>(this.OnRecurrenceChooserDialogClosed, recurrenceChooserDialogResult);

				bool result = this.DisplayActivityRecurrenceChooserDialog(container,
															ScheduleUtilities.GetString("DLG_RecurrenceChooserDialog_Title_Open"),
															activity,
															RecurrenceChooserType.ChooseForOpening,
															recurrenceChooserDialogResult,
															null,
															closeHelper.OnClosed);

				// Just return here - we will call DisplayActivityDialogPart2 in our OnRecurrenceChooserDialogClosed
				// callback and pass the appropriate activity instance based on the user's choice.
				return result;
			}

			return this.DisplayActivityDialogPart2(activity, container);
		}
		#endregion //DisplayActivityDialog

		#region DisplayActivityCategoryDialog
		/// <summary>
		/// Displays the <see cref="ActivityCategoryDialog"/> which lets the end user manage <see cref="ActivityCategory"/>s for the currently selected <see cref="ActivityBase"/> objects.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> instance.</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed</returns>
		/// <seealso cref="ActivityCategory"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public bool DisplayActivityCategoryDialog(FrameworkElement container, object header, ActivityCategoryHelper activityCategoryHelper, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			CoreUtilities.ValidateNotNull(activityCategoryHelper, "activityCategoryHelper");

			if (container == null)
				container = this._rootPanel;

			// Get the ActivityCategoryDialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityCategoryDialog(container, this, activityCategoryHelper, true);

			if (fe == null)
				fe = new ActivityCategoryDialog(activityCategoryHelper);

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.GetDialogResources() /* JM 03-30-11 TFS69614 */, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.GetDialogResources() , closingCallback, closedCallback, false);

			return true;
		}
		#endregion //DisplayActivityCategoryDialog

		#region DisplayActivityCategoryCreationDialog
		/// <summary>
		/// Displays the <see cref="ActivityCategoryCreationDialog"/> which lets the user create an <see cref="ActivityCategory"/>.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> instance.</param>
		/// <param name="creationResult">An instance of the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> class that holds the <see cref="ActivityCategory"/> created by the dialog and (optionally) a piece of user data.</param>
		/// <param name="updateOwningResource">True to automatically update the owning <see cref="Resource"/> with the new <see cref="ActivityCategory"/>.</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed</returns>
		public bool DisplayActivityCategoryCreationDialog(FrameworkElement container, object header, ActivityCategoryHelper activityCategoryHelper, ActivityCategoryCreationDialog.ChooserResult creationResult, bool updateOwningResource, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			// Get the TimeZone Dialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityCategoryCreationDialog(container, activityCategoryHelper, creationResult, updateOwningResource);

			if (fe == null)
				fe = new ActivityCategoryCreationDialog(activityCategoryHelper, creationResult, updateOwningResource);

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.GetDialogResources() /* JM 03-30-11 TFS69614 */, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.GetDialogResources() , closingCallback, closedCallback, false);

			return true;
		}
		#endregion //DisplayActivityCategoryCreationDialog

		#region DisplayActivityRecurrenceDialog
		/// <summary>
		/// Displays the Activity Recurrence dialog.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="activity">The activity to edit.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed; otherwise false is returned</returns>
		public bool DisplayActivityRecurrenceDialog(FrameworkElement container, ActivityBase activity, object header, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			return this.DisplayActivityRecurrenceDialog(container, activity, header,  closingCallback, closedCallback, this.IsRecurringActivityAllowed(activity));
		}

		internal bool DisplayActivityRecurrenceDialog( FrameworkElement container, ActivityBase activity, object header, Func<bool> closingCallback, Action<bool?> closedCallback, bool allowModifications )
		{
			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			// JM 02-18-11 TFS61928 - Fire the new ActivityRecurrenceDialogDisplaying event instead.
			// Fire the ActivityDialogDisplaying Event
			//ActivityDialogDisplayingEventArgs args	= new ActivityDialogDisplayingEventArgs(activity, allowModifications);
			//args.Cancel								= false;
			//this.OnActivityDialogDisplaying(args);
			ActivityRecurrenceDialogDisplayingEventArgs args = new ActivityRecurrenceDialogDisplayingEventArgs(activity, allowModifications);
			args.Cancel = false;
			this.OnActivityRecurrenceDialogDisplaying(args);
			if (args.Cancel)
				return false;
			else
				allowModifications = args.AllowModifications;

			// Get the Recurrence Dialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityRecurrenceDialog(container, this, activity, allowModifications);

			if (fe == null)
			{
				fe = new ActivityRecurrenceDialogCore(container, this, activity);
				((ActivityRecurrenceDialogCore)fe).IsActivityModifiable = allowModifications;
			}

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//// JM 04-12-11 TFS69396 Call the new overload that takes a resizeSilverlightWindowToFitInBrowser parameter
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.ColorSchemeResolved.DialogResources, closingCallback, closedCallback, true);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.ColorSchemeResolved.DialogResources, closingCallback, closedCallback, true);

			return dialogElement != null;
		}
		#endregion //DisplayActivityRecurrenceDialog

		#region DisplayActivityRecurrenceChooserDialog
		/// <summary>
		/// Displays the <see cref="ActivityRecurrenceChooserDialog"/> which lets the end user choose a Recurrence Series or Occurrence.  A parameter can be passed to the dialog
		/// that specifies whether the choice applies to the opening or deletion of the Series/Occurrence.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="activity">The recurring <see cref="ActivityBase"/> for which the choice is being made.</param>
		/// <param name="chooserType">The type of <see cref="ActivityRecurrenceChooserDialog"/> to display, i.e. a chooser for opening recurrences or deleting recurrences.</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed</returns>
		public bool DisplayActivityRecurrenceChooserDialog(FrameworkElement container, object header, ActivityBase activity, RecurrenceChooserType chooserType, ActivityRecurrenceChooserDialog.ChooserResult chooserResult, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			CoreUtilities.ValidateNotNull(activity, "activity");
			CoreUtilities.ValidateNotNull(chooserResult, "chooserResult");

			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			var args = new ActivityRecurrenceChooserDialogDisplayingEventArgs(activity, chooserType);

			this.OnActivityRecurrenceChooserDialogDisplaying(args);

			switch (args.ChooserAction)
			{
				case RecurrenceChooserAction.Cancel:
					chooserResult.Choice = RecurrenceChooserChoice.None;
					return false;

				case RecurrenceChooserAction.Occurrence:
				case RecurrenceChooserAction.Series:
					chooserResult.Choice = args.ChooserAction == RecurrenceChooserAction.Occurrence 
						? RecurrenceChooserChoice.Occurrence 
						: RecurrenceChooserChoice.Series;

					if (null != closedCallback)
						closedCallback(true);

					return true;
			}

			// Get the Recurrence Dialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityRecurrenceChooserDialog(container, this, activity, chooserType, chooserResult);

			if (fe == null)
				fe = new ActivityRecurrenceChooserDialog(this, activity, chooserType, chooserResult);

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.GetDialogResources() /* JM 03-30-11 TFS69614*/, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.GetDialogResources() , closingCallback, closedCallback, false);

			return true;
		}
		#endregion //DisplayActivityRecurrenceChooserDialog

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		#region DisplayActivityTypeChooserDialog
		/// <summary>
		/// Displays the <see cref="ActivityTypeChooserDialog"/> which lets the end user choose a Recurrence Series or Occurrence.  A parameter can be passed to the dialog
		/// that specifies whether the choice applies to the opening or deletion of the Series/Occurrence.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="availableTypes">The set of available activity types from which to choose</param>
		/// <param name="chooserReason">The reason for which the dialog is being displayed</param>
		/// <param name="calendar">The calendar associated with the action or null if there is no calendar associated with the action</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed</returns>
		public bool DisplayActivityTypeChooserDialog(FrameworkElement container, object header, ActivityTypes availableTypes, ActivityTypeChooserReason chooserReason, ResourceCalendar calendar, ActivityTypeChooserDialog.ChooserResult chooserResult, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			CoreUtilities.ValidateNotNull(chooserResult, "chooserResult");

			var args = new ActivityTypeChooserDialogDisplayingEventArgs(availableTypes, calendar, chooserReason);
			this.OnActivityTypeChooserDialogDisplaying(args);

			if (args.Cancel)
				return false;

			if (args.ActivityType != null)
			{
				chooserResult.Choice = args.ActivityType.Value;

				if (null != closedCallback)
					closedCallback(true);

				return true;
			}

			CoreUtilities.ValidateNotNull(chooserResult, "chooserResult");

			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			// Get the Recurrence Dialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityTypeChooserDialog(container, this, availableTypes, chooserReason, calendar, chooserResult);

			if (fe == null)
				fe = new ActivityTypeChooserDialog(this, availableTypes, chooserReason, chooserResult);

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.GetDialogResources() /* JM 03-30-11 TFS69614*/, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.GetDialogResources() , closingCallback, closedCallback, false);

			return true;
		}
		#endregion //DisplayActivityTypeChooserDialog

		#region DisplayReminderDialog
		/// <summary>
		/// Displays the <see cref="ReminderDialog"/> which lets the end user dismiss or snooze active Reminders.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed (or activated if the window was still displayed); otherwise false if the dialog was not displayed.</returns>
		/// <seealso cref="Reminder"/>
		/// <seealso cref="ReminderDialog"/>
		public bool DisplayReminderDialog(FrameworkElement container, object header, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			var args = new ReminderDialogDisplayingEventArgs();
			this.OnReminderDialogDisplaying(args);

			if (args.Cancel)
				return false;

			FrameworkElement previousDialog = _reminderDialog == null ? null : CoreUtilities.GetWeakReferenceTargetSafe(_reminderDialog) as FrameworkElement;
			if (null != previousDialog)
			{
				DialogManager.ActivateDialog(previousDialog);
				return true;
			}

			// Get the ReminderDialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateReminderDialog(container, this);

			if (fe == null)
				fe = new ReminderDialog(this);

			// we always want to show a single reminder dialog so we need to know when the 
			// dialog is closed. however if they provide a closedcallback, we'll wrap it and 
			// call it as well as our closed handler
			if ( closedCallback == null )
				closedCallback = this.OnReminderDialogClosed;
			else
				closedCallback = new DialogManager.CloseDialogHelper<Action<bool?>>(this.OnReminderDialogClosed, closedCallback).OnClosed;

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, false, this.GetDialogResources() /* JM 03-30-11 TFS69614*/, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, false, this.GetDialogResources() , closingCallback, closedCallback, false);

			if (dialogElement == null)
				_reminderDialog = null;
			else
				_reminderDialog = new WeakReference(dialogElement);

			return dialogElement != null;
		}
		#endregion //DisplayReminderDialog

		#region DisplayTimeZoneChooserDialog
		/// <summary>
		/// Displays the <see cref="TimeZoneChooserDialog"/> which lets the end user choose a TimeZone Series or Occurrence.  A parameter can be passed to the dialog
		/// that specifies whether the choice applies to the opening or deletion of the Series/Occurrence.
		/// </summary>
		/// <param name="container">The FrameworkElement that will contain the dialog.</param>
		/// <param name="header">The header (caption) of the dialog</param>
		/// <param name="chooserResult">A reference to a <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult"/> instance.  The dialog will set the <see cref="ScheduleDialogBase&lt;TChoice&gt;.ChooserResult.Choice"/> property when the dialog closes to reflect the user's choice.</param>
		/// <param name="closingCallback">A callback to call when when the window is closing, since the call to show the dialog may not be blocking.</param>
		/// <param name="closedCallback">A callback to call when when the window has closed, since the call to show the dialog may not be blocking.</param>
		/// <returns>Returns true if the dialog was displayed</returns>
		public bool DisplayTimeZoneChooserDialog(FrameworkElement container, object header, TimeZoneChooserDialog.ChooserResult chooserResult, Func<bool> closingCallback, Action<bool?> closedCallback)
		{
			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			// Get the TimeZone Dialog contents.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateTimeZoneChooserDialog(container, this, this.TimeZoneInfoProviderResolved, chooserResult);

			if (fe == null)
				fe = new TimeZoneChooserDialog(this, this.TimeZoneInfoProviderResolved, chooserResult);

			// JM 01-06-12 TFS95261 - Moved the following call to a new helper method that will display the dialog AND maintain 
			//						  the list of ActiveSubDialogs
			//FrameworkElement dialogElement = DialogManager.DisplayDialog(container, fe, Size.Empty, false, header, true, this.GetDialogResources() /* JM 03-30-11 TFS69614*/, closingCallback, closedCallback);
			FrameworkElement dialogElement = this.DisplaySubDialogHelper(container, fe, Size.Empty, false, header, true, this.GetDialogResources() , closingCallback, closedCallback, false);

			return true;
		}
		#endregion //DisplayTimeZoneChooserDialog

		#region GetActivities

		/// <summary>
		/// Returns activities matching the criteria specified by the query parameter.
		/// </summary>
		/// <param name="query">Contains information regarding which activties to get.</param>
		/// <returns><see cref="ActivityQueryResult"/> instance.</returns>
		public ActivityQueryResult GetActivities( ActivityQuery query )
		{
			ScheduleDataConnectorBase connector = this.DataConnector;
			ActivityQueryResult result = null != connector ? connector.GetActivities( query ) : null;

			// RaiseErrorHelper checks for nulls.
			// 
			ScheduleUtilities.RaiseErrorHelper( this, result );

			return result;
		}

		#endregion // GetActivities

		#region GetResolvedDefaultWorkingHours

		/// <summary>
		/// Gets the default working hours.
		/// </summary>
		/// <returns>A read-only WorkingHoursCollection instance.</returns>
		/// <seealso cref="ScheduleSettings.WorkingHours"/>
		/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
		public WorkingHoursCollection GetResolvedDefaultWorkingHours( )
		{
			WorkingHoursCollection workingHours = null != _cachedSettings ? _cachedSettings.WorkingHours : null;

			if ( null == workingHours || 0 == workingHours.Count )
				workingHours = WorkingHoursCollection.DefaultWorkingHours;

			return workingHours;
		}

		#endregion // GetResolvedDefaultWorkingHours

		#region GetSupportedActivityCategories

		// SSP 12/9/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Gets the list of activity categories that are supported for the specified activity. Default implementation returns <i>null</i>.
		/// </summary>
		/// <param name="activity">Activity for which to get the list of supported categories.</param>
		/// <returns>IEnumerable that can optionally implement INotifyCollectionChanged to notify
		/// of changes to the list.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used to retrieve the list of activity categories that are supported by the specified activity.
		/// It's used by the activity dialogs to display the list of applicable categories from which the user can select
		/// one or more categories.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResolveActivityCategories"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public virtual IEnumerable<ActivityCategory> GetSupportedActivityCategories( ActivityBase activity )
		{
			ScheduleDataConnectorBase dc = this.DataConnector;
			if ( null != dc )
				return dc.GetSupportedActivityCategories( activity );

			return null;
		}

		#endregion // GetSupportedActivityCategories

		#region GetWorkingHours

		/// <summary>
		/// Gets the working hours for the specified resource on the day specified by the 'date' parameter.
		/// </summary>
		/// <param name="resource">Resource whose working hours to get.</param>
		/// <param name="date">Day for which to get the working hours.</param>
		/// <returns>WorkingHoursCollection instance that contains the working hours or null if the day is not a work-day.</returns>
		/// <seealso cref="ScheduleSettings.WorkingHours"/>
		/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
		public WorkingHoursCollection GetWorkingHours( Resource resource, DateTime date )
		{
			bool isWorkDay;
			return this.GetWorkingHours( resource, date, out isWorkDay );
		}
		
		#endregion // GetWorkingHours

		#region IsWorkDayResolved

		/// <summary>
		/// Gets a value indicating whether the specified day of week is a work-day.
		/// </summary>
		/// <param name="resource">Resource whose information to get.</param>
		/// <param name="dayOfWeek">Day of week.</param>
		/// <returns>True if the day of week is a work-day, false otherwise.</returns>
		/// <seealso cref="ScheduleSettings.WorkingHours"/>
		/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
		/// <seealso cref="IsWorkDayResolved(Resource, DateTime)"/>
		public bool IsWorkDayResolved( Resource resource, DayOfWeek dayOfWeek )
		{
			bool isWorkDay;
			this.GetWorkingHoursHelper( resource, dayOfWeek, null, out isWorkDay );
			
			return isWorkDay;
		}

		/// <summary>
		/// Gets a value indicating whether the day specified by the 'date' parameter is a work-day.
		/// </summary>
		/// <param name="resource">Resource whose information to get.</param>
		/// <param name="date">Day for which to get the information.</param>
		/// <returns>True if the day is a work-day, false otherwise.</returns>
		/// <seealso cref="ScheduleSettings.WorkingHours"/>
		/// <seealso cref="ScheduleSettings.DaysOfWeek"/>
		/// <seealso cref="IsWorkDayResolved(Resource, DayOfWeek)"/>
		public bool IsWorkDayResolved( Resource resource, DateTime date )
		{
			
			bool isWorkDay;
			this.GetWorkingHours( resource, date, out isWorkDay );

			return isWorkDay;
		}

		#endregion // IsWorkDayResolved

		#region RefreshDisplay

		/// <summary>
		/// Calls RefreshDisplay on all associated controls
		/// </summary>
		public virtual void RefreshDisplay()
		{
			foreach (IScheduleControl control in this._controls)
				control.RefreshDisplay();
		}

		#endregion //RefreshDisplay	

		#region ResolveActivityCategories

		// SSP 12/9/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Gets the activity categories that are currently applied to the specified activity.
		/// </summary>
		/// <param name="activity">Activity whose currently applied categories to return.</param>
		/// <returns>A collection of <see cref="ActivityCategory"/> objects. <b>Note</b> that the
		/// returned collection will support change notifications to notify when the activity's
		/// categories are changed (<see cref="ActivityBase.Categories"/>) or when the 
		/// underlying activity categories are removed or added.</returns>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public virtual IEnumerable<ActivityCategory> ResolveActivityCategories( ActivityBase activity )
		{
			ScheduleDataConnectorBase dc = this.DataConnector;
			if ( null != dc )
				return dc.ResolveActivityCategories( activity );

			return null;
		}

		#endregion // ResolveActivityCategories

		#region ValidateActivity
		
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)

		#endregion // ValidateActivity

		#endregion // Public Methods

		#region Internal Methods

		#region AddExtraActivity
		internal void AddExtraActivity(ActivityBase activity)
		{
			Debug.Assert(activity.IsAddNew, "This should only be used (at least for now) with addnew activities that haven't been committed");
			Debug.Assert(activity.PendingOperation != null, "This activity doesn't have a pending operation but is an addnew? Has the edit been ended?");
			Debug.Assert(!activity.IsInEdit, "The activity is still in edit mode?");
			Debug.Assert(activity.PendingOperation == null || activity.PendingOperation.Error == null, "There's an error on the activity. Should we be storing it or was it not committed?");

			foreach (var item in this.Controls)
			{
				ScheduleControlBase ctrl = item as ScheduleControlBase;

				if (null == ctrl)
					continue;

				if (ctrl.ExtraActivities.Contains(activity))
					continue;

				ctrl.ExtraActivities.Add(activity);
			}
		}
		#endregion //AddExtraActivity

		#region AddPendingOperation
		internal void AddPendingOperation(OperationResult result)
		{
			if (null == result || result.IsComplete)
				return;

			this.PendingOperationsSource.Add(result);
		} 
		#endregion // AddPendingOperation

		#region ApplyLogicalDayOffset


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void ApplyLogicalDayOffset(ref DateTime date)
		{
			TimeSpan logicalDayOffset = this.LogicalDayOffset;
			double totalMinutesAbsolute = Math.Abs(logicalDayOffset.TotalMinutes);

			// We should not ignore a value of 1, just values less then
			// 1 - otherwise you get incorrect results with an offset of
			// 1 or -1 minutes.
			if (totalMinutesAbsolute < 1.0f)
				return;

			DateTime? adjustedDate = this.DateInfoProviderResolved.Add(date, logicalDayOffset);

			if ( adjustedDate.HasValue)
				date = adjustedDate.Value;
		}
		#endregion ApplyLogicalDayOffset

		#region BumpBrushVersion

		internal void BumpBrushVersion()
		{
			ScheduleControlBase.SetBrushVersion(this, ScheduleControlBase.GetBrushVersion(this) + 1);
		}

		#endregion //BumpBrushVersion

		#region CloseDialog
		internal void CloseDialog(object dialogKey)
		{
			if (dialogKey == null)
				return;

			FrameworkElement dialog;

			// AS 10/27/10 TFS36504
			// Refactored the close into a helper method.
			//
			// JM 01-06-12 TFS95261 Change the ActiveDialogMap references to be non-static.
			if (this.ActiveDialogMap.TryGetValue(dialogKey, out dialog))
			{
				this.ActiveDialogMap.Remove(dialogKey);
				DialogManager.CloseDialog(dialog);
			}
		}
		#endregion //CloseDialog

		#region CloseAllDialogs
		internal void CloseAllDialogs()
		{
			// JM 01-06-12 TFS95261 - Close all sub dialogs first.
			this._isClosingAllDialogs = true;





			try
			{
				foreach (SubDialogCloseHelper sdch in this.ActiveSubDialogs.Reverse())
				{
					if (sdch.Dialog != null)
						DialogManager.CloseDialog(sdch.Dialog);
				}
				this.ActiveSubDialogs.Clear();


				// JM 01-06-12 TFS95261 Change the ActiveDialogMap references to be non-static.
				List<object> keys = new List<object>(this.ActiveDialogMap.Count);
				foreach (KeyValuePair<object, FrameworkElement> entry in this.ActiveDialogMap)
					keys.Add(entry.Key);

				foreach (object key in keys)
					this.CloseDialog(key);
			}
			finally
			{
				this._isClosingAllDialogs = false;
			}
		}
		#endregion //CloseAllDialogs

		#region CommitActivityHelper

		/// <summary>
		/// Calls BeginEdit and EndEdit on the activity to commit any changes that may have been
		/// made to the activity.
		/// </summary>
		/// <param name="activity">Activity to commit.</param>
		/// <remarks>
		/// This is necessary because a data connector may rely on EndEdit as a trigger to update
		/// the underlying backend. If that's the case then simply changing an activity's property
		/// will not cause the connector to commit it. To put less responsibility on connector 
		/// implementations, we'll go through BeginEdit and EndEdit process when we make a change
		/// to an activity. Furthermore, any errors in the process can be handled by the UI.
		/// </remarks>
		internal void CommitActivityHelper( ActivityBase activity )
		{
			DataErrorInfo error;
			if ( this.BeginEdit( activity, out error ) )
			{
				ActivityOperationResult result = this.EndEdit( activity );
				if ( !result.IsComplete )
					this.AddPendingOperation( result );
			}
		} 

		#endregion // CommitActivityHelper

		#region DisplayErrorMessageBox
		internal void DisplayErrorMessageBox( DataErrorInfo error, string title )
		{
			if (error == null)
				return;

			var args = new ErrorDisplayingEventArgs(error, ScheduleErrorDisplayType.MessageBox);

			this.OnErrorDisplaying(args);

			if (args.Cancel)
				return;

			MessageBox.Show(
				error.UserErrorText
				, title
				, MessageBoxButton.OK

				, MessageBoxImage.Exclamation

			);
		}
		#endregion // DisplayErrorMessageBox

		// JJD 10/12/11 - TFS89043 - added
		#region GetLogicalDayRange

		// JJD 10/12/11 - TFS89043 
		// Moved logic for getting the logical day range into a helper method so it could be 
		// called by XamDateNavigator's GetTodayRange method
		internal DateRange? GetLogicalDayRange()
		{
			TimeSpan logOffset = this.LogicalDayOffset;

			DateInfoProvider dateProvider = this.DateInfoProviderResolved;

			DateTime? start = dateProvider.Add(this.CurrentDate, logOffset);

			if (null != start)
			{
				var tzProvider = this.TimeZoneInfoProviderResolved;

				// JJD 10/22/10 - TFS57632, TFS57534 and TFS57635
				// Verify the we have both a local and utc token
				//if (tzProvider.LocalToken != null )
				if (tzProvider.LocalToken != null && tzProvider.UtcToken != null)
				{
					start = ScheduleUtilities.ConvertToUtc(tzProvider.LocalToken, start.Value);
					DateTime? end = dateProvider.AddDays(start.Value, 1);

					if (end != null)
						return new DateRange(start.Value, end.Value);
				}
			}

			return null;
		}

		#endregion //GetLogicalDayRange
    
		// JM 11-1-10 TFS 58839 - Added.
		#region EnsureOwningCalendarInitialized
		internal void EnsureOwningCalendarInitialized(ActivityBase activity)
		{
			ListScheduleDataConnector dc = this.DataConnector as ListScheduleDataConnector;
			if ( null != dc && ( null == activity.OwningResource || null == activity.OwningCalendar ) )
			{
				Debug.Assert( !activity.IsEditCopy );
				dc.InitializeOwningCalendar( activity );
			}
		}
		#endregion //EnsureOwningCalendarInitialized

		#region IsActivityDraggingAllowed

		/// <summary>
		/// Gets a value indicating if activity dragging is supported and if so whether it can be dragged to a different calendar of the same resource
		/// or a calendar of a different user.
		/// </summary>
		/// <param name="activity">Activity that is to be dragged.</param>
		/// <param name="targetCalendar">The calendar to which the activity is being dragged or null when the drag is about to be invoked</param>
		/// <param name="copy">Determines if the drag operation will be a copy or a move</param>
		/// <returns>A boolean indicating if the drag operation is allowed.</returns>
		internal bool IsActivityDraggingAllowed(ActivityBase activity, ResourceCalendar targetCalendar, bool copy)
		{
			if (null == this.DataConnector)
				return false;

			ActivityType activityType = activity.ActivityType;
			ResourceCalendar calendar = activity.OwningCalendar;
			ActivitySettings settings = this.GetActivitySettings(activityType);

			const AllowActivityDragging DefaultAllowDragging = AllowActivityDragging.AcrossResources;
			AllowActivityDragging allowDrag = (null != settings ? settings.AllowDragging : null) ?? DefaultAllowDragging;

			// if dragging is not allowed...
			if (allowDrag == AllowActivityDragging.No || calendar == null)
				return false;

			#region TargetCalendar == null (i.e. starting drag)
			// if this is testing for capabilities to know if we should start a drag...
			if (targetCalendar == null)
			{
				if (copy)
				{
					// technically moving an activity to another calendar may be acheived by 
					// an edit operation assuming the connector supports it. however we want 
					// to be consistent so we will not allow a copy if addnew is not allowed 
					if (null != settings && settings.AllowAdd == false)
						return false;

					// we checked AllowAdd above. we can't know whether another calendar 
					// in the datamanager supports an add without querying every resource's
					// calendars (or at least those used by the control using this datamanager)
					// so we have to assume that the drag should be started and then we'll
					// evaluate each drop target as the end user goes over it
					return true;
				}
				else // move
				{
					// if we can edit it then we can change its start and/or end so we can
					// allow the drag to begin
					if (this.IsActivityOperationAllowed(activity, ActivityOperation.Edit))
						return true;

					if (!activity.IsOccurrence)
					{
						// if the source activity may be removed and new activity in general is not 
						// disallowed then we can allow a copy to be created to simulate the move 
						// so allow the drag
						if (null != settings && settings.AllowAdd != false && this.IsActivityOperationAllowed(activity, ActivityOperation.Remove))
							return true;
					}

					return false;
				}
			} 
			#endregion // TargetCalendar == null (i.e. starting drag)

			if (copy)
			{
				// if we are creating a copy then the target calendar must support add to allow creation of the copy
				// AS 11/1/10 TFS58843
				//if (!connector.IsActivityOperationSupported(activityType, ActivityOperation.Add, targetCalendar))
				if (!this.IsActivityOperationAllowed(activityType, ActivityOperation.Add, targetCalendar))
					return false;

				
			}
			else // move
			{
				// moving within the source calendar so we just need to be able to manipulate the start/end
				if (!this.IsActivityOperationAllowed(activity, ActivityOperation.Edit))
					return false;

				if (targetCalendar != calendar)
				{
					if (allowDrag == AllowActivityDragging.WithinCalendar)
						return false;

					// like the copy operation, moving an activity to another calendar may 
					// be acheived by an edit operation assuming the connector supports it. 
					// however we want to be consistent regardless of the means by which 
					// we have to implement the operation for the connector being used so 
					// don't allow a move to another calendar if add or remove is not supported. 
					if (null != settings && (settings.AllowRemove == false || settings.AllowAdd == false))
						return false;

					// occurrences cannot be moved across calendars - only copied
					if (activity.IsOccurrence)
						return false;

					// AS 11/1/10 TFS58851
					// While we don't need to check the IsActivityFeatureSupported, we do still need 
					// to check the allow drag state if dragging to another resource.
					//
					if ( calendar.OwningResource != targetCalendar.OwningResource )
					{
						if ( allowDrag == AllowActivityDragging.WithinResource )
							return false;
					}

					
#region Infragistics Source Cleanup (Region)




























#endregion // Infragistics Source Cleanup (Region)

					{
						if (!this.IsActivityOperationAllowed(activityType, ActivityOperation.Add, targetCalendar))
							return false;

						if (!this.IsActivityOperationAllowed(activity, ActivityOperation.Remove))
							return false;
					}
				}
			}

			return true;
		}

		#endregion // IsActivityDraggingAllowed

		#region IsActivityOperationAllowedHelper
		/// <summary>
		/// Helper method used by the controls to determine if operations are allowed.
		/// </summary>
		/// <param name="activity">Activity for which the operation will be performed</param>
		/// <param name="operation">The operation to query</param>
		/// <param name="checkPendingOperation">True to consider the PendingOperation</param>
		/// <returns></returns>
		internal bool IsActivityOperationAllowedHelper( ActivityBase activity, ActivityOperation operation, bool checkPendingOperation = true )
		{
			if ( activity == null )
				return false;

			if ( checkPendingOperation )
			{
				if ( activity.PendingOperation != null )
					return false;

				var root = activity.RootActivity;

				if ( null != root && root.PendingOperation != null )
					return false;
			}

			if (this.BlockingError != null)
				return false;

			return this.IsActivityOperationAllowed(activity, operation);
		}

		internal bool IsActivityOperationAllowedHelper( ActivityType activityType, ActivityOperation operation, ResourceCalendar calendar )
		{
			if (this.BlockingError != null)
				return false;

			return this.IsActivityOperationAllowed( activityType, operation, calendar );
		}

		// AS 3/1/11 NA 2011.1 ActivityTypeChooser
		internal bool IsActivityOperationAllowedHelper(ActivityType activityType, ActivityCreationTrigger addTrigger, ResourceCalendar calendar )
		{
			if (!this.IsActivityOperationAllowedHelper(activityType, ActivityOperation.Add, calendar))
				return false;

			var settings = this.GetActivitySettings(activityType);
			bool? isAddEnabled = null;
			
			if (null != settings)
			{
				switch(addTrigger)
				{
					case ActivityCreationTrigger.DoubleClick:
						isAddEnabled = settings.IsAddViaDoubleClickEnabled;
						break;
					case ActivityCreationTrigger.Typing:
						isAddEnabled = settings.IsAddViaTypingEnabled;
						break;
					case ActivityCreationTrigger.ClickToAdd:
						isAddEnabled = settings.IsAddViaClickToAddEnabled;
						break;
				}
			}

			// by default only support these operations for appointment
			if (isAddEnabled == null)
				isAddEnabled = activityType == ActivityType.Appointment;

			return isAddEnabled.Value;
		}

		#endregion // IsActivityOperationAllowedHelper

		#region IsActivityReminderAllowed

		/// <summary>
		/// Indicates whether the user can specify activity reminders.
		/// </summary>
		/// <param name="activity">Activity object to check.</param>
		/// <returns>True if the reminder is allowed. False otherwise.</returns>
		internal bool IsActivityReminderAllowed( ActivityBase activity )
		{
			return this.IsActivityFeatureSupported( activity, ActivityFeature.Reminder );
		}

		#endregion // IsActivityReminderAllowed

		#region IsActivityResizeAllowed
		/// <summary>
		/// Indicates whether the end user may resize an activity within the ui
		/// </summary>
		/// <param name="activity">Activity to evaluate</param>
		/// <param name="start">True for the leading edge, otherwise false for the trailing edge</param>
		/// <returns></returns>
		internal bool IsActivityResizeAllowed(ActivityBase activity, bool start)
		{
			ActivityType activityType = activity.ActivityType;
			ActivitySettings settings = this.GetActivitySettings(activityType);

			AllowActivityResizing allowResize = (null != settings ? settings.AllowResizing : null) ?? AllowActivityResizing.StartAndEnd;

			if (allowResize == AllowActivityResizing.No)
				return false;

			if (!this.IsActivityOperationAllowedHelper(activity, ActivityOperation.Edit))
				return false;

			if (allowResize == AllowActivityResizing.Start && !start)
				return false;

			if (allowResize == AllowActivityResizing.End && start)
				return false;

			// can only resize if the activity's duration can be changed which means the end must be modifiable
			if (!this.IsEndDateSupportedByActivity(activity))
				return false;

			return true;
		} 
		#endregion // IsActivityResizeAllowed

		#region IsEndDateSupportedByActivity (static)
		internal bool IsEndDateSupportedByActivity(ActivityBase activity)
		{
			return this.IsActivityFeatureSupported( activity, ActivityFeature.EndTime );
		}
		#endregion //IsEndDateSupportedByActivity

		#region IsRecurringActivityAllowed

		/// <summary>
		/// Gets a resolved value indicating whether the user allowed to create recurring activities.
		/// </summary>
		/// <param name="activity">Activity object to check if it can be made recurring.</param>
		/// <returns>True if recurrence is allowed. False otherwise.</returns>
		internal bool IsRecurringActivityAllowed( ActivityBase activity )
		{
			return this.IsRecurringActivityAllowed( activity.ActivityType, activity.OwningCalendar );
		}

		/// <summary>
		/// Gets a resolved value indicating whether the user allowed to create recurring activities.
		/// </summary>
		/// <param name="activityType">Activity type - one of appointment, journal or task.</param>
		/// <param name="calendar">Calendar associated with the activity in question.</param>
		/// <returns>True if the recurring activity is allowed. False otherwise.</returns>
		internal bool IsRecurringActivityAllowed( ActivityType activityType, ResourceCalendar calendar )
		{
			ActivitySettings s = this.GetActivitySettings( activityType );

			if ( null == s || ( s.AllowRecurring ?? true ) )
			{
				if ( this.IsActivityFeatureSupported( activityType, ActivityFeature.Recurrence, calendar ) )
					return true;
			}

			return false;
		}

		#endregion // IsRecurringActivityAllowed

		#region IsTimeZoneNeutralActivityAllowed

		/// <summary>
		/// Gets a resolved value indicating whether the user allowed to create time-zone neutral activities.
		/// </summary>
		/// <param name="activity">Activity object to check if it can be made time-zone neutral.</param>
		/// <returns>True if the time-zone neutral activity is allowed. False otherwise.</returns>
		internal bool IsTimeZoneNeutralActivityAllowed( ActivityBase activity )
		{
			return this.IsTimeZoneNeutralActivityAllowed( activity.ActivityType, activity.OwningCalendar );
		}

		/// <summary>
		/// Gets a resolved value indicating whether the user allowed to create time-zone neutral activities.
		/// </summary>
		/// <param name="activityType">Activity type - one of appointment, journal or task.</param>
		/// <param name="calendar">Calendar associated with the activity in question.</param>
		/// <returns>True if the time-zone neutral activity is allowed. False otherwise.</returns>
		internal bool IsTimeZoneNeutralActivityAllowed( ActivityType activityType, ResourceCalendar calendar )
		{
			ActivitySettings s = this.GetActivitySettings( activityType );

			if ( null == s || ( s.AllowTimeZoneNeutral ?? true ) )
			{
				ScheduleDataConnectorBase connector = this.DataConnector;
				if ( null != connector && connector.IsActivityFeatureSupported( activityType, ActivityFeature.TimeZoneNeutrality, calendar ) )
					return true;
			}

			return false;
		}

		#endregion // IsTimeZoneNeutralActivityAllowed

		#region IsVarianceActivityAllowed

		/// <summary>
		/// Gets a resolved value indicating whether the user allowed to create variance of the specified occurrence.
		/// </summary>
		/// <param name="activity">An activity that is an occurrence of a recurring activity.</param>
		/// <returns>True if making the activity a variance is allowed. False otherwise.</returns>
		internal bool IsVarianceActivityAllowed( ActivityBase activity )
		{
			return this.IsActivityFeatureSupported( activity, ActivityFeature.Variance );
		}

		#endregion // IsVarianceActivityAllowed

		#region GetFirstOccurrenceStartTime

		/// <summary>
		/// Returns the date-time of the first occurrence of the recurrence that occurs at or after the specified 'start' value.
		/// </summary>
		/// <param name="activity">Activity object. Provides the context of the time-zones.</param>
		/// <param name="start">The date-time of the first occurrence that occurs at or after this date-time will be returned by this method.</param>
		/// <param name="recurrence">Object describing the recurrence rules.</param>
		/// <returns>The date-time of the first occurrence at or after the 'start' parameter is returned. If
		/// the rules described by the specified recurrence do not have an occurrence at or after the 'start' time 
		/// null is returned.</returns>
		internal DateTime? GetFirstOccurrenceStartTime( ActivityBase activity, DateTime start, RecurrenceBase recurrence )
		{
			var calculator = ScheduleUtilities.GetRecurrenceCalculatorHelper( this, activity, recurrence, start );
			if ( null != calculator )
			{
				DateTime? firstOccurrenceDate = calculator.FirstOccurrenceDate;
				Debug.Assert( firstOccurrenceDate.HasValue );
				if ( firstOccurrenceDate.HasValue )
					return firstOccurrenceDate.Value;
			}

			return null;
		}

		#endregion // GetFirstOccurrenceStartTime

		#region HasOverlappingOccurrences

		// SSP 4/14/11
		// 
		/// <summary>
		/// Returns a value indicating if the specified recurrence rules will result in overlapping occurrences.
		/// </summary>
		/// <param name="activity">Activity instance used to get <see cref="ActivityBase.Start"/> of the series as well as the 
		/// duration of each occurrence based on its <see cref="ActivityBase.End"/> property value.</param>
		/// <param name="recurrence">Recurrence rules.</param>
		/// <returns>True if two or more occurrences overlap. False otherwise.</returns>
		internal bool HasOverlappingOccurrences( ActivityBase activity, DateRecurrence recurrence )
		{
			var calculator = ScheduleUtilities.GetRecurrenceCalculatorHelper( this, activity, recurrence );
			if ( null != calculator )
			{
				DateTime start = activity.Start;
				DateRange range = new DateRange( start, start.AddDays( 2 * 366 ) );

				return ScheduleUtilities.HasOverlappingInstances( calculator.GetOccurrences( range ) );
			}

			return false;
		} 

		#endregion // HasOverlappingOccurrences

		#region OnReminderAdded

		internal void OnReminderAdded( ReminderInfo reminderInfo )
		{
			// If the reminder is being displayed for an activity, then the context will be that activity.
			// 
			ActivityBase activity = reminderInfo.Context as ActivityBase;

			if ( null != activity )
				Debug.WriteLine( "Reminder for activity " + activity.Id + " " + activity.Subject );
			else
				Debug.WriteLine( "Reminder without activity context." );

			// raise the reminder activated to allow the developer to handle the reminder in 
			// case there is an action, etc. associated with the reminder
			ReminderActivatedEventArgs args = new ReminderActivatedEventArgs(reminderInfo);

			this.OnReminderActivated( args );

			// if the reminder is dismissed or is no longer active then do not try to show the reminder dialog
			if (reminderInfo.IsDismissed || reminderInfo.IsSnoozed)
			{
				return;
			}

			// Display the reminder dialog if it is not already displayed.
			this.DisplayReminderDialog(this, ScheduleUtilities.GetString("DLG_Reminder_Title"), null, null);
		}

		#endregion // OnReminderAdded

		#region PopulateListWithAppointmentStartTimes


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal void PopulateListWithAppointmentStartTimes(ObservableCollection<TimePickerItem> startTimes, int month, int day, int year)
		{
			DateInfoProvider	dateProvider		= this.DateInfoProviderResolved;
			DateTime			firstDateTime		= new DateTime(year, month, day, 0, 0, 0);
			DateRange			logicalDayRange1	= ScheduleUtilities.GetLogicalDayRange(firstDateTime, this.LogicalDayOffset, this.LogicalDayDuration);

			DateTime?			lastDateTime		= dateProvider.AddDays(firstDateTime, 1);
			if (lastDateTime == null)
				lastDateTime = dateProvider.MaxSupportedDateTime;
			DateRange			logicalDayRange2	= ScheduleUtilities.GetLogicalDayRange(lastDateTime.Value, this.LogicalDayOffset, this.LogicalDayDuration);

			DateTime			nextDateToProcess	= firstDateTime;

			for (int i = 0; i < 48; i++)
			{
				if (logicalDayRange1.Contains(nextDateToProcess) ||
					logicalDayRange2.Contains(nextDateToProcess))
				{
					startTimes.Add(new TimePickerItem(nextDateToProcess));
				}

				DateTime? adjustedDate = dateProvider.AddMinutes(nextDateToProcess, 30);
				if (adjustedDate == null)
					break;

				nextDateToProcess = adjustedDate.Value;
			}
		}
		#endregion PopulateListWithAppointmentStartTimes

		#region PopulateListWithAppointmentEndTimes


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		internal void PopulateListWithAppointmentEndTimes(ObservableCollection<TimePickerItem> endTimes, DateTime appointmentStart, DateTime firstDateTime, bool includeDuration)
		{
			DateInfoProvider	dateProvider		= this.DateInfoProviderResolved;
			DateRange			logicalDayRange1	= ScheduleUtilities.GetLogicalDayRange(firstDateTime, this.LogicalDayOffset, this.LogicalDayDuration);

			DateTime?			lastDateTime		= dateProvider.AddDays(firstDateTime, 1);
			if (lastDateTime == null)
				lastDateTime = dateProvider.MaxSupportedDateTime;
			DateRange			logicalDayRange2	= ScheduleUtilities.GetLogicalDayRange(lastDateTime.Value, this.LogicalDayOffset, this.LogicalDayDuration);

			DateTime			nextDateToProcess	= firstDateTime;

			for (int i = 0; i < 48; i++)
			{
				if (logicalDayRange1.Contains(nextDateToProcess) ||
					logicalDayRange2.Contains(nextDateToProcess))
				{
					if (includeDuration)
						endTimes.Add(new TimePickerItem(nextDateToProcess, nextDateToProcess.Subtract(appointmentStart)));
					else
						endTimes.Add(new TimePickerItem(nextDateToProcess));
				}

				DateTime? adjustedDate = dateProvider.AddMinutes(nextDateToProcess, 30);
				if (adjustedDate == null)
					break;

				nextDateToProcess = adjustedDate.Value;
			}
		}
		#endregion PopulateListWithAppointmentEndTimes

		#region ProcessError

		internal ErrorEventArgs ProcessError(DataErrorInfo error)
		{
			ErrorEventArgs args = new ErrorEventArgs( error );

			this.RaiseError( args );

			if (args.LogError )
			{
				ScheduleUtilities.LogDebuggerError("Global", error);
			}

			DataErrorInfo blockingError = ScheduleUtilities.FindBlockingErrors(error);

			if (null != blockingError)
			{
				// JJD 4/4/11 - TFS69535
				// If we are showing the time zone chooser or its display is pending and this blocking error
    			// is the local TZ missing error then just return
				if (this._timeZoneChooserPending != null ||
					_pendingTimeZoneError != null ||
					_isTimeZoneChooserDisplaying == true)
				{
					if (blockingError.ErrorList == null &&
						blockingError.Context is TimeZoneInfoProvider &&
						blockingError.IsLocalTZTokenError)
						return args;
				}

				var displayingArgs = new ErrorDisplayingEventArgs(blockingError, ScheduleErrorDisplayType.BlockingError);

				this.OnErrorDisplaying(displayingArgs);

				if (!displayingArgs.Cancel)
					this.BlockingError = blockingError;
			}

			return args;
		}

		#endregion // ProcessError

		#region ProcessVerifyInitialState

		private void ProcessVerifyInitialState()
		{
			// if we are displaying the time zone chooser dialog then delay verifying the initial state
			if (_isTimeZoneChooserDisplaying)
			{
				_verifyInitialStatePending = true;
				return;
			}

			_verifyInitialStatePending = false;

			// set the token to a refernce to this so we prevent re-triggering this verification
			_verifyToken = this;
			
			List<DataErrorInfo> errorList = new List<DataErrorInfo>();

			ScheduleDataConnectorBase connector = this.DataConnector;

			if (connector == null)
				errorList.Add(ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NoConnectorSpecifed", this.GetType().Name));
			else
			{
				connector.VerifyInitialState(errorList);
			}

			// walk over the controls to let them verify their state
			foreach (IScheduleControl control in this.Controls)
			{
				if (control != null)
				{
					control.VerifyInitialState(errorList);
				}
			}

			Resource user = this.CurrentUser;
			if (user == null)
			{
				errorList.Add(ScheduleUtilities.CreateDiagnosticFromId(this, "LE_CurrentUserNotSet", this.GetType().Name));
			}

			if (errorList.Count == 0)
				return;

			DataErrorInfo error = new DataErrorInfo( errorList );

			error.Severity = ErrorSeverity.Diagnostic;

			ScheduleUtilities.RaiseErrorHelper(this, error);
		}

		#endregion //ProcessVerifyInitialState	
		
		#region RaisePendingTimeZoneError

		private void RaisePendingTimeZoneError()
		{
			if (_pendingTimeZoneError != null)
			{
				DataErrorInfo error = _pendingTimeZoneError;
				_pendingTimeZoneError = null;
				ScheduleUtilities.RaiseErrorHelper(this, error);
			}
		}

		#endregion //RaisePendingTimeZoneError	
        
		#region RemoveActivityFromDialogMap
		internal bool RemoveActivityFromDialogMap(ActivityBase activity)
		{
			// JM 01-06-12 TFS95261 Change the ActiveDialogMap references to be non-static.
			if (this.ActiveDialogMap.ContainsKey(activity))
			{
				this.ActiveDialogMap.Remove(activity);
				return true;
			}

			return false;
		}
		#endregion //RemoveActivityFromDialogMap

		#region VerifyInitialState

		internal void VerifyInitialState()
		{
			if (_verifyToken != null || DesignerProperties.GetIsInDesignMode(this))
				return;

			DateTime dt = DateTime.UtcNow.AddSeconds(10);

			_verifyToken = TimeManager.Instance.AddTask(dt , ( ) => this.ProcessVerifyInitialState());
		}

		#endregion //VerifyInitialState

		#region VerifyTimeZones

		internal void VerifyTimeZones()
		{
			if (this.ShouldShowTimeZoneChooser())
			{
				_pendingTimeZoneError = null;
				_timeZoneChooserPending = this.Dispatcher.BeginInvoke(new ScheduleUtilities.MethodInvoker(this.ProcessShowTimeZoneChooser));
			}
			else
			{
				// if we aren't about to show 
				if (this._timeZoneChooserPending == null && _pendingTimeZoneError != null && _isTimeZoneChooserDisplaying == false)
					RaisePendingTimeZoneError();
			}

		}

		#endregion //VerifyTimeZones	
    
		#endregion // Internal Methods

		#region Private Methods

		#region CheckPermissionsForActivityOperation

		private bool CheckPermissionsForActivityOperation( ActivityType? activityType, ActivityOperation activityOperation, ActivityBase activityContext, ResourceCalendar calendarContext )
		{
			Debug.Assert( ActivityOperation.Add == activityOperation || null != activityContext, "Activity must be specified except for the add operation." );

			if ( !activityType.HasValue && null != activityContext )
				activityType = activityContext.ActivityType;

			if ( null == calendarContext && null != activityContext )
				calendarContext = activityContext.OwningCalendar;

			// If neither the activity type nor the activity context is specified then return false.
			// 
			if ( !activityType.HasValue )
				return false;

			if ( null != activityContext && ( activityContext.IsLocked ?? false ) )
				return false;

			Resource resource = null != calendarContext ? calendarContext.OwningResource : null;
			if ( null != resource && resource.IsLocked )
				return false;

			ActivitySettings activitySettings = this.GetActivitySettings( activityType.Value );
			if ( null != activitySettings && !activitySettings.IsActivityOperationAllowed( activityOperation ) )
				return false;

			return true;
		}

		#endregion // CheckPermissionsForActivityOperation

		#region ClearSelectedActivities

		/// <summary>
		/// Clears selected activities in all controls.
		/// </summary>
		private void ClearSelectedActivities( )
		{
			foreach ( IScheduleControl cc in _controls )
			{
				ScheduleControlBase ctrl = cc as ScheduleControlBase;
				if ( null != ctrl )
					ctrl.SelectedActivities.Clear( );
			}
		}

		#endregion // ClearSelectedActivities

		#region DisplayActivityDialogPart2
		private bool DisplayActivityDialogPart2(ActivityBase activity, FrameworkElement container)
		{
			// Ensure that we have a container.
			Debug.Assert(container != null, "Container is null!");
			if (container == null)
				container = this._rootPanel;

			// Determine if modifications are allowed.
			bool allowModifications = activity.IsAddNew ? true : this.IsActivityOperationAllowed(activity, ActivityOperation.Edit);
			if (activity.IsOccurrence && false == this.IsVarianceActivityAllowed(activity))
				allowModifications = false;

			// Raise the ActivityDialogDisplaying event.  Return if the dialog was celcelled in the event handler.
			ActivityDialogDisplayingEventArgs args = new ActivityDialogDisplayingEventArgs(activity, allowModifications);
			args.Cancel = false;
			this.OnActivityDialogDisplaying(args);
			if (args.Cancel)
				return false;

			// Update allowModifications and allowRemove flags
			allowModifications	= args.AllowModifications;
			bool allowRemove	= this.IsActivityOperationAllowed(activity, ActivityOperation.Remove);

			// Check to see if we have already displayed an appointment for this dialog.  If so, activate it.
			FrameworkElement previousDialog;
			// JM 01-06-12 TFS95261 Change the ActiveDialogMap references to be non-static.
			if (this.ActiveDialogMap.TryGetValue(activity, out previousDialog))
			{
				DialogManager.ActivateDialog(previousDialog);
				return previousDialog != null;
			}

			// If we have a DialogFactory, get the dialog from the factory.
			FrameworkElement fe = null;
			if (this.DialogFactory != null)
				fe = this.DialogFactory.CreateActivityDialog(container, this, activity, allowModifications, allowRemove);

			// If there is no DialogFactory or the DialogFactory did not return a dialog, create an appropriate activity dialog.
			if (fe == null)
			{
				ActivityDialogRibbonLite ribbonLite;

				switch (activity.ActivityType)
				{
					case ActivityType.Appointment:
						{
							fe			= new AppointmentDialogCore(container, this, activity as Appointment);
							ribbonLite	= new ActivityDialogRibbonLite(this.ColorSchemeResolved, true, true, true);
							break;
						}
					case ActivityType.Journal:
						{
							fe			= new JournalDialogCore(container, this, activity as Journal);
							ribbonLite	= new ActivityDialogRibbonLite(this.ColorSchemeResolved, true, false, false);
							break;
						}
					case ActivityType.Task:
						{
							fe			= new TaskDialogCore(container, this, activity as Task);
							ribbonLite	= new ActivityDialogRibbonLite(this.ColorSchemeResolved, true, false, false);
							break;
						}
					default:	// Return false for unsupported activity types.
						return false;
				}

				if (fe != null)
				{
					ribbonLite.HorizontalAlignment	= System.Windows.HorizontalAlignment.Stretch;
					ribbonLite.Margin				= new Thickness(4);
					ribbonLite.DataContext			= fe;
					((ActivityDialogCore)fe).IsActivityModifiable			= allowModifications;
					((ActivityDialogCore)fe).NavigationControlSiteContent	= ribbonLite;

					// JM 10-3-11 TFS90121 - Cast to ActivityDialogCore instead of AppointmentDialogCore
					// JM 9-29-11 TFS80833 - Set IsAppointmentRemoveable
					((ActivityDialogCore)fe).IsActivityRemoveable = allowRemove;
				}
			}

			// AS 11/19/10 TFS60350
			// Currently we only clean up the activity dialog map from the appointment dialog core 
			// but really we should clean it up even if we have a different activity dialog provided.
			//
			Action<bool?, ActivityBase> callback = delegate(bool? dialogResult, ActivityBase a)
			{
				this.RemoveActivityFromDialogMap(a);
			};

			Size dialogSize = new Size(800, 600);


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


			FrameworkElement dialogElement = DialogManager.DisplayDialog(container,
																			fe,
																			dialogSize,
																			true,
																			activity.Subject,
																			false,
																			this.GetDialogResources(),	// JM 03-30-11 TFS69614
																			null,
																			new DialogManager.CloseDialogHelper<ActivityBase>(callback, activity).OnClosed //null - AS 11/19/10 TFS60350
																			);
			if (dialogElement != null)
			{
				// JM 01-06-12 TFS95261 Change the ActiveDialogMap references to be non-static.
				this.ActiveDialogMap.Add(activity, dialogElement);
				DialogManager.ActivateDialog(dialogElement);
			}

			return dialogElement != null;
		}
		#endregion //DisplayActivityDialogPart2

		// JM 01-06-12 TFS95261 Added.
		#region DisplaySubDialogHelper
		private FrameworkElement DisplaySubDialogHelper(FrameworkElement container, FrameworkElement dialogContents, Size dialogSize, bool resizable, object header, bool showModally, ResourceDictionary resources, Func<bool> closingCallback, Action<bool?> closedCallback, bool resizeSilverlightWindowToFitInBrowser)
		{
			SubDialogCloseHelper sdch	= new SubDialogCloseHelper(this, closedCallback);
			closedCallback				= sdch.OnClosed;
			this.ActiveSubDialogs.Add(sdch);

			// JM 04-12-11 TFS69396 Call the new overload that takes a resizeSilverlightWindowToFitInBrowser parameter
			FrameworkElement dialogElement = DialogManager.DisplayDialog(container, dialogContents, dialogSize, resizable, header, showModally, resources, closingCallback, closedCallback, resizeSilverlightWindowToFitInBrowser);

			if (dialogElement != null)
				sdch.Dialog = dialogElement;
			else
				this.ActiveSubDialogs.Remove(sdch);

			return dialogElement;
		}
		#endregion //DisplaySubDialogHelper

		#region EndEditOnCopyHelper

		/// <summary>
		/// Ends or cancels edit operation that was started via the <see cref="BeginEditWithCopy"/> method.
		/// </summary>
		/// <param name="cloneActivity">The cloned activity that was returned by the <i>BeginEditWithCopy</i> method.</param>
		/// <param name="cancel">If true, cancels the edit operation. If false, commits the changes made to the clone to the original activity.</param>
		/// <param name="force"></param>
		/// <returns>When canceling, returns a DataErrorInfo or null. When committing, returns an ActivityOperationResult instance or DataErrorInfo instance.</returns>
		private object EndEditOnCopyHelper( ActivityBase cloneActivity, bool cancel, bool force )
		{
			// Get the original activity associated with the clone.
			// 
			ActivityBase clonedFrom = cloneActivity.GetValueHelper<ActivityBase>( ActivityBase.StorageProps.EditClonedFrom );
			if ( null == clonedFrom )
				return ScheduleUtilities.CreateDiagnosticFromId(cloneActivity, "LE_ActivityNotAClone"); // "Specified activity is not a cloned activity that was returned from BeginEditWithCopy call."

			object retValue = null;
			DataErrorInfo error;
			if ( !cancel )
			{
				// Start BeginEdit operation on the original activity via the connector.
				// 
				if ( this.BeginEdit( clonedFrom, out error ) )
				{
					// Copy modified values from the clone into the original activity.
					// 
					clonedFrom.ApplyEditCopy( true );

					// End the edit operation on the original activity.
					// 
					retValue = this.EndEdit( clonedFrom, force );
				}
				else
					retValue = error;
			}

			// Clear the clone reference that's stored on the original activity and the original activity reference
			// that's stored on the clone.
			// 
			clonedFrom.ClearEditCopy( );

			return retValue;
		}

		#endregion // EndEditOnCopyHelper

		#region GetActivitySettings

		private ActivitySettings GetActivitySettings( ActivityType activityType )
		{
			ActivitySettings activitySettings = null;

			ScheduleSettings settings = this.Settings;
			if ( null != settings )
			{
				switch ( activityType )
				{
					case ActivityType.Appointment:
						activitySettings = settings.AppointmentSettings;
						break;
					case ActivityType.Journal:
						activitySettings = settings.JournalSettings;
						break;
					case ActivityType.Task:
						activitySettings = settings.TaskSettings;
						break;
					default:
						Debug.Assert( false, "Unknown activity type." );
						break;
				}
			}

			return activitySettings;
		} 

		#endregion // GetActivitySettings

		// JM 03-30-11 TFS69614 Added.
		#region GetDialogResources
		private ResourceDictionary GetDialogResources()
		{
			ResourceDictionary dialogResources = new ResourceDictionary();

			// AS 3/14/12 TFS104787
			//if (null != this.ColorSchemeResolved.DialogResources)
			//	dialogResources.MergedDictionaries.Add(this.ColorSchemeResolved.DialogResources);
			var colorSchemeResources = this.ColorSchemeResolved.DialogResources;

			if (null != colorSchemeResources)
				dialogResources.MergedDictionaries.Add(colorSchemeResources);

			// AS 3/19/12 TFS105110
			// Have the color scheme do this so it can choose whether it should or not.
			//
			//Style scrollBarStyle = this.ColorSchemeResolved.ScrollBarStyle;
			//if (null != scrollBarStyle)
			//    dialogResources.Add(typeof(System.Windows.Controls.Primitives.ScrollBar), scrollBarStyle);

			// AS 3/9/12 TFS102032
			// Have the color scheme include this if it needs it. This is important now because 
			// we selectively include the dialog resources from the color scheme.
			//
//            // JM 04-15-11 TFS72710 - Load IGTheme resources for the xamDialog if the current color scheme is IGColorScheme.
//#if SILVERLIGHT
//            if (this.ColorSchemeResolved is IGColorScheme)
//            {
//                ResourceDictionary rd	= new ResourceDictionary();
//                rd.Source				= CoreUtilities.BuildEmbeddedResourceUri(typeof(XamScheduleDataManager).Assembly, "Themes/IG.xamDialogWindow.xaml");
//                dialogResources.MergedDictionaries.Add(rd);
//            }
//#endif

			return dialogResources;
		}
		#endregion //GetDialogResources

		#region GetWorkingHours

		internal WorkingHoursCollection GetWorkingHours( Resource resource, DateTime date, out bool isWorkDay )
		{
			return this.GetWorkingHoursHelper( resource, null, date, out isWorkDay );
		}

		private WorkingHoursCollection GetWorkingHoursHelper( Resource resource, DayOfWeek? dayOfWeekParam, DateTime? dateParam, out bool isWorkDay )
		{
			Debug.Assert( dayOfWeekParam.HasValue ^ dateParam.HasValue, "One and only one of 'dayOfWeekParam' or 'dateParam' parameters must be specified." );

			DateTime date = dateParam ?? default( DateTime );
			DayOfWeek dayOfWeek = dayOfWeekParam ?? date.DayOfWeek;
			bool? isWorkDayNN = null;
			WorkingHoursCollection workingHours = null;

			DaySettingsOverride dsOverride = null;

			if ( dateParam.HasValue )
			{
				if ( resource != null )
				{
					dsOverride = this.ResolveDaySettingsOverride( resource, resource.DaySettingsOverridesIfAllocated, date );

					if ( null != dsOverride )
						GetWorkingHoursHelper( dsOverride.DaySettings, dayOfWeek, ref workingHours, ref isWorkDayNN );
				}

				if ( ( null == workingHours || !isWorkDayNN.HasValue ) && null != _cachedSettings )
				{
					dsOverride = this.ResolveDaySettingsOverride( resource, _cachedSettings.DaySettingsOverridesIfAllocated, date );
					if ( null != dsOverride )
						GetWorkingHoursHelper( dsOverride.DaySettings, dayOfWeek, ref workingHours, ref isWorkDayNN );
				}
			}

			if ( null == workingHours || !isWorkDayNN.HasValue )
			{
				if (null != resource)
					GetWorkingHoursHelper( resource.DaysOfWeek, dayOfWeek, ref workingHours, ref isWorkDayNN );

				if ( ( null == workingHours || !isWorkDayNN.HasValue ) && null != _cachedSettings )
					GetWorkingHoursHelper( _cachedSettings.DaysOfWeek, dayOfWeek, ref workingHours, ref isWorkDayNN );
			}

			isWorkDay = isWorkDayNN ?? ScheduleUtilities.IsSet( 
				null != _cachedSettings ? _cachedSettings.WorkDays : ScheduleSettings.DEFAULT_WORKDAYS, dayOfWeek );

			if ( isWorkDay && ( null == workingHours || 0 == workingHours.Count ) )
				workingHours = this.GetResolvedDefaultWorkingHours( );

			return isWorkDay ? workingHours : null;
		} 

		#endregion // GetWorkingHours

		#region GetWorkingHoursHelper

		private static void GetWorkingHoursHelper( ScheduleDaysOfWeek coll, DayOfWeek dayOfWeek, ref WorkingHoursCollection workingHours, ref bool? isWorkDay )
		{
			DaySettings settings = null != coll ? coll.GetDaySettingsIfAllocated( dayOfWeek ) : null;
			GetWorkingHoursHelper( settings, dayOfWeek, ref workingHours, ref isWorkDay );
		}

		private static void GetWorkingHoursHelper( DaySettings settings, DayOfWeek dayOfWeek, ref WorkingHoursCollection workingHours, ref bool? isWorkDay )
		{
			if ( null != settings )
			{
				if ( !isWorkDay.HasValue )
					isWorkDay = settings.IsWorkday;

				if ( null == workingHours )
				{
					workingHours = settings.WorkingHoursIfAllocated;
					if ( null != workingHours && 0 == workingHours.Count )
						workingHours = null;
				}
			}
		}

		#endregion // GetWorkingHoursHelper

		#region InitializeCurrentDate
		private void InitializeCurrentDate()
		{
			var tzProvider = this.TimeZoneInfoProviderResolved;
			if (tzProvider.IsValid)
			{
				DateTime now = ScheduleUtilities.ConvertFromUtc( tzProvider.LocalToken, CurrentTime.Now);
				TimeSpan logicalDayOffset = this.LogicalDayOffset;
				DateRange range = ScheduleUtilities.GetLogicalDayRange(now, logicalDayOffset);

				Debug.Assert(range.ContainsExclusive(now), "LogicalDayRange should contain the current time");

				DateTime date = range.Start.Subtract(logicalDayOffset);
				Debug.Assert(date.TimeOfDay.Ticks == 0, "Calculated calendar date is not the start of a day");
				this.CurrentDate = date;
			}
			else
				this.CurrentDate = DateTime.Today;
		}
		#endregion // InitializeCurrentDate

		#region NotifySettingsChanged

		private void NotifySettingsChanged( object sender, string property, object extraInfo )
		{
			foreach ( IScheduleControl ctrl in this.Controls )
			{
				ctrl.OnSettingsChanged( sender, property, extraInfo );
			}
		}
		
		#endregion //NotifySettingsChanged

		#region OnCurrentDayTimerTick
		private void OnCurrentDayTimerTick()
		{
			// release our strong reference to the timer token
			_currentDayTimerToken = null;

			// update the current date
			this.InitializeCurrentDate();

			// add another timer watch for when this date changes
			this.UpdateLogicalDayTimer();
		}
		#endregion // OnCurrentDayTimerTick

		#region OnLoaded


		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// unhok the event
			this.Loaded -= new RoutedEventHandler(OnLoaded);

			// re-cerify time zones 
			this.VerifyTimeZones();
		}


		#endregion //OnLoaded	
    
		#region OnPendingOperationsChanged
		private static void OnPendingOperationsChanged(XamScheduleDataManager owner, object dataItem, string propertyName, object extraInfo)
		{
			owner.OnPendingOperationsChanged(dataItem, propertyName, extraInfo);
		}

		private void OnPendingOperationsChanged(object dataItem, string propertyName, object extraInfo)
		{
			if (dataItem == _pendingOperationsSource)
			{
				bool newHasPending = _pendingOperationsSource.Count == 0;
				bool oldHasPending = this.HasPendingOperations;

				if (oldHasPending != newHasPending)
					this.SetValue(HasPendingOperationsPropertyKey, KnownBoxes.FromValue(newHasPending));
			}
			else
			{
				var result = dataItem as OperationResult;

				if (null != result && result.IsComplete)
					_pendingOperationsSource.Remove(result);
			}
		}
		#endregion // OnPendingOperationsChanged

		#region OnRecurrenceChooserDialogClosed
		private void OnRecurrenceChooserDialogClosed(bool? dialogResult, ActivityRecurrenceChooserDialog.ChooserResult recurrenceChooserDialogResult)
		{
			if (true == dialogResult)
			{
				if (recurrenceChooserDialogResult.Choice == RecurrenceChooserChoice.None)
					return;

				var data = recurrenceChooserDialogResult.UserData as Tuple<FrameworkElement, ActivityBase>;
				var activity = recurrenceChooserDialogResult.Choice == RecurrenceChooserChoice.Series 
					? data.Item2.RootActivity 
					: data.Item2;

				this.DisplayActivityDialogPart2(activity, data.Item1);
			}
		}
		#endregion //OnRecurrenceChooserDialogClosed

		#region OnReminderDialogClosed
		private void OnReminderDialogClosed(bool? dialogResult)
		{
			_reminderDialog = null;
		}

		private void OnReminderDialogClosed( bool? dialogResult, Action<bool?> originalCallback )
		{
			this.OnReminderDialogClosed(dialogResult);

			if ( null != originalCallback )
				originalCallback(dialogResult);
		}
		#endregion //OnReminderDialogClosed

		#region OnSubObjectPropertyChanged

		private static void OnSubObjectPropertyChanged( XamScheduleDataManager dm, object sender, string property, object extraInfo )
		{
			dm.NotifySettingsChanged( sender, property, extraInfo );

			// If the Id of the CurrentUser resource changes, update the CurrentUserId property to reflect the new id of the resource.
			// 
			if ( sender is Resource && sender == dm.CurrentUser )
			{
				dm.SyncCurrentUserAndId( false );
			}
			else if ( ( sender == dm._cachedSettings && property == "LogicalDayOffset" ) || ( sender == dm && property == "Settings" ) )
			{
				dm.UpdateLogicalDayTimer( );
			}
			else if ( ( sender == dm && property == "ResourceItems" ) || sender is ResourceCollection )
			{
				dm.SyncCurrentUserAndId( true );

				// JJD 6/17/11 - TFS74180
				// If the sender is the ResourceItems then let the ColorSchemeResolved know that
				// resources have changed so that it can see if any of the old resource calendars with
				// provider assignments are still in the collection. If none are then the color scheme
				// can clear its cached maps so that base color assignment order can be consistent 
				// when all of the resource items have been changed out.
				if (sender == dm.ResourceItems)
				{
					CalendarColorScheme colorScheme = dm.ColorSchemeResolved;

					if (colorScheme != null)
						colorScheme.OnResourceItemsChanged(sender as ResourceCollection);
				}
			}
			else if ( ( sender is TimeZoneInfoProvider ) && property == "Version" ||
					 ( sender is ScheduleDataConnectorBase ) && property == "TimeZoneInfoProviderResolved" )
			{
				dm.RefreshDisplay( );

				// since the current date is dependant on the local timezone, update that when the 
				// time zone changes
				dm.InitializeCurrentDate();
			}
			else if ( sender is ScheduleDataConnectorBase )
			{
				switch ( property )
				{
					case "DataSourceReset":
						// When an activities data source is reset, clear the selected activities. Alternative
						// is to query the data source for existence of data items of the selected activities
						// and keep activities that still exist however this would not be performant.
						// 
						dm.ClearSelectedActivities( );
						break;
					case "Error":
						DataErrorInfo error = extraInfo as DataErrorInfo;
						if (null != error)
						{
							TimeZoneInfoProvider tzProvider = error.Context as TimeZoneInfoProvider;
							if (tzProvider != null)
							{
								dm._pendingTimeZoneError = error;
								dm.VerifyTimeZones();
							}
							else
								ScheduleUtilities.RaiseErrorHelper(dm, error);
						}
						break;
					case "ClearBlockingError":
						dm.BlockingError = null;
						break;
				}
			}

			if ( sender == dm._colorSchemeResolved )
			{
				switch (property)
				{
					case "BrushVersion":
						dm.BumpBrushVersion();
						break;
				}
			}
		}

		#endregion // OnSubObjectPropertyChanged

		#region OnTimeZoneChooserClosed

		private void OnTimeZoneChooserClosed(bool? dialogResult, TimeZoneChooserDialog.ChooserResult tzChooserResult)
		{
			_isTimeZoneChooserDisplaying = false;

			_pendingTimeZoneError = null;

			if (tzChooserResult != null && tzChooserResult.Choice != null)
			{
				TimeZoneInfoProvider._timeZoneIdSelected = tzChooserResult.Choice.Id;

				this.TimeZoneInfoProviderResolved.OnDefaultLocalTimeZomeChanged();

				this.RefreshDisplay();
			}

			if (_verifyInitialStatePending)
				this.ProcessVerifyInitialState();
		}

		#endregion //OnTimeZoneChooserClosed

		// JM 01-04-12 TFS95261 Added.
		#region OnUnloaded
		void OnUnloaded(object sender, RoutedEventArgs e)
		{
			this.CloseAllDialogs();
		}
		#endregion //OnUnloaded

		#region ProcessShowTimeZoneChooser

		private void ProcessShowTimeZoneChooser()
		{
			// clear the pending operation
			_timeZoneChooserPending = null;

			// re-verify that we should show the dialog
			if (!this.ShouldShowTimeZoneChooser())
				return;

			// if the root panel isn't set then return
			if (this._rootPanel == null)
				return;


			// if we are not loaded then hook the loaded event and get out
			if (!this.IsLoaded)
			{
				this.Loaded += new RoutedEventHandler(OnLoaded);
				return;
			}


			// set the flag so we show it only once
			_isTimeZoneChooserDisplaying = true;
			_wasTimeZoneChooserDisplayed = true;
			_pendingTimeZoneError = null;

			var tzChooserResult = new TimeZoneChooserDialog.ChooserResult(null);
			var closeHelper = new DialogManager.CloseDialogHelper<TimeZoneChooserDialog.ChooserResult>(this.OnTimeZoneChooserClosed, tzChooserResult);

			this.DisplayTimeZoneChooserDialog(
				this, 
				ScheduleUtilities.GetString("DLG_TimeZoneChooser_Header"), 
				tzChooserResult, 
				null, 
				closeHelper.OnClosed);
		}

		#endregion //ProcessShowTimeZoneChooser	
    
		#region ResolveDaySettingsOverride

		private DaySettingsOverride ResolveDaySettingsOverride( Resource resource, IList<DaySettingsOverride> overrides, DateTime date )
		{
			if ( null != overrides )
			{
				for ( int i = 0, count = overrides.Count; i < count; i++ )
				{
					DaySettingsOverride ii = overrides[i];
					if ( ii.DoesDayMatch( resource, date, this ) )
						return ii;
				}
			}

			return null;
		}

		#endregion // ResolveDaySettingsOverride

		#region ShouldShowTimeZoneChooser

		private bool ShouldShowTimeZoneChooser()
		{
			if (this._timeZoneChooserPending != null ||
				 _wasTimeZoneChooserDisplayed ||
				DesignerProperties.GetIsInDesignMode(this))
				return false;

			PromptForLocalTimeZone promptOption = this.PromptForLocalTimeZone;

			if (promptOption == PromptForLocalTimeZone.Never)
				return false;

			TimeZoneInfoProvider provider = this.TimeZoneInfoProviderResolved;

			if (provider.TimeZoneTokens.Count == 0)
				return false;

			TimeZoneToken utctoken = provider.UtcToken;

			if (utctoken == null)
				return false;

			TimeZoneToken token = provider.LocalToken;

			return token == null || promptOption == PromptForLocalTimeZone.Always;
		}

		#endregion //ShouldShowTimeZoneChooser	
    
		#region UpdateLogicalDayTimer
		private void UpdateLogicalDayTimer()
		{
			if (_currentDayTimerToken != null)
			{
				TimeManager.Instance.Remove(_currentDayTimerToken);
				_currentDayTimerToken = null;
			}

			// JJD 10/12/11 - TFS89043 
			// Moved logic for getting the logical day range into a helper method so it could be 
 			// called by XamDateNavigator's GetTodayRange method
			DateRange? logDayRange = this.GetLogicalDayRange();

			// watch for when the date enters the logical day
			if ( logDayRange.HasValue )
				_currentDayTimerToken = TimeManager.Instance.AddTimeRange(logDayRange.Value, false, this.OnCurrentDayTimerTick);
		}

		#endregion // UpdateLogicalDayTimer

		#region NotifyColorSchemeResolvedChanged
		private void NotifyColorSchemeResolvedChanged()
		{
			foreach (IScheduleControl ctrl in this.Controls)
			{
				ctrl.OnColorSchemeResolvedChanged();
			}
		}
		#endregion // NotifyColorSchemeResolvedChanged

		#region ValidateHasConnector

		private bool ValidateHasConnector( out DataErrorInfo errorInfo )
		{
			if ( null == this.DataConnector )
			{
				errorInfo = ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NoConnectorSpecifed", this.GetType().Name);// "No data connector has been specified on {0}."
				return false;
			}

			errorInfo = null;
			return true;
		} 

		#endregion // ValidateHasConnector

		#endregion // Private Methods

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