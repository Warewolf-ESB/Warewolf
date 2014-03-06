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
	#region SortingBaseCommand
	/// <summary>
	/// Base class for Sorting commands which encapsulates shared logic.
	/// </summary>
	public abstract class SortingBaseCommand : ColumnCommandBase
	{
		#region Methods

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param propertyName="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			Column column = parameter as Column;
			return column != null && column.IsSortable && column.ColumnLayout.SortingSettings.AllowSortingResolved;
		}
		#endregion

		#endregion // Public

		#region Protected

		#region ExecuteCommandHelper

		/// <summary>
		///  Method designed to combine similar code into a single path.
		/// </summary>
		/// <param name="sortDirection"></param>
		/// <param name="col"></param>
		protected static void ExecuteCommandHelper(SortDirection sortDirection, Column col)
		{
			if (col.ColumnLayout.SortingSettings.MultiSortingKeyResolved == MultiSortingKey.Control)
			{
				col.SetSortedColumnState(sortDirection, (System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control);
			}
			else
			{
				col.SetSortedColumnState(sortDirection, (System.Windows.Input.Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
			}
		}

		#endregion // ExecuteCommandHelper

		#endregion // Protected

		#endregion // Methods
	}
	#endregion // SortingBaseCommand

	#region SortAscendingCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will sort in an ascending manner.
	/// </summary>
	public class SortAscendingCommand : SortingBaseCommand
	{
		/// <summary>
		/// Applies a sort to the column.
		/// </summary>
		/// <param propertyName="col">A <see cref="Column"/> object that will be sorted.</param>
		protected override void ExecuteCommand(Column col)
		{
			SortingBaseCommand.ExecuteCommandHelper(SortDirection.Ascending, col);
		}
	}
	#endregion // SortAscendingCommand

	#region SortDescendingCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will sort in an descending manner.
	/// </summary>
	public class SortDescendingCommand : SortingBaseCommand
	{
		/// <summary>
		/// Applies a sort to the column.
		/// </summary>
		/// <param propertyName="col">A <see cref="Column"/> object that will be sorted.</param>
		protected override void ExecuteCommand(Column col)
		{
			SortingBaseCommand.ExecuteCommandHelper(SortDirection.Descending, col);
		}
	}
	#endregion // SortDescendingCommand

	#region SortToggleCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will change the sort order.
	/// </summary>
	/// <remarks>
	/// The sort order will go from unsorted to ascending, from ascending to descending, and from descending to ascending.
	/// </remarks>
	public class SortToggleCommand : SortingBaseCommand
	{
		/// <summary>
		/// Applies a sort to the column.
		/// </summary>
		/// <param propertyName="col">A <see cref="Column"/> object that will be sorted.</param>
		protected override void ExecuteCommand(Column col)
		{
			SortingBaseCommand.ExecuteCommandHelper(col.NextSortDirection, col);
		}
	}
	#endregion // SortToggleCommand

	#region UnsortCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will unsort.
	/// </summary>
	public class UnsortCommand : SortingBaseCommand
	{
		/// <summary>
		/// Removes a sort from the column.
		/// </summary>
		/// <param propertyName="col">A <see cref="Column"/> object that will be unsorted.</param>
		protected override void ExecuteCommand(Column col)
		{
			SortingBaseCommand.ExecuteCommandHelper(SortDirection.None, col);
		}
	}
	#endregion // SortAscendingCommand

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