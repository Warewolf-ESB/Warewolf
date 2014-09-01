using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.Interfaces.ComponentModel;
using Dev2.Common.Interfaces.Runtime.Configuration.Settings;
using Dev2.Runtime.Configuration.ComponentModel;

namespace Dev2.Runtime.Configuration.Settings
{
    public class LoggingSettings : SettingsBase, ILoggingSettings
    {
        #region Fields

        public new const string SettingName = "Logging";

        private bool _isLoggingEnabled;
        private bool _isVersionLogged;
        private bool _isTypeLogged;
        private bool _isDurationLogged;
        private bool _isDataAndTimeLogged;
        private bool _isInputLogged;
        private bool _isOutputLogged;
        private int _nestedLevelCount;
        private string _logFileDirectory;
        private ObservableCollection<IWorkflowDescriptor> _workflows;

        private bool _logAll;
        private bool _runPostWorkflow;
        private string _serviceInput;
        private IWorkflowDescriptor _postWorkflow;

        #endregion

        #region Properties

        public ObservableCollection<IWorkflowDescriptor> Workflows
        {
            get
            {
                if (_workflows == null)
                {
                    _workflows = new ObservableCollection<IWorkflowDescriptor>();
                    _workflows.CollectionChanged += WorkflowsCollectionChanged;
                }
                return _workflows;
            }
        }

        public bool LogAll
        {
            get
            {
                return _logAll;
            }
            set
            {
                if (_logAll == value)
                {
                    return;
                }

                _logAll = value;
                NotifyOfPropertyChange(() => LogAll);
            }
        }

        public bool RunPostWorkflow
        {
            get
            {
                return _runPostWorkflow;
            }
            set
            {
                if (_runPostWorkflow == value)
                {
                    return;
                }

                _runPostWorkflow = value;
                NotifyOfPropertyChange(() => RunPostWorkflow);
            }
        }
        
        public string ServiceInput
        {
            get
            {
                return _serviceInput;
            }
            set
            {
                if (_serviceInput == value)
                {
                    return;
                }

                _serviceInput = value;
                NotifyOfPropertyChange(() => ServiceInput);
            }
        }

        public IWorkflowDescriptor PostWorkflow
        {
            get
            {
                return _postWorkflow;
            }
            set
            {
                if (_postWorkflow == value)
                {
                    return;
                }

                _postWorkflow = value;
                NotifyOfPropertyChange(() => PostWorkflow);
            }
        }

        public string LogFileDirectory
        {
            get
            {
                return _logFileDirectory;
            }
            set
            {
                if (_logFileDirectory == value)
                {
                    return;
                }

                _logFileDirectory = value;
                NotifyOfPropertyChange(() => LogFileDirectory);
            }
        }

        public bool IsLoggingEnabled
        {
            get
            {
                return _isLoggingEnabled;
            }
            set
            {
                if (_isLoggingEnabled == value)
                {
                    return;
                }

                _isLoggingEnabled = value;
                NotifyOfPropertyChange(() => IsLoggingEnabled);
            }
        }

        public bool IsVersionLogged
        {
            get
            {
                return _isVersionLogged;
            }
            set
            {
                if (_isVersionLogged == value)
                {
                    return;
                }

                _isVersionLogged = value;
                NotifyOfPropertyChange(() => IsVersionLogged);
            }
        }

        public bool IsTypeLogged
        {
            get
            {
                return _isTypeLogged;
            }
            set
            {
                if (_isTypeLogged == value)
                {
                    return;
                }

                _isTypeLogged = value;
                NotifyOfPropertyChange(() => IsTypeLogged);
            }
        }

        public bool IsDurationLogged
        {
            get
            {
                return _isDurationLogged;
            }
            set
            {
                if (_isDurationLogged == value)
                {
                    return;
                }

                _isDurationLogged = value;
                NotifyOfPropertyChange(() => IsDurationLogged);
            }
        }

        public bool IsDataAndTimeLogged
        {
            get
            {
                return _isDataAndTimeLogged;
            }
            set
            {
                if (_isDataAndTimeLogged == value)
                {
                    return;
                }

                _isDataAndTimeLogged = value;
                NotifyOfPropertyChange(() => IsDataAndTimeLogged);
            }
        }

        public bool IsInputLogged
        {
            get
            {
                return _isInputLogged;
            }
            set
            {
                if (_isInputLogged == value)
                {
                    return;
                }

                _isInputLogged = value;
                NotifyOfPropertyChange(() => IsInputLogged);
            }
        }

        public bool IsOutputLogged
        {
            get
            {
                return _isOutputLogged;
            }
            set
            {
                if (_isOutputLogged == value)
                {
                    return;
                }

                _isOutputLogged = value;
                NotifyOfPropertyChange(() => IsOutputLogged);
            }
        }

        public int NestedLevelCount
        {
            get
            {
                return _nestedLevelCount;
            }
            set
            {
                if (_nestedLevelCount == value)
                {
                    return;
                }

                _nestedLevelCount = value;
                NotifyOfPropertyChange(() => NestedLevelCount);
            }
        }

        #endregion

        #region CTOR

        public LoggingSettings(string webserverUri)
            : base(SettingName, "Logging", webserverUri)
        {
        }

        public LoggingSettings(XElement xml, string webserverUri)
            : base(xml, webserverUri)
        {
            IsInitializing = true;

            var postWorkflow = xml.Element("PostWorkflow");

            bool boolValue;
            int intValue;
            IsLoggingEnabled = bool.TryParse(xml.AttributeSafe("IsLoggingEnabled"), out boolValue) && boolValue;
            IsVersionLogged = bool.TryParse(xml.AttributeSafe("IsVersionLogged"), out boolValue) && boolValue;
            IsTypeLogged = bool.TryParse(xml.AttributeSafe("IsTypeLogged"), out boolValue) && boolValue;
            IsDurationLogged = bool.TryParse(xml.AttributeSafe("IsDurationLogged"), out boolValue) && boolValue;
            IsDataAndTimeLogged = bool.TryParse(xml.AttributeSafe("IsDataAndTimeLogged"), out boolValue) && boolValue;
            IsInputLogged = bool.TryParse(xml.AttributeSafe("IsInputLogged"), out boolValue) && boolValue;
            IsOutputLogged = bool.TryParse(xml.AttributeSafe("IsOutputLogged"), out boolValue) && boolValue;
            NestedLevelCount = Int32.TryParse(xml.AttributeSafe("NestedLevelCount"), out intValue) ? intValue : 0;
            LogAll = bool.TryParse(xml.AttributeSafe("LogAll"), out boolValue) && boolValue;
            LogFileDirectory = xml.AttributeSafe("LogFileDirectory");
            ServiceInput = xml.AttributeSafe("ServiceInput");

            var workflows = xml.Element("Workflows");
            if (workflows != null)
            {
                foreach (var workflow in workflows.Elements())
                {
                    Workflows.Add(new WorkflowDescriptor(workflow));
                }
            }

            if (postWorkflow != null)
            {
                PostWorkflow = new WorkflowDescriptor(xml.Element("PostWorkflow"));
            }

            RunPostWorkflow = (PostWorkflow != null);

            IsInitializing = false;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Fired when the workflows collection is changed. Used to maintain IsSelected/Dirty state
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/17</date>
        /// <exception cref="System.NotImplementedException"></exception>
        private void WorkflowsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count > 0)
            {
                foreach (var workflow in e.NewItems.Cast<WorkflowDescriptor>())
                {
                    workflow.PropertyChanged += WorkflowPropertyChanged;
                }
            }

            if (e.OldItems != null && e.OldItems.Count > 0)
            {
                foreach (var workflow in e.OldItems.Cast<WorkflowDescriptor>())
                {
                    workflow.PropertyChanged -= WorkflowPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Escalates the selection changed event so that dirty state can be maintained
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <author>Jurie.smit</author>
        /// <date>2013/06/17</date>
        /// <exception cref="System.NotImplementedException"></exception>
        private void WorkflowPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => Workflows);
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            XElement postWorkflow = null;
            if (PostWorkflow != null)
            {
                postWorkflow = PostWorkflow.ToXml();
                postWorkflow.Name = "PostWorkflow";
            }

            var workflows = new XElement("Workflows");

            var toPersist = from wf in Workflows
                            where wf.IsSelected
                            select wf;

            foreach (var workflow in toPersist)
            {
                workflows.Add(workflow.ToXml());
            }

            var result = base.ToXml();
            result.Add(
                new XAttribute("IsLoggingEnabled", IsLoggingEnabled),
                new XAttribute("IsVersionLogged", IsVersionLogged),
                new XAttribute("IsTypeLogged", IsTypeLogged),
                new XAttribute("IsDurationLogged", IsDurationLogged),
                new XAttribute("IsDataAndTimeLogged", IsDataAndTimeLogged),
                new XAttribute("IsInputLogged", IsInputLogged),
                new XAttribute("IsOutputLogged", IsOutputLogged),
                new XAttribute("NestedLevelCount", NestedLevelCount),
                new XAttribute("LogAll", LogAll),
                new XAttribute("LogFileDirectory", LogFileDirectory ?? string.Empty),
                new XAttribute("ServiceInput", ServiceInput ?? string.Empty),
                postWorkflow,
                workflows
                );
            return result;
        }

        #endregion
    }
}