/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common;
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
using Warewolf.Data.Options;
using Warewolf.Options;
using Warewolf.UI;

namespace Dev2.Activities.Designers2.Core
{
    public class ManageWebServiceInputViewModel : IManageWebInputViewModel
    {
        string _testResults;
        bool _testResultsAvailable;
        bool _isTestResultsEmptyRows;
        bool _isTesting;
        bool _okSelected;
        IWebService _model;
        bool _pasteResponseVisible;
        bool _pasteResponseAvailable;
        readonly IGenerateOutputArea _generateOutputArea;
        readonly IGenerateInputArea _generateInputArea;
        bool _isEnabled;
        readonly IWebServiceBaseViewModel _viewmodel;
        readonly IWebServiceModel _serverModel;
        bool _isGenerateInputsEmptyRows;
        RecordsetList _recordsetList;
        bool _outputCountExpandAllowed;
        bool _inputCountExpandAllowed;
        bool _testPassed;
        bool _testFailed;
        readonly IWebServiceHeaderBuilder _serviceHeaderBuilder;
        private bool _isFormDataChecked;
        private IOptionsWithNotifier _conditionExpressionOptions;

        public ManageWebServiceInputViewModel(IWebServiceHeaderBuilder serviceHeaderBuilder)
        {
            _serviceHeaderBuilder = serviceHeaderBuilder;
        }

        public ManageWebServiceInputViewModel(IWebServiceBaseViewModel model, IWebServiceModel serviceModel)
            : this(new WebServiceHeaderBuilder())
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
            LoadConditionExpressionOptions();
        }

        public bool OutputCountExpandAllowed
        {
            get => _outputCountExpandAllowed;
            set
            {
                _outputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }

        public bool InputCountExpandAllowed
        {
            get => _inputCountExpandAllowed;
            set
            {
                _inputCountExpandAllowed = value;
                OnPropertyChanged();
            }
        }

        public void BuidHeaders(string requestpayLoad)
        {
            try
            {
                _serviceHeaderBuilder.BuildHeader(_viewmodel.GetHeaderRegion(), requestpayLoad);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);
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
                    BuidHeaders(TestResults);
                    _recordsetList = responseService.Recordsets;
                    if (_recordsetList.Any(recordset => recordset.HasErrors))
                    {
                        var errorMessage = string.Join(Environment.NewLine, _recordsetList.Select(recordset => recordset.ErrorMessage));
                        throw new Exception(errorMessage);
                    }

                    Description = responseService.GetOutputDescription();
                }

                var outputMapping = _recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                {
                    var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                    return serviceOutputMapping;
                }).Cast<IServiceOutputMapping>().ToList();

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
                    _viewmodel.OutputsRegion.ResetOutputs(OutputArea.Outputs);
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

        public IGenerateInputArea InputArea => _generateInputArea;

        public bool IsFormDataChecked
        {
            get => _isFormDataChecked;
            set
            {
                _isFormDataChecked = value;
                OnPropertyChanged();
            }
        }

        public IOptionsWithNotifier ConditionExpressionOptions
        {
            get => _conditionExpressionOptions;
            set
            {
                _conditionExpressionOptions = value;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
                _conditionExpressionOptions.OptionChanged += UpdateConditionExpressionOptions;
            }
        }

        private void LoadConditionExpressionOptions()
        {
            var conditionExpressions = new List<FormDataConditionExpression>();
            var result = OptionConvertor.ConvertFromListOfT(conditionExpressions);
            ConditionExpressionOptions = new OptionsWithNotifier { Options = result };
            UpdateConditionExpressionOptions();
        }

        private void UpdateConditionExpressionOptions()
        {
            if (ConditionExpressionOptions?.Options != null)
            {
                AddEmptyConditionExpression();
                foreach (var item in ConditionExpressionOptions.Options)
                {
                    if (item is FormDataOptionConditionExpression conditionExpression)
                    {
                        conditionExpression.DeleteCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(a =>
                        {
                            RemoveConditionExpression(conditionExpression);
                        });
                    }
                }
            }
        }

        private void RemoveConditionExpression(FormDataOptionConditionExpression conditionExpression)
        {
            var count = ConditionExpressionOptions.Options.Count(o => o is FormDataOptionConditionExpression optionCondition && optionCondition.IsEmptyRow);
            var empty = conditionExpression.IsEmptyRow;
            var allow = !empty || count > 1;

            if (_conditionExpressionOptions.Options.Count > 1 && allow)
            {
                var list = new List<IOption>(_conditionExpressionOptions.Options);
                list.Remove(conditionExpression);
                ConditionExpressionOptions.Options = list;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
            }
        }

        private void AddEmptyConditionExpression()
        {
            var emptyRows = ConditionExpressionOptions.Options.Where(o => o is FormDataOptionConditionExpression optionCondition && optionCondition.IsEmptyRow);

            if (!emptyRows.Any())
            {
                var conditionExpression = new FormDataOptionConditionExpression();
                var list = new List<IOption>(_conditionExpressionOptions.Options)
                {
                    conditionExpression
                };
                ConditionExpressionOptions.Options = list;
                OnPropertyChanged(nameof(ConditionExpressionOptions));
            }
        }

        public void LoadConditionExpressionOptions(IList<IOption> options)
        {
            ConditionExpressionOptions.Options = new List<IOption>(options);
            UpdateConditionExpressionOptions();
            OnPropertyChanged(nameof(ConditionExpressionOptions));
        }

        public string TestResults
        {
            get => _testResults;
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
            get => _okSelected;
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
            get => _testPassed;
            set
            {
                _testPassed = value;
                OnPropertyChanged();
            }
        }

        public bool TestFailed
        {
            get => _testFailed;
            set
            {
                _testFailed = value;
                OnPropertyChanged();
            }
        }

        public bool TestResultsAvailable
        {
            get => _testResultsAvailable;
            set
            {
                _testResultsAvailable = value;
                OnPropertyChanged();
            }
        }
        public bool IsTestResultsEmptyRows
        {
            get => _isTestResultsEmptyRows;
            set
            {
                _isTestResultsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        public bool IsTesting
        {
            get => _isTesting;
            set
            {
                _isTesting = value;
                OnPropertyChanged();
            }
        }

        public bool PasteResponseVisible
        {
            get => _pasteResponseVisible;
            set
            {
                _pasteResponseVisible = value;
                OnPropertyChanged();
            }
        }

        public bool PasteResponseAvailable
        {
            get => _pasteResponseAvailable;
            set
            {
                _pasteResponseAvailable = value;
                OnPropertyChanged();
            }
        }

        public bool IsGenerateInputsEmptyRows
        {
            get => _isGenerateInputsEmptyRows;
            set
            {
                _isGenerateInputsEmptyRows = value;
                OnPropertyChanged();
            }
        }

        public ImageSource TestIconImageSource => Application.Current.TryFindResource("Explorer-WebService-White") as DrawingImage;

        public ICommand CloseCommand { get; private set; }
        public ICommand OkCommand { get; private set; }
        public Action CloseAction { get; set; }
        public IWebService Model
        {
            get => _model ?? new WebServiceDefinition();
            set => _model = value;
        }
        public string TestHeader { get; set; }
        public Action OkAction { get; set; }
        public ICommand PasteResponseCommand { get; private set; }
        public IGenerateOutputArea OutputArea => _generateOutputArea;

        public IOutputDescription Description { get; set; }

        public string ToolRegionName { get; set; }
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }
        public IList<IToolRegion> Dependants { get; set; }
        public IList<string> Errors { get; private set; }

        public IToolRegion CloneRegion() => this;

        public void RestoreRegion(IToolRegion toRestore)
        {
        }

        public EventHandler<List<string>> ErrorsHandler
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}