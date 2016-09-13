
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Resource.Errors;

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestViewModel : BindableBase, IServiceTestViewModel
    {
        private IServiceTestModel _selectedServiceTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<IServiceTestModel> _tests;
        private string _displayName;
        public IPopupController PopupController { get; }
        private bool _canSave;
        private string _errorMessage;
        private IShellViewModel _shellViewModel;

        public ServiceTestViewModel(IContextualResourceModel resourceModel, IAsyncWorker asyncWorker)
        {
            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            AsyncWorker = asyncWorker;
            ResourceModel = resourceModel;
            DisplayName = resourceModel.DisplayName + " - Tests";
            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            PopupController = CustomContainer.Get<IPopupController>();
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowser);
            RunAllTestsCommand = new DelegateCommand(RunAllTests);
            RunSelectedTestInBrowserCommand = new DelegateCommand(RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(CreateTests);
            DeleteTestCommand = new DelegateCommand<IServiceTestModel>(DeleteTest, CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(DuplicateTest, () => CanDuplicateTest);
            CanSave = true;
            RunAllTestsUrl = WebServer.GetWorkflowUri(resourceModel, "", UrlType.Tests)?.ToString();
            IsLoading = true;
            AsyncWorker.Start(GetTests, models =>
            {
                var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                models.Add(dummyTest);
                SelectedServiceTest = dummyTest;
                Tests = models;
                IsLoading = false;
            });
        }

        private void StopTest()
        {
            SelectedServiceTest.IsTestRunning = false;
            ServiceTestCommandHandler.StopTest();
        }

        #region CommandMethods

        private void RunSelectedTestInBrowser()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            ServiceTestCommandHandler.RunSelectedTestInBrowser();
        }

        private void RunSelectedTest()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            SelectedServiceTest.IsTestRunning = true;
            ServiceTestCommandHandler.RunSelectedTest();
        }

        private void RunAllTestsInBrowser()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty);
        }

        private void RunAllTests()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            ServiceTestCommandHandler.RunAllTestsCommand(IsDirty);
        }

        private void DuplicateTest()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            var duplicateTest = ServiceTestCommandHandler.DuplicateTest(SelectedServiceTest);
            AddAndSelectTest(duplicateTest);
        }

        #endregion

        private bool CanDeleteTest(IServiceTestModel selectedTestModel)
        {
            return GetPermissions() && selectedTestModel != null && !selectedTestModel.Enabled;
        }
        public bool IsLoading { get; set; }

        public IAsyncWorker AsyncWorker { get; set; }

        private void CreateTests()
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID,ResourceModel.ServerID,ResourceModel.Environment.ID);
                return;
            }
            if (IsDirty)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestSaveEditedTestsMessage, Resources.Languages.Core.ServiceTestSaveEditedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var testNumber = RealTests().Count() + 1;
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, testNumber);
            AddAndSelectTest(testModel);
        }

        private void AddAndSelectTest(IServiceTestModel testModel)
        {
            var index = _tests.Count - 1;
            if (index >= 0)
            {
                _tests.Insert(index, testModel);
            }
            else
            {
                _tests.Add(testModel);
            }
            SelectedServiceTest = testModel;
            SetSelectedTestUrl();
        }

        private bool CanStopTest { get; set; }
        private bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty;
        private bool CanRunSelectedTest => GetPermissions();
        private bool CanDuplicateTest => GetPermissions() && SelectedServiceTest != null;

        public bool CanSave
        {
            get
            {
                _canSave = IsDirty && IsValidName(SelectedServiceTest.TestName);
                return _canSave;
            }
            set
            {
                //_canSave = value;
            }
        }

        private bool GetPermissions()
        {
            return true;
        }

        private bool IsValidName(string name)
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrEmpty(name))
            {
                ErrorMessage = string.Format(ErrorResource.CannotBeNull, "'name'");
            }
            else if (NameHasInvalidCharacters(name))
            {
                ErrorMessage = string.Format(ErrorResource.ContainsInvalidCharecters, "'name'");
            }
            else if (name.Trim() != name)
            {
                ErrorMessage = string.Format(ErrorResource.ContainsLeadingOrTrailingWhitespace, "'name'");
            }

            return string.IsNullOrEmpty(ErrorMessage);
        }
        private static bool NameHasInvalidCharacters(string name)
        {
            return Regex.IsMatch(name, @"[^a-zA-Z0-9._\s-]");
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            set
            {
                _errorMessage = value;
                OnPropertyChanged(() => ErrorMessage);
            }
        }

        public bool IsDirty
        {
            get
            {
                try
                {
                    if (_tests == null || _tests.Count <= 1)
                    {
                        return false;
                    }
                    var isDirty = _tests.Any(resource => resource.IsDirty) || _tests.Any(resource => resource.NewTest);

                    var isConnected = ResourceModel.Environment.Connection.IsConnected;

                    return isDirty && isConnected;
                }
                // ReSharper disable once UnusedVariable
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
            try
            {
                if (!ValidateIfResourceExists())
                {
                    _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                    return;
                }
                if (HasDuplicateTestNames())
                {
                    return;
                }

                var serviceTestModels = RealTests().Where(a => a.IsDirty).ToList();
                var serviceTestModelTos = serviceTestModels.Select(model => new ServiceTestModelTO()
                {
                    TestName = model.TestName,
                    ResourceId = model.ParentId,
                    AuthenticationType = model.AuthenticationType,
                    Enabled = model.Enabled,
                    ErrorExpected = model.ErrorExpected,
                    NoErrorExpected = model.NoErrorExpected,
                    Inputs = model.Inputs,
                    LastRunDate = model.LastRunDate,
                    OldTestName = model.OldTestName,
                    Outputs = model.Outputs,
                    Password = model.Password,
                    IsDirty = model.IsDirty,
                    TestPending = model.TestPending,
                    UserName = model.UserName,
                    TestFailing = model.TestFailing,
                    TestInvalid = model.TestInvalid,
                    TestPassed = model.TestPassed
                } as IServiceTestModelTO).ToList();
                ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel.ID, serviceTestModelTos);
                MarkTestsAsNotNew();
                SetSelectedTestUrl();
            }
            catch (Exception)
            {
                // MarkTestsAsDirty(true);
            }
        }

        private bool ValidateIfResourceExists()
        {
            var contextualResourceModel = ResourceModel.Environment.ResourceRepository.LoadResourceFromWorkspace(ResourceModel.ID,GlobalConstants.ServerWorkspaceID);
            if (contextualResourceModel == null)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage,
                    Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null,
                    false, true, false, false);
                return false;
            }
            return true;
        }

        private bool HasDuplicateTestNames()
        {
            var dupTests = RealTests().ToList().GroupBy(x => x.TestName).Where(group => @group.Count() > 1).Select(group => @group.Key);
            if (dupTests.Any())
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return true;
            }
            return false;
        }

        private void SetSelectedTestUrl()
        {
            SelectedServiceTest.RunSelectedTestUrl = WebServer.GetWorkflowUri(ResourceModel, "", UrlType.Tests) + "/" + SelectedServiceTest.TestName;
            if (SelectedServiceTest.AuthenticationType == AuthenticationType.Public)
            {
                SelectedServiceTest.RunSelectedTestUrl = SelectedServiceTest.RunSelectedTestUrl.Replace("/secure/", "/public/");
            }
        }

        private void MarkTestsAsNotNew()
        {
            foreach (var model in _tests.Where(model => model.NewTest))
            {
                model.NewTest = false;
            }
            foreach (var model in _tests.Where(model => model.IsDirty))
            {
                model.SetItem(model);
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
                    _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                    _selectedServiceTest = null;
                    OnPropertyChanged(() => SelectedServiceTest);
                    return;
                }
                if (Equals(_selectedServiceTest, value) || value.IsNewTest)
                {
                    return;
                }
                if (_selectedServiceTest != null)
                    _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                _selectedServiceTest = value;
                _selectedServiceTest.PropertyChanged += ActionsForPropChanges;
                SetSelectedTestUrl();
                OnPropertyChanged(() => SelectedServiceTest);
            }
        }

        private void ActionsForPropChanges(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
            }
            if (e.PropertyName == "RunSelectedTestUrl")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            }
            ViewModelUtils.RaiseCanExecuteChanged(DuplicateTestCommand);
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
        private IEnumerable<IServiceTestModel> RealTests() => _tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToObservableCollection();

        public ObservableCollection<IServiceTestModel> Tests
        {
            get
            {
                return _tests;
            }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        private void DeleteTest(IServiceTestModel test)
        {
            if (!ValidateIfResourceExists())
            {
                _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                return;
            }
            if (test == null) return;
            var nameOfItemBeingDeleted = test.NameForDisplay.Replace("*", "").TrimEnd(' ');
            if (PopupController.ShowDeleteConfirmation(nameOfItemBeingDeleted) == MessageBoxResult.Yes)
            {
                try
                {
                    ResourceModel.Environment.ResourceRepository.DeleteResourceTest(ResourceModel.ID, test.TestName);
                    var testToRemove = _tests.SingleOrDefault(model => model.ParentId == test.ParentId && model.TestName == test.TestName);
                    _tests.Remove(testToRemove);//test
                    OnPropertyChanged(() => Tests);//test
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("IServiceTestModelTO DeleteTest(IServiceTestModel model)", ex);
                    return;
                }
            }
            return;
        }

        private ObservableCollection<IServiceTestModel> GetTests()
        {
            try
            {

                var serviceTestModels = new List<ServiceTestModel>();
                var loadResourceTests = ResourceModel.Environment.ResourceRepository.LoadResourceTests(ResourceModel.ID);
                if (loadResourceTests != null)
                {
                    foreach (var to in loadResourceTests)
                    {
                        var serviceTestModel = ToServiceTestModel(to);
                        serviceTestModel.Item = ToServiceTestModel(to);
                        serviceTestModels.Add(serviceTestModel);
                    }
                }
                return serviceTestModels.ToObservableCollection<IServiceTestModel>();
            }
            catch (Exception)
            {
                return new ObservableCollection<IServiceTestModel>();
            }
        }

        private ServiceTestModel ToServiceTestModel(IServiceTestModelTO to)
        {
            var serviceTestModel = new ServiceTestModel(ResourceModel.ID)
            {
                OldTestName = to.TestName,
                TestName = to.TestName,
                NameForDisplay = to.TestName,
                UserName = to.UserName,
                AuthenticationType = to.AuthenticationType,
                Enabled = to.Enabled,
                ErrorExpected = to.ErrorExpected,
                NoErrorExpected = to.NoErrorExpected,
                LastRunDate = to.LastRunDate,
                TestPending = to.TestPending,
                TestFailing = to.TestFailing,
                TestPassed = to.TestPassed,
                Password = to.Password,
                ParentId = to.ResourceId,
                TestInvalid = to.TestInvalid,
                Inputs = to.Inputs?.Select(input => new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput).ToList(),
                Outputs = to.Outputs?.Select(output => new ServiceTestOutput(output.Variable, output.Value) as IServiceTestOutput).ToList()
            };
            return serviceTestModel;
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
                var displayName = _displayName.Replace("*", "").TrimEnd(' ');
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
