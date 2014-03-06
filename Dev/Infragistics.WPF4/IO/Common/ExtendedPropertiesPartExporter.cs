using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Infragistics.Documents.Core
{
    #region ExtendedPropertiesPartExporter class

    internal class ExtendedPropertiesPartExporter : ContentTypeExporterBase
    {
		#region Constants

        //  Duplicated from
        //  Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes.ExtendedPropertiesPart

        /// <summary>'application/vnd.openxmlformats-officedocument.extended-properties+xml'</summary>
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.extended-properties+xml";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/extended-properties'</summary>
        public const string DefaultNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";

        /// <summary>'/docProps/app.xml'</summary>
        public const string DefaultPartName = "/docProps/app.xml";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties'</summary>
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties";

        /// <summary>'vt'</summary>
		public const string VariantTypesNamespacePrefix = "vt";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes'</summary>
        public const string VariantTypesNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";

		#endregion Constants

		#region ContentType

		public override string ContentType
		{
			get { return ExtendedPropertiesPartExporter.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return ExtendedPropertiesPartExporter.RelationshipTypeValue; }
		}

		#endregion RelationshipType

		#region Save

		public override void Save( OfficeDocumentExportManager manager, Stream contentTypeStream, out bool closeStream, ref bool popCounterStack )
		{
            //  BF 1/10/11  TFS62660
            //  Keep it open; the export manager will close it upon disposal.
            //closeStream = true;
            closeStream = false;

            //  TFS67621
            popCounterStack = false;

            //  Get an ExtendedPropertiesInfo struct from the manager
            OfficeDocumentProperties docProps = manager.GetDocumentProperties();

            //  Create a new OfficeXmlDocument to which we will write the XML
            OfficeXmlDocument document = new OfficeXmlDocument( contentTypeStream, false );
            
            //  Add the Xml declaration
            OfficeXmlDeclaration xmlDec = document.CreateXmlDeclaration( ContentTypeBase.XmlVersion, ContentTypeBase.XmlEncoding, ContentTypeBase.XmlStandalone );
            document.AppendChild( xmlDec );

            OfficeXmlElement rootElement = null;
            OfficeXmlElement vectorElement = null;
            OfficeXmlElement variantElement = null;
            OfficeXmlElement lpStrElement = null;
            OfficeXmlElement i4Element = null;
            OfficeXmlAttribute sizeAttribute = null;
            OfficeXmlAttribute baseTypeAttribute = null;

            OfficeXmlElement element = null;

            #region Root element <Properties>

            //  Create the <Properties> node

            //  BF 1/7/11   TFS62660 (see below)
            //  CLR4 has a problem with adding an xmlns declaration as an attribute,
            //  so do it as a default namespace.
            //rootElement = this.CreateElement( document, ExtendedPropertiesElementType.Properties );
            rootElement = new OfficeXmlElement(
                null,
                ExtendedPropertiesElementType.Properties.ToString(),
                ExtendedPropertiesPartExporter.DefaultNamespace,
                document );

            //  Add the <Properties> node to the document
            document.AppendChild( rootElement );

            //  Add the namespace declarations, which appear as attributes
            //  of the root node

            //  BF 1/7/11   TFS62660
            //  CLR4 XmlWriter doesn't like this (see above)
            #region Obsolete code
            //OfficeXmlNode.AddNamespaceDeclaration(
            //    rootElement,
            //    null,
            //    ExtendedPropertiesPartExporter.DefaultNamespace );
            #endregion Obsolete code

			OfficeXmlNode.AddNamespaceDeclaration(
				rootElement,
				ExtendedPropertiesPartExporter.VariantTypesNamespacePrefix,
				ExtendedPropertiesPartExporter.VariantTypesNamespace );
            
            #endregion Root element <Properties>

            #region <Template>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.GetTemplate()) == false )
            {
                element = this.CreateElement( document, ExtendedPropertiesElementType.Template );
                rootElement.AppendChild( element, docProps.GetTemplate() );
            }

            #endregion <Template>

            #region <Application>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Application) == false )
            {
                element = this.CreateElement( document, ExtendedPropertiesElementType.Application );
                rootElement.AppendChild( element, docProps.Application );
            }

            #endregion <Application>

            #region <DocSecurity>

            element = this.CreateElement( document, ExtendedPropertiesElementType.DocSecurity );
            rootElement.AppendChild( element, "0" );

            #endregion <DocSecurity>

            #region <ScaleCrop>

            element = this.CreateElement( document, ExtendedPropertiesElementType.ScaleCrop );
            rootElement.AppendChild( element, OfficeXmlNode.GetXmlString(false, DataType.Boolean) );

            #endregion <ScaleCrop>

            #region <HeadingPairs> / <TitlesOfParts>

            if ( docProps != null )
            {
                string headingPairsValue = null;
                List<string> titlesOfParts = null;
                docProps.GetHeadingPairsAndTitlesOfParts( out headingPairsValue, out titlesOfParts );

                if ( string.IsNullOrEmpty(headingPairsValue) == false &&
                     titlesOfParts != null &&
                     titlesOfParts.Count > 0 )
                {
                    #region <HeadingPairs>

                    //  <HeadingPairs>
                    OfficeXmlElement headingPairsElement = this.CreateElement( document, ExtendedPropertiesElementType.HeadingPairs );
                    rootElement.AppendChild( headingPairsElement );

                    //  <vt:vector>
                    vectorElement = this.CreateElement( document, VariantElementType.vector );
                    headingPairsElement.AppendChild( vectorElement );

                    //  size attribute
                    sizeAttribute = this.CreateAttribute( document, VariantElementType.size );
                    sizeAttribute.Value = "2";
                    vectorElement.Attributes.Add( sizeAttribute );

                    //  baseType attribute
                    baseTypeAttribute = this.CreateAttribute( document, VariantElementType.baseType );
                    baseTypeAttribute.Value = VariantElementType.variant.ToString();
                    vectorElement.Attributes.Add( baseTypeAttribute );

                    //  <vt:variant>
                    variantElement = this.CreateElement( document, VariantElementType.variant );
                    vectorElement.AppendChild( variantElement );

                    //  <vt:lpStr>
                    lpStrElement = this.CreateElement( document, VariantElementType.lpstr );
                    variantElement.AppendChild( lpStrElement, headingPairsValue );

                    //  Another <vt:variant>
                    variantElement = this.CreateElement( document, VariantElementType.variant );
                    vectorElement.AppendChild( variantElement );

                    //  <vt:i4>
                    i4Element = this.CreateElement( document, VariantElementType.i4 );
                    string i4Value = OfficeXmlNode.GetXmlString( titlesOfParts.Count, DataType.Integer );
                    variantElement.AppendChild( i4Element, i4Value );

                    #endregion <HeadingPairs>

                    #region <TitlesOfParts>

                    //  <HeadingPairs>
                    OfficeXmlElement titlesOfPartsElement = this.CreateElement( document, ExtendedPropertiesElementType.TitlesOfParts );
                    rootElement.AppendChild( titlesOfPartsElement );

                    //  <vt:vector>
                    vectorElement = this.CreateElement( document, VariantElementType.vector );
                    titlesOfPartsElement.AppendChild( vectorElement );

                    //  size attribute
                    sizeAttribute = this.CreateAttribute( document, VariantElementType.size );
                    sizeAttribute.Value = OfficeXmlNode.GetXmlString(titlesOfParts.Count, DataType.Integer);
                    vectorElement.Attributes.Add( sizeAttribute );

                    //  baseType attribute
                    baseTypeAttribute = this.CreateAttribute( document, VariantElementType.baseType );
                    baseTypeAttribute.Value = VariantElementType.lpstr.ToString();
                    vectorElement.Attributes.Add( baseTypeAttribute );

                    //  <vt:lpStr>
                    for ( int i = 0; i < titlesOfParts.Count; i ++ )
                    {
                        lpStrElement = this.CreateElement( document, VariantElementType.lpstr );
                        vectorElement.AppendChild( lpStrElement, titlesOfParts[i] );
                    }

                    #endregion <TitlesOfParts>
                }
            }

            #endregion <HeadingPairs> / <TitlesOfParts>

            #region <Company>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Company) == false )
            {
                element = this.CreateElement( document, ExtendedPropertiesElementType.Company );
                rootElement.AppendChild( element, docProps.Company );
            }
            #endregion <Company>

            #region <Manager>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Manager) == false )
            {
                element = this.CreateElement( document, ExtendedPropertiesElementType.Manager );
                rootElement.AppendChild( element, docProps.Manager );
            }
            #endregion <Manager>

            #region <LinksUpToDate>

            element = this.CreateElement( document, ExtendedPropertiesElementType.LinksUpToDate );
            rootElement.AppendChild( element, OfficeXmlNode.GetXmlString(false, DataType.Boolean) );

            #endregion <LinksUpToDate>

            #region <SharedDoc>

            element = this.CreateElement( document, ExtendedPropertiesElementType.SharedDoc );
            rootElement.AppendChild( element, OfficeXmlNode.GetXmlString(false, DataType.Boolean) );

            #endregion <SharedDoc>

            #region <HyperlinksChanged>

            element = this.CreateElement( document, ExtendedPropertiesElementType.HyperlinksChanged );
            rootElement.AppendChild( element, OfficeXmlNode.GetXmlString(false, DataType.Boolean) );

            #endregion <HyperlinksChanged>

            #region <AppVersion>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.AppVersion) == false )
            {
                element = this.CreateElement( document, ExtendedPropertiesElementType.AppVersion );
                rootElement.AppendChild( element, docProps.AppVersion );
            }
            #endregion <AppVersion>

            //  Now that the ChildNodes collections are populated,
            //  call the WriteElementsToStream method to write out
            //  each element's value and attributes to the XML.
            ContentTypeExporterBase.WriteElementsToStream( document, contentTypeStream );

            //  Close the writer so the XML actually gets into the stream
            document.Writer.Close();
        }

		#endregion Save

        #region GetElementName
        
        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>,
        /// which is implied to be of type ExtendedPropertiesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        protected override string GetElementName( Enum element, out string ns, out string prefix, out string localName )
        {
            return ExtendedPropertiesPartExporter.GetElementNameHelper( element, out ns, out prefix, out localName );
        }

        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>,
        /// which is implied to be of type ExtendedPropertiesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        static internal string GetElementNameHelper( Enum element, out string ns, out string prefix, out string localName )
        {
            ns = null;
            prefix = null;
            localName = null;

            if ( (element is ExtendedPropertiesElementType) == false &&
                 (element is VariantElementType) == false )
            {
                SerializationUtilities.DebugFail( "Wrong enum type here; should be typeof(ExtendedPropertiesElementType) or typeof(VariantElementType)" );
                return string.Empty;
            }

            if ( element is ExtendedPropertiesElementType )
            {
                prefix = string.Empty;
                
                //  BF 1/7/11   TFS62660
                //  CLR4 XmlWriter enforces the way xmlns declarations appear
                //  differently, so now instead of adding them as attributes,
                //  we have to have an actual default namespace for all these
                //  elements.
                //ns = string.Empty;
                ns = ExtendedPropertiesPartExporter.DefaultNamespace;
                
                return OfficeXmlNode.BuildQualifiedName( ns, element, out localName );
            }

            VariantElementType elementType = (VariantElementType)element;

            //  Get the namespace name off the enum constant.
            switch ( elementType )
            {
                //  no prefix or namespace
                case VariantElementType.size:
                case VariantElementType.baseType:
                    prefix = string.Empty;
                    ns = string.Empty;
                    break;

                case VariantElementType.i4:
                case VariantElementType.lpstr:
                case VariantElementType.variant:
                case VariantElementType.vector:
                    prefix = ExtendedPropertiesPartExporter.VariantTypesNamespacePrefix;
                    ns = ExtendedPropertiesPartExporter.VariantTypesNamespace;
                    break;

                default:
                    SerializationUtilities.DebugFail( "Unhandled ExtendedPropertiesElementType constant." );
                    return string.Empty;
            }

            //  Build the string and return it.
            return OfficeXmlNode.BuildQualifiedName( ns, elementType, out localName );
        }
        #endregion GetElementName
    }

    #endregion ExtendedPropertiesPartExporter class
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