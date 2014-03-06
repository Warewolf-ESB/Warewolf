using System;
using System.Net;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections;
using System.IO;
using System.Linq;






using Infragistics.Controls.Schedules;

namespace Infragistics

{
	#region PropertySerializationInfo Class

	/// <summary>
	/// For internal use only.
	/// </summary>
	// SSP 9/3/09 TFS18172
	// Added support for saving and loading summaries.
	// Made ObjectSerializationInfo and PropertySerializationInfo classes public.
	// 
	//internal class PropertySerializationInfo
	public class PropertySerializationInfo
	{
		private Type _type;
		private string _name;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of the property.</param>
		/// <param name="name">Name of the property.</param>
		public PropertySerializationInfo( Type type, string name )
		{
            CoreUtilities.ValidateNotNull( type );
            CoreUtilities.ValidateNotEmpty( name );

			_type = type;
			_name = name;
		}

		/// <summary>
		/// Type of the property.
		/// </summary>
		public Type Type
		{
			get
			{
				return _type;
			}
		}

		/// <summary>
		/// Name of the property.
		/// </summary>
		public string Name
		{
			get
			{
				return _name;
			}
		}

		/// <summary>
		/// Overridden. Returns the hash code.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode( )
		{
			return _type.GetHashCode( ) ^ _name.GetHashCode( );
		}

		/// <summary>
		/// Overridden. Checks to see if the specified object is equal to this instance.
		/// </summary>
		/// <returns></returns>
		public override bool Equals( object obj )
		{
			PropertySerializationInfo i = obj as PropertySerializationInfo;
			return null != i && _type == i._type && _name == i._name;
		}
	}

	#endregion // PropertySerializationInfo Class

	#region ObjectSerializationInfo Class

	/// <summary>
	/// For internal use only.
	/// </summary>
	// SSP 9/3/09 TFS18172
	// Added support for saving and loading summaries.
	// Made ObjectSerializationInfo and PropertySerializationInfo classes public.
	// 
	//internal abstract class ObjectSerializationInfo
	public abstract class ObjectSerializationInfo
    {
        #region Member Vars

        /// <summary>
        /// Used for designating collection elements in the dictionary returned
        /// by Serialize and in the dictionary passed into the Deserialize.
        /// </summary>
        public const string COLLECTION_ELEMENT_DESIGNATOR_KEY = "[]";

		private IEnumerable<PropertySerializationInfo> _props;
        private Dictionary<string, PropertySerializationInfo> _propsTable;

        #endregion // Member Vars

        #region Properties

        #region SerializedProperties

        /// <summary>
        /// Returns a list of properties that are to be serialized.
        /// </summary>
        public IEnumerable<PropertySerializationInfo> SerializedProperties 
		{
			get
			{
				if ( null == _props )
					_props = this.GetSerializedProperties( );

				return _props;
			}
		}

        #endregion // SerializedProperties 

        #endregion // Properties

        #region Methods

        #region Deserialize

        /// <summary>
        /// Deserializes the specified information into an instance of the object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract object Deserialize( Dictionary<string, object> obj );

        #endregion // Deserialize

        #region GetPropertyInfo

        internal virtual PropertySerializationInfo GetPropertyInfo( string propertyName )
        {
            if ( null == _propsTable )
            {
                _propsTable = new Dictionary<string, PropertySerializationInfo>( );

                foreach ( PropertySerializationInfo ii in this.SerializedProperties )
                    _propsTable[ii.Name] = ii;
            }

            PropertySerializationInfo ret;
            if ( _propsTable.TryGetValue( propertyName, out ret ) )
                return ret;

            return null;
        }

        #endregion // GetPropertyInfo

		#region GetSerializedProperties

		/// <summary>
		/// Returns a list of properties that are to be serialized.
		/// </summary>
		protected abstract IEnumerable<PropertySerializationInfo> GetSerializedProperties( );

		#endregion // GetSerializedProperties

        #region Serialize

        /// <summary>
        /// Serializes the specified object. Returns a map of properties to their values.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> Serialize( object obj );

        #endregion // Serialize 

        #endregion // Methods
	}

	#endregion // ObjectSerializationInfo Class

	#region DependencyObjectSerializationInfo Class

	/// <summary>
	/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing dependency objects.
	/// </summary>
	internal class DependencyObjectSerializationInfo : ObjectSerializationInfo
	{
		#region Member Vars

		private DependencyProperty[] _dpProps;
		private Type _type;

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="dpProps"></param>
		internal DependencyObjectSerializationInfo( Type type, params DependencyProperty[] dpProps )
		{
			CoreUtilities.ValidateNotNull( type );
			CoreUtilities.ValidateNotNull( dpProps );

			if ( !typeof( DependencyObject ).IsAssignableFrom( type ) )
				throw new ArgumentException( );

			_type = type;
			_dpProps = dpProps;
		}

		#endregion // Constructor

		#region CreateFromDependencyProperties

		private static PropertySerializationInfo[] CreateFromDependencyProperties( DependencyProperty[] dpProps )
		{
			PropertySerializationInfo[] arr = new PropertySerializationInfo[dpProps.Length];

			for ( int i = 0; i < arr.Length; i++ )
			{
				DependencyProperty ii = dpProps[i];
				arr[i] = new PropertySerializationInfo( DependencyPropertyUtilities.GetType( ii ), DependencyPropertyUtilities.GetName( ii ) );
			}

			return arr;
		}

		#endregion // CreateFromDependencyProperties

		#region Base Overrides

		#region Deserialize

		public override object Deserialize( Dictionary<string, object> values )
		{
			DependencyObject dpObj = (DependencyObject)Activator.CreateInstance( _type );
			object v;

			foreach ( DependencyProperty dp in _dpProps )
			{
				if ( values.TryGetValue( DependencyPropertyUtilities.GetName( dp ), out v ) )
					dpObj.SetValue( dp, v );
			}

			return dpObj;
		}

		#endregion // Deserialize

		#region GetSerializedProperties

		protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
		{
			return CreateFromDependencyProperties( _dpProps );
		}

		#endregion // GetSerializedProperties

		#region Serialize

		public override Dictionary<string, object> Serialize( object obj )
		{
			DependencyObject dpObj = (DependencyObject)obj;

			Dictionary<string, object> values = new Dictionary<string, object>( );

			foreach ( DependencyProperty dp in _dpProps )
			{
				if ( DependencyPropertyUtilities.ShouldSerialize( dpObj, dp ) )
					values[DependencyPropertyUtilities.GetName( dp )] = dpObj.GetValue( dp );
			}

			return values;
		}

		#endregion // Serialize

		#endregion // Base Overrides
	}

	#endregion // DependencyObjectSerializationInfo Class

	#region ReflectionSerializationInfo Class

	/// <summary>
	/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing an object using reflection. 
	/// It only serializes public properties.
	/// </summary>
	internal class ReflectionSerializationInfo : ObjectSerializationInfo
	{
		private class ReflectionPropertySerializationInfo : PropertySerializationInfo
		{
			internal PropertyInfo _prop;

			internal ReflectionPropertySerializationInfo( PropertyInfo prop )
				: base( prop.PropertyType, prop.Name )
			{
				_prop = prop;
			}
		}

		private Type _type;
		private Func<object> _objectCreator;
		private bool _serializeDefaultValues;
		
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type">Type of objects that this serialization info will serialize.</param>
		/// <param name="objectCreator">Used to create the object when deserializing. If null then public parameterless constructor will be used.</param>
		/// <param name="serializeDefaultValues">Specifies whether to serialize null values.</param>
		public ReflectionSerializationInfo( Type type, Func<object> objectCreator = null, bool serializeDefaultValues = false )
		{
			CoreUtilities.ValidateNotNull( type );
			_type = type;
			_objectCreator = objectCreator;
			_serializeDefaultValues = serializeDefaultValues;
		}

		protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
		{
			PropertyInfo[] props = _type.GetProperties( );

			return from ii in props
				   where ii.CanRead && ii.CanWrite && ! CoreUtilities.HasItems( ii.GetIndexParameters( ) )
				   select (PropertySerializationInfo)new ReflectionPropertySerializationInfo( ii );
		}

		private bool ShouldSerialize( ReflectionPropertySerializationInfo property, object value )
		{
			if ( !_serializeDefaultValues )
			{
				if ( null == value )
					return false;
			}

			return true;
		}

		public override Dictionary<string, object> Serialize( object obj )
		{
			Dictionary<string, object> map = null;

			foreach ( ReflectionPropertySerializationInfo ii in this.SerializedProperties )
			{
				object value = ii._prop.GetValue( obj, null );
				if ( this.ShouldSerialize( ii, value ) )
					map = ScheduleUtilities.AddEntryHelper( map, ii.Name, value );
			}

			return map;
		}

		public override object Deserialize( Dictionary<string, object> values )
		{
			object obj = ItemFactory.CreateNew( _type );

			foreach ( ReflectionPropertySerializationInfo ii in this.SerializedProperties )
			{
				object v;
				if ( values.TryGetValue( ii.Name, out v ) )
					ii._prop.SetValue( obj, v, null );
			}

			return obj;
		}
	}

	#endregion // ReflectionSerializationInfo Class

	#region CollectionSerializationInfo Class

	/// <summary>
	/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing a collection type.
	/// </summary>
	internal class CollectionSerializationInfo<TCollection, TItem> : ObjectSerializationInfo
		where TCollection: IList<TItem>, new( )
	{
		#region GetSerializedProperties

		protected override IEnumerable<PropertySerializationInfo> GetSerializedProperties( )
		{
			return new PropertySerializationInfo[]
			{
				new PropertySerializationInfo( typeof( TItem ), COLLECTION_ELEMENT_DESIGNATOR_KEY ),
			};
		}

		#endregion // GetSerializedProperties

		public override Dictionary<string, object> Serialize( object obj )
		{
			return ScheduleUtilities.AddEntryHelper<string, object>( null, COLLECTION_ELEMENT_DESIGNATOR_KEY, (TCollection)obj );
		}

		public override object Deserialize( Dictionary<string, object> values )
		{
			TCollection coll = new TCollection( );

			object v;
			if ( values.TryGetValue( COLLECTION_ELEMENT_DESIGNATOR_KEY, out v ) )
			{
				foreach ( TItem ii in (IEnumerable)v )
					coll.Add( ii );
			}

			return coll;
		}
	}

	#endregion // CollectionSerializationInfo Class

	#region AttributeValueParser Class

	internal class AttributeValueParser
    {
        #region Data Structures

        #region TypeInfo Class

        private class TypeInfo
        {
            internal Type _type;
            internal TypeConverter _converter;
            internal string _name;

            internal TypeInfo( Type type, string name, TypeConverter converter )
            {
                _type = type;
                _name = name;
                _converter = converter;
            }
        } 

        #endregion // TypeInfo Class

        #region DelegateTypeConverter Class

        internal class DelegateTypeConverter<T> : TypeConverter
        {
            private Converter<T, string> _toString;
            private Converter<string, T> _fromString;

            public DelegateTypeConverter( Converter<T, string> toString, Converter<string, T> fromString )
            {
                _toString = toString;
                _fromString = fromString;
            }

            public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
            {
                return typeof( string ) == sourceType && null != _fromString;
            }

            public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
            {
                return typeof( string ) == destinationType && null != _toString;
            }

            public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
            {
                if ( value is string && null != _fromString )
                    return _fromString( (string)value );

                return base.ConvertFrom( context, culture, value );
            }

            public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
            {
                if ( typeof( string ) == destinationType && value is T && null != _toString )
                    return _toString( (T)value );

                return base.ConvertTo( context, culture, value, destinationType );
            }
        }

        #endregion // DelegateTypeConverter Class

        #endregion // Data Structures

        #region Constants

        private const char TYPE_NAME_SEPARATOR = ':';

        #endregion // Constants

        #region Member Vars

        private Regex _typeValuePairRegex;
        private CultureInfo _cultureInfo;
        private Dictionary<Type, TypeInfo> _knownTypesInfo;
        private Dictionary<string, TypeInfo> _knownTypesInfoByName;

        private bool _dataPresenterCompatibilityMode;


        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        internal AttributeValueParser( )
        {
            this.Initialize( );
        }


        // This overload is for maintainging backward compatibility for data presenter.
        // 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataPresenterCompatibilityMode">This parameter is for maintaining data presenter
        /// load-save behavior where it uses some wpf specific converters as well as has fallback
        /// logic for generically parsing some types using Utilities.ConvertDataValule.</param>
        internal AttributeValueParser( bool dataPresenterCompatibilityMode )
        {
            _dataPresenterCompatibilityMode = dataPresenterCompatibilityMode;

            this.Initialize( );
        }

        
        #endregion // Constructor
        
        #region Methods

        #region Private Methods

        #region ConvertFromStringHelper

        private bool ConvertFromStringHelper( string valueAsString, TypeInfo typeInfo, out object convertedValue )
        {
            if ( null != typeInfo )
            {
				// Consider string.Empty as null.
				// 
				if ( string.IsNullOrEmpty( valueAsString ) )
				{
					convertedValue = null;
					return true;
				}
                else if ( null != typeInfo._converter )
                {
                    try
                    {
                        convertedValue = typeInfo._converter.ConvertFrom( null, _cultureInfo, valueAsString );
                        return true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    convertedValue = CoreUtilities.ConvertDataValue( valueAsString, typeInfo._type, _cultureInfo, null );
                    if ( null != convertedValue )
                        return true;
                }
            }

            convertedValue = null;
            return false;
        }

        #endregion // ConvertFromStringHelper

        #region ConvertToStringHelper

        private bool ConvertToStringHelper( object value, out string convertedString )
        {
            convertedString = null;

            if ( null == value )
            {
                convertedString = string.Empty;
            }
            else
            {
                TypeInfo typeInfo = this.GetTypeInfo( value.GetType( ) );
                if ( null != typeInfo )
                {
                    if ( null != typeInfo._converter )
                        convertedString = (string)typeInfo._converter.ConvertTo( null, _cultureInfo, value, typeof( string ) );

                    else if ( ! _dataPresenterCompatibilityMode )
                        convertedString = (string)CoreUtilities.ConvertDataValue( value, typeof( string ), CultureInfo.InvariantCulture, null );

                }
            }

            return null != convertedString;
        }

        #endregion // ConvertToStringHelper

        #region CreateAndRegisterKnownTypeInfo

        private TypeInfo CreateAndRegisterKnownTypeInfo( Type type )
        {
            if ( type.IsEnum )
            {
                TypeConverter converter = new DelegateTypeConverter<object>( ii => ii.ToString( ), ii => Enum.Parse( type, ii, true ) );
                return this.RegisterTypeConverterHelper( type, converter );
            }

            return null;
        }

        #endregion // CreateAndRegisterKnownTypeInfo

        #region GetTypeInfo

        private TypeInfo GetTypeInfo( string name, Type propertyType )
        {
            TypeInfo info;
            if ( _knownTypesInfoByName.TryGetValue( name, out info ) )
                return info;

			propertyType = CoreUtilities.GetUnderlyingType( propertyType );

            return this.CreateAndRegisterKnownTypeInfo( propertyType );
        }

        private TypeInfo GetTypeInfo( Type type )
        {
            TypeInfo info;
            if ( _knownTypesInfo.TryGetValue( type, out info ) )
                return info;

			Type typeUnderlying = CoreUtilities.GetUnderlyingType( type );
			if ( typeUnderlying != type )
				return this.GetTypeInfo( typeUnderlying );

            return this.CreateAndRegisterKnownTypeInfo( type );
        }

        #endregion // GetTypeInfo

        #region Initialize

        private void Initialize( )
        {
            _knownTypesInfo = new Dictionary<Type, TypeInfo>( );
            _knownTypesInfoByName = new Dictionary<string, TypeInfo>( );

            _cultureInfo = CultureInfo.InvariantCulture;

            _typeValuePairRegex = new Regex( @"^ *([^ \:]+) *\: *(.*) *$", RegexOptions.IgnoreCase );

            this.RegisterDefaultConverters( );
        }

        #endregion // Initialize

        #region ParseAttrubuteValueHelper

        private bool ParseAttrubuteValueHelper( string attributeValue, Type propertyType, out object parsedValue )
        {
            if ( !this.ParseTypeValuePair( attributeValue, propertyType, out parsedValue ) )
            {
                TypeInfo typeInfo = this.GetTypeInfo( propertyType );
                if ( null == typeInfo || !this.ConvertFromStringHelper( attributeValue, typeInfo, out parsedValue ) )
                {
                    // If property type is object and there was no type information included with the attribute 
                    // value then assume it was a string value. For string values, we do not put any type 
                    // information in ToAttributeValueHelper method.
                    // 
                    if ( null == typeInfo && typeof( object ) == propertyType )
                        parsedValue = attributeValue;
                    else
                        return false;
                }
            }

			if ( null == parsedValue )
				return true;

            parsedValue = CoreUtilities.ConvertDataValue( parsedValue, propertyType, _cultureInfo, null );
            return null != parsedValue;
        }

        #endregion // ParseAttrubuteValueHelper

        #region ParseTypeValuePair

        private bool ParseTypeValuePair( string attributeValue, Type propertyType, out object parsedValue )
        {
            if ( attributeValue.IndexOf( TYPE_NAME_SEPARATOR ) > 0 )
            {
                Match match = _typeValuePairRegex.Match( attributeValue );
                if ( null != match && match.Success )
                {
                    string typeName = match.Groups[1].Value;
                    string valueAsString = match.Groups[2].Value;

                    TypeInfo typeInfo = this.GetTypeInfo( typeName, propertyType );
                    //Debug.Assert( null != typeInfo );
                    if ( null != typeInfo )
                    {
                        if ( this.ConvertFromStringHelper( valueAsString, typeInfo, out parsedValue ) )
                            return true;
                    }
                }
            }

            parsedValue = null;
            return false;
        }

        #endregion // ParseTypeValuePair

        #region RegisterDefaultConverters

        private void RegisterDefaultConverters( )
        {
            const int STEP = 2;
            const int TYPE = 0;
            const int CONVERTER = 1;
            object[] arr = null;


            if ( _dataPresenterCompatibilityMode )
            {
                arr = new object[] 
				{
					// type,			converter
					typeof( string ),	null,
					typeof( sbyte ),	null,
					typeof( byte ),		null,				
					typeof( short ),	null,
					typeof( ushort ),	null,
					typeof( int ),		null,
					typeof( uint ),		null,
					typeof( long  ),	null,
					typeof( ulong  ),	null,
					typeof( float ),	null,
					typeof( double ),	null,
					typeof( char ),		null,
					typeof( bool ),		null,
					typeof( decimal ),	null,
					typeof( DateTime ),	null,
					typeof( TimeSpan ),	null,
                    typeof( System.Windows.Media.Color ),	new System.Windows.Media.ColorConverter( ),
					typeof( Thickness ), new ThicknessConverter( ),
					typeof( Rect ),		new RectConverter( ),
					typeof( Size ),		new SizeConverter( ),
					typeof( Point ),	new PointConverter( )
				};
            }


			if ( null == arr )
            {
                arr = new object[] 
			    {
				    // type,			converter
				    typeof( string ),	new DelegateTypeConverter<string>( ii => ii, ii => ii ),
				    typeof( sbyte ),	new DelegateTypeConverter<sbyte>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToSByte( ii ) ),
				    typeof( byte ),		new DelegateTypeConverter<byte>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToByte( ii ) ),
				    typeof( short ),	new DelegateTypeConverter<short>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToInt16( ii ) ),
				    typeof( ushort ),	new DelegateTypeConverter<ushort>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToUInt16( ii ) ),
				    typeof( int ),		new DelegateTypeConverter<int>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToInt32( ii ) ),
				    typeof( uint ),		new DelegateTypeConverter<uint>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToUInt32( ii ) ),
				    typeof( long  ),	new DelegateTypeConverter<long>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToInt64( ii ) ),
				    typeof( ulong  ),	new DelegateTypeConverter<ulong>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToUInt64( ii ) ),
				    typeof( float ),	new DelegateTypeConverter<float>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToSingle( ii ) ),
				    typeof( double ),	new DelegateTypeConverter<double>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToDouble( ii ) ),
				    typeof( char ),		new DelegateTypeConverter<char>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToChar( ii ) ),
				    typeof( bool ),		new DelegateTypeConverter<bool>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToBoolean( ii ) ),
				    typeof( decimal ),	new DelegateTypeConverter<decimal>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToDecimal( ii ) ),
				    typeof( DateTime ),	new DelegateTypeConverter<DateTime>( ii => XmlConvert.ToString( ii, XmlDateTimeSerializationMode.RoundtripKind ), ii => XmlConvert.ToDateTime( ii, XmlDateTimeSerializationMode.RoundtripKind ) ),
                    typeof( System.Windows.Media.Color ), new DelegateTypeConverter<System.Windows.Media.Color>( ConvertColorToString, ParseColor ),
					typeof( TimeSpan ),	new DelegateTypeConverter<TimeSpan>( ii => XmlConvert.ToString( ii ), ii => XmlConvert.ToTimeSpan( ii ) ),
			    };
            }

            for ( int i = 0; i < arr.Length; i += STEP )
            {
                Type type = (Type)arr[TYPE + i];
                TypeConverter converter = (TypeConverter)arr[CONVERTER + i];


                if ( null == converter && _dataPresenterCompatibilityMode )
                    converter = TypeDescriptor.GetConverter( type );


                this.RegisterTypeConverter( type, converter );
            }
        }

        private static string ConvertColorToString( System.Windows.Media.Color color )
        {
            
            return color.ToString( );
        }

        private static System.Windows.Media.Color ParseColor( string str )
        {
            

			if ( '#' == str[0] )
				str = str.Remove( 0, 1 );

            uint v = uint.Parse( str, NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier );
            return System.Windows.Media.Color.FromArgb( (byte)( v >> 24) , (byte)( ( v >> 16 ) & 0xff ), (byte)( ( v >> 8 ) & 0xff ), (byte)( v & 0xff ) );
        }

        #endregion // RegisterDefaultConverters

        #region RegisterTypeConverterHelper

        private TypeInfo RegisterTypeConverterHelper( Type type, TypeConverter converter )
        {
            CoreUtilities.ValidateNotNull( type );
            CoreUtilities.ValidateNotNull( converter );

            string typeName = type.Name;
            TypeInfo info = new TypeInfo( type, typeName, converter );
            _knownTypesInfo[type] = info;
            _knownTypesInfoByName[typeName] = info;

            return info;
        }

        #endregion // RegisterTypeConverterHelper

        #region ToAttributeValueHelper

        private bool ToAttributeValueHelper( object value, Type propertyType, out string attributeValue )
        {
            if ( this.ConvertToStringHelper( value, out attributeValue ) )
            {
				Type valueTypeUnderlying = null != value ? CoreUtilities.GetUnderlyingType( value.GetType( ) ) : null;
				Type propertyTypeUnderlying = CoreUtilities.GetUnderlyingType( propertyType );
                bool includeTypeInfo = null != valueTypeUnderlying && valueTypeUnderlying != propertyTypeUnderlying && !( value is string );
                if ( includeTypeInfo )
                {
                    TypeInfo typeInfo = this.GetTypeInfo( value.GetType( ) );
                    if ( null != typeInfo )
                        attributeValue = typeInfo._name + ":" + attributeValue;
                }

                return true;
            }

            return false;
        }

        #endregion // ToAttributeValueHelper

        #endregion // Private Methods

        #region Public Methods

        #region ParseAttrubuteValue

        public bool ParseAttrubuteValue( string attributeValue, Type propertyType, out object parsedValue )
        {
            return this.ParseAttrubuteValueHelper( attributeValue, propertyType, out parsedValue );
        }

        #endregion // ParseAttrubuteValue

        #region RegisterTypeConverter

        public void RegisterTypeConverter( Type type, TypeConverter converter )
        {
            this.RegisterTypeConverterHelper( type, converter );
        } 

        #endregion // RegisterTypeConverter

        #region ToAttributeValue

        public bool ToAttributeValue( object value, Type propertyType, out string attributeValue )
        {
            return this.ToAttributeValueHelper( value, propertyType, out attributeValue );
        }

        #endregion // ToAttributeValue

        #endregion // Public Methods

        #endregion // Methods
    }

    #endregion // AttributeValueParser Class

    #region ObjectSerializer Class

    /// <summary>
    /// For internal use only.
    /// </summary>
    internal class ObjectSerializer
    {
        #region Nested Data Structures

        #region ErrorType Enum

        protected enum ErrorType
        {
            ValueParseError,
            UnrecognizedXmlAttribute,
            UnrecognizedXmlElement,
            UnrecognizedXmlContent,
            ObjectNotSerializable,
            InvalidPropertyValue
        }

        #endregion // ErrorType Enum

        #endregion // Nested Data Structures

        #region Member Vars

        private Dictionary<Type, ObjectSerializationInfo> _registeredInfos;
        private AttributeValueParser _attributeParser;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectSerializer( AttributeValueParser attributeParser )
        {
            CoreUtilities.ValidateNotNull( attributeParser );

            _attributeParser = attributeParser;
            _registeredInfos = new Dictionary<Type, ObjectSerializationInfo>( );
        }

        #endregion // Constructor

        #region Methods

        #region Public Methods

        private struct Parser
        {
            private ObjectSerializer _serializer;
            private AttributeValueParser _attributeParser;
            internal XmlWriter _writer;
            internal XmlReader _reader;
			private XmlNodeType _nodeType;
			private bool _readAttributes;
            internal int _elementDepth;
			private bool _isEmptyElement;

            internal Parser( ObjectSerializer serializer, XmlReader reader, XmlWriter writer )
            {
                _serializer = serializer;
                _attributeParser = serializer._attributeParser;
                _writer = writer;
                _reader = reader;
				_nodeType = XmlNodeType.None;
				_readAttributes = false;
                _elementDepth = 0;
				_isEmptyElement = false;
            }

            #region AppendValueHelper

            internal bool AppendValueAsAttribute( object value, PropertySerializationInfo propInfo )
            {
                string val;
                if ( _attributeParser.ToAttributeValue( value, propInfo.Type, out val ) )
                {
                    _writer.WriteAttributeString( propInfo.Name, val );
                    return true;
                }

                return false;
            }

            #endregion // AppendValueHelper

            #region OnError

            internal bool OnError( ErrorType errorType, string message )
            {
                return false;
            }

            #endregion // OnError

            #region ParseObject

            internal bool ParseObject( ObjectSerializationInfo serializationInfo, out object parsedValue )
            {
                parsedValue = null;
                Dictionary<string, object> valuesTable = new Dictionary<string, object>( );
                int startElementDepth = _elementDepth;
                string lastElementName = string.Empty;

                while ( _elementDepth >= startElementDepth && this.Read( ) )
                {
					XmlNodeType nodeType = _nodeType;
                    switch ( nodeType )
                    {
                        case XmlNodeType.Element:
                            {
                                _elementDepth++;
                                string elemName = _reader.Name;
                                lastElementName = elemName;

                                // The xml element represents the property name. Get the serialization property information for that
                                // property.
                                // 
                                PropertySerializationInfo propInfo = serializationInfo.GetPropertyInfo( elemName );

                                // If no such property exists in the serialization information of the object being deserialized then 
                                // the object is a collection and the xml element and the sibling elements represent the collection 
                                // items.
                                // 
                                bool isCollection = false;
                                if ( null == propInfo )
                                {
                                    propInfo = serializationInfo.GetPropertyInfo( ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY );
                                    isCollection = null != propInfo;
                                }

                                if ( null != propInfo )
                                {
                                    object tmp;

                                    // If reading collection items, since above we've read the first item's xml element tag, 
                                    // pass processCurrentXmlNode parameter as true to indicate that it should process the
                                    // already read element tag.
                                    // 
                                    if ( !this.ParsePropertyValue( propInfo, out tmp, isCollection ) )
                                        return false;

                                    valuesTable[propInfo.Name] = tmp;
                                }
                                else
                                {
                                    bool tmp = this.OnError( ErrorType.UnrecognizedXmlElement, string.Format( "Unknown xml element {0}.", elemName ) );
                                    _reader.Skip( );
                                    if ( !tmp )
                                        return false;
                                }                                
                            }
                            break;
                        case XmlNodeType.EndElement:
                            _elementDepth--;
							break;
                        case XmlNodeType.Attribute:
                            {
                                string attributeName = _reader.Name;
                                PropertySerializationInfo propInfo = serializationInfo.GetPropertyInfo( attributeName );
                                if ( null != propInfo )
                                {
                                    object tmp;
                                    if ( _attributeParser.ParseAttrubuteValue( _reader.Value, propInfo.Type, out tmp ) )
                                        valuesTable[propInfo.Name] = tmp;
                                    else
                                    {
                                        if ( !this.OnError( ErrorType.ValueParseError, string.Format( "Unable to parse attribute {0}'s value {1}.", attributeName, _reader.Value ) ) )
                                            return false;
                                    }
                                }
                                else
                                {
                                    if ( !this.OnError( ErrorType.UnrecognizedXmlAttribute, string.Format( "Unknown xml attribute {0} on element {1}.", attributeName, lastElementName ) ) )
                                        return false;
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Entity:
                        case XmlNodeType.EndEntity:
                        case XmlNodeType.EntityReference:
                        case XmlNodeType.DocumentFragment:
                            {
                                if ( !this.OnError( ErrorType.UnrecognizedXmlContent, string.Format( "This text should not be here as part of the element {0}: {1}.", lastElementName, _reader.Value ) ) )
                                    return false;
                            }
                            break;
                        case XmlNodeType.Document:
                        case XmlNodeType.DocumentType:
                        case XmlNodeType.XmlDeclaration:
                        case XmlNodeType.Comment:
                        case XmlNodeType.None:
                        case XmlNodeType.Notation:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.Whitespace:
                            break;
                    }
                }

				parsedValue = serializationInfo.Deserialize( valuesTable );
                return true;
            }

            #endregion // ParseObject

            #region ParsePropertyValue

            internal bool ParsePropertyValue( PropertySerializationInfo propInfo, out object parsedValue, bool processCurrentXmlNode )
            {
                parsedValue = null;
                bool? retVal = null;
                bool isCollection = ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY == propInfo.Name;
				List<object> collectionItems = null;

                // processCurrentXmlNode parameter is passed in as true if the caller has already read an xml node that it wants
				// this method to process. Decrement the _elementDepth since we'll increment it below without making a call to
				// Read.
                // 
				if ( processCurrentXmlNode )
					_elementDepth--;

				int startElementDepth = _elementDepth;
                while ( _elementDepth >= startElementDepth && ( processCurrentXmlNode ? !( processCurrentXmlNode = false ) : this.Read( ) ) )
                {
					XmlNodeType nodeType = _nodeType;
                    switch ( nodeType )
                    {
                        case XmlNodeType.Element:
                            {
								_elementDepth++;

                                string elemName = _reader.Name;
                                ObjectSerializationInfo serializationInfo = _serializer.GetRegisteredInfo( propInfo.Type, elemName );
                                if ( null == serializationInfo )
                                {
                                    bool tmp = this.OnError( ErrorType.UnrecognizedXmlElement, string.Format( "Unknown xml element {0}.", elemName ) );
                                    _reader.Skip( );
                                    if ( !tmp )
                                        return false;

                                    break;
                                }

								object tmpParsedVal;
                                retVal = this.ParseObject( serializationInfo, out tmpParsedVal );

								if ( retVal.Value )
								{
									// If we are reading collection elements, then add them to a list.
									// 
									if ( isCollection )
									{
										if ( null == collectionItems )
											parsedValue = collectionItems = new List<object>( );

										collectionItems.Add( tmpParsedVal );
									}
									else
									{
										parsedValue = tmpParsedVal;
									}
								}
                            }
                            break;
                        case XmlNodeType.EndElement:
                            {
                                _elementDepth--;

                                // Return if the XML element's end tag is read - except when reading collection items in
                                // which case we have to read multiple items and thus multiple elements and in that case
                                // return when we have finished reading all the items which would be indicated by the
                                // fact that we've come accross the end tag of the parent element. Note that in such
                                // a case, the parent element would represent the collection type name and all the 
                                // child elements of it would represent the collection items.
                                // 
								if ( !isCollection || _elementDepth < startElementDepth )
									return retVal ?? false;
                            }
                            break;
                        case XmlNodeType.Attribute:
                        case XmlNodeType.Text:
                        case XmlNodeType.SignificantWhitespace:
                        case XmlNodeType.CDATA:
                        case XmlNodeType.Entity:
                        case XmlNodeType.EndEntity:
                        case XmlNodeType.EntityReference:
                        case XmlNodeType.DocumentFragment:
                            return this.OnError( ErrorType.UnrecognizedXmlContent, string.Format( "Unrecognized xml content {0} {1}.", _reader.Name, _reader.Value ) );
                        case XmlNodeType.Document:
                        case XmlNodeType.DocumentType:
                        case XmlNodeType.Whitespace:
                        case XmlNodeType.Comment:
                        case XmlNodeType.None:
                        case XmlNodeType.Notation:
                        case XmlNodeType.ProcessingInstruction:
                        case XmlNodeType.XmlDeclaration:
                            break;
                    }
                }

                return true;
            }

            #endregion // ParsePropertyValue

			#region Read

			private bool Read( )
			{
				if ( _readAttributes )
				{
					if ( _reader.MoveToNextAttribute( ) )
					{
						_nodeType = XmlNodeType.Attribute;
						return true;
					}

					_readAttributes = false;
				}

				if ( _isEmptyElement )
				{
					_isEmptyElement = false;
					_nodeType = XmlNodeType.EndElement;
					return true;
				}

				if ( _reader.Read( ) )
				{
					_nodeType = _reader.NodeType;

					bool isElement = XmlNodeType.Element == _nodeType;
					_readAttributes = isElement;
					_isEmptyElement = isElement && _reader.IsEmptyElement;
					return true;
				}

				return false;
			}

			#endregion // Read

            #region SaveHelper

            internal bool SaveHelper( object value, PropertySerializationInfo propInfo )
            {
                // Get the value's type and registered ObjectSerializationInfo object for that type.
                // ObjectSerializationInfo contains the list of properties to serialize out.
                // 
                Type type = value.GetType( );
                ObjectSerializationInfo info = _serializer.GetRegisteredInfo( type );

                // Get the list of properties to serialize.
                IEnumerable<PropertySerializationInfo> serializedProperties = null != info ? info.SerializedProperties : null;

                // If the object is not serializable or no serialization information was returned then consider it
                // an error.
                // 
                if ( null == serializedProperties )
                    return this.OnError( ErrorType.ObjectNotSerializable, string.Format( "Error saving object {0}.", type.Name ) );

				// Also get the list of property values to serialize. The reason for having separate SerializedProperties
				// is that when parsing back, we need to know the types of properties.
				// 
				Dictionary<string, object> table = null != info ? info.Serialize( value ) : null;

                // If the object being saved is a value of a property of a parent object, then we need to write
                // out XML element that identifies the property. When parsing back, we'll use that to know which 
                // property the XML element corresponds to.
                // 
                string propElemName = null != propInfo ? propInfo.Name : null;
                bool writePropElem = !string.IsNullOrEmpty( propElemName ) && ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY != propElemName;
                if ( writePropElem )
                    _writer.WriteStartElement( propElemName );

                // Write out the type name of the object being saved.
                // 
                _writer.WriteStartElement( type.Name );

                bool retVal = true;

                // Loop through the list of properties of the object being serialized.
                // 
                foreach ( PropertySerializationInfo ii in serializedProperties )
                {
                    string iiName = ii.Name;
                    object iiVal;

                    // Get the value of the property if any.
                    // 
                    if ( null != table && table.TryGetValue( iiName, out iiVal )
                        // If the object being serialized is a collection then it's items are always saved
                        // as XML elements further below in the next for loop.
                        // 
                        && ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY != iiName )
                    {
                        // Try to see if the property's value can be saved as an attribute. For simple
                        // values like Int32, string, double etc..., we'll save them as attributes.
                        // Complex objects that require XML elements are saved below.
                        // 
                        if ( this.AppendValueAsAttribute( iiVal, ii ) )
                            // If the value was saved as attribute, remove it from the table so 
                            // we don't re-save it as XML element below.
                            // 
                            table.Remove( iiName );
                    }
                }

                // Now save properties that could not be saved as attributes above as XML elements.
                // 
                foreach ( PropertySerializationInfo ii in serializedProperties )
                {
                    string iiName = ii.Name;
                    object iiVal;

                    // Get the value of the property if any.
                    // 
                    if ( null != table && table.TryGetValue( iiName, out iiVal ) )
                    {
                        // If the object being serialized out is a collection, it will have a single entry
                        // in the table with the key of COLLECTION_ELEMENT_DESIGNATOR_KEY that returns an
                        // IEnumerable of items. Each item is saved out as a XML node.
                        // 
                        if ( ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY == iiName )
                        {
                            IEnumerable children = value as IEnumerable;
                            Debug.Assert( null != children, "[] key is meant for collection elements!" );

                            if ( null != children )
                            {
                                foreach ( object item in children )
                                {
                                    if ( !this.SaveHelper( item, ii ) )
                                        return false;
                                }
                            }
                            // If the property's value in the table is null or is not an IEnumerable then
                            // consider it an error since COLLECTION_ELEMENT_DESIGNATOR_KEY is only meant 
                            // for collections.
                            // 
                            else if ( !this.OnError( ErrorType.InvalidPropertyValue, string.Format( "Invalid collection value of {0}.", null == value ? "null" : value.GetType( ).Name ) ) )
                            {
                                retVal = false;
                                break;
                            }
                        }
                        else
                        {
                            if ( !this.SaveHelper( iiVal, ii ) )
                            {
                                if ( !this.OnError( ErrorType.ObjectNotSerializable, string.Format( "Error saving {0}.{1} {2}.", type.Name, iiName, iiVal.GetType( ).Name ) ) )
                                {
                                    retVal = false;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Write the end tag for the object type element.
                // 
                _writer.WriteEndElement( );

                // Write the end tag for the property whose value is being saved out.
                // 
                if ( writePropElem )
                    _writer.WriteEndElement( );

                return retVal;
            }

            #endregion // SaveHelper
        }

        #region Parse

        #region ParseXmlFragment

        public bool ParseXmlFragment( string xmlFragment, Type type, out object parsedValue )
        {
            StringReader stringReader = new StringReader( xmlFragment );

            XmlReader reader = XmlReader.Create( stringReader );
            bool success = this.Parse( reader, type, out parsedValue );

            reader.Close( );
            stringReader.Close( );
            stringReader.Dispose( );

            return success;
        } 

        #endregion // ParseXmlFragment

        #region Parse

        public bool Parse( XmlReader reader, Type type, out object parsedValue )
        {
            CoreUtilities.ValidateNotNull( reader );
            CoreUtilities.ValidateNotNull( type );

            Parser parser = new Parser( this, reader, null );

            if ( parser.ParsePropertyValue( new PropertySerializationInfo( type, type.Name ), out parsedValue, false ) )
            {
                Debug.Assert( 0 == parser._elementDepth );
                return true;
            }

            return false;
        } 

        #endregion // Parse

        #region SaveAsXmlFragment

        public bool SaveAsXmlFragment( object value, out string xmlFragment )
        {
			StringBuilder sb = new StringBuilder();

			XmlWriter xmlWriter = XmlWriter.Create(sb);

			bool success = this.Save(xmlWriter, value);

			xmlWriter.Close();

			xmlFragment = success ? sb.ToString() : null;

			return success;
        } 

        #endregion // SaveAsXmlFragment

        #region Save

        public bool Save( XmlWriter writer, object value )
        {
            CoreUtilities.ValidateNotNull( writer );
            CoreUtilities.ValidateNotNull( value );

            Parser parser = new Parser( this, null, writer );

            if ( parser.SaveHelper( value, null ) )
            {
                Debug.Assert( 0 == parser._elementDepth );
                return true;
            }

            return false;
        } 

        #endregion // Save

        #endregion // Parse

        #region RegisterInfo

        /// <summary>
        /// Registers an instance of ObjectSerializationInfo for a specific type where ObjectSerializationInfo
        /// can serialize and deserialize instances of that type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="info"></param>
        public void RegisterInfo( Type type, ObjectSerializationInfo info )
        {
            CoreUtilities.ValidateNotNull( info, "info" );
            _registeredInfos[type] = info;
        }

		/// <summary>
		/// Registers an instance of ReflectionSerializationInfo for the specified type.
        /// </summary>
        /// <param name="type"></param>
		public void RegisterReflectionSerializationInfo( Type type )
		{
			this.RegisterInfo( type, new ReflectionSerializationInfo( type ) );
		}

        #endregion // RegisterInfo
        
        #endregion // Public Methods

        #region Private/Internal Methods

        #region GetRegisteredInfo

        /// <summary>
        /// Gets the object serialization information that was registed with <see cref="RegisterInfo"/> method.
        /// </summary>
        /// <param name="type">Type for which to get the registered object serialization information</param>
        /// <returns><see cref="ObjectSerializationInfo"/> instance.</returns>
        public ObjectSerializationInfo GetRegisteredInfo( Type type )
        {
            ObjectSerializationInfo r;
            if ( _registeredInfos.TryGetValue( type, out r ) )
                return r;

            return null;
        }

        /// <summary>
        /// Gets the object serialization info for the type with the name of the 'typeName' and that is a base of the
        /// specified 'baseType'.
        /// </summary>
        /// <param name="baseType">Base type to match.</param>
        /// <param name="typeName">Name of the type to match.</param>
        /// <returns>ObjectSerializationInfo instance.</returns>
        public ObjectSerializationInfo GetRegisteredInfo( Type baseType, string typeName )
        {
            if ( null != baseType && baseType.Name == typeName )
                return this.GetRegisteredInfo( baseType );

            foreach ( KeyValuePair<Type, ObjectSerializationInfo> ii in _registeredInfos )
            {
                if ( ( null == baseType || baseType.IsAssignableFrom( ii.Key ) ) && ii.Key.Name == typeName )
                    return ii.Value;
            }

            return null;
        }

        #endregion // GetRegisteredInfo

        #endregion // Private/Internal Methods

        #endregion // Methods
    }

    #endregion // ObjectSerializer Class
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