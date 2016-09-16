
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Data;
using Dev2.Data.ServiceModel.Messages;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Network;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Resource.Errors;
// ReSharper disable ParameterTypeCanBeEnumerable.Local

namespace Warewolf.Studio.ViewModels
{
    public class ServiceTestViewModel : BindableBase, IServiceTestViewModel
    {
        private readonly IExternalProcessExecutor _processExecutor;
        private IServiceTestModel _selectedServiceTest;
        private string _runAllTestsUrl;
        private string _testPassingResult;
        private ObservableCollection<IServiceTestModel> _tests;
        private string _displayName;
        public IPopupController PopupController { get; }
        private bool _canSave;
        private string _errorMessage;
        private readonly IShellViewModel _shellViewModel;
        private IContextualResourceModel _resourceModel;

        public ServiceTestViewModel(IContextualResourceModel resourceModel, IAsyncWorker asyncWorker, IEventAggregator eventPublisher,IExternalProcessExecutor processExecutor)
        {
            
            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            _processExecutor = processExecutor;
            AsyncWorker = asyncWorker;
            EventPublisher = eventPublisher;
            ResourceModel = resourceModel;
            ResourceModel.Environment.IsConnectedChanged += (sender, args) =>
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
                RefreshCommands();
            };
            ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage += OnReceivedResourceAffectedMessage;
            DisplayName = resourceModel.DisplayName + " - Tests";
            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            PopupController = CustomContainer.Get<IPopupController>();
            _shellViewModel = CustomContainer.Get<IShellViewModel>();
            RunAllTestsInBrowserCommand = new DelegateCommand(RunAllTestsInBrowser, IsServerConnected);
            RunAllTestsCommand = new DelegateCommand(RunAllTests, IsServerConnected);
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

        private void OnReceivedResourceAffectedMessage(Guid resourceId, CompileMessageList changeList)
        {
            AsyncWorker.Start(()=>
            {
                var contextModel = ResourceModel.Environment.ResourceRepository.LoadContextualResourceModel(resourceId);
                _resourceModel = contextModel;
                return GetTests();
            }, models =>
            {
                var dummyTest = new DummyServiceTest(CreateTests) { TestName = "Create a new test." };
                models.Add(dummyTest);
                var testName = SelectedServiceTest.TestName;
                SelectedServiceTest = dummyTest;
                _tests = models;
                OnPropertyChanged(()=>Tests);
                SelectedServiceTest = _tests.FirstOrDefault(model => model.TestName == testName);
                IsLoading = false;
            });
        }

        private bool IsServerConnected()
        {
            var isConnected = ResourceModel.Environment.IsConnected;
            return isConnected;
        }

        private void StopTest()
        {
            SelectedServiceTest.IsTestRunning = false;
            SelectedServiceTest.TestPending = true;
            ServiceTestCommandHandler.StopTest(ResourceModel);
        }

        #region CommandMethods

        private void RunSelectedTestInBrowser()
        {
            ServiceTestCommandHandler.RunSelectedTestInBrowser(SelectedServiceTest.RunSelectedTestUrl, _processExecutor);
        }

        private void RunSelectedTest()
        {
            if (SelectedServiceTest.IsDirty)
            {
                Save(new List<IServiceTestModel> {SelectedServiceTest});
            }
            ServiceTestCommandHandler.RunSelectedTest(SelectedServiceTest, ResourceModel, AsyncWorker);
            ViewModelUtils.RaiseCanExecuteChanged(StopTestCommand);
        }

        private void RunAllTestsInBrowser()
        {
            ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty,RealTests(),_processExecutor);
        }

        private void RunAllTests()
        {
            ServiceTestCommandHandler.RunAllTestsCommand(IsDirty, RealTests(), ResourceModel,AsyncWorker);
            SelectedServiceTest = null;
        }

        private void DuplicateTest()
        {
            var testNumber = GetNewTestNumber(SelectedServiceTest.TestName);
            var duplicateTest = ServiceTestCommandHandler.DuplicateTest(SelectedServiceTest, testNumber);
            AddAndSelectTest(duplicateTest);
        }

        #endregion


        private bool CanDeleteTest(IServiceTestModel selectedTestModel)
        {
            return GetPermissions() && selectedTestModel != null && !selectedTestModel.Enabled && IsServerConnected();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool IsLoading { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public IAsyncWorker AsyncWorker { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public IEventAggregator EventPublisher { get; set; }

        private void CreateTests()
        {
            SelectedServiceTest = null;
            if (IsDirty)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestSaveEditedTestsMessage, Resources.Languages.Core.ServiceTestSaveEditedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var testNumber = GetNewTestNumber("Test");
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, testNumber);
            AddAndSelectTest(testModel);
        }

        private int GetNewTestNumber(string testName)
        {
            int counter = 1;
            string fullName = testName + " " + counter;

            while (Contains(fullName))
            {
                counter++;
                fullName = testName + " " + counter;
            }

            return counter;
        }

        private bool Contains(string nameToCheck)
        {
            var serviceTestModel = RealTests().FirstOrDefault(a => a.TestName.Contains(nameToCheck));
            return serviceTestModel != null;
        }

        private void SetDuplicateTestTooltip()
        {
            if (SelectedServiceTest != null)
            {
                if (SelectedServiceTest.NewTest)
                {
                    SelectedServiceTest.DuplicateTestTooltip = Resources.Languages.Core.ServiceTestNewTestDisabledDuplicateSelectedTestTooltip;
                }
                else
                {
                    SelectedServiceTest.DuplicateTestTooltip = CanDuplicateTest ? Resources.Languages.Core.ServiceTestDuplicateSelectedTestTooltip : Resources.Languages.Core.ServiceTestDisabledDuplicateSelectedTestTooltip;
                }
            }
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
            SetDuplicateTestTooltip();
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool CanStopTest => SelectedServiceTest != null && SelectedServiceTest.IsTestRunning;
        private bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty && IsServerConnected();
        private bool CanRunSelectedTest => GetPermissions() && IsServerConnected();
        private bool CanDuplicateTest => GetPermissions() && SelectedServiceTest != null && !SelectedServiceTest.NewTest;

        public bool CanSave
        {
            get
            {
                var isValid = true;
                if (SelectedServiceTest != null)
                {
                    isValid = IsValidName(SelectedServiceTest.TestName);
                }
                _canSave = IsDirty && isValid;
                SetDisplayName();
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
                catch (Exception)
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
                if (ShowPoputWhenDuplicates())
                {
                    return;
                }

                var serviceTestModels = RealTests().Where(a => a.IsDirty).ToList();
                Save(serviceTestModels);
            }
            catch (Exception)
            {
                // MarkTestsAsDirty(true);
            }
        }

        private void Save(List<IServiceTestModel> serviceTestModels)
        {
            var serviceTestModelTos = serviceTestModels.Select(model => new ServiceTestModelTO
            {
                TestName = model.TestName,
                ResourceId = model.ParentId,
                AuthenticationType = model.AuthenticationType,
                Enabled = model.Enabled,
                ErrorExpected = model.ErrorExpected,
                NoErrorExpected = model.NoErrorExpected,
                Inputs = model.Inputs.ToList(),
                LastRunDate = model.LastRunDate,
                OldTestName = model.OldTestName,
                Outputs = model.Outputs.ToList(),
                Password = model.Password,
                IsDirty = model.IsDirty,
                TestPending = model.TestPending,
                UserName = model.UserName,
                TestFailing = model.TestFailing,
                TestInvalid = model.TestInvalid,
                TestPassed = model.TestPassed
            } as IServiceTestModelTO).ToList();
            var result = ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel, serviceTestModelTos);
            switch(result.Result)
            {
                case SaveResult.Success:
                    MarkTestsAsNotNew();
                    SetSelectedTestUrl();
                    break;
                case SaveResult.ResourceDeleted:
                    PopupController?.Show(Resources.Languages.Core.ServiceTestResourceDeletedMessage, Resources.Languages.Core.ServiceTestResourceDeletedHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                    _shellViewModel.CloseResourceTestView(ResourceModel.ID, ResourceModel.ServerID, ResourceModel.Environment.ID);
                    break;
                case SaveResult.ResourceUpdated:
                    UpdateTestsFromResourceUpdate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateTestsFromResourceUpdate()
        {
            //ResourceModel = ResourceModel.Environment.ResourceRepository.LoadResourceFromWorkspace(ResourceModel.ID,GlobalConstants.ServerWorkspaceID);

        }

        private bool ShowPoputWhenDuplicates()
        {
            if (HasDuplicates())
            {
                ShowDuplicatePopup();
                return true;
            }
            return false;
        }

        public void ShowDuplicatePopup()
        {
            PopupController?.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
        }

        public void RefreshCommands()
        {
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunAllTestsInBrowserCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestCommand);
            ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            OnPropertyChanged(() => DisplayName);
        }

        public bool HasDuplicates() => RealTests().ToList().GroupBy(x => x.TestName).Where(group => @group.Count() > 1).Select(group => @group.Key).Any();

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
            foreach (var model in RealTests())
            {
                var clone = model.Clone() as IServiceTestModel;
                model.SetItem(clone);
            }
            
        }

        public IContextualResourceModel ResourceModel
        {
            get
            {
                return _resourceModel;
            }
            set
            {
                _resourceModel = value;
            }
        }

        public IServiceTestModel SelectedServiceTest
        {
            get { return _selectedServiceTest; }
            set
            {
                if (value == null)
                {
                    if (_selectedServiceTest != null)
                    {
                        _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                    }
                    
                    _selectedServiceTest = null;
                    EventPublisher.Publish(new DebugOutputMessage(new List<IDebugState>()));
                    OnPropertyChanged(() => SelectedServiceTest);
                    return;
                }
                if (Equals(_selectedServiceTest, value) || value.IsNewTest)
                {
                    return;
                }
                if (_selectedServiceTest != null)
                {
                    _selectedServiceTest.PropertyChanged -= ActionsForPropChanges;
                }
                _selectedServiceTest = value;
                _selectedServiceTest.PropertyChanged += ActionsForPropChanges;
                SetSelectedTestUrl();
                SetDuplicateTestTooltip();
                OnPropertyChanged(() => SelectedServiceTest);
                EventPublisher.Publish(new DebugOutputMessage(_selectedServiceTest.DebugForTest ?? new List<IDebugState>()));
            }
        }

      

        private void ActionsForPropChanges(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Enabled")
            {
                ViewModelUtils.RaiseCanExecuteChanged(DeleteTestCommand);
            }
            if (e.PropertyName == "IsDirty")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
                SetDisplayName();
            }
            if (e.PropertyName == "Inputs" || e.PropertyName == "Outputs")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            }
            if (e.PropertyName == "RunSelectedTestUrl")
            {
                ViewModelUtils.RaiseCanExecuteChanged(RunSelectedTestInBrowserCommand);
            }
            if (e.PropertyName == "DebugForTest")
            {
                EventPublisher.Publish(new DebugOutputMessage(SelectedServiceTest.DebugForTest ?? new List<IDebugState>()));
            }
            ViewModelUtils.RaiseCanExecuteChanged(DuplicateTestCommand);
        }

        private void SetDisplayName()
        {
            if (IsDirty)
            {
                if (!DisplayName.EndsWith(" *"))
                {
                    DisplayName += " *";
                }
            }
            else
            {
                DisplayName = _displayName.Replace("*", "").TrimEnd(' ');
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

        private IEnumerable<IServiceTestModel> RealTests() => _tests.Where(model => model.GetType() != typeof(DummyServiceTest)).ToObservableCollection();

        public ObservableCollection<IServiceTestModel> Tests
        {
            get { return _tests; }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
            }
        }

        private void DeleteTest(IServiceTestModel test)
        {
            if (test == null) return;
            var nameOfItemBeingDeleted = test.NameForDisplay.Replace("*", "").TrimEnd(' ');
            if (PopupController.ShowDeleteConfirmation(nameOfItemBeingDeleted) == MessageBoxResult.Yes)
            {
                try
                {
                    ResourceModel.Environment.ResourceRepository.DeleteResourceTest(ResourceModel.ID, test.TestName);
                    var testToRemove = _tests.SingleOrDefault(model => model.ParentId == test.ParentId && model.TestName == SelectedServiceTest.TestName);
                    _tests.Remove(testToRemove); //test
                    OnPropertyChanged(() => Tests); //test
                    SelectedServiceTest = null;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("IServiceTestModelTO DeleteTest(IServiceTestModel model)", ex);
                }
            }
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
                IsTestRunning = false,
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
                Inputs = to.Inputs?.Select(input => new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput).ToObservableCollection(),
                Outputs = to.Outputs?.Select(output => new ServiceTestOutput(output.Variable, output.Value) as IServiceTestOutput).ToObservableCollection()
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
                return _displayName;
            }
            set
            {
                _displayName = value;
                OnPropertyChanged(() => DisplayName);
            }
        }

        public void Dispose()
        {
            // ReSharper disable DelegateSubtraction
            if(ResourceModel?.Environment?.Connection != null)
                ResourceModel.Environment.Connection.ReceivedResourceAffectedMessage -= OnReceivedResourceAffectedMessage;
        }

        public void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}
