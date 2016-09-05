
using Microsoft.Practices.Prism.Commands;

namespace Warewolf.Studio.ViewModels
{
    public class TestFrameworkViewModel
    {
        public TestFrameworkViewModel()
        {
            RenameCommand = new DelegateCommand(RenameAction, () => CanRename);
            SaveCommand = new DelegateCommand(SaveAction, () => CanSave);
            EnableTestCommand = new DelegateCommand(EnableTestAction, () => CanEnableTest);
            DisableTestCommand = new DelegateCommand(DisableTestAction, () => CanDisableTest);
            DeleteTestCommand = new DelegateCommand(DeleteTestAction, () => CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(DuplicateTestAction, () => CanDuplicateTest);
            RunTestCommand = new DelegateCommand(RunTestAction, () => CanRunTest);
            WebRunTestCommand = new DelegateCommand(WebRunTestAction, () => CanWebRunTest);
            StopTestCommand = new DelegateCommand(StopTestAction, () => CanStopTest);
        }

        public bool CanStopTest { get; set; }

        private void StopTestAction()
        {
        }

        public bool CanWebRunTest { get; set; }

        private void WebRunTestAction()
        {
        }

        public bool CanRunTest { get; set; }

        private void RunTestAction()
        {
        }

        public bool CanDuplicateTest { get; set; }

        private void DuplicateTestAction()
        {
        }

        public bool CanDeleteTest { get; set; }

        private void DeleteTestAction()
        {
        }

        #region Commands

        public bool CanDisableTest { get; set; }

        private void DisableTestAction()
        {
        }

        public bool CanEnableTest { get; set; }

        private void EnableTestAction()
        {
        }

        public bool CanSave { get; set; }

        private void SaveAction()
        {
        }

        public bool CanRename { get; set; }

        private void RenameAction()
        {
        }



        public DelegateCommand RenameCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand EnableTestCommand { get; set; }
        public DelegateCommand DisableTestCommand { get; set; }
        public DelegateCommand DeleteTestCommand { get; set; }
        public DelegateCommand DuplicateTestCommand { get; set; }
        public DelegateCommand RunTestCommand { get; set; }
        public DelegateCommand WebRunTestCommand { get; set; }
        public DelegateCommand StopTestCommand { get; set; }

        #endregion

    }
}
