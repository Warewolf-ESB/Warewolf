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

namespace Infragistics.Controls.Schedules.Primitives
{
	#region ActivityDialogCoreCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ActivityDialogCore"/>.
	/// </summary>
	public abstract class ActivityDialogCoreCommandBase : CommandBase
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
		/// <param name="parameter">The <see cref=" ActivityDialogCore"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			ActivityDialogCore adc = parameter as ActivityDialogCore;
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
		/// Executes the specific command on the specified <see cref="ActivityDialogCore"/>
		/// </summary>
		/// <param name="activityDialogCore">The window for which the command will be executed.</param>
		protected abstract void ExecuteCommand(ActivityDialogCore activityDialogCore);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ActivityDialogCoreCommandBase Class

	#region ActivityDialogCoreCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ActivityDialogCoreCommand which is to be executed by the command.
		/// </summary>
		public ActivityDialogCoreCommand CommandType
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
				case ActivityDialogCoreCommand.SaveAndClose:
					return new ActivityDialogCoreSaveAndCloseCommand();

				case ActivityDialogCoreCommand.Close:
					return new ActivityDialogCoreCloseCommand();

				case ActivityDialogCoreCommand.Delete:
					return new ActivityDialogCoreDeleteCommand();

				case ActivityDialogCoreCommand.DisplayRecurrenceDialog:
					return new ActivityDialogCoreDisplayRecurrenceDialogCommand();

				case ActivityDialogCoreCommand.ShowTimeZonePickers:
					return new ActivityDialogCoreShowTimeZonePickersCommand();

				case ActivityDialogCoreCommand.HideTimeZonePickers:
					return new ActivityDialogCoreHideTimeZonePickersCommand();
			}
			
			return null;
		}
	}

	#endregion //ActivityDialogCoreCommandSource Class

	#region ActivityDialogCore Commands

	#region ActivityDialogCoreSaveAndCloseCommand
	/// <summary>
	/// A command that saves the <see cref="Appointment"/> and closes the <see cref="ActivityDialogCore"/> object that is hosting it. 
	/// </summary>
	public class ActivityDialogCoreSaveAndCloseCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityDialogCore)
				return ((ActivityDialogCore)parameter).IsDirty &&
					   ((ActivityDialogCore)parameter).IsActivityModifiable;

			return true;
		}

		/// <summary>
		/// Saves the <see cref="Appointment"/> and closes the <see cref="ActivityDialogCore"/> object that is hosting it. 
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object that will be saved closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.SaveAndClose();
		}
	}
	#endregion // ActivityDialogCoreSaveAndCloseCommand

	#region ActivityDialogCoreCloseCommand
	/// <summary>
	/// A command that closes the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreCloseCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Closes the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object that will be closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.Close();
		}
	}
	#endregion // ActivityDialogCoreCloseCommand

	#region ActivityDialogCoreDeleteCommand
	/// <summary>
	/// A command that deletes the <see cref="Appointment"/> current being edited in the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreDeleteCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityDialogCore)
				return ((ActivityDialogCore)parameter).IsActivityRemoveable  &&
					   ((ActivityDialogCore)parameter).IsActivityModifiable;

			return true;
		}

		/// <summary>
		/// Deletes the <see cref="Appointment"/> current being edited in the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object which is editing the <see cref="Appointment"/> that will be closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.Delete();
		}
	}
	#endregion // ActivityDialogCoreDeleteCommand

	#region ActivityDialogCoreDisplayRecurrenceDialogCommand
	/// <summary>
	/// A command that deletes the <see cref="Appointment"/> current being edited in the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreDisplayRecurrenceDialogCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			return true;
		}

		/// <summary>
		/// Deletes the <see cref="Appointment"/> current being edited in the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object which is editing the <see cref="Appointment"/> that will be closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.DisplayRecurrenceDialog();
		}
	}
	#endregion // ActivityDialogCoreDisplayRecurrenceDialogCommand

	#region ActivityDialogCoreShowTimeZonePickersCommand
	/// <summary>
	/// A command that shows the time zone pickers in the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreShowTimeZonePickersCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			ActivityDialogCore adc = parameter as ActivityDialogCore;
			if (adc != null)
			{
				return adc.CanChangeTimeZonePickerVisibility;
			}

			return false;
		}

		/// <summary>
		/// Shows the time zone pickers in the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object which is editing the <see cref="Appointment"/> that will be closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.TimeZonePickerVisibility = Visibility.Visible;
		}
	}
	#endregion //ActivityDialogCoreShowTimeZonePickersCommand

	#region ActivityDialogCoreHideTimeZonePickersCommand
	/// <summary>
	/// A command that shows the time zone pickers in the <see cref="ActivityDialogCore"/> object.
	/// </summary>
	public class ActivityDialogCoreHideTimeZonePickersCommand : ActivityDialogCoreCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			ActivityDialogCore adc = parameter as ActivityDialogCore;
			if (adc != null)
			{
				return adc.CanChangeTimeZonePickerVisibility;
			}

			return false;
		}

		/// <summary>
		/// Hides the time zone pickers in the <see cref="ActivityDialogCore"/> object.
		/// </summary>
		/// <param name="activityDialogCore">The <see cref="ActivityDialogCore"/> object which is editing the <see cref="Appointment"/> that will be closed.</param>
		protected override void ExecuteCommand(ActivityDialogCore activityDialogCore)
		{
			activityDialogCore.TimeZonePickerVisibility = Visibility.Collapsed;
		}
	}
	#endregion //ActivityDialogCoreHideTimeZonePickersCommand

	#endregion //ActivityDialogCore Commands
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