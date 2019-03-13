/*
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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Utils;
using Dev2.Common;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Utils;
using Dev2.Communication;
using Dev2.Providers.Errors;
using Dev2.Runtime.Configuration.ViewModels.Base;
using Dev2.Services.Events;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.Views;
using Dev2.Threading;
using Warewolf.Resource.Errors;
using Dev2.Studio.Interfaces;
using Dev2.Studio.Interfaces.DataList;
using Dev2.Common.Common;

namespace Dev2.Activities.Designers2.Service
{
    public class ServiceDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>, INotifyPropertyChanged
    {
        readonly IEventAggregator _eventPublisher;

        const string DoneText = "Done";
        const string FixText = "Fix";

        [ExcludeFromCodeCoverage]
        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : this(modelItem, rootModel, CustomContainer.Get<IServerRepository>(), EventPublishers.Aggregator, new AsyncWorker())
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel,
                                        IServerRepository serverRepository, IEventAggregator eventPublisher)
            : this(modelItem, rootModel, serverRepository, eventPublisher, new AsyncWorker())
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IServerRepository serverRepository, IEventAggregator eventPublisher, IAsyncWorker asyncWorker)
            : base(modelItem)
        {
            ValidationMemoManager = new ValidationMemoManager(this);
            MappingManager = new MappingManager(this);
            if (modelItem.ItemType != typeof(DsfDatabaseActivity) && modelItem.ItemType != typeof(DsfPluginActivity))
            {
                AddTitleBarEditToggle();
            }
            AddTitleBarMappingToggle();

            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", serverRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            VerifyArgument.IsNotNull("asyncWorker", asyncWorker);

            _worker = asyncWorker;
            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            ButtonDisplayValue = DoneText;

            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            ValidationMemoManager.DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new DelegateCommand(o =>
            {
                ValidationMemoManager.FixErrors();
                IsFixed = IsWorstErrorReadOnly;
            });
            DoneCommand = new DelegateCommand(o => Done());
            DoneCompletedCommand = new DelegateCommand(o => DoneCompleted());

            InitializeDisplayName();

            InitializeImageSource();

            IsAsyncVisible = ActivityTypeToActionTypeConverter.ConvertToActionType(Type) == Common.Interfaces.Core.DynamicServices.enActionType.Workflow;
            OutputMappingEnabled = !RunWorkflowAsync;

            var activeEnvironment = serverRepository.ActiveServer;
            if (EnvironmentID == Guid.Empty && !activeEnvironment.IsLocalHostCheck())
            {
                _environment = activeEnvironment;
            }
            else
            {
                var environment = serverRepository.FindSingle(c => c.EnvironmentID == EnvironmentID);
                if (environment == null)
                {
                    var environments = ServerRepository.Instance.LookupEnvironments(activeEnvironment);
                    environment = environments.FirstOrDefault(model => model.EnvironmentID == EnvironmentID);
                }
                _environment = environment;
            }

            ValidationMemoManager.InitializeValidationService(_environment);
            IsLoading = true;
            _worker.Start(() => InitializeResourceModel(_environment), b =>
              {
                  if (b)
                  {
                      UpdateDesignerAfterResourceLoad(serverRepository);
                  }
              });

            ViewComplexObjectsCommand = new RelayCommand(item =>
            {
                ViewJsonObjects(item as IComplexObjectItemModel, new JsonObjectsView());
            }, CanViewComplexObjects);
        }

        void UpdateDesignerAfterResourceLoad(IServerRepository serverRepository)
        {

            if (!IsDeleted)
            {
                MappingManager.InitializeMappings();
                ValidationMemoManager.InitializeLastValidationMemo(_environment);
                if (IsItemDragged.Instance.IsDragged)
                {
                    Expand();
                    IsItemDragged.Instance.IsDragged = false;
                }
            }
            var environmentModel = serverRepository.Get(EnvironmentID);
            if (EnvironmentID == Guid.Empty)
            {
                environmentModel = serverRepository.ActiveServer;
            }
            if (environmentModel?.Connection?.WebServerUri != null)
            {
                var servUri = new Uri(environmentModel.Connection.WebServerUri.ToString());
                var host = servUri.Host;
                if (!host.Equals(FriendlySourceName, StringComparison.InvariantCultureIgnoreCase))
                {
                    FriendlySourceName = host;
                }
            }

            InitializeProperties();
            if (_environment != null)
            {
                _environment.AuthorizationServiceSet += OnEnvironmentOnAuthorizationServiceSet;
                AuthorizationServiceOnPermissionsChanged(null, null);
            }
            IsLoading = false;
            if (ResourceModel == null)
            {
                ValidationMemoManager.UpdateLastValidationMemoWithSourceNotFoundError();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        static bool CanViewComplexObjects(Object itemx)
        {
            var item = itemx as IDataListItemModel;
            return item != null && !item.IsComplexObject;
        }

        static void ViewJsonObjects(IComplexObjectItemModel item, JsonObjectsView window)
        {
            if (item != null && window != null)
            {
                window.Height = 280;
                var contentPresenter = window.FindChild<TextBox>();
                if (contentPresenter != null)
                {
                    var json = item.GetJson();
                    contentPresenter.Text = JSONUtils.Format(json);
                }

                window.ShowDialog();
            }
        }

        void OnEnvironmentOnAuthorizationServiceSet(object sender, EventArgs args)
        {
            if (_environment?.AuthorizationService != null)
            {
                _environment.AuthorizationService.PermissionsChanged += AuthorizationServiceOnPermissionsChanged;
            }
        }

        void AuthorizationServiceOnPermissionsChanged(object sender, EventArgs eventArgs)
        {
            ValidationMemoManager.RemovePermissionsError();

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
                MappingManager.UpdateLastValidationMemo(memo);
            }
        }

        bool HasNoPermission()
        {
            var hasNoPermission = ResourceModel != null && ResourceModel.UserPermissions == Permissions.None;
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
                ValidationMemoManager.FixErrors();
            }
        }

        public bool IsFixed
        {
            get => (bool)GetValue(IsFixedProperty);
            set { SetValue(IsFixedProperty, value); }
        }

        public static readonly DependencyProperty IsFixedProperty = DependencyProperty.Register("IsFixed", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public ICommand FixErrorsCommand { get; private set; }

        public ICommand DoneCommand { get; private set; }

        RelayCommand ViewComplexObjectsCommand { get; set; }

        public ICommand DoneCompletedCommand { get; private set; }

        public List<KeyValuePair<string, string>> Properties
        {
            get => _properties;
            private set
            {
                _properties = value;
                OnPropertyChanged("Properties");
            }
        }

        public IContextualResourceModel ResourceModel { get; set; }

        public IContextualResourceModel RootModel { get; set; }

        public static readonly DependencyProperty WorstErrorProperty =
            DependencyProperty.Register("WorstError", typeof(ErrorType), typeof(ServiceDesignerViewModel), new PropertyMetadata(ErrorType.None));

        public bool IsWorstErrorReadOnly
        {
            get => (bool)GetValue(IsWorstErrorReadOnlyProperty);
            set
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

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            if (propertyName == ShowLargeProperty.Name && !isChecked)
            {
                MappingManager.UpdateMappings();
                MappingManager.CheckForRequiredMapping();
            }

        }

        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsDeleted
        {
            get => (bool)GetValue(IsDeletedProperty);
            set
            {
                if (!(bool)GetValue(IsDeletedProperty))
                {
                    SetValue(IsDeletedProperty, value);
                }
            }
        }

        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsEditable
        {
            get => (bool)GetValue(IsEditableProperty);
            set { SetValue(IsEditableProperty, value); }
        }

        public bool IsAsyncVisible
        {
            get => (bool)GetValue(IsAsyncVisibleProperty);
            private set { SetValue(IsAsyncVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsAsyncVisibleProperty =
            DependencyProperty.Register("IsAsyncVisible", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public bool RunWorkflowAsync
        {
            get => GetProperty<bool>();
            set
            {
                _runWorkflowAsync = value;
                OutputMappingEnabled = !_runWorkflowAsync;
                SetProperty(value);
            }
        }

        public bool OutputMappingEnabled
        {
            get => (bool)GetValue(OutputMappingEnabledProperty);
            private set { SetValue(OutputMappingEnabledProperty, value); }
        }

        public static readonly DependencyProperty OutputMappingEnabledProperty =
            DependencyProperty.Register("OutputMappingEnabled", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(true));

        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.Register("IsEditable", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            private set { SetValue(ImageSourceProperty, value); }
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(null));

        public bool ShowParent
        {
            get => (bool)GetValue(ShowParentProperty);
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

        string ServiceUri => GetProperty<string>();
        public string ServiceName => GetProperty<string>();
        string ActionName => GetProperty<string>();

        string FriendlySourceName
        {
            get
            {
                var friendlySourceName = GetProperty<string>();

                return friendlySourceName;
            }
            set
            {
                SetProperty(value);
                OnPropertyChanged("FriendlySourceName");

            }
        }

        public IDataMappingViewModel DataMappingViewModel => MappingManager.DataMappingViewModel;

        public string Type => GetProperty<string>();

        Guid EnvironmentID => GetProperty<Guid>();

        public Guid ResourceID => GetProperty<Guid>();
        public Guid UniqueID => GetProperty<Guid>();
        public string OutputMapping
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }
        public string InputMapping
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        public string ButtonDisplayValue
        {
            get => (string)GetValue(ButtonDisplayValueProperty);
            set => SetValue(ButtonDisplayValueProperty, value);
        }

        public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(default(string)));
        readonly IServer _environment;
        bool _runWorkflowAsync;
        readonly IAsyncWorker _worker;
        bool _isLoading;
        List<KeyValuePair<string, string>> _properties;

        public override void Validate()
        {
            Errors = new List<IActionableErrorInfo>();
            if (HasNoPermission())
            {
                var errorInfos = ValidationMemoManager.DesignValidationErrors.Where(info => info.FixType == FixType.InvalidPermissions);
                Errors = new List<IActionableErrorInfo> { new ActionableErrorInfo(errorInfos.ToList()[0], () => { }) };
            }
            else
            {
                ValidationMemoManager.RemovePermissionsError();
            }
        }

        void InitializeDisplayName()
        {
            var serviceName = ServiceName;
            if (!string.IsNullOrEmpty(serviceName))
            {
                var displayName = DisplayName;
                if (string.IsNullOrEmpty(displayName) || displayName == ModelItem.ItemType.Name)
                {
                    DisplayName = serviceName;
                }
            }
        }

        bool InitializeResourceModel(IServer server)
        {
            if (server != null)
            {
                if (!server.IsLocalHost && server.IsConnected)
                {
                    var contextualResourceModel = server.ResourceRepository.LoadContextualResourceModel(ResourceID);
                    if (contextualResourceModel != null)
                    {
                        ResourceModel = contextualResourceModel;
                    }
                    else
                    {
                        ResourceModel = ResourceModelFactory.CreateResourceModel(server);
                        ResourceModel.Inputs = InputMapping;
                        ResourceModel.Outputs = OutputMapping;
                        server.Connection.Verify(ValidationMemoManager.UpdateLastValidationMemoWithOfflineError, false);
                        server.ResourcesLoaded += OnEnvironmentModel_ResourcesLoaded;
                    }
                    return true;
                }
                var init = InitializeResourceModelFromRemoteServer(server);
                return init;

            }
            return true;
        }

        void OnEnvironmentModel_ResourcesLoaded(object sender, ResourcesLoadedEventArgs e)
        {
            _worker.Start(() => GetResourceModel(e.Model), () => MappingManager.CheckVersions(this));
            e.Model.ResourcesLoaded -= OnEnvironmentModel_ResourcesLoaded;
        }

        void GetResourceModel(IServer server)
        {
            var resourceId = ResourceID;

            if (resourceId != Guid.Empty)
            {
                NewModel = server.ResourceRepository.FindSingle(c => c.ID == resourceId, true) as IContextualResourceModel;

            }
        }

        public IContextualResourceModel NewModel { get; set; }

        bool InitializeResourceModelFromRemoteServer(IServer server)
        {
            var resourceId = ResourceID;
            if (!server.IsConnected)
            {
                server.Connection.Verify(ValidationMemoManager.UpdateLastValidationMemoWithOfflineError);
            }
            if (server.IsConnected && resourceId != Guid.Empty)
            {
                ResourceModel = server.ResourceRepository.LoadContextualResourceModel(resourceId);
            }
            
            if (!CheckSourceMissing())
            {
                return false;
            }
            return true;
        }

        public bool CheckSourceMissing()
        {
            if (ResourceModel != null && _environment != null)
            {
                var resourceModel = ResourceModel;
                string srcId;
                var workflowXml = resourceModel?.WorkflowXaml;
                try
                {
                    var xe = workflowXml?.Replace("&", "&amp;").ToXElement();
                    srcId = xe?.AttributeSafe("SourceID");
                }
                catch (XmlException xe)
                {
                    Dev2Logger.Error(xe, "Warewolf Error");
                    srcId = workflowXml.ExtractXmlAttributeFromUnsafeXml("SourceID=\"");
                }

                if (Guid.TryParse(srcId, out Guid sourceId))
                {
                    SourceId = sourceId;
                    var sourceResource = _environment.ResourceRepository.LoadContextualResourceModel(sourceId);
                    if (sourceResource == null)
                    {
                        ValidationMemoManager.UpdateLastValidationMemoWithSourceNotFoundError();
                        return false;
                    }
                }
            }
            return true;
        }

        public Guid SourceId { get; set; }

        void InitializeProperties()
        {
            _properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", FriendlySourceName);
            AddProperty("Type :", Type);
            AddProperty("Procedure :", ActionName);
            Properties = _properties;
        }

        void AddProperty(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        void InitializeImageSource()
        {
            var actionType = ActivityTypeToActionTypeConverter.ConvertToActionType(Type);
            ImageSource = GetIconPath(actionType);
        }

        public string ResourceType
        {
            get;
            set;
        }
        public MappingManager MappingManager { get; }
        public ValidationMemoManager ValidationMemoManager { get; }

#pragma warning disable S1541 // Methods and properties should not be too complex
        string GetIconPath(Common.Interfaces.Core.DynamicServices.enActionType actionType)
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            switch (actionType)
            {
                case Common.Interfaces.Core.DynamicServices.enActionType.Workflow:
                    if (string.IsNullOrEmpty(ServiceUri))
                    {
                        ResourceType = "WorkflowService";
                        return "Workflow-32";
                    }
                    ResourceType = "Server";
                    return "RemoteWarewolf-32";

                case Common.Interfaces.Core.DynamicServices.enActionType.RemoteService:
                    ResourceType = "Server";
                    return "RemoteWarewolf-32";
                case Common.Interfaces.Core.DynamicServices.enActionType.BizRule:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeStoredProc:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeWebService:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeDynamicService:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.InvokeServiceMethod:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.Plugin:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.ComPlugin:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.Switch:
                    break;
                case Common.Interfaces.Core.DynamicServices.enActionType.Unknown:
                    break;
                default:
                    break;
            }
            return "ToolService-32";
        }

        void AddTitleBarEditToggle()
        {
            var toggle = ActivityDesignerToggle.Create("ServicePropertyEdit", "Edit", "ServicePropertyEdit", "Edit", "ShowParentToggle",
                autoReset: true,
                target: this,
                dp: ShowParentProperty
                );
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

        public void Handle(UpdateResourceMessage message)
        {
            if (message?.ResourceModel != null && SourceId != Guid.Empty && SourceId == message.ResourceModel.ID)
            {
                var sourceNotAvailableMessage = ValidationMemoManager.DesignValidationErrors.FirstOrDefault(info => info.Message == ValidationMemoManager.SourceNotFoundMessage);
                if (sourceNotAvailableMessage != null)
                {
                    ValidationMemoManager.RemoveError(sourceNotAvailableMessage);
                    ValidationMemoManager.UpdateWorstError();
                    MappingManager.InitializeMappings();
                    MappingManager.UpdateMappings();
                }
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged() => OnPropertyChanged(null);
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public override void UpdateHelpDescriptor(string helpText) => CustomContainer.Get<IShellViewModel>()?.HelpViewModel.UpdateHelpText(helpText);
    }
}