
using Dev2.Common.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class TestFrameworkViewModel : BindableBase
    {

        private bool _canCreateTest;
        private bool _canStopTest;
        private ITestFrameworkModel _testFrameworkModel;

        public TestFrameworkViewModel(ITestFrameworkModel frameworkModel)
            : this()
        {
            TestFrameworkModel = frameworkModel;
        }

        private TestFrameworkViewModel()
        {
            RenameCommand = new DelegateCommand(RenameAction, () => CanRename);
            SaveCommand = new DelegateCommand(SaveAction, () => CanSave);
            EnableTestCommand = new DelegateCommand(EnableTestAction, () => CanEnableTest);
            DisableTestCommand = new DelegateCommand(DisableTestAction, () => CanDisableTest);
            DeleteTestCommand = new DelegateCommand(DeleteTestAction, () => CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(DuplicateTestAction, () => CanDuplicateTest);
            RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowserAction, () => CanRunAllTestsInBrowser);
            RunAllTestsCommand = new DelegateCommand(RunAllTestsCommandAction, () => CanRunAllTestsCommand);
            RunSelectedTestInBrowserCommand = new DelegateCommand(RunSelectedTestInBrowserAction, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(RunSelectedTestAction, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(StopTestAction, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(CreateTestAction, () => CanCreateTest);
        }

        public bool CanCreateTest
        {
            get
            {
                _canCreateTest = true;
                return _canCreateTest;
            }
            set
            {
                _canCreateTest = value;
                OnPropertyChanged(() => CanCreateTest);

            }
        }

        private void CreateTestAction()
        {
        }

        public bool CanStopTest
        {
            get
            {
                return _canStopTest;
            }
            set
            {
                _canStopTest = value;
                OnPropertyChanged(() => CanStopTest);
            }
        }

        private void StopTestAction()
        {
        }

        public bool CanRunAllTestsInBrowser { get; set; }

        private void RunAllTestsInBrowserAction()
        {
        }

        public bool CanRunAllTestsCommand { get; set; }

        private void RunAllTestsCommandAction()
        {
        }
        public bool CanRunSelectedTestInBrowser { get; set; }

        private void RunSelectedTestInBrowserAction()
        {
        }

        public bool CanRunSelectedTest { get; set; }

        private void RunSelectedTestAction()
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
        public DelegateCommand RunAllTestsInBrowserCommand { get; set; }
        public DelegateCommand RunAllTestsCommand { get; set; }
        public DelegateCommand RunSelectedTestInBrowserCommand { get; set; }
        public DelegateCommand RunSelectedTestCommand { get; set; }
        public DelegateCommand StopTestCommand { get; set; }
        public DelegateCommand CreateTestCommand { get; set; }

        #endregion

        public ITestFrameworkModel TestFrameworkModel
        {
            get
            {
                return _testFrameworkModel;
            }
            set
            {
                _testFrameworkModel = value;
                OnPropertyChanged(() => TestFrameworkModel);
            }
        }

    }
}
