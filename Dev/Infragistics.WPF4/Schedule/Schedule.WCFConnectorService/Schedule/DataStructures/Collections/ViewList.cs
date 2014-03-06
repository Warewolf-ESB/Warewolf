using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;


using Infragistics.Services;
using Infragistics.Collections.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Collections.Services






{
	#region ViewList Class

	/// <summary>
	/// Represents a list of view items that are created based on a list of data items.
	/// </summary>
	/// <typeparam name="T">Type of items contained in the collection.</typeparam>
	public class ViewList<T> : IList<T>, IList, INotifyCollectionChanged, ISupportPropertyChangeNotifications
		where T : class
	{
		#region Nested Data Structures

		#region Enumerator Class

		private class Enumerator : IEnumerator<T>
		{
			#region Member Vars

			private ViewList<T> _list;
			private int _index;

			#endregion // Member Vars

			#region Constructor

			internal Enumerator( ViewList<T> list )
			{
				_list = list;
				this.Reset( );
			}

			#endregion // Constructor

			#region Properties

			#region Current

			public T Current
			{
				get
				{
					return _list[_index];
				}
			}

			#endregion // Current

			#endregion // Properties

			#region Methods

			#region Dispose

			public void Dispose( )
			{
			}

			#endregion // Dispose

			#region MoveNext

			public bool MoveNext( )
			{
				return ++_index < _list.Count;
			}

			#endregion // MoveNext

			#region Reset

			public void Reset( )
			{
				_index = -1;
			}

			#endregion // Reset

			#endregion // Methods

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			#endregion
		}

		#endregion // Enumerator Class

		#region SelfViewItemFactory Class

		private class SelfViewItemFactory<TViewItem> : IViewItemFactory<TViewItem>
		{
			public TViewItem CreateViewItem( object dataItem )
			{
				return (TViewItem)dataItem;
			}

			public object GetDataItem( TViewItem viewItem )
			{
				return viewItem;
			}

			public void SetDataItem( TViewItem viewItem, object newDataItem )
			{
			}

			public object GetDataItemComparisonTokenForRecycling( object dataItem )
			{
				return dataItem;
			}
		} 

		#endregion // SelfViewItemFactory Class

		#endregion // Nested Data Structures

		#region Member Vars

		private IEnumerable _source;
        private SparseArray _sparseArray;
		private NotifyCollectionChangedEventHandler _collectionChanged;
        private IViewItemFactory<T> _viewItemsFactory;
		private bool _isDirty;
        private PropertyChangeListenerList _propChangeListeners = new PropertyChangeListenerList( );
		private Func<ViewList<T>, IEnumerable> _preverifyCallback;
		private bool _hookIntoItems;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
        /// Constructor. Initializes a new instance of <see cref="ViewList&lt;T&gt;"/>.
		/// </summary>
        /// <param name="source">List of data items.</param>
        /// <param name="viewItemsFactory">Used for creating and new T instances for each data item in the source and also setting/getting their associated data items.</param>
		/// <param name="hookIntoItems">Specifies whether to hook into items in the list via their ISupportPropertyChangeNotifications implementations.</param>
		/// <param name="preverifyCallback">Called before the view list is verified.</param>
		internal ViewList( IEnumerable source, IViewItemFactory<T> viewItemsFactory, bool hookIntoItems = true, Func<ViewList<T>, IEnumerable> preverifyCallback = null )
		{
            CoreUtilities.ValidateNotNull( viewItemsFactory );

            _viewItemsFactory = viewItemsFactory;
			_preverifyCallback = preverifyCallback;
			_hookIntoItems = hookIntoItems;

            _sparseArray = new SparseArray( 20, 0.25f, false );

			this.SourceItems = source;
		}

		/// <summary>
		/// Constructor. Creates a new instance of <see cref="ViewList&lt;T&gt;"/>.
		/// </summary>
		/// <param name="source">Source items collection. Any modifications made to the source collection
		/// will be reflected by this collection.</param>
		/// <remarks>
		/// <para class="body">
		/// <b>Note</b> that the <i>ViewList&lt;T&gt;</i> is a read-only collection. Any modifications made to 
		/// the source collection are reflected by this collection, assuming the source collection implements 
		/// INotifyCollectionChanged interface.
		/// </para>
		/// </remarks>
		public ViewList( IEnumerable<T> source )
			: this( source, new SelfViewItemFactory<T>( ) )
		{
		}

		#endregion // Constructor

		#region Events

		#region CollectionChanged

		/// <summary>
		/// Raised when the collection's contents change.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				_collectionChanged = Delegate.Combine( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
			}
			remove
			{
				_collectionChanged = Delegate.Remove( _collectionChanged, value ) as NotifyCollectionChangedEventHandler;
			}
		}

		#endregion // CollectionChanged

		#endregion // Events

		#region Indexers

		/// <summary>
		/// Returns the item at the specified index.
		/// </summary>
		/// <param name="index">Index of the item to get.</param>
		/// <returns>Item at the specified index.</returns>
		public T this[int index]
		{
			get
			{
				return this.GetItemAt( index, true );
			}
			set
			{
                CoreUtilities.RaiseReadOnlyCollectionException( );
			}
		}

		#endregion // Indexers

		#region Methods

		#region Protected Methods

		#region OnCollectionChanged

		/// <summary>
		/// Called to raise CollectionChanged notification as well as notify property change listeners added
		/// via ISupportPropertyChangeNotifications interface.
		/// </summary>
		/// <param name="eventArgs">Collection change event args.</param>
		protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs eventArgs )
		{
			if ( null != _collectionChanged )
				_collectionChanged( this, eventArgs );

			if ( null != _propChangeListeners )
				_propChangeListeners.OnCollectionChanged( this, eventArgs );
		}

		#endregion // OnCollectionChanged 

		#endregion // Protected Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Indicates whether the specified item is in the collection.
		/// </summary>
		/// <param name="item">Item to check if it's contained in the collection.</param>
		/// <returns>True if the item exists in the collection, false otherwise.</returns>
		public bool Contains( T item )
		{
			this.EnsureNotDirty( );

			return _sparseArray.Contains( item );
		}

		#endregion // Contains

		#region CopyTo

		/// <summary>
		/// Copies items from this collection to the specified array.
		/// </summary>
		/// <param name="array">Destination array.</param>
		/// <param name="arrayIndex">The location in the destination array where the items will be copied.</param>
		public void CopyTo( T[] array, int arrayIndex )
		{
			this.EnsureNotDirty( );

			for ( int i = 0; i < this.Count; i++ )
				array[arrayIndex++] = this.GetItemAt( i, true );
		}

		#endregion // CopyTo

		#region GetEnumerator

		/// <summary>
		/// Returns the enumerator for enumerating items in the collection.
		/// </summary>
		/// <returns>IEnumerator instance.</returns>
		public IEnumerator<T> GetEnumerator( )
		{
			return new Enumerator( this );
		}

		#endregion // GetEnumerator

		#region IndexOf

		/// <summary>
		/// Returns the index of the specified item in the collection.
		/// </summary>
		/// <param name="item">The item whose index will be returned.</param>
		/// <returns>The index of the item if the item exists in the collection, -1 otherwise.</returns>
		public int IndexOf( T item )
		{
			this.EnsureNotDirty( );

			return _sparseArray.IndexOf( item );
		}

		#endregion // IndexOf

		#endregion // Public Methods

		#region Private Methods

		#region Dirty

        /// <summary>
        /// Marks the collection as needing to be resynced with the underlying source collection.
        /// </summary>
		internal void Dirty( bool discardExistingItems )
		{
			if ( discardExistingItems && _sparseArray.Count > 0 )
			{
				this.HookUnhookItems( new TypedEnumerable<T>( _sparseArray.NonNullItems ), false );
				_sparseArray.Clear( );
			}

			if ( ! _isDirty )
			{
				_isDirty = true;
				this.RaiseResetNotification( );
			}
		}

		#endregion // Dirty

		#region EnsureNotDirty

		private void EnsureNotDirty( )
		{
			if ( _isDirty )
			{
				_isDirty = false;

                // Pass in false for raiseReset parameter because the when the collection was dirtied,
                // we should have raised Reset notification at that point. Raising reset here may cause
                // problems for listeners that are not preparted to receive reset notification while
                // processing a reset notification that might have been sent previously when the 
                // collection was dirtied.
                // 
				this.SyncWithSource( false );
			}
		}

		#endregion // EnsureNotDirty

		#region GetDataList

		private IList GetDataList( )
		{
			return _source as IList;
		}

		#endregion // GetDataList

		#region HookIntoSource

        private void HookIntoSource( IEnumerable previousSource )
        {
            

            INotifyCollectionChanged notifyCollection = previousSource as INotifyCollectionChanged;
            if ( null != notifyCollection )
                notifyCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnSourceCollectionChanged );
            
            notifyCollection = _source as INotifyCollectionChanged;
            if ( null != notifyCollection )
                notifyCollection.CollectionChanged += new NotifyCollectionChangedEventHandler( OnSourceCollectionChanged );
            else if ( null != _source )
                Debug.WriteLine( "Collection doesn't implement INotifyCollectionChanged." );
        }

		#endregion // HookIntoSource

		#region HookUnhookItem

		private void HookUnhookItem( T item, bool hook )
		{
			if ( _hookIntoItems )
			{
				ISupportPropertyChangeNotifications n = item as ISupportPropertyChangeNotifications;
				if ( null != n )
				{
					if ( hook )
						n.AddListener( _propChangeListeners, false );
					else
						n.RemoveListener( _propChangeListeners );
				}
			}
		}

		#endregion // HookUnhookItem

		#region HookUnhookItems

		private void HookUnhookItems( IEnumerable<T> items, bool hook )
		{
			if ( null != items && _hookIntoItems )
			{
				foreach ( T ii in items )
					this.HookUnhookItem( ii, hook );
			}
		}

		#endregion // HookUnhookItems

		#region OnSourceCollectionChanged

		private void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( _isDirty )
				return;

			NotifyCollectionChangedEventArgs args = null;

			IList newItems = null;
			ListSection listSection = null;
			bool processAsReset = false;

			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					{
						newItems = e.NewItems;
						int newItemCount = newItems.Count;

						for ( int i = 0; i < newItemCount; i++ )
							_sparseArray.Insert( e.NewStartingIndex + i, null );

						listSection = new ListSection( this, e.NewStartingIndex, newItemCount );
						args = ScheduleUtilities.CreateAddRemoveNCCArgs( true, listSection, e.NewStartingIndex );
					}
					break;
				// Apparently silverlight doesn't have Move in NotifyCollectionChangedAction enum.
				// 

				case NotifyCollectionChangedAction.Move:
					{
						List<object> tmpItems = new List<object>( );
						int movedItemCount = e.NewItems.Count;

						for ( int i = 0; i < movedItemCount; i++ )
							tmpItems.Add( _sparseArray[e.OldStartingIndex + i] );

						_sparseArray.RemoveRange( e.OldStartingIndex, movedItemCount );
						_sparseArray.InsertRange( e.NewStartingIndex, tmpItems );

						args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Move,
							new ListSection( this, e.OldStartingIndex, movedItemCount ), e.NewStartingIndex, e.OldStartingIndex );
					}
					break;

				case NotifyCollectionChangedAction.Remove:
					{
                        int oldStartingIndex = e.OldStartingIndex;
                        int oldItemsCount = e.OldItems.Count;

                        T[] removedItems = CoreUtilities.GetItemsInRange<T>( _sparseArray, oldStartingIndex, oldItemsCount );
						this.HookUnhookItems( removedItems, false );

						_sparseArray.RemoveRange( oldStartingIndex, oldItemsCount );

						args = ScheduleUtilities.CreateAddRemoveNCCArgs( false, removedItems, oldStartingIndex );
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						// Apparently OldStartingIndex is always -1 in silverlight and NewStartingIndex is the 
						// correct index. Also in WPF, there are overloads of NotifyCollectionChangedEventArgs
						// without indexes which leave both the OldStartingIndex and NewStartingIndex to -1.
						// 
						int index = e.NewStartingIndex;
						if ( index >= 0 )
						{
							newItems = e.NewItems;
							int count = newItems.Count;

							T[] removedItems = CoreUtilities.GetItemsInRange<T>( _sparseArray, index, count );
							this.HookUnhookItems( removedItems, false );

							for ( int i = 0; i < count; i++ )
								_sparseArray[index + i] = null;

							listSection = new ListSection( this, index, count );
							args = ScheduleUtilities.CreateReplaceNCCArgs( removedItems, listSection, index );
						}
						else
							processAsReset = true;
					}
					break;
				case NotifyCollectionChangedAction.Reset:
				default:
					{
						Debug.Assert( NotifyCollectionChangedAction.Reset == e.Action, "Unknown type of notification." );
						processAsReset = true;
					}
					break;
			}

			if ( processAsReset )
				this.Dirty( false );

			// If underlying data list contains T instances, then make sure this list includes them. Otherwise IndexOf operation
			// on this list will fail for such an instance since _sparseArray wound't contain the item.
			// 
			if ( null != newItems && null != listSection && newItems.Count > 0 && newItems[0] is T )
				CoreUtilities.Traverse( listSection );

			if ( null != args )
				this.RaiseCollectionChanged( args );
		}

		#endregion // OnSourceCollectionChanged

		#region SyncWithSource

		private void SyncWithSource( bool raiseReset )
		{
			// Call OnPreVerify which gives the derived classes or the _preverifyCallback
			// to provide a new data list. Note that the 'null' is a valid return value.
			// 
			IEnumerable newDataList = this.OnPreVerify( );
			this.SetSourceItemsHelper( newDataList, false );
			
			SparseArray sparseArray = _sparseArray;
			IEnumerable source = _source;
			IList sourceList = this.GetDataList( );
			IViewItemFactory<T> viewItemFactory = _viewItemsFactory;

			List<T> removedItems = new List<T>( );
			List<T> createdItems = new List<T>( );
            
			bool loadOnDemand = null != sourceList;
			
            
			bool useIndexOfForRecycling = false && loadOnDemand && null != sourceList;
			if ( useIndexOfForRecycling )
			{
                List<T> oldItems = new List<T>( new TypedEnumerable<T>( sparseArray.NonNullItems ) );

                sparseArray.Clear( );
				sparseArray.Expand( sourceList.Count );

				foreach ( T ii in oldItems )
				{
                    object oldDataItem = viewItemFactory.GetDataItem( ii );
                    object oldDataItemToken = null != oldDataItem ? viewItemFactory.GetDataItemComparisonTokenForRecycling( oldDataItem ) : null;

					bool reused = false;

                    // NOTE: It is intentional that we are checking for oldDataItemToken being null but doing IndexOf on oldDataItem.
                    // 
					int listIndex = null != oldDataItemToken ? sourceList.IndexOf( oldDataItem ) : -1;
					if ( listIndex >= 0 && null == sparseArray[listIndex] )
					{
						object newDataItem = sourceList[listIndex];

						if ( object.Equals( oldDataItemToken, viewItemFactory.GetDataItemComparisonTokenForRecycling( newDataItem ) ) )
						{
							sparseArray[listIndex] = ii;
							viewItemFactory.SetDataItem( ii, newDataItem );
							reused = true;
						}
					}

                    if ( ! reused )
                        removedItems.Add( ii );
				}
			}
			else
			{
                // Keep track of old items so we can reuse them if the items source contains the same data items.
                // 
                Dictionary<object, T> oldItems = new Dictionary<object, T>( );

                foreach ( T ii in sparseArray.NonNullItems )
                {
                    object dataItem = viewItemFactory.GetDataItem( ii );
                    object token = null != dataItem ? viewItemFactory.GetDataItemComparisonTokenForRecycling( dataItem ) : null;

                    if ( null != token && !oldItems.ContainsKey( token ) )
                        oldItems[token] = ii;
                    else
                        removedItems.Add( ii );
                }

                sparseArray.Clear( );

                if ( null != source )
                {
                    foreach ( object dataItem in source )
                    {
                        object dataItemToken = viewItemFactory.GetDataItemComparisonTokenForRecycling( dataItem );
                        T viewItem = null;

                        if ( null != dataItemToken && oldItems.TryGetValue( dataItemToken, out viewItem ) )
                        {
                            oldItems.Remove( dataItemToken );
                            viewItemFactory.SetDataItem( viewItem, dataItem );
                        }
                        // AS 5/21/10
                        // Added "dataItem is T" in case we are bound to a collection of the items in which case 
                        // we don't need to defer. If we don't do this then the IndexOf/Contains/etc won't work 
                        // because the items won't be in the sparse array.
                        //
                        else if ( !loadOnDemand || dataItem is T )
                        {
                            viewItem = viewItemFactory.CreateViewItem( dataItem );
                            createdItems.Add( viewItem );
                        }

                        sparseArray.Add( viewItem );
                    }
                }

                removedItems.AddRange( oldItems.Values );
			}

            this.HookUnhookItems( removedItems, false );
            this.HookUnhookItems( createdItems, true );

			if ( raiseReset )
				this.RaiseResetNotification( );
		}

		#endregion // SyncWithSource

		#endregion // Private Methods

		#region Internal Methods

		#region GetItemAt

		internal T GetItemAt( int index, bool allocateIfNecessary )
		{
			this.EnsureNotDirty( );

			T item = (T)_sparseArray[index];

			if ( null == item && allocateIfNecessary )
			{
				IList sourceList = this.GetDataList( );
				Debug.Assert( null != sourceList );
				if ( null != sourceList )
				{
					item = _viewItemsFactory.CreateViewItem( sourceList[index] );
					_sparseArray[index] = item;
					this.HookUnhookItem( item, true );
				}
			}

			return item;
		}

		#endregion // GetItemAt

		#region OnPreVerify

		/// <summary>
		/// If preverifyCallback callback was specified then calls that and returns
		/// the return value of that callback. Otherwise returns the Source data list.
		/// </summary>
		/// <returns>The data list to use.</returns>
		internal virtual IEnumerable OnPreVerify( )
		{
			return null != _preverifyCallback ? _preverifyCallback( this ) : _source;
		}

		#endregion // OnPreVerify

		#region RaiseCollectionChanged

		internal void RaiseCollectionChanged( NotifyCollectionChangedEventArgs eventArgs )
		{
            this.OnCollectionChanged( eventArgs );
		}

		#endregion // RaiseCollectionChanged

		#region RaiseResetNotification

		internal void RaiseResetNotification( )
		{
			this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		}

		#endregion // RaiseResetNotification

		#region SetSourceItemsHelper

		private void SetSourceItemsHelper( IEnumerable value, bool dirty )
		{
			if ( _source != value )
			{
				IEnumerable previousSource = _source;

				_source = value;

				this.HookIntoSource( previousSource );

				if ( dirty )
					this.Dirty( true );
			}
		}

		#endregion // SetSourceItemsHelper

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

        #region AllocatedItems

        /// <summary>
        /// Returns items in the collection that have been allocated so far.
        /// </summary>
        /// <remarks>
        /// Returns items in the collection that have been allocated so far. This differs from GetEnumerator 
        /// in that the GetEnumerator allocates items that haven't been allocated yet and returns all the
        /// items of the collection.
        /// </remarks>
        public IEnumerable<T> AllocatedItems
        {
            get
            {
                // TypedEnumerable handles null parameter.
                // 
                return new TypedEnumerable<T>( null != _sparseArray ? _sparseArray.NonNullItems : null );
            }
        }

        #endregion // AllocatedItems

		#region Count

		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				this.EnsureNotDirty( );

				return _sparseArray.Count;
			}
		}

		#endregion // Count

		#region IsReadOnly

		/// <summary>
		/// Indicates whether the collection is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		#endregion // IsReadOnly

		#region SourceItems

		/// <summary>
		/// Gets or sets the source items collection.
		/// </summary>
		public IEnumerable SourceItems
		{
			get
			{
				return _source;
			}
			set
			{
				this.SetSourceItemsHelper( value, true );
			}
		}

		#endregion // SourceItems

		#endregion // Public Properties

		#region Internal Properties

		#region IsDirty

		internal bool IsDirty
		{
			get
			{
				return _isDirty;
			}
		} 

		#endregion // IsDirty

		#endregion // Internal Properties

		#endregion // Properties

		#region IList<T> Members

		void IList<T>.Insert( int index, T item )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		void IList<T>.RemoveAt( int index )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		#endregion // IList<T> Members

		#region ICollection<T> Members

		void ICollection<T>.Add( T item )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		void ICollection<T>.Clear( )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		bool ICollection<T>.Remove( T item )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
			return false;
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion

		#region IList Members

		int IList.Add( object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
			return -1;
		}

		void IList.Clear( )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		bool IList.Contains( object value )
		{
			return ( null == value || value is T ) && this.Contains( (T)value );
		}

		int IList.IndexOf( object value )
		{
			return ( null == value || value is T ) ? this.IndexOf( (T)value ) : -1;
		}

		void IList.Insert( int index, object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		bool IList.IsFixedSize
		{
			get
			{
				return true;
			}
		}

		void IList.Remove( object value )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		void IList.RemoveAt( int index )
		{
            CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		object IList.this[int index]
		{
			get
			{
				return this[index];
			}
			set
			{
                CoreUtilities.RaiseReadOnlyCollectionException( );
			}
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo( Array array, int index )
		{
			this.EnsureNotDirty( );

			for ( int i = 0; i < this.Count; i++ )
				array.SetValue( this.GetItemAt( i, true ), index++ );
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				return false;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				return _sparseArray.SyncRoot;
			}
		}

		#endregion

        #region ISupportPropertyChangeNotifications Implementation

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            _propChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            _propChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation
	}

	#endregion // ViewList Class
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