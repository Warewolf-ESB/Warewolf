using System.Windows;
using System.Windows.Input;
using Infragistics.Controls;
using Infragistics.Controls.Menus.Primitives;

namespace Infragistics.Controls.Menus
{
    #region XamContextMenuCommandSource

    /// <summary>
    /// The command source object for <see cref="XamContextMenu"/> object.
    /// </summary>
    public class XamContextMenuCommandSource : CommandSource
    {
        #region Properties

        /// <summary>
        /// Gets/sets the XamContextMenuCommand which is to be executed by the command.
        /// </summary>
        public XamContextMenuCommand CommandType { get; set; }

        #endregion //Properties

        #region Base Class Overrides

        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns>The object that will execute the command.</returns>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case XamContextMenuCommand.Open:
                    return new OpenCommand();
                case XamContextMenuCommand.Close:
                    return new CloseCommand();
            }

            return null;
        }

        #endregion //Base Class Overrides
    }

    #endregion //XamContextMenuCommandSource
}

namespace Infragistics.Controls.Menus.Primitives
{
    #region XamContextMenuCommandBase
    /// <summary>
    /// Base class for all commands that deal with a <see cref="XamContextMenu"/>.
    /// </summary>
    public abstract class XamContextMenuCommandBase : CommandBase
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
            XamContextMenu contextMenu = parameter as XamContextMenu;
            if (contextMenu != null)
            {
                return this.CanExecuteCommand(contextMenu);
            }

            return base.CanExecute(parameter);
        }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter">The <see cref="XamContextMenu"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamContextMenu contextMenu = parameter as XamContextMenu;
            if (contextMenu != null)
            {
                this.ExecuteCommand(contextMenu);
                if (this.CommandSource != null)
                {
                    this.CommandSource.Handled = true;
                }
            }

            base.Execute(parameter);
        }
        #endregion // Execute

        #endregion // Public

        #region Protected
        /// <summary>
        /// Executes the specific command on the specified <see cref="XamContextMenu"/>
        /// </summary>
        /// <param name="contextMenu">target XamContextMenu</param>
        protected abstract void ExecuteCommand(XamContextMenu contextMenu);

        /// <summary>
        /// Executes the specific command on the specified <see cref="XamContextMenu"/>
        /// </summary>
        /// <param name="contextMenu">target XamContextMenu</param>
        /// <returns>
        /// <c>true</c> if the object can execute command; otherwise, <c>false</c>.
        /// </returns>
        protected abstract bool CanExecuteCommand(XamContextMenu contextMenu);
        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // XamContextMenuCommandBase

    #region OpenCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will open the XamContextMenu.
    /// </summary>
    public class OpenCommand : XamContextMenuCommandBase
    {
        /// <summary>
        /// Opens the XamContextMenu.
        /// </summary>
        /// <param name="contextMenu">A <see cref="XamContextMenu"/> object that will be opened.</param>
        protected override void ExecuteCommand(XamContextMenu contextMenu)
        {
            ContextMenuService.PositionContextMenu(contextMenu, new Point(0, 0), contextMenu.ParentElement);
        }

        /// <summary>
        /// Reports if the OpenCommand can be executed on the specified <see cref="XamContextMenu"/>.
        /// </summary>
        /// <param name="contextMenu">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteCommand(XamContextMenu contextMenu)
        {
            return contextMenu != null && !contextMenu.IsOpen;
        }
    }
    #endregion // OpenCommand

    #region CloseCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will close the XamContextMenu.
    /// </summary>
    public class CloseCommand : XamContextMenuCommandBase
    {
        /// <summary>
        /// Closes the XamContextMenu.
        /// </summary>
        /// <param name="contextMenu">A <see cref="XamContextMenu"/> object that will be closed.</param>
        protected override void ExecuteCommand(XamContextMenu contextMenu)
        {
            contextMenu.IsOpen = false;
        }

        /// <summary>
        /// Reports if the CloseCommand can be executed on the specified <see cref="XamContextMenu"/>.
        /// </summary>
        /// <param name="contextMenu">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteCommand(XamContextMenu contextMenu)
        {
            return contextMenu != null && contextMenu.IsOpen;
        }
    }
    #endregion // OpenCommand
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