using System.Collections.ObjectModel;
using Infragistics.Collections;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	///  A collection of selectable items. 
	/// </summary>
	/// <typeparam propertyName="T">An object of type <see cref="ISelectableObject"/></typeparam>
	public abstract class SelectedCollectionBase<T> : CollectionBase<T> where T: ISelectableObject
	{
		#region Members

		Collection<T> _shiftSelectedItems;
        Collection<T> _nonShiftSelectedItems = new Collection<T>();

		#endregion // Members

		#region Methods

		#region Public

		#region AddRange

		/// <summary>
		/// Adds the specified collection of items to this collection. 
		/// </summary>
		/// <param propertyName="items"></param>
		public void AddRange(SelectedCollectionBase<T> items)
		{
			SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
			previouslySelectedItems.InternalAddRangeSilently(this);

			foreach (T item in items)
			{
				if(!item.IsSelected)
                    this.InternalAddItemSilently(this.Count, item, false);
			}

			if (this.Grid != null)
			{
				this.OnSelectionChanged(previouslySelectedItems, this);
				this.Grid.InvalidateScrollPanel(false);
			}
		}

		#endregion // AddRange

		#endregion // Public

		#region Protected

		#region SelectItem

		/// <summary>
		/// Selects the specified item. 
		/// </summary>
		/// <param propertyName="item">The item being selected</param>
		/// <param propertyName="shiftKey">Whether or not the shift key is down.</param>
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
                        this.ShiftSelectedItems.Add(item);
                    else
                    {
                        this.Items.Remove(item);
                        this._nonShiftSelectedItems.Add(item);
                    }
				}
			}
			else
			{
                foreach (T shiftItem in this.ShiftSelectedItems)
                    this._nonShiftSelectedItems.Add(shiftItem);

				this.ShiftSelectedItems.Clear();
				this.PivotItem = default(T);
			}

            this.InternalAddItemSilently(this.Count, item, shiftKey);
		}

		#endregion // SelectItem

		#region OnSelectionChanged

		/// <summary>
		/// Called when the Selection collection has changed. 
		/// </summary>
		protected abstract void OnSelectionChanged(SelectedCollectionBase<T> oldCollection, SelectedCollectionBase<T> newCollection);

		#endregion // OnSelectionChanged

		#region CreateNewInstance

		/// <summary>
		/// Creates a new instance of this collection.
		/// </summary>
		protected abstract SelectedCollectionBase<T> CreateNewInstance();

		#endregion // CreateNewInstance

		#endregion // Protected

		#region Internal

		internal void InternalAddItemSilently(int index, T item)
		{
			if(this.Grid != null)
				item.SetSelected(true);
			base.AddItem(index, item);
		}

		internal bool InternalRemoveItemSilently(int index)
		{
			T item = this.Items[index];

			if (this.ShiftSelectedItems.Contains(item))
				this.ShiftSelectedItems.Remove(item);

            if (this._nonShiftSelectedItems.Contains(item))
                this._nonShiftSelectedItems.Remove(item);

			if (this.Grid != null)
				item.SetSelected(false);

			return base.RemoveItem(index);
		}

		internal void InternalReplaceItemSilently(int index, T item)
		{
			if (this.Grid != null)
			{
				this.Items[index].SetSelected(false);
				item.SetSelected(true);
			}

			base.ReplaceItem(index, item);
		}

		internal void InternalResetItemsSilently()
		{
			if (this.Grid != null)
			{
				foreach (T item in this.Items)
					item.SetSelected(false);
			}

			this.ShiftSelectedItems.Clear();
            this._nonShiftSelectedItems.Clear();
			this.PivotItem = default(T);

			base.ResetItems();
		}

		internal void InternalAddRangeSilently(SelectedCollectionBase<T> items)
		{
			foreach (T item in items)
			{
                this.InternalAddItemSilently(this.Count, item, false);
			}
		}

        internal void InternalAddItemSilently(int index, T item, bool isShiftSelected)
        {
            this.InternalAddItemSilently(index, item);

            if (!isShiftSelected)
                this._nonShiftSelectedItems.Add(item);
        }

        internal void UnselectShiftSelectedItems()
        {
            int selectionCount = this.Count;
            int count = this.ShiftSelectedItems.Count;
            int diff = selectionCount - count;
            if (diff < count)
            {
                List<T> normalSelection = new List<T>(this._nonShiftSelectedItems);

                this.InternalResetItemsSilently();

                foreach (T item in normalSelection)
                {
                    this.InternalAddItemSilently(this.Count, item, false);
                }
            }
            else
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    int index = this.IndexOf(this.ShiftSelectedItems[i]);
                    this.InternalRemoveItemSilently(index);
                }
            }
        }

		#endregion // Internal

		#endregion // Methods

		#region Properties

		#region Public

		#region Grid
		/// <summary>
		/// Gets a reference to the <see cref="XamGrid"/> that this collection belongs to. 
		/// </summary>
		public XamGrid Grid
		{
			get;
			protected internal set;
		}
		#endregion // Grid

		#endregion // Public

		#region Protected

		#region ShiftSelectedItems

		/// <summary>
		/// Gets a collection of items that were selected while the shift key was down. 
		/// </summary>
		protected internal Collection<T> ShiftSelectedItems
		{
			get
			{
				if (this._shiftSelectedItems == null)
					this._shiftSelectedItems = new Collection<T>();

				return this._shiftSelectedItems;
			}
		}

		#endregion // ShiftSelectedItems

		#region PivotItem

		/// <summary>
		/// Gets the item that was first selected when the shift key was pressed down. 
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
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void AddItem(int index, T item)
		{
			if (!item.IsSelected)
			{
				SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
				previouslySelectedItems.InternalAddRangeSilently(this);

                this.InternalAddItemSilently(index, item, false);

				if (this.Grid != null)
				{
					this.OnSelectionChanged(previouslySelectedItems, this);
					this.Grid.InvalidateScrollPanel(false);
				}
			}
		}
		#endregion // AddItem

		#region InsertItem
		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="item"></param>
		protected override void InsertItem(int index, T item)
		{
			if (!item.IsSelected)
			{
				SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
				previouslySelectedItems.InternalAddRangeSilently(this);

                this.InternalAddItemSilently(index, item, false);

				if (this.Grid != null)
				{
					this.OnSelectionChanged(previouslySelectedItems, this);
					this.Grid.InvalidateScrollPanel(false);
				}
			}
		}
		#endregion // InsertItem

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		protected override bool RemoveItem(int index)
		{
			SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
			previouslySelectedItems.InternalAddRangeSilently(this);

			bool removed = this.InternalRemoveItemSilently(index);

			if (this.Grid != null)
			{
				this.OnSelectionChanged(previouslySelectedItems, this);
				this.Grid.InvalidateScrollPanel(false);
			}

			return removed;
		}
		#endregion // RemoveItem

		#region ReplaceItem

		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <param propertyName="newItem"></param>
		protected override void ReplaceItem(int index, T newItem)
		{
			SelectedCollectionBase<T> previouslySelectedItems = this.CreateNewInstance();
			previouslySelectedItems.InternalAddRangeSilently(this);

			this.InternalReplaceItemSilently(index, newItem);

			if (this.Grid != null)
			{
				this.OnSelectionChanged(previouslySelectedItems, this);
				this.Grid.InvalidateScrollPanel(false);
			}
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

			if (this.Grid != null)
			{
				this.OnSelectionChanged(previouslySelectedItems, this);
				this.Grid.InvalidateScrollPanel(false);
			}
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