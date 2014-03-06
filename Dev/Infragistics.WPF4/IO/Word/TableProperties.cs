using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;


using System.Windows;
using System.Windows.Media;





namespace Infragistics.Documents.Word
{    
    #region TablePropertiesBase class
    /// <summary>
    /// Encapsulates the properties of a table which can be applied
    /// globally to all tables in a document.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class TablePropertiesBase : ParagraphPropertiesBase
    {
        #region Member variables

        private float?                      cellSpacing = null;
        private TableBorderProperties       borderProperties = null;

        #endregion Member variables

        #region Constructor
        internal TablePropertiesBase( IUnitOfMeasurementProvider provider ) : base( provider )
        {
        }
        #endregion Constructor

        #region Properties

        #region BorderProperties
        /// <summary>
        /// Returns an object which provides a way to customize borders
        /// for the cells in the associated table.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The properties of the instance returned from this property are
        /// also inherited by the cells which appear within the associated table.
        /// For example, if the border style is set to 'Dotted' on this instance,
        /// the cells within the associated table will acquire that setting, unless
        /// the cell has its own overriding border definition.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TableCellProperties.BorderProperties">BorderProperties (TableCellProperties class)</seealso>
        public TableBorderProperties BorderProperties
        {
            get
            {
                if ( this.borderProperties == null )
                    this.borderProperties = new TableBorderProperties( this.unitOfMeasurementProvider );
                
                return this.borderProperties;
            }
        }

        internal bool ShouldSerializeBorderProperties() { return this.borderProperties != null && this.borderProperties.ShouldSerialize(); }

        #endregion BorderProperties

        #region CellSpacing
        /// <summary>
        /// Returns or sets a value which determines the amount of spacing
        /// applied between adjacent cells and the table borders.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// </remarks>
        public float? CellSpacing
        {
            get
            {
                return this.cellSpacing.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.cellSpacing.Value ) : this.cellSpacing;
            }

            set
            {
                this.cellSpacing = value.HasValue ?
                    WordUtilities.ConvertToTwips( this.Unit, value.Value ) :
                    (float?)null;
            }
        }

        internal bool HasCellSpacing { get { return this.cellSpacing.HasValue; } }
        
        #endregion CellSpacing

        #region _CellProperties
        
        internal virtual TableCellPropertiesBase _CellProperties { get { return null; } }
        
        internal bool HasCellProperties { get { return this._CellProperties != null; } }

        internal bool ShouldSerializeCellProperties() { return this.HasCellProperties && this._CellProperties.ShouldSerialize(); }

        #endregion _CellProperties

        #endregion Properties

        #region Methods

        #region GetPreferredWidth
        internal virtual int? GetPreferredWidth( out string unit )
        {
            unit = null;
            return null;
        }
        #endregion GetPreferredWidth

        #region InitializeFrom
        internal void InitializeFrom( TablePropertiesBase source )
        {
            if ( source == null )
                return;

            base.InitializeFrom( source );

            this.cellSpacing = source.cellSpacing;

            if ( source.borderProperties != null )
            {
                if ( this.borderProperties == null )
                    this.borderProperties = new TableBorderProperties( this.unitOfMeasurementProvider );

                this.borderProperties.InitializeFrom( source.borderProperties );
            }
            else
                this.borderProperties = null;
        }
        #endregion InitializeFrom

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            this.cellSpacing = null;
            this.borderProperties = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                this.ShouldSerializeBorderProperties() ||
                this.cellSpacing.HasValue;
        }
        #endregion ShouldSerialize

        #region Virtual

        internal virtual TableLayout GetLayout() { return TableLayout.Auto; }

        internal virtual Padding? _CellMargins { get { return null; } }
        internal bool HasCellMargins { get { return this._CellMargins.HasValue; } }

        #endregion Virtual

        #endregion Methods
    }
    #endregion TablePropertiesBase class

    #region DefaultTableProperties class
    /// <summary>
    /// Encapsulates the properties of a table which can be applied
    /// globally to all tables in a document.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class DefaultTableProperties : TablePropertiesBase
    {
        #region Member variables

        private TableCellPropertiesBase     cellProperties = null;

        #endregion Member variables

        #region Constructor
        internal DefaultTableProperties( IUnitOfMeasurementProvider provider ) : base( provider )
        {
        }
        #endregion Constructor

        #region Properties

        #region CellMargins

        internal override Padding? _CellMargins
        {
            get { return this.cellProperties != null && this.cellProperties.HasMargins ? this.cellProperties.Margins : null; }
        }
        #endregion CellMargins

        #region CellProperties
        /// <summary>
        /// Returns a
        /// <see cref="Infragistics.Documents.Word.TableCellPropertiesBase">TableCellPropertiesBase</see>
        /// instance which defines the default properties for table cells.
        /// </summary>
        public TableCellPropertiesBase CellProperties
        {
            get
            {
                if ( this.cellProperties == null )
                    this.cellProperties = new TableCellPropertiesBase( this.unitOfMeasurementProvider );

                return this.cellProperties;
            }
        }

        internal override TableCellPropertiesBase _CellProperties { get { return this.CellProperties; } }

        #endregion CellProperties

        #endregion Properties

        #region Methods

        #region InitializeFrom
        internal void InitializeFrom( DefaultTableProperties source )
        {
            if ( source == null )
                return;

            base.InitializeFrom( source as TablePropertiesBase );

            if ( source.cellProperties != null )
            {
                if ( this.cellProperties == null )
                    this.cellProperties = new TableCellPropertiesBase( this.unitOfMeasurementProvider );

                this.cellProperties.InitializeFrom( source.cellProperties );
            }
            else
                this.cellProperties = null;

        }
        #endregion InitializeFrom

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            if ( this.cellProperties != null )
                this.cellProperties.Reset();
        }
        #endregion Reset

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                this.ShouldSerializeCellProperties();
        }
        #endregion ShouldSerialize

        #endregion Methods
    }
    #endregion DefaultTableProperties class

    #region TableProperties class
    /// <summary>
    /// <summary>
    /// Encapsulates the properties of a table which can be applied
    /// to a specific table.
    /// </summary>
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class TableProperties : TablePropertiesBase
    {
        #region Member variables

        private TableLayout             layout = TableLayout.Auto;
        private float?                  preferredWidth = null;
        private float?                  preferredWidthAsPercentage = null;
        private Padding?                cellMargins = null;

        #endregion Member variables

        #region Constructor
        internal TableProperties( IUnitOfMeasurementProvider provider ) : base( provider )
        {
        }
        #endregion Constructor

        #region Properties

        #region CellMargins
        /// <summary>
        /// Returns or sets a value which determines the amount of padding
        /// applied between the inside of the cell borders and the cell content.
        /// </summary>
        public Padding? CellMargins
        {
            get
            {
                return this.cellMargins.HasValue ? WordUtilities.ConvertPaddingFromTwips( this.Unit, this.cellMargins.Value ) : this.cellMargins;
            }

            set
            {
                this.cellMargins = value.HasValue ?
                    WordUtilities.ConvertPaddingToTwips( this.Unit, value.Value ) :
                    (Padding?)null;
            }
        }

        internal override Padding? _CellMargins
        {
            get { return this.CellMargins; }
        }

        #endregion CellMargins
        
        #region Layout
        /// <summary>
        /// Returns or sets a value which determines the layout
        /// algorithm that is used to determine the size and position
        /// of cells in the associated table.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// By default, the table layout is automatic, which means that
        /// cells are automatically sized to fully display their contents.
        /// The Layout property can be used to apply explicit column widths.
        /// </p>
        /// </remarks>
        public TableLayout Layout
        {
            get { return this.layout; }
            set { this.layout = value; }
        }
        #endregion Layout

        #region PreferredWidth
        /// <summary>
        /// Returns or sets the preferred width of the associated table, expressed
        /// as an explicit linear quantity.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is only applicable when the
        /// <see cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</see>
        /// property is set to 'Fixed'.
        /// </p>
        /// <p class="body">
        /// The value of this property is considered preferred because a consumer
        /// may elect to override this value based on the current table layout algorithm.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// <p class="body">
        /// The PreferredWidth property and the
        /// <see cref="Infragistics.Documents.Word.TableProperties.PreferredWidthAsPercentage">PreferredWidthAsPercentage</see>
        /// properties are mutually exclusive, and the PreferredWidth property takes precedence,
        /// so in the case where both properties are explicitly set, the PreferredWidthAsPercentage
        /// property is not applicable.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidthAsPercentage">PreferredWidthAsPercentage</seealso>
        /// <seealso cref="Infragistics.Documents.Word.TableCellProperties.PreferredWidthAsPercentage">PreferredWidthAsPercentage (TableCellProperties class)</seealso>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</seealso>
        public float? PreferredWidth
        {
            get
            {
                return this.preferredWidth.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.preferredWidth.Value ) : this.preferredWidth;
            }

            set
            {
                this.preferredWidth = value.HasValue ?
                    WordUtilities.ConvertToTwips( this.Unit, value.Value ) :
                    (float?)null;
            }
        }
        #endregion PreferredWidth

        #region PreferredWidthAsPercentage
        /// <summary>
        /// Returns or sets the preferred width of the associated table,
        /// expressed as a percentage of the width of the containing element.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is only applicable when the
        /// <see cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</see>
        /// property is set to 'Fixed'.
        /// </p>
        /// <p class="body">
        /// This value is considered preferred because a consumer may elect to
        /// override this value based on the current table layout algorithm.
        /// </p>
        /// <p class="body">
        /// The valid range for this property's value is a number between .02 and 100.
        /// Note that the unit of measure is <b>not</b> defined by the associated
        /// document writer's unit of measure.
        /// </p>
        /// <p class="body">
        /// The PreferredWidthAsPercentage property and the
        /// <see cref="Infragistics.Documents.Word.TableProperties.PreferredWidth">PreferredWidth</see>
        /// properties are mutually exclusive, and the PreferredWidth property takes precedence,
        /// so in the case where both properties are explicitly set, the PreferredWidth property
        /// takes precedence, and this property is not applicable.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidth">PreferredWidth</seealso>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</seealso>
        public float? PreferredWidthAsPercentage
        {
            get { return this.preferredWidthAsPercentage; }
            set 
            {
                if ( this.preferredWidthAsPercentage.HasValue )
                {
                    WordUtilities.VerifyFloatPropertySetting(
                        "percent",
                        "PreferredWidthAsPercentage",
                        this.preferredWidthAsPercentage.Value,
                        .02f,
                        100f);
                }

                this.preferredWidthAsPercentage = value;
            }
        }
        #endregion PreferredWidthAsPercentage

        #endregion Properties

        #region Methods

        #region InitializeFrom
        internal void InitializeFrom( TableProperties source )
        {
            if ( source == null )
                return;

            base.InitializeFrom( source );

            this.cellMargins = source.cellMargins;
            this.layout = source.layout;
            this.preferredWidth = source.preferredWidth;
            this.preferredWidthAsPercentage = source.preferredWidthAsPercentage;
        }
        #endregion InitializeFrom

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
        /// <see cref="Infragistics.Documents.Word.TableProperties">TableProperties</see>
        /// instance.
        /// </returns>
        public static TableProperties Create(WordDocumentWriter writer)
        {
            return new TableProperties(writer);
        }
        #endregion Create
        
        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            this.cellMargins = null;
            this.layout = TableLayout.Auto;
            this.preferredWidth = null;
            this.preferredWidthAsPercentage = null;
        }
        #endregion Reset

        #region GetPreferredWidth
        internal override int? GetPreferredWidth( out string unit )
        {
            return TableProperties.GetPreferredWidth( this.preferredWidth, this.preferredWidthAsPercentage, out unit );
        }

        static internal int? GetPreferredWidth( float? preferredWidth, float? preferredWidthAsPercentage, out string unit )
        {
            unit  = null;

            if ( preferredWidth.HasValue )
            {
                unit = preferredWidth.Value == 0 ? "auto" : "dxa";
                return (int)preferredWidth.Value;
            }
            else
            if ( preferredWidthAsPercentage.HasValue )
            {
                unit = "pct";
                int fiftieths = (int)(preferredWidthAsPercentage.Value * 50);
                return fiftieths;
            }
            else
                return null;
        }
        #endregion GetPreferredWidth

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                this.cellMargins.HasValue ||
                this.layout != TableLayout.Auto ||
                this.preferredWidth.HasValue ||
                this.preferredWidthAsPercentage.HasValue;
        }
        #endregion ShouldSerialize

        #region Virtual

        internal override TableLayout GetLayout() { return this.Layout; }
        //internal override float? GetPreferredWidth() { return this.PreferredWidth; }
        //internal override float? GetPreferredWidthAsPercentage() { return this.PreferredWidthAsPercentage; }

        #endregion Virtual

        #endregion Methods
    }
    #endregion TableProperties class

    #region TableBorderProperties
    /// <summary>
    /// Encapsulates the properties of the borders applied to tables and cells.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public class TableBorderProperties : WordPropertiesBase
    {
        #region Constants
      





        static internal int DefaultBorderWidthInEighthsOfAPoint = 4;
        





        static private float MinBorderWidthInTwips = .25f * WordUtilities.TwipsPerPoint;
        





        static private float MaxBorderWidthInTwips = 12f * WordUtilities.TwipsPerPoint;
        
        #endregion Constants

        #region Member variables

        //  BF 3/11/11
        //  Changed 'style' and 'width' to default to null...a nullable
        //  member should default to null :-/

        private TableBorderStyle?               style = null;
        private Color                           color = WordUtilities.ColorEmpty;
        private float?                          width = null;
        private TableBorderSides?               sides = null;

        #endregion Member variables

        #region Constructor
        internal TableBorderProperties( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region Sides
        /// <summary>
        /// Returns or sets a value which defines whether the top, left,
        /// right, and bottom borders are drawn for a table or cell.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// All borders are drawn for all cells in a table by default;
        /// however, in cases where the border style does not preclude
        /// it, the borders of adjacent cells are merged, so as not
        /// to cause the borders to appear "thicker" than the user would
        /// expect. Because of these border merging algorithms, setting
        /// this property may not yield the expected result.
        /// </p>
        /// <p class="body">
        /// Note that the value of this property takes precedence over the
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.VerticalMerge">VerticalMerge</see>
        /// and
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.ColumnSpan">ColumnSpan</see>
        /// properties with regard to border drawing.
        /// </p>
        /// </remarks>
        public TableBorderSides? Sides
        {
            get { return this.sides; }
            set { this.sides = value; }
        }

        #endregion Sides

        #region Style
        /// <summary>
        /// Returns or sets a value which determines the style of the borders.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public TableBorderStyle? Style
        {
            get { return this.style; }
            set { this.style = value; }
        }

        #endregion Style

        #region Color
        /// <summary>
        /// Returns or sets a value which determines the color of the borders.
        /// </summary>
        public Color Color
        {
            get { return this.color; }
            set { this.color = value; }
        }

        internal bool HasColor { get { return WordUtilities.ColorIsEmpty(this.color) == false; } }

        #endregion Color

        #region Width
        /// <summary>
        /// Returns or sets a value which determines the width of the borders.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// WordprocessingML persists border widths in units equal to one-eighth
        /// of a desktop publishing point; this equates to 1/576 of an inch. The range
        /// of valid values spans from one-fourth of a point to 12 points.
        /// </p>
        /// <p class="body">
        /// A value which is based on a specific number of pixels can be assigned using the
        /// <see cref="Infragistics.Documents.Word.TableBorderProperties.SetWidthInPixels">SetWidthInPixels</see>
        /// method.
        /// </p>
        /// <p class="body">
        /// WordprocessingML supports border thicknesses up to one-sixth of an inch. Values
        /// which exceed this amount are not recognized, and the resulting border thickness
        /// resolves to this value.
        /// </p>
        /// </remarks>
        public float? Width
        {
            get
            {
                return this.width.HasValue ? WordUtilities.ConvertFromTwips( this.Unit, this.width.Value ) : this.width;
            }

            set
            {
                this.width = value.HasValue ?
                    WordUtilities.ConvertToTwips( this.Unit, value.Value ) :
                    (float?)null;
            }
        }

        internal int WidthInEighthsOfAPoint
        {
            get
            {
                if ( this.width.HasValue == false )
                    return TableBorderProperties.DefaultBorderWidthInEighthsOfAPoint;

                //  Return the value in eights of a point
                return (int)TableBorderProperties.TwipsToEighthsOfAPoint(this.width.Value);
            }
        }

        static private float TwipsToEighthsOfAPoint( float value )
        {
            if ( value < 0f )
                return 0f;

            return (value / WordUtilities.TwipsPerPoint) * 8f;
        }

        /// <summary>
        /// Assigns a value to the
        /// <see cref="Infragistics.Documents.Word.TableBorderProperties.Width">Width</see>
        /// property that is equals to the specified number of pixels at the
        /// specified resolution.
        /// </summary>
        /// <param name="value">The border's width, expressed as an integral number of pixels.</param>
        /// <param name="dpi">The pixel resolution, expressed as the number of pixels required to span one inch.</param>
        /// <remarks>
        /// <p class="body">
        /// Values less than zero are not permitted for either parameter; an exception is thrown when a negative value is assigned.
        /// Values that would exceed the minimum or maximum allowable value are rounded to the min/max as required.
        /// </p>
        /// </remarks>
        public void SetWidthInPixels( int value, float dpi )
        {
            if ( value < 0 )
                throw new ArgumentOutOfRangeException( "value" );

            if ( dpi < 0f )
                throw new ArgumentOutOfRangeException( "dpi" );

            //  Get the value in inches
            float inches = value / dpi;

            //  Convert inches to twips and assign.
            float twips = WordUtilities.Convert( inches, UnitOfMeasurement.Inch, UnitOfMeasurement.Twip );

            //  Clip at the min/max
            twips = Math.Min( twips, TableBorderProperties.MaxBorderWidthInTwips );
            twips = Math.Max( twips, TableBorderProperties.MinBorderWidthInTwips );

            this.width = twips;
        }

        #endregion Width

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
        /// <see cref="Infragistics.Documents.Word.TableBorderProperties">TableBorderProperties</see>
        /// instance.
        /// </returns>
        public TableBorderProperties Create(WordDocumentWriter writer)
        {
            return new TableBorderProperties(writer);
        }
        #endregion Create

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.color = WordUtilities.ColorEmpty;
            this.sides = null;
            this.style = null;
            this.width = null;
        }
        #endregion Reset

        #region InitializeFrom
        internal void InitializeFrom( TableBorderProperties source )
        {
            if ( source == null )
                return;

            this.color = source.color;
            this.sides = source.sides;
            this.style = source.style;
            this.width = source.width;
        }
        #endregion Reset

        #region ShouldSerialize
        internal bool ShouldSerialize()
        {
            return
                this.HasColor ||
                this.sides.HasValue ||
                this.style.HasValue ||
                this.width.HasValue;
        }
        #endregion ShouldSerialize

        #region Merge
        internal void Merge( TableBorderProperties source )
        {
            TableBorderProperties.Merge( source, this );
        }

        static internal void Merge( TableBorderProperties source, TableBorderProperties target )
        {
            if ( source == null || target == null )
                return;

            if ( WordUtilities.ColorIsEmpty(target.color) && WordUtilities.ColorIsEmpty(source.color) == false )
                target.color = source.color;

            if ( target.sides.HasValue == false && source.sides.HasValue )
                target.sides = source.sides;

            if ( target.style.HasValue == false && source.style.HasValue )
                target.style = source.style;

            if ( target.width.HasValue == false && source.width.HasValue )
                target.width = source.width;
        }
        #endregion Merge

        #endregion Methods
    }
    #endregion TableBorderProperties
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