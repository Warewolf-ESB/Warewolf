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
	/// The Cell object for the Header of a <see cref="ColumnLayout"/>.
	/// </summary>
	public class ChildBandCell : Cell
	{
		#region Static

		static Type _recyclingElementType = typeof(ChildBandCellControl);

		#endregion // Static

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ChildBandCell"/> class.
		/// </summary>
		protected internal ChildBandCell(RowBase row, Column column) : base(row, column) { }

		#endregion // Constructor

		#region Overrides

		#region Properties

		#region RecyclingElementType

		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="Cell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return ChildBandCell._recyclingElementType;
			}
		}
		#endregion // RecyclingElementType

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="ChildBandCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style == null)
					return this.Row.ColumnLayout.ChildBandHeaderStyleResolved;
				else
					return this.Style;
			}
		}

		#endregion // ResolveStyle

		#endregion // Properties

		#region Methods

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of <see cref="ChildBandCellControl"/>. 
		/// Note: this method is only meant for use by the Recycling framework. 
		/// </summary>
		/// <returns>A new instance of ChildLayoutCellCellControl.</returns>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new ChildBandCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

		#endregion // Methods

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