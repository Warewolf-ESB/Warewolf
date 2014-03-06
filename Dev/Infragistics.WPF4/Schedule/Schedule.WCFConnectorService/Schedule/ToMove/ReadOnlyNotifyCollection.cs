using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.ComponentModel;



using Infragistics.Services;

namespace Infragistics.Collections.Services



{
	#region ReadOnlyNotifyCollection Class

	/// <summary>
	/// Represents a read-only collection of items.
	/// </summary>
	/// <typeparam name="T">Type of items contained in the collection.</typeparam>
	public class ReadOnlyNotifyCollection<T> : IList<T>, IList, INotifyCollectionChanged, ISupportPropertyChangeNotifications, INotifyPropertyChanged
	{
		#region Member Vars

		private IList<T> _source;
		private NotifyCollectionChangedEventHandler _collectionChanged;
        private PropertyChangeListenerList _propChangeListeners;
		private PropertyChangedEventHandler _propertyChanged;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="ReadOnlyNotifyCollection&lt;T&gt;"/>.
		/// </summary>
		/// <param name="source">This is the list that this ReadOnlyNotifyCollection will wrap and provide items from.</param>
		public ReadOnlyNotifyCollection( IList<T> source )
		{
			this.SetSourceCollection( source );
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
				return _source[index];
			}
			set
			{
				CoreUtilities.RaiseReadOnlyCollectionException( );
			}
		}

		#endregion // Indexers

		#region Methods

		#region Public Methods

		#region Contains

		/// <summary>
		/// Indicates whether the specified item is in the collection.
		/// </summary>
		/// <param name="item">Item to check if it's contained in the collection.</param>
		/// <returns>True if the item exists in the collection, false otherwise.</returns>
		public bool Contains( T item )
		{
			return _source.Contains( item );
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
			CoreUtilities.CopyTo<T>( _source, array, arrayIndex );
		}

		#endregion // CopyTo

		#region GetEnumerator

		/// <summary>
		/// Returns the enumerator for enumerating items in the collection.
		/// </summary>
		/// <returns>IEnumerator instance.</returns>
		public IEnumerator<T> GetEnumerator( )
		{
			return _source.GetEnumerator( );
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
			return _source.IndexOf( item );
		}

		#endregion // IndexOf

		#endregion // Public Methods

		#region Private Methods

		#region OnSourceCollectionChanged

		private void OnSourceCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.RaiseCollectionChanged( e, true );
		}

		#endregion // OnSourceCollectionChanged

		#region OnSourcePropertyChanged

		private void OnSourcePropertyChanged( object sender, PropertyChangedEventArgs e )
		{
			this.RaisePropertyChanged(e, true);
		}

		#endregion // OnSourcePropertyChanged

		#endregion // Private Methods

		#region Internal Methods

		#region RaiseCollectionChanged

		internal void RaiseCollectionChanged( NotifyCollectionChangedEventArgs eventArgs, bool notifyListeners )
		{
			if ( null != _collectionChanged )
				_collectionChanged( this, eventArgs );

            if ( notifyListeners && null != _propChangeListeners )
                _propChangeListeners.OnCollectionChanged( this, eventArgs );
		}

        #endregion // RaiseCollectionChanged

        #region RaiseCollectionReset

        internal void RaiseCollectionReset( )
		{
			this.RaisePropertyChanged( new PropertyChangedEventArgs("Count"), true );
			this.RaisePropertyChanged( new PropertyChangedEventArgs("Item[]"), true );

			this.RaiseCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ), true );
		}

		#endregion // RaiseCollectionReset

		#region RaisePropertyChanged

		internal void RaisePropertyChanged( PropertyChangedEventArgs eventArgs, bool notifyListeners )
		{
			if ( null != _propertyChanged )
				_propertyChanged(this, eventArgs);

			if ( notifyListeners && null != _propChangeListeners )
				_propChangeListeners.OnPropertyValueChanged(this, eventArgs.PropertyName, eventArgs);
		}

		#endregion // RaisePropertyChanged

		#region SetSourceCollection

		internal virtual void SetSourceCollection( IList<T> source )
		{
			CoreUtilities.ValidateNotNull( source );

			if ( _source != source )
			{
                this.HookUnhook( _source, false );

				_source = source;

                this.HookUnhook( _source, true );

				this.RaiseCollectionReset( );
			}
		}

        private void HookUnhook( object source, bool hook )
        {
            if ( null != source )
            {
                ISupportPropertyChangeNotifications scn = source as ISupportPropertyChangeNotifications;
                if ( null != scn )
                {
					// note: the way this is currently implemented we are using the same listener
					// to listen to the source collection as we do when someone adds a listener to 
					// this collection. that means that the object hooked into this read-only 
					// collection will get a reference to the underlying collection when the 
					// underlying collection changes. while in theory in might be best to have 
					// this collection catch changes from the underlying collection and send 
					// the read-only collection as the dataitem that would mean that internal 
					// usage that expects to get the underlying collection would not be able
					// to do so and therefore may not be able to properly identify the source 
					// of the change. therefore we have decided to leave this implemented in 
					// this way and expect that developers will not try to modify the dataitem 
					// that is sending a change notification
					//
                    if ( hook )
                        scn.AddListener( this.PropChangeListeners, false );
                    else
                        scn.RemoveListener( this.PropChangeListeners );
                }
                else
                {
                    INotifyCollectionChanged notifyCollection = _source as INotifyCollectionChanged;
                    if ( null != notifyCollection )
                    {
                        if ( hook )
                            notifyCollection.CollectionChanged += new NotifyCollectionChangedEventHandler( OnSourceCollectionChanged );
                        else
                            notifyCollection.CollectionChanged -= new NotifyCollectionChangedEventHandler( OnSourceCollectionChanged );
                    }
                    else
                    {
                        // Sometimes we use an empty array as the source to return an empty read-only collection.
                        // 
						// AS 7/15/10
						// Sometimes we use a single item array too. :)
						//
                        Debug.Assert( source is T[] && ( (T[])source ).Length < 2, "Collection doesn't implement INotifyCollectionChanged." );
                    }

					INotifyPropertyChanged notifyProperty = _source as INotifyPropertyChanged;
					if ( null != notifyProperty )
					{
						if ( hook )
							notifyProperty.PropertyChanged += new PropertyChangedEventHandler(OnSourcePropertyChanged);
						else
							notifyProperty.PropertyChanged -= new PropertyChangedEventHandler(OnSourcePropertyChanged);
					}
                }
            }
        }

		#endregion // SetSourceCollection

		#endregion // Internal Methods

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
				return _source.Count;
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

		#region Source

		/// <summary>
		/// Gets the source collection.
		/// </summary>
		protected IList<T> Source
		{
			get
			{
				return _source;
			}
		} 

		#endregion // Source

        #region Private/Internal Properties

        #region PropChangeListeners

        /// <summary>
        /// Gets collection of property change listeners.
        /// </summary>
        private PropertyChangeListenerList PropChangeListeners
        {
            get
            {
                if ( null == _propChangeListeners )
                {
                    _propChangeListeners = new PropertyChangeListenerList( );
                    _propChangeListeners.Add( new PropertyChangeListener<ReadOnlyNotifyCollection<T>>( this, OnListenersSubObjectPropertyChanged ), false );
                }

                return _propChangeListeners;
            }
        }

        private void OnListenersSubObjectPropertyChanged( ReadOnlyNotifyCollection<T> owner, object sender, string property, object extraInfo )
        {
			if ( sender == _source )
            {
                NotifyCollectionChangedEventArgs e = extraInfo as NotifyCollectionChangedEventArgs;

                if ( null != e )
                    this.RaiseCollectionChanged( e, true );
				else
				{
					PropertyChangedEventArgs propArgs = extraInfo as PropertyChangedEventArgs;

					if ( null != propArgs )
						this.RaisePropertyChanged( propArgs, true );
				}
            }
        }

        #endregion // PropChangeListeners

        #endregion // Private/Internal Properties

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
				return _source[index];
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
			CoreUtilities.CopyTo( _source, array, index );
		}

		bool ICollection.IsSynchronized
		{
			get
			{
				ICollection sourceColl = _source as ICollection;
				return null != sourceColl && sourceColl.IsSynchronized;
			}
		}

		object ICollection.SyncRoot
		{
			get
			{
				ICollection sourceColl = _source as ICollection;
				return null != sourceColl ? sourceColl.SyncRoot : _source;
			}
		}

		#endregion

        #region ISupportPropertyChangeNotifications Implementation

        void ITypedSupportPropertyChangeNotifications<object, string>.AddListener( ITypedPropertyChangeListener<object, string> listener, bool useWeakReference )
        {
            this.PropChangeListeners.Add( listener, useWeakReference );
        }

        void ITypedSupportPropertyChangeNotifications<object, string>.RemoveListener( ITypedPropertyChangeListener<object, string> listener )
        {
            this.PropChangeListeners.Remove( listener );
        }

        #endregion // ISupportPropertyChangeNotifications Implementation

		#region INotifyPropertyChanged Members

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { _propertyChanged = Delegate.Combine(_propertyChanged, value) as PropertyChangedEventHandler; }
			remove { _propertyChanged = Delegate.Remove(_propertyChanged, value) as PropertyChangedEventHandler; }
		}

		#endregion //INotifyPropertyChanged Members
	} 

	#endregion // ReadOnlyNotifyCollection Class
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