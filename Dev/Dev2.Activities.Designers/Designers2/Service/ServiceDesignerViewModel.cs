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
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Common.Interfaces.Infrastructure.Providers.Errors;
using Dev2.Common.Interfaces.Security;
using Dev2.Common.Interfaces.Threading;
using Dev2.Common.Utils;
using Dev2.Communication;
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
        public bool _resourcesUpdated;
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
            MappingManager = new MappingManager(this);
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
                MappingManager.InitializeMappings();
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
                MappingManager.UpdateLastValidationMemo(memo);
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


        public ICommand FixErrorsCommand { get; private set; }

        public ICommand DoneCommand { get; private set; }

        private RelayCommand ViewComplexObjectsCommand { get; set; }

        public ICommand DoneCompletedCommand { get; private set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }

        public IContextualResourceModel ResourceModel { get; set; }

        public IContextualResourceModel RootModel { get; set; }

        public DesignValidationMemo LastValidationMemo { get; set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; set; }

        
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

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            if (propertyName == ShowLargeProperty.Name)
            {
                if (!isChecked)
                {
                    MappingManager.UpdateMappings();
                    MappingManager.CheckForRequiredMapping();
                }
            }
        }

        public static readonly DependencyProperty IsWorstErrorReadOnlyProperty =
            DependencyProperty.Register("IsWorstErrorReadOnly", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsDeleted
        {
            get { return (bool)GetValue(IsDeletedProperty); }
            set { if (!(bool)GetValue(IsDeletedProperty)) SetValue(IsDeletedProperty, value); }
        }

        public static readonly DependencyProperty IsDeletedProperty =
            DependencyProperty.Register("IsDeleted", typeof(bool), typeof(ServiceDesignerViewModel), new PropertyMetadata(false));

        public bool IsEditable
        {
            get { return (bool)GetValue(IsEditableProperty); }
            set { SetValue(IsEditableProperty, value); }
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

        string ServiceUri => GetProperty<string>();
        public string ServiceName => GetProperty<string>();
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
        public Guid UniqueID => GetProperty<Guid>();
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

            MappingManager.UpdateLastValidationMemo(designValidationMemo);
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
            _worker.Start(() => GetResourceModel(e.Model), () => MappingManager.CheckVersions(this));
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

        public IContextualResourceModel NewModel { get; set; }

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

        public void UpdateLastValidationMemoWithVersionChanged()
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
            MappingManager.UpdateLastValidationMemo(memo, false);
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
                catch(XmlException xe)
                {
                    Dev2Logger.Error(xe);
                    srcId = workflowXml.ExtractXmlAttributeFromUnsafeXml("SourceID=\"");
                }

                Guid sourceId;
                if(Guid.TryParse(srcId, out sourceId))
                {
                    SourceId = sourceId;
                    var sourceResource = _environment.ResourceRepository.LoadContextualResourceModel(sourceId);
                    if(sourceResource == null)
                    {
                        UpdateLastValidationMemoWithSourceNotFoundError();
                        return false;
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
                _validationService.Subscribe(UniqueID, a => MappingManager.UpdateLastValidationMemo(a));
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
        public MappingManager MappingManager { get; }
        public bool VersionsDifferent
        {
            set
            {
                _versionsDifferent = value;
            }
            get
            {
                return _versionsDifferent;
            }
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
                        MappingManager.UpdateLastValidationMemo(memo);
                        break;
                }
            });
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
                        var xml = MappingManager.FetchXElementFromFixData();
                        var inputs = MappingManager.GetMapping(xml, true, MappingManager.DataMappingViewModel.Inputs);
                        var outputs = MappingManager.GetMapping(xml, false, MappingManager.DataMappingViewModel.Outputs);

                        MappingManager.DataMappingViewModel.Inputs.Clear();
                        foreach (var input in inputs)
                        {
                            MappingManager.DataMappingViewModel.Inputs.Add(input);
                        }

                        MappingManager.DataMappingViewModel.Outputs.Clear();
                        foreach (var output in outputs)
                        {
                            MappingManager.DataMappingViewModel.Outputs.Add(output);
                        }
                        MappingManager.SetInputs();
                        MappingManager.SetOuputs();
                        RemoveError(WorstDesignError);
                        UpdateWorstError();
                    }
                    else if (_versionsDifferent)
                    {
                        ResourceModel = NewModel;
                        MappingManager.InitializeMappings();
                        RemoveErrors(
                      LastValidationMemo.Errors.Where(a => a.Message.Contains("Incorrect Version")).ToList());
                        UpdateWorstError();
                    }
                    break;

                case FixType.IsRequiredChanged:
                    ShowLarge = true;
                    var inputOutputViewModels = MappingManager.DeserializeMappings(true, MappingManager.FetchXElementFromFixData());
                    foreach (var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
                    {
                        IInputOutputViewModel model = inputOutputViewModel;
                        var actualViewModel = MappingManager.DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == model.Name);
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

        void RemoveError(IErrorInfo worstError)
        {
            DesignValidationErrors.Remove(worstError);
            RootModel.RemoveError(worstError);
        }

        // ReSharper disable ParameterTypeCanBeEnumerable.Local
        public void RemoveErrors(IList<IErrorInfo> worstErrors)
        // ReSharper restore ParameterTypeCanBeEnumerable.Local
        {
            worstErrors.ToList().ForEach(RemoveError);
        }

        public void UpdateWorstError()
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

        public void UpdateDesignValidationErrors(IEnumerable<IErrorInfo> errors)
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
                        MappingManager.InitializeMappings();
                        MappingManager.UpdateMappings();
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





