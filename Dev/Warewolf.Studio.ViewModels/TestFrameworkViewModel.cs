
using System;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class TestFrameworkViewModel : BindableBase
    {

        private bool _canCreateTest;
        private bool _canStopTest;
        private ITestFrameworkModel _testFrameworkModel;

        public TestFrameworkViewModel(IResourceModel resourceModel)
            : this()
        {
            if(resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            ResourceModel = resourceModel;
        }

        public IResourceModel ResourceModel { get; set; }

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



        public ICommand RenameCommand { get; set; }
        public ICommand SaveCommand { get; set; }
        public ICommand EnableTestCommand { get; set; }
        public ICommand DisableTestCommand { get; set; }
        public ICommand DeleteTestCommand { get; set; }
        public ICommand DuplicateTestCommand { get; set; }
        public ICommand RunTestCommand { get; set; }
        public ICommand RunAllTestsInBrowserCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand RunSelectedTestInBrowserCommand { get; set; }
        public ICommand RunSelectedTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }

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
        public ICommand WebRunTestCommand { get; set; }
    }
}
