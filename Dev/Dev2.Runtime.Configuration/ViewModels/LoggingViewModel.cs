using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Dev2.Common.Interfaces.ComponentModel;
using Dev2.Common.Interfaces.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.ComponentModel;
using Dev2.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.Views;
using Dev2.Util.ExtensionMethods;

namespace Dev2.Runtime.Configuration.ViewModels
{
    public class LoggingViewModel : SettingsViewModelBase, IDataErrorInfo
    {

        #region private fields
        private ObservableCollection<string> _serviceInputOptions;
        private bool _logAll;
        private string _webServerUri;
        private BindableCollection<string> _workflowNames;
        private ListCollectionView _filteredWorkflowNames;
        private string _postWorkflowName;
        private string _selectedServiceInputOption;
        private bool _runPostWorkflow;
        private string _searchText = string.Empty;
        private bool _isRefreshing;

        #endregion private fields

        #region ctor

        public LoggingViewModel()
        {
            UnderlyingObjectChanged += SettingsChanged;
        }

        #endregion

        #region public properties

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                if(_isRefreshing == value)
                {
                    return;
                }

                _isRefreshing = value;

                NotifyOfPropertyChange(() => IsRefreshing);
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if(_searchText == value)
                {
                    return;
                }

                _searchText = value;

                NotifyOfPropertyChange(() => SearchText);
            }
        }

        public ListCollectionView FilteredWorkflows
        {
            get
            {
                if(_filteredWorkflowNames == null)
                {
                    _filteredWorkflowNames = new ListCollectionView(LoggingSettings.Workflows)
                        {
                            Filter = o =>
                                {
                                    var descriptor = (IWorkflowDescriptor)o;
                                    return String.IsNullOrWhiteSpace(SearchText) ||
                                           descriptor.ResourceName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                           descriptor.IsSelected;
                                }
                        };
                }
                return _filteredWorkflowNames;
            }
        }

        public bool RunPostWorkflow
        {
            get { return _runPostWorkflow; }
            set
            {
                if(_runPostWorkflow == value)
                {
                    return;
                }

                _runPostWorkflow = value;

                if(!LoggingSettings.IsInitializing)
                {
                    LoggingSettings.RunPostWorkflow = _runPostWorkflow;
                    if(!RunPostWorkflow)
                    {
                        ServiceInputOptions.Clear();
                        PostWorkflowName = string.Empty;
                    }
                }

                NotifyOfPropertyChange(() => RunPostWorkflow);
            }
        }

        public BindableCollection<string> WorkflowNames
        {
            get
            {
                return _workflowNames ?? (_workflowNames = new BindableCollection<string>());
            }
        }

        public string PostWorkflowName
        {
            get { return _postWorkflowName; }
            set
            {
                if(_postWorkflowName == value)
                {
                    return;
                }

                _postWorkflowName = value;

                if(!LoggingSettings.IsInitializing)
                {
                    var postWorkflow = LoggingSettings.Workflows.FirstOrDefault(wf => wf.ResourceName == _postWorkflowName);
                    LoggingSettings.PostWorkflow = postWorkflow;
                    UpdatePostWorkflow(postWorkflow);
                }

                NotifyOfPropertyChange(() => PostWorkflowName);
            }
        }

        public string SelectedServiceInputOption
        {
            get { return _selectedServiceInputOption; }
            set
            {
                if(_selectedServiceInputOption == value)
                {
                    return;
                }

                _selectedServiceInputOption = value;
                if(!LoggingSettings.IsInitializing)
                {
                    LoggingSettings.ServiceInput = _selectedServiceInputOption;
                }

                NotifyOfPropertyChange(() => SelectedServiceInputOption);
            }
        }

        public bool HasServiceInputOptions
        {
            get
            {
                return (RunPostWorkflow &&
                        !String.IsNullOrEmpty(PostWorkflowName) &&
                        ServiceInputOptions.Count > 0);
            }
        }

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
                _logAll = value;

                if(LoggingSettings == null)
                {
                    return;
                }

                ToggleLogAll(value);
                NotifyOfPropertyChange(() => LogAll);
            }
        }

        public ILoggingSettings LoggingSettings
        {
            get { return Object as ILoggingSettings; }
        }

        #endregion properties

        #region public methods

        public void RefreshData()
        {
            IsRefreshing = true;
            LoadWorkflows();
            IsRefreshing = false;
        }

        public void UpdateSearchFilter(string filter)
        {
            SearchText = filter;
            FilteredWorkflows.Refresh();
        }

        public void UpdatePostWorkflow(IWorkflowDescriptor postWorkflow)
        {
            if(postWorkflow == null && !string.IsNullOrWhiteSpace(PostWorkflowName))
            {
                ClearPostWorkflow(false);
            }
            else if(postWorkflow != null)
            {
                LoggingSettings.PostWorkflow =
                    LoggingSettings.Workflows
                                   .FirstOrDefault(wf =>
                                                   wf.ResourceID.Equals(LoggingSettings.PostWorkflow.ResourceID));
                PostWorkflowName = postWorkflow.ResourceName;
                LoadServiceInputs();
            }
            else if(!string.IsNullOrWhiteSpace(PostWorkflowName))
            {
                LoggingSettings.PostWorkflow = null;
                LoggingSettings.ServiceInput = string.Empty;
            }
        }

        public void LoadServiceInputs()
        {
            ServiceInputOptions.Clear();

            if(LoggingSettings.PostWorkflow == null)
            {
                LoggingSettings.ServiceInput = string.Empty;
                return;
            }

            var datalistInputs = GetDataListInputs();
            if(datalistInputs != null)
            {
                datalistInputs.ToList().ForEach(i =>
                    {
                        if(!ServiceInputOptions.Contains(i.Name))
                        {
                            ServiceInputOptions.Add(i.Name);
                        }
                    });
            }

            if(ServiceInputOptions.Contains(LoggingSettings.ServiceInput))
            {
                _selectedServiceInputOption = LoggingSettings.ServiceInput;
            }
            else
            {
                _selectedServiceInputOption = null;
                LoggingSettings.ServiceInput = string.Empty;
            }

            NotifyOfPropertyChange(() => HasServiceInputOptions);
        }

        #endregion public methods

        #region private methods

        private void Initialize()
        {
            LoggingSettings.IsInitializing = true;

            //Set privately to avoid changing underlying object
            _logAll = LoggingSettings.LogAll;
            LoadWorkflows();
            InitPostWorkflow();
            LoggingSettings.IsInitializing = false;

            NotifyOfPropertyChange("");
        }

        private void InitPostWorkflow()
        {
            _runPostWorkflow = LoggingSettings.RunPostWorkflow;
            if(!RunPostWorkflow) return;

            var postWorkflow = LoggingSettings.PostWorkflow;
            UpdatePostWorkflow(postWorkflow);
        }

        private void LoadWorkflows()
        {
            WorkflowNames.Clear();
            var resources = GetResources();
            foreach(var resource in resources)
            {
                if(LoggingSettings.Workflows.All(wf => wf.ResourceID != resource.ResourceID))
                {
                    resource.IsSelected = LogAll;
                    LoggingSettings.Workflows.Add(resource);
                }
                WorkflowNames.Add(resource.ResourceName);
            }

            //Remove deleted workflows from settings
            foreach(var descriptor in LoggingSettings.Workflows.ToList())
            {
                if(!WorkflowNames.Contains(descriptor.ResourceName))
                {
                    LoggingSettings.Workflows.Remove(descriptor);
                    if(LoggingSettings.PostWorkflow.ResourceName == descriptor.ResourceName)
                    {
                        ClearPostWorkflow();
                    }
                }
            }

            if(LoggingSettings.RunPostWorkflow && LoggingSettings.PostWorkflow != null)
            {
                if(LoggingSettings.Workflows.All(wf => wf.ResourceName != LoggingSettings.PostWorkflow.ResourceName))
                {
                    ClearPostWorkflow();
                }
            }

            UpdateSearchFilter(SearchText);
        }

        private void ClearPostWorkflow(bool clearServiceInput = true)
        {
            LoggingSettings.RunPostWorkflow = false;
            LoggingSettings.ServiceInput = string.Empty;
            PostWorkflowName = string.Empty;
            RunPostWorkflow = false;

            if(clearServiceInput)
            {
                ServiceInputOptions.Clear();
                SelectedServiceInputOption = string.Empty;
            }
        }

        private void ToggleLogAll(bool logAll)
        {
            foreach(var workflowDescriptor in LoggingSettings.Workflows)
            {
                workflowDescriptor.IsSelected = logAll;
            }

            LoggingSettings.LogAll = logAll;
        }

        private void SettingsChanged()
        {
            if(LoggingSettings == null)
            {
                return;
            }

            var loggingSettings = Object as LoggingSettings;
            if(loggingSettings != null)
            {
                _webServerUri = loggingSettings.WebServerUri + "/wwwroot/services/Service/Resources/";
                loggingSettings.PropertyChanged += LoggingSettingsPropertyChanged;
            }
            else
            {
                throw new InvalidCastException("Error casting base Object to LoggingSettings.");
            }

            Initialize();

            NotifyOfPropertyChange(() => LoggingSettings);
        }

        private void LoggingSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "Workflows": UpdateSearchFilter(SearchText);
                    break;
                case "IsInitializing":
                    IsRefreshing = LoggingSettings.IsInitializing;
                    break;
                case "LogAll": UpdateSearchFilter(SearchText);
                    break;
            }
        }

        #region Webclient calls

        private IEnumerable<WorkflowDescriptor> GetResources()
        {
            var address = String.Format(_webServerUri + "{0}", "Services");
            return CommunicationService.GetResources(address);
        }

        private IEnumerable<DataListVariable> GetDataListInputs()
        {
            var address = String.Format(_webServerUri + "{0}", "DataListInputVariables");
            return CommunicationService.GetDataListInputs(address, LoggingSettings.PostWorkflow.ResourceID);
        }

        #endregion

        #endregion private methods

        #region overrides

        public override object GetView(object context = null)
        {
            return new LoggingView();
        }

        #endregion

        #region IDataErrorInfo

        public string this[string propertyName]
        {
            get
            {
                string result = string.Empty;
                propertyName = propertyName ?? string.Empty;
                if(LoggingSettings.IsLoggingEnabled && RunPostWorkflow && propertyName == "PostWorkflowName")
                {
                    if(!WorkflowNames.Contains(PostWorkflowName))
                    {
                        result = "Invalid workflow selected";
                    }
                }

                Error = result;
                LoggingSettings.Error = result;
                return result;
            }
        }

        public string Error { get; private set; }

        #endregion
    }
}
