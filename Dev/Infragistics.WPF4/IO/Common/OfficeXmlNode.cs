using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

namespace Infragistics.Documents.Core
{
    /// <summary>
    /// Duplicated:
    /// Infragistics.Documents.Excel.Serialization.Excel2007.ExcelXmlNode
    /// </summary>
	internal abstract class OfficeXmlNode
    {
        #region Was on XmlElementBase

        internal const string NamespaceSeparator = "/";
		internal const string NamespaceDeclarationPrefix = "xmlns";
		internal const string NamespaceDeclarationNamespace = "http://www.w3.org/2000/xmlns/";
        
        #endregion Was on XmlElementBase

        #region Members

        private List<OfficeXmlNode> childNodes = new List<OfficeXmlNode>();
		private string name;
		private OfficeXmlDocument ownerDocument;
		private OfficeXmlNode parentNode;
        
        #endregion Members

        #region Constructor

        public OfficeXmlNode( OfficeXmlDocument ownerDocument )
		{
			this.ownerDocument = ownerDocument;
		}
        
        #endregion Constructor

		#region Methods

		public OfficeXmlNode AppendChild( OfficeXmlNode node )
		{
			this.childNodes.Add( node );
			node.parentNode = this;
			return node;
        }

        //  BF 11/15/10
        //  Added this for an easy way to add a node with a value
		public OfficeXmlNode AppendChild( OfficeXmlNode node, string value )
		{
            this.AppendChild( node );

            if ( string.IsNullOrEmpty(value) == false )
                OfficeXmlNode.AddValueNode( node as OfficeXmlElement, value );

            return node;
		}

		public string GetPrefixOfNamespace( string namespaceURI )
		{
			string prefixOfNamespaceStrict = this.GetPrefixOfNamespaceStrict( namespaceURI );

			if ( prefixOfNamespaceStrict == null )
				return string.Empty;

			return prefixOfNamespaceStrict;
		}

		private string GetPrefixOfNamespaceStrict( string namespaceURI )
		{
			OfficeXmlDocument document = this.OwnerDocument;

			if ( document == null )
				return null;

			namespaceURI = document.NameTable.Add( namespaceURI );
			OfficeXmlNode parentNode = this;

			while ( parentNode != null )
			{
				if ( parentNode.NodeType == XmlNodeType.Element )
				{
					OfficeXmlElement element = (OfficeXmlElement)parentNode;
					List<OfficeXmlAttribute> attributes = element.Attributes;

					for ( int i = 0; i < attributes.Count; i++ )
					{
						OfficeXmlAttribute attribute = attributes[ i ];

						if ( attribute.Prefix.Length == 0 )
						{
							if ( attribute.LocalName == document.strXmlns && attribute.Value == namespaceURI )
								return string.Empty;
						}
						else if ( attribute.Prefix == document.strXmlns )
						{
							if ( attribute.Value == namespaceURI )
								return attribute.LocalName;
						}
						else if ( attribute.NamespaceURI == namespaceURI )
						{
							return attribute.Prefix;
						}
					}

					if ( parentNode.NamespaceURI == namespaceURI )
						return parentNode.Prefix;

					parentNode = parentNode.ParentNode;
				}
				else if ( parentNode.NodeType == XmlNodeType.Attribute )
				{
					parentNode = ( (OfficeXmlAttribute)parentNode ).OwnerElement;
				}
				else
				{
					parentNode = parentNode.ParentNode;
				}
			}

			if ( object.Equals( document.strReservedXml, namespaceURI ) )
				return document.strXml;

			if ( object.Equals( document.strReservedXmlns, namespaceURI ) )
				return document.strXmlns;

			return null;
		}

		public virtual void WriteEnd() { }

		public void WriteNode()
		{
			this.WriteStart();
			this.WriteEnd();
		}

		public virtual void WriteStart() { }

        static internal string BuildQualifiedName( string ns, Enum element, out string localName )
        {
            localName = element.ToString();

            if ( localName.StartsWith("_") )
                localName = localName.Replace("_", string.Empty);

            if ( string.IsNullOrEmpty(ns) )
                return localName;

            string format = ns.EndsWith( OfficeXmlNode.NamespaceSeparator ) ? "{0}{1}" : "{0}/{1}";
            return string.Format(format, ns, localName);
        }

		#endregion Methods

		#region Properties

		public List<OfficeXmlNode> ChildNodes
		{
			get { return this.childNodes; }
		}

		public abstract string LocalName { get; }

		public string Name
		{
			get
			{
				if ( this.name == null )
				{
					string prefix = this.Prefix;
					string localName = this.LocalName;
					OfficeXmlDocument ownerDocument = this.OwnerDocument;

					if ( prefix.Length > 0 )
					{
						if ( localName.Length > 0 )
							this.name = ownerDocument.NameTable.Add( prefix + ":" + localName );
						else
							this.name = prefix;
					}
					else
					{
						this.name = localName;
					}
				}

				return this.name;
			}
		}

		public virtual string NamespaceURI
		{
			get { return string.Empty; }
		}

		public abstract XmlNodeType NodeType { get; }

		public OfficeXmlDocument OwnerDocument
		{
			get
			{
				if ( this.NodeType == XmlNodeType.Document )
					return (OfficeXmlDocument)this;

				return this.ownerDocument;
			}
		}

		public OfficeXmlNode ParentNode
		{
			get { return this.parentNode; }
		}

		public virtual string Prefix
		{
			get { return string.Empty; }
		}

		public virtual string Value
		{
			get { return null; }
			set { SerializationUtilities.DebugFail( "The Value can't be set on the base node." ); }
		}

		#endregion Properties

        //  IGWordStreamer
        #region Was on XmlElementBase

        #region AddAttribute
        private static void AddAttribute( OfficeXmlElement element, string prefix, string localName, string namespaceName, string value )
		{
			OfficeXmlAttribute attribute = element.OwnerDocument.CreateAttribute( prefix, localName, namespaceName );
			attribute.Value = value;
			element.Attributes.Add( attribute );
		}
        #endregion AddAttribute

        #region AddNamespaceDeclaration
        public static void AddNamespaceDeclaration( OfficeXmlElement element, string namespacePrefix, string namespaceValue )
		{
			OfficeXmlNode.AddAttribute(
				element,
				OfficeXmlNode.NamespaceDeclarationPrefix,
				namespacePrefix,
				OfficeXmlNode.NamespaceDeclarationNamespace,
				namespaceValue );
		}
        #endregion AddNamespaceDeclaration

        #region AddValueNode
        public static void AddValueNode( OfficeXmlElement element, string value )
		{
			Debug.Assert( element.ChildNodes.Count == 0, "An element cannot have a value and child nodes." );

			OfficeXmlNode valueNode = null;

            if (value.Trim().Length == 0)
            {
                bool isValidWhitespace = true;
                char[] charArray = value.ToCharArray();
                for (int i = 0; i < charArray.Length; i++)
                {
                    int charCode = (int)charArray[i];
                    if (charCode != 20 && charCode != 10 && charCode != 13 && charCode != 9)
                    {
                        isValidWhitespace = false;
                        break;
                    }
                }
           
                // If any of the characters in the value are not valid for the CreateSignificantWhitspace
                // method, just add the value as a regular text node.
                if(isValidWhitespace)
                    valueNode = element.OwnerDocument.CreateSignificantWhitespace(value);
                else
                    valueNode = element.OwnerDocument.CreateTextNode(value);
            }
            else
                valueNode = element.OwnerDocument.CreateTextNode(value);

			element.AppendChild( valueNode );
		}
        #endregion AddValueNode

		#region WriteNodeRecursive
		internal static void WriteNodeRecursive( OfficeXmlNode node )
		{
			node.WriteStart();

			foreach ( OfficeXmlNode childNode in node.ChildNodes )
            {
				OfficeXmlNode.WriteNodeRecursive( childNode );
            }

			node.WriteEnd();
		}

		#endregion WriteNodeRecursive

		#region GetXmlString



#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

		internal protected static string GetXmlString( object value, DataType dataType )
		{
			string returnedValue = OfficeXmlNode.GetXmlString( value, dataType, null, true );
			return returnedValue != null ? returnedValue : string.Empty;
		}



#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)

		protected static string GetXmlString( object value, DataType dataType, object defaultValue, bool valueIsRequired )
		{

			//  if the value is the defaultValue or the value is not provided and the value is not required, nothing should be written to the xml
			if ( valueIsRequired == false &&
				( value == null || Object.Equals( value, defaultValue ) ) )
				return null;

			// if no value was provided, use the default value
			if ( value == null )
			{
				if ( defaultValue == null )
				{
					SerializationUtilities.DebugFail( "If the value is required, the value and the default value cannot be null." );
					return null;
				}

				value = defaultValue;
			}

			Debug.Assert( defaultValue == null || value.GetType() == defaultValue.GetType(), "The default value is not of the same type as the value." );

			IFormatProvider formatProvider = System.Globalization.CultureInfo.InvariantCulture;

            string retVal = String.Empty;

			try
			{
				switch ( dataType )
				{
					//  string
					case DataType.String:
						return (string)value;

					//  int
					case DataType.Integer:
						return ( (int)value ).ToString( formatProvider );
                    
					//  bool
					case DataType.Boolean:
                        // Excel stores booleans as a '0' or '1' and not 'false' or 'true'
                        bool bValue = (bool)value;
                        return bValue ? "1" : "0";

					//  DateTime
					case DataType.DateTime:
						
						return ( (DateTime)value ).ToString( formatProvider );

					//  float
					case DataType.Float:
						return ( (float)value ).ToString( formatProvider );

					//  double
					case DataType.Double:
						return ( (double)value ).ToString( formatProvider );

					//  long
					case DataType.Long:
						return ( (long)value ).ToString( formatProvider );

					//  short
					case DataType.Short:
						return ( (short)value ).ToString( formatProvider );

					//  ushort
					case DataType.UShort:
						return ( (ushort)value ).ToString( formatProvider );

					//  uint
					case DataType.UInt:
                        return (Convert.ToUInt32(value, formatProvider).ToString(formatProvider));

					//  ulong
					case DataType.ULong:
						return ( (ulong)value ).ToString( formatProvider );

					//  byte
					case DataType.Byte:
						return ( (byte)value ).ToString( formatProvider );

					//  sbyte
					case DataType.SByte:
						return ( (sbyte)value ).ToString( formatProvider );

					//  decimal
					case DataType.Decimal:
						return ( (decimal)value ).ToString( formatProvider );

					//  object
					case DataType.Object:
						return value.ToString();

                    #region Was supported in XmlElementBase

                    ////  ST_VectorBaseType
                    //case DataType.ST_VectorBaseType:
                    //    {
                    //        Debug.Assert( Enum.IsDefined( typeof( ST_VectorBaseType ), (ST_VectorBaseType)value ), "The value is not defined." );
                    //        string retValue = ( (ST_VectorBaseType)value ).ToString();

                    //        if ( retValue.StartsWith( "_" ) )
                    //            retValue = retValue.Substring( 1 );

                    //        return retValue;
                    //    }

                    ////  ST_Visibility/ST_SheetState
                    //case DataType.ST_Visibility:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Visibility ), (ST_Visibility)(WorksheetVisibility)value ), "The value is not defined." );
                    //    return ( (ST_Visibility)(WorksheetVisibility)value ).ToString();

                    ////  ST_CalcMode
                    //case DataType.ST_CalcMode:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_CalcMode ), (ST_CalcMode)(CalculationMode)value ), "The value is not defined." );
                    //    return ( (ST_CalcMode)(CalculationMode)value ).ToString();

                    ////  ST_CalcMode
                    //case DataType.ST_RefMode:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_RefMode ), (ST_RefMode)(CellReferenceMode)value ), "The value is not defined." );
                    //    return ( (ST_RefMode)(CellReferenceMode)value ).ToString();

                    ////  ST_FontScheme
                    //case DataType.ST_FontScheme:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_FontScheme ), (ST_FontScheme)value ), "The value is not defined." );
                    //    return ((ST_FontScheme)value).ToString();

                    ////  ST_UnderlineValues
                    //case DataType.ST_UnderlineValues:
                    //    FontUnderlineStyle style = (FontUnderlineStyle)value;

                    //    // There is no concept of "AllPages" in Excel, so we should
                    //    // resolve our default
                    //    if (style == FontUnderlineStyle.AllPages)
                    //        style = FontUnderlineStyle.None;

                    //    Debug.Assert( Enum.IsDefined( typeof( ST_UnderlineValues ), (ST_UnderlineValues)value ), "The value is not defined." );

                    //    retVal = ((ST_UnderlineValues)style).ToString();
                    //    if (retVal.StartsWith("_"))
                    //        retVal = retVal.Substring(1);

                    //    return retVal;

                    ////  ST_VerticalAlignment
                    //case DataType.ST_VerticalAlignRun:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_VerticalAlignRun ), (ST_VerticalAlignRun)(FontSuperscriptSubscriptStyle)value ), "The value is not defined." );
                    //    return ((ST_VerticalAlignRun)(FontSuperscriptSubscriptStyle)value).ToString();

                    ////  ST_BorderStyle
                    //case DataType.ST_BorderStyle:
                    //    CellBorderLineStyle borderStyle = (CellBorderLineStyle)value;

                    //    // There is no concept of "AllPages" in Excel, so we should
                    //    // use Excel's default
                    //    if (borderStyle == CellBorderLineStyle.AllPages)
                    //        borderStyle = CellBorderLineStyle.None;

                    //    Debug.Assert( Enum.IsDefined( typeof( ST_BorderStyle ), (ST_BorderStyle)value ), "The value is not defined." );

                    //    retVal = ((ST_BorderStyle)borderStyle).ToString();
                    //    if (retVal.StartsWith("_"))
                    //        retVal = retVal.Substring(1);

                    //    return retVal;

                    //case DataType.ST_SheetViewType:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_SheetViewType ), (ST_SheetViewType)(WorksheetView)value ), "The value is not defined." );
                    //    return ((ST_SheetViewType)(WorksheetView)value).ToString();

                    //case DataType.ST_Pane:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Pane ), (ST_Pane)value ), "The value is not defined." );
                    //    return ((ST_Pane)value).ToString();
                    ////  ST_GradientType
                    //case DataType.ST_GradientType:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_GradientType ), (ST_GradientType)value ), "The value is not defined." );
                    //    return ((ST_GradientType)value).ToString();
                    ////  ST_BorderStyle
                    //case DataType.ST_PatternType:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_PatternType ), (ST_PatternType)(FillPatternStyle)value ), "The value is not defined." );
                    //    return ((ST_PatternType)(FillPatternStyle)value).ToString();

                    //case DataType.ST_Objects:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Objects ), (ST_Objects)(ObjectDisplayStyle)value ), "The value is not defined." );
                    //    return ((ST_Objects)(ObjectDisplayStyle)value).ToString();

                    //case DataType.ST_Links:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Links ), (ST_Links)value ), "The value is not defined." );
                    //    return ((ST_Links)value).ToString();
                    //case DataType.ST_HorizontalAlignment:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_HorizontalAlignment ), (ST_HorizontalAlignment)(HorizontalCellAlignment)value ), "The value is not defined." );
                    //    return ((ST_HorizontalAlignment)(HorizontalCellAlignment)value).ToString();
                    //case DataType.ST_VerticalAlignment:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_VerticalAlignment ), (ST_VerticalAlignment)(VerticalCellAlignment)value ), "The value is not defined." );
                    //    return ((ST_VerticalAlignment)(VerticalCellAlignment)value).ToString();

                    //case DataType.ST_CellType:
                    //    // MD 10/21/10 - TFS34398
                    //    // Since this is a common case, implement it manually. It seems to be faster than the ToString() call.
                    //    //Debug.Assert( Enum.IsDefined( typeof( ST_CellType ), (ST_CellType)value ), "The value is not defined." );
                    //    //return ((ST_CellType)value).ToString();
                    //    switch ((ST_CellType)value)
                    //    {
                    //        case ST_CellType.b:
                    //            return "b";
                    //        case ST_CellType.e:
                    //            return "e";
                    //        case ST_CellType.inlineStr:
                    //            return "inlineStr";
                    //        case ST_CellType.n:
                    //            return "n";
                    //        case ST_CellType.s:
                    //            return "s";
                    //        case ST_CellType.str:
                    //            return "str";
                    //        default:
                    //            Utilities.DebugFail("The value is not defined.");
                    //            return ((ST_CellType)value).ToString();
                    //    }

                    //case DataType.ST_Orientation:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Orientation ), (ST_Orientation)value ), "The value is not defined." );
                    //    retVal = ((ST_Orientation)value).ToString();
                    //    if (retVal.StartsWith("_"))
                    //        retVal = retVal.Substring(1);

                    //    return retVal;

                    //case DataType.ST_PageOrder:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_PageOrder ), (ST_PageOrder)(PageOrder)value ), "The value is not defined." );
                    //    return ((ST_PageOrder)(PageOrder)value).ToString();

                    //case DataType.ST_CellComments:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_CellComments ), (ST_CellComments)(PrintNotes)value ), "The value is not defined." );
                    //    return ((ST_CellComments)(PrintNotes)value).ToString();

                    //case DataType.ST_PrintError:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_PrintError ), (ST_PrintError)(PrintErrors)value ), "The value is not defined." );
                    //    return ((ST_PrintError)(PrintErrors)value).ToString();

                    //case DataType.ST_PaneState:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_PaneState ), (ST_PaneState)value ), "The value is not defined." );
                    //    return ((ST_PaneState)value).ToString();

                    //case DataType.ST_Comments:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_Comments ), (ST_Comments)(CommentDisplayStyle)value ), "The value is not defined." );
                    //    return ( (ST_Comments)(CommentDisplayStyle)value ).ToString();

                    //case DataType.ST_CellFormulaType:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_CellFormulaType ), (ST_CellFormulaType)value ), "The value is not defined." );
                    //    return ((ST_CellFormulaType)value).ToString();

                    //case DataType.ST_Guid:
                    //    //  BF 8/12/08
                    //    //  Note that Excel will choke on the GUID if it isn't enclosed
                    //    //  with curly braces, which is what the "B" format does. It also
                    //    //  wants it in uppercase.
                    //    Guid guid = (Guid)value;
                    //    return guid.ToString("B").ToUpper();

                    ////  BF 8/14/08
                    //case DataType.ST_UnsignedIntHex:
                    //    if ( (value is int) == false &&
                    //         (value is Color) == false )
                    //    {
                    //        Utilities.DebugFail( "Could not convert the specified type to ST_UnsignedIntHex." );
                    //        return Utilities.ToUnsignedIntHex(0);
                    //    }    

                    //    int unsignedIntHexValue = Utilities.ToInteger( value );
                    //    return Utilities.ToUnsignedIntHex( unsignedIntHexValue );

                    //case DataType.ST_SystemColorVal:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_SystemColorVal ), (ST_SystemColorVal)value ), "The value is not defined." );
                    //    retVal = ((ST_SystemColorVal)value).ToString();
                    //    if (retVal.StartsWith("_"))
                    //        retVal = retVal.Substring(1);

                    //    return retVal;

                    ////  BF 8/18/08
                    //case DataType.ST_EditAs:
                    //    Debug.Assert( Enum.IsDefined( typeof( ST_EditAs ), (ST_EditAs)(ShapePositioningMode)value ), "The value is not defined." );
                    //    return ( (ST_EditAs)(ShapePositioningMode)value ).ToString();

                    ////  BF 8/19/08
                    //case DataType.ST_DrawingElementId:
                    //    return value.ToString();

                    ////  BF 8/20/08
                    //case DataType.ST_Coordinate:
                    //case DataType.ST_PositiveCoordinate:
                    //    return value.ToString();

                    #endregion Was supported in XmlElementBase

					default:
						SerializationUtilities.DebugFail( string.Format( "Unhandled data type '{0}' specified in XmlElementBase.GetXmlString.", dataType ) );
						return null;
				}
			}
			catch ( Exception ex )
			{
				SerializationUtilities.DebugFail( string.Format( "Exception thrown trying to parse value '{0}' in XmlElementBase.GetXmlString ({1})", value, ex.Message ) );
				return null;
			}
		}

		#endregion GetXmlString

        #endregion Was on XmlElementBase
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