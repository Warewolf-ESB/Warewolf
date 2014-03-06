using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// A cell that represents a <see cref="RowSelectionCheckBoxColumn"/>column in the header of the <see cref="XamMultiColumnComboEditor"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class RowSelectionCheckBoxHeaderCell : ComboHeaderCell
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowSelectionCheckBoxHeaderCell"/> class.
		/// </summary>
		/// <param propertyName="row">The <see cref="ComboRowBase"/> object that owns the <see cref="ComboHeaderCell"/></param>
		/// <param propertyName="column">The <see cref="ComboColumn"/> object that the <see cref="ComboHeaderCell"/> represents.</param>
		public RowSelectionCheckBoxHeaderCell(ComboRowBase row, ComboColumn column)
			: base(row, column)
		{
		}

		#endregion // Constructor

		#region Properties

		#region Protected

		#region ResolveStyle

		/// <summary>
		/// Gets the Style that should be applied to the <see cref="ComboHeaderCellControl"/> when it's attached.
		/// </summary>
		protected override Style ResolveStyle
		{
			get
			{
				if (this.Style != null)
					return this.Style;
				else
					return this.Column.HeaderStyle;	// JM 9-9-11 TFS86630 - Return the column's header style
			}
		}

		#endregion // ResolveStyle

		#endregion // Protected

		#endregion // Properties

		#region Overrides

		#region CreateInstanceOfRecyclingElement

		/// <summary>
		/// Creates a new instance of the <see cref="ComboCellControl"/> that represents the object.
		/// </summary>
		/// <returns></returns>
		protected override ComboCellControlBase CreateInstanceOfRecyclingElement()
		{
			return new RowSelectionCheckBoxHeaderCellControl();
		}
		#endregion // CreateInstanceOfRecyclingElement

		#region EnsureCurrentState

		/// <summary>
		/// Ensures that <see cref="ComboHeaderCell"/> is in the correct state.
		/// </summary>
		protected internal override void EnsureCurrentState()
		{
		}

		#endregion // EnsureCurrentState

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