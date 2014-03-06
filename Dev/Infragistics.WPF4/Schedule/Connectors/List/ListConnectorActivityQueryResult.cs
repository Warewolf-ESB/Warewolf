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
using Infragistics.Controls.Primitives;

namespace Infragistics.Controls.Schedules

{
	internal class ListQueryResult : PropertyChangeNotifierExtended
	{
		#region Member Vars

		/// <summary>
		/// This object contains information from which the query criteria of this result are
		/// derived. This is used when query conditions need to be re-derived, for example when 
		/// property mappings are changed. In the case of ActivityListManager, this is an instance
		/// of ActivityQueryComponent.
		/// </summary>
		private object _externalQueryInfo;

		private IList _dataList;
		private IList _viewList;
		private DataErrorInfo _error;
		private bool _isComplete;

		private LinqQueryManager.ILinqStatement _linqStatement;

		/// <summary>
		/// True if the result is simply a filtered list of data items. If it's something else,
		/// like summary of data or some other value derived from data then this will be false.
		/// </summary>
		private bool _isResultFilteredDataItems;

		/// <summary>
		/// If true then ViewList will be created by the list manager for this result. If false,
		/// only dataList will be assigned but the view list won't be created.
		/// </summary>
		private bool _shouldCreateViewList;

		/// <summary>
		/// This callback is used to verify the result after it's marked dirty.
		/// </summary>
		private Action<ListQueryResult> _reevaluateCallback;
		private bool _isDirty;

		/// <summary>
		/// If true then the items in the view list can be reused when the result is verified. If false
		/// the items in the view list will be discarded and new ones will be created. This is set to true
		/// when property mappings or the list itself etc... changes.
		/// </summary>
		private bool _isDirty_discardViewItems;

		/// <summary>
		/// List manager calls ProcessChangeEvent method to give the result itself a chance to process
		/// the change notification from the underlying data source. ProcessChangeEvent delegates the call
		/// to this callback if it's provided in the constructor.
		/// </summary>
		private Func<ListQueryResult, DataListChangeInfo, bool> _processChangeEventAction;

		// SSP 4/21/11 TFS73037
		// 
		internal TypedEnumerable<object> _verifyItemsInResult_Enumerable;
		internal System.Linq.IQueryable _verifyItemsInResult_Result;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="externalQueryInfo">This object contains information from which the the list manager derives
		/// the query condition of this result. This is used when query conditions need to be re-derived, for example 
		/// when property mappings are changed. In the case of ActivityListManager, this is an instance of ActivityQuery.
		/// </param>
		/// <param name="isResultFilteredDataItems">True indicates that this query result contains a subset of the items
		/// from the data source. False indicates that the query result is not a simple filtered subset of items
		/// in the data source but rather other calculated information, like navigation related piece of data or a calculated
		/// summary of a field.</param>
		/// <param name="shouldCreateViewList">Specifies whether the list manager should create view list around the
		/// resultant data list. It's false for HasNext/PreviousActivity type queries where the resulting data is not
		/// a list of data items.</param>
		/// <param name="processChangeEventAction">
		/// Called to process list change event.
		/// </param>
		public ListQueryResult( object externalQueryInfo, bool isResultFilteredDataItems, bool shouldCreateViewList, Func<ListQueryResult, DataListChangeInfo, bool> processChangeEventAction = null )
		{
			CoreUtilities.ValidateNotNull( externalQueryInfo );

			_externalQueryInfo = externalQueryInfo;
			_isResultFilteredDataItems = isResultFilteredDataItems;
			_shouldCreateViewList = shouldCreateViewList;
			_processChangeEventAction = processChangeEventAction;
		}

		#endregion // Constructor

		#region Properties

		#region DataList

		/// <summary>
		/// The result list that contains the data source objects.
		/// </summary>
		internal IList DataList
		{
			get
			{
				return _dataList;
			}
		}

		#endregion // DataList

		#region Error

		/// <summary>
		/// Error if any.
		/// </summary>
		public DataErrorInfo Error
		{
			get
			{
				return _error;
			}
		}

		#endregion // Error

		#region ExternalQueryInfo

		/// <summary>
		/// This object contains information from which the query condition of this result are
		/// derived. This is used when query conditions need to be re-derived, for example when 
		/// property mappings are changed. In the case of ActivityListManager, this is an instance
		/// of ActivityQuery.
		/// </summary>
		internal object ExternalQueryInfo
		{
			get
			{
				return _externalQueryInfo;
			}
		}

		#endregion // ExternalQueryInfo

		#region IsComplete

		internal bool IsComplete
		{
			get
			{
				return _isComplete;
			}
		}

		#endregion // IsComplete

		#region IsDirty

		internal bool IsDirty
		{
			get
			{
				return _isDirty;
			}
		}

		#endregion // IsDirty

		#region IsResultFilteredDataItems

		internal bool IsResultFilteredDataItems
		{
			get
			{
				return _isResultFilteredDataItems;
			}
		}

		#endregion // IsResultFilteredDataItems

		#region LinqStatement

		internal LinqQueryManager.ILinqStatement LinqStatement
		{
			get
			{
				return _linqStatement;
			}
		}

		#endregion // LinqStatement

		#region ShouldCreateViewList

		internal bool ShouldCreateViewList
		{
			get
			{
				return _shouldCreateViewList;
			}
		}

		#endregion // ShouldCreateViewList

		#region ShouldProcessChangeEvent

		internal bool ShouldProcessChangeEvent
		{
			get
			{
				if ( _isDirty || null != _error || null == _dataList )
					return false;

				return true;
			}
		}

		#endregion // ShouldProcessChangeEvent

		#region ViewList

		internal IList ViewList
		{
			get
			{
				return _viewList;
			}
		}

		#endregion // ViewList 

		#endregion // Properties

		#region Methods

		#region Initialize

		internal void Initialize( IList viewList, IList dataList, DataErrorInfo error, bool isComplete )
		{
			this.Initialize( _linqStatement, viewList, dataList, error, isComplete );
		}

		private void Initialize( LinqQueryManager.ILinqStatement linqStatement, IList viewList, IList dataList, DataErrorInfo error, bool isComplete )
		{
			this.InitializeLinqStatement( linqStatement );

			bool raiseViewList = _viewList != viewList;
			bool raiseDataList = _dataList != dataList;
			bool raiseError = _error != error;
			bool raiseIsComplete = _isComplete != isComplete;

			// Set all the properties first and then raise the property change events. This is so that listeners
			// don't have to process the notifications multiple times if they process all of these values once
			// in any one of the prop change notification.
			// 
			_viewList = viewList;
			_dataList = dataList;
			_error = error;
			_isComplete = isComplete;

			if ( raiseViewList )
				this.RaisePropertyChangedEvent( "ViewList" );

			if ( raiseDataList )
				this.RaisePropertyChangedEvent( "DataList" );

			if ( raiseError )
				this.RaisePropertyChangedEvent( "Error" );

			if ( raiseIsComplete )
				this.RaisePropertyChangedEvent( "IsComplete" );
		}

		#endregion // Initialize

		#region InitializeLinqStatement

		internal void InitializeLinqStatement( LinqQueryManager.ILinqStatement value )
		{
			if ( _linqStatement != value )
			{
				// SSP 4/21/11 TFS73037
				// 
				_verifyItemsInResult_Enumerable = null;
				_verifyItemsInResult_Result = null;

				_linqStatement = value;
				this.RaisePropertyChangedEvent( "LinqStatement" );
			}
		}

		#endregion // InitializeLinqStatement

		#region MarkClean

		internal void MarkClean( Action<ListQueryResult> reevaluateCallback )
		{
			if ( _isDirty )
			{
				_isDirty = false;
				_isDirty_discardViewItems = false;

				Debug.Assert( null != reevaluateCallback );
				_reevaluateCallback = reevaluateCallback;

				this.RaisePropertyChangedEvent( "IsDirty" );
			}
		}

		#endregion // MarkClean

		#region MarkDirty

		// MD 4/29/11 - TFS57206
		// Added a parameter to indicate whether we should clear the data list.
		//internal void MarkDirty( bool discardViewItems )
		internal void MarkDirty(bool discardViewItems, bool clearDataList = true)
		{
			if ( !_isDirty || discardViewItems && !_isDirty_discardViewItems )
			{
				if ( !_isDirty )
				{
					_isDirty = true;
					this.RaisePropertyChangedEvent( "IsDirty" );
				}

				if ( discardViewItems )
				{
					_isDirty_discardViewItems = true;
					this.Initialize( null, null, null, null, false );
				}
				else if ( null != _dataList )
				{
					// MD 4/29/11 - TFS57206
					//_dataList.Clear( );
					if (clearDataList)
						_dataList.Clear();
				}
			}
		}

		#endregion // MarkDirty

		#region ProcessChangeEvent

		internal bool ProcessChangeEvent( DataListChangeInfo changeInfo )
		{
			if ( ! this.ShouldProcessChangeEvent )
				return false;
			
			return null == _processChangeEventAction || _processChangeEventAction( this, changeInfo );
		}

		#endregion // ProcessChangeEvent

		#region Verify

		internal void Verify( )
		{
			if ( !_isDirty && null != _reevaluateCallback )
			{
				var tmp = _reevaluateCallback;
				_reevaluateCallback = null;
				tmp( this );
			}
		}

		#endregion // Verify 

		#endregion // Methods
	}

	#region ActivityQueryComponent Class

	internal class ActivityQueryComponent
	{
		internal bool _isRecurrenceQuery;
		internal bool _isReminderQuery;
		internal TimeSpan[] _reminderIntervalBrackets;
		internal DateRange[] _reminderQueryDateRanges;
		internal ActivityQuery _query;
		internal ActivityQueryRequestedDataFlags _singleFlag;

		private ActivityQueryComponent( )
		{
		}

		internal static ActivityQueryComponent Create( bool isRecurrenceQuery, ActivityQuery query, ActivityQueryRequestedDataFlags singleFlag )
		{
			return new ActivityQueryComponent( )
			{
				_isRecurrenceQuery = isRecurrenceQuery,
				_query = query,
				_singleFlag = singleFlag
			};
		}

		internal static ActivityQueryComponent CreateReminderQuery( bool isRecurrenceQuery, ActivityQuery query, TimeSpan[] reminderIntervalBrackets )
		{
			return new ActivityQueryComponent( )
			{
				_isReminderQuery = true,
				_isRecurrenceQuery = isRecurrenceQuery,
				_reminderIntervalBrackets = reminderIntervalBrackets,
				_reminderQueryDateRanges = null != reminderIntervalBrackets ? new DateRange[ reminderIntervalBrackets.Length ] : null,
				_query = query,
				_singleFlag = ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges
			};
		}
	}

	#endregion // ActivityQueryComponent Class

	internal class ListConnectorActivityQueryResult : ActivityQueryResult
	{
		private IEnumerable<ListQueryResult> _individualActivityResults;
		private IList<ActivityBase> _lastOverallResultList;

		public ListConnectorActivityQueryResult( ActivityQuery query, IEnumerable<ListQueryResult> individualActivityResults )
			: base( query )
		{
			this.InitializeResults( individualActivityResults );
		}

		private void InitializeResults( IEnumerable<ListQueryResult> results )
		{
			_individualActivityResults = results;

			PropertyChangeListener<ListConnectorActivityQueryResult> listener =
				new PropertyChangeListener<ListConnectorActivityQueryResult>( this, OnListQueryResultChangedHandler );

			foreach ( ListQueryResult ii in results )
			{
				ii.AddListener( listener, false );
			}

			this.ReinitializeResults( null );
		}

		private void ReinitializeResults( ListQueryResult changedQueryResult )
		{
			List<IEnumerable> viewLists = null;
			List<DataErrorInfo> errors = null;
			bool isComplete = true;

			ActivityNavigationInfo navigationInfo = new ActivityNavigationInfo( );

			foreach ( ListQueryResult ii in _individualActivityResults )
			{
				ii.Verify( );

				// Skip results marked dirty. They re-evaluation is pending.
				// 
				if ( !ii.IsDirty )
				{
					ActivityQueryComponent qc = ii.ExternalQueryInfo as ActivityQueryComponent;
					
					ActivityQueryRequestedDataFlags flag = qc._singleFlag;
					switch ( flag )
					{
						case ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges:
							if ( null != ii.ViewList )
							{
								if ( null == viewLists )
									viewLists = new List<IEnumerable>( );

								viewLists.Add( ii.ViewList );
							}
							break;
						default:
							{
								ActivityBase activityValue = CoreUtilities.GetFirstItem<ActivityBase>( ii.ViewList, true );
								bool hasValue = null != CoreUtilities.GetFirstItem<object>( ii.DataList, true );

								switch ( flag )
								{
									case ActivityQueryRequestedDataFlags.HasPreviousActivity:
										navigationInfo.SetHasPrevNextHelper( hasValue, true );
										break;
									case ActivityQueryRequestedDataFlags.HasNextActivity:
										navigationInfo.SetHasPrevNextHelper( hasValue, false );
										break;
									case ActivityQueryRequestedDataFlags.PreviousActivity:
										navigationInfo.SetPrevNextHelper( activityValue, true );
										break;
									case ActivityQueryRequestedDataFlags.NextActivity:
										navigationInfo.SetPrevNextHelper( activityValue, false );
										break;
								}

								break;
							}
					}					

					if ( null != ii.Error )
					{
						if ( null == errors )
							errors = new List<DataErrorInfo>( );

						errors.Add( ii.Error );
					}

					isComplete = isComplete && ii.IsComplete;
				}
				else
				{
					isComplete = false;
				}
			}

			DataErrorInfo errorInfo = null != errors && errors.Count > 0 ? new DataErrorInfo( errors ) : null;

			IList<ActivityBase> overallResultList = null;
			if ( null != viewLists )
			{
				if ( viewLists.Count > 1 )
				{
					AggregateCollection<ActivityBase> lastColl = _lastOverallResultList as AggregateCollection<ActivityBase>;
					if ( null != lastColl && ScheduleUtilities.AreEqual( lastColl.Collections, viewLists ) )
						overallResultList = lastColl;
					else
						overallResultList = new AggregateCollection<ActivityBase>( viewLists );
				}
				else if ( viewLists.Count > 0 )
					overallResultList = (IList<ActivityBase>)viewLists[0];
			}

			_lastOverallResultList = overallResultList;

			// Only initialize the results if we either have result list or error info. If we have none,
			// which is the case when data source hasn't been provided then simply return an uninitialized
			// result.
			// 
			this.InitializeResult( ActivityQueryRequestedDataFlags.All, 
				overallResultList, 
				navigationInfo._hasPrevActivity, 
				navigationInfo._hasNextActivity, 
				navigationInfo._prevActivity, 
				navigationInfo._nextActivity, 
				errorInfo, isComplete 
			);
		}

		private static void OnListQueryResultChangedHandler( ListConnectorActivityQueryResult owner, object sender, string propName, object extraInfo )
		{
			ListQueryResult queryResult = sender as ListQueryResult;
			if ( null != queryResult && ( "IsDirty" == propName
				// If the query result is dirty then skip IsComplete and ViewList since we don't include the dirty
				// results in the overall result list. When the result is marked IsDirty = false, we'll put it
				// back into the overall result list.
				// 
				|| ! queryResult.IsDirty && ( "IsComplete" == propName || "ViewList" == propName ) ) )
				owner.ReinitializeResults( queryResult );
		}
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