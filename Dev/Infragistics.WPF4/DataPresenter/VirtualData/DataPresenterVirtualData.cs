using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.Windows.Markup;
using System.ComponentModel;
using Infragistics.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Collections.ObjectModel;


using System.Linq.Expressions;


namespace Infragistics.Windows.DataPresenter
{

	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	internal class GroupInfo
	{
		#region Member Vars

		private object _tag;
		private object _value;
		private int _childGroupCount;
		private int _childDataRecordCount;
		private string _description; 

		#endregion // Member Vars

		#region Value

		/// <summary>
		/// Specifies the value associated with the group.
		/// </summary>
		public object Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value = value;
			}
		}

		#endregion // Value

		#region ChildGroupCount

		/// <summary>
		/// Specifies the number of sub-groups if any.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Whether a group has sub-groups depends on whether there's another group-by field in the
		/// associated <see cref="FieldLayout"/>'s <see cref="FieldLayout.SortedFields"/> collection
		/// after the group-by field associated with this group. If there are no other group-by fields
		/// after the current group's group-by field, then this value will be ignored.
		/// </para>
		/// </remarks>
		public int ChildGroupCount
		{
			get
			{
				return _childGroupCount;
			}
			set
			{
				_childGroupCount = value;
			}
		}

		#endregion // ChildGroupCount

		#region ChildDataRecordCount

		/// <summary>
		/// Specifies the number of child data records this group has.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// This value is used for the purposes of deriving a default description for the group-by record.
		/// </para>
		/// </remarks>
		public int ChildDataRecordCount
		{
			get
			{
				return _childDataRecordCount;
			}
			set
			{
				_childDataRecordCount = value;
			}
		}

		#endregion // ChildDataRecordCount

		#region Description

		/// <summary>
		/// Description to display in the group-by record.
		/// </summary>
		public string Description
		{
			get
			{
				return _description;
			}
			set
			{
				_description = value;
			}
		}

		#endregion // Description

		#region Tag

		/// <summary>
		/// Gets or sets tag object.
		/// </summary>
		public object Tag
		{
			get
			{
				return _tag;
			}
			set
			{
				_tag = value;
			}
		} 

		#endregion // Tag

		#region ChildCount

		/// <summary>
		/// Returns the child group count if there's a level below otherwise returns the child data record count.
		/// </summary>
		internal int ChildCount
		{
			get
			{
				return _childGroupCount > 0 ? _childGroupCount : _childDataRecordCount;
			}
		} 

		#endregion // ChildCount
	}

	/// <summary>
	/// Enum for specifying <see cref="FieldLayoutSettings.SortEvaluationMode"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	public enum SortEvaluationMode
	{
		/// <summary>
		/// Default is resolved to <i>Internal</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Sorting operation is done internally by the data presenter. The data presenter sorts its record collection.
		/// </summary>
		Auto,

		/// <summary>
		/// The data presenter will take no actions when the user sorts a field. This option is useful if you want to 
		/// externally perform the sorting operation on the data source. You can use the data presenter’s 
		/// <see cref="DataPresenterBase.Sorting"/> or <see cref="DataPresenterBase.Sorted"/> event to get notified 
		/// of change in the sorted fields and take appropriate action to sort the data source.
		/// </summary>
		Manual,

		/// <summary>
		/// If the underlying data source is ICollectionView, the data presenter will manipulate the
		/// ICollectionView's SortedDescriptions to reflect the sort critieria chosen by the user. It will
		/// also synchronize its <see cref="FieldLayout.SortedFields"/> to reflect the ICollectionView's
		/// SortDescriptions, however only for the root field-layout.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that if the ICollectionView's SortDescriptions contains an entry that does not
		/// have a corresponding field in the data presenter, the entry will be removed upon any changes
		/// made in the sorting criteria via the data presenter UI.
		/// </para>
		/// </remarks>
		UseCollectionView
	}

	/// <summary>
	/// Enum for specifying <see cref="FieldLayoutSettings.FilterEvaluationMode"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	public enum FilterEvaluationMode
	{
		/// <summary>
		/// Default is resolved to <i>Internal</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Filtering operation is done internally by the data presenter.
		/// </summary>
		Auto,

		/// <summary>
		/// The data presenter will take no actions when the user selects filter criteria. This option is useful if you 
		/// want to externally apply filtering to the data source. You can use the data presenter’s 
		/// <see cref="DataPresenterBase.RecordFilterChanging"/> and <see cref="DataPresenterBase.RecordFilterChanged"/> 
		/// events to get notified when the filter criteria is changed by the user and take 
		/// appropriate action to filter the underlying data list. Furthermore, the data presenter will not populate the 
		/// filter drop-down list with unique list of items. You’ll need to hook into the RecordFilterDropDownPopulating 
		/// event and populate the filter drop-down list with unique values that the user can select, assuming you wish 
		/// to present to the user such a list of unique values.
		/// </summary>
		Manual,

		/// <summary>
		/// If the underlying data source is ICollectionView, the data presenter will utilize the ICollectionView's
		/// Filter property to perform the filtering operation. Note that the Filter property will be reset to a
		/// predicate created by the data presenter when the end user selects a filter criteria through the data presenter
		/// UI.
		/// </summary>
		UseCollectionView
	}

	/// <summary>
	/// Enum for specifying <see cref="FieldLayoutSettings.GroupByEvaluationMode"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	public enum GroupByEvaluationMode
	{
		/// <summary>
		/// Default is resolved to <i>Internal</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Grouping operation is done internally by the data presenter.
		/// </summary>
		Auto,

		/// <summary>
		/// If the underlying data source is ICollectionView, the data presenter will utilize the ICollectionView's
		/// GroupDescriptions property to perform the grouping operation. The data presenter will manipulate the
		/// GroupDescriptions property to reflect the group-by fields in the FieldLayout's 
		/// <see cref="FieldLayout.SortedFields"/> collection. The data presenter will also synchronize its
		/// SortedFields to reflect the group-by fields in the ICollectionView's GroupDescriptions property, 
		/// however only for the root field-layout.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that any entry in the ICollectionView's GroupDescriptions that doesn't have a corresponding
		/// field in the data presenter will be removed.
		/// </para>
		/// </remarks>
		UseCollectionView
	}

	/// <summary>
	/// Enum for specifying <see cref="FieldLayoutSettings.SummaryEvaluationMode"/> property.
	/// </summary>
	[InfragisticsFeature( FeatureName = "External_ViewOperations", Version = "12.1" )]
	public enum SummaryEvaluationMode
	{
		/// <summary>
		/// Default is resolved to <i>Internal</i>.
		/// </summary>
		Default,

		/// <summary>
		/// Summaries are calculated internally by the data presenter.
		/// </summary>
		Auto,

		/// <summary>
		/// The data presenter will not calculate summary values. You’ll have to perform the calculation and 
		/// provide the result via the data presenter’s <see cref="DataPresenterBase.QuerySummaryResult"/> event.
		/// </summary>
		Manual,


		/// <summary>
		/// Uses LINQ for calculating summaries. Note that the underlying data items and the data source must 
		/// support LINQ. If LINQ is not supported then summaries will not be calculated.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the <see cref="FieldLayoutSettings.CalculationScope"/> property will not be supported. 
		/// <i>CalculationScope</i> defaults to <i>FilteredSortedList</i> which will be the behavior of using this option. 
		/// The non-default  options of <i>CalculationScope</i> (<i>FullUnsortedList</i> and <i>FullSortedList</i>) 
		/// will not be supported.
		/// </para>
		/// </remarks>
		UseLinq

	}

	internal class SortedFieldsToCollectionViewSynchronizer
	{
		internal readonly RecordManager _rm;
		internal readonly FieldLayout _fieldLayout;
		private PropertyValueTracker _sortVersionTracker;
		private PropertyValueTracker _groupByVersionTracker;
		private bool _isUpdatingCollectionView;
		private bool _isUpdatingFieldLayoutSortedFields;
		private SortDescriptionCollection _lastHookedSortDescriptions;
		private ObservableCollection<GroupDescription> _lastHookedGroupDescriptions;

		private SortedFieldsToCollectionViewSynchronizer( RecordManager rm )
		{
			_rm = rm;
			_fieldLayout = rm.FieldLayout;
		}

		private void Init( )
		{
			_sortVersionTracker = new PropertyValueTracker( _fieldLayout, "SortVersion", this.OnSortVersionChanged );
			_groupByVersionTracker = new PropertyValueTracker( _fieldLayout, "GroupByVersion", this.OnGroupByVersionChanged );
		}

		internal bool IsUpdatingCollectionView
		{
			get
			{
				return _isUpdatingCollectionView;
			}
		}

		private ICollectionView GetCollectionView( )
		{
			return VUtils.GetCollectionView( _rm );
		}

		private void OnSortVersionChanged( )
		{
			this.UpdateCollectionViewSortDescriptions( );
		}

		private void OnGroupByVersionChanged( )
		{
			this.UpdateCollectionViewGroupDescriptions( );
		}

		internal void Synchronize( bool async )
		{
			DataPresenterBase dp = _fieldLayout.DataPresenter;
			if ( !async || null == dp || dp.IsSynchronousControl )
			{
				this.SynchronizeHelper( );
			}
			else
			{
				dp.Dispatcher.BeginInvoke( DispatcherPriority.DataBind, new GridUtilities.MethodDelegate( this.SynchronizeHelper ) );
			}
		}

		private void SynchronizeHelper( )
		{
			// Give higher priority to the collection view's sort descriptions.
			// 
			if ( _fieldLayout.HasSortedFields )
			{
				this.UpdateCollectionViewGroupDescriptions( );
				this.UpdateCollectionViewSortDescriptions( );
			}
			else
			{
				if ( ! this.AreGroupDescriptionsSynchronized( ) )
					this.UpdateFieldLayoutGroupedFields( );

				if ( !this.AreSortedDescriptionsSynchronized( ) )
					this.UpdateFieldLayoutSortedFields( );
			}
		}

		internal bool AreSortedDescriptionsSynchronized( )
		{
			ICollectionView view = this.GetCollectionView( );

			Debug.Assert( null != view && view.CanSort );

			if ( null != view && view.CanSort )
			{
				FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;
				bool areSame = true;
				int viewCollIndex = 0;

				for ( int i = 0; areSame && i < fsdColl.Count; i++ )
				{
					FieldSortDescription fsd = fsdColl[i];
					if ( !fsd.IsGroupBy )
					{
						if ( viewCollIndex >= view.SortDescriptions.Count )
						{
							areSame = false;
						}
						else
						{
							SortDescription sd = view.SortDescriptions[viewCollIndex++];

							if ( fsd.Direction != sd.Direction || VUtils.GetPropertyName( fsd ) != sd.PropertyName )
								areSame = false;
						}
					}
				}

				areSame = areSame && viewCollIndex == view.SortDescriptions.Count;

				return areSame;
			}

			return false;
		}

		internal bool AreGroupDescriptionsSynchronized( )
		{
			ICollectionView view = this.GetCollectionView( );

			Debug.Assert( null != view && view.CanGroup );

			if ( null != view && view.CanGroup )
			{
				FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;
				bool areSame = true;
				int viewCollIndex = 0;

				for ( int i = 0; areSame && i < fsdColl.Count; i++ )
				{
					FieldSortDescription fsd = fsdColl[i];
					if ( fsd.IsGroupBy )
					{
						if ( viewCollIndex >= view.GroupDescriptions.Count )
						{
							areSame = false;
						}
						else
						{
							PropertyGroupDescription gd = view.GroupDescriptions[viewCollIndex++] as PropertyGroupDescription;

							if ( null == gd || VUtils.GetPropertyName( fsd ) != gd.PropertyName )
								areSame = false;
						}
					}
					else
						break;
				}

				areSame = areSame && viewCollIndex == view.GroupDescriptions.Count;

				return areSame;
			}

			return false;
		}

		internal void UpdateCollectionViewSortDescriptions( )
		{
			if ( _isUpdatingCollectionView || _isUpdatingFieldLayoutSortedFields )
				return;

			_isUpdatingCollectionView = true;
			try
			{
				ICollectionView view = this.GetCollectionView( );

				if ( null != view && view.CanSort && SortEvaluationMode.UseCollectionView == _rm.SortEvaluationModeResolved )
				{
					if ( !this.AreSortedDescriptionsSynchronized( ) )
					{
						IDisposable deferRefresh = view.DeferRefresh( );
						try
						{
							FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;
							view.SortDescriptions.Clear( );

							for ( int i = 0; i < fsdColl.Count; i++ )
							{
								FieldSortDescription fsd = fsdColl[i];
								if ( !fsd.IsGroupBy )
								{
									SortDescription sd = new SortDescription( VUtils.GetPropertyName( fsd ), fsd.Direction );
									view.SortDescriptions.Add( sd );
								}
							}
						}
						finally
						{
							deferRefresh.Dispose( );
						}
					}

					this.HookUnhook_CollectionView_SortDescriptions( view, false );
				}
			}
			finally
			{
				_isUpdatingCollectionView = false;
			}
		}

		internal void UpdateCollectionViewGroupDescriptions( )
		{
			if ( _isUpdatingCollectionView || _isUpdatingFieldLayoutSortedFields )
				return;

			_isUpdatingCollectionView = true;
			try
			{
				ICollectionView view = this.GetCollectionView( );

				if ( null != view && view.CanGroup && GroupByEvaluationMode.UseCollectionView == _rm.GroupByEvaluationModeResolved )
				{
					if ( !this.AreGroupDescriptionsSynchronized( ) )
					{
						this.ClearGroupsSynchronizerHelper( );

						IDisposable deferRefresh = view.DeferRefresh( );
						try
						{
							FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;
							view.GroupDescriptions.Clear( );

							for ( int i = 0; i < fsdColl.Count; i++ )
							{
								FieldSortDescription fsd = fsdColl[i];
								if ( fsd.IsGroupBy )
								{
									PropertyGroupDescription gd = new PropertyGroupDescription(
										VUtils.GetPropertyName( fsd ), null, StringComparison.CurrentCultureIgnoreCase );

									view.GroupDescriptions.Add( gd );
								}
								else
									break;
							}
						}
						finally
						{
							deferRefresh.Dispose( );
						}

						this.CreateGroupByRecordsFromGroups( view );
					}

					this.HookUnhook_CollectionView_GroupDescriptions( view, false );
				}
			}
			finally
			{
				_isUpdatingCollectionView = false;
			}
		}

		private void HookUnhook_CollectionView_SortDescriptions( ICollectionView cv, bool unhookOnly )
		{
			SortDescriptionCollection coll = null != cv && ! unhookOnly ? cv.SortDescriptions : null;

			if ( _lastHookedSortDescriptions != coll )
			{
				INotifyCollectionChanged nc = _lastHookedSortDescriptions as INotifyCollectionChanged;
				if ( null != nc )
					nc.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnCollectionView_SortDescriptionsChanged );

				_lastHookedSortDescriptions = coll;

				nc = _lastHookedSortDescriptions as INotifyCollectionChanged;
				if ( null != nc )
					nc.CollectionChanged += new NotifyCollectionChangedEventHandler( OnCollectionView_SortDescriptionsChanged );
			}
		}

		private void HookUnhook_CollectionView_GroupDescriptions( ICollectionView cv, bool unhookOnly )
		{
			ObservableCollection<GroupDescription> coll = null != cv && ! unhookOnly ? cv.GroupDescriptions : null;

			if ( _lastHookedGroupDescriptions != coll )
			{
				INotifyCollectionChanged nc = _lastHookedGroupDescriptions as INotifyCollectionChanged;
				if ( null != nc )
					nc.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnCollectionView_GroupDescriptionsChanged );

				_lastHookedGroupDescriptions = coll;

				nc = _lastHookedGroupDescriptions as INotifyCollectionChanged;
				if ( null != nc )
					nc.CollectionChanged += new NotifyCollectionChangedEventHandler( OnCollectionView_GroupDescriptionsChanged );
			}
		}

		private void CreateGroupByRecordsFromGroups( ICollectionView cv )
		{
			if ( cv.CanGroup && cv.GroupDescriptions.Count > 0 )
			{
				ReadOnlyObservableCollection<object> groups = cv.Groups;
				if ( null != groups )
					VUtils.CreateGroupByRecordsSynchronizerHelper( _rm.Groups, groups );
			}
			else
				this.ClearGroupsSynchronizerHelper( );
		}

		private void ClearGroupsSynchronizerHelper( )
		{
			if ( null != _rm.GroupsInternal && null != _rm.GroupsInternal._groupsSynchronizer )
			{
				_rm.GroupsInternal._groupsSynchronizer.Dispose( );
				_rm.GroupsInternal._groupsSynchronizer = null;
			}
		}

		private void UpdateFieldLayoutSortedFields( )
		{
			if ( _isUpdatingFieldLayoutSortedFields || _isUpdatingCollectionView )
				return;

			_isUpdatingFieldLayoutSortedFields = true;

			FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;

			fsdColl.BeginUpdate( );

			try
			{
				int startIndex = 0;

				// Move the startIndex to the first sort field that's not a group-by field.
				// 
				while ( startIndex < fsdColl.Count && fsdColl[startIndex].IsGroupBy )
					startIndex++;

				// Remove all the sorted fields.
				// 
				while ( startIndex < fsdColl.Count )
					fsdColl.RemoveAt( startIndex );

				ICollectionView view = this.GetCollectionView( );
				if ( null != view && view.CanSort )
				{
					SortDescriptionCollection coll = view.SortDescriptions;
					if ( null != coll )
					{
						foreach ( SortDescription ii in coll )
						{
							Field field = null != ii ? VUtils.GetField( _fieldLayout, ii.PropertyName ) : null;
							bool isCompatible = null != field;

							if ( isCompatible )
								fsdColl.Add( new FieldSortDescription( field.Name, ii.Direction, false ) );
						}
					}
				}
			}
			finally
			{
				fsdColl.EndUpdate( );

				_isUpdatingFieldLayoutSortedFields = false;
			}
		}

		private void UpdateFieldLayoutGroupedFields( )
		{
			if ( _isUpdatingFieldLayoutSortedFields || _isUpdatingCollectionView )
				return;

			_isUpdatingFieldLayoutSortedFields = true;

			FieldSortDescriptionCollection fsdColl = _fieldLayout.SortedFields;

			fsdColl.BeginUpdate( );

			try
			{
				// Remove all the group-by fields.
				// 
				while ( fsdColl.Count > 0 && fsdColl[0].IsGroupBy )
					fsdColl.RemoveAt( 0 );

				ICollectionView view = this.GetCollectionView( );
				if ( null != view && view.CanGroup )
				{
					ObservableCollection<GroupDescription> coll = view.GroupDescriptions;
					if ( null != coll )
					{
						foreach ( GroupDescription ii in coll )
						{
							PropertyGroupDescription pgd = ii as PropertyGroupDescription;

							Field field = null != pgd ? VUtils.GetField( _fieldLayout, pgd.PropertyName ) : null;
							bool isCompatible = null != field;

							if ( isCompatible )
								fsdColl.Add( new FieldSortDescription( field.Name, ListSortDirection.Ascending, true ) );
						}
					}

					this.CreateGroupByRecordsFromGroups( view );
				}
			}
			finally
			{
				fsdColl.EndUpdate( );

				_isUpdatingFieldLayoutSortedFields = false;
			}
		}

		private void OnCollectionView_SortDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs args )
		{
			if ( ! _isUpdatingCollectionView )
			{
				this.UpdateFieldLayoutSortedFields( );
			}
		}

		private void OnCollectionView_GroupDescriptionsChanged( object sender, NotifyCollectionChangedEventArgs args )
		{
			if ( !_isUpdatingCollectionView )
			{
				this.UpdateFieldLayoutGroupedFields( );
			}
		}

		internal void Dispose( )
		{
			// Unhook from the collection view's sort descriptions.
			// 
			this.HookUnhook_CollectionView_SortDescriptions( null, true );
			this.HookUnhook_CollectionView_GroupDescriptions( null, true );
		}

		internal static SortedFieldsToCollectionViewSynchronizer CreateHelper( RecordManager rm )
		{
			SortedFieldsToCollectionViewSynchronizer ret = null;
			FieldLayout fl = null != rm ? rm.FieldLayout : null;
			if ( null != fl )
			{
				ret = new SortedFieldsToCollectionViewSynchronizer( rm );
				ret.Init( );
			}

			return ret;
		}
	}






	internal class GroupsSynchronizer : ListSynchronizer<object, GroupByRecord>
	{
		internal readonly RecordCollectionBase _groupByRecordsColl;
		internal readonly Field _groupByField;

		internal GroupsSynchronizer( RecordCollectionBase groupByRecordsColl, IList<object> groups )
			: base( groups, new CoreUtilities.TypedList<GroupByRecord>( groupByRecordsColl.SparseArray ) )
		{
			_groupByRecordsColl = groupByRecordsColl;
			_groupByField = _groupByRecordsColl.GroupByField;

			this.Initialize( CreateGroupByRecordCallback, InitGroupByRecordCallback, GetGroupObjectFromGroupByRecord );
		}

		private GroupByRecord CreateGroupByRecordCallback( object groupObj )
		{
			GroupInfo groupInfo = groupObj as GroupInfo;
			if ( null == groupInfo )
			{
				CollectionViewGroup cvg = groupObj as CollectionViewGroup;
				if ( null != cvg )
				{
					groupInfo = new GroupInfo( )
					{
						 Value = cvg.Name,
						 ChildDataRecordCount = cvg.ItemCount,
						 ChildGroupCount = cvg.IsBottomLevel ? 0 : cvg.Items.Count,
						 Tag = cvg
					};
				}
			}

			Debug.Assert( null != groupInfo );
			return new GroupByRecord( _groupByRecordsColl.GroupByField, _groupByRecordsColl, groupInfo );
		}

		private object GetGroupObjectFromGroupByRecord( GroupByRecord groupByRecord )
		{
			return groupByRecord.Value;
		}

		private void InitGroupByRecordCallback( GroupByRecord groupByRecord )
		{
			groupByRecord.FireInitializeRecord( );
		}

		protected override void RaiseDestCollectionEvent( NotifyCollectionChangedEventArgs args )
		{
			_groupByRecordsColl.RaiseChangeEvents( true );
		}
	}


	internal class DataRecordsSynchronizer : ListSynchronizer<object, DataRecord>
	{
		internal readonly RecordCollectionBase _dataRecordsColl;

		internal DataRecordsSynchronizer( RecordCollectionBase dataRecordsColl, IList<object> dataItems )
			: base( dataItems, new CoreUtilities.TypedList<DataRecord>( dataRecordsColl.SparseArray ) )
		{
			_dataRecordsColl = dataRecordsColl;

			this.Initialize( CreateDataRecordCallback, InitDataRecordCallback, GetDataObjectFromDataRecord );
		}

		private DataRecord CreateDataRecordCallback( object dataItem )
		{
			Debug.Assert( null != dataItem );
			return DataRecord.Create( _dataRecordsColl, dataItem, false, -1 );
		}

		private object GetDataObjectFromDataRecord( DataRecord dataRecord )
		{
			return dataRecord.DataItem;
		}

		private void InitDataRecordCallback( DataRecord dataRecord )
		{
			dataRecord.FireInitializeRecord( );
		}

		protected override void RaiseDestCollectionEvent( NotifyCollectionChangedEventArgs args )
		{
			_dataRecordsColl.RaiseChangeEvents( true );
		}
	}

	internal class ListSynchronizer<TSource, TDest>
	{
		internal readonly IList<TSource> _source;
		internal readonly IList<TDest> _dest;
		private Func<TSource, TDest> _createItem;
		private Action<TDest> _initializeItem;
		private Func<TDest, TSource> _getSourceFromDest;

		public ListSynchronizer( IList<TSource> source, IList<TDest> dest )
		{
			_source = source;
			_dest = dest;
		}

		internal void Initialize( 
			Func<TSource, TDest> createItemDelegate,
			Action<TDest> initializeItemDelegate,
			Func<TDest, TSource> getSourceFromDest )
		{
			_createItem = createItemDelegate;
			_initializeItem = initializeItemDelegate;
			_getSourceFromDest = getSourceFromDest;

			this.HookUnhookSource( true );
		}

		private void HookUnhookSource( bool hook )
		{
			INotifyCollectionChanged ncSource = _source as INotifyCollectionChanged;
			if ( null != ncSource )
			{
				NotifyCollectionChangedEventHandler handler = new NotifyCollectionChangedEventHandler( OnSource_CollectionChanged );

				if ( hook )
					ncSource.CollectionChanged += handler;
				else
					ncSource.CollectionChanged -= handler;
			}
		}

		internal void Dispose( )
		{
			this.HookUnhookSource( false );
		}

		/// <summary>
		/// Synchronizes the destination list with the source list.
		/// </summary>
		internal void Synchronize( )
		{
			Dictionary<TSource, TDest> existingItems = new Dictionary<TSource,TDest>( );

			foreach ( TDest ii in _dest )
			{
				TSource source = _getSourceFromDest( ii );
				if ( null != source )
					existingItems[source] = ii;
			}

			_dest.Clear( );

			this.AddHelper( 0, _source, existingItems );

			this.RaiseDestCollectionEvent( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		/// <summary>
		/// Creates TDest item for the corresponding source item and initializes it.
		/// </summary>
		/// <param name="sourceItem"></param>
		/// <returns></returns>
		private TDest CreateAndInitialize( TSource sourceItem )
		{
			TDest newDest = _createItem( sourceItem );
			_initializeItem( newDest );

			return newDest;
		}

		private static void RemoveRange<T>( IList<T> list, int index, int count )
		{
			for ( int i = 0; i < count; i++ )
				list.RemoveAt( index + i );
		}

		private static void InsertRange<T>( IList<T> list, int index, IEnumerable<T> itemsToAdd )
		{
			foreach ( T ii in itemsToAdd )
				list.Insert( index++, ii );
		}

		private void AddHelper( int index, IEnumerable addedSourceItems, Dictionary<TSource, TDest> itemsToRecycle )
		{
			foreach ( TSource ii in addedSourceItems )
			{
				TDest newDest;
				if ( null == itemsToRecycle || ! itemsToRecycle.TryGetValue( ii, out newDest ) )
					newDest = this.CreateAndInitialize( ii );

				_dest.Insert( index++, newDest );
			}
		}

		private void OnSource_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			bool processAsReset = false;

			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					{
						this.AddHelper( e.NewStartingIndex, e.NewItems, null );
					}
					break;
				case NotifyCollectionChangedAction.Move:
					{
						List<TDest> tmpItems = new List<TDest>( );
						int movedItemCount = e.NewItems.Count;

						for ( int i = 0; i < movedItemCount; i++ )
							tmpItems.Add( _dest[e.OldStartingIndex + i] );

						RemoveRange( _dest, e.OldStartingIndex, movedItemCount );
						InsertRange( _dest, e.NewStartingIndex, tmpItems );
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						int oldStartingIndex = e.OldStartingIndex;
						int oldItemsCount = e.OldItems.Count;

						RemoveRange( _dest, e.OldStartingIndex, e.OldItems.Count );
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						// Apparently OldStartingIndex is always -1 in silverlight and NewStartingIndex is the 
						// correct index. Also in WPF, there are overloads of NotifyCollectionChangedEventArgs
						// without indexes which leave both the OldStartingIndex and NewStartingIndex to -1.
						// 
						int index = e.NewStartingIndex;
						if ( index >= 0 )
						{
							int count = e.NewItems.Count;
							RemoveRange( _dest, index, count );

							this.AddHelper( index, e.NewItems, null );
						}
						else
							processAsReset = true;
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					processAsReset = true;
					break;
				default:
					Debug.Assert( false );
					break;
			}

			if ( processAsReset )
			{
				this.Synchronize( );
			}
		}

		protected virtual void RaiseDestCollectionEvent( NotifyCollectionChangedEventArgs args )
		{
		}
	}

	internal class VUtils
	{
		internal static void CreateGroupByRecordsSynchronizerHelper( RecordCollectionBase groupByRecordsColl, IList<object> groupObjects )
		{
			if ( null == groupByRecordsColl._groupsSynchronizer
				|| groupObjects != groupByRecordsColl._groupsSynchronizer._source
				|| groupByRecordsColl.GroupByField != groupByRecordsColl._groupsSynchronizer._groupByField )
			{
				groupByRecordsColl.InitializeGroupByField( );

				groupByRecordsColl._groupsSynchronizer = new GroupsSynchronizer( groupByRecordsColl, groupObjects );
				groupByRecordsColl._groupsSynchronizer.Synchronize( );
			}
		}

		internal static void CreateDataRecordsSynchronizerHelper( RecordCollectionBase dataRecords, IList<object> dataObjects )
		{
			if ( null == dataRecords._recordsSynchronizer || dataRecords != dataRecords._recordsSynchronizer._source )
			{
				dataRecords._recordsSynchronizer = new DataRecordsSynchronizer( dataRecords, dataObjects );
				dataRecords._recordsSynchronizer.Synchronize( );
			}
		}

		internal static ICollectionView GetCollectionView( RecordManager rm )
		{
			return rm.DataSource as ICollectionView;
		}

		internal static IEnumerable GetCollectionViewSourceItems( RecordManager rm )
		{
			ICollectionView view = GetCollectionView( rm );
			return null != view ? view.SourceCollection : null;
		}


		internal static string GetPropertyName( FieldSortDescription fsd )
		{
			return fsd.FieldName;
		}

		internal static string GetPropertyName( Field field )
		{
			return field.Name;
		}

		internal static Field GetField( FieldLayout fieldLayout, string propertyName )
		{
			if ( !string.IsNullOrEmpty( propertyName ) )
			{
				foreach ( Field ii in fieldLayout.Fields )
				{
					if ( GridUtilities.AreKeysEqual( GetPropertyName( ii ), propertyName ) )
						return ii;
				}
			}

			return null;
		}


		internal static LinqQueryManager CreateLinqQueryManager( RecordManager rm, bool useSourceCollection )
		{
			ICollectionView view = VUtils.GetCollectionView( rm );
			if ( null != view )
			{
				Type elemType = LinqQueryManager.GetListElementType( view, true );
				if ( null != elemType )
					return new LinqQueryManager( ( useSourceCollection ? view.SourceCollection : null ) ?? view, elemType );
			}
			else
			{
				IEnumerable dataSource = rm.DataSource;
				Type elemType = null != dataSource ? LinqQueryManager.GetListElementType( dataSource, true ) : null;
				if ( null != elemType )
					return new LinqQueryManager( dataSource, elemType );
			}

			return null;
		}


	}


	internal class ScheduleUtilities
	{
		#region AddEntryHelper<TKey, TValue>

		internal static Dictionary<TKey, TValue> AddEntryHelper<TKey, TValue>( Dictionary<TKey, TValue> map, TKey key, TValue value )
		{
			if ( null != value )
			{
				if ( null == map )
					map = new Dictionary<TKey, TValue>( );

				map[key] = value;
			}

			return map;
		}

		#endregion // AddEntryHelper<TKey, TValue>

		#region GetString

		internal static string GetString( string name )
		{
#pragma warning disable 436
			return SR.GetString( name );
#pragma warning restore 436
		}

		internal static string GetString( string name, params object[] args )
		{
#pragma warning disable 436
			return SR.GetString( name, args );
#pragma warning restore 436
		}

		#endregion //GetString	

		#region GetNonNullValues

		internal static T[] GetNonNullValues<T>( params T[] values ) where T : class
		{
			List<T> list = new List<T>( );

			for ( int i = 0; i < values.Length; i++ )
			{
				T ii = values[i];
				if ( null != ii )
					list.Add( ii );
			}

			return list.ToArray( );
		}

		#endregion // GetNonNullValues
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