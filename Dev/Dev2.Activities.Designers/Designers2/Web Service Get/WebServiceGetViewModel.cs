using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Interfaces.ToolBase;
using Dev2.Common.Interfaces.WebService;
using Dev2.Communication;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Interfaces;
using Dev2.Threading;
using Microsoft.Practices.Prism.Commands;

namespace Dev2.Activities.Designers2.Web_Service_Get
{
    public class WebServiceGetViewModel :CustomToolWithRegionBase,IWebServiceGetViewModel
    {
        private IOutputsToolRegion _outputs;
        private IWebGetInputArea _inputArea;
        private ISourceToolRegion<IWebServiceSource> _source;
        private IEventAggregator _eventPublisher;
        private string _buttonDisplayValue;
        private int _labelWidth;
        private bool _testComplete;
        private IContextualResourceModel _rootModel;
        private ObservableCollection<IErrorInfo> _designValidationErrors;
        private Runtime.Configuration.ViewModels.Base.DelegateCommand _fixErrorsCommand;
        private string _resourceType;
        private string _imageSource;

#pragma warning disable 169
        private IEnvironmentModel _environment;
#pragma warning restore 169
        private DelegateCommand _testInputCommand;
        private DesignValidationService _validationService;
        private IErrorInfo _worstDesignError;
        const string DoneText = "Done";
        const string FixText = "Fix";
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotFound;
        readonly string _sourceNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceSourceNotSelected;
        readonly string _methodNotSelectedMessage = Warewolf.Studio.Resources.Languages.Core.PluginServiceMethodNotSelected;
        readonly string _serviceExecuteOnline = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteOnline;
        readonly string _serviceExecuteLoginPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteLoginPermission;
        readonly string _serviceExecuteViewPermission = Warewolf.Studio.Resources.Languages.Core.DatabaseServiceExecuteViewPermission;

        public WebServiceGetViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : base(modelItem)
        {
            var shellViewModel = CustomContainer.Get<IShellViewModel>();
            var server = shellViewModel.ActiveServer;
            var pluginServiceModel = CustomContainer.CreateInstance<IWebServiceModel>(server.UpdateRepository, server.QueryProxy, shellViewModel, server);
            Model = pluginServiceModel;
            BuildRegions();
            InitialiseViewModel(rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker(), new ManagePluginServiceInputViewModel(), pluginServiceModel);

        }

        public WebServiceGetViewModel(ModelItem modelItem, IList<IToolRegion> regions)
            : base(modelItem, regions)
        {
        }

        public WebServiceGetViewModel(ModelItem modelItem, Action<Type> showExampleWorkflow, IList<IToolRegion> regions)
            : base(modelItem, showExampleWorkflow, regions)
        {
        }

        #region Overrides of ActivityDesignerViewModel

        public override void Validate()
        {
        }

        private void InitialiseViewModel(IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker, IManagePluginServiceInputViewModel manageServiceInputViewModel, IWebServiceModel webServiceModel)
        {
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            //ButtonDisplayValue = DoneText;

            //LabelWidth = 70;
            ReCalculateHeight();

            TestComplete = false;
            ShowLarge = true;
            ThumbVisibility = Visibility.Visible;
            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Runtime.Configuration.ViewModels.Base.DelegateCommand(o =>
            {
                FixErrors();
            });

            InitializeDisplayName();

            InitializeImageSource();

            //Outputs.OutputMappingEnabled = true;

            //// When the active environment is not local, we need to get smart around this piece of logic.
            //// It is very possible we are treating a remote active as local since we cannot logically assign 
            //// an environment id when this is the case as it will fail with source not found since the remote 
            //// does not contain localhost's connections ;)
            //var activeEnvironment = environmentRepository.ActiveEnvironment;
            //if (EnvironmentID == Guid.Empty && !activeEnvironment.IsLocalHostCheck())
            //{
            //    _environment = activeEnvironment;
            //}
            //else
            //{
            //    var environment = environmentRepository.FindSingle(c => c.ID == EnvironmentID);
            //    if (environment == null)
            //    {
            //        IList<IEnvironmentModel> environments = EnvironmentRepository.Instance.LookupEnvironments(activeEnvironment);
            //        environment = environments.FirstOrDefault(model => model.ID == EnvironmentID);
            //    }
            //    _environment = environment;
            //}
            //TestInputCommand = new Microsoft.Practices.Prism.Commands.DelegateCommand(TestAction, CanTestProcedure);
            //InitializeValidationService(_environment);
            //InitializeLastValidationMemo(_environment);
            //ManageServiceInputViewModel = manageServiceInputViewModel;
            //if (_environment != null)
            //{
            //    _isInitializing = true;


            //    Model = webServiceModel;


             
               

            //}
            //InitializeProperties();
            //if (IsItemDragged.Instance.IsDragged)
            //{
            //    Expand();
            //    IsItemDragged.Instance.IsDragged = false;
            //}

            //if (_environment != null)
            //{
            //    _environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
            //    AuthorizationServiceOnPermissionsChanged(null, null);
            //}
            //InitializeResourceModel(_environment);

            //_isInitializing = false;

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

        bool CheckSourceMissing()
        {
            return true;
        }


        void AuthorizationServiceOnPermissionsChanged(object sender, EventArgs eventArgs)
        {
            //RemovePermissionsError();

            //var hasNoPermission = HasNoPermission();
            //if (hasNoPermission)
            //{
            //    var memo = new DesignValidationMemo
            //    {
            //        InstanceID = UniqueID,
            //        IsValid = false,
            //    };
            //    memo.Errors.Add(new ErrorInfo
            //    {
            //        InstanceID = UniqueID,
            //        ErrorType = ErrorType.Critical,
            //        FixType = FixType.InvalidPermissions,
            //        Message = _serviceExecuteViewPermission
            //    });
            //    UpdateLastValidationMemo(memo);
            }
        

        private void OnEnvironmentOnAuthorizationServiceSet(object sender, EventArgs e)
        {
        }

        public List<KeyValuePair<string, string>> Properties { get; private set; }
        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", Source.SelectedSource.Name);
            AddProperty("Type :", Type);
            AddProperty("Url :", Source.SelectedSource.HostName);
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public DelegateCommand NewSourceCommand
        {
            get
            {
                return _newSourceCommand;
            }
            set
            {
                _newSourceCommand = value;
            }
        }

        public IManagePluginServiceInputViewModel ManageServiceInputViewModel
        {
            get
            {
                return _manageServiceInputViewModel;
            }
            set
            {
                _manageServiceInputViewModel = value;
            }
        }

        private bool CanTestProcedure()
        {
            return false;
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

        public IErrorInfo NoError
        {
            get
            {
                return _noError;
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
    DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(WebServiceGetViewModel), new PropertyMetadata(false));


        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }
        public static readonly DependencyProperty WorstErrorProperty =
        DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(WebServiceGetViewModel), new PropertyMetadata(ErrorType.None));
        private System.Action _testAction;
        private IManagePluginServiceInputViewModel _manageServiceInputViewModel;
#pragma warning disable 414
#pragma warning disable 169
        private bool _isInitializing;
#pragma warning restore 169
#pragma warning restore 414
        private IWebServiceModel _model;
        private DelegateCommand _newSourceCommand;
#pragma warning disable 649
        private IErrorInfo _noError;
#pragma warning restore 649

        // ReSharper disable once UnusedParameter.Local
        void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource = true)
        {
            LastValidationMemo = memo;

           // CheckIsDeleted(memo);

            //UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == UniqueID && info.ErrorType != ErrorType.None));
            //if (SourceId == Guid.Empty)
            //{
            //    if (checkSource && CheckSourceMissing())
            //    {
            //    }
            //}
        }

        public DesignValidationMemo LastValidationMemo { get; private set; }

        void InitializeValidationService(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null && environmentModel.Connection != null && environmentModel.Connection.ServerEvents != null)
            {
                _validationService = new DesignValidationService(environmentModel.Connection.ServerEvents);
                _validationService.Subscribe(UniqueID, a => UpdateLastValidationMemo(a));
            }
        }

        public DelegateCommand TestInputCommand
        {
            get
            {
                return _testInputCommand;
            }
            set
            {
                _testInputCommand = value;
            }
        }

        public string Type { get { return GetProperty<string>(); } }
        // ReSharper disable InconsistentNaming
        Guid EnvironmentID { get { return GetProperty<Guid>(); } }
        Guid ResourceID { get { return GetProperty<Guid>(); } }
        Guid UniqueID { get { return GetProperty<Guid>(); } }



        private void FixErrors()
        {
        }

        void InitializeImageSource()
        {
            ImageSource = GetIconPath();
        }

        public string ImageSource
        {
            get
            {
                return _imageSource;
            }
            set
            {
                _imageSource = value;
                OnPropertyChanged();
            }
        }

        string GetIconPath()
        {
            ResourceType = Common.Interfaces.Data.ResourceType.PluginService.ToString();
            return "PluginService-32";
        }

        public string ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                _resourceType = value;
            }
        }

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
        public string ServiceName { get { return GetProperty<string>(); } }
        public Runtime.Configuration.ViewModels.Base.DelegateCommand FixErrorsCommand
        {
            get
            {
                return _fixErrorsCommand;
            }
            set
            {
                _fixErrorsCommand = value;
            }
        }

        public ObservableCollection<IErrorInfo> DesignValidationErrors
        {
            get
            {
                return _designValidationErrors;
            }
            set
            {
                _designValidationErrors = value;
            }
        }

        public IContextualResourceModel RootModel
        {
            get
            {
                return _rootModel;
            }
            set
            {
                _rootModel = value;
            }
        }

        public bool TestComplete
        {
            get
            {
                return _testComplete;
            }
            set
            {
                _testComplete = value;
            }
        }



        public int LabelWidth
        {
            get
            {
                return _labelWidth;
            }
            set
            {
                _labelWidth = value;
            }
        }

        public string ButtonDisplayValue
        {
            get
            {
                return _buttonDisplayValue;
            }
            set
            {
                _buttonDisplayValue = value;
            }
        }

        public override void UpdateHelpDescriptor(string helpText)
        {
        }

        #endregion

        #region Overrides of CustomToolWithRegionBase

        protected override IList<IToolRegion> BuildRegions()
        {
            IList<IToolRegion> regions = new List<IToolRegion>();
            if (Source == null)
            {
                Source = new WebSourceRegion(Model, ModelItem);
                regions.Add(Source);
                InputArea = new WebGetInputRegion(ModelItem,Source);
            }
            Regions = regions;
            ReCalculateHeight();
            return regions;
        }

        #endregion

        #region Implementation of IWebServiceGetViewModel

        public IOutputsToolRegion Outputs
        {
            get
            {
                return _outputs;
            }
            set
            {
                _outputs = value;
                OnPropertyChanged();
            }
        }
        public IWebGetInputArea InputArea
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
        public ISourceToolRegion<IWebServiceSource> Source
        {
            get
            {
                return _source;
            }
            set
            {
                _source = value;
                OnPropertyChanged();
            }
        }
        public System.Action TestAction
        {
            get
            {
                return _testAction;
            }
            set
            {
                _testAction = value;
            }
        }
        public IWebServiceModel Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        #endregion
    }


}
