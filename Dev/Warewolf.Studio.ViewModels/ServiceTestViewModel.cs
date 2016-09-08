
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestViewModel : BindableBase, IServiceTestViewModel
    {
        private IServiceTestModel _selectedServiceTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<IServiceTestModel> _tests;
        private string _displayName;
        private bool _isDirty;

        public ServiceTestViewModel(IContextualResourceModel resourceModel)
        {

            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            ResourceModel = resourceModel;
            DisplayName = resourceModel.DisplayName + " - Tests";
            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();

            DeleteTestCommand = new DelegateCommand(ServiceTestCommandHandler.DeleteTest, () => CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(ServiceTestCommandHandler.DuplicateTest, () => CanDuplicateTest);
            RunAllTestsInBrowserCommand = new DelegateCommand(ServiceTestCommandHandler.RunAllTestsInBrowser, () => CanRunAllTestsInBrowser);
            RunAllTestsCommand = new DelegateCommand(ServiceTestCommandHandler.RunAllTestsCommand, () => CanRunAllTestsCommand);
            RunSelectedTestInBrowserCommand = new DelegateCommand(ServiceTestCommandHandler.RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(ServiceTestCommandHandler.RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(ServiceTestCommandHandler.StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(CreateTests);
            CanSave = true;
        }

        private void CreateTests()
        {
            if (IsDirty)
            {
                var popupController = CustomContainer.Get<Dev2.Common.Interfaces.Studio.Controller.IPopupController>();
                popupController?.Show(@"Please save currently edited Test(s) before creating a new one.", @"Save before continuing", MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel);
            var index = Tests.Count - 1;
            if (index >= 0)
            {
                Tests.Insert(index, testModel);
            }
            else
            {
                Tests.Add(testModel);
            }
            SelectedServiceTest = testModel;
        }

        public bool CanStopTest { get; set; }
        public bool CanRunAllTestsInBrowser { get; set; }
        public bool CanRunAllTestsCommand { get; set; }
        public bool CanRunSelectedTestInBrowser { get; set; }
        private bool CanRunSelectedTest => GetPermissions();
        public bool CanDuplicateTest => GetPermissions();
        private bool CanDeleteTest => GetPermissions();
        public bool CanSave { get; set; }

        private bool GetPermissions()
        {
            return true;
        }

        public bool IsDirty
        {
            get
            {
                try
                {
                    if (Tests == null || Tests.Count == 0)
                    {
                        return false;
                    }
                    var isDirty = Tests.Any(resource => resource.IsDirty);

                    var isConnected = ResourceModel.Environment.Connection.IsConnected;

                    return isDirty && isConnected;
                }
                catch (Exception ex)
                {
                    //if (!_errorShown)
                    //{
                    //    _popupController.ShowCorruptTaskResult(ex.Message);
                    //    Dev2Logger.Error(ex);
                    //    _errorShown = true;
                    //}
                }
                return false;
            }
        }

        public void Save()
        {
            var serviceTestModels = Tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToList();
            var executeMessage = ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel.ID, serviceTestModels);
            foreach(var model in Tests)
            {
                model.IsDirty = false;
            }
        }

      

        public IContextualResourceModel ResourceModel { get; }

        public IServiceTestModel SelectedServiceTest
        {
            get
            {
                return _selectedServiceTest;
            }
            set
            {
                if (value == null)
                {
                    _selectedServiceTest = null;
                    OnPropertyChanged(() => SelectedServiceTest);
                    return;
                }
                if (Equals(_selectedServiceTest, value) || value.IsNewTest)
                {
                    return;
                }
                _selectedServiceTest = value;

                OnPropertyChanged(() => SelectedServiceTest);
            }
        }

        public IServiceTestCommandHandler ServiceTestCommandHandler { get; set; }

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

        public ObservableCollection<IServiceTestModel> Tests
        {
            get
            {
                if (_tests == null)
                {
                    _tests = GetTests();
                    var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                    _tests.Add(dummyTest);
                    SelectedServiceTest = dummyTest;
                }
                return _tests;

            }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        private ObservableCollection<IServiceTestModel> GetTests()
        {
            return new ObservableCollection<IServiceTestModel>();
        }

        public ICommand DeleteTestCommand { get; set; }
        public ICommand DuplicateTestCommand { get; set; }
        public ICommand RunAllTestsInBrowserCommand { get; set; }
        public ICommand RunAllTestsCommand { get; set; }
        public ICommand RunSelectedTestInBrowserCommand { get; set; }
        public ICommand RunSelectedTestCommand { get; set; }
        public ICommand StopTestCommand { get; set; }
        public ICommand CreateTestCommand { get; set; }

        public string DisplayName
        {
            get
            {
                if (IsDirty)
                {
                    if (!_displayName.EndsWith(" *"))
                    {
                        _displayName += " *";
                    }
                    return _displayName;
                }
                var displayName = _displayName.Replace("*","").TrimEnd(' ');
                return displayName;
            }
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public void Dispose()
        {
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
