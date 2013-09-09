using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Help;
using Dev2.Providers.Errors;
using Dev2.Studio.Core.ViewModels.Base;

namespace Dev2.Activities.Adorners
{
    public class HelpViewModel : Screen
    {

        private string _helpText;
        List<IActionableErrorInfo> _errors;
        readonly ICommand _openHyperlinkCommand;

        public HelpViewModel()
        {
            _openHyperlinkCommand = new RelayCommand(PerformAction);
        }

        static void PerformAction(object o)
        {

            var actionableErrorInfo = o as IActionableErrorInfo;
            if(actionableErrorInfo != null)
            {
                actionableErrorInfo.Do();
            }
        }

        public ICommand OpenHyperlinkCommand
        {
            get
            {
                return _openHyperlinkCommand;
            }
        }

        public string HelpText
        {
            get
            {
                return _helpText;
            }
            set
            {
                if(_helpText == value)
                {
                    return;
                }

                _helpText = value;
                NotifyOfPropertyChange(() => HelpText);
                NotifyOfPropertyChange(() => DisplayErrors);
                NotifyOfPropertyChange(() => DisplayHelpText);
            }
        }

        public List<IActionableErrorInfo> Errors
        {
            get
            {
                return _errors;
            }
            set
            {
                _errors = value;
                NotifyOfPropertyChange(() => Errors);
                NotifyOfPropertyChange(() => DisplayErrors);
                NotifyOfPropertyChange(() => DisplayHelpText);
            }
        }

        public Visibility DisplayHelpText
        {
            get
            {
                return !HasErrors() ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility DisplayErrors
        {
            get
            {
                return HasErrors() ? Visibility.Visible : Visibility.Hidden;
            }
        }

        bool HasErrors()
        {
            return Errors != null && Errors.Count > 0;
        }

        public override object GetView(object context = null)
        {
            return new HelpView();
        }

        public void ResetProperties()
        {
            this.Errors = new List<IActionableErrorInfo>();
            this.HelpText = string.Empty;
        }
    }
}
