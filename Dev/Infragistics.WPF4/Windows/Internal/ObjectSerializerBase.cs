using System;
using System.ComponentModel;
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Data;
using Infragistics.Shared;
using System.Runtime.CompilerServices;
using System.Reflection;
using System.Text.RegularExpressions;
using Infragistics.Windows.Controls;
using System.Xml.Serialization;
using System.IO;
using System.Text;

namespace Infragistics.Windows.Internal
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

		/// <summary>
		/// Creates PropertySerializationInfo using the specified DependencyProperty.
		/// </summary>
		/// <param name="dpProperty"></param>
		public static PropertySerializationInfo Create( DependencyProperty dpProperty )
		{
			return new PropertySerializationInfo( dpProperty.PropertyType, dpProperty.Name );
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
		/// <summary>
		/// Used for designating collection elements in the dictionary returned
		/// by Serialize and in the dictionary passed into the Deserialize.
		/// </summary>
		public const string COLLECTION_ELEMENT_DESIGNATOR_KEY = "[]";

		/// <summary>
		/// Returns a list of properties that are to be serialized.
		/// </summary>
		public abstract IEnumerable<PropertySerializationInfo> SerializedProperties { get; }

		/// <summary>
		/// Serializes the specified object. Returns a map of properties to their values.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public abstract Dictionary<string, object> Serialize( object obj );

		/// <summary>
		/// Deserializes the specified information into an instance of the object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public abstract object Deserialize( Dictionary<string, object> obj );
	}

	#endregion // ObjectSerializationInfo Class

	#region XmlSerializationInfo Class

	// SSP 5/17/11 TFS34944
	// 
	/// <summary>
	/// Provides necessary information to the ObjectSerializerBase for serializing and deserializing an object using XmlSerializer. 
	/// </summary>
	internal class XmlSerializationInfo : ObjectSerializationInfo
	{
		private List<PropertySerializationInfo> _props;
		internal const string ELEMENT_NAME = "XmlSerializerData";

		private Dictionary<Type, XmlSerializer> _serializerCache;

		/// <summary>
		/// Constructor.
		/// </summary>
		public XmlSerializationInfo( )
		{
		}

		public override IEnumerable<PropertySerializationInfo> SerializedProperties
		{
			get
			{
				if ( null == _props )
					_props = this.GetSerializedProperties( );

				return _props;
			}
		}

		private List<PropertySerializationInfo> GetSerializedProperties( )
		{
			return new List<PropertySerializationInfo>( )
			{
				new PropertySerializationInfo( typeof( string ), "Type" ),
				new PropertySerializationInfo( typeof( string ), "Data" )
			};
		}

		private XmlSerializer GetSerializer( Type type )
		{
			if ( null == _serializerCache )
				_serializerCache = new Dictionary<Type, XmlSerializer>( );

			XmlSerializer serializer;
			if ( !_serializerCache.TryGetValue( type, out serializer ) )
			{
				try
				{
					serializer = new XmlSerializer( type );
				}
				catch
				{
					serializer = null;
				}

				_serializerCache[type] = serializer;
			}

			return serializer;
		}

		private static Type GetTypeHelper( string name )
		{
			try
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies( );

				for ( int i = 0; i < assemblies.Length; i++ )
				{
					Assembly ii = assemblies[i];

					Type type = ii.GetType( name );
					if ( null != type )
						return type;
				}
			}
			catch
			{
				Debug.Assert( false );
			}

			return null;
		}

		public override Dictionary<string, object> Serialize( object obj )
		{
			if ( null == obj )
				return null;

			Type type = obj.GetType( );
			XmlSerializer serializer = this.GetSerializer( type );

			if ( null == type )
				return null;

			MemoryStream stream = new MemoryStream( );

			serializer.Serialize( stream, obj );

			stream.Position = 0;
			byte[] data = stream.GetBuffer( );

			Dictionary<string, object> map = new Dictionary<string, object>( );

			map["Type"] = type.FullName;
			map["Data"] = Convert.ToBase64String( data );

			stream.Close( );
			stream.Dispose( );

			return map;
		}

		public override object Deserialize( Dictionary<string, object> values )
		{
			object v;

			Type type = null;
			if ( values.TryGetValue( "Type", out v ) )
				type = GetTypeHelper( (string)v );

			XmlSerializer serializer = null != type ? this.GetSerializer( type ) : null;

			if ( null != serializer && values.TryGetValue( "Data", out v ) )
			{
				byte[] data = Convert.FromBase64String( (string)v );

				MemoryStream stream = new MemoryStream( data );

				object r = serializer.Deserialize( stream );

				stream.Close( );
				stream.Dispose( );

				return r;
			}

			return null;
		}

		public bool IsTypeSupported( Type type )
		{
			return null != this.GetSerializer( type );
		}
	}

	#endregion // XmlSerializationInfo Class

	#region ObjectSerializerBase Class

	/// <summary>
	/// For internal use only.
	/// </summary>
	public class ObjectSerializerBase
	{
		#region Nested Data Structures

		#region AttributeValueParser Class

		private class AttributeValueParser
		{
			#region Data Structures

			private class TypeInfo
			{
				internal Type _type;
				internal TypeConverter _converter;
				internal string _name;

				/// <summary>
				/// Constructor
				/// </summary>
				/// <param name="type"></param>
				internal TypeInfo( Type type )
				{
					_type = type;
					_name = type.Name;
					_converter = TypeDescriptor.GetConverter( type );
				}

				internal TypeInfo( Type type, string name, TypeConverter converter )
				{
					_type = type;
					_name = name;
					_converter = converter;
				}
			}

			#endregion // Data Structures

			#region Constants

			private const char TYPE_NAME_SEPARATOR = ':';

			#endregion // Constants

			#region Member Vars

			private TypeInfo[] _knownTypesInfo;
			private Regex _typeValuePairRegex;
			private CultureInfo _cultureInfo;

			private static AttributeValueParser g_instance = new AttributeValueParser( );

			#endregion // Member Vars

			#region Constructor

			/// <summary>
			/// Constructor
			/// </summary>
			internal AttributeValueParser( )
			{
				_cultureInfo = CultureInfo.InvariantCulture;

				const int STEP = 3;
				const int TYPE = 0;
				const int CONVERTER = 2;

				object[] arr = new object[] 
				{
					// type,			name,		converter
					typeof( string ),	null,		null,
					typeof( sbyte ),	null,		null,
					typeof( byte ),		null,		null,				
					typeof( short ),	null,		null,
					typeof( ushort ),	null,		null,
					typeof( int ),		null,		null,
					typeof( uint ),		null,		null,
					typeof( long  ),	null,		null,
					typeof( ulong  ),	null,		null,
					typeof( float ),	null,		null,
					typeof( double ),	null,		null,
					typeof( char ),		null,		null,
					typeof( bool ),		null,		null,
					typeof( decimal ),	null,		null,
					typeof( DateTime ),	null,		null,
					typeof( string ),	null,		null,
					typeof( System.Windows.Media.Color ),	null,		new System.Windows.Media.ColorConverter( ),
					typeof( Thickness ), null,		new ThicknessConverter( ),
					typeof( Rect ),		null,		new RectConverter( ),
					typeof( Size ),		null,		new SizeConverter( ),
					typeof( Point ),	null,		new PointConverter( )
				};

				List<TypeInfo> list = new List<TypeInfo>( );

				for ( int i = 0; i < arr.Length; i += STEP )
				{
					TypeInfo ii = new TypeInfo( (Type)arr[TYPE + i] );
					if ( null != arr[CONVERTER + i] )
						ii._converter = (TypeConverter)arr[CONVERTER + i];

					list.Add( ii );
				}

				_knownTypesInfo = list.ToArray( );
				_typeValuePairRegex = new Regex( @"^ *([^ \:]+) *\: *(.*) *$", RegexOptions.IgnoreCase );
			}

			#endregion // Constructor

			#region Methods

			#region Private Methods

			#region ConvertFromStringHelper

			private bool ConvertFromStringHelper( string valueAsString, TypeInfo typeInfo, out object convertedValue )
			{
				if ( null != typeInfo )
				{
					if ( null != typeInfo._converter )
					{
						try
						{
							convertedValue = typeInfo._converter.ConvertFromString( null, _cultureInfo, valueAsString );
							return true;
						}
						catch
						{
						}
					}
					else
					{
						convertedValue = Utilities.ConvertDataValue( valueAsString, typeInfo._type, _cultureInfo, null );
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
							convertedString = typeInfo._converter.ConvertToString( null, _cultureInfo, value );
						else
							convertedString = (string)Utilities.ConvertDataValue( value, typeof( string ), CultureInfo.InvariantCulture, null );
					}
				}

				return null != convertedString;
			}

			#endregion // ConvertToStringHelper

			#region GetTypeInfo

			private TypeInfo GetTypeInfo( string name, Type propertyType )
			{
				TypeInfo[] info = _knownTypesInfo;
				for ( int i = 0; i < info.Length; i++ )
				{
					TypeInfo ii = info[i];
					if ( 0 == string.Compare( ii._name, name, true, _cultureInfo ) )
						return ii;
				}

				// SSP 10/20/10 TFS34749
				// If the type is nullable then get the type information for the underlying type.
				// 
				if ( null != propertyType )
					propertyType = Utilities.GetUnderlyingType( propertyType );

				if ( null != propertyType && propertyType.IsEnum )
					return new TypeInfo( propertyType );

				return null;
			}

			private TypeInfo GetTypeInfo( Type type )
			{
				TypeInfo[] info = _knownTypesInfo;
				for ( int i = 0; i < info.Length; i++ )
				{
					TypeInfo ii = info[i];
					if ( type == ii._type )
						return ii;
				}

				// SSP 10/20/10 TFS34749
				// If the type is nullable then get the type information for the underlying type.
				// 
				if ( null != type )
				{
					Type typeUnderlying = Utilities.GetUnderlyingType( type );
					if ( typeUnderlying != type )
						return this.GetTypeInfo( typeUnderlying );
				}

				if ( null != type && type.IsEnum )
					return new TypeInfo( type );

				return null;
			}

			#endregion // GetTypeInfo

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

				parsedValue = Utilities.ConvertDataValue( parsedValue, propertyType, _cultureInfo, null );
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
						Debug.Assert( null != typeInfo );
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

			#region ToAttributeValueHelper

			private bool ToAttributeValueHelper( object value, Type propertyType, out string attributeValue )
			{
				if ( this.ConvertToStringHelper( value, out attributeValue ) )
				{
					// SSP 10/20/10 TFS34749
					// Take into account nullable types.
					// 
					// ------------------------------------------------------------------------------------------------
					//bool includeTypeInfo = null != value && value.GetType( ) != propertyType && !( value is string );
					Type valueTypeUnderlying = null != value ? Utilities.GetUnderlyingType( value.GetType( ) ) : null;
					Type propertyTypeUnderlying = Utilities.GetUnderlyingType( propertyType );
					bool includeTypeInfo = null != valueTypeUnderlying 
						&& valueTypeUnderlying != propertyTypeUnderlying && !( value is string );
					// ------------------------------------------------------------------------------------------------
					
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

			public static bool ParseAttrubuteValue( string attributeValue, Type propertyType, out object parsedValue )
			{
				return g_instance.ParseAttrubuteValueHelper( attributeValue, propertyType, out parsedValue );
			}

			#endregion // ParseAttrubuteValue

			#region ToAttributeValue

			public static bool ToAttributeValue( object value, Type propertyType, out string attributeValue )
			{
				return g_instance.ToAttributeValueHelper( value, propertyType, out attributeValue );
			}

			#endregion // ToAttributeValue

			#endregion // Public Methods

			#endregion // Methods
		}

		#endregion // AttributeValueParser Class

		#endregion // Nested Data Structures

		#region Member Vars

		private XmlDocument _document;
		private Dictionary<Type, ObjectSerializationInfo> _registeredInfos;
		
		// SSP 5/17/11 TFS34944
		// 
		private XmlSerializationInfo _xmlSerializationInfo = new XmlSerializationInfo( );

		#endregion // Member Vars

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public ObjectSerializerBase( )
		{
			_document = new XmlDocument( );
			_registeredInfos = new Dictionary<Type, ObjectSerializationInfo>( );

			this.RegisterInfo( typeof( ConditionGroup ), new ConditionGroup.SerializationInfo( ) );
			this.RegisterInfo( typeof( ComparisonCondition ), new ComparisonCondition.SerializationInfo( ) );
			this.RegisterInfo( typeof( ComplementCondition ), new ComplementCondition.SerializationInfo( ) );
			SpecialFilterOperandFactory.RegisterSerializationInfos( this );
			SpecialFilterOperands.RegisterSerializationInfos( this );
		}

		#endregion // Constructor

		#region Methods

		#region Protected Methods

		#region AppendValueHelper

		/// <summary>
		/// Saves the specified value to the parentElem. It saves it as either attribute or child element depending
		/// on whether attributeName or elementName parameters are specified. If both are specified, then it 
		/// tries to save it as attribute if it can otherwise saves it as element.
		/// </summary>
		/// <param name="parentElem">Element where to save the value object.</param>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <param name="elementName">Name of the child element.</param>
		/// <param name="value">Value to save.</param>
		/// <param name="propertyType">Type of the property where the value came from. This will be used to determine if
		/// any type related information needs to be stored.</param>
		/// <returns>True if value was saved. False otherwise.</returns>
		protected bool AppendValueHelper( XmlElement parentElem, string attributeName, string elementName, object value, Type propertyType )
		{
			if ( !string.IsNullOrEmpty( attributeName ) )
			{
				string val;
				if ( this.SaveAsAttribute( value, propertyType, out val ) )
				{
					parentElem.SetAttribute( attributeName, val );
					return true;
				}
			}

			XmlElement elem;
			if ( this.InternalSaveAsElement( value, propertyType, out elem ) )
			{
				if ( !string.IsNullOrEmpty( elementName ) )
				{
					XmlElement childElem = this.CreateNewElement( elementName );
					parentElem.AppendChild( childElem );
					parentElem = childElem;
				}

				parentElem.AppendChild( elem );
				return true;
			}

			return false;
		}

		#endregion // AppendValueHelper

		#region CreateNewElement

		/// <summary>
		/// Creates a new empty XmlElement.
		/// </summary>
		/// <param name="name">Name of the created XmlElement.</param>
		/// <returns>New empty XmlElement.</returns>
		protected XmlElement CreateNewElement( string name )
		{
			return _document.CreateElement( name );
		}

		#endregion // CreateNewElement

		#region ParseAttributeOverride

		/// <summary>
		/// Parses the specified attribute. This along with the SaveAsAttributeOverride methods are meant 
		/// to be overridden by the derived classes to provide save/parse logic for object types
		/// for which default save/load logic does not exist.
		/// </summary>
		/// <param name="attributeValue">Attribute value.</param>
		/// <param name="propertyType">Type of the property whose value was serialized.</param>
		/// <param name="parsedValue">This out parameter is set to the result.</param>
		/// <returns>Returns true if the attribute value is parsed. False otherwise.</returns>
		protected virtual bool ParseAttributeOverride( string attributeValue, Type propertyType, out object parsedValue )
		{
			parsedValue = null;
			return false;
		}

		#endregion // ParseAttributeOverride

		#region ParseElementOverride

		/// <summary>
		/// Parses the specified element into an object of specified propertyType.
		/// This along with the SaveElementOverride methods are meant 
		/// to be overridden by the derived classes to provide save/parse logic for object types
		/// for which default save/load logic does not exist.
		/// </summary>
		/// <param name="element">XmlElement to parse.</param>
		/// <param name="propertyType">Type of property whose value was serialized.</param>
		/// <param name="parsedValue">This will be set to the parsed value.</param>
		/// <returns>Returns true if the element was succesfully parsed into an object of specified property type. Flase otherwise.</returns>
		protected virtual bool ParseElementOverride( XmlElement element, Type propertyType, out object parsedValue )
		{
			parsedValue = null;
			return false;
		}

		#endregion // ParseElementOverride

		#region SaveAsAttributeOverride

		/// <summary>
		/// Saves the following object as an attribute. This along with the ParseAttributeOverride methods are meant 
		/// to be overridden by the derived classes to provide save/parse logic for object types
		/// for which default save/load logic does not exist.
		/// </summary>
		/// <param name="obj">Object to serialize.</param>
		/// <param name="propertyType">Type of the property whose value is being serialized.</param>
		/// <param name="attributeValue">This will be set to the resultant attribute value.</param>
		/// <returns>True if success, false if the specified object cannot be saved as an attribute.</returns>
		protected virtual bool SaveAsAttributeOverride( object obj, Type propertyType, out string attributeValue )
		{
			attributeValue = null;
			return false;
		}

		#endregion // SaveAsAttributeOverride

		#region SaveAsElementOverride

		/// <summary>
		/// Saves the following object as an XmlElement.
		/// This along with the ParseElementOverride methods are meant 
		/// to be overridden by the derived classes to provide save/parse logic for object types
		/// for which default save/load logic does not exist.
		/// </summary>
		/// <param name="obj">Object to save as an element.</param>
		/// <param name="propertyType">Type of property whose value is being saved.</param>
		/// <param name="element">This will be set to the element that was created.</param>
		/// <returns>True if success, false otherwise.</returns>
		protected virtual bool SaveAsElementOverride( object obj, Type propertyType, out XmlElement element )
		{
			element = null;
			return false;
		}

		#endregion // SaveAsElementOverride

		#endregion // Protected Methods

		#region Public Methods

		#region ParseAttribute

		/// <summary>
		/// Parses the specified attribute.
		/// </summary>
		/// <param name="elem">XmlElement whose attribute is to be parsed.</param>
		/// <param name="attributeName">Name of the attribute in the specified element that needs to be parsed back into an object.</param>
		/// <param name="propertyType">Type of the property whose value was serialized.</param>
		/// <param name="parsedValue">This out parameter is set to the result.</param>
		/// <returns>Returns true if the attribute value is parsed. False otherwise.</returns>
		public bool ParseAttribute( XmlElement elem, string attributeName, Type propertyType, out object parsedValue )
		{
			parsedValue = null;

			XmlAttribute attr = elem.Attributes[attributeName];
			return null != attr && this.ParseAttribute( attr.Value, propertyType, out parsedValue );
		}

		/// <summary>
		/// Parses the specified attribute.
		/// </summary>
		/// <param name="attributeValue">Attribute value.</param>
		/// <param name="propertyType">Type of the property whose value was serialized.</param>
		/// <param name="parsedValue">This out parameter is set to the result.</param>
		/// <returns>Returns true if the attribute value is parsed. False otherwise.</returns>
		public bool ParseAttribute( string attributeValue, Type propertyType, out object parsedValue )
		{
			if ( this.ParseAttributeOverride( attributeValue, propertyType, out parsedValue ) )
				return true;

			if ( AttributeValueParser.ParseAttrubuteValue( attributeValue, propertyType, out parsedValue ) )
				return true;

			return false;
		}

		#endregion // ParseAttribute

		#region ParseElement

		/// <summary>
		/// Parses the specified element back into an object.
		/// </summary>
		/// <param name="element">XmlElement to parse.</param>
		/// <param name="propertyType">Type of the property whose value was saved.</param>
		/// <param name="parsedValue">This will be set to the parsed value.</param>
		/// <returns>True if success, false otherwise.</returns>
		public bool ParseElement( XmlElement element, Type propertyType, out object parsedValue )
		{
			if ( this.ParseElementOverride( element, propertyType, out parsedValue ) )
				return true;

			if ( this.InternalParseElement( element, propertyType, out parsedValue ) )
				return true;

			parsedValue = null;
			return false;
		}

		#endregion // ParseElement

		#region SaveAsAttribute

		/// <summary>
		/// Saves the following object as an attribute.
		/// </summary>
		/// <param name="obj">Object to serialize.</param>
		/// <param name="propertyType">Type of the property whose value is being serialized.</param>
		/// <param name="attributeValue">This will be set to the resultant attribute value.</param>
		/// <returns>True if success, false if the specified object cannot be saved as an attribute.</returns>
		public bool SaveAsAttribute( object obj, Type propertyType, out string attributeValue )
		{
			if ( this.SaveAsAttributeOverride( obj, propertyType, out attributeValue ) )
				return true;

			if ( AttributeValueParser.ToAttributeValue( obj, propertyType, out attributeValue ) )
				return true;

			return false;
		}

		#endregion // SaveAsAttribute

		#region SaveAsElement

		/// <summary>
		/// Saves the following object as an XmlElement.
		/// </summary>
		/// <param name="obj">Object to save as an element.</param>
		/// <param name="propertyType">Type of property whose value is being saved.</param>
		/// <param name="element">This will be set to the element that was created.</param>
		/// <returns>True if success, false otherwise.</returns>
		public bool SaveAsElement( object obj, Type propertyType, out XmlElement element )
		{
			if ( this.SaveAsElementOverride( obj, propertyType, out element ) )
				return true;

			if ( this.InternalSaveAsElement( obj, propertyType, out element ) )
				return true;

			element = null;
			return false;
		}

		#endregion // SaveAsElement

		#endregion // Public Methods

		#region Private/Internal Methods

		#region GetFirstChildElement

		private XmlElement GetFirstChildElement( XmlElement parentElem )
		{
			foreach ( XmlNode child in parentElem.ChildNodes )
			{
				if ( child is XmlElement )
					return child as XmlElement;
			}

			return null;
		}

		#endregion // GetFirstChildElement

		#region GetRegisteredInfo

		internal ObjectSerializationInfo GetRegisteredInfo( Type type )
		{
			ObjectSerializationInfo r;
			if ( _registeredInfos.TryGetValue( type, out r ) )
				return r;

			// SSP 5/17/11 TFS34944
			// 
			if ( _xmlSerializationInfo.IsTypeSupported( type ) )
			{
				_registeredInfos[type] = r = _xmlSerializationInfo;
				return r;
			}

			return null;
		}

		#endregion // GetRegisteredInfo

		#region InternalParseElement

		private ObjectSerializationInfo GetObjectSerializationInfo( XmlElement element, Type propertyType )
		{
			// SSP 5/17/11 TFS34944
			// 
			if ( XmlSerializationInfo.ELEMENT_NAME == element.Name )
				return _xmlSerializationInfo;

			foreach ( Type registeredType in _registeredInfos.Keys )
			{
				if ( propertyType.IsAssignableFrom( registeredType )
					&& registeredType.Name == element.Name )
				{
					return this.GetRegisteredInfo( registeredType );
				}
			}

			return null;
		}

		/// <summary>
		/// Parses the specified element back into an object.
		/// </summary>
		/// <param name="element">XmlElement to parse.</param>
		/// <param name="propertyType">Type of the property whose value was saved.</param>
		/// <param name="parsedValue">This will be set to the parsed value.</param>
		/// <returns>True if success, false otherwise.</returns>
		private bool InternalParseElement( XmlElement element, Type propertyType, out object parsedValue )
		{
			if ( null != element && null != propertyType )
			{
				ObjectSerializationInfo serializationInfo = this.GetObjectSerializationInfo( element, propertyType );
				if ( null != serializationInfo )
				{
					IEnumerable<PropertySerializationInfo> serializedProperties = serializationInfo.SerializedProperties;
					Dictionary<string, object> values = new Dictionary<string, object>( );

					int failureCount = 0;
					foreach ( PropertySerializationInfo ii in serializedProperties )
					{
						string name = ii.Name;
						Type type = ii.Type;

						object v;
						if ( ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY == name )
						{
							ArrayList list = new ArrayList( );
							foreach ( XmlNode childNode in element.ChildNodes )
							{
								XmlElement childElem = childNode as XmlElement;
								if ( null != childElem && this.InternalParseElement( childElem, type, out v ) )
									list.Add( v );
							}

							values[name] = list.ToArray( type );
						}
						else
						{
							XmlAttribute attribute = element.Attributes[name];
							if ( null != attribute )
							{
								if ( this.ParseAttribute( attribute.Value, type, out v ) )
									values[name] = v;
								else
									failureCount++;
							}
							else
							{
								XmlElement childElem = element.SelectSingleNode( name ) as XmlElement;
								if ( null != childElem )
								{
									// We need to drill down one more level to get the object node from
									// the property node.
									// <PropertyName> <ObjectName></ObjectName> </PropertyName>
									// 
									childElem = this.GetFirstChildElement( childElem );
									if ( null != childElem && this.InternalParseElement( childElem, type, out v ) )
										values[name] = v;
									else
										failureCount++;
								}
							}
						}
					}

					parsedValue = serializationInfo.Deserialize( values );
					if ( null != parsedValue )
						return true;
				}
			}

			parsedValue = null;
			return false;
		}

		#endregion // InternalParseElement

		#region InternalSaveAsElement

		/// <summary>
		/// Saves the following object as an XmlElement.
		/// </summary>
		/// <param name="obj">Object to save as an element.</param>
		/// <param name="propertyType">Type of property whose value is being saved.</param>
		/// <param name="element">This will be set to the element that was created.</param>
		/// <returns>True if success, false otherwise.</returns>
		public bool InternalSaveAsElement( object obj, Type propertyType, out XmlElement element )
		{
			Type objType = null != obj ? obj.GetType( ) : null;
			if ( null != objType )
			{
				ObjectSerializationInfo serializationInfo = this.GetRegisteredInfo( objType );

				if ( null != serializationInfo )
				{
					// SSP 5/17/11 TFS34944
					// If we are saving the object using XmlSerializer then use a specific element tag instead
					// of the type name. The type name is embedded inside the element as an attribute by the
					// XmlSerializationInfo.
					// 
					string elementName = objType.Name;
					if ( serializationInfo is XmlSerializationInfo )
						elementName = XmlSerializationInfo.ELEMENT_NAME;

					element = this.CreateNewElement( elementName );
					IEnumerable<PropertySerializationInfo> serializedProperties = serializationInfo.SerializedProperties;
					Dictionary<string, object> values = serializationInfo.Serialize( obj );

					int failureCount = 0;

					foreach ( PropertySerializationInfo ii in serializedProperties )
					{
						string name = ii.Name;
						Type type = ii.Type;
						object value;
						if ( ! values.TryGetValue( name, out value ) )
							continue;

						if ( ObjectSerializationInfo.COLLECTION_ELEMENT_DESIGNATOR_KEY == name )
						{
							IEnumerable children = value as IEnumerable;
							Debug.Assert( null != children, "[] key is meant for collection elements!" );
							
							if ( null != children )
							{
								foreach ( object item in children )
								{
									if ( !this.AppendValueHelper( element, null, null, item, type ) )
										failureCount++;
								}
							}
							else
								failureCount++;
						}
						else
						{
							if ( ! this.AppendValueHelper( element, name, name, value, type ) )
								failureCount++;
						}
					}

					if ( 0 == failureCount )
						return true;
				}
			}

			element = null;
			return false;
		}

		#endregion // InternalSaveAsElement

		#region RegisterInfo

		/// <summary>
		/// Registers an instance of ObjectSerializationInfo for a specific type where ObjectSerializationInfo
		/// can serialize and deserialize instances of that type.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="info"></param>
		public void RegisterInfo( Type type, ObjectSerializationInfo info )
		{
			Utilities.ThrowIfNull( info, "info" );
			_registeredInfos[type] = info;
		}

		#endregion // RegisterInfo

		#endregion // Private/Internal Methods

		#endregion // Methods
	}

	#endregion // ObjectSerializerBase Class
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