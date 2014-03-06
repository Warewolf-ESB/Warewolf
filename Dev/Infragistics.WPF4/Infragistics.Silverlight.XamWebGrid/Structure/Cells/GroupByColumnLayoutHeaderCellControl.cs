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
	/// Visual object for the <see cref="GroupByColumnLayoutHeaderCell"/> object.
	/// </summary>
	public class GroupByColumnLayoutHeaderCellControl : GroupByHeaderCellControl
	{
		#region Constructor
		/// <summary>
		/// Creates a new instance of the <see cref="GroupByColumnLayoutHeaderCellControl"/> object.
		/// </summary>
		public GroupByColumnLayoutHeaderCellControl()
		{
			base.DefaultStyleKey = typeof(GroupByColumnLayoutHeaderCellControl);
		}
		#endregion // Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			bool notSet = true;

			ColumnLayout column = this.Cell.Row.ColumnLayout;

			if (column.HeaderTemplate != null)
			{
				this.Content = column.HeaderTemplate.LoadContent();
				this.DataContext = column.Key;
				notSet = false;
			}

			if (notSet || this.Content == null)
			{
				if (string.IsNullOrEmpty(column.HeaderText))
					this.Content = column.Key;
				else
					this.Content = column.HeaderText;
			}

		}

		#endregion // AttachContent

        #region OnMouseLeftButtonDown_ColumnMoving

        /// <summary>
        /// Called before the <see cref="UIElement.MouseLeftButtonDown"/> event occurs.
        /// </summary>
        /// <param propertyName="e">The data for the event</param>
        protected override bool OnMouseLeftButtonDownColumnMoving(MouseButtonEventArgs e)
        {
            return false;
        }

        #endregion // OnMouseLeftButtonDown_ColumnMoving

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