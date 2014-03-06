using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Windows.Threading;
using System.ServiceModel;


using Infragistics.Controls.Schedules.WcfListConnectorServiceWpf;
using ActivityOperationWcf = Infragistics.Controls.Schedules.WcfListConnectorServiceWpf.ActivityOperation;






namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Used for providing schedule data from a server over WCF services.
	/// </summary>
	/// <seealso cref="XamScheduleDataManager.DataConnector"/>
	public class WcfListScheduleDataConnector : ScheduleDataConnectorBase
	{
		#region Constants

		private const int PingInterval = 10000; 

		#endregion  // Constants

		#region Member Variables

		// MD 2/9/11 - TFS65718
		private ListManagerVersionInfo _activityCategoryListVersionInfo;

		private ListManagerVersionInfo _appointmentListVersionInfo;
		private EndpointAddress _cachedRemoteAddress;
		private object _cachedSecurityToken;



		private ListManagerVersionInfo _journalListVersionInfo;
		private ObjectSerializer _linqStatementSerializer;
		private ListScheduleDataConnectorForWcf _listConnector;
		private DispatcherTimer _pingTimer;
		private DispatcherTimer _pollingTimer;
		private ListManagerVersionInfo _recurringAppointmentListVersionInfo;
		private ListManagerVersionInfo _resourceCalendarListVersionInfo;
		private ListManagerVersionInfo _resourceListVersionInfo;
		private ListManagerVersionInfo _taskListVersionInfo;
		private bool _serverCanBeReached;
		private WcfListConnectorServiceClient _wcfServiceClient;
		private RemoteProperties _usedRemoteProperties;
		private DeferredOperation _verifyServiceClientDeferredOperation;

		#endregion  // Member Variables

		#region Constructor

		/// <summary>
		/// Creates a new <see cref="WcfListScheduleDataConnector"/> instance.
		/// </summary>
		public WcfListScheduleDataConnector()
		{
			// Create a serializer to use when serializing linq queries
			_linqStatementSerializer = new ObjectSerializer(new AttributeValueParser());
			LinqQueryManager.RegisterSerializerInfos(_linqStatementSerializer);

			// Create the underlying list connector and hook up perform query overrides so we can intervene 
			// when the queries are made and send them to the server instead of letting the list connector 
			// go to the lists.
			_listConnector = new ListScheduleDataConnectorForWcf(this);
			ScheduleUtilities.AddListener(_listConnector, this._listener, false);

			// MD 2/9/11 - TFS65718
			_listConnector.ActivityCategoryPropertyMappings = new ActivityCategoryPropertyMappingCollection();

			_listConnector.AppointmentPropertyMappings = new AppointmentPropertyMappingCollection();
			_listConnector.JournalPropertyMappings = new JournalPropertyMappingCollection();
			_listConnector.RecurringAppointmentPropertyMappings = new AppointmentPropertyMappingCollection();
			_listConnector.ResourceCalendarPropertyMappings = new ResourceCalendarPropertyMappingCollection();
			_listConnector.ResourcePropertyMappings = new ResourcePropertyMappingCollection();
			_listConnector.TaskPropertyMappings = new TaskPropertyMappingCollection();
			_listConnector._appointmentListManager._performQueryOverride =
				new Func<object, LinqQueryManager.ILinqStatement, Action<object, IEnumerable, DataErrorInfo>, bool>(this.PerformQueryOverrideAppointments);
			_listConnector._journalListManager._performQueryOverride =
				new Func<object, LinqQueryManager.ILinqStatement, Action<object, IEnumerable, DataErrorInfo>, bool>(this.PerformQueryOverrideJournals);
			_listConnector._recurringAppointmentListManager._performQueryOverride =
				new Func<object, LinqQueryManager.ILinqStatement, Action<object, IEnumerable, DataErrorInfo>, bool>(this.PerformQueryOverrideRecurringAppointments);
			_listConnector._taskListManager._performQueryOverride =
				new Func<object, LinqQueryManager.ILinqStatement, Action<object, IEnumerable, DataErrorInfo>, bool>(this.PerformQueryOverrideTasks);

			// Create a deferred operation to invoke when anything server connection information is dirtied.
			_verifyServiceClientDeferredOperation = new DeferredOperation(this.VerifyServiceClient);





		}

		#endregion  // Constructor

		#region Interfaces

		

		#endregion // Interfaces

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
		protected internal override bool AreActivitiesSupported(ActivityType activityType)
		{
			return _listConnector.AreActivitiesSupported(activityType);
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
			if (this.Connected == false)
			{
				errorInfo = DataErrorInfo.CreateDiagnostic(this, ScheduleUtilities.GetString("LE_RemoteServiceCantBeReached"));
				return false;
			}

			return _listConnector.BeginEdit(activity, out errorInfo);
		}

		#endregion // BeginEdit

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
		protected internal override CancelOperationResult CancelPendingOperation(OperationResult operation)
		{
			return _listConnector.CancelPendingOperation(operation);
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
		/// doesn't get commited to the data source until <see cref="EndEdit"/> method is called. Also if you wish to
		/// not commit the created activity then it is necessary to call <see cref="ScheduleDataConnectorBase.CancelEdit(ActivityBase, out DataErrorInfo)"/> 
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
		/// <seealso cref="ScheduleDataConnectorBase.CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="EndEdit"/>
		/// <seealso cref="Remove"/>
		protected internal override ActivityBase CreateNew(ActivityType activityType, out Infragistics.DataErrorInfo errorInfo)
		{
			return _listConnector.CreateNew(activityType, out errorInfo);
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
		/// <seealso cref="ScheduleDataConnectorBase.CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="Remove"/>
		protected internal override ActivityOperationResult EndEdit(ActivityBase activity, bool force)
		{
			return _listConnector.EndEdit(activity, force);
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
			return _listConnector.GetActivities(query);
		}

		#endregion  // GetActivities

		// MD 2/9/11 - TFS65718
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
		/// <seealso cref="ResolveActivityCategories"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected override IEnumerable<ActivityCategory> GetSupportedActivityCategories(ActivityBase activity)
		{
			return _listConnector.GetSupportedActivityCategories(activity);
		}

		#endregion // GetSupportedActivityCategories

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
		/// This method is used by the data manager to determine if the specified feature is supprted for activities of
		/// the specified type that belong the specified calendar. If the feature is not supported, relevant user interface 
		/// will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityOperationSupported(ActivityType, ActivityOperation, ResourceCalendar)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		protected internal override bool IsActivityFeatureSupported(ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar)
		{
			return _listConnector.IsActivityFeatureSupported(activityType, activityFeature, calendar);
		}

		#endregion // IsActivityFeatureSupported

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
		/// This method is used by the data manager to determine if the specified operation is supprted for the activities of the specified type
		/// for the specified resource calendar. If the operation is not supported, relevant user interface will be disabled or hidden in 
		/// the schedule controls.
		/// </para>
		/// <para class="body">
		/// When the context of an activity object for which the operation is being performed available, the overload
		/// <see cref="ScheduleDataConnectorBase.IsActivityOperationSupported(ActivityBase, ActivityOperation)"/> that takes in the activity is used.
		/// </para>
		/// </remarks>
		/// <seealso cref="ScheduleDataConnectorBase.IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		protected internal override bool IsActivityOperationSupported(ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar)
		{
			return this.IsActivityOperationSupportedHelper(null, activityType, activityOperation, calendar);
		}

		/// <summary>
		/// Indicates whether the specified activity operation for the specified activity type is supported.
		/// </summary>
		/// <param name="activity">Activity instance on which the operation is going to be performed.</param>
		/// <param name="activityOperation">Activity operation.</param>
		/// <returns>True if the operation is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified operation is supprted for the specified activity.
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
			return this.IsActivityOperationSupportedHelper(activity, activity.ActivityType, activityOperation, activity.OwningCalendar);
		}

		#endregion  // IsActivityOperationSupported

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged(object sender, string propName, object extraInfo)
		{
			base.OnSubObjectPropertyChanged(sender, propName, extraInfo);

			if ( sender == this )
			{
				switch ( propName )
				{
					case "Error":
					case "ClearBlockingError":
					case "TimeZoneInfoProviderResolved":
						break;

					case "RecurrenceCalculatorFactory":
						_listConnector.RecurrenceCalculatorFactory = this.RecurrenceCalculatorFactory;
						break;

					case "TimeZoneInfoProvider":
						_listConnector.TimeZoneInfoProvider = this.TimeZoneInfoProvider;
						break;

					default:
						Debug.Assert(false, "One of the properties is not synced with the underlying list connector: " + propName);
						break;
				}
			}
			else if (sender == _listConnector)
			{
				switch ( propName )
				{
					case "Error":
					case "ClearBlockingError":
						this.NotifyListeners(this, propName, extraInfo);
						break;
				}
			}
		} 

		#endregion  // OnSubObjectPropertyChanged

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
		/// are available via the ActivityOperationResult's <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>
		/// method. The caller, which may be a schedule control, will indicate via the UI that the operation
		/// is pending and when the results are initialized (via <see cref="ItemOperationResult&lt;T&gt;.InitializeResult"/>), 
		/// it will show the user with appropriate status of the operation.
		/// </para>
		/// </remarks>
		/// <seealso cref="CreateNew"/>
		/// <seealso cref="EndEdit"/>
		/// <seealso cref="ScheduleDataConnectorBase.CancelEdit(ActivityBase, out DataErrorInfo)"/>
		protected internal override ActivityOperationResult Remove(ActivityBase activity)
		{
			// If we are not connected to a WCF service, return a diagnostic error.
			if (this.Connected == false)
			{
				return new ActivityOperationResult(
					activity,
					ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NotConnectedToService", this.PublicDisplayName),
					true);
			}


			// Let the underlying list connector remove the activity first.
			ActivityOperationResult listConnectorResult = _listConnector.Remove(activity);

			// If there was a problem removing the activity from the underlying connector, 
			// don't even try to do the removal on the server. Just return the result.
			if (listConnectorResult.Error != null || listConnectorResult.IsCanceled)
				return listConnectorResult;

			// Otherwise, perform the activity operation on the server and wait until the call returns 
			// asychronously to complete the result.
			PerformActivityOperationContext context = new PerformActivityOperationContext();
			context.SecurityToken = this.SecurityTokenResolved;
			context.Activity = (ActivityBaseSerializableItem)activity.DataItem;
			context.Operation = ActivityOperationWcf.Remove;
			context.ClientVersionInfo = this.GetAssociatedListVersionInfo(activity);

			ActivityOperationResult result = new ActivityOperationResult(activity);

			this.WCFServiceClient.PerformActivityOperationAsync(context, result);

			return result;
		}

		#endregion  // Remove

		// MD 2/9/11 - TFS65718
		#region ResolveActivityCategories

		/// <summary>
		/// Gets the activity categories for the specified activity. Default implementation returns <i>null</i>.
		/// </summary>
		/// <param name="activity">Activity for which to get the list of activity categories.</param>
		/// <returns>IEnumerable that can optionally implement INotifyCollectionChanged to notify
		/// of changes to the list.</returns>
		/// <seealso cref="GetSupportedActivityCategories"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		protected internal override IEnumerable<ActivityCategory> ResolveActivityCategories(ActivityBase activity)
		{
			return _listConnector.ResolveActivityCategories(activity);
		} 

		#endregion  // ResolveActivityCategories

		#region ResourceItems

		/// <summary>
		/// Gets the resources collection.
		/// </summary>
		public override ResourceCollection ResourceItems
		{
			get { return _listConnector.ResourceItems; }
		}

		#endregion  // ResourceItems

		#region SubscribeToReminders

		/// <summary>
		/// Subscribes to reminders for the activities of the specified calendar. Note that more than one 
		/// subscriber can be subscribed to a single calendar as well as multiple calendars can be 
		/// subscribed to.
		/// </summary>
		/// <param name="calendar">This calendars activity reminders will be deilvered to the specified subscribed.</param>
		/// <param name="subscriber">When a reminder is due for an activity, the subscriber's <see cref="ReminderSubscriber.DeliverReminder"/> method will be invoked.</param>
		/// <param name="error">If there's an error, this will be set to error information.</param>
		protected internal override void SubscribeToReminders(ResourceCalendar calendar, ReminderSubscriber subscriber, out Infragistics.DataErrorInfo error)
		{
			_listConnector.SubscribeToReminders(calendar, subscriber, out error);
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
			_listConnector.UnsubscribeFromReminders(calendar, subscriber);
		}

		#endregion  // UnsubscribeFromReminders

		#region ValidateSettings

		internal override void ValidateSettings(List<DataErrorInfo> errorList)
		{
			if (_listConnector != null)
				_listConnector.ValidateSettings(errorList);
		} 

		#endregion  // ValidateSettings

		#region VerifyInitialState

		/// <summary>
		/// Called to verify that the data connector has sufficient state to operate.
		/// </summary>
		/// <param name="errorList">A list to receive the errors</param>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this method gets called once by the <see cref="XamScheduleDataManager"/> when it is verifying its inital state.
		/// </para>
		/// </remarks>
		internal protected override void VerifyInitialState(List<DataErrorInfo> errorList)
		{
			base.VerifyInitialState(errorList);

			// If there is no endpoint configuration, the developer should see an error, because without it, 
			// this connector is useless.
			if (this.EndpointConfigurationName == null &&
				this.RemoteBinding == null)
			{
				errorList.Add(ScheduleUtilities.CreateDiagnosticFromId(
					this,
					"LE_NoEndPointConfig",
					this.PublicDisplayName,
					"EndpointConfigurationName",
					"RemoteBinding",
					"RemoteAddress"));
			}
		}

		#endregion  // VerifyInitialState

		#endregion // Base Class Overrides

		#region Properties

		#region Public Properties

		#region EndpointConfigurationName

		/// <summary>
		/// Identifies the <see cref="EndpointConfigurationName"/> dependency property
		/// </summary>
		public readonly static DependencyProperty EndpointConfigurationNameProperty = DependencyProperty.Register(
			"EndpointConfigurationName",
			typeof(string),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnEndpointConfigurationNameChanged));

		private static void OnEndpointConfigurationNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;
			connector.DirtyServiceClient(RemoteProperties.EndpointConfigurationName);
		}

		/// <summary>
		/// Returns or sets the name of the endpoint defined in the client configuration file which is used to communicate 
		/// with the WCF service.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If this is not set, both the <see cref="RemoteBinding"/> and <see cref="RemoteAddress"/> must be set for the connector
		/// to be able to connect to the WCF service.
		/// </p>
		/// </remarks>
		/// <seealso cref="EndpointConfigurationNameProperty"/>
		/// <seealso cref="RemoteBinding"/>
		/// <seealso cref="RemoteAddress"/>
		public string EndpointConfigurationName
		{
			get { return (string)this.GetValue(WcfListScheduleDataConnector.EndpointConfigurationNameProperty); }
			set { this.SetValue(WcfListScheduleDataConnector.EndpointConfigurationNameProperty, value); }
		}

		#endregion  // EndpointConfigurationName

		#region PollingInterval

		/// <summary>
		/// Identifies the <see cref="PollingInterval"/> dependency property
		/// </summary>
		public readonly static DependencyProperty PollingIntervalProperty = DependencyProperty.Register(
			"PollingInterval",
			typeof(TimeSpan),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(new TimeSpan(0, 0, 30), OnPollingIntervalChanged));

		private static void OnPollingIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;

			if (connector._pollingTimer != null)
				connector._pollingTimer.Interval = (TimeSpan)e.NewValue;
		}

		/// <summary>
		/// Returns or sets the interval at which the client will poll the server for changes to the data sources.
		/// The default is 30 seconds.
		/// </summary>
		/// <seealso cref="PollingMode"/>
		public TimeSpan PollingInterval
		{
			get { return (TimeSpan)this.GetValue(WcfListScheduleDataConnector.PollingIntervalProperty); }
			set { this.SetValue(WcfListScheduleDataConnector.PollingIntervalProperty, value); }
		}

		#endregion  // PollingInterval

		#region PollingMode

		/// <summary>
		/// Identifies the <see cref="PollingMode"/> dependency property.
		/// </summary>
		public readonly static DependencyProperty PollingModeProperty = DependencyProperty.Register(
			"PollingMode",
			typeof(WcfSchedulePollingMode),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(WcfSchedulePollingMode.Detailed, OnPollingModeChanged));

		private static void OnPollingModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;

			connector.VerifyPollingTimerEnabledState();
		}

		/// <summary>
		/// Returns or sets the mode of polling to use when polling the server for changes.
		/// </summary>
		/// <remarks>
		/// <p class="note">
		/// <b>Note:</b> Polling only works when the item sources on the service send out list change notifications.
		/// </p>
		/// </remarks>
		/// <seealso cref="PollingInterval"/>
		public WcfSchedulePollingMode PollingMode
		{
			get { return (WcfSchedulePollingMode)this.GetValue(WcfListScheduleDataConnector.PollingModeProperty); }
			set { this.SetValue(WcfListScheduleDataConnector.PollingModeProperty, value); }
		}

		#endregion  // PollingMode

		#region RemoteAddress

		/// <summary>
		/// Identifies the <see cref="RemoteAddress"/> dependency property
		/// </summary>
		public readonly static DependencyProperty RemoteAddressProperty = DependencyProperty.Register(
			"RemoteAddress",
			typeof(object),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnRemoteAddressChanged));

		private static void OnRemoteAddressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;
			connector._cachedRemoteAddress = null;
			connector.DirtyServiceClient(RemoteProperties.RemoteAddress);

			if (e.NewValue != null)
			{
				bool isTypeValid = false;

				if (e.NewValue is string ||
					e.NewValue is Uri ||
					e.NewValue is EndpointAddress)
				{
					isTypeValid = true;
				}

				if (isTypeValid == false)
					throw new InvalidOperationException(ScheduleUtilities.GetString("LE_InvalidRemoteAddressType", connector.PublicDisplayName, "RemoteAddress"));
			}
		}

		/// <summary>
		/// Returns or sets the remote address of the WCF service associated with this connector.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This can be set to a null or an instance of one of these types: string, System.Uri, or System.ServiceModel.EndpointAddress.
		/// </p>
		/// <p class="body">
		/// If the <see cref="EndpointConfigurationName"/> is specified, this property is not required, but will override the 
		/// service address defined in the endpoint when it is set. If the EndpointConfigurationName is not set, then it is required
		/// that this property and <see cref="RemoteBinding"/> are set for the connector to be able to connect to the WCF service.
		/// </p>
		/// </remarks>
		/// <seealso cref="RemoteAddressProperty"/>
		public object RemoteAddress
		{
			get { return this.GetValue(WcfListScheduleDataConnector.RemoteAddressProperty); }
			set { this.SetValue(WcfListScheduleDataConnector.RemoteAddressProperty, value); }
		}

		#endregion  // RemoteAddress

		#region RemoteBinding

		/// <summary>
		/// Identifies the <see cref="RemoteBinding"/> dependency property
		/// </summary>
		public readonly static DependencyProperty RemoteBindingProperty = DependencyProperty.Register(
			"RemoteBinding",
			typeof(System.ServiceModel.Channels.Binding),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnRemoteBindingChanged));

		private static void OnRemoteBindingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;
			connector.DirtyServiceClient(RemoteProperties.RemoteBinding);
		}

		/// <summary>
		/// Returns or sets the binding to use when connecting to the WCF service.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// If the RemoteBinding and the <see cref="RemoteAddress"/> are both set, the <see cref="EndpointConfigurationName"/>
		/// will not be used even if it is set.
		/// </p>
		/// </remarks>
		/// <seealso cref="RemoteBindingProperty"/>
		public System.ServiceModel.Channels.Binding RemoteBinding
		{
			get { return (System.ServiceModel.Channels.Binding)this.GetValue(WcfListScheduleDataConnector.RemoteBindingProperty); }
			set { this.SetValue(WcfListScheduleDataConnector.RemoteBindingProperty, value); }
		}

		#endregion  // RemoteBinding

		#region SecurityToken

		/// <summary>
		/// Identifies the <see cref="SecurityToken"/> dependency property
		/// </summary>
		public readonly static DependencyProperty SecurityTokenProperty = DependencyProperty.Register(
			"SecurityToken",
			typeof(object),
			typeof(WcfListScheduleDataConnector),
			DependencyPropertyUtilities.CreateMetadata(null, OnSecurityTokenChanged));

		private static void OnSecurityTokenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			WcfListScheduleDataConnector connector = (WcfListScheduleDataConnector)d;

			connector._cachedSecurityToken = e.NewValue;

			// Get all initial info from the service since the security token has changed.
			connector.GetInitialInfoIfNecessary(true);
		}

		/// <summary>
		/// Gets or sets security information which will be authenticated by the service on all remote calls.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This property can be set to a string instance that contained security information or an instance of an object
		/// whose ToString method returns the security information.
		/// </para>
		/// </remarks>
		/// <seealso cref="SecurityTokenProperty"/>
		public object SecurityToken
		{
			get { return _cachedSecurityToken; }
			set { this.SetValue(WcfListScheduleDataConnector.SecurityTokenProperty, value); }
		}

		#endregion  // SecurityToken

		#endregion  // Public Properties

		#region Private Properties

		#region Connected

		private bool Connected
		{
			get 
			{
				WcfListConnectorServiceClient client = this.WCFServiceClient;
				if (client == null)
					return false;






				if (_serverCanBeReached == false)
				{
					client.PingAsync();
					return false;
				}

				return true;
			}
		} 

		#endregion  // Connected

		#region RemoteAddressResolved

		private EndpointAddress RemoteAddressResolved
		{
			get
			{
				if (_cachedRemoteAddress == null)
					_cachedRemoteAddress = this.GetResolvedRemoteAddress();

				return _cachedRemoteAddress;
			}
		}

		private EndpointAddress GetResolvedRemoteAddress()
		{
			object remoteAddress = this.RemoteAddress;

			if (remoteAddress == null)
				return null;

			EndpointAddress remoteAddressObject = remoteAddress as EndpointAddress;
			if (remoteAddressObject != null)
				return remoteAddressObject;

			string remoteAddressString = remoteAddress as string;
			if (remoteAddressString != null)
				return new EndpointAddress(remoteAddressString);

			Uri remoteAddressUri = remoteAddress as Uri;
			if (remoteAddressUri != null)
				return new EndpointAddress(remoteAddressUri);

			Debug.Assert(false, "The RemoteAddress was not of the correct type.");
			return null;
		} 

		#endregion  // RemoteAddressResolved

		#region SecurityTokenResolved






		private string SecurityTokenResolved
		{
			get
			{
				if (this.SecurityToken == null)
					return null;

				return this.SecurityToken.ToString();
			}
		}

		#endregion  // SecurityTokenResolved

		#region WCFServiceClient






		private WcfListConnectorServiceClient WCFServiceClient
		{
			get
			{
				this.VerifyServiceClient();
				return _wcfServiceClient;
			}
		}

		#endregion  // WCFServiceClient

		#endregion  // Private Properties

		#endregion  // Properties

		#region Methods

		#region Private Methods

		#region CopyPublicPropertyValues



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private static void CopyPublicPropertyValues<T>(T source, T destination)
		{
			Type type = source.GetType();
			foreach (PropertyInfo property in type.GetProperties())
				property.SetValue(destination, property.GetValue(source, null), null);
		}

		#endregion  // CopyPublicPropertyValues

		#region DirtyServiceClient






		private void DirtyServiceClient(RemoteProperties dirtiedProperty)
		{
			// When we are using the RemoteBinding property, we ignore the EndpointConfigurationName, 
			// so we don't care when it changes.
			if ((_usedRemoteProperties & RemoteProperties.RemoteBinding) != 0 &&
				dirtiedProperty == RemoteProperties.EndpointConfigurationName)
			{
				return;
			}

			_usedRemoteProperties = RemoteProperties.None;

			// Start the async operation to reverify the service client.
			_verifyServiceClientDeferredOperation.StartAsyncOperation();

			if (_wcfServiceClient == null)
				return;

			// Unhook the service client and release it.
			_wcfServiceClient.GetInitialInfoCompleted -= new EventHandler<GetInitialInfoCompletedEventArgs>(this.OnGetInitialInfoCompleted);
			_wcfServiceClient.PerformActivityOperationCompleted -= new EventHandler<PerformActivityOperationCompletedEventArgs>(this.OnPerformActivityOperationCompleted);
			_wcfServiceClient.PingCompleted -= new EventHandler<AsyncCompletedEventArgs>(this.OnPingCompleted);
			_wcfServiceClient.PollForItemSourceChangesCompleted -= new EventHandler<PollForItemSourceChangesCompletedEventArgs>(this.OnPollForItemSourceChangesCompleted);
			_wcfServiceClient.PollForItemSourceChangesDetailedCompleted -= new EventHandler<PollForItemSourceChangesDetailedCompletedEventArgs>(this.OnPollForItemSourceChangesDetailedCompleted);
			_wcfServiceClient.QueryActivitiesCompleted -= new EventHandler<QueryActivitiesCompletedEventArgs>(this.OnQueryActivitiesCompleted);
			_wcfServiceClient = null;

			// Stop the ping timer
			this.VerifyPingTimerEnabledState();

			// Clear all item sources and mappigs collections. When we reconnect, these will be reinitialized again.
			_listConnector.AppointmentItemsSource = null;
			_listConnector.AppointmentPropertyMappings.Clear();
			_listConnector.JournalItemsSource = null;
			_listConnector.JournalPropertyMappings.Clear();
			_listConnector.ResourceCalendarItemsSource = null;
			_listConnector.ResourceCalendarPropertyMappings.Clear();
			_listConnector.ResourceItemsSource = null;
			_listConnector.ResourcePropertyMappings.Clear();
			_listConnector.TaskItemsSource = null;
			_listConnector.TaskPropertyMappings.Clear();
		}

		#endregion  // DirtyServiceClient

		#region GetAssociatedListVersion






		private ListManagerVersionInfo GetAssociatedListVersionInfo(ActivityBase activity)
		{
			switch (activity.ActivityType)
			{
				case ActivityType.Appointment:
					return this.ShouldUseRecurringAppointmentList(activity)
						? _recurringAppointmentListVersionInfo
						: _appointmentListVersionInfo;

				case ActivityType.Journal:
					return _journalListVersionInfo;

				case ActivityType.Task:
					return _taskListVersionInfo;

				default:
					Debug.Assert(false, "Unknown ActivityType: " + activity.ActivityType);
					return null;
			}
		}

		#endregion // GetAssociatedListVersion

		#region GetError



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		// MD 9/30/10 - TFS49870
		// We can't get the service's error info to pass into this method if there is a remote call error because just asking 
		// for the result of a remote call throws an exception when there is a remote call error. So now we will only call this
		// when we already know there is no remote call error.
		//private static Exception GetError(WcfRemoteErrorInfo serviceErrorInfo, Exception callError)
		//{
		//    if (serviceErrorInfo != null)
		//        return new WcfServiceException(serviceErrorInfo.Message, serviceErrorInfo.ExceptionType, serviceErrorInfo.StackTrace);
		//
		//    return callError;
		//}
		private static Exception GetError(WcfRemoteErrorInfo serviceErrorInfo)
		{
			if (serviceErrorInfo != null)
				return new WcfServiceException(serviceErrorInfo.Message, serviceErrorInfo.ExceptionType, serviceErrorInfo.StackTrace);

			return null;
		}

		#endregion // GetError

		#region GetInitialInfoIfNecessary



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		// MD 2/9/11 - TFS65718
		// Added a parameter and made the last parameters all optional so the callers don't need to specify them if they are passing all False values.
		//private void GetInitialInfoIfNecessary(bool allInitialInfoNeeded, bool resourcesNeeded, bool resourceCalendarsNeeded)
		private void GetInitialInfoIfNecessary(bool allInitialInfoNeeded, bool resourcesNeeded = false, bool resourceCalendarsNeeded = false, bool activityCategoriesNeeded = false)
		{
			WcfListConnectorServiceClient client = this.WCFServiceClient;

			if (client == null)
				return;

			InitialInfoTypes neededInformation = (InitialInfoTypes)0;

			if (allInitialInfoNeeded)
			{
				neededInformation = InitialInfoTypes.All;
			}
			else
			{
				if (resourcesNeeded)
					neededInformation |= InitialInfoTypes.ResourceItemSource;

				if (resourceCalendarsNeeded)
					neededInformation |= InitialInfoTypes.ResourceCalendarItemSource;

				// MD 2/9/11 - TFS65718
				if (activityCategoriesNeeded)
					neededInformation |= InitialInfoTypes.ActivityCategoryItemSource;
			}

			if (neededInformation != 0)
			{
				GetInitialInfoContext context = new GetInitialInfoContext();
				context.SecurityToken = this.SecurityTokenResolved;
				context.RequestedInfo = neededInformation;
				client.GetInitialInfoAsync(context, neededInformation);
			}
		}

		#endregion  // GetInitialInfoIfNecessary

		#region IsActivityOperationSupportedHelper

		private bool IsActivityOperationSupportedHelper(ActivityBase activity, ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar)
		{
			// The version information will contain information about what the lists on the server are 
			// capable of, so determine which version info to use.
			ListManagerVersionInfo versionInfo;
			switch (activityType)
			{
				case ActivityType.Appointment:
					if (activity != null && this.ShouldUseRecurringAppointmentList(activity))
						versionInfo = _recurringAppointmentListVersionInfo;
					else
						versionInfo = _appointmentListVersionInfo;
					break;

				case ActivityType.Journal:
					versionInfo = _journalListVersionInfo;
					break;

				case ActivityType.Task:
					versionInfo = _appointmentListVersionInfo;
					break;

				default:
					Debug.Assert(false, "Unknown ActivityType: " + activityType);
					return false;
			}

			if (versionInfo == null)
				return false;

			// Query the capabilities stored on the version info.
			ListCapabilities capabilities = (ListCapabilities)versionInfo.ListCapabilities;

			bool isOperationAllowed;
			switch (activityOperation)
			{
				case ActivityOperation.Add:
					isOperationAllowed = (0 != (ListCapabilities.Add & capabilities));
					break;

				case ActivityOperation.Edit:
					isOperationAllowed = (0 != (ListCapabilities.Edit & capabilities));
					break;

				case ActivityOperation.Remove:
					isOperationAllowed = (0 != (ListCapabilities.Remove & capabilities));
					break;

				default:
					Debug.Assert(false, "Unknown ActivityOperation: " + activityOperation);
					return false;
			}

			if (isOperationAllowed == false)
				return false;

			if (activity == null)
				return _listConnector.IsActivityOperationSupported(activityType, activityOperation, calendar);
			else
				return _listConnector.IsActivityOperationSupported(activity, activityOperation);
		} 

		#endregion  // IsActivityOperationSupportedHelper

		#region OnGetInitialInfoCompleted






		private void OnGetInitialInfoCompleted(object sender, GetInitialInfoCompletedEventArgs e)
		{
			// MD 9/30/10 - TFS49870
			// If there is an Error on the event args or Cancelled is True, getting the Result property will throw 
			// an exception, so we need to check those first before getting the Result.
			//Exception error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo, e.Error);
			Exception error = e.Error;
			if (error == null)
			{
				// If the call was cancelled, don't do anything.
				if (e.Cancelled)
					return;

				error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo);
			}

			// If there was an error, let any listeners know about it.
			if (error != null)
			{
				DataErrorInfo dataErrorInfo = new DataErrorInfo(error);
				dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
				this.RaiseError(dataErrorInfo);
				return;
			}

			// MD 9/30/10 - TFS49870
			// We are now checking this at the top of the method, so we don't need to check it here.
			//// If the call was cancelled, don't do anything.
			//if (e.Cancelled)
			//    return;

			Dictionary<ItemSourceType, ItemSourceInfo> itemSourceInfos = e.Result.ItemSourceInfos;

			// Determine which information we queried for.
			InitialInfoTypes requestedInfo = (InitialInfoTypes)e.UserState;
			bool containsItemSourceInfos = (requestedInfo & InitialInfoTypes.ItemSourceInfos) != 0;
			bool containsResourceCalendarItemSource = (requestedInfo & InitialInfoTypes.ResourceCalendarItemSource) != 0;
			bool containsResourceItemSource = (requestedInfo & InitialInfoTypes.ResourceItemSource) != 0;

			// MD 2/9/11 - TFS65718
			bool containsActivityCategoryItemSource = (requestedInfo & InitialInfoTypes.ActivityCategoryItemSource) != 0;

			// If the item source information was requested, first initialize the resources and resource calendars property mappings.
			if (containsItemSourceInfos)
			{
				ItemSourceInfo itemSourceInfo;
				itemSourceInfos.TryGetValue(ItemSourceType.Resource, out itemSourceInfo);
				WcfListScheduleDataConnector.ParseItemSourceInfo<ResourceSerializableItem, ResourceProperty>(
					itemSourceInfo, _listConnector.ResourcePropertyMappings, false, ref _resourceListVersionInfo);

				itemSourceInfos.TryGetValue(ItemSourceType.ResourceCalendar, out itemSourceInfo);
				WcfListScheduleDataConnector.ParseItemSourceInfo<ResourceCalendarSerializableItem, ResourceCalendarProperty>(
					itemSourceInfo, _listConnector.ResourceCalendarPropertyMappings, false, ref _resourceCalendarListVersionInfo);

				// MD 2/9/11 - TFS65718
				itemSourceInfos.TryGetValue(ItemSourceType.ActivityCategory, out itemSourceInfo);
				WcfListScheduleDataConnector.ParseItemSourceInfo<ActivityCategorySerializableItem, ActivityCategoryProperty>(
					itemSourceInfo, _listConnector.ActivityCategoryPropertyMappings, false, ref _activityCategoryListVersionInfo);
			}

			// Set the resources collection if it was requested.
			if (containsResourceItemSource)
			{
				_listConnector.ResourceItemsSource = e.Result.ResourceItemsSource;
				_resourceListVersionInfo = e.Result.ResourceItemsVersionInfo;
			}

			// Set the resource calendars collection if it was requested.
			if (containsResourceCalendarItemSource)
			{
				_listConnector.ResourceCalendarItemsSource = e.Result.ResourceCalendarItemsSource;
				_resourceCalendarListVersionInfo = e.Result.ResourceCalendarItemsVersionInfo;
			}

			// MD 2/9/11 - TFS65718
			// Set the activity category collection if it was requested.
			if (containsActivityCategoryItemSource)
			{
				_listConnector.ActivityCategoryItemsSource = e.Result.ActivityCategoryItemsSource;
				_activityCategoryListVersionInfo = e.Result.ActivityCategoryItemsVersionInfo;
			}

			// If the item source information was requested, now initialize the remaining collections.
			if (containsItemSourceInfos)
			{
				ItemSourceInfo itemSourceInfo;

				itemSourceInfos.TryGetValue(ItemSourceType.Appointment, out itemSourceInfo);
				_listConnector.AppointmentItemsSource = WcfListScheduleDataConnector.ParseItemSourceInfo<AppointmentSerializableItem, AppointmentProperty>(
					itemSourceInfo, _listConnector.AppointmentPropertyMappings, true, ref _appointmentListVersionInfo);

				itemSourceInfos.TryGetValue(ItemSourceType.Journal, out itemSourceInfo);
				_listConnector.JournalItemsSource = WcfListScheduleDataConnector.ParseItemSourceInfo<JournalSerializableItem, JournalProperty>(
					itemSourceInfo, _listConnector.JournalPropertyMappings, true, ref _journalListVersionInfo);

				itemSourceInfos.TryGetValue(ItemSourceType.RecurringAppointment, out itemSourceInfo);
				_listConnector.RecurringAppointmentItemsSource = WcfListScheduleDataConnector.ParseItemSourceInfo<AppointmentSerializableItem, AppointmentProperty>(
					itemSourceInfo, _listConnector.RecurringAppointmentPropertyMappings, true, ref _recurringAppointmentListVersionInfo);

				itemSourceInfos.TryGetValue(ItemSourceType.Task, out itemSourceInfo);
				_listConnector.TaskItemsSource = WcfListScheduleDataConnector.ParseItemSourceInfo<TaskSerializableItem, TaskProperty>(
					itemSourceInfo, _listConnector.TaskPropertyMappings, true, ref _taskListVersionInfo);
			}

			// Make sure the polling timer is started if this is the first time we are getting the 
			// initial info results.
			this.VerifyPollingTimer();
		}

		#endregion  // OnGetInitialInfoCompleted



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


		#region OnPerformActivityOperationCompleted






		private void OnPerformActivityOperationCompleted(object sender, PerformActivityOperationCompletedEventArgs e)
		{
			// Get the ActivityOperationResult associated with this activity operation.
			ActivityOperationResult result;
			PerformActivityOperationEndEditCallState endEditCallState = e.UserState as PerformActivityOperationEndEditCallState;
			if (endEditCallState != null)
				result = endEditCallState.result;
			else
				result = (ActivityOperationResult)e.UserState;

			switch (result.Activity.ActivityType)
			{
				case ActivityType.Appointment:
					if (this.ShouldUseRecurringAppointmentList(result.Activity))
					{
						// MD 4/29/11 - TFS57206
						// OnPerformActivityOperationCompletedHelper now needs to take the list manager.
						//ProxyItemsSource<AppointmentSerializableItem> itemSource = (ProxyItemsSource<AppointmentSerializableItem>)_listConnector._recurringAppointmentListManager.List;
						//this.OnPerformActivityOperationCompletedHelper(e, itemSource, ref _recurringAppointmentListVersionInfo);
						this.OnPerformActivityOperationCompletedHelper<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							e, _listConnector._recurringAppointmentListManager, ref _recurringAppointmentListVersionInfo);
					}
					else
					{
						// MD 4/29/11 - TFS57206
						// OnPerformActivityOperationCompletedHelper now needs to take the list manager.
						//ProxyItemsSource<AppointmentSerializableItem> itemSource = (ProxyItemsSource<AppointmentSerializableItem>)_listConnector._appointmentListManager.List;
						//this.OnPerformActivityOperationCompletedHelper(e, itemSource, ref _appointmentListVersionInfo);
						this.OnPerformActivityOperationCompletedHelper<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							e, _listConnector._appointmentListManager, ref _appointmentListVersionInfo);
					}
					break;

				case ActivityType.Journal:
					{
						// MD 4/29/11 - TFS57206
						// OnPerformActivityOperationCompletedHelper now needs to take the list manager.
						//ProxyItemsSource<JournalSerializableItem> itemSource = (ProxyItemsSource<JournalSerializableItem>)_listConnector._journalListManager.List;
						//this.OnPerformActivityOperationCompletedHelper(e, itemSource, ref _journalListVersionInfo);
						this.OnPerformActivityOperationCompletedHelper<JournalSerializableItem, Journal, JournalProperty>(
							e, _listConnector._journalListManager, ref _journalListVersionInfo);
					}
					break;

				case ActivityType.Task:
					{
						// MD 4/29/11 - TFS57206
						// OnPerformActivityOperationCompletedHelper now needs to take the list manager.
						//ProxyItemsSource<TaskSerializableItem> itemSource = (ProxyItemsSource<TaskSerializableItem>)_listConnector._taskListManager.List;
						//this.OnPerformActivityOperationCompletedHelper(e, itemSource, ref _taskListVersionInfo);
						this.OnPerformActivityOperationCompletedHelper<TaskSerializableItem, Task, TaskProperty>(
							e, _listConnector._taskListManager, ref _taskListVersionInfo);
					}
					break;

				default:
					Debug.Assert(false, "Unknown ActivityType: " + result.Activity.ActivityType);
					break;
			}
		}

		#endregion // OnPerformActivityOperationCompleted

		#region OnPerformActivityOperationCompletedHelper

		// MD 4/29/11 - TFS57206
		// OnPerformActivityOperationCompletedHelper now needs to take the list manager.
		//private void OnPerformActivityOperationCompletedHelper<TSerializable>(
		//    PerformActivityOperationCompletedEventArgs e,
		//    ProxyItemsSource<TSerializable> itemSource,
		//    ref ListManagerVersionInfo listVersionInfoMember)
		//    where TSerializable : ActivityBaseSerializableItem
		private void OnPerformActivityOperationCompletedHelper<TSerializable, TViewItem, TMappingKey>(
			PerformActivityOperationCompletedEventArgs e,
			ActivityListManager<TViewItem, TMappingKey> listManager,
			ref ListManagerVersionInfo listVersionInfoMember)
			where TSerializable : ActivityBaseSerializableItem
			where TViewItem : ActivityBase, new()
		{
			// MD 4/29/11 - TFS57206
			// We don't need the item source passed in because we can get it from the list manager.
			ProxyItemsSource<TSerializable> itemSource = (ProxyItemsSource<TSerializable>)listManager.List;

			// MD 9/30/10 - TFS49870
			// If there is an Error on the event args or Cancelled is True, getting the Result property will throw 
			// an exception, so we need to check those first before getting the Result.
			Exception error = e.Error;
			if (error == null && e.Cancelled == false)
				error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo);

			DataErrorInfo dataErrorInfo = null;
			if (error != null)
			{
				dataErrorInfo = new DataErrorInfo(error);
				dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
			}

			// MD 9/30/10 - TFS49870
			// Wrapped this code in an if statement. If there was an Error of Cancelled was True, we can't get the 
			// Result property.
			if (dataErrorInfo == null && e.Cancelled == false)
			{
				ListManagerVersionInfo newVersionInfo = e.Result.NewVersionInfo;

				if (listVersionInfoMember != null && newVersionInfo != null &&
					listVersionInfoMember.PropertyMappingsVersion != newVersionInfo.PropertyMappingsVersion)
				{
					// Requery all initial info again because the mappings have changed on the service.
					this.GetInitialInfoIfNecessary(true);
				}

				// Cache the latest version information for the associated list manager from the WCF service.
				listVersionInfoMember = newVersionInfo;
			}

			// Get the ActivityOperationResult associated with this activity operation.
			ActivityOperationResult result;
			PerformActivityOperationEndEditCallState endEditCallState = e.UserState as PerformActivityOperationEndEditCallState;
			if (endEditCallState != null)
				result = endEditCallState.result;
			else
				result = (ActivityOperationResult)e.UserState;

			// MD 9/30/10 - TFS49870
			// This code has been moved to the top of the method.
			//Exception error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo, e.Error);
			//
			//// If an error was an error, create a DataErrorInfo instance for it.
			//DataErrorInfo dataErrorInfo = null;
			//if (error != null)
			//{
			//    dataErrorInfo = new DataErrorInfo(error);
			//    dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
			//}

			// MD 9/30/10 - TFS49870
			// If there was an Error of Cancelled was True, we can't get the Result property.
			//if (e.Result.UpdatedActivityIfChanged != null)
			if (dataErrorInfo == null && e.Cancelled == false && e.Result.UpdatedActivityIfChanged != null)
			{
				TSerializable dataItem = (TSerializable)result.Activity.DataItem;
				WcfListScheduleDataConnector.CopyPublicPropertyValues((TSerializable)e.Result.UpdatedActivityIfChanged, dataItem);
			}

			// If this activity operation was called from an edit edit call and there was an error or it was cancelled,
			// we need to cancel the edit operation so the activity's old values are restored.
			if (endEditCallState != null && (dataErrorInfo != null || e.Cancelled))
			{
				DataErrorInfo cancelEditError = null;
				this.CancelEdit(result.Activity, out cancelEditError);

				// If an error occurred, combine it with the remote call error if necessary.
				if (cancelEditError != null)
				{
					if (dataErrorInfo != null)
					{
						List<DataErrorInfo> dataErrorInfoList = new List<DataErrorInfo>();
						dataErrorInfoList.Add(dataErrorInfo);
						dataErrorInfoList.Add(cancelEditError);

						dataErrorInfo = new DataErrorInfo(dataErrorInfoList);
						dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
					}
					else
					{
						dataErrorInfo = cancelEditError;
					}
				}
			}

			// If one or more errors have occurred, initialize the ActivityOperationResult with it.
			if (dataErrorInfo != null)
			{
				result.InitializeResult(dataErrorInfo, true);
				return;
			}

			// if the call was cancelled, cancel the ActivityOperationResult.
			if (e.Cancelled)
			{
				result.OnCanceled();
				return;
			}

			// If the activity operation was done due ot an end edit call, we can now call the base 
			// EndEditOverride of the list connector.
			if (endEditCallState != null)
				_listConnector.EndEditOverrideInternal(endEditCallState.editList, result, endEditCallState.force);

			// Mark the ActivityOperationResult as complete now.
			result.InitializeResult(null, true);

			// If the version we had on the client did not match the WCF service's version of it's item source, send 
			// a reset notification on the client item source so we requery appointments.
			if (e.Result.VersionWasOutOfDate && itemSource != null)
			{
				// MD 4/29/11 - TFS57206
				// Resetting the item source will clear all activities until we get the re-query response from the server.
				// Instead, dirty all query results manually and pass in False for the clearDataList parameter so each query
				// result keeps its current list of activities until the re-query result comes back and we repopulate the result.
				//itemSource.Reset();
				listManager.DirtyAllQueryResults(false, true, false);
			}
		}

		#endregion // OnPerformActivityOperationCompletedHelper

		#region OnPingTimerTick

		private void OnPingTimerTick(object sender, EventArgs e)
		{
			WcfListConnectorServiceClient client = this.WCFServiceClient;

			if (client == null)
				return;

			client.PingAsync();
		}

		#endregion  // OnPingTimerTick

		#region OnPollForItemSourceChangesCompleted






		private void OnPollForItemSourceChangesCompleted(object sender, PollForItemSourceChangesCompletedEventArgs e)
		{
			// MD 9/30/10 - TFS49870
			// If there is an Error on the event args or Cancelled is True, getting the Result property will throw 
			// an exception, so we need to check those first before getting the Result.
			//Exception error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo, e.Error);
			Exception error = e.Error;
			if (error == null)
			{
				// If the call was cancelled, don't do anything.
				if (e.Cancelled)
					return;

				error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo);
			}

			// If there was an error, let any listeners know about it.
			if (error != null)
			{
				DataErrorInfo dataErrorInfo = new DataErrorInfo(error);
				dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
				this.RaiseError(dataErrorInfo);
				return;
			}

			Dictionary<ItemSourceType, ListManagerVersionInfo> serviceVersionInfos = e.Result.VersionInfos;

			bool resourcesNeeded = false;
			bool resourceCalendarsNeeded = false;

			// MD 2/9/11 - TFS65718
			bool activityCategoriesNeeded = false;

			bool allInitialInfoNeeded = false;

			// Loop over all version information returned from the WCF service and requery any information on 
			// collections which are out of date.
			foreach (KeyValuePair<ItemSourceType, ListManagerVersionInfo> versionInfo in serviceVersionInfos)
			{
				switch (versionInfo.Key)
				{
					case ItemSourceType.ResourceCalendar:
						if (_resourceCalendarListVersionInfo == null)
						{
							if (versionInfo.Value.HasList)
								allInitialInfoNeeded = true;
						}
						else
						{
							if (_resourceCalendarListVersionInfo.PropertyMappingsVersion != versionInfo.Value.PropertyMappingsVersion)
							{
								allInitialInfoNeeded = true;
							}
							else if (_resourceCalendarListVersionInfo.DataListVersion != versionInfo.Value.DataListVersion)
							{
								resourceCalendarsNeeded = true;
							}

							_resourceCalendarListVersionInfo = versionInfo.Value;
						}
						break;

					case ItemSourceType.Resource:
						if (_resourceListVersionInfo == null)
						{
							if (versionInfo.Value.HasList)
								allInitialInfoNeeded = true;
						}
						else
						{
							if (_resourceListVersionInfo.PropertyMappingsVersion != versionInfo.Value.PropertyMappingsVersion)
							{
								allInitialInfoNeeded = true;
							}
							else if (_resourceListVersionInfo.DataListVersion != versionInfo.Value.DataListVersion)
							{
								resourcesNeeded = true;
							}

							_resourceListVersionInfo = versionInfo.Value;
						}
						break;

					// MD 2/9/11 - TFS65718
					case ItemSourceType.ActivityCategory:
						if (_activityCategoryListVersionInfo == null)
						{
							if (versionInfo.Value.HasList)
								allInitialInfoNeeded = true;
						}
						else
						{
							if (_activityCategoryListVersionInfo.PropertyMappingsVersion != versionInfo.Value.PropertyMappingsVersion)
							{
								allInitialInfoNeeded = true;
							}
							else if (_activityCategoryListVersionInfo.DataListVersion != versionInfo.Value.DataListVersion)
							{
								resourcesNeeded = true;
							}

							_activityCategoryListVersionInfo = versionInfo.Value;
						}
						break;

					case ItemSourceType.Appointment:
						WcfListScheduleDataConnector.UpdateProxyItemsSource<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							_listConnector._appointmentListManager,
							versionInfo.Value,
							ref _appointmentListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.Journal:
						WcfListScheduleDataConnector.UpdateProxyItemsSource<JournalSerializableItem, Journal, JournalProperty>(
							_listConnector._journalListManager,
							versionInfo.Value,
							ref _journalListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.RecurringAppointment:
						WcfListScheduleDataConnector.UpdateProxyItemsSource<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							_listConnector._recurringAppointmentListManager,
							versionInfo.Value,
							ref _recurringAppointmentListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.Task:
						WcfListScheduleDataConnector.UpdateProxyItemsSource<TaskSerializableItem, Task, TaskProperty>(
							_listConnector._taskListManager,
							versionInfo.Value,
							ref _taskListVersionInfo,
							ref allInitialInfoNeeded);
						break;

					default:
						Debug.Assert(false, "Unknown ItemSourceType: " + versionInfo.Key);
						continue;
				}
			}

			// If we need to requery any initial information, do it now.
			this.GetInitialInfoIfNecessary(allInitialInfoNeeded, resourcesNeeded, resourceCalendarsNeeded, activityCategoriesNeeded);
		}

		#endregion // OnPollForItemSourceChangesCompleted

		#region OnPollForItemSourceChangesDetailedCompleted






		private void OnPollForItemSourceChangesDetailedCompleted(object sender, PollForItemSourceChangesDetailedCompletedEventArgs e)
		{
			// MD 9/30/10 - TFS49870
			// If there is an Error on the event args or Cancelled is True, getting the Result property will throw 
			// an exception, so we need to check those first before getting the Result.
			//Exception error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo, e.Error);
			Exception error = e.Error;
			if (error == null)
			{
				// If the call was cancelled, don't do anything.
				if (e.Cancelled)
					return;

				error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo);
			}

			// If there was an error, let any listeners know about it.
			if (error != null)
			{
				DataErrorInfo dataErrorInfo = new DataErrorInfo(error);
				dataErrorInfo.Severity = ErrorSeverity.Diagnostic;
				this.RaiseError(dataErrorInfo);
				return;
			}

			bool resourcesNeeded = false;
			bool resourceCalendarsNeeded = false;

			// MD 2/9/11 - TFS65718
			bool activityCategoriesNeeded = false;

			bool allInitialInfoNeeded = false;

			// Loop over all item source change information returned from the WCF service and perform any changes made 
			// on the WCF service's list managers to the list managers on the client.
			foreach (KeyValuePair<ItemSourceType, DetailedItemSourceChangeInfo> detailedChangeInfo in e.Result.DetailedChanges)
			{
				switch (detailedChangeInfo.Key)
				{
					case ItemSourceType.ResourceCalendar:
						WcfListScheduleDataConnector.UpdateObservableCollection<ResourceCalendarSerializableItem, ResourceCalendar, ResourceCalendarProperty>(
							_listConnector._resourceCalendarListManager,
							detailedChangeInfo.Value,
							ref _resourceCalendarListVersionInfo,
							ref allInitialInfoNeeded,
							out resourceCalendarsNeeded);
						break;

					case ItemSourceType.Resource:
						WcfListScheduleDataConnector.UpdateObservableCollection<ResourceSerializableItem, Resource, ResourceProperty>(
							_listConnector._resourceListManager,
							detailedChangeInfo.Value,
							ref _resourceListVersionInfo,
							ref allInitialInfoNeeded,
							out resourcesNeeded);
						break;
					// MD 2/9/11 - TFS65718
					case ItemSourceType.ActivityCategory:
						WcfListScheduleDataConnector.UpdateObservableCollection<ActivityCategorySerializableItem, ActivityCategory, ActivityCategoryProperty>(
							_listConnector._activityCategoryListManager,
							detailedChangeInfo.Value,
							ref _activityCategoryListVersionInfo,
							ref allInitialInfoNeeded,
							out activityCategoriesNeeded);
						break;

					case ItemSourceType.Appointment:
						this.UpdateProxyItemsSource<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							_listConnector._appointmentListManager,
							detailedChangeInfo.Value,
							ref _appointmentListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.Journal:
						this.UpdateProxyItemsSource<JournalSerializableItem, Journal, JournalProperty>(
							_listConnector._journalListManager,
							detailedChangeInfo.Value,
							ref _journalListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.RecurringAppointment:
						this.UpdateProxyItemsSource<AppointmentSerializableItem, Appointment, AppointmentProperty>(
							_listConnector._recurringAppointmentListManager,
							detailedChangeInfo.Value,
							ref _recurringAppointmentListVersionInfo,
							ref allInitialInfoNeeded);
						break;
					case ItemSourceType.Task:
						this.UpdateProxyItemsSource<TaskSerializableItem, Task, TaskProperty>(
							_listConnector._taskListManager,
							detailedChangeInfo.Value,
							ref _taskListVersionInfo,
							ref allInitialInfoNeeded);
						break;

					default:
						Debug.Assert(false, "Unknown ItemSourceType: " + detailedChangeInfo.Key);
						continue;
				}
			}

			// If we need to requery any initial information, do it now.
			this.GetInitialInfoIfNecessary(allInitialInfoNeeded, resourcesNeeded, resourceCalendarsNeeded, activityCategoriesNeeded);
		}

		#endregion  // OnPollForItemSourceChangesDetailedCompleted

		#region OnPollingTimerTick

		private void OnPollingTimerTick(object sender, EventArgs e)
		{
			if (this.Connected == false)
				return;

			WcfListConnectorServiceClient client = this.WCFServiceClient;

			switch (this.PollingMode)
			{
				case WcfSchedulePollingMode.RequeryOnAnyChange:
					{
						PollForItemSourceChangesContext context = new PollForItemSourceChangesContext();
						context.SecurityToken = this.SecurityTokenResolved;
						client.PollForItemSourceChangesAsync(context);
					}
					break;

				case WcfSchedulePollingMode.Detailed:
					{
						// If we are doing detailed level polling, we need to send over the current version information of item 
						// sources on this connector so the WCF service can determine what has changed since the client last received
						// information about the item sources.
						Dictionary<ItemSourceType, ListManagerVersionInfo> clientItemSourceVersions = new Dictionary<ItemSourceType, ListManagerVersionInfo>();

						clientItemSourceVersions[ItemSourceType.Appointment] = _appointmentListVersionInfo;
						clientItemSourceVersions[ItemSourceType.Journal] = _journalListVersionInfo;
						clientItemSourceVersions[ItemSourceType.RecurringAppointment] = _recurringAppointmentListVersionInfo;
						clientItemSourceVersions[ItemSourceType.Resource] = _resourceListVersionInfo;
						clientItemSourceVersions[ItemSourceType.ResourceCalendar] = _resourceCalendarListVersionInfo;
						clientItemSourceVersions[ItemSourceType.Task] = _taskListVersionInfo;

						PollForItemSourceChangesDetailedContext context = new PollForItemSourceChangesDetailedContext();
						context.SecurityToken = this.SecurityTokenResolved;
						context.ClientItemSourceVersions = clientItemSourceVersions;

						client.PollForItemSourceChangesDetailedAsync(context, clientItemSourceVersions);
					}
					break;

				case WcfSchedulePollingMode.None:
					break;

				default:
					Debug.Assert(false, "Unknown PollingMode: " + this.PollingMode);
					break;
			}
		}

		#endregion  // OnPollingTimerTick

		#region OnQueryActivitiesCompleted






		private void OnQueryActivitiesCompleted(object sender, QueryActivitiesCompletedEventArgs e)
		{
			QueryActivitiesCallState callState = (QueryActivitiesCallState)e.UserState;

			// MD 9/30/10 - TFS49870
			// If there is an Error on the event args or Cancelled is True, getting the Result property will throw 
			// an exception, so we need to check those first before getting the Result.
			//Exception error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo, e.Error);
			Exception error = e.Error;
			if (error == null)
			{
				// If the call was cancelled, don't do anything.
				if (e.Cancelled)
				{
					callState.provideQueryResultData(callState.context, null, null);
					return;
				}

				error = WcfListScheduleDataConnector.GetError(e.Result.ErrorInfo);
			}

			// Provice the query results to the underlying list connector.
			if (error == null)
			{
				callState.provideQueryResultData(callState.context, e.Result.Activities, null);
			}
			else
			{
				DataErrorInfo errorInfo = new DataErrorInfo(error);
				errorInfo.Severity = ErrorSeverity.Diagnostic;
				callState.provideQueryResultData(callState.context, null, errorInfo);
			}
		}

		#endregion // OnQueryActivitiesCompleted

		#region ParseItemSourceInfo






		private static IEnumerable ParseItemSourceInfo<TSerializableItem, TMappingKey>(
			ItemSourceInfo info, 
			IPropertyMappingCollection<TMappingKey> mappings, 
			bool listNeeded, 
			ref ListManagerVersionInfo listVersionInfoMember) where TMappingKey : struct
		{
			listVersionInfoMember = null;
			mappings.Clear();

			if (info == null)
				return null;

			listVersionInfoMember = info.VersionInfo;

			// If any properties are mapped on the WCF servie, we have to mapp the same properties on the underlying list 
			// connector. We don't care what the mapped names are on the service, because our backing items on the client
			// have properties with the same names as their associated properties on the view items. But we do need to map
			// the same properties mapped on the service so the linq queries are created correctly and the list connector
			// knows what the capabilities are of the backing data items.
			if (info.MappedProperties != null)
			{
				Type serializableType = typeof(TSerializableItem);

				foreach (string mappedProperty in info.MappedProperties)
				{
					PropertyInfo propertyInfo = serializableType.GetProperty(mappedProperty);
					if (propertyInfo == null)
						continue;

					TMappingKey scheduleProperty;
					if (Enum.TryParse<TMappingKey>(mappedProperty, out scheduleProperty) == false)
					{
						Debug.Assert(false, "Could not parse the property: " + mappedProperty);
						continue;
					}

					mappings[scheduleProperty] = mappedProperty;
				}
			}

			if (listNeeded)
				return new ProxyItemsSource<TSerializableItem>();

			return null;
		}

		#endregion  // ParseItemSourceInfo

		#region PerformQueryOverrideActivities



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private bool PerformQueryOverrideActivities(
			ActivityListType type, 
			object context, 
			LinqQueryManager.ILinqStatement linqStatement, 
			Action<object, IEnumerable, DataErrorInfo> provideQueryResultData)
		{
			try
			{
				DataErrorInfo error = null;

				// If we are not connected to the WCF service, there is an error.
				if (this.Connected == false)
					error = ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NotConnectedToService", this.PublicDisplayName); 

				string linqStatementEncoded = null;
				
				// If we are connected, try to serialize the linq statement and its descendants into an XML string.
				if ( error == null )
					linqStatementEncoded = this.SerializeLinqStatement(linqStatement, out error);

				// If there was an error connecting or serializing the linq statement, provide the error results and return.
				if (error != null)
				{
					provideQueryResultData(context, null, error); 
					return true;
				}

				// Construct and user state object for the remote call.
				QueryActivitiesCallState callState = new QueryActivitiesCallState();
				callState.context = context;
				callState.provideQueryResultData = provideQueryResultData;

				QueryActivitiesContext callContext = new QueryActivitiesContext();
				callContext.SecurityToken = this.SecurityTokenResolved;
				callContext.ListType = type;
				callContext.LinqStatementEncoded = linqStatementEncoded;

				// Otherwise, send the query to the service. We will provide the query results when it returns.
				this.WCFServiceClient.QueryActivitiesAsync(callContext, callState);
			}
			catch (Exception exc)
			{
				// If any exceptions occur, provide them to the query results.
				DataErrorInfo error = new DataErrorInfo(exc);
				error.Severity = ErrorSeverity.Diagnostic;
				provideQueryResultData(context, null, error);
			}

			// Always return true because we are always processing the query.
			return true;
		}

		#endregion // PerformQueryOverrideActivities

		#region PerformQueryOverrideAppointments

		private bool PerformQueryOverrideAppointments(object context, LinqQueryManager.ILinqStatement linqStatement, Action<object, IEnumerable, DataErrorInfo> provideQueryResultData)
		{
			return this.PerformQueryOverrideActivities(ActivityListType.Appointment, context, linqStatement, provideQueryResultData);
		}

		#endregion  // PerformQueryOverrideAppointments

		#region PerformQueryOverrideJournals

		private bool PerformQueryOverrideJournals(object context, LinqQueryManager.ILinqStatement linqStatement, Action<object, IEnumerable, DataErrorInfo> provideQueryResultData)
		{
			return this.PerformQueryOverrideActivities(ActivityListType.Journal, context, linqStatement, provideQueryResultData);
		}

		#endregion  // PerformQueryOverrideJournals

		#region PerformQueryOverrideAppointments

		private bool PerformQueryOverrideRecurringAppointments(object context, LinqQueryManager.ILinqStatement linqStatement, Action<object, IEnumerable, DataErrorInfo> provideQueryResultData)
		{
			return this.PerformQueryOverrideActivities(ActivityListType.RecurringAppointment, context, linqStatement, provideQueryResultData);
		}

		#endregion  // PerformQueryOverrideAppointments

		#region PerformQueryOverrideTasks

		private bool PerformQueryOverrideTasks(object context, LinqQueryManager.ILinqStatement linqStatement, Action<object, IEnumerable, DataErrorInfo> provideQueryResultData)
		{
			return this.PerformQueryOverrideActivities(ActivityListType.Task, context, linqStatement, provideQueryResultData);
		}

		#endregion  // PerformQueryOverrideTasks

		#region OnPingCompleted

		private void OnPingCompleted(object sender, AsyncCompletedEventArgs e)
		{
			if (e.Error == null && e.Cancelled == false)
			{
				if (_serverCanBeReached == false)
				{
					_serverCanBeReached = true;
					this.VerifyPollingTimerEnabledState();
					this.GetInitialInfoIfNecessary(true);
				}
			}
			else
			{
				if (_serverCanBeReached)
				{
					_serverCanBeReached = false;
					this.VerifyPollingTimerEnabledState();
				}
			}
		} 

		#endregion  // OnPingCompleted

		#region SerializeLinqStatement



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private string SerializeLinqStatement(LinqQueryManager.ILinqStatement linqStatement, out DataErrorInfo error)
		{
			error = null;

			try
			{
				using (Stream stream = new MemoryStream())
				{
					using (XmlDictionaryWriter xmlWriter = XmlDictionaryWriter.CreateTextWriter(stream, Encoding.UTF8, false))
					{
						if (_linqStatementSerializer.Save(xmlWriter, linqStatement) == false)
						{
							error = ScheduleUtilities.CreateDiagnosticFromId(linqStatement, "LE_LinQSerializeFailure"); 
							return null;
						}
					}

					stream.Position = 0;
					using (StreamReader stringReader = new StreamReader(stream))
					{
						return stringReader.ReadToEnd();
					}
				}
			}
			catch (Exception e)
			{
				error = new DataErrorInfo(e);
				error.Severity = ErrorSeverity.Diagnostic;
				return null;
			}

		}

		#endregion  // SerializeLinqStatement

		#region ShouldUseRecurringAppointmentList






		private bool ShouldUseRecurringAppointmentList(ActivityBase activity)
		{
			if (_listConnector._recurringAppointmentListManager.HasRecurringActivities == false)
				return false;

			return activity.IsOccurrence || activity.IsRecurrenceRoot;
		}

		#endregion // ShouldUseRecurringAppointmentList

		#region UpdateObservableCollection



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

		private static void UpdateObservableCollection<TSerializable, TViewItem, TMappingKey>(
			ScheduleItemCollectionListManager<TViewItem, TMappingKey> listManager,
			DetailedItemSourceChangeInfo detailedChangeInfo,
			ref ListManagerVersionInfo listVersionInfoMember,
			ref bool allInitialInfoNeeded,
			out bool reset)
			where TSerializable : SerializableItemBase
			where TViewItem : class
		{
			reset = false;

			if (listVersionInfoMember == null)
			{
				if (detailedChangeInfo.VersionInfo.HasList)
					allInitialInfoNeeded = true;

				return;
			}

			// If the history ID is no longer the same on the server or the the property mappings version have changed on the server, 
			// we need to requery all information about this collection and it's mappings again.
			if (listVersionInfoMember.ChangeHistoryId != detailedChangeInfo.VersionInfo.ChangeHistoryId ||
				listVersionInfoMember.PropertyMappingsVersion != detailedChangeInfo.VersionInfo.PropertyMappingsVersion)
			{
				allInitialInfoNeeded = true;
				return;
			}

			// Cache the latest version information.
			listVersionInfoMember = detailedChangeInfo.VersionInfo;

			// If the list manager on the server has no list, clear the list on the client's list manager.
			if (listVersionInfoMember.HasList == false)
			{
				listManager.List = null;
				return;
			}

			// If there are changes to the list, perform each change the observable collection.
			if (detailedChangeInfo.Changes != null)
			{
				ICollection<TSerializable> itemSource = (ICollection<TSerializable>)listManager.List;

				// If we don't have an item source yet, create and set it now.
				if (itemSource == null)
				{
					itemSource = new ObservableCollection<TSerializable>();
					listManager.List = itemSource;
				}

				Type viewItemType = typeof(TViewItem);
				PropertyInfo idProperty = viewItemType.GetProperty("Id");

				foreach (ItemSourceChange change in detailedChangeInfo.Changes)
				{
					TSerializable changedItem = (TSerializable)change.ChangedItem;

					switch (change.ChangeType)
					{
						case ItemSourceChangeType.AddItem:
							itemSource.Add(changedItem);
							break;

						case ItemSourceChangeType.ChangeItem:
							bool updatedItem = false;

							// If an item has changes, loop over all view items and update the backing data item of the matching view item.
							foreach (TViewItem viewItem in listManager.Items)
							{
								if ((string)idProperty.GetValue(viewItem, null) != changedItem.Id)
									continue;

								TSerializable dataItem = (TSerializable)listManager.ViewItemFactory.GetDataItem(viewItem);
								WcfListScheduleDataConnector.CopyPublicPropertyValues(changedItem, dataItem);
								updatedItem = true;
								break;
							}

							// If we could not find the correct item to update, just rese the collection.
							if (updatedItem == false)
								goto case ItemSourceChangeType.Reset;

							break;

						case ItemSourceChangeType.RemoveItem:
							itemSource.Remove(changedItem);
							break;

						case ItemSourceChangeType.Reset:
							reset = true;
							break;
					}
				}
			}
		}

		#endregion  // UpdateObservableCollection

		#region UpdateProxyItemsSource



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private static void UpdateProxyItemsSource<TSerializable, TViewItem, TMappingKey>(
			ActivityListManager<TViewItem, TMappingKey> listManager,
			ListManagerVersionInfo serverListVersionInfo,
			ref ListManagerVersionInfo listVersionInfoMember,
			ref bool allInitialInfoNeeded) where TViewItem : ActivityBase, new()
		{
			if (listVersionInfoMember == null)
			{
				if (serverListVersionInfo.HasList)
					allInitialInfoNeeded = true;

				return;
			}

			// If the history ID is no longer the same on the server or the the property mappings version have changed on the server, 
			// we need to requery all information about this collection and it's mappings again.
			if (listVersionInfoMember.ChangeHistoryId != serverListVersionInfo.ChangeHistoryId ||
				listVersionInfoMember.PropertyMappingsVersion != serverListVersionInfo.PropertyMappingsVersion)
			{
				allInitialInfoNeeded = true;
				return;
			}

			// If all version numbers are the same, we don't have to do anything.
			if (listVersionInfoMember.DataListVersion == serverListVersionInfo.DataListVersion)
				return;

			listVersionInfoMember = serverListVersionInfo;

			// If the list manager on the server no longer has a list, clear the list on this list manager.
			if (listVersionInfoMember.HasList == false)
			{
				listManager.List = null;
				return;
			}

			ProxyItemsSource<TSerializable> itemSource = (ProxyItemsSource<TSerializable>)listManager.List;

			// If there is no list currently, set it to a new instance. Otherwise, send out a reset notification so 
			// activities will be required.
			if (itemSource == null)
			{
				listManager.List = new ProxyItemsSource<TSerializable>();
			}
			else
			{
				// MD 4/29/11 - TFS57206
				// Resetting the item source will clear all activities until we get the re-query response from the server.
				// Instead, dirty all query results manually and pass in False for the clearDataList parameter so each query
				// result keeps its current list of activities until the re-query result comes back and we repopulate the result.
				//itemSource.Reset();
				listManager.DirtyAllQueryResults(false, true, false);
			}
		}



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		private void UpdateProxyItemsSource<TSerializable, TViewItem, TMappingKey>(
			ActivityListManager<TViewItem, TMappingKey> listManager,
			DetailedItemSourceChangeInfo detailedChangeInfo,
			ref ListManagerVersionInfo listVersionInfoMember,
			ref bool allInitialInfoNeeded)
			where TSerializable : ActivityBaseSerializableItem
			where TViewItem : ActivityBase, new()
		{
			if (listVersionInfoMember == null)
			{
				if (detailedChangeInfo.VersionInfo.HasList)
					allInitialInfoNeeded = true;

				return;
			}

			// If the history ID is no longer the same on the server or the the property mappings version have changed on the server, 
			// we need to requery all information about this collection and it's mappings again.
			if (listVersionInfoMember.ChangeHistoryId != detailedChangeInfo.VersionInfo.ChangeHistoryId ||
				listVersionInfoMember.PropertyMappingsVersion != detailedChangeInfo.VersionInfo.PropertyMappingsVersion)
			{
				allInitialInfoNeeded = true;
				return;
			}

			// Cache the latest version information.
			listVersionInfoMember = detailedChangeInfo.VersionInfo;

			// If the list manager on the server has no list, clear the list on the client's list manager.
			if (listVersionInfoMember.HasList == false)
			{
				listManager.List = null;
				return;
			}

			// If there are changes to the list, perform each change the observable collection.
			if (detailedChangeInfo.Changes != null)
			{
				ProxyItemsSource<TSerializable> itemSource = (ProxyItemsSource<TSerializable>)listManager.List;

				// If we don't have an item source yet, create and set it now.
				if (itemSource == null)
				{
					itemSource = new ProxyItemsSource<TSerializable>();
					listManager.List = itemSource;
				}

				foreach (ItemSourceChange change in detailedChangeInfo.Changes)
				{
					TSerializable changedItem = (TSerializable)change.ChangedItem;

					switch (change.ChangeType)
					{
						case ItemSourceChangeType.AddItem:
							((ICollection<TSerializable>)itemSource).Add(changedItem);
							break;

						case ItemSourceChangeType.ChangeItem:
							ActivityBase appointment = listManager.ViewItemManager.GetViewItemWithId(changedItem.Id);

							// If we could not find the correct item to update, just rese the collection.
							if (appointment == null)
								goto case ItemSourceChangeType.Reset;

							TSerializable dataItem = (TSerializable)appointment.DataItem;
							WcfListScheduleDataConnector.CopyPublicPropertyValues(changedItem, dataItem);
							break;

						case ItemSourceChangeType.RemoveItem:
							((ICollection<TSerializable>)itemSource).Remove(changedItem);
							break;

						case ItemSourceChangeType.Reset:
							// MD 4/29/11 - TFS57206
							// Resetting the item source will clear all activities until we get the re-query response from the server.
							// Instead, dirty all query results manually and pass in False for the clearDataList parameter so each query
							// result keeps its current list of activities until the re-query result comes back and we repopulate the result.
							//itemSource.Reset();
							listManager.DirtyAllQueryResults(false, true, false);
							break;
					}
				}
			}
		}

		#endregion // UpdateProxyItemsSource

		#region VerifyPingTimer






		private void VerifyPingTimer()
		{
			if (_pingTimer != null)
				return;

			_pingTimer = new DispatcherTimer();
			_pingTimer.Tick += new EventHandler(this.OnPingTimerTick);
			_pingTimer.Interval = TimeSpan.FromMilliseconds(WcfListScheduleDataConnector.PingInterval);

			this.VerifyPingTimerEnabledState();
		}

		#endregion // VerifyPingTimer

		#region VerifyPingTimerEnabledState






		private void VerifyPingTimerEnabledState()
		{
			if (_pingTimer == null)
				return;

			bool shouldBeEnabled = (this.WCFServiceClient != null);

			if (_pingTimer.IsEnabled == shouldBeEnabled)
				return;

			if (shouldBeEnabled)
				_pingTimer.Start();
			else
				_pingTimer.Stop();
		}

		#endregion  // VerifyPingTimerEnabledState

		#region VerifyPollingTimer






		private void VerifyPollingTimer()
		{
			if (_pollingTimer != null)
				return;

			_pollingTimer = new DispatcherTimer();
			_pollingTimer.Tick += new EventHandler(this.OnPollingTimerTick);
			_pollingTimer.Interval = this.PollingInterval;

			this.VerifyPollingTimerEnabledState();
		}

		#endregion // VerifyPollingTimer

		#region VerifyPollingTimerEnabledState






		private void VerifyPollingTimerEnabledState()
		{
			if (_pollingTimer == null)
				return;

			bool shouldBeEnabled = (this.PollingMode != WcfSchedulePollingMode.None && _serverCanBeReached);

			if (_pollingTimer.IsEnabled == shouldBeEnabled)
				return;

			if (shouldBeEnabled)
				_pollingTimer.Start();
			else
				_pollingTimer.Stop();
		}

		#endregion  // VerifyPollingTimerEnabledState

		#region VerifyServiceClient






		private void VerifyServiceClient()
		{
			if (_wcfServiceClient != null)
				return;

			try
			{
				string endpointConfigurationName = this.EndpointConfigurationName;
				EndpointAddress remoteAddress = this.RemoteAddressResolved;
				System.ServiceModel.Channels.Binding remoteBinding = this.RemoteBinding;

				if (remoteBinding != null && remoteAddress != null)
				{
					_wcfServiceClient = new WcfListConnectorServiceClient(remoteBinding, remoteAddress);
					_usedRemoteProperties = RemoteProperties.RemoteAddress | RemoteProperties.RemoteBinding;
				}
				else if (endpointConfigurationName != null)
				{
					if (remoteAddress != null)
					{
						_wcfServiceClient = new WcfListConnectorServiceClient(endpointConfigurationName, remoteAddress);
						_usedRemoteProperties = RemoteProperties.RemoteAddress | RemoteProperties.EndpointConfigurationName;
					}
					else
					{
						_wcfServiceClient = new WcfListConnectorServiceClient(endpointConfigurationName);
						_usedRemoteProperties = RemoteProperties.EndpointConfigurationName;
					}
				}

				if (_wcfServiceClient == null)
					return;
			}
			catch (Exception exc)
			{
				DataErrorInfo errorInfo = new DataErrorInfo(exc);
				errorInfo.Severity = ErrorSeverity.Diagnostic;
				this.RaiseError(errorInfo);
				return;
			}

			_serverCanBeReached = true;
			this.VerifyPingTimer();

			// Hook into all events that return results from async calls to the WCF service.
			_wcfServiceClient.GetInitialInfoCompleted += new EventHandler<GetInitialInfoCompletedEventArgs>(this.OnGetInitialInfoCompleted);
			_wcfServiceClient.PerformActivityOperationCompleted += new EventHandler<PerformActivityOperationCompletedEventArgs>(this.OnPerformActivityOperationCompleted);
			_wcfServiceClient.PingCompleted += new EventHandler<AsyncCompletedEventArgs>(this.OnPingCompleted);
			_wcfServiceClient.PollForItemSourceChangesCompleted += new EventHandler<PollForItemSourceChangesCompletedEventArgs>(this.OnPollForItemSourceChangesCompleted);
			_wcfServiceClient.PollForItemSourceChangesDetailedCompleted += new EventHandler<PollForItemSourceChangesDetailedCompletedEventArgs>(this.OnPollForItemSourceChangesDetailedCompleted);
			_wcfServiceClient.QueryActivitiesCompleted += new EventHandler<QueryActivitiesCompletedEventArgs>(this.OnQueryActivitiesCompleted);

			// Get all initial info from the service client.
			this.GetInitialInfoIfNecessary(true);
		}

		#endregion  // VerifyServiceClient

		#endregion  // Private Methods

		#endregion  // Methods


		#region ListScheduleDataConnectorForWcf class

		private class ListScheduleDataConnectorForWcf : ListScheduleDataConnector
		{
			private WcfListScheduleDataConnector _owner;

			public ListScheduleDataConnectorForWcf(WcfListScheduleDataConnector owner)
			{
				_owner = owner;
			}

			internal override void EndEditOverride(IEditList<ActivityBase> editList, ActivityOperationResult result, bool force)
			{
				ActivityBase activity = result.Activity;

				DataErrorInfo error = null;

				// If we are not connected, there is an error here.
				if (_owner.Connected == false)
					error = ScheduleUtilities.CreateDiagnosticFromId(this, "LE_NotConnectedToService", this.PublicDisplayName);

				bool isAddNew = editList.IsAddNew(activity)
					// Also consider an activity add-new if it's an occurrence. Note that we only commit variances
					// which we check for in EndEditHelper method.
					// 
					|| activity.IsOccurrence && activity.DataItem is OccurrenceId;

				// We need to cache this value, because if it is true, it will change to false when we call AssociateDataItemToAddNewViewItem.
				bool activityIsAddNew = activity.IsAddNew;

				// If we are connected, try to associate the activity with a data item
				if (error == null && isAddNew)
					editList.AssociateDataItemToAddNewViewItem(activity, out error);

				// If an error occured connecting or associated the data item to the activity, intialize the result with the error.
				if (error != null)
				{
					result.InitializeResult(error, true);
					return;
				}

				// Create a user state object to pass ot the async call.
				PerformActivityOperationEndEditCallState callState = new PerformActivityOperationEndEditCallState();
				callState.editList = editList;
				callState.force = force;
				callState.result = result;

				PerformActivityOperationContext context = new PerformActivityOperationContext();
				context.SecurityToken = _owner.SecurityTokenResolved;
				context.Activity = (ActivityBaseSerializableItem)activity.DataItem;
				context.Operation = activityIsAddNew ? ActivityOperationWcf.Add : ActivityOperationWcf.Edit;
				context.ClientVersionInfo = _owner.GetAssociatedListVersionInfo(activity);

				_owner.WCFServiceClient.PerformActivityOperationAsync(context, callState);
			}






			internal void EndEditOverrideInternal(IEditList<ActivityBase> editList, ActivityOperationResult result, bool force)
			{
				base.EndEditOverride(editList, result, force);
			}

			internal override string PublicDisplayName
			{
				get { return _owner.GetType().Name; }
			}
		}

		#endregion // ListScheduleDataConnectorForWcf class

		#region ProxyItemsSource class

		private class ProxyItemsSource<T> :
			IList<T>,
			INotifyCollectionChanged
		{
			#region Constructor

			public ProxyItemsSource() { }

			#endregion  // Constructor

			#region Interfaces

			#region ICollection<T> Members

			void ICollection<T>.Add(T item)
			{
				this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, -1));
			}

			void ICollection<T>.Clear()
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
			}

			bool ICollection<T>.Contains(T item)
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
				return false;
			}

			void ICollection<T>.CopyTo(T[] array, int arrayIndex)
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
			}

			int ICollection<T>.Count
			{
				get
				{
					Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
					return 0;
				}
			}

			bool ICollection<T>.IsReadOnly
			{
				get { return false; }
			}

			bool ICollection<T>.Remove(T item)
			{
				this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, -1));
				return true;
			}

			#endregion

			#region IEnumerable<T> Members

			IEnumerator<T> IEnumerable<T>.GetEnumerator()
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
				return null;
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator()
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
				return null;
			}

			#endregion

			#region IList<T> Members

			int IList<T>.IndexOf(T item)
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
				return -1;
			}

			void IList<T>.Insert(int index, T item)
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
			}

			void IList<T>.RemoveAt(int index)
			{
				Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
			}

			T IList<T>.this[int index]
			{
				get
				{
					Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
					return default(T);
				}
				set
				{
					Debug.Assert(false, "Unexpected call to the ProxyItemsSource");
				}
			}

			#endregion

			#region INotifyCollectionChanged Members

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			#endregion

			#endregion  // Interfaces

			#region Methods

			#region RaiseCollectionChanged

			private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
			{
				if (this.CollectionChanged != null)
					this.CollectionChanged(this, args);
			}

			#endregion // RaiseCollectionChanged

			#region Reset

			public void Reset()
			{
				this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}

			#endregion  // Reset

			#endregion // Methods
		}

		#endregion  // ProxyItemsSource class

		#region PerformActivityOperationEndEditCallState class

		private class PerformActivityOperationEndEditCallState
		{
			public IEditList<ActivityBase> editList;
			public bool force;
			public ActivityOperationResult result;
		} 

		#endregion  // PerformActivityOperationEndEditCallState class

		#region QueryActivitiesCallState class

		private class QueryActivitiesCallState
		{
			public object context;
			public Action<object, IEnumerable, DataErrorInfo> provideQueryResultData;
		}

		#endregion  // QueryActivitiesCallState class

		#region RemoteProperties enum

		private enum RemoteProperties : byte
		{
			None = 0,
			EndpointConfigurationName = 1,
			RemoteAddress = 2,
			RemoteBinding = 4,
		} 

		#endregion  // RemoteProperties enum
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