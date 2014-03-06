using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Data;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;
using System.Data;
using System.Diagnostics;
using System.Threading;
//using Infragistics.Windows.Input;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
    /// A collection of FieldSortDescription objects
    /// </summary>
	/// <seealso cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>
	/// <seealso cref="Field"/>
	/// <seealso cref="FieldLayout"/>
	/// <seealso cref="FieldLayout.SortedFields"/>
	/// <seealso cref="FieldSortDescription"/>
	public class FieldSortDescriptionCollection : IList<FieldSortDescription>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
		#region Private Members

        private int _countOfGroupByFields;
        private List<FieldSortDescription> _list;
        private bool _collectionDirty;
        private int _groupbyVersion;
		private object _syncRoot;
        private bool _inBeginUpdate; // JJD 1/29/09 - added
        // JJD 3/19/09 - TFS14672
        // Keep track of the groupby fields when begin update is called
        private List<FieldSortDescription> _oldGroupByFields;
        

		private FieldLayout _owner;

		#endregion //Private Members

		#region Constructors

        internal FieldSortDescriptionCollection(FieldLayout owner, List<FieldSortDescription> list) 
		{
			this._owner = owner;
            this._list = list;
		}

		#endregion //Constructors

		#region Evemts

		/// <summary>
		/// Occurs when a property value has changed
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when a the contents of the collection is modified
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion // Events
    
		#region Properties

			#region Public Properties

				#region Count

		/// <summary>
		/// Returns the number of items in the collection
		/// </summary>
		public int Count
		{
			get { return this._list.Count; }
		}

				#endregion //Count	
    
                #region CountOfGroupByFields

        /// <summary>
        /// Returns the count of the fields that are grouped by (read-only).
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public int CountOfGroupByFields
        {
            get
            {
                // JJD 5/20/09 - TFS17823
                // Until the owner's fields are initialized (i.e. a record is
                // encountered that uses the field layout we need to count up
                // the groupby fields 
                if (this._owner.AreFieldsInitialized == false)
                {
                    int count = this.Count;
                    int countOfGroupByFields = 0;

                    for ( int i = 0; i < count; i++)
                    {
                        FieldSortDescription fsd = this[i];

                        // if the field isn't a groupby
                        // then stop counting and break out
                        if (fsd.IsGroupBy)
                            countOfGroupByFields++;
                        else
                            break;

                        // verify that the field has been predefined
                        Field field = fsd.Field;

                        if (field == null)
                        {
                            // since the field hasn't been resolved yet
                            // try to do it now
                            this.InitializeFieldStatus(fsd);
                            
                            field = fsd.Field;

                            // if the field isn't found then return 0
                            // since it they didn't predefine the field we
                            // can't make any assumptions about what we should do
                            if (field == null)
                                return 0;
                        }
                    }

                    return countOfGroupByFields;
                }

                return this._countOfGroupByFields;
            }
        }

                #endregion //CountOfGroupByFields	
    
				#region Owner

		/// <summary>
		/// Returns the FieldLayout that owns this collection
		/// </summary>
		public FieldLayout Owner
		{
			get
			{
				return this._owner;
			}
		}

				#endregion //Owner	
    
			#endregion //Public Properties

            #region Internal Properties

                #region GroupbyVersion

        internal int GroupbyVersion { get { return this._groupbyVersion; } }

                #endregion //GroupbyVersion

            #endregion //Internal Properties	
        
		#endregion //Properties

		#region Indexers

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="field">The associated field</param>
		/// <returns>The FieldSortDescription in the collection that is associated with this field.</returns>
		public FieldSortDescription this[Field field]
		{
			get
			{
				int index = this.IndexOf(field);

				if (index >= 0)
					return this._list[index];

				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_23" ) );
			}
		}

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="fieldName">The associated field's name</param>
		/// <returns>The FieldSortDescription in the collection that is associated with this field.</returns>
		public FieldSortDescription this[string fieldName]
		{
			get
			{
				int index = this.IndexOf(fieldName);

				if (index >= 0)
					return this._list[index];

				// JJD 6/15/07
				// If the name is a string see if it can be parsed as an int.
				if (fieldName != null && fieldName.Trim().Length > 0 &&
					int.TryParse(fieldName, out index) && index >= 0)
					return this._list[index];

				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_24" ) );
			}
		}

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index">The zero-based index into the collection.</param>
		/// <returns>The object in the collection at the specified index</returns>
		public FieldSortDescription this[int index] 
        { 
            get 
            {
                this.VerifySortOrder();

                return this._list[index]; 
            }
			set
			{
				throw new NotSupportedException( DataPresenterBase.GetString( "LE_NotSupportedException_10" ) );
			}
		}

		#endregion Indexers

		#region Methods

			#region Public Methods

				#region Add

		/// <summary>
		/// Adds an item to the collection
		/// </summary>
		/// <param name="item">The item to add.</param>
		public void Add(FieldSortDescription item)
		{
			this.InsertItem(this._list.Count, item);
		}

				#endregion //Add	

                // JJD 1/29/09 - added
                #region BeginUpdate

        /// <summary>
        /// Prevents change notification from being raised while multiple updates are made to the collection.
        /// </summary>
        /// <remarks>
        /// <para class="note"><b>Note:</b> it is the caller's responsibilty to call <see cref="EndUpdate"/> after the updates are completed.</para></remarks>
        /// <seealso cref="EndUpdate"/>
        public void BeginUpdate()
        {
            if ( this._inBeginUpdate )
                return;

            // JJD 3/19/09 - TFS14672
            // Keep track of the groupby fields when begin update is called
            int countOfGroupBys = this.CountOfGroupByFields;

            if (countOfGroupBys == 0)
                this._oldGroupByFields = null;
            else
            {
                this._oldGroupByFields = new List<FieldSortDescription>(countOfGroupBys);

                for (int i = 0; i < countOfGroupBys; i++)
                    this._oldGroupByFields.Add(this._list[i]);
            }

            this._inBeginUpdate = true;
        }

                #endregion //BeginUpdate	
    
				#region Clear

		/// <summary>
		/// Clears all items from the collection
		/// </summary>
		public void Clear()
		{
			this.ClearItems();
		}

				#endregion //Clear	
    
                #region ClearNonGroupByFields

        /// <summary>
        /// Clears all sort descriptions that are not flagged as groupby fields.
        /// </summary>
        public void ClearNonGroupByFields()
        {
            // if all of the fields are groupbys then just return
            if (this.CountOfGroupByFields == this.Count)
                return;

            // if there are no groupbys then call the Clear method and return
            if (this.CountOfGroupByFields < 1)
            {
                this.Clear();
                return;
            }

            // remove from the back going forward
            while (this.CountOfGroupByFields < this.Count)
            {
                int index = this.Count - 1;

                this[index].Field.SetSortingStatus(SortStatus.NotSorted, false);
                this._list.RemoveAt(index);
            }

            this.RaisePropertyChangedEvent( "Count");
            this.RaisePropertyChangedEvent( "Item[]");
			this.OnCollectionReset();

        }

                #endregion //ClearNonGroupByFields	

				#region Contains

		/// <summary>
		/// Determines if the collection contains an item.
		/// </summary>
		/// <param name="item">The item to check</param>
		/// <returns>True if the item is in the collection.</returns>
		public bool Contains(FieldSortDescription item)
		{
			return this._list.Contains(item);
		}

				#endregion //Contains	
    
                #region Contains (field)

        /// <summary>
        /// Determines if the field has an associated FieldSortDescription in the collection. 
        /// </summary>
        /// <param name="field">The associated field</param>
        /// <returns>True if the field has an associated FieldSortDescription in the collection.</returns>
        public bool Contains(Field field)
		{
            return (this.IndexOf(field) >= 0);
        }

		/// <summary>
		/// Determines if the field has an associated FieldSortDescription in the collection. 
		/// </summary>
		/// <param name="fieldName">The associated field name</param>
		/// <returns>True if the field has an associated FieldSortDescription in the collection.</returns>
		public bool Contains(string fieldName)
		{
            return (this.IndexOf(fieldName) >= 0);
        }

                #endregion //Contains (field)

				#region CopyTo

		/// <summary>
		/// Copies the items into an array
		/// </summary>
		/// <param name="array">The array to receive the items.</param>
		/// <param name="arrayIndex">The starting index</param>
		public void CopyTo(FieldSortDescription[] array, int arrayIndex)
		{
			this._list.CopyTo(array, arrayIndex);
		}

				#endregion //CopyTo	

                // JJD 1/29/09 - added
                #region EndUpdate

        /// <summary>
        /// Raises and re-enables change notifications suspended by a prior call to <see cref="BeginUpdate"/>.
        /// </summary>
        /// <seealso cref="BeginUpdate"/>
        public void EndUpdate()
        {
            this._inBeginUpdate = false;

            bool groupByFieldsHaveChanged = false;

            // JJD 3/19/09 - TFS14672
            // Keep track of the groupby fields when begin update is called
            int countOfGroupBys = this.CountOfGroupByFields;

            if (this._oldGroupByFields == null )
                groupByFieldsHaveChanged = countOfGroupBys > 0;
            else
            {
                if (this._oldGroupByFields.Count != countOfGroupBys)
                    groupByFieldsHaveChanged = true;
                else
                {
                    for (int i = 0; i < countOfGroupBys; i++)
                    {
                        FieldSortDescription fsdNew = this[i];
                        FieldSortDescription fsdOld = this._oldGroupByFields[i];

                        if (fsdNew != fsdOld)
                        {
                            groupByFieldsHaveChanged = true;
                            break;
                        }
                    }
                }
            }

            // JJD 3/19/09 - TFS14672
            // clear the old group by cache
            this._oldGroupByFields = null;

            this.RaisePropertyChangedEvent("Count");

            // JJD 3/19/09 - TFS14672
            // Only raise the CountOfGroupByFields notification if the groupbys have changed
            // since the call to begin
            if (groupByFieldsHaveChanged)
                this.RaisePropertyChangedEvent("CountOfGroupByFields");

            this.RaisePropertyChangedEvent("Item[]");
			this.OnCollectionReset();
        }

                #endregion //BeginUpdate	

				#region GetEnumerator

		/// <summary>
		/// Gets an enumerator for the items in the collection
		/// </summary>
		/// <returns>An enumerator</returns>
		public IEnumerator<FieldSortDescription> GetEnumerator()
		{
			this.VerifySortOrder();
			return this._list.GetEnumerator();
		}

				#endregion //GetEnumerator	
    
                #region IndexOf

        /// <summary>
		/// Gets the index of the FieldSortDescription for the specified field.
		/// </summary>
		/// <param name="field">The associated field</param>
		/// <returns>The zero-based index or -1 if the field is not found</returns>
		public int IndexOf(Field field)
		{
            if (field == null)
				throw new ArgumentNullException( "field", DataPresenterBase.GetString( "LE_ArgumentNullException_3", "field" ) );

            this.VerifySortOrder();

            int count = this.Count;

			FieldSortDescription fsd;
			for (int i = 0; i < count; i++)
			{
				fsd = this._list[i];

				if (field == fsd.Field)
					return i;
			}

			return -1;
        }

        /// <summary>
		/// Gets the index of the FieldSortDescription for the specified field.
		/// </summary>
		/// <param name="fieldName">The associated field</param>
		/// <returns>The zero-based index or -1 if the field is not found</returns>
		public int IndexOf(string fieldName)
		{
            if (fieldName == null)
				throw new ArgumentNullException( "fieldName", DataPresenterBase.GetString( "LE_ArgumentNullException_3", "fieldName" ) );

            this.VerifySortOrder();
            
            int count = this.Count;

			FieldSortDescription fsd;
			for (int i = 0; i < count; i++)
			{
				fsd = this._list[i];

				if (fieldName == fsd.FieldName)
					return i;
			}

			return -1;
        }

        /// <summary>
		/// Gets the index of the item in the collection.
		/// </summary>
		/// <param name="item">The item in question</param>
		/// <returns>The zero-based index or -1 if the item is not found</returns>
        public int IndexOf(FieldSortDescription item)
		{
            this.VerifySortOrder();

            return this._list.IndexOf(item);
        }

                #endregion //IndexOf

				#region Insert

		/// <summary>
		/// Inserts an item into the collection at a specified index
		/// </summary>
		/// <param name="index">The index to insert the item</param>
		/// <param name="item">The item to insert</param>
		public void Insert(int index, FieldSortDescription item)
		{
			this.InsertItem(index, item);
		}

				#endregion //Insert	

				#region Remove

		/// <summary>
		/// Removes an item from the collection
		/// </summary>
		/// <param name="item">The item to remove</param>
		/// <returns>True if successful.</returns>
		public bool Remove(FieldSortDescription item)
		{
			int index = this._list.IndexOf(item);

			if (index < 0)
				return false;

			this.RemoveItem(index);

			return true;
		}

				#endregion //Remove	

				#region RemoveAt

		/// <summary>
		/// Removes an item from that collection at a specified index.
		/// </summary>
		/// <param name="index">The index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			this.RemoveItem(index);
		}

				#endregion //RemoveAt	
        
            #endregion //Public Methods

            #region Internal Methods

				#region AddRange

		// SSP 8/25/10 TFS30982
		// 
		internal void AddRange( IEnumerable<FieldSortDescription> items )
		{
			bool beginEditCalled = false;
			if ( !_inBeginUpdate )
			{
				this.BeginUpdate( );
				beginEditCalled = true;
			}

			foreach ( FieldSortDescription ii in items )
				this.Add( ii );

			if ( beginEditCalled )
				this.EndUpdate( );
		} 

				#endregion // AddRange

				// AS 6/1/09 NA 2009.2 Undo/Redo
				#region CloneNonGroupBy
		internal FieldSortDescription[] CloneNonGroupBy()
		{
			List<FieldSortDescription> clones = new List<FieldSortDescription>();

			for (int i = 0; i < this.Count; i++)
			{
				FieldSortDescription fsd = this[i];

				if (!fsd.IsGroupBy)
					clones.Add(fsd.Clone());
			}

			return clones.ToArray();
		} 
				#endregion //CloneNonGroupBy

                #region OnOwnerInitialized

        internal void OnOwnerInitialized()
        {
			// AS 3/22/10 TFS29701
			// If the fieldlayout was previously in use then we may have had set 
			// the _countOfGroupByFields. If we get here then we should reset the 
			// count of the groupby as we're going to increment it if the fsd 
			// is a group by.
			//
			int oldGroupByCount = this._countOfGroupByFields;
			_countOfGroupByFields = 0;

            foreach (FieldSortDescription fsd in this)
            {
                this.InitializeFieldStatus(fsd);
            }

			// AS 3/22/10 TFS29701
			// If we had group by fields but don't any more then we need to send a 
			// change notification for the CountOfGroupByFields since one would not 
			// have been set when the InitializeFieldStatus was invoked.
			//
			if (_countOfGroupByFields == 0 && oldGroupByCount != 0)
			{
				this.RaisePropertyChangedEvent("CountOfGroupByFields");
			}
        }

                #endregion //OnOwnerInitialized	
    
				#region Sort

		/// <summary>
		/// Called to sort the list
		/// </summary>
		internal void Sort()
		{
			if (this.Count < 2)
				return;

			int i = 0;

			// Initialize the PreSportIndex before we do the sort so that we
			// will maintain the relative order but move all groupby entires
			// before non groupby entries
			foreach (FieldSortDescription fsd in this)
				fsd.PreSortIndex = i++;

			this._list.Sort(new GroupBySortComparer());
		}

				#endregion //Sort	
    
				// AS 6/1/09 NA 2009.2 Undo/Redo
				#region TryGetValue
		internal int TryGetValue(Field field, out FieldSortDescription fsd)
		{
			for (int i = 0, count = this.Count; i < count; i++)
			{
				FieldSortDescription tempFsd = this[i];

				if (tempFsd.Field == field)
				{
					fsd = tempFsd;
					return i;
				}
			}

			fsd = null;
			return -1;
		} 
				#endregion //TryGetValue

            #endregion //Internal Methods

            #region Private Methods

				#region ClearItems

        private void ClearItems()
        {
			if (this._list.Count == 0)
				return;

            // reset the readonly flags on the field objects
            foreach (FieldSortDescription fsd in this)
            {
                if (fsd.Field != null)
                    fsd.Field.SetSortingStatus(SortStatus.NotSorted, false);
            }

            this._list.Clear();

            if (this._countOfGroupByFields != 0)
            {
                this._groupbyVersion++;
                this._countOfGroupByFields = 0;
                this.RaisePropertyChangedEvent("CountOfGroupByFields");
            }

            this.RaisePropertyChangedEvent("Count");
            this.RaisePropertyChangedEvent("Item[]");
			this.OnCollectionReset();
       }

				#endregion //ClearItems

                #region InitializeFieldStatus

        private void InitializeFieldStatus( FieldSortDescription fsd )
        {
            // AS 4/18/07 BR21988
            // We need to wait until the field layout's fields
            // have been initialized or we will end up incrementing
            // the counters - e.g. _countOfGroupByFields - twice.
            //
            // JJD 5/20/09 - TFS17823
            // Moved this check below so we can still initialize the sortdescription
            // before any records are encountered
            //if (this._owner.AreFieldsInitialized == false)
            //    return;

            Field field = fsd.Field;

            if ( field == null )
            {
                int index = this._owner.Fields.IndexOf( fsd.FieldName );

                if ( index < 0 )
                    return;

                field = this._owner.Fields[index];

                fsd.Field = field;
            }

            // JM 6-01-09 TFS17948 - Move this further down so we can set the sort status on the Field before any records
            //						 are loaded.
            //// JJD 5/20/09 - TFS17823
            //// Moved this check from above so we can still initialize the sortdescription
            //// before any records are encountered
            //if (this._owner.AreFieldsInitialized == false)
            //    return;

            //if (!fsd.IsSealed)
            //    fsd.Seal();

            SortStatus status;

            if ( fsd.Direction == ListSortDirection.Ascending )
                status = SortStatus.Ascending;
            else
                status = SortStatus.Descending;

            // set the readonly flags on the field object
            field.SetSortingStatus( status, fsd.IsGroupBy );

            // JM 6-01-09 TFS17948 - Moved this from up above so we can set the sort status on the Field before any records
            //						 are loaded.
            // JJD 5/20/09 - TFS17823
            // Moved this check from above so we can still initialize the sortdescription
            // before any records are encountered
            // SSP 6/14/10 TFS32985
            // We need to bump the _countOfGroupByFields property's value below otherwise that counter
            // will never get initialized to the correct value unless OnOwnerInitialized is called which 
            // may not get called if the records have already been created. Also OnOwnerInitialized now
            // resets the count to 0 so the original bug BR21988 should not occur anymore even without 
            // this.
            // 
            //if (this._owner.AreFieldsInitialized == false)
            //	return;

            if ( !fsd.IsSealed )
                fsd.Seal( );

            if ( fsd.IsGroupBy )
            {
                this._countOfGroupByFields++;
                this._groupbyVersion++;
                this.RaisePropertyChangedEvent( "CountOfGroupByFields" );
            }
        }

                #endregion //InitializeFieldStatus	

				#region InsertItem

        private void InsertItem(int index, FieldSortDescription fsd)
        {
            if (fsd == null)
				throw new ArgumentNullException( "fsd", DataPresenterBase.GetString( "LE_ArgumentNullException_3", "FieldSortDescription" ) );

            if (fsd.Field != null &&
                fsd.Field.Owner != this._owner)
				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_25" ) );

			// JM 05-30-08 BR33244 - Remove this check since we allow Field.Name to be null this check can fail unneccesarily if there is more 
			// than one field with the same name.  (Note:the FieldCollection allows Fields with duplicate names)
			//if (this.IndexOf(fsd.FieldName) >= 0)
			//	throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_26" ) );

            int existingIndex = -1;

            // JJD 1/29/09
            // See if an entry for this field already exists
            if (fsd.Field != null)
                existingIndex = this.IndexOf(fsd.Field);
            else
                if (fsd.FieldName != null)
                    existingIndex = this.IndexOf(fsd.FieldName);

            if (existingIndex >= 0)
            {
                this.RemoveAt(existingIndex);
                if (existingIndex < index)
                    index--;
            }

            this._list.Insert(index, fsd);

            //// seal the FieldSortDescription object so it will be immutable
            if (fsd.Field != null)
            {
                fsd.Seal();
            
            //    this._collectionDirty = true;

            //    if (fsd.IsGroupBy)
            //    {
            //        this._countOfGroupByFields++;
            //        this.OnPropertyChanged("CountOfGroupByFields");
            //    }
            }
            
            this._collectionDirty = true;

            this.InitializeFieldStatus(fsd);
		
			this.RaisePropertyChangedEvent("Count");
			this.RaisePropertyChangedEvent("Item[]");
		    this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, fsd, index));
		}

				#endregion //InsertItem

				#region OnCollectionReset

		private void OnCollectionReset()
		{
			this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

				#endregion //OnCollectionReset
    
				#region RaiseCollectionChangedEvent

		private void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
		{
            // JJD 1/29/09 
            // Don't raise any notifications if BeginUpdate was called
            //if (this.CollectionChanged != null)
            if (this._inBeginUpdate == false && this.CollectionChanged != null)
				this.CollectionChanged(this, e);
		}

				#endregion //RaiseCollectionChangedEvent	

				#region RaisePropertyChangedEvent

		private void RaisePropertyChangedEvent(string propertyName)
		{
            // JJD 1/29/09 
            // Don't raise any notifications if BeginUpdate was called
            //if (this.PropertyChanged != null)
            if (this._inBeginUpdate == false && this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

				#endregion //RaisePropertyChangedEvent	
    
				#region RemoveItem

        /// <summary>
        /// Called when an item is removed
        /// </summary>
        private void RemoveItem(int index)
        {
            FieldSortDescription fsd = this[index];

            // JJD 1/28/09 - TFS12487
            bool groupByCountChanged = false;

            // reset the readonly flags on the field object
            if (fsd.Field != null)
            {
                fsd.Field.SetSortingStatus(SortStatus.NotSorted, false);

                this._collectionDirty = true;

                if (fsd.IsGroupBy)
                {
					// JM 6-01-09 TFS17948 - Since we now allow the sorting status to be set before records are loaded (see
					//						 changes in the InitializeFieldStatus method above with this bug number) the countOfGroupByFields
					//						 may not be initialized even though we have a FieldSortDescription with IsGroupBy = true.
					//						 Check to make sure it is greater than zero before decrementing.
					if (this._countOfGroupByFields > 0)
						this._countOfGroupByFields--;

                    Debug.Assert(this._countOfGroupByFields >= 0);

                    this._groupbyVersion++;

                    // JJD 1/28/09 - TFS12487
                    // Delay raising the notification until after we update the list
                    //this.RaisePropertyChangedEvent( "CountOfGroupByFields" );
                    groupByCountChanged = true;
                }
            }

            this._list.RemoveAt(index);

			this.RaisePropertyChangedEvent("Count");
			this.RaisePropertyChangedEvent("Item[]");
			// JM 07-15-09 TFS 18672 - Move below.
			//this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, fsd, index));
            
            if ( groupByCountChanged == true)
                this.RaisePropertyChangedEvent( "CountOfGroupByFields" );

			// JM 07-15-09 TFS 18672 - Moved here from above.
			this.RaiseCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, fsd, index));
		}

				#endregion //RemoveItem

                #region VerifySortOrder

        // the collection must be sorted so that the groupby field come before
        // the non-groupby fields
        private void VerifySortOrder()
        {
            if (this._collectionDirty == true)
            {
                this._collectionDirty = false;
                this.Sort();
            }
        }

                #endregion //VerifySortOrder	
    
            #endregion //Private Methods

        #endregion //Methods

        #region GroupBySortComparer

        private class GroupBySortComparer : IComparer<FieldSortDescription>
        {
            int IComparer<FieldSortDescription>.Compare(FieldSortDescription x, FieldSortDescription y)
            {
                if (x == y)
                    return 0;

                if (x == null)
                    return -1;

                if (y == null)
                    return -1;

                if (y.IsGroupBy != x.IsGroupBy)
                {
                    if (x.IsGroupBy)
                        return -1;
                    else
                        return 1;
                }

                if (x.PreSortIndex < y.PreSortIndex)
                    return -1;
                else
                    return 1;
            }

        }

        #endregion //GroupBySortComparer

        #region IEnumerable Members

        /// <summary>
        /// Returns a enumerator for the list
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            this.VerifySortOrder();
            return this._list.GetEnumerator();
        }

        #endregion

		#region ICollection<FieldSortDescription> Members

		bool ICollection<FieldSortDescription>.IsReadOnly
		{
			get { return false; }
		}

		#endregion

		#region IList Members

		int IList.Add(object value)
		{
			this.Add(value as FieldSortDescription);

			return this.Count;
		}

		void IList.Clear()
		{
			this.ClearItems();
		}

		bool IList.Contains(object value)
		{
			return this._list.Contains(value as FieldSortDescription);
		}

		int IList.IndexOf(object value)
		{
			this.VerifySortOrder();
			return this._list.IndexOf(value as FieldSortDescription);
		}

		void IList.Insert(int index, object value)
		{
			this.InsertItem(index, value as FieldSortDescription);
		}

		bool IList.IsFixedSize
		{
			get { return false; }
		}

		bool IList.IsReadOnly
		{
			get { return false; }
		}

		void IList.Remove(object value)
		{
			this.RemoveItem(this._list.IndexOf(value as FieldSortDescription));
		}

		void IList.RemoveAt(int index)
		{
			this.RemoveItem(index);
		}

		object IList.this[int index]
		{
			get
			{
				this.VerifySortOrder();
				return this[index];
			}
			set
			{
				throw new NotSupportedException(DataPresenterBase.GetString("LE_NotSupportedException_10"));
			}
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo(Array array, int index)
		{
			this.VerifySortOrder();

			this._list.CopyTo(array as FieldSortDescription[], index);
		}

		bool ICollection.IsSynchronized
		{
			get {return false; }
		}

		object ICollection.SyncRoot
		{
			get 
			{
				if (this._syncRoot == null)
					Interlocked.CompareExchange(ref this._syncRoot, new object(), null);

				return this._syncRoot; 
			}
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