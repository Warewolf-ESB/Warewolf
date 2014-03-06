using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Collections;
using System.Diagnostics;
using System.Windows.Data;
using System.Linq;


using Infragistics.Services;
using Infragistics.Controls.Schedules.Services;

namespace Infragistics.Collections.Services





{	
	#region IntKeyArrayMapBase<TKey, TValue> Class

	/// <summary>
	/// Map implementation that uses an array for the storage. The key values are either
	/// integer or convertible to integer and the integer values and the smallest integer
	/// value should be no less than 0 and the largest value shouldn't generally be a 
	/// very large value because the array will be allocated based on the largest value
	/// encountered.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	internal abstract class IntKeyArrayMapBase<TKey, TValue> : IMap<TKey, TValue>
	{
		#region Nested Data Structures

		#region Enumerator Class

		protected class Enumerator : IEnumerator<TKey>
		{
			#region Member Vars

			private IntKeyArrayMapBase<TKey, TValue> _map;
			protected int _index;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="map">Map whose entries will be enumerated.</param>
			internal Enumerator( IntKeyArrayMapBase<TKey, TValue> map )
			{
				_map = map;

				this.Reset( );
			}

			#endregion // Constructor

			#region IEnumerator<TKey> Members

			public virtual TKey Current
			{
				get
				{
					return _map.GetKeyValue( _index );
				}
			}

			#endregion

			#region IDisposable Members

			public void Dispose( )
			{
			}

			#endregion

			#region IEnumerator Members

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public bool MoveNext( )
			{
				TValue[] arr = _map._array;
				int max = _map._maxIndex;

				while ( ++_index <= max )
				{
					if ( ! EqualityComparer<TValue>.Default.Equals( default( TValue ), arr[_index] ) )
						return true;
				}

				return false;
			}

			public void Reset( )
			{
				_index = -1;
			}

			#endregion
		}

		#endregion // Enumerator Class 

		#endregion // Nested Data Structures

		#region Member Vars

		protected TValue[] _array;
		protected int _maxIndex;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		protected IntKeyArrayMapBase( )
		{
			_array = new TValue[4];
		}

		#endregion // Constructor

		#region Indexer

		public virtual TValue this[TKey key]
		{
			get
			{
				int i = this.GetIntValue( key );
				return i < _array.Length ? _array[i] : default( TValue );
			}
			set
			{
				int i = this.GetIntValue( key );
				if ( i > _maxIndex )
				{
					_maxIndex = i;
					if ( i >= _array.Length )
					{
						TValue[] newArr = new TValue[Math.Max( 1 + i, 2 * _array.Length )];
						_array.CopyTo( newArr, 0 );
						_array = newArr;
					}
				}

				_array[i] = value;
			}
		}

		#endregion // Indexer

		#region Methods

		#region Public Methods

		#region Clear

		/// <summary>
		/// Clears all values.
		/// </summary>
		public void Clear( )
		{
			_maxIndex = 0;
			_array.Initialize( );
		} 

		#endregion // Clear

		#region GetIntValue

		/// <summary>
		/// Gets the integer value of the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public abstract int GetIntValue( TKey key );

		#endregion // GetIntValue

		#region GetKeyValue

		/// <summary>
		/// Gets the key associated with the specified integer value.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public abstract TKey GetKeyValue( int val );

		#endregion // GetKeyValue

		#endregion // Public Methods 

		#endregion // Methods

		#region IEnumerable<TKey> Members

		public virtual IEnumerator<TKey> GetEnumerator( )
		{
			return new Enumerator( this );
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion
	}

	#endregion // IntKeyArrayMapBase<TKey, TValue> Class

	#region IntKeyArrayMap<TKey, TValue> Class

	/// <summary>
	/// Map implementation that uses an array for the storage. The key values are either
	/// integer or convertible to integer and the integer values and the smallest integer
	/// value should be no less than 0 and the largest value shouldn't generally be a 
	/// very large value because the array will be allocated based on the largest value
	/// encountered.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	internal class IntKeyArrayMap<TKey, TValue> : IntKeyArrayMapBase<TKey, TValue>
	{
		#region Member Vars

		private Converter<TKey, int> _keyToIntConverter;
		private Converter<int, TKey> _intToKeyConverter;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public IntKeyArrayMap( Converter<TKey, int> keyToIntConverter, Converter<int, TKey> intToKeyConverter )
			: base( )
		{
			CoreUtilities.ValidateNotNull( keyToIntConverter );
			CoreUtilities.ValidateNotNull( intToKeyConverter );

			_keyToIntConverter = keyToIntConverter;
			_intToKeyConverter = intToKeyConverter;
		}

		#endregion // Constructor

		#region Base Overrides

		#region GetIntValue

		/// <summary>
		/// Gets the integer value of the specified key.
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public override int GetIntValue( TKey key )
		{
			return _keyToIntConverter( key );
		}

		#endregion // GetIntValue

		#region GetKeyValue

		/// <summary>
		/// Gets the key associated with the specified integer value.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public override TKey GetKeyValue( int val )
		{
			return _intToKeyConverter( val );
		}

		#endregion // GetKeyValue

		#endregion // Base Overrides
	}

	#endregion // IntKeyArrayMap<TKey, TValue> Class

	#region DictionaryMap<TKey, TValue> Class

	/// <summary>
	/// Map implementation that uses dictionary for storage.
	/// </summary>
	/// <typeparam name="TKey">Key type.</typeparam>
	/// <typeparam name="TValue">Value type.</typeparam>
	internal class DictionaryMap<TKey, TValue> : IMap<TKey, TValue>
	{
		#region Member Vars

		private Dictionary<TKey, TValue> _dictionary; 

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		public DictionaryMap( )
		{
			_dictionary = new Dictionary<TKey, TValue>( );
		} 

		#endregion // Constructor

		#region Indexer

		public TValue this[TKey key]
		{
			get
			{
				TValue val;
				if ( _dictionary.TryGetValue( key, out val ) )
					return val;

				return default( TValue );
			}
			set
			{
				_dictionary[key] = value;
			}
		} 

		#endregion // Indexer

		#region IEnumerable<TKey> Members

		public IEnumerator<TKey> GetEnumerator( )
		{
			return _dictionary.Keys.GetEnumerator( );
		}

		#endregion

		#region IEnumerable Members

		IEnumerator IEnumerable.GetEnumerator( )
		{
			return this.GetEnumerator( );
		}

		#endregion
	} 

	#endregion // DictionaryMap<TKey, TValue> Class

	#region MapsFactory Class

	internal class MapsFactory
	{
		#region Nested Data Structures

		#region IntMap<TValue> Class

		internal class IntMap<TValue> : IntKeyArrayMapBase<int, TValue>
		{
			#region Nested Data Structures

			#region Enumerator Class

			private class IntEnumerator : Enumerator
			{
				#region Constructor

				/// <summary>
				/// Constructor.
				/// </summary>
				/// <param name="map">Map whose entries will be enumerated.</param>
				internal IntEnumerator( IntMap<TValue> map )
					: base( map )
				{
				}

				#endregion // Constructor

				#region Current

				public override int Current
				{
					get
					{
						return _index;
					}
				} 

				#endregion // Current
			}

			#endregion // Enumerator Class  

			#endregion // Nested Data Structures

			#region Base Overrides

			#region GetIntValue

			/// <summary>
			/// Gets the integer value of the specified key.
			/// </summary>
			/// <param name="key"></param>
			/// <returns></returns>
			public override int GetIntValue( int key )
			{
				return key;
			}

			#endregion // GetIntValue

			#region GetKeyValue

			/// <summary>
			/// Gets the key associated with the specified integer value.
			/// </summary>
			/// <param name="val"></param>
			/// <returns></returns>
			public override int GetKeyValue( int val )
			{
				return val;
			}

			#endregion // GetKeyValue

			#region Indexer

			public override TValue this[int key]
			{
				get
				{
					return key <= _maxIndex ? _array[key] : default( TValue );
				}
				set
				{
					base[key] = value;
				}
			}

			#endregion // Indexer

			#region IEnumerable<int> Members

			public override IEnumerator<int> GetEnumerator( )
			{
				return new IntEnumerator( this );
			}

			#endregion

			#endregion // Base Overrides
		}

		#endregion // IntMap<TValue> Class 

		#endregion // Nested Data Structures

		#region Methods

		#region Public Methods

		#region CreateMapHelper<TKey, TValue>

		public static IMap<TKey, TValue> CreateMapHelper<TKey, TValue>( )
		{
			Type t = typeof( TKey );

			if ( typeof( int ) == t )
				return (IMap<TKey, TValue>)new IntMap<TValue>( );

			if ( typeof( AppointmentProperty ) == t )
				return (IMap<TKey, TValue>)new IntKeyArrayMap<AppointmentProperty, TValue>( i => (int)i, i => (AppointmentProperty)i );

			if ( typeof( JournalProperty ) == t )
				return (IMap<TKey, TValue>)new IntKeyArrayMap<JournalProperty, TValue>( i => (int)i, i => (JournalProperty)i );

			if ( typeof( TaskProperty ) == t )
				return (IMap<TKey, TValue>)new IntKeyArrayMap<TaskProperty, TValue>( i => (int)i, i => (TaskProperty)i );

			if ( typeof( ResourceProperty ) == t )
				return (IMap<TKey, TValue>)new IntKeyArrayMap<ResourceProperty, TValue>( i => (int)i, i => (ResourceProperty)i );

			if ( typeof( ResourceCalendarProperty ) == t )
				return (IMap<TKey, TValue>)new IntKeyArrayMap<ResourceCalendarProperty, TValue>( i => (int)i, i => (ResourceCalendarProperty)i );

			return new DictionaryMap<TKey, TValue>( );
		}

		#endregion // CreateMapHelper<TKey, TValue>  

		#endregion // Public Methods

		#region Internal Methods

		#region SetValues

		internal static void SetValues<TKey, TValue>( IMap<TKey, TValue> map, IEnumerable<TKey> keys, TValue value )
		{
			foreach ( TKey ii in keys )
				map[ii] = value;
		}

		#endregion // SetValues

		#endregion // Internal Methods

		#endregion // Methods
	}

	#endregion // MapsFactory Class
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