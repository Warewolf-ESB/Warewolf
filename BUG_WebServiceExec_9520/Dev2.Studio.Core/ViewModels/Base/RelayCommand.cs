using System;
using System.Windows.Input;

namespace Dev2.Studio.Core.ViewModels.Base
{
    public class RelayCommand : ICommand
    {
        readonly Action<object> _handlingMethod;
        readonly Predicate<object> _canHandlingMethodExecute;

        public RelayCommand(Action<object> handlingMethod, Predicate<object> canHandingMethodExecute)
        {
            if (handlingMethod == null)
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
            return _canHandlingMethodExecute == null ? true : _canHandlingMethodExecute(parameter);
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

        readonly Action<T> _execute = null;
        readonly Predicate<T> _canExecute = null;

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
            if (execute == null)
                throw new ArgumentNullException("execute");

            _execute = execute;
            _canExecute = canExecute;
        }

        #endregion // Constructors

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        #endregion // ICommand Members
    }
}
