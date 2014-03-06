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
	#region XamGridRowCommand
	/// <summary>
	/// An enumeration of available commands for the <see cref="Row"/> object.
	/// </summary>
	public enum XamGridRowCommand
	{
		/// <summary>
		/// Deletes the Row that the command is attached to.
		/// </summary>
		Delete,

		/// <summary>
		/// Expands the Row that the command is attached to.
		/// </summary>
		Expand,

		/// <summary>
		/// Collapses the Row that the command is attached to.
		/// </summary>
		Collapse,

		/// <summary>
		/// Puts the Row that the command is attached to, into edit mode.
		/// </summary>
		Edit
	}
	#endregion //XamGridRowCommand

	#region RowCommandBase
	/// <summary>
	/// Base class for all commands that deal with a <see cref="Row"/>.
	/// </summary>
	public abstract class RowCommandBase : CommandBase
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
		/// <param propertyName="parameter">The <see cref="Row"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			Row row = parameter as Row;
			if (row != null)
			{
				this.ExecuteCommand(row);
				this.CommandSource.Handled = true;
			}
			base.Execute(parameter);
		}
		#endregion // Execute
		#endregion // Public

		#region Protected
		/// <summary>
		/// Executes the specific command on the specified <see cref="Row"/>
		/// </summary>
		/// <param propertyName="row"></param>
		protected abstract void ExecuteCommand(Row row);
		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // RowCommandBase

	#region XamGridRowCommandSource
	/// <summary>
	/// The command source object for <see cref="Row"/> object.
	/// </summary>
	public class XamGridRowCommandSource : CommandSource
	{
		#region Properties
		#region Public
		/// <summary>
		/// Gets / sets the <see cref="XamGridColumnCommand"/> which is to be executed by the command.
		/// </summary>
		public XamGridRowCommand CommandType
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
				case XamGridRowCommand.Delete:
					{
						command = new RowDeleteCommand();
						break;
					}
				case XamGridRowCommand.Edit:
					{
						command = new RowEditCommand();
						break;
					}
				case XamGridRowCommand.Expand:
					{
						command = new RowExpandCommand();
						break;
					}

				case XamGridRowCommand.Collapse:
					{
						command = new RowCollapseCommand();
						break;
					}
			}
			return command;
		}
		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // XamGridRowCommandSource
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