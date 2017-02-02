using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageEnhancedPluginServiceInputViewModel : IManageEnhancedPluginServiceInputViewModel
    {
        readonly IGenerateOutputArea _generateOutputArea;
        readonly IGenerateInputArea _generateInputArea;
        bool _isEnabled;
        bool _pasteResponseAvailable;
        readonly IDotNetEnhancedViewModel _viewmodel;
        bool _isGenerateInputsEmptyRows;
        private bool _okSelected;
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private IPluginService _model;
        private bool _pasteResponseVisible;
        private bool _outputCountExpandAllowed;
        private bool _inputCountExpandAllowed;
        private bool _testPassed;
        private bool _testFailed;

        public ManageEnhancedPluginServiceInputViewModel(IDotNetEnhancedViewModel model)
        {
            PasteResponseAvailable = false;
            PasteResponseVisible = false;
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
            _generateOutputArea = new GenerateOutputsRegion();
            _generateInputArea = new GenerateInputsRegion();
            Errors = new List<string>();
            _viewmodel = model;
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

        void ResetOutputsView()
        {
            IsEnabled = false;
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

        public void ExecuteOk()
        {
            try
            {
                //_viewmodel.OutputsRegion.Outputs.Clear();
                //if (OutputArea != null)
                //{
                //    _viewmodel.OutputsRegion.Outputs = new ObservableCollection<IServiceOutputMapping>(OutputArea.Outputs);
                //    var recSet = _recordsetList.FirstOrDefault(recordset => !string.IsNullOrEmpty(recordset.Name));
                //    if (recSet != null)
                //    {
                //        _viewmodel.OutputsRegion.RecordsetName = recSet.Name;
                //    }
                //}
                //else
                //{
                //    throw new Exception(ErrorResource.NoOutPuts);
                //}
                //_viewmodel.OutputsRegion.ObjectResult = TestResults;
                //_viewmodel.OutputsRegion.Description = Description;
                //_viewmodel.OutputsRegion.IsEnabled = _viewmodel.OutputsRegion.Outputs.Count > 0;
                //OutputCountExpandAllowed = _viewmodel.OutputsRegion.Outputs.Count > 3;
                //ResetOutputsView();
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                _viewmodel.ErrorMessage(e, true);
            }

            OkSelected = true;
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

        public List<IActionableErrorInfo> ViewErrors { get; set; }

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

        #region Implementation of IManageServiceInputViewModel<IPluginService>

        public Action TestAction { get; set; }
        public ICommand TestCommand { get; set; }

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

        public ImageSource TestIconImageSource { get; set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action OkAction { get; set; }
        public Action CloseAction { get; set; }
        public IPluginService Model
        {
            get
            {
                return _model ?? new PluginServiceDefinition();
            }
            set
            {
                _model = value;
            }
        }
        public string TestHeader { get; set; }

        #endregion

        #region Implementation of IManageDatabaseInputViewModel

        public ICollection<IConstructorParameter> Inputs { get; set; }
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
            get
            {
                return _okSelected;
            }
            set
            {
                _okSelected = value;
                OnPropertyChanged();
            }
        }
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

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}