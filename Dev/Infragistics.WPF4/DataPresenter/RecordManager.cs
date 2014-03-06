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
//using System.Windows.Events;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Data;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Controls;
using Infragistics.Shared;
using Infragistics.Windows.Internal;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
	/// Used by <see cref="DataPresenterBase"/> to manage the root record collection and by <see cref="ExpandableFieldRecord"/> to manage its child records.
    /// </summary>
	/// <remarks>
	/// <para class="body"><see cref="DataRecord"/>s are created to represent each item in the <see cref="DataPresenterBase.DataSource"/>. The <see cref="DataRecord"/> exposes a read-only <see cref="DataRecord.DataItem"/> property that returns the associated item from the data source as well as a corresponding <see cref="DataRecord.DataItemIndex"/> property.
	/// These <see cref="DataRecord"/>s are managed by the <see cref="RecordManager"/> and exposed via its <see cref="Infragistics.Windows.DataPresenter.RecordManager.Unsorted"/>, <see cref="Infragistics.Windows.DataPresenter.RecordManager.Sorted"/> and <see cref="Infragistics.Windows.DataPresenter.RecordManager.Groups"/> collection properties.</para>  
	/// <para></para>
	/// <para class="body">The RecordManager is responsible for listening to events raised by the <see cref="DataPresenterBase.DataSource"/> thru either the <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> or the <see cref="System.ComponentModel.IBindingList"/> interfaces. 
	/// Based on these notifcations it keeps its <see cref="Sorted"/> and <see cref="Unsorted"/> collections of <see cref="DataRecord"/>s in sync and raises corresponding events on each thru their <see cref="System.Collections.Specialized.INotifyCollectionChanged"/> interface implemenations.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="DataPresenterBase.RecordManager"/>
	/// <seealso cref="ExpandableFieldRecord.ChildRecordManager"/>
	/// <seealso cref="RecordCollectionBase.ParentRecordManager"/>
	public class RecordManager : PropertyChangeNotifier, IWeakEventListener
    {
        #region Private Members

        private DataPresenterBase       _dataPresenter;
        private ExpandableFieldRecord _parentRecord;
        private DataRecordCollection _unsortedRecords;
        private DataRecordCollection _sortedRecords;
        private GroupByRecordCollection
                                    _groups;
        private IEnumerable         _sourceItems;
        private IEnumerable         _underlyingSourceItems;
		private IBindingList		_bindingList;
		private IList				_list;
		private ICollectionView		_collectionView;
        private FieldLayout         _fieldLayout;
        private Field               _field;
		private DispatcherOperation _resetOperationPending;
		
		// JJD 10/18/10 - TFS30715
		private DispatcherOperation _trackSortGroupOperationPending;
		private DispatcherOperation _cleanupTrackersOperationPending;
        
		private bool                _isSorted;
        private bool                _hasSourceBeenSetAtLeastOnce;
        private bool                _hasMultipleFieldLayouts;
        private bool                _unsortedRecordsDirty;
 		private bool				_firstResetProcessed;
		
		// JJD 6/28/11 - TFS79556 -added
		private bool				_isInitializingDataSource;

        // JJD 5/22/09 - TFS17691
        // Changed from bool to the object that was wired
		//private bool				_notifyCollectionChangedHooked;
		private INotifyCollectionChanged
                                    _notifyCollectionChangedHooked;

		private bool				_inAddNew;
        private bool                _isInReset; // JJD 5/23/08 - BR33317
		private DataRecord			_recordBeingCommitted;
//		private List<DataRecord>	_recordsInsertedAtTop;

        
        
        
        
		
        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping
        // keep track of any field layout sort and groupbyVersions so we can
        // be notified when sort/group criteria change
        private WeakDictionary<FieldLayout, SortGroupVersionTracker> 
									// JJD 08/17/12 - TFS119037
									// Pass false in as the 1st param so we don't root the fieldlayouts by using a strong reference
									//_sortGroupByVersionTrackers = new WeakDictionary<FieldLayout,SortGroupVersionTracker>(false, true);
                                    _sortGroupByVersionTrackers = new WeakDictionary<FieldLayout,SortGroupVersionTracker>(true, true);

        private int                 _underlyingDataVersion;
		private DataRecord			_currentAddRecord;
		private DataRecord			_recordBeingDeleted;
		private HybridDictionary	_addRecordCellDataDictionary;
		private bool				_isGroupBySupported;

        // JJD 11/17/08 - TFS6743/BR35763 - added
        // Since IEditableCollectionView wasn't implemented until v3.5 sp1 of the .Net Framework
        // and we are targeting the 3.0 framework we created a proxy class that calls the
        // interface's methods using reflection
        private EditableCollectionViewProxy 
                                    _editableCollectionView;
        
        // JJD 11/17/08 - TFS6743/BR35763 - added 
        // Wrapper implements IList interface, enough methods for our purposes
        private CollectionViewWrapper _collectionViewWrapper;

		// JJD 5/9/07 - BR22698
		// Added support for listening to data item's PropertyChanged event
		// if the datasource doesn't supply cell value change notifications thru IBindlingList
		private bool				_dataSourceRaisesCellValueChangeEvents;

        // JJD 2/6/09 - TFS13615
        // Cache a flag so we know if we can get property descriptors from the datasource
        // even if there are no items in the list
        private bool                _canGetPropertiesFromDataSourceWithoutItems;

        // JJD 12/08/08 - Added IsSynchronizedWithCurrentItem property
        private bool                _isSynchronizingWithCurrentItem;

		// JJD 11/18/11 - TFS79001
		// Keep track of item type from the list if it has a default constructor 
		private Type				_typeWithPublicCtor;
		// JJD 05/08/12 - TFS110865
		// Keep a flag that can be used to determine if the items in the list are known types
		private bool				_typeIsKnownType;

		// JJD 11/24/10 - TFS59031
		// Cache the provider for use when there aren't any rcds
		private FieldLayout.LayoutPropertyDescriptorProvider _typedListDefaultProvider;

		// JJD 7/9/07
		// Added support for handling change notifications on another thread 
		private ArrayList			_asyncChangeList;
		private DispatcherOperation _asyncChangePending;
		// SSP 12/21/11 TFS73767 - Optimizations
		// 
		private List<ChangeNotification> _processAsyncChangeList;
		private HashSet _processAsyncChangeListSet;



        // JJD 4/3/08 - added support for printing
        // Lazily maintain a map between print records and their associated records
        // in the source ui datapresenter
        private Dictionary<Record, Record> _associatedRecordMap;
        private RecordManager _associatedRecordManager;
        private int _reportVersion;


		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		private RecordFilterCollection _recordFilters;
		private ResolvedRecordFilterCollection _recordFiltersResolved;
		private FilteredDataItemsCollection _filteredInDataItems;

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// 
		internal BeginUpdateInformation _beginUpdateInfo;
		
		
		
		private int _scrollCountVersion;

		// AS 5/7/09 NA 2009.2 ClipboardSupport
		private int _newRecordOnTopOffset = 0;

		// AS 7/21/09 NA 2009.2 Field Sizing
		private int _nestingDepth;

		// AS 8/7/09 NA 2009.2 Field Sizing
		private int _fieldAutoSizeVersion = 1;

		// SSP 2/14/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private SortedFieldsToCollectionViewSynchronizer _sortedFieldsToCollectionViewSynchronizer;

		// JJD 07/27/12 - TFS117958 
		// Added anti-recursion flag 
		private bool _inProcessQueuedChangs;

        #endregion //Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal RecordManager(DataPresenterBase dataPresenter, ExpandableFieldRecord parentRecord, Field field)
        {
            this._dataPresenter = dataPresenter;
            this._field = field;

			if (parentRecord != null)
			{
				// JJD 1/29/07 
				// This should not be set to the fieldlayout of the parent record
				//this._fieldLayout   = parentRecord.FieldLayout;
				this._parentRecord = parentRecord;

				// AS 7/1/09 NA 2009.2 Field Sizing
				_nestingDepth = parentRecord.RecordManagerNestingDepth + 1;
			}

            this._sortedRecords = new DataRecordCollection(this._parentRecord, this, this._fieldLayout, false);
            this._unsortedRecords = new DataRecordCollection(this._parentRecord, this, this._fieldLayout, true);

            // JJD 7/9/07
            // Added support for handling change notifications on another thread 
            this._asyncChangeList = new ArrayList();

            if (field == null || field.IsExpandableByDefault)
                this.PostDelayedReset();
        }

        #endregion //Constructors

        #region Properties

            #region Public Properties

                #region Current

        /// <summary>
        /// Returns the currently displayed collection of records.
        /// </summary>
		/// <remarks>
		/// <para class="body">If the <see cref="DataRecord"/>s are grouped then the <see cref="HasGroups"/> property will return true. 
		/// In that case this property will return the <see cref="Groups"/> collection. Otherwise it will return the <see cref="Sorted"/> collection.</para>
		/// </remarks>
		/// <seealso cref="Sorted"/>
		/// <seealso cref="Groups"/>
		/// <seealso cref="HasGroups"/>
		/// <seealso cref="DataPresenterBase.Records"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public RecordCollectionBase Current
        {
            get
            {
                if (this.HasGroups)
                    return this.Groups;

                return this.Sorted;
            }
        }

                #endregion //Current

                #region CurrentAddRecord

		/// <summary>
		/// Returns the current add record for this manager
		/// </summary>
		/// <remarks>
		/// <para class="note">This <see cref="DataRecord"/> will not be included in the <see cref="Sorted"/> or <see cref="Unsorted"/> collections. 
		/// Instead it will be included within either the <see cref="DataPresenterBase.ViewableRecords"/> collection or a <see cref="Record.ViewableChildRecords"/> collection.</para>
		/// </remarks>
		/// <seealso cref="ViewableRecordCollection"/>
		/// <seealso cref="DataPresenterBase.ViewableRecords"/>
		/// <seealso cref="Record.ViewableChildRecords"/>
		[Browsable( false )]
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
		public DataRecord CurrentAddRecord
		{
			get
			{
                AddNewRecordLocation location = this.AddNewRecordLocation;
                //Debug.Assert(this._fieldLayout != null);

                // JJD 12/1/08 
                // if the location is default then there is no add record 
                //if ( this._fieldLayout == null )
                //if (this._fieldLayout == null ||
                if (location == AddNewRecordLocation.Default)
                {
                    //this._currentAddRecord = null;
                    return null;
                }
                else
                {
                    // lazily create an add record
                    // JJD 10/17/08
                    // We should only create an add record if we have a datasource
                    //if ( this._currentAddRecord == null )
					if ( this._currentAddRecord == null && this.DataSource != null)
						this._currentAddRecord = DataRecord.Create( this._sortedRecords, null, true, -1 );
				}

				return this._currentAddRecord;
			}
		}

		
		
#region Infragistics Source Cleanup (Region)































































#endregion // Infragistics Source Cleanup (Region)


                #endregion //CurrentAddRecord

                #region DataPresenter

        /// <summary>
        /// Returns the owning <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> object (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataPresenterBase DataPresenter { get { return this._dataPresenter; } }

                #endregion //DataPresenter

				#region DataSourceAllowsAddNew

		/// <summary>
		/// Returns true if the data source supports adding records (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="body">Adding records is only supported for <see cref="DataPresenterBase.DataSource"/>s that implement the <see cref="System.ComponentModel.IBindingList"/> interface and return true from its <see cref="System.ComponentModel.IBindingList.AllowNew"/> property.</para>
		/// <para></para>
		/// <para class="body">Adding new records is disabled by default and is not supported by all views, e.g. <see cref="CarouselView"/> does not support adding records directly so its <see cref="CarouselView.IsAddNewRecordSupported"/> returns false.</para> 
		/// <para></para>
		/// <para class="body">To enable adding records set the <see cref="FieldLayoutSettings"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowAddNew"/> property to true.</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>The location of the add record is specified via the <see cref="FieldLayoutSettings"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AddNewRecordLocation"/> property.</para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordManager"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.InitializeTemplateAddRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdding"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordAdded"/>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowAddNew"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AddNewRecordLocation"/>
		/// <seealso cref="DataPresenterBase.InitializeTemplateAddRecord"/>
		/// <seealso cref="DataSourceAllowsDelete"/>
		/// <seealso cref="DataSourceAllowsEdit"/>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool DataSourceAllowsAddNew
		{
			get
			{
				// JJD 05/08/12 - TFS110865
				// Moved logic to AllowsAddNewCore and added DoesTypeSupportEditInList method
				// which will return false for known types if the underlying data source 
				// doesn't support IList or its IList.IsReadOnly returns true;
				return this.AllowsAddNewCore && this.DoesTypeSupportEditInList();
			}
		}

				#endregion //DataSourceAllowsAddNew	

				#region DataSourceAllowsDelete

		/// <summary>
		/// Returns true if the data source supports deleting records (read-only)
		/// </summary>
		/// <remarks>
		/// <para class="body">Deleting <see cref="DataRecord"/>s is supported for <see cref="DataPresenterBase.DataSource"/>s that implement the <see cref="System.ComponentModel.IBindingList"/> interface and return true from its <see cref="System.ComponentModel.IBindingList.AllowRemove"/> property.  
		/// It is also supported for <see cref="DataPresenterBase.DataSource"/>s that only implement the <see cref="System.Collections.IList"/> interface and return false from both its <see cref="System.Collections.IList.IsReadOnly"/> and <see cref="System.Collections.IList.IsFixedSize"/> properties.</para>
		/// <para></para>
		/// <para class="body">Assuming deleting of <see cref="DataRecord"/>s is enabled then when the user selects one or more <see cref="DataRecord"/>s and presses the <b>Delete</b> key the <see cref="DataPresenterBase.RecordsDeleting"/> and <see cref="DataPresenterBase.RecordsDeleted"/> events will be raised. 
		/// <see cref="DataRecord"/>s can also be deleted programmatically by first selecting them (via the <see cref="Record.IsSelected"/> property or the <see cref="DataPresenterBase.SelectedItems"/> collection) and then calling <see cref="DataPresenterBase.ExecuteCommand(RoutedCommand)"/> with the <see cref="DataPresenterCommands.DeleteSelectedDataRecords"/> command.</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>Deleting records is enabled by default. To disable deleting records set the <see cref="FieldLayoutSettings"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowDelete"/> property to false.</para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordManager"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleting"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.RecordsDeleted"/>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayoutSettings"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.AllowDelete"/>
		/// <seealso cref="DataSourceAllowsAddNew"/>
		/// <seealso cref="DataSourceAllowsEdit"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		public bool DataSourceAllowsDelete
		{
			get
			{
                // JJD 11/17/08 - TFS6743/BR35763 
                // If we have an IEditableCollectionView implementation
                // then return its CanRemove
                if (this._editableCollectionView != null)
                    return this._editableCollectionView.CanRemove;

                IBindingList bindingList = this.BindingList;

				if (bindingList != null)
					return bindingList.AllowRemove;

				if (this._list != null)
					return	this._list.IsReadOnly == false && 
							this._list.IsFixedSize == false;

				return false;
			}
		}

				#endregion //DataSourceAllowsDelete	

				#region DataSourceAllowsEdit

		/// <summary>
		/// Returns true if the data source supports editing cells (read-only)
		/// </summary>
		/// <seealso cref="DataSourceAllowsAddNew"/>
		/// <seealso cref="DataSourceAllowsDelete"/>
		[DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden)]
		public bool DataSourceAllowsEdit
		{
			get
			{
                
                IBindingList bindingList = this.BindingList;

				if (bindingList != null)
					return bindingList.AllowEdit;

				// JJD 3/13/07 - BR21058
				// We can't use IsReadOnly on the list to determine allow edit we have to assume editing is possible 
				//IList list = this._sourceItems as IList;

				//if (list != null)
				//    return list.IsReadOnly == false;

				// JJD 05/08/12 - TFS110865
				// Added DoesTypeSupportEditInList method which will return false for known types if the 
				// underlying data source doesn't support IList or its IList.IsReadOnly returns true;
				//return true;
				return this.DoesTypeSupportEditInList();
			}
		}

				#endregion //DataSourceAllowsEdit	

				#region FilteredInDataItems

		// SSP 1/5/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns collection of data items from the data list that match the current filter criteria
		/// in the DataPresenter.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Record filtering functionality allows the user to filter data by specifying 
		/// filter criteria. You can use <b>FilteredInDataItems</b> property to get 
		/// data items from the data list that match the filter criteria. More precisely,
		/// the returned collection is a collection of data items associated with filtered
		/// data records.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="GetFilteredOutDataRecords"/>
		public ICollectionView FilteredInDataItems
		{
			get
			{
				if ( null == _filteredInDataItems )
					_filteredInDataItems = new FilteredDataItemsCollection( this );

				return _filteredInDataItems;
			}
		}

		internal FilteredDataItemsCollection FilteredInDataItemsIfAllocated
		{
			get
			{
				return _filteredInDataItems;
			}
		}

				#endregion // FilteredInDataItems

                #region Groups

        /// <summary>
        /// Returns a read-only collection of <see cref="GroupByRecord"/>s.
        /// </summary>
		/// <value>A <see cref="GroupByRecordCollection"/> or null if <see cref="HasGroups"/> is false.</value>
		/// <remarks>The order is determined by the <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/> collection. Note: <see cref="FieldSortDescription"/>s in this collection are ordered so that the ones with <see cref="FieldSortDescription.IsGroupBy"/> values set to true will be first.</remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
        /// <seealso cref="GroupByRecord"/>
        /// <seealso cref="Sorted"/>
        /// <seealso cref="Unsorted"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowGroupBy"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByComparer"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByEvaluator"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByMode"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByRecordPresenterStyle"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.GroupByRecordPresenterStyleSelector"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.Field.IsGroupBy"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.FieldSortDescription.IsGroupBy"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GroupByRecordCollection Groups
        {
            get
            {
                // JJD 1/10/08 - BR29572
                // Make sure that our DataSource is initialized
                this.VerifyRootDataSource();

                if (this._groups == null)
					this._groups = new GroupByRecordCollection(this.ParentRecord, this, this._fieldLayout);
				else
				{
					if (this._fieldLayout != null &&
						this._fieldLayout != this._groups.FieldLayout)
						this._groups.InitializeFieldLayout(this._fieldLayout);
				}

                return this._groups;
            }
        }

                #endregion //Groups

                #region HasGroups

        /// <summary>
        /// Returns true if the records are grouped (read-only)
        /// </summary>
		/// <remarks>
		/// <para class="body">If the <see cref="DataRecord"/>s are grouped then the <see cref="HasGroups"/> property will return true.</para> 
		/// </remarks>
		/// <seealso cref="Groups"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// <seealso cref="FieldSortDescription"/>
		[Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasGroups
        {
            get
            {
                return this._groups != null && this._groups.Count > 0;
            }
        }

                #endregion //HasGroups	

                #region Field

        /// <summary>
        /// Returns the parent field if any (read-only).
        /// </summary>
        /// <remarks>Only has meaning for child records.</remarks>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Field Field { get { return this._field; } }

                #endregion //Field	

                #region HasMultipleFieldLayouts

        /// <summary>
        /// Returns true if the root level records are associated with more than 1 <see cref="FieldLayout"/> (read-only)
        /// </summary>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.FieldLayouts"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HasMultipleFieldLayouts { get { return this._hasMultipleFieldLayouts; } }

                #endregion //HasMultipleFieldLayouts	
    
                #region IsSorted

        /// <summary>
        /// Returns true if the records have been sorted by the data presenter.
        /// </summary>
        /// <seealso cref="Sorted"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSorted { get { return this._isSorted; } }

                #endregion //IsSorted	
    
                #region ParentRecord

        /// <summary>
        /// Returns the parent <see cref="ExpandableFieldRecord"/> or null (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ExpandableFieldRecord ParentRecord { get { return this._parentRecord; } }

                #endregion //ParentRecord

                #region ParentDataRecord

        /// <summary>
        /// Returns the <see cref="DataRecord"/> that is the parent of the <see cref="ParentRecord"/> or null (read-only)
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRecord ParentDataRecord 
        { 
            get 
            {
                if (this._parentRecord != null)
                    return this._parentRecord.ParentDataRecord;

                return null; 
            } 
        }

                #endregion //ParentDataRecord

				#region RecordFilters

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 

		/// <summary>
		/// Specifies the filter criteria with which to filter recods.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RecordFilters</b> property gets the record filters collection for this record manager. You can use this 
		/// collection to specify filter criteria to filter data records. These filters will be applied to all data 
		/// records associated with this record manager. 
		/// Note that if the <see cref="FieldLayoutSettings.RecordFilterScope"/> is set to AllRecords then the
		/// FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/> 
		/// will be used. For the root field layout, that's always the case. However for child field layouts,
		/// RecordFilterScope property determines whether the FieldLayout's RecordFilters get used or the
		/// RecordManager's RecordFilters get used. Since the root RecordManager's RecordFilterCollection never gets
		/// used, modifying it will raise an exception.
		/// </para>
		/// <para class="body">
		/// Also note that you can enable record filtering user interface by setting 
		/// <see cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// and <see cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.FilterUIType"/> properties.
		/// When the user modifies filter criteria, this collection will be updated to reflect the new criteria.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayoutSettings.RecordFilterScope"/>
		/// <seealso cref="RecordManager.RecordFilters"/>
		//[Description( "Specifies the filter criteria with which to filter recods." )]
		//[Category( "Data" )]
		[Bindable( true )]



		public RecordFilterCollection RecordFilters

		{
			get
			{
				if ( null == _recordFilters )
				{
					//Debug.Assert( !this.IsRootManager, "RecordFilters of the root record manager should never get used. See RecordFilterCollection.VerifyNotRootRecordManager method." );

					_recordFilters = new RecordFilterCollection( this );
				}

				return _recordFilters;
			}
		}

		internal RecordFilterCollection RecordFiltersIfAllocated
		{
			get
			{
				return _recordFilters;
			}
		}

		/// <summary>
		/// Returns true if the RecordFilters property is set to a non-default value.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public bool ShouldSerializeRecordFilters( )
		{
			return null != _recordFilters && _recordFilters.ShouldSerialize( );
		}

		/// <summary>
		/// Resets the RecordFilters property to its default state.
		/// </summary>
		[EditorBrowsable( EditorBrowsableState.Never )]
		public void ResetRecordFilters( )
		{
			if ( null != _recordFilters )
				_recordFilters.Clear( );
		}

				#endregion // RecordFilters

                #region Sorted

        /// <summary>
        /// Returns a read-only collection of <see cref="DataRecord"/>s in their sorted order.
        /// </summary>
        /// <remarks>
		/// <para class="note"><b>Note: </b>If the records aren't sorted (i.e. the <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/> collection is empty) this collection will contain the <see cref="DataRecord"/>s in the same order as the <see cref="Unsorted"/> collection. 
		/// Otherwise the order is determined by the <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/> collection.
		/// </para>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DefaultFieldLayout"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
        /// <seealso cref="DataRecord"/>
        /// <seealso cref="DataRecord.DataItem"/>
        /// <seealso cref="DataRecord.DataItemIndex"/>
        /// <seealso cref="Unsorted"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataRecordCollection Sorted
        {
            get
            {
                // JJD 1/10/08 - BR29572
                // Make sure that our DataSource is initialized
                this.VerifyRootDataSource();

                return this._sortedRecords;
            }
        }

                #endregion //Sorted

				// AS 5/13/09 NA 2009.2 Undo/Redo
				#region SourceItems
		/// <summary>
		/// Returns the underlying collection that provides the items for the RecordManager. 
		/// </summary>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_ClipboardSupport)]
		public IEnumerable SourceItems
		{
			get { return _sourceItems; }
		} 
				#endregion //SourceItems

                #region Unsorted

        /// <summary>
        /// Returns a read only collection of <see cref="DataRecord"/>s in their original unsorted order.
        /// </summary>
        /// <remarks>
		/// <para class="body">The order of the <see cref="DataRecord"/>s in this collection matches the order of their associated items in the <see cref="DataPresenterBase"/>'s <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>.
		/// </para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>If the records aren't sorted (i.e. the <see cref="Infragistics.Windows.DataPresenter.FieldLayout"/>'s <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/> collection is empty) the <see cref="Sorted"/> collection will contain the <see cref="DataRecord"/>s in the same order as this collection.</para>
		/// </remarks>
        /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.DataSource"/>
        /// <seealso cref="DataRecord"/>
        /// <seealso cref="DataRecord.DataItem"/>
        /// <seealso cref="DataRecord.DataItemIndex"/>
        /// <seealso cref="Sorted"/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public IList<DataRecord> Unsorted
        {
            get
            {
				
				
				
				
				return this.UnsortedInternalVerify;
            }
        }

                #endregion //Unsorted	

            #endregion //Public Properties

            #region Internal Properties

                // JJD 4/3/08 - added support for printing
                #region AssociatedRecordManager


        // Lazily maintain a map between print records and their associated records
        // in the source ui datapresenter
        internal RecordManager AssociatedRecordManager
        {
            get
            {
                // MBS 7/28/09 - NA9.2 Excel Exporting
                //DataPresenterReportControl dprc = this._dataPresenter as DataPresenterReportControl;
                DataPresenterExportControlBase dprc = this._dataPresenter as DataPresenterExportControlBase;

                Debug.Assert(dprc != null);

                if (dprc == null)
                    return null;

                if (dprc.ReportVersion != this._reportVersion)
                {
                    this._reportVersion = dprc.ReportVersion;
                    this._associatedRecordManager = null;
                }

                if ( this._associatedRecordManager == null )
                {
                    if (this._parentRecord == null)
                        this._associatedRecordManager = dprc.SourceDataPresenter.RecordManager;
                    else
                    {
                        ExpandableFieldRecord associatedParentRecord = this._parentRecord.GetAssociatedRecord() as ExpandableFieldRecord;

                        Debug.Assert(associatedParentRecord != null);

                        if (associatedParentRecord == null)
                            return null;

						// JJD 09/22/11  - TFS84708 - Optimization
						// Use the ChildRecordManagerIfNeeded instead which won't create
						// child rcd managers for leaf records
						//this._associatedRecordManager = associatedParentRecord.ChildRecordManager;
 						this._associatedRecordManager = associatedParentRecord.ChildRecordManagerIfNeeded;
                   }
                }

                return this._associatedRecordManager;
            }
        }

                #endregion //AssociatedRecordManager

				// AS 8/7/09 NA 2009.2 Field Sizing
				#region FieldAutoSizeVersion
		internal int FieldAutoSizeVersion
		{
			get { return _fieldAutoSizeVersion; }
			set { _fieldAutoSizeVersion = value; }
		} 
				#endregion //FieldAutoSizeVersion

				#region BeginUpdateInfo

		// SSP 2/11/09 TFS12467
		// 
		internal BeginUpdateInformation BeginUpdateInfo
		{
			get
			{
				return _beginUpdateInfo;
			}
		}

				#endregion // BeginUpdateInfo

                #region BindingList

        internal IBindingList BindingList
		{
			get
			{
				return this._bindingList;
			}
		}

				#endregion //BindingList	

                // JJD 2/6/09 - TFS13615
                #region CanGetPropertiesFromDataSourceWithoutItems

        // Cache a flag so we know if we can get property descriptors from the datasource
        // even if there are no items in the list
        internal bool CanGetPropertiesFromDataSourceWithoutItems { get { return this._canGetPropertiesFromDataSourceWithoutItems; } }

                #endregion //CanGetPropertiesFromDataSourceWithoutItems	
    
                #region CurrentAddRecordInternal

		internal DataRecord CurrentAddRecordInternal
		{
			get
			{
				return this._currentAddRecord;
			}
		}

				#endregion //CurrentAddRecordInternal

                // JJD 3/11/10 - TFS28705 
				#region CurrentInternal

		/// <summary>
        /// Returns the _groups or _sortedRecords member var.
		/// </summary>
		internal RecordCollectionBase CurrentInternal
		{
			get
			{
                if ( this.HasGroups )
				    return _groups;

                return this._sortedRecords;
			}
		}

				#endregion // CurrentInternal

                #region DataSource

        internal IEnumerable DataSource
        {
            get
            {
                return this._sourceItems;
            }
            set
            {
				// JJD 5/8/07
				// Compare against the underlying source instead
//				if ( value != this._sourceItems )
				// JJD 10/19/07 - BR26277
				// Moved logic to DataBindingUtilities.GetUnderlyingItemSource.
				//if ( this.GetUnderlyingSource( value ) != this._underlyingSourceItems )
				//    this.SetDataSource(value, this._hasSourceBeenSetAtLeastOnce == false);
				// SSP 3/31/10 TFS24458
				// We also need to check if the source is different. For example if it's a different 
				// instance of collection view that points to the same underlying list however has 
				// a different filter then we also need to re-load the records.
				// 
				//if (DataBindingUtilities.GetUnderlyingItemSource(value) != this._underlyingSourceItems)
				if ( value != _sourceItems || DataBindingUtilities.GetUnderlyingItemSource( value ) != this._underlyingSourceItems )
					this.SetDataSource(value, this._hasSourceBeenSetAtLeastOnce == false);
            }
        }

        private void SetDataSource(IEnumerable currentValue, bool postReset)
        {
            // JJD 9/21/09 - TFS20737
            // Hold the old source so we can compare it below
            IEnumerable oldSource = this._sourceItems;

			// JJD 6/28/11 - TFS79556 
			// If the DataSource is being initialized then set a flag so we can bypass some notifications
			// when initializing the RecordManager on any RecordManager but the root.
			if (oldSource == null && currentValue != null && _parentRecord != null)
				this._isInitializingDataSource = true;

            // unhook from the old source's events
			if (this._collectionView != null && this._parentRecord == null)
			{
                
                
                
                
                this.UnwireCollectionViewEvents();
			}

            
#region Infragistics Source Cleanup (Region)























#endregion // Infragistics Source Cleanup (Region)


            // JJD 11/17/08 - TFS6743/BR35763 
            // moved logic to ClearAddRecordOnDataSourceChange
            this.ClearAddRecordOnDataSourceChange();

			this._firstResetProcessed = false;

			// set the source to the new value
            this._sourceItems = currentValue;

            // JJD 7/30/08 - BR34858
            // If the source is an IListSource then call the GetList method to get at the real list.
            // This fixes a weird error that was caused by the way a Linq EntitySet<> is implemented.
            // For some reason they implement IlistSource and IList for the underlying items.
            // Not sure why this was done.
            if (this._sourceItems is IListSource)
                this._sourceItems = ((IListSource)this._sourceItems).GetList();
			
			// JJD 5/8/07
			// Use GetUnderlyingSource method 
			// JJD 10/19/07 - BR26277
			// Moved logic to DataBindingUtilities.GetUnderlyingItemSource.
			//this._underlyingSourceItems = this.GetUnderlyingSource(currentValue);
			this._underlyingSourceItems = DataBindingUtilities.GetUnderlyingItemSource(currentValue);

			this._hasSourceBeenSetAtLeastOnce = true;

			// set the collectionView
            // JJD 11/24/09 - TFS25123
            // If the data source implements ICollectionViewFactory then call 
            // CollectionViewSource.GetDefaultView
            if (currentValue is ICollectionViewFactory)
                this._collectionView = CollectionViewSource.GetDefaultView(currentValue);
            else
			    this._collectionView = currentValue as ICollectionView;

			if (this._collectionView != null && this._parentRecord == null)
			{
				// JJD 5/8/07 initialized above
				//this._underlyingSourceItems = this._collectionView.SourceCollection;
                
                
                
				
                this.WireCollectionViewEvents();
			}

			// JJD 5/8/07 initialized above
			//else
			//{
			//    this._underlyingSourceItems = this._sourceItems;
			//}

            // JJD 11/17/08 - TFS6743/BR35763 
            // Moved logic to VerifyListSourceToUse
            this.VerifyListSourceToUse();

			// SSP 10/12/08 TFS8826
			// We have to recreate special records list because visibility of certain
			// records may rely on the data source capabilities, like add-row.
			// 
			if ( null != this.ViewableRecords )
				this.ViewableRecords.DirtySpecialRecords( false );

			if (postReset)
				this.PostDelayedReset();
			else
				this.OnSourceCollectionReset();

            // JJD 9/21/09 - TFS20737
            // Reset the overall scroll position only if the old source has changed
			if (oldSource != null &&
				oldSource != this._sourceItems)
			{
				// JJD 7/15/10 - TFS35815
				// Don't reset the overall scroll position unless this is the root record collection
				if (this._parentRecord == null)
					((IViewPanelInfo)this._dataPresenter).OverallScrollPosition = 0;
			}

		}

                #endregion //DataSource
		
				// JJD 5/9/07 - BR22698
				// Added support for listening to data item's PropertyChanged event
				// if the datasource doesn't supply cell value change notifications thru IBindlingList
				#region DataSourceRaisesCellValueChangeEvents

		internal bool DataSourceRaisesCellValueChangeEvents { get { return this._dataSourceRaisesCellValueChangeEvents; } }

				#endregion //DataSourceRaisesCellValueChangeEvents	
        
                // JJD 11/17/08 - TFS6743/BR35763 - added
                #region EditableCollectionView

        // Since IEditableCollectionView wasn't implemented until v3.5 sp1 of the .Net Framework
        // and we are targeting the 3.0 framework we created a proxy class that calls the
        // interface's methods using reflection
        internal EditableCollectionViewProxy EditableCollectionView
        {
            get { return this._editableCollectionView; }
        }

                #endregion //EditableCollectionView	
        
				#region FieldLayout

		internal FieldLayout FieldLayout
		{
			get
			{
				return this._fieldLayout;
			}
		}

				#endregion //FieldLayout	
            
				#region FilterEvaluationModeResolved

		// SSP 2/14/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal FilterEvaluationMode FilterEvaluationModeResolved
		{
			get
			{
				return null != _fieldLayout ? _fieldLayout.FilterEvaluationModeResolved : FilterEvaluationMode.Auto;
			}
		}

				#endregion // FilterEvaluationModeResolved

                // JJD 11/17/08 - TFS6743/BR35763 - added
                #region GetEditableCollectionViewProxy

		// JJD 09/22/11  - TFS84708 - Optimization 
		// Made internal static method so we could call it from the ExpandableFieldRecord.
		// Also added existingProxy parameter since this is now a static method
		//private EditableCollectionViewProxy GetEditableCollectionViewProxy(object source)
		internal static EditableCollectionViewProxy GetEditableCollectionViewProxy(object source, EditableCollectionViewProxy existingProxy)
        {
            CollectionView view = source as CollectionView;

            if (view == null)
                return null;

            // if the IEditableCollectionViewType is null that means we are using a version 
            // of the .Net framework prior to v3.5 sp1 so therefore we can't use the proxy
            // so return null
            if (EditableCollectionViewProxy.IEditableCollectionViewType == null)
                return null;

            // if the object doesn't implement the IEditableCollectionView interface then
            // return null
            if (!EditableCollectionViewProxy.IEditableCollectionViewType.IsAssignableFrom(source.GetType()))
                return null;

            // if the view is the same as the one we have cached then return that
			if (existingProxy != null &&
				 existingProxy.View == view)
				return existingProxy;

            return new EditableCollectionViewProxy(view);
        }

                #endregion //GetEditableCollectionViewProxy	

				#region GroupByEvaluationModeResolved

		// SSP 2/14/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal GroupByEvaluationMode GroupByEvaluationModeResolved
		{
			get
			{
				return null != _fieldLayout ? _fieldLayout.GroupByEvaluationModeResolvedDefault : GroupByEvaluationMode.Auto;
			}
		} 

				#endregion // GroupByEvaluationModeResolved

                // JJD 3/11/10 - TFS28705 
				#region GroupsInternal

		/// <summary>
        /// Returns the _groups member var.
		/// </summary>
		internal GroupByRecordCollection GroupsInternal
		{
			get
			{
				return _groups;
			}
		}

				#endregion // GroupsInternal
    
				#region IsAddRecordOnTop

		internal bool IsAddRecordOnTop
		{
			get
			{
				switch (this.AddNewRecordLocation)
				{
					case AddNewRecordLocation.OnTop:
					case AddNewRecordLocation.OnTopFixed:
						return true;
				}

				return false;
			}
		}

				#endregion //IsAddRecordOnTop	
     
                // JJD 12/21/09 - TFS25835 - added
                #region IsInActiveRecordChain

        internal bool IsInActiveRecordChain
        {
            get
            {
                Record activeRecord = this.DataPresenter.ActiveRecord;
                DataRecord dr;

                if (activeRecord == null)
                    dr = null;
                else
                {
                    dr = activeRecord.RecordType == RecordType.DataRecord
                        ? activeRecord as DataRecord
                        : activeRecord.ParentDataRecord;
                }

                // walk up the active DataRecord chain to see if any
                // of them are associated with this RecordManager
                while (dr != null)
                {
                    if (this == dr.RecordManager)
                        return true;

                    dr = dr.ParentDataRecord;
                }

                return false;
            }
        }

                #endregion //IsInActiveRecordChain	

                // JJD 5/23/08 - BR33317 - added 
                #region IsInReset

        internal bool IsInReset
        {
            get
            {
                return this._isInReset;
            }
        }

               #endregion //IsInReset	
    
				#region IsCurrentAddRecordAllocated
		internal bool IsCurrentAddRecordAllocated
		{
			get { return this._currentAddRecord != null; }
		} 
				#endregion //IsCurrentAddRecordAllocated
    
                // JJD 12/21/09 - TFS25835 - added
				#region IsCollectionView
        internal bool IsCollectionView
		{
			get { return this._collectionView != null; }
		} 
				#endregion //IsCollectionView

                // JJD 12/02/08 - TFS6743/BR35763 - added
                #region IsInVerfySort

        internal bool IsInVerifySort { get; private set; }

                #endregion //IsInVerfySort	

				// JJD 7/15/10 - TFS35815 - added
				#region IsObservable

		internal bool IsObservable
		{
			get
			{
				return this._notifyCollectionChangedHooked != null || this._bindingList != null;
			}
		}

				#endregion //IsObservable	
    
				#region IsRootManager

		
		
		/// <summary>
		/// Indicates whether this record manager is the root root record manager in the data grid.
		/// </summary>
		internal bool IsRootManager
		{
			get
			{
				return null == _parentRecord;
			}
		}

				#endregion // IsRootManager

				#region IsUpdating

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// 
		internal bool IsUpdating
		{
			get
			{
				return null != _beginUpdateInfo;
			}
		}
    
				#endregion // IsUpdating

                #region List

        internal IList List
        {
            get
            {
                return this._list;
            }
        }

                #endregion //List

				// AS 7/21/09 NA 2009.2 Field Sizing
				#region NestingDepth
		/// <summary>
		/// Returns the depth (i.e. # of parent record managers above this recordmanager).
		/// </summary>
		internal int NestingDepth
		{
			get
			{
				Debug.Assert(_nestingDepth >= 0);
				return _nestingDepth;
			}
		} 
				#endregion //NestingDepth

				#region RecordFiltersResolved

		// SSP 12/11/08 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns a collection that returns resolved RecordFilter instance to use for a particular field.
		/// This is needed because a record manager could have records from different field layouts and thus
		/// the RecordFilter for a particular field may be in the FieldLayout's RecordFilters collection or
		/// the RecordManager's RecordFilters collection.
		/// </summary>
		internal ResolvedRecordFilterCollection RecordFiltersResolved
		{
			get
			{
				if ( null == _recordFiltersResolved )
					_recordFiltersResolved = new ResolvedRecordFilterCollection( this );

				return _recordFiltersResolved;
			}
		}

		internal ResolvedRecordFilterCollection RecordFiltersResolvedIfAllocated
		{
			get
			{
				return _recordFiltersResolved;
			}
		}

				#endregion // RecordFiltersResolved

				#region ScrollCountVersion

		// SSP 8/4/09 - NAS9.2 Enhanced grid view
		// Added _scrollCountRecalcVersion on the data presenter. Add that to the record manager's
		// scroll count version.
		// 
		internal int ScrollCountVersion
		{
			get
			{
				DataPresenterBase dp = this.DataPresenter;
				return _scrollCountVersion + ( null != dp ? dp._cachedScrollCountRecalcVersion : 0 );
			}
		}

				#endregion // ScrollCountVersion

				#region SortedInternal

		
		
		/// <summary>
		/// Returns the _sortedRecords member var.
		/// </summary>
		internal DataRecordCollection SortedInternal
		{
			get
			{
				return _sortedRecords;
			}
		}

				#endregion // SortedInternal

				#region SortEvaluationModeResolved

		// SSP 2/14/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal SortEvaluationMode SortEvaluationModeResolved
		{
			get
			{
				return null != _fieldLayout ? _fieldLayout.SortEvaluationModeResolvedDefault : SortEvaluationMode.Auto;
			}
		} 

				#endregion // SortEvaluationModeResolved

				#region UnderlyingDataVersion

		internal int UnderlyingDataVersion
		{
			get
			{
				return this._underlyingDataVersion;
			}
		}

				#endregion //UnderlyingDataVersion	

				#region UnsortedInternalVerify

		
		
		/// <summary>
		/// Returns the _unsortedRecords member var.
		/// </summary>
		internal DataRecordCollection UnsortedInternalVerify
		{
			get
			{
				// JJD 1/10/08 - BR29572
				// Make sure that our DataSource is initialized
				this.VerifyRootDataSource( );

				return this._unsortedRecords;
			}
		}

				#endregion // UnsortedInternalVerify

				#region ViewableRecords







		internal ViewableRecordCollection ViewableRecords
		{
			get
			{
				if (this.HasGroups)
					return this.Groups.ViewableRecords;
	
				return this._sortedRecords.ViewableRecords;
			}
		}

				#endregion //ViewableRecords	

            #endregion //Internal Properties

            #region Private Properties

                // JJD 12/21/09 - TFS25835 - added
                #region ActiveDataRecordUnsortedIndex

        private int ActiveDataRecordUnsortedIndex
        {
            get
            {
                // For root managers use the old logic
                if (this._parentRecord == null)
                    return this.RootActiveDataRecordUnsortedIndex;

                // get the active record
                Record activeRecord = this._dataPresenter.ActiveRecord;

                if (activeRecord == null)
                    return -1;

                // see if the active record is a data record
                // if not get the active record's parent data record
                DataRecord dataRecord = activeRecord.RecordType == RecordType.DataRecord
                    ? activeRecord as DataRecord
                    : activeRecord.ParentDataRecord;

                // walk up the datarecord ancestor chain until we find the record 
                // at our level
                while (dataRecord != null && dataRecord.ParentRecord != this._parentRecord)
                    dataRecord = dataRecord.ParentDataRecord;
               
                // if null return -1
                if (dataRecord == null)
                    return -1;

                // If a reset is pending then process it now
                if (this._resetOperationPending != null)
                    this.OnSourceCollectionReset();
 
                return this._unsortedRecords.IndexOf(dataRecord);
            }
        }

                #endregion //ActiveDataRecordUnsortedIndex	
    
                #region CollectionViewCurrentPosition

        private int CollectionViewCurrentPosition
        {
            get
            {
                if (this._collectionViewWrapper != null)
                    return this._collectionViewWrapper.CurrentPosition;

                if (this._collectionView != null)
                    return this._collectionView.CurrentPosition;

                return -1;
            }
        }

                #endregion //CollectionViewCurrentPosition	

                // JJD 12/08/08 - Added 
                #region CollectionViewCurrentPosition

        private bool IsInSyncWithCurrentPosition
        {
            get
            {
                if (this._dataPresenter == null ||
                    this._collectionView == null ||
                    //this._parentRecord != null || JJD 12/21/09 - TFS25835 - allow nested record synchronization
                    !this._dataPresenter.IsInitialized ||
                    !this._dataPresenter.IsSynchronizedWithCurrentItem)
                    return false;

                // If a reset is pending then process it now
                if (this._resetOperationPending != null)
                    this.OnSourceCollectionReset();

                // JJD 12/21/09 - TFS25835 
                // Use the new ActiveDataRecordUnsortedIndex property instead
                // with supports synchronization of nested collectionviews
                //int rootActiveRecordindex = this.RootActiveDataRecordUnsortedIndex;
                int activeRecordindex = this.ActiveDataRecordUnsortedIndex;
                int position = this.CollectionViewCurrentPosition;
                bool isPositionOutOfRange = position < 0 || position == this._unsortedRecords.Count;

                if (activeRecordindex < 0)
                    return isPositionOutOfRange;

                if (isPositionOutOfRange)
                    return activeRecordindex < 0 ||
                        (this._currentAddRecord != null &&
                         this._currentAddRecord.IsAddRecord );

                return position == activeRecordindex;
            }
        }

                #endregion //CollectionViewCurrentPosition	
    
				// JJD 05/08/12 - TFS110865 - added
				#region AllowsAddNewCore

		private bool AllowsAddNewCore
		{
			get
			{
				// JJD 11/17/08 - TFS6743/BR35763 
				// If we have an IEditableCollectionView implementation
				// then return its CanAddNew
				if (this._editableCollectionView != null)
					return this._editableCollectionView.CanAddNew;

				// JJD 11/18/11 - TFS79001
				// If we don't have a binding list then return whether we have a type with a public 
				// parameterless constructor and a list that is not read-only
				if (this._bindingList == null)
				{
					if (_typeWithPublicCtor != null && _list != null)
					{
						return this._list.IsReadOnly == false &&
								this._list.IsFixedSize == false;
					}
				}

				// JJD 09/22/11  - TFS84708 - Optimization
				// Moved logic to helper method
				return DoesBindingListSupportAddNew(this.BindingList, _canGetPropertiesFromDataSourceWithoutItems);
			}
		}

				#endregion //AllowsAddNewCore	
        
				#region IsResetPending

		internal bool IsResetPending { get { return this._resetOperationPending != null; } }

				#endregion //IsResetPending	
    
				#region RecordsInsertedAtTop

		private List<DataRecord> RecordsInsertedAtTop
		{
			get
			{
				return this.ViewableRecords.RecordsInsertedAtTop;
			}
		}
    
				#endregion //RecordsInsertedAtTop	
      
                // JJD 12/08/08 - Added 
                #region RootActiveDataRecordUnsortedIndex

        private int RootActiveDataRecordUnsortedIndex
        {
            get
            {
                Record activeRecord = this._dataPresenter.ActiveRecord;
                DataRecord topLevelDataRecord = activeRecord != null ? activeRecord.TopLevelDataRecord : null;

                // If a reset is pending then process it now
                if (this._resetOperationPending != null)
                    this.OnSourceCollectionReset();

                return topLevelDataRecord != null ? this._unsortedRecords.IndexOf(topLevelDataRecord) : -1;
            }
        }

                #endregion //RootActiveDataRecordUnsortedIndex
         

                // JJD 11/25/08 - TFS6743/BR35763 - added 
                #region TreatUpdatesAsResets

        // JJD 11/25/08 - TFS6743/BR35763 - added 
        // If we have a filtered or sorted collectionView that doesn't
        // implement IEditableCollectiionView then we can't map the
        // indices that get passed into the change notifocations so we need
        // to treat them as resets
        private bool TreatUpdatesAsResets
        {
            get
            {
                if (this._collectionView != null &&
                    this._list == null)
                    return true;

                return false;
            }
        }

                #endregion //TreatUpdatesAsResets

            #endregion //Private Properties

        #endregion //Properties

        #region Methods

			#region Public Methods

				#region CommitAddRecord

		// SSP 11/18/09 TFS22330
		// 
		/// <summary>
		/// Commits the add-record to the data source.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>CommitAddRecord</b> method can be used to commit the add-record to the data source.
		/// If the add-record record is a template add-record that hasn't been associated with a data 
		/// item yet, a new data item will be added to the data list and associated with the add-record 
		/// and record will be committed to the data source.
		/// </para>
		/// </remarks>
		public void CommitAddRecord( )
		{
			DataRecord currentAddRecord = this.CurrentAddRecord;
			if ( null != currentAddRecord )
			{
				if ( currentAddRecord.IsAddRecordTemplate )
					currentAddRecord.OnEditValueChanged( true );

				currentAddRecord.Update( );
			}
		}

				#endregion // CommitAddRecord

				#region GetFilteredInDataRecords

		// SSP 1/5/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns data records that pass record filter conditions.
		/// </summary>
		/// <returns>Filtered data records.</returns>
		/// <remarks>
		/// <para class="body">
		/// Record filtering functionality allows the user to filter data by specifying 
		/// filter criteria. You can use <b>GetFilteredInDataRecords</b> method to get 
		/// data records that match the filter criteria.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="GetFilteredOutDataRecords"/>
		public IEnumerable<DataRecord> GetFilteredInDataRecords( )
		{
			GridUtilities.IMeetsCriteria filter = new GridUtilities.MeetsCriteriaChain(
				new GridUtilities.MeetsCriteria_RecordVisible( ),
				new GridUtilities.MeetsCriteria_FilteredInRecords( ),
				false );
			return GridUtilities.Filter<DataRecord>( this.Sorted, filter );
		}

				#endregion // GetFilteredInDataRecords

				#region GetFilteredOutDataRecords

		// SSP 1/5/09 - NAS9.1 Record Filtering
		// 
		/// <summary>
		/// Returns data records that do not pass record filter conditions.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// Record filtering functionality allows the user to filter data by specifying 
		/// filter criteria. You can use <b>GetFilteredOutDataRecords</b> method to get 
		/// data records that do not match the filter criteria. Note that the 
		/// <see cref="GetFilteredInDataRecords"/> method returns the data records that
		/// pass the filter criteria.
		/// </para>
		/// </remarks>
		/// <returns>Filtered out data records</returns>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldSettings.AllowRecordFiltering"/>
		/// <seealso cref="FieldLayoutSettings.FilterUIType"/>
		/// <seealso cref="GetFilteredInDataRecords"/>
		public IEnumerable<DataRecord> GetFilteredOutDataRecords( )
		{
			GridUtilities.IMeetsCriteria filter = new GridUtilities.MeetsCriteria_FilteredInRecords( );
			filter = new GridUtilities.MeetsCriteriaComplement( filter );

			return GridUtilities.Filter<DataRecord>( this.Sorted, filter );
		}

				#endregion // GetFilteredOutDataRecords

				// AS - NA 11.2 Excel Style Filtering
				#region GetRecordFiltersResolved
		/// <summary>
		/// Returns the <see cref="RecordFilterCollection"/> that will affect the specified FieldLayout's fields for records in this RecordManager instance.
		/// </summary>
		/// <param name="fieldLayout">The field layout whose RecordFilterCollection is to be returned.</param>
		/// <returns>Returns the <see cref="RecordManager.RecordFilters"/> or <see cref="Infragistics.Windows.DataPresenter.FieldLayout.RecordFilters"/> depending on the resolved <see cref="FieldLayoutSettings.RecordFilterScope"/>.</returns>
		[InfragisticsFeature(Version = FeatureInfo.Version_11_2, FeatureName = FeatureInfo.FeatureName_ExcelFiltering)]
		public RecordFilterCollection GetRecordFiltersResolved(FieldLayout fieldLayout)
		{
			return this.RecordFiltersResolved.GetRecordFilters(fieldLayout, true);
		} 
				#endregion //GetRecordFiltersResolved

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent
				#region ResetDataValueHistory

		/// <summary>
		/// Removes all data value change history (except for the latest value) for all <see cref="Cell"/>s in all <see cref="Record"/>s within this RecordManager.
		/// </summary>
		/// <param name="recursive">If true, then this method is recursively called on child <see cref="RecordManager"/>s.</param>
		[InfragisticsFeature(Version = FeatureInfo.Version_9_2, FeatureName = FeatureInfo.FeatureName_DataValueChangedEvent)]
		public void ResetDataValueHistory(bool recursive)
		{
			// AS 5/26/11 Optimization
			// Don't force allocation of the records just to remove history that won't be there.
			//
			//foreach(DataRecord dataRecord in this.Unsorted)
			foreach (DataRecord dataRecord in this._unsortedRecords.SparseArray.NonNullItems)
			{
				dataRecord.ResetDataValueHistory(recursive);
			}
		}

				#endregion //ResetDataValueHistory

			#endregion // Public Methods

			#region Internal Methods

				// JJD 10/02/08 added
                #region AccessAllRecords

		// AS 3/3/11 NA 2011.1 - Async Exporting
		// Access all records to force them to be created
		//internal void AccessAllRecords(bool recursive)
		//{
		//    if (this.IsResetPending)
		//        this.OnSourceCollectionReset();
		//
		//    int count = this.Unsorted.Count;
		//
		//    for (int i = 0; i < count; i++)
		//    {
		//        DataRecord dr = this._unsortedRecords[i];
		//
		//        // JJD 11/10/09 - TFS24217
		//        // Don't allocate the ChildRecords collection unless there is at least
		//        // one expandable field that is not collapsed
		//        //if (dr != null && recursive)
		//        if (dr != null && recursive && dr.FieldLayout.Fields.NotCollapsedExpandableFieldsCount > 0)
		//        {
		//            ExpandableFieldRecordCollection children = dr.ChildRecords;
		//
		//            if (children != null)
		//            {
		//                int childCount = children.Count;
		//
		//                for (int j = 0; j < childCount; j++)
		//                {
		//                    ExpandableFieldRecord efr = children[j];
		//
		//                    if (efr != null)
		//                    {
		//                        RecordManager rm = efr.ChildRecordManager;
		//
		//                        if (rm != null)
		//                            rm.AccessAllRecords(true);
		//                    }
		//                }
		//            }
		//        }
		//    }
		//}

                #endregion //AccessAllRecords	

				#region AddNew

		internal object AddNew()
		{
			return this.AddNew(0);
		}

		// AS 5/7/09 NA 2009.2 ClipboardSupport
		// Added an overload so the clipboard support could try to maintain the order of the 
		// records with respect to the data when the add record is on top.
		//
		internal object AddNew(int recordOnTopOffset)
		{
			IBindingList bl = this.BindingList;

            // JJD 11/17/08 - TFS6743/BR35763 
            // We only need to bail if both _editableCollectionView and bindinglist are null
            if (this._editableCollectionView == null)
            {
				// JJD 11/18/11 - TFS79001
				// If IBindingList is null allow logic to fall thru as long as 
				// the type has a public parameterless constructor
				//if (bl == null || bl.AllowNew == false)
				if (bl != null)
				{
					if (bl.AllowNew == false)
						return null;
				}
				else
				{
					// JJD 11/18/11 - TFS79001
					// Make sure we have a type that is creatable and
					// a list that can accept it by seeing if DataSourceAllowsAddNew
					// returns true
					if (!this.DataSourceAllowsAddNew)
						return null;
				}
            }

			// set a flag so we know when we get the add notification
			// we know that we called AddNew
			this._inAddNew = true;

			try
			{
				DataRecord addRecord = this.CurrentAddRecord;

				// AS 5/7/09 NA 2009.2 ClipboardSupport [Start]
				// We need to cache the requested on top offset so that we can insert the record 
				// in the OnSourceCollectionAddOrRemove in the proper sorted index.
				//
				bool isAddRecordOnTop = this.IsAddRecordOnTop;
				int actualRecordOnTopOffset = 0;

				if (isAddRecordOnTop)
				{
					List<DataRecord> recordsOnTop = this.RecordsInsertedAtTop;

					// make sure its within the records on top count
					actualRecordOnTopOffset = Math.Max(0, Math.Min(recordsOnTop.Count, recordOnTopOffset));
				}
				this._newRecordOnTopOffset = actualRecordOnTopOffset;
				//
				// AS 5/7/09 NA 2009.2 ClipboardSupport [End]

                // JJD 11/17/08 - TFS6743/BR35763 
                // If we have an EditableCollectionView then call its AddNew
				//object dataItem = bl.AddNew();
				object dataItem;
				if (this._editableCollectionView != null)
					dataItem = this._editableCollectionView.AddNew();
				else
				{
					if (bl != null)
					{
						dataItem = bl.AddNew();
					}
					else
					{
						// JJD 11/18/11 - TFS79001
						// Create an item using its public ctor and
						// keep track of it in a member variable and add
						// it to the list
						try
						{
							// JJD 05/08/12 - TFS110865 - special case strings
							if (_typeWithPublicCtor == typeof(string))
								dataItem = string.Empty;
							else
								dataItem = Activator.CreateInstance(_typeWithPublicCtor);
						}
						catch (Exception e)
						{
							// raise the data error event if the ctor threw and exception
							_dataPresenter.RaiseDataError(null, this._parentRecord != null ? this._parentRecord.Field : null, e, DataErrorOperation.RecordAdd, 
								DataPresenterBase.GetString("DataErrorUnableToCreateInstance", _typeWithPublicCtor, e.InnerException));
							return null;
						}
						
						// add the item to the underlying list
						int index = _list.Add(dataItem);

						// JJD 11/18/11 - TFS79001
						// if the list doesn't support change notifications then we need to simulate
						// the notification that would have been generated so we can synchronize
						// our record collections
						
						if (_list.Count > _unsortedRecords.Count)
							this.OnSourceCollectionAddOrRemove(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<object> { dataItem }, index));
					}
				}

				// if we get here then the add was successful so if
				// we are 
				if (this.IsAddRecordOnTop)
				{
					// add the record into out list that keeps track of the records
					// that were inserted at the top. We use this list so we can accurately map
					// the unsorted/sorted indices.
					//
					// NOTE: we add this record to the end of the list for efficiency sake
					// which is the reverse order to their order in the ViewableRecords collection 
					// AS 5/7/09 NA 2009.2 ClipboardSupport
					//this.RecordsInsertedAtTop.Add(addRecord); 
					List<DataRecord> recordsOnTop = this.RecordsInsertedAtTop;
					if (recordOnTopOffset == 0)
						recordsOnTop.Add(addRecord);
					else
					{
						int index = Math.Max(0, Math.Min(recordsOnTop.Count, recordsOnTop.Count - recordOnTopOffset));
						recordsOnTop.Insert(index, addRecord);
					}
				}

				return dataItem;
			}
			finally
			{
				// AS 5/7/09 NA 2009.2 ClipboardSupport
				this._newRecordOnTopOffset = 0;

				// reset the flag
				this._inAddNew = false;
			}
		}

				#endregion //AddNew	
 
				#region AddNewRecordLocation

		internal AddNewRecordLocation AddNewRecordLocation
		{
			get
			{
				if (this.DataSourceAllowsAddNew == false)
					return AddNewRecordLocation.Default;

				DataPresenterBase dp = this.DataPresenter;

				Debug.Assert(dp != null);

				if (dp == null)
					return AddNewRecordLocation.Default;

				ViewBase view = dp.CurrentViewInternal;

				// Ask the View if it supports an AddNew record.
				if (view == null ||	!view.IsAddNewRecordSupported)
					return AddNewRecordLocation.Default;

				// we don't support add records while we are in groupby mode
				if (this.HasGroups)
					return AddNewRecordLocation.Default;

				// JM 12-22-09 TFS25883 - Don't show add record if there is any sort criteria on the FieldLayout
				if (this._fieldLayout != null && this._fieldLayout.HasGroupBySortFields)
					return AddNewRecordLocation.Default;

				if (this._fieldLayout == null)
				{
					// get the appropriate field layout based on the source items
					// AS 6/5/07
					// We should have been using the helper method.
					//
					//this._fieldLayout = dp.FieldLayouts.GetDefaultLayoutForItem(null, this._sourceItems);
                    // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                    // Added parentCollection parameter
                    //this.InitializeFieldLayout(dp.FieldLayouts.GetDefaultLayoutForItem(null, this._sourceItems));
                    RecordCollectionBase parentRecordCollection = this._parentRecord != null ? this._parentRecord.ChildRecords : null;
                    this.InitializeFieldLayout(dp.FieldLayouts.GetDefaultLayoutForItem(null, this._sourceItems, parentRecordCollection));

					// make sure the field layout is initialized
                    if (this._fieldLayout != null &&
                        !this._fieldLayout.AreFieldsInitialized)
                    {
                        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                        // Added parentRecordCollection parameter
                        //this._fieldLayout.InitializeFields(null, this._sourceItems);
                        this._fieldLayout.InitializeFields(null, this._sourceItems, parentRecordCollection);
                    }
				}

				if (this._fieldLayout != null &&
					this._fieldLayout.AllowAddNewResolved == true)
				{
					AddNewRecordLocation location = this._fieldLayout.AddNewRecordLocationResolved;

					if (!view.IsFixedRecordsSupported)
					{
						if (location == AddNewRecordLocation.OnTopFixed)
							location = AddNewRecordLocation.OnTop;
						else
						if (location == AddNewRecordLocation.OnBottomFixed)
							location = AddNewRecordLocation.OnBottom;
					}

					return location;
				}

				return AddNewRecordLocation.Default;
			}
		}

				#endregion //AddNewRecordLocation	
    
                // JJD 2/22/08 - BR30660 
                // Not used
                #region AddRecord

        //internal void AddRecord(DataRecord record)
        //{
        //    this.InsertRecord(this._unsortedRecords.Count, record);
        //}

                #endregion //AddRecord

				#region BeginUpdate

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// 
		internal void BeginUpdate( )
		{
			if ( null == _beginUpdateInfo )
				_beginUpdateInfo = new BeginUpdateInformation( this );

			_beginUpdateInfo._beginUpdateCount++;
		}

				#endregion // BeginUpdate

                #region Clear

        internal void Clear()
        {
            // JJD 12/02/08 - TFS6743/BR35763
            // By default clear the groups
            //this.Clear(true);
            this.Clear(true, true);
		}

        // JJD 12/02/08 - TFS6743/BR35763
        // Added clearGroups param
        //private void Clear(bool notifyListeners)
        private void Clear(bool notifyListeners,  bool clearGroups)
        {
			this.ClearCachedRecordsOnTop();

            // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing 
            // clear the fixed records
            this.ViewableRecords.ClearFixedRecords();

            if (this._unsortedRecords.Count > 0)
            {
                if (notifyListeners)
                {
                    this._unsortedRecords.Clear();
                    this.RaisePropertyChangedEvent("Unsorted");
                }
                else
                    this._unsortedRecords.SparseArray.Clear();
            }

            if (this._sortedRecords != null && this._sortedRecords.Count > 0)
            {
                if (notifyListeners)
                {
                    this._sortedRecords.Clear();
                    this.RaisePropertyChangedEvent("Sorted");
                }
                else
                    this._sortedRecords.SparseArray.Clear();
            }

            // JJD 12/02/08 - TFS6743/BR35763
            // Only clear the groups if the flag is set
            //if (this._groups != null && this._groups.Count > 0)
            if (clearGroups == true && this._groups != null && this._groups.Count > 0)
            {
                if (notifyListeners)
                {
                    this._groups.Clear();
                    this.RaisePropertyChangedEvent("Groups");
                }
                else
                    this._groups.SparseArray.Clear();
            }
        }

                #endregion //RemoveRecord
            
				#region CommitRecord

		internal bool CommitRecord(DataRecord record)
		{
			// reset the flag on the record in case we catch an exception below
			record.IsDataChangedSinceLastCommitAttempt = false;

			// cache the record being committed so we can deal
			// with notificiations correctly
			this._recordBeingCommitted = record;

			bool isAddRecord = record.IsAddRecord;

			try
			{
                // JJD 11/17/08 - TFS6743/BR35763 
                // If we have an IEditableCollectionView implementation
                // then use its commit methods
                if (this._editableCollectionView != null)
                {
                    if (this._editableCollectionView.IsAddingNew &&
						// JJD 11/18/11 - TFS79001 
						// Use the GridUtilities.AreEqual helper method instead
						//EditableCollectionView.CurrentAddItem == record.DataItem)
						GridUtilities.AreEqual( EditableCollectionView.CurrentAddItem, record.DataItem))
                        this._editableCollectionView.CommitNew();
                    else
                    {
                        if (this._editableCollectionView.IsEditingItem)
                        {
							// JJD 11/18/11 - TFS79001 
							// Use the GridUtilities.AreEqual helper method instead
							//Debug.Assert(this._editableCollectionView.CurrentEditItem == record.DataItem);
                            Debug.Assert(GridUtilities.AreEqual( this._editableCollectionView.CurrentEditItem, record.DataItem));

							// JJD 11/18/11 - TFS79001 
							// Use the GridUtilities.AreEqual helper method instead
							//if (this._editableCollectionView.CurrentEditItem == record.DataItem)
                            if (GridUtilities.AreEqual( this._editableCollectionView.CurrentEditItem, record.DataItem))
                                this._editableCollectionView.CommitEdit();
                        }
                    }
                }
                else
                {
					
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

					IEditableObject editableObject = record.DataItem as IEditableObject;
					// JJD 6/17/09 - TFS12228 
					// Don't call CancelEdit unless BeginEdit was called
					// JJD 5/04/10 - TFS31723
					// Also call EndEdit() if this is the add record
					//if (null != editableObject && record.WasBeginEditCalled == true)
					if (null != editableObject && (isAddRecord || record.WasBeginEditCalled == true))
					{
						editableObject.EndEdit();
					}
                    // SSP 10/12/07 BR26397
                    // If the list object doesn't implement IEditableObject then see if the
                    // list implements ICancelAddNew. If so then call EndNew on that if this
                    // row is an add-row (ICancelAddNew is only meant for add-rows as its
                    // name implies).
                    // 
					
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)


                    if (isAddRecord)
                    {
                        IList dataList = this.List;
                        ICancelAddNew cancelAddNew = dataList as ICancelAddNew;
                        if (null != cancelAddNew)
                        {
                            int listIndex = record.DataItemIndex;
							if (listIndex >= 0 && listIndex < dataList.Count)
							{
								cancelAddNew.EndNew(listIndex);
								
								
								
								
								
							}
                        }
                    }

					
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

                }

				// SSP 4/30/08 BR32427
				// We should do this at the same time that we reset the isAddRecord flag.
				// 
				
#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


				return true;
			}
			// SSP 8/10/07 BR25635
			// Moved this into the DataRecord.CommitChanges. The purpose of the move was to 
			// raise the data error after resetting the _isCommittingChanges flag.
			// 
			//catch (Exception exception)
			//{
			//    this.DataPresenter.RaiseDataError(record, null, exception, DataErrorOperation.RecordUpdate, "Unable to update record.");

			//    return false;
			//}
			finally
			{
				this._recordBeingCommitted = null;
			}
		}

				#endregion //CommitRecord	

                #region ContainsFieldLayout

        internal bool ContainsFieldLayout(FieldLayout fl)
        {
            if (this._fieldLayout == fl)
                return true;

            if (this._sortGroupByVersionTrackers != null)
                return this._sortGroupByVersionTrackers.ContainsKey(fl);

            return false;
        }

                #endregion //ContainsFieldLayout	
        
				#region DeleteRecord






		internal void DeleteRecord(DataRecord record)
		{
			if (null == this.DataPresenter || 
				this.DataSourceAllowsDelete == false)
				return;

			Debug.Assert(record.ParentCollection.ParentRecordManager == this);

			if (record.ParentCollection.ParentRecordManager != this)
				return;

			// If the cell in edit mode is from this record then exit the edit mode before 
			// deleting the row.
			//
			Cell activeCell = this.DataPresenter.ActiveCell;
			if (null != activeCell && 
				record == activeCell.Record && 
				activeCell.IsInEditMode)
				activeCell.EndEditMode(true, true);

			try
			{
				this._recordBeingDeleted = record;


				// cancel any pending edits on the record
                if (record.IsDataChanged)
                    record.CancelUpdate();

				// JJD 1/24/07 -BR19237
				// add records would have been removed by the call
				// to CancelUpdate above so we can bypass the logic below and
				// just return
				if (record.IsAddRecord)
					return;

				// get the list index
				int listIndex = record.DataItemIndex;

				Debug.Assert(listIndex >= 0 &&
                             listIndex < this._list.Count &&
							// JJD 11/18/11 - TFS79001 
							// Use the GridUtilities.AreEqual helper method instead
                             //this._list[listIndex] == record.DataItem);
                            GridUtilities.AreEqual( this._list[listIndex], record.DataItem));

                if (listIndex >= 0 &&
                     listIndex < this._list.Count &&
					// JJD 11/18/11 - TFS79001 
					// Use the GridUtilities.AreEqual helper method instead
					//this._list[listIndex] == record.DataItem)
                     GridUtilities.AreEqual( this._list[listIndex], record.DataItem))
                    this._list.RemoveAt(listIndex);
                else
                {
                    listIndex = -1;

                    if (record.DataItem != null)
                    {
                        // JJD 2/20/09 - TFS6106/BR33996
                        // Get the index so we know it is valid
                        //this._list.Remove(record.DataItem);
                        listIndex = this._list.IndexOf(record.DataItem);

                        if (listIndex >= 0)
                            this._list.RemoveAt(listIndex);

                    }
                }

                Debug.Assert(listIndex >= 0, "Should have a valid index during delete");

                // JJD 2/20/09 - TFS6106/BR33996
                // If we didn't receive a notification from the datasoource
                // that the item was deleted then we need to call the RemoveRecordHelper
                // method so we remove it from our record collections
                if (listIndex >= 0 && !record.IsDeleted)
                {
                    // JJD 5/22/09 - TFS17691
                    // Changed from bool to the object that was wired
                    //Debug.Assert(this._notifyCollectionChangedHooked == false && this._bindingList == null, "If the datasource implemneted INotifyCollectionChange or IBindlingList the record should have already been deleted.");
                    Debug.Assert(this._notifyCollectionChangedHooked == null && this._bindingList == null, "If the datasource implemented INotifyCollectionChange or IBindlingList the record should have already been deleted.");
                    int index = this._unsortedRecords.IndexOf(record);

                    if (index >= 0)
                        this.RemoveRecordHelper(index);
                }
 
			}
			catch (Exception exception)
			{
				
				
				
				this.DataPresenter.RaiseDataError( record, null, exception, DataErrorOperation.RecordDelete, DataPresenterBase.GetString( "DataErrorDeleteRecordUnableToDelete", exception.Message ) );
			}
			finally
			{
				this._recordBeingDeleted = null;
			}

		}

				#endregion //DeleteRecord	

				#region DirtySummaryResults

		
		
		internal static void DirtySummaryResults( Record recordContext, 
			Field fieldContext, RecordCollectionBase recordsContext, RecordManager recordManagerContext )
		{
			// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
			// Don't recalculated the summaries when we are updating the collection view with new sorted 
			// fields because that results in a reset notification however nothing has really changed for 
			// us to recalculate summaries.
			// 
			if ( null != recordManagerContext && null != recordManagerContext._sortedFieldsToCollectionViewSynchronizer
				&& recordManagerContext._sortedFieldsToCollectionViewSynchronizer.IsUpdatingCollectionView )
				return;

			// SSP 8/2/09 - Optimizations
			// 
			if ( null != recordContext && null == recordsContext )
				recordsContext = recordContext.ParentCollection;

			// SSP 8/2/09 - Optimizations
			// Added the if block and enclosed the existing code into the else block.
			// 
			if ( null != recordsContext )
			{
				RecordCollectionBase iiRecords = recordsContext;

				while ( null != iiRecords )
				{
					SummaryResultCollection summaryResults = iiRecords.SummaryResultsIfAllocated;
					if ( null != summaryResults )
						// SSP 9/19/2011 TFS88364
						// Added fieldLayoutContext parameter.
						// 
						//summaryResults.DirtyAffectedSummariesAsync( fieldContext );
						summaryResults.DirtyAffectedSummariesAsync( fieldContext, recordsContext.FieldLayout );

					Record iiParentRecord = iiRecords.ParentRecord;
					iiRecords = null != iiParentRecord ? iiParentRecord.ParentCollection : null;
				}
			}
			else
			{
				FieldLayout fl = GridUtilities.GetFieldLayout( recordContext, fieldContext, recordsContext, recordManagerContext );
				SummaryDefinitionCollection summaries = null != fl ? fl.SummaryDefinitionsIfAllocated : null;
				if ( null != summaries )
					summaries.BumpDataVersion( );
			}
		}

				#endregion // DirtySummaryResults

				// JJD 09/22/11  - TFS84708 - Optimization - added
				#region DoesBindingListSupportAddNew

		internal static bool DoesBindingListSupportAddNew(IBindingList bindingList, bool canGetPropertiesFromDataSourceWithoutItems)
		{

			// JJD 2/6/09 - TFS13615
			// Even if IBindingList.AllowNew returns true we might still not have enough information
			// to create an add record
			if (bindingList != null)
			//return bindingList.AllowNew;
			{
				if (bindingList.AllowNew == false)
					return false;

				// JJD 2/6/09 - TFS13615
				// Checked the flag that tells us if we can get property descriptors from the datasource
				// even if there are no items in the list
				if (canGetPropertiesFromDataSourceWithoutItems)
					return true;

				// JJD 2/6/09 - TFS13615
				// return true if we have at least one record whose DataItem we can use as a template
				return (bindingList.Count > 0);
			}

			return false;
		}

				#endregion //DoesBindingListSupportAddNew	
    
				#region EndUpdate

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// 
		// JJD 5/19/07
		// Optimization - Added dirtyScrollCount flag on EndUpdate method
		//internal void EndUpdate()
		internal void EndUpdate( bool dirtyScrollCount )
		{
			BeginUpdateInformation info = _beginUpdateInfo;
			if ( null != info )
			{
				if ( dirtyScrollCount )
					info._dirtyScrollCount = true;

				info._beginUpdateCount--;
				if ( 0 == info._beginUpdateCount )
				{
					if ( info._dirtyScrollCount )
					{
						_scrollCountVersion++;

						ViewableRecordCollection vrc = this.ViewableRecords;
						MainRecordSparseArray sparseArr = null != vrc ? vrc.RecordCollectionSparseArray : null;
						if ( null != sparseArr )
							sparseArr.DirtyScrollCount( );
					}

					_beginUpdateInfo = null;
					info.ProcessPendingNotifications( );
				}
			}
		}

				#endregion // EndUpdate
		
				#region GetAddRecordCellData

		internal object GetAddRecordCellData(Field field)
		{
			// if the cache contains a value for this field then return it
			if (this._addRecordCellDataDictionary != null &&
				 this._addRecordCellDataDictionary.Count > 0 &&
				 this._addRecordCellDataDictionary.Contains(field))
				return this._addRecordCellDataDictionary[field];

			// nothing was in the cache so return null
			return null;
		}

				#endregion //GetAddRecordCellData	

                // JJD 4/3/08 - added support for printing
                #region GetAssociatedDataRecord


        // Lazily maintain a map between print records and their associated records
        // in the source ui datapresenter
        internal Record GetAssociatedRecordInternal(Record record)
        {
            // MBS 7/29/09 - NA9.2 Excel Exporting
            //DataPresenterReportControl dprc = this._dataPresenter as DataPresenterReportControl;
            DataPresenterExportControlBase dprc = this._dataPresenter as DataPresenterExportControlBase;

            Debug.Assert(dprc != null);

            if (dprc == null)
                return null;

            // if the report version has changed clear the cached map
            if (dprc.ReportVersion != this._reportVersion)
                this._associatedRecordMap = null;

            if (this._associatedRecordMap == null)
            {
                // allocate a new map
                this._associatedRecordMap = new Dictionary<Record, Record>((int)(this._unsortedRecords.Count * 1.5));

                RecordManager associatedRecordManager = this.AssociatedRecordManager;

                Debug.Assert(associatedRecordManager != null);

                if (associatedRecordManager == null)
                    return null;

                int count = this._unsortedRecords.Count;

                // If the datasource hasn't been initialized yet, i.e. the ui dp hasn't been displayed yet
                // then force it now.
                if ( associatedRecordManager.DataSource != associatedRecordManager.DataPresenter.DataSourceInternal )
                    associatedRecordManager.DataPresenter.VerifyRecordManagerDataSource();

                // If a reset is pending perform it now
                if (associatedRecordManager.IsResetPending)
                    associatedRecordManager.OnSourceCollectionReset();

                // If the associated rm has no records then return null
                if (associatedRecordManager._unsortedRecords.Count == 0)
                    return null;

                Debug.Assert(count == associatedRecordManager._unsortedRecords.Count);

                if (count != associatedRecordManager._unsortedRecords.Count)
                    return null;

                // loop over the unsorted records and create a map entry for each one
                for (int i = 0; i < count; i++)
                {
                    this._associatedRecordMap[this._unsortedRecords[i]] = associatedRecordManager._unsortedRecords[i];

                    Debug.Assert(this._unsortedRecords[i].IsDataItemEqual(associatedRecordManager._unsortedRecords[i].DataItem));
                }

                this.VerifySort();
                associatedRecordManager.VerifySort();

                if (this.HasGroups && associatedRecordManager.HasGroups)
                    this.MapAssociatedGroupByRecords(this.Groups, associatedRecordManager.Groups);
            }

            Record associatedDataRecord;

            // Get the asscoiated recoird from the map
            bool rcdFoundInMap = this._associatedRecordMap.TryGetValue(record, out associatedDataRecord);

            Debug.Assert(rcdFoundInMap == true || record is GroupByRecord || this._associatedRecordMap.Count == 0);

            return associatedDataRecord;
        }

                #endregion //GetAssociatedDataRecord

                #region InitializeAddRecordCellValuesFromCache

        internal void InitializeAddRecordCellValuesFromCache(DataRecord addRecord)
		{
			Debug.Assert(this._currentAddRecord == addRecord);

			if (this._currentAddRecord == addRecord)
			{
				//// call VerifySpecialRecordCounts with false to
				//// prevent a rest notification being generated the next
				//// time the collection is accessed
				//if (this._visibleRecords != null)
				//    this._visibleRecords.VerifySpecialRecordCounts(false);

				if (this._addRecordCellDataDictionary != null &&
					this._addRecordCellDataDictionary.Count > 0)
				{
					// JJD 6/29/11 - TFS80117
					// Verify that each entry's FieldLayout matches the Datarecord's FieldLayout.
					// Therefore we need to copy the DictionaryEntries in a temp list
					// so we can remove entries from the dictionary without meeing
					// with the state of the enumerator
					//// loop over all the cached cell values and call set value now that
					//// we have an actual data record
					//foreach (DictionaryEntry entry in this._addRecordCellDataDictionary)
					//    // AS 4/15/09 Field.(Get|Set)CellValue
					//    //((Field)(entry.Key)).SetCellValue(addRecord, entry.Value, false);
					//    addRecord.SetCellValue((Field)(entry.Key), entry.Value, false);
					List<DictionaryEntry> entries = new List<DictionaryEntry>(this._addRecordCellDataDictionary.Count);
					foreach (DictionaryEntry entry in this._addRecordCellDataDictionary)
						entries.Add(entry);

					// JJD 6/29/11 - TFS80117
					// cache the record's FieldLayout
					FieldLayout flRecord = addRecord.FieldLayout;

					// loop over all the cached cell values and call set value now that
					// we have an actual data record
					foreach (DictionaryEntry entry in entries)
					{
						Field fld = (Field)(entry.Key);
						FieldLayout flEntry = fld != null ? fld.Owner : null;

						// JJD 6/29/11 - TFS80117
						// Verify that the layouts match
						if (flEntry == null || flEntry != flRecord)
						{
							// JJD 6/29/11 - TFS80117
							// If the entry's layout is no longer in the FieldLayout's collection
							// then remove the entry from the dictionary
							if ( flEntry == null || _dataPresenter.FieldLayouts.Contains(flEntry))
								_addRecordCellDataDictionary.Remove(fld);
						}
						else
						{
							addRecord.SetCellValue(fld, entry.Value, false);
						}
					}
				}
			}
		}

				#endregion //InitializeAddRecordCellValuesFromCache	

                #region InsertRecord

        // JJD 2/22/08 - BR30660 
        // Not used
        //internal void InsertRecord(int index, DataRecord record)
        //{
        //    this.InsertRecord(index, index, record, true);
        //}
        private void InsertRecord(int index, int sortedIndex, DataRecord record, bool notifyListeners)
        {
            if (notifyListeners)
            {
                this._unsortedRecords.InsertRecord(index, record);
                this._sortedRecords.InsertRecord(sortedIndex, record);
                this.RaisePropertyChangedEvent("Unsorted");
            }
            else
            {
                this._unsortedRecords.SparseArray.Insert(index, record);
                this._sortedRecords.SparseArray.Insert(sortedIndex, record);
            }

            if (this._groups != null && this._groups.Count > 0)
            {
                // JJD 2/25/08 - BR30660
                // Instead of clearing the groups which was just wrong
                // we should add the specific record to the Groups's ViewableRecords.
                // Note: this causes a top down insertion looking for and/or
                // constructing the appropriate groupby record hierarchy.
                //if (notifyListeners)
                //{
                //    this._groups.Clear();
                //    this.RaisePropertyChangedEvent("Groups");
                //}

                Debug.Assert(record != null);

                this._groups.ViewableRecords.InsertDataRecordInGroupBySlot(record);

            }
        }

                #endregion //InsertRecord

				#region IsFixedRecord
		internal bool IsFixedRecord(Record record)
		{
			switch (this.AddNewRecordLocation)
			{
				case AddNewRecordLocation.OnTopFixed:
				case AddNewRecordLocation.OnBottomFixed:
					return this._currentAddRecord == record;
				default:
					return false;
			}
		} 
				#endregion //IsFixedRecord

				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
				#region OnCellValueChangeNotification

		internal void OnCellValueChangeNotification(DataRecord record, PropertyChangedEventArgs e)
		{
			this.OnChangeNotification(record, e, false);
		}

				#endregion //OnCellValueChangeNotification	

				#region OnDataChanged

		
		
		/// <summary>
		/// Called whenever underlying data changes so affected functionalities like summaries
		/// get recalculated.
		/// </summary>
		internal void OnDataChanged( DataChangeType dataChangeType, DataRecord recordContext, Field fieldContext )
		{
			FieldLayout fl = GridUtilities.GetFieldLayout( recordContext, fieldContext, null, this );
			if ( null == fl )
				return;

			DirtySummaryResults( recordContext, fieldContext, null, this );

			// SSP 12/17/08 - NAS9.1 Record Filtering
			// We need to re-evaluate filters when data changes.
			// 
			this.OnDataChanged_DirtyFiltersHelper( dataChangeType, fl, recordContext, fieldContext );

			// SSP 9/11/11 Calc
			// 
			this.NotifyCalcAdapterHelper( dataChangeType, recordContext, fieldContext );

			// AS 7/1/09 NA 2009.2 Field Sizing
			fl.AutoSizeInfo.OnDataChanged(dataChangeType, recordContext, fieldContext, this);
		}

				#endregion // OnDataChanged

				#region OnDataChanged_DirtyFiltersHelper

		// SSP 12/17/08 - NAS9.1 Record Filtering
		// 
		internal void OnDataChanged_DirtyFiltersHelper( DataChangeType dataChangeType, FieldLayout fl, DataRecord recordContext, Field fieldContext )
		{
			ResolvedRecordFilterCollection filters = _recordFiltersResolved;
			if ( null != filters )
			{
				bool dirtyAllRecords = false;

				// If we get Reset notification then re-evaluate filters on all records.
				// 
				if ( DataChangeType.Reset == dataChangeType )
				{
					// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// Enclosed the existing code in the if block. Don't re-evaluate filters when we are updating the collection view
					// with new sorted fields because that results in a reset notification however nothing has really changed for us to
					// re-evaluate filters.
					// 
					if ( null == _sortedFieldsToCollectionViewSynchronizer || ! _sortedFieldsToCollectionViewSynchronizer.IsUpdatingCollectionView )
						dirtyAllRecords = true;
				}
				// Otherwise if we get item change notification and ReevaluateFiltersOnDataChange is
				// set to true then re-evaluate filters on the changed record.
				// 
				else if ( fl.ReevaluateFiltersOnDataChangeResolved )
				{
					if ( DataChangeType.CellDataChange == dataChangeType || DataChangeType.RecordDataChange == dataChangeType )
					{
						DataPresenterBase dp = this.DataPresenter;
						Record activeRecord = null != dp ? dp.ActiveRecordInternal : null;
						// Don't re-evaluate filters on active record until its committed. 
						
						// 
						if ( null != recordContext && activeRecord == recordContext )
						{
							// _activeRecordWithPendingDirtyFilterState should have been cleared when
							// the active record changed.
							// 
							Record tmp = dp._activeRecordWithPendingDirtyFilterState;
							if ( null != tmp && tmp != activeRecord )
							{
								// JM 01-22-09 - I hit this assert in the CustomFilterSelectionControl's grid. After showing it to Sandip
								// he recommended commenting it out since this condition is being properly dealt with somewhere else.
								//Debug.Assert( false,
								//    "_activeRecordWithPendingDirtyFilterState is always cleared when active"
								//    + " record changes and therefore it can't be a different record than the active record." );

								tmp.DirtyFilterState( );
							}

							dp._activeRecordWithPendingDirtyFilterState = recordContext;
						}
						else
						{
							// SSP 5/24/11 TFS76271
							// Moved the existing code into the new DoesFilterCriteriaUseAllValues method.
							// 
							if ( filters.DoesFilterCriteriaUseAllValues( fieldContext, recordContext ) )
								dirtyAllRecords = true;

							if ( !dirtyAllRecords && null != recordContext )
							{
								// SSP 12/22/11 TFS67264 - Optimizations
								// If filter conditions have not been applied to the record, then either there are no filter
								// conditions or that they are yet to be applied. In either case, there's no need to dirty
								// the filter state. Although this results in a NOOP anyways, there is performance overhead
								// of checking for filter conditions. This optimization comes into play in high frequency
								// update scenarios.
								// 
								//recordContext.DirtyFilterState( );
								if ( recordContext.HasFilterCriteriaBeenApplied )
									recordContext.DirtyFilterState( );
							}
						}
					}
						// SSP 5/24/11 TFS76271
						// If filter criteria like Top10 or AboveAverage is being used and a record is added or removed
						// then we need to re-evaluate the filters on all records.
						// 
					else if ( DataChangeType.AddRecord == dataChangeType || DataChangeType.RemoveRecord == dataChangeType )
					{
						if ( filters.DoesFilterCriteriaUseAllValues( fieldContext, recordContext ) )
						    dirtyAllRecords = true;
					}
				}

				if ( dirtyAllRecords )
					filters.BumpVersion( );
			}
		}

				#endregion // OnDataChanged_DirtyFiltersHelper

				// JJD 11/18/11 - TFS79001
				#region OnDataItemRemoved

		internal void OnDataItemRemoved(DataRecord rcd, object dataItem)
		{
			// if the counts match then the list must have sent a remove notification
			// so we can bail
			if (_list.Count == _unsortedRecords.Count)
				return;

			// otherwise remove the rcd from our cached lists
			int index = this._unsortedRecords.SparseArray.IndexOf(rcd);

			if (index >= 0)
				this.RemoveAt(index);
		}

				#endregion //OnDataItemRemoved	
    
				#region OnIsNestedDataDisplayEnabledChanged

		internal void OnIsNestedDataDisplayEnabledChanged()
		{
			if (this._currentAddRecord != null)
				this._currentAddRecord.OnIsNestedDataDisplayEnabledChanged();

			if (this._unsortedRecords.Count == 0)
				return;

			// loop over any previously allocated records to notify them
			foreach (DataRecord record in this._unsortedRecords.SparseArray.NonNullItems)
				record.OnIsNestedDataDisplayEnabledChanged();
		}

				#endregion //OnIsNestedDataDisplayEnabledChanged	
    
				#region PrepareForNewCurrentAddRecord

		internal void PrepareForNewCurrentAddRecord()
		{
			this._currentAddRecord = null;

			// clear out the dictionary that caches the data for cells in an add record template before
			// AddNew is called on the IBindingList
			if (this._addRecordCellDataDictionary != null)
				this._addRecordCellDataDictionary.Clear();

			this.BumpUnderlyingDataVersion();

			// SSP 4/30/08 BR32427
			// We should do this at the same time that we reset the isAddRecord flag.
			// 
			// ------------------------------------------------------------------------
			



			if ( null != _sortedRecords )
				_sortedRecords.ViewableRecords.DirtySpecialRecords( true );
			// ------------------------------------------------------------------------

			//ViewableRecordCollection viewableRecords = this._sortedRecords.ViewableRecords;

			//if (viewableRecords != null)
			//{
			//    DataRecord newAddRecord = this.CurrentAddRecord;

			//    if ( newAddRecord != null )
			//        viewableRecords.OnNewAddRecord(newAddRecord);
			//}
		}

				#endregion //PrepareForNewCurrentAddRecord

				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
				#region ProcessQueuedChangeNotifications

		// SSP 12/21/11 TFS73767 - Optimizations
		// 
		//internal void ProcessQueuedChangeNotifications()
		internal void ProcessQueuedChangeNotifications( bool calledFromDelayedChange )
		{
			this._dataPresenter.VerifyAccess();

			// JJD 07/27/12 - TFS117958 
			// If we are already in this routine then bail. This can happen e.g. if a
			// data item sends out a change notification from inside a property getter. This
			// is not generally good practice but it can happen.
			if (_inProcessQueuedChangs)
				return;

			// JJD 07/27/12 - TFS117958 
			// Set anti-recursion flag 
			_inProcessQueuedChangs = true;

			// SSP 12/21/11 TFS73767 - Optimizations
			// Commented out the original code and added new code.
			// 
			// --------------------------------------------------------------------------------------------
			try
			{
				lock ( this._asyncChangeList.SyncRoot )
				{
					ArrayList list = _asyncChangeList;

					if ( null == _processAsyncChangeList )
					{
						_processAsyncChangeList = new List<ChangeNotification>( list.Count );
						_processAsyncChangeListSet = new HashSet( list.Count, 0.8f, new ChangeNotification.EqualityComparer( ) );
					}

					for ( int i = 0, count = list.Count; i < count; i++ )
					{
						ChangeNotification cn = list[i] as ChangeNotification;
						if ( !_processAsyncChangeListSet.Contains( cn ) )
						{
							_processAsyncChangeList.Add( cn );
							_processAsyncChangeListSet.Add( cn );
						}
					}

					list.Clear( );
				}

				if ( null != _processAsyncChangeList )
				{
					List<ChangeNotification> list = _processAsyncChangeList;

					DateTime operationStartTime = DateTime.Now;

					int lastProcessedIndex = -1;
					for ( int i = 0; i < list.Count; i++ )
					{
						ChangeNotification cn = list[i] as ChangeNotification;
						lastProcessedIndex = i;
						_processAsyncChangeListSet.Remove( cn );

						this.ProcessChangeNotification( cn.Sender, cn.EventArgs );

						// If the method was called from OnDelayedChange begin invoke callback then break up the work
						// in chunks to prevent ui thread from locking up since in this case the changes to the the data
						// source are being done through another thread.
						// 
						if ( calledFromDelayedChange && 0 == ( i % 50 ) && ( DateTime.Now - operationStartTime ).TotalMilliseconds > 50 )
							break;
					}
					
					// Remove processed items.
					// 
					list.RemoveRange( 0, 1 + lastProcessedIndex );
				}
			}
			finally
			{
				if ( null != _processAsyncChangeList && _processAsyncChangeList.Count > 0 
					|| GridUtilities.GetSyncLockedCount( _asyncChangeList ) > 0 )
					this._asyncChangePending = this._dataPresenter.Dispatcher.BeginInvoke( DispatcherPriority.Input, new GridUtilities.MethodDelegate( OnDelayedChange ) );
				else
					this._asyncChangePending = null;

				// JJD 07/27/12 - TFS117958 
				// Reset anti-recursion flag 
				_inProcessQueuedChangs = false;
			}
			
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------------
		}

				#endregion //ProcessQueuedChangeNotifications	
    
				#region RemoveAt

        internal void RemoveAt(int index)
        {
            this.RemoveAt(index, true);
        }
        private void RemoveAt(int index, bool notifyListeners )
        {
            DataRecord record = this._unsortedRecords.SparseArray.GetItem(index, false) as DataRecord;

            if (this._unsortedRecords.Count > 0)
            {
                if (notifyListeners)
                {
 					if (record != null)
						this._unsortedRecords.RemoveRecord(record);
					else
						this._unsortedRecords.RemoveAt(index);

                    this.RaisePropertyChangedEvent("Unsorted");
                }
                else
                    this._unsortedRecords.SparseArray.RemoveAt(index);
            }

			if (this._sortedRecords != null && this._sortedRecords.Count > 0)
			{
				if (record != null)
					this._sortedRecords.ViewableRecords.RemoveRecord(record, notifyListeners);
				else
					this._sortedRecords.ViewableRecords.RemoveRecordAtUnsortedIndex(index, notifyListeners);
			}

            if (this._groups != null && this._groups.Count > 0)
            {
                // JJD 2/25/08 - BR30660
                // Instead of clearing the groups which was just wrong
                // we should remove the specific record from the parent groupbyRecord's
                // child records ViewableRecords collection.
                // Note: if this is the last child record of the parent groupbyrecord
                // that that groupbyrecord will also get deleted.
                #region Old code

                //if (notifyListeners)
                //{
                //    this._groups.Clear();
                //    this.RaisePropertyChangedEvent("Groups");
                //}
                //else
                //    this._groups.SparseArray.Clear();

                #endregion //Old code	
    
                Debug.Assert(record != null);

                if (record != null)
                {
                    GroupByRecord parentGroupBy = record.ParentRecord as GroupByRecord;

                    Debug.Assert(parentGroupBy != null, "If we have groups then the record's parent must be a GroupByrecord");

                    if (parentGroupBy != null)
                        parentGroupBy.ChildRecords.ViewableRecords.RemoveRecord(record, notifyListeners);
                }
            }
        }

                #endregion //RemoveAt

				#region ReselectRecordAtSortedIndex

		internal void ReselectRecordAtSortedIndex(int index)
		{
            
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


            DataPresenterBase dp = this.DataPresenter;

            int count = this._sortedRecords.Count;

            Record candidateRecord = null;

            // JJD 05/06/10 - TFS27757
            // try to get a candidate for activation based on the passed in sort index
            if (index >= 0 && index < count)
                candidateRecord = this._sortedRecords[index];
            else if (count > 0)
                candidateRecord = this._sortedRecords[count - 1];

            // JJD 05/06/10 - TFS27757
            // if the record is enabled and is not filtered out then activate it and return
            if (candidateRecord != null &&
                candidateRecord.IsActivatable)
            {
                dp.ActiveRecord = candidateRecord;
                return;
            }

            if ( candidateRecord == null )
            {
                // JJD 05/06/10 - TFS27757
                // see if there are any sibling activatable special records
                ViewableRecordCollection vrc = this._sortedRecords.ViewableRecords;

                int vrcCount = vrc.Count;

                for (int i = vrcCount - 1; i >= 0; i--)
                {
                    candidateRecord = vrc[i];

                    if (candidateRecord.IsActivatable)
                    {
                        dp.ActiveRecord = candidateRecord;
                        return;
                    }
                }

                candidateRecord = null;
            }

            // JJD 05/06/10 - TFS27757
            // if we don't have a candidate record at this point then use the parent record
            if (candidateRecord == null)
                candidateRecord = this._parentRecord;

            if (candidateRecord != null)
            {
                // JJD 05/06/10 - TFS27757
                // Get the overall scroll position of this record.
                // Note: by passing in true for the first parameter a valid
                // scroll position willbe returned even if the record is filtered out
                bool dummy = false;
                int scrollpos = candidateRecord.GetOverallScrollPosition(true, ref dummy);

                // JJD 05/06/10 - TFS27757
                // First look over the available scroll positions looking for a data record
                // to activate.
                candidateRecord = GetActivatableRecordFromScrollPos(dp, scrollpos, true);

                if (candidateRecord != null && candidateRecord.IsActivatable)
                {
                    dp.ActiveRecord = candidateRecord;
                    return;
                }

                // JJD 05/06/10 - TFS27757
                // First look over the available scroll positions looking for any record
                // to activate.
                candidateRecord = GetActivatableRecordFromScrollPos(dp, scrollpos, false);

                if (candidateRecord != null && candidateRecord.IsActivatable)
                {
                    dp.ActiveRecord = candidateRecord;
                    return;
                }
            }
		}

				#endregion //ReselectRecordAtSortedIndex	

				#region SetAddRecordCellData

		internal void SetAddRecordCellData(Field field, object newValue)
		{
			// allocate the cache if necessary
			if (this._addRecordCellDataDictionary == null)
				this._addRecordCellDataDictionary = new HybridDictionary();

			// if the cache contains a value for this field then update
			// its value, otherwise add it to the cache
			if (this._addRecordCellDataDictionary.Contains(field))
				this._addRecordCellDataDictionary[field] = newValue;
			else
				this._addRecordCellDataDictionary.Add(field, newValue);

		}

				#endregion //SetAddRecordCellData	

                // JJD 12/08/08 - Added IsSynchronizedWithCurrentItem property
                #region SetActiveRecordFromCurrentItem

        private delegate bool BoolReturnMethodDelegate();

        internal void SetActiveRecordFromCurrentItem()
        {
            this.SetActiveRecordFromCurrentItem(false);
        }

        private void SetActiveRecordFromCurrentItem(bool calledFromCurrentItemChanged)
        {
            // if the datapresenter isn't loaded or we aren't bound to a collection view 
            // or IsSynchronizedWithCurrentItem is null then return.
            if (this._dataPresenter == null ||
                this._collectionView == null ||
                !this._dataPresenter.IsInitialized ||
                !this._dataPresenter.IsSynchronizedWithCurrentItem)
                return;

            // JJD 12/21/09 - TFS25835
            // Only return in nested managers if we weren't called from CurrentChanged event or this manager
            // is not in the active record chain
            //if (this._parentRecord != null)
            if (this._parentRecord != null &&
                (calledFromCurrentItemChanged == false || this.IsInActiveRecordChain == false))
               return;

            // check the anti-recursion flag
            if (this._isSynchronizingWithCurrentItem)
                return;

            // set the anti-recursion flag
            this._isSynchronizingWithCurrentItem = true;

            DataRecord dr = null;

            try
            {
                // If a reset is pending then process it now
                if (this._resetOperationPending != null)
                    this.OnSourceCollectionReset();

                if (!SetActiveRecordFromCurrentItemHelper(out dr))
                    return;
            }
            finally
            {
                // reset the anti-recursion flag
                this._isSynchronizingWithCurrentItem = false;
            }

            // If the we successfully synced up then bring the record into view
            // otherwise reset the current item asynchronously
            if (dr == this._dataPresenter.ActiveRecord ||
                (dr != null &&
                 this._dataPresenter.ActiveRecord != null &&
                 dr.IsAncestorOf(this._dataPresenter.ActiveRecord)))
            {
                if (dr != null)
                    this._dataPresenter.BringRecordIntoView(dr);
            }
            else
            {
                // If OnCurrentItemChanged is in the call stack then call
                // SetCurrentItemFromActiveRecord asynchronously.
                // Otherwise we can call it synchronously
                if (calledFromCurrentItemChanged)
                    this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Send, new BoolReturnMethodDelegate(this.SetCurrentItemFromActiveRecord));
                else
                    this.SetCurrentItemFromActiveRecord();
            }
        }

        private bool SetActiveRecordFromCurrentItemHelper(out DataRecord dr)
        {
            dr = null;

            int position = this.CollectionViewCurrentPosition;

            if (position < 0 || position >= this._unsortedRecords.Count)
            {
                DataRecord activeRcd = this._dataPresenter.ActiveRecord as DataRecord;

                if (activeRcd != null &&
                    !activeRcd.IsAddRecord)
                {
                    // JJD 12/21/09 - TFS25835
                    // For nested managers we should set the active record to the parent DataRecord
                    // instead of null
                    //this._dataPresenter.ActiveRecord = null;
                    this._dataPresenter.ActiveRecord = this._parentRecord != null ? this._parentRecord.ParentDataRecord : null;
                    return true;
                }
                else
                    return false;
            }
            else
            {
                Debug.Assert(position < this._unsortedRecords.Count, "Position should never be greater than or equal to the count of unsorted rcds");

                if (position > this._unsortedRecords.Count)
                    return false;

                dr = this._unsortedRecords[position];

                Debug.Assert(Object.Equals(dr.DataItem, this._collectionView.CurrentItem), "Current item doesn't match");

                Record currentActiveRecord = this._dataPresenter.ActiveRecord;

                if (currentActiveRecord == null ||
                     (dr != currentActiveRecord && !dr.IsAncestorOf(currentActiveRecord)))
                    this._dataPresenter.ActiveRecord = dr;

                return true;
            }
        }

                #endregion //SetActiveRecordFromCurrentItem	
    
                // JJD 12/08/08 - Added IsSynchronizedWithCurrentItem property
                #region SetCurrentItemFromActiveRecord

        
        
        
        internal bool SetCurrentItemFromActiveRecord()
        {
            // if the datapresenter isn't loaded or we aren't bound to a collection view 
            // or IsSynchronizedWithCurrentItem is null then return.
            if (this._dataPresenter == null ||
                this._collectionView == null ||
                !this._dataPresenter.IsInitialized ||
                !this._dataPresenter.IsSynchronizedWithCurrentItem)
                return true;

            
            
            
            
            

            // check the anti-recursion flag
            if (this._isSynchronizingWithCurrentItem)
                return true;

            // set the anti-recursion flag
            this._isSynchronizingWithCurrentItem = true;

            try
            {
                // JJD 12/21/09 - TFS25835 
                // Use the new ActiveDataRecordUnsortedIndex property instead
                // with supports synchronization of nested collectionviews
                //int index = this.RootActiveDataRecordUnsortedIndex;
                int index = this.ActiveDataRecordUnsortedIndex;

                if (this._collectionViewWrapper != null)
                    this._collectionViewWrapper.MoveCurrentToPosition(index);
                else
                    this._collectionView.MoveCurrentToPosition(index);

                if ( !this.IsInSyncWithCurrentPosition )
                {
                    DataRecord dummy;
                    this.SetActiveRecordFromCurrentItemHelper(out dummy);

                    // JJD 12/21/09 - TFS25835
                    // return false since the we had to re-sync the ActiveRecord
                    return false;
                }

                return true;

            }
            finally
            {
                // reset the anti-recursion flag
                this._isSynchronizingWithCurrentItem = false;
            }

        }

                #endregion //SetCurrentItemFromActiveRecord	

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
                #region TrackSortGroupVersionNumberChanges

        internal SortGroupVersionTracker TrackSortGroupVersionNumberChanges(FieldLayout fl)
        {
            SortGroupVersionTracker tracker;
            if (this._sortGroupByVersionTrackers.TryGetValue(fl, out tracker))
                return tracker;

            tracker = new SortGroupVersionTracker(this, fl);

            this._sortGroupByVersionTrackers.Add(fl, tracker);

			// JJD 10/18/10 - TFS30715
			// Only call BeginInvoke if one isn't already pending
			//if ( this._dataPresenter != null && fl.HasSortedFields )
            if ( this._trackSortGroupOperationPending == null &&
				this._dataPresenter != null && fl.HasSortedFields )
                this._trackSortGroupOperationPending = this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(this.OnSortGroupByVersionChanged));

            return tracker;
        }

                #endregion //TrackSortGroupVersionNumberChanges	
 
                // JJD 12/21/09 - TFS25835 - add
                #region UnwireCollectionViewEvents

        internal void UnwireCollectionViewEvents()
        {
            if (this._collectionView != null)
            {
                CurrentChangingEventManager.RemoveListener(this._collectionView, this);
                CurrentChangedEventManager.RemoveListener(this._collectionView, this);
            }
        }

                #endregion //UnwireCollectionViewEvents	
    
				#region UpdateRecordsWithPendingChanges

		internal void UpdateRecordsWithPendingChanges(bool discardChanges, bool recursive)
		{
			// first process the add record
			if (this._currentAddRecord != null &&
				this._currentAddRecord.DataItem != null)
			{
				this._currentAddRecord.ProcessPendingChanges(discardChanges, false);
			}

			// loop over all of the previously allocated records and if they
			// had their BeginEdit methods called without a subsequent commit or cancel
			// then call those methods now
			foreach (DataRecord record in this.Sorted.SparseArray.NonNullItems)
			{
				record.ProcessPendingChanges(discardChanges, recursive);
			}
		}

				#endregion //UpdateRecordsWithPendingChanges	

                #region VerifySort

		private void SortByFieldLayout( Record[] records, out int[] segments )
		{
			int jj = 0;

			List<int> segmentsList = new List<int>( 4 );

			while ( jj < records.Length )
			{
				FieldLayout fieldLayout = records[jj].FieldLayout;

				jj++;
				for ( int ii = jj; ii < records.Length; ii++ )
				{
					Record rii = records[ii];

					if ( fieldLayout == rii.FieldLayout )
					{
						if ( ii != jj )
						{
							records[ii] = records[jj];
							records[jj] = rii;
						}

						jj++;
					}
				}

				segmentsList.Add( jj );
			}

			segments = segmentsList.ToArray( );
		}

        
#region Infragistics Source Cleanup (Region)








































#endregion // Infragistics Source Cleanup (Region)

        // SSP 5/29/09 - TFS17233 - Optimization
		// Added areRecordsInUnsortedOrder parameter.
		// 
		//private void SortHelper( Record[] records, out bool hasMultipleFieldLayouts )
        // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
        // added hasGroupByFields param
        //private void SortHelper( Record[] records, out bool hasMultipleFieldLayouts, bool areRecordsInUnsortedOrder )
		private void SortHelper( Record[] records, bool hasGroupByFields, out bool hasMultipleFieldLayouts, bool areRecordsInUnsortedOrder )
		{
			int[] segments;
			this.SortByFieldLayout( records, out segments );

			hasMultipleFieldLayouts = segments.Length > 1;

			Record[] tmpArray = new Record[records.Length];

			for ( int i = 0; i < segments.Length; i++ )
			{
				int startIndex = i > 0 ? segments[i - 1] : 0;
				int endIndex = segments[i] - 1;

				
				
				
				
				FieldLayout fieldLayout = records[startIndex].FieldLayout;

				RecordManager.SameFieldRecordsSortComparer comparer = new RecordManager.SameFieldRecordsSortComparer(
							fieldLayout, 1 + endIndex - startIndex
							// SSP 5/29/09 - TFS17233 - Optimization
							// Added areRecordsInUnsortedOrder parameter.
							// 
							, areRecordsInUnsortedOrder
					);

				Utilities.SortMergeGeneric<Record>(
					records,
					tmpArray,
					comparer,
					startIndex,
					endIndex );
				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				
			}

            // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
            // determine if groupby records will be created
            bool willCreateGroupByRecords = (hasGroupByFields ||
                (hasMultipleFieldLayouts && this._dataPresenter.SortRecordsByDataType));

            // JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
            // we only want to move fixed records when we
            // aren't creating groupby rcds. Otherwise we
            // will affect the order of the groupbys
            if (willCreateGroupByRecords == false)
            {
                // JJD 8/27/09 - TFS21513
                // Moved logic to GridUtilities
                //this.MoveFixedRecords(records);
                GridUtilities.MoveFixedRecords(records);

				// JJD 6/22/09 - NA 2009 Vol 2 - Record Fixing
				// If the FixedRecordSortOrder resolves to 'Sorted' then
				// have the ViewableRecordCollection refresh the sort order of its
				// cached fixed records 
				if (this._fieldLayout != null &&
					this._fieldLayout.FixedRecordSortOrderResolved == FixedRecordSortOrder.Sorted)
				{
					ViewableRecordCollection vrc = this.ViewableRecords;
					if (vrc != null)
						vrc.RefreshSortOrderOfFixedRecords();
				}
            }
		}

		internal delegate bool VerifySortDelegate();

		internal bool VerifySort( )
		{

            // JJD 1/29/09
            // Check flag to prvent recursion
            if (this.IsInVerifySort)
                return false;

			if (this._field != null &&
				!this._field.IsExpandableByDefault )
				return false;

			// JJD 1/31/11 - TFS63999
			// Added flag to allow us to sort the records without re-crating the groups
			bool bypassGroupCreation = false;

			bool isGroupBySupported = false;

			// check to see if groupby is supported by the view
			if ( this.DataPresenter != null )
			{
				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
				// by processing any pending change notifications
				
				
				
				
				this.ProcessQueuedChangeNotifications( false );

				ViewBase view = this.DataPresenter.CurrentViewInternal;
				isGroupBySupported = view != null && view.IsGroupBySupported;
			}

			// see if the view's support for groupby changed and
			// set theb dirty flag if we need to reprocess the list
			if ( this._isGroupBySupported != isGroupBySupported )
			{
				if ( isGroupBySupported == false )
				{
					if ( this.HasGroups )
						this._unsortedRecordsDirty = true;
				}
				else
					this._unsortedRecordsDirty = true;

				// set the member variable for the groupby
				this._isGroupBySupported = isGroupBySupported;
			}

            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
            // Since we are supporting multiple fieldlayout (heterogeneous data) call
            // the GetSortGroupByVersionStatus method to determine is any field layouts
            // require sorting or grouping
            SortGroupVersionStatus sortGroupStatus = this.GetSortGroupByVersionStatus();

			if ( this._unsortedRecordsDirty == true )
				this._unsortedRecordsDirty = false;
			else
			{
				if ( this._fieldLayout != null )
				{
                    // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                    // If both the sort ad groupby versions haven't changed then
                    // we are done.
                    
                    
                    
                    if (sortGroupStatus.IsSortVersionChanged == false &&
						sortGroupStatus.IsGroupByVersionChanged == false )
						return false;

					if ( this.HasGroups )
					{
                        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                        // If the groupby version hasn't changed then we can just verify
                        // the sort order of the groups instead of recreating them
                        
                        
                        
                        if (sortGroupStatus.IsGroupByVersionChanged == false)
						{
                            // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                            // sync up the sortgroupy version number
							
							this.SyncSortGroupByVersions();

							if ( this._parentRecord == null ||
								 this._parentRecord.IsExpanded )
							{
								this.Groups.VerifySortOrder( true );
							}
							// JJD 1/31/11 - TFS63999
							// Instead of returning set a flag so we know not to re-create the groups
							// and fall thru so that we end up sorting the records so their order is in sync
							// with their order in the groups
							//return false;
							bypassGroupCreation = true;
						}
					}
				}
			}

			//// clear the old sorted collection
			//if (this._sortedRecords != null)
			//    this._sortedRecords.SparseArray.Clear();

			// allocate an array to hold the sorted items
			//DataRecord[] itemsToSort = new DataRecord[this._unsortedRecords.Count];

			this._hasMultipleFieldLayouts = false;

			// JM/JD 06-13-08 BR33852
			bool wasSorted = this._isSorted;

            // JJD 12/02/08 - TFS6743/BR35763
            // Set the IsInVerifySort flag to true so we know to bypass certain
            // sort related logic until we finish sorting all the records
            this.IsInVerifySort = true;

			// [BR20825] 3/1/07
            // JJD 12/02/08 - TFS6743/BR35763
            // Moved below since we don't want to reset if
            // before we trigger the creation of the 1st record below
			//this._isSorted = false;

			bool clearGroups = true;

            if (this._sortedRecords.Count > 0)
            {
                // get the fieldlayout of the 1st record in the unsorted list
                FieldLayout fl = this._unsortedRecords[0].FieldLayout;

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                // Re-get the sortGroupStatus since we may have added an entry
                // to the sortgroupversiontracker dictionary by accessing the first
                // record above
                sortGroupStatus = this.GetSortGroupByVersionStatus();

                // check it is different from the last one
                
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


                // JJD 12/02/08 - TFS6743/BR35763
                // Moved from above since we don't want to reset if
                // before we trigger the creation of the 1st record above
                this._isSorted = false;

                this.InitializeFieldLayout(fl);

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                
                
                bool sortRecords = this.DataPresenter.SortRecordsByDataType
                                    || sortGroupStatus.HasSortFields;

                if (sortRecords)
                {
					// SSP 2/14/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// Added the if block and enclosed the existing code into the else-if block.
					// 
					// SortViaCollectionViewHelper returns true if the sorting was done using
					// collection view.
					// 
					if ( this.SortViaCollectionViewHelper( ) )
					{
					}
					else if ( SortEvaluationMode.Auto == this.SortEvaluationModeResolved )
					{
						this._isSorted = true;

						// JJD 1/29/07 - BR19570
						// Don't clear the cached records on top until we allocate
						// all the null slots below
						//// clear the cached records that were inserted at the top
						//this.ClearCachedRecordsOnTop();

						// JJD 5/19/07
						// Optimization - Wrap possibly creating multiple items in begin/endupdate calls
						this._sortedRecords.BeginUpdate( );

						bool hasMultipleFieldLayouts;

						try
						{
							// first get the array to force any null slots to be allocated 
							// JJD 12/08/08 - TFS8630
							// Use the unsortedrecords to get the list since then is no possibility
							// of having gaps. Using the sorted sparsearray there is a possibility
							// of gaps if the sortedindexes aren't mapped correctly
							//Record[] sortedItems = this._sortedRecords.SparseArray.ToArray(true);
							Record[] sortedItems = this._unsortedRecords.SparseArray.ToArray( true );

							// SSP 5/29/07 - Optimization
							// 
							
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

							// SSP 5/29/09 - TFS17233 - Optimization
							// 
							//this.SortHelper(sortedItems, out hasMultipleFieldLayouts);
							// JJD 8/25/09 - NA 2009 Vol 2 - Record fixing
							// Added hasGroupByFields param
							//this.SortHelper( sortedItems, out hasMultipleFieldLayouts, true );
							this.SortHelper( sortedItems, sortGroupStatus.HasGroupByFields, out hasMultipleFieldLayouts, true );

							this._sortedRecords.SparseArray.Clear( );
							this._sortedRecords.SparseArray.AddRange( sortedItems );

							// JJD 11/22/11 - TFS96310
							// Call VerifyFixedRecordLists which will make sure the lists of fixed records that the
							// vrc maintains pick up any records that were fixed from the group islands.
							if ( false == sortGroupStatus.HasGroupByFields )
								this._sortedRecords.ViewableRecords.VerifyFixedRecordLists( );
						}
						finally
						{
							// SSP 2/11/09 TFS12467
							// No need to pass true for dirtyScrollCount because any modifications done to
							// the sparse array, like the Clear and AddRange calls above, will automatically 
							// dirty the scroll count.
							// 
							//this._sortedRecords.EndUpdate(true);
							this._sortedRecords.EndUpdate( false );
						}

						// SSP 5/29/07 - Optimization
						// In case the top and bottom fixed rows use the same field layout, there is no
						// guarrentee that intervening rows will not use different field layouts.
						// 
						//this._hasMultipleFieldLayouts = this._sortedRecords[0].FieldLayout != this._sortedRecords[this._sortedRecords.Count - 1].FieldLayout;
						this._hasMultipleFieldLayouts = hasMultipleFieldLayouts;
					}					

                    // JJD 5/19/09 - NA 2009 vol 2 - Cross band grouping 
                    // Re-get the sortGroupStatus since we may have added entries
                    // to the sortgroupversiontracker sorting all the records
                    sortGroupStatus = this.GetSortGroupByVersionStatus();
                }
                else
                {
                    // JJD 2/23/07 
                    // If the records had been sorted then sort them back
                    // JM/JD 06-13-08 BR33852 - Check the new 'wasSorted' flag since this.isSorted is getting
                    // reset above
                    //if ( this._isSorted &&
                    //     this._sortedRecords.SparseArray.Count > 1 )
                    if (wasSorted && this._sortedRecords.SparseArray.Count > 1)
                    {
                        // JM/JD 06-13-08 - Optimization: do Begin/End Update so we do not synchronously process notifications.
                        this._sortedRecords.BeginUpdate();

                        try
                        {
                            // sort the sparse array
                            this._sortedRecords.SparseArray.Sort(new RecordsSortComparer(true));
                        }
                        finally
                        {
                            // JJD 8/27/09 - TFS21513
                            // Dirty the FixedRecordOrder so it will get verified the next time
                            // someone accesses the collection
							// JJD 04/13/12 - TFS104021
							// Only dirty the fixed rcd order if FixedRecordSortOrderResolved is 'Sorted'
							if (this._fieldLayout != null &&
								this._fieldLayout.FixedRecordSortOrderResolved == FixedRecordSortOrder.Sorted)
								this._sortedRecords.ViewableRecords.DirtyFixedRecordOrder();

							// SSP 2/11/09 TFS12467
							// No need to pass true for dirtyScrollCount because any modifications done to
							// the sparse array, like the Sort call above, will automatically dirty the 
							// scroll count.
							// 
                            //this._sortedRecords.EndUpdate(true);
							this._sortedRecords.EndUpdate( false );
                        }
                    }

                    this._isSorted = false;
                }

				// JJD 1/31/11 - TFS63999
				// Check the bypassGroupCreation flag. If it was set to true above that means that the 
				// existing groups don't need to be re-created
				if (bypassGroupCreation)
					clearGroups = false;
				else
				if (this._isGroupBySupported == true)
                {
                    
                    
                    
                    if ((this._hasMultipleFieldLayouts && this.DataPresenter.SortRecordsByDataType) ||
                        sortGroupStatus.HasGroupByFields)
                    {
						// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
						// Added the if block and enclosed the existing code into the else block.
						// 
						if ( GroupByEvaluationMode.UseCollectionView == this.GroupByEvaluationModeResolved && sortGroupStatus.HasGroupByFields )
						{
							this.SortViaCollectionViewHelper( );
						}
						else
						{
							// SSP 6/14/12 TFS113458 - External Sorting
							// Now with external sorting functionality all the records do not necessarily have to be allocated.
							// This means that we have to ensure they are allocated here.
							// 
							// --------------------------------------------------------------------------------------------------
							//this.Groups.CreateGroupByRecordsHelper( this._sortedRecords.SparseArray.ToArray( typeof( DataRecord ) ) as DataRecord[] );
							DataRecord[] recordsToGroup = this._sortedRecords.SparseArray.ToArray( true, typeof( DataRecord ) ) as DataRecord[];

							// If we did not internally sort the records ourselves (sort evaluation mode is either Manual or UseCollectionView)
							// then make sure we sort the records which may require group-by specific comparison logic to group correctly.
							// 
							if ( !_isSorted )
							{
								bool discard;
								this.SortHelper( recordsToGroup, true, out discard, ! wasSorted );
							}

							this.Groups.CreateGroupByRecordsHelper( recordsToGroup );
							// --------------------------------------------------------------------------------------------------

							this.Groups.RaiseChangeEvents( true );
						}

                        clearGroups = false;
                    }
                }

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping 
                // sync up the sortgroupy version number
                
                
                // JJD 11/10/09 - TFS24611
                // Moved (see note below)
                //this.SyncSortGroupByVersions();
            }
            else
            {
                // JJD 12/02/08 - TFS6743/BR35763
                // Moved from above (refer to above comments)
                this._isSorted = false;
            }

            // JJD 11/10/09 - TFS24611
            // Moved call from above so we sync the version numbers even in the case
            // where we have no records
            this.SyncSortGroupByVersions();

			if ( clearGroups )
			{
				if ( this._groups != null && this._groups.Count > 0 )
				{
					// clear the old groupby parent settings on every record
					foreach ( Record record in this._sortedRecords )
						record.InitializeParentCollection( this._sortedRecords );

					this._groups.Clear( );
					this._groups.RaiseChangeEvents( true );
				}
			}

            // JJD 12/02/08 - TFS6743/BR35763
            // Reset the IsInVerifySort flag to re-enable 
            // sort related logic 
            this.IsInVerifySort = false;

            // JJD 5/27/09 - TFS17889
            // Dirty the special records so they will get verified
            // asynchronously
            ViewableRecordCollection vrc = this.ViewableRecords;
            if (vrc != null)
                vrc.DirtySpecialRecords(false);


			// SSP 6/27/08 BR33815
			// When records are sorted we need to recalculate summaries with calculators
			// that are affected by sort.
			// 
			SummaryResultCollection summaryResults = null != _sortedRecords ? _sortedRecords.SummaryResultsIfAllocated : null;
			if ( null != summaryResults )
				summaryResults.RefreshSummariesAffectedBySort( );

			this.RaisePropertyChangedEvent( "Current" );
			this.RaisePropertyChangedEvent( "Sorted" );

			if ( this._parentRecord != null )
				this._parentRecord.OnViewableChildRecordsChanged( );

			if ( this._sortedRecords != null )
				this._sortedRecords.RaiseChangeEvents( true );

			// JM 11-13-07 BR27986
			// JJD 6/28/11 - TFS79556 - Optimization
			// If the _isInitializingDataSource is true then just set it to false
			// otherwise bump the SortOperationVersion
			//if (this._fieldLayout != null)
			//    this._fieldLayout.BumpSortOperationVersion();
			if (this._isInitializingDataSource)
			{
				this._isInitializingDataSource = false;
			}
			else
			{
				if (this._fieldLayout != null)
					this._fieldLayout.BumpSortOperationVersion();
			}

			return true;

		}

                #endregion //VerifySort	
 
                // JJD 12/21/09 - TFS25835 - add
                #region WireCollectionViewEvents

        internal void WireCollectionViewEvents()
        {
            if (this._collectionView != null)
            {
                CurrentChangingEventManager.AddListener(this._collectionView, this);
                CurrentChangedEventManager.AddListener(this._collectionView, this);
            }
        }

                #endregion //WireCollectionViewEvents	
    
            #endregion //Internal Methods

            #region Private Methods

				#region BumpUnderlyingDataVersion

		private void BumpUnderlyingDataVersion()
		{
			this._underlyingDataVersion++;
			base.RaisePropertyChangedEvent("UnderlyingDataVersion");

			
			
			
			
			
			
			
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		}

				#endregion //BumpUnderlyingDataVersion	

                // JJD 5/6/09 - NA 2009 vol 2 - Cross band grouping
                #region CleanupUnusedSortGroupByVersionTrackers

        private void CleanupUnusedSortGroupByVersionTrackers()
        {
			// JJD 10/18/10 - TFS30715
			// Clear the pending operation member
			this._cleanupTrackersOperationPending = null;

			// If we don't have any records then clear all trackers and return
            if (this._unsortedRecords.Count == 0)
            {
                this._sortGroupByVersionTrackers.Clear();
                return;
            }

			// JJD 4/25/11 - TFS67129
			// get the current sort/group statuc
			SortGroupVersionStatus currentSortGroupStatus = this.GetSortGroupByVersionStatus();

            int trackerCount = this._sortGroupByVersionTrackers.Count;

            Dictionary<FieldLayout, FieldLayout> fieldLayoutsFound = new Dictionary<FieldLayout, FieldLayout>();
            FieldLayout lastFieldLayout = null;

            // walk over all the records keeping track of all fieldlayouts encountered
            foreach (Record rcd in this._unsortedRecords.SparseArray.NonNullItems)
            {
                FieldLayout fl = rcd.FieldLayout;

                if (fl == lastFieldLayout)
                    continue;

                // Since there needs to be at least one tracker. If we have only one
                // we can return
                if (trackerCount == 1)
                {
                    if (this._sortGroupByVersionTrackers.ContainsKey(fl))
                        return;

                    Debug.Fail("the single tracker's fieldlayout should match in RecordManager.CleanupUnusedSortGroupByVersionTrackers");
                }

                lastFieldLayout = fl;

				if (!fieldLayoutsFound.ContainsKey(fl))
					fieldLayoutsFound.Add(fl, fl);
            }

            List<FieldLayout> tempList = new List<FieldLayout>(this._sortGroupByVersionTrackers.Keys);

			// JJD 4/25/11 - TFS67129
			// Keep track if we end up removing any trackers
			bool wereTrackersRemoved = false;

            foreach (FieldLayout fl in tempList)
            {
				if (!fieldLayoutsFound.ContainsKey(fl))
				{
					this._sortGroupByVersionTrackers.Remove(fl);

					// JJD 4/25/11 - TFS67129
					// Keep track if we end up removing any trackers
					wereTrackersRemoved = true;
				}
            }

			// JJD 4/25/11 - TFS67129
			// If we removed any trackers and there were groupby fields
			// then dirty the version numbers and call VerifySort again
			if (wereTrackersRemoved && currentSortGroupStatus.HasGroupByFields)
			{
				if (this._sortGroupByVersionTrackers.Count > 0)
				{
					foreach (SortGroupVersionTracker tracker in _sortGroupByVersionTrackers.Values)
						tracker.DirtyVersionNumbers();
				}

				this.VerifySort();
			}

            
            
        }

                #endregion //CleanupUnusedSortGroupByVersionTrackers	
    
                // JJD 11/17/08 - TFS6743/BR35763 - added
                #region ClearAddRecordOnDataSourceChange

        private void ClearAddRecordOnDataSourceChange()
        {
            // JJD 2/28/07 - BR20116
            // clear the current add record.
            // Note: it will get recreated the next time someone accesses the CurrentAddRecord property
            if (this._currentAddRecord != null)
            {
                ViewableRecordCollection viewableRecords = this._sortedRecords.ViewableRecords;

                DataRecord holdAddRecord = this._currentAddRecord;

                // clear the member 
                this._currentAddRecord = null;

                // let the ViewableRecordCollection know but suppress the notifications
                if (viewableRecords != null)
                {
					// SSP 4/30/08 BR32427
					// 
					//viewableRecords.OnAddRecordRemoved( holdAddRecord, false );
					viewableRecords.DirtySpecialRecords( false );
                }
            }
        }

                #endregion //ClearAddRecordOnDataSourceChange	
    
				#region ClearCachedRecordsOnTop

		private void ClearCachedRecordsOnTop()
		{
			ViewableRecordCollection vrc = this._sortedRecords.ViewableRecords;

			if ( vrc != null )
				vrc.ClearCachedRecordsOnTop();
		}

				#endregion //ClearCachedRecordsOnTop	

				// JJD 05/08/12 - TFS110865 - added
				#region DoesTypeSupportEditInList

		private bool DoesTypeSupportEditInList()
		{
			if (_typeIsKnownType)
			{
				if (_list == null || _list.IsReadOnly)
					return false;
			}
			return true;
		}

				#endregion //DoesTypeSupportEditInList	
    
                // JJD 05/06/10 - TFS27757 - added
                #region GetActivatableRecordFromScrollPos

        private static Record GetActivatableRecordFromScrollPos(DataPresenterBase dp, int scrollpos, bool dataRecordsOnly)
        {
            IViewPanelInfo info = dp as IViewPanelInfo;

            Record candidateRecord = null;

            if (scrollpos >= 0)
            {
                // JJD 05/06/10 - TFS27757
                // Loop over the scrollpositions from the passed in pos to the end of the list
                // looking for a valid candidate
                for (int i = scrollpos; i < info.OverallScrollCount; i++)
                {
                    // JJD 05/06/10 - TFS27757
                    // Get the record at this scroll position 
                    candidateRecord = info.GetRecordAtOverallScrollPosition(i);

                    if (candidateRecord != null)
                    {
                        // JJD 05/06/10 - TFS27757
                        // if dataRecordsOnly is true then ignore any other record
                        if (dataRecordsOnly == false || candidateRecord.RecordType == RecordType.DataRecord)
                        {
                            if (candidateRecord.IsActivatable)
                                return candidateRecord;
                        }
                    }
                }
            }

            scrollpos--;

            if (scrollpos >= 0)
            {
                // JJD 05/06/10 - TFS27757
                // Loop over the scrollpositions decrementing the position from the passed in pos to the beginning
                // looking for a valid candidate
                for (int i = scrollpos; i >= 0; i--)
                {
                    // JJD 05/06/10 - TFS27757
                    // Get the record at this scroll position 
                    candidateRecord = info.GetRecordAtOverallScrollPosition(i);

                    if (candidateRecord != null)
                    {
                        // JJD 05/06/10 - TFS27757
                        // if dataRecordsOnly is true then ignore any other record
                        if (dataRecordsOnly == false || candidateRecord.RecordType == RecordType.DataRecord)
                        {
                            if (candidateRecord.IsActivatable)
                                return candidateRecord;
                        }
                    }
                }
            }

            return null;
        }

                #endregion //GetActivatableRecordFromScrollPos	
        
				#region GetPropertyDescriptorProvider

		private FieldLayout.LayoutPropertyDescriptorProvider GetPropertyDescriptorProvider()
		{
			// see if any DataRecords have been loaded
			if (this._unsortedRecords != null &&
				 this._unsortedRecords.Count > 0)
			{
				DataRecord rcd = null;

				// get the first non null item
				foreach (object obj in this._unsortedRecords.SparseArray.NonNullItems)
				{
					rcd = obj as DataRecord;
					Debug.Assert(rcd != null);

					// we only need one so break
					break;
				}

				if (rcd != null)
					return rcd.PropertyDescriptorProvider;
			}

			// JJD 11/24/10 - TFS59031
			// Return the cached default provider
			//return null;
			return _typedListDefaultProvider;
		}

				#endregion //GetPropertyDescriptorProvider	

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
                #region GetSortGroupByVersionStatus

        private SortGroupVersionStatus GetSortGroupByVersionStatus()
        {
            bool isSortVersionChanged = false;
            bool isGroupByVersionChanged = false;
            bool hasSortFields = false;
            bool hasGroupByFields = false;

            if (this._sortGroupByVersionTrackers != null &&
                 this._sortGroupByVersionTrackers.Count > 0)
            {
                foreach (SortGroupVersionTracker tracker in this._sortGroupByVersionTrackers.Values)
                {
                    if (tracker == null)
                        continue;

                    FieldLayout fl = tracker.FieldLayout;

                    if (isSortVersionChanged == false &&
                         tracker.SortVersion != fl.SortVersion)
                        isSortVersionChanged = true;

                    if (isGroupByVersionChanged == false &&
                         tracker.GroupByVersion != fl.GroupByVersion)
                        isGroupByVersionChanged = true;

                    if (hasSortFields == false &&
                         fl.HasSortedFields)
                        hasSortFields = true;

                    if (hasGroupByFields == false &&
                         fl.HasGroupBySortFields)
                        hasGroupByFields = true;
                }
            }

            return new SortGroupVersionStatus(isSortVersionChanged, isGroupByVersionChanged, hasSortFields, hasGroupByFields);
        }

                #endregion //GetSortGroupByVersionStatus	
    
				// JJD 10/19/07 - BR26277
				// Moved this logic to DataBindingUtilities.GetUnderlyingItemSource.
				#region Old code

		// JJD 5/8/07
		// Added routine to compare underlying data source
		//        #region GetUnderlyingSource

		//private IEnumerable GetUnderlyingSource(IEnumerable source)
		//{

		//    // JJD 8/1/07 - BR25196
		//    //if (source is ICollectionView)
		//    //    return ((ICollectionView)source).SourceCollection;
		//    //
		//    //return source;

		//    IEnumerable underlyingSource;

		//    if (source is ICollectionView)
		//        underlyingSource = ((ICollectionView)source).SourceCollection;
		//    else
		//        underlyingSource = source;

		//    // JJD 8/1/07 - BR25196
		//    // If the underlying source implements IListSource the get the list
		//    // thru its GetList method
		//    IListSource listSource = underlyingSource as IListSource;

		//    if (listSource != null)
		//        underlyingSource = listSource.GetList();

		//    return underlyingSource;
		//}

		//        #endregion //GetUnderlyingSource	

				#endregion //Old code	
            
				// AS 3/22/07 RecordCollection FieldLayout
				// While implementing AutoFit changes, we noticed that the FieldLayout
				// of the RecordCollection was not getting initialized to the field layout
				// since VerifySort was also setting the _fieldLayout member without 
				// hooking the PropertyChanged and without calling InitializeFieldLayout.
				// This routine was moved from VerifySort.
				//
				#region InitializeFieldLayout
		private void InitializeFieldLayout(FieldLayout fl)
		{
			if (fl != this._fieldLayout)
			{
				// JJD 1/29/07 - BR19605
				// Added logic to refresh the sort on nested bands by
				// listening to the property change notifications of the
				// associated field layout
				//
				// unhook from the property change of the old fl
				// JM 05-06-10 TFS31643 - Use a WeakEventListener to avoid rooting the RecordManager 
				if (this._fieldLayout != null)
					//this._fieldLayout.PropertyChanged -= new PropertyChangedEventHandler(this.OnFieldLayoutPropertyChanged);
					PropertyChangedEventManager.RemoveListener(this.FieldLayout, this, string.Empty);

				this._fieldLayout = fl;

				// hook the property change of the new fl
				// JM 05-06-10 TFS31643 - Use a WeakEventListener to avoid rooting the RecordManager 
				if (this._fieldLayout != null)
					//this._fieldLayout.PropertyChanged += new PropertyChangedEventHandler(this.OnFieldLayoutPropertyChanged);
					PropertyChangedEventManager.AddListener(this.FieldLayout, this, string.Empty);

				this._unsortedRecords.InitializeFieldLayout(this._fieldLayout);
				this._sortedRecords.InitializeFieldLayout(this._fieldLayout);

				// SSP 1/21/09 - NAS9.1 Record Filtering
				// Also initialize the group collection's field layout to the new field layout.
				// 
				if ( null != _groups )
					_groups.InitializeFieldLayout( _fieldLayout );

                // JJD 2/12/09 - TFS13615
                // Dirty the special rcds because we could have an add record now
                ViewableRecordCollection vrc = this.ViewableRecords;
                if (vrc != null)
                    vrc.DirtySpecialRecords(false);

				// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
				// 
				this.SortViaCollectionViewHelper( );

				// SSP 12/12/08 - NAS9.1 Record Filtering
				// Resolved record filter collection keeps track of record filters of potentially multiple
				// field layouts, if the record manager contains records from different field layouts. 
				// Therefore we need to let the resolved record filter collection know that a new field
				// layout is associated with the record manager.
				// 
				if ( null != _recordFiltersResolved && null != _fieldLayout )
					_recordFiltersResolved.OnFieldLayoutAdded( _fieldLayout );
			}
		} 
				#endregion //InitializeFieldLayout

                // JJD 4/3/08 - added support for printing
                #region MapAssociatedGroupbyRecords


            private void MapAssociatedGroupByRecords(RecordCollectionBase collection, RecordCollectionBase associatedCollection)
        {
            if (collection == null ||
                collection.Count == 0 ||
                associatedCollection == null ||
                associatedCollection.Count == 0)
                return;

            // MBS 9/17/09
            //DataPresenterReportControl rc = this.DataPresenter as DataPresenterReportControl;
            DataPresenterExportControlBase rc = this.DataPresenter as DataPresenterExportControlBase;

            if (collection.GroupByField != null)
            {
                if (associatedCollection.GroupByField == null)
                    return;

                // make sure the field match
                if (collection.GroupByField.Name != associatedCollection.GroupByField.Name)
                    return;

                // MBS 9/17/09
                // Moved above
                //
                //// MBS 7/31/09 - NA9.2 Excel Exporting
                ////DataPresenterReportControl rc = this.DataPresenter as DataPresenterReportControl;
                //DataPresenterExportControlBase rc = this.DataPresenter as DataPresenterExportControlBase;

                Debug.Assert(null != rc);

                if (null != rc && rc.GetClonedField(associatedCollection.GroupByField) != collection.GroupByField)
                    return;
            }
            else
            {
                if (associatedCollection.GroupByField != null)
                    return;

                if (!(collection[0] is GroupByRecord) ||
                    !(associatedCollection[0] is GroupByRecord))
                    return;
            }

            // populate a temporary cache keyed by value
            Dictionary<object, GroupByRecord> existingGroupByRcds = new Dictionary<object, GroupByRecord>(collection.Count);
            foreach (Record rcd in collection)
            {
                GroupByRecord gbr = rcd as GroupByRecord;

                if (gbr != null)
                     existingGroupByRcds[RecordCollectionBase.GetValueKey(gbr.Value)] = gbr;
            }

            Dictionary<GroupByRecord, GroupByRecord> rcdsToMap = new Dictionary<GroupByRecord, GroupByRecord>(collection.Count + 10);

            // walk over associated collection and look for value
            // matches
            foreach (Record rcd in associatedCollection)
            {
                GroupByRecord associatedGbr = rcd as GroupByRecord;

                if (associatedGbr != null)
                {
                    object valueKey = RecordCollectionBase.GetValueKey(associatedGbr.Value);

                    // MBS 9/17/09
                    // Handle the case where we have heterogeneous data and may have created
                    // the FieldLayout during grouping
                    if (associatedGbr.RecordType == RecordType.GroupByFieldLayout)
                    {
                        FieldLayout layout = valueKey as FieldLayout;
                        if (layout != null)
                        {
                            if (rc != null)
                            {
                                valueKey = rc.GetClonedFieldLayout(layout);
                            }
                        }
                        else
                            Debug.Fail("Expected the value key to be a FieldLayout");
                    }

                    GroupByRecord gbr;

                    if (existingGroupByRcds.TryGetValue(valueKey, out gbr))
                    {
                        rcdsToMap.Add(gbr, associatedGbr);
                    }
                }
            }

            // see if we need to walk down to nested groupbyrcds
            bool hasNestedGroups =  RecordCollectionBase.HasAdditionalGroupByFields(collection.FieldLayout, collection.GroupByField) &&
                                    RecordCollectionBase.HasAdditionalGroupByFields(associatedCollection.FieldLayout, associatedCollection.GroupByField);


            foreach (KeyValuePair<GroupByRecord, GroupByRecord> entry in rcdsToMap)
            {
                // update the main cache
                this._associatedRecordMap.Add(entry.Key, entry.Value);

                // call this method recursively for nested groups
                if ( hasNestedGroups )
                    this.MapAssociatedGroupByRecords(entry.Key.ChildRecords, entry.Value.ChildRecords);
            }
        }

                #endregion //MapAssociatedGroupbyRecords

				#region NotifyCalcAdapterHelper

			// SSP 9/11/11 Calc
			// 
			internal void NotifyCalcAdapterHelper( DataChangeType dataChangeType, DataRecord recordContext, Field fieldContext )
			{

				DataPresenterBase dp = this._dataPresenter;
				Infragistics.Windows.DataPresenter.Calculations.IDataPresenterCalculationAdapterInternal adapter 
					= null != dp ? dp._calculationAdapter : null;
				
				if ( null != adapter )
				{
					switch ( dataChangeType )
					{
						case DataChangeType.CellDataChange:
							adapter.OnPropertyValueChanged( recordContext, "CellValue", fieldContext );
							break;
						case DataChangeType.RecordDataChange:
							adapter.OnPropertyValueChanged( recordContext, "Record", null );
							break;
						case DataChangeType.AddRecord:
							adapter.OnPropertyValueChanged( recordContext, "Add", null );
							break;
						case DataChangeType.RemoveRecord:
							adapter.OnPropertyValueChanged( recordContext, "Remove", null );
							break;
						case DataChangeType.ItemMoved:
							adapter.OnPropertyValueChanged( this.Current, "ItemMoved", null );
							break;
						case DataChangeType.Reset:
							adapter.OnPropertyValueChanged( this.Current, "Reset", null );
							break;
					}
				}

			}

				#endregion // NotifyCalcAdapterHelper

                #region NotifyScrollCountChanged

		// SSP 2/11/09 TFS12467
		// Re-worked the way we manage scroll counts. Commented out the following method.
		// 
		
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				#endregion //NotifyScrollCountChanged	
        
                #region OnBindingListChanged

        private void OnBindingListChanged(object sender, ListChangedEventArgs e)
        {
            IList list = sender as IList;

            Debug.Assert(list != null 
						&& (list == this._sourceItems || list == this._bindingList ));

            if (list != this._sourceItems &&
				list != this._bindingList)
                return;

            switch (e.ListChangedType)
            {
				case ListChangedType.ItemAdded:
					if (this.IsResetPending)
						this.OnSourceCollectionReset();
					else
						this.OnSourceCollectionAddOrRemove(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, list[e.NewIndex], e.NewIndex));
					break;
				case ListChangedType.ItemDeleted:
					if (this.IsResetPending)
						this.OnSourceCollectionReset();
					else
						this.OnSourceCollectionAddOrRemove(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, this._unsortedRecords.SparseArray.GetItem(e.NewIndex, false), e.NewIndex));
					break;

                case ListChangedType.ItemChanged:
					{
						// SSP 1/2/07 BR29133
						// We should be using the NewIndex instead of the OldIndex in ItemChanged.
						// 
						//int index = e.OldIndex;
						int index = e.NewIndex;

						DataRecord dr = null;

						Debug.Assert(index >= 0 && index < this._unsortedRecords.Count, "Invalid ItemChanged index");

						// get the record but only if it was allocated already
						if (index >= 0 && index < this._unsortedRecords.Count)
							dr = this._unsortedRecords.SparseArray.GetItem(index, false) as DataRecord;

						// if null that means the record was never accessed so we cane 
						// ignore the notification
						if (dr == null)
							break;
						
						// if the PropertyDescriptor argumnet is null that means
						// that more than one cell was updated so we need to refresh each 
						// one
						if (e.PropertyDescriptor == null)
						{
							Debug.Assert(list != null && list.Count > index);

							if ( list != null && list.Count > index )
								dr.InitializeDataItem(list[index], index);

							// SSP 3/3/09 TFS11407
							// Added raiseInitializeRecord parameter.
							// 
							//dr.RefreshCellValues();
							dr.RefreshCellValues( true, true );

							// SSP 2/3/10 TFS26323
							// Dirty any affected summaries as well as filter states.
							// 
							this.OnDataChanged( DataChangeType.RecordDataChange, dr, null );

							break;
						}

						// JJD 5/9/07 - BR22698
						// Added support for listening to data item's PropertyChanged event
						// if the datasource doesn't supply cell value change notifications thru IBindlingList
						//
						// Centralized logic for refreshing a cell's value on a change notification in 
						// DataRecord's RefreshCellValue method
						#region obsolete code

						//int fldIndex = dr.FieldLayout.Fields.IndexOf(e.PropertyDescriptor.Name);

						//// if the index is less than zero then this record's
						//// FieldLayout didn't create a corresponding field so
						//// we can ignore the notification
						//if (fldIndex < 0)
						//    break;

						//Field field = dr.FieldLayout.Fields[fldIndex];

						//CellValuePresenter cvp = CellValuePresenter.FromRecordAndField(dr, field);

						//// synchronize the cell value presenter's value property
						//if (cvp != null)
						//    cvp.Value = field.GetCellValue(dr);

						#endregion //obsolete code

						// SSP 3/3/09 TFS11407
						// Added raiseInitializeRecord parameter.
						// 
						//dr.RefreshCellValue(e.PropertyDescriptor.Name);
						// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
						//dr.RefreshCellValue( e.PropertyDescriptor.Name, true );
						RecordPresenter rp = dr.AssociatedRecordPresenter;
						dr.RefreshCellValue( e.PropertyDescriptor.Name, true, rp != null && rp.IsDeactivated );
						
						break;
					}
				case ListChangedType.ItemMoved:
					if (this.IsResetPending)
						this.OnSourceCollectionReset();
					else
						this.OnSourceCollectionItemMoved(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, this._unsortedRecords[e.OldIndex], e.NewIndex, e.OldIndex));
					break;

                case ListChangedType.PropertyDescriptorAdded:
                case ListChangedType.PropertyDescriptorChanged:
                case ListChangedType.PropertyDescriptorDeleted:
					{
						FieldLayout.LayoutPropertyDescriptorProvider layoutProvider = this.GetPropertyDescriptorProvider();

						// notify all field layouts
						if (layoutProvider != null &&
							layoutProvider.FieldLayout.DataPresenter != null)
							layoutProvider.FieldLayout.DataPresenter.FieldLayouts.OnListMetaDataChanged(e, layoutProvider.Provider);
					}

                    break;

				case ListChangedType.Reset:
					// SSP 8/16/10 TFS35913
					// Do what we do when we get Reset from INotifyCollectionChanged. Check to see if we
					// are already synchronized with the data source and if so then process the reset 
					// notification synchronously.
					// 
					if ( this._firstResetProcessed )
						this.OnSourceCollectionReset( );
					else
						this.PostDelayedReset();
					break;
            }
        }

                #endregion //OnBindingListChanged	

				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
				#region OnChangeNotification
	    
    		private void OnChangeNotification(object sender, object eventArgs, bool isReset)
			{
                // JJD 3/29/08 - added support for printing.
                // We can't do asynchronous operations during a print
                //
                // MBS 7/29/09 - NA9.2 Excel Exporting
                //if (this._dataPresenter.IsReportControl)
                if(this._dataPresenter.IsSynchronousControl)
                    return;

                // if the notification is a reset then we can purge any pending change notifications
				if (isReset)
				{
					// If we are getting notifications from the IBindingList thern we ignore reset notifications
					// from INotifyCollectionChange. This is because the CollectionView translates Delete 
					// notifications to resets which we are trying to avoid processing. So only
					// clear the list if that is not the case.
					if (!(eventArgs is NotifyCollectionChangedEventArgs) ||
						this._bindingList == null || !this._bindingList.SupportsChangeNotification)
					{
						lock (this._asyncChangeList.SyncRoot)
						{
							this._asyncChangePending = null;

							this._asyncChangeList.Clear();

							// SSP 12/21/11 TFS73767 - Optimizations
							// Also null out the new variables.
							// 
							_processAsyncChangeList = null;
							_processAsyncChangeListSet = null;

							// see if the call is being made on the dp's thread
							if (this._dataPresenter.CheckAccess())
							{
								// process this notification
								this.ProcessChangeNotification(sender, eventArgs);
								return;
							}
						}
					}
				}

				// see if the call is being made on the dp's thread
				if (this._dataPresenter.CheckAccess())
				{
					// if so, process the queued notifications
					
					
					
					
					this.ProcessQueuedChangeNotifications( false );

					// process this notification
					this.ProcessChangeNotification(sender, eventArgs);
				}
				else
				{
                    // JJD 4/24/09 - TFS17045
                    // Allow PropertyChanged notifications from another thread 
                    if (!(eventArgs is PropertyChangedEventArgs))
                    {
                        ListChangedEventArgs listChangeArgs = eventArgs as ListChangedEventArgs;

                        // JJD 2/4/09 - TFS13049
                        // Only allow ListChangedType.ItemChanged notification from another thread.
                        // Otherwise, raise a 'NotSupportedException'
                        if (listChangeArgs == null ||
                             listChangeArgs.ListChangedType != ListChangedType.ItemChanged)
                        {
                            throw new NotSupportedException(DataPresenterBase.GetString("LE_NotSupportedException_14"));
                        }
                    }

					// lock the queue
					lock (this._asyncChangeList.SyncRoot)
					{
                        // append this notification
                        this._asyncChangeList.Add(new ChangeNotification(sender, eventArgs));

                        if (this._asyncChangePending == null)
							// SSP 12/21/11 TFS73767 - Optimizations
							// Using DataBind priority which is higher than Render and Input can potentially cause the UI to become unresponsive.
							// 
                            //this._asyncChangePending = this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new GridUtilities.MethodDelegate(OnDelayedChange));
							this._asyncChangePending = this._dataPresenter.Dispatcher.BeginInvoke( DispatcherPriority.Input, new GridUtilities.MethodDelegate( OnDelayedChange ) );

					}
				}
			}

   				#endregion //OnChangeNotification	
   
				#region OnCollectionViewCurrentChanged

		void OnCollectionViewCurrentChanged(object sender, EventArgs e)
		{
            // JJD 12/08/08 - Added IsSynchronizedWithCurrentItem property
            this.SetActiveRecordFromCurrentItem(true);
		}

				#endregion //OnCollectionViewCurrentChanged	
    
				#region OnCollectionViewCurrentChanging

		void OnCollectionViewCurrentChanging(object sender, CurrentChangingEventArgs e)
		{
            // check the anti-recursion flag
            if (this._isSynchronizingWithCurrentItem)
                return;

            if (this._dataPresenter == null ||
                this._collectionView == null ||
                !this._dataPresenter.IsInitialized ||
                !this._dataPresenter.IsSynchronizedWithCurrentItem)
                return;

            
            
            
            

            // if we aren't in sync nthen don't force ending of edit mode
            if (!this.IsInSyncWithCurrentPosition)
                return;

            Cell activeCell = this._dataPresenter.ActiveCell;

            // check if a cell is in edit mode and its not on the add record
            if (activeCell != null &&
                 activeCell.IsInEditMode &&
                !activeCell.Record.IsAddRecord)
            {
                // try to end edit mode, forcing it if the event is not cancelable
                activeCell.EndEditMode(true, !e.IsCancelable);

                // if we are still in edit mode then cancel this event if possible
                if (activeCell.IsInEditMode && e.IsCancelable)
                    e.Cancel = true;
            }
        }

				#endregion //OnCollectionViewCurrentChanging	
    
				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
                #region OnDelayedChange

        private void OnDelayedChange()
        {
			
			
			
            
			this.ProcessQueuedChangeNotifications( true );
        }

                #endregion //OnDelayedReset	
    
                #region OnDelayedReset

        // AS 1/27/09
        // Optimization - only have 1 parameterless void delegate class defined.
        //
        //delegate void MethodDelegate();

        internal void OnDelayedReset()
        {
            if (this.IsResetPending == true)
                this.OnSourceCollectionReset();
        }

                #endregion //OnDelayedReset	
 
				#region OnFieldLayoutPropertyChanged

		private void OnFieldLayoutPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case "GroupByVersion":
				case "SortVersion":
					// JJD 1/29/07 - BR19605
					// Added logic to refresh the sort on nested bands by
					// listening to the property change notifications of the
					// associated field layout

					// JJD 6/6/07
					// Only post the verify if this is not the root rm since
					// i's verify will be called by another code path after the
					// overall scroll position has been possibly reset to 0.
					//if (this._dataPresenter != null)
                    if (this._dataPresenter != null && this._parentRecord != null)
                    {
                        // JJD 3/29/08 - added support for printing.
                        // We can't do asynchronous operations during a print
                        //
                        // MBS 7/29/09 - NA9.2 Excel Exporting
                        //if (!this._dataPresenter.IsReportControl)
                        if(!this._dataPresenter.IsSynchronousControl)
                            this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Render, new VerifySortDelegate(VerifySort));
                    }
					break;
			}
		}

				#endregion //OnFieldLayoutPropertyChanged	
    
                #region OnSourceCollectionAddOrRemove

        private void OnSourceCollectionAddOrRemove(NotifyCollectionChangedEventArgs e)
        {
            if (this._sourceItems == null)
                return;

			// JM 03-20-12 TFS104401 - Move this inside the foreach block below
            //DataRecord record = null;

            // JJD 2/25/08 - BR30660
            // unused code
            //ArrayList changedList = null;
			IList items = null;
			int index = -1;

            if (e.NewItems != null)
			{ 
				items = e.NewItems;
				index = e.NewStartingIndex;
                // JJD 2/25/08 - BR30660
                // unused code
                //if (e.NewItems.Count > 1)
                //    changedList = new ArrayList(e.NewItems.Count);
			}
			else
			if (e.OldItems != null)
			{
				items = e.OldItems;
				index = e.OldStartingIndex;
                // JJD 2/25/08 - BR30660
                // unused code
                //if (e.OldItems.Count > 1)
                //    changedList = new ArrayList(e.OldItems.Count);
			}

			// SSP 8/18/10 TFS28525
			// Apparently CompositeCollection specifies -1 for index in add notification in which case
			// process it as a reset. Alternative would be find the indeces in the collection for the 
			// added items however collections shouldn't send -1 and that's really a bug in the composite
			// collection implementation.
			// 
			if ( index < 0 )
			{
				this.PostDelayedReset( );
				return;
			}

            // JJD 2/20/09 - TFS6106/BR33996
            // Moved logic into RemoverRecordHelper
            //bool activeRecordDeleted = false;
            //int activeRecordSortedIndex = -1;

			// JM 03-20-12 TFS104401 - Move this inside the foreach block below
			//int sortedIndex = index;

//			bool notifyVisibleRecordsCollection = true; 

            foreach (object listObject in items)
            {
				// JM 03-20-12 TFS104401
				DataRecord record = null;
				int sortedIndex = index;

				if (e.Action == NotifyCollectionChangedAction.Remove)
                {
                    // JJD 2/20/09 - TFS6106/BR33996
                    // Moved logic into RemoverRecordHelper
                    #region Old code - moved into RemoverRecordHelper method

                    //record = this._unsortedRecords.SparseArray.GetItem(index, false) as DataRecord;

                    //if (record != null && record.IsActive)
                    //{
                    //    activeRecordSortedIndex = record.SortIndex;

                    //    // JJD 12/2/06 - TFS6743/BR35763
                    //    // Check if the active cell is in edit mode and
                    //    // if is is force it out of edit mode since the record is
                    //    // being deleted.
                    //    Cell activeCell = this._dataPresenter.ActiveCell;
                    //    if (activeCell != null &&
                    //         activeCell.IsInEditMode)
                    //    {
                    //        activeCell.EndEditMode(false, true);
                    //    }
                    //}

                    //if (record != null)
                    //{
                    //    // JJD 1/8/08 - BR29410
                    //    // If the record is selected then remove it from selected records collection
                    //    if (record.IsSelected)
                    //        this._dataPresenter.SelectedItems.Records.Remove(record);
                    //    else
                    //    {
                    //        // JJD 1/8/08 - BR29410
                    //        // Since selected records and cells are mutually exclusive we only
                    //        // need to check the SelectedCells collection if the record isn't selelcted
                    //        SelectedCellCollection selectedCells = this._dataPresenter.SelectedItems.Cells;

                    //        int countOfCells = selectedCells.Count;

                    //        // JJD 1/8/08 - BR29410
                    //        // Loop over the selected cells backwards to remove any cells from this record
                    //        for (int i = countOfCells - 1; i >= 0; i--)
                    //        {
                    //            Cell cell = selectedCells[i];

                    //            if (cell != null && cell.Record == record)
                    //                selectedCells.RemoveAt(i);
                    //        }
                    //    }
                    //}

                    //this.RemoveAt(index, true);

                    //if ( record != null )
                    //{
                    //    if (!record.IsAddRecord)
                    //    {
                    //        record.InternalSetIsDeleted();

                        //    // SSP 2/11/09 TFS12467
                        //    // Dirtying parent records's scroll count won't help if the this record's parent 
                        //    // viewable record collection's scroll count is not re-calculated properly. So 
                        //    // dirty that instead by dirtying the scroll count of this record which will 
                        //    // cause its parent viewable record collection to recalculate its scroll count 
                        //    // which in turn will dirty the parent record's scroll count.
                        //    // 
                        //    // ------------------------------------------------------------------------------
                        //    //if (record.ParentRecord != null)
                        //    //	record.ParentRecord.DirtyScrollCount();
                        //    record.DirtyScrollCount( );
                        //    // ------------------------------------------------------------------------------
                        //}

                    //    if (record.IsActive && !record.IsAddRecord)
                    //    {
                    //        activeRecordDeleted = true;
                    //        this._dataPresenter.ActiveRecord = null;
                    //    }
                    //}

                    //this.BumpUnderlyingDataVersion();

                    //// when the add record is tranmsitioning from one state 
                    //// to another (template -> real record or vice versa) we
                    //// shouldn't pass any notification to the visible records 
                    //// collection since from its perspecitive noting has changed
                    //if (record != null && record.IsAddRecord)
                    //{
                    //    // JJD 11/24/08 - TFS6743/BR35763
                    //    // Since the notification sequence is different when dealing with an editable collection view
                    //    // we need to clear the cell values hete when the add record is being deleted so call
                    //    // PrepareForNewCurrentAddRecord which set the addrecord back to its original state
                    //    if (this._editableCollectionView != null)
                    //    {
                    //        record.SetDataItemHelper(null);
                    //        record.InternalSetDataChanged(false);
                    //        this.PrepareForNewCurrentAddRecord();
                    //    }
                    
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


					
					
					
					
					//this.OnDataChanged( DataChangeType.RemoveRecord, record, null );
                    //}

                    #endregion //Old code - moved into RemoverRecordHelper method
                    this.RemoveRecordHelper(index);
                }
                else
                {
 
					// use the current add record if we are calling AddNew explicitly 
					if (items.Count == 1)
					{
						if (this._inAddNew == true)
						{
							record = this.CurrentAddRecord;

							// if the add record is on top then insert it at position zero in the 
							// sorted records collection
							if (this.IsAddRecordOnTop)
							{
								// AS 5/7/09 NA 2009.2 ClipboardSupport
								//sortedIndex = 0;
								sortedIndex = this._newRecordOnTopOffset;
							}

							// when the add record is tranmsitioning from one state 
							// to another (template -> real record or vice versa) we
							// shouldn't pass any notification to the visible records 
							// collection since from its perspecitive noting has changed
							switch (this.AddNewRecordLocation)
							{
								case AddNewRecordLocation.OnTop:
								case AddNewRecordLocation.OnBottom:
									//notifyVisibleRecordsCollection = false;
									break;
							}
						}
						else
						{
                            // JJD 12/04/08 - TFS6743/BR35763
                            // map the sorted idex properly
                            sortedIndex = this._sortedRecords.ViewableRecords.GetSortedIndexFromUnsortedIndex(index);

							//if (record != null && record.IsDataItemEqual(listObject))
							//     //DataRecord.GetObjectForRecordComparision(listObject) == 
							//     //DataRecord.GetObjectForRecordComparision(record.DataItem) )
                            IList sourceList = this.List;
                        
							// if the count is the same as the as our unsorted aray count
							// then this is the redundant add notification you get when
							// committing an object created by calling AddNew on the IBindingList.
							// These notifications we can ignore.
							//
							// Note: when bound to a sorted IBindingList there may be a 
							// subsequent 'move' notification to re-slot the added item
							// in its appropriate slot based on the sort criteria in effect.
							if ( sourceList != null &&
								 this._unsortedRecords.SparseArray.Count == sourceList.Count)
							{
								record = this.CurrentAddRecord;

								// JJD 5/8/07 - BR22680
								// Make sure we have a current dada record before proceeding
                                if (record != null)
                                {
                                    if (record.IsDataItemEqual(listObject))
                                    {
                                        // reinitialze the data item since this may have changed
                                        record.InitializeDataItem(listObject, index);
                                    }

                                    this.BumpUnderlyingDataVersion();

                                    // JJD 2/22/07 - BR20439
                                    // clear this flag so we knoiw that the notification was received
                                    this._recordBeingCommitted = null;

									
									
									
									
									this.OnDataChanged( DataChangeType.AddRecord, record, null );

									// SSP 4/30/08 BR32427
									// We should do this at the same time that we reset the isAddRecord flag.
									// 
									//this._sortedRecords.ViewableRecords.OnAddRecordCommitted(record);

                                    //    // if the index is different then move it to the new index
                                    //    if (oldIndex != index)
                                    //    {
                                    //        // move the record to its new slot in the unsorted collection
                                    //        this._unsortedRecords.SparseArray.RemoveAt(oldIndex);
                                    //        this._unsortedRecords.SparseArray.Insert(index, record);

                                    //        // notify listeners
                                    //        this.RaisePropertyChangedEvent("Unsorted");
                                    //        this._unsortedRecords.OnCollectionChanged(NotifyCollectionChangedAction.Move, record, index, oldIndex);
                                    //    }

                                    //    if (record.ParentRecord != null)
                                    //        record.ParentRecord.DirtyScrollCount();

                                    //    return;
                                    //}
                                }
                                else
                                {
                                    // JJD 5/8/09 - TFS17464
                                    // Since the record was added outside of our UI and this is the
                                    // 2nd add notification we received (on the acceptchanges call,
                                    // the 1st was received on the AddNew call), then if the record
                                    // already has a record presenter we show refresh the cell values
                                    // to pick up changes made between AddNew and AcceptChanges.
                                    // The reason we need to do this is because we don't get cell
                                    // change notifications during this period.
                                    if (index >= 0 && index < this._unsortedRecords.Count)
                                    {
                                        record = this._unsortedRecords[index];

                                        if (record != null &&
                                             record.AssociatedRecordPresenter != null)
                                            record.RefreshCellValues(false, true);
                                    }
                                }

								return;
							}

							record = null;
						}
					}

                    // JJD 2/25/08 - BR30660
                    // When adding a record to a previously empty datasource we
                    // need to process it as a reset to make sure grouping is
                    // domne properly
                    if (this._unsortedRecords.Count == 0)
                    {
                        this.OnSourceCollectionReset();
                        return;
                    }

                    if (record == null)
                        record = DataRecord.Create(this._sortedRecords, listObject, false, index);

                    this.InsertRecord(index, sortedIndex, record, true);

					// SSP 2/11/09 TFS12467
					// Dirtying parent records's scroll count won't help if the this record's parent 
					// viewable record collection's scroll count is not re-calculated properly. So 
					// dirty that instead by dirtying the scroll count of this record which will 
					// cause its parent viewable record collection to recalculate its scroll count 
					// which in turn will dirty the parent record's scroll count.
					// 
					// ------------------------------------------------------------------------------
                    //if (record.ParentRecord != null)
                    //    record.ParentRecord.DirtyScrollCount();
					record.DirtyScrollCount( );
					// ------------------------------------------------------------------------------

                    this.BumpUnderlyingDataVersion();

                    record.FireInitializeRecord();
 
                    index++;

					
					
					
					
					this.OnDataChanged( DataChangeType.AddRecord, record, null );

					// JJD 11/30/10 - TFS31984 
					// Adjust the scroll position to maintain current records in view
					if (this._dataPresenter.ScrollBehaviorOnListChange == ScrollBehaviorOnListChange.PreserveRecordsInView &&
						record.IsFixed == false)
					{
						IScrollInfo si = this._dataPresenter.ScrollInfo;

						if (si != null)
						{
						    ViewBase currentView = this._dataPresenter.CurrentViewInternal;
						    bool isHorizontal = currentView.HasLogicalOrientation && currentView.LogicalOrientation == Orientation.Horizontal;

							// JJD 12/2/10 - TFS31984 
							// only increment the scrollposition if there are more records than can be displayed, 
							// i.e. the scrollbar's extent is greater than its viewport. 
							// This prevents some jumpiness when UseNestedPanels is set to true in grid view.						    
							if ((isHorizontal == true  && si.ExtentWidth  > si.ViewportWidth) ||
						        (isHorizontal == false && si.ExtentHeight > si.ViewportHeight))
							{
								int rcdScrollCount = record.ScrollCountInternal;

								if (rcdScrollCount > 0 &&
									record.OverallScrollPosition <= ((IViewPanelInfo)(this._dataPresenter)).OverallScrollPosition)
								{
									((IViewPanelInfo)(this._dataPresenter)).OverallScrollPosition += rcdScrollCount;
								}
							}
						}
					}
				}

                // JJD 2/25/08 - BR30660
                // unused code
                //if (changedList != null)
                //    changedList.Add(record);
            }

            // JJD 2/20/09 - TFS6106/BR33996
            // MOved logic into RemoverRecordHelper
            // reselect next record after deletion unless this was done externally
            //if (activeRecordDeleted == true &&
            //    this._recordBeingDeleted == null)
            //    this.ReselectRecordAtSortedIndex(activeRecordSortedIndex);

			// JJD 2/21/07 commented out redundant notifications
			#region Old code

			//this.RaisePropertyChangedEvent("Unsorted");
			//this.RaisePropertyChangedEvent("Sorted");
			//this.RaisePropertyChangedEvent("Groups");
			//this.RaisePropertyChangedEvent("Current");

			//object changeParam;

			//if (changedList != null) 
			//    changeParam = changedList;
			//else
			//    changeParam = record;

			//if ( changeParam != null )
			//    this._unsortedRecords.OnCollectionChanged(e.Action, changeParam, index);
			//else
			//    this._unsortedRecords.RaiseChangeEvents(true);

			#endregion //Old code	
    
        }

                #endregion //OnSourceCollectionAddOrRemove
    
                #region OnSourceCollectionItemMoved

        private void OnSourceCollectionItemMoved(NotifyCollectionChangedEventArgs e)
        {
			int newIndex = e.NewStartingIndex;
			int oldIndex = e.OldStartingIndex;

			Debug.Assert(newIndex >= 0 && newIndex < this._unsortedRecords.Count, "Invalid ItemMoved NewIndex");
			Debug.Assert(oldIndex >= 0 && oldIndex < this._unsortedRecords.Count, "Invalid ItemMoved OldIndex");

			if (newIndex < 0 || newIndex > this._unsortedRecords.Count - 1)
				return;

			if (oldIndex < 0 || oldIndex > this._unsortedRecords.Count - 1)
				return;

			//// map to the sorted index values
			int sortedOldIndex = this._sortedRecords.ViewableRecords.GetSortedIndexFromUnsortedIndex(oldIndex);
			int sortedNewIndex = this._sortedRecords.ViewableRecords.GetSortedIndexFromUnsortedIndex(newIndex);

			// get the existing record but only if it was allocated previously
			Record record = this._unsortedRecords.SparseArray.GetItem(oldIndex, false);

			// move the records in the unsorted collection
			this._unsortedRecords.SparseArray.RemoveAt(oldIndex);
			this._unsortedRecords.SparseArray.Insert(newIndex, record);

            bool adjustRecordPositionInSortedList = !this._isSorted;

            if (record is DataRecord)
            {
                // JJD 12/04/08 - TFS6743/BR35763
                // If the record being moved is the addrecord then don't move it in the sorted collection
                if (record == this._recordBeingCommitted &&
                    ((DataRecord)record).IsAddRecord)
                    adjustRecordPositionInSortedList = false;
                else
                {
                    IList<DataRecord> recordsInsertedAtTop = this.RecordsInsertedAtTop;
                    if (recordsInsertedAtTop != null &&
                        recordsInsertedAtTop.Contains(record as DataRecord))
                        adjustRecordPositionInSortedList = false;
                }
            }

            // JJD 12/04/08 - TFS6743/BR35763
            // Make sure the indexes are different
            //if (adjustRecordPositionInSortedList)
            if (adjustRecordPositionInSortedList && sortedNewIndex != sortedOldIndex )
			{
				// move the same records in the sorted collection
				this._sortedRecords.ViewableRecords.MoveRecord(record, sortedNewIndex, sortedOldIndex);
			}

			this.BumpUnderlyingDataVersion();

			// notify any listeners
			this._unsortedRecords.OnCollectionChanged(NotifyCollectionChangedAction.Move, this._unsortedRecords[newIndex], newIndex, oldIndex);

			
			
			
			
			this.OnDataChanged( DataChangeType.ItemMoved, record as DataRecord, null );

			//if (adjustRecordPositionInSortedList)
			//    this._sortedRecords.OnCollectionChanged(NotifyCollectionChangedAction.Move, this._unsortedRecords[newIndex], sortedNewIndex, sortedOldIndex);

			//if (this._visibleRecords != null && adjustRecordPositionInSortedList)
			//    this._visibleRecords.RaiseChangeEvents(true);

        }

                #endregion //OnSourceCollectionItemMoved
    
                #region OnSourceCollectionChanged

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
			// JJD 6/1/07
			// We always want to check the underlying source on a reset even if _bindingList is null
			//if (this._bindingList != null &&
			//    this._firstResetProcessed)
			if (this._firstResetProcessed)
			{
				if (e.Action == NotifyCollectionChangedAction.Reset)
				{
					// On a reset if the underlying source of a collection view has changed 
					// then redo the hookup
                    if (this._collectionView != null)
                    {
                        if (this._collectionView.SourceCollection != this._underlyingSourceItems)
                        {
                            this.SetDataSource(this._collectionView, false);
                            return;
                        }
                        else
                        {
                            // JJD 11/17/08 - TFS6743/BR35763 
                            // Call VerifyListSourceToUse to make sure the data source hasn't changed
                            // If it returns true then set the datasource to trigger a reset syncronously
                            if (this.VerifyListSourceToUse())
                            {
                                this.SetDataSource(this._collectionView, false);
                                return;
                            }
                        }
                    }
				}

				// JJD 5/9/07
				// Only bypass notifications if the bindingList is actually raising its own
				// JJD 6/1/07
				// Cjheck for a non-null _bindingList
				//if (this._bindingList.SupportsChangeNotification )
				if (this._bindingList != null && this._bindingList.SupportsChangeNotification )
					return;
			}

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    // JJD 11/25/08 - TFS6743/BR35763 - added 
                    // If we have a filtered or sorted collectionView that doesn't
                    // implement IEditableCollectiionView then we can't map the
                    // indices that get passed into the change notifocations so we need
                    // to treat them as resets
                    //if ( this.IsResetPending )
                    if ( this.IsResetPending || this.TreatUpdatesAsResets)
                        this.OnSourceCollectionReset();
                    else
                        this.OnSourceCollectionItemMoved(e);
                    break;

                case NotifyCollectionChangedAction.Replace:
				{
					Debug.Assert(e.OldItems != null && e.OldItems.Count > 0);
					Debug.Assert(e.NewItems != null && e.NewItems.Count > 0);
					Debug.Assert(e.OldStartingIndex == e.NewStartingIndex);
					Debug.Assert(e.OldStartingIndex == e.NewStartingIndex);

					if ( e.OldItems == null || e.NewItems == null)
						break;
					
					Debug.Assert(e.OldItems.Count == e.NewItems.Count);

                    // JJD 11/25/08 - TFS6743/BR35763 - added 
                    // If we have a filtered or sorted collectionView that doesn't
                    // implement IEditableCollectiionView then we can't map the
                    // indices that get passed into the change notifocations so we need
                    // to treat them as resets
                    //if ( this.IsResetPending )
                    if (this.IsResetPending || this.TreatUpdatesAsResets)
                        this.OnSourceCollectionReset();
					else
					{
						int index = e.OldStartingIndex;

						Debug.Assert(index >= 0);

						if (index >= 0 &&
							e.NewItems != null &&
							e.OldItems != null &&
							e.NewItems.Count == e.OldItems.Count)
						{
							int count = e.OldItems.Count;

							// loop over the array of items passed into the event
							for (int i = 0; i < count; i++)
							{
								// see if we have already created a DataRecord for this item
								DataRecord dr = this._unsortedRecords.SparseArray.GetItem(index + i, false) as DataRecord;

								// if so reinitialize the data item 
								if ( dr != null )
								{
									dr.InitializeDataItem( e.NewItems[i], index );

									// SSP 3/3/09 TFS11407
									// Added code to raise InitializeRecord event whenever a cell value changes.
									// 
									// JJD 11/17/11 - TFS78651 
									// Added sortValueChanged parameter
									//dr.FireInitializeRecord( true );
									dr.FireInitializeRecord( true, true );
								}
							}
						}
						else
							this.OnSourceCollectionReset();
					}
                    break;
				}
                case NotifyCollectionChangedAction.Reset:
                    // JJD 11/17/08 - TFS6743/BR35763 
                    // If we are using the collectionviewWrapper we want the Reset to be synchronous
                    //if (this._collectionViewWrapper != null && this._firstResetProcessed)
                    if ( this._firstResetProcessed)
                        this.OnSourceCollectionReset();
                    else
                        this.PostDelayedReset();
                    break;
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                    // JJD 11/25/08 - TFS6743/BR35763 - added 
                    // If we have a filtered or sorted collectionView that doesn't
                    // implement IEditableCollectiionView then we can't map the
                    // indices that get passed into the change notifocations so we need
                    // to treat them as resets
                    //if ( this.IsResetPending )
                    if (this.IsResetPending || this.TreatUpdatesAsResets)
                        this.OnSourceCollectionReset();
                    else
                        this.OnSourceCollectionAddOrRemove(e);
                    break;
            }
        }

                #endregion //OnSourceCollectionChanged

                #region OnSourceCollectionReset

        internal void OnSourceCollectionReset()
        {

			// JJD 06/21/12 - TFS113886 
			// If we are processing a reset then log an error to the debugger
			if (_isInReset && _sourceItems != null)
				GridUtilities.LogDebuggerError(string.Format("DataSource reset notification received recursively. DataSource should not send a change notifcation during a reset, DataSource: {0}{1}", _sourceItems, Environment.NewLine));

			bool changeEventsRaised = false;

			// JJD 7/9/07
			// Added support for handling change notifications on another thread 
			lock (this._asyncChangeList.SyncRoot)
			{
                // JJD 5/23/08 - BR33317 
                // Added isinreset flag
                this._isInReset = true;

				// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
				// 
				DataPresenterBase dataPresenter = this.DataPresenter;

                try
                {
                    this._asyncChangePending = null;

                    this._asyncChangeList.Clear();

					// SSP 12/21/11 TFS73767 - Optimizations
					// Also null out the new variables.
					// 
					_processAsyncChangeList = null;
					_processAsyncChangeListSet = null;

                    this._resetOperationPending = null;

                    #region Check if source is null

                    if (this._sourceItems == null)
                    {
                        this.Clear();
                        this.ClearCachedRecordsOnTop();

						
						
						this.OnDataChanged( DataChangeType.Reset, null, null );

                        return;
                    }

                    #endregion //Check if source is null

                    #region Setup

                    this.BumpUnderlyingDataVersion();

                    int oldCount = this._unsortedRecords.Count;

                    // AS 3/22/07 RecordCollection FieldLayout
                    //this._fieldLayout = null;
                    this.InitializeFieldLayout(null);

					
					
					
                    

                    int i;
                    bool inSameOrder = false;

                    // JJD 11/17/08 - TFS6743/BR35763 
                    // Use the collectionviewwrapper if applicaable
					// JJD 10/27/10 - TFS58275
					// First use the _list member (which will be set to the _collectionViewWrapper is non-null) then use the
					// _collectionView. Then firnally the _sourceItems if the others are null
                    //IEnumerable sourceToUse = this._collectionViewWrapper != null ? this._collectionViewWrapper : this._sourceItems;

					// JJD 10/07/11 - TFS90710
					// In partial trust XBAP applications the following nested ?: line triggered a verification exception
					// for some reason. Breaking it out into multiple statements resolved the issue. Very strange.
					//IEnumerable sourceToUse = this._list != null ? this._list : this._collectionView != null ? this._collectionView : this._sourceItems;
					IEnumerable sourceToUse = this._list;

					if (sourceToUse == null)
					{
						if (this._collectionView != null)
							sourceToUse = this._collectionView;
						else
							sourceToUse = this._sourceItems;
					}

				    //ICollection sourceList = this._sourceItems as ICollection;
					// JJD 10/27/10 - TFS58275
					// If we don't have a _list then we need to not rely on the count
				    //ICollection sourceList = sourceToUse as ICollection;
					ICollection sourceList = this._list;
					int newCount = 0;

				    // get the count of new items
                    if (sourceList != null)
                        newCount = sourceList.Count;
                    else
                    {
                        // JJD 12/18/09 - TFS24565
                        // Wrap the enumerator in a try/catch and raise a data error event
                        // instead of leaving it unhandled
                        try
                        {
                            // since the source doesn't implement ICollection we need to count each
                            foreach (Object obj in sourceToUse)
                            {
                                // JJD 1/13/08
                                // Bypass XmlComments
                                if (obj is System.Xml.XmlComment)
                                    continue;

                                newCount++;

                            }
                        }
                        catch (Exception e)
                        {
                            // JJD 12/18/09 - TFS24565
                            // raise the data error event
                            dataPresenter.RaiseDataError(null, this._parentRecord != null ? this._parentRecord.Field : null, e, DataErrorOperation.Other, null, true);

                            // JJD 12/18/09 - TFS24565
                            // set the count to 0 since the enumerator is not usable and
                            // provide an empty enumerable so the logic below doesn't
                            // try to use the bad one
                            newCount = 0;
                            sourceToUse = new GridUtilities.EmptyEnumerable<object>();
                        }
                    }

                    // JJD 5/8/09 - TFS17464
                    // Allocate a list to hold records that are being reused that have 
                    // associated record presenters so we can refresh their cell values
                    // since they may have changed.
                    List<DataRecord> existingRcdsWithPresenters =
                        newCount > 0 && this._unsortedRecords.Count > 0
                            ? new List<DataRecord>(Math.Min(100, this._unsortedRecords.Count))
                            : null;

                    this._firstResetProcessed = true; // newCount > 0;

                    #endregion //Setup

                    #region Check to see if the records are in the same order as the last time

					// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
					// 
					DataSourceResetBehavior resetBehavior = null != dataPresenter
						? dataPresenter.DataSourceResetBehavior
						: DataSourceResetBehavior.ReuseRecordsViaEnumerator;

                    // If the rows in binding list and the the rows in our list are in the same order,
                    // then we don't need to syncronize.
                    //
					// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
					// 
                    //if (sourceList != null && this._unsortedRecords.Count == newCount)
					if ( DataSourceResetBehavior.ReuseRecordsViaEnumerator == resetBehavior
						// JJD 10/27/10 - TFS58275
						// Use sourceToUse instead
						//&& sourceList != null && this._unsortedRecords.Count == newCount )
						&& sourceToUse != null && this._unsortedRecords.Count == newCount )
                    {
                        inSameOrder = true;

                        i = 0;

						// JJD 10/27/10 - TFS58275
						// Use sourceToUse instead
						//foreach (object source in sourceList)
                        foreach (object source in sourceToUse)
                        {
                            // JJD 1/13/08
                            // Bypass XmlComments
                            if (source is System.Xml.XmlComment)
                                continue;

                            DataRecord record = (DataRecord)this._unsortedRecords.SparseArray.GetItem(i, false);
                            if (null != record)
                            {
                                if (!record.IsDataItemEqual(source))
                                {
                                    inSameOrder = false;
                                    break;
                                }
                            }

                            i++;
                        }
                    }

                    #endregion //Check to see if the records are in the same order as the last time

					// SSP 3/5/09 TFS5842 - Optimization
					// Moved this here from within the below if block.
					// 
					this.BeginUpdate( );

                    if (!inSameOrder)
                    {
                        #region Try to preserve existing records if possible

						
						
						
						RecordSparseArrayBase unsortedSparseArr = _unsortedRecords.SparseArray;
						RecordSparseArrayBase sortedSparseArr = _sortedRecords.SparseArray;

						Record currentActiveRecord = dataPresenter.ActiveRecordInternal;
						bool activeRecordFromThisManager = null != currentActiveRecord ? unsortedSparseArr.Contains( currentActiveRecord ) : false;

						ICollection<DataRecord> discardedRecords;
						bool discardedRecordsInfoComplete;
						int reusedRecordsCount = 0;
						

						// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
						// Moved this here from below.
						// 
						// JJD 11/17/08 - TFS6743/BR35763 
						// Use sourceToUse stack variable instead
						bool preload = dataPresenter.RecordLoadMode == RecordLoadMode.PreloadRecords ||
									// JJD 10/27/10 - TFS58275
									//!(this._sourceItems is IList);
									// JJD 10/27/10 - TFS58275
									// If _list is null then we need to preload
									//!( sourceToUse is IList );
									null == this._list;

						// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
						// Added the if block and enclosed the existing code into the else-if block.
						// 
						// JJD 10/27/10 - TFS58275
						// Check to make sure _list is not null
						//if ( sourceToUse is IList 
						if ( this._list != null 
							&& ( DataSourceResetBehavior.ReuseRecordsViaIndexOf == resetBehavior 
							|| DataSourceResetBehavior.DiscardExistingRecords == resetBehavior ) )
						{
							// JJD 10/27/10 - TFS58275
							// Use the cached list
							//IList dataList = sourceToUse as IList;
							IList dataList = this._list;

							List<DataRecord> existingRecords = GridUtilities.ToList<DataRecord>( this._unsortedRecords.SparseArray.NonNullItems, false );

							// JJD 12/02/08 - TFS6743/BR35763
							// Don't clear the groups collection just yet since it will be 
							// updated inside the VerifySort method that is called at the end of
							// this method. This allows us to possibly re-use the groupbyrecords to
							// preserve their state, e.g. IsExpanded.
							//this.Clear(false);
							this.Clear( false, false );

							unsortedSparseArr.Expand( newCount );
							sortedSparseArr.Expand( newCount );

							if ( DataSourceResetBehavior.DiscardExistingRecords != resetBehavior )
							{
								int existingRecordsCount = existingRecords.Count;
								for ( i = 0; i < existingRecordsCount; i++ )
								{
									DataRecord dr = existingRecords[i];

									object dataItem = null != dr ? dr.DataItemInternal : null;
									int listIndex = null != dataItem ? dataList.IndexOf( dataItem ) : -1;
									if ( listIndex >= 0 )
									{
										unsortedSparseArr[ listIndex ] = dr;
										sortedSparseArr[ listIndex ] = dr;

										dr.InitializeDataItem( dataItem, listIndex );

										// Null out the entry since we've recycled the record.
										existingRecords[i] = null;
										reusedRecordsCount++;

										// JJD 5/8/09 - TFS17464
										// If the record has an associated record presenters add it to
										// the queue so we can refresh its cell values
										// since they may have changed.
										if ( existingRcdsWithPresenters != null &&
											dr.AssociatedRecordPresenter != null )
											existingRcdsWithPresenters.Add( dr );
									}
								}
							}

							// SSP 4/11/08 BR31448
							// If we get a reset while adding a new record, we need to associate the added data
							// item to the template add-record.
							// Added the if block and enclosed the existing code into the else-if block.
							// 
							if ( _inAddNew && null != _currentAddRecord && newCount > 0 )
							{
								if ( null == _unsortedRecords[newCount - 1] )
									_unsortedRecords[newCount - 1] = _currentAddRecord;
								else
									Debug.Assert( false, "This can result from underlying data source not raising proper notifications when adding new record." );
							}

							// Remove nulled out entries. All the remaining non-null entries are records that
							// we couldn't recycle and thus are being discarded.
							// 
							if ( reusedRecordsCount > 0 )
								Utilities.RemoveAll<DataRecord>( existingRecords, null );

							discardedRecords = existingRecords;
							discardedRecordsInfoComplete = true;

							if ( preload )
							{
								for ( i = 0; i < newCount; i++ )
								{
									if ( null == unsortedSparseArr[i] )
									{
										object listObject = dataList[i];
										DataRecord dr = DataRecord.Create( _sortedRecords, listObject, false, i );

										unsortedSparseArr[i] = dr;
										sortedSparseArr[i] = dr;

										dr.FireInitializeRecord( );
									}
								}
							}
						}
						else // if ( DataSourceResetBehavior.ReuseRecordsViaEnumerator == resetBehavior )
						{
							DataRecordHolder recordHolder;
							Hashtable hash = null;

							// AS 5/19/09 TFS17455
							int nonNullCount = 0;
							int hashCount = 0;

							// if we have existing records load a hash table so we can hopefully reuse them
							if ( this._unsortedRecords.Count > 0 )
							{
								hash = new Hashtable( this._unsortedRecords.Count, 0.90f );

								foreach ( DataRecord dr in this._unsortedRecords.SparseArray.NonNullItems )
								{
									// AS 5/19/09 TFS17455
									nonNullCount++;

									// SSP 11/16/10 TFS59686
									// The list index is not used by the DataRecordHolder therefore there's no need to pass it along.
									// 
									//recordHolder = new DataRecordHolder( dr, dr.DataItemForComparison, dr.DataItemIndex );
									recordHolder = new DataRecordHolder( dr, dr.DataItemForComparison );

									hash[recordHolder] = recordHolder;
								}

								// AS 5/19/09 TFS17455
								hashCount = hash.Count;
							}

							// SSP 3/5/09 TFS5842 - Optimization
							// Moved this before the containting if block.
							// 
							
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


							// JJD 12/02/08 - TFS6743/BR35763
							// Don't clear the groups collection just yet since it will be 
							// updated inside the VerifySort method that is called at the end of
							// this method. This allows us to possibly re-use the groupbyrecords to
							// preserve their state, e.g. IsExpanded.
							//this.Clear(false);
							this.Clear( false, false );

							int index = 0;

							
							
							
							
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


							unsortedSparseArr.Expand( newCount );
							sortedSparseArr.Expand( newCount );

							// JJD 7/17/07 - BR29924
							// Optimization - keep a flag to determine if records have been inserted 
							//bool recordsHaveBeenInserted = false;

							// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
							// Moved this above.
							// 
							
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


							// SSP 4/11/08 BR31448
							// If we get a reset while adding a new record, we need to associate the added data
							// item to the template add-record.
							// 
							//if (preload || hash != null)
							if ( preload || hash != null || _inAddNew )
							{
								// SSP 4/11/08 BR31448
								// 
								bool templateAddRecordUtilized = false;

								// JJD 11/17/08 - TFS6743/BR35763 
								// Use sourceToUse stack variable instead
								//foreach (object listObject in this._sourceItems)
								foreach ( object listObject in sourceToUse )
								{
									// JJD 1/13/08
									// Bypass XmlComments
									if ( listObject is System.Xml.XmlComment )
										continue;

									if ( listObject != null )
									{
										recordHolder = null;

										if ( hash != null && hash.Count > 0
											// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
											// If data source reset behavior is to discard old records then don't
											// recycle.
											// 
											&& DataSourceResetBehavior.DiscardExistingRecords != resetBehavior
											)
										{
											// SSP 11/16/10 TFS59686
											// The list index is not used by the DataRecordHolder therefore there's no need to pass it along.
											// 
											//recordHolder = (DataRecordHolder)hash[new DataRecordHolder( DataRecord.GetObjectForRecordComparision( listObject ), index )];
											recordHolder = (DataRecordHolder)hash[new DataRecordHolder( DataRecord.GetObjectForRecordComparision( listObject ) )];

											// if we find it in the hash table just reinsert it back into the list
											// then remove it from the hash and continue
											if ( recordHolder != null )
												hash.Remove( recordHolder );
										}

										
										
										
										
										

										DataRecord record = null;

										if ( recordHolder == null )
										{
											// SSP 4/11/08 BR31448
											// If we get a reset while adding a new record, we need to associate the added data
											// item to the template add-record.
											// Added the if block and enclosed the existing code into the else-if block.
											// 
											if ( 1 + index == newCount && _inAddNew && !templateAddRecordUtilized && null != _currentAddRecord )
											{
												record = _currentAddRecord;
												templateAddRecordUtilized = true;
											}
											else if ( !preload )
											{
												if ( hash != null && hash.Count == 0 )
													break;
												else
												{
													index++;
													continue;
												}
											}
										}

										// JJD 7/17/07 - BR29924
										// Optimization - set a flag so we know records have been inserted
										//recordsHaveBeenInserted = true;

										if ( recordHolder != null )
										{
											record = recordHolder.record;
											record.InitializeDataItem( listObject, index );

											// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
											// 
											reusedRecordsCount++;

											// JJD 5/8/09 - TFS17464
											// If the record has an associated record presenters add it to
											// the queue so we can refresh its cell values
											// since they may have changed.
											if ( existingRcdsWithPresenters != null &&
												record.AssociatedRecordPresenter != null )
												existingRcdsWithPresenters.Add( record );
										}
										// SSP 4/11/08 BR31448
										// Related to change above. Only create a new data record if record has been
										// created above.
										// 
										//else
										else if ( null == record )
											record = DataRecord.Create( this._sortedRecords, listObject, false, index );

										if ( this._fieldLayout == null )
										{
											// AS 3/22/07 RecordCollection FieldLayout
											//this._fieldLayout = record.FieldLayout;
											this.InitializeFieldLayout( record.FieldLayout );
										}

										
										
										
										
										unsortedSparseArr[index] = record;
										sortedSparseArr[index] = record;

										if ( recordHolder == null )
											record.FireInitializeRecord( );
									}

									index++;
								}
							}

							// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
							// If original hash didn't have the same number of entries as nonNullCount then
							// there were duplicate list objects in the data list.
							// 
							discardedRecordsInfoComplete = hashCount == nonNullCount;
							discardedRecords = null == hash 
								? new GridUtilities.EnumeratorCollection<DataRecord>( new GridUtilities.EmptyEnumerable<DataRecord>( ), 0 )
								: GridUtilities.GetConvertedCollection<DataRecordHolder, DataRecord>( hash.Values, hash.Count,
									delegate( DataRecordHolder drh ) 
									{ 
										return drh.record; 
									} );
						}

						// SSP 3/5/09 TFS5842 - Optimization
						// Moved this after the containting if block.
						// 
						
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

                        
                        
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)


						
						
						
						
						
						
						
						
						if ( null != currentActiveRecord && activeRecordFromThisManager )
						{
							if ( !unsortedSparseArr.Contains( currentActiveRecord ) )
								dataPresenter.ClearActiveRecord( true );
						}
						
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

						

						// AS 5/19/09 TFS17455
						// Update the SelectedItems when the collection is reset.
						//
						// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
						// 
						// ----------------------------------------------------------------------------
						if ( dataPresenter != null )
						{
							// If none of the items were reused and this is the root recordmanager then clear
							// the selected collections (excluding the fields).
							if ( reusedRecordsCount == 0 && _dataPresenter.RecordManager == this )
							{
								// JJD 1/16/12 - TFS63720
								// Pass false in as new resetItemSelectionStates parameter
								//_dataPresenter.SelectedItems.InternalClearSelection( true, true, false );
								_dataPresenter.SelectedItems.InternalClearSelection( true, true, false, false );
							}
							else
							{
								UpdateSelectedItems( _dataPresenter, discardedRecordsInfoComplete ? discardedRecords : null );
							}
						}
						
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

						// ----------------------------------------------------------------------------

                        #endregion //Try to preserve existing records if possible
                    }
                    else
                    {
                        #region Keep track of any existing rcds that have record presenters

                        // JJD 5/8/09 - TFS17464
                        // Since the records are in the same order loop over the
                        // unsorted records and keep track of any that have 
                        // associated record presenters so we can refresh their cell values
                        if (existingRcdsWithPresenters != null)
                        {
                            foreach (DataRecord record in this._unsortedRecords.SparseArray.NonNullItems)
                            {
                                if (record != null &&
                                    record.AssociatedRecordPresenter != null)
                                    existingRcdsWithPresenters.Add(record);
                            }
                        }

                        #endregion //Keep track of any existing rcds that have record presenters
                    }

                    this._unsortedRecordsDirty = true;

                    // JJD 5/31/07 - BR23233, BR23055, BR23285, BR23286
                    // If there are no records in the source list then get a field layout from the list itself (if possible)
                    if (this._unsortedRecords.Count == 0)
                    {
                        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
                        // Added parentCollection parameter
                        //FieldLayout fl = this._dataPresenter.FieldLayouts.GetDefaultLayoutForItem(null, this._underlyingSourceItems);
                        RecordCollectionBase parentRecordCollection = this._parentRecord != null ? this._parentRecord.ChildRecords : null;
                        FieldLayout fl = this._dataPresenter.FieldLayouts.GetDefaultLayoutForItem(null, this._underlyingSourceItems, parentRecordCollection);

                        if (fl != null)
                        {
							// AS 3/22/10 TFS29701
							// We were checking the IsInitialized but that may be true but have 
							// the AreFieldsInitialized flag be false. It is the latter that determines 
							// if the fields have been initialized and is what the InitializeFields
							// method sets to true. To be safe I added an assert that IsInitialized is 
							// true in case we get into some weird state where IsInitialized is false 
							// but AreFieldsInitialized is true but this change doesn't make things any 
							// worst because the InitializeFields would have exited if _areFieldsInitialized
							// was true.
							//
							//if (fl.IsInitialized == false)
							if (fl.AreFieldsInitialized == false)
							{
								// JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
								// Added parentCollection parameter
								//fl.InitializeFields(null, this._underlyingSourceItems);
								fl.InitializeFields(null, this._underlyingSourceItems, parentRecordCollection);
							}
							else
							{
								Debug.Assert(fl.IsInitialized);
							}

                            this.InitializeFieldLayout(fl);
                        }
                    }

                    // JJD 5/6/09 - NA 2009 vol 2 - Cross band grouping
                    // Make sure we cleanup any trackers for fieldlayouts
                    // that are no longer being used

					// JJD 10/18/10 - TFS30715
					// Only call BeginInvoke if one isn't already pending
					if (this._dataPresenter != null && !this._dataPresenter.IsSynchronousControl)
					{
						if (this._cleanupTrackersOperationPending == null)
							this._cleanupTrackersOperationPending = this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Send, new GridUtilities.MethodDelegate(this.CleanupUnusedSortGroupByVersionTrackers));
					}
					else
						this.CleanupUnusedSortGroupByVersionTrackers();

                    changeEventsRaised = this.VerifySort();

                    // SSP 3/31/08 - Summaries Functionality
                    // Call OnDataChanged so any functionalities affected by data value get dirtied,
                    // like summaries functionality.
                    // 
					
					
					
                    this.OnDataChanged( DataChangeType.Reset, null, null );

                    // SSP 5/12/10 - TFS32148
                    // Moved the call to EndUpdate from above right before the OnDataChanged call to here.
                    // We need to ensure that the filter version is bumped before we send reset notification
                    // from viewable record collection so that any listeners when they access the vrc,
                    // vrc applies the filters to its records. Otherwise when the filters are applied 
                    // asynchronously, it will send another reset notification.
                    // 
                    // SSP 3/5/09 TFS5842 - Optimization
                    // Moved this here from if block above where records are synced.
                    // 
                    this.EndUpdate( !inSameOrder );

                    // JJD 5/8/09 - TFS17464
                    // Allocate a queue to hold records that are being reused that have 
                    // associated record presenters so we can refresh their cell values
                    // since they may have changed.
                    if (existingRcdsWithPresenters != null)
                    {
                        int cnt = existingRcdsWithPresenters.Count;

                        for (i = 0; i < cnt; i++)
                        {
                            DataRecord record = existingRcdsWithPresenters[i];

                            if (record.AssociatedRecordPresenter != null)
								// SSP 2/2/10 TFS62391
								// Added an overload that takes fromDataSourceReset parameter. Pass true for that.
								// 
                                //record.RefreshCellValues(false, false);
								record.RefreshCellValues( false, false, true, true );
                        }
                    }

                }
                finally
                {
                    // JJD 5/23/08 - BR33317 
                    // Reset the flag flag
                    this._isInReset = false;
                }
			}
 
			#region Raise events

			this.RaisePropertyChangedEvent("Unsorted");
			this.RaisePropertyChangedEvent("Sorted");
			this.RaisePropertyChangedEvent("Groups");
			this.RaisePropertyChangedEvent("Current");

			this._unsortedRecords.RaiseChangeEvents(true);

			if ( changeEventsRaised == false)
				this._sortedRecords.RaiseChangeEvents(true);

			
			
			
			
			this.OnDataChanged( DataChangeType.Reset, null, null );

            // JJD 12/08/08 - added IsSynchronizedWithCurrentItem property
            // Sync up with current item
            this.SetActiveRecordFromCurrentItem();

			#endregion //Raise events
		}

                #endregion //OnSourceCollectionReset

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
                #region OnSortGroupByVersionChanged

        private void OnSortGroupByVersionChanged()
        {
			// JJD 10/18/10 - TFS30715
			// Clear pending operation
			this._trackSortGroupOperationPending = null;

			this.VerifySort();
        }

                #endregion //OnSortGroupByVersionChanged	
    
                #region PostDelayedReset

        private void PostDelayedReset()
        {
            if (this._unsortedRecords.Count == 0 || this._sourceItems == null )
                this.OnSourceCollectionReset();
            else
			if ( !this.IsResetPending )
			{
                if (this._dataPresenter != null)
                {
                    // JJD 3/29/08 - added support for printing.
                    // We can't do asynchronous operations during a print
                    //
                    // MBS 7/29/09 - NA9.2 Excel Exporting
                    //if (this._dataPresenter.IsReportControl)
                    if(this._dataPresenter.IsSynchronousControl)
                        this.OnSourceCollectionReset();
                    else
                        this._resetOperationPending = this._dataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new GridUtilities.MethodDelegate(OnDelayedReset));
                }
                else
                {
                    Debug.Fail("Should have a data presenter here.");
                    this.OnSourceCollectionReset();
                }
			}
        }

                #endregion //PostDelayedReset	
    
				// JJD 7/9/07
				// Added support for handling change notifications on another thread 
				#region ProcessChangeNotification

		private void ProcessChangeNotification(object sender, object eventArgs)
		{
			PropertyChangedEventArgs propchange = eventArgs as PropertyChangedEventArgs;

			if (propchange != null)
			{
				DataRecord dr = sender as DataRecord;

				Debug.Assert(dr != null);

				if (dr != null && !dr.IsDeleted)
				{
					// JM 11-19-08 TFS10468 - INotifyPropertyChanged.PropertyChangedEvent conventions require that when the property name = string.Empty
					//						  this be interpreted as 'all properties have changed'.
					if (propchange.PropertyName == string.Empty)
					{
						// SSP 3/3/09 TFS11407
						// Added raiseInitializeRecord parameter.
						// 
						//dr.RefreshCellValues(true);
						dr.RefreshCellValues(true, true);

						// SSP 2/3/10 TFS26323
						// Dirty any affected summaries as well as filter states.
						// 
						this.OnDataChanged(DataChangeType.RecordDataChange, dr, null);
					}
					else
					{
						// SSP 3/3/09 TFS11407
						// Added raiseInitializeRecord parameter.
						// 
						//dr.RefreshCellValue(propchange.PropertyName);
						
						// JJD 04/12/12 - TFS108549 - Optimization - added isRecordPresenterDeactivated parameter
						//dr.RefreshCellValue(propchange.PropertyName, true);
						RecordPresenter rp = dr.AssociatedRecordPresenter;
						dr.RefreshCellValue(propchange.PropertyName, true, rp != null && rp.IsDeactivated);
					}
				}

				return;
			}

			NotifyCollectionChangedEventArgs collChange = eventArgs as NotifyCollectionChangedEventArgs;

			if (collChange != null)
			{
				this.OnSourceCollectionChanged(sender, (NotifyCollectionChangedEventArgs)eventArgs);
				return;
			}

			ListChangedEventArgs listChange = eventArgs as ListChangedEventArgs;

			if (listChange != null)
			{
				this.OnBindingListChanged(sender, (ListChangedEventArgs)eventArgs);
				return;
			}

			Debug.Fail("Unknown event args in RecordManager.ProcessChangeNotification"); 
		}

				#endregion //ProcessChangeNotification	

                // JJD 2/20/09 - TFS6106/BR33996 - moved logic into this helper method
                #region RemoveRecordHelper

        private void RemoveRecordHelper(int index)
        {
            bool activeRecordDeleted = false;
            int activeRecordSortedIndex = -1;

            DataRecord record = this._unsortedRecords.SparseArray.GetItem(index, false) as DataRecord;

            if (record != null && record.IsActive)
            {
                activeRecordSortedIndex = record.SortIndex;

                // JJD 12/2/06 - TFS6743/BR35763
                // Check if the active cell is in edit mode and
                // if is is force it out of edit mode since the record is
                // being deleted.
                Cell activeCell = this._dataPresenter.ActiveCell;
                if (activeCell != null &&
                     activeCell.IsInEditMode)
                {
                    activeCell.EndEditMode(false, true);
                }
            }

            if (record != null)
            {
				// JJD 11/30/10 - TFS31984 
				// Adjust the scroll position to maintain current records in view
				if (this._dataPresenter.ScrollBehaviorOnListChange == ScrollBehaviorOnListChange.PreserveRecordsInView &&
					record.IsFixed == false)
				{
					int rcdScrollCount = record.ScrollCountInternal;

					if (rcdScrollCount > 0 &&
						record.OverallScrollPosition <= ((IViewPanelInfo)(this._dataPresenter)).OverallScrollPosition)
					{
						((IViewPanelInfo)(this._dataPresenter)).OverallScrollPosition -= rcdScrollCount;
					}
				}

				// JJD 1/8/08 - BR29410
                // If the record is selected then remove it from selected records collection
				if (record.IsSelected)
				{
					this._dataPresenter.SelectedItems.Records.Remove(record);

					// JJD 11/22/11 - TFS79023
					// Raise the SelectedItemsChanged event asynchronously
					this._dataPresenter.RaiseSelectedItemsChangedAsync(typeof(Record));
				}
				else
				{
					// JJD 1/8/08 - BR29410
					// Since selected records and cells are mutually exclusive we only
					// need to check the SelectedCells collection if the record isn't selelcted
					SelectedCellCollection selectedCells = this._dataPresenter.SelectedItems.Cells;

					int countOfCells = selectedCells.Count;

					// JJD 11/22/11 - TFS79023
					//
					bool selectedCellsFound = false;

					// JJD 1/8/08 - BR29410
					// Loop over the selected cells backwards to remove any cells from this record
					for (int i = countOfCells - 1; i >= 0; i--)
					{
						Cell cell = selectedCells[i];

						if (cell != null && cell.Record == record)
						{
							selectedCells.RemoveAt(i);

							// JJD 11/22/11 - TFS79023
							// Set the flag so we know to raise the event below
							selectedCellsFound = true;
						}
					}

					// JJD 11/22/11 - TFS79023
					// Raise the SelectedItemsChanged event asynchronously
					if ( selectedCellsFound )
						this._dataPresenter.RaiseSelectedItemsChangedAsync(typeof(Cell));
				}
            }

            this.RemoveAt(index, true);

            if (record != null)
            {
                // JJD 2/20/09 
                // Cache the IsActive state since once we call InternalSetIsDeleted
                // IsActive will return false
                bool wasActive = record.IsActive;

                if (!record.IsAddRecord)
                {
                    record.InternalSetIsDeleted();

                    // SSP 2/11/09 TFS12467
                    // Dirtying parent records's scroll count won't help if the this record's parent 
                    // viewable record collection's scroll count is not re-calculated properly. So 
                    // dirty that instead by dirtying the scroll count of this record which will 
                    // cause its parent viewable record collection to recalculate its scroll count 
                    // which in turn will dirty the parent record's scroll count.
                    // 
                    // ------------------------------------------------------------------------------
                    //if (record.ParentRecord != null)
                    //	record.ParentRecord.DirtyScrollCount();
                    record.DirtyScrollCount();
                    // ------------------------------------------------------------------------------
                }

                // JJD 2/20/09 
                // Used the wasActive state cached above
                //if (record.IsActive && !record.IsAddRecord)
                if (wasActive && !record.IsAddRecord)
                {
                    activeRecordDeleted = true;
                    this._dataPresenter.ActiveRecord = null;
                }
            }

            this.BumpUnderlyingDataVersion();

            // when the add record is tranmsitioning from one state 
            // to another (template -> real record or vice versa) we
            // shouldn't pass any notification to the visible records 
            // collection since from its perspecitive noting has changed
            if (record != null && record.IsAddRecord)
            {
                // JJD 11/24/08 - TFS6743/BR35763
                // Since the notification sequence is different when dealing with an editable collection view
                // we need to clear the cell values hete when the add record is being deleted so call
                // PrepareForNewCurrentAddRecord which set the addrecord back to its original state
                if (this._editableCollectionView != null)
                {
                    record.SetDataItemHelper(null);
                    record.InternalSetDataChanged(false);
                    this.PrepareForNewCurrentAddRecord();
                }
                
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            }

            
            
            
            
            this.OnDataChanged( DataChangeType.RemoveRecord, record, null );

            // reselect next record after deletion unless this was done externally
            if (activeRecordDeleted == true &&
                this._recordBeingDeleted == null)
                this.ReselectRecordAtSortedIndex(activeRecordSortedIndex);
        }

                #endregion //RemoveRecordHelper	

				#region SortViaCollectionViewHelper

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		private bool SortViaCollectionViewHelper( )
		{
			bool sortUsingCollectionView = SortEvaluationMode.UseCollectionView == this.SortEvaluationModeResolved;
			bool groupUsingCollectionView = GroupByEvaluationMode.UseCollectionView == this.GroupByEvaluationModeResolved;

			if ( null != _sortedFieldsToCollectionViewSynchronizer && null != _fieldLayout
				&& ( _fieldLayout != _sortedFieldsToCollectionViewSynchronizer._fieldLayout
					// If the field-layout is non-null and sort evaluation mode is set to not use collection view then
					// dispose of any previously allocated _sortedFieldsToCollectionViewSynchronizer.
					// 
					|| !sortUsingCollectionView && !groupUsingCollectionView
				) 
			   )
			{
				_sortedFieldsToCollectionViewSynchronizer.Dispose( );
				_sortedFieldsToCollectionViewSynchronizer = null;
			}

			if ( sortUsingCollectionView || groupUsingCollectionView )
			{
				if ( null == _sortedFieldsToCollectionViewSynchronizer )
					_sortedFieldsToCollectionViewSynchronizer = SortedFieldsToCollectionViewSynchronizer.CreateHelper( this );

				_sortedFieldsToCollectionViewSynchronizer.Synchronize( true );
				return true;
			}

			return false;
		}

				#endregion // SortViaCollectionViewHelper

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
                #region SyncSortGroupByVersions

        private void SyncSortGroupByVersions()
        {
            if (this._sortGroupByVersionTrackers != null &&
                 this._sortGroupByVersionTrackers.Count > 0)
            {
                foreach (SortGroupVersionTracker tracker in this._sortGroupByVersionTrackers.Values)
                {
                    if (tracker != null)
                        tracker.SyncVersionNumbers();
                }
            }
        }

                #endregion //SyncSortGroupByVersions	
        
    
				// AS 5/19/09 TFS17455
				#region UpdateSelectedItems

		// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
		// 
		//private static void UpdateSelectedItems(DataPresenterBase dp, Hashtable dataRecordHolderHash, int nonNullCount, int originalHashCount)
		private static void UpdateSelectedItems( DataPresenterBase dp, ICollection<DataRecord> discardedRecords )
		{
			// SSP 2/11/10 - TFS26273 - Added DataSourceResetBehavior property
			// 
			// ------------------------------------------------------------------------
			if ( dp == null )
				return;

			DataPresenterBase.SelectedItemHolder holder = dp.SelectedItems;
			bool hasRecords = holder.HasRecords;

			// if there aren't any selected records or cells then we don't need to process anything
			if ( !hasRecords && !holder.HasCells )
				return;

			int discardedRecordsCount = null != discardedRecords ? discardedRecords.Count : 0;
			int potentialDiscardedSelectedItemsCount = hasRecords ? discardedRecordsCount : discardedRecordsCount * 5;
			int selectionCount = hasRecords ? holder.Records.Count : holder.Cells.Count;

			// Compare the number of records/cells we have in discarded records against the number of items 
			// in the selection to determine whether we should loop through the discarded items or the selected
			// items.
			// 
			if ( null != discardedRecords && selectionCount > potentialDiscardedSelectedItemsCount )
			{
				List<object> invalidSelectedItems = new List<object>( );

				SelectedItemCollectionBase selectedCollection = hasRecords ? holder.Records : (SelectedItemCollectionBase)holder.Cells;
				selectedCollection.BeginUpdate( );

				try
				{
					foreach ( DataRecord dr in discardedRecords )
					{
						UnselectItems( !hasRecords, dr, holder );
					}
				}
				finally
				{
					selectedCollection.EndUpdate( );
				}
			}
			
#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------
			else
			{
				if (hasRecords)
				{
					List<Record> selectedRecords = new List<Record>();

					foreach (Record record in holder.Records)
					{
						if (record.IsStillValid)
							selectedRecords.Add(record);
					}

					if (selectedRecords.Count == 0)
						// JJD 1/16/12 - TFS63720
						// Pass false in as new resetItemSelectionStates parameter
						//dp.SelectedItems.InternalClearSelection(true, true, false);
						dp.SelectedItems.InternalClearSelection(true, true, false, false);
					else
						dp.SelectedItems.Records.InternalAddRange(selectedRecords, true);
				}
				else
				{
					List<Cell> selectedCells = new List<Cell>();
					DataRecord previousRecord = null;
					bool isValid = false;

					foreach (Cell cell in holder.Cells)
					{
						DataRecord cellRecord = cell.Record;

						if (cellRecord != previousRecord)
						{
							previousRecord = cellRecord;
							isValid = cellRecord.IsStillValid;
						}

						if (isValid)
							selectedCells.Add(cell);
					}

					if (selectedCells.Count == 0)
						// JJD 1/16/12 - TFS63720
						// Pass false in as new resetItemSelectionStates parameter
						//dp.SelectedItems.InternalClearSelection(true, true, false);
						dp.SelectedItems.InternalClearSelection(true, true, false, false);
					else
						dp.SelectedItems.Cells.InternalAddRange(selectedCells, true);
				}
			}
		}
				#endregion //UpdateSelectedItems

				// AS 5/19/09 TFS17455
				#region UnselectItems
		private static void UnselectItems(bool cells, Record record, DataPresenterBase.SelectedItemHolder selected)
		{
			if (cells)
			{
				DataRecord dataRecord = record as DataRecord;

				if (null != dataRecord)
				{
					FieldCollection fields = dataRecord.FieldLayout.Fields;

					for (int i = 0, count = fields.Count; i < count; i++)
					{
						Cell cell = dataRecord.GetCellIfAllocated(fields[i]);

						if (null != cell && cell.IsSelected)
							selected.Cells.InternalRemove(cell);
					}
				}
			}
			else if (record.IsSelected)
			{
				selected.Records.InternalRemove(record);
			}

			if (record.HasChildrenInternal)
			{
				foreach (Record child in record.ChildRecordsInternal.SparseArray.NonNullItems)
				{
					UnselectItems(cells, child, selected);
				}
			}
		} 
				#endregion //UnselectItems

                // JJD 11/17/08 - TFS6743/BR35763 - added
                #region VerifyListSourceToUse

        // returns true if there was a change applied
        private bool VerifyListSourceToUse()
        {
            IList list = null;
            IBindingList bindingList = null;
            CollectionViewWrapper collectionViewWrapper = null;

            
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


            // JJD 11/17/08 - TFS6743/BR35763 
            // Get the proxy if the source implements the IEditableCollectionView interface
            // JJD 11/24/09 - TFS25123
            // Use the _collectionView member to get the proxy instead
            //EditableCollectionViewProxy proxy = this.GetEditableCollectionViewProxy(this._sourceItems);
			// JJD 09/22/11  - TFS84708 - Optimization 
			// Made internal static method so we could call it from the ExpandableFieldRecord.
			// Also added existingProxy parameter since this is now a static method
			//EditableCollectionViewProxy proxy = this.GetEditableCollectionViewProxy(this._collectionView);
            EditableCollectionViewProxy proxy = GetEditableCollectionViewProxy(this._collectionView, _editableCollectionView);

            bool useCollectionView = proxy != null;
            
            // JJD 11/24/09 - TFS25123
            // Use the _collectionView member instead
            //CollectionView cview = this._sourceItems as CollectionView;
            CollectionView cview = this._collectionView as CollectionView;
            
            if (useCollectionView == false)
            {
                // Since we don't have a proxy only use the CollectionView when
                // the view is sorted or filtered 
                if (this._collectionView != null && 
					// JJD 06/28/10 - TFS34795
					// Before trying to access the Filter or SortDescriptions collection make
					// sure that the collection view suports that capability. Otherwise
					// an exception may be thrown.
                    //(this._collectionView.Filter != null || this._collectionView.SortDescriptions.Count > 0))
                    ((this._collectionView.CanFilter && this._collectionView.Filter != null) || 
					 (this._collectionView.CanSort && this._collectionView.SortDescriptions.Count > 0))
					// SSP 8/18/10 TFS28525
					// CompositeCollection's indexers and Count do not reflect the actual items in
					// the view and therefore we can't make use of it either for retrieving data items.
					// 
					|| _underlyingSourceItems is CompositeCollection
					)
                    list = null;
                else
                {
                    list = this._underlyingSourceItems as IList;
                    bindingList = this._underlyingSourceItems as IBindingList;

                    if (list == null)
                        list = this._collectionView as IList;
                }
            }

            // If we are using the collectionview then we need to create the wrapper
            // which implements IList
            if (useCollectionView)
            {
                if (this._collectionViewWrapper != null &&
                     this._collectionViewWrapper.View == cview &&
                     this._collectionViewWrapper.EditableCollectionViewProxy == proxy)
                    collectionViewWrapper = this._collectionViewWrapper;
                else
                    collectionViewWrapper = new CollectionViewWrapper(cview, proxy);

                list = collectionViewWrapper;
            }

            // JJD 5/22/09 - TFS17910
            // Moved stack variable from below
            INotifyCollectionChanged newCollectionChangedNotifier = collectionViewWrapper != null ? collectionViewWrapper : this._sourceItems as INotifyCollectionChanged;

            // If everything is the same then return false
            if (list == this._list &&
                 bindingList == this._bindingList &&
                 proxy == this._editableCollectionView &&
                 collectionViewWrapper == this._collectionViewWrapper &&
                 // JJD 5/22/09 - TFS17910
                 // compare the new and old INotifyCollectionChanged as well.
                 // This allows us to support INotifyCollectionChanged implementations
                 // other than CollectionView
                 newCollectionChangedNotifier == this._notifyCollectionChangedHooked)
            {
                // if the data source doesn't allow addnew make sure we 
                // clear any old one out
                if (this.DataSourceAllowsAddNew == false)
                    this.ClearAddRecordOnDataSourceChange();
                return false;
            }

			// JJD 11/18/11 - TFS79001
			// Reset the cached item type 
			_typeWithPublicCtor = null;

            this._list = list;
            this._editableCollectionView = proxy;
            this._collectionViewWrapper = collectionViewWrapper;

            // JJD 5/9/07 - BR22698
            // Added support for listening to data item's PropertyChanged event
            // if the datasource doesn't supply cell value change notifications thru IBindlingList
            //
            // Initialize the member variable to false
            this._dataSourceRaisesCellValueChangeEvents = false;

            if (bindingList != this._bindingList)
            {
                // JJD 5/9/07
                // Always hook and unhook the BindingListChanged event since the SupportsChangeNotification
                // property setting can be changed
                //if (this._bindingList != null &&
                //    this._bindingList.SupportsChangeNotification)
                if (this._bindingList != null)
                    BindingListChangedEventManager.RemoveListener(this._bindingList, this);

                this._bindingList = bindingList;

                // First try hooking into the ListChanged event if the source 
                // implements IBindingList
                if (this._bindingList != null)
                {
                    // JJD 5/9/07 - BR22698
                    // Added support for listening to data item's PropertyChanged event
                    // if the datasource doesn't supply cell value change notifications thru IBindlingList
                    //
                    // Cache whether the datasource will raise the cell value change events
                    IRaiseItemChangedEvents raiseChangeEvents = this._underlyingSourceItems as IRaiseItemChangedEvents;

                    // JJD 11/17/08 - TFS6743/BR35763 
                    // Only set the _dataSourceRaisesCellValueChangeEvents to true if the source items object
                    // is not a CollectionView. This is because if they subsequently sort or filter the
                    // view we will no longer use the underlying IBindingList so ll the record's need
                    // to hook into their respective INotifyPropertyChange events
                    if (cview == null)
                        this._dataSourceRaisesCellValueChangeEvents = raiseChangeEvents == null || raiseChangeEvents.RaisesItemChangedEvents;

                    // JJD 5/9/07
                    // Always hook the BindingListChanged event since the SupportsChangeNotification
                    // property setting can be changed
                    //if (this._bindingList.SupportsChangeNotification)
                    BindingListChangedEventManager.AddListener(this._bindingList, this);
                }
            }

            // JJD 5/22/09 - TFS17910
            // Moved stack variable up
            //INotifyCollectionChanged newCollectionChangedNotifier = this._collectionViewWrapper != null ? this._collectionViewWrapper : this._sourceItems as INotifyCollectionChanged;

            // JJD 5/22/09 - TFS17691
            //if (newCollectionChangedNotifier != oldCollectionChangedNotifier)
            if (newCollectionChangedNotifier != this._notifyCollectionChangedHooked)
            {
                // unhook the old one
                if ( this._notifyCollectionChangedHooked != null )
                    CollectionChangedEventManager.RemoveListener(this._notifyCollectionChangedHooked, this);

                // hook into the CollectionChanged event
                if (newCollectionChangedNotifier != null)
                {
                    // JJD 5/22/09 - TFS17691
                    // Changed from bool to the object that was wired
                    //this._notifyCollectionChangedHooked = true;
                    CollectionChangedEventManager.AddListener(newCollectionChangedNotifier, this);
                }
//                else
//                    this._notifyCollectionChangedHooked = false;

                // JJD 5/22/09 - TFS17691
                // Changed from bool to the object that was wired
                this._notifyCollectionChangedHooked = newCollectionChangedNotifier;
            }

            ITypedList typedList = null;
			
			// JJD 05/08/12 - TFS110865
			// clear the _typeIsKnownType flag
			_typeIsKnownType = false;

            // JJD 2/6/09 - TFS13615
            // Cache a flag so we know if we can get property descriptors from the datasource
            // even if there are no items in the list
            if (this._underlyingSourceItems == null)
                this._canGetPropertiesFromDataSourceWithoutItems = false;
            else
            {
                Type itemType;

                typedList = PropertyDescriptorProvider.GetTypedListFromContainingList(this._underlyingSourceItems, out itemType);

                this._canGetPropertiesFromDataSourceWithoutItems = typedList != null || itemType != null;

				// JJD 11/18/11 - TFS79001
				// Keep track of the items type if it has a public parameterless constructor
				if (itemType != null && !itemType.IsAbstract)
				{
					// JJD 05/08/12 - TFS110865
					// set the _typeIsKnownType flag
					_typeIsKnownType = Utilities.IsKnownType(itemType);

					if (itemType.IsPrimitive || 
						itemType == typeof(string) || // JJD 05/08/12 - TFS110865 - special case strings
						itemType.GetConstructor(BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, Type.EmptyTypes, null) != null)
						_typeWithPublicCtor = itemType;
				} 
            }

            // JJD 5/11/09 - NA 2009 vol 2 - Cross band grouping
            // If the datasource implements ITypedList then preprosss the structure so we 
            // can initalize the FieldLayouts' ParentFieldName and ParentFieldLayoutKey 
            // properties appropriately 
            // Note: we are calling the InitializeFieldLayoutStructureFromTypedList method
            // even if typedList is null if this is the root RecordManager so we can
            // clear any cached info
			if (this._parentRecord == null &&
				this._dataPresenter != null)
			{
				// JJD 11/24/10 - TFS59031
				// Cache the provider for use when there aren't any rcds
				//this._dataPresenter.FieldLayouts.InitializeFieldLayoutStructureFromTypedList(typedList);
				this._typedListDefaultProvider = this._dataPresenter.FieldLayouts.InitializeFieldLayoutStructureFromTypedList(typedList);
			}
			else
			{
				// JJD 11/24/10 - TFS59031
				// Clear the cached value
				this._typedListDefaultProvider = null;
			}

            // if the data source doesn't allow addnew make sure we 
            // clear any old one out.
            // Pass true in for the 2nd param so we suppress notifications here
            if (this.DataSourceAllowsAddNew == false)
                this.ClearAddRecordOnDataSourceChange();

            // return true since there was some change of state
            return true;

        }

                #endregion //VerifyListSourceToUse	
    
                // JJD 1/10/08 - BR29572
                #region VerifyRootDataSource

        private void VerifyRootDataSource()
        {
            // JJD 1/10/08 - BR29572
            // If we aren't the root record manager then bail
            if (this._parentRecord != null ||
                this._dataPresenter == null)
                return;

            // make sure we are hooked up
            if (this.DataSource == null)
                this._dataPresenter.VerifyRecordManagerDataSource();


        }

              #endregion //VerifyRootDataSource	
        
            #endregion //Private Methods

        #endregion //Methods

		#region BeginUpdateInformation Class

		// SSP 2/11/09 TFS12467
		// 
		internal class BeginUpdateInformation
		{
			#region PendingNotificationInfo Class

			private class PendingNotificationInfo
			{
				internal ViewableRecordCollection _vrc;
				internal EventArgs _eventArgs;

				internal PendingNotificationInfo( ViewableRecordCollection vrc, EventArgs eventArgs )
				{
					_vrc = vrc;
					_eventArgs = eventArgs;
				}
			}

			#endregion // PendingNotificationInfo Class

			#region Member Vars

			private RecordManager _recordManager;
			internal int _beginUpdateCount;
			private HashSet _pendingResetVRCs;
			private List<PendingNotificationInfo> _pendingNotificationList;
			internal bool _dirtyScrollCount;
			internal bool _scrollCountDirtied;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			internal BeginUpdateInformation( RecordManager recordManager )
			{
				_recordManager = recordManager;
			}

			#endregion // Constructor

			#region Properties

			#region PendingNotificationList

			private List<PendingNotificationInfo> PendingNotificationList
			{
				get
				{
					if ( null == _pendingNotificationList )
						_pendingNotificationList = new List<PendingNotificationInfo>( );

					return _pendingNotificationList;
				}
			}

			#endregion // PendingNotificationList

			#region PendingResetVRCs

			private HashSet PendingResetVRCs
			{
				get
				{
					if ( null == _pendingResetVRCs )
						_pendingResetVRCs = new HashSet( );

					return _pendingResetVRCs;
				}
			}

			#endregion // PendingResetVRCs

			#endregion // Properties

			#region Methods

			#region EnqueNotification

			internal void EnqueNotification( ViewableRecordCollection vrc, EventArgs eventArgs )
			{
				if ( eventArgs is NotifyCollectionChangedEventArgs
					&& NotifyCollectionChangedAction.Reset == ( (NotifyCollectionChangedEventArgs)eventArgs ).Action )
				{
					this.PendingResetVRCs.Add( vrc );
				}

				this.PendingNotificationList.Add( new PendingNotificationInfo( vrc, eventArgs ) );
			}

			#endregion // EnqueNotification

			#region HasPendingResetNotification

			internal bool HasPendingResetNotification( ViewableRecordCollection vrc )
			{
				return null != _pendingResetVRCs && _pendingResetVRCs.Exists( vrc );
			}

			#endregion // HasPendingResetNotification

			#region ProcessPendingNotifications

			internal void ProcessPendingNotifications( )
			{
				ViewableRecordCollection topLevelVRC = _recordManager.ViewableRecords;
				bool scrollCountDirtiedProcessedOnTopLevelVRC = false;

				List<PendingNotificationInfo> pendingNotifications = _pendingNotificationList;
				if ( null != pendingNotifications && pendingNotifications.Count > 0 )
				{
					// Multiple events for the same viewable record collection should be raised
					// as a single reset notification.
					// 
					HashSet tmp = new HashSet( );
					for ( int i = 0, count = pendingNotifications.Count; i < count; i++ )
					{
						PendingNotificationInfo ii = pendingNotifications[i];
						if ( tmp.Exists( ii._vrc ) )
							this.PendingResetVRCs.Add( ii._vrc );
						else
							tmp.Add( ii._vrc );
					}

					for ( int i = 0, count = pendingNotifications.Count; i < count; i++ )
					{
						PendingNotificationInfo ii = pendingNotifications[i];

						if ( null == _pendingResetVRCs || ! _pendingResetVRCs.Exists( ii._vrc ) )
						{
							if ( ii._eventArgs is NotifyCollectionChangedEventArgs )
							{
								ii._vrc.RaiseCollectionChanged( (NotifyCollectionChangedEventArgs)ii._eventArgs );
							}
							else if ( ii._eventArgs is PropertyChangedEventArgs )
							{
								ii._vrc.RaisePropertyChangedEvent( ( (PropertyChangedEventArgs)ii._eventArgs ).PropertyName );
							}
							else
							{
								Debug.Assert( false, "Unknown type of event args." );
								this.PendingResetVRCs.Add( ii._vrc );
							}
						}
					}

					if ( null != _pendingResetVRCs )
					{
						foreach ( ViewableRecordCollection vrc in _pendingResetVRCs )
						{
							MainRecordSparseArray sparseArr = vrc.RecordCollectionSparseArray;
							if ( null != sparseArr )
							{
								sparseArr.DirtyScrollCount( );
								if ( vrc == topLevelVRC )
									scrollCountDirtiedProcessedOnTopLevelVRC = true;
							}

							vrc.RaiseChangeEvents( true );
						}
					}
				}

				if ( null != topLevelVRC && _scrollCountDirtied && ! scrollCountDirtiedProcessedOnTopLevelVRC )
					topLevelVRC.OnScrollCountDirtiedHelper( );
			}

			#endregion // ProcessPendingNotifications

			#endregion // Methods
		}

		#endregion // BeginUpdateInformation Class

		#region RecordsSortComparer Class

		internal class RecordsSortComparer : IComparer, IComparer<Record>
		{
			#region Private Members

			private bool _originalUnsortedOrder;
			private Dictionary<Field, IGroupByEvaluator> _groupByEvaluators;

			// SSP 4/1/09 Cell Text
			// 
			private Dictionary<Field, CellTextConverterInfo> _cellTextConverters = new Dictionary<Field,CellTextConverterInfo>( );

			// SSP 3/9/10 TFS25402
			// 
			private Dictionary<FieldLayout, SameFieldRecordsSortComparer> _sameFieldRecordComparers = new Dictionary<FieldLayout, SameFieldRecordsSortComparer>( );

			#endregion //Private Members

			#region Constructor

			internal RecordsSortComparer( bool originalUnsortedOrder )
			{
				this._originalUnsortedOrder = originalUnsortedOrder;
			}

			#endregion //Constructor

			#region ConvertToEditAsTypeHelper

            
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)


			#endregion // ConvertToEditAsTypeHelper

			#region GetGroupByEvaluator

			// JJD 2/23/07 - BR19943
			// Added support for groupby evaluators to specify their own sort comparer
			private IGroupByEvaluator GetGroupByEvaluator( Field field )
			{
				IGroupByEvaluator evaluator = null;

				if ( this._groupByEvaluators == null )
					this._groupByEvaluators = new Dictionary<Field, IGroupByEvaluator>( );
				else
				{
					this._groupByEvaluators.TryGetValue( field, out evaluator );

					if ( evaluator != null )
						return evaluator;
				}

				evaluator = field.GroupByEvaluatorResolved;

				if ( evaluator != null )
					this._groupByEvaluators.Add( field, evaluator );

				return evaluator;
			}

			#endregion //GetGroupByEvaluator

			#region DefaultCompare static method



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

			internal static int DefaultCompare( object x, object y, Field field )
			{
				return RecordManager.RecordsSortComparer.DefaultCompare( x, y, true, field );
			}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			internal static int DefaultCompare( object x, object y, bool duringSort, Field field )
			{
				Debug.Assert( null != field );
				return DefaultCompare( x, y, duringSort, null != field && FieldSortComparisonType.CaseInsensitive == field.SortComparisonTypeResolved );
			}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			internal static int DefaultCompare( object x, object y, bool duringSort, bool caseInSensitive )
			{
				// SSP 2/23/09 TFS13975
				// Use the DefaultCompare of the SameFieldRecordsSortComparer instead of duplicating the
				// code.
				// 
				// ------------------------------------------------------------------------------------------
				return SameFieldRecordsSortComparer.DefaultCompare( x, y, duringSort, caseInSensitive );
				
#region Infragistics Source Cleanup (Region)





































































































































































































#endregion // Infragistics Source Cleanup (Region)

				// ------------------------------------------------------------------------------------------
			}

			#endregion //DefaultCompare static method

			#region IComparer.Compare






			int IComparer.Compare( object x, object y )
			{
				DataRecord recordX = x as DataRecord;
				DataRecord recordY = y as DataRecord;

				if ( recordX == recordY )
					return 0;

				#region CompareFixedStatus


                // JJD 6/23/09 - NA 2009 Vol 2 - Record fixing
                // Compare based on the FixedLocation property
				// SSP 9/3/09
				// Check for records being null.
				// 
                //FixedRecordLocation fixedLocationX = recordX.FixedLocation;
                //FixedRecordLocation fixedLocationY = recordY.FixedLocation;
				FixedRecordLocation fixedLocationX = null != recordX ? recordX.FixedLocation : FixedRecordLocation.Scrollable;
				FixedRecordLocation fixedLocationY = null != recordY ? recordY.FixedLocation : FixedRecordLocation.Scrollable;

                // fixed records should always sort above or below the non-fixed records
                if (fixedLocationX != FixedRecordLocation.Scrollable || fixedLocationY != FixedRecordLocation.Scrollable)
                {
                    if (fixedLocationX != fixedLocationY)
                    {
                        if (fixedLocationX == FixedRecordLocation.FixedToTop)
                            return -1;

                        if (fixedLocationX == FixedRecordLocation.FixedToBottom)
                            return 1;

                        // at this point fixedLocationX is scrollable so check the fixedLocationY
                        if (fixedLocationY == FixedRecordLocation.FixedToTop)
                            return 1;
                        else
                            return -1;
                    }
                }

				#endregion //CompareFixedStatus

				#region Handle the case where one record is null

				FieldLayout flX;
				FieldLayout flY;

				if ( recordX == null || recordY == null )
				{
					if ( null != recordX )
					{
						flX = recordX.FieldLayout;

						if ( flX.HasSortedFields && flX.SortedFields[0].Direction == ListSortDirection.Descending )
							return 1;

						return -1;
					}

					if ( null != recordY )
					{
						flY = recordY.FieldLayout;
						if ( flY.HasSortedFields && flY.SortedFields[0].Direction == ListSortDirection.Descending )
							return -1;

						return 1;
					}
				}

				#endregion //Handle the case where one record is null

				int compare = 0;

				if ( this._originalUnsortedOrder == false )
				{
					#region Handle the case where the field layouts don't match

					flX = recordX.FieldLayout;
					flY = recordY.FieldLayout;

					// JJD 2/23/07
					// Compare fieldlayouts by their index which would normally
					// be by the order that the records were encountered. This is
					// also something that can be controlled by the application developer
					if ( flX != flY )
						return flX.Index < flY.Index ? -1 : 1;
					//return String.Compare(flX.Description, flY.Description, true);

					#endregion //Handle the case where the field layouts don't match

					#region Compare based on sort criteria

					// SSP 3/9/10 TFS25402
					// 
					// ------------------------------------------------------------------------------------------
					
					SameFieldRecordsSortComparer recordComparer;
					if ( ! _sameFieldRecordComparers.TryGetValue( flX, out recordComparer ) )
					{
						recordComparer = new SameFieldRecordsSortComparer( flX, 0, true );
						_sameFieldRecordComparers[flX] = recordComparer;
					}

					if ( null != recordComparer )
						compare = recordComparer.Compare( recordX, recordY );

					
#region Infragistics Source Cleanup (Region)































































































#endregion // Infragistics Source Cleanup (Region)

					// ------------------------------------------------------------------------------------------

					#endregion //Compare based on sort criteria
				}

				// Instead of returning 0 here which will cause the Array.Sort method
				// to put equal records any way it wishes, return -1 or 1 based on the
				// index so that it will put the record with higher list index after the
				// record with lower list index. The result of this will be that when
				// a field is sorted by and all the cells of that field are blank,
				// the sorting will retain records' positions. Before Array.Sort was 
				// reordering the records even when all the records were equal randomly
				// so to the user it would look as if grid sorted even if it did not 
				// need to sort.
				//
				if ( 0 == compare )
					compare = recordX.DataItemIndex < recordY.DataItemIndex ? -1 : 1;

				return compare;
			}

			#endregion //IComparer.Compare

			#region IComparer<Record> Members

			/// <summary>
			/// Compares 2 items
			/// </summary>
			public int Compare( Record x, Record y )
			{
				return ( (IComparer)this ).Compare( x, y );
			}

			#endregion
		}

		#endregion // RecordsSortComparer Class

        #region RecordsSortComparer Class

        internal class SameFieldRecordsSortComparer : IComparer, IComparer<Record>
		{
			#region CellInfo Class

			internal class CellInfo
			{
				internal DataRecord _record;
				private int _dataItemIndex = -1;
				internal object _value;

				// SSP 9/18/08 BR34967
				// 
				//internal string _text;
				private object _cachedCompareValue;

				// SSP 5/29/09 - TFS17233 - Optimization
				// 
				private FieldSortInfo _fieldSortInfo;
				private string _text;

				// SSP 5/29/09 - TFS17233 - Optimization
				// 
				//internal CellInfo( DataRecord record, Field field )
				internal CellInfo( DataRecord record, FieldSortInfo info )
				{
					_record = record;
					_fieldSortInfo = info;

					// SSP 5/29/09 - TFS17233 - Optimization
					// Store the _value converted to EditAsType. We were converting in the GetCompareValue
					// however moved that logic in here since we don't ever need the non-converted value.
					// 
					// ----------------------------------------------------------------------------------------
					//_value = record.GetCellValue( field, true );

					// If EditAsType and DataType are different then convert the value to the EditAsType for
					// comparison purposes. The scenario is that if you are bound to XML data source with a
					// field that contains numeric field however the property descriptor's data type happens
					// to be string, then we'll end up sorting them as strings instead of numeric values
					// even if EditAsType is set to a numeric type. Therefore we have to convert to the 
					// EditAsType.
					// 
					object val = record.GetCellValue( info._field, true );
					if ( info._editAsTypeUnderlying != info._dataTypeUnderlying && null != val && !( val is DBNull ) )
						_value = Utilities.ConvertDataValue( val, info._editAsTypeUnderlying, info._converterCultureResolved, null );
					else
						_value = val;
					// ----------------------------------------------------------------------------------------
				}

				internal int DataItemIndex
				{
					get
					{
						if ( _dataItemIndex > 0 )
							return _dataItemIndex;

						return _dataItemIndex = _record.DataItemIndex;
					}
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// 
				internal string GetCellText( )
				{
					if ( null == _text )
						_text = _fieldSortInfo._cellTextConverter.ConvertCellValue( _value );

					return _text;
				}

				// SSP 9/18/08 BR34967
				// If EditAsType is set on the Field then convert the cell values to that type
				// for comparison purposes.
				// 
				// SSP 5/29/09 - TFS17233 - Optimization
				// Now we are storing a reference to fieldSortInfo in the class.
				// 
				//internal object GetCompareValue( FieldSortInfo info )
				internal object GetCompareValue( )
				{
					if ( null != _cachedCompareValue )
						return _cachedCompareValue;

					// SSP 5/29/09 - TFS17233 - Optimization
					// Now we are storing a reference to fieldSortInfo in the class.
					// 
					FieldSortInfo info = _fieldSortInfo;

					// SSP 4/1/09 Cell Text
					// 
					//bool compareByValue = true;
					bool compareByValue = ! info._cellTextConverter.SortByText;

					if ( compareByValue )
					{
						// SSP 5/29/09 - TFS17233 - Optimization
						// Moved the logic to convert to EditAsType in the constructor. Now the _value is 
						// already converted to EditAsType.
						// 
						// ------------------------------------------------------------------------------------
						_cachedCompareValue = _value;
						
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

						// ------------------------------------------------------------------------------------
					}
					else
					{
						// SSP 4/1/09 Cell Text
						// 
						//_cachedCompareValue = _record.GetCellText( info._field );
						// SSP 5/29/09 - TFS17233 - Optimization
						// Use the new GetCellText method.
						// 
						//_cachedCompareValue = info._cellTextConverter.ConvertCellValue( _record );
						_cachedCompareValue = this.GetCellText( );
					}

					return _cachedCompareValue;

					
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

				}
			}

			#endregion // CellInfo Class

			#region FieldSortInfo Class

			internal class FieldSortInfo
			{
				internal FieldSortDescription _fieldSortDescription;
				internal Field _field;
				internal IComparer _comparer;
				internal IGroupByEvaluator _groupByEvaluator;
				internal IComparer _groupByEvaluatorSortComparer;
				internal int _direction;
				internal FieldSortComparisonType _comparisonType;
				internal bool _isGroupBy;
				internal Dictionary<Record, CellInfo> _cellInfo;

				// SSP 9/19/08 BR34967
				// Cached these values as well.
				// 
				internal Type _dataTypeUnderlying;
				internal Type _editAsTypeUnderlying;
				internal CultureInfo _converterCultureResolved;

				// SSP 4/1/09 Cell Text
				// 
				internal CellTextConverterInfo _cellTextConverter;

				// SSP 5/29/09 - TFS17233 - Optimization
				// Added GetCachedCellInfoX and GetCachedCellInfoY.
				// 
				private CellInfo _lastInfoX, _lastInfoY;

				// SSP 12/15/08 - NAS9.1 Record Filtering
				// Added an overload of FieldSortInfo that just takes in field.
				// 
				internal FieldSortInfo( Field field )
				{
					FieldSortDescription fsd = new FieldSortDescription( );
					fsd.Field = field;

                    // JJD 3/24/09 - TFS15895
                    // Added fieldlayout parameter
					//this.Initialize( fsd, null );
					this.Initialize( fsd, null, field.Owner );
				}

                // JJD 3/24/09 - TFS15895
                // Added fieldlayout parameter
                //internal FieldSortInfo( FieldSortDescription fieldSortDescription, SameFieldRecordsSortComparer comparerInfo)
                internal FieldSortInfo(FieldSortDescription fieldSortDescription, SameFieldRecordsSortComparer comparerInfo, FieldLayout fieldLayout)
				{
                    // JJD 3/24/09 - TFS15895
                    // Added fieldlayout parameter
					this.Initialize( fieldSortDescription, comparerInfo, fieldLayout );
				}

				// SSP 12/15/08 - NAS9.1 Record Filtering
				// Added an overload of FieldSortInfo that just takes in field. Also refactored logic in the
				// old constructor into new Initialize method that can be called from both constructors.
				// 
                // JJD 3/24/09 - TFS15895
                // Added fieldlayout parameter
				//private void Initialize( FieldSortDescription fieldSortDescription, SameFieldRecordsSortComparer comparerInfo )
				private void Initialize( FieldSortDescription fieldSortDescription, SameFieldRecordsSortComparer comparerInfo, FieldLayout fieldLayout )
				{
					Field field = fieldSortDescription.Field;
 
                    // JJD 3/24/09 - TFS15895
                    // Check for a valid field
                    if (field == null)
                        throw new ArgumentException(DataPresenterBase.GetString("LE_NotSupportedException_8", new object[] { fieldSortDescription.FieldName, fieldLayout.Description }), "FieldName");

					_fieldSortDescription = fieldSortDescription;
					_field = field;
					_comparer = field.SortComparerResolved;
					_groupByEvaluator = field.GroupByEvaluatorResolved;
					_groupByEvaluatorSortComparer = null != _groupByEvaluator ? _groupByEvaluator.SortComparer : null;
					_direction = ListSortDirection.Descending == fieldSortDescription.Direction ? -1 : 1;
					_comparisonType = field.SortComparisonTypeResolved;
					_isGroupBy = field.IsGroupBy;

					// SSP 9/19/08 BR34967
					// 
					_dataTypeUnderlying = field.DataTypeUnderlying;
					_editAsTypeUnderlying = Utilities.GetUnderlyingType( field.EditAsTypeResolved );
					_converterCultureResolved = field.ConverterCultureResolved;

					_cellInfo = new Dictionary<Record, CellInfo>(
						// SSP 12/15/08 - NAS9.1 Record Filtering
						// Check for comparerInfo being null.
						//Math.Max( 1, comparerInfo._recordsBeingSortedCount ),
						// SSP 3/9/10 TFS25402
						// 
						//Math.Max( 1, null != comparerInfo ? comparerInfo._recordsBeingSortedCount : 10 ),
						Math.Max( 2, null != comparerInfo && comparerInfo._recordsBeingSortedCount > 0 ? comparerInfo._recordsBeingSortedCount : 10 ),
						new ReferenceEqualityComparer<Record>( )
						);

					// SSP 4/1/09 Cell Text
					// 
					
					
					
					
					_cellTextConverter = CellTextConverterInfo.GetCachedConverter( field );
				}

				
				
				internal CellInfo GetCellInfo( DataRecord record )
				{
					CellInfo ret;
					if ( !_cellInfo.TryGetValue( record, out ret ) )
						// SSP 5/29/09 - TFS17233 - Optimization
						// Now we are storing a reference to fieldSortInfo in the CellInfo class.
						// 
						//_cellInfo[record] = ret = new CellInfo( record, _field );
						_cellInfo[record] = ret = new CellInfo( record, this );

					return ret;
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// Added GetCachedCellInfoX and GetCachedCellInfoY.
				// 
				internal CellInfo GetCachedCellInfoX( DataRecord record )
				{
					CellInfo ii = _lastInfoX;
					if ( null != ii && ii._record == record )
						return ii;

					ii = this.GetCellInfo( record );
					_lastInfoX = ii;

					return ii;
				}

				// SSP 5/29/09 - TFS17233 - Optimization
				// Added GetCachedCellInfoX and GetCachedCellInfoY.
				// 
				internal CellInfo GetCachedCellInfoY( DataRecord record )
				{
					CellInfo ii = _lastInfoY;
					if ( null != ii && ii._record == record )
						return ii;

					ii = this.GetCellInfo( record );
					_lastInfoY = ii;

					return ii;
				}

				// SSP 3/30/09 TFS15818
				// 
				internal object GetCellValueAsEditAsType( DataRecord record )
				{
					object val = record.GetCellValue( _field, true );

					// If EditAsType and DataType are different then convert the value to the EditAsType.
					// 
					if ( _editAsTypeUnderlying != _dataTypeUnderlying && null != val && !( val is DBNull ) )
						val = Utilities.ConvertDataValue( val, _editAsTypeUnderlying, _converterCultureResolved, null );

					return val;
				}
			}

			private class ReferenceEqualityComparer<T> : IEqualityComparer<T>
			{
				public bool Equals( T x, T y )
				{
					return object.ReferenceEquals( x, y );
				}

				public int GetHashCode( T obj )
				{
					return obj.GetHashCode( );
				}
			}

			#endregion // FieldSortInfo Class

			#region Private Members

			private int _recordsBeingSortedCount;
			private FieldLayout _fieldLayout;
			private FieldSortInfo[] _sortedFields;

			// SSP 5/29/09 - TFS17233 - Optimization
			// Added areRecordsInUnsortedOrder parameter.
			// 
			private bool _areRecordsInUnsortedOrder;

			// SSP 3/9/10 TFS25807
			// 
			private int _firstGroupByFieldWithGroupBySortComparer = -1;
			private int _lastGroupByFieldWithGroupBySortComparer = -1;

            #endregion //Private Members	
    
            #region Constructor

			// SSP 5/29/09 - TFS17233 - Optimization
			// Added areRecordsInUnsortedOrder parameter.
			// 
			//internal SameFieldRecordsSortComparer( FieldLayout fieldLayout, int recordsBeingSortedCount )
			internal SameFieldRecordsSortComparer( FieldLayout fieldLayout, int recordsBeingSortedCount, bool areRecordsInUnsortedOrder )
            {
				this._fieldLayout = fieldLayout;
				this._recordsBeingSortedCount = recordsBeingSortedCount;

				// SSP 5/29/09 - TFS17233 - Optimization
				// Added areRecordsInUnsortedOrder parameter.
				// 
				_areRecordsInUnsortedOrder = areRecordsInUnsortedOrder;

				FieldSortDescriptionCollection sortedFieldsColl = fieldLayout.SortedFields;
				this._sortedFields = new FieldSortInfo[sortedFieldsColl.Count];

                // SSP 6/14/10 TFS32985
                // 
                int nullCount = 0;

				for ( int i = 0; i < sortedFieldsColl.Count; i++ )
				{
                    // SSP 6/14/10 TFS32985
                    // 
                    FieldSortDescription fsd = sortedFieldsColl[i];
                    Field field = null != fsd ? fsd.Field : null;
                    if ( null == field || null == fsd.Field || fsd.Field.Index < 0 )
                    {
                        nullCount++;
                        continue;
                    }

					// JJD 3/24/09 - TFS15895
					// Added fieldlayout parameter
					//this._sortedFields[i] = new FieldSortInfo(sortedFieldsColl[i], this);
					FieldSortInfo fieldSortInfo = new FieldSortInfo( fsd, this, fieldLayout );
					this._sortedFields[i] = fieldSortInfo;

					// SSP 3/9/10 TFS25807
					// 
					if ( fieldSortInfo._isGroupBy && null != fieldSortInfo._groupByEvaluatorSortComparer )
					{
                        if ( _firstGroupByFieldWithGroupBySortComparer < 0 )
                            
                            
                            _firstGroupByFieldWithGroupBySortComparer = i - nullCount;

                        
						
                        _lastGroupByFieldWithGroupBySortComparer = i - nullCount;
					}
				}

                // SSP 6/14/10 TFS32985
                // 
                if ( nullCount > 0 )
                    _sortedFields = GridUtilities.RemoveAll( _sortedFields, null );
            }

			#endregion //Constructor
        
            #region DefaultCompare static method



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

            internal static int DefaultCompare(object x, object y, Field field)
            {
                return SameFieldRecordsSortComparer.DefaultCompare(x, y, true, field);
            }
    


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

            internal static int DefaultCompare(object x, object y, bool duringSort, Field field)
            {
                Debug.Assert(null != field);
                return DefaultCompare(x, y, duringSort, null != field && FieldSortComparisonType.CaseInsensitive == field.SortComparisonTypeResolved);
            }

			// SSP 2/23/09 TFS13975
			// 
			private static Dictionary<Type, int> g_typeOrderTable;

			// SSP 2/23/09 TFS13975
			// 
			private static int CompareTypeHelper( Type x, Type y )
			{
				Dictionary<Type, int> table = g_typeOrderTable;
				if ( null == table )
				{
					Type[] typeOrderArr = new Type[]
					{
						typeof( string ),
						typeof( char ),
						typeof( bool ),
						typeof( sbyte ),
						typeof( byte ),
						typeof( short ),
						typeof( ushort ),
						typeof( int ),
						typeof( uint ),
						typeof( long ),
						typeof( ulong ),
						typeof( decimal ),
						typeof( float ),
						typeof( double ),
						typeof( DayOfWeek ),
						typeof( DateTime ),					
						typeof( System.Drawing.Color ),
						typeof( System.Windows.Media.Color )
					};

					table = new Dictionary<Type, int>( typeOrderArr.Length );

					for ( int i = 0; i < typeOrderArr.Length; i++ )
						table.Add( typeOrderArr[i], i );

					g_typeOrderTable = table;
				}

				int xx, yy;
				if ( table.TryGetValue( x, out xx ) && table.TryGetValue( y, out yy ) )
				{
					return xx < yy ? -1 : ( xx > yy ? 1 : 0 );
				}

				return -2;
			}

			// SSP 2/23/09 TFS13975
			// 
			private static int CompareStringHelper( string xxStr, string yyStr, bool caseInSensitive )
			{
				// MD 4/4/12 - TFS105131
				// If xxStr is null, this will throw an exception. But we don't need to use the CompareTo method anyway.
				// The static string.Compare method will handle both values for caseInSensitive.
				//return caseInSensitive
				//        ? string.Compare( xxStr, yyStr, caseInSensitive, System.Globalization.CultureInfo.CurrentCulture )
				//        : xxStr.CompareTo( yyStr );
				return string.Compare(xxStr, yyStr, caseInSensitive, System.Globalization.CultureInfo.CurrentCulture);
			}



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

			internal static int DefaultCompare( object x, object y, bool duringSort, bool caseInSensitive )
            {
				// SSP 2/23/09 TFS13975
				// 
				// ----------------------------------------------------------------------------------------------------
				string xxStr = x as string;
				string yyStr = y as string;
				if ( null != xxStr && null != yyStr )
				{
					return CompareStringHelper( xxStr, yyStr, caseInSensitive );
				}

				//Check for a null database field
				if ( x == null || System.DBNull.Value == x )
				{
					if ( y == null || System.DBNull.Value == y )
						return 0;

					return -1;
				}

				if ( y == null || System.DBNull.Value == y )
					return 1;

				// SSP 2/23/09 TFS13975
				// If the two values are of different type then sort them by type first.
				// 
				Type xType = x.GetType( );
				Type yType = y.GetType( );
				if ( xType != yType )
				{
					int r = CompareTypeHelper( xType, yType );
					
					// -2 is an indication that a type is not known in which case we should fallback to
					// performing IComparable.CompareTo below.
					// 
					if ( -2 != r )
						return r;
				}

				IComparable comparable = x as IComparable;

				// If the objects implement IComparable then call that
				//
				if ( comparable != null )
					return comparable.CompareTo( y );


				if ( xType == yType )
				{
					int compare = 0;

					if ( xType == typeof( System.DayOfWeek ) )
					{
						if ( (System.DayOfWeek)x < (System.DayOfWeek)y )
							compare = -1;
						else
							if ( (System.DayOfWeek)x > (System.DayOfWeek)y )
								compare = 1;

						return compare;
					}
					else if ( xType == typeof( System.Drawing.Color ) )
					{
						int xArgbVal = ( (System.Drawing.Color)x ).ToArgb( );
						int yArgbVal = ( (System.Drawing.Color)y ).ToArgb( );

						if ( xArgbVal < yArgbVal )
							compare = -1;
						else
							if ( xArgbVal > yArgbVal )
								compare = 1;

						return compare;
					}
				}

				//Default: convert to string and compare
				// If not sorting (like when merging, or filtering), don't consider two objects 
				// as equal if their ToString results are equal because the ToString by default
				// returns the type name which would be the same even if the objects were 
				// different. Added the following else-if block.
				//
				// ----------------------------------------------------------------------------
				if ( !duringSort )
				{
					return x.Equals( y ) ? 0 : -2;
				}
				// ----------------------------------------------------------------------------
				else
				{
					return CompareStringHelper( x.ToString( ), y.ToString( ), caseInSensitive );
				}

				#region Original Code Commented Out

				
#region Infragistics Source Cleanup (Region)











































































#endregion // Infragistics Source Cleanup (Region)


				#endregion // Original Code Commented Out
			}

            #endregion //DefaultCompare static method

            #region IComparer.Compare






			int IComparer.Compare( object x, object y )
			{
				DataRecord recordX = x as DataRecord;
				DataRecord recordY = y as DataRecord;

				
				
				
				return this.CompareHelper( recordX, recordY );
			}

			
			
			private int CompareHelper( DataRecord recordX, DataRecord recordY )
			{
				if ( recordX == recordY )
					return 0;

				#region Handle the case where one record is null

				FieldSortInfo[] sortedFields = _sortedFields;
				if ( recordX == null || recordY == null )
				{
					int r = null != recordX ? 1 : -1;

					if ( sortedFields.Length > 0 )
						return r *= sortedFields[0]._direction;
				}

				#endregion //Handle the case where one record is null

				int compare = 0;

				CellInfo xxCellInfo = null, yyCellInfo = null;

				#region Compare based on sort criteria

				// SSP 3/9/10 TFS25807
				// 
				//for ( int i = 0; i < sortedFields.Length; i++ )
				bool groupByPhase = _lastGroupByFieldWithGroupBySortComparer >= 0;
				int sortedFieldIterationEnd = sortedFields.Length - 1;
				for ( int i = 0; i <= sortedFieldIterationEnd; i++ )
				{
					FieldSortInfo fieldInfo = sortedFields[i];
					Field field = fieldInfo._field;

					IComparer groupByEvaluatorSortComparer = null;

					// JJD 2/23/07 - BR19943
					// Added support for groupby evaluators to specify their own sort comparer
					// SSP 3/9/10 TFS25807
					// 
					//if ( fieldInfo._isGroupBy )
					if ( groupByPhase && fieldInfo._isGroupBy )
					{
						groupByEvaluatorSortComparer = fieldInfo._groupByEvaluatorSortComparer;

						// JJD 10/09/08 - TFS6745
						// If the user specified a comparer and the groupbyevaluator is 
						// out default CellTextEvaluator then use the comparer specified
						// by the user by nulling out the groupByEvaluatorSortComparer
						if ( fieldInfo._comparer != null && fieldInfo._groupByEvaluator is GroupByEvaluatorFactory.CellTextEvaluator )
							groupByEvaluatorSortComparer = null;
					}

					// SSP 5/29/09 - TFS17233 - Optimization
					// Moved this here from below. Also use the new GetCachedCellInfoX and GetCachedCellInfoY
					// methods instead of GetCellInfo to reduce the number of times dictionary has to be
					// indexed by taking advantage of the fact that often times merge sorting algorithm will
					// compare the same x value to a few different y values at a time and the other way around
					// as well.
					// 
					//xxCellInfo = fieldInfo.GetCellInfo( recordX );
					//yyCellInfo = fieldInfo.GetCellInfo( recordY );
					xxCellInfo = fieldInfo.GetCachedCellInfoX( recordX );
					yyCellInfo = fieldInfo.GetCachedCellInfoY( recordY );

					if ( groupByEvaluatorSortComparer != null )
					{
						// Note that the groupby evaluator comparer is expecting Cells in its compare
						// SSP 5/29/09 - TFS17233 - Optimization
						// 
						//compare = groupByEvaluatorSortComparer.Compare( recordX.Cells[field], recordY.Cells[field] );
						IComparer<CellInfo> cellInfoComparer = groupByEvaluatorSortComparer as IComparer<CellInfo>;
						if ( null != cellInfoComparer )
							compare = cellInfoComparer.Compare( xxCellInfo, yyCellInfo );
						else
							compare = groupByEvaluatorSortComparer.Compare( recordX.Cells[field], recordY.Cells[field] );
					}
					else
					{
						// If we have a user specified comparer, then pass in cell objects
						// for comparing the cells.
						//
						IComparer fieldSortComparer = fieldInfo._comparer;

						
						
						
						// SSP 5/29/09 - TFS17233 - Optimization
						// Moved this above.
						// 
						//xxCellInfo = fieldInfo.GetCellInfo( recordX );
						//yyCellInfo = fieldInfo.GetCellInfo( recordY );
						
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

						

						if ( fieldSortComparer != null )
						{
							compare = fieldSortComparer.Compare( xxCellInfo._value, yyCellInfo._value );
						}
						else
						{
							// SSP 9/18/08 BR34967
							// If EditAsType is set on the Field then convert the cell values to that type
							// for comparison purposes. Added new CompareValue properties on the CellInfo.
							//
							// ------------------------------------------------------------------------------
							// SSP 5/29/09 - TFS17233 - Optimization
							// Now we are storing a reference to fieldSortInfo in the class.
							// 
							//object valueX = xxCellInfo.GetCompareValue( fieldInfo );
							//object valueY = yyCellInfo.GetCompareValue( fieldInfo );
							object valueX = xxCellInfo.GetCompareValue( );
							object valueY = yyCellInfo.GetCompareValue( );

							
#region Infragistics Source Cleanup (Region)




















#endregion // Infragistics Source Cleanup (Region)

							// ------------------------------------------------------------------------------

							compare = SameFieldRecordsSortComparer.DefaultCompare( valueX, valueY, true, FieldSortComparisonType.CaseInsensitive == fieldInfo._comparisonType );
						}
					}

					if ( compare != 0 )
					{
						// if we are sorting descended then flip the sign
						//
						compare *= fieldInfo._direction;

						return compare;
					}

					// SSP 3/9/10 TFS25807
					// 
					if ( groupByPhase && i == sortedFieldIterationEnd )
					{
						groupByPhase = false;
						i = _firstGroupByFieldWithGroupBySortComparer - 1;
						sortedFieldIterationEnd = _lastGroupByFieldWithGroupBySortComparer;

						continue;
					}
				}

				#endregion //Compare based on sort criteria

				// Instead of returning 0 here which will cause the Array.Sort method
				// to put equal records any way it wishes, return -1 or 1 based on the
				// index so that it will put the record with higher list index after the
				// record with lower list index. The result of this will be that when
				// a field is sorted by and all the cells of that field are blank,
				// the sorting will retain records' positions. Before Array.Sort was 
				// reordering the records even when all the records were equal randomly
				// so to the user it would look as if grid sorted even if it did not 
				// need to sort.
				//
				// SSP 5/29/09 - TFS17233 - Optimization
				// Since we are performing proper sort if the records array being sorted
				// are in the unsorted order then there's no need to compare by the data
				// item index.
				// 
				//if ( 0 == compare )
				if ( 0 == compare && ! _areRecordsInUnsortedOrder )
				{
					int xxIndex = null != xxCellInfo ? xxCellInfo.DataItemIndex : recordX.DataItemIndex;
					int yyIndex = null != yyCellInfo ? yyCellInfo.DataItemIndex : recordY.DataItemIndex;

					compare = xxIndex < yyIndex ? -1 : 1;
				}

				return compare;
			}

            #endregion //IComparer.Compare

            #region IComparer<Record> Members

            /// <summary>
            /// Compares 2 items
            /// </summary>
            public int Compare(Record x, Record y)
            {
				
				
				
				return this.CompareHelper( x as DataRecord, y as DataRecord );
            }

            #endregion
		}

        #endregion // RecordsSortComparer Class

        #region DataRecordHolder Class

        private class DataRecordHolder
        {
            internal object comparisionListObject = null;
            internal DataRecord record = null;

            internal DataRecordHolder(DataRecord record, object comparisionListObject)
                : this(comparisionListObject)
            {
                this.record = record;
            }

            internal DataRecordHolder(object comparisionListObject)
            {
                this.comparisionListObject = comparisionListObject;
            }

            public override int GetHashCode()
            {
				// JM 07-07-09 TFS 19057 - Check for null - return 0 if null;
				if (this.comparisionListObject != null)
					return this.comparisionListObject.GetHashCode();

				return 0;
            }

            public override bool Equals(object o)
            {
                DataRecordHolder listObjectHolder = o as DataRecordHolder;
				return null != listObjectHolder && listObjectHolder.comparisionListObject == this.comparisionListObject;
            }
        }

        #endregion // DataRecordHolder Class

		// JJD 7/9/07
		// Added support for handling change notifications on another thread 
		#region ChangeNotification class

		private class ChangeNotification
		{
			internal object Sender;
			internal object EventArgs;
			internal ChangeNotification(object sender, object eventArgs)
			{
				this.Sender = sender;
				this.EventArgs = eventArgs;
			}

			// SSP 12/21/11 TFS73767 - Optimizations
			// 
			#region EqualityComparer Class

			internal class EqualityComparer : IEqualityComparer
			{
				private static string GetPropertyName( PropertyDescriptor pd )
				{
					return null != pd ? pd.Name : null;
				}

				private static int GetHashCodeHelper( object s )
				{
					return null != s ? s.GetHashCode( ) : 0;
				}

				bool IEqualityComparer.Equals( object x, object y )
				{
					ChangeNotification xx = x as ChangeNotification;
					ChangeNotification yy = y as ChangeNotification;

					if ( null != xx && null != yy )
					{
						PropertyChangedEventArgs xxPcnArgs = xx.EventArgs as PropertyChangedEventArgs;
						if ( null != xxPcnArgs )
						{
							PropertyChangedEventArgs yyPcnArgs = yy.EventArgs as PropertyChangedEventArgs;
							if ( null != xxPcnArgs && null != yyPcnArgs )
							{
								if ( xx.Sender == yy.Sender && xxPcnArgs.PropertyName == yyPcnArgs.PropertyName )
									return true;
							}
						}
						else
						{
							ListChangedEventArgs xxLcnArgs = xx.EventArgs as ListChangedEventArgs;
							if ( null != xxLcnArgs )
							{
								ListChangedEventArgs yyLcnArgs = yy.EventArgs as ListChangedEventArgs;
								if ( null != xxLcnArgs && null != yyLcnArgs )
								{
									if ( xxLcnArgs.NewIndex == yyLcnArgs.NewIndex
										&& GetPropertyName( xxLcnArgs.PropertyDescriptor ) == GetPropertyName( yyLcnArgs.PropertyDescriptor )
										&& ListChangedType.ItemChanged == xxLcnArgs.ListChangedType && ListChangedType.ItemChanged == yyLcnArgs.ListChangedType )
										return true;
								}
							}
						}
					}

					return false;
				}

				int IEqualityComparer.GetHashCode( object obj )
				{
					ChangeNotification cn = obj as ChangeNotification;
					if ( null != cn )
					{
						PropertyChangedEventArgs pcnArgs = cn.EventArgs as PropertyChangedEventArgs;
						if ( null != pcnArgs )
						{
							return GetHashCodeHelper( cn.Sender ) ^ GetHashCodeHelper( pcnArgs.PropertyName );
						}
						else
						{
							ListChangedEventArgs lcnArgs = cn.EventArgs as ListChangedEventArgs;
							if ( null != lcnArgs )
							{
								return lcnArgs.NewIndex.GetHashCode( ) ^ GetHashCodeHelper( GetPropertyName( lcnArgs.PropertyDescriptor ) );
							}
						}
					}

					return 0;
				}
			} 

			#endregion // EqualityComparer Class
		}

		#endregion //ChangeNotification	
        
        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added

        
        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
        #region SortGroupVersionStatus class

        private struct SortGroupVersionStatus
        {
            private bool _isSortVersionChanged;
            private bool _isGroupByVersionChanged;
            private bool _hasSortFields;
            private bool _hasGroupByFields;

            internal SortGroupVersionStatus(bool isSortVersionChanged,
                                            bool isGroupByVersionChanged,
                                            bool hasSortFields,
                                            bool hasGroupByFields)
            {
                this._hasGroupByFields = hasGroupByFields;
                this._hasSortFields = hasSortFields;
                this._isGroupByVersionChanged = isGroupByVersionChanged;
                this._isSortVersionChanged = isSortVersionChanged;
            }

            internal bool HasSortFields { get { return this._hasSortFields; } }
            internal bool HasGroupByFields { get { return this._hasGroupByFields; } }
            internal bool IsSortVersionChanged { get { return this._isSortVersionChanged; } }
            internal bool IsGroupByVersionChanged { get { return this._isGroupByVersionChanged; } }
        }

        #endregion //SortGroupVersionStatus class	
            
        // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
        #region SortGroupVersionTracker class

        internal class SortGroupVersionTracker
        {
            #region Private Members

            private PropertyValueTracker _sortVersionTracker;
            private PropertyValueTracker _groupByVersionTracker;
            private int _sortVersion;
            private int _groupByVersion;
            private FieldLayout _fl;

            #endregion //Private Members	
    
            #region Constructors

            internal SortGroupVersionTracker(RecordManager rm, FieldLayout fl)
            {
                this._fl = fl;

                this._sortVersionTracker = new PropertyValueTracker(fl, "SortVersion", rm.OnSortGroupByVersionChanged, true);
                this._groupByVersionTracker = new PropertyValueTracker(fl, "GroupByVersion", rm.OnSortGroupByVersionChanged, true);
            }

            #endregion //Constructors	
    
            #region Properties

            internal FieldLayout FieldLayout { get { return this._fl; } }
            internal int GroupByVersion { get { return this._groupByVersion; } }
            internal int SortVersion { get { return this._sortVersion; } }

            #endregion //Properties	
 
            #region Methods

			// JJD 4/25/11 - TFS67129 - added
			internal void DirtyVersionNumbers()
            {
                this._sortVersion--;
				this._groupByVersion--;
            }

            internal void SyncVersionNumbers()
            {
                this._sortVersion = this._fl.SortVersion;
                this._groupByVersion = this._fl.GroupByVersion;
            }

            #endregion //Methods
        }

        #endregion //SortGroupVersionTracker class	
    
		#region IWeakEventListener Members

		bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType == typeof(CurrentChangedEventManager))
			{
				this.OnCollectionViewCurrentChanged(sender, e);
				return true;
			}

			if (managerType == typeof(CurrentChangingEventManager))
			{
				if (e is CurrentChangingEventArgs)
				{
					this.OnCollectionViewCurrentChanging(sender, (CurrentChangingEventArgs)e);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordManager, arg type: " + e != null ? e.ToString() : "null");
			}

			if (managerType == typeof(CollectionChangedEventManager))
			{
				if (e is NotifyCollectionChangedEventArgs)
				{
					// JJD 7/9/07
					// Added support for handling change notifications on another thread 
					//this.OnSourceCollectionChanged(sender, (NotifyCollectionChangedEventArgs)e);
					this.OnChangeNotification(sender, e, ((NotifyCollectionChangedEventArgs)e).Action == NotifyCollectionChangedAction.Reset);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordManager, arg type: " + e != null ? e.ToString() : "null");
			}

			if (managerType == typeof(BindingListChangedEventManager))
			{
				if (e is ListChangedEventArgs)
				{
					// JJD 7/9/07
					// Added support for handling change notifications on another thread 
					//this.OnBindingListChanged(sender, (ListChangedEventArgs)e);
					this.OnChangeNotification(sender, e, ((ListChangedEventArgs)e).ListChangedType == ListChangedType.Reset);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordManager, arg type: " + e != null ? e.ToString() : "null");
			}

			// JM 05-06-10 TFS31643 - Use a WeakEventListener for FieldLayout property changes to avoid rooting the RecordManager 
			if (managerType == typeof(PropertyChangedEventManager))
			{
				if (e is PropertyChangedEventArgs)
				{
					this.OnFieldLayoutPropertyChanged(sender, e as PropertyChangedEventArgs);
					return true;
				}
				Debug.Fail("Invalid args in ReceiveWeakEvent for RecordManager, arg type: " + e != null ? e.ToString() : "null");
			}

			Debug.Fail("Invalid managerType in ReceiveWeakEvent for RecordManager, type: " + managerType != null ? managerType.ToString() : "null");
			return false;
		}

		#endregion
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