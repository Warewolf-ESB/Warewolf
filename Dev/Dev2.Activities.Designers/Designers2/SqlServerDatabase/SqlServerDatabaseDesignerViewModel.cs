﻿using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Designers2.Core.ActionRegion;
using Dev2.Activities.Designers2.Core.InputRegion;
using Dev2.Activities.Designers2.Core.Source;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.ToolBase.Database;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Providers.Errors;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Dev2.Activities.Designers2.SqlServerDatabase
{
    public class SqlServerDatabaseDesignerViewModel : CustomToolWithRegionBase, IDatabaseServiceViewModel
    {
        private IOutputsToolRegion _outputsRegion;
        private IDatabaseInputRegion _inputArea;
        private ISourceToolRegion<IDbSource> _sourceRegion;
        private IActionToolRegion<IDbAction> _actionRegion;

        private IErrorInfo _worstDesignError;

        const string DoneText = "Done";
        const string FixText = "Fix";
        const string OutputDisplayName = " - Outputs";
        // ReSharper disable UnusedMember.Local
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;

        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _methodNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.PluginServiceMethodNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;
        // ReSharper restore UnusedMember.Local

        public SqlServerDatabaseDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var model = CustomContainer.CreateInstance<IDbServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = model;

            SetupCommonProperties();
        }

        Guid UniqueID { get { return GetProperty<Guid>(); } }
        private void SetupCommonProperties()
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(new ManageDatabaseServiceInputViewModel(this, Model));
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

        private void InitialiseViewModel(IManageDatabaseInputViewModel manageServiceInputViewModel)
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
            InitializeImageSource();
            OutputsRegion.OutputMappingEnabled = true;
            TestInputCommand = new DelegateCommand(TestProcedure);

            InitializeProperties();

            if (OutputsRegion != null && OutputsRegion.IsVisible)
            {
                var recordsetItem = OutputsRegion.Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (recordsetItem != null)
                {
                    OutputsRegion.IsVisible = true;
                }
            }
            ReCalculateHeight();
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

        public SqlServerDatabaseDesignerViewModel(ModelItem modelItem, IDbServiceModel model)
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
            if (!OutputsRegion.IsVisible)
            {
                Errors = new List<IActionableErrorInfo>() { new ActionableErrorInfo() { Message = "Database get must be validated before minimising" } };
            }
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
                    WorstError = value == null ? ErrorType.None : value.ErrorType;
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
            AddProperty("Procedure :", ActionRegion.SelectedAction == null ? "" : ActionRegion.SelectedAction.Name);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public IManageDatabaseInputViewModel ManageServiceInputViewModel { get; set; }

        public void TestProcedure()
        {
            if (ActionRegion.SelectedAction != null)
            {
                var service = ToModel();
                ManageServiceInputViewModel.InputArea.Inputs = service.Inputs;
                ManageServiceInputViewModel.Model = service;

                GenerateOutputsVisible = true;
                ManageServiceInputViewModel.SetInitialVisibility();
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
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(SqlServerDatabaseDesignerViewModel), new PropertyMetadata(false));

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
        DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(SqlServerDatabaseDesignerViewModel), new PropertyMetadata(ErrorType.None));

        bool _generateOutputsVisible;

        public DelegateCommand TestInputCommand { get; set; }

        private string Type { get { return GetProperty<string>(); } }
        // ReSharper disable InconsistentNaming

        [ExcludeFromCodeCoverage]
        private void FixErrors()
        {
        }

        void InitializeImageSource()
        {
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        public string ButtonDisplayValue { get; set; }

        [ExcludeFromCodeCoverage]
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        public override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (SourceRegion == null)
            {
                SourceRegion = new DatabaseSourceRegion(Model, ModelItem,enSourceType.SqlDatabase) { SourceChangedAction = () => { OutputsRegion.IsVisible = false; } };
                regions.Add(SourceRegion);
                ActionRegion = new DbActionRegion(Model, ModelItem, SourceRegion) { SourceChangedAction = () => { OutputsRegion.IsVisible = false; } };
                regions.Add(ActionRegion);
                InputArea = new DatabaseInputRegion(ModelItem, ActionRegion);
                regions.Add(InputArea);
                OutputsRegion = new OutputsRegion(ModelItem);
                regions.Add(OutputsRegion);
                if (OutputsRegion.Outputs.Count > 0)
                {
                    OutputsRegion.IsVisible = true;

                }
                ErrorRegion = new ErrorRegion();
                regions.Add(ErrorRegion);
                SourceRegion.Dependants.Add(InputArea);
                SourceRegion.Dependants.Add(OutputsRegion);
            }
            regions.Add(ManageServiceInputViewModel);
            Regions = regions;
            foreach (var toolRegion in regions)
            {
                toolRegion.HeightChanged += toolRegion_HeightChanged;
            }
            ReCalculateHeight();
            return regions;
        }
        public ErrorRegion ErrorRegion { get; private set; }

        void toolRegion_HeightChanged(object sender, IToolRegion args)
        {
            ReCalculateHeight();
            if (TestInputCommand != null)
            {
                TestInputCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Implementation of IDatabaseServiceViewModel

        public IActionToolRegion<IDbAction> ActionRegion
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
        public ISourceToolRegion<IDbSource> SourceRegion
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
        public IDatabaseInputRegion InputArea
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
                if (value)
                {
                    ManageServiceInputViewModel.InputArea.IsVisible = true;
                    ManageServiceInputViewModel.OutputArea.IsVisible = false;
                    SetRegionVisibility(false);

                }
                else
                {
                    ManageServiceInputViewModel.InputArea.IsVisible = false;
                    ManageServiceInputViewModel.OutputArea.IsVisible = false;
                    SetRegionVisibility(true);
                }

                OnPropertyChanged();
                ReCalculateHeight();
            }
        }

        public IDatabaseService ToModel()
        {
            var databaseService = new DatabaseService
            {
                Source = SourceRegion.SelectedSource,
                Action = ActionRegion.SelectedAction,
                Inputs = new List<IServiceInput>()
            };
            foreach (var serviceInput in InputArea.Inputs)
            {
                databaseService.Inputs.Add(new ServiceInput(serviceInput.Name, ""));
            }
            return databaseService;
        }

        public void ErrorMessage(Exception exception, bool hasError)
        {
            Errors = new List<IActionableErrorInfo>();
            if (hasError)
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(new ErrorInfo() { ErrorType = ErrorType.Critical, FixData = "", FixType = FixType.None, Message = exception.Message, StackTrace = exception.StackTrace }, () => { }) };
        }

        public void ValidateTestComplete()
        {
            OutputsRegion.IsVisible = true;
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

        private IList<IServiceInput> InputsFromModel()
        {
            var dt = new List<IServiceInput>();
            foreach (var nameValue in InputArea.Inputs)
            {
                GetValue(nameValue.Name, dt);
                GetValue(nameValue.Value, dt);
            }
            return dt;
        }

        private static void GetValue(string s, List<IServiceInput> dt)
        {
            var exp = WarewolfDataEvaluationCommon.parseLanguageExpressionWithoutUpdate(s);
            if (exp.IsComplexExpression)
            {
                var item = ((LanguageAST.LanguageExpression.ComplexExpression)exp).Item;
                var vals = item.Where(a => a.IsRecordSetExpression || a.IsScalarExpression).Select(WarewolfDataEvaluationCommon.languageExpressionToString);
                dt.AddRange(vals.Select(a => new ServiceInput(a, "")));
            }
            if (exp.IsScalarExpression)
            {

                dt.Add(new ServiceInput(s, ""));
            }
            if (exp.IsRecordSetExpression)
            {

                dt.Add(new ServiceInput(s, ""));
            }
        }

        private IDbServiceModel Model { get; set; }

        void SetRegionVisibility(bool value)
        {
            InputArea.IsVisible = value;
            OutputsRegion.IsVisible = value && OutputsRegion.Outputs.Count > 0;
            ErrorRegion.IsVisible = value;
            SourceRegion.IsVisible = value;
        }

        public override void ReCalculateHeight()
        {
            if (_regions != null)
            {
                bool isInputVisible = false;
                foreach (var toolRegion in _regions)
                {
                    if (toolRegion.ToolRegionName == "DatabaseInputRegion")
                    {
                        isInputVisible = toolRegion.IsVisible;
                    }
                }

                DesignMinHeight = _regions.Where(a => a.IsVisible).Sum(a => a.MinHeight);
                DesignMaxHeight = _regions.Where(a => a.IsVisible).Sum(a => a.MaxHeight);
                DesignHeight = _regions.Where(a => a.IsVisible).Sum(a => a.CurrentHeight);

                if (isInputVisible && !GenerateOutputsVisible)
                {
                    DesignMaxHeight += 30;
                    DesignHeight += 30;
                    DesignMinHeight += 30;
                }
            }
        }

        #endregion
    }
}
