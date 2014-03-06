using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Threading;






namespace Infragistics.Collections

{
	/// <summary>
	/// An extended <see cref="ObservableCollection&lt;T&gt;"/> that supports adding and removing multiple items at once.
	/// </summary>
	/// <typeparam name="T">The type of item that the collection will contain. If T implements <see cref="INotifyPropertyChanged"/> then the collection will hook the <see cref="INotifyPropertyChanged.PropertyChanged"/> event of all objects added to the collection.</typeparam>

	[Serializable]

	public class ObservableCollectionExtended<T> : ObservableCollection<T>
        // SSP 4/22/10 - XamSchedule
        // 
        , ISupportPropertyChangeNotifications
	{
		#region Member Variables

		private int _suspendCount = 0;
		private bool _hasChanged = false;
		private bool _hookSubObjects = false;
		private PropertyChangedEventHandler _propChangeHandler;

        // SSP 4/22/10 - XamSchedule
        // 
        private PropertyChangeListenerList _propChangeListeners;
        private bool _hookSubObjectsListeners = false;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes a new <see cref="ObservableCollectionExtended&lt;T&gt;"/>
		/// </summary>
		public ObservableCollectionExtended()
            
            
            : this( true, false )
		{
		}

		/// <summary>
		/// Initializes a new <see cref="ObservableCollectionExtended&lt;T&gt;"/>
		/// </summary>
		/// <param name="list">The list from which elements are inserted</param>
		public ObservableCollectionExtended(List<T> list)
			: base(list)
		{
			Debug.Assert(this.Items is List<T>);

            
            
            
			
            this.Initialize( true, false );
		}

        // SSP 4/30/10 - XamSchedule
        // 
        /// <summary>
        /// Initializes a new <see cref="ObservableCollectionExtended&lt;T&gt;"/>
        /// </summary>
        /// <param name="hookItemPropertyChanged">Specifies whether to hook into PropertyChanged notifications of items.</param>
        /// <param name="hookItemListeners">Specifies whether to hook into item listeners.</param>
        internal ObservableCollectionExtended( bool hookItemPropertyChanged, bool hookItemListeners )
            : base( )
        {
            Debug.Assert( this.Items is List<T> );
            this.Initialize( hookItemPropertyChanged, hookItemListeners );
        }

		#endregion //Constructor

		#region Properties

        #region Public Properties

        #region IsUpdating

		/// <summary>
		/// Returns true if the change notifications have been suspended using the <see cref="BeginUpdate"/> method.
		/// </summary>
		/// <seealso cref="EndUpdate"/>
		/// <seealso cref="BeginUpdate"/>
		public bool IsUpdating
		{
			get { return this._suspendCount != 0; }
		}

        #endregion // IsUpdating 

        #endregion // Public Properties

        #region Protected Properties

        #region NotifyItemsChanged

		/// <summary>
		/// Returns a boolean indicating whether the derived collection should have its <see cref="OnItemAdded"/> and <see cref="OnItemRemoved"/> methods invoked.
		/// </summary>
		/// <seealso cref="OnItemRemoved(T)"/>
		/// <seealso cref="OnItemAdded(T)"/>
		/// <seealso cref="OnItemAdding(T)"/>
		protected virtual bool NotifyItemsChanged
		{
			get { return false; }
		}

        #endregion // NotifyItemsChanged 

        #endregion // Protected Properties

        #region Internal Properties

        #region PropChangeListeners

        /// <summary>
        /// List of listeners that will be added as a listener to the contained items.
        /// Also collection change notifications will be raised on the listeners as well.
        /// </summary>
        internal PropertyChangeListenerList PropChangeListeners
        {
            get
            {
                if ( null == _propChangeListeners )
                    _propChangeListeners = new PropertyChangeListenerList( );

                return _propChangeListeners;
            }
        }

        #endregion // PropChangeListeners 

        #endregion // Internal Properties

		#endregion //Properties

		#region Methods

		#region Public

		#region AddRange

		/// <summary>
		/// Adds the elements of a collection to the end of this collection.
		/// </summary>
		/// <param name="collection">The collection whose elements should be inserted into the List. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
		public void AddRange(IEnumerable<T> collection)
		{
			this.InsertRange(this.Count, collection);
		}
		#endregion //AddRange

		#region BeginUpdate
		/// <summary>
		/// Prevents change notifications (e.g. <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> and <see cref="INotifyPropertyChanged.PropertyChanged"/>) from being raised.
		/// </summary>
		/// <remarks>
		/// <p class="note">The <see cref="EndUpdate"/> method must be called exactly once for each time that the <b>BeginUpdate</b> method is called.</p>
		/// </remarks>
		/// <seealso cref="EndUpdate"/>
		/// <seealso cref="IsUpdating"/>
		public void BeginUpdate()
		{
			int count = Interlocked.Increment(ref this._suspendCount);

			if (count == 1)
				this.OnBeginUpdate();
		} 
		#endregion //BeginUpdate

		#region BinarySearch
		/// <summary>
		/// Searches the entire list for the specified element and returns the zero based index.
		/// </summary>
		/// <param name="item">The item to locate</param>
		/// <returns>The zero based index if found; otherwise the bitwise complement of the index of the next larger element.</returns>
		public int BinarySearch(T item)
		{
			return BinarySearch(item, null);
		}

		/// <summary>
		/// Searches the entire list for the specified element using the specified comparer and returns the zero based index.
		/// </summary>
		/// <param name="item">The item to locate</param>
		/// <param name="comparer">The object to use when comparing the elements in the collection</param>
		/// <returns>The zero based index if found; otherwise the bitwise complement of the index of the next larger element.</returns>
		public int BinarySearch(T item, IComparer<T> comparer)
		{
			List<T> list = this.Items as List<T>;

			if (null == list)
			{
				// this is a safety fallback in case ms changes the impl of 
				// Collection<T>'s Items property
				T[] items = new T[this.Count];
				this.CopyTo(items, 0);
				return Array.BinarySearch<T>(items, item, comparer);
			}
			else
				return list.BinarySearch(item, comparer);
		} 
		#endregion //BinarySearch

		#region EndUpdate
		/// <summary>
		/// Resumes change notifications (e.g. <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> and <see cref="INotifyPropertyChanged.PropertyChanged"/>).
		/// </summary>
		/// <remarks>
		/// <p class="note">The <b>EndUpdate</b> method must be called exactly once for each time that the <see cref="BeginUpdate"/> method is called.</p>
		/// </remarks>
		/// <seealso cref="BeginUpdate"/>
		/// <seealso cref="IsUpdating"/>
		public void EndUpdate()
		{
			int newCount = Interlocked.Decrement(ref this._suspendCount);

			Debug.Assert(newCount >= 0, "EndUpdate has been called more times than 'BeginUpdate'!");

			if (newCount == 0)
			{
				if (this._hasChanged)
				{
                    
                    
                    
                    _hasChanged = false;

					this.OnPropertyChanged("Count");
					this.OnPropertyChanged("Item[]");

					this.OnResetCollection();
				}

				this.OnEndUpdate();
			}
		} 
		#endregion //EndUpdate

		#region InsertRange

		/// <summary>
		/// Inserts the elements of a collection into the collection at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which the new elements should be inserted.</param>
		/// <param name="collection">The collection whose elements should be inserted into the List. The collection itself cannot be null, but it can contain elements that are null, if type T is a reference type.</param>
		public virtual void InsertRange(int index, IEnumerable<T> collection)
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			this.CheckReentrancy();

			bool notifyChanges = this.NotifyItemsChanged;

			if (notifyChanges)
			{
				foreach (T item in collection)
					this.OnItemAdding(item);
			}

			foreach (T item in collection)
			{
                // SSP 4/22/10 - XamSchedule
                // Moved the check for _hookSubObjects flag into the HookSubObject method.
                // 
				//if (this._hookSubObjects)
					this.HookSubObject(item);

				// AS 1/30/08
				//this.Items.Add(item);
				this.Items.Insert(index++, item);

				if (notifyChanges)
					this.OnItemAdded(item);
			}

			if (false == this.IsUpdating)
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");

				this.OnResetCollection();
			}
			else
			{
				this._hasChanged = true;
			}
		}
		#endregion //InsertRange

		#region ReInitialize

		/// <summary>
		/// Clears and repopulates the collection with the specified 
		/// </summary>
		/// <param name="collection">The collection of items that should be used to repopulate the collection.</param>
		public void ReInitialize(IEnumerable<T> collection)
		{
			if (null == collection)
				throw new ArgumentNullException("collection");

			this.BeginUpdate();
			this.Clear();
			this.AddRange(collection);
			this.EndUpdate();
		}
		#endregion //ReInitialize

		#region RemoveRange

		/// <summary>
		/// Removes a contiguous block of items from the collection.
		/// </summary>
		/// <param name="index">The zero-based starting index of the range of elements to remove.</param>
		/// <param name="count">The number of elements to remove</param>
		public virtual void RemoveRange(int index, int count)
		{
			if (index < 0)
				throw new ArgumentOutOfRangeException("index");

			if (count < 0)
				throw new ArgumentOutOfRangeException("count");

			this.CheckReentrancy();

			bool notifyChanges = this.NotifyItemsChanged;

			for (int i = index + count - 1; i >= index; i--)
			{
				T item = this[i];

                // SSP 4/22/10 - XamSchedule
                // Moved the check for _hookSubObjects flag into the HookSubObject and UnhookSubObject methods.
                // 
				//if (this._hookSubObjects)
					this.UnhookSubObject(item);

				this.Items.RemoveAt(i);

				if (notifyChanges)
					this.OnItemRemoved(item);
			}

			if (this.IsUpdating == false)
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");

				this.OnResetCollection();
			}
			else 
				this._hasChanged = true;

		}
		#endregion //RemoveRange

		#endregion //Public

		#region Protected

		/// <summary>
		/// Invoked when BeginUpdate is first called and <see cref="IsUpdating"/> becomes true.
		/// </summary>
		protected virtual void OnBeginUpdate()
		{
		}

		/// <summary>
		/// Invoked when EndUpdate is called and <see cref="IsUpdating"/> becomes false.
		/// </summary>
		protected virtual void OnEndUpdate()
		{

		}

		#region OnItemAdded
		/// <summary>
		/// Invoked when an item has been added if the <see cref="NotifyItemsChanged"/> returns true.
		/// </summary>
		/// <param name="itemAdded">The item that was added</param>
		/// <seealso cref="OnItemRemoved(T)"/>
		/// <seealso cref="OnItemAdding(T)"/>
		/// <seealso cref="NotifyItemsChanged"/>
		protected virtual void OnItemAdded(T itemAdded)
		{
		}
		#endregion //OnItemAdded

		#region OnItemAdding
		/// <summary>
		/// Invoked when an item is about to be added if the <see cref="NotifyItemsChanged"/> returns true.
		/// </summary>
		/// <param name="itemAdded">The item that is being added</param>
		/// <seealso cref="OnItemRemoved(T)"/>
		/// <seealso cref="OnItemAdded(T)"/>
		/// <seealso cref="NotifyItemsChanged"/>
		protected virtual void OnItemAdding(T itemAdded)
		{
		}
		#endregion //OnItemAdding

		#region OnItemRemoved
		/// <summary>
		/// Invoked when an item has been removed if the <see cref="NotifyItemsChanged"/> returns true.
		/// </summary>
		/// <param name="itemRemoved">The item that was removed</param>
		/// <seealso cref="OnItemAdded(T)"/>
		/// <seealso cref="OnItemAdding(T)"/>
		/// <seealso cref="NotifyItemsChanged"/>
		protected virtual void OnItemRemoved(T itemRemoved)
		{
		}
		#endregion //OnItemRemoved

		#endregion //Protected

		#region Private

        // SSP 4/22/10 - XamSchedule
        // Apparently in silverlight CheckReentrancy is in-accessible.
        // 







		#region HookSubObject
		private void HookSubObject(T item)
		{
            // SSP 4/22/10 - XamSchedule
            // 
            if ( _hookSubObjectsListeners )
            {
                ISupportPropertyChangeNotifications notifier = item as ISupportPropertyChangeNotifications;
                if ( null != notifier )
                    notifier.AddListener( this.PropChangeListeners, false );
            }
            
            if ( _hookSubObjects )
            {
				INotifyPropertyChanged propChange = item as INotifyPropertyChanged;
	
				if (propChange != null)
					propChange.PropertyChanged += this._propChangeHandler;
			} 
		} 
		#endregion //HookSubObject

		#region Initialize

        // SSP 4/30/10 - XamSchedule
        // Added hookItemPropertyChanged and hookItemListeners parameters.
        // 
		//private void Initialize()
        private void Initialize( bool hookItemPropertyChanged, bool hookItemListeners )
		{
            // SSP 4/22/10 - XamSchedule
            // Added hookItemPropertyChanged and hookItemListeners parameters.
            // 
            // ----------------------------------------------------------------------------------------
			//this._hookSubObjects = typeof(INotifyPropertyChanged).IsAssignableFrom(typeof(T));
            this._hookSubObjects = hookItemPropertyChanged
                && typeof( INotifyPropertyChanged ).IsAssignableFrom( typeof( T ) );

            _hookSubObjectsListeners = hookItemListeners
                && typeof( ISupportPropertyChangeNotifications ).IsAssignableFrom( typeof( T ) );
            // ----------------------------------------------------------------------------------------

            // SSP 4/30/10 - XamSchedule
            // Also call HookSubObject if the new _hookSubObjectsListeners flag is true.
            // 
			//if (this._hookSubObjects)
            if ( this._hookSubObjects || _hookSubObjectsListeners )
			{
                if ( _hookSubObjects )
					this._propChangeHandler = new PropertyChangedEventHandler(this.OnSubObjectChanged);

				// hook subobjects
				foreach (T item in this.Items)
					this.HookSubObject(item);
			}
		}
		#endregion //Initialize

		#region OnCollectionChanged

        //private void OnCollectionChanged(NotifyCollectionChangedAction action, IList<T> items)
        //{
        //    this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, (IList)items));
        //}

		#endregion //OnCollectionChanged

		#region OnPropertyChanged

		private void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}
		#endregion //OnPropertyChanged

		#region OnResetCollection

		private void OnResetCollection()
		{
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
		}
		#endregion //OnResetCollection

		#region OnSubObjectChanged
		private void OnSubObjectChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnItemPropertyChanged(new ItemPropertyChangedEventArgs(sender, e.PropertyName));
		}
		#endregion //OnSubObjectChanged

		#region UnhookSubObject
		private void UnhookSubObject(T item)
		{
            // SSP 4/22/10 - XamSchedule
            // 
            if ( _hookSubObjectsListeners )
            {
                ISupportPropertyChangeNotifications notifier = item as ISupportPropertyChangeNotifications;
                if ( null != notifier )
                {
                    notifier.RemoveListener( this.PropChangeListeners );
                }
            }
            
            if ( _hookSubObjects )
            {
				INotifyPropertyChanged propChange = item as INotifyPropertyChanged;
	
				if (propChange != null)
					propChange.PropertyChanged -= this._propChangeHandler;
			}
		}
		#endregion //HookSubObject

		#endregion //Private

		#endregion //Methods

		#region Events

		#region ItemPropertyChanged

		/// <summary>
		/// Raises the <see cref="ItemPropertyChanged"/> event with the specified arguments.
		/// </summary>
		/// <param name="e">The event arguments for the event to be raised.</param>
		protected virtual void OnItemPropertyChanged(ItemPropertyChangedEventArgs e)
		{
			if (this.ItemPropertyChanged != null)
				this.ItemPropertyChanged(this, e);
		}

		/// <summary>
		/// Raised when an object in the collection raises its <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note:</b> The <b>ItemPropertyChanged</b> event will only be raised if the 
		/// type T implements <see cref="INotifyPropertyChanged"/>.</p>
		/// </remarks>
		public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged; 

		#endregion //ItemPropertyChanged 

		#endregion //Events

		#region Base class overrides

		#region ClearItems
		/// <summary>
		/// Removes all the items from the collection.
		/// </summary>
		protected override void ClearItems()
		{
			this.CheckReentrancy();

			#region UnhookSubObject

			// unhook any objects
            // SSP 4/22/10 - XamSchedule
            // Moved the check for _hookSubObjects flag into the HookSubObject and UnhookSubObject methods.
            // 
            
            if ( _hookSubObjects || _hookSubObjectsListeners )
			{
                IList<T> itemsList = this.Items;
                for ( int i = 0, count = itemsList.Count; i < count; i++ )
                    this.UnhookSubObject( itemsList[i] );
			} 

			#endregion //UnhookSubObject

			#region Prepare for Notify

			// if we're going to notify about when an item is remove then we need
			// to cache the items
			bool notifyChanges = this.NotifyItemsChanged;
			T[] items = notifyChanges ? new T[this.Items.Count] : null;

			if (null != items)
				this.Items.CopyTo(items, 0); 

			#endregion //Prepare for Notify

			this.Items.Clear();

			#region Notify
			if (items != null)
			{
				for (int i = 0; i < items.Length; i++)
					this.OnItemRemoved(items[i]);
			} 
			#endregion //Notify

			if (this.IsUpdating)
			{
				this._hasChanged = true;
			}
			else
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");

				this.OnResetCollection();
			}
		} 
		#endregion //ClearItems

		#region InsertItem
		/// <summary>
		/// Inserts a new item at the specified index in the collection.
		/// </summary>
		/// <param name="index">The index at which to insert the <paramref name="item"/></param>
		/// <param name="item">The object to insert in the collection</param>
		protected override void InsertItem(int index, T item)
		{
			this.CheckReentrancy();

			bool notifyChanges = this.NotifyItemsChanged;

			if (notifyChanges)
				this.OnItemAdding(item);

			this.Items.Insert(index, item);

			// hook subobjects
            // SSP 4/22/10 - XamSchedule
            // Moved the check for _hookSubObjects flag into the HookSubObject method.
            // 
			//if (this._hookSubObjects)
				this.HookSubObject(item);

			if (notifyChanges)
				this.OnItemAdded(item);

			if (this.IsUpdating)
			{
				this._hasChanged = true;
			}
			else
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
			}
		} 
		#endregion //InsertItem

		#region MoveItem



		/// <summary>
		/// Moves an item from one index in the collection to a new location.
		/// </summary>
		/// <param name="oldIndex">The index of the item to relocate</param>
		/// <param name="newIndex">The new index of the item currently located at index <paramref name="oldIndex"/></param>
		protected override void MoveItem(int oldIndex, int newIndex)
		{
			this.CheckReentrancy();

			T item = this.Items[oldIndex];
			this.Items.RemoveAt(oldIndex);
			this.Items.Insert(newIndex, item);

			if (this.IsUpdating)
			{
				this._hasChanged = true;
			}
			else
			{
				this.OnPropertyChanged("Item[]");
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
			}
		} 



		#endregion //MoveItem

		#region OnCollectionChanged
		/// <summary>
		/// Raises the <see cref="ObservableCollection&lt;T&gt;.CollectionChanged"/> event.
		/// </summary>
		/// <param name="e">The arguments providing information about the collection change.</param>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			Debug.Assert(this.IsUpdating == false, "We shouldn't be raising a collection change notification while we are updating!");

			base.OnCollectionChanged(e);

            // SSP 4/22/10 - XamSchedule
            // 
            if ( null != _propChangeListeners )
                _propChangeListeners.OnCollectionChanged( this, e );
		} 
		#endregion //OnCollectionChanged

		#region OnPropertyChanged
		/// <summary>
		/// Raises the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <param name="e">The arguments providing information about the property change.</param>
		protected override void OnPropertyChanged(PropertyChangedEventArgs e)
		{
			Debug.Assert(this.IsUpdating == false, "We shouldn't be raising a property change notification while we are updating!");

			base.OnPropertyChanged(e);

			// SSP 4/22/10 - XamSchedule
			// 
			if ( null != _propChangeListeners )
				_propChangeListeners.OnPropertyValueChanged( this, e.PropertyName, null );
		} 
		#endregion //OnPropertyChanged

		#region RemoveItem
		/// <summary>
		/// Removes an item at the specified index.
		/// </summary>
		/// <param name="index">The index of the item in the collection to be removed.</param>
		protected override void RemoveItem(int index)
		{
			this.CheckReentrancy();

			T item = this.Items[index];

            // SSP 4/22/10 - XamSchedule
            // Moved the check for _hookSubObjects flag into the HookSubObject and UnhookSubObject methods.
            // 
			//if (this._hookSubObjects)
				this.UnhookSubObject(item);

			this.Items.RemoveAt(index);

			if (this.NotifyItemsChanged)
				this.OnItemRemoved(item);

			if (this.IsUpdating)
			{
				this._hasChanged = true;
			}
			else
			{
				this.OnPropertyChanged("Count");
				this.OnPropertyChanged("Item[]");
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
			}
		} 
		#endregion //RemoveItem

		#region SetItem
		/// <summary>
		/// Replaces an item at the specified index in the collection 
		/// </summary>
		/// <param name="index">Index of the item to replace</param>
		/// <param name="item">The item to insert into the collection.</param>
		protected override void SetItem(int index, T item)
		{
			this.CheckReentrancy();

			bool notifyChanges = this.NotifyItemsChanged;

			if (notifyChanges)
				this.OnItemAdding(item);

			T oldItem = this.Items[index];

            // SSP 4/22/10 - XamSchedule
            // Moved the check for _hookSubObjects flag into the HookSubObject and UnhookSubObject methods.
            // 
			//if (this._hookSubObjects)
			//{
				this.UnhookSubObject(oldItem);
				this.HookSubObject(item);
			//}

			this.Items[index] = item;

			if (notifyChanges)
			{
				this.OnItemRemoved(oldItem);
				this.OnItemAdded(item);
			}

			if (this.IsUpdating)
			{
				this._hasChanged = true;
			}
			else
			{
				this.OnPropertyChanged("Item[]");
				// AS 1/8/08 BR29509
				// The order of the items was incorrect. The first item is the new one.
				//
				//this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, oldItem, item, index));
				this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, oldItem, index));
			}
		} 
		#endregion //SetItem

		#endregion //Base class overrides

        #region ISupportPropertyChangeNotifications Implementation

        // SSP 4/22/10 - XamSchedule
        // Implemented ISupportPropertyChangeNotifications interface.
        // 

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            this.PropChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            this.PropChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation
	}

	/// <summary>
	/// Event arguments for a property changed on another object.
	/// </summary>
	public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
	{
		#region Member Variables

		private object _item;

		#endregion //Member Variables

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="ItemPropertyChangedEventArgs"/>
		/// </summary>
		/// <param name="item">The item whose property has changed</param>
		/// <param name="propertyName">The name of the property that has changed</param>
		public ItemPropertyChangedEventArgs(object item, string propertyName)
			: base(propertyName)
		{
			this._item = item;
		} 
		#endregion //Constructor

		#region Properties
		/// <summary>
		/// Returns the item whose property has changed.
		/// </summary>
		/// <see cref="PropertyChangedEventArgs.PropertyName"/>
		public object Item
		{
			get { return this._item; }
		} 
		#endregion //Properties
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