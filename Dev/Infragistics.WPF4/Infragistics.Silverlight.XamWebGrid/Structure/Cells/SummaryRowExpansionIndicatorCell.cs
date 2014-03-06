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
	/// An object that represents the particular expansion indicator area for a row.
	/// </summary>
	public class SummaryRowExpansionIndicatorCell : ExpansionIndicatorCell
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="SummaryRowExpansionIndicatorCell"/> class.
		/// </summary>
		/// <param propertyName="row">The row that owns this particular cell.</param>
		/// <param propertyName="column">The <see cref="ExpansionIndicatorColumn"/> that this cell represents.</param>
		protected internal SummaryRowExpansionIndicatorCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="SummaryRowExpansionIndicatorCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				return this.Row.ColumnLayout.SummaryRowSettings .ExpansionIndicatorStyleResolved;
			}
		}

		#endregion // ResolveStyle

		#region RecyclingElementType
		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="SummaryRowExpansionIndicatorCell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return typeof(SummaryRowExpansionIndicatorCellControl);
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="SummaryRowExpansionIndicatorCellControl"/> for the <see cref="ExpansionIndicatorCell"/>.
		/// </summary>
		/// <returns>A new <see cref="SummaryRowExpansionIndicatorCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new SummaryRowExpansionIndicatorCellControl();
		}

		#endregion // CreateInstanceOfRecyclingElement

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