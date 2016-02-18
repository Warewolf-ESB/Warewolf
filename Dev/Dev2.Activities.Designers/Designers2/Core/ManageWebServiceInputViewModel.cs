using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
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
        double _minHeight;
        double _currentHeight;
        double _maxHeight;
        bool _isVisible;
        IWebServiceGetViewModel _viewmodel;
        IWebServiceModel _serverModel;
        private const double BaseHeight = 200;

        public ManageWebServiceInputViewModel(IWebServiceGetViewModel model, IWebServiceModel serviceModel)
        {
            PasteResponseAvailable = true;
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
            PasteResponseCommand = new DelegateCommand(ExecutePaste);
            TestCommand = new DelegateCommand(ExecuteTest);
            _generateOutputArea = new GenerateOutputsRegion();
            _generateOutputArea.HeightChanged += GenerateAreaHeightChanged;
            _generateInputArea = new GenerateInputsRegion();
            _generateInputArea.HeightChanged += GenerateAreaHeightChanged;
            Errors = new List<string>();
            _viewmodel = model;
            _serverModel = serviceModel;
            SetInitialHeight();
        }

        private void SetInitialHeight()
        {
            MinHeight = BaseHeight;
            MaxHeight = BaseHeight;
            CurrentHeight = BaseHeight;
        }

        void GenerateAreaHeightChanged(object sender, IToolRegion args)
        {
            MaxHeight = _generateInputArea.MaxHeight + _generateOutputArea.MaxHeight;
            MinHeight = _generateInputArea.MinHeight + _generateOutputArea.MinHeight;
            CurrentHeight = _generateInputArea.CurrentHeight + _generateOutputArea.CurrentHeight;
            OnHeightChanged(this);
        }

        public void ExecuteTest()
        {
            ViewErrors = new List<IActionableErrorInfo>();
            OutputArea.IsVisible = true;
            TestResults = null;
            IsTesting = true;

            try
            {
                var testResult = _serverModel.TestService(Model);
                var serializer = new Dev2JsonSerializer();
                RecordsetList recordsetList;
                using (var responseService = serializer.Deserialize<WebService>(testResult))
                {
                    TestResults = responseService.RequestResponse;
                    recordsetList = responseService.Recordsets;
                    if (recordsetList.Any(recordset => recordset.HasErrors))
                    {
                        var errorMessage = string.Join(Environment.NewLine, recordsetList.Select(recordset => recordset.ErrorMessage));
                        throw new Exception(errorMessage);
                    }

                    Description = responseService.GetOutputDescription();
                }
                // ReSharper disable MaximumChainedReferences
                var outputMapping = recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                {
                    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                    return serviceOutputMapping;
                }).Cast<IServiceOutputMapping>().ToList();
                // ReSharper restore MaximumChainedReferences
                var recSet = recordsetList.FirstOrDefault(recordset => !string.IsNullOrEmpty(recordset.Name));
                if (recSet != null)
                {
                    _viewmodel.OutputsRegion.RecordsetName = recSet.Name;
                }

                _generateOutputArea.IsVisible = true;
                _generateOutputArea.Outputs = outputMapping;
                if (TestResults != null)
                {
                    TestResultsAvailable = TestResults != null;
                    IsTesting = false;
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
                _viewmodel.ErrorMessage(e, true);
            }
            OnHeightChanged(this);

            PasteResponseVisible = false;
        }

        public List<IActionableErrorInfo> ViewErrors { get; set; }

        public void ExecutePaste()
        {
            OutputArea.IsVisible = true;
            _generateOutputArea.IsVisible = true;
            _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
            PasteResponseVisible = true;
            OnHeightChanged(this);
        }

        public void ExecuteOk()
        {
            try
            {
                _viewmodel.OutputsRegion.Outputs.Clear();
                if (OutputArea != null)
                {
                    foreach (var serviceOutputMapping in OutputArea.Outputs)
                    {
                        _viewmodel.OutputsRegion.Outputs.Add(serviceOutputMapping);
                    }
                }
                else
                {
                    throw new Exception("No Outputs detected");
                }

                _viewmodel.OutputsRegion.Description = Description;
                _viewmodel.OutputsRegion.IsVisible = _viewmodel.OutputsRegion.Outputs.Count > 0;
                ResetOutputsView();
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                _viewmodel.ErrorMessage(e, true);
            }

            OkSelected = true;
            OnHeightChanged(this);
        }

        void ResetOutputsView()
        {
            IsVisible = false;
            _viewmodel.GenerateOutputsVisible = false;
            PasteResponseVisible = false;
            InputArea.IsVisible = false;
            OutputArea.IsVisible = false;
            TestResults = String.Empty;
            TestResultsAvailable = false;

            _viewmodel.SetDisplayName("");
            _viewmodel.ErrorMessage(new Exception(), false);
        }

        public void ExecuteClose()
        {
            _viewmodel.OutputsRegion.Outputs.Clear();
            _viewmodel.OutputsRegion.IsVisible = _viewmodel.OutputsRegion.Outputs.Count > 0;
            if (TestResults != null)
            {
                TestResultsAvailable = TestResults != null;
                IsTesting = false;
            }
            ResetOutputsView();
            OnHeightChanged(this);
        }

        public IGenerateInputArea InputArea
        {
            get
            {
                return _generateInputArea;
            }
            [ExcludeFromCodeCoverage]
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

        public void SetInitialVisibility()
        {
            SetInitialHeight();
            IsVisible = true;
            InputArea.IsVisible = true;
            OutputArea.IsVisible = false;
        }

        [ExcludeFromCodeCoverage]
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
            [ExcludeFromCodeCoverage]
            set
            {
            }
        }
        public IOutputDescription Description { get; set; }
        [ExcludeFromCodeCoverage]
        public virtual void ShowView()
        {
        }
        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
        }

        #region Implementation of IToolRegion

        public string ToolRegionName { get; set; }
        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {
                _minHeight = value;
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public double CurrentHeight
        {
            get
            {
                return _currentHeight;
            }
            set
            {
                _currentHeight = value;
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public double MaxHeight
        {
            get
            {
                return _maxHeight;
            }
            set
            {
                _maxHeight = value;
                OnHeightChanged(this);
                OnPropertyChanged();
            }
        }
        public event HeightChanged HeightChanged;
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; private set; }

        public IToolRegion CloneRegion()
        {
            return this;
        }

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        #endregion

        protected virtual void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}