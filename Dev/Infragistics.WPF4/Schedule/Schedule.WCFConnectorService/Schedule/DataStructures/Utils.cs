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
using Infragistics.Collections.Services;

namespace Infragistics.Controls.Schedules.Services





{
	#region StoragePropsInfo Class

	internal abstract class StoragePropsInfo
	{
		#region Nested Data Structures

		#region CopyMethod Enum

		internal enum CopyMethod
		{
			None,
			CopyValue,
			CopyClone,
			CopyContents
		} 

		#endregion // CopyMethod Enum

		#region DefaultValueFactory<TItem, TValue> Class
			
		internal class DefaultValueFactory<TItem, TValue> : IDefaultValueFactory<TItem, TValue>
		{
			private Func<TItem, TValue> _creator;

			public DefaultValueFactory( Func<TItem, TValue> creator )
			{
				ScheduleUtilities.ValidateNotNull( creator );
				_creator = creator;
			}

			public TValue CreateDefaultValue( TItem item )
			{
				return _creator( item );
			}

			public object CreateDefaultValue( object item )
			{
				return this.CreateDefaultValue( (TItem)item );
			}
		} 
		
		#endregion // DefaultValueFactory<TItem, TValue> Class

		#region DelegateEqualityComparer

		internal class DelegateEqualityComparer<T> : IEqualityComparer<T>, IEqualityComparer
		{
			private Func<T, T, bool> _equalsComparer;
			private Func<T, int> _hashCodeProvider;

			internal DelegateEqualityComparer( Func<T, T, bool> equalsComparer, Func<T, int> hashCodeProvider )
			{
				CoreUtilities.ValidateNotNull( equalsComparer );
				CoreUtilities.ValidateNotNull( hashCodeProvider );

				_equalsComparer = equalsComparer;
				_hashCodeProvider = hashCodeProvider;
			}

			public bool Equals( T x, T y )
			{
				return _equalsComparer( x, y );
			}

			public int GetHashCode( T obj )
			{
				return _hashCodeProvider( obj );
			}

			bool IEqualityComparer.Equals( object x, object y )
			{
				return this.Equals( (T)x, (T)y );
			}

			int IEqualityComparer.GetHashCode( object obj )
			{
				return this.GetHashCode( (T)obj );
			}
		} 

		#endregion // DelegateEqualityComparer

		#region MergeMethod Enum

		internal enum MergeMethod
		{
			CopyAll,
			CopyNonEmptyValues,
			CopyNonEmptyValuesOnlyIfDestIsEmpty
		}

		#endregion // MergeMethod Enum

		#region PropInfo Class

		internal class PropInfo
		{
			#region Member Vars

			internal readonly int _key;
			internal readonly string _name;
			internal readonly Type _type;
			internal readonly object _mappedFields;
			internal readonly IValueConverter _converter;
			internal readonly bool _isReadOnly;
			internal readonly CopyMethod _copyMethod;
			internal readonly IDefaultValueFactory _defaultValueFactory;
			private readonly IEqualityComparer _equalityComparer;

			
			
			
			internal readonly object _defaultValue;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="key">Property key.</param>
			/// <param name="name">Property name.</param>
			/// <param name="type">Property type.</param>
			/// <param name="mappedFields">Either a single field or IEnumerable of fields to which this property is mapped.
			/// For example, it can be a AppointmentProperty instance or IEnumerable&lt;AppointmentProperty&gt;.</param>
			/// <param name="isReadOnly">Whether the property is read-only.</param>
			/// <param name="converter">Converter that should be used to convert data source value to the value of the property
			/// and other way around as well.</param>
			/// <param name="copyMethod">Specifies if and how the value of the property will be copied when item is cloned.</param>
			/// <param name="defaultValue">Default value of the property.</param>
			/// <param name="defaultValueFactory">If specified, it's used to initialize the default value of the property.</param>
			/// <param name="equalityComparer">Used for comparing values.</param>
			internal PropInfo( int key, string name, Type type, object mappedFields = null, bool isReadOnly = false, 
				IValueConverter converter = null, CopyMethod copyMethod = CopyMethod.CopyValue, object defaultValue = null,
				IDefaultValueFactory defaultValueFactory = null, IEqualityComparer equalityComparer = null )
			{
				_key = key;
				_name = name;
				_type = type;
				_mappedFields = mappedFields;
				_isReadOnly = isReadOnly;
				_converter = converter;
				_copyMethod = copyMethod;
				_defaultValue = defaultValue;
				_defaultValueFactory = defaultValueFactory;
				_equalityComparer = equalityComparer;
			}

			#endregion // Constructor

			#region AreValuesEqual

			/// <summary>
			/// Checks to see if the values in the property stores are different for this property.
			/// </summary>
			/// <typeparam name="T">Item type.</typeparam>
			/// <param name="x">This object's value for the property will be compared against the 'y' object's value for the property.</param>
			/// <param name="xStorage">Property storage for 'x' item.</param>
			/// <param name="y">This object's value for the property will be compared against the 'X' object's value for the property.</param>
			/// <param name="yStorage">Property storage for 'y' item.</param>
			/// <returns>True if the property values are equal, false otherwise.</returns>
			internal virtual bool AreValuesEqual<T>( T x, IPropertyStorage<T, int> xStorage, T y, IPropertyStorage<T, int> yStorage )
			{
				object xVal = xStorage.GetValue<object>( x, _key );
				object yVal = yStorage.GetValue<object>( y, _key );

				return null != _equalityComparer ? _equalityComparer.Equals( xVal, yVal ) : object.Equals( xVal, yVal );
			}

			#endregion // AreValuesEqual

			#region GetMappedField<T>

			/// <summary>
			/// Gets the field to which this property is mapped to. The &lt;T&gt; template parameter specifies
			/// the type of the mapped field value (like AppointmentProperty enum, or TaskProperty enum).
			/// </summary>
			/// <typeparam name="T">The type of the mapped field value, like AppointmentProperty enum or TaskProperty enum.</typeparam>
			/// <param name="mappedField">This will be set to the any matching mapped field.</param>
			/// <returns>True if a match is found, false otherwise.</returns>
			internal bool GetMappedField<T>( out T mappedField )
			{
				object v = _mappedFields;
				if ( v is T )
				{
					mappedField = (T)v;
					return true;
				}

				// A single property on ActivityBase can be mapped to members of multiple enums. For example, ActivityBase.Start is mapped to
				// AppointmentProperty.Start, JournalProperty.Start and TaskProperty.Start. Therefore find the one that matches
				// the TMappingKey enum type.
				// 
				IEnumerable e = v as IEnumerable;
				if ( null != e )
				{
					foreach ( object i in e )
					{
						if ( i is T )
						{
							mappedField = (T)i;
							return true;
						}
					}
				}

				mappedField = default( T );
				return false;
			}

			#endregion // GetMappedField<T>

			#region CreateStore

			internal virtual IPropertyStore CreateStore( )
			{
				return new DictionaryPropertyStore<object, object>( );
			}

			#endregion // CreateStore

			#region GetValue

			/// <summary>
			/// Gets the value of the property for the specified 'x' object.
			/// </summary>
			/// <typeparam name="T">Item type.</typeparam>
			/// <param name="x">The item whose property value to get.</param>
			/// <param name="xStorage">Property storage.</param>
			/// <returns>The value of the property.</returns>
			internal virtual object GetValue<T>( T x, IPropertyStorage<T, int> xStorage )
			{
				return xStorage.GetValue<object>( x, _key );
			} 

			#endregion // GetValue
		}

		#endregion // PropInfo Class

		#region TypedPropInfo<TItem, TValue> Class

		internal class TypedPropInfo<TItem, TValue> : PropInfo, IFieldValueAccessor<TValue>
		{
			#region Member Vars
			
			private readonly Func<TItem, IPropertyStorage<TItem, int>> _storageRetriever;
			private readonly IEqualityComparer<TValue> _equalityComparer;
			
			
			
			internal readonly new TValue _defaultValue;

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="key">Property key.</param>
			/// <param name="name">Property name.</param>
			/// <param name="storageRetriever">Used to get the storage for an item that will be used by the 
			/// IFieldValueAccessor&lt;TValue&gt; implementation to retrieve property values.</param>
			/// <param name="mappedFields">>Either a single field or IEnumerable of fields to which this property is mapped.
			/// For example, it can be a AppointmentProperty instance or IEnumerable&lt;AppointmentProperty&gt;.</param>
			/// <param name="isReadOnly">Whether the property is read-only.</param>
			/// <param name="converter">Converter that should be used to convert data source value to the value of the property
			/// and other way around as well.</param>
			/// <param name="copyMethod">Specifies if and how the value of the property will be copied when item is cloned.</param>
			/// <param name="defaultValueFactoryFunc">If specified, it's used to initialize the default value of the property.</param>
			/// <param name="defaultValue">Default value of the property.</param>
			/// <param name="equalityComparer">Used for comparing values.</param>
			internal TypedPropInfo( int key, string name, Func<TItem, IPropertyStorage<TItem, int>> storageRetriever,
				object mappedFields = null, bool isReadOnly = false, IValueConverter converter = null,
				CopyMethod copyMethod = CopyMethod.CopyValue, object defaultValue = null, 
				Func<TItem, TValue> defaultValueFactoryFunc = null, IEqualityComparer<TValue> equalityComparer = null )
				: base( key, name, typeof( TValue ), mappedFields, isReadOnly, converter, copyMethod, defaultValue,
					
					
					
					
					
					null != defaultValueFactoryFunc ? new DefaultValueFactory<TItem, TValue>( defaultValueFactoryFunc )
						: ( null != defaultValue ? new DefaultValueFactory<TItem, TValue>( i => (TValue)defaultValue ) : null ), 
				(IEqualityComparer)equalityComparer )
			{
				CoreUtilities.ValidateNotNull( storageRetriever );
				_storageRetriever = storageRetriever;
				_equalityComparer = equalityComparer;
				_defaultValue = null != defaultValue ? (TValue)defaultValue : default( TValue );
			}

			#endregion // Constructor

			#region AreValuesEqual

			/// <summary>
			/// Checks to see if the values in the property stores are different for this property.
			/// </summary>
			/// <typeparam name="T">Item type.</typeparam>
			/// <param name="x">This object's value for the property will be compared against the 'y' object's value for the property.</param>
			/// <param name="xStorage">Property storage for 'x' item.</param>
			/// <param name="y">This object's value for the property will be compared against the 'X' object's value for the property.</param>
			/// <param name="yStorage">Property storage for 'y' item.</param>
			/// <returns>True if the property values are equal, false otherwise.</returns>
			internal override bool AreValuesEqual<T>( T x, IPropertyStorage<T, int> xStorage, T y, IPropertyStorage<T, int> yStorage )
			{
				TValue xVal = xStorage.GetValue<TValue>( x, _key );
				TValue yVal = yStorage.GetValue<TValue>( y, _key );

				return null != _equalityComparer ? _equalityComparer.Equals( xVal, yVal ) : EqualityComparer<TValue>.Default.Equals( xVal, yVal );
			}

			#endregion // AreValuesEqual

			#region GetValue

			public virtual TValue GetValue( object dataItem )
			{
				TItem item = (TItem)dataItem;
				return _storageRetriever( item ).GetValue<TValue>( item, _key );
			}

			/// <summary>
			/// Gets the value of the property for the specified 'x' object.
			/// </summary>
			/// <typeparam name="T">Item type.</typeparam>
			/// <param name="x">The item whose property value to get.</param>
			/// <param name="xStorage">Property storage.</param>
			/// <returns>The value of the property.</returns>
			internal override object GetValue<T>( T x, IPropertyStorage<T, int> xStorage )
			{
				return xStorage.GetValue<TValue>( x, _key );
			}

			#endregion // GetValue

			#region SetValue

			public virtual void SetValue( object dataItem, TValue newValue )
			{
				TItem item = (TItem)dataItem;
				_storageRetriever( item ).SetValue<TValue>( item, _key, newValue );
			}

			#endregion // SetValue

			#region IFieldValueAccessor Interface Implementation

			object IFieldValueAccessor.GetValue( object dataItem )
			{
				return this.GetValue( dataItem );
			}

			void IFieldValueAccessor.SetValue( object dataItem, object newValue )
			{
				this.SetValue( dataItem, null != newValue ? (TValue)newValue : default( TValue ) );
			}

			#endregion // IFieldValueAccessor Interface Implementation

			#region CreateStore

			internal override IPropertyStore CreateStore( )
			{
				return new DictionaryPropertyStore<TItem, TValue>( );
			} 

			#endregion // CreateStore
		}

		#endregion // TypedPropInfo<TItem, TValue> Class

		#endregion // Nested Data Structures

		#region Member Vars

		/// <summary>
		/// Listing of all the properties.
		/// </summary>
		private IMap<int, PropInfo> _props;

		/// <summary>
		/// These properties are candidates for serializing out in unmapped properties field if explicit mappings
		/// are not provided. Certain properties such as Id, Start, End etc... are not suitable for serializing
		/// in unmapped properties field since we perform LINQ queries on them. Further more, certain properties
		/// that are states, like Error, also should not be saved in unmapped properties field.
		/// </summary>
		internal readonly IMap<int, bool> _unmappedPropertiesStoreCandidates = MapsFactory.CreateMapHelper<int, bool>( );
		
		private IMap<int, IDefaultValueFactory> _cachedDefaultValueFactories = null;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		protected StoragePropsInfo( )
		{
			IMap<int, PropInfo> map = MapsFactory.CreateMapHelper<int, PropInfo>( );
			this.Initialize( map );

			_props = map;
		}

		#endregion // Constructor

		#region Properties

		#region Internal Properties

		#region Props

		internal IMap<int, PropInfo> Props
		{
			get
			{
				return _props;
			}
		}

		#endregion // Props

		#endregion // Internal Properties

		#endregion // Properties

		#region Methods

		#region Protected Methods

		#region CopyCloneOverride

		protected virtual bool CopyCloneOverride( PropInfo propInfo, object sourceVal, ref object destVal )
		{
			if ( null != sourceVal )
			{
				if ( sourceVal is RecurrenceBase )
				{
					destVal = ( (RecurrenceBase)sourceVal ).Clone( );
					return true;
				}

				// SSP 7/21/11 TFS81068
				// If the property has a converter specified then use that.
				// 
				// --------------------------------------------------------------------------------------
				IValueConverter converter = propInfo._converter;
				if ( null != converter )
				{
					var culture = ScheduleUtilities.ParseCulture;

					string text = converter.ConvertBack( sourceVal, typeof( string ), null, culture ) as string;
					if ( !string.IsNullOrEmpty( text ) )
					{
						destVal = converter.Convert( text, sourceVal.GetType( ), null, culture );
						if ( null != destVal )
							return true;
					}
				}
				// --------------------------------------------------------------------------------------
			}

			return false;
		}

		#endregion // CopyCloneOverride

		#region CopyContentsOverride

		protected virtual bool CopyContentsOverride( PropInfo propInfo, object sourceVal, object destVal, MergeMethod mergeMethod )
		{
			if ( propInfo._type == typeof( MetadataPropertyValueStore ) )
			{
				MetadataPropertyValueStore source = sourceVal as MetadataPropertyValueStore;
				MetadataPropertyValueStore dest = destVal as MetadataPropertyValueStore;
				if ( null != source && null != dest )
				{
					dest.CopyValuesFrom( source, mergeMethod );
					return true;
				}
				else if ( null == source && null != dest && MergeMethod.CopyAll == mergeMethod )
				{
					// If the source is null then clear all values in dest.
					// 
					dest.ClearAllValues( );
				}
			}

			return false;
		}

		#endregion // CopyContentsOverride

		#region Initialize

		protected abstract void Initialize( IMap<int, PropInfo> props );

		#endregion // Initialize

		#endregion // Protected Methods

		#region Internal Methods

		#region CopyValues

		internal void CopyValues<T>( T sourceItem, IPropertyStorage<T, int> source, T destItem, IPropertyStorage<T, int> dest )
		{
			this.Merge<T>( sourceItem, source, destItem, dest, MergeMethod.CopyAll );
		} 

		#endregion // CopyValues

		#region CreateHelper

		internal static T CreateHelper<TItem, T>( TItem item )
			where T : new( )
		{
			return new T( );
		}

		#endregion // CreateHelper

		#region CreateDefaultMetadata

		// SSP 7/12/12 TFS100159
		
		
		
		
		internal static DictionaryMetadataPropertyValueStore CreateDefaultMetadata<TItem>( TItem item )
			where TItem: IPropertyChangeListener
		{
			var r = new DictionaryMetadataPropertyValueStore( );

			// SSP 7/12/12 TFS100159
			// 
			r.AddListener( item, true );

			return r;
		}

		#endregion // CreateDefaultMetadata

		#region FillMap

		internal static void FillMap( PropInfo[] infos, IMap<int, PropInfo> map )
		{
			for ( int i = 0; i < infos.Length; i++ )
			{
				PropInfo ii = infos[i];

				Debug.Assert( null == map[ii._key], "Map already contains an item at the index." );

				map[ii._key] = ii;
			}
		}

		#endregion // FillMap

		#region GetAllMappedFields<TMappedField>

		internal IEnumerable<TMappedField> GetAllMappedFields<TMappedField>( )
		{
			List<TMappedField> list = new List<TMappedField>( );

			IMap<int, PropInfo> props = this.Props;
			foreach ( int i in props )
			{
				PropInfo ii = this.Props[i];
				TMappedField iiMappedField;
				if ( ii.GetMappedField<TMappedField>( out iiMappedField ) )
					list.Add( iiMappedField );
			}

			return list;
		}

		#endregion // GetAllMappedFields<TMappedField>

		#region GetDefaultValueFactories

		
		
		/// <summary>
		/// Gets default value factories for properties that have them.
		/// </summary>
		/// <returns>Map where the key is the property and value is the default value factory.</returns>
		internal IMap<int, IDefaultValueFactory> GetDefaultValueFactories( )
		{
			if ( null == _cachedDefaultValueFactories )
			{
				var map = MapsFactory.CreateMapHelper<int, IDefaultValueFactory>( );

				foreach ( int ii in this.Props )
				{
					var defValFactory = this.Props[ii]._defaultValueFactory;
					if ( null != defValFactory )
						map[ii] = defValFactory;
				}

				_cachedDefaultValueFactories = map;
			}

			return _cachedDefaultValueFactories;
		} 

		#endregion // GetDefaultValueFactories

		#region GetMappedPropInfo<TMappedField>

		internal PropInfo GetMappedPropInfo<TMappedField>( TMappedField mappedField )
		{
			IEqualityComparer<TMappedField> comparer = EqualityComparer<TMappedField>.Default;

			IMap<int, PropInfo> props = this.Props;
			foreach ( int i in props )
			{
				PropInfo ii = this.Props[i];
				TMappedField iiMappedField;
				if ( ii.GetMappedField<TMappedField>( out iiMappedField )
					&& comparer.Equals( iiMappedField, mappedField ) )
					return ii;
			}

			return null;
		}

		#endregion // GetMappedPropInfo<TMappedField>

		#region GetMatchingProps

		internal IMap<int, PropInfo> GetMatchingProps( Predicate<PropInfo> predicate )
		{
			IMap<int, PropInfo> map = MapsFactory.CreateMapHelper<int, PropInfo>( );

			var props = this.Props;
			foreach ( int i in props )
			{
				var ii = props[i];
				if ( predicate( ii ) )
					map[i] = ii;
			}

			return map;
		}

		#endregion // GetMatchingProps

		#region InitializeNewStore

		internal void InitializeNewStore<T>( T item, ref IPropertyStorage<T, int> storage, IPropertyStorage<T, int> newStorage )
		{
			if ( storage != newStorage )
			{
				if ( null != storage )
					this.Merge( item, storage, item, newStorage, StoragePropsInfo.MergeMethod.CopyNonEmptyValuesOnlyIfDestIsEmpty, true );

				storage = newStorage;
			}
		} 

		#endregion // InitializeNewStore

		#region IsDataDifferent

		internal bool IsDataDifferent<T>( T x, IPropertyStorage<T, int> xStorage, T y, IPropertyStorage<T, int> yStorage, IMap<int, bool> propertiesToCheck, IMap<int, bool> propsWithDifferentValues = null )
		{
			return this.IsDataDifferent( x, xStorage, y, yStorage,
				null != propertiesToCheck ? ( ii => propertiesToCheck[ii._key] ) : (Predicate<PropInfo>)null, propsWithDifferentValues );
		}

		internal bool IsDataDifferent<T>( T x, IPropertyStorage<T, int> xStorage, T y, IPropertyStorage<T, int> yStorage, Predicate<PropInfo> propertiesToCheck, IMap<int, bool> propsWithDifferentValues = null )
		{
			bool ret = false;

			IMap<int, StoragePropsInfo.PropInfo> props = this.Props;
			foreach ( int prop in props )
			{
				PropInfo propInfo = props[prop];
				if ( null == propertiesToCheck || propertiesToCheck( propInfo ) )
				{
					if ( ! propInfo.AreValuesEqual( x, xStorage, y, yStorage ) )
					{
						ret = true;

						if ( null != propsWithDifferentValues )
							propsWithDifferentValues[prop] = true;
						else
							break;
					}
				}
			}

			return ret;
		}

		#endregion // IsDataDifferent

		#region GetPropInfoFromName

		internal PropInfo GetPropInfoFromName( string name )
		{
			IMap<int, PropInfo> props = this.Props;
			foreach ( int i in props )
			{
				PropInfo ii = props[i];
				if ( ii._name == name )
					return ii;
			}

			return null;
		} 

		#endregion // GetPropInfoFromName

		#region GetPropFromName

		internal int GetPropFromName( string name )
		{
			PropInfo propInfo = this.GetPropInfoFromName( name );
			return null != propInfo ? propInfo._key : -1;
		}

		#endregion // GetPropFromName

		#region Merge<T>

		internal void Merge<T>( T item, IPropertyStorage<T, int> source, IPropertyStorage<T, int> dest, MergeMethod mergeMethod )
		{
			this.Merge( item, source, item, dest, mergeMethod );
		}

		internal void Merge<T>( T sourceItem, IPropertyStorage<T, int> source, T destItem, IPropertyStorage<T, int> dest,
			MergeMethod mergeMethod, bool initializingNewStore = false, IMap<int, bool> skipProperties = null )
		{
			if ( null != source && null != dest
				// SSP 5/24/12 - Optimization
				// 
				&& source != dest 
				)
			{
				IMap<int, StoragePropsInfo.PropInfo> props = this.Props;
				foreach ( int prop in props )
				{
					PropInfo propInfo = props[prop];

					CopyMethod copyMethod = initializingNewStore 
						? ( propInfo._isReadOnly ? propInfo._copyMethod :  CopyMethod.CopyValue )
						: propInfo._copyMethod;

					if ( CopyMethod.None != copyMethod && ( null == skipProperties || !skipProperties[prop] ) )
					{
						object val = source.HasValue( sourceItem, prop ) 
							? source.GetValue<object>( sourceItem, prop )
							
							
							
							
							
							//: null;
							: propInfo._defaultValue;

						bool copy;
						switch ( mergeMethod )
						{
							case MergeMethod.CopyNonEmptyValues:
								copy = !IsValueEmpty( val );
								break;
							case MergeMethod.CopyNonEmptyValuesOnlyIfDestIsEmpty:
								copy = !IsValueEmpty( val )
									&& ( !dest.HasValue( destItem, prop ) || IsValueEmpty( dest.GetValue<object>( destItem, prop ) ) );
								break;
							case MergeMethod.CopyAll:
							default:
								copy = true;
								break;
						}

						// SSP 7/17/12 TFS100159
						// Don't copy if copy was set to false above.
						// 
						//if ( CopyMethod.CopyContents == copyMethod )
						if ( CopyMethod.CopyContents == copyMethod )
						{
							copy = false;
							object destVal = dest.GetValue<object>( destItem, prop );
							this.CopyContentsOverride( propInfo, val, destVal, mergeMethod );
						}

						if ( copy )
						{
							if ( CopyMethod.CopyClone == copyMethod )
							{
								object tmp = val;
								if ( this.CopyCloneOverride( propInfo, val, ref tmp ) )
									val = tmp;
							}

							dest.SetValue<object>( destItem, prop, val );
						}
					}
				}
			}
		}

		#endregion // Merge<T>

		#endregion // Internal Methods

		#region Private Methods

		#region IsValueEmpty

		private static bool IsValueEmpty( object val )
		{
			if ( CoreUtilities.IsValueEmpty( val ) )
				return true;

			if ( val is DateTime && 0 == ( (DateTime)val ).Ticks )
				return true;

			return false;
		}

		#endregion // IsValueEmpty

		#endregion // Private Methods

		#endregion // Methods
	} 

	#endregion // StoragePropsInfo Class

	#region ValueGetter Class

	internal class ValueGetter : FrameworkElement, IValueConverter
	{
		private ConverterInfo _converterInfo;

		public ValueGetter( string path, ConverterInfo converterInfo )
		{
			Binding binding = new Binding( path );
			binding.Mode = BindingMode.TwoWay;
			_converterInfo = converterInfo;

			if ( null != converterInfo )
			{
				binding.Converter = this;
				binding.ConverterParameter = converterInfo._parameter;
				binding.ConverterCulture = converterInfo._culture;
			}

			this.SetBinding( ValueProperty, binding );
		}

		#region Value

		/// <summary>
		/// Identifies the <see cref="Value"/> dependency property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyPropertyUtilities.Register(
			"Value",
			typeof( object ),
			typeof( ValueGetter ),
			DependencyPropertyUtilities.CreateMetadata( null )
		);

		public object Value
		{
			get
			{
				return (object)this.GetValue( ValueProperty );
			}
			set
			{
				this.SetValue( ValueProperty, value );
			}
		}

		#endregion // Value

		internal object GetValueForDataItem( object dataItem )
		{
			this.DataContext = dataItem;

			object val = this.Value;

			this.DataContext = null;

			return val;
		}

		internal void SetValueOnDataItem( object dataItem, object newValue )
		{
			this.DataContext = dataItem;

			this.Value = newValue;

			this.DataContext = null;
		}

		public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			// Parameter and culture should be the same as the ones that _converterInfo has since that's what we
			// set those values on the binding to in the constructor.
			// 
			if ( null != _converterInfo )
				return _converterInfo.Convert( value );

			return value;
		}


		public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
		{
			// Parameter and culture should be the same as the ones that _converterInfo has since that's what we
			// set those values on the binding to in the constructor.
			// 
			if ( null != _converterInfo )
				return _converterInfo.ConvertBack( value, targetType );

			return value;
		}
	} 

	#endregion // ValueGetter Class

	#region WeakAction Class

	/// <summary>
	/// A class used to specify a parameterless action without rooting the object where the action is
	/// to be performed. Creating a pointer to an instance method and holding onto it roots the object
	/// containing the instance method. If you want to not root the object, then use this class to
	/// create a parameterless action that delegates to a static parametered action that you provide,
	/// where the parameter is the object instance which is kept using weak reference.
	/// For example, new DeferredOperation( WeakAction.Create( obj, parameteredAction ) ).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	internal class WeakAction<T>
	{
		private WeakReference _data;
		private Action<T> _action;

		private WeakAction( T data, Action<T> action )
		{
			_data = new WeakReference( data );
			_action = action;
		}

		private void OnAction( )
		{
			T data = (T)CoreUtilities.GetWeakReferenceTargetSafe( _data );

			if ( null != data )
				_action( data );
		}

		internal static Action CreateWeakAction( T data, Action<T> action )
		{
			return new WeakAction<T>( data, action ).OnAction;
		}

		internal static DeferredOperation ExecuteAsync( T data, Action<T> action )
		{
			var op = new DeferredOperation( CreateWeakAction( data, action ) );
			op.StartAsyncOperation( );

			return op;
		}
	}

	#endregion // WeakAction Class
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