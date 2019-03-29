#pragma warning disable
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

                if (typeOfCommand == typeof(Microsoft.Practices.Prism.Commands.DelegateCommand) && commandForCanExecuteChange is Microsoft.Practices.Prism.Commands.DelegateCommand command)
                {
                    if (Application.Current != null && Application.Current.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                       {
                           command.RaiseCanExecuteChanged();
                       }));
                    }


                    return;
                }

                // TODO: refactor RaiseCanExecuteChanged into a common interface for the "Command" type

                if (typeOfCommand.BaseType == typeof(Microsoft.Practices.Prism.Commands.DelegateCommandBase) && commandForCanExecuteChange is Microsoft.Practices.Prism.Commands.DelegateCommandBase commandBase)
                {
                    commandBase.RaiseCanExecuteChanged();
                    return;
                }

                if (typeOfCommand == typeof(RelayCommand) && commandForCanExecuteChange is RelayCommand relayCommand)
                {
                    relayCommand.RaiseCanExecuteChanged();
                    return;
                }

                if (typeOfCommand == typeof(DelegateCommand))
                {
                    var delegateCommand = commandForCanExecuteChange as DelegateCommand;
                    delegateCommand?.RaiseCanExecuteChanged();
                }
                if (typeOfCommand == typeof(AuthorizeCommand))
                {
                    var authorizeCommand = commandForCanExecuteChange as AuthorizeCommand;
                    authorizeCommand?.RaiseCanExecuteChanged();
                }
            }
        }
    }
}