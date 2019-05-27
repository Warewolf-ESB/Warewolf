#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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

        public bool CanExecute(object parameter) => _canHandlingMethodExecute == null || _canHandlingMethodExecute(parameter);

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
    }
}
