
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Dev2.Common.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class TestViewModel : BindableBase, ITestViewModel
    {

        
        private ITestModel _selectedTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<ITestModel> _tests;

        public TestViewModel(IResourceModel resourceModel)
        {
            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            ResourceModel = resourceModel;
            TestCommandHandler = new TestCommandHandlerModel();

            SaveCommand = new DelegateCommand(TestCommandHandler.Save, () => CanSave);
            DeleteTestCommand = new DelegateCommand(TestCommandHandler.DeleteTest, () => CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(TestCommandHandler.DuplicateTest, () => CanDuplicateTest);
            RunAllTestsInBrowserCommand = new DelegateCommand(TestCommandHandler.RunAllTestsInBrowser, () => CanRunAllTestsInBrowser);
            RunAllTestsCommand = new DelegateCommand(TestCommandHandler.RunAllTestsCommand, () => CanRunAllTestsCommand);
            RunSelectedTestInBrowserCommand = new DelegateCommand(TestCommandHandler.RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(TestCommandHandler.RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(TestCommandHandler.StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(() =>
            {
                var testModel = TestCommandHandler.CreateTest(ResourceModel);
                if (Tests == null)
                {
                    Tests = new ObservableCollection<ITestModel>();
                }
                Tests.Add(testModel);
                SelectedTest = testModel;

            }, () => CanCreateTest);
        }

        public bool CanCreateTest { get; set; }
        public bool CanStopTest { get; set; }
        public bool CanRunAllTestsInBrowser { get; set; }
        public bool CanRunAllTestsCommand { get; set; }
        public bool CanRunSelectedTestInBrowser { get; set; }
        public bool CanRunSelectedTest { get; set; }
        public bool CanDuplicateTest { get; set; }
        public bool CanDeleteTest { get; set; }
        public bool CanSave { get; set; }

        public IResourceModel ResourceModel { get; set; }

        public ITestModel SelectedTest
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

        public ITestCommandHandler TestCommandHandler { get; set; }

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

        public ObservableCollection<ITestModel> Tests
        {
            get { return _tests; }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        public ICommand SaveCommand { get; set; }
        public ICommand DeleteTestCommand { get; set; }
        public ICommand DuplicateTestCommand { get; set; }
        public ICommand RunAllTestsInBrowserCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand RunSelectedTestInBrowserCommand { get; set; }
        public ICommand RunSelectedTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }
    }
}
