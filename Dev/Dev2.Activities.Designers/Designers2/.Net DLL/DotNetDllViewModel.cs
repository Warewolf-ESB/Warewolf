
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
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Communication;
using Dev2.Interfaces;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Threading;
using Warewolf.Core;

namespace Dev2.Activities.Designers2.Net_DLL
{
    public class DotNetDllViewModel : CustomToolViewModelBase<IPluginSource>, IHandle<UpdateResourceMessage>
    {
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;
        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _methodNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.PluginServiceMethodNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;

        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };
        IEventAggregator _eventPublisher;

        IDesignValidationService _validationService;
        IErrorInfo _worstDesignError;
        bool _isDisposed;
        const string DoneText = "Done";
        const string FixText = "Fix";
        bool _isInitializing;
        public DotNetDllViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : base(modelItem)
        {
            AddTitleBarMappingToggle();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var pluginServiceModel = CustomContainer.CreateInstance<IPluginServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            InitialiseViewModel(rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), new ManagePluginServiceInputViewModel(), pluginServiceModel);
        }

        public DotNetDllViewModel(ModelItem modelItem, IContextualResourceModel rootModel,
                                        IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            AddTitleBarMappingToggle();
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var pluginServiceModel = CustomContainer.CreateInstance<IPluginServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            InitialiseViewModel(rootModel, environmentRepository, eventPublisher, new AsyncWorker(), new ManagePluginServiceInputViewModel(), pluginServiceModel);
        }

        public DotNetDllViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManagePluginServiceInputViewModel manageServiceInputViewModel, IPluginServiceModel pluginServiceModel)
            : base(modelItem)
        {
            AddTitleBarMappingToggle();
            InitialiseViewModel(rootModel, environmentRepository, eventPublisher, asyncWorker, manageServiceInputViewModel, pluginServiceModel);
        }

        private void InitialiseViewModel(IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManagePluginServiceInputViewModel manageServiceInputViewModel, IPluginServiceModel pluginServiceModel)
        {
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            ButtonDisplayValue = DoneText;

            LabelWidth = 70;
            ResetHeightValues(DefaultToolHeight);
            SetInitialHeight();
            TestComplete = false;
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new DelegateCommand(o =>
            {
                FixErrors();
            });

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
            TestInputCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestAction, CanTestProcedure);
            InitializeValidationService(_environment);
            InitializeLastValidationMemo(_environment);
            ManageServiceInputViewModel = manageServiceInputViewModel;
            if (_environment != null)
            {
                _isInitializing = true;


                _dllModel = pluginServiceModel;
                NewSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(_dllModel.CreateNewSource);
                EditSourceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() => _dllModel.EditSource(SelectedSource), CanEditSource);
                RefreshActionsCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
                {
                    IsRefreshing = true;
                    if (_selectedMethod != null)
                    {
                        var keepSelectedProcedure = _selectedMethod.Method;
                        Methods = _dllModel.GetActions(_selectedSource, _selectedNamespace);
                        if (keepSelectedProcedure != null)
                        {
                            SelectedMethod = Methods.FirstOrDefault(action => action.Method == MethodName);
                        }
                    }
                    IsRefreshing = false;
                }, CanRefresh);
                RefreshNamespaceCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(() =>
                {
                    IsNamespaceRefreshing = true;
                    if (_selectedMethod != null)
                    {
                        var keepSelectedProcedure = _selectedNamespace;
                        Namespaces = _dllModel.GetNameSpaces(_selectedSource);
                        if (keepSelectedProcedure != null)
                        {
                            SelectedNamespace = Namespaces.FirstOrDefault(action => action.FullName == SelectedNamespace.FullName);
                        }
                    }
                    IsNamespaceRefreshing = false;
                }, CanRefresh);
                var pluginSources = _dllModel.RetrieveSources().OrderBy(source => source.Name);
                Sources = pluginSources.ToObservableCollection();
                SourceVisible = true;
                if (SourceId != Guid.Empty)
                {
                    SelectedSource = Sources.FirstOrDefault(source => source.Id == SourceId);
                    if (SelectedSource != null)
                    {
                        FriendlySourceNameValue = SelectedSource.Name;
                        if(Method != null)
                        {
                            var method = Method.Method;
                            if(!string.IsNullOrEmpty(method))
                            {
                                if(Methods != null)
                                {
                                    SelectedMethod = Methods.FirstOrDefault(action => action.Method == method);
                                    if(SelectedMethod == null)
                                    {
                                        Inputs = new List<IServiceInput>();
                                        Outputs = new List<IServiceOutputMapping>();
                                        UpdateLastValidationMemoWithProcedureNotSelectedError();
                                    }
                                }
                                else
                                {
                                    UpdateLastValidationMemoWithProcedureNotSelectedError();
                                }
                            }
                            else
                            {
                                UpdateLastValidationMemoWithProcedureNotSelectedError();
                            }
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
            }
            if (Outputs != null)
            {
                TestComplete = Outputs.Count >= 1 || PreviousTestComplete;
                OutputsHasItems = Outputs.Count >= 1;
                var recordsetItem = Outputs.FirstOrDefault(mapping => !string.IsNullOrEmpty(mapping.RecordSetName));
                if (recordsetItem != null)
                {
                    RecordsetName = recordsetItem.RecordSetName;
                }
            }
            _isInitializing = false;
            SetToolHeight();
            ResetHeightValues(DefaultToolHeight);
        }

        private bool CanRefresh()
        {
            return SelectedSource != null;
        }

        private bool CanEditSource()
        {
            return SelectedSource != null;
        }

        // ReSharper disable UnusedAutoPropertyAccessor.Local
        // ReSharper restore UnusedAutoPropertyAccessor.Local

        IPluginService ToModel()
        {
            var pluginServiceDefinition = new PluginServiceDefinition
            {
                Source = SelectedSource,
                Action = SelectedMethod,
                Inputs = new List<IServiceInput>()
            };
            foreach (var serviceInput in Inputs)
            {
                pluginServiceDefinition.Inputs.Add(new ServiceInput(serviceInput.Name, ""){TypeName = serviceInput.TypeName});
            }
            return pluginServiceDefinition;
        }
        protected IManagePluginServiceInputViewModel ManageServiceInputViewModel { get; set; }
        public void TestAction()
        {
            try
            {
                Errors = new List<IActionableErrorInfo>();
                var service = ToModel();
                ManageServiceInputViewModel.Model = service;
                ManageServiceInputViewModel.Inputs = service.Inputs;
                ManageServiceInputViewModel.TestResults = null;
                ManageServiceInputViewModel.TestAction = () =>
                {
                    ManageServiceInputViewModel.IsTesting = true;
                    try
                    {
                        ManageServiceInputViewModel.TestResults = _dllModel.TestService(ManageServiceInputViewModel.Model);
                        var serializer = new Dev2JsonSerializer();
                        var responseService = serializer.Deserialize<RecordsetListWrapper>(ManageServiceInputViewModel.TestResults);
                        if (responseService.RecordsetList.Any(recordset => recordset.HasErrors))
                        {
                            var errorMessage = string.Join(Environment.NewLine, responseService.RecordsetList.Select(recordset => recordset.ErrorMessage));
                            throw new Exception(errorMessage);
                        }

                        ManageServiceInputViewModel.Description = responseService.Description;
                        // ReSharper disable MaximumChainedReferences
                        var outputMapping = responseService.RecordsetList.SelectMany(recordset => recordset.Fields, (recordset, recordsetField) =>
                        {
                            RecordsetName = recordset.Name;
                            var serviceOutputMapping = new ServiceOutputMapping(recordsetField.Name, recordsetField.Alias, recordset.Name);
                            return serviceOutputMapping;
                        }).Cast<IServiceOutputMapping>().ToList();
                        // ReSharper restore MaximumChainedReferences

                        ManageServiceInputViewModel.OutputMappings = outputMapping;
                        if (ManageServiceInputViewModel.TestResults != null)
                        {
                            ManageServiceInputViewModel.TestResultsAvailable = ManageServiceInputViewModel.TestResults != null;
                            TestSuccessful = true;
                            ManageServiceInputViewModel.IsTesting = false;
                        }
                    }
                    catch (Exception e)
                    {
                        PreviousTestComplete = false;
                        TestComplete = false;
                        ErrorMessage(e);
                        ManageServiceInputViewModel.IsTesting = false;
                        ManageServiceInputViewModel.CloseCommand.Execute(null);
                    }
                };
                ManageServiceInputViewModel.OkAction = () =>
                {
                    Outputs = new ObservableCollection<IServiceOutputMapping>(ManageServiceInputViewModel.OutputMappings);
                    OutputDescription = ManageServiceInputViewModel.Description;
                };
                ManageServiceInputViewModel.CloseView();
                ManageServiceInputViewModel.ShowView();
                if (ManageServiceInputViewModel.OkSelected)
                {
                    ValidateTestComplete();
                }
                SetToolHeight();
                ResetHeightValues(DefaultToolHeight);
            }
            catch (Exception e)
            {
                ErrorMessage(e);
            }
        }

        public IOutputDescription OutputDescription
        {
            get
            {
                return GetProperty<IOutputDescription>();
            }
            set
            {
                SetProperty(value);
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

        public override ObservableCollection<IPluginSource> Sources { get; set; }

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

        public ICommand FixErrorsCommand { get; private set; }

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
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(DotNetDllViewModel), new PropertyMetadata(ErrorType.None));

        public bool IsWorstErrorReadOnly
        {
            get { return (bool)GetValue(IsWorstErrorReadOnlyProperty); }
            private set
            {
                ButtonDisplayValue = value ? DoneText : FixText;
                SetValue(IsWorstErrorReadOnlyProperty, value);
            }
        }

        public static readonly DependencyProperty FriendlySourceNameValueProperty =
           DependencyProperty.Register("FriendlySourceNameValue", typeof(string), typeof(DotNetDllViewModel), new PropertyMetadata(null, OnFriendlyNameChanged));

        private static void OnFriendlyNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DotNetDllViewModel)d;
            if (viewModel != null)
            {
                viewModel.FriendlySourceName = e.NewValue as string;
                viewModel.InitializeProperties();
            }
        }

        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(DotNetDllViewModel), new PropertyMetadata(false));

        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            private set { if (!(bool)GetValue(IsDeletedProperty)) SetValue(IsDeletedProperty, value); }
        }

        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(DotNetDllViewModel), new PropertyMetadata(false));

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
            DependencyProperty.Register("OutputMappingEnabled", typeof(bool), typeof(DotNetDllViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(DotNetDllViewModel), new PropertyMetadata(false));

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            private set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(DotNetDllViewModel), new PropertyMetadata(null));

        public bool ShowParent
        {
            get { return (bool)GetValue(ShowParentProperty); }
            set { SetValue(ShowParentProperty, value); }
        }

        public static readonly DependencyProperty ShowParentProperty =
            DependencyProperty.Register("ShowParent", typeof(bool), typeof(DotNetDllViewModel), new PropertyMetadata(false, OnShowParentChanged));

        static void OnShowParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (DotNetDllViewModel)d;
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

        string MethodName
        {
            get
            {
                if (_selectedMethod != null)
                {
                    return _selectedMethod.Method;
                }
                return "";
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
        public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(DotNetDllViewModel), new PropertyMetadata(default(string)));
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        IEnvironmentModel _environment;
        private IPluginServiceModel _dllModel;
        private IPluginSource _selectedSource;
        private bool _actionVisible;
        private ICollection<IPluginAction> _methods;
        private IPluginAction _selectedMethod;
        private bool _isTesting;
        private bool _canEditMappings;
        private bool _testSuccessful;
        private string _errorText;
        bool _isRefreshing;
        private INamespaceItem _selectedNamespace;
        private ICollection<INamespaceItem> _namespaces;
        private bool _namespaceVisible;
        bool _isNamespaceRefreshing;
        private IPluginAction _previousMethod;
        private ICollection<IServiceInput> _previousInputs;
        private string _previousRecset;
        private ICollection<IServiceOutputMapping> _previousOutputs;
        private INamespaceItem _previosNamespace;
        private ICollection<IPluginAction> _previousMethods;
        private IPluginSource _previousSource;
        private ICollection<INamespaceItem> _previosNamespaces;
        double _toolHeight = 210;
        double _maxToolHeight = 210;
        const double DefaultToolHeight = 210;

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
            if (!TestComplete)
            {
                ErrorMessage(new Exception("Please select all relevant fields and validate before selecting the Done button."));
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
                IsValid = RootModel.Errors == null || RootModel.Errors.Count == 0
            };
            var errors = RootModel.GetErrors(uniqueId).Cast<ErrorInfo>();
            designValidationMemo.Errors.AddRange(errors);

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
            AddProperty("Method :", MethodName);
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

        public ICollection<INamespaceItem> Namespaces
        {
            get
            {
                return _namespaces;
            }
            set
            {
                _namespaces = value;
                if (Namespace != null)
                {
                    SelectedNamespace = Namespaces.FirstOrDefault(a => a.FullName == Namespace.FullName);
                }
                OnPropertyChanged("Namespaces");
            }
        }
        public INamespaceItem SelectedNamespace
        {
            get
            {
                return _selectedNamespace;
            }
            set
            {
                if (value != null && value == _previosNamespace)
                {
                    Errors = new List<IActionableErrorInfo>();
                    _selectedNamespace = value;
                    Methods = _previousMethods;
                    Outputs = _previousOutputs;
                    SelectedMethod = _previousMethod;
                    ActionVisible = Methods.Count != 0;
                    InputsVisible = SelectedMethod != null;
                    SetToolHeight();
                    ResetHeightValues(DefaultToolHeight);
                }
                else if (!Equals(value, _selectedNamespace))
                {
                    TestComplete = false;
                    IsRefreshing = true;
                    InputsVisible = false;
                    Errors = new List<IActionableErrorInfo>();

                    _previosNamespace = _selectedNamespace;
                    _previousMethods = Methods;
                    _previousMethod = _selectedMethod;
                    _previousInputs = _inputs;
                    _previousOutputs = Outputs;
                    _previousRecset = _recordsetName;

                    _selectedNamespace = value;
                    Namespace = value;
                    try
                    {
                        Methods = _dllModel.GetActions(_selectedSource, SelectedNamespace).OrderBy(action => action.Method).ToList();
                        if (!_isInitializing)
                        {
                            Inputs = new List<IServiceInput>();
                            Outputs = new List<IServiceOutputMapping>();
                            RecordsetName = "";
                        }
                        ActionVisible = Methods.Count != 0;

                        if (Methods.Count <= 0 && SelectedNamespace != null)
                        {
                            ErrorMessage(new Exception("The selected dll does not contain actions to perform"));
                        }
                    }
                    catch (Exception e)
                    {
                        Methods = new List<IPluginAction>();
                        SelectedMethod = null;
                        ActionVisible = false;
                        _previosNamespace = null;
                        var errorInfo = new ErrorInfo
                        {
                            ErrorType = ErrorType.Critical,
                            Message = e.Message
                        };
                        Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfo, () => { }) };
                    }
                    SetToolHeight();
                    ResetHeightValues(DefaultToolHeight);
                }
                IsRefreshing = false;
                OnPropertyChanged("SelectedNamespace");
            }
        }
        public INamespaceItem Namespace
        {
            get
            {
                return GetProperty<INamespaceItem>();
            }
            set
            {
                SetProperty(value);
            }
        }

        public override IPluginSource SelectedSource
        {
            get
            {
                return _selectedSource;
            }
            set
            {
                if (!Equals(value, _selectedSource))
                {
                    if (value != null && Equals(value, _previousSource))
                    {
                        Errors = new List<IActionableErrorInfo>();
                        _selectedSource = value;
                        Namespaces = _previosNamespaces;
                        SelectedNamespace = _previosNamespace;
                        NamespaceVisible = Namespaces.Count != 0;
                        Methods = _previousMethods;
                        Outputs = _previousOutputs;
                        SelectedMethod = _previousMethod;
                        ActionVisible = Methods.Count != 0;
                        InputsVisible = PreviousInputsVisible;
                        SetToolHeight();
                        ResetHeightValues(DefaultToolHeight);
                    }
                    else
                    {
                        _previousSource = _selectedSource;
                        _previosNamespaces = Namespaces;
                        _previosNamespace = _selectedNamespace;
                        _previousMethods = Methods;
                        _previousMethod = _selectedMethod;
                        _previousInputs = _inputs;
                        _previousOutputs = Outputs;
                        _previousRecset = _recordsetName;

                        TestComplete = false;
                        IsNamespaceRefreshing = true;
                        InputsVisible = false;
                        Errors = new List<IActionableErrorInfo>();
                        _selectedSource = value;
                        try
                        {
                            Namespaces = _dllModel.GetNameSpaces(_selectedSource).OrderBy(item => item.AssemblyName).ToList();
                            if (!_isInitializing)
                            {
                                RecordsetName = "";
                                Inputs = new List<IServiceInput>();
                                Outputs = new List<IServiceOutputMapping>();
                            }
                            NamespaceVisible = Namespaces.Count != 0;
                            if (Methods != null)
                            {
                                ActionVisible = Methods.Count != 0;
                            }
                            else
                            {
                                ActionVisible = false;
                            }
                            if (Namespaces.Count <= 0)
                            {
                                ErrorMessage(new Exception("The selected dll does not contain Namespaces"));
                            }
                            if (_selectedSource != null)
                            {
                                SourceId = _selectedSource.Id;
                                if (SourceId != Guid.Empty)
                                {
                                    RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_sourceNotSelectedMessage)).ToList());
                                    FriendlySourceNameValue = _selectedSource.Name;
                                }
                            }
                            SetToolHeight();
                            ResetHeightValues(DefaultToolHeight);
                        }
                        catch (Exception e)
                        {
                            Methods = new List<IPluginAction>();
                            SelectedMethod = null;
                            ActionVisible = false;
                            _previousSource = null;
                            var errorInfo = new ErrorInfo
                            {
                                ErrorType = ErrorType.Critical,
                                Message = e.Message
                            };
                            Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfo, () => { }) };
                        }
                    }
                    IsNamespaceRefreshing = false;
                    ViewModelUtils.RaiseCanExecuteChanged(EditSourceCommand);
                    ViewModelUtils.RaiseCanExecuteChanged(RefreshActionsCommand);
                }
            }
        }

        void ValidateTestComplete()
        {
            TestComplete = SelectedSource != null && SelectedNamespace != null && SelectedMethod != null && Errors.Count < 1;
        }

        public bool NamespaceVisible
        {
            get
            {
                return _namespaceVisible;
            }
            set
            {
                _namespaceVisible = value;
                OnPropertyChanged("NamespaceVisible");
            }
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
        public ICollection<IPluginAction> Methods
        {
            get
            {
                return _methods;
            }
            set
            {
                _methods = value;
                if (Method != null)
                {
                    SelectedMethod = _methods.FirstOrDefault(a => a.FullName == Method.FullName);
                }
                OnPropertyChanged("Methods");
            }
        }
        public IPluginAction SelectedMethod
        {
            get
            {
                return _selectedMethod;
            }
            set
            {
                if (value != null && value == _previousMethod)
                {
                    Errors = new List<IActionableErrorInfo>();
                    _selectedMethod = value;
                    Inputs = _previousInputs;
                    Outputs = _previousOutputs;
                    RecordsetName = _previousRecset;
                    InputsVisible = PreviousInputsVisible;
                    TestComplete = PreviousTestComplete || Outputs.Count > 0;
                    SetToolHeight();
                    ResetHeightValues(DefaultToolHeight);
                    OnPropertyChanged("SelectedMethod");
                }
                else if (!Equals(value, _selectedMethod))
                {
                    _previousMethod = _selectedMethod;
                    _previousInputs = _inputs;
                    _previousRecset = _recordsetName;
                    _previousOutputs = Outputs;

                    TestComplete = false;
                    _selectedMethod = value;
                    if (_selectedMethod != null)
                    {
                        if (!_isInitializing)
                        {
                            Inputs = new List<IServiceInput>();
                            Outputs = new List<IServiceOutputMapping>();
                            RecordsetName = "";
                        }

                        Inputs = _selectedMethod.Inputs;

                        RemoveErrors(DesignValidationErrors.Where(a => a.Message.Contains(_methodNotSelectedMessage)).ToList());
                    }
                    Method = _selectedMethod;
                    InitializeProperties();
                    InputsVisible = _selectedMethod != null;
                    SetToolHeight();
                    ResetHeightValues(DefaultToolHeight);
                    ViewModelUtils.RaiseCanExecuteChanged(TestInputCommand);
                    OnPropertyChanged("SelectedMethod");
                }
            }
        }
        public IPluginAction Method
        {
            get
            {
                return GetProperty<IPluginAction>();
            }
            set
            {
                SetProperty(value);
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
        public bool IsNamespaceRefreshing
        {
            get
            {
                return _isNamespaceRefreshing;
            }
            set
            {
                _isNamespaceRefreshing = value;
                OnPropertyChanged("IsNamespaceRefreshing");
            }
        }
        public ICommand RefreshActionsCommand
        {
            get;
            set;
        }
        public ICommand RefreshNamespaceCommand
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
            return SelectedMethod != null;
        }

        string GetIconPath()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.PluginService.ToString();
            return "PluginService-32";
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
                Message = _methodNotSelectedMessage
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

        ~DotNetDllViewModel()
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

        #region Overrides of CustomToolViewModelBase

        public override double ToolHeight
        {
            get
            {
                return _toolHeight;
            }
            set
            {
                _toolHeight = value;
            }
        }
        public override double MaxToolHeight
        {
            get
            {
                return _maxToolHeight;
            }
            set
            {
                _maxToolHeight = value;
            }
        }

        #endregion

        #region Overrides of CustomToolViewModelBase<IPluginSource>

        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            if (mainViewModel != null)
            {
                mainViewModel.HelpViewModel.UpdateHelpText(helpText);
            }
        }

        #endregion
    }
}
