/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Activities.Designers2.Core.Web.Put;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Web_Put
{
    public class WebPutActivityViewModel : CustomToolWithRegionBase, IWebServicePutViewModel
    {
        readonly IServiceInputBuilder _builder;
        const string DoneText = "Done";
        const string FixText = "Fix";
        const string OutputDisplayName = " - Outputs";
        IWebPutInputArea _inputArea;
        IOutputsToolRegion _outputsRegion;
        ISourceToolRegion<IWebServiceSource> _sourceRegion;

        IErrorInfo _worstDesignError;

        public WebPutActivityViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IWebServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;
            _builder = new ServiceInputBuilder();
            SetupCommonProperties();
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_WebMethod_Put;
        }

        public WebPutActivityViewModel(ModelItem modelItem, IWebServiceModel model)
            : base(modelItem)
        {
            Model = model;
            _builder = new ServiceInputBuilder();
            SetupCommonProperties();
        }

        Guid UniqueID => GetProperty<Guid>();

        void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageWebServiceInputViewModel(this, Model));
            NoError = new ErrorInfo
            {
                ErrorType = ErrorType.None,
                Message = "Service Working Normally"
            };
            if (SourceRegion.SelectedSource == null)
            {
                UpdateLastValidationMemoWithSourceNotFoundError();
            }
            UpdateWorstError();
        }

        void UpdateLastValidationMemoWithSourceNotFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.None,
                Message = _sourceNotFoundMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        public void ClearValidationMemoWithNoFoundError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = UniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType.None,
                FixType = FixType.None,
                Message = ""
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            foreach (var error in errors)
            {
                DesignValidationErrors.Add(error);
            }
            UpdateWorstError();
        }

        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        public override void Validate()
        {
            if (Errors == null)
            {
                Errors = new List<IActionableErrorInfo>();
            }
            Errors.Clear();

            Errors = Regions.SelectMany(a => a.Errors).Select(a => new ActionableErrorInfo(new ErrorInfo { Message = a, ErrorType = ErrorType.Critical }, () => { }) as IActionableErrorInfo).ToList();
            if (SourceRegion.Errors.Count > 0)
            {
                foreach (var designValidationError in SourceRegion.Errors)
                {
                    DesignValidationErrors.Add(new ErrorInfo { ErrorType = ErrorType.Critical, Message = designValidationError });
                }
            }
            if (Errors.Count <= 0)
            {
                ClearValidationMemoWithNoFoundError();
            }
            UpdateWorstError();
            InitializeProperties();
        }

        void UpdateWorstError()
        {
            if (DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
            }

            IErrorInfo[] worstError = { DesignValidationErrors[0] };

            foreach (var error in DesignValidationErrors.Where(error => error.ErrorType > worstError[0].ErrorType))
            {
                worstError[0] = error;
                if (error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }
            SetWorstDesignError(worstError[0]);
        }

        void SetWorstDesignError(IErrorInfo value)
        {
            if (_worstDesignError != value)
            {
                _worstDesignError = value;
                IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                WorstError = value?.ErrorType ?? ErrorType.None;
            }
        }

        void InitialiseViewModel(IManageWebServiceInputViewModel manageServiceInputViewModel)
        {
            ManageServiceInputViewModel = manageServiceInputViewModel;

            BuildRegions();

            LabelWidth = 46;
            ButtonDisplayValue = DoneText;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;

            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o => { IsWorstErrorReadOnly = true; });

            SetDisplayName("");
            OutputsRegion.OutputMappingEnabled = true;
            TestInputCommand = new DelegateCommand(TestProcedure);

            InitializeProperties();

            if (OutputsRegion != null && OutputsRegion.IsEnabled)
            {
                var recordsetItem = OutputsRegion.Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (recordsetItem != null)
                {
                    OutputsRegion.IsEnabled = true;
                }
            }
        }

        public int LabelWidth { get; set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }

        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", SourceRegion.SelectedSource == null ? "" : SourceRegion.SelectedSource.Name);
            AddProperty("Type :", Type);
            AddProperty("Url :", SourceRegion.SelectedSource == null ? "" : SourceRegion.SelectedSource.HostName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public IManageWebServiceInputViewModel ManageServiceInputViewModel { get; set; }

        public void TestProcedure()
        {
            if (SourceRegion.SelectedSource != null)
            {
                var service = ToModel();
                ManageServiceInputViewModel.InputArea.Inputs = service.Inputs;
                ManageServiceInputViewModel.Model = service;

                ManageServiceInputViewModel.IsGenerateInputsEmptyRows = service.Inputs.Count < 1;
                ManageServiceInputViewModel.InputCountExpandAllowed = service.Inputs.Count > 5;

                GenerateOutputsVisible = true;
                SetDisplayName(OutputDisplayName);
            }
        }

        IErrorInfo NoError { get; set; }

        public bool IsWorstErrorReadOnly
        {
            get => (bool)GetValue(IsWorstErrorReadOnlyProperty);
            private set
            {
                ButtonDisplayValue = value ? DoneText : FixText;
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }
        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(WebPutActivityViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get => (ErrorType)GetValue(WorstErrorProperty);
            private set
            {
                SetValue(WorstErrorProperty, value);
            }
        }
        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(WebPutActivityViewModel), new PropertyMetadata(ErrorType.None));

        bool _generateOutputsVisible;

        public DelegateCommand TestInputCommand { get; set; }

        string Type => GetProperty<string>();

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        public void SetDisplayName(string displayName)
        {
            var index = DisplayName.IndexOf(" -", StringComparison.Ordinal);

            if (index > 0)
            {
                DisplayName = DisplayName.Remove(index);
            }

            var displayName2 = DisplayName;

            if (!string.IsNullOrEmpty(displayName2) && displayName2.Contains("Dsf"))
            {
                DisplayName = displayName2;
            }
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                DisplayName = displayName2 + displayName;
            }
        }

        public IHeaderRegion GetHeaderRegion() => InputArea;

        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        public string ButtonDisplayValue { get; set; }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (SourceRegion == null)
            {
                SourceRegion = new WebSourceRegion(Model, ModelItem) { SourceChangedAction = () => { OutputsRegion.IsEnabled = false; } };
                regions.Add(SourceRegion);
                InputArea = new WebPutInputRegion(ModelItem, SourceRegion);
                InputArea.PropertyChanged += (sender, args) =>
                 {
                     if (args.PropertyName == "PutData" && InputArea.Headers.All(value => string.IsNullOrEmpty(value.Name)) && args.PropertyName is "IsPutDataBase64")
                     {
                         ((ManageWebServiceInputViewModel)ManageServiceInputViewModel).BuidHeaders(InputArea.PutData);
                     }

                 };
                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem, true);
                regions.Add(OutputsRegion);
                if (OutputsRegion.Outputs.Count > 0)
                {
                    OutputsRegion.IsEnabled = true;
                }
                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(InputArea);
                SourceRegion.Dependants.Add(OutputsRegion);
            }
            regions.Add(ManageServiceInputViewModel);
            Regions = regions;
            return regions;
        }

        public ErrorRegion ErrorRegion { get; private set; }

        public IOutputsToolRegion OutputsRegion
        {
            get => _outputsRegion;
            set
            {
                _outputsRegion = value;
                OnPropertyChanged();
            }
        }
        public IWebPutInputArea InputArea
        {
            get => _inputArea;
            set
            {
                _inputArea = value;
                OnPropertyChanged();
            }
        }
        public ISourceToolRegion<IWebServiceSource> SourceRegion
        {
            get => _sourceRegion;
            set
            {
                _sourceRegion = value;
                InitializeProperties();
                OnPropertyChanged();
            }
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
            }
        }

        public void ValidateTestComplete()
        {
            OutputsRegion.IsEnabled = true;
        }

        public IWebService ToModel()
        {
            var webServiceDefinition = new WebServiceDefinition
            {
                Inputs = InputsFromModel(),
                OutputMappings = new List<IServiceOutputMapping>(),
                Source = SourceRegion.SelectedSource,
                Name = "",
                Path = "",
                Id = Guid.NewGuid(),
                PostData = InputArea.PutData,
                Headers = InputArea.Headers.Select(value => new NameValue { Name = value.Name, Value = value.Value } as INameValue).ToList(),
                QueryString = InputArea.QueryString,
                RequestUrl = SourceRegion.SelectedSource.HostName,
                Response = "",
                Method = WebRequestMethod.Put
            };
            return webServiceDefinition;
        }

        IList<IServiceInput> InputsFromModel()
        {
            var dt = new List<IServiceInput>();
            var s = InputArea.QueryString;
            var postValue = InputArea.PutData;
            _builder.GetValue(s, dt);
            _builder.GetValue(postValue, dt);
            foreach (var nameValue in InputArea.Headers)
            {
                _builder.GetValue(nameValue.Name, dt);
                _builder.GetValue(nameValue.Value, dt);
            }
            return dt;
        }

        IWebServiceModel Model { get; set; }
        public bool GenerateOutputsVisible
        {
            get => _generateOutputsVisible;
            set
            {
                _generateOutputsVisible = value;
                OutputVisibilitySetter.SetGenerateOutputsVisible(ManageServiceInputViewModel.InputArea, ManageServiceInputViewModel.OutputArea, SetRegionVisibility, value);
                OnPropertyChanged();
            }
        }

        public ICommand CellChangedCommand { get; set; }

        void SetRegionVisibility(bool value)
        {
            InputArea.IsEnabled = value;
            OutputsRegion.IsEnabled = value && OutputsRegion.Outputs.Count > 0;
            ErrorRegion.IsEnabled = value;
            SourceRegion.IsEnabled = value;
        }
    }
}