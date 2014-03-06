using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Collections;
using Infragistics.Collections;
using Infragistics.Controls.Primitives;
using System.ComponentModel;
using System.Diagnostics;

using Infragistics.Windows.Helpers;

using System.Collections.ObjectModel;

namespace Infragistics.Collections
{
	internal interface ILogicalTreeNode
	{

		void AddLogicalChild(object child);
		void RemoveLogicalChild(object child);

	}

	/// <summary>
	/// Collection used by controls that expose both an Items and an itemsSource property. 
	/// </summary>
	internal class ObservableItemCollection : IList, INotifyCollectionChanged, INotifyPropertyChanged
	{
		#region Private Members

		private ILogicalTreeNode _owner;
		private IEnumerable _itemsSource;
		private IEnumerable _itemsSourceUnderlying;
		private ObservableCollectionExtended<object> _nonBoundList;
		private IList _boundList;
		private DataListEventListener _listener;
		private int _updateCount;
		private bool _changeDetectedAfterBeginUpdate;
		private Action<object, object, string> _itemPropChangeHandler;

		private HashSet _beginUpdateSnapshot;
        private Action<object, DataListEventListener, DataListChangeInfo> _propertyDescriptorChangedHandler;

		
		#endregion //Private Members	
 
		#region Constructor

		internal ObservableItemCollection(ILogicalTreeNode owner) : this(owner, null) {}

		// JJD 9/2/11 - Added support for callbacks to be passed into in ctor
		internal ObservableItemCollection(ILogicalTreeNode owner, Action<object, object, string> itemPropChangeHandler)

			: this(owner, itemPropChangeHandler, null) { }
		
		internal ObservableItemCollection(ILogicalTreeNode owner, Action<object, object,string> itemPropChangeHandler, 
			Action<object, DataListEventListener, DataListChangeInfo> propertyDescriptorChangedHandler)

		{
			CoreUtilities.ValidateNotNull(owner, "ObservableItemCollection:ctor owner");
			_owner = owner;
			_itemPropChangeHandler = itemPropChangeHandler;

			if ( _itemPropChangeHandler != null )
				_listener = new DataListEventListener(this, OnSourceListChanged, itemPropChangeHandler, null, true);
			else
				_listener = new DataListEventListener(this, OnSourceListChanged, null, false, false);


			_propertyDescriptorChangedHandler = propertyDescriptorChangedHandler;

		}

		#endregion //Constructor	
    
		#region Properties

		#region Internal Properties

		#region CurrentList

		internal IList CurrentList
		{
			get 
			{
				this.VerifyList();

				if ( _boundList != null )
					return _boundList;

				return _nonBoundList;
			}
		}

		#endregion //CurrentList

		#region ItemsSource

		internal IEnumerable ItemsSource
		{
			get { return this._itemsSource; }
			set 
			{
				if (value != _itemsSource)
				{
					if (_nonBoundList != null && _nonBoundList.Count > 0)
						throw new NotSupportedException(GetString("LE_CantSetItemsSourceWithExistingItems"));

					if ( value == null && _updateCount > 0 )
						throw new NotSupportedException(GetString("LE_CantClearItemsSourceAfterBeginUpdate"));

					_itemsSource = value;

					// JJD 9/28/11 - TFS89531
					// Pass false into returnCollectionViewsSourceCollection 
					// so we don't get the collection view's source returned
					_itemsSourceUnderlying = Infragistics.Windows.Internal.DataBindingUtilities.GetUnderlyingItemSource(_itemsSource, false);



					_listener.List = _itemsSourceUnderlying;

					_boundList = null;

					if (_itemsSourceUnderlying != null)
					{
						_nonBoundList = null;
						_boundList = _itemsSourceUnderlying as IList;

						// If the items source doesn't implement IList then we need to create
						// a wrapper list from IEnumerable
						if (_boundList == null)
							_boundList = CreateWrapperList();
					}

					// raise a reset here since the ItemsSource has changed
					this.RaiseResetEvent();
				}
			}
		}

		#endregion //ItemsSource

		#region ItemsSourceUnderlying

		internal IEnumerable ItemsSourceUnderlying { get { return _itemsSourceUnderlying; } }

		#endregion //ItemsSourceUnderlying	

		#endregion //Internal Properties	

		#region Private Properties

		#endregion //Private Properties

		#endregion //Properties

		#region Methods

		#region PublicMethods

		#region AddRange

		/// <summary>
		/// Adds the elements of a collection to the end of this collection.
		/// </summary>
		/// <param name="collection">The collection whose elements should be inserted into the List. The collection itself cannot be null, but it can contain elements that are null.</param>
		public void AddRange(IEnumerable collection)
		{
			CoreUtilities.ValidateNotNull(collection, "collection");

			this.VerifyNonBoundList();

			IEnumerable<object> objEnum = collection as IEnumerable<object>;

			// if the passed in object doesn't implement IEnumerable<object> then create a list from the enumerable
			if (objEnum == null)
			{
				List<object> list = new List<object>();

				foreach (object o in collection)
				{
					list.Add(o);
				}

				objEnum = list;
			}

			int insertAtIndex = _nonBoundList.Count;

			_nonBoundList.AddRange(objEnum);

			// raise the appropriate change notification
			this.RaiseRangeInsertChangeEvent(true, objEnum, insertAtIndex);
		}

		#endregion //AddRange

		#region BeginUpdate

		/// <summary>
		/// Prevents change notifications (e.g. <see cref="CollectionChanged"/> and <see cref="PropertyChanged"/>) from being raised.
		/// </summary>
		/// <remarks>
		/// <para class="body"> The <see cref="EndUpdate"/> method must be called exactly once for each time that the BeginUpdate method is called.
		/// </para>
		/// </remarks>
		public void BeginUpdate()
		{
			this.VerifyList();
			
			this._updateCount++;

			if (this._itemsSourceUnderlying == null)
			{

				if (_beginUpdateSnapshot == null)
				{
					Debug.Assert(_updateCount == 1, "Update count out of sync");

					_beginUpdateSnapshot = new Windows.Helpers.HashSet();
					_beginUpdateSnapshot.AddItems(_nonBoundList);
				}


				this._nonBoundList.BeginUpdate();
			}
		}

		#endregion //BeginUpdate

		#region EndUpdate

		/// <summary>
		/// Resumes change notifications (e.g. <see cref="CollectionChanged"/> and <see cref="PropertyChanged"/>).
		/// </summary>
		/// <remarks>
		/// <para class="body"> The <see cref="EndUpdate"/> method must be called exactly once for each time that the BeginUpdate method is called.
		/// </para>
		/// </remarks>
		public void EndUpdate()
		{
			this.VerifyList();
			
			this._updateCount--;

			if (this._itemsSourceUnderlying != null)
			{
				if (_updateCount == 0 && _changeDetectedAfterBeginUpdate)
				{
					_changeDetectedAfterBeginUpdate = false;

					// simulate a Reset notification
					this.OnSourceListChanged(this, _listener, new DataListChangeInfo(_listener.List, null));
				}
			}
			else
			{

				if (_updateCount == 0)
				{
					Debug.Assert(_beginUpdateSnapshot != null, "_beginUpdateSnapshot missing");

					if (_beginUpdateSnapshot != null)
					{
						HashSet newHash = new Windows.Helpers.HashSet();
						newHash.AddItems(_nonBoundList);

						HashSet intersection = HashSet.GetIntersection(newHash, _beginUpdateSnapshot);

						if (_beginUpdateSnapshot.Count != intersection.Count)
						{
							foreach (object o in _beginUpdateSnapshot)
							{
								if (!intersection.Exists(o))
									_owner.RemoveLogicalChild(o);
							}
						}

						if (newHash.Count != intersection.Count)
						{
							foreach (object o in newHash)
							{
								if (!intersection.Exists(o))
									_owner.AddLogicalChild(o);
							}
						}
					}

				}

				_beginUpdateSnapshot = null;


				this._nonBoundList.EndUpdate();
			}
		}

		#endregion //EndUpdate

		#region InsertRange

		/// <summary>
		/// Inserts the elements of a collection into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the List. The collection itself cannot be null, but it can contain elements that are null.</param>
		public void InsertRange(int index, IEnumerable collection)
		{
			CoreUtilities.ValidateNotNull(collection, "collection");

			this.VerifyNonBoundList();

			IEnumerable<object> objEnum = collection as IEnumerable<object>;

			if (objEnum == null)
			{
				List<object> list = new List<object>();

				foreach (object o in collection)
					list.Add(o);

				objEnum = list;
			}


			if (_updateCount < 1)
			{
				foreach (object o in objEnum)
					_owner.AddLogicalChild(o);
			}


			_nonBoundList.InsertRange(index, objEnum);

			// raise the appropriate change notification
			this.RaiseRangeInsertChangeEvent(true, objEnum, index);
		}

		#endregion //InsertRange

		#region RemoveRange

		/// <summary>
		///  Removes a contiguous block of items from the collection.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove</param>
		public void RemoveRange(int index, int count)
		{
			this.VerifyNonBoundList();

			List<object> objectsToRemove = null;

			if (_updateCount < 1)
			{
				objectsToRemove = new List<object>();

				int endIndex = Math.Min(Math.Max(index + count - 1, 0), _nonBoundList.Count - 1);

				for (int i = index; i <= endIndex; i++)
					objectsToRemove.Add(_nonBoundList[i]);
			}

			_nonBoundList.RemoveRange(index, count);

			if (objectsToRemove != null)
			{

				foreach (object o in objectsToRemove)
					_owner.RemoveLogicalChild(o);


				// Raise the reset event since SL doesn't support multiple add or remove notifications
				this.RaiseRangeInsertChangeEvent(false, objectsToRemove, index);
			}
		}

		#endregion //RemoveRange

		#endregion //PublicMethods

		#region Internal Methods

		#region TryRemove

		internal bool TryRemove(object item)
		{
			if (item == null)
				return false;

			if (_itemsSource == null)
			{
				if (_nonBoundList != null)
				{
					int index = _nonBoundList.IndexOf(item);

					if (index >= 0)
					{
						_nonBoundList.RemoveAt(index);

						// Raise the appropriate change events
						if (_updateCount < 1)
							this.RaiseChangedEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

						return true;
					}
				}

				return false;
			}

			if (this._itemsSourceUnderlying != null)
			{
				ItemSourceWrapper wrapper = new ItemSourceWrapper(this._itemsSourceUnderlying);

				if (wrapper.CanRemove)
				{
					wrapper.Remove(item);
					return true;
				}
			}

			return false;
		}

		#endregion //TryRemove
    
		#endregion //Internal Methods	
        
		#region Private Methods
		
#region Infragistics Source Cleanup (Region)
























































#endregion // Infragistics Source Cleanup (Region)


		#region CreateWrapperList

		private List<object> CreateWrapperList()
		{
			List<object> list = new List<object>();

			foreach (Object o in _itemsSourceUnderlying)
				list.Add(o);
			return list;
		}

		#endregion //CreateWrapperList	
    
		#region GetString

		internal static string GetString(string name)
		{
#pragma warning disable 436
			return SR.GetString(name);
#pragma warning restore 436
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}

		#endregion //GetString

		#region OnSourceListChanged

		private void OnSourceListChanged(object owner, DataListEventListener listener, DataListChangeInfo changeInfo)
		{

			// JJD 9/2/11 - Added support for callbacks to be passed into in ctor
			switch (changeInfo._changeType)
			{
				case DataListChangeInfo.ChangeType.PropertyDescriptorAdded:
				case DataListChangeInfo.ChangeType.PropertyDescriptorChanged:
				case DataListChangeInfo.ChangeType.PropertyDescriptorRemoved:
					{
						if (_propertyDescriptorChangedHandler != null)
							_propertyDescriptorChangedHandler(owner, listener, changeInfo);

						return;
					}
			}


			// JJD 9/2/11 - Added support for callbacks to be passed into in ctor
			// We can bypass this code if we have an unbound list because the
			// event would have already been raised
			if (_nonBoundList != null)
				return;

			// if there aren't any listeners then we can just return
			if (this.CollectionChanged == null)
				return;

			// If we are between calls of BegingUpdate and EndUpdate just set a flag and return
			if (_updateCount > 0)
			{
				_changeDetectedAfterBeginUpdate = true;
				return;
			}

			NotifyCollectionChangedEventArgs args = null;

			switch (changeInfo._changeType)
			{
				case DataListChangeInfo.ChangeType.Add:
					{
						// If we had to create a wrapper list then we need to update it now
						if (_boundList != null && _boundList != _itemsSourceUnderlying)
						{
							int index = changeInfo._newIndex;
							foreach (Object item in changeInfo.NewItems)
							{
								_boundList.Insert(index, item);
								index++;
							}
						}
						
						
						
						
						
						args = DataListEventListener.CreateAddRemoveNCCArgs( true, changeInfo.NewItems, changeInfo._newIndex );
					}
					break;

				case DataListChangeInfo.ChangeType.Move:
					{
						// If we had to create a wrapper list then we need to update it now
						if (_boundList != null && _boundList != _itemsSourceUnderlying)
						{
							int oldIndex = changeInfo._oldIndex;
							int count = changeInfo.OldItems.Count;

							for (int i = 0; i < count; i++)
								_boundList.RemoveAt(oldIndex);

							int index = changeInfo._newIndex;
							foreach (Object item in changeInfo.NewItems)
							{
								_boundList.Insert(index, item);
								index++;
							}
						}
						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changeInfo.NewItems, changeInfo._newIndex, changeInfo._oldIndex);
					}
					break;

				case DataListChangeInfo.ChangeType.Remove:
					{
						// If we had to create a wrapper list then we need to update it now
						if (_boundList != null && _boundList != _itemsSourceUnderlying)
						{
							int oldIndex = changeInfo._oldIndex;
							int count = changeInfo.OldItems.Count;

							for (int i = 0; i < count; i++)
								_boundList.RemoveAt(oldIndex);
						}
						
						
						
						
						args = DataListEventListener.CreateAddRemoveNCCArgs( false, changeInfo.OldItems, changeInfo._oldIndex );
					}
					break;
				case DataListChangeInfo.ChangeType.Replace:
					{
						// If we had to create a wrapper list then we need to update it now
						if (_boundList != null && _boundList != _itemsSourceUnderlying)
						{
							int index = changeInfo._newIndex;
							foreach (Object item in changeInfo.NewItems)
							{
								_boundList[index] = item;
								index++;
							}
						}
						
						
						
						
						
						// JJD 06/11/12 - TFS113628
						// If _oldItems is NULL then treat this as a reset. The reason is because when a item is replaced
						// in a BindingList<> it sends out a ListChanged event (with ListChangedType.ItemChanged) and 
						// no property descriptor. Unfortunately the event doesn't provide the old item (i.e. the one that 
						// was removed). We can't map this to INotifyCollectionChangedAction 'Replace' because 
						// the NotifyCollectionChangedEventArgs ctor will throw an exception if olditems is null.
						if ( changeInfo._oldItems == null )
							args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
						else
							args = DataListEventListener.CreateReplaceNCCArgs(changeInfo.OldItems, changeInfo.NewItems, changeInfo._newIndex);
					}
					break;

				case DataListChangeInfo.ChangeType.Reset:
					{
						// If we had to create a wrapper list then we need to recreate it now
						if (_boundList != null && _boundList != _itemsSourceUnderlying)
							_boundList = CreateWrapperList();

						args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
					}
					break;
			}

			if (args != null)
				this.RaiseChangedEvents(args);
		}

		#endregion // OnSourceListChanged

		#region RaiseChangedEvents

		private void RaiseChangedEvents(NotifyCollectionChangedEventArgs e)
		{
			if (this.CollectionChanged != null)
				this.CollectionChanged(this, e);

			if (this.PropertyChanged != null)
			{
				this.PropertyChanged(this , new PropertyChangedEventArgs("Count"));
				this.PropertyChanged(this , new PropertyChangedEventArgs("Item[]"));
			}
		}

		#endregion //RaiseChangedEvents

		#region RaiseRangeInsertChangeEvent

		private void RaiseRangeInsertChangeEvent(bool add, IEnumerable<object> objEnum, int insertAtIndex)
		{

			if (_updateCount < 1)
			{
				int itemsProcessed = 0;

				// count the number of items added
				foreach (object o in objEnum)
				{

					if (add)
						_owner.AddLogicalChild(o);
					else
						_owner.RemoveLogicalChild(o);


					itemsProcessed++;
				}

				IList lst = objEnum as IList;

				// create a stack array to hold the items
				object[] items = new object[itemsProcessed];

				if (lst != null)
				{
					// populate the array with the items from the list
					lst.CopyTo(items, 0);
				}
				else
				{
					// populate the array by looping over the enumerator
					int index = 0;
					foreach (object item in objEnum)
					{
						items[index] = item;
						index++;
					}

				}
				
				// raise the appropriate change event
				
				
				
				
				this.RaiseChangedEvents(DataListEventListener.CreateAddRemoveNCCArgs(add, items, insertAtIndex));
			}

		}

		#endregion //RaiseRangeInsertChangeEvent	
    
		#region RaiseResetEvent

		private void RaiseResetEvent()
		{
			this.RaiseChangedEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}

		#endregion //RaiseResetEvent	
    
		#region VerifyList

		private void VerifyList()
		{
			if (_itemsSourceUnderlying != null)
			{
				Debug.Assert(_nonBoundList == null, "We can't have both an unbound list and an item source in ObservableItemCollection.VerifyList()");
				return;
			}

			if (_nonBoundList == null)
			{
				_nonBoundList = new ObservableCollectionExtended<object>();

				// JJD 9/2/11 - Added support for callbacks to be passed into in ctor
				// If a callback is specified then we always have to listen, even if
				// we are using an unbound list
				if (_itemPropChangeHandler != null

					|| _propertyDescriptorChangedHandler != null

					)
				{
					_listener.List = _nonBoundList;
				}
				else
					_listener.List = null;
			}
		}

		#endregion //VerifyList	

		#region VerifyNonBoundList

		private void VerifyNonBoundList()
		{
			if (_itemsSourceUnderlying != null)
				throw new NotSupportedException(GetString("LE_CantMdifyItemsCollectionWhenItemsSourceSet"));

			this.VerifyList();
		}

		#endregion //VerifyNonBoundList

		#endregion //Private Methods

		#endregion //Methods

		#region IList Members

		#region Add

		/// <summary>
		/// Adds an item
		/// </summary>
		/// <param name="value">The object to add</param>
		/// <returns>The position into which the new element was inserted.</returns>
		public int Add(object value)
		{
			this.VerifyNonBoundList();

			int index = ((IList)(this._nonBoundList)).Add(value);

			if (_updateCount < 1)
			{

				_owner.AddLogicalChild(value);

				// Raise the appropriate change event
				this.RaiseChangedEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
			}

			return index;
		}

		#endregion //Add	
    
		#region Clear

		/// <summary>
		/// Removes all items
		/// </summary>
		public void Clear()
		{
			this.VerifyNonBoundList();


			if (_updateCount < 1)
			{
				foreach (object o in _nonBoundList)
					_owner.RemoveLogicalChild(o);
			}


			if (_nonBoundList.Count > 0)
			{
				this._nonBoundList.Clear();

				if (_updateCount < 1)
					this.RaiseResetEvent();
			}
		}

		#endregion //Clear	
    
		#region Contains

		/// <summary>
		/// Determines whether the System.Collections.IList contains a specific value.
		/// </summary>
		/// <param name="value">The object to locate.</param>
		/// <returns>true if the object is found; otherwise, false.</returns>
		public bool Contains(object value)
		{
			return this.CurrentList.Contains(value);
		}

		#endregion //Contains	
    
		#region IndexOf

		/// <summary>
		/// Determines the index of a specific item
		/// </summary>
		/// <param name="value">The object to locate</param>
		/// <returns>The index of value if found in the list; otherwise, -1.</returns>
		public int IndexOf(object value)
		{
			return this.CurrentList.IndexOf(value);
		}

		#endregion //IndexOf	
 
		#region Insert

		/// <summary>
		/// Inserts an item into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The object to insert</param>
		public void Insert(int index, object value)
		{
			this.VerifyNonBoundList();

			this._nonBoundList.Insert(index, value);

			if (_updateCount < 1)
			{

				_owner.AddLogicalChild(value);

				// Raise the appropriate change event
				this.RaiseChangedEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value, index));
			}
		}

		#endregion //Insert	

		#region IsFixedSize

		/// <summary>
		/// Gets a value indicating whether collection has a fixed size.
		/// </summary>
		/// <value>true if the collection has a fixed size; otherwise, false.</value>
		public bool IsFixedSize
		{
			get { return this.CurrentList.IsFixedSize; }
		}

		#endregion //IsFixedSize	

		#region IsReadOnly

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		/// <value>true if the collection is read-only; otherwise, false.</value>
		public bool IsReadOnly
		{
			get { return this.CurrentList.IsReadOnly; }
		}

		#endregion //IsReadOnly	

		#region Remove

		/// <summary>
		/// Removes the first occurrence of a specific object.
		/// </summary>
		/// <param name="item">The object to remove</param>
		public void Remove(object item)
		{
			this.VerifyNonBoundList();

			int index = _nonBoundList.IndexOf(item);

			if ( index  < 0)
				return;

			RemoveHelper(item, index);
		}

		private void RemoveHelper(object item, int index)
		{
			this._nonBoundList.RemoveAt(index);

			if (_updateCount < 1)
			{

				_owner.RemoveLogicalChild(item);

				this.RaiseChangedEvents(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			}
		}

		#endregion //Remove	

		#region RemoveAt

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the item to remove.</param>
		public void RemoveAt(int index)
		{
			this.VerifyNonBoundList();

			object item = _nonBoundList[index];

			this.RemoveHelper(item, index);
		}

		#endregion //RemoveAt	

		#region Indexer

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		public object this[int index]
		{
			get
			{
				return this.CurrentList[index];
			}
			set
			{
				this.VerifyNonBoundList();


				if (_updateCount < 1 && index >= 0 && index < _nonBoundList.Count)
				{
					object oldValue = _nonBoundList[index];

					if (oldValue != value)
					{
						_owner.RemoveLogicalChild(oldValue);
						_owner.AddLogicalChild(value);
					}
				}

				this._nonBoundList[index] = value;
			}
		}

		#endregion //Indexer	
    
		#endregion

		#region ICollection Members

		#region CopyTo

		/// <summary>
		/// Copies the elements of the collection to an System.Array, starting at a particular System.Array index.
		/// </summary>
		/// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection.</param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public void CopyTo(Array array, int index)
		{
			this.CurrentList.CopyTo(array, index);
		}

		#endregion //CopyTo	
    
		#region Count

		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		public int Count
		{
			get { return this.CurrentList.Count; }
		}

		#endregion //Count	
    
		#region IsSynchronized

		/// <summary>
		/// Gets a value indicating whether access to the collection is synchronized (i.e. thread safe).
		/// </summary>
		public bool IsSynchronized
		{
			get { return this.CurrentList.IsSynchronized; }
		}

		#endregion //IsSynchronized	

		#region SyncRoot

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection.
		/// </summary>
		public object SyncRoot
		{
			get { return this.CurrentList.SyncRoot; }
		}

		#endregion //SyncRoot	
    
		#endregion

		#region IEnumerable Members

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>An IEnumerator object that can be used to iterate through the collection.</returns>
		public IEnumerator GetEnumerator()
		{
			return this.CurrentList.GetEnumerator();
		}

		#endregion

		#region INotifyCollectionChanged Members

		/// <summary>
		/// Notifies listeners of dynamic changes, such as when items get added and removed or the whole list is refreshed.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		#endregion

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

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