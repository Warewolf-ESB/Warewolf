#pragma warning disable
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.WCF;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Studio.Interfaces;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;




namespace Dev2.Activities.Designers2.WCFEndPoint
{
    public class WcfEndPointViewModel : CustomToolWithRegionBase, IWcfEndPointViewModel
    {
        IOutputsToolRegion _outputsRegion;
        IWcfInputRegion _inputArea;
        ISourceToolRegion<IWcfServerSource> _sourceRegion;
        IActionToolRegion<IWcfAction> _actionRegion;

        IErrorInfo _worstDesignError;

        const string DoneText = "Done";
        const string FixText = "Fix";
        const string OutputDisplayName = " - Outputs";

        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        public WcfEndPointViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IWcfServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;

            SetupCommonProperties();
            this.RunViewSetup();
            HelpText = Warewolf.Studio.Resources.Languages.HelpText.Tool_Resources_WCF;
        }

        Guid UniqueID => GetProperty<Guid>();

        void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageWcfServiceInputViewModel(this, Model));
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

        void InitialiseViewModel(IManageWcfServiceInputViewModel manageServiceInputViewModel)
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

        public WcfEndPointViewModel(ModelItem modelItem, IWcfServiceModel model)
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

        public IErrorInfo WorstDesignError
        {
            
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
            AddProperty("Procedure :", ActionRegion.SelectedAction == null ? "" : ActionRegion.SelectedAction.FullName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public IManageWcfServiceInputViewModel ManageServiceInputViewModel { get; set; }

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

        public IErrorInfo NoError { get; set; }

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
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(WcfEndPointViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
        DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(WcfEndPointViewModel), new PropertyMetadata(ErrorType.None));

        bool _generateOutputsVisible;

        public DelegateCommand TestInputCommand { get; set; }

        string Type => GetProperty<string>();


        void FixErrors()
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
            var mainViewModel = CustomContainer.Get<IShellViewModel>();
            mainViewModel?.HelpViewModel?.UpdateHelpText(helpText);
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (SourceRegion == null)
            {
                SourceRegion = new WcfSourceRegion(Model, ModelItem) { SourceChangedAction = () => { OutputsRegion.IsEnabled = false; } };
                regions.Add(SourceRegion);
                ActionRegion = new WcfActionRegion(Model, ModelItem, SourceRegion) { SourceChangedAction = () => { OutputsRegion.IsEnabled = false; } };
                ActionRegion.ErrorsHandler += (sender, list) =>
                {
                    var errorInfos = list.Select(error => new ActionableErrorInfo(new ErrorInfo { ErrorType = ErrorType.Critical, Message = error }, () => { })).ToList();
                    UpdateDesignValidationErrors(errorInfos);
                    Errors = new List<IActionableErrorInfo>(errorInfos);
                };
                regions.Add(ActionRegion);
                InputArea = new WcfInputRegion(ModelItem, ActionRegion);
                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem, true);
                regions.Add(OutputsRegion);
                if (OutputsRegion.Outputs.Count > 0)
                {
                    OutputsRegion.IsEnabled = true;

                }
                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(ActionRegion);
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

        public IActionToolRegion<IWcfAction> ActionRegion
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
        public ISourceToolRegion<IWcfServerSource> SourceRegion
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
       
        public IWcfInputRegion InputArea
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

        public IWcfService ToModel()
        {
            var wcfServiceDefinition = new WcfService()
            {
                Source = SourceRegion.SelectedSource,
                Action = ActionRegion.SelectedAction,
                Inputs = new List<IServiceInput>()
            };
            foreach (var serviceInput in InputArea.Inputs)
            {
                wcfServiceDefinition.Inputs.Add(new ServiceInput(serviceInput.Name, "") { TypeName = serviceInput.TypeName });
            }
            return wcfServiceDefinition;
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
            {
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo() { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
            }
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

        IWcfServiceModel Model { get; set; }

        public void SetRegionVisibility(bool value)
        {
            InputArea.IsEnabled = value;
            OutputsRegion.IsEnabled = value && OutputsRegion.Outputs.Count > 0;
            ErrorRegion.IsEnabled = value;
            SourceRegion.IsEnabled = value;
        }

        #endregion
    }
}
