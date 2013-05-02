using Dev2.Runtime.Configuration.ComponentModel;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.Settings
{
    public class LoggingSettings : SettingsBase
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
        private string _serviceInput;
        private WorkflowDescriptor _postWorkflow;
        private List<WorkflowDescriptor> _workflows;

        #endregion

        #region Properties

        public bool IsLoggingEnabled
        {
            get
            {
                return _isLoggingEnabled;
            }
            set
            {
                _isLoggingEnabled = value;
                OnPropertyChanged("IsLoggingEnabled");
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
                _isVersionLogged = value;
                OnPropertyChanged("IsVersionLogged");
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
                _isTypeLogged = value;
                OnPropertyChanged("IsTypeLogged");
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
                _isDurationLogged = value;
                OnPropertyChanged("IsDurationLogged");
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
                _isDataAndTimeLogged = value;
                OnPropertyChanged("IsDataAndTimeLogged");
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
                _isInputLogged = value;
                OnPropertyChanged("IsInputLogged");
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
                _isOutputLogged = value;
                OnPropertyChanged("IsOutputLogged");
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
                _nestedLevelCount = value;
                OnPropertyChanged("NestedLevelCount");
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
                _logFileDirectory = value;
                OnPropertyChanged("LogFileDirectory");
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
                _serviceInput = value;
                OnPropertyChanged("ServiceInput");
            }
        }

        public WorkflowDescriptor PostWorkflow
        {
            get
            {
                return _postWorkflow;
            }
            set
            {
                _postWorkflow = value;
                OnPropertyChanged("PostWorkflow");
            }
        }

        public List<WorkflowDescriptor> Workflows
        {
            get
            {
                return _workflows;
            }
            private set
            {
                _workflows = value;
                OnPropertyChanged("Workflows");
            }
        }

        #endregion

        #region CTOR

        public LoggingSettings()
            : base(SettingName, "Logging")
        {
            PostWorkflow = new WorkflowDescriptor();
            Workflows = new List<WorkflowDescriptor>();
        }

        public LoggingSettings(XElement xml)
            : base(xml)
        {
            Workflows = new List<WorkflowDescriptor>();
            PostWorkflow = new WorkflowDescriptor(xml.Element("PostWorkflow"));

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
            LogFileDirectory = xml.AttributeSafe("LogFileDirectory");
            ServiceInput = xml.AttributeSafe("ServiceInput");

            var workflows = xml.Element("Workflows");
            if(workflows != null)
            {
                foreach(var workflow in workflows.Elements())
                {
                    Workflows.Add(new WorkflowDescriptor(workflow));
                }
            }
        }

        #endregion

        #region ToXml

        public override XElement ToXml()
        {
            var postWorkflow = PostWorkflow.ToXml();
            postWorkflow.Name = "PostWorkflow";

            var workflows = new XElement("Workflows");
            foreach(var workflow in Workflows)
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