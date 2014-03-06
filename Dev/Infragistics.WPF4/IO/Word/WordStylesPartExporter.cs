using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;
using Infragistics.Documents.Core;
using WORD = Infragistics.Documents.Word;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{
    #region WordStylesPartExporter class

    internal class WordStylesPartExporter : WordCustomizablePartExporterBase
    {
		#region Constants

        /// <summary>'vnd-openxmlformats.officedocument.wordprocessingml.styles+xml'</summary>
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml";

        /// <summary>'/word/styles.xml'</summary>
        public const string DefaultPartName = "/word/styles.xml";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles'</summary>
		private const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles";

        internal const string TableStyleType = "table";
        internal const string TableStyleId = "TableNormal";

        static internal Color DefaultHyperlinkColor = Color.FromArgb(255, 0, 0, 255);

		#endregion Constants

        #region Member variables

        private bool hasHyperlinks = false;
        private WORD.Font defaultFont = null;
        private ParagraphProperties defaultParagraphProperties = null;
        private DefaultTableProperties defaultTableProperties = null;

        #endregion Member variables

        #region Constructor
        internal WordStylesPartExporter(
            bool hasHyperlinks,
            WORD.Font defaultFont,
            ParagraphProperties defaultParagraphProperties,
            DefaultTableProperties defaultTableProperties)
        {
            this.hasHyperlinks = hasHyperlinks;
            this.defaultFont = defaultFont;
            this.defaultParagraphProperties = defaultParagraphProperties;
            this.defaultTableProperties = defaultTableProperties;
        }
        #endregion Constructor

        #region ContentType

        public override string ContentType
		{
			get { return WordStylesPartExporter.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return WordStylesPartExporter.RelationshipTypeValue; }
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

            ////  Create the writer, write the XML header
            //this.textWriter = XmlWriter.Create( contentTypeStream );
            //string declaration = SerializationUtilities.GetXmlDeclaration( ContentTypeBase.XmlVersion, ContentTypeBase.XmlEncoding, ContentTypeBase.XmlStandalone );
            //this.textWriter.WriteProcessingInstruction( ContentTypeBase.XmlName, declaration );

            this.CreateXmlWriter( contentTypeStream, true );

            //  Add the <w:styles> element
            this.WriteStartElement( WordStylesElementType.styles );

            //  Add the namespace declarations
            this.AddNamespaceDeclarations();

            #region <w:docDefaults>

            //  Start <w:docDefaults>, <w:rPrDefault>, <w:rPr> elements
            this.WriteStartElement( WordStylesElementType.docDefaults );

            #region <w:rPrDefault>

            this.WriteStartElement( WordStylesElementType.rPrDefault );
            this.WriteStartElement( WordProcessingMLElementType.rPr );

            WordDocumentProperties docProps = manager.GetDocumentProperties() as WordDocumentProperties;
            
            //  TFS63993
            Font defaultFont = new Font(null);
            if ( this.defaultFont != null )
                Font.MergeProperties( this.defaultFont, ref defaultFont );

            if ( defaultFont.ShouldSerialize(Font.PropertyIds.Size) == false )
                defaultFont.Size = 11f;

            this.AddRunProperties( defaultFont, null, RunStyle.None, docProps, null );

            //  Close <w:docDefaults>, <w:rPrDefault>, <w:rPr> elements
            this.WriteEndElement( WordProcessingMLElementType.rPr );
            this.WriteEndElement( WordStylesElementType.rPrDefault );
            
            #endregion </w:rPrDefault>

            #region <w:pPrDefault>

            this.WriteStartElement( WordStylesElementType.pPrDefault );
            
            this.WriteParagraphProperties( this.defaultParagraphProperties );
            
            this.WriteEndElement( WordStylesElementType.pPrDefault );
            
            #endregion </w:pPrDefault>

            this.WriteEndElement( WordStylesElementType.docDefaults );

            #endregion <w:docDefaults>

            #region <w:style> - TableNormal

            this.AddTableStyle( WordStylesPartExporter.TableStyleType, WordStylesPartExporter.TableStyleId, true, this.defaultTableProperties );
            
            #endregion </w:style> - TableNormal

            #region Hyperlinks
            if ( this.hasHyperlinks )
            {
                //  <w:style>
                this.WriteStartElement( WordStylesElementType.style );

                //  <w:type>, <w:styleId>
                this.WriteAttributeString( WordStylesAttributeType.type, "character" );
                this.WriteAttributeString( WordStylesAttributeType.styleId, RunStyle.Hyperlink.ToString() );
                
                //  <w:name>
                this.WriteElementString( WordStylesElementType.name, null, RunStyle.Hyperlink.ToString() );

                //  Font attributes
                WORD.Font font = new WORD.Font(null);
                font.Underline = Underline.Single;
                font.ForeColor = WordStylesPartExporter.DefaultHyperlinkColor;
                this.AddRunProperties( font, false, RunStyle.None, null, null );

                //  </w:style>
                this.WriteEndElement( WordStylesElementType.style );
            }
            #endregion Hyperlinks

            //  Close the </w:styles> element
            this.WriteEndElement( WordStylesElementType.styles );

            //  Close the writer
            this.textWriter.Close();
        }

		#endregion Save

        #region GetElementName
        
        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>,
        /// which is implied to be of type StylesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        protected override string GetElementName( Enum element, out string ns, out string prefix, out string localName )
        {
            return WordStylesPartExporter.GetElementNameHelper( element, out ns, out prefix, out localName );
        }

        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>,
        /// which is implied to be of type StylesElementType.
        /// The method knows by the enum constant which namespace it
        /// belongs to, and the local name of the element.
        /// </summary>
        /// <returns>The fully qualified name of the XML element.</returns>
        static internal string GetElementNameHelper( Enum element, out string ns, out string prefix, out string localName )
        {
            ns = null;
            prefix = null;
            localName = null;

            if ( element is WordStylesElementType ||
                 element is WordStylesAttributeType )
            {
                prefix = WordDocumentPartExporter.DefaultXmlNamespacePrefix;
                ns = WordDocumentPartExporter.DefaultXmlNamespace;
            }
            else
            if ( element is WordProcessingMLElementType ||
                 element is WordProcessingMLAttributeType ||
                 element is WordFontElementType )
                return WordDocumentPartExporter.GetElementNameHelper( element, out ns, out prefix, out localName );

            //  Build the string and return it.
            return OfficeXmlNode.BuildQualifiedName( ns, element, out localName );
        }
        #endregion GetElementName

        #region AddNamespaceDeclarations
        private void AddNamespaceDeclarations()
        {
            Dictionary<string, string> namespaceDeclarations = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
            namespaceDeclarations.Add( WordDocumentPartExporter.mcNamespacePrefix, WordDocumentPartExporter.mcNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.RelationshipsNamespacePrefix, WordDocumentPartExporter.RelationshipsNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.DefaultXmlNamespacePrefix, WordDocumentPartExporter.DefaultXmlNamespace );

            base.AddNamespaceDeclarations( namespaceDeclarations );
        }
        #endregion AddNamespaceDeclarations

    }
    #endregion WordStylesPartExporter class
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