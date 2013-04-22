using System;
using System.Xml.Linq;

namespace Dev2.Runtime.Configuration.ComponentModel
{
    public class WorkflowDescriptor
    {
        public Guid WorkflowID { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }

        #region WorkflowDescriptor

        public WorkflowDescriptor()
        {
        }

        public WorkflowDescriptor(XElement xml)
        {
            if(xml == null)
            {
                return;
            }

            Name = xml.AttributeSafe("Name");

            Guid workflowID;
            WorkflowID = Guid.TryParse(xml.AttributeSafe("WorkflowID"), out workflowID) ? workflowID : Guid.Empty;

            bool isSelected;
            IsSelected = bool.TryParse(xml.AttributeSafe("IsSelected"), out isSelected) && isSelected;

        }

        #endregion

        #region ToXml

        public XElement ToXml()
        {
            var result = new XElement("Workflow",
                new XAttribute("WorkflowID", WorkflowID),
                new XAttribute("Name", Name ?? string.Empty),
                new XAttribute("IsSelected", IsSelected)
                );
            return result;
        }

        #endregion
    }
}
