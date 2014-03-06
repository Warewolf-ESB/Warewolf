using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Linq;
using System.Reflection;


using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Services


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

{
    #region IViewItemFactory<TViewItem> Interface

    internal interface IViewItemFactory<TViewItem>
	{
        TViewItem CreateViewItem( object dataItem );

		object GetDataItem( TViewItem viewItem );

		void SetDataItem( TViewItem viewItem, object newDataItem );

        object GetDataItemComparisonTokenForRecycling( object dataItem );
	}

    #endregion // IViewItemFactory<TViewItem> Interface

	#region IViewItemManager<TViewItem> Interface

	internal interface IViewItemManager<TViewItem> : IViewItemFactory<TViewItem>
	{
		TViewItem CreateAddNewViewItem( );

		bool IsAddNewItem( TViewItem item );

		void BeginAddNewOperation( TViewItem viewItem );

		bool AssociateDataItemToAddNewItem( object dataItem );

		void CancelAddNewOperation( );

		void EndAddNewOperation( object dataItem );

		bool IsAddNewOperationInProgress { get; }

		TViewItem GetViewItem( object dataItem, bool createNewIfNecessary );

		TViewItem GetViewItemWithId( object id );

		void OnItemDeleted( object dataItem );

		void DiscardCachedItems( );

		IList GetResultDataList( IEnumerable dataItems );

		void UpdateResultDataList_Add( IList resultDataList, object dataItem, bool checkIfAlreadyExists, bool replaceExistingItem );

		void UpdateResultDataList_Remove( IList resultDataList, object dataItem );

		void AssignId( TViewItem viewItem, object dataItem );
	} 

	#endregion // IViewItemManager<TViewItem> Interface

	#region ViewItemManager<TViewItem> Class

	internal abstract class ViewItemManager<TViewItem> : IViewItemManager<TViewItem>
		where TViewItem : class
	{
		#region Nested Data Structures

		#region IdToken Class

		internal class IdToken
		{
			#region Member Vars

			private object _id;
			private object _dataItem;
			private object _viewItemsList; 

			#endregion // Member Vars

			#region Constructors

			private IdToken( )
			{
			} 

			internal IdToken( object dataItem, ViewItemManager<TViewItem> manager, bool hookIntoDataItem )
			{
				_dataItem = dataItem;
				_id = manager._idField.GetValue( dataItem );

				if ( hookIntoDataItem )
					this.HookUnhookIntoDataItem( manager._dataListEventListener, hookIntoDataItem );
			}

			#endregion // Constructors

			#region Properties

			#region DataItem

			public object DataItem
			{
				get
				{
					return _dataItem;
				}
			}

			#endregion // DataItem 

			#endregion // Properties

			#region Methods

			#region AssociateViewModelItem

			public void AssociateViewModelItem( TViewItem viewItem )
			{
				if ( null == _viewItemsList || _viewItemsList == viewItem )
					_viewItemsList = viewItem;
				else
				{
					IList<TViewItem> list = _viewItemsList as IList<TViewItem>;
					if ( null == list )
					{
						list = new List<TViewItem>( );
						list.Add( (TViewItem)_viewItemsList );
						_viewItemsList = list;
					}

					// SSP 3/31/11 TFS68023
					// Check to see if the item is already in the list.
					// 
					//list.Add( viewItem );
					if ( !list.Contains( viewItem ) )
						list.Add( viewItem );
				}
			}

			#endregion // AssociateViewModelItem

			#region CreateFromId

			internal static IdToken CreateFromId( object id )
			{
				return new IdToken( )
				{
					_id = id
				};
			}

			#endregion // CreateFromId

			#region DeassociateViewModelItem

			public void DeassociateViewModelItem( TViewItem viewItem )
			{
				TViewItem existingItem = _viewItemsList as TViewItem;
				if ( null != existingItem )
				{
					if ( existingItem == viewItem )
						
						
						
						_viewItemsList = null;
				}
				else
				{
					IList<TViewItem> list = (IList<TViewItem>)_viewItemsList;
					if ( null != list )
					{
						list.Remove( viewItem );
						
						// SSP 3/31/11 TFS68023
						// 
						if ( 0 == list.Count )
							_viewItemsList = null;
					}
				}
			}

			#endregion // DeassociateViewModelItem

			#region Equals

			public override bool Equals( object obj )
			{
				IdToken t = obj as IdToken;
				return null != t && object.Equals( _id, t._id );
			}

			#endregion // Equals

			#region GetHashCode

			public override int GetHashCode( )
			{
				return null != _id ? _id.GetHashCode( ) : 0;
			}

			#endregion // GetHashCode

			#region GetAssociatedViewItems

			public IEnumerable<TViewItem> GetAssociatedViewItems( )
			{
				TViewItem item = _viewItemsList as TViewItem;
				if ( null != item )
					return ScheduleUtilities.GetSingleItemEnumerable( item );

				return (IList<TViewItem>)_viewItemsList;
			}

			#endregion // GetAssociatedViewItems

			#region GetFirstAssociatedViewItem

			public TViewItem GetFirstAssociatedViewItem( )
			{
				TViewItem item = _viewItemsList as TViewItem;
				if ( null != item )
					return item;

				IList<TViewItem> list = (IList<TViewItem>)_viewItemsList;
				if ( null != list )
					return list[0];

				return null;
			}

			#endregion // GetFirstAssociatedViewItem

			#region HookUnhookIntoDataItem

			internal void HookUnhookIntoDataItem( DataListEventListener dataSourceListener, bool hook )
			{
				if ( null != dataSourceListener && null != _dataItem )
					dataSourceListener.HookUnHookDataItem( _dataItem, hook );
			}

			#endregion // HookUnhookIntoDataItem

			#region UpdateDataModelObject

			public void UpdateDataModelObject( object dataItem, ViewItemManager<TViewItem> manager )
			{
				if ( _dataItem != dataItem )
				{
					this.HookUnhookIntoDataItem( manager._dataListEventListener, false );

					_dataItem = dataItem;

					this.HookUnhookIntoDataItem( manager._dataListEventListener, true );

					IEnumerable<TViewItem> viewItems = this.GetAssociatedViewItems( );
					if ( null != viewItems )
					{
						foreach ( TViewItem ii in viewItems )
						{
							manager.NotifyDataItemChanged( ii );
						}
					}
				}
			}

			#endregion // UpdateDataModelObject 

			#endregion // Methods
		}

		#endregion // IdToken Class

		#region ResultDataList Class

		private class ResultDataList : ObservableCollection<object>
		{
			public ResultDataList( IEnumerable list )
				: base( ScheduleUtilities.ToTyped<object>( list ) )
			{
			}
		}

		#endregion // ResultDataList Class 

		#endregion // Nested Data Structures

		#region Member Vars

		private WeakDictionary<IdToken, IdToken> _viewItemsCache = new WeakDictionary<IdToken, IdToken>( true, true );

		private IFieldValueAccessor _idField;
		private DataListEventListener _dataListEventListener;
		private TViewItem _addNewOperation_ViewItem;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="idField">Used for accessing id field values of data items.</param>
		/// <param name="dataListEventListener">Used for hooking into data items for property change notifications.</param>
		internal ViewItemManager( IFieldValueAccessor idField, DataListEventListener dataListEventListener )
		{
			CoreUtilities.ValidateNotNull( idField );

			_idField = idField;
			_dataListEventListener = dataListEventListener;
		} 

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region AssignId

		public void AssignId( TViewItem viewItem, object dataItem )
		{
			object idOverride = this.CreateIdOverride( viewItem, dataItem );
			object currentId = _idField.GetValue( dataItem );
			object id;

			if ( !CoreUtilities.IsValueEmpty( idOverride ) )
			{
				id = idOverride;
			}
			else if ( !CoreUtilities.IsValueEmpty( currentId ) )
			{
				// Since we are going to use the current id value of the data item, simply return.
				// 
				return;
			}
			else
			{
				// If Id field hasn't been initialized then create a new one. This means that the data source
				// is not auto-generating id field values. In which case we should generate some id value.
				// The developer does have a chance to initialize the id via schedule data connector's 
				// CreateNew method.
				// 
				id = this.CreateNewID( );
			}

			_idField.SetValue( dataItem, id );
		}

		#endregion // AssignId

		#region AssociateDataItemToAddNewItem

		public bool AssociateDataItemToAddNewItem( object dataItem )
		{
			TViewItem viewItem = _addNewOperation_ViewItem;
			if ( null != viewItem && null == this.GetDataItem( viewItem ) )
			{
				object currentDataItem = this.GetDataItem( viewItem );
				Debug.Assert( null == currentDataItem || currentDataItem == dataItem );
				if ( null == currentDataItem )
				{
					this.AssignId( viewItem, dataItem );
					this.SetDataItem( viewItem, dataItem );
					return true;
				}

				return dataItem == currentDataItem;
			}

			return false;
		}

		#endregion // AssociateDataItemToAddNewItem

		#region DiscardCachedItems

		public void DiscardCachedItems( )
		{
			_viewItemsCache.Clear( );
		}

		#endregion // DiscardCachedItems

		#region GetDataItem

		public object GetDataItem( TViewItem viewItem )
		{
			IdToken token = this.GetIdToken( viewItem );
			return null != token ? token.DataItem : null;
		}

		#endregion // GetDataItem

		#region GetResultDataList

		public IList GetResultDataList( IEnumerable dataItems )
		{
			return new ResultDataList( dataItems );
		}

		#endregion // GetResultDataList

		#region GetViewItem

		public TViewItem GetViewItem( object dataItem, bool createNewIfNecessary )
		{
			TViewItem viewItem;
			IdToken idToken = this.GetIdToken( dataItem, true, true );

			viewItem = idToken.GetFirstAssociatedViewItem( );
			if ( null == viewItem )
			{
				if ( createNewIfNecessary )
				{
					// If the data item is a TViewItem itself, then just use that. This would be the case if a 
					// list of view items themeselves, like the list of activities, is supplied as the data source.
					// 
					viewItem = dataItem as TViewItem;
					if ( null == viewItem )
						viewItem = this.AllocateViewItem( );

					this.SetDataItem( viewItem, dataItem );
				}
			}
			// SSP 3/25/11 TFS67051
			// If the data source is a collection of TViewItem's and an old TViewItem is removed and a new
			// one with the same id is added (not a typical scenario, however for example a collection is
			// cleared and re-populated with items with common id's) - then we need to discard the old
			// TViewItem and associate the new TViewItem to the id token. Added the following else-if block.
			// 
			else if ( dataItem != viewItem && dataItem is TViewItem )
			{
				// De-associate the old view item.
				// 
				idToken.DeassociateViewModelItem( viewItem );

				// Associate the new view item.
				// 
				viewItem = dataItem as TViewItem;
				this.SetDataItem( viewItem, dataItem );
				idToken.AssociateViewModelItem( viewItem );

				//Debug.Assert( false, "Does the data source have duplicate items with the same id's ?" );
			}

			return viewItem;
		}

		#endregion // GetViewItem

		#region GetViewItemWithId

		public TViewItem GetViewItemWithId( object id )
		{
			IdToken idToken = IdToken.CreateFromId( id );

			IdToken storedToken;
			if ( _viewItemsCache.TryGetValue( idToken, out storedToken ) )
				return storedToken.GetFirstAssociatedViewItem( );

			return null;
		}

		#endregion // GetViewItemWithId

		#region IsAddNewItem

		public bool IsAddNewItem( TViewItem item )
		{
			return _addNewOperation_ViewItem == item;
		}

		#endregion // IsAddNewItem

		#region OnItemDeleted

		public void OnItemDeleted( object dataItem )
		{
			IdToken idToken = this.GetIdToken( dataItem, false, false );
			if ( null != idToken )
			{
				idToken.HookUnhookIntoDataItem( _dataListEventListener, false );
				IEnumerable<TViewItem> viewItems = idToken.GetAssociatedViewItems( );
				if ( null != viewItems )
				{
					foreach ( TViewItem ii in viewItems )
						this.OnViewItemDeleted( ii );
				}
			}
		}

		#endregion // OnItemDeleted

		#region SetDataItem

		public void SetDataItem( TViewItem viewItem, object newDataItem )
		{
			IdToken origIdToken = this.GetIdToken( viewItem );
			IdToken newIdToken = null != newDataItem ? this.GetIdToken( newDataItem, true, true ) : null;

			if ( origIdToken == newIdToken )
			{
				if ( null != newIdToken )
					newIdToken.UpdateDataModelObject( newDataItem, this );
			}
			else
			{
				if ( null != origIdToken )
					origIdToken.DeassociateViewModelItem( viewItem );

				this.SetIdToken( viewItem, newIdToken );

				if ( null != newIdToken )
				{
					newIdToken.AssociateViewModelItem( viewItem );

					// If the view item is a new item then initialize it with the storage.
					// 
					if ( null == origIdToken )
						this.InitializeNewViewItem( viewItem );
				}

				if ( null != origIdToken )
					this.NotifyDataItemChanged( viewItem );
			}
		}

		#endregion // SetDataItem

		#region UpdateResultDataList_Add

		/// <summary>
		/// This method is called whenever we get an add notification from the data source to update an individual query
		/// result list with the added item. It is also called whenever an item's property value changes and query criteria
		/// is re-evaluated and the item is to be added to a matching result data list. Note that at this point the query 
		/// criteria is already evaluated and determined that the added item should be in the result data list.
		/// </summary>
		/// <param name="resultDataList">Query result's list, which contains the data items.</param>
		/// <param name="dataItem">Data item that is to be added.</param>
		/// <param name="checkIfAlreadyExists">Whether to check if the item already exists in the result data list.</param>
		/// <param name="replaceExistingItem">If there's an existing item with the same id value then this parameter specifies
		/// whether to replace it with the passed in one or leave the original item there and do nothing.</param>
		public void UpdateResultDataList_Add( IList resultDataList, object dataItem, bool checkIfAlreadyExists, bool replaceExistingItem )
		{
			int index = -1;
			object existingItem = null;
			if ( checkIfAlreadyExists )
				existingItem = this.GetCorrespondingDataItem( resultDataList, dataItem, out index );

			if ( index < 0 )
				resultDataList.Add( dataItem );
			else if ( replaceExistingItem && existingItem != dataItem )
				resultDataList[index] = dataItem;
		}

		#endregion // UpdateResultDataList_Add

		#region UpdateResultDataList_Remove

		/// <summary>
		/// Called to remove the specified data item from the result data list.
		/// </summary>
		/// <param name="resultDataList">Query result's list, which contains the data items.</param>
		/// <param name="dataItem">Data item that is to be removed.</param>
		public void UpdateResultDataList_Remove( IList resultDataList, object dataItem )
		{
			int index = -1;
			object existingItem = this.GetCorrespondingDataItem( resultDataList, dataItem, out index );
			if ( index >= 0 )
				resultDataList.RemoveAt( index );
		}

		#endregion // UpdateResultDataList_Remove

		#endregion // Public Methods

		#region Protected Methods

		#region GetIdToken

		protected abstract IdToken GetIdToken( TViewItem viewItem );

		#endregion // GetIdToken

		#region SetIdToken

		protected abstract void SetIdToken( TViewItem viewItem, IdToken idToken );

		#endregion // SetIdToken

		#region OnViewItemDeleted

		protected virtual void OnViewItemDeleted( TViewItem viewItem )
		{
		}

		#endregion // OnViewItemDeleted

		#endregion // Protected Methods

		#region Internal Methods

		#region AllocateViewItem

		internal abstract TViewItem AllocateViewItem( );

		#endregion // AllocateViewItem

		#region CreateIdOverride

		internal virtual object CreateIdOverride( TViewItem viewItem, object dataItem )
		{
			return null;
		}

		#endregion // CreateIdOverride

		#region GetIdToken

		internal IdToken GetIdToken( object dataItem, bool addToCache, bool updateDataItemOnIdToken )
		{
			IdToken tmpKey = new IdToken( dataItem, this, false );
			IdToken idToken;

			if ( _viewItemsCache.TryGetValue( tmpKey, out idToken ) )
			{
				if ( updateDataItemOnIdToken )
					idToken.UpdateDataModelObject( dataItem, this );
			}
			else if ( addToCache )
			{
				_viewItemsCache[tmpKey] = idToken = tmpKey;

				// Since we passed false for hookIntoDataItem when we constructed id token above,
				// we need to make sure it hooks into the data item.
				// 
				idToken.HookUnhookIntoDataItem( _dataListEventListener, true );
			}

			return idToken;
		}

		#endregion // GetIdToken

		#region InitializeNewViewItem

		internal abstract void InitializeNewViewItem( TViewItem viewItem );

		#endregion // InitializeNewViewItem

		#region NotifyDataItemChanged

		internal abstract void NotifyDataItemChanged( TViewItem viewItem );

		#endregion // NotifyDataItemChanged

		#endregion // Internal Methods

		#region Private Methods

		#region CreateNewID

		private string CreateNewID( )
		{
			return Guid.NewGuid( ).ToString( );
		}

		#endregion // CreateNewID

		#region GetCorrespondingDataItem

		private object GetCorrespondingDataItem( IList list, object item, out int index )
		{
			object itemId = null != item ? _idField.GetValue( item ) : null;
			bool isItemIdEmpty = CoreUtilities.IsValueEmpty( itemId );

			for ( int i = 0, count = list.Count; i < count; i++ )
			{
				object ii = list[i];
				if ( item == ii || !isItemIdEmpty && null != ii && object.Equals( itemId, _idField.GetValue( ii ) ) )
				{
					index = i;
					return ii;
				}
			}

			index = -1;
			return null;
		}

		#endregion // GetCorrespondingDataItem

		#region VerifyAddNewOperationInProgress

		private void VerifyAddNewOperationInProgress( bool shouldBeInProgress, string method )
		{
			if ( shouldBeInProgress )
			{
				if ( null == _addNewOperation_ViewItem )
					Debug.Assert( false, string.Format( "Invalid call to {0}. No add-new operation is in progress.", method ) );
			}
			else
			{
				if ( null != _addNewOperation_ViewItem )
					Debug.Assert( false, string.Format( "Invalid call to {0}. An add-new operation is already in progress.", method ) );
			}
		}

		#endregion // VerifyAddNewOperationInProgress

		#endregion // Private Methods 

		#endregion // Methods

		#region IViewItemFactory<TViewItem> Interface Implementation

		#region CreateViewItem

		TViewItem IViewItemFactory<TViewItem>.CreateViewItem( object dataItem )
		{
			return this.GetViewItem( dataItem, true );
		}

		#endregion // CreateViewItem

		#region GetDataItemComparisonTokenForRecycling

		object IViewItemFactory<TViewItem>.GetDataItemComparisonTokenForRecycling( object dataItem )
		{
			return ScheduleUtilities.GetObjectForDataItemComparison( dataItem );
		} 

		#endregion // GetDataItemComparisonTokenForRecycling

		#region CreateAddNewViewItem

		TViewItem IViewItemManager<TViewItem>.CreateAddNewViewItem( )
		{
			return this.AllocateViewItem( );
		} 

		#endregion // CreateAddNewViewItem

		#region IsAddNewOperationInProgress

		bool IViewItemManager<TViewItem>.IsAddNewOperationInProgress
		{
			get
			{
				return null != _addNewOperation_ViewItem;
			}
		} 

		#endregion // IsAddNewOperationInProgress

		#region BeginAddNewOperation

		void IViewItemManager<TViewItem>.BeginAddNewOperation( TViewItem viewItem )
		{
			this.VerifyAddNewOperationInProgress( false, "BeginAddNewOperation" );

			_addNewOperation_ViewItem = viewItem;
		} 

		#endregion // BeginAddNewOperation

		#region CancelAddNewOperation

		void IViewItemManager<TViewItem>.CancelAddNewOperation( )
		{
			this.VerifyAddNewOperationInProgress( true, "CancelAddNewOperation" );

			_addNewOperation_ViewItem = null;
		} 

		#endregion // CancelAddNewOperation

		#region EndAddNewOperation

		void IViewItemManager<TViewItem>.EndAddNewOperation( object dataItem )
		{
			this.VerifyAddNewOperationInProgress( true, "EndAddNewOperation" );

			this.AssociateDataItemToAddNewItem( dataItem );
			_addNewOperation_ViewItem = null;
		} 

		#endregion // EndAddNewOperation

		#endregion // IViewItemFactory<TViewItem> Interface Implementation
	} 

	#endregion // ViewItemManager<TViewItem> Class

}


namespace Infragistics.Controls.Schedules.Services



{
	#region ScheduleViewItemManagerBase

	internal abstract class ScheduleViewItemManagerBase<TViewItem> : ViewItemManager<TViewItem>
	where TViewItem : class
	{
		protected readonly IPropertyStorage<TViewItem, int> _propertyStorage;

		internal ScheduleViewItemManagerBase( IPropertyStorage<TViewItem, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
			: base( idField, dataListEventListener )
		{
			CoreUtilities.ValidateNotNull( propertyStorage );
			_propertyStorage = propertyStorage;
		}
	} 

	#endregion // ScheduleViewItemManagerBase

	#region ResourceItemManager

	internal class ResourceItemManager : ScheduleViewItemManagerBase<Resource>
	{
		private bool _hasResourceCalendarsDataSource;

		public ResourceItemManager( IPropertyStorage<Resource, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
			: base( propertyStorage, idField, dataListEventListener )
		{
		}

		internal bool HasResourceCalendarsDataSource
		{
			get
			{
				return _hasResourceCalendarsDataSource;
			}
			set
			{
				_hasResourceCalendarsDataSource = value;
			}
		}

		internal override Resource AllocateViewItem( )
		{
			return new Resource( );
		}

		protected override IdToken GetIdToken( Resource resource )
		{
			return resource.IdToken;
		}

		protected override void SetIdToken( Resource resource, IdToken idToken )
		{
			resource.IdToken = idToken;
		}

		internal override void NotifyDataItemChanged( Resource resource )
		{
			IdToken idToken = resource.IdToken;
			Debug.Assert( null != idToken );
			if ( null != idToken )
				resource.RaisePropertyChangedEvent( string.Empty );
		}

		internal override void InitializeNewViewItem( Resource resource )
		{
			// Pass along for the 'createDefaultCalendar' parameter a value based on whether we have calendars
			// data source. If calendars data source is not provided then the resource should create a default
			// primary calendar. Otherwise since the calendars data source is provided, we will assume that 
			// every resource will have a corresponding calendar in the calendars data source and even if there
			// isn't, we should not create a default one for the resource since we don't know if the calendars
			// data source will get initialized with such a calendar later on.
			// 
			resource.Initialize( _propertyStorage, !this.HasResourceCalendarsDataSource );
		}
	} 

	#endregion // ResourceItemManager

	#region ResourceCalendarItemManager

	internal class ResourceCalendarItemManager : ScheduleViewItemManagerBase<ResourceCalendar>
	{
		public ResourceCalendarItemManager( IPropertyStorage<ResourceCalendar, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
			: base( propertyStorage, idField, dataListEventListener )
		{
		}

		internal override ResourceCalendar AllocateViewItem( )
		{
			return new ResourceCalendar( );
		}

		protected override IdToken GetIdToken( ResourceCalendar calendar )
		{
			return calendar.IdToken;
		}

		protected override void SetIdToken( ResourceCalendar calendar, IdToken idToken )
		{
			calendar.IdToken = idToken;
		}

		internal override void NotifyDataItemChanged( ResourceCalendar calendar )
		{
			IdToken idToken = calendar.IdToken;
			Debug.Assert( null != idToken );
			if ( null != idToken )
				calendar.RaisePropertyChangedEvent( string.Empty );
		}

		internal override void InitializeNewViewItem( ResourceCalendar calendar )
		{
			calendar.Initialize( _propertyStorage );
		}
	} 

	#endregion // ResourceCalendarItemManager

	#region ActivityItemManager

	internal class ActivityItemManager<TActivity> : ScheduleViewItemManagerBase<ActivityBase>
		where TActivity : ActivityBase, new( )
	{
		private readonly IScheduleDataConnector _connector;

		internal ActivityItemManager( IScheduleDataConnector connector, IPropertyStorage<ActivityBase, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
			: base( propertyStorage, idField, dataListEventListener )
		{
			CoreUtilities.ValidateNotNull( connector );
			_connector = connector;
		}

		internal override ActivityBase AllocateViewItem( )
		{
			return new TActivity( );
		}

		protected override IdToken GetIdToken( ActivityBase activity )
		{
			return activity.IdToken;
		}

		protected override void SetIdToken( ActivityBase activity, IdToken idToken )
		{
			activity.IdToken = idToken;
		}

		internal override object CreateIdOverride( ActivityBase activity, object dataItem )
		{
			// When making an occurrence a variance and committing it, we must use the id as it was
			// generated by the OccurrenceId object. The id value is what we use to match the variances
			// to the associated recurring activity when the data is loaded the next time.
			// 
			if ( activity.IsOccurrence )
				return activity.Id;

			return null;
		}

		internal override void NotifyDataItemChanged( ActivityBase activity )
		{
			IdToken idToken = activity.IdToken;
			Debug.Assert( null != idToken );
			if ( null != idToken )
				activity.RaisePropertyChangedEvent( string.Empty );
		}

		internal override void InitializeNewViewItem( ActivityBase activity )
		{
			activity.Initialize( _connector, _propertyStorage );
		}

		protected override void OnViewItemDeleted( ActivityBase activity )
		{
			activity.RaisePropertyChangedEvent( "IsDeleted" );
		}
	} 

	#endregion // ActivityItemManager

	#region ActivityCategoryItemManager

	// SSP 1/5/11 - NAS11.1 Activity Categories
	// 

	internal class ActivityCategoryItemManager : ScheduleViewItemManagerBase<ActivityCategory>
	{
		public ActivityCategoryItemManager( IPropertyStorage<ActivityCategory, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
			: base( propertyStorage, idField, dataListEventListener )
		{
		}

		internal override ActivityCategory AllocateViewItem( )
		{
			return new ActivityCategory( );
		}

		protected override IdToken GetIdToken( ActivityCategory category )
		{
			return category.IdToken;
		}

		protected override void SetIdToken( ActivityCategory category, IdToken idToken )
		{
			category.IdToken = idToken;
		}

		internal override void NotifyDataItemChanged( ActivityCategory category )
		{
			IdToken idToken = category.IdToken;
			Debug.Assert( null != idToken );
			if ( null != idToken )
				category.RaisePropertyChangedEvent( string.Empty );
		}

		internal override void InitializeNewViewItem( ActivityCategory category )
		{
			category.Initialize( _propertyStorage );
		}
	}

	#endregion // ActivityCategoryItemManager

	#region ProjectItemManager

	// SSP 1/6/12 - NAS12.1 XamGantt
	// 

	//internal class ProjectItemManager : ScheduleViewItemManagerBase<Project>
	//{
	//    public ProjectItemManager( IPropertyStorage<Project, int> propertyStorage, IFieldValueAccessor idField, DataListEventListener dataListEventListener )
	//        : base( propertyStorage, idField, dataListEventListener )
	//    {
	//    }

	//    internal override Project AllocateViewItem( )
	//    {
	//        return new Project( );
	//    }

	//    protected override IdToken GetIdToken( Project project )
	//    {
	//        return project.IdToken;
	//    }

	//    protected override void SetIdToken( Project project, IdToken idToken )
	//    {
	//        project.IdToken = idToken;
	//    }

	//    internal override void NotifyDataItemChanged( Project project )
	//    {
	//        IdToken idToken = project.IdToken;
	//        Debug.Assert( null != idToken );
	//        if ( null != idToken )
	//            project.RaisePropertyChangedEvent( string.Empty );
	//    }

	//    internal override void InitializeNewViewItem( Project project )
	//    {
	//        project.Initialize( _propertyStorage );
	//    }
	//}

	#endregion // ProjectItemManager
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