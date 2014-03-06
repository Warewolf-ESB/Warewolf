using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Infragistics.Controls.Grids.Primitives
{
    /// <summary>
    /// The <see cref="FilterSelectorControl"/> controls which <see cref="SelectionControl"/> will be displayed in the FilterMenu.
    /// </summary>
    public class FilterSelectorControl : ContentControl
    {
        #region Members
        SelectionControl _filterSelectionControl;
        #endregion // Members

        #region Constructor


        /// <summary>
        /// Static constructor for the <see cref="FilterSelectorControl"/> class.
        /// </summary>
        static FilterSelectorControl()
        {
            Style style = new Style();
            style.Seal();
            FocusVisualStyleProperty.OverrideMetadata(typeof(FilterSelectorControl), new FrameworkPropertyMetadata(style));
        }


        #endregion // Constructor

        #region Properties

        #region Cell

        /// <summary>
        /// Identifies the <see cref="Cell"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty CellProperty = DependencyProperty.Register("Cell", typeof(CellBase), typeof(FilterSelectorControl), new PropertyMetadata(new PropertyChangedCallback(CellChanged)));

        /// <summary>
        /// Gets / sets the <see cref="CellBase"/> object which hosts the <see cref="FilterSelectorControl"/>.
        /// </summary>
        public CellBase Cell
        {
            get { return (CellBase)this.GetValue(CellProperty); }
            set { this.SetValue(CellProperty, value); }
        }

        private static void CellChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FilterSelectorControl ssc = (FilterSelectorControl)obj;
            ssc.OnCellAssigned();
        }

        #endregion // Cell

        #endregion // Properties

        #region Overrides

        #region OnCellAssigned

        /// <summary>
        /// Raised when a Cell is assigned to the control.
        /// </summary>
        protected virtual void OnCellAssigned()
        {
            if (this.Cell != null && this.Cell.Column != null)
            {
                this._filterSelectionControl = this.Cell.Column.GenerateFilterSelectionControl();
                this._filterSelectionControl.Cell = this.Cell;
                this.Content = this._filterSelectionControl;
            }
        }

        #endregion // OnCellAssigned

        #endregion // Overrides
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