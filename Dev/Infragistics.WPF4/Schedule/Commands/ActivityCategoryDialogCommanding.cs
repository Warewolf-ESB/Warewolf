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
	#region ActivityCategoryDialogCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ActivityCategory"/>.
	/// </summary>
	public abstract class ActivityCategoryDialogCommandBase : CommandBase
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
			ActivityCategoryDialog acd = parameter as ActivityCategoryDialog;
			if (parameter != null)
			{
				this.ExecuteCommand(parameter as ActivityCategoryDialog);

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
		/// Executes the specific command on the specified <see cref="ActivityCategory"/>
		/// </summary>
		/// <param name="parameter">The parameter info for the command.</param>
		protected abstract void ExecuteCommand(ActivityCategoryDialog parameter);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ActivityCategoryDialogCommandBase Class

	#region ActivityCategoryDialogCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ActivityCategoryDialog"/> object.
	/// </summary>
	public class ActivityCategoryDialogCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ActivityCategoryDialogCommand which is to be executed by the command.
		/// </summary>
		public ActivityCategoryDialogCommand CommandType
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
				case ActivityCategoryDialogCommand.SaveAndClose:
					return new ActivityCategoryDialogSaveAndCloseCommand();

				case ActivityCategoryDialogCommand.Close:
					return new ActivityCategoryDialogCloseCommand();

				case ActivityCategoryDialogCommand.CreateNewCategory:
					return new ActivityCategoryDialogCreateNewCategoryCommand();

				case ActivityCategoryDialogCommand.EditSelectedCategory:
					return new ActivityCategoryDialogEditSelectedCategoryCommand();

				case ActivityCategoryDialogCommand.DeleteSelectedCategory:
					return new ActivityCategoryDialogDeleteSelectedCategoryCommand();
			}

			return null;
		}
	}

	#endregion //ActivityCategoryDialogCommandSource Class

	#region ActivityCategoryDialogSaveAndCloseCommand
	/// <summary>
	/// A command that saves changes to the <see cref="ActivityCategory"/>s and closes the <see cref="ActivityCategoryDialog"/> object that is hosting it. 
	/// </summary>
	public class ActivityCategoryDialogSaveAndCloseCommand : ActivityCategoryDialogCommandBase
	{
		/// <summary>
		/// Saves changes to the <see cref="ActivityCategory"/>s and closes the <see cref="ActivityCategoryDialog"/> object that is hosting it. 
		/// </summary>
		/// <param name="activityCategoryDialog">The <see cref="ActivityCategoryDialog"/> object that will be saved and closed.</param>
		protected override void ExecuteCommand(ActivityCategoryDialog activityCategoryDialog)
		{
			activityCategoryDialog.SaveAndClose();
		}
	}
	#endregion // ActivityCategoryDialogSaveAndCloseCommand

	#region ActivityCategoryDialogCloseCommand
	/// <summary>
	/// A command that Closes the <see cref="ActivityCategoryDialog"/> and cancels all changes. 
	/// </summary>
	public class ActivityCategoryDialogCloseCommand : ActivityCategoryDialogCommandBase
	{
		/// <summary>
		/// Closes the <see cref="ActivityCategoryDialog"/> and cancels all changes. 
		/// </summary>
		/// <param name="activityCategoryDialog">The <see cref="ActivityCategoryDialog"/> object that will be closed.</param>
		protected override void ExecuteCommand(ActivityCategoryDialog activityCategoryDialog)
		{
			activityCategoryDialog.Close();
		}
	}
	#endregion // ActivityCategoryDialogSaveAndCloseCommand

	#region ActivityCategoryDialogCreateNewCategoryCommand
	/// <summary>
	/// A command that triggers a UI for creating a new <see cref="ActivityCategory"/>. 
	/// </summary>
	public class ActivityCategoryDialogCreateNewCategoryCommand : ActivityCategoryDialogCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityCategoryDialog)
				// JM 02-25-11 TFS67015 Also check whether custom activity categories are allowed.
				return ((ActivityCategoryDialog)parameter).IsOwningResourceModifiable &&
					   ((ActivityCategoryDialog)parameter).ActivityCategoryHelper.AreCustomActivityCategoriesAllowed;

			return true;
		}

		/// <summary>
		/// Triggers a UI for creating a new <see cref="ActivityCategory"/>. 
		/// </summary>
		/// <param name="activityCategoryDialog">The <see cref="ActivityCategoryDialog"/> object that will be saved closed.</param>
		protected override void ExecuteCommand(ActivityCategoryDialog activityCategoryDialog)
		{
			activityCategoryDialog.CreateNewCategory();
		}
	}
	#endregion // ActivityCategoryDialogCreateNewCategoryCommand

	#region ActivityCategoryDialogEditSelectedCategoryCommand
	/// <summary>
	/// A command that triggers a UI for editing the currently selected <see cref="ActivityCategory"/>. 
	/// </summary>
	public class ActivityCategoryDialogEditSelectedCategoryCommand : ActivityCategoryDialogCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityCategoryDialog)
			{
				if (false == ((ActivityCategoryDialog)parameter).IsOwningResourceModifiable)
					return false;

				ActivityCategoryListItem selectedListItem = ((ActivityCategoryDialog)parameter).SelectedActivityCategoryListItem;
				if (null != selectedListItem)
					return selectedListItem.IsCustomizable;
			}

			// JM 04-07-11 TFS71671
			//return true;
			return false;
		}

		/// <summary>
		/// Triggers a UI for editing the currently selected <see cref="ActivityCategory"/>. 
		/// </summary>
		/// <param name="activityCategoryDialog">The <see cref="ActivityCategoryDialog"/> object that will be saved closed.</param>
		protected override void ExecuteCommand(ActivityCategoryDialog activityCategoryDialog)
		{
			activityCategoryDialog.EditSelectedCategory();
		}
	}
	#endregion // ActivityCategoryDialogEditSelectedCategoryCommand

	#region ActivityCategoryDialogDeleteSelectedCategoryCommand
	/// <summary>
	/// A command that deletes the currently selected <see cref="ActivityCategory"/>. 
	/// </summary>
	public class ActivityCategoryDialogDeleteSelectedCategoryCommand : ActivityCategoryDialogCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ActivityCategoryDialog)
			{
				if (false == ((ActivityCategoryDialog)parameter).IsOwningResourceModifiable)
					return false;

				ActivityCategoryListItem selectedListItem = ((ActivityCategoryDialog)parameter).SelectedActivityCategoryListItem;
				if (null != selectedListItem)
					return selectedListItem.IsCustomizable;
			}

			return false;
		}

		/// <summary>
		/// Deletes the currently selected <see cref="ActivityCategory"/>. 
		/// </summary>
		/// <param name="activityCategoryDialog">The <see cref="ActivityCategoryDialog"/> object that will be saved closed.</param>
		protected override void ExecuteCommand(ActivityCategoryDialog activityCategoryDialog)
		{
			activityCategoryDialog.DeleteSelectedCategory();
		}
	}
	#endregion // ActivityCategoryDialogDeleteSelectedCategoryCommand
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