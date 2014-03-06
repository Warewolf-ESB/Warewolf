using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;


using System.Windows;
using System.Windows.Media;
using SizeF = System.Windows.Size;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{
    #region ParagraphPropertiesBase class
    /// <summary>
    /// Provides a way to control formatting for a paragraph or table
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class ParagraphPropertiesBase : WordPropertiesBase
    {
        #region Constants

        /// <summary>
        /// Returns the maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.ParagraphPropertiesBase.LeftIndent">LeftIndent</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.RightIndent">RightIndent</see>
        /// property, expressed in twentieths of a point (31,680).
        /// </summary>
        public const int                        IndentMaxValue = 31680;
        
        #endregion Constants

        #region Member variables

        private ParagraphAlignment?             alignment = null;
        private float?                          leftIndent = null;

        #endregion Member variables

        #region Constructor
        internal ParagraphPropertiesBase( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region Alignment
        /// <summary>
        /// Returns or sets a value which determines the horizontal alignment
        /// for the associated paragraph or table.
        /// </summary>
        public ParagraphAlignment? Alignment
        {
            get { return this.alignment; }
            set { this.alignment = value; }
        }
        #endregion Alignment

        #region LeftIndent
        /// <summary>
        /// Returns or sets a value which determines the indentation
        /// from the left margin for the associated paragraph or table.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// For tables, the LeftIndent property is not applicable when the
        /// <see cref="Infragistics.Documents.Word.ParagraphPropertiesBase.Alignment">Alignment</see>
        /// property is set to 'Center' or 'Right'.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.IndentMaxValue">IndentMaxValue</seealso>
        public float? LeftIndent
        {
            get { return this.leftIndent.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.leftIndent.Value ) : this.leftIndent; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "LeftIndent",
                    newValue,
                    ParagraphPropertiesBase.IndentMaxValue * -1,
                    ParagraphPropertiesBase.IndentMaxValue);

                this.leftIndent = newValue;
            }
        }

        internal int LeftIndentInTwips { get { return this.leftIndent.HasValue ? (int)this.leftIndent.Value : 0; } }

        #endregion LeftIndent

        #endregion Properties

        #region Methods

        #region InitializeFrom
        internal void InitializeFrom( ParagraphPropertiesBase source )
        {
            this.alignment = source.alignment;
            this.leftIndent = source.leftIndent;
        }
        #endregion InitializeFrom

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.alignment = null;
            this.leftIndent = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal virtual bool ShouldSerialize()
        {
            return this.alignment.HasValue || this.leftIndent.HasValue;
        }
        #endregion ShouldSerialize

        #endregion Methods
    }
    #endregion ParagraphPropertiesBase class

    #region ParagraphProperties class
    /// <summary>
    /// Provides a way to control formatting for a paragraph.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class ParagraphProperties : ParagraphPropertiesBase
    {
        #region Constants
        
        /// <summary>
        /// Returns the maximum allowable value for the line spacing
        /// properties, expressed in twentieths of a point (-20).
        /// </summary>
        public const int                        SpacingMinValue = -20;
        
        /// <summary>
        /// Returns the maximum allowable value for the line spacing
        /// properties, expressed in twentieths of a point (31,680).
        /// </summary>
        public const int                        SpacingMaxValue = 31680;
        
        /// <summary>
        /// Returns the maximum allowable value for the
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAuto">LineSpacingAuto</see>
        /// property, expressed as an integral number of lines (132 lines).
        /// </summary>
        public const int                        LineSpacingAutoMaxValue = 132;
        
        #endregion Constants

        #region Member variables

        private float?                          rightIndent = null;
        private float?                          spacingBefore = null;
        private float?                          spacingAfter = null;
        private float?                          lineSpacingMinimum = null;
        private float?                          lineSpacingExact = null;
        private float?                          lineSpacingAuto = null;
        private bool?                           pageBreakBefore = null;
        private bool?                           rightToLeft = null;

        #endregion Member variables

        #region Constructor
        internal ParagraphProperties( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region RightIndent
        /// <summary>
        /// Returns or sets a value which determines the indentation
        /// from the right margin for the associated paragraph.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphPropertiesBase.IndentMaxValue">IndentMaxValue</seealso>
        public float? RightIndent
        {
            get { return this.rightIndent.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.rightIndent.Value ) : this.rightIndent; }
            set
            { 
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "RightIndent",
                    newValue,
                    ParagraphPropertiesBase.IndentMaxValue * -1,
                    ParagraphPropertiesBase.IndentMaxValue);

                this.rightIndent = newValue;
            }
        }

        internal int RightIndentInTwips { get { return this.rightIndent.HasValue ? (int)this.rightIndent.Value : 0; } }

        #endregion RightIndent

        #region SpacingBefore
        /// <summary>
        /// Returns or sets a value which determines the amount of spacing
        /// that appears above the paragraph.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMinValue">SpacingMinValue</seealso>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMaxValue">SpacingMaxValue</seealso>
        public float? SpacingBefore
        {
            get { return this.spacingBefore.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.spacingBefore.Value ) : this.spacingBefore; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "SpacingBefore",
                    newValue,
                    ParagraphProperties.SpacingMinValue,
                    ParagraphProperties.SpacingMaxValue);

                this.spacingBefore = newValue;
            }
        }

        internal int SpacingBeforeInTwips { get { return this.spacingBefore.HasValue ? (int)this.spacingBefore.Value : 0; } }

        #endregion SpacingBefore

        #region SpacingAfter
        /// <summary>
        /// Returns or sets a value which determines the amount of spacing
        /// that appears after the last line of the associated paragraph.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMinValue">SpacingMinValue</seealso>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMaxValue">SpacingMaxValue</seealso>
        public float? SpacingAfter
        {
            get { return this.spacingAfter.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.spacingAfter.Value ) : this.spacingAfter; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "SpacingAfter",
                    newValue,
                    ParagraphProperties.SpacingMinValue,
                    ParagraphProperties.SpacingMaxValue);

                this.spacingAfter = newValue;
            }
        }

        internal int SpacingAfterInTwips { get { return this.spacingAfter.HasValue ? (int)this.spacingAfter.Value : 0; } }

        #endregion SpacingAfter

        #region LineSpacingExact
        /// <summary>
        /// Returns or sets a value which determines the exact amount of
        /// vertical spacing that appears between lines of text in the associated
        /// paragraph.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The LineSpacingExact,
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingMinimum">LineSpacingMinimum</see>,
        /// and
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAuto">LineSpacingAuto</see>
        /// properties are mutually exclusive; when one property is set, the others are not applicable.
        /// </p>
        /// <p class="body">
        /// In the case where more than one of the properties is explicitly set,
        /// the order of precedence is as follows:
        /// <ul>
        /// <li>LineSpacingExact</li>
        /// <li>LineSpacingMinimum</li>
        /// <li>LineSpacingAuto</li>
        /// </ul>
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMaxValue">SpacingMaxValue</seealso>
        public float? LineSpacingExact
        {
            get { return this.lineSpacingExact.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.lineSpacingExact.Value ) : this.lineSpacingExact; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "LineSpacingExact",
                    newValue,
                    0,
                    ParagraphProperties.SpacingMaxValue);

                this.lineSpacingExact = newValue;
            }
        }

        #endregion LineSpacingExact

        #region LineSpacingMinimum
        /// <summary>
        /// Returns or sets a value which determines the minimum amount of
        /// vertical spacing that appears between lines of text in the associated
        /// paragraph. Applicable only when the
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact">LineSpacingExact</see>
        /// property is not explicitly set.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact">LineSpacingExact</see>,
        /// The LineSpacingMinimum,
        /// and
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAuto">LineSpacingAuto</see>
        /// properties are mutually exclusive; when one property is set, the others are not applicable.
        /// </p>
        /// <p class="body">
        /// In the case where more than one of the properties is explicitly set,
        /// the order of precedence is as follows:
        /// <ul>
        /// <li>LineSpacingExact</li>
        /// <li>LineSpacingMinimum</li>
        /// <li>LineSpacingAuto</li>
        /// </ul>
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.SpacingMaxValue">SpacingMaxValue</seealso>
        public float? LineSpacingMinimum
        {
            get { return this.lineSpacingMinimum.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.lineSpacingMinimum.Value ) : this.lineSpacingMinimum; }
            set
            {
                float? newValue = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value;

                WordUtilities.VerifyFloatPropertySetting(
                    this.Unit,
                    "LineSpacingMinimum",
                    newValue,
                    0,
                    ParagraphProperties.SpacingMaxValue);

                this.lineSpacingMinimum = newValue;
            }
        }

        #endregion LineSpacingMinimum

        #region LineSpacingAuto
        /// <summary>
        /// Returns or sets a value which determines the amount of
        /// vertical spacing that appears between lines of text in the associated
        /// paragraph, expressed as a multiple of the line height for the paragraph.
        /// Applicable only when the
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact">LineSpacingExact</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingMinimum">LineSpacingMinimum</see>
        /// properties are not explicitly set.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The LineSpacingAuto property provides a way for the developer to space lines
        /// such that the actual value is automatically calculated by the document
        /// consumer based on the line height for the associated paragraph. An explicit
        /// value can be specified for line spacing using the
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact">LineSpacingExact</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// The
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingExact">LineSpacingExact </see>,
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingMinimum">LineSpacingMinimum</see>
        /// and LineSpacingAuto properties are mutually exclusive; when one property is set, the others are not applicable.
        /// </p>
        /// <p class="body">
        /// In the case where more than one of the properties is explicitly set,
        /// the order of precedence is as follows:
        /// <ul>
        /// <li>LineSpacingExact</li>
        /// <li>LineSpacingMinimum</li>
        /// <li>LineSpacingAuto</li>
        /// </ul>
        /// </p>
        /// <p class="body">
        /// Unlike the LineSpacingExact and LineSpacingMinimum properties,
        /// this property's value is always expressed as a multiple
        /// of the line height for the paragraph, and is never converted
        /// based on the value of the associated WordDocumentWriter's 
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// For example, to apply vertical spacing between lines of text that
        /// is equal to .5 times the height of the line itself, assign a value
        /// of .5 to the LineSpacingAuto property, and leave the values of the
        /// LineSpacingExact and LineSpacingMinimum properties at their respective
        /// defaults.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.ParagraphProperties.LineSpacingAutoMaxValue">LineSpacingAutoMaxValue</seealso>
        public float? LineSpacingAuto
        {
            get { return this.lineSpacingAuto.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.lineSpacingAuto.Value ) : this.lineSpacingAuto; }
            set
            {
                float? newValue = value.HasValue ? value.Value : value;

                WordUtilities.VerifyFloatPropertySetting(
                    "lines",
                    "LineSpacingAuto",
                    newValue,
                    0,
                    ParagraphProperties.LineSpacingAutoMaxValue);

                this.lineSpacingAuto = newValue;
            }
        }

        #endregion LineSpacingAuto

        #region PageBreakBefore
        /// <summary>
        /// Returns or sets a value which determines whether a page break
        /// is inserted before the associated paragraph.
        /// </summary>
        public bool? PageBreakBefore
        {
            get { return this.pageBreakBefore; }
            set { this.pageBreakBefore = value; }
        }
        #endregion RightIndent

        #region RightToLeft
        /// <summary>
        /// Returns or sets a value which determines whether the paragraph
        /// is presented in a right-to-left direction.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property only applies to paragraph-level properties, and does not
        /// affect the layout of the text within. Setting this property to true has
        /// an effect similar to that of setting the
        /// <see cref="Infragistics.Documents.Word.ParagraphPropertiesBase.Alignment">Alignment</see>
        /// property to 'Right'.
        /// </p>
        /// <p class="body">
        /// To change the layout of the text within the paragraph, use the
        /// <see cref="Infragistics.Documents.Word.Font.RightToLeft">RightToLeft</see>
        /// property of the
        /// <see cref="Infragistics.Documents.Word.Font">Font</see>
        /// class.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.Font.RightToLeft">RightToLeft</seealso>
        public bool? RightToLeft
        {
            get { return this.rightToLeft; }
            set { this.rightToLeft = value; }
        }
        #endregion RightIndent

        #region HasSpacing
        internal bool HasSpacing
        {
            get
            {
                return 
                    this.lineSpacingExact.HasValue ||
                    this.lineSpacingMinimum.HasValue ||
                    this.lineSpacingAuto.HasValue ||
                    this.spacingAfter.HasValue ||
                    this.spacingBefore.HasValue;
            }
        }
        #endregion HasSpacing

        #endregion Properties

        #region Methods

        #region GetLineSpacing
        internal int GetLineSpacing( out LineSpacingRule? rule )
        {
            rule = null;

            if ( this.lineSpacingExact.HasValue )
            {
                rule = LineSpacingRule.Exact;
                return (int)this.lineSpacingExact.Value;
            }
            else
            if ( this.lineSpacingMinimum.HasValue )
            {
                rule = LineSpacingRule.AtLeast;
                return (int)this.lineSpacingMinimum.Value;
            }
            else
            if ( this.lineSpacingAuto.HasValue )
            {
                rule = LineSpacingRule.Auto;
                return (int)(this.lineSpacingAuto.Value * 240);
            }

            return 0;
        }
        #endregion GetLineSpacing

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
        /// <see cref="Infragistics.Documents.Word.ParagraphProperties">ParagraphProperties</see>
        /// instance.
        /// </returns>
        public static ParagraphProperties Create(WordDocumentWriter writer)
        {
            return new ParagraphProperties(writer);
        }
        #endregion Create

        #region InitializeFrom
        internal void InitializeFrom( ParagraphProperties source )
        {
            base.InitializeFrom( source );

            this.rightIndent = source.rightIndent;
            this.spacingBefore = source.spacingBefore;
            this.spacingAfter = source.spacingAfter;
            this.lineSpacingExact = source.lineSpacingExact;
            this.lineSpacingMinimum = source.lineSpacingMinimum;
            this.lineSpacingAuto = source.lineSpacingAuto;
            this.pageBreakBefore = source.pageBreakBefore;
            this.rightToLeft = source.rightToLeft;
        }
        #endregion InitializeFrom

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            this.rightIndent = null;
            this.spacingBefore = null;
            this.spacingAfter = null;
            this.lineSpacingExact = null;
            this.lineSpacingMinimum = null;
            this.lineSpacingAuto = null;
            this.pageBreakBefore = null;
            this.rightToLeft = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                this.lineSpacingAuto.HasValue ||
                this.lineSpacingExact.HasValue ||
                this.lineSpacingMinimum.HasValue ||
                this.pageBreakBefore.HasValue ||
                this.rightIndent.HasValue ||
                this.rightToLeft.HasValue ||
                this.spacingAfter.HasValue ||
                this.spacingBefore.HasValue;
        }
        #endregion ShouldSerialize

        #endregion Methods
    }
    #endregion ParagraphProperties class

    #region SectionProperties class
    /// <summary>
    /// Provides a way to control page attributes such as size, margins, and orientation.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class SectionProperties : WordPropertiesBase
    {
        #region Member variables

        private SizeF?                  pageSize = null;
        private Padding?                pageMargins = null;
        private float?                  headerMargin = null;
        private float?                  footerMargin = null;
        private int?                    paperCode = null;
        private PageOrientation         pageOrientation = PageOrientation.Default;
        private List<HeaderFooterInfo>  headerFooterInfoItems = null;
        private int?                    startingPageNumber = null;

        #endregion Member variables

        #region Constructor
        internal SectionProperties(IUnitOfMeasurementProvider unitOfMeasurementProvider) : base(unitOfMeasurementProvider)
        {
        }
        #endregion Constructor

        #region Properties

        #region PageSize
        /// <summary>
        /// Returns or sets the size for all pages in the associated section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// When the
        /// <see cref="Infragistics.Documents.Word.SectionProperties.PageOrientation">PageOrientation</see>
        /// property is left at its default setting, the PageSize property defines the effective
        /// orientation. In cases where the width is greater than the height, the PageOrientation property
        /// resolves to 'Landscape'.
        /// </p>
        /// <p class="body">
        /// In the absence of an explicit setting, this property resolves to a value
        /// that is equal to 8.5 inches in width by 11 inches in height, or, if the
        /// PageOrientation property is explicitly set to 'Landscape', 11" x 8.5".
        /// </p>
        /// <p class="body">
        /// The maximum page size in MS Word is 22" x 22" (see
        /// <a href="http://support.microsoft.com/kb/95109">KB article 95109</a>).
        /// An exception is thrown if the specified value is outside this range.
        /// </p>
        /// </remarks>
        public SizeF? PageSize
        {
            get
            {
                return
                    this.pageSize.HasValue ?
                    WordUtilities.ConvertSizeFromTwips(this.Unit, this.pageSize.Value) :
                    this.pageSize;
            }

            set
            {
                if (value.HasValue == false)
                    this.pageSize = null;
                else
                {
                    if (value.Value.Width < float.Epsilon || value.Value.Height < float.Epsilon)
                        throw new ArgumentOutOfRangeException("value");

                    //  TFS70627
                    //this.pageSize = WordUtilities.ConvertSizeToTwips(this.Unit, value.Value);
                    SizeF valueInTwips = WordUtilities.ConvertSizeToTwips(this.Unit, value.Value);
                    SizeF max = this.MaxPageSizeInTwips;

                    if ( valueInTwips.Width > max.Width || valueInTwips.Height > max.Height )
                    {
                        string unit = WordUtilities.UnitToString(this.Unit);
                        string s = string.Format( "{1}{0} x {2}{0}", unit, value.Value.Width, value.Value.Height );
                        throw new Exception( SR.GetString("Exception_SectionProperties_MaxPageSize", s) );
                    }

                    this.pageSize = valueInTwips;
                }
            }
        }

        private SizeF MaxPageSizeInTwips
        {
            get { return new SizeF( WordUtilities.TwipsPerInch * 22, WordUtilities.TwipsPerInch * 22 ); }
        }



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

        internal void GetPageSizeInTwips( out int width, out int height, out PageOrientation pageOrientation )
        {
            pageOrientation = this.PageOrientation;

            SizeF size =
                this.pageSize.HasValue ?
                this.pageSize.Value :
                new SizeF( 8.5f * WordUtilities.TwipsPerInch, 11f * WordUtilities.TwipsPerInch );

            width = (int)size.Width;
            height = (int)size.Height;
            bool swap = false;

            switch ( this.PageOrientation )
            {
                case PageOrientation.Portrait:
                    pageOrientation = PageOrientation.Portrait;
                    swap = width > height;
                    break;

                case PageOrientation.Landscape:
                    pageOrientation = PageOrientation.Landscape;
                    swap = height > width;
                    break;

                case PageOrientation.Default:
                    pageOrientation = width > height ? PageOrientation.Landscape : PageOrientation.Portrait;
                    swap = false;
                    break;
            }

            if ( swap )
            {
                int temp = width;
                width = height;
                height = temp;
            }
        }

        #endregion PageSize

        #region PageMargins
        /// <summary>
        /// Returns or sets the margins for all pages in the associated section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the absence of an explicit setting, this property resolves to a value
        /// that is equal to one inch on all sides.
        /// </p>
        /// </remarks>
        public Padding? PageMargins
        {
            get { return this.pageMargins.HasValue ? WordUtilities.ConvertPaddingFromTwips(this.Unit, this.pageMargins.Value) : this.pageMargins; }
            set { this.pageMargins = value.HasValue ? WordUtilities.ConvertPaddingToTwips(this.Unit, value.Value) : value; }
        }

        internal Padding PageMarginsInTwips
        {
            get
            {
                return
                    this.pageMargins.HasValue ?
                    this.pageMargins.Value :
                    Padding.PadAll( WordUtilities.TwipsPerInch );
            }
        }

        #endregion RightIndent

        #region HeaderMargin
        /// <summary>
        /// Returns or sets the distance between the top edge of the page and the top edge of the header.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// In the absence of an explicit setting, this property resolves to .5".
        /// </p>
        /// </remarks>
        public float? HeaderMargin
        {
            get { return this.headerMargin.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.headerMargin.Value ) : this.headerMargin; }
            set { this.headerMargin = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value; }
        }

        internal bool HasHeaderMargin { get { return this.headerMargin.HasValue; } }

        internal int HeaderMarginInTwips { get { return this.headerMargin.HasValue ? (int)this.headerMargin.Value : (int)(WordUtilities.TwipsPerInch / 2); } }

        #endregion HeaderMargin

        #region FooterMargin
        /// <summary>
        /// Returns or sets the distance between the top edge of the page and the top edge of the footer.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// When set explicitly, the unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// In the absence of an explicit setting, this property resolves to .5".
        /// </p>
        /// </remarks>
        public float? FooterMargin
        {
            get { return this.footerMargin.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.footerMargin.Value ) : this.footerMargin; }
            set { this.footerMargin = value.HasValue ? WordUtilities.ConvertToTwips( this.Unit, value.Value ) : value; }
        }

        internal bool HasFooterMargin { get { return this.footerMargin.HasValue; } }
        
        internal int FooterMarginInTwips { get { return this.footerMargin.HasValue ? (int)this.footerMargin.Value : (int)(WordUtilities.TwipsPerInch / 2); } }

        #endregion FooterMargin

        #region PaperCode
        /// <summary>
        /// Returns or sets a value which represents a printer-specific paper code
        /// for the paper type for all pages in the associated section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is used to ensure that the proper paper type is chosen if the
        /// specified paper size matches the sizes of multiple paper types supported by
        /// the current printer. How this value is consumed is dependent solely on the printer,
        /// and is not interpreted or modified in any way by this class.
        /// </p>
        /// </remarks>
        public int? PaperCode
        {
            get { return this.paperCode; }
            set { this.paperCode = value; }
        }
        #endregion PaperCode

        #region PageOrientation
        /// <summary>
        /// Returns or sets a value which defines the orientation for all
        /// pages in the associated section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Under the default setting, the actual value is resolved
        /// based on the ratio of the width to the height of the page size. If
        /// the width is greater than the height, this property resolves to
        /// 'Landscape', otherwise it resolves to 'Portrait'.
        /// </p>
        /// <p class="body">
        /// When the PageOrientation property is explicitly set, the value of the
        /// <see cref="Infragistics.Documents.Word.SectionProperties.PageSize">PageSize</see>
        /// property is adjusted if necessary so that the ratio of width to height is in agreement
        /// with the page orientation. For example, if the page size is explicitly set to have a
        /// width of five inches and a height of seven, and PageOrientation is explicitly set to
        /// 'Landscape', the page size is inverted so as not to contradict the orientation.
        /// </p>
        /// </remarks>
        public PageOrientation PageOrientation
        {
            get { return this.pageOrientation; }
            set { this.pageOrientation = value; }
        }

        #endregion PageOrientation

        #region HeaderFooterInfoItems

        internal List<HeaderFooterInfo> HeaderFooterInfoItems
        {
            get { return this.headerFooterInfoItems; }
        }

        internal void SetHeaderFooterInfoItems( List<HeaderFooterInfo> value )
        {
            this.headerFooterInfoItems = value;
        }

        internal bool HasHeaderFooterInfoItems { get { return this.headerFooterInfoItems != null && this.headerFooterInfoItems.Count > 0; } }

        #endregion HeaderFooterInfoItems

        #region StartingPageNumber
        /// <summary>
        /// Returns or sets the page number for the first page in this section.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is only applicable when a header/footer is
        /// defined for the section. Headers/footers can be created using the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.AddSectionHeaderFooter(SectionHeaderFooterParts, SectionProperties)">AddSectionHeaderFooter</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// Page numbers can be specified for a header or footer using the
        /// <see cref="Infragistics.Documents.Word.WordHeaderFooterWriter.AddPageNumberField(PageNumberFieldFormat)">AddPageNumberField</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// When no value is specified for this property, page numbers continue
        /// from the previous section.
        /// </p>
        /// </remarks>
        public int? StartingPageNumber
        {
            get { return this.startingPageNumber; }
            set { this.startingPageNumber = value; }
        }
        #endregion StartingPageNumber

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
        /// <see cref="Infragistics.Documents.Word.SectionProperties">SectionProperties</see>
        /// instance.
        /// </returns>
        public static SectionProperties Create(WordDocumentWriter writer)
        {
            return new SectionProperties( writer );
        }
        #endregion Create

        #region InitializeFrom
        internal void InitializeFrom( SectionProperties source )
        {
            if ( source == null )
                return;

            this.pageSize = source.pageSize;
            this.pageMargins = source.pageMargins;
            this.headerMargin = source.headerMargin;
            this.footerMargin = source.footerMargin;
            this.paperCode = source.paperCode;
            this.pageOrientation = source.pageOrientation;
            this.startingPageNumber = source.startingPageNumber;
        }
        #endregion InitializeFrom

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.pageSize = null;
            this.pageMargins = null;
            this.headerMargin = null;
            this.footerMargin = null;
            this.paperCode = null;
            this.pageOrientation = PageOrientation.Default;
            this.startingPageNumber = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal bool ShouldSerialize()
        {
            return
                this.pageSize != null ||
                this.pageMargins != null ||
                this.headerMargin != null ||
                this.footerMargin != null ||
                this.paperCode != null ||
                this.pageOrientation != PageOrientation.Default ||
                this.startingPageNumber != null;
        }
        #endregion ShouldSerialize

        #endregion Methods
    }
    #endregion SectionProperties class

    #region HeaderFooterInfo
    internal class HeaderFooterInfo
    {
        private HeaderOrFooter type = HeaderOrFooter.Header;
        private string rId = null;
        private HeaderFooterType refType = HeaderFooterType.AllPages;
        internal HeaderFooterInfo( HeaderOrFooter type, HeaderFooterType refType, string rId )
        {
            this.type = type;
            this.refType = refType;
            this.rId = rId;
        }

        internal void SetRelationshipId( string value ) { this.rId = value; }

        internal HeaderOrFooter Type { get { return this.type; } }
        internal HeaderFooterType ReferenceType { get { return this.refType; } }
        internal string RelationshipId { get { return this.rId; } }
    }
    #endregion HeaderFooterInfo
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