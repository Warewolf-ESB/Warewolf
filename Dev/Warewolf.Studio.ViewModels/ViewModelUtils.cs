using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public static class ViewModelUtils
    {
        
        public static void RaiseCanExecuteChanged(ICommand commandForCanExecuteChange)
        {
            var command = commandForCanExecuteChange as DelegateCommand;
            if (command != null)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }
}