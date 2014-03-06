using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Controls;
using Infragistics.Windows.Internal;
using Infragistics.Windows.DataPresenter.Internal;
using Infragistics.Windows.Reporting;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Editors;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Infragistics.Collections;

// SSP 1/21/09 - NAS9.1 Record Filtering
// Added FilteredDataItemsCollection class.
// 

namespace Infragistics.Windows.DataPresenter
{
	#region FilteredDataItemsCollection Class

	/// <summary>
	/// Collection used by the <see cref="RecordManager.FilteredInDataItems"/> property. Contains date items
	/// associated with filtered in data records.
	/// </summary>
	internal class FilteredDataItemsCollection : IList, ICollectionView, INotifyPropertyChanged
	{
		#region Nested Data Structures

		#region DeferRefreshObject Class

		/// <summary>
		/// Class used by ICollectionView.DeferRefresh implementation.
		/// </summary>
		private class DeferRefreshObject : IDisposable
		{
			private FilteredDataItemsCollection _collection;

			internal DeferRefreshObject( FilteredDataItemsCollection collection )
			{
				_collection = collection;
			}

			public void Dispose( )
			{
				if ( null != _collection )
				{
					_collection.ResumeRefresh( );
					_collection = null;
				}
			}
		}

		#endregion // DeferRefreshObject Class

		#endregion // Nested Data Structures

		#region Member Vars

		private RecordManager _rm;
		private SparseList _list;
		private PropertyValueTracker _dpActiveRecordTracker;
		private PropertyValueTracker _filtersVersionTracker;
		private DataRecordCollection _hooked_sortedRecords;
		private bool _listDirty;
		private int _suspendRefreshCount;
		private int _currentPosition;

		// These cached members are strictly for raising property change notifications
		// by RaiseCurrentPositionRelatedEventsHelper method and may not reflect the correct
		// state of the respective property.
		// 
		private bool _cachedLastIsEmpty;
		private object _cachedLastCurrentItem;
		private int _cachedLastCurrentPosition;
		private bool _cachedLastIsCurrentBeforeFirst;
		private bool _cachedLastIsCurrentAfterLast;

		// Event handlers
		// 
		private NotifyCollectionChangedEventHandler _collectionChangedHandler;
		private event CurrentChangingEventHandler _currentChangingHandler;
		private event EventHandler _currentChangedHandler;
		private event PropertyChangedEventHandler _propertyChangedHandler;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="rm"></param>
		internal FilteredDataItemsCollection( RecordManager rm )
		{
			GridUtilities.ValidateNotNull( rm );
			_rm = rm;
		}

		#endregion // Constructor

		#region Events

		#region CollectionChanged

		/// <summary>
		/// Raised when a property has changed
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged
		{
			add
			{
				bool hadListeners = this.HasListeners;

				_collectionChangedHandler = System.Delegate.Combine( _collectionChangedHandler, value ) as NotifyCollectionChangedEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
			remove
			{
				// get the current HasListeners state
				bool hadListeners = this.HasListeners;

				_collectionChangedHandler = System.Delegate.Remove( _collectionChangedHandler, value ) as NotifyCollectionChangedEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
		}

		#endregion // CollectionChanged

		#region CurrentChanged

		/// <summary>
		/// Raised when a property has changed
		/// </summary>
		public event EventHandler CurrentChanged
		{
			add
			{
				bool hadListeners = this.HasListeners;

				_currentChangedHandler = System.Delegate.Combine( _currentChangedHandler, value ) as EventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );

			}
			remove
			{
				bool hadListeners = this.HasListeners;

				_currentChangedHandler = System.Delegate.Remove( _currentChangedHandler, value ) as EventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
		}

		#endregion // CurrentChanged

		#region CurrentChanging

		/// <summary>
		/// Raised when a property has changed
		/// </summary>
		public event CurrentChangingEventHandler CurrentChanging
		{
			add
			{
				bool hadListeners = this.HasListeners;

				_currentChangingHandler = System.Delegate.Combine( _currentChangingHandler, value ) as CurrentChangingEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
			remove
			{
				bool hadListeners = this.HasListeners;

				_currentChangingHandler = System.Delegate.Remove( _currentChangingHandler, value ) as CurrentChangingEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
		}

		#endregion // CurrentChanging

		#region PropertyChanged

		public event PropertyChangedEventHandler PropertyChanged
		{
			add
			{
				bool hadListeners = this.HasListeners;

				_propertyChangedHandler = System.Delegate.Combine( _propertyChangedHandler, value ) as PropertyChangedEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
			remove
			{
				// get the current HasListeners state
				bool hadListeners = this.HasListeners;

				_propertyChangedHandler = System.Delegate.Remove( _propertyChangedHandler, value ) as PropertyChangedEventHandler;

				// If the HasListeners state has changed e.g. we when from 0 to 1 or 1 to 0 listeners
				// then call OnHasListenersChanged so derived classes can trigger off the state change
				if ( this.HasListeners != hadListeners )
					this.OnHasListenersChanged( );
			}
		}

		#endregion // PropertyChanged

		#endregion // Events

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Indicates if the collection contains the specified item.
		/// </summary>
		/// <param name="item">Item to check if it exits in the collection.</param>
		/// <returns>True if the item exists in the collection, false otherwise.</returns>
		public bool Contains( object item )
		{
			this.Verify( );
			return this.IndexOf( item ) >= 0;
		}

		#endregion // Contains

		#region Indexer[int]

		/// <summary>
		/// Gets item at the specified index.
		/// </summary>
		/// <param name="index">Gets the item at the specified index.</param>
		/// <returns>Item at the sepcified index.</returns>
		public object this[int index]
		{
			get
			{
				this.Verify( );
				return ((DataRecord)_list[index]).DataItem;
			}
			set
			{
				throw new NotSupportedException( );
			}
		}

		#endregion // Indexer[int]

		#region IndexOf

		/// <summary>
		/// Gets the index of the specified item in the collection.
		/// </summary>
		/// <param name="item">Item whose index to get.</param>
		/// <returns>Index of the item in the collection. -1 if the item doesn't exist in the collection.</returns>
		public int IndexOf( object item )
		{
			this.Verify( );

			SparseList list = _list;
			for ( int i = 0, count = list.Count; i < count; i++ )
			{
				DataRecord dr = (DataRecord)list[i];
				// JJD 11/18/11 - TFS79001 
				// Use the GridUtilities.AreEqual helper method instead
				//if ( item == dr.DataItem )
				if ( GridUtilities.AreEqual( item, dr.DataItem ))
					return i;
			}

			return -1;
		}

		#endregion // IndexOf

		#region Refresh

		/// <summary>
		/// Recreates the collection.
		/// </summary>
		public void Refresh( )
		{
			if ( null != _list )
			{
				if ( !this.HasListeners )
				{
					_list = null;
					return;
				}

				_listDirty = true;
				this.Verify( );
			}
		}

		#endregion // Refresh

		#endregion // Public Methods

		#region Private/Internal Methods

		#region AddRemoveRecordHelper

		/// <summary>
		/// Called when a record is added, removed or its filtered out state changes so this 
		/// collection can add/remove the record to itself and raise necessary add/remove notifications.
		/// </summary>
		/// <param name="dataRecord">Data record that was added, removed or whose filtered out state changed.</param>
		internal void AddRemoveRecordHelper( DataRecord dataRecord )
		{
			if ( null == _list )
				return;

			if ( this.IsRefreshSuspended )
			{
				_listDirty = true;
				return;
			}

			bool isFilteredOut = dataRecord.InternalIsFilteredOut_Verify;
			
			SparseArray sortedArr = _rm.Sorted.SparseArray;
			int dataRecordSortIndex = sortedArr.IndexOf( dataRecord );

			bool remove = isFilteredOut || dataRecordSortIndex < 0;

			int oldIndex = _list.IndexOf( dataRecord );
			if ( remove )
			{
				// If a record gets filtered out then remove it from list and send out appropriate
				// collection and property change notifications as well as update CurrentItem, 
				// CurrentPosition and related properties.
				// 
				if ( oldIndex >= 0 )
				{
					// If the item being removed is the current item then we have to raise 
					// current changing/changed notifications.
					// 
					bool raiseCurrentChangeNotifications = _currentPosition == oldIndex;
					if ( raiseCurrentChangeNotifications )
						this.RaiseCurrentChanging( new CurrentChangingEventArgs( false ) );

					_list.RemoveAt( oldIndex );

					// If the current position is after the item being removed, decrement 
					// the current position so it keeps pointing to the current item.
					// 
					if ( _currentPosition > oldIndex )
						_currentPosition--;
					// SSP 4/10/09 TFS16485 TFS16490 - Optimizations
					// If the active record is filtered out, null out the ActiveRecord.
					// 
					else if ( _currentPosition == oldIndex )
						_currentPosition = -1;

					// Raise Remove CollectionChanged notification.
					// 
					NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Remove, dataRecord.DataItem, oldIndex );
					this.RaiseCollectionChanged( eventArgs );

					// If current item changed then raise the CurrentChanged notification.
					// 
					if ( raiseCurrentChangeNotifications )
						this.RaiseCurrentChanged( );
				}
			}
			else
			{
				// If a record gets filtered in then insert it into the list at the correct 
				// location. Also send out appropriate collection and property change 
				// notifications as well as update CurrentItem, CurrentPosition and related 
				// properties.
				// 
				if ( oldIndex < 0 )
				{
					if ( dataRecordSortIndex >= 0 )
					{
						// Find the index at which the record should be inserted in the list. The records
						// in the list should be in the same order as the record manager's sorted records 
						// collection.
						// 
						int si = 0, ei = _list.Count - 1;
						int mi = 0;

						while ( si <= ei )
						{
							mi = ( si + ei ) / 2;

							DataRecord rr = (DataRecord)_list[mi];
							int ii = sortedArr.IndexOf( rr );
							Debug.Assert( ii >= 0 );

							if ( ii < dataRecordSortIndex )
								ei = mi - 1;
							else if ( ii > dataRecordSortIndex )
								si = mi + 1;
							else
							{
								Debug.Assert( false );
								break;
							}
						}

						_list.Insert( mi, dataRecord );

						// Increment the current position to account for inserted record. Note that
						// this will not change the CurrentItem property value and therefore there's 
						// no need to raise CurrentChanging and CurrentChanged notifications. 
						// Furthermore, we will be raising property change notifications for the
						// CurrentPosition and related properties in RaiseCollectionChanged call
						// below.
						// 
						if ( _currentPosition >= mi )
							_currentPosition++;

						NotifyCollectionChangedEventArgs eventArgs = new NotifyCollectionChangedEventArgs(
							NotifyCollectionChangedAction.Add, dataRecord.DataItem, mi );

						this.RaiseCollectionChanged( eventArgs );
					}
				}
			}
		}

		#endregion // AddRemoveRecordHelper

		#region GetPositionAssociatedWithActiveRecord

		/// <summary>
		/// Returns the index in this list of the trivial ancestor of the specified record 
		/// that belongs to this list. Basically used to calculate the CurrentPosition associated
		/// with the active record in the data presenter, taking into account active record being
		/// a descendant record of a record manager that this filtered data items collection is
		/// associated with.
		/// </summary>
		/// <param name="record">Index of this record or its ancestor will be returned.</param>
		/// <returns>If the record or its ancestor is found in this list, the matching index 
		/// will be returend. Otherwise -1 is returned.</returns>
		private int GetPositionAssociatedWithActiveRecord( Record record )
		{
			int newPosition = -1;
			while ( null != record && ( newPosition = _list.IndexOf( record ) ) < 0 )
				record = record.ParentDataRecord;

			return newPosition;
		}

		#endregion // GetPositionAssociatedWithActiveRecord

		#region HookUnhookRecordManager

		/// <summary>
		/// Hookes into the record manager's Sorted record collection's CollectionChanged,
		/// data presenter's ActiveRecord property change notification and filter version number
		/// so this collection can be kept in sync.
		/// </summary>
		/// <param name="unhook">Whether to hook or unhook into notifications.</param>
		private void HookUnhookRecordManager( bool unhook )
		{
			DataPresenterBase dp = _rm.DataPresenter;
			DataRecordCollection sortedRecords = _rm.Sorted;

			if ( unhook || _hooked_sortedRecords != sortedRecords )
			{
				if ( null != _hooked_sortedRecords )
				{
					_hooked_sortedRecords.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnSortedRecords_CollectionChanged );
					_hooked_sortedRecords = null;
				}
			}

			if ( !unhook && _hooked_sortedRecords != sortedRecords )
			{
				if ( null != sortedRecords )
					sortedRecords.CollectionChanged += new NotifyCollectionChangedEventHandler( OnSortedRecords_CollectionChanged );

				_hooked_sortedRecords = sortedRecords;
			}

			if ( unhook )
			{
				_dpActiveRecordTracker = null;
				_filtersVersionTracker = null;
			}
			else if ( null == _filtersVersionTracker )
			{
				_filtersVersionTracker = new PropertyValueTracker( _rm.RecordFiltersResolved,
					ResolvedRecordFilterCollection.VersionProperty, this.OnFiltersChanged, true );

				_filtersVersionTracker.AsynchronousDispatcherPriority = DispatcherPriority.Background;

				_dpActiveRecordTracker = new PropertyValueTracker( dp, DataPresenterBase.ActiveRecordProperty,
					OnDataPresenterActiveRecordChanged, false );
			}
		}

		#endregion // HookUnhookRecordManager

		#region OnDataPresenterActiveRecordChanged

		/// <summary>
		/// Called when the ActiveRecord of the DataPresenter is called. This method syncrhonizes
		/// the CurrentPosition and CurrentItem properties to the new active record.
		/// </summary>
		private void OnDataPresenterActiveRecordChanged( )
		{
			if ( this.IsRefreshSuspended )
				return;

			// SSP 11/1/10 TFS32864
			// 
			this.SyncCurrentPositionToActiveRecordHelper( true, true );
		}

		// SSP 11/1/10 TFS32864
		// Refactored existing code in OnDataPresenterActiveRecordChanged into the new
		// SyncCurrentPositionToActiveRecord method so it can be called from other places
		// as well.
		// 
		private void SyncCurrentPositionToActiveRecordHelper( bool verify, bool raiseChangeEvents )
		{
			if ( verify )
				this.Verify( );

			DataPresenterBase dp = _rm.DataPresenter;
			Debug.Assert( null != dp );
			if ( null != dp )
			{
				// When active record in data presenter is changed, we need to set the current position
				// to the index of the data record in the ancestor hierarchy of the new active record.
				// 
				int newPosition = this.GetPositionAssociatedWithActiveRecord( dp.ActiveRecord );

				// JJD 06/28/10 - TFS34266
				// If the position does not reporesent a record (.e. -1) and the _currentPosition is either
				// before the beginning or after the end of the records then don't do anything
				if (newPosition < 0 && (this._currentPosition < 0 || this._currentPosition == this.Count))
					return;

				this.SetCurrentPosition( newPosition, raiseChangeEvents );
			}
		}

		#endregion // OnDataPresenterActiveRecordChanged

		#region OnFiltersChanged

		/// <summary>
		/// Called whenever the filters are changed.
		/// </summary>
		private void OnFiltersChanged( )
		{
			this.Refresh( );
		}

		#endregion // OnFiltersChanged

		#region OnHasListenersChanged

		/// <summary>
		/// Called when the value of HasListeners property has changed. Basically it gets called
		/// when all events are unhooked from or when one of the events is hooked into when none
		/// of the events were hooked into begin with.
		/// </summary>
		private void OnHasListenersChanged( )
		{
			// If this collection is unbound from what it was bound to, (none of its events 
			// are hooked into anymore), then null out the _list and unhook from data presenter 
			// events because we don't need to synchronize with the data presenter anymore. We
			// do this to release memory used by the _list and also there's no need to continue 
			// syncrhonizing it with the data presenter. This is for better efficiency.
			// 
			// Note that however if one were to access this collection without hooking into it, 
			// it will recreate the _list at that point in Verify method (which will also hook 
			// into data presenter events for syncrhonization purposes) so the collection will 
			// always reflect correct contents.
			// 
			if ( !this.HasListeners && !this.IsRefreshSuspended )
			{
				_list = null;

				this.HookUnhookRecordManager( true );
			}
		}

		#endregion // OnHasListenersChanged

		#region OnRecordFilteredOutStateChanged

		/// <summary>
		/// Called when a record's filter out state changes so this collection can send necessary add/remove notifications.
		/// </summary>
		/// <param name="dataRecord">Data record whose filtered out state changed.</param>
		internal void OnRecordFilteredOutStateChanged( DataRecord dataRecord )
		{
			this.AddRemoveRecordHelper( dataRecord );
		}

		#endregion // OnRecordFilteredOutStateChanged

		#region OnSortedRecords_CollectionChanged

		/// <summary>
		/// Event handler for record manager's Sorted record collection's CollectionChanged event.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnSortedRecords_CollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( null != _list )
			{
				switch ( e.Action )
				{
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Remove:
						{
							DataRecord dr = GridUtilities.GetOneAndOnlyOneItem(
								NotifyCollectionChangedAction.Add == e.Action ? e.NewItems : e.OldItems ) as DataRecord;

							if ( null != dr )
								this.AddRemoveRecordHelper( dr );
							else
								this.Refresh( );

							break;
						}
					default:
						this.Refresh( );
						break;
				}
			}
		}

		#endregion // OnSortedRecords_CollectionChanged

		#region RaiseCollectionChanged

		/// <summary>
		/// Raises CollectionChanged notification as well as necessary property change notifications.
		/// </summary>
		/// <param name="e">Event args</param>
		private void RaiseCollectionChanged( NotifyCollectionChangedEventArgs e )
		{
			if ( null != _collectionChangedHandler )
				_collectionChangedHandler( this, e );

			this.RaiseCurrentPositionRelatedEventsHelper( true );
		}

		#endregion // RaiseCollectionChanged

		#region RaiseCurrentChanging

		/// <summary>
		/// Raises ICollectionView.CurrentChanging event.
		/// </summary>
		/// <param name="e"></param>
		private void RaiseCurrentChanging( CurrentChangingEventArgs e )
		{
			if ( null != _currentChangingHandler )
				_currentChangingHandler( this, e );
		}

		#endregion // RaiseCurrentChanging

		#region RaiseCurrentChanged

		/// <summary>
		/// Raises ICollectionView.CurrentChanged event.
		/// </summary>
		private void RaiseCurrentChanged( )
		{
			if ( null != _currentChangedHandler )
				_currentChangedHandler( this, new EventArgs( ) );
		}

		#endregion // RaiseCurrentChanged

		#region RaiseCurrentPositionRelatedEvents

		/// <summary>
		/// Raises Count, IsEmpty, CurrentItem, CurrentPosition, IsCurrentBeforeFirst
		/// and IsCurrentAfterLast property change notifications if the values have
		/// changed.
		/// </summary>
		/// <param name="raisePropertyChange">Whether to actually raise the property change
		/// notifications or just to update the cached property values that we use to determine
		/// next time this method is called whether a property's value has changed.</param>
		private void RaiseCurrentPositionRelatedEventsHelper( bool raisePropertyChange )
		{
			this.RaisePropertyChanged( "Count" );

			bool oldIsEmpty = _cachedLastIsEmpty;
			_cachedLastIsEmpty = this.IsEmpty;
			if ( oldIsEmpty != _cachedLastIsEmpty && raisePropertyChange )
				this.RaisePropertyChanged( "IsEmpty" );

			object oldCurrentItem = _cachedLastCurrentItem;
			_cachedLastCurrentItem = this.CurrentItem;
			if ( oldCurrentItem != _cachedLastCurrentItem && raisePropertyChange )
				this.RaisePropertyChanged( "CurrentItem" );

			int oldCurrentPosition = _cachedLastCurrentPosition;
			_cachedLastCurrentPosition = this.CurrentPosition;
			if ( oldCurrentPosition != _cachedLastCurrentPosition && raisePropertyChange )
				this.RaisePropertyChanged( "CurrentPosition" );

			bool oldIsCurrentBeforeFirst = _cachedLastIsCurrentBeforeFirst;
			_cachedLastIsCurrentBeforeFirst = ((ICollectionView)this).IsCurrentBeforeFirst;
			if ( oldIsCurrentBeforeFirst != _cachedLastIsCurrentBeforeFirst && raisePropertyChange )
				this.RaisePropertyChanged( "IsCurrentBeforeFirst" );

			bool oldIsCurrentAfterLast = _cachedLastIsCurrentAfterLast;
			_cachedLastIsCurrentAfterLast = ((ICollectionView)this).IsCurrentAfterLast;
			if ( oldIsCurrentAfterLast != _cachedLastIsCurrentAfterLast && raisePropertyChange )
				this.RaisePropertyChanged( "IsCurrentAfterLast" );
		}

		#endregion // RaiseCurrentPositionRelatedEvents

		#region RaisePropertyChanged

		/// <summary>
		/// Raises INotifyPropertyChanged.PropertyChanged event.
		/// </summary>
		/// <param name="name">Name of the property whose value changed.</param>
		private void RaisePropertyChanged( string name )
		{
			if ( null != _propertyChangedHandler )
				_propertyChangedHandler( this, new PropertyChangedEventArgs( name ) );
		}

		#endregion // RaisePropertyChanged

		#region ResumeRefresh

		private void ResumeRefresh( )
		{
			if ( _suspendRefreshCount > 0 )
			{
				_suspendRefreshCount--;

				if ( !this.IsRefreshSuspended )
					this.Verify( );
			}
		}

		#endregion // ResumeRefresh

		#region SetCurrentPosition

		/// <summary>
		/// Sets the CurrentPosition property and updates the related properties 
		/// (like CurrentItem, IsCurrentBeforeFirst, IsCurrentAfterLast etc..).
		/// </summary>
		/// <param name="position">New position.</param>
		/// <returns>True if the position was changed successfully. Otherwise false, which could
		/// happen if CurrentChanging or RecordActivating were canceled.</returns>
		private bool SetCurrentPosition( int position )
		{
			return this.SetCurrentPosition( position, true );
		}

		/// <summary>
		/// Sets the CurrentPosition property and updates the related properties 
		/// (like CurrentItem, IsCurrentBeforeFirst, IsCurrentAfterLast etc..).
		/// </summary>
		/// <param name="position">New position.</param>
		/// <param name="raiseChangeEvents">Specifies whether to raise current position changing and changed events.</param>
		/// <returns>True if the position was changed successfully. Otherwise false, which could
		/// happen if CurrentChanging or RecordActivating were canceled.</returns>
		private bool SetCurrentPosition( int position, bool raiseChangeEvents )
		{
			this.Verify( );

			// SSP 10/28/10 TFS32864
			// 
			int originalRequestedPosition = position;

			// JJD 06/28/10 - TFS34266
			// Constrain the new position from -1 to count
			position = Math.Max(Math.Min(position, this.Count), -1);

			int oldPosition = this.CurrentPosition;
			if ( oldPosition != position )
			{
				// SSP 11/1/10 TFS32864
				// Added raiseChangeEvents parameter.
				// 
				if ( raiseChangeEvents )
				{
					CurrentChangingEventArgs eventArgs = new CurrentChangingEventArgs( true );
					this.RaiseCurrentChanging( eventArgs );
					if ( eventArgs.Cancel )
						return false;
				}

				DataRecord newCurrentRecord = position >= 0 && position < _list.Count
					? (DataRecord)_list[position] : null;

				DataPresenterBase dp = _rm.DataPresenter;
				Debug.Assert( null != dp );
				if ( null != dp )
				{
					// JJD 06/28/10 - TFS34266
					// Set the _currentPosition member so when we set the ActiveRecord below
					// we don't end up raising the CurrentChanging event again since we
					// are listening for ActiveRecord changes. This will prevent the event
					// from being raised twice
					_currentPosition = position;

					// JJD 6/24/11 - TFS71543
					// We only want to sync the dp's ActiveRecord if the newCurrentRecord is not null
					// or the dp's active record is a data record. This prevents clearing of the dp's
					// ActiveRecord when it is set to a special rcd, e.g. a GroupByRecord.
					//dp.ActiveRecord = newCurrentRecord;
					Record activeRcd = dp.ActiveRecord;
					if (newCurrentRecord != null || activeRcd == null || activeRcd.IsDataRecord  )
						dp.ActiveRecord = newCurrentRecord;

					if ( dp.ActiveRecord == newCurrentRecord )
						_currentPosition = position;
					else
						_currentPosition = this.GetPositionAssociatedWithActiveRecord( dp.ActiveRecord );
				}

				// SSP 11/1/10 TFS32864
				// Added raiseChangeEvents parameter.
				// 
				//if ( oldPosition != this.CurrentPosition )
				if ( raiseChangeEvents && oldPosition != this.CurrentPosition )
				{
					this.RaiseCurrentChanged( );
					this.RaiseCurrentPositionRelatedEventsHelper( true );
				}
			}

			// SSP 10/28/10 TFS32864
			// 
			//return this.CurrentPosition == position;
			return this.CurrentPosition == originalRequestedPosition;
		}

		#endregion // SetCurrentPosition

		#region SuspendRefresh

		private void SuspendRefresh( )
		{
			_suspendRefreshCount++;
		}

		#endregion // SuspendRefresh

		#region Verify

		/// <summary>
		/// Verifies the list of filtered out records.
		/// </summary>
		private void Verify( )
		{
			if ( null == _list || _listDirty && ! this.IsRefreshSuspended )
			{
				_listDirty = false;

				// If _list was null then we don't need to raise any notifications because this is
				// the first time the collection is being accessed.
				// 
				bool raisePropertyChangeNotifications = null != _list && this.HasListeners;

				_list = new SparseList( );
				foreach ( DataRecord dr in _rm.Sorted )
				{
					if ( ! dr.InternalIsFilteredOut_Verify )
						_list.Add( dr );
				}

				// Hook into various data presenter events so we can synchronize this collection with
				// what's in the data presenter.
				// 
				this.HookUnhookRecordManager( false );

				// SSP 11/1/10 TFS32864
				// Synchronize the CurrentPosition property to the active record.
				// 
				this.SyncCurrentPositionToActiveRecordHelper( false, false );

				// Raise Reset CollectionChanged notification.
				// 
				if ( raisePropertyChangeNotifications )
					this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

				// Raise property change notifications for Count, CurrentPosition and
				// related properties.
				// 
				this.RaiseCurrentPositionRelatedEventsHelper( raisePropertyChangeNotifications );
			}
		}

		#endregion // Verify

		#endregion // Private/Internal Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Returns the number of items in the collection.
		/// </summary>
		public int Count
		{
			get
			{
				this.Verify( );
				return _list.Count;
			}
		}

		#endregion // Count

		#region CurrentItem

		/// <summary>
		/// Returns the current item (ICollectionView.CurrentItem).
		/// </summary>
		public object CurrentItem
		{
			get
			{
				this.Verify( );
				int pos = this.CurrentPosition;
				DataRecord dr = pos >= 0 && pos < _list.Count ? (DataRecord)_list[pos] : null;
				return null != dr ? dr.DataItem : null;
			}
		}

		#endregion // CurrentItem

		#region CurrentPosition

		/// <summary>
		/// Returns the current position (ICollectionView.CurrentPosition).
		/// </summary>
		public int CurrentPosition
		{
			get
			{
				this.Verify( );
				return _currentPosition;
			}
		}

		#endregion // CurrentPosition

		#region IsEmpty

		/// <summary>
		/// Indicates if the collection is empty.
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				return 0 == this.Count;
			}
		}

		#endregion // IsEmpty

		#endregion // Public Properties

		#region Private/Internal Properties

		#region HasListeners

		/// <summary>
		/// Indicates if any of the events are hooked into.
		/// </summary>
		private bool HasListeners
		{
			get
			{
				return null != _collectionChangedHandler
					|| null != _currentChangingHandler
					|| null != _currentChangedHandler
					|| null != _propertyChangedHandler;
			}
		}

		#endregion // HasListeners

		#region IsRefreshSuspended

		/// <summary>
		/// Indicates if updates to _list are suspended.
		/// </summary>
		private bool IsRefreshSuspended
		{
			get
			{
				return _suspendRefreshCount > 0;
			}
		}

		#endregion // IsRefreshSuspended

		#endregion // Private/Internal Properties

		#endregion // Properties

		#region IList Members

		int IList.Add( object value )
		{
			throw new NotSupportedException( );
		}

		void IList.Clear( )
		{
			throw new NotSupportedException( );
		}

		void IList.Insert( int index, object value )
		{
			throw new NotSupportedException( );
		}

		public bool IsFixedSize
		{
			get { return true; }
		}

		public bool IsReadOnly
		{
			get { return true; }
		}

		void IList.Remove( object value )
		{
			throw new NotSupportedException( );
		}

		void IList.RemoveAt( int index )
		{
			throw new NotSupportedException( );
		}

		#endregion

		#region ICollection Members

		void ICollection.CopyTo( Array array, int index )
		{
			this.Verify( );

			foreach ( object ii in this )
				array.SetValue( ii, index++ );
		}

		public bool IsSynchronized
		{
			get { return false; }
		}

		public object SyncRoot
		{
			get { return this; }
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			this.Verify( );

			// The _list is a list of DataRecords. Here we need to return an enumerator of
			// data items. Therefore create a ConverterEnumerator that converts DataRecords
			// into their data items.
			// 
			return new GridUtilities.ConverterEnumerator<DataRecord, object>( 
				new TypedEnumerable<DataRecord>.Enumerator( _list.GetEnumerator( ) ),
				delegate ( DataRecord dr )
				{
					return dr.DataItem;
				} 
			);
		}

		#endregion

		#region ICollectionView Explicit Implementations

		#region ICollectionView.CanFilter

		bool ICollectionView.CanFilter
		{
			get 
			{
				return false;
			}
		}

		#endregion // ICollectionView.CanFilter

		#region ICollectionView.CanGroup

		bool ICollectionView.CanGroup
		{
			get 
			{
				return false;
			}
		}

		#endregion // ICollectionView.CanGroup

		#region ICollectionView.CanSort

		bool ICollectionView.CanSort
		{
			get 
			{
				return false;
			}
		}

		#endregion // ICollectionView.CanSort

		#region ICollectionView.Culture

		CultureInfo ICollectionView.Culture
		{
			get
			{
				return GridUtilities.GetDefaultCulture( _rm.FieldLayout );
			}
			set
			{
				// Since Culture is used for sorting, set should not get called since
				// this collection view implementation doesn't support sorting.
				// 
			}
		}

		#endregion // ICollectionView.Culture

		#region ICollectionView.DeferRefresh

		IDisposable ICollectionView.DeferRefresh( )
		{
			this.SuspendRefresh( );
			return new DeferRefreshObject( this );
		}

		#endregion // ICollectionView.DeferRefresh

		#region ICollectionView.Filter

		Predicate<object> ICollectionView.Filter
		{
			get
			{
				return null;
			}
			set
			{
				throw new NotSupportedException( );
			}
		}

		#endregion // ICollectionView.Filter

		#region ICollectionView.GroupDescriptions

		ObservableCollection<GroupDescription> ICollectionView.GroupDescriptions
		{
			get 
			{
				return null;
			}
		}

		#endregion // ICollectionView.GroupDescriptions

		#region ICollectionView.Groups

		ReadOnlyObservableCollection<object> ICollectionView.Groups
		{
			get 
			{
				return null;
			}
		}

		#endregion // ICollectionView.Groups

		#region ICollectionView.IsCurrentAfterLast

		bool ICollectionView.IsCurrentAfterLast
		{
			get 
			{
				int currPos = this.CurrentPosition;
				return currPos >= _list.Count;
			}
		}

		#endregion // ICollectionView.IsCurrentAfterLast

		#region ICollectionView.IsCurrentBeforeFirst

		bool ICollectionView.IsCurrentBeforeFirst
		{
			get 
			{
				return this.CurrentPosition < 0;
			}
		}

		#endregion // ICollectionView.IsCurrentBeforeFirst

		#region ICollectionView.MoveCurrentTo

		bool ICollectionView.MoveCurrentTo( object item )
		{
			this.Verify( );
			return this.SetCurrentPosition( this.IndexOf( item ) );
		}

		#endregion // ICollectionView.MoveCurrentTo

		#region ICollectionView.MoveCurrentToFirst

		bool ICollectionView.MoveCurrentToFirst( )
		{
			this.Verify( );
			return _list.Count > 0 && this.SetCurrentPosition( 0 );
		}

		#endregion // ICollectionView.MoveCurrentToFirst

		#region ICollectionView.MoveCurrentToLast

		bool ICollectionView.MoveCurrentToLast( )
		{
			this.Verify( );
			return _list.Count > 0 && this.SetCurrentPosition( _list.Count - 1 );
		}

		#endregion // ICollectionView.MoveCurrentToLast

		#region ICollectionView.MoveCurrentToNext

		bool ICollectionView.MoveCurrentToNext( )
		{
			this.Verify( );
			return this.SetCurrentPosition( this.CurrentPosition + 1 );
		}

		#endregion // ICollectionView.MoveCurrentToNext

		#region ICollectionView.MoveCurrentToPosition

		bool ICollectionView.MoveCurrentToPosition( int position )
		{
			this.Verify( );
			return this.SetCurrentPosition( position );
		}

		#endregion // ICollectionView.MoveCurrentToPosition

		#region ICollectionView.MoveCurrentToPrevious

		bool ICollectionView.MoveCurrentToPrevious( )
		{
			this.Verify( );
			return this.SetCurrentPosition( this.CurrentPosition - 1 );
		}

		#endregion // ICollectionView.MoveCurrentToPrevious

		#region ICollectionView.SortDescriptions

		SortDescriptionCollection ICollectionView.SortDescriptions
		{
			get 
			{
				return null;
			}
		}

		#endregion // ICollectionView.SortDescriptions

		#region ICollectionView.SourceCollection

		IEnumerable ICollectionView.SourceCollection
		{
			get 
			{
				return this;
			}
		}

		#endregion // ICollectionView.SourceCollection

		#endregion // ICollectionView Explicit Implementations

	}

	#endregion // FilteredDataItemsCollection Class

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