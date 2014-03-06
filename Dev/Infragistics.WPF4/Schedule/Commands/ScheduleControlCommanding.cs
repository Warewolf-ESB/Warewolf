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
	#region ScheduleControlCommandBase Class
	/// <summary>
	/// Base class for all commands that deal with a <see cref=" ScheduleControlBase"/>.
	/// </summary>
	public abstract class ScheduleControlCommandBase : CommandBase
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
		/// <param name="parameter">The <see cref=" ScheduleControlBase"/> object that will be executed against.</param>
		public override void Execute(object parameter)
		{
			ScheduleControlBase adc = parameter as ScheduleControlBase;

			// AS 12/6/10 NA 11.1 - XamOutlookCalendarView
			if ( adc == null )
			{
				var outlookView = parameter as XamOutlookCalendarView;

				if ( null != outlookView )
					adc = outlookView.CurrentViewControl;
			}

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
		/// Executes the specific command on the specified <see cref="ScheduleControlBase"/>
		/// </summary>
		/// <param name="scheduleControl">The control for which the command will be executed.</param>
		protected abstract void ExecuteCommand(ScheduleControlBase scheduleControl);
		#endregion //ExecuteCommand

		#endregion // Protected

		#endregion // Overrides
	}
	#endregion // ScheduleControlCommandBase Class

	#region ScheduleControlCommandSource Class
	/// <summary>
	/// The command source object for <see cref="ScheduleControlBase"/> object.
	/// </summary>
	public class ScheduleControlCommandSource : CommandSource
	{
		/// <summary>
		/// Gets or sets the ScheduleControlCommand which is to be executed by the command.
		/// </summary>
		public ScheduleControlCommand CommandType
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
			return new ScheduleControlBaseCommand(this.CommandType);
		}
	}

	#endregion //ScheduleControlCommandSource Class

	#region ScheduleControl Commands

	#region ScheduleControlBaseCommand
	/// <summary>
	/// A command object used to execute a command for a <see cref="ScheduleControlBase"/>.
	/// </summary>
	public class ScheduleControlBaseCommand : ScheduleControlCommandBase
	{
		private ScheduleControlCommand _commandType;

		/// <summary>
		/// Initializes a new <see cref="ScheduleControlCommand"/>
		/// </summary>
		/// <param name="commandType"></param>
		public ScheduleControlBaseCommand(ScheduleControlCommand commandType)
		{
			_commandType = commandType;
		}

		/// <summary>
		/// Reports if the command can be executed on the object inputted.
		/// </summary>
		/// <param name="parameter">The object that the command will be executed against.</param>
		/// <returns>True if the object can support this command.</returns>
		public override bool CanExecute(object parameter)
		{
			if (parameter is ScheduleControlBase)
				return ((ScheduleControlBase)parameter).CanExecuteCommand(this._commandType);

			return true;
		}

		/// <summary>
		/// Executes the command
		/// </summary>
		/// <param name="ScheduleControl">The <see cref="ScheduleControlBase"/> object that will execute the command.</param>
		protected override void ExecuteCommand(ScheduleControlBase ScheduleControl)
		{
			ScheduleControl.ExecuteCommand(this._commandType);
		}
	}
	#endregion // ScheduleControlCloseCommand

	#endregion // ScheduleControl Commands
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