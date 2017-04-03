﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
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

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.Designers2.ComDLL
{
    public class ComDllViewModel : CustomToolWithRegionBase, IComViewModel
    {
        private IOutputsToolRegion _outputsRegion;
        private IDotNetInputRegion _inputArea;
        private ISourceToolRegion<IComPluginSource> _sourceRegion;
        private INamespaceToolRegion<INamespaceItem> _namespaceRegion;
        private IActionToolRegion<IPluginAction> _actionRegion;

        private IErrorInfo _worstDesignError;

        const string DoneText = "Done";
        const string FixText = "Fix";
        const string OutputDisplayName = " - Outputs";

        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        public ComDllViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IComPluginServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;

            SetupCommonProperties();
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Resources_Com_DLL;
        }

        Guid UniqueID => GetProperty<Guid>();

        private void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageComPluginServiceInputViewModel(this, Model));
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

        private void InitialiseViewModel(IManageComPluginServiceInputViewModel manageServiceInputViewModel)
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

        public ComDllViewModel(ModelItem modelItem, IComPluginServiceModel model)
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
            AddProperty("Source :", SourceRegion.SelectedSource == null ? "" : SourceRegion.SelectedSource.ResourceName);
            AddProperty("Type :", Type);
            AddProperty("Procedure :", ActionRegion.SelectedAction == null ? "" : ActionRegion.SelectedAction.FullName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public IManageComPluginServiceInputViewModel ManageServiceInputViewModel { get; set; }

        public void TestProcedure()
        {
            if (ActionRegion.SelectedAction != null)
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
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ComDllViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
        DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(ComDllViewModel), new PropertyMetadata(ErrorType.None));

        bool _generateOutputsVisible;
        private ServiceInputBuilder _builder;

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
                SourceRegion = new ComSourceRegion(Model, ModelItem)
                {
                    SourceChangedAction = () =>
                        {
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
                NamespaceRegion = new ComNamespaceRegion(Model, ModelItem, SourceRegion)
                {
                    SourceChangedNamespace = () =>
                        {
                            OutputsRegion.IsEnabled = false;
                            if (Regions != null)
                            {
                                foreach (var toolRegion in Regions)
                                {
                                    toolRegion.Errors?.Clear();
                                }
                            }
                        }
                };
                NamespaceRegion.SomethingChanged += (sender, args) =>
                {
                    if (args.Errors != null)
                        Errors =
                            args.Errors.Select(e => new ActionableErrorInfo { ErrorType = ErrorType.Critical, Message = e } as IActionableErrorInfo)
                                .ToList();
                };
                regions.Add(NamespaceRegion);
                ActionRegion = new ComActionRegion(Model, ModelItem, SourceRegion, NamespaceRegion)
                {
                    SourceChangedAction = () =>
                        {
                            OutputsRegion.IsEnabled = false;
                            if (Regions != null)
                            {
                                foreach (var toolRegion in Regions)
                                {
                                    toolRegion.Errors?.Clear();
                                }
                            }
                        }
                };
                ActionRegion.ErrorsHandler += (sender, list) =>
                {
                    List<ActionableErrorInfo> errorInfos = list.Select(error => new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, Message = error }, () => { })).ToList();
                    UpdateDesignValidationErrors(errorInfos);
                    Errors = new List<IActionableErrorInfo>(errorInfos);
                };
                regions.Add(ActionRegion);
                InputArea = new DotNetInputRegion(ModelItem, ActionRegion);
                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem, true);
                regions.Add(OutputsRegion);
                if (OutputsRegion.Outputs.Count > 0)
                {
                    OutputsRegion.IsEnabled = true;

                }
                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(NamespaceRegion);
                NamespaceRegion.Dependants.Add(ActionRegion);
                ActionRegion.Dependants.Add(InputArea);
                ActionRegion.Dependants.Add(OutputsRegion);
            }
            regions.Add(ManageServiceInputViewModel);
            Regions = regions;
            return regions;
        }

        public ErrorRegion ErrorRegion { get; private set; }

        #endregion

        #region Implementation of IDatabaseServiceViewModel

        public IActionToolRegion<IPluginAction> ActionRegion
        {
            get
            {
                return _actionRegion;
            }
            set
            {
                _actionRegion = value;
                OnPropertyChanged();
            }
        }
        public ISourceToolRegion<IComPluginSource> SourceRegion
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
        public IDotNetInputRegion InputArea
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
        public bool GenerateOutputsVisible
        {
            get
            {
                return _generateOutputsVisible;
            }
            set
            {
                _generateOutputsVisible = value;
                OutputVisibilitySetter.SetGenerateOutputsVisible(ManageServiceInputViewModel.InputArea, ManageServiceInputViewModel.OutputArea, SetRegionVisibility, value);
                OnPropertyChanged();
            }
        }

        public IComPluginService ToModel()
        {
            var pluginServiceDefinition = new ComPluginServiceDefinition
            {
                Source = SourceRegion.SelectedSource,
                Action = ActionRegion.SelectedAction,
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
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
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

        private IComPluginServiceModel Model { get; set; }

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
