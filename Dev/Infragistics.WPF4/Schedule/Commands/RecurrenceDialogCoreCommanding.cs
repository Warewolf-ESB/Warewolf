using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infragistics.Controls.Schedules;
using System.Windows.Input;

namespace Infragistics.Controls.Schedules.Primitives
{
	#region RecurrenceDialogCoreCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ActivityRecurrenceDialogCore"/>.
	/// </summary>
	public abstract class RecurrenceDialogCoreCommandBase : CommandBase
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
		/// <param name="parameter">The <see cref=" ActivityRecurrenceDialogCore"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			ActivityRecurrenceDialogCore adc = parameter as ActivityRecurrenceDialogCore;
			if (adc != null)
			{
				this.ExecuteCommand(adc);

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
		/// Executes the specific command on the specified <see cref="ActivityRecurrenceDialogCore"/>
		/// </summary>
		/// <param name="appointmentDialogCore">The window for which the command will be executed.</param>
		protected abstract void ExecuteCommand(ActivityRecurrenceDialogCore appointmentDialogCore);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // RecurrenceDialogCoreCommandBase Class

	#region RecurrenceDialogCoreCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ActivityRecurrenceDialogCore"/> object.
	/// </summary>
	public class RecurrenceDialogCoreCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the RecurrenceDialogCoreCommand which is to be executed by the command.
		/// </summary>
		public RecurrenceDialogCoreCommand CommandType
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
				case RecurrenceDialogCoreCommand.SaveAndClose:
					return new RecurrenceDialogCoreSaveAndCloseCommand();

				case RecurrenceDialogCoreCommand.Close:
					return new RecurrenceDialogCoreCloseCommand();

				case RecurrenceDialogCoreCommand.RemoveRecurrence:
					return new RecurrenceDialogCoreRemoveRecurrenceCommand();
			}
			
			return null;
		}
	}

	#endregion //RecurrenceDialogCoreCommandSource Class

	#region RecurrenceDialogCore Commands

	#region RecurrenceDialogCoreSaveAndCloseCommand
	/// <summary>
	/// A command that saves the changes (if any) to the Recurrence information and closes the <see cref="ActivityRecurrenceDialogCore"/>. 
	/// </summary>
	public class RecurrenceDialogCoreSaveAndCloseCommand : RecurrenceDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityRecurrenceDialogCore)
				return ((ActivityRecurrenceDialogCore)parameter).IsActivityModifiable;

			return true;
		}

		/// <summary>
		/// Saves the changes (if any) to the Recurrence information and closes the <see cref="ActivityRecurrenceDialogCore"/>. 
		/// </summary>
		/// <param name="recurrenceDialogCore">The <see cref="ActivityRecurrenceDialogCore"/> object that will be saved closed.</param>
		protected override void ExecuteCommand(ActivityRecurrenceDialogCore recurrenceDialogCore)
		{
			recurrenceDialogCore.SaveAndClose();
		}
	}
	#endregion // RecurrenceDialogCoreSaveAndCloseCommand

	#region RecurrenceDialogCoreCloseCommand
	/// <summary>
	/// A command that closes the <see cref="ActivityRecurrenceDialogCore"/> object.
	/// </summary>
	public class RecurrenceDialogCoreCloseCommand : RecurrenceDialogCoreCommandBase
	{
		/// <summary>
		/// Closes the <see cref="ActivityRecurrenceDialogCore"/> object.
		/// </summary>
		/// <param name="recurrenceDialogCore">The <see cref="ActivityRecurrenceDialogCore"/> object that will be closed.</param>
		protected override void ExecuteCommand(ActivityRecurrenceDialogCore recurrenceDialogCore)
		{
			recurrenceDialogCore.Close();
		}
	}
	#endregion // RecurrenceDialogCoreCloseCommand

	#region RecurrenceDialogCoreRemoveRecurrenceCommand
	/// <summary>
	/// A command that removes the recurrence definition from the <see cref="ActivityBase"/> being edited and closes the <see cref="ActivityRecurrenceDialogCore"/>.
	/// </summary>
	public class RecurrenceDialogCoreRemoveRecurrenceCommand : RecurrenceDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityRecurrenceDialogCore)
				return ((ActivityRecurrenceDialogCore)parameter).IsRecurrenceRemoveable &&
					   ((ActivityRecurrenceDialogCore)parameter).IsActivityModifiable;

			return true;
		}

		/// <summary>
		/// Removes the recurrence definition from the <see cref="ActivityBase"/> being edited and closes the <see cref="ActivityRecurrenceDialogCore"/>.
		/// </summary>
		/// <param name="recurrenceDialogCore">The <see cref="ActivityRecurrenceDialogCore"/> object that will be closed.</param>
		protected override void ExecuteCommand(ActivityRecurrenceDialogCore recurrenceDialogCore)
		{
			recurrenceDialogCore.RemoveRecurrence();
		}
	}
	#endregion // RecurrenceDialogCoreRemoveRecurrenceCommand

	#endregion // RecurrenceDialogCore Commands
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