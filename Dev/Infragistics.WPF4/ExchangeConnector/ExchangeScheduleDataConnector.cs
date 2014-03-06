using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Security;
using System.Net;
using System.Globalization;
using System.Diagnostics;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.EWS;
using System.Windows;
using System.Collections.Specialized;
using System.Windows.Threading;
using System.ComponentModel;
using System.Windows.Media;
using System.Threading;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Used for providing schedule data from a Microsoft Exchange Server.
	/// </summary>
	/// <seealso cref="XamScheduleDataManager.DataConnector"/>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)] 

	public partial class ExchangeScheduleDataConnector : ScheduleDataConnectorBase
	{
		#region Static Variables

		internal static Color[] ExchangeCategoryColors = new Color[] { 
			Color.FromArgb(255, 231,161, 162), 
			Color.FromArgb(255, 249, 186, 137), 
			Color.FromArgb(255, 247, 221, 143), 
			Color.FromArgb(255, 252, 250, 144), 
			Color.FromArgb(255, 120, 209, 104), 
			Color.FromArgb(255, 159, 220, 201), 
			Color.FromArgb(255, 198, 210, 176), 
			Color.FromArgb(255, 157, 183, 232), 
			Color.FromArgb(255, 181, 161, 226), 
			Color.FromArgb(255, 218, 174, 194),
			Color.FromArgb(255, 218, 217, 220), 
			Color.FromArgb(255, 107, 121, 148), 
			Color.FromArgb(255, 191, 191, 191), 
			Color.FromArgb(255, 111, 111, 111), 
			Color.FromArgb(255, 79, 79, 79),
			Color.FromArgb(255, 193, 26, 37), 
			Color.FromArgb(255,226, 98, 13 ), 
			Color.FromArgb(255, 199, 153, 48), 
			Color.FromArgb(255, 185, 179, 0), 
			Color.FromArgb(255, 54, 143, 43),
			Color.FromArgb(255, 50, 155, 122), 
			Color.FromArgb(255, 119, 139, 69), 
			Color.FromArgb(255, 40, 88, 165), 
			Color.FromArgb(255, 92, 63, 163), 
			Color.FromArgb(255, 147, 68, 107)
		};

		#endregion //Static Variables

		#region Member Variables

		private WeakList<ExchangeActivityQueryResult> _activityQueryResults;
		private int _activityQueryResultsCompactLimit = 10;

		// MD 4/27/11 - TFS72779
		private ExchangeVersion? _autoDetectedServerVersion;

		private WeakDictionary<string, ActivityBase> _cachedActvities;
		private int _cachedActvitiesCompactLimit = 10;	// MD 4/11/11 - TFS72059
		private ExchangeServerConnectionSettings _cachedServerConnectionSettings;
		private List<ExchangeService> _cancelledConnections;
		private readonly ErrorCallback _defaultErrorCallback;
		private List<ExchangeService> _exchangeServices;
		private DeferredOperation _initializeConnection;

		// MD 4/27/11 - TFS72779
		private bool _isVersionAutoDetected;

		private WeakDictionary<ActivityBase, WeakList<ActivityBase>> _occurrencesByRoot;
		private int _occurrencesByRootCompactLimit = 10;
		private List<ExchangeService> _pendingConnections;
		private int _pollingCount;
		private DispatcherTimer _pollingTimer;
		private ExchangeActivityReminderManager _reminderManager;
		private ResourceListManager _resourceListManager;
		private ObservableCollection<Resource> _resources;
		private ItemNotificationCollection<ExchangeUser> _users;

		#endregion  // Member Variables

		#region Constructors

		/// <summary>
		/// Initializes a new <see cref="ExchangeScheduleDataConnector"/> instance.
		/// </summary>
		public ExchangeScheduleDataConnector()
		{
			_defaultErrorCallback = this.DefaultErrorCallbackMethod;

			_activityQueryResults = new WeakList<ExchangeActivityQueryResult>();
			_cachedActvities = new WeakDictionary<string, ActivityBase>(false, true);
			_cancelledConnections = new List<ExchangeService>();
			_occurrencesByRoot = new WeakDictionary<ActivityBase, WeakList<ActivityBase>>(true, false);
			_pendingConnections = new List<ExchangeService>();

			_initializeConnection = new DeferredOperation(this.InitializeConnection);

			_pollingTimer = new DispatcherTimer();
			_pollingTimer.Tick += new EventHandler(this.OnPollingTimerTick);
			this.UpdatePollingInterval(this.PollingInterval);
		}

		#endregion  // Constructors

		#region Base Class Overrides

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
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>
		/// <seealso cref="IsActivityOperationSupported(ActivityType, ActivityOperation, ResourceCalendar)"/>
		protected internal override bool AreActivitiesSupported(ActivityType activityType)
		{
			switch (activityType)
			{
				case ActivityType.Appointment:
				case ActivityType.Journal:
				case ActivityType.Task:
					return true;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown activity type: " + activityType);
					return false;
			}
		} 

		#endregion  // AreActivitiesSupported

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
		/// calls should result an error and return false.
		/// </para>
		/// </remarks>
		protected internal override bool BeginEdit(ActivityBase activity, out DataErrorInfo errorInfo)
		{
			if (base.BeginEdit(activity, out errorInfo) == false)
				return false;

			if (errorInfo != null)
				return false;

			ItemType item = activity.DataItem as ItemType;
			if (item == null)
				return true;

			ActivityBase editClone = activity.GetValueHelper<ActivityBase>(ActivityBase.StorageProps.EditClone);

			if (editClone == null ||
				ExchangeScheduleDataConnector.ShouldQueryOriginalDescriptionFormat(editClone.OriginalDescriptionFormat))
			{
				this.InitializeOriginalDescriptionFormat(activity);
			}

			// Copy the original item so we can determine what changed.
			activity.BeginEditData.DataItem = item.Clone();

			return true;
		} 

		#endregion  // BeginEdit

		#region CancelEdit

		/// <summary>
		/// Cancels modifications to an existing activity or cancels a new activity that was created by the 
		/// <see cref="CreateNew"/> call however one that hasn't been committed yet.
		/// </summary>
		/// <param name="activity">ActivityBase derived object that was put in edit state by <see cref="BeginEdit(ActivityBase, out DataErrorInfo)"/> call
		/// or one that was created using <see cref="CreateNew"/> method however one that hasn't been committed yet.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>True to indicate that the operation was successful, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>CancelEdit</b> method is called by the <see cref="XamScheduleDataManager"/>
		/// to cancel modifications to an existing activity. It also calls this method to cancel and discard a 
		/// new activity that was created using the <see cref="CreateNew"/> method, however the activity must 
		/// not have been committed yet. This is typically done when the user cancels the dialog for creating 
		/// a new activity, like the new appointment dialog.
		/// </para>
		/// </remarks>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		protected internal override bool CancelEdit(ActivityBase activity, out DataErrorInfo errorInfo)
		{
			if (base.CancelEdit(activity, out errorInfo) == false)
				return false;

			if (activity.OriginalDescriptionFormat == DescriptionFormat.Unknown)
				activity.OriginalDescriptionFormat = DescriptionFormat.Default;

			return true;
		} 

		#endregion  // CancelEdit

		#region CancelPendingOperation

		/// <summary>
		/// Cancels a pending operation.
		/// </summary>
		/// <param name="operation">Pending operation that is to be canceled.</param>
		/// <returns>True to indicate that the operation was successful, False otherwise.</returns>
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
		/// it's not necessary to retrieve activities for the previously visible date range. In such a case,
		/// the previous query operation will be canceled if it's still pending.
		/// </para>
		/// </remarks>
		protected internal override CancelOperationResult CancelPendingOperation(OperationResult operation)
		{
			
			
			return new CancelOperationResult(operation, null, true);
		} 

		#endregion  // CancelPendingOperation

		#region CreateNew

		/// <summary>
		/// Creates a new ActivityBase derived instance based on the activityType parameter.
		/// </summary>
		/// <param name="activityType">Indicates the type of activity to create.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A new ActivityBase derived object created according to the activityType parameter.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>CreateActivity</b> creates a new <see cref="ActivityBase"/> derived object, like Appointment, Journal, or Task. 
		/// Which activity type to create is specified by the <paramref name="activityType"/> parameter. Note that the created activity 
		/// doesn't get committed to the data source until <see cref="EndEdit(ActivityBase, bool)"/> method is called. Also if you wish to
		/// not commit the created activity then it is necessary to call <see cref="CancelEdit(ActivityBase, out DataErrorInfo)"/> 
		/// so the activity object can be properly discarded by the the data connector.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> <b>CreateNew</b> method is called by the <see cref="XamScheduleDataManager"/> to create a 
		/// new Appointment, Journal, or Task object. This is typically done when the user initiates creation 
		/// of a new activity in one of the calendar view controls. If the user commits the appointment then 
		/// <i>EndEdit</i> method is called to commit the activity. If the user cancels the activity creation 
		/// then <i>CancelEdit</i> method is called to discard the activity object.
		/// </para>
		/// </remarks>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="Remove"/>
		protected internal override ActivityBase CreateNew(ActivityType activityType, out DataErrorInfo errorInfo)
		{
			errorInfo = null;

			ActivityBase activity;
			switch (activityType)
			{
				case ActivityType.Appointment:
					activity = new Appointment();
					break;

				case ActivityType.Journal:
					activity = new Journal();
					break;

				case ActivityType.Task:
					activity = new Task();
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ActivityType: " + activityType);
					errorInfo = DataErrorInfo.CreateError(null, ExchangeConnectorUtilities.GetString("LE_CouldNotCreateActivity"));
					return null;
			}

			activity.Initialize(this);
			return activity;
		} 

		#endregion  // CreateNew

		#region EndEdit

		/// <summary>
		/// Commits a new or modified activity.
		/// </summary>
		/// <param name="activity">A new or modified ActivityBase derived instance.</param>
		/// <returns><see cref="ActivityOperationResult"/> instance which may be initialized with the result
		/// asynchronously.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>EndEdit</b> method is used to commit a modified activity, or a new activity that 
		/// was created using the <see cref="CreateNew"/> method.
		/// </para>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode and therefore the
		/// edit operation must be ended.
		/// </param>
		/// <para class="body">
		/// <b>Note</b> that the operation of committing an activity can be performed either synchronously
		/// or asynchronously. If the operation is performed synchronously then the information regarding
		/// the result of the operation will be contained in the returned <i>ActivityOperationResult</i>
		/// instance. If the operation is performed asynchronously, the method will return an 
		/// <i>ActivityOperationResult</i> instance whose results will be initialized later when they
		/// are available via the ActivityOperationResult's <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>
		/// method. The caller, which may be a schedule control, will indicate via the UI that the operation
		/// is pending and when the results are initialized (via <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>), 
		/// it will show the user with appropriate status of the operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		protected internal override ActivityOperationResult EndEdit(ActivityBase activity, bool force)
		{
			return EndEditForActivitiesHelper.Execute(this, activity);
		}

		/// <summary>
		/// Commits a modified Resource object.
		/// </summary>
		/// <param name="resource">A modified Resource object.</param>
		/// <returns><see cref="OperationResult"/> instance which may be initialized with the result
		/// asynchronously.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>EndEdit</b> method is used to commit a modified Resource object.
		/// </para>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode and therefore the
		/// edit operation must be ended.
		/// </param>
		/// <para class="body">
		/// <b>Note</b> that the operation of committing an activity can be performed either synchronously
		/// or asynchronously. If the operation is performed synchronously then the information regarding
		/// the result of the operation will be contained in the returned <i>OperationResult</i>
		/// instance. If the operation is performed asynchronously, the method will return an 
		/// <i>OperationResult</i> instance whose results will be initialized later when they
		/// are available via the OperationResult's <see cref="OperationResult.InitializeResult"/>
		/// method. The caller, which may be a schedule control, will indicate via the UI that the operation
		/// is pending and when the results are initialized (via <see cref="OperationResult.InitializeResult"/>), 
		/// it will show the user with appropriate status of the operation.
		/// </para>
		/// </remarks>
		protected internal override ResourceOperationResult EndEdit(Resource resource, bool force)
		{
			return EndEditForResourcesHelper.Execute(this, resource);
		}

		#endregion  // EndEdit

		#region GetActivities

		/// <summary>
		/// Gets activities that meet the criteria specified by the <i>query</i> parameter.
		/// </summary>
		/// <param name="query">Query criteria - contains information about which activities to get.</param>
		/// <returns><see cref="ActivityQueryResult"/> object that contains the activities that meet the 
		/// criteria specified by the <i>query</i> parameter.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>GetActivities</b> method is called by the <see cref="XamScheduleDataManager"/> 
		/// to retrieve appointments, tasks, journals or a combination of the three for one or more 
		/// resources as needed to display them in calendar view controls that may be associated with
		/// it. This method may get called multiple times to satisfy
		/// multiple calendar view controls and also may get called again as dates are navigated in
		/// those calendar view controls.
		/// </para>
		/// </remarks>
		protected internal override ActivityQueryResult GetActivities(ActivityQuery query)
		{
			ExchangeActivityQueryResult queryResult = new ExchangeActivityQueryResult(query);
			this.OnActivityQueryResultCreated(queryResult);

			
			if ((query.RequestedInformation & ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges) == 0)
			{
				queryResult.InitializeResult(ActivityQueryRequestedDataFlags.None, null, null, null, null, null, null, true);
				return queryResult;
			}

			queryResult.InitializeResult(
				ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges,
				queryResult.ActivitiesInternal,
				null,
				null,
				null,
				null,
				null,
				false);

			QueryActivitiesHelper.Execute(this, queryResult);

			return queryResult;
		}

		#endregion  // GetActivities

		#region GetDefaultCategoryColors

		/// <summary>
		/// Returns the collection of colors which should be displayed in the drop down when modifying a category.
		/// </summary>
		/// <param name="areCustomCategoryColorsAllowed">Indicates whether the user can use any color when creating a custom category.</param>
		/// <returns>The collection of default colors or null if the normal default colors should be used.</returns>
		protected override IList<Color> GetDefaultCategoryColors(out bool areCustomCategoryColorsAllowed)
		{
			areCustomCategoryColorsAllowed = false;
			return ExchangeScheduleDataConnector.ExchangeCategoryColors;
		}

		#endregion //GetDefaultCategoryColors

		#region GetSupportedActivityCategories

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
		protected internal override IEnumerable<ActivityCategory> GetSupportedActivityCategories(ActivityBase activity)
		{
			return new ResolvedActivityCategoryCollection(null, null, activity, true);
		} 

		#endregion //GetSupportedActivityCategories

		#region InitializeEditCopy

		/// <summary>
		/// Allows the data connector to perform any additional initialization on the edit copy before the edit operation begins.
		/// </summary>
		/// <param name="editCopy">The copy of the original activity being edited.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>InitializeEditCopy</b> is called by the <see cref="XamScheduleDataManager.BeginEditWithCopy"/> 
		/// method after creating the edit copy, but before returning it to the caller.
		/// </para>
		/// </remarks>
		protected internal override void InitializeEditCopy(ActivityBase editCopy)
		{
			this.InitializeOriginalDescriptionFormat(editCopy);

			base.InitializeEditCopy(editCopy);
		} 

		#endregion  // InitializeEditCopy

		#region IsActivityEditingAllowed

		/// <summary>
		/// Indicates whether the specified property can be edited on the specified activity.
		/// </summary>
		/// <param name="activity">Activity instance on which the property might be edited.</param>
		/// <param name="property">The property to check whether editing is allowed.</param>
		/// <returns>True if the property can be edited. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified property can be edited for an activity of
		/// If the property cannot be edited, the appropriate part of the UI will be disabled to prevent the user from editing
		/// that property.
		/// </para>
		/// </remarks>
		protected internal override bool IsActivityEditingAllowed(ActivityBase activity, EditableActivityProperty property)
		{
			switch (property)
			{
				case EditableActivityProperty.Recurrence:
					{
						// MD 5/31/11 - TFS75816
						// We don't use the Recurrence property on Journals, so it should be allowed to be edited.
						if (activity is Journal)
							return false;

						TaskType taskItem = activity.DataItem as TaskType;
						if (taskItem != null && taskItem.IsCompleteSpecified && taskItem.IsComplete)
							return false;
					}
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown EditableActivityProperty: " + property);
					break;
			}

			return base.IsActivityEditingAllowed(activity, property);
		} 

		#endregion  // IsActivityEditingAllowed

		#region IsActivityFeatureSupported

		/// <summary>
		/// Indicates whether the specified feature is supported for the activities of the specified type.
		/// </summary>
		/// <param name="activityType">Activity type for which to check if the specified feature is supported.</param>
		/// <param name="activityFeature">Feature to check for support.</param>
		/// <param name="calendar">Resource calendar context.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified feature is supported for activities of
		/// the specified type that belong the specified calendar. If the feature is not supported, relevant user interface 
		/// will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="ScheduleDataConnectorBase.IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		protected internal override bool IsActivityFeatureSupported(ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar)
		{
			bool isActivity = false;
			bool isJournal = false;
			bool isTask = false;
			switch (activityType)
			{
				case ActivityType.Appointment:
					isActivity = true;
					break;

				case ActivityType.Journal:
					isJournal = true;
					break;

				case ActivityType.Task:
					isTask = true;
					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ActivityType: " + activityType);
					break;
			}

			switch (activityFeature)
			{
				case ActivityFeature.Recurrence:
					return isActivity || isTask;

				case ActivityFeature.Variance:
					return isActivity;

				case ActivityFeature.TimeZoneNeutrality:
					return isActivity || isTask || isJournal;

				case ActivityFeature.Reminder:
					return isActivity || isTask || isJournal;

				case ActivityFeature.CanChangeOwningCalendar:
					return isActivity || isTask || isJournal;

				case ActivityFeature.CanChangeOwningResource:
					return isActivity || isTask || isJournal;

				case ActivityFeature.EndTime:
					return isActivity || isTask || isJournal;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ActivityFeature: " + activityFeature);
					return false;
			}
		} 

		#endregion  // IsActivityFeatureSupported

		#region IsActivityOperationSupported

		/// <summary>
		/// Indicates whether the specified activity operation for the specified activity type is supported.
		/// </summary>
		/// <param name="activity">Activity instance on which the operation is going to be performed.</param>
		/// <param name="activityOperation">Activity operation.</param>
		/// <returns>True if the operation is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified operation is supported for the specified activity.
		/// This method is called when there's a context of an activity object available, for which the operation is to be performed.
		/// If there's no context of an activity available, then the overload <see cref="IsActivityOperationSupported(ActivityType, ActivityOperation, ResourceCalendar)"/>
		/// that takes in an <see cref="ActivityType"/> value is used by the data manager to determine if an operation
		/// can be performed. If the operation is not supported, relevant user interface will be disabled or hidden in 
		/// the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityOperationSupported(ActivityType, ActivityOperation, ResourceCalendar)"/>
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		protected internal override bool IsActivityOperationSupported(ActivityBase activity, ActivityOperation activityOperation)
		{
			if (base.IsActivityOperationSupported(activity, activityOperation) == false)
				return false;

			ItemType item = activity.DataItem as ItemType;
			if (item != null && item.EffectiveRights != null)
			{
				switch (activityOperation)
				{
					case ActivityOperation.Edit:
						return item.EffectiveRights.Modify;

					case ActivityOperation.Add:
						// Ignore this because it applies to folders, not items.
						break;

					case ActivityOperation.Remove:
						return item.EffectiveRights.Delete;

					default:
						ExchangeConnectorUtilities.DebugFail("Unknown ActivityOperation: " + activityOperation);
						break;
				}
			}

			return true;
		} 

		#endregion  // IsActivityOperationSupported

		#region IsActivityOperationSupported

		/// <summary>
		/// Indicates whether the specified activity operation for the specified activity type is supported.
		/// </summary>
		/// <param name="activityType">Activity type.</param>
		/// <param name="activityOperation">Activity operation.</param>
		/// <param name="calendar">ResourceCalendar for which to check if the operation can be performed.</param>
		/// <returns>True if the operation is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified operation is supported for the activities of the specified type
		/// for the specified resource calendar. If the operation is not supported, relevant user interface will be disabled or hidden in 
		/// the schedule controls.
		/// </para>
		/// <para class="body">
		/// When the context of an activity object for which the operation is being performed available, the overload
		/// <see cref="ScheduleDataConnectorBase.IsActivityOperationSupported(ActivityBase, ActivityOperation)"/> that takes in the activity is used.
		/// </para>
		/// </remarks>
		/// <seealso cref="ScheduleDataConnectorBase.IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		protected internal override bool IsActivityOperationSupported(ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar)
		{
			if (calendar == null)
				return true;

			ExchangeFolder exchangeFolder = calendar.DataItem as ExchangeFolder;

			if (exchangeFolder == null)
				return false;

			// All types are supported in the default calendar folder
			if (exchangeFolder == exchangeFolder.Service.CalendarFolder)
			{
				switch (activityType)
				{
					case ActivityType.Appointment:
						return exchangeFolder.IsActivityOperationAllowed(activityOperation);

					case ActivityType.Journal:
						return exchangeFolder.Service.JournalFolder.IsActivityOperationAllowed(activityOperation);

					case ActivityType.Task:
						return exchangeFolder.Service.TasksFolder.IsActivityOperationAllowed(activityOperation);

					default:
						ExchangeConnectorUtilities.DebugFail("Unknown ActivityType: " + activityType);
						return true;
				}
			}

			switch (activityType)
			{
				case ActivityType.Appointment:
					if ((exchangeFolder is ExchangeCalendarFolder) == false)
						return false;

					break;

				case ActivityType.Journal:
					if ((exchangeFolder is ExchangeJournalFolder) == false)
						return false;

					break;

				case ActivityType.Task:
					if ((exchangeFolder is ExchangeTasksFolder) == false)
						return false;

					break;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ActivityType: " + activityType);
					return true;
			}

			return exchangeFolder.IsActivityOperationAllowed(activityOperation);
		}

		#endregion  // IsActivityOperationSupported

		#region IsResourceFeatureSupported

		/// <summary>
		/// Indicates whether the specified feature is supported for the specified resource.
		/// </summary>
		/// <param name="resource">Resource instance.</param>
		/// <param name="resourceFeature">Resource feature.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified feature is supported for a resource.
		/// If the feature is not supported, relevant user interface will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>
		protected internal override bool IsResourceFeatureSupported(Resource resource, ResourceFeature resourceFeature)
		{
			switch (resourceFeature)
			{
				case ResourceFeature.CustomActivityCategories:
					// MD 4/27/11 - TFS72779
					//return this.RequestedServerVersionResolved > ExchangeVersion.Exchange2007_SP1;
					return EWSUtilities.AreCategoriesSupported(this.RequestedServerVersionResolved);

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown ResourceFeature: " + resourceFeature);
					break;
			}

			return base.IsResourceFeatureSupported(resource, resourceFeature);
		}

		#endregion //IsResourceFeatureSupported

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged(object sender, string propName, object extraInfo)
		{
			base.OnSubObjectPropertyChanged(sender, propName, extraInfo);

			if (sender is TimeZoneInfoProvider)
			{
				switch (propName)
				{
					case "Version":
						if (_exchangeServices != null)
						{
							// The local time zone is technically one of the server connection settings, so reinitialize all the exchange service 
							// bindings when it changes.
							this.OnServerConnectionSettingsChanged();
						}
						else if (_resourceListManager != null)
						{
							// But if we haven't initialized yet and we are at the point where we can initialize, re queue the init connection
							// async operation.
							_initializeConnection.StartAsyncOperation();
						}
						break;
				}
			}
		}

		#endregion //OnSubObjectPropertyChanged

		#region RecurringTaskGenerationBehavior

		/// <summary>
		/// Gets the value indicating how this connector generates recurring tasks.
		/// </summary>
		protected internal override RecurringTaskGenerationBehavior RecurringTaskGenerationBehavior
		{
			get { return RecurringTaskGenerationBehavior.GenerateCurrentTask; }
		} 

		#endregion  // RecurringTaskGenerationBehavior

		#region Remove

		/// <summary>
		/// Removes an activity.
		/// </summary>
		/// <param name="activity">ActivityBase derived instance to remove.</param>
		/// <returns><see cref="ActivityOperationResult"/> instance.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the operation of removing an activity can be performed either synchronously
		/// or asynchronously. If the operation is performed synchronously then the information regarding
		/// the result of the operation will be contained in the returned <i>ActivityOperationResult</i>
		/// instance. If the operation is performed asynchronously, the method will return an 
		/// <i>ActivityOperationResult</i> instance whose results will be initialized later when they
		/// are available via the ActivityOperationResult's InitializeResult
		/// method. The caller, which may be a schedule control, will indicate via the UI that the operation
		/// is pending and when the results are initialized (via InitializeResult), 
		/// it will show the user with appropriate status of the operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		protected internal override ActivityOperationResult Remove(ActivityBase activity)
		{
			return RemoveHelper.RemoveActivity(this, activity);
		} 

		#endregion  // Remove

		#region ResolveActivityCategories

		/// <summary>
		/// Gets the activity categories that are currently applied to the specified activity. 
		/// Default implementation returns <i>null</i>.
		/// </summary>
		/// <param name="activity">Activity for which to get the list of currently applied activity categories.</param>
		/// <returns>IEnumerable that can optionally implement INotifyCollectionChanged to notify
		/// of changes to the list.</returns>
		/// <seealso cref="GetSupportedActivityCategories"/>
		protected internal override IEnumerable<ActivityCategory> ResolveActivityCategories(ActivityBase activity)
		{
			return new ResolvedActivityCategoryCollection(null, null, activity, false);
		}

		#endregion //ResolveActivityCategories

		#region ResourceItems

		/// <summary>
		/// Gets the resources collection.
		/// </summary>
		public override ResourceCollection ResourceItems
		{
			get
			{
				if (_resourceListManager == null)
				{
					_resourceListManager = new ResourceListManager(this);

					ResourcePropertyMappingCollection mappings = new ResourcePropertyMappingCollection();
					mappings.UseDefaultMappings = true;
					_resourceListManager.Mappings = mappings;

					_initializeConnection.StartAsyncOperation();
				}

				return _resourceListManager.Resources;
			}
		}

		#endregion  // ResourceItems

		#region SubscribeToReminders

		/// <summary>
		/// Subscribes to reminders for the activities of the specified calendar. Note that more than one 
		/// subscriber can be subscribed to a single calendar as well as multiple calendars can be 
		/// subscribed to.
		/// </summary>
		/// <param name="calendar">This calendars activity reminders will be delivered to the specified subscribed.</param>
		/// <param name="subscriber">When a reminder is due for an activity, the subscriber's <see cref="ReminderSubscriber.DeliverReminder"/> method will be invoked.</param>
		/// <param name="error">If there's an error, this will be set to error information.</param>
		protected internal override void SubscribeToReminders(ResourceCalendar calendar, ReminderSubscriber subscriber, out DataErrorInfo error)
		{
			this.ReminderManager.SubscribeToReminders(calendar, subscriber, out error);
		} 

		#endregion  // SubscribeToReminders

		#region UnsubscribeFromReminders

		/// <summary>
		/// Unsubscribes from activity reminders of the specified calendar. If the specified subscriber hadn't been subscribed
		/// previously then this method will take no action.
		/// </summary>
		/// <param name="calendar">The calendar's activity reminders to unsubscribe from.</param>
		/// <param name="subscriber">Subscriber instance.</param>
		protected internal override void UnsubscribeFromReminders(ResourceCalendar calendar, ReminderSubscriber subscriber)
		{
			this.ReminderManager.UnsubscribeFromReminders(calendar, subscriber);
		} 

		#endregion  // UnsubscribeFromReminders

		#region VerifyInitialState

		/// <summary>
		/// Called to verify that the data connector has sufficient state to operate.
		/// </summary>
		/// <param name="errorList">A list to receive the errors.</param>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this method gets called once by the <see cref="XamScheduleDataManager"/> when it is verifying its initial state.
		/// </para>
		/// </remarks>
		protected internal override void VerifyInitialState(List<DataErrorInfo> errorList)
		{
			base.VerifyInitialState(errorList);

			if (this.ServerConnectionSettings == null ||
				this.ServerConnectionSettings.Url == null)
			{
				errorList.Add(DataErrorInfo.CreateError(this, ExchangeConnectorUtilities.GetString("LE_NoUrlSpecified")));
			}

			bool hasAtLeastOneUser = false;


			if (this.UseDefaultCredentials)
				hasAtLeastOneUser = true;


			IEnumerable<ExchangeUser> users = this.Users;
			if (hasAtLeastOneUser == false && users != null)
			{
				foreach (ExchangeUser user in users)
				{
					hasAtLeastOneUser = true;
					break;
				}
			}

			if (hasAtLeastOneUser == false)
				errorList.Add(DataErrorInfo.CreateError(this, ExchangeConnectorUtilities.GetString("LE_NoUsersSpecified")));
		}

		#endregion  // VerifyInitialState

		#endregion  // Base Class Overrides

		#region Methods

		#region Public Methods

		#region Connect

		/// <summary>
		/// Forces a connection to the Exchange server. If the data connector is already connected to the server, this method does nothing.
		/// </summary>
		/// <seealso cref="Disconnect"/>
		public void Connect()
		{
			if (_exchangeServices != null)
			{
				// If we already have active services, we are already connected, so we don't have to do anything.
				if (_exchangeServices.Count > 0)
					return;

				_exchangeServices = null;
			}

			this.InitializeConnection();
		} 

		#endregion  // Connect

		#region Disconnect

		/// <summary>
		/// Disconnects the data connector from the Exchange server.
		/// </summary>
		/// <seealso cref="Connect"/>
		public void Disconnect()
		{
			this.RemoveAllExchangeServices();

			// MD 4/11/11 - TFS72059
			Debug.Assert(_cachedActvities.Count == 0, "We should have no more cached activities.");
			Debug.Assert(_occurrencesByRoot.Count == 0, "We should have no more occurrences cached by the root.");
		} 

		#endregion  // Disconnect

		#region PollForChanges

		/// <summary>
		/// Forces the connector to poll the Exchange server for changes.
		/// </summary>
		/// <remarks>
		/// This can be called at any time, regardless of the <see cref="PollingInterval"/> value.
		/// </remarks>
		/// <seealso cref="PollingInterval"/>
		public void PollForChanges()
		{
			this.IncrementPollingCount();
			PollForChangesHelper.Execute(this, this.DecrementPollingCount);
		}

		#endregion  // PollForChanges

		// MD 10/21/11 - TFS87807
		#region PreInitialize

		/// <summary>
		/// Performs some initialization on a background thread to shorten initial loading time. 
		/// This only needs to be called once when an application first starts.
		/// </summary>
		public static void PreInitialize()
		{
			if (ExchangeServiceBindingInternal.HasAnyServiceBindingBeenCreated == false)
			{
				new Thread(new ThreadStart(() => 
				{
					if (ExchangeServiceBindingInternal.HasAnyServiceBindingBeenCreated == false)
					{
						new ExchangeServiceBindingInternal();
					}
				})).Start();
			}
		}

		#endregion  // PreInitialize

		#endregion  // Public Methods

		#region Internal Methods

		#region FindServieAssociatedWithResource

		internal ExchangeService FindServiceAssociatedWithResource(Resource resource)
		{
			if (_exchangeServices == null || resource == null)
				return null;

			for (int i = 0; i < _exchangeServices.Count; i++)
			{
				ExchangeService service = _exchangeServices[i];

				if (service.Resource == resource)
					return service;
			}

			return null;
		}

		#endregion //FindServieAssociatedWithResource

		// MD 4/5/12 - TFS101338
		#region GetCalendars

		internal IList<ResourceCalendar> GetCalendars(ActivityQuery query)
		{
			if (query.Calendars != null && query.Calendars.Count != 0)
				return query.Calendars;

			List<ResourceCalendar> calendars = new List<ResourceCalendar>();

			foreach (Resource resource in this.ResourcesInternal)
				calendars.AddRange(resource.Calendars);

			return calendars;
		}

		#endregion // GetCalendars

		#region GetDataErrorInfo

		internal DataErrorInfo GetDataErrorInfo(RemoteCallErrorReason reason, Exception error, object context = null)
		{
			switch (reason)
			{
				case RemoteCallErrorReason.Cancelled:
					return DataErrorInfo.CreateError(context, ExchangeConnectorUtilities.GetString("LE_RemoteCallCancelledWhileAddingActivity"));

				case RemoteCallErrorReason.Error:
					return new DataErrorInfo(error) { Context = context, Severity = ErrorSeverity.Error };

				case RemoteCallErrorReason.Warning:
					return new DataErrorInfo(error) { Context = context, Severity = ErrorSeverity.Warning };

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown error reason: " + reason);
					return DataErrorInfo.CreateError(context, ExchangeConnectorUtilities.GetString("LE_UnknownError"));
			}
		}

		#endregion  // GetDataErrorInfo

		#region GetDateRangesAndCalendarsWhichMayHaveOccurrences

		internal void GetDateRangesAndCalendarsWhichMayHaveOccurrences(
			List<ActivityBase> recurringActivityRoots,
			out IEnumerable<DateRange> dateRanges,
			out IEnumerable<ResourceCalendar> calendars)
		{
			List<DateRange> dateRangesList = new List<DateRange>();
			Dictionary<ResourceCalendar, object> calendarsDictionary = new Dictionary<ResourceCalendar, object>();

			TimeZoneInfoProvider timeZoneInfoProvider = this.TimeZoneInfoProviderResolved;

			// Determine all the date ranges and calendars which need to be requeried to see if a recurring item may have an 
			// occurrence in one of our queries.
			ExchangeActivityQueryResult[] queryResults = _activityQueryResults.ToArray();
			for (int i = 0; i < queryResults.Length; i++)
			{
				ExchangeActivityQueryResult queryResult = queryResults[i];

				if (queryResult == null)
					continue;

				
				if ((queryResult.Query.RequestedInformation & ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges) == 0)
					continue;

				// Add any date ranges which can contain occurrences to the list of date ranges.
				bool isUsingAnyRanges = false;
				foreach (DateRange dateRange in queryResult.Query.DateRanges)
				{
					if (ExchangeConnectorUtilities.CanOccurrencesBeInDateRange(dateRange, recurringActivityRoots, timeZoneInfoProvider) == false)
						continue;

					isUsingAnyRanges = true;
					dateRangesList.Add(dateRange);
				}

				if (isUsingAnyRanges)
				{
					// If we are using any of the date ranges in the query, add all calendars from the query to the collection 
					// of calendars.
					// MD 4/5/12 - TFS101338
					//foreach (ResourceCalendar calendar in queryResult.Query.Calendars)
					foreach (ResourceCalendar calendar in this.GetCalendars(queryResult.Query))
						calendarsDictionary[calendar] = null;
				}
			}

			dateRanges = ExchangeConnectorUtilities.CombineRanges(dateRangesList);
			calendars = calendarsDictionary.Keys;
		}

		#endregion  // GetDateRangesAndCalendarsWhichMayHaveOccurrences

		#region GetOwningExchangeFolder

		internal ExchangeFolder GetOwningExchangeFolder(ActivityBase activity)
		{
			ExchangeFolder exchangeFolder = this.GetOwningExchangeFolderHelper(activity);

			if (exchangeFolder != null)
				return exchangeFolder;

			ExchangeService service = this.FindServiceAssociatedWithResource(activity.OwningResource);

			if (service == null && _exchangeServices.Count != 0)
				service = _exchangeServices[0];

			if (service == null)
				return null;

			switch (activity.ActivityType)
			{
				case ActivityType.Appointment:
					return service.CalendarFolder;

				case ActivityType.Journal:
					return service.JournalFolder;

				case ActivityType.Task:
					return service.TasksFolder;

				default:
					ExchangeConnectorUtilities.DebugFail("Unknown activity type: " + activity.ActivityType);
					goto case ActivityType.Appointment;
			}
		}

		private ExchangeFolder GetOwningExchangeFolderHelper(ActivityBase activity)
		{
			if (activity == null)
				return null;

			ResourceCalendar owningCalendar = activity.OwningCalendar;

			if (owningCalendar == null)
				return null;

			return owningCalendar.DataItem as ExchangeFolder;
		}

		#endregion  // GetOwningExchangeFolder

		#region OnActivityQueryResultCreated

		internal void OnActivityQueryResultCreated(ExchangeActivityQueryResult queryResult)
		{
			_activityQueryResults.Add(queryResult);

			if (_activityQueryResults.Count >= _activityQueryResultsCompactLimit)
			{
				_activityQueryResults.Compact();
				_activityQueryResultsCompactLimit = _activityQueryResults.Count * 2;
			}
		}

		#endregion  // OnActivityQueryResultCreated

		#region OnItemReceivedFromServer

		internal ActivityBase OnItemReceivedFromServer(
			ExchangeService service,
			ItemType item,
			bool isItemFromRemindersFolder,
			out bool needsRecurringMasterItem)
		{
			needsRecurringMasterItem = false;

			// MD 2/2/11 - TFS64549
			// Moved this check from below and the value is now stored so we can use it in multiple places.
			bool isOccurrence = false;
			CalendarItemType calendarItem = item as CalendarItemType;
			if (calendarItem != null &&
				calendarItem.CalendarItemType1Specified &&
				(calendarItem.CalendarItemType1 == CalendarItemTypeType.Occurrence || calendarItem.CalendarItemType1 == CalendarItemTypeType.Exception))
			{
				isOccurrence = true;
			}

			ActivityBase activity;
			if (_cachedActvities.TryGetValue(item.ItemId.Id, out activity))
			{
				ItemType existingItem = (ItemType)activity.DataItem;
				Debug.Assert(existingItem.ItemId.Id == item.ItemId.Id, "This is not the same logical calendar item.");

				// MD 4/11/11 - TFS72059
				Debug.Assert(activity.OwningResource == service.Resource, "This activity belongs to a different resource.");

				bool wasRecurringMaster = ExchangeConnectorUtilities.IsRecurringMaster(activity);

				if (existingItem.ItemId.ChangeKey != item.ItemId.ChangeKey)
					EWSUtilities.UpdateActivity(activity, item, this.TimeZoneInfoProviderResolved);

				// MD 2/2/11 - TFS64549
				// If this is an occurrence and we will haven't retrieved the root activity from the server, 
				// return it, but don't add it to the query results yet.
				if (isOccurrence && activity.RootActivity == null)
					return activity;

				this.VerifyItemsInQueryResults(activity, null, isItemFromRemindersFolder, wasRecurringMaster, true);
				return activity;
			}

			ResourceCalendar resourceCalendar = service.FindResourceCalendarWithId(item.ParentFolderId);
			if (resourceCalendar == null)
				return null;

			// MD 4/11/11 - TFS72059
			Debug.Assert(resourceCalendar.OwningResource == service.Resource, "The calendar doesn't belong to the correct resource.");

			activity = EWSUtilities.ActivityFromItem(item, resourceCalendar, this.TimeZoneInfoProviderResolved);

			if (activity == null)
				return null;

			activity.Initialize(this);
			_cachedActvities.Add(activity.Id, activity);

			// MD 4/11/11
			// Found while fixing TFS72059
			// We should be compacting the cached activities collection when it gets too large.
			if (_cachedActvities.Count >= _cachedActvitiesCompactLimit)
			{
				_cachedActvities.Compact(true);
				_cachedActvitiesCompactLimit = _cachedActvities.Count * 2;
			}

			// If we have a new occurrence, don't add it to the other query results just yet. 
			// It is not complete until we have the associated root activity.
			// MD 2/2/11 - TFS64549
			// This check was moved above and now its value is stored in isOccurrence, so just check that.
			//CalendarItemType calendarItem = item as CalendarItemType;
			//if (calendarItem != null &&
			//    calendarItem.CalendarItemType1Specified &&
			//    (calendarItem.CalendarItemType1 == CalendarItemTypeType.Occurrence || calendarItem.CalendarItemType1 == CalendarItemTypeType.Exception))
			//{
			if (isOccurrence)
			{
				needsRecurringMasterItem = true;
				return activity;
			}

			this.VerifyItemsInQueryResults(activity, true, isItemFromRemindersFolder);
			return activity;
		}

		#endregion  // OnItemReceivedFromServer

		#region OnItemsReceivedFromServer



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		internal void OnItemsReceivedFromServer(
			ExchangeFolder folder,
			IList<ItemType> items,
			ref int pendingCallCount,
			out List<ActivityBase> recurringActivityRoots,
			Action onQueryForRecurringMastersErrorCompleted = null,
			ErrorCallback onQueryForRecurringMastersError = null)
		{
			List<ActivityBase> occurrencesNeedingMasterItems = new List<ActivityBase>();
			List<ItemType> occurrenceItemsNeedingMasterItems = new List<ItemType>();
			recurringActivityRoots = new List<ActivityBase>();

			for (int i = 0; i < items.Count; i++)
			{
				bool needsRecurringMasterItem;
				ActivityBase activity = this.OnItemReceivedFromServer(
					folder.Service,
					items[i],
					folder.IsRemindersFolder,
					out needsRecurringMasterItem);

				if (activity == null)
				{
					ExchangeConnectorUtilities.DebugFail("Could not get the activity.");
					continue;
				}

				if (activity.IsRecurrenceRoot)
				{
					recurringActivityRoots.Add(activity);
				}
				else if (needsRecurringMasterItem)
				{
					occurrencesNeedingMasterItems.Add(activity);
					occurrenceItemsNeedingMasterItems.Add((ItemType)activity.DataItem);
				}
			}

			// If we have any new occurrences or variances from the server, we need to find their associated recurring 
			// master items before adding them into the query results, so ask the server for those recurring masters.
			if (occurrencesNeedingMasterItems.Count > 0)
			{
				if (onQueryForRecurringMastersError == null)
					onQueryForRecurringMastersError = this._defaultErrorCallback;

				pendingCallCount++;
				folder.Service.FindRecurringMasterCalendarItems(
					folder,
					occurrenceItemsNeedingMasterItems,
					(folder2, recurringMasterItems) =>
					{
						for (int i = 0; i < recurringMasterItems.Count; i++)
						{
							CalendarItemType recurringMasterItem = recurringMasterItems[i];
							ActivityBase occurrence = occurrencesNeedingMasterItems[i];

							bool needsRecurringMasterItem;
							ActivityBase rootActivity = this.OnItemReceivedFromServer(
								folder2.Service, 
								recurringMasterItem, 
								folder2.IsRemindersFolder, 
								out needsRecurringMasterItem);

							if (rootActivity == null)
							{
								ExchangeConnectorUtilities.DebugFail("Could not get the activity.");
								continue;
							}

							Debug.Assert(needsRecurringMasterItem == false, "The root activity should not need to get its recurring master item.");

							this.OnOccurrenceCreated(rootActivity, occurrence);

							this.VerifyItemsInQueryResults(occurrence, true, false);
						}

						if (onQueryForRecurringMastersErrorCompleted != null)
							onQueryForRecurringMastersErrorCompleted();
					},
					onQueryForRecurringMastersError);
			}
		}

		#endregion  // OnItemsReceivedFromServer

		#region OnRecurringItemsAddedOrModified

		internal void OnRecurringItemsAddedOrModified(
			List<ActivityBase> recurringActivityRoots,
			Action<ExchangeFolder, IList<ItemType>> onOccurrencesReceived,
			Action onOccurrencesQueryCompleted,
			ErrorCallback onError)
		{
			if (recurringActivityRoots.Count == 0)
			{
				if (onOccurrencesQueryCompleted != null)
					onOccurrencesQueryCompleted();

				return;
			}

			GetOcurrencesInDateRangesForCalendarHelper.Execute(this, recurringActivityRoots, 
				onOccurrencesReceived, onOccurrencesQueryCompleted, onError);
		}

		#endregion  // OnRecurringItemsAddedOrModified

		#region OnServerConnectionSettingsChanged

		internal void OnServerConnectionSettingsChanged()
		{
			ExchangeServerConnectionSettings settings = this.ServerConnectionSettings;

			if (settings != null)
				this._isVersionAutoDetected = (settings.RequestedServerVersion == ExchangeVersion.AutoDetect);

			if (_exchangeServices == null)
			{
				if (_resourceListManager != null)
					_initializeConnection.StartAsyncOperation();

				return;
			}

			if (settings == null)
				return;

			string timeZoneId = this.TimeZoneInfoProviderResolved.LocalTimeZoneIdResolved;

			for (int i = 0; i < _exchangeServices.Count; i++)
			{
				// MD 5/31/11 - TFS77450
				// If the server version is already auto detected, we need to know what it is.
				//EWSUtilities.InitializeServiceBinding(_exchangeServices[i].ServiceBinding, settings, timeZoneId);
				EWSUtilities.InitializeServiceBinding(_exchangeServices[i].ServiceBinding, settings, timeZoneId, _autoDetectedServerVersion);
			}

			for (int i = 0; i < _pendingConnections.Count; i++)
			{
				// MD 5/31/11 - TFS77450
				// If the server version is already auto detected, we need to know what it is.
				//EWSUtilities.InitializeServiceBinding(_pendingConnections[i].ServiceBinding, settings, timeZoneId);
				EWSUtilities.InitializeServiceBinding(_pendingConnections[i].ServiceBinding, settings, timeZoneId, _autoDetectedServerVersion);
			}
		}

		#endregion  // OnServerConnectionSettingsChanged

		#region RequeryAll

		internal void RequeryAll()
		{
			ExchangeActivityQueryResult[] queryResults = _activityQueryResults.ToArray();
			for (int i = 0; i < queryResults.Length; i++)
			{
				ExchangeActivityQueryResult queryResult = queryResults[i];

				if (queryResult == null)
					continue;

				
				if ((queryResult.Query.RequestedInformation & ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges) == 0)
					continue;

				QueryActivitiesHelper.Execute(this, queryResult);
			}
		} 

		#endregion  // RequeryAll

		// MD 4/27/11 - TFS72779
		#region SetAutoDetectedVersion

		internal void SetAutoDetectedVersion(ExchangeVersion versionValue)
		{
			_autoDetectedServerVersion = versionValue;

			if (_isVersionAutoDetected)
			{
				if (_exchangeServices != null)
				{
					for (int i = 0; i < _exchangeServices.Count; i++)
						_exchangeServices[i].SetAutoDetectedVersion(versionValue);
				}

				if (_pendingConnections != null)
				{
					for (int i = 0; i < _pendingConnections.Count; i++)
						_pendingConnections[i].SetAutoDetectedVersion(versionValue);
				}
			}
		}

		#endregion  // SetAutoDetectedVersion

		#region VerifyItemsInQueryResults

		internal void VerifyItemsInQueryResults(ActivityBase activity, bool? add, bool isChangeInRemindersFolder, bool wasRecurringMaster = false, bool removeRecurringMasters = false)
		{
			bool isAdd = add.HasValue && add.Value;
			bool isRemove = add.HasValue && !add.Value;
			bool change = !add.HasValue;

			TimeZoneInfoProvider timeZoneInfoProvider = this.TimeZoneInfoProviderResolved;

			ExchangeActivityQueryResult[] queryResults = _activityQueryResults.ToArray();

			for (int i = 0; i < queryResults.Length; i++)
			{
				ExchangeActivityQueryResult result = queryResults[i];

				if (result == null)
					continue;

				
				if ((result.Query.RequestedInformation & ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges) == 0)
					continue;

				bool isRecurringMaster = ExchangeConnectorUtilities.IsRecurringMaster(activity);

				bool isInQueryRange = false;
				if (result.IsRemindersQueryResult)
				{
					isInQueryRange =
						isRecurringMaster == false &&
						activity.ReminderEnabled;

					// Reminders shouldn't be shown for occurrences in the past.
					// Also, If this is an occurrence, and the root item says the reminder is due after this occurrence's end,
					// the reminder should be displayed for a future occurrence and not this one, so exclude it.
					if (isInQueryRange && activity.IsOccurrence)
					{
						ItemType item = activity.DataItem as ItemType;
						DateTime? itemEnd = EWSUtilities.GetEndTime(item);

						// MD 11/14/11 - TFS81049
						// The item end will be in local time when returned from an Exchange 2010 server and UTC time from an 
						// Exchange 2007 server, so we may need to convert here. Also, some of the old code here wasn't checking
						// for itemEnd to have a value.
						#region Old Code

						//if (itemEnd.HasValue && itemEnd <= DateTime.UtcNow)
						//{
						//    isInQueryRange = false;
						//}
						//else
						//{
						//    ItemType rootItem = activity.RootActivity.DataItem as ItemType;
						//
						//    if (rootItem != null &&
						//        rootItem.ReminderDueBySpecified &&
						//        itemEnd <= rootItem.ReminderDueBy)
						//    {
						//        isInQueryRange = false;
						//    }
						//}

						#endregion  // Old Code
						if (itemEnd.HasValue)
						{
							DateTime itemEndResolved = itemEnd.Value;

							if (itemEndResolved.Kind == DateTimeKind.Local)
							{
								itemEndResolved = timeZoneInfoProvider.ConvertTime(
									timeZoneInfoProvider.LocalToken,
									timeZoneInfoProvider.UtcToken,
									DateTime.SpecifyKind(itemEndResolved, DateTimeKind.Unspecified));
							}

							if (itemEndResolved <= DateTime.UtcNow)
							{
								isInQueryRange = false;
							}
							else
							{
								ItemType rootItem = activity.RootActivity.DataItem as ItemType;

								if (rootItem != null &&
									rootItem.ReminderDueBySpecified &&
									itemEndResolved <= rootItem.ReminderDueBy)
								{
									isInQueryRange = false;
								}
							}
						}

						// MD 12/5/11 - TFS81049
						// Make sure the occurrence starts after the time of the last displayed reminder so we don't show a 
						// duplicate reminders.
						if (isInQueryRange)
						{
							Reminder reminder = activity.RootActivity.Reminder;
							if (reminder != null)
								isInQueryRange = activity.Start > reminder.LastDisplayedTime;
						}
					}
				}
				else
				{
					// If the item was added or removed from the reminders folder, it won't affect the other query 
					// results, so skip them.
					if (isChangeInRemindersFolder)
						continue;

					// MD 4/5/12 - TFS101338
					//if (result.Query.Calendars.Contains(activity.OwningCalendar))
					if (this.GetCalendars(result.Query).Contains(activity.OwningCalendar))
					{
						foreach (DateRange range in result.Query.DateRanges)
						{
							if (ExchangeScheduleDataConnector.IsActivityInDateRange(activity, range, timeZoneInfoProvider))
							{
								isInQueryRange = true;
								break;
							}
						}
					}
				}

				if (isRecurringMaster)
				{
					// If the activity was a single activity and a recurrence was added to it, it should be removed.
					if (removeRecurringMasters)
						result.RemoveActivity(activity);

					if (isRemove)
						result.RemoveAllOccurrences(activity);
				}
				else
				{
					// If the activity was a recurring master and no longer is, remove all its occurrences from the query results.
					if (wasRecurringMaster)
						result.RemoveAllOccurrences(activity);

					if (change)
					{
						if (isInQueryRange)
							result.AddActivity(activity);
						else
							result.RemoveActivity(activity);
					}
					else if (isInQueryRange)
					{
						if (isAdd)
							result.AddActivity(activity);
						else if (isRemove)
							result.RemoveActivity(activity);
					}
				}
			}
		}

		#endregion  // VerifyItemsInQueryResults 

		#endregion  // Internal Methods

		#region Private Methods

		#region DecrementPollingCount

		private void DecrementPollingCount()
		{
			_pollingCount--;
			if (_pollingCount == 0)
				this.IsPolling = false;
		}

		#endregion  // DecrementPollingCount

		#region DefaultErrorCallback

		private bool DefaultErrorCallbackMethod(RemoteCallErrorReason reason, ResponseCodeType serverResponseCode, Exception error)
		{
			this.RaiseError(this.GetDataErrorInfo(reason, error));
			return false;
		}

		#endregion  // DefaultErrorCallback

		#region IsActivityInDateRange

		private static bool IsActivityInDateRange(ActivityBase activity, DateRange dateRange, TimeZoneInfoProvider timeZoneInfoProvider)
		{
			DateTime activityStart = ExchangeConnectorUtilities.GetActualStartTimeUtc(activity, timeZoneInfoProvider);
			DateTime activityEnd = ExchangeConnectorUtilities.GetActualEndTimeUtc(activity, timeZoneInfoProvider);

			if (activityStart == activityEnd && dateRange.Start == activityStart)
				return true;

			return dateRange.Start < activityEnd && activityStart < dateRange.End;
		}

		#endregion  // IsActivityInDateRange

		#region IncrementPollingCount

		private void IncrementPollingCount()
		{
			if (_pollingCount == 0)
				this.IsPolling = true;

			_pollingCount++;
		} 

		#endregion  // IncrementPollingCount

		#region InitializeOriginalDescriptionFormat

		private void InitializeOriginalDescriptionFormat(ActivityBase activity)
		{
			if (ExchangeScheduleDataConnector.ShouldQueryOriginalDescriptionFormat(activity.OriginalDescriptionFormat) == false)
				return;

			// If this is a new activity being edited, we are going to set the body in Text format, 
			// so set that as the original description.
			if (activity.IsAddNew)
			{
				
				activity.OriginalDescriptionFormat = DescriptionFormat.Text;
				return;
			}

			ItemType item = activity.DataItem as ItemType;
			if (item == null)
			{
				ActivityBase editClonedFrom = activity.GetValueHelper<ActivityBase>(ActivityBase.StorageProps.EditClonedFrom);
				if (editClonedFrom == null)
					return;

				item = editClonedFrom.DataItem as ItemType;
				if (item == null)
				{
					ExchangeConnectorUtilities.DebugFail("This activity has no data item associated with it.");
					return;
				}
			}

			DetermineBodyFormatHelper.Execute(this, activity);
		}

		#endregion  // InitializeOriginalDescriptionFormat

		#region InitializeConnection

		private void InitializeConnection()
		{
			// If we are already connected, we don't have to do anything here.
			if (_exchangeServices != null)
				return;

			if (this.ServerConnectionSettings == null ||
				this.ServerConnectionSettings.Url == null ||
				this.TimeZoneInfoProviderResolved.LocalToken == null)
			{
				return;
			}

			_exchangeServices = new List<ExchangeService>();


			if (this.UseDefaultCredentials)
				this.InitializeUser(null);

			IEnumerable<ExchangeUser> users = this.Users;

			if (users != null)
			{
				foreach (ExchangeUser user in users)
				{
					if (user == null)
						continue;


					// If the default credentials are being used and the current user is the logged in user, skip it so we only have 
					// one resource representing this user.
					if (this.UseDefaultCredentials &&
						string.Equals(user.Domain, Environment.UserDomainName, StringComparison.InvariantCultureIgnoreCase) &&
						string.Equals(user.UserName, Environment.UserName, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					} 


					this.InitializeUser(user);
				}
			}
		}

		#endregion // InitializeConnection

		#region InitializeUser

		private void InitializeUser(ExchangeUser user)
		{
			ExchangeServerConnectionSettings settings = this.ServerConnectionSettings;
			if (settings == null)
				return;

			Resource resource = new Resource();

			string localTimeZoneId = this.TimeZoneInfoProviderResolved.LocalTimeZoneIdResolved;

			ExchangeServiceBindingInternal binding;

			if (user == null)
			{




				// MD 5/31/11 - TFS77450
				// If the server version is already auto detected, we need to know what it is.
				//binding = EWSUtilities.CreateExchangeServiceBindingForDefaultUser(settings, localTimeZoneId);
				binding = EWSUtilities.CreateExchangeServiceBindingForDefaultUser(settings, localTimeZoneId, _autoDetectedServerVersion);

				resource.Id = ExchangeConnectorUtilities.CreateResourceId(Environment.UserDomainName.ToLower(), Environment.UserName.ToLower());

			}
			else
			{
				// MD 5/31/11 - TFS77450
				// If the server version is already auto detected, we need to know what it is.
				//binding = EWSUtilities.CreateExchangeServiceBindingForUser(user, settings, localTimeZoneId);
				binding = EWSUtilities.CreateExchangeServiceBindingForUser(user, settings, localTimeZoneId, _autoDetectedServerVersion);

				resource.Id = ExchangeConnectorUtilities.CreateResourceId(user.Domain, user.UserName);
			}

			ExchangeService exchangeService = new ExchangeService(this, binding, resource, user);
			_pendingConnections.Add(exchangeService);

			InitializeUserHelper.Execute(resource, exchangeService);
		}

		#endregion  // InitializeUser

		#region OnOccurrenceCreated

		private void OnOccurrenceCreated(ActivityBase rootActivity, ActivityBase occurrence)
		{
			occurrence.RootActivity = rootActivity;
			occurrence.RootActivityId = rootActivity.Id;

			WeakList<ActivityBase> occurrences;
			if (_occurrencesByRoot.TryGetValue(rootActivity, out occurrences) == false)
			{
				occurrences = new WeakList<ActivityBase>();
				_occurrencesByRoot.Add(rootActivity, occurrences);

				if (_occurrencesByRoot.Count >= _occurrencesByRootCompactLimit)
				{
					_occurrencesByRoot.Compact(false);
					_occurrencesByRootCompactLimit = _occurrencesByRoot.Count * 2;
				}
			}

			occurrences.Add(occurrence);
		}

		#endregion  // OnOccurrenceCreated

		#region OnPollingTimerTick

		private void OnPollingTimerTick(object sender, EventArgs e)
		{
			if (this.IsPolling == false)
				this.PollForChanges();
		}

		#endregion  // OnPollingTimerTick

		#region OnUsersAdded

		private void OnUsersAdded(IEnumerable<ExchangeUser> addedUsers)
		{
			foreach (ExchangeUser user in addedUsers)
			{
				bool containsUser = false;
				for (int i = 0; i < _exchangeServices.Count; i++)
				{
					if (_exchangeServices[i].AssociatedUser == user)
					{
						containsUser = true;
						break;
					}
				}

				if (containsUser)
					continue;

				for (int i = 0; i < _pendingConnections.Count; i++)
				{
					if (_pendingConnections[i].AssociatedUser == user)
					{
						containsUser = true;
						break;
					}
				}

				if (containsUser)
					continue;

				this.InitializeUser(user);
			}
		}

		#endregion // OnUsersAdded

		#region OnUsersCollectionChanged

		private static void OnUsersCollectionChanged(ExchangeScheduleDataConnector owner, object item, string property, object extraInfo)
		{
			if (item is IEnumerable<ExchangeUser>)
			{
				NotifyCollectionChangedEventArgs eventArgs = extraInfo as NotifyCollectionChangedEventArgs;

				if (eventArgs != null)
					owner.OnUsersCollectionChanged(item, eventArgs);
			}
			else if (item is ExchangeUser)
			{
				ExchangeUser user = (ExchangeUser)item;

				if (owner._exchangeServices == null)
					return;

				NetworkCredential credentials = user.CreateNetworkCredentials();
				for (int i = 0; i < owner._exchangeServices.Count; i++)
				{
					ExchangeService service = owner._exchangeServices[i];

					if (service.AssociatedUser != user)
						continue;

					service.ServiceBinding.Credentials = credentials;
				}

				for (int i = 0; i < owner._pendingConnections.Count; i++)
				{
					ExchangeService service = owner._pendingConnections[i];

					if (service.AssociatedUser != user)
						continue;

					service.ServiceBinding.Credentials = credentials;
				}
			}
		}

		private void OnUsersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (_exchangeServices == null)
			{
				if (_resourceListManager != null)
					_initializeConnection.StartAsyncOperation();

				return;
			}

			switch (e.Action)
			{
				case NotifyCollectionChangedAction.Add:
					this.OnUsersAdded(e.NewItems.OfType<ExchangeUser>());
					break;

				case NotifyCollectionChangedAction.Remove:
					this.OnUsersRemoved(e.OldItems.OfType<ExchangeUser>());
					break;

				case NotifyCollectionChangedAction.Replace:
					this.OnUsersRemoved(e.OldItems.OfType<ExchangeUser>());
					this.OnUsersAdded(e.NewItems.OfType<ExchangeUser>());
					break;

				case NotifyCollectionChangedAction.Reset:

					// MD 4/19/11 - TFS72590
					IEnumerable<ExchangeUser> users = this.Users;

					for (int i = _exchangeServices.Count - 1; i >= 0; i--)
					{
						ExchangeService service = _exchangeServices[i];

						if (service.AssociatedUser == null)
							continue;

						// If the user still exists in the collection, don't touch it's associated service.
						// MD 4/19/11 - TFS72590
						// The users collection could be null. 
						//if (this.Users.Contains(service.AssociatedUser))
						if (users != null && users.Contains(service.AssociatedUser))
							continue;

						this.RemoveExchangeService(service, i);
					}

					for (int i = 0; i < _pendingConnections.Count; i++)
					{
						ExchangeService service = _pendingConnections[i];

						if (service.AssociatedUser == null)
							continue;

						// If the user still exists in the collection, don't touch it's associated service.
						// MD 4/19/11 - TFS72590
						// The users collection could be null. 
						//if (this.Users.Contains(service.AssociatedUser))
						if (users != null && users.Contains(service.AssociatedUser))
							continue;

						_cancelledConnections.Add(service);
					}

					// MD 4/19/11 - TFS72590
					// The users collection could be null. If it is, don't do anything.
					//this.OnUsersAdded(this.Users);
					if (users != null)
						this.OnUsersAdded(users);

					break;
			}
		}

		#endregion // OnUsersCollectionChanged

		#region OnUsersRemoved

		private void OnUsersRemoved(IEnumerable<ExchangeUser> removedUsers)
		{
			foreach (ExchangeUser user in removedUsers)
				this.RemoveExchangeServiceAssociatedWithUser(user);
		}

		#endregion // OnUsersRemoved

		#region RemoveAllExchangeServices

		private void RemoveAllExchangeServices()
		{
			// Remove all currently connected services.
			for (int i = _exchangeServices.Count - 1; i >= 0; i--)
				this.RemoveExchangeService(_exchangeServices[i], i);

			// Cancel all pending service connections.
			_cancelledConnections.AddRange(_pendingConnections);
		}

		#endregion  // RemoveAllExchangeServices

		#region RemoveExchangeService

		private void RemoveExchangeService(ExchangeService service, int index)
		{
			_resources.Remove(service.Resource);

			service.UnsubscribeFromPullNotifications(this.DefaultErrorCallback);

			_exchangeServices.RemoveAt(index);

			// MD 4/11/11 - TFS72059
			// We should clean up a bunch of collections when a resource is no longer active.
			// Remove all activities that belong to the resource and clear all cached activities belonging 
			// to the resource so we never reuse them.
			ExchangeActivityQueryResult[] queryResults = _activityQueryResults.ToArray();
			for (int i = 0; i < queryResults.Length; i++)
			{
				ExchangeActivityQueryResult result = queryResults[i];

				if (result == null)
					continue;

				for (int j = result.ActivitiesInternal.Count - 1; j >= 0; j--)
				{
					ActivityBase activity = result.ActivitiesInternal[j];
					if (activity.OwningResourceId == service.Resource.Id)
						result.ActivitiesInternal.RemoveAt(j);
				}

				ActivityQuery query = result.Query;

				// If a query has no calendars, it wants all activities, so don't remove those.
				// MD 4/5/12 - TFS101338
				//if (query.Calendars.Count == 0)
				IList<ResourceCalendar> calendars = this.GetCalendars(query);
				if (calendars.Count == 0)
					continue;

				bool hasActiveCalendar = false;

				// MD 4/5/12 - TFS101338
				//for (int j = 0; j < query.Calendars.Count; j++)
				for (int j = 0; j < calendars.Count; j++)
				{
					// MD 4/5/12 - TFS101338
					//if (_resources.Contains(query.Calendars[j].OwningResource))
					if (_resources.Contains(calendars[j].OwningResource))
					{
						hasActiveCalendar = true;
						break;
					}
				}

				if (hasActiveCalendar == false)
					_activityQueryResults.Remove(result);
			}

			KeyValuePair<string, ActivityBase>[] cachedActivityPairs = _cachedActvities.ToArray();
			for (int i = 0; i < cachedActivityPairs.Length; i++)
			{
				KeyValuePair<string, ActivityBase> cachedActivityPair = cachedActivityPairs[i];
				ActivityBase activity = cachedActivityPair.Value;

				if (activity == null)
					continue;

				if (activity.OwningResourceId == service.Resource.Id)
					_cachedActvities.Remove(cachedActivityPair.Key);
			}

			KeyValuePair<ActivityBase, WeakList<ActivityBase>>[] occurrencesByRootPairs = _occurrencesByRoot.ToArray();
			for (int i = 0; i < occurrencesByRootPairs.Length; i++)
			{
				KeyValuePair<ActivityBase, WeakList<ActivityBase>> occurrencesByRootPair = occurrencesByRootPairs[i];
				ActivityBase activity = occurrencesByRootPair.Key;

				if (activity == null)
					continue;

				if (activity.OwningResourceId == service.Resource.Id)
					_occurrencesByRoot.Remove(activity);
			}

			_activityQueryResults.Compact();
			_cachedActvities.Compact(true);
			_occurrencesByRoot.Compact(true);
		}

		#endregion // RemoveExchangeService

		#region RemoveExchangeServiceAssociatedWithUser

		private void RemoveExchangeServiceAssociatedWithUser(ExchangeUser user)
		{
			for (int i = _exchangeServices.Count - 1; i >= 0; i--)
			{
				ExchangeService service = _exchangeServices[i];

				if (service.AssociatedUser == user)
					this.RemoveExchangeService(service, i);
			}

			for (int i = _pendingConnections.Count - 1; i >= 0; i--)
			{
				ExchangeService service = _pendingConnections[i];

				if (service.AssociatedUser == user)
					_cancelledConnections.Add(service);
			}
		}

		#endregion // RemoveExchangeServiceAssociatedWithUser

		#region ShouldQueryOriginalDescriptionFormat

		private static bool ShouldQueryOriginalDescriptionFormat(DescriptionFormat format)
		{
			return format == DescriptionFormat.Default || format == DescriptionFormat.Unknown;
		}

		#endregion  // ShouldQueryOriginalDescriptionFormat

		#region SynchronizePropertyToRootHelper

		private void SynchronizePropertyToRootHelper(ActivityBase root, ActivityBase occurrence, int prop)
		{
			if (occurrence.IsVariance)
			{
				object rootValue = root.GetValueHelper<object>(prop);
				if (!occurrence.IsVariantPropertyFlagSet(prop, true))
					occurrence.SetValueHelper(prop, rootValue);
			}
			else
			{
				occurrence.PropsInfo.OnPropertyValueChanged(occurrence, prop, null);
			}
		}

		#endregion  // SynchronizePropertyToRootHelper

		#region UpdatePollingInterval

		private void UpdatePollingInterval(TimeSpan newPollingInterval)
		{
			_pollingTimer.Interval = this.PollingInterval;

			if (newPollingInterval == TimeSpan.Zero)
				_pollingTimer.Stop();
			else
				_pollingTimer.Start();
		}

		#endregion  // UpdatePollingInterval 

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region Public Properties

		#region IsPolling

		private readonly static DependencyPropertyKey IsPollingPropertyKey =  DependencyPropertyUtilities.RegisterReadOnly(
			"IsPolling",
			typeof(bool),
			typeof(ExchangeScheduleDataConnector),
			false, 
			null);

		/// <summary>
		/// Identifies the <see cref="IsPolling"/> dependency property
		/// </summary>
		public static DependencyProperty IsPollingProperty = ExchangeScheduleDataConnector.IsPollingPropertyKey.DependencyProperty;

		/// <summary>
		/// Gets the value indicating whether the connector is currently polling for changes on the Exchange server.
		/// </summary>
		/// <seealso cref="IsPollingProperty"/>
		/// <seealso cref="PollingInterval"/>
		/// <seealso cref="PollForChanges"/>
		public bool IsPolling
		{
			get { return (bool)this.GetValue(IsPollingProperty); }
			internal set { this.SetValue(IsPollingPropertyKey, value); }
		}

		#endregion // IsPolling

		#region PollingInterval

		/// <summary>
		/// Identifies the <see cref="PollingInterval"/> dependency property
		/// </summary>
		public readonly static DependencyProperty PollingIntervalProperty = DependencyProperty.Register(
			"PollingInterval",
			typeof(TimeSpan),
			typeof(ExchangeScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(new TimeSpan(0, 0, 30), OnPollingIntervalChanged));

		private static void OnPollingIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ExchangeScheduleDataConnector connector = (ExchangeScheduleDataConnector)d;
			connector.UpdatePollingInterval((TimeSpan)e.NewValue);
		}

		/// <summary>
		/// Returns or sets the interval at which the client will poll the server for changes.
		/// The default is 30 seconds.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// To disable polling, set the interval to TimeSpan.Zero.
		/// </para>
		/// </remarks>
		/// <seealso cref="PollingIntervalProperty"/>
		/// <seealso cref="PollForChanges"/>
		/// <seealso cref="IsPolling"/>
		public TimeSpan PollingInterval
		{
			get { return (TimeSpan)this.GetValue(ExchangeScheduleDataConnector.PollingIntervalProperty); }
			set { this.SetValue(ExchangeScheduleDataConnector.PollingIntervalProperty, value); }
		}

		#endregion  // PollingInterval

		#region ServerConnectionSettings

		/// <summary>
		/// Identifies the <see cref="ServerConnectionSettings"/> dependency property
		/// </summary>
		public readonly static DependencyProperty ServerConnectionSettingsProperty = DependencyProperty.Register(
			"ServerConnectionSettings",
			typeof(ExchangeServerConnectionSettings),
			typeof(ExchangeScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnServerConnectionSettingsChanged));

		private static void OnServerConnectionSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ExchangeScheduleDataConnector connector = (ExchangeScheduleDataConnector)d;

			ExchangeServerConnectionSettings oldSettings = (ExchangeServerConnectionSettings)e.OldValue;
			if (oldSettings != null)
				oldSettings.RemoveOwner(connector);

			ExchangeServerConnectionSettings newSettings = (ExchangeServerConnectionSettings)e.NewValue;
			connector._cachedServerConnectionSettings = newSettings;

			if (newSettings != null)
			{
				newSettings.AddOwner(connector);
				connector.OnServerConnectionSettingsChanged();
			}
			else if (connector._exchangeServices != null)
			{
				connector.RemoveAllExchangeServices();
			}
		}

		/// <summary>
		/// Gets or sets the settings used to connect with the Microsoft Exchange Server.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// The value is set after the first connection to the server has been made.
		/// </exception>
		/// <seealso cref="ServerConnectionSettingsProperty"/>
		public ExchangeServerConnectionSettings ServerConnectionSettings
		{
			get { return _cachedServerConnectionSettings; }
			set { this.SetValue(ServerConnectionSettingsProperty, value); }
		}

		#endregion // ServerConnectionSettings


		#region UseDefaultCredentials

		/// <summary>
		/// Identifies the <see cref="UseDefaultCredentials"/> dependency property
		/// </summary>
		public readonly static DependencyProperty UseDefaultCredentialsProperty = DependencyProperty.Register(
			"UseDefaultCredentials",
			typeof(bool),
			typeof(ExchangeScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(true, OnUseDefaultCredentialsChanged));

		private static void OnUseDefaultCredentialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ExchangeScheduleDataConnector connector = (ExchangeScheduleDataConnector)d;

			if (connector._exchangeServices == null)
				return;

			bool oldValue = (bool)e.OldValue;
			bool newValue = (bool)e.NewValue;

			if (oldValue == newValue)
				return;

			if (newValue)
			{
				connector.InitializeUser(null);
			}
			else
			{
				for (int i = connector._exchangeServices.Count - 1; i >= 0; i--)
				{
					ExchangeService service = connector._exchangeServices[i];

					if (service.AssociatedUser == null)
					{
						connector.RemoveExchangeService(service, i);
						break;
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the value indicating whether one of the users should be the user currently logged on to the system.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// UseDefaultCredentials can be used in conjunction with the <see cref="Users"/> collection. If it is, the logged in user will
		/// just be added as another user when connecting to the server.
		/// </p>
		/// <p class="body">
		/// When UseDefaultCredentials is True, the Resource created to represent the user will have a Id of "{domain}\{userName}"
		/// in lowercase.
		/// </p>
		/// </remarks>
		/// <seealso cref="UseDefaultCredentialsProperty"/>
		/// <seealso cref="Users"/>
		public bool UseDefaultCredentials
		{
			get { return (bool)this.GetValue(UseDefaultCredentialsProperty); }
			set { this.SetValue(UseDefaultCredentialsProperty, value); }
		}

		#endregion // UseDefaultCredentials 


		#region Users

		/// <summary>
		/// Identifies the <see cref="Users"/> dependency property
		/// </summary>
		public readonly static DependencyProperty UsersProperty = DependencyProperty.Register(
			"Users",
			typeof(IEnumerable<ExchangeUser>),
			typeof(ExchangeScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnUsersChanged));

		private static void OnUsersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ExchangeScheduleDataConnector connector = (ExchangeScheduleDataConnector)d;

			IEnumerable<ExchangeUser> newCollection = e.NewValue as IEnumerable<ExchangeUser>;

			if (newCollection == null)
				newCollection = new ExchangeUser[0];

			if (connector._users == null)
			{
				connector._users = new ItemNotificationCollection<ExchangeUser>(newCollection);

				ScheduleUtilities.AddListener(
					connector._users,
					new PropertyChangeListener<ExchangeScheduleDataConnector>(connector, OnUsersCollectionChanged, false),
					false);

				// MD 2/24/11 - TFS67033
				// Let the connector know that the collection has changed.
				connector._users.RaiseCollectionReset();
			}
			else
			{
				connector._users.SetSourceCollection(newCollection);
			}
		}

		/// <summary>
		/// Gets or sets the collection of users whose calendars should be retrieved from the server.
		/// </summary>
		/// <p class="body">
		/// Each user will have a Resource instance to represent it with an Id of "{Domain}\{UserName}" from the ExchangeUser.
		/// </p>
		/// <seealso cref="UsersProperty"/>
		public IEnumerable<ExchangeUser> Users
		{
			get { return (IEnumerable<ExchangeUser>)this.GetValue(UsersProperty); }
			set { this.SetValue(UsersProperty, value); }
		}

		#endregion // Users

		#region UseServerWorkingHours

		/// <summary>
		/// Identifies the <see cref="UseServerWorkingHours"/> dependency property
		/// </summary>
		public readonly static DependencyProperty UseServerWorkingHoursProperty = DependencyProperty.Register(
			"UseServerWorkingHours",
			typeof(bool),
			typeof(ExchangeScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(true));

		/// <summary>
		/// Gets or sets the value indicating whether the custom working hours should be downloaded from the server for each user.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this value is True, the working hours on the XamScheduleDataManager.Settings property are ignored.
		/// </p>
		/// </remarks>
		/// <seealso cref="UseServerWorkingHoursProperty"/>
		public bool UseServerWorkingHours
		{
			get { return (bool)this.GetValue(UseServerWorkingHoursProperty); }
			set { this.SetValue(UseServerWorkingHoursProperty, value); }
		}

		#endregion // UseDefaultCredentials

		#endregion // Public Properties

		#region Internal Properties

		// MD 4/27/11 - TFS72779
		#region AutoDetectedServerVersion

		internal ExchangeVersion? AutoDetectedServerVersion
		{
			get { return _autoDetectedServerVersion; }
		}

		#endregion  // AutoDetectedServerVersion

		#region CachedActvities

		internal WeakDictionary<string, ActivityBase> CachedActvities
		{
			get { return _cachedActvities; }
		} 

		#endregion  // CachedActvities

		#region CancelledConnections

		internal List<ExchangeService> CancelledConnections
		{
			get { return _cancelledConnections; }
		}

		#endregion  // CancelledConnections

		#region DefaultErrorCallback

		internal ErrorCallback DefaultErrorCallback
		{
			get { return _defaultErrorCallback; }
		}

		#endregion  // DefaultErrorCallback

		#region ExchangeServices

		internal List<ExchangeService> ExchangeServices
		{
			get { return _exchangeServices; }
		}

		#endregion  // ExchangeServices

		#region OccurrencesByRoot

		internal WeakDictionary<ActivityBase, WeakList<ActivityBase>> OccurrencesByRoot
		{
			get { return _occurrencesByRoot; }
		} 

		#endregion  // OccurrencesByRoot

		#region PendingConnections

		internal List<ExchangeService> PendingConnections
		{
			get { return _pendingConnections; }
		} 

		#endregion  // PendingConnections

		#region ReminderManager

		private ExchangeActivityReminderManager ReminderManager
		{
			get
			{
				if (null == _reminderManager)
					_reminderManager = new ExchangeActivityReminderManager(this);

				return _reminderManager;
			}
		}

		#endregion // ReminderManager

		#region RequestedServerVersionResolved
		
		internal ExchangeVersion RequestedServerVersionResolved
		{
		    get
		    {
		        ExchangeServerConnectionSettings setting = this.ServerConnectionSettings;
		
		        if (setting == null)
		            return ExchangeVersion.Exchange2007_SP1;
		
		        //return setting.RequestedServerVersion;
				ExchangeVersion version = setting.RequestedServerVersion;

				if (version == ExchangeVersion.AutoDetect)
				{
					if (_autoDetectedServerVersion.HasValue)
						return _autoDetectedServerVersion.Value;

					return ExchangeVersion.Exchange2007_SP1;
				}

				return version;
		    }
		}
		
		#endregion //RequestedServerVersionResolved

		#region ResourcesInternal

		internal ObservableCollection<Resource> ResourcesInternal
		{
			get
			{
				if (_resources == null)
				{
					_resources = new ObservableCollection<Resource>();
					_resourceListManager.List = _resources;
				}

				return _resources;
			}
		} 

		#endregion  // ResourcesInternal

		#endregion // Internal Properties 

		#endregion // Properties		
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