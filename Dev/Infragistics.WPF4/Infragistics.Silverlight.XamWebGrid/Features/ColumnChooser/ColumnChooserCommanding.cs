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
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    #region XamGridColumnChooserCommand
    /// <summary>
    /// An enumeration of available commands for the <see cref="ColumnChooserDialog"/>.
    /// </summary>
    public enum XamGridColumnChooserCommand
    {
       /// <summary>
       /// Closes the ColumnChooserDialog.
       /// </summary>
       Close,

       /// <summary>
       ///  Opens the ColumnChooserDialog.
       /// </summary>
       Open
    }
    #endregion //XamGridColumnChooserCommand

    #region ColumnChooserCommandBase

    /// <summary>
    /// A base class for all commands related to the <see cref="ColumnChooserDialog"/>
    /// </summary>
    public abstract class ColumnChooserCommandBase : CommandBase
    {
        #region Overrides

        #region Public

        #region CanExecute
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param propertyName="parameter">The object that the command will be executed against.</param>
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
        /// <param propertyName="parameter">The <see cref="ColumnLayout"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            ColumnLayout layout = parameter as ColumnLayout;
            if (layout != null && layout.Grid != null)
            {
                this.ExecuteCommand(layout);
            }
            else
            {

                Column col = parameter as Column;
                if (col != null && col.ColumnLayout != null && col.ColumnLayout.Grid != null)
                {
                    if (col.ParentColumn != null)
                        this.ExecuteCommand(col.ParentColumn);
                    else
                        this.ExecuteCommand(col.ColumnLayout);
                }
            }
            base.Execute(parameter);
        }
        #endregion // Execute
        #endregion // Public

        #region Protected
        /// <summary>
        /// Executes the specific command on the specified <see cref="ColumnLayout"/>
        /// </summary>
        /// <param propertyName="col"></param>
        protected abstract void ExecuteCommand(ColumnLayout col);

        /// <summary>
        /// Executes the specific command on the specified <see cref="Column"/>
        /// </summary>
        /// <param name="col"></param>
        protected abstract void ExecuteCommand(Column col);
        

        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // ColumnChooserCommandBase

    #region ColumnChooserCloseCommand

    /// <summary>
    /// A Command that closes the <see cref="ColumnChooserDialog"/> of the <see cref="XamGrid"/>
    /// </summary>
    public class ColumnChooserCloseCommand : ColumnChooserCommandBase
    {
        /// <summary>
        /// Hides the <see cref="ColumnChooserDialog"/> if it's open.
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(ColumnLayout col)
        {
            col.Grid.HideColumnChooser();
        }
        /// <summary>
        /// Hides the <see cref="ColumnChooserDialog"/> if it's open.
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(Column col)
        {
            col.ColumnLayout.Grid.HideColumnChooser();
        }

    }
    #endregion // ColumnChooserCloseCommand

    #region ColumnChooserOpenCommand

    /// <summary>
    /// A Command that opens the <see cref="ColumnChooserDialog"/> for a specified <see cref="ColumnLayout"/> of the <see cref="XamGrid"/>.
    /// </summary>
    public class ColumnChooserOpenCommand : ColumnChooserCommandBase
    {
        /// <summary>
        /// Opens the <see cref="ColumnChooserDialog"/> for the specified <see cref="ColumnLayout"/>
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(ColumnLayout col)
        {
            col.Grid.ShowColumnChooser(col);
        }

        /// <summary>
        /// Opens the <see cref="ColumnChooserDialog"/> for the specified <see cref="Column"/>
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(Column col)
        {
            col.ColumnLayout.Grid.ShowColumnChooser(col);
        }
    }

    #endregion // ColumnChooserOpenCommand

    #region XamGridColumnChooserCommandSource
    /// <summary>
    /// The command source object for <see cref="ColumnChooserDialog"/> object.
    /// </summary>
    public class XamGridColumnChooserCommandSource : CommandSource
    {
        #region Properties

        #region Public

        /// <summary>
        /// Gets / sets the <see cref="XamGridColumnChooserCommand"/> which is to be executed by the command.
        /// </summary>
        public XamGridColumnChooserCommand CommandType
        {
            get;
            set;
        }

        #endregion // Public

        #endregion // Properties

        #region Overrides

        #region Protected

        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        /// <returns></returns>
        protected override ICommand ResolveCommand()
        {
            ICommand command = null;

            switch (this.CommandType)
            {
                case XamGridColumnChooserCommand.Close:
                    {
                        command = new ColumnChooserCloseCommand();
                        break;
                    }
                case XamGridColumnChooserCommand.Open:
                    {
                        command = new ColumnChooserOpenCommand();
                        break;
                    }
            }
            return command;
        }

        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // XamGridColumnChooserCommandSource

    #region HideColumnCommand

    /// <summary>
    /// A Command that hides a <see cref="Column"/>
    /// </summary>
    public class HideColumnCommand : ColumnCommandBase
    {
        /// <summary>
        /// Hides the specified <see cref="Column"/>
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(Column col)
        {
            col.Visibility = Visibility.Collapsed;
        }
    }

    #endregion // HideColumnCommand

    #region ShowColumnCommand

    /// <summary>
    /// A Command that unhides a <see cref="Column"/>
    /// </summary>
    public class ShowColumnCommand : ColumnCommandBase
    {
        /// <summary>
        /// Makes the specified <see cref="Column"/> visible.
        /// </summary>
        /// <param name="col"></param>
        protected override void ExecuteCommand(Column col)
        {            
            col.Visibility = Visibility.Visible;
        }
    }
    #endregion // ShowColumnCommand

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