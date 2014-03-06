using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Linq;
using System.Windows.Data;
using Infragistics.Collections;
using Infragistics.Controls.Schedules.Primitives;

namespace Infragistics.Controls.Schedules
{
	#region ListScheduleDataConnector Class

	/// <summary>
	/// Used for providing schedule data from various data sources to <see cref="XamScheduleDataManager"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ListScheduleDataConnector</b> exposes properties for specifying data sources from which the data for appointments, 
	/// tasks, journals, resources and resource calendars are retrieved from. These schedule objects are then provided to the
	/// <see cref="XamScheduleDataManager"/> whose <see cref="XamScheduleDataManager.DataConnector"/> property
	/// is set to this instance of <i>ListScheduleDataConnector</i>.
	/// </para>
	/// <para class="body">
	/// Not all data sources are required. Minimally however the <see cref="ResourceItemsSource"/> and one of
	/// <see cref="AppointmentItemsSource"/>, <see cref="TaskItemsSource"/> or <see cref="JournalItemsSource"/>
	/// is required for a useful setup. If you want to utilize multiple calendars per resource capability then
	/// you need to specify <see cref="ResourceCalendarItemsSource"/> as well.
	/// </para>
	/// <para class="body">
	/// The data sources can be any objects that implement <i>IEnumerable</i> interface. You need to specify property
	/// mappings that map properties or fields of the data source objects to the properties of the corresponding schedule 
	/// objects using corresponding property mapping collections exposed on the class. For example 
	/// <see cref="ListScheduleDataConnector.AppointmentPropertyMappings"/> is used to map properties or fields of 
	/// the data objects in the <see cref="ListScheduleDataConnector.AppointmentItemsSource"/> to the properties of
	/// the <see cref="Appointment"/> clas. You can set the <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> 
	/// property on the mappings collection to indicate that the property or field names in the data items are the same as 
	/// the names of properties in the corresponding class in the schedule object model. If a property's name is different then 
	/// you can add an entry in the mapping collection for that property.
	/// </para>
	/// <para class="body">
	/// For example, if you set <i>UseDefaultMappings</i> to true on the <i>AppointmentPropertyMappings</i> collection,
	/// the data objects in the <see cref="ListScheduleDataConnector.AppointmentItemsSource"/> are assumed 
	/// to contain properties or fields with the same names as the properties of the <see cref="Appointment"/> 
	/// class. If one or more properties' names do not match, you can add entries in the mapping collection for
	/// those properties. Other properties will still use the default mappings. If you leave <i>UseDefaultMappings</i>
	/// to false, then you need to add an entry in the mapping collection each property that you want to map.
	/// Data for certain properties are required for proper functioning. See <see cref="AppointmentProperty"/>,
	/// <see cref="JournalProperty"/>, <see cref="TaskProperty"/>, <see cref="ResourceProperty"/> and 
	/// <see cref="ResourceCalendarProperty"/> enums for more information on which properties are required.
	/// </para>
	/// </remarks>
	/// <seealso cref="ListScheduleDataConnector"/>
	/// <seealso cref="XamScheduleDataManager.DataConnector"/>
	public class ListScheduleDataConnector : ScheduleDataConnectorBase
	{
		#region Member Vars

		internal AppointmentListManager _appointmentListManager;
        internal JournalListManager _journalListManager;
        internal AppointmentListManager _recurringAppointmentListManager;
        internal TaskListManager _taskListManager;
		internal ResourceListManager _resourceListManager;
        internal ResourceCalendarListManager _resourceCalendarListManager;
		internal ActivityCategoryListManager _activityCategoryListManager;
		// SSP 1/6/12 - NAS12.1 XamGantt
		// 
		//internal ProjectListManager _projectListManager;

		internal readonly List<IListManager> _listManagers;

		private AggregateQueries _aggregateQueries;
		private ListConnectorActivityReminderManager _reminderManager;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ScheduleDataConnectorBase"/>.
		/// </summary>
		public ListScheduleDataConnector( )
		{
			_listManagers = new List<IListManager>( );
			_appointmentListManager = Manage( new AppointmentListManager( this ) );
			_journalListManager = Manage( new JournalListManager( this ) );
			_recurringAppointmentListManager = Manage( new AppointmentListManager( this ) );
			_taskListManager = Manage( new TaskListManager( this ) );
			_resourceListManager = Manage( new ResourceListManager( this ) );
            _resourceCalendarListManager = Manage( new ResourceCalendarListManager( this ) );

			// SSP 12/9/10 - NAS11.1 Activity Categories
			// 
			_activityCategoryListManager = Manage( new ActivityCategoryListManager( this ) );

			// SSP 1/6/12 - NAS12.1 XamGantt
			// 
			//_projectListManager = Manage( new ProjectListManager( this ) );

			// List managers need to know whether they contain recurring activities or non-recurring
			// activities or both. To determine which of these cases applies, they need to check
			// whether the other list manager has data and what type of data etc...
			// 
			_appointmentListManager.SetRecurringListManager( _recurringAppointmentListManager );

			_aggregateQueries = new AggregateQueries( this );
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region ResourceItems

		/// <summary>
		/// Gets the resources collection.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the changes to the returned collection will be synchronized with the 
		/// the data items in the <see cref="ResourceItemsSource"/>.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResourceItemsSource" />
		public override ResourceCollection ResourceItems
		{
			get
			{
				return _resourceListManager.Resources;
			}
		}

		#endregion // ResourceItems

		#endregion // Properties

		#region Methods

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
		internal protected override bool AreActivitiesSupported( ActivityType activityType )
		{
			bool ret;

			switch ( activityType )
			{
				case ActivityType.Appointment:
					ret = _appointmentListManager.HasList;
					break;
				case ActivityType.Journal:
					ret = _journalListManager.HasList;
					break;
				case ActivityType.Task:
					ret = _taskListManager.HasList;
					break;
				default:
					Debug.Assert( false );
					ret = false;
					break;
			}

			return ret;
		} 

		#endregion // AreActivitiesSupported

		#region BeginEdit

		/// <summary>
		/// Begins modifications to an activity.
		/// </summary>
		/// <param name="activity">ActivityBase derived object that is to be modified.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A value indicating whether the operation succeeded.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> BeginEdit can be called multiple times for an activity without intervening EndEdit or CancelEdit calls. Successive BeginEdit
		/// calls should result in NOOP and return true and null error information. However note that only a single EndEdit or CancelEdit call may 
		/// be made to come out of edit state.
		/// </para>
		/// </remarks>
		protected internal override bool BeginEdit( ActivityBase activity, out DataErrorInfo errorInfo )
		{
			return ListScheduleDataConnectorUtilities<ActivityBase>.BeginEdit(
				this, activity,
				new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditListHelper), 
				out errorInfo);
		}

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Begins modifications to a Resource object.
		/// </summary>
		/// <param name="resource">Resource to be modified.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>A value indicating whether the operation succeeded.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> BeginEdit cannot be called more than once without an intervening call to CancelEdit or EndEdit. Successive BeginEdit
		/// calls should result an error and return false.
		/// </para>
		/// </remarks>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected override bool BeginEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			return ListScheduleDataConnectorUtilities<Resource>.BeginEdit(
				this, resource,
				ii => _resourceListManager,
				out errorInfo );
		}

		#endregion // BeginEdit

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
        /// <b>Note:</b> <b>CancelEdit</b> method is called by the <see cref="XamScheduleDataManager"/>
        /// to cancel a new activity that was created using the <see cref="CreateNew"/> method
        /// however the activity must not have been commited yet. This is typically done when the user 
        /// cancels the dialog for creating a new activity, like the new appointment dialog.
        /// </para>
        /// </remarks>
        /// <seealso cref="EndEdit(ActivityBase, bool)"/>
        /// <seealso cref="BeginEdit(ActivityBase, out DataErrorInfo)"/>
        /// <seealso cref="Remove"/>
		internal protected override bool CancelEdit( ActivityBase activity, out DataErrorInfo errorInfo )
        {
			return ListScheduleDataConnectorUtilities<ActivityBase>.DefaultCancelEditImplementation( activity, this.GetEditListHelper, out errorInfo );
        }

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Cancels modifications to a Resource object.
		/// </summary>
		/// <param name="resource">Resource object that was put in edit state by <see cref="BeginEdit(Resource, out DataErrorInfo)"/> call.</param>
		/// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
		/// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>CancelEdit</b> method is called by the <see cref="XamScheduleDataManager"/>
		/// to cancel modifications to a Resource object.
		/// </para>
		/// </remarks>
		/// <seealso cref="EndEdit(Resource, bool)"/>
		/// <seealso cref="CancelEdit(Resource, out DataErrorInfo)"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected override bool CancelEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			return ListScheduleDataConnectorUtilities<Resource>.DefaultCancelEditImplementation( resource, ii => _resourceListManager, out errorInfo );
		}

        #endregion // CancelEdit

		#region CancelPendingOperation

		/// <summary>
		/// Cancels a pending operation.
		/// </summary>
		/// <param name="operation">Pending operation.</param>
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
		internal protected override CancelOperationResult CancelPendingOperation( OperationResult operation )
		{
			Debug.Assert( false, "Since list connector doesn't perform any operations asynchronously, no result should be pending." );

			return new CancelOperationResult( operation, null, true );
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
		/// so the activity object can be properly discarded by the the data connector.
		/// </para>
		/// <para class="body">
		/// <b>Note:</b> <b>CreateNew</b> method is called by the <see cref="XamScheduleDataManager"/> to create a 
		/// new Appointment, Journal or Task object. This is typically done when the user initiates creation 
		/// of a new activity in one of the calendar view controls. If the user commits the appointment then 
		/// <i>EndEdit</i> method is called to commit the activity. If the user cancels the activity creation 
		/// then <i>CancelEdit</i> method is called to discard the activity object.
		/// </para>
		/// </remarks>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		/// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="Remove"/>
		internal protected override ActivityBase CreateNew( ActivityType activityType, out DataErrorInfo errorInfo )
		{
			IEditList<ActivityBase> editList = this.GetEditListHelper( activityType );

			errorInfo = null;
			return null != editList ? editList.CreateNew( out errorInfo ) : null;
		}

		#endregion // CreateNew

        #region EndEdit

        /// <summary>
        /// Commits a new or modified activity.
        /// </summary>
        /// <param name="activity">A new or modified ActivityBase derived instance.</param>
		/// <param name="force">True to force the edit operation to end. Used when user interface
		/// being used to perform the edit operation cannot remain in edit mode and therefore the
		/// edit operation must be ended. If the specified activity is deemed invalid to be committed
		/// then an error result should be returned.
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
		internal protected override ActivityOperationResult EndEdit( ActivityBase activity, bool force )
        {
			return ListScheduleDataConnectorUtilities<ActivityBase>.EndEdit(
				this, activity, force,
				( editList, result, forceArg ) => this.EndEditOverride( editList, result as ActivityOperationResult, forceArg ),
				this.GetEditListHelper ) as ActivityOperationResult;
		}

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
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
		/// <seealso cref="CancelEdit(Resource, out DataErrorInfo)"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected override ResourceOperationResult EndEdit( Resource resource, bool force )
		{
			DataErrorInfo error;
			if (this.ValidateResource(resource, out error) == false)
			{
				DataErrorInfo tmp;
				this.CancelEdit(resource, out tmp);

				return new ResourceOperationResult(resource, error, true);
			}

			return ListScheduleDataConnectorUtilities<Resource>.EndEdit(
				this, resource, force,
				( editList, result, forceArg ) => this.EndEditOverride( editList, result as ResourceOperationResult, forceArg ),
				ii => _resourceListManager ) as ResourceOperationResult;
		}

        #endregion // EndEdit

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
        /// <para class="body">
		/// <b>Note:</b> If there's an error the returned ActivityQueryResult's <see cref="OperationResult.Error"/> 
        /// property will return the error information.
        /// </para>
        /// </remarks>
		internal protected override ActivityQueryResult GetActivities( ActivityQuery query )
        {
            return _aggregateQueries.PerformQuery( query );
        }

        #endregion // GetActivities

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

		internal protected override IEnumerable<ActivityCategory> GetSupportedActivityCategories( ActivityBase activity )
		{
			return _activityCategoryListManager.GetCategories( activity, true );
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
		/// <seealso cref="IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		internal protected override bool IsActivityFeatureSupported( ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar )
		{
			IActivityListManager listManager = this.GetEditListHelper( activityType ) as IActivityListManager;
			Debug.Assert( null != listManager );

			return null != listManager && listManager.IsActivityFeatureSupported( activityFeature );
		}

		#endregion // IsActivityFeatureSupported

		#region IsActivityOperationSupported

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
		protected internal override bool IsActivityOperationSupported( ActivityBase activity, ActivityOperation activityOperation )
		{
			return this.IsActivityOperationSupportedHelper( activity.ActivityType, activityOperation, activity.OwningCalendar, activity );
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
		internal protected override bool IsActivityOperationSupported( ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar )
		{
			return this.IsActivityOperationSupportedHelper( activityType, activityOperation, calendar );
		}

		private bool IsActivityOperationSupportedHelper( ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar, ActivityBase activityContext = null )
		{
			bool ret = false;

			IEditList<ActivityBase> editList = this.GetEditListHelper( activityType );
			if ( null != editList )
			{
				switch ( activityOperation )
				{
					case ActivityOperation.Add:
						ret = editList.AllowAdd;
						break;
					case ActivityOperation.Edit:
						ret = editList.AllowEdit;
						break;
					case ActivityOperation.Remove:
						ret = editList.AllowRemove;
						break;
					default:
						Debug.Assert( false );
						ret = false;
						break;
				}

				if ( ret && null != activityContext )
				{
					// If variances are not supported then return false if an occurrence is being modified.
					// 
					if ( activityContext.IsOccurrence && !this.IsActivityFeatureSupported( activityType, ActivityFeature.Variance, calendar ) )
						ret = false;
				}
			}

			return ret;
		}

		#endregion // IsActivityOperationSupported

		#region IsResourceFeatureSupported

		/// <summary>
		/// Indicates whether the specified feature is supported for the specified resource.
		/// </summary>
		/// <param name="resource">Resource instance.</param>
		/// <param name="resourceFeature">Resource feature.</param>
		/// <returns>True if the feature is supported. False otherwise.</returns>
		/// <remarks>
		/// <para class="body">
		/// This method is used by the data manager to determine if the specified feature is supprted for a resource.
		/// If the feature is not supported, relevant user interface will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>

		[InfragisticsFeature( FeatureName = "ActivityCategories", Version = "11.1" )]

		internal protected override bool IsResourceFeatureSupported( Resource resource, ResourceFeature resourceFeature )
		{
			return _resourceListManager.IsFeatureSupported( resourceFeature );
		}

		#endregion // IsResourceFeatureSupported

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
        /// <seealso cref="EndEdit(ActivityBase, bool)"/>
        /// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
		internal protected override ActivityOperationResult Remove( ActivityBase activity )
        {
			return ListScheduleDataConnectorUtilities<ActivityBase>.Remove(
				this, activity,
				( editList, result, force ) =>  this.EndEditOverride( editList, result as ActivityOperationResult, force ),
				new Func<ActivityBase, IEditList<ActivityBase>>(this.GetEditListHelper));
        }

        #endregion // Remove

		#region ResolveActivityCategories

		/// <summary>
		/// Gets the activity categories for the specified activity. Default implementation returns <i>null</i>.
		/// </summary>
		/// <param name="activity">Activity for which to get the list of activity categories.</param>
		/// <returns>IEnumerable that can optionally implement INotifyCollectionChanged to notify
		/// of changes to the list.</returns>
		/// <seealso cref="GetSupportedActivityCategories"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected override IEnumerable<ActivityCategory> ResolveActivityCategories( ActivityBase activity )
		{
			return _activityCategoryListManager.GetCategories( activity, false );
		}

		#endregion // ResolveActivityCategories

		#region SubscribeToReminders

		/// <summary>
		/// Subscribes to reminders for the activities of the specified calendar. Note that more than one 
		/// subscriber can be subscribed to a single calendar as well as multiple calendars can be 
		/// subscribed to.
		/// </summary>
		/// <param name="calendar">This calendars activity reminders will be deilvered to the specified subscribed.</param>
		/// <param name="subscriber">When a reminder is due for an activity, the subscriber's <see cref="ReminderSubscriber.DeliverReminder"/> method will be invoked.</param>
		/// <param name="error">If there's an error, this will be set to error information.</param>
		internal protected override void SubscribeToReminders( ResourceCalendar calendar, ReminderSubscriber subscriber, out DataErrorInfo error )
		{
			this.ReminderManager.SubscribeToReminders( calendar, subscriber, out error );
		}

		#endregion // SubscribeToReminders

		#region UnsubscribeFromReminders

		/// <summary>
		/// Unsubscribes from activity reminders of the specified calendar. If the specified subscriber hadn't been subscribed
		/// previously then this method will take no action.
		/// </summary>
		/// <param name="calendar">The calendar's activity reminders to unsubscribe from.</param>
		/// <param name="subscriber">Subscriber instance.</param>
		internal protected override void UnsubscribeFromReminders( ResourceCalendar calendar, ReminderSubscriber subscriber )
		{
			this.ReminderManager.UnsubscribeFromReminders( calendar, subscriber );
		}

		#endregion // UnsubscribeFromReminders

        #endregion // Methods

        #endregion // Base Overrides

        #region Properties

        #region Public Properties

		#region ActivityCategoryItemsSource

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 

		/// <summary>
		/// Identifies the <see cref="ActivityCategoryItemsSource"/> dependency property.
		/// </summary>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public static readonly DependencyProperty ActivityCategoryItemsSourceProperty = DependencyPropertyUtilities.Register(
			"ActivityCategoryItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnActivityCategoryItemsSourceChanged ) )
		);

		private static void OnActivityCategoryItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._activityCategoryListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the activityCategorys are stored.
		/// </summary>
		/// <seealso cref="ActivityCategoryItemsSourceProperty"/>
		/// <seealso cref="ActivityCategoryPropertyMappings"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public IEnumerable ActivityCategoryItemsSource
		{
			get
			{
				return _activityCategoryListManager.List;
			}
			set
			{
				this.SetValue( ActivityCategoryItemsSourceProperty, value );
			}
		}

		#endregion // ActivityCategoryItemsSource

		#region ActivityCategoryPropertyMappings

		// SSP 12/8/10 - NAS11.1 Activity Categories
		// 

		/// <summary>
		/// Identifies the <see cref="ActivityCategoryPropertyMappings"/> dependency property.
		/// </summary>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public static readonly DependencyProperty ActivityCategoryPropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"ActivityCategoryPropertyMappings",
			typeof( ActivityCategoryPropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnActivityCategoryPropertyMappingsChanged ) )
		);

		private static void OnActivityCategoryPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			ActivityCategoryPropertyMappingCollection val = (ActivityCategoryPropertyMappingCollection)e.NewValue;

			item._activityCategoryListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="ActivityCategory"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ActivityCategoryPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="ActivityCategoryItemsSource"/> provide data for which properties of the
		/// <see cref="ActivityCategory"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>ActivityCategoryPropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="ActivityCategoryProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="ActivityCategoryPropertyMappingCollection"/>
		/// <seealso cref="ActivityCategoryPropertyMapping"/>
		/// <seealso cref="ActivityCategoryProperty"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		public ActivityCategoryPropertyMappingCollection ActivityCategoryPropertyMappings
		{
			get
			{
				return (ActivityCategoryPropertyMappingCollection)_activityCategoryListManager.Mappings;
			}
			set
			{
				this.SetValue( ActivityCategoryPropertyMappingsProperty, value );
			}
		}

		#endregion // ActivityCategoryPropertyMappings

		#region AppointmentItemsSource

		/// <summary>
		/// Identifies the <see cref="AppointmentItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AppointmentItemsSourceProperty = DependencyPropertyUtilities.Register(
			"AppointmentItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnAppointmentItemsSourceChanged ) )
		);

		private static void OnAppointmentItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._appointmentListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the appointments are stored.
		/// </summary>
		/// <seealso cref="AppointmentItemsSourceProperty"/>
		/// <seealso cref="AppointmentPropertyMappings"/>
		public IEnumerable AppointmentItemsSource
		{
			get
			{
				return _appointmentListManager.List;
			}
			set
			{
				this.SetValue( AppointmentItemsSourceProperty, value );
			}
		}

		#endregion // AppointmentItemsSource

        #region AppointmentPropertyMappings

        /// <summary>
		/// Identifies the <see cref="AppointmentPropertyMappings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty AppointmentPropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"AppointmentPropertyMappings",
			typeof( AppointmentPropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnAppointmentPropertyMappingsChanged ) )
		);

		private static void OnAppointmentPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			AppointmentPropertyMappingCollection val = (AppointmentPropertyMappingCollection)e.NewValue;
			
			item._appointmentListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Appointment"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>AppointmentPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="AppointmentItemsSource"/> provide data for which properties of the
		/// <see cref="Appointment"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>AppointmentPropertyMappingCollection</i> to true if the field names 
		/// in the data source are the same as the property names as defined by the <see cref="AppointmentProperty"/> 
		/// enum. You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having the 
		/// rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="AppointmentPropertyMappingCollection"/>
		/// <seealso cref="AppointmentPropertyMapping"/>
		/// <seealso cref="AppointmentProperty"/>
		public AppointmentPropertyMappingCollection AppointmentPropertyMappings
		{
			get
			{
				return (AppointmentPropertyMappingCollection)_appointmentListManager.Mappings;
			}
			set
			{
				this.SetValue( AppointmentPropertyMappingsProperty, value );
			}
		}

		#endregion // AppointmentPropertyMappings

		#region JournalItemsSource

		/// <summary>
		/// Identifies the <see cref="JournalItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty JournalItemsSourceProperty = DependencyPropertyUtilities.Register(
			"JournalItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnJournalItemsSourceChanged ) )
		);

		private static void OnJournalItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._journalListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the journals are stored.
		/// </summary>
		/// <seealso cref="JournalItemsSourceProperty"/>
		/// <seealso cref="JournalPropertyMappings"/>
		public IEnumerable JournalItemsSource
		{
			get
			{
				return _journalListManager.List;
			}
			set
			{
				this.SetValue( JournalItemsSourceProperty, value );
			}
		}

		#endregion // JournalItemsSource

		#region JournalPropertyMappings

		/// <summary>
		/// Identifies the <see cref="JournalPropertyMappings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty JournalPropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"JournalPropertyMappings",
			typeof( JournalPropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnJournalPropertyMappingsChanged ) )
		);

		private static void OnJournalPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			JournalPropertyMappingCollection val = (JournalPropertyMappingCollection)e.NewValue;

			item._journalListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Journal"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>JournalPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="JournalItemsSource"/> provide data for which properties of the
		/// <see cref="Journal"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>JournalPropertyMappingCollection</i>
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="JournalProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="JournalPropertyMappingCollection"/>
		/// <seealso cref="JournalPropertyMapping"/>
		/// <seealso cref="JournalProperty"/>
		public JournalPropertyMappingCollection JournalPropertyMappings
		{
			get
			{
				return (JournalPropertyMappingCollection)_journalListManager.Mappings;
			}
			set
			{
				this.SetValue( JournalPropertyMappingsProperty, value );
			}
		}

		#endregion // JournalPropertyMappings


		#region ProjectItemsSource

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 

//        /// <summary>
//        /// Identifies the <see cref="ProjectItemsSource"/> dependency property.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public static readonly DependencyProperty ProjectItemsSourceProperty = DependencyPropertyUtilities.Register(
//            "ProjectItemsSource",
//            typeof( IEnumerable ),
//            typeof( ListScheduleDataConnector ),
//            DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnProjectItemsSourceChanged ) )
//        );

//        private static void OnProjectItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
//        {
//            ListScheduleDataConnector item = (ListScheduleDataConnector)d;
//            IEnumerable val = (IEnumerable)e.NewValue;

//            item._projectListManager.List = val;
//        }

//        /// <summary>
//        /// Specifies the data source where the projects are stored.
//        /// </summary>
//        /// <seealso cref="ProjectItemsSourceProperty"/>
//        /// <seealso cref="ProjectPropertyMappings"/>
//        /// <seealso cref="Project"/>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public IEnumerable ProjectItemsSource
//        {
//            get
//            {
//                return _projectListManager.List;
//            }
//            set
//            {
//                this.SetValue( ProjectItemsSourceProperty, value );
//            }
//        }

		#endregion // ProjectItemsSource

		#region ProjectPropertyMappings

		// SSP 1/6/12 - NAS12.1 XamGantt
		// 

//        /// <summary>
//        /// Identifies the <see cref="ProjectPropertyMappings"/> dependency property.
//        /// </summary>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public static readonly DependencyProperty ProjectPropertyMappingsProperty = DependencyPropertyUtilities.Register(
//            "ProjectPropertyMappings",
//            typeof( ProjectPropertyMappingCollection ),
//            typeof( ListScheduleDataConnector ),
//            DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnProjectPropertyMappingsChanged ) )
//        );

//        private static void OnProjectPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
//        {
//            ListScheduleDataConnector item = (ListScheduleDataConnector)d;
//            ProjectPropertyMappingCollection val = (ProjectPropertyMappingCollection)e.NewValue;

//            item._projectListManager.Mappings = val;
//        }

//        /// <summary>
//        /// Specifies the data source field mappings for the <see cref="Project"/> object.
//        /// </summary>
//        /// <remarks>
//        /// <para class="body">
//        /// <b>ProjectPropertyMappings</b> property is used to specify which fields in the 
//        /// <see cref="ProjectItemsSource"/> provide data for which properties of the
//        /// <see cref="Project"/> object.
//        /// </para>
//        /// <para class="body">
//        /// This property by default returns an empty collection. You can set the 
//        /// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
//        /// on the returned <i>ProjectPropertyMappingCollection</i>  
//        /// to true if the field names in the data source are the same as the 
//        /// property names as defined by the <see cref="ProjectProperty"/> enum.
//        /// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
//        /// fields to override the field mappings for those particular fields while having
//        /// the rest of the fields use the default field mappings.
//        /// </para>
//        /// </remarks>
//        /// <seealso cref="ProjectPropertyMappingCollection"/>
//        /// <seealso cref="ProjectPropertyMapping"/>
//        /// <seealso cref="ProjectProperty"/>
//#if WPF && !WCFService
//        [InfragisticsFeature( FeatureName = "XamGantt", Version = "12.1" )]
//#endif
//        public ProjectPropertyMappingCollection ProjectPropertyMappings
//        {
//            get
//            {
//                return (ProjectPropertyMappingCollection)_projectListManager.Mappings;
//            }
//            set
//            {
//                this.SetValue( ProjectPropertyMappingsProperty, value );
//            }
//        }

		#endregion // ProjectPropertyMappings

		#region RecurringAppointmentItemsSource

		/// <summary>
		/// Identifies the <see cref="RecurringAppointmentItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty RecurringAppointmentItemsSourceProperty = DependencyPropertyUtilities.Register(
			"RecurringAppointmentItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnRecurringAppointmentItemsSourceChanged ) )
		);

		private static void OnRecurringAppointmentItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._recurringAppointmentListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the recurring appointments are stored.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecurringAppointmentItemsSource</b> specifies the data source where the recurring
		/// appointments are stored. This is optional. If not specified then the recurring appointments
		/// will be stored in the data source specified by <see cref="AppointmentItemsSource"/>.
		/// If specified then the recurring appointments, including variances, will be stored in this 
		/// data source.
		/// </para>
		/// <para class="body">
		/// Note that if this data source is specified then all the appointments in the 
		/// <i>AppointmentItemsSource</i> will be assumed to be non-recurring appointments. 
		/// In other words it's not allowed for both the <i>AppointmentItemsSource</i> and 
		/// <i>RecurringAppointmentItemsSource</i> data sources to contain recurring appointments.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecurringAppointmentPropertyMappings"/>
		public IEnumerable RecurringAppointmentItemsSource
		{
			get
			{
				return _recurringAppointmentListManager.List;
			}
			set
			{
				this.SetValue( RecurringAppointmentItemsSourceProperty, value );
			}
		}

		#endregion // RecurringAppointmentItemsSource

		#region RecurringAppointmentPropertyMappings

		/// <summary>
		/// Identifies the <see cref="RecurringAppointmentPropertyMappings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty RecurringAppointmentPropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"RecurringAppointmentPropertyMappings",
			typeof( AppointmentPropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnRecurringAppointmentPropertyMappingsChanged ) )
		);

		private static void OnRecurringAppointmentPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			AppointmentPropertyMappingCollection val = (AppointmentPropertyMappingCollection)e.NewValue;

			item._recurringAppointmentListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Appointment"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecurringAppointmentPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="RecurringAppointmentItemsSource"/> provide data for which properties of the
		/// <see cref="Appointment"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>AppointmentPropertyMappingCollection</i> to true if the field names 
		/// in the data source are the same as the property names as defined by the <see cref="AppointmentProperty"/> 
		/// enum. You can also set <i>UseDefaultMappings</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having the 
		/// rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="AppointmentPropertyMappingCollection"/>
		/// <seealso cref="AppointmentPropertyMapping"/>
		/// <seealso cref="AppointmentProperty"/>
		/// <seealso cref="RecurringAppointmentItemsSource"/>
		public AppointmentPropertyMappingCollection RecurringAppointmentPropertyMappings
		{
			get
			{
				return (AppointmentPropertyMappingCollection)_recurringAppointmentListManager.Mappings;
			}
			set
			{
				this.SetValue( RecurringAppointmentPropertyMappingsProperty, value );
			}
		}

		#endregion // RecurringAppointmentPropertyMappings

		#region ResourceItemsSource

		/// <summary>
		/// Identifies the <see cref="ResourceItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResourceItemsSourceProperty = DependencyPropertyUtilities.Register(
			"ResourceItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnResourceItemsSourceChanged ) )
		);

		private static void OnResourceItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._resourceListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the resources are stored.
		/// </summary>
		/// <seealso cref="ResourceItemsSourceProperty"/>
		/// <seealso cref="ResourcePropertyMappings"/>
		public IEnumerable ResourceItemsSource
		{
			get
			{
				return _resourceListManager.List;
			}
			set
			{
				this.SetValue( ResourceItemsSourceProperty, value );
			}
		}

		#endregion // ResourceItemsSource

		#region ResourcePropertyMappings

		/// <summary>
		/// Identifies the <see cref="ResourcePropertyMappings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResourcePropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"ResourcePropertyMappings",
			typeof( ResourcePropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnResourcePropertyMappingsChanged ) )
		);

		private static void OnResourcePropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			ResourcePropertyMappingCollection val = (ResourcePropertyMappingCollection)e.NewValue;

			item._resourceListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Resource"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>ResourcePropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="ResourceItemsSource"/> provide data for which properties of the
		/// <see cref="Resource"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>ResourcePropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="ResourceProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="ResourcePropertyMappingCollection"/>
		/// <seealso cref="ResourcePropertyMapping"/>
		/// <seealso cref="ResourceProperty"/>
		public ResourcePropertyMappingCollection ResourcePropertyMappings
		{
			get
			{
				return (ResourcePropertyMappingCollection)_resourceListManager.Mappings;
			}
			set
			{
				this.SetValue( ResourcePropertyMappingsProperty, value );
			}
		}

		#endregion // ResourcePropertyMappings

		#region ResourceCalendarItemsSource

		/// <summary>
		/// Identifies the <see cref="ResourceItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ResourceCalendarItemsSourceProperty = DependencyPropertyUtilities.Register(
			"ResourceCalendarItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnResourceCalendarItemsSourceChanged ) )
		);

		private static void OnResourceCalendarItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._resourceCalendarListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the resource calendars are stored.
		/// </summary>
		/// <seealso cref="ResourceCalendarItemsSourceProperty"/>
		/// <seealso cref="ResourceCalendarPropertyMappings"/>
		public IEnumerable ResourceCalendarItemsSource
		{
			get
			{
				return _resourceCalendarListManager.List;
			}
			set
			{
				this.SetValue( ResourceCalendarItemsSourceProperty, value );
			}
		}

		#endregion // ResourceCalendarItemsSource

        #region ResourceCalendarPropertyMappings

        /// <summary>
        /// Identifies the <see cref="ResourcePropertyMappings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResourceCalendarPropertyMappingsProperty = DependencyPropertyUtilities.Register(
            "ResourceCalendarPropertyMappings",
            typeof( ResourceCalendarPropertyMappingCollection ),
            typeof( ListScheduleDataConnector ),
            DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnResourceCalendarPropertyMappingsChanged ) )
        );

        private static void OnResourceCalendarPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ListScheduleDataConnector item = (ListScheduleDataConnector)d;
            ResourceCalendarPropertyMappingCollection val = (ResourceCalendarPropertyMappingCollection)e.NewValue;

            item._resourceCalendarListManager.Mappings = val;
        }

        /// <summary>
        /// Specifies the data source field mappings for the <see cref="ResourceCalendar"/> object.
        /// </summary>
        /// <remarks>
        /// <para class="body">
        /// <b>ResourceCalendarPropertyMappings</b> property is used to specify which fields in the 
        /// <see cref="ResourceItemsSource"/> provide data for which properties of the
        /// <see cref="Resource"/> object.
        /// </para>
        /// <para class="body">
        /// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
        /// on the returned <i>ResourceCalendarPropertyMappingCollection</i>  
        /// to true if the field names in the data source are the same as the 
        /// property names as defined by the <see cref="ResourceProperty"/> enum.
        /// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
        /// fields to override the field mappings for those particular fields while having
        /// the rest of the fields use the default field mappings.
        /// </para>
        /// </remarks>
        /// <seealso cref="ResourceCalendarPropertyMappingCollection"/>
        /// <seealso cref="ResourceCalendarPropertyMapping"/>
        /// <seealso cref="ResourceCalendarProperty"/>
        public ResourceCalendarPropertyMappingCollection ResourceCalendarPropertyMappings
        {
            get
            {
                return (ResourceCalendarPropertyMappingCollection)_resourceCalendarListManager.Mappings;
            }
            set
            {
                this.SetValue( ResourceCalendarPropertyMappingsProperty, value );
            }
        }

        #endregion // ResourceCalendarPropertyMappings

		#region TaskItemsSource

		/// <summary>
		/// Identifies the <see cref="TaskItemsSource"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TaskItemsSourceProperty = DependencyPropertyUtilities.Register(
			"TaskItemsSource",
			typeof( IEnumerable ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnTaskItemsSourceChanged ) )
		);

		private static void OnTaskItemsSourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			IEnumerable val = (IEnumerable)e.NewValue;

			item._taskListManager.List = val;
		}

		/// <summary>
		/// Specifies the data source where the tasks are stored.
		/// </summary>
		/// <seealso cref="TaskItemsSourceProperty"/>
		/// <seealso cref="TaskPropertyMappings"/>
		public IEnumerable TaskItemsSource
		{
			get
			{
				return _taskListManager.List;
			}
			set
			{
				this.SetValue( TaskItemsSourceProperty, value );
			}
		}

		#endregion // TaskItemsSource

        #region TaskPropertyMappings

        /// <summary>
		/// Identifies the <see cref="TaskPropertyMappings"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty TaskPropertyMappingsProperty = DependencyPropertyUtilities.Register(
			"TaskPropertyMappings",
			typeof( TaskPropertyMappingCollection ),
			typeof( ListScheduleDataConnector ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnTaskPropertyMappingsChanged ) )
		);

		private static void OnTaskPropertyMappingsChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ListScheduleDataConnector item = (ListScheduleDataConnector)d;
			TaskPropertyMappingCollection val = (TaskPropertyMappingCollection)e.NewValue;

			item._taskListManager.Mappings = val;
		}

		/// <summary>
		/// Specifies the data source field mappings for the <see cref="Task"/> object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>TaskPropertyMappings</b> property is used to specify which fields in the 
		/// <see cref="TaskItemsSource"/> provide data for which properties of the
		/// <see cref="Task"/> object.
		/// </para>
		/// <para class="body">
		/// This property by default returns an empty collection. You can set the 
		/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.UseDefaultMappings"/> property 
		/// on the returned <i>TaskPropertyMappingCollection</i>  
		/// to true if the field names in the data source are the same as the 
		/// property names as defined by the <see cref="TaskProperty"/> enum.
		/// You can also set <i>UseDefaultMapping</i> to true and add entries for specific
		/// fields to override the field mappings for those particular fields while having
		/// the rest of the fields use the default field mappings.
		/// </para>
		/// </remarks>
		/// <seealso cref="TaskPropertyMappingCollection"/>
		/// <seealso cref="TaskPropertyMapping"/>
		/// <seealso cref="TaskProperty"/>
		public TaskPropertyMappingCollection TaskPropertyMappings
		{
			get
			{
				return (TaskPropertyMappingCollection)_taskListManager.Mappings;
			}
			set
			{
				this.SetValue( TaskPropertyMappingsProperty, value );
			}
		}

		#endregion // TaskPropertyMappings

		#endregion // Public Properties

		#region Private Properties

		#region ReminderManager

		private ListConnectorActivityReminderManager ReminderManager
		{
			get
			{
				if ( null == _reminderManager )
					_reminderManager = new ListConnectorActivityReminderManager( this );

				return _reminderManager;
			}
		} 

		#endregion // ReminderManager
		
		#endregion // Private Properties

		#region Internal Properties

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#endregion // Public Methods

		#region Internal Methods

		#region EndEditOverride

		/// <summary>
		/// This is called to commit changes to an activity to the underlying data source.
		/// </summary>
		/// <param name="editList">This is the list manager.</param>
		/// <param name="result">Result's Activity is being updated. This result should be initialized with the result of the opreation.</param>
		/// <param name="force">True if the UI cannot remain in edit mode and therefore the operation must be ended,
		/// either with success or with an error. It cannot be canceled.</param>
		internal virtual void EndEditOverride( IEditList<ActivityBase> editList, ActivityOperationResult result, bool force )
		{
			ListScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditOverrideImplementation( editList, result, force );
		}

		// SSP 1/8/11 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// This is called to commit changes to a <see cref="Resource"/> to the underlying data source.
		/// </summary>
		/// <param name="editList">This is the list manager.</param>
		/// <param name="result">Result's Resource is being updated. This result should be initialized with the result of the opreation.</param>
		/// <param name="force">True if the UI cannot remain in edit mode and therefore the operation must be ended,
		/// either with success or with an error. It cannot be canceled.</param>
		internal virtual void EndEditOverride( IEditList<Resource> editList, ResourceOperationResult result, bool force )
		{
			ListScheduleDataConnectorUtilities<Resource>.DefaultEndEditOverrideImplementation( editList, result, force );
		}

		#endregion // EndEditOverride 

		#region InitializeOwningCalendar

		// SSP 11/1/10 TFS58839
		// 
		internal void InitializeOwningCalendar( ActivityBase activity )
		{
			IEditList<ActivityBase> editList = this.GetEditListHelper( activity );

			Debug.Assert( null != editList );
			if ( null != editList )
				editList.EnsureViewItemInitialized( activity );
		}

		#endregion // InitializeOwningCalendar

		#endregion // Internal Methods

		#region Private Methods

		#region GetEditListHelper

		private IEditList<ActivityBase> GetEditListHelper( ActivityBase activity )
		{
			return this.GetEditListHelper( activity.ActivityType, activity );
		}

		private IEditList<ActivityBase> GetEditListHelper( ActivityType activityType, ActivityBase activityContext = null )
		{
			IEditList<ActivityBase> editList = null;

			switch ( activityType )
			{
				case ActivityType.Appointment:
					// If we are adding a recurring activity or a variance, use the recurring list manager if it has data list.
					// Otherwise the regular list manager will contain recurring and variance activities as well.
					// 
					if ( null != activityContext && ( activityContext.IsRecurrenceRoot || activityContext.IsOccurrence ) )
					{
						if ( _recurringAppointmentListManager.HasRecurringActivities )
							editList = _recurringAppointmentListManager;
					}

					if ( null == editList )
						editList = _appointmentListManager;
					break;
				case ActivityType.Journal:
					editList = _journalListManager;
					break;
				case ActivityType.Task:
					editList = _taskListManager;
					break;
				default:
					Debug.Assert( false, "Unknown activity type." );
					CoreUtilities.ValidateEnum( typeof( ActivityType ), activityType );
					break;
			}

			return editList;
		}

		#endregion // GetEditListHelper	

		#region Manage

		private TListManager Manage<TListManager>( TListManager lm )
			where TListManager : IListManager, IPropertyChangeListener
		{
			// Hook into the list managers so we get notified when their field mappings change
			// or when list is set so we can re-evaluate field mappings for error.
			// 
			ISupportPropertyChangeNotifications pcn = lm as ISupportPropertyChangeNotifications;
			pcn.AddListener( _listener, false );

			// Keep a list of list managers.
			// 
			_listManagers.Add( lm );

			return lm;
		}

		#endregion // Manage

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged( object sender, string propName, object extraInfo )
		{
			base.OnSubObjectPropertyChanged( sender, propName, extraInfo );

			bool revalidateSettings = false;

			switch ( propName )
			{
				case "FieldValueAccessors":
					revalidateSettings = true;
					break;
			}

			if ( revalidateSettings )
				this.ValidateSettingsAsync( );
		} 

		#endregion // OnSubObjectPropertyChanged

		#region ValidateActivityListManagerSettings

		private void ValidateActivityListManagerSettings<TViewItem, TMappingKey>( ActivityListManager<TViewItem, TMappingKey> lm,
			ActivityProperty[] extraRequiredFields, bool isRecurring, bool hasSeparateRecurringData, List<DataErrorInfo> errorList )
			where TViewItem: ActivityBase, new( )
		{
			if ( lm.HasList )
			{
				lm.ValidateMinimumFieldMappings( errorList );

				DataErrorInfo error;
				if ( null != extraRequiredFields && !lm.ValidateFieldMappingsHelper( extraRequiredFields, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error, ErrorSeverity.SevereError );

				if ( ( isRecurring || !hasSeparateRecurringData ) && !lm.ValidateFieldMappingsHelper( ActivityFeature.Recurrence, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error, isRecurring ? ErrorSeverity.SevereError : default( ErrorSeverity? ) );

				if ( ( isRecurring || !hasSeparateRecurringData ) && !lm.ValidateFieldMappingsHelper( ActivityFeature.Variance, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error );

				if ( !lm.ValidateFieldMappingsHelper( ActivityFeature.Reminder, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error );

				if ( !lm.ValidateFieldMappingsHelper( ActivityFeature.TimeZoneNeutrality, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error );

				if ( !lm.ValidateFieldMappingsHelper( ActivityFeature.EndTime, out error ) && null != error )
					ScheduleUtilities.AddErrorHelper( errorList, error );
			}
		}

		#endregion // ValidateActivityListManagerSettings

		#region ValidateSettings

		internal override void ValidateSettings( List<DataErrorInfo> errorList )
		{
			base.ValidateSettings( errorList );

			List<ActivityProperty> extraActivityFieldsRequiredList = new List<ActivityProperty>( );

			if ( _resourceListManager.HasList )
				_resourceListManager.ValidateMinimumFieldMappings( errorList );

			if ( _resourceCalendarListManager.HasList )
			{
				_resourceCalendarListManager.ValidateMinimumFieldMappings( errorList );
				extraActivityFieldsRequiredList.Add( ActivityProperty.OwningCalendarId );
			}

			ActivityProperty[] extraActivityFieldsRequired = extraActivityFieldsRequiredList.ToArray( );

			this.ValidateActivityListManagerSettings( _appointmentListManager, extraActivityFieldsRequired, false, _recurringAppointmentListManager.HasList, errorList );
			this.ValidateActivityListManagerSettings( _recurringAppointmentListManager, extraActivityFieldsRequired, true, false, errorList );
			this.ValidateActivityListManagerSettings( _journalListManager, extraActivityFieldsRequired, false, false, errorList );
			this.ValidateActivityListManagerSettings( _taskListManager, extraActivityFieldsRequired, false, false, errorList );
		} 

		#endregion // ValidateSettings

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ListScheduleDataConnector Class
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