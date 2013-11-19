using System.Xml;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkflowPropertyInterigator
    {
        // string xml
        public static DsfActivity SetActivityProperties(IContextualResourceModel resource, DsfActivity activity)
        {

            XmlDocument document = new XmlDocument();
            document.LoadXml(resource.WorkflowXaml);

            if(document.DocumentElement != null)
            {
                XmlNode node = document.SelectSingleNode("//HelpLink");
                if(node != null)
                {
                    activity.HelpLink = node.InnerText;
                }
            }
            activity.Type = "Workflow";
            return activity;
        }
    }
}
