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
using System.Collections.ObjectModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A cell that represents the ExpansionIndicator of a <see cref="FooterRow"/>.
	/// </summary>
	public class ExpansionIndicatorFooterCell : CellBase
	{
		#region Static

		static Type _recyclingElementType = typeof(ExpansionIndicatorFooterCellControl);

		#endregion // Static

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpansionIndicatorFooterCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="FooterRow"/> object that owns the <see cref="ExpansionIndicatorFooterCell"/></param>
		/// <param propertyName="column">The <see cref="ExpansionIndicatorColumn"/> object that the <see cref="ExpansionIndicatorFooterCell"/> represents.</param>
		protected internal ExpansionIndicatorFooterCell(RowBase row, Column column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Overrides

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="ExpansionIndicatorCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;

				return this.Row.ColumnLayout.ExpansionIndicatorSettings.FooterStyleResolved;
			}
		}

		#endregion // ResolveStyle

		#region RecyclingElementType
		/// <summary>
		/// Gets the Type of control that should be created for the <see cref="ExpansionIndicatorFooterCell"/>.
		/// </summary>
		protected override Type RecyclingElementType
		{
			get
			{
				return ExpansionIndicatorFooterCell._recyclingElementType;
			}
		}
		#endregion // RecyclingElementType

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of a <see cref="ExpansionIndicatorFooterCellControl"/> for the <see cref="ExpansionIndicatorFooterCell"/>.
		/// </summary>
		/// <returns>A new <see cref="ExpansionIndicatorFooterCellControl"/></returns>
		/// <remarks>This method should only be used by the <see cref="Infragistics.RecyclingManager"/></remarks>
		protected override CellControlBase CreateInstanceOfRecyclingElement()
		{
			return new ExpansionIndicatorFooterCellControl();
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