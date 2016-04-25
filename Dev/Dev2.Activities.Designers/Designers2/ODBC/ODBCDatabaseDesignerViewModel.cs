
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
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Microsoft.Practices.Prism.Commands;
using Warewolf.Core;

// ReSharper disable ExplicitCallerInfoArgument

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
namespace Dev2.Activities.Designers2.ODBC
{
    public class ODBCDatabaseDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>, INotifyPropertyChanged
    {
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;
        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _procedureNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceProcedureNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;

        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };
        //readonly IEventAggregator _eventPublisher;
        IEventAggregator _eventPublisher;
        IDesignValidationService _validationService;
        IErrorInfo _worstDesignError;
        bool _isDisposed;
        const string DoneText = "Done";
        const string FixText = "Fix";
        public string MyCommand { get; set; }
        //readonly bool _isInitializing;
        bool _isInitializing;
        public ODBCDatabaseDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : base(modelItem)
        {
            AddTitleBarMappingToggle();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var dbServiceModel = CustomContainer.CreateInstance<IDbServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            InitialiseViewModel(rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), null, dbServiceModel);
        }

        public ODBCDatabaseDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel,
                                        IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
             : base(modelItem)
        {
            AddTitleBarMappingToggle();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var dbServiceModel = CustomContainer.CreateInstance<IDbServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            InitialiseViewModel(rootModel, environmentRepository, eventPublisher, new AsyncWorker(), null, dbServiceModel);
        }

        public ODBCDatabaseDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManageServiceInputViewModel manageServiceInputViewModel, IDbServiceModel dbServiceModel)
             : base(modelItem)
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(rootModel, environmentRepository, eventPublisher, asyncWorker, manageServiceInputViewModel, dbServiceModel);
        }

        private void InitialiseViewModel(IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManageServiceInputViewModel manageServiceInputViewModel, IDbServiceModel dbServiceModel)
        {
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            ButtonDisplayValue = DoneText;

            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o =>
            {
                FixErrors();
                IsFixed = IsWorstErrorReadOnly;
            });
            DoneCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o => Done());
            DoneCompletedCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o => DoneCompleted());

            InitializeDisplayName();

            InitializeImageSource();

            OutputMappingEnabled = true;

            // When the active environment is not local, we need to get smart around this piece of logic.
            // It is very possible we are treating a remote active as local since we cannot logically assign 
            // an environment id when this is the case as it will fail with source not found since the remote 
            // does not contain localhost's connections ;)
            var activeEnvironment = environmentRepository.ActiveEnvironment;
            if (EnvironmentID == Guid.Empty && !activeEnvironment.IsLocalHostCheck())
            {
                _environment = activeEnvironment;
            }
            else
            {
                var environment = environmentRepository.FindSingle(c => c.ID == EnvironmentID);
                if (environment == null)
                {
                    IList<IEnvironmentModel> environments = EnvironmentRepository.Instance.LookupEnvironments(activeEnvironment);
                    environment = environments.FirstOrDefault(model => model.ID == EnvironmentID);
                }
                _environment = environment;
            }
            TestInputCommand = new DelegateCommand(TestAction, CanTestProcedure);
            InitializeValidationService(_environment);
            InitializeLastValidationMemo(_environment);
            ManageServiceInputViewModel = manageServiceInputViewModel;
            if (_environment != null)
            {
                _isInitializing = true;


                _dbServiceModel = dbServiceModel;
                NewSourceCommand = new DelegateCommand(_dbServiceModel.CreateNewSource);
                EditSourceCommand = new DelegateCommand(() => _dbServiceModel.EditSource(SelectedSource), CanEditSource);
                RefreshActionsCommand = new DelegateCommand(() =>
                {
                    IsRefreshing = true;
                    if (_selectedProcedure != null)
                    {
                        var keepSelectedProcedure = _selectedProcedure.Name;
                        Procedures = _dbServiceModel.GetActions(_selectedSource);
                        if (keepSelectedProcedure != null)
                        {
                            SelectedProcedure = Procedures.FirstOrDefault(action => action.Name == ProcedureName);
                        }
                    }
                    IsRefreshing = false;
                }, CanRefresh);
                var dbSources = _dbServiceModel.RetrieveSources();
                Sources = dbSources.Where(source => source != null && source.Type == enSourceType.ODBC).ToObservableCollection();
                SourceVisible = true;
                if (SourceId != Guid.Empty)
                {
                    SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
                    if (SelectedSource != null)
                    {
                        FriendlySourceNameValue = SelectedSource.Name;
                        if (!string.IsNullOrEmpty(ProcedureName))
                        {
                           
                                Inputs = new List<IServiceInput>();
                               // Outputs = new List<IServiceOutputMapping>();
                                InputsVisible = false;
                                InputsExpanded = false;
                               // OutputsVisible = false;
                               // OutputsExpanded = false;
                               // ProcedureName = "";
                                UpdateLastValidationMemoWithProcedureNotSelectedError();
                                MyCommand = ProcedureName;
                            RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_procedureNotSelectedMessage)).ToList());

                        }
                        else
                        {
                           
                            UpdateLastValidationMemoWithProcedureNotSelectedError();
                        }
                    }
                    else
                    {
                        UpdateLastValidationMemoWithSourceNotFoundError();
                    }
                }
                else
                {
                    UpdateLastValidationMemoWithSourceNotSelectedError();
                }
            }
            InitializeProperties();
            if (IsItemDragged.Instance.IsDragged)
            {
                Expand();
                IsItemDragged.Instance.IsDragged = false;
            }

            if (_environment != null)
            {
                _environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
                AuthorizationServiceOnPermissionsChanged(null, null);
            }
            InitializeResourceModel(_environment);
            if (Inputs != null)
            {
                InputsVisible = true;
                InputsExpanded = true;
            }
            if (Outputs != null)
            {
                OutputsVisible = true;
                OutputsExpanded = true;
                var recordsetItem = Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (recordsetItem != null)
                {
                    RecordsetName = recordsetItem.RecordSetName;
                }
            }
            _isInitializing = false;
        }


        private bool CanRefresh()
        {
            return SelectedSource != null;
        }

        private bool CanEditSource()
        {
            return SelectedSource != null;
        }

        public IManageServiceInputViewModel ManageServiceInputViewModel { get; set; }

        IDatabaseService ToModel()
        {
            DbAction command = new DbAction();
            if (string.IsNullOrEmpty(MyCommand))
            {
                throw new Exception("Action cannot be empty");
            }
            command.Name = MyCommand;
            SelectedProcedure = command;
          
            var databaseService = new DatabaseService
            {
                Action = command,
                Source = SelectedSource,
            };
            databaseService.Inputs = new List<IServiceInput>();
            
            
            return databaseService;
        }

        void TestAction()
        {
            try
            {
                RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_procedureNotSelectedMessage)).ToList());
                Errors = new List<IActionableErrorInfo>();
                var databaseService = ToModel();
                if (string.IsNullOrEmpty(databaseService.Action.Name))
                {
                    throw new Exception("Action cannot be empty");
                }
                ManageServiceInputViewModel.Model = databaseService;
                ManageServiceInputViewModel.Inputs = databaseService.Inputs;
                ManageServiceInputViewModel.TestResults = null;
                ManageServiceInputViewModel.TestAction = () =>
                {
                    ManageServiceInputViewModel.IsTesting = true;
                    try
                    {
                        ManageServiceInputViewModel.TestResults = _dbServiceModel.TestService(ManageServiceInputViewModel.Model);
                        if (ManageServiceInputViewModel.TestResults != null)
                        {
                            ManageServiceInputViewModel.TestResultsAvailable = ManageServiceInputViewModel.TestResults.Rows.Count != 0;
                            TestSuccessful = true;
                            ManageServiceInputViewModel.IsTesting = false;
                        }
                    }
                    catch (Exception e)
                    {
                        ErrorMessage(e);
                        ManageServiceInputViewModel.CloseCommand.Execute(null);
                    }
                };
                ManageServiceInputViewModel.OkAction = () =>
                {
                    Outputs = new ObservableCollection<IServiceOutputMapping>(GetDbOutputMappingsFromTable(ManageServiceInputViewModel.TestResults));
                    if (Outputs != null)
                    {
                        OutputsVisible = true;
                        OutputsExpanded = true;
                    }
                };
                ManageServiceInputViewModel.ShowView();
            }
            catch (Exception e)
            {
                ErrorMessage(e);
            }
        }

        void ErrorMessage(Exception e)
        {
            var errorInfo = new ErrorInfo
            {
                ErrorType = ErrorType.Critical,
                Message = e.Message
            };
            Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfo, () => { }) };
            CanEditMappings = false;
            Inputs = null;
            Outputs = null;
            TestSuccessful = false;
            IsTesting = false;
            TestResults = null;
        }

        List<IServiceOutputMapping> GetDbOutputMappingsFromTable(DataTable testResults)
        {
            List<IServiceOutputMapping> mappings = new List<IServiceOutputMapping>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            RecordsetName = "Command";
            for (int i = 0; i < testResults.Columns.Count; i++)
            {
                var column = testResults.Columns[i];
                var dbOutputMapping = new ServiceOutputMapping(column.ToString(), column.ToString(), RecordsetName);
                mappings.Add(dbOutputMapping);
            }
            return mappings;
        }

        public string RecordsetName
        {
            get
            {
                return _recordsetName;
            }
            set
            {
                if (Outputs != null)
                {
                    foreach (var serviceOutputMapping in Outputs)
                    {
                        if (_recordsetName != null && serviceOutputMapping.RecordSetName != null && serviceOutputMapping.RecordSetName.Equals(_recordsetName))
                        {
                            serviceOutputMapping.RecordSetName = value;
                        }
                    }
                }
                _recordsetName = value;
                OnPropertyChanged("RecordsetName");
            }
        }

        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        public bool TestSuccessful
        {
            get
            {
                return _testSuccessful;
            }
            set
            {
                _testSuccessful = value;
                OnPropertyChanged("TestSuccessful");
            }
        }

        public bool CanEditMappings
        {
            get
            {
                return _canEditMappings;
            }
            set
            {
                _canEditMappings = value;
                OnPropertyChanged("CanEditMappings");
            }
        }

        public DataTable TestResults { get; set; }

        public bool IsTesting
        {
            get
            {
                return _isTesting;
            }
            set
            {
                _isTesting = value;
                OnPropertyChanged("IsTesting");
            }
        }

        public ObservableCollection<IDbSource> Sources { get; set; }

        void OnEnvironmentOnAuthorizationServiceSet(object sender, EventArgs args)
        {
            if (_environment != null && _environment.AuthorizationService != null)
            {
                _environment.AuthorizationService.PermissionsChanged += AuthorizationServiceOnPermissionsChanged;
            }
        }

        void AuthorizationServiceOnPermissionsChanged(object sender, EventArgs eventArgs)
        {
            RemovePermissionsError();

            var hasNoPermission = HasNoPermission();
            if (hasNoPermission)
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
                    FixType = FixType.InvalidPermissions,
                    Message = _serviceExecuteViewPermission
                });
                UpdateLastValidationMemo(memo);
            }
        }

        void RemovePermissionsError()
        {
            var errorInfos = DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
            RemoveErrors(errorInfos.ToList());
        }

        bool HasNoPermission()
        {
            var hasNoPermission = _environment.AuthorizationService != null && _environment.AuthorizationService.GetResourcePermissions(ResourceID) == Permissions.None;
            return hasNoPermission;
        }

        void DoneCompleted()
        {
            IsFixed = true;
        }

        void Done()
        {
            if (!IsWorstErrorReadOnly)
            {
                FixErrors();
            }
        }

        public bool IsFixed
        {
            get { return (bool)GetValue(IsFixedProperty); }
            set { SetValue(IsFixedProperty, value); }
        }

        public static readonly DependencyProperty IsFixedProperty =
            DependencyProperty.Register("IsFixed", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(true));

        public ICommand FixErrorsCommand { get; private set; }

        public ICommand DoneCommand { get; private set; }

        public ICommand DoneCompletedCommand { get; private set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }


        public IContextualResourceModel RootModel { get; private set; }

        public DesignValidationMemo LastValidationMemo { get; private set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; private set; }

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }

        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(ErrorType.None));

        public bool IsWorstErrorReadOnly
        {
            get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
            private set
            {
                if (value)
                {
                    ButtonDisplayValue = DoneText;
                }
                else
                {
                    ButtonDisplayValue = FixText;
                    IsFixed = false;
                }
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty FriendlySourceNameValueProperty =
           DependencyProperty.Register("FriendlySourceNameValue", typeof(string), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(null, OnFriendlyNameChanged));

        private static void OnFriendlyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ODBCDatabaseDesignerViewModel)d;
            if (viewModel != null)
            {
                viewModel.FriendlySourceName = e.NewValue as string;
                viewModel.InitializeProperties();
            }
        }

        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(false));

        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            private set { if (!(bool)GetValue(IsDeletedProperty)) SetValue(IsDeletedProperty, value); }
        }

        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(false));

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            private set { SetValue(IsEditableProperty, value); }
        }

        public bool OutputMappingEnabled
        {
            get { return (bool)GetValue(OutputMappingEnabledProperty); }
            private set { SetValue(OutputMappingEnabledProperty, value); }
        }

        public static readonly DependencyProperty OutputMappingEnabledProperty =
            DependencyProperty.Register("OutputMappingEnabled", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(false));

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            private set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(null));

        public bool ShowParent
        {
            get { return (bool)GetValue(ShowParentProperty); }
            set { SetValue(ShowParentProperty, value); }
        }

        public static readonly DependencyProperty ShowParentProperty =
            DependencyProperty.Register("ShowParent", typeof(bool), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(false, OnShowParentChanged));

        static void OnShowParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ODBCDatabaseDesignerViewModel)d;
            var showParent = (bool)e.NewValue;
            if (showParent)
            {
                viewModel.DoShowParent();
            }
        }

        IErrorInfo WorstDesignError
        {
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

        public string ServiceName { get { return GetProperty<string>(); } }

        string ProcedureName
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
            }
        }

        public string FriendlySourceNameValue
        {
            get { return (string)GetValue(FriendlySourceNameValueProperty); }
            set { SetValue(FriendlySourceNameValueProperty, value); }
        }

        public string FriendlySourceName
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
            }
        }

        public string Type { get { return GetProperty<string>(); } }
        // ReSharper disable InconsistentNaming
        Guid EnvironmentID { get { return GetProperty<Guid>(); } }
        Guid ResourceID { get { return GetProperty<Guid>(); } }
        Guid UniqueID { get { return GetProperty<Guid>(); } }
        // ReSharper restore InconsistentNaming

        public string ButtonDisplayValue
        {
            get { return (string)GetValue(ButtonDisplayValueProperty); }
            set { SetValue(ButtonDisplayValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonDisplayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(ODBCDatabaseDesignerViewModel), new PropertyMetadata(default(string)));
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        IEnvironmentModel _environment;
        private IDbServiceModel _dbServiceModel;
        private IDbSource _selectedSource;
        private bool _actionVisible;
        private ICollection<IDbAction> _procedures;
        private IDbAction _selectedProcedure;
        private ICollection<IServiceInput> _inputs;
        private bool _inputsVisible;
        private bool _inputsExpanded;
        private bool _outputsVisible;
        private bool _outputsExpanded;
        private bool _isTesting;
        private bool _canEditMappings;
        private bool _testSuccessful;
        private string _errorText;
        private string _recordsetName;
        bool _isRefreshing;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        public override void Validate()
        {
            Errors = new List<IActionableErrorInfo>();
            if (HasNoPermission())
            {
                var errorInfos = DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfos.ToList()[0], () => { }) };
            }
            else
            {
                RemovePermissionsError();
            }
        }


        #region Overrides of ActivityDesignerViewModel

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            // AddTitleBarMappingToggle() binds Mapping button to ShowLarge property
            if (propertyName == ShowLargeProperty.Name)
            {
                if (!isChecked)
                {
                }
            }
        }

        #endregion

        void InitializeDisplayName()
        {
            var serviceName = ServiceName;
            if (!string.IsNullOrEmpty(serviceName))
            {
                var displayName = DisplayName;
                if (!string.IsNullOrEmpty(displayName) && displayName.Contains("Dsf"))
                {
                    DisplayName = serviceName;
                }
            }
        }

        void InitializeLastValidationMemo(IEnvironmentModel environmentModel)
        {
            var uniqueId = UniqueID;
            var designValidationMemo = new DesignValidationMemo
            {
                InstanceID = uniqueId,
                ServiceID = ResourceID,
                IsValid = RootModel != null && (RootModel.Errors != null && RootModel.Errors.Count == 0)
            };
            if (RootModel != null)
            {
                var errorInfos = RootModel.GetErrors(uniqueId);
                if (errorInfos != null)
                {
                    designValidationMemo.Errors.AddRange(errorInfos.Cast<ErrorInfo>());
                }
            }

            if (environmentModel == null)
            {
                designValidationMemo.IsValid = false;
                designValidationMemo.Errors.Add(new ErrorInfo
                {
                    ErrorType = ErrorType.Critical,
                    FixType = FixType.None,
                    InstanceID = uniqueId,
                    Message = "Server source not found. This service will not execute."
                });
            }

            UpdateLastValidationMemo(designValidationMemo);
        }

        void InitializeResourceModel(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null)
            {
                if (!environmentModel.IsLocalHost && !environmentModel.HasLoadedResources)
                {
                    environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError, false);
                    environmentModel.ResourcesLoaded += OnEnvironmentModel_ResourcesLoaded;
                }
                InitializeResourceModelSync(environmentModel);
            }
        }

        // ReSharper disable InconsistentNaming
        void OnEnvironmentModel_ResourcesLoaded(object sender, ResourcesLoadedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
        }


        private void InitializeResourceModelSync(IEnvironmentModel environmentModel)
        {
            if (!environmentModel.IsConnected) // if we are not connected then just verify connection and return
            {
                environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError);
            }
            CheckSourceMissing();
        }

        bool CheckSourceMissing()
        {
            return true;
        }

        Guid SourceId
        {
            get
            {
                return GetProperty<Guid>();
            }
            set
            {
                SetProperty(value);
            }
        }

        void InitializeValidationService(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null && environmentModel.Connection != null && environmentModel.Connection.ServerEvents != null)
            {
                _validationService = new DesignValidationService(environmentModel.Connection.ServerEvents);
                _validationService.Subscribe(UniqueID, a => UpdateLastValidationMemo(a));
            }
        }

        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", FriendlySourceName);
            AddProperty("Type :", Type);
            AddProperty("Procedure :", ProcedureName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        void InitializeImageSource()
        {
            ImageSource = GetIconPath();
        }

        public string ResourceType
        {
            get;
            set;
        }
        public IDbSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(value, _selectedSource))
                {
                    IsRefreshing = true;
                    Errors = new List<IActionableErrorInfo>();
                    _selectedSource = value;
                    try
                    {
                       
                        if (!_isInitializing)
                        {
                            Inputs = new List<IServiceInput>();
                            Outputs = new List<IServiceOutputMapping>();
                            RecordsetName = "";
                            InputsVisible = true;
                            InputsExpanded = false;
                            OutputsVisible = false;
                            OutputsExpanded = false;
                        }
                      //  ActionVisible = Procedures.Count != 0;
                        ActionVisible =true;
                       
                      //  Procedures = _dbServiceModel.GetActions(_selectedSource);
                        ViewModelUtils.RaiseCanExecuteChanged(TestInputCommand);
                        SourceId = _selectedSource.Id;
                        
                            RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_sourceNotSelectedMessage)).ToList());
                            FriendlySourceNameValue = _selectedSource.Name;
                        
                    }
                    catch (Exception e)
                    {
                        Procedures = new List<IDbAction>();
                        SelectedProcedure = null;
                        ActionVisible = false;
                        var errorInfo = new ErrorInfo
                        {
                            ErrorType = ErrorType.Critical,
                            Message = e.Message
                        };
                        Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfo, () => { }) };
                    }
                }
                IsRefreshing = false;
                ViewModelUtils.RaiseCanExecuteChanged(EditSourceCommand);
                ViewModelUtils.RaiseCanExecuteChanged(RefreshActionsCommand);
            }
        }
        public ICommand EditSourceCommand
        {
            get;
            set;
        }
        public ICommand NewSourceCommand
        {
            get;
            set;
        }
        public bool SourceVisible
        {
            get;
            set;
        }
        public bool ActionVisible
        {
            get
            {
                return _actionVisible;
            }
            set
            {
                _actionVisible = value;
                OnPropertyChanged("ActionVisible");
            }
        }
        public ICollection<IDbAction> Procedures
        {
            get
            {
                return _procedures;
            }
            set
            {
                _procedures = value;
                OnPropertyChanged("Procedures");
            }
        }
        public IDbAction SelectedProcedure
        {
            get
            {
                return _selectedProcedure;
            }
            set
            {
                
                if (!Equals(value, _selectedProcedure))
                {
                    _selectedProcedure = value;
                    if (_selectedProcedure != null)
                    {
                        if (!_isInitializing)
                        {
                            Outputs = new List<IServiceOutputMapping>();
                            RecordsetName = "";
                            Inputs = _selectedProcedure.Inputs;
                        }
                        RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_procedureNotSelectedMessage)).ToList());
                    }
                    OutputsVisible = false;
                    OutputsExpanded = false;
                    ProcedureName = _selectedProcedure != null ? _selectedProcedure.Name : "";
                    InitializeProperties();
                   ViewModelUtils.RaiseCanExecuteChanged(TestInputCommand);
                    OnPropertyChanged("SelectedProcedure");
                }
            }
        }
        public bool IsRefreshing
        {
            get
            {
                return _isRefreshing;
            }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
        public ICommand RefreshActionsCommand
        {
            get;
            set;
        }
        public bool AdditionalInfoVisible
        {
            get;
            set;
        }

        bool CanTestProcedure()
        {
            return SelectedSource != null;
        }

        public bool InputsVisible
        {
            get
            {
                return _inputsVisible;
            }
            set
            {
                _inputsVisible = value;
                OnPropertyChanged("InputsVisible");
            }
        }
        public bool InputsExpanded
        {
            get
            {
                return _inputsExpanded;
            }
            set
            {
                _inputsExpanded = value;
                OnPropertyChanged("InputsExpanded");
            }
        }
        public bool OutputsVisible
        {
            get
            {
                return _outputsVisible;
            }
            set
            {
                _outputsVisible = value;
                OnPropertyChanged("OutputsVisible");
            }
        }
        public bool OutputsExpanded
        {
            get
            {
                return _outputsExpanded;
            }
            set
            {
                _outputsExpanded = value;
                OnPropertyChanged("OutputsExpanded");
            }
        }
        public ICollection<IServiceInput> Inputs
        {
            get
            {
                var serviceInputs = GetProperty<ICollection<IServiceInput>>();
                return serviceInputs;
            }
            set
            {
                if (!Equals(value, _inputs))
                {
                    _inputs = value;
                    InputsVisible = true;
                    InputsExpanded = true;
                    SetProperty(value);
                    OnPropertyChanged("Inputs");
                }
            }
        }
        public ICommand TestInputCommand
        {
            get;
            set;
        }

        public ICollection<IServiceOutputMapping> Outputs
        {
            get
            {
                var serviceOutputMappings = GetProperty<ICollection<IServiceOutputMapping>>();
                return serviceOutputMappings;
            }
            set
            {
                OutputsVisible = true;
                OutputsExpanded = true;
                SetProperty(value);
                OnPropertyChanged("Outputs");
            }
        }

        string GetIconPath()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.DbService.ToString();
            return "DatabaseService-32";
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        void DoShowParent()
        {
            if (!IsDeleted)
            {
                _eventPublisher.Publish(new EditActivityMessage(ModelItem, EnvironmentID, null));
            }
        }

        void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource = true)
        {
            LastValidationMemo = memo;

            CheckIsDeleted(memo);

            UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == UniqueID && info.ErrorType != ErrorType.None));
            if (SourceId == Guid.Empty)
            {
                if (checkSource && CheckSourceMissing())
                {
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

        void UpdateLastValidationMemoWithProcedureNotSelectedError()
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
                Message = _procedureNotSelectedMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        void UpdateLastValidationMemoWithSourceNotSelectedError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = UniqueID,
                IsValid = false
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = UniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.None,
                Message = _sourceNotSelectedMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        void UpdateLastValidationMemoWithOfflineError(ConnectResult result)
        {
            Dispatcher.Invoke(() =>
            {
                switch (result)
                {
                    case ConnectResult.Success:

                        break;

                    case ConnectResult.ConnectFailed:
                    case ConnectResult.LoginFailed:
                        var uniqueId = UniqueID;
                        var memo = new DesignValidationMemo
                        {
                            InstanceID = uniqueId,
                            IsValid = false,
                        };
                        memo.Errors.Add(new ErrorInfo
                        {
                            InstanceID = uniqueId,
                            ErrorType = ErrorType.Warning,
                            FixType = FixType.None,
                            Message = result == ConnectResult.ConnectFailed
                                          ? "Server is offline. " + _serviceExecuteOnline
                                          : "Server login failed. " + _serviceExecuteLoginPermission
                        });
                        UpdateLastValidationMemo(memo);
                        break;
                }
            });
        }

        void CheckIsDeleted(DesignValidationMemo memo)
        {
            var error = memo.Errors.FirstOrDefault(c => c.FixType == FixType.Delete);
            IsDeleted = error != null;
            IsEditable = !IsDeleted;
            if (IsDeleted)
            {
                while (memo.Errors.Count > 1)
                {
                    error = memo.Errors.FirstOrDefault(c => c.FixType != FixType.Delete);
                    if (error != null)
                    {
                        memo.Errors.Remove(error);
                    }
                }
            }
        }


        // PBI 6690 - 2013.07.04 - TWR : added
        void FixErrors()
        {
            if (WorstDesignError.ErrorType == ErrorType.None || WorstDesignError.FixData == null)
            {
            }
        }

        #region RemoveWorstError

        void RemoveError(IErrorInfo worstError)
        {
            DesignValidationErrors.Remove(worstError);
            RootModel.RemoveError(worstError);
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        void RemoveErrors(IList<IErrorInfo> worstErrors)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            worstErrors.ToList().ForEach(RemoveError);
            UpdateWorstError();
        }

        #endregion

        #region UpdateWorstError

        void UpdateWorstError()
        {
            if (DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
                if (!RootModel.HasErrors)
                {
                    RootModel.IsValid = true;
                }
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

        #endregion

        #region UpdateDesignValidationErrors

        void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            RootModel.ClearErrors();
            foreach (var error in errors)
            {
                DesignValidationErrors.Add(error);
                RootModel.AddError(error);
            }
            UpdateWorstError();
        }

        #endregion

        #region Implementation of IDisposable

        public void Handle(UpdateResourceMessage message)
        {
            if (message != null && message.ResourceModel != null)
            {
                //                if(message.ResourceModel.ID == ResourceID)
                //                {
                //                    InitializeMappings();
                //                }
                if (SourceId != Guid.Empty && SourceId == message.ResourceModel.ID)
                {

                    IErrorInfo sourceNotAvailableMessage = DesignValidationErrors.FirstOrDefault(info => info.Message == _sourceNotFoundMessage);
                    if (sourceNotAvailableMessage != null)
                    {
                        RemoveError(sourceNotAvailableMessage);
                        UpdateWorstError();
                    }
                }
            }
        }

        ~ODBCDatabaseDesignerViewModel()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        protected override void OnDispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.OnDispose();
        }

        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_validationService != null)
                    {
                        _validationService.Dispose();
                    }
                    if (_environment != null)
                    {
                        _environment.AuthorizationServiceSet -= OnEnvironmentOnAuthorizationServiceSet;
                    }
                }

                // Dispose unmanaged resources.

                _isDisposed = true;
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        [ExcludeFromCodeCoverage]
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }
    }
}
