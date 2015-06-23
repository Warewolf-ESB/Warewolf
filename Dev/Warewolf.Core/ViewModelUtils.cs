using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Core
{
    public static class ViewModelUtils
    {
        
        public static void RaiseCanExecuteChanged(this ICommand commandForCanExecuteChange)
        {
            var command = commandForCanExecuteChange as DelegateCommand;
            if (command != null)
            {
                command.RaiseCanExecuteChanged();
            }
        }
    }
}