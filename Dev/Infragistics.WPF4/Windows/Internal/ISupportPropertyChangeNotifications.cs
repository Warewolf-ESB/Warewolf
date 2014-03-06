using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;




namespace Infragistics

{
    #region ITypedPropertyChangeListener<TItem, TProperty> Interface

    /// <summary>
    /// Interface used to hook into property change notifications of objects that implement 
    /// <see cref="ITypedSupportPropertyChangeNotifications&lt;TItem, TProperty&gt;"/> interface.
    /// </summary>
    /// <typeparam name="TItem">Type of the item whose change notifications are hooked into.</typeparam>
    /// <typeparam name="TProperty">Type of object that identifies properties.</typeparam>
    internal interface ITypedPropertyChangeListener<TItem, TProperty>
    {
        /// <summary>
        /// Called to notify of a change in the value of a property.
        /// </summary>
        /// <param name="dataItem">Item whose property value changed.</param>
        /// <param name="property">Identifies the property whose value changed.</param>
        /// <param name="extraInfo">Any other information regarding the change. The information passed depends on
        /// the implementation of the item that's sending the notification. This can be an instance of 
        /// PropertyChangedEventArgs, or DependencyPropertyChangedEventArgs, NotifyCollectionChangedEventArgs or null.</param>
        void OnPropertyValueChanged( TItem dataItem, TProperty property, object extraInfo );
    }

    #endregion // ITypedPropertyChangeListener<TItem, TProperty> Interface

    #region IPropertyChangeListener Interface

    /// <summary>
    /// Interface used to hook into property change notifications of objects that implement
    /// <see cref="ISupportPropertyChangeNotifications"/> interface.
    /// </summary>
    internal interface IPropertyChangeListener : ITypedPropertyChangeListener<object, string>
    {
    }

    #endregion // IPropertyChangeListener Interface

    #region ITypedSupportPropertyChangeNotifications<TItem, TProperty> Interface

    /// <summary>
    /// Interface implemented by objects to support <see cref="ITypedPropertyChangeListener&lt;TItem, TProperty&gt;"/> listeners.
    /// </summary>
    /// <typeparam name="TItem">Type of the item whose notifications are being sent. Typically the object implementing the interface.</typeparam>
    /// <typeparam name="TProperty">Type of the object that identifies the property.</typeparam>
    internal interface ITypedSupportPropertyChangeNotifications<TItem, TProperty>
    {
        void AddListener( ITypedPropertyChangeListener<TItem, TProperty> listener, bool useWeakReference );
        void RemoveListener( ITypedPropertyChangeListener<TItem, TProperty> listener );
    }

    #endregion // ITypedSupportPropertyChangeNotifications<TItem, TProperty> Interface

    #region ISupportPropertyChangeNotifications Interface

    /// <summary>
    /// Interface implemented by objects to support <see cref="IPropertyChangeListener"/> listeners.
    /// </summary>
    internal interface ISupportPropertyChangeNotifications : ITypedSupportPropertyChangeNotifications<object, string>
    {
    }

    #endregion // ISupportPropertyChangeNotifications Interface

    #region IValueChangeListener Class

    /// <summary>
    /// Listener interface used to listen for value change of a specific property.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    internal interface IValueChangeListener<TItem>
    {
        /// <summary>
        /// Called to notify the change in the value of the property.
        /// </summary>
        /// <param name="dataItem">Item whose property value changed.</param>
        void OnValueChanged( TItem dataItem );
    }

    #endregion // IValueChangeListener Class


    #region ListenerList Class

    /// <summary>
    /// A helper class used for managing one or more listeners. It uses a List when more than one listeners are added.
    /// </summary>
    internal class ListenerList : List<object>
    {
        #region Member Vars

        private int _isTraversingList;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor. Initializes a new instance of <see cref="ListenerList"/>.
        /// </summary>
        private ListenerList( int capacity )
            : base( capacity )
        {
        }

        #endregion // Constructor

        #region CreateWeakRefHelper

        private static object CreateWeakRefHelper( object obj )
        {
            WeakReference w = obj as WeakReference;
            if ( null == w )
                w = new WeakReference( obj );

            return w;
        }

        #endregion // CreateWeakRefHelper

        #region GetItemHelper

        internal static object GetItemHelper( object obj )
        {
            WeakReference w = obj as WeakReference;
            if ( null != w )
                return CoreUtilities.GetWeakReferenceTargetSafe( w );

            return obj;
        }

        #endregion // GetItemHelper

        #region Add

        public static object Add( object currentListener, object listener, bool useWeakReference )
        {
            CoreUtilities.ValidateNotNull( listener );

            object listenerObj = useWeakReference ? CreateWeakRefHelper( listener ) : listener;

            if ( null == currentListener )
            {
                return listenerObj;
            }
            else
            {
                ListenerList list = currentListener as ListenerList;
                if ( null != list )
                {
                    list.Add( listenerObj );
                }
                else
                {
                    list = new ListenerList( 2 );
                    list.Add( currentListener );
                    list.Add( listenerObj );
                }

                return list;
            }
        }

        #endregion // Add

        #region Remove

        public static object Remove( object currentListener, object listener )
        {
            ListenerList list = currentListener as ListenerList;
            if ( null != list )
            {
                for ( int i = list.Count - 1; i >= 0; i-- )
                {
                    if ( listener == GetItemHelper( list[i] ) )
                    {
                        // If we are in the middle of looping through the list and an item is removed,
                        // don't modify the list that's being traversed. Instead create a new list.
                        // 
                        if ( 0 != list._isTraversingList )
                        {
                            ListenerList newList = new ListenerList( list.Count );
                            newList.AddRange( list );
                            list = newList;
                        }

                        list.RemoveAt( i );
                        break;
                    }
                }

                return 1 == list.Count ? list[0] : list;
            }
            else if ( GetItemHelper( currentListener ) == listener )
            {
                return null;
            }
            else
            {
                return currentListener;
            }
        }

        #endregion // Remove

        #region Enumerable Class

        private class Enumerable : IEnumerable<object>
        {
            #region Enumerator Class

            private class Enumerator : IEnumerator<object>
            {
                #region Member Vars

                private IList _l;
                private int _index;
                private object _current;

                #endregion // Member Vars

                #region Constructor

                internal Enumerator( IList l )
                {
                    _l = l;

                    this.Reset( );
                }

                #endregion // Constructor

                #region Current

                public object Current
                {
                    get
                    {
                        return _current;
                    }
                }

                #endregion // Current

                #region Dispose

                public void Dispose( )
                {
                }

                #endregion // Dispose

                #region IEnumerator.Current

                object IEnumerator.Current
                {
                    get
                    {
                        return this.Current;
                    }
                }

                #endregion // IEnumerator.Current

                #region MoveNext

                public bool MoveNext( )
                {
                    _current = null;

                    while ( null == _current && ++_index < _l.Count )
                        _current = GetItemHelper( _l[_index] );

                    return null != _current;
                }

                #endregion // MoveNext

                #region Reset

                public void Reset( )
                {
                    _index = -1;
                    _current = null;
                }

                #endregion // Reset
            }

            #endregion // Enumerator Class

            #region Member Vars

            private IList _l;

            #endregion // Member Vars

            #region Constructor

            internal Enumerable( IList l )
            {
                _l = l;
            }

            #endregion // Constructor

            #region GetEnumerator

            public IEnumerator<object> GetEnumerator( )
            {
                return new Enumerator( _l );
            }

            #endregion // GetEnumerator

            #region IEnumerable.GetEnumerator

            IEnumerator IEnumerable.GetEnumerator( )
            {
                return this.GetEnumerator( );
            }

            #endregion // IEnumerable.GetEnumerator
        }

        #endregion // Enumerable Class

        #region GetListeners

        public static IEnumerable<object> GetListeners( object currentListener )
        {
            ListenerList list = currentListener as ListenerList;
            if ( null != list )
                return new Enumerable( list );
            else if ( null != currentListener )
                return new object[] { currentListener };
            else
                return new object[0];
        }

        public static void GetListeners( object currentListener, out IEnumerable<object> multiListeners, out object singleListener )
        {
            singleListener = null;
            multiListeners = null;

			ListenerList list = currentListener as ListenerList;
            if ( null != list )
            {
                multiListeners = new Enumerable( list );
            }
            else
            {
                object listner = GetItemHelper( currentListener );
                if ( null != listner )
                    singleListener = listner;
            }
        }

        #endregion // GetListeners

        #region RaisePropertyChanged<TItem, TProperty>

        internal static void RaisePropertyChanged<TItem, TProperty>( object listeners, TItem sender, TProperty property, object extraInfo )
        {
            ListenerList list = listeners as ListenerList;
            if ( null != list )
            {
                list._isTraversingList++;
                try
                {
                    for ( int i = 0, count = list.Count; i < count; i++ )
                    {
                        ITypedPropertyChangeListener<TItem, TProperty> item = (ITypedPropertyChangeListener<TItem, TProperty>)GetItemHelper( list[i] );
                        if ( null != item )
                            item.OnPropertyValueChanged( sender, property, extraInfo );
                    }
                }
                finally
                {
                    list._isTraversingList--;
                }
            }
            else if ( null != listeners )
            {
                ITypedPropertyChangeListener<TItem, TProperty> item = (ITypedPropertyChangeListener<TItem, TProperty>)GetItemHelper( listeners );
                if ( null != item )
                    item.OnPropertyValueChanged( sender, property, extraInfo );
            }
        }

        #endregion // RaisePropertyChanged<TItem, TProperty>

        #region RaiseValueChanged<TItem>

        internal static void RaiseValueChanged<TItem>( object listeners, TItem item )
        {
            ListenerList list = listeners as ListenerList;
            if ( null != list )
            {
                list._isTraversingList++;
                try
                {
                    for ( int i = 0, count = list.Count; i < count; i++ )
                    {
                        IValueChangeListener<TItem> ii = (IValueChangeListener<TItem>)GetItemHelper( list[i] );
                        if ( null != ii )
                            ii.OnValueChanged( item );
                    }
                }
                finally
                {
                    list._isTraversingList--;
                }
            }
            else if ( null != listeners )
            {
                IValueChangeListener<TItem> ii = (IValueChangeListener<TItem>)GetItemHelper( listeners );
                if ( null != ii )
                    ii.OnValueChanged( item );
            }
        }

        #endregion // RaiseValueChanged<TItem>
    }

    #endregion // ListenerList Class

    #region TypedPropertyChangeListenerList<TItem, TProperty> Class

    /// <summary>
    /// Used for managing property change listeners.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    internal class TypedPropertyChangeListenerList<TItem, TProperty> : ITypedPropertyChangeListener<TItem, TProperty>
    {
        private object _listeners;

        internal TypedPropertyChangeListenerList( )
        {
        }

        internal void Add( ITypedPropertyChangeListener<TItem, TProperty> listener, bool useWeakReference )
        {
            _listeners = ListenerList.Add( _listeners, listener, useWeakReference );
        }

        internal void Remove( ITypedPropertyChangeListener<TItem, TProperty> listener )
        {
            _listeners = ListenerList.Remove( _listeners, listener );
        }

        public virtual void OnPropertyValueChanged( TItem sender, TProperty property, object extraInfo )
        {
            if ( null != _listeners )
                ListenerList.RaisePropertyChanged<TItem, TProperty>( _listeners, sender, property, extraInfo );
        }
    }

    #endregion // TypedPropertyChangeListenerList<TItem, TProperty> Class

    #region PropertyChangeListenerList Class

    /// <summary>
    /// Used for managing property change listeners.
    /// </summary>
    internal class PropertyChangeListenerList : TypedPropertyChangeListenerList<object, string>, IPropertyChangeListener
    {
		#region Constructor

		public PropertyChangeListenerList( )
		{
		} 

		#endregion // Constructor

		#region ManageListenerHelper

		internal static void ManageListenerHelper<T>( ref T val, T newVal, IPropertyChangeListener listener, bool useWeakReference )
			where T : ISupportPropertyChangeNotifications
		{
			if ( val != null )
				val.RemoveListener( listener );

			val = newVal;

			if ( val != null )
				val.AddListener( listener, useWeakReference );
		}

		internal static void ManageListenerHelperObj<T>( ref T val, T newVal, IPropertyChangeListener listener, bool useWeakReference )
		{
			ISupportPropertyChangeNotifications vcn = val as ISupportPropertyChangeNotifications;
			if ( vcn != null )
				vcn.RemoveListener( listener );

			val = newVal;

			vcn = newVal as ISupportPropertyChangeNotifications;
			if ( vcn != null )
				vcn.AddListener( listener, useWeakReference );
		}

		#endregion // ManageListenerHelper

		#region OnCollectionChanged

		internal void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			this.OnPropertyValueChanged( sender, e.Action.ToString( ), e );
		} 

		#endregion // OnCollectionChanged
    }

    #endregion // PropertyChangeListenerList Class

    #region PropertyChangeListener<TOwner> Class




    internal class PropertyChangeListener<TOwner> : IPropertyChangeListener
        where TOwner : class
    {
        private object _wwOwner;
        private Action<TOwner, object, string, object> _action;








        public PropertyChangeListener( TOwner owner, Action<TOwner, object, string, object> action, bool useWeakReference 

			= true 

			)
        {
            CoreUtilities.ValidateNotNull( owner );

            _wwOwner = useWeakReference ? (object)new WeakReference( owner ) : owner;
            _action = action;
        }

        public TOwner Owner
        {
            get
            {
                return (TOwner)ListenerList.GetItemHelper( _wwOwner );
            }
        }

        public virtual void OnPropertyValueChanged( object dataItem, string property, object extraInfo )
        {
            TOwner owner = this.Owner;
            if ( null != owner && null != _action )
                _action( owner, dataItem, property, extraInfo );
        }
    }
    #endregion // PropertyChangeListener<TOwner> Class
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