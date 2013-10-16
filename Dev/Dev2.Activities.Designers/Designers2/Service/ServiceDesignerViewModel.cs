using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using Caliburn.Micro;
using Dev2.Activities.Designers2.Core;
using Dev2.Communication;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Network;
using Dev2.Providers.Errors;
using Dev2.Providers.Logs;
using Dev2.Services;
using Dev2.Services.Events;
using Dev2.Simulation;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.Activities.Designers2.Service
{
    public class ServiceDesignerViewModel : ActivityDesignerViewModel, IDisposable
    {
        public static readonly IErrorInfo NoError = new ErrorInfo
        {
            ErrorType = ErrorType.None,
            Message = "Service Working Normally"
        };

        readonly IEnvironmentRepository _environmentRepository;
        readonly IEventAggregator _eventPublisher;
        IDesignValidationService _validationService;
        readonly Guid _uniqueID;


        bool _isDisposed;
        IErrorInfo _worstDesignError;

        public ServiceDesignerViewModel(ModelItem modelItem)//, IContextualResourceModel rootModel)
            : this(modelItem, null, EnvironmentRepository.Instance, EventPublishers.Aggregator)
        {
        }

        public ServiceDesignerViewModel(ModelItem modelItem, IContextualResourceModel rootModel, IEnvironmentRepository environmentRepository, IEventAggregator eventPublisher)
            : base(modelItem)//, rootModel)
        {
            AddTitleBarEditToggle();
            AddTitleBarLargeToggle();
            AddTitleBarHelpToggle();

            // PBI 6690 - 2013.07.04 - TWR : added
            // BUG 9634 - 2013.07.17 - TWR : resourceModel may be null if it is a remote resource whose environment is not connected!
            //VerifyArgument.IsNotNull("rootModel", rootModel);
            VerifyArgument.IsNotNull("environmentRepository", environmentRepository);
            VerifyArgument.IsNotNull("eventPublisher", eventPublisher);

            _environmentRepository = environmentRepository;
            _eventPublisher = eventPublisher;
            _uniqueID = ModelItemUtils.GetUniqueID(modelItem);

            DesignValidationErrors = new ObservableCollection<IErrorInfo>();
            FixErrorsCommand = new RelayCommand(o => FixErrors());

            SetProperties();
            SetImageSource();

            var environment = environmentRepository.FindSingle(c => c.ID == EnvironmentID);
            if(environment != null)
            {
                SetValidationService(environment);
                SetResourceModel(environment);
            }
        }

        public event EventHandler<DesignValidationMemo> OnDesignValidationReceived;

        public ICommand FixErrorsCommand
        {
            get;
            private set;
        }

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
            private set { SetValue(IsWorstErrorReadOnlyProperty, value); }
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
                viewModel.ShowParent = false; // reset
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
        //string IconPath { get { return GetProperty<string>(); } }
        string ServiceUri { get { return GetProperty<string>(); } }
        //string ServiceName { get { return GetProperty<string>(); } }
        //string HelpLink { get { return GetProperty<string>(); } }
        string ActionName { get { return GetProperty<string>(); } }
        SimulationMode SimulationMode { get { return GetProperty<SimulationMode>(); } }
        string FriendlySourceName { get { return GetProperty<string>(); } }
        string Type { get { return GetProperty<string>(); } }
        Guid EnvironmentID { get { return GetProperty<Guid>(); } }
        Guid ResourceID { get { return GetProperty<Guid>(); } }
        string OutputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }
        string InputMapping { get { return GetProperty<string>(); } set { SetProperty(value); } }

        public override void Validate()
        {
        }

        void SetInputs()
        {
            InputMapping = DataMappingViewModel.GetInputString(DataMappingViewModel.Inputs);
        }

        void SetOuputs()
        {
            OutputMapping = DataMappingViewModel.GetOutputString(DataMappingViewModel.Outputs);
        }

        void SetResourceModel(IEnvironmentModel environmentModel)
        {
            var resourceID = ResourceID;
            if(resourceID != Guid.Empty)
            {
                ResourceModel = environmentModel.ResourceRepository.FindSingle(c => c.ID == resourceID) as IContextualResourceModel;
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

        void SetValidationService(IEnvironmentModel environmentModel)
        {
            if(environmentModel.Connection != null && environmentModel.Connection.ServerEvents != null)
            {
                _validationService = new DesignValidationService(environmentModel.Connection.ServerEvents);
                _validationService.Subscribe(_uniqueID, UpdateLastValidationMemo);
            }
        }

        void SetProperties()
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

        void SetImageSource()
        {
            DynamicServices.enActionType actionType;
            Enum.TryParse(Type, true, out actionType);
            ImageSource = GetIconPath(actionType);
        }

        string GetIconPath(DynamicServices.enActionType actionType)
        {
            switch(actionType)
            {
                case DynamicServices.enActionType.InvokeStoredProc:
                    return "pack://application:,,,/Warewolf Studio;component/images/DatabaseService-32.png";

                case DynamicServices.enActionType.InvokeWebService:
                    return "pack://application:,,,/Warewolf Studio;component/images/WebService-32.png";

                case DynamicServices.enActionType.Plugin:
                    return "pack://application:,,,/Warewolf Studio;component/images/PluginService-32.png";

                case DynamicServices.enActionType.Workflow:
                    return string.IsNullOrEmpty(ServiceUri)
                        ? "pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png"
                        : "pack://application:,,,/Warewolf Studio;component/images/RemoteWarewolf-32.png";

                case DynamicServices.enActionType.RemoteService:
                    return "pack://application:,,,/Warewolf Studio;component/images/RemoteWarewolf-32.png";

            }
            return "pack://application:,,,/Warewolf Studio;component/images/ToolService-32.png";
        }

        void AddTitleBarEditToggle()
        {
            var toggle = ActivityDesignerToggle.Create(
                collapseImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServicePropertyEdit-32.png",
                collapseToolTip: "Edit",
                expandImageSourceUri: "pack://application:,,,/Dev2.Activities.Designers;component/Images/ServicePropertyEdit-32.png",
                expandToolTip: "Edit",
                automationID: "ShowParentToggle",
                target: this,
                dp: ShowParentProperty
                );
            TitleBarToggles.Add(toggle);
        }

        void DoShowParent()
        {
            if(!IsDeleted)
            {
                Logger.TraceInfo("Publish message of type - " + typeof(EditActivityMessage));
                _eventPublisher.Publish(new EditActivityMessage(ModelItem, EnvironmentID, null));
            }
        }

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

        void CheckRequiredMappingChangedErrors(DesignValidationMemo memo)
        {
            bool keepError = false;
            ErrorInfo reqiredMappingChanged = memo.Errors.FirstOrDefault(c => c.FixType == FixType.IsRequiredChanged);
            if(reqiredMappingChanged != null)
            {
                XElement xElement = XElement.Parse(reqiredMappingChanged.FixData);
                IEnumerable<IInputOutputViewModel> inputOutputViewModels = DeserializeMappings(true, xElement);

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

        // PBI 6690 - 2013.07.04 - TWR : added
        void FixErrors()
        {
            if(WorstDesignError.ErrorType == ErrorType.None)
            {
                return;
            }

            switch(WorstDesignError.FixType)
            {
                case FixType.ReloadMapping:
                    ShowLarge = true;
                    var xml = XElement.Parse(WorstDesignError.FixData);
                    DataMappingViewModel.Inputs = GetMapping(xml, true, DataMappingViewModel.Inputs);
                    DataMappingViewModel.Outputs = GetMapping(xml, false, DataMappingViewModel.Outputs);
                    SetInputs();
                    SetOuputs();
                    RemoveWorstError(WorstDesignError);
                    UpdateWorstError();
                    break;

                case FixType.IsRequiredChanged:
                    ShowLarge = true;
                    var inputOutputViewModels = DeserializeMappings(true, XElement.Parse(WorstDesignError.FixData));
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

        static IEnumerable<IInputOutputViewModel> DeserializeMappings(bool isInput, XElement input)
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

        #region RemoveWorstError

        void RemoveWorstError(IErrorInfo worstError)
        {
            DesignValidationErrors.Remove(worstError);
            RootModel.RemoveError(worstError);
        }

        #endregion

        #region UpdateWorstError

        void UpdateWorstError()
        {
            if(DesignValidationErrors.Count == 0)
            {
                DesignValidationErrors.Add(NoError);
                RootModel.IsValid = true;
            }

            IActionableErrorInfo[] worstError = { Errors[0] };

            foreach(var error in Errors.Where(error => error.ErrorType > worstError[0].ErrorType))
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

        #region UpdateErrors

        void UpdateErrors(IEnumerable<ErrorInfo> errors)
        {
            DesignValidationErrors.Clear();
            foreach(var error in errors)
            {
                DesignValidationErrors.Add(error);
            }
            UpdateWorstError();
        }

        #endregion

        #region Implementation of IDisposable

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