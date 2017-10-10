/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.Configuration.ViewModels.Base
{
    public class RelayCommand : IRelayCommand
    {
        readonly Action<object> _handlingMethod;
        readonly Predicate<object> _canHandlingMethodExecute;

        public RelayCommand(Action<object> handlingMethod, Predicate<object> canHandingMethodExecute)
        {
            if(handlingMethod == null)
            {
                
                throw new ArgumentNullException("HandingMethod");
                
            }

            _handlingMethod = handlingMethod;
            _canHandlingMethodExecute = canHandingMethodExecute;
        }

        public RelayCommand(Action<object> handlingMethod) : this(handlingMethod, null) { }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canHandlingMethodExecute == null || _canHandlingMethodExecute(parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _handlingMethod(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
                CommandManager.InvalidateRequerySuggested();
            }
        }
        #endregion
    }

    public class RelayCommand<T> : ICommand
    {
        #region Fields

        readonly Action<T> _execute;
        readonly Predicate<T> _canExecute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if(execute == null)
            {
                throw new ArgumentNullException("execute");
            }

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            
            return _canExecute?.Invoke((T)parameter) ?? true;
            
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

    
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        #endregion // ICommand Members
    }
}
