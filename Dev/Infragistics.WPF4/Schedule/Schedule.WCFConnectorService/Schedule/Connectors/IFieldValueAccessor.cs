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
using System.Globalization;
using System.Linq;


#pragma warning disable 0649



using Infragistics.Services;
using Infragistics.Collections.Services;

namespace Infragistics.Services





{
    #region IFieldValueAccessor Interface

    internal interface IFieldValueAccessor
    {
        object GetValue( object dataItem );
        void SetValue( object dataItem, object newValue );
    }

    #endregion // IFieldValueAccessor Interface

    #region IFieldValueAccessor<T> Interface

    internal interface IFieldValueAccessor<T> : IFieldValueAccessor
    {
        new T GetValue( object dataItem );
        void SetValue( object dataItem, T newValue );
    }

    #endregion // IFieldValueAccessor<T> Interface

    #region IPropertyStore Interface

    internal interface IPropertyStore
    {
        object GetValue( object item );
        void SetValue( object item, object newValue );
		bool HasValue( object item );
    }

    #endregion // IPropertyStore Interface

    #region IPropertyStore<TItem, TValue> Interface

    internal interface IPropertyStore<TItem, TValue> : IPropertyStore
    {
        TValue GetValue( TItem item );
        void SetValue( TItem item, TValue newValue );
		bool HasValue( TItem item );
    }

    #endregion // IPropertyStore<TItem, TValue> Interface

	#region IPropertyStorage<TItem, TProperty> Interface

	internal interface IPropertyStorage<TItem, TProperty>
	{
		T GetValue<T>( TItem item, TProperty property );
		void SetValue<T>( TItem item, TProperty property, T newValue );
		bool HasValue( TItem item, TProperty property );
	}

	#endregion // IPropertyStorage<TItem, TProperty> Interface

	#region ConverterInfo Class

	internal class ConverterInfo
	{
		internal IValueConverter _converter;
		internal object _parameter;
		internal CultureInfo _culture;

		/// <summary>
		/// Type of the converted values. Used as the target type for the Convert method.
		/// </summary>
		internal Type _targetValueType;

		internal object Convert( object val )
		{
			return null != _converter
				? _converter.Convert( val, _targetValueType, _parameter, _culture )
				: val;
		}

		internal object ConvertBack( object val, Type targetType )
		{
			return null != _converter
				? _converter.ConvertBack( val, targetType, _parameter, _culture )
				: val;
		}
	} 

	#endregion // ConverterInfo Class
}


namespace Infragistics.Controls.Schedules.Services



{
	#region MetadataPropertyValueStore

	/// <summary>
	/// Used to store extra field values.
	/// </summary>
	/// <remarks>
	/// <para class="body">
	/// You can use the <b>Metadata</b> to store and later retrieve information. The information is stored in
	/// properties of your data items as specified in the <see cref="MetadataPropertyMappingCollection"/>.
	/// For example, the metadata property mappings is specified using the <see cref="AppointmentPropertyMappingCollection"/>'s
	/// <see cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.MetadataPropertyMappings"/>
	/// property. Each property defined in the mapping collection has a corresponding entry in the returned MetadataPropertyValueStore.
	/// Its indexer is used to retrieve or set the the property's value. Furthermore, you can use bindings to
	/// bind to a specific value in the returned object.
	/// </para>
	/// </remarks>
	/// <seealso cref="MetadataPropertyMappingCollection"/>
	/// <seealso cref="PropertyMappingCollection&lt;TKey, TMapping&gt;.MetadataPropertyMappings"/>
	/// <seealso cref="ActivityBase.Metadata"/>
	/// <seealso cref="Resource.Metadata"/>
	/// <seealso cref="ResourceCalendar.Metadata"/>
	public abstract class MetadataPropertyValueStore : PropertyChangeNotifierExtended, IEnumerable<KeyValuePair<string, object>>
	{
		#region Indexer

		/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <param name="key">Key associated with the value.</param>
		/// <returns>Gets the value for the specified key or null if the value doesn't exist for the key.</returns>
		/// <remarks>
		/// <para class="body">
		/// <b>NOTE:</b> If the underlying metadata field mapping doesn't exist for the specified 'key' then an exception is thrown.
		/// </para>
		/// </remarks>
		public object this[string key]
		{
			get
			{
				return this.GetValue( key );
			}
			set
			{
				this.SetValue( key, value );
			}
		}

		#endregion // Indexer

		#region Add

		// SSP 10/20/10 TFS57589
		// An Add method is necessary to support XmlSerializer.
		// 
		/// <summary>
		/// Stores the specified entry's Value using the entry's Key as the key.
		/// </summary>
		/// <param name="entry">Contains 'key' and 'value'.</param>
		public void Add( KeyValuePair<string, object> entry )
		{
			this[entry.Key] = entry.Value;
		} 

		#endregion // Add

		#region GetEnumerator

		/// <summary>
		/// Returns the enumerator to enumerate values of the store.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerator<KeyValuePair<string, object>> GetEnumerator( ); 

		#endregion // GetEnumerator

		#region ClearAllValues

		internal virtual void ClearAllValues( )
		{
			List<string> keys = ( from ii in this select ii.Key ).ToList( );
			foreach ( string key in keys )
				this[key] = null;
		} 

		#endregion // ClearAllValues

		#region CopyValuesFrom

		internal void CopyValuesFrom( MetadataPropertyValueStore source, StoragePropsInfo.MergeMethod mergeMethod )
		{
			if ( StoragePropsInfo.MergeMethod.CopyAll == mergeMethod )
				this.ClearAllValues( );

			foreach ( KeyValuePair<string, object> ii in source )
			{
				object val = ii.Value;
				if ( StoragePropsInfo.MergeMethod.CopyAll == mergeMethod
					|| !ScheduleUtilities.IsValueEmpty( val ) )
				{
					this[ii.Key] = val;
				}
			}
		} 

		#endregion // CopyValuesFrom

		#region GetHashCode

		internal static int GetHashCode( MetadataPropertyValueStore x )
		{
			int h = 0;
			foreach ( var ii in x )
			{
				h ^= ii.Key.GetHashCode( );

				if ( null != ii.Value )
					h ^= null != ii.Value ? ii.Value.GetHashCode( ) : 0;
			}

			return h;
		}

		#endregion // GetHashCode

		#region GetKeysWithNonEmptyValues

		private HashSet<string> GetKeysWithNonEmptyValues( )
		{
			return new HashSet<string>( from ii in this where !CoreUtilities.IsValueEmpty( ii.Value ) select ii.Key );
		}

		#endregion // GetKeysWithNonEmptyValues

		#region GetValue

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="key">String that identifies the key.</param>
		/// <returns>Value associated with the key. If no such value exists, returns null.</returns>
		protected abstract object GetValue( string key );

		#endregion // GetValue

		#region HasValue

		// SSP 7/18/12 TFS100159
		// 
		/// <summary>
		/// Indicates if the value for the specified key is available or non-empty.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		internal virtual bool HasValue( string key )
		{
			object v = this.GetValue( key );
			return !ScheduleUtilities.IsValueEmpty( v );
		} 

		#endregion // HasValue

		#region HasNonEmptyValue

		internal bool HasNonEmptyValue( )
		{
			foreach ( var ii in this )
			{
				if ( !CoreUtilities.IsValueEmpty( ii.Value ) )
					return true;
			}

			return false;
		} 

		#endregion // HasNonEmptyValue

		#region HasSameValues

		internal static bool HasSameValues( MetadataPropertyValueStore x, MetadataPropertyValueStore y )
		{
			if ( x != y )
			{
				HashSet<string> xxKeysWithValues = null != x ? x.GetKeysWithNonEmptyValues( ) : new HashSet<string>( );
				HashSet<string> yyKeysWithValues = null != y ? y.GetKeysWithNonEmptyValues( ) : new HashSet<string>( );

				if ( xxKeysWithValues.SetEquals( yyKeysWithValues ) )
				{
					foreach ( string ii in xxKeysWithValues )
					{
						if ( !object.Equals( x[ii], y[ii] ) )
							return false;
					}
				}
			}

			return true;
		} 

		#endregion // HasSameValues

		#region SetValue

		/// <summary>
		/// Sets the specified value for the speicifed key.
		/// </summary>
		/// <param name="key">String that identifies the key.</param>
		/// <param name="value">Value to associate with the key.</param>
		protected abstract void SetValue( string key, object value );

		#endregion // SetValue

		#region IEnumerable Implementation

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion // IEnumerable Implementation
	} 

	#endregion // MetadataPropertyValueStore

	#region DictionaryMetadataPropertyValueStore Class

	internal class DictionaryMetadataPropertyValueStore : MetadataPropertyValueStore
	{
		#region Member Vars

		private Dictionary<string, object> _values; 

		#endregion // Member Vars

		#region Constructor

		public DictionaryMetadataPropertyValueStore( )
			: this( null )
		{
		}

		public DictionaryMetadataPropertyValueStore( MetadataPropertyValueStore source )
		{
			_values = new Dictionary<string, object>( );

			if ( null != source )
				this.CopyValuesFrom( source, StoragePropsInfo.MergeMethod.CopyAll );
		}


		#endregion // Constructor

		#region GetValue

		protected override object GetValue( string key )
		{
			object val;
			if ( _values.TryGetValue( key, out val ) )
				return val;

			return null;
		} 

		#endregion // GetValue

		#region HasValue

		// SSP 7/18/12 TFS100159
		// 
		/// <summary>
		/// Indicates if the value for the specified key is available or non-empty.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		internal override bool HasValue( string key )
		{
			return _values.ContainsKey( key );
		}

		#endregion // HasValue

		#region SetValue

		protected override void SetValue( string key, object value )
		{
			// SSP 7/18/12 TFS100159
			// We need to raise notification.
			// 
			
			object oldValue;
			if ( !_values.TryGetValue( key, out oldValue ) || !object.Equals( oldValue, value ) )
			{
				_values[key] = value;

				this.RaisePropertyChangedEvent( string.Empty );
			}
		} 

		#endregion // SetValue

		#region GetEnumerator

		public override IEnumerator<KeyValuePair<string, object>> GetEnumerator( )
		{
			return _values.GetEnumerator( );
		} 

		#endregion // GetEnumerator
	} 

	#endregion // DictionaryMetadataPropertyValueStore Class

	#region AntiRecursionList Class

	internal class AntiRecursionList
	{
		#region Member Vars

		private object _items; 

		#endregion // Member Vars

		#region Methods

		#region Internal Methods

		#region Enter

		internal bool Enter( object item )
		{
			if ( null == _items )
			{
				_items = item;
				return true;
			}
			else if ( _items == item )
			{
				return false;
			}
			else
			{
				Dictionary<object, bool> map = _items as Dictionary<object, bool>;
				if ( null != map && map.ContainsKey( item ) )
					return false;

				if ( null == map )
				{
					map = new Dictionary<object, bool>( );
					map[_items] = true;
					_items = map;
				}

				map[item] = true;
				return true;
			}
		}

		#endregion // Enter

		#region Exit

		internal void Exit( object item )
		{
			if ( _items == item )
			{
				_items = null;
			}
			else
			{
				Dictionary<object, bool> map = _items as Dictionary<object, bool>;
				Debug.Assert( null != map && map.ContainsKey( item ), "Enter wasn't called on the item." );

				if ( null != map )
					map.Remove( item );
			}
		}

		#endregion // Exit

		#region InProgress

		internal bool InProgress( object item )
		{
			if ( null != _items )
			{
				if ( _items == item )
				{
					return true;
				}
				else
				{
					Dictionary<object, bool> map = _items as Dictionary<object, bool>;
					if ( null != map && map.ContainsKey( item ) )
						return true;

				}
			}

			return false;
		}

		#endregion // InProgress  

		#endregion // Internal Methods

		#endregion // Methods
	}

	#endregion // AntiRecursionList Class

	#region ScheduleTimeSpanConverter Class

	// SSP 10/28/10 TFS57603
	// Added time-span converter that converts from long to time-span and also from string to time-span.
	// 
	internal class ScheduleTimeSpanConverter : IValueConverter
	{
		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( null == value || value is TimeSpan )
				return value;

			if ( value is long )
			{
				long n = (long)value;
				return new TimeSpan( n );
			}

			string str = value as string;
			if ( null != str )
			{
				TimeSpan span;
				if ( TimeSpan.TryParse( str, out span ) )
					return span;
			}

			return value;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( value is TimeSpan )
			{
				TimeSpan span = (TimeSpan)value;

				if ( typeof( long ) == targetType )
				{
					return span.Ticks;
				}

				if ( typeof( string ) == targetType )
				{
					return span.ToString( null, culture );
				}
			}

			return value;
		}
	}

	#endregion // ScheduleTimeSpanConverter Class

	#region DelegateFieldValueAccessor Class

	// SSP 1/7/11 - NAS11.1 Activity Categories
	// 
	internal class DelegateFieldValueAccessor<TValue> : IFieldValueAccessor<TValue>
	{
		private Func<object, TValue> _getter;
		private Action<object, TValue> _setter;

		public DelegateFieldValueAccessor( Func<object, TValue> getter, Action<object, TValue> setter )
		{
			_getter = getter;
			_setter = setter;
		}

		public TValue GetValue( object dataItem )
		{
			return _getter( dataItem );
		}

		public void SetValue( object dataItem, TValue newValue )
		{
			if ( null == _setter )
				throw new InvalidOperationException( "Field is read-only." );

			_setter( dataItem, newValue );
		}

		object IFieldValueAccessor.GetValue( object dataItem )
		{
			return this.GetValue( dataItem );
		}

		public void SetValue( object dataItem, object newValue )
		{
			this.SetValue( dataItem, (TValue)newValue );
		}
	} 

	#endregion // DelegateFieldValueAccessor Class

	#region DelegatePropertyStore Class

	// SSP 12/9/11 - NAS11.1 Activity Categories
	// 

	internal class DelegatePropertyStore<TItem, TValue> : SupportValueChangeNotificationsImpl<TItem>, IPropertyStore<TItem, TValue>
	{
		private Func<TItem, TValue> _getter;
		private Action<TItem, TValue> _setter;
		private EqualityComparer<TValue> _equalityComparer;
		// SSP 5/24/12 XamGantt
		// 
		Func<TItem, bool> _hasValue;
		private bool _raisePropertyChangeNotifications;

		public DelegatePropertyStore( Func<TItem, TValue> getter, Action<TItem, TValue> setter, Func<TItem, bool> hasValue = null, bool raisePropertyChangeNotifications = true )
		{
			CoreUtilities.ValidateNotNull( getter );
			CoreUtilities.ValidateNotNull( setter );

			_getter = getter;
			_setter = setter;
			_hasValue = hasValue;
			_equalityComparer = EqualityComparer<TValue>.Default;
			_raisePropertyChangeNotifications = raisePropertyChangeNotifications;
		}

		public TValue GetValue( TItem item )
		{
			return _getter( item );
		}

		public void SetValue( TItem item, TValue newValue )
		{
			// SSP 5/24/12 XamGantt
			// Enclosed the existing code into the if block and added the else block.
			// 
			if ( _raisePropertyChangeNotifications )
			{
				// Check to see if the value actually changed before raising the event.
				// 
				TValue oldValue = this.GetValue( item );
				bool changed = !_equalityComparer.Equals( oldValue, newValue );

				_setter( item, newValue );

				if ( changed )
					this.NotifyPropertyChangeListeners( item );
			}
			else
			{
				_setter( item, newValue );
			}
		}

		public virtual bool HasValue( TItem item )
		{
			// SSP 5/24/12 XamGantt
			// Added _hasValue.
			// 
			//return !EqualityComparer<TValue>.Default.Equals( default( TValue ), this.GetValue( item ) );
			return null != _hasValue 
				? _hasValue( item ) 
				: ! EqualityComparer<TValue>.Default.Equals( default( TValue ), this.GetValue( item ) );
		}

		public object GetValue( object item )
		{
			return this.GetValue( (TItem)item );
		}

		public void SetValue( object item, object newValue )
		{
			this.SetValue( (TItem)item, null != newValue ? (TValue)newValue : default( TValue ) );
		}

		public bool HasValue( object item )
		{
			return this.HasValue( (TItem)item );
		}
	} 

	#endregion // DelegatePropertyStore Class

	#region DelegateValueConverter Class

	internal class DelegateValueConverter : IValueConverter
	{
		private Func<object, Type, object, CultureInfo, object> _convert;
		private Func<object, Type, object, CultureInfo, object> _convertBack;

		internal DelegateValueConverter( Func<object, Type, object, CultureInfo, object> convert, Func<object, Type, object, CultureInfo, object> convertBack )
		{
			_convert = convert;
			_convertBack = convertBack;
		}

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			object ret = _convert( value, targetType, parameter, culture );

			if ( ret is DataErrorInfo )
			{
				
				return null;
			}

			return ret;
		}

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			object ret = _convertBack( value, targetType, parameter, culture );

			if ( ret is DataErrorInfo )
			{
				
				return null;
			}

			return ret;
		}
	}

	#endregion // DelegateValueConverter Class

	#region FieldValueAccessor_Binding Class

	internal class FieldValueAccessor_Binding : IFieldValueAccessor
	{
		private ValueGetter _valueGetter;

		public FieldValueAccessor_Binding( string field, ConverterInfo converterInfo )
		{
			_valueGetter = new ValueGetter( field, converterInfo );
		}

		public object GetValue( object dataItem )
		{
			return _valueGetter.GetValueForDataItem( dataItem );
		}

		public void SetValue( object dataItem, object value )
		{
			_valueGetter.SetValueOnDataItem( dataItem, value );
		}
	}

	#endregion // FieldValueAccessor_Binding Class

	#region Index Class

	internal class Index<TItem, TKey> : IEnumerable<TItem>
		where TItem : class
	{
		#region Member Vars

		private IList<TItem> _items;
		private Converter<TItem, TKey> _keyGetter;
		private IComparer<TKey> _keyComparer;
		private List<TItem> _sortedItems;
		private IComparer<TItem> _itemComparer;
		private Action<NotifyCollectionChangedEventArgs> _itemsChangedCallback;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="items">Items to be indexed.</param>
		/// <param name="keyGetter">Gets the key value from an item.</param>
		/// <param name="keyComparer">Comparer used for comparing key values.</param>
		/// <param name="itemsChangedCallback">If specified, it's invoked when a collection change notification is 
		/// recieved from the items list, after the index has been modified to reflect the change.</param>
		internal Index( IList<TItem> items, Converter<TItem, TKey> keyGetter, IComparer<TKey> keyComparer,
			Action<NotifyCollectionChangedEventArgs> itemsChangedCallback = null )
		{
			CoreUtilities.ValidateNotNull( items );
			CoreUtilities.ValidateNotNull( keyGetter );
			CoreUtilities.ValidateNotNull( keyComparer );

			_items = items;
			_keyGetter = keyGetter;
			_keyComparer = keyComparer;
			_itemsChangedCallback = itemsChangedCallback;

			_itemComparer = new CoreUtilities.ConverterComparer<TItem, TKey>( _keyGetter, _keyComparer );

			this.HookIntoChildList( );
		}

		#endregion // Constructor

		#region Properties

		#region Private Properties

		#region IsIndexDirty

		private bool IsIndexDirty
		{
			get
			{
				return null == _sortedItems;
			}
		}

		#endregion // IsIndexDirty

		#endregion // Private Properties

		#endregion // Properties

		#region Methods

		#region Public Methods

		#region GetAnyMatchingItem

		/// <summary>
		/// Gets any matching item. If a single matching item exists, returns that. If multiple matching
		/// items exist then this method may return any one of them.
		/// </summary>
		/// <param name="key">The key value.</param>
		/// <returns>Matching item.</returns>
		public TItem GetAnyMatchingItem( TKey key )
		{
			int index = this.GetIndexHelper( key, false );
			if ( index >= 0 )
			{
				return _sortedItems[index];
			}

			return default( TItem );
		}

		#endregion // GetAnyMatchingItem

		#region GetEnumerator

		public IEnumerator<TItem> GetEnumerator( )
		{
			this.VerifyIndex( );
			return _sortedItems.GetEnumerator( );
		}

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion // GetEnumerator

		#region GetMatchingItems

		/// <summary>
		/// Gets all the matching items.
		/// </summary>
		/// <param name="key">The key value.</param>
		/// <returns>All the matching items.</returns>
		public IEnumerable<TItem> GetMatchingItems( TKey key )
		{
			int index = this.GetIndexHelper( key, false );
			if ( index >= 0 )
			{
				int startIndex = index;
				int endIndex = index;

				while ( startIndex > 0 && 0 == _keyComparer.Compare( key, _keyGetter( _sortedItems[startIndex - 1] ) ) )
					startIndex--;

				while ( endIndex + 1 < _sortedItems.Count && 0 == _keyComparer.Compare( key, _keyGetter( _sortedItems[endIndex + 1] ) ) )
					endIndex++;

				return new TypedEnumerable<TItem>( new ListSection( _sortedItems, startIndex, endIndex - startIndex + 1 ) );
			}

			return null;
		}

		#endregion // GetMatchingItems

		#endregion // Public Methods

		#region Private Methods

		#region AddItemToIndex

		private void AddItemToIndex( TItem item )
		{
			if ( !this.IsIndexDirty )
			{
				int index = this.GetIndexHelper( _keyGetter( item ), true );
				_sortedItems.Insert( index, item );
			}
		}

		#endregion // AddItemToIndex

		#region ConstructIndex

		private List<TItem> ConstructIndex( )
		{
			int count = _items.Count;
			List<TItem> sortedItems = new List<TItem>( _items );

			CoreUtilities.SortMergeGeneric<TItem>( sortedItems, _itemComparer );

			return sortedItems;
		}

		#endregion // ConstructIndex

		#region DirtyIndex

		private void DirtyIndex( )
		{
			_sortedItems = null;
		}

		#endregion // DirtyIndex

		#region HookIntoChildList

		private void HookIntoChildList( )
		{
			INotifyCollectionChanged notifyCollection = _items as INotifyCollectionChanged;
			if ( null != notifyCollection )
			{
				notifyCollection.CollectionChanged += new NotifyCollectionChangedEventHandler( OnListChanged );
			}
		}

		#endregion // HookIntoChildList

		#region GetIndexHelper

		private int GetIndexHelper( TKey key, bool findInsertIndex )
		{
			this.VerifyIndex( );

			List<TItem> arr = _sortedItems;

			int i = 0, j = arr.Count - 1;
			int m = -1;
			int r = 0;

			while ( i <= j )
			{
				m = ( i + j ) / 2;

				TKey iiVal = _keyGetter( arr[m] );
				r = _keyComparer.Compare( key, iiVal );

				if ( r < 0 )
					j = m - 1;
				else if ( r > 0 )
					i = m + 1;
				else
					break;
			}

			return i <= j ? m : ( findInsertIndex ? ( r < 0 ? m : 1 + m ) : -1 );
		}

		#endregion // GetIndexHelper

		#region OnListChanged

		private void OnListChanged( object sender, NotifyCollectionChangedEventArgs e )
		{
			if ( !this.IsIndexDirty )
			{
				switch ( e.Action )
				{
					case NotifyCollectionChangedAction.Add:
						{
							foreach ( TItem ii in e.NewItems )
								this.AddItemToIndex( ii );
						}
						break;
					case NotifyCollectionChangedAction.Remove:
						{
							foreach ( TItem ii in e.OldItems )
								this.RemoveItemFromIndex( ii );
						}
						break;
					default:
						this.DirtyIndex( );
						break;
				}
			}

			if ( null != _itemsChangedCallback )
				_itemsChangedCallback( e );
		}

		#endregion // OnListChanged

		#region RemoveItemFromIndex

		private void RemoveItemFromIndex( TItem item )
		{
			if ( null != _sortedItems )
				_sortedItems.Remove( item );
		}

		#endregion // RemoveItemFromIndex

		#region VerifyIndex

		private void VerifyIndex( )
		{
			if ( null == _sortedItems )
				_sortedItems = this.ConstructIndex( );
		}

		#endregion // VerifyIndex

		#endregion // Private Methods

		#endregion // Methods
	} 

	#endregion // Index Class

	#region FieldValueAccessorWrapper

	internal class FieldValueAccessorWrapper : IFieldValueAccessor
	{
		internal IFieldValueAccessor _fva;

		public object GetValue( object dataItem )
		{
			return null != _fva ? _fva.GetValue( dataItem ) : null;
		}

		public void SetValue( object dataItem, object newValue )
		{
			if ( null != _fva )
				_fva.SetValue( dataItem, newValue );
		}
	}

	internal class AggregateTupleFieldValueAccessorWrapper<T1, T2> : IFieldValueAccessor
	{
		internal IFieldValueAccessor _fva1;
		internal IFieldValueAccessor _fva2;

		public object GetValue( object dataItem )
		{
			T1 v1 = (T1)_fva1.GetValue( dataItem );
			T2 v2 = (T2)_fva2.GetValue( dataItem );

			return Tuple.Create( v1, v2 );
		}

		public void SetValue( object dataItem, object newValue )
		{
			Tuple<T1, T2> tuple = (Tuple<T1, T2>)newValue;
			_fva1.SetValue( dataItem, tuple.Item1 );
			_fva2.SetValue( dataItem, tuple.Item2 );
		}
	}

	#endregion // FieldValueAccessorWrapper

	#region ConvertBackFieldValueAccessor

	internal class ConvertBackFieldValueAccessor : IFieldValueAccessor
	{
		private IFieldValueAccessor _source;
		private ConverterInfo _converterInfo;
		private Type _convertType;

		internal ConvertBackFieldValueAccessor( IFieldValueAccessor source, ConverterInfo converterInfo, Type convertType )
		{
			_source = source;
			_converterInfo = converterInfo;
			_convertType = convertType;
		}

		public object GetValue( object dataItem )
		{
			object val = _source.GetValue( dataItem );

			if ( null != _converterInfo )
				val = _converterInfo.ConvertBack( val, _convertType );

			return val;
		}

		public void SetValue( object dataItem, object newValue )
		{
			if ( null != _converterInfo )
				newValue = _converterInfo.Convert( newValue );

			_source.SetValue( dataItem, newValue );
		}
	} 

	#endregion // ConvertBackFieldValueAccessor

	#region RelationPropertyStore Class

	internal class RelationPropertyStore<TParent, TChild, TValue> : IPropertyStore<TParent, TValue>
		where TParent : class
		where TChild : class
		where TValue : IList<TChild>, new( )
	{
		private IList<TParent> _parentList;
		private IList<TChild> _childList;
		private IPropertyStore _valueStore;
		private IPropertyStore _relationParentId;
		private IPropertyStore _relationChildId;
		private Index<TParent, object> _parentIndex;
		private Index<TChild, object> _childIndex;
		private IDefaultValueFactory<TParent, TValue> _defaultValueFactory;
		private Action<TChild, TParent> _parentReferenceSetter;
		private Action<TValue, IEnumerable<TChild>> _replaceChildCollectionContents;

		internal RelationPropertyStore( IList<TParent> parentList, IPropertyStore relationParentId,
			IList<TChild> childList, IPropertyStore relationChildId, IPropertyStore valueStore,
			IDefaultValueFactory<TParent, TValue> defaultValueFactory,
			Action<TChild, TParent> parentReferenceSetter,
			Action<TValue, IEnumerable<TChild>> replaceChildCollectionContents = null
			)
		{
			_parentList = parentList;
			_childList = childList;
			_valueStore = valueStore;
			_relationParentId = relationParentId;
			_relationChildId = relationChildId;
			_defaultValueFactory = defaultValueFactory;
			_parentReferenceSetter = parentReferenceSetter;
			_replaceChildCollectionContents = replaceChildCollectionContents;

			_parentIndex = new Index<TParent, object>( _parentList,
				delegate( TParent parent )
				{
					return _relationParentId.GetValue( parent );
				},
				Comparer<object>.Default,
				this.OnParentListChanged
			);

			_childIndex = new Index<TChild, object>( _childList,
				delegate( TChild child )
				{
					return _relationChildId.GetValue( child );
				},
				Comparer<object>.Default,
				this.OnChildListChanged
			);
		}

		internal IPropertyStore ValueStore
		{
			get
			{
				return _valueStore;
			}
		}

		private TParent GetParentFromId( object id )
		{
			return _parentIndex.GetAnyMatchingItem( id );
		}

		private TParent GetParentPointer( TChild child )
		{
			object id = _relationChildId.GetValue( child );
			return _parentIndex.GetAnyMatchingItem( child );
		}

		private void ProcessAddChild( TChild child )
		{
			object id = _relationChildId.GetValue( child );
			TParent parent = this.GetParentFromId( id );
			if ( null != parent )
			{
				TValue children = (TValue)_valueStore.GetValue( parent );

				// We only need to add to children collection if it has been allocated yet. If it hasn't been
				// allocated yet then when it is allocated, it will be populated with all the matching child 
				// items and threfore we should not do anything here.
				// 
				if ( null != children )
				{
					children.Add( child );
					this.SetParentReferenceOnChild( child, parent );
				}
			}
		}

		private void SetParentReferenceOnChild( TChild child, TParent parent )
		{
			if ( null != _parentReferenceSetter )
				_parentReferenceSetter( child, parent );
		}

		private void ProcessRemoveChild( TChild child )
		{
			TParent parent = this.GetParentPointer( child );
			if ( null != parent )
			{
				TValue children = (TValue)_valueStore.GetValue( parent );
				if ( null != children )
				{
					children.Remove( child );
					this.SetParentReferenceOnChild( child, null );
				}
			}
		}

		private void RefreshChildItems( TParent parent )
		{
			this.GetValueHelper( parent, false, true );
		}

		private void RefreshChildItems( TParent parent, TValue childColl )
		{
			IEnumerable<TChild> matchingChildItems = _childIndex.GetMatchingItems( _relationParentId.GetValue( parent ) );

			if ( null != matchingChildItems )
			{
				foreach ( TChild ii in matchingChildItems )
					this.SetParentReferenceOnChild( ii, parent );
			}

			if ( null != _replaceChildCollectionContents )
			{
				_replaceChildCollectionContents( childColl, matchingChildItems );
			}
			else
			{
				childColl.Clear( );

				if ( null != matchingChildItems )
				{
					foreach ( TChild ii in matchingChildItems )
						childColl.Add( ii );
				}
			}
		}

		private TValue GetValueHelper( TParent parent, bool allocateIfNecessary, bool refreshChildItems )
		{
			TValue childColl = (TValue)_valueStore.GetValue( parent );
			if ( null == childColl && allocateIfNecessary )
			{
				childColl = null != _defaultValueFactory ? _defaultValueFactory.CreateDefaultValue( parent ) : new TValue( );
				_valueStore.SetValue( parent, childColl );

				// Since we've just allocated the child items collection, we need to populate it.
				// 
				refreshChildItems = true;
			}

			if ( null != childColl && refreshChildItems )
				this.RefreshChildItems( parent, childColl );

			return childColl;
		}

		private void OnParentListChanged( NotifyCollectionChangedEventArgs e )
		{
		}

		private void OnChildListChanged( NotifyCollectionChangedEventArgs e )
		{
			NotifyCollectionChangedAction action = e.Action;

			switch ( e.Action )
			{
				case NotifyCollectionChangedAction.Add:
					{
						foreach ( TChild ii in e.NewItems )
						{
							if ( null != ii )
								this.ProcessAddChild( ii );
						}
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						foreach ( TChild ii in e.OldItems )
						{
							if ( null != ii )
								this.ProcessRemoveChild( ii );
						}
					}
					break;
				default:
					{
						

						foreach ( TParent parent in _parentList )
							this.RefreshChildItems( parent );
					}
					break;
			}
		}

		public TValue GetValue( TParent parent )
		{
			return this.GetValueHelper( parent, true, false );
		}

		public bool HasValue( TParent parent )
		{
			return _valueStore.HasValue( parent );
		}

		private IEnumerable GetChildren( object id )
		{
			
			return from ii in _childList where object.Equals( id, _relationChildId.GetValue( ii ) ) select ii;
		}

		public void SetValue( TParent item, TValue newValue )
		{
			
			//throw new InvalidOperationException( );
		}

		object IPropertyStore.GetValue( object item )
		{
			return this.GetValue( (TParent)item );
		}

		void IPropertyStore.SetValue( object item, object newValue )
		{
			this.SetValue( (TParent)item, null != newValue ? (TValue)newValue : default( TValue ) );
		}

		bool IPropertyStore.HasValue( object item )
		{
			return this.HasValue( (TParent)item );
		}
	} 

	#endregion // RelationPropertyStore Class

	#region DictionaryPropertyStore<TItem, TValue> Class

	internal class DictionaryPropertyStore<TItem, TValue> : SupportValueChangeNotificationsImpl<TItem>, IPropertyStore<TItem, TValue>
    {
        #region Member Vars

        private WeakDictionary<TItem, TValue> _values;
		private IEqualityComparer<TValue> _equalityComparer;
		private IDefaultValueFactory _defaultValueFactory;

        #endregion // Member Vars

        #region Constructor

		public DictionaryPropertyStore( IDefaultValueFactory defaultValueFactory = null )
        {
            _values = new WeakDictionary<TItem, TValue>( true, false );

			_defaultValueFactory = defaultValueFactory;
			
			_equalityComparer = EqualityComparer<TValue>.Default;
        } 

        #endregion // Constructor

        #region GetValue

        public virtual TValue GetValue( TItem item )
        {
            TValue val;
			if ( !_values.TryGetValue( item, out val ) )
			{
				if ( null != _defaultValueFactory )
					_values[item] = val = (TValue)_defaultValueFactory.CreateDefaultValue( item );
			}

            return val;
        }

        #endregion // GetValue

        #region SetValue

        public virtual void SetValue( TItem item, TValue newValue )
        {
			// Check to see if the value actually changed before raising the event.
			// 
			TValue oldValue;
			_values.TryGetValue( item, out oldValue );
			bool changed = ! _equalityComparer.Equals( oldValue, newValue );

			if ( _equalityComparer.Equals( newValue, default( TValue ) ) )
				_values.Remove( item );
			else							
				_values[item] = newValue;

			if ( changed )
				this.NotifyPropertyChangeListeners( item );
        }

        #endregion // SetValue

		#region HasValue

		public virtual bool HasValue( TItem item )
		{
			return _values.ContainsKey( item );
		} 

		#endregion // HasValue

        #region IPropertyStore Members

        object IPropertyStore.GetValue( object item )
        {
            return this.GetValue( (TItem)item );
        }

        void IPropertyStore.SetValue( object item, object newValue )
        {
            this.SetValue( (TItem)item, null != newValue ? (TValue)newValue : default( TValue ) );
        }

		bool IPropertyStore.HasValue( object item )
		{
			return this.HasValue( (TItem)item );
		}

        #endregion
    }

	#endregion // DictionaryPropertyStore<TItem, TValue> Class

	#region IDefaultValueFactory<TItem, TValue> Interface

	internal interface IDefaultValueFactory
	{
		object CreateDefaultValue( object item );
	}

	internal interface IDefaultValueFactory<TItem, TValue> : IDefaultValueFactory
	{
		TValue CreateDefaultValue( TItem item );
	}

	#endregion // IDefaultValueFactory<TItem, TValue> Interface

	#region DictionaryPropertyStorage<TItem, TProperty> Class

	internal class DictionaryPropertyStorage<TItem, TProperty> : IPropertyStorage<TItem, TProperty>
    {
        private Dictionary<TProperty, object> _values;
        private ITypedPropertyChangeListener<TItem, TProperty> _propsChangeListener;
		private IMap<TProperty, IDefaultValueFactory> _defaultValueFactories;
		
        internal DictionaryPropertyStorage( 
			ITypedPropertyChangeListener<TItem, TProperty> propsChangeListener = null,
			IMap<TProperty, IDefaultValueFactory> defaultValueFactories = null )
        {
            _propsChangeListener = propsChangeListener;
			_defaultValueFactories = defaultValueFactories;
        }

        private Dictionary<TProperty, object> Values
        {
            get
            {
                if ( null == _values )
                    _values = new Dictionary<TProperty, object>( );

                return _values;
            }
        }

        public T GetValue<T>( TItem item, TProperty property )
        {
            object val = null;
            if ( null == _values || !_values.TryGetValue( property, out val ) )
            {
				IDefaultValueFactory defValueFactory = null != _defaultValueFactories ? _defaultValueFactories[property] : null;
				if ( null != defValueFactory )
					this.Values[property] = val = (T)defValueFactory.CreateDefaultValue( item );
            }

            return null == val ? default( T ) : (T)val;
        }

        public void SetValue<T>( TItem item, TProperty property, T newValue )
        {
			// MD 1/20/11
			// Found while implementing NA 11.1 - Exchange Data Connector
			// We shouldn't se tthe value or fire the property changed callback unless the value has actually changed.
			if (EqualityComparer<T>.Default.Equals(this.GetValue<T>(item, property), newValue))
				return;

            this.Values[property] = newValue;

            if ( null != _propsChangeListener )
                _propsChangeListener.OnPropertyValueChanged( item, property, null );
        }

		public bool HasValue( TItem item, TProperty property )
		{
			return null != _values && _values.ContainsKey( property );
		}
    }

    #endregion // DictionaryPropertyStorage<TItem, TProperty> Class

    #region PropertyStorage<TItem, TProperty> Class

    internal class PropertyStorage<TItem, TProperty> : IPropertyStorage<TItem, TProperty>
    {
        #region Listener Class

        private class Listener : IValueChangeListener<TItem>
        {
			private PropertyStorage<TItem, TProperty> _storage;
            private TProperty _property;
            private ITypedPropertyChangeListener<TItem, TProperty> _propertyChangeListener;
            internal ISupportValueChangeNotifications<TItem> _source;

            internal Listener( PropertyStorage<TItem, TProperty> storage, TProperty property, ITypedPropertyChangeListener<TItem, TProperty> propertyChangeListener, ISupportValueChangeNotifications<TItem> source )
            {
				_storage = storage;
                _property = property;
                _propertyChangeListener = propertyChangeListener;
                _source = source;
            }

            public void OnValueChanged( TItem dataItem )
            {
                _propertyChangeListener.OnPropertyValueChanged( dataItem, _property, _storage );
            }
        }

        #endregion // Listener Class

        private IMap<TProperty, IPropertyStore> _propertyStores;
        private ITypedPropertyChangeListener<TItem, TProperty> _propertyChangeListener;
        private List<Listener> _listeners;

        internal PropertyStorage( IMap<TProperty, IPropertyStore> propertyStores, ITypedPropertyChangeListener<TItem, TProperty> propertyChangeListener )
        {
            _propertyStores = propertyStores;
            _propertyChangeListener = propertyChangeListener;

            this.HookIntoStores( );
        }

        internal IMap<TProperty, IPropertyStore> PropertyStores
        {
            get
            {
                return _propertyStores;
            }
        }

        private void HookIntoStores( )
        {
            this.UnhookFromStores( );

            foreach ( TProperty ii in _propertyStores )
            {
                IPropertyStore ss = _propertyStores[ii];

                ISupportValueChangeNotifications<TItem> hh = ss as ISupportValueChangeNotifications<TItem>;

                if ( null != hh )
                {
                    Listener ll = new Listener( this, ii, _propertyChangeListener, hh );
                    hh.AddListener( ll );

                    if ( null == _listeners )
                        _listeners = new List<Listener>( );

                    _listeners.Add( ll );
                }
            }
        }

        private void UnhookFromStores( )
        {
            List<Listener> list = _listeners;
            _listeners = null;

            if ( null != list )
            {
                foreach ( Listener ii in list )
                {
                    ii._source.RemoveListener( ii );
                    ii._source = null;
                }

                list.Clear( );
            }
        }

        public T GetValue<T>( TItem item, TProperty property )
        {
            IPropertyStore s = _propertyStores[property];
            IPropertyStore<TItem, T> store = s as IPropertyStore<TItem, T>;

            if ( null != store )
            {
                return store.GetValue( item );
            }
            else
            {
                
				object v = s.GetValue( item );
				return null != v ? (T)v : default( T );
            }
        }

        public void SetValue<T>( TItem item, TProperty property, T newValue )
        {
            IPropertyStore s = _propertyStores[property];
            IPropertyStore<TItem, T> store = s as IPropertyStore<TItem, T>;

            if ( null != store )
            {
                store.SetValue( item, newValue );
            }
            else
            {
                s.SetValue( item, newValue );
            }
        }

		public bool HasValue( TItem item, TProperty property )
		{
			IPropertyStore s = _propertyStores[property];

			return s.HasValue( item );
		}
    }

    #endregion // PropertyStorage<TItem, TProperty> Class

	#region ReadOnlyPropertyStore Class

	internal class ReadOnlyPropertyStore<TItem, TValue> : IPropertyStore<TItem, TValue>
	{
		private IPropertyStore<TItem, TValue> _store;
		private string _readOnlyMessage;

		internal ReadOnlyPropertyStore( IPropertyStore<TItem, TValue> store, string readOnlyMessage )
		{
			CoreUtilities.ValidateNotNull( store );

			_store = store;
			_readOnlyMessage = readOnlyMessage;
		}

		public TValue GetValue( TItem item )
		{
			return _store.GetValue( item );
		}

		public void SetValue( TItem item, TValue newValue )
		{
			this.RaiseReadOnlyException( );
		}

		public bool HasValue( TItem item )
		{
			return _store.HasValue( item );
		}

		object IPropertyStore.GetValue( object item )
		{
			return _store.GetValue( item );
		}

		private void RaiseReadOnlyException( )
		{
			throw new NotSupportedException(_readOnlyMessage ?? ScheduleUtilities.GetString( "LE_PropertyIsReadOnly"));//"Property is read-only."
		}

		void IPropertyStore.SetValue( object item, object newValue )
		{
			this.RaiseReadOnlyException( );
		}

		bool IPropertyStore.HasValue( object item )
		{
			return _store.HasValue( item );
		}
	}

	#endregion // ReadOnlyPropertyStore Class

	#region ValueChangeListener<TOwner, TItem> Class

	internal class ValueChangeListener<TOwner, TItem> : IValueChangeListener<TItem>
	{
		private WeakReference _owner;
		private Action<TOwner, TItem> _action;

		public ValueChangeListener( TOwner owner, Action<TOwner, TItem> action )
		{
			_owner = new WeakReference( owner );
			_action = action;
		}

		public void OnValueChanged( TItem item )
		{
			if ( null != _action )
			{
				TOwner owner = (TOwner)CoreUtilities.GetWeakReferenceTargetSafe( _owner );

				if ( null != owner )
					_action( owner, item );
			}
		}
	} 

	#endregion // ValueChangeListener<TOwner, TItem> Class

    #region ValueChangeListenerList Class

    internal class ValueChangeListenerList<TItem> : IValueChangeListener<TItem>
    {
        private object _listeners;

        internal ValueChangeListenerList( )
        {
        }

        public int Count
        {
            get
            {
                IList list = _listeners as IList;

                return null != list ? list.Count : ( null != _listeners ? 1 : 0 );
            }
        }

        public void AddListener( IValueChangeListener<TItem> listener )
        {
            ScheduleUtilities.ValidateNotNull( listener );

            _listeners = ListenerList.Add( _listeners, listener, false );
        }

        public void RemoveListener( IValueChangeListener<TItem> listener )
        {
            _listeners = ListenerList.Remove( _listeners, listener );
        }
        
        public void OnValueChanged( TItem item )
        {
            if ( null != _listeners )
                ListenerList.RaiseValueChanged<TItem>( _listeners, item );
        }

        internal static IValueChangeListener<TItem> Add( IValueChangeListener<TItem> listeners, IValueChangeListener<TItem> listener )
        {
            ScheduleUtilities.ValidateNotNull( listener );

            if ( null == listeners )
                return listener;

            ValueChangeListenerList<TItem> list = listeners as ValueChangeListenerList<TItem>;
            if ( null == list )
            {
                list = new ValueChangeListenerList<TItem>( );
                list.AddListener( listeners );
            }

            list.AddListener( listener );

            return list;
        }
    }

    #endregion // ValueChangeListenerList Class

	#region MetadataStore Class

	internal class MetadataStore<TItem> : IPropertyStore<TItem, MetadataPropertyValueStore>
	{
		#region EntryInfo Class

		private class EntryInfo : IValueChangeListener<TItem>
		{
			#region Members

			internal readonly MetadataStore<TItem> _owner;
			internal StoragePropsInfo.PropInfo _xmlDictionaryPropInfo;
			internal string _key;
			private IPropertyStore _store; 

			#endregion // Members

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="owner"></param>
			internal EntryInfo( MetadataStore<TItem> owner )
			{
				CoreUtilities.ValidateNotNull( owner );
				_owner = owner;
			} 

			#endregion // Constructor

			#region Store

			internal IPropertyStore Store
			{
				get
				{
					if ( null == _store )
					{
						var owner = _owner;
						_store = null != owner._mappedStores ? owner._mappedStores[_key] : null;
						if ( null == _store && null != owner._unmappedStore )
						{
							if ( null == _xmlDictionaryPropInfo )
							{
								_xmlDictionaryPropInfo = new StoragePropsInfo.PropInfo(
									owner._xmlDictionaryPropCounter++, _key, typeof( object ) );
							}

							_store = owner._unmappedStore.CreateStore<object>( _xmlDictionaryPropInfo );
						}

						ISupportValueChangeNotifications<TItem> notifier = _store as ISupportValueChangeNotifications<TItem>;
						Debug.Assert( null != notifier );
						if ( null != notifier )
							notifier.AddListener( this );
					}

					return _store;
				}
			} 

			#endregion // Store

			#region OnValueChanged

			public void OnValueChanged( TItem item )
			{
				ScheduleMetadataPropertyValueStore s = _owner.GetMetadataStoreHelper( item, false );
				if ( null != s )
					s.RaiseReset( );
			} 

			#endregion // OnValueChanged
		}

		#endregion // EntryInfo Class

		#region ScheduleMetadataPropertyValueStore Class

		private class ScheduleMetadataPropertyValueStore : MetadataPropertyValueStore
		{
			#region Members

			internal MetadataStore<TItem> _owner;
			internal TItem _item; 

			#endregion // Members

			#region Constructor

			internal ScheduleMetadataPropertyValueStore( MetadataStore<TItem> owner, TItem item )
			{
				_owner = owner;
				_item = item;
			} 

			#endregion // Constructor

			#region Base Overrides

			#region GetValue

			protected override object GetValue( string key )
			{
				return this.GetValueHelper( key );
			}

			#endregion // GetValue

			#region HasValue

			// SSP 7/18/12 TFS100159
			// 
			/// <summary>
			/// Indicates if the value for the specified key is available or non-empty.
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			internal override bool HasValue( string key )
			{
				IPropertyStore store = this.GetStore( key );
				return null != store && store.HasValue( _item );
			}

			#endregion // HasValue

			#region SetValue

			protected override void SetValue( string key, object value )
			{
				this.SetValueHelper( key, value, true );
			}

			#endregion // SetValue

			#endregion // Base Overrides

			#region Methods

			#region Public Methods

			#region ContainsKey

			public bool ContainsKey( string key )
			{
				return null != this.GetStore( key, false );
			}

			#endregion // ContainsKey 

			#region GetEnumerator

			public override IEnumerator<KeyValuePair<string, object>> GetEnumerator( )
			{
				return ( from key in _owner._dictionaryEntries.Keys select new KeyValuePair<string, object>( key, this.GetValueHelper( key ) ) ).GetEnumerator( );
			}

			#endregion // GetEnumerator

			#endregion // Public Methods

			#region Internal Methods

			#region RaiseReset

			internal void RaiseReset( )
			{
				this.RaisePropertyChangedEvent( string.Empty );

				// SSP 7/18/12 TFS100159
				// We need to route the changes through the view item's listener chain.
				// 
				var listener = _item as IPropertyChangeListener;
				if ( null != listener )
					listener.OnPropertyValueChanged( this, string.Empty, null );
			}

			#endregion // RaiseReset 

			#endregion // Internal Methods

			#region Private Methods

			#region GetStore

			private IPropertyStore GetStore( string key, bool allocateIfNecessary = true )
			{
				EntryInfo entry = _owner.GetEntry( key, allocateIfNecessary );
				return null != entry ? entry.Store : null;
			}

			#endregion // GetStore

			#region GetValueHelper

			private object GetValueHelper( string key )
			{
				IPropertyStore store = this.GetStore( key );
				return null != store ? store.GetValue( _item ) : null;
			}

			#endregion // GetValueHelper 

			#region SetValueHelper

			private void SetValueHelper( string key, object value, bool replace )
			{
				IPropertyStore store = this.GetStore( key );
				if ( null != store )
					store.SetValue( _item, value );
				else
					throw new ArgumentException( string.Format( "Invalid key of {0}", key ) );
			}

			#endregion // SetValueHelper

			#endregion // Private Methods

			#endregion // Methods
		}

		#endregion // ScheduleMetadataPropertyValueStore Class

		#region Member Vars

		private IMap<string, IPropertyStore> _mappedStores;
		private XmlDictionaryStorage<TItem> _unmappedStore;
		private DictionaryPropertyStore<TItem, ScheduleMetadataPropertyValueStore> _dictionaries;
		private Dictionary<string, EntryInfo> _dictionaryEntries;
		private int _xmlDictionaryPropCounter;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public MetadataStore( )
		{
			_dictionaries = new DictionaryPropertyStore<TItem, ScheduleMetadataPropertyValueStore>( );
			_dictionaryEntries = new Dictionary<string, EntryInfo>( );
		} 

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region GetValue

		public MetadataPropertyValueStore GetValue( TItem item )
		{
			return this.GetMetadataStoreHelper( item, true );
		}

		public object GetValue( object item )
		{
			return this.GetValue( (TItem)item );
		}

		#endregion // GetValue

		#region HasValue

		public bool HasValue( TItem item )
		{
			return true;
		}

		public bool HasValue( object item )
		{
			return this.HasValue( (TItem)item );
		}

		#endregion // HasValue

		#region Initialize

		public void Initialize( IMap<string, IPropertyStore> mappedStores, XmlDictionaryStorage<TItem> unmappedStore )
		{
			_mappedStores = mappedStores;
			_unmappedStore = unmappedStore;

			// SSP 7/17/12 TFS80449
			// Pre-allocate the entries so the ScheduleMetadataPropertyValueStore.GetEnumerator returns entries for explicitly
			// mapped metadata fields even if their values have not yet been accessed. When an activity's copy is made for
			// editing purposes, it uses the GetEnumerator to copy metadata values from the source activity and if the source
			// activity's metadata values have not been accessed, the _dictionaryEntries will be empty and thus the values will
			// not be copied.
			// 
			if ( null != _mappedStores )
			{
				foreach ( string key in _mappedStores )
					this.GetEntry( key, true );
			}
		}

		#endregion // Initialize

		#region SetValue

		public void SetValue( TItem item, MetadataPropertyValueStore newValue )
		{
			this.RaiseReadOnlyException( );
		}

		public void SetValue( object item, object newValue )
		{
			this.RaiseReadOnlyException( );
		}

		#endregion // SetValue

		#endregion // Public Methods

		#region Private Methods

		#region GetEntry

		private EntryInfo GetEntry( string key, bool createIfNecessary )
		{
			this.ValidateKey( key );

			EntryInfo entry;
			if ( !_dictionaryEntries.TryGetValue( key, out entry ) && createIfNecessary )
			{
				if ( null != _mappedStores && null != _mappedStores[key] || null != _unmappedStore )
				{
					entry = new EntryInfo( this )
					{
						_key = key
					};

					_dictionaryEntries[key] = entry;
				}
			}

			return entry;
		}

		#endregion // GetEntry

		#region GetMetadataStoreHelper

		private ScheduleMetadataPropertyValueStore GetMetadataStoreHelper( TItem item, bool allocateIfNecessary )
		{
			ScheduleMetadataPropertyValueStore dictionary = _dictionaries.GetValue( item );

			if ( null == dictionary && allocateIfNecessary )
			{
				dictionary = new ScheduleMetadataPropertyValueStore( this, item );
				_dictionaries.SetValue( item, dictionary );
			}

			return dictionary;
		}

		#endregion // GetMetadataStoreHelper

		#region RaiseReadOnlyException

		private void RaiseReadOnlyException( )
		{
			throw new NotSupportedException( ScheduleUtilities.GetString( "LE_PropertyIsReadOnly" ) );//"Property is read-only."
		}

		#endregion // RaiseReadOnlyException

		#region ValidateKey

		private void ValidateKey( string key )
		{
			CoreUtilities.ValidateNotEmpty( key );
		}

		#endregion // ValidateKey

		#endregion // Private Methods 

		#endregion // Methods
	} 

	#endregion // MetadataStore Class

	#region XmlDeserializerConverter Class

	internal class XmlDeserializerConverter : IValueConverter
	{
		#region Member Vars

		protected ObjectSerializer _serializer; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="valueType"></param>
		public XmlDeserializerConverter( Type valueType )
		{
			AttributeValueParser avp = new AttributeValueParser( );
			_serializer = new ObjectSerializer( avp );
			ScheduleUtilities.RegisterSerializerInfos( _serializer );
		}

		#endregion // Constructor

		#region Convert

		public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( null != value && targetType.IsInstanceOfType( value ) )
				return value;

			string xmlFragment = value as string;
			if ( !string.IsNullOrEmpty( xmlFragment ) )
			{
				object parsedVal;
				if ( _serializer.ParseXmlFragment( xmlFragment, targetType, out parsedVal ) )
					return parsedVal;
			}

			Debug.Assert( null == value );
			return null;
		} 

		#endregion // Convert

		#region ConvertBack

		public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
		{
			if ( null != value && targetType.IsInstanceOfType( value ) )
				return value;

			if ( null != value )
			{
				string xmlFragment;
				if ( _serializer.SaveAsXmlFragment( value, out xmlFragment ) )
					return xmlFragment;
			}

			Debug.Assert( null == value );
			return null;
		} 

		#endregion // ConvertBack
	}

	#endregion // XmlDeserializerConverter Class

	#region XmlDictionaryStorage Class

	internal class XmlDictionaryStorage<TItem>
	{
		#region Nested Data Structures

		#region Properties Class

		private class Properties
		{
			internal XmlDictionaryStorage<TItem> _storage;
			internal TItem _item;
			internal Dictionary<string, object> _deserializedValues;
		}

		#endregion // Properties Class

		#region PropertiesSerializationInfo Class

		private class PropertiesSerializationInfo : ObjectSerializationInfo
		{
			private XmlDictionaryStorage<TItem> _storage;

			internal PropertiesSerializationInfo( XmlDictionaryStorage<TItem> storage )
			{
				_storage = storage;
			}

			protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
			{
				return from ii in _storage._storesList select new PropertySerializationInfo( ii._propInfo._type, ii._propInfo._name );
			}

			public override Dictionary<string, object> Serialize( object obj )
			{
				Properties properties = (Properties)obj;
				Dictionary<string, object> map = null;

				XmlDictionaryStorage<TItem> xmlStorage = properties._storage;
				TItem item = properties._item;

				foreach ( var ii in xmlStorage._storesList )
				{
					object value = ii._store.GetValue( item );

					map = ScheduleUtilities.AddEntryHelper<string, object>( map, ii._propInfo._name, value );
				}

				return map;
			}

			internal override PropertySerializationInfo GetPropertyInfo( string propertyName )
			{
				PropertySerializationInfo ret = base.GetPropertyInfo( propertyName );
				if ( null == ret )
				{
					// If we get here than that means a property value exists in XML for which a store hasn't been created.
					// 
					Debug.Assert( false, "Unknown value in XML." );
					ret = new PropertySerializationInfo( typeof( object ), propertyName );
				}

				return ret;
			}

			public override object Deserialize( Dictionary<string, object> map )
			{
				return new Properties
				{
					_deserializedValues = map
				};
			}
		}

		#endregion // PropertiesSerializationInfo Class

		//#region Property Class

		//private class Property
		//{
		//    #region SerializationInfo Class

		//    internal class SerializationInfo : ObjectSerializationInfo
		//    {
		//        protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
		//        {
		//            return new PropertySerializationInfo[]
		//            {
		//                new PropertySerializationInfo( typeof( string ), "Property" ),
		//                new PropertySerializationInfo( typeof( object ), "Value" )
		//            };
		//        }

		//        public override Dictionary<string, object> Serialize( object obj )
		//        {
		//            Property entry = (Property)obj;
		//            var map = ScheduleUtilities.AddEntryHelper<string, object>( null, "Property", entry._property );
		//            map = ScheduleUtilities.AddEntryHelper<string, object>( map, "Value", entry._value );

		//            return map;
		//        }

		//        public override object Deserialize( Dictionary<string, object> map )
		//        {
		//            string property = null;
		//            object value = null;

		//            object v;
		//            if ( map.TryGetValue( "Property", out v ) )
		//                property = (string)v;

		//            if ( map.TryGetValue( "Value", out v ) )
		//                value = v;

		//            return new Property
		//            {
		//                _property = property,
		//                _value = value
		//            };
		//        }
		//    } 

		//    #endregion // SerializationInfo Class

		//    internal string _property;
		//    internal object _value;
		//}

		//#endregion // Property Class

		#region PropertyStore Class

		private class PropertyStore<TValue> : DictionaryPropertyStore<TItem, TValue>
		{
			internal XmlDictionaryStorage<TItem> _owner;

			internal PropertyStore( XmlDictionaryStorage<TItem> owner )
			{
				_owner = owner;
			}

			private void Verify( TItem item )
			{
				_owner.VerifyPropertyValues( item );
			}

			internal Action<TItem> GetNotifyPropertyChangeListenersDelegate( )
			{
				return this.NotifyPropertyChangeListeners;
			}

			public override TValue GetValue( TItem item )
			{
				this.Verify( item );

				return base.GetValue( item );
			}

			public override bool HasValue( TItem item )
			{
				this.Verify( item );

				return base.HasValue( item );
			}

			public override void SetValue( TItem item, TValue newValue )
			{
				this.Verify( item );

				base.SetValue( item, newValue );
			}
		}

		#endregion // PropertyStore Class

		#region StoreInfo Clas

		private class StoreInfo
		{
			internal StoragePropsInfo.PropInfo _propInfo;
			internal IPropertyStore _store;
			internal Action<TItem> _notifyStoreValueChanged;
		}

		#endregion // StoreInfo Clas

		#endregion // Nested Data Structures

		#region Member Vars

		/// <summary>
		/// This is where the XML as string is stored.
		/// </summary>
		private IPropertyStore<TItem, string> _xmlValueStore;

		/// <summary>
		/// When an individual property value is set, we need to update the XML. However we don't want to do it
		/// right away so we'll keep track of items whose XML value needs to be updated.
		/// </summary>
		private IPropertyStore<TItem, bool> _xmlNeedUpdating;

		/// <summary>
		/// This is a list of items that need to have their XML synchronized with the property values.
		/// </summary>
		private Queue<TItem> _xmlUpdateList;

		/// <summary>
		/// The operation used to asynchronously update XML.
		/// </summary>
		private DeferredOperation _updateXmlOperation;

		/// <summary>
		/// When we get a notification from the data source indicating that the field where the XML is stored
		/// has changed, we need to parse it and update individual property values. This keeps track of such
		/// items where the property values need to be updated.
		/// </summary>
		private IPropertyStore<TItem, bool> _propertyValuesNeedUpdating;

		/// <summary>
		/// Listener for xml value property store.
		/// </summary>
		private ValueChangeListener<XmlDictionaryStorage<TItem>, TItem> _xmlValueListener;

		/// <summary>
		/// Listner for property value stores.
		/// </summary>
		private ValueChangeListener<XmlDictionaryStorage<TItem>, TItem> _propertyValueListener;

		/// <summary>
		/// Used to make sure we don't synchronize property values to XML when we are the one updating the XML value.
		/// </summary>
		private AntiRecursionList _inUpdateXmlValue;

		/// <summary>
		/// Used to make sure we don't synchronize XML to property values when we are in the process of synchronizing property values to XML.
		/// </summary>
		private AntiRecursionList _inUpdatePropertyValues;

		/// <summary>
		/// Used to save and parse XML.
		/// </summary>
		private ObjectSerializer _serializer;

		/// <summary>
		/// This is where individual values from the XML are cached. This is because we don't want to 
		/// parse the XML every time a value is retrieved or re-create XML every time a value is set.
		/// </summary>
		private IMap<int, StoreInfo> _storesMap;
		private List<StoreInfo> _storesList;
		private MapsFactory.IntMap<bool> _cachedHadValuesInXml;
		private IMap<string, StoreInfo> _namesMap;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public XmlDictionaryStorage( )
		{
			_xmlNeedUpdating = new DictionaryPropertyStore<TItem, bool>( );
			_propertyValuesNeedUpdating = new DictionaryPropertyStore<TItem, bool>( );
			_inUpdateXmlValue = new AntiRecursionList( );
			_inUpdatePropertyValues = new AntiRecursionList( );
			_xmlValueListener = new ValueChangeListener<XmlDictionaryStorage<TItem>, TItem>( this, OnXmlValueChanged );
			_propertyValueListener = new ValueChangeListener<XmlDictionaryStorage<TItem>, TItem>( this, OnPropertyValueChangedHandler );
			_xmlUpdateList = new Queue<TItem>( );

			_storesMap = MapsFactory.CreateMapHelper<int, StoreInfo>( );
			_storesList = new List<StoreInfo>( );
			_namesMap = MapsFactory.CreateMapHelper<string, StoreInfo>( );

			_serializer = new ObjectSerializer( new AttributeValueParser( ) );
			ScheduleUtilities.RegisterSerializerInfos( _serializer );
		}

		#endregion // Constructor

		#region Methods

		#region Public Methods

		#region CreateStore

		public IPropertyStore<TItem, TValue> CreateStore<TValue>( StoragePropsInfo.PropInfo propInfo )
		{
			var storeInfo = _storesMap[propInfo._key];
			var store = null != storeInfo ? storeInfo._store as PropertyStore<TValue> : null;
			if ( null == store )
			{
				store = new PropertyStore<TValue>( this );
				storeInfo = new StoreInfo
				{
					_propInfo = propInfo,
					_store = store,
					_notifyStoreValueChanged = store.GetNotifyPropertyChangeListenersDelegate( )
				};

				_storesMap[propInfo._key] = storeInfo;
				_storesList.Add( storeInfo );
				_namesMap[propInfo._name] = storeInfo;
				store.AddListener( _propertyValueListener );
				
				// Since a new store has been added, we need to re-create the serialization info to include that property.
				// 
				_serializer.RegisterInfo( typeof( Properties ), new PropertiesSerializationInfo( this ) );
			}
			else
				Debug.Assert( null == storeInfo );

			return store;
		}

		#endregion // CreateStore

		#region Initialize

		public void Initialize( IPropertyStore<TItem, string> xmlValueStore )
		{
			ISupportValueChangeNotifications<TItem> l = _xmlValueStore as ISupportValueChangeNotifications<TItem>;
			if ( null != l )
				l.RemoveListener( _xmlValueListener );

			_xmlValueStore = xmlValueStore;

			l = _xmlValueStore as ISupportValueChangeNotifications<TItem>;
			if ( null != l )
				l.AddListener( _xmlValueListener );
		}

		#endregion // Initialize

		#endregion // Public Methods

		#region Internal Methods

		#region UpdateXmlValue

		internal void UpdateXmlValue( TItem item, out DataErrorInfo error )
		{
			error = null;
			if ( _inUpdateXmlValue.Enter( item ) )
			{
				try
				{
					string xml = this.CreateXmlValue( item, out error );
					if ( null == error )
						_xmlValueStore.SetValue( item, xml );
				}
				finally
				{
					_inUpdateXmlValue.Exit( item );
				}
			}
		}

		#endregion // UpdateXmlValue

		#region UpdateXmlValues

		internal void UpdateXmlValues( )
		{
			if ( null != _updateXmlOperation )
			{
				_updateXmlOperation.CancelPendingOperation( );
				_updateXmlOperation = null;
			}

			while ( _xmlUpdateList.Count > 0 )
			{
				TItem item = _xmlUpdateList.Dequeue( );
				DataErrorInfo error;
				this.UpdateXmlValue( item, out error );
			}
		}

		#endregion // UpdateXmlValues

		#endregion // Internal Methods

		#region Private Methods

		#region CreateXmlValue

		private string CreateXmlValue( TItem item, out DataErrorInfo error )
		{
			Properties _values = new Properties
			{
				_storage = this,
				_item = item
			};

			string xml;
			if ( _serializer.SaveAsXmlFragment( _values, out xml ) )
			{
				error = null;
				return xml;
			}
			else
			{
				error = DataErrorInfo.CreateError( item, "An error occurred updating." );
				error.DiagnosticText = "Error saving unmapped properties. Unable to create XML.";
				return null;
			}
		}

		#endregion // CreateXmlValue

		#region OnPropertyValueChanged

		private static void OnPropertyValueChangedHandler( XmlDictionaryStorage<TItem> owner, TItem item )
		{
			owner.OnPropertyValueChanged( item );
		}

		private void OnPropertyValueChanged( TItem item )
		{
			if ( !_inUpdatePropertyValues.InProgress( item ) )
			{
				if ( !_xmlNeedUpdating.GetValue( item ) )
				{
					_xmlNeedUpdating.SetValue( item, true );
					_xmlUpdateList.Enqueue( item );
					if ( null == _updateXmlOperation )
					{
						_updateXmlOperation = new DeferredOperation( this.UpdateXmlValues );
						_updateXmlOperation.StartAsyncOperation( );
					}
				}
			}
		}

		#endregion // OnPropertyValueChanged

		#region OnXmlValueChanged

		private static void OnXmlValueChanged( XmlDictionaryStorage<TItem> owner, TItem item )
		{
			if ( !owner._inUpdateXmlValue.InProgress( item ) )
			{
				owner._propertyValuesNeedUpdating.SetValue( item, true );

				foreach ( StoreInfo ii in owner._storesList )
				{
					if ( null != ii._notifyStoreValueChanged )
						ii._notifyStoreValueChanged( item );
				}
			}
		}

		#endregion // OnXmlValueChanged

		#region ParseXmlValue

		private Properties ParseXmlValue( string xml, out DataErrorInfo error )
		{
			object val;
			if ( _serializer.ParseXmlFragment( xml, typeof( Properties ), out val ) )
			{
				error = null;
				return (Properties)val;
			}
			else
			{
				error = DataErrorInfo.CreateError( xml, "Unable to parse data contained in the datasource." );
				error.DiagnosticText = xml;
				return null;
			}
		}

		#endregion // ParseXmlValue

		#region VerifyPropertyValues

		private void VerifyPropertyValues( TItem item )
		{
			if ( _propertyValuesNeedUpdating.GetValue( item ) )
			{
				_propertyValuesNeedUpdating.SetValue( item, false );

				if ( _inUpdatePropertyValues.Enter( item ) )
				{
					try
					{
						string xml = _xmlValueStore.GetValue( item );
						if ( !string.IsNullOrEmpty( xml ) )
						{
							DataErrorInfo error;
							Properties props = this.ParseXmlValue( xml, out error );
							if ( null != props )
							{
								MapsFactory.IntMap<bool> hadValuesInXml = _cachedHadValuesInXml ?? new MapsFactory.IntMap<bool>( );
								_cachedHadValuesInXml = null;
								hadValuesInXml.Clear( );

								Debug.Assert( null != props._deserializedValues );
								if ( null != props._deserializedValues )
								{
									foreach ( KeyValuePair<string, object> ii in props._deserializedValues )
									{
										StoreInfo storeInfo = _namesMap[ ii.Key ];
										Debug.Assert( null != storeInfo, "Unknown property in the XML." );
										if ( null != storeInfo )
										{
											storeInfo._store.SetValue( item, ii.Value );
											hadValuesInXml[storeInfo._propInfo._key] = true;
										}
									}
								}

								// Clear the values of properties that were not in XML.
								// 
								foreach ( StoreInfo store in _storesList )
								{
									int prop = store._propInfo._key;
									if ( !hadValuesInXml[prop] )
										store._store.SetValue( item, null );
									else
										hadValuesInXml[prop] = false;
								}

								_cachedHadValuesInXml = hadValuesInXml;
							}
						}
					}
					finally
					{
						_inUpdatePropertyValues.Exit( item );
					}
				}
			}
		}

		#endregion // VerifyPropertyValues

		#endregion // Private Methods 

		#endregion // Methods
	}

	#endregion // XmlDictionaryStorage Class


    #region ISupportValueChangeNotifications<TItem> Interface

    internal interface ISupportValueChangeNotifications<TItem>
    {
        void AddListener( IValueChangeListener<TItem> listener );
        void RemoveListener( IValueChangeListener<TItem> listener );
    }

    #endregion // ISupportValueChangeNotifications<TItem> Interface

    #region SupportValueChangeNotificationsImpl Class

    internal class SupportValueChangeNotificationsImpl<TItem> : ISupportValueChangeNotifications<TItem>
    {
        private ValueChangeListenerList<TItem> _listeners;

        internal SupportValueChangeNotificationsImpl( )
        {
            _listeners = new ValueChangeListenerList<TItem>( );
        }

        protected ValueChangeListenerList<TItem> Listeners
        {
            get
            {
                if ( null == _listeners )
                    _listeners = new ValueChangeListenerList<TItem>( );

                return _listeners;
            }
        }

        public void AddListener( IValueChangeListener<TItem> listener )
        {
            this.Listeners.AddListener( listener );
        }

        public void RemoveListener( IValueChangeListener<TItem> listener )
        {
            if ( null != _listeners )
                _listeners.RemoveListener( listener );
        }

        protected void NotifyPropertyChangeListeners( TItem item )
        {
            if ( null != _listeners )
                ListenerList.RaiseValueChanged<TItem>( _listeners, item );
        }
    }

    #endregion // SupportValueChangeNotificationsImpl Class


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