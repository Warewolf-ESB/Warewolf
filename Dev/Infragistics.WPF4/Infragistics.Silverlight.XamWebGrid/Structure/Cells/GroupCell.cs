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
    /// A cell that display children cells stacked horizontally based on the children columns of the <see cref="GroupColumn"/> it represents.
    /// </summary>
    public class GroupCell : CellBase
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupCell"/> class.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        public GroupCell(RowBase row, Column column)
            : base(row, column)
        {

        }
        #endregion // Constructor

        #region Overrides

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of a <see cref="GroupCellControl"/> for the <see cref="GroupCell"/>.
        /// </summary>
        /// <returns>A new <see cref="GroupCellControl"/></returns>
        /// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
        protected override CellControlBase CreateInstanceOfRecyclingElement()
        {
            return new GroupCellControl();
        }
        #endregion // CreateInstanceOfRecyclingElement

        #region EnsureCurrentState

        /// <summary>
        /// Ensures that <see cref="GroupCell"/> is in the correct state.
        /// </summary>
        protected internal override void EnsureCurrentState()
        {
            base.EnsureCurrentState();

            if (this.Control != null)
            {
                GroupColumn gc = (GroupColumn)this.Column;

                foreach (Column col in gc.Columns)
                {
                    CellBase cell = this.Row.Cells[col];
                    cell.EnsureCurrentState();
                }
            }
        }
        #endregion // EnsureCurrentState

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