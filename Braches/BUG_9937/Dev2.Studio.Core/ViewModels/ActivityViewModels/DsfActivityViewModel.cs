using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.Providers.Errors;
using Dev2.Services;
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
        string _serviceName;
        bool _showAdorners;
        bool _showAdornersPreviousValue;
        bool _showMapping;
        bool _showMappingPreviousValue;
        ObservableCollection<KeyValuePair<string, string>> _properties;
        readonly IContextualResourceModel _contextualResourceModel;

        // PBI 6690 - 2013.07.04 - TWR : added
        readonly IDesignValidationService _validationService;
        ErrorType _worstError;
        ICommand _fixErrorsCommand;
        IContextualResourceModel _rootModel;

        #endregion Fields

        #region Ctor

        public DsfActivityViewModel(ModelItem modelItem, IContextualResourceModel resourceModel, IContextualResourceModel rootModel, IDesignValidationService validationService)
        {
            WizardEngine = new WizardEngine();
            Errors = new ObservableCollection<IErrorInfo>();

            // PBI 6690 - 2013.07.04 - TWR : added
            VerifyArgument.IsNotNull("modelItem", modelItem);
            VerifyArgument.IsNotNull("resourceModel", resourceModel);
            VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("validationService", validationService);

            var instanceIDStr = ModelItemUtils.GetProperty("UniqueID", modelItem) as string;
            Guid instanceID;
            Guid.TryParse(instanceIDStr, out instanceID);

            _modelItem = modelItem;
            _contextualResourceModel = resourceModel;
            _validationService = validationService;
            _validationService.Subscribe(instanceID, UpdateLastValidationMemo);
            _rootModel = rootModel;

            var memo = new DesignValidationMemo
            {
                InstanceID = instanceID,
                ServiceName = resourceModel.ResourceName,
                ServerID = resourceModel.ServerID,
                IsValid = rootModel.Errors.Count == 0
            };
            memo.Errors.AddRange(rootModel.GetErrors(instanceID).Cast<ErrorInfo>());
            UpdateLastValidationMemo(memo);

            SetViewModelProperties(modelItem);
        }

        #endregion Ctor

        #region Properties

        // PBI 6690 - 2013.07.04 - TWR : added
        public DesignValidationMemo LastValidationMemo { get; private set; }

        public ObservableCollection<IErrorInfo> Errors { get; private set; }

        public ErrorType WorstError
        {
            get { return _worstError; }
            set
            {
                _worstError = value;
                NotifyOfPropertyChange(() => WorstError);
            }
        }

        public IWizardEngine WizardEngine { get; set; }

        public string IconPath
        {
            get { return _iconPath; }
            set
            {
                _iconPath = value;
                NotifyOfPropertyChange(() => IconPath);
            }
        }

        public bool ShowAdorners
        {
            get { return _showAdorners; }
            set
            {
                _showAdorners = value;
                NotifyOfPropertyChange(() => ShowAdorners);
            }
        }

        public bool ShowAdornersPreviousValue
        {
            get { return _showAdornersPreviousValue; }
            set
            {
                _showAdornersPreviousValue = value;
                NotifyOfPropertyChange(() => ShowAdornersPreviousValue);
            }
        }

        public string SeriveName
        {
            get { return _serviceName; }
            set
            {
                _serviceName = value;
                NotifyOfPropertyChange(() => SeriveName);
            }
        }

        public string HelpLink
        {
            get { return _helpLink; }
            set
            {
                _helpLink = value;
                NotifyOfPropertyChange(() => HelpLink);
            }
        }

        public bool HasHelpLink
        {
            get { return _hasHelpLink; }
            set
            {
                _hasHelpLink = value;
                NotifyOfPropertyChange(() => HasHelpLink);
            }
        }

        public bool HasWizard
        {
            get { return _hasWizard; }
            set
            {
                _hasWizard = value;
                NotifyOfPropertyChange(() => HasWizard);
            }
        }

        public IDataMappingViewModel DataMappingViewModel { get; set; }

        public ObservableCollection<KeyValuePair<string, string>> Properties
        {
            get { return _properties; }
            private set
            {
                _properties = value;
                NotifyOfPropertyChange(() => Properties);
            }
        }

        public bool ShowMapping
        {
            get { return _showMapping; }
            set
            {
                _showMapping = value;
                NotifyOfPropertyChange(() => ShowMapping);
            }
        }

        public bool ShowMappingPreviousValue
        {
            get { return _showMappingPreviousValue; }
            set
            {
                _showMappingPreviousValue = value;
                NotifyOfPropertyChange(() => ShowMappingPreviousValue);
            }
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
            UpdateErrors(memo.Errors);
            if(OnDesignValidationReceived != null)
            {
                OnDesignValidationReceived(this, memo);
            }
        }

        #endregion

        #region FixErrors

        // PBI 6690 - 2013.07.04 - TWR : added
        void FixErrors()
        {
            if(_worstError == ErrorType.None)
            {
                return;
            }
            var worstError = Errors.FirstOrDefault(e => e.ErrorType == _worstError);
            if(worstError == null)
            {
                return;
            }

            switch(worstError.FixType)
            {
                case FixType.ReloadMapping:
                    ShowMapping = true;
                    var xml = XElement.Parse(worstError.FixData);
                    DataMappingViewModel.Inputs = GetMapping(xml, true, DataMappingViewModel.Inputs);
                    DataMappingViewModel.Outputs = GetMapping(xml, false, DataMappingViewModel.Outputs);
                    SetInputs();
                    SetOuputs();
                    RemoveWorstError(worstError);
                    UpdateWorstError();
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
                var serializer = new JsonSerializer();
                var defs = serializer.Deserialize<List<Dev2Definition>>(input.Value);
                IList<IDev2Definition> idefs = new List<IDev2Definition>(defs);
                var newMappings = isInput
                    ? DataMappingListFactory.CreateListToDisplayInputs(idefs)
                    : DataMappingListFactory.CreateListToDisplayOutputs(idefs);

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

        #endregion

        #endregion

        #region RemoveWorstError

        void RemoveWorstError(IErrorInfo worstError)
        {
            Errors.Remove(worstError);
            _rootModel.RemoveError(worstError);
        }

        #endregion

        #region UpdateWorstError

        void UpdateWorstError()
        {
            if(Errors.Count == 0)
            {
                Errors.Add(NoError);
            }
            WorstError = Errors.Max(e => e.ErrorType);
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
            EventAggregator.Publish(new ShowActivityWizardMessage(_modelItem));
        }

        void OpenSettings()
        {
            EventAggregator.Publish(new ShowActivitySettingsWizardMessage(_modelItem));
        }

        void OpenHelp()
        {
            if(HasHelpLink)
            {
                EventAggregator.Publish(new ShowHelpTabMessage(HelpLink));
            }
        }

        void OpenMapping()
        {
            ShowMapping = !ShowMapping;
        }

        void OpenParent()
        {
            EventAggregator.Publish(new EditActivityMessage(_modelItem));
        }

        public void SetViewModelProperties(ModelItem modelItem)
        {
            if(modelItem == null) return;

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

            SeriveName = ModelItemUtils.GetProperty("ServiceName", modelItem) as string;

            Properties = new ObservableCollection<KeyValuePair<string, string>>();

            if(_contextualResourceModel != null)
            {
                HasWizard = WizardEngine.HasWizard(modelItem, _contextualResourceModel.Environment);
                //// set Remote URI
                var modelProperty = _modelItem.Properties["ServiceUri"];
                if(modelProperty != null)
                {
                    string serviceUri = modelProperty.ComputedValue as string;
                    if(!string.IsNullOrEmpty(serviceUri))
                    {
                        IconPath = StringResources.RemoteWarewolfIconPath;
                    }
                    else
                    {
                        IconPath = GetDefaultIconPath(_contextualResourceModel);
                    }
                }
            }
            else
            {
                HasWizard = false;
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
            _validationService.Dispose();
            Errors.Clear();

            _modelItem = null;
            DataMappingViewModel = null;

            EventAggregator.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose

    }
}