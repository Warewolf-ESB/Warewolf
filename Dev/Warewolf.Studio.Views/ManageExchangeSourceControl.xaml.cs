using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Studio.ViewModels;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ManageEmailSourceControl.xaml
    /// </summary>
    public partial class ManageExchangeSourceControl : IView, ICheckControlEnabledView
    {
        public ManageExchangeSourceControl()
        {
            InitializeComponent();
        }

        public void TestSend()
        {
            TestSendCommand.Command.Execute(null);
        }

        #region Implementation of ICheckControlEnabledView

        public bool GetControlEnabled(string controlName)
        {
            switch (controlName)
            {
                case "Send":
                    return TestSendCommand.Command.CanExecute(null);
                case "Save":
                    var viewModel = DataContext as ManageEmailSourceViewModel;
                    return viewModel != null && viewModel.OkCommand.CanExecute(null);
            }
            return false;
        }

        #endregion
    }
}
