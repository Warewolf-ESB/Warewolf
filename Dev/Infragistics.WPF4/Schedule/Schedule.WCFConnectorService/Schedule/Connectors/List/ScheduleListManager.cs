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
using System.Linq;


using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services






{
	#region ItemCollectionListManager Class

	internal abstract class ItemCollectionListManager<TViewItem, TMappingKey> : PropertyStorageListManager<TViewItem, TMappingKey>
		where TViewItem : class
	{
		#region Member Vars

		protected IFieldValueAccessor _idFieldValueAccessor;
		protected ViewList<TViewItem> _items;
		protected IViewItemFactory<TViewItem> _viewItemFactory;
		protected int _viewListVerificationSuspendedCount;
		protected DeferredOperation _asyncDirtyViewListOperation;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ItemCollectionListManager( StoragePropsInfo propsInfo, Converter<TViewItem, object> viewItemToDataItemConverter )
			: base( propsInfo, viewItemToDataItemConverter )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region ViewItemFactory

		internal override IViewItemFactory<TViewItem> ViewItemFactory
		{
			get
			{
				if ( null == _viewItemFactory )
				{
					this.InitIdFieldValueAccessor( );
					_viewItemFactory = this.CreateViewItemFactory( _idFieldValueAccessor );
				}

				return _viewItemFactory;
			}
		}

		#endregion // ViewItemFactory

		#endregion // Properties

		#region Methods

		#region MarkCleanPendingDirtyResults

		internal override void MarkCleanPendingDirtyResults( )
		{
			base.MarkCleanPendingDirtyResults( );

			if ( null != _asyncDirtyViewListOperation )
				_asyncDirtyViewListOperation.InvokePendingOperation( );
		}

		#endregion // MarkCleanPendingDirtyResults

		#region RecreateAllViewItems

		/// <summary>
		/// Discards all the existing view items and dirties all the query results so the view items get re-created.
		/// </summary>
		public override void RecreateAllViewItems( )
		{
			base.RecreateAllViewItems( );

			this.DirtyItemsCollection( true );
		}

		#endregion // RecreateAllViewItems

		#endregion // Methods

		#endregion // Base Overrides

		#region Items

		internal ViewList<TViewItem> Items
		{
			get
			{
				if ( null == _items )
					this.CreateItemsCollection( ref _items );

				return _items;
			}
		}

		#endregion // Items

		#region Methods

		#region Protected Methods

		#region CreateItemsCollection

		protected abstract void CreateItemsCollection( ref ViewList<TViewItem> collection );

		#endregion // CreateItemsCollection

		#region CreateViewItemFactory

		protected abstract IViewItemFactory<TViewItem> CreateViewItemFactory( IFieldValueAccessor idField );

		#endregion // CreateViewItemFactory

		#region InitIdFieldValueAccessor

		protected abstract void InitIdFieldValueAccessor( bool clearWrappedFva = false );

		#endregion // InitIdFieldValueAccessor

		#region OnPreVerifyViewList

		protected IEnumerable OnPreVerifyViewList( ViewList<TViewItem> viewList )
		{
			if ( _viewListVerificationSuspendedCount > 0 )
			{
				// SSP 4/16/11 TFS63818
				// Moved this here from into OnPreVerifyViewList method.
				// 
				if ( null == _asyncDirtyViewListOperation )
					_asyncDirtyViewListOperation = WeakAction<ItemCollectionListManager<TViewItem, TMappingKey>>.ExecuteAsync( this, DirtyItemsCollectionAsyncHandler );

				return null;
			}

			this.InitIdFieldValueAccessor( );
			return this.List;
		}

		#endregion // OnPreVerifyViewList

		#endregion // Protected Methods

		#region Internal Methods

		#region DirtyItemsCollection

		internal void DirtyItemsCollection( bool discardOldItems )
		{
			// We need to discard existing Resource objects and re-populate the Resources collection
			// because if mappings are changed or the list is changed, we can't reuse the existing
			// Resource items unless we send out PropertyChanged notifications on individual 
			// Resource objects for the properties whose mappings have changed.
			// 
			if ( discardOldItems )
			{
				var itemManager = _viewItemFactory as IViewItemManager<TViewItem>;
				if ( null != itemManager )
					itemManager.DiscardCachedItems( );
			}

			if ( null != _items )
			{
				// As the view list raises reset notification, if the listener tries to synchronously
				// access the list then don't verify it and populate it with items since we could in
				// the middle of populating field mapping collection and threfore everytime a mapping
				// is added, we don't want to re-populate the view list. That's what the 
				// _viewListVerificationSuspendedCount is for and which look at it in the 
				// OnPreVerifyViewList to return null for the source of the view list which will cause
				// the list be empty. To re-dirty it we are enquing an async operation below to make
				// sure that the list eventually reflects the correct contents.
				// 
				_viewListVerificationSuspendedCount++;
				try
				{
					_items.Dirty( discardOldItems );
				}
				finally
				{
					_viewListVerificationSuspendedCount--;

					// if _items.IsDirty is false then someone accessed it and caused it to be verified in
					// above Dirty call (which sends a reset notification) and threfore re-dirty it because
					// we return 0 items via OnPreVerifyViewList while _viewListVerificationSuspendedCount 
					// is non-zero.
					// 
					// SSP 4/16/11 TFS63818
					// Moved this into OnPreVerifyViewList method.
					// 
					//if ( 0 == _viewListVerificationSuspendedCount && !_items.IsDirty && null == _asyncDirtyViewListOperation )
					//	_asyncDirtyViewListOperation = WeakAction<ItemCollectionListManager<TViewItem, TMappingKey>>.ExecuteAsync( this, DirtyItemsCollectionAsyncHandler );
				}
			}
		}

		#endregion // DirtyItemsCollection

		#endregion // Internal Methods

		#region Private Methods

		#region DirtyItemsCollectionAsyncHandler

		private static void DirtyItemsCollectionAsyncHandler( ItemCollectionListManager<TViewItem, TMappingKey> lm )
		{
			if ( null != lm._asyncDirtyViewListOperation )
			{
				lm._asyncDirtyViewListOperation.CancelPendingOperation( );
				lm._asyncDirtyViewListOperation = null;
			}

			if ( null != lm._items )
				lm._items.Dirty( false );
		}

		#endregion // DirtyItemsCollectionAsyncHandler

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ItemCollectionListManager Class

	#region ScheduleItemCollectionListManager Class

	internal abstract class ScheduleItemCollectionListManager<TViewItem, TMappingKey> : ItemCollectionListManager<TViewItem, TMappingKey>
		where TViewItem : class
	{
		#region Member Vars

		protected readonly IScheduleDataConnector _connector;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ScheduleItemCollectionListManager( IScheduleDataConnector connector, StoragePropsInfo propsInfo, Converter<TViewItem, object> viewItemToDataItemConverter )
			: base( propsInfo, viewItemToDataItemConverter )
		{
			CoreUtilities.ValidateNotNull( connector );
			_connector = connector;
		}

		#endregion // Constructor

		#region Base Overrides

		#region Methods

		#region OnError

		protected override void OnError( DataErrorInfo error )
		{
			ScheduleUtilities.RaiseErrorHelper( _connector, error );
		}

		#endregion // OnError 

		#endregion // Methods

		#endregion // Base Overrides
	}

	#endregion // ScheduleItemCollectionListManager Class

	//#region ScheduleItemCollectionListManager Class

	//internal abstract class ScheduleItemCollectionListManager<TViewItem, TMappingKey> : ScheduleListManager<TViewItem, TMappingKey>
	//    where TViewItem: class
	//{
	//    #region Member Vars

	//    protected IFieldValueAccessor _idFieldValueAccessor;
	//    protected ViewList<TViewItem> _items;
	//    protected IViewItemFactory<TViewItem> _viewItemFactory;
	//    protected int _viewListVerificationSuspendedCount;
	//    protected DeferredOperation _asyncDirtyViewListOperation;

	//    #endregion // Member Vars

	//    #region Constructor

	//    /// <summary>
	//    /// Constructor.
	//    /// </summary>
	//    internal ScheduleItemCollectionListManager( IScheduleDataConnector connector, StoragePropsInfo propsInfo, Converter<TViewItem, object> viewItemToDataItemConverter )
	//        : base( connector, propsInfo, viewItemToDataItemConverter )
	//    {
	//    } 

	//    #endregion // Constructor

	//    #region Base Overrides

	//    #region Properties

	//    #region ViewItemFactory

	//    internal override IViewItemFactory<TViewItem> ViewItemFactory
	//    {
	//        get
	//        {
	//            if ( null == _viewItemFactory )
	//            {
	//                this.InitIdFieldValueAccessor( );
	//                _viewItemFactory = this.CreateViewItemFactory( _idFieldValueAccessor );
	//            }

	//            return _viewItemFactory;
	//        }
	//    }

	//    #endregion // ViewItemFactory 

	//    #endregion // Properties

	//    #region Methods

	//    #region MarkCleanPendingDirtyResults

	//    internal override void MarkCleanPendingDirtyResults( )
	//    {
	//        base.MarkCleanPendingDirtyResults( );

	//        if ( null != _asyncDirtyViewListOperation )
	//            _asyncDirtyViewListOperation.InvokePendingOperation( );
	//    }

	//    #endregion // MarkCleanPendingDirtyResults

	//    #region RecreateAllViewItems

	//    /// <summary>
	//    /// Discards all the existing view items and dirties all the query results so the view items get re-created.
	//    /// </summary>
	//    public override void RecreateAllViewItems( )
	//    {
	//        base.RecreateAllViewItems( );

	//        this.DirtyItemsCollection( true );
	//    }

	//    #endregion // RecreateAllViewItems

	//    #endregion // Methods

	//    #endregion // Base Overrides

	//    #region Items

	//    internal ViewList<TViewItem> Items
	//    {
	//        get
	//        {
	//            if ( null == _items )
	//                this.CreateItemsCollection( ref _items );

	//            return _items;
	//        }
	//    }

	//    #endregion // Items

	//    #region Methods

	//    #region Protected Methods

	//    #region CreateItemsCollection

	//    protected abstract void CreateItemsCollection( ref ViewList<TViewItem> collection );

	//    #endregion // CreateItemsCollection

	//    #region CreateViewItemFactory

	//    protected abstract IViewItemFactory<TViewItem> CreateViewItemFactory( IFieldValueAccessor idField );

	//    #endregion // CreateViewItemFactory

	//    #region InitIdFieldValueAccessor

	//    protected abstract void InitIdFieldValueAccessor( bool clearWrappedFva = false );

	//    #endregion // InitIdFieldValueAccessor

	//    #region OnPreVerifyViewList

	//    protected IEnumerable OnPreVerifyViewList( ViewList<TViewItem> viewList )
	//    {
	//        if ( _viewListVerificationSuspendedCount > 0 )
	//        {
	//            // SSP 4/16/11 TFS63818
	//            // Moved this here from into OnPreVerifyViewList method.
	//            // 
	//            if ( null == _asyncDirtyViewListOperation )
	//                _asyncDirtyViewListOperation = WeakAction<ScheduleItemCollectionListManager<TViewItem, TMappingKey>>.ExecuteAsync( this, DirtyItemsCollectionAsyncHandler );
				
	//            return null;
	//        }

	//        this.InitIdFieldValueAccessor( );
	//        return this.List;
	//    }

	//    #endregion // OnPreVerifyViewList

	//    #endregion // Protected Methods 

	//    #region Internal Methods

	//    #region DirtyItemsCollection

	//    internal void DirtyItemsCollection( bool discardOldItems )
	//    {
	//        // We need to discard existing Resource objects and re-populate the Resources collection
	//        // because if mappings are changed or the list is changed, we can't reuse the existing
	//        // Resource items unless we send out PropertyChanged notifications on individual 
	//        // Resource objects for the properties whose mappings have changed.
	//        // 
	//        if ( discardOldItems )
	//        {
	//            var itemManager = _viewItemFactory as IViewItemManager<TViewItem>;
	//            if ( null != itemManager )
	//                itemManager.DiscardCachedItems( );
	//        }

	//        if ( null != _items )
	//        {
	//            // As the view list raises reset notification, if the listener tries to synchronously
	//            // access the list then don't verify it and populate it with items since we could in
	//            // the middle of populating field mapping collection and threfore everytime a mapping
	//            // is added, we don't want to re-populate the view list. That's what the 
	//            // _viewListVerificationSuspendedCount is for and which look at it in the 
	//            // OnPreVerifyViewList to return null for the source of the view list which will cause
	//            // the list be empty. To re-dirty it we are enquing an async operation below to make
	//            // sure that the list eventually reflects the correct contents.
	//            // 
	//            _viewListVerificationSuspendedCount++;
	//            try
	//            {
	//                _items.Dirty( discardOldItems );
	//            }
	//            finally
	//            {
	//                _viewListVerificationSuspendedCount--;

	//                // if _items.IsDirty is false then someone accessed it and caused it to be verified in
	//                // above Dirty call (which sends a reset notification) and threfore re-dirty it because
	//                // we return 0 items via OnPreVerifyViewList while _viewListVerificationSuspendedCount 
	//                // is non-zero.
	//                // 
	//                // SSP 4/16/11 TFS63818
	//                // Moved this into OnPreVerifyViewList method.
	//                // 
	//                //if ( 0 == _viewListVerificationSuspendedCount && !_items.IsDirty && null == _asyncDirtyViewListOperation )
	//                //	_asyncDirtyViewListOperation = WeakAction<ScheduleItemCollectionListManager<TViewItem, TMappingKey>>.ExecuteAsync( this, DirtyItemsCollectionAsyncHandler );
	//            }
	//        }
	//    }

	//    #endregion // DirtyItemsCollection

	//    #endregion // Internal Methods

	//    #region Private Methods

	//    #region DirtyItemsCollectionAsyncHandler

	//    private static void DirtyItemsCollectionAsyncHandler( ScheduleItemCollectionListManager<TViewItem, TMappingKey> lm )
	//    {
	//        if ( null != lm._asyncDirtyViewListOperation )
	//        {
	//            lm._asyncDirtyViewListOperation.CancelPendingOperation( );
	//            lm._asyncDirtyViewListOperation = null;
	//        }

	//        if ( null != lm._items )
	//            lm._items.Dirty( false );
	//    }

	//    #endregion // DirtyItemsCollectionAsyncHandler

	//    #endregion // Private Methods

	//    #endregion // Methods
	//} 

	//#endregion // ScheduleItemCollectionListManager Class

	#region ResourceListManager Class

	internal class ResourceListManager : ScheduleItemCollectionListManager<Resource, ResourceProperty>
	{
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ResourceListManager( IScheduleDataConnector connector )
			: base( connector, Resource.StorageProps.Info.Instance,
				delegate( Resource resource )
				{
					return resource.DataItem;
				}
			)
		{
			this.InitializeUnmappedPropertiesMappingKey( ResourceProperty.UnmappedProperties );
		} 

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region MinimumRequiredFields

		internal override ResourceProperty[] MinimumRequiredFields
		{
			get
			{
				return new ResourceProperty[]
				{
					ResourceProperty.Id
				};
			}
		}

		#endregion // MinimumRequiredFields 

		#endregion // Properties

		#region Methods

		#region CreateItemsCollection

		protected override void CreateItemsCollection( ref ViewList<Resource> collection )
		{
			collection = new ResourceCollection( this.List, this.ViewItemFactory, this.OnPreVerifyViewList );
			this.RefreshCalendarsStore( );
		}

		#endregion // CreateItemsCollection

		#region CreateViewItemFactory

		protected override IViewItemFactory<Resource> CreateViewItemFactory( IFieldValueAccessor idField )
		{
			return new ResourceItemManager( this.PropertyStorage, idField, this.ListEventListener );
		}

		#endregion // CreateViewItemFactory

		#region InitIdFieldValueAccessor

		protected override void InitIdFieldValueAccessor( bool clearWrappedFva = false )
		{
			FieldValueAccessorWrapper fva = (FieldValueAccessorWrapper)_idFieldValueAccessor;
			if ( null == fva )
				_idFieldValueAccessor = fva = new FieldValueAccessorWrapper( );

			fva._fva = clearWrappedFva ? null : this.GetFieldValueAccessor( ResourceProperty.Id );
		}

		#endregion // InitIdFieldValueAccessor

		#region RecreateAllViewItems

		/// <summary>
		/// Discards all the existing view items and dirties all the query results so the view items get re-created.
		/// </summary>
		public override void RecreateAllViewItems( )
		{
			base.RecreateAllViewItems( );

			this.RecreateAllActivities( );
		}

		#endregion // RecreateAllViewItems

		#region VerifyFieldValueAccessors

		protected override void VerifyFieldValueAccessors( )
		{
			base.VerifyFieldValueAccessors( );

			if ( null != _items )
				this.RefreshCalendarsStore( );
		}

		#endregion // VerifyFieldValueAccessors  

		#endregion // Methods

		#endregion // Base Overrides

		#region Properties

		#region Internal Properties

		#region HasCalendarsDataSource

		internal bool HasCalendarsDataSource
		{
			get
			{
				





				return false;

			}
		}

		#endregion // HasCalendarsDataSource

		#region Resources

		internal ResourceCollection Resources
		{
			get
			{
				return (ResourceCollection)this.Items;
			}
		}

		#endregion // Resources

		#endregion // Internal Properties 

		#endregion // Properties

		#region Methods

		#region Private Methods

		#region CreateRelatedCalendarsStore

		private RelationPropertyStore<Resource, ResourceCalendar, ObservableCollectionExtended<ResourceCalendar>> CreateRelatedCalendarsStore( ResourceCollection resources, IMap<int, IPropertyStore> stores )
		{
			


#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)


			return null;
		}

		#endregion // CreateRelatedCalendarsStore

		#region RecreateAllActivities

		private void RecreateAllActivities( )
		{


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		}

		#endregion // RecreateAllActivities

		#region RefreshCalendarsStore

		private void RefreshCalendarsStore( ResourceCollection resourceCollection = null )
		{
			if ( null == resourceCollection )
				resourceCollection = this.Resources;

			PropertyStorage<Resource, int> storage = (PropertyStorage<Resource, int>)_propertyStorage;
			IMap<int, IPropertyStore> stores = null != storage ? storage.PropertyStores : null;
			if ( null != stores )
			{
				var psw = stores[Resource.StorageProps.Calendars] as PropertyStoreWrapper<ObservableCollectionExtended<ResourceCalendar>>;
				Debug.Assert( null != psw );
				if ( null != psw )
				{
					// Create a relation store that finds the matching calendars associated with each resource in the 
					// calendars data source and returns the collection of the matching calendars that belong to 
					// that resource.
					// 
					var calendarsStore = this.CreateRelatedCalendarsStore( resourceCollection, stores );
					if ( null != calendarsStore || psw.InternalStore is RelationPropertyStore<Resource, ResourceCalendar, ObservableCollectionExtended<ResourceCalendar>> )
						psw.ReInitializeInternalStore( calendarsStore );

					// If calendars data source is not specified then don't set the PrimaryCalendarId field to auto-created
					// calendar's id. Existence of a PrimaryCalendarId mapping and non-existence of a calendars data source
					// could be result of a timing issue where the calendars data source is going to be provided later on,
					// and if so the resource is going to auto-create primary calendar however that calendar's id should
					// not be set on the underlying resource data item since when they do set the calendars data source, 
					// that id will not correspond to any calendar in the calendars data source.
					// 
					var primaryCalendarIdStore = stores[Resource.StorageProps.PrimaryCalendarId] as PropertyStoreWrapper<string>;
					Debug.Assert( null != primaryCalendarIdStore );
					if ( null != primaryCalendarIdStore )
					{
						bool dontUsePrimaryCalendarIdField = null == calendarsStore;
						primaryCalendarIdStore.VerifyFieldAccessor( dontUsePrimaryCalendarIdField );
					}
				}
			}
		}

		#endregion // RefreshCalendarsStore  

		#endregion // Private Methods

		#region Internal Methods

		#region IsFeatureSupported

		internal bool IsFeatureSupported( ResourceFeature resourceFeature )
		{
			switch ( resourceFeature )
			{
				case ResourceFeature.CustomActivityCategories:
					if ( this.HasMappedField( ResourceProperty.CustomActivityCategories, false ) )
						return true;

					break;
			}

			return false;
		}

		#endregion // IsFeatureSupported 

		#endregion // Internal Methods

		#endregion // Methods
	}

	#endregion // ResourceListManager Class

    #region ResourceCalendarListManager Class

	internal class ResourceCalendarListManager : ScheduleItemCollectionListManager<ResourceCalendar, ResourceCalendarProperty>
    {
		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ResourceCalendarListManager( IScheduleDataConnector connector )
			: base( connector, ResourceCalendar.StorageProps.Info.Instance,
				delegate( ResourceCalendar calendar )
				{
					return calendar.DataItem;
				}
			)
		{
			this.InitializeUnmappedPropertiesMappingKey( ResourceCalendarProperty.UnmappedProperties );
		} 

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region MinimumRequiredFields

		internal override ResourceCalendarProperty[] MinimumRequiredFields
		{
			get
			{
				return new ResourceCalendarProperty[]
				{
					ResourceCalendarProperty.Id,
					ResourceCalendarProperty.OwningResourceId
				};
			}
		}

		#endregion // MinimumRequiredFields

		#endregion // Properties

		#region Methods

		#region CreateItemsCollection

		protected override void CreateItemsCollection( ref ViewList<ResourceCalendar> collection )
		{
			collection = new ResourceCalendarCollection( this.List, this.ViewItemFactory, this.OnPreVerifyViewList );
		}

		#endregion // CreateItemsCollection

		#region CreateViewItemFactory

		protected override IViewItemFactory<ResourceCalendar> CreateViewItemFactory( IFieldValueAccessor idField )
		{
			return new ResourceCalendarItemManager( this.PropertyStorage, idField, this.ListEventListener );
		}

		#endregion // CreateViewItemFactory

		#region DirtyFieldValueAccessors

		internal override void DirtyFieldValueAccessors( )
		{
			base.DirtyFieldValueAccessors( );

			// When resource calendars data source is specified or when the Id or the 
			// OwningResourceId a field mappings change, re-create the resource collection
			// so the resources' Calendars collections return the right calendars based
			// on the calendars data source.
			// 





		}

		#endregion // DirtyFieldValueAccessors

		#region InitIdFieldValueAccessor

		protected override void InitIdFieldValueAccessor( bool clearWrappedFva = false )
		{
			var aggregateFva = (AggregateTupleFieldValueAccessorWrapper<object, object>)_idFieldValueAccessor;
			if ( null == aggregateFva )
			{
				aggregateFva = new AggregateTupleFieldValueAccessorWrapper<object, object>( );
				aggregateFva._fva1 = new FieldValueAccessorWrapper( );
				aggregateFva._fva2 = new FieldValueAccessorWrapper( );
				_idFieldValueAccessor = aggregateFva;
			}

			( (FieldValueAccessorWrapper)aggregateFva._fva1 )._fva = clearWrappedFva ? null : this.GetFieldValueAccessor( ResourceCalendarProperty.Id );
			( (FieldValueAccessorWrapper)aggregateFva._fva2 )._fva = clearWrappedFva ? null : this.GetFieldValueAccessor( ResourceCalendarProperty.OwningResourceId );
		}

		#endregion // InitIdFieldValueAccessor

		#endregion // Methods 

		#endregion // Base Overrides

		#region Properties

		#region Internal Properties

		#region ResourceCalendars

		internal ResourceCalendarCollection ResourceCalendars
		{
			get
			{
				return (ResourceCalendarCollection)this.Items;
			}
		}

		#endregion // ResourceCalendars  

		#endregion // Internal Properties

		#endregion // Properties
    }

    #endregion // ResourceCalendarListManager Class

	#region IActivityListManager Interface

	internal interface IActivityListManager
	{
		bool IsActivityFeatureSupported( ActivityFeature activityFeature );

		void RecreateAllViewItems( );
	} 

	#endregion // IActivityListManager Interface

    #region ActivityListManager Class

	internal abstract class ActivityListManager<TViewItem, MappingKey> : PropertyStorageListManager<ActivityBase, MappingKey>, IActivityListManager
		where TViewItem : ActivityBase, new( )
	{
		#region Member Vars

		protected readonly IScheduleDataConnector _connector;

		private bool _isRecurringListManager;
		private ActivityItemManager<TViewItem> _activityItemManager;
		/// <summary>
		/// Mapping of ActivityProperty enum values to their corresponding MappingKey enum values.
		/// </summary>
		private IMap<ActivityProperty, MappingKey> _mappedKeys;
		private IMap<ActivityProperty, ActivityProperty> _mappedKeysForActivityQuery;
		/// <summary>
		/// This is set to the recurring list manager on a non-recurring list manager and the other way around as well.
		/// </summary>
		private ActivityListManager<TViewItem, MappingKey> _complementaryListManager;
		private bool _hasRecurringActivities;
		private bool _hasVarianceActivities;
		private VerifyAsyncItemsQueue<object> _reevalActivitiesQueue;
		private OccurrenceManager<TViewItem> _occurrencesManager;
		private ValueChangeListener<ActivityListManager<TViewItem, MappingKey>, ActivityBase> _propertyStoreListener;

		#endregion // Member Vars

		#region Constructor

		internal ActivityListManager( IScheduleDataConnector connector, StoragePropsInfo propsInfo )
            : base( propsInfo, GetActivityDataItem )
		{
			CoreUtilities.ValidateNotNull( connector );
			_connector = connector;
			_reevalActivitiesQueue = new VerifyAsyncItemsQueue<object>( ii => this.VerifyItemsInCleanResults( ii, null ), true );
            this.InitializeMappedKeys( );
		}

		#endregion // Constructor

		#region Base Overrides

		#region Methods

		#region CreateLinqStatement

		/// <summary>
		/// This method is called to create linq condition from a list specific query result, like ActivityQuery for the
		/// activity list manager.
		/// </summary>
		/// <param name="result">Linq statement for the query associated with the specified result object will be returned.</param>
		/// <param name="error">This out param will be set if an error occurs.</param>
		/// <returns>Returns linq condition derived from the specified listSpecificQueryInfo object.</returns>
		protected override LinqQueryManager.ILinqStatement CreateLinqStatement( ListQueryResult result, out DataErrorInfo error )
		{
			error = null;
			ActivityQueryComponent qc = result.ExternalQueryInfo as ActivityQueryComponent;
			Debug.Assert( null != qc );

			bool isReminderQuery = qc._isReminderQuery;
			bool isRecurrenceQuery = qc._isRecurrenceQuery;
			ActivityQuery query = qc._query;
			IEnumerable<ResourceCalendar> calendars = qc._query.Calendars;
			IEnumerable<DateRange> dateRanges = qc._query.DateRanges;

			if (
				// If querying for recurring activities and the list doesn't have recurring activities then return.
				// 
				isRecurrenceQuery && ! _hasRecurringActivities
				// If querying for non-recurring activities and this list manager is a recurring list manager then
				// return since the recurring list manager only contains recurring activities.
				// 
				|| ! isRecurrenceQuery && _isRecurringListManager )
				return null;

			List<DataErrorInfo> errorList = new List<DataErrorInfo>( );

			// Create condition for the calendars.
			// 
			LinqQueryManager.ILinqCondition calendarCondition = this.CreateCalendarsCondition( calendars, errorList );

			LinqQueryManager.ILinqCondition dateCondition = null;
			LinqQueryManager.ILinqStatement orderByStatement = null;

			bool? nextFlag = null;

			switch ( qc._singleFlag )
			{
				case ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges:
					{
						if ( isRecurrenceQuery )
						{
							LinqQueryManager.ILinqCondition c1 = LinqQueryManager.AndHelper(
								this.CreateConditionQuery( ActivityProperty.Recurrence, LinqQueryManager.LinqOperator.NotEqual, null, errorList ),
								this.CreateDateRangesQuery( qc, "recurring", dateRanges, errorList )
							);

							LinqQueryManager.ILinqCondition c2 = ! _hasVarianceActivities ? null : LinqQueryManager.AndHelper(
								this.CreateConditionQuery( ActivityProperty.RootActivityId, LinqQueryManager.LinqOperator.NotEqual, null, errorList ),
								this.CreateDateRangesQuery( qc, "variances", dateRanges, errorList )
							);

							dateCondition = LinqQueryManager.OrHelper( c1, c2 );
						}
						else
						{
							dateCondition = LinqQueryManager.AndHelper(
								_hasRecurringActivities ? this.CreateConditionQuery( ActivityProperty.Recurrence, LinqQueryManager.LinqOperator.Equal, null, errorList ) : null,
								_hasVarianceActivities ? this.CreateConditionQuery( ActivityProperty.RootActivityId, LinqQueryManager.LinqOperator.Equal, null, errorList ) : null,
								this.CreateDateRangesQuery( qc, null, dateRanges, errorList )
							);
						}
					}
					break;
				case ActivityQueryRequestedDataFlags.HasNextActivity:
				case ActivityQueryRequestedDataFlags.NextActivity:
					nextFlag = true;
					DateTime? maxDate = null != dateRanges ? dateRanges.Max( ii => ii.End ) : default( DateTime? );

					dateCondition = maxDate.HasValue
						? this.CreateConditionQuery( ActivityProperty.Start, LinqQueryManager.LinqOperator.GreaterThanOrEqual, maxDate, errorList )
						: null;

					break;
				case ActivityQueryRequestedDataFlags.HasPreviousActivity:
				case ActivityQueryRequestedDataFlags.PreviousActivity:
					nextFlag = false;
					DateTime? minDate = null != dateRanges ? dateRanges.Min( ii => ii.Start ) : default( DateTime? );

					dateCondition = minDate.HasValue
						? this.CreateConditionQuery( ActivityProperty.End, LinqQueryManager.LinqOperator.LessThanOrEqual, minDate, errorList )
						: null;

					break;
				default:
					error = ScheduleUtilities.CreateErrorFromId(query, "LE_UnknownQueryType", typeof(TViewItem).Name );//"Unknown type of query."
					return null;
			}


			LinqQueryManager.ILinqCondition filterCondition = LinqQueryManager.AndHelper( calendarCondition, dateCondition );

			// If this is a reminder query, add reminder related conditions.
			// 
			if ( isReminderQuery && this.HasMappedField( ActivityProperty.ReminderEnabled, true ) )
			{
				filterCondition = LinqQueryManager.AndHelper(
					CreateConditionQuery( ActivityProperty.ReminderEnabled, LinqQueryManager.LinqOperator.Equal, true, errorList ),
					filterCondition
				);
			}

			LinqQueryManager.ILinqStatement returnValue = filterCondition;

			// If the query is one of HasNext/Prev or Next/Prev.
			// 
			if ( nextFlag.HasValue )
			{
				string dateField;
				DataErrorInfo tmpError;
				if ( this.GetMappedField( this.GetActualActivityQueryField( nextFlag.Value ? ActivityProperty.Start : ActivityProperty.End ), out dateField, out tmpError ) )
				{
					bool descending = !nextFlag.Value;
					orderByStatement = new LinqQueryManager.LinqInstructionOrderBy( dateField, filterCondition, descending );

					returnValue = new LinqQueryManager.LinqInstructionFirstOrLast( true, true, null, orderByStatement );
				}
				else
					errorList.Add( tmpError );
			}

			if ( errorList.Count > 0 )
			{
				error = new DataErrorInfo( errorList );
				return null;
			}

			return returnValue;
		}

		#endregion // CreateLinqStatement

		#region CreateViewList

		protected override IList CreateViewList( ListQueryResult result, IList dataList, IList oldViewList )
		{
			ActivityQueryComponent qc = result.ExternalQueryInfo as ActivityQueryComponent;
			if ( null != qc && qc._isRecurrenceQuery )
			{
				

				IList<ActivityBase> sourceActivities = (IList<ActivityBase>)base.CreateViewList( result, dataList, null );

				if ( null == _occurrencesManager && null != this.ActivityItemManager )
					_occurrencesManager = new OccurrenceManager<TViewItem>( _connector, this.ActivityItemManager, (ActivityBase.StorageProps.Info)_propsInfo );

				if ( null != _occurrencesManager && null != sourceActivities )
				{
					IEnumerable ids = null;

					// Reminders query is different and requires separate logic for creating occurrences of recurring activities.
					// 
					if ( qc._isReminderQuery )
						ids = CreateRemindersOccurrenceIdCollection( qc, sourceActivities, _occurrencesManager );

					if ( null == ids )
						ids = new AggregateOccurrenceIdCollection<TViewItem>( _occurrencesManager, sourceActivities, qc._query.DateRanges );

					return new ViewList<ActivityBase>( ids, _occurrencesManager );
				}
				else
				{
					Debug.Assert( false );
					return null;
				}
			}

			return base.CreateViewList( result, dataList, oldViewList );
		}

		#endregion // CreateViewList

		#region MarkCleanPendingDirtyResults

		protected override void MarkCleanPendingDirtyResults( IEnumerable<ListQueryResult> results )
		{
			// Since activity items rely on resources and calendars (when they try to initialize
			// their owning resource and owning resource calendar references), we need to verify
			// any pending results in the resource list manager first, which also takes care of
			// the resource calendars.
			// 







			base.MarkCleanPendingDirtyResults( results );
		} 

		#endregion // MarkCleanPendingDirtyResults

		#region OnDataListChanged

		protected override void OnDataListChanged( DataListEventListener listener, DataListChangeInfo info )
		{
			base.OnDataListChanged( listener, info );

			switch ( info._changeType )
			{
				case DataListChangeInfo.ChangeType.Reset:
					// Notify the data manager of the data source reset so it can clear selected activities.
					// 
					_connector.NotifyListeners( _connector, "DataSourceReset", typeof( TViewItem ) );
					break;
			}
		} 

		#endregion // OnDataListChanged

		#region OnError

		protected override void OnError( DataErrorInfo error )
		{
			ScheduleUtilities.RaiseErrorHelper( _connector, error );
		}

		#endregion // OnError 

		#region OnPropertyStorageCreated

		protected override void OnPropertyStorageCreated( )
		{
			this.HookIntoPropertyStores( );
		}

		#endregion // OnPropertyStorageCreated

		#region RecreateAllViewItems

		/// <summary>
		/// Discards all the existing view items and dirties all the query results so the view items get re-created.
		/// </summary>
		public override void RecreateAllViewItems( )
		{
			_activityItemManager = null;

			base.RecreateAllViewItems( );
		} 

		#endregion // RecreateAllViewItems

		#region VerifyFieldValueAccessors

		protected override void VerifyFieldValueAccessors( )
		{
			this.InitHasRecurringAppointmentsFlag( );

			base.VerifyFieldValueAccessors( );

			bool isActivityFeatureNotSupported = null != this.List && ! this.IsActivityFeatureSupported( ActivityFeature.EndTime );
			_mappedKeysForActivityQuery = MapsFactory.CreateMapHelper<ActivityProperty, ActivityProperty>( );

			this.RemapField( ActivityProperty.Start, ActivityProperty.End, isActivityFeatureNotSupported );
			this.RemapField( ActivityProperty.StartTimeZoneId, ActivityProperty.EndTimeZoneId, isActivityFeatureNotSupported );
			this.RemapField( ActivityProperty.OriginalOccurrenceStart, ActivityProperty.OriginalOccurrenceEnd, isActivityFeatureNotSupported );
		}

		private void RemapField( ActivityProperty sourceProp, ActivityProperty destProp, bool isActivityFeatureNotSupported )
		{
			PropertyStoreWrapperBase source = this.PropertyStorage.PropertyStores[_propsInfo.GetMappedPropInfo<MappingKey>( _mappedKeys[sourceProp] )._key] as PropertyStoreWrapperBase;
			PropertyStoreWrapperBase dest = this.PropertyStorage.PropertyStores[_propsInfo.GetMappedPropInfo<MappingKey>( _mappedKeys[destProp] )._key] as PropertyStoreWrapperBase;
			if ( isActivityFeatureNotSupported )
			{
				_mappedKeysForActivityQuery[destProp] = sourceProp;
				dest.InitializeFieldValueAccessorFrom( source );
			}
		}

		#endregion // VerifyFieldValueAccessors

		#endregion // Methods

        #region Properties

		#region MinimumRequiredFields

		internal override MappingKey[] MinimumRequiredFields
		{
			get
			{
				
				return ( from ii in this.MinimumRequiredActivityProps select _mappedKeys[ii] ).ToArray( );
			}
		} 

		#endregion // MinimumRequiredFields

        #endregion // Properties

		#endregion // Base Overrides

		#region Properties

		#region Internal Properties

		#region ActivityItemManager

		private ActivityItemManager<TViewItem> ActivityItemManager
		{
			get
			{
				if ( null == _activityItemManager )
				{
					// Id field is required for activities.
					// 
					IFieldValueAccessor idField = this.GetFieldValueAccessor( this.GetActualActivityQueryField( ActivityProperty.Id ) );
					if ( null != idField )
						_activityItemManager = new ActivityItemManager<TViewItem>( _connector, this.PropertyStorage, idField, this.ListEventListener );
				}

				return _activityItemManager;
			}
		} 

		#endregion // ActivityItemManager

		#region HasRecurringActivities

		internal bool HasRecurringActivities
		{
			get
			{
				return _hasRecurringActivities;
			}
		}

		#endregion // HasRecurringActivities

		#region HasVarianceActivities

		internal bool HasVarianceActivities
		{
			get
			{
				return _hasVarianceActivities;
			}
		}

		#endregion // HasVarianceActivities

		#region IsEndTimeRequired

		/// <summary>
		/// Gets a value indicating whether the end time is required for the activities that this activity list manager manages.
		/// Default implementation returns true.
		/// </summary>
		internal virtual bool IsEndTimeRequired
		{
			get
			{
				return true;
			}
		}

		#endregion // IsEndTimeRequired

		#region MinimumRequiredActivityProps

		internal ActivityProperty[] MinimumRequiredActivityProps
		{
			get
			{
				List<ActivityProperty> props = new List<ActivityProperty>( )
				{
					ActivityProperty.Id,
					ActivityProperty.OwningResourceId,
					ActivityProperty.Start
				};

				if ( this.IsEndTimeRequired )
					props.Add( ActivityProperty.End );

				return props.ToArray( );
			}
		}

		#endregion // MinimumRequiredActivityProps

        #region ViewItemFactory

        internal override IViewItemFactory<ActivityBase> ViewItemFactory
		{
			get
			{
				return this.ActivityItemManager;
			}
		}

        #endregion // ViewItemFactory

        #endregion // Internal Properties

        #endregion // Properties

        #region Methods

		#region Private Methods

		#region CreateAnyOfQuery

		private LinqQueryManager.ILinqCondition CreateAnyOfQuery( ActivityProperty field, IEnumerable values, IList<DataErrorInfo> errorList )
		{
			DataErrorInfo error;
			var ret = this.CreateAnyOfQuery( this.GetActualActivityQueryField( field ), values, out error );

			if ( null != error )
				errorList.Add( error );

			return ret;
		}

		#endregion // CreateAnyOfQuery

		#region CreateConditionQuery

		private LinqQueryManager.ILinqCondition CreateConditionQuery( ActivityProperty lhsField, LinqQueryManager.LinqOperator linqOperator, object rhsValue, IList<DataErrorInfo> errorList )
		{
			DataErrorInfo error;
			var ret = this.CreateConditionQuery( this.GetActualActivityQueryField( lhsField ), linqOperator, rhsValue, out error );

			if ( null != error )
				errorList.Add( error );

			return ret;
		}

		#endregion // CreateConditionQuery

		#region CreateEqualQuery

		private LinqQueryManager.ILinqCondition CreateEqualQuery( ActivityProperty field, object value, IList<DataErrorInfo> errorList )
		{
			DataErrorInfo error;
			var ret = this.CreateEqualQuery( this.GetActualActivityQueryField( field ), value, out error );

			if ( null != error )
				errorList.Add( error );

			return ret;
		}

		#endregion // CreateEqualQuery

		#region CreateRemindersOccurrenceIdCollection

		/// <summary>
		/// Creates occurrence id collection for recurring activities for reminders activities result.
		/// </summary>
		/// <param name="qc">Query component that has information regarding the reminders query.</param>
		/// <param name="sourceActivities">This is a collection of recurring and variance activities. Recurring activities
		/// in this collection will be resolved into occurrences with the variances in the collection taken into account.
		/// The variances will also be included in the resultant occurrence id collection.</param>
		/// <param name="occurrencesManager">Occurrence manager.</param>
		/// <returns>Collection of occurrence id's that define the occurrences and variances.</returns>
		private static IEnumerable<OccurrenceId> CreateRemindersOccurrenceIdCollection(
			ActivityQueryComponent qc, IList<ActivityBase> sourceActivities, OccurrenceManager<TViewItem> occurrencesManager )
		{
			Debug.Assert( qc._isReminderQuery, "Only valid for a reminder activities query." );

			// Reminders activities query creates a condition where it breaks up the query condition into multiple date ranges
			// based on reminder interval field of the activities. For activities with reminder interval <= 1 hour, it uses
			// date range that's from now to 1 hour in the future. For activities with reminder interval > 1 hour and <= 1 day,
			// it uses date range that's from 1 hour from now to 1 day from now. So on and so on for further brackets.
			// 
			// The query is like this:
			// ( ReminderInterval <= 1 hour && Start < now + 1 hour )
			// || ( ReminderInterval > 1 hour && ReminderInterval <= 1 day && Start < now + 1 day )
			// || ( ReminderInterval > 1 day && ReminderInterval <= 7 days && Start < now + 7 days )
			// etc...
			// As you can see from above query, for a recurring activity in the result, the effective query date range for
			// variances that would have been returned depend upon the ReminderInterval of the recurring activity. Any 
			// occurrences that are generated for the recurring activity must be generate in the effective query date range
			// which is based on the ReminderInterval of the root so they can take into account variances properly. 
			// Essentially the query result should contain all the necessary variances for the occurrences to be generated
			// properly within the date range. Therefore we have to break up the result into separate collections of
			// activities based on their reminder intervals and then generate each recurring activity's occurrences based 
			// on the associated effective query date range.
			
			
			// 
			TimeSpan[] intervals = qc._reminderIntervalBrackets;
			if ( null != intervals && intervals.Length > 0 )
			{
				IList<Predicate<ActivityBase>> predicates =
					( from ii in intervals select (Predicate<ActivityBase>)( a => a.ReminderInterval <= ii ) ).ToArray( );

				DividedCollection<ActivityBase> dividedCollections = new DividedCollection<ActivityBase>( sourceActivities, predicates );

				var idsCollections = new UnorderedTranslatedCollection<ReadOnlyNotifyCollection<ActivityBase>, IEnumerable>(
					dividedCollections,
					ii => new AggregateOccurrenceIdCollection<TViewItem>( occurrencesManager, ii, new DateRange[] { qc._reminderQueryDateRanges[dividedCollections.IndexOf( ii )] } )
				);

				return new AggregateCollection<OccurrenceId>( idsCollections );
			}

			return null;
		}

		#endregion // CreateRemindersOccurrenceIdCollection

		#region GetActualActivityQueryField

		/// <summary>
		/// Returns the actualy field to be used when constructing queries for activities. This takes into account
		/// if 'End' mapping is not provided in which case it maps 'End', 'EndTimeZoneId' and 'OriginalOccurrenceEnd'
		/// properties to 'Start', 'StartTimeZoneId' and 'OriginalOccurrenceStart' respectively.
		/// </summary>
		/// <param name="propertyParam"></param>
		/// <returns></returns>
		private MappingKey GetActualActivityQueryField( ActivityProperty propertyParam )
		{
			ActivityProperty property = propertyParam;

			if ( !this.IsActivityFeatureSupported( ActivityFeature.EndTime ) )
			{
				switch ( property )
				{
					case ActivityProperty.End:
						property = ActivityProperty.Start;
						break;
					case ActivityProperty.EndTimeZoneId:
						property = ActivityProperty.StartTimeZoneId;
						break;
					case ActivityProperty.OriginalOccurrenceEnd:
						property = ActivityProperty.OriginalOccurrenceStart;
						break;
				}
			}

			return _mappedKeys[property];
		} 

		#endregion // GetActualActivityQueryField

		#region HookIntoPropertyStores

		/// <summary>
		/// Hooks into property stores for properties that require certain actions, like re-evaluating to see if they
		/// should be added to or removed from query results.
		/// </summary>
		private void HookIntoPropertyStores( )
		{
			if ( null != _propertyStorage )
			{
				
				
				// Changes to following properties make the activity subject to being re-evaluated for whether 
				// the activity belongs to a query result or not.
				// 
				IMap<int, bool> propsToHook = MapsFactory.CreateMapHelper<int, bool>( );
				int[] queryReevalProps = new int[]
				{ 
					ActivityBase.StorageProps.Start, 
					ActivityBase.StorageProps.End, 
					ActivityBase.StorageProps.OwningResourceId, 
					ActivityBase.StorageProps.OwningCalendarId,
					ActivityBase.StorageProps.IsTimeZoneNeutral,
					ActivityBase.StorageProps.ReminderEnabled,
					ActivityBase.StorageProps.ReminderInterval
				};

				MapsFactory.SetValues( propsToHook, queryReevalProps, true );

				if ( _hasRecurringActivities || _hasVarianceActivities )
				{
					int[] recurrenceProps = new int[]
					{
						ActivityBase.StorageProps.Recurrence,
						ActivityBase.StorageProps.IsOccurrenceDeleted,
						ActivityBase.StorageProps.MaxOccurrenceDateTime
					};

					MapsFactory.SetValues( propsToHook, recurrenceProps, true );
				}

				if ( null == _propertyStoreListener )
					_propertyStoreListener = new ValueChangeListener<ActivityListManager<TViewItem, MappingKey>, ActivityBase>( this, OnActivity_RequeryPropChangedHandler );

				var stores = _propertyStorage.PropertyStores;
				foreach ( int i in stores )
				{
					var vcn = stores[i] as ISupportValueChangeNotifications<ActivityBase>;
					Debug.Assert( null != vcn );
					if ( null != vcn )
						vcn.RemoveListener( _propertyStoreListener );

					if ( propsToHook[i] )
						vcn.AddListener( _propertyStoreListener );
				}
			}
		}

		#endregion // HookIntoPropertyStores

		#region InitHasRecurringAppointmentsFlag

		/// <summary>
		/// If recurring appointment data source is specified then the appointment data source doesn't contain
		/// recurring appointments. If it's not specified then the appointment data source should be considered
		/// to contain the recurring appointments. Sets _hasRecurringActivities appropriately on both list managers.
		/// </summary>
		private void InitHasRecurringAppointmentsFlag( )
		{
			var otherLM = _complementaryListManager;

			if ( _isRecurringListManager || null == otherLM )
			{
				// For the recurring list manager, it contains recurring activities if it 
				// has a data source.
				// 
				bool hasList = this.HasList;
				_hasRecurringActivities = hasList && this.ValidateFieldMappings( ActivityFeature.Recurrence );
				_hasVarianceActivities = _hasRecurringActivities && this.ValidateFieldMappings( ActivityFeature.Variance );

				// We conditionally hook into certain fields based on the the above flags and therefore we need to re-hook.
				// 
				this.HookIntoPropertyStores( );

				// For the non-recurring list manager, it contains recurring activities if 
				// the recurring data source has not been specified separately.
				// 
				if ( null != otherLM )
				{
					otherLM._hasRecurringActivities = ! hasList && otherLM.ValidateFieldMappings( ActivityFeature.Recurrence );
					otherLM._hasVarianceActivities = otherLM._hasRecurringActivities && otherLM.ValidateFieldMappings( ActivityFeature.Variance );

					// We conditionally hook into certain fields based on the the above flags and therefore we need to re-hook.
					// 
					otherLM.HookIntoPropertyStores( );
				}
			}
			else if ( null != otherLM )
			{
				otherLM.InitHasRecurringAppointmentsFlag( );
			}
		}

		#endregion // InitHasRecurringAppointmentsFlag 

		#endregion // Private Methods

		#region Internal Methods

		#region CreateCalendarsCondition

		internal LinqQueryManager.ILinqCondition CreateCalendarsCondition( IEnumerable<ResourceCalendar> calendars, IList<DataErrorInfo> errorList )
		{
			LinqQueryManager.ILinqCondition calendarIdCondition = null;

			if ( null != calendars )
			{
				List<string> resourceIds = null;
				LinqQueryManager.ILinqCondition calendarsCondition = null;

				foreach ( ResourceCalendar calendar in calendars )
				{
					if ( calendar.IsAutoCreated )
					{
						if ( null == resourceIds )
							resourceIds = new List<string>( );

						resourceIds.Add( calendar.OwningResourceId );
					}
					else
					{
						LinqQueryManager.ILinqCondition c = LinqQueryManager.AndHelper(
							this.CreateEqualQuery( ActivityProperty.OwningResourceId, calendar.OwningResourceId, errorList ),
							this.CreateEqualQuery( ActivityProperty.OwningCalendarId, calendar.Id, errorList )
						);

						calendarsCondition = LinqQueryManager.OrHelper( calendarsCondition, c );
					}
				}

				Debug.Assert( null == resourceIds || null == calendarsCondition, "We can't have intermix of auto-created resource calendars and resource calendars that come from data source." );

				calendarIdCondition = LinqQueryManager.OrHelper( calendarsCondition,
					null != resourceIds ? this.CreateAnyOfQuery( ActivityProperty.OwningResourceId, resourceIds, errorList ) : null );
			}

			return calendarIdCondition;
		}

		#endregion // CreateCalendarsCondition

		#region CreateDateRangeQuery

		private LinqQueryManager.ILinqCondition CreateDateRangeQuery( ActivityQueryComponent qc, ActivityProperty startField, ActivityProperty endField, DateRange dateRange, IList<DataErrorInfo> errorList )
		{
			// The following conditions are meant to do the following:
			// 
			// This essentially checks if the activity's date range intersects with the query date range.
			// It does it by making the activity's End time exclusive - that is for a 10-11 activity,
			// 9-10 and 11-12 queries should not include it. However if an activity has the same start 
			// and end times (like a journal), let's say 11-11 then 11-12 query should include it.
			// 
			// end > dateRange.Start && start < dateRange.End || start == dateRange.Start
			// 

			LinqQueryManager.ILinqCondition c1 = this.CreateConditionQuery( endField, LinqQueryManager.LinqOperator.GreaterThan, dateRange.Start, errorList );

			// If MaxOccurrenceDateTime field is null then include the recurrence in the result. 
			
			// 
			if ( null != qc && qc._isRecurrenceQuery && ActivityProperty.MaxOccurrenceDateTime == endField )
			{
				LinqQueryManager.ILinqCondition tmp = this.CreateConditionQuery( ActivityProperty.MaxOccurrenceDateTime, LinqQueryManager.LinqOperator.Equal, null, errorList );
				c1 = LinqQueryManager.OrHelper( c1, tmp );
			}

			// End date is exclusive so use LessThan.
			// 
			LinqQueryManager.ILinqCondition c2 = this.CreateConditionQuery( startField, LinqQueryManager.LinqOperator.LessThan, dateRange.End, errorList );

			// The following represents start == dateRange.Start.
			// 
			LinqQueryManager.ILinqCondition c3 = this.CreateConditionQuery( startField, LinqQueryManager.LinqOperator.Equal, dateRange.Start, errorList );

			return LinqQueryManager.OrHelper( LinqQueryManager.AndHelper( c1, c2 ), c3 );
		}

		private LinqQueryManager.ILinqCondition CreateDateRangeQuery( ActivityQueryComponent qc, string context, DateRange dateRange, IList<DataErrorInfo> errorList )
		{
			if ( "recurring" == context )
			{
				// If MaxOccurrenceDateTime is mapped then use the following condition:
				// MaxOccurrenceDateTime > dateRange.Start && start < dateRange.End || start == dateRange.Start
				// 
				if ( this.HasMappedField( ActivityProperty.MaxOccurrenceDateTime, true ) )
					return CreateDateRangeQuery( qc, ActivityProperty.Start, ActivityProperty.MaxOccurrenceDateTime, dateRange, errorList );
				else
					// If MaxOccurrenceDateTime is not mapped then use the following condition:
					// Start < dateRange.End
					// 
					return this.CreateConditionQuery( ActivityProperty.Start, LinqQueryManager.LinqOperator.LessThan, dateRange.End, errorList );
			}
			else if ( "variances" == context )
			{
				// For variance activities, use the following condition:
				// ( end > dateRange.Start && start < dateRange.End || start == dateRange.Start )
				// || ( OriginalOccurrenceEnd > dateRange.Start && OriginalOccurrenceStart < dateRange.End || OriginalOccurrenceStart == dateRange.Start )
				// 
				return LinqQueryManager.OrHelper(
					CreateDateRangeQuery( qc, ActivityProperty.Start, ActivityProperty.End, dateRange, errorList ),
					CreateDateRangeQuery( qc, ActivityProperty.OriginalOccurrenceStart, ActivityProperty.OriginalOccurrenceEnd, dateRange, errorList )
				);
			}
			else
			{
				// For regular activities, use the following condition:
				// end > dateRange.Start && start < dateRange.End || start == dateRange.Start
				// 
				return CreateDateRangeQuery( qc, ActivityProperty.Start, ActivityProperty.End, dateRange, errorList );
			}
		}

		#endregion // CreateDateRangeQuery

		#region CreateDateRangesQuery

		private LinqQueryManager.ILinqCondition CreateDateRangesQueryHelper( ActivityQueryComponent qc, string context, IEnumerable<DateRange> dateRanges, IList<DataErrorInfo> errorList )
		{
			LinqQueryManager.ILinqCondition condition = null;

			if ( qc._isReminderQuery )
			{
				for ( int bracketIndex = 0, len = qc._reminderIntervalBrackets.Length; bracketIndex < len; bracketIndex++ )
				{
					foreach ( DateRange ii in dateRanges )
					{
						DateRange iiExpanded = ii;
						LinqQueryManager.ILinqCondition prefixCondition = this.CreateRemindersQueryLinqCondition( qc, errorList, bracketIndex, ref iiExpanded );

						LinqQueryManager.ILinqCondition iiCondition = LinqQueryManager.AndHelper( prefixCondition, this.CreateDateRangeQuery( qc, context, iiExpanded, errorList ) );

						condition = LinqQueryManager.OrHelper( condition, iiCondition );
					}
				}
			}
			else
			{
				foreach ( DateRange ii in dateRanges )
				{
					LinqQueryManager.ILinqCondition iiCondition = this.CreateDateRangeQuery( qc, context, ii, errorList );
					condition = LinqQueryManager.OrHelper( condition, iiCondition );
				}
			}

			return condition;
		}

		private LinqQueryManager.ILinqCondition CreateDateRangesQuery( ActivityQueryComponent qc, string context, IEnumerable<DateRange> dateRanges, IList<DataErrorInfo> errorList )
		{
			LinqQueryManager.ILinqCondition utcDateRangesCondition = CreateDateRangesQueryHelper( qc, context, dateRanges, errorList );
			if ( null != utcDateRangesCondition )
			{
				// To query for time-zone neutral activities, convert utc date ranges to local date ranges and
				// then query for activities with IsTimeZoneNeutral = true.
				// 
				bool areTimeZoneNeutralActivitiesSupported = this.HasMappedField( ActivityProperty.IsTimeZoneNeutral, true );
				if ( areTimeZoneNeutralActivitiesSupported )
				{
					LinqQueryManager.ILinqCondition nonNeutralCondition = CreateConditionQuery( ActivityProperty.IsTimeZoneNeutral, LinqQueryManager.LinqOperator.NotEqual, true, errorList );
					LinqQueryManager.ILinqCondition neutralCondition = CreateConditionQuery( ActivityProperty.IsTimeZoneNeutral, LinqQueryManager.LinqOperator.Equal, true, errorList );

					IEnumerable<DateRange> localDateRanges = ScheduleUtilities.ConvertFromUtc( _connector.TimeZoneInfoProviderResolved.LocalToken, dateRanges );
					LinqQueryManager.ILinqCondition localDateRangesCondition = CreateDateRangesQueryHelper( qc, context, localDateRanges, errorList );

					utcDateRangesCondition = LinqQueryManager.AndHelper( utcDateRangesCondition, nonNeutralCondition );
					localDateRangesCondition = LinqQueryManager.AndHelper( neutralCondition, localDateRangesCondition );

					return LinqQueryManager.OrHelper( utcDateRangesCondition, localDateRangesCondition );
				}
				else
				{
					return utcDateRangesCondition;
				}
			}

			return null;
		}

		#endregion // CreateDateRangesQuery

		#region GetQueryResults

		internal void GetQueryResults( ActivityQuery query, IList<ListQueryResult> resultsList )
		{
			int origResultsListCount = resultsList.Count;

			ActivityQueryRequestedDataFlags requestedInfoFlags = query.RequestedInformation;

			// If we are getting the next activity then no need to have a separate HasNextActivity query.
			// 
			if ( 0 != ( ActivityQueryRequestedDataFlags.NextActivity & requestedInfoFlags ) )
				requestedInfoFlags &= ~ActivityQueryRequestedDataFlags.HasNextActivity;

			// If we are getting the next activity then no need to have a separate HasPreviousActivity query.
			// 
			if ( 0 != ( ActivityQueryRequestedDataFlags.PreviousActivity & requestedInfoFlags ) )
				requestedInfoFlags &= ~ActivityQueryRequestedDataFlags.HasPreviousActivity;

			// Recurring activities need to be queried separately because the query condition is different and also
			// because it can be a separate data source.
			// 
			for ( int pass = 0; pass < 2; pass++ )
			{
				bool queryRecurringActivities = 1 == pass;
				ActivityQueryRequestedDataFlags flags = requestedInfoFlags;

				for ( int i = 0; i < 5 && 0 != flags; i++ )
				{
					ActivityQueryRequestedDataFlags ii = (ActivityQueryRequestedDataFlags)( 1 << i );

					if ( 0 != ( ii & flags ) )
					{
						flags ^= ii;

						ListQueryResult iiResult = new ListQueryResult( 
							ActivityQueryComponent.Create( queryRecurringActivities, query, ii ),
							ActivityQueryRequestedDataFlags.ActivitiesWithinDateRanges == ii,
							ActivityQueryRequestedDataFlags.HasNextActivity != ii && ActivityQueryRequestedDataFlags.HasPreviousActivity != ii,
							this.ProcessListChanged
						);

						resultsList.Add( iiResult );
					}
				}
			}

			for ( int i = origResultsListCount, count = resultsList.Count; i < count; i++ )
				this.PerformQuery( resultsList[i], true );
		}

		#endregion // GetQueryResults

		#region GetReminderQueryResult

		internal void GetReminderQueryResults( ActivityQuery query, List<ListQueryResult> resultList )
		{
			if ( this.IsActivityFeatureSupported( ActivityFeature.Reminder ) )
			{
				Debug.Assert( 1 == query.DateRanges.Count, "Only one date range is supported for reminders query." );

				TimeSpan[] brackets;
				if ( this.HasMappedField( ActivityProperty.ReminderInterval, true ) )
				{
					brackets = new TimeSpan[]
					{
						TimeSpan.FromHours( 1 ),
						TimeSpan.FromDays( 1 ),
						TimeSpan.FromDays( 7 ),
						TimeSpan.FromDays( 28 ),
						TimeSpan.FromDays( 365 )
					};
				}
				else
				{
					
					TimeSpan DEFAULT_REMINDER_INTERVAL = TimeSpan.FromHours( 1 );

					brackets = new TimeSpan[]
					{
						DEFAULT_REMINDER_INTERVAL
					};
				}

				for ( int pass = 0; pass < 2; pass++ )
				{
					bool queryRecurringActivities = 1 == pass;

					ActivityQueryComponent qc = ActivityQueryComponent.CreateReminderQuery( queryRecurringActivities, query, brackets );

					ListQueryResult result = new ListQueryResult( qc, true, true, this.ProcessListChanged );

					this.PerformQuery( result, true );

					resultList.Add( result );
				}
			}
		}

		private LinqQueryManager.ILinqCondition CreateRemindersQueryLinqCondition( ActivityQueryComponent qc, IList<DataErrorInfo> errorList, int bracketIndex, ref DateRange queryRange )
		{
			bool hasReminderIntervalField = this.HasMappedField( ActivityProperty.ReminderInterval, true );
			bool hasReminderEnabledField = this.HasMappedField( ActivityProperty.ReminderEnabled, true );

			TimeSpan? bracketPrev = bracketIndex > 0 ? (TimeSpan?)qc._reminderIntervalBrackets[bracketIndex - 1] : null;
			TimeSpan bracket = qc._reminderIntervalBrackets[bracketIndex];

			
			// don't go back. And If reminder enabled field is not specified then only query for
			// future or in-progress activities.
			// 
			TimeSpan leftDelta = hasReminderEnabledField ? TimeSpan.FromDays( 365 ) : TimeSpan.FromDays( 7 );
			queryRange = ScheduleUtilities.ExpandDateRange( queryRange, leftDelta, bracket );

			if ( default( DateTime ) == qc._reminderQueryDateRanges[bracketIndex].Start )
				qc._reminderQueryDateRanges[bracketIndex] = queryRange;

			return !hasReminderIntervalField ? null : LinqQueryManager.AndHelper(
				bracketPrev.HasValue ? CreateConditionQuery( ActivityProperty.ReminderInterval, LinqQueryManager.LinqOperator.GreaterThan, this.GetReminderIntervalLinqComparisonValue( bracketPrev.Value ), errorList ) : null,
				CreateConditionQuery( ActivityProperty.ReminderInterval, LinqQueryManager.LinqOperator.LessThanOrEqual, this.GetReminderIntervalLinqComparisonValue( bracket ), errorList )
			);
		}

		#endregion // GetReminderQueryResult

		#region HasMappedField

		internal bool HasMappedField( ActivityProperty field, bool checkForLinqQueriability )
		{
			return this.HasMappedField( _mappedKeys[field], checkForLinqQueriability );
		}

		#endregion // HasMappedField

		#region HasMappedFields

		internal bool HasMappedFields( ActivityProperty[] fields, bool checkForLinqQueriability, bool onlyCheckExplicitlyDefinedMappings = false, IList<MappingKey> missingFields = null, IList<MappingKey> mappedFields = null )
		{
			return this.HasMappedFields( from ii in fields select _mappedKeys[ii], checkForLinqQueriability, onlyCheckExplicitlyDefinedMappings, missingFields, mappedFields );
		}

		#endregion // HasMappedFields

		#region SetRecurringListManager

		internal void SetRecurringListManager( ActivityListManager<TViewItem, MappingKey> recurringListManager )
		{
			_complementaryListManager = recurringListManager;
			recurringListManager._complementaryListManager = this;
			recurringListManager._isRecurringListManager = true;
		}

		#endregion // SetRecurringListManager

		#endregion // Internal Methods

		#region Private Methods

		#region GetActivityDataItem

		private static object GetActivityDataItem( ActivityBase a )
		{
			return a.DataItem;
		} 

		#endregion // GetActivityDataItem

		#region GetDataItemPropertyType

		/// <summary>
		/// Gets the type of the underlying property in the data source.
		/// </summary>
		/// <param name="property">Mapped property.</param>
		/// <returns>Type of the associated property in the data source. Null if the property is not mapped.</returns>
		private Type GetDataItemPropertyType( ActivityProperty property )
		{
			string field = this.GetMappedFieldIfAny( _mappedKeys[ActivityProperty.ReminderInterval] );
			LinqQueryManager linqManager = this.LinqQueryManager;
			if ( !string.IsNullOrEmpty( field ) && null != linqManager )
				return linqManager.GetPropertyType( field );

			return null;
		}

		#endregion // GetDataItemPropertyType

		#region GetReminderIntervalLinqComparisonValue

		/// <summary>
		/// Gets the specified reminder interval value converted to long if the underlying data source object's
		/// reminder interval is of long type. Otherwise it returns the span value itself.
		/// </summary>
		/// <param name="span">Reminder interval value.</param>
		/// <returns>Value used for comparing in linq query conditions.</returns>
		private object GetReminderIntervalLinqComparisonValue( TimeSpan span )
		{
			Type type = this.GetDataItemPropertyType( ActivityProperty.ReminderInterval );

			if ( typeof( long ) == type )
				return span.Ticks;

			return span;
		}

		#endregion // GetReminderIntervalLinqComparisonValue

		#region InitializeMappedKeys

		private void InitializeMappedKeys( )
		{
			IMap<ActivityProperty, MappingKey> map = MapsFactory.CreateMapHelper<ActivityProperty, MappingKey>( );

			ActivityProperty[] activityFields = new ActivityProperty[] 
            { 
                ActivityProperty.Id, 
                ActivityProperty.OwningResourceId, 
                ActivityProperty.OwningCalendarId, 
                ActivityProperty.Start,
				ActivityProperty.StartTimeZoneId,
				ActivityProperty.EndTimeZoneId,
				ActivityProperty.IsTimeZoneNeutral,
				ActivityProperty.End,
                ActivityProperty.Subject, 
                ActivityProperty.Description, 
                ActivityProperty.IsVisible,
				ActivityProperty.Recurrence,
				ActivityProperty.RecurrenceVersion,
				ActivityProperty.MaxOccurrenceDateTime,
				ActivityProperty.OriginalOccurrenceStart,
				ActivityProperty.OriginalOccurrenceEnd,
				ActivityProperty.IsOccurrenceDeleted,
				ActivityProperty.VariantProperties,
				ActivityProperty.RootActivityId,
				ActivityProperty.ReminderInterval,
				ActivityProperty.ReminderEnabled,
				ActivityProperty.Reminder,
				ActivityProperty.UnmappedProperties
            };

			foreach ( ActivityProperty key in activityFields )
			{
				string iiName = Enum.GetName( typeof( ActivityProperty ), key );

				MappingKey mappedField = (MappingKey)Enum.Parse( typeof( MappingKey ), iiName, false );

				map[key] = mappedField;
			}

			_mappedKeys = map;

			this.InitializeUnmappedPropertiesMappingKey( _mappedKeys[ ActivityProperty.UnmappedProperties ] );
		}

		#endregion // InitializeMappedKeys 

		#region OnActivity_RequeryPropChanged

		private static void OnActivity_RequeryPropChangedHandler( ActivityListManager<TViewItem, MappingKey> listManager, ActivityBase activity )
		{
			listManager.OnActivity_RequeryPropChanged( activity );
		}

		/// <summary>
		/// This is called when a property on an activity changes that may require us to remove the activity
		/// from one or more query results and furthermore add it to one or more query results.
		/// </summary>
		/// <param name="activity">Activity whose property changed.</param>
		private void OnActivity_RequeryPropChanged( ActivityBase activity )
		{
			object dataItem = activity.DataItem;
			if ( null != dataItem && !activity.IsInitializing )
			{
				IList<ListQueryResult> results = this.GetCleanResults( );
				if ( null != results && results.Count > 0 )
					_reevalActivitiesQueue.Enque( dataItem, true );
			}
		}

		#endregion // OnActivity_RequeryPropChanged

		#region ProcessListChanged

		private bool ProcessListChanged( ListQueryResult result, DataListChangeInfo info )
		{
			ActivityQueryComponent qc = result.ExternalQueryInfo as ActivityQueryComponent;

			if ( null != qc )
			{
				switch ( qc._singleFlag )
				{
					case ActivityQueryRequestedDataFlags.HasPreviousActivity:
					case ActivityQueryRequestedDataFlags.HasNextActivity:
					case ActivityQueryRequestedDataFlags.PreviousActivity:
					case ActivityQueryRequestedDataFlags.NextActivity:
						this.DirtyQueryResult( result, true, true );
						return false;
				}

				switch ( info._changeType )
				{
					case DataListChangeInfo.ChangeType.Remove:
					case DataListChangeInfo.ChangeType.Replace:
						IList oldItems = info.OldItems;
						Debug.Assert( null != oldItems );
						if ( null != oldItems )
							_reevalActivitiesQueue.RemoveItems( new TypedEnumerable<object>( oldItems ) );
						break;
					case DataListChangeInfo.ChangeType.Reset:
						_reevalActivitiesQueue.RemoveAllItems( );
						break;
				}
			}

			return true;
		}

		#endregion // ProcessListChanged

		#region ValidateFieldMappings

		internal bool ValidateFieldMappings( ActivityFeature? feature )
		{
			DataErrorInfo error;
			return this.ValidateFieldMappingsHelper( feature, out error );
		}

		internal bool ValidateFieldMappingsHelper( ActivityFeature? feature, out DataErrorInfo error )
		{
			List<DataErrorInfo> errorList = new List<DataErrorInfo>( );
			bool ret = this.ValidateFieldMappingsHelper( feature, errorList );

			error = DataErrorInfo.CreateFromList( errorList );
			
			return ret;
		}

		internal bool ValidateFieldMappingsHelper( ActivityProperty[] props, out DataErrorInfo error )
		{
			List<DataErrorInfo> errorList = new List<DataErrorInfo>( );
			bool ret = this.ValidateFieldMappingsHelper( props, errorList );

			error = DataErrorInfo.CreateFromList( errorList );

			return ret;
		}

		internal bool ValidateFieldMappingsHelper( ActivityProperty[] props, IList<DataErrorInfo> errorList, ActivityProperty[] dependentProps = null, string errorMsgHeader = null )
		{
			List<MappingKey> missingFields = new List<MappingKey>( );
			if ( this.HasMappedFields( props, true, false, missingFields ) )
				return true;

			string diagnosticMsg = ScheduleUtilities.GetString( "LE_MissingMappingHeader", typeof( TViewItem ).Name );

			if ( null != errorMsgHeader )
				diagnosticMsg += System.Environment.NewLine + errorMsgHeader;

			if (errorMsgHeader == null || props.Length > 1)
				diagnosticMsg += System.Environment.NewLine + ScheduleUtilities.BuildList( missingFields, System.Environment.NewLine );

			DataErrorInfo error = DataErrorInfo.CreateDiagnostic( this.Mappings, diagnosticMsg );
			// This will make the error severe if some mappings are provided but not others. If none of the mappings are
			// provided then the feature is disabled in which case it's just a diagnostic error.
			// 
			error.Severity = missingFields.Count < props.Length ? ErrorSeverity.SevereError : ErrorSeverity.Diagnostic;
			errorList.Add( error );

			if ( null != dependentProps )
			{
				List<MappingKey> mappedFields = new List<MappingKey>( );
				this.HasMappedFields( dependentProps, false, true, mappedFields: mappedFields );
				if ( mappedFields.Count > 0 )
				{
					//string errorMsg =
						//"The following field mappings explicitly defined in the mappings collection require the following mappings for proper functioning:"
						//+ "Explicitly mapped fields: " + ScheduleUtilities.BuildList( mappedFields, System.Environment.NewLine )
						//+ "Required fields: " + ScheduleUtilities.BuildList( props, System.Environment.NewLine ) ) );
					errorList.Add( ScheduleUtilities.CreateDiagnosticFromId( this.Mappings, "LE_RequiredMappingHeader", ScheduleUtilities.BuildList( mappedFields, System.Environment.NewLine ), ScheduleUtilities.BuildList( props, System.Environment.NewLine )));
				}
			}

			return false;
		}

		private ActivityProperty[] GetRequiredFields( ActivityFeature? feature )
		{
			string msg;
			ActivityProperty[] dependentProps;
			return this.GetRequiredFields( feature, out msg, out dependentProps );
		}

		private ActivityProperty[] GetRequiredFields( ActivityFeature? feature, out string msg, out ActivityProperty[] dependentProps )
		{
			ActivityProperty[] props = null;
			ActivityFeature[] dependentFeatures = null;
			dependentProps = null;
			msg = null;

			if ( feature.HasValue )
			{
				switch ( feature.Value )
				{
					case ActivityFeature.Recurrence:
						msg = ScheduleUtilities.GetString("LE_MissingMappingRecurrence"); //"In order to support recurring activities, the 'Recurrence' field is required.";
						props = new ActivityProperty[] { ActivityProperty.Recurrence };
						dependentFeatures = new ActivityFeature[] { ActivityFeature.Variance };
						break;
					case ActivityFeature.Variance:
						msg = ScheduleUtilities.GetString("LE_MissingMappingVariance"); //"In order to support variances (modifications to occurrence of a recurring activity), the following fields are required.";
						props = new ActivityProperty[] 
						{ 
							ActivityProperty.RootActivityId,
							ActivityProperty.OriginalOccurrenceStart,
							ActivityProperty.OriginalOccurrenceEnd,
							ActivityProperty.IsOccurrenceDeleted,
							ActivityProperty.VariantProperties,
							ActivityProperty.RecurrenceVersion
						};

						dependentProps = new ActivityProperty[] { };
						break;
					case ActivityFeature.TimeZoneNeutrality:
						msg = ScheduleUtilities.GetString("LE_MissingMappingTimeZoneNeutral"); //"In order to support time-zone neutral activities, 'IsTimeZoneNeutral' field is required.";
						props = new ActivityProperty[] { ActivityProperty.IsTimeZoneNeutral };
						break;
					case ActivityFeature.Reminder:
						msg = ScheduleUtilities.GetString("LE_MissingMappingReminder"); //"In order to support reminders, the following fields are required.";
						props = new ActivityProperty[] 
						{ 
							ActivityProperty.ReminderEnabled,
							ActivityProperty.ReminderInterval
						};

						dependentProps = new ActivityProperty[] { ActivityProperty.Reminder };
						break;
					case ActivityFeature.CanChangeOwningCalendar:
					case ActivityFeature.CanChangeOwningResource:
						// Changing owning calendar and owning resource do not require any specific field mappings.
						// 
						props = new ActivityProperty[] { };
						break;
					case ActivityFeature.EndTime:
						props = new ActivityProperty[] 
						{ 
							ActivityProperty.End
						};

						dependentProps = new ActivityProperty[] { ActivityProperty.EndTimeZoneId, ActivityProperty.OriginalOccurrenceEnd };
						msg = ScheduleUtilities.GetString("LE_MissingMappingEnd"); //'End' field is required in order to support end-times for activities.
						break;
					default:
						msg = "Unknown activity feature.";
						Debug.Assert( false, msg );
						break;
				}
			}
			else
			{
				props = this.MinimumRequiredActivityProps;
			}

			if ( null != dependentFeatures )
			{
				dependentProps = ScheduleUtilities.Aggregate<ActivityProperty>(
						dependentProps,
						ScheduleUtilities.Aggregate<ActivityProperty>( true, from ii in dependentFeatures select (IEnumerable<ActivityProperty>)GetRequiredFields( ii ) )
				).Distinct( ).ToArray( );
			}

			return props;
		}

		internal bool ValidateFieldMappingsHelper( ActivityFeature? feature, IList<DataErrorInfo> errorList )
		{
			ActivityProperty[] props = null;
			ActivityProperty[] dependentProps = null;
			string msg = null;

			props = this.GetRequiredFields( feature, out msg, out dependentProps );

			return null == props || this.ValidateFieldMappingsHelper( props, errorList, dependentProps, msg );
		} 

		#endregion // ValidateFieldMappings

        #endregion // Private Methods

		#region Public Methods

		#region IsActivityFeatureSupported

		public bool IsActivityFeatureSupported( ActivityFeature feature )
		{
			if ( !this.HasList )
				return false;

			switch ( feature )
			{
				case ActivityFeature.Recurrence:
					return _hasRecurringActivities
						|| null != _complementaryListManager && _complementaryListManager._hasRecurringActivities;
				case ActivityFeature.Variance:
					return _hasVarianceActivities
						|| null != _complementaryListManager && _complementaryListManager._hasVarianceActivities;
				case ActivityFeature.Reminder:
				case ActivityFeature.TimeZoneNeutrality:
				case ActivityFeature.CanChangeOwningCalendar:
				case ActivityFeature.CanChangeOwningResource:
				case ActivityFeature.EndTime:
					return this.ValidateFieldMappings( feature );
				default:
					Debug.Assert( false, "Unknown feature." );
					return false;
			}
		}

		#endregion // IsActivityFeatureSupported 

		#endregion // Public Methods

		#endregion // Methods
	}

	#endregion // ActivityListManager Class

	#region AppointmentListManager Class

	internal class AppointmentListManager : ActivityListManager<Appointment, AppointmentProperty>
	{
		#region Constructor

		internal AppointmentListManager( IScheduleDataConnector connector )
            : base( connector, Appointment.StorageProps.Info.Instance )
		{
		}

		#endregion // Constructor
	}

	#endregion // AppointmentListManager Class

	#region JournalListManager Class

	internal class JournalListManager : ActivityListManager<Journal, JournalProperty>
	{
		#region Constructor

		internal JournalListManager( IScheduleDataConnector connector )
            : base( connector, Journal.StorageProps.Info.Instance )
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region IsEndTimeRequired

		/// <summary>
		/// Overridden. Returns false since the journals do not require end time.
		/// </summary>
		internal override bool IsEndTimeRequired
		{
			get
			{
				return false;
			}
		}

		#endregion // IsEndTimeRequired  

		#endregion // Properties

		#endregion // Base Overrides
	}

	#endregion // JournalListManager Class

	#region TaskListManager Class

	internal class TaskListManager : ActivityListManager<Task, TaskProperty>
	{
		#region Constructor

		internal TaskListManager( IScheduleDataConnector connector )
            : base( connector, Task.StorageProps.Info.Instance )
		{
		}

		#endregion // Constructor
	}

	#endregion // TaskListManager Class    

	#region ActivityCategoryListManager Class

	// SSP 12/8/10 - NAS11.1 Activity Categories
	// 

	internal class ActivityCategoryListManager : ScheduleItemCollectionListManager<ActivityCategory, ActivityCategoryProperty>
	{
		#region Member Vars

		private Index<ActivityCategory, string> _categoriesIndex;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		internal ActivityCategoryListManager( IScheduleDataConnector connector )
			: base( connector, ActivityCategory.StorageProps.Info.Instance,
				delegate( ActivityCategory activityCategory )
				{
					return activityCategory.DataItem;
				}
			)
		{
		}

		#endregion // Constructor

		#region Base Overrides

		#region Properties

		#region MinimumRequiredFields

		internal override ActivityCategoryProperty[] MinimumRequiredFields
		{
			get
			{
				return new ActivityCategoryProperty[]
				{
					ActivityCategoryProperty.CategoryName,
					ActivityCategoryProperty.Color
				};
			}
		}

		#endregion // MinimumRequiredFields

		#endregion // Properties

		#region Methods

		#region CreateItemsCollection

		protected override void CreateItemsCollection( ref ViewList<ActivityCategory> collection )
		{
			collection = new ViewList<ActivityCategory>( this.List, this.ViewItemFactory, true, this.OnPreVerifyViewList );
		}

		#endregion // CreateItemsCollection

		#region CreateViewItemFactory

		protected override IViewItemFactory<ActivityCategory> CreateViewItemFactory( IFieldValueAccessor idField )
		{
			return new ActivityCategoryItemManager( this.PropertyStorage, idField, this.ListEventListener );
		}

		#endregion // CreateViewItemFactory

		#region GetCategories

		internal IEnumerable<ActivityCategory> GetCategories( ActivityBase activity, bool includeAllSupportedCategories )
		{
			this.EnsureFieldValueAccessorsVerified( );

			if ( null == _categoriesIndex )
				_categoriesIndex = ActivityCategoryCollection.CreateIndexHelper( this.Items );

			return new ResolvedActivityCategoryCollection( this.Items, _categoriesIndex, activity, includeAllSupportedCategories );
		}

		#endregion // GetCategories

		#region InitIdFieldValueAccessor

		protected override void InitIdFieldValueAccessor( bool clearWrappedFva = false )
		{
			FieldValueAccessorWrapper fva = (FieldValueAccessorWrapper)_idFieldValueAccessor;
			if ( null == fva )
				_idFieldValueAccessor = fva = new FieldValueAccessorWrapper( );

			fva._fva = clearWrappedFva ? null : new DelegateFieldValueAccessor<object>( ii => ii, null );
		}

		#endregion // InitIdFieldValueAccessor

		#endregion // Methods

		#endregion // Base Overrides

		#region Properties

		#region Internal Properties

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Private Methods

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ActivityCategoryListManager Class

	#region ProjectListManager Class

	// SSP 1/6/12 - NAS12.1 XamGantt
	// 

	//internal class ProjectListManager : ScheduleItemCollectionListManager<Project, ProjectProperty>
	//{
	//    #region Member Vars

	//    #endregion // Member Vars

	//    #region Constructor

	//    /// <summary>
	//    /// Constructor.
	//    /// </summary>
	//    internal ProjectListManager( IScheduleDataConnector connector )
	//        : base( connector, Project.StorageProps.Info.Instance,
	//            delegate( Project project )
	//            {
	//                return project.DataItem;
	//            }
	//        )
	//    {
	//    }

	//    #endregion // Constructor

	//    #region Base Overrides

	//    #region Properties

	//    #region MinimumRequiredFields

	//    internal override ProjectProperty[] MinimumRequiredFields
	//    {
	//        get
	//        {
	//            return new ProjectProperty[]
	//            {
	//                ProjectProperty.Id
	//            };
	//        }
	//    }

	//    #endregion // MinimumRequiredFields

	//    #endregion // Properties

	//    #region Methods

	//    #region CreateItemsCollection

	//    protected override void CreateItemsCollection( ref ViewList<Project> collection )
	//    {
	//        collection = new ViewList<Project>( this.List, this.ViewItemFactory, true, this.OnPreVerifyViewList );
	//    }

	//    #endregion // CreateItemsCollection

	//    #region CreateViewItemFactory

	//    protected override IViewItemFactory<Project> CreateViewItemFactory( IFieldValueAccessor idField )
	//    {
	//        return new ProjectItemManager( this.PropertyStorage, idField, this.ListEventListener );
	//    }

	//    #endregion // CreateViewItemFactory

	//    #region InitIdFieldValueAccessor

	//    protected override void InitIdFieldValueAccessor( bool clearWrappedFva = false )
	//    {
	//        FieldValueAccessorWrapper fva = (FieldValueAccessorWrapper)_idFieldValueAccessor;
	//        if ( null == fva )
	//            _idFieldValueAccessor = fva = new FieldValueAccessorWrapper( );

	//        fva._fva = clearWrappedFva ? null : this.GetFieldValueAccessor( ProjectProperty.Id );
	//    }

	//    #endregion // InitIdFieldValueAccessor

	//    #endregion // Methods

	//    #endregion // Base Overrides

	//    #region Properties

	//    #region Internal Properties

	//    #endregion // Internal Properties

	//    #endregion // Properties

	//    #region Methods

	//    #region Private Methods

	//    #endregion // Private Methods

	//    #endregion // Methods
	//}

	#endregion // ProjectListManager Class

	#region VerifyAsyncItemsQueue Class

	/// <summary>
	/// Used to manage one or more items which are processed asynchronously. When an item is added, an async
	/// operation can be started. Any further items added in the meantime are collected into a list. When the
	/// async handler is executed, all the items are processed via the the 'verifyCallback' delegate
	/// specified in the constructor.
	/// </summary>
	/// <typeparam name="T">Type of items that are to be processed asynchronously.</typeparam>
	internal class VerifyAsyncItemsQueue<T>
		where T: class
	{
		#region Member Vars

		private ISet<T> _dataItems;
		private DeferredOperation _operation;
		private Action<IEnumerable<T>> _verifyCallback;
		private bool _manageItemsAsWeakReferences;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="verifyCallback">This will be called to process the items.</param>
		/// <param name="manageItemsAsWeakReferences">Whether to manage items as weak references.</param>
		internal VerifyAsyncItemsQueue( Action<IEnumerable<T>> verifyCallback, bool manageItemsAsWeakReferences )
		{
			_verifyCallback = verifyCallback;
			_manageItemsAsWeakReferences = manageItemsAsWeakReferences;
		} 

		#endregion // Constructor

		#region Properties

		#region Private Properties

		#region DataItems

		/// <summary>
		/// The set used to manage the items.
		/// </summary>
		private ISet<T> DataItems
		{
			get
			{
				if ( null == _dataItems )
					_dataItems = _manageItemsAsWeakReferences ? (ISet<T>)new WeakSet<T>( ) : (ISet<T>)new HashSet<T>( );

				return _dataItems;
			}
		}

		#endregion // DataItems  

		#endregion // Private Properties

		#endregion // Properties

		#region Internal Methods

		#region Enque

		/// <summary>
		/// Adds an item to be processed asynchronously.
		/// </summary>
		/// <param name="dataItem">Item object.</param>
		/// <param name="startAsyncOperation">Whether to start an async operation if it already hasn't been started previously.</param>
		internal void Enque( T dataItem, bool startAsyncOperation )
		{
			this.DataItems.Add( dataItem );

			if ( startAsyncOperation )
				this.StartAsyncVerification( );
		}

		#endregion // Enque

		#region PerformVerification

		internal void PerformVerification( )
		{
			this.ProcessItems( );
		} 

		#endregion // PerformVerification

		#region RemoveItem

		/// <summary>
		/// Removes the specified item from the items queue.
		/// </summary>
		/// <param name="dataItem">Item to remove.</param>
		internal void RemoveItem( T dataItem )
		{
			if ( null != _dataItems )
				_dataItems.Remove( dataItem );
		}

		#endregion // RemoveItem

		#region RemoveItems

		/// <summary>
		/// Removes the specified items.
		/// </summary>
		/// <param name="dataItems">Items to remove.</param>
		internal void RemoveItems( IEnumerable<T> dataItems )
		{
			Debug.Assert( null != dataItems );

			if ( null != _dataItems )
			{
				foreach ( T ii in dataItems )
					_dataItems.Remove( ii );
			}
		}

		#endregion // RemoveItems

		#region RemoveAllItems

		/// <summary>
		/// Removes all items and stops any pending async callback.
		/// </summary>
		internal void RemoveAllItems( )
		{
			_dataItems = null;

			this.CancelPendingOperation( );
		}

		#endregion // RemoveAllItems

		#region StartAsyncVerification

		/// <summary>
		/// Starts the async verification process.
		/// </summary>
		internal void StartAsyncVerification( )
		{
			if ( null == _operation )
			{
				_operation = new DeferredOperation( this.ProcessItems );
				_operation.StartAsyncOperation( );
			}
		}

		#endregion // StartAsyncVerification 

		#endregion // Internal Methods

		#region Private Methods

		#region CancelPendingOperation

		private void CancelPendingOperation( )
		{
			DeferredOperation operation = _operation;
			_operation = null;
			if ( null != operation )
				operation.CancelPendingOperation( );
		}

		#endregion // CancelPendingOperation

		#region ProcessItems

		private void ProcessItems( )
		{
			if ( null != _operation )
			{
				_operation.CancelPendingOperation( );
				_operation = null;
			}

			ISet<T> dataItems = _dataItems;
			_dataItems = null;

			if ( null != dataItems && dataItems.Count > 0 && null != _verifyCallback )
				_verifyCallback( dataItems );
		}

		#endregion // ProcessItems 

		#endregion // Private Methods
	}

	#endregion // VerifyAsyncItemsQueue Class 
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