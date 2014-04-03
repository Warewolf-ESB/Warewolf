using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Activities.Utils;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Simulation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

namespace Dev2.Activities.Designers2.Service
{
    public class ServiceDesignerViewModel : ActivityDesignerViewModel, IHandle<UpdateResourceMessage>, IDisposable
    {
        const string SourceNotFoundMessage = "Source was not found. This service will not execute.";
        public static readonly ErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };

        readonly IEventAggregator _eventPublisher;

        IDesignValidationService _validationService;
        IErrorInfo _worstDesignError;
        bool _isDisposed;
        const string DoneText = "Done";
        const string FixText = "Fix";

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel)
            : this(modelItem, rootModel, EnvironmentRepository.Instance, EventPublishers.Aggregator)
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
            : base(modelItem)
        {
            AddTitleBarEditToggle();
            AddTitleBarMappingToggle();

            // PBI 6690 - 2013.07.04 - TWR : added
            // BUG 9634 - 2013.07.17 - TWR : resourceModel may be null if it is a remote resource whose environment is not connected!
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);
            _eventPublisher = eventPublisher;
            eventPublisher.Subscribe(this);
            ButtonDisplayValue = DoneText;

            ShowExampleWorkflowLink = Visibility.Collapsed;
            RootModel = rootModel;
            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new Studio.Core.ViewModels.Base.RelayCommand(o => FixErrors());
            DoneCommand = new Studio.Core.ViewModels.Base.RelayCommand(o => Done());
            DoneCompletedCommand = new Studio.Core.ViewModels.Base.RelayCommand(o => DoneCompleted());

            InitializeDisplayName();
            InitializeProperties();
            InitializeImageSource();

            IsAsyncVisible = ActivityTypeToActionTypeConverter.ConvertToActionType(Type) == DynamicServices.enActionType.Workflow;
            OutputMappingEnabled = !RunWorkflowAsync;

            var environment = environmentRepository.FindSingle(c => c.ID == EnvironmentID);
            _environment = environment;
            InitializeValidationService(environment);
            if(!InitializeResourceModel(environment))
            {
                return;
            }

            // MUST InitializeMappings() first!
            InitializeMappings();
            InitializeLastValidationMemo(environment);
            if(IsItemDragged.Instance.IsDragged)
            {
                Expand();
                IsItemDragged.Instance.IsDragged = false;
            }
        }

        void DoneCompleted()
        {
            IsFixed = true;
        }

        void Done()
        {
            if(!IsWorstErrorReadOnly)
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

        public ICommand DoneCompletedCommand { get; private set; }

        public IDataMappingViewModel DataMappingViewModel { get; private set; }

        public List<KeyValuePair<string, string>> Properties { get; private set; }

        public IContextualResourceModel ResourceModel { get; private set; }

        public IContextualResourceModel RootModel { get; private set; }

        // PBI 6690 - 2013.07.04 - TWR : added
        public DesignValidationMemo LastValidationMemo { get; private set; }

        public ObservableCollection<IErrorInfo> DesignValidationErrors { get; private set; }

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
                if(value)
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
            private set { SetValue(IsDeletedProperty, value); }
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
            if(showParent)
            {
                viewModel.DoShowParent();
            }
        }

        IErrorInfo WorstDesignError
        {
            get { return _worstDesignError; }
            set
            {
                if(_worstDesignError != value)
                {
                    _worstDesignError = value;
                    IsWorstErrorReadOnly = value == null || value.ErrorType == ErrorType.None || value.FixType == FixType.None || value.FixType == FixType.Delete;
                    WorstError = value == null ? ErrorType.None : value.ErrorType;
                }
            }
        }

        // ModelItem properties
        string ServiceUri { get { return GetProperty<string>(); } }
        string ServiceName { get { return GetProperty<string>(); } }
        string ActionName { get { return GetProperty<string>(); } }
        SimulationMode SimulationMode { get { return GetProperty<SimulationMode>(); } }
        string FriendlySourceName { get { return GetProperty<string>(); } }
        string Type { get { return GetProperty<string>(); } }
        Guid EnvironmentID { get { return GetProperty<Guid>(); } }
        Guid ResourceID { get { return GetProperty<Guid>(); } }
        Guid UniqueID { get { return GetProperty<Guid>(); } }
        string OutputMapping { set { SetProperty(value); } }
        string InputMapping { set { SetProperty(value); } }
        



        public string ButtonDisplayValue
        {
            get { return (string)GetValue(ButtonDisplayValueProperty); }
            set { SetValue(ButtonDisplayValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ButtonDisplayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ButtonDisplayValueProperty = DependencyProperty.Register("ButtonDisplayValue", typeof(string), typeof(ServiceDesignerViewModel), new PropertyMetadata(default(string)));
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        IEnvironmentModel _environment;
        bool _runWorkflowAsync;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        public override void Validate()
        {
        }

        public void UpdateMappings()
        {
            SetInputs();
            SetOuputs();
        }

        #region Overrides of ActivityDesignerViewModel

        protected override void OnToggleCheckedChanged(string propertyName, bool isChecked)
        {
            base.OnToggleCheckedChanged(propertyName, isChecked);

            // AddTitleBarMappingToggle() binds Mapping button to ShowLarge property
            if(propertyName == ShowLargeProperty.Name)
            {
                if(!isChecked)
                {
                    // Collapsing
                    UpdateMappings();
                    CheckForRequiredMapping();
                }
            }
        }

        #endregion

        void SetInputs()
        {
            if(DataMappingViewModel != null)
            {
                InputMapping = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
            }
        }

        void SetOuputs()
        {
            if(DataMappingViewModel != null)
            {
                OutputMapping = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
            }
        }

        void InitializeMappings()
        {
            var webAct = WebActivityFactory.CreateWebActivity(ModelItem, ResourceModel, ServiceName);
            DataMappingViewModel = new DataMappingViewModel(webAct, OnMappingCollectionChanged);
        }

        void OnMappingCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach(IInputOutputViewModel mapping in e.NewItems)
                {
                    mapping.PropertyChanged += OnMappingPropertyChanged;
                }
            }
            if(e.OldItems != null)
            {
                foreach(IInputOutputViewModel mapping in e.OldItems)
                {
                    mapping.PropertyChanged -= OnMappingPropertyChanged;
                }
            }
        }

        void OnMappingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
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
            if(!string.IsNullOrEmpty(serviceName))
            {
                var displayName = DisplayName;
                if(!string.IsNullOrEmpty(displayName) && displayName.Contains("Dsf"))
                {
                    DisplayName = serviceName;
                }
            }
        }

        void InitializeLastValidationMemo(IEnvironmentModel environmentModel)
        {
            var uniqueID = UniqueID;
            var designValidationMemo = new DesignValidationMemo
            {
                InstanceID = uniqueID,
                ServiceID = ResourceID,
                IsValid = RootModel.Errors.Count == 0
            };
            designValidationMemo.Errors.AddRange(RootModel.GetErrors(uniqueID).Cast<ErrorInfo>());

            if(environmentModel == null)
            {
                designValidationMemo.IsValid = false;
                designValidationMemo.Errors.Add(new ErrorInfo
                {
                    ErrorType = ErrorType.Critical,
                    FixType = FixType.None,
                    InstanceID = uniqueID,
                    Message = "Server source not found. This service will not execute."
                });
            }

            UpdateLastValidationMemo(designValidationMemo);
        }

        bool InitializeResourceModel(IEnvironmentModel environmentModel)
        {
            if(environmentModel != null)
            {
                var resourceID = ResourceID;
                if(resourceID != Guid.Empty)
                {
                    ResourceModel = environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceID, true) as IContextualResourceModel;

                }
                else
                {
                    if(!String.IsNullOrEmpty(ServiceName))
                    {
                        ResourceModel = environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == ServiceName, true) as IContextualResourceModel;
                    }
                    else
                    {
                        return true;
                    }
                }
                if(ResourceModel == null)
                {
                    if(environmentModel.IsConnected)
                    {
                        UpdateLastValidationMemoWithDeleteError();
                        return false;
                    }
                    // BUG 9634 - 2013.07.17 - TWR : added connection check
                    environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError);
                }
                else
                {
                    if(!CheckSourceMissing())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        bool CheckSourceMissing()
        {
            if(ResourceModel != null && (ResourceModel.ResourceType == ResourceType.Service && _environment != null))
            {
                var resourceModel = _environment.ResourceRepository.FindSingle(c => c.ID == ResourceModel.ID, true) as IContextualResourceModel;
                if(resourceModel != null)
                {
                    var xe = resourceModel.WorkflowXaml.Replace("&", "&amp;").ToXElement();
                    var srcID = xe.AttributeSafe("SourceID");
                    Guid sourceID;
                    if(Guid.TryParse(srcID, out sourceID))
                    {
                        SourceID = sourceID;
                        var sourceResource = _environment.ResourceRepository.FindSingle(c => c.ID == sourceID);
                        if(sourceResource == null)
                        {
                            UpdateLastValidationMemoWithSourceNotFoundError();
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public Guid SourceID { get; set; }

        void InitializeValidationService(IEnvironmentModel environmentModel)
        {
            if(environmentModel != null && environmentModel.Connection != null && environmentModel.Connection.ServerEvents != null)
            {
                _validationService = new DesignValidationService(environmentModel.Connection.ServerEvents);
                _validationService.Subscribe(UniqueID, UpdateLastValidationMemo);
            }
        }

        void InitializeProperties()
        {
            Properties = new List<KeyValuePair<string, string>>();
            AddProperty("Source :", FriendlySourceName);
            AddProperty("Type :", Type);
            AddProperty("Procedure :", ActionName);
            AddProperty("Simulation :", SimulationMode.ToString());
        }

        void AddProperty(string key, string value)
        {
            if(!string.IsNullOrEmpty(value))
            {
                Properties.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        void InitializeImageSource()
        {
            DynamicServices.enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType(Type);
            ImageSource = GetIconPath(actionType);
        }

        string GetIconPath(DynamicServices.enActionType actionType)
        {
            switch(actionType)
            {
                case DynamicServices.enActionType.InvokeStoredProc:
                    return "DatabaseService-32";

                case DynamicServices.enActionType.InvokeWebService:
                    return "WebService-32";

                case DynamicServices.enActionType.Plugin:
                    return "PluginService-32";

                case DynamicServices.enActionType.Workflow:
                    return string.IsNullOrEmpty(ServiceUri)
                        ? "Workflow-32"
                        : "RemoteWarewolf-32";

                case DynamicServices.enActionType.RemoteService:
                    return "RemoteWarewolf-32";

            }
            return "ToolService-32";
        }

        void AddTitleBarEditToggle()
        {
            // ReSharper disable RedundantArgumentName
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServicePropertyEdit-32.png",
                collapseToolTip: "Edit",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServicePropertyEdit-32.png",
                expandToolTip: "Edit",
                automationID: "ShowParentToggle",
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
            if(!IsDeleted)
            {
                _eventPublisher.Publish(new EditActivityMessage(ModelItem, EnvironmentID, null));
            }
        }

        void UpdateLastValidationMemo(DesignValidationMemo memo)
        {
            LastValidationMemo = memo;

            CheckRequiredMappingChangedErrors(memo);
            CheckIsDeleted(memo);

            UpdateDesignValidationErrors(memo.Errors.Where(info => info.InstanceID == UniqueID && info.ErrorType != ErrorType.None));
            if(SourceID == Guid.Empty)
            {
                if(CheckSourceMissing())
                {
                    InitializeMappings();
                    UpdateMappings();
                }
            }
            if(OnDesignValidationReceived != null)
            {
                OnDesignValidationReceived(this, memo);
            }
        }

        void UpdateLastValidationMemoWithDeleteError()
        {
            var uniqueID = UniqueID;
            var memo = new DesignValidationMemo
            {
                InstanceID = uniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = uniqueID,
                ErrorType = ErrorType.Warning,
                FixType = FixType.Delete,
                Message = "Resource was not found. This service will not execute."
            });
            UpdateLastValidationMemo(memo);
        }

        void UpdateLastValidationMemoWithSourceNotFoundError()
        {
            var uniqueID = UniqueID;
            var memo = new DesignValidationMemo
            {
                InstanceID = uniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = uniqueID,
                ErrorType = ErrorType.Critical,
                FixType = FixType.None,
                Message = SourceNotFoundMessage
            });
            UpdateDesignValidationErrors(memo.Errors);
        }

        void UpdateLastValidationMemoWithOfflineError(ConnectResult result)
        {
            switch(result)
            {
                case ConnectResult.Success:

                    break;

                case ConnectResult.ConnectFailed:
                case ConnectResult.LoginFailed:
                    var uniqueID = UniqueID;
                    var memo = new DesignValidationMemo
                    {
                        InstanceID = uniqueID,
                        IsValid = false,
                    };
                    memo.Errors.Add(new ErrorInfo
                    {
                        InstanceID = uniqueID,
                        ErrorType = ErrorType.Warning,
                        FixType = FixType.None,
                        Message = result == ConnectResult.ConnectFailed
                                      ? "Server is offline. This service will only execute when the server is online."
                                      : "Server login failed. This service will only execute when the login permissions issues have been resolved."
                    });
                    UpdateLastValidationMemo(memo);
                    break;
            }
        }

        void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            var keepError = false;
            var reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if(reqiredMappingChanged != null)
            {
                if(reqiredMappingChanged.FixData != null)
                {
                    var xElement = XElement.Parse(reqiredMappingChanged.FixData);
                    var inputOutputViewModels = DeserializeMappings(true, xElement);

                    foreach(var input in inputOutputViewModels)
                    {
                        IInputOutputViewModel currentInputViewModel = input;
                        if(DataMappingViewModel != null)
                        {
                            var inputOutputViewModel = DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == currentInputViewModel.Name);
                            if(inputOutputViewModel != null)
                            {
                                inputOutputViewModel.Required = input.Required;
                                if(inputOutputViewModel.MapsTo == string.Empty && inputOutputViewModel.Required)
                                {
                                    keepError = true;
                                }
                            }
                        }
                    }
                }

                if(!keepError)
                {
                    var worstErrors = memo.Errors.Where(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == UniqueID).ToList();
                    memo.Errors.RemoveAll(c => c.FixType == FixType.IsRequiredChanged && c.InstanceID == UniqueID);
                    RemoveErrors(worstErrors);
                }
            }
        }

        void CheckIsDeleted(DesignValidationMemo memo)
        {
            // BUG 9940 - 2013.07.30 - TWR - added
            var error = memo.Errors.FirstOrDefault(c => c.FixType == FixType.Delete);
            IsDeleted = error != null;
            IsEditable = !IsDeleted;
            if(IsDeleted)
            {
                while(memo.Errors.Count > 1)
                {
                    error = memo.Errors.FirstOrDefault(c => c.FixType != FixType.Delete);
                    if(error != null)
                    {
                        memo.Errors.Remove(error);
                    }
                }
            }
        }

        void CheckForRequiredMapping()
        {
            if(DataMappingViewModel != null && DataMappingViewModel.Inputs.Any(c => c.Required && String.IsNullOrEmpty(c.MapsTo)))
            {
                if(DesignValidationErrors.All(c => c.FixType != FixType.IsRequiredChanged))
                {
                    var listToRemove = DesignValidationErrors.Where(c => c.FixType == FixType.None && c.ErrorType == ErrorType.None).ToList();

                    foreach(var errorInfo in listToRemove)
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

            if(DesignValidationErrors.Any(c => c.FixType == FixType.IsRequiredChanged))
            {
                var listToRemove = DesignValidationErrors.Where(c => c.FixType == FixType.IsRequiredChanged).ToList();

                foreach(var errorInfo in listToRemove)
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

        // PBI 6690 - 2013.07.04 - TWR : added
        void FixErrors()
        {
            if(WorstDesignError.ErrorType == ErrorType.None || WorstDesignError.FixData == null)
            {
                return;
            }

            switch(WorstDesignError.FixType)
            {
                case FixType.ReloadMapping:
                    ShowLarge = true;

                    var xml = XElement.Parse(WorstDesignError.FixData);
                    var inputs = GetMapping(xml, true, DataMappingViewModel.Inputs);
                    var outputs = GetMapping(xml, false, DataMappingViewModel.Outputs);

                    DataMappingViewModel.Inputs.Clear();
                    foreach(var input in inputs)
                    {
                        DataMappingViewModel.Inputs.Add(input);
                    }

                    DataMappingViewModel.Outputs.Clear();
                    foreach(var output in outputs)
                    {
                        DataMappingViewModel.Outputs.Add(output);
                    }
                    SetInputs();
                    SetOuputs();
                    RemoveError(WorstDesignError);
                    UpdateWorstError();

                    break;

                case FixType.IsRequiredChanged:
                    ShowLarge = true;
                    var inputOutputViewModels = DeserializeMappings(true, XElement.Parse(WorstDesignError.FixData));
                    foreach(var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
                    {
                        IInputOutputViewModel model = inputOutputViewModel;
                        var actualViewModel =
                        DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == model.Name);
                        if(actualViewModel != null)
                        {
                            if(actualViewModel.Value == string.Empty)
                            {
                                actualViewModel.RequiredMissing = true;
                            }
                        }
                    }

                    break;
            }
        }

        #region GetMapping

        static IEnumerable<IInputOutputViewModel> GetMapping(XContainer xml, bool isInput, ObservableCollection<IInputOutputViewModel> oldMappings)
        {
            var result = new ObservableCollection<IInputOutputViewModel>();

            var input = xml.Descendants(isInput ? "Input" : "Output").FirstOrDefault();
            if(input != null)
            {
                var newMappings = DeserializeMappings(isInput, input);

                foreach(var newMapping in newMappings)
                {
                    IInputOutputViewModel mapping = newMapping;
                    var oldMapping = oldMappings.FirstOrDefault(m => m.Name.Equals(mapping.Name, StringComparison.InvariantCultureIgnoreCase));
                    if(oldMapping != null)
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

        static IEnumerable<IInputOutputViewModel> DeserializeMappings(bool isInput, XElement input)
        {
            var serializer = new Dev2JsonSerializer();
            var defs = serializer.Deserialize<List<Dev2Definition>>(input.Value);
            IList<IDev2Definition> idefs = new List<IDev2Definition>(defs);
            var newMappings = isInput
                ? InputOutputViewModelFactory.CreateListToDisplayInputs(idefs)
                : InputOutputViewModelFactory.CreateListToDisplayOutputs(idefs);
            return newMappings;
        }

        #endregion

        #region RemoveWorstError

        void RemoveError(IErrorInfo worstError)
        {
            DesignValidationErrors.Remove(worstError);
            RootModel.RemoveError(worstError);
        }

        void RemoveErrors(IEnumerable<ErrorInfo> worstErrors)
        {
            worstErrors.ToList().ForEach(RemoveError);
        }

        #endregion

        #region UpdateWorstError

        void UpdateWorstError()
        {
            if(DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
                if(!RootModel.HasErrors)
                {
                    RootModel.IsValid = true;
                }
            }

            IErrorInfo[] worstError = { DesignValidationErrors[0] };

            foreach(var error in DesignValidationErrors.Where(error => error.ErrorType > worstError[0].ErrorType))
            {
                worstError[0] = error;
                if(error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }
            WorstDesignError = worstError[0];
        }

        #endregion

        #region UpdateDesignValidationErrors

        void UpdateDesignValidationErrors(IEnumerable<ErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            RootModel.ClearErrors();
            foreach(var error in errors)
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
            if(message != null && message.ResourceModel != null && SourceID != Guid.Empty && SourceID == message.ResourceModel.ID)
            {
                IErrorInfo sourceNotAvailableMessage = DesignValidationErrors.FirstOrDefault(info => info.Message == SourceNotFoundMessage);
                if(sourceNotAvailableMessage != null)
                {
                    RemoveError(sourceNotAvailableMessage);
                    UpdateWorstError();
                    InitializeMappings();
                    UpdateMappings();
                }
            }
        }

        ~ServiceDesignerViewModel()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
            Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
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
            if(!_isDisposed)
            {
                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if(disposing)
                {
                    // Dispose managed resources.
                    if(_validationService != null)
                    {
                        _validationService.Dispose();
                    }
                }

                // Dispose unmanaged resources.

                _isDisposed = true;
            }
        }

        #endregion
    }
}