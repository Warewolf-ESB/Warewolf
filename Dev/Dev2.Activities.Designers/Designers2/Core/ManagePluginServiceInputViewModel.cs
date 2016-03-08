
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Communication;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.Practices.Prism.Commands;
using Newtonsoft.Json;
using Warewolf.Core;
// ReSharper disable FieldCanBeMadeReadOnly.Local

namespace Dev2.Activities.Designers2.Core
{
    public class ManagePluginServiceInputViewModel : IManagePluginServiceInputViewModel
    {
        IGenerateOutputArea _generateOutputArea;
        IGenerateInputArea _generateInputArea;
        double _minHeight;
        double _currentHeight;
        double _maxHeight;
        bool _isVisible;
        bool _pasteResponseAvailable;
        IDotNetViewModel _viewmodel;
        IPluginServiceModel _serverModel;
        bool _isGenerateInputsEmptyRows;
        private bool _okSelected;
        private string _testResults;
        private bool _testResultsAvailable;
        private bool _isTestResultsEmptyRows;
        private bool _isTesting;
        private IPluginService _model;
        private bool _pasteResponseVisible;
        private RecordsetList _recordsetList;
        private bool _outputCountExpandAllowed;
        private bool _inputCountExpandAllowed;

        public ManagePluginServiceInputViewModel(IDotNetViewModel model, IPluginServiceModel serviceModel)
        {
            PasteResponseAvailable = false;
            PasteResponseVisible = false;
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
            IsVisible = false;
            _viewmodel.GenerateOutputsVisible = false;
            InputArea.IsVisible = false;
            OutputArea.IsVisible = false;
            TestResults = String.Empty;
            TestResultsAvailable = false;

            _viewmodel.SetDisplayName("");
            _viewmodel.ErrorMessage(new Exception(), false);
        }

        public void ExecuteClose()
        {
           
            _viewmodel.OutputsRegion.IsVisible = _viewmodel.OutputsRegion.Outputs.Count > 0;
            if (TestResults != null)
            {
                TestResultsAvailable = TestResults != null;
                IsTesting = false;
            }
            ResetOutputsView();
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
                var responseService = serializer.Deserialize<RecordsetListWrapper>(testResult);
                if (responseService != null)
                {
                    if (responseService.RecordsetList.Any(recordset => recordset.HasErrors))
                    {
                        var errorMessage = string.Join(Environment.NewLine, responseService.RecordsetList.Select(recordset => recordset.ErrorMessage));
                        throw new Exception(errorMessage);
                    }
                    TestResults = testResult;
                    _recordsetList = responseService.RecordsetList;
                    if (_recordsetList.Any(recordset => recordset.HasErrors))
                    {
                        var errorMessage = string.Join(Environment.NewLine, _recordsetList.Select(recordset => recordset.ErrorMessage));
                        throw new Exception(errorMessage);
                    }
                    Description = responseService.Description;
                    // ReSharper disable MaximumChainedReferences
                    var outputMapping = _recordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                    {
                        var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name) { Path = recordsetField.Path };
                        return serviceOutputMapping;
                    }).Cast<IServiceOutputMapping>().ToList();
                    // ReSharper restore MaximumChainedReferences
                    _generateOutputArea.IsVisible = true;
                    _generateOutputArea.Outputs = outputMapping;
                }
                if(TestResults != null)
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
                _generateOutputArea.IsVisible = false;
                _generateOutputArea.Outputs = new List<IServiceOutputMapping>();
                _viewmodel.ErrorMessage(e, true);
            }
            OnHeightChanged(this);
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
        public double MinHeight
        {
            get
            {
                return _minHeight;
            }
            set
            {

                if (Math.Abs(_minHeight - value) > GlobalConstants.DesignHeightTolerance)
                {
                    _minHeight = value;
                    OnHeightChanged(this);
                    OnPropertyChanged();
                }
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
                if (Math.Abs(_currentHeight - value) > GlobalConstants.DesignHeightTolerance)
                {
                    _currentHeight = value;
                    OnHeightChanged(this);
                    OnPropertyChanged();
                }
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
                if (Math.Abs(_maxHeight - value) > GlobalConstants.DesignHeightTolerance)
                {
                    _maxHeight = value;
                    OnHeightChanged(this);
                    OnPropertyChanged();
                }
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

        #region Implementation of IManageServiceInputViewModel<IPluginService>

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

        [ExcludeFromCodeCoverage]
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
        [ExcludeFromCodeCoverage]
        public void ShowView()
        {
        }
        [ExcludeFromCodeCoverage]
        public void CloseView()
        {
        }

        #endregion

        #region Implementation of IManageDatabaseInputViewModel

        public ICollection<IServiceInput> Inputs { get; set; }
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
            [ExcludeFromCodeCoverage]
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
            [ExcludeFromCodeCoverage]
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

        public void SetInitialVisibility()
        {
            IsVisible = true;
            InputArea.IsVisible = true;
            OutputArea.IsVisible = false;
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        protected void OnHeightChanged(IToolRegion args)
        {
            var handler = HeightChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

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