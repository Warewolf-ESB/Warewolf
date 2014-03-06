using System.Windows;
using Infragistics.Controls.Editors;
using System.Collections.Generic;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A <see cref="Column"/> that generates a <see cref="XamDateTimeInput"/> as the content for a <see cref="Cell"/>.
    /// </summary>
    public class DateTimeColumn : DateColumnBase
    {        
        #region Constructor

        /// <summary> 
		/// Initializes a new instance of the <see cref="DateTimeColumn"/> class.
		/// </summary>
        public DateTimeColumn()
        {
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

		#endregion // Constructor

        #region Overrides
        
        #region GenerateContentProvider

        /// <summary>
		/// Generates a new <see cref="DateTimeColumnContentProvider"/> that will be used to generate content for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
        protected internal override ColumnContentProviderBase GenerateContentProvider()
        {
            return new DateTimeColumnContentProvider();
        }

        #endregion // GenerateContentProvider

        #region FillAvailableFilters
        
        /// <summary>
        /// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
        /// </summary>
        /// <param name="availableFilters"></param>
        protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
        {
            base.FillAvailableFilters(availableFilters);

            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                var rule = new AreMaskedDateTimesEqualFilterOperand
                {
                    XamWebGrid = this.ColumnLayout.Grid,
                    DateTimeColumn = this
                };

                availableFilters.Add(rule);
            }
        }

        #endregion // FillAvailableFilters

        #region FillFilterMenuOptions

        /// <summary>
        /// Fills the inputted list with options for the FilterMenu control.
        /// </summary>
        /// <param name="list"></param>
        protected internal override void FillFilterMenuOptions(List<FilterMenuTrackingObject> list)
        {
            base.FillFilterMenuOptions(list);
            
            if (this.ColumnLayout != null && this.ColumnLayout.Grid != null)
            {
                if (list != null && list.Count > 0)
                {
                    FilterMenuTrackingObject fmto = list[0];
                    list = fmto.Children;

                    var rule = new AreMaskedDateTimesEqualFilterOperand
                                   {
                                       XamWebGrid = this.ColumnLayout.Grid, 
                                       DateTimeColumn = this
                                   };

                    list.Add(new FilterMenuTrackingObject(rule));
                }
            }
        }

        #endregion // FillFilterMenuOptions

        #endregion // Overrides

        #region Properties

        #region SelectedDateMask

        /// <summary>
        /// Identifies the <see cref="SelectedDateMask"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedDateMaskProperty = DependencyProperty.Register("SelectedDateMask", typeof(string), typeof(DateTimeColumn), new PropertyMetadata("{date}", new PropertyChangedCallback(SelectedDateFormatChanged)));
        
        /// <summary>
        /// Gets / sets the <see cref="XamDateTimeInput.Mask"/> that will be assigned to the <see cref="XamDateTimeInput"/> controls of the <see cref="DateTimeColumn"/>.
        /// </summary>
        public string SelectedDateMask
        {
            get { return this.GetValue(SelectedDateMaskProperty) as string; }
            set { this.SetValue(SelectedDateMaskProperty, value); }
        }

        private static void SelectedDateFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DateTimeColumn column = (DateTimeColumn)obj;
            column.OnPropertyChanged("SelectedDateMask");

            if (column.EditorDisplayBehavior == EditorDisplayBehaviors.EditMode)
            {
                if (column.ColumnLayout != null && column.ColumnLayout.Grid != null && column.ColumnLayout.Grid.IsLoaded)
                {
                    column.ColumnLayout.Grid.ResetPanelRows(true);
                }
            }
        }

        #endregion // SelectedDateMask

        #region FormatStringResolved

        /// <summary>
        /// A format string for formatting data in this column.
        /// </summary>
        protected internal override string FormatStringResolved
        {
            get
            {
                return "{0:g}";
            }
        }

        #endregion // FormatStringResolved

        #endregion // Properties
    }
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