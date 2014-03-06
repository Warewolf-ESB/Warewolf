using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using Infragistics.Documents.Core;
using System.Xml;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{
    #region WordCustomizablePartExporterBase class

    internal abstract class WordCustomizablePartExporterBase : ContentTypeExporterBase
    {
		#region Constants
        
        internal const string AsciiThemeAttributeValue = "minorHAnsi";
        internal const string EastAsiaThemeAttributeValue = "minorEastAsia";
        internal const string HAnsiThemeAttributeValue = WordCustomizablePartExporterBase.AsciiThemeAttributeValue;
        internal const string ComplexScriptThemeAttributeValue = "minorBidi";

        #endregion Constants

        #region Member variables
        
        protected Stream contentStream = null;
        protected XmlWriter textWriter = null;
        protected Enum lastElementStarted;
        protected Enum lastAttributeWritten;
        protected Enum lastElementEnded;

		// AS 2/15/11
		// Instead of building a list of these enums each time in the AddBorderProperties, 
		// we can use a static list of them since we are just enumerating the list.
		//
		private static readonly WordProcessingMLElementType[] borderTypes;
		private static readonly TableBorderSides[] borderTypeSides;

		private static readonly string xmlTrueString;
		private static readonly string xmlFalseString;

        private TableBorderProperties   tempBorderProperties = null;

        #endregion Member variables

		#region Constructor
		static WordCustomizablePartExporterBase()
		{
			// AS 2/15/11
			// The borderTypes and borderTypeSides are used in the AddBorderProperties
			// method and must be kept in the same order as each other.
			//
			borderTypes = new WordProcessingMLElementType[] {
				WordProcessingMLElementType.top,
				WordProcessingMLElementType.left,
				WordProcessingMLElementType.bottom,
				WordProcessingMLElementType.right,
				WordProcessingMLElementType.insideH,
				WordProcessingMLElementType.insideV,
			};

			borderTypeSides = new TableBorderSides[] {
				TableBorderSides.Top,
				TableBorderSides.Left,
				TableBorderSides.Bottom,
				TableBorderSides.Right,
				TableBorderSides.InsideH,
				TableBorderSides.InsideV,
			};

			xmlTrueString = OfficeXmlNode.GetXmlString( true, DataType.Boolean );
			xmlFalseString = OfficeXmlNode.GetXmlString( false, DataType.Boolean );
		} 
		#endregion //Constructor

        #region ContentType

		public override string ContentType
		{
			get { throw new NotSupportedException(); }
		}

		#endregion ContentType

		#region RelationshipType

		public override string RelationshipType
		{
			get { throw new NotSupportedException(); }
		}

		#endregion RelationshipType

        #region DefaultTableCellBorderProperties
        internal virtual TableBorderProperties DefaultTableCellBorderProperties { get { return null; } }
        #endregion DefaultTableCellBorderProperties

        #region DefaultTableBorderProperties
        internal virtual TableBorderProperties DefaultTableBorderProperties { get { return null; } }
        #endregion DefaultTableBorderProperties

		#region Save

		public override void Save( OfficeDocumentExportManager manager, Stream contentTypeStream, out bool closeStream, ref bool popCounterStack )
		{
            throw new NotSupportedException();
        }

		#endregion Save

        #region AddNamespaceDeclarations
        protected void AddNamespaceDeclarations( Dictionary<string, string> namespaceDeclarations )
        {
            if ( namespaceDeclarations == null )
                return;

            foreach( KeyValuePair<string, string> pair in namespaceDeclarations )
            {
                this.WriteNamespaceDeclaration( pair.Key, pair.Value );

                //try
                //{
                //    this.textWriter.WriteAttributeString(
                //        OfficeXmlNode.NamespaceDeclarationPrefix,
                //        pair.Key,
                //        OfficeXmlNode.NamespaceDeclarationNamespace,
                //        pair.Value );
                //}
                //catch ( Exception exception )
                //{
                //    this.RaiseXmlWriterException( exception, null, null );
                //}

            }
        }
        #endregion AddNamespaceDeclarations

        #region GetElementName
        
        /// <summary>
        /// Returns the fully qualified name of the XMl element
        /// corresponding to the specified <paramref name="element"/>.
        /// </summary>
        protected override string GetElementName( Enum element, out string ns, out string prefix, out string localName )
        {
            throw new NotSupportedException();
        }

        #endregion GetElementName

        #region AddRunProperties

        protected void AddRunProperties( Font font, bool? checkSpellingAndGrammar, RunStyle runStyle, WordDocumentProperties docProps, Font defaultFont )
        {
            bool hasDocProps = docProps != null;

            #region Font / NoProof

            if ( font != null || checkSpellingAndGrammar.HasValue || runStyle != RunStyle.None || hasDocProps )
            {
                string s = null;

                //  Start the <w:rPr> tag
                this.WriteStartElement( WordProcessingMLElementType.rPr );

                #region <w:shd> - background color
                if ( font != null && WordUtilities.ColorIsEmpty(font.BackColor) == false )
                {
                    this.WriteStartElement( WordProcessingMLElementType.shd );

                    this.WriteAttributeString( WordFontElementType.color, "auto" );

                    s = WordUtilities.ToHexBinary3( font.BackColor );
                    this.WriteAttributeString( WordProcessingMLAttributeType.fill, s );
                    
                    this.WriteEndElement( WordProcessingMLElementType.shd );
                }
                #endregion <w:shd> - background color

                #region <w:noProof>
                if ( checkSpellingAndGrammar.HasValue )
                {
                    bool noProof = (checkSpellingAndGrammar.Value == false);

                    if ( noProof )
                        this.WriteEmptyElement( WordProcessingMLElementType.noProof );
                    else
                    {
                        this.WriteStartElement( WordProcessingMLElementType.noProof );
                        s = OfficeXmlNode.GetXmlString( false, DataType.Boolean );

                        //  TFS70190
                        //  I never added the attribute
                        this.WriteAttributeString( WordProcessingMLAttributeType.val, s );

                        this.WriteEndElement( WordProcessingMLElementType.noProof );
                    }
                }
                #endregion <w:noProof>

                //  Add the <w:rStyle> element if the 'runStyle' parameter indicates we should.
                if ( runStyle != RunStyle.None )
                {
                    s = runStyle.ToString();
                    this.WriteElementString( WordProcessingMLElementType.rStyle, null, s );
                }

                //  Add each of the font tags as required.
                if ( font != null || hasDocProps )
                {
                    #region Name

                    if ( (font != null && font.HasAnyName) || hasDocProps )
                    {
                        //  Start the <w:rFonts> tag
                        this.WriteStartElement( WordFontElementType.rFonts );

                        bool hasName = font != null && string.IsNullOrEmpty(font.Name) == false;
                        bool hasNameEastAsia = font != null && string.IsNullOrEmpty(font.NameEastAsia) == false;
                        bool hasNameComplexScript = font != null && string.IsNullOrEmpty(font.NameComplexScript) == false;
                        bool hasNameHighAnsi = font != null && string.IsNullOrEmpty(font.NameHighAnsi) == false;

                        if ( font != null )
                        {
                            //  <w:ascii>
                            if ( hasName )
                                this.WriteAttributeString( WordFontElementType.ascii, font.Name );

                            //  <w:eastAsia>
                            if ( hasNameEastAsia )
                                this.WriteAttributeString( WordFontElementType.eastAsia, font.NameEastAsia );

                            //  <w:cs>
                            if ( hasNameComplexScript )
                                this.WriteAttributeString( WordFontElementType.cs, font.NameComplexScript );

                            //  <w:hAnsi>
                            if ( hasNameHighAnsi )
                                this.WriteAttributeString( WordFontElementType.hAnsi, font.NameHighAnsi );
                        }

                        //  <w:asciiTheme> et. al.
                        if ( hasDocProps )
                        {
                            //  (Discovered while addressing TFS63993)
                            //  Setting the theme makes the font name get ignored,
                            //  so don't set these attributes if the name is explicitly
                            //  set.

                            if ( hasName == false )
                                this.WriteAttributeString( WordProcessingMLAttributeType.asciiTheme, WordCustomizablePartExporterBase.AsciiThemeAttributeValue );
                            
                            if ( hasNameEastAsia == false )
                                this.WriteAttributeString( WordProcessingMLAttributeType.eastAsiaTheme, WordCustomizablePartExporterBase.EastAsiaThemeAttributeValue );

                            if ( hasNameHighAnsi == false )
                                this.WriteAttributeString( WordProcessingMLAttributeType.hAnsiTheme, WordCustomizablePartExporterBase.HAnsiThemeAttributeValue );

                            if ( hasNameComplexScript == false )
                                this.WriteAttributeString( WordProcessingMLAttributeType.cstheme, WordCustomizablePartExporterBase.ComplexScriptThemeAttributeValue );
                        }

                        //  End the </w:rFonts> tag
                        this.WriteEndElement( WordFontElementType.rFonts );
                    }
                    
                    #endregion Name

                    #region <w:lang>
                    if ( hasDocProps )
                    {
                        this.WriteStartElement( WordProcessingMLElementType.lang );

                        this.WriteAttributeString( WordProcessingMLAttributeType.val, docProps.LatinCulture );
                        this.WriteAttributeString( WordProcessingMLAttributeType.eastAsia, docProps.EastAsiaCulture );
                        this.WriteAttributeString( WordProcessingMLAttributeType.bidi, docProps.ComplexScriptCulture );
                        
                        this.WriteEndElement( WordProcessingMLElementType.lang );
                    }
                    #endregion <w:lang>

                    if ( font != null )
                    {
                        //  If the NameComplexScript property is explicitly set, add the
                        //  <w:bCs>, <w:iCs>, and <w:szCs> elements. Also add them if either
                        //  the UseComplexScript or RightToLeft properties are explicitly set.
                        //
                        //  Note that when you set RightToLeft in Word, they use a complex
                        //  script font by default, which is why I decided to write out the
                        //  complex script font elements when RightToLeft is set.
                        //
                        bool hasComplexScriptName = string.IsNullOrEmpty(font.NameComplexScript) == false;
                        bool addComplexScriptElements =
                            hasComplexScriptName ||
                            (font.UseComplexScript.HasValue && font.UseComplexScript.Value) ||
                            (font.RightToLeft.HasValue && font.RightToLeft.Value);

                        #region UseComplexScript
                        
                        this.WriteNullableBooleanValue( WordFontElementType.cs, font.UseComplexScript );
                        
                        #endregion UseComplexScript

                        #region Size
                        //  <w:sz>, <w:szCs>
                        if ( font.Size.HasValue )
                        {
                            s = OfficeXmlNode.GetXmlString( font.SizeInHalfPoints, DataType.Float );
                            this.WriteElementString( WordFontElementType.sz, null, s );

                            if ( addComplexScriptElements )
                                this.WriteElementString( WordFontElementType.szCs, null, s );
                        }
                        #endregion Size

                        #region Color

                        //  (MSWord) If embossing or engraving is enabled,
                        //  and the color is not set, resolve it to white
                        bool basRelief = font.HasEffects && (font.Effects.TextEffect == FontTextEffect.EmbossingOn || font.Effects.TextEffect == FontTextEffect.EngravingOn);
                        
                        Color foreColor =
                            WordUtilities.ColorIsEmpty(font.ForeColor) == false ?
                            font.ForeColor :
                            basRelief ?
                            WordUtilities.ColorFromArgb(255, 255, 255) :
                            WordUtilities.ColorEmpty;

                        //  TFS70727
                        //
                        //  If the default font has bas relief enabled, the
                        //  run font has it disabled, and neither have a fore
                        //  color specified, make the foreColor black, i.e.,
                        //  the converse of what we do when bas relief is enabled.
                        //
                        if ( defaultFont != null &&
                             WordUtilities.ColorIsEmpty(font.ForeColor) &&
                             WordUtilities.ColorIsEmpty(defaultFont.ForeColor) &&
                             font.HasEffects && defaultFont.HasEffects )
                        {
                            bool noBasRelief = font.Effects.TextEffect == FontTextEffect.EmbossingOff || font.Effects.TextEffect == FontTextEffect.EngravingOff;
                            bool defaultFontHasBasRelief = defaultFont.Effects.TextEffect == FontTextEffect.EmbossingOn || defaultFont.Effects.TextEffect == FontTextEffect.EngravingOn;
                            if ( noBasRelief && defaultFontHasBasRelief )
                            {
                                foreColor = WordUtilities.ColorFromArgb(0, 0, 0);
                            }
                        }

                        //  <w:color>
                        if ( SerializationUtilities.ColorIsEmpty(foreColor) == false )
                        {
                            s = SerializationUtilities.ToHexBinary3( foreColor );
                            this.WriteElementString( WordFontElementType.color, null, s );
                        }
                        #endregion Color

                        #region RightToLeft

                        this.WriteNullableBooleanValue( WordFontElementType.rtl, font.RightToLeft );

                        #endregion RightToLeft

                        #region Bold / Italic / Underline / UnderlineColor

                        //  <w:b>, <w:bCs> (bold)
                        this.WriteNullableBooleanValue( WordFontElementType.b, font.Bold );
                        
                        if ( addComplexScriptElements )
                            this.WriteNullableBooleanValue( WordFontElementType.bCs, font.Bold );
                        
                        //  <w:i>, <w:iCs> (italic)
                        this.WriteNullableBooleanValue( WordFontElementType.i, font.Italic );
                        
                        if ( addComplexScriptElements )
                            this.WriteNullableBooleanValue( WordFontElementType.iCs, font.Italic );
                        
                        //  <w:u> (underline)
                        if ( font.Underline != Underline.Default ||
                             SerializationUtilities.ColorIsEmpty(font.UnderlineColor) == false )
                        {
                            this.WriteStartElement( WordFontElementType.u );

                            if ( font.Underline != Underline.Default )
                            {
                                s = EnumConverter.FromUnderline(font.Underline);
                                this.WriteAttributeString( WordProcessingMLElementType.val, s );
                            }

                            if ( SerializationUtilities.ColorIsEmpty(font.UnderlineColor) == false )
                            {
                                s = SerializationUtilities.ToHexBinary3( font.UnderlineColor );
                                this.WriteAttributeString( WordFontElementType.color, s );
                            }

                            this.WriteEndElement( WordFontElementType.u );
                        }
                        
                        #endregion Bold / Italic / Underline / UnderlineColor

                        #region CharacterSpacing
                        if ( font.HasCharacterSpacing )
                        {
                            FontCharacterSpacing charSpacing = font.CharacterSpacing;

                            if ( charSpacing.Kerning.HasValue )
                            {
                                s = OfficeXmlNode.GetXmlString( charSpacing.KerningInHalfPoints, DataType.Integer );
                                this.WriteElementString( WordProcessingMLElementType.kern, null, s );
                            }

                            if ( charSpacing.Scaling.HasValue )
                            {
                                s = OfficeXmlNode.GetXmlString( charSpacing.Scaling.Value, DataType.Integer );
                                this.WriteElementString( WordProcessingMLElementType.w, null, s );
                            }

                            if ( charSpacing.Spacing.HasValue )
                            {
                                s = OfficeXmlNode.GetXmlString( charSpacing.SpacingInTwips, DataType.Integer );
                                this.WriteElementString( WordProcessingMLElementType.spacing, null, s );
                            }

                            if ( charSpacing.Position.HasValue )
                            {
                                s = OfficeXmlNode.GetXmlString( charSpacing.PositionInHalfPoints, DataType.Integer );
                                this.WriteElementString( WordProcessingMLElementType.position, null, s );
                            }
                        }
                        #endregion CharacterSpacing

                        #region FontEffects
                        if ( font.HasEffects )
                        {
                            FontEffects effects = font.Effects;
                            WordFontElementType? onElement = null;
                            WordFontElementType? offElement = null;

                            #region Capitalization
                            if ( effects.Capitalization != Capitalization.Default )
                            {
                                //  If Capitalization is set on the document-level font,
                                //  we might have to switch it off.
                                switch ( effects.Capitalization )
                                {
                                    case Capitalization.CapsOn:
                                    case Capitalization.SmallCapsOn:
                                    {
                                        if ( defaultFont != null && defaultFont.HasEffects &&
                                             defaultFont.Effects.ShouldSerialize(FontEffects.PropertyIds.Capitalization) )
                                        {
                                            switch ( defaultFont.Effects.Capitalization )
                                            {
                                                case Capitalization.CapsOn:
                                                    offElement = WordFontElementType.caps;
                                                    break;

                                                case Capitalization.SmallCapsOn:
                                                    offElement = WordFontElementType.smallCaps;
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                }

                                switch ( effects.Capitalization )
                                {
                                    case Capitalization.CapsOff:
                                        offElement = WordFontElementType.caps;
                                        break;

                                    case Capitalization.SmallCapsOff:
                                        offElement = WordFontElementType.smallCaps;
                                        break;

                                    case Capitalization.CapsOn:
                                        onElement = WordFontElementType.caps;
                                        break;

                                    case Capitalization.SmallCapsOn:
                                        onElement = WordFontElementType.smallCaps;
                                        break;
                                }

                                if ( onElement.HasValue )
                                    this.WriteElementString( onElement.Value, null, xmlTrueString );
 
                                if ( offElement.HasValue )
                                    this.WriteElementString( offElement.Value, null, xmlFalseString ); 
                            }
                            #endregion Capitalization

                            #region StrikeThrough
                            if ( effects.StrikeThrough != StrikeThrough.Default )
                            {
                                //  If StrikeThrough is set on the document-level font,
                                //  we might have to switch it off.
                                switch ( effects.StrikeThrough )
                                {
                                    case StrikeThrough.SingleOn:
                                    case StrikeThrough.DoubleOn:
                                    {
                                        if ( defaultFont != null && defaultFont.HasEffects &&
                                             defaultFont.Effects.ShouldSerialize(FontEffects.PropertyIds.StrikeThrough) )
                                        {
                                            switch ( defaultFont.Effects.StrikeThrough )
                                            {
                                                case StrikeThrough.SingleOn:
                                                    offElement = WordFontElementType.strike;
                                                    break;

                                                case StrikeThrough.DoubleOn:
                                                    offElement = WordFontElementType.dstrike;
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                }

                                switch ( effects.StrikeThrough )
                                {
                                    case StrikeThrough.SingleOff:
                                        offElement = WordFontElementType.strike;
                                        break;

                                    case StrikeThrough.DoubleOff:
                                        offElement = WordFontElementType.dstrike;
                                        break;

                                    case StrikeThrough.SingleOn:
                                        onElement = WordFontElementType.strike;
                                        break;

                                    case StrikeThrough.DoubleOn:
                                        onElement = WordFontElementType.dstrike;
                                        break;
                                }

                                if ( onElement.HasValue )
                                    this.WriteElementString( onElement.Value, null, xmlTrueString );
 
                                if ( offElement.HasValue )
                                    this.WriteElementString( offElement.Value, null, xmlFalseString ); 
                            }
                            #endregion StrikeThrough

                            #region Hidden

                            if ( effects.Hidden.HasValue )
                            {
                                if ( effects.Hidden.Value )
                                    this.WriteEmptyElement( WordFontElementType.vanish );
                                else
                                {
                                    this.WriteElementString( WordFontElementType.vanish, null, xmlFalseString );
                                }
                            }

                            #endregion Hidden

                            #region TextEffect
                            if ( effects.TextEffect != FontTextEffect.Default )
                            {
                                switch ( effects.TextEffect )
                                {
                                    case FontTextEffect.EngravingOn:
                                    case FontTextEffect.EmbossingOn:
                                    case FontTextEffect.OutliningOn:
                                    {
                                        if ( defaultFont != null && defaultFont.HasEffects &&
                                             defaultFont.Effects.ShouldSerialize(FontEffects.PropertyIds.TextEffect) )
                                        {
                                            switch ( defaultFont.Effects.TextEffect )
                                            {
                                                case FontTextEffect.EngravingOn:
                                                    offElement = WordFontElementType.imprint;
                                                    break;

                                                case FontTextEffect.EmbossingOn:
                                                    offElement = WordFontElementType.emboss;
                                                    break;

                                                case FontTextEffect.OutliningOn:
                                                    offElement = WordFontElementType.outline;
                                                    break;

                                            }
                                        }
                                    }
                                    break;

                                }

                                switch ( effects.TextEffect )
                                {
                                    case FontTextEffect.EmbossingOff:
                                        offElement = WordFontElementType.emboss;
                                        break;

                                    case FontTextEffect.EngravingOff:
                                        offElement = WordFontElementType.imprint;
                                        break;

                                    case FontTextEffect.OutliningOff:
                                        offElement = WordFontElementType.outline;
                                        break;

                                    case FontTextEffect.EmbossingOn:
                                        onElement = WordFontElementType.emboss;
                                        break;

                                    case FontTextEffect.EngravingOn:
                                        onElement = WordFontElementType.imprint;
                                        break;

                                    case FontTextEffect.OutliningOn:
                                        onElement = WordFontElementType.outline;
                                        break;

                                }

                                if ( onElement.HasValue )
                                    this.WriteElementString( onElement.Value, null, xmlTrueString );
 
                                if ( offElement.HasValue )
                                    this.WriteElementString( offElement.Value, null, xmlFalseString );
                            }
                            #endregion TextEffect

                            #region Shadow

                            if ( effects.Shadow.HasValue &&
                                 effects.TextEffect != FontTextEffect.EmbossingOn &&
                                 effects.TextEffect != FontTextEffect.EngravingOn )
                            {
                                if ( effects.Shadow.Value )
                                    this.WriteEmptyElement( WordFontElementType.shadow );
                                else
                                {
                                    this.WriteElementString( WordFontElementType.shadow, null, xmlFalseString );
                                }
                            }

                            #endregion Shadow

                            //  TFS65022
                            #region VerticalAlignment
                            if ( effects.VerticalAlignment != FontVerticalAlignment.Default )
                            {
                                s = EnumConverter.FromFontVerticalAlignment( effects.VerticalAlignment );
                                this.WriteElementString( WordFontElementType.vertAlign, null, s );
                            }
                            #endregion VerticalAlignment

                        }
                        #endregion FontEffects
                    }
                }

                //  Close the </w:rPr> tag
                this.WriteEndElement( WordProcessingMLElementType.rPr );
            }

            #endregion Font / NoProof

        }
        #endregion AddRunProperties

        #region AddCellSpacing
        protected void AddCellSpacing( float cellSpacing, UnitOfMeasurement unit )
        {
            this.WriteStartElement( WordProcessingMLElementType.tblCellSpacing );

            int twips = (int)WordUtilities.ConvertToTwips( unit, cellSpacing );
            string s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );

            //  TFS70699
            //  Word2007 chokes when this is missing, Word2010 handles it
            this.WriteAttributeString( WordProcessingMLAttributeType.type, "dxa" );

            this.WriteEndElement( WordProcessingMLElementType.tblCellSpacing );

        }
        #endregion AddCellSpacing

        #region AddCellMargins
        protected void AddCellMargins( WordProcessingMLElementType parentElement, Padding cellMargins, UnitOfMeasurement unit )
        {
            int twips = 0;
            string s = null;
            string dxa = "dxa";

            this.WriteStartElement( parentElement );

            //  top
            twips = (int)WordUtilities.ConvertToTwips( unit, cellMargins.Top );
            s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
            this.WriteStartElement( WordProcessingMLElementType.top );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
            this.WriteAttributeString( WordProcessingMLAttributeType.type, dxa );
            this.WriteEndElement( WordProcessingMLElementType.top );

            //  left
            twips = (int)WordUtilities.ConvertToTwips( unit, cellMargins.Left );
            s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
            this.WriteStartElement( WordProcessingMLElementType.left );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
            this.WriteAttributeString( WordProcessingMLAttributeType.type, dxa );
            this.WriteEndElement( WordProcessingMLElementType.left );

            //  right
            twips = (int)WordUtilities.ConvertToTwips( unit, cellMargins.Right );
            s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
            this.WriteStartElement( WordProcessingMLElementType.right );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
            this.WriteAttributeString( WordProcessingMLAttributeType.type, dxa );
            this.WriteEndElement( WordProcessingMLElementType.right );

            //  bottom
            twips = (int)WordUtilities.ConvertToTwips( unit, cellMargins.Bottom );
            s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
            this.WriteStartElement( WordProcessingMLElementType.bottom );
            this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
            this.WriteAttributeString( WordProcessingMLAttributeType.type, dxa );
            this.WriteEndElement( WordProcessingMLElementType.bottom );

            this.WriteEndElement( parentElement );

        }
        #endregion AddCellMargins

        #region AddTableStyle
        protected void AddTableStyle( string type, string styleId, bool isDefault, DefaultTableProperties properties )
        {
            //  <w:style>
            this.WriteStartElement( WordStylesElementType.style );

            //  <w:type>
            this.WriteAttributeString( WordStylesAttributeType.type, type );

            //  <w:default>
            if ( isDefault )
                this.WriteAttributeString( WordStylesAttributeType._default, "1" );

            //  <w:styleId>
            this.WriteAttributeString( WordStylesAttributeType.styleId, styleId );

            this.AddTableProperties( properties, true );

            //  BF 3/21/11
            if ( properties != null && properties.ShouldSerializeCellProperties() )
                this.AddTableCellProperties( properties._CellProperties, true );
            
            //  </w:style>
            this.WriteEndElement( WordStylesElementType.style );
        }
        #endregion AddTableStyle

        #region AddTableProperties
        protected void AddTableProperties( TablePropertiesBase properties, bool isStyle )
        {
            string s = null;
            UnitOfMeasurement unit = properties != null ? properties.Unit : WordUtilities.DefaultUnitOfMeasurement;

            //  <w:tblPr>
            this.WriteStartElement( WordProcessingMLElementType.tblPr );

            //  <w:tblStyle>
            if ( isStyle == false )
            {
				this.WriteElementString(WordProcessingMLElementType.tblStyle, null, WordStylesPartExporter.TableStyleId);
            }
            
            //  ***** NOTE *****
            //
            //  Where this element goes matters, at least in relation
            //  to <w:tblStyle>. I wasn't able to get fixed layout tables
            //  working, and it turned out to be that I was adding this
            //  element before the tblStyle one.
            //
            //  <w:tblLayout>
            TableLayout layout = properties != null ? properties.GetLayout() : TableLayout.Auto;
            if ( properties != null && layout != TableLayout.Auto )
            {
                switch ( layout )
                {
                    case TableLayout.Fixed:

                        //  BF 2/3/11
                        string tableWidthUnits = null;
                        int? tableWidth = properties.GetPreferredWidth( out tableWidthUnits );
                        int width = tableWidth.HasValue ? tableWidth.Value : 0;
                        if ( string.IsNullOrEmpty(tableWidthUnits) )
                            tableWidthUnits = "auto";

                        //  <w:tblW>
                        this.WriteStartElement( WordProcessingMLElementType.tblW );

                        //  <w:w>
                        s = OfficeXmlNode.GetXmlString( width, DataType.Int32 );
                        this.WriteAttributeString( WordProcessingMLAttributeType.w, s );

                        //  <w:type>
                        this.WriteAttributeString( WordProcessingMLAttributeType.type, tableWidthUnits );

                        this.WriteEndElement( WordProcessingMLElementType.tblW );
                        
                        //  <w:tblLayout>
                        this.WriteStartElement( WordProcessingMLElementType.tblLayout );
                        this.WriteAttributeString( WordProcessingMLAttributeType.type, "fixed" );
                        this.WriteEndElement( WordProcessingMLElementType.tblLayout );
                        
                        break;
                }
            }

            //  TFS64050
            bool ignoreLeftIndent =
                properties != null &&
                (properties.Alignment == ParagraphAlignment.Right ||
                 properties.Alignment == ParagraphAlignment.Center);

            //  TFS63779
            if ( ignoreLeftIndent == false &&
                 properties != null && properties.LeftIndent.HasValue )
            {
                //  <w:tblInd>
                this.WriteStartElement( WordProcessingMLElementType.tblInd );
                s = OfficeXmlElement.GetXmlString( properties.LeftIndentInTwips, DataType.Int32 );
                this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
                this.WriteAttributeString( WordProcessingMLAttributeType.type, "dxa" );
                this.WriteEndElement( WordProcessingMLElementType.tblInd );
            }

            //  Borders
            if ( (properties != null && properties.ShouldSerializeBorderProperties()) || isStyle )
            {
                TableBorderProperties borderProps = properties != null && properties.ShouldSerializeBorderProperties() ? properties.BorderProperties : null;
                this.AddBorderProperties( borderProps, isStyle, false );
            }

            if ( properties != null && properties.Alignment.HasValue )
            {
                s = EnumConverter.FromParagraphAlignment( properties.Alignment.Value );
                this.WriteElementString( WordProcessingMLElementType.jc, null, s );
            }

            //  TFS63779
            #region Obsolete
            ////  <w:ind>
            //if ( properties != null && properties.LeftIndent.HasValue )
            //{
            //    s = OfficeXmlNode.GetXmlString( properties.LeftIndentInTwips, DataType.Integer );
            //    attribute = this.CreateAttribute( WordProcessingMLAttributeType.left, s );
            //    this.WriteElementString( WordProcessingMLElementType.ind, null, attribute );
            //}
            #endregion Obsolete

            //  <w:tblCellMar>
            if ( (properties != null && properties._CellMargins.HasValue) || isStyle )
            {
                if ( properties != null && properties._CellMargins.HasValue )
                    this.AddCellMargins( WordProcessingMLElementType.tblCellMar, properties._CellMargins.Value, unit );
                else
                {
                    Padding defaultCellMargins = new Padding( 108, 0, 108, 0 );
                    this.AddCellMargins( WordProcessingMLElementType.tblCellMar, defaultCellMargins, UnitOfMeasurement.Twip );
                }
            }

            //  <w:tblCellSpacing>
            if ( properties != null && properties.CellSpacing.HasValue )
                this.AddCellSpacing( properties.CellSpacing.Value, unit );

            //  </w:tblPr>
            this.WriteEndElement( WordProcessingMLElementType.tblPr );
        }
        #endregion AddTableProperties

        #region AddTableCellProperties
        protected void AddTableCellProperties( TableCellPropertiesBase properties, bool isTableStyle )
        {
            string s = null;

            #region <w:tcPr>
            if ( properties != null )
            {
                //  <w:tcPr>
                this.WriteStartElement( WordProcessingMLElementType.tcPr );

                //  <w:shd>
                if ( WordUtilities.ColorIsEmpty(properties.BackColor) == false )
                {
                    this.WriteStartElement( WordProcessingMLElementType.shd );

                    //  BF 3/11/11
                    //  This made no difference (word's compare method finds
                    //  two identical cells different in formatting) but when
                    //  this is done through word's UI they add this attribute,
                    //  so we will too.
                    this.WriteAttributeString( WordProcessingMLAttributeType.val, "clear" );

                    this.WriteAttributeString( WordFontElementType.color, "auto" );

                    s = WordUtilities.ToHexBinary3( properties.BackColor );
                    this.WriteAttributeString( WordProcessingMLAttributeType.fill, s );
                    
                    this.WriteEndElement( WordProcessingMLElementType.shd );
                }

                //  BF 12/23/10
                //  Borders
                if ( properties.ShouldSerializeBorderProperties() )
                    this.AddBorderProperties( properties._BorderProperties, false, true );

                if ( properties._ColumnSpan > 1 )
                {
                    s = OfficeXmlNode.GetXmlString( properties._ColumnSpan, DataType.Integer );
					this.WriteElementString(WordProcessingMLElementType.gridSpan, null, s);
                }

                string widthUnit = null;
                int? width = properties.GetPreferredWidth( out widthUnit );
                if ( width.HasValue )
                {
                    this.WriteStartElement( WordProcessingMLElementType.tcW );
                    
                    this.WriteAttributeString( WordProcessingMLAttributeType.type, widthUnit );

                    s = OfficeXmlNode.GetXmlString( width.Value, DataType.Integer );
                    this.WriteAttributeString( WordProcessingMLAttributeType.w, s );
                    
                    this.WriteEndElement( WordProcessingMLElementType.tcW );
                }

                //  BF 3/21/11
                //
                //  Added 'isTableStyle' to conditional because when it is a table
                //  style, the margins are defined at the table properties level
                //  by the tblCellMargins element, and doing it again here is
                //  supported, but redundant and has no benefit.
                //
                //if ( properties.HasMargins )
                if ( properties.HasMargins && isTableStyle == false )
                    this.AddCellMargins( WordProcessingMLElementType.tcMar, properties.Margins.Value, properties.Unit );

                if ( properties._VerticalMerge != TableCellVerticalMerge.None )
                {
                    s = EnumConverter.FromTableCellVerticalMerge( properties._VerticalMerge );
                    this.WriteElementString( WordProcessingMLElementType.vMerge, null, s );
                }

                //  BF 12/23/10
                if ( properties.VerticalAlignment != TableCellVerticalAlignment.Default )
                {
                    s = EnumConverter.FromTableCellVerticalAlignment( properties.VerticalAlignment );
                    this.WriteElementString( WordProcessingMLElementType.vAlign, null, s );
                }

                if ( properties._TextDirection != TableCellTextDirection.Normal )
                {
                    s = EnumConverter.FromTableCellTextDirection( properties._TextDirection );
					this.WriteElementString(WordProcessingMLElementType.textDirection, null, s);
                }

                //  </w:tcPr>
                this.WriteEndElement( WordProcessingMLElementType.tcPr );
            }
            #endregion </w:tcPr>
        }
        #endregion AddTableCellProperties

        #region AddBorderProperties
        
        protected void AddBorderProperties( TableBorderProperties borderProps, bool isStyle, bool isCell )
        {
            //  BF 3/16/11
            //  Unless this is a table style, there should be no info written out
            //  if no border properties were explicitly set.
            if ( isStyle == false && (borderProps == null || borderProps.ShouldSerialize() == false) )
                return;

            TableBorderProperties properties = null;

            //  BF 3/14/11
            //  Create a clone of the TableBorderProperties that was passed in,
            //  so we aren't changing properties on an instance that was created
            //  by the caller.
            if ( borderProps != null && borderProps.ShouldSerialize() )
            {
                properties = this.GetTempBorderProperties( borderProps );
                TableBorderProperties defaultProps = isCell ? this.DefaultTableCellBorderProperties : this.DefaultTableBorderProperties;
                TableBorderProperties.Merge( defaultProps, properties );
            }

            if ( properties != null || isStyle )
            {
                string s = null;

                //  Note that the border has to be explicitly removed, otherwise it is shown.
				TableBorderSides borderSides = (properties != null ? properties.Sides : null) ?? TableBorderSides.All;

                if ( isCell )
                    //  <w:tcBorders>
                    this.WriteStartElement( WordProcessingMLElementType.tcBorders );
                else
                    //  <w:tblBorders>
                    this.WriteStartElement( WordProcessingMLElementType.tblBorders );

				string width = null;
				string color = null;

				// AS 2/15/11
				// Moved these out of the loop since we only need to do it once since this won't
				// change for each side.
				
                //  BF 6/13/11  TFS73818
                //
                //  If they explicitly set the style we will write it out,
                //  in which case if they did not explicitly set the width,
                //  the text within the cells will be clipped.
                //
				//if ((properties != null && properties.Width.HasValue) || isStyle)
				if ( isStyle ||
                     (properties != null && properties.Width.HasValue) ||
                     (properties != null && properties.Style.HasValue) )

				{
					int widthValue = properties != null && properties.Width.HasValue ? (int)properties.WidthInEighthsOfAPoint : TableBorderProperties.DefaultBorderWidthInEighthsOfAPoint;
					width = OfficeXmlElement.GetXmlString(widthValue, DataType.Integer);
				}

				if ((properties != null && properties.HasColor) || isStyle)
				{
					color = properties != null && properties.HasColor ? WordUtilities.ToHexBinary3(properties.Color) : "auto";
				}

                for( int i = 0; i < borderTypes.Length; i++ )
                {
					WordProcessingMLElementType element = borderTypes[i];
					TableBorderSides side = borderTypeSides[i];
					bool isNil = false;

                    //  If this border side was explicitly stripped out,
                    //  resolve the style to 'None'
					TableBorderStyle? style = (borderSides & side) != side
						? TableBorderStyle.None
						: properties != null ? properties.Style :
                        (TableBorderStyle?)null;
						
                    //  <w:bottom>, etc.
                    this.WriteStartElement( element );

                    if ( (properties != null && style.HasValue) || isStyle )
                    {
                        TableBorderStyle bs = properties != null && style.HasValue ? style.Value : WordUtilities.DefaultTableBorderStyle;

						// if this is a table cell where the side does not need/want a border 
						// but for which other sides may want a border then use nil and don't 
						// bother including the width/color below
						if (isCell && bs == TableBorderStyle.None)
						{
							s = EnumConverter.NilBorder;
							isNil = true;
						}
						else
							s = EnumConverter.FromTableBorderStyle(bs);

                        this.WriteAttributeString( WordProcessingMLAttributeType.val, s );
                    }

					// do not write the width/color if part of the border is excluded
					if (!isNil)
					{
						if (null != width)
							this.WriteAttributeString(WordFontElementType.sz, width);

						if (null != color)
							this.WriteAttributeString(WordFontElementType.color, color);
					}

                    //  </w:bottom>, etc.
                    this.WriteEndElement( element );
                }

                if ( isCell )
                    //  </w:tcBorders>
                    this.WriteEndElement( WordProcessingMLElementType.tcBorders );
                else
                    //  </w:tblBorders>
                    this.WriteEndElement( WordProcessingMLElementType.tblBorders );

            }
        }

        private TableBorderProperties GetTempBorderProperties( TableBorderProperties borderProps )
        {
            if ( this.tempBorderProperties == null )
                this.tempBorderProperties = new TableBorderProperties( null );

            if ( borderProps == null )
            {
                WordUtilities.DebugFail( "Shouldn't be in here (GetTempBorderProperties)" );
                return this.tempBorderProperties;
            }

            this.tempBorderProperties.unitOfMeasurementProvider = borderProps.unitOfMeasurementProvider;
            this.tempBorderProperties.InitializeFrom( borderProps );
            return this.tempBorderProperties;

        }

        #endregion AddBorderProperties

        #region AddRowProperties

        protected void AddRowProperties( TableRowProperties properties )
        {
            int twips = 0;
            string s = null;

            #region <w:trPr>

            float? height = properties != null ? properties.Height : (float?)null;
            RowHeightRule? heightRule = properties != null ? properties.HeightRule : (RowHeightRule?)null;
            float? cellSpacing = properties != null ? properties.CellSpacing : (float?)null;
            bool? isHeaderRow = properties != null ? properties.IsHeaderRow : (bool?)null;
            bool? allowPageBreak = properties != null ? properties.AllowPageBreak : (bool?)null;
            int? cellsBefore = properties != null ? properties.CellsBefore : (int?)null;
            int? cellsAfter = properties != null ? properties.CellsAfter : (int?)null;
            UnitOfMeasurement unit = properties != null ? properties.Unit : WordUtilities.DefaultUnitOfMeasurement;
            
            if ( height.HasValue ||
                 heightRule.HasValue  ||
                 isHeaderRow.HasValue ||
                 cellSpacing.HasValue ||
                 allowPageBreak.HasValue ||
                 cellsBefore.HasValue ||
                 cellsAfter.HasValue )
            {
                //  <w:trPr>
                this.WriteStartElement( WordProcessingMLElementType.trPr );

                #region <w:trHeight>
                
                if ( height.HasValue || heightRule.HasValue )
                {
                    this.WriteStartElement( WordProcessingMLElementType.trHeight );

                    //  <w:val>
                    twips = height.HasValue ? (int)WordUtilities.ConvertToTwips( unit, height.Value ) : 0;
                    s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
                    this.WriteAttributeString( WordProcessingMLAttributeType.val, s );

                    //  <w:hRule>
                    //  If no rule was specified, but a height was, make the rule 'Exact'.
                    //  Otherwise, use either the specified rule or auto.
                    RowHeightRule rule =
                        heightRule.HasValue == false && height.HasValue ?
                        RowHeightRule.Exact :
                        heightRule.HasValue ?
                        heightRule.Value :
                        RowHeightRule.Auto;

                    s = EnumConverter.FromRowHeightRule( rule );
                    this.WriteAttributeString( WordProcessingMLAttributeType.hRule, s );

                    this.WriteEndElement( WordProcessingMLElementType.trHeight );
                }
                #endregion </w:trHeight>

                #region <w:tblHeader>
                
                if ( isHeaderRow.HasValue && isHeaderRow.Value )
                    this.WriteEmptyElement( WordProcessingMLElementType.tblHeader );

                #endregion <w:tblHeader>

                //  TFS63894
                #region <w:cantSplit>

                if ( allowPageBreak.HasValue && allowPageBreak.Value == false )
                    this.WriteEmptyElement( WordProcessingMLElementType.cantSplit );
                
                #endregion <w:cantSplit>

                #region <w:tableCellSpacing>
                
                if ( cellSpacing.HasValue )
                    this.AddCellSpacing( cellSpacing.Value, unit );

                #endregion <w:tableCellSpacing>

                //  TFS66400
                #region <w:gridBefore> / <w:gridAfter>
                
                if ( cellsBefore.HasValue )
                {
                    s = OfficeXmlElement.GetXmlString(cellsBefore.Value, DataType.Integer);
					this.WriteElementString( WordProcessingMLElementType.gridBefore, null, s );
                }

                if ( cellsAfter.HasValue )
                {
                    s = OfficeXmlElement.GetXmlString(cellsAfter.Value, DataType.Integer);
					this.WriteElementString(WordProcessingMLElementType.gridAfter, null, s);
                }

                #endregion <w:gridBefore> / <w:gridAfter>

                //  </w:trPr>
                this.WriteEndElement( WordProcessingMLElementType.trPr );
            }
            #endregion <w:trPr>
        }
        #endregion AddRowProperties

        #region WriteParagraphProperties
        protected void WriteParagraphProperties( ParagraphProperties properties )
        {
            if ( properties == null )
                return;

            int twips = 0;
            string s = null;
            UnitOfMeasurement unitOfMeasure = properties.Unit;

            //  <w:pPr>
            this.WriteStartElement( WordProcessingMLElementType.pPr );

            //  <w:jc>
            if ( properties.Alignment.HasValue )
            {
                s = EnumConverter.FromParagraphAlignment( properties.Alignment.Value );
                this.WriteElementString( WordProcessingMLElementType.jc, null, s );
            }

            //  <w:spacing>
            if ( properties.HasSpacing )
            {
                this.WriteStartElement( WordProcessingMLElementType.spacing );

                if ( properties.SpacingBefore.HasValue )
                {
                    s = OfficeXmlNode.GetXmlString( properties.SpacingBeforeInTwips, DataType.Integer );
                    this.WriteAttributeString( WordProcessingMLAttributeType.before, s );
                }

                if ( properties.SpacingAfter.HasValue )
                {
                    s = OfficeXmlNode.GetXmlString( properties.SpacingAfterInTwips, DataType.Integer );
                    this.WriteAttributeString( WordProcessingMLAttributeType.after, s );
                }


                //  TFS63778
                #region Obsolete
                //if ( properties.LineSpacing.HasValue )
                //{
                //    s = OfficeXmlNode.GetXmlString( properties.LineSpacingInTwips, DataType.Integer );
                //    this.WriteAttributeString( WordProcessingMLAttributeType.line, s );

                //    s = EnumConverter.FromLineSpacingRule( properties.LineSpacingRule );
                //    this.WriteAttributeString( WordProcessingMLAttributeType.lineRule, s );
                //}
                #endregion Obsolete

                LineSpacingRule? lineSpacingRule = null;
                int lineSpacing = properties.GetLineSpacing( out lineSpacingRule );
                if ( lineSpacingRule.HasValue )
                {
                    s = OfficeXmlNode.GetXmlString( lineSpacing, DataType.Integer );
                    this.WriteAttributeString( WordProcessingMLAttributeType.line, s );

                    s = EnumConverter.FromLineSpacingRule( lineSpacingRule.Value );
                    this.WriteAttributeString( WordProcessingMLAttributeType.lineRule, s );
                }

                this.WriteEndElement( WordProcessingMLElementType.spacing );
            }

            //  <w:ind>
            if ( properties.LeftIndent.HasValue || properties.RightIndent.HasValue )
            {
				this.WriteStartElement(WordProcessingMLElementType.ind);

                if ( properties.LeftIndent.HasValue )
                {
                    twips = properties.LeftIndentInTwips;
                    s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
					this.WriteAttributeString( WordProcessingMLAttributeType.left, s );
                }

                if ( properties.RightIndent.HasValue )
                {
                    twips = properties.RightIndentInTwips;
                    s = OfficeXmlNode.GetXmlString( twips, DataType.Integer );
					this.WriteAttributeString(WordProcessingMLAttributeType.right, s);
                }

                this.WriteEndElement( WordProcessingMLElementType.ind );
            }

            //  <w:pageBreakBefore>
            if ( properties.PageBreakBefore.HasValue )
            {
                if ( properties.PageBreakBefore.Value )
                    this.WriteEmptyElement( WordProcessingMLElementType.pageBreakBefore );
                else
                {
					this.WriteElementString( WordProcessingMLElementType.pageBreakBefore, null, properties.PageBreakBefore.Value ? xmlTrueString : xmlFalseString );
                }
            }

            //  <w:bidi>
            this.WriteNullableBooleanValue( WordProcessingMLAttributeType.bidi, properties.RightToLeft );
            
            //  </w:pPr>
            this.WriteEndElement( WordProcessingMLElementType.pPr );
        }
        #endregion WriteParagraphProperties

        #region Dispose
        public virtual void Dispose()
        {
            //  BF 4/6/11 (Unit testing)
            //
            //  There is no way to tell if a stream has been disposed
            //  of (although I suppose if CanRead and CanWrite both
            //  return false we could consider that to mean the same
            //  thing), so wrap this up and catch ObjectDisposedExceptions.
            //
            try
            {
                if ( this.textWriter != null )
                    this.textWriter.Close();

                if ( this.contentStream != null )
                    this.contentStream.Dispose();
            }
            catch ( ObjectDisposedException )
            {
            }
            finally
            {
                this.textWriter = null;
                this.contentStream = null;
            }
        }
        #endregion Dispose

        #region XmlException event

        internal event WordPartExporterXmlExceptionHandler XmlException;
        
        #endregion XmlException event

        #region XmlWriter helpers

        protected void WriteStartElement( Enum element )
        {
            string prefix = null;
            string ns = null;
            string localName = null;

            this.GetElementName( element, out ns, out prefix, out localName );

            try { this.textWriter.WriteStartElement( prefix, localName, ns ); }
            catch ( Exception exception )
            {
                this.RaiseXmlWriterException( exception, element, null );
            }

            this.lastElementStarted = element;
        }

        protected void WriteElementValue( Enum element, object value, DataType dataType )
        {
            string s = OfficeXmlNode.GetXmlString( value, dataType );
            this.WriteElementString( element, s );
        }

        protected void WriteString( string value )
        {
            this.textWriter.WriteString( value );
        }

        protected void WriteElementString( Enum element, string value )
        {
            string prefix = null;
            string ns = null;
            string localName = null;

            this.GetElementName( element, out ns, out prefix, out localName );
            try { this.textWriter.WriteElementString( prefix, localName, ns, value ); }
            catch ( Exception exception )
            {
                this.RaiseXmlWriterException( exception, element, null );
            }
            
            this.lastElementStarted = element;
            this.lastElementEnded = element;
        }

		protected void WriteElementString(Enum element, string value, string valAttributeValue)
		{
			this.WriteStartElement(element);
			this.WriteAttributeString(WordProcessingMLAttributeType.val, valAttributeValue);

			if (string.IsNullOrEmpty(value) == false)
				this.WriteString(value);

			this.WriteEndElement(element);
		}

		protected void WriteElementString(Enum element, string value, SimpleXmlAttribute attribute)
        {
            this.WriteElementString( element, value, new SimpleXmlAttribute[]{ attribute } );
        }

        protected void WriteElementString( Enum element, string value, IList<SimpleXmlAttribute> attributes )
        {
            this.WriteStartElement( element );

            foreach( SimpleXmlAttribute attribute in attributes )
            {
                try
                {
                    this.textWriter.WriteAttributeString(
                        attribute.Prefix,
                        attribute.LocalName,
                        attribute.Namespace,
                        attribute.Value );
                }
                catch ( Exception exception )
                {
                    this.RaiseXmlWriterException( exception, element, attribute.Attribute );
                }

            }

            if ( string.IsNullOrEmpty(value) == false )
                this.WriteString( value );

            this.WriteEndElement( element );
        }

        protected void WriteAttributeValue( Enum attribute, object value, DataType dataType )
        {
            string s = OfficeXmlNode.GetXmlString( value, dataType );
            this.WriteAttributeString( attribute, s );
        }

        protected void WriteAttributeString( Enum attribute, string value )
        {
            string prefix = null;
            string ns = null;
            string localName = null;

            this.GetElementName( attribute, out ns, out prefix, out localName );

            try { this.textWriter.WriteAttributeString( prefix, localName, ns, value ); }
            catch ( Exception exception )
            {
                this.RaiseXmlWriterException( exception, null, attribute );
            }

            this.lastAttributeWritten = attribute;
        }

        protected void WriteEndElement( Enum element )
        {
            try { this.textWriter.WriteEndElement(); }            
            catch ( Exception exception )
            {
                this.RaiseXmlWriterException( exception, element, null );
            }

            this.lastElementEnded = element;
        }

        protected void WriteEmptyElement( Enum element )
        {
            this.WriteStartElement( element );
            this.WriteEndElement( element );
        }

        protected void WriteNullableBooleanValue( Enum element, bool? value )
        {
            if ( value.HasValue == false )
                return;

            if ( value.Value )
                this.WriteEmptyElement( element );
            else
            {
                //  TFS70725
                //this.WriteElementString( WordFontElementType.color, null, value.Value ? xmlTrueString : xmlFalseString );
                this.WriteElementString( element, null, value.Value ? xmlTrueString : xmlFalseString );
            }
        }

        protected List<SimpleXmlAttribute> CreateAttributeList( Enum attribute, string value )
        {
            SimpleXmlAttribute retVal = this.CreateAttribute( attribute, value );
            return new List<SimpleXmlAttribute>( new SimpleXmlAttribute[]{ retVal } );
        }

        protected SimpleXmlAttribute CreateAttribute( Enum attribute, string value )
        {
            string prefix = null;
            string ns = null;
            string localName = null;

            this.GetElementName( attribute, out ns, out prefix, out localName );

            return new SimpleXmlAttribute( attribute, prefix, localName, ns, value );
        }

        protected void WriteNamespaceDeclaration( string prefix, string ns )
        {
            try
            {
                this.textWriter.WriteAttributeString(
                    OfficeXmlNode.NamespaceDeclarationPrefix,
                    prefix,
                    OfficeXmlNode.NamespaceDeclarationNamespace,
                    ns );
            }
            catch ( Exception exception )
            {
                this.RaiseXmlWriterException( exception, null, null );
            }

        }

        private void RaiseXmlWriterException( Exception innerException, Enum element, Enum attribute )
        {
            if ( this.XmlException != null )
            {
                string[] unused = new string[]{ null, null, null };
                string elementName = element == null ? null : this.GetElementName( element, out unused[0], out unused[1], out unused[2] );
                string attributeName = attribute == null ? null : this.GetElementName( attribute, out unused[0], out unused[1], out unused[2] );

                WordPartExporterXmlExceptionEventArgs args =
                    WordPartExporterXmlExceptionEventArgs.Create(
                    innerException,
                    elementName,
                    attributeName );

                this.XmlException( this, args );
            }
        }

        #endregion XmlWriter helpers

        #region CreateXmlWriter
        protected void CreateXmlWriter( Stream contentTypeStream, bool writeProcessingInstruction )
        {
            //  Create the writer, write the XML header
            this.textWriter = XmlWriter.Create( contentTypeStream );
            string declaration = SerializationUtilities.GetXmlDeclaration( ContentTypeBase.XmlVersion, ContentTypeBase.XmlEncoding, ContentTypeBase.XmlStandalone );

            if ( writeProcessingInstruction )
		        this.textWriter.WriteProcessingInstruction( ContentTypeBase.XmlName, declaration );
        }
        #endregion CreateXmlWriter

        #region SimpleXmlAttribute class
        internal class SimpleXmlAttribute
        {
            Enum attribute = null;
            string prefix = null;
            string ns = null;
            string localName = null;
            string value = null;

            internal SimpleXmlAttribute( Enum attribute, string prefix, string localName, string ns, string value )
            {
                this.attribute = attribute;
                this.prefix = prefix;
                this.localName = localName;
                this.ns = ns;
                this.value = value;
            }

            internal Enum Attribute { get { return this.attribute; } }
            internal string Prefix { get { return this.prefix; } }
            internal string LocalName { get { return this.localName; } }
            internal string Namespace { get { return this.ns; } }
            internal string Value { get { return this.value; } }
        }
        #endregion SimpleXmlAttribute class
    }

    #endregion WordCustomizablePartExporterBase class

    #region WordPartExporterXmlExceptionEventArgs class
    internal class WordPartExporterXmlExceptionEventArgs : EventArgs
    {
        Exception innerException = null;
        string element = null;
        string attribute = null;

        private WordPartExporterXmlExceptionEventArgs( Exception innerException, string element, string attribute )
        {
            this.innerException = innerException;
            this.element = element;
            this.attribute = attribute;
        }

        static internal WordPartExporterXmlExceptionEventArgs Create( Exception innerException, string element, string attribute )
        {
            return new WordPartExporterXmlExceptionEventArgs( innerException, element, attribute );
        }

        /// <summary>
        /// Returns a reference to the exception that was thrown by the XmlWriter.
        /// </summary>
        internal Exception InnerException { get { return this.innerException; } }
        
        /// <summary>
        /// Returns the name of the XML element that caused the exception
        /// or null if there is no associated XML element.
        /// </summary>
        internal string Element { get { return this.element; } }
        
        /// <summary>
        /// Returns the name of the XML attribute that caused the exception
        /// or null if there is no associated XML attribute.
        /// </summary>
        internal string Attribute { get { return this.attribute; } }
    }
    #endregion WordPartExporterXmlExceptionEventArgs class

    #region WordPartExporterXmlExceptionHandler
    internal delegate void WordPartExporterXmlExceptionHandler( object sender, WordPartExporterXmlExceptionEventArgs e );
    #endregion WordPartExporterXmlExceptionHandler
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