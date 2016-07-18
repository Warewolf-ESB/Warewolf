using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.Database;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;
using Warewolf.Resource.Errors;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageDatabaseServiceInputViewModel : IManageDatabaseInputViewModel
    {
        readonly IGenerateOutputArea _generateOutputArea;
        readonly IGenerateInputArea _generateInputArea;
        bool _isEnabled;
        readonly IDatabaseServiceViewModel _viewmodel;
        readonly IDbServiceModel _serverModel;
        bool _isGenerateInputsEmptyRows;
        private bool _okSelected;
        private DataTable _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private IDatabaseService _model;
        private bool _inputCountExpandAllowed;
        private bool _outputCountExpandAllowed;

        public ManageDatabaseServiceInputViewModel(IDatabaseServiceViewModel model, IDbServiceModel serviceModel)
        {
            IsTesting = false;
            CloseCommand = new DelegateCommand(ExecuteClose);
            OkCommand = new DelegateCommand(ExecuteOk);
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

        void ResetOutputsView()
        {
            IsEnabled = false;
            _viewmodel.GenerateOutputsVisible = false;
            InputArea.IsEnabled = false;
            OutputArea.IsEnabled = false;
            TestResults = new DataTable();
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
                _viewmodel.OutputsRegion.Outputs.Clear();
                if (TestResults != null)
                {
                    _viewmodel.OutputsRegion.Outputs = new ObservableCollection<IServiceOutputMapping>(GetDbOutputMappingsFromTable(TestResults));
                }
                else
                {
                    throw new Exception(ErrorResource.NoOutPuts);
                }
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

        List<IServiceOutputMapping> GetDbOutputMappingsFromTable(DataTable testResults)
        {
            List<IServiceOutputMapping> mappings = new List<IServiceOutputMapping>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            if (testResults != null)
            {
                if (testResults.Columns.Count < 1)
                {
                    _viewmodel.OutputsRegion.RecordsetName = String.Empty;
                    throw new Exception("Invalid table returned. Please check parameter or stored procedure.");
                }
                var recordsetName = string.IsNullOrEmpty(testResults.TableName) ? Model.Action.Name.Replace(".", "_") : testResults.TableName;
                _viewmodel.OutputsRegion.RecordsetName = recordsetName;
                for (int i = 0; i < testResults.Columns.Count; i++)
                {
                    var column = testResults.Columns[i];
                    var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString(), recordsetName);
                    mappings.Add(dbOutputMapping);
                }
                return mappings;
            }
            return new List<IServiceOutputMapping>();
        }

        public void ExecuteTest()
        {
            OutputArea.IsEnabled = true;
            TestResults = null;
            IsTesting = true;
            Errors = new List<string>();
            _viewmodel.ErrorMessage(new Exception(""), false);
            try
            {
                TestResults = _serverModel.TestService(Model);
                if (Model.Source.Type == enSourceType.ODBC)
                {
                    TestResults.TableName = @"Unnamed";
                }
                if (TestResults != null)
                {
                    if (TestResults.Columns.Count < 1)
                    {
                        throw new Exception("Invalid table returned. Please check parameter or stored procedure.");
                    }
                    TestResultsAvailable = TestResults.Rows.Count != 0;
                    IsTestResultsEmptyRows = TestResults.Rows.Count < 1;
                    _generateOutputArea.IsEnabled = true;
                    OutputCountExpandAllowed = TestResults.Rows.Count > 3;

                    if (!OutputCountExpandAllowed)
                    {
                        InputCountExpandAllowed = true;
                    }

                    IsTesting = false;
                }
            }
            catch (Exception e)
            {
                Errors.Add(e.Message);
                IsTesting = false;
                TestResultsAvailable = false;
                _generateOutputArea.IsEnabled = false;
                _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
                _viewmodel.ErrorMessage(e, true);
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

        #region Implementation of IManageServiceInputViewModel<IDatabaseService>

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

        public ImageSource TestIconImageSource { get; set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; set; }
        public Action OkAction { get; set; }
        public Action CloseAction { get; set; }
        public IDatabaseService Model
        {
            get
            {
                return _model ?? new DatabaseService();
            }
            set
            {
                _model = value;
            }
        }
        public string TestHeader { get; set; }

        #endregion

        #region Implementation of IManageDatabaseInputViewModel

        public ICollection<IServiceInput> Inputs { get; set; }
        public DataTable TestResults
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

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
