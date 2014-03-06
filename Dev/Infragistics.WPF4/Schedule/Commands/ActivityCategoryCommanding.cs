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
	#region ActivityCategoryCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ActivityCategory"/>.
	/// </summary>
	public abstract class ActivityCategoryCommandBase : CommandBase
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
			ActivityCategoryCommandParameterInfo parameterInfo = parameter as ActivityCategoryCommandParameterInfo;
			if (parameterInfo != null)
			{
				this.ExecuteCommand(parameterInfo);

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
		/// <param name="parameterInfo">The parameter info for the command.</param>
		protected abstract void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ActivityCategoryCommandBase Class

	#region ActivityCategoryCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ActivityCategory"/> object.
	/// </summary>
	public class ActivityCategoryCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ActivityCategoryCommand which is to be executed by the command.
		/// </summary>
		public ActivityCategoryCommand CommandType
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
				case ActivityCategoryCommand.ClearAllActivityCategories:
					return new ActivityCategoryClearAllActivityCategoriesCommand();

				case ActivityCategoryCommand.DisplayActivityCategoriesDialog:
					return new ActivityCategoryDisplayActivityCategoriesDialogCommand();

				case ActivityCategoryCommand.ToggleActivityCategorySelectedState:
					return new ActivityCategoryToggleActivityCategorySelectedStateCommand();

				case ActivityCategoryCommand.ClearActivityCategorySelectedState:
					return new ActivityCategoryClearActivityCategorySelectedStateCommand();

				case ActivityCategoryCommand.SetActivityCategorySelectedState:
					return new ActivityCategorySetActivityCategorySelectedStateCommand();
			}

			return null;
		}
	}

	#endregion //ActivityCategoryCommandSource Class

	#region ActivityCategoryClearAllActivityCategoriesCommand
	/// <summary>
	/// A command that clears all Activity Categories assigned to the Activity.
	/// </summary>
	public class ActivityCategoryClearAllActivityCategoriesCommand : ActivityCategoryCommandBase
	{
		/// <summary>
		/// Returns true if the command can be executed on the object.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			ActivityCategoryCommandParameterInfo parameterInfo = parameter as ActivityCategoryCommandParameterInfo;
			if (null != parameterInfo &&
				null != parameterInfo.ActivityCategoryHelper)
			{
				return parameterInfo.ActivityCategoryHelper.SelectedCategoryListItems.Count > 0;
			}

			return true;
		}

		/// <summary>
		/// Clears all Activity Categories assigned to the Activity.
		/// </summary>
		/// <param name="parameterInfo">The parameter info associated with the command.</param>
		protected override void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			ActivityCategoryHelper ach = parameterInfo is ActivityCategoryCommandParameterInfo ? ((ActivityCategoryCommandParameterInfo)parameterInfo).ActivityCategoryHelper : null;
			if (null != ach)
				ach.ClearAllActivityCategories(parameterInfo as ActivityCategoryCommandParameterInfo);
		}
	}
	#endregion // ActivityCategoryClearAllActivityCategoriesCommand

	#region ActivityCategoryDisplayActivityCategoriesDialogCommand
	/// <summary>
	/// A command that displays a dialog that shows all valid ActivityCategories for the Activity and which allows editing of those categories.
	/// </summary>
	public class ActivityCategoryDisplayActivityCategoriesDialogCommand : ActivityCategoryCommandBase
	{
		/// <summary>
		/// Displays a dialog that shows all valid ActivityCategories for the Activity and which allows editing of those categories.
		/// </summary>
		/// <param name="parameterInfo">The parameter info associated with the command.</param>
		protected override void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			ActivityCategoryHelper ach = parameterInfo is ActivityCategoryCommandParameterInfo ? ((ActivityCategoryCommandParameterInfo)parameterInfo).ActivityCategoryHelper : null;
			if (null != ach)
				ach.DisplayActivityCategoriesDialog(parameterInfo as ActivityCategoryCommandParameterInfo);
		}
	}
	#endregion // ActivityCategoryDisplayActivityCategoriesDialogCommand

	#region ActivityCategoryToggleActivityCategorySelectedStateCommand
	/// <summary>
	/// A command that toggles the selected state of an ActivityCategory for the Activity.
	/// </summary>
	public class ActivityCategoryToggleActivityCategorySelectedStateCommand : ActivityCategoryCommandBase
	{
		/// <summary>
		/// Toggles the selected state of an ActivityCategory for the Activity.
		/// </summary>
		/// <param name="parameterInfo">The parameter info associated with the command.</param>
		protected override void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			ActivityCategoryHelper ach = parameterInfo is ActivityCategoryCommandParameterInfo ? ((ActivityCategoryCommandParameterInfo)parameterInfo).ActivityCategoryHelper : null;
			if (null != ach)
				ach.ToggleActivityCategorySelectedState(parameterInfo as ActivityCategoryCommandParameterInfo);
		}
	}
	#endregion // ActivityCategoryToggleActivityCategorySelectedStateCommand

	#region ActivityCategoryClearActivityCategorySelectedStateCommand
	/// <summary>
	/// A command that toggles the selected state of a specific ActivityCategory for the Activity.
	/// </summary>
	public class ActivityCategoryClearActivityCategorySelectedStateCommand : ActivityCategoryCommandBase
	{
		/// <summary>
		/// Toggles the selected state of an ActivityCategory for the Activity.
		/// </summary>
		/// <param name="parameterInfo">The parameter info associated with the command.</param>
		protected override void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			ActivityCategoryHelper ach = parameterInfo is ActivityCategoryCommandParameterInfo ? ((ActivityCategoryCommandParameterInfo)parameterInfo).ActivityCategoryHelper : null;
			if (null != ach)
				ach.ClearActivityCategorySelectedState(parameterInfo as ActivityCategoryCommandParameterInfo);
		}
	}
	#endregion // ActivityCategoryClearActivityCategorySelectedStateCommand

	#region ActivityCategorySetActivityCategorySelectedStateCommand
	/// <summary>
	/// A command that sets the selected state of a specific ActivityCategory for the Activity.
	/// </summary>
	public class ActivityCategorySetActivityCategorySelectedStateCommand : ActivityCategoryCommandBase
	{
		/// <summary>
		/// Sets the selected state of an ActivityCategory for the Activity.
		/// </summary>
		/// <param name="parameterInfo">The parameter info associated with the command.</param>
		protected override void ExecuteCommand(ActivityCategoryCommandParameterInfo parameterInfo)
		{
			ActivityCategoryHelper ach = parameterInfo is ActivityCategoryCommandParameterInfo ? ((ActivityCategoryCommandParameterInfo)parameterInfo).ActivityCategoryHelper : null;
			if (null != ach)
				ach.SetActivityCategorySelectedState(parameterInfo as ActivityCategoryCommandParameterInfo);
		}
	}
	#endregion // ActivityCategorySetActivityCategorySelectedStateCommand

	#region ActivityCategoryCommandParameterInfo
	/// <summary>
	/// Class that is passed as a parameter to ActivityCategory commands that exposes properties of interest tothe commands.
	/// </summary>
	/// <seealso cref="ActivityCategoryClearAllActivityCategoriesCommand"/>
	/// <seealso cref="ActivityCategoryDisplayActivityCategoriesDialogCommand"/>
	/// <seealso cref="ActivityCategoryToggleActivityCategorySelectedStateCommand"/>
	/// <seealso cref="ActivityCategoryHelper"/>
	public class ActivityCategoryCommandParameterInfo
	{
		/// <summary>
		/// Creates an instance of ActivityCategoryCommandParameterInfo.
		/// </summary>
		/// <param name="activityCategoryHelper">A reference to an <see cref="ActivityCategoryHelper"/> class instance that is processing the command</param>
		/// <param name="activityCategoryListItem">A reference to an <see cref="ActivityCategoryListItem"/> class instance that the command should act on.  This parameter can be ull for commands that do not act on a specific <see cref="ActivityCategoryListItem"/>.</param>
		/// <seealso cref="ActivityCategoryClearAllActivityCategoriesCommand"/>
		/// <seealso cref="ActivityCategoryDisplayActivityCategoriesDialogCommand"/>
		/// <seealso cref="ActivityCategoryToggleActivityCategorySelectedStateCommand"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		public ActivityCategoryCommandParameterInfo(ActivityCategoryHelper activityCategoryHelper, ActivityCategoryListItem activityCategoryListItem)
		{
			CoreUtilities.ValidateNotNull(activityCategoryHelper, "activityCategoryHelper");

			this.ActivityCategoryHelper		= activityCategoryHelper;
			this.ActivityCategoryListItem	= activityCategoryListItem;
		}

		/// <summary>
		/// Returns the <see cref="ActivityCategoryHelper"/> class that will handle the command.
		/// </summary>
		/// <seealso cref="ActivityCategoryClearAllActivityCategoriesCommand"/>
		/// <seealso cref="ActivityCategoryDisplayActivityCategoriesDialogCommand"/>
		/// <seealso cref="ActivityCategoryToggleActivityCategorySelectedStateCommand"/>
		/// <seealso cref="ActivityCategoryHelper"/>
		public ActivityCategoryHelper ActivityCategoryHelper { get; private set; }

		/// <summary>
		/// Returns the <see cref="ActivityCategory"/> (if any) associated with the command.
		/// </summary>
		/// <seealso cref="ActivityCategoryClearAllActivityCategoriesCommand"/>
		/// <seealso cref="ActivityCategoryDisplayActivityCategoriesDialogCommand"/>
		/// <seealso cref="ActivityCategoryToggleActivityCategorySelectedStateCommand"/>
		/// <seealso cref="ActivityCategoryListItem"/>
		public ActivityCategoryListItem ActivityCategoryListItem { get; private set; }
	}
	#endregion //ActivityCategoryCommandParameterInfo
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