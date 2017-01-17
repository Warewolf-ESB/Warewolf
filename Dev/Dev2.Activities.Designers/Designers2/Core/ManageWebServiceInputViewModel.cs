using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using Warewolf.Core;
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.Designers2.Core
{
    public class ManageWebServiceInputViewModel : IManageWebServiceInputViewModel
    {
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private bool _okSelected;
        private IWebService _model;
        bool _pasteResponseVisible;
        bool _pasteResponseAvailable;
        IGenerateOutputArea _generateOutputArea;
        IGenerateInputArea _generateInputArea;
        bool _isEnabled;
        IWebServiceBaseViewModel _viewmodel;
        IWebServiceModel _serverModel;
        bool _isGenerateInputsEmptyRows;
        private RecordsetList _recordsetList;
        private bool _outputCountExpandAllowed;
        private bool _inputCountExpandAllowed;
        private bool _testPassed;
        private bool _testFailed;

        public ManageWebServiceInputViewModel(IWebServiceBaseViewModel model, IWebServiceModel serviceModel)
        {
            PasteResponseAvailable = true;
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
            PasteResponseCommand = new DelegateCommand(ExecutePaste);
            TestCommand = new DelegateCommand(ExecuteTest);
            _generateOutputArea = new GenerateOutputsRegion();
            _generateInputArea = new GenerateInputsRegion();
            Errors = new List<string>();
            _viewmodel = model;
            _serverModel = serviceModel;
        }

        public bool OutputCountExpandAllowed
        {
            get
            {
                return _outputCountExpandAllowed;
            }
            set
            {
                _outputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }

        public bool InputCountExpandAllowed
        {
            get
            {
                return _inputCountExpandAllowed;
            }
            set
            {
                _inputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }
        public void ExecuteTest()
        {
            ViewErrors = new List<IActionableErrorInfo>();
            Errors = new List<string>();
            OutputArea.IsEnabled = true;
            TestResults = null;
            IsTesting = true;

            try
            {
                var testResult = _serverModel.TestService(Model);
                var serializer = new Dev2JsonSerializer();
                using (var responseService = serializer.Deserialize<WebService>(testResult))
                {
                   TestResults = responseService.RequestResponse;
                    _recordsetList = responseService.Recordsets;
                    if (_recordsetList.Any(recordset => recordset.HasErrors))
                    {
                        var errorMessage = string.Join(Environment.NewLine, _recordsetList.Select(recordset => recordset.ErrorMessage));
                        throw new Exception(errorMessage);
                    }

                    Description = responseService.GetOutputDescription();
                }
                // ReSharper disable MaximumChainedReferences
                var outputMapping = _recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                {
                    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                    return serviceOutputMapping;
                }).Cast<IServiceOutputMapping>().ToList();
                // ReSharper restore MaximumChainedReferences
                _generateOutputArea.IsEnabled = true;
                _generateOutputArea.Outputs = outputMapping;
                
                
                if (TestResults != null)
                {
                    TestResultsAvailable = TestResults != null;
                    IsTesting = false;
                    TestPassed = true;
                    TestFailed = false;
                    OutputCountExpandAllowed = true;
                }
            }
            catch (JsonSerializationException)
            {
                OutputArea.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Result", "[[Result]]", "") };
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                TestPassed = false;
                TestFailed = true;
                _generateOutputArea.IsEnabled = false;
                _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
                _viewmodel.ErrorMessage(e, true);
            }
            PasteResponseVisible = false;
        }

        public List<IActionableErrorInfo> ViewErrors { get; set; }

        public void ExecutePaste()
        {
            OutputArea.IsEnabled = true;
            _generateOutputArea.IsEnabled = true;
            _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
            PasteResponseVisible = true;
        }

        public void ExecuteOk()
        {
            try
            {
                _viewmodel.OutputsRegion.Outputs.Clear();
                if (OutputArea != null)
                {
                    _viewmodel.OutputsRegion.Outputs = new ObservableCollection<IServiceOutputMapping>(OutputArea.Outputs);
                    var recSet = _recordsetList.FirstOrDefault(recordset => !string.IsNullOrEmpty(recordset.Name));
                    if (recSet != null)
                    {
                        _viewmodel.OutputsRegion.RecordsetName = recSet.Name;
                    }
                }
                else
                {
                    throw new Exception("No Outputs detected");
                }
                _viewmodel.OutputsRegion.ObjectResult = TestResults;
                _viewmodel.OutputsRegion.Description = Description;
                _viewmodel.OutputsRegion.IsEnabled = _viewmodel.OutputsRegion.Outputs.Count > 0;
                OutputCountExpandAllowed = _viewmodel.OutputsRegion.Outputs.Count > 3;
                ResetOutputsView();
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                _viewmodel.ErrorMessage(e, true);
            }

            OkSelected = true;
        }

        void ResetOutputsView()
        {
            IsEnabled = false;
            _viewmodel.GenerateOutputsVisible = false;
            PasteResponseVisible = false;
            InputArea.IsEnabled = false;
            OutputArea.IsEnabled = false;
            TestResults = string.Empty;
            TestFailed = false;
            TestPassed = false;
            TestResultsAvailable = false;
            Errors.Clear();

            _viewmodel.SetDisplayName("");
            _viewmodel.ErrorMessage(new Exception(), false);
        }

        public void ExecuteClose()
        {
            _viewmodel.OutputsRegion.IsEnabled = _viewmodel.OutputsRegion.Outputs.Count > 0;
            if (TestResults != null)
            {
                TestResultsAvailable = TestResults != null;
                IsTesting = false;
            }
            ResetOutputsView();
        }

        public IGenerateInputArea InputArea
        {
            get
            {
                return _generateInputArea;
            }
            set
            {
            }
        }
        public string TestResults
        {
            get
            {
                return _testResults;
            }
            set
            {
                _testResults = value;
                if (!string.IsNullOrEmpty(_testResults))
                {
                    Model.Response = _testResults;
                }
                OnPropertyChanged();
            }
        }

        public bool OkSelected
        {
            get { return _okSelected; }
            set
            {
                _okSelected = value;
                OnPropertyChanged();
            }
        }
        public Action TestAction { get; set; }
        public ICommand TestCommand { get; private set; }

        public bool TestPassed
        {
            get { return _testPassed; }
            set
            {
                _testPassed = value;
                OnPropertyChanged();
            }
        }

        public bool TestFailed
        {
            get { return _testFailed; }
            set
            {
                _testFailed = value;
                OnPropertyChanged();
            }
        }

        public bool TestResultsAvailable
        {
            get
            {
                return _testResultsAvailable;
            }
            set
            {
                _testResultsAvailable = value;
                OnPropertyChanged();
            }
        }
        public bool IsTestResultsEmptyRows
        {
            get
            {
                return _isTestResultsEmptyRows;
            }
            set
            {
                _isTestResultsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                _isTesting = value;
                OnPropertyChanged();
            }
        }

        public bool PasteResponseVisible
        {
            get
            {
                return _pasteResponseVisible;
            }
            set
            {
                _pasteResponseVisible = value;
                OnPropertyChanged();
            }
        }

        public bool PasteResponseAvailable
        {
            get
            {
                return _pasteResponseAvailable;
            }
            set
            {
                _pasteResponseAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool IsGenerateInputsEmptyRows
        {
            get
            {
                return _isGenerateInputsEmptyRows;
            }
            set
            {
                _isGenerateInputsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        public ImageSource TestIconImageSource
        {
            get
            {
                return Application.Current.TryFindResource("Explorer-WebService-White") as DrawingImage;
            }
            set
            {

            }
        }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action CloseAction { get; set; }
        public IWebService Model
        {
            get
            {

                return _model ?? new WebServiceDefinition();

            }
            set
            {
                _model = value;
            }
        }
        public string TestHeader { get; set; }


        public Action OkAction { get; set; }
        public ICommand PasteResponseCommand { get; private set; }
        public IGenerateOutputArea OutputArea
        {
            get
            {
                return _generateOutputArea;
            }
            set
            {
            }
        }
        public IOutputDescription Description { get; set; }

        

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; private set; }

        public IToolRegion CloneRegion()
        {
            return this;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}