using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules.Primitives
{
	#region ReminderDialogCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ReminderDialog"/>.
	/// </summary>
	public abstract class ReminderDialogCommandBase : CommandBase
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
			ReminderDialog reminderDialog = parameter as ReminderDialog;
			if (reminderDialog != null)
				return reminderDialog.CanExecuteCommand(this.CommandType);

			return false;
		}
		#endregion

		#region Execute
		/// <summary>
		/// Executes the command 
		/// </summary>
		/// <param name="parameter">The <see cref=" ReminderDialog"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			ReminderDialog reminderDialog = parameter as ReminderDialog;
			if (reminderDialog != null)
			{
				reminderDialog.ExecuteCommand(this.CommandType);

				if (null != this.CommandSource)
					this.CommandSource.Handled = true;
			}

			base.Execute(parameter);
		}
		#endregion // Execute

		#endregion // Public

		#region Protected

		#region CommandType
		/// <summary>
		/// Returns the type of command.
		/// </summary>
		protected abstract ReminderDialogCommand CommandType { get; }
		#endregion CommandType

		#endregion Protected

		#endregion // Overrides
	}
	#endregion // ReminderDialogCommandBase Class

	#region ReminderDialogCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ReminderDialog"/> object.
	/// </summary>
	public class ReminderDialogCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ReminderDialogCommand which is to be executed by the command.
		/// </summary>
		public ReminderDialogCommand CommandType
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
				case ReminderDialogCommand.DismissAll:
					return new ReminderDialogDismissAllCommand();

				case ReminderDialogCommand.DismissSelected:
					return new ReminderDialogDismissSelectedCommand();

				case ReminderDialogCommand.OpenSelected:
					return new ReminderDialogOpenSelectedCommand();

				case ReminderDialogCommand.SnoozeSelected:
					return new ReminderDialogSnoozeSelectedCommand();
			}
			
			return null;
		}
	}

	#endregion //ReminderDialogCommandSource Class

	#region ReminderDialog Commands

	#region ReminderDialogDismissAllCommand
	/// <summary>
	/// A command that dismisses all the reminders in the <see cref="ReminderDialog"/>. 
	/// </summary>
	public class ReminderDialogDismissAllCommand : ReminderDialogCommandBase
	{
		/// <summary>
		/// Returns a <see cref="ReminderDialogCommand"/> enumeration that specified which <see cref="ReminderDialogCommandBase"/> command this is.
		/// </summary>
		protected override ReminderDialogCommand CommandType { get { return ReminderDialogCommand.DismissAll; } }
	}
	#endregion // ReminderDialogDismissAllCommand

	#region ReminderDialogOpenSelectedCommand
	/// <summary>
	/// A command that opens the <see cref="Appointment"/> dialog for all currently selected reminder(s) that represent a reminder for an <see cref="Appointment"/> activity type. 
	/// </summary>
	public class ReminderDialogOpenSelectedCommand : ReminderDialogCommandBase
	{
		/// <summary>
		/// Returns a <see cref="ReminderDialogCommand"/> enumeration that specified which <see cref="ReminderDialogCommandBase"/> command this is.
		/// </summary>
		protected override ReminderDialogCommand CommandType { get { return ReminderDialogCommand.OpenSelected; } }
	}
	#endregion //ReminderDialogOpenSelectedCommand

	#region ReminderDialogDismissSelectedCommand
	/// <summary>
	/// A command that dismisses the currently selected reminder(s) in the <see cref="ReminderDialog"/>. 
	/// </summary>
	public class ReminderDialogDismissSelectedCommand : ReminderDialogCommandBase
	{
		/// <summary>
		/// Returns a <see cref="ReminderDialogCommand"/> enumeration that specified which <see cref="ReminderDialogCommandBase"/> command this is.
		/// </summary>
		protected override ReminderDialogCommand CommandType { get { return ReminderDialogCommand.DismissSelected; } }
	}
	#endregion //ReminderDialogDismissSelectedCommand

	#region ReminderDialogSnoozeSelectedCommand
	/// <summary>
	/// A command that snoozes the currently selected reminder(s) in the <see cref="ReminderDialog"/>. 
	/// </summary>
	public class ReminderDialogSnoozeSelectedCommand : ReminderDialogCommandBase
	{
		/// <summary>
		/// Returns a <see cref="ReminderDialogCommand"/> enumeration that specified which <see cref="ReminderDialogCommandBase"/> command this is.
		/// </summary>
		protected override ReminderDialogCommand CommandType { get { return ReminderDialogCommand.SnoozeSelected; } }
	}
	#endregion //ReminderDialogSnoozeSelectedCommand

	#endregion // ReminderDialog Commands
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