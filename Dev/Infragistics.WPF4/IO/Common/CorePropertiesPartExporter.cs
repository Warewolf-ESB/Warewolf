using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Infragistics.Documents.Core.Packaging;

namespace Infragistics.Documents.Core
{
    #region CorePropertiesPartExporter class

    internal class CorePropertiesPartExporter : ContentTypeExporterBase
    {
		#region Constants

        //  Duplicated from
        //  Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes.CorePropertiesPart

        /// <summary>'application/vnd.openxmlformats-package.core-properties+xml'</summary>
		public const string ContentTypeValue = "application/vnd.openxmlformats-package.core-properties+xml";

        /// <summary>'/docProps/core.xml'</summary>
        public const string DefaultPartName = "/docProps/core.xml";

        /// <summary>'http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties'</summary>
		private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties";

        /// <summary>'http://schemas.openxmlformats.org/package/2006/metadata/core-properties'</summary>
		public const string CorePropertiesNamespace = "http://schemas.openxmlformats.org/package/2006/metadata/core-properties";

        /// <summary>'http://purl.org/dc/elements/1.1'</summary>
        public const string DcNamespace = "http://purl.org/dc/elements/1.1/";

        /// <summary>'http://purl.org/dc/terms'</summary>
		public const string DctermsNamespace = "http://purl.org/dc/terms/";

        /// <summary>'http://www.w3.org/2001/XMLSchema-instance'</summary>
		public const string XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";

        /// <summary>'cp'</summary>
		public const string CorePropertiesNamespacePrefix = "cp";
		
        /// <summary>'dc'</summary>
        public const string DcNamespacePrefix = "dc";

        /// <summary>'dcterms'</summary>
		public const string DctermsNamespacePrefix = "dcterms";
		
        /// <summary>'xsi'</summary>
        public const string XsiNamespacePrefix = "xsi";

        /// <summary>'dcterms:W3CDTF'</summary>
        public const string XsiTypeAttributeValue = "dcterms:W3CDTF";

		#endregion Constants

		#region ContentType

		public override string ContentType
		{
			get { return CorePropertiesPartExporter.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return CorePropertiesPartExporter.RelationshipTypeValue; }
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

            //  Get a CorePropertiesInfo struct from the manager
            OfficeDocumentProperties docProps = manager.GetDocumentProperties();

            //  Create a new OfficeXmlDocument to which we will write the XML
            OfficeXmlDocument document = new OfficeXmlDocument( contentTypeStream, false );
            
            //  Add the Xml declaration
            OfficeXmlDeclaration xmlDec = document.CreateXmlDeclaration( ContentTypeBase.XmlVersion, ContentTypeBase.XmlEncoding, ContentTypeBase.XmlStandalone );
            document.AppendChild( xmlDec );

            string now = PackageUtilities.DateTimeToW3CDTFValue( DateTime.Now );
            OfficeXmlElement rootElement = null;
            OfficeXmlElement element = null;
            OfficeXmlAttribute attribute = null;

            #region Root element <cp:coreProperties>

            //  Create the <cp:coreProperties> node
            rootElement = this.CreateElement( document, CorePropertiesElementType.coreProperties );

            //  Add the <cp:coreProperties> node to the document
            document.AppendChild( rootElement );

            //  Add the namespace declarations, which appear as attributes
            //  of the root node
			OfficeXmlNode.AddNamespaceDeclaration(
				rootElement,
				CorePropertiesPartExporter.DcNamespacePrefix,
				CorePropertiesPartExporter.DcNamespace );

			OfficeXmlNode.AddNamespaceDeclaration(
				rootElement,
				CorePropertiesPartExporter.DctermsNamespacePrefix,
				CorePropertiesPartExporter.DctermsNamespace );

			OfficeXmlNode.AddNamespaceDeclaration(
				rootElement,
				CorePropertiesPartExporter.XsiNamespacePrefix,
				CorePropertiesPartExporter.XsiNamespace );
            
			OfficeXmlNode.AddNamespaceDeclaration(
				rootElement,
				CorePropertiesPartExporter.CorePropertiesNamespacePrefix,
				CorePropertiesPartExporter.CorePropertiesNamespace );
            
            #endregion Root element <cp:coreProperties>

            #region <dc:title>

            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Title) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.title );
                rootElement.AppendChild( element, docProps.Title);
            }

            #endregion <dc:title>

            #region <dc:subject>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Subject) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.subject );
                rootElement.AppendChild( element, docProps.Subject );
            }

            #endregion <dc:subject>

            #region <dc:creator>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Author) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.creator );
                rootElement.AppendChild( element, docProps.Author );
            }

            #endregion <dc:creator>

            #region <cp:keywords>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Keywords) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.keywords );
                rootElement.AppendChild( element, docProps.Keywords );
            }

            #endregion <cp:keywords>

            #region <dc:description>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Comments) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.description );
                rootElement.AppendChild( element, docProps.Comments );
            }

            #endregion <dc:description>

            #region <dcterms:created>
            
            //  Create the <dcterms:created> element
            element = this.CreateElement( document, CorePropertiesElementType.created );

            //  Create the <xsi:type> attribute
            attribute = this.CreateAttribute( document, CorePropertiesElementType.type );
            attribute.Value = CorePropertiesPartExporter.XsiTypeAttributeValue;
            
            //  Add the attribute, then add the element
            element.Attributes.Add( attribute );
            rootElement.AppendChild( element, now );

            #endregion <dcterms:created>

            #region <dcterms:modified>
            
            //  Create the <dcterms:modified> element
            element = this.CreateElement( document, CorePropertiesElementType.modified );

            //  Create the <xsi:type> attribute
            attribute = this.CreateAttribute( document, CorePropertiesElementType.type );
            attribute.Value = CorePropertiesPartExporter.XsiTypeAttributeValue;

            //  Add the attribute, then add the element
            element.Attributes.Add( attribute );
            rootElement.AppendChild( element, now );

            #endregion <dcterms:modified>

            #region <cp:category>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Category) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.category );
                rootElement.AppendChild( element, docProps.Category );
            }

            #endregion <cp:category>

            #region <cp:contentStatus>
            
            if ( docProps != null &&
                 string.IsNullOrEmpty(docProps.Status) == false )
            {
                element = this.CreateElement( document, CorePropertiesElementType.contentStatus );
                rootElement.AppendChild( element, docProps.Status );
            }

            #endregion <cp:contentStatus>

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
        /// which is implied to be of type CorePropertiesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        protected override string GetElementName( Enum element, out string ns, out string prefix, out string localName )
        {
            return CorePropertiesPartExporter.GetElementNameHelper( element, out ns, out prefix, out localName );
        }

        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>,
        /// which is implied to be of type CorePropertiesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        static internal string GetElementNameHelper( Enum element, out string ns, out string prefix, out string localName )
        {
            ns = null;
            prefix = null;
            localName = null;

            if ( (element is CorePropertiesElementType) == false )
            {
                SerializationUtilities.DebugFail( "Wrong enum type here; should be typeof(CorePropertiesElementType)" );
                return string.Empty;
            }

            CorePropertiesElementType elementType = (CorePropertiesElementType)element;

            //  Get the namespace name off the enum constant.
            switch ( elementType )
            {
                //  prefix = 'dc'
                case CorePropertiesElementType.title:
                case CorePropertiesElementType.subject:
                case CorePropertiesElementType.creator:
                case CorePropertiesElementType.description:
                    prefix = CorePropertiesPartExporter.DcNamespacePrefix;
                    ns = CorePropertiesPartExporter.DcNamespace;
                    break;

                //  prefix = 'dcterms'
                case CorePropertiesElementType.created:
                case CorePropertiesElementType.modified:
                    prefix = CorePropertiesPartExporter.DctermsNamespacePrefix;
                    ns = CorePropertiesPartExporter.DctermsNamespace;
                    break;

                //  prefix = 'cp'
                case CorePropertiesElementType.coreProperties:
                case CorePropertiesElementType.keywords:
                case CorePropertiesElementType.lastModifiedBy:
                case CorePropertiesElementType.category:
                case CorePropertiesElementType.contentStatus:
                    prefix = CorePropertiesPartExporter.CorePropertiesNamespacePrefix;
                    ns = CorePropertiesPartExporter.CorePropertiesNamespace;
                    break;

                //  Attributes
                //  prefix = 'xsi'
                case CorePropertiesElementType.type:
                    prefix = CorePropertiesPartExporter.XsiNamespacePrefix;
                    ns = CorePropertiesPartExporter.XsiNamespace;
                    break;

                default:
                    SerializationUtilities.DebugFail( "Unhandled CorePropertiesElementType constant." );
                    return string.Empty;
            }

            //  Build the string and return it.
            return OfficeXmlNode.BuildQualifiedName( ns, elementType, out localName );
        }
        #endregion GetElementName
    }

    #endregion CorePropertiesPartExporter class
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