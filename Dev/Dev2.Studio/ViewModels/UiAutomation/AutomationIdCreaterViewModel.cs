using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Studio.Core.ViewModels.Base;
using System.Windows.Input;

// ReSharper disable CheckNamespace
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
                return _OkCommand ?? (_OkCommand = new DelegateCommand(param => SaveID()));
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return _CancelCommand ?? (_CancelCommand = new DelegateCommand(param => Cancel()));
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
