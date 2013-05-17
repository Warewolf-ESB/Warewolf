
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Properties;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.Views;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public class LoggingViewModel : SettingsViewModelBase
    {

        #region private fields
        private ObservableCollection<string> _serviceInputOptions;
        private ObservableCollection<ComputerDrive> _computerDrives;
        private bool _logAll;
        private ObservableCollection<string> _fileInputOptions;
        string _webServerUri;

        #endregion private fields

        #region ctor

        public LoggingViewModel()
        {
            WebClient = new WebClient();
            UnderlyingObjectChanged += SettingsChanged;
            
        }

        private void LoadDriveDirectoryStructure()
        {
            var drive = new ComputerDrive();
            _computerDrives = new ObservableCollection<ComputerDrive>();
            _computerDrives.Add(drive);
        }

        #endregion

        #region properties

        public bool HasServiceInputOptions
        {
            get
            {
                return (LoggingSettings.RunPostWorkflow &&
                    LoggingSettings.PostWorkflow != null)
                    && ServiceInputOptions.Count > 0;
            }
        }

        public ObservableCollection<string> FileInputOptions
        {
            get { return _fileInputOptions ?? (_fileInputOptions = new ObservableCollection<string>()); }
        }

        public ObservableCollection<ComputerDrive> ComputerDrives
        {
            get
            {
                if (_computerDrives == null)
                {
                    _computerDrives = new ObservableCollection<ComputerDrive>();                    
                }
                return _computerDrives;
            }
        }


        public bool IsError { get; set; }

        public string ErrorMessage { get; set; }

        public ObservableCollection<string> ServiceInputOptions
        {
            get
            {
                return _serviceInputOptions ??
                    (_serviceInputOptions = new ObservableCollection<string>());
            }
        }

        public bool LogAll
        {
            get { return _logAll; }
            set
            {
                if (_logAll == value)
                {
                    return;
                }

                _logAll = value;

                if (LoggingSettings == null)
                {
                    return;
                }

                LoggingSettings.LogAll = value;
                ToggleLogAll(value);
                NotifyOfPropertyChange(() => LogAll);
            }
        }

        public ILoggingSettings LoggingSettings
        {
            get { return Object as ILoggingSettings; }
        }

        public WebClient WebClient { get; private set; } 

        #endregion properties

        #region public methods

        public void UpdateSelection()
        {
            LoggingSettings.NotifyOfPropertyChange("Workflows");
            LoggingSettings.NotifyOfPropertyChange("PostWorkflow");
        }

        public void FileSelectionTextChanged()
        {
            
        }

        #endregion public methods

        #region overrides

        public override object GetView(object context = null)
        {
            return new LoggingView();
        }

        protected override void OnViewLoaded(object view)
        {
            LoadWorkflows();
        }

        #endregion overrides

        #region private methods
        private void ToggleLogAll(bool logAll)
        {
            foreach (var workflowDescriptor in LoggingSettings.Workflows)
            {
                workflowDescriptor.IsSelected = logAll;
            }
        }

        private void LoadWorkflows()
        {
            LoggingSettings.IsInitializing = true;

            var resources = GetResources();
            foreach (var resource in resources)
            {
                if (LoggingSettings.Workflows.All(wf => wf.ResourceID != resource.ResourceID))
                    LoggingSettings.Workflows.Add(resource);
            }

            if (LoggingSettings.RunPostWorkflow)
            {
                LoggingSettings.PostWorkflow =
                    LoggingSettings.Workflows
                                   .FirstOrDefault(wf =>
                                                   wf.ResourceID.Equals(LoggingSettings.PostWorkflow.ResourceID));
            }

            LoggingSettings.IsInitializing = false;
        }

        private IEnumerable<WorkflowDescriptor> GetResources()
        {
            var address = String.Format(_webServerUri + "{0}", "Services");
            var workflowsJSON = WebClient.UploadString(address, "WorkflowService");
            var workFlowlist = JsonConvert.DeserializeObject<IEnumerable<WorkflowDescriptor>>(workflowsJSON);

            return workFlowlist.ToList();
        }

        private void SettingsChanged()
        {
            if (LoggingSettings == null)
            {
                return;
            }
            
            var loggingSettings = this.Object as LoggingSettings;
            if (loggingSettings != null)
            {
                _webServerUri = loggingSettings.WebServerUri+"/wwwroot/services/Service/Resources/";
            }
            else
            {
                throw new InvalidCastException("Error casting base Object to LoggingSettings.");
            }
            LoadDriveDirectoryStructure();
            LoggingSettings.PropertyChanged += LoggingSettingsOnPropertyChanged;
            NotifyOfPropertyChange(() => LoggingSettings);
            LogAll = LoggingSettings.LogAll;
        }

        private void LoggingSettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "PostWorkflow":
                    LoadServiceInputs();
                    break;
                case "RunPostWorkflow":
                    if (!LoggingSettings.RunPostWorkflow)
                    {
                        LoggingSettings.ServiceInput = string.Empty;
                        LoggingSettings.PostWorkflow = null;
                    }
                    break;
            }
        }

        private void LoadServiceInputs()
        {
            ServiceInputOptions.Clear();

            if (LoggingSettings.PostWorkflow == null)
            {
                ServiceInputOptions.Clear();
                LoggingSettings.ServiceInput = string.Empty;
                return;
            }

            var datalistInputs = GetDataListInputs();
            if (datalistInputs != null)
            {
                datalistInputs.ToList().ForEach(i => ServiceInputOptions.Add(i.Name));
            }

            NotifyOfPropertyChange(() => HasServiceInputOptions);
        }

        private IEnumerable<DataListVariable> GetDataListInputs()
        {
             var address = String.Format(_webServerUri+"{0}", "DataListInputVariables");
             var datalistJSON = WebClient.UploadString(address, LoggingSettings.PostWorkflow.ResourceID);
             return JsonConvert.DeserializeObject<IEnumerable<DataListVariable>>(datalistJSON);
        }
        #endregion private methods
    }
}
