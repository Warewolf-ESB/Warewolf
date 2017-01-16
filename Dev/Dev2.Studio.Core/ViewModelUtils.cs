using System;
using System.Windows;
using System.Windows.Input;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Security;

namespace Dev2
{
    public static class ViewModelUtils
    {
        
        public static void RaiseCanExecuteChanged(ICommand commandForCanExecuteChange)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null && Application.Current.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RaiseCanExecuteChangedInternal(commandForCanExecuteChange);
                });
            }
            else
            {
                RaiseCanExecuteChangedInternal(commandForCanExecuteChange);
            }
        }

        static void RaiseCanExecuteChangedInternal(ICommand commandForCanExecuteChange)
        {
            if(commandForCanExecuteChange != null)
            {
                var typeOfCommand = commandForCanExecuteChange.GetType();
            
                if (typeOfCommand == typeof(Microsoft.Practices.Prism.Commands.DelegateCommand))
                {
                    var command = commandForCanExecuteChange as Microsoft.Practices.Prism.Commands.DelegateCommand;
                    if (command != null)
                    {
                        if(Application.Current != null)
                        {
                            if(Application.Current.Dispatcher != null)
                            {
                                Application.Current.Dispatcher.BeginInvoke( new Action(() =>
                                {
                                    command.RaiseCanExecuteChanged();
                                }));
                            }
                        }

                        return;
                    }
                }
                if (typeOfCommand.BaseType == typeof(Microsoft.Practices.Prism.Commands.DelegateCommandBase))
                {
                    var command = commandForCanExecuteChange as Microsoft.Practices.Prism.Commands.DelegateCommandBase;
                    if (command != null)
                    {
                        command.RaiseCanExecuteChanged();
                        return;
                    }
                }
                if (typeOfCommand == typeof(RelayCommand))
                {
                    var command = commandForCanExecuteChange as RelayCommand;
                    if (command != null)
                    {
                        command.RaiseCanExecuteChanged();
                        return;
                    }
                }
                if (typeOfCommand == typeof(DelegateCommand))
                {
                    var command = commandForCanExecuteChange as DelegateCommand;
                    command?.RaiseCanExecuteChanged();
                }
                if (typeOfCommand == typeof(AuthorizeCommand))
                {
                    var command = commandForCanExecuteChange as AuthorizeCommand;
                    command?.RaiseCanExecuteChanged();
                }
            }
        }
    }
}