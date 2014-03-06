using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System.Collections.Specialized;
using System.Reflection;


using Infragistics.Collections.Services;

namespace Infragistics.Services





{
	/// <summary>
	/// Contains static helper methods.
	/// </summary>
	public class CoreUtilities
	{
		
		#region Data Structures

		#region AggregateEnumerable

		internal class AggregateEnumerable<T> : IEnumerable<T>
		{
			#region Enumerator Class

			public class Enumerator : IEnumerator<T>
			{
				private IEnumerator<IEnumerable<T>> _enumerables;
				private IEnumerator<T> _current;

				public Enumerator( params IEnumerable<T>[] enumerables )
					: this( (IEnumerable<IEnumerable<T>>)enumerables )
				{
				}

				public Enumerator( IEnumerable<IEnumerable<T>> enumerables )
				{
					_enumerables = null != enumerables ? enumerables.GetEnumerator( ) : null;
				}

				public void Reset( )
				{
					if ( null != _enumerables )
						_enumerables.Reset( );

					_current = null;
				}

				public bool MoveNext( )
				{
					if ( null != _current && _current.MoveNext( ) )
						return true;

					while ( null != _enumerables && _enumerables.MoveNext( ) )
					{
						IEnumerable<T> ee = _enumerables.Current;
						if ( null != ee )
						{
							_current = ee.GetEnumerator( );
							if ( _current.MoveNext( ) )
								return true;
						}
					}

					return false;
				}

				public T Current
				{
					get
					{
						return null != _current ? _current.Current : default( T );
					}
				}

				#region IEnumerator<T> Members

				object IEnumerator.Current
				{
					get 
					{
						return this.Current;
					}
				}

				#endregion

				#region IDisposable Members

				public void Dispose( )
				{
				}

				#endregion
			}

			#endregion // Enumerator Class

			private IEnumerable<IEnumerable<T>> _enumerables;

			public AggregateEnumerable( params IEnumerable<T>[] enumerables )
				: this( (IEnumerable<IEnumerable<T>>)enumerables )
			{
			}

			public AggregateEnumerable( IEnumerable<IEnumerable<T>> enumerables )
			{
				_enumerables = enumerables;
			}

			public IEnumerator GetEnumerator( )
			{
				return new Enumerator( _enumerables );
			}

			#region IEnumerable<T> Members

			IEnumerator<T> IEnumerable<T>.GetEnumerator( )
			{
				return new Enumerator( _enumerables );
			}

			#endregion
		}

		#endregion // AggregateEnumerable

		// AS 8/14/08 NA 2008 Vol 2
		#region ComparisonComparer<T>
		private sealed class ComparisonComparer<T> : IComparer<T>
		{
			private Comparison<T> _c;

			internal ComparisonComparer( Comparison<T> comparison )
			{
				ValidateNotNull( comparison, "comparison" );
				this._c = comparison;
			}

			#region IComparer<T> Members

			public int Compare( T x, T y )
			{
				return this._c( x, y );
			}

			#endregion
		}
		#endregion //ComparisonComparer<T>

		#region ConverterComparer<T, Z> Class

		// SSP 5/27/10 - XamSchedule
		// Moved this from grid-bag layout manager to here.
		// 
		// SSP 7/10/09 NAS9.2 Synchronous Sizing
		// 
		/// <summary>
		/// A comparer that compares converted values.
		/// </summary>
		/// <typeparam name="T">Type of objects being sorted.</typeparam>
		/// <typeparam name="Z">The type of the value of the object that will be compared.</typeparam>
		internal class ConverterComparer<T, Z> : IComparer<T>
		{
			private Converter<T, Z> _converter;
			private IComparer<Z> _comparer;

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="converter">Converter used to convert <i>T</i> instances to <i>Z</i> values for comparison.</param>
			/// <param name="comparison">Comparison delegate for comparing converted <i>Z</i> values.</param>
			public ConverterComparer( Converter<T, Z> converter, Comparison<Z> comparison )
				: this( converter, CoreUtilities.CreateComparer<Z>( comparison ) )
			{
			}

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="converter">Converter used to convert <i>T</i> instances to <i>Z</i> values for comparison.</param>
			/// <param name="comparer">Comparer for comparing converted <i>Z</i> values.</param>
			public ConverterComparer( Converter<T, Z> converter, IComparer<Z> comparer )
			{
				_converter = converter;
				_comparer = comparer;
			}

			/// <summary>
			/// Compares the two items.
			/// </summary>
			/// <param name="i"></param>
			/// <param name="j"></param>
			/// <returns></returns>
			public int Compare( T i, T j )
			{
				return _comparer.Compare( _converter( i ), _converter( j ) );
			}
		}

		#endregion // ConverterComparer<T, Z> Class

		#region EmptyEnumerable<T>

		internal class EmptyEnumerable<T> : IEnumerable<T>
		{
			#region Vars

			#endregion // Vars

			#region Enumerator Class

			public class Enumerator : IEnumerator<T>
			{
				#region Constructor

				public Enumerator( )
				{
				}

				#endregion // Constructor

				#region IEnumerator<T> Members

				public T Current
				{
					get { throw new InvalidOperationException( ); }
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
					get { throw new InvalidOperationException( ); }
				}

				public bool MoveNext( )
				{
					return false;
				}

				public void Reset( )
				{
				}

				#endregion
			}

			#endregion // Enumerator Class

			#region Constructor

			public EmptyEnumerable( )
			{
			}

			#endregion // Constructor

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator( )
			{
				return new Enumerator( );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return this.GetEnumerator( );
			}

			#endregion
		}

		#endregion // EmptyEnumerable<T>

		#region TypedList<T> class
		/// <summary>
		/// IList&lt;T&gt; implementation that wraps an IList
		/// </summary>
		/// <typeparam name="T">Type of the items in the collection</typeparam>
		internal class TypedList<T> : IList<T>
		{
			#region Member Variables

			private IList _list;

			#endregion // Member Variables

			#region Constructor
			internal TypedList( IList list )
			{
				CoreUtilities.ValidateNotNull( list );
				_list = list;
			}
			#endregion // Constructor

			#region Properties
			internal IList List
			{
				get { return _list; }
			}
			#endregion // Properties

			#region IList<T> Members

			public int IndexOf( T item )
			{
				return _list.IndexOf( item );
			}

			public void Insert( int index, T item )
			{
				_list.Insert( index, item );
			}

			public void RemoveAt( int index )
			{
				_list.RemoveAt( index );
			}

			public T this[int index]
			{
				get
				{
					return (T)_list[index];
				}
				set
				{
					_list[index] = value;
				}
			}

			#endregion

			#region ICollection<T> Members

			public void Add( T item )
			{
				_list.Add( item );
			}

			public void Clear( )
			{
				_list.Clear( );
			}

			public bool Contains( T item )
			{
				return _list.Contains( item );
			}

			public void CopyTo( T[] array, int arrayIndex )
			{
				CoreUtilities.CopyTo<T>( this, array, arrayIndex );
			}

			public int Count
			{
				get { return _list.Count; }
			}

			public bool IsReadOnly
			{
				get { return _list.IsReadOnly; }
			}

			public bool Remove( T item )
			{
				int index = _list.IndexOf( item );

				if ( index >= 0 )
					_list.RemoveAt( index );

				return index >= 0;
			}

			#endregion

			#region IEnumerable<T> Members

			public IEnumerator<T> GetEnumerator( )
			{
				return new TypedEnumerable<T>.Enumerator( _list.GetEnumerator( ) );
			}

			#endregion

			#region IEnumerable Members

			IEnumerator IEnumerable.GetEnumerator( )
			{
				return _list.GetEnumerator( );
			}

			#endregion
		}
		#endregion // TypedList<T> class

		#region ObservableTypedList<T>
		/// <summary>
		/// IList&lt;T&gt; implementation that wraps an IList and supports propogating the collection and property changes from the source collection.
		/// </summary>
		/// <typeparam name="T">Type of the items in the collection</typeparam>
		internal class ObservableTypedList<T> : TypedList<T>
			, INotifyCollectionChanged
			, INotifyPropertyChanged
		{
			#region Constructor
			internal ObservableTypedList( IList list )
				: base( list )
			{
				INotifyCollectionChanged incc = list as INotifyCollectionChanged;

				if ( incc != null )
					incc.CollectionChanged += new NotifyCollectionChangedEventHandler( OnCollectionChanged );

				INotifyPropertyChanged inpc = list as INotifyPropertyChanged;

				if ( inpc != null )
					inpc.PropertyChanged += new PropertyChangedEventHandler( OnCollectionPropertyChanged );
			}
			#endregion // Constructor

			#region Methods
			private void OnCollectionPropertyChanged( object sender, PropertyChangedEventArgs e )
			{
				PropertyChangedEventHandler handler = this.PropertyChanged;

				if ( null != handler )
					handler( this, e );
			}

			private void OnCollectionChanged( object sender, NotifyCollectionChangedEventArgs e )
			{
				NotifyCollectionChangedEventHandler handler = this.CollectionChanged;

				if ( null != handler )
					handler( this, e );
			}
			#endregion // Methods

			#region INotifyPropertyChanged Members

			public event PropertyChangedEventHandler PropertyChanged;

			#endregion

			#region INotifyCollectionChanged Members

			public event NotifyCollectionChangedEventHandler CollectionChanged;

			#endregion
		}
		#endregion // ObservableTypedList<T>

		#endregion // Data Structures

		#region Constructor
		/// <summary>
		/// For internal use
		/// </summary>
		protected CoreUtilities( )
		{
		}
		#endregion // Constructor

		#region Properties

		#region AntirecursionUtility

		[ThreadStatic]
		private static AntirecursionUtility g_antirecursionUtility;

		internal static AntirecursionUtility Antirecursion
		{
			get
			{
				if (null == g_antirecursionUtility)
					g_antirecursionUtility = new AntirecursionUtility();

				return g_antirecursionUtility;
			}
		}

		#endregion // AntirecursionUtility

		#endregion // Properties

		#region AreClose

		internal static bool AreClose( Point pt1, Point pt2 )
		{
			return AreClose( pt1.X, pt2.X )
				&& AreClose( pt1.Y, pt2.Y );
		}

		internal static bool AreClose( Size x, Size y )
		{
			return AreClose( x.Width, y.Width )
				&& AreClose( x.Height, y.Height );
		}

		internal static bool AreClose( double value1, double value2 )
		{
			if ( value1 == value2 )
				return true;

			return Math.Abs( value1 - value2 ) < .0000000001;
		}

		
		internal static bool LessThan( double x, double y )
		{
			return x < y && !AreClose( x, y );
		}

		
		internal static bool GreaterThan( double x, double y )
		{
			return x > y && !AreClose( x, y );
		}

		
		internal static bool LessThanOrClose(double x, double y)
		{
			return x <= y || AreClose(x, y);
		}

		
		internal static bool GreaterThanOrClose(double x, double y)
		{
			return x >= y || AreClose(x, y);
		}

		#endregion //AreClose


		#region AreEqual
		internal static bool AreEqual<T>(IList<T> items1, IList<T> items2, IEqualityComparer<T> comparer = null)
		{
			if (items1 == items2)
				return true;

			if (items1 == null || items2 == null)
				return false;

			if (items1.Count != items2.Count)
				return false;

			if (comparer == null)
				comparer = EqualityComparer<T>.Default;

			for (int i = 0, count = items1.Count; i < count; i++)
			{
				if (!comparer.Equals(items1[i], items2[i]))
					return false;
			}

			return true;
		}
		#endregion // AreEqual

		#region BinarySearch

		#region BinarySearch(IList<DateRange>, DateTime)
		internal static int BinarySearch(IList<DateRange> ranges, DateTime date)
		{
			Func<IList<DateRange>, int, DateRange> callback = (IList<DateRange> list, int index) => { return ranges[index]; };
			return BinarySearchDateRange(ranges, callback, date, ranges.Count);
		}
		#endregion // BinarySearch(IList<DateRange>, DateTime)

		#region BinarySearch<T>( IList<T>, T, IComparer<T>, bool )
		/// <summary>
		/// Helper method for performing a binary search of a sorted list
		/// </summary>
		/// <typeparam name="T">The type of items in the list</typeparam>
		/// <param name="list">The sorted list to be searched</param>
		/// <param name="item">The item to look for</param>
		/// <param name="comparer">The comparer used to perform the search</param>
		/// <param name="ignoreItem">False for a standard binary search. True if the <paramref name="item"/> should 
		/// be ignored in the search. This is useful where the item's sort criteria may have changed to find out where 
		/// the item should be moved to.</param>
		/// <returns>The index where the item in the list. If the item does not exist then it returns the bitwise 
		/// complement. In the case where <paramref name="ignoreItem"/> is true, the index of the item will be returned 
		/// if it exists at the correct location otherwise the bitwise complement of where it should be. Note the complement 
		/// may need to be decremented if doing a move and the item is before the new index.</returns>
		internal static int BinarySearch<T>(IList<T> list, T item, IComparer<T> comparer, bool ignoreItem)
		{
			if (comparer == null)
				comparer = Comparer<T>.Default;

			int si = 0, ei = list.Count - 1;
			int mi = 0;
			EqualityComparer<T> equalComparer = !ignoreItem ? null : EqualityComparer<T>.Default;

			while (si <= ei)
			{
				mi = (si + ei) / 2;

				T tempItem = list[mi];

				// if this is the ignore item...
				if (null != equalComparer && equalComparer.Equals(item, tempItem))
				{
					if (mi > si)
						tempItem = list[--mi];
					else if (mi < ei)
						tempItem = list[++mi];
					else
						return mi;
				}

				int result = comparer.Compare(tempItem, item);

				if (result > 0)
					ei = mi - 1;
				else if (result < 0)
					si = mi + 1;
				else
					return mi;
			}

			// if the index we will return is where the item is then just return the value and
			// not the bitwise complement. in this way we only return a bitwise complement if 
			// the item is not already at the specified location
			if (null != equalComparer && si < list.Count - 1 && equalComparer.Equals(item, list[si]))
				return si;

			return ~si;
		}
		#endregion // BinarySearch<T>( IList<T>, T, IComparer<T>, bool )

		#region BinarySearch(IList<TItem>, Func<TComparison, TItem, int>, TComparison)
		/// <summary>
		/// Helper method for performing a binary search where the search criteria is a separate piece of data
		/// </summary>
		/// <typeparam name="TItem"></typeparam>
		/// <typeparam name="TComparison"></typeparam>
		/// <param name="items"></param>
		/// <param name="comparer"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		internal static int BinarySearch<TItem, TComparison>(IList<TItem> items, Func<TItem, TComparison, int> comparer, TComparison value)
		{
			int si = 0, ei = items.Count - 1;
			int mi = 0;

			while (si <= ei)
			{
				mi = (si + ei) / 2;

				TItem item = items[mi];

				int result = comparer(item, value);

				if (result > 0)
					ei = mi - 1;
				else if (result < 0)
					si = mi + 1;
				else
					return mi;
			}

			return ~si;
		}
		#endregion // BinarySearch(IList<TItem>, Func<TComparison, TItem, int>, TComparison)

		#region BinarySearchDateRange(IList<T>, Func<IList<T>, int, DateRange>, DateTime, int)
		internal static int BinarySearchDateRange<T>(IList<T> rangeProviders, Func<IList<T>, int, DateRange> rangeProviderCallback, DateTime date, int count)
		{
			int si = 0, ei = count - 1;
			int mi = 0;

			while (si <= ei)
			{
				mi = (si + ei) / 2;

				DateRange range = rangeProviderCallback(rangeProviders, mi);

				if (range.Start > date)
					ei = mi - 1;
				else if (range.End <= date) // note we're doing <= because the end date is exclusive
					si = mi + 1;
				else
					return mi;
			}

			return ~si;
		}
		#endregion // BinarySearchDateRange(IList<T>, Func<IList<T>, int, DateRange>, DateTime, int)

		#endregion //BinarySearch	

		#region BuildEmbeddedResourceUri

		/// <summary>
		/// Creates a Uri for a resource embedded in an assembly
		/// </summary>
		/// <param name="assembly">The assembly that contains the resource</param>
		/// <param name="resourcePath">The relative path to the resource (e.g. "Looks/Onyx.xaml")</param>
		/// <returns>A Uri to the resource.</returns>
		public static Uri BuildEmbeddedResourceUri(Assembly assembly, string resourcePath)
		{
			if (assembly == null)
				throw new ArgumentNullException("assembly");

			if (resourcePath == null)
				throw new ArgumentNullException("resourcePath");


			string assemblyName = assembly.FullName;


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)


			StringBuilder sb = new StringBuilder(assemblyName.Length + 15 + resourcePath.Length);

			// JM 04-08-10 TFS Work Item #30630 - Add leading slash.
			sb.Append('/');

			sb.Append(assemblyName);
			sb.Append(";component/");
			sb.Append(resourcePath);

			return new Uri(sb.ToString(), UriKind.RelativeOrAbsolute);
		}

		#endregion //BuildEmbeddedResourceUri

		#region Contains

		internal static bool Contains<T>( IEnumerable<T> e, T item )
		{
			IList<T> listT = e as IList<T>;
			if ( null != listT )
				return listT.IndexOf( item ) >= 0;

			IList list = e as IList;
			if ( null != list )
				return list.IndexOf( item ) >= 0;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			foreach ( T i in e )
			{
				if ( comparer.Equals( i, item ) )
					return true;
			}

			return false;
		}

		internal static bool Contains( IEnumerable e, object item )
		{
			IList list = e as IList;
			if ( null != list )
				return list.IndexOf( item ) >= 0;

			foreach ( object i in e )
			{
				if ( i == item )
					return true;
			}

			return false;
		}

		#endregion // Contains

		#region ConvertDataValue

		[ThreadStatic( )]
		private static Type lastNonConvertibleDataType;

		// SSP 5/3/07 BR22014
		// Added support for using type converters.
		// 
		internal class TypeConverterInfo
		{
			internal TypeConverter convertFromTypeConverter;
			internal TypeConverter convertToTypeConverter;
			internal bool useConvertFromTypeConverterFirst;

			internal TypeConverterInfo( TypeConverter convertFromTypeConverter, TypeConverter convertToTypeConverter,
				bool useConvertFromTypeConverterFirst )
			{
				this.convertFromTypeConverter = convertFromTypeConverter;
				this.convertToTypeConverter = convertToTypeConverter;
				this.useConvertFromTypeConverterFirst = useConvertFromTypeConverterFirst;
			}
		}

		// SSP 5/22/07 BR22014
		// 
		internal static TypeConverterInfo GetTypeConverterInfo( Type objectType, Type convertToType )
		{
			TypeConverter objectTypeConverter = null, convertToTypeConverter = null;


			objectTypeConverter = null != objectType ? TypeDescriptor.GetConverter( objectType ) : null;
			convertToTypeConverter = null != convertToType ? TypeDescriptor.GetConverter( convertToType ) : null;


			if ( null != objectTypeConverter || null != convertToTypeConverter )
			{
				// First use the converter of non-primitive data type. The idea is to let any
				// custom type converters first crack at converting.
				// 
				bool useConvertFromTypeConverterFirst = null != objectType && !objectType.IsPrimitive;

				return new TypeConverterInfo( convertToTypeConverter, objectTypeConverter, useConvertFromTypeConverterFirst );
			}

			return null;
		}

		/// <summary>
		/// Converts 'valueToConvert' to an object of the type 'convertToType'. If it cannot perform the conversion
		/// it returns null. It makes use of any formatting information provided passed in.
		/// </summary>
		/// <returns>Converted value, null if unsuccessful.</returns>
		public static object ConvertDataValue( object valueToConvert, Type convertToType,
			IFormatProvider formatProvider, string format )
		{
			// SSP 6/3/09 - TFS17233 - Optimization
			// In WPF we don't have any property that controls whether we use type converters
			// or not. So let the overload of ConvertDataValue with the actual conversion
			// logic handle the usage of type converters.
			// 
			// ------------------------------------------------------------------------------
			return ConvertDataValue( valueToConvert, convertToType, formatProvider, format, true );
			
#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

			// ------------------------------------------------------------------------------
		}

		/// <summary>
		/// Converts 'valueToConvert' to an object of the type 'convertToType'. If it cannot perform the conversion
		/// it returns null. It makes use of any formatting information provided passed in.
		/// </summary>
		/// <returns>Converted value, null if unsuccessful.</returns>
		// SSP 6/3/09 - TFS17233 - Optimization
		// 
		//internal static object ConvertDataValue( object valueToConvert, Type convertToType,
		//	IFormatProvider formatProvider, string format, TypeConverterInfo typeConverterInfo )
		// SSP 10/12/11 TFS88251 TFS91512
		// Changed the scope from private to internal.
		// 
		internal static object ConvertDataValue( object valueToConvert, Type convertToType,
			IFormatProvider formatProvider, string format, bool useTypeConverters )
		{
			object convertedValue = null;

			// SSP 5/22/07 BR22014
			// We need to get the underlying type if the type is nullable.
			// In the case of nullable types, we need to get the underlying type against which to perform
			// comparisons, since the Convert.ChangeType method does not handle nullable types.
			//
			System.Type dataType = GetUnderlyingType( convertToType );

			Debug.Assert( null != dataType, "Cannot accept null as the data type!" );

			// If the value is DBNull, return itself.
			//
			if ( null == valueToConvert || valueToConvert is DBNull )
			{
				convertedValue = valueToConvert;
			}
			else if ( null != dataType )
			{
				// If valueToConvert is already an instance of desired type, then return itself.
				//
				if ( valueToConvert.GetType( ) == dataType ||
					dataType.IsInstanceOfType( valueToConvert ) )
				{
					convertedValue = valueToConvert;

					// SSP 6/3/09 - TFS17233 - Optimization
					// 
					return convertedValue;
				}

				// SSP 6/3/09 - TFS17233 - Optimization
				// 
				// ----------------------------------------------------------------------------------
				if ( null == convertedValue )
				{
					// SSP 10/12/11 TFS88251 TFS91512
					// Honor the existing 'useTypeConverters' parameter.
					// 
					//TypeConverterInfo typeConverterInfo = GetTypeConverterInfo( valueToConvert.GetType( ), convertToType );
					TypeConverterInfo typeConverterInfo = useTypeConverters ? GetTypeConverterInfo( valueToConvert.GetType( ), convertToType ) : null;

					if ( null != typeConverterInfo )
					{
						
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

						if ( typeof( string ) == dataType )
						{
							try
							{
								convertedValue = ToString( valueToConvert, format, formatProvider, true );
							}
							catch ( Exception )
							{
							}
						}

						// The idea behind for loop is to implement useConvertFromTypeConverterFirst.
						// 
						for ( int i = 0; i <= 1; i++ )
						{
							if ( null == convertedValue
								&& ( 0 == i && typeConverterInfo.useConvertFromTypeConverterFirst
								  || 1 == i && !typeConverterInfo.useConvertFromTypeConverterFirst )
								)
							{
								try
								{
									if ( null != typeConverterInfo.convertFromTypeConverter
										&& typeConverterInfo.convertFromTypeConverter.CanConvertFrom( valueToConvert.GetType( ) ) )
										convertedValue = typeConverterInfo.convertFromTypeConverter.ConvertFrom(
											null, formatProvider as System.Globalization.CultureInfo, valueToConvert );
								}
								catch ( Exception )
								{
								}
							}

							if ( null == convertedValue
								&& ( 0 == i && !typeConverterInfo.useConvertFromTypeConverterFirst
								  || 1 == i && typeConverterInfo.useConvertFromTypeConverterFirst ) )
							{
								try
								{
									if ( null != typeConverterInfo.convertToTypeConverter
										&& typeConverterInfo.convertToTypeConverter.CanConvertTo( dataType ) )
										convertedValue = typeConverterInfo.convertToTypeConverter.ConvertTo(
											null, formatProvider as System.Globalization.CultureInfo, valueToConvert, dataType );
								}
								catch ( Exception )
								{
								}
							}
						}
					}
				}
				// ----------------------------------------------------------------------------------

				// JAS 3/9/05 - Need an explicit check for System.Uri as the target data type.
				//
				if ( null == convertedValue && typeof( System.Uri ) == dataType )
				{
					// If the target data type is Uri, the only way to convert the value is by
					// passing it into the Uri constructor and seeing if it throws an exception.
					//
					try { convertedValue = new Uri( valueToConvert.ToString( ) ); }
					catch ( Exception ) { }
				}

				// SSP 5/23/02
				// NOTE: I am not sure if this behaviour is correct. When data is going back
				// to a column whose data type is string, should we be applying format (Column.Format property)
				// to the data?
				//
				// If the desired type is a string, make use of format string to format
				// if possible, otherwise follow
				//
				if ( null == convertedValue && typeof( string ) == dataType )
				{
					
#region Infragistics Source Cleanup (Region)












#endregion // Infragistics Source Cleanup (Region)

					try
					{
						convertedValue = ToString( valueToConvert, format, formatProvider, false );
					}
					catch ( Exception )
					{
					}
				}

				// AS 7/28/09 Optimization
				// Moved this up from below to avoid unnecessary first chance exceptions.
				//
				// MRS 11/29/05 - BR06949
				// If the desired type is an enum, try to convert the value
				// into an enum value for that type. ChangeType does not do
				// this for some reason. 
				//
				if ( convertedValue == null && dataType.IsEnum )
				{
					// MD 10/4/10 - TFS50092
					// The value passed in may be of a numeric type that is different from the enum's underlying type, 
					// so convert it to the underlying type first.
					Type underlyingType =Enum.GetUnderlyingType(dataType);

					object tempValueToConvert = valueToConvert;
					if (underlyingType != valueToConvert.GetType())
						tempValueToConvert = CoreUtilities.ConvertDataValue(valueToConvert, underlyingType, formatProvider, format) ?? valueToConvert;

					try
					{
						// MD 10/4/10 - TFS50092
						// Use the value that was converted to the underlying data type.
						//if ( Enum.IsDefined( dataType, valueToConvert ) )
						if (Enum.IsDefined(dataType, tempValueToConvert))
							convertedValue = System.Enum.ToObject( dataType, valueToConvert );
					}
					catch ( Exception )
					{
					}
				}

				// MRS 12/28/05 - Only try ChangeType if the valueToConvert
				// is IConvertible. Otherwise, it will always fail and raise 
				// an exception. 
				// SSP 10/30/08 - Optmization
				// If we've already converted, then don't bother with the below logic
				// since it will end up being a NOOP anyways.
				// 
				//if ( valueToConvert is IConvertible )
				if ( null == convertedValue && valueToConvert is IConvertible )
				{
					string valueToConvertAsString = valueToConvert as string;

					// MRS 2/17/06 - BR10225
					// If the valueToConvert is a string and the dataType is not
					// IConvertible, we will have stored it the last time we blew up.
					// In that case, we know this won't work, so skip it so we
					// don't raise an exception. 
					if ( lastNonConvertibleDataType != dataType ||
						
						//!( valueToConvert is string ) )
						valueToConvertAsString == null )
					{
						// Try using the static method of the Convert class to convert the
						// data type if the above ConvertFrom call failed
						//
						if ( null == convertedValue )
						{
							try
							{
								convertedValue = Convert.ChangeType( valueToConvert, dataType, formatProvider );
							}
							catch ( Exception )
							{
								// MRS 2/17/06 - BR10225
								// If the valueToConvert is a string and the dataType is not
								// IConvertible, store the data type, so we can avoid this 
								// exception multiple times.
								
								//if ( valueToConvert is string &&
								if ( valueToConvertAsString != null &&
									// MRS 3/24/06 - BR10225 - Had this backward
									//!dataType.IsAssignableFrom(typeof(IConvertible)))
									!typeof( IConvertible ).IsAssignableFrom( dataType ) )
								{
									lastNonConvertibleDataType = dataType;
								}
							}
						}

						// Then try without the format info
						//
						if ( null == convertedValue )
						{
							try
							{

								convertedValue = Convert.ChangeType( valueToConvert, dataType );

							}
							catch ( Exception )
							{
							}
						}
					}
				}

				// If the desired type is a string, then just call ToString to convert to
				// string if above calls fail.
				//
				if ( null == convertedValue && typeof( string ) == dataType )
				{
					try
					{
						if ( null == convertedValue )
							convertedValue = valueToConvert.ToString( );
					}
					catch ( Exception )
					{
					}
				}

				
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

			}

			return convertedValue;
		}

		#endregion // ConvertDataValue

		#region CopyTo<T>

		internal static void CopyTo<T>( IEnumerable<T> source, T[] dest, int destStartIndex )
		{
			ICollection<T> sourceColl = source as ICollection<T>;

			if ( null != sourceColl )
			{
				sourceColl.CopyTo( dest, destStartIndex );
			}
			else
			{
				foreach ( T ii in source )
					dest[destStartIndex++] = ii;
			}
		}

		internal static void CopyTo( IEnumerable source, Array dest, int destStartIndex )
		{
			foreach ( object ii in source )
				dest.SetValue( ii, destStartIndex++ );
		}

		#endregion // CopyTo<T>

		// AS 5/18/11 NA 11.2
		// Moved AggregateCollection out of the schedule assembly so we needed this method from ScheduleUtilities.
		//
		#region CreateAddRemoveNCCArgs

		internal static NotifyCollectionChangedEventArgs CreateAddRemoveNCCArgs(
			bool add, IList addRemoveMultiItems, int index)
		{
			NotifyCollectionChangedAction action = add ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove;



#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			return new NotifyCollectionChangedEventArgs(action, addRemoveMultiItems, index);

		}

		#endregion // CreateAddRemoveNCCArgs

		// AS 8/14/08 NA 2008 Vol 2
		#region CreateComparer<T>

		/// <summary>
		/// Creates an IComparer that wraps the specified comparison.
		/// </summary>
		/// <typeparam name="T">The type of object being compared</typeparam>
		/// <param name="comparison">The comparison delegate to use when comparing elements.</param>
		/// <returns>A new IComparer&lt;T&gt; that uses the specified <paramref name="comparison"/> to perform the compare.</returns>
		public static IComparer<T> CreateComparer<T>( Comparison<T> comparison )
		{
			return new ComparisonComparer<T>( comparison );
		}

		/// <summary>
		/// If the specified comparer is IComparer&lt;T&gt; then returns it otherwise creates an 
		/// IComparer&lt;T&gt; that wraps the specified IComparer.
		/// </summary>
		/// <typeparam name="T">The type of object being compared</typeparam>
		/// <param name="comparer">Non-generic comparer.</param>
		/// <returns>A new IComparer&lt;T&gt; that uses the specified IComparer to perform the comparison.</returns>
		public static IComparer<T> CreateComparer<T>( IComparer comparer )
		{
			return comparer as IComparer<T> ?? new ComparerWrapper<T>( comparer );
		}

		#endregion //CreateComparer<T>

		// AS 5/18/11 NA 11.2
		// Moved AggregateCollection out of the schedule assembly so we needed this method from ScheduleUtilities.
		//
		#region CreateReplaceNCCArgs

		internal static NotifyCollectionChangedEventArgs CreateReplaceNCCArgs(IList oldItems, IList newItems, int index)
		{


#region Infragistics Source Cleanup (Region)











#endregion // Infragistics Source Cleanup (Region)

			return new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItems, oldItems, index);

		}

		#endregion // CreateReplaceNCCArgs

		#region EnsureIsANumber
		internal static void EnsureIsANumber( double number )
		{
			if ( double.IsNaN( number ) )
				throw new ArgumentOutOfRangeException("number", "The value cannot be 'double.NaN'."); 
		}
		#endregion //EnsureIsANumber

		#region GetCount

		internal static int GetCount( IEnumerable e )
		{
			return GetCount( e, false );
		}

		internal static int GetCount( IEnumerable e, bool nonOptimized )
		{
			if ( !nonOptimized )
			{
				ICollection coll = e as ICollection;

				if ( null != coll )
					return coll.Count;
			}

			int count = 0;

			if ( null != e )
			{
				foreach ( object o in e )
					count++;
			}

			return count;
		}

		#endregion // GetCount

		#region GetFirstItem

		internal static T GetFirstItem<T>( IEnumerable<T> e )
		{
			if ( null != e )
			{
				foreach ( T i in e )
					return i;
			}

			return default( T );
		}

		internal static T GetFirstItem<T>( IEnumerable e, bool optimized )
		{
			if ( null != e )
			{
				if ( optimized )
				{
					IList<T> listTyped = e as IList<T>;
					if ( null != listTyped )
						return listTyped.Count > 0 ? listTyped[0] : default( T );

					IList list = e as IList;
					if ( null != list )
						return list.Count > 0 ? (T)list[0] : default( T );
				}

				foreach ( T i in e )
					return i;
			}

			return default( T );
		}

		#endregion // GetFirstItem

		// JJD 10/27/11 - TFS92815 - Moved from DataBindingHelpers and made internal
		#region IsKnownType

		/// <summary>
		/// Checks if the type is a known type (to Infragistics controls).
		/// </summary>
		internal static bool IsKnownType(Type type)
		{
			if (type == null)
				return false;

			// JJD 04/17/08
			// if the type is nullable get the underlying type
			type = CoreUtilities.GetUnderlyingType(type);

			if (type.IsPrimitive ||
				typeof(decimal) == type ||
				typeof(string) == type ||
				// AS 1/13/09
				// The CLR 4.0 framework was changed such that the Type.Equals(Type) 
				// method was made virtual. We could call the Type.Equals(object) 
				// but to be consistent I just changed these to reference comparison 
				// as we do with the others.
				//
				//typeof(System.DateTime).Equals(type) ||
				//typeof(System.DayOfWeek).Equals(type) ||
				typeof(System.DateTime) == type ||
				typeof(System.DayOfWeek) == type ||
				typeof(System.Windows.Media.Color) == type ||




				typeof(System.Windows.Media.Visual).IsAssignableFrom(type) ||
				typeof(System.Windows.Media.Animation.Animatable).IsAssignableFrom(type)



				)
				return true;
			else
				return false;
		}

		#endregion //IsKnownType

		#region GetIndexOf

		internal static int GetIndexOf( IEnumerable e, object item )
		{
			IList list = e as IList;
			if ( null != list )
				return list.IndexOf( item );

			int index = 0;

			if ( null != e )
			{
				foreach ( object o in e )
				{
					if ( o == item )
						return index;

					index++;
				}
			}

			return -1;
		}

		internal static int GetIndexOf<T>( IEnumerable<T> e, T item )
		{
			IList<T> listGeneric = e as IList<T>;
			if ( null != listGeneric )
				return listGeneric.IndexOf( item );

			IList list = e as IList;
			if ( null != list )
				return list.IndexOf( item );

			int index = 0;

			if ( null != e )
			{
				EqualityComparer<T> comparer = EqualityComparer<T>.Default;

				foreach ( T o in e )
				{
					if ( comparer.Equals( o, item ) )
						return index;

					index++;
				}
			}

			return -1;
		}

		#endregion // GetIndexOf

		#region GetItemAt

		internal static object GetItemAt( IEnumerable e, int index )
		{
			IList list = e as IList;
			if ( null != list )
				return list[index];

			if ( null != e )
			{
				foreach ( object o in e )
				{
					if ( 0 == index-- )
						return o;
				}
			}

			return null;
		}

		internal static bool GetItemAt<T>( IEnumerable<T> e, int index, out T item )
		{
			item = default( T );

			IList<T> listGeneric = e as IList<T>;
			if ( null != listGeneric )
			{
				if ( index >= listGeneric.Count )
					return false;

				item = listGeneric[index];
				return true;
			}

			IList list = e as IList;
			if ( null != list )
			{
				if ( index >= list.Count )
					return false;
				
				item = (T)list[index];
				return true;
			}

			if ( null != e )
			{
				foreach ( T o in e )
				{
					if ( 0 == index-- )
					{
						item = o;
						return true;
					}
				}
			}

			return false;
		}

		#endregion // GetItemAt

		#region GetItemsInRange<T>

		/// <summary>
		/// Gets items from the list which must be an IList or IList&lt;T&gt;.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list">Must be an IList or IList&lt;T&gt;.</param>
		/// <param name="startIndex">Start of the range.</param>
		/// <param name="count">Number of items to return.</param>
		/// <returns>Array of items.</returns>
		internal static T[] GetItemsInRange<T>( IEnumerable list, int startIndex, int count )
		{
			T[] ret = new T[count];

			IList<T> lt = list as IList<T>;
			if ( null != lt )
			{
				for ( int i = 0; i < count; i++ )
					ret[i] = lt[startIndex + i];
			}
			else
			{
				IList l = list as IList;
				if ( null != l )
				{
					for ( int i = 0; i < count; i++ )
						ret[i] = (T)l[startIndex + i];
				}
				else
					throw new InvalidOperationException( "Source must be an IList<T> or IList." );
			}

			return ret;
		}

		#endregion // GetItemsInRange<T>

		#region GetUnderlyingType

		/// <summary>
		/// Takes a Type and returns the underlying (non-nullable) type, if the Type is nullable. If the specified type is not nullable, then the passed-in type is returned. 
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static Type GetUnderlyingType( Type type )
		{
			if ( type == null )
				return null;

			// Check for Nullable types
			System.Type nullabletype = System.Nullable.GetUnderlyingType( type );
			if ( nullabletype != null )
				type = nullabletype;

			return type;
		}

		#endregion // GetUnderlyingType

		#region GetWeakReferenceTargetSafe

		/// <summary>
		/// Wraps the 'get' of the Target property in a try/catch to prevent unhandled exceptions
		/// </summary>
		/// <param name="weakReference">The WeakRefernce holding the target.</param>
		/// <returns>The Target or null if an exception was thrown.</returns>
		public static object GetWeakReferenceTargetSafe( WeakReference weakReference )
		{
			if ( null != weakReference )
			{
				try
				{
					return weakReference.Target;
				}
				catch ( Exception )
				{
				}
			}

			return null;
		}

		#endregion //GetWeakReferenceTargetSafe

		#region HasItems

		internal static bool HasItems( IEnumerable e )
		{
			if ( null != e )
			{
				if ( e is ICollection )
					return ( (ICollection)e ).Count > 0;

				if ( null != e )
				{
					foreach ( object o in e )
						return true;
				}
			}

			return false;
		}

		#endregion // HasItems

		#region IsValueEmpty

		/// <summary>
		/// Returns true if the specified val is null, DBNull, empty string, or DependencyProperty.UnsetValue.
		/// </summary>
		/// <param name="val">Value to test</param>
		internal static bool IsValueEmpty( object val )
		{
			return null == val
				|| DBNull.Value == val
				|| string.Empty == val as string
				|| DependencyProperty.UnsetValue == val;
		}

		#endregion // IsValueEmpty

		#region Normalize
		internal static void Normalize( ref DateTime start, ref DateTime end )
		{
			if ( end < start )
			{
				DateTime temp = start;
				start = end;
				end = temp;
			}
		}
		#endregion

		#region RaiseReadOnlyCollectionException

		internal static void RaiseReadOnlyCollectionException( )
		{
			throw new NotSupportedException( "Collection is read-only." );
		}

		#endregion // RaiseReadOnlyCollectionException

		#region RemoveAll

		/// <summary>
		/// Removes all occurrences of itemToRemove from list.
		/// </summary>
		/// <param name="list">List whose items should be removed</param>
		/// <param name="itemToRemove">The value of the items that should be removed</param>
		public static void RemoveAll<T>( List<T> list, T itemToRemove )
		{
			int s = RemoveAllHelper( list, itemToRemove );
			list.RemoveRange( s, list.Count - s );
		}

		/// <summary>
		/// Removes all occurrences of itemToRemove from list.
		/// </summary>
		/// <param name="list">List whose items should be removed</param>
		/// <param name="itemToRemove">The value of the items that should be removed</param>
		public static void RemoveAll<T>( IList<T> list, T itemToRemove )
		{
			int s = RemoveAllHelper( list, itemToRemove );

			for ( int i = list.Count - 1; i >= s; i-- )
				list.RemoveAt( i );
		}


		/// <summary>
		/// Removes all occurrences of itemToRemove from list.
		/// </summary>
		/// <param name="list">List whose items should be removed</param>
		/// <param name="itemToRemove">The value of the items that should be removed</param>
		public static void RemoveAll<T>( ObservableCollectionExtended<T> list, T itemToRemove )
		{
			list.BeginUpdate( );
			try
			{
				int s = RemoveAllHelper( list, itemToRemove );
				list.RemoveRange( s, list.Count - s );
			}
			finally
			{
				list.EndUpdate( );
			}
		}


		private static int RemoveAllHelper<T>( IList<T> list, T itemToRemove )
		{
			int delta = 0;
			int count = list.Count;

			EqualityComparer<T> comparer = EqualityComparer<T>.Default;

			for ( int i = 0; i < count; i++ )
			{
				if ( comparer.Equals( list[i], itemToRemove ) )
					delta++;
				else if ( 0 != delta )
					list[i - delta] = list[i];
			}

			return count - delta;
		}

		#endregion // RemoveAll
		
		// JJD 02/27/12 - Added
		#region RoundToIntegralValue

		internal static double RoundToIntegralValue(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
				return value;

			if (value < 0)
			{
				double ceiling = Math.Ceiling(value);

				if (value - ceiling <= -.5)
					return ceiling - 1;
				else
					return ceiling;
			}
			else
			{
				double floor = Math.Floor(value);

				if (value - floor >= .5)
					return floor + 1;
				else
					return floor;
			}
		}

		#endregion //RoundToIntegralValue

		#region SortMerge

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. It requires allocation of an array equal in size to the array to be sorted.
		/// Merge sort should be used if the operation of comparing items is expensive.
		/// </summary>
		/// <param name="array">Array to be sorted.</param>
		/// <param name="comparer">Object used to compare the items during the sort</param>
		public static void SortMerge( object[] array, IComparer comparer )
		{
			SortMerge( (object[])array, null, comparer );
		}

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
		/// The method will make use of that array instead of allocating one. If null is passed in, 
		/// then it will allocate one. Merge sort should be used if the operation of comparing items 
		/// is expensive.
		/// </summary>
		/// <param name="array">Array to be sorted.</param>
		/// <param name="tempArray">Null or a temporary array equal (or greater) in size to arr.</param>
		/// <param name="comparer">Object used to compare the items during the sort</param>
		public static void SortMerge( object[] array, object[] tempArray, IComparer comparer )
		{
			SortMerge( array, tempArray, comparer, 0, array.Length - 1 );
		}

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
		/// The method will make use of that array instead of allocating one. If null is passed in, 
		/// then it will allocate one. Merge sort should be used if the operation of comparing items 
		/// is expensive.
		/// </summary>
		/// <param name="array">Array to be sorted.</param>
		/// <param name="tempArray">Null or a temporary array equal (or greater) in size to arr.</param>
		/// <param name="comparer">Object used to compare the items during the sort</param>
		/// <param name="si">Start index in the array.</param>
		/// <param name="ei">End index in the array.</param>
		public static void SortMerge( object[] array, object[] tempArray, IComparer comparer, int si, int ei )
		{
			if ( array == null )
				throw new ArgumentNullException( "array" );

			if ( comparer == null )
				throw new ArgumentNullException( "comparer" );

			if ( null == tempArray )
				tempArray = (object[])array.Clone( );
			else
				Array.Copy( array, tempArray, array.Length );

			// MD 8/7/07 - 7.3 Performance
			// Use generics
			//SortMergeHelper( arr, tmpArr, comparer, si, ei );
			SortMergeHelper( array, tempArray, new ComparerWrapper<object>( comparer ), si, ei );
		}

		/// <summary>
		/// Sorts the passed in list based on the passed in comparer using a modified merge-sort
		/// algorithm. 
		/// </summary>
		/// <param name="list">The list to be sorted.</param>
		/// <param name="comparer">The comparer (must not be null).</param>
		public static void SortMergeGeneric<T>( List<T> list, IComparer<T> comparer )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			if ( comparer == null )
				throw new ArgumentNullException( "comparer" );

			// get the items as an array
			T[] array = list.ToArray( );

			// sort the array
			SortMergeGeneric<T>( array, null, comparer );

			// clear the list
			list.Clear( );

			// Add the sorted items back into the list
			list.AddRange( array );
		}

		
		
		
		/// <summary>
		/// Sorts the passed in list based on the passed in comparer using a modified merge-sort
		/// algorithm. 
		/// </summary>
		/// <param name="list">The list to be sorted.</param>
		/// <param name="comparer">The comparer (must not be null).</param>
		/// <param name="startIndex">Start index in the array. Items between the specified start index and end index will be sorted. Other items will be left as they are.</param>
		/// <param name="endIndex">End index in the array. Items between the specified start index and end index will be sorted. Other items will be left as they are.</param>
		public static void SortMergeGeneric<T>( List<T> list, IComparer<T> comparer, int startIndex, int endIndex )
		{
			if ( list == null )
				throw new ArgumentNullException( "list" );

			if ( comparer == null )
				throw new ArgumentNullException( "comparer" );

			// get the items as an array

			T[] array = new T[1 + endIndex - startIndex];

			list.CopyTo( startIndex, array, 0, array.Length );

			// sort the array
			SortMergeGeneric<T>( array, null, comparer );

			for ( int i = 0; i < array.Length; i++ )
			{
				list[startIndex + i] = array[i];
			}
		}

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. It requires allocation of an array equal in size to the array to be sorted.
		/// Merge sort should be used if the operation of comparing items is expensive.
		/// </summary>
		/// <param name="array">The array to be sorted</param>
		/// <param name="comparer">The comparer to use for the sort</param>
		public static void SortMergeGeneric<T>( T[] array, IComparer<T> comparer )
		{
			SortMergeGeneric<T>( array, null, comparer );
		}

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
		/// The method will make use of that array instead of allocating one. If null is passed in, 
		/// then it will allocate one. Merge sort should be used if the operation of comparing items 
		/// is expensive.
		/// </summary>
		/// <param name="array">Array to be sorted.</param>
		/// <param name="tempArray">Null or a temporary array equal (or greater) in size to <paramref name="array"/>.</param>
		/// <param name="comparer">Comparer.</param>
		public static void SortMergeGeneric<T>( T[] array, T[] tempArray, IComparer<T> comparer )
		{
			SortMergeGeneric<T>( array, tempArray, comparer, 0, array.Length - 1 );
		}

		/// <summary>
		/// Sorts the passed in array based on the passed in comparer using a modified merge-sort
		/// algorithm. Optionally you can pass in a temporary array equal (or greater) in size to arr. 
		/// The method will make use of that array instead of allocating one. If null is passed in, 
		/// then it will allocate one. Merge sort should be used if the operation of comparing items 
		/// is expensive.
		/// </summary>
		/// <param name="array">Array to be sorted.</param>
		/// <param name="tempArray">Null or a temporary array equal (or greater) in size to <paramref name="array"/>.</param>
		/// <param name="comparer">Comparer.</param>
		/// <param name="startIndex">Start index in the array.</param>
		/// <param name="endIndex">End index in the array.</param>
		public static void SortMergeGeneric<T>( T[] array, T[] tempArray, IComparer<T> comparer, int startIndex, int endIndex )
		{
			if ( array == null )
				throw new ArgumentNullException( "arr" );

			if ( comparer == null )
				throw new ArgumentNullException( "comparer" );

			if ( null == tempArray )
				tempArray = (T[])array.Clone( );
			else
				Array.Copy( array, tempArray, array.Length );

			SortMergeHelper<T>( array, tempArray, comparer, startIndex, endIndex );
		}






		// MD 8/7/07 - 7.3 Performance
		// Use generics
		//private static void SortMergeHelper( object[] arr, object[] tmpArr, IComparer comparer, int si, int ei )
		private static void SortMergeHelper<T>( T[] arr, T[] tmpArr, IComparer<T> comparer, int startIndex, int endIndex )
		{
			int i, j, k, m;

			// MD 8/7/07 - 7.3 Performance
			// Use generics
			//object o1 = null, o2 = null;
			T o1 = default( T ), o2 = default( T );

			if ( endIndex - startIndex < 6 )
			{
				for ( i = 1 + startIndex; i <= endIndex; i++ )
				{
					o1 = arr[i];

					for ( j = i; j > startIndex; j-- )
					{
						o2 = arr[j - 1];

						if ( comparer.Compare( o1, o2 ) < 0 )
							arr[j] = o2;
						else
							break;
					}

					if ( i != j )
						arr[j] = o1;
				}
				return;
			}

			m = ( startIndex + endIndex ) / 2;
			SortMergeHelper( tmpArr, arr, comparer, startIndex, m );
			SortMergeHelper( tmpArr, arr, comparer, 1 + m, endIndex );

			for ( i = startIndex, j = 1 + m, k = startIndex; k <= endIndex; k++ )
			{
				if ( i <= m )
					o1 = tmpArr[i];
				if ( j <= endIndex )
					o2 = tmpArr[j];

				if ( j > endIndex || i <= m && comparer.Compare( o1, o2 ) <= 0 )
				{
					arr[k] = o1;
					i++;
				}
				else
				{
					arr[k] = o2;
					j++;
				}
			}
		}

		private class ComparerWrapper<T> : IComparer<T>
		{
			private IComparer comparer;

			public ComparerWrapper( IComparer comparer )
			{
				this.comparer = comparer;
			}

			public int Compare( T x, T y )
			{
				return this.comparer.Compare( x, y );
			}
		}

		#endregion // SortMerge

		#region Swap<T>

		// SSP 6/25/09 - NAS9.2
		// 

		/// <summary>
		/// Swaps values in a list at specified indexes.
		/// </summary>
		/// <typeparam name="T">Type of elements in the list.</typeparam>
		/// <param name="arr">The list to modify</param>
		/// <param name="x">Index location 1</param>
		/// <param name="y">Index location 2</param>
		public static void Swap<T>( IList<T> arr, int x, int y )
		{
			T tmp = arr[x];
			arr[x] = arr[y];
			arr[y] = tmp;
		}

		// AS 3/13/12 TFS104664
		/// <summary>
		/// Swaps the values of the specified 
		/// </summary>
		/// <typeparam name="T">The type of variable to be swapped</typeparam>
		/// <param name="value1">The member to be updated with the value of <paramref name="value2"/></param>
		/// <param name="value2">The member to be updated with the value of <paramref name="value1"/></param>
		internal static void Swap<T>( ref T value1, ref T value2 )
		{
			T temp = value2;
			value2 = value1;
			value1 = temp;
		}
		#endregion // Swap<T>

		#region ToString
		internal static string ToString( object value, string format, IFormatProvider provider, bool ignoreFormatIfEmpty )
		{
			string text = null;

			// follow the string builder and get the customformatter if there is one
			if ( provider != null )
			{
				ICustomFormatter customFormatter = provider.GetFormat( typeof( ICustomFormatter ) ) as ICustomFormatter;

				if ( customFormatter != null )
					text = customFormatter.Format( format, value, provider );
			}

			// if the custom formatter couldn't convert the value and the value is 
			// formattable, then let it convert to text. since some users only 
			// honored the ToString if there was a format, we'll conditionally 
			// call ToString if the caller didn't care if there was a format or
			// if there was a format
			if ( null == text && value is IFormattable && ( ignoreFormatIfEmpty == false || string.IsNullOrEmpty( format ) == false ) )
				text = ( (IFormattable)value ).ToString( format, provider );

			return text;
		}
		#endregion //ToString

		#region Traverse

		/// <summary>
		/// Traverses the enumerator. This may be used to force allocation of lazily allocated list items.
		/// </summary>
		/// <param name="e"></param>
		internal static void Traverse( IEnumerable e )
		{
			foreach ( object o in e )
			{
			}
		} 

		#endregion // Traverse

		#region ValidateEnum

		private static bool IsEnumFlags( Type enumType )
		{
			object[] flagsAttributes = enumType.GetCustomAttributes( typeof( FlagsAttribute ), true );
			return null != flagsAttributes && flagsAttributes.Length > 0;
		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && !IsEnumFlags( enumType ) )




				throw new InvalidEnumArgumentException( enumType.Name, (int)enumVal, enumType );

		}



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static void ValidateEnum( string argumentName, Type enumType, object enumVal )
		{
			if ( !Enum.IsDefined( enumType, enumVal ) && !IsEnumFlags( enumType ) )



				throw new InvalidEnumArgumentException( argumentName, (int)enumVal, enumType );

		}

		#endregion // ValidateEnum

		// JJD 1/22/10 - Tiles - added
		#region ValidateIsNotNan

		internal static void ValidateIsNotNan(double value)
		{
			if (double.IsNaN(value))
				throw new ArgumentException(SR.GetString("LE_ValueCannotBeNan"));
		}

		#endregion //ValidateIsNotNan

		// JJD 1/22/10 - Tiles - added
		#region ValidateIsNotInfinity

		internal static void ValidateIsNotInfinity(double value)
		{
			if (double.IsInfinity(value))
				throw new ArgumentException(SR.GetString("LE_ValueCannotBeInfinity"));
		}

		#endregion //ValidateIsNotInfinity	
   
        // JJD 1/22/10 - Tiles - added
        #region ValidateIsNotNegative

        internal static void ValidateIsNotNegative(double value)
        {
            if (value < 0)
                throw new ArgumentException(SR.GetString("LE_ValueCannotBeNegative"));
        }

		internal static void ValidateIsNotNegative(double value, string paramName)
        {
            if (value < 0)
                throw new ArgumentException(paramName, SR.GetString("LE_ValueCannotBeNegative"));
        }

        #endregion //ValidateIsNotNegative	

		#region ValidateNotEmpty

		internal static void ValidateNotEmpty( string value )
		{
			if ( string.IsNullOrEmpty( value ) )
				throw new ArgumentException( );
		}

		internal static void ValidateNotEmpty( string value, string paramName )
		{
			if ( string.IsNullOrEmpty( value ) )
				throw new ArgumentException( paramName );
		}

		#endregion // ValidateNotEmpty

		#region ValidateNotNull

		internal static void ValidateNotNull( object value )
		{
			if ( null == value )
				throw new ArgumentNullException( );
		}

		internal static void ValidateNotNull( object value, string paramName )
		{
			if ( null == value )
				throw new ArgumentNullException( paramName );
		}

		#endregion // ValidateNotNull

		#region Nested Data Structures

		#region AntirecursionUtility Class

		/// <summary>
		/// Used instead of anti-recursion flag to prevent an action from being taken recursively.
		/// </summary>
		internal class AntirecursionUtility
		{
			#region Data Structures

			#region ID Structures

			private struct ID
			{
				internal object _item;
				internal object _flagId;

				internal ID(object item, object flagId)
				{
					_item = item;
					_flagId = flagId;
				}

				internal static int GetHashCode(object o)
				{
					return null != o ? o.GetHashCode() : 0;
				}

				public override int GetHashCode()
				{
					return GetHashCode(_item) ^ GetHashCode(_flagId);
				}

				public override bool Equals(object obj)
				{
					if (obj is ID)
					{
						ID id = (ID)obj;

						if (id._item == _item && id._flagId == _flagId)
							return true;
					}

					return false;
				}
			}

			#endregion // ID Structures

			#endregion // Data Structures

			#region Member Vars

			private Dictionary<ID, int> _map;

			#endregion // Member Vars

			#region Map

			private Dictionary<ID, int> Map
			{
				get
				{
					if (null == _map)
						_map = new Dictionary<ID, int>();

					return _map;
				}
			}

			#endregion // Map

			#region Enter

			public bool Enter(object item, object flagId, bool disallowMultipleEntries)
			{
				ID id = new ID(item, flagId);

				int val;
				if (this.Map.TryGetValue(id, out val))
				{
					if (disallowMultipleEntries)
						return false;
				}
				else
				{
					val = 0;
				}

				this.Map[id] = 1 + val;

				return true;
			}

			#endregion // Enter

			#region Exit

			public void Exit(object item, object flagId)
			{
				ID id = new ID(item, flagId);

				Debug.Assert(null != _map && _map.ContainsKey(id));

				int val;
				if (null != _map && _map.TryGetValue(id, out val))
				{
					val--;

					if (val <= 0)
						_map.Remove(id);
					else
						_map[id] = val;
				}
				else
				{
					Debug.Assert(false, "Exit called without accompanying Enter call.");
				}
			}

			#endregion // Exit

			#region InProgress

			public bool InProgress(object item, object flagId)
			{
				return null != _map && _map.ContainsKey(new ID(item, flagId));
			}

			#endregion // InProgress
		}

		#endregion // AntirecursionUtility Class

		#endregion // Nested Data Structures
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