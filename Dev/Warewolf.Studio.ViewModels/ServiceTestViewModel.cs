
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Dev2.Common.Interfaces.Threading;
using Dev2.Interfaces;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Network;
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
        public IPopupController PopupController { get; }
        private bool _canSave;

        public ServiceTestViewModel(IContextualResourceModel resourceModel,IAsyncWorker asyncWorker)
        {
            if (resourceModel == null)
                throw new ArgumentNullException(nameof(resourceModel));
            AsyncWorker = asyncWorker;
            ResourceModel = resourceModel;
            DisplayName = resourceModel.DisplayName + " - Tests";
            ServiceTestCommandHandler = new ServiceTestCommandHandlerModel();
            PopupController = CustomContainer.Get<IPopupController>();

            RunAllTestsInBrowserCommand = new DelegateCommand(() => ServiceTestCommandHandler.RunAllTestsInBrowser(IsDirty));
            RunAllTestsCommand = new DelegateCommand(() => ServiceTestCommandHandler.RunAllTestsCommand(IsDirty));
            RunSelectedTestInBrowserCommand = new DelegateCommand(ServiceTestCommandHandler.RunSelectedTestInBrowser, () => CanRunSelectedTestInBrowser);
            RunSelectedTestCommand = new DelegateCommand(ServiceTestCommandHandler.RunSelectedTest, () => CanRunSelectedTest);
            StopTestCommand = new DelegateCommand(ServiceTestCommandHandler.StopTest, () => CanStopTest);
            CreateTestCommand = new DelegateCommand(CreateTests);
            DeleteTestCommand = new DelegateCommand<IServiceTestModel>(selectedServiceTest =>
            {
                var serviceTestModel = DeleteTest(selectedServiceTest);
                var testToRemove = Tests.SingleOrDefault(model => model.ParentId == serviceTestModel?.ParentId && model.TestName == serviceTestModel.TestName);
                Tests.Remove(testToRemove);//test
                OnPropertyChanged(() => Tests);//test
            }, CanDeleteTest);
            DuplicateTestCommand = new DelegateCommand(() =>
            {
                var duplicateTest = ServiceTestCommandHandler.DuplicateTest(SelectedServiceTest);
                AddAndSelectTest(duplicateTest);
            }, () => CanDuplicateTest);
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

        private bool CanDeleteTest(IServiceTestModel selectedTestModel)
        {
          return GetPermissions() && selectedTestModel != null && !selectedTestModel.Enabled;
        } 
        public bool IsLoading { get; set; }

        public IAsyncWorker AsyncWorker { get; set; }

        private void CreateTests()
        {
            if (IsDirty)
            {
                PopupController?.Show(Resources.Languages.Core.ServiceTestSaveEditedTestsMessage, Resources.Languages.Core.ServiceTestSaveEditedTestsHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                return;
            }

            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel, RealTests().Count());
            AddAndSelectTest(testModel);
        }

        private void AddAndSelectTest(IServiceTestModel testModel)
        {
            var index = _tests.Count - 1;
            if(index >= 0)
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

        public bool CanStopTest { get; set; }
        private bool CanRunSelectedTestInBrowser => SelectedServiceTest != null && !SelectedServiceTest.IsDirty;
        private bool CanRunSelectedTest => GetPermissions();
        public bool CanDuplicateTest => GetPermissions() && !IsDirty && SelectedServiceTest != null;

        public bool CanSave
        {
            get
            {
                _canSave = IsDirty;
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
                    var isDirty = _tests.Any(resource => resource.IsDirty);

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
                var serviceTestModels = RealTests().Where(a => a.IsDirty).ToList();
                var duplicateTests = RealTests().ToList().GroupBy(x => x.TestName).Where(group => group.Count() > 1).Select(group => group.Key);
                if (duplicateTests.Any())
                {
                    PopupController?.Show(Resources.Languages.Core.ServiceTestDuplicateTestNameMessage, Resources.Languages.Core.ServiceTestDuplicateTestNameHeader, MessageBoxButton.OK, MessageBoxImage.Error, null, false, true, false, false);
                    return;
                }
                ResourceModel.Environment.ResourceRepository.SaveTests(ResourceModel.ID, serviceTestModels);
                MarkTestsAsDirty(false);
                SetSelectedTestUrl();
            }
            catch (Exception)
            {
                // MarkTestsAsDirty(true);
            }
        }

        private void SetSelectedTestUrl()
        {
            SelectedServiceTest.RunSelectedTestUrl = WebServer.GetWorkflowUri(ResourceModel, "", UrlType.Tests) + "/" +
                                                     SelectedServiceTest.TestName;
        }

        private void MarkTestsAsDirty(bool isDirty)
        {
            foreach (var model in _tests) //This is based on the fact that the save will do a bulk save all the time
            {
                model.IsDirty = isDirty;
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

        private IServiceTestModel DeleteTest(IServiceTestModel model)
        {
            if(model == null) return default(ServiceTestModel);
            var nameOfItemBeingDeleted = model.NameForDisplay.Replace("*", "").TrimEnd(' ');
            if (PopupController.ShowDeleteConfirmation(nameOfItemBeingDeleted) == MessageBoxResult.Yes)
            {
                try
                {
                    var serviceTestModelTO = ResourceModel.Environment.ResourceRepository.DeleteResourceTest(ResourceModel.ID, model.TestName);
                    IServiceTestModel serviceTestModel = new ServiceTestModel(ResourceModel.ID) { TestName = serviceTestModelTO.TestName };
                    return serviceTestModel;
                }
                catch (Exception ex)
                {
                    Dev2Logger.Error("IServiceTestModelTO DeleteTest(IServiceTestModel model)", ex);
                    return default(ServiceTestModel);
                }
            }
            return default(ServiceTestModel);
        }

        private ObservableCollection<IServiceTestModel> GetTests()
        {
            try
            {
                
                var serviceTestModels = new List<ServiceTestModel>();
                var loadResourceTests = ResourceModel.Environment.ResourceRepository.LoadResourceTests(ResourceModel.ID);
                if (loadResourceTests != null)
                {
                    serviceTestModels = loadResourceTests.Select(to => new ServiceTestModel(ResourceModel.ID)
                    {
                        OldTestName = to.TestName,
                        TestName = to.TestName,
                        UserName = to.UserName,
                        AuthenticationType = to.AuthenticationType,
                        Enabled = to.Enabled,
                        ErrorExpected = to.ErrorExpected,
                        NoErrorExpected = to.NoErrorExpected,
                        LastRunDate = to.LastRunDate,
                        TestPending = to.TestPending,
                        IsDirty = to.IsDirty,
                        TestFailing = to.TestFailing,
                        TestPassed = to.TestPassed,
                        Password = to.Password,
                        TestInvalid = to.TestInvalid,
                        Inputs =to.Inputs?.Select(input => new ServiceTestInput(input.Variable, input.Value) as IServiceTestInput).ToList(),
                        Outputs =to.Outputs?.Select(output => new ServiceTestOutput(output.Variable, output.Value) as IServiceTestOutput).ToList()
                    }).ToList();

                }
                return serviceTestModels.ToObservableCollection<IServiceTestModel>();
            }
            catch (Exception)
            {
                return new ObservableCollection<IServiceTestModel>();
            }
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
