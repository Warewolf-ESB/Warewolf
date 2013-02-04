using System;
using System.Diagnostics;
using System.Windows.Input;

namespace Dev2.Studio.Diagnostics
{
    public class MoreLinkCommand : ICommand
    {
        public void Execute(object parameter)
        {
            var moreLink = parameter as string;
            if(!string.IsNullOrEmpty(moreLink))
            {
                Process.Start(new ProcessStartInfo(moreLink));
            }
        }

        public bool CanExecute(object parameter)
        {
            var moreLink = parameter as string;
            return !string.IsNullOrEmpty(moreLink);
        }

        public event EventHandler CanExecuteChanged;
    }

}
