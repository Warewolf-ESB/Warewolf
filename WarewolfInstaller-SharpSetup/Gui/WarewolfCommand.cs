using System;
using System.Windows.Input;

namespace Gui
{
    public class WarewolfCommand : ICommand
    {
        readonly Action _action;

        public WarewolfCommand(Action action)
        {
            _action = action;
        }

        #region Implementation of ICommand

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            if(_action != null)
            {
                _action();
            }
        }

        public event EventHandler CanExecuteChanged;

        #endregion
    }
}