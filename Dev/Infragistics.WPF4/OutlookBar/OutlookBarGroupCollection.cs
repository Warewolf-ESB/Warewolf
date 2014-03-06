using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Collections.Specialized;
using System.Collections;
using Infragistics.Shared;
using Infragistics.Collections;

namespace Infragistics.Windows.OutlookBar
{
    #region OutlookBarGroupCollection class
    /// <summary>
    /// Represents a modifiable collection of <see cref="OutlookBarGroup"/> objects.
    /// </summary>
	/// <remarks>The <see cref="OutlookBarGroup"/>s in OutlookBarGroupCollection can be accessed either via key or integer index.</remarks>
	/// <seealso cref="OutlookBarGroup"/>
	/// <seealso cref="XamOutlookBar.Groups"/>
	public class OutlookBarGroupCollection : ObservableCollectionExtended<OutlookBarGroup>
    {
        #region Member variables

        private Dictionary<string, OutlookBarGroup> _keysHashtable = 
            new Dictionary<string, OutlookBarGroup>(17);    // holds all keys in the collection

        private XamOutlookBar _xob; // XamOutlookBar wich contains the groups collection
        internal bool CanEdit;
        #endregion //Member variables	

        #region Constructors
        /// <summary>
        /// Initialize a new OutlookBarGroupCollection
        /// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>An instance of this class is automatically created by the <see cref="XamOutlookBar"/>. You do not normally need to create an instance of this class.</p>
		/// </remarks>
		public OutlookBarGroupCollection()
        {
            _xob = null;
        }
        internal OutlookBarGroupCollection(XamOutlookBar control)
        {
            CanEdit = false;
            _xob = control;
        }

        #endregion //Constructors	

        #region Base Class Overrides

        #region InsertItem
        /// <summary>
        /// Inserts a new <see cref="OutlookBarGroup"/> at the specified index in the collection.
        /// </summary>
        /// <param name="index">The index at which to insert the OutlookBarGroup/></param>
        /// <param name="item">The OutlookBarGroup to insert in the collection</param>
        protected override void InsertItem(int index, OutlookBarGroup item)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.InsertItem(index, item);
        }

        #endregion //InsertItem	
         
        #region RemoveItem

        /// <summary>
        /// Removes an  <see cref="OutlookBarGroup"/> at the specified index.
        /// </summary>
        /// <param name="index">The index of the item in the collection to be removed.</param>
        protected override void RemoveItem(int index)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.RemoveItem(index);
        }

        #endregion //RemoveItem	
    
        #region InsertRange

        /// <summary>
        /// Inserts the elements of a collection into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the new elements should be inserted.</param>
        /// <param name="collection">The collection whose elements should be inserted into the List.</param>
        public override void InsertRange(int index, IEnumerable<OutlookBarGroup> collection)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.InsertRange(index, collection);
        }

        #endregion //InsertRange	
    
        #region MoveItem

        /// <summary>
        /// Moves an item from one index in the collection to a new location.
        /// </summary>
        /// <param name="oldIndex">The index of the item to relocate</param>
        /// <param name="newIndex">The new index of the item currently located at index <paramref name="oldIndex"/></param>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.MoveItem(oldIndex, newIndex);
        }

        #endregion //MoveItem	
    
        #region RemoveRange

        /// <summary>
        /// Removes a contiguous block of items from the collection.
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove</param>
        public override void RemoveRange(int index, int count)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.RemoveRange(index, count);
        }

        #endregion //RemoveRange	

        #region SetItem

        /// <summary>
        /// Replaces an item at the specified index in the collection 
        /// </summary>
        /// <param name="index">Index of the item to replace</param>
        /// <param name="item">The item to insert into the collection.</param>
        protected override void SetItem(int index, OutlookBarGroup item)
        {
            CheckIsUsingGroupsSource();  // trows an InvalidOperationException if using GroupsSource
            base.SetItem(index, item);
        }

        #endregion //SetItem	

        #region OnItemPropertyChanged
        /// <summary>
        /// Raises the ItemPropertyChanged event
        /// </summary>
        /// <param name="e">ItemPropertyChangedEventArgs</param>
        protected override void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Key")
            {
                OutlookBarGroup gr = e.Item as OutlookBarGroup;
                foreach (KeyValuePair<string,OutlookBarGroup> de in _keysHashtable)
                {
                    if (de.Value == gr)
                    {
                        _keysHashtable.Remove(de.Key);
                        break;
                    }
                }//end for - remove old value

                AddKey(gr);
            }
            base.OnItemPropertyChanged(e);
        }
        
        #endregion //OnItemPropertyChanged	
    
        #region OnCollectionChanged
        /// <summary>
        /// Raises the CollectionChanged event
        /// </summary>
        /// <param name="e">The argument providing information about the collection change</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems != null)
                    {
                        foreach (OutlookBarGroup gr in e.OldItems)
                            RemoveKey(gr);
                    }//end if- remove old items
                    break;
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Replace:
                    if (e.OldItems != null)
                    {
                        foreach (OutlookBarGroup gr in e.OldItems)
                            RemoveKey(gr);
                    }//end if- remove old items

                    if (e.NewItems != null)
                    {
                        foreach (OutlookBarGroup gr in e.NewItems)
                            AddKey(gr);
                    }// end if- update new items
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ResetKeysHashtable();   // rebuild hashtable
                    break;
            }
            base.OnCollectionChanged(e);
        }

        #endregion //OnCollectionChanged	
    
    
        #endregion //Base Class Overrides	
    
        #region Methods

        #region Public Methods

        /// <summary>
        /// Determines whether OutlookBarGroupCollection contains an <see cref="OutlookBarGroup"/> with the specified key
        /// </summary>
		/// <param name="key">The key if the <see cref="OutlookBarGroup"/> to check</param>
        /// <returns>True if the OutlookBarGroup with the specified key exists, otherwise false</returns>
        public bool ContainsKey(string key)
        {
            return _keysHashtable.ContainsKey(key);
        }

        /// <summary>
		/// Returns the <see cref="OutlookBarGroup"/> with the specified key.
        /// </summary>
        /// <param name="key">The key</param>
		/// <returns>The <see cref="OutlookBarGroup"/> with the specified key or null if an <see cref="OutlookBarGroup"/> with the specified key was not found.</returns>
        public OutlookBarGroup this[string key]
        {
            get
            {
                if (!string.IsNullOrEmpty(key))
                {
                    if (this.ContainsKey(key))
                        return _keysHashtable[key];
                }
                return null;
            }
        }

        #endregion //Public Methods	
    
        #region Private Methods

        void CheckIsUsingGroupsSource()
        {
            if (_xob != null)
                if (_xob.GroupsSource != null && !CanEdit)
                    throw new InvalidOperationException(XamOutlookBar.GetString("LE_CannotModifyBoundGroups"));
        }

        #region Key property supporting methods

        private void AddKey(OutlookBarGroup gr)
        {
            CheckKey(gr);
            if (!string.IsNullOrEmpty(gr.Key))
                _keysHashtable[gr.Key] = gr;
        }

        private void RemoveKey(OutlookBarGroup gr)
        {
            if (!string.IsNullOrEmpty(gr.Key))
                _keysHashtable.Remove(gr.Key);
        }

        private void ResetKeysHashtable()
        {
            _keysHashtable.Clear();
            foreach (OutlookBarGroup gr in this.Items)
                AddKey(gr);
        }

        private void CheckKey(OutlookBarGroup gr)
        {
            if (IsDuplicateKey(gr))
                OnDuplicateKey(gr);
        }

        private void OnDuplicateKey(OutlookBarGroup gr)
        {
			string errMessage = string.Format(XamOutlookBar.GetString("LE_DuplicateKey"), 
                gr.Header, gr.Key);
            gr.Key = "";
            throw new Exception(string.Format(errMessage));
        }

        private bool IsDuplicateKey(OutlookBarGroup gr)
        {
            if (string.IsNullOrEmpty(gr.Key))
                return false;

            if (!_keysHashtable.ContainsKey(gr.Key))
                return false;

            OutlookBarGroup existingGroup = _keysHashtable[gr.Key] as OutlookBarGroup;

            if (existingGroup == null)
                return false;

            return !existingGroup.Equals(gr);
        }

        #endregion //Key property supporting methods	
    
        #endregion //Private Methods


        internal void Refresh()
        {
            NotifyCollectionChangedEventArgs e = 
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(e);
        }

        #endregion //Methods	
    }

    #endregion //OutlookBarGroupCollection class	
    
    #region ReadOnlyOutlookBarGroupCollection

    /// <summary>
    /// A read-only collection of <see cref="OutlookBarGroup"/>s used in the navigation, overflow and context menu areas of the <see cref="XamOutlookBar"/>.
    /// </summary>
	/// <seealso cref="OutlookBarGroup"/>
	/// <seealso cref="XamOutlookBar.OverflowAreaGroups"/>
	/// <seealso cref="XamOutlookBar.NavigationAreaGroups"/>
	/// <seealso cref="XamOutlookBar.ContextMenuGroups"/>
	public class ReadOnlyOutlookBarGroupCollection : ReadOnlyObservableCollection<OutlookBarGroup>
    {
        #region Member Variables
        private OutlookBarGroupCollection _source;
        #endregion //Member Variables

        #region Constructors
        /// <summary>
        /// Creates a readonly Collection of OutlookBarGroup items from source collection
        /// </summary>
        /// <param name="grCollection">Source OutlookBarGroupCollection</param>
        public ReadOnlyOutlookBarGroupCollection(OutlookBarGroupCollection grCollection)
            : base(grCollection)
        {
            _source = grCollection;
        }
        #endregion

        #region Properties

        #region Internal Properties





        internal OutlookBarGroupCollection Source
        {
            get { return _source; }
            //set { _source = value; }
        }

        #endregion //Internal Properties	
    
        #endregion //Properties
    }

    #endregion //ReadOnlyOutlookBarGroupCollection
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