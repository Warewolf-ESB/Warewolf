using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;







namespace Infragistics.Controls.Schedules

{
    #region ActivityEventArgs Class

    /// <summary>
    /// Base class for activity event args.
    /// </summary>
    public class ActivityEventArgs : EventArgs
    {
        #region Member Vars

        private ActivityBase _activity;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityEventArgs( ActivityBase activity )
        {
            CoreUtilities.ValidateNotNull( activity );

            _activity = activity;
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region Activity

        /// <summary>
        /// The activity for which the operation is being performed.
        /// </summary>
        public ActivityBase Activity
        {
            get
            {
                return _activity;
            }
        }

        #endregion // Activity

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // ActivityEventArgs Class


    #region CancellableActivityEventArgs Class

    /// <summary>
    /// Base class for activity event args that are cancellable.
    /// </summary>
    public class CancellableActivityEventArgs : ActivityEventArgs
    {
        #region Member Vars

        private bool _cancel;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public CancellableActivityEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region Cancel

        /// <summary>
        /// Specifies whether to cancel the operation.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// You can set this property to true to cancel the operation being performed.
        /// </para>
        /// </remarks>
        public bool Cancel
        {
            get
            {
                return _cancel;
            }
            set
            {
                _cancel = value;
            }
        }

        #endregion // Cancel

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // CancellableActivityEventArgs Class

	#region ActivitiesEventArgs
	/// <summary>
	/// Base class for an event that potentially involves multiple <see cref="ActivityBase"/> instances
	/// </summary>
	public class ActivitiesEventArgs : EventArgs
	{
		#region Member Variables

		private IList<ActivityBase> _activities;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initialize a new <see cref="ActivitiesEventArgs"/>
		/// </summary>
		/// <param name="activities">The activities being dragged</param>
		public ActivitiesEventArgs(IList<ActivityBase> activities)
		{
			CoreUtilities.ValidateNotNull(activities, "activities");

			var activitiesCopy = new ActivityBase[activities.Count];
			activities.CopyTo(activitiesCopy, 0);
			_activities = activities;
		}
		#endregion // Constructor

		#region Properties

		#region Activities
		/// <summary>
		/// Returns the associated <see cref="ActivityBase"/> instances
		/// </summary>
		public IList<ActivityBase> Activities
		{
			get { return _activities; }
		}
		#endregion // Activities

		#endregion // Properties
	} 
	#endregion // ActivitiesEventArgs

	#region CancellableActivitiesEventArgs
	/// <summary>
	/// Base class for a cancellable event that potentially involves multiple <see cref="ActivityBase"/> instances
	/// </summary>
	public class CancellableActivitiesEventArgs : ActivitiesEventArgs
	{
		#region Member Variables

        private bool _cancel;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initialize a new <see cref="CancellableActivitiesEventArgs"/>
		/// </summary>
		/// <param name="activities">The activities being dragged</param>
		public CancellableActivitiesEventArgs(IList<ActivityBase> activities) : base(activities)
		{
		}
		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Cancel

		/// <summary>
		/// Specifies whether to cancel the operation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can set this property to true to cancel the operation being performed.
		/// </para>
		/// </remarks>
		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		#endregion // Cancel

		#endregion // Public Properties

		#endregion // Properties
	}
	#endregion // CancellableActivitiesEventArgs

	#region ActivitiesDraggingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivitiesDragging"/> event
	/// </summary>
	public class ActivitiesDraggingEventArgs : CancellableActivitiesEventArgs
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivitiesDraggingEventArgs"/>
		/// </summary>
		/// <param name="activities">1 or more activities that are being dragged</param>
		public ActivitiesDraggingEventArgs(IList<ActivityBase> activities)
			: base(activities)
		{
		}
		#endregion // Constructor
	} 
	#endregion // ActivitiesDraggingEventArgs

	#region ActivitiesDraggedEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivitiesDragged"/> event
	/// </summary>
	public class ActivitiesDraggedEventArgs : ActivitiesEventArgs // AS 12/16/10 TFS61910 Should not derive from CancellableActivitiesEventArgs
	{
		#region Member Variables

		private bool _isCopy; 

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivitiesDraggedEventArgs"/>
		/// </summary>
		/// <param name="activities">1 or more activities that are being dragged</param>
		/// <param name="isCopy">True if the drag resulted in a copy of the activities.</param>
		public ActivitiesDraggedEventArgs(IList<ActivityBase> activities, bool isCopy)
			: base(activities)
		{
			_isCopy = isCopy;
		}
		#endregion // Constructor

		#region Properties

		#region IsCopy
		/// <summary>
		/// Returns true if the drag operation resulted in a copy of the activities.
		/// </summary>
		/// <remarks>
		/// <p class="body">If true then the activities provided are the new activities that were created.</p>
		/// </remarks>
		public bool IsCopy
		{
			get { return _isCopy; }
		}
		#endregion // IsCopy

		#endregion // Properties
	}
	#endregion // ActivitiesDraggedEventArgs

	#region ActivityAddedEventArgs Class

	/// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityAdded"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity that was added.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityAdded"/>
    public class ActivityAddedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityAddedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityAddedEventArgs Class

    #region ActivityAddingEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityAdding"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity being added.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityAdding"/>
    public class ActivityAddingEventArgs : CancellableActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity">ActivityBase object.</param>
        public ActivityAddingEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityAddingEventArgs Class

    #region ActivityChangedEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityChanged"/> event.
    /// </summary>
    public class ActivityChangedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity">Activity object.</param>
        public ActivityChangedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityChangedEventArgs Class

    #region AppointmentChangingEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityChanging"/> event.
    /// </summary>
    public class ActivityChangingEventArgs : CancellableActivityEventArgs
    {
        #region Member Vars

        private ActivityBase _originalActivityData;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity">Activity object.</param>
        /// <param name="originalActivityData">Original activity data.</param>
        public ActivityChangingEventArgs( ActivityBase activity, ActivityBase originalActivityData )
            : base( activity )
        {
            _originalActivityData = originalActivityData;
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #region OriginalActivity

        /// <summary>
        /// Used to get the original activity data.
        /// </summary>
        public ActivityBase OriginalActivityData
        {
            get
            {
                return _originalActivityData;
            }
        }

        #endregion // OriginalActivity

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // AppointmentChangingEventArgs Class

	#region ActivityRecurrenceChooserDialogDisplayingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivityRecurrenceChooserDialogDisplaying"/> event.
	/// </summary>
	public class ActivityRecurrenceChooserDialogDisplayingEventArgs : ActivityEventArgs
	{
		#region Member Variables

		private RecurrenceChooserType _chooserType;
		private RecurrenceChooserAction _chooserAction = RecurrenceChooserAction.Prompt;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityRecurrenceChooserDialogDisplayingEventArgs"/>
		/// </summary>
		/// <param name="activity">The activity for which the chooser is to be displayed</param>
		/// <param name="chooserType">The operation for which the dialog is being displayed</param>
		public ActivityRecurrenceChooserDialogDisplayingEventArgs( ActivityBase activity, RecurrenceChooserType chooserType )
			: base(activity)
		{
			_chooserType = chooserType;
		}
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// Returns an enumeration indicating the type of operation to be performed on the occurrence/series.
		/// </summary>
		public RecurrenceChooserType ChooserType
		{
			get { return _chooserType; }
		}

		/// <summary>
		/// Returns or sets the action that should be taken by the object initiating the event.
		/// </summary>
		public RecurrenceChooserAction ChooserAction
		{
			get { return _chooserAction; }
			set { _chooserAction = value; }
		}
		#endregion // Properties
	} 
	#endregion // ActivityRecurrenceChooserDialogDisplayingEventArgs

    #region ActivityDialogDisplayingEventArgs Class

    /// <summary>
	/// Event args associated with the <see cref="XamScheduleDataManager.ActivityDialogDisplaying"/> event.
	/// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity for which the dialog is
    /// being displayed.
    /// </para>
    /// <para class="body">
    /// You can set the <see cref="CancellableActivityEventArgs.Cancel"/> property to true to prevent 
    /// the dialog from being displayed. This can also be useful when you want to display your own dialog 
    /// instead of the default built-in dialog.
    /// </para>
    /// </remarks>
	public class ActivityDialogDisplayingEventArgs : CancellableActivityEventArgs
	{
		#region Member Vars

		private bool _allowModifications;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="activity">ActivityBase derived object.</param>
		/// <param name="allowModifications">Whether the user can modify the activity.</param>
        public ActivityDialogDisplayingEventArgs( ActivityBase activity, bool allowModifications )
            : base( activity )
		{
			_allowModifications = allowModifications;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region AllowModifications

		/// <summary>
		/// Specifies whether the user is allowed to modify the activity.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The activity dialog can be displayed in read-only mode where the user is not
		/// allowed to modify the contents. You can force the dialog to be read-only by setting
		/// this property to false or force the dialog to be modifiable by setting this property
		/// to true.
		/// </para>
		/// </remarks>
		public bool AllowModifications
		{
			get
			{
				return _allowModifications;
			}
			set
			{
				_allowModifications = value;
			}
		}

		#endregion // AllowModifications

		#endregion // Public Properties

		#endregion // Properties
	}

    #endregion // ActivityDialogDisplayingEventArgs Class

	// JM 02-18-11 TFS61928 Added.
	#region ActivityRecurrenceDialogDisplayingEventArgs Class

	/// <summary>
	/// Event args associated with the <see cref="XamScheduleDataManager.ActivityRecurrenceDialogDisplaying"/> event.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <see cref="ActivityEventArgs.Activity"/> property returns the activity for which the dialog is
	/// being displayed.
	/// </para>
	/// <para class="body">
	/// You can set the <see cref="CancellableActivityEventArgs.Cancel"/> property to true to prevent 
	/// the dialog from being displayed. This can also be useful when you want to display your own dialog 
	/// instead of the default built-in dialog.
	/// </para>
	/// </remarks>
	/// <seealso cref="Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore"/>
	public class ActivityRecurrenceDialogDisplayingEventArgs : CancellableActivityEventArgs
	{
		#region Member Vars

		private bool _allowModifications;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="activity">ActivityBase derived object.</param>
		/// <param name="allowModifications">Whether the user can modify the activity.</param>
		public ActivityRecurrenceDialogDisplayingEventArgs(ActivityBase activity, bool allowModifications)
			: base(activity)
		{
			_allowModifications = allowModifications;
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region AllowModifications

		/// <summary>
		/// Specifies whether the user is allowed to modify the contents of the dialog.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// The <see cref="Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore"/> can be displayed in read-only mode where the user is not
		/// allowed to modify the contents. You can force the dialog to be read-only by setting
		/// this property to false or force the dialog to be modifiable by setting this property
		/// to true.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Controls.Schedules.Primitives.ActivityRecurrenceDialogCore"/>
		public bool AllowModifications
		{
			get
			{
				return _allowModifications;
			}
			set
			{
				_allowModifications = value;
			}
		}

		#endregion // AllowModifications

		#endregion // Public Properties

		#endregion // Properties
	}

	#endregion // ActivityRecurrenceDialogDisplayingEventArgs Class


    #region ActivityRemovedEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityRemoved"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity that was removed.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityAdded"/>
    public class ActivityRemovedEventArgs : ActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity"><see cref="ActivityBase"/> derived object.</param>
        public ActivityRemovedEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityRemovedEventArgs Class

    #region ActivityRemovingEventArgs Class

    /// <summary>
    /// Event args associated with the <see cref="XamScheduleDataManager.ActivityRemoving"/> event.
    /// </summary>
    /// <remarks>
    /// <para class="body">
    /// <see cref="ActivityEventArgs.Activity"/> property returns the activity being removed.
    /// </para>
    /// </remarks>
    /// <seealso cref="XamScheduleDataManager.ActivityRemoving"/>
    public class ActivityRemovingEventArgs : CancellableActivityEventArgs
    {
        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="activity">ActivityBase object.</param>
        public ActivityRemovingEventArgs( ActivityBase activity )
            : base( activity )
        {
        }

        #endregion // Constructor
    }

    #endregion // ActivityRemovingEventArgs Class

	#region ActivityResizingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivityResizing"/> event
	/// </summary>
	public class ActivityResizingEventArgs : CancellableActivityEventArgs
	{
		#region Member Variables

		private bool _isAdjustingStart;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityResizingEventArgs"/>
		/// </summary>
		/// <param name="activity">The activity to be resized</param>
		/// <param name="isAdjustingStart">True if the start time is being adjusted; otherwise false if the end/duration is being adjusted</param>
		public ActivityResizingEventArgs(ActivityBase activity, bool isAdjustingStart)
			: base(activity)
		{
			_isAdjustingStart = isAdjustingStart;
		}
		#endregion // Constructor

		#region Properties

		#region IsAdjustingStart
		/// <summary>
		/// Returns a boolean indicating whether the <see cref="ActivityBase.Start"/> or <see cref="ActivityBase.End"/> is being adjusted.
		/// </summary>
		public bool IsAdjustingStart
		{
			get { return _isAdjustingStart; }
		}
		#endregion // IsAdjustingStart

		#endregion // Properties
	} 
	#endregion // ActivityResizingEventArgs

	#region ActivityResizedEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivityResized"/> event
	/// </summary>
	public class ActivityResizedEventArgs : ActivityEventArgs
	{
		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityResizedEventArgs"/>
		/// </summary>
		/// <param name="activity">The activity that was resized</param>
		public ActivityResizedEventArgs(ActivityBase activity)
			: base(activity)
		{
		}
		#endregion // Constructor
	} 
	#endregion // ActivityResizedEventArgs

	// AS 3/1/11 NA 2011.1 ActivityTypeChooser
	#region ActivityTypeChooserDialogDisplayingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ActivityTypeChooserDialogDisplaying"/> event.
	/// </summary>
	public class ActivityTypeChooserDialogDisplayingEventArgs : EventArgs
	{
		#region Member Variables

		private ActivityTypes _availableTypes;
		private ActivityType? _activityType;
		private bool _cancel;
		private ResourceCalendar _calendar;
		private ActivityTypeChooserReason _chooserReason;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ActivityTypeChooserDialogDisplayingEventArgs"/>
		/// </summary>
		/// <param name="availableTypes">The activity types from which the end may choose</param>
		/// <param name="calendar">The calendar associated with the event or null if the action is not associated with a specific calendar.</param>
		/// <param name="chooserReason">Indicating the reason for which the event is being raised.</param>
		public ActivityTypeChooserDialogDisplayingEventArgs(ActivityTypes availableTypes, ResourceCalendar calendar, ActivityTypeChooserReason chooserReason)
		{
			_availableTypes = availableTypes;
			_calendar = calendar;
			_chooserReason = chooserReason;
		}
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// Returns an enumeration indicating the types of activities from which the end user may choose.
		/// </summary>
		public ActivityTypes AvailableTypes
		{
			get { return _availableTypes; }
		}

		/// <summary>
		/// Returns or sets the type of activity that should be used. If left set to null, the end user will be prompted with the <see cref="AvailableTypes"/>
		/// </summary>
		public ActivityType? ActivityType
		{
			get { return _activityType; }
			set { _activityType = value; }
		}

		/// <summary>
		/// Returns the calendar associated with the event or null if there is no calendar associated with the initiating action.
		/// </summary>
		public ResourceCalendar Calendar
		{
			get { return _calendar; }
		}

		/// <summary>
		/// Returns or sets a boolean indicating whether to cancel the initiating operation.
		/// </summary>
		public bool Cancel
		{
			get { return _cancel; }
			set { _cancel = value; }
		}

		/// <summary>
		/// Returns an enumeration indicating the type of operation for which the dialog is being displayed.
		/// </summary>
		public ActivityTypeChooserReason ChooserReason
		{
			get { return _chooserReason; }
		}
		#endregion // Properties
	}
	#endregion // ActivityTypeChooserDialogDisplayingEventArgs

	#region ErrorDisplayingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamScheduleDataManager.ErrorDisplaying"/> event
	/// </summary>
	public class ErrorDisplayingEventArgs : EventArgs
	{
		#region Member Variables

		private DataErrorInfo _error;
		private bool _cancel;
		private ScheduleErrorDisplayType _displayType;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ErrorDisplayingEventArgs"/>
		/// </summary>
		/// <param name="error">The error for which the message is to be displayed</param>
		/// <param name="displayType">Indicates the type of display that will be used to present the error to the user</param>
		public ErrorDisplayingEventArgs( DataErrorInfo error, ScheduleErrorDisplayType displayType )
		{
			CoreUtilities.ValidateNotNull(error, "error");

			_error = error;
			_displayType = displayType;
		}
		#endregion // Constructor

		#region Properties

		#region Cancel

		/// <summary>
		/// Specifies whether to cancel the operation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can set this property to true to prevent the error from being displayed.
		/// </para>
		/// </remarks>
		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		#endregion // Cancel

		#region DisplayType
		/// <summary>
		/// Returns an enumerating indicating the type of error ui that will be displayed if the event is not cancelled.
		/// </summary>
		public ScheduleErrorDisplayType DisplayType
		{
			get { return _displayType; }
		} 
		#endregion // DisplayType

		#region Error

		/// <summary>
		/// Returns an object that contains the error information.
		/// </summary>
		public DataErrorInfo Error
		{
			get
			{
				return _error;
			}
		}

		#endregion // Error

		#endregion // Properties
	} 
	#endregion // ErrorDisplayingEventArgs

	#region ErrorEventArgs Class

	/// <summary>
	/// Event args associated with the <see cref="XamScheduleDataManager.Error"/> event.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <see cref="ErrorEventArgs.Error"/> property returns an <see cref="DataErrorInfo"/> object that
	/// contains the error information.
	/// </para>
	/// </remarks>
	/// <seealso cref="XamScheduleDataManager.Error"/>
	public class ErrorEventArgs : EventArgs
	{
		#region Member Vars

		private DataErrorInfo _error;
		private bool _logError = true;

		#endregion // Member Vars

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ErrorEventArgs"/>.
		/// </summary>
		/// <param name="error">Error information.</param>
		public ErrorEventArgs( DataErrorInfo error )
		{
			CoreUtilities.ValidateNotNull( error );

			_error = error;
		}

		#region Error

		/// <summary>
		/// Gets <see cref="DataErrorInfo"/> object that contains the error information.
		/// </summary>
		public DataErrorInfo Error
		{
			get
			{
				return _error;
			}
		}

		#endregion // Error

		#region LogError

		/// <summary>
		/// Specifies whether to log the error to the debugger's outut window
		/// </summary>
		/// <remarks>
		/// <para class="note">
		/// <b>Note</b>: this property only has no effect if a debugger is attached or is logging.
		/// </para>
		/// </remarks>
		/// <value>The value defaults to true.</value>
		public bool LogError
		{
			get
			{
				return _logError;
			}
			set
			{
				if ( _logError != value )
				{
					_logError = value;
				}
			}
		}

		#endregion // LogError


	} 

	#endregion // ErrorEventArgs Class

	#region ReminderActivatedEventArgs
	/// <summary>
	/// Event args associated with the <see cref="XamScheduleDataManager.ReminderActivated"/> event.
	/// </summary>
	public class ReminderActivatedEventArgs : EventArgs
	{
		#region Member Variables

		private ReminderInfo _reminderInfo;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ReminderActivatedEventArgs"/>
		/// </summary>
		/// <param name="reminderInfo">Reminder that has been activated</param>
		public ReminderActivatedEventArgs(ReminderInfo reminderInfo)
		{
			CoreUtilities.ValidateNotNull(reminderInfo, "reminderInfo");
			_reminderInfo = reminderInfo;
		}
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// Returns the reminder that was activated.
		/// </summary>
		public ReminderInfo ReminderInfo
		{
			get { return _reminderInfo; }
		}
		#endregion // Properties
	} 
	#endregion // ReminderActivatedEventArgs

	#region ReminderDialogDisplayingEventArgs
	/// <summary>
	/// Event args associated with the <see cref="XamScheduleDataManager.ReminderDialogDisplaying"/> event.
	/// </summary>
	public class ReminderDialogDisplayingEventArgs : EventArgs
	{
		#region Member Variables

		private bool _cancel;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ReminderDialogDisplayingEventArgs"/>
		/// </summary>
		public ReminderDialogDisplayingEventArgs()
		{
		}
		#endregion // Constructor

		#region Public Properties

		#region Cancel

		/// <summary>
		/// Specifies whether to cancel operation.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// You can set this property to true to cancel the operation being performed.
		/// </para>
		/// </remarks>
		public bool Cancel
		{
			get
			{
				return _cancel;
			}
			set
			{
				_cancel = value;
			}
		}

		#endregion // Cancel

		#endregion // Public Properties
	} 
	#endregion // ReminderDialogDisplayingEventArgs

	#region SelectedActivitiesChangedEventArgs Class

    /// <summary>
	/// Event args associated with the <see cref="ScheduleControlBase.SelectedActivitiesChanged"/> event.
    /// </summary>
    public class SelectedActivitiesChangedEventArgs : EventArgs
    {
        #region Member Vars

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
 		public SelectedActivitiesChangedEventArgs()
        {
        }

        #endregion // Constructor

        #region Properties

        #region Public Properties

        #endregion // Public Properties

        #endregion // Properties
    }

    #endregion // SelectedActivitiesChangedEventArgs Class

	#region ScheduleHeaderClickEventArgs
	/// <summary>
	/// Event arguments for an event in a <see cref="ScheduleControlBase"/> involving a header representing a date associated with a given <see cref="ResourceCalendar"/>
	/// </summary>
	/// <see cref="XamMonthView.DayHeaderClick"/>
	/// <see cref="XamMonthView.WeekHeaderClick"/>
	/// <see cref="XamDayView.DayHeaderClick"/>
	public class ScheduleHeaderClickEventArgs : EventArgs
	{
		#region Member Variables

		private ResourceCalendar _calendar;
		private DateTime _date;

		#endregion // Member Variables

		#region Constructor
		internal ScheduleHeaderClickEventArgs(ResourceCalendar calendar, DateTime date)
		{
			_calendar = calendar;
			_date = date;
		}
		#endregion // Constructor

		#region Properties

		#region Calendar
		/// <summary>
		/// The calendar for which a click event had occurred.
		/// </summary>
		public ResourceCalendar Calendar
		{
			get { return _calendar; }
		} 
		#endregion // Calendar

		#region Date
		/// <summary>
		/// Returns the logical date of the day whose day header was clicked.
		/// </summary>
		public DateTime Date
		{
			get { return _date; }
		}
		#endregion // Date

		#endregion // Properties
	} 
	#endregion // ScheduleHeaderClickEventArgs

	// AS 12/8/10 NA 11.1 - XamOutlookCalendarView
	#region CurrentViewModeChangingEventArgs
	/// <summary>
	/// Event arguments for the <see cref="XamOutlookCalendarView.CurrentViewModeChanging"/> event
	/// </summary>
	public class CurrentViewModeChangingEventArgs : EventArgs
	{
		#region Member Variables

		private OutlookCalendarViewMode _newValue;
		private OutlookCalendarViewMode _oldValue;
		private bool _cancel;

		#endregion // Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CurrentViewModeChangingEventArgs"/>
		/// </summary>
		/// <param name="oldValue">The old value</param>
		/// <param name="newValue">The proposed new value</param>
		public CurrentViewModeChangingEventArgs( OutlookCalendarViewMode oldValue, OutlookCalendarViewMode newValue )
		{
			_oldValue = oldValue;
			_newValue = newValue;
		}
		#endregion // Constructor

		#region Properties
		/// <summary>
		/// Returns or sets a boolean indicating whether to continue with the operation.
		/// </summary>
		public bool Cancel
		{
			get { return _cancel; }
			set { _cancel = value; }
		}

		/// <summary>
		/// Returns the new proposed value.
		/// </summary>
		public OutlookCalendarViewMode NewValue
		{
			get { return _newValue; }
		}

		/// <summary>
		/// Returns the original/existing value.
		/// </summary>
		public OutlookCalendarViewMode OldValue
		{
			get { return _oldValue; }
		}
		#endregion // Properties
	} 
	#endregion // CurrentViewModeChangingEventArgs

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