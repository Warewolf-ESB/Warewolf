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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// The <see cref="Column"/> object that represents <see cref="ColumnLayoutTemplateCell"/> objects.
	/// </summary>
	internal class ColumnLayoutTemplateColumn : Column
	{
		#region Members
		ColumnWidth _width;
		#endregion // Members

		#region Constructor 

        internal ColumnLayoutTemplateColumn()
        {
            this._width = new ColumnWidth(1, true);
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Stretch;
        }
		#endregion // Constructor		

		#region Overrides

		#region Width
		/// <summary>
		/// The width of a <see cref="ColumnLayoutTemplateColumn"/> is always *. 
		/// </summary>
		public override ColumnWidth WidthResolved
		{
			get
			{
				return this._width;
			}
		}
		#endregion // Width

		#region GenerateCell

		/// <summary>
		/// If given a <see cref="ColumnLayoutTemplateRow"/> a <see cref="ColumnLayoutTemplateCell"/> will be returned.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <returns></returns>
		protected internal override CellBase GenerateCell(RowBase row)
		{
			CellBase cb = null;

			if (row is ColumnLayoutTemplateRow)
				cb = new ColumnLayoutTemplateCell(row, this);

			return cb;
		}

		#endregion // GenerateCell

		#region IsResizable

		/// <summary>
		/// A <see cref="ColumnLayoutTemplateColumn"/> is not resizable.
		/// </summary>
		public override bool IsResizable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsResizable

		#region IsFixable

		/// <summary>
		/// A <see cref="ColumnLayoutTemplateColumn"/> can not be locked into place.
		/// </summary>
		public override bool IsFixable
		{
			get
			{
				return false;
			}
		}
		#endregion // IsFixable

		#region IsSortable

		/// <summary>
		/// A <see cref="ColumnLayoutTemplateColumn"/> is always sortable.
		/// </summary>
		public override bool IsSortable
		{
			get
			{
				return true;
			}
		}
		#endregion // IsSortable

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