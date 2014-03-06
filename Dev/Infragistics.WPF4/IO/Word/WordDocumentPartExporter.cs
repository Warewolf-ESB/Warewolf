using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Xml;

using Infragistics.Documents.Core;
using Infragistics.Documents.Core.Packaging;
using WORD = Infragistics.Documents.Word;


using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;






using Image = System.Windows.Media.Imaging.BitmapSource;


namespace Infragistics.Documents.Word
{
    #region WordDocumentPartExporter class

    internal class WordDocumentPartExporter : WordCustomizablePartExporterBase
    {
		#region Constants

        /// <summary>Returns char #32</summary>
        public const string SpaceChar = " ";

        /// <summary>'application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml'</summary>
		public const string ContentTypeValue = "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml";

        /// <summary>'/word/document.xml'</summary>
		public const string DefaultPartName = "/word/document.xml";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument'</summary>
		public const string RelationshipTypeValue = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";

        /// <summary>
        /// 'http://www.w3.org/XML/1998/namespace'
        /// </summary>
        public const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

        /// <summary>
        /// 'w'
        /// </summary>
        public const string DefaultXmlNamespacePrefix = "w";

        /// <summary>
        /// 'http://schemas.openxmlformats.org/wordprocessingml/2006/main'
        /// </summary>
        public const string DefaultXmlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";

        /// <summary>'http://schemas.openxmlformats.org/officeDocument/2006/relationships'</summary>
		public const string RelationshipsNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
		
        /// <summary>'r'</summary>
        public const string RelationshipsNamespacePrefix = "r";

        //  It seemed more identifiable to base the naming for these things on the prefix
        internal const string wpcNamespacePrefix = "wpc";
        internal const string wpcNamespace = "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas";

        internal const string mcNamespacePrefix = "mc";
        internal const string mcNamespace = "http://schemas.openxmlformats.org/markup-compatibility/2006";

        internal const string oNamespacePrefix = "o";
        internal const string oNamespace = "urn:schemas-microsoft-com:office:office";

        internal const string mNamespacePrefix = "m";
        internal const string mNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/math";

        internal const string vNamespacePrefix = "v";
        internal const string vNamespace = "urn:schemas-microsoft-com:vml";

        internal const string wp14NamespacePrefix = "wp14";
        internal const string wp14Namespace = "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing";

        internal const string wpNamespacePrefix = "wp";
        internal const string wpNamespace = "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";

        internal const string w10NamespacePrefix = "w10";
        internal const string w10Namespace = "urn:schemas-microsoft-com:office:word";

        internal const string w14NamespacePrefix = "w14";
        internal const string w14Namespace = "http://schemas.microsoft.com/office/word/2010/wordml";

        internal const string wpgNamespacePrefix = "wpg";
        internal const string wpgNamespace = "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup";

        internal const string wpiNamespacePrefix = "wpi";
        internal const string wpiNamespace = "http://schemas.microsoft.com/office/word/2010/wordprocessingInk";

        internal const string wneNamespacePrefix = "wne";
        internal const string wneNamespace = "http://schemas.microsoft.com/office/word/2006/wordml";

        internal const string wpsNamespacePrefix = "wps";
        internal const string wpsNamespace = "http://schemas.microsoft.com/office/word/2010/wordprocessingShape";

        internal const string IgnorableAttributeLocalName = "Ignorable";
        
        internal const string IgnorableAttributeName =
            WordDocumentPartExporter.mcNamespace +
            OfficeXmlNode.NamespaceSeparator +
            WordDocumentPartExporter.IgnorableAttributeLocalName;
        
        internal const string IgnorableAttributeValue = "w14 wp14";

        internal const string SpaceAttributeValue = "preserve";
        
        //internal const string AsciiThemeAttributeValue = "minorHAnsi";
        //internal const string EastAsiaThemeAttributeValue = "minorEastAsia";
        //internal const string HAnsiThemeAttributeValue = WordDocumentPartExporter.AsciiThemeAttributeValue;
        //internal const string ComplexScriptThemeAttributeValue = "minorBidi";

        internal const string DrawingMLNamespacePrefix = "a";
        internal const string DrawingMLNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";

        internal const string DrawingMLPictureNamespacePrefix = "pic";
        internal const string DrawingMLPictureNamespace = "http://schemas.openxmlformats.org/drawingml/2006/picture";

        internal const string UriAttributeValue = "http://schemas.openxmlformats.org/drawingml/2006/picture";
        internal const string RectAttributeValue = "rect";
        internal const string RelativeHeightAttributeValue = "251658240";

        #endregion Constants

        #region Member variables
        
        private WordDocumentExportManager exportManager = null;
        Stack<PartRelationshipCounter> partRelationshipCounters = new Stack<PartRelationshipCounter>();
        private int nextHeaderPartNumber = 1;
        private int nextFooterPartNumber = 1;

		// AS 2/15/11
		// Added caching of the element name info.
		//
		[ThreadStatic]
		private static Dictionary<Enum, ElementNameCache> _elementNameTable;

        //  BF 3/8/11   TFS67979
        private Dictionary<ImageCacheInfo, PackagePartCache> imageCache = null;

        private TableBorderProperties           defaultTableBorderProperties = null;
        private Stack<TableBorderProperties>    tableBorderProperties = null;




        #endregion Member variables

        #region Constructor
        internal WordDocumentPartExporter( WordDocumentExportManager exportManager ) : base()
        {
            this.exportManager = exportManager;
        }
        #endregion Constructor

        #region PartRelationshipCounters
        internal Stack<PartRelationshipCounter> PartRelationshipCounters
        {
            get { return this.partRelationshipCounters; }
        }
        #endregion PartRelationshipCounters

        #region ContentType

        public override string ContentType
		{
			get { return WordDocumentPartExporter.ContentTypeValue; }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { return WordDocumentPartExporter.RelationshipTypeValue; }
		}

		#endregion RelationshipType

        //  BF 3/8/11   TFS67979
        #region ImageCache

        internal Dictionary<ImageCacheInfo, PackagePartCache> ImageCache
        {
            get
            {
                if ( this.imageCache == null )
                    this.imageCache = new Dictionary<ImageCacheInfo,PackagePartCache>();

                return this.imageCache;
            }
        }

        #endregion ImageCache

        //  BF 3/14/11
        #region DefaultTableCellBorderProperties
        internal override TableBorderProperties DefaultTableCellBorderProperties
        {
            get
            {
                TableBorderProperties currentProps = this.tableBorderProperties != null && this.tableBorderProperties.Count > 0 ? this.tableBorderProperties.Peek() : null;
                TableBorderProperties defaultBorderProps = this.DefaultTableBorderProperties;
                TableBorderProperties.Merge( defaultBorderProps, currentProps );
                return currentProps != null ? currentProps : defaultBorderProps;
            }
        }
        #endregion DefaultTableCellBorderProperties

        //  BF 3/14/11
        #region DefaultTableBorderProperties
        internal override TableBorderProperties DefaultTableBorderProperties
        {
            get
            {
                if ( this.defaultTableBorderProperties == null )
                {
                    this.defaultTableBorderProperties = new TableBorderProperties(null);
                    this.defaultTableBorderProperties.Style = WordUtilities.DefaultTableBorderStyle;
                }

                DefaultTableProperties defaultTableProps = this.exportManager.GetDefaultTableProperties();
                TableBorderProperties defaultBorderProps = defaultTableProps != null && defaultTableProps.ShouldSerializeBorderProperties() ? defaultTableProps.BorderProperties : null;

                if ( defaultBorderProps == null )
                    defaultBorderProps = this.defaultTableBorderProperties;
                else
                    TableBorderProperties.Merge( this.defaultTableBorderProperties, defaultBorderProps );

                return defaultBorderProps;
            }
        }
        #endregion DefaultTableBorderProperties

        #region ImageEncoder


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        #endregion ImageEncoder

        #region Save

        public override void Save( OfficeDocumentExportManager manager, Stream contentTypeStream, out bool closeStream, ref bool popCounterStack )
		{
            //  This method is called by the part creation logic.
            //  We set 'closeStream' to false to signify that the
            //  stream is to be kept open, and cache a reference to the
            //  stream; that is what we write the data directly to.

            closeStream = false;

            //  TFS67621
            popCounterStack = false;

            this.contentStream = contentTypeStream;
        }

		#endregion Save

        #region GetElementName
        
        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>.
        /// </summary>
        protected override string GetElementName( Enum element, out string ns, out string prefix, out string localName )
        {
            return WordDocumentPartExporter.GetElementNameHelper( element, out ns, out prefix, out localName );
        }

        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>.
        /// </summary>
        static internal string GetElementNameHelper( Enum element, out string ns, out string prefix, out string localName )
        {
			if (_elementNameTable == null)
				_elementNameTable = new Dictionary<Enum, ElementNameCache>();

			// AS 2/15/11
			// Added caching of the element name info.
			//
			ElementNameCache cache;
			if (_elementNameTable.TryGetValue(element, out cache))
			{
				ns = cache.ns;
				prefix = cache.prefix;
				localName = cache.localName;
				return cache.qualifiedName;
			}

            ns = null;
            prefix = null;
            localName = null;

            #region WordProcessingMLElementType / WordFontElementType
            if ( element is WordProcessingMLElementType ||
                 element is WordProcessingMLAttributeType ||
                 element is WordFontElementType )
            {
                prefix = WordDocumentPartExporter.DefaultXmlNamespacePrefix;
                ns = WordDocumentPartExporter.DefaultXmlNamespace;
            }
            #endregion WordProcessingMLElementType / WordFontElementType
            else
            #region XmlElementType
            if ( element is XmlElementType )
            {
                prefix = WordDocumentPartExporter.XmlName;
                ns = WordDocumentPartExporter.XmlNamespace;
            }
            #endregion XmlElementType
            else
            #region RelationshipsElementType
            if ( element is RelationshipsElementType ||
                 element is RelationshipsAttributeType )
            {
                prefix = WordDocumentPartExporter.RelationshipsNamespacePrefix;
                ns = WordDocumentPartExporter.RelationshipsNamespace;
            }
            #endregion RelationshipsElementType
            else
            #region WordProcessingDrawingElementType
            if ( element is WordProcessingDrawingElementType )
            {
                prefix = WordDocumentPartExporter.wpNamespacePrefix;
                ns = WordDocumentPartExporter.wpNamespace;
            }
            #endregion WordProcessingDrawingElementType
            else
            #region WordProcessingDrawingAttributeType
            if ( element is WordProcessingDrawingAttributeType ||
                 element is DrawingMLAttributeType )
            {
                prefix = null;
                ns = null;
            }
            #endregion WordProcessingDrawingAttributeType
            else
            #region DrawingMLElementType
            if ( element is DrawingMLElementType )
            {
                prefix = WordDocumentPartExporter.DrawingMLNamespacePrefix;
                ns = WordDocumentPartExporter.DrawingMLNamespace;
            }
            #endregion DrawingMLElementType
            else
            #region DrawingMLPicElementType
            if ( element is DrawingMLPicElementType )
            {
                prefix = WordDocumentPartExporter.DrawingMLPictureNamespacePrefix;
                ns = WordDocumentPartExporter.DrawingMLPictureNamespace;
            }
            #endregion DrawingMLPicElementType
            else
            #region VmlElementType
            if ( element is VmlElementType )
            {
                prefix = WordDocumentPartExporter.vNamespacePrefix;
                ns = WordDocumentPartExporter.vNamespace;
            }
            #endregion VmlElementType
            else
            #region VmlAttributeType
            if ( element is VmlAttributeType )
            {
                VmlAttributeType vmlAttributeTypeElement = (VmlAttributeType)element;
                switch ( vmlAttributeTypeElement )
                {
                    case VmlAttributeType.ext:
                        prefix = WordDocumentPartExporter.vNamespacePrefix;
                        ns = WordDocumentPartExporter.vNamespace;
                        break;

                    default:
                        prefix = null;
                        ns = null;
                        break;
                }
            }
            #endregion VmlAttributeType
            else
            #region OfficeElementType / OfficeAttributeType 
            if ( element is OfficeElementType ||
                 element is OfficeAttributeType )
            {
                prefix = WordDocumentPartExporter.oNamespacePrefix;
                ns = WordDocumentPartExporter.oNamespace;
            }
            #endregion OfficeElementType / OfficeAttributeType 
            else
            #region OfficeWordElementType
            if ( element is OfficeWordElementType )
            {
                prefix = WordDocumentPartExporter.w10NamespacePrefix;
                ns = WordDocumentPartExporter.w10Namespace;
            }
            #endregion OfficeWordElementType
            else
            #region OfficeWordAttributeType
            if ( element is OfficeWordAttributeType )
            {
                prefix = null;
                ns = null;
            }
            #endregion OfficeWordAttributeType
            else
                CommonUtilities.DebugFail( string.Format("Unrecognized enum type '{0}' in GetElementNameHelper", element) );

            //  Build the string and return it.
            string qualifiedName = OfficeXmlNode.BuildQualifiedName( ns, element, out localName );

			// AS 2/15/11
			// Added caching of the element name info.
			//
			cache = new ElementNameCache();
			cache.ns = ns;
			cache.localName = localName;
			cache.prefix = prefix;
			cache.qualifiedName = qualifiedName;
			_elementNameTable[element] = cache;

			return qualifiedName;
        }
        #endregion GetElementName

        #region Streaming support

        #region AddNamespaceDeclarations
        protected void AddNamespaceDeclarations()
        {
            Dictionary<string, string> namespaceDeclarations = new Dictionary<string,string>(StringComparer.InvariantCultureIgnoreCase);
            namespaceDeclarations.Add( WordDocumentPartExporter.wpcNamespacePrefix, WordDocumentPartExporter.wpcNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.mcNamespacePrefix, WordDocumentPartExporter.mcNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.oNamespacePrefix, WordDocumentPartExporter.oNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.RelationshipsNamespacePrefix, WordDocumentPartExporter.RelationshipsNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.mNamespacePrefix, WordDocumentPartExporter.mNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.vNamespacePrefix, WordDocumentPartExporter.vNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wp14NamespacePrefix, WordDocumentPartExporter.wp14Namespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wpNamespacePrefix, WordDocumentPartExporter.wpNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.w10NamespacePrefix, WordDocumentPartExporter.w10Namespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.DefaultXmlNamespacePrefix, WordDocumentPartExporter.DefaultXmlNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wpgNamespacePrefix, WordDocumentPartExporter.wpgNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wpiNamespacePrefix, WordDocumentPartExporter.wpiNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wneNamespacePrefix, WordDocumentPartExporter.wneNamespace );
            namespaceDeclarations.Add( WordDocumentPartExporter.wpsNamespacePrefix, WordDocumentPartExporter.wpsNamespace );

            foreach( KeyValuePair<string, string> pair in namespaceDeclarations )
            {
                this.textWriter.WriteAttributeString(
                    OfficeXmlNode.NamespaceDeclarationPrefix,
                    pair.Key,
                    OfficeXmlNode.NamespaceDeclarationNamespace,
                    pair.Value );
            }
        }
        #endregion AddNamespaceDeclarations

        #region ContentStream
        /// <summary>
        /// Returns the stream to which the document content is written.
        /// </summary>
        public Stream ContentStream { get { return this.contentStream; } }
        #endregion ContentStream

        #region WriteState

        /// <summary>
        /// Returns the write state of the XMLWriter.
        /// Returns WriteState.Start if the XMLWriter has not yet been created.
        /// </summary>
        internal WriteState WriteState { get { return this.textWriter != null ? this.textWriter.WriteState : WriteState.Start; } }

        #endregion WriteState
       
        #region StartDocument/EndDocument

        public virtual WriteState StartDocument()
        {
            //  We can't prevent that many mistakes, but this one we can.
            if ( this.lastElementStarted is WordProcessingMLElementType &&
                 (WordProcessingMLElementType)this.lastElementStarted == WordProcessingMLElementType.document )
                return this.WriteState;

            this.CreateXmlWriter( this.contentStream, true );

            this.VerifyDocumentWriteState();

            //  <w:document>
            this.WriteStartElement( WordProcessingMLElementType.document );

            //  Add the ns declarations for the <document> element
            this.AddNamespaceDeclarations();

            //  <w:body>
            this.WriteStartElement( WordProcessingMLElementType.body );

            return this.WriteState;
        }
        
        public virtual void EndDocument()
        {
            this.VerifyDocumentWriteState();

            //  Add the default section properties, as the last thing
            //  before the closing of the <body> element
            this.AddSectionProperties( this.exportManager.GetFinalSectionProperties(), true );

            //  </w:body>
            this.WriteEndElement( WordProcessingMLElementType.body );

            //  </w:document>
            this.WriteEndElement( WordProcessingMLElementType.document );

            this.textWriter.Close();
        }
        
        #endregion StartDocument/EndDocument

        #region StartParagraph/EndParagraph

        public void StartParagraph( ParagraphProperties properties )
        {
            this.VerifyDocumentWriteState();

            //  <w:p>
            this.WriteStartElement( WordProcessingMLElementType.p );

            //  <w:pPr> and descendants
            if ( properties != null )
                this.WriteParagraphProperties( properties );
        }

        
        public void EndParagraph()
        {
            this.VerifyDocumentWriteState();

            //  <w:p>
            this.WriteEndElement( WordProcessingMLElementType.p );
        }

        #endregion StartParagraph/EndParagraph

        #region AddTextRun

        public void AddTextRun( TextRun textRun, RunStyle runStyle, NewLineType defaultNewLineType )
        {
            NewLineType newLineType = WordDocumentWriter.ResolveNewLineType( textRun.NewLineType, defaultNewLineType );
            Font font = textRun.HasFont ? textRun.Font : null;
            this.AddTextRunHelper( textRun.Text, font, textRun.CheckSpellingAndGrammar, runStyle, newLineType ); 
        }

        private void AddTextRunHelper( string text, WORD.Font font, bool? checkSpellingAndGrammar, RunStyle runStyle, NewLineType? newLineType )
        {
            //  TFS67025
            WordProcessingMLElementType newLineElement =
                (newLineType.HasValue == false || newLineType.Value == NewLineType.LineBreak) ?
                WordProcessingMLElementType.br :
                WordProcessingMLElementType.cr;

            string s = null;

            //  Start the <w:r> tag
            this.WriteStartElement( WordProcessingMLElementType.r );

            this.AddRunProperties( font, checkSpellingAndGrammar, runStyle, null, this.exportManager.GetDefaultFont() );

            #region Text, carriage returns

            if ( string.IsNullOrEmpty(text) == false )
            {
                //  Get the text and split it on newlines
                string[] split = WordUtilities.SplitOnNewLine(text);

                //  Iterate the result of the split; if the entry is empty
                for ( int i = 0; i < split.Length; i ++ )
                {
                    s = split[i];

                    //  Since we used StringSplitOptions.None to split, we will get
                    //  empty entries for each cr/lf, so add a CarriageReturnElement
                    //  for each one of these.
                    if ( string.IsNullOrEmpty(s) )
                        continue;

                    if ( s.Equals(Environment.NewLine) )
                        this.WriteEmptyElement( newLineElement );
                    else
                    {
                        //  If there are leading or trailing spaces, we have to
                        //  add the 'space' attribute to preserve them.
                        if ( s.StartsWith(WordDocumentPartExporter.SpaceChar) ||
                             s.EndsWith(WordDocumentPartExporter.SpaceChar) )
                        {
							this.WriteStartElement(WordProcessingMLElementType.t);
							this.WriteAttributeString(XmlElementType.space, WordDocumentPartExporter.SpaceAttributeValue);
							this.WriteString(s);
							this.WriteEndElement(WordProcessingMLElementType.t);
                        }
                        else
                            this.WriteElementString( WordProcessingMLElementType.t, s );
                    }
                }
            }
            #endregion Text, carriage returns

            //  Close the </w:r> tag
            this.WriteEndElement( WordProcessingMLElementType.r );
        }

        #endregion AddTextRun

        #region AddHyperlink

        public void AddHyperlink( string address, IList<TextRun> textRuns, string toolTipText, bool addToHistory, NewLineType defaultNewLineType )
        {
            this.AddHyperlink(
                WordProcessingMLElementType.hyperlink,
                WordProcessingMLAttributeType.tooltip,
                address,
                textRuns,
                toolTipText,
                defaultNewLineType );
        }

        private void AddHyperlink( Enum hyperlinkElement, Enum toolTipTextElement, string address, IList<TextRun> textRuns, string toolTipText, NewLineType defaultNewLineType )
        {
            string s = null;

            string rId = this.exportManager.CreateRelationshipInPackage(
                new Uri(address, UriKind.Absolute),
                WordDocumentExportManager.HyperlinkContentTypeValue,
                RelationshipTargetMode.External,
                false,
                this.partRelationshipCounters);

            //  Flag the export manager so it knows to add the styles part
            this.exportManager.HasHyperlinks = true;

            //  Start the <w:hyperlink> (or <a:hlinkClick>) element
            this.WriteStartElement( hyperlinkElement );

            //  Add the 'r:id' attribute
            this.WriteAttributeString( RelationshipsElementType.id, rId );

            //  Add the 'w:history' attribute if necessary
            bool addToHistory = false;
            if ( addToHistory )
            {
                s = OfficeXmlNode.GetXmlString( addToHistory, DataType.Boolean );
                this.WriteAttributeString( WordProcessingMLAttributeType.history, s );
            }

            //  Add the tooltip attribute if necessary
            if ( string.IsNullOrEmpty(toolTipText) == false )
                this.WriteAttributeString( toolTipTextElement, toolTipText );

            //  Text runs if there are any
            if ( textRuns != null && textRuns.Count > 0 )
            {
                foreach( TextRun textRun in textRuns )
                {
                    if ( textRun == null || string.IsNullOrEmpty(textRun.Text) )
                        continue;

                    this.AddTextRun( textRun, RunStyle.Hyperlink, defaultNewLineType );
                }
            }

            //  Close the </w:hyperlink> (or <a:hlinkClick>) element
            this.WriteEndElement( hyperlinkElement );
        }

        #endregion AddHyperlink

        #region AddInlinePicture

        public void AddInlinePicture( Image image, SizeF? size, PictureOutlineProperties outline, Hyperlink hyperlink, string altText, UnitOfMeasurement? unit )
        {
            this.AddInlinePictureHelper( image, size, outline, hyperlink, altText, unit );
        }

        #endregion AddInlinePicture

        #region AddAnchoredPicture

        public void AddAnchoredPicture( AnchoredPicture anchoredPicture)
        {
            this.AddAnchoredPictureHelper( anchoredPicture );
        }

        #endregion AddAnchoredPicture

        #region AddAnchoredPictureHelper

        private void AddAnchoredPictureHelper( AnchoredPicture anchoredPicture )
        {
            if ( anchoredPicture == null )
                return;

            string s = null;
            string zero = "0";
            string one = "1";
            Image image = anchoredPicture.Image;

            //  Get the image size in EMUs
            Size sizeInEMUs = anchoredPicture.SizeInEMUs;

            //  Start the <w:r> element
            this.WriteStartElement( WordProcessingMLElementType.r );

            #region <w:rPr>
            this.WriteStartElement( WordProcessingMLElementType.rPr );

            //  Add the <w:noProof> element
            this.WriteEmptyElement( WordProcessingMLElementType.noProof );

            //  Close the </w:rPr> element
            this.WriteEndElement( WordProcessingMLElementType.rPr );
            #endregion <w:rPr>

            #region <w:drawing>
            this.WriteStartElement( WordProcessingMLElementType.drawing );

            #region Get the relationshipId
            
            //  Put the image into the package, and get the resulting
            //  relationship ID and name.
            string relId = null;
            string name = null;
            OfficeDocumentExportManager.AddImageToPackage(
                this.exportManager,
                this.ImageCache,
                anchoredPicture.Image,



                null,

                this.partRelationshipCounters,
                out relId,
                out name );

            if ( string.IsNullOrEmpty(relId) )
                return;

            //  Get the numeric component of the relationship ID
            string relIdNumeric = relId.Replace( PackageUtilities.RelationshipIdPrefix, string.Empty );
            
            #endregion Get the relationshipId

            #region <wp:anchor>
            
            //  Start the <w:anchor> element
            this.WriteStartElement( WordProcessingDrawingElementType.anchor );

            #region <w:anchor> attributes

            //  Add the 'distT', 'distB', 'distL', and 'distR' attributes,
            //  which reflect the TextPadding values
            Padding textPadding = anchoredPicture.TextPaddingInEMUs;
            this.WriteAttributeValue( WordProcessingDrawingAttributeType.distT, (int)textPadding.Top, DataType.Integer );
            this.WriteAttributeValue( WordProcessingDrawingAttributeType.distB, (int)textPadding.Bottom, DataType.Integer );
            this.WriteAttributeValue( WordProcessingDrawingAttributeType.distL, (int)textPadding.Left, DataType.Integer );
            this.WriteAttributeValue( WordProcessingDrawingAttributeType.distR, (int)textPadding.Right, DataType.Integer );

            //  simplePos
            this.WriteAttributeString( WordProcessingDrawingAttributeType.simplePos, zero );

            //  relativeHeight
            this.WriteAttributeString( WordProcessingDrawingAttributeType.relativeHeight, WordDocumentPartExporter.RelativeHeightAttributeValue );

            //  behindDoc
            bool behindDoc = (anchoredPicture.TextWrapping == AnchorTextWrapping.TextInForeground);
            s = OfficeXmlNode.GetXmlString( behindDoc, DataType.Boolean );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.behindDoc, s );

            //  locked
            this.WriteAttributeString( WordProcessingDrawingAttributeType.locked, zero );

            //  layoutInCell
            this.WriteAttributeString( WordProcessingDrawingAttributeType.layoutInCell, one );

            //  allowOverlap
            this.WriteAttributeString( WordProcessingDrawingAttributeType.allowOverlap, one );

            #endregion <w:anchor> attributes

            #region <wp:simplePos>
            
            //  Start the <wp:simplePos> element
            this.WriteStartElement( WordProcessingDrawingElementType.simplePos );

            //  Add the x and y attributes
            this.WriteAttributeString( WordProcessingDrawingAttributeType.x, zero );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.y, zero );
            
            //  Close the </wp:simplePos> element
            this.WriteEndElement( WordProcessingDrawingElementType.simplePos );
            
            #endregion <wp:simplePos>

            #region <wp:positionH>

            //  Start the <wp:positionH> element
            this.WriteStartElement( WordProcessingDrawingElementType.positionH );

            //  Add the relativeFrom attribute
            s = EnumConverter.FromRelativeHorizontalPosition( anchoredPicture.RelativeHorizontalPosition );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.relativeFrom, s );

            //  posOffset/align
            if ( anchoredPicture.HasHorizontalAlignment )
            {
                //  Add the 'align' element with its value
                s = EnumConverter.FromHorizontalAlignment( anchoredPicture.HorizontalAlignment );
                this.WriteElementString( WordProcessingDrawingElementType.align, s );
            }
            else
            {
                //  Add the 'posOffset' element with its value
                s = OfficeXmlNode.GetXmlString( anchoredPicture.HorizontalOffsetInEMUs, DataType.Integer );
                this.WriteElementString( WordProcessingDrawingElementType.posOffset, s );
            }

            //  Close the </wp:positionH> element
            this.WriteEndElement( WordProcessingDrawingElementType.positionH );
            
            #endregion <wp:positionH>

            #region <wp:positionV>
            
            //  Start the <wp:positionV> element
            this.WriteStartElement( WordProcessingDrawingElementType.positionV );

            //  Add the relativeFrom attribute
            AnchorRelativeVerticalPosition vPos = anchoredPicture.RelativeVerticalPosition;

            //  TFS65546
            if ( anchoredPicture.HasVerticalAlignment &&
                vPos == AnchorRelativeVerticalPosition.Paragraph )
                vPos = AnchorRelativeVerticalPosition.Page;

            s = EnumConverter.FromRelativeVerticalPosition( vPos );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.relativeFrom, s );

            //  posOffset/align
            if ( anchoredPicture.HasVerticalAlignment )
            {
                //  Add the 'align' element with its value
                s = EnumConverter.FromAnchorVerticalAlignment( anchoredPicture.VerticalAlignment );
                this.WriteElementString( WordProcessingDrawingElementType.align, s );
            }
            else
            {
                //  Add the 'posOffset' element with its value
                s = OfficeXmlNode.GetXmlString( anchoredPicture.VerticalOffsetInEMUs, DataType.Integer );
                this.WriteElementString( WordProcessingDrawingElementType.posOffset, s );
            }
             
            //  Close the </wp:positionH> element
            this.WriteEndElement( WordProcessingDrawingElementType.positionV );

            #endregion <wp:positionV>

            #region <wp:extent>

            //  Start the <wp:extent> element
            this.WriteStartElement( WordProcessingDrawingElementType.extent );

            //  Add the cx and cy attributes
            s = OfficeXmlNode.GetXmlString( (int)sizeInEMUs.Width, DataType.Integer );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cx, s );
            
            s = OfficeXmlNode.GetXmlString( (int)sizeInEMUs.Height, DataType.Integer );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cy, s );
            
            //  Close the </wp:extent> element
            this.WriteEndElement( WordProcessingDrawingElementType.extent );
            
            #endregion <wp:extent>

            #region <wp:wrapSquare> et. al.
            AnchorTextWrapping textWrapping = anchoredPicture.TextWrapping;
            switch ( textWrapping )
            {
                case AnchorTextWrapping.Square:
                    this.WriteStartElement( WordProcessingDrawingElementType.wrapSquare );
                    s = EnumConverter.FromTextWrappingSide( anchoredPicture.TextWrappingSide );
                    this.WriteAttributeString( WordProcessingDrawingAttributeType.wrapText, s );
                    this.WriteEndElement( WordProcessingDrawingElementType.wrapSquare );
                    break;

                case AnchorTextWrapping.TopAndBottom:
                    this.WriteEmptyElement( WordProcessingDrawingElementType.wrapTopAndBottom );
                    break;

                case AnchorTextWrapping.TextInBackground:
                case AnchorTextWrapping.TextInForeground:
                    this.WriteEmptyElement( WordProcessingDrawingElementType.wrapNone );
                    break;

            }
            #endregion <wp:wrapSquare> et. al.

            #region <wp:docPr>

            //  Start the <wp:docPr> element
            this.WriteStartElement( WordProcessingDrawingElementType.docPr );

            //  Add the id and name attributes
            this.WriteAttributeString( WordProcessingDrawingAttributeType.id, relIdNumeric );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.name, name );

            //  descr
            if ( anchoredPicture.HasAlternateTextDescription )
                this.WriteAttributeString( WordProcessingDrawingAttributeType.descr, anchoredPicture.AlternateTextDescription );
            
            //  <a:hLinkClick>
            if ( anchoredPicture.HasHyperlink )
            {
                Hyperlink hyperlink = anchoredPicture.Hyperlink;
                this.AddHyperlink( DrawingMLElementType.hlinkClick, DrawingMLAttributeType.tooltip, hyperlink.Address, null, hyperlink.ToolTipText, WordUtilities.DefaultNewLineType );
            }

            //  Close the </wp:docPr> element
            this.WriteEndElement( WordProcessingDrawingElementType.docPr );
            
            #endregion <wp:docPr>

            #region <wp:cNvGraphicFramePr>

            //if ( anchoredPicture.Picture.PreserveAspectRatio.HasValue )
            //    this.SetPreserveAspectRatio( anchoredPicture.Picture.PreserveAspectRatio.Value );

            #endregion </wp:cNvGraphicFramePr>

            #region <a:graphic>
            
            PictureOutlineProperties outline = anchoredPicture.HasOutline ? anchoredPicture.Outline : null;
            this.AddPictureGraphicHelper( relIdNumeric, name, sizeInEMUs, outline );
            
            #endregion <a:graphic>

            //  Close the </wp:anchor> element
            this.WriteEndElement( WordProcessingDrawingElementType.anchor );
            #endregion <wp:anchor>
            
            //  Close the </w:drawing> element
            this.WriteEndElement( WordProcessingMLElementType.drawing );
            #endregion <w:drawing>
            
            //  Close the </w:r> element
            this.WriteEndElement( WordProcessingMLElementType.r );

        }

        #endregion AddAnchoredPictureHelper

        #region AddInlinePictureHelper

        private void AddInlinePictureHelper( Image image, SizeF? size, PictureOutlineProperties outline, Hyperlink hyperlink, string altText, UnitOfMeasurement? unit )
        {
            if ( image == null )
                throw new ArgumentNullException();

            string s = null;
            string zero = "0";

            //  Get the image size in EMUs
            Size sizeInEMUs = WordUtilities.GetImageSizeInEMUs( image, size, unit );

            //  Start the <w:r> element
            this.WriteStartElement( WordProcessingMLElementType.r );

            #region <w:rPr>
            this.WriteStartElement( WordProcessingMLElementType.rPr );

            //  Add the <w:noProof> element
            this.WriteEmptyElement( WordProcessingMLElementType.noProof );

            //  Close the </w:rPr> element
            this.WriteEndElement( WordProcessingMLElementType.rPr );
            #endregion <w:rPr>

            #region <w:drawing>
            this.WriteStartElement( WordProcessingMLElementType.drawing );

            #region <wp:inline>
            
            //  Start the <w:inline> element
            this.WriteStartElement( WordProcessingDrawingElementType.inline );

            //  Add the 'distT', 'distB', 'distL', and 'distR' attributes
            this.WriteAttributeString( WordProcessingDrawingAttributeType.distT, zero );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.distB, zero );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.distL, zero );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.distR, zero );
            
            #region <wp:extent>
            
            //  Start the <wp:extent> element
            this.WriteStartElement( WordProcessingDrawingElementType.extent );

            //  Add the cx and cy attributes
            s = OfficeXmlNode.GetXmlString( (int)sizeInEMUs.Width, DataType.Integer );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cx, s );
            
            s = OfficeXmlNode.GetXmlString( (int)sizeInEMUs.Height, DataType.Integer );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cy, s );
            
            //  Close the </wp:extent> element
            this.WriteEndElement( WordProcessingDrawingElementType.extent );
            
            #endregion <wp:extent>

            #region Get the relationshipId
            
            //  Put the image into the package, and get the resulting
            //  relationship ID and name.
            string relId = null;
            string name = null;
            OfficeDocumentExportManager.AddImageToPackage(
                this.exportManager,
                this.ImageCache,
                image,



                null,

                this.partRelationshipCounters,
                out relId,
                out name );

            if ( string.IsNullOrEmpty(relId) )
                return;

            //  Get the numeric component of the relationship ID
            string relIdNumeric = relId.Replace( PackageUtilities.RelationshipIdPrefix, string.Empty );
            
            #endregion Get the relationshipId

            #region <wp:docPr>
            
            //  Start the <wp:docPr> element
            this.WriteStartElement( WordProcessingDrawingElementType.docPr );

            //  Add the id and name attributes
            this.WriteAttributeString( WordProcessingDrawingAttributeType.id, relIdNumeric );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.name, name );
            
            //  descr
            if ( string.IsNullOrEmpty(altText) == false )
                this.WriteAttributeString( WordProcessingDrawingAttributeType.descr, altText );
            
            //  <a:hLinkClick>
            if ( hyperlink != null && string.IsNullOrEmpty(hyperlink.Address) == false )
                this.AddHyperlink( DrawingMLElementType.hlinkClick, DrawingMLAttributeType.tooltip, hyperlink.Address, null, hyperlink.ToolTipText, WordUtilities.DefaultNewLineType );

            //  Close the </wp:docPr> element
            this.WriteEndElement( WordProcessingDrawingElementType.docPr );
            
            #endregion <wp:docPr>

            #region <wp:cNvGraphicFramePr>

            //if ( preserveAspectRatio.HasValue )
            //    this.SetPreserveAspectRatio( preserveAspectRatio.Value );

            #endregion </wp:cNvGraphicFramePr>

            #region <a:graphic>
            
            this.AddPictureGraphicHelper( relIdNumeric, name, sizeInEMUs, outline );
            
            #endregion <a:graphic>

            //  Close the <w:inline> element
            this.WriteEndElement( WordProcessingDrawingElementType.inline );
            #endregion <wp:inline>
            
            //  Close the </w:drawing> element
            this.WriteEndElement( WordProcessingMLElementType.drawing );
            #endregion <w:drawing>
            
            //  Close the </w:r> element
            this.WriteEndElement( WordProcessingMLElementType.r );

        }

        #endregion AddInlinePictureHelper

        #region AddPictureGraphicHelper



#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

        private void AddPictureGraphicHelper( string relIdWithoutPrefix, string name, Size sizeInEMUs, PictureOutlineProperties outline )
        {
            string s = null;
            string zero = "0";
            string relId = string.Format("{0}{1}", PackageUtilities.RelationshipIdPrefix, relIdWithoutPrefix);

            #region <a:graphic>
            this.WriteStartElement( DrawingMLElementType.graphic );

            //  Write the namespace declaration
            this.WriteNamespaceDeclaration( WordDocumentPartExporter.DrawingMLNamespacePrefix, WordDocumentPartExporter.DrawingMLNamespace );

            #region <a:graphicData>
            this.WriteStartElement( DrawingMLElementType.graphicData );

            //  Write the 'uri' attribute
            this.WriteAttributeString( WordProcessingDrawingAttributeType.uri, WordDocumentPartExporter.UriAttributeValue );

            #region <pic:pic>
            this.WriteStartElement( DrawingMLPicElementType.pic );

            //  Write the namespace declaration
            this.WriteNamespaceDeclaration( WordDocumentPartExporter.DrawingMLPictureNamespacePrefix, WordDocumentPartExporter.DrawingMLPictureNamespace );

            #region <pic:nvPicPr>

            //  Start the <pic:nvPicPr> element
            this.WriteStartElement( DrawingMLPicElementType.nvPicPr );

            //  Start the <pic:cNvPr> element
            this.WriteStartElement( DrawingMLPicElementType.cNvPr );

            //  Write the 'id' and 'name' attributes
            this.WriteAttributeString( WordProcessingDrawingAttributeType.id, relIdWithoutPrefix );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.name, name );
                      
            //  Close the <pic:cNvPr> element
            this.WriteEndElement( DrawingMLPicElementType.cNvPr );

            //  Add the <pic:cNvPicPr> element
            this.WriteEmptyElement( DrawingMLPicElementType.cNvPicPr );

            //  Close the <pic:nvPicPr> element
            this.WriteEndElement( DrawingMLPicElementType.nvPicPr );

            #endregion <pic:nvPicPr>

            #region <pic:blipFill>

            this.WriteStartElement( DrawingMLPicElementType.blipFill );

            #region <a:blip>
            this.WriteStartElement( DrawingMLElementType.blip );

            //  Write the 'r:embed' attribute
            this.WriteAttributeString( RelationshipsAttributeType.embed, relId );

            //  Close the </a:blip> element
            this.WriteEndElement( DrawingMLElementType.blip );
            #endregion <a:blip>

            #region <a:stretch>
            this.WriteStartElement( DrawingMLElementType.stretch );
            this.WriteEmptyElement( DrawingMLElementType.fillRect );
            this.WriteEndElement( DrawingMLElementType.stretch );
            #endregion <a:stretch>

            //  Close the </pic:blipFill> element
            this.WriteEndElement( DrawingMLPicElementType.blipFill );

            #endregion <pic:blipFill>

            #region <pic:spPr>
            this.WriteStartElement( DrawingMLPicElementType.spPr );

            #region <a:xfrm>
            this.WriteStartElement( DrawingMLElementType.xfrm );
            
            #region <a:off>
            this.WriteStartElement( DrawingMLElementType.off );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.x, zero );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.y, zero );
            this.WriteEndElement( DrawingMLElementType.off );
            #endregion <a:off>
            
            #region <a:ext>
            this.WriteStartElement( DrawingMLElementType.ext );
            s = OfficeXmlNode.GetXmlString( (int)sizeInEMUs.Width, DataType.Integer );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cx, s );

			s = OfficeXmlNode.GetXmlString((int)sizeInEMUs.Height, DataType.Integer);
            this.WriteAttributeString( WordProcessingDrawingAttributeType.cy, s );
            this.WriteEndElement( DrawingMLElementType.ext );
            #endregion <a:ext>
            
            this.WriteEndElement( DrawingMLElementType.xfrm );
            #endregion <a:xfrm>

            #region <a:prstGeom>
            this.WriteStartElement( DrawingMLElementType.prstGeom );
            this.WriteAttributeString( WordProcessingDrawingAttributeType.prst, WordDocumentPartExporter.RectAttributeValue );
            this.WriteEndElement( DrawingMLElementType.prstGeom );
            #endregion <a:prstGeom>

            #region <a:ln>
            if ( outline != null )
            {
                //  <a:ln>
                this.WriteStartElement( DrawingMLElementType.ln );

                if ( outline.Style != PictureOutlineStyle.None && outline.LineWidthInEMUs > 0 )
                {
                    s = OfficeXmlNode.GetXmlString( outline.LineWidthInEMUs, DataType.Integer );
                    this.WriteAttributeString( WordProcessingDrawingAttributeType.w, s );

                    s = EnumConverter.FromPictureOutlineStyle(outline.Style);
                    this.WriteAttributeString( WordProcessingDrawingAttributeType.cmpd, s );

                    //  <a:solidFill>
                    this.WriteStartElement( DrawingMLElementType.solidFill );
                    
                    //  <a:srgbClr>
                    this.WriteStartElement( DrawingMLElementType.srgbClr );

					Color color = WordUtilities.ColorIsEmpty(outline.Color) == false ? outline.Color : WordUtilities.ColorFromArgb(0, 0, 0);
                    s = WordUtilities.ToHexBinary3( color );
                    this.WriteAttributeString( WordProcessingDrawingAttributeType.val, s );

                    //  </a:srgbClr>
                    this.WriteEndElement( DrawingMLElementType.srgbClr );
                    
                    //  </a:solidFill>
                    this.WriteEndElement( DrawingMLElementType.solidFill );

                    //  TFS64076
                    switch ( outline.CornerStyle )
                    {
                        case PictureOutlineCornerStyle.Round:
                            this.WriteEmptyElement( DrawingMLElementType.round );
                            break;
                        
                        case PictureOutlineCornerStyle.Bevel:
                            this.WriteEmptyElement( DrawingMLElementType.bevel );
                            break;
                        
                        case PictureOutlineCornerStyle.Miter:
                            this.WriteStartElement( DrawingMLElementType.miter );
                            this.WriteAttributeString( WordProcessingDrawingAttributeType.lim, "800000" );
                            this.WriteEndElement( DrawingMLElementType.miter );
                            break;
                    }

                }

                //  </a:ln>
                this.WriteEndElement( DrawingMLElementType.ln );
            }
            #endregion <a:ln>

            this.WriteEndElement( DrawingMLPicElementType.spPr );
            #endregion <pic:spPr>

            //  Close the </pic:pic> element
            this.WriteEndElement( DrawingMLPicElementType.pic );
            #endregion <pic:pic>

            //  Close the </a:graphicData> element
            this.WriteEndElement( DrawingMLElementType.graphicData );
            #endregion <a:graphicData>
            
            //  Close the </a:graphic> element
            this.WriteEndElement( DrawingMLElementType.graphic );
            #endregion <a:graphic>
            
        }

        #endregion AddPictureGraphicHelper

        #region AddAnchoredShape, AddInlineShape and related

        public void AddAnchoredShape( AnchoredShape anchoredShape )
        {
            this.AddShapeHelper_VML( anchoredShape );
        }

        public void AddInlineShape( VmlShape shape )
        {
            this.AddShapeHelper_VML( shape, null, null, null, null );
        }

        public void AddInlineShape( InlineShape inlineShape )
        {
            this.AddShapeHelper_VML( inlineShape.Shape as VmlShape, null, inlineShape.Hyperlink, inlineShape.AlternateTextDescription, inlineShape.SizeInTwips );
        }

        private void AddShapeHelper_VML( AnchoredShape anchoredShape )
        {
            this.AddShapeHelper_VML( anchoredShape.Shape as VmlShape, anchoredShape, anchoredShape.Hyperlink, anchoredShape.AlternateTextDescription, anchoredShape.SizeInTwips );
        }

        private void AddShapeHelper_VML( VmlShape shape, Anchor anchor, Hyperlink hyperlink, string altText, SizeF? sizeInTwips )
        {
            ShapeType shapeType = shape.Type;

            SizeF size = sizeInTwips.HasValue ? sizeInTwips.Value : shape.SizeInTwips;

            string s1 = null; string s2 = null;

            //  <w:r>, <w:pict>
            this.WriteStartElement( WordProcessingMLElementType.r );
            this.WriteStartElement( WordProcessingMLElementType.pict );

            VmlElementType shapeElement = VmlElementWriter.GetShapeElement( shapeType );

            #region <v:shape>, <v:rect>, etc.

            //  page 4667
            this.WriteStartElement( shapeElement );
            
            #region <id>, <type>
            
            s1 = VmlElementWriter.GetShapeId( shapeType, out s2 );

            this.WriteAttributeString( VmlAttributeType.id, s1 );

            if ( string.IsNullOrEmpty(s2) == false )
                this.WriteAttributeString( VmlAttributeType.type, s2 );
            
            #endregion <id>, <type>

            #region alt attribute
            if ( string.IsNullOrEmpty(altText) == false )
            {
                this.WriteAttributeString( VmlAttributeType.alt, altText );
            }
            #endregion alt attribute

            #region style attribute

            StringBuilder sb = new StringBuilder();
            string textWrapping = null;
            string textWrappingSide = null;

            if ( anchor != null )
            {
                //  position:absolute
                sb.Append( "position:absolute;" );

                //  margin-left / margin-top
                float leftInPoints = WordUtilities.Convert(anchor.HorizontalOffsetInTwips, UnitOfMeasurement.Twip, UnitOfMeasurement.Point );
                float topInPoints = WordUtilities.Convert(anchor.VerticalOffsetInTwips, UnitOfMeasurement.Twip, UnitOfMeasurement.Point );
                s1 = OfficeXmlNode.GetXmlString( leftInPoints, DataType.Float );
                s2 = OfficeXmlNode.GetXmlString( topInPoints, DataType.Float );
                sb.AppendFormat( "margin-left:{0}pt;margin-top:{1}pt;", s1, s2 );
            }

            //  width / height
            float width = WordUtilities.Convert( (float)size.Width, UnitOfMeasurement.Twip, UnitOfMeasurement.Point );
			float height = WordUtilities.Convert( (float)size.Height, UnitOfMeasurement.Twip, UnitOfMeasurement.Point );
            s1 = OfficeXmlNode.GetXmlString( width, DataType.Float );
            s2 = OfficeXmlNode.GetXmlString( height, DataType.Float );
            sb.AppendFormat( "width:{0}pt;height:{1}pt;", s1, s2 );

            if ( anchor != null )
            {
                //  z-index
                int zIndex = 0;
                textWrapping = anchor.GetTextWrapping_Vml( out textWrappingSide, out zIndex );
                s1 = zIndex < 0 ? "-" : string.Empty;
                sb.Append( string.Format("z-index:{0}251658240;", s1) );

                //  mso-position-horizontal, mso-position-horizontal-relative
                s1 = anchor.GetHorizontalAlignment_Vml( out s2 );
                sb.AppendFormat( "mso-position-horizontal:{0};mso-position-horizontal-relative:{1};", s1, s2 );

                //  mso-position-vertical, mso-position-vertical-relative
                s1 = anchor.GetVerticalAlignment_Vml( out s2 );
                sb.AppendFormat( "mso-position-vertical:{0};mso-position-vertical-relative:{1};", s1, s2 );
            }
            else
                //  This seems to be required for an inline shape
                sb.Append( "mso-left-percent:-10001;mso-top-percent:-10001;mso-position-horizontal:absolute;mso-position-horizontal-relative:char;mso-position-vertical:absolute;mso-position-vertical-relative:line;mso-left-percent:-10001;mso-top-percent:-10001;" );

            //  flip
            if ( shape._FlipX && shape._FlipY )
                sb.Append( "flip:yx;" );
            else
            if ( shape._FlipX )
                sb.Append( "flip:x;" );
            else
            if ( shape._FlipY )
                sb.Append( "flip:y;" );

            //  rotation
            if ( shape._Rotation != 0 )
            {
                sb.AppendFormat( "rotation:{0};", shape._Rotation );
            }

            this.WriteAttributeString( VmlAttributeType.style, sb.ToString() );

            #endregion style attribute

            //  strokecolor
            if ( WordUtilities.ColorIsEmpty(shape.LineColor) == false )
            {
                s1 = string.Format( "#{0}", WordUtilities.ToHexBinary3(shape.LineColor) );
                this.WriteAttributeString( VmlAttributeType.strokecolor, s1 );
            }

            //  fillcolor
            if ( WordUtilities.ColorIsEmpty(shape._BackColor) == false )
            {
                s1 = string.Format( "#{0}", WordUtilities.ToHexBinary3(shape._BackColor) );
                this.WriteAttributeString( VmlAttributeType.fillcolor, s1 );
            }

            //  strokeweight
            if ( shape.LineWidth.HasValue )
            {
                s1 = string.Format( "{0}pt", shape.LineWidthInPoints );
                this.WriteAttributeString( VmlAttributeType.strokeweight, s1 );
            }

            //  href / title
            if ( hyperlink != null && string.IsNullOrEmpty(hyperlink.Address) == false )
            {
                this.WriteAttributeString( VmlAttributeType.href, hyperlink.Address );

                if ( string.IsNullOrEmpty(hyperlink.ToolTipText) == false )
                    this.WriteAttributeString( VmlAttributeType.title, hyperlink.ToolTipText );
            }

            //  BF 4/25/11  TFS73612
            //
            //  I moved this up from after the <stroke> element...this is
            //  an attribute of the shape element, so it has to precede the
            //  <stroke> element.
            //
            if ( string.IsNullOrEmpty(shape._ConnectorType) == false )
            {
                //  <o:connectortype>
                this.WriteAttributeString( OfficeAttributeType.connectortype, shape._ConnectorType );
            }

            //  TFS67121
            if ( shape.LineStyle != ShapeLineStyle.Solid )
            {
                this.WriteStartElement( VmlElementType.stroke );
                s1 = EnumConverter.FromShapeLineStyle( shape.LineStyle );
                this.WriteAttributeString( VmlAttributeType.dashstyle, s1 );
                
                //  BF 4/11/11  TFS72253
                //this.WriteStartElement( VmlElementType.stroke );
                this.WriteEndElement( VmlElementType.stroke );
            }

            //  BF 4/25/11  TFS73612 (see above)
            #region Moved up
            //if ( string.IsNullOrEmpty(shape._ConnectorType) == false )
            //{
            //    //  <o:connectortype>
            //    this.WriteAttributeString( OfficeAttributeType.connectortype, shape._ConnectorType );
            //}
            #endregion Moved up

            #region <w10:wrap>

            if ( anchor == null )
            {
                textWrapping = "none";
                textWrappingSide = null;
            }
            
            this.WriteStartElement( OfficeWordElementType.wrap );

            //  type
            if ( string.IsNullOrEmpty(textWrapping) == false )
                this.WriteAttributeString( OfficeWordAttributeType.type, textWrapping );
            
            //  side
            if ( string.IsNullOrEmpty(textWrappingSide) == false )
                this.WriteAttributeString( OfficeWordAttributeType.side, textWrappingSide );
            
            this.WriteEndElement( OfficeWordElementType.wrap );
            
            #endregion <w10:wrap>

            //  <w10:anchorlock>
            if ( anchor == null )
                this.WriteEmptyElement( OfficeWordElementType.anchorlock );

            this.WriteEndElement( shapeElement );

            #endregion </v:shape>, </v:rect>, etc.

            //  </w:r>, </w:pict>
            this.WriteEndElement( WordProcessingMLElementType.r );
            this.WriteEndElement( WordProcessingMLElementType.pict );
        }

        #endregion AddAnchoredShape, AddInlineShape and related

        #region SetPreserveAspectRatio
        private void SetPreserveAspectRatio( bool value )
        {
            //  Start the <wp:cNvGraphicFramePr> element
            this.WriteStartElement( WordProcessingDrawingElementType.cNvGraphicFramePr );

            //  Start the <a:graphicFrameLocks> element
            this.WriteStartElement( DrawingMLElementType.graphicFrameLocks );

            //  Add the noChangeAspect attribute
            string s = value ? "1" : "0";
            this.WriteAttributeString( WordProcessingDrawingAttributeType.noChangeAspect, s );

            //  Close the </a:graphicFrameLocks> element
            this.WriteEndElement( DrawingMLElementType.graphicFrameLocks );

            //  Close the </wp:cNvGraphicFramePr> element
            this.WriteEndElement( WordProcessingDrawingElementType.cNvGraphicFramePr );
        }
        #endregion SetPreserveAspectRatio

        #region StartTable / EndTable

        public void StartTable( IList<float> columnWidths, TableProperties properties, UnitOfMeasurement unit )
        {
            #region Push a TableBorderProperties onto the stack
            //
            //  BF 3/14/11
            //
            //  In MS Word, if the color (for example) is set on a cell borders,
            //  but the style is not explicitly set, no borders appear, because
            //  cells do not inherit borders from the table. This seems wrong or
            //  at least guaranteed to be a source of confusion for users, so
            //  we will cache the table's border definition here so that we can
            //  resolve the cell borders in subsequent calls to StartTableCell.
            //
            if ( this.tableBorderProperties == null )
                this.tableBorderProperties = new Stack<TableBorderProperties>();

            TableBorderProperties borderProps = properties != null && properties.ShouldSerializeBorderProperties() ? properties.BorderProperties : null;

            //  If the table has border settings, clone them, so that we use
            //  a snapshot of what the properties were when they started the
            //  table. This is because if they reuse that instance, we don't
            //  want to pick up any changes they might have applied to it.
            if ( borderProps != null )
            {
                TableBorderProperties clone = new TableBorderProperties(borderProps.unitOfMeasurementProvider);
                clone.InitializeFrom( borderProps );
                borderProps = clone;
            }

            this.tableBorderProperties.Push( borderProps );
            
            #endregion Push a TableBorderProperties onto the stack

            //  <w:tbl>
            this.WriteStartElement( WordProcessingMLElementType.tbl );

            #region <w:tblPr>
            this.AddTableProperties( properties, false );
            #endregion </w:tblPr>

            #region <w:tblGrid>

            this.WriteStartElement( WordProcessingMLElementType.tblGrid );

            for ( int i = 0, count = columnWidths.Count; i < count; i ++ )
            {
                //  <w:gridCol>
                this.WriteStartElement( WordProcessingMLElementType.gridCol );

                //  <w:w>
                int width =(int)WordUtilities.ConvertToTwips( unit, columnWidths[i] );
                this.WriteAttributeValue( WordProcessingMLAttributeType.w, width, DataType.Integer );
                
                //  </w:gridCol>
                this.WriteEndElement( WordProcessingMLElementType.gridCol );
            }

            this.WriteEndElement( WordProcessingMLElementType.tblGrid );
            
            #endregion <w:tblGrid>
        }

        public void EndTable()
        {
            this.WriteEndElement( WordProcessingMLElementType.tbl );

            //  BF 3/14/11
            if ( this.tableBorderProperties != null && this.tableBorderProperties.Count > 0 )
                this.tableBorderProperties.Pop();
        }

        #endregion StartTable / EndTable

        #region StartTableRow / EndTableRow
        public void StartTableRow( TableRowProperties properties )
        {
            //  <w:tr>
            this.WriteStartElement( WordProcessingMLElementType.tr );

            this.AddRowProperties( properties );
        }

        public void EndTableRow()
        {
            //  </w:tr>
            this.WriteEndElement( WordProcessingMLElementType.tr );
        }
        #endregion StartTableRow / EndTableRow

        #region StartTableCell / EndTableCell
        public void StartTableCell( TableCellProperties properties )
        {
            //  <w:tc>
            this.WriteStartElement( WordProcessingMLElementType.tc );

            this.AddTableCellProperties( properties, false );
        }

        public void EndTableCell()
        {
            this.WriteEndElement( WordProcessingMLElementType.tc );
        }

        #endregion StartTableCell / EndTableCell

        #region AddSectionProperties
        public void AddSectionProperties( SectionProperties properties, bool isDefaultSection )
        {
            this.AddSectionProperties( properties, null, isDefaultSection );
        }

        public virtual void AddSectionProperties( SectionProperties properties, List<HeaderFooterInfo> headerFooterInfoItems, bool isDefaultSection )
        {
            string s = null;

            if ( properties == null )
                properties = new SectionProperties( null );

            if ( isDefaultSection == false )
            {
                //  <w:p>
                this.WriteStartElement( WordProcessingMLElementType.p );

                //  <w:pPr>
                this.WriteStartElement( WordProcessingMLElementType.pPr );
            }

            //  <w:sectPr>
            this.WriteStartElement( WordProcessingMLElementType.sectPr );

            #region <w:headerReference> / <w:footerReference>
            //
            //  Add each of the headerReference and footerReference elements
            //  that we need for this section.
            //

            //  If this is the default page section, and a header/footer was written
            //  to it at some point, we have to add the reference elements now.
            if ( isDefaultSection && properties.HasHeaderFooterInfoItems )
                headerFooterInfoItems = properties.HeaderFooterInfoItems;

            if ( headerFooterInfoItems != null && headerFooterInfoItems.Count > 0 )
            {
                //  Write out a <w:titlePg> element if a "first page only" header
                //  or footer was specified.
                foreach ( HeaderFooterInfo headerFooterInfo in headerFooterInfoItems )
                {
                    if ( headerFooterInfo.ReferenceType == HeaderFooterType.FirstPageOnly )
                        this.WriteEmptyElement( WordProcessingMLElementType.titlePg );
                }

                foreach ( HeaderFooterInfo headerFooterInfo in headerFooterInfoItems )
                {
                    WordProcessingMLElementType headerFooterReferenceElement =
                        headerFooterInfo.Type == HeaderOrFooter.Header ?
                        WordProcessingMLElementType.headerReference :
                        WordProcessingMLElementType.footerReference;

                    //  <w:headerReference> / <w:footerReference>
                    this.WriteStartElement( headerFooterReferenceElement );

                    //  <w:type> attribute
                    s = headerFooterInfo.ReferenceType == HeaderFooterType.AllPages ?
                        "default" :
                        "first";

                    this.WriteAttributeString( WordProcessingMLAttributeType.type, s );

                    //  <r:id> attribute
                    s = headerFooterInfo.RelationshipId;
                    this.WriteAttributeString( RelationshipsElementType.id, s );

                    //  </w:headerReference> / </w:footerReference>
                    this.WriteEndElement( headerFooterReferenceElement );
                }
            }
            #endregion <w:headerReference> / <w:footerReference>

            #region <w:pgSz>

            this.WriteStartElement( WordProcessingMLElementType.pgSz );

            //  <w:w>, <w:h>
            int width = 0;
            int height = 0;
            PageOrientation pageOrientation = PageOrientation.Default;
            properties.GetPageSizeInTwips( out width, out height, out pageOrientation );
            
            s = OfficeXmlElement.GetXmlString( width, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
            
            s = OfficeXmlElement.GetXmlString( height, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.h, s );

            if ( pageOrientation == PageOrientation.Landscape )
                this.WriteAttributeString( WordProcessingMLAttributeType.orient, PageOrientation.Landscape.ToString().ToLower() );
            
            if ( properties.PaperCode.HasValue )
            {
                s = OfficeXmlElement.GetXmlString( properties.PaperCode.Value, DataType.Int32 );
                this.WriteAttributeString( WordProcessingMLAttributeType.code, s );
            }

            this.WriteEndElement( WordProcessingMLElementType.pgSz );

            #endregion </w:pgSz>

            #region <w:pgMar>

            this.WriteStartElement( WordProcessingMLElementType.pgMar );
            
            Padding pageMargins = properties.PageMarginsInTwips;
            
            s = OfficeXmlElement.GetXmlString( (int)pageMargins.Top, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.top, s );

            s = OfficeXmlElement.GetXmlString( (int)pageMargins.Right, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.right, s );

            s = OfficeXmlElement.GetXmlString( (int)pageMargins.Bottom, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.bottom, s );

            s = OfficeXmlElement.GetXmlString( (int)pageMargins.Left, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.left, s );

            s = OfficeXmlElement.GetXmlString( (int)properties.HeaderMarginInTwips, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.header, s );

            s = OfficeXmlElement.GetXmlString( (int)properties.FooterMarginInTwips, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.footer, s );

            s = OfficeXmlElement.GetXmlString( 0, DataType.Int32 );
            this.WriteAttributeString( WordProcessingMLAttributeType.gutter, s );

            this.WriteEndElement( WordProcessingMLElementType.pgMar );
            
            #endregion <w:pgMar>

            #region <w:pgNumType
            if ( properties.StartingPageNumber.HasValue )
            {
                this.WriteStartElement( WordProcessingMLElementType.pgNumType );
                s = OfficeXmlNode.GetXmlString( properties.StartingPageNumber.Value, DataType.Int32 );
                this.WriteAttributeString( WordProcessingMLAttributeType.start, s );
                this.WriteEndElement( WordProcessingMLElementType.pgNumType );
            }
            #endregion <w:pgNumType

            #region <w:cols>

            //this.WriteStartElement(WordProcessingMLElementType.cols);
            //s = OfficeXmlElement.GetXmlString(0, DataType.Int32);
            //this.WriteAttributeString(WordProcessingMLAttributeType.space, "720");
            //this.WriteEndElement(WordProcessingMLElementType.cols);
            
            #endregion <w:cols>

            //  </w:sectPr>
            this.WriteEndElement( WordProcessingMLElementType.sectPr );

            if ( isDefaultSection == false )
            {
                //  </w:pPr>
                this.WriteEndElement( WordProcessingMLElementType.pPr );

                //  </w:p>
                this.WriteEndElement( WordProcessingMLElementType.p );
            }
        }
        #endregion AddSectionProperties

        #region DefineSection

        public virtual void DefineSection( SectionProperties properties )
        {
            this.VerifyDocumentWriteState();
            
            this.AddSectionProperties( properties, false );
        }

        #endregion DefineSection

        #region Flush
        public void Flush()
        {
            if ( this.textWriter != null )
            {
                try
                {
                    this.textWriter.Flush();
                }
                catch( Exception exception )
                {
                    throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.WriterError, exception);
                }
            }
        }
        #endregion Flush

        #endregion Streaming support

        #region VerifyDocumentWriteState
        private void VerifyDocumentWriteState()
        {
            if ( this.textWriter == null || this.contentStream == null )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.DocumentNotOpen);

            if ( this.textWriter.WriteState == WriteState.Closed ||
                 this.textWriter.WriteState == WriteState.Error )
                throw new WordDocumentWriterException(WordDocumentWriterExceptionReason.WriterError);
        }
        #endregion VerifyDocumentWriteState

        #region AddSectionHeaderFooter
        internal virtual SectionHeaderFooterWriterSet AddSectionHeaderFooter(
            WordDocumentWriter documentWriter,
            SectionHeaderFooterParts parts,
            SectionProperties sectionProperties,
            bool isDefaultSection )
        {
            #region Create the HeaderFooterInfo objects 

            List<HeaderFooterInfo> items = new List<HeaderFooterInfo>();

            if ( (parts & SectionHeaderFooterParts.HeaderAllPages) == SectionHeaderFooterParts.HeaderAllPages )
                items.Add( new HeaderFooterInfo(HeaderOrFooter.Header, HeaderFooterType.AllPages, null) );

            if ( (parts & SectionHeaderFooterParts.HeaderFirstPageOnly) == SectionHeaderFooterParts.HeaderFirstPageOnly )
                items.Add( new HeaderFooterInfo(HeaderOrFooter.Header, HeaderFooterType.FirstPageOnly, null) );

            if ( (parts & SectionHeaderFooterParts.FooterAllPages) == SectionHeaderFooterParts.FooterAllPages )
                items.Add( new HeaderFooterInfo(HeaderOrFooter.Footer, HeaderFooterType.AllPages, null) );

            if ( (parts & SectionHeaderFooterParts.FooterFirstPageOnly) == SectionHeaderFooterParts.FooterFirstPageOnly )
                items.Add( new HeaderFooterInfo(HeaderOrFooter.Footer, HeaderFooterType.FirstPageOnly, null) );

            #endregion Create the HeaderFooterInfo objects 

            #region Create the package parts for each reference

            List<WordHeaderFooterWriter> writerList = new List<WordHeaderFooterWriter>(items.Count);

            WordHeaderFooterPartExporterBase partExporter = null;
            int partNumber = 0;

            //  Iterate the list we populated before and create a package part
            //  for each one of them. The creation routine gives us back the
            //  relationship id, so take that and update the HeaderFooterInfo
            //  with it.
            foreach ( HeaderFooterInfo item in items )
            {
                switch ( item.Type )
                {
                    case HeaderOrFooter.Footer:
                        partExporter = new WordFooterPartExporter( exportManager, this.nextFooterPartNumber );
                        partNumber = this.nextFooterPartNumber;
                        this.nextFooterPartNumber++;
                        break;

                    case HeaderOrFooter.Header:
                        partExporter = new WordHeaderPartExporter( exportManager, this.nextHeaderPartNumber );
                        partNumber = this.nextHeaderPartNumber;
                        this.nextHeaderPartNumber++;
                        break;
                }

                //  Create the part for the header/footer
                //  Create the '/word/document.xml' part.
                string relationshipId = null;
                string partPath = string.Format(WordHeaderFooterPartExporterBase.PartNameTemplate, partExporter.Id, partNumber);
                this.exportManager.CreatePartInPackage(
                    partExporter,
                    partPath,
                    null,
                    //  BF 1/22/11  partRelationshipCounters_change
                    //partExporter.partRelationshipCounters,
                    this.partRelationshipCounters,
                    out relationshipId);

                //  BF 1/22/11  partRelationshipCounters_change
                //
                //  Push the part counter that was just created onto the
                //  export manager's stack. In essence what we are doing is
                //  duplicating the part counters for the sake of the part
                //  creation logic. For example, when the instance of this
                //  class which represents the \word\document.xml part is
                //  created, we put a part counter for it on its own stack,
                //  so that parts created under that part are seen as "children"
                //  of it rather  than siblings.
                //
                partExporter.PartRelationshipCounters.Push(this.partRelationshipCounters.Peek());

                //  BF 1/22/11  partRelationshipCounters_change
                //
                //  And now, pop it off this one's stack, so that the next
                //  header/footer doesn't get added as a part within this
                //  header/footer. This line and the line before it is
                //  basically a workaround for the fact that the part creation
                //  logic depends on a stack to get the relationship ids.
                this.partRelationshipCounters.Pop();

                //  Update the rId now that we have it
                item.SetRelationshipId( relationshipId );

                //  Create the WordHeaderFooterWriter instance that will
                //  be returned to the caller for public consumption.
                WordprocessingMLHeaderFooterWriter writer =
                    new WordprocessingMLHeaderFooterWriter(
                        documentWriter,
                        partExporter,
                        item.Type,
                        item.ReferenceType );

                //  Add that instance to the list
                writerList.Add( writer );
            }
            
            #endregion Create the package parts for each reference

            #region Write the <w:headerReference> and <w:footerReference> elements
            
            //  If this is the default page section, cache the list of
            //  HeaderFooterInfo items, since we don't write the XML out
            //  until later.
            //  
            //  If this is not the default section, write the section properties
            //  element now.
            //
            if ( isDefaultSection )
                sectionProperties.SetHeaderFooterInfoItems( items );
            else
                this.AddSectionProperties( sectionProperties, items, isDefaultSection );
            
            #endregion Write the <w:headerReference> and <w:footerReference> elements

            //  Return the SectionHeaderFooterWriterSet, which contains one or more
            //  WordHeaderFooterWriter instance, each of which can be used to write
            //  content to a different header/footer part in the section.
            SectionHeaderFooterWriterSet writerSet = new SectionHeaderFooterWriterSet( writerList );

            //  BF 1/27/11
            documentWriter.CacheHeaderFooterWriters( writerList );
            
            return writerSet;

        }
        #endregion AddSectionHeaderFooter

        #region AddPageNumberField
        public void AddPageNumberField( PageNumberFieldFormat fieldType, Font font )
        {
            string s = string.Format( "PAGE \\* {0}", EnumConverter.FromPageNumberFieldFormat(fieldType) );

            if ( font == null )
            {
                this.WriteStartElement( WordProcessingMLElementType.fldSimple );
                this.WriteAttributeString( WordProcessingMLAttributeType.instr, s );
                this.WriteEndElement( WordProcessingMLElementType.fldSimple );
            }
            else
            {
                //  <w:r>
                this.WriteStartElement( WordProcessingMLElementType.r );

                //  <w:rPr>
                this.AddRunProperties(font, false, RunStyle.None, null, null );

                //  <w:fldChar> (begin)
                this.WriteStartElement( WordProcessingMLElementType.fldChar );
                this.WriteAttributeString( WordProcessingMLAttributeType.fldCharType, "begin" );
                this.WriteEndElement( WordProcessingMLElementType.fldChar );

                //  <w:instrText>
                this.WriteElementString( WordProcessingMLElementType.instrText, s );

                //  <w:fldChar> (end)
                this.WriteStartElement( WordProcessingMLElementType.fldChar );
                this.WriteAttributeString( WordProcessingMLAttributeType.fldCharType, "end" );
                this.WriteEndElement( WordProcessingMLElementType.fldChar );

                //  </w:r>
                this.WriteEndElement( WordProcessingMLElementType.r );
            }

        }
        #endregion AddPageNumberField

		// AS 2/15/11
		// Added caching of the element name info.
		//
		#region ElementNameCache class
		private class ElementNameCache
		{
			internal string ns;
			internal string prefix;
			internal string localName;
			internal string qualifiedName;
		}
		#endregion //ElementNameCache class
	}

    #endregion WordDocumentPartExporter class

    #region VmlElementWriter
    internal class VmlElementWriter
    {
        //internal const string ShapeType_Id = "_x0000_t32";
        //internal const string ShapeType_CoordSize = "21600,21600";
        //internal const string ShapeType_Path = "m,l21600,21600e";

        internal const string Shape_Id_Line = "_x0000_s1026";
        internal const string Shape_Id_Rectangle = "_x0000_s1027";
        internal const string Shape_Id_Ellipse = "_x0000_s1028";
        internal const string Shape_Id_IsosceleseTriangle = "_x0000_s1029";
        internal const string Shape_Id_RightTriangle = "_x0000_s1030";
        
        internal const string Shape_Type_Line = "#_x0000_t32";
        internal const string Shape_Type_IsosceleseTriangle = "#_x0000_t5";
        internal const string Shape_Type_RightTriangle = "#_x0000_t6";
        
        internal const string Shape_ConnectorType = "straight";

        static internal string GetShapeId( ShapeType shapeType, out string type )
        {
            type = null;
            string retVal = null;

            switch ( shapeType )
            {
                case ShapeType.Line:
                    type = VmlElementWriter.Shape_Type_Line;
                    retVal = VmlElementWriter.Shape_Id_Line;
                    break;

                case ShapeType.Rectangle:
                    retVal = VmlElementWriter.Shape_Id_Rectangle;
                    break;

                case ShapeType.Ellipse:
                    retVal = VmlElementWriter.Shape_Id_Ellipse;
                    break;

                case ShapeType.IsosceleseTriangle:
                    type = VmlElementWriter.Shape_Type_IsosceleseTriangle;
                    retVal = VmlElementWriter.Shape_Id_IsosceleseTriangle;
                    break;

                case ShapeType.RightTriangle:
                    type = VmlElementWriter.Shape_Type_RightTriangle;
                    retVal = VmlElementWriter.Shape_Id_RightTriangle;
                    break;

                default:
                    WordUtilities.DebugFail( "Unrecognized ShapeType constant." );
                    break;
            }

            return retVal;
        }

        static internal VmlElementType GetShapeElement( ShapeType shapeType )
        {
            switch ( shapeType )
            {
                case ShapeType.Line:
                case ShapeType.IsosceleseTriangle:
                case ShapeType.RightTriangle:
                    return VmlElementType.shape;

                case ShapeType.Rectangle:
                    return VmlElementType.rect;

                case ShapeType.Ellipse:
                    return VmlElementType.oval;

                default:
                    WordUtilities.DebugFail( "Unrecognized ShapeType constant." );
                    break;
            }

            return VmlElementType.shape;
        }
    }
    #endregion VmlElementWriter

    #region WordHeaderFooterPartExporterBase class

    internal abstract class WordHeaderFooterPartExporterBase : WordDocumentPartExporter
    {
        #region Constants

        /// <summary>'/word/[header/footer]{0}.xml'</summary>
        public const string PartNameTemplate = "/word/{0}{1}.xml";

        #endregion Constants

        #region Member variables

        private int partNumber = 0;
        private bool wasOpened = false;
        
        #endregion Member variables

        #region Constructor
        internal WordHeaderFooterPartExporterBase( WordDocumentExportManager exportManager, int partNumber ) : base( exportManager )
        {
            this.partNumber = partNumber;
        }

        static internal WordHeaderFooterPartExporterBase Create( WordDocumentExportManager exportManager, HeaderOrFooter type, int partNumber )
        {
            switch ( type )
            {
                case HeaderOrFooter.Footer:
                    return new WordFooterPartExporter( exportManager, partNumber );

                default:
                case HeaderOrFooter.Header:
                    return new WordHeaderPartExporter( exportManager, partNumber );
            }
        }

        #endregion Constructor

        #region Id
        internal abstract string Id { get; }
        #endregion Id

        #region RootElement
        internal abstract WordProcessingMLElementType RootElement { get; }
        #endregion RootElement

        #region ContentType

        public override string ContentType
        {
            get { return string.Format("application/vnd.openxmlformats-officedocument.wordprocessingml.{0}+xml", this.Id); }
        }

        #endregion ContentType

        #region RelationshipType

        public override string RelationshipType
        {
            get { return string.Format("http://schemas.openxmlformats.org/officeDocument/2006/relationships/{0}", this.Id); }
        }

        #endregion RelationshipType

        #region Streaming support

        #region StartDocument/EndDocument

        public override WriteState StartDocument()
        {
            throw new NotSupportedException();
        }
        
        public override void EndDocument()
        {
            throw new NotSupportedException();
        }
        
        #endregion StartDocument/EndDocument

        #region AddSectionProperties
        public override void AddSectionProperties( SectionProperties properties, List<HeaderFooterInfo> headerFooterInfo, bool isDefaultSection )
        {
            throw new NotSupportedException();
        }
        #endregion AddSectionProperties

        #region DefineSection

        public override void DefineSection( SectionProperties properties )
        {
            throw new NotSupportedException();
        }

        #endregion DefineSection

        #region AddSectionHeaderFooter
        internal override SectionHeaderFooterWriterSet AddSectionHeaderFooter(
            WordDocumentWriter writer,
            SectionHeaderFooterParts parts,
            SectionProperties sectionProperties,
            bool isDefaultSection )
        {
            throw new NotSupportedException();
        }
        #endregion AddSectionHeaderFooter

        #region Open / Close
        
        internal void Open()
        {
            //  Take this to mean it's already been opened
            if ( this.textWriter == null )
            {
                this.CreateXmlWriter( this.contentStream, true );

                //  Write the root element, i.e., <w:hdr> or <w:ftr>
                this.WriteStartElement( this.RootElement );

                //  Add the ns declarations for the root element
                this.AddNamespaceDeclarations();

                //  Flag this part as having been opened
                this.wasOpened = true;
            }

            //  At this point content can be written to this part,
            //  for example StartParagraph can be called.
        }
        
        internal void Close()
        {
            if ( this.wasOpened == false )
                this.Open();

            if (this.wasOpened && this.textWriter == null)
                return;

            //  Close the root element, i.e., </w:hdr> or </w:ftr>
            this.WriteEndElement( this.RootElement );

            //  Close the TextWriter
            if ( this.textWriter != null )
            {
                this.textWriter.Close();
                this.textWriter = null;
            }
        }

        #endregion StartSectionHeaderFooter

        #endregion Streaming support
    }

    #endregion WordHeaderPartExporter class

    #region WordHeaderPartExporter class

    internal class WordHeaderPartExporter : WordHeaderFooterPartExporterBase
    {
        internal WordHeaderPartExporter( WordDocumentExportManager exportManager, int partNumber ) : base( exportManager, partNumber ){}

        internal override string Id { get { return "header"; } }

        internal override WordProcessingMLElementType RootElement { get { return WordProcessingMLElementType.hdr; } }

    }

    #endregion WordHeaderPartExporter class

    #region WordFooterPartExporter class

    internal class WordFooterPartExporter : WordHeaderFooterPartExporterBase
    {
        internal WordFooterPartExporter( WordDocumentExportManager exportManager, int partNumber ) : base( exportManager, partNumber ){}

        internal override string Id { get { return "footer"; } }

        internal override WordProcessingMLElementType RootElement { get { return WordProcessingMLElementType.ftr; } }
    }

    #endregion WordFooterPartExporter class
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