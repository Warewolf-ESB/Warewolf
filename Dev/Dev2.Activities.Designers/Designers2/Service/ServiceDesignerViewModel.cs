/*
*  Warewolf - Once bitten, there's no going back
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Utils;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Interfaces;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Interfaces.DataList;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Views;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;
using Dev2.Threading;
using Warewolf.Resource.Errors;
// ReSharper disable NonLocalizedString
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Activities.Designers2.Service
{
    public class ServiceDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>, INotifyPropertyChanged
    {
        readonly string _sourceNotFoundMessage = Warewolf.Studio.Resources.Languages.Core.ServiceDesignerSourceNotFound;
        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };
        private bool _resourcesUpdated;
        private readonly IEventAggregator _eventPublisher;

        private IDesignValidationService _validationService;
        private IErrorInfo _worstDesignError;
        private bool _isDisposed;
        private const string DoneText = "Done";
        private const string FixText = "Fix";

        [ExcludeFromCodeCoverage]
        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : this(modelItem, rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator, new AsyncWorker())
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel,
                                        IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
            : this(modelItem, rootModel, environmentRepository, eventPublisher, new AsyncWorker())
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker)
            : base(modelItem)
        {
            if (modelItem.ItemType != typeof(DsfDatabaseActivity) && modelItem.ItemType != typeof(DsfPluginActivity) && modelItem.ItemType != typeof(DsfWebserviceActivity))
            {
                AddTitleBarEditToggle();
            }
            AddTitleBarMappingToggle();

            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _worker = asyncWorker;
            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            ButtonDisplayValue = DoneText;

            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new DelegateCommand(o =>
            {
                FixErrors();
                IsFixed = IsWorstErrorReadOnly;
            });
            DoneCommand = new DelegateCommand(o => Done());
            DoneCompletedCommand = new DelegateCommand(o => DoneCompleted());

            InitializeDisplayName();

            InitializeImageSource();

            IsAsyncVisible = ActivityTypeToActionTypeConverter.ConvertToActionType(Type) == Common.Interfaces.Core.DynamicServices.enActionType.Workflow;
            OutputMappingEnabled = !RunWorkflowAsync;

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

            InitializeValidationService(_environment);
            if (!InitializeResourceModel(_environment))
            {
                return;
            }
            if (!IsDeleted)
            {
                InitializeMappings();
                InitializeLastValidationMemo(_environment);
                if (IsItemDragged.Instance.IsDragged)
                {
                    Expand();
                    IsItemDragged.Instance.IsDragged = false;
                }
            }
            var source = _environment?.ResourceRepository.FindSingle(a => a.ID == SourceId);
            if (source != null)
            {
                FriendlySourceName = source.DisplayName;
            }

            InitializeProperties();
            if (_environment != null)
            {
                _environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
                AuthorizationServiceOnPermissionsChanged(null, null);
            }

            ViewComplexObjectsCommand = new RelayCommand(item =>
            {
                ViewJsonObjects(item as IComplexObjectItemModel);
            }, CanViewComplexObjects);
        }

        private static bool CanViewComplexObjects(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.IsComplexObject;
        }

        private static void ViewJsonObjects(IComplexObjectItemModel item)
        {
            if (item != null)
            {
                var window = new JsonObjectsView {Height = 280};
                var contentPresenter = window.FindChild<TextBox>();
                if (contentPresenter != null)
                {
                    var json = item.GetJson();
                    contentPresenter.Text = JSONUtils.Format(json);
                }

                window.ShowDialog();
            }
        }

        private void OnEnvironmentOnAuthorizationServiceSet(object sender, EventArgs args)
        {
            if (_environment?.AuthorizationService != null)
            {
                _environment.AuthorizationService.PermissionsChanged += AuthorizationServiceOnPermissionsChanged;
            }
        }

        private void AuthorizationServiceOnPermissionsChanged(object sender, EventArgs eventArgs)
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
                    Message = ErrorResource.NoPermissionToExecuteTool
                });
                UpdateLastValidationMemo(memo);
            }
        }

        private void RemovePermissionsError()
        {
            var errorInfos = DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
            RemoveErrors(errorInfos.ToList());
        }

        private bool HasNoPermission()
        {
            var hasNoPermission = _environment.AuthorizationService != null && _environment.AuthorizationService.GetResourcePermissions(ResourceID) == Permissions.None;
            return hasNoPermission;
        }

        private void DoneCompleted()
        {
            IsFixed = true;
        }

        private void Done()
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

        public static readonly DependencyProperty IsFixedProperty = DependencyProperty.Register("IsFixed", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        public ICommand FixErrorsCommand { get; private set; }

        public ICommand DoneCommand { get; private set; }

        private RelayCommand ViewComplexObjectsCommand { get; set; }

        public ICommand DoneCompletedCommand { get; private set; }

        public IDataMappingViewModel DataMappingViewModel { get; private set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }

        public IContextualResourceModel ResourceModel { get; private set; }

        public IContextualResourceModel RootModel { get; private set; }

        public DesignValidationMemo LastValidationMemo { get; private set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; private set; }

        public IWebActivityFactory ActivityFactory
        {
            get { return _activityFactory ?? new InstanceWebActivityFactory(); }
            set { _activityFactory = value; }
        }

        public IDataMappingViewModelFactory MappingFactory
        {
            get { return _mappingFactory ?? new DataMappingViewModelFactory(); }
            set { _mappingFactory = value; }
        }

        public ErrorType WorstError
        {
            get { return (ErrorType)GetValue(WorstErrorProperty); }
            private set { SetValue(WorstErrorProperty, value); }
        }

        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(ServiceDesignerViewModel), new PropertyMetadata(ErrorType.None));

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

        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            private set { if (!(bool)GetValue(IsDeletedProperty)) SetValue(IsDeletedProperty, value); }
        }

        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            private set { SetValue(IsEditableProperty, value); }
        }

        public bool IsAsyncVisible
        {
            get { return (bool)GetValue(IsAsyncVisibleProperty); }
            private set { SetValue(IsAsyncVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsAsyncVisibleProperty =
            DependencyProperty.Register("IsAsyncVisible", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public bool RunWorkflowAsync
        {
            get
            {
                return GetProperty<bool>();
            }
            set
            {
                _runWorkflowAsync = value;
                OutputMappingEnabled = !_runWorkflowAsync;
                SetProperty(value);
            }
        }

        public bool OutputMappingEnabled
        {
            get { return (bool)GetValue(OutputMappingEnabledProperty); }
            private set { SetValue(OutputMappingEnabledProperty, value); }
        }

        public static readonly DependencyProperty OutputMappingEnabledProperty =
            DependencyProperty.Register("OutputMappingEnabled", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public string ImageSource
        {
            get { return (string)GetValue(ImageSourceProperty); }
            private set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(null));

        public bool ShowParent
        {
            get { return (bool)GetValue(ShowParentProperty); }
            set { SetValue(ShowParentProperty, value); }
        }

        public static readonly DependencyProperty ShowParentProperty =
            DependencyProperty.Register("ShowParent", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false, OnShowParentChanged));


        static void OnShowParentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var viewModel = (ServiceDesignerViewModel)d;
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
                    WorstError = value?.ErrorType ?? ErrorType.None;
                }
            }
        }

        string ServiceUri => GetProperty<string>();
        string ServiceName => GetProperty<string>();
        string ActionName => GetProperty<string>();

        string FriendlySourceName
        {
            get
            {
                return GetProperty<string>();
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged("FriendlySourceName");
                
            }
        }
        public string Type => GetProperty<string>();
        // ReSharper disable InconsistentNaming
        Guid EnvironmentID => GetProperty<Guid>();

        Guid ResourceID => GetProperty<Guid>();
        Guid UniqueID => GetProperty<Guid>();
        public string OutputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string InputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }

        public string ButtonDisplayValue
        {
            get { return (string)GetValue(ButtonDisplayValueProperty); }
            set { SetValue(ButtonDisplayValueProperty, value); }
        }

        public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(default(string)));
        readonly IEnvironmentModel _environment;
        bool _runWorkflowAsync;
        private readonly IAsyncWorker _worker;
        private bool _versionsDifferent;
        private IWebActivityFactory _activityFactory;
        private IDataMappingViewModelFactory _mappingFactory;

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

        public void UpdateMappings()
        {
            if (!_resourcesUpdated)
            {
                SetInputs();
                SetOuputs();
            }
        }


        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            if (propertyName == ShowLargeProperty.Name)
            {
                if (!isChecked)
                {
                    UpdateMappings();
                    CheckForRequiredMapping();
                }
            }
        }

        void SetInputs()
        {
            if (DataMappingViewModel != null)
            {
                InputMapping = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
            }
        }

        void SetOuputs()
        {
            if (DataMappingViewModel != null)
            {
                OutputMapping = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
            }
        }

        void InitializeMappings()
        {
            var webAct = ActivityFactory.CreateWebActivity(ModelItem, ResourceModel, ServiceName);
            DataMappingViewModel = new DataMappingViewModel(webAct, OnMappingCollectionChanged);
        }

        void OnMappingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IInputOutputViewModel mapping in e.NewItems)
                {
                    mapping.PropertyChanged += OnMappingPropertyChanged;
                }
            }
            if (e.OldItems != null)
            {
                foreach (IInputOutputViewModel mapping in e.OldItems)
                {
                    mapping.PropertyChanged -= OnMappingPropertyChanged;
                }
            }
        }

        void OnMappingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "MapsTo":
                    SetInputs();
                    break;

                case "Value":
                    SetOuputs();
                    break;
            }
        }

        void InitializeDisplayName()
        {
            var serviceName = ServiceName;
            if (!string.IsNullOrEmpty(serviceName))
            {
                var displayName = DisplayName;
                if (!string.IsNullOrEmpty(displayName))
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
                IsValid = RootModel.Errors.Count == 0
            };
            designValidationMemo.Errors.AddRange(RootModel.GetErrors(uniqueId).Cast<ErrorInfo>());

            if (environmentModel == null)
            {
                designValidationMemo.IsValid = false;
                designValidationMemo.Errors.Add(new ErrorInfo
                {
                    ErrorType = ErrorType.Critical,
                    FixType = FixType.None,
                    InstanceID = uniqueId,
                    Message = ErrorResource.ServerSourceNotFound
                });
            }

            UpdateLastValidationMemo(designValidationMemo);
        }

        bool InitializeResourceModel(IEnvironmentModel environmentModel)
        {
            if (environmentModel != null)
            {
                if (!environmentModel.IsLocalHost && environmentModel.IsConnected)
                {
                    var contextualResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(ResourceID);
                    if (contextualResourceModel != null)
                    {
                        ResourceModel = contextualResourceModel;
                    }
                    else
                    {
                        ResourceModel = ResourceModelFactory.CreateResourceModel(environmentModel);
                        ResourceModel.Inputs = InputMapping;
                        ResourceModel.Outputs = OutputMapping;
                        environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError, false);
                        environmentModel.ResourcesLoaded += OnEnvironmentModel_ResourcesLoaded;
                    }
                    return true;
                }

                if (!InitializeResourceModelSync(environmentModel))
                    return false;

            }
            return true;
        }

        // ReSharper disable InconsistentNaming
        void OnEnvironmentModel_ResourcesLoaded(object sender, ResourcesLoadedEventArgs e)
        // ReSharper restore InconsistentNaming
        {
            _worker.Start(() => GetResourceModel(e.Model), CheckVersions);
            e.Model.ResourcesLoaded -= OnEnvironmentModel_ResourcesLoaded;
        }

        private void GetResourceModel(IEnvironmentModel environmentModel)
        {
            var resourceId = ResourceID;

            if (resourceId != Guid.Empty)
            {
                NewModel = environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceId, true) as IContextualResourceModel;

            }            
        }

        private void CheckVersions()
        {
            if (LastValidationMemo != null && LastValidationMemo.Errors.Any(a => a.Message.Contains("This service will only execute when the server is online.")))
            {
                RemoveErrors(LastValidationMemo.Errors.Where(
                        a => a.Message.Contains("This service will only execute when the server is online.")).ToList());
                UpdateWorstError();
            }
            var webAct = ActivityFactory.CreateWebActivity(NewModel, NewModel, ServiceName);
            var newMapping = MappingFactory.CreateModel(webAct, OnMappingCollectionChanged);
            if (newMapping.GetInputString(DataMappingViewModel.Inputs) != DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs) ||
                newMapping.GetOutputString(DataMappingViewModel.Outputs) != DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs))
            {
                UpdateLastValidationMemoWithVersionChanged();
                _resourcesUpdated = true;
                _versionsDifferent = true;
            }
        }


        protected IContextualResourceModel NewModel { get; set; }

        private bool InitializeResourceModelSync(IEnvironmentModel environmentModel)
        {
            var resourceId = ResourceID;
            if (!environmentModel.IsConnected)
            {
                environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError);
                return true;
            }
            if (resourceId != Guid.Empty)
            {
                ResourceModel = environmentModel.ResourceRepository.LoadContextualResourceModel(resourceId);

            }
            if(!CheckSourceMissing())
            {
                return false;
            }
            return true;
        }

        private void UpdateLastValidationMemoWithVersionChanged()
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
                FixType = FixType.ReloadMapping,
                Message = "Incorrect Version. The remote workflow has changed.Please refresh"
            });
            UpdateLastValidationMemo(memo, false);
        }

        bool CheckSourceMissing()
        {
            if (ResourceModel != null && ResourceModel.ResourceType == Studio.Core.AppResources.Enums.ResourceType.Service && _environment != null)
            {
                var resourceModel = ResourceModel;
                if (resourceModel != null)
                {
                    string srcId;
                    var workflowXml = resourceModel.WorkflowXaml;
                    try
                    {
                        var xe = workflowXml.Replace("&", "&amp;").ToXElement();
                        srcId = xe.AttributeSafe("SourceID");
                    }
                    catch (XmlException xe)
                    {
                        Dev2Logger.Error(xe);
                        srcId = workflowXml.ExtractXmlAttributeFromUnsafeXml("SourceID=\"");
                    }

                    Guid sourceId;
                    if (Guid.TryParse(srcId, out sourceId))
                    {
                        SourceId = sourceId;
                        var sourceResource = _environment.ResourceRepository.LoadContextualResourceModel(sourceId);
                        if (sourceResource == null)
                        {
                            UpdateLastValidationMemoWithSourceNotFoundError();
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public Guid SourceId { get; set; }

        void InitializeValidationService(IEnvironmentModel environmentModel)
        {
            if (environmentModel?.Connection?.ServerEvents != null)
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
            AddProperty("Procedure :", ActionName);
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
            Common.Interfaces.Core.DynamicServices.enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType(Type);
            ImageSource = GetIconPath(actionType);
        }

        public string ResourceType
        {
            get;
            set;
        }

        string GetIconPath(Common.Interfaces.Core.DynamicServices.enActionType actionType)
        {
            switch (actionType)
            {
                case Common.Interfaces.Core.DynamicServices.enActionType.Workflow:
                    if(string.IsNullOrEmpty(ServiceUri))
                    {
                        ResourceType = "WorkflowService";
                        return "Workflow-32";
                    }
                    ResourceType = "Server";
                    return "RemoteWarewolf-32";

                case Common.Interfaces.Core.DynamicServices.enActionType.RemoteService:
                    ResourceType = "Server";
                    return "RemoteWarewolf-32";

            }
            return "ToolService-32";
        }

        void AddTitleBarEditToggle()
        {
            // ReSharper disable RedundantArgumentName
            var toggle = ActivityDesignerToggle.Create("ServicePropertyEdit", "Edit", "ServicePropertyEdit", "Edit", "ShowParentToggle",
                autoReset: true,
                target: this,
                dp: ShowParentProperty
                );
            // ReSharper restore RedundantArgumentName
            TitleBarToggles.Add(toggle);
        }

        void AddTitleBarMappingToggle()
        {
            HasLargeView = true;
        }

        void DoShowParent()
        {
            if (!IsDeleted)
            {
                _eventPublisher.Publish(new EditActivityMessage(ModelItem, EnvironmentID));
            }
        }

        void UpdateLastValidationMemo(DesignValidationMemo memo, bool checkSource = true)
        {
            LastValidationMemo = memo;

            CheckRequiredMappingChangedErrors(memo);
            CheckIsDeleted(memo);

            UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == UniqueID && info.ErrorType != ErrorType.None));
            if (SourceId == Guid.Empty)
            {
                if (checkSource && CheckSourceMissing())
                {
                    InitializeMappings();
                    UpdateMappings();
                }
            }
            OnDesignValidationReceived?.Invoke(this, memo);
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
                                          ? "Server is offline. This service will only execute when the server is online."
                                          : "Server login failed. This service will only execute when the login permissions issues have been resolved."
                        });
                        UpdateLastValidationMemo(memo);
                        break;
                }
            });
        }

        void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            var keepError = false;
            var reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if (reqiredMappingChanged != null)
            {
                if (reqiredMappingChanged.FixData != null)
                {
                    var xElement = XElement.Parse(reqiredMappingChanged.FixData);
                    var inputOutputViewModels = DeserializeMappings(true, xElement);

                    foreach (var input in inputOutputViewModels)
                    {
                        IInputOutputViewModel currentInputViewModel = input;
                        var inputOutputViewModel = DataMappingViewModel?.Inputs.FirstOrDefault(c => c.Name == currentInputViewModel.Name);
                        if (inputOutputViewModel != null)
                        {
                            inputOutputViewModel.Required = input.Required;
                            if (inputOutputViewModel.MapsTo == string.Empty && inputOutputViewModel.Required)
                            {
                                keepError = true;
                            }
                        }
                    }
                }

                if (!keepError)
                {
                    var worstErrors = memo.Errors.Where(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == UniqueID).ToList();
                    memo.Errors.RemoveAll(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == UniqueID);
                    RemoveErrors(worstErrors);
                }
            }
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

        void CheckForRequiredMapping()
        {
            if (DataMappingViewModel != null && DataMappingViewModel.Inputs.Any(c => c.Required && String.IsNullOrEmpty(c.MapsTo)))
            {
                if (DesignValidationErrors.All(c => c.FixType != FixType.IsRequiredChanged))
                {
                    var listToRemove = DesignValidationErrors.Where(c => c.FixType == FixType.None && c.ErrorType == ErrorType.None).ToList();

                    foreach (var errorInfo in listToRemove)
                    {
                        DesignValidationErrors.Remove(errorInfo);
                    }

                    var mappingIsRequiredMessage = CreateMappingIsRequiredMessage();
                    DesignValidationErrors.Add(mappingIsRequiredMessage);
                    RootModel.AddError(mappingIsRequiredMessage);
                }
                UpdateWorstError();
                return;
            }

            if (DesignValidationErrors.Any(c => c.FixType == FixType.IsRequiredChanged))
            {
                var listToRemove = DesignValidationErrors.Where(c => c.FixType == FixType.IsRequiredChanged).ToList();

                foreach (var errorInfo in listToRemove)
                {
                    DesignValidationErrors.Remove(errorInfo);
                    RootModel.RemoveError(errorInfo);
                }
                UpdateWorstError();
            }
        }

        ErrorInfo CreateMappingIsRequiredMessage()
        {
            return new ErrorInfo { ErrorType = ErrorType.Critical, FixData = CreateFixedData(), FixType = FixType.IsRequiredChanged, InstanceID = UniqueID };
        }

        string CreateFixedData()
        {
            var serializer = new Dev2JsonSerializer();
            var result = serializer.Serialize(DataMappingListFactory.CreateListInputMapping(DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs)));
            return string.Concat("<Input>", result, "</Input>");
        }

        void FixErrors()
        {
            if (!_versionsDifferent && (WorstDesignError.ErrorType == ErrorType.None || WorstDesignError.FixData == null))
            {
                return;
            }

            switch (WorstDesignError.FixType)
            {
                case FixType.ReloadMapping:
                    ShowLarge = true;
                    if (!_versionsDifferent)
                    {
                        var xml = FetchXElementFromFixData();
                        var inputs = GetMapping(xml, true, DataMappingViewModel.Inputs);
                        var outputs = GetMapping(xml, false, DataMappingViewModel.Outputs);

                        DataMappingViewModel.Inputs.Clear();
                        foreach (var input in inputs)
                        {
                            DataMappingViewModel.Inputs.Add(input);
                        }

                        DataMappingViewModel.Outputs.Clear();
                        foreach (var output in outputs)
                        {
                            DataMappingViewModel.Outputs.Add(output);
                        }
                        SetInputs();
                        SetOuputs();
                        RemoveError(WorstDesignError);
                        UpdateWorstError();
                    }
                    else if (_versionsDifferent)
                    {
                        ResourceModel = NewModel;
                        InitializeMappings();
                        RemoveErrors(
                      LastValidationMemo.Errors.Where(a => a.Message.Contains("Incorrect Version")).ToList());
                        UpdateWorstError();
                    }
                    break;

                case FixType.IsRequiredChanged:
                    ShowLarge = true;
                    var inputOutputViewModels = DeserializeMappings(true, FetchXElementFromFixData());
                    foreach (var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
                    {
                        IInputOutputViewModel model = inputOutputViewModel;
                        var actualViewModel = DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == model.Name);
                        if (actualViewModel != null)
                        {
                            if (actualViewModel.Value == string.Empty)
                            {
                                actualViewModel.RequiredMissing = true;
                            }
                        }
                    }

                    break;
            }
        }

        XElement FetchXElementFromFixData()
        {
            if (!string.IsNullOrEmpty(WorstDesignError?.FixData))
            {
                try
                {
                    return XElement.Parse(WorstDesignError.FixData);
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e);
                }
            }

            return XElement.Parse("<x/>");
        }

        IEnumerable<IInputOutputViewModel> GetMapping(XContainer xml, bool isInput, ObservableCollection<IInputOutputViewModel> oldMappings)
        {
            var result = new ObservableCollection<IInputOutputViewModel>();

            var input = xml.Descendants(isInput ? "Input" : "Output").FirstOrDefault();
            if (input != null)
            {
                var newMappings = DeserializeMappings(isInput, input);

                foreach (var newMapping in newMappings)
                {
                    IInputOutputViewModel mapping = newMapping;
                    var oldMapping = oldMappings.FirstOrDefault(m => m.Name.Equals(mapping.Name, StringComparison.InvariantCultureIgnoreCase));
                    if (oldMapping != null)
                    {
                        newMapping.MapsTo = oldMapping.MapsTo;
                        newMapping.Value = oldMapping.Value;
                    }
                    else
                    {
                        newMapping.IsNew = true;
                    }
                    result.Add(newMapping);
                }
            }
            return result;
        }

        IEnumerable<IInputOutputViewModel> DeserializeMappings(bool isInput, XElement input)
        {
            try
            {
                var serializer = new Dev2JsonSerializer();
                var defs = serializer.Deserialize<List<Dev2Definition>>(input.Value);
                IList<IDev2Definition> idefs = new List<IDev2Definition>(defs);
                var newMappings = isInput
                    ? InputOutputViewModelFactory.CreateListToDisplayInputs(idefs)
                    : InputOutputViewModelFactory.CreateListToDisplayOutputs(idefs);
                return newMappings;
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }

            return new List<IInputOutputViewModel>();
        }


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

        public void Handle(UpdateResourceMessage message)
        {
            if (message?.ResourceModel != null)
            {
                if (SourceId != Guid.Empty && SourceId == message.ResourceModel.ID)
                {
                    IErrorInfo sourceNotAvailableMessage = DesignValidationErrors.FirstOrDefault(info => info.Message == _sourceNotFoundMessage);
                    if (sourceNotAvailableMessage != null)
                    {
                        RemoveError(sourceNotAvailableMessage);
                        UpdateWorstError();
                        InitializeMappings();
                        UpdateMappings();
                    }
                }
            }
        }

        ~ServiceDesignerViewModel()
        {
            Dispose(false);
        }

        protected override void OnDispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            base.OnDispose();
        }
        
        void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _validationService?.Dispose();
                    if (_environment != null)
                    {
                        _environment.AuthorizationServiceSet -= OnEnvironmentOnAuthorizationServiceSet;
                    }
                }
                _isDisposed = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public override void UpdateHelpDescriptor(string helpText)
        {
            var mainViewModel = CustomContainer.Get<IMainViewModel>();
            mainViewModel?.HelpViewModel.UpdateHelpText(helpText);
        }
    }
}





