using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Diagnostics;



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;
using Infragistics.Controls.Schedules;

namespace Infragistics

{
	#region OperationResult Class

	/// <summary>
	/// Used as a return value by methods that perform operations asynchronously.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// An operation that needs to be performed asynchronously may not have the results available until a later
	/// time. A method performing such an operation can return an instance of OperationResult or a derived class.
	/// When the operation is complete, the result instance can be initialized with the available result.
	/// </para>
	/// </remarks>
	public class OperationResult : PropertyChangeNotifierExtended
	{
		#region Member Vars

		private bool _isComplete;
		private DataErrorInfo _errorInfo;
		private bool _isCanceled;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="OperationResult"/> object.
		/// </summary>
		protected OperationResult( )
		{
		}

		#endregion // Constructor

		#region Public Properties

		#region Error

		/// <summary>
		/// If an error occurred during the process of getting activity query results, this property returns the error information.
		/// </summary>
		public DataErrorInfo Error
		{
			get
			{
				return _errorInfo;
			}
		}

		#endregion // Error

		#region IsCanceled

		/// <summary>
		/// Indicates if the operation was canceled.
		/// </summary>
		public bool IsCanceled
		{
			get
			{
				return _isCanceled;
			}
		}

		#endregion // IsCanceled

		#region IsComplete

		/// <summary>
		/// Indicates if the query operation has been completed.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// When a query is performed asynchronously <b>IsComplete</b> will return false until the operation 
		/// has been completed. Also <i>Activities</i> property will return an empty collection until the query
		/// is complete after which it will raise its <i>INotifyCollectionChanged.CollectionChanged</i> event
		/// in accordance with the query results.
		/// </para>
		/// </remarks>
		public bool IsComplete
		{
			get
			{
				return _isComplete;
			}
		}

		#endregion // IsComplete

		#endregion // Public Properties
		
		#region Methods

		#region Protected Methods

		#region InitializeResult

		/// <summary>
		/// Initializes the <see cref="Error"/> and <see cref="IsComplete"/> properties based on the specified values.
		/// </summary>
		/// <param name="error"><see cref="DataErrorInfo"/> object if an error occurred, null otherwise.</param>
		/// <param name="isComplete">Indicates whether the results are complete. Specify false if further results are
		/// going to be available.</param>
		/// <remarks>
		/// <para class="body">
		/// An operation may be performed asynchronously. When a method that performs an operation is required 
		/// to return an instance of <i>OperationResult</i> derived class, you can return a new instance 
		/// and specify the result of the operation later via this method when it's available. While the asynchronous 
		/// operation is still pending, the <see cref="IsComplete"/> property will return <i>False</i>. When the 
		/// result is available, you can call this method and specify 'isComplete' parameter as true to indicate 
		/// that the operation is complete and that the <see cref="IsComplete"/> property should be set to true.
		/// </para>
		/// <para class="body">
		/// If an error occurs during the operation, you can call this method with the <see cref="DataErrorInfo"/> 
		/// object containing the error information. Also specifiy true for 'isComplete' since no further action
		/// will be taken and operation is considered to be complete with an error.
		/// </para>
		/// </remarks>
		/// <seealso cref="IsComplete"/>
		/// <seealso cref="Error"/>
		protected void InitializeResult( DataErrorInfo error, bool isComplete )
		{
			if ( _isCanceled )
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CanNotInitCancelledOp")); // "Can't initialize result of a canceled operation."

			bool raiseIsComplete = false;
			bool raiseError = false;

			if ( _errorInfo != error )
			{
				_errorInfo = error;
				raiseError = true;
			}

			if ( _isComplete != isComplete )
			{
				_isComplete = isComplete;
				raiseIsComplete = true;
			}

			if ( raiseError )
				this.RaisePropertyChangedEvent( "Error" );

			if ( raiseIsComplete )
				this.RaisePropertyChangedEvent( "IsComplete" );
		}

		#endregion // InitializeResult

		#endregion // Protected Methods

		#region Public Methods

		#region OnCanceled

		/// <summary>
		/// Called when the operation is canceled.
		/// </summary>
		public void OnCanceled( )
		{
			if ( _isComplete )
				throw new InvalidOperationException(ScheduleUtilities.GetString("LE_CanNotCancelCompletedOp"));//"A completed operation can not be canceled."

			bool raiseIsComplete = false;
			bool raiseIsCanceled = false;

			if ( !_isComplete )
			{
				_isComplete = true;
				raiseIsComplete = true;
			}

			if ( !_isCanceled )
			{
				_isCanceled = true;
				raiseIsCanceled = true;
			}

			if ( raiseIsCanceled )
				this.RaisePropertyChangedEvent( "IsCanceled" );

			if ( raiseIsComplete )
				this.RaisePropertyChangedEvent( "IsComplete" );
		}

		#endregion // OnCanceled 

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // OperationResult Class

	#region CancelOperationResult

	/// <summary>
	/// Represents the result of a cancel operation of a pending operation.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>CancelOperationResult</b> is used by the <see cref="XamScheduleDataManager.CancelPendingOperation"/>
	/// method to return the status of the cancelation operation.
	/// </para>
	/// </remarks>
	public class CancelOperationResult : OperationResult
	{
		#region Member Vars

		private OperationResult _operation;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="operation">Pending operation which is being canceled.</param>
		public CancelOperationResult( OperationResult operation )
		{
			CoreUtilities.ValidateNotNull( operation );
			_operation = operation;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="operation">Pending operation which is being canceled.</param>
		/// <param name="error">Error info if any.</param>
		/// <param name="isComplete">Specifies whether the operation is complete.</param>
		public CancelOperationResult( OperationResult operation, DataErrorInfo error, bool isComplete )
		{
			_operation = operation;
			this.InitializeResult( error, isComplete );
		}

		#endregion // Constructor

		#region Operation

		/// <summary>
		/// Operation being canceled.
		/// </summary>
		public OperationResult Operation
		{
			get
			{
				return _operation;
			}
		}

		#endregion // Operation

		#region Methods

		#region Public Methods

		#region InitializeResult

		/// <summary>
		/// Initializes the <see cref="OperationResult.Error"/> and 
		/// <see cref="OperationResult.IsComplete"/> properties based on the specified values.
		/// </summary>
		/// <param name="error"><see cref="DataErrorInfo"/> object if an error occurred, null otherwise.</param>
		/// <param name="isComplete">Indicates whether the results are complete. Specify false if further results are
		/// going to be available.</param>
		/// <remarks>
		/// <para class="body">
		/// Activity operations may be executed asynchronously. When a method that performs an operation is required 
		/// to return an instance of <i>OperationResult</i> derived class, you can return a new instance 
		/// and specify the result of the operation later via this method when it's available. While the asynchronous 
		/// operation is still pending, the <see cref="OperationResult.IsComplete"/> property will return <i>False</i>. When the 
		/// result is available, you can call this method and specify 'isComplete' parameter as true to indicate 
		/// that the operation is complete and that the <see cref="OperationResult.IsComplete"/> property should be set to true.
		/// </para>
		/// <para class="body">
		/// If an error occurs during the operation, you can call this method with the <see cref="DataErrorInfo"/> 
		/// object containing the error information. Also specifiy true for 'isComplete' since no further action
		/// will be taken and operation is considered to be complete with an error.
		/// </para>
		/// </remarks>
		/// <seealso cref="OperationResult.IsComplete"/>
		/// <seealso cref="OperationResult.Error"/>
		public new void InitializeResult( DataErrorInfo error, bool isComplete )
		{
			base.InitializeResult( error, isComplete );
		}

		#endregion // InitializeResult

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // CancelOperationResult
}





namespace Infragistics.Controls.Schedules

{
	#region ItemOperationResult Class

	// SSP 1/8/11 - NAS11.1 Activity Categories
	// 
	/// <summary>
	/// Result of an operation performed on an item. The operation may be performed asynchronously.
	/// </summary>	
	public class ItemOperationResult<T> : OperationResult
	{
		#region Member Vars

		/// <summary>
		/// The item associated with the operation.
		/// </summary>
		private T _item;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ItemOperationResult&lt;T&gt;"/>.
		/// </summary>
		/// <param name="item">Item associated with the operation.</param>
		public ItemOperationResult( T item )
		{
			_item = item;
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ItemOperationResult&lt;T&gt;"/>.
		/// </summary>
		/// <param name="item">Item associated with the operation.</param>
		/// <param name="errorInfo">Error information if there's an error.</param>
		/// <param name="markComplete">Whether to mark the operation as complete. Pass false for asynchronous
		/// operation whose result is going to be available later.</param>
		public ItemOperationResult( T item, DataErrorInfo errorInfo, bool markComplete )
		{
			_item = item;
			this.InitializeResult( errorInfo, markComplete );
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityOperationResult"/>.
		/// </summary>
		public ItemOperationResult( T item, bool isCanceled )
		{
			_item = item;

			if ( isCanceled )
				this.OnCanceled( );
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Item

		/// <summary>
		/// Returns the item associated with the operation.
		/// </summary>
		public T Item
		{
			get
			{
				return _item;
			}
		}

		#endregion // Item

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region InitializeResult

		/// <summary>
		/// Initializes the <see cref="OperationResult.Error"/> and <see cref="OperationResult.IsComplete"/> properties based on the specified values.
		/// </summary>
		/// <param name="error"><see cref="DataErrorInfo"/> object if an error occurred, null otherwise.</param>
		/// <param name="isComplete">Indicates whether the results are complete. Specify false if further results are
		/// going to be available.</param>
		/// <remarks>
		/// <para class="body">
		/// An operation may be performed asynchronously. When a method that performs an operation is required 
		/// to return an instance of <i>OperationResult</i> derived class, you can return a new instance 
		/// and specify the result of the operation later via this method when it's available. While the asynchronous 
		/// operation is still pending, the <see cref="OperationResult.IsComplete"/> property will return <i>False</i>. When the 
		/// result is available, you can call this method and specify 'isComplete' parameter as true to indicate 
		/// that the operation is complete and that the <see cref="OperationResult.IsComplete"/> property should be set to true.
		/// </para>
		/// <para class="body">
		/// If an error occurs during the operation, you can call this method with the <see cref="DataErrorInfo"/> 
		/// object containing the error information. Also specifiy true for 'isComplete' since no further action
		/// will be taken and operation is considered to be complete with an error.
		/// </para>
		/// </remarks>
		/// <seealso cref="OperationResult.IsComplete"/>
		/// <seealso cref="OperationResult.Error"/>
		public new void InitializeResult( DataErrorInfo error, bool isComplete )
		{
			base.InitializeResult( error, isComplete );
		}

		#endregion // InitializeResult

		#endregion // Methods
	}

	#endregion // ItemOperationResult Class

	#region ActivityOperationResult Class

	/// <summary>
	/// Result of an activity operation that may be performed asynchronously.
	/// </summary>
	public class ActivityOperationResult : ItemOperationResult<ActivityBase>
	{
		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityOperationResult"/>.
		/// </summary>
		/// <param name="activity">Activity associated with the operation.</param>
		public ActivityOperationResult( ActivityBase activity )
			: base( activity )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityOperationResult"/>.
		/// </summary>
		/// <param name="activity">Activity associated with the operation.</param>
		/// <param name="errorInfo">Error information if there's an error.</param>
		/// <param name="markComplete">Whether to mark the operation as complete. Pass false for asynchronous
		/// operation whose result is going to be available later.</param>
		public ActivityOperationResult( ActivityBase activity, DataErrorInfo errorInfo, bool markComplete )
			: base( activity, errorInfo, markComplete )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityOperationResult"/>.
		/// </summary>
		public ActivityOperationResult( ActivityBase activity, bool isCanceled )
			: base( activity, isCanceled )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Activity

		/// <summary>
		/// Returns the activity associated with the operation.
		/// </summary>
		/// <remarks>
		/// This property returns the same value as the <see cref="ItemOperationResult&lt;T&gt;.Item"/> property.
		/// </remarks>
		public ActivityBase Activity
		{
			get
			{
				return this.Item;
			}
		}

		#endregion // Activity

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // ActivityOperationResult Class

	#region ResourceOperationResult Class

	// SSP 1/8/11 - NAS11.1 Activity Categories
	// 
	/// <summary>
	/// Result of a resource operation that may be performed asynchronously.
	/// </summary>
	public class ResourceOperationResult : ItemOperationResult<Resource>
	{
		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ResourceOperationResult"/>.
		/// </summary>
		/// <param name="resource">Resource associated with the operation.</param>
		public ResourceOperationResult( Resource resource )
			: base( resource )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ResourceOperationResult"/>.
		/// </summary>
		/// <param name="resource">Resource associated with the operation.</param>
		/// <param name="errorInfo">Error information if there's an error.</param>
		/// <param name="markComplete">Whether to mark the operation as complete. Pass false for asynchronous
		/// operation whose result is going to be available later.</param>
		public ResourceOperationResult( Resource resource, DataErrorInfo errorInfo, bool markComplete )
			: base( resource, errorInfo, markComplete )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ResourceOperationResult"/>.
		/// </summary>
		public ResourceOperationResult( Resource resource, bool isCanceled )
			: base( resource, isCanceled )
		{
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Resource

		/// <summary>
		/// Returns the resource.
		/// </summary>
		/// <remarks>
		/// This property returns the same value as the <see cref="ItemOperationResult&lt;T&gt;.Item"/> property.
		/// </remarks>
		public Resource Resource
		{
			get
			{
				return this.Item;
			}
		}

		#endregion // Resource

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // ResourceOperationResult Class

	#region ActivityNavigationInfo Struct

	internal struct ActivityNavigationInfo
	{
		internal bool? _hasNextActivity;
		internal bool? _hasPrevActivity;
		internal ActivityBase _nextActivity;
		internal ActivityBase _prevActivity;

		internal void SetPrevNextHelper( ActivityBase activity, bool prev )
		{
			if ( null != activity )
			{
				ActivityBase c = prev ? _prevActivity : _nextActivity;
				if ( null == c || ( prev ? activity.End > c.End : activity.Start < c.Start ) )
				{
					if ( prev )
						_prevActivity = activity;
					else
						_nextActivity = activity;

					this.SetHasPrevNextHelper( null != activity, prev );
				}
			}
			else
			{
				this.SetHasPrevNextHelper( false, prev );
			}
		}

		internal void SetHasPrevNextHelper( bool value, bool prev )
		{
			bool? c = prev ? _hasPrevActivity : _hasNextActivity;

			if ( value || !c.HasValue )
			{
				if ( prev )
					_hasPrevActivity = value;
				else
					_hasNextActivity = value;
			}
		}
	}

	#endregion // ActivityNavigationInfo Struct

	#region ActivityQueryResult Class

	/// <summary>
	/// Represents results of an activity query.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// <b>ActivityQueryResult</b> is returned by the <see cref="XamScheduleDataManager.GetActivities"/> method.
	/// </para>
	/// </remarks>
	/// <seealso cref="XamScheduleDataManager.GetActivities(ActivityQuery)"/>
	public class ActivityQueryResult : OperationResult
	{
		#region Member Vars

		private ActivityQuery _query;

		private ReadOnlyNotifyCollection<ActivityBase> _activities;
		private ActivityNavigationInfo _navigationInfo;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ActivityQueryResult"/>.
		/// </summary>
		public ActivityQueryResult( ActivityQuery query )
		{
			ScheduleUtilities.ValidateNotNull( query );

			_query = query;

			// Seal the query so no further modifications can be done to it.
			// 
			_query.Seal( );
		}

		#endregion // Constructor

		#region Properties

		#region Public Properties

		#region Activities

		/// <summary>
		/// Returns the collection of activities.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Returns a read-only collection of activities that are the result of an activity query. 
		/// The returned collection will be kept upto-date with any changes that occur to the activities. For example,
		/// when an activity belonging to this collection is removed by the user via the UI or through the data 
		/// source, the returned collection will be updated and it will raise the necessary change notification.
		/// </para>
		/// </remarks>
		public ReadOnlyNotifyCollection<ActivityBase> Activities
		{
			get
			{
				if ( null == _activities )
				{
					// SSP 2/10/10
					// If the activities collection that's passed into the InitializeResult method is not
					// ISupportPropertyChangeNotifications that propagates activity property change
					// notifications, when we need to have the collection hook into the activities and
					// propagate property change notifications. UI relies on the result propagating
					// these changes. The new ItemNotificationCollection derives from ReadOnlyNotifyCollection
					// and takes care of this.
					// 
					//_activities = new ReadOnlyNotifyCollection<ActivityBase>( new ActivityBase[0] );
					_activities = new ItemNotificationCollection<ActivityBase>( new ActivityBase[0] );

					( (ISupportPropertyChangeNotifications)_activities ).AddListener( this, false );
				}

				return _activities;
			}
		}

		#endregion // Activities

		#region HasNextActivity

		/// <summary>
		/// Indicates whether there's an activity after the latest date in the <see cref="ActivityQuery.DateRanges"/> of the query.
		/// Property's value is valid only if this information was requested by the associated activity query and the 
		/// information is available.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasNextActivity</b> property returns <i>true</i> to indicate that an activity exists after the latest date in 
		/// the <see cref="ActivityQuery.DateRanges"/> of the associated <see cref="ActivityQuery"/> object. Likewise
		/// it returns <i>false</i> to indicate that no such activity exists. It returns <i>null</i> if the information is
		/// not available, which could be because ActivityQuery's <see cref="ActivityQuery.RequestedInformation"/> flags didn't
		/// contain the <see cref="ActivityQueryRequestedDataFlags.HasNextActivity"/> flag, or the underlying data connector
		/// doesn't support being able to query for this information.
		/// </para>
		/// <para class="body">
		/// This information is used by schedule controls to provide navigation user interface for navigating to the next
		/// activity that occurs after the currently displayed activities.
		/// </para>
		/// </remarks>
		/// <seealso cref="NextActivity"/>
		/// <seealso cref="HasPreviousActivity"/>
		public bool? HasNextActivity
		{
			get
			{
				return _navigationInfo._hasNextActivity;
			}
		}

		#endregion // HasNextActivity

		#region HasPreviousActivity

		/// <summary>
		/// Indicates whether there's an activity before the earliest date in the <see cref="ActivityQuery.DateRanges"/> of the query.
		/// Property's value is valid only if this information was requested by the associated activity query and the 
		/// information is available.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>HasPreviousActivity</b> property returns <i>true</i> to indicate that an activity exists before the earliest date in 
		/// the <see cref="ActivityQuery.DateRanges"/> of the associated <see cref="ActivityQuery"/> object. Likewise
		/// it returns <i>false</i> to indicate that no such activity exists. It returns <i>null</i> if the information is
		/// not available, which could be because ActivityQuery's <see cref="ActivityQuery.RequestedInformation"/> flags didn't
		/// contain the <see cref="ActivityQueryRequestedDataFlags.HasPreviousActivity"/> flag, or the underlying data connector
		/// doesn't support being able to query for this information.
		/// </para>
		/// <para class="body">
		/// This information is used by schedule controls to provide navigation user interface for navigating to the previous
		/// activity that occurs before the currently displayed activities.
		/// </para>
		/// </remarks>
		/// <seealso cref="NextActivity"/>
		/// <seealso cref="HasPreviousActivity"/>
		public bool? HasPreviousActivity
		{
			get
			{
				return _navigationInfo._hasPrevActivity;
			}
		}

		#endregion // HasPreviousActivity

		#region NextActivity

		/// <summary>
		/// Returns the activity that occurs after the latest date in the <see cref="ActivityQuery.DateRanges"/> of the query.
		/// Property's value is valid only if this information was requested by the associated activity query and the 
		/// information is available.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>NextActivity</b> property returns the next activity that exists after the latest date in 
		/// the <see cref="ActivityQuery.DateRanges"/> of the associated <see cref="ActivityQuery"/> object. 
		/// It returns <i>null</i> if no such activity exists or that the information is not available, 
		/// which could be because ActivityQuery's 
		/// <see cref="ActivityQuery.RequestedInformation"/> flags didn't contain the 
		/// <see cref="ActivityQueryRequestedDataFlags.NextActivity"/> flag, or the underlying data connector
		/// doesn't support being able to query for this information. You can check to see if the <see cref="HasNextActivity"/>
		/// property returns false to ascertain that such an activity doesn't exist.
		/// </para>
		/// <para class="body">
		/// This information is used by schedule controls to provide navigation user interface for navigating to the next
		/// activity that occurs after the currently displayed activities.
		/// </para>
		/// </remarks>
		/// <seealso cref="HasNextActivity"/>
		/// <seealso cref="PreviousActivity"/>
		public ActivityBase NextActivity
		{
			get
			{
				return _navigationInfo._nextActivity;
			}
		}

		#endregion // NextActivity

		#region PreviousActivity

		/// <summary>
		/// Returns the activity that occurs before the earliest date in the <see cref="ActivityQuery.DateRanges"/> of the query.
		/// Property's value is valid only if this information was requested by the associated activity query and the 
		/// information is available.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>PreviousActivity</b> property returns the previous activity that occurs before the earliest date in 
		/// the <see cref="ActivityQuery.DateRanges"/> of the associated <see cref="ActivityQuery"/> object. 
		/// It returns <i>null</i> if no such activity exists or that the information is not available, 
		/// which could be because ActivityQuery's 
		/// <see cref="ActivityQuery.RequestedInformation"/> flags didn't contain the 
		/// <see cref="ActivityQueryRequestedDataFlags.PreviousActivity"/> flag, or the underlying data connector
		/// doesn't support being able to query for this information. You can check to see if the <see cref="HasPreviousActivity"/>
		/// property returns false to ascertain that such an activity doesn't exist.
		/// </para>
		/// <para class="body">
		/// This information is used by schedule controls to provide navigation user interface for navigating to the previous
		/// activity that occurs before the currently displayed activities.
		/// </para>
		/// </remarks>
		/// <seealso cref="HasNextActivity"/>
		/// <seealso cref="PreviousActivity"/>
		public ActivityBase PreviousActivity
		{
			get
			{
				return _navigationInfo._prevActivity;
			}
		}

		#endregion // PreviousActivity
		
		#region Query

		/// <summary>
		/// Gets the query that resulted in this ActivityQueryResult object.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Query</b> property returns the instance of the <see cref="ActivityQuery"/> object
		/// that was used to obtain this <i>ActivityQueryResult</i> instance. <b>Note</b> however
		/// that modifying this query object will not change the <i>ActivityQueryResult</i> to
		/// reflect those modifications. It's only for information reference purposes.
		/// </para>
		/// </remarks>
		/// <seealso cref="XamScheduleDataManager.GetActivities(ActivityQuery)"/>
		public ActivityQuery Query
		{
			get
			{
				return _query;
			}
		}

		#endregion // Query

		#endregion // Public Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region InitializeResult

		/// <summary>
		/// Initializes the the <see cref="Activities"/> and sets <see cref="OperationResult.IsComplete"/> property based on the the 'markCompleted' parameter.
		/// </summary>
		/// <param name="informationBeingProvided">Which information is being provided with this call.</param>
		/// <param name="activities">The activities. Can be null if an error occurred in which case 'error' parameter must be specified.</param>
		/// <param name="hasPreviousActivity">Whether there's an activity prior to the earliest of the dates in the <see cref="ActivityQuery.DateRanges"/>.</param>
		/// <param name="hasNextActivity">Whether there's an activity after to the latest of the dates in the <see cref="ActivityQuery.DateRanges"/>.</param>
		/// <param name="previousActivity">The latest activity prior to the earliest of the dates in the <see cref="ActivityQuery.DateRanges"/>.</param>
		/// <param name="nextActivity">The earliest activity after to the latest of the dates in the <see cref="ActivityQuery.DateRanges"/>.</param>
		/// <param name="error"><see cref="DataErrorInfo"/> object if an error occurred, null otherwise.</param>
		/// <param name="isComplete">Indicates whether the results are complete. Specify fals if further results are
		/// going to be available.</param>
		/// <remarks>
		/// <para class="body">
		/// Activity queries can be executed asynchronously. When a query method is required to return an instance of 
		/// <i>ActivityQueryResult</i>, you can return a new instance and specify the results of the query later via
		/// this method. While the asynchronous query operation is still pending, the <see cref="OperationResult.IsComplete"/> property
		/// will return <i>False</i>. When the results are available, you can call this method with the results and 
		/// specify 'isComplete' parameter as true to indicate that the query operation is complete and that the
		/// <see cref="OperationResult.IsComplete"/> property should be set to true. If only a partial set of results are available and
		/// the query operation is still being executed, you can still call this method to provide with the partial
		/// results however with 'markCompleted' parameter as false to indicate that the results are partial. Once
		/// the results are fully available, you can call this method again (you can pass the same or a different activities 
		/// list instance that you passed in previously) and specify 'markCompleted' parameter accordingly.
		/// The UI objects utilizing the <i>ActivityQueryResult</i> may use IsComplete property to indicate to the end user
		/// that the results are being loaded.
		/// </para>
		/// <para class="body">
		/// If an error occurs during query operation, you can call this method with the <see cref="DataErrorInfo"/> 
		/// object containing the error information. You may specify 'null' for the activities parameter.
		/// Also specifiy true for 'isComplete' since no further action will be taken and operation is 
		/// considered to be complete with an error.
		/// </para>
		/// </remarks>
		/// <seealso cref="Activities"/>
		/// <seealso cref="OperationResult.IsComplete"/>
		/// <seealso cref="OperationResult.Error"/>
		public void InitializeResult( 
			ActivityQueryRequestedDataFlags informationBeingProvided, 
			IList<ActivityBase> activities,
			bool? hasPreviousActivity,
			bool? hasNextActivity,
			ActivityBase previousActivity,
			ActivityBase nextActivity,			
			DataErrorInfo error, 
			bool isComplete )
		{
			if ( 0 != ( ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges & informationBeingProvided ) )
			{
				if ( this.Activities != activities )
					this.Activities.SetSourceCollection( null != activities ? activities : new ActivityBase[0] );
			}

			Action<string> raisePropChangedDelegate = this.RaisePropertyChangedEvent;

			if ( 0 != ( ActivityQueryRequestedDataFlags.NextActivity & informationBeingProvided ) )
				ScheduleUtilities.ChangePropValueHelper( ref _navigationInfo._nextActivity, nextActivity, raisePropChangedDelegate, "NextActivity" );

			if ( 0 != ( ActivityQueryRequestedDataFlags.PreviousActivity & informationBeingProvided ) )
				ScheduleUtilities.ChangePropValueHelper( ref _navigationInfo._prevActivity, previousActivity, raisePropChangedDelegate, "PreviousActivity" );

			if ( 0 != ( ActivityQueryRequestedDataFlags.HasPreviousActivity & informationBeingProvided ) )
				ScheduleUtilities.ChangePropValueHelper( ref _navigationInfo._hasPrevActivity, hasPreviousActivity, raisePropChangedDelegate, "HasPreviousActivity" );

			if ( 0 != ( ActivityQueryRequestedDataFlags.HasNextActivity & informationBeingProvided ) )
				ScheduleUtilities.ChangePropValueHelper( ref _navigationInfo._hasNextActivity, hasNextActivity, raisePropChangedDelegate, "HasNextActivity" );

			base.InitializeResult( error, isComplete );
		}

		#endregion // InitializeResult

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // ActivityQueryResult Class
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