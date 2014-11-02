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
    public class RelayCommand : ICommand
    {
        private readonly Predicate<object> _canHandlingMethodExecute;
        private readonly Action<object> _handlingMethod;

        public RelayCommand(Action<object> handlingMethod, Predicate<object> canHandingMethodExecute)
        {
            if (handlingMethod == null)
            {
                // ReSharper disable NotResolvedInText
                throw new ArgumentNullException("HandingMethod");
                // ReSharper restore NotResolvedInText
            }

            _handlingMethod = handlingMethod;
            _canHandlingMethodExecute = canHandingMethodExecute;
        }

        public RelayCommand(Action<object> handlingMethod) : this(handlingMethod, null)
        {
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canHandlingMethodExecute == null || _canHandlingMethodExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _handlingMethod(parameter);
        }

        #endregion
    }

    public class RelayCommand<T> : ICommand
    {
        #region Fields

        private readonly Predicate<T> _canExecute;
        private readonly Action<T> _execute;

        #endregion // Fields

        #region Constructors

        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommand(Action<T> execute, Predicate<T> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            // ReSharper disable SimplifyConditionalTernaryExpression
            return _canExecute == null ? true : _canExecute((T) parameter);
            // ReSharper restore SimplifyConditionalTernaryExpression
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T) parameter);
        }

        #endregion // ICommand Members
    }
}