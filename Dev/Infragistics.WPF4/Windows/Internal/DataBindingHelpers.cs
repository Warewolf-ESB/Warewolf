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
using System.Collections.ObjectModel;
using Infragistics.Windows.Helpers;
using System.Threading;
using Infragistics.Collections;

namespace Infragistics.Windows.Internal
{
    #region DataBindingUtils

    /// <summary>
    /// Internal.
    /// </summary>
    public static class DataBindingUtilities
    {
		#region Member Variables

		//private static Hashtable sampleData = null;

		#endregion //Member Variables

        #region ValuePropertyDescriptorCreator

        //// MRS 5/6/05 - Moved the code that creates Design-time sample data from
        //// the grid to here, so it can also be used by tree
        //private static Hashtable sampleData = null;

        /// <summary>
        /// Delegate for property descriptor creators.
        /// </summary>
        public delegate PropertyDescriptor ValuePropertyDescriptorCreator(Type type, string name);

        #endregion //ValuePropertyDescriptorCreator	
		
		// JJD 10/23/07 - BR26277
		// Moved this logic from DataPresenter RecordManager class to here.
		#region GetUnderlyingItemSource

		/// <summary>
		/// Gets the underlying data source.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <returns>The underlying source of the items.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if source implements <see cref="ICollectionView"/> then its <see cref="ICollectionView.SourceCollection"/> will be returned. If it implements <see cref="IListSource"/> then its <see cref="IListSource.GetList"/> method is called.</para></remarks>
		public static IEnumerable GetUnderlyingItemSource(object source)
		{
			return GetUnderlyingItemSource(source, true);
		}
		/// <summary>
		/// Gets the underlying data source.
		/// </summary>
		/// <param name="source">The source object.</param>
		/// <param name="returnCollectionViewsSourceCollection">If true and the source implements ICollectionView will return its SourceCollection.</param>
		/// <returns>The underlying source of the items.</returns>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if source implements <see cref="ICollectionView"/> then its <see cref="ICollectionView.SourceCollection"/> will be returned. If it implements <see cref="IListSource"/> then its <see cref="IListSource.GetList"/> method is called.</para></remarks>
		// JJD 9/28/11 - TFS89531
		// Added overload with returnCollectionViewsSourceCollection param
		public static IEnumerable GetUnderlyingItemSource(object source, bool returnCollectionViewsSourceCollection)
		{

			// JJD 8/1/07 - BR25196
			//if (source is ICollectionView)
			//    return ((ICollectionView)source).SourceCollection;
			//
			//return source;

            // JJD 11/24/09 - TFS25123
            // If the data source implements ICollectionViewFactory then call 
            // CollectionViewSource.GetDefaultView
            if (source is ICollectionViewFactory)
                source = CollectionViewSource.GetDefaultView(source);

			IEnumerable underlyingSource;

			// JJD 9/28/11 - TFS89531
			// Check returnCollectionViewsSourceCollection param before returning SourceCollection
			//if (source is ICollectionView)
			if (returnCollectionViewsSourceCollection && source is ICollectionView)
				underlyingSource = ((ICollectionView)source).SourceCollection;
			else
				underlyingSource = source as IEnumerable;

			// JJD 8/1/07 - BR25196
			// If the underlying source implements IListSource the get the list
			// thru its GetList method
			IListSource listSource = underlyingSource as IListSource;

			if (listSource != null)
				underlyingSource = listSource.GetList();

            // JJD 4/29/08
            // If the underlyingSource is null at this point then return the passed insource
            if (underlyingSource == null)
                return source as IEnumerable;

			return underlyingSource;
		}

				#endregion //GetUnderlyingSource	

        #region GetXmlNodeProperties

        /// <summary>
        /// Gets the collection of property descriptors for an XmlNode. 
        /// </summary>
        /// <param name="node">the node</param>
        /// <returns>Property collection.</returns>
        public static PropertyDescriptorCollection GetXmlNodeProperties(XmlNode node)
        {
			List<PropertyDescriptor> props = new List<PropertyDescriptor>();

            if (node.Attributes != null)
            {
                foreach (XmlAttribute attribute in node.Attributes)
                    props.Add(new XmlNodePropertyDescriptor(attribute));
            }


            if (node.ChildNodes != null)
            {
				int countOfChildren = 0;
				int countOfChildTextNodes = 0;

				// JJD 4/23/07 - BR21716
				// Loop over the child nodes to get a count of the children
				foreach (XmlNode child in node.ChildNodes)
				{
					if (child.NodeType == XmlNodeType.Element)
						countOfChildren++;
					else
					if (child.NodeType == XmlNodeType.Text)
					{
						countOfChildren++;
						countOfChildTextNodes++;
					}
				}

				if (countOfChildTextNodes > 0 &&
					 countOfChildren > 1)
				{
					// JJD 4/23/07 - BR21716
					// Added new constructor to allow inner text to be used for situations
					// where there are multiple child nodes and some of them are text elements
					props.Add(new XmlNodePropertyDescriptor("Text"));
				}
				else
				{
                    // JJD 3/26/09 - TFS15578
                    // create dictionaries on the stack to keep track of single nodes. When more that one
                    // node has the same name we nned to create a property that returns a list of those child 
                    // nodes. Those are held in the listProps dictionary
                    Dictionary<string, XmlNode> singleElementNodes = new Dictionary<string, XmlNode>();
                    Dictionary<string, XmlNodePropertyDescriptor> listProps = new Dictionary<string, XmlNodePropertyDescriptor>();

					foreach (XmlNode child in node.ChildNodes)
					{
						// JJD 4/18/07 - BR21716
						// Only add nodes whose NodeType is 'Element' or 'Text'
						switch (child.NodeType )
						{
                            case XmlNodeType.Element:
                                {
                                    string name = child.Name;

                                    if (name != null && name.Length > 0)
                                    {
                                        // JJD 3/26/09 - TFS15578
                                        // if the name is already in the listProps dictionary we can 
                                        // ignore this child and continue
                                        if (listProps.ContainsKey(name))
                                            continue;

                                        // JJD 3/26/09 - TFS15578
                                        // see if the name was already encountered
                                        if (singleElementNodes.ContainsKey(name))
                                        {
                                            // JJD 3/26/09 - TFS15578
                                            // since we have a duplicate name remove the entry in the
                                            // singleElement collection and add the name to the 
                                            // listProps dictionary
                                            XmlNode temp = singleElementNodes[name];
                                            singleElementNodes.Remove(name);
                                            listProps.Add(name, new XmlNodePropertyDescriptor(name, temp.LocalName, true));
                                        }
                                        else
                                        {
                                            // JJD 3/26/09 - TFS15578
                                            // add the elelemtn to the singleElementNodes dictionary
                                            // since this is the first chld node with that name
                                            singleElementNodes.Add(name, child);
                                        }

                                        // JJD 3/26/09 - TFS15578
                                        // continue here so we can delay adding the property descriptors until
                                        // after the foreach loop completes
                                        continue;
                                    }
								
                                    props.Add(new XmlNodePropertyDescriptor(child));

                                    break;
                                }
							case XmlNodeType.Text:
								props.Add(new XmlNodePropertyDescriptor(child));
							    break;
						}
					}

                    // JJD 3/26/09 - TFS15578
                    // Process any singleElement nodes we encountered above
                    if (singleElementNodes.Count > 0)
                    {
                        foreach (XmlNode child in singleElementNodes.Values)
                        {
                            XmlAttributeCollection childAttribs = child.Attributes;
                            string childName = child.Name;

                            // JJD 3/26/09 - TFS15578
                            // If the node has a name and it has attributes then treat it as a list
                            // of one since that way we can pick up the attribute data.
                            if (childAttribs != null && 
                                childAttribs.Count > 0 &&
                                childName != null &&
                                childName.Length > 0)
                            {
                                listProps.Add(childName, new XmlNodePropertyDescriptor(childName, child.LocalName, true));
                            }
                            else
                            {
                                // JJD 3/26/09 - TFS15578
                                // Otherwise add a property descriptor for the child node
                                props.Add(new XmlNodePropertyDescriptor(child));
                            }
                        }
                    }

                    // JJD 3/26/09 - TFS15578
                    // Add a list type property for each listprop we encountered above
                    if (listProps.Count > 0)
                    {
                        foreach (XmlNodePropertyDescriptor prp in listProps.Values)
							props.Add(prp);
                    }
				}
            }

			PropertyDescriptor[] propArray = new PropertyDescriptor[props.Count];

			if (props.Count > 0)
				props.CopyTo(propArray);

            return new PropertyDescriptorCollection(propArray);
        }

        #endregion //GetXmlNodeProperties	
    
        #region IsKnownType

        // ZS 3/3/04 - I moved this in Infragistics.Win as it is common code for Grid, TabStripControl and
        // CalendarInfo data binding.
        /// <summary>
        /// Checks if the type is a known type (to Infragistics controls).
        /// </summary>
        public static bool IsKnownType(Type type)
        {
			// JJD 10/27/11 - TFS92815 - Moved logic to CoreUtilities so it could be easily shared in SL
			return CoreUtilities.IsKnownType(type);
			
#region Infragistics Source Cleanup (Region)


























#endregion // Infragistics Source Cleanup (Region)

        }

        #endregion //IsKnownType

        #region StripMember

        /// <summary>
        /// Used to strip out all characters that are before the last period
        /// of the passed in value.
        /// </summary>
        public static string StripMember(string text)
        {
            if (text == null || text.Length < 1)
                return string.Empty;

            string[] strippedValue = text.Split('.');

            int count = strippedValue.Length - 1;

            return strippedValue[count];
        }

        #endregion //StripMember	
    
        #region GetObjectForComparison

        /// <summary>
        /// This method returns the object that will be compared against when syncing data bound items
        /// with the underlying bound list.
        /// </summary>
        /// <param name="listObject"></param>
        /// <returns></returns>
        public static object GetObjectForComparison(object listObject)
        {
            System.Data.DataRowView dvr = listObject as System.Data.DataRowView;
            if (null != dvr)
                return dvr.Row;

			// AS 5/13/09 
			// Moved this here from the DataPresenter's GetObjectForRecordComparision.
			//
			// JJD 12/13/07
			// If the listobject is an EnumerableObjectWrapper them
			// return its underlying items 
			EnumerableObjectWrapper wrapper = listObject as EnumerableObjectWrapper;

			if (wrapper != null)
				return wrapper.Items;

			return listObject;
        }

        #endregion //GetObjectForComparison	

		#region GetDataSourceNullValue

		// SSP 2/20/09 TFS13093
		// Adedd GetDataSourceNullValue method.
		// 
		/// <summary>
		/// Gets the null or empty value that should be used for the specified property descriptor.
		/// </summary>
		/// <param name="propertyDescriptor">Property descriptor instance</param>
		/// <param name="nullValue">This out param will be set to the null value.</param>
		/// <returns>True if null value was ascertained from property descriptor, false otherwise.</returns>
		public static bool GetDataSourceNullValue( PropertyDescriptor propertyDescriptor, out object nullValue )
		{
			// If the data source is DataTable or DataView then use DBNull.
			// 
			Type componentType = propertyDescriptor.ComponentType;
			if ( propertyDescriptor is DataColumnPropertyDescriptor
				|| typeof( DataRowView ).IsAssignableFrom( componentType )
				|| typeof( DataRow ).IsAssignableFrom( componentType ) )
			{
				nullValue = DBNull.Value;
				return true;
			}

			Type propertyType = propertyDescriptor.PropertyType;
			Type underlyingType = Nullable.GetUnderlyingType( propertyType );
			
			// If the property type is Nullable then default value is null.
			// 
			bool isNullable = null != underlyingType && underlyingType != propertyType;
			if ( isNullable )
			{
				nullValue = null;
				return true;
			}

			// If there's DefaultValueAttribute and the default value is null or
			// dbnull then return that value.
			// 
			AttributeCollection attribs = propertyDescriptor.Attributes;
			DefaultValueAttribute defValAttrib = null != attribs ? (DefaultValueAttribute)attribs[ typeof( DefaultValueAttribute ) ] : null;
			if ( null != defValAttrib )
			{
				object defVal = defValAttrib.Value;
				if ( null == defVal || DBNull.Value == defVal )
				{
					nullValue = defVal;
					return true;
				}
			}

			bool isValueType = propertyType.IsValueType;
			if ( ! isValueType )
			{
				// If the property type is reference type then return null
				// for DependencyPropertyDescriptor and ReflectPropertyDescriptor.
				// 

				// JJD 2/4/11 - TFS65133
				// Also use null if the property descriptor is a ValuePropertyDescriptor, e.g. that represents a string
				//if ( propertyDescriptor is DependencyPropertyDescriptor )
				if ( propertyDescriptor is DependencyPropertyDescriptor || propertyDescriptor is ValuePropertyDescriptor)
				{
					nullValue = null;
					return true;
				}

				Type pdType = propertyDescriptor.GetType( );
				if ( "ReflectPropertyDescriptor" == pdType.Name )
				{
					nullValue = null;
					return true;
				}
			}
			else
			{
				// If it's value type then return UnsetValue for dependency property descriptor.
				// 
				if ( propertyDescriptor is DependencyPropertyDescriptor )
				{
					nullValue = DependencyProperty.UnsetValue;
					return true;
				}

				// If it's value type then return default value atribute value if any.
				// 
				if ( null != defValAttrib )
				{
					nullValue = defValAttrib.Value;
					return true;
				}
			}

			// We can't ascertain the whether to use null or dbnull. Return false.
			// 
			nullValue = null;
			return false;
		}

		#endregion // GetDataSourceNullValue

		#region DataBindConvert

		/// <summary>
        /// Converts source value to destination type using specific culture.
        /// </summary>
        /// <param name="source">Value to convert. Can be null or DBNull.</param>
        /// <param name="destinationType">Type to convert to.</param>
        /// <param name="culture">Specific culture used for conversion. If null, default culture is used.</param>
        /// <param name="format">Format string.</param>
        /// <param name="formatInfo">Format provider.</param>
        /// <param name="validConversion">True if the conversion was valid.</param>
        /// <returns>Converted value.</returns>
        public static object DataBindConvert(object source, Type destinationType, CultureInfo culture,
            string format, IFormatProvider formatInfo, out bool validConversion)
        {
            // Conversion is valid until proven opposite.
            validConversion = true;

            // Return source if it is null or DBNull.
            if (source == null || source is DBNull)
                return source;

            Type sourceType = source.GetType();

            // Return source if it is of the same type or is derived type.

			// JJD 6/14/07
			// Use IsAssignableFrom instead of IsSubclassOf since that is 10x more efficient
            //if (sourceType == destinationType || sourceType.IsSubclassOf(destinationType))
			if (destinationType.IsAssignableFrom(sourceType))
                return source;

            // Otherwise get type converters and convert if possible.

            // Try with source type converter.
            TypeConverter sourceTC = TypeDescriptor.GetConverter(sourceType);
            if (sourceTC != null && sourceTC.CanConvertTo(destinationType))
            {
                try
                {
                    return sourceTC.ConvertTo(null, culture, source, destinationType);
                }
                catch (NotSupportedException)
                {
                }
            }

            // Try with destination type converter.
            TypeConverter destinationTC = TypeDescriptor.GetConverter(destinationType);
            if (destinationTC != null && destinationTC.CanConvertFrom(sourceType))
            {
                try
                {
                    return destinationTC.ConvertFrom(null, culture, source);
                }
                catch (NotSupportedException)
                {
                }
            }

            // Special case if destination type is string.
            if (destinationType == typeof(string))
            {
				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				string text = Utilities.ToString(source, format, formatInfo, false);

				return text ?? source.ToString();
            }

            // Try with Convert.ChangeType().
            if (source is IConvertible)
            {
                try
                {
                    if (formatInfo != null)
                        return Convert.ChangeType(source, destinationType, formatInfo);
                    else if (culture != null)
                        return Convert.ChangeType(source, destinationType, culture);
                    else
                        return Convert.ChangeType(source, destinationType);
                }
                catch (InvalidCastException)
                {
                }
            }

            // No conversion is possible.
            validConversion = false;
            return null;
        }

        #endregion //DataBindConvert	
    
        #region GetSampleDataForType

		
#region Infragistics Source Cleanup (Region)














































#endregion // Infragistics Source Cleanup (Region)


        #endregion //GetSampleDataForType - commented out

		#region GetSampleData

		private static XmlDataProvider g_sampleDataProvider;

		/// <summary>
		/// Creates an xml data provider used to display sample data
		/// </summary>
		/// <returns>Returns an object suitable for use as a item source for an ItemsControl derived class"/></returns>
		public static IEnumerable GetSampleData()
		{
			if (g_sampleDataProvider == null)
			{
				// JM 02-25-10 Task TFS 28356
				byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(SR.GetString("SampleData_XML"));
				System.IO.Stream stream = new System.IO.MemoryStream(byteArray);
				
				if (stream != null)
				{
					g_sampleDataProvider = new XmlDataProvider();
					XmlDocument doc = new XmlDocument();
					doc.Load(stream);
					using (g_sampleDataProvider.DeferRefresh())
					{
						g_sampleDataProvider.Document = doc;
						g_sampleDataProvider.XPath = AssemblyRef.SampleDataXPath; // @"/SampleData/users/user";
						g_sampleDataProvider.IsAsynchronous = false;
					}
					g_sampleDataProvider.Refresh();
					g_sampleDataProvider.InitialLoad();

					stream.Close();
				}
			}

			return g_sampleDataProvider.Data as IEnumerable;
		}

		#endregion //GetSampleData

    }

    #endregion //DataBindingUtils	

    #region DataColumnPropertyDescriptor

    /// <summary>
    /// Wraps a DataColumn to provide consisten access to cell data
    /// </summary>
    public class DataColumnPropertyDescriptor : PropertyDescriptor
    {
        private DataColumn _column;

        /// <summary>
		/// Initializes a new instance of the <see cref="DataColumnPropertyDescriptor"/> class.
        /// </summary>
        /// <param name="column">The associated <see cref="DataColumn"/></param>
        public DataColumnPropertyDescriptor(DataColumn column)
            : base(column.ColumnName, null)
        {
			if (column == null)
				throw new ArgumentNullException("column");
			this._column = column;
        }

        /// <summary>
        /// Gets the DataColumn
        /// </summary>
		public DataColumn Column { get { return this._column; } }

        /// <summary>
        /// Gets component type.
        /// </summary>
        public override Type ComponentType
        {
            get
            {
                return typeof(DataRow);
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Returns the name to display for the property
        /// </summary>
        public override string DisplayName
        {
            get
            {
				string caption = this._column.Caption;

				if (caption != null && caption.Length > 0)
					return caption;

                return this._column.ColumnName;
            }
        }

        /// <summary>
        /// Returns the value of the property on the passed in object.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override object GetValue(object component)
        {
            DataRow row = DataColumnPropertyDescriptor.GetRow(component);

			return row[this._column];
        }

		private static DataRow GetRow(object component)
        {
            DataRow row = component as DataRow;

			if (row == null)
			{
				DataRowView drv = component as DataRowView;

				if (drv != null)
					row = drv.Row;
			}

            if (row == null)
                throw new ArgumentException(SR.GetString("LE_ArgumentException_1"));

			return row;
        }

        /// <summary>
        /// Gets property type.
        /// </summary>
        public override Type PropertyType
        {
            get
            {
				return this._column.DataType;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="component"></param>
        public override void ResetValue(object component)
        {
            // DO nothing since CanReset returns false
            //
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        // JJD 5/26/09 - TFS17590
        /// <summary>
        /// Returns true is the value is browsable.
        /// </summary>
        public override bool IsBrowsable
        {
            get
            {
                return (this._column.ColumnMapping != MappingType.Hidden) && base.IsBrowsable;
            }
        }

        /// <summary>
        /// Returns true is the value is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return this._column.ReadOnly;
            }
        }

        /// <summary>
        /// Sets the value of the property on the passed in object.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        public override void SetValue(object component, object value)
        {
			DataRow row = DataColumnPropertyDescriptor.GetRow(component);

			row[this._column] = value;
		}
	}

	#endregion //DataColumnPropertyDescriptor

	#region ValuePropertyDescriptor

	/// <summary>
    /// Represents a pseudo property to deal with arrays of 
    /// primitive types.
    /// </summary>
    public class ValuePropertyDescriptor : PropertyDescriptor
    {
        private Type _type;

        /// <summary>
		/// Initializes a new instance of the <see cref="ValuePropertyDescriptor"/> class
        /// </summary>
        /// <param name="type">The associated property type</param>
        /// <param name="name">The name of the property</param>
        public ValuePropertyDescriptor(Type type, string name)
            : base(name, null)
        {
            _type = type;
        }

        /// <summary>
        /// Gets component type.
        /// </summary>
        public override Type ComponentType
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Returns the passed in object.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override object GetValue(object component)
        {
            // Passed in component would be the value in the bound list.
            // For example if we were bound to an array of DateTime objects then
            // the component passed in would be an instance of DateTime.
            //
            return component;
        }

        /// <summary>
        /// Gets property type.
        /// </summary>
        public override Type PropertyType
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="component"></param>
        public override void ResetValue(object component)
        {
            // DO nothing since CanReset returns false
            //
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

		/// <summary>
		/// Returns a value indicating whether the property is read-only.
		/// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Returns a value indicating whether the source list is read-only.
		/// </summary>
		/// <param name="sourceList"></param>
		/// <returns></returns>
		public bool IsReadOnlyList( IEnumerable sourceList )
		{
			IList list = sourceList as IList;

			return null == list || list.IsReadOnly;
		}

		/// <summary>
		/// Component is an instance of a DataItem for which to set
		/// the value to the bound list.
		/// </summary>
		/// <param name="dataitem"></param>
		/// <param name="value"></param>
		public override void SetValue( object dataitem, object value )
		{
		}
    }

    #endregion //ValuePropertyDescriptor

    #region XmlNodePropertyDescriptor

    /// <summary>
    /// Represents a property on an xml node..
    /// </summary>
    public class XmlNodePropertyDescriptor : PropertyDescriptor
    {
        private Type _type;
        private IXmlSchemaInfo _schema;
        private bool _isReadOnly;
        private bool _isAttribute;
        private bool _useInnerText;
        private string _displayName;

		// JJD 4/23/07 - BR21716
		// Added new constructor to allow inner text to be used for situations
		// where there are multiple child nodes and some of them are text elements
		internal XmlNodePropertyDescriptor(string name)
            // JJD 3/26/09 TFS15578
            // call the new ctor instead passing in false for treatAsList
            //: base(name, null)
            : this(name, name, false)
		{
            //this._isReadOnly = true;
            //this._useInnerText = true;
            //this._type = typeof(string);
            //this._displayName = name;
		}

		// JJD 3/26/09 TFS15578
		// Added new constructor to allow us to contruct a list of child nodes
        // based on the node name
		internal XmlNodePropertyDescriptor(string name, string localName, bool treatAsList)
			: base(name, null)
		{
			this._isReadOnly = true;

		    // JJD 3/26/09 TFS15578
            // we return an IList as the value if treatAsList is true
            if (treatAsList)
            {
                this._useInnerText = false;
                this._type = typeof(IList);
            }
            else
            {
                this._useInnerText = true;
                this._type = typeof(string);
            }

			this._displayName = localName;
		}

        /// <summary>
		/// Initializes a new instance of the <see cref="XmlNodePropertyDescriptor"/> class
        /// </summary>
        /// <param name="node">The associated xml node</param>
        public XmlNodePropertyDescriptor(XmlNode node)
            : base(node.Name, null)
        {
            this._isAttribute = node is XmlAttribute;
            this._isReadOnly = node.IsReadOnly;
            this._schema = node.SchemaInfo;
            this._displayName = node.LocalName;

            if (node.HasChildNodes &&
                (node.ChildNodes.Count > 1 ||
                (node.ChildNodes.Count == 1 && node.FirstChild is XmlElement)))
                this._type = typeof(IEnumerable);
            else
            {
                if (this._schema != null)
                {
                    if (this._schema.MemberType != null && this._schema.MemberType.Datatype != null)
                        this._type = this._schema.MemberType.Datatype.ValueType;

                    if (this._type == null && this._schema.SchemaType != null && this._schema.SchemaType.Datatype != null)
                        this._type = this._schema.SchemaType.Datatype.ValueType;
                }

                if (this._type == null)
                    this._type = typeof(string);
            }

        }

		/// <summary>
		/// Returns the IXmlSchemaInfo object
		/// </summary>
		public IXmlSchemaInfo SchemaInfo { get { return this._schema; } }

        /// <summary>
        /// Gets component type.
        /// </summary>
        public override Type ComponentType
        {
            get
            {
				// JJD 4/23/07 
				// This should return XmlNode not the property type
                //return this._type;
                return typeof(XmlNode);
            }
        }

        /// <summary>
        /// Returns false.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool CanResetValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Returns the name to display for the property
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return this._displayName;
            }
        }

		/// <summary>
        /// Returns the value of the property on the passed in object.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override object GetValue(object component)
        {
            XmlNode node = component as XmlNode;

            if (node == null)
                throw new ArgumentException(SR.GetString("LE_ArgumentException_2"));

			// JJD 4/19/07
			// Moved logic into common routine so we pick up variations in how the xml 
			// is specified from node to node (i.e. whether a property is specified as a attribute
			// or a child node)
			object value = this.GetValueInternal(node, true);
			
			string text = value as string;

			if (value != null &&
				 text == null)
				return value;

			//string text = null;

			//if (this._isAttribute)
			//{
			//    XmlNode attribute = node.Attributes.GetNamedItem(this.Name);

			//    if (attribute == null)
			//        return null;

			//    text = attribute.InnerText;
			//}
			//else
			//{
			//    XmlNode child = node.FirstChild;

			//    while (child != null)
			//    {
			//        if (child.Name == this.Name)
			//        {
			//            if (this._type == typeof(IEnumerable))
			//                return child.ChildNodes;
			//            else
			//            {
			//                text = child.InnerText;
			//                break;
			//            }
			//        }

			//        child = child.NextSibling;
			//    }
			//}

            #region Convert and return the specified type

            if (this._type == typeof(string))
                return text;

            if (this._type == typeof(bool))
                return XmlConvert.ToBoolean(text);

            if (this._type == typeof(Byte))
                return XmlConvert.ToByte(text);

            if (this._type == typeof(Char))
                return XmlConvert.ToChar(text);

            if (this._type == typeof(DateTime))
                return XmlConvert.ToDateTime(text, XmlDateTimeSerializationMode.RoundtripKind);

            if (this._type == typeof(Decimal))
                return XmlConvert.ToDecimal(text);

            if (this._type == typeof(double))
                return XmlConvert.ToDouble(text);

            if (this._type == typeof(Guid))
                return XmlConvert.ToGuid(text);

            if (this._type == typeof(Int16))
                return XmlConvert.ToInt16(text);

            if (this._type == typeof(Int32))
                return XmlConvert.ToInt32(text);

            if (this._type == typeof(Int64))
                return XmlConvert.ToInt64(text);

            if (this._type == typeof(SByte))
                return XmlConvert.ToSByte(text);

            if (this._type == typeof(Single))
                return XmlConvert.ToSingle(text);

            if (this._type == typeof(TimeSpan))
                return XmlConvert.ToTimeSpan(text);

            if (this._type == typeof(UInt16))
                return XmlConvert.ToUInt16(text);

            if (this._type == typeof(UInt32))
                return XmlConvert.ToUInt32(text);

            if (this._type == typeof(UInt64))
                return XmlConvert.ToUInt64(text);

            if ( typeof(IConvertible).IsAssignableFrom(this._type))
                return ((IConvertible)text).ToType(this._type, null);

            return text;
            
            #endregion //Convert and return the specified type
        }

        /// <summary>
        /// Gets property type.
        /// </summary>
        public override Type PropertyType
        {
            get
            {
                return this._type;
            }
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="component"></param>
        public override void ResetValue(object component)
        {
            // DO nothing since CanReset returns false
            //
        }

        /// <summary>
        /// Returns true.
        /// </summary>
        /// <param name="component"></param>
        /// <returns></returns>
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        /// <summary>
        /// Returns true is the value is read-only.
        /// </summary>
        public override bool IsReadOnly
        {
            get
            {
                return this._isReadOnly;
            }
        }

        /// <summary>
        /// Sets the value of the property on the passed in object.
        /// </summary>
        /// <param name="component"></param>
        /// <param name="value"></param>
        public override void SetValue(object component, object value)
        {
            XmlNode node = component as XmlNode;

            if (node == null)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_2" ) );
			
			// JJD 4/23/07 - BR21716
			// Added new constructor to allow inner text to be used for situations
			// where there are multiple child nodes and some of them are text elements.
			// Howver, these are read only
			if (this._useInnerText)
				throw new ReadOnlyException();

            string text = value as string;

            if (value != null && text == null)
            {
                // convert the value to a string
                if (value is bool)
                    text = XmlConvert.ToString((bool)value);
                else if (value is Byte)
                    text = XmlConvert.ToString((Byte)value);
                else if (value is Char)
                    text = XmlConvert.ToString((Char)value);
                else if (value is DateTime)
                    text = XmlConvert.ToString((DateTime)value, XmlDateTimeSerializationMode.RoundtripKind);
                else if (value is Decimal)
                    text = XmlConvert.ToString((Decimal)value);
                else if (value is double)
                    text = XmlConvert.ToString((double)value);
                else if (value is Guid)
                    text = XmlConvert.ToString((Guid)value);
                else if (value is Int16)
                    text = XmlConvert.ToString((Int16)value);
                else if (value is Int32)
                    text = XmlConvert.ToString((Int32)value);
                else if (value is Int64)
                    text = XmlConvert.ToString((Int64)value);
                else if (value is SByte)
                    text = XmlConvert.ToString((SByte)value);
                else if (value is Single)
                    text = XmlConvert.ToString((Single)value);
                else if (value is TimeSpan)
                    text = XmlConvert.ToString((TimeSpan)value);
                else if (value is UInt16)
                    text = XmlConvert.ToString((UInt16)value);
                else if (value is UInt32)
                    text = XmlConvert.ToString((UInt32)value);
                else if (value is UInt64)
                    text = XmlConvert.ToString((UInt64)value);
                else
                    text = value.ToString();
            }
			// JJD 4/19/07
			// Moved logic into common routine so we pick up variations in how the xml 
			// is specified from node to node (i.e. whether a property is specified as a attribute
			// or a child node)
			this.SetValueInternal(node, text, true);

			//XmlNode childNode = null;
			//string oldText = null;
                
			//if (this._isAttribute)
			//{
			//    childNode = node.Attributes.GetNamedItem(this.Name);
			//}
			//else
			//{
			//    XmlNode child = node.FirstChild;

			//    while (child != null)
			//    {
			//        if (child.Name == this.Name)
			//        {
			//            childNode = child;
			//            break;
			//        }

			//        child = child.NextSibling;
			//    }
			//}

			//if (childNode != null)
			//{
			//    try
			//    {
			//        // cache the old value
			//        oldText = childNode.InnerText;

			//        // set the new value
			//        childNode.InnerText = text;

			//        // validate the node
			//        if ( childNode.OwnerDocument != null &&
			//            childNode.OwnerDocument.Schemas.Count > 0 )
			//            childNode.OwnerDocument.Validate(new ValidationEventHandler(OnValidateChild), childNode);
			//    }
			//    catch
			//    {
			//        // restore the old value
			//        childNode.InnerText = oldText;

			//        //re-throw the exception
			//        throw;
			//    }
			//}
        }

		// JJD 4/19/07
		// Moved logic into common routine so we pick up variations in how the xml 
		// is specified from node to node (i.e. whether a property is specified as a attribute
		// or a child node)
		private object GetValueInternal(XmlNode node, bool useDefaultSource)
		{
			// JJD 4/23/07 - BR21716
			// Added new constriuctor to allow inner text to be used for situations
			// where there are multiple child nodes and some of them are text elements
			if (this._useInnerText)
				return node.InnerText;

			bool useAttribute;

			if (useDefaultSource == true)
				useAttribute = this._isAttribute;
			else
				useAttribute = !this._isAttribute;

			if (useAttribute)
			{
				XmlNode attribute = node.Attributes.GetNamedItem(this.Name);

				if (attribute == null)
				{
					// JJD 4/19/07
					// if we are on the 1st pass (using the default) then call this
					// method again passing false as the useDefaultSource so we pick 
					// up variations in how the xml is specified from node to node
					if (useDefaultSource == true)
						return GetValueInternal(node, false);
					else
						return null;
				}

				return attribute.InnerText;
			}
			else
			{
                // JJD 3/26/09 - TFS15578
                // create a a List if the type is IList
                List<XmlNode> list = this._type == typeof(IList) ? new List<XmlNode>() : null;

				XmlNode child = node.FirstChild;

				while (child != null)
				{
					if (child.Name == this.Name)
					{
                        // JJD 3/26/09 - TFS15578
                        // if this is a list then just add it in and continue
                        if (list != null)
                            list.Add(child);
                        else
						if (this._type == typeof(IEnumerable))
							return child.ChildNodes;
						else
							return child.InnerText;
					}

					child = child.NextSibling;
				}

                // JJD 3/26/09 - TFS15578
                // If this is a list then just return it
                if (list != null)
                    return list;

				// JJD 4/19/07
				// if we are on the 1st pass (using the default) then call this
				// method again passing false as the useDefaultSource so we pick 
				// up variations in how the xml is specified from node to node
				if (useDefaultSource == true)
					return GetValueInternal(node, false);
				else
					return null;
			}

		}

		// JJD 4/19/07
		// Moved logic into common routine so we pick up variations in how the xml 
		// is specified from node to node (i.e. whether a property is specified as a attribute
		// or a child node)
		private void SetValueInternal(XmlNode node, string text, bool useDefaultSource)
		{
			bool useAttribute;

			if (useDefaultSource == true)
				useAttribute = this._isAttribute;
			else
				useAttribute = !this._isAttribute;

			XmlNode childNode = null;
			string oldText = null;

			if (useAttribute)
			{
				childNode = node.Attributes.GetNamedItem(this.Name);
			}
			else
			{
				XmlNode child = node.FirstChild;

				while (child != null)
				{
					if (child.Name == this.Name)
					{
						childNode = child;
						break;
					}

					child = child.NextSibling;
				}
			}

			if (childNode == null)
			{
				if (useDefaultSource)
				{
					this.SetValueInternal(node, text, false);
					return;
				}
			}

			if (childNode != null)
			{
				try
				{
					// cache the old value
					oldText = childNode.InnerText;

					// set the new value
					childNode.InnerText = text;

					// validate the node
					if (childNode.OwnerDocument != null &&
						childNode.OwnerDocument.Schemas.Count > 0)
						childNode.OwnerDocument.Validate(new ValidationEventHandler(OnValidateChild), childNode);
				}
				catch
				{
					// restore the old value
					childNode.InnerText = oldText;

					//re-throw the exception
					throw;
				}
			}
		}

        private void OnValidateChild(object sender, ValidationEventArgs e)
        {
            // throw the exception
            throw e.Exception;
        }
	}

	#endregion //XmlNodePropertyDescriptor

	#region PropertyDescriptorProvider classes

		#region PropertyDescriptorProvider abstract base class

	/// <summary>
	/// Abstract base class used to get a set of PropertyDescriptors for an object.
	/// </summary>
	public abstract class PropertyDescriptorProvider
	{
		#region Private Members

		private object _source;

		#endregion //Private Members

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="PropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="source">The underlying object that will be used to get the properties</param>
		protected PropertyDescriptorProvider(object source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			this._source = source;
		}

		#endregion //Constructors

		#region Base class overrides

			#region ToString

		/// <summary>
		/// Returns a string representation of the object
		/// </summary>
		/// <returns>The <see cref="Name"/> property</returns>
		public override string ToString()
		{
			return this.Name;
		}

			#endregion //ToString

		#endregion //Base class overrides	

        #region Events

            // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
            #region PropertyDescriptorsChanged

        /// <summary>
        /// Occurs whr
        /// </summary>
        public event EventHandler<PropertyDescriptorsChangedEventArgs> PropertyDescriptorsChanged;

        /// <summary>
        /// Raises the PropertyDescriptorsChanged event
        /// </summary>
        /// <param name="args">The event args.</param>
        protected virtual void RaisePropertyDescriptorsChangedEvent(PropertyDescriptorsChangedEventArgs args)
        {
            if (this.PropertyDescriptorsChanged != null)
                this.PropertyDescriptorsChanged(this, args);
        }

        /// <summary>
        /// Compares the old and new list of properties and raises the PropertyDescriptorsChanged event 
        /// if necessary
        /// </summary>
        /// <param name="oldProperties">The old list of properties</param>
        /// <param name="newProperties">The new list of properties</param>
        [EditorBrowsable( EditorBrowsableState.Never )] 
        protected void OnPropertiesChangedHelper(PropertyDescriptorCollection oldProperties, 
                                                 PropertyDescriptorCollection newProperties)
        {
            // if know one is listening then we can bypass all this logic
            if (this.PropertyDescriptorsChanged == null)
                return;

            // JJD 4/8/10 - TFS30619
            // Check for null
            //HashSet oldHash = new HashSet(oldProperties.Count, 0.5f, PropertyDescriptorComparer.Instance); 
            //HashSet newHash = new HashSet(newProperties.Count, 0.5f, PropertyDescriptorComparer.Instance);
            HashSet oldHash = new HashSet( oldProperties != null ? oldProperties.Count : 0, 0.5f, PropertyDescriptorComparer.Instance); 
            HashSet newHash = new HashSet( newProperties != null ? newProperties.Count : 0, 0.5f, PropertyDescriptorComparer.Instance);

            // JJD 4/8/10 - TFS30619
            // Check for null
            if ( oldProperties != null )
                oldHash.AddItems(oldProperties);

            // JJD 4/8/10 - TFS30619
            // Check for null
            if (newProperties != null )
                newHash.AddItems(newProperties);

            if (HashSet.AreEqual(oldHash, newHash))
                return;

            HashSet intersection = HashSet.GetIntersection(oldHash, newHash);

            PropertyDescriptor[] propertiesAdded = new PropertyDescriptor[newHash.Count - intersection.Count];
            PropertyDescriptor[] propertiesRemoved = new PropertyDescriptor[oldHash.Count - intersection.Count];

            int i = 0;

            // popuplate the array of properties that were added
            foreach (PropertyDescriptor pd in newHash)
            {
                if (intersection.Exists(pd))
                    continue;

                propertiesAdded[i] = pd;

                i++;
            }

            i = 0;

            // popuplate the array of properties that were removed
            foreach (PropertyDescriptor pd in oldHash)
            {
                if (intersection.Exists(pd))
                    continue;

                propertiesRemoved[i] = pd;

                i++;
            }

            List<PropertyDescriptor> propsChanged = new List<PropertyDescriptor>();

            foreach (PropertyDescriptor pd in intersection)
            {

                PropertyDescriptor oldPd = oldHash.GetEquivalentItem(pd) as PropertyDescriptor;
                PropertyDescriptor newPd = newHash.GetEquivalentItem(pd) as PropertyDescriptor;

                // if nothing of interest has changed then ignore this pd
                if (oldPd.PropertyType == newPd.PropertyType &&
                    oldPd.DisplayName == newPd.DisplayName &&
                    oldPd.IsReadOnly == newPd.IsReadOnly &&
                    oldPd.IsBrowsable == newPd.IsBrowsable)
                    continue;

                // since something has changed add this to our changed collection
                propsChanged.Add(newPd);
            }

            PropertyDescriptor[] propertiesChanged = new PropertyDescriptor[propsChanged.Count];

            propsChanged.CopyTo(propertiesChanged);

            // raise the event
            this.RaisePropertyDescriptorsChangedEvent(new PropertyDescriptorsChangedEventArgs(this, propertiesAdded, propertiesChanged, propertiesRemoved));

        }


            #endregion //PropertyDescriptorsChanged
        
        #endregion //Events	
    
		#region Properties

			#region Public Properties

                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
                #region Key

        /// <summary>
        /// Returns the Name of the provider (read-only)
        /// </summary>
        public virtual object Key
        {
            get
            {
                string name = this.Name;

                // prefer the name to use as a key
                if (name != null && name.Length > 0)
                    return name;

                return this.Source;
            }
        }

                #endregion //Key	
    
				#region Name

		/// <summary>
		/// Returns the Name of the provider (read-only)
		/// </summary>
		public abstract string Name { get; }

				#endregion //Name	

                // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
                #region RaisesPropertyDescriptorsChangedEvent

        /// <summary>
        /// Returns whether or not this provider can raise the <see cref="PropertyDescriptorsChanged"/> event
        /// </summary>
        public virtual bool RaisesPropertyDescriptorsChangedEvent { get { return false; } }

                #endregion //RaisesPropertyDescriptorsChangedEvent	
    
				#region Source

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public object Source { get { return this._source; } }

				#endregion //Source

			#endregion //Public Properties

		#endregion //Properties

		#region Methods

			#region Public Methods

				#region CreateProvider (static)

		/// <summary>
		/// Creates an instance of a PropertyDescriptorProvider class
		/// </summary>
		/// <param name="item">The source item.</param>
        /// <param name="containingList">The list that contains the item (optional).</param>
		/// <returns>An instance of a PropertyDescriptorProvider derived class that can supply a PropertyDescriptionCollection.</returns>
        /// <remarks> The optional containingList is checked to see if it implements the ITypedList interface.</remarks>
		public static PropertyDescriptorProvider CreateProvider(object item, IEnumerable containingList)
		{
			if (item != null)
			{
				DataRowView drv = item as DataRowView;

				if (drv != null)
					return new DataViewPropertyDescriptorProvider(drv.DataView);

				DataRow dr = item as DataRow;

				// SSP 8/11/09 TFS20010
				// 
				//if (dr != null)
				if ( dr != null && ( typeof( DataRow ) == dr.GetType( ) || !( containingList is ITypedList ) ) )
					return new DataTablePropertyDescriptorProvider(dr.Table);

				XmlNode node = item as XmlNode;

				if (node != null)
					return new XmlNodePropertyDescriptorProvider(node);
			}
            else
            {
                // JJD 5/27/09 - TFS17887
                // Since the item wasn't passed in see if the containingList is
                // a DataView or DataTable
                DataView dv = containingList as DataView;

                if (dv != null)
                    return new DataViewPropertyDescriptorProvider(dv);

                DataTable dt = containingList as DataTable;

                if (dt != null)
                    return new DataTablePropertyDescriptorProvider(dt);
            }

			Type itemTypeFromList;

			// JJD 6/1/07 - BR23233, BR23055, BR23285, BR23286
			// Added out parameter to get the item type from the list if it is a generic IEnumerable<T>
			ITypedList typedList = GetTypedListFromContainingList(containingList, out itemTypeFromList);

			if (typedList != null)
				return new TypedListPropertyDescriptorProvider(typedList);

			if (item != null)
				return new TypePropertyDescriptorProvider(item.GetType());

			// JJD 5/31/07 - BR23233, BR23055, BR23285, BR23286
			// If the containing list is a generic IEnumerable<T> then use T as the type
            if (itemTypeFromList != null)
            {
                // JJD 5/28/09 
                // If the type is XmlNode then return null since we don't
                // want to return the property descriptor for type XmlNode
                if (itemTypeFromList == typeof(XmlNode))
                    return null;
              
                return new TypePropertyDescriptorProvider(itemTypeFromList);
            }

            
            
            
            
            
			
            return null;
		}

				#endregion //CreateProvider

				#region GetProperties

		/// <summary>
		/// Creates a collection of property descriptors
		/// </summary>
		/// <returns>A collection of property descriptors</returns>
		public abstract PropertyDescriptorCollection GetProperties();

				#endregion //GetProperties

				#region IsCompatibleItem

		/// <summary>
		/// Determines if the passed in item is compatible with the property descriptors this provider will return
		/// </summary>
		/// <param name="item">The source item.</param>
        /// <param name="containingList">The list that contains the item (optional).</param>
		/// <returns>True if the item is compatible.</returns>
        /// <remarks> The optional containingList is checked to see if it implements the ITypedList interface.</remarks>
		public virtual bool IsCompatibleItem(object item, IEnumerable containingList)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			DataRowView drv = item as DataRowView;

			if (drv != null)
			{
				DataViewPropertyDescriptorProvider dvpd = this as DataViewPropertyDescriptorProvider;

				if (dvpd == null)
					return false;

				// JJD 7/16/07 - BR24924
				// Use the Equals method instead of the == operator because RelatedView overrides
				// Equals to reuse the same column collection which we want to take advantage of
				//return dvpd.View == drv.DataView;
				DataView dataView = dvpd.View;

				return dataView != null && dataView.Equals(drv.DataView);
			}

			DataRow dr = item as DataRow;

			if (dr != null)
			{
				DataTablePropertyDescriptorProvider dtpd = this as DataTablePropertyDescriptorProvider;

				if (dtpd == null)
					return false;

				return dtpd.Table == dr.Table;
			}

			XmlNode node = item as XmlNode;

			if (node != null)
			{
				XmlNode thisNode = this._source as XmlNode;

				if (thisNode == null)
					return false;

				IXmlSchemaInfo info1 = node.SchemaInfo;
				IXmlSchemaInfo info2 = thisNode.SchemaInfo;

				if (info1 != null ||
					 info2 != null)
					return info1 == info2;

				if ( node.GetType() != thisNode.GetType())
					return false;

				return node.LocalName == thisNode.LocalName;
			}

			ITypedList typedList;

			// JJD 10/23/07 - BR26277
			// Call the GetUnderlyingItemSource utility method which correctly handles IListSource
            //ItemCollection itemCollection = containingList as ItemCollection;

			//if (null != itemCollection)
			//    typedList = itemCollection.SourceCollection as ITypedList;
			//else
            //    typedList = containingList as ITypedList;
			typedList = DataBindingUtilities.GetUnderlyingItemSource(containingList) as ITypedList;

			if (typedList != null)
				return typedList == this._source as ITypedList;

			return item.GetType() == this._source as Type;
		}

				#endregion //IsCompatibleProvider

				#region IsCompatibleList

		/// <summary>
		/// Determines if the passed in list will return the same collection of property descriptors
		/// </summary>
		/// <returns>True if the list will return the same collection of PropertyDescriptors.</returns>
		public virtual bool IsCompatibleList(IEnumerable list)
		{
			Debug.Assert(list != null);

            // JJD 4/17/08 - added support for null objects
            //if (list == null || !(this._source is IEnumerable) )
            //    return false;
			if (list == null )
				return false;

            if ( this._source is Type )
            {
                // JJD 4/17/08 - added support for null objects
                // Call GetTypedListFromContainingList which may return an itemTypeFromList
                Type itemTypeFromList;
                ITypedList typedList = GetTypedListFromContainingList(list, out itemTypeFromList);

                // JJD 4/17/08 - added support for null objects
                // if this is a typed list then return null
                if (typedList != null)
                    return false;

                // JJD 4/17/08 - added support for null objects
                // If the underlying type matches then return true
                if (itemTypeFromList != null)
                    return Utilities.GetUnderlyingType(this._source as Type) == Utilities.GetUnderlyingType(itemTypeFromList);

                // JJD 4/17/08 - added support for null objects
                // if this is a TypePropertyDescriptorProvider and its type is object then return true here.
                TypePropertyDescriptorProvider typeProvider = this as TypePropertyDescriptorProvider;
                return ( typeProvider != null &&
                            typeProvider.Type == typeof(object));
            }
			
			// JJD 7/16/07 - BR24924
			// In the case of DataViews use the Equals method because RelatedView overrides
			// Equals to reuse the same column collection which we want to take advantage of
			DataView dv;

			if (this._source is ItemCollection)
				dv = ((ItemCollection)this._source).SourceCollection as DataView;
			else
				dv = this._source as DataView;

			if (dv != null)
			{
				DataView dvToTest;
				// JJD 10/23/07 - BR26277
				// Call the GetUnderlyingItemSource utility method which correctly handles IListSource
				//if (list is ItemCollection)
				//    dvToTest = ((ItemCollection)list).SourceCollection as DataView;
				//else
				//    dvToTest = list as DataView;
				dvToTest = DataBindingUtilities.GetUnderlyingItemSource(list) as DataView;

				return dvToTest != null && dv.Equals(dvToTest);
			}

			Type itemType;

			// JJD 6/1/07 - BR23233, BR23055, BR23285, BR23286
			// Added out parameter
			ITypedList tl1 = GetTypedListFromContainingList(list, out itemType);
			ITypedList tl2 = GetTypedListFromContainingList(this._source as IEnumerable, out itemType);

			return (tl1 != null && tl2 == tl1);
		}

				#endregion //IsCompatibleList

				#region IsCompatibleProvider

		/// <summary>
		/// Determines if the passed in provider will return the same collection of property descriptors
		/// </summary>
		/// <returns>True if the provider will return the same collection of PropertyDescriptors.</returns>
		public virtual bool IsCompatibleProvider(PropertyDescriptorProvider provider)
		{
			Debug.Assert(provider != null);

			if (provider == null)
				return false;

			return this._source == provider._source;
		}

				#endregion //IsCompatibleProvider

			#endregion //Public Methods	

			#region Internal Methods
    
				#region GetTypedListFromContainingList

		// JJD 7/16/07 - BR24924
		// Added struct to cache type info
		private struct ListTypeInfo
		{
			internal Type ListType;
			internal bool IsKnownType;
			internal Type ItemType;
		}

		// JJD 7/16/07 - BR24924
		// Added dictionary to cache type info
		static private Dictionary<Type, ListTypeInfo> g_ListTypeItemTypeMap = new Dictionary<Type, ListTypeInfo>();

		// JJD 6/1/07 
		// also return the item type
		//internal static ITypedList GetTypedListFromContainingList(IEnumerable sourceList)
        // JJD 2/6/09 - TFS13615
        // Made method public
		//internal static ITypedList GetTypedListFromContainingList(IEnumerable sourceList, out Type itemType)
        /// <summary>
        /// Gets an ITypedList from an IEnumerable if it implements the interface
        /// </summary>
        /// <param name="sourceList">The source list</param>
        /// <param name="itemType">an out parameter that returns a type if the csourcelist implements IEnumerable&lt;T&gt;</param>
        /// <returns>An ITypedList if the sourcelist implements it or null.</returns>
		public static ITypedList GetTypedListFromContainingList(IEnumerable sourceList, out Type itemType)
		{
			// JJD 10/23/07 - BR26277
			// Call the GetUnderlyingItemSource utility method which correctly handles IListSource
			#region Old code

			//ItemCollection itemCollection = sourceList as ItemCollection;

			//// JJD 6/1/07 
			//// also return the item type
			//// Initialize it to null
			//// JJD 7/16/07 - BR24924
			//// No need to init out param since it is done below
			////itemType = null;

			////if (null != itemCollection)
			////    return itemCollection.SourceCollection as ITypedList;
			////else
			////    return sourceList as ITypedList;

			//if (null != itemCollection)
			//    sourceList = itemCollection.SourceCollection;

			#endregion //Old code
			sourceList = DataBindingUtilities.GetUnderlyingItemSource(sourceList);
           
            // JJD 4/29/08
            // Check for null sourceList to prevent exception below
            if (sourceList == null)
            {
                itemType = null;
                return null;
            }

			Type listType = sourceList.GetType();

			// JJD 7/16/07 - BR24924
			// lock the static dictionary used to cache list type info 
			lock (g_ListTypeItemTypeMap)
			{
				ListTypeInfo info;

				// JJD 7/16/07 - BR24924
				// See if the type is already in the map
				if (g_ListTypeItemTypeMap.TryGetValue(listType, out info))
				{
					itemType = info.ItemType;

					if (info.IsKnownType)
						return null;

					return sourceList as ITypedList;
				}

				// JJD 7/16/07 - BR24924
				// init the list type
				info.ListType = listType;

				// JJD 6/1/07 
				// compare the name of the type so we don't unnecessarily load the System.Windows.Forms
				// assembly by referencing the type here. If the name matches then the assembly is already
				// loaded and we can call a helper method to do the BindingSource specific logic
				if (listType.FullName == "System.Windows.Forms.BindingSource")
					listType = GetListTypeFromBindingSource(sourceList);

				// JJD 6/1/07 - BR23233, BR23055, BR23285, BR23286
				// Loop over the interfaces to see if the list implements the IEnumerable<T> interface
				Type[] interfaces = listType.GetInterfaces();
				int count = interfaces.Length;

				// JJD 3/18/11 - TFS66201
				// added stack variable to hold generic type 
				Type genericType = null;

				for (int i = 0; i < count; i++)
				{
					Type interfaceType = interfaces[i];

					// JJD 7/17/07 - BR24924
					// Optimization - IsGenericType is expensive so try to avoid calling it for known interfaces
					//				if (interfaceType.IsGenericType)
					if (interfaceType != typeof(IEnumerable) &&
						 interfaceType != typeof(ICollection) &&
						 interfaceType != typeof(IList) &&
						 interfaceType != typeof(IBindingList) &&
						 interfaceType != typeof(IBindingListView) &&
						 interfaceType != typeof(ITypedList) &&
						 interfaceType.IsGenericType)
					{
						Type genericInterfaceType = interfaceType.GetGenericTypeDefinition();

						if (genericInterfaceType == typeof(IEnumerable<>))
						{
							// JJD 3/18/11 - TFS66201
							// set stack variable to hold generic type 
							// moved rest of implementation below
							genericType = interfaceType;
							break;

							
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

						}
					}
				}

				// JJD 3/18/11 - TFS66201
				// check stack variable to see if we set the generic type above
				// If null see if the list is a generic ObjectView class
				if ( genericType == null )
				{
					// compare the type name prefix instead of comparing the type
					// since we don't want to take a ref on the ObjectView<T> assembly especially
					// since it was only introduced in the 4.0 framework
					string listName = listType.Name;

					if (listName != null && 
						listName.Length > 10 &&
						listName.StartsWith("ObjectView"))
					{
						if (listType.IsGenericType)
							genericType = listType;
					}
				}

				// JJD 3/18/11 - TFS66201
				// If we have a generic type then get its arguments.
				// If it has a single generic type argument (other that object)
				// then use that type
				if (genericType != null)
				{
					Type[] types = genericType.GetGenericArguments();
					if (types != null && types.Length == 1 && types[0] != typeof(object))
					{
						// JJD 7/16/07 - BR24924
						// Update the info struct's item type property
						//itemType = types[0];
						info.ItemType = types[0];

						// If it is a known type then don't return the ITypedList implementation
						//if (DataBindingUtilities.IsKnownType(itemType))
						//    return null;
						// JJD 7/16/07 - BR24924
						// Init the info struct's IsKnownType flag
						info.IsKnownType = DataBindingUtilities.IsKnownType(info.ItemType);
					}
				}

				// JJD 7/16/07 - BR24924
				// update the out parameter
				itemType = info.ItemType;

				// JJD 7/16/07 - BR24924
				// update the list type cache
				g_ListTypeItemTypeMap.Add(info.ListType, info);

				// JJD 7/16/07 - BR24924
				// If it is a known type then don't return the ITypedList implementation
				if (info.IsKnownType)
					return null;
			}

			return sourceList as ITypedList;
		}

		// JJD 6/1/07 added
		[MethodImpl(MethodImplOptions.NoInlining)]
		private static Type GetListTypeFromBindingSource(IEnumerable containingList)
		{
			System.Windows.Forms.BindingSource bindingSource = containingList as System.Windows.Forms.BindingSource;

			if (bindingSource != null && bindingSource.List != null)
				return bindingSource.List.GetType();

			return containingList.GetType();
		}

				#endregion //GetTypedListFromContainingList

			#endregion //Internal Methods	
        
		#endregion //Methods

        // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
        #region PropertyDescriptorComparer

        internal class PropertyDescriptorComparer : IEqualityComparer
        {
            static internal PropertyDescriptorComparer Instance = new PropertyDescriptorComparer();

            #region IEqualityComparer Members

            bool IEqualityComparer.Equals(object x, object y)
            {
                PropertyDescriptor xPd = x as PropertyDescriptor;
                PropertyDescriptor yPd = y as PropertyDescriptor;

                if (xPd == yPd)
                    return true;

                if (xPd == null || yPd == null)
                    return false;

                return xPd.ComponentType == yPd.ComponentType &&
                        xPd.Name == yPd.Name;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                PropertyDescriptor pd = obj as PropertyDescriptor;

                if (pd == null)
                {
                    if (obj != null)
                        return obj.GetHashCode();

                    return 0;
                }

                return pd.Name.GetHashCode() ^ pd.ComponentType.GetHashCode();
            }

            #endregion
        }

        #endregion //PropertyDescriptorComparer
    }

		#endregion //PropertyDescriptorProvider

		#region DataTablePropertyDescriptorProvider class

	/// <summary>
	/// Class used to get a set of PropertyDescriptors for an DataTable.
	/// </summary>
	public class DataTablePropertyDescriptorProvider : PropertyDescriptorProvider
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DataTablePropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="table">The underlying object that will be used to get the properties</param>
		public DataTablePropertyDescriptorProvider(DataTable table)
			: base(table)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Public Properties

				#region Table

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public DataTable Table { get { return this.Source as DataTable; } }

				#endregion //Table

			#endregion //Public Properties

		#endregion //Properties

		#region Base class overrides

			#region GetProperties

		private PropertyDescriptorCollection _properties;

		/// <summary>
		/// Creates a collection of property descriptors
		/// </summary>
		/// <returns>A collection of property descriptors</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			if (this._properties == null)
			{
				DataTable table = this.Table;

				PropertyDescriptor[] props = new PropertyDescriptor[table.Columns.Count];

				// create a property descriptor for every column in the table
				for (int i = 0; i < table.Columns.Count; i++)
					props[i] = new DataColumnPropertyDescriptor(table.Columns[i]);

				this._properties = new PropertyDescriptorCollection(props, true);
			}

			return this._properties;

		}

			#endregion //GetProperties

			#region Name

		/// <summary>
		/// Returns the name of the provider (read-only)
		/// </summary>
		public override string Name
		{
			get { return this.Table.TableName; }
		}

			#endregion //Name

		#endregion //Base class overrides
	}

		#endregion //DataTablePropertyDescriptorProvider class

		#region DataViewPropertyDescriptorProvider class

	/// <summary>
	/// Class used to get a set of PropertyDescriptors for an DataView.
	/// </summary>
    
	
    
	public class DataViewPropertyDescriptorProvider : TypedListPropertyDescriptorProvider
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DataViewPropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="view">The underlying object that will be used to get the properties</param>
		public DataViewPropertyDescriptorProvider(DataView view)
			: base(view)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Public Properties

				#region View

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public DataView View { get { return this.Source as DataView; } }

				#endregion //View

			#endregion //Public Properties

		#endregion //Properties

        
#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)


    }

		#endregion //DataViewPropertyDescriptorProvider class

		#region TypedListPropertyDescriptorProvider class

	/// <summary>
	/// Class used to get a set of PropertyDescriptors for an ITypedList.
	/// </summary>
	public class TypedListPropertyDescriptorProvider : PropertyDescriptorProvider
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TypedListPropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="typedList">The underlying object that will be used to get the properties</param>
		public TypedListPropertyDescriptorProvider(ITypedList typedList)
			: base(typedList)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Public Properties

				#region TypedList

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public ITypedList TypedList { get { return this.Source as ITypedList; } }

				#endregion //TypedList

			#endregion //Public Properties

		#endregion //Properties

		#region Base class overrides

			#region GetProperties

		/// <summary>
		/// Creates a collection of property descriptors
		/// </summary>
		/// <returns>A collection of property descriptors</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			return this.TypedList.GetItemProperties(null);

		}

			#endregion //GetProperties

			#region Name

		/// <summary>
		/// Returns the name of the provider (read-only)
		/// </summary>
		public override string Name
		{
			get	{ return this.TypedList.GetListName(null); }
		}

			#endregion //Name

		#endregion //Base class overrides

        #region Methods

            #region Public Methods

                // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping
                #region GetProviderTree

        /// <summary>
        /// Creates and returns a tree of <see cref="ProviderTreeNode"/>s
        /// </summary>
        /// <returns>The root <see cref="ProviderTreeNode"/>.</returns>
        public ProviderTreeNode GetProviderTree()
        {
            
            //return new ProviderTreeNode(this.TypedList, this, null, null);
            return new ProviderTreeNode(this.TypedList, this, new Dictionary<PropertyDescriptorCollection,ProviderTreeNode>(PropertyDescriptorCollectionComparer.Instance), null, null);
        }

                #endregion //GetProviderTree

            #endregion //Public Methods

        #endregion //Methods

        // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
        #region ProviderTreeNode class

        /// <summary>
        /// Represents a node in the data
        /// </summary>
        public class ProviderTreeNode
        {
            #region Private Members

            private ITypedList _ancestorList;
            private ProviderTreeNode _parentNode;
            private PropertyDescriptor[] _listAccessors;
            private ReadOnlyCollection<ProviderTreeNode> _children;
            private PropertyDescriptorProvider _propertyDescriptorProvider;

            #endregion //Private Members	
    
            #region Constructor

            
            
            
            internal ProviderTreeNode(ITypedList ancestorList, PropertyDescriptorProvider propertyDescriptorProvider, Dictionary<PropertyDescriptorCollection,ProviderTreeNode> nodeMap, ProviderTreeNode parentNode, PropertyDescriptor[] listAccessors)
            {
                this._ancestorList = ancestorList;
                this._propertyDescriptorProvider = propertyDescriptorProvider;
                this._parentNode = parentNode;
                this._listAccessors = listAccessors;

                List<ProviderTreeNode> children = new List<ProviderTreeNode>();
                
                PropertyDescriptorCollection properties = this._propertyDescriptorProvider.GetProperties();

                
                // add this instance to the nodeMap
                nodeMap.Add(properties, this);

                // loop over the property descriptors looking for any properties
                // that implement IEnumerable (other than string
                foreach (PropertyDescriptor pd in properties)
                {
                    Type type = pd.PropertyType;

                    // bypass string properties
                    if (type == null || type == typeof(string))
                        continue;

                    // bypass other properties whose type does not 
                    // implement IEnumerable
                    if (!typeof(IEnumerable).IsAssignableFrom(type))
                        continue;

                    try
                    {
                        // allocate an array one bigger than the one that was passed in
                        PropertyDescriptor[] childAccessors = new PropertyDescriptor[listAccessors != null ? listAccessors.Length + 1 : 1];

                        // copy the list accessors over into the child accessor array
                        if (listAccessors != null)
                            listAccessors.CopyTo(childAccessors, 0);

                        // append this property descriptor to the end
                        // of the childAccessor array
                        childAccessors[childAccessors.Length - 1] = pd;
                        
                        ProviderTreeNodePropertyDescriptorProvider childProvider = new ProviderTreeNodePropertyDescriptorProvider(ancestorList, childAccessors, pd.Name);

                        PropertyDescriptorCollection childProperties = childProvider.GetProperties();

                        
                        // If the collection of child properties is already in the node map then bypass it since we don't want
                        // to create more than one of them
                        if (nodeMap.ContainsKey(childProperties))
                            continue;

                        
                        // add a child node to the tree
                        
                        children.Add(new ProviderTreeNode(ancestorList, childProvider, nodeMap, this, childAccessors));
                    }
                    catch (Exception e)
                    {
                        if (e != null)
                            Debug.Fail(string.Format("TypedListPropertyDescriptorProvider.Node ctor exception: {0}", e.ToString()));
                    }
                }

                this._children = new ReadOnlyCollection<ProviderTreeNode>(children);
            }

            #endregion //Constructor	
    
            #region Properties

                #region Public Properties

            /// <summary>
            /// Returns the PropertyDescriptorProvider that is used to access data for this node (Read-only)
            /// </summary>
            public PropertyDescriptorProvider PropertyDescriptorProvider
            {
                get { return this._propertyDescriptorProvider; }
            }

            /// <summary>
            /// Returns a read-only collection of child nodes.
            /// </summary>
            public ReadOnlyCollection<ProviderTreeNode> ChildNodes { get { return this._children; } }

                #endregion //Public Properties	
    
            #endregion //Properties	
    
        }

        #endregion //DataTreeNode class	
    
        // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
        #region ProviderTreeNodePropertyDescriptorProvider class

        /// <summary>
        /// Class used to get a set of PropertyDescriptors for a descendant node of a <see cref="TypedListPropertyDescriptorProvider"/>.
        /// </summary>
        public class ProviderTreeNodePropertyDescriptorProvider : PropertyDescriptorProvider
        {
            private PropertyDescriptor[] _listAccessors;
            private string _propName;

            internal ProviderTreeNodePropertyDescriptorProvider(ITypedList typedList, PropertyDescriptor[] listAccessors, string propName)
                : base(typedList)
            {
                this._listAccessors = listAccessors;
                this._propName      = propName;
            }

            #region Properties

            #region Public Properties

            #region TypedList

            /// <summary>
            /// The underlying object that provides the properties (read-only)
            /// </summary>
            public ITypedList TypedList { get { return this.Source as ITypedList; } }

            #endregion //TypedList

            #endregion //Public Properties

            #endregion //Properties

            #region Base class overrides

            #region GetProperties

            /// <summary>
            /// Creates a collection of property descriptors
            /// </summary>
            /// <returns>A collection of property descriptors</returns>
            public override PropertyDescriptorCollection GetProperties()
            {
                return this.TypedList.GetItemProperties(this._listAccessors);

            }

            #endregion //GetProperties

            #region Key

            /// <summary>
            /// Returns the key of the provider (read-only)
            /// </summary>
            public override object Key
            {
                get { return this.TypedList.GetListName(this._listAccessors); }
            }

            #endregion //Name

            #region Name

            /// <summary>
            /// Returns the name of the provider (read-only)
            /// </summary>
            public override string Name
            {
                // The reason we are returning the property name from the parent node's 
                // property descriptor is so we have a way to match up with the 
                // same propertydecriptor from the parent later on
                get { return this._propName; }
            }

            #endregion //Name

            #endregion //Base class overrides
        }

        #endregion //ProviderTreeNodePropertyDescriptorProvider class
    }

		#endregion //TypedListPropertyDescriptorProvider class

		#region TypePropertyDescriptorProvider class

	/// <summary>
	/// Class used to get a set of PropertyDescriptors for a Type.
	/// </summary>
	public class TypePropertyDescriptorProvider : PropertyDescriptorProvider
	{
        #region Members

        // JJD 5/12/09 - NA 2009 vol 2 
        // added support for CustomTypeDescriptors
        private static WeakList<TypePropertyDescriptorProvider> s_ActiveTypeProviders = new WeakList<TypePropertyDescriptorProvider>();
        private static RefreshEventHandler s_RefreshHandler = null;

        #endregion //Members	
    
		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="TypePropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="type">The underlying object that will be used to get the properties</param>
		public TypePropertyDescriptorProvider(Type type)
			: base(type)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Public Properties

				#region Type

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public Type Type { get { return this.Source as Type; } }

				#endregion //Type

			#endregion //Public Properties

		#endregion //Properties

		#region Base class overrides

			#region GetProperties

		private PropertyDescriptorCollection _properties;

		/// <summary>
		/// Creates a collection of property descriptors
		/// </summary>
		/// <returns>A collection of property descriptors</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			if (this._properties != null)
				return this._properties;

            Type type = this.Type;

            // JJD 4/17/08 - added support for null objects
            // if the type is object return the special Value property
 			//if (DataBindingUtilities.IsKnownType(type))
 			if (type == typeof(object) || DataBindingUtilities.IsKnownType(type))
			{
				this._properties = new PropertyDescriptorCollection(
                        new PropertyDescriptor[] { new ValuePropertyDescriptor(type, "Value") }, true);
				return this._properties;
			}

            // JJD 5/12/09 - NA 2009 vol 2 
            // Add to list of providers that are interested in being notified when the
            // properties ar changed
            AddToNotifyList(this);

			
			
			
			
            // JJD 04/17/08
            // if the type is nullable get the underlying type
            //return _properties = TypeDescriptor.GetProperties(this.Type);
            return _properties = TypeDescriptor.GetProperties(Utilities.GetUnderlyingType( type));
		}

			#endregion //GetProperties
        
            // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
            #region Key

        /// <summary>
        /// Returns the Name of the provider (read-only)
        /// </summary>
        public override object Key
        {
            get
            {
                return this.Source;
            }
        }

            #endregion //Key	

			#region Name

		/// <summary>
		/// Returns the name of the provider (read-only)
		/// </summary>
		public override string Name
		{
			get { return this.Type.Name; }
		}

			#endregion //Name

            // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
            #region RaisesPropertyDescriptorsChangedEvent

        /// <summary>
        /// Returns whether or not this provider can raise the <see cref="PropertyDescriptorProvider.PropertyDescriptorsChanged"/> event
        /// </summary>
        public override bool RaisesPropertyDescriptorsChangedEvent { get { return true; } }

            #endregion //RaisesPropertyDescriptorsChangedEvent	

		#endregion //Base class overrides

        #region Methods

            #region Private Methods

                // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
                #region AddToNotifyList

        private static void AddToNotifyList(TypePropertyDescriptorProvider provider)
        {
            try
            {
                Monitor.Enter(typeof(TypePropertyDescriptorProvider));

                if (s_ActiveTypeProviders.Contains(provider))
                    return;

                s_ActiveTypeProviders.Add(provider);

                // if we aren't already wire into the TypeDescriptor.Refreshed event
                // then do it now
                if (s_RefreshHandler == null)
                {
                    s_RefreshHandler = new RefreshEventHandler(OnTypeDescriptorRefreshed);
                    TypeDescriptor.Refreshed += s_RefreshHandler;
                }

            }
            finally
            {
                Monitor.Exit(typeof(TypePropertyDescriptorProvider));
            }
        }

                #endregion //AddToNotifyList	
    
                // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
                #region OnPropertiesChanged

        private void OnPropertiesChanged()
        {
            PropertyDescriptorCollection oldProperties = this._properties;

            // clear the member so a new list will get allocated on the call
            // to GetProperties below
            this._properties = null;

            PropertyDescriptorCollection newProperties = this.GetProperties();

            // call the helper method to process the properties and
            // possibly raise the PropertyDescriptorsChanged event
            this.OnPropertiesChangedHelper(oldProperties, newProperties);

        }

                #endregion //OnPropertiesChanged	
    
                // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
                #region OnTypeDescriptorRefreshed

        private static void OnTypeDescriptorRefreshed(RefreshEventArgs e)
        {
            List<TypePropertyDescriptorProvider> providersToNotity = new List<TypePropertyDescriptorProvider>();

            try
            {
                Monitor.Enter(typeof(TypePropertyDescriptorProvider));

                int countOfActiveProviders = 0;

                foreach (TypePropertyDescriptorProvider provider in s_ActiveTypeProviders)
                {
                    if (provider == null)
                        continue;

                    countOfActiveProviders++;

                    if (provider.Type == e.TypeChanged)
                        providersToNotity.Add(provider);
                }

                // if all the providers have been released then we
                // can unhokk from the event
                if (countOfActiveProviders == 0)
                {
                    if (s_RefreshHandler != null)
                    {
                        TypeDescriptor.Refreshed -= s_RefreshHandler;
                        s_RefreshHandler = null;
                    }
                    return;
                }
            }
            finally
            {
                Monitor.Exit(typeof(TypePropertyDescriptorProvider));
            }

            if (providersToNotity.Count > 0)
            {
                foreach (TypePropertyDescriptorProvider provider in providersToNotity)
                    provider.OnPropertiesChanged();
            }
        }

                #endregion //OnTypeDescriptorRefreshed	
    
            #endregion //Private Methods	
    
        #endregion //Methods
    }

		#endregion //ITypePropertyDescriptorProvider class

		#region XmlNodePropertyDescriptorProvider class

	/// <summary>
	/// Class used to get a set of PropertyDescriptors for an XmlNode.
	/// </summary>
	public class XmlNodePropertyDescriptorProvider : PropertyDescriptorProvider
	{
        // JJD 5/18/09 - added
        private HashSet _propertyDescriptorHash;

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="XmlNodePropertyDescriptorProvider"/> class
		/// </summary>
		/// <param name="node">The underlying object that will be used to get the properties</param>
		public XmlNodePropertyDescriptorProvider(XmlNode node)
			: base(node)
		{
		}

		#endregion //Constructors

		#region Properties

			#region Public Properties

				#region Node

		/// <summary>
		/// The underlying object that provides the properties (read-only)
		/// </summary>
		public XmlNode Node { get { return this.Source as XmlNode; } }

				#endregion //Node

			#endregion //Public Properties

		#endregion //Properties

		#region Base class overrides

			#region GetProperties

		private PropertyDescriptorCollection _properties;

		/// <summary>
		/// Creates a collection of property descriptors
		/// </summary>
		/// <returns>A collection of property descriptors</returns>
		public override PropertyDescriptorCollection GetProperties()
		{
			if (this._properties == null)
				this._properties = DataBindingUtilities.GetXmlNodeProperties(this.Node);

			return this._properties;
		}

			#endregion //GetProperties
    
			#region IsCompatibleProvider

		/// <summary>
		/// Determines if the passed in provider will return the same collection of property descriptors
		/// </summary>
		/// <returns>True if the provider will return the same collection of PropertyDescriptors.</returns>
		public override bool IsCompatibleProvider(PropertyDescriptorProvider provider)
		{
			XmlNodePropertyDescriptorProvider xmlProvider = provider as XmlNodePropertyDescriptorProvider;

			if (xmlProvider == null)
				return false;

			if (this.Node.GetType() != xmlProvider.Node.GetType())
				return false;

			if (this.Node.SchemaInfo != null ||
				 xmlProvider.Node.SchemaInfo != null)
				return this.Node.SchemaInfo == xmlProvider.Node.SchemaInfo;

            // JJD 5/13/09 
            // Compare the Name instead which is the fully qualified name
			//return this.Node.LocalName == xmlProvider.Node.LocalName;
			return this.Node.Name == xmlProvider.Node.Name;
		}

			#endregion //IsCompatibleProvider	
    
            // JJD 5/13/09 - added
			#region Key

		/// <summary>
		/// Returns the name of the provider (read-only)
		/// </summary>
		public override object Key
		{
			get { return this.Node.Name; }
		}

			#endregion //Name
    
			#region Name

		/// <summary>
		/// Returns the name of the provider (read-only)
		/// </summary>
		public override string Name
		{
			get { return this.Node.LocalName; }
		}

			#endregion //Name

            // JJD 5/13/09 - NA 2009 vol 2 - Cross band grouping - added
            #region RaisesPropertyDescriptorsChangedEvent

        /// <summary>
        /// Returns whether or not this provider can raise the <see cref="PropertyDescriptorProvider.PropertyDescriptorsChanged"/> event
        /// </summary>
        public override bool RaisesPropertyDescriptorsChangedEvent { get { return true; } }

            #endregion //RaisesPropertyDescriptorsChangedEvent	

		#endregion //Base class overrides

        #region Methods

            #region Public Methods

                #region MergePropertiesFromNode

        /// <summary>
        /// Merges properties from the passed in node to the exiisting set of properties.
        /// </summary>
        /// <remarks>
        /// <para class="body">Since each node doesn't necessarily contain all of the possible attributes and 
        /// child nodes that can make up its set of properties we want to add new properties as we encounter nodes.</para>
        /// <para class="note"><b>Note:</b> if properties are added then the <see cref="PropertyDescriptorProvider.PropertyDescriptorsChanged"/> event will be raised.</para>
        /// </remarks>
        /// <seealso cref="GetProperties"/>
        /// <param name="node">The node whose properties will be merged.</param>
        public void MergePropertiesFromNode(XmlNode node)
        {
            Utilities.ThrowIfNull(node, "node");

            PropertyDescriptorCollection oldProperties = this.GetProperties();
            PropertyDescriptorCollection nodeProperties = DataBindingUtilities.GetXmlNodeProperties(node);
            
            // use the cached hashset as the oldhash (optimization)
            HashSet oldHash = this._propertyDescriptorHash;

            if (oldHash == null)
            {
                oldHash = new HashSet(oldProperties.Count, 0.5f, PropertyDescriptorComparer.Instance);
                oldHash.AddItems(oldProperties);

                // cache the hashset fto optimize the comparison for the next node
                this._propertyDescriptorHash = oldHash;
            }

            HashSet newHash = new HashSet(nodeProperties.Count, 0.5f, PropertyDescriptorComparer.Instance);

            newHash.AddItems(nodeProperties);

            // if the new hash is a suset of the oldhash then we can rturn and do nothing
            if (newHash.IsSubsetOf(oldHash))
                return;

            // cache the hashset fto optimize the comparison for the next node
            this._propertyDescriptorHash = newHash;

            // get the union of the old and the new since we only add properties
            // for xmlnodes never remove them
            HashSet unionHash = HashSet.GetUnion(oldHash, newHash);

            PropertyDescriptor[] unionArray = new PropertyDescriptor[unionHash.Count];

            int i = 0;
            foreach (PropertyDescriptor pd in unionHash)
            {
                unionArray[i] = pd;
                i++;
            }

            // allocate and populate the new member collection
            this._properties = new PropertyDescriptorCollection(unionArray, true);

            this.OnPropertiesChangedHelper(oldProperties, this._properties);
        }

                #endregion //MergePropertiesFromNode	
    
            #endregion //Public Methods	
    
        #endregion //Methods
    }

		#endregion //XmlNodePropertyDescriptorProvider class

	#endregion //PropertyDescriptorProvider classes	

    // JJD 8/22/09 - NA 2009 vol 2 - Cross band grouping - added
    
    #region PropertyDescriptorCollectionComparer internal class

    internal class PropertyDescriptorCollectionComparer : IEqualityComparer<PropertyDescriptorCollection>
    {
        internal static readonly PropertyDescriptorCollectionComparer Instance = new PropertyDescriptorCollectionComparer();

        #region IEqualityComparer<PropertyDescriptorCollection> Members

        public bool Equals(PropertyDescriptorCollection x, PropertyDescriptorCollection y)
        {
            // if the collections are the same return true 
            if (x == y)
                return true;

            if (x == null || y == null)
                return false;

            int count = x.Count;

            // if the counts do't match return false
            if (count != y.Count || count == 0)
                return false;

            // if any descriptor doesn't match return false
            for (int i = 0; i < count; i++)
            {
                if (x[i] != y[i])
                    return false;
            }

            // since all of the prop descriptor were the same the collections are equal
            return true;
        }

        public int GetHashCode(PropertyDescriptorCollection obj)
        {
            // this isn't the greatest hash code but since we don't expect more than a handful of collections
            // it will work fine
            return obj.Count + 1;
        }

        #endregion
    }

    #endregion //PropertyDescriptorCollectionComparer internal class	
     
    // JJD 5/12/09 - NA 2009 vol 2 - Cross band grouping - added
    #region PropertyDescriptorsChangedEventArgs

    /// <summary>
    /// Event arguments used in the <see cref="PropertyDescriptorProvider"/>'s <see cref="PropertyDescriptorProvider.PropertyDescriptorsChanged"/> event
    /// </summary>
    public class PropertyDescriptorsChangedEventArgs : EventArgs
    {
        private PropertyDescriptorProvider _provider;
        private PropertyDescriptor[] _propertiesAdded;
        private PropertyDescriptor[] _propertiesChanged;
        private PropertyDescriptor[] _propertiesRemoved;

        internal PropertyDescriptorsChangedEventArgs(PropertyDescriptorProvider provider,
                                                    PropertyDescriptor[] propertiesAdded,
                                                    PropertyDescriptor[] propertiesChanged,
                                                    PropertyDescriptor[] propertiesRemoved)
        {
            this._provider = provider;
            this._propertiesAdded = propertiesAdded;
            this._propertiesChanged = propertiesChanged;
            this._propertiesRemoved = propertiesRemoved;
        }

        #region Properties

        /// <summary>
        /// The provider whose properies have changed
        /// </summary>
        public PropertyDescriptorProvider Provider { get { return this._provider; } }

        /// <summary>
        /// The properties that have been added
        /// </summary>
        public PropertyDescriptor[] PropertiesAdded { get { return this._propertiesAdded; } }

        /// <summary>
        /// The properties that have been changed
        /// </summary>
        public PropertyDescriptor[] PropertiesChanged { get { return this._propertiesChanged; } }

        /// <summary>
        /// The properties that have been removed
        /// </summary>
        public PropertyDescriptor[] PropertiesRemoved { get { return this._propertiesRemoved; } }

        #endregion //Properties
    }

    #endregion //PropertyDescriptorsChangedEventArgs
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