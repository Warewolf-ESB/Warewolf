using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using System.Diagnostics;
//using Infragistics.Windows.Input;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A collection of Field objects
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Field"/>
	/// <seealso cref="FieldLayout"/>
	/// <seealso cref="FieldLayout.Fields"/>
	/// <seealso cref="DataPresenterBase"/>
	public class FieldCollection : ObservableCollection<Field>
	{
		#region Private Members

        int _expandableFieldsCount = -1;

        // JJD 12/11/08 - TFS11474
        // Keep track of the # of expandable field that are not collapsed
        int _notCollapsedExpandableFieldsCount = -1;
		
		// JJD 7/18/07 - BR24617
		// Keep track of the number of unbound fields
		int _unboundFieldsCount = -1;

        // JJD 1/24/08 - BR29985 - added
        Field[] _unboundFields;
        static Field[] EmptyFieldArray = new Field[0];

        // JJD 1/26/08 - Optimization
        // Maintain a map of keys to indices to make IndexOf processing more efficient
        Dictionary<string, int> _indexMap = new Dictionary<string, int>();

        int _version = 0;

		// JJD 8/18/20 - TFS37033 - added
		int _internalVersion;

		private FieldLayout _owner;

		// JM 6/12/09 NA 2009.2 DataValueChangedEvent - Keep track of the Fields that have DataValueChangedScope set to AllRecords
		private List<Field>			_fieldsWithDataValueChangedScopeSetToAllRecords;

		// MD 8/17/10
		// Added the ability to suspend and resume updating on this collection.
		private int _updateCount;
		private BumpVersionFlags _bumpVersionFlagsOnUpdate;

		#endregion //Private Members

		#region Constructors

		internal FieldCollection (FieldLayout owner)
		{
			this._owner = owner;
		}

		#endregion //Constructors

		#region Base class overrides

            #region ClearItems

        /// <summary>
        /// Called when the collection is cleared
        /// </summary>
        protected override void ClearItems()
        {
			// JJD 7/18/07 - BR24617
			int oldCount = this.Count;

            base.ClearItems();

            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-recated on the next call to IndexOf
            this._indexMap.Clear();

			// JJD 7/18/07 - BR24617
			// let the owner know that a field was added or removed
			if (this._owner != null && oldCount > 0)
				this._owner.OnFieldAddedRemovedFromCollection();

            // JJD 6/03/10 - TFS33273
            // Only bump the ScrollCountRecalcVersion if there were some expandable fields
            //this.BumpVersion();
            this.BumpVersion(false, this._expandableFieldsCount > 0);

            this._expandableFieldsCount = 0;
			
			// JJD 7/18/07 - BR24617
			// Keep track of the number of unbound fields
			this._unboundFieldsCount = 0;

        }

            #endregion //ClearItems	

			#region InsertItem

        /// <summary>
        /// Inserts an item into the collection
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
		protected override void InsertItem(int index, Field item)
		{
			item.Initialize(this._owner);

			// MD 8/13/10
			// We way to prevent the FieldLayout.BumpLayoutManagerVersionRequired() method from being called while calling
			// the base InsertItem becasue it is a relatively slow method and it will be called from the BumpVersion method
			// anyway, which is called below.
			bool oldPreventBumpLayoutManagerVersionRequired = false;
			if (_owner != null)
			{
				oldPreventBumpLayoutManagerVersionRequired = _owner.PreventBumpLayoutManagerVersionRequired;
				_owner.PreventBumpLayoutManagerVersionRequired = true;
			}

			base.InsertItem(index, item);

			// MD 8/13/10
			// We no longer need to suppress the FieldLayout.BumpLayoutManagerVersionRequired() call, so restore the previous
			// value on the Prevent... property.
			if (_owner != null)
				this._owner.PreventBumpLayoutManagerVersionRequired = oldPreventBumpLayoutManagerVersionRequired;

            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-recated on the next call to IndexOf
            this._indexMap.Clear();

			// JJD 7/18/07 - BR24617
			// let the owner know that a field was added or removed
			if (this._owner != null && item != null && !item.IsUnbound)
				this._owner.OnFieldAddedRemovedFromCollection();

            // JJD 6/03/10 - TFS33273
            // Only bump the ScrollCountRecalcVersion if the field is expandable
            //this.BumpVersion();
            this.BumpVersion(false, item != null && item.IsExpandableResolved);

			// JM 07-29-09 TFS 19241
			FieldLayout fl = this._owner;
			if (fl != null &&
				fl.AutoGenerateFieldsResolved == false &&
				fl.FieldLayoutInitializedEventRaised == true &&
				// JJD 2/2/11 - TFS64770
				// Don't bypass unbound fields 
				//item.IsUnbound							== false	&&
				item.IsPropertyDescriptorInitialized == false)
			{
				// JJD 2/2/11 - TFS64770
				// If we are between BeginUpdate/EndUpdate calls then set a flag so we know to
				// VerifyAllPropertyDescriptorProviders in EndUpdate.
				// The reason is that the logic checks the Field's VersionNumber
				// and that doesn't get updated until EndUpdate
				if (_updateCount > 0)
					_bumpVersionFlagsOnUpdate |= BumpVersionFlags.VerifyAllPropertyDescriptorProviders;
				else
					this._owner.VerifyAllPropertyDescriptorProviders();
			}
		}

			#endregion //InsertItem	

			#region MoveItem
		/// <summary>
		/// Moves an item within the collection
		/// </summary>
		/// <param name="oldIndex">0 based index of the item to move.</param>
		/// <param name="newIndex">0 based index indicating the new position of the item.</param>
		protected override void MoveItem(int oldIndex, int newIndex)
		{
			base.MoveItem(oldIndex, newIndex);

            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-recated on the next call to IndexOf
            this._indexMap.Clear();

			// AS 7/18/07 BR24929
			this.BumpVersion(true);
		} 
			#endregion //MoveItem

            #region RemoveItem

        /// <summary>
        /// Called when an item is removed
        /// </summary>
        protected override void RemoveItem(int index)
        {
			Field oldItem = this[index];

            // JJD 1/8/08 - BR29410
            // Remove the field from the selected Fields collection and remove
            // any associated cells from the selected cells collection
            if (this._owner != null)
            {
                DataPresenterBase dp = this._owner.DataPresenter;

                if (dp != null)
                {
                    SelectedCellCollection selectedCells = dp.SelectedItems.Cells;

                    int count = selectedCells.Count;
                    int i;

                    // JJD 1/8/08 - BR29410
                    // Loop over the selected cells collection backwards to remove any
                    // cells from this field 
                    for (i = count - 1; i >= 0; i--)
                    {
                        if (oldItem == selectedCells[i].Field)
                            selectedCells.RemoveAt(i);
                    }

                    SelectedFieldCollection selectedFields = dp.SelectedItems.Fields;

                    i = selectedFields.IndexOf(oldItem);

                    // JJD 1/8/08 - BR29410
                    // Remove the field from the selected Fields collection 
                    if (i >= 0)
                        selectedFields.RemoveAt(i);
                }
            }

			base.RemoveItem(index);

            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-recated on the next call to IndexOf
            this._indexMap.Clear();

			// JJD 7/18/07 - BR24617
			// let the owner know that a field was added or removed
			if (this._owner != null && oldItem != null && oldItem.IsUnbound == false)
				this._owner.OnFieldAddedRemovedFromCollection();

            // JJD 6/03/10 - TFS33273
            // Only bump the ScrollCountRecalcVersion if the removed field was expandable
            //this.BumpVersion();
            this.BumpVersion(false, oldItem != null && oldItem.IsExpandableResolved);
        }

            #endregion //RemoveItem	

            #region SetItem

        /// <summary>
        /// Sets an items in the collection replacing what was there.
        /// </summary>
        /// <param name="index">The index where the item should be placed.</param>
        /// <param name="item">Teh item to set</param>
        protected override void SetItem(int index, Field item)
        {
			// JJD 7/18/07 - BR24617
			Field oldItem = this[index];

            item.Initialize(this._owner);

            base.SetItem(index, item);

            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-recated on the next call to IndexOf
            this._indexMap.Clear();

			// JJD 7/18/07 - BR24617
			// let the owner know that a field was added or removed
			if (this._owner != null && (item != null && item.IsUnbound == false) || (oldItem != null && oldItem.IsUnbound == false))
				this._owner.OnFieldAddedRemovedFromCollection();

			// AS 7/18/07 BR24929
			//this.BumpVersion();
			this.BumpVersion(true);
		}

            #endregion //SetItem	
    
		#endregion //Base class overrides	
    
		#region Properties

			#region Public Properties

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

                #region ExpandableFieldsCount

        internal int ExpandableFieldsCount
        {
            get
            {
                // if the count is dirty then re-calculate it
                if (this._expandableFieldsCount < 0)
                {
                    this._expandableFieldsCount = 0;

                    foreach (Field fld in this)
                    {
                        if (fld.IsExpandableResolved)
                            this._expandableFieldsCount++;
                    }
                }

                return this._expandableFieldsCount;
            }
        }

                #endregion //ExpandableFieldsCount	

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent - Added
				#region FieldsWithDataValueChangedScopeSetToAllRecords

		internal List<Field> FieldsWithDataValueChangedScopeSetToAllRecords
		{
			get
			{
				if (this._fieldsWithDataValueChangedScopeSetToAllRecords == null)
				{
					this._fieldsWithDataValueChangedScopeSetToAllRecords = new List<Field>(this.Count);

					foreach (Field field in this)
					{
						if (field.DataValueChangedNotificationsActiveResolved	== true &&
							field.DataValueChangedScopeResolved					== DataValueChangedScope.AllAllocatedRecords)
							this._fieldsWithDataValueChangedScopeSetToAllRecords.Add(field);
					}
				}

				return this._fieldsWithDataValueChangedScopeSetToAllRecords;
			}
		}

				#endregion //FieldsWithDataValueChangedScopeSetToAllRecords

				// JJD 8/18/20 - TFS37033 - added
				#region InternalVersion

		internal int InternalVersion { get { return this._internalVersion; } }

				#endregion //InternalVersion	
    
                // JJD 12/11/08 - TFS11474
                // Keep track of the # of expandable field that are not collapsed
                #region NotCollapsedExpandableFieldsCount

        internal int NotCollapsedExpandableFieldsCount
        {
            get
            {
                // if the count is dirty then re-calculate it
                if (this._notCollapsedExpandableFieldsCount < 0)
                {
                    this._notCollapsedExpandableFieldsCount = 0;

                    foreach (Field fld in this)
                    {
                        if (fld.IsExpandableResolved && fld.Visibility != Visibility.Collapsed)
                            this._notCollapsedExpandableFieldsCount++;
                    }
                }

                return this._notCollapsedExpandableFieldsCount;
            }
        }

                #endregion //NotCollapsedExpandableFieldsCount	

                // JJD 1/24/08 - BR29985 - added 
                #region UnboundFields

        internal Field[] UnboundFields 
        { 
            get 
            {
                if (this.UnboundFieldsCount < 1)
                    return EmptyFieldArray;

                return this._unboundFields; 
            } 
        }

                #endregion //UnboundFields	
    
                #region UnboundFieldsCount
			
		// JJD 7/18/07 - BR24617
		// Keep track of the number of unbound fields
       internal int UnboundFieldsCount
        {
            get
            {
                // if the count is dirty then re-calculate it
                if (this._unboundFieldsCount < 0)
                {
                    this._unboundFieldsCount = 0;

                    // JJD 1/24/08 - BR29985 
                    // clear the old cached array
                    this._unboundFields = null;

                    // JJD 1/24/08 - BR29985 
                    // create a list on the stack for unbound fields
                    List<Field> unboundFlds = null;

                    foreach (Field fld in this)
                    {
                        if (fld.IsUnbound)
                        {
                            this._unboundFieldsCount++;

                            // JJD 1/24/08 - BR29985
                            // 1st trime thru allocate the stack queue
                            if ( unboundFlds == null )
                                unboundFlds = new List<Field>(this.Count);

                            // JJD 1/24/08 - BR29985
                            // add the field to the queue
                            unboundFlds.Add(fld);
                        }
                    }

                    // JJD 1/24/08 - BR29985
                    // build the member array for the stack queue
                    if (unboundFlds != null)
                        this._unboundFields = unboundFlds.ToArray();
                }

                return this._unboundFieldsCount;
            }
        }

                #endregion //UnboundFieldsCount	

                // JJD 6/1/09 - TFS18108 - made public
                #region Version

        /// <summary>
        /// For internal use only
        /// </summary>
        [EditorBrowsable( EditorBrowsableState.Never)]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //internal int Version { get { return this._version; } }
        public int Version { get { return this._version; } }

                #endregion //Version	
    
            #endregion //Internal Properties

		#endregion //Properties

		#region Indexer (string key)

		/// <summary>
        /// Key based indexer (read-only) 
        /// </summary>
        /// <param name="key">The key of the field.</param>
        /// <returns>The <see cref="Field"/> with the specified key.</returns>
        public Field this[string key]
		{
			get
			{
				int index = this.IndexOf(key);

				if (index >= 0)
					return this[index];

				// JJD 6/15/07
				// If the name is a string see if it can be parsed as an int.
				if (key != null && key.Trim().Length > 0 && 
					int.TryParse(key, out index) && index >= 0)
					return this[index];

				throw new ArgumentException( DataPresenterBase.GetString( "LE_ArgumentException_14" ) );
			}
		}

		#endregion Indexer (int)

		#region Methods

		    #region Public Methods

				// MD 8/17/10
				// Added the ability to suspend and resume updating on this collection.
				#region BeginUpdate

		/// <summary>
		/// Begins an update operation on the collection. This should be called before multiple items being added and/or removed.
		/// </summary>
		/// <seealso cref="EndUpdate"/>
		public void BeginUpdate()
		{
			_updateCount++;
		}

				#endregion // BeginUpdate

				// MD 8/17/10
				// Added the ability to suspend and resume updating on this collection.
				#region EndUpdate

		/// <summary>
		/// Ends an update operation on the collection. This should be called after multiple items being added and/or removed.
		/// </summary>
		/// <seealso cref="BeginUpdate"/>
		public void EndUpdate()
		{
			_updateCount--;

			if (_updateCount > 0)
				return;

			if (_updateCount < 0)
			{
				Debug.Fail("The update count should never be less than zero.");
				_updateCount = 0;
			}

			if (_bumpVersionFlagsOnUpdate != 0)
			{
				this.BumpVersion(
					(_bumpVersionFlagsOnUpdate & BumpVersionFlags.BumpVersionAndInvalidate) != 0,
					(_bumpVersionFlagsOnUpdate & BumpVersionFlags.BumpVersionAndScrollCountRecalcVersion) != 0);
				
				// JJD 2/2/11 - TFS64770
				// If the VerifyAllPropertyDescriptorProviders flag is set then call
				// VerifyAllPropertyDescriptorProviders now.
				if ((_bumpVersionFlagsOnUpdate & BumpVersionFlags.VerifyAllPropertyDescriptorProviders) != 0)
					this._owner.VerifyAllPropertyDescriptorProviders();

				_bumpVersionFlagsOnUpdate = 0;
			}
		}

				#endregion // EndUpdate

		        #region IndexOf

		/// <summary>
		/// Gets the index of the Field with the specified key.
		/// </summary>
		/// <param name="name">The name of the item to search for</param>
		/// <returns>The zero-based index or -1 if key not found</returns>
		public int IndexOf(string name)
		{
            // JJD 1/26/08 - Optimization
            // Create a hash table to speed up index searches
            // If the indexMap count is zero then it needs to be initialized
            #region Old code

            //int count = this.Count;

            //Field fld;
            //for (int i = 0; i < count; i++)
            //{
            //    fld = this[i];

            //    if (name == fld.Name)
            //        return i;
            //}

            #endregion //Old code
            if (this._indexMap.Count == 0)
                this.InitializeIndexMap();

            int index;

            if ( null != name && this._indexMap.TryGetValue(name, out index))
                return index;

			return -1;
		}

		        #endregion //IndexOf

		    #endregion //Public Methods

            #region Internal Methods

				#region GetFieldFromPropertyDescriptor

		internal Field GetFieldFromPropertyDescriptor(PropertyDescriptor propertyDescriptor)
		{
			// find the matching field
            // JJD 5/15/09
            // Optimization - use for loop instead of foreach
            //foreach (Field fld in this)
            int fieldCount = this.Count;

            for (int i = 0; i < fieldCount; i++)
            {
                Field fld = this[i];

				if (fld.DoesPropertyMatch(propertyDescriptor))
					return fld;
			}

			return null;
		}

				#endregion //GetFieldFromPropertyDescriptor	

                #region GetFirstFieldVisibleInCellArea

		internal Field GetFirstFieldVisibleInCellArea()
        {
            int count = this.Count;

            for (int i = 0; i < Count; i++)
            {
                if (this[i].IsVisibleInCellArea)
                    return this[i];
            }

            return null;
		}

				#endregion //GetFirstFieldVisibleInCellArea

                // JJD 1/26/08 - Optimization - added
                #region OnFieldNameChanged

        internal void OnFieldNameChanged(Field field, string newName, string oldName)
        {
            // JJD 1/26/08 - Optimization
            // clear the index map. It will get re-created on the next call to IndexOf
            this._indexMap.Clear();
        }

                #endregion //OnFieldNameChanged	
    
				#region OnIsExpandableFieldInvalidated

		internal void OnIsExpandableFieldInvalidated(Field field)
        {
            this._expandableFieldsCount = -1;

            // JJD 12/11/08 - TFS11474
            // Keep track of the # of expandable field that are not collapsed
            this._notCollapsedExpandableFieldsCount = -1;

			// SSP 8/28/09 - Enhanced grid-view - TFS21591
			// We need to bump the version number otherwise ExpandableFieldRecordCollection.VerifyChildren
			// will not do anything. We need to verify the expandable field record collections and 
			// add/remove the associated expandable field record for the field whose IsExpandable state
			// changed.
			// 
			this.BumpVersion( true );
        }

                #endregion //OnIsExpandableFieldInvalidated

				// JM 6/12/09 NA 2009.2 DataValueChangedEvent - Added
				#region RefreshDataValueChangedCounts

		internal void RefreshDataValueChangedCounts()
		{
		}

				#endregion //RefreshDataValueChangedCounts

				#region ShouldSerialize

		/// <summary>
		/// Determines if any property value is set to a non-default value.
		/// </summary>
		/// <returns>Returns true if any property value is set to a non-default value.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
        {
            // return true if any field in the collection should be serialized
            foreach (Field fld in this)
            {
                if (fld.ShouldSerialize())
                    return true;
            }

            return false;
        }

                #endregion //ShouldSerialize	
    
            #endregion //Internal Methods	

            #region Private Methods

                #region BumpVersion

		private void BumpVersion()
		{
			// AS 7/18/07 BR24929
			this.BumpVersion(false);
		}

		// AS 7/18/07 BR24929
		// Added an overload so we can invalidate the owning items host.
		//
        private void BumpVersion(bool invalidate)
        {
		    // JJD 6/03/10 - TFS33273
            this.BumpVersion(invalidate, true);
        }

		// JJD 6/03/10 - TFS33273
		// Added an overload so we can selectively bump the ScrollCountRecalcVersion.
		//
        private void BumpVersion(bool invalidate, bool bumpScrollCountRecalcVersion) 
        {
			// JJD 8/18/20 - TFS37033
			// Always bunp the internal version but don't send out any notification
			// even if the _updateCount is > 0
			this._internalVersion++;

			// MD 8/17/10
			// If we are currently updating, don't actually bump the version. Just keep track of the parameters 
			// we should use when we eventually do bump the version.
			if (_updateCount > 0)
			{
				if (invalidate)
					_bumpVersionFlagsOnUpdate |= BumpVersionFlags.BumpVersionAndInvalidate;

				if (bumpScrollCountRecalcVersion)
					_bumpVersionFlagsOnUpdate |= BumpVersionFlags.BumpVersionAndScrollCountRecalcVersion;

				return;
			}

            this._version++;
            this._expandableFieldsCount = -1;

            // JJD 12/11/08 - TFS11474
            // Keep track of the # of expandable field that are not collapsed
            this._notCollapsedExpandableFieldsCount = -1;

			// JJD 7/18/07 - BR24617
			// Dirty the number of unbound fields
			this._unboundFieldsCount = -1;

			// JM 6/12/09 NA 2009.2 DataValueChangedEvent
			// Clear the list of Fields that have DataValueChangedScope set to AllRecords.
			if (this._fieldsWithDataValueChangedScopeSetToAllRecords != null)
			{
				this._fieldsWithDataValueChangedScopeSetToAllRecords.Clear();
				this._fieldsWithDataValueChangedScopeSetToAllRecords = null;
			}
		
			// SSP 8/28/09 - Enhanced grid-view - TFS21591
			// 
			DataPresenterBase dp = null != _owner ? _owner.DataPresenter : null;

            // JJD 6/03/10 - TFS33273
            // Only bump the ScrollCountRecalcVersion if param is true
            //if ( null != dp )
			if ( null != dp && bumpScrollCountRecalcVersion)
				dp.BumpScrollCountRecalcVersion( );

			// AS 7/18/07 BR24929
			// For a Move and a Set/Replace, we need to explicitly invalidate the 
			// items host in order for the header to be displayed.
			//
			
			
			
            
			
			if ( invalidate && null != dp )
            {
                dp.InvalidateItemsHost();

                // JJD 6/1/09 - TFS18108
                // Bump the overall sort version so the GroupbyAreaMulti will get refreshed if needed
                dp.BumpOverallSortVersion();
            }

            // AS 12/17/08 NA 2009 Vol 1 - Fixed Fields
            if (null != this._owner)
                this.Owner.BumpLayoutManagerVersion();

            // JJD 6/1/09 - TFS18108
            // Rais the propchanged notification
            this.OnPropertyChanged( new PropertyChangedEventArgs("Version"));

        }

                #endregion //BumpVersion

                // JJD 1/26/08 - Optimization - added
                #region InitializeIndexMap

        void InitializeIndexMap()
        {
            int count = this.Count;

            if (count == 0)
                return;

            Field fld;
            string key;

            // JJD 1/26/08 - Optimization
            // Lopp over all the fields and build a map between the field names
            // and the index of the field
            for (int i = 0; i < count; i++)
            {
                fld = this[i];

                key = fld.Name;

                if (key != null && key.Length > 0 &&
                    !this._indexMap.ContainsKey(key))
                    this._indexMap.Add(key, i);
            }
        }

                #endregion //InitializeIndexMap	
    
            #endregion //Private Methods	
        
		#endregion //Methods

        #region FieldEnumerator class

        internal class FieldEnumerator : IEnumerator, IEnumerator<Field>
        {
            IEnumerator _enumerator;

            internal FieldEnumerator(IEnumerator enumerator)
            {
                this._enumerator = enumerator;
            }

            #region IEnumerator<Record> Members

            public Field Current
            {
                get { return this._enumerator.Current as Field; }
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

            Field IEnumerator<Field>.Current
            {
                get { return this._enumerator.Current as Field; }
            }
        }
        #endregion //FieldEnumerator

		// MD 8/17/10
		// Added the ability to suspend and resume updating on this collection.
		#region BumpVersionFlags enum

		private enum BumpVersionFlags : byte
		{
			BumpVersionAndInvalidate = 0x01,
			BumpVersionAndScrollCountRecalcVersion = 0x02,
			VerifyAllPropertyDescriptorProviders = 0x04,
		} 

		#endregion // BumpVersionFlags enum
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