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
using System.Windows.Data;

namespace Infragistics.Controls.Grids.Primitives
{

	/// <summary>
	/// Visual object for the <see cref="FooterCell"/> object.
	/// </summary>
	public class FooterCellControl : CellControlBase
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="FooterCellControl"/> class.
		/// </summary>
		public FooterCellControl()
		{
			base.DefaultStyleKey = typeof(FooterCellControl);
		}

		#endregion // Constructor

		#region Members

		DataTemplate _currentTemplate;
		FrameworkElement _contentElement;

		#endregion // Members

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

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected override void EnsureContent()
		{
			Column column = this.Cell.Column;
			if (column != null)
			{
				bool notSet = true;

				string key = null;
				if (column.Key != null)
				{
					string[] keys = column.Key.Split('.');
					key = keys[keys.Length - 1];
				}

				if (column.FooterTemplate != null)
				{
					if (this._currentTemplate != column.FooterTemplate)
					{
						this._currentTemplate = column.FooterTemplate;
						this._contentElement = column.FooterTemplate.LoadContent() as FrameworkElement;
					}

					this.Content = this._contentElement;
					this.DataContext = key;

					notSet = false;
				}
				else if (column.SupportsHeaderFooterContent)
				{
					DataTemplate template = this.Cell.Row.ColumnLayout.ColumnsFooterTemplateResolved;
					if (template != null)
					{
						if (this._currentTemplate != template)
						{
							this._contentElement = template.LoadContent() as FrameworkElement;
						}

						this.Content = this._contentElement;

						this.DataContext = key;
						notSet = false;
					}
					this._currentTemplate = template;
				}


				if (notSet || this.Content == null)
				{
					if (string.IsNullOrEmpty(column.FooterText))
						this.Content = " "; // So that the Footer has some height. 
					else
						this.Content = column.FooterText;
				}
			}
		}

		#endregion // EnsureContent

		#region OnReleased
		/// <summary>
		/// Called when the <see cref="FooterCell"/> releases the <see cref="FooterCellControl"/>.
		/// </summary>
		protected internal override void OnReleased(CellBase cell)
		{
			base.OnReleased(cell);
			this.DataContext = null;
		}
		#endregion // OnReleased

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