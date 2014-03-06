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
	#region RowExpandCommand

	/// <summary>
	/// A Row command that expands a <see cref="Row"/>.
	/// </summary>
	public class RowExpandCommand : RowCommandBase
	{
		#region Execute
		/// <summary>
		/// Expands the specifed <see cref="Row"/>.
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void ExecuteCommand(Row row)
		{
			row.IsExpanded = true;
		}
		#endregion // Execute
	}

	#endregion // RowExpandCommand

	#region RowCollapseCommand

	/// <summary>
	/// A Row command that collapses a <see cref="Row"/>.
	/// </summary>
	public class RowCollapseCommand : RowCommandBase
	{
		#region Execute
		/// <summary>
		/// Collapses the specifed <see cref="Row"/>.
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void ExecuteCommand(Row row)
		{
			row.IsExpanded = false;
		}
		#endregion // Execute
	}

	#endregion // RowCollapseCommand

	#region RowDeleteCommand

	/// <summary>
	/// A Row command that deletes a <see cref="Row"/>.
	/// </summary>
	public class RowDeleteCommand : RowCommandBase
	{
		#region Execute
		/// <summary>
		/// Deletes the specifed <see cref="Row"/>.
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void ExecuteCommand(Row row)
		{
			row.Manager.Rows.ActualCollection.Remove(row);
		}
		#endregion // Execute
	}

	#endregion // RowDeleteCommand

	#region RowEditCommand

	/// <summary>
	/// A Row command that puts a <see cref="Row"/> into edit mode.
	/// </summary>
	public class RowEditCommand : RowCommandBase
	{
		#region Execute
		/// <summary>
		/// Puts the specifed <see cref="Row"/> into edit mode.
		/// </summary>
		/// <param propertyName="row"></param>
		protected override void ExecuteCommand(Row row)
		{
			if(row.ColumnLayout != null && row.ColumnLayout.Grid != null)
				row.ColumnLayout.Grid.EnterEditMode(row);
		}
		#endregion // Execute
	}

	#endregion // RowEditCommand

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