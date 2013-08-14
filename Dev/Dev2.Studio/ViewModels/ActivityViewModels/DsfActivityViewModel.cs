using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Studio.Core.Activities.TO;
using Dev2.Studio.Core.Activities.Translators;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public class DsfActivityViewModel : SimpleBaseViewModel
    {
        public static readonly IErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };

        #region Fields

        readonly IEnvironmentRepository _environmentRepository;
        readonly IEventAggregator _eventPublisher;
        ICommand _fixErrorsCommand;
        bool _hasHelpLink;
        bool _hasWizard;
        string _helpLink;
        string _iconPath;
        ModelItem _modelItem;
        ICommand _openHelpCommand;
        //string _getMappingKeyToDereg;
        ICommand _openMappingCommand;
        ICommand _openParentCommand;
        ICommand _openSettingsCommand;
        ICommand _openWizardCommand;
        ObservableCollection<KeyValuePair<string, string>> _properties;
        string _serviceName;
        bool _showAdorners;
        bool _showAdornersPreviousValue;
        bool _showMapping;
        bool _showMappingPreviousValue;

        // PBI 6690 - 2013.07.04 - TWR : added
        readonly IDesignValidationService _validationService;
        IErrorInfo _worstError;
        readonly Guid _uniqueID;
        bool _isEditable;

        #endregion Fields

        #region Ctor

        public DsfActivityViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository)
            : this(modelItem, rootModel, environmentRepository, EventPublishers.Aggregator)
        {
        }

        public DsfActivityViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
        {
            WizardEngine = new WizardEngine();
            Errors = new ObservableCollection<IErrorInfo>();

            // PBI 6690 - 2013.07.04 - TWR : added
            // BUG 9634 - 2013.07.17 - TWR : resourceModel may be null if it is a remote resource whose environment is not connected!
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);

            _modelItem = modelItem;
            _environmentRepository = environmentRepository;
            _eventPublisher = eventPublisher;
            _uniqueID = ModelItemUtils.GetUniqueID(modelItem);
            RootModel = rootModel;
            SeriveName = ModelItemUtils.GetProperty("ServiceName", modelItem) as string;

            var environmentID = Guid.Empty;
            var envID = ModelItemUtils.GetProperty("EnvironmentID", modelItem) as InArgument<Guid>;
            if(envID != null)
            {
                Guid.TryParse(envID.Expression.ToString(), out environmentID);
            }

            var designValidationMemo = new DesignValidationMemo
            {
                InstanceID = _uniqueID,
                ServiceName = SeriveName,
                IsValid = rootModel.Errors.Count == 0
            };
            designValidationMemo.Errors.AddRange(rootModel.GetErrors(_uniqueID).Cast<ErrorInfo>());

            var environmentModel = _environmentRepository.FindSingle(c => c.ID == environmentID);

            // BUG 9634 - 2013.07.17 - TWR : if resourceModel is not null then validationService cannot be null
            if(environmentModel != null)
            {
                if(environmentModel.Connection != null)
                {
                    if(environmentModel.Connection.ServerEvents != null)
                    {
                        _validationService = new DesignValidationService(environmentModel.Connection.ServerEvents);
                        _validationService.Subscribe(_uniqueID, UpdateLastValidationMemo);
                    }

                    if(!string.IsNullOrEmpty(SeriveName))
                    {
                        ResourceModel = environmentModel.ResourceRepository.FindSingle(c => c.ResourceName == SeriveName) as IContextualResourceModel;
                        if(ResourceModel == null)
                        {
                            if(environmentModel.IsConnected)
                            {
                                UpdateLastValidationMemoWithDeleteError();
                                return;
                            }

                            // BUG 9634 - 2013.07.17 - TWR : added connection check
                            environmentModel.Connection.Verify(UpdateLastValidationMemoWithOfflineError);
                        }
                    }
                }
            }
            else
            {
                designValidationMemo.IsValid = false;
                designValidationMemo.Errors.Add(new ErrorInfo
                {
                    ErrorType = ErrorType.Critical,
                    FixType = FixType.None,
                    InstanceID = _uniqueID,
                    Message = "Server source not found. This service will not execute."
                });
            }

            var webAct = WebActivityFactory.CreateWebActivity(modelItem, ResourceModel, SeriveName);
            DataMappingViewModel = new DataMappingViewModel(webAct);

            UpdateLastValidationMemo(designValidationMemo);

            SetViewModelProperties(modelItem);
        }

        #endregion Ctor

        #region Properties

        public bool IsEditable
        {
            get { return _isEditable; }
            private set
            {
                if(value.Equals(_isEditable))
                {
                    return;
                }
                _isEditable = value;
                NotifyOfPropertyChange(() => IsEditable);
            }
        }

        public bool IsDeleted { get; private set; }

        public IContextualResourceModel RootModel { get; private set; }

        public IContextualResourceModel ResourceModel { get; private set; }

        // PBI 6690 - 2013.07.04 - TWR : added
        public DesignValidationMemo LastValidationMemo { get; private set; }

        public ObservableCollection<IErrorInfo> Errors { get; private set; }

        public bool IsWorstErrorReadOnly
        {
            get
            {
                return _worstError.ErrorType == ErrorType.None || _worstError.FixType == FixType.None || _worstError.FixType == FixType.Delete;
            }
        }

        public ErrorType WorstError
        {
            get
            {
                return _worstError.ErrorType;
            }
        }

        public IWizardEngine WizardEngine { get; set; }

        public string IconPath
        {
            get
            {
                return _iconPath;
            }
            set
            {
                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        public bool ShowAdorners
        {
            get
            {
                return _showAdorners;
            }
            set
            {
                _showAdorners = value;
                NotifyOfPropertyChange(() => ShowAdorners);
            }
        }

        public bool ShowAdornersPreviousValue
        {
            get
            {
                return _showAdornersPreviousValue;
            }
            set
            {
                _showAdornersPreviousValue = value;
                NotifyOfPropertyChange(() => ShowAdornersPreviousValue);
            }
        }

        public string SeriveName
        {
            get
            {
                return _serviceName;
            }
            set
            {
                _serviceName = value;
                NotifyOfPropertyChange(() => SeriveName);
            }
        }

        public string HelpLink
        {
            get
            {
                return _helpLink;
            }
            set
            {
                _helpLink = value;
                NotifyOfPropertyChange(() => HelpLink);
            }
        }

        public bool HasHelpLink
        {
            get
            {
                return _hasHelpLink;
            }
            set
            {
                _hasHelpLink = value;
                NotifyOfPropertyChange(() => HasHelpLink);
            }
        }

        public bool HasWizard
        {
            get
            {
                return _hasWizard;
            }
            set
            {
                _hasWizard = value;
                NotifyOfPropertyChange(() => HasWizard);
            }
        }

        public IDataMappingViewModel DataMappingViewModel { get; set; }

        public ObservableCollection<KeyValuePair<string, string>> Properties
        {
            get
            {
                return _properties;
            }
            private set
            {
                _properties = value;
                NotifyOfPropertyChange(() => Properties);
            }
        }

        public bool ShowMapping
        {
            get
            {
                return _showMapping;
            }
            set
            {
                SetShowMapping(value);
            }
        }

        public bool ShowMappingPreviousValue
        {
            get
            {
                return _showMappingPreviousValue;
            }
            set
            {
                _showMappingPreviousValue = value;
                NotifyOfPropertyChange(() => ShowMappingPreviousValue);
            }
        }

        void SetShowMapping(bool value)
        {
            _showMapping = value;
            if(!value)
            {
                CheckForRequiredMapping();
            }
            NotifyOfPropertyChange(() => ShowMapping);
        }

        void CheckForRequiredMapping()
        {
            if(DataMappingViewModel != null && DataMappingViewModel.Inputs.Any(c => c.Required && String.IsNullOrEmpty(c.MapsTo)))
            {
                if(Errors.All(c => c.FixType != FixType.IsRequiredChanged))
                {
                    List<IErrorInfo> listToRemove = Errors.Where(c => c.FixType == FixType.None).ToList();

                    foreach(IErrorInfo errorInfo in listToRemove)
                    {
                        Errors.Remove(errorInfo);
                    }

                    ErrorInfo mappingIsRequiredMessage = CreateMappingIsRequiredMessage();
                    Errors.Add(mappingIsRequiredMessage);
                    RootModel.AddError(mappingIsRequiredMessage);
                    //_rootModel.IsValid = false;
                }
                UpdateWorstError();
                return;
            }

            if(Errors.Any(c => c.FixType == FixType.IsRequiredChanged))
            {
                List<IErrorInfo> listToRemove = Errors.Where(c => c.FixType == FixType.IsRequiredChanged).ToList();

                foreach(IErrorInfo errorInfo in listToRemove)
                {
                    Errors.Remove(errorInfo);
                    RootModel.RemoveError(errorInfo);
                }
                //                if(Errors.Count == 0)
                //                {
                //                    _rootModel.IsValid = true;
                //                }
                UpdateWorstError();
            }
        }

        ErrorInfo CreateMappingIsRequiredMessage()
        {
            return new ErrorInfo { ErrorType = ErrorType.Critical, FixData = CreateFixedData(), FixType = FixType.IsRequiredChanged, InstanceID = _uniqueID };
        }

        string CreateFixedData()
        {
            JsonSerializer serializer = new JsonSerializer();
            string result = serializer.Serialize(DataMappingListFactory.CreateListInputMapping(DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs)));
            return string.Concat("<Input>", result, "</Input>");
        }

        #endregion Properties

        #region Commands

        // PBI 6690 - 2013.07.04 - TWR : added
        public ICommand FixErrorsCommand
        {
            get
            {
                return _fixErrorsCommand ??
                       (_fixErrorsCommand = new RelayCommand(param => FixErrors()));
            }
        }

        public ICommand OpenWizardCommand
        {
            get
            {
                return _openWizardCommand ??
                       (_openWizardCommand = new RelayCommand(param => OpenWizard()));
            }
        }

        public ICommand OpenSettingsCommand
        {
            get
            {
                return _openSettingsCommand ??
                       (_openSettingsCommand = new RelayCommand(param => OpenSettings()));
            }
        }

        public ICommand OpenHelpCommand
        {
            get
            {
                return _openHelpCommand ?? (_openHelpCommand =
                    new RelayCommand(param => OpenHelp()));
            }
        }

        public ICommand OpenMappingCommand
        {
            get
            {
                return _openMappingCommand ??
                       (_openMappingCommand = new RelayCommand(param => OpenMapping()));
            }
        }

        public ICommand OpenParentCommand
        {
            get
            {
                return _openParentCommand ??
                       (_openParentCommand = new RelayCommand(param => OpenParent()));
            }
        }

        #endregion Commands

        #region Methods

        #region SetLastValidationMemo

        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        void UpdateLastValidationMemo(DesignValidationMemo memo)
        {
            LastValidationMemo = memo;

            CheckRequiredMappingChangedErrors(memo);
            CheckIsDeleted(memo);

            UpdateErrors(memo.Errors);

            if(OnDesignValidationReceived != null)
            {
                OnDesignValidationReceived(this, memo);
            }
        }

        void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            bool keepError = false;
            ErrorInfo reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if(reqiredMappingChanged != null)
            {
                XElement xElement = XElement.Parse(reqiredMappingChanged.FixData);
                IList<IInputOutputViewModel> inputOutputViewModels = DeserializeMappings(true, xElement);

                foreach(var input in inputOutputViewModels)
                {
                    IInputOutputViewModel inputOutputViewModel = DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == input.Name);
                    if(inputOutputViewModel != null)
                    {
                        inputOutputViewModel.Required = input.Required;
                        if(inputOutputViewModel.MapsTo == string.Empty && inputOutputViewModel.Required)
                        {
                            keepError = true;
                        }
                    }
                }

                if(!keepError)
                {
                    memo.Errors.Remove(reqiredMappingChanged);
                    RemoveWorstError(reqiredMappingChanged);
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

        void UpdateLastValidationMemoWithDeleteError()
        {
            var memo = new DesignValidationMemo
            {
                InstanceID = _uniqueID,
                IsValid = false,
            };
            memo.Errors.Add(new ErrorInfo
            {
                InstanceID = _uniqueID,
                ErrorType = ErrorType.Warning,
                FixType = FixType.Delete,
                Message = "Resource was not found. This service will not execute."
            });
            UpdateLastValidationMemo(memo);
        }

        void UpdateLastValidationMemoWithOfflineError(ConnectResult result)
        {
            switch(result)
            {
                case ConnectResult.Success:

                    break;

                case ConnectResult.ConnectFailed:
                case ConnectResult.LoginFailed:
                    var memo = new DesignValidationMemo
                    {
                        InstanceID = _uniqueID,
                        IsValid = false,
                    };
                    memo.Errors.Add(new ErrorInfo
                    {
                        InstanceID = _uniqueID,
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

        #endregion

        #region FixErrors

        // PBI 6690 - 2013.07.04 - TWR : added
        void FixErrors()
        {
            if(_worstError.ErrorType == ErrorType.None)
            {
                return;
            }

            switch(_worstError.FixType)
            {
                case FixType.ReloadMapping:
                    ShowMapping = true;
                    var xml = XElement.Parse(_worstError.FixData);
                    DataMappingViewModel.Inputs = GetMapping(xml, true, DataMappingViewModel.Inputs);
                    DataMappingViewModel.Outputs = GetMapping(xml, false, DataMappingViewModel.Outputs);
                    SetInputs();
                    SetOuputs();
                    RemoveWorstError(_worstError);
                    UpdateWorstError();
                    break;

                case FixType.IsRequiredChanged:
                    ShowMapping = true;
                    var inputOutputViewModels = DeserializeMappings(true, XElement.Parse(_worstError.FixData));
                    foreach(var inputOutputViewModel in inputOutputViewModels.Where(c => c.Required))
                    {
                        var actualViewModel = DataMappingViewModel.Inputs.FirstOrDefault(c => c.Name == inputOutputViewModel.Name);
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

        static ObservableCollection<IInputOutputViewModel> GetMapping(XElement xml, bool isInput, ObservableCollection<IInputOutputViewModel> oldMappings)
        {
            var result = new ObservableCollection<IInputOutputViewModel>();

            var input = xml.Descendants(isInput ? "Input" : "Output").FirstOrDefault();
            if(input != null)
            {
                var newMappings = DeserializeMappings(isInput, input);

                foreach(var newMapping in newMappings)
                {
                    var oldMapping = oldMappings.FirstOrDefault(m => m.Name.Equals(newMapping.Name, StringComparison.InvariantCultureIgnoreCase));
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

        static IList<IInputOutputViewModel> DeserializeMappings(bool isInput, XElement input)
        {
            var serializer = new JsonSerializer();
            var defs = serializer.Deserialize<List<Dev2Definition>>(input.Value);
            IList<IDev2Definition> idefs = new List<IDev2Definition>(defs);
            var newMappings = isInput
                ? InputOutputViewModelFactory.CreateListToDisplayInputs(idefs)
                : InputOutputViewModelFactory.CreateListToDisplayOutputs(idefs);
            return newMappings;
        }

        #endregion

        #endregion

        #region RemoveWorstError

        void RemoveWorstError(IErrorInfo worstError)
        {
            Errors.Remove(worstError);
            RootModel.RemoveError(worstError);
        }

        #endregion

        #region UpdateWorstError

        void UpdateWorstError()
        {
            if(Errors.Count == 0)
            {
                Errors.Add(NoError);
                RootModel.IsValid = true;
            }

            _worstError = Errors[0];

            foreach(var error in Errors.Where(error => error.ErrorType > _worstError.ErrorType))
            {
                _worstError = error;
                if(error.ErrorType == ErrorType.Critical)
                {
                    break;
                }
            }

            NotifyOfPropertyChange(() => WorstError);
            NotifyOfPropertyChange(() => IsWorstErrorReadOnly);
        }

        #endregion

        #region UpdateErrors

        void UpdateErrors(IEnumerable<ErrorInfo> errors)
        {
            Errors.Clear();
            foreach(var error in errors)
            {
                Errors.Add(error);
            }
            UpdateWorstError();
        }

        #endregion

        void OpenWizard()
        {
            _eventPublisher.Publish(new ShowActivityWizardMessage(_modelItem));
        }

        void OpenSettings()
        {
            _eventPublisher.Publish(new ShowActivitySettingsWizardMessage(_modelItem));
        }

        void OpenHelp()
        {
            if(HasHelpLink)
            {
                _eventPublisher.Publish(new ShowHelpTabMessage(HelpLink));
            }
        }

        void OpenMapping()
        {
            ShowMapping = !ShowMapping;
        }

        void OpenParent()
        {
            if(!IsDeleted)
            {
                _eventPublisher.Publish(new EditActivityMessage(_modelItem, RootModel.Environment.ID, null));
            }
        }

        void SetViewModelProperties(ModelItem modelItem)
        {
            var iconArg = ModelItemUtils.GetProperty("IconPath", modelItem) as InArgument<string>;
            if(iconArg != null)
            {
                IconPath = iconArg.Expression.ToString();
            }

            var argument = ModelItemUtils.GetProperty("HelpLink", modelItem) as InArgument;
            if(argument != null)
            {
                HelpLink = argument.Expression.ToString();
                if(!string.IsNullOrWhiteSpace(HelpLink))
                {
                    HasHelpLink = true;
                }
            }

            Properties = new ObservableCollection<KeyValuePair<string, string>>();

            // BUG 9634 - 2013.07.17 - TWR : Remote URI does not depend on a non-null _contextualResourceModel
            var serviceUri = ModelItemUtils.GetProperty("ServiceUri", modelItem) as string;
            if(!string.IsNullOrEmpty(serviceUri))
            {
                IconPath = StringResources.RemoteWarewolfIconPath;
            }

            if(ResourceModel != null)
            {
                HasWizard = WizardEngine.HasWizard(modelItem, ResourceModel.Environment);
            }
            else
            {
                HasWizard = false;
            }
            if(string.IsNullOrEmpty(IconPath))
            {
                IconPath = GetDefaultIconPath(ResourceModel);
            }

            var translator = new ServiceXmlTranslator();
            ActivityViewModelTO transObject = translator.GetActivityViewModelTO(modelItem);

            if(transObject != null && Properties != null)
            {
                if(!string.IsNullOrWhiteSpace(transObject.SourceName))
                {
                    Properties.Add(new KeyValuePair<string, string>("Source :", transObject.SourceName));
                }
                if(!string.IsNullOrWhiteSpace(transObject.Type))
                {
                    Properties.Add(new KeyValuePair<string, string>("Type :", transObject.Type));
                }
                if(!string.IsNullOrWhiteSpace(transObject.Action))
                {
                    Properties.Add(new KeyValuePair<string, string>("Procedure :", transObject.Action));
                }
                if(!string.IsNullOrWhiteSpace(transObject.Simulation))
                {
                    Properties.Add(new KeyValuePair<string, string>("Simulation :", transObject.Simulation));
                }
            }
        }

        public void SetInputs()
        {
            string inputString = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
            ModelItemUtils.SetProperty("InputMapping", inputString, _modelItem);
        }

        public void SetOuputs()
        {
            string outputString = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
            ModelItemUtils.SetProperty("OutputMapping", outputString, _modelItem);
        }

        string GetDefaultIconPath(IContextualResourceModel resource)
        {
            if(resource != null)
            {
                if(resource.ResourceType == ResourceType.WorkflowService)
                {
                    return "pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png";
                }
                if(resource.ResourceType == ResourceType.Service)
                {
                    return "pack://application:,,,/Warewolf Studio;component/images/ToolService-32.png";
                }
                if(resource.ResourceType == ResourceType.Source)
                {
                    return "pack://application:,,,/Warewolf Studio;component/images/ExplorerSources-32.png";
                }
            }
            return string.Empty;
        }

        #endregion

        #region Dispose

        protected override void OnDispose()
        {
            if(Properties != null)
            {
                Properties.Clear();
            }

            // PBI 6690 - 2013.07.04 - TWR : added
            if(_validationService != null)
            {
                _validationService.Dispose();
            }
            Errors.Clear();

            _modelItem = null;
            DataMappingViewModel = null;

            base.OnDispose();
        }

        #endregion Dispose
    }
}