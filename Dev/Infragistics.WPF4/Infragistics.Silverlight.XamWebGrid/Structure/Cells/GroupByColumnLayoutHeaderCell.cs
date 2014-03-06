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
	/// An object that represents a <see cref="ColumnLayout"/> in the <see cref="GroupByAreaPanel"/>.
	/// </summary>
	public class GroupByColumnLayoutHeaderCell : GroupByHeaderCell
	{
		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="GroupByColumnLayoutHeaderCell"/> object.
		/// </summary>
		/// <param propertyName="row"></param>
		/// <param propertyName="column"></param>
		public GroupByColumnLayoutHeaderCell(RowBase row, Column column)
			: base(row, column)
		{

		}
		#endregion // Constructor

		#region Overrides

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="GroupByColumnLayoutHeaderCell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(GroupByColumnLayoutHeaderCellControl);
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of <see cref="GroupByColumnLayoutHeaderCellControl"/>. 
		/// Note: this method is only meant for use by the Recycling framework. 
		/// </summary>
		/// <returns>A new instance of GroupByColumnLayoutHeaderCellControl.</returns>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new GroupByColumnLayoutHeaderCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="GroupByColumnLayoutHeaderCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;

				return this.Row.ColumnLayout.GroupBySettings.GroupByColumnLayoutHeaderStyleResolved;
			}
		}

		#endregion // ResolveStyle

		
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