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
	/// A <see cref="RowBase"/> which acts as a top level row for the <see cref="XamGrid"/> to summarize.
	/// </summary>
	public class SummaryRow : Row
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryRow"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManager"/> that owns the <see cref="SummaryRow"/>.</param>
		protected internal SummaryRow(RowsManager manager)
			: base(-1, manager, null)
		{
			this.FixedPositionSortOrder = 4;			
		}

		#endregion // Constructor

		#region Overrides

		#region HeightResolved

		/// <summary>
		/// Resolves the <see cref="RowBase.Height"/> property for this Row.
		/// </summary>
		public override RowHeight HeightResolved
		{
			get
			{
				return RowHeight.Dynamic;
			}
		}

		#endregion // HeightResolved

		#region RowType

		/// <summary>
		/// Gets the <see cref="RowType"/> of this <see cref="RowBase"/>
		/// </summary>
		public override RowType RowType
		{
			get { return RowType.SummaryRow; }
		}

		#endregion // RowType

		#region Public
		
		#region HasChildren

		/// <summary>
		/// Gets whether or not <see cref="ExpandableRowBase"/> has any child rows.
		/// </summary>
		public override bool HasChildren
		{
			get
			{
				return false;
			}
		}
		#endregion // HasChildren

		#endregion // Public

		#region Protected

		#region AllowEditing

		/// <summary>
		/// Gets if the <see cref="RowBase"/> object should allow editing.
		/// </summary>
		protected internal override EditingType AllowEditing
		{
			get
			{
				return EditingType.None;
			}
		}

		#endregion // AllowEditing

		#region AllowSelection
		/// <summary>
		/// Gets whether selection will be allowed on the <see cref="RowBase"/>.
		/// </summary>
		protected internal override bool AllowSelection
		{
			get
			{
				return false;
			}
		}
		#endregion // AllowSelection

		#region AllowKeyboardNavigation
		/// <summary>
		/// Gets whether the <see cref="RowBase"/> will allow keyboard navigation.
		/// </summary>
		protected internal override bool AllowKeyboardNavigation
		{
			get
			{
				return false;
			}
		}
		#endregion // AllowKeyboardNavigation

		#region RequiresFixedRowSeparator
		/// <summary>
		/// Used to determine if a FixedRow separator is neccessary for this <see cref="RowBase"/>
		/// </summary>
		protected internal override bool RequiresFixedRowSeparator
		{
			get
			{
				return true;
			}
		}
		#endregion //RequiresFixedRowSeparator

		#endregion // Protected

		#endregion // Overrides

        #region Methods

        #region InvalidateRow
        /// <summary>
        /// Cycles through the <see cref="CellBase"/>s in this <see cref="SummaryRow"/> and refreshs them.
        /// </summary>
        protected internal virtual void InvalidateRow()
        {
            foreach (CellBase cb in this.Cells)
                cb.Refresh();
        }
        #endregion // InvalidateRow

        #endregion // Methods
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