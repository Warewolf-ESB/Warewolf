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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	#region XamGridColumnCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="Column"/> object.
	/// </summary>
	public enum XamGridColumnCommand
	{
		/// <summary>
		/// Sorts the <see cref="Column"/> in an ascending manner.
		/// </summary>
		SortAscending,

		/// <summary>
		/// Changes the <see cref="SortDirection"/> on the <see cref="Column"/> from nothing to ascending, ascending to descending, and descending to ascending.
		/// </summary>
		SortToggle,

		/// <summary>
		/// Sorts the <see cref="Column"/> in an descending manner.
		/// </summary>
		SortDescending,

		/// <summary>
		/// Unsorts the <see cref="Column"/>.
		/// </summary>
		Unsort,

		/// <summary>
		/// Fixes a <see cref="Column"/> to either the left or right based on the <see cref="Column.FixedIndicatorDirection"/> property.
		/// </summary>
		Fix,

		/// <summary>
		/// Fixes a <see cref="Column"/> to the left side of the <see cref="XamGrid"/>
		/// </summary>
		FixLeft,

		/// <summary>
		/// Fixes a <see cref="Column"/> to the right side of the <see cref="XamGrid"/>
		/// </summary>
		FixRight,

		/// <summary>
		/// Unfixes a <see cref="Column"/>.
		/// </summary>
		Unfix,

		/// <summary>
		/// Selects the <see cref="Column"/>
		/// </summary>
		Select,

		/// <summary>
		/// Unselects the <see cref="Column"/>
		/// </summary>
		Unselect,

		/// <summary>
		/// Groups the data in the <see cref="XamGrid"/> by this <see cref="Column"/>
		/// </summary>
		GroupBy,

		/// <summary>
		/// Removes the grouping of data in the <see cref="XamGrid"/>.
		/// </summary>
		RemoveGroupBy,

        /// <summary>
        /// Hides the <see cref="Column"/>
        /// </summary>
        Hide,

        /// <summary>
        /// Shows the <see cref="Column"/>
        /// </summary>
        Show
	}
	#endregion //XamGridColumnCommand

	#region ColumnCommandBase
	/// <summary>
	/// Base class for all commands that deal with a <see cref="Column"/>.
	/// </summary>
	public abstract class ColumnCommandBase : CommandBase
	{
		#region Overrides

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param propertyName="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			return true;
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param propertyName="parameter">The <see cref="CellBase"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			Column col = parameter as Column;
			if (col != null)
			{
				this.ExecuteCommand(col);
				this.CommandSource.Handled = true;
			}
			base.Execute(parameter);
		}
		#endregion // Execute
		#endregion // Public

		#region Protected
		/// <summary>
		/// Executes the specific command on the specified <see cref="Column"/>
		/// </summary>
		/// <param propertyName="col"></param>
		protected abstract void ExecuteCommand(Column col);
		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ColumnCommandBase

	#region XamGridColumnCommandSource
	/// <summary>
	/// The command source object for <see cref="Column"/> object.
	/// </summary>
	public class XamGridColumnCommandSource : CommandSource
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets / sets the <see cref="XamGridColumnCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGridColumnCommand CommandType
		{
			get;
			set;
		}
		#endregion // Public
		#endregion // Properties

		#region Overrides

		#region Protected
		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			ICommand command = null;

			switch (this.CommandType)
			{
				case XamGridColumnCommand.SortToggle:
					{
						command = new SortToggleCommand();
						break;
					}
				case XamGridColumnCommand.SortAscending:
					{
						command = new SortAscendingCommand();
						break;
					}
				case XamGridColumnCommand.SortDescending:
					{
						command = new SortDescendingCommand();
						break;
					}
				case XamGridColumnCommand.Unsort:
					{
						command = new UnsortCommand();
						break;
					}
				case XamGridColumnCommand.Fix:
					{
						command = new FixColumnCommand();
						break;
					}
				case XamGridColumnCommand.FixLeft:
					{
						command = new FixColumnLeftCommand();
						break;
					}
				case XamGridColumnCommand.FixRight:
					{
						command = new FixColumnRightCommand();
						break;
					}
				case XamGridColumnCommand.Unfix:
					{
						command = new UnfixColumnCommand();
						break;
					}
				case XamGridColumnCommand.Select:
					{
						command = new SelectColumnCommand();
						break;
					}
				case XamGridColumnCommand.Unselect:
					{
						command = new UnselectColumnCommand();
						break;
					}
				case XamGridColumnCommand.GroupBy:
					{
						command = new GroupByColumnCommand();
						break;
					}
				case XamGridColumnCommand.RemoveGroupBy:
					{
						command = new RemoveGroupByColumnCommand();
						break;
					}
                case XamGridColumnCommand.Hide:
                    {
                        command = new HideColumnCommand();
                        break;
                    }

                case XamGridColumnCommand.Show:
                    {
                        command = new ShowColumnCommand();
                        break;
                    }
			}
			return command;
		}
		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // XamGridColumnCommandSource
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