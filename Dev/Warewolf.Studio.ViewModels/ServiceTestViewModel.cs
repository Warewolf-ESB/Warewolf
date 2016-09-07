
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private bool _hasChanged;

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
            CreateTests();
            CreateTestCommand = new DelegateCommand(CreateTests, () => CanCreateTest);
            CanSave = true;
        }

        private void CreateTests()
        {
            var testModel = ServiceTestCommandHandler.CreateTest(ResourceModel);
            if (Tests == null)
            {
                Tests = new ObservableCollection<IServiceTestModel>();
            }
            Tests.Add(testModel);
            SelectedServiceTest = testModel;
            HasChanged = true;
        }

        private bool CanCreateTest
        {
            get
            {
                return true;
            }
        }
        public bool CanStopTest { get; set; }
        public bool CanRunAllTestsInBrowser { get; set; }
        public bool CanRunAllTestsCommand { get; set; }
        public bool CanRunSelectedTestInBrowser { get; set; }
        public bool CanRunSelectedTest { get; set; }
        public bool CanDuplicateTest { get; set; }
        public bool CanDeleteTest { get; set; }
        public bool CanSave { get; set; }

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
                    //var cnct = Server.IsConnected;
                    return isDirty;
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

        public bool HasChanged
        {
            get
            {
                return _hasChanged;
            }
            set
            {
                _hasChanged = value;
                if (_hasChanged)
                {
                    if (!DisplayName.EndsWith(" *"))
                    {
                        DisplayName += " *";
                    }
                }
            }
        }

        public void Save()
        {
            ResourceModel.SaveTests(Tests.ToList());
        }

        public IContextualResourceModel ResourceModel { get; set; }

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
            get { return _tests; }
            set
            {
                _tests = value;
                OnPropertyChanged(() => Tests);
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
            get { return _displayName; }
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
