using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Infragistics.Collections
{
	/// <summary>
	/// A base collection class that provides hooks for derived classes to override base functionality.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class CollectionBase<T> : ICollectionBase<T>
		, INotifyPropertyChanged	// MD 7/20/11
    {
        #region Members

        Collection<T> _items;
        object _syncRoot;        
        Dictionary<int, T> _unIndexedItems;
        Dictionary<T, int> _unIndexedItemsReverse;

        #endregion // Members

        #region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="CollectionBase&lt;T&gt;"/> class.
		/// </summary>
        public CollectionBase()
        {
			this._items = new Collection<T>();
            this._unIndexedItems = new Dictionary<int, T>();
            this._unIndexedItemsReverse = new Dictionary<T, int>();
        }

        #endregion // Constructor

		#region Methods

		#region Protected Virtual

		#region OnNotifyCollectionChanged
		/// <summary>
		/// Invoked when this collection changes.
		/// </summary>
		/// <param name="args"></param>
		protected virtual void OnNotifyCollectionChanged(NotifyCollectionChangedEventArgs args)
		{
			// MD 7/20/11
			// If any operation would cause the amount of items to potentially change, send out a property change 
			// notification for the Count.
			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Reset:
					this.OnPropertyChanged("Count");
					break;
			}

			// MD 7/20/11
			// All change operations could change the value returned at a specific index, so send out a change notification 
			// for the indexer.
			this.OnPropertyChanged("Item[]");

			if (this.CollectionChanged != null)
				this.CollectionChanged(this, args);
		}
		#endregion // OnNotifyCollectionChanged

		#region GetItem

		/// <summary>
		/// Gets the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected virtual T GetItem(int index)
		{
			return this.ResolveItem(index);
		}
		#endregion // GetItem

		#region AddItem

		/// <summary>
		/// Adds the item at the specified index. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected virtual void AddItem(int index, T item)
		{
			this.AddItemSilently(index, item);			
			this.OnItemAdded(index, item);
		}

		#endregion // AddItem

		#region InsertItem
		/// <summary>
		/// Adds an item to the collection at a given index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected virtual void InsertItem(int index, T item)
		{
			this.AddItemSilently(index, item);
			this.OnItemAdded(index, item);
		}
		#endregion // InsertItem

		#region AddItemSilently

		/// <summary>
		/// Adds the item at the specified index, without triggering any events. 
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected virtual void AddItemSilently(int index, T item)
		{
			if (index > this._items.Count)
            {
                this._unIndexedItems.Add(index, item);
                this._unIndexedItemsReverse.Add(item, index);
            }
			else
				this._items.Insert(index, item);
		}

		#endregion // AddItemSilently

		#region OnItemAdded

		/// <summary>
		/// Invoked when an Item is added at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected virtual void OnItemAdded(int index, T item)
		{
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
		}

		#endregion // OnItemAdded

		#region RemoveItem

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected virtual bool RemoveItem(int index)
		{
			T item = this[index];

			bool val = this.RemoveItemSilently(index);

			if (val)
				this.OnItemRemoved(index, item);

			return val;
		}

		#endregion // RemoveItem

		#region RemoveItemSilently

		/// <summary>
		/// Removes the item at the specified index, without triggering any events. 
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		protected virtual bool RemoveItemSilently(int index)
		{
			bool val = false;
            T item = default(T);
            if (this._unIndexedItems.TryGetValue(index, out item))
            {
                val = true;
                this._unIndexedItems.Remove(index);
                this._unIndexedItemsReverse.Remove(item);
            }
            else
            {
                if(index >= 0 && index < this._items.Count)
                {
                    item = this._items[index];
                    val = this._items.Remove(item);
                }
            }

			return val;
		}

		#endregion // RemoveItemSilently

		#region OnItemRemoved

		/// <summary>
		/// Invoked when an Item is removed from the collection.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		protected virtual void OnItemRemoved(int index, T item)
		{
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));            
		}

		#endregion // OnItemRemoved

		#region ResetItems

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		protected virtual void ResetItems()
		{
			this.ResetItemsSilently();
			this.OnResetItems();
		}

		#endregion // ResetItems

		#region ResetItemsSilently

		/// <summary>
		/// Removes all items from the collection without firing any events.
		/// </summary>
		protected virtual void ResetItemsSilently()
		{
			this._items.Clear();
			this._unIndexedItems.Clear();
            this._unIndexedItemsReverse.Clear();
		}

		#endregion // ResetItemsSilently

		#region OnResetItems

		/// <summary>
		/// Invoked when all items are cleared from a collection.
		/// </summary>
		protected virtual void OnResetItems()
		{
			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion // OnResetItems

		#region ReplaceItem

		/// <summary>
		/// Replaces the item at the specified index with the specified item.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="newItem"></param>
		protected virtual void ReplaceItem(int index, T newItem)
		{
			T oldItem = this.ResolveItem(index);
			if (this._unIndexedItems.ContainsKey(index))
            {
                this._unIndexedItems[index] = newItem;
                this._unIndexedItemsReverse.Remove(oldItem);
                this._unIndexedItemsReverse.Add(newItem, index);
            }
			else if (index < this._items.Count)
				this._items[index] = newItem;
			else
				this.AddItemSilently(index, newItem);

			this.OnNotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
		}

		#endregion // ReplaceItem

		#region GetCount

		/// <summary>
		/// Retrieves the amount of items in the collection.
		/// </summary>
		/// <returns></returns>
		protected virtual int GetCount()
		{
			return this._items.Count;
		}

		#endregion // GetCount

		// MD 7/20/11
		#region OnPropertyChanged

		/// <summary>
		/// Occurs when a property changes.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handler = this.PropertyChanged;

			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion  // OnPropertyChanged

		#endregion // Protected Virtual

		#region Private

		private T ResolveItem(int index)
		{
			if (index > -1 && index < this._items.Count)
				return this._items[index];
			else
			{
                T val;
                if (this._unIndexedItems.TryGetValue(index, out val))
                    return val;
                else 
                    return default(T);
			}
		}

		#endregion // Private

		#endregion // Methods

		#region Properties

		/// <summary>
		/// Gets the unerlying collection used to store all items.
		/// </summary>
		protected virtual Collection<T> Items
		{
			get { return this._items; }
		}

		#endregion // Properties

        #region INotifyCollectionChanged Members

		/// <summary>
		/// Fired when the collection changes.
		/// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region IList<T> Members

		/// <summary>
		/// Gets the index of the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
        public virtual int IndexOf(T item)
        {
            if (item == null)
                return -1;

            int index = -1;
            if (this._unIndexedItemsReverse.TryGetValue(item, out index))
                return index;
            else
                return this._items.IndexOf(item);
        }

		/// <summary>
		/// Inserts the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="item"></param>
		public virtual void Insert(int index, T item)
		{
			this.InsertItem(index, item);
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
        public void RemoveAt(int index)
        {
            this.RemoveItem(index);
        }

		/// <summary>
		/// Gets the item at the specified index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
        public T this[int index]
        {
            get
            {
                return this.GetItem(index);
            }
            set
            {
                this.ReplaceItem(index, value);
            }
        }

        #endregion

        #region ICollection<T> Members

		/// <summary>
		/// Adds the item to the end of the collection.
		/// </summary>
		/// <param name="item"></param>
        public virtual void Add(T item)
        {
            this.AddItem(this.Count, item);
        }

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
        public virtual void Clear()
        {
            this.ResetItems();
        }

		/// <summary>
		/// Determines if the collection contains the specified item.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if the item is in the collection.</returns>
        public bool Contains(T item)
        {
            return (item != null && this._unIndexedItemsReverse.ContainsKey(item)) || this._items.Contains(item);
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
			for(int i = arrayIndex; i < array.Length; i++)
				array[i] = this.GetItem(i);
        }

		/// <summary>
		/// Gets the amount of items in the collection.
		/// </summary>
        public int Count
        {
            get { return this.GetCount(); }
        }

		/// <summary>
		/// Gets whether or not the collection is read only.
		/// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

		/// <summary>
		/// Removes the specified item. 
		/// </summary>
		/// <param name="item"></param>
		/// <returns>True if the item was removed.</returns>
        public bool Remove(T item)
        {			
            return this.RemoveItem(this.IndexOf(item));
        }

        #endregion

        #region IEnumerable<T> Members

		/// <summary>
		/// Gets the <see cref="IEnumerator"/> for the collection.
		/// </summary>
		/// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new CollectionBaseEnumerator<T>(this);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            if (value is T)
            {
                this.Add((T)value);
                return this.Count;
            }
            return -1;
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                IList items = this._items as IList;
                if (items != null)
                    return items.IsFixedSize;
                return false;
            }
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return this[index];
            }
            set
            {
                this[index] = (T)value;
            }
        }

        #endregion

        #region ICollection Members

		/// <summary>
		/// Copies the collection to the specified array, starting at the specified index.
		/// </summary>
		/// <param name="array"></param>
		/// <param name="index"></param>
        public void CopyTo(System.Array array, int index)
        {
			int count = this.GetCount();
			for (int i = index; i < count; i++)
			{
				array.SetValue(this.GetItem(i), i);
			}
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (this._syncRoot == null)
                {
                    ICollection items = this._items as ICollection;
                    if (items != null)
                    {
                        this._syncRoot = items.SyncRoot;
                    }
                    else
                    {
                        Interlocked.CompareExchange<object>(ref this._syncRoot, new object(), null);
                    }
                }
                return this._syncRoot;
            }
        }

        #endregion      

		#region IDisposable Members

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="CollectionBase&lt;T&gt;"/> and optionally
		/// releases the managed resources.
		/// </summary>
		/// <param name="disposing">
		/// true to release both managed and unmanaged resources; 
		/// false to release only unmanaged resources.
		/// </param>
		protected virtual void Dispose(bool disposing)
		{
			
		}

		/// <summary>
		/// Releases the unmanaged and managed resources used by the <see cref="CollectionBase&lt;T&gt;"/>.
		/// </summary>
		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion

		#region ICollectionBase<T> Members

		void ICollectionBase<T>.AddItemSilently(int index, T item)
		{
			this.AddItemSilently(index, item);
		}

		#endregion

		#region ICollectionBase Members

		void ICollectionBase.AddItemSilently(int index, object item)
		{
			this.AddItemSilently(index, (T)item);
		}

		bool ICollectionBase.RemoveItemSilently(int index)
		{
			return this.RemoveItemSilently(index);
		}

		void ICollectionBase.ResetItemsSilently()
		{
			this.ResetItemsSilently();
		}

		#endregion

		// MD 7/20/11
		#region INotifyPropertyChanged Members

		private event PropertyChangedEventHandler PropertyChanged;
		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { this.PropertyChanged += value; }
			remove { this.PropertyChanged -= value; }
		}

		#endregion
	}

    /// <summary>
    /// An <see cref="IEnumerator"/>  for the <see cref="CollectionBase&lt;T&gt;"/> class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollectionBaseEnumerator<T> : IEnumerator<T>, IDisposable
    {
        #region Members

        int _index;
        CollectionBase<T> _collection;

        #endregion // Members

        #region Constructor


        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionBaseEnumerator&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="collection"></param>
        public CollectionBaseEnumerator(CollectionBase<T> collection)
        {
            this._index = 0; 
            this._collection = collection;
        }

        #endregion // Constructor 

        #region IEnumerator Members

        object IEnumerator.Current
        {
            get
            {
                return this._collection[this._index++];
            }
        }

        /// <summary>
        /// Gets the next item in the Enumerator.
        /// </summary>
        /// <returns></returns>
        bool IEnumerator.MoveNext()
        {
            return (this._index < this._collection.Count);
        }

        /// <summary>
        ///  Resets the Enumerator.
        /// </summary>
        void IEnumerator.Reset()
        {
            this._index = 0;
        }

        #endregion

        #region IEnumerator<T> Members

        T IEnumerator<T>.Current
        {
            get
            {
                return this._collection[this._index++];
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes the <see cref="CollectionBaseEnumerator&lt;T&gt;"/>
        /// </summary>
        public void Dispose()
        {

        }

        #endregion // Dispose
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