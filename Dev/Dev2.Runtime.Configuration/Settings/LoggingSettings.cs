using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Runtime.Configuration.ComponentModel;

namespace Dev2.Runtime.Configuration.Settings
{
    public class LoggingSettings : SettingsBase
    {
        #region Properties

        public bool IsLoggingEnabled { get; set; }
        public bool IsVersionLogged { get; set; }
        public bool IsTypeLogged { get; set; }
        public bool IsDurationLogged { get; set; }
        public bool IsDataAndTimeLogged { get; set; }
        public bool IsInputLogged { get; set; }
        public bool IsOutputLogged { get; set; }
        public int NestedLevelCount { get; set; }
        public string LogFileDirectory { get; set; }
        public string ServiceInput { get; set; }
        public WorkflowDescriptor PostWorkflow { get; private set; }
        public List<WorkflowDescriptor> Workflows { get; private set; }

        #endregion
        
        #region CTOR

        public LoggingSettings()
            : base("Logging")
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