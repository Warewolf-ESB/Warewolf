using System;
using System.Windows.Input;
using Infragistics.Controls.Interactions.Primitives;

namespace Infragistics.Controls.Interactions.Primitives
{
    #region CloseCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will be closed.
    /// </summary>
    public class CloseCommand : XamDialogWindowCommandBase
    {
        /// <summary>
        /// Applies closing a XamDialogWindow.
        /// </summary>
        /// <param name="win">A <see cref="XamDialogWindow"/> object that will be closed.</param>
        protected override void ExecuteCommand(XamDialogWindow win)
        {
            win.Close();
        }
    }
    #endregion // CloseCommand

    #region MaximizeChangeCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will be maximized.
    /// </summary>
    public class MaximizeCommand : XamDialogWindowCommandBase
    {
        /// <summary>
        /// Applies maximizing a XamDialogWindow.
        /// </summary>
        /// <param name="win">A <see cref="XamDialogWindow"/> object that will be maximized.</param>
        protected override void ExecuteCommand(XamDialogWindow win)
        {
            win.WindowState = WindowState.Maximized;
        }
    }
    #endregion // MaximizeChangeCommand

    #region MinimizeChangeCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will be minimized.
    /// </summary>
    public class MinimizeCommand : XamDialogWindowCommandBase
    {
        /// <summary>
        /// Applies minimizing a XamDialogWindow.
        /// </summary>
        /// <param name="win">A <see cref="XamDialogWindow"/> object that will be minimized.</param>
        protected override void ExecuteCommand(XamDialogWindow win)
        {
			win.WindowState = WindowState.Minimized; ;
        }
    }
    #endregion // MinimizeChangeCommand

	#region RestoreCommand
	/// <summary>
	/// A <see cref="CommandBase"/> for setting a <see cref="XamDialogWindow"/>'s WindowState to normal.
	/// </summary>
	public class RestoreCommand : XamDialogWindowCommandBase
	{
		/// <summary>
		/// Executes the commmand.
		/// </summary>
		/// <param name="win"></param>
		protected override void ExecuteCommand(XamDialogWindow win)
		{
			win.WindowState = WindowState.Normal;
		}
	}
	#endregion // RestoreCommand

	#region ToggleMaximizeCommand
	/// <summary>
	/// A <see cref="CommandBase"/> for toggling between the Normal and Maximized states.
	/// </summary>
	public class ToggleMaximizeCommand : XamDialogWindowCommandBase
	{
		/// <summary>
		/// Executes the commmand.
		/// </summary>
		/// <param name="win"></param>
		protected override void ExecuteCommand(XamDialogWindow win)
		{
			if (win.WindowState != WindowState.Maximized)
				win.WindowState = WindowState.Maximized;
			else
				win.WindowState = WindowState.Normal;

		}
	}
	#endregion // ToggleMaximizeCommand

	#region ToggleMinimizeCommand
	/// <summary>
	/// A <see cref="CommandBase"/> for toggling between the Normal and Minimized states.
	/// </summary>
	public class ToggleMinimizeCommand : XamDialogWindowCommandBase
	{
		/// <summary>
		/// Executes the commmand.
		/// </summary>
		/// <param name="win"></param>
		protected override void ExecuteCommand(XamDialogWindow win)
		{
			if (win.WindowState != WindowState.Minimized)
				win.WindowState = WindowState.Minimized;
			else
				win.WindowState = WindowState.Normal;
		}
	}
	#endregion // ToggleMinimizeCommand


#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

    #region XamDialogWindowCommandBase
    /// <summary>
    /// Base class for all commands that deal with a <see cref=" XamDialogWindow"/>.
    /// </summary>
    public abstract class XamDialogWindowCommandBase : CommandBase
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
        /// <param name="parameter">The <see cref=" XamDialogWindow"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamDialogWindow win = parameter as XamDialogWindow;
            if (win != null)
            {
                this.ExecuteCommand(win);
                this.CommandSource.Handled = true;
            }

            base.Execute(parameter);
        }
        #endregion // Execute
        #endregion // Public

        #region Protected
        /// <summary>
        /// Executes the specific command on the specified <see cref="XamDialogWindow"/>
        /// </summary>
        /// <param name="win">The window for which the command will be executed.</param>
        protected abstract void ExecuteCommand(XamDialogWindow win);
        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // XamDialogWindowCommandBase

}

namespace Infragistics.Controls.Interactions
{
    /// <summary>
    /// The command source object for <see cref="XamDialogWindow"/> object.
    /// </summary>
    public class XamDialogWindowCommandSource : CommandSource
    {
        /// <summary>
        /// Gets or sets the XamDialogWindowCommand which is to be executed by the command.
        /// </summary>
        public XamDialogWindowCommand CommandType
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
                case XamDialogWindowCommand.Maximize:
                    return new MaximizeCommand();

                case XamDialogWindowCommand.Minimize:
                    return new MinimizeCommand();

                case XamDialogWindowCommand.Close:
                    return new CloseCommand();

				case XamDialogWindowCommand.ToggleMaximize:
					return new ToggleMaximizeCommand();

				case XamDialogWindowCommand.ToggleMinimize:
					return new ToggleMinimizeCommand();

				case XamDialogWindowCommand.Restore:
					return new RestoreCommand();





            }

            return null;
        }
    }
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