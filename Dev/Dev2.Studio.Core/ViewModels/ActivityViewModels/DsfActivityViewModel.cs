using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Input;
using Dev2.Studio.Core.Activities.TO;
using Dev2.Studio.Core.Activities.Translators;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Core.ErrorHandling;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;
using Dev2.Studio.Core.ViewModels.Base;
using Dev2.Studio.Core.Wizards;
using Dev2.Studio.Core.Wizards.Interfaces;
using Dev2.DynamicServices;

namespace Dev2.Studio.Core.ViewModels.ActivityViewModels
{
    public class DsfActivityViewModel : SimpleBaseViewModel
    {
        #region Fields

        private bool _hasHelpLink;
        private bool _hasWizard;
        private string _helpLink;
        private string _iconPath;
        private ModelItem _modelItem;
        private ICommand _openHelpCommand;
        //private string _getMappingKeyToDereg;
        private ICommand _openMappingCommand;
        private ICommand _openParentCommand;
        private ICommand _openSettingsCommand;
        private ICommand _openWizardCommand;
        private ObservableCollection<KeyValuePair<string, string>> _propertyCollection;
        private string _serviceName;
        private bool _showAdorners;
        private bool _showAdornersPreviousValue;
        private bool _showMapping;
        private bool _showMappingPreviousValue;
        private enErrorType _worstError;
        private IContextualResourceModel _contextualResourceModel;

        #endregion Fields

        #region Ctor

        public DsfActivityViewModel(ModelItem modelItem, IContextualResourceModel contextualResourceModel)
        {
            WizardEngine = new WizardEngine();
            _modelItem = modelItem;
            _contextualResourceModel = contextualResourceModel;
            
            SetViewModelProperties(modelItem);
        }

        #endregion Ctor

        #region Properties

        
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

        public ObservableCollection<KeyValuePair<string, string>> PropertyCollection
        {
            get { return _propertyCollection; }
            set
            {
                _propertyCollection = value;
                NotifyOfPropertyChange(() => PropertyCollection);
            }
        }

        public ObservableCollection<Dev2ErrorObject> ErrorCollection { get; set; }

        public enErrorType WorstError
        {
            get
            {
                _worstError = enErrorType.Correct;
                if (ErrorCollection.FirstOrDefault(c => c.ErrorType == enErrorType.Critical) != null)
                {
                    _worstError = enErrorType.Critical;
                }
                else if (ErrorCollection.FirstOrDefault(c => c.ErrorType == enErrorType.Warning) != null)
                {
                    _worstError = enErrorType.Warning;
                }

                return _worstError;
            }
            set { _worstError = value; }
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

        private void OpenWizard()
        {
            EventAggregator.Publish(new ShowActivityWizardMessage(_modelItem));
        }

        private void OpenSettings()
        {
            EventAggregator.Publish(new ShowActivitySettingsWizardMessage(_modelItem));
        }

        private void OpenHelp()
        {
            if (HasHelpLink)
            {
                EventAggregator.Publish(new ShowHelpTabMessage(HelpLink));
            }
        }

        private void OpenMapping()
        {
            ShowMapping = !ShowMapping;
        }

        private void OpenParent()
        {
            EventAggregator.Publish(new EditActivityMessage(_modelItem));
        }

        public void SetViewModelProperties(ModelItem modelItem)
        {
            if (modelItem == null) return;

            IconPath = ModelItemUtils.GetProperty("IconPath", modelItem) as string;
            var argument = ModelItemUtils.GetProperty("HelpLink", modelItem) as InArgument;
            if (argument != null)
            {
                HelpLink = argument.Expression.ToString();
                if (!string.IsNullOrWhiteSpace(HelpLink))
                {
                    HasHelpLink = true;
                }
            }

            SeriveName = ModelItemUtils.GetProperty("ServiceName", modelItem) as string;
            PropertyCollection = new ObservableCollection<KeyValuePair<string, string>>();
            ErrorCollection = new ObservableCollection<Dev2ErrorObject>
            {
                new Dev2ErrorObject
                    {
                        ErrorType = enErrorType.Correct,
                        UserErrorMessage = "Service Working Normaly"
                    }
            };

            //Test Dummy data until the error handeling is implemented
            //ErrorCollection.Add(new Dev2ErrorObject() { ErrorType = enErrorType.Warning, UserErrorMessage = "Mapping Out Of Date" });
            //ErrorCollection.Add(new Dev2ErrorObject() { ErrorType = enErrorType.Critical, UserErrorMessage = "Source Not Available" });            

            if (_contextualResourceModel != null)
            {
                HasWizard = WizardEngine.HasWizard(modelItem, _contextualResourceModel.Environment);
                // set Remote URI
                var modelProperty = _modelItem.Properties["ServiceUri"];
                var modelPropertyID = _modelItem.Properties["ServiceServer"];
                if (modelProperty != null)
                {
                    var uri = _contextualResourceModel.Environment.Connection.WebServerUri.AbsoluteUri;
                    // check for local hostm if not set ServiceURI, else we do not care ;)
                    if (!ModelItemUtils.IsLocalService(uri))
                    {
                        modelProperty.SetValue(uri);
                        if (modelPropertyID != null)
                        {
                            // set the server id for debug info later ;)
                            modelPropertyID.SetValue(_contextualResourceModel.Environment.ID);
                        }

                        // Set the icon to the remote warewolf icon ;)
                        IconPath = StringResources.RemoteWarewolfIconPath;
                    }
                }
            }
            else
            {
                HasWizard = false;
            }  

            var translator = new ServiceXmlTranslator();
            ActivityViewModelTO transObject = translator.GetActivityViewModelTO(modelItem);

            if (transObject != null && PropertyCollection != null)
            {
                if (!string.IsNullOrWhiteSpace(transObject.SourceName))
                {
                    PropertyCollection.Add(new KeyValuePair<string, string>("Source :", transObject.SourceName));
                }
                if (!string.IsNullOrWhiteSpace(transObject.Type))
                {
                    PropertyCollection.Add(new KeyValuePair<string, string>("Type :", transObject.Type));    
                }
                if (!string.IsNullOrWhiteSpace(transObject.Action))
                {
                    PropertyCollection.Add(new KeyValuePair<string, string>("Procedure :", transObject.Action));
                }
                if (!string.IsNullOrWhiteSpace(transObject.Simulation))
                {
                    PropertyCollection.Add(new KeyValuePair<string, string>("Simulation :", transObject.Simulation));
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

        #endregion Methods

        #region Dispose

        protected override void OnDispose()
        {
            if (PropertyCollection != null)
            {
                PropertyCollection.Clear();
            }


            if (ErrorCollection != null)
            {
                ErrorCollection.Clear();
            }

            _modelItem = null;
            DataMappingViewModel = null;

            EventAggregator.Unsubscribe(this);
            base.OnDispose();
        }

        #endregion Dispose

    }
}