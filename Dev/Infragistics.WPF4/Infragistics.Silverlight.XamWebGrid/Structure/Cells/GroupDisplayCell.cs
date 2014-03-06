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
    /// A cell that represents a <see cref="Cell"/> in a <see cref="GroupDisplayColumn"/>
    /// </summary>
    public class GroupDisplayCell : Cell
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="AddNewRowCell"/> class.
        /// </summary>
        /// <param propertyName="row">The <see cref="Row"/> object that owns the <see cref="GroupDisplayCell"/></param>
        /// <param propertyName="column">The <see cref="Column"/> object that the <see cref="GroupDisplayCell"/> represents.</param>
        protected internal GroupDisplayCell(RowBase row, Column column)
            : base(row, column)
        {
        }

        #endregion // Constructor

        #region Overrides
        #region CreateInstanceOfRecyclingElement
        /// <summary>
        /// Creates a new instance of a <see cref="GroupDisplayCellControl"/> for the <see cref="GroupDisplayCell"/>.
        /// </summary>
        /// <returns>A new <see cref="GroupDisplayCellControl"/></returns>
        /// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
        protected override CellControlBase CreateInstanceOfRecyclingElement()
        {
            return new GroupDisplayCellControl();
        }
        #endregion // CreateInstanceOfRecyclingElement

        #region OnCellDoubleClick
        /// <summary>
        /// Invoked when a cell is double clicked.
        /// </summary>
        protected internal override void OnCellDoubleClick()
        {
            XamGridConditionInfo data = (XamGridConditionInfo)this.Row.Data;

            ToggleOperator();

            base.OnCellDoubleClick();
        }
        #endregion // OnCellDoubleClick


        #endregion // Overrides


        internal void ToggleOperator()
        {
            int groupDisplayColumnCount = 0;

            ReadOnlyKeyedColumnBaseCollection<Column> dataColumns = this.Column.ColumnLayout.Columns.DataColumns;

            for (int i = 0; i < dataColumns.Count; i++)
            {
                if (dataColumns[i] is GroupDisplayColumn)
                    groupDisplayColumnCount++;
            }

            int myIndex = dataColumns.IndexOf(this.Column);

            // ok so at this point I know what index I am at, so I need to go up to the level that I am 

            XamGridConditionInfoGroup currentGroup = ((XamGridConditionInfo)this.Row.Data).Group;

            while (currentGroup != null)
            {
                if (currentGroup.Level == myIndex)
                {
                    if (currentGroup.Operator == LogicalOperator.Or)
                        currentGroup.Operator = LogicalOperator.And;
                    else
                        currentGroup.Operator = LogicalOperator.Or;

                    this.Row.ColumnLayout.Grid.InvalidateScrollPanel(false);
                    break;
                }
                currentGroup = currentGroup.ParentGroup;
            }            
        }
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