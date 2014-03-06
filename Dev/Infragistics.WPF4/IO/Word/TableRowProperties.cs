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
    #region TableRowProperties class
    /// <summary>
    /// Encapsulates the properties of a table row.
    /// </summary>

    [InfragisticsFeature( Version = FeatureInfo.Version_11_1, FeatureName = FeatureInfo.FeatureName_IGWord)]

    public partial class TableRowProperties : WordPropertiesBase
    {
        #region Member variables

        private float? height = null;
        private RowHeightRule? heightRule = null;
        private float? cellSpacing = null;        
        private bool? isHeaderRow = null;        
        private bool? allowPageBreak = null;
        private int? cellsBefore = null;
        private int? cellsAfter = null;
        
        #endregion Member variables

        #region Constructor
        internal TableRowProperties( IUnitOfMeasurementProvider unitOfMeasurementProvider ) : base( unitOfMeasurementProvider )
        {
        }
        #endregion Constructor

        #region Properties

        #region CellsBefore
        /// <summary>
        /// Returns or sets a value which determines the number of
        /// logical grid units that are skipped before the first
        /// cell in this row is displayed.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// By default, the first cell of the row is displayed in the first
        /// logical grid position. This property can be used to "offset" the
        /// beginning of the row; this can be useful in situations where the
        /// row should appear indented with respect to other rows in the same
        /// table.
        /// </p>
        /// </remarks>
        public int? CellsBefore
        {
            get { return this.cellsBefore; }
            set { this.cellsBefore = value; }
        }
        #endregion CellsBefore

        #region CellsAfter
        /// <summary>
        /// Returns or sets a value which determines the number of
        /// logical grid units that are left after the last cell in
        /// this row is displayed.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// By default, the last cell of the row is displayed in the last
        /// logical grid position. This property can be used to omit one
        /// or more cells from the end of the row.
        /// </p>
        /// </remarks>
        public int? CellsAfter
        {
            get { return this.cellsAfter; }
            set { this.cellsAfter = value; }
        }
        #endregion CellsAfter

        #region CellSpacing
        /// <summary>
        /// Returns or sets a value which determines the amount of spacing
        /// applied between adjacent cells and the table borders.
        /// </summary>
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

        #region Height
        /// <summary>
        /// Returns or sets the height of the associated row.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// The unit of measure is determined by the value of the
        /// <see cref="Infragistics.Documents.Word.WordDocumentWriter.Unit">Unit</see>
        /// property.
        /// </p>
        /// <p class="body">
        /// When a value is explicitly assigned to the Height property, but not to the
        /// <see cref="Infragistics.Documents.Word.TableRowProperties.HeightRule">HeightRule</see>
        /// property, the HeightRule resolves to 'Exact'.
        /// </p>
        /// <p class="body">
        /// When the HeightRule property is set to 'Auto', the value of the Height
        /// property is not applicable.
        /// </p>
        /// </remarks>
        public float? Height
        {
            get { return this.height; }
            set { this.height = value; }
        }
        #endregion Height

        #region HeightRule
        /// <summary>
        /// Returns or sets the rule applied when calculating the
        /// <see cref="Infragistics.Documents.Word.TableRowProperties.Height">Height</see>
        /// of the associated row.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// By default, the row height rule is 'Auto', unless a value is explicitly
        /// assigned to the Height property, in which case it defaults to 'Exact'.
        /// Explicitly assigning a value to the HeightRule property overrides this
        /// behavior.
        /// </p>
        /// </remarks>
        public RowHeightRule? HeightRule
        {
            get { return this.heightRule; }
            set { this.heightRule = value; }
        }
        #endregion HeightRule

        #region IsHeaderRow
        /// <summary>
        /// Returns or sets a boolean value indicating whether the associated
        /// row is repeated at the top of each page on which it appears.
        /// </summary>
        public bool? IsHeaderRow
        {
            get { return this.isHeaderRow; }
            set { this.isHeaderRow = value; }
        }
        #endregion IsHeaderRow

        #region AllowPageBreak
        /// <summary>
        /// Returns or sets a boolean value indicating whether the associated
        /// row can span across multiple pages.
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// In the case where this property is not explicitly set, rows are
        /// allowed to span multiple pages. This property can be explicitly
        /// set to false to ensure that the associated row is not displayed
        /// on more than one page.
        /// </p>
        /// </remarks>
        public bool? AllowPageBreak
        {
            get { return this.allowPageBreak; }
            set { this.allowPageBreak = value; }
        }
        #endregion AllowPageBreak

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
        /// <see cref="Infragistics.Documents.Word.TableRowProperties">TableRowProperties</see>
        /// instance.
        /// </returns>
        public static TableRowProperties Create(WordDocumentWriter writer)
        {
            return new TableRowProperties( writer );
        }
        #endregion Create

        #region Reset
        /// <summary>
        /// Restores all property values for this instance to their respective defaults.
        /// </summary>
        public override void Reset()
        {
            this.cellSpacing = null;
            this.height = null;
            this.heightRule = null;
            this.isHeaderRow = null;
            this.allowPageBreak = null;
            this.cellsBefore = null;
            this.cellsAfter = null;
        }
        #endregion Reset

        #region ShouldSerialize
        internal bool ShouldSerialize()
        {
            return
                this.cellSpacing != null ||
                this.height != null ||
                this.heightRule != null ||
                this.isHeaderRow != null ||
                this.allowPageBreak != null ||
                this.cellsBefore != null ||
                this.cellsAfter != null;
        }
        #endregion ShouldSerialize

        #endregion Methods

    }
    #endregion TableRowProperties class
    
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