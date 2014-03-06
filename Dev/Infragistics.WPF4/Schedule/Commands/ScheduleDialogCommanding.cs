using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules.Primitives
{
	#region ScheduleDialogCommandBase Class
	/// <summary>
	/// Base class for commands that deal with a schedule dialog.
	/// </summary>
	public abstract class ScheduleDialogCommandBase : CommandBase
	{
		#region Overrides

		#region Public

		#region CanExecute
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
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
		/// <param name="parameter">The <see cref=" ActivityCategory"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			IScheduleDialog acd = parameter as IScheduleDialog;
			if (parameter != null)
			{
				this.ExecuteCommand(parameter as IScheduleDialog);

				if (null != this.CommandSource)
					this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute

		#endregion // Public

		#region Protected

		#region ExecuteCommand
		/// <summary>
		/// Executes the specific command on the specified dialog
		/// </summary>
		/// <param name="parameter">The parameter info for the command.</param>
		internal abstract void ExecuteCommand(IScheduleDialog parameter);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ScheduleDialogCommandBase Class

	#region ScheduleDialogCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ScheduleDialogBase&lt;TChoice&gt;"/> object.
	/// </summary>
	public class ScheduleDialogCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ScheduleDialogCommand which is to be executed by the command.
		/// </summary>
		public ScheduleDialogCommand CommandType
		{
			get;
			set;
		}

		/// <summary>
		/// Generates the <see cref="ICommand"/> object that will execute the command.
		/// </summary>
		/// <returns></returns>
		protected override ICommand ResolveCommand()
		{
			switch (this.CommandType)
			{
				case ScheduleDialogCommand.SaveAndClose:
					return new ScheduleDialogSaveAndCloseCommand();

				case ScheduleDialogCommand.Close:
					return new ScheduleDialogCloseCommand();
			}

			return null;
		}
	}

	#endregion //ScheduleDialogCommandSource Class

	#region ScheduleDialogSaveAndCloseCommand
	/// <summary>
	/// A command that saves the current changes and closes the dialog
	/// </summary>
	public class ScheduleDialogSaveAndCloseCommand : ScheduleDialogCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is IScheduleDialog)
				return ((IScheduleDialog)parameter).CanSaveAndClose;

			return true;
		}

		/// <summary>
		/// Saves the current changes and closes the dialog
		/// </summary>
		/// <param name="dialog">The dialog object that will be saved and closed.</param>
		internal override void ExecuteCommand(IScheduleDialog dialog)
		{
			dialog.SaveAndClose();
		}
	}
	#endregion // ScheduleDialogSaveAndCloseCommand

	#region ScheduleDialogCloseCommand
	/// <summary>
	/// A command that closes the dialog object without saving the changes
	/// </summary>
	public class ScheduleDialogCloseCommand : ScheduleDialogCommandBase
	{
		/// <summary>
		/// Closes the dialog object without saving the changes
		/// </summary>
		/// <param name="dialog">The dialog object that will be closed.</param>
		internal override void ExecuteCommand(IScheduleDialog dialog)
		{
			dialog.Close();
		}
	}
	#endregion // ScheduleDialogSaveAndCloseCommand
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