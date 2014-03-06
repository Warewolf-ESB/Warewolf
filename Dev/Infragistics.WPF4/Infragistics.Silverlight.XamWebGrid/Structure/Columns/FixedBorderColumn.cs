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
    /// A column object that represents the area for border between pinnned and unpinned <see cref="Column"/> objects.
    /// </summary>
    public class FixedBorderColumn : Column
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="FixedBorderColumn"/> class.
        /// </summary>
        internal FixedBorderColumn()
        {
        }
        #endregion // Constructor

        #region Overrides

		#region GenerateDataCell

		/// <summary>
		/// Returns a new instance of a <see cref="FixedBorderCell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected override CellBase GenerateDataCell(RowBase row)
		{
			return new FixedBorderCell(row, this);
		}

		#endregion // GenerateDataCell

		#region GenerateHeaderCell

		/// <summary>
		/// Returns a new instance of a <see cref="FixedBorderHeaderCell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected override CellBase GenerateHeaderCell(RowBase row)
		{
			return new FixedBorderHeaderCell(row, this);
		}

		#endregion // GenerateHeaderCell

		#region GenerateFooterCell

		/// <summary>
		/// Returns a new instance of a <see cref="FixedBorderFooterCell"/>
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected override CellBase GenerateFooterCell(RowBase row)
		{
			return new FixedBorderFooterCell(row, this);
		}

		#endregion // GenerateFooterCell

		#region IsSortable
		/// <summary>
		/// A <see cref="FixedBorderColumn"/>  is not Sortable
		/// </summary>
		public override bool IsSortable
		{
			get { return false; }
		}
		#endregion // IsSortable

		#region IsFixable

		/// <summary>
		/// A <see cref="FixedBorderColumn"/>  is not Fixable
		/// </summary>
		public override bool IsFixable
		{
			get { return false; }
		}

		#endregion // IsFixable

        #region IsGroupable

        /// <summary>
        /// A <see cref="FixedBorderColumn"/>  is not Groupable
        /// </summary>
        public override bool IsGroupable
        {
            get { return false; }
        }

        #endregion // IsGroupable

        #region IsMovable

        /// <summary>
		/// A <see cref="FixedBorderColumn"/> is not Movable
		/// </summary>
		public override bool IsMovable
		{
			get { return false; }
		}

		#endregion // IsMovable

		#region IsResizable
		/// <summary>
		/// A <see cref="FixedBorderColumn"/> is not Resizable
		/// </summary>
		public override bool IsResizable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsResizable

		#region MinimumWidth
		/// <summary>
		/// Gets/sets the minimum width the column is allowed to be.
		/// </summary>
		public override double MinimumWidth
		{
			get { return 0; }
		}
		#endregion // MinimumWidth

		#region GenerateContentProvider

		/// <summary>
		/// This column does not generate any conent.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return null;
		}

		#endregion // GenerateContentProvider

		#region WidthResolved

		/// <summary>
		/// Resolves the <see cref="ColumnWidth"/> that is being applied to this <see cref="Column"/>
		/// </summary>
		public override ColumnWidth WidthResolved
		{
			get
			{
				if (this.Width == null)
					return ColumnWidth.Auto;
				else
					return (ColumnWidth)this.Width;
			}
		}
		#endregion // WidthResolved

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