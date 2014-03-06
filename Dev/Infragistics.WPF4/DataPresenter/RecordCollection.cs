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
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Internal;
using Infragistics.Windows.Virtualization;
using Infragistics.Collections;

namespace Infragistics.Windows.DataPresenter
{
    #region SparseArray classes

        #region RecordSparseArrayBase







    internal abstract class RecordSparseArrayBase : SparseArray, ICreateItemCallback
    {
        #region Private Variables

        private RecordCollectionBase _ownerCollection = null;

        #endregion // Private Variables

        #region Constants

        internal const int DefaultFactor = 40;
		internal const float DefaultGrowthFactor = 0.2f;

        #endregion //Constants	
    
        #region Constructor






        protected RecordSparseArrayBase(RecordCollectionBase ownerCollection, bool manageScrollCounts, int factor, float growthFactor)
            : base(manageScrollCounts, factor, growthFactor)
        {
            if (null == ownerCollection)
                throw new ArgumentNullException("ownerCollection");

            this._ownerCollection = ownerCollection;
        }

        #endregion // Constructor

        #region DataPresenterBase

        internal DataPresenterBase DataPresenter { get { return this._ownerCollection.DataPresenter; } }

        #endregion //DataPresenterBase	

        #region OwnerCollection

        internal RecordCollectionBase OwnerCollection { get { return this._ownerCollection; } }

        #endregion //OwnerCollection	

		#region EnsureAllItemsCreated

		// SSP 7/29/08 
		// Added EnsureAllItemsCreated which will allocate all records if they haven't been allocated yet.
		// 
		internal abstract void EnsureAllItemsCreated( );

		#endregion // EnsureAllItemsCreated

		#region GetItem



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal abstract Record GetItem(int index, bool create);

        #endregion // GetItem

        #region NotifyItemScrollCountChanged

        internal new void NotifyItemScrollCountChanged(ISparseArrayMultiItem item)
        {
			Debug.Assert(this is MainRecordSparseArray);

            base.NotifyItemScrollCountChanged(item);
        }

        #endregion // NotifyItemScrollCountChanged
        
        #region ToArray

		// SSP 6/14/12 TFS113458
		// Added an overload that takes the array element type so we can create DataRecord[] instead of Record[].
		// 
		internal Record[] ToArray( bool create )
		{
			return this.ToArray( create, typeof( Record ) );
		}

		// SSP 6/14/12 TFS113458
		// Added an overload that takes the array element type so we can create DataRecord[] instead of Record[].
		// 
		/// <summary>
		/// Returns an array containing all the items of the sparse array. If 'create' is false then any null entries
		/// will be returned in the returned array.
		/// </summary>
		/// <param name="create">True to allocate any un-allocated record objects. If false then null entries will
		/// be returned in the returned array.</param>
		/// <param name="arrayElementType">Element type of the array to create. Must be Record or a derived type.</param>
		/// <returns>Array of the same length as the count of the sparse array.</returns>
        internal abstract Record[] ToArray(bool create, Type arrayElementType );

        #endregion //ToArray	

        #region ICreateItemCallback.CreateItem

        public abstract object CreateItem(SparseArray array, int relativeIndex);

        #endregion // ICreateItemCallback.CreateItem
    }

        #endregion // RecordSparseArrayBase

        #region MainRecordSparseArray







    internal class MainRecordSparseArray : RecordSparseArrayBase
    {
        #region Private Variables

        private int _verifiedScrollCountVersion = -1;
        private bool _initializeRecordPending;

        #endregion // Private Variables
 
        #region Constructor






        internal MainRecordSparseArray(RecordCollectionBase ownerCollection)
            : this(ownerCollection, true, DefaultFactor, DefaultGrowthFactor) { }





        internal MainRecordSparseArray(RecordCollectionBase ownerCollection, bool manageScrollCounts)
            : this(ownerCollection, manageScrollCounts, DefaultFactor, DefaultGrowthFactor) { }





        internal MainRecordSparseArray(RecordCollectionBase ownerCollection, bool manageScrollCounts, int factor)
            : this(ownerCollection, manageScrollCounts, factor, DefaultGrowthFactor) { }





        internal MainRecordSparseArray(RecordCollectionBase ownerCollection, bool manageScrollCounts, int factor, float growthFactor)
            : base(ownerCollection, manageScrollCounts, factor, growthFactor)
        {
			// SSP 6/14/12 TFS114354
			// Use the record manager's scroll count version number which is what we verify the version number against.
			// 
            //this._verifiedScrollCountVersion = this.DataPresenter.ScrollableRecordCountVersion;
			RecordManager rm = ownerCollection.ParentRecordManager;
			if ( null != rm )
				this._verifiedScrollCountVersion = rm.ScrollCountVersion;
        }

        #endregion // Constructor

        #region DirtyScrollCount

        internal void DirtyScrollCount()
        {
			// SSP 2/11/09 TFS12467
			// All we need to do is call SparseArray's DirtyScrollCountInfo which in turn will call
			// OnScrollCountChanged which will dirty the parent record's scroll count.
			// 
			// --------------------------------------------------------------------------------------
			RecordCollectionBase collection = this.OwnerCollection;
			RecordManager rm = collection.ParentRecordManager;

			// Set the _verifiedScrollCountVersion so we don't dirty the scroll count again.
			// 
			if ( null != rm )
				// SSP 8/4/09 - NAS9.2 Enhanced grid view
				// Use the new ScrollCountVersion property instead of the _scrollCountVersion member var.
				// 
				//_verifiedScrollCountVersion = rm._scrollCountVersion;
				_verifiedScrollCountVersion = rm.ScrollCountVersion;

			this.DirtyScrollCountInfo( );
			
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------
        }

        #endregion // DirtyScrollCount

		#region EnsureAllItemsCreated

		// SSP 7/29/08 
		// Added EnsureAllItemsCreated which will allocate all records if they haven't been allocated yet.
		// 
		internal override void EnsureAllItemsCreated( )
		{
			this.ToArray( true );
		}

		#endregion // EnsureAllItemsCreated

        #region VerifyAgainstScrollVersion

        private void VerifyAgainstScrollVersion()
        {
			// SSP 2/11/09 TFS12467
			// 
			// ----------------------------------------------------------------------------
			RecordCollectionBase ownerCollection = this.OwnerCollection;
			if ( null != ownerCollection && !ownerCollection.IsUpdating )
			{
				ownerCollection.EnsureNotDirty( );

				RecordManager rm = ownerCollection.ParentRecordManager;
				// SSP 8/4/09 - NAS9.2 Enhanced grid view
				// Use the new ScrollCountVersion property instead of the _scrollCountVersion member var.
				// 
				//if ( null != rm && _verifiedScrollCountVersion != rm._scrollCountVersion )
				if ( null != rm && _verifiedScrollCountVersion != rm.ScrollCountVersion )
				{
					// SSP 8/4/09 - NAS9.2 Enhanced grid view
					// Use the new ScrollCountVersion property instead of the _scrollCountVersion member var.
					// 
					//_verifiedScrollCountVersion = rm._scrollCountVersion;
					_verifiedScrollCountVersion = rm.ScrollCountVersion;

					this.DirtyScrollCountInfo( );
				}
			}

			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

			// ----------------------------------------------------------------------------
        }

        #endregion // VerifyAgainstScrollVersion

        #region GetItem



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

        internal override Record GetItem(int index, bool create)
        {
            return (Record)base.GetItem(index, create ? this : null);
        }

        #endregion // GetItem

        #region GetOwnerData

        protected override object GetOwnerData(object item)
        {
			if (this.OwnerCollection is DataRecordCollection)
                return ((Record)item)._sparseArrayOwnerData_Sorted;
            else
				return ((Record)item)._sparseArrayOwnerData_Grouped;
        }

        #endregion // GetOwnerData

        #region SetOwnerData

        protected override void SetOwnerData(object item, object ownerData)
        {
			if (this.OwnerCollection is DataRecordCollection)
                ((Record)item)._sparseArrayOwnerData_Sorted = ownerData;
            else
				((Record)item)._sparseArrayOwnerData_Grouped = ownerData;
        }

        #endregion // SetOwnerData

        #region VisibleCount

        internal int VisibleCount
        {
            get
            {
                this.VerifyAgainstScrollVersion();
                return this.GetVisibleCount();
            }
        }

        #endregion // VisibleCount

        #region ScrollCount

        internal int ScrollCount
        {
            get
            {
                this.VerifyAgainstScrollVersion();
                return this.GetScrollCount();
            }
        }

        #endregion // ScrollCount

		#region InCalculatingScrollCount

		// SSP 5/18/09 TFS16576
		// Added a flag to keep track of whether EnsureScrollCountCalculated gets called
		// recursively. This will let us debug.assert to let the caller know that it's
		// getting called recursively.
		// 
		/// <summary>
		/// Indicates whether the sparse array is currently in the process of calculating its scroll count.
		/// </summary>
		internal new bool InCalculatingScrollCount
		{
			get
			{
				return base.InCalculatingScrollCount;
			}
		}

		#endregion // InCalculatingScrollCount

        #region VisibleIndexOf

        internal int VisibleIndexOf(ISparseArrayMultiItem immediateChild)
        {
			return this.VisibleIndexOf( immediateChild, false );
        }

		
		
		
		internal int VisibleIndexOf( ISparseArrayMultiItem immediateChild, bool ignoreItemHiddenState )
		{
			this.VerifyAgainstScrollVersion( );
			return base.GetVisibleIndexOf( immediateChild, ignoreItemHiddenState );
		}

        #endregion // VisibleIndexOf

        #region ScrollIndexOf

        // JJD 05/06/10 - TFS27757 
        // Added ignoreHiddenItemState param
        //internal int ScrollIndexOf(ISparseArrayMultiItem immediateChild)
        internal int ScrollIndexOf(ISparseArrayMultiItem immediateChild, bool ignoreHiddenItemState)
        {
            this.VerifyAgainstScrollVersion();
            
            // JJD 05/06/10 - TFS27757 
            // Pass along ignoreHiddenItemState param
            //return base.GetScrollIndexOf(immediateChild);
            return base.GetScrollIndexOf(immediateChild, ignoreHiddenItemState);
        }

        #endregion // ScrollIndexOf

        #region GetItemAtVisibleIndex

        internal Record GetItemAtVisibleIndex(int visibleIndex)
        {
            this.VerifyAgainstScrollVersion();
            return (Record)this.GetItemAtVisibleIndex(visibleIndex, this);
        }

        #endregion // GetItemAtVisibleIndex

        #region GetItemAtVisibleIndexOffset

        internal Record GetItemAtVisibleIndexOffset(Record startRow, int offset)
        {
            this.VerifyAgainstScrollVersion();
            return (Record)base.GetItemAtVisibleIndexOffset(startRow, offset, this);
        }

        #endregion // GetItemAtVisibleIndexOffset

        #region GetItemAtScrollIndex

        internal Record GetItemAtScrollIndex(int scrollIndex)
        {
            this.VerifyAgainstScrollVersion();
            return (Record)this.GetItemAtScrollIndex(scrollIndex, this);
        }

        #endregion // GetItemAtScrollIndex

		#region GetItemContainingScrollIndex

		// SSP 7/30/09 - NAS9.2 Enhanced grid view
		// Added GetItemContainingScrollIndex method.
		// 
		internal Record GetItemContainingScrollIndex( ref int scrollIndex )
		{
			this.VerifyAgainstScrollVersion( );
			return (Record)base.GetItemContainingScrollIndex( ref scrollIndex, this );
		}

		#endregion // GetItemContainingScrollIndex

		#region GetVisibleRecords

		// SSP 2/21/08
		// Added GetVisibleRecords that returns visible items.
		/// <summary>
		/// Returns visible items.
		/// </summary>
		/// <param name="createItems">Whether to create new items to fill null slots.</param>
		/// <returns>Returns visible items.</returns>
		internal IEnumerable GetVisibleRecords( bool createItems )
		{
			return base.GetVisibleItems( createItems ? this : null );
		}

		#endregion // GetVisibleRecords

		#region ToArray

		// SSP 6/14/12 TFS113458
		// Added an overload that takes the array element type so we can create DataRecord[] instead of Record[].
		// 
		//internal override Record[] ToArray(bool create)
		internal override Record[] ToArray( bool create, Type arrayElementType )
        {
            if (!create)
				// SSP 6/14/12 TFS113458
				// 
                //return (Record[])base.ToArray(typeof(Record));
				return (Record[])base.ToArray( arrayElementType );

			// SSP 6/14/12 TFS113458
			// 
			//Record[] array = new Record[this.Count];
			Record[] array = (Record[])Array.CreateInstance( arrayElementType, this.Count );

			// JJD 5/19/07
			// Optimization - Wrap possibly creating multiple items in begin/endupdate calls
			this.OwnerCollection.BeginUpdate();

			try
			{
				for (int i = 0; i < this.Count; i++)
					array[i] = this.GetItem(i, true);
			}
			finally
			{
				// SSP 2/11/09 TFS12467
				// No need to pass true for dirtyScrollCount since creation of item will cause the
				// sparse array to dirty it scroll count if the created item happens to have scroll 
				// count of something other than 1.
				// 
				//this.OwnerCollection.EndUpdate(true);
				this.OwnerCollection.EndUpdate( false );
			}

            return array;
        }

        #endregion // ToArray

		#region OnItemCreated

		/// <summary>
		/// Called when a item is created to occupy a null slot
		/// </summary>
		/// <param name="item">The item that was created.</param>
		/// <param name="index">The zero-based index of the item in the array</param>
		protected override void OnItemCreated(object item, int index)
		{
			// SSP 12/03/10 TFS60379
			// Reset the _cachedFilterState to resume filtering on the record. We are setting
			// _cachedFilterState to NeverFilter in the CreateItem implementation.
			// 
			Record rcd = item as Record;
			if ( null != rcd && Record.FilterState.NeverFilter == rcd._cachedFilterState )
				rcd._cachedFilterState = Record.FilterState.NeedsToRefilter;

			base.OnItemCreated(item, index);

			// JJD 4/17/07
			// We delayed raising the InitializeRecord event
			// until after the record has been added into the sparse array
			if (this._initializeRecordPending == true)
			{
				this._initializeRecordPending = false;

				if (rcd != null)
					rcd.FireInitializeRecord();
			}
		}

		#endregion //OnItemCreated	
    
        #region OnScrollCountChanged






        protected override void OnScrollCountChanged()
        {
			RecordCollectionBase collection = this.OwnerCollection;

			// SSP 2/11/09 TFS12467
			// 
			// --------------------------------------------------------------------------------------
			ViewableRecordCollection vrc = collection.ViewableRecordsIfAllocated;
			if ( null != vrc )
				vrc.OnScrollCountDirtiedHelper( );
			
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

			// --------------------------------------------------------------------------------------
        }

        #endregion // OnScrollCountChanged

        #region ICreateItemCallback.CreateItem

        public override object CreateItem(SparseArray array, int relativeIndex)
        {
            bool newRowCreated;

			RecordManager manager = this.OwnerCollection.ParentRecordManager;
            
            UnsortedRecordSparseArray unsortedArray = ((DataRecordCollection)(manager.Unsorted)).SparseArray as UnsortedRecordSparseArray;

            Debug.Assert(unsortedArray != null);

			ViewableRecordCollection vrc = manager.Sorted.ViewableRecords;

			Debug.Assert(vrc != null);

			// adjust unsorted index to allow for records inserted at the top
			int unsortedIndex = vrc.GetUnsortedIndexFromSortedIndex(relativeIndex);

			Record record = unsortedArray.GetItem(unsortedIndex, true, true, out newRowCreated);

            // If the record is created then fire initialize record on the record.
            //
			if (newRowCreated && null != record)
			{
				record.InitializeParentCollection(this.OwnerCollection);

				// JJD 4/17/07
				// Delay raising the InitializeRecord event
				// until after the record has been added into the sparse array
				// just set a flag here and override OnItemCreated.

				// record.FireInitializeRecord();
				this._initializeRecordPending = true;

				// SSP 12/03/10 TFS60379
				// If record filters version number is different then prevent the record from
				// causing filter verification because vrc's EnsureFiltersEvaluated will dirty
				// the scroll count on the sparse array and may cause it be calculated 
				// synchronously, all while the sparse array is still in the middle of 
				// allocating this item. This causes the scroll count to off by 1 if this 
				// record gets filtered out because as part of the item creation, the sparse
				// array will decrement the scroll count by 1 and since the scroll count has
				// already been verified, it will cause it to be 1 less than what it should be.
				// 
				if ( vrc.VerificationNeeded )
					record._cachedFilterState = Record.FilterState.NeverFilter;
			}

            return record;
        }

        #endregion // ICreateItemCallback.CreateItem
    }

        #endregion // MainRecordSparseArray

        #region UnsortedRecordSparseArray Class






    internal class UnsortedRecordSparseArray : RecordSparseArrayBase
    {
        #region Consructor

        internal UnsortedRecordSparseArray(RecordCollectionBase records)
            : base(records, true, DefaultFactor, DefaultGrowthFactor)
        {
        }

        #endregion //Consructor	
    
        #region GetOwnerData

        protected override object GetOwnerData(object item)
        {
            return ((Record)item)._sparseArrayOwnerData_Unsorted;
        }

        #endregion //GetOwnerData	
    
        #region SetOwnerData

        protected override void SetOwnerData(object item, object ownerData)
        {
            ((Record)item)._sparseArrayOwnerData_Unsorted = ownerData;
        }

        #endregion //SetOwnerData	

		#region EnsureAllItemsCreated

		// SSP 7/29/08 
		// Added EnsureAllItemsCreated which will allocate all records if they haven't been allocated yet.
		// 
		internal override void EnsureAllItemsCreated( )
		{
			this.ToArray( true );
		}

		#endregion // EnsureAllItemsCreated
    
        #region GetItem

        internal override Record GetItem(int index, bool create)
        {
            bool newRowCreated;
            return this.GetItem(index, create, false, out newRowCreated);
        }

        internal Record GetItem(int index, bool create, bool calledFromSortedManagerSparseArray, out bool newRowCreated)
        {
            newRowCreated = false;
            Record record = (Record)this[index];

            if (null == record && create)
            {
                record = this.OwnerCollection.AllocateNewRecord(index);
                newRowCreated = true;
                this[index] = record;

                if (!calledFromSortedManagerSparseArray)
                {
					RecordManager manager = this.OwnerCollection.ParentRecordManager;

					// get the sorted collection counterpart
					RecordCollectionBase sortedCollection = manager.Sorted;

					ViewableRecordCollection vrc = sortedCollection.ViewableRecords;

					Debug.Assert(vrc != null);

					// adjust sorted index to allow for records inserted at the top
					int sortedIndex = vrc.GetSortedIndexFromUnsortedIndex(index);

					Debug.Assert(manager.IsInVerifySort || null == (Record)sortedCollection.SparseArray[sortedIndex]);

					// initialize the parent collection to the sorted collection
					record.InitializeParentCollection(sortedCollection);

					// SSP 12/15/08 - NAS9.1 Record Filtering
					// We need to prevent record from lazily applying filters until it gets added to both the
					// unsorted and sorted sparse arrays. Applying filters here will result in viewable record
					// collection not being able to send Remove notification for the record since it won't be
					// able to locate the index of the record (since it's not part of the sorted collection 
					// yet). Also delaying applying of filters unti InitializeRecord event is raised is better
					// because that way any data initialization, especially on unbound cells, will be taken 
					// into account in filtering.
					//
					record.SuspendFiltering( );
					
					int scrollCountOfRecord = record.ScrollCountInternal;

					if (scrollCountOfRecord == 1)
						sortedCollection.BeginUpdate();

					try
					{
						// set the record at the same index
						sortedCollection.SparseArray[sortedIndex] = record;
					}
					finally
					{
						// JJD 5/19/07
						// Optimization - added dirtyscrollcount flag to end update call
						if (scrollCountOfRecord == 1)
							sortedCollection.EndUpdate(false);
					}

                    // Fire InitializeRow for the new record only if not called from the scroll 
                    // count manager sparse array. If we did get called from there then we 
                    // don't need to fire the initializerow here as it does that itself.
                    //
                    record.FireInitializeRecord();

					// SSP 12/15/08 - NAS9.1 Record Filtering
					// 
					record.ResumeFiltering( true );
                }
            }

            return record;
        }

        #endregion //GetItem	
    
        #region ToArray

		// SSP 6/14/12 TFS113458
		// Added an overload that takes the array element type so we can create DataRecord[] instead of Record[].
		// 
        //internal override Record[] ToArray(bool create)
		internal override Record[] ToArray( bool create, Type arrayElementType )
        {
			// SSP 6/14/12 TFS113458
			// 
            //Record[] array = (Record[])base.ToArray(typeof(Record));
			Record[] array = (Record[])base.ToArray( arrayElementType );

            if (create)
            {
				// JJD 5/19/07
				// Optimization - Wrap possibly creating multiple items in begin/endupdate calls
				this.OwnerCollection.BeginUpdate();

				try
				{
					for (int i = 0; i < array.Length; i++)
					{
						if (null == array[i])
						{
							// JJD 12/7/06
							// Call GetItem instead so that the Sorted sparse array will be kept in sync 
							//this.OwnerCollection.AllocateNewRecord(i);
							//array[i].FireInitializeRecord();
							bool newRowCreated;
							array[i] = this.GetItem(i, create, false, out newRowCreated);
						}
					}
				}
				finally
				{
					// SSP 2/11/09 TFS12467
					// No need to pass true for dirtyScrollCount since creation of item will cause the
					// sparse array to dirty it scroll count if the created item happens to have scroll 
					// count of something other than 1.
					// 
					//this.OwnerCollection.EndUpdate(true);
					this.OwnerCollection.EndUpdate( false );
				}

            }

            return array;
        }

        #endregion //ToArray	
    
        #region ICreateItemCallback.CreateItem

        public override object CreateItem(SparseArray array, int relativeIndex)
        {
            bool newRowCreated;
            return this.GetItem(relativeIndex, true, false, out newRowCreated);
        }

        #endregion // ICreateItemCallback.CreateItem
    }

        #endregion // UnsortedRecordSparseArray Class

    #endregion //SparseArray classes	
    
    #region RecordCollectionBase class

    /// <summary>
    /// An abstract base collection of Records used by the DataPresenterBase control
    /// </summary>
	/// <remarks>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="DataRecord"/>
 	/// <seealso cref="GroupByRecord"/>
 	/// <seealso cref="ExpandableFieldRecord"/>
    public abstract class RecordCollectionBase : INotifyPropertyChanged, INotifyCollectionChanged, IList, IList<Record>
    {
        #region Private Members

        private PropertyChangedEventHandler _propertyChangedHandler;

        private NotifyCollectionChangedEventHandler _collectionChangedHandler;

        private Record _parentRecord;
        private RecordManager _parentRecordManager;
		private ViewableRecordCollection _viewableRecordCollection;
        private FieldLayout _fieldLayout;
        private RecordSparseArrayBase _sparseArray;
		private int _sortVersion;
		private int _groupByVersion;
	
	    // JJD 1/28/09 - TFS12487
        private Field _groupByField;

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// 
		
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)


        // JJD 3/19/09 - TFS15701
        // Keep track of the last sort status
        private SortStatus _previousSortStatus = SortStatus.NotSorted;

		private WeakReference _lastRecordList;

		
		
		private SummaryResultCollection _summaryResults;

		// JM 6-28-10 TFS33366
		private DispatcherOperation		_bumpVersionOperation;
		private int						_collectionVersion;

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// 
		internal GroupsSynchronizer _groupsSynchronizer;
		internal DataRecordsSynchronizer _recordsSynchronizer;

        #endregion //Private Members

        #region Constructors

        /// <summary>
		/// Initializes a new instance of the <see cref="RecordCollectionBase"/> class
        /// </summary>
		/// <param name="parentRecord">The parent record or null if there is no parent record</param>
		/// <param name="fieldLayout">The associated <see cref="FieldLayout"/></param>
		/// <param name="parentRecordManager">The owning <see cref="RecordManager"/></param>
        /// <param name="createUnsortedArray">A flag indicating what type of sparse array to create</param>
        // JJD 2/25/09 - Optimization
        // Added createUnsortedArray param
        protected RecordCollectionBase(Record parentRecord, RecordManager parentRecordManager, FieldLayout fieldLayout, bool createUnsortedArray)
        {
            if (null == parentRecordManager)
                throw new ArgumentNullException("parentRecordManager");

            this._parentRecord          = parentRecord;
            this._parentRecordManager   = parentRecordManager;
            this._fieldLayout           = fieldLayout;
   
            // JJD 2/25/09 - Optimization
            // Create the sparse array in the constructor so we avoid
            // the overhead in the property gets to check to
            // see if we need to create it.
            if (createUnsortedArray)
                this._sparseArray = new UnsortedRecordSparseArray(this);
            else
            if ( this is ExpandableFieldRecordCollection )
                 this._sparseArray = new MainRecordSparseArray(this, true, 2);
            else
                this._sparseArray = new MainRecordSparseArray(this);

            // JJD 2/25/09 - Optimization
            // Create the viewable records collection in the constructor 
            // so we avoid the overhead in the property gets to check to
            // see if we need to create it.

            if (this._sparseArray is MainRecordSparseArray)
            {
                this._viewableRecordCollection = new ViewableRecordCollection(this);
            }

            // JJD 1/28/09 - TFS12487
            // Call the InitializeGroupByField method to cache the value
            this.InitializeGroupByField();
        }

        #endregion //Constructors

        #region Properties

            #region Public Properties

				// JM 6-28-10 TFS33366 - Added.
				#region CollectionVersion

		/// <summary>
		/// Returns the internal version number of the record collection.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int CollectionVersion
        {
            get
            {
				return this._collectionVersion;
            }
		}

				#endregion //CollectionVersion

                #region Count

        /// <summary>
        /// Returns the number of records in the collection (read-only)
        /// </summary>
        public int Count
        {
            get { return this.SparseArray.Count; }
        }

                #endregion //Count	
    
                #region DataPresenterBase

        /// <summary>
        /// Returns the associated DataPresenterBase control
        /// </summary>
        public DataPresenterBase DataPresenter { get { return this._parentRecordManager.DataPresenter; } }

                #endregion //DataPresenterBase	
    
                #region FieldLayout

        /// <summary>
        /// Returns the field layout of the items in the collection (read-only)
        /// </summary>
        /// <remarks>
		/// <p class="body">Returns the <see cref="FieldLayout"/> of the items in the collection. If the collection 
		/// contains heterogenous data (or records with different <see cref="Record.FieldLayout"/> references), the 
		/// FieldLayout of one record is returned.</p>
		/// </remarks>
        public FieldLayout FieldLayout { get { return this._fieldLayout; } }

                #endregion //FieldLayout	

				#region LastRecordList

		internal RecordListControl LastRecordList
		{
			get
			{
				if (this._lastRecordList == null)
					return null;

				RecordListControl rlc = Utilities.GetWeakReferenceTargetSafe(this._lastRecordList) as RecordListControl;

				if (rlc == null)
					return null;

				// verify the CellPresenter is still valid
				if (rlc.ItemsSource != this)
				{
					this._lastRecordList = null;
					return null;
				}

				return rlc;
			}
			set
			{
				if (value == null)
					this._lastRecordList = null;
				else if ( value != this.LastRecordList)
					this._lastRecordList = new WeakReference(value);
			}
		}

				#endregion //LastRecordList

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
                if (this._parentRecord is DataRecord)
                    return this._parentRecord as DataRecord;

                return this.ParentRecordManager.ParentDataRecord;
            }
        }

                #endregion //ParentDataRecord
     
                #region ParentRecord

        /// <summary>
        /// Returns the parent record of the collection (read-only)
        /// </summary>
        /// <remarks>
		/// <para class="body">Returns null for the root collection.</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record ParentRecord { get { return this._parentRecord; } }

                #endregion //ParentRecord
     
                #region ParentRecordManager

        /// <summary>
        /// Returns the owner's record manager (read-only)
        /// </summary>
         public RecordManager ParentRecordManager { get { return this._parentRecordManager; } }

                #endregion //ParentRecordManager

				#region SummaryResults

		
		
		/// <summary>
		/// Returns a collection summary calculation results for record data from this record collection.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>SummaryResults</b> property returns the summary calculation results for record data from this
		/// record collection.
		/// </para>
		/// <para class="body">
		/// To add summaries, use the FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/> collection.
		/// The return SummaryResultsCollection will contain a <see cref="SummaryResult"/> instance for each
		/// <see cref="SummaryDefinition"/> in the FieldLayout's SummaryDefinitions collection.
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SummaryDefinitions"/>
		/// <seealso cref="SummaryResultCollection"/>
		/// <seealso cref="SummaryResult"/>
		public SummaryResultCollection SummaryResults
		{
			get
			{
				FieldLayout fl = this.FieldLayout;

				// SSP 2/5/09
				// Allocate the SummaryDefinitions collection otherwise when it does get allocated, it 
				// changes the value of this property implicitly and we won't know to notify that this
				// property changed. For that we would have to have notification mechanism. Instead 
				// simply allocate since having any summary elements will cause allocation of summary 
				// definitions anyways.
				// 
				//SummaryDefinitionCollection summaries = null != fl ? fl.SummaryDefinitionsIfAllocated : null;
				SummaryDefinitionCollection summaries = null != fl ? fl.SummaryDefinitions : null;
				
				if ( null != _summaryResults && _summaryResults.SummaryDefinitions == summaries )
					return _summaryResults;

				bool notify = false;
				if ( null != summaries )
				{
					_summaryResults = new SummaryResultCollection( this, fl, summaries );
					notify = true;
				}
				else if ( null != _summaryResults )
				{
					_summaryResults = null;
					notify = true;
				}

				if ( notify )
					this.OnPropertyChanged( "SummaryResults" );

				return _summaryResults;
			}
		}

				#endregion // SummaryResults

			#endregion //Public Properties

			#region Internal Properties

		 // JJD 4/27/07
				 // Optimization - added update count to bypass notifications while building a list
				 #region BeginUpdateCount

		// SSP 2/11/09 TFS12467
		// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
		// Not used any more.
		// 
		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

				#endregion //BeginUpdateCount	

				#region BeginUpdateInfo

		// SSP 2/11/09 TFS12467
		// 
		internal RecordManager.BeginUpdateInformation BeginUpdateInfo
		{
			get
			{
				RecordManager rm = this.ParentRecordManager;
				return null != rm ? rm._beginUpdateInfo : null;
			}
		}

				#endregion // BeginUpdateInfo

				#region GroupByField






        internal Field GroupByField
        {
            get
            {
                // JJD 1/28/09 - TFS12487
                // Return member variable that was cached in InitializeGroupByField method
                return this._groupByField;
            }
        }

                #endregion // GroupByField

				#region GroupByVersion

		internal int GroupByVersion { get { return this._groupByVersion; } }

				#endregion //GroupByVersion	

                #region IsTopLevel







        internal bool IsTopLevel
        {
            get
            {
                Record rcd = this.ParentRecord;
                return rcd == null || rcd is ExpandableFieldRecord;
            }
        }

                #endregion // IsTopLevel

				#region IsRootLevel

		
		
		/// <summary>
		/// Indicates whether this record collection is the root record collection in the data grid.
		/// </summary>
		internal bool IsRootLevel
		{
			get
			{
				return null == this.ParentRecord;
			}
		}

				#endregion // IsRootLevel

				#region IsUpdating

		// SSP 2/11/09 TFS12467
		// 
		/// <summary>
		/// Indicates if BeginUpdate has been called without corresponding EndUpdate.
		/// </summary>
		internal bool IsUpdating
		{
			get
			{
				RecordManager rm = this.ParentRecordManager;
				return null != rm && rm.IsUpdating;
			}
		}

				#endregion // IsUpdating

				#region IsViewableRecordsCollectionAllocated

		// JJD 7/20/07
		internal bool IsViewableRecordsCollectionAllocated
		{
			get { return this._viewableRecordCollection != null; }
		}

				#endregion //IsViewableRecordsCollectionAllocated	

				#region RecordsType

		
		
		/// <summary>
		/// Returns the type of records contained in this record collection.
		/// </summary>
		internal abstract RecordType RecordsType
		{ 
			get;
		}

				#endregion // RecordsType

				#region SortVersion

		internal int SortVersion { get { return this._sortVersion; } }

				#endregion //SortVersion	
    
                #region SparseArray

        internal RecordSparseArrayBase SparseArray
        {
            get
            {
                // JJD 2/25/09 - Optimization
                // Now created in the constructor
                //if (this._sparseArray == null)
                //    this._sparseArray = this.CreateSparseArray();

                return this._sparseArray;
            }
        }

                #endregion // SparseArray

				#region SummaryResultsIfAllocated

		/// <summary>
		/// Returns the summary result collection if it's allocated.
		/// </summary>
		internal SummaryResultCollection SummaryResultsIfAllocated
		{
			get
			{
				return _summaryResults;
			}
		}

				#endregion // SummaryResultsIfAllocated

				#region ViewableRecords



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		 internal ViewableRecordCollection ViewableRecords
		 {
			 get
			 {
                 // JJD 2/25/09 - Optimization
                 // Now created in the constructor
                 #region Old code

                 //// JJD 2/28/08
                 //// Use the public property which will lazily create the SparseArray
                 ////if (!(this._sparseArray is MainRecordSparseArray))
                 //if (!(this.SparseArray is MainRecordSparseArray))
                 //    return null;

                 //if (this._viewableRecordCollection == null)
                 //    this._viewableRecordCollection = new ViewableRecordCollection(this);

                 #endregion //Old code	
    
				 return this._viewableRecordCollection;
			 }
		 }

		 // SSP 2/11/09 TFS12467
		 // 
		 internal ViewableRecordCollection ViewableRecordsIfAllocated
		 {
			 get
			 {
				 return _viewableRecordCollection;
			 }
		 }

				 #endregion //ViewableRecords	

            #endregion //Internal Properties

            #region Protected Properties
     
            #endregion //Protected Properties

            #region Private Properties

            #endregion //Private Properties

        #endregion //Properties

        #region Methods
        
            #region Public Methods

				#region CollapseAll

        // SSP 7/8/09 - TFS19155
		// 
		/// <summary>
		/// Collapses all records.
		/// </summary>
		/// <param name="recursive">Whether to collapse descendant records as well.</param>
		 /// <seealso cref="ExpandAll(int)"/>
		public void CollapseAll( bool recursive )
		{
			this.ExpandCollapseAllHelper( false, recursive ? -1 : 0 );
		}

		/// <summary>
		/// Collapses all records recursively upto the recursionDepth.
		/// </summary>
		/// <param name="recursionDepth">Number of levels to recurse down. 0 means just this level.</param>
		public void CollapseAll( int recursionDepth )
		{
			GridUtilities.ValidateNonNegative( recursionDepth );

			this.ExpandCollapseAllHelper( false, recursionDepth );
		}

				#endregion // CollapseAll

                #region Contains

        /// <summary>
        /// Returns true if the record is in the collection
        /// </summary>
        public bool Contains(Record item)
        {
            return this.SparseArray.Contains(item);
        }

                #endregion //Contains	
    
                #region CopyTo

        /// <summary>
        /// Copies all the records in the collection into an array
        /// </summary>
		/// <param name="array">The target array to receive the records.</param>
		/// <param name="arrayIndex">The index in the target array to receive the first record.</param>
        public void CopyTo(Record[] array, int arrayIndex)
        {
            this.SparseArray.CopyTo(array, arrayIndex, this.SparseArray);
        }

                #endregion //CopyTo	

				#region ExpandAll

        // SSP 7/8/09 - TFS19155
		// 
		/// <summary>
		/// Expands all records.
		/// </summary>
		/// <param name="recursive">Whether to expand descendant records as well.</param>
		/// <seealso cref="CollapseAll(int)"/>
		public void ExpandAll( bool recursive )
		{
			this.ExpandCollapseAllHelper( true, recursive ? -1 : 0 );
		}

		/// <summary>
		/// Expands all records recursively upto the recursionDepth.
		/// </summary>
		/// <param name="recursionDepth">Number of levels to recurse down. 0 means just this level.</param>
		public void ExpandAll( int recursionDepth )
		{
			GridUtilities.ValidateNonNegative( recursionDepth );

			this.ExpandCollapseAllHelper( true, recursionDepth );
		}

				#endregion // ExpandAll
   
                #region IndexOf

        /// <summary>
        /// Returns the zero-based index of the record in the collection
        /// </summary>
        public int IndexOf(Record item)
        {
            return this.SparseArray.IndexOf(item);
        }

                #endregion //IndexOf	

				#region RefreshSort

		// SSP 7/29/08 
		// Added this method to position the row according to the current
		// sort criteria without having to resort the whole rows collection.
		// 
		/// <summary>
		/// Sorts the records based on the current sort criteria.
		/// </summary>
		/// <remarks>
		/// <para class="body">
		/// <b>RefreshSort</b> sorts the records in this collection to be in
		/// the sort order specified by the FieldLayout's <see cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		/// collection.
		/// </para>
		/// <para class="body">
		/// Also Record's <see cref="Record.RefreshSortPosition"/> method can be used to re-position an individual
		/// record in the correct sort position after modifying its data.
		/// </para>
		/// </remarks>
		/// <seealso cref="Record.RefreshSortPosition"/>
		/// <seealso cref="Infragistics.Windows.DataPresenter.FieldLayout.SortedFields"/>
		public void RefreshSort( )
		{
            // JJD 12/02/08 - TFS6743/BR35763
            // If the rm is in Reset then we can ignore this request since 
            // all gthe records will be sorted in one shot
            if (this._parentRecordManager == null ||
                 this._parentRecordManager.IsInReset)
                return;

			// JM 07-07-09 - TFS19056 Decrement the SortVersion number so it is different than the FieldLayout.SortVersion
			//				 to force VerifySortOrder to perform the sort.
			this._sortVersion--;

            this.VerifySortOrder(false);

            // JJD 6/22/09 - NA 2009 Vol 2 - Record Fixing
            // Since the developer explicitly called refreshsort we need to sort any fixed
            // cached by ViewableRecordCollection
            ViewableRecordCollection vrc = this._parentRecordManager.ViewableRecords;
            if (vrc != null)
                vrc.RefreshSortOrderOfFixedRecords();
		}

				#endregion // RefreshSort
        
            #endregion//Public Methods
        
            #region Internal Methods

                #region AddRecord

        internal void AddRecord(Record record)
        {
            this.InsertRecord(this.SparseArray.Count, record);
        }

                #endregion //AddRecord

                #region AllocateNewRecord

        internal virtual Record AllocateNewRecord( int index )
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_11" ) );
        }

                #endregion //AllocateNewRecord	

				// JJD 4/27/07
				// Optimization - added update count to bypass notifications while building a list
				#region BeginUpdate

		internal void BeginUpdate()
		{
			// SSP 2/11/09 TFS12467
			// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
			// 
			//this._beginUpdateCount++;
			RecordManager rm = this.ParentRecordManager;
			Debug.Assert( null != rm );
			if ( null != rm )
				rm.BeginUpdate( );
		}

				#endregion //BeginUpdate	

				// JM 6-28-10 TFS33366 - Added.
				#region BumpCollectionVersion

		internal void BumpCollectionVersion()
		{
			if (this._bumpVersionOperation				== null &&
				this._parentRecordManager				!= null &&
				this._parentRecordManager.DataPresenter	!= null)
				this._bumpVersionOperation = this._parentRecordManager.DataPresenter.Dispatcher.BeginInvoke(DispatcherPriority.Render, new GridUtilities.MethodDelegate(OnBumpVersionNumber));
		}

				#endregion //BumpCollectionVersion
    
                #region CreateSparseArray

                // JJD 2/25/09 - Optimization - no longer needed
        //internal abstract RecordSparseArrayBase CreateSparseArray();

                #endregion //CreateSparseArray	
    
                #region Clear

        internal void Clear()
        {
            if (this.SparseArray.Count > 0)
            {
                this.SparseArray.Clear();
                this.RaiseChangeEvents(true);
            }
        }

                #endregion //RemoveRecord

                #region CreateGroupByRecordsHelper







        internal void CreateGroupByRecordsHelper(DataRecord[] records)
        {
            // JJD 1/28/09 - TFS12487
            // Call the InitializeGroupByField method to cache the value
            this.InitializeGroupByField();

			int tmp = 0;
			this.CreateGroupByRecordsHelper( records, ref tmp );
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal void CreateGroupByRecordsHelper(DataRecord[] records, ref int startIndex)
        {
            Field field = this.GroupByField;

            GroupByRecord parentGroupByRecord = this.ParentRecord as GroupByRecord;

            //if ( parentGroupByRecord == null )
            //    return;

			// copy over the existing rcds into an array so we can try
			// to reuse them
            // JJD 2/25/08 - Optimization
            // Use a Dictionary, keyed by the Value, instead of a List.
            //List<GroupByRecord> existingGroupByRcds = new List<GroupByRecord>(this.Count);
			Dictionary<object, GroupByRecord> existingGroupByRcds = new Dictionary<object, GroupByRecord>(this.Count);
			foreach (Record rcd in this.SparseArray.NonNullItems)
			{
                GroupByRecord gbr = rcd as GroupByRecord;

                if (gbr != null)
				{
                    if (gbr.GroupByField != field)
						break;

                    // JJD 2/25/08 - Optimization
                    // Use a Dictionary, keyed by the Value, instead of a List.
					//existingGroupByRcds.Add(rcd as GroupByRecord);
                    // JJD 3/25/08 - Fixed regression cause by optimization of using a dictionary
                    // The Dictionary class throws an exception when trying to use a key of null
                    // so we need to use GetValueKey method which will never return null. 
                    // Instead it returns a singleton object to represent a null value.
                    //existingGroupByRcds[gbr.Value] = gbr;
					existingGroupByRcds[GetValueKey( gbr.Value ) ] = gbr;
				}
			}

            IGroupByEvaluator evaluator = null;

            if ( field != null )
                evaluator = field.GroupByEvaluatorResolved;

			// JJD 4/27/07
			// Optimization - added update count to bypass notifications while building a list
			this.BeginUpdate();

			try
			{
				
				
				
				RecordSparseArrayBase sparseArray = this.SparseArray;

				// clear our list
				sparseArray.Clear();

				GroupByRecord groupByRecord = null;

				//Debug.WriteLine( "CreateGroupByRecordsHelper Start:  startIndex = " + startIndex );

				while (startIndex < records.Length)
				{
					DataRecord record = records[startIndex];

					if (null != parentGroupByRecord)
					{
						// We always want to add at least one child record to a group by record.
						if (sparseArray.Count > 0
							&& !parentGroupByRecord.DoesRecordMatchGroup(record))
						{
							break;
						}
					}

					// Is it time to create a new GroupByRecord ?
					if (null == groupByRecord || !groupByRecord.DoesRecordMatchGroup(record, false, evaluator))
					{
						bool newGroupByRecordCreated = false;
						FieldLayout layout;
                        GroupByRecord gbr = null;

						// JJD 2/23/07 - BR19829
						// Also create a field layout groupby record if the 
						// groupby field's owner doesn't match the record's field layout.
						// This will happen when we are dealing with heterogenieous data
						if (field == null ||
							field.Owner != record.FieldLayout)
						{
							layout = record.FieldLayout;

                            // JJD 5/19/09 - NA 2009 vol2 - Cross band grouping
                            // See if there is an existing GroupByRecord keyed by the
                            // field layout. If so use the existing one so we
                            // can maintain its state (e.g. IsExpanded)
                            if (existingGroupByRcds.TryGetValue(layout, out gbr))
                            {
                                existingGroupByRcds.Remove(layout);
                                groupByRecord = gbr;
                            }
                            else
                            {
                                groupByRecord = new GroupByRecord(layout, this);

                                newGroupByRecordCreated = true;
                            }
						}
						else
						{
							layout = field.Owner;
							groupByRecord = null;

                            // JJD 2/25/08 - Optimization
                            // Use the cellValue as the key into the Dictionary 
                            // JJD 5/29/09 - TFS18063 
                            // Use the new overload to GetCellValue which will return the value 
                            // converted into EditAsType
                            //object cellValue = record.GetCellValue(field, true);
							object cellValue = record.GetCellValue(field, CellValueType.EditAsType);

                            // JJD 3/25/08 - Fixed regression cause by optimization of using a dictionary
                            // The Dictionary class throws an exception when trying to use a key of null
                            // so we need to use GetValueKey method which will never return null. 
                            // Instead it returns a singleton object to represent a null value.
                            //if (existingGroupByRcds.TryGetValue(cellValue, out gbr))
                            object cellValueKey = GetValueKey(cellValue);
                            if ( existingGroupByRcds.TryGetValue(cellValueKey, out gbr))
                            {
                                if (gbr != null &&
                                    gbr.DoesRecordMatchGroup(record, false, evaluator))
                                {
                                    // JJD 3/25/08 - Fixed regression
                                    //existingGroupByRcds.Remove(cellValue);
                                    existingGroupByRcds.Remove(cellValueKey);
                                    groupByRecord = gbr;
                                }
                            }

                            // JJD 2/25/08 - Optimization
                            // If the groupbyrecord wasn't found based on its cellvalue key
                            // but there is a custom evaluator then iterate over the
                            // existong records using the old brute force approach
                            if (groupByRecord == null && evaluator != null
								// SSP 5/29/09 - TFS17233 - Optimization
								// Don't create the enumerator if Count is 0.
								&& existingGroupByRcds.Count > 0
								)
                            {
                                // JJD 2/25/08
                                // Loop over dictionary instead
                                #region Old code

                                //for (int i = 0; i < existingGroupByRcds.Count; i++)
                                //{
                                //    GroupByRecord gbr = existingGroupByRcds[i];

                                //    if (gbr != null &&
                                //        gbr.DoesRecordMatchGroup(record, false))
                                //    {
                                //        groupByRecord = gbr;
                                //        existingGroupByRcds[i] = null;
                                //        break;
                                //    }
                                //}

                                #endregion //Old code
                                foreach (KeyValuePair<object, GroupByRecord> entry in existingGroupByRcds)
                                {
                                    gbr = entry.Value;

                                    if (gbr != null &&
                                        gbr.DoesRecordMatchGroup(record, false, evaluator))
                                    {
                                        groupByRecord = gbr;
                                        existingGroupByRcds.Remove(entry.Key);
                                        break;
                                    }
                                }
                            }

                            if (groupByRecord == null)
                            {
                                //object cellValue = record.GetCellValue(field);
                                groupByRecord = new GroupByRecord(field, this, cellValue);
                                newGroupByRecordCreated = true;
                            }
                            else
                            {
                                // JJD 5/27/09 - TFS17889
                                // Since we are re-using the groupbyrecord we should dirty
                                // the special records so they will get re-verified
                                // asynchronously
                                ViewableRecordCollection vrc = groupByRecord.ViewableChildRecords;
                                if (vrc != null)
                                    vrc.DirtySpecialRecords(false);
                            }
						}

						sparseArray.Add(groupByRecord);

						if (newGroupByRecordCreated)
						{
							// If they have specified an evaulator then get the group by value 
							//
                            // JJD 4/17/08
                            // Make sure GroupByField is not null
                            //if (evaluator != null && groupByRecord.GroupByField != null)
                            if (evaluator != null && groupByRecord.GroupByField != null)
								groupByRecord.InitializeCommonValue(evaluator.GetGroupByValue(groupByRecord, record));

                            // JJD 2/25/08
                            // Delay raising InitializeRecord event until after its ChildRecords collection
                            // has been populated
							// raise InitializeRecord event
							//groupByRecord.FireInitializeRecord();
						}

						int oldStartIndex = startIndex;

                        // JJD 1/28/09 - TFS12487
                        // Call the InitializeGroupByField method to cache the value
						
						
						
						RecordCollection childRecords = groupByRecord.ChildRecords;
                        childRecords.InitializeGroupByField();

						
						
						
						
						
						
						if ( null != childRecords.GroupByField )
						{
							childRecords.CreateGroupByRecordsHelper(records, ref startIndex);
						}
						else
						{
							
							
							
							
							childRecords.CreateGroupByRecordRecords( records, ref startIndex, evaluator );
						}

						// JJD 7/20/07
						// Raise a reset event on the child records collection if the groupby 
						// record is being re-used.
						if (newGroupByRecordCreated == false)
						{
							
							
							
							
							childRecords.RaiseChangeEvents( true );

							// JM 05-30-08 BR33286 - Notify the GroupByRecord that the grouping has changed so it
							// can fire a property changed notification for its description property.
							groupByRecord.OnGroupingChanged();
						}
						else
						{
							// JJD 2/25/08
							// Now that we have populated Ithe ChildRecords collection
							// we can raise InitializeRecord event
							groupByRecord.FireInitializeRecord();
						}


						Debug.Assert(startIndex != oldStartIndex, "Some records should have been added to the child group by records");

						if (startIndex <= oldStartIndex)
							startIndex = 1 + oldStartIndex;
					}
				}

				// Added GroupByComparer property to allow the user to sort group-by 
				// records using a custom comparer.
				IComparer groupBySortComparer = null;

				if (field != null)
					groupBySortComparer = field.GroupByComparerResolved;

				if (groupBySortComparer != null)
					this.SortGroupByRecords(groupBySortComparer, SortStatus.Ascending != field.SortStatus);

				this.InitializeSortVersion();

                // JJD 3/19/09 - TFS15701
                this.InitializeSortStatus();
            }
			finally
			{
                // JJD 8/27/09 - TFS21513
                // Dirty the FixedRecordOrder so it will get verified the next time
                this.DirtyFixedRecordOrder();

				// JJD 4/27/07
				// Optimization - added update count to bypass notifications while building a list
				this.EndUpdate(false);
			}
        }

                #endregion // CreateGroupByRecordsHelper

                #region DirtyParentRecordScrollCount

		// SSP 2/11/09 TFS12467
		// Re-worked the way we manage scroll counts. Commented out the following method.
		// 
		
#region Infragistics Source Cleanup (Region)






















#endregion // Infragistics Source Cleanup (Region)

                #endregion //DirtyParentRecordScrollCount	

				// JJD 4/27/07
				// Optimization - added update count to bypass notifications while building a list
				#region EndUpdate

		// JJD 5/19/07
		// Optimization - Added dirtyScrollCount flag on EndUpdate method
		//internal void EndUpdate()
		internal void EndUpdate(bool dirtyScrollCount)
		{
			// SSP 2/11/09 TFS12467
			// Moved BeginUpdate/EndUpdate to RecordManager from RecordCollectionBase. Also reimplemented it.
			// 
			RecordManager rm = this.ParentRecordManager;
			Debug.Assert( null != rm );
			if ( null != rm )
				rm.EndUpdate( dirtyScrollCount );
			
#region Infragistics Source Cleanup (Region)

























#endregion // Infragistics Source Cleanup (Region)

		}

				#endregion //EndUpdate	

                #region EnsureNotDirty

        internal virtual void EnsureNotDirty()
        {
            
        }

                #endregion //EnsureNotDirty

				#region ExpandCollapseAllHelper

		// SSP 7/8/09 - TFS19155
		// 
		
		
		
		
		private void ExpandCollapseAllHelper( bool expand, int recursionDepth )
		{
			this.BeginUpdate( );

			DataPresenterBase dp = this.DataPresenter;
			RecordListControl rlc = null;

            Record parentRecord = this.ParentRecord;

            // If the parent record is null (i.e. a collection of root records) or
            // it is expanded along with all of its ancestors then get the
            // root recordlistcontrol so we can temporarily unhook its ItemsSource
            // to optimize the process
            if (parentRecord == null ||
                (parentRecord.IsExpanded && !parentRecord.HasCollapsedAncestor))
            {
                rlc = null != dp ? dp.RootRecordListControl : null;
            }

			RecyclingItemContainerGenerator generator = null != rlc ? rlc.RecyclingItemContainerGenerator : null;
			if ( null != generator )
			{
				// MD 3/17/11 - TFS34785
				// We want to recycle/deactivate containers when possible if we are in flat view, so don't remove 
				// all containers from the generator.
				//generator.RemoveAll();
				if (dp == null || dp.IsFlatView == false)
				    generator.RemoveAll();
			}

			IEnumerable oldItemsSource = null;
			if ( null != rlc )
			{
				oldItemsSource = rlc.ItemsSource;
				
				// JJD 11/21/11 - TFS69163
				// Instead of setting the ItemsSource to null (which will blow away any existing bindings)
				// call BeginCoerceItemsSourceToNull which will temporarily cause the property to return null
				//rlc.ItemsSource = null;
				if (oldItemsSource != null)
				{
					rlc.BeginCoerceItemsSourceToNull();
					rlc.UpdateLayout();
				}
			}

			try
			{
				foreach ( Record record in this )
				{
					
					
					
					
					if ( 0 != recursionDepth && record.HasChildren )
					{
						RecordCollectionBase childRecords = record.ChildRecordsInternal;
						if ( null != childRecords )
							
							
							
							
							childRecords.ExpandCollapseAllHelper( expand, recursionDepth - 1 );
					}

					record.IsExpanded = expand;
				}
			}
			finally
			{
				this.EndUpdate( true );

				// JJD 11/21/11 - TFS69163
				// Instead of setting the ItemsSource back to its old value (which could blow away any existing bindings)
				// call EndCoerceItemsSourceToNull which will undo the temporarily coercion of the property to null
				// causing by the call to BeginCoerceItemsSourceToNull above.
				//if ( null != rlc )
				//    rlc.ItemsSource = oldItemsSource;
				if ( null != rlc && oldItemsSource != null)
					rlc.EndCoerceItemsSourceToNull();
			}
		}

				#endregion // ExpandCollapseAllHelper

				// AS 6/25/09 NA 2009.2 Field Sizing
				#region GetAllRecords
		/// <summary>
		/// Helper method to return all records including hidden records and special records on the viewable record collection.
		/// </summary>
		internal IEnumerable<Record> GetAllRecords()
		{
			IEnumerator<Record> specialRecords = this.ViewableRecords.GetSpecialRecords(true);

			if (null != specialRecords)
			{
				while (specialRecords.MoveNext())
					yield return specialRecords.Current;
			}

			foreach (Record record in this)
				yield return record;

			specialRecords = this.ViewableRecords.GetSpecialRecords(false);

			if (null != specialRecords)
			{
				while (specialRecords.MoveNext())
					yield return specialRecords.Current;
			}
		} 
				#endregion //GetAllRecords

				#region GetRecordEnumerator

		/// <summary>
		/// Gets the records associated with this records collection as well as descendant record collections
		/// upto the lowestLevelFieldLayout.
		/// </summary>
		/// <param name="recordType">Whether to get data records or group-by records.</param>
		/// <param name="fieldLayoutToMatch">If this parameter is non-null then records from
		/// only this field layout will be returned. If it is null, then records from all field layouts will be returned.</param>
		/// <param name="lowestLevelFieldLayout">If this parameter is non-null then records from
		/// field layouts upto the lowestLevelFieldLayout (inclusive) will be returned.</param>
		/// <param name="visibleRecordsOnly">If true then only the visible records will be returned.</param>
		/// <returns>Returns records based on the specified parameters.</returns>
		internal IEnumerable<Record> GetRecordEnumerator(RecordType recordType, FieldLayout fieldLayoutToMatch, 
			FieldLayout lowestLevelFieldLayout, bool visibleRecordsOnly )
		{
			IEnumerable<Record> allRecords = new RecursiveRecordsEnumerator.Enumerable( 
				this, lowestLevelFieldLayout, visibleRecordsOnly );

			GridUtilities.IMeetsCriteria filter = new GridUtilities.MeetsCriteria_RecordType( recordType );

			if ( null != fieldLayoutToMatch )
				filter = new GridUtilities.MeetsCriteriaChain( filter, 
					new GridUtilities.MeetsCriteria_RecordFieldLayout( fieldLayoutToMatch ),
					false );

			return new GridUtilities.MeetsCriteriaEnumerator<Record>.Enumerable( allRecords, filter );
		}

				#endregion // // GetRecordEnumerator

				// AS 8/7/09 NA 2009.2 Field Sizing
				#region GetSpecialRecords
		internal IEnumerable<Record> GetSpecialRecords()
		{
			for (int i = 0; i < 2; i++)
			{
				IEnumerator<Record> specialRecords = this.ViewableRecords.GetSpecialRecords(i == 0);

				if (null != specialRecords)
				{
					while (specialRecords.MoveNext())
						yield return specialRecords.Current;
				}
			}
		} 
				#endregion //GetSpecialRecords

				#region HasAdditionalGroupByFields






        internal static bool HasAdditionalGroupByFields(FieldLayout layout, Field field)
        {
            if (field == null)
            {
                Debug.Assert(layout != null);

                return layout != null && layout.HasGroupBySortFields;
            }

            int index = field.Owner.SortedFields.IndexOf(field);

            Debug.Assert(index >= 0);

            return (index < field.Owner.SortedFields.CountOfGroupByFields - 1);
        }

                #endregion // HasAdditionalGroupByFields

				#region InitializeSortVersion

		internal void InitializeSortVersion()
		{
			if (this._fieldLayout != null)
			{
				this._groupByVersion	= this._fieldLayout.GroupByVersion;
				this._sortVersion		= this._fieldLayout.SortVersion;
			}
		}

				#endregion //InitializeSortVersion	

                // JJD 3/19/09 - TFS15701 - added
				#region InitializeSortStatus

		internal void InitializeSortStatus()
		{
			if (this._groupByField != null)
				this._previousSortStatus	= this._groupByField.SortStatus;
		}

				#endregion //InitializeSortVersion	
    
                #region InitializeFieldLayout

        internal void InitializeFieldLayout(FieldLayout layout)
        {
            // this method should only ever be called for a collection of DataRecords
            Debug.Assert(this is DataRecordCollection || this is GroupByRecordCollection);

			
			
			
			
            
			this.InternalSetFieldLayout( layout );
        }

                #endregion //InitializeFieldLayout	
    
                #region InsertRecord

        internal void InsertRecord(int index, Record record)
        {
            this.SparseArray.Insert(index, record);

            this.RaiseChangeEvents(false);
            this.OnCollectionChanged(NotifyCollectionChangedAction.Add, record, index);
        }

                #endregion //InsertRecord

                #region RaiseChangeEvents

        internal void RaiseChangeEvents(bool fullReset)
        {
            this.OnPropertyChanged("Count");
            this.OnPropertyChanged("Item[]");
            if (fullReset)
                this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, -1);
        }

                #endregion //RaiseChangeEvents	
    
                #region RemoveAt

        internal void RemoveAt(int index)
        {
            Record record = this.SparseArray.GetItem(index, null) as Record;

			// SSP 1/30/08 BR30266
			// We still need to remove slot if the record hasn't been allocated yet.
			// Took out the check for null.
			// 
            //if (record != null)
            //{
                this.SparseArray.RemoveAt(index);

                this.RaiseChangeEvents(false);
                this.OnCollectionChanged(NotifyCollectionChangedAction.Remove, record, index);
            //}
        }
		
                #endregion //RemoveRecord

                #region RemoveRecord

        internal void RemoveRecord(Record record)
        {
            int index = this.SparseArray.IndexOf(record);

            if ( index >= 0)
                this.RemoveAt(index);
        }

                #endregion //RemoveRecord

                #region VerifySortOrder

        /// <summary>
        /// Verifies that the sort order is correct
        /// </summary>
        // JJD 3/19/09 - TFS15701
        // Made non-virtual and moved logic from overrides to here
        //internal virtual void VerifySortOrder(bool recursive)
		internal void VerifySortOrder(bool recursive)
        {
            // first sort our items
            if (this.FieldLayout != null &&
                !(this is ExpandableFieldRecordCollection) &&
                this.SparseArray.Count > 1 &&
                this.SortVersion != this.FieldLayout.SortVersion)
            {
                this.InitializeSortVersion();

                Record firstRecord = this[0];

				// SSP 7/29/08 
				// Ensure that all items are created. We don't want to sort with empty
				// slots in the sparse array.
				// 
				if ( !( firstRecord is GroupByRecord ) )
				{
					// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// Enclosed the existing code in the if block.
					// 
					if ( null == _parentRecordManager || SortEvaluationMode.Auto == _parentRecordManager.SortEvaluationModeResolved )
					{
						this.SparseArray.EnsureAllItemsCreated( );

						IComparer comparer;

						// JJD 3/19/09 - TFS15701
						// If the parent record is a groupby record then there can only be one
						// fieldlayout so use the more efficient SameFieldRecordsSortComparer
						if ( firstRecord.ParentRecord is GroupByRecord )
							// SSP 5/29/09 - TFS17233 - Optimization
							// Pass along the new areRecordsInUnsortedOrder parameter.
							// 
							//comparer = new RecordManager.SameFieldRecordsSortComparer(firstRecord.FieldLayout, this.Count);
							comparer = new RecordManager.SameFieldRecordsSortComparer( firstRecord.FieldLayout, this.Count, false );
						else
							comparer = new RecordManager.RecordsSortComparer( false );

						this.SparseArray.Sort( comparer );
					}
				}
				else
				{
					// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
					// Enclosed the existing code in the if block.
					// 
					if ( null == _parentRecordManager || GroupByEvaluationMode.Auto == _parentRecordManager.GroupByEvaluationModeResolved )
					{
						Field field = this.GroupByField;

						if ( field != null )
						{
							IComparer comparer = field.GroupByComparerResolved;

							if ( comparer == null )
							{
								// JJD 3/19/09 - TFS15701
								// if they didn't supply a comparer then
								// just reverse the sparse array if the previous sort order
								// was different
								if ( this._previousSortStatus != field.SortStatus )
									this.SparseArray.Reverse( );
							}
							else
							{
								this.SortGroupByRecords( comparer, SortStatus.Ascending != field.SortStatus );
							}

							// JJD 3/19/09 - TFS15701
							// update the cached sort status
							this._previousSortStatus = field.SortStatus;
						}
					}
				}

                // JJD 8/27/09 - TFS21513
                // Dirty the FixedRecordOrder so it will get verified the next time
				// JJD 04/13/12 - TFS104021
				// Only dirty the fixed rcd order if FixedRecordSortOrderResolved is 'Sorted'
				if ( _fieldLayout != null && _fieldLayout.FixedRecordSortOrderResolved == FixedRecordSortOrder.Sorted )
					this.DirtyFixedRecordOrder();

                this.OnPropertyChanged("Item[]");
                this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, -1);
            }

            if (recursive && this.Count > 0)
			{
				foreach (Record rcd in this.SparseArray.NonNullItems)
				{
					if (rcd.IsExpanded)
						rcd.VerifySortOrderOfChildren();
				}
			}

			// SSP 6/27/08 BR33815
			// 
			if ( null != _summaryResults )
				_summaryResults.RefreshSummariesAffectedBySort( );
        }

                #endregion //VerifySortOrder	

            #endregion //Internal Methods

            #region Protected Methods
    
                #region OnCollectionChanged

        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        internal protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            if (this._collectionChangedHandler != null)
            {
                if (item is IList)
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, (IList)item, index));
                else
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, item, index));
            }

        }

        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        internal protected virtual void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            if (this._collectionChangedHandler != null)
            {
                if (item is IList)
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, (IList)item, index, oldIndex));
                else
                    this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
            }
        }

                #endregion //OnCollectionChanged

                #region OnPropertyChanged

        /// <summary>
        /// Raises the PropertyChanged event
        /// </summary>
        /// <param name="propertyName"></param>
        internal protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this._propertyChangedHandler != null)
                this._propertyChangedHandler(this, new PropertyChangedEventArgs(propertyName));
        }

                #endregion //OnPropertyChanged

                #region SortGroupByRecords

        /// <summary>
        /// Sorts the groupby records in the collection
        /// </summary>
        protected virtual void SortGroupByRecords(IComparer comparer, bool reverse)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_12" ) );
        }

                #endregion //SortGroupByRecords	
    
            #endregion //Protected Methods

            #region Private Methods

				#region CreateGroupByRecordRecords



#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

		
		
		
        
		private void CreateGroupByRecordRecords( DataRecord[] records, ref int startIndex, IGroupByEvaluator groupByEvaluator )
        {
			// JJD 4/27/07
			// Optimization - added update count to bypass notifications while building a list
			this.BeginUpdate();

			try
			{
				
				
				
				RecordSparseArrayBase sparseArray = this.SparseArray;

				// clear our list
				sparseArray.Clear();

				
				
				
				
				//this._fieldLayout = null;
				this.InternalSetFieldLayout( null );

				GroupByRecord parentGroupByRecord = this.ParentRecord as GroupByRecord;

				//Debug.WriteLine( "CreateGroupByRecordRecords start:  startIndex = " + startIndex );

				while (startIndex < records.Length)
				{
					DataRecord record = records[startIndex] as DataRecord;

					if (this._fieldLayout == null)
						
						
						
						
						//this._fieldLayout = record.FieldLayout;
						this.InternalSetFieldLayout( record.FieldLayout );

					if (null != parentGroupByRecord)
					{
						// Note: we always want to add at least one child record to a group by record.
						//
						if (sparseArray.Count > 0 &&
							
							
							
							
							!parentGroupByRecord.DoesRecordMatchGroup( record, true, groupByEvaluator )
							)
						{
							break;
						}
					}

					// set the parent record to this collections owner
					record.InitializeParentCollection(this);

					sparseArray.Add(record);

					startIndex++;
				}

				this.InitializeSortVersion();
			}
			finally
			{
                // JJD 8/27/09 - TFS21513
                // Dirty the FixedRecordOrder so it will get verified the next time
                this.DirtyFixedRecordOrder();

				// JJD 4/27/07
				// Optimization - added update count to bypass notifications while building a list
				this.EndUpdate(false);
			}
        }

                #endregion // CreateGroupByRecordRecords

                // JJD 8/27/09 - TFS21513 - added
                #region DirtyFixedRecordOrder

        /// <summary>
        /// For internal use only
        /// </summary>
        internal protected void DirtyFixedRecordOrder()
        {
            ViewableRecordCollection vrc = ViewableRecordsIfAllocated;

            // Dirty the FixedRecordOrder so it will get verified the next time
            if (vrc != null)
                vrc.DirtyFixedRecordOrder();
        }

                #endregion //DirtyFixedRecordOrder	
    
                // JJD 3/25/08 - added
                #region GetValueKey

        private static object NullKey = new object();
        internal static object GetValueKey(object key)
        {
            if (key != null)
                return key;

            // return a singleton object to represent a null key
            // this is because a dictionary throws an exception trying to use a null key
            return NullKey;
        }

                #endregion //GetValueKey	

                // JJD 1/28/09 - TFS12487 - added
                #region InitializeGroupByField

		// SSP 2/21/12 NAS12.1 External Sorting/Filtering/Grouping/Summaries
		// Changed the scope to internal from private.
		// 
        //private void InitializeGroupByField()
		internal void InitializeGroupByField( )
        {
            this._groupByField = null;

            // If there are records from multiple field layouts then we 
            // are grouping by field layout not a field
            if (this._fieldLayout == null)
                return;

            if (null == this.ParentRecord)
            {
                // JJD 7/1/09 - TFS18067
                // If we don't have a parent record and our recordmanager
                // has multiple field layouts then we want to leave groupbyfield
                // un-initialized because this collction will contain special
                // GroupByFieldLayout rcds
                RecordManager rm = this.ParentRecordManager;
                if (rm != null && rm.HasMultipleFieldLayouts)
                    return;

                if (this._fieldLayout.HasGroupBySortFields)
                {
                    this._groupByField = this._fieldLayout.SortedFields[0].Field;
                    return;
                }
            }
            else if (this.ParentRecord is GroupByRecord)
            {
                GroupByRecord parentGroupByRecord = (GroupByRecord)this.ParentRecord;

                int index;

                if (parentGroupByRecord.RecordType == RecordType.GroupByFieldLayout)
                    index = 0;
                else
                {
                    index = parentGroupByRecord.FieldLayout.SortedFields.IndexOf(parentGroupByRecord.GroupByField);
                    if (index < 0)
                        return;
                    index++;
                }

                // JJD 5/19/09 - TFS17766
                // The index test should be greater than or equal to
                //if (index > 0)
                if (index >= 0)
                {
                    FieldSortDescription fsd = null;

                    if (index < parentGroupByRecord.FieldLayout.SortedFields.Count)
                    {
                        fsd = parentGroupByRecord.FieldLayout.SortedFields[index];

                        if (null != fsd && fsd.IsGroupBy)
                        {
                            this._groupByField = fsd.Field;
                        }
                    }
                }
            }
            else if (this.IsTopLevel && this._fieldLayout != null)
            {
                if (this._fieldLayout.HasGroupBySortFields)
                    this._groupByField = this._fieldLayout.SortedFields[0].Field;
            }

            // JJD 3/19/09 - TFS15701
            this.InitializeSortStatus();
        }

                #endregion //InitializeGroupByField	
        

				#region InternalSetFieldLayout

		
		
		
		
		private void InternalSetFieldLayout( FieldLayout fieldLayout )
		{
			_fieldLayout = fieldLayout;
			if ( null != _viewableRecordCollection )
				_viewableRecordCollection.OnRecordCollectionFieldLayoutChanged( );				
		}

				#endregion // InternalSetFieldLayout

				// JM 6-28-10 TFS33366 - Added.
				#region OnBumpVersionNumber

		private void OnBumpVersionNumber()
		{
			this._collectionVersion++;
			this._bumpVersionOperation = null;

			this.OnPropertyChanged("CollectionVersion");
		}

				#endregion //OnBumpVersionNumber

			#endregion //Private Methods

		#endregion //Methods

		#region Indexer (int)

		/// <summary>
        /// The record at the specified zero-based index (read-only)
        /// </summary>
		/// <remarks>
		/// <para class="note"><b>Note: </b>Since <see cref="Record"/> is an abstract base class for <see cref="DataRecord"/>, <see cref="GroupByRecord"/> and <see cref="ExpandableFieldRecord"/> you may have to cast this property to the appropiate derived class to access specific properties, e.g. the <see cref="DataRecord"/>'s <see cref="DataRecord.Cells"/> collection.</para>
		/// </remarks>
		/// <seealso cref="DataRecord"/>
		/// <seealso cref="GroupByRecord"/>
		/// <seealso cref="ExpandableFieldRecord"/>
		public Record this[int index]
        {
            get
            {
                return this.SparseArray.GetItem(index, true) as Record;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion //Indexer (int)	
    
        #region INotifyCollectionChanged Members

        /// <summary>
        /// Occurs when a changes are made to the colleciton, i.e. records are added, removed or the entire collection is reset.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this._collectionChangedHandler = System.Delegate.Combine(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this._collectionChangedHandler = System.Delegate.Remove(this._collectionChangedHandler, value) as NotifyCollectionChangedEventHandler;
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Raised when a property has changed
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                this._propertyChangedHandler = System.Delegate.Combine(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                this._propertyChangedHandler = System.Delegate.Remove(this._propertyChangedHandler, value) as PropertyChangedEventHandler;
            }
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool IList.Contains(object value)
        {
            return this.SparseArray.Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return this.SparseArray.IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return true; }
        }

        void IList.Remove(object value)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        object IList.this[int index]
        {
            get
            {
                return this.SparseArray.GetItem(index, true);
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            this.SparseArray.CopyTo(array, index, this.SparseArray);
        }

        bool ICollection.IsSynchronized
        {
            get { return this.SparseArray.IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return this.SparseArray.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SparseArray.GetEnumerator( this.SparseArray );
        }

        #endregion

        #region IList<Record> Members

        void IList<Record>.Insert(int index, Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void IList<Record>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region ICollection<Record> Members

        void ICollection<Record>.Add(Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        void ICollection<Record>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        bool ICollection<Record>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<Record>.Remove(Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region IEnumerable<Record> Members

        IEnumerator<Record> IEnumerable<Record>.GetEnumerator()
        {
            return new RecordEnumerator(this.SparseArray);
        }

        #endregion

        #region RecordEnumerators

            #region RecordEnumeratorBase

        internal abstract class RecordEnumeratorBase : IEnumerator
        {
            IEnumerator _enumerator;

            internal RecordEnumeratorBase(IEnumerator enumerator)
            {
                this._enumerator = enumerator;
            }

            #region IEnumerator<Record> Members

            public Record Current
            {
                get { return this._enumerator.Current as Record; }
            }

            #endregion

            public void Dispose()
            {
            }

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get
                {
                    return this._enumerator.Current;
                }
            }

            public bool MoveNext()
            {
                return this._enumerator.MoveNext();
            }

            public void Reset()
            {
                this._enumerator.Reset();
            }

            #endregion
        }

            #endregion //RecordEnumeratorBase	
    
            #region RecordEnumerator

        internal class RecordEnumerator : RecordEnumeratorBase, IEnumerator<Record>
        {
            internal RecordEnumerator(RecordSparseArrayBase sparseArray) : base( sparseArray.GetEnumerator(sparseArray)) { }
            internal RecordEnumerator(IEnumerator enumerator) : base(enumerator) { }

            public new Record Current
            {
                get { return base.Current as Record; }
            }
        }

            #endregion //RecordEnumerator	
    
            #region DataRecordEnumerator

        internal class DataRecordEnumerator : RecordEnumeratorBase, IEnumerator<DataRecord>
        {
            internal DataRecordEnumerator(RecordSparseArrayBase sparseArray) : base(sparseArray.GetEnumerator(sparseArray)) { }

            public new DataRecord Current
            {
                get { return base.Current as DataRecord; }
            }
        }

            #endregion //DataRecordEnumerator
    
            #region GroupByRecordEnumerator

        internal class GroupByRecordEnumerator : RecordEnumeratorBase, IEnumerator<GroupByRecord>
        {
            internal GroupByRecordEnumerator(RecordSparseArrayBase sparseArray) : base(sparseArray.GetEnumerator(sparseArray)) { }

            public new GroupByRecord Current
            {
                get { return base.Current as GroupByRecord; }
            }
        }

            #endregion //GroupByRecordEnumerator
    
            #region ExpandableFieldRecordEnumerator

        internal class ExpandableFieldRecordEnumerator : RecordEnumeratorBase, IEnumerator<ExpandableFieldRecord>
        {
            internal ExpandableFieldRecordEnumerator(RecordSparseArrayBase sparseArray) : base(sparseArray.GetEnumerator(sparseArray)) { }

            public new ExpandableFieldRecord Current
            {
                get { return base.Current as ExpandableFieldRecord; }
            }
        }

            #endregion //ExpandableFieldRecordEnumerator
	
        #endregion //RecordEnumerators

    }

    #endregion //RecordCollectionBase class	

    #region RecordCollection class

    /// <summary>
    /// A collection of Records used by the DataPresenterBase control
    /// </summary>
	/// <remarks>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="DataRecord"/>
	/// <seealso cref="GroupByRecord"/>
	/// <seealso cref="ExpandableFieldRecord"/>
	public class RecordCollection : RecordCollectionBase, IList<Record>
    {
        #region Private Members

        #endregion //Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal RecordCollection(GroupByRecord owner, RecordManager parentRecordManager, FieldLayout fieldlayout)
            : base(owner, parentRecordManager, fieldlayout, false)
        {
        }

        #endregion //Constructors

        #region Base class overrides

            #region CreateSparseArray

        // JJD 2/25/09 - Optimization - no longer needed
        //internal override RecordSparseArrayBase CreateSparseArray()
        //{
        //    return new MainRecordSparseArray(this);
        //}

            #endregion //CreateSparseArray	

			#region RecordsType

		
		
		/// <summary>
		/// Returns the type of records contained in this record collection.
		/// </summary>
		internal override RecordType RecordsType
		{
			get
			{
				
				SparseArray sparseArray = this.SparseArray;
				if ( sparseArray.Count > 0 )
				{
					Record record = sparseArray[0] as Record;
					if ( null != record )
						return record.RecordType;
				}

				//Debug.Assert( false );
				return RecordType.DataRecord;
			}
		}

			#endregion // RecordsType

            #region SortGroupByRecords

        /// <summary>
        /// Sorts the groupby records in the collection
        /// </summary>
        protected override void SortGroupByRecords(IComparer comparer, bool reverse)
        {
            this.SparseArray.Sort((IComparer)comparer);
            if (reverse)
                this.SparseArray.Reverse();
        }

            #endregion //SortGroupByRecords

			#region VerifySortOrder

        // JJD 3/19/09 - TFS15701
        // Made non-virtual and moved logic from overrides up to the base 
        
#region Infragistics Source Cleanup (Region)













































#endregion // Infragistics Source Cleanup (Region)


			#endregion //VerifySortOrder	

        #endregion //Base class overrides	
            
        #region Properties

            #region Public Properties

            #endregion //Public Properties

            #region Internal Properties
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region Sort

        /// <summary>
        /// Sorts the collection
        /// </summary>
        public void Sort()
        {
            if (this.SparseArray.Count < 2)
                return;

            // JJD 6/23/09
            // Only pass false into the comparer if we aer sorted
            //this.SparseArray.Sort(new RecordManager.RecordsSortComparer(false));
            RecordManager rm = this.ParentRecordManager;
            this.SparseArray.Sort(new RecordManager.RecordsSortComparer(rm == null || !rm.IsSorted));

            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(NotifyCollectionChangedAction.Reset, null, -1);
        }

                #endregion //Sort

            #endregion //Public Methods	
        
        #endregion //Methods

        #region IList<Record> Members

        /// <summary>
        /// Returns the zero-based index of the specified item
        /// </summary>
        public new int IndexOf(Record item)
        {
            return this.SparseArray.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection (not supported)
        /// </summary>
        void IList<Record>.Insert(int index, Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        void IList<Record>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns the item at the specified index (read-only);
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new Record this[int index]
        {
            get
            {
                return this.SparseArray.GetItem(index, true) as Record;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection<Record> Members

        /// <summary>
        /// Adds an item to the collection (not supported)
        /// </summary>
        void ICollection<Record>.Add(Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        /// <summary>
        /// Clears the collection (not supported)
        /// </summary>
        void ICollection<Record>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns true if the collection contains the specified item.
        /// </summary>
        public new bool Contains(Record item)
        {
            return this.SparseArray.Contains(item);
        }

        /// <summary>
        /// Copies the items into an array
        /// </summary>
        public new void CopyTo(Record[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        bool ICollection<Record>.Remove(Record item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region IEnumerable<Record> Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        public IEnumerator<Record> GetEnumerator()
        {
            return new RecordEnumerator(this.SparseArray);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SparseArray.GetEnumerator( this.SparseArray );
        }

        #endregion
}

    #endregion //RecordCollection class

    #region DataRecordCollection class

    /// <summary>
    /// A collection of Records used by the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> control
    /// </summary>
	/// <remarks>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="DataRecord"/>
 	/// <seealso cref="ExpandableFieldRecord"/>
    public class DataRecordCollection : RecordCollectionBase, IList<DataRecord>
    {
        #region Private Members

        

        #endregion //Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal DataRecordCollection(Record owner, RecordManager parentRecordManager, FieldLayout fieldLayout, bool unsortedCache)
            : base(owner, parentRecordManager, fieldLayout, unsortedCache)
        {
            
        }

        #endregion //Constructors

        #region Base class overrides

            #region AllocateNewRecord

        internal override Record AllocateNewRecord(int index)
        {
            IList dataList = this.ParentRecordManager.List;

            Debug.Assert(dataList != null);

            if ( dataList != null )
            {
				// JJD 9/2/10 - TFS37394
				// Verify that the index is valid but still create the record
				// with a null listobject if it isn't
				//object listObject = dataList[index];
                object listObject = index >= 0 && index < dataList.Count ? dataList[index] : null;

                // JJD 10/02/08 added index param to support printing so we can default to
                // the associated record's FieldLayout  
                //return DataRecord.Create(this, listObject, false);
                return DataRecord.Create(this, listObject, false, index);
            }

            return null;
        }

            #endregion //AllocateNewRecord	
    
            #region CreateSparseArray

        // JJD 2/25/09 - Optimization - no longer needed
        //internal override RecordSparseArrayBase CreateSparseArray()
        //{
        //    if (this._isRawUnsorted)
        //        return new UnsortedRecordSparseArray(this);
        //    else
        //        return new MainRecordSparseArray(this);
        //}

            #endregion //CreateSparseArray	

			#region RecordsType

		
		
		/// <summary>
		/// Returns the type of records contained in this record collection.
		/// </summary>
		internal override RecordType RecordsType
		{
			get
			{
				return RecordType.DataRecord;
			}
		}

			#endregion // RecordsType

			#region VerifySortOrder

        // JJD 3/19/09 - TFS15701
        // Made non-virtual and moved logic from overrides up to the base 
        
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)


			#endregion //VerifySortOrder	

        #endregion //Base class overrides	

        #region Properties

            #region Public Properties

            #endregion //Public Properties

            #region Internal Properties
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region GetDataRecordFromDataItem

        /// <summary>
        /// Returns the DataRecord that represents the list object from the data source
        /// </summary>
        /// <param name="dataItem">The object from the data source</param>
		/// <param name="recursive">If true will check all descendant records as well.</param>
        /// <returns>The associated DataRecord or null</returns>
		/// <remarks>Infragistics.Windows.DataPresenter this may be an expensive operation depending on the size and structure of the data. The logic first checks all of the DataRecords in this collection and if a match is not found then checks all of the descendant records.</remarks>
        public DataRecord GetDataRecordFromDataItem(object dataItem, bool recursive)
        {
			if (dataItem == null)
				throw new ArgumentNullException("dataItem");

            // JJD 1/28/08
            // Get the object for record comparison
            dataItem = DataRecord.GetObjectForRecordComparision(dataItem);

            // JJD 2/09/10 - TFS26821
            // Loop over the root level data source first.
            // This will allow us to get to the desired item without having to 
            // instantiate all of the records up to the record we want.
            // But we only want to do this preliminary pass if we aren't preloading or sorting
            // records. Otherwise, we can do the normal processing below so we
            // don't incur the additional overhead of calling GetObjectForComparison 
            // on each item in the list (this information is cached on the DataRecord.
            DataPresenterBase dp = this.DataPresenter;
            RecordManager rm = this.ParentRecordManager;
            IList list = rm != null ? rm.List : null;
            if (dp != null &&
                list != null &&
                rm.IsSorted == false &&
                dp.RecordLoadMode != RecordLoadMode.PreloadRecords)
            {
                int count = list.Count;

                for (int i = 0; i < count; i++)
                {
                    object item = DataBindingUtilities.GetObjectForComparison(list[i]);

                    if (dataItem == item)
                        return rm.Unsorted[i];
                }
            }
            else
            {
                // loop over the collection looking for a direct
                // match with the dataItem
                foreach (DataRecord rcd in this)
                {
                    // JJD 1/28/08
                    // compare the DataItemForComparison instead
                    //if (rcd.DataItem == dataItem)
                    if (rcd.DataItemForComparison == dataItem)
                        return rcd;
                }
            }

			if (recursive == false)
				return null;
			
			// loop over the records again to check for any descendant records
			foreach (DataRecord rcd in this)
            {
				if (rcd.HasChildren)
				{
					DataRecord descendant = rcd.ChildRecords.GetDataRecordFromDataItem(dataItem, recursive);

					if (descendant != null)
						return descendant;
				}
            }

            return null;
        }

                #endregion //GetDataRecordFromDataItem

            #endregion //Public Methods	
        
        #endregion //Methods

        #region IList<DataRecord> Members

        /// <summary>
        /// Returns the zero-based index of the specified item
        /// </summary>
        public int IndexOf(DataRecord item)
        {
            return this.SparseArray.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection (not supported)
        /// </summary>
        void IList<DataRecord>.Insert(int index, DataRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        void IList<DataRecord>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns the item at the specified index (read-only);
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new DataRecord this[int index]
        {
            get
            {
                return this.SparseArray.GetItem(index, true) as DataRecord;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection<DataRecord> Members

        /// <summary>
        /// Adds an item to the collection (not supported)
        /// </summary>
        void ICollection<DataRecord>.Add(DataRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        /// <summary>
        /// Clears the collection (not supported)
        /// </summary>
        void ICollection<DataRecord>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns true if the collection contains the specified item.
        /// </summary>
        public bool Contains(DataRecord item)
        {
            return this.SparseArray.Contains(item);
        }

        /// <summary>
        /// Copies the items into an array
        /// </summary>
        public void CopyTo(DataRecord[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        bool ICollection<DataRecord>.Remove(DataRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region IEnumerable<DataRecord> Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        public IEnumerator<DataRecord> GetEnumerator()
        {
            return new DataRecordEnumerator(this.SparseArray);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SparseArray.GetEnumerator(this.SparseArray);
        }

        #endregion
    }

    #endregion //RecordCollection class

    #region GroupByRecordCollection class

    /// <summary>
    /// A collection of <see cref="GroupByRecord"/>s used by the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/> control
    /// </summary>
	/// <remarks>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="GroupByRecord"/>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.Records"/>
    /// <seealso cref="RecordManager"/>
    /// <seealso cref="RecordManager.Groups"/>
    /// <seealso cref="FieldLayout.SortedFields"/>
    /// <seealso cref="FieldSettings.GroupByComparer"/>
    /// <seealso cref="FieldSettings.GroupByEvaluator"/>
    /// <seealso cref="FieldSettings.GroupByMode"/>
    /// <seealso cref="FieldSettings.GroupByRecordPresenterStyle"/>
    /// <seealso cref="Field.IsGroupBy"/>
    public class GroupByRecordCollection : RecordCollectionBase, IList<GroupByRecord>
    {
        #region Private Members

        #endregion //Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal GroupByRecordCollection(Record owner, RecordManager parentRecordManager, FieldLayout fieldLayout)
            : base(owner, parentRecordManager, fieldLayout, false)
        {
        }

        #endregion //Constructors

        #region Base class overrides

            #region CreateSparseArray

        // JJD 2/25/09 - Optimization - no longer needed
        //internal override RecordSparseArrayBase CreateSparseArray()
        //{
        //    return new MainRecordSparseArray(this);
        //}

            #endregion //CreateSparseArray	

			#region RecordsType

		
		
		/// <summary>
		/// Returns the type of records contained in this record collection.
		/// </summary>
		internal override RecordType RecordsType
		{
			get
			{
				return RecordType.GroupByField;
			}
		}

			#endregion // RecordsType

            #region SortGroupByRecords

        /// <summary>
        /// Sorts the groupby records in the collection
        /// </summary>
        protected override void SortGroupByRecords(IComparer comparer, bool reverse)
        {
			this.InitializeSortVersion();
			
			if (this.Count < 2)
				return;

            this.SparseArray.Sort(comparer);
            
			if (reverse)
                this.SparseArray.Reverse();
        }

            #endregion //SortGroupByRecords

			#region VerifySortOrder

        // JJD 3/19/09 - TFS15701
        // Made non-virtual and moved logic from overrides up to the base 
        
#region Infragistics Source Cleanup (Region)
















































#endregion // Infragistics Source Cleanup (Region)


			#endregion //VerifySortOrder	

        #endregion //Base class overrides	

        #region Properties

            #region Public Properties

            #endregion //Public Properties

            #region Internal Properties
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

            #endregion //Public Methods	

            #region Internal Methods

            #endregion //Internal Methods

            #region Private Methods

            #endregion //Private Methods

        #endregion //Methods

        #region IList<GroupByRecord> Members

        /// <summary>
        /// Returns the zero-based index of the specified item
        /// </summary>
        public int IndexOf(GroupByRecord item)
        {
            return base.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection (not supported)
        /// </summary>
        void IList<GroupByRecord>.Insert(int index, GroupByRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        void IList<GroupByRecord>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns the item at the specified index (read-only);
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new GroupByRecord this[int index]
        {
            get
            {
                return base[index] as GroupByRecord;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection<GroupByRecord> Members

        /// <summary>
        /// Adds an item to the collection (not supported)
        /// </summary>
        void ICollection<GroupByRecord>.Add(GroupByRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        /// <summary>
        /// Clears the collection (not supported)
        /// </summary>
        void ICollection<GroupByRecord>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns true if the collection contains the specified item.
        /// </summary>
        public bool Contains(GroupByRecord item)
        {
            return base.Contains(item);
        }

        /// <summary>
        /// Copies the items into an array
        /// </summary>
        public void CopyTo(GroupByRecord[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        bool ICollection<GroupByRecord>.Remove(GroupByRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region IEnumerable<GroupByRecord> Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        public IEnumerator<GroupByRecord> GetEnumerator()
        {
            return new GroupByRecordEnumerator(this.SparseArray);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SparseArray.GetEnumerator(this.SparseArray);
        }

        #endregion
}

    #endregion //RecordCollection class

    #region ExpandableFieldRecordCollection class

    /// <summary>
    /// A collection of Records used by the DataPresenterBase control
    /// </summary>
	/// <remarks>
	/// <para class="body">Refer to the <a href="xamData_Terms_Records.html">Records</a> topic in the Developer's Guide for a explanation of the various record types.</para>
	/// <para class="body">Refer to the <a href="xamData_Terms_Record_Manager.html">Record Manager</a> topic in the Developer's Guide.</para>
	/// <para class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an overall explanation of how everything works together.</para>
	/// </remarks>
	/// <seealso cref="Record"/>
	/// <seealso cref="ExpandableFieldRecord"/>
	public class ExpandableFieldRecordCollection : RecordCollectionBase, IList<ExpandableFieldRecord>
    {
        #region Private Members
        
        // JJD 6/1/09 - TFS18108 - added
        private bool _inVerifyChildren;

		// SSP 8/10/09 - NAS9.2 Enhanced grid-view
		// 
		private int _verifiedFieldsVersion = -1;

        #endregion //Private Members

        #region Constructors

        /// <summary>
        /// Constructor
        /// </summary>
        internal ExpandableFieldRecordCollection(DataRecord owner, RecordManager parentRecordManager, FieldLayout fieldLayout)
            : base( owner, parentRecordManager, fieldLayout, false)
        {
        }

        #endregion //Constructors

        #region Base class overrides

            #region CreateSparseArray

        // JJD 2/25/09 - Optimization - no longer needed
        //internal override RecordSparseArrayBase CreateSparseArray()
        //{
        //    return new MainRecordSparseArray(this, true, 2);
        //}

            #endregion //CreateSparseArray	

			#region RecordsType

		
		
		/// <summary>
		/// Returns the type of records contained in this record collection.
		/// </summary>
		internal override RecordType RecordsType
		{
			get
			{
				return RecordType.ExpandableFieldRecord;
			}
		}

			#endregion // RecordsType

        #endregion //Base class overrides	

        #region Properties

            #region Public Properties

            #endregion //Public Properties

            #region Internal Properties
    
            #endregion //Internal Properties

        #endregion //Properties

        #region Indexer (field)

        /// <summary>
        /// Returns the RecordManager for this field 
        /// </summary>
        public ExpandableFieldRecord this[Field field]
        {
            get
            {
                int index = this.IndexOf(field);

                if (index >= 0)
                    return this[index];

				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_23" ) );
            }
        }

        #endregion Indexer (field name)

        #region Indexer (string field name)

        /// <summary>
        /// Returns the RecordManager by field name
        /// </summary>
        public ExpandableFieldRecord this[string fieldName]
        {
            get
            {
                int index = this.IndexOf(fieldName);

                if (index >= 0)
                    return this[index];

				// JJD 6/15/07
				// If the name is a string see if it can be parsed as an int.
				if (fieldName != null && fieldName.Trim().Length > 0 && 
					int.TryParse(fieldName, out index) && index >= 0)
					return this[index];

				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_24" ) );
            }
        }

        #endregion Indexer (field name)

        #region Methods

            #region Public Methods

                #region Exists

        /// <summary>
        /// Returns true if the collection contains a record for the specified field.
        /// </summary>
        public bool Exists(Field field)
        {
            return this.IndexOf(field) >= 0;
        }

        /// <summary>
        /// Returns true if the collection contains a record for the specified field.
        /// </summary>
        public bool Exists(string fieldName)
        {
            return this.IndexOf(fieldName) >= 0;
        }

                #endregion //Exists	

				#region GetDataRecordFromDataItem

		/// <summary>
		/// Returns the DataRecord that represents the list object from the data source
		/// </summary>
		/// <param name="dataItem">The object from the data source</param>
		/// <param name="recursive">If true will check all descendant records as well.</param>
		/// <returns>The associated DataRecord or null</returns>
		/// <remarks>Infragistics.Windows.DataPresenter this may be an expensive operation depending on the size and structure of the data. The logic first checks all of the DataRecords in this collection and if a match is not found then checks all of the descendant records.</remarks>
		public DataRecord GetDataRecordFromDataItem(object dataItem, bool recursive)
		{
			if (dataItem == null)
				throw new ArgumentNullException("dataItem");

			// loop over the collection looking for a direct
			// match with the dataItem
			foreach (ExpandableFieldRecord rcd in this)
			{
				if (rcd.HasChildren)
				{
					// JJD 09/22/11  - TFS84708 - Optimization
					// Use the ChildRecordManagerIfNeeded instead which won't create
					// child rcd managers for leaf records
					//RecordManager rm = rcd.ChildRecordManager;
					RecordManager rm = rcd.ChildRecordManagerIfNeeded;

					if (rm != null)
					{
						DataRecord descendant = rm.Sorted.GetDataRecordFromDataItem(dataItem, recursive);

						if (descendant != null)
							return descendant;
					}
				}
			}

			return null;
		}

				#endregion //GetDataRecordFromDataItem
    
                #region IndexOf

        /// <summary>
        /// Gets the index of the ExpandableFieldRecord with the specified field name.
        /// </summary>
        /// <param name="name">The name of the field to search for</param>
        /// <returns>The zero-based index or -1 if field not found</returns>
        public int IndexOf(string name)
        {
            int count = this.Count;

            Field fld;
            for (int i = 0; i < count; i++)
            {
                fld = this[i].Field;

                if (fld != null &&
                     fld.Name == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Gets the index of the ExpandableFieldRecord with the specified parent field.
        /// </summary>
        /// <param name="field">The field to search for</param>
        /// <returns>The zero-based index or -1 if field not found</returns>
        public int IndexOf(Field field)
        {
            int count = this.Count;

            for (int i = 0; i < count; i++)
            {
                if (field == this[i].Field)
                    return i;
            }

            return -1;
        }

                #endregion //IndexOf

            #endregion //Public Methods

            #region Internal Methods

                // JJD 6/1/09 - TFS18108 - added
                #region VerifyChildren

        internal void VerifyChildren()
        {
			// SSP 8/10/09 - NAS9.2 Enhanced grid-view
			// Added _verifiedFieldsVersion so check for that. Also moved code that 
			// gets the field layout and fields here from below.
			// 
			
			FieldLayout fl = this.FieldLayout;

			if ( fl == null )
				return;

			FieldCollection fields = fl.Fields;

			if ( fields.Version == _verifiedFieldsVersion )
				return;

			_verifiedFieldsVersion = fields.Version;
			

            // check anti-recursion flag
            if (this._inVerifyChildren == true)
                return;

            // set anti-recursion flag to true
            this._inVerifyChildren = true;

            try
            {
                List<Field> expandableFields = new List<Field>();

                int expandableFieldCount = fields.ExpandableFieldsCount;

                // get all the expandable fields in their relative order
                if (expandableFieldCount > 0)
                {
                    foreach (Field fld in fields)
                    {
                        if (fld.IsExpandableResolved)
                        {
                            expandableFields.Add(fld);

                            if (expandableFieldCount == expandableFields.Count)
                                break;
                        }
                    }
                }

                int newCount = expandableFields.Count;

                if (newCount == this.Count)
                {
                    bool areRcdsInCorrectOrder = true;

                    // loop over the existing expandablefieldrcds and
                    // see if their Fields are in exactly the same
                    // order as the new field list
                    for (int i = 0; i < newCount; i++)
                    {
                        if (expandableFields[i] != this[i].Field)
                        {
                            areRcdsInCorrectOrder = false;
                            break;
                        }
                    }

                    // if they are the same them get out
                    if (areRcdsInCorrectOrder == true)
                        return;
                }

                Dictionary<Field, ExpandableFieldRecord> oldRcds = new Dictionary<Field, ExpandableFieldRecord>();

                // create a dictionary so we can reuse existing records
                foreach (ExpandableFieldRecord efr in this)
                    oldRcds.Add(efr.Field, efr);

				// SSP 3/9/10 TFS25465
				// While populating the sparse array, we don't want to handle its OnScrollCountChange
				// notifications until the collection is fully populated. Enclosed the existing code
				// in BeginUpdate and EndUpdate calls.
				// 
				this.BeginUpdate( );
				try
				{
					this.SparseArray.Clear( );

					for ( int i = 0; i < newCount; i++ )
					{
						Field field = expandableFields[i];

						if ( oldRcds.ContainsKey( field ) )
						{
							// reuse the existing rcds
							this.SparseArray.Add( oldRcds[field] );
						}
						else
						{
							// create a new record
							ExpandableFieldRecord record = new ExpandableFieldRecord( this, field );
							this.SparseArray.Add( record );

							// initialize it now that it has been added to the sparse array
							record.Initialize( );
						}
					}
				}
				finally
				{
					this.EndUpdate( false );
				}

                this.RaiseChangeEvents(true);
            }
            finally
            {
                // reset anti-recursion flag to false
                this._inVerifyChildren = false;
            }
        }

                #endregion //VerifyChildren	
    
				#region GetItemIfExists

		// SSP 8/28/09 TFS21591
		// 
		/// <summary>
		/// Returns the ExpandableFieldRecord associated with the specified field if it exists
		/// in this collection. Otherwise it returns null.
		/// </summary>
		/// <param name="field">ExpandableFieldRecord for this field will be returned.</param>
		/// <returns>ExpandableFieldRecord instance.</returns>
		internal ExpandableFieldRecord GetItemIfExists( Field field )
		{
			for ( int i = 0, count = this.Count; i < count; i++ )
			{
				ExpandableFieldRecord ii = this[i];
				if ( field == ii.Field )
					return ii;
			}

			return null;
		}

				#endregion // GetItemIfExists

            #endregion //Internal Methods	
    
        #endregion //Methods

        #region IList<ExpandableFieldRecord> Members

        /// <summary>
        /// Returns the zero-based index of the specified item
        /// </summary>
        public int IndexOf(ExpandableFieldRecord item)
        {
            return base.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item into the collection (not supported)
        /// </summary>
        void IList<ExpandableFieldRecord>.Insert(int index, ExpandableFieldRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        void IList<ExpandableFieldRecord>.RemoveAt(int index)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns the item at the specified index (read-only);
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new ExpandableFieldRecord this[int index]
        {
            get
            {
                return base[index] as ExpandableFieldRecord;
            }
            set
            {
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
            }
        }

        #endregion

        #region ICollection<ExpandableFieldRecord> Members

        /// <summary>
        /// Adds an item to the collection (not supported)
        /// </summary>
        void ICollection<ExpandableFieldRecord>.Add(ExpandableFieldRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }


        /// <summary>
        /// Clears the collection (not supported)
        /// </summary>
        void ICollection<ExpandableFieldRecord>.Clear()
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        /// <summary>
        /// Returns true if the collection contains the specified item.
        /// </summary>
        public bool Contains(ExpandableFieldRecord item)
        {
            return base.Contains(item);
        }

        /// <summary>
        /// Copies the items into an array
        /// </summary>
        public void CopyTo(ExpandableFieldRecord[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns true
        /// </summary>
        public bool IsReadOnly
        {
            get { return true; }
        }

        /// <summary>
        /// Remove an item from the collection (not supported)
        /// </summary>
        bool ICollection<ExpandableFieldRecord>.Remove(ExpandableFieldRecord item)
        {
			throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_1" ) );
        }

        #endregion

        #region IEnumerable<ExpandableFieldRecord> Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        public IEnumerator<ExpandableFieldRecord> GetEnumerator()
        {
            return new ExpandableFieldRecordEnumerator(this.SparseArray);
        }

        #endregion

        #region IEnumerable Members

        /// <summary>
        /// Returms an object to enumerate the items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.SparseArray.GetEnumerator(this.SparseArray);
        }

        #endregion
    }

    #endregion //RecordCollection class
	
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