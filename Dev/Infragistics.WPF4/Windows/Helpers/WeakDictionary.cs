using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Collections;

// SSP 1/16/09 - NAS9.1 Record Filtering
// Added WeakDictionary generic class. It manages both the keys and values as weak references.
// 





namespace Infragistics.Collections

{
	/// <summary>
	/// IDictionary implementation that manages keys and/or values as weak references so they can be
	/// garbage collected.
	/// </summary>
	/// <typeparam name="TKey">Type of keys</typeparam>
	/// <typeparam name="TValue">Type of values</typeparam>
	public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
		#region Nested Data Structures

		#region Entry Class

		private class Entry
		{
			// _key and _value will be the actual objects or weak references to those
			// objects depending on the _keysAsWeakReferences and _valuesAsWeakReferences.
			// 
			private object _key;
			private object _value;
			internal int _hash;
			internal Entry _next;

			public Entry( int hash, object key, object value )
			{
				_hash = hash;
				_key = key;
				_value = value;
			}

			internal TKey GetKey( WeakDictionary<TKey, TValue> wd )
			{
				object o = _key;

				if ( wd._keysAsWeakReferences )
					o = GetWeakReferenceTargetSafe( (WeakReference)o );

				return (TKey)o;
			}

			internal TValue GetValue( WeakDictionary<TKey, TValue> wd )
			{
				object o = _value;

				if ( wd._valuesAsWeakReferences )
					o = GetWeakReferenceTargetSafe( (WeakReference)o );

				return (TValue)o;
			}

			internal void SetValue( WeakDictionary<TKey, TValue> wd, TValue value )
			{
				_value = wd._valuesAsWeakReferences ? (object)wd.CreateValueWeakReference( value ) : (object)value;
			}

			internal bool HasKey( WeakDictionary<TKey, TValue> wd )
			{
				return null != this.GetKey( wd );
			}

			internal bool HasValue( WeakDictionary<TKey, TValue> wd )
			{
				return null != this.GetValue( wd );
			}

			internal bool IsSameKey( WeakDictionary<TKey, TValue> wd, TKey key )
			{
				TKey thisKey = this.GetKey( wd );
				return wd._keyComparer.Equals( key, thisKey );
			}

			internal bool IsSameValue( WeakDictionary<TKey, TValue> wd, TValue value )
			{
				TValue thisValue = this.GetValue( wd );
				return object.Equals( value, thisValue );
			}
		}

		#endregion // Entry Class

		#region EntryEnumerable Class

		private class EntryEnumerable : IEnumerable<Entry>
		{
			#region Nested Data Structures

			#region CollectionBase Class

			internal abstract class CollectionBase
			{
				#region Member Vars

				protected WeakDictionary<TKey, TValue> _wd;

				#endregion // Member Vars

				#region Constructor

				internal CollectionBase( WeakDictionary<TKey, TValue> wd )
				{
					_wd = wd;
				}

				#endregion // Constructor

				#region Protected Methods

				protected void NotSupported( )
				{
					throw new NotSupportedException( );
				}

				#endregion // Protected Methods

				#region ICollection Members

				public int Count
				{
					get 
					{
						return _wd.Count;
					}
				}

				public bool IsSynchronized
				{
					get 
					{
						return false;
					}
				}

				public object SyncRoot
				{
					get 
					{
						return _wd;
					}
				}

				#endregion
			}

			#endregion // CollectionBase Class

			#region KeyCollection Class

			internal class KeyCollection : CollectionBase, ICollection<TKey>
			{
				#region Enumerator Class

				internal class Enumerator : IEnumerator<TKey>
				{
					#region Private Vars

					private WeakDictionary<TKey, TValue> _wd;
					private EntryEnumerable.Enumerator _e;

					#endregion // Private Vars

					#region Constructor

					internal Enumerator( WeakDictionary<TKey, TValue> wd )
					{
						_wd = wd;
						_e = new EntryEnumerable.Enumerator( _wd );
					}

					#endregion // Constructor

					#region IEnumerator<TKey> Members

					public TKey Current
					{
						get
						{
							// SSP 5/5/09
							// Use the new CurrentKey and CurrentValue properties.
							// 
							//return _e.Current.GetKey( _wd );
							return _e.CurrentKey;
						}
					}

					#endregion

					#region IDisposable Members

					public void Dispose( )
					{
						_e.Dispose( );
					}

					#endregion

					#region IEnumerator Members

					object System.Collections.IEnumerator.Current
					{
						get
						{
							return this.Current;
						}
					}

					public bool MoveNext( )
					{
						return _e.MoveNext( );
					}

					public void Reset( )
					{
						_e.Reset( );
					}

					#endregion
				}

				#endregion // Enumerator Class

				#region Constructor

				internal KeyCollection( WeakDictionary<TKey, TValue> wd )
					: base( wd )
				{
				}

				#endregion // Constructor

				#region ICollection<TKey> Members

				public void Add( TKey item )
				{
					this.NotSupported( );
				}

				public void Clear( )
				{
					this.NotSupported( );
				}

				public bool Contains( TKey item )
				{
					return _wd.ContainsKey( item );
				}

				public void CopyTo( TKey[] array, int arrayIndex )
				{
					foreach ( KeyValuePair<TKey, TValue> ii in _wd )
						array[arrayIndex++] = ii.Key;
				}

				public void CopyTo( Array array, int arrayIndex )
				{
					foreach ( KeyValuePair<TKey, TValue> ii in _wd )
						array.SetValue( ii.Key, arrayIndex++ );
				}

				public bool IsReadOnly
				{
					get 
					{
						return true;
					}
				}

				public bool Remove( TKey item )
				{
					this.NotSupported( );
					return false;
				}

				#endregion

				#region IEnumerable<TKey> Members

				public IEnumerator<TKey> GetEnumerator( )
				{
					return new Enumerator( _wd );
				}

				IEnumerator IEnumerable.GetEnumerator( )
				{
					return this.GetEnumerator( );
				}

				#endregion
			}

			#endregion // KeyCollection Class

			#region ValueCollection Class

			internal class ValueCollection : CollectionBase, ICollection<TValue>
			{
				#region Enumerator Class

				internal class Enumerator : IEnumerator<TValue>
				{
					#region Private Vars

					private WeakDictionary<TKey, TValue> _wd;
					private EntryEnumerable.Enumerator _e;

					#endregion // Private Vars

					#region Constructor

					internal Enumerator( WeakDictionary<TKey, TValue> wd )
					{
						_wd = wd;
						_e = new EntryEnumerable.Enumerator( _wd );
					}

					#endregion // Constructor

					#region IEnumerator<TValue> Members

					public TValue Current
					{
						get
						{
							// SSP 5/5/09
							// Use the new CurrentKey and CurrentValue properties.
							// 
							//return _e.Current.GetValue( _wd );
							return _e.CurrentValue;
						}
					}

					#endregion

					#region IDisposable Members

					public void Dispose( )
					{
						_e.Dispose( );
					}

					#endregion

					#region IEnumerator Members

					object System.Collections.IEnumerator.Current
					{
						get
						{
							return this.Current;
						}
					}

					public bool MoveNext( )
					{
						return _e.MoveNext( );
					}

					public void Reset( )
					{
						_e.Reset( );
					}

					#endregion
				}

				#endregion // Enumerator Class

				#region Constructor

				internal ValueCollection( WeakDictionary<TKey, TValue> wd )
					: base( wd )
				{
				}

				#endregion // Constructor

				#region ICollection<TValue> Members

				public void Add( TValue item )
				{
					this.NotSupported( );
				}

				public void Clear( )
				{
					this.NotSupported( );
				}

				public bool Contains( TValue item )
				{
					return _wd.ContainsValue( item );
				}

				public void CopyTo( TValue[] array, int arrayIndex )
				{
					foreach ( KeyValuePair<TKey, TValue> ii in _wd )
						array[arrayIndex++] = ii.Value;
				}

				public void CopyTo( Array array, int arrayIndex )
				{
					foreach ( KeyValuePair<TKey, TValue> ii in _wd )
						array.SetValue( ii.Value, arrayIndex++ );
				}

				public bool IsReadOnly
				{
					get
					{
						return true;
					}
				}

				public bool Remove( TValue item )
				{
					this.NotSupported( );
					return false;
				}

				#endregion

				#region IEnumerable<TValue> Members

				public IEnumerator<TValue> GetEnumerator( )
				{
					return new Enumerator( _wd );
				}

				IEnumerator IEnumerable.GetEnumerator( )
				{
					return this.GetEnumerator( );
				}

				#endregion
			}

			#endregion // ValueCollection Class

			#region Enumerator Class

			private class Enumerator : IEnumerator<Entry>
			{
				#region Private Vars

				private WeakDictionary<TKey, TValue> _wd;
				private Entry[] _arr;
				private int _i;
				private Entry _current;

				// SSP 5/5/09
				// Hold references to the current entry's key and value so it doesn't get 
				// collected while enumerating.
				// 
				private TKey _currentKey;
				private TValue _currentValue;

				#endregion // Private Vars

				#region Constructor

				internal Enumerator( WeakDictionary<TKey, TValue> wd )
				{
					_wd = wd;
					_arr = _wd._arr;

					// SSP 5/5/09
					// 
					this.Reset( );
				}

				#endregion // Constructor

				#region IEnumerator<Entry> Members

				public Entry Current
				{
					get { return _current; }
				}

				#endregion

				#region CurrentKey

				// SSP 5/5/09
				// 
				public TKey CurrentKey
				{
					get
					{
						return _currentKey;
					}
				}

				#endregion // CurrentKey

				#region CurrentValue

				// SSP 5/5/09
				// 
				public TValue CurrentValue
				{
					get
					{
						return _currentValue;
					}
				}

				#endregion // CurrentValue

				#region IDisposable Members

				public void Dispose( )
				{
				}

				#endregion

				#region IEnumerator Members

				object System.Collections.IEnumerator.Current
				{
					get { return this.Current; }
				}

				public bool MoveNext( )
				{
					// SSP 5/5/09
					// Skip entries that have key or value garbage collected.
					// 
					// ------------------------------------------------------------------
					
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

					do
					{
						if ( null != _current )
							_current = _current._next;

						while ( null == _current && ++_i < _arr.Length )
							_current = _arr[_i];

						if ( null == _current )
							return false;

						_currentKey = _current.GetKey( _wd );
						_currentValue = _current.GetValue( _wd );
					}
					// SSP 9/29/10 
					// Don't skip entries if the value is null but the values aren't being managed as 
					// weak references. In other words, the entry was created using a null value.
					// 
					//while ( null == _currentKey || null == _currentValue );
					while ( null == _currentKey || _wd._valuesAsWeakReferences && null == _currentValue );

					return true;
					// ------------------------------------------------------------------
				}

				public void Reset( )
				{
					_i = -1;
					_current = null;
				}

				#endregion
			}

			#endregion // Enumerator Class

			#region KeyValuePairEnumerator Class

			internal class KeyValuePairEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
			{
				#region Private Vars

				private WeakDictionary<TKey, TValue> _wd;
				private EntryEnumerable.Enumerator _e;

				#endregion // Private Vars

				#region Constructor

				internal KeyValuePairEnumerator( WeakDictionary<TKey, TValue> wd )
				{
					_wd = wd;
					_e = new EntryEnumerable.Enumerator( _wd );
				}

				#endregion // Constructor

				#region IEnumerator<KeyValuePair<TKey, TValue>> Members

				public KeyValuePair<TKey, TValue> Current
				{
					get
					{
						// SSP 5/5/09
						// Use the new CurrentKey and CurrentValue properties.
						// 
						//Entry ii = _e.Current;
						//return new KeyValuePair<TKey, TValue>( ii.GetKey( _wd ), ii.GetValue( _wd ) );
						return new KeyValuePair<TKey, TValue>( _e.CurrentKey, _e.CurrentValue );
					}
				}

				#endregion

				#region IDisposable Members

				public void Dispose( )
				{
					_e.Dispose( );
				}

				#endregion

				#region IEnumerator Members

				object System.Collections.IEnumerator.Current
				{
					get
					{
						return this.Current;
					}
				}

				public bool MoveNext( )
				{
					return _e.MoveNext( );
				}

				public void Reset( )
				{
					_e.Reset( );
				}

				#endregion
			}

			#endregion // KeyValuePairEnumerator Class

			#region EnumerableWrapper<T>

			internal class EnumerableWrapper<T> : IEnumerable<T>
			{
				private IEnumerable<T> _e;

				internal EnumerableWrapper( IEnumerable<T> e )
				{
					_e = e;
				}

				public IEnumerator<T> GetEnumerator( )
				{
					return _e.GetEnumerator( );
				}

				IEnumerator IEnumerable.GetEnumerator( )
				{
					return this.GetEnumerator( );
				}
			}

			#endregion // EnumerableWrapper<T>

			#endregion // Nested Data Structures

			#region Private Vars

			private WeakDictionary<TKey, TValue> _wd;

			#endregion // Private Vars

			#region Constructor

			internal EntryEnumerable( WeakDictionary<TKey, TValue> wd )
			{
				_wd = wd;
			}

			#endregion // Constructor

			#region Methods

			public IEnumerator<Entry> GetEnumerator( )
			{
				return new Enumerator( _wd );
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion // Methods
		}

		#endregion // EntryEnumerable Class

		#endregion // Nested Data Structures

		#region Constants

		private const int DEFAULT_CAPACITY = 11;
		private const float DEFAULT_LOAD_FACTOR = 0.9f;

		#endregion // Constants

		#region Member Vars

		private Entry[] _arr;
		private int _count;
		private int _version;
		private IEqualityComparer<TKey> _keyComparer;
		private float _loadFactor;
		private bool _keysAsWeakReferences = true;
		private bool _valuesAsWeakReferences = true;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="manageKeysAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="manageValuesAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		public WeakDictionary( bool manageKeysAsWeakReferences, bool manageValuesAsWeakReferences ) 
			: this( manageKeysAsWeakReferences, manageValuesAsWeakReferences, DEFAULT_CAPACITY )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="manageKeysAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="manageValuesAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="initialCapacity">Initial capacity.</param>
		public WeakDictionary( bool manageKeysAsWeakReferences, bool manageValuesAsWeakReferences, 
			int initialCapacity )
			: this( manageKeysAsWeakReferences, manageValuesAsWeakReferences, initialCapacity, null )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="manageKeysAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="manageValuesAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="keyComparer">Comparer for compring keys.</param>
		public WeakDictionary( bool manageKeysAsWeakReferences, bool manageValuesAsWeakReferences, 
			IEqualityComparer<TKey> keyComparer )
			: this( manageKeysAsWeakReferences, manageValuesAsWeakReferences, DEFAULT_CAPACITY, keyComparer )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="manageKeysAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="manageValuesAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="initialCapacity">Initial capacity.</param>
		/// <param name="keyComparer">Comparer for compring keys.</param>
		public WeakDictionary( bool manageKeysAsWeakReferences, bool manageValuesAsWeakReferences, 
			int initialCapacity, IEqualityComparer<TKey> keyComparer )
			: this( manageKeysAsWeakReferences, manageValuesAsWeakReferences, initialCapacity, DEFAULT_LOAD_FACTOR, keyComparer )
		{
		}

		/// <summary>
		/// Constructor. Initializes a new instance of <see cref="WeakDictionary&lt;TKey, TValue&gt;"/>.
		/// </summary>
		/// <param name="manageKeysAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="manageValuesAsWeakReferences">Specifies whether to manage keys as weak references.</param>
		/// <param name="initialCapacity">Initial capacity.</param>
		/// <param name="loadFactor">Specifies load factor. Must be greater than 0 and less than or equal to 1.</param>
		/// <param name="keyComparer">Comparer for compring keys.</param>
		public WeakDictionary( bool manageKeysAsWeakReferences, bool manageValuesAsWeakReferences,
			int initialCapacity, float loadFactor, IEqualityComparer<TKey> keyComparer )
		{
			if ( initialCapacity <= 0 )
				throw new ArgumentOutOfRangeException( "initialCapacity", SR.GetString( "LE_ArgumentOutOfRangeException_7" ) );

			if ( loadFactor <= 0f || loadFactor > 1f )
				throw new ArgumentOutOfRangeException( "loadFactor", SR.GetString( "LE_ArgumentOutOfRangeException_8" ) );

			

			_keysAsWeakReferences = manageKeysAsWeakReferences;
			_valuesAsWeakReferences = manageValuesAsWeakReferences;
			_loadFactor = loadFactor;
			_arr = new Entry[(int)( initialCapacity / _loadFactor )];
			_keyComparer = null != keyComparer ? keyComparer : EqualityComparer<TKey>.Default;
		}

		#endregion // Constructor

		#region Methods

		#region Private Methods

		#region AddHelper

		private void AddHelper( TKey key, TValue value, bool replace )
		{
			int hash = this.GetHashCode( key );
			int i = this.GetIndex( hash );
			Entry ii = _arr[i];
			if ( null != ii )
			{
				Entry match = this.FindInList( ii, key );
				if ( null != match )
				{
					// If performing "Add" (! replace) operation, then throw a key already exists 
					// exception.
					// 
					// SSP 9/29/10 
					// Modified the behavior to auto-collect entries whose values have been garbage collected.
					// 
					//if ( !replace )
					if ( ! replace && ( ! _valuesAsWeakReferences || match.HasValue( this ) ) )
						throw new InvalidOperationException( SR.GetString( "LE_InvalidOperationException_7" ) );

					match.SetValue( this, value );
					_version++;
					return;
				}
			}

			Entry newEntry = new Entry( hash,
					_keysAsWeakReferences ? (object)this.CreateKeyWeakReference( key ) : (object)key,
					_valuesAsWeakReferences ? (object)this.CreateValueWeakReference( value ) : (object)value
				);

			newEntry._next = ii;
			_arr[i] = newEntry;
			_count++;
			_version++;
			this.ExpandCollapseHelper( true );
		}

		#endregion // AddHelper

		#region ContainsValue

		private bool ContainsValue( TValue value )
		{
			foreach ( Entry ii in new EntryEnumerable( this ) )
			{
				if ( ii.IsSameValue( this, value ) )
					return true;
			}

			return false;
		}

		#endregion // ContainsValue

		#region CreateKeyWeakReference

		/// <summary>
		/// Creates a weak reference for the key.
		/// </summary>
		/// <param name="key">Key object.</param>
		/// <returns>Weak reference to the key.</returns>
		private WeakReference CreateKeyWeakReference( TKey key )
		{
			return null != key ? new WeakReference( key ) : null;
		} 

		#endregion // CreateKeyWeakReference

		#region CreateValueWeakReference

		/// <summary>
		/// Creates a weak reference for the key.
		/// </summary>
		/// <param name="value">Value object.</param>
		/// <returns>Weak reference to the value.</returns>
		private WeakReference CreateValueWeakReference( TValue value )
		{
			return null != value ? new WeakReference( value ) : null;
		} 

		#endregion // CreateValueWeakReference

		#region ExpandCollapseHelper

		private int CalcNewLengthHelper( int currLen, int requiredLen )
		{
			int newLen;
			if ( currLen < requiredLen )
			{
				newLen = 2 * currLen;
			}
			else
			{
				newLen = currLen;
				while ( newLen > 4 * requiredLen )
				{
					int tmp = newLen / 2;
					int defaultLen = (int)( DEFAULT_CAPACITY / _loadFactor );
					if ( tmp < defaultLen )
						break;

					newLen = tmp;
				}
			}

			return newLen;
		}

		/// <summary>
		/// Expands or contracts the size of storage based on the new count after an operation,
		/// like add, remove or clear, is performed.
		/// </summary>
		/// <param name="expandOnly">Specifies that the storage should only be expanded, and not contracted.</param>
		private void ExpandCollapseHelper( bool expandOnly )
		{
			int newCount = _count;
			int requiredLen = (int)( newCount / _loadFactor );
			int currLen = _arr.Length;
			// When performing add operation, we should not reduce the size below initial capacity 
			// unless remove/clear operations are performed. When add operation is performed, we 
			// should only increase the size. ExpandOnly parameter is passed in as true when add
			// operation is performed.
			// 
			if ( currLen < requiredLen || ! expandOnly && currLen > 4 * requiredLen )
			{
				int newLen = this.CalcNewLengthHelper( currLen, requiredLen );
				if ( currLen != newLen && newLen > 1 )
				{
					Entry[] oldArr = _arr;
					Entry[] newArr = new Entry[newLen];
					bool keysAsWeakReferences = _keysAsWeakReferences;

					int removedItemCount = 0;
					for ( int i = 0; i < oldArr.Length; i++ )
					{
						Entry ii = oldArr[i];
						while ( null != ii )
						{
							if ( keysAsWeakReferences && !ii.HasKey( this )
								|| _valuesAsWeakReferences && ! ii.HasValue( this ) )
							{
								ii = ii._next;
								removedItemCount++;
								continue;
							}

							int newIndex = GetIndexHelper( ii._hash, newLen );

							Entry tmp = newArr[newIndex];
							newArr[newIndex] = ii;
							Entry oldNext = ii._next;
							ii._next = tmp;
							ii = oldNext;
						}
					}

					_arr = newArr;
					if ( removedItemCount > 0 )
					{
						_count -= removedItemCount;
						_version++;
					}
				}
			}
		}

		#endregion // ExpandCollapseHelper

		#region FindExistingEntry

		private Entry FindExistingEntry( TKey key )
		{
			int index = this.GetIndex( key );
			Entry ii = _arr[index];
			if ( null != ii )
				return this.FindInList( ii, key );

			return null;
		}

		#endregion // FindExistingEntry

		#region FindInList

		private Entry FindInList( Entry list, TKey key )
		{
			Entry ii = list;
			while ( null != ii )
			{
				if ( ii.IsSameKey( this, key ) )
					return ii;

				ii = ii._next;
			}

			return null;
		}

		#endregion // FindInList

		#region GetHashCode

		private int GetHashCode( TKey key )
		{
			return _keyComparer.GetHashCode( key );
		}

		#endregion // GetHashCode

		#region GetIndex

		private int GetIndex( TKey key )
		{
			return this.GetIndex( this.GetHashCode( key ) );
		}

		private int GetIndex( int hash )
		{
			return GetIndexHelper( hash, _arr.Length );
		}

		#endregion // GetIndex

		#region GetIndexHelper

		private static int GetIndexHelper( int hash, int arrLen )
		{
			// JJD 12/03/09 - TFS25389
			// If the hash is negative then flip the sign. Otherwise we
			// will end up returning a negative index.
			if (hash < 0)
				hash = -hash;

			return hash % arrLen;
		}

		#endregion // GetIndexHelper

		#region TryGetValueHelper

		// SSP 9/29/10 
		// Modified the behavior to auto-collect entries whose values have been garbage collected. Moved code from TryGetValue
		// into the new TryGetValueHelper method and added the includeEntriesWithNullValues parameter.
		// 
		private bool TryGetValueHelper( TKey key, out TValue value, bool includeEntriesWithNullValues )
		{
			Entry ii = this.FindExistingEntry( key );
			if ( null != ii )
			{
				value = ii.GetValue( this );
				// SSP 9/29/10 
				// Take into account the new includeEntriesWithNullValues parameter.
				// 
				// return true;
				return includeEntriesWithNullValues || ! _valuesAsWeakReferences || null != value;
			}

			value = default( TValue );
			return false;
		}

		#endregion // TryGetValueHelper

		#endregion // Private Methods

		#region Internal Methods

		#region GetWeakReferenceTargetSafe

		internal static object GetWeakReferenceTargetSafe(WeakReference weakReference)
		{
			if (null != weakReference)
			{
				try
				{
					return weakReference.Target;
				}
				catch (Exception)
				{
				}
			}

			return null;
		}

		#endregion //GetWeakReferenceTargetSafe

		#endregion // Internal Methods

		#region Public Methods

		#region Add

		/// <summary>
		/// Adds an entry to the dictionary. If an entry with the specified key already exists, 
		/// this method throws an exception.
		/// </summary>
		/// <param name="key">Key of the entry to add.</param>
		/// <param name="value">Value to associate with the key.</param>
		public void Add( TKey key, TValue value )
		{
			this.AddHelper( key, value, false );
		}

		#endregion // Add

		#region Clear

		/// <summary>
		/// Removes all entries from the dictionary.
		/// </summary>
		public void Clear( )
		{
			Array.Clear( _arr, 0, _arr.Length );
			_count = 0;
			_version++;
			this.ExpandCollapseHelper( false );
		}

		#endregion // Clear

		#region Compact

		/// <summary>
		/// Removes entries from the dictionary where keys are no longer alive (have been garbage 
		/// collected). Note that keys can get garbage collected during the process of compacting 
		/// and therefore it's not guarrenteed that all the entries in the dictionary will be 
		/// with live keys after this operation is completed.
		/// </summary>
		/// <param name="removeEntriesWithNullValues">Whether to also remove entries where
		/// value is null or has been garbage collected.</param>
		public void Compact( bool removeEntriesWithNullValues )
		{
			Entry[] arr = _arr;
			bool keysAsWeakReferences = _keysAsWeakReferences;

			int removedItemCount = 0;
			for ( int i = 0; i < arr.Length; i++ )
			{
				Entry ii = arr[i];

				Entry prev = null;
				while ( null != ii )
				{
					if ( keysAsWeakReferences && !ii.HasKey( this )
						|| removeEntriesWithNullValues && !ii.HasValue( this ) )
					{
						if ( null != prev )
							prev._next = ii._next;
						else
							arr[i] = ii._next;

						ii = ii._next;
						removedItemCount++;
					}
					else
					{
						prev = ii;
						ii = ii._next;
					}
				}
			}

			if ( removedItemCount > 0 )
			{
				_count -= removedItemCount;
				_version++;
				this.ExpandCollapseHelper( false );
			}
		}

		#endregion // Compact

		#region ContainsKey

		/// <summary>
		/// Returns true if an entry with the specified key exists in the dictionary.
		/// </summary>
		/// <param name="key">Key to check for existence.</param>
		/// <returns>Returns true if an entry with the specified key exists in the dictionary.</returns>
		public bool ContainsKey( TKey key )
		{
			// SSP 9/29/10 
			// Modified the behavior to auto-collect entries whose values have been garbage collected.
			// 
			//return null != this.FindExistingEntry( key );
			Entry entry = this.FindExistingEntry( key );
			return null != entry && ( ! _valuesAsWeakReferences || entry.HasValue( this ) );
		}

		#endregion // ContainsKey

		#region Indexer[ TKey ]

		/// <summary>
		/// Gets or sets the value associated with the specified key. Set will add an entry for the key 
		/// if it doesn't already exist.
		/// </summary>
		/// <param name="key">Value associated with this key will be returned.</param>
		/// <returns>Value associated with the specified key.</returns>
		public TValue this[TKey key]
		{
			get
			{
				TValue value;
				if ( !this.TryGetValueHelper( key, out value, true ) )
					throw new KeyNotFoundException( );

				return value;
			}
			set
			{
				this.AddHelper( key, value, true );
			}
		}

		#endregion // Indexer[ TKey ]

		#region Remove

		/// <summary>
		/// Removes the entry with the specified key. Does nothing if the specified key doesn't exist.
		/// </summary>
		/// <param name="key">Key of the entry to remove.</param>
		/// <returns>True if an entry was removed. False if entry with the specified key was not found.</returns>
		public bool Remove( TKey key )
		{
			int index = this.GetIndex( key );
			Entry ii = _arr[index];

			Entry prev = null;
			while ( null != ii )
			{
				if ( ii.IsSameKey( this, key ) )
				{
					if ( null != prev )
						prev._next = ii._next;
					else
						_arr[index] = ii._next;

					_count--;
					_version++;
					this.ExpandCollapseHelper( false );
					return true;
				}

				prev = ii;
				ii = ii._next;
			}

			return false;
		}

		#endregion // Remove

		#region TryGetValue

		/// <summary>
		/// Gets the value associated with the specified key. If the entry doesn't exist, returns false.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public bool TryGetValue( TKey key, out TValue value )
		{
			// SSP 9/29/10 
			// Modified the behavior to auto-collect entries whose values have been garbage collected. Refactored into the new
			// TryGetValueHelper method.
			// 
			return this.TryGetValueHelper( key, out value, false );
		}

		#endregion // TryGetValue

		#endregion // Public Methods

		#endregion // Methods

		#region Properties

		#region Public Properties

		#region Count

		/// <summary>
		/// Returns the number of entries in the dictionary. Note that this will not take into account 
		/// any garbage collected items - they will be included in the count.
		/// </summary>
		public int Count
		{
			get
			{
				return _count;
			}
		}

		#endregion // Count

		#region Keys

		/// <summary>
		/// Returns the keys in the dictionary. Only keys with non-null values are returned.
		/// </summary>
		public IEnumerable<TKey> Keys
		{
			get
			{
				return new EntryEnumerable.EnumerableWrapper<TKey>( new EntryEnumerable.KeyCollection( this ) );
			}
		}

		#endregion // Keys

		#region Values

		/// <summary>
		/// Returns the values in the dictionary. Only values with non-null keys are returned.
		/// </summary>
		public IEnumerable<TValue> Values
		{
			get
			{
				return new EntryEnumerable.EnumerableWrapper<TValue>( new EntryEnumerable.ValueCollection( this ) );
			}
		}

		#endregion // Values

		#endregion // Public Properties

		#endregion // Properties

		#region Explicitly Implemented Interfaces

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		void ICollection<KeyValuePair<TKey,TValue>>.Add( KeyValuePair<TKey, TValue> item )
		{
			this.Add( item.Key, item.Value );
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains( KeyValuePair<TKey, TValue> item )
		{
			Entry ii = this.FindExistingEntry( item.Key );
			return null != ii && ii.IsSameKey( this, item.Key ) && ii.IsSameValue( this, item.Value )
				// SSP 9/29/10 
				// Modified the behavior to auto-collect entries whose values have been garbage collected.
				// 
				&& ( ! _valuesAsWeakReferences || null != item.Value );
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo( KeyValuePair<TKey, TValue>[] array, int arrayIndex )
		{
			foreach ( KeyValuePair<TKey, TValue> ii in this )
				array[arrayIndex++] = ii;
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get 
			{ 
				return false; 
			}
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove( KeyValuePair<TKey, TValue> item )
		{
			Entry ii = this.FindExistingEntry( item.Key );
			if ( null != ii && ii.IsSameValue( this, item.Value ) )
			{
				this.Remove( item.Key );
				return true;
			}

			return false;
		}

		#endregion

		#region Keys

		/// <summary>
		/// Returns the keys in the dictionary.
		/// </summary>
		ICollection<TKey> IDictionary<TKey, TValue>.Keys
		{
			get
			{
				return new EntryEnumerable.KeyCollection( this );
			}
		}

		#endregion // Keys

		#region Values

		/// <summary>
		/// Returns the values in the dictionary.
		/// </summary>
		ICollection<TValue> IDictionary<TKey, TValue>.Values
		{
			get
			{
				return new EntryEnumerable.ValueCollection( this );
			}
		}

		#endregion // Values

		#region IEnumerable<KeyValuePair<TKey,TValue>> Members

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator( )
		{
			return new EntryEnumerable.KeyValuePairEnumerator( this );
		}

		#endregion

		#region IEnumerable Members

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator( )
		{
			return ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator( );
		}

		#endregion

		#endregion // Explicitly Implemented Interfaces
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