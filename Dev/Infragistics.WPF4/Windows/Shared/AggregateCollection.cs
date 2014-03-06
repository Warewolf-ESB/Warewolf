using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;


using System.Linq;




#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

using Infragistics.Collections;

namespace Infragistics.Collections

{	
	#region AggregateCollection Class

	internal class AggregateCollection<T> : IList<T>, IList, INotifyCollectionChanged, ISupportPropertyChangeNotifications
	{
		#region Data Structures

		#region AggregateSparseArray Class

		private class AggregateSparseArray : SparseArray
		{
			#region Data Stuctures

			#region MultiItem Class

			internal class MultiItem : ISparseArrayMultiItem
			{
				#region Member Vars

				internal IEnumerable _originalList;
				internal IEnumerable<T> _list;
				private object _ownerData;

				#endregion // Member Vars

				#region Constructor

				internal MultiItem( IEnumerable originalList, IEnumerable<T> list )
				{
					_originalList = originalList;
					_list = list;
				}

				#endregion // Constructor

				#region ProcessHelper

				internal NotifyCollectionChangedEventArgs ProcessHelper( AggregateSparseArray sparseArr, NotifyCollectionChangedEventArgs args )
				{
					NotifyCollectionChangedEventArgs returnValue = null;

					int offset = sparseArr.GetAggregateIndex( this );
					Debug.Assert( offset >= 0 );
					if ( offset >= 0 )
					{
						NotifyCollectionChangedAction action = args.Action;
						switch ( action )
						{
							case NotifyCollectionChangedAction.Add:
								returnValue = CoreUtilities.CreateAddRemoveNCCArgs( true, args.NewItems, offset + args.NewStartingIndex );
								break;
							case NotifyCollectionChangedAction.Remove:
								returnValue = CoreUtilities.CreateAddRemoveNCCArgs( false, args.OldItems, offset + args.OldStartingIndex );
								break;
							case NotifyCollectionChangedAction.Replace:
								returnValue = CoreUtilities.CreateReplaceNCCArgs( args.OldItems, args.NewItems, offset + args.NewStartingIndex );
								break;
						}
					}

					if ( null != returnValue )
						sparseArr.DirtyItemScrollCount( this );
					else
						sparseArr.DirtyScrollCountInfo( );

					return returnValue;
				} 

				#endregion // ProcessHelper

				#region ISparseArrayMultiItem Members

				#region ScrollCount

				public int ScrollCount
				{
					get
					{
						return CoreUtilities.GetCount( _list );
					}
				}

				#endregion // ScrollCount

				#region GetItemAtScrollIndex

				public object GetItemAtScrollIndex( int scrollIndex )
				{
					T item;
                    if ( CoreUtilities.GetItemAt<T>( _list, scrollIndex, out item ) )
						return item;

					return null;
				}

				#endregion // GetItemAtScrollIndex

				#endregion

				#region ISparseArrayItem Members

				#region GetOwnerData

				public object GetOwnerData( SparseArray context )
				{
					return _ownerData;
				}

				#endregion // GetOwnerData

				#region SetOwnerData

				public void SetOwnerData( object ownerData, SparseArray context )
				{
					_ownerData = ownerData;
				}

				#endregion // SetOwnerData

				#endregion
			}

			#endregion // MultiItem Class

			#endregion // Data Stuctures

			#region Constructor

			internal AggregateSparseArray( )
				: base( true, 2, 1f )
			{
			}

			#endregion // Constructor

			#region Methods

			#region Internal Methods

			#region DirtyItemScrollCount

			internal void DirtyItemScrollCount( MultiItem item )
			{
				this.NotifyItemScrollCountChanged( item );
			}

			#endregion // DirtyItemScrollCount

			#region GetAggregateCount

			internal int GetAggregateCount( )
			{
				return this.GetScrollCount( );
			}

			#endregion // GetAggregateCount

			#region GetAggregateIndex

			internal int GetAggregateIndex( MultiItem multiItem )
			{
				return this.GetScrollIndexOf( multiItem, true );
			}

			internal int GetAggregateIndex( T item )
			{
				for ( int i = 0, count = this.Count; i < count; i++ )
				{
					MultiItem ii = (MultiItem)this[i];

					int index = CoreUtilities.GetIndexOf( ii._list, item );
					if ( index >= 0 )
					{
						int listAggregateStartIndex = this.GetAggregateIndex( ii );
						Debug.Assert( listAggregateStartIndex >= 0, "Item is in the sparse array yet its scroll index is -1." );

						if ( listAggregateStartIndex >= 0 )
							return listAggregateStartIndex + index;
					}
				}

				return -1;
			}

			#endregion // GetAggregateIndex

			#region GetCorrespondingMultiItem

			internal MultiItem GetCorrespondingMultiItem( IEnumerable list )
			{
				for ( int i = 0, count = this.Count; i < count; i++ )
				{
					MultiItem ii = (MultiItem)this[i];
					if ( ii._originalList == list || ii._list == list )
						return ii;
				}

				return null;
			}

			#endregion // GetCorrespondingMultiItem

			#region GetItemAtAggregateIndex

			internal bool GetItemAtAggregateIndex( int index, out T item )
			{
				object tmp = this.GetItemAtScrollIndex( index, null );
				if ( tmp is T )
				{
					item = (T)tmp;
					return true;
				}

				item = default( T );
				return false;
			}

			#endregion // GetItemAtAggregateIndex

			#endregion // Internal Methods

			#endregion // Methods
		}

		#endregion // AggregateSparseArray Class

		#endregion // Data Structures

		#region Member Vars

		private IList<IEnumerable> _lists;
		private NotifyCollectionChangedEventHandler _collectionChanged;
		private AggregateSparseArray _sparseArr;
		private PropertyChangeListenerList _propChangeListeners;
		private NotifyCollectionChangedEventHandler _nccHandler;
		private PropertyChangeListener<AggregateCollection<T>> _listener;

		#endregion // Member Vars

		#region Constructor

		internal AggregateCollection( )
			: this( null )
		{
		}

		internal AggregateCollection( IList<IEnumerable> lists )
		{
			_listener = new PropertyChangeListener<AggregateCollection<T>>( this, OnContainedCollectionNotification );
			_nccHandler = new NotifyCollectionChangedEventHandler( OnContainedCollectionChanged );

			this.SetCollections( lists );
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

		#region Methods

		#region Private Methods

		#region HookUnhook

		private void HookUnhook( IList<IEnumerable> lists, bool hook, bool hookUnhookFromSubLists )
		{
			if ( null != lists )
			{
				INotifyCollectionChanged nc = lists as INotifyCollectionChanged;
				if ( null != nc )
				{
					NotifyCollectionChangedEventHandler handler = new NotifyCollectionChangedEventHandler( this.OnCollectionListChanged );

					if ( hook )
						nc.CollectionChanged += handler;
					else
						nc.CollectionChanged -= handler;
				}

				if ( hookUnhookFromSubLists )
				{
					for ( int i = 0, count = lists.Count; i < count; i++ )
						this.HookUnhookSubList( lists[i], hook );
				}
			}
		}

		#endregion // HookUnhook

		#region HookUnhookSubList

		private void HookUnhookSubList( IEnumerable ii, bool hook )
		{
			INotifyCollectionChanged notifyCollection = ii as INotifyCollectionChanged;
			if ( null != notifyCollection )
			{
				if ( hook )
					notifyCollection.CollectionChanged += _nccHandler;
				else
					notifyCollection.CollectionChanged -= _nccHandler;
			}
			else
				Debug.Assert( ii is Array, "Collection doesn't implement INotifyCollectionChanged.");

			ISupportPropertyChangeNotifications pcn = ii as ISupportPropertyChangeNotifications;
			if ( null != pcn )
			{
				if ( hook )
					pcn.AddListener( _listener, false );
				else
					pcn.RemoveListener( _listener );
			}
		}

		#endregion // HookUnhookSubList

		#region InsertMultiItemHelper

		private void InsertMultiItemHelper( int i, bool raiseReset )
		{
			IEnumerable ii = _lists[i];
			IEnumerable<T> iiTyped = ii as IEnumerable<T>;

			if ( null == iiTyped )
			{
				if ( ii is IList )
					iiTyped = new CoreUtilities.TypedList<T>( ii as IList );
				else
					iiTyped = new TypedEnumerable<T>( ii );
			}

			_sparseArr.Insert( i, new AggregateSparseArray.MultiItem( ii, iiTyped ) );
			this.HookUnhookSubList( ii, true );

			if ( raiseReset )
				this.RaiseCollectionReset( );
		}

		#endregion // InsertMultiItemHelper

		#region OnCollectionListChanged

		private void OnCollectionListChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					if ( null != e.NewItems )
					{
						for ( int i = 0, count = e.NewItems.Count; i < count; i++ )
							this.InsertMultiItemHelper( e.NewStartingIndex + i, true );

						return;
					}

					break;
				case NotifyCollectionChangedAction.Remove:
					if ( null != e.OldItems )
					{
						for ( int i = 0, count = e.OldItems.Count; i < count; i++ )
							this.RemoveMultiItemHelper( e.OldStartingIndex + i, e.OldItems[i] as IEnumerable, true );

						return;
					}

					break;
				default:
					break;
			}

			this.SetCollections( _lists );
		}

		#endregion // OnCollectionListChanged

		#region OnContainedCollectionChanged

		private void OnContainedCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			NotifyCollectionChangedEventArgs args = null;

			IEnumerable senderList = sender as IEnumerable;
			AggregateSparseArray.MultiItem multiItem = _sparseArr.GetCorrespondingMultiItem( senderList );
			Debug.Assert( null != multiItem );
			if ( null != multiItem )
				args = multiItem.ProcessHelper( _sparseArr, e );

			if ( null == args )
				args = new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset );

			this.RaiseCollectionChanged( args );
		}

		#endregion // OnContainedCollectionChanged

		#region OnContainedCollectionNotification

		private static void OnContainedCollectionNotification( AggregateCollection<T> owner, object sender, string propName, object extraInfo )
		{
			// Don't pass collection changed notifications from contained collection since we are sending out a collection
			// changed notification from this collection. Any other notifications, including from items of the contained
			// collections should be passed along, which we do further below.
			// 
			IEnumerable senderList = sender as IEnumerable;
			if ( null != senderList )
			{
				AggregateSparseArray.MultiItem multiItem = owner._sparseArr.GetCorrespondingMultiItem( senderList );
				if ( null != multiItem )
					return;
			}

			if ( null != owner._propChangeListeners )
				owner._propChangeListeners.OnPropertyValueChanged( sender, propName, extraInfo );
		}

		#endregion // OnContainedCollectionNotification

		#region RemoveMultiItemHelper

		private void RemoveMultiItemHelper( int i, IEnumerable item, bool raiseReset )
		{
			AggregateSparseArray.MultiItem multiItem = (AggregateSparseArray.MultiItem)_sparseArr[i];
			if ( null != multiItem )
			{
				this.HookUnhookSubList( item, false );
				_sparseArr.RemoveAt( i );

				if ( raiseReset )
					this.RaiseCollectionReset( );
			}
		}

		#endregion // RemoveMultiItemHelper

		#region SetCollections

		private void SetCollections( IList<IEnumerable> lists )
		{
			this.HookUnhook( _lists, false, true );

			_lists = lists;
			int listCount = null != lists ? lists.Count : 0;
			_sparseArr = new AggregateSparseArray( );

			for ( int i = 0; i < listCount; i++ )
				this.InsertMultiItemHelper( i, false );

			this.HookUnhook( _lists, true, false );

			this.RaiseCollectionReset( );
		}

		#endregion // SetCollections

		#endregion // Private Methods

		#region Internal Methods

		#region RaiseCollectionChanged

		internal void RaiseCollectionChanged( NotifyCollectionChangedEventArgs eventArgs )
		{
			if ( null != _collectionChanged )
				_collectionChanged( this, eventArgs );

			if ( null != _propChangeListeners )
				_propChangeListeners.OnCollectionChanged( this, eventArgs );
		}

		#endregion // RaiseCollectionChanged

		#region RaiseCollectionReset

		internal void RaiseCollectionReset( )
		{
			this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
		} 

		#endregion // RaiseCollectionReset

		#endregion // Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Collections

		/// <summary>
		/// Gets or sets the list of collections that constitue this aggregate collection.
		/// </summary>
		public IList<IEnumerable> Collections
		{
			get
			{
				return _lists;
			}
			set
			{
				if ( _lists != value )
				{
					this.SetCollections( value );
				}
			}
		} 

		#endregion // Collections

		#region Count

		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				return _sparseArr.GetAggregateCount( );
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

		#endregion // Public Properties

		#endregion // Properties

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
			return value is T && this.Contains( (T)value );
		}

		int IList.IndexOf( object value )
		{
			return value is T ? this.IndexOf( (T)value ) : -1;
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
			CoreUtilities.CopyTo( this, array, index );
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
				return this;
			}
		}

		#endregion

		#region IList<T> Members

		public int IndexOf( T item )
		{
			return _sparseArr.GetAggregateIndex( item );
		}

		public void Insert( int index, T item )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		void IList<T>.RemoveAt( int index )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		public T this[int index]
		{
			get
			{
				T item;
				if ( _sparseArr.GetItemAtAggregateIndex( index, out item ) )
					return item;

				throw new IndexOutOfRangeException( );
			}
			set
			{
				CoreUtilities.RaiseReadOnlyCollectionException( );
			}
		}

		#endregion

		#region ICollection<T> Members

		public void Add( T item )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		void ICollection<T>.Clear( )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
		}

		public bool Contains( T item )
		{
			return _sparseArr.GetAggregateIndex( item ) >= 0;
		}

		public void CopyTo( T[] array, int arrayIndex )
		{
			CoreUtilities.CopyTo<T>( this, array, arrayIndex );
		}

		public bool Remove( T item )
		{
			CoreUtilities.RaiseReadOnlyCollectionException( );
			return false;
		}

		#endregion

		#region IEnumerable<T> Members

		public IEnumerator<T> GetEnumerator( )
		{


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

			return new CoreUtilities.AggregateEnumerable<T>.Enumerator( 
				from ii in _lists select ( ii is IEnumerable<T> ? (IEnumerable<T>)ii : new TypedEnumerable<T>( ii ) ) );

		}

		#endregion

		#region ISupportPropertyChangeNotifications Implementation

		void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
		{
			if ( null == _propChangeListeners )
				_propChangeListeners = new PropertyChangeListenerList( );

			_propChangeListeners.Add( listener, useWeakReference );
		}

		void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
		{
			if ( null != _propChangeListeners )
				_propChangeListeners.Remove( listener );
		}

		#endregion // ISupportPropertyChangeNotifications Implementation
	}

	#endregion // AggregateCollection Class
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