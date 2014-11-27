
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Input;

namespace Dev2.Runtime.Configuration.ViewModels.Base
{
    public class DelegateCommand : ICommand
    {
        readonly Action<object> _action;
        private readonly Predicate<object> _canExecute;

        public DelegateCommand(Action<object> action, Predicate<object> canExecute)
        {
            if(action == null)
            {
                throw new ArgumentNullException("action");
            }

            _action = action;
            _canExecute = canExecute;
        }

        public DelegateCommand(Action<object> action)
        {
            if(action == null)
            {
                throw new ArgumentNullException("action");
            }

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
            return _canExecute == null || _canExecute(parameter);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if(handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
