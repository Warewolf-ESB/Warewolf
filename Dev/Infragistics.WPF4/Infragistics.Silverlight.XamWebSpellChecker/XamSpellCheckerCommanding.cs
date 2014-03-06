using Infragistics;
using System.Windows.Input;
using Infragistics.Controls.Interactions.Primitives;

namespace Infragistics.Controls.Interactions.Primitives
{

    #region SpellCheckerCommandBase
    /// <summary>
    /// Base class for all commands that deal with a <see cref="XamSpellCheckerDialogWindow"/>.
    /// </summary>
    public abstract class XamSpellCheckerCommandBase : CommandBase
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
            XamSpellCheckerDialogWindow win = parameter as XamSpellCheckerDialogWindow;
            if (win != null)
                return this.CanExecuteDialogCommand(win);
            else
                return false;
        }

        /// <summary>
        /// Reports if the <see cref="CanExecuteDialogCommand"/> can be executed on the <see cref="XamSpellCheckerDialogWindow"/>.
        /// </summary>
        /// <param name="dialog">The dialog that the command will be executed against.</param>
        /// <returns>True if the dialog can support this command.</returns>
        protected virtual bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog) { return false; }
        #endregion

        #region Execute
        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter">The <see cref="XamSpellCheckerDialogWindow"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamSpellCheckerDialogWindow win = parameter as XamSpellCheckerDialogWindow;
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
        /// Executes the specific command on the specified <see cref="XamSpellCheckerDialogWindow"/>
        /// </summary>
        /// <param name="dialog"></param>
        protected abstract void ExecuteCommand(XamSpellCheckerDialogWindow dialog);

        #endregion // Protected

        #endregion // Overrides
    }
    #endregion // XamDialogWindowCommandBase

    #region AddToDictionaryCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will change the current misspelled word.
    /// </summary>
    public class AddToDictionaryCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes change command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.AddToDictionary();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }
    #endregion // ChangeCommand

    #region CancelAsyncDictionaryCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object which will cancel the dictionaries downloading.
    /// </summary>
    public class CancelAsyncDictionaryCommand : CommandBase
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
        /// <param name="parameter">The <see cref="DictionaryLoadProgressDialog"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            DictionaryLoadProgressDialog win = parameter as DictionaryLoadProgressDialog;
            if (win != null)
            {
                win.CancelAsyncDictionaryLoad();
                this.CommandSource.Handled = true;
            }
            base.Execute(parameter);
        }
        #endregion // Execute

        #endregion // Public

        #endregion // Overrides
    }
    #endregion // XamDialogWindowCommandBase

    #region IgnoreCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will ignore the current misspelled word.  This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class IgnoreCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes ignore command on the supplied XamSpellChecker.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.Ignore();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return false;
            return dialog.BadWords.Count > 0;
        }
    }
    #endregion // IgnoreCommand

    #region IgnoreAllCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will ignore the current misspelled word in whole text.  This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class IgnoreAllCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes ignore command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.IgnoreAll();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return false;
            return dialog.BadWords.Count > 0;
        }
    }
    #endregion // IgnoreAllCommand

    #region ChangeCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will change the current misspelled word.  This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class ChangeCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes change command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.Change();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return false;
            return dialog.BadWords.Count > 0;
        }
    }
    #endregion // ChangeCommand

    #region ChangeAllCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will change the current misspelled word in whole text.  This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class ChangeAllCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes ignore command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.ChangeAll();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return false;
            return dialog.BadWords.Count > 0;
        }
    }
    #endregion // ChangeAllCommand

    #region CloseDialogCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will close the spell checker dialog window. This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class CloseDialogCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes the close dialog command.
        /// </summary>
        /// <param name="dialog"></param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;

            if (dialog._checker.CurrentBadWord != null)
                dialog._checker.OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, true, null));

            dialog.Close();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }

    #endregion //CloseDialogCommand

    #region CancelDialogCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will close the spell checker dialog window. This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class CancelDialogCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes the close dialog command.
        /// </summary>
        /// <param name="dialog"></param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            if (dialog._checker.CurrentBadWord == null)
                dialog._checker.OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, false, null));
            else
                dialog._checker.OnSpellCheckCompleted(new SpellCheckCompletedEventArgs(null, true, null));
            dialog.Close();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }
    }

    #endregion //CancelDialogCommand

    #region NextFieldCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will begin spell checking the next field. This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class NextFieldCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes next editor command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null)
                return;
            dialog.NextField();
        }

        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null || dialog._checker == null)
                return false;
            return dialog._checker.CurrentSpellCheckTargetIndex + 1 < dialog._checker.SpellCheckTargets.Count;
        }

    }
    #endregion // SpellCheckCompleteCommand

    #region PreviousFieldCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object that will begin spell checking the previous field. This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class PreviousFieldCommand : XamSpellCheckerCommandBase
    {
        /// <summary>
        /// Executes previous field command on the supplied SpellCheckDialog.
        /// </summary>
        /// <param name="dialog">A <see cref="XamSpellCheckerDialogWindow"/> object that will be used to apply command.</param>
        protected override void ExecuteCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog != null)
                dialog.PreviousField();
        }
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="dialog">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        protected override bool CanExecuteDialogCommand(XamSpellCheckerDialogWindow dialog)
        {
            if (dialog == null || dialog._checker == null)
                return false;
            return dialog._checker.CurrentSpellCheckTargetIndex - 1 > -1;
        }
    }
    #endregion // SpellCheckCompleteCommand

    #region SpellCheckCommand
    /// <summary>
    /// A <see cref="CommandBase"/> object used to execute spell check commands. This command only works from whithin the spell
    /// checker dialog window.
    /// </summary>
    public class SpellCheckCommand : CommandBase
    {
        /// <summary>
        /// Reports if the command can be executed on the object inputted.
        /// </summary>
        /// <param name="parameter">The object that the command will be executed against.</param>
        /// <returns>True if the object can support this command.</returns>
        public override bool CanExecute(object parameter)
        {
            return true;
        }


        /// <summary>
        /// Executes the command 
        /// </summary>
        /// <param name="parameter">The <see cref="DictionaryLoadProgressDialog"/> object that will be executed against.</param>
        public override void Execute(object parameter)
        {
            XamSpellChecker spellChecker = parameter as XamSpellChecker;
            if (spellChecker != null)
            {
                if (this.CommandSource.Parameter != null)
                    spellChecker.SpellCheck(this.CommandSource.Parameter as string);
                else
                    spellChecker.SpellCheck();
                this.CommandSource.Handled = true;
            }
            base.Execute(parameter);
        }

        //protected abstract void ExecuteCommand(XamSpellChecker spellChecker);
    }
    #endregion // SpellCheckCommand
}

namespace Infragistics.Controls.Interactions
{

    #region XamSpellCheckerCommandSource


    /// <summary>
    /// The command source object for <see cref="XamSpellChecker"/> object.
    /// </summary>
    public class XamSpellCheckerCommandSource : CommandSource
    {
        /// <summary>
        /// Gets / Sets the XamSpellCheckerCommand which is to be executed by the command.
        /// </summary>
        public XamSpellCheckerCommand CommandType
        {
            get;
            set;
        }

        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case XamSpellCheckerCommand.SpellCheck:
                    return new SpellCheckCommand();

                case XamSpellCheckerCommand.CancelAsyncDictionaryDownload:
                    return new CancelAsyncDictionaryCommand();

            }
            return null;
        }
    }
    #endregion //XamSpellCheckerCommandSource

    #region XamSpellCheckerDialogCommandSource
    /// <summary>
    /// The command source object for <see cref="XamSpellCheckerDialogWindow"/> object.
    /// </summary>
    public class XamSpellCheckerDialogCommandSource : CommandSource
    {
        /// <summary>
        /// Gets / Sets the XamSpellCheckerCommand which is to be executed by the command.
        /// </summary>
        public XamSpellCheckerDialogCommand CommandType
        {
            get;
            set;
        }

        /// <summary>
        /// Generates the <see cref="ICommand"/> object that will execute the command.
        /// </summary>
        protected override ICommand ResolveCommand()
        {
            switch (this.CommandType)
            {
                case XamSpellCheckerDialogCommand.Ignore:
                    return new IgnoreCommand();

                case XamSpellCheckerDialogCommand.IgnoreAll:
                    return new IgnoreAllCommand();

                case XamSpellCheckerDialogCommand.Change:
                    return new ChangeCommand();

                case XamSpellCheckerDialogCommand.ChangeAll:
                    return new ChangeAllCommand();

                case XamSpellCheckerDialogCommand.CloseDialog:
                    return new CloseDialogCommand();

                case XamSpellCheckerDialogCommand.NextField:
                    return new NextFieldCommand();

                case XamSpellCheckerDialogCommand.PreviousField:
                    return new PreviousFieldCommand();

                case XamSpellCheckerDialogCommand.CancelDialog:
                    return new CancelDialogCommand();

                case XamSpellCheckerDialogCommand.AddToDictionary:
                    return new AddToDictionaryCommand();

            }
            return null;
        }
    }
    #endregion //XamSpellCheckerDialogCommandSource
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