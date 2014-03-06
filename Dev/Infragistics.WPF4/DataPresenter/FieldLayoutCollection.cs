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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;
using System.Data;
using System.Diagnostics;
using System.Xml;
using Infragistics.Windows.Internal;
using System.Windows.Threading;

namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
	/// A collection of FieldLayout objects
	/// </summary>
	/// <remarks>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields.html">Fields</a> topic in the Developer's Guide for an explanation of fields.</p>
	/// <p class="body">Refer to the <a href="xamData_Terms_Fields_Field_Layout.html">Field Layout</a> topic in the Developer's Guide for an explanation of the FieldLayout object.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this object is used.</p>
	/// </remarks>
	/// <seealso cref="Field"/>
	/// <seealso cref="FieldLayout"/>
	/// <seealso cref="DataPresenterBase"/>
	/// <seealso cref="DataPresenterBase.FieldLayouts"/>
	sealed public class FieldLayoutCollection : ObservableCollection<FieldLayout>
	{
		#region Private Members

        private DataPresenterBase _presenter;
		private List<WeakReference> _propertyDescriptorProviders;
		private RecordCollectionBase _templateDataRecords;

		// AS 8/25/09 TFS17560
		private RecordCollectionBase _templateGroupByRecords;

        // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
        private FieldLayout _typedListRootFieldLayout;

		#endregion //Private Members

		#region Constructors

        internal FieldLayoutCollection(DataPresenterBase presenter)
		{
			this._presenter = presenter;
		}

		#endregion //Constructors

		#region Base class overrides

			#region ClearItems

		/// <summary>
		/// Called when the collection is cleared
		/// </summary>
		protected override void ClearItems()
		{
            int count = this.Count;

            // JJD 9/21/09 - TFS18162 
            // Call OnRemovedFromCollection on each fieldlayout so they can remove their template records and cleanup
            // any cached elements
            for (int i = 0; i < count; i++) 
            {
                FieldLayout fl = this[i];

                if (fl != null)
                    fl.OnRemovedFromCollection();
            }

			base.ClearItems();

            // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping 
            // clear the cached fl since it was removed
            this._typedListRootFieldLayout = null;

			// verify the property descriptors
			this.VerifyPropertyDescriptorProviders();
		}

			#endregion //ClearItems	
    
			#region InsertItem

        /// <summary>
        /// Inserts an item in the collection
        /// </summary>
        /// <param name="index">The index where the item should be inserted.</param>
        /// <param name="item">The item to insert</param>
		protected override void InsertItem(int index, FieldLayout item)
		{
			item.InitializeOwner(this._presenter);

			base.InsertItem(index, item);

            // JJD 9/22/09 - TFS18162 
            // Let the FieldLayout know that it has been added to the collection
            item.OnAddedToCollection();
        }

			#endregion //InsertItem	

			#region RemoveItem

		/// <summary>
		/// Called when an item is removed
		/// </summary>
		/// <param name="index"></param>
		protected override void RemoveItem(int index)
		{
            FieldLayout fl = this[index];

            // JJD 9/21/09 - TFS18162 
            // Call OnRemovedFromCollection so the field layout can remove its template records and cleanup
            // any cached elements
            if (fl != null)
                fl.OnRemovedFromCollection();

            // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping 
            // clear the cached fl if it is being removed
            if (fl == this._typedListRootFieldLayout)
                this._typedListRootFieldLayout = null;

			base.RemoveItem(index);

			// verify the property descriptors when the count goes to zero
			if (this.Count == 0 )
				this.VerifyPropertyDescriptorProviders();
		}

			#endregion //RemoveItem	
    
			#region SetItem

        /// <summary>
        /// Sets an items in the collection replacing what was there.
        /// </summary>
        /// <param name="index">The index where the item should be placed.</param>
        /// <param name="item">Teh item to set</param>
		protected override void SetItem(int index, FieldLayout item)
		{
            FieldLayout fl = this[index];

            // JJD 9/21/09 - TFS18162 
            // Call OnRemovedFromCollection so the field layout can remove its template records and cleanup
            // any cached elements
            if (fl != null)
                fl.OnRemovedFromCollection();

            item.InitializeOwner(this._presenter);

			base.SetItem(index, item);

            // JJD 9/22/09 - TFS18162 
            // Let the FieldLayout know that it has been added to the collection
            item.OnAddedToCollection();
        }

			#endregion //SetItem	

		#endregion //Base class overrides	
    
		#region Properties

			#region Public Properties

                #region DataPresenterBase

        /// <summary>
        /// Returns the DataPresenterBase that owns this collection.
        /// </summary>
        /// <remarks>This property will return null if this fieldlayout is not being used inside a <see cref="Infragistics.Windows.DataPresenter.DataPresenterBase"/>.</remarks>
        // JJD 3/29/08
        // Added attrobutres to prevent datapresenter property from being serialized
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public DataPresenterBase DataPresenter
        {
            get
            {
                return this._presenter;
            }
        }

                #endregion //DataPresenterBase	
    
			#endregion //Public Properties

			#region Internal Properties

				#region TemplateDataRecords





		internal RecordCollectionBase TemplateDataRecords
		{
			get
			{
				if (null == this._templateDataRecords)
					this._templateDataRecords = new RecordCollection(null, this._presenter.RecordManager, null);

				return this._templateDataRecords;
			}
		} 
				#endregion //TemplateDataRecords
    
				// AS 8/25/09 TFS17560
				#region TemplateGroupByRecords





		internal RecordCollectionBase TemplateGroupByRecords
		{
			get
			{
				if (null == this._templateGroupByRecords)
					this._templateGroupByRecords = new RecordCollection(null, this._presenter.RecordManager, null);

				return this._templateGroupByRecords;
			}
		}
				#endregion //TemplateGroupByRecords

			#endregion //Internal Properties

		#endregion //Properties

		#region Indexers

		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="key">The key of the item</param>
		/// <returns>The object in the collection that has the specified key</returns>
		public FieldLayout this[object key]
		{
			get
			{
				// JJD 1/29/07 - BR19603
				// in case anyone calls this indexer with an int delegate to the base int indexer
				if (key is int)
					return this.Items[(int)key];

				int index = this.IndexOfKey(key);

				if (index >= 0)
					return this.Items[index];

				// JJD 6/15/07
				// If the key is a string see if it can be parsed as an int.
				if (key is string && ((string)key).Trim().Length > 0 &&
					int.TryParse((string)key, out index) && index >= 0)
					return this.Items[index];

				throw new ArgumentException(DataPresenterBase.GetString("LE_ArgumentException_14"));
			}
		}

		// JJD 1/29/07 - BR19603
		// We had commented this out because FxCop had a warning about hiding the indexer from the base class.
		// However, this caused a problem because the 'object' inex above was called for int types.
		/// <summary>
		/// Indexer
		/// </summary>
		/// <param name="index">The zero-based index into the collection.</param>
		/// <returns>The object in the collection at the specified index</returns>
		public new FieldLayout this[int index] { get { return this.Items[index]; } }
		

		#endregion Indexers

		#region Methods

			#region Public Methods

                #region Exists

        /// <summary>
		/// Determines if an items with the specified key is in the collection
		/// </summary>
		/// <param name="key">The key of the item to search for</param>
		/// <returns>True if key is found</returns>
		public bool Exists(object key)
		{
			return this.IndexOfKey(key) >= 0;
        }

                #endregion //Exists

                #region IndexOfKey

        /// <summary>
		/// Gets the index of the FieldLayout with the specified key.
		/// </summary>
		/// <param name="key">The key of the item to search for</param>
		/// <returns>The zero-based index or -1 if key not found</returns>
		public int IndexOfKey(object key)
		{
			// JJD 6/15/07
			// Check to make sure the passed in key is not null
			if (key != null)
			{
				int count = this.Count;

				FieldLayout fl;
				for (int i = 0; i < count; i++)
				{
					fl = this.Items[i];

					// JJD 6/15/07
					// Use the Object.Equals method which will do more that just a reference comparison
					// Thios is necessary since we are comparing things declared as type 'object'
					//if (key == fl.Key)
					if (Object.Equals( key, fl.Key))
						return i;
                }
			}

			return -1;
		}

				#endregion //IndexOfKey

			#endregion //Public Methods

			#region Internal Methods

				#region BumpLayoutVersions

		internal void BumpLayoutVersions()
		{
			foreach (FieldLayout fl in this)
				fl.InternalVersion++;
		}

				#endregion //BumpLayoutVersions	

                // JJD 5/5/09 - NA 2009 vol 2 - Cross band grouping - added
				#region BumpSortVersions

		internal void BumpSortVersions()
		{
            foreach (FieldLayout fl in this)
            {
                if (fl.HasSortedFields)
                {
                    if (fl.HasGroupBySortFields)
                        fl.BumpGroupByVersion();
                    else
                        fl.BumpSortVersion();
                }
            }
		}

				#endregion //BumpSortVersions	

                #region GetDefaultFieldLayout

        internal FieldLayout GetDefaultFieldLayout()
        {
            // JJD 5/18/09 - NA 2009 vol 2
            // Get the fieldlayout from the 1st record in the root RecordManager
            // and use that if found.
            if (this._presenter != null)
            {
                FieldLayout fl = this._presenter.RecordManager.FieldLayout;

                if (fl != null)
                    return fl;
            }

            // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping 
            // Return the cached _typedListRootFieldLayout if available
            if (this._typedListRootFieldLayout != null)
                return this._typedListRootFieldLayout;

			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
                FieldLayout fl = this.Items[i];

                if (fl.IsInitialRecordLoaded && fl.ParentFieldLayoutKey == null)
					return fl;
			}

            // JJD 4/17/09 - NA 2009 vol 2 - cross band grouping
            // Loop over the collection again checking the 
            // HasBeenInitializeAfterDataSourceChange flag
			for (int i = 0; i < count; i++)
			{
                FieldLayout fl = this.Items[i];

				if (fl.HasBeenInitializedAfterDataSourceChange)
					return fl;
			}

			return count > 0 ? this.Items[0] : null;
		}

                #endregion //GetDefaultLayoutKey	

                #region GetDefaultLayoutForItem

		// JJD 8/2/07 - Optimization
		// Added overload that takes a PropertyDescriptorProvider
        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
        // Added parentRecordCollection parameter
        //internal FieldLayout GetDefaultLayoutForItem(object item, IEnumerable containingCollection)
        internal FieldLayout GetDefaultLayoutForItem(object item, IEnumerable containingCollection, RecordCollectionBase parentRecordCollection)
		{
			return this.GetDefaultLayoutForItem(item, containingCollection, parentRecordCollection, null);
		}

		// JJD 8/2/07 - Optimization
		// Added overload that takes a PropertyDescriptorProvider
        // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
        // Added parentRecordCollection parameter
        //internal FieldLayout GetDefaultLayoutForItem(object item, IEnumerable containingCollection, PropertyDescriptorProvider propertyDescriptorProvider)
        internal FieldLayout GetDefaultLayoutForItem(object item, IEnumerable containingCollection, RecordCollectionBase parentRecordCollection, PropertyDescriptorProvider propertyDescriptorProvider)
        {
            Debug.Assert(item != null || containingCollection != null);

            if (item == null && containingCollection == null)
                return null;

            // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
            ExpandableFieldRecord parentRecord = parentRecordCollection != null ? parentRecordCollection.ParentRecord as ExpandableFieldRecord : null;
            Field parentField = parentRecord != null ? parentRecord.Field : null;
            FieldLayout parentFieldLayout = parentField != null ? parentField.Owner : null;

            return this.GetDefaultLayoutForItem(item, containingCollection, parentRecordCollection, propertyDescriptorProvider, parentField, parentFieldLayout);
        }

        // JJD 5/11/09 - NA 2009 vol 2 - Cross band grouping
        // Added overload that gets passed in ParentField and ParentFieldLayout
        internal FieldLayout GetDefaultLayoutForItem(object item, IEnumerable containingCollection, RecordCollectionBase parentRecordCollection, PropertyDescriptorProvider propertyDescriptorProvider,
            Field parentField, FieldLayout parentFieldLayout)
		{

			// JJD 8/2/07 - Optimization
			// Only get the PropertyDescriptorProvider if it wasn't passed in
			//PropertyDescriptorProvider propertyDescriptorProvider = GetPropertyDescriptorProvider(item, containingCollection);
			if (propertyDescriptorProvider == null)
			{
				propertyDescriptorProvider = GetPropertyDescriptorProvider(item, containingCollection);

				if (propertyDescriptorProvider == null)
					return null;
			}

			FieldLayout fl;

            // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
            // copy the collection into a stack list we can remoe items from if necessary
			//IList<FieldLayout> items = this.Items;
			// SSP 3/19/10 - Optimizations
			// Allocate a new list only when necessary. Made the necessary change further below
			// to allocate it when we go to modify it.
			// 
			//List<FieldLayout> items = new List<FieldLayout>( this.Items );
			IList<FieldLayout> items = this.Items;
			IList<FieldLayout> thisItems = this.Items;

			int count = items.Count;

            // JJD 4/16/09 - NA 2009 vol 2 - Cross band grouping
            #region Try to match based on parentFieldLayout and Parent Field

            // zeroeth pass:
            //
            // loop over the existing layouts looking for matches on ParentFieldLayout and ParentField
            for (int i = 0; i < count; i++)
            {
                fl = items[i];

                // keep track of whether this fieldlayout is a candidate fro removal
                // from consideration if its ParentFieldLayout and ParentField confict
                bool isCandidateForRemove = false;

                // we can ignore fieldlayouts whose parentFieldLayout doesn't match
                if (!fl.DoesParentFieldLayoutMatch(parentFieldLayout))
                {
                    // If the fieldlayout in the list has its ParentFieldLayout set 
                    // then flag the fieldlayout for possible removal from the list so 
                    // we don't match it below in one of the later passes
                    if (fl.ParentFieldLayoutKey != null)
                        isCandidateForRemove = true;
                    else
                        continue;
                }

                // we can ignore fieldlayouts whose parentField doesn't match
                if (isCandidateForRemove == false &&
                    !fl.DoesParentFieldMatch(parentField))
                {
                    // If the fieldlayout in the list has its ParentFieldName set 
                    // then flag the fieldlayout for possible removal from the list so 
                    // we don't match it below in one of the later passes
                    if (fl.ParentFieldName != null)
                        isCandidateForRemove = true;
                    else
                        continue;
                }

                // If the flag is set walk up the ancestor DataRecord chain to see if
                // this fieldlayout is asscoiated with any of the ancestor records
                if (isCandidateForRemove)
                {
                    bool isFieldLayoutInAncestorChain = false;

                    DataRecord parentDataRecord = parentRecordCollection != null ? parentRecordCollection.ParentDataRecord : null;

                    while (parentDataRecord != null)
                    {
                        if (parentDataRecord.FieldLayout == fl)
                        {
                            isFieldLayoutInAncestorChain = true;
                            break;
                        }

                        parentDataRecord = parentDataRecord.ParentDataRecord;
                    }

                    // If the fieldlayout wasn't in the ancestor record chain
                    // then remove it from the list so we don't match it below 
                    // in one of the later passes.

                    // Note: The reason we allow fieldayouts that exist in the
                    // ancestor chain to be considered below is so we can
                    // support recursive data structures
                    if (!isFieldLayoutInAncestorChain)
                    {
						// SSP 3/19/10 - Optimizations
						// Allocate a new list only when necessary.
						// 
						// --------------------------------------------------------------------------
                        //items.RemoveAt(i);
						// If i is the last item then just decrement the count. Allocating a new list
						// is not necessary.
						// 
						if ( i < count - 1 )
						{
							if ( thisItems == items )
								items = new List<FieldLayout>( items );

							items.RemoveAt( i );
						}
						// --------------------------------------------------------------------------

                        count--;
                        i--;
                    }

                    continue;
                }

                // if this is a child record and we got here then we matched on 
                // ParetnFieldLayout and ParentField.
                // Therefore we can return the field layout as a match
                // but only if the list is compatible
                if (parentField != null &&
                    fl.IsListObjectCompatible(item, containingCollection, false, propertyDescriptorProvider))
                {
                    return fl;
                }
            }

            #endregion //Try to match based on parentFieldLayout and Parent Field	

			// first pass:
			//
			// loop over the existing layouts looking for an exact key match ased on provider
			for (int i = 0; i < count; i++)
			{
				fl = items[i];

				if ( fl.DoesProviderMatchKeyExactly(propertyDescriptorProvider))
					return fl;
			}

			// second pass:
			//
			// loop over the existing layouts looking for string match
			for (int i = 0; i < count; i++)
			{
				fl = items[i];

				// JJD 7/18/07 - BR24617
				// Pass false in as the 3rd param so we may attempt a more exact match if the fl was auto generated
				//if (fl.DoesProviderMatchKeyByName(propertyDescriptorProvider) &&
				//    fl.IsListObjectCompatible(item, containingCollection))
				
				// JJD 8/2/07 - Optimization
				// Added PropertyDescriptorProvider param so we don't have to re-get it 
				//if (fl.DoesProviderMatchKeyByName(propertyDescriptorProvider) &&
				//    fl.IsListObjectCompatible(item, containingCollection, false))
				if (fl.DoesProviderMatchKeyByName(propertyDescriptorProvider) &&
					fl.IsListObjectCompatible(item, containingCollection, false, propertyDescriptorProvider))
					return fl;
			}

			// third pass:
			//
			// loop over the existing layouts looking for the first layout that
			// already has cached this provider for a previous DataRecord
			for (int i = 0; i < count; i++)
			{
				fl = items[i];

				if (fl.IsProviderCached(propertyDescriptorProvider))
					return fl;
			}

			// fourth pass:
			//
			// loop over the existing layouts looking for any compatible layout
			for (int i = 0; i < count; i++)
			{
				fl = items[i];

				// JJD 7/18/07 - BR24617
				// Pass false in as the 3rd param so we may attempt a more exact match if the fl was auto generated
				//if ( fl.IsListObjectCompatible(item, containingCollection))
				// JJD 8/2/07 - Optimization
				// Added PropertyDescriptorProvider param so we don't have to re-get it 
				//if ( fl.IsListObjectCompatible(item, containingCollection, false))

                // JJD 6/25/09 - NA 2009 Vol 2 - Cross band grouping
                // Only match fieldlayouts with null Keys or that have at least 1 field defined
                // This covers the situation where the user defines a FieldLayout in xaml
                // without any fields that is intended to be a child fieldlayout where
                // they specify a Key but no fields. This check will prevent that fieldlayout
                // matching the wrong item but only if KeyMatchingEnforced is set to true
                if (fl.Key == null || fl.KeyMatchingEnforced == false || fl.Fields.Count > 0)
                {
                    if (fl.IsListObjectCompatible(item, containingCollection, false, propertyDescriptorProvider))
                        return fl;
                }
			}

            // since we didn't find it in the collection we need to add it now
            fl = new FieldLayout();

			this.Add(fl);

            return fl;
        }

                #endregion //GetDefaultLayoutForItem	
    
				#region GetPropertyDescriptorProvider

		internal PropertyDescriptorProvider GetPropertyDescriptorProvider(object listObject, IEnumerable containingCollection)
		{
			// create a cacahe on the 1st time thru
			if (this._propertyDescriptorProviders == null)
				this._propertyDescriptorProviders = new List<WeakReference>();
			else
				this.VerifyPropertyDescriptorProviders();

			PropertyDescriptorProvider pdp;

			// check to see if we have already cached the provider
			for (int i = 0; i < this._propertyDescriptorProviders.Count; i++)
			{
				pdp = Utilities.GetWeakReferenceTargetSafe(this._propertyDescriptorProviders[i]) as PropertyDescriptorProvider;

				if (pdp != null)
				{
					if (listObject != null)
					{
						if (pdp.IsCompatibleItem(listObject, containingCollection))
							return pdp;
					}
					else
					{
						if (pdp.IsCompatibleList(containingCollection))
							return pdp;
					}
				}
			}

			// create a holder object to cache the properties
			pdp = PropertyDescriptorProvider.CreateProvider(listObject, containingCollection);

			// JJD 2/22/07 - BR20439
			// check for null
			if (pdp != null)
			{
				// add it to the cache via a WeakReference
				this._propertyDescriptorProviders.Add(new WeakReference(pdp));

                // JJD 5/15/09 - NA 2009 - vol 2 
                // instantiate a listener if the provider raises the propchanged event
                // Note: we don't need/want to keep a reference to this object since it
                // will be kept alive because it wires the provider's PropertyDescriptorsChanged
                // event. It maintains a weak reference to this collection so it won't root
                // anything.
                if (pdp.RaisesPropertyDescriptorsChangedEvent)
                    new PropertyDescriptorsChangedListener(this, pdp);
            }

			return pdp;
		}

				#endregion //GetPropertyDescriptorProvider	

                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
                #region InitializeFieldLayoutStructureFromTypedList

		// JJD 11/24/10 - TFS59031
		// Return the provider so the caller can cache it
        //internal void InitializeFieldLayoutStructureFromTypedList(ITypedList typedList)
		internal FieldLayout.LayoutPropertyDescriptorProvider InitializeFieldLayoutStructureFromTypedList(ITypedList typedList)
        {
            // clear the old member
            this._typedListRootFieldLayout = null;

            if (!(typedList is IEnumerable))
                return null;

            TypedListPropertyDescriptorProvider tlProvider = this.GetPropertyDescriptorProvider(null, typedList as IEnumerable) as TypedListPropertyDescriptorProvider;

            Debug.Assert(tlProvider != null, "The provider should be a TypedListPropertyDescriptorProvider");

            if (tlProvider == null)
                return null;

            TypedListPropertyDescriptorProvider.ProviderTreeNode rootNode = tlProvider.GetProviderTree();

            FieldLayout fl = this.GetDefaultLayoutForItem(null, typedList as IEnumerable, null, tlProvider);

            if (fl != null)
            {
				// AS 3/22/10 TFS29701
				// See comments in RecordManager.OnSourceCollectionReset for details
				//
				//if (!fl.IsInitialized)
				if (!fl.AreFieldsInitialized)
					fl.InitializeFields(null, null, null, tlProvider);
				else
					Debug.Assert(fl.IsInitialized);

                this._typedListRootFieldLayout = fl;

                this.InitializeChildFieldLayouts(rootNode, fl);

				// JJD 11/24/10 - TFS59031
				// Return the provider so the caller can cache it
				return fl.GetProvider(tlProvider);
            }

			return null;
        }

                #endregion //InitializeFieldLayoutStructureFromTypedList	
    
                #region InvalidateGeneratedStyles

		// MD 3/17/11 - TFS34785
		// Added a regenerateTemplates parameter.
        //internal void InvalidateGeneratedStyles(bool bumpVersion)
		internal void InvalidateGeneratedStyles(bool bumpVersion, bool regenerateTemplates)
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				// MD 3/17/11 - TFS34785
				// Pass off the new regenerateTemplates parameter.
                //this.Items[i].InvalidateGeneratedStyles(bumpVersion);
				this.Items[i].InvalidateGeneratedStyles(bumpVersion, regenerateTemplates);
			}
        }

                #endregion //InvalidateGeneratedStyles
    
                #region OnDataSourceChanged

		internal void OnDataSourceChanged()
		{
			int count = this.Count;

			for (int i = 0; i < count; i++)
			{
				this.Items[i].OnDataSourceChanged();
			}
        }

                #endregion //OnDataSourceChanged

				#region OnListMetaDataChanged

		internal void OnListMetaDataChanged(ListChangedEventArgs e, PropertyDescriptorProvider provider)
		{
			if (this.Count > 0)
			{
				foreach (FieldLayout fl in this)
				{
					switch (e.ListChangedType)
					{
						case ListChangedType.PropertyDescriptorAdded:
							fl.OnPropertyDescriptorAdded(e.PropertyDescriptor, provider);
							break;
						case ListChangedType.PropertyDescriptorChanged:
							fl.OnPropertyDescriptorChanged(e.PropertyDescriptor, provider);
							break;
						case ListChangedType.PropertyDescriptorDeleted:
							fl.OnPropertyDescriptorDeleted(e.PropertyDescriptor, provider);
							break;
					}
				}
			}
		}

				#endregion //OnListMetaDataChanged	

                #region ShouldSerialize

		/// <summary>
		/// Determines if any property value is set to a non-default value.
		/// </summary>
		/// <returns>Returns true if any property value is set to a non-default value.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public bool ShouldSerialize()
        {
            // return true if any layout in the collection should be serialized
            foreach (FieldLayout fl in this)
            {
                if (fl.ShouldSerialize())
                    return true;
            }

            return false;
        }

                #endregion //ShouldSerialize	

            #endregion //Internal Methods

			#region Private Methods

                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
                #region InitializeChildFieldLayouts

        private void InitializeChildFieldLayouts(TypedListPropertyDescriptorProvider.ProviderTreeNode node, FieldLayout parentFieldLayout)
        {
            foreach (TypedListPropertyDescriptorProvider.ProviderTreeNode childNode in node.ChildNodes)
            {
                int index = parentFieldLayout.Fields.IndexOf(childNode.PropertyDescriptorProvider.Name);

                Field parentField = index < 0 ? null : parentFieldLayout.Fields[index];

                FieldLayout fl = this.GetDefaultLayoutForItem(null, null, null, childNode.PropertyDescriptorProvider, parentField, parentFieldLayout);

                if (fl != null)
                {
                    // JJD 11/20/09 - TFS24845
                    // Walk up the ancestor chain to ensure this is not a recursive structure
                    bool isInAncestorChain = false;
                    FieldLayout ancestor = parentFieldLayout;
                    while (ancestor != null)
                    {
                        if (fl == ancestor)
                        {
                            isInAncestorChain = true;
                            break;
                        }
                        ancestor = ancestor.ParentFieldLayout;
                    }

                    // JJD 11/20/09 - TFS24845
                    // Only initialize the parent field layout if the field layout wasn't in the ancestor chain
                    if (!isInAncestorChain)
                    {
                        fl.InitializeParentInfo(parentFieldLayout, parentField);

						// AS 3/22/10 TFS29701
						// See comments in RecordManager.OnSourceCollectionReset for details
						//
						//if (!fl.IsInitialized)
                        if (!fl.AreFieldsInitialized)
                            fl.InitializeFields(null, null, null, childNode.PropertyDescriptorProvider);
						else
							Debug.Assert(fl.IsInitialized);

                        this.InitializeChildFieldLayouts(childNode, fl);
                    }
                }

            }
        }

                #endregion //InitializeChildFieldLayouts	

                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
                #region OnProviderPropertyDescriptorsChanged

        private delegate void ProviderPropertyDescriptorsChangedCallback(PropertyDescriptorsChangedEventArgs e);

        private void OnProviderPropertyDescriptorsChanged(PropertyDescriptorsChangedEventArgs e)
        {
            if (this.Count > 0)
            {
                foreach (FieldLayout fl in this)
                {
                    fl.OnProviderPropertyDescriptorsChanged(e);
                }
            }
        }

                #endregion //OnProviderPropertyDescriptorsChanged	
    
				#region VerifyPropertyDescriptorProviders

		// makes sure the weak references are still alive
		private void VerifyPropertyDescriptorProviders()
		{
			if (this._propertyDescriptorProviders == null)
				return;

			// walk over the list backward and remove any weak references
			// that are not alive
			for (int i = this._propertyDescriptorProviders.Count - 1; i >= 0; i--)
			{
				if (!this._propertyDescriptorProviders[i].IsAlive)
					this._propertyDescriptorProviders.RemoveAt(i);
			}

		}

				#endregion //VerifyPropertyDescriptorProviders	
    
   			#endregion //Private Methods	
        
        #endregion //Methods

        // JJD 5/15/09 - NA 2009 - vol 2 - added 
        #region PropertyDescriptorsChangedListener class

        private class PropertyDescriptorsChangedListener
        {
            WeakReference _weakRef;

            internal PropertyDescriptorsChangedListener(FieldLayoutCollection fieldsLayouts, PropertyDescriptorProvider provider)
            {
                this._weakRef = new WeakReference(fieldsLayouts);
                provider.PropertyDescriptorsChanged += new EventHandler<PropertyDescriptorsChangedEventArgs>(OnProviderPropertyDescriptorsChanged);
            }

            private void OnProviderPropertyDescriptorsChanged(object sender, PropertyDescriptorsChangedEventArgs e)
            {
                FieldLayoutCollection fieldLayouts = this._weakRef.Target as FieldLayoutCollection;

                // If the weak ref target is null then unhook from the event and return
                // since noone is holding a ref to this object it will get cleaned up
                if (fieldLayouts == null)
                {
                    e.Provider.PropertyDescriptorsChanged -= new EventHandler<PropertyDescriptorsChangedEventArgs>(OnProviderPropertyDescriptorsChanged);
                    return;
                }

                // let the collection know that things have changed
                if (fieldLayouts._presenter != null)
                {
                    // if we are on the same thread then call the collection's
                    // OnProviderPropertyDescriptorsChanged synchronously.
                    // Otherwise, marshall the caclback onto the dp's thread
                    if ( fieldLayouts._presenter.Dispatcher.CheckAccess() )
                        fieldLayouts.OnProviderPropertyDescriptorsChanged(e);
                    else
                        fieldLayouts._presenter.Dispatcher.BeginInvoke(DispatcherPriority.Send, new ProviderPropertyDescriptorsChangedCallback(fieldLayouts.OnProviderPropertyDescriptorsChanged), new object[] { e } );
                }   
            }
        }

        #endregion //PropertyDescriptorsChangedListener class
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