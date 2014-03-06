using System.Windows;
using Infragistics.Controls.Grids.Primitives;
using System.Windows.Media;

namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A custom column used by the <see cref="CompoundFilterDialogControl"/>.
    /// </summary>
    /// <remarks>Not for general use.
    /// </remarks>
    public class GroupDisplayColumn : UnboundColumn
    {
        #region Properties

        #region IsResizable
        /// <summary>
        /// Gets/Sets if a Column can be resized via the UI.
        /// </summary>
        public override bool IsResizable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsResizable = value;
            }
        }
        #endregion // IsResizable

        #region IsEditable

        /// <summary>
        /// Resolves whether this <see cref="Column"/> supports editing.
        /// </summary>
        protected internal override bool IsEditable
        {
            get { return false; }
        }

        #endregion // IsEditable

        #region IsSortable

        /// <summary>
        /// Gets/Sets whether the <see cref="UnboundColumn"/> is sortable. 		 
        /// </summary>
        /// <remarks>
        /// In order for a Unbound to be sortable, it must have a <see cref="Infragistics.Controls.Grids.Column.SortComparer" /> or <see cref="Infragistics.Controls.Grids.Column.ValueConverter"/>.
        /// </remarks>
        public override bool IsSortable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsSortable = value;
            }
        }
        #endregion // IsSortable

        #region IsFilterable

        /// <summary>
        /// Gets/sets if a column can be filtered via the UI.
        /// </summary>
        public override bool IsFilterable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsFilterable = value;
            }
        }

        #endregion // IsFilterable

        #region AndColorBrush

        /// <summary>
        /// Gets / sets the <see cref="Brush"/> that will be used by the control to denote that the conditions are AND'd together.
        /// </summary>
        public Brush AndColorBrush
        {
            get;
            set;
        }

        #endregion // AndColorBrush

        #region OrColorBrush

        /// <summary>
        /// Gets / sets the <see cref="Brush"/> that will be used by the control to denote that the conditions are OR'd together.
        /// </summary>
        public Brush OrColorBrush
        {
            get;
            set;
        }

        #endregion // OrColorBrush 

        #endregion // Properties

        #region GenerateContentProvider

        /// <summary>
        /// Generates a new <see cref="ColumnContentProviderBase"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
        /// </summary>
        /// <returns></returns>
        protected internal override Infragistics.Controls.Grids.Primitives.ColumnContentProviderBase GenerateContentProvider()
        {
            return new GroupDisplayColumnContentProvider();
        }

        #endregion // GenerateContentProvider

        #region GenerateDataCell

        /// <summary>
        /// Returns a new instance of a <see cref="Cell"/>
        /// </summary>
        /// <param propertyName="row"></param>
        /// <returns></returns>
        protected override CellBase GenerateDataCell(RowBase row)
        {
            if (row.RowType == RowType.DataRow)
            {
                return new GroupDisplayCell(row, this);
            }
            return base.GenerateDataCell(row);
        }

        #endregion // GenerateDataCell
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