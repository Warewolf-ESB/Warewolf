using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Windows.Data;

namespace Infragistics
{
	/// <summary>
	/// Wraps an IEnumerable to get items while using IList or IQueryable to improve performance if available.
	/// </summary>
	public abstract class DataManagerBase : INotifyCollectionChanged
	{
		#region Members
		private bool _enablePaging;
		private IEnumerable _dataSource, _originalDataSource;
		private ObservableCollection<SortContext> _currentSort;
		private RecordFilterCollection _currentFilter;
        // Default PageSize to -1, so that we always account for it being set.
		private int _pageSize = -1, _currentPage = 1, _recordCount = -1, _totalRecordCount = -1, _pageCount;
		private IList _sortedFilterDataSource;
		private SummaryExecution _summaryExecution = SummaryExecution.PriorToFilteringAndPaging;
		private Type _cachedType, _cachedCollectionType;
		private IPagedCollectionView _pagedCollection;
		private ICollectionView _collectionView;
		private IList _list;
		private IEditableCollectionView _editableCollectionView;
		private IFilteredCollectionView _filteredCollectionView;
		bool _suspendInvalidateDataSource;
		SummaryDefinitionCollection _currentSummaries;
		FormattingRuleCollection<IRule> _conditionalFormatRules;
        bool _allowCollectionViewOverrides = true;
        List<MergedDataContext> _mergeDataContexts;
        bool _ignoreCurrentChanging;
        private WeakCollectionChangedHandler<DataManagerBase> _weakDataSourceCollectionChanged;
        private WeakEventHandler<DataManagerBase, ICollectionView, EventArgs> _weakCollectionViewCurrentChanged;


        private WeakEventHandler<DataManagerBase, IBindingList, ListChangedEventArgs> _weakBindingListChanged;
        private IBindingList _bindingList;
        private ITypedList _typedList;
        private IBindingListView _bindingListView;


		#endregion

		#region Properties

		#region Public

		#region Sort
		/// <summary>
		/// Gets an observable collection of sorts, from primary sort to final sort.
		/// Sorts after the first are only applied if all the previous sorts were equal.
		/// </summary>
		public Collection<SortContext> Sort
		{
			get
			{
				if (this._currentSort == null)
				{
					this._currentSort = new ObservableCollection<SortContext>();
				}
				return this._currentSort;
			}
		}
		#endregion // Sort

		#region ConditionalFormats
		/// <summary>
		/// Gets / sets the collection of conditional formatting rules which will be requesting data.
		/// </summary>
		public FormattingRuleCollection<IRule> ConditionalFormattingRules
		{
			get
			{
				return this._conditionalFormatRules;
			}
			set
			{
				this._conditionalFormatRules = value;

				this.ClearCachedDataSource(false);
			}
		}
		#endregion // ConditionalFormats

		#region Filter
		/// <summary>
		/// Gets / sets the <see cref="RecordFilterCollection"/> which will be applied to the records during the databinding.
		/// </summary>
		public RecordFilterCollection Filters
		{
			get
			{
				return this._currentFilter;
			}
			set
			{
				this.DetachFilterEvents();

				this._currentFilter = value;

                // We can't currently support Filtering at the same time as GroupBy, so, instead we'll ignore the reset if that happens.
                if(this.ICollectionViewData == null || !this.ICollectionViewData.CanGroup || this.GroupByObject == null)
				    this.ClearCachedDataSource(false);

				if (this._currentFilter != null)
				{
					this._currentFilter.CollectionChanged += new NotifyCollectionChangedEventHandler(CurrentFilter_CollectionChanged);
					this._currentFilter.CollectionItemChanged += new EventHandler<EventArgs>(CurrentFilter_CollectionItemChanged);
				}
			}
		}

		#endregion

		#region Summaries
		/// <summary>
		/// Gets / sets the <see cref="SummaryDefinitionCollection"/> which will be applied to the records during the databinding.
		/// </summary>
		public SummaryDefinitionCollection Summaries
		{
			get
			{
				return this._currentSummaries;
			}
			protected internal set
			{
                this.SummariesDirty = true;
				this._currentSummaries = value;

				this.ClearCachedDataSource(false);
			}
		}
		#endregion // Summaries

		#region SummaryResultCollection
		/// <summary>
		/// The collection of <see cref="SummaryResult"/> objects that will be populated by the <see cref="DataManagerBase"/>.
		/// </summary>
		public SummaryResultCollection SummaryResultCollection
		{
			get;
			protected internal set;
		}
		#endregion // SummaryResultCollection

		#region SummaryExecution
		/// <summary>
		/// Gets / sets the <see cref="SummaryExecution"/> which will determine where the summaries should be calculated by default.
		/// </summary>
		public SummaryExecution SummaryExecution
		{
			get
			{
				return this._summaryExecution;
			}
			set
			{
				if (this._summaryExecution != value)
				{
					this._summaryExecution = value;
					this.ClearCachedDataSource(true);
				}
			}
		}

		#endregion // SummaryExecution

		#region GroupBy

		#region GroupByObject

		/// <summary>
		/// Gets/Sets the object in which the data that this <see cref="DataManagerBase"/> represents, should be grouped by.
		/// </summary>
		public GroupByContext GroupByObject
		{
			get;
			set;
		}

		#endregion // GroupByObject

		#region GroupBySortContext

		/// <summary>
		/// Gets/Sets the CurrentSort that will be applied when the data is Grouped by a particular field.
		/// </summary>
		public SortContext GroupBySortContext
		{
			get;
			set;
		}

		#endregion // GroupBySortContext

		#region GroupBySortAscending

		/// <summary>
		/// Gets/Sets the sort direction that should be applied to the field that the underlying data has been grouped by.
		/// </summary>
		public bool GroupBySortAscending
		{
			get;
			set;
		}

		#endregion // GroupBySortAscending

		#endregion // GroupBy

		#region Paging

		#region CurrentPage
		/// <summary>
		/// Gets / sets the index of the page of data which should be retrieved from the manager.
		/// </summary>
		public int CurrentPage
		{
			get
			{
				return (this._currentPage > this.PageCount) ? 1 : this._currentPage;
			}
			set
			{
				if (value != this._currentPage)
				{
					_currentPage = value;
					this._sortedFilterDataSource = null;
					this.IsSortedFilteredDataSourceCalculated = false;
					this.InvalidateSortedFilterdDataSource();

				}
			}
		}
		#endregion // CurrentPage

		#region PageSize
		/// <summary>
		/// Gets / sets how many records constitute a page of data
		/// </summary>
		public int PageSize
		{
			get
			{
				return _pageSize;
			}
			set
			{
				if (_pageSize != value)
				{
					_pageSize = value;
					if (this.EnablePaging)
						this.ClearCachedDataSource(false);

                    // Need to keep the PageSize synced.
                    if (this.IPagedCollectionViewData != null && this.IPagedCollectionViewData.PageSize != this.PageSize)
                        this.IPagedCollectionViewData.PageSize = this.PageSize;
				}
			}
		}
		#endregion // PageSize

		#region EnablePaging
		/// <summary>
		/// Gets / sets whether paging should be used by the manager.
		/// </summary>
		public bool EnablePaging
		{
			get
			{
				return _enablePaging;
			}
			set
			{
				if (_enablePaging != value)
				{
					_enablePaging = value;
					this.ClearCachedDataSource(false);
				}
			}
		}
		#endregion // EnablePaging

		#endregion // Paging

		#region DataSource

		/// <summary>
		/// Gets or sets the IEnumerable that this <see cref="DataManagerBase"/> manages.
		/// </summary>
		public IEnumerable DataSource
		{
			get { return this.GetDataSource(); }
			set 
            {
                this.SetDataSource(value); 
            }
		}

		#endregion // DataSource

		#region OriginalDataSource
		/// <summary>
		/// Gets or sets the IEnumerable that this <see cref="DataManagerBase"/> manages without converting types.
		/// </summary>
		public IEnumerable OriginalDataSource
		{
			get { return this._originalDataSource; }
			set
			{
				this._originalDataSource = value;
				this._pagedCollection = value as IPagedCollectionView;
				this._collectionView = value as ICollectionView;
				this._list = value as IList;
				this._editableCollectionView = value as IEditableCollectionView;
				this._filteredCollectionView = value as IFilteredCollectionView;


                IEnumerable source = value;
                if (this._collectionView != null)
                {
                    if (this._collectionView.SourceCollection != null)
                        source = this._collectionView.SourceCollection;
                }

                this._bindingList = source as IBindingList;
                this._typedList = source as ITypedList;
                this._bindingListView = source as IBindingListView;

			    if (this._list == null && source is IListSource)
			    {
                    this._list = ((IListSource)source).GetList();
			    }

			}
		}
		#endregion // OriginalDataSource

		#region PageCount
		/// <summary>
		/// Gets the total number of pages available in the data source based on page size.  If 
		/// <see cref="EnablePaging"/> is false, this will report 1. 
		/// </summary>
		public int PageCount
		{
			get
			{
				if (this.IPagedCollectionViewData != null && this.IPagedCollectionViewData.TotalItemCount != -1)
				{
					this._pageCount = (int)Math.Ceiling((double)this.IPagedCollectionViewData.TotalItemCount / (double)this.IPagedCollectionViewData.PageSize);
				}

				return this._pageCount;
			}
			set
			{
				this._pageCount = value;
			}
		}
		#endregion // PageCount

		#region CollectionType
		/// <summary>
		/// The <see cref="Type"/> that the collection is designed to hold.
		/// </summary>
		protected Type CollectionType
		{
			get
			{
				Type itemType = null;
				if (this.DataSource != null)
				{
					itemType = DataManagerBase.ResolveCollectionType(this.OriginalDataSource);
				}
				return itemType;
			}
		}
		#endregion // CollectionType

		#region CachedCollectionType

		/// <summary>
		/// A cached version of CollectionType.
		/// </summary>
		public Type CachedCollectionType
		{
			get
			{
				if (this._cachedCollectionType == null)
					this._cachedCollectionType = this.CollectionType;

				return this._cachedCollectionType;
			}
			set
			{
				this._cachedCollectionType = value;
			}
		}

		#endregion // CachedCollectionType

		#region DataType

		/// <summary>
		/// Returns type of data that this IEnumerable represents. 
		/// </summary>
		protected Type DataType
		{
			get
			{
				Type itemType = null;
				if (this.DataSource != null)
				{
					itemType = DataManagerBase.ResolveItemType(this.DataSource);
                    
				}
				return itemType;
			}
		}

		#endregion //DataType

		#region CachedType

		/// <summary>
		/// Gets/Sets a cached version of the <see cref="DataManagerBase.DataType"/>.
		/// </summary>
		public Type CachedType
		{
			get
			{
                if (this._cachedType == null)
                {
                    this._cachedType = this.DataType;
                }

				return this._cachedType;
			}
			set
			{
				this._cachedType = value;
			}
		}

		#endregion // CachedType

        #region CachedTypedInfo
        CachedTypedInfo _cachedTypedInfo;

        /// <summary>
        /// Gets/Sets a cached version of the <see cref="DataManagerBase.DataType"/>.
        /// </summary>
        public CachedTypedInfo CachedTypedInfo
        {
            get
            {
                if (this._cachedTypedInfo != null)
                    return this._cachedTypedInfo; 

                CachedTypedInfo cti = new CachedTypedInfo() { CachedType = this.CachedType };

                if(this.ITypedListData != null)
                {
                    cti.PropertyDescriptors = this.ITypedListData.GetItemProperties(null);
                }

                return cti;
            }
            internal set
            {
                this._cachedTypedInfo = value;
            }
        }

        #endregion // CachedTypedInfo

        #region TotalRecordCount

        /// <summary>
		/// Gets the total number of records available from the datasource.
		/// </summary>
		/// <remarks>
		/// This excludes filtering, paging and grouping. 
		/// </remarks>
		public virtual int TotalRecordCount
		{
			get
			{
				if (this._totalRecordCount == -1)
				{
					int count = 0;


					if (this.IPagedCollectionViewData != null && this.IPagedCollectionViewData.TotalItemCount != -1)
					{
						count = this.IPagedCollectionViewData.TotalItemCount;
					}
					else
					{
						// If we're an IList, we can simply get the count
						if (this.IListData != null)
						{
							count = this.IListData.Count;
						}
						else
						{
							count = this.ResolveCount();
						}
					}

					this._totalRecordCount = count;
				}

				return this._totalRecordCount;
			}
			protected set
			{
				this._totalRecordCount = value;
			}

		}

		#endregion // TotalRecordCount

		#region RecordCount
		/// <summary>
		/// Gets the number of records that can be currently displayed.
		/// </summary>
		/// <remarks>
		/// This takes into account filtering, paging and grouping. 
		/// </remarks>
		public virtual int RecordCount
		{
			get
			{
				int count = 0;

				if (this._dataSource != null)
				{
					if (this.IPagedCollectionViewData != null && this.IPagedCollectionViewData.TotalItemCount != -1)
					{
						if (this._recordCount == -1)
							this._recordCount = this.ResolveCount();
					}
					else if (this._recordCount == -1)
					{
						this._recordCount = this.TotalRecordCount;
					}

					count = this._recordCount;
				}

				return count;
			}
			protected set
			{
				this._recordCount = value;
			}
		}

		#endregion //RecordCount

		#region ICollectionViewData

		/// <summary>
		/// Gets the underlying data source as an <see cref="ICollectionView"/>. If the datasource isn't an ICollectionView, null is returned.
		/// </summary>
		protected ICollectionView ICollectionViewData
		{
			get { return this._collectionView; }
		}

		#endregion // ICollectionViewData

		#region IFilteredCollectionViewData
		/// <summary>
		/// Gets the underlying data source as an <see cref="IFilteredCollectionView"/>. If the datasource isn't an IFilteredCollectionView, null is returned.
		/// </summary>
		protected IFilteredCollectionView IFilteredCollectionViewData
		{
			get { return this._filteredCollectionView; }
		}
		#endregion // IFilteredCollectionViewData

		#region IPagedCollectionViewData

		/// <summary>
		/// Gets the underlying data source as an <see cref="IPagedCollectionView"/>. If the datasource isn't an IPagedCollectionView, null is returned.
		/// </summary>
		internal IPagedCollectionView IPagedCollectionViewData
		{
			get { return this._pagedCollection; }
		}
		#endregion // IPagedCollectionViewData

		#region IListData

		/// <summary>
		/// Gets the underlying data source as an <see cref="IList"/>. If the datasource isn't an IList, null is returned.
		/// </summary>
		protected IList IListData
		{
			get { return this._list; }
		}
		#endregion // IListData

		#region IEditableCollectionViewData

		/// <summary>
		/// Gets the underlying data source as an <see cref="IEditableCollectionView"/>. If the datasource isn't an IEditableCollectionView, null is returned.
		/// </summary>
		protected IEditableCollectionView IEditableCollectionViewData
		{
			get { return this._editableCollectionView; }
		}
		#endregion // IEditableCollectionViewData

		#region SupportsDataManipulations

		/// <summary>
		/// Gets/ sets whether data manipulations such as Sorting are supported on this particular data manager.
		/// </summary>
		public bool SupportsDataManipulations
		{
			get;
			set;
		}

		#endregion // SupportsDataManipulations

		#region SuspendInvalidateDataSource

		/// <summary>
		/// Gets / sets if the DataManager should be prevented from invaliating it's cached data stores, so that 
		/// multiple actions can be built up and executed at one time.
		/// </summary>
		public bool SuspendInvalidateDataSource
		{
			get
			{
				return _suspendInvalidateDataSource;
			}
			set
			{
				if (_suspendInvalidateDataSource != value)
				{
					_suspendInvalidateDataSource = value;
					if (!value)
					{
						this.InvalidateSortedFilterdDataSource();
					}
				}
			}
		}
		#endregion // SuspendInvalidateDataSource

        #region AllowCollectionViewOverrides

        /// <summary>
        /// Gets/Sets whether this <see cref="DataManagerBase"/> is allowed to set properties for sorting and grouping if DataSource is an ICollectionView
        /// </summary>
        public bool AllowCollectionViewOverrides
        {
            get { return this._allowCollectionViewOverrides; }
            set { this._allowCollectionViewOverrides = value; }
        }

        #endregion // AllowCollectionViewOverrides

        #region MergeDataContexts

        /// <summary>
        /// Gets the list of <see cref="MergedDataContext"/> objects, that the manager should be displaying the data as. 
        /// </summary>
        public List<MergedDataContext> MergeDataContexts
        {
            get
            {
                if (this._mergeDataContexts == null)
                    this._mergeDataContexts = new List<MergedDataContext>();

                return this._mergeDataContexts;
            }
        }

        #endregion // MergeDataContexts

        #region SupportsEditing

        /// <summary>
        /// Gets whether editing is supported by the collection.
        /// </summary>
        public bool SupportsEditing
        {
            get
            {

                if (this.IBindingListData != null)
                    return this.IBindingListData.AllowEdit;


                return true;
            }
        }

        #endregion // SupportsEditing

        #endregion // Public

        #region Protected

        #region SortedFilteredDataSource
        /// <summary>
		/// Gets/sets a cached list of sorted items.
		/// </summary>		
		public IList SortedFilteredDataSource
		{
			get
			{
				this.InvalidateSortedFilterdDataSource();
				return this._sortedFilterDataSource;
			}
		}
		#endregion // SortedFilteredDataSource

		#region IsSortedFilteredDataSourceCalculated

		/// <summary>
		/// Gets/ sets whether the the SortedFilteredDataSource needs to be recalculated.
		/// </summary>
		protected bool IsSortedFilteredDataSourceCalculated
		{
			get;
			set;
		}
		#endregion // IsSortedFilteredDataSourceCalculated

        #region Defer

        /// <summary>
        /// Prevents operations from happening when the control is in an inconsistant state.
        /// </summary>
        protected internal bool Defer { get; set; }

        #endregion // Defer


        #region IBindingListData

        /// <summary>
        /// Gets the underlying data source as an <see cref="IBindingList"/>. If the datasource isn't an IBindingList, null is returned.
        /// </summary>
        protected IBindingList IBindingListData
        {
            get { return this._bindingList; }
        }

        #endregion // IBindingListData

        #region IBindingLisViewtData

        /// <summary>
        /// Gets the underlying data source as an <see cref="IBindingListView"/>. If the datasource isn't an IBindingListView, null is returned.
        /// </summary>
        protected IBindingListView IBindingLisViewtData
        {
            get { return this._bindingListView; }
        }

        #endregion // IBindingLisViewtData

        #region ITypedListData

        /// <summary>
        /// Gets the underlying data source as an <see cref="ITypedList"/>. If the datasource isn't an ITypedList, null is returned.
        /// </summary>
        protected ITypedList ITypedListData
        {
            get { return this._typedList; }
        }

        #endregion // ITypedListData

        #endregion // Protected

        #region Internal

        #region SortedFilteredList






        internal virtual IList SortedFilteredList
        {
            get
            {
                return this.SortedFilteredDataSource;
            }
        }

        #endregion // SortedFilteredList

        #region SummariesDirty

        internal bool SummariesDirty
        {
            get;
            set;
        }

        #endregion // SummariesDirty

        #region IsSorted

        internal bool IsSorted
        {
            get;
            set;
        }

        #endregion // IsSorted

        #region SupportsChangeNotification

        /// <summary>
        /// Gets a value indicating whether the <see cref="DataSource"/> raises change notification events.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the DataSource supports change notification; otherwise, <c>false</c>.
        /// </value>
        internal bool SupportsChangeNotification
	    { 
	        get
	        {
	            bool supportsChangeNotification = this.DataSource is INotifyCollectionChanged;


                


                supportsChangeNotification |= (this.IBindingListData != null);


	            return supportsChangeNotification;
	        }
	    }

        #endregion // SupportsChangeNotification

        #endregion // Internal

        #endregion // Properties

        #region Methods

        #region Static

        #region CreateDataManager
        /// <summary>
		/// Creates a generic data manager of the type of the first object in the source.
		/// </summary>
		/// <param name="dataSource">The source that the created manager should manage.</param>
		/// <returns>A new DataManagerBase.</returns>
		public static DataManagerBase CreateDataManager(IEnumerable dataSource)
		{
            return DataManagerBase.CreateDataManager(dataSource, null);
		}

        /// <returns></returns>
        /// <summary>
        /// Creates a generic data manager of the type of the first object in the source.
        /// </summary>
        /// <param name="dataSource">The source that the created manager should manage.</param>
        /// <param name="provider">The <see cref="DataManagerProvider"/> that will be used to Generate a DataManager.</param>
        /// <returns>A new DataManagerBase.</returns>
        public static DataManagerBase CreateDataManager(IEnumerable dataSource, DataManagerProvider provider)
        {
			try
			{
				DataManagerBase manager = null;

				Type itemType = DataManagerBase.ResolveItemType(dataSource);

                Type dataManagerType = typeof(DataManager<>);
                if (provider != null)
                    dataManagerType = provider.ResolveDataManagerType();

				// Needed to check if the itemType is of type object as well, as thats basically a fallback if the 
				// collection is actually empty, and since the object type has no properties, its usless for binding.
				if (itemType != null && itemType != typeof(object))
				{
                    bool create = true;

                    // Ok, so we're dealing with an entity set, and b/c of that, we're not going 
                    // to be able to createa a DM until data is added, but if its an IEditableCollectionView, 
                    // then we can actually cheat, and create a new data object, and use that instead.
                    if (itemType.FullName == "System.ServiceModel.DomainServices.Client.Entity")
                    {
                        IEditableCollectionView iecv = dataSource as IEditableCollectionView;
                        if (iecv != null && iecv.CanAddNew)
                        {
                            ICollectionView icv = dataSource as ICollectionView;
                            if (icv != null)
                            {
                                try
                                {
                                    object obj = iecv.AddNew();
                                    if (obj != null)
                                        itemType = obj.GetType();
                                    iecv.CancelNew();
                                }
                                catch (ArgumentNullException)
                                {
                                    // in some versions of RIA they don't do propery error checking so an ArgNull can be rasied and we can't prevent
                                    // it.  So trap for that case
                                }
                                
                                //// Need to call refresh, as the addNew operation can cause the loaded operation to be cancelled. 
                                icv.Refresh();
                            }
                        }
                        else
                            create = false;
                    }

                    if (create)
                    {
                        Type specificType = dataManagerType.MakeGenericType(new Type[] { itemType });
                        manager = Activator.CreateInstance(specificType, new object[] { }) as DataManagerBase;
                        manager.DataSource = dataSource;
                        manager.SupportsDataManipulations = true;

                        // Ok, so you gave us an empty collection, that also doesn't have any fields that we can auto generate. 
                        // So we're not going to create a data manager for you, until one of the following criteria are met. 
                        if (manager.GetDataProperties().Count() == 0 && manager.TotalRecordCount == 0)
                        {
                            // Make sure we unhook all events that may have been attached. 
                            manager.DataSource = null;
                            return null;
                        }
                    }
				}

				return manager;
			}
			catch (NotImplementedException)
			{
				return null;
			}
			catch (MethodAccessException)
			{
				TypelessDataManager manager = new TypelessDataManager();
				manager.DataSource = dataSource;
				manager.SupportsDataManipulations = true;
				return manager;
			}

		}
		#endregion //CreateDataManager

		#region ResolveCollectionType
		/// <summary>
		/// Resolves the underlying type of the item that the specified collection contains.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <returns></returns>
		public static Type ResolveCollectionType(IEnumerable dataSource)
		{
			Type itemType = null;

			IEnumerator enumerator = dataSource.GetEnumerator();
			if (enumerator == null)
				throw new InvalidEnumeratorException(SR.GetString("InvalidEnumeratorException"));

			Type iEnumerableType = enumerator.GetType();

			if (iEnumerableType.IsGenericType)
			{
				Type[] types = iEnumerableType.GetGenericArguments();
				if (types.Length > 0)
				{
					itemType = types[0];
				}
			}

			return itemType;
		}
		#endregion // ResolveCollectionType

		#region ResolveItemType
		/// <summary>
		/// Resolves the underlying type of the item that the specified collection contains.
		/// </summary>
		/// <param name="dataSource"></param>
		/// <returns></returns>
		public static Type ResolveItemType(IEnumerable dataSource)
		{
			Type itemType = null;
			Type enumerableItemType = null;
			Type returnType = null;

			IEnumerator enumerator = dataSource.GetEnumerator();
			if (enumerator == null)
				throw new InvalidEnumeratorException(SR.GetString("InvalidEnumeratorException"));

			if (enumerator.MoveNext())
			{
				object current = enumerator.Current;
				if (current != null)
					itemType = current.GetType();
			}

		    bool isDictionary = false;
		    Type dataSourceType = dataSource.GetType();

            if (typeof(IDictionary).IsAssignableFrom(dataSourceType))
            {
                isDictionary = true;
            }
            else if (dataSourceType.IsGenericType &&
                     dataSourceType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                isDictionary = true;
            }
            else if (dataSourceType.GetInterfaces().Any(
                        i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
            {
                isDictionary = true;
            }

            if (!isDictionary)
            {
                Type iEnumerableType = dataSource.GetType();
                if (iEnumerableType.IsGenericType)
                {
                    Type[] types = iEnumerableType.GetGenericArguments();
                    if (types.Length > 0)
                    {
                        foreach (Type t in types)
                        {
                            if (t == itemType)
                            {
                                enumerableItemType = t;
                            }
                        }
                        if (enumerableItemType == null)
                            enumerableItemType = types[0];
                    }
                }
                else
                {
                    iEnumerableType = enumerator.GetType();
                    if (iEnumerableType.IsGenericType)
                    {
                        Type[] types = iEnumerableType.GetGenericArguments();
                        if (types.Length > 0)
                        {
                            foreach (Type t in types)
                            {
                                if (t == itemType)
                                {
                                    enumerableItemType = t;
                                }
                            }
                            if (enumerableItemType == null)
                                enumerableItemType = types[0];
                        }
                    }
                }
            }

			if (itemType != null && enumerableItemType == null)
				returnType = itemType;
			else if (itemType == null && enumerableItemType != null)
				returnType = enumerableItemType;
			else // Both aren't null
			{
                IEnumerable<DataField> fields1 = ResolvePropertiesForItemType(enumerableItemType, null);
                IEnumerable<DataField> fields2 = ResolvePropertiesForItemType(itemType, null);

                if (fields1 != null && fields1.Count() == 0)
                    returnType = itemType;
                else if (fields2 != null && fields2.Count() == 0)
                    returnType = enumerableItemType;
                else
                {
                    // if the enumerableItemType isn't of type object, then lets use it
                    // as it's most likely just a base class. 
                    // Also, make sure it's not a Ria.Entity. I don't like doing this, however i don't see another choice
                    // b/c sometimes it's completely legitimate to generate a type for this, for example if the enumerableItemType
                    // is an interface or a base class, and the items in the collection aren't neccessairly going to be of the same type.
                    if (enumerableItemType != null && enumerableItemType != typeof(object) && (enumerableItemType.FullName != "System.Windows.Ria.Entity" && enumerableItemType.FullName != "System.ServiceModel.DomainServices.Client.Entity"))
                        returnType = enumerableItemType;
                    else
                        returnType = itemType;
                }
			}

			return returnType;

		}
		#endregion //ResolveItemType

		#region BuildPropertyExpressionFromPropertyName

        /// <summary>
        /// Builds a <see cref="Expression"/> for complex property names such as Address.Street1 or Items[FirstName]
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="paramExpression"></param>
		/// <returns></returns>
        public static System.Linq.Expressions.Expression BuildPropertyExpressionFromPropertyName(string propertyName, ParameterExpression paramExpression)
        {
            return BuildPropertyExpressionFromPropertyName(propertyName, paramExpression, null, null, null);
        }


		/// <summary>
        /// Builds a <see cref="Expression"/> for complex property names such as Address.Street1 or Items[FirstName]
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="paramExpression"></param>
        /// <param name="cachedTypedInfo"/>
        /// <param name="propertyType"/>
        /// <param name="defaultValue"/>
		/// <returns></returns>
        public static System.Linq.Expressions.Expression BuildPropertyExpressionFromPropertyName(string propertyName, ParameterExpression paramExpression, CachedTypedInfo cachedTypedInfo, Type propertyType, object defaultValue)
        {
            if (propertyName.Contains('.') || propertyName.Contains('['))
            {
                System.Linq.Expressions.Expression expression = null;
                string currentName = "";
                int length = propertyName.Length;

                // Walk the propertyName character by character to build the expression.
                for (int currentIndex = 0; currentIndex < length; currentIndex++)
                {
                    char c = propertyName[currentIndex];
                    switch (c)
                    {
                        case '.':
                            if (currentName.Length > 0)
                            {
                                if (expression == null)
                                {
                                    expression = System.Linq.Expressions.Expression.Property(paramExpression, currentName);
                                }
                                else
                                {
                                    expression = AddNullRefCheckToExpression(System.Linq.Expressions.Expression.Property(expression, currentName), expression);
                                }
                            }
                            currentName = "";
                            break;
                        case '[':
                            if (expression == null)
                            {
                                if (currentName.Length > 0)
                                    expression = System.Linq.Expressions.Expression.Property(paramExpression, currentName);
                                else
                                    expression = paramExpression;
                            }
                            else
                            {
                                expression = AddNullRefCheckToExpression(System.Linq.Expressions.Expression.Property(expression, currentName), expression);
                            }

                            int startIndex = currentIndex + 1;
                            currentIndex = propertyName.IndexOf(']', startIndex);

                            List<object> indexerParams = new List<object>();

                            PropertyInfo pi = DataManagerBase.ResolveIndexerPropertyInfo(propertyName, startIndex, currentIndex, expression.Type, indexerParams);
                     
                            if (pi != null)
                            {
                                // Used to support multiple parameter indexers  Items[FirstName, 0]
                                System.Linq.Expressions.Expression[] expressionParams = new System.Linq.Expressions.Expression[indexerParams.Count];
                                for (int i = 0; i < indexerParams.Count; i++)
                                    expressionParams[i] = System.Linq.Expressions.Expression.Constant(indexerParams[i]);


                                expression = System.Linq.Expressions.Expression.MakeIndex(expression, pi, expressionParams);

								// the indexer could be something like object. if it is known that the return value is a specific type (e.g. 
								// string, datetime, etc.) then we should be able to wrap the expression in a convert. this allows filtering 
								// in xamgrid columns when using an indexer that publicly returns object but where the column's datatype 
								// was explicitly set to a known type (e.g. string, datetime?, etc.) and allow filtering to work. in this 
								// case it is needed for gantt.
								if (propertyType != null && propertyType != pi.PropertyType && pi.PropertyType.IsAssignableFrom(propertyType))
									expression = System.Linq.Expressions.Expression.Convert(expression, propertyType);

                            }
                            else
                            {
                                MethodInfo mi = DataManagerBase.ResolveIndexerMethodInfo(expression.Type, indexerParams);

                                if (mi != null)
                                {
                                    // Used to support multiple parameter indexers  Items.Get(FirstName, 0)
                                    System.Linq.Expressions.Expression[] expressionParams = new System.Linq.Expressions.Expression[indexerParams.Count];
                                    for (int i = 0; i < indexerParams.Count; i++)
                                        expressionParams[i] = System.Linq.Expressions.Expression.Constant(indexerParams[i]);

                                    expression = System.Linq.Expressions.Expression.Call(expression, mi, expressionParams);
                                }
                                else
                                {
                                    throw new Exception(String.Format(SR.GetString("InvalidPropertyPathException"), propertyName));
                                }
                            }

                            currentName = "";
                            break;

                        default:
                            currentName += c;
                            break;

                    }
                }

                if (currentName.Length > 0)
                {
                    expression = AddNullRefCheckToExpression(System.Linq.Expressions.Expression.Property(expression, currentName), expression);
                }

                return expression;
            }
            else
            {

                if (cachedTypedInfo != null && cachedTypedInfo.PropertyDescriptors != null && cachedTypedInfo.PropertyDescriptors.Count > 0)
                {
                    System.ComponentModel.PropertyDescriptor pd = cachedTypedInfo.PropertyDescriptors[propertyName];
                    MethodInfo mi = pd.GetType().GetMethod("GetValue");
                    Expression method = Expression.Call(Expression.Constant(pd), mi, paramExpression);
                    Expression castedMethod = Expression.Convert(method, propertyType);

                    Expression equalExpression0 = Expression.Equal(method, Expression.Constant(null, typeof(object)));
                    Expression equalExpression1 = Expression.Equal(method, Expression.Constant(DBNull.Value, typeof(object)));
                    Expression orExpression = Expression.Or(equalExpression0, equalExpression1);

                    System.Linq.Expressions.Expression constExpression0 = System.Linq.Expressions.Expression.Constant(defaultValue, propertyType);
                    return Expression.Condition(orExpression, constExpression0, castedMethod);

                }


                return System.Linq.Expressions.Expression.Property(paramExpression, propertyName);
            }
        }

		#endregion // BuildPropertyExpressionFromPropertyName
        
        #region AddNullRefCheckToExpression

        private static Expression AddNullRefCheckToExpression(Expression newExpression, Expression expression)
        {
            if (expression != null && !expression.Type.IsValueType)
            {
                Expression equalExpression = Expression.Equal(expression, Expression.Constant(null, expression.Type));
                System.Linq.Expressions.Expression constExpression = ExpressionTypeGenerationBase.CreateGenericExpressionConstant(newExpression.Type);
                newExpression = Expression.Condition(equalExpression, constExpression, newExpression);
            }

            return newExpression;
        }

        private abstract class ExpressionTypeGenerationBase
        {
            protected abstract Expression GenerateConstantExpression();

            public static Expression CreateGenericExpressionConstant(Type type)
            {
                Type expressionTypeGenerator = typeof(ExpressionTypeGeneration<>).MakeGenericType(new System.Type[] { type});
                ExpressionTypeGenerationBase etgb = (ExpressionTypeGenerationBase)Activator.CreateInstance(expressionTypeGenerator, new object[] {});
                return etgb.GenerateConstantExpression();
            }
        }

        private class ExpressionTypeGeneration<T> : ExpressionTypeGenerationBase
        {
            protected override Expression GenerateConstantExpression()
            {
                return System.Linq.Expressions.Expression.Constant(default(T), typeof(T));
            }
        }

        #endregion // AddNullRefCheckToExpression

        #region ResolvePropertyTypeFromPropertyName
        /// <summary>
		/// Resolves the type of a property for complex properties such as Address.Stree1.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <param name="rootType"></param>
		/// <returns></returns>
        public static Type ResolvePropertyTypeFromPropertyName(string propertyName, CachedTypedInfo rootType)
		{
            if (propertyName.Contains('['))
            {
                System.Linq.Expressions.Expression expression = DataManagerBase.BuildPropertyExpressionFromPropertyName(propertyName, Expression.Parameter(rootType.CachedType, "param"));
                return expression.Type;
            }
            else
            {

                if (rootType.PropertyDescriptors != null)
                {
                    PropertyDescriptorCollection props = rootType.PropertyDescriptors;
                    if (props != null && props.Count > 0)
                    {
                        PropertyDescriptor pd = props[propertyName];
                        if (pd != null)
                            return pd.PropertyType;
                    }
                }


                string[] keys = propertyName.Split('.');
                Type columnType = rootType.CachedType.GetProperty(keys[0]).PropertyType;
                for (int i = 1; i < keys.Length; i++)
                {
                    PropertyInfo pi = columnType.GetProperty(keys[i]);
                    columnType = pi.PropertyType;
                }
                return columnType;
            }
		}
		
        #endregion // ResolvePropertyTypeFromPropertyName

        #region ResolveValueFromPropertyPath
        /// <summary>
        /// Walks the property tree of an object to resolve properties such as Address.Street1 or Items[FirstName] 
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static object ResolveValueFromPropertyPath(string propertyPath, object data)
        {
            return ResolveFromPropertyPath(propertyPath, data, true);
        }

        /// <summary>
        /// Walks through the property tree of an object to resolve the propretyInfo 
        /// </summary>
        /// <param name="propertyPath"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static PropertyInfo ResolvePropertyInfoFromPropertyPath(string propertyPath, object data)
        {
            return ResolveFromPropertyPath(propertyPath, data, false) as PropertyInfo;
        }

        private static object ResolveFromPropertyPath(string propertyPath, object data, bool returnValue)
		{
            if (propertyPath.Contains('.') || propertyPath.Contains('['))
            {
                Type columnType = data.GetType();
                string currentName = "";
                object obj = data;
                int length = propertyPath.Length;
                PropertyInfo pi = null;

                // Walk the propertyPath character by character to resolve the property.
                for (int currentIndex = 0; currentIndex < length; currentIndex++)
                {
                    char c = propertyPath[currentIndex];
                    switch (c)
                    {
                        case '.':
                            if (currentName.Length > 0)
                            {
                                pi = columnType.GetProperty(currentName);
                                if (pi == null)
                                    return null;
                                
                                columnType = pi.PropertyType;
                                obj = pi.GetValue(obj, null);
                                
                                if (obj == null)
                                    return null;

                                currentName = "";
                            }
                            break;
                        case '[':

                            if (currentName.Length > 0)
                            {
                                pi = columnType.GetProperty(currentName);
                                if (pi == null)
                                    return null;
                                columnType = pi.PropertyType;
                                obj = pi.GetValue(obj, null);

                                if (obj == null)
                                    return null;
                            }
                                
                            currentName = "";

                            int startIndex = currentIndex + 1;
                            currentIndex = propertyPath.IndexOf(']', startIndex);

                            List<object> indexerParams = new List<object>();

                            pi = DataManagerBase.ResolveIndexerPropertyInfo(propertyPath, startIndex, currentIndex, columnType, indexerParams);

                            if (pi == null)
                            {
                                MethodInfo mi = DataManagerBase.ResolveIndexerMethodInfo(columnType, indexerParams);

                                if(mi == null)
                                {
                                    return null;
                                }
                                else
                                {
                                    columnType = mi.ReturnType;
                                    obj = mi.Invoke(obj, indexerParams.ToArray());
                                }
                            }
                            else
                            {
                                
                                columnType = pi.PropertyType;
                                obj = pi.GetValue(obj, indexerParams.ToArray());
                            }
                                
                            if (obj == null)
                                return null;

                            currentName = "";
                            break;

                        default:
                            currentName += c;
                            break;

                    }
                }

                if (currentName.Length > 0)
                {
                    pi = columnType.GetProperty(currentName);
                    if (pi == null)
                        return null;

                    columnType = pi.PropertyType;
                    obj = pi.GetValue(obj, null);
                }

                if (returnValue)
                    return obj;
                else
                    return pi;
            }
            else
            {
                string[] keys = propertyPath.Split('.');
                object obj = data;
                PropertyInfo pi = null;
                Type columnType = data.GetType();
                
                foreach (string key in keys)
                {
                    data = obj;

                    PropertyInfo[] properties = columnType.GetProperties();

                    // NZ 24 April 2012 - TFS109026 - Search through the non-indexer properties
                    foreach (var propertyInfo in properties)
                    {
                        if (string.Compare(propertyInfo.Name, key, StringComparison.Ordinal) == 0 &&
                            propertyInfo.GetIndexParameters().Length == 0)
                        {
                            pi = propertyInfo;
                            break;
                        }
                    }

                    if (pi == null)
                        break;
                    obj = pi.GetValue(obj, null);
                    if (obj == null)
                        break;
                    columnType = pi.PropertyType;
                }


                if (obj == data)
                {
                    ICustomTypeDescriptor ictd = obj as ICustomTypeDescriptor;
                    if (ictd != null)
                    {
                        PropertyDescriptorCollection pdc = ictd.GetProperties();
                        foreach (PropertyDescriptor pd in pdc)
                        {
                            if (propertyPath == pd.Name)
                            {
                                obj = pd.GetValue(data);
                                break;
                            }
                        }
                    }
                }

                if (returnValue)
                    return obj;
                else
                    return pi;
            }
		}

        #endregion // ResolveValueFromPropertyPath

		#region CopyValues
		private static bool CopyValues(object newObj, object obj)
		{
			Type t = obj.GetType();
			if (newObj.GetType() == t)
			{

                ICustomTypeDescriptor ictd = obj as ICustomTypeDescriptor;
                if (ictd != null)
                {
                    ICustomTypeDescriptor newIctd = (ICustomTypeDescriptor)newObj;
                    PropertyDescriptorCollection pdcs = ictd.GetProperties();
                    foreach (PropertyDescriptor pd in pdcs)
                    {
                        pd.SetValue(newIctd, pd.GetValue(ictd));
                    }

                    return true;
                }


				PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
				foreach (PropertyInfo prop in props)
				{
					if (prop.GetSetMethod() != null)
					{
						object oldPropValue = prop.GetValue(newObj, null);
						object newPropValue = prop.GetValue(obj, null);

						if (newPropValue != oldPropValue && (oldPropValue == null || newPropValue == null || !newPropValue.Equals(oldPropValue)))
							prop.SetValue(newObj, prop.GetValue(obj, null), null);
					}
				}

				return true;
			}
			return false;
		}
		#endregion // CopyValues

        #region ResolveIndexerMethodInfo

        private static MethodInfo ResolveIndexerMethodInfo(Type type, List<object> indexerParams)
        {
            MethodInfo mi = null;
            // Now we're at a best guess trial, so based on the different indexers we found above, see if we can match the types.
            MethodInfo[] mis = type.GetMethods();
            
            if (mis.Length == 1)
            {
                mi = type.GetMethod("Get");
            }
            else
            {
                foreach (MethodInfo current in mis)
                {
                    if (current.Name == "Get")
                    {
                        bool matchFound = false;

                        ParameterInfo[] currentParams = current.GetParameters();
                        if (currentParams != null && currentParams.Length == indexerParams.Count)
                        {
                            matchFound = true;

                            for (int i = 0; i < indexerParams.Count; i++)
                            {
                                if (currentParams[i].ParameterType != indexerParams[i].GetType())
                                {
                                    matchFound = false;
                                    break;
                                }
                            }
                        }

                        if (matchFound)
                        {
                            mi = current;
                            break;
                        }
                    }
                }
            }

            return mi;
        }

        #endregion // ResolveIndexerMethodInfo

        #region ResolveIndexerPropertyInfo

        private static PropertyInfo ResolveIndexerPropertyInfo(string propertyName, int startIndex, int endIndex, Type type, List<object> indexerParams)
        {
            if (endIndex != -1)
            {
                string indexerString = propertyName.Substring(startIndex, endIndex - startIndex);

                string[] splitIndexers = indexerString.ToString().Split(',');

                // Used to support multiple parameter indexers  Items[FirstName, 0]
                foreach (string splitIndex in splitIndexers)
                {
                    object indexer = splitIndex;
                    int parsed = -1;
                    if (int.TryParse(splitIndex, out parsed))
                        indexer = parsed;
                    indexerParams.Add(indexer);
                }
            }

            PropertyInfo pi = null;

            string indexerName = "Item";

            // The default is "Item" however, it can apparently be customized using the IndexerNameAttribute, which gets changed to
            // DefaultmemberAttribute at compile time. 
            object[] attributes = type.GetCustomAttributes(typeof(DefaultMemberAttribute), true);
            if (attributes.Length > 0)
            {
                DefaultMemberAttribute attribute = attributes[0] as DefaultMemberAttribute;
                if (attribute != null)
                    indexerName = attribute.MemberName;
            }

            // Now we're at a best guess trial, so based on the different indexers we found above, see if we can match the types.
            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo current in pis)
            {
                if (current.Name == indexerName)
                {
                    bool matchFound = false;

                    ParameterInfo[] currentParams = current.GetIndexParameters();
                    if (currentParams != null && currentParams.Length == indexerParams.Count)
                    {
                        matchFound = true;

                        for (int i = 0; i < indexerParams.Count; i++)
                        {
                            if (currentParams[i].ParameterType != indexerParams[i].GetType())
                            {
                                matchFound = false;
                                break;
                            }
                        }
                    }

                    if (matchFound)
                    {
                        pi = current;
                        break;
                    }
                }

            }

            return pi;
        }

        #endregion // ResolveIndexerPropertyInfo

        #region ResolvePropertiesForItemType

        private static IEnumerable<DataField> ResolvePropertiesForItemType(Type itemType, DataManagerBase manager)
        {
            if (itemType != null)
            {
                List<DataField> fields = new List<DataField>();
                PropertyInfo[] propinfo = itemType.GetProperties();

                int index = propinfo.Length;
                foreach (PropertyInfo info in propinfo)
                {
                    if (manager != null)
                    {
                        DataField field = manager.GenerateDataField(info);

                        if (field != null)
                        {
                            fields.Add(field);

                            // We default the Order to int.Max
                            // But when we sort on this and they're all Max
                            // They will actually be in reverse order than they would have been in otherwise
                            if (field.Order == int.MaxValue)
                            {
                                field.Order -= index;
                            }
                        }
                    }
                    else
                    {
                        DataField field = new DataField(info.Name, info.PropertyType);
                        fields.Add(field);
                    }

                    index--;
                }

                fields.Sort(new DataFieldComparer());

                return fields;
            }
            return null;
        }

        #endregion // ResolvePropertiesForItemType

        #endregion // Static

        #region Internal

        #region FilterItems

        /// <summary>
        /// Filters a list of items using the filter operands applied on the <see cref="DataManagerBase"/>.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>Filtered list or null if filtering cannot be applied.</returns>
        /// <remarks>
        /// This method is added solely for the purpose of fast adding of an item in sorted/filtered list.
        /// </remarks>
        internal virtual IList FilterItems(IList items)
        {
            return null;
        }

        #endregion // FilterItems

        #region UpdateCachedDataManipulations

        /// <summary>
        /// Updates the cached data manipulations.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isAdding">if set to <c>true</c> the item will be added to the cached lists, otherwise removed.</param>
        /// <returns><c>true</c> if the cache was updated, otherwise <c>false</c>.</returns>
        internal virtual bool UpdateCachedDataManipulations(object item, bool isAdding)
        {
            return false;
        }

        #endregion // UpdateCachedDataManipulations

        #region ResolveIndexForInsertOrDelete

        /// <summary>
	    /// Resolves the index for insert or delete or return null if the Filtered DataSource is not supproeted.
	    /// </summary>
	    /// <param name="data">The data.</param>
	    /// <param name="isAdding"></param>
	    /// <returns></returns>
	    internal virtual int? ResolveIndexForInsertOrDelete(object data, bool isAdding)
	    {
	        // For DataManagerBase just use ResolveIndexForRecord.
	        // In DataManager<T> this method is overridden and tries to use faster algorithms
	        // to find the index (for example BinarySearch is used when the data is sorted)
	        return this.ResolveIndexForRecord(data);
	    }

        #endregion // ResolveIndexForInsertOrDelete

        #endregion // Internal

        #region Public

        #region GenerateNewObject

        /// <summary>
		/// Creates a new object with of <see cref="DataType"/> type.
		/// </summary>
		/// <returns></returns>
		public object GenerateNewObject()
		{
			object obj = null;

			if (this.IEditableCollectionViewData != null && this.IEditableCollectionViewData.CanAddNew)
			{
                object currentItem = null;
                if (this.ICollectionViewData != null)
                {
                    currentItem = this.ICollectionViewData.CurrentItem;
                }
                                
                this._ignoreCurrentChanging = true;
				obj = this.IEditableCollectionViewData.AddNew();
				this.IEditableCollectionViewData.CancelNew();
              

                this._ignoreCurrentChanging = false;

                if (this.ICollectionViewData != null)
                {
                    this.ICollectionViewData.MoveCurrentTo(currentItem);

                    if (this.ICollectionViewData.IsEmpty)
                         this.ICollectionViewData.Refresh();
                }

				return obj;
			}
			else
			{
				obj = this.OnNewObjectGeneration();
				if (obj != null)
				{
					if (this.CachedType.IsAssignableFrom(obj.GetType()))
					{
						return obj;
					}
					throw new DataObjectTypeMismatchException();
				}
			}

			try
			{
                if (this.CachedType != null)
                {
                    obj = this.CachedType.Assembly.CreateInstance(this.CachedType.FullName);
                }
			}
			catch (MissingMethodException me)
			{
				if (this.CachedType != null)
				{
					throw new RequireEmptyConstructorException(string.Format(SR.GetString("RequireEmptyConstructorExceptionWithType"), this.CachedType.FullName), me);
				}
				throw new RequireEmptyConstructorException(SR.GetString("RequireEmptyConstructorException"), me);
			}

			return obj;
		}
		#endregion // GenerateNewObject

		#region GetRecord
		/// <summary>
		/// Returns an object in the data source at a given index, after applying the sort and filter.
		/// </summary>
		/// <param name="recordIndex">The index of the item to find.</param>
		/// <returns>The object at that index.</returns>
		public object GetRecord(int recordIndex)
		{
            IEnumerable ds = this.GetDataSource();
            if (ds != null)
            {
                if (this.SortedFilteredDataSource == null)
                {
                    if (this.IListData != null)
                        return this.IListData[recordIndex];

                    return this.ResolveRecord(recordIndex);
                }
                else
                    return this.SortedFilteredDataSource[recordIndex];
            }

            return null;
		}

		#endregion // GetRecord

        #region ResolveIndexForRecord

        /// <summary>
        /// Looks through the filtered DataSource for the index of the item specified. 
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public int ResolveIndexForRecord(object data)
        {
            if (this._sortedFilterDataSource == null)
            {
                if (this.IListData != null)
                {
                    return this.IListData.IndexOf(data);
                }
                else
                {
                    int index = -1;

                    foreach (object current in this.OriginalDataSource)
                    {
                        index++;
                        
                        if (current == data)
                        {
                            return index;
                        }
                    }
                }
            }
            else
            {
                return this._sortedFilterDataSource.IndexOf(data);
            }

            return -1;
        }

        #endregion // ResolveIndexForRecord

        #region InsertRecord
        /// <summary>
		/// Adds inputted object to the datasource at the given index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		public void InsertRecord(int index, object value)
		{
			if (this.IListData != null)
			{
				this.IListData.Insert(index, value);
				this.ClearCachedDataSource(true);
				this.OnDataUpdated();
			}
			else
				throw new DataSourceDoesNotSupportIListException();
		}
		#endregion // InsertRecord

		#region AddRecord
		/// <summary>
		/// Adds inputted object to the datasource
		/// </summary>
		/// <param name="value"></param>
		public void AddRecord(object value)
		{
			if (this.IEditableCollectionViewData != null && this.IEditableCollectionViewData.CanAddNew)
			{
                this._ignoreCurrentChanging = true;
                object newObj = this.IEditableCollectionViewData.AddNew();

                if (DataManagerBase.CopyValues(newObj, value))
                    this.IEditableCollectionViewData.CommitNew();
                else
                    this.IEditableCollectionViewData.CancelNew();

                this._ignoreCurrentChanging = false;
			}
            else if (this.IListData != null)
            {

                if (this.IBindingListData != null)
                {
                    if (this.IBindingListData.AllowNew)
                    {
                        object newItem = value;
                        IEditableObject editable = value as IEditableObject;
                        // If the object is Editable, then calling endEdit will actually add it. 
                        // Which means we don't need to invoke add. 
                        if (editable != null)
                        {
                            editable.EndEdit();
                        }
                        else
                        {
                            newItem = this.IBindingListData.AddNew();
                            CopyValues(newItem, value);
                        }

                        if (!this.IBindingListData.SupportsChangeNotification)
                        {
                            this.ClearCachedDataSource(true);
                            NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem);
                            this.OnCollectionChanged(e);
                        }
                    }
                }
                else
                {
                    if (!this.IListData.IsFixedSize)
                    {
                        this.IListData.Add(value);
                        this.ClearCachedDataSource(true);
                    }
                    else
                        throw new DataSourceDoesNotSupportIListException();
                }




            }
            else
                throw new DataSourceDoesNotSupportIListException();

			this.OnDataUpdated();
		}
		#endregion // AddRecord

		#region RemoveRecord
		/// <summary>
		/// Removes a record from the datasource.
		/// </summary>
		/// <param name="value"></param>
        /// <returns>True if the record was removed, false otherwise.</returns>
		public bool RemoveRecord(object value)
		{
			if (this.IEditableCollectionViewData != null && this.IEditableCollectionViewData.CanRemove)
			{
				this.IEditableCollectionViewData.Remove(value);
			}
			else if (this.IListData != null)
			{

                if (this.IBindingListData == null || this.IBindingListData.AllowRemove)

                {
                    this.IListData.Remove(value);
                    this.ClearCachedDataSource(true);
                }

                else
                {
                    return false;
                }

			}
			else
				throw new DataSourceDoesNotSupportIListException();

			this.OnDataUpdated();

            return true;
		}
		#endregion  // RemoveRecord

		#region GetDataProperties

		/// <summary>
		/// Returns an IEnumerable of <see cref="DataField"/>'s that describe the different fields in this object.
		/// </summary>
		/// <returns>An IEnumerable of the DataField's for all the properties of the object.</returns>
		public IEnumerable<DataField> GetDataProperties()
		{

            PropertyDescriptorCollection pdcs = null;            
            if (this.ITypedListData != null)
            {
                pdcs = this.ITypedListData.GetItemProperties(null);
            }

            if (pdcs != null)
            {
                List<DataField> fields = new List<DataField>();

                foreach (PropertyDescriptor pd in pdcs)
                {
                    DataField field = new DataField(pd.Name, pd.PropertyType);
                    field.DisplayName = pd.DisplayName;
                    fields.Add(field);
                }

                return fields;
            }


			Type itemType = this.CachedType;
            return ResolvePropertiesForItemType(itemType, this);
		}

		#endregion //GetDataProperties

		#region Reset

		/// <summary>
		/// Clears out any stored information on the previous DataSource.
		/// </summary>
		public virtual void Reset()
		{
			this.Sort.Clear();
			this.ClearCachedDataSource(true);
			this._cachedType = null;
			this._cachedCollectionType = null;
            this._cachedTypedInfo = null;

            if(this.SummaryResultCollection != null)
                this.SummaryResultCollection.Clear();

			this.DetachFilterEvents();
			this._currentFilter = null;
		}

		#endregion // Reset

		#region UpdateData

		/// <summary>
		/// Clears the underlying cached data, and triggeres all data operations to be applied again.
		/// </summary>
		public void UpdateData()
		{
			this.ClearCachedDataSource(false);
		}

		#endregion // UpdateData      

        #region CancelEdit
        /// <summary>
        /// Wraps the IEditableCollectionVie.CancelEdit method
        /// </summary>
        /// <returns>Returns true if it's an IEditableCollectionView</returns>
        public bool CancelEdit()
        {
            if (this.IEditableCollectionViewData != null && this.IEditableCollectionViewData.CanCancelEdit)
            {
                this.IEditableCollectionViewData.CancelEdit();
                return true;
            }

            return false;
        }
        #endregion // CancelEdit

        #region CommitEdit

        /// <summary>
        /// Wraps the IEditableCollectionVie.CommitEdit method
        /// </summary>
        /// <returns>Returns true if it's an IEditableCollectionView</returns>
        public bool CommitEdit()
        {
            if (this.IEditableCollectionViewData != null)
            {
                this.IEditableCollectionViewData.CommitEdit();
                return true;
            }

            return false;   
        }
        #endregion // CommitEdit

        #region EditItem

        /// <summary>
        /// Wraps the IEditableCollectionVie.EditItem method
        /// </summary>
        /// <param name="item"></param>
        /// <returns>Returns true if it's an IEditableCollectionView</returns>
        public bool EditItem(object item)
        {
            if (this.IEditableCollectionViewData != null)
            {
                this.IEditableCollectionViewData.EditItem(item);
                return true;
            }

            return false;
        }
        #endregion // EditItem

        #region RefreshSummaries

        /// <summary>
        /// Reevaluates the summaries for the ItemsSource bound to this <see cref="DataManagerBase"/>.
        /// </summary>
        public virtual void RefreshSummaries()
        {

        }

        #endregion // RefreshSummaries

        #region UpdateCurrentItem

        /// <summary>
        /// Moves the <see cref="ICollectionView"/> current item pointer to the inputted item.
        /// </summary>
        /// <param name="item"></param>
        public void UpdateCurrentItem(object item)
        {
            if (this.ICollectionViewData != null)
            {
                this._ignoreCurrentChanging = true;
                this.ICollectionViewData.MoveCurrentTo(item);
                this._ignoreCurrentChanging = false;
            }
        }

        #endregion // UpdateCurrentItem

        #endregion //Public

        #region Protected

        #region GenerateDataField

        /// <summary>
        /// Creates a <see cref="DataField"/> object, which contains information about a specific property.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        protected virtual DataField GenerateDataField(PropertyInfo info)
        {
            DataField field = new DataField(info.Name, info.PropertyType);



#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


            return field;
        }
        #endregion // GenerateDataField

        #region SetDataSource
        /// <summary>
		/// Sets the <see cref="DataManagerBase.DataSource"/> while registering for change notification.
		/// </summary>
		/// <param name="source">The IEnumerable to set the DataSource to.</param>
		protected virtual void SetDataSource(IEnumerable source)
		{
			if (source == this._dataSource)
			{
				return;
			}

            if (this._weakDataSourceCollectionChanged != null)
            {
                this._weakDataSourceCollectionChanged.Detach();
                this._weakDataSourceCollectionChanged = null;
            }


            if (this._weakBindingListChanged != null)
            {
                this._weakBindingListChanged.Detach();
                this._weakBindingListChanged = null;
            }


            if (this._weakCollectionViewCurrentChanged != null)
            {
                this._weakCollectionViewCurrentChanged.Detach();
                this._weakCollectionViewCurrentChanged = null;
            }

            IEnumerable oldDataSource = this._dataSource;
            this._dataSource = source;

            if (oldDataSource != null)
				this.ClearCachedDataSource(true);			

			if (source == null)
			{
				this.Reset();
				return;
			}

			this.CachedType = this.DataType;
			this.CachedCollectionType = this.CollectionType;

            INotifyCollectionChanged notifyCollectionChanged = this.OriginalDataSource as INotifyCollectionChanged;
            if (notifyCollectionChanged != null)
			{
                this._weakDataSourceCollectionChanged =
                    new WeakCollectionChangedHandler<DataManagerBase>
                        (
                            this,
                            notifyCollectionChanged,
                            (instance, s, e) => instance.DataSource_CollectionChanged(s, e)
                        );

                notifyCollectionChanged.CollectionChanged += this._weakDataSourceCollectionChanged.OnEvent;
			}


            if (this.IBindingListData != null && this.IBindingListData.SupportsChangeNotification)
            {
                this._weakBindingListChanged = 
                    new WeakEventHandler<DataManagerBase, IBindingList, ListChangedEventArgs>
                        (
                            this,
                            this.IBindingListData,
                            (instance, s, e) => instance.IBindingListData_ListChanged(s, e),
                            (weakHandler, eventSource) => eventSource.ListChanged -= weakHandler.OnEvent
                        );

                this.IBindingListData.ListChanged += this._weakBindingListChanged.OnEvent;
            }


            if (this.ICollectionViewData != null)
            {
                this._weakCollectionViewCurrentChanged =
                    new WeakEventHandler<DataManagerBase, ICollectionView, EventArgs>
                        (
                            this,
                            this.ICollectionViewData,
                            (instance, s, e) => instance.ICollectionViewData_CurrentChanged(s, e),
                            (weakHandler, eventSource) => eventSource.CurrentChanged -= weakHandler.OnEvent
                        );

                this.ICollectionViewData.CurrentChanged += this._weakCollectionViewCurrentChanged.OnEvent;
            }
		}        
      
		#endregion // SetDataSource

		#region GetDataSource
		/// <summary>
		/// Gets the <see cref="DataManagerBase.DataSource"/> associated with this <see cref="DataManagerBase"/>.
		/// </summary>
		protected virtual IEnumerable GetDataSource()
		{
			return this._dataSource;
		}
		#endregion // GetDataSource

		#region ResolveCount

		/// <summary>
		/// Determines the size of the collection by walking through the DataSource.
		/// </summary>
		/// <returns></returns>
		protected virtual int ResolveCount()
		{
			int count = 0;
			IEnumerable ds = this.GetDataSource();
			foreach (object item in ds)
				count++;
			return count;
		}

		#endregion // ResolveCount

		#region ResolveRecord

		/// <summary>
		/// Resolve the specified record at a given index. 
		/// </summary>
		/// <param name="recordIndex"></param>
		/// <returns></returns>
		/// <remarks> This method only gets called, if the data source isn't of type IList.</remarks>
		protected virtual object ResolveRecord(int recordIndex)
		{
			int index = 0;
			IEnumerable ds = this.GetDataSource();
			foreach (object item in ds)
			{
				if (index == recordIndex)
					return item;
				index++;
			}

			return null;
		}

		#endregion // ResolveCount

		#region ResolveFilteredSortedPagedDataSource
		/// <summary>
		/// Uses the existing paging, sorting, and filtering information to build a cached object for the
		/// DataManagerBase to use.
		/// </summary>
		protected virtual void ResolveFilteredSortedPagedDataSource()
		{

		}

		#endregion // ResolveFilteredSortedPagedDataSource

		#region ClearCachedDataSource

		/// <summary>
		/// Clears any cached information that the manager keeps.
		/// </summary>
		protected virtual void ClearCachedDataSource(bool invalidateTotalRowCount)
		{
			this._recordCount = -1;
			this._sortedFilterDataSource = null;

			this.IsSortedFilteredDataSourceCalculated = false;

			if (invalidateTotalRowCount)
				this._totalRecordCount = -1;

			this.InvalidateSortedFilterdDataSource();
		}

		#endregion // ClearCachedDataSource

		#region OnDataSourceCollectionChanged

		/// <summary>
		/// Triggered when the underlying data source's data is changed.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnDataSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
            bool clearCache = true;

            if (this._sortedFilterDataSource != null)
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    // Check if the items are not filtered.
                    IList filteredItems = FilterItems(e.NewItems);

                    // Add the items to the _sortedFilterDataSource if the items conform with the filter criterias
                    if (filteredItems != null && filteredItems.Count != 0)
                    {
                        clearCache = false;

                        foreach (var newItem in filteredItems)
                        {
                            int? index = ResolveIndexForInsertOrDelete(newItem, true);

                            if (index != null)
                            {
                                if (index < 0)
                                {
                                    index = ~index;
                                }

                                if (this.IsSorted)
                                {
                                    // If we are sorted, let's insert at the proper position ...
                                    this._sortedFilterDataSource.Insert(index.Value, newItem);
                                }
                                else
                                {
                                    // ... otherwise, just add the item in the end.
                                    this._sortedFilterDataSource.Add(newItem);
                                }

                                this.TotalRecordCount = -1;

                                if (!this.UpdateCachedDataManipulations(newItem, true))
                                {
                                    this.SetSortedFilteredDataSource(this._sortedFilterDataSource);    
                                }
                            }
                            else
                            {
                                // If the index is null (in case of GroupBy or Merging),
                                // let's mark that the cache should be cleared
                                clearCache = true;
                            }
                        }
                    }
                }
                else if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in e.OldItems)
                    {
                        int? index = ResolveIndexForInsertOrDelete(item, false);

                        if (index != null)
                        {
                            if (index >= 0)
                            {
                                this._sortedFilterDataSource.RemoveAt(index.Value);

                                this.TotalRecordCount = -1;
                                this.SetSortedFilteredDataSource(this._sortedFilterDataSource);
                            }

                            this.UpdateCachedDataManipulations(item, false);

                            // It's possible that this item was filtered out,
                            // so even if removed or not, there is no need to clear the cache.
                            clearCache = false;
                        }
                        else
                        {
                            




                            bool cacheUpdated = this.UpdateCachedDataManipulations(item, false);

                            clearCache = !cacheUpdated;
                        }
                    }
                }
            }

            if (clearCache)
            {
                this.ClearCachedDataSource(true);
            }

            this.OnCollectionChanged(e);
            this.OnDataUpdated();
		}
		#endregion // OnDataSourceCollectionChanged

		#region SetSortedFilteredDataSource

		/// <summary>
		/// Used to update the sorted, filtered, paged, and grouped data source.
		/// </summary>
		/// <param name="source"></param>
		protected virtual void SetSortedFilteredDataSource(IList source)
		{
			this._sortedFilterDataSource = source;

			if (source != null)
			{
				// Finally lets cache the new RecordCount
				this.RecordCount = source.Count;
			}
		}
		#endregion // SetSortedFilteredDataSource

		#endregion // Protected

		#region Private

		#region DetachFilterEvents

		private void DetachFilterEvents()
		{
			if (this._currentFilter != null)
			{
				this._currentFilter.CollectionChanged -= CurrentFilter_CollectionChanged;
				this._currentFilter.CollectionItemChanged -= CurrentFilter_CollectionItemChanged;
			}
		}

		#endregion DetachFilterEvents

		#region InvalidateSortedFilterdDataSource

		private void InvalidateSortedFilterdDataSource()
		{
			if (!this.SuspendInvalidateDataSource)
			{
				if (!this.IsSortedFilteredDataSourceCalculated)
				{
                    if(this._dataSource != null)
                    {
					    this.ResolveFilteredSortedPagedDataSource();
					    this.IsSortedFilteredDataSourceCalculated = true;
                    }
				}
			}
		}

		#endregion // InvalidateSortedFilterdDataSource

		#endregion   // Private

		#endregion //Methods

		#region Events

		#region CollectionChanged
		/// <summary>
		/// Occurs when the data source has changed and it implements <see cref="INotifyCollectionChanged"/>.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Raises the CollectionChanged event.
		/// </summary>
		/// <param name="e">Data about the collection being changed.</param>
		protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (CollectionChanged != null)
			{
				this.CollectionChanged(this, e);
			}
		}

		#endregion

		#region ResolvingData

		/// <summary>
		/// Event raised when paging, filtering, sorting, or groupby actions are changed.
		/// </summary>
		public event EventHandler<DataAcquisitionEventArgs> ResolvingData;

		/// <summary>
		/// Raises the <see cref="ResolvingData"/> event.
		/// </summary>
		/// <param name="args"></param>
		protected internal virtual void OnResolvingData(DataAcquisitionEventArgs args)
		{
			if (this.ResolvingData != null)
			{
				this.ResolvingData(this, args);
			}
		}

		#endregion // ResolvingData

		#region NewObjectGeneration
		/// <summary>
		/// Event raised when the <see cref="DataManagerBase"/> is attempting to create a new instance of the <see cref="CachedType"/> object.
		/// </summary>
		public event EventHandler<HandleableObjectGenerationEventArgs> NewObjectGeneration;

		/// <summary>
		/// Raises the <see cref="NewObjectGeneration"/> event.
		/// </summary>
		/// <returns></returns>
        protected internal virtual object OnNewObjectGeneration()
        {
            HandleableObjectGenerationEventArgs args = new HandleableObjectGenerationEventArgs();

            args.CollectionType = this.CollectionType;

            args.ObjectType = this.CachedType;

            args.Handled = false;

            if (this.NewObjectGeneration != null)
                this.NewObjectGeneration(this, args);

            if (args.Handled)
            {
                return args.NewObject;
            }

            return null;
        }
		#endregion // NewObjectGeneration

		#region DataUpdated

		/// <summary>
		/// Event raised when the underlying data changes.
		/// </summary>
		public event EventHandler<EventArgs> DataUpdated;

		/// <summary>
		/// Raises the <see cref="DataUpdated"/> event.
		/// </summary>
		protected virtual void OnDataUpdated()
		{
			if (this.DataUpdated != null)
			{
				this.DataUpdated(this, new EventArgs());
			}

		}
		#endregion // DataUpdated

        #region DataUpdated

        /// <summary>
        /// Event raised when the currentItem changes.
        /// </summary>
        public event EventHandler<CurrentItemEventArgs> CurrentItemChanged;

        /// <summary>
        /// Raises the <see cref="CurrentItemChanged"/> event.
        /// </summary>
        /// <param name="item"></param>
        protected virtual void OnCurrentItemChanged(object item)
        {
            if (this.CurrentItemChanged != null)
            {
                this.CurrentItemChanged(this, new CurrentItemEventArgs() { Item = item });
            }

        }
        #endregion // DataUpdated

		#endregion // Events

		#region EventHandlers

		private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.OnDataSourceCollectionChanged(e);
		}

		private void CurrentFilter_CollectionItemChanged(object sender, EventArgs e)
		{
			this.ClearCachedDataSource(false);
		}

		private void CurrentFilter_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.ClearCachedDataSource(false);
		}

		private void CurrentSummaries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.ClearCachedDataSource(false);
		}

        void ICollectionViewData_CurrentChanged(object sender, EventArgs e)
        {
            if (!this._ignoreCurrentChanging)
            {
                this.OnCurrentItemChanged(this.ICollectionViewData.CurrentItem);
            }
        }


        void IBindingListData_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.ListChangedType == ListChangedType.ItemAdded || e.ListChangedType == ListChangedType.ItemDeleted ||
                e.ListChangedType == ListChangedType.ItemMoved || e.ListChangedType == ListChangedType.Reset)
            {
                if(!this.SuspendInvalidateDataSource && !this.Defer)
                    this.ClearCachedDataSource(true);
            }
            else if (e.ListChangedType == ListChangedType.ItemChanged && e.PropertyDescriptor != null)
            {
                
            }

            NotifyCollectionChangedEventArgs args = null;
            if (e.ListChangedType == ListChangedType.ItemAdded)
            {
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<object>());
            }
            else if (e.ListChangedType == ListChangedType.ItemDeleted)
            {
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new List<object>());
            }
            else if (e.ListChangedType == ListChangedType.Reset)
            {
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            }
            else if (e.ListChangedType == ListChangedType.ItemMoved)
            {
                args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<object>());
            }

            if (args != null)
                this.OnDataSourceCollectionChanged(args);

        }


        #endregion
	}

	#region TypelessDataManager

	/// <summary>
	/// Wraps an IEnumerable to get items while using IList or IQueryable to improve performance if available.
	/// This particular DataManager doesn't support Sorting, Filtering, or Paging.
	/// </summary>
	/// <remarks>
	/// Currently this DataManager is only used if the underlying data is an Anonymous type.
	/// </remarks>
	public class TypelessDataManager : DataManagerBase
	{
		#region SetDataSource
		/// <summary>
		/// Sets the <see cref="DataManagerBase.DataSource"/> while registering for change notification.
		/// </summary>
		/// <param name="source">The IEnumerable to set the DataSource to.</param>
		protected override void SetDataSource(IEnumerable source)
		{
			this.OriginalDataSource = source;
			base.SetDataSource(source);
		}
		#endregion // SetDataSource

	}

	#endregion // TypelessDataManager

	/// <summary>
	/// Wraps an IEnumerable to get items while using IList or IQueryable to improve performance if available.
	/// </summary>
	/// <typeparam name="T">The type of the IEnumerable<![CDATA[<T>]]> that this data manager manages.</typeparam>
	public class DataManager<T> : DataManagerBase
	{
		#region Members

		IEnumerable<T> _dataSource;
		List<T> _cachedFilterSortedList;
		private bool _needSort, _needGroupBy, _needPaging, _needFiltering, _needSummary, _needConditionalFormat, _needMerge;

        private MultiSortComparer<T> _multiSortComparer;


		#endregion // Members

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="DataManagerBase"/> class.
		/// </summary>
		public DataManager()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DataManagerBase"/> class and sets the <see cref="DataManagerBase.DataSource"/>.
		/// </summary>
		/// <param name="dataSource">The datasource that this datamanager should manage.</param>
		public DataManager(IEnumerable<T> dataSource)
			: this()
		{
			this.SetDataSource(dataSource);
		}
		#endregion // Constructor

		#region Methods

        #region Public

        #region RefreshSummaries

        /// <summary>
        /// Reevaluates the summaries for the ItemsSource bound to this <see cref="DataManagerBase"/>.
        /// </summary>
        public override void RefreshSummaries()
        {
            this._needSummary = true;
            this._needSort = true;
            this._needPaging = true;
            this._needFiltering = true;
            this._needGroupBy = this.GroupByObject != null;
            this.SummaryResultCollection.Clear();
            this.ApplyClientDataManipulations(false);
        }

        #endregion // RefreshSummaries

        #endregion // Public

        #region Protected

        #region SetDataSource
        /// <summary>
		/// Sets the <see cref="DataManagerBase.DataSource"/> while registering for change notification.
		/// </summary>
		/// <param name="source">The IEnumerable to set the DataSource to.</param>
		protected override void SetDataSource(IEnumerable source)
		{
			if (source != null)
			{
				IEnumerable<T> ds = source as IEnumerable<T>;
				if (ds == null)
					this._dataSource = source.Cast<T>();
				else
					this._dataSource = ds;

				this.OriginalDataSource = source;
			}
			else
			{
				this._dataSource = null;
				this.OriginalDataSource = null;
			}

			base.SetDataSource(this._dataSource);
		}

		#endregion // SetDataSource

		#region GetDataSource
		/// <summary>
		/// Gets the <see cref="DataManagerBase.DataSource"/> associated with this <see cref="DataManagerBase"/>.
		/// </summary>
		protected override IEnumerable GetDataSource()
		{
			return this._dataSource;
		}
		#endregion // GetDataSource

		#region ResolveRecord
		/// <summary>
		/// Resolve the specified record at a given index. 
		/// </summary>
		/// <param name="recordIndex"></param>
		/// <returns></returns>
		/// <remarks> This method only gets called, if the data source isn't of type IList.</remarks>
		protected override object ResolveRecord(int recordIndex)
		{
			IQueryable<T> query = this._dataSource.AsQueryable<T>();
			return query.Skip(recordIndex).Take(1).ToList<T>()[0];
		}
		#endregion // ResolveRecord

		#region ResolveCount
		/// <summary>
		/// Determines the size of the collection by walking through the DataSource.
		/// </summary>
		/// <returns></returns>
		protected override int ResolveCount()
		{
            if (this.IListData != null)
            {
                return this.IListData.Count;
            }

			int count = (from a in this._dataSource
						 select a).Count();

			return count;
		}
		#endregion // ResolveCount

		#region OnResolvingData

		/// <summary>
		/// Raises the ResolvingData event.
		/// </summary>
		/// <param name="args"></param>
		protected internal override void OnResolvingData(DataAcquisitionEventArgs args)
		{
			base.OnResolvingData(args);

			if (args != null)
			{
				args.Handled = args.DataSource != null;
			}
		}
		#endregion // OnResolvingData

		#region ClearCachedDataSource

		/// <summary>
		/// Clears any cached information that the manager keeps.
		/// </summary>
		protected override void ClearCachedDataSource(bool invalidateTotalRowCount)
		{
			this._cachedFilterSortedList = null;

			IList source = this.SortedFilteredDataSource;

			base.ClearCachedDataSource(invalidateTotalRowCount);

			if (source != null && this.SortedFilteredDataSource == null && !this.Defer)
				this.OnDataUpdated();
		}

		#endregion // ClearCachedDataSource

		#region OnDataSourceCollectionChanged

		/// <summary>
		/// Triggered when the underlying data source's data is changed.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnDataSourceCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (this.ICollectionViewData != null)
			{
				this.TotalRecordCount = -1;
				this.RecordCount = -1;

				this.OnCollectionChanged(e);

				if (this.ICollectionViewData.Groups != null)
				{
                    this.ApplyServerGrouping();
				}

                if (!this.Defer && this.ICollectionViewData.Groups == null)
                {
                    base.OnDataSourceCollectionChanged(e);
                }
                else
                {
                    if (this.IPagedCollectionViewData != null)
                    {
                        this.ApplyServerPaging();
                    }

                    this.ApplyClientDataManipulations();
                }

                this.Defer = false;

				this.OnDataUpdated();
			}
			else
            {
                base.OnDataSourceCollectionChanged(e);
            }
        }

		#endregion // OnDataSourceCollectionChanged        

		#region ResolveFilteredSortedPagedDataSource
		/// <summary>
		/// Uses the existing paging, sorting, and filtering information to build a cached object for the
		/// DataManagerBase to use.
		/// </summary>
		protected override void ResolveFilteredSortedPagedDataSource()
		{
			bool isAsyncSort = false, isAsyncFilter = false, isAsyncPaging = false, isAsyncGroupBy = false, isAsyncMerging = false;
			bool needSort = false, needGroupBy = false, needFiltering = false, needPaging = false, needMerging = false;
			
			if (this.SummaryResultCollection != null)
			{
				this.SummaryResultCollection.Clear();
			}

            if (this.ICollectionViewData != null && this.AllowCollectionViewOverrides)
			{
				isAsyncSort = this.ICollectionViewData.CanSort;
				isAsyncMerging = isAsyncGroupBy = this.ICollectionViewData.CanGroup;

				// SORTING
				if (isAsyncSort)
				{
					SortDescriptionCollection sorts = this.ICollectionViewData.SortDescriptions;
					int sortCount = this.Sort.Count + ((this.GroupBySortContext != null) ? 1 : 0);

                    // Since merging , like groupby requries sortings, take it into consideration
                    sortCount += this.MergeDataContexts.Count;
                    
					if (sorts.Count != sortCount)
					{
						needSort = true;
					}
					else
					{
						int index = 0;

						if (this.GroupBySortContext != null)
						{
							SortDescription sd = sorts[0];
							bool sdSort = sd.Direction == ListSortDirection.Ascending;
							bool gbSort = this.GroupBySortContext.SortAscending;
							if (sdSort != gbSort || sd.PropertyName != this.GroupBySortContext.SortPropertyName)
								needSort = true;

							index = 1;
						}

                        // Loop through the MergedDataContexts and determine if we need to sort
                        foreach (MergedDataContext mdc in this.MergeDataContexts)
                        {
                            SortDescription sd = sorts[index];
                            bool sdSort = sd.Direction == ListSortDirection.Ascending;
                            bool gbSort = mdc.SortContext.SortAscending;
                            if (sdSort != gbSort || sd.PropertyName != mdc.SortContext.SortPropertyName)
                                needSort = true;

                            index++;
                        }

						int count = this.Sort.Count;
						for (int i = 0; i < count; i++)
						{
							SortContext context = this.Sort[i];
							SortDescription sd = sorts[i + index];
							bool sdSort = sd.Direction == ListSortDirection.Ascending;
							bool contextSort = context.SortAscending;
							if (contextSort != sdSort || sd.PropertyName != context.SortPropertyName)
							{
								needSort = true;
								break;
							}
						}
					}
				}

				// GROUP-BY
				if (isAsyncGroupBy)
				{
					if (this.GroupByObject != null)
					{
						if (this.ICollectionViewData.GroupDescriptions.Count == 0 || ((PropertyGroupDescription)this.ICollectionViewData.GroupDescriptions[0]).PropertyName != this.GroupByObject.PropertyName)
						{
							needGroupBy = true;
						}
					}
					else if (this.ICollectionViewData.GroupDescriptions != null && this.ICollectionViewData.GroupDescriptions.Count > 0)
					{
                        if(this.MergeDataContexts.Count == 0)
						    needGroupBy = true;
					}
				}

                if (isAsyncMerging && this.GroupByObject == null)
                {
                    if (this.SummariesDirty && this.MergeDataContexts.Count > 0)
                        needMerging = true;
                    else
                    {
                        if (this.MergeDataContexts.Count != this.ICollectionViewData.GroupDescriptions.Count)
                        {
                            needMerging = true;
                        }
                        else
                        {
                            int index = 0;
                            foreach (MergedDataContext mdc in this.MergeDataContexts)
                            {
                                PropertyGroupDescription pgd = this.ICollectionViewData.GroupDescriptions[index] as PropertyGroupDescription;
                                if (pgd == null || mdc.PropertyName != pgd.PropertyName)
                                {
                                    needMerging = true;
                                    break;
                                }
                                index++;
                            }
                        }
                    }
                }
			}

			// Filtering
			if (this.IFilteredCollectionViewData != null && this.Filters != null)
			{
				isAsyncFilter = this.IFilteredCollectionViewData.CanFilter;

				if (isAsyncFilter)
				{
					// Determine if filtering needs to recalculated.
					if (this.Filters.Count == 0 && this.IFilteredCollectionViewData.FilterConditions != null && this.IFilteredCollectionViewData.FilterConditions.Count > 0)
					{
						needFiltering = true;
					}
					else
					{
						
					}
				}
			}

			// Paging
			if (this.IPagedCollectionViewData != null)
			{
				isAsyncPaging = true;
			}

			if (needSort || needGroupBy || needFiltering || needMerging)
			{
				//If we don't do this check here, then just clearing during a referesh, causes an InvalidOperation exception
                if (needSort && !needGroupBy && !needFiltering && this.Sort.Count == 0 && this.GroupBySortContext == null && this.MergeDataContexts.Count == 0)
                {
                    this.Defer = true;
                    this.ICollectionViewData.SortDescriptions.Clear();
                }
                else
                {
                    using (this.ICollectionViewData.DeferRefresh())
                    {
                        if (needSort)
                        {
                            this.ICollectionViewData.SortDescriptions.Clear();
                            int count = this.Sort.Count;

                            if (count != 0 || this.GroupBySortContext != null || this.MergeDataContexts.Count > 0)
                            {
                                int index = 0;

                                if (this.GroupBySortContext != null)
                                {
                                    this.ICollectionViewData.SortDescriptions.Add(new SortDescription(this.GroupBySortContext.SortPropertyName, (this.GroupBySortContext.SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending));
                                }
                                else if (this.MergeDataContexts.Count > 0)
                                {
                                    // Add SortDescriptions for merged data.
                                    foreach (MergedDataContext mdc in this.MergeDataContexts)
                                    {
                                        this.ICollectionViewData.SortDescriptions.Add(new SortDescription(mdc.SortContext.SortPropertyName, (mdc.SortContext.SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending));
                                    }
                                }
                                else
                                {
                                    this.ICollectionViewData.SortDescriptions.Add(new SortDescription(this.Sort[0].SortPropertyName, (this.Sort[0].SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending));
                                    index = 1;
                                }

                                for (int i = index; i < count; i++)
                                    this.ICollectionViewData.SortDescriptions.Add(new SortDescription(this.Sort[i].SortPropertyName, (this.Sort[i].SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending));
                            }
                        }

                        if (needFiltering)
                        {
                            
                        }

                        // for now, b/c there is no actual thing as async filtering, we need to disable it:
                        this._needFiltering = false;

                        if (needGroupBy)
                        {
                            if (this.GroupByObject != null)
                            {
                                // We can only support 1 level.
                                this.ICollectionViewData.GroupDescriptions.Clear();
                                this.ICollectionViewData.GroupDescriptions.Add(new PropertyGroupDescription(this.GroupByObject.PropertyName));
                            }
                            else
                                this.ICollectionViewData.GroupDescriptions.Clear();
                        }

                        if (needMerging)
                        {
                            this.ICollectionViewData.GroupDescriptions.Clear();

                            foreach (MergedDataContext mdc in this.MergeDataContexts)
                            {
                                this.ICollectionViewData.GroupDescriptions.Add(new PropertyGroupDescription(mdc.PropertyName));
                            }
                        }

                        this.Defer = true;
                    }
                }

				needSort = needFiltering = needGroupBy = false;
			}

            bool needsForcedUpdate = false;


            if (!isAsyncSort && this.IBindingLisViewtData != null && this.IBindingLisViewtData.SupportsSorting && this.CachedTypedInfo.PropertyDescriptors != null && this.CachedTypedInfo.PropertyDescriptors.Count > 0)
            {
                isAsyncSort = true;

                ListSortDescriptionCollection sorts = this.IBindingLisViewtData.SortDescriptions;
				int sortCount = this.Sort.Count + ((this.GroupBySortContext != null) ? 1 : 0);

                // Since merging , like groupby requries sortings, take it into consideration
                sortCount += this.MergeDataContexts.Count;
                    
				if (sorts.Count != sortCount)
				{
					needSort = true;
				}
				else
				{
					int index = 0;

					if (this.GroupBySortContext != null)
					{
						ListSortDescription sd = sorts[0];
						bool sdSort = sd.SortDirection == ListSortDirection.Ascending;
						bool gbSort = this.GroupBySortContext.SortAscending;
						if (sdSort != gbSort || sd.PropertyDescriptor.Name != this.GroupBySortContext.SortPropertyName)
							needSort = true;

						index = 1;
					}

                    // Loop through the MergedDataContexts and determine if we need to sort
                    foreach (MergedDataContext mdc in this.MergeDataContexts)
                    {
                        ListSortDescription sd = sorts[index];
                        bool sdSort = sd.SortDirection == ListSortDirection.Ascending;
                        bool gbSort = mdc.SortContext.SortAscending;
                        if (sdSort != gbSort || sd.PropertyDescriptor.Name != mdc.SortContext.SortPropertyName)
                            needSort = true;

                        index++;
                    }

					int count = this.Sort.Count;
					for (int i = 0; i < count; i++)
					{
						SortContext context = this.Sort[i];
						ListSortDescription sd = sorts[i + index];
						bool sdSort = sd.SortDirection == ListSortDirection.Ascending;
						bool contextSort = context.SortAscending;
						if (contextSort != sdSort || sd.PropertyDescriptor.Name != context.SortPropertyName)
						{
							needSort = true;
							break;
						}
					}
				}

                if (needSort)
                {
                    ListSortDescriptionCollection lsdc = this.IBindingLisViewtData.SortDescriptions;                    
                    int lsdcCount = lsdc.Count;                    

                    for(int i = 0; i < lsdcCount; i++)
                    {
                        bool deferVal = this.Defer;
                        this.Defer = true;
                        this.IBindingLisViewtData.RemoveSort();
                        this.Defer = deferVal;
                    }

                    CachedTypedInfo cti = this.CachedTypedInfo;

                    List<PropertyDescriptor> pds = new List<PropertyDescriptor>();

                    foreach(SortContext sc in this.Sort)
                    {
                        PropertyDescriptor pd = cti.PropertyDescriptors[sc.SortPropertyName];
                        if(pd != null)
                            pds.Add(pd);
                    }

                    int count = pds.Count;

                    ListSortDescription[] newSorts = null;

                    if (count != 0 || this.GroupBySortContext != null || this.MergeDataContexts.Count > 0)
                    {
                        int index = 0;
                        int offset = 0; 
                        if (this.GroupBySortContext != null)
                        {

                            PropertyDescriptor pd = cti.PropertyDescriptors[this.GroupBySortContext.SortPropertyName];
                            if(pd != null)
                            {
                                newSorts = new ListSortDescription[count + 1];
                                newSorts[0] = new ListSortDescription(pd, (this.GroupBySortContext.SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending);
                                offset = 1;
                            }
                            else
                            {
                                newSorts = new ListSortDescription[count];
                            }
                        }
                        else if (this.MergeDataContexts.Count > 0)
                        {
                            List<PropertyDescriptor> mpds = new List<PropertyDescriptor>();
                            foreach(MergedDataContext mdc in this.MergeDataContexts)
                            {
                                PropertyDescriptor pd = cti.PropertyDescriptors[mdc.SortContext.SortPropertyName];
                                if(pd != null)
                                    mpds.Add(pd);
                            }
                            newSorts = new ListSortDescription[count + mpds.Count];

                            // Add SortDescriptions for merged data.
                            for(int i = 0; i < this.MergeDataContexts.Count; i++)
                            {
                                MergedDataContext mdc = this.MergeDataContexts[i];
                                if (mdc.SortContext.SortPropertyName == mpds[i].Name)
                                    newSorts[i] = new ListSortDescription(mpds[i], (mdc.SortContext.SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending);
                            }

                            offset = mpds.Count;
                        }
                        else
                        {
                            newSorts = new ListSortDescription[count];
                            newSorts[index] = new ListSortDescription(pds[index], (this.Sort[0].SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending);
                            index = 1;
                        }

                        for (int i = index; i < this.Sort.Count; i++)
                        {
                            SortContext sc = this.Sort[i];
                            if(sc.SortPropertyName == pds[i].Name)
                                newSorts[i + offset] = new ListSortDescription(pds[i], (sc.SortAscending) ? ListSortDirection.Ascending : ListSortDirection.Descending);
                        }
                         
                    }

                    if (newSorts != null)
                    {
                        bool deferVal = this.Defer;
                        this.Defer = true;
                        
                        needSort = needFiltering = needGroupBy = false;                        

                        this.IBindingLisViewtData.ApplySort(new ListSortDescriptionCollection(newSorts));

                        needsForcedUpdate = true;

                        this.Defer = deferVal;
                    }
                }
            }


			if (isAsyncPaging && !this.Defer)
			{
				this.ApplyServerPaging();
				this.Defer = needPaging = this.IPagedCollectionViewData.IsPageChanging;
			}

			if (!this.SupportsDataManipulations)
				needSort = false;
			else if (this.ICollectionViewData == null && this.IPagedCollectionViewData == null && this.IFilteredCollectionViewData == null)
			{
				DataAcquisitionEventArgs args = new DataAcquisitionEventArgs()
				{
					CurrentPage = this.CurrentPage,
					EnablePaging = this.EnablePaging,
					PageSize = this.PageSize,
					CurrentSort = new ObservableCollection<SortContext>(),
					GroupByContext = this.GroupByObject,
					Filters = this.Filters
				};

				foreach (SortContext sc in this.Sort)
					args.CurrentSort.Add(sc);

				this.OnResolvingData(args);

				if (args.Handled)
				{
					this.SetSortedFilteredDataSource(args.DataSource);
					this.OnDataUpdated();
					return;
				}
			}

			if (!isAsyncSort)
				needSort = this.Sort.Count > 0 || this.GroupByObject != null || this.MergeDataContexts.Count > 0;

			if (!isAsyncFilter && (!isAsyncGroupBy || (isAsyncGroupBy && this.GroupByObject == null)))
				needFiltering = (this.Filters != null && this.Filters.Count > 0);

            if (!isAsyncGroupBy)
                needGroupBy = this.GroupByObject != null;
           
			if (!isAsyncPaging)
				needPaging = this.EnablePaging && this.PageSize > 0;

			this._needSummary = (this.Summaries != null && this.Summaries.Count > 0);

			this._needConditionalFormat = this.ConditionalFormattingRules!=null&& this.ConditionalFormattingRules.Count > 0;

			if (!this.SupportsDataManipulations)
				needSort = false;

			this._needFiltering = needFiltering;
			this._needGroupBy = needGroupBy;
			this._needSort = needSort;
			this._needPaging = needPaging;
            this._needMerge = needMerging;

            // Update the need merging flag for ClientDataManipulations
            if (this.MergeDataContexts.Count > 0)
            {
                if (!isAsyncMerging)
                {
                    this._needMerge = true;
                    this._needSort = true;
                }
            }

			if (!this.Defer)
			{
                bool updateDataSource = true;

                // Since MergedData is calculated, if we're in a CollectionView, and we were basically told to invalidate
                // and we didn't, then we need to update the sortedFilterdDataSource appropritatly . 
                if (isAsyncGroupBy && (this.MergeDataContexts.Count > 0 || (this.ICollectionViewData.Groups != null && this.ICollectionViewData.Groups.Count > 0)))
                {
                    this.ApplyServerGrouping();
                    updateDataSource = false;
                }

                if (!this.ApplyClientDataManipulations(updateDataSource) && needsForcedUpdate)
                {
                    this.OnDataUpdated();
                }
			}

            this.SummariesDirty = false;
		}
		#endregion // ResolveFilteredSortedPagedDataSource

        #region ResolveIndexForInsertOrDelete

        /// <summary>
        /// Looks through the filtered DataSource for the index of the item specified. 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="isAdding"></param>
        /// <returns></returns>
        internal override int? ResolveIndexForInsertOrDelete(object data, bool isAdding)
        {
            if (this.IsSortedFilteredDataSourceCalculated && this.IsSorted && this._multiSortComparer != null)
            {
                var list = this.SortedFilteredDataSource as List<T>;

                if (list != null && data is T)
                {
                    return this.ResolveIndexForInsertOrDelete(list, (T)data, isAdding);
                }
            }

            return null;
        }

        private int? ResolveIndexForInsertOrDelete(List<T> list, T data, bool isAdding)
        {
            int? result = null;

            if (this.IsSortedFilteredDataSourceCalculated && this.IsSorted && this._multiSortComparer != null)
            {
                int index = list.BinarySearch(data, this._multiSortComparer);

                if (isAdding)
                {
                    if (index < 0)
                    {
                        result = ~index;
                    }
                }
                else
                {
                    bool found = false;

                    // Worst case scenario ... the binarySearch with multiSort comaprer didn't find the item,
                    // we'll have to manually walk around the nearest point where the item could be...
                    if (index < 0)
                    {
                        int count = list.Count;
                        int nearestIndex = ~index;

                        int back = nearestIndex - 1;
                        int forth = nearestIndex + 1;
                        bool hitStart = false, hitEnd = false;

                        while (!(hitStart && hitEnd))
                        {
                            if (back < 0)
                            {
                                hitStart = true;
                            }
                            else
                            {
                                object obj1 = list[back]; // possible boxing
                                object obj2 = data;

                                if (obj1 == obj2)
                                {
                                    index = back;
                                    found = true;
                                    break;
                                }

                                back--;
                            }

                            if (forth >= count)
                            {
                                hitEnd = true;
                            }
                            else
                            {
                                object obj1 = list[forth]; // possible boxing
                                object obj2 = data;

                                if (obj1 == obj2)
                                {
                                    index = forth;
                                    found = true;
                                    break;
                                }

                                forth++;
                            }
                        }
                    }
                    else
                    {
                        found = true;
                    }

                    if (found)
                    {
                        result = index;
                    }
                }
            }

            return result;
        }

        #endregion // ResolveIndexForInsertOrDelete

        #region UpdateCachedDataManipulations

        /// <summary>
        /// Updates the cached data manipulations.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="isAdding">if set to <c>true</c> the item will be added to the cached lists, otherwise removed.</param>
        /// <returns><c>true</c> if the cache was updated, otherwise <c>false</c>.</returns>
        internal override bool UpdateCachedDataManipulations(object item, bool isAdding)
        {
            bool cacheUpdated = base.UpdateCachedDataManipulations(item, isAdding);

            if (_cachedFilterSortedList != null && this._multiSortComparer != null && item is T)
            {
                bool applyClientDataManipulations = false;

                T data = (T)item;
                int? index = this.ResolveIndexForInsertOrDelete(_cachedFilterSortedList, data, isAdding);

                if (index != null)
                {
                    if (isAdding)
                    {
                        _cachedFilterSortedList.Insert(index.Value, data);
                    }
                    else
                    {
                        _cachedFilterSortedList.RemoveAt(index.Value);
                    }

                    applyClientDataManipulations = true;
                    cacheUpdated = true;
                }

                if (applyClientDataManipulations)
                {
                    this._needSort = true;
                    this._needPaging = true;
                    this._needFiltering = true;
                    this.ApplyClientDataManipulations(true);
                }
            }

            return cacheUpdated;
        }

        #endregion // UpdateCachedDataManipulations

        #region FilterItems

        /// <summary>
        /// Filters a list of items using the filter operands applied on the <see cref="DataManagerBase"/>.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <returns>Filtered list or null if filtering cannot be applied.</returns>
        /// <remarks>
        /// This method is added solely for the purpose of fast adding of an item in sorted/filtered list.
        /// </remarks>
        internal override IList FilterItems(IList items)
        {
            IQueryable<T> filteredItems = items.OfType<T>().AsQueryable();

            filteredItems = ApplyFilter(filteredItems);

            return filteredItems.ToList(); // <-- Perf Hit.
        }

        #endregion // FilterItems

        #region SortedFilteredList



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal override IList SortedFilteredList
	    {
	        get
	        {
	            if (this._cachedFilterSortedList != null)
	            {
	                return this._cachedFilterSortedList;
	            }

	            return base.SortedFilteredList;
	        }
	    }

        #endregion // SortedFilteredList

        #endregion // Protected

        #region Private

        #region ApplySort

        private IQueryable<T> ApplySort(IQueryable<T> query)
		{
			int count = this.Sort.Count;

			if (count == 0 && this.GroupBySortContext == null && this.MergeDataContexts.Count == 0)
				return query;

			IOrderedQueryable<T> singleSort = null;
            List<SortContext> sortContexts = new List<SortContext>();

			if (this.GroupBySortContext != null)
			{
				singleSort = this.GroupBySortContext.Sort<T>(query);
                sortContexts.Add(this.GroupBySortContext);

				if (count > 0)
				{
				    singleSort = this.Sort[0].AppendSort<T>(singleSort);
                    sortContexts.Add(this.Sort[0]);
				}
			}
            else if (this.MergeDataContexts.Count > 0)
            {
                int index = 0;
                foreach(MergedDataContext mdc in this.MergeDataContexts)
                {
                    if (index == 0)
                        singleSort = mdc.SortContext.Sort<T>(query);
                    else
                        singleSort = mdc.SortContext.AppendSort<T>(singleSort);

                    index++;
                    sortContexts.Add(mdc.SortContext);
                }

                if (count > 0)
                {
                    singleSort = this.Sort[0].AppendSort<T>(singleSort);
                    sortContexts.Add(this.Sort[0]);
                }
            }
            else
            {
                singleSort = this.Sort[0].Sort<T>(query);
                sortContexts.Add(this.Sort[0]);
            }

			for (int i = 1; i < count; i++)
			{
				singleSort = this.Sort[i].AppendSort<T>(singleSort);
                sortContexts.Add(this.Sort[i]);
			}


            this._multiSortComparer = new MultiSortComparer<T>(sortContexts);

			return singleSort;
		}

		#endregion // ApplySort

		#region ApplyFilter

		private IQueryable<T> ApplyFilter(IQueryable<T> query)
		{
			if (this.Filters != null && this.Filters.Count > 0)
			{
				FilterContext fc = FilterContext.CreateGenericFilter(this.CachedTypedInfo, null, false, false);

				System.Linq.Expressions.Expression expression = ((IGroupFilterConditions)this.Filters).GetCurrentExpression(fc);
				if (expression != null)
				{
					query = query.Where<T>((Expression<Func<T, bool>>)expression);
				}
			}
			return query;
		}

		#endregion // ApplyFilter

		#region ApplySummary

		private void ApplySummary(IQueryable<T> query, SummaryExecution summaryExecution)
		{
			if (this.Summaries == null || this.SummaryResultCollection == null)
				return;

			ReadOnlyCollection<SummaryDefinition> sums = this.Summaries.GetDefinitionsBySummaryExecution(summaryExecution, summaryExecution == this.SummaryExecution);

			foreach (SummaryDefinition sd in sums)
			{
				ISupportLinqSummaries isls = sd.SummaryOperand.SummaryCalculator as ISupportLinqSummaries;
				if (isls != null)
				{
					SummaryContext sc = SummaryContext.CreateGenericSummary(this.CachedTypedInfo, sd.ColumnKey, isls.SummaryType);
					isls.SummaryContext = sc;
					this.SummaryResultCollection.Add(new SummaryResult(sd, sc.Execute(query)));
					continue;
				}
				SynchronousSummaryCalculator ssc = sd.SummaryOperand.SummaryCalculator as SynchronousSummaryCalculator;
				if (ssc != null)
				{
					this.SummaryResultCollection.Add(new SummaryResult(sd, ssc.Summarize(query, sd.ColumnKey)));
				}
			}
		}
		#endregion // ApplySummary

		#region ApplyServerPaging

		private void ApplyServerPaging()
		{
			if (!this.IPagedCollectionViewData.IsPageChanging && this.IPagedCollectionViewData.TotalItemCount != -1)
			{
				int currentPage = this.CurrentPage - 1;

				if (currentPage != this.IPagedCollectionViewData.PageIndex && this.IPagedCollectionViewData.CanChangePage)
				{
					this.IPagedCollectionViewData.MoveToPage(currentPage);
				}
			}
		}

		#endregion // ApplyServerPaging

        #region ApplyServerGrouping

        private void ApplyServerGrouping()
        {
            if (this.ICollectionViewData.Groups != null)
            {
                if (this.MergeDataContexts.Count > 0)
                {
                    MergedDataContext mdc = this.MergeDataContexts[0];
                    List<MergedRowInfo> mris = new List<MergedRowInfo>();
                    this.BuildMergedGroupings(mris, this.ICollectionViewData.Groups, new List<MergedColumnInfo>(), mdc, new Dictionary<MergedColumnInfo, bool>());

                    this.SetSortedFilteredDataSource(mris);
                }
                else
                {
                    List<GroupByDataContext> groups = new List<GroupByDataContext>();
                
                    foreach (object data in this.ICollectionViewData.Groups)
                    {
                        CollectionViewGroup group = data as CollectionViewGroup;
                        if (group != null)
                        {
                            GroupByDataContext gbc = new GroupByDataContext();

                            while (!group.IsBottomLevel)
                            {
                                group = group.Items[0] as CollectionViewGroup;
                            }

                            gbc.Records = group.Items;
                            gbc.Value = group.Name;
                            groups.Add(gbc);
                        }
                    }

                    this.SetSortedFilteredDataSource(groups);
                }
            }
        }

        #endregion // ApplyServerGrouping

        #region GatherDataForConditionalFormatting

        private void GatherDataForConditionalFormatting(IQueryable<T> query, EvaluationStage stage)
		{
			if (this.ConditionalFormattingRules == null || this.ConditionalFormattingRules.Count == 0)
				return;

			ReadOnlyCollection<IRule> rules = this.ConditionalFormattingRules.GetRulesForStage(stage);

			if (rules.Count == 0)
				return;


			foreach (IRule rule in rules)
			{
				rule.GatherData(query);
			}			
		}

        #endregion // GatherDataForConditionalFormatting

        #region ApplyClientDataManipulations

        /// <summary>
        /// Applies all Clientside related data manipulations, such as sorting, paging, groupBy and filtering.
        /// </summary>
        protected bool ApplyClientDataManipulations()
        {
            return this.ApplyClientDataManipulations(true);
        }

		/// <summary>
		/// Applies all Clientside related data manipulations, such as sorting, paging, groupBy and filtering.
		/// </summary>
        /// <param name="setSortedFilteredDataSource">True if the internal filtered datasource should be set.</param>
		protected bool ApplyClientDataManipulations(bool setSortedFilteredDataSource)
		{
            // If we are doing any maniuplations of GroupBy, using the ICollectionView, then we can't handle any data maniuplations ourselves.
            if ((this.ICollectionViewData == null || (this.ICollectionViewData.Groups == null || this.MergeDataContexts.Count > 0) ) && !this.SuspendInvalidateDataSource && (this._needSort || this._needMerge || this._needFiltering || this._needGroupBy || this._needPaging || this._needSummary || this._needConditionalFormat))			
			{
                if(this.SummaryResultCollection != null)
                    this.SummaryResultCollection.Clear();

				IList source = null;

				IQueryable<T> query = from item in this._dataSource.AsQueryable<T>()
									  select item;

				this.ApplySummary(query, SummaryExecution.PriorToFilteringAndPaging);

				this.GatherDataForConditionalFormatting(query, EvaluationStage.PriorToFilteringAndPaging);

				IQueryable<T> sortedQuery = query;
				IQueryable<GroupByDataContext> groupQuery = null;

                if (this._needFiltering)
                    sortedQuery = this.ApplyFilter(sortedQuery);

                this.IsSorted = false;

				if (this._needSort)
				{
				    sortedQuery = this.ApplySort(sortedQuery);
                    this.IsSorted = true;
				}

				if (this._needGroupBy)
				{
                    this.GroupByObject.Summaries = this.Summaries;
					source = this.GroupByObject.Group<T>(sortedQuery);
					sortedQuery = null;
				}

                // Apply merging, similar to how we apply groupBy
                if (this._needMerge && this.MergeDataContexts.Count > 0)
                {
                    source = this.MergeDataContexts[0].Merge(sortedQuery, this.MergeDataContexts, this.Summaries);
                    sortedQuery = null;
                }
				
				if (sortedQuery != null)
				{
					this.ApplySummary(sortedQuery, SummaryExecution.AfterFilteringBeforePaging);
					this.GatherDataForConditionalFormatting(sortedQuery, EvaluationStage.AfterFilteringBeforePaging);
				}

				if (this._needPaging && this.IPagedCollectionViewData == null)
				{
					int count = this.TotalRecordCount;

					if (!this._needGroupBy && !this._needMerge)
					{
						// If we're sorting or filtering, lets cache off the Lis<T> so that if a user changes the page, we don't re-evaluate the sort and filter.
						if ((this._needSort || this._needFiltering) && this._cachedFilterSortedList == null)
						{
							this._cachedFilterSortedList = sortedQuery.ToList<T>(); // <-- Perf Hit. (Resolves the query for Sorting and Filtering)
						}

						// Lets calculate the PageCount
						if (this._needFiltering)
							count = this._cachedFilterSortedList.Count;
					}


					this.PageCount = (int)Math.Ceiling((double)count / (double)this.PageSize);

					int currentPage = this.CurrentPage;

					// The following performs paging.
					if (this._cachedFilterSortedList != null)
					{
						sortedQuery = this._cachedFilterSortedList.Skip<T>(this.PageSize * (currentPage - 1)).Take(this.PageSize).AsQueryable<T>();
					}
					else
					{
						if (this._needGroupBy)
						{
							List<GroupByDataContext> groups = source as List<GroupByDataContext>;
							if (groups != null)
							{
								this.PageCount = (int)Math.Ceiling((double)source.Count / (double)this.PageSize);

								currentPage = this.CurrentPage;

								groupQuery = groups.AsQueryable<GroupByDataContext>();
								groupQuery = groupQuery.Skip(this.PageSize * (currentPage - 1)).Take(this.PageSize);
							}
						}
                        else if (this._needMerge)
                        {
                            // Apply paging on top of the merged Data.
                            List<MergedRowInfo> mergeData = source as List<MergedRowInfo>;
                            if (mergeData != null)
                            {
                                this.PageCount = (int)Math.Ceiling((double)source.Count / (double)this.PageSize);

                                currentPage = this.CurrentPage;

                                IQueryable<MergedRowInfo> mergeQuery = mergeData.AsQueryable<MergedRowInfo>();
                                mergeQuery = mergeQuery.Skip(this.PageSize * (currentPage - 1)).Take(this.PageSize);

                                source = mergeQuery.ToList();   //  <-- Perf Hit (This is where the query will actually be performed on the datasource.)
                            } 
                        }
                        else
                            sortedQuery = sortedQuery.Skip(this.PageSize * (currentPage - 1)).Take(this.PageSize);
					}
				}

				// Lets apply the query
				if (groupQuery != null)
				{
					//this.ApplySummary(groupQuery, SummaryExecution.AfterFilteringAndPaging);
					source = groupQuery.ToList<GroupByDataContext>(); //  <-- Perf Hit (This is where the query will actually be performed on the datasource.)			
				}
				else if (sortedQuery != null)
				{
					this.ApplySummary(sortedQuery, SummaryExecution.AfterFilteringAndPaging);
					this.GatherDataForConditionalFormatting(sortedQuery, EvaluationStage.AfterFilteringAndPaging);
					source = sortedQuery.ToList<T>();	//  <-- Perf Hit (This is where the query will actually be performed on the datasource.)			
				}

                if (setSortedFilteredDataSource)
                {
                    this.SetSortedFilteredDataSource(source);
                    this.OnDataUpdated();
                }

				this._needSummary = this._needSort = this._needFiltering = this._needGroupBy = this._needPaging = this._needMerge = false;

                return true;
			}

            return false;   
		}

		#endregion // ApplyClientDataManipulations

        #region BuildMergedGroupings

        /// <summary>
        /// Goes through each grouping returned by an ICollectionView, and flattens it out for merged information.
        /// </summary>
        /// <param name="mris"></param>
        /// <param name="groups"></param>
        /// <param name="mcis"></param>
        /// <param name="mdc"></param>
        /// <param name="lastMciLookup"></param>
        /// <returns></returns>
        private List<object> BuildMergedGroupings(List<MergedRowInfo> mris, ReadOnlyObservableCollection<object> groups, List<MergedColumnInfo> mcis, MergedDataContext mdc, Dictionary<MergedColumnInfo, bool> lastMciLookup)
        {
            List<object> children = new List<object>();
            int index = 0;
            int count = groups.Count;
            foreach (object data in groups)
            {
                bool isLast = (index == count - 1);

                CollectionViewGroup group = data as CollectionViewGroup;
                if (group != null)
                {
                    MergedColumnInfo mci = new MergedColumnInfo() { Key = group.Name, Summaries = this.Summaries };
                    if (mcis.Count > 0)
                        mci.ParentMergedColumnInfo = mcis[mcis.Count - 1];
                    mci.DataType = this.CachedType;
                    List<MergedColumnInfo> childMcis = new List<MergedColumnInfo>(mcis);
                    childMcis.Add(mci);
                    mci.MergingObject = mdc.MergedObject;

                    MergedDataContext nextMDC = null;
                    int mdcIndex = this.MergeDataContexts.IndexOf(mdc);
                    if (mdcIndex != this.MergeDataContexts.Count - 1)
                        nextMDC = this.MergeDataContexts[mdcIndex + 1];                    

                    if (mci.ParentMergedColumnInfo != null)
                    {
                        if (lastMciLookup.ContainsKey(mci.ParentMergedColumnInfo))
                            lastMciLookup[mci.ParentMergedColumnInfo] = isLast;
                        else
                            lastMciLookup.Add(mci.ParentMergedColumnInfo, isLast);    
                    }

                    List<object> currentChildren = this.BuildMergedGroupings(mris, group.Items, childMcis, nextMDC, lastMciLookup);
                    children.AddRange(currentChildren);
                    mci.Children = currentChildren;
                }
                else
                {
                    MergedRowInfo mri = new MergedRowInfo() { Data = data };
                    foreach (MergedColumnInfo mci in mcis)
                    {
                        if (isLast)
                        {
                            if(lastMciLookup.ContainsKey(mci))
                                mri.IsLastRowInGroup.Add(mci.MergingObject, lastMciLookup[mci]);
                            else
                                mri.IsLastRowInGroup.Add(mci.MergingObject, true);
                        }
                        else
                            mri.IsLastRowInGroup.Add(mci.MergingObject, false);
                    }
                    mri.MergedGroups = mcis;
                    mris.Add(mri);
                    children.Add(data);
                }
                index++;
            }
            return children;
        }

        #endregion // BuildMergedGroupings

        #endregion //Private

        #endregion // Methods
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