using Dev2.Studio.Core.ViewModels.Base;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.ViewModels.UiAutomation
{
    public class AutomationIdCreaterViewModel : SimpleBaseViewModel
    {
        #region Fields

        // ReSharper disable InconsistentNaming
        public ICommand _OkCommand;
        public ICommand _CancelCommand;
        // ReSharper restore InconsistentNaming

        #endregion Fields

        #region Properties

        public string AutomationID { get; set; }

        #endregion Properties

        #region Methods

        public ICommand OkCommand
        {
            get
            {
                if(_OkCommand == null)
                {
                    _OkCommand = new RelayCommand(param => { SaveID(); });
                }
                return _OkCommand;
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                if(_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(param => { Cancel(); });
                }
                return _CancelCommand;
            }
        }

        public void SaveID()
        {
            RequestClose(ViewModelDialogResults.Okay);
        }

        public void Cancel()
        {
            RequestClose(ViewModelDialogResults.Cancel);
        }

        #endregion Methods
    }
}
