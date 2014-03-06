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







using Infragistics.Collections;

namespace Infragistics.Controls.Primitives

{
	#region DataListChangeInfo Struct

	internal struct DataListChangeInfo
	{
		internal enum ChangeType
		{
			Add,
			Remove,
			Replace,
			Move,
			Reset,
			PropertyDescriptorAdded,
			PropertyDescriptorChanged,
			PropertyDescriptorRemoved
		}

		internal IEnumerable _list;
		internal ChangeType _changeType;
		internal IList _oldItems, _newItems;
		internal int _oldIndex, _newIndex;
		internal object _eventArgs;

		// JJD 9/09/11 - Added property descriptor member for PropertyDescriptorAdded/ChangedRemoved 
		internal PropertyDescriptor _propertyDescriptor;


		internal DataListChangeInfo( IEnumerable list, object eventArgs )
		{
			_list = list;
			_eventArgs = eventArgs;
			_changeType = ChangeType.Reset;
			_oldItems = null;
			_newItems = null;
			_oldIndex = -1;
			_newIndex = -1;

			// JJD 9/09/11 - Added property descriptor member for PropertyDescriptorAdded/ChangedRemoved 
			_propertyDescriptor = null;

		}

		internal IList OldItems
		{
			get
			{
				return _oldItems;
			}
		}

		internal IList NewItems
		{
			get
			{
				if (null == _newItems && _list != null && _newIndex >= 0)
					_newItems = new object[] { CoreUtilities.GetItemAt( _list, _newIndex ) };

				return _newItems;
			}
		}

		/// <summary>
		/// If the NewItems has only single item, then returns it. If it has no items or has more
		/// than 1 items, it returns null.
		/// </summary>
		internal object FirstAndOnlyNewItem
		{
			get
			{
				int c = null != _newItems ? CoreUtilities.GetCount( _newItems )
					: ( _newIndex >= 0 ? 1 : 0 );

				if ( 1 == c )
					return null != _newItems ? _newItems[0] : CoreUtilities.GetItemAt( _list, _newIndex );

				return null;
			}
		}
	}

	#endregion // DataListChangeInfo Struct

    #region DataListEventListener Class

    internal class DataListEventListener
    {
        private IEnumerable _list;
        private WeakReference _owner;
        private Action<object, DataListEventListener, DataListChangeInfo> _handler;
        private Action<object, object, string> _itemPropChangedHandler;
        private bool? _hookIntoItemsNotifyPropChanged;
        private bool _isHookedIntoBindingList;
        private bool _isHookedIntoNotifyCollectionChanged;
        private bool _hookIntoItemsNotifyPropChangedResolved;
		private bool _isHookedIntoItemsNotifyPropChanged;

		// JJD 9/06/11 - added autoHookItemsNotifyPropChanged 
		private bool _autoHookItemsNotifyPropChanged;

        private WeakDictionary<INotifyPropertyChanged, WeakPropChangeListener> _pcnListeners = new WeakDictionary<INotifyPropertyChanged, WeakPropChangeListener>( true, true );
		private int _beginTrackingEventsCount;
		private List<DataListChangeInfo> _trackEventsList;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="owner">Owner object which is passed in as the first argument when the handler is invoked.
        /// The handler is typically a reference to a static method and therefore it needs to know which owner
        /// to delegate the processing to.</param>
		/// <param name="listChangedHandler">Called when list changes.</param>
		/// <param name="itemPropChangedHandler">Called when a list item changes.</param>
        /// <param name="hookIntoItemsNotifyPropChanged">Hook into prop changed.</param>
		/// <param name="autoHookItemsNotifyPropChanged">If thru will callHookIntoDataItem automatically</param>
		// JJD 9/06/11 - added autoHookItemsNotifyPropChanged parameter
		internal DataListEventListener( object owner, 
            Action<object, DataListEventListener, DataListChangeInfo> listChangedHandler, 
            Action<object, object, string> itemPropChangedHandler,
			bool? hookIntoItemsNotifyPropChanged, bool autoHookItemsNotifyPropChanged = false)
        {
            CoreUtilities.ValidateNotNull( owner );
			
			// JJD 9/06/11 - added autoHookItemsNotifyPropChanged 
			_autoHookItemsNotifyPropChanged = autoHookItemsNotifyPropChanged && itemPropChangedHandler != null;

            _owner = new WeakReference( owner );
            _handler = listChangedHandler;
            _itemPropChangedHandler = itemPropChangedHandler;
            _hookIntoItemsNotifyPropChanged = hookIntoItemsNotifyPropChanged;
        }

        internal IEnumerable List
        {
            get
            {
                return _list;
            }
            set
            {
                this.SetListHelper( value, _list );
            }
        }

        private object Owner
        {
            get
            {
                object r = null != _owner ? CoreUtilities.GetWeakReferenceTargetSafe( _owner ) : null;
                if ( null == r )
                {
                    _owner = null;
                    this.List = null;
                }

                return r;
            }
        }

		/// <summary>
		/// Indicates if the data items' property change notifications are going to be raised.
		/// </summary>
		internal bool SupportsItemPropChangeNotifications
		{
			get
			{
				return _isHookedIntoItemsNotifyPropChanged || _isHookedIntoBindingList;
			}
		}

        private class WeakPropChangeListener
        {
            private WeakReference _owner;
            private INotifyPropertyChanged _pcn;

            internal WeakPropChangeListener( INotifyPropertyChanged pcn, DataListEventListener owner )
            {
                CoreUtilities.ValidateNotNull( pcn );

                _owner = new WeakReference( owner );
                _pcn = pcn;

                pcn.PropertyChanged += new PropertyChangedEventHandler( this.OnPropertyChangedHandler );
            }

            private void OnPropertyChangedHandler( object sender, PropertyChangedEventArgs e )
            {
                DataListEventListener owner = this.Owner;
                object ownerOwner = null != owner ? owner.Owner : null;

                if ( null == ownerOwner )
                {
					this.UnHook( );
                    return;
                }

				var handler = owner._itemPropChangedHandler;
                if ( null != handler )
					handler( ownerOwner, sender, e.PropertyName );
            }

            internal void UnHook( )
            {
                _pcn.PropertyChanged -= new PropertyChangedEventHandler( this.OnPropertyChangedHandler );
            }

            private DataListEventListener Owner
            {
                get
                {
                    return (DataListEventListener)CoreUtilities.GetWeakReferenceTargetSafe( _owner );
                }
            }

        }

        internal void HookIntoDataItem( object dataItem )
        {
            this.HookUnHookDataItem( dataItem, true );
        }

        internal void UnhookFromDataItem( object dataItem )
        {
            this.HookUnHookDataItem( dataItem, true );
        }

        internal void HookUnHookDataItem( object dataItem, bool hook )
        {
            

            if ( _hookIntoItemsNotifyPropChangedResolved )
            {
                INotifyPropertyChanged pcn = dataItem as INotifyPropertyChanged;
                if ( null != pcn )
                {
                    WeakPropChangeListener listener;
                    if ( _pcnListeners.TryGetValue( pcn, out listener ) )
                    {
                        if ( !hook )
                        {
                            listener.UnHook( );
                            _pcnListeners.Remove( pcn );
                        }
                    }
                    else
                    {
                        if ( hook )
                        {
                            listener = new WeakPropChangeListener( pcn, this );
                            _pcnListeners[pcn] = listener;
							_isHookedIntoItemsNotifyPropChanged = true;
                        }
                    }
                }
            }
        }

        private void SetListHelper( IEnumerable newList, IEnumerable oldList )
        {
            INotifyCollectionChanged cc = oldList as INotifyCollectionChanged;


            IBindingList bl = oldList as IBindingList;



            if ( null != bl )
                bl.ListChanged -= new ListChangedEventHandler( this.OnBindingListChanged );

            if ( null != cc )
                cc.CollectionChanged -= new NotifyCollectionChangedEventHandler( this.OnCollectionChanged );

			_isHookedIntoBindingList = _isHookedIntoNotifyCollectionChanged = _isHookedIntoItemsNotifyPropChanged = false;

            _list = newList;

            cc = newList as INotifyCollectionChanged;


            bl = newList as IBindingList;



            if ( null != bl )
            {
                bl.ListChanged += new ListChangedEventHandler( this.OnBindingListChanged );
                _isHookedIntoBindingList = true;
            }

            
            if ( null != cc )
            {
                cc.CollectionChanged += new NotifyCollectionChangedEventHandler( this.OnCollectionChanged );
                _isHookedIntoNotifyCollectionChanged = true;
            }

            _hookIntoItemsNotifyPropChangedResolved = _hookIntoItemsNotifyPropChanged.HasValue ? _hookIntoItemsNotifyPropChanged.Value 
                // If hooked into the binding list, we don't need to hook into individual data item's INotifyPropertyChanged since
                // we expect the binding list to raise ItemChanged notification as part of its ListChanged event.
                // 
                : ! _isHookedIntoBindingList;

            // Unhook from old data items.
            // 
            if ( null != _pcnListeners && _pcnListeners.Count > 0 )
            {
                List<WeakPropChangeListener> listeners = new List<WeakPropChangeListener>( _pcnListeners.Values );
                foreach ( WeakPropChangeListener ii in listeners )
                    ii.UnHook( );

				_pcnListeners.Clear( );
            }

			// JJD 9/06/11 - added autoHookItemsNotifyPropChanged 
			// If we are auto hooking each item's NotifyPropertyChanged event then loop over the 
			// items and wire them up
			if (_autoHookItemsNotifyPropChanged && _hookIntoItemsNotifyPropChangedResolved && _list != null)
			{
				foreach (object item in _list)
				{
					this.HookIntoDataItem(item);
				}
			}
        }

		// JJD 9/06/11 - added 
		private void ProcesAutoHookOnReset()
		{
			// Create a HashSet of all the items that are currently hooked
			HashSet<object> oldItems = new HashSet<object>();

			// Create a HashSet of all the new items
			foreach (object item in _pcnListeners.Keys)
				oldItems.Add(item);

			HashSet<object> newItems = new HashSet<object>();

			if (_list != null)
			{
				foreach (object item in _list)
					newItems.Add(item);
			}

			// create a hash of the intersection of the old and new items
			HashSet<object> intersection = new HashSet<object>(oldItems);
			intersection.IntersectWith(newItems);

			// walk over the old items and unhook any that are not still in the list
			foreach (object oldItem in oldItems)
			{
				if (!intersection.Contains(oldItem))
					this.UnhookFromDataItem(oldItem);
			}

			// walk over the new items and hook any that weren't there before
			if (_list != null)
			{
				foreach (object newItem in _list)
				{
					if (!intersection.Contains(newItem))
						this.HookIntoDataItem(newItem);
				}
			}
		}

        private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
        {
            object owner = this.Owner;
            if ( null == owner )
                return;

            DataListChangeInfo info = new DataListChangeInfo( _list, e );

			// JJD 9/06/11 - added autoHookItemsNotifyPropChanged 
			bool autoHook = (_autoHookItemsNotifyPropChanged && _hookIntoItemsNotifyPropChangedResolved && _list != null);
			
            switch ( e.Action )
            {
                case NotifyCollectionChangedAction.Add:
                    info._changeType = DataListChangeInfo.ChangeType.Add;
                    info._newIndex = e.NewStartingIndex;
                    info._newItems = e.NewItems;

					// JJD 9/06/11 
					// if we are auto-hooking then process then change
					if (autoHook)
					{
						foreach (object item in e.NewItems)
							this.HookIntoDataItem(item);
					}
                    break;

                case NotifyCollectionChangedAction.Move:
                    info._changeType = DataListChangeInfo.ChangeType.Move;
                    info._newIndex = e.NewStartingIndex;
                    info._newItems = e.NewItems;
                    info._oldIndex = e.OldStartingIndex;
                    info._oldItems = e.OldItems;
                    break;

                case NotifyCollectionChangedAction.Remove:
                    info._changeType = DataListChangeInfo.ChangeType.Remove;
                    info._oldIndex = e.OldStartingIndex;
                    info._oldItems = e.OldItems;
					// JJD 9/06/11 
					// if we are auto-hooking then process then change
					if (autoHook)
					{
						foreach (object item in e.OldItems)
							this.UnhookFromDataItem(item);
					}
                    break;
                case NotifyCollectionChangedAction.Replace:
                    info._changeType = DataListChangeInfo.ChangeType.Replace;
                    info._newIndex = e.NewStartingIndex;
                    info._newItems = e.NewItems;
                    info._oldIndex = e.OldStartingIndex;
                    info._oldItems = e.OldItems;
					// JJD 9/06/11 
					// if we are auto-hooking then process then change
					if (autoHook)
					{
						foreach (object item in e.OldItems)
							this.UnhookFromDataItem(item);
						foreach (object item in e.NewItems)
							this.HookIntoDataItem(item);
					}
                    break;
                case NotifyCollectionChangedAction.Reset:
                    info._changeType = DataListChangeInfo.ChangeType.Reset;
 					// JJD 9/06/11 
					// if we are auto-hooking then process then change
					if ( autoHook )
						this.ProcesAutoHookOnReset();
                   break;
            }

			this.InvokeHandlerHelper( owner, ref info );
        }


        private void OnBindingListChanged( object sender, ListChangedEventArgs e )
        {
            object owner = this.Owner;
            if ( null == owner )
                return;

            DataListChangeInfo info = new DataListChangeInfo( _list, e );

            switch ( e.ListChangedType )
            {
                case ListChangedType.ItemAdded:
                    if ( _isHookedIntoNotifyCollectionChanged )
                        return;

                    info._changeType = DataListChangeInfo.ChangeType.Add;
                    info._newIndex = e.NewIndex;
                    break;
                case ListChangedType.ItemChanged:
					if (null != _itemPropChangedHandler)
					{
						object item = CoreUtilities.GetItemAt(_list, e.NewIndex);
						if (null != item)
							_itemPropChangedHandler(owner, item, null != e.PropertyDescriptor ? e.PropertyDescriptor.Name : null);
					}

					// JJD 06/11/12 - TFS113628
					// If the PropertyDescriptor is null treat this as an item replace.
					if (e.PropertyDescriptor == null)
					{
						info._changeType = DataListChangeInfo.ChangeType.Replace;
						info._oldIndex = e.OldIndex;
						info._newIndex = e.NewIndex;
						break;
					}
                    return;
                case ListChangedType.ItemDeleted:
                    if ( _isHookedIntoNotifyCollectionChanged )
                        return;

                    info._changeType = DataListChangeInfo.ChangeType.Remove;

					// JJD 11/29/11 - TFS96679
					// Since ListChangedEventArgs uses the new index to identify the
					// removed item set the _oldIndex to that value since the
					// NotifyCollectionChangedArgs uses the old index instead.
					// info._oldIndex = e.OldIndex;
					info._oldIndex = e.NewIndex;
                    break;
                case ListChangedType.ItemMoved:
                    if ( _isHookedIntoNotifyCollectionChanged )
                        return;

                    info._changeType = DataListChangeInfo.ChangeType.Move;
                    info._oldIndex = e.OldIndex;
                    info._newIndex = e.NewIndex;
                    break;
                case ListChangedType.Reset:
                    if ( _isHookedIntoNotifyCollectionChanged )
                        return;

                    info._changeType = DataListChangeInfo.ChangeType.Reset;
                    break;
                case ListChangedType.PropertyDescriptorAdded:
                    info._changeType = DataListChangeInfo.ChangeType.PropertyDescriptorAdded;
					// JJD 9/09/11 - Added property descriptor member for PropertyDescriptorAdded/ChangedRemoved 
					info._propertyDescriptor = e.PropertyDescriptor;
                    break;
                case ListChangedType.PropertyDescriptorChanged:
                    info._changeType = DataListChangeInfo.ChangeType.PropertyDescriptorChanged;
					// JJD 9/09/11 - Added property descriptor member for PropertyDescriptorAdded/ChangedRemoved 
					info._propertyDescriptor = e.PropertyDescriptor;
                    break;
                case ListChangedType.PropertyDescriptorDeleted:
                    info._changeType = DataListChangeInfo.ChangeType.PropertyDescriptorRemoved;
					// JJD 9/09/11 - Added property descriptor member for PropertyDescriptorAdded/ChangedRemoved 
					info._propertyDescriptor = e.PropertyDescriptor;
					break;
            }

            
			this.InvokeHandlerHelper( owner, ref info );
        }


		private void InvokeHandlerHelper( object owner, ref DataListChangeInfo info )
		{
			if ( null != _trackEventsList )
				_trackEventsList.Add( info );

			_handler( owner, this, info );
		}

		private void BeginTrackingEvents( )
		{
			_beginTrackingEventsCount++;

			if ( null == _trackEventsList )
				_trackEventsList = new List<DataListChangeInfo>( );
		}

		private void EndTrackingEvents( )
		{
			if ( _beginTrackingEventsCount > 0 )
			{
				_beginTrackingEventsCount--;
				
				if ( 0 == _beginTrackingEventsCount )
					_trackEventsList = null;
			}
			else
				Debug.Assert( false );
		}

		internal void BeginAdd( )
		{
			this.BeginAddRemoveHelper( true );
		}

		internal void EndAdd( object dataItem, bool ensureAddRaised )
		{
			this.EndAddRemoveHelper( true, dataItem, ensureAddRaised );
		}

		internal void BeginRemove( )
		{
			this.BeginAddRemoveHelper( false );
		}

		internal void EndRemove( object dataItem, bool ensureRemoveRaised )
		{
			this.EndAddRemoveHelper( false, dataItem, ensureRemoveRaised );
		}

		private bool HasEventInTrackedList( DataListChangeInfo.ChangeType changeType )
		{
			Debug.Assert( null != _trackEventsList );
			if ( null != _trackEventsList )
				return _trackEventsList.Any( ii => changeType == ii._changeType );

			return false;
		}

		private void BeginAddRemoveHelper( bool add )
		{
			this.BeginTrackingEvents( );
		}

		private void EndAddRemoveHelper( bool add, object dataItem, bool ensureEventRaised )
		{
			if ( ensureEventRaised )
			{
				bool addOrResetWasReceived = 
					HasEventInTrackedList( add ? DataListChangeInfo.ChangeType.Add : DataListChangeInfo.ChangeType.Remove )
					|| HasEventInTrackedList( DataListChangeInfo.ChangeType.Reset );

				if ( !addOrResetWasReceived )
					this.RaiseAddOrRemoveHelper( dataItem, add );
			}

			this.EndTrackingEvents( );
		}

		private void RaiseAddOrRemoveHelper( object dataItem, bool add )
		{
			object[] items = new object[] { dataItem };

			NotifyCollectionChangedEventArgs args = CreateAddRemoveNCCArgs( add, items, -1 );

			this.OnCollectionChanged( _list, args );
		}

		#region CreateAddRemoveNCCArgs

		internal static NotifyCollectionChangedEventArgs CreateAddRemoveNCCArgs(
			bool add, IList addRemoveMultiItems, int index)
		{
			NotifyCollectionChangedAction action = add ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove;



#region Infragistics Source Cleanup (Region)

















#endregion // Infragistics Source Cleanup (Region)

			// JJD 11/29/11 - TFS96679
			// Check to make sure the passed in list of items is not null. If it is pass
			// in an array of 1 entry containiing a null ptr
			//return new NotifyCollectionChangedEventArgs(action, addRemoveMultiItems, index);
			return new NotifyCollectionChangedEventArgs( action, addRemoveMultiItems != null ? addRemoveMultiItems : new object[1], index );

		}

		#endregion // CreateAddRemoveNCCArgs

		#region CreateReplaceNCCArgs

		internal static NotifyCollectionChangedEventArgs CreateReplaceNCCArgs( IList oldItems, IList newItems, int index )
		{


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			return new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, newItems, oldItems, index );

		}

		#endregion // CreateReplaceNCCArgs
	}

    #endregion // DataListEventListener Class
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