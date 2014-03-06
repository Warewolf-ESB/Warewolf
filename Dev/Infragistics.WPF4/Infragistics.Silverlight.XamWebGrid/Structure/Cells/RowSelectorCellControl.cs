using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="RowSelectorCell"/> object.
	/// </summary>
	[TemplateVisualState(GroupName = "ActiveStates", Name = "Active")]
	[TemplateVisualState(GroupName = "ActiveStates", Name = "InActive")]
	[TemplateVisualState(GroupName = "ActiveStates", Name = "Editing")]
	public class RowSelectorCellControl : CellControlBase
	{
		#region Members

		TextBlock _tb;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="RowSelectorCellControl"/> class.
		/// </summary>
		public RowSelectorCellControl()
		{
			base.DefaultStyleKey = typeof(RowSelectorCellControl);
			this._tb = new TextBlock();
			this.Content = this._tb;
		}

		#endregion // Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			this.EnsureContent();
		}
		#endregion // AttachContent

		#region ReleaseContent

		/// <summary>
		/// Invoked before content is released from the control.
		/// </summary>
		protected override void ReleaseContent()
		{
			
		}
		#endregion // ReleaseContent

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected override void EnsureContent()
		{
			RowBase row = this.Cell.Row;

			string text = null;

			if (row != null && row.ColumnLayout.RowSelectorSettings.EnableRowNumberingResolved)
				text = (row.ColumnLayout.RowSelectorSettings.RowNumberingSeedResolved + row.Index).ToString(CultureInfo.CurrentCulture);

			if (this._tb.Text != text)
				this._tb.Text = text;
		}

		#endregion // EnsureContent

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