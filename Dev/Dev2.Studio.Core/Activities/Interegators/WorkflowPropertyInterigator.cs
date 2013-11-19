using System.Xml;
using Dev2.Studio.Core.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Activities.Interegators
{
    public static class WorkflowPropertyInterigator
    {
        // string xml
        public static void SetActivityProperties(IContextualResourceModel resource, ref DsfActivity activity)
        {

            XmlDocument document = new XmlDocument();

            if (resource.WorkflowXaml != null)
            {
                document.LoadXml(resource.WorkflowXaml);

            if(document.DocumentElement != null)
            {
                XmlNode node = document.SelectSingleNode("//HelpLink");
                if(node != null)
                {
                    activity.HelpLink = node.InnerText;
                }
            }
            }

            if(resource.Environment != null) activity.FriendlySourceName = resource.Environment.Name;
            activity.IsWorkflow = true;
            activity.Type = "Workflow";
        }
    }
}
