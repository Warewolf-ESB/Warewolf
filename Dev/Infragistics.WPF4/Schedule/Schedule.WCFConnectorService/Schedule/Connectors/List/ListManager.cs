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
using System.Threading;


#pragma warning disable 0649



using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services






{
	#region ListManager Class

	internal abstract class ListManager<TViewItem, TMappingKey> : PropertyChangeNotifierExtended, IEditList<TViewItem>, IListManager
        where TViewItem: class
	{
		#region Nested Data Structures

		#region DataItemPropListener Class

		private class DataItemPropListener : ValueChangeListenerList<TViewItem>, ISupportValueChangeNotifications<TViewItem>, IValueChangeListener<object>
		{
			private Func<object, TViewItem> _dataItemToViewItem;

			internal DataItemPropListener( Func<object, TViewItem> dataItemToViewItem )
			{
				_dataItemToViewItem = dataItemToViewItem;
			}

			void IValueChangeListener<object>.OnValueChanged( object dataItem )
			{
				TViewItem viewItem = _dataItemToViewItem( dataItem );
				if ( null != viewItem )
					this.OnValueChanged( viewItem );
			}
		}

		#endregion // DataItemPropListener Class 

		#endregion // Nested Data Structures

		#region Member Vars

		private IEnumerable _list;
		private IPropertyMappingCollection<TMappingKey> _mappings;
		private LinqQueryManager _linqQueryManager;

		private IMap<TMappingKey, IFieldValueAccessor> _fieldValueAccessors;
		private IMap<TMappingKey, IFieldValueAccessor> _rawFieldValueAccessors;
		private IMap<string, IFieldValueAccessor> _fieldValueAccessorsMetadata;
		private IMap<string, IFieldValueAccessor> _rawFieldValueAccessorsMetadata;
        private IMap<IFieldValueAccessor, ISupportValueChangeNotifications<TViewItem>> _fieldValueNotifiers;
        private IMap<string, IValueChangeListener<object>> _dataItemPropNameToListenerMap;
        private WeakSet<ListQueryResult> _results;
		private VerifyAsyncItemsQueue<ListQueryResult> _pendingDirtyResults;
        private DataListEventListener _listEventListener;
		private GenericListProxy _genericListProxy;
		private ITableProxy _iTableProxy;
		internal Func<object, LinqQueryManager.ILinqStatement, Action<object, IEnumerable, DataErrorInfo>, bool> _performQueryOverride;

		private ListManagerChangeInformation _changeInformation;

		#endregion // Member Vars

		#region Constructor

		internal ListManager( )
		{
			_pendingDirtyResults = new VerifyAsyncItemsQueue<ListQueryResult>( this.MarkCleanPendingDirtyResults, true );
		}

		#endregion // Constructor

		#region Interfaces

		#region IListManager Members

		ListManagerChangeInformation IListManager.ChangeInformation
		{
			get { return this.ChangeInformation; }
		}

		IEnumerable IListManager.List
		{
			get { return this.List; }
		}

		#endregion 

		#endregion  // Interfaces

		#region Properties

		#region Public Properties

		#region AllowAdd

		public bool AllowAdd
		{
			get
			{
				// View item manager has to be present for the list manager to be able to add.
				// 
				return null != this.ViewItemManager
					&& 0 != ( ListCapabilities.Add & this.QueryListCapabilities( ListCapabilities.Add ) );
			}
		}

		#endregion // AllowAdd

		#region AllowEdit

		public bool AllowEdit
		{
			get
			{
				return 0 != ( ListCapabilities.Edit & this.QueryListCapabilities( ListCapabilities.Edit ) );
			}
		}

		#endregion // AllowEdit

		#region AllowRemove

		public bool AllowRemove
		{
			get
			{
				// View item manager has to be present for the list manager to be able to add.
				// 
				return null != this.ViewItemManager
					&& 0 != ( ListCapabilities.Remove & this.QueryListCapabilities( ListCapabilities.Remove ) );
			}
		}

		#endregion // AllowRemove 

		#endregion // Public Properties

		#region Internal Properties

		#region ChangeInformation

		internal ListManagerChangeInformation ChangeInformation
		{
			get { return _changeInformation; }
			set 
			{
				if (_changeInformation == value)
					return;

				_changeInformation = value;

				if (_changeInformation != null)
				{
					// MD 10/4/10 - TFS50092
					// The List may already be set on the list manager when the ChangeInformation is set, so initialize the 
					// cached list capabilities.
					_changeInformation.RecacheListCapabilities(this);

					_changeInformation.VerifyHasList(this);
				}
			}
		} 

		#endregion  // ChangeInformation

        #region HasList

        internal bool HasList
        {
            get
            {
                return null != _list;
            }
        }

        #endregion // HasList

		#region IsListViewItemsList

		/// <summary>
		/// Indicates if the associated data source list contains actual view items.
		/// </summary>
		internal bool IsListViewItemsList
		{
			get
			{
				LinqQueryManager qm = this.LinqQueryManager;
				Type dataItemType = null != qm ? qm.ListItemType : null;
				return null != dataItemType && typeof( TViewItem ).IsAssignableFrom( dataItemType );
			}
		}

		#endregion // IsListViewItemsList

        #region List

		/// <summary>
		/// The data source list.
		/// </summary>
        internal IEnumerable List
		{
			get
			{
				return _list;
			}
			set
			{
				if ( _list != value )
				{
					_list = value;

					this.RaisePropertyChangedEvent( "List" );
				}
			}
		}

		#endregion // List

		#region ListEventListener

		internal DataListEventListener ListEventListener
		{
			get
			{
				if ( null == _listEventListener )
					_listEventListener = new DataListEventListener( 
						this, OnDataListChangedHandler, OnDataItemPropertyChangedHandler, null );

				return _listEventListener;
			}
		} 

		#endregion // ListEventListener

		#region LinqQueryManager

		/// <summary>
		/// Linq query manager used for quering the data source list.
		/// </summary>
		internal LinqQueryManager LinqQueryManager
		{
			get
			{
				if ( null == _linqQueryManager && null != _list )
				{
					Type listItemType = LinqQueryManager.GetListElementType( _list );
					if ( null != listItemType )
						_linqQueryManager = new LinqQueryManager( _list, listItemType );
				}

				return _linqQueryManager;
			}
		}

		#endregion // LinqQueryManager

		#region Mappings

		/// <summary>
		/// Gets or sets the field mappings collection.
		/// </summary>
		internal IPropertyMappingCollection<TMappingKey> Mappings
		{
			get
			{
				return _mappings;
			}
			set
			{
				if ( _mappings != value )
				{
					ScheduleUtilities.ManageListenerHelperObj( ref _mappings, value, this, true );
					
					this.RaisePropertyChangedEvent( "Mappings" );
				}
			}
		}

		#endregion // Mappings

        #region ViewItemFactory

		/// <summary>
		/// Factory if any used to create TViewItem instances for specific data items.
		/// </summary>
        internal virtual IViewItemFactory<TViewItem> ViewItemFactory
        {
            get
            {
                return null;
            }
        }

        #endregion // ViewItemFactory

        #region ViewItemManager

		/// <summary>
		/// View item manager used to manage and track view item objects by their id values.
		/// </summary>
        internal virtual IViewItemManager<TViewItem> ViewItemManager
        {
            get
            {
                return this.ViewItemFactory as IViewItemManager<TViewItem>;
            }
        }

        #endregion // ViewItemManager

		#region UseDefaultMappingsResolved

		internal bool UseDefaultMappingsResolved
		{
			get
			{
				return null != _mappings && _mappings.UseDefaultMappings
					|| this.IsListViewItemsList;
			}
		} 

		#endregion // UseDefaultMappingsResolved

        #endregion // Internal Properties

        #region Private Properties

        #region Results

		/// <summary>
		/// List of results that are actively being managed and whose result lists are updated to reflect
		/// any changes that occur to the data source list.
		/// </summary>
        private WeakSet<ListQueryResult> Results
        {
            get
            {
				if ( null == _results )
					_results = new WeakSet<ListQueryResult>( );

                return _results;
            }
        }

        #endregion // Results

        #endregion // Private Properties

        #endregion // Properties

        #region Methods

        #region Public Methods

		#region AssociateDataItemToAddNewViewItem

		/// <summary>
		/// Creates a new data item and associates it with the specified view item. Note that the data item
		/// is not added to the underlying list. It must be added separately, either via EndEdit call or
		/// manually. Also the underlying list must support adding a data item.
		/// </summary>
		/// <param name="item">View item.</param>
		/// <param name="error">Will be set to an error if any.</param>
		public void AssociateDataItemToAddNewViewItem( TViewItem item, out DataErrorInfo error )
		{
			error = null;

			IViewItemManager<TViewItem> viewItemManager = this.ViewItemManager;
			object dataItem = null != viewItemManager && null != _genericListProxy ? _genericListProxy.CreateNew( out error ) : null;
			if ( null != dataItem )
			{
				viewItemManager.BeginAddNewOperation( item );
				viewItemManager.AssociateDataItemToAddNewItem( dataItem );
			}
			else if ( null == error )
				error = ScheduleUtilities.CreateErrorFromId(item, "LE_UnknownAddError", typeof(TViewItem).Name); //"Unknown error adding."
		}

		#endregion // AssociateDataItemToAddNewViewItem

		#region BeginEdit

		public bool BeginEdit( TViewItem item, out DataErrorInfo error )
		{
			error = null;

			IEditableObject editableObject = this.GetEditableObject( item );
			if ( null != editableObject )
			{
				try
				{
					editableObject.BeginEdit( );
					return true;
				}
				catch ( Exception exception )
				{
					error = new DataErrorInfo( exception );
				}
			}

			return false;
		} 

		#endregion // BeginEdit

		#region CancelEdit

		public bool CancelEdit( TViewItem item, out DataErrorInfo dataError )
		{
			IEditableObject editableObject = this.GetEditableObject( item );
			if ( null != editableObject )
				editableObject.CancelEdit( );

			dataError = null;
			return false;
		}

		#endregion // CancelEdit

		#region CreateNew

		/// <summary>
		/// Creates a new TViewItem instance.
		/// </summary>
		/// <param name="dataError"></param>
		/// <returns></returns>
		public TViewItem CreateNew( out DataErrorInfo dataError )
		{
			dataError = null;

			IViewItemManager<TViewItem> itemManager = this.ViewItemManager;
			TViewItem ret = null != itemManager ? itemManager.CreateAddNewViewItem( ) : default( TViewItem );

			// If item manager is null then it's because Id field mapping has not been provided.
			// 
			if ( null == itemManager || null == ret )
				dataError = ScheduleUtilities.CreateErrorFromId(_list, "LE_MissingMapping", typeof(TViewItem).Name, "Id");//"{1} property mapping must be specified for {0} datasource"."

			return ret;
		}

		#endregion // CreateNew

		#region OnEditEnded

		
		public void OnEditEnded( TViewItem viewItem, out DataErrorInfo error )
		{
			error = null;
			if ( null != _iTableProxy )
				_iTableProxy.SubmitChanges( out error );
		} 

		#endregion // OnEditEnded

		#region EndEdit

		/// <summary>
		/// Commits the specified view item.
		/// </summary>
		/// <param name="viewItem"></param>
		/// <param name="dataError"></param>
		/// <returns></returns>
		public bool EndEdit( TViewItem viewItem, out DataErrorInfo dataError )
		{
			dataError = null;
			IEnumerable list = _list;
			IViewItemManager<TViewItem> viewItemManager = this.ViewItemManager;

			// If the view item has a data item associated with it then call EndEdit on the data item.
			// If the view item is an add-new item then fall through to the logic below that adds a 
			// new item to the underlying data source list.
			// 
			if ( null == viewItemManager || ! this.IsAddNew( viewItem ) )
			{
				IEditableObject editableObject = this.GetEditableObject( viewItem );
				if ( null != editableObject )
				{
					try
					{
						editableObject.EndEdit( );
					}
					catch ( Exception exception )
					{
						dataError = new DataErrorInfo( exception );
					}
				}

				if ( null == dataError )
					this.OnEditEnded( viewItem, out dataError );

				return true;
			}

			// Perform add-new operation on the data source list.
			// 

			IEditableCollectionView editableCollectionView = list as IEditableCollectionView;


			IBindingList bindingList = list as IBindingList;


			if ( ! viewItemManager.IsAddNewItem( viewItem ) )
				viewItemManager.BeginAddNewOperation( viewItem );

			// If the view item already has a data item then that means the view item was associated 
			// with a new data item using the AssociateDataItemToAddNewViewItem call, in which case 
			// all we have to do is add it to the list.
			// 
			object dataItem = viewItemManager.GetDataItem( viewItem );

			// This is to handle cases where the data source doesn't raise notification.
			// 
			this.ListEventListener.BeginAdd( );

			// Check if the data list is a list of activity objects in which case simply add the new activity to the list.
			// 
			if ( this.IsListViewItemsList )
			{
				try
				{
					dataItem = viewItem;

					if ( null != _genericListProxy )
						_genericListProxy.Add( viewItem, out dataError );
				}
				catch ( Exception exception )
				{
					dataError = new DataErrorInfo( exception );
				}
			}
			else
			{
				try
				{
					// If we already have a data item then don't call AddNew since that will create
					// a new data item. Instead simply add it to the list further below.
					// 
					if ( null == dataItem && null != editableCollectionView )
					{
						dataItem = editableCollectionView.AddNew( );

						// SSP 1/10/12 TFS89492
						// We also need to call CommitNew to commit the data item to the editable collection view.
						// 
						if ( null != dataItem )
						{
							viewItemManager.AssociateDataItemToAddNewItem( dataItem );
							editableCollectionView.CommitNew( );
						}
					}

					else if ( null == dataItem && null != bindingList )
						dataItem = bindingList.AddNew( );

					else
					{
						// Create a new data item if we didn't already have it.
						// 
						if ( null == dataItem && null != _genericListProxy )
						{
							dataItem = _genericListProxy.CreateNew( out dataError );
							viewItemManager.AssociateDataItemToAddNewItem( dataItem );
						}

						if ( null != dataItem )
						{
							// If the list is ITable then add using its InsertOnSubmit method.
							// 
							if ( null != _iTableProxy )
								_iTableProxy.Insert( dataItem, true, out dataError );
							// If the list implements IList or IList<T> then simply add the data item to the list.
							// 
							else if ( null != _genericListProxy )
								_genericListProxy.Add( dataItem, out dataError );
							else
								dataError = ScheduleUtilities.CreateErrorFromId( viewItem, "LE_AddNotSupportedByDataSource", typeof( TViewItem ).Name );//"Underlying data source list doesn't support add operation."
						}
					}
				}
				catch ( Exception exception )
				{
					dataError = new DataErrorInfo( exception );
				}
			}

			if ( null == dataError && null == dataItem )
				dataError = ScheduleUtilities.CreateErrorFromId(viewItem, "LE_AddOperationFailed", typeof(TViewItem).Name);//"Underlying data source list failed to perform add operation."

			// This handles the case where the data source did not raise a notification.
			// 
			this.ListEventListener.EndAdd( dataItem, null == dataError );

			// Let item manager initialize the view item and associate the new data item to it.
			// 
			if ( null == dataError )
			{
				viewItemManager.EndAddNewOperation( dataItem );
				this.VerifyItemsInCleanResults( new object[] { dataItem }, null );
				return true;
			}
			else
			{
				// If there was an error, cancel the add-new operation.
				// 
				viewItemManager.CancelAddNewOperation( );
				return false;
			}
		}

		#endregion // EndEdit

		#region EnsureViewItemInitialized

		// SSP 11/1/10 TFS58839
		// 
		/// <summary>
		/// When view items are used as data items in the data source (in other words the data source
		/// comprises of view items) then when a view item is used directly, we may need to initialize
		/// the view item (the same way ViewList would do it). For example, in the case of activity,
		/// activity's owning resource and calendar need to be initialized based on the id properties.
		/// This initialization is done by the view list however if a view item is used directly
		/// by accessing it from the data source, then we need to make sure it's initialized.
		/// </summary>
		/// <param name="item"></param>
		public void EnsureViewItemInitialized( TViewItem item )
		{
			if ( this.IsListViewItemsList )
			{
				var viewItemManager = this.ViewItemManager;
				if ( null != viewItemManager )
					viewItemManager.GetViewItem( item, true );
			}
		}

		#endregion // EnsureViewItemInitialized

		#region IsAddNew

		public bool IsAddNew( TViewItem item )
		{
			IViewItemManager<TViewItem> viewItemManager = this.ViewItemManager;
			return null != viewItemManager
				&& ( viewItemManager.IsAddNewItem( item ) || null == viewItemManager.GetDataItem( item ) );
		}

		#endregion // IsAddNew		

		#region GetFieldValueAccessor

		public IFieldValueAccessor GetRawFieldValueAccessor( TMappingKey field )
		{
			this.EnsureFieldValueAccessorsVerified( );

			return _rawFieldValueAccessors[field];
		}

		public IFieldValueAccessor GetFieldValueAccessor( TMappingKey field )
		{
			ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifier;
			return this.GetFieldValueAccessor( field, out fieldValueChangeNotifier );
		}

		/// <summary>
		/// Gets field value accessor that will convert the values to the specified target type.
		/// </summary>
		/// <param name="field">Identifies the field.</param>
		/// <param name="convertTargetType">Target type to convert the values to.</param>
		/// <returns>IFieldValueAccessor instance.</returns>
		public IFieldValueAccessor GetFieldValueAccessorConverted( TMappingKey field, Type convertTargetType )
		{
			IFieldValueAccessor accessor = this.GetFieldValueAccessor(field);
			if (accessor == null)
				return null;

			ConverterInfo converter = this.GetConverter( field );

			IFieldValueAccessor fva = new ConvertBackFieldValueAccessor(accessor, converter, convertTargetType);

			return fva;
		}

		public IFieldValueAccessor GetFieldValueAccessor( TMappingKey field, out ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifier )
		{
			this.EnsureFieldValueAccessorsVerified( );

			IFieldValueAccessor accessor = _fieldValueAccessors[field];
			fieldValueChangeNotifier = null != accessor ? _fieldValueNotifiers[accessor] : null;

			return accessor;
		}

		public IFieldValueAccessor GetMetadataFieldValueAccessor( string metadataField )
		{
			ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifier;
			return this.GetMetadataFieldValueAccessor( metadataField, out fieldValueChangeNotifier );
		}

		public IFieldValueAccessor GetMetadataFieldValueAccessor( string metadataField, out ISupportValueChangeNotifications<TViewItem> fieldValueChangeNotifier )
		{
			this.EnsureFieldValueAccessorsVerified( );

			IFieldValueAccessor accessor = _fieldValueAccessorsMetadata[metadataField];
			fieldValueChangeNotifier = null != accessor ? _fieldValueNotifiers[accessor] : null;

			return accessor;
		}

		#endregion // GetFieldValueAccessor
        
		#region GetMappedField

		public bool GetMappedField( TMappingKey field, out string mappedField, out DataErrorInfo error )
		{
            error = null;
            mappedField = this.GetMappedFieldIfAny( field );
            if ( string.IsNullOrEmpty( mappedField ) )
            {
				error = ScheduleUtilities.CreateErrorFromId(field, "LE_MissingMapping", typeof( TViewItem).Name , field);//"{1} property mapping must be specified for {0} datasource"."
				return false;
            }

            return true;
        }

		#endregion // GetMappedField

		#region IsEditTransactionSupported

		/// <summary>
		/// Returns true if BeginEdit, CancelEdit and EndEdit are supported.
		/// </summary>
		public bool IsEditTransactionSupported( TViewItem viewItem )
		{
			return null != this.GetEditableObject( viewItem );
		}

		#endregion // IsEditTransactionSupported

		#region RecreateAllViewItems

		/// <summary>
		/// Discards all the existing view items and dirties all the query results so the view items get re-created.
		/// </summary>
		public virtual void RecreateAllViewItems( )
		{
			var viewItemManager = this.ViewItemManager;
			if ( null != viewItemManager )
				viewItemManager.DiscardCachedItems( );

			this.DirtyAllQueryResults( true, true );
		}

		#endregion // RecreateAllViewItems

		#region ReevaluateQuery

		/// <summary>
		/// Re-evaluates the specified query result and updates its result list.
		/// </summary>
		/// <param name="result"></param>
		public void ReevaluateQuery( ListQueryResult result )
		{
			this.EvaluateQueryOverride( result );
		}

		#endregion // ReevaluateQuery

		#region Remove

		/// <summary>
		/// Removes the specified view item.
		/// </summary>
		/// <param name="viewItem"></param>
		/// <param name="dataError"></param>
		/// <returns></returns>
		public bool Remove( TViewItem viewItem, out DataErrorInfo dataError )
		{
			dataError = null;
			IEnumerable list = _list;
			IViewItemManager<TViewItem> viewItemManager = this.ViewItemManager;

			IEditableCollectionView editableCollectionView = list as IEditableCollectionView;


			IBindingList bindingList = list as IBindingList;

			// This is to handle cases where the data source doesn't raise notification.
			// 
			this.ListEventListener.BeginRemove( );

			// Check if the data list is a list of activity objects in which case simply add the new activity to the list.
			// 
			object dataItem;
			if ( this.IsListViewItemsList )
			{
				dataItem = viewItem;

				try
				{
					if ( null != _genericListProxy )
						_genericListProxy.Remove( viewItem, out dataError );
				}
				catch ( Exception exception )
				{
					dataError = new DataErrorInfo( exception );
				}
			}
			else
			{
				dataItem = viewItemManager.GetDataItem( viewItem );
				if ( null != dataItem )
				{
					try
					{
						// Add a new data item to the list.
						// 
						if ( null != editableCollectionView )
							editableCollectionView.Remove( dataItem );

						else if ( null != bindingList )
							bindingList.Remove( dataItem );

						else if ( null != _iTableProxy )
							_iTableProxy.Remove( dataItem, true, out dataError );
						else if ( null != _genericListProxy )
							// SSP 1/10/12 TFS89492
							// Don't return here. This was a typo. We need to process ListEventListener.EndRemove call below.
							// 
							//return _genericListProxy.Remove( dataItem, out dataError );
							_genericListProxy.Remove( dataItem, out dataError );
					}
					catch ( Exception exception )
					{
						dataError = new DataErrorInfo( exception );
					}
				}
				else
				{
					dataError = ScheduleUtilities.CreateErrorFromId(viewItem, "LE_NoDataItemForViewItem", typeof(TViewItem).Name);//"The view item doesn't have an underlying data item and thus can't be removed from the data source."
				}
			}

			// This is to handle cases where the data source doesn't raise notification.
			// 
			this.ListEventListener.EndRemove( dataItem, null == dataError );

			return null == dataError;
		}

		#endregion // Remove

		#endregion // Public Methods

		#region Protected Methods

        #region CreateFieldValueAccessorOverride

        protected virtual IFieldValueAccessor CreateFieldValueAccessorOverride( TMappingKey field )
        {
            return null;
        }

        #endregion // CreateFieldValueAccessorOverride

		#region CreateLinqStatement

		/// <summary>
        /// This method is called to create linq condition from a list specific query result, like ActivityQuery for the
        /// activity list manager.
        /// </summary>
		/// <param name="result">Query result object that was passed into the <see cref="PerformQuery"/> method, from which
		/// the derived list manager is to create the corresponding linq condition.</param>
        /// <param name="error">This out param will be set if an error occurs.</param>
        /// <returns>Returns linq condition derived from the specified listSpecificQueryInfo object.</returns>
        protected virtual LinqQueryManager.ILinqStatement CreateLinqStatement( ListQueryResult result, out DataErrorInfo error )
        {
			error = ScheduleUtilities.CreateErrorFromId(null, "LE_MustImplementLinq", typeof(TViewItem).Name);//"Derived list manager needs to override CreateLinqStatement method."
			return null;
        }

		#endregion // CreateLinqStatement

		#region CreateViewList

		protected virtual IList CreateViewList( ListQueryResult result, IList dataList, IList oldViewList )
		{
			IViewItemFactory<TViewItem> viewItemFactory = this.ViewItemFactory;

			ViewList<TViewItem> viewList = oldViewList as ViewList<TViewItem>;

			if ( null == viewList )
				viewList = null != viewItemFactory
					? new ViewList<TViewItem>( dataList, viewItemFactory )
					: null;
			else
				viewList.SourceItems = dataList;

			return viewList;
		}

		#endregion // CreateViewList

		#region EnsureFieldValueAccessorsVerified

		protected bool EnsureFieldValueAccessorsVerified( )
		{
			if ( null == _fieldValueAccessors )
			{
				this.VerifyFieldValueAccessors( );
				return true;
			}

			return false;
		}

		#endregion // EnsureFieldValueAccessorsVerified

		#region EvaluateQueryOverride

		protected virtual void EvaluateQueryOverride( ListQueryResult result )
		{
			DataErrorInfo error = null;

			LinqQueryManager.ILinqStatement linqStatement = null;

			// If there's no list then initialize result with no data.
			// 
			if ( this.HasList )
			{
				// If Id field mapping hasn't been provided (indicated by null ViewItemManager), then return false.
				// 
				if ( null == error && null == this.ViewItemManager )
					error = ScheduleUtilities.CreateErrorFromId(null, "LE_MissingMapping", typeof(TViewItem).Name, "Id");//"{1} property mapping must be specified for {0} datasource"."

				if ( null == error )
					linqStatement = this.CreateLinqStatement( result, out error );
			}

			// Note that it is valid for CreateLinqStatement to return null, in which case we should
			// consider the result to be an empty list.
			// 
			result.InitializeLinqStatement( linqStatement );
			if ( null == error && null != linqStatement )
			{
				if ( null == _performQueryOverride || !_performQueryOverride( result, linqStatement, this.ProvideQueryResultData ) )
				{
					IEnumerable data = this.PerformQueryHelper( linqStatement, out error );
					this.ProvideQueryResultData( result, data, error );
				}
			}
			else
			{
				result.Initialize( null, null, error, true );
			}
		}

		#endregion // EvaluateQueryOverride

		#region GetAllMappingKeys

		/// <summary>
        /// Returns all potential mapping keys.
        /// </summary>
        /// <returns></returns>
        protected abstract IEnumerable<TMappingKey> GetAllMappingKeys( );

        #endregion // GetAllMappingKeys

        #region GetConverter

        protected virtual ConverterInfo GetConverter( TMappingKey field )
        {
            PropertyMappingBase<TMappingKey> mapping = null != _mappings ? _mappings.GetItem( field ) : null;
            return null != mapping ? mapping._converterInfo : null;
        } 

        #endregion // GetConverter

		#region GetConverterForMetadataField

		protected virtual ConverterInfo GetConverterForMetadataField( string metadataKey )
		{
			MetadataPropertyMappingCollection metadataMappings = null != _mappings ? _mappings.MetadataPropertyMappings : null;
			MetadataPropertyMapping mapping = null != metadataMappings ? metadataMappings.GetItem( metadataKey ) : null;
			return null != mapping ? mapping._converterInfo : null;
		}

		#endregion // GetConverterForMetadataField

		#region GetDefaultPropertyMapping

		protected virtual string GetDefaultPropertyMapping( TMappingKey key )
        {
            return null;
        }

        #endregion // GetDefaultPropertyMapping

		#region GetViewItemFromDataItem

		protected virtual TViewItem GetViewItemFromDataItem( object dataItem )
		{
			IViewItemManager<TViewItem> manager = this.ViewItemManager;
			return null != manager ? manager.GetViewItem( dataItem, false ) : null;
		}

		#endregion // GetViewItemFromDataItem

        #region ManageQueryResult

        /// <summary>
        /// Manages query results so any changes made to the list manager, like list or mappings being changed, will
        /// cause the list manager to re-evaluate the result and re-initialize the result with the new set of
        /// queried data.
        /// </summary>
        /// <param name="result">Result to manage.</param>
        protected void ManageQueryResult( ListQueryResult result )
        {
            CoreUtilities.ValidateNotNull( result );
            this.Results.Add( result );
        } 

        #endregion // ManageQueryResult

		#region OnDataItemPropertyChanged

		protected virtual void OnDataItemPropertyChanged( object dataItem, string propName )
		{
			if (_changeInformation != null)
				_changeInformation.OnListChanged(dataItem, ListChangeType.ChangeItem);

			this.EnsureFieldValueAccessorsVerified( );

			if ( string.IsNullOrEmpty( propName ) )
			{
				foreach ( string ii in _dataItemPropNameToListenerMap )
					_dataItemPropNameToListenerMap[ii].OnValueChanged( dataItem );
			}
			else
			{
				IValueChangeListener<object> vcl = _dataItemPropNameToListenerMap[propName];
				if ( null != vcl )
					vcl.OnValueChanged( dataItem );
			}
		}

		#endregion // OnDataItemPropertyChanged

		#region OnDataListChanged

		protected virtual void OnDataListChanged( DataListEventListener listener, DataListChangeInfo info )
		{
			if (_changeInformation != null)
				_changeInformation.OnDataListChanged(info);

			if ( null == _results )
				return;

			bool processAsReset = false;

			switch ( info._changeType )
			{
				case DataListChangeInfo.ChangeType.Add:
				case DataListChangeInfo.ChangeType.Remove:
					{
						bool isAdd = DataListChangeInfo.ChangeType.Add == info._changeType;
						IViewItemManager<TViewItem> itemManager = this.ViewItemManager;

						if ( isAdd )
						{
							// If we are in the middle of add-new operation then skip processing the notification.
							// The list manager will verify item is in the list when the add-new operation is
							// completed.
							// 
							if ( null != itemManager && itemManager.IsAddNewOperationInProgress )
								break;
						}

						IList items = isAdd ? info.NewItems : info.OldItems;
						if ( null != items )
						{
							foreach ( ListQueryResult ii in _results )
							{
								// If the query result itself processes the change then it will return false and therefore
								// we shouldn't process it here.
								// 
								if ( ii.ProcessChangeEvent( info ) )
									this.VerifyItemsInResult( ii, items, isAdd );
							}

							// Notify the item managers when items get deleted so they can raise IsDeleted property change
							// notification so the selected activities collection can remove the associated activity from
							// itself.
							// 
							if ( !isAdd )
							{
								foreach ( object dataItem in items )
									itemManager.OnItemDeleted( dataItem );
							}
						}
						// If the list is a IBindingList, it's list changed event args doesn't have the item that was removed.
						// It simply has the index at which the item was removed however from that we can't get hold of a list
						// object since it has already been deleted.
						// 
						else
							processAsReset = true;
					}
					break;
				case DataListChangeInfo.ChangeType.Move:
					// We shouldn't have to take any action with move since we don't rely on ordering of the 
					// the data items in the data list.
					// 
					break;
				
				case DataListChangeInfo.ChangeType.Replace:
				case DataListChangeInfo.ChangeType.Reset:
					processAsReset = true;
					break;
				case DataListChangeInfo.ChangeType.PropertyDescriptorAdded:
				case DataListChangeInfo.ChangeType.PropertyDescriptorRemoved:
				case DataListChangeInfo.ChangeType.PropertyDescriptorChanged:
					this.DirtyFieldValueAccessors( );
					break;
				default:
					Debug.Assert( false, "Unknown type of list change type." );
					break;
			}

			if ( processAsReset )
				this.DirtyAllQueryResults( false, true );
		}

		#endregion // OnDataListChanged

		#region OnError

		protected abstract void OnError( DataErrorInfo error );

		#endregion // OnError

		#region OnSubObjectPropertyChanged

		internal override void OnSubObjectPropertyChanged( object sender, string propName, object extraInfo )
		{
			base.OnSubObjectPropertyChanged( sender, propName, extraInfo );

			bool mappingsChanged = false;
			bool listChanged = false;

			if ( this == sender )
			{
				switch ( propName )
				{
					case "List":
						{
							if (_changeInformation != null)
							{
								_changeInformation.OnListChanged(null, ListChangeType.Reset);
								_changeInformation.VerifyHasList(this);

								// MD 10/4/10 - TFS50092
								// Updated this method so we don't have to pass in the same capabilities from all places it is called.
								//_changeInformation.RecacheListCapabilities(this.QueryListCapabilities(
								//    ListCapabilities.Add |
								//    ListCapabilities.Edit |
								//    ListCapabilities.Remove));
								_changeInformation.RecacheListCapabilities(this);
							}

							listChanged = true;
							_linqQueryManager = null;
							_genericListProxy = null;
							_iTableProxy = null;

							IEnumerable list = this.List;
							if ( null != list )
							{
								this.ListEventListener.List = list;

								_genericListProxy = GenericListProxy.Create( list, LinqQueryManager.GetListElementType( list ) );
								_iTableProxy = ITableProxy.Create( list );
							}
						}
						break;
					case "Mappings":
						mappingsChanged = true;
						break;
				}
			}
			else if ( _mappings == sender )
			{
				mappingsChanged = true;
			}

			// If mappings are changed or list is changed to a different list then property descriptors
			// may change and therefore we need to reget the property descriptors.
			// 
			if ( mappingsChanged || listChanged )
			{
				this.DirtyFieldValueAccessors( );
			}
		}

		#endregion // OnSubObjectPropertyChanged

		#region PerformQuery

		/// <summary>
		/// Performs query and returns the result object. <b>NOTE:</b> CreateLinqCondition method must be
		/// overridden to provide logic for converting the specified listSpecificQueryInfo object into
		/// linq query conditions.
		/// </summary>
		/// <param name="result">Performs query referenced by this result.</param>
		/// <param name="manageResult"></param>
		/// <returns></returns>
		protected void PerformQuery( ListQueryResult result, bool manageResult )
		{
			this.ReevaluateQuery( result );

			if ( manageResult )
				this.ManageQueryResult( result );
		}

		#endregion // PerformQuery

		#region VerifyFieldValueAccessors

		protected virtual void VerifyFieldValueAccessors( )
		{
			_fieldValueAccessors = MapsFactory.CreateMapHelper<TMappingKey, IFieldValueAccessor>( );
			_rawFieldValueAccessors = MapsFactory.CreateMapHelper<TMappingKey, IFieldValueAccessor>( );
			_fieldValueNotifiers = MapsFactory.CreateMapHelper<IFieldValueAccessor, ISupportValueChangeNotifications<TViewItem>>( );
			_fieldValueAccessorsMetadata = MapsFactory.CreateMapHelper<string, IFieldValueAccessor>( );
			_rawFieldValueAccessorsMetadata = MapsFactory.CreateMapHelper<string, IFieldValueAccessor>( );
			_dataItemPropNameToListenerMap = MapsFactory.CreateMapHelper<string, IValueChangeListener<object>>( );

			IEnumerable<TMappingKey> allMappingKeys = this.GetAllMappingKeys( );
			if ( this.HasList && null != allMappingKeys )
			{
				LinqQueryManager linqQueryManager = this.LinqQueryManager;

				object propDescriptorAccessorFactoryCache = null;
				object reflectionPropAcessorFactoryCache = null;

				// Create field value accessors.
				// 
				foreach ( TMappingKey field in allMappingKeys )
				{
					IFieldValueAccessor accessor = this.CreateFieldValueAccessorOverride( field );
					IFieldValueAccessor rawAccessor = null;
					string mappedField = null;
					if ( null == accessor )
					{
						mappedField = this.GetMappedFieldIfAny( field );
						if ( !string.IsNullOrEmpty( mappedField ) )
						{
							ConverterInfo converterInfo = this.GetConverter( field );
							bool isFieldExplicitlyMapped = this.IsExplicitlyMapped( field );

							accessor = this.VerifyFieldValueAccessors_CreateAccessorHelper( mappedField, isFieldExplicitlyMapped, converterInfo, linqQueryManager,
								ref propDescriptorAccessorFactoryCache, ref reflectionPropAcessorFactoryCache );

							rawAccessor = this.VerifyFieldValueAccessors_CreateAccessorHelper( mappedField, isFieldExplicitlyMapped, null, linqQueryManager,
								ref propDescriptorAccessorFactoryCache, ref reflectionPropAcessorFactoryCache );
						}
					}

					if ( null != accessor )
					{
						_fieldValueAccessors[field] = accessor;
						_fieldValueNotifiers[accessor] = VerifyFieldValueAccessors_AddDataItemPropListener( mappedField, accessor );
					}

					if ( null == rawAccessor )
						rawAccessor = accessor;

					if ( null != rawAccessor )
						_rawFieldValueAccessors[field] = rawAccessor;
				}

				// Create field value accessors for metadata field mappings if any.
				// 
				MetadataPropertyMappingCollection metadataMappings = null != _mappings ? _mappings.MetadataPropertyMappings : null;
				if ( null != metadataMappings )
				{
					foreach ( MetadataPropertyMapping mapping in metadataMappings )
					{
						string metadataKey = mapping.MetadataProperty;
						string mappedField = mapping.DataObjectProperty;

						if ( !string.IsNullOrEmpty( mappedField ) )
						{
							ConverterInfo converterInfo = mapping._converterInfo;

							bool isFieldExplicitlyMapped = true;

							IFieldValueAccessor accessor = this.VerifyFieldValueAccessors_CreateAccessorHelper( 
								mappedField, isFieldExplicitlyMapped, converterInfo, linqQueryManager,
								ref propDescriptorAccessorFactoryCache, ref reflectionPropAcessorFactoryCache );

							IFieldValueAccessor rawAccessor = this.VerifyFieldValueAccessors_CreateAccessorHelper( 
								mappedField, isFieldExplicitlyMapped, null, linqQueryManager,
								ref propDescriptorAccessorFactoryCache, ref reflectionPropAcessorFactoryCache );

							if ( null != accessor )
							{
								_fieldValueAccessorsMetadata[metadataKey] = accessor;
								_fieldValueNotifiers[accessor] = VerifyFieldValueAccessors_AddDataItemPropListener( mappedField, accessor );
							}

							if ( null == rawAccessor )
								rawAccessor = accessor;

							if ( null != rawAccessor )
								_rawFieldValueAccessorsMetadata[metadataKey] = rawAccessor;
						}
					}
				}
			}

			// Error validation logic listens for FieldValueAccessors to validate the field mappings and
			// raise blocking or diagnostic errors.
			// 
			this.RaisePropertyChangedEvent( "FieldValueAccessors" );
		}

		#endregion // VerifyFieldValueAccessors 

		#region VerifyItemsInCleanResults

		/// <summary>
		/// Adds or removes specified items from the results based on the 'add' parameter. If 'add' parameter
		/// is null it verifies whether the item should exist in the result and adds or removes it from the
		/// result accordingly.
		/// </summary>
		/// <param name="dataItemsToVerify">Items beign added/removed or to be verified.</param>
		/// <param name="add">If true, performs add. If false, performs removal. If null verifies whether
		/// the item should exist in the result based on the result's query criteria and adds or removes
		/// it from the result accordingly.</param>
		protected void VerifyItemsInCleanResults( IEnumerable dataItemsToVerify, bool? add )
		{
			IList<ListQueryResult> results = this.GetCleanResults( );
			if ( null != results && results.Count > 0 )
			{
				foreach ( ListQueryResult ii in results )
					this.VerifyItemsInResult( ii, dataItemsToVerify, add );
			}
		}

		#endregion // VerifyItemsInCleanResults

		#region VerifyItemsInResult

		// SSP 4/21/11 TFS73037
		// 
		private IEnumerable VerifyItemsInResult_GetCachedResult( ListQueryResult result, IEnumerable dataItemsToVerify )
		{
			// Find the items that match the query criteria associated with the result.
			// 
			LinqQueryManager.ILinqStatement linqStatement = result.LinqStatement;

			TypedEnumerable<object> querySourceEnumerable = result._verifyItemsInResult_Enumerable;
			if ( null == querySourceEnumerable )
				result._verifyItemsInResult_Enumerable = querySourceEnumerable = new TypedEnumerable<object>( dataItemsToVerify );
			else
				querySourceEnumerable.ResetSourceEnumerable( dataItemsToVerify );

			IEnumerable matchingItems = result._verifyItemsInResult_Result;
			if ( null == matchingItems )
			{
				matchingItems = null != linqStatement ? this.LinqQueryManager.PerformQuery( querySourceEnumerable, linqStatement ) : null;
				result._verifyItemsInResult_Result = matchingItems as IQueryable;
			}

			return matchingItems;
		}

		/// <summary>
		/// Adds or removes specified items from the result based on the 'add' parameter. If 'add' parameter
		/// is null it verifies whether the item should exist in the result and adds or removes it from the
		/// result accordingly.
		/// </summary>
		/// <param name="result">Query result.</param>
		/// <param name="dataItemsToVerify">Items beign added/removed or to be verified.</param>
		/// <param name="add">If true, performs add. If false, performs removal. If null verifies whether
		/// the item should exist in the result based on the result's query criteria and adds or removes
		/// it from the result accordingly.</param>
		protected void VerifyItemsInResult( ListQueryResult result, IEnumerable dataItemsToVerify, bool? add )
		{
			// If ShouldProcessChangeEvent is false then the result is dirty and it will be verified later on.
			// 
			if ( null != result && result.ShouldProcessChangeEvent )
			{
				bool isAdd = add.HasValue && add.Value;
				bool isRemove = add.HasValue && !add.Value;
				bool change = !add.HasValue;

				// Find the items that match the query criteria associated with the result.
				// 
				// SSP 4/21/11 TFS73037
				// Refactored and added caching logic to cache the resultant IQueriable to prevent re-compilation
				// of linq expression.
				// 
				
				//LinqQueryManager.ILinqStatement linqStatement = result.LinqStatement;
				//IEnumerable matchingItems = null != linqStatement ? this.LinqQueryManager.PerformQuery( dataItemsToVerify, linqStatement ) : null;

				IEnumerable matchingItems = this.VerifyItemsInResult_GetCachedResult( result, dataItemsToVerify );
				

				if ( null != matchingItems )
				{
					IViewItemManager<TViewItem> viewItemManager = this.ViewItemManager;
					IList rdl = result.DataList;

					IEnumerable itemsToAdd = null;
					IEnumerable itemsToRemove = null;
					
					if ( isAdd )
					{
						// When we get an add notification from the data source, simply add the item if it matches the query criteria
						// associated with the query result.
						// 
						itemsToAdd = matchingItems;
					}
					else if ( isRemove )
					{
						// When we get remove notification from the data source, simply remove the item from the query result.
						// 
						itemsToRemove = matchingItems;
					}
					else if ( change )
					{
						// When an item's value is changed, add it to the result if it matches query criteria otherwise remove
						// it from the list.
						// 
						HashSet<object> matchingItemsSet = null;
						foreach ( object ii in matchingItems )
						{
							if ( null == matchingItemsSet )
								matchingItemsSet = new HashSet<object>( );

							matchingItemsSet.Add( ii );
						}

						itemsToAdd = matchingItemsSet;

						// Items that don't match the query criteria have to be removed.
						// 
						List<object> removeList = new List<object>( );
						foreach ( object ii in dataItemsToVerify )
						{
							if ( null == matchingItemsSet || !matchingItemsSet.Contains( ii ) )
								removeList.Add( ii );
						}

						itemsToRemove = removeList;
					}

					if ( null != itemsToRemove )
					{
						foreach ( object ii in itemsToRemove )
						{
							if ( null != viewItemManager )
								viewItemManager.UpdateResultDataList_Remove( rdl, ii );
							else
								rdl.Remove( ii );
						}
					}

					if ( null != itemsToAdd )
					{
						foreach ( object ii in itemsToAdd )
						{
							if ( null != viewItemManager )
								viewItemManager.UpdateResultDataList_Add( rdl, ii, change, change );
							else if ( isAdd || ! rdl.Contains( ii ) )
								rdl.Add( ii );
						}
					}					
				}
			}
		}

		#endregion // VerifyItemsInResult

		#endregion // Protected Methods

		#region Internal Methods

		#region CreateAnyOfQuery

		internal LinqQueryManager.ILinqCondition CreateAnyOfQuery( TMappingKey field, IEnumerable values, out DataErrorInfo error )
		{
			return this.CreateConditionQuery( field, LinqQueryManager.LinqOperator.AnyOf, values, out error );
		}

		#endregion // CreateAnyOfQuery

		#region CreateConditionQuery

		internal LinqQueryManager.ILinqCondition CreateConditionQuery( TMappingKey lhsField, LinqQueryManager.LinqOperator linqOperator, object rhsValue, out DataErrorInfo error )
		{
			string mappedField;
			if ( !this.GetMappedField( lhsField, out mappedField, out error ) )
				return null;

			return new LinqQueryManager.LinqCondition( mappedField, linqOperator, rhsValue );
		}

		#endregion // CreateConditionQuery

		#region CreateEqualQuery

		internal LinqQueryManager.ILinqCondition CreateEqualQuery( TMappingKey field, object value, out DataErrorInfo error )
		{
			return this.CreateConditionQuery( field, LinqQueryManager.LinqOperator.Equal, value, out error );
		}

		#endregion // CreateEqualQuery

		#region DirtyAllQueryResults

		// MD 4/29/11 - TFS57206
		// Added a parameter to indicate whether we should clear the data list on all query results.
		//internal void DirtyAllQueryResults( bool discardViewItems, bool markCleanAsync )
		internal void DirtyAllQueryResults(bool discardViewItems, bool markCleanAsync, bool clearDataList = true)
		{
			if ( null != _results )
			{
				foreach ( ListQueryResult ii in _results )
				{
					// MD 4/29/11 - TFS57206
					// Added a parameter to indicate whether we should clear the data list on all query results.
					//this.DirtyQueryResult( ii, discardViewItems, markCleanAsync );
					this.DirtyQueryResult(ii, discardViewItems, markCleanAsync, clearDataList);
				}
			}
		}

		#endregion // DirtyAllQueryResults

		#region DirtyFieldValueAccessors

		/// <summary>
		/// Called to dirty field value accessors. This is typically called when the list is changed to different
		/// list or one or more mappings are changed.
		/// </summary>
		internal virtual void DirtyFieldValueAccessors( )
		{
			if ( _changeInformation != null )
				_changeInformation.BumpPropertyMappingsVersion( );

			_fieldValueAccessors = null;

			this.RecreateAllViewItems( );
		}

		#endregion // DirtyFieldValueAccessors

		#region DirtyQueryResult

		// MD 4/29/11 - TFS57206
		// Added a parameter to indicate whether we should clear the data list on the query result.
		//internal void DirtyQueryResult( ListQueryResult result, bool discardViewItems, bool markCleanAsync )
		internal void DirtyQueryResult(ListQueryResult result, bool discardViewItems, bool markCleanAsync, bool clearDataList = true)
		{
			Debug.Assert( null != result );
			if ( null != result )
			{
				// MD 4/29/11 - TFS57206
				// Added a parameter to indicate whether we should clear the data list on the query result.
				//result.MarkDirty( discardViewItems );
				result.MarkDirty(discardViewItems, clearDataList);

				_pendingDirtyResults.Enque( result, false );
			}

			if ( markCleanAsync )
				_pendingDirtyResults.StartAsyncVerification( );
		}

		#endregion // DirtyQueryResult

		#region GetCleanResults

		internal IList<ListQueryResult> GetCleanResults( )
		{
			
			// 
			return ( from ii in this.Results where !ii.IsDirty select ii ).ToArray( );
		} 

		#endregion // GetCleanResults

		#region GetMappedFieldIfAny

		internal string GetMappedFieldIfAny( TMappingKey field, bool onlyCheckExplicitlyDefinedMappings = false )
		{
			string mappedField = null != _mappings ? _mappings[field] : null;

			if ( string.IsNullOrEmpty( mappedField ) && ! onlyCheckExplicitlyDefinedMappings && this.UseDefaultMappingsResolved )
				mappedField = this.GetDefaultPropertyMapping( field );

			return mappedField;
		}

		#endregion // GetMappedFieldIfAny

		#region HasMappedField

		internal bool HasMappedField( TMappingKey field, bool checkForLinqQueriability, bool onlyCheckExplicitlyDefinedMappings = false )
		{
			string s = this.GetMappedFieldIfAny( field, onlyCheckExplicitlyDefinedMappings );
			if ( string.IsNullOrEmpty( s ) )
				return false;

			if ( checkForLinqQueriability )
			{
				LinqQueryManager qm = this.LinqQueryManager;
				if ( null == qm || ! qm.HasProperty( s ) )
					return false;
			}

			return true;
		}

		#endregion // HasMappedField

		#region HasMappedFields

		internal bool HasMappedFields( IEnumerable<TMappingKey> fields, bool checkForLinqQueriability, bool onlyCheckExplicitlyDefinedMappings = false, IList<TMappingKey> missingFields = null, IList<TMappingKey> mappedFields = null )
		{
			bool hasMissingField = false;

			foreach ( TMappingKey ii in fields )
			{
				if ( !this.HasMappedField( ii, checkForLinqQueriability, onlyCheckExplicitlyDefinedMappings ) )
				{
					hasMissingField = true;

					if ( null != missingFields )
						missingFields.Add( ii );
				}
				else
				{
					if ( null != mappedFields )
						mappedFields.Add( ii );
				}
			}

			return ! hasMissingField;
		}

		#endregion // HasMappedFields

		#region MarkCleanPendingDirtyResults

		internal virtual void MarkCleanPendingDirtyResults( )
		{
			_pendingDirtyResults.PerformVerification( );
		}

		protected virtual void MarkCleanPendingDirtyResults( IEnumerable<ListQueryResult> results )
		{
			Action<ListQueryResult> reevaluateCallback = this.ReevaluateQuery;

			foreach ( ListQueryResult ii in results )
				ii.MarkClean( reevaluateCallback );
		}

		#endregion // MarkCleanPendingDirtyResults

		#region PerformQueryHelper

		internal IEnumerable PerformQueryHelper( LinqQueryManager.ILinqStatement query, out DataErrorInfo error )
		{
			// When List is changed, we dirty the field value accessors since the underlying property descriptors
			// could potentially change and also dirty all the query results. When the first of the query result
			// is re-evaluated, verify the field value accessors.
			// 
			this.EnsureFieldValueAccessorsVerified( );

			error = null;
			LinqQueryManager qm = this.LinqQueryManager;
			if ( null != qm )
			{
				try
				{
					return qm.PerformQuery( query );
				}
				catch ( Exception exception )
				{
					error = new DataErrorInfo( exception );
					return null;
				}
			}
			else
			{
				
				Debug.Assert( false );
				return null;
			}
		}

		#endregion // PerformQueryHelper

		#region QueryListCapabilities

		internal ListCapabilities QueryListCapabilities( ListCapabilities capabilitiesToQuery )
		{
			IEnumerable list = _list;
			IEditableCollectionView editableCollectionView = list as IEditableCollectionView;


			IBindingList bindingList = list as IBindingList;

			ListCapabilities result = ListCapabilities.None;

			if ( 0 != ( ListCapabilities.Add & capabilitiesToQuery ) )
			{
				bool allowed = false;

				if ( null != editableCollectionView )
					allowed = editableCollectionView.CanAddNew;

				else if ( null != bindingList )
					allowed = bindingList.AllowNew;

				else if ( null != _iTableProxy )
					allowed = ! _iTableProxy.IsReadOnly;
				else if ( null != _genericListProxy )
					allowed = _genericListProxy.IsAddAllowed && _genericListProxy.CanCreateNew;

				if ( allowed )
					result |= ListCapabilities.Add;
			}

			if ( 0 != ( ListCapabilities.Remove & capabilitiesToQuery ) )
			{
				bool allowed = false;

				if ( null != editableCollectionView )
					allowed = editableCollectionView.CanRemove;

				else if ( null != bindingList )
					allowed = bindingList.AllowRemove;

				else if ( null != _iTableProxy )
					allowed = !_iTableProxy.IsReadOnly;
				else if ( null != _genericListProxy )
					allowed = _genericListProxy.IsRemoveAllowed;

				if ( allowed )
					result |= ListCapabilities.Remove;
			}

			if ( 0 != ( ListCapabilities.Edit & capabilitiesToQuery ) )
			{
				// Assume editing is allowed.
				// 
				bool allowed = true;


				if ( null != bindingList )
					allowed = bindingList.AllowEdit;


				if ( allowed )
					result |= ListCapabilities.Edit;
			}

			return result;
		} 

		#endregion // QueryListCapabilities

		#endregion // Internal Methods

		#region Private Methods

		#region GetEditableObject

		private IEditableObject GetEditableObject( TViewItem item )
		{
			return null != this.ViewItemFactory
				? this.ViewItemFactory.GetDataItem( item ) as IEditableObject
				: null;
		}

		#endregion // GetEditableObject

		#region IsExplicitlyMapped

		private bool IsExplicitlyMapped( TMappingKey field )
		{
			return null != this.GetMappedFieldIfAny( field, true );
		}

		#endregion // IsExplicitlyMapped

		#region OnDataListChangedHandler

		private static void OnDataListChangedHandler( object listManager, DataListEventListener listener, DataListChangeInfo info )
		{
			( (ListManager<TViewItem, TMappingKey>)listManager ).OnDataListChanged( listener, info );
		}

		#endregion // OnDataListChangedHandler

		#region OnDataItemPropertyChangedHandler

		private static void OnDataItemPropertyChangedHandler( object listManager, object dataItem, string propName )
		{
			( (ListManager<TViewItem, TMappingKey>)listManager ).OnDataItemPropertyChanged( dataItem, propName );
		}

		#endregion // OnDataItemPropertyChangedHandler

		#region ProvideQueryResultData

		private void ProvideQueryResultData( object resultObj, IEnumerable dataEnumerable, DataErrorInfo error )
		{
			ListQueryResult result = (ListQueryResult)resultObj;

			IList viewList = null;
			IList dataList = null;
			if ( null != dataEnumerable )
			{
				var viewItemManager = this.ViewItemManager;
				dataList = null != viewItemManager 
					? viewItemManager.GetResultDataList( dataEnumerable ) 
					: new ObservableCollection<object>( ScheduleUtilities.ToTyped<object>( dataEnumerable ) );

				// Only create view list if the query is for getting activities. If the query is for getting navigation
				// values (next/prev activity) then there's no view list.
				// 
				if ( result.ShouldCreateViewList )
					viewList = this.CreateViewList( result, dataList, result.ViewList );
			}

			result.Initialize( viewList, dataList, error, true );
		}

		#endregion // ProvideQueryResultData

		#region VerifyFieldValueAccessors_AddDataItemPropListener

		/// <summary>
		/// Adds an entry to _dataItemPropNameToListenerMap for the specified mapped field. DataItemPropNameToListenerMap is used
		/// to route property change notification from the data item to the correct field value accessor.
		/// </summary>
		/// <param name="mappedField">Mapped field.</param>
		/// <param name="accessor">Field value accessor.</param>
		/// <returns>Notifier associated with the accessor.</returns>
		private ISupportValueChangeNotifications<TViewItem> VerifyFieldValueAccessors_AddDataItemPropListener( string mappedField, IFieldValueAccessor accessor )
		{
			ISupportValueChangeNotifications<TViewItem> valueChangeNotifier = accessor as ISupportValueChangeNotifications<TViewItem>;
			if ( null == valueChangeNotifier )
			{
				DataItemPropListener dataItemPropListener = new DataItemPropListener( this.GetViewItemFromDataItem );
				valueChangeNotifier = dataItemPropListener;

				if ( !string.IsNullOrEmpty( mappedField ) )
				{
					_dataItemPropNameToListenerMap[mappedField] = ValueChangeListenerList<object>.Add(
						_dataItemPropNameToListenerMap[mappedField], dataItemPropListener );
				}
			}

			return valueChangeNotifier;
		}

		#endregion // VerifyFieldValueAccessors_AddDataItemPropListener

		#region VerifyFieldValueAccessors_CreateAccessorHelper

		/// <summary>
		/// Creates IFieldValueAccessor for the specified mapped field.
		/// </summary>
		/// <param name="mappedField">Field in the data source.</param>
		/// <param name="isExplicitlyMapped">Whether the field was explicitly mapped. If true then fallbacks to using binding if the corresponding property is not found.</param>
		/// <param name="converterInfo">Converter information.</param>
		/// <param name="linqQueryManager">Linq query manager.</param>
		/// <param name="propDescriptorAccessorFactoryCache">Used by the method to cache certain information in this variable.</param>
		/// <param name="reflectionPropAcessorFactoryCache">Used by the method to cache certain information in this variable.</param>
		/// <returns>Created field value accessor or null if mapped field is not found.</returns>
		private IFieldValueAccessor VerifyFieldValueAccessors_CreateAccessorHelper(
				string mappedField,
				bool isExplicitlyMapped,
				ConverterInfo converterInfo,
				LinqQueryManager linqQueryManager,
				ref object propDescriptorAccessorFactoryCache,
				ref object reflectionPropAcessorFactoryCache
			)
		{
			IFieldValueAccessor accessor = null;

			// SSP 4/21/11 TFS73037 - Performance
			// Moved this if block from below after the block for getting PropertyDescriptorValueAccessor. 
			// Give preference to ReflectionPropertyValueAccessor over PropertyDescriptorValueAccessor for 
			// better performance.
 			// 
			if ( null == accessor )
			{
				if ( null == reflectionPropAcessorFactoryCache && null != linqQueryManager )
					reflectionPropAcessorFactoryCache = new ReflectionPropertyValueAccessorFactory( linqQueryManager );

				var factory = (ReflectionPropertyValueAccessorFactory)reflectionPropAcessorFactoryCache;
				if ( null != factory )
					accessor = factory.CreateHelper( mappedField, converterInfo );
			}



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


			if ( null == accessor )
			{
				if ( isExplicitlyMapped )
				{
					accessor = new FieldValueAccessor_Binding( mappedField, converterInfo );

					// A direct property doesn't exist on the list element type which could mean the
					// generic list's element type is a base class of the actual objects that the list
					// contains or the field mapping is incorrect. Raise a diagnostic error message
					// to alter developer of a potentially incorrect property mapping.
					// 
					//this.OnError( DataErrorInfo.CreateDiagnostic( null, "'{0}' as specified in field mapping doesn't exist as a direct property of the item type of the data source list. Is the data source a generic list with the correct template type?", mappedField ) );
					this.OnError(ScheduleUtilities.CreateDiagnosticFromId(null, "LE_MappingDoesNotExist", mappedField, typeof(TViewItem).Name));
				}
			}

			return accessor;
		}

		#endregion // VerifyFieldValueAccessors_CreateAccessorHelper

		#endregion // Private Methods

		#endregion // Methods
	}

	#endregion // ListManager Class

	#region IListManager Interface

	internal interface IListManager
	{
		ListManagerChangeInformation ChangeInformation { get; }
		IEnumerable List { get; }
	} 

	#endregion  // IListManager Interface

	#region ListChangeHistoryItem Class

	internal class ListChangeHistoryItem : IComparable<ListChangeHistoryItem>
	{
		private object _changedItem;
		private ListChangeType _changeType;
		private int _dataListVersion;

		public ListChangeHistoryItem(int dataListVersion)
			: this(null, ListChangeType.Reset, dataListVersion) { }

		public ListChangeHistoryItem(object changedItem, ListChangeType changeType, int dataListVersion)
		{
			_changedItem = changedItem;
			_changeType = changeType;
			_dataListVersion = dataListVersion;
		}

		public object ChangedItem
		{
			get { return _changedItem; }
		}

		public ListChangeType ChangeType
		{
			get { return _changeType; }
		}

		public int DataListVersion
		{
			get { return _dataListVersion; }
		}

		#region IComparable<ListChangeHistoryItem> Members

		int IComparable<ListChangeHistoryItem>.CompareTo(ListChangeHistoryItem other)
		{
			return _dataListVersion.CompareTo(other._dataListVersion);
		}

		#endregion
	}

	#endregion  // ListChangeHistoryItem Class

	#region ListManagerChangeInformation Class

	internal class ListManagerChangeInformation
	{
		private const int MaxHistorySize = 100;

		#region Member Variables

		private ListChangeHistoryItem[] _changeHistory;
		private int _dataListVersion;
		private bool _hasList;
		private int _historyStartIndex;
		private Guid _id;
		private bool _isHistoryFull;
		private bool _isThreadSafe;
		private ListCapabilities _listCapabilities;
		private object _previousPropertyMappings;
		private int _propertyMappingsVersion;

		#endregion  // Member Variables

		#region Constructor

		public ListManagerChangeInformation(bool isThreadSafe)
		{
			_id = Guid.NewGuid();
			_isThreadSafe = isThreadSafe;

			_changeHistory = new ListChangeHistoryItem[MaxHistorySize];
		}

		#endregion  // Constructor

		#region Methods

		#region Public Methods

		#region BumpDataListVersion

		public void BumpDataListVersion()
		{
			this.BumpVersion(ref _dataListVersion);
		}

		#endregion  // BumpDataListVersion

		#region BumpPropertyMappingsVersion

		public void BumpPropertyMappingsVersion()
		{
			this.BumpVersion(ref _propertyMappingsVersion);
		}

		#endregion  // BumpPropertyMappingsVersion

		#region GetHistoryFromVersion

		public IEnumerable<ListChangeHistoryItem> GetHistoryFromVersion(int lastCheckedVersion)
		{
			try
			{
				if (_isThreadSafe)
					Monitor.Enter(this);

				// If there are no history items, there are no changes to report.
				if (_changeHistory[0] == null)
					yield break;

				if (lastCheckedVersion < _changeHistory[_historyStartIndex].DataListVersion - 1)
				{
					yield return new ListChangeHistoryItem(null, ListChangeType.Reset, _dataListVersion);
					yield break;
				}

				int validLength;
				if (_isHistoryFull)
				{
					validLength = _changeHistory.Length;
				}
				else
				{
					for (validLength = 0; validLength < _changeHistory.Length; validLength++)
					{
						if (_changeHistory[validLength] == null)
							break;
					}
				}

				int index = Array.BinarySearch<ListChangeHistoryItem>(_changeHistory, 0, validLength, new ListChangeHistoryItem(lastCheckedVersion));

				int startIndex;
				if (index >= 0)
					startIndex = index + 1;
				else
					startIndex = ~index;

				List<ListChangeHistoryItem> historyFromVersion = new List<ListChangeHistoryItem>();

				bool foundEnd = false;
				this.GenerateRecentHistoryForClient(historyFromVersion, startIndex, _changeHistory.Length - 1, ref foundEnd);

				if (foundEnd == false)
					this.GenerateRecentHistoryForClient(historyFromVersion, 0, startIndex - 1, ref foundEnd);

				for (int i = 0; i < historyFromVersion.Count; i++)
					yield return historyFromVersion[i];
			}
			finally
			{
				if (_isThreadSafe)
					Monitor.Exit(this);
			}
		}

		#endregion  // GetHistoryFromVersion

		#region OnDataListChanged

		public void OnDataListChanged(DataListChangeInfo info)
		{
			try
			{
				if (_isThreadSafe)
					Monitor.Enter(this);

				bool versionHasBeenBumped = false;
				switch (info._changeType)
				{
					case DataListChangeInfo.ChangeType.Add:
						this.AddHistoryItems(info, ListChangeType.AddItem, ref versionHasBeenBumped);
						break;

					case DataListChangeInfo.ChangeType.Remove:
						this.AddHistoryItems(info, ListChangeType.RemoveItem, ref versionHasBeenBumped);
						break;

					case DataListChangeInfo.ChangeType.Replace:
						this.AddHistoryItems(info, ListChangeType.AddItem, ref versionHasBeenBumped);
						this.AddHistoryItems(info, ListChangeType.RemoveItem, ref versionHasBeenBumped);
						break;

					case DataListChangeInfo.ChangeType.Reset:
						this.OnListChanged(null, ListChangeType.Reset);
						versionHasBeenBumped = true;
						break;
				}

				if (versionHasBeenBumped == false)
					this.BumpDataListVersion();

			}
			finally
			{
				if (_isThreadSafe)
					Monitor.Exit(this);
			}
		}

		#endregion  // OnDataListChanged

		#region OnListChanged

		public void OnListChanged(object changedItem, ListChangeType changeType)
		{
			try
			{
				if (_isThreadSafe)
					Monitor.Enter(this);

				this.OnListChangedHelper(changedItem, changeType);
			}
			finally
			{
				if (_isThreadSafe)
					Monitor.Exit(this);
			}
		}

		#endregion  // OnListChanged

		#region RecacheListCapabilities

		// MD 10/4/10 - TFS50092
		// Updated this method so we don't have to pass in the same capabilities from all places it is called.
		//public void RecacheListCapabilities(ListCapabilities listCapabilities)
		//{
		//    _listCapabilities = listCapabilities;
		//}
		public void RecacheListCapabilities<TViewItem, TMappingKey>(ListManager<TViewItem, TMappingKey> listManager)
			where TViewItem : class
		{
			_listCapabilities = listManager.QueryListCapabilities(
				ListCapabilities.Add |
				ListCapabilities.Edit |
				ListCapabilities.Remove);
		}

		#endregion  // RecacheListCapabilities

		#region VerifyHasList

		public void VerifyHasList(IListManager listManager)
		{
			this._hasList = (listManager.List != null);
		}

		#endregion  // VerifyHasList

		#endregion  // Public Methods

		#region Private Methods

		#region AddHistoryItems

		private void AddHistoryItems(DataListChangeInfo info, ListChangeType changeType, ref bool versionHasBeenBumped)
		{
			IList items = changeType == ListChangeType.AddItem
				? info.NewItems
				: info.OldItems;

			if (items == null)
			{
				this.OnListChangedHelper(null, ListChangeType.Reset);
				return;
			}

			foreach (object dataItem in items)
			{
				this.OnListChangedHelper(dataItem, changeType);
				versionHasBeenBumped = true;
			}
		}

		#endregion  // AddHistoryItems

		#region BumpVersion

		private void BumpVersion(ref int version)
		{
			if (_isThreadSafe)
				Interlocked.Increment(ref version);
			else
				version++;
		}

		#endregion  // BumpVersion

		#region GenerateRecentHistoryForClient

		private void GenerateRecentHistoryForClient(List<ListChangeHistoryItem> recentHistoryForClient, int startIndex, int endIndex, ref bool foundEnd)
		{
			for (int i = startIndex; i <= endIndex; i++)
			{
				ListChangeHistoryItem historyItem = _changeHistory[i];

				if (historyItem == null)
				{
					foundEnd = true;
					break;
				}

				ListManagerChangeInformation.GenerateRecentHistoryForClientHelper(recentHistoryForClient, historyItem);
				recentHistoryForClient.Add(historyItem);
			}
		}

		#endregion  // GenerateRecentHistoryForClient

		#region GenerateRecentHistoryForClientHelper

		private static void GenerateRecentHistoryForClientHelper(List<ListChangeHistoryItem> recentHistoryForClient, ListChangeHistoryItem newHistoryItem)
		{
			switch (newHistoryItem.ChangeType)
			{
				case ListChangeType.ChangeItem:
				case ListChangeType.RemoveItem:
					bool isRemoval = newHistoryItem.ChangeType == ListChangeType.RemoveItem;

					for (int i = recentHistoryForClient.Count - 1; i >= 0; i--)
					{
						ListChangeHistoryItem existingHistoryItem = recentHistoryForClient[i];

						if (existingHistoryItem.ChangedItem != newHistoryItem)
							continue;

						switch (existingHistoryItem.ChangeType)
						{
							case ListChangeType.AddItem:
								// If we have an add operation in the list, a removal will supersede it, so remove the old add.
								if (isRemoval)
									recentHistoryForClient.RemoveAt(i);
								break;

							case ListChangeType.ChangeItem:
								// If we have a change operation in the list, this change or removal will supersede it, so remove the old change.
								recentHistoryForClient.RemoveAt(i);
								break;
						}
					}

					break;

				case ListChangeType.Reset:
					recentHistoryForClient.Clear();
					break;
			}
		}

		#endregion  // GenerateRecentHistoryForClientHelper

		#region OnListChangedHelper

		private void OnListChangedHelper(object changedItem, ListChangeType changeType)
		{
			Debug.Assert(
				changedItem != null || changeType == ListChangeType.Reset,
				"The changed item must be specified.");

			this.BumpDataListVersion();

			ListChangeHistoryItem historyItem = new ListChangeHistoryItem(changedItem, changeType, _dataListVersion);

			if (_isHistoryFull == false)
			{
				Debug.Assert(_historyStartIndex == 0, "When the history is not full, the start index should be 0.");

				for (int i = 0; i < _changeHistory.Length; i++)
				{
					if (_changeHistory[i] == null)
					{
						_changeHistory[i] = historyItem;
						return;
					}
				}

				_isHistoryFull = true;
			}

			_historyStartIndex++;
			if (_changeHistory.Length <= _historyStartIndex)
				_historyStartIndex = 0;

			_changeHistory[_historyStartIndex] = historyItem;
		}

		#endregion  // OnListChangedHelper

		#endregion  // Private Methods

		#endregion  // Methods

		#region Properties

		#region DataListVersion

		public int DataListVersion
		{
			get { return _dataListVersion; }
		}

		#endregion  // DataListVersion

		#region Id

		public Guid Id
		{
			get { return _id; }
		}

		#endregion  // Id

		#region HasList

		public bool HasList
		{
			get { return _hasList; }
		}

		#endregion  // HasList

		#region ListCapabilities

		public ListCapabilities ListCapabilities
		{
			get { return _listCapabilities; }
		}

		#endregion  // ListCapabilities

		#region PreviousPropertyMappings
      
		public object PreviousPropertyMappings
		{
			get { return _previousPropertyMappings; }
			set { _previousPropertyMappings = value; }
		} 
    
		#endregion  // PreviousPropertyMappings

		#region PropertyMappingsVersion

		public int PropertyMappingsVersion
		{
			get { return _propertyMappingsVersion; }
		}

		#endregion  // PropertyMappingsVersion

		#endregion  // Properties
	}

	#endregion  // ListManagerChangeInformation Class
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