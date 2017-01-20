﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.ConstructorRegion;
using Dev2.Activities.Designers2.Core.Extensions;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.NamespaceRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.DotNet;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Net_Dll_Enhanced
{
    public class DotNetDllEnhancedViewModel : CustomToolWithRegionBase, IDotNetEnhancedViewModel
    {
        private IOutputsToolRegion _outputsRegion;
        private IOutputsToolRegion _methodOutputsRegion;
        private IDotNetConstructorInputRegion _inputArea;
        private IDotNetMethodInputRegion _methodInputRegion;
        private ISourceToolRegion<IPluginSource> _sourceRegion;
        private INamespaceToolRegion<INamespaceItem> _namespaceRegion;
        private IConstructorRegion<IPluginConstructor> _constructorRegion;
        private IMethodToolRegion<IPluginAction> _methodRegion;

        private IErrorInfo _worstDesignError;

        const string DoneText = "Done";
        const string FixText = "Fix";
        const string OutputDisplayName = " - Outputs";

        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        public DotNetDllEnhancedViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IPluginServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;

            SetupCommonProperties();
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Resources_Dot_net_DLL;
        }

        Guid UniqueID => GetProperty<Guid>();

        private void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageEnhancedPluginServiceInputViewModel(this));
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

        private void InitialiseViewModel(IManageEnhancedPluginServiceInputViewModel manageServiceInputViewModel)
        {
            ManageServiceInputViewModel = manageServiceInputViewModel;

            BuildRegions();

            LabelWidth = 46;
            ButtonDisplayValue = DoneText;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;

            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o =>
            {
                FixErrors();
                IsWorstErrorReadOnly = true;
            });

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

        public DotNetDllEnhancedViewModel(ModelItem modelItem, IPluginServiceModel model)
            : base(modelItem)
        {
            Model = model;
            SetupCommonProperties();
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
            if (Errors == null)
            {
                Errors = new List<IActionableErrorInfo>();
            }
            Errors.Clear();

            Errors = Regions.SelectMany(a => a.Errors).Select(a => new ActionableErrorInfo(new ErrorInfo() { Message = a, ErrorType = ErrorType.Critical }, () => { }) as IActionableErrorInfo).ToList();
            if (SourceRegion.Errors.Count > 0)
            {
                foreach (var designValidationError in SourceRegion.Errors)
                {
                    DesignValidationErrors.Add(new ErrorInfo() { ErrorType = ErrorType.Critical, Message = designValidationError });
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
            WorstDesignError = worstError[0];
        }

        IErrorInfo WorstDesignError
        {
            // ReSharper disable once UnusedMember.Local
            get { return _worstDesignError; }
            set
            {
                if (_worstDesignError != value)
                {
                    _worstDesignError = value;
                    IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                    WorstError = value?.ErrorType ?? ErrorType.None;
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
            AddProperty("Constructor :", ConstructorRegion.SelectedConstructor == null ? "" : ConstructorRegion.SelectedConstructor.ConstructorName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public IManageEnhancedPluginServiceInputViewModel ManageServiceInputViewModel { get; set; }

        public void TestProcedure()
        {
            if (MethodRegion.SelectedMethod != null)
            {
                var service = ToModel();
                ManageServiceInputViewModel.InputArea.Inputs = service.Inputs;
                ManageServiceInputViewModel.Model = service;

                ManageServiceInputViewModel.IsGenerateInputsEmptyRows = service.Inputs.Count < 1;
                ManageServiceInputViewModel.InputCountExpandAllowed = service.Inputs.Count > 5;
                ManageServiceInputViewModel.OutputCountExpandAllowed = true;

                GenerateOutputsVisible = true;
                SetDisplayName(OutputDisplayName);
            }
        }

        private IErrorInfo NoError { get; set; }

        public bool IsWorstErrorReadOnly
        {
            get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
            private set
            {
                ButtonDisplayValue = value ? DoneText : FixText;
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }
        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(DotNetDllEnhancedViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(DotNetDllEnhancedViewModel), new PropertyMetadata(ErrorType.None));

        bool _generateOutputsVisible;
        private ServiceInputBuilder _builder;
        private List<IMethodToolRegion<IPluginAction>> _methodsToRunList;

        public DelegateCommand TestInputCommand { get; set; }

        private string Type => GetProperty<string>();
        // ReSharper disable InconsistentNaming

        private void FixErrors()
        {
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        public string ButtonDisplayValue { get; set; }

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (SourceRegion == null)
            {
                SourceRegion = new DotNetSourceRegion(Model, ModelItem)
                {
                    SourceChangedAction = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = false;
                            OutputsRegion.IsObject = true;
                        }
                        if (Regions != null)
                        {
                            foreach (var toolRegion in Regions)
                            {
                                toolRegion.Errors?.Clear();
                            }
                        }
                    }
                };
                regions.Add(SourceRegion);
                NamespaceRegion = new DotNetNamespaceRegion(Model, ModelItem, SourceRegion)
                {
                    SourceChangedNamespace = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = true;
                            OutputsRegion.IsObject = true;
                        }
                        if (Regions != null)
                        {
                            foreach (var toolRegion in Regions)
                            {
                                toolRegion.Errors?.Clear();
                            }
                        }
                    },
                    IsNewPluginNamespace = true
                };
                NamespaceRegion.SomethingChanged += (sender, args) =>
                {
                    if (args.Errors != null)
                        Errors =
                            args.Errors.Select(e => new ActionableErrorInfo { ErrorType = ErrorType.Critical, Message = e } as IActionableErrorInfo)
                                .ToList();
                    var dotNetNamespaceRegion = sender as DotNetNamespaceRegion;
                    var outputsRegion = dotNetNamespaceRegion?.Dependants.Single(region => region is OutputsRegion) as OutputsRegion;
                    if(outputsRegion != null)
                    {
                        if(dotNetNamespaceRegion.SelectedNamespace != null)
                        {
                            outputsRegion.ObjectResult = dotNetNamespaceRegion.SelectedNamespace.JsonObject;
                        }
                    }
                };
                regions.Add(NamespaceRegion);
                ConstructorRegion = new DotNetConstructorRegion(Model, ModelItem, SourceRegion, NamespaceRegion)
                {
                    SourceChangedAction = () =>
                    {
                        if (OutputsRegion != null)
                        {
                            OutputsRegion.IsEnabled = true;
                            OutputsRegion.IsObject = true;
                        }
                        if (Regions != null)
                        {
                            foreach (var toolRegion in Regions)
                            {
                                toolRegion.Errors?.Clear();
                            }
                        }
                    },
                };

                ConstructorRegion.ErrorsHandler += (sender, list) =>
                {
                    List<ActionableErrorInfo> errorInfos = list.Select(error => new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, Message = error }, () => { })).ToList();
                    UpdateDesignValidationErrors(errorInfos);
                    Errors = new List<IActionableErrorInfo>(errorInfos);
                };
                regions.Add(ConstructorRegion);
                CreateMethodRegion();
                regions.Add(MethodRegion);
                InputArea = new DotNetConstructorInputRegion(ModelItem, ConstructorRegion);

                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem, true)
                {
                    IsObject = true,
                    IsEnabled = false,
                    IsOutputsEmptyRows = false
                };
                regions.Add(OutputsRegion);
                if (OutputsRegion.Outputs.Count > 0)
                {
                    OutputsRegion.IsEnabled = true;
                }

                MethodInputRegion = new DotNetMethodInputRegion(ModelItem, MethodRegion);
                regions.Add(MethodInputRegion);
                MethodOutputsRegion = new DotNetMethodOutputsRegion(ModelItem, true)
                {
                    IsObject = true,
                    IsEnabled = false
                };
                regions.Add(MethodOutputsRegion);
                if (MethodOutputsRegion.Outputs == null || MethodOutputsRegion.Outputs.Count == 0)
                {
                    MethodOutputsRegion.IsEnabled = false;
                }
                else if (MethodOutputsRegion.Outputs.Count > 0)
                {
                    MethodOutputsRegion.IsEnabled = true;
                }

                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(NamespaceRegion);
                NamespaceRegion.Dependants.Add(ConstructorRegion);
                NamespaceRegion.Dependants.Add(OutputsRegion);
                NamespaceRegion.Dependants.Add(MethodRegion);
                ConstructorRegion.Dependants.Add(InputArea);
                ConstructorRegion.Dependants.Add(OutputsRegion);
                MethodRegion.Dependants.Add(MethodInputRegion);
                MethodRegion.Dependants.Add(MethodOutputsRegion);

            }
            regions.Add(ManageServiceInputViewModel);
            Regions = regions;

            MethodsToRunList = new List<IMethodToolRegion<IPluginAction>>();
            AddToMethodsList();
            return regions;
        }

        private void AddToMethodsList()
        {
            if (MethodsToRunList == null)
            {
                MethodsToRunList = new List<IMethodToolRegion<IPluginAction>>();
            }
            MethodsToRunList.Add(MethodRegion);
        }

        private void CreateMethodRegion()
        {
            MethodRegion = new DotNetMethodRegion(Model, ModelItem, SourceRegion, NamespaceRegion)
            {
                SourceChangedAction = () =>
                {
                    MethodOutputsRegion.IsEnabled = false;
                    MethodOutputsRegion.IsObject = true;
                    if (Regions != null)
                    {
                        foreach (var toolRegion in Regions)
                        {
                            toolRegion.Errors?.Clear();
                        }
                    }
                    //CreateMethodRegion();
                }
            };
            MethodRegion.ErrorsHandler += (sender, list) =>
            {
                List<ActionableErrorInfo> errorInfos =
                    list.Select(
                        error =>
                            new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, Message = error }, () => { }))
                        .ToList();
                UpdateDesignValidationErrors(errorInfos);
                Errors = new List<IActionableErrorInfo>(errorInfos);
            };
            AddToMethodsList();
        }

        public List<IMethodToolRegion<IPluginAction>> MethodsToRunList
        {
            get
            {
                return _methodsToRunList;
            }
            set
            {
                _methodsToRunList = value;
                OnPropertyChanged();
            }
        }

        public ErrorRegion ErrorRegion { get; private set; }

        #endregion

        #region Implementation of IDatabaseServiceViewModel

        public IConstructorRegion<IPluginConstructor> ConstructorRegion
        {
            get
            {
                return _constructorRegion;
            }
            set
            {
                _constructorRegion = value;
                OnPropertyChanged();
            }
        }
        public IMethodToolRegion<IPluginAction> MethodRegion
        {
            get
            {
                return _methodRegion;
            }
            set
            {
                _methodRegion = value;
                OnPropertyChanged();
            }
        }
        public ISourceToolRegion<IPluginSource> SourceRegion
        {
            get
            {
                return _sourceRegion;
            }
            set
            {
                _sourceRegion = value;
                OnPropertyChanged();
            }
        }
        public INamespaceToolRegion<INamespaceItem> NamespaceRegion
        {
            get
            {
                return _namespaceRegion;
            }
            set
            {
                _namespaceRegion = value;
                OnPropertyChanged();
            }
        }
        public IDotNetConstructorInputRegion InputArea
        {
            get
            {
                return _inputArea;
            }
            set
            {
                _inputArea = value;
                OnPropertyChanged();
            }
        }
        public IDotNetMethodInputRegion MethodInputRegion
        {
            get
            {
                return _methodInputRegion;
            }
            set
            {
                _methodInputRegion = value;
                OnPropertyChanged();
            }
        }
        public IOutputsToolRegion OutputsRegion
        {
            get
            {
                return _outputsRegion;
            }
            set
            {
                _outputsRegion = value;
                OnPropertyChanged();
            }
        }
        public IOutputsToolRegion MethodOutputsRegion
        {
            get
            {
                return _methodOutputsRegion;
            }
            set
            {
                _methodOutputsRegion = value;
                OnPropertyChanged();
            }
        }
        public bool GenerateOutputsVisible
        {
            get
            {
                return _generateOutputsVisible;
            }
            set
            {
                _generateOutputsVisible = value;
                if (value)
                {
                    ManageServiceInputViewModel.InputArea.IsEnabled = true;
                    ManageServiceInputViewModel.OutputArea.IsEnabled = false;
                    SetRegionVisibility(false);

                }
                else
                {
                    ManageServiceInputViewModel.InputArea.IsEnabled = false;
                    ManageServiceInputViewModel.OutputArea.IsEnabled = false;
                    SetRegionVisibility(true);
                }

                OnPropertyChanged();
            }
        }

        public IPluginService ToModel()
        {
            var pluginServiceDefinition = new PluginServiceDefinition
            {
                Source = SourceRegion.SelectedSource,
                Action = MethodRegion.SelectedMethod,
                Inputs = new List<IServiceInput>()
            };
            var dt = new List<IServiceInput>();
            foreach (var serviceInput in InputArea.Inputs)
            {
                _builder = _builder ?? new ServiceInputBuilder();
                _builder.GetValue(serviceInput.Value, dt);
                pluginServiceDefinition.Inputs.Add(new ServiceInput(serviceInput.Name, serviceInput.Value)
                {
                    TypeName = serviceInput.TypeName
                });
            }
            return pluginServiceDefinition;
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo() { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
        }

        public void SetDisplayName(string outputFieldName)
        {
            var index = DisplayName.IndexOf(" -", StringComparison.Ordinal);

            if (index > 0)
            {
                DisplayName = DisplayName.Remove(index);
            }

            var displayName = DisplayName;

            if (!string.IsNullOrEmpty(displayName) && displayName.Contains("Dsf"))
            {
                DisplayName = displayName;
            }
            if (!string.IsNullOrWhiteSpace(outputFieldName))
            {
                DisplayName = displayName + outputFieldName;
            }
        }

        private IPluginServiceModel Model { get; set; }

        void SetRegionVisibility(bool value)
        {
            InputArea.IsEnabled = value;
            OutputsRegion.IsEnabled = value && OutputsRegion.Outputs.Count > 0;
            ErrorRegion.IsEnabled = value;
            SourceRegion.IsEnabled = value;
        }

        #endregion
    }
}