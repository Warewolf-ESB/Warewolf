#region using ...

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
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Data;
using System.Xml;
using System.Windows.Markup;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Selection;

#endregion //using ...	

namespace Infragistics.Windows.DataPresenter
{
    #region SelectedItemCollectionBase abstract base class

    /// <summary>
    /// Abstract base class for the <see cref="SelectedRecordCollection"/>, <see cref="SelectedFieldCollection"/> and <see cref="SelectedCellCollection"/>s.
    /// </summary>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItems"/>
    /// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemHolder"/>
    public abstract class SelectedItemCollectionBase : PropertyChangeNotifier, INotifyCollectionChanged, ICollection
    {
        #region Private Members

        private DataPresenterBase.SelectedItemHolder _owner;
		private NotifyCollectionChangedEventHandler _collectionChangedHandler;

		
		
		
		private SparseList _list;

		private DataPresenterBase _dataPresenter;

		// AS 5/19/09 TFS17455
		private int _beginUpdateCount;
		private bool _isDirty;

		#endregion //Private Members

        #region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SelectedItemCollectionBase"/> class
		/// </summary>
		/// <param name="dataPresenter">The owning <see cref="DataPresenterBase"/></param>
		protected SelectedItemCollectionBase(DataPresenterBase dataPresenter)
        {
			if (dataPresenter == null)
				throw new ArgumentNullException("dataPresenter");

			this._dataPresenter = dataPresenter;

			
			
			_list = new SparseList( );
        }

        #endregion //Constructor

        #region Properties

			#region Public Properties

				#region Count

		/// <summary>
		/// Returns the number of items in the collection (read-only).
		/// </summary>
		public int Count
		{
			get { return this.List.Count; }
		}

				#endregion //Count	
    
				#region IsReadOnly

		/// <summary>
		/// Indicates whether the collection is read-only
		/// </summary>
		public abstract bool IsReadOnly { get; }

				#endregion //IsReadOnly

			#endregion //Public Properties	
    
            #region Internal Properties

                #region DataPresenterBase

        internal DataPresenterBase DataPresenter
        {
            get
            {
                return this._dataPresenter;
            }
        }

                #endregion //DataPresenterBase

				#region List

		
		
		
#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Returns the internal list used to hold the items.
		/// </summary>
		protected IList List
		{
			get
			{
				return _list;
			}
		}

				#endregion //List	
    
                #region Owner

        internal DataPresenterBase.SelectedItemHolder Owner { get { return this._owner; } }

                #endregion //Owner

            #endregion //Internal Properties

        #endregion //Properties

        #region Methods

			#region Public Methods

				#region Contains

		/// <summary>
		/// Returns true if the item is in the collection 
		/// </summary>
		/// <param name="item">The item to check</param>
		public bool Contains(object item)
		{
			return this.List.Contains(item);
		}

				#endregion //Contains

				#region IndexOf

		/// <summary>
		/// Returns the zero-based index of the item or -1 if not in collection
		/// </summary>
		/// <param name="item">The item to check</param>
		public int IndexOf(object item)
		{
			return this.List.IndexOf(item);
		}

				#endregion //IndexOf

			#endregion //Public Methods	
    
            #region Internal Methods

				// AS 5/19/09 TFS17455
				#region BeginUpdate
		internal void BeginUpdate()
		{
			this._beginUpdateCount++;
		} 
				#endregion //BeginUpdate

				// AS 5/19/09 TFS17455
				#region EndUpdate
		internal void EndUpdate()
		{
			this._beginUpdateCount--;

			if (_beginUpdateCount < 0)
			{
				if (_isDirty)
				{
					_isDirty = true;
					this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				}
			}
		} 
				#endregion //EndUpdate

				#region GetItem
		internal ISelectableItem GetItem(int index)
		{
			return this._list[index] as ISelectableItem;
		} 
				#endregion //GetItem

                #region Initialize

        internal void Initialize(DataPresenterBase.SelectedItemHolder owner)
        {
            this._owner = owner;
        }

                #endregion //Initialize

				// AS 5/19/09 TFS17455
				#region RaiseNotifications
		internal void RaiseNotifications(NotifyCollectionChangedEventArgs args)
		{
			if (_beginUpdateCount > 0)
			{
				_isDirty = true;
				return;
			}

			this.RaisePropertyChangedEvent("Count");
			this.RaisePropertyChangedEvent("Item[]");
			this.RaiseCollectionChangedEvent(args);
		}
				#endregion //RaiseNotifications

				// JJD 1/16/12 - TFS63720 - added
				#region ResetItemSelectionStates abstract

		internal abstract void ResetItemSelectionStates();

				#endregion //ResetItemSelectionStates abstract	
    
            #endregion //Internal Methods

			#region Protected Methods

				#region InternalAdd

		/// <summary>
		/// Appends a single item to the collection
		/// </summary>
		/// <param name="item">The item to add</param>
		/// <returns>The zero-based index of the item that was added.</returns>
		protected int InternalAdd(object item)
		{
			int index = this.List.Add(item);

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaisePropertyChangedEvent("Item[]");
			//this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			return index;
		}

				#endregion //InternalAdd	
    
				#region InternalAddRange

		/// <summary>
		/// Adds multiple items
		/// </summary>
		protected internal void InternalAddRange(ICollection collection)
		{
			InternalAddRange(collection, false);
		}

		// AS 5/19/09 TFS17455
		internal void InternalAddRange(ICollection collection, bool clear)
		{
			int oldCount = this.Count;

			// AS 5/19/09 TFS17455
			if (clear)
				_list.Clear();

			
			
			
			_list.AddRange( collection );

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaisePropertyChangedEvent("Item[]");
			//// AS 7/2/07
			//// The original issue I noticed is that we were using the overload of the 
			//// NotifyCollectionChangedEventArgs that took an object because the overload
			//// that signifies multiple adds/removes would take an ilist. Then after passing
			//// an IList I found that when you are using binding, an exception will be thrown
			//// if the add/remove action has multiple items so I changed it to a Reset.
			////
			////this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, collection, oldCount));
			//this.RaiseCollectionChangedEvent(NotifyCollectionChangedAction.Reset);
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

				#endregion //InternalAddRange	

				#region InternalClear

		/// <summary>
		/// Removes all items from the collection
		/// </summary>
		protected internal void InternalClear()
		{
			this.List.Clear();

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaisePropertyChangedEvent("Item[]");
			//this.RaiseCollectionChangedEvent(NotifyCollectionChangedAction.Reset);
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		// JJD 1/16/12 - TFS63720
		// Added overlaod with resetItemSelectionStates parameter
		internal void InternalClear(bool resetItemSelectionStates)
		{
			if ( resetItemSelectionStates )
				this.ResetItemSelectionStates();

			this.InternalClear();
		}

				#endregion //InternalAdd	

				#region InternalInsert

		/// <summary>
		/// Inserts an item into the collection
		/// </summary>
		/// <param name="index">The zero-based index where to isert the item.</param>
		/// <param name="item">The item to insert</param>
		protected void InternalInsert(int index, object item)
		{
			this.List.Insert(index, item);

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaisePropertyChangedEvent("Item[]");
			//this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

				#endregion //InternalInsert	

				#region InternalRemove

		/// <summary>
		/// Removes an item from the collection
		/// </summary>
		/// <param name="item">The item to remove</param>
		protected void InternalRemove(object item)
		{
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			int index = this.IndexOf(item);

			if (0 <= index)
				this.InternalRemoveAt(index);
		}

				#endregion //InternalRemove	

				#region InternalRemoveAt

		/// <summary>
		/// Removes an item from the collection
		/// </summary>
		/// <param name="index">The index of the item to remove</param>
		protected void InternalRemoveAt(int index)
		{
			object item = this.List[index];
			this.List.RemoveAt(index);

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaisePropertyChangedEvent("Item[]");
			//this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
		}

				#endregion //InternalRemoveAt
    
				#region RaiseCollectionChangedEvent

		/// <summary>
		/// Raises the CollectionChanged event
		/// </summary>
		/// <param name="action">The action that triggered the event</param>
		protected void RaiseCollectionChangedEvent(NotifyCollectionChangedAction action)
		{
			if (this._collectionChangedHandler != null)
				this._collectionChangedHandler(this, new NotifyCollectionChangedEventArgs(action));
		}

		/// <summary>
		/// Raises the CollectionChanged event
		/// </summary>
		/// <param name="e">The event args</param>
		protected void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e )
		{
			if (this._collectionChangedHandler != null)
				this._collectionChangedHandler(this, e);
		}

				#endregion //RaiseCollectionChangedEvent

			#endregion //Protected Methods	

        #endregion //Methods

		#region INotifyCollectionChanged Members

		/// <summary>
		/// Raised when items are added or removed from the collection
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

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			this.List.CopyTo(array, index);
		}

		bool ICollection.IsSynchronized
		{
			get { return this.List.IsSynchronized; }
		}

		object ICollection.SyncRoot
		{
			get { return this.List.SyncRoot; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.List.GetEnumerator();
		}

		#endregion
	}

    #endregion //SelectedItemCollectionBase abstract base class	
    
    #region SelectedRecordCollection class

    /// <summary>
    /// Collection of selected <see cref="Record"/>s
    /// </summary>
	/// <seealso cref="Record"/>
	/// <seealso cref="Record.IsSelected"/>
    /// <seealso cref="DataPresenterBase"/>
    /// <seealso cref="DataPresenterBase.SelectedItems"/>
    /// <seealso cref="DataPresenterBase.SelectedItemsChanging"/>
    /// <seealso cref="DataPresenterBase.SelectedItemsChanged"/>
	/// <seealso cref="FieldLayoutSettings"/>
    /// <seealso cref="FieldLayoutSettings.SelectionTypeRecord"/>
    /// <seealso cref="DataPresenterBase.SelectedItemHolder"/>
	/// <seealso cref="DataPresenterBase.SelectedItemHolder.Records"/>
    public sealed class SelectedRecordCollection : SelectedItemCollectionBase
    {
        #region Private Members

        #endregion //Private Members

        #region Constructor

		internal SelectedRecordCollection(DataPresenterBase dataPresenter) : base(dataPresenter)
        {
        }

        #endregion //Constructor

        #region Base class overrides

			// JJD 1/16/12 - TFS63720 - added
			#region ResetItemSelectionStates
    
   		internal override void ResetItemSelectionStates()
		{
			// walk over each item and reset its selection state to false
			foreach (Record rcd in this)
				rcd.InternalSelect(false);
		}

   			#endregion //ResetItemSelectionStates	
    
			#region IsReadOnly

        /// <summary>
        /// Indicates whether the collection is read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                // We want to return true only for the main selected
                // object and not for the onces that get passed in 
                // various selectchange events.
                return null == this.DataPresenter || this.Owner != this.DataPresenter.SelectedItems || this != this.Owner.Records;
            }
        }

            #endregion //IsReadOnly
    
        #endregion //Base class overrides

        #region Properties

            #region Public Properties

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region Add

        /// <summary>
        /// Adds the specified record to this selected records collection.
        /// </summary>
        /// <param name="record"></param>
        public void Add(Record record)
        {
            this.AddRange(new Record[] { record });
        }

                #endregion // Add

                #region AddRange

        /// <summary>
        /// Selects records contained in passed in records array keeping the existing selected records selected.
        /// </summary>
        /// <param name="records"></param>
        /// <remarks>
        /// <p>This method can be used to select a range of records. It will keep the existing selected records and select the passed in records.</p>
        /// <p>You can use Record.IsSelected property to select or unselect individual records.</p>
        /// <p><seealso cref="Clear"/>, <seealso cref="Record.IsSelected"/></p>
        /// </remarks>		
        public void AddRange(Record[] records)
        {
            this.AddRange(records, true, true);
        }

        /// <summary>
        /// Selects records contained in passed in records array keeping the existing selected records selected.
        /// </summary>
        /// <param name="records">Records to select.</param>
        /// <param name="ensureRecordsOrdered">If true then sorts the records in their visible order. For efficiency reason you may want to specify false for this parameter, if for example the order of records in the selected records collection doesn't matter to your logic that accesses and manipulates the selected records collection or you know the records to be in the correct order.</param>
        /// <param name="fireSelectionChangeEvent">Specifies whether to fire the <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanging"/> and <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase.SelectedItemsChanged"/> events.</param>
        /// <remarks>
        /// <p>This method can be used to select a range of records. It will keep the existing selected records and select the passed in records.</p>
        /// <p>You can use Record.IsSelected property to select or unselect individual records.</p>
        /// <p><seealso cref="Clear"/>, <seealso cref="Record.IsSelected"/></p>
        /// </remarks>		
        public void AddRange(Record[] records, bool ensureRecordsOrdered, bool fireSelectionChangeEvent)
        {
            // If the passed in array is of length 0, then do nothing.
            //
            if (records.Length <= 0)
                return;

            int i;
            DataPresenterBase dataPresenter = this.DataPresenter;

            // Ensure that all the records are from the same fieldLayout. We don't allow selecting records from
            // different bands.
            //
            FieldLayout fieldLayout = records[0].FieldLayout;

            for (i = 1; i < records.Length; i++)
            {
                if (records[i].FieldLayout != fieldLayout)
					throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_30" ), "records" );
            }

            // Ensure that the records are from this dataPresenter. We don't want to add records from a different dataPresenter
            // or a different layout to the selected of this dataPresenter.
            //
            if (null == fieldLayout || fieldLayout.DataPresenter != dataPresenter)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_31" ), "records" );

			// AS 5/6/09 Selection across field layouts
			// We were inconsistently preventing selection across field layouts. If you tried through 
			// the ui we didn't unless you selected a range where the pivot and end record was in the 
			// same fieldlayout. Similarly above we were only checking with the records you provided and 
			// not against any records we may already have in the collection.
			//
			if (this.Count > 0 && fieldLayout != this[0].FieldLayout)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_30" ), "records" );

            SelectionStrategyBase strategy = ((ISelectionHost)this.DataPresenter).GetSelectionStrategyForItem(records[0]);

            if (strategy == null)
            {
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_17" ) );
            }

            if (!strategy.IsMultiSelect)
            {
                if (strategy.IsSingleSelect)
                {
                    if (records.Length + this.Count > 1)
						throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_18" ) );
                }
                else
                {
					throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_19" ) );
                }
            }

            // Clear the selected cells since cells and records can't be selected at the same time. If that gets 
            // cancelled, then return.
            //
            if (!dataPresenter.ClearSelectedCells())
                return;

            // If there are no previously selected records and the new records are in correct order
            // then skip sorting and merging.
            // 
            bool needsToMergeAndSort = ensureRecordsOrdered || 0 != this.Count;
            if (needsToMergeAndSort && 0 == this.Count)
            {
                needsToMergeAndSort = false;
                for (i = 1; !needsToMergeAndSort && i < records.Length; i++)
                    needsToMergeAndSort = 0 < DataPresenterBase.SelectedItemHolder.SelectionPositionSortComparer.CompareRecord(records[i - 1], records[i]);
            }

            object[] arr;

            // If there are no previously selected records and the new records are in correct order
            // then skip sorting and merging. Enclosed the existing code into the if block and
            // added the else block.
            // 
            if (needsToMergeAndSort)
            {
                Hashtable newSelectedRecords = new Hashtable();
                object dummy = new object();

                // Add the existing records.
                //
                for (i = 0; i < this.Count; i++)
                {
                    newSelectedRecords[this[i]] = dummy;
                }

                // Add the passed in records.
                //
                for (i = 0; i < records.Length; i++)
                {
                    newSelectedRecords[records[i]] = dummy;
                }

                arr = new object[newSelectedRecords.Count];
                newSelectedRecords.Keys.CopyTo(arr, 0);
                newSelectedRecords = null;

                // Sort the new records by the selected position.
                //
                Infragistics.Windows.Utilities.SortMerge(arr, new DataPresenterBase.SelectedItemHolder.SelectionPositionSortComparer());
            }
            else
            {
                arr = records;
            }

			int oldCount = this.Count;

            if (!fireSelectionChangeEvent)
            {
                this.InternalClear();
                this.InternalAddRange(arr);

                // Set the Selected property to true on each item.
                // 
                foreach (Record item in this)
                    item.InternalSelect(true);
            }
            else
            {
                DataPresenterBase.SelectedItemHolder newSelection = new DataPresenterBase.SelectedItemHolder(this.DataPresenter);
                newSelection.Records.InternalAddRange(arr);

                // Copy the columns since columns and records can be selected at the same time. Records and cells
                // can't be selected at the same time.
                //
                newSelection.Fields.InternalSetList(dataPresenter.SelectedItems.Fields);

                // Finally select the new selection by calling SelectNewSelection which fires the 
                // select change events.
                //
                dataPresenter.SelectNewSelection(typeof(Record), newSelection);
            }

			if (this.Count == oldCount)
				return;

			// AS 5/19/09 TFS17455
			//this.RaisePropertyChangedEvent("Count");
			//this.RaiseCollectionChangedEvent( new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, records));
			this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, records));
        }

                #endregion // AddRange

                #region Clear

        /// <summary>
        /// Clears the collection unselecting any selected records.
        /// </summary>
        /// <remarks>
        /// <p><seealso cref="AddRange(Record[])"/>, <seealso cref="Record.IsSelected"/></p>
        /// </remarks>
        public void Clear()
        {
			if (this.Count == 0)
				return;

            DataPresenterBase dataPresenter = this.DataPresenter;

            if (this.IsReadOnly)
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_13" ) );

            // IsReadOnly should ensure that following condition in Debug.Assert is met before
            // returning true.
            //
            Debug.Assert(this == dataPresenter.SelectedItems.Records, "DataPresenterBase's selected and this are not the same !");

            // Call ClearSelectedRecords to clear the selected records. It also fires the select change
            // events.
            //
            dataPresenter.ClearSelectedRecords();

			if (this.Count == 0)
			{
				// AS 5/19/09 TFS17455
				//this.RaisePropertyChangedEvent("Count");
				//this.RaiseCollectionChangedEvent(NotifyCollectionChangedAction.Reset);
				this.RaiseNotifications(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
        }

                #endregion // Clear

                #region Contains

        /// <summary>
        /// Returns true if the item is in the collection 
        /// </summary>
        /// <param name="item">The item to check</param>
		public bool Contains(Record item)
        {
            return this.List.Contains(item);
        }

                #endregion //Contains

                #region CopyTo

        /// <summary>
        /// Copies the elements of the collection into the array.
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="index">The index to begin copying to.</param>
        public void CopyTo(Record[] array, int index)
        {
            this.List.CopyTo((System.Array)array, index);
        }

                #endregion //CopyTo

                #region GetEnumerator

        /// <summary>
        /// Returns a type safe enumerator.
        /// </summary>
        public IEnumerator<Record> GetEnumerator() // non-IEnumerable version
        {
            return new RecordCollection.RecordEnumerator(((IEnumerable)this).GetEnumerator());
        }

                #endregion //GetEnumerator

                #region Indexer (int)

        /// <summary>
        /// Indexer
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
                return (Record)this.List[index];
            }
        }

                #endregion //Indexer (int)

            #endregion //Public Methods

            #region Internal Methods

                #region InternalAdd






        internal int InternalAdd(Record selectedRecord)
        {
            return this.InternalAdd(selectedRecord, true);
        }






        internal int InternalAdd(Record selectedRecord, bool validateAlreadyExists)
        {
            // Check to see if the item being added to the list
            // is already in the list is inefficient so added an overload with which you can
            // specify not to do that.
            if (validateAlreadyExists)
            {
                int index = this.List.IndexOf(selectedRecord);

                if (index >= 0)
                    return index;
            }

            return base.InternalAdd(selectedRecord);
        }

                #endregion //InternalAdd

                #region InternalInsert



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void InternalInsert(int index, Record selectedRecord)
        {
            if (this.List.Contains(selectedRecord))
            {
                Debug.Assert(false, "Record is already in the selected records collection. Can't added it again.");
                return;
            }

            base.InternalInsert(index, selectedRecord);
        }

                #endregion //InternalInsert

				// AS 5/19/09 TFS17455
				#region InternalRemove
		internal void InternalRemove(Record record)
		{
			base.InternalRemove((object)record);
		}
				#endregion //InternalRemove

                #region InternalSetList







        internal void InternalSetList(SelectedRecordCollection records)
        {
			// JJD 02/16/12 - TFS101563
			// If the passed in collection is this collection then return without doing anything
			if (this == records)
				return;

			// AS 5/19/09 TFS17455
            //this.InternalClear();
            //this.InternalAddRange(records);
			this.InternalAddRange(records, true);
        }

                #endregion // InternalSetList

                #region RemoveAt







        internal void RemoveAt(int index)
        {
            this.InternalRemoveAt(index);
        }

                #endregion //RemoveAt

                #region Remove






        internal void Remove(Record selectedRecord)
        {
            this.InternalRemove(selectedRecord);
        }

                #endregion //Remove

                #region ToArray

        /// <summary>
        /// The collection as an array of objects
        /// </summary>
        internal Record[] ToArray()
        {
			Record[] rcds = new Record[this.Count];

			if (rcds.Length > 0)
				this.CopyTo(rcds, 0);

            return rcds;
        }

                #endregion // ToArray

            #endregion //Internal Methods

        #endregion //Methods
	}

    #endregion //SelectedRecordCollection class	
    
    #region SelectedCellCollection class

    /// <summary>
    /// Collection of selected <see cref="Cell"/>s
    /// </summary>
	/// <seealso cref="Cell"/>
	/// <seealso cref="Cell.IsSelected"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="DataPresenterBase.SelectedItems"/>
	/// <seealso cref="DataPresenterBase.SelectedItemsChanging"/>
	/// <seealso cref="DataPresenterBase.SelectedItemsChanged"/>
	/// <seealso cref="FieldLayoutSettings"/>
	/// <seealso cref="FieldLayoutSettings.SelectionTypeCell"/>
	/// <seealso cref="DataPresenterBase.SelectedItemHolder"/>
	public sealed class SelectedCellCollection : SelectedItemCollectionBase
    {
        #region Private Members

        #endregion //Private Members

        #region Constructor

        internal SelectedCellCollection(DataPresenterBase dataPresenter) : base(dataPresenter)
        {
        }

        #endregion //Constructor

        #region Base class overrides

			// JJD 1/16/12 - TFS63720 - added
			#region ResetItemSelectionStates
    
   		internal override void ResetItemSelectionStates()
		{
			// walk over each item and reset its selection state to false
			foreach (Cell cell in this)
				cell.InternalSelect(false);
		}

   			#endregion //ResetItemSelectionStates	

            #region IsReadOnly

        /// <summary>
        /// Indicates whether the collection is read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                // We want to return true only for the main selected
                // object and not for the onces that get passed in 
                // various selectchange events.
                return null == this.DataPresenter || this.Owner != this.DataPresenter.SelectedItems || this != this.Owner.Cells;
            }
        }

            #endregion //IsReadOnly

        #endregion //Base class overrides

        #region Properties

            #region Public Properties

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region Add

        /// <summary>
        /// Adds the specified cell to this selected cells collection.
        /// </summary>
        /// <param name="cell"></param>
        public void Add(Cell cell)
        {
            this.AddRange(new Cell[] { cell });
        }

                #endregion // Add

                #region AddRange

        /// <summary>
        /// Selects cells contained in passed in cells array keeping the existing selected cells selected.
        /// </summary>
        /// <param name="cells">Cells to select.</param>
        /// <remarks>
        /// <p>This method can be used to select a range of cells. It will keep the existing selected cells and select the passed in cells.</p>
        /// <p>You can use Cell.IsSelected property to select or unselect individual cells.</p>
        /// <p><seealso cref="Clear"/>, <seealso cref="Cell.IsSelected"/></p>
        /// </remarks>		
        public void AddRange(Cell[] cells)
        {
            // If the passed in array is of length 0, then do nothing.
            //
            if (cells.Length <= 0)
                return;

            int i;
            DataPresenterBase dataPresenter = this.DataPresenter;

            // Ensure that all the cells are from the same fieldLayout. We don't allow selecting cells from
            // different bands.
            //
            FieldLayout fieldLayout = cells[0].Record.FieldLayout;

            for (i = 1; i < cells.Length; i++)
            {
                if (cells[i].Record.FieldLayout != fieldLayout)
					throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_32" ), "cells" );
            }

            // Ensure that the cells are from this dataPresenter. We don't want to add cells from a different dataPresenter
            // or a different layout to the selected of this dataPresenter.
            //
            if (null == fieldLayout || fieldLayout.DataPresenter != dataPresenter)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_33" ), "cells" );

			// AS 5/6/09 Selection across field layouts
			// We were inconsistently preventing selection across field layouts. If you tried through 
			// the ui we didn't unless you selected a range where the pivot and end record was in the 
			// same fieldlayout. Similarly above we were only checking with the records you provided and 
			// not against any records we may already have in the collection.
			//
			if (this.Count > 0 && fieldLayout != this[0].Record.FieldLayout)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_32" ), "cells" );

            SelectionStrategyBase strategy = ((ISelectionHost)this.DataPresenter).GetSelectionStrategyForItem(cells[0]);

            if (strategy == null)
            {
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_20" ) );
            }

            if (!strategy.IsMultiSelect)
            {
				if (strategy.IsSingleSelect)
				{
					if (cells.Length + this.Count > 1)
					{
						throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_21" ) );
					}
				}
				else
				{
					throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_22" ) );
				}
            }

            // Clear the selected cells since cells and cells can't be selected at the same time. If that gets 
            // cancelled, then return.
            //
            if (!dataPresenter.ClearSelectedRecords())
                return;

            object[] arr;

            Hashtable newSelectedCells = new Hashtable();
            object dummy = new object();

            // Add the existing cells.
            //
            for (i = 0; i < this.Count; i++)
            {
                newSelectedCells[this[i]] = dummy;
            }

            // Add the passed in cells.
            //
            for (i = 0; i < cells.Length; i++)
            {
                newSelectedCells[cells[i]] = dummy;
            }

            arr = new object[newSelectedCells.Count];
            newSelectedCells.Keys.CopyTo(arr, 0);
            newSelectedCells = null;

            // Sort the new cells by the selected position.
            //
            Infragistics.Windows.Utilities.SortMerge(arr, new DataPresenterBase.SelectedItemHolder.SelectionPositionSortComparer());

            DataPresenterBase.SelectedItemHolder newSelection = new DataPresenterBase.SelectedItemHolder(this.DataPresenter);
            newSelection.Cells.InternalAddRange(arr);

            // Copy the fields since fields and cells can be selected at the same time. Cells and records
            // can't be selected at the same time.
            //
            newSelection.Fields.InternalSetList(dataPresenter.SelectedItems.Fields);

            // Finally select the new selection by calling SelectNewSelection which fires the 
            // select change events.
            //
            dataPresenter.SelectNewSelection(typeof(Cell), newSelection);
        }

                #endregion // AddRange

                #region Clear

        /// <summary>
        /// Clears the collection unselecting any selected cells.
        /// </summary>
        /// <remarks>
        /// <p><seealso cref="AddRange"/>, <seealso cref="Cell.IsSelected"/></p>
        /// </remarks>
        public void Clear()
        {
            DataPresenterBase dataPresenter = this.DataPresenter;

            if (this.IsReadOnly)
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_13" ) );

            // IsReadOnly should ensure that following condition in Debug.Assert is met before
            // returning true.
            //
            Debug.Assert(this == dataPresenter.SelectedItems.Cells, "DataPresenterBase's selected and this are not the same !");

            // Call ClearSelectedCells to clear the selected cells. It also fires the select change
            // events.
            //
            dataPresenter.ClearSelectedCells();
        }

                #endregion // Clear

				#region Contains

		/// <summary>
		/// Returns true if the item is in the collection 
		/// </summary>
		/// <param name="item">The item to check</param>
		public bool Contains(Cell item)
		{
			return this.List.Contains(item);
		}

				#endregion //Contains

                #region CopyTo

        /// <summary>
        /// Copies the elements of the collection into the array.
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="index">The index to begin copying to.</param>
        public void CopyTo(Cell[] array, int index)
        {
            this.List.CopyTo((System.Array)array, index);
        }

                #endregion //CopyTo

                #region GetEnumerator

        /// <summary>
        /// Returns a type safe enumerator.
        /// </summary>
        public IEnumerator<Cell> GetEnumerator() // non-IEnumerable version
        {
			return new CellEnumerator(((IEnumerable)this).GetEnumerator());
		}

                #endregion //GetEnumerator

                #region Indexer (int)

        /// <summary>
        /// Indexer
        /// </summary>
        public Cell this[int index]
        {
            get
            {
                return (Cell)this.List[index];
            }
        }

                #endregion //Indexer (int)

            #endregion //Public Methods

            #region Internal Methods

                #region InternalAdd






        internal int InternalAdd(Cell selectedCell)
        {
            return this.InternalAdd(selectedCell, true);
        }






        internal int InternalAdd(Cell selectedCell, bool validateAlreadyExists)
        {
            // Check to see if the item being added to the list
            // is already in the list is inefficient so added an overload with which you can
            // specify not to do that.
            if (validateAlreadyExists)
            {
                int index = this.List.IndexOf(selectedCell);

                if (index >= 0)
                    return index;
            }

            return base.InternalAdd(selectedCell);
        }

                #endregion //InternalAdd

                #region InternalInsert



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void InternalInsert(int index, Cell selectedCell)
        {
            if (this.List.Contains(selectedCell))
            {
                Debug.Assert(false, "Cell is already in the selected cells collection. Can't added it again.");
                return;
            }

            base.InternalInsert(index, selectedCell);
        }

                #endregion //InternalInsert

				// AS 5/19/09 TFS17455
				#region InternalRemove
		internal void InternalRemove(Cell record)
		{
			base.InternalRemove((object)record);
		} 
				#endregion //InternalRemove

                #region InternalSetList







        internal void InternalSetList(SelectedCellCollection cells)
        {
			// JJD 02/16/12 - TFS101563
			// If the passed in collection is this collection then return without doing anything
			if (this == cells)
				return;

			// AS 5/19/09 TFS17455
            //this.InternalClear();
            //this.InternalAddRange(cells);
			this.InternalAddRange(cells, true);
        }

                #endregion // InternalSetList

                #region RemoveAt







        internal void RemoveAt(int index)
        {
			this.InternalRemoveAt(index);
        }

                #endregion //RemoveAt

                #region Remove






        internal void Remove(Cell selectedCell)
        {
            this.InternalRemove(selectedCell);
        }

                #endregion //Remove

				#region RemoveCells

		
		
		/// <summary>
		/// Removes all cells of specified data record from this collection.
		/// </summary>
		/// <param name="record">Data record whose cells to remove</param>
		internal void RemoveCells( DataRecord record )
		{
			CellCollection recordCells = record.CellsIfAllocated;
			if ( null != recordCells )
			{
				for ( int i = 0, count = recordCells.Count; i < count; i++ )
				{
					Cell cell = recordCells.GetCellIfAllocated( i );
					if ( null != cell && cell.IsSelected )
						this.Remove( cell );
				}
			}
		}

				#endregion // RemoveCells

				#region ToArray

		/// <summary>
        /// The collection as an array of objects
        /// </summary>
        internal Cell[] ToArray()
        {
			Cell[] cells = new Cell[this.Count];

			if (cells.Length > 0)
				this.CopyTo(cells, 0);

            return cells;
        }

                #endregion // ToArray

            #endregion //Internal Methods

        #endregion //Methods
		
		#region CellEnumerator internal class

		internal class CellEnumerator : IEnumerator, IEnumerator<Cell>
		{
			IEnumerator _enumerator;

			internal CellEnumerator(IEnumerator enumerator)
			{
				this._enumerator = enumerator;
			}

			#region IEnumerator<Record> Members

			public Cell Current
			{
				get { return this._enumerator.Current as Cell; }
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

			Cell IEnumerator<Cell>.Current
			{
				get { return this._enumerator.Current as Cell; }
			}
		}
		#endregion //CellEnumerator	
    }

    #endregion //SelectedCellCollection class

    #region SelectedFieldCollection class

    /// <summary>
    /// Collection of selected <see cref="Field"/>s
    /// </summary>
	/// <seealso cref="Field"/>
	/// <seealso cref="Field.IsSelected"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="DataPresenterBase.SelectedItems"/>
	/// <seealso cref="DataPresenterBase.SelectedItemsChanging"/>
	/// <seealso cref="DataPresenterBase.SelectedItemsChanged"/>
	/// <seealso cref="FieldLayoutSettings"/>
	/// <seealso cref="FieldLayoutSettings.SelectionTypeField"/>
	/// <seealso cref="FieldSettings"/>
	/// <seealso cref="FieldSettings.LabelClickAction"/>
	/// <seealso cref="DataPresenterBase.SelectedItemHolder"/>
	/// <seealso cref="DataPresenterBase.SelectedItemHolder.Fields"/>
	public sealed class SelectedFieldCollection : SelectedItemCollectionBase
    {
        #region Private Members

        #endregion //Private Members

        #region Constructor

		internal SelectedFieldCollection(DataPresenterBase dataPresenter) : base(dataPresenter)
        {
        }

        #endregion //Constructor

        #region Base class overrides

			// JJD 1/16/12 - TFS63720 - added
			#region ResetItemSelectionStates
    
   		internal override void ResetItemSelectionStates()
		{
			// walk over each item and reset its selection state to false
			foreach (Field field in this)
				field.InternalSelect(false);
		}

   			#endregion //ResetItemSelectionStates	

            #region IsReadOnly

        /// <summary>
        /// Indicates whether the collection is read-only
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                // We want to return true only for the main selected
                // object and not for the onces that get passed in 
                // various selectchange events.
                return null == this.DataPresenter || this.Owner != this.DataPresenter.SelectedItems || this != this.Owner.Fields;
            }
        }

            #endregion //IsReadOnly

        #endregion //Base class overrides

        #region Properties

            #region Public Properties

            #endregion //Public Properties

        #endregion //Properties

        #region Methods

            #region Public Methods

                #region Add

        /// <summary>
        /// Adds the specified field to this selected fields collection.
        /// </summary>
        /// <param name="field"></param>
        public void Add(Field field)
        {
            this.AddRange(new Field[] { field });
        }

                #endregion // Add

                #region AddRange

        /// <summary>
        /// Selects fields contained in passed in fields array keeping the existing selected fields selected.
        /// </summary>
        /// <param name="fields"></param>
        /// <remarks>
        /// <p>This method can be used to select a range of fields. It will keep the existing selected fields and select the passed in fields.</p>
        /// <p>You can use Field.IsSelected property to select or unselect individual fields.</p>
        /// <p><seealso cref="Clear"/>, <seealso cref="Field.IsSelected"/></p>
        /// </remarks>		
        public void AddRange(Field[] fields)
        {
            // If the passed in array is of length 0, then do nothing.
            //
            if (fields.Length <= 0)
                return;

            int i;
            DataPresenterBase dataPresenter = this.DataPresenter;

            // Ensure that all the fields are from the same fieldLayout. We don't allow selecting fields from
            // different bands.
            //
            FieldLayout fieldLayout = fields[0].Owner;

            for (i = 1; i < fields.Length; i++)
            {
                if (fields[i].Owner != fieldLayout)
					throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_34" ), "fields" );
            }

            // Ensure that the fields are from this dataPresenter. We don't want to add fields from a different dataPresenter
            // or a different layout to the selected of this dataPresenter.
            //
            if (null == fieldLayout || fieldLayout.DataPresenter != dataPresenter)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_35" ), "fields" );

			// AS 5/6/09 Selection across field layouts
			// We were inconsistently preventing selection across field layouts. If you tried through 
			// the ui we didn't unless you selected a range where the pivot and end record was in the 
			// same fieldlayout. Similarly above we were only checking with the records you provided and 
			// not against any records we may already have in the collection.
			//
			if (this.Count > 0 && fieldLayout != this[0].Owner)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_34" ), "fields" );

            SelectionStrategyBase strategy = ((ISelectionHost)this.DataPresenter).GetSelectionStrategyForItem(fields[0]);

            if (strategy == null)
            {
				throw new InvalidOperationException( DataPresenterBase.GetString( "LE_ArgumentException_36" ) );
            }

            if (!strategy.IsMultiSelect)
            {
                if (strategy.IsSingleSelect)
                {
                    if (fields.Length + this.Count > 1)
						throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_23" ) );
                }
                else
                {
					throw new InvalidOperationException( DataPresenterBase.GetString( "LE_InvalidOperationException_24" ) );
                }
            }

            object[] arr;

            Hashtable newSelectedFields = new Hashtable();
            object dummy = new object();

            // Add the existing fields.
            //
            for (i = 0; i < this.Count; i++)
            {
                newSelectedFields[this[i]] = dummy;
            }

            // Add the passed in fields.
            //
            for (i = 0; i < fields.Length; i++)
            {
                newSelectedFields[fields[i]] = dummy;
            }

            arr = new object[newSelectedFields.Count];
            newSelectedFields.Keys.CopyTo(arr, 0);
            newSelectedFields = null;

            // Sort the new fields by the selected position.
            //
            Infragistics.Windows.Utilities.SortMerge(arr, new DataPresenterBase.SelectedItemHolder.SelectionPositionSortComparer());

            DataPresenterBase.SelectedItemHolder newSelection = new DataPresenterBase.SelectedItemHolder(this.DataPresenter);
            newSelection.Fields.InternalAddRange(arr);

			// JM 12-02-08 TFS11031
			//// Copy the columns since columns and fields can be selected at the same time. Fields and fields
			//// can't be selected at the same time.
			////
			//newSelection.Fields.InternalSetList(dataPresenter.SelectedItems.Fields);
			// Copy the cells since cells and fields can be selected at the same time. Records and fields
			// can't be selected at the same time.
			//
			newSelection.Cells.InternalSetList(dataPresenter.SelectedItems.Cells);

            // Finally select the new selection by calling SelectNewSelection which fires the 
            // select change events.
            //
            dataPresenter.SelectNewSelection(typeof(Field), newSelection);
        }

                #endregion // AddRange

                #region Clear

        /// <summary>
        /// Clears the collection unselecting any selected fields.
        /// </summary>
        /// <remarks>
        /// <p><seealso cref="AddRange"/>, <seealso cref="Field.IsSelected"/></p>
        /// </remarks>
        public void Clear()
        {
            DataPresenterBase dataPresenter = this.DataPresenter;

            if (this.IsReadOnly)
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_13" ) );

            // IsReadOnly should ensure that following condition in Debug.Assert is met before
            // returning true.
            //
            Debug.Assert(this == dataPresenter.SelectedItems.Fields, "DataPresenterBase's selected and this are not the same !");

            // Call ClearSelectedFields to clear the selected fields. It also fires the select change
            // events.
            //
            dataPresenter.ClearSelectedFields();
        }

                #endregion // Clear

				#region Contains

		/// <summary>
		/// Returns true if the item is in the collection 
		/// </summary>
		/// <param name="item">The item to check</param>
		public bool Contains(Field item)
		{
			return this.List.Contains(item);
		}

				#endregion //Contains

                #region CopyTo

        /// <summary>
        /// Copies the elements of the collection into the array.
        /// </summary>
        /// <param name="array">The array to copy to</param>
        /// <param name="index">The index to begin copying to.</param>
        public void CopyTo(Field[] array, int index)
        {
            this.List.CopyTo((System.Array)array, index);
        }

                #endregion //CopyTo

                #region GetEnumerator

        /// <summary>
        /// Returns a type safe enumerator.
        /// </summary>
        public IEnumerator<Field> GetEnumerator() // non-IEnumerable version
        {
            return new FieldCollection.FieldEnumerator(((IEnumerable)this).GetEnumerator());
        }

                #endregion //GetEnumerator

                #region Indexer (int)

        /// <summary>
        /// Indexer
        /// </summary>
        public Field this[int index]
        {
            get
            {
                return (Field)this.List[index];
            }
        }

                #endregion //Indexer (int)

            #endregion //Public Methods

            #region Internal Methods

                #region InternalAdd






        internal int InternalAdd(Field selectedField)
        {
            return this.InternalAdd(selectedField, true);
        }






        internal int InternalAdd(Field selectedField, bool validateAlreadyExists)
        {
            // Check to see if the item being added to the list
            // is already in the list is inefficient so added an overload with which you can
            // specify not to do that.
            if (validateAlreadyExists)
            {
                int index = this.List.IndexOf(selectedField);

                if (index >= 0)
                    return index;
            }

            return base.InternalAdd(selectedField);
        }

                #endregion //InternalAdd

                #region InternalInsert



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void InternalInsert(int index, Field selectedField)
        {
            if (this.List.Contains(selectedField))
            {
                Debug.Assert(false, "Field is already in the selected fields collection. Can't added it again.");
                return;
            }

            base.InternalInsert(index, selectedField);
        }

                #endregion //InternalInsert

                #region InternalSetList







        internal void InternalSetList(SelectedFieldCollection fields)
        {
			// JJD 02/16/12 - TFS101563
			// If the passed in collection is this collection then return without doing anything
			if (this == fields)
				return;

            this.InternalClear();
            this.InternalAddRange(fields);
        }

                #endregion // InternalSetList

                #region RemoveAt







        internal void RemoveAt(int index)
        {
			this.InternalRemoveAt(index);
        }

                #endregion //RemoveAt

                #region Remove






        internal void Remove(Field selectedField)
        {
            this.InternalRemove(selectedField);
        }

                #endregion //Remove

                #region ToArray

        /// <summary>
        /// The collection as an array of objects
        /// </summary>
        internal Field[] ToArray()
        {
			Field[] fields = new Field[this.Count];

			if (fields.Length > 0)
				this.CopyTo(fields, 0);

            return fields;
        }

                #endregion // ToArray

            #endregion //Internal Methods

        #endregion //Methods
    }

    #endregion //SelectedFieldCollection class

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