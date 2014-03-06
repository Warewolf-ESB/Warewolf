using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Core;


using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region Font class
    /// <summary>
    /// Provides a way to customize the visual attributes of a character or range of characters.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The Font class exposes properties like Bold, Italic, Underline, etc., which
    /// enable the developer to format characters in the same manner as a user of MS Word.
    /// </p>
    /// </remarks>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class Font
    {
        #region Constants

        /// <summary>
        /// The maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.Font.Size">Size</see>
        /// property, expressed in twips (32760)
        /// </summary>
        public const float SizeMaxValueTwips = 32760;

        #endregion Constants

        #region Member variables

        private IUnitOfMeasurementProvider  unitOfMeasurementProvider = null;
        private bool?                       bold = null;
        private Color                       foreColor = WordUtilities.ColorEmpty;
        private Color                       backColor = WordUtilities.ColorEmpty;
        private FontCharacterSpacing        characterSpacing = null;
        private FontEffects                 effects = null;
        private bool?                       italic = null;
        private string                      name = null;
        private string                      nameComplexScript = null;
        private string                      nameEastAsia = null;
        private string                      nameHighAnsi = null;
        private float?                      size = null;
        private Underline                   underline = Underline.Default;
        private Color                       underlineColor = WordUtilities.ColorEmpty;
        private bool?                       rightToLeft = null;
        private bool?                       useComplexScript = null;

        #endregion Member variables

        #region Constructor

        ///// <summary>
        ///// Creates a new instance of the class.
        ///// </summary>
        ///// <param name="name">The initial value of the Name property.</param>
        ///// <param name="size">The initial value of the Size property.</param>
        //public Font( string name, float size ) : this( null )
        //{
        //    this.name = name;
        //    this.size = size;
        //}

        internal Font( IUnitOfMeasurementProvider unitOfMeasurementProvider )
        {
            this.unitOfMeasurementProvider = unitOfMeasurementProvider;
        }

        #endregion Constructor

        #region Properties

        #region Unit
        internal UnitOfMeasurement Unit
        {
            get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; }
        }
        #endregion Unit

        #region Bold
        /// <summary>
        /// Returns or sets a boolean value indicating whether the font is bolded.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// <p class="body">
        /// This property can be restored to its default value by setting it to null.
        /// </p>
        /// </remarks>
        public bool? Bold
        {
            get { return this.bold; }
            set { this.bold = value; }
        }
        #endregion Bold

        #region CharacterSpacing
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing">FontCharacterSpacing</see>
        /// instance which provides a way to customize spacing and kerning for a
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// To reduce the memory footprint, creation of the FontCharacterSpacing object
        /// returned from this property is deferred until it is requested publicly
        /// by a consumer. To avoid triggering creation, use the
        /// <see cref="Infragistics.Documents.Word.Font.HasCharacterSpacing">HasCharacterSpacing</see>
        /// property before accessing this property.
        /// </p>
        /// </remarks>
        public FontCharacterSpacing CharacterSpacing
        {
            get
            {
                if ( this.characterSpacing == null )
                    this.characterSpacing = new FontCharacterSpacing( this );

                return this.characterSpacing;
            }
        }
        
        /// <summary>
        /// Returns a boolean value indicating whether the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing">FontCharacterSpacing</see>
        /// instance returned from the 
        /// <see cref="Infragistics.Documents.Word.Font.CharacterSpacing">CharacterSpacing</see>
        /// property has been created.
        /// </summary>
        public bool HasCharacterSpacing { get { return this.characterSpacing != null && this.characterSpacing.ShouldSerialize(); } }

        #endregion CharacterSpacing

        #region ForeColor
        /// <summary>
        /// Returns or sets the color of the text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default color value can be reinstated by setting this property to Color.Empty.
        /// </p>
        /// </remarks>
        public Color ForeColor
        {
            get { return this.foreColor; }
            set { this.foreColor = value; }
        }
        #endregion ForeColor

        #region BackColor
        /// <summary>
        /// Returns or sets the background color that is applied
        /// to the area behind the text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default color value can be reinstated by setting this property to Color.Empty.
        /// </p>
        /// <p class="body">
        /// By default, no color is applied to the area behind the text.
        /// Use this property to fill the rectangle which encloses the text
        /// area with a particular color, i.e., to "highlight" the text.
        /// </p>
        /// </remarks>
        public Color BackColor
        {
            get { return this.backColor; }
            set { this.backColor = value; }
        }
        #endregion BackColor

        #region Effects
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.FontEffects">FontEffects</see>
        /// instance which provides extended formatting functionality
        /// for the font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// To reduce the memory footprint, creation of the FontEffects object
        /// returned from this property is deferred until it is requested publicly
        /// by a consumer. To avoid triggering creation, use the
        /// <see cref="Infragistics.Documents.Word.Font.HasEffects">HasEffects</see>
        /// property before accessing this property.
        /// </p>
        /// </remarks>
        public FontEffects Effects
        {
            get
            {
                if ( this.effects == null )
                    this.effects = new FontEffects( this );

                return this.effects;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether the
        /// <see cref="Infragistics.Documents.Word.FontEffects">FontEffects</see>
        /// instance returned from the 
        /// <see cref="Infragistics.Documents.Word.Font.Effects">Effects</see>
        /// property has been created.
        /// </summary>
        public bool HasEffects { get { return this.effects != null && this.effects.ShouldSerialize(); } }

        #endregion Effects

        #region Italic
        /// <summary>
        /// Returns or sets a boolean value indicating whether the font is italicized.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// <p class="body">
        /// This property can be restored to its default value by setting it to null.
        /// </p>
        /// </remarks>
        public bool? Italic
        {
            get { return this.italic; }
            set { this.italic = value; }
        }
        #endregion Italic

        #region Name
        /// <summary>
        /// Returns or sets the name of the font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// <p class="body">
        /// This property can be restored to its default value by setting it to null or an empty string.
        /// </p>
        /// <p class="body">
        /// ASCII characters within the range of 0 through 127 are displayed
        /// with the font associated with the value of this property. Other fonts
        /// can be designated for use with character sets outside the standard
        /// ASCII range using the following properties:
        /// <ul>
        ///     <li><see cref="Infragistics.Documents.Word.Font.NameComplexScript">NameComplexScript</see></li>
        ///     <li><see cref="Infragistics.Documents.Word.Font.NameEastAsia">NameEastAsia</see></li>
        ///     <li><see cref="Infragistics.Documents.Word.Font.NameHighAnsi">NameHighAnsi</see></li>
        /// </ul>
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Font.NameComplexScript">NameComplexScript</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Font.NameEastAsia">NameEastAsia</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Font.NameHighAnsi">NameHighAnsi</seealso>
        public string Name
        {
            get { return WordUtilities.StringPropertyGetHelper(this.name); }
            set { this.name = value; }
        }
        #endregion Name

        #region NameComplexScript
        /// <summary>
        /// Returns or sets the font which is used to format all characters
        /// in a complex script Unicode range within the associated run.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If this property is not explicitly set, the text is displayed in
        /// a default font which supports complex script content.
        /// </p>
        /// </remarks>
        public string NameComplexScript
        {
            get { return WordUtilities.StringPropertyGetHelper(this.nameComplexScript); }
            set { this.nameComplexScript = value; }
        }
        #endregion NameComplexScript

        #region NameEastAsia
        /// <summary>
        /// Returns or sets the font which is used to format all characters
        /// in an East Asian Unicode range within the associated run.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If this property is not explicitly set, the text is displayed in
        /// a default font which supports East Asian content.
        /// </p>
        /// </remarks>
        public string NameEastAsia
        {
            get { return WordUtilities.StringPropertyGetHelper(this.nameEastAsia); }
            set { this.nameEastAsia = value; }
        }
        #endregion NameEastAsia

        #region NameHighAnsi
        /// <summary>
        /// Returns or sets the font which is used to format all characters
        /// in a Unicode range within the associated run which does not fall
        /// into any of the categories defined by the
        /// <see cref="Infragistics.Documents.Word.Font.Name">Name</see>,
        /// <see cref="Infragistics.Documents.Word.Font.NameComplexScript">NameComplexScript</see>,
        /// or
        /// <see cref="Infragistics.Documents.Word.Font.NameEastAsia">NameEastAsia</see>
        /// properties.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// If this property is not explicitly set, the text is displayed in
        /// a default font which supports high ANSI content.
        /// </p>
        /// </remarks>
        public string NameHighAnsi
        {
            get { return WordUtilities.StringPropertyGetHelper(this.nameHighAnsi); }
            set { this.nameHighAnsi = value; }
        }
        #endregion NameHighAnsi

        #region Size
        /// <summary>
        /// Returns or sets the size at which the font is displayed.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value assigned to this property must be a value between the
        /// range of 1 and 1638 DTPs (desktop publishing points), inclusive.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// <p class="body">
        /// This property can be restored to its default value by setting it to null.
        /// </p>
        /// </remarks>
        public float? Size
        {
            get { return this.size.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.size.Value ) : this.size ; }
            set { this.size = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value; }
        }

        internal float? SizeInHalfPoints{ get { return this.size.HasValue ? this.size.Value / 10f : (float?)null; } }

        #endregion Size

        #region Underline
        /// <summary>
        /// Returns or sets a value indicating the manner in which the font is underlined.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// </remarks>
        public Underline Underline
        {
            get { return this.underline; }
            set { this.underline = value; }
        }
        #endregion Underline

        #region UnderlineColor
        /// <summary>
        /// Returns or sets the color of the line drawn under the text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The default underline color value can be reinstated by setting this property to Color.Empty.
        /// </p>
        /// </remarks>
        public Color UnderlineColor
        {
            get { return this.underlineColor; }
            set { this.underlineColor = value; }
        }
        #endregion UnderlineColor

        #region HasAnyName
        internal bool HasAnyName
        {
            get
            {
                return
                    String.IsNullOrEmpty(this.name) == false ||
                    String.IsNullOrEmpty(this.nameComplexScript) == false ||
                    String.IsNullOrEmpty(this.nameEastAsia) == false ||
                    String.IsNullOrEmpty(this.nameHighAnsi) == false;
            }
        }
        #endregion HasAnyName

        #region RightToLeft
        /// <summary>
        /// Returns or sets a value which determines the reading order for the
        /// associated run of text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This setting determines the manner in which the run contents are presented
        /// in the document when punctuation characters are part of the run's contents.
        /// When this property is specified, each part of the run between a punctuation
        /// mark is laid out in a right-to-left direction on the line.
        /// </p>
        /// <p class="body">
        /// Typically the developer will also set the RightToLeft property on the
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties">ParagraphProperties</see>
        /// instance for the paragraph to which this run belongs when using this property.
        /// </p>
        /// <p class="body">
        /// Setting this property to true will cause the serialization layer to add
        /// complex script XML elements for the
        /// <see cref="Infragistics.Documents.Word.Font.Bold">Bold</see>,
        /// <see cref="Infragistics.Documents.Word.Font.Italic">Italic</see>,
        /// and
        /// <see cref="Infragistics.Documents.Word.Font.Size">Size</see>
        /// properties to the output document. This behavior can be overridden
        /// by explicitly setting the 
        /// <see cref="Infragistics.Documents.Word.Font.UseComplexScript">UseComplexScript</see>
        /// property to false.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Font.UseComplexScript">UseComplexScript</seealso>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.RightToLeft">RightToLeft (ParagraphProperties class)</seealso>
        public bool? RightToLeft
        {
            get { return this.rightToLeft; }
            set { this.rightToLeft = value; }
        }
        #endregion RightIndent

        #region UseComplexScript
        /// <summary>
        /// Returns or sets a value which determines whether the contents of this run
        /// shall be treated as complex script text regardless of the Unicode character
        /// values contained therein when determining the formatting for this run.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML uses different XML elements to express the bold, italic,
        /// and size attributes of the font for standard and complex scripts. When the
        /// <see cref="Infragistics.Documents.Word.Font.NameComplexScript">NameComplexScript</see>
        /// property is explicitly set, the complex scripts elements are automatically added.
        /// </p>
        /// <p class="body">
        /// This property causes the text contained within the associated run to be treated
        /// as complex script, effectively resolving any ambiguities that may arise by virtue
        /// of the bold, italic, or size attributes being set for both standard and complex
        /// scripts.
        /// </p>
        /// </remarks>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public bool? UseComplexScript
        {
            get { return this.useComplexScript; }
            set { this.useComplexScript = value; }
        }
        #endregion RightIndent

        #endregion Properties

        #region Methods

        #region Create
        /// <summary>
        /// Returns a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// instance.
        /// </param>
        /// <returns>
        /// A new
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// instance.
        /// </returns>
        public static Font Create(WordDocumentWriter writer)
        {
            return new Font(writer);
        }
        #endregion Create

        #region ToString
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if ( this.ShouldSerialize(PropertyIds.Name) )
                sb.Append( string.Format("{0} ", this.Name) );

            if ( this.ShouldSerialize(PropertyIds.Size) && this.Size.Value > 0f )
                sb.Append( string.Format("Size={0} ", this.Size.Value.ToString("0.00")) );

            if ( this.ShouldSerialize(PropertyIds.Bold) && this.Bold.Value )
                sb.Append( string.Format("{0} ", "Bold") );

            if ( this.ShouldSerialize(PropertyIds.Italic) && this.Italic.Value )
                sb.Append( string.Format("{0} ", "Italic") );

            if ( this.ShouldSerialize(PropertyIds.Underline) )
                sb.Append( string.Format("Underline={0} ", this.Underline) );

            return sb.Length > 0 ? sb.ToString() : base.ToString();

        }
        #endregion ToString

        #endregion Methods

        #region Internal

        #region ShouldSerialize

        internal bool ShouldSerialize()
        {
            return this.ShouldSerialize( PropertyIds.All );
        }

        internal bool ShouldSerialize( PropertyIds propId )
        {
            if ( (propId & PropertyIds.Bold) == PropertyIds.Bold &&
                 this.Bold.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.CharacterSpacing) == PropertyIds.CharacterSpacing &&
                 this.HasCharacterSpacing &&
                 this.CharacterSpacing.ShouldSerialize() )
                return true;
                 
            if ( (propId & PropertyIds.ForeColor) == PropertyIds.ForeColor &&
				 WordUtilities.ColorIsEmpty(this.ForeColor) == false)
                return true;
                 
            if ( (propId & PropertyIds.BackColor) == PropertyIds.BackColor &&
				 WordUtilities.ColorIsEmpty(this.BackColor) == false)
                return true;
                 
            if ( (propId & PropertyIds.Effects) == PropertyIds.Effects &&
                 this.HasEffects &&
                 this.Effects.ShouldSerialize() )
                return true;

            if ( (propId & PropertyIds.Italic) == PropertyIds.Italic &&
                 this.Italic.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.Name) == PropertyIds.Name &&
                 string.IsNullOrEmpty(this.Name) == false )
                return true;
                 
            if ( (propId & PropertyIds.NameComplexScript) == PropertyIds.NameComplexScript &&
                 string.IsNullOrEmpty(this.NameComplexScript) == false )
                return true;
                 
            if ( (propId & PropertyIds.NameEastAsia) == PropertyIds.NameEastAsia &&
                 string.IsNullOrEmpty(this.NameEastAsia) == false )
                return true;
                 
            if ( (propId & PropertyIds.NameHighAnsi) == PropertyIds.NameHighAnsi &&
                 string.IsNullOrEmpty(this.NameHighAnsi) == false )
                return true;
                 
            if ( (propId & PropertyIds.Size) == PropertyIds.Size &&
                 this.size.HasValue && this.size.Value > 0f )
                return true;
                 
            if ( (propId & PropertyIds.Underline) == PropertyIds.Underline &&
                 this.Underline != Underline.Default )
                return true;
                 
            if ( (propId & PropertyIds.UnderlineColor) == PropertyIds.UnderlineColor &&
				 WordUtilities.ColorIsEmpty(this.UnderlineColor) == false)
                return true;
                                                  
            if ( (propId & PropertyIds.RightToLeft) == PropertyIds.RightToLeft &&
				 this.rightToLeft.HasValue )
                return true;
                                                  
            if ( (propId & PropertyIds.UseComplexScript) == PropertyIds.UseComplexScript &&
				 this.useComplexScript.HasValue )
                return true;
                                                  
            return false;
        }

        #endregion ShouldSerialize

        #region MergeProperties
        /// <summary>
        /// Merges the property values that are explicitly set on this instance
        /// into the same properties on the <paramref name="destination"/> instance
        /// if they are not explicitly set on the destination instance.
        /// </summary>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal void MergeProperties( ref Font destination )
        {
            Font.MergeProperties( this, ref destination );
        }

        /// <summary>
        /// Merges the property values that are explicitly set on the
        /// <paramref name="source"/> instance into the same properties
        /// on the <paramref name="destination"/> instance if they are
        /// not explicitly set on the destination instance.
        /// </summary>
        /// <param name="source">The instance from which the property values are copied.</param>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal static void MergeProperties( Font source, ref Font destination )
        {
            if ( source == null || destination == null )
                return;

            if ( source.ShouldSerialize(PropertyIds.Bold) &&
                 destination.ShouldSerialize(PropertyIds.Bold) == false )
                destination.bold = source.bold.Value;

            if ( source.ShouldSerialize(PropertyIds.CharacterSpacing) )
            {
                FontCharacterSpacing characterSpacing = destination.CharacterSpacing;
                FontCharacterSpacing.MergeProperties( source.CharacterSpacing, ref characterSpacing );
            }

            if ( source.ShouldSerialize(PropertyIds.ForeColor) &&
                 destination.ShouldSerialize(PropertyIds.ForeColor) == false )
                destination.foreColor = source.foreColor;

            //  TFS70726
            if ( source.ShouldSerialize(PropertyIds.BackColor) &&
                 destination.ShouldSerialize(PropertyIds.BackColor) == false )
                destination.backColor = source.backColor;

            if ( source.ShouldSerialize(PropertyIds.Effects) )
            {
                FontEffects effects = destination.Effects;
                FontEffects.MergeProperties( source.Effects, ref effects );
            }

            if ( source.ShouldSerialize(PropertyIds.Italic) &&
                 destination.ShouldSerialize(PropertyIds.Italic) == false )
                destination.italic = source.italic.Value;

            if ( source.ShouldSerialize(PropertyIds.Name) &&
                 destination.ShouldSerialize(PropertyIds.Name) == false )
                destination.name = source.name;

            if ( source.ShouldSerialize(PropertyIds.NameComplexScript) &&
                 destination.ShouldSerialize(PropertyIds.NameComplexScript) == false )
                destination.nameComplexScript = source.nameComplexScript;

            if ( source.ShouldSerialize(PropertyIds.NameEastAsia) &&
                 destination.ShouldSerialize(PropertyIds.NameEastAsia) == false )
                destination.nameEastAsia = source.nameEastAsia;

            if ( source.ShouldSerialize(PropertyIds.NameHighAnsi) &&
                 destination.ShouldSerialize(PropertyIds.NameHighAnsi) == false )
                destination.nameHighAnsi = source.nameHighAnsi;

            if ( source.ShouldSerialize(PropertyIds.RightToLeft) &&
                 destination.ShouldSerialize(PropertyIds.RightToLeft) == false )
                destination.rightToLeft = source.rightToLeft;

            if ( source.ShouldSerialize(PropertyIds.Size) &&
                 destination.ShouldSerialize(PropertyIds.Size) == false )
                destination.size = source.size.Value;

            if ( source.ShouldSerialize(PropertyIds.Underline) &&
                 destination.ShouldSerialize(PropertyIds.Underline) == false )
                destination.underline = source.underline;

            if ( source.ShouldSerialize(PropertyIds.UnderlineColor) &&
                 destination.ShouldSerialize(PropertyIds.UnderlineColor) == false )
                destination.underlineColor = source.underlineColor;

            if ( source.ShouldSerialize(PropertyIds.UseComplexScript) &&
                 destination.ShouldSerialize(PropertyIds.UseComplexScript) == false )
                destination.useComplexScript = source.useComplexScript;

        }
        #endregion MergeProperties

        #region GetResolvedFont


#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

        static internal Font GetResolvedFont( List<Font> fonts )
        {
            if ( fonts == null || fonts.Count == 0 )
                return null;

            //  First determine whether any of the fonts have
            //  anything set on them; if they don't we'll just
            //  return null right away.
            Font retVal = null;
            foreach( Font font in fonts )
            {
                if ( font != null && font.ShouldSerialize() )
                {
                    retVal = new Font( null );
                    break;
                }
            }

            if ( retVal == null )
                return null;

            //  Merge in the properties that are set on the
            //  specified font, in the same order in which they
            //  appear in the list. Note that the closer the font
            //  is to the beginning of the list, the more weight
            //  it has in the returned font.
            foreach( Font font in fonts )
            {
                Font.MergeProperties( font, ref retVal );
            }

            return retVal;
        }
        #endregion GetResolvedFont

        #region Reset

        /// <summary>
        /// Restores all property values to their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.Reset( PropertyIds.All );
        }

        internal void Reset( PropertyIds propId )
        {
            if ( (propId & PropertyIds.Bold) == PropertyIds.Bold &&
                 this.Bold.HasValue )
                this.Bold = null;
                 
            if ( (propId & PropertyIds.CharacterSpacing) == PropertyIds.CharacterSpacing &&
                 this.characterSpacing != null )
               this.characterSpacing.Reset();
                 
            if ( (propId & PropertyIds.ForeColor) == PropertyIds.ForeColor &&
				 WordUtilities.ColorIsEmpty(this.ForeColor) == false)
                this.ForeColor = WordUtilities.ColorEmpty;
                 
            if ( (propId & PropertyIds.BackColor) == PropertyIds.BackColor &&
				 WordUtilities.ColorIsEmpty(this.BackColor) == false)
                this.BackColor = WordUtilities.ColorEmpty;
                 
            if ( (propId & PropertyIds.Effects) == PropertyIds.Effects &&
                 this.effects != null )
                this.effects.Reset();

            if ( (propId & PropertyIds.Italic) == PropertyIds.Italic &&
                 this.Italic.HasValue )
                this.Italic = null;
                 
            if ( (propId & PropertyIds.Name) == PropertyIds.Name &&
                 string.IsNullOrEmpty(this.Name) == false )
                this.Name = null;
                 
            if ( (propId & PropertyIds.NameComplexScript) == PropertyIds.NameComplexScript &&
                 string.IsNullOrEmpty(this.NameComplexScript) == false )
                this.NameComplexScript = null;
                 
            if ( (propId & PropertyIds.NameEastAsia) == PropertyIds.NameEastAsia &&
                 string.IsNullOrEmpty(this.NameEastAsia) == false )
                this.NameEastAsia = null;
                 
            if ( (propId & PropertyIds.NameHighAnsi) == PropertyIds.NameHighAnsi &&
                 string.IsNullOrEmpty(this.NameHighAnsi) == false )
                this.NameHighAnsi = null;
                 
            if ( (propId & PropertyIds.Size) == PropertyIds.Size &&
                 this.size.HasValue && this.size.Value > 0f )
                this.Size = null;
                 
            if ( (propId & PropertyIds.Underline) == PropertyIds.Underline &&
                 this.Underline != Underline.Default )
                this.Underline = Underline.Default;
                 
            if ( (propId & PropertyIds.RightToLeft) == PropertyIds.RightToLeft &&
				 this.rightToLeft.HasValue )
                this.rightToLeft = null;
                 
            if ( (propId & PropertyIds.UseComplexScript) == PropertyIds.UseComplexScript &&
				 this.useComplexScript.HasValue )
                this.useComplexScript = null;
        }

        #endregion Reset

        #region InitializeFrom
        internal void InitializeFrom( Font source )
        {
            if ( source == null )
                return;

            this.Reset();
            Font target = this;
            Font.MergeProperties( source, ref target );
        }
        #endregion InitializeFrom

        #endregion Internal

        #region PropertyIds
        internal enum PropertyIds
        {
            None = 0x0000,
            Bold = 0x0001,
            CharacterSpacing = 0x0002,
            ForeColor = 0x0004,
            BackColor = 0x0008,
            Effects = 0x0010,
            Italic = 0x0020,
            Name = 0x0040,
            NameComplexScript = 0x0080,
            NameEastAsia = 0x0100,
            NameHighAnsi = 0x0200,
            Size = 0x0400,
            Underline = 0x0800,
            UnderlineColor = 0x1000,
            RightToLeft = 0x2000,
            UseComplexScript = 0x4000,
            All = Bold | CharacterSpacing | ForeColor | BackColor | Effects | Italic | Name | NameComplexScript | NameEastAsia | NameHighAnsi | Size | Underline | UnderlineColor | RightToLeft | UseComplexScript,
        }
        #endregion PropertyIds

    }
    #endregion Font class

    #region FontEffects
    /// <summary>
    /// Encapsulates the various effects that can be applied to a
    /// <see cref="Infragistics.Documents.Word.Font">Font</see>, such as
    /// capitalization, strikethrough, subscript/superscript, and
    /// text effects such as engraving and embossing.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class FontEffects
    {
        #region Member variables
        
        private Font                    owningFont = null;
        private Capitalization          capitalization = Capitalization.Default;
        private FontTextEffect          textEffect = FontTextEffect.Default;
        private bool?                   hidden = null;
        private StrikeThrough           strikeThrough = StrikeThrough.Default;
        private FontVerticalAlignment   verticalAlignment = FontVerticalAlignment.Default;
        private bool?                   shadow = null;

        #endregion Member variables

        #region Constructor (internal)
        internal FontEffects( Font owningFont )
        {
            this.owningFont = owningFont;
        }
        #endregion Constructor (internal)

        #region Properties

        #region Unit
        internal UnitOfMeasurement Unit { get { return this.owningFont != null ? this.owningFont.Unit : WordUtilities.DefaultUnitOfMeasurement; } }
        #endregion Unit

        #region Capitalization
        /// <summary>
        /// Returns or sets a value indicating whether the font is
        /// displayed in normal or small capitalized letters.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// </remarks>
        public Capitalization Capitalization
        {
            get { return this.capitalization; }
            set { this.capitalization = value; }
        }
        #endregion Capitalization

        #region TextEffect
        /// <summary>
        /// Returns or sets a value indicating whether the font appears
        /// embossed, engraved, or outlined.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// <p class="body">
        /// When the embossing or engraving effect is enabled, the
        /// <see cref="Infragistics.Documents.Word.FontEffects.Shadow">Shadow</see>
        /// property is not applicable; shadowing cannot be applied in conjunction
        /// with either the embossing or engraving effect.
        /// </p>
        /// </remarks>
        public FontTextEffect TextEffect
        {
            get { return this.textEffect; }
            set { this.textEffect = value; }
        }
        #endregion TextEffect

        #region Hidden
        /// <summary>
        /// Returns or sets a boolean value indicating whether the text is hidden.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// </remarks>
        public bool? Hidden
        {
            get { return this.hidden; }
            set { this.hidden = value; }
        }
        #endregion Hidden

        #region Shadow
        /// <summary>
        /// Returns or sets a boolean value indicating whether a shadow
        /// is drawn around the text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// <p class="body">
        /// The Shadow property is not applicable when the
        /// <see cref="Infragistics.Documents.Word.FontEffects.TextEffect">TextEffect</see>
        /// property is set to enable embossing or engraving.
        /// </p>
        /// </remarks>
        public bool? Shadow
        {
            get { return this.shadow; }
            set { this.shadow = value; }
        }
        #endregion Shadow

        #region StrikeThrough
        /// <summary>
        /// Returns or sets a boolean value indicating whether a single
        /// or double line is drawn through the text.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// </remarks>
        public StrikeThrough StrikeThrough
        {
            get { return this.strikeThrough; }
            set { this.strikeThrough = value; }
        }
        #endregion StrikeThrough

        #region VerticalAlignment
        /// <summary>
        /// Returns or sets a value indicating whether the text
        /// is drawn in small letters above or below the baseline,
        /// i.e., in subscript or superscript.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When not explicitly set, the actual value of the property is determined
        /// at a higher level of the property resolution hierarchy.
        /// </p>
        /// </remarks>
        public FontVerticalAlignment VerticalAlignment
        {
            get { return this.verticalAlignment; }
            set { this.verticalAlignment = value; }
        }
        #endregion VerticalAlignment

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to
        /// their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.capitalization = Capitalization.Default;
            this.hidden = null;
            this.shadow = null;
            this.strikeThrough = StrikeThrough.Default;
            this.textEffect = FontTextEffect.Default;
            this.verticalAlignment = FontVerticalAlignment.Default;
        }
        #endregion Reset

        #endregion Methods

        #region Internal

        #region ShouldSerialize

        internal bool ShouldSerialize()
        {
            return this.ShouldSerialize( PropertyIds.All );
        }

        internal bool ShouldSerialize( PropertyIds propId )
        {
            if ( (propId & PropertyIds.Capitalization) == PropertyIds.Capitalization &&
                 this.Capitalization != Capitalization.Default )
                return true;
                 
            if ( (propId & PropertyIds.Hidden) == PropertyIds.Hidden &&
                 this.Hidden.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.Shadow) == PropertyIds.Shadow &&
                 this.Shadow.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.StrikeThrough) == PropertyIds.StrikeThrough &&
                 this.StrikeThrough != StrikeThrough.Default )
                return true;
                 
            if ( (propId & PropertyIds.TextEffect) == PropertyIds.TextEffect &&
                 this.TextEffect != FontTextEffect.Default )
                return true;
                 
            if ( (propId & PropertyIds.VerticalAlignment) == PropertyIds.VerticalAlignment &&
                 this.VerticalAlignment != FontVerticalAlignment.Default )
                return true;
                 
            return false;
        }

        #endregion ShouldSerialize

        #region MergeProperties
        /// <summary>
        /// Merges the property values that are explicitly set on this instance
        /// into the same properties on the <paramref name="destination"/> instance
        /// if they are not explicitly set on the destination instance.
        /// </summary>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal void MergeProperties( ref FontEffects destination )
        {
            FontEffects.MergeProperties( this, ref destination );
        }

        /// <summary>
        /// Merges the property values that are explicitly set on the
        /// <paramref name="source"/> instance into the same properties
        /// on the <paramref name="destination"/> instance if they are
        /// not explicitly set on the destination instance.
        /// </summary>
        /// <param name="source">The instance from which the property values are copied.</param>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal static void MergeProperties( FontEffects source, ref FontEffects destination )
        {
            if ( source == null || destination == null )
                return;

            if ( source.ShouldSerialize(PropertyIds.Capitalization) &&
                 destination.ShouldSerialize(PropertyIds.Capitalization) == false )
                destination.capitalization = source.capitalization;

            if ( source.ShouldSerialize(PropertyIds.Hidden) &&
                 destination.ShouldSerialize(PropertyIds.Hidden) == false )
                destination.hidden = source.hidden.Value;

            if ( source.ShouldSerialize(PropertyIds.Shadow) &&
                 destination.ShouldSerialize(PropertyIds.Shadow) == false )
                destination.shadow = source.shadow.Value;

            if ( source.ShouldSerialize(PropertyIds.StrikeThrough) &&
                 destination.ShouldSerialize(PropertyIds.StrikeThrough) == false )
                destination.strikeThrough = source.strikeThrough;

            if ( source.ShouldSerialize(PropertyIds.TextEffect) &&
                 destination.ShouldSerialize(PropertyIds.TextEffect) == false )
                destination.textEffect = source.textEffect;

            if ( source.ShouldSerialize(PropertyIds.VerticalAlignment) &&
                 destination.ShouldSerialize(PropertyIds.VerticalAlignment) == false )
                destination.verticalAlignment = source.verticalAlignment;
        }
        #endregion MergeProperties

        #endregion Internal

        #region PropertyIds
        internal enum PropertyIds
        {
            None = 0x0000,
            Capitalization = 0x0001,
            Hidden = 0x0002,
            Shadow = 0x0004,
            StrikeThrough = 0x0008,
            TextEffect = 0x0010,
            VerticalAlignment = 0x0020,
            All = Capitalization | Hidden | Shadow | StrikeThrough | TextEffect | VerticalAlignment,
        }
        #endregion PropertyIds
    }
    #endregion FontEffects

    #region FontCharacterSpacing
    /// <summary>
    /// Encapsulates the spacing and kerning properties of a
    /// <see cref="Infragistics.Documents.Word.Font">Font</see>.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class FontCharacterSpacing
    {
        #region Constants

        /// <summary>
        /// The minimum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Kerning">Kerning</see>
        /// property, expressed in twips (20)
        /// </summary>
        public const int KerningMinValue = 20;

        /// <summary>
        /// The maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Kerning">Kerning</see>
        /// property, expressed in twips (32760)
        /// </summary>
        public const int KerningMaxValue = 32760;

        /// <summary>
        /// The minimum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Scaling">Scaling</see>
        /// property, expressed as a percentage (1%)
        /// </summary>
        public const int ScalingMinValue = 1;

        /// <summary>
        /// The maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Scaling">Scaling</see>
        /// property, expressed as a percentage (600%)
        /// </summary>
        public const int ScalingMaxValue = 600;

        /// <summary>
        /// The minimum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Spacing">Spacing</see>
        /// property, expressed in twips (-31680)
        /// </summary>
        public const float SpacingMinValue = SpacingMaxValue * -1;

        /// <summary>
        /// The maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Spacing">Spacing</see>
        /// property, expressed in twips (31680)
        /// </summary>
        public const float SpacingMaxValue = 31680f;

        /// <summary>
        /// The maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Position">Position</see>
        /// property, expressed in twips (31680)
        /// </summary>
        public const float PositionMaxValue = 31680f;

        /// <summary>
        /// The minimum allowable value for the
        /// <see cref="Infragistics.Documents.Word.FontCharacterSpacing.Position">Position</see>
        /// property, expressed in twips (31680)
        /// </summary>
        public const float PositionMinValue = -31680f;

        #endregion Constants

        #region Member variables

        private Font                    owningFont = null;
        private float?                  kerning = null;
        private int?                    scaling = null;
        private float?                  spacing = null;
        private float?                  position = null;

        #endregion Member variables

        #region Constructor (internal)
        internal FontCharacterSpacing( Font owningFont )
        {
            this.owningFont = owningFont;
        }
        #endregion Constructor (internal)

        #region Properties

        #region Unit
        internal UnitOfMeasurement Unit { get { return this.owningFont != null ? this.owningFont.Unit : WordUtilities.DefaultUnitOfMeasurement; } }
        #endregion Unit

        #region Kerning
        /// <summary>
        /// Returns or sets a value indicating the size at which kerning algorithms are
        /// applied to the font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value assigned to this property must be a value between the
        /// range of 1 and 1638 DTPs (desktop publishing points), inclusive.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        public float? Kerning
        {
            get { return this.kerning.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.kerning.Value ) : this.kerning; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "Kerning",
                    newValue,
                    FontCharacterSpacing.KerningMinValue,
                    FontCharacterSpacing.KerningMaxValue );

                this.kerning = newValue;
            }
        }

        internal int KerningInHalfPoints{ get { return this.kerning.HasValue ? (int)(this.kerning.Value / 10f): 0; } }

        #endregion Kerning

        #region Scaling
        /// <summary>
        /// Returns or sets a value indicating the horizontal scaling
        /// percentage for the associated font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value assigned to this property must be between the range of 1 and 600, inclusive.
        /// </p>
        /// <p class="body">
        /// This property represents the percentage of scaling, with a value of 100
        /// representing the default scaling for the font. Values less than 100 cause
        /// the font to be horizontally compressed; Values greater than 100 cause the
        /// font to be horizontally elongated.
        /// </p>
        /// </remarks>
        public int? Scaling
        {
            get { return this.scaling; }
            set
            {
                int newValue = 0;
                if ( value.HasValue )
                {
                    newValue = value.Value;

                    if ( newValue < FontCharacterSpacing.ScalingMinValue || newValue > FontCharacterSpacing.ScalingMaxValue )
                    {
                        string format = SR.GetString("Exception_PropertyValueOutOfRange");
                        throw new ArgumentOutOfRangeException( string.Format(format, newValue, "Scaling", FontCharacterSpacing.ScalingMinValue, FontCharacterSpacing.ScalingMaxValue, "%") );
                    }
                }

                this.scaling = value.HasValue ? newValue : value;
            }
        }
        #endregion Scaling

        #region Spacing
        /// <summary>
        /// Returns or sets a value indicating the amount of horizontal spacing
        /// between characters for the associated font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value assigned to this property must be a value between the
        /// range of -1584 and +1584 DTPs (desktop publishing points), inclusive.
        /// </p>
        /// <p class="body">
        /// This property represents the amount of space that appears between horizontally adjacent characters.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        public float? Spacing
        {
            get { return this.spacing.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.spacing.Value ) : this.spacing; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "Spacing",
                    newValue,
                    FontCharacterSpacing.SpacingMinValue,
                    FontCharacterSpacing.SpacingMaxValue );

                this.spacing = newValue;
            }
        }

        internal int SpacingInTwips{ get { return this.spacing.HasValue ? (int)this.spacing.Value : 0; } }
        
        #endregion Spacing

        #region Position
        /// <summary>
        /// Returns or sets a value indicating the amount by which the text is
        /// vertically offset from the baseline for the associated font.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The value assigned to this property must be a value between the
        /// range of -1584 and +1584 DTPs (desktop publishing points), inclusive.
        /// A value of zero restores the position to normal as relative to the baseline.
        /// </p>
        /// <p class="body">
        /// This property represents the amount of vertical space between the
        /// bottom of the character and the baseline for the associated font.
        /// Positive values cause characters to appear higher than characters
        /// whose position is normal as relative to the baseline; negative values
        /// cause characters to appear lower.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        public float? Position
        {
            get { return this.position.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.position.Value ) : this.position; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "Position",
                    newValue,
                    FontCharacterSpacing.PositionMinValue,
                    FontCharacterSpacing.PositionMaxValue);

                this.position = newValue;
            }
        }

        internal int PositionInHalfPoints{ get { return this.position.HasValue ? (int)(this.position.Value / 10f): 0; } }
        
        #endregion Position

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to
        /// their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.kerning = null;
            this.scaling = null;
            this.spacing = null;
            this.position = null;
        }
        #endregion Reset

        #endregion Methods

        #region Internal

        #region ShouldSerialize

        internal bool ShouldSerialize()
        {
            return this.ShouldSerialize( PropertyIds.All );
        }

        internal bool ShouldSerialize( PropertyIds propId )
        {
            if ( (propId & PropertyIds.Scaling) == PropertyIds.Scaling &&
                 this.Scaling.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.Spacing) == PropertyIds.Spacing &&
                 this.Spacing.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.Position) == PropertyIds.Position &&
                 this.Position.HasValue )
                return true;
                 
            if ( (propId & PropertyIds.Kerning) == PropertyIds.Kerning &&
                 this.Kerning.HasValue )
                return true;

            return false;
        }

        #endregion ShouldSerialize

        #region MergeProperties
        /// <summary>
        /// Merges the property values that are explicitly set on this instance
        /// into the same properties on the <paramref name="destination"/> instance
        /// if they are not explicitly set on the destination instance.
        /// </summary>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal void MergeProperties( ref FontCharacterSpacing destination )
        {
            FontCharacterSpacing.MergeProperties( this, ref destination );
        }

        /// <summary>
        /// Merges the property values that are explicitly set on the
        /// <paramref name="source"/> instance into the same properties
        /// on the <paramref name="destination"/> instance if they are
        /// not explicitly set on the destination instance.
        /// </summary>
        /// <param name="source">The instance from which the property values are copied.</param>
        /// <param name="destination">The instance to which the property values are assigned.</param>
        internal static void MergeProperties( FontCharacterSpacing source, ref FontCharacterSpacing destination )
        {
            if ( source == null || destination == null )
                return;

            if ( source.ShouldSerialize(PropertyIds.Kerning) &&
                 destination.ShouldSerialize(PropertyIds.Kerning) == false )
                destination.kerning = source.kerning.Value;

            if ( source.ShouldSerialize(PropertyIds.Position) &&
                 destination.ShouldSerialize(PropertyIds.Position) == false )
                destination.position = source.position.Value;

            if ( source.ShouldSerialize(PropertyIds.Scaling) &&
                 destination.ShouldSerialize(PropertyIds.Scaling) == false )
                destination.scaling = source.scaling.Value;

            if ( source.ShouldSerialize(PropertyIds.Spacing) &&
                 destination.ShouldSerialize(PropertyIds.Spacing) == false )
                destination.spacing = source.spacing.Value;
        }
        #endregion MergeProperties

        #endregion Internal

        #region PropertyIds
        internal enum PropertyIds
        {
            None = 0x0000,
            Scaling = 0x0001,
            Spacing = 0x0002,
            Position = 0x0004,
            Kerning = 0x0008,
            All = Scaling | Spacing | Position | Kerning,
        }
        #endregion PropertyIds
    }
    #endregion FontCharacterSpacing


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