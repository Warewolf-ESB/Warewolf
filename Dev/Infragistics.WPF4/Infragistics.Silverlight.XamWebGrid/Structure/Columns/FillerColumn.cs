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
	/// A <see cref="Column"/> that fills any remaining space in the <see cref="XamGrid"/>.
	/// </summary>
	public sealed class FillerColumn : Column
	{
		#region Members
		ColumnWidth _width;
		#endregion // Members

		#region Constructor
		internal FillerColumn()
		{
			this._width = new ColumnWidth(1, true);
		}
		#endregion // Constructor

		#region Overrides

		#region Width
		/// <summary>
		/// The width of a <see cref="FillerColumn"/> is always *. 
		/// </summary>
		public override ColumnWidth WidthResolved
		{
			get
			{
				return this._width;
			}
		}
		#endregion // Width

		#region IsSortable
		/// <summary>
		/// A <see cref="FillerColumn"/>  is not Sortable
		/// </summary>
		public override bool IsSortable
		{
			get { return false; }
		}
		#endregion // IsSortable

		#region IsFixable

		/// <summary>
		/// A <see cref="FillerColumn"/> is not Fixable.
		/// </summary>
		public override bool IsFixable
		{
			get { return false; }
		}

		#endregion // IsFixable

		#region IsFilterable

		/// <summary>
		/// A <see cref="FillerColumn"/> is not Filterable.
		/// </summary>
		public override bool IsFilterable
		{
			get
			{
				return false;
			}
		}

		#endregion // IsFilterable

		#region IsMovable

		/// <summary>
		/// A <see cref="FillerColumn"/>  is not Movable
		/// </summary>
		public override bool IsMovable
		{
			get { return false; }
		}

		#endregion // IsMovable

		#region IsResizable
		/// <summary>
		/// A <see cref="FillerColumn"/>  is not Resizable.
		/// </summary>
		public override bool IsResizable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsResizable

		#region SupportsHeaderFooterContent

		/// <summary>
		/// Gets whether a <see cref="HeaderCell"/> or <see cref="FooterCell"/> are allowed to display
		/// content that's not directly set on them, such as the <see cref="XamGrid.ColumnsHeaderTemplate"/> property.
		/// </summary>
		protected internal override bool SupportsHeaderFooterContent
		{
			get { return false; }
		}

		#endregion // SupportsHeaderFooterContent

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

		#region FillAvailableFilters

		/// <summary>
		/// Fills the <see cref="FilterOperandCollection"/> with the operands that the column expects as filter values.
		/// </summary>
		/// <param name="availableFilters"></param>
		protected internal override void FillAvailableFilters(FilterOperandCollection availableFilters)
		{
			return;
		}

		#endregion // FillAvailableFilters

		#region IsSummable

		/// <summary>
		/// Gets / sets if the column will show the UI for SummaryRow.
		/// </summary>
		public override bool IsSummable
		{
			get
			{
				return false;
			}
		}

		#endregion // IsSummable

		#region CellStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="CellControl"/>
		/// </summary>
		public override Style CellStyleResolved
		{
			get
			{
				if (this.CellStyle == null)
					return this.ColumnLayout.FillerColumnSettings.StyleResolved;
				else
					return this.CellStyle;
			}
		}

		#endregion // CellStyleResolved

		#region HeaderStyleResolved


		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="HeaderCellControl"/>
		/// </summary>
		public override Style HeaderStyleResolved
		{
			get
			{
				if (this.HeaderStyle == null)
					return this.ColumnLayout.FillerColumnSettings.HeaderStyleResolved;
				else
					return this.HeaderStyle;
			}
		}

		#endregion //HeaderStyleResolved

		#region FooterStyleResolved

		/// <summary>
		/// Resolves the actual Style that will be applied to the <see cref="FooterCellControl"/>
		/// </summary>
		public override Style FooterStyleResolved
		{
			get
			{
				if (this.FooterStyle == null)
					return this.ColumnLayout.FillerColumnSettings.FooterStyleResolved;
				else
					return this.FooterStyle;
			}
		}

		#endregion //FooterStyleResolved

        #region IsGroupable

        /// <summary>
        /// This column is not Groupable.
        /// </summary>
        public override bool IsGroupable
        {
            get
            {
                return false;
            }
            set
            {
                base.IsGroupable = value;
            }
        }
        #endregion // IsGroupable

        #region IsHideable
        /// <summary>
        /// A <see cref="FillerColumn"/>  is not Hideable
        /// </summary>
        public override bool IsHideable
        {
            get { return false; }
        }
        #endregion // IsHideable

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