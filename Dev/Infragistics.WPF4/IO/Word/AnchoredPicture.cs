using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;


using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;

//  BF 3/23/11
//  Using this for SL as well now
using Image = System.Windows.Media.Imaging.BitmapSource;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region Anchor class
    /// <summary>
    /// Encapsulates an anchor to a specific location within the document.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class Anchor : IUnitOfMeasurementProvider
    {
        #region Constants
        
        internal const AnchorTextWrapping                         DefaultTextWrapping = AnchorTextWrapping.Square;
        internal const AnchorTextWrappingSide                     DefaultTextWrappingSide = AnchorTextWrappingSide.Both;

        internal const AnchorHorizontalAlignment            DefaultHorizontalAlignment = AnchorHorizontalAlignment.None;
        internal const AnchorRelativeHorizontalPosition     DefaultRelativeHorizontalPosition = AnchorRelativeHorizontalPosition.Column;
        internal const AnchorRelativeVerticalPosition       DefaultRelativeVerticalPosition = AnchorRelativeVerticalPosition.Paragraph;
        internal const AnchorVerticalAlignment              DefaultVerticalAlignment = AnchorVerticalAlignment.None;

        #endregion Constants

        #region Member variables

        internal IUnitOfMeasurementProvider         unitOfMeasurementProvider = null;
        private AnchorTextWrapping                  textWrapping = Anchor.DefaultTextWrapping;
        private AnchorTextWrappingSide              textWrappingSide = Anchor.DefaultTextWrappingSide;
        private AnchorHorizontalAlignment           horizontalAlignment = Anchor.DefaultHorizontalAlignment;
        private float                               horizontalOffset = 0f;
        private AnchorRelativeHorizontalPosition    relativeHorizontalPosition = Anchor.DefaultRelativeHorizontalPosition;
        private AnchorRelativeVerticalPosition      relativeVerticalPosition = Anchor.DefaultRelativeVerticalPosition;
        private AnchorVerticalAlignment             verticalAlignment = Anchor.DefaultVerticalAlignment;
        private float                               verticalOffset = 0f;
        private Hyperlink                           hyperlink = null;
        private string                              alternateTextDescription = null;
        private SizeF?                              size = null;

        #endregion Member variables

        #region Constructor

        internal Anchor( IUnitOfMeasurementProvider unitOfMeasurementProvider )
        {
            this.unitOfMeasurementProvider = unitOfMeasurementProvider;
        }

        #endregion Constructor

        #region Properties

        #region AlternateTextDescription
        /// <summary>
        /// Returns or sets the description of the alternative text for the picture or shape,
        /// for use by assistive technologies or applications which will not display the
        /// associated picture or shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML consumers which do not support the display of pictures or shapes
        /// may make use this property to describe the picture to the user. Screen
        /// reading applications or other assistive technologies may also make use
        /// of the property to describe the picture or shape to handicapped users.
        /// </p>
        /// </remarks>
        public string AlternateTextDescription
        {
            get { return WordUtilities.StringPropertyGetHelper( this.alternateTextDescription ); }
            set { this.alternateTextDescription = value; }
        }

        internal bool HasAlternateTextDescription { get { return string.IsNullOrEmpty(this.alternateTextDescription) == false; } }
        
        #endregion AlternateTextDescription

        #region HorizontalAlignment
        /// <summary>
        /// Returns or sets a value which defines the anchor's horizontal alignment.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The HorizontalAlignment property is used in conjunction with the
        /// <see cref="Infragistics.Documents.Word.Anchor.RelativeHorizontalPosition">RelativeHorizontalPosition</see>
        /// to define the horizontal aspect of the anchor's position. An anchor can be aligned
        /// with the left or right edge of the page, margin, paragraph, etc.
        /// </p>
        /// <p class="body">
        /// By default, the HorizontalAlignment property is not applicable, and positioning is
        /// implied to be absolute, defined by the value of the
        /// <see cref="Infragistics.Documents.Word.Anchor.HorizontalOffset">HorizontalOffset</see>
        /// property. Explicitly setting the HorizontalAlignment property to a value other than 'None'
        /// overrides the HorizontalOffset property setting.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.RelativeHorizontalPosition">RelativeHorizontalPosition</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.HorizontalOffset">HorizontalOffset</seealso>
        public AnchorHorizontalAlignment HorizontalAlignment
        {
            get { return this.horizontalAlignment; }
            set { this.horizontalAlignment = value; }
        }

        internal bool HasHorizontalAlignment { get { return this.horizontalAlignment != AnchorHorizontalAlignment.None; } }

        #endregion HorizontalAlignment

        #region HorizontalOffset
        /// <summary>
        /// Returns or sets the amount by which the anchor is offset horizontally
        /// from the boundary defined by the
        /// <see cref="Infragistics.Documents.Word.Anchor.RelativeHorizontalPosition">RelativeHorizontalPosition</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.Anchor.HorizontalAlignment">HorizontalAlignment</see>
        /// properties.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The RelativeHorizontalPosition and HorizontalAlignment properties define the
        /// boundary at which the anchor is positioned; the HorizontalOffset
        /// property defines an additional amount of space to be added by which to offset
        /// the position. For example, if an anchor is left-aligned with the margin, and the offset
        /// is set to 72 pts., the object will appear one inch from the left edge of the margin.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// <p class="body">
        /// When the
        /// <see cref="Infragistics.Documents.Word.Anchor.HorizontalAlignment">HorizontalAlignment</see>
        /// property is set to a value other than 'None', this property is not applicable.
        /// The HorizontalOffset property can be restored to its factory setting
        /// by assigning 'None' to this property, in which case the HorizontalAlignment property determines
        /// the horizontal positioning.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.RelativeHorizontalPosition">RelativeHorizontalPosition</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.HorizontalAlignment">HorizontalAlignment</seealso>
        public float HorizontalOffset
        {
            get
            {
                if ( this.horizontalOffset == 0f )
                    return this.horizontalOffset;

                return WordUtilities.ConvertFromTwips( this.Unit, this.horizontalOffset );
            }

            set { this.horizontalOffset = WordUtilities.ConvertToTwips( this.Unit, value ); }
        }

        internal int HorizontalOffsetInTwips { get { return (int)this.horizontalOffset; } }
        internal int HorizontalOffsetInEMUs { get { return this.horizontalOffset != 0f ? WordUtilities.TwipsToEMU( this.horizontalOffset ) : 0; } }

        #endregion HorizontalOffset

        #region RelativeHorizontalPosition
        /// <summary>
        /// Returns or sets a value which defines the part of the document
        /// to which the anchor's horizontal position is relative.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The RelativeHorizontalPosition property defines the area
        /// of the document to which the value of the
        /// <see cref="Infragistics.Documents.Word.Anchor.HorizontalAlignment">HorizontalAlignment</see>
        /// or
        /// <see cref="Infragistics.Documents.Word.Anchor.HorizontalOffset">HorizontalOffset</see>
        /// property is relative. For example, if AnchorHorizontalAlignment is set to 'Left',
        /// and RelativeHorizontalPosition is set to 'LeftMarginArea', the anchor's left
        /// edge is aligned with the left edge of the document's left margin. If
        /// AnchorHorizontalAlignment is set to 'Right', and RelativeHorizontalPosition is
        /// set to 'Page', the anchor's right edge is aligned with the right
        /// edge of the page, overlapping with the area outside the margin.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.HorizontalAlignment">HorizontalAlignment</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.HorizontalOffset">HorizontalOffset</seealso>
        public AnchorRelativeHorizontalPosition RelativeHorizontalPosition
        {
            get { return this.relativeHorizontalPosition; }
            set { this.relativeHorizontalPosition = value; }
        }
        #endregion RelativeHorizontalPosition

        #region RelativeVerticalPosition
        /// <summary>
        /// Returns or sets a value which defines the part of the document
        /// to which the anchor's vertical position is relative.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The RelativeVerticalPosition property defines the area
        /// of the document to which the value of the
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</see>
        /// or
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalOffset">VerticalOffset</see>
        /// property is relative. For example, if VerticalAlignment is set to 'Top',
        /// and RelativeVerticalPosition is set to 'Page', the anchor's top
        /// edge is aligned with the top edge of the page. If
        /// VerticalAlignment is set to 'Bottom', and RelativeVerticalPosition is
        /// set to 'Page', the anchor's bottom edge is aligned with the bottom
        /// edge of the page, overlapping with the area outside the margin.
        /// </p>
        /// <p class="body">
        /// When the
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</see>
        /// property is explicitly set, the 'Paragraph' setting for this property is not supported;
        /// in that case this property resolves to 'Page'
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.VerticalOffset">VerticalOffset</seealso>
        public AnchorRelativeVerticalPosition RelativeVerticalPosition
        {
            get { return this.relativeVerticalPosition; }
            set { this.relativeVerticalPosition = value; }
        }
        #endregion RelativeVerticalPosition

        #region VerticalAlignment
        /// <summary>
        /// Returns or sets a value which defines the anchor's vertical alignment.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The VerticalAlignment property is used in conjunction with the
        /// <see cref="Infragistics.Documents.Word.Anchor.RelativeVerticalPosition">RelativeVerticalPosition</see>
        /// to define the vertical aspect of the anchor's position. An anchor can be aligned
        /// with the top or bottom edge of the page, margin, paragraph, etc.
        /// </p>
        /// <p class="body">
        /// By default, the VerticalAlignment property is not applicable, and positioning is
        /// implied to be absolute, defined by the value of the
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalOffset">VerticalOffset</see>
        /// property.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.RelativeVerticalPosition">RelativeVerticalPosition</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.VerticalOffset">VerticalOffset</seealso>
        public AnchorVerticalAlignment VerticalAlignment
        {
            get { return this.verticalAlignment; }
            set { this.verticalAlignment = value; }
        }

        internal bool HasVerticalAlignment { get { return this.verticalAlignment != AnchorVerticalAlignment.None; } }

        #endregion VerticalAlignment

        #region VerticalOffset
        /// <summary>
        /// Returns or sets the amount by which the anchor is offset
        /// from the boundary defined by the
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</see>
        /// property.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.Anchor.RelativeVerticalPosition">RelativeVerticalPosition</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</see>
        /// properties define the boundary at which the anchor is positioned; the VerticalOffset
        /// property defines an additional amount of space to be added by which to offset
        /// the position. For example, if an anchor is top-aligned with the margin, and the offset
        /// is set to 72 pts., the object will appear one inch down from the top edge of the margin.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.RelativeVerticalPosition">RelativeVerticalPosition</seealso>
        /// <seealso cref="Infragistics.Documents.Word.Anchor.VerticalAlignment">VerticalAlignment</seealso>
        public float VerticalOffset
        {
            get
            {
                if ( this.verticalOffset == 0f )
                    return this.verticalOffset;

                return WordUtilities.ConvertFromTwips( this.Unit, this.verticalOffset );
            }

            set { this.verticalOffset = value != 0f ? WordUtilities.ConvertToTwips( this.Unit, value ) : 0f; }
        }

        internal int VerticalOffsetInTwips { get { return (int)this.verticalOffset; } }
        internal int VerticalOffsetInEMUs { get { return this.verticalOffset != 0f ? WordUtilities.TwipsToEMU( this.verticalOffset ) : 0; } }

        #endregion VerticalOffset

        #region Size
        /// <summary>
        /// Returns or sets the size at which the associated picture or shape is rendered.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the absence of an explicit setting, this property returns null.
        /// In this case, pictures are displayed at their natural size, and
        /// shapes are displayed at a size of one inch square.
        /// </p>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// </remarks>
        public SizeF? Size
        {
            get
            {
                return Anchor.GetSize( this.SizeRawValue, this.Unit );
            }

            set
            { 
                Anchor.SetSize( value, ref this.size, this.Unit, false );
            }
        }

        internal virtual SizeF? SizeRawValue
        {
	        get { return this.size; }
        }

        static internal SizeF? GetSize( SizeF? member, UnitOfMeasurement unit )
        {
            return member.HasValue ? WordUtilities.ConvertSizeFromTwips(unit, member.Value) : member;
        }

        static internal void SetSize( SizeF? value, ref SizeF? member, UnitOfMeasurement unit, bool allowZeroSize )
        {
            if ( value.HasValue == false )
                member = null;
            else
            {
                float min = allowZeroSize ? 0f : float.Epsilon;

                if ( value.Value.Width < min || value.Value.Height < min )
                    throw new ArgumentOutOfRangeException( "value" );

                member = WordUtilities.ConvertSizeToTwips( unit, value.Value );
            }
        }

        internal bool HasExplicitSize { get { return this.SizeRawValue.HasValue; } }

        /// <summary>
        /// Returns the resolved size in TWIPs
        /// </summary>
        internal Size SizeInTwips
        {
            get
            {
                SizeF size = this.SizeRawValue.HasValue ? this.SizeRawValue.Value : this.DefaultSizeInTwips;
                return WordUtilities.ToSize( size );
            }
        }

        /// <summary>
        /// Returns the resolved size in EMUs
        /// </summary>
        internal Size SizeInEMUs
        {
            get
            {
                Size sizeInTwips = this.SizeInTwips;

                Size sizeInEMUs = new Size(
                    WordUtilities.TwipsToEMU( sizeInTwips.Width ),
                    WordUtilities.TwipsToEMU( sizeInTwips.Height ) );

                return sizeInEMUs;

            }
        }

        #endregion Size

        #region TextWrapping
        /// <summary>
        /// Returns or sets a value which determines the layout for text
        /// included in the same paragraph as the anchor.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The TextWrapping property works in conjunction with the
        /// <see cref="Infragistics.Documents.Word.Anchor.TextWrappingSide">TextWrappingSide</see>
        /// property to define the manner in which text that is adjacent to the anchor
        /// is wrapped.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.AnchorTextWrappingSide">AnchorTextWrappingSide</seealso>
        public AnchorTextWrapping TextWrapping
        {
            get { return this.textWrapping; }
            set { this.textWrapping = value; }
        }
        #endregion TextWrapping

        #region TextWrappingSide
        /// <summary>
        /// Returns or sets a value which determines the sides of
        /// the anchor around which adjacent text can be wrapped.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The TextWrappingSide property works in conjunction with the
        /// <see cref="Infragistics.Documents.Word.Anchor.TextWrapping">TextWrapping</see>
        /// property to define the manner in which text that is adjacent to the anchor
        /// is wrapped.
        /// </p>
        /// <p class="body">
        /// The TextWrappingSide property is not applicable when the TextWrapping property
        /// is set to 'TextInBackground' or 'TextInForeground'.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.AnchorTextWrapping">AnchorTextWrapping</seealso>
        public AnchorTextWrappingSide TextWrappingSide
        {
            get { return this.textWrappingSide; }
            set { this.textWrappingSide = value; }
        }
        #endregion TextWrappingSide

        #region DefaultSizeInTwips

        internal abstract SizeF DefaultSizeInTwips{ get; }
        
        #endregion DefaultSizeInTwips

        #region Unit
        internal UnitOfMeasurement Unit
        {
            get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; }
        }
        #endregion Unit

        #region Hyperlink
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.Hyperlink">Hyperlink</see>
        /// instance which provides a way to add a hyperlink to the picture.
        /// </summary>
        public Hyperlink Hyperlink
        {
            get
            {
                if ( this.hyperlink == null )
                    this.hyperlink = new Hyperlink(null, true);

                return this.hyperlink;
            }
        }

        internal bool HasHyperlink { get { return this.hyperlink != null && string.IsNullOrEmpty(this.hyperlink.Address) == false; } }

        #endregion Hyperlink
        
        #endregion Properties

        #region Methods

        #region GetTextWrapping_Vml
        internal string GetTextWrapping_Vml( out string side, out int zIndex )
        {
            side = string.Empty;
            zIndex = 1;
            string retVal = string.Empty;

            switch ( this.TextWrapping )
            {
                case AnchorTextWrapping.TextInBackground:
                    break;

                case AnchorTextWrapping.TextInForeground:
                    zIndex = -1;
                    break;

                case AnchorTextWrapping.Square:
                    retVal = "square";
                    break;

                case AnchorTextWrapping.TopAndBottom:
                    retVal = "topAndBottom";
                    break;
            }

            switch ( this.TextWrappingSide )
            {
                case AnchorTextWrappingSide.Left:
                case AnchorTextWrappingSide.Right:
                case AnchorTextWrappingSide.Largest:
                    side = this.TextWrappingSide.ToString().ToLower();
                    break;

                case AnchorTextWrappingSide.Both:
                    break;
            }

            return retVal;
        }
        #endregion GetTextWrapping_Vml

        #region GetVerticalAlignment_Vml
        internal string GetVerticalAlignment_Vml( out string relativeTo )
        {
            AnchorVerticalAlignment alignment = this.VerticalAlignment;

            string retVal = null;

            switch ( alignment )
            {
                case AnchorVerticalAlignment.None:
                    retVal = "absolute";
                    break;

                default:
                    retVal = EnumConverter.FromAnchorVerticalAlignment_Vml( alignment );
                    break;
            }

            relativeTo = EnumConverter.FromAnchorRelativeVerticalPosition_Vml( this.RelativeVerticalPosition );

            return retVal;
        }
        #endregion GetVerticalAlignment_Vml

        #region GetHorizontalAlignment_Vml
        internal string GetHorizontalAlignment_Vml( out string relativeTo )
        {
            AnchorHorizontalAlignment alignment = this.HorizontalAlignment;

            string retVal = null;

            switch ( alignment )
            {
                case AnchorHorizontalAlignment.None:
                    retVal = "absolute";
                    break;

                default:
                    retVal = EnumConverter.FromAnchorHorizontalAlignment_Vml( alignment );
                    break;
            }

            relativeTo = EnumConverter.FromAnchorRelativeHorizontalPosition_Vml( this.RelativeHorizontalPosition );

            return retVal;
        }
        #endregion GetHorizontalAlignment_Vml

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public void Reset()
        {
            this.horizontalAlignment = Anchor.DefaultHorizontalAlignment;
            this.horizontalOffset = 0f;            
            this.relativeHorizontalPosition = Anchor.DefaultRelativeHorizontalPosition;
            this.relativeVerticalPosition = Anchor.DefaultRelativeVerticalPosition;
            this.textWrapping = Anchor.DefaultTextWrapping;
            this.textWrappingSide = Anchor.DefaultTextWrappingSide;
            this.verticalAlignment = Anchor.DefaultVerticalAlignment;
            this.verticalOffset = 0f;
            this.size = null;

            if ( this.hyperlink != null )
                this.hyperlink.Reset( null, true );
        }
        #endregion Reset

        #endregion Methods

        #region IUnitOfMeasurementProvider Members

        UnitOfMeasurement IUnitOfMeasurementProvider.Unit
        {
            get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; }
        }

        #endregion
    }
    #endregion Anchor class

    #region AnchoredShape class

    /// <summary>
    /// Encapsulates a shape that is anchored to a specific
    /// location within the document.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Shapes are rendered by the consumer using Vector Markup Language (VML),
    /// which is supported in both MS Word 2007 and MS Word 2010.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class AnchoredShape : Anchor
    {
        #region Member variables

        private Shape shape = null;

        #endregion Member variables

        #region Constructor
        
        internal AnchoredShape( Shape shape, IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
            this.shape = shape;
        }

        internal AnchoredShape( ShapeType shapeType, IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
            this.shape = VmlShape.Create( unitOfMeasurementProvider, shapeType );
        }
        
        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="shape">
        /// A previously created
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// shape instance on which this instance is based.
        /// </param>
        static public AnchoredShape Create( WordDocumentWriter writer, Shape shape )
        {
            return new AnchoredShape( shape, writer );
        }

        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the shape.
        /// </param>
        static public AnchoredShape Create( WordDocumentWriter writer, ShapeType shapeType )
        {
            return new AnchoredShape( shapeType, writer );
        }

        #endregion Constructor

        #region Properties

        #region Shape
        /// <summary>
        /// Returns the associated
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// </summary>
        public Shape Shape { get { return this.shape; } }
        #endregion Shape

        #region Size

        internal override SizeF DefaultSizeInTwips
        {
            get { return this.shape.DefaultSizeInTwips; }
        }
        
        internal override SizeF? SizeRawValue
        {
            get
            { 
                SizeF? mySize = base.SizeRawValue;
                if ( mySize.HasValue )
                    return mySize.Value;

                return this.shape.SizeRawValue;
            }
        }
        #endregion Size

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        /// <param name="resetShape">
        /// Specifies whether to reset the properties of the associated
        /// <see cref="Infragistics.Documents.Word.AnchoredShape.Shape">Shape</see>.
        /// </param>
        public void Reset( bool resetShape )
        {
            base.Reset();

            if ( resetShape && this.shape != null )
                this.shape.Reset();
        }

        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        /// <param name="shape">
        /// The new
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// with which this instance is to be associated.
        /// </param>
        public void Reset( Shape shape )
        {
            if ( shape == null )
                throw new ArgumentNullException("shape");

            base.Reset();
            
            this.shape = shape;
        }
        #endregion Reset

        #region ToString
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return this.shape != null ? this.shape.ToString() : base.ToString();
        }
        #endregion ToString

        #endregion Methods
    }

    #endregion AnchoredShape class

    #region Shape class
    /// <summary>
    /// Abstract class which encapsulates a geometric shape such
    /// as a line, rectangle, ellipse, etc.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class Shape
    {
        #region Member variables
        
        private SizeF?                      size = null;
        private IUnitOfMeasurementProvider  unitOfMeasurementProvider = null;

        #endregion Member variables

        #region Constructor
        internal Shape( IUnitOfMeasurementProvider unitOfMeasurementProvider )
        {
            this.unitOfMeasurementProvider = unitOfMeasurementProvider;
        }

        #endregion Constructor

        #region Properties

        #region Size
        /// <summary>
        /// Returns or sets the size at which the shape is rendered.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the absence of an explicit setting, this property returns null.
        /// In this case, shapes are displayed at a size of one inch square.
        /// </p>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// </remarks>
        public SizeF? Size
        {
            get
            {
                return Anchor.GetSize( this.size, this.Unit );
            }

            set
            { 
                Anchor.SetSize( value, ref this.size, this.Unit, true );
            }
        }

        internal SizeF?  SizeRawValue
        {
	        get { return this.size; }
        }

        internal SizeF DefaultSizeInTwips
        {
            get { return new SizeF( WordUtilities.TwipsPerInch, WordUtilities.TwipsPerInch ); }
        }

        /// <summary>
        /// Returns the resolved size in TWIPs
        /// </summary>
        internal Size SizeInTwips
        {
            get
            {
                SizeF size = this.SizeRawValue.HasValue ? this.SizeRawValue.Value : this.DefaultSizeInTwips;
                return WordUtilities.ToSize( size );
            }
        }

        #endregion Size

        #region Unit
        internal UnitOfMeasurement Unit
        {
            get { return this.unitOfMeasurementProvider != null ? this.unitOfMeasurementProvider.Unit : WordUtilities.DefaultUnitOfMeasurement; }
        }
        #endregion Unit

        #endregion Properties

        #region Abstract/Virtual

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public abstract ShapeType Type { get; }
        #endregion Type

        #endregion Abstract

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public virtual void Reset()
        {
            this.size = null;
        }
        #endregion Reset

        #region ToString
        /// <summary>
        /// Returns the string representation of this instance.
        /// </summary>
        public override string ToString()
        {
            return this.Type.ToString();
        }
        #endregion ToString

        #endregion Methods
    }
    #endregion Shape class

    #region VmlShape class
    /// <summary>
    /// Abstract class which encapsulates a geometric shape such
    /// as a line, rectangle, ellipse, etc., which is rendered by
    /// the consumer using Vector Markup Language (VML).
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Shapes are rendered by the consumer using Vector Markup Language (VML),
    /// which is supported in both MS Word 2007 and MS Word 2010.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public abstract class VmlShape : Shape
    {
        #region Member variables
        
        private float?                      lineWidth = null;
        private Color                       lineColor = WordUtilities.ColorEmpty;
        private ShapeLineStyle              lineStyle = ShapeLineStyle.Solid;


        #endregion Member variables

        #region Constructor
        internal VmlShape( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }

        /// <summary>
        /// Creates a new
        /// <see cref="Infragistics.Documents.Word.VmlShape">VmlShape</see>-derived
        /// instance based on the specified <paramref name="shapeType"/>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </param>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the type of shape to be created.
        /// </param>
        /// <remarks>
        /// <p class="body">
        /// The type of the object returned from the Create method is different
        /// based on the specified <paramref name="shapeType"/>. For example,
        /// when 'Line' is specified as the shape type, a VmlLine instance is
        /// returned.
        /// </p>
        /// <p class="body">
        /// To access properties that are specific to the VmlShape-derived type
        /// that is returned, the reference returned from this method can be upcasted.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ShapeType">ShapeType enumeration</seealso>
        static public VmlShape Create( WordDocumentWriter writer, ShapeType shapeType )
        {
            IUnitOfMeasurementProvider unitOfMeasurementProvider = writer as IUnitOfMeasurementProvider;
            return VmlShape.Create( unitOfMeasurementProvider, shapeType );
        }

        static internal VmlShape Create( IUnitOfMeasurementProvider unitOfMeasurementProvider, ShapeType shapeType )
        {
            switch ( shapeType )
            {
                case ShapeType.Line:
                    return new VmlLine(unitOfMeasurementProvider);

                case ShapeType.Rectangle:
                    return new VmlRectangle(unitOfMeasurementProvider);

                case ShapeType.Ellipse:
                    return new VmlEllipse(unitOfMeasurementProvider);

                case ShapeType.IsosceleseTriangle:
                    return new VmlIsosceleseTriangle(unitOfMeasurementProvider);

                case ShapeType.RightTriangle:
                    return new VmlRightTriangle(unitOfMeasurementProvider);
            }

            return null;
        }

        #endregion Constructor

        #region Properties

        #region LineColor
        /// <summary>
        /// Returns or sets the color used to render the perimeter of the shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The LineColor property controls the color for a
        /// <see cref="Infragistics.Documents.Word.VmlLine">line</see>
        /// shape.
        /// </p>
        /// <p class="body">
        /// The LineColor property controls the color of the outer border for
        /// shapes which have area. Use the
        /// <see cref="Infragistics.Documents.Word.VmlRotatableShape.FillColor">FillColor</see>
        /// property to change the color of the interior region.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.VmlRotatableShape.FillColor">FillColor</seealso>
        public Color LineColor
        {
            get { return this.lineColor; }
            set { this.lineColor = value; }
        }
        #endregion LineColor

        #region LineWidth
        /// <summary>
        /// Returns or sets the width of the line which renders the perimeter of the shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// </remarks>
        public float? LineWidth
        {
            get { return this.lineWidth.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.lineWidth.Value ) : this.lineWidth; }
            set
            {
                this.lineWidth = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;
            }
        }

        internal float LineWidthInPoints
        {
            get
            {
                if ( this.lineWidth.HasValue == false )
                    return 0;

                return
                    this.lineWidth.HasValue ?
                    WordUtilities.ConvertFromTwips(UnitOfMeasurement.Point, this.lineWidth.Value ) :
                    0f;
            }
        }
        #endregion LineWidth

        #region LineStyle
        /// <summary>
        /// Returns or sets the style of the line drawn around
        /// the perimeter of the shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The LineStyle property controls the style for a
        /// <see cref="Infragistics.Documents.Word.VmlLine">line</see>
        /// shape. By default, lines are drawn with a solid, unbroken
        /// stroke; the LineStyle property can be used to draw dashes,
        /// dots, or combinations thereof.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.VmlShape.LineColor">LineColor</seealso>
        public ShapeLineStyle LineStyle
        {
            get { return this.lineStyle; }
            set { this.lineStyle = value; }
        }
        #endregion LineStyle

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { throw new NotSupportedException(); } }
        #endregion Type

        #endregion Properties

        #region Abstract/Virtual

        #region _BackColor
        
        internal virtual Color _BackColor { get { return WordUtilities.ColorEmpty; } }
        
        #endregion _BackColor

        #region _FlipX
        
        internal virtual bool _FlipX { get { return false; } }
        
        #endregion _FlipX

        #region _FlipY
        
        internal virtual bool _FlipY { get { return false; } }
        
        #endregion _FlipY

        #region _Rotation
        
        internal virtual int _Rotation { get { return 0; } }
        
        #endregion _Rotation

        #region _ConnectorType
        internal virtual string _ConnectorType { get { return null; } }
        #endregion _ConnectorType

        #endregion Abstract

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.lineColor = WordUtilities.ColorEmpty;
            this.lineWidth = null;
            this.lineStyle = ShapeLineStyle.Solid;
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion VmlShape class

    #region VmlLine class
    /// <summary>
    /// Renders a straight line using Vector Markup Language (VML)
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The origin for a line is always implied to be the top-left
    /// corner of it bounding rectangle. When the bounding rectangle's
    /// height is non-zero, the slope of the line is negative (resembles
    /// a backslash); to make the slope positive, set the
    /// <see cref="Infragistics.Documents.Word.VmlLine.InvertY">InvertY</see>
    /// property to true.
    /// </p>
    /// <p class="body">
    /// To render a horizontal line, set the Height component of the
    /// <see cref="Infragistics.Documents.Word.Shape.Size">Size</see>
    /// property to zero.
    /// </p>
    /// <p class="body">
    /// To render a vertical line, set the Width component of the
    /// <see cref="Infragistics.Documents.Word.Shape.Size">Size</see>
    /// property to zero.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlLine : VmlShape
    {
        #region Member variables
        private bool invertY = false;
        #endregion Member variables

        #region Constructor
        internal VmlLine( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region InvertY
        /// <summary>
        /// Returns or sets a boolean value which determines whether the
        /// y-axis of the line is inverted.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The origin for a line is always implied to be the top-left
        /// corner of it bounding rectangle. When the bounding rectangle's
        /// height is non-zero, the slope of the line is negative (resembles
        /// a backslash). Setting the InvertY property switches the orientation
        /// of the y-axis, making the slope of the line positive.
        /// </p>
        /// <p class="body">
        /// In the case where either the width or height component of the
        /// <see cref="Infragistics.Documents.Word.Shape.Size">Size</see>
        /// is zero, this property has no effect.
        /// </p>
        /// </remarks>
        public bool InvertY
        {
            get { return this.invertY; }
            set { this.invertY = value; }
        }
        #endregion InvertY

        #endregion Properties

        #region Abstract/Virtual

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { return ShapeType.Line; } }
        #endregion Type

        #region _FlipY
        
        internal override bool _FlipY { get { return this.InvertY; } }
        
        #endregion _FlipY

        #region _ConnectorType
        internal override string _ConnectorType { get { return "straight"; } }
        #endregion _ConnectorType

        #endregion Abstract/Virtual

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.invertY = false;
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion VmlLine class

    #region VmlRotatableShape class
    /// <summary>
    /// Base class for shapes that support rotational transformation.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlRotatableShape : VmlShape
    {
        #region Member variables
		private Color fillColor = WordUtilities.ColorEmpty;
        private int rotation = 0;
        #endregion Member variables

        #region Constructor
        internal VmlRotatableShape( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region FillColor
        /// <summary>
        /// Returns or sets the color used to fill the interior of the shape.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The FillColor property controls the color of the interior region.
        /// Use the
        /// <see cref="Infragistics.Documents.Word.VmlShape.LineColor">LineColor</see>
        /// property to change the color of the perimeter of the shape.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.VmlShape.LineColor">LineColor</seealso>
        public Color FillColor
        {
            get { return this.fillColor; }
            set { this.fillColor = value; }
        }
        #endregion FillColor

        #region Rotation
        /// <summary>
        /// Returns or sets the rotation for the shape, expressed in degrees.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Use positive values between 1 and 180 for clockwise rotation about the x-axis.
        /// </p>
        /// <p class="body">
        /// Use negative values between -1 and -180 for counter-clockwise rotation about the x-axis.
        /// </p>
        /// <p class="body">
        /// Rotating a shape by exactly 180 degrees is the equivalent of inverting it about the x-axis.
        /// </p>
        /// </remarks>
        public int Rotation
        {
            get { return this.rotation; }
            set
            { 
                int normalized = value;
                if ( normalized > 180 )
                    normalized -= 360;
                else
                if ( normalized < -180 )
                    normalized += 360;

                if ( normalized < -180 || normalized > 180 )
                    throw new Exception( SR.GetString("Exception_VmlRotatableShape_Rotation") );

                this.rotation = normalized;
            }
        }
        #endregion Rotation

        #endregion Properties

        #region Abstract

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { throw new NotSupportedException(); } }
        #endregion Type

        #region _BackColor
        
        internal override Color _BackColor { get { return this.FillColor; } }
        
        #endregion _BackColor

        #region _FlipX
        
        internal override bool _FlipX { get { return false; } }
        
        #endregion _FlipX

        #region _Rotation
        
        internal override int _Rotation { get { return this.Rotation; } }
        
        #endregion _Rotation

        #endregion Abstract

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.fillColor = WordUtilities.ColorEmpty;
            this.rotation = 0;
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion VmlLine class

    #region VmlRectangle class
    /// <summary>
    /// Renders a rectangle using Vector Markup Language (VML).
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlRectangle : VmlRotatableShape
    {
        #region Member variables
        
        #endregion Member variables

        #region Constructor
        internal VmlRectangle( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #endregion Properties

        #region Abstract

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { return ShapeType.Rectangle; } }
        #endregion Type

        #endregion Abstract

        #region Methods

        #endregion Methods
    }
    #endregion VmlRectangle class

    #region VmlEllipse class
    /// <summary>
    /// Renders an ellipse using Vector Markup Language (VML)
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlEllipse : VmlRotatableShape
    {
        #region Member variables
        
        #endregion Member variables

        #region Constructor
        internal VmlEllipse( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #endregion Properties

        #region Abstract

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { return ShapeType.Ellipse; } }
        #endregion Type

        #endregion Abstract

        #region Methods

        #endregion Methods
    }
    #endregion VmlEllipse class

    #region VmlIsosceleseTriangle class
    /// <summary>
    /// Renders a triangle whose apex is at the top center of the bounding rectangle,
    /// using Vector Markup Language (VML).
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The apex of the triangle can be made to appear at the bottom
    /// of the bounding rectangle by setting the
    /// <see cref="Infragistics.Documents.Word.VmlRotatableShape.Rotation">Rotation</see>
    /// property to 180 degrees.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlIsosceleseTriangle : VmlRotatableShape
    {
        #region Member variables
        
        #endregion Member variables

        #region Constructor
        internal VmlIsosceleseTriangle( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #endregion Properties

        #region Abstract

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { return ShapeType.IsosceleseTriangle; } }
        #endregion Type

        #endregion Abstract

        #region Methods

        #endregion Methods
    }
    #endregion VmlIsosceleseTriangle class

    #region VmlRightTriangle class
    /// <summary>
    /// Renders a right triangle whose apex is at the top left of the bounding rectangle,
    /// using Vector Markup Language (VML).
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// The orientation of the triangle can be changed using the
    /// <see cref="Infragistics.Documents.Word.VmlRightTriangle.InvertX">InvertX</see>
    /// and
    /// <see cref="Infragistics.Documents.Word.VmlRightTriangle.InvertY">InvertY</see>
    /// properties.
    /// </p>
    /// <p class="body">
    /// Setting both the InvertX and InvertY properties to true results
    /// in no change to the shape's orientation. 
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class VmlRightTriangle : VmlRotatableShape
    {
        #region Member variables

        private bool invertX = false;
        private bool invertY = false;
        
        #endregion Member variables

        #region Constructor
        internal VmlRightTriangle( IUnitOfMeasurementProvider unitOfMeasurementProvider) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region InvertX
        /// <summary>
        /// Returns or sets a boolean value which determines whether the
        /// x-axis of the triangle is inverted.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The apex for a right triangle is always implied to be aligned with
        /// the the left edge of the bounding rectangle. To align it with
        /// the right edge, set this property to true.
        /// </p>
        /// </remarks>
        public bool InvertX
        {
            get { return this.invertX; }
            set { this.invertX = value; }
        }

        internal override bool _FlipX { get { return this.InvertX; } }

        #endregion InvertX

        #region InvertY
        /// <summary>
        /// Returns or sets a boolean value which determines whether the
        /// y-axis of the line is inverted.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The apex for a right triangle is always implied to point to
        /// the top edge of the bounding rectangle. To switch vertical
        /// orientation, set the InvertY property to true.
        /// </p>
        /// </remarks>
        public bool InvertY
        {
            get { return this.invertY; }
            set { this.invertY = value; }
        }

        internal override bool _FlipY { get { return this.InvertY; } }
        
        #endregion InvertY

        #endregion Properties

        #region Abstract

        #region Type
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which describes the type of the shape.
        /// </summary>
        public override ShapeType Type { get { return ShapeType.RightTriangle; } }
        #endregion Type

        #endregion Abstract

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            this.invertX = false;
            this.invertY = false;
        }
        #endregion Reset

        #endregion Methods
    }
    #endregion VmlRightTriangle class

    #region InlineShape
    /// <summary>
    /// Enacpsulates a shape which appears inline with textual content.
    /// </summary>
    public class InlineShape : SizeableContent
    {
        #region Member variables
        //private ShapeType   shapeType = ShapeType.Line;
        private Shape       shape = null;
        #endregion Member variables

        #region Constructor
        internal InlineShape( IUnitOfMeasurementProvider unitOfMeasurementProvider, ShapeType shapeType ) : base( unitOfMeasurementProvider, null )
        {
            this.Reset( shapeType );
        }

        internal InlineShape( IUnitOfMeasurementProvider unitOfMeasurementProvider, Shape shape ) : base( unitOfMeasurementProvider, null )
        {
            this.Reset( shape );
        }

        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="shapeType">
        /// A
        /// <see cref="Infragistics.Documents.Word.ShapeType">ShapeType</see>
        /// constant which defines the shape.
        /// </param>
        static public InlineShape Create( WordDocumentWriter writer, ShapeType shapeType )
        {
            return new InlineShape( writer, shapeType );
        }

        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="shape">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>.
        /// </param>
        static public InlineShape Create( WordDocumentWriter writer, Shape shape )
        {
            return new InlineShape( writer, shape );
        }
        #endregion Constructor

        #region Shape
        /// <summary>
        /// Returns the associated
        /// <see cref="Infragistics.Documents.Word.Shape">Shape</see>
        /// </summary>
        public Shape Shape { get { return this.shape; } }
        #endregion Shape

        #region Size
        internal override SizeF? SizeRawValue
        {
            get
            {
                SizeF? mySize = base.SizeRawValue;
                if ( mySize.HasValue )
                    return mySize.Value;

                return this.shape.SizeRawValue;
            }
        }
        #endregion Size

        #region DefaultSize
        internal override SizeF DefaultSizeInTwips
        {
            get { return new SizeF( WordUtilities.TwipsPerInch, WordUtilities.TwipsPerInch ); }
        }
        #endregion DefaultSize

        #region Reset
        /// <summary>
        /// Restores all property values of this instance to their respective defaults.
        /// </summary>
        public void Reset( ShapeType shapeType )
        {
            base.Reset();
            this.shape = VmlShape.Create( this.unitOfMeasurementProvider, shapeType );
        }

        /// <summary>
        /// Restores all property values of this instance to their respective defaults.
        /// </summary>
        public void Reset( Shape shape )
        {
            if ( shape == null )
                throw new ArgumentNullException("shape");

            base.Reset();
            this.shape = shape;
        }
        #endregion Reset

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            return this.shape != null ? this.shape.ToString() : base.ToString();
        }
        #endregion ToString
    }
    #endregion InlineShape

    #region AnchoredPicture class

    /// <summary>
    /// Encapsulates a picture or image that is anchored to a specific
    /// location within the document.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// Unlike an inline picture, which moves along with adjacent content,
    /// an AnchoredPicture remains at a fixed location within the paragraph,
    /// with adjacent text flowing around it.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class AnchoredPicture : Anchor
    {
        #region Constants
        
        #endregion Constants

        #region Member variables

        private Padding                             textPadding = Padding.Empty;
        private PictureOutlineProperties            outline = null;
        private Image                               image = null;

        #endregion Member variables

        #region Constructor (internal)
        internal AnchoredPicture( IUnitOfMeasurementProvider unitOfMeasurementProvider, Image image ) : base( unitOfMeasurementProvider )
        {
            if ( image == null )
                throw new ArgumentNullException( "image" );
            this.image = image;
        }

        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="image">The associated image.</param>
        static public AnchoredPicture Create( WordDocumentWriter writer, Image image )
        {
            return new AnchoredPicture( writer, image );
        }

        #endregion Constructor (internal)

        #region Properties

        #region TextPadding
        /// <summary>
        /// Returns or sets the padding between the picture and the
        /// text which flows around it.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is only applicable when the
        /// <see cref="Infragistics.Documents.Word.Anchor.TextWrapping">TextWrapping</see>
        /// property is set to 'Square' or 'TopAndBottom'. When set to 'TopAndBottom',
        /// only the vertical components of the value are applicable.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        public Padding TextPadding
        {
            get { return WordUtilities.ConvertPaddingFromTwips(this.Unit, this.textPadding); }
            set { this.textPadding = WordUtilities.ConvertPaddingToTwips(this.Unit, value); }
        }

        internal Padding TextPaddingInEMUs
        {
            get
            {
                float top = WordUtilities.TwipsToEMU( this.textPadding.Top );
                float left = WordUtilities.TwipsToEMU( this.textPadding.Left );
                float bottom = WordUtilities.TwipsToEMU( this.textPadding.Bottom );
                float right = WordUtilities.TwipsToEMU( this.textPadding.Right );

                return new Padding( left, top, right, bottom );
            }
        }
        #endregion TextPadding

        #region Outline
        /// <summary>
        /// Returns an object which defines the properties of the outline
        /// that is drawn around the associated picture.
        /// </summary>
        public PictureOutlineProperties Outline
        {
            get
            {
                if ( this.outline == null )
                    this.outline = new PictureOutlineProperties(this.unitOfMeasurementProvider);

                return this.outline;
            }
        }

        internal bool HasOutline { get { return this.outline != null; } }

        #endregion Outline

        #region Image

        /// <summary>
        /// Returns the underlying
        /// <a href="http://msdn.microsoft.com/en-us/library/system.windows.media.imaging.bitmapsource.aspx">Image</a>
        /// associated with this instance.
        /// </summary>


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        public Image Image
        {
            get { return this.image; }
        }
        #endregion Image

        #region DefaultSize
        internal override SizeF DefaultSizeInTwips
        {
            get { return WordUtilities.GetSize(this.Image, UnitOfMeasurement.Twip); }
        }
        #endregion DefaultSize

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        /// <param name="image">
        /// The
        /// <see cref="Infragistics.Documents.Word.AnchoredPicture.Image">Image</see>
        /// with which this instance is to be associated.
        /// </param>
        public void Reset( Image image )
        {
            if ( image == null )
                throw new ArgumentNullException( "image" );

            base.Reset();

            this.image = image;            
            this.textPadding = Padding.Empty;
        }
        #endregion Reset

        #region ToString
        /// <summary>
        /// Returns the string representation of this object.
        /// </summary>
        public override string ToString()
        {
            SizeF? size = Anchor.GetSize( this.SizeRawValue, this.Unit );
            string unit = WordUtilities.UnitToString(this.Unit);
            return size.HasValue ? string.Format("AnchoredPicture ({0}{2}W x {1}{2}H)", size.Value.Width, size.Value.Height, unit) : base.ToString();
        }
        #endregion ToString

        #endregion Methods
    }
    #endregion AnchoredPicture class

    #region PictureOutlineProperties
    /// <summary>
    /// Encapsulates the properties of the outline that is
    /// displayed around an anchored or inline picture.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class PictureOutlineProperties : WordPropertiesBase
    {
        #region Constants
        

        #endregion Constants

        #region Member variables

        private PictureOutlineStyle             style = WordUtilities.DefaultPictureOutlineStyle;
        private float?                          lineWidth = null;
        private Color                           color = WordUtilities.ColorEmpty;
        private PictureOutlineCornerStyle       cornerStyle = WordUtilities.DefaultPictureOutlineCornerStyle;

        #endregion Member variables

        #region Constructor (internal)
        internal PictureOutlineProperties( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor (internal)

        #region Properties

        #region Color
        /// <summary>
        /// Returns or sets a value which determines the color of the outline.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The Color property is not applicable unless the
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties.Style">Style</see>
        /// property is set to a value other than 'None'.
        /// </p>
        /// </remarks>
        public Color Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        internal bool HasColor { get { return WordUtilities.ColorIsEmpty(this.color) == false; } }

        #endregion Color

        #region CornerStyle
        /// <summary>
        /// Returns or sets a value indicating the corner style of the outline.
        /// </summary>
        public PictureOutlineCornerStyle CornerStyle
        {
            get { return this.cornerStyle; }
            set { this.cornerStyle = value; }
        }
        #endregion Style

        #region Style
        /// <summary>
        /// Returns or sets a value indicating the style of the outline.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The Style property resolves to 'None', so that by default no outline
        /// is drawn around the associated picture.
        /// </p>
        /// </remarks>
        public PictureOutlineStyle Style
        {
            get { return this.style; }
            set { this.style = value; }
        }
        #endregion Style

        #region LineWidth
        /// <summary>
        /// Returns or sets a value indicating the width of the outline.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The LineWidth property is not applicable unless the
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties.Style">Style</see>
        /// property is set to a value other than 'None'.
        /// </p>
        /// </remarks>
        public float? LineWidth
        {
            get
            {
                return this.lineWidth.HasValue ?
                    WordUtilities.ConvertFromTwips( this.Unit, this.lineWidth.Value ) :
                    this.lineWidth;
            }

            set
            {
                if ( value.HasValue == false || value.Value == 0f )
                {
                    this.lineWidth = null;
                    return;
                }

                if ( value < float.Epsilon )
                    throw new ArgumentOutOfRangeException("value");

                this.lineWidth = WordUtilities.ConvertToTwips( this.Unit, value.Value );
            }
        }

        internal int LineWidthInEMUs
        {
            get
            {
                //  If the Style property was set but no other properties were,
                //  draw a 1/4-point thick black line around it.
                float twips =
                    this.lineWidth.HasValue ?
                    this.lineWidth.Value :
                    this.style != PictureOutlineStyle.None ?
                    (WordUtilities.TwipsPerPoint / 4) :
                    0f;

                return WordUtilities.TwipsToEMU( twips );
            }
        }
        #endregion LineWidth

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
        /// <see cref="Infragistics.Documents.Word.PictureOutlineProperties">PictureOutlineProperties</see>
        /// instance.
        /// </returns>
        public static PictureOutlineProperties Create(WordDocumentWriter writer)
        {
            return new PictureOutlineProperties(writer);
        }
        #endregion Create

        #region Reset
        /// <summary>
        /// Resets all properties of this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.style = WordUtilities.DefaultPictureOutlineStyle;
            this.lineWidth = null;
            this.color = WordUtilities.ColorEmpty;
            this.cornerStyle = WordUtilities.DefaultPictureOutlineCornerStyle;
        }
        #endregion Reset

        #endregion Methods

    }
    #endregion PictureOutlineProperties

    #region InlinePicture
    /// <summary>
    /// Enacpsulates a picture which appears inline with textual content.
    /// </summary>
    public class InlinePicture : SizeableContent
    {
        #region Member variables
        private Image image = null;
        #endregion Member variables

        #region Constructor
        internal InlinePicture( IUnitOfMeasurementProvider unitOfMeasurementProvider, Image image ) : base( unitOfMeasurementProvider, null )
        {
            this.Reset( image );
        }

        /// <summary>
        /// Creates a new instance which is associated with the specified
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </summary>
        /// <param name="writer">
        /// The associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>
        /// </param>
        /// <param name="image">The associated image.</param>
        static public InlinePicture Create( WordDocumentWriter writer, Image image )
        {
            return new InlinePicture( writer, image );
        }

        #endregion Constructor

        #region Image

        /// <summary>
        /// Returns the underlying
        /// <a href="http://msdn.microsoft.com/en-us/library/system.windows.media.imaging.bitmapsource.aspx">Image</a>
        /// associated with this instance.
        /// </summary>


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        public Image Image
        {
            get { return this.image; }
        }
        #endregion Image

        #region DefaultSize
        internal override SizeF DefaultSizeInTwips
        {
            get { return WordUtilities.GetSize(this.Image, UnitOfMeasurement.Twip); }
        }
        #endregion DefaultSize

        #region Reset
        /// <summary>
        /// Restores all property values of this instance to their respective defaults.
        /// </summary>
        public void Reset( Image image )
        {
            if ( image == null )
                throw new ArgumentNullException( "image" );

            base.Reset();
            this.image = image;
        }
        #endregion Reset
    }
    #endregion InlinePicture
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