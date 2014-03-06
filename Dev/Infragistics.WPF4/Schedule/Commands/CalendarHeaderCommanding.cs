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
	#region CalendarHeaderCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" CalendarHeader"/>.
	/// </summary>
	public abstract class CalendarHeaderCommandBase : CommandBase
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
		/// <param name="parameter">The <see cref=" CalendarHeader"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			CalendarHeader adc = parameter as CalendarHeader;
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
		/// Executes the specific command on the specified <see cref="CalendarHeader"/>
		/// </summary>
		/// <param name="CalendarHeader">The window for which the command will be executed.</param>
		protected abstract void ExecuteCommand(CalendarHeader CalendarHeader);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // CalendarHeaderCommandBase Class

	#region CalendarHeaderCommandSource Class
	/// <summary>
	/// The command source object for <see cref="CalendarHeader"/> object.
	/// </summary>
	public class CalendarHeaderCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the CalendarHeaderCommand which is to be executed by the command.
		/// </summary>
		public CalendarHeaderCommand CommandType
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
				case CalendarHeaderCommand.Close:
					return new CalendarHeaderCloseCommand();

				case CalendarHeaderCommand.ToggleOverlayMode:
					return new CalendarHeaderToggleOverlayModeCommand();
			}

			return null;
		}
	}

	#endregion //CalendarHeaderCommandSource Class

	#region CalendarHeader Commands

	#region CalendarHeaderCloseCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will be closed.
	/// </summary>
	public class CalendarHeaderCloseCommand : CalendarHeaderCommandBase
	{
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is CalendarHeader)
				return ((CalendarHeader)parameter).CanClose;

			return true;
		}

		/// <summary>
		/// Applies closing a XamDialogWindow.
		/// </summary>
		/// <param name="CalendarHeader">The <see cref="CalendarHeader"/> object that will be closed.</param>
		protected override void ExecuteCommand(CalendarHeader CalendarHeader)
		{
			CalendarHeader.Close();
		}
	}
	#endregion // CalendarHeaderCloseCommand

	#region CalendarHeaderToggleOverlayCommand
	/// <summary>
	/// A <see cref="CommandBase"/> object that will be closed.
	/// </summary>
	public class CalendarHeaderToggleOverlayModeCommand : CalendarHeaderCommandBase
	{
		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			CalendarHeader header = parameter as CalendarHeader;
			if (header != null)
			{
				header.VerifyState();
				return header.OverlayButtonVisibility == Visibility.Visible;
			}

			return true;
		}

		/// <summary>
		/// Applies closing a XamDialogWindow.
		/// </summary>
		/// <param name="CalendarHeader">The <see cref="CalendarHeader"/> object that will be closed.</param>
		protected override void ExecuteCommand(CalendarHeader CalendarHeader)
		{
			CalendarHeader.ToggleOverlayMode();
		}
	}
	#endregion // CalendarHeaderToggleOverlayCommand

	#endregion // CalendarHeader Commands
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