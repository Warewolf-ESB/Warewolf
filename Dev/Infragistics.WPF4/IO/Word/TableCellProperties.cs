using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;


using System.Windows;
using System.Windows.Media;







using SR = Infragistics.Shared.SR;


namespace Infragistics.Documents.Word
{    
    #region TableCellPropertiesBase class
    /// <summary>
    /// Encapsulates the basic properties of a table cell, which are applicable
    /// at different levels of the property resolution hierarchy.
    /// </summary>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class TableCellPropertiesBase : WordPropertiesBase
    {
        #region Member variables

        private Color                       backColor = WordUtilities.ColorEmpty;
        private TableCellVerticalAlignment  verticalAlignment = TableCellVerticalAlignment.Default;
        private Padding?                    margins = null;

        #endregion Member variables

        #region Constructor
        internal TableCellPropertiesBase( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base ( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region BackColor
        /// <summary>
        /// Returns or sets the background color for this cell.
        /// </summary>
        public Color BackColor
        {
            get { return this.backColor; }
            set { this.backColor = value; }
        }
        #endregion BackColor

        #region Margins
        /// <summary>
        /// Returns or sets a value which determines the amount of padding
        /// applied between the inside of this cell's borders and its content.
        /// </summary>
        public Padding? Margins
        {
            get
            {
                return this.margins.HasValue ? WordUtilities.ConvertPaddingFromTwips( this.Unit, this.margins.Value ) : this.margins;
            }

            set
            {
                this.margins = value.HasValue ?
                    WordUtilities.ConvertPaddingToTwips( this.Unit, value.Value ) :
                    (Padding?)null;
            }
        }

        internal bool HasMargins { get { return this.margins.HasValue; } }

        #endregion Margins

        #region VerticalAlignment
        /// <summary>
        /// Returns or sets a value indicating the vertical alignment
        /// of the cell's contents.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Horizontal alignment of a cell's contents is defined at the paragraph level.
        /// </p>
        /// </remarks>
        /// <see cref="Infragistics.Documents.Word.ParagraphPropertiesBase.Alignment">Alignment</see>
        public TableCellVerticalAlignment VerticalAlignment
        {
            get { return this.verticalAlignment; }
            set { this.verticalAlignment = value; }
        }

        #endregion VerticalAlignment

        #region Virtual
        
        internal virtual int                    _ColumnSpan { get { return 0; } }
        internal virtual TableCellVerticalMerge _VerticalMerge { get { return TableCellVerticalMerge.None; } }
        internal virtual TableCellTextDirection _TextDirection { get { return WordUtilities.DefaultTableCellTextDirection; } }
        internal virtual TableBorderProperties  _BorderProperties { get { return null; } }

        internal virtual bool ShouldSerializeBorderProperties() { return this._BorderProperties != null && this._BorderProperties.ShouldSerialize(); }

        internal virtual int? GetPreferredWidth( out string unit )
        { 
            unit = null;
            return null;
        }

        #endregion Virtual

        #endregion Properties

        #region Methods

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.backColor = WordUtilities.ColorEmpty;

            this.margins = null;
            this.verticalAlignment = TableCellVerticalAlignment.Default;
        }
        #endregion Reset

        #region ShouldSerialize
        internal virtual bool ShouldSerialize()
        {
            return
                WordUtilities.ColorIsEmpty(this.backColor) == false ||
                this.margins != null ||
                this.verticalAlignment != TableCellVerticalAlignment.Default;
        }
        #endregion ShouldSerialize

        #region InitializeFrom
        internal virtual void InitializeFrom( TableCellPropertiesBase source )
        {
            if ( source == null )
                return;

            this.backColor = source.backColor;
            this.margins = source.margins;
            this.verticalAlignment = source.verticalAlignment;
        }
        #endregion InitializeFrom

        #endregion Methods

    }
    #endregion TableCellPropertiesBase class
    
    #region TableCellProperties class
    /// <summary>
    /// Encapsulates the properties of a table cell.
    /// </summary>
    /// <remarks>
    /// <p class="body">
    /// This class is used to override the table-level cell properties 
    /// for a cell or group of cells. One instance can be used to define
    /// the properties of multiple cells.
    /// </p>
    /// </remarks>

    [InfragisticsFeature(Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class TableCellProperties : TableCellPropertiesBase
    {
        #region Member variables

        private float?                      preferredWidth = null;
        private float?                      preferredWidthAsPercentage = null;
        private int                         columnSpan = 0;
        private TableCellVerticalMerge      verticalMerge = TableCellVerticalMerge.None;
        private TableCellTextDirection      textDirection = WordUtilities.DefaultTableCellTextDirection;
        private TableBorderProperties       borderProperties = null;

        #endregion Member variables

        #region Constructor
        internal TableCellProperties( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base ( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region BorderProperties
        /// <summary>
        /// Returns or sets an object which provides a way to customize
        /// the borders for the associated cell.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Cell border properties are inherited from the table
        /// to which the cell belongs. This property provides a way to
        /// interrupt inheritance of table borders for a cell or group of cells.
        /// </p>
        /// <p class="body">
        /// The instance returned from this property's get method will be
        /// created when it is first requested, i.e., "lazily". A previously
        /// created instance can be assigned (for example, from a previously
        /// created TableCellProperties instance), provided that they are
        /// associated with the same
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// This approach can reduce the heap fragmentation that can result from excessive
        /// object instantiation in the case where the border properties must be explicitly
        /// set for a relatively large number of cells.
        /// </p>
        /// </remarks>
        public TableBorderProperties BorderProperties
        {
            get
            {
                if ( this.borderProperties == null )
                    this.borderProperties = new TableBorderProperties( this.unitOfMeasurementProvider );
                
                return this.borderProperties;
            }

            set
            {
                if ( value != null )
                {
                    //  Only allow instance sharing if they were created by the same writer
                    if ( value.unitOfMeasurementProvider != this.unitOfMeasurementProvider )
                        throw new Exception( SR.GetString("Exception_AssociatedWithDifferentWriter") );
                }

                this.borderProperties = value;
            }
        }

        /// <summary>
        /// Returns a boolean value indicating whether an object has been created for or assigned to the
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.BorderProperties">BorderProperties</see>
        /// property.
        /// </summary>
        public bool HasBorderProperties
        {
            get { return this.borderProperties != null; }
        }

        internal override bool ShouldSerializeBorderProperties()
        {
            return this.HasBorderProperties && this.borderProperties.ShouldSerialize();
        }

        internal override TableBorderProperties _BorderProperties
        {
            get { return this.borderProperties; }
        }

        #endregion BorderProperties

        #region ColumnSpan
        /// <summary>
        /// Returns or sets an integral value which determines the number
        /// of columns across which this cell spans.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// By default, a cell spans across only one column, typically having
        /// the same PreferredWidth as that column. The ColumnSpan property makes it possible
        /// to support horizontal cell merging, whereby one cell can appear to span
        /// across two or more columns. For example, consider a grid with 3 columns,
        /// A, B, and C. If the developer wants the cell in position A to extend to
        /// the left edge of cell C, effectively skipping cell B, the ColumnSpan
        /// property of cell A would be set to 2.
        /// </p>
        /// <p class="body">
        /// In addition to setting the ColumnSpan property, the
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.PreferredWidth">PreferredWidth</see>
        /// property must also be set to an appropriate value in order
        /// to make a cell span across multiple columns. Using the example
        /// above, the PreferredWidth of cell A must be set to (A.PreferredWidth + B.PreferredWidth)
        /// in order to make it span to the left edge of cell C.
        /// </p>
        /// <p class="body">
        /// The default value of zero logically represents a span of one column.
        /// </p>
        /// </remarks>
        public int ColumnSpan
        {
            get
            { 
                return this.columnSpan;
            }
            
            set
            {
                if ( value < 0 )
                    throw new ArgumentOutOfRangeException("value");

                this.columnSpan = value;
            }
        }

        internal override int _ColumnSpan
        {
            get { return this.columnSpan; }
        }
        #endregion ColumnSpan

        #region TextDirection
        /// <summary>
        /// Returns or sets a value indicating the direction of
        /// text flow for text within the associated table cell.
        /// </summary>
        public TableCellTextDirection TextDirection
        {
            get { return this.textDirection; }
            set { this.textDirection = value; }
        }

        internal override TableCellTextDirection _TextDirection
        {
            get { return this.textDirection; }
        }
        #endregion TextDirection

        #region VerticalMerge
        /// <summary>
        /// Returns or sets a value indicating whether this cell should
        /// begin or continue a vertical merging run with adjacent cells.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// Similarly to how the
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.ColumnSpan">ColumnSpan</see>
        /// property supports horizontal cell merging, the VerticalMerge
        /// property can be used to start a vertical merging run, or
        /// continue an existing one.
        /// </p>
        /// <p class="body">
        /// When set to 'Start', a vertical merging run begins with this cell.
        /// Subsequent cells whose VerticalMerge property is set to 'Continue'
        /// will appear to merge with the cell directly above in the same column.
        /// When set to 'None', any existing vertical merging run is interrupted.
        /// The 'Start' setting also discontinues any existing vertical merging run.
        /// </p>
        /// </remarks>
        public TableCellVerticalMerge VerticalMerge
        {
            get { return this.verticalMerge; }
            set { this.verticalMerge = value; }
        }

        internal override TableCellVerticalMerge _VerticalMerge
        {
            get { return this.verticalMerge; }
        }

        #endregion VerticalMerge

        #region PreferredWidth
        /// <summary>
        /// Returns or sets the preferred width of the associated table cell,
        /// expressed as an explicit linear quantity.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property is only applicable when the
        /// <see cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</see>
        /// property is set to 'Auto'.
        /// </p>
        /// <p class="body">
        /// The PreferredWidth property provides a way to explicitly define the width
        /// of the associated table cell. The value is considered preferred because
        /// the actual width may be different based on the current table layout algorithm.
        /// </p>
        /// <p class="body">
        /// The unit of measurement used to express the property value is defined by the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property of the associated
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter">WordDocumentWriter</see>.
        /// </p>
        /// <p class="body">
        /// The PreferredWidth property and the
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.PreferredWidthAsPercentage">PreferredWidthAsPercentage</see>
        /// properties are mutually exclusive, and the PreferredWidth property takes precedence,
        /// so in the case where both properties are explicitly set, the PreferredWidthAsPercentage
        /// property is not applicable.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidth">PreferredWidth</seealso>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.Layout">Layout</seealso>
        public float? PreferredWidth
        {
            get
            { 
                if ( this.preferredWidth.HasValue == false )
                    return this.preferredWidth;

                return WordUtilities.ConvertFromTwips( this.Unit, this.preferredWidth.Value );
            }

            set
            {
                if ( value.HasValue == false || value.Value == 0f )
                {
                    this.preferredWidth = null;
                    return;
                }

                if ( value < float.Epsilon )
                    throw new ArgumentOutOfRangeException("value");

                this.preferredWidth = WordUtilities.ConvertToTwips( this.Unit, value.Value );
            }
        }
        #endregion PreferredWidth

        #region PreferredWidthAsPercentage
        /// <summary>
        /// Returns or sets the preferred width of the associated table cell,
        /// expressed as a percentage of the width of the containing table.
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
        /// <see cref="Infragistics.Documents.Word.TableCellProperties.PreferredWidth">PreferredWidth</see>
        /// properties are mutually exclusive, and the PreferredWidth property takes precedence,
        /// so in the case where both properties are explicitly set, the PreferredWidth property
        /// takes precedence, and this property is not applicable.
        /// </p>
        /// </remarks>
        /// <seealso cref="Infragistics.Documents.Word.TableCellProperties.PreferredWidth">PreferredWidth</seealso>
        /// <seealso cref="Infragistics.Documents.Word.TableProperties.PreferredWidth">PreferredWidth (TableProperties class)</seealso>
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
        /// <see cref="Infragistics.Documents.Word.TableCellProperties">TableCellProperties</see>
        /// instance.
        /// </returns>
        public static TableCellProperties Create(WordDocumentWriter writer)
        {
            return new TableCellProperties(writer);
        }
        #endregion Create

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            if ( this.borderProperties != null )
                this.borderProperties.Reset();

            this.columnSpan = 0;
            this.textDirection = TableCellTextDirection.Normal;
            this.verticalMerge = TableCellVerticalMerge.None;
            this.preferredWidth = null;
            this.preferredWidthAsPercentage = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal override bool ShouldSerialize()
        {
            return
                base.ShouldSerialize() ||
                (this.borderProperties != null && this.borderProperties.ShouldSerialize()) ||
                this.columnSpan > 0 ||
                this.textDirection != TableCellTextDirection.Normal ||
                this.verticalMerge != TableCellVerticalMerge.None ||
                this.preferredWidth.HasValue ||
                this.preferredWidthAsPercentage.HasValue;
        }
        #endregion ShouldSerialize

        #region GetPreferredWidth
        internal override int? GetPreferredWidth( out string unit )
        {
            return TableProperties.GetPreferredWidth( this.preferredWidth, this.preferredWidthAsPercentage, out unit );
        }
        #endregion GetPreferredWidth

        #region InitializeFrom
        internal void InitializeFrom( TableCellProperties source )
        {
            if ( source == null )
                return;

            base.InitializeFrom( source as TableCellPropertiesBase );

            if ( source.borderProperties != null )
            {
                if ( this.borderProperties == null )
                    this.borderProperties = new TableBorderProperties( this.unitOfMeasurementProvider );

                this.borderProperties.InitializeFrom( source.BorderProperties );
            }
            else
                this.borderProperties = source.borderProperties;

            this.columnSpan = source.columnSpan;
            this.preferredWidth = source.preferredWidth;
            this.preferredWidthAsPercentage = source.preferredWidthAsPercentage;
            this.textDirection = source.textDirection;
            this.verticalMerge = source.verticalMerge;
        }
        #endregion InitializeFrom

        #endregion Methods

    }
    #endregion TableCellProperties class
    
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