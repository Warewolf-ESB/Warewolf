using System.Collections.ObjectModel;
using Infragistics.Collections;

namespace Infragistics.Controls
{
    /// <summary>
    ///  A collection of selectable items. 
    /// </summary>
    /// <typeparam name="T">An object of type <see cref="ISelectableObject"/></typeparam>
    public abstract class SelectedCollectionBase<T> : CollectionBase<T> where T : ISelectableObject
    {
        #region Members

        private Collection<T> _shiftSelectedItems;

        #endregion // Members

        #region Methods

        #region Public

        #region AddRange

        /// <summary>
        /// Adds the specified collection of items to this collection. 
        /// </summary>
        /// <param name="items">items which should be added</param>
        public void AddRange(SelectedCollectionBase<T> items)
        {
            if (items.Count == 0)
            {
                return;
            }

            SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
            previouslySelectedItems.InternalAddRangeSilently(this);

            foreach (T item in items)
            {
                //if (!item.IsSelected)
                //{
                    this.InternalAddItemSilently(this.Count, item);
                //}
            }

            this.OnSelectionChanged(previouslySelectedItems, this);
            //this.Tree.InvalidateScrollPanel(false);
        }

        #endregion // AddRange

        #endregion // Public

        #region Protected

        #region SelectItem

        /// <summary>
        /// Selects the specified item. 
        /// </summary>
        /// <param name="item">The item being selected</param>
        /// <param name="shiftKey">Whether or not the shift key is down.</param>
        protected internal virtual void SelectItem(T item, bool shiftKey)
        {
            if (shiftKey)
            {
                if (this.ShiftSelectedItems.Count == 0 && this.PivotItem == null)
                {
                    this.PivotItem = item;

                    if (item.IsSelected)
                    {
                        this.InternalRemoveItemSilently(this.IndexOf(item));
                    }

                    this.ShiftSelectedItems.Add(item);
                }
                else
                {
                    if (!item.IsSelected)
                    {
                        this.ShiftSelectedItems.Add(item);
                    }
                    else
                    {
                        this.Items.Remove(item);
                    }
                }
            }
            else
            {
                this.ShiftSelectedItems.Clear();
                this.PivotItem = default(T);
            }

            this.InternalAddItemSilently(this.Count, item);
        }

        #endregion // SelectItem

        #region OnSelectionChanged

        /// <summary>
        /// Called when the Selection collection has changed. 
        /// </summary>
        /// <param name="oldCollection">
        /// The old collection.
        /// </param>
        /// <param name="newCollection">
        /// The new collection.
        /// </param>
        protected abstract void OnSelectionChanged(SelectedCollectionBase<T> oldCollection, SelectedCollectionBase<T> newCollection);

        #endregion // OnSelectionChanged

        #region CreateNewInstance

        /// <summary>
        /// Creates a new instance of this collection.
        /// </summary>
        /// <returns>
        /// The created instance.
        /// </returns>
        protected abstract SelectedCollectionBase<T> CreateNewInstance();

        #endregion // CreateNewInstance

        #endregion // Protected

        #region Internal

        internal void InternalAddItemSilently(int index, T item)
        {
            item.SetSelected(true);
            base.AddItem(index, item);
        }

        internal bool InternalRemoveItemSilently(int index)
        {
            T item = this.Items[index];

            if (this.ShiftSelectedItems.Contains(item))
            {
                this.ShiftSelectedItems.Remove(item);
            }

            item.SetSelected(false);
            return base.RemoveItem(index);
        }

        internal void InternalReplaceItemSilently(int index, T item)
        {
            this.Items[index].SetSelected(false);
            item.SetSelected(true);

            base.ReplaceItem(index, item);
        }

        internal void InternalResetItemsSilently()
        {
            foreach (T item in this.Items)
            {
                item.SetSelected(false);
            }

            this.ShiftSelectedItems.Clear();
            this.PivotItem = default(T);

            base.ResetItems();
        }

        internal void InternalAddRangeSilently(SelectedCollectionBase<T> items)
        {
            foreach (T item in items)
            {
                this.InternalAddItemSilently(this.Count, item);
            }
        }

        #endregion // Internal

        #endregion // Methods

        #region Properties

        #region Protected

        #region ShiftSelectedItems

        /// <summary>
        /// Gets a collection of items that were selected while the shift key was down. 
        /// </summary>
        protected internal Collection<T> ShiftSelectedItems
        {
            get
            {
                return this._shiftSelectedItems ?? (this._shiftSelectedItems = new Collection<T>());
            }
        }

        #endregion // ShiftSelectedItems

        #region PivotItem

        /// <summary>
        /// Gets or sets the item that was first selected when the shift key was pressed down. 
        /// </summary>
        /// <remarks> This is important to know, so that selected ranges can be changed.</remarks>
        protected internal T PivotItem
        {
            get;
            set;
        }

        #endregion // PivotItem

        #endregion // Protected

        #endregion // Properties

        #region Overrides

        #region AddItem
        /// <summary>
        /// Adds the item at the specified index. 
        /// </summary>
        /// <param name="index">The zero-based index of the element to add.</param>
        /// <param name="item">item to be added</param>
        protected override void AddItem(int index, T item)
        {
            if (!item.IsSelected)
            {
                SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
                previouslySelectedItems.InternalAddRangeSilently(this);

                this.InternalAddItemSilently(index, item);
                this.OnSelectionChanged(previouslySelectedItems, this);
            }
        }
        #endregion // AddItem

        #region InsertItem
        /// <summary>
        /// Adds the item at the specified index. 
        /// </summary>
        /// <param name="index">The zero-based index of the element to insert.</param>
        /// <param name="item">item to be inserted</param>
        protected override void InsertItem(int index, T item)
        {
            if (!item.IsSelected)
            {
                SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
                previouslySelectedItems.InternalAddRangeSilently(this);

                this.InternalAddItemSilently(index, item);
                this.OnSelectionChanged(previouslySelectedItems, this);
            }
        }
        #endregion // InsertItem

        #region RemoveItem

        /// <summary>
        /// Removes the item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <returns>true if element was removed succesful</returns>
        protected override bool RemoveItem(int index)
        {
            SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
            previouslySelectedItems.InternalAddRangeSilently(this);

            bool removed = this.InternalRemoveItemSilently(index);

            this.OnSelectionChanged(previouslySelectedItems, this);
            return removed;
        }
        #endregion // RemoveItem

        #region ReplaceItem

        /// <summary>
        /// Replaces the item at the specified index with the specified item.
        /// </summary>
        /// <param name="index">The zero-based index of the element to perlace.</param>
        /// <param name="newItem">item to add</param>
        protected override void ReplaceItem(int index, T newItem)
        {
            SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
            previouslySelectedItems.InternalAddRangeSilently(this);

            this.InternalReplaceItemSilently(index, newItem);
            this.OnSelectionChanged(previouslySelectedItems, this);
        }
        #endregion // ReplaceItem

        #region ResetItems

        /// <summary>
        /// Removes all items from the collection.
        /// </summary>
        protected override void ResetItems()
        {
            SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
            previouslySelectedItems.InternalAddRangeSilently(this);

            this.InternalResetItemsSilently();
            this.OnSelectionChanged(previouslySelectedItems, this);
        }
        #endregion // ResetItems

        #endregion // Overrides
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