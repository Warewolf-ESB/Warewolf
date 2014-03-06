using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Windows;
using Infragistics.Controls.Schedules.Primitives;
using System.ComponentModel;
using Infragistics.Collections;
using System.Diagnostics;
using System.Windows.Media;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// An abstract base class that is used to provide schedule data to the <see cref="XamScheduleDataManager"/>.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ScheduleDataConnectorBase</b> is an abstract base class. Derived instances are used to provide schedule data to the <see cref="XamScheduleDataManager"/>.
	/// </para>
	/// </remarks>
	/// <see cref="ListScheduleDataConnector"/>
	public abstract class ScheduleDataConnectorBase : FrameworkElement, ISupportPropertyChangeNotifications, IScheduleDataConnector
	{
		#region Static Variables

		private static Color[] DefaultCategoryColors = new Color[] { 
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

		#region Member Vars

		private PropertyChangeListenerList _listeners;
		private RecurrenceCalculatorFactoryBase _recurrenceCalculatorFactoryResolved;
		private TimeZoneInfoProvider _timeZoneProvider;
		private TimeZoneInfoProvider _timeZoneProviderResolved;
		private DeferredOperation _validateSettingsAsyncOperation;
		internal PropertyChangeListener<ScheduleDataConnectorBase> _listener;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ScheduleDataConnectorBase"/>.
		/// </summary>
		protected ScheduleDataConnectorBase( )
		{
			// initialize the cached _timeZoneProviderResolved member so we always return non null from TimeZoneInfoProviderResolved
			_timeZoneProviderResolved =  this.GetTzResolved();

			// Have OnSubObjectPropertyChanged called whenever there's a change in a property of the connector
			// or a subobject.
			// 
			_listener = new PropertyChangeListener<ScheduleDataConnectorBase>( this, OnSubObjectPropertyChangedHandler );
			_listeners = new PropertyChangeListenerList( );
			_listeners.Add( _listener, false );
			
			// initialize the TimeZoneInfoProviderResolved property
			this.TimeZoneInfoProviderResolved = this.GetTzResolved();
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region RecurrenceCalculatorFactory

		/// <summary>
		/// Identifies the read-only <see cref="RecurrenceCalculatorFactory"/> dependency property
		/// </summary>
		public static readonly DependencyProperty RecurrenceCalculatorFactoryProperty = DependencyPropertyUtilities.Register(
			"RecurrenceCalculatorFactory",
			typeof( RecurrenceCalculatorFactoryBase ),
			typeof( ScheduleDataConnectorBase ),
			DependencyPropertyUtilities.CreateMetadata( null, new PropertyChangedCallback( OnRecurrenceCalculatorFactoryChanged ) )
		);

		private static void OnRecurrenceCalculatorFactoryChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
		{
			ScheduleDataConnectorBase item = (ScheduleDataConnectorBase)d;

			ScheduleUtilities.NotifyListenersHelperWithResolved<RecurrenceCalculatorFactoryBase>( 
				item, e, item._listeners, true, true, ref item._recurrenceCalculatorFactoryResolved );
		}

		/// <summary>
		/// Gets the recurrence calculator factory used to provide recurrence calculation logic.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that this is used by the <see cref="XamScheduleDataManager"/> to evaluate recurrences
		/// specified on <see cref="DaySettingsOverride"/> objects. It's not used by the <i>XamScheduleDataManager</i>
		/// to resolve activity recurrences since activity recurrences are resolved by the schedule data connector
		/// itself.
		/// </para>
		/// <para class="body">
		/// A connector may have its own logic for resolving activity recurences and may not necessarily use
		/// this recurrence calculator factory for activity recurrence resolution. <b>Note</b> that 
		/// <see cref="ListScheduleDataConnector"/> does utilize this to resolve activity recurrences.
		/// </para>
		/// </remarks>
		/// <seealso cref="RecurrenceCalculatorFactoryProperty"/>
		/// <seealso cref="RecurrenceCalculatorFactoryResolved"/>
		public RecurrenceCalculatorFactoryBase RecurrenceCalculatorFactory
		{
			get
			{
				return (RecurrenceCalculatorFactoryBase)this.GetValue( RecurrenceCalculatorFactoryProperty );
			}
			set
			{
				this.SetValue( RecurrenceCalculatorFactoryProperty, value );
			}
		}

		#endregion // RecurrenceCalculatorFactory

		#region RecurrenceCalculatorFactoryResolved

		/// <summary>
		/// Gets the resolved recurrence calculator factory.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// If <see cref="RecurrenceCalculatorFactory"/> property has been set to an instance of <see cref="RecurrenceCalculatorFactoryBase"/> then 
		/// that instance will be returned. Otherwise a default RecurrenceCalculatorFactoryBase instance will be returned.
		/// </para>
		/// </remarks>
		public RecurrenceCalculatorFactoryBase RecurrenceCalculatorFactoryResolved
		{
			get
			{
				if ( null == _recurrenceCalculatorFactoryResolved )
					_recurrenceCalculatorFactoryResolved = this.RecurrenceCalculatorFactory ?? new DefaultRecurrenceCalculatorFactory( );

				return _recurrenceCalculatorFactoryResolved;
			}
		} 

		#endregion // RecurrenceCalculatorFactoryResolved

		#region ResourceItems

		/// <summary>
		/// Gets the resources collection.
		/// </summary>
		public abstract ResourceCollection ResourceItems
		{
			get;
		}

		#endregion // ResourceItems

		#region TimeZoneInfoProvider

		/// <summary>
		/// Identifies the <see cref="TimeZoneInfoProvider"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeZoneInfoProviderProperty = DependencyPropertyUtilities.Register("TimeZoneInfoProvider",
			typeof(TimeZoneInfoProvider), typeof(ScheduleDataConnectorBase),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnTimeZoneInfoProviderChanged))
			);

		private static void OnTimeZoneInfoProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleDataConnectorBase instance = (ScheduleDataConnectorBase)d;

			instance._timeZoneProvider = e.NewValue as TimeZoneInfoProvider;

			instance.TimeZoneInfoProviderResolved = instance.GetTzResolved();

			ScheduleUtilities.NotifyListenersHelper(instance, e, instance._listeners, true, true);

		}

		/// <summary>
		/// Returns or sets and instance of a class that supply timezone information.
		/// </summary>
		/// <seealso cref="TimeZoneInfoProviderProperty"/>
		public TimeZoneInfoProvider TimeZoneInfoProvider
		{
			get
			{
				return this._timeZoneProvider;
			}
			set
			{
				this.SetValue(ScheduleDataConnectorBase.TimeZoneInfoProviderProperty, value);
			}
		}

		#endregion //TimeZoneInfoProvider

		#region TimeZoneInfoProviderResolved

		private static readonly DependencyPropertyKey TimeZoneInfoProviderResolvedPropertyKey = DependencyPropertyUtilities.RegisterReadOnly("TimeZoneInfoProviderResolved",
			typeof(TimeZoneInfoProvider), typeof(ScheduleDataConnectorBase),
			null,
			new PropertyChangedCallback(OnTimeZoneInfoProviderResolvedChanged)
			);

		/// <summary>
		/// Identifies the read-only <see cref="TimeZoneInfoProviderResolved"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TimeZoneInfoProviderResolvedProperty = TimeZoneInfoProviderResolvedPropertyKey.DependencyProperty;

		private static void OnTimeZoneInfoProviderResolvedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			ScheduleDataConnectorBase instance = (ScheduleDataConnectorBase)d;

			instance._timeZoneProviderResolved = e.NewValue as TimeZoneInfoProvider;

			ScheduleUtilities.NotifyListenersHelper(instance, e, instance._listeners, true, true);
		}

		/// <summary>
		/// Returns an instance of a <see cref="TimeZoneInfoProvider"/> derived class (read-only)
		/// </summary>
		/// <value>The value of the <see cref="ScheduleDataConnectorBase.TimeZoneInfoProvider"/> property if set. Otherwise a default provider.</value>
		/// <seealso cref="TimeZoneInfoProviderResolvedProperty"/>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		[Browsable(false)]
		[ReadOnly(true)]
		public TimeZoneInfoProvider TimeZoneInfoProviderResolved
		{
			get
			{
				return this._timeZoneProviderResolved;
			}
			private set
			{
				this.SetValue(ScheduleDataConnectorBase.TimeZoneInfoProviderResolvedPropertyKey, value);
			}
		}

		#endregion //TimeZoneInfoProviderResolved
    
		#endregion // Public Properties

		#region Internal Properties

		#region PublicDisplayName

		internal virtual string PublicDisplayName
		{
			get { return this.GetType().Name; }
		}

		#endregion  // PublicDisplayName 

		#endregion  // Internal Properties

		#region Protected Properties

		#region RecurringTaskGenerationBehavior

		/// <summary>
		/// Gets the value indicating how this connector generates recurring tasks.
		/// </summary>

		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

		protected internal virtual RecurringTaskGenerationBehavior RecurringTaskGenerationBehavior
		{
			get { return RecurringTaskGenerationBehavior.GenerateAllTasks; }
		}

		#endregion  // RecurringTaskGenerationBehavior 

		#endregion  // Protected Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region GetDefaultCategoryColors

		/// <summary>
		/// Returns the collection of colors which should be displayed in the drop down when modifying a category.
		/// </summary>
		/// <param name="areCustomCategoryColorsAllowed">Indicates whether the user can use any color when creating a custom category.</param>
		/// <returns>The collection of default colors or null if the normal default colors should be used.</returns>

		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

		protected virtual IList<Color> GetDefaultCategoryColors(out bool areCustomCategoryColorsAllowed)
		{
			areCustomCategoryColorsAllowed = true;
			return null;
		}

		#endregion //GetDefaultCategoryColors 

		#endregion //Protected Methods

		#region Internal Protected Methods

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
		internal protected abstract bool AreActivitiesSupported( ActivityType activityType ); 

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
		/// <b>Note:</b> BeginEdit cannot be called more than once without an intervening call to CancelEdit or EndEdit. Successive BeginEdit
		/// calls should result an error and return false.
		/// </para>
		/// </remarks>
		internal protected virtual bool BeginEdit(ActivityBase activity, out DataErrorInfo errorInfo)
		{
			return ScheduleDataConnectorUtilities<ActivityBase>.DefaultBeginEditImplementation( activity, out errorInfo );
		}

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

		internal protected virtual bool BeginEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			return ScheduleDataConnectorUtilities<Resource>.DefaultBeginEditImplementation( resource, out errorInfo );
		}

		#endregion // BeginEdit

		#region CancelEdit

		/// <summary>
        /// Cancels modifications to an existing activity or cancels a new activity that was created by the 
		/// <see cref="CreateNew"/> call however one that hasn't been commited yet.
        /// </summary>
		/// <param name="activity">ActivityBase derived object that was put in edit state by <see cref="BeginEdit(ActivityBase, out DataErrorInfo)"/> call
		/// or one that was created using <see cref="CreateNew"/> method however one that hasn't been committed yet.</param>
        /// <param name="errorInfo">If there's an error this will be set to a new DataErrorInfo object with the error information.</param>
        /// <returns>True to indicate that the operation was successfull, False otherwise.</returns>
        /// <remarks>
        /// <para class="body">
        /// <b>Note:</b> <b>CancelEdit</b> method is called by the <see cref="XamScheduleDataManager"/>
        /// to cancel modifications to an existing activity. It also calls this method to cancel and discard a 
		/// new activity that was created using the <see cref="CreateNew"/> method, however the activity must 
		/// not have been commited yet. This is typically done when the user cancels the dialog for creating 
		/// a new activity, like the new appointment dialog.
        /// </para>
        /// </remarks>
        /// <seealso cref="EndEdit(ActivityBase, bool)"/>
		/// <seealso cref="CancelEdit(ActivityBase, out DataErrorInfo)"/>
        /// <seealso cref="Remove"/>
		internal protected virtual bool CancelEdit( ActivityBase activity, out DataErrorInfo errorInfo )
		{
			return ScheduleDataConnectorUtilities<ActivityBase>.DefaultCancelEditImplementation( activity, out errorInfo );
		}

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

		internal protected virtual bool CancelEdit( Resource resource, out DataErrorInfo errorInfo )
		{
			return ScheduleDataConnectorUtilities<Resource>.DefaultCancelEditImplementation( resource, out errorInfo );
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
		internal protected abstract CancelOperationResult CancelPendingOperation( OperationResult operation );

		#endregion // CancelPendingOperation

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
		/// doesn't get commited to the data source until <see cref="EndEdit(ActivityBase, bool)"/> method is called. Also if you wish to
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
		internal protected abstract ActivityBase CreateNew( ActivityType activityType, out DataErrorInfo errorInfo );

		#endregion // CreateNew

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
		internal protected virtual ActivityOperationResult EndEdit( ActivityBase activity, bool force )
		{
			return ScheduleDataConnectorUtilities<ActivityBase>.DefaultEndEditImplementation( activity ) as ActivityOperationResult;
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
		/// <seealso cref="CancelEdit(Resource, out DataErrorInfo)"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected virtual ResourceOperationResult EndEdit( Resource resource, bool force )
		{
			DataErrorInfo error;
			if (this.ValidateResource(resource, out error) == false)
			{
				DataErrorInfo tmp;
				this.CancelEdit(resource, out tmp);

				return new ResourceOperationResult(resource, error, true);
			}

			return ScheduleDataConnectorUtilities<Resource>.DefaultEndEditImplementation(resource) as ResourceOperationResult;
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
        /// </remarks>
		internal protected abstract ActivityQueryResult GetActivities( ActivityQuery query );

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

		internal protected virtual IEnumerable<ActivityCategory> GetSupportedActivityCategories( ActivityBase activity )
		{
			return null;
		} 

		#endregion // GetSupportedActivityCategories

		// MD 1/5/11 - NA 11.1 - Exchange Data Connector
		#region InitializeEditCopy

		/// <summary>
		/// Allows the data connector to perform any additional initialization on the edit copy before the edit operation begins.
		/// </summary>
		/// <param name="editCopy">The copy of the original activity being editted.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Note:</b> <b>InitializeEditCopy</b> is called by the <see cref="XamScheduleDataManager.BeginEditWithCopy"/> 
		/// method after creating the edit copy, but before returning it to the caller.
		/// </para>
		/// </remarks>
		internal protected virtual void InitializeEditCopy(ActivityBase editCopy) { } 

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

		[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)] 

		internal protected virtual bool IsActivityEditingAllowed(ActivityBase activity, EditableActivityProperty property)
		{
			return true;
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
		/// This method is used by the data manager to determine if the specified feature is supprted for activities of
		/// the specified type that belong the specified calendar. If the feature is not supported, relevant user interface 
		/// will be disabled or hidden in the schedule controls.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		internal protected abstract bool IsActivityFeatureSupported( ActivityType activityType, ActivityFeature activityFeature, ResourceCalendar calendar );

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
		internal protected virtual bool IsActivityOperationSupported( ActivityBase activity, ActivityOperation activityOperation )
		{
			return this.IsActivityOperationSupported( activity.ActivityType, activityOperation, activity.OwningCalendar );
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
		/// <see cref="IsActivityOperationSupported(ActivityBase, ActivityOperation)"/> that takes in the activity is used.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsActivityOperationSupported(ActivityBase, ActivityOperation)"/>
		/// <seealso cref="IsActivityFeatureSupported(ActivityType, ActivityFeature, ResourceCalendar)"/>
		/// <seealso cref="AreActivitiesSupported(ActivityType)"/>
		internal protected abstract bool IsActivityOperationSupported( ActivityType activityType, ActivityOperation activityOperation, ResourceCalendar calendar );

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

		internal protected virtual bool IsResourceFeatureSupported( Resource resource, ResourceFeature resourceFeature )
		{
			return true;
		}

		#endregion // IsResourceFeatureSupported

		#region RaiseError

		/// <summary>
		/// This method is called when there's an error.
		/// </summary>
		/// <param name="error">Error information.</param>
		/// <remarks>
		/// <para class="body">
		/// Derived connectors can call this method when there's an error to notify the data manager and controls
		/// of the error. Doing so will cause the data manager to raise its Error event. Also depending upon the
		/// severity of the error, the data manager and the controls will take appropriate action to alert the 
		/// user of the error.
		/// </para>
		/// <para class="body">
		/// <b>Note</b> that typically there's no need to call this method since most of the operations performed
		/// on the data connector return an error as an out parameter (like <see cref="BeginEdit(ActivityBase, out DataErrorInfo)"/> method), or
		/// return an operation result object that contains the error information. The caller, typically the
		/// <see cref="XamScheduleDataManager"/>, gets those errors and takes appropriate action. This method
		/// should be called when an error occurs outside of these operations or method calls where the 
		/// <see cref="XamScheduleDataManager"/> or the view controls do not have any way of knowing. An example
		/// would be that a connection to the server is lost and you want to notify the user of that via the
		/// view control's error user interface.
		/// </para>
		/// </remarks>
		/// <seealso cref="XamScheduleDataManager.Error"/>
		internal protected void RaiseError( DataErrorInfo error )
		{
			this.NotifyListeners( this, "Error", error );
		} 

		#endregion // RaiseError

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
		internal protected abstract ActivityOperationResult Remove( ActivityBase activity );

        #endregion // Remove

		#region ResolveActivityCategories

		// SSP 12/9/10 - NAS11.1 Activity Categories
		// 
		/// <summary>
		/// Gets the activity categories that are currently applied to the specified activity. 
		/// Default implementation returns <i>null</i>.
		/// </summary>
		/// <param name="activity">Activity for which to get the list of currently applied activity categories.</param>
		/// <returns>IEnumerable that can optionally implement INotifyCollectionChanged to notify
		/// of changes to the list.</returns>
		/// <seealso cref="GetSupportedActivityCategories"/>

		[InfragisticsFeature(FeatureName = "ActivityCategories", Version = "11.1")]

		internal protected virtual IEnumerable<ActivityCategory> ResolveActivityCategories( ActivityBase activity )
		{
			return null;
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
		internal protected abstract void SubscribeToReminders( ResourceCalendar calendar, ReminderSubscriber subscriber, out DataErrorInfo error );

		#endregion // SubscribeToReminders

		#region UnsubscribeFromReminders

		/// <summary>
		/// Unsubscribes from activity reminders of the specified calendar. If the specified subscriber hadn't been subscribed
		/// previously then this method will take no action.
		/// </summary>
		/// <param name="calendar">The calendar's activity reminders to unsubscribe from.</param>
		/// <param name="subscriber">Subscriber instance.</param>
		internal protected abstract void UnsubscribeFromReminders( ResourceCalendar calendar, ReminderSubscriber subscriber );

		#endregion // UnsubscribeFromReminders

		#region VerifyInitialState

		/// <summary>
		/// Called to verify that the data connector has sufficient state to operate.
		/// </summary>
		/// <param name="errorList">A list to receive the errors.</param>
		/// <remarks>
		/// <para class="note"><b>Note</b>: this method gets called once by the <see cref="XamScheduleDataManager"/> when it is verifying its inital state.
		/// </para>
		/// </remarks>
		internal protected virtual void VerifyInitialState(List<DataErrorInfo> errorList)
		{
			this.ValidateSettings(errorList);
		}

		#endregion //VerifyInitialState	
    
		#endregion // Internal Protected Methods

		#region Internal Methods

		#region GetDefaultCategoryColorsResolved

		internal IList<Color> GetDefaultCategoryColorsResolved(out bool areCustomCategoryColorsAllowed)
		{
			IList<Color> defaultCategoryColors = this.GetDefaultCategoryColors(out areCustomCategoryColorsAllowed);

			if (defaultCategoryColors != null)
				return defaultCategoryColors;

			return ScheduleDataConnectorBase.DefaultCategoryColors;
		}

		#endregion //GetDefaultCategoryColorsResolved

		#region NotifyListeners

		/// <summary>
		/// Notifies the ISupportPropertyChangeNotifications listeners.
		/// </summary>
		/// <param name="sender">Object whose property changed.</param>
		/// <param name="property">Property that changed.</param>
		/// <param name="extraInfo">Any extra info associated with the property change.</param>
		internal void NotifyListeners( object sender, string property, object extraInfo )
		{
			_listeners.OnPropertyValueChanged( sender, property, extraInfo );
		} 

		#endregion // NotifyListeners

		#region OnSubObjectPropertyChanged

		private static void OnSubObjectPropertyChangedHandler( ScheduleDataConnectorBase connector, object sender, string propName, object extraInfo )
		{
			connector.OnSubObjectPropertyChanged( sender, propName, extraInfo );
		}

		internal virtual void OnSubObjectPropertyChanged( object sender, string propName, object extraInfo )
		{
			bool revalidateSettings = false;

			if ( sender == this )
			{
				switch ( propName )
				{
					case "TimeZoneInfoProviderResolved":
						revalidateSettings = true;
						break;
				}
			}
			else if ( sender is TimeZoneInfoProvider )
			{
				switch ( propName )
				{
					case "Version":
						revalidateSettings = true;
						break;
				}
			}
			else
			{
				switch ( propName )
				{
					case "FieldValueAccessors":
						revalidateSettings = true;
						break;
					case "TimeZoneInfoProviderResolved":
						revalidateSettings = true;
						break;
				}
			}

			if ( revalidateSettings )
				this.ValidateSettingsAsync( );
		}

		#endregion // OnSubObjectPropertyChanged

		#region ValidateResource

		internal bool ValidateResource(Resource resource, out DataErrorInfo error)
		{
			error = null;

			ActivityCategoryCollection customActivityCategories = resource.CustomActivityCategories;
			if (customActivityCategories != null && customActivityCategories.Count > 0)
			{
				bool areCustomCategoryColorsAllowed;
				IList<Color> defaultCategoryColors = this.GetDefaultCategoryColorsResolved(out areCustomCategoryColorsAllowed);

				if (areCustomCategoryColorsAllowed == false)
				{
					for (int i = 0; i < customActivityCategories.Count; i++)
					{
						ActivityCategory category = customActivityCategories[i];
						if (category.Color.HasValue &&
							defaultCategoryColors.Contains(category.Color.Value) == false)
						{
							error = DataErrorInfo.CreateError(resource, ScheduleUtilities.GetString("LE_InvalidCategoryColor", category.CategoryName));
							return false;
						}
					}
				}
			}

			return true;
		} 

		#endregion //ValidateResource

		#region ValidateSettings

		private void ValidateSettings(  )
		{
			List<DataErrorInfo> errorList = new List<DataErrorInfo>( );

			this.ValidateSettings( errorList );

			DataErrorInfo error = null;
			if ( 1 == errorList.Count )
				error = errorList[0];
			else if ( errorList.Count > 0 )
				error = new DataErrorInfo( errorList );

			if ( null != error )
				this.RaiseError( error );
			else
				
				// This is to notify the data manager to clear the blocking error.
				// 
				this.NotifyListeners( this, "ClearBlockingError", null );
		}

		internal virtual void ValidateSettings( List<DataErrorInfo> errorList )
		{
			if ( null != _validateSettingsAsyncOperation )
			{
				_validateSettingsAsyncOperation.CancelPendingOperation( );
				_validateSettingsAsyncOperation = null;
			}

			TimeZoneInfoProvider tzInfoProvider = this.TimeZoneInfoProviderResolved;

			string publicDisplayName = this.PublicDisplayName;

			if ( null == tzInfoProvider )
			{
				errorList.Add(ScheduleUtilities.CreateBlockingFromId(this, "LE_TZProviderMissing", publicDisplayName, "TimeZoneInfoProvider"));
			}
			else
			{
				if (tzInfoProvider.TimeZoneTokens.Count == 0)
					errorList.Add(ScheduleUtilities.CreateBlockingFromId(tzInfoProvider, "LE_NoTimeZones", publicDisplayName, "TimeZoneInfoProvider"));
				else
				{
					if (null == tzInfoProvider.LocalToken)
					{
						// JJD 4/4/11 - TFS69535 
						// Added overload with isLocalTZTokenError parameter
						//errorList.Add(ScheduleUtilities.CreateBlockingFromId(tzInfoProvider, "LE_NoLOcalTimeZones", publicDisplayName, "TimeZoneInfoProvider", "LocalTimeZoneId"));
						errorList.Add(ScheduleUtilities.CreateBlockingFromId(tzInfoProvider, "LE_NoLOcalTimeZones", true, publicDisplayName, "TimeZoneInfoProvider", "LocalTimeZoneId"));
					}

					if (null == tzInfoProvider.UtcToken)
					{

						if ( tzInfoProvider is OSTimeZoneInfoProvider )
							errorList.Add(ScheduleUtilities.CreateBlockingFromId(tzInfoProvider, "LE_NoUTCTimeZone_OS", TimeZoneInfo.Utc.Id));
						else

							errorList.Add(ScheduleUtilities.CreateBlockingFromId(tzInfoProvider, "LE_NoUTCTimeZone", publicDisplayName, "TimeZoneInfoProvider", "UtcTimeZoneId"));
					}
				}
			}
		}

		#endregion // ValidateSettings

		#region ValidateSettingsAsync

		internal void ValidateSettingsAsync( )
		{
			if ( null == _validateSettingsAsyncOperation )
			{
				_validateSettingsAsyncOperation = new DeferredOperation( this.ValidateSettings );
				_validateSettingsAsyncOperation.StartAsyncOperation( );
			}
		}

		#endregion // ValidateSettingsAsync

		#endregion // Internal Methods

		#region Private Methods

		#region GetTzResolved

		private TimeZoneInfoProvider GetTzResolved()
		{
			if (_timeZoneProvider != null)
				return _timeZoneProvider;

			return TimeZoneInfoProvider.DefaultProvider;
		}

		#endregion //GetTzResolved

		#endregion //Private Methods	
        
		#endregion // Methods

		#region IScheduleDataConnector Interface Implementation

		void IScheduleDataConnector.NotifyListeners(object sender, string property, object extraInfo)
		{
			_listeners.OnPropertyValueChanged(sender, property, extraInfo);
		}

		void IScheduleDataConnector.OnError( DataErrorInfo error )
		{
			this.RaiseError( error );
		}

		bool IScheduleDataConnector.IsActivityOperationSupported( ActivityBase activity, ActivityOperation operation )
		{
			return this.IsActivityOperationSupported( activity, operation );
		}

		#endregion // IScheduleDataConnector Interface Implementation

		#region ISupportPropertyChangeNotifications Interface Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			_listeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			_listeners.Remove( listener );
		}

		#endregion // ISupportPropertyChangeNotifications Interface Implementation
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