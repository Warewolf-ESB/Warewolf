
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class TestFrameworkViewModel : BindableBase, ITestFrameworkViewModel
    {

        private bool _canCreateTest;
        private bool _canStopTest;
        private ITestFrameworkModel _selectedTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<ITestFrameworkModel> _testsList;

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
            TestFrameworkCommandHandler.RenameCommand = new DelegateCommand(RenameAction, () => CanRename);
            TestFrameworkCommandHandler.SaveCommand = new DelegateCommand(SaveAction, () => CanSave);
            TestFrameworkCommandHandler.EnableTestCommand = new DelegateCommand(EnableTestAction, () => CanEnableTest);
            TestFrameworkCommandHandler.DisableTestCommand = new DelegateCommand(DisableTestAction, () => CanDisableTest);
            TestFrameworkCommandHandler.DeleteTestCommand = new DelegateCommand(DeleteTestAction, () => CanDeleteTest);
            TestFrameworkCommandHandler.DuplicateTestCommand = new DelegateCommand(DuplicateTestAction, () => CanDuplicateTest);
            TestFrameworkCommandHandler.RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowserAction, () => CanRunAllTestsInBrowser);
            TestFrameworkCommandHandler.RunAllTestsCommand = new DelegateCommand(RunAllTestsCommandAction, () => CanRunAllTestsCommand);
            TestFrameworkCommandHandler.RunSelectedTestInBrowserCommand = new DelegateCommand(RunSelectedTestInBrowserAction, () => CanRunSelectedTestInBrowser);
            TestFrameworkCommandHandler.RunSelectedTestCommand = new DelegateCommand(RunSelectedTestAction, () => CanRunSelectedTest);
            TestFrameworkCommandHandler.StopTestCommand = new DelegateCommand(StopTestAction, () => CanStopTest);
            TestFrameworkCommandHandler.CreateTestCommand = new DelegateCommand(CreateTestAction, () => CanCreateTest);
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

        #endregion

        public ITestFrameworkModel SelectedTest
        {
            get
            {
                return _selectedTest;
            }
            set
            {
                _selectedTest = value;
                OnPropertyChanged(() => SelectedTest);
            }
        }

        public ITestFrameworkCommandHandler TestFrameworkCommandHandler { get; set; }

        public string RunAllTestsUrl
        {
            get { return _runAllTestsUrl; }
            set
            {
                _runAllTestsUrl = value;
                OnPropertyChanged(() => RunAllTestsUrl);
            }
        }

        public string TestPassingResult
        {
            get { return _testPassingResult; }
            set
            {
                _testPassingResult = value; 
                OnPropertyChanged(() => TestPassingResult);
            }
        }

        public ObservableCollection<ITestFrameworkModel> TestsList
        {
            get { return _testsList; }
            set
            {
                _testsList = value; 
                OnPropertyChanged(() => TestsList);
            }
        }
    }
}
